//using Sandbox.ModAPI;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {

        class Sequence : Animation
        {
            /// EXTERNALS ///
            public JointSet JointSet;
            public List<Root> Frames = new List<Root>();
            public KeyFrame[] CurrentFrames = new KeyFrame[2];

            // Logic
            public ClockMode RisidualClockMode = ClockMode.FOR;
            public ClockMode CurrentClockMode = ClockMode.PAUSE;
            public float CurrentClockTime = 0;
            public bool StepDelay;

            public Sequence(AnimationData data, JointSet set = null) : base(data)
            {
                TAG = SeqTag;
                JointSet = set;
                GenerateSetting(ClockSpeedDef);
            }
            public Sequence(string input, JointSet set, List<KeyFrame> buffer) : base(input)
            {
                JointSet = set;
                Frames.AddRange(buffer);
                BUILT = Load(input);
            }
            public KeyFrame GetKeyFrame(int index)
            {
                if (index < 0 || index >= Frames.Count)
                    return null;
                return (KeyFrame)Frames[index];
            }

            protected override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try { GenerateSetting(float.Parse(data[(int)PARAM.SettingInit])); }
                catch { GenerateSetting(0); }

                return true;
            }
            public void OverrideSet()
            {
                foreach (Joint joint in JointSet.Joints)
                    joint.OverrideIndex = MyIndex;
            }
            public override void Insert(Root root, int index = -1)
            {
                if (root is KeyFrame)
                    insert(Frames, root, index);
            }
            public override void Sort(eRoot root = eRoot.DEFAULT)
            {
                Frames.Sort(SORT);
            }
            public override void GenerateSetting(float init)
            {
                MySetting = new Setting("Clock Speed", "Speed at which the sequence will interpolate between frames", init, ClockIncrmentMag, ClockSpeedCap, ClockSpeedMin, SequenceSpeedIncrement);
            }

            public void ZeroSequence()
            {
                RisidualClockMode = CurrentClockMode;
                LoadKeyFrames(false, 0);
                CurrentClockMode = ClockMode.PAUSE;
                CurrentClockTime = 0;
            }
            public void SetClockMode(ClockMode mode)
            {
                RisidualClockMode = mode == ClockMode.PAUSE ? CurrentClockMode : mode;
                CurrentClockMode = mode;
            }
            public void ToggleClockPause()
            {
                CurrentClockMode = CurrentClockMode == ClockMode.PAUSE ? RisidualClockMode : ClockMode.PAUSE;
            }
            public void ToggleClockDirection()
            {
                RisidualClockMode = RisidualClockMode == ClockMode.FOR ? ClockMode.REV : ClockMode.FOR;
                CurrentClockMode = CurrentClockMode == ClockMode.PAUSE ? CurrentClockMode : RisidualClockMode;
            }

            public bool DemoKeyFrame(int index)
            {
                if (index < 0 ||
                    index >= Frames.Count)
                    return false;

                foreach (JointFrame jFrame in GetKeyFrame(index).Jframes)
                    jFrame.Joint.OverwriteAnimTarget(jFrame.MySetting.MyValue());

                return true;
            }
            public bool UpdateSequence(bool anim)
            {
                if (CurrentFrames == null ||
                    CurrentClockMode == ClockMode.PAUSE)
                    return false;

                //StreamDlog("Update Sequence...");
                if (anim)
                    UpdateSequenceClock();
                else
                    UpdateTriggers();

                LerpFrame(CurrentClockTime);
                return true;
            }
            public bool AddKeyFrameSnapshot(int index, string name = null, bool snapping = false)
            {
                if (JointSet == null ||
                    JointSet.Joints.Count == 0)
                    return false;

                if (name == null)
                    name = $"Frame_{index}";

                KeyFrame newKFrame = NewKeyFrame(new AnimationData(ParentData(name, index), FrameLengthDef), JointSet);

                Insert(newKFrame, index);
                return true;
            }
            public bool RemoveKeyFrameAtIndex(int index)
            {
                if (index < 0 ||
                    index >= Frames.Count)
                    return false;

                Frames.RemoveAt(index);
                return true;
            }

            void UpdateTriggers()
            {
                if (WithinTargetThreshold)
                    UpdateSequenceClock();

                if (CheckFrameTimer())
                    LoadKeyFrames(false);

                UpdateStepDelay();

                if (!IgnoreFeet.MyState() && !StepDelay && JointSet.CheckStep())
                {

                    StepDelay = true;
                    JointSet.IncrementStepping(CurrentClockMode);
                    LoadKeyFrames(true);
                }
            }
            void UpdateSequenceClock()
            {
                CurrentClockTime += MySetting.MyValue() * (1 / CurrentFrames[0].MySetting.MyValue()) * (int)CurrentClockMode;
                CurrentClockTime = CurrentClockTime < 0 ? 0 : CurrentClockTime > 1 ? 1 : CurrentClockTime;
            }
            void UpdateStepDelay()
            {
                if (CurrentClockMode == ClockMode.PAUSE ||
                    !StepDelay)
                    return;

                float triggerTime = CurrentClockMode != ClockMode.REV ? CurrentClockTime : 1 - CurrentClockTime;

                if (triggerTime >= StepThreshold.MyValue())
                    StepDelay = false;
            }
            void LerpFrame(float lerpTime)
            {
                foreach (Joint joint in JointSet.Joints)
                {
                    if (!joint.IsAlive() || joint.OverrideIndex != MyIndex)
                        continue;

                    joint.LerpAnimationFrame(lerpTime);
                }
            }

            bool CheckFrameTimer()
            {
                if (!WithinTargetThreshold)
                    return false;
                if (CurrentClockMode == ClockMode.FOR && CurrentClockTime == 1)
                    return true;
                if (CurrentClockMode == ClockMode.REV && CurrentClockTime == 0)
                    return true;
                return false;
            }
            public bool LoadKeyFrames(bool footInterrupt, int index = -1)
            {
                bool forward = CurrentClockMode != ClockMode.REV;
                CurrentClockTime = forward ? 0 : 1;

                int indexZero = CurrentFrames[0] == null || index != -1 ?
                    forward ? index : NextFrameIndex(index) :
                    NextFrameIndex(CurrentFrames[0].MyIndex);

                int indexOne = CurrentFrames[1] == null || index != -1 ?
                    forward ? NextFrameIndex(index) : index :
                    NextFrameIndex(CurrentFrames[1].MyIndex);

                CurrentFrames[0] = GetKeyFrame(indexZero);
                CurrentFrames[1] = GetKeyFrame(indexOne);

                return LoadJointFrames(forward, footInterrupt);
            }
            bool LoadJointFrames(bool forward = true, bool interrupt = false)
            {
                if (JointSet == null)
                    return false;

                JointFrame zero, one;
                Joint joint;

                for (int i = 0; i < JointSet.Joints.Count; i++)
                {
                    joint = JointSet.GetJoint(i);
                    if (joint is Piston || joint.OverrideIndex != MyIndex || !joint.IsAlive())
                        continue;

                    zero = CurrentFrames[0] == null ? null : CurrentFrames[0].GetJointFrameByFrameIndex(i);
                    one = CurrentFrames[1] == null ? null : CurrentFrames[1].GetJointFrameByFrameIndex(i);
                    JointSet.GetJoint(i).LoadJointFrames(zero, one, forward, interrupt);
                }
                return true;
            }

            int NextFrameIndex(int current)
            {
                int next = current + (CurrentClockMode != ClockMode.REV ? 1 : -1);
                next = next < 0 ? Frames.Count - 1 : next >= Frames.Count ? 0 : next;
                return next;
            }
        }

    }
}
