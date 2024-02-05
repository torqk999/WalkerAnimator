//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {


        class JointSet : Root
        {
            string _name;

            public IMyTerminalBlock Plane;
            public List<Root> Feet = new List<Root>();
            public List<Root> Joints = new List<Root>();
            public List<Root> Sequences = new List<Root>();

            public KeyFrame ZeroFrame = null;

            public MatrixD TargetPlane;
            public MatrixD TurnPlane;
            public MatrixD BufferPlane;
            public Vector3D PlaneBuffer;
            public Vector3D TurnBuffer;

            public bool Locked;
            public bool StepInterrupt;
            public int LockedIndex;
            int ReleaseTimer = 0;

            public JointSet(RootData root, IMyTerminalBlock plane, string groupName) : base(root)
            {
                TAG = JointSetTag;
                Plane = plane;
                _name = groupName;
                GenerateZeroFrame();
            }

            public JointSet(string input, IMyTerminalBlock plane, List<Foot> buffer) : base()
            {
                Plane = plane;
                Feet.AddRange(buffer);
                BUILT = Load(input);
            }

            public override string Name()
            {
                return _name;
            }

            public override void SetName(string newName)
            {
                _name = newName;
            }

            public void GenerateZeroFrame()
            {
                ZeroFrame = NewKeyFrame(new AnimationData(ParentData("Zero Frame"), FrameLengthDef), this);
            }

            public Foot GetFoot(int index)
            {
                if (index < 0 || index >= Feet.Count)
                    return null;
                return (Foot)Feet[index];
            }
            public Joint GetJoint(int index)
            {
                if (index < 0 || index >= Joints.Count)
                    return null;
                return (Joint)Joints[index];
            }
            public Sequence GetSequence(int index)
            {
                if (index < 0 || index >= Sequences.Count)
                    return null;
                return (Sequence)Sequences[index];
            }

            public override void Insert(Root root, int index = -1)
            {
                if (root is Joint)
                    insert(Joints, root, index);

                if (root is Sequence)
                    insert(Sequences, root, index);
            }

            public override void Remove(int index, eRoot type)
            {
                switch(type)
                {
                    case eRoot.JOINT:
                        remove(Joints, index);
                        break;

                    case eRoot.SEQUENCE:
                        remove(Sequences, index);
                        break;
                }
            }

            public override void Swap(int target, int delta, eRoot type)
            {
                switch (type)
                {
                    case eRoot.JOINT:
                        swap(Joints, target, target + delta);
                        break;

                    case eRoot.SEQUENCE:
                        swap(Sequences, target, target + delta);
                        break;
                }
            }

            public override void ReIndex(eRoot type)
            {
                switch(type)
                {
                    case eRoot.JOINT:
                        reIndex(Joints);
                        break;

                    case eRoot.SEQUENCE:
                        reIndex(Sequences);
                        break;
                }

            }
            public override void Sort(eRoot type)
            {
                switch(type)
                {
                    case eRoot.JOINT:
                        Joints.Sort(SORT);
                        break;

                    case eRoot.SEQUENCE:
                        Sequences.Sort(SORT);
                        break;
                }
            }

            public bool UpdateJoints()
            {
                bool withinThreshold = true;

                foreach (Joint joint in Joints)
                {
                    if (!joint.IsAlive())
                        continue;

                    joint.UpdateJoint(StatorTarget.MyState());
                    if (withinThreshold)
                        withinThreshold = joint.TargetThreshold;
                }

                return withinThreshold;
            }
            public bool UpdateFootLockStatus()
            {
                ReleaseTimer -= 1;
                ReleaseTimer = ReleaseTimer < 0 ? 0 : ReleaseTimer;

                if (StepInterrupt)
                    return false;

                bool oldState = Locked;
                Foot locked = GetFoot(LockedIndex);
                Locked = locked != null && locked.CheckLocked();


                if (!Locked && ReleaseTimer <= 0) // TouchDown
                    NewLockCandidate();

                UnlockOtherFeet();

                bool changed = oldState != Locked;

                return changed;
            }
            public bool CheckStep(bool forward = true)
            {
                Foot step = GetFoot(StepIndex(forward));
                Foot release = GetFoot(LockedIndex);

                bool touch = step != null && step.CheckTouching();
                bool locked = step != null && step.CheckLocked();
                bool released = release != null && !release.CheckLocked();

                if (!touch && !locked) // Any attempt to step?
                {
                    StepInterrupt = false;
                    return false;
                }

                StepInterrupt = true;

                if (touch) // Initial contact
                    step.ToggleLock(); // lock foot

                if (locked && release != null) // Initial lock
                    release.ToggleLock(false); // Release

                if (StepInterrupt && released) // Initial release
                    StepInterrupt = false;

                return !StepInterrupt; // Still Stepping?
            }
            public void UnlockAllFeet()
            {
                ReleaseTimer = ReleaseCount;
                LockedIndex = -1;
                UnlockOtherFeet();
            }
            public void IncrementStepping(ClockMode mode)
            {
                IncrementStepping(mode != ClockMode.REV ? 1 : -1);
            }
            public void InitFootStatus()
            {
                NewLockCandidate();
                UnlockOtherFeet();

                foreach (Foot foot in Feet)
                    foot.GearInit();
            }

            int StepIndex(bool forward)
            {
                int stepIndex = LockedIndex + (forward ? 1 : -1);
                stepIndex = stepIndex < 0 ? Feet.Count - 1 : stepIndex >= Feet.Count ? 0 : stepIndex;
                return stepIndex;
            }
            void UnlockOtherFeet()
            {
                Foot expected = GetFoot(LockedIndex);
                foreach (Foot foot in Feet)
                    if (foot != expected)
                        foot.ToggleLock(false);
            }
            void IncrementStepping(int incr)
            {
                SetLockedIndex(LockedIndex + incr);
            }
            void SetLockedIndex(int step)
            {
                LockedIndex = step;
                LockedIndex = LockedIndex < 0 ? Feet.Count - 1 : LockedIndex >= Feet.Count ? 0 : LockedIndex;
            }
            void NewLockCandidate()
            {
                for (int i = 0; i < Feet.Count; i++)
                {
                    Foot check = GetFoot(i);
                    if (check.CheckLocked() || check.CheckTouching())
                    {
                        check.ToggleLock(true);
                        LockedIndex = i;
                        Locked = true;

                        if (CurrentWalk != null)
                        {
                            CurrentWalk.LoadKeyFrames(true, check.LockIndex);
                            CurrentWalk.StepDelay = true;
                        }


                        return;
                    }
                }

                Locked = false;
                LockedIndex = -1;
            }

            public void SyncJoints()
            {
                foreach (Joint joint in Joints)
                    joint.Sync();
            }
            public void ZeroJointSet(KeyFrame frame = null)
            {
                if (frame == null)
                    foreach (Joint joint in Joints)
                        joint.OverwriteAnimTarget(0);
                else
                    foreach (JointFrame jFrame in frame.Jframes)
                        jFrame.Joint.OverwriteAnimTarget(jFrame.MySetting.MyValue());
            }
            public void SnapShotPlane()
            {
                if (Plane == null)
                    return;

                TargetPlane = Plane.WorldMatrix;
                TurnPlane = Plane.WorldMatrix;
            }
            public void TogglePlaneing(bool toggle)
            {
                if (toggle && Locked)
                    SnapShotPlane();

                foreach (Foot foot in Feet)
                    foot.UpdateFootPlaneing(toggle && Locked);
            }
            void UpdatePlaneBuffer(Vector3 playerInput)
            {
                playerInput *= PlaneScalar;

                BufferPlane = MatrixD.CreateFromYawPitchRoll(playerInput.Y, playerInput.X, playerInput.Z);

                TargetPlane = MatrixD.Multiply(BufferPlane, TargetPlane);

                BufferPlane = MatrixD.Multiply(TargetPlane, MatrixD.Invert(Plane.WorldMatrix));

                MatrixD.GetEulerAnglesXYZ(ref BufferPlane, out PlaneBuffer);
            }
            void UpdateTurnBuffer(double playerTurn)
            {
                TurnBuffer.Y = playerTurn * TurnScalar;
            }
            public bool UpdatePlanars()
            {
                if (Plane == null)
                    return false;

                UpdatePlaneBuffer(InputRotationBuffer);
                UpdateTurnBuffer(InputTurnBuffer);

                bool safety = false;
                for (int i = 0; i < 3; i++)
                    if (Math.Abs(PlaneBuffer.GetDim(i)) > SAFETY)
                    {
                        SnapShotPlane();
                        break;
                    }

                foreach (Foot foot in Feet)
                {
                    if (foot != null)
                    {
                        foot.GenerateAxisMagnitudes(Plane.WorldMatrix);
                        for (int i = 0; i < foot.Planars.Count; i++)
                            if (foot.Planars[i] != null)
                            {
                                Joint plane = foot.GetPlanar(i);

                                if (safety)
                                {
                                    plane.PlaneCorrection = 0;
                                    continue;
                                }

                                if (plane.TAG == TurnTag && !foot.Locked)
                                {
                                    plane.PlaneCorrection = GeneratePlaneCorrection(plane, foot.PlanarRatio, TurnBuffer);
                                    DebugBinStream.Append($"PlaneCorrection Lifted {i}: {plane.PlaneCorrection}\n");
                                }

                                else
                                {
                                    plane.PlaneCorrection = GeneratePlaneCorrection(plane, foot.PlanarRatio, -PlaneBuffer);
                                }
                            }
                    }
                }
                return true;
            }
            double GeneratePlaneCorrection(Joint joint, Vector3 planarRatios, Vector3 angleCorrections)
            {
                if (joint == null)
                    return 0;

                double output = 0;
                for (int i = 0; i < 3; i++)
                {
                    double planarsum = joint.PlanarDots.GetDim(i) * planarRatios.GetDim(i) * (angleCorrections.GetDim(i) * RAD2DEG);

                    /*
                    
                     1,0,0
                     0,1,0
                     0,0,-1

                     */

                    // x = + / y = + / z = -
                    //output = i == 2 ? output - planarsum : output + planarsum;
                    output += planarsum;
                }
                return output;
            }
        }


    }
}
