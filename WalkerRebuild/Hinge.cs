//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        class Hinge : Joint
        {
            public IMyMotorStator Stator;

            public Hinge(IMyMotorStator stator, JointData data) : base(stator, data)
            {
                Stator = stator;
            }
            public Hinge(IMyMotorStator stator) : base(stator)
            {
                Stator = stator;
            }
            public override void SetForce(bool max)
            {
                base.SetForce(max);
                Connection.SetValue("Torque", CurrentForce);
            }
            public override float TorqueMax()
            {
                return Connection.GetMaximum<float>("Torque");
            }
            public override Vector3 ReturnRotationAxis()
            {
                return LargeGrid ? Stator.WorldMatrix.Down : Stator.WorldMatrix.Up;
            }
            public override double CurrentPosition()
            {
                return Stator.Angle * RAD2DEG;
            }
            public override float ClampTargetValue(float target)
            {
                target = target < -90 ? -90 : target;
                target = target > 90 ? 90 : target;
                return target;
            }
            public override void LerpAnimationFrame(float lerpTime)
            {
                AnimTarget = LerpPoints[0] + ((LerpPoints[1] - LerpPoints[0]) * lerpTime);
            }
            public override void UpdateCorrectionDisplacement()
            {
                CorrectionMag = ActiveTarget - (Stator.Angle * RAD2DEG);
                CorrectionDir = Math.Sign(CorrectionMag);
                CorrectionMag = Math.Abs(CorrectionMag);
            }
            public override void UpdatePlaneDisplacement()
            {
                base.UpdatePlaneDisplacement();

                ActiveTarget = ActiveTarget % 360;
                ActiveTarget = ActiveTarget > 180 ? ActiveTarget - 360 : ActiveTarget;
                ActiveTarget = ActiveTarget > 90 ? 90 : ActiveTarget;
            }
        }
    }
}
