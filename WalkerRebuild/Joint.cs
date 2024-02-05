//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {

        class Joint : Functional
        {
            public int FootIndex;
            public int SyncIndex;
            public int OverrideIndex = -1;
            public int GripDirection;
            public bool LargeGrid;
            public IMyMechanicalConnectionBlock Connection;

            // Instance
            public double[] LerpPoints = new double[2];
            public bool Planeing = false;
            public bool Gripping = false;
            public bool TargetThreshold = false;

            // Ze Maths
            public int CorrectionDir;

            public float CurrentForce;
            public double PlaneCorrection;
            public double AnimTarget;
            public double ActiveTarget;
            public double CorrectionMag;
            public double TargetVelocity;
            public double OldVelocity;
            public double LiteralVelocity; // Not used atm? but it works! : D
            public double LastPosition;

            public Vector3 PlanarDots;
            public JointSet Parent => GetJointSet(ParentIndex);

            public Joint(IMyMechanicalConnectionBlock mechBlock, JointData data) : base(mechBlock, data.Root)
            {
                StaticDlog("Joint Constructor:");
                LargeGrid = mechBlock.BlockDefinition.ToString().Contains("Large");
                Connection = mechBlock;
                Connection.Enabled = true;
                FootIndex = data.FootIndex;
                SyncIndex = data.SyncIndex;
                GripDirection = data.GripDirection;
                TAG = data.TAG;
                //SetForce(true);
            }

            public Joint(IMyMechanicalConnectionBlock mechBlock) : base(mechBlock)
            {
                LargeGrid = mechBlock.BlockDefinition.ToString().Contains("Large");
                Connection = mechBlock;
                Connection.Enabled = true;
                BUILT = Load(mechBlock == null ? null : mechBlock.CustomData);
            }
            public bool IsAlive()
            {
                try { return Connection.IsWorking; }
                catch { return false; }
            }
            public override List<int> Indexes()
            {
                List<int> indexes = base.Indexes();
                indexes.Add(FootIndex);
                indexes.Add(GripDirection);
                indexes.Add(SyncIndex);

                return indexes;
            }
            public virtual void Sync()
            {

            }
            public virtual void SetForce(bool max)
            {
                if (Connection == null)
                    return;

                CurrentForce = TorqueMax();
                CurrentForce = max ? CurrentForce : CurrentForce * ForceMin;
            }
            protected override bool Load(string[] data)//, Option option = null)
            {
                if (!base.Load(data))
                    return false;

                try
                {
                    FootIndex = int.Parse(data[(int)PARAM.fIX]);
                    GripDirection = int.Parse(data[(int)PARAM.GripDirection]);
                    SyncIndex = int.Parse(data[(int)PARAM.sIX]);
                }
                catch
                {
                    FootIndex = -1;
                    GripDirection = 0;
                    SyncIndex = -1;
                }
                return true;
            }
            protected override void saveData(string[] buffer)
            {
                base.saveData(buffer);
                buffer[(int)PARAM.fIX] = FootIndex.ToString();
                buffer[(int)PARAM.sIX] = SyncIndex.ToString();
                buffer[(int)PARAM.GripDirection] = GripDirection.ToString();
            }
            public void LoadJointFrames(JointFrame zero, JointFrame one, bool forward, bool interrupt)
            {
                LerpPoints[0] = interrupt && forward ? CurrentPosition() : zero == null ? 0 : zero.MySetting.MyValue();
                LerpPoints[1] = interrupt && !forward ? CurrentPosition() : one == null ? 0 : one.MySetting.MyValue();
            }
            public void OverwriteAnimTarget(double value)
            {
                AnimTarget = value;
            }
            public void UpdateJoint(bool activeTargetTracking)
            {
                UpdateLiteralVelocity();
                if (!activeTargetTracking)
                {
                    UpdateStatorVelocity(activeTargetTracking);
                    return;
                }

                ActiveTarget = AnimTarget;

                UpdateCorrectionDisplacement();

                if (!(this is Piston) && Planeing)
                {
                    UpdatePlaneDisplacement();
                    UpdateCorrectionDisplacement();
                }

                UpdateStatorVelocity(activeTargetTracking);
                TargetThreshold = DisThreshold();
            }
            void UpdateLiteralVelocity()
            {
                double currentPosition = CurrentPosition();
                LiteralVelocity = ((currentPosition - LastPosition) / 360) / GetGridTimeSinceLastRun().TotalMinutes;
                LastPosition = currentPosition;
            }
            void UpdateStatorVelocity(bool active)
            {
                if (active)
                {
                    OldVelocity = TargetVelocity;
                    if (TAG == "G")
                    {
                        TargetVelocity = MaxSpeed.MyValue() * GripDirection * (Gripping ? -1 : 1); // Needs changing!
                    }
                    else
                    {
                        TargetVelocity = CorrectionDir * DEG2VEL * (CorrectionMag);
                        TargetVelocity = Math.Abs(TargetVelocity - OldVelocity) > MaxAcceleration.MyValue() ? OldVelocity + (MaxAcceleration.MyValue() * Math.Sign(TargetVelocity - OldVelocity)) : TargetVelocity;
                        TargetVelocity = Math.Abs(TargetVelocity) > MaxSpeed.MyValue() ? MaxSpeed.MyValue() * CorrectionDir : TargetVelocity;
                        TargetVelocity = Math.Abs(TargetVelocity) > MIN_VEL ? TargetVelocity : 0;
                    }
                }
                else
                    TargetVelocity = 0;

                UpdateConnection();
            }
            public bool DisThreshold()
            {
                return CorrectionMag < FrameThreshold.MyValue();
            }
            public void UpdatePlanarDot(MatrixD plane)
            {
                PlanarDots.X = Vector3.Dot(ReturnRotationAxis(), plane.Right);
                PlanarDots.Y = Vector3.Dot(ReturnRotationAxis(), plane.Up);
                PlanarDots.Z = Vector3.Dot(ReturnRotationAxis(), plane.Backward);
            }
            public virtual double CurrentPosition()
            {
                return -100;
            }
            public float LimitMin()
            {
                return Connection.GetValueFloat("LowerLimit");
            }
            public float LimitMax()
            {
                return Connection.GetValueFloat("UpperLimit");
            }
            public virtual float TorqueMax()
            {
                return 0;
            }
            public virtual Vector3 ReturnRotationAxis()
            {
                return Vector3.Zero;
            }
            public virtual float ClampTargetValue(float target)
            {
                return 0;
            }
            public virtual void LerpAnimationFrame(float lerpTime)
            {
            }
            public virtual void UpdatePlaneDisplacement()
            {
                if (!Planeing)
                    return;

                PlaneCorrection -= (CorrectionMag * CorrectionDir);
                ActiveTarget += PlaneCorrection;
            }
            public virtual void UpdateCorrectionDisplacement()
            {

            }
            public void UpdateConnection()
            {
                Connection.SetValueFloat("Velocity", (float)TargetVelocity);
            }
        }

    }
}
