//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using System;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {

        class Rotor : Joint
        {
            public IMyMotorStator Stator;

            public Rotor(IMyMotorStator stator, JointData data) : base(stator, data)
            {
                Stator = stator;
            }

            public Rotor(IMyMotorStator stator) : base(stator)
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
                return LargeGrid ? Stator.WorldMatrix.Up : Stator.WorldMatrix.Down;
            }
            public override double CurrentPosition()
            {
                return Stator.Angle * RAD2DEG;
            }
            public override float ClampTargetValue(float target)
            {
                target %= 360;
                target = target < 0 ? target + 360 : target;
                return target;
            }
            public override void LerpAnimationFrame(float lerpTime)
            {
                double mag = Math.Abs(LerpPoints[0] - LerpPoints[1]);
                int dir = (mag > 180) ? Math.Sign(LerpPoints[0] - LerpPoints[1]) : Math.Sign(LerpPoints[1] - LerpPoints[0]);

                mag = mag > 180 ? 360 - mag : mag;
                mag *= (lerpTime * dir);


                AnimTarget = LerpPoints[0] + mag;
                AnimTarget = (AnimTarget > 360) ? AnimTarget - 360 : AnimTarget;

                AnimTarget = (AnimTarget < 0) ? AnimTarget + 360 : AnimTarget;
            }
            public override void UpdateCorrectionDisplacement()
            {
                double current = (Stator.Angle * RAD2DEG);

                double delta = Math.Abs(ActiveTarget - current);
                CorrectionDir = (delta > 180) ? Math.Sign(current - ActiveTarget) : Math.Sign(ActiveTarget - current);
                CorrectionMag = (delta > 180) ? 360 - delta : delta;
            }
            public override void UpdatePlaneDisplacement()
            {
                base.UpdatePlaneDisplacement();

                ActiveTarget = ActiveTarget % 360;
                ActiveTarget = ActiveTarget < 0 ? ActiveTarget + 360 : ActiveTarget;
            }
        }

    }
}
