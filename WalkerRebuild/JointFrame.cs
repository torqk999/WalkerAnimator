//using Sandbox.ModAPI;

using EmptyKeys.UserInterface.Generated.DataTemplatesContracts_Bindings;

namespace IngameScript
{
    partial class Program
    {

        class JointFrame : Animation
        {
            public Joint Joint;

            public JointFrame(AnimationData data, Joint joint, bool snapping = true) : base(data) // Snapshot
            {
                TAG = JframeTag;
                Joint = joint;
                GenerateSetting((float)Joint.CurrentPosition());

                if (snapping)
                    MySetting.Change((int)MySetting.MyValue());
                
            }
            public JointFrame(string input, Joint joint) : base(input)
            {
                Joint = joint;
                BUILT = Load(input);
            }
            public override void GenerateSetting(float init)
            {
                Static($"jFrame {Name()} GeneratingSetting...\n");
                MySetting = new Setting("Joint Position", "The animation value of the joint associated joint within a given keyFrame.",
                    init, Snapping ? 1 : 0.1f,
                    (Joint == null ? 0 : Joint.LimitMax()),
                    (Joint == null ? 0 : Joint.LimitMin()),
                    SnappingIncrement);
            }
            public void ChangeStatorLerpPoint(float value)
            {
                MySetting.Change(Joint.ClampTargetValue(value));
            }
        }

    }
}
