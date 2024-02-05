using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {

        class Piston : Joint
        {
            public IMyPistonBase PistonBase;
            public Joint Reference;

            public Piston(IMyPistonBase pistonBase, JointData data) : base(pistonBase, data)
            {
                PistonBase = pistonBase;
            }
            public Piston(IMyPistonBase pistonBase) : base(pistonBase)
            {
                PistonBase = pistonBase;
            }
            public override void Sync()
            {
                Reference = Parent.GetJoint(SyncIndex);
            }

            public override void SetForce(bool max)
            {
                base.SetForce(max);
                PistonBase.SetValue("MaxImpulseAxis", CurrentForce);
            }
            public override float TorqueMax()
            {
                return PistonBase.GetMaximum<float>("MaxImpulseAxis");
            }
            public override double CurrentPosition()
            {
                StreamDlog($"Reference Exists: {Reference != null}\n" +
                    $"SyncIndex: {SyncIndex}");

                if (Reference != null)
                {
                    StreamDlog("Piston has Reference!");
                    double currentPosition = Reference.CurrentPosition();
                    StreamDlog($"CurrentPosition of reference: {currentPosition}");
                    return currentPosition;
                }

                return base.CurrentPosition();
            }
            public override float ClampTargetValue(float target)
            {
                if (Reference != null)
                {
                    if (Reference is Hinge)
                        return ((Hinge)Reference).ClampTargetValue(target);
                    if (Reference is Rotor)
                        return ((Rotor)Reference).ClampTargetValue(target);
                }

                return base.ClampTargetValue(target);
            }

            public override Vector3 ReturnRotationAxis()
            {
                return -PistonBase.WorldMatrix.Forward;
            }
            public override void UpdateCorrectionDisplacement()
            {
                if (Reference != null)
                {
                    Vector3 cross = Vector3.Cross(Reference.ReturnRotationAxis(), ReturnRotationAxis());
                    Vector3 b_pos = PistonBase.GetPosition();
                    Vector3 m_pos = ((PistonBase.CurrentPosition / 2) * PistonBase.WorldMatrix.Forward) + b_pos;
                    Vector3 displace = Reference.Connection.GetPosition() - m_pos;
                    float dot = Vector3.Dot(cross, displace);
                    CorrectionMag = Reference.CorrectionMag;
                    CorrectionDir = Reference.CorrectionDir * Math.Sign(dot);
                    return;
                }

                base.UpdateCorrectionDisplacement();
            }
        }


    }
}
