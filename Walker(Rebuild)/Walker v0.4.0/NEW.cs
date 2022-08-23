using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    /*
     
                void UpdateCurrentStatorVelocity(double delta, ref string debug)
            {
                double mag;
                switch (Type)
                {
                    case JointType.ROTOR:
                        if (Stator == null)
                            return;

                        mag = Stator.Angle - LastPosition;
                        int dir = Math.Sign(mag);
                        if (Math.Abs(mag) > ROTOR_SPEED_CHK)
                            mag = (mag - 360) * dir;

                        CurrentVelocity = (mag / TWO_PI) * (60000d/delta); // milsec to rpm
                        LastPosition = Stator.Angle;

                        debug += $"Rotor: {LastPosition} : {CurrentVelocity}\n";
                        break;

                    case JointType.HINGE:
                        if (Stator == null)
                            return;

                        mag = Stator.Angle - LastPosition;
                        CurrentVelocity = (mag / TWO_PI) * (60000d/delta); // milsec to rpm
                        LastPosition = Stator.Angle;

                        debug += $"Hinge: {LastPosition} : {CurrentVelocity}\n";
                        break;

                    case JointType.PISTON:
                        break;
                }
            }

     */

    partial class Program : MyGridProgram
    {
        #region MAIN

        const string CockpitName = "PILOT";
        const string LCDgroupName = "LCDS";

        const float Rad2Pi = (float)(180 / Math.PI);
        const float Threshold = .2f;
        const float Scaling = .5f;
        const float MaxSpeed = 5f;
        const float ForceGradientMin = 1000;
        const float ForceGradientMax = 10000000000;
        const float ClockIncrmentMag = 0.005f;
        const float TriggerCap = 0.1f;

        const double TWO_PI = (2 * Math.PI);
        const double ONE_RPM = TWO_PI / 60;
        const double ROTOR_SPEED_CHK = TWO_PI * 0.9d;

        IMyCockpit Control;

        List<JointSet> JointBin = new List<JointSet>();
        List<Sequence> Animations;

        Sequence CurrentWalk;
        JointSet CurrentWalkSet;

        ClockMode AnimationState;

        bool bInitialized = false;
        bool bWalking = false;
        //bool bAnimated;
        bool bSnapping = true;
        bool bLoaded = false;

        bool bTargetActive = true;
        bool bStatorControlActive = true;

        /// GUI SECTION ///////////////////////////
        IMyTextPanel ButtonPanel;
        IMyTextPanel GUIPanel;
        IMyTextPanel SplashPanel;

        bool bCapLines = false;

        GUIMode CurrentGUIMode = GUIMode.MAIN;
        GUILayer CurrentGUILayer = GUILayer.JSET;

        int[] CurrentGUISelection = new int[4];
        int CursorIndex = 0;
        int LineBufferSize = 6;
        int[] SelObjIndex = new int[] { 0, 0, 0, 0 };
        int SelectedLineIndex = 0;

        string[][] ButtonLabels = new string[3][];
        string MainText;
        string DefaultMainText = "(MainMenu)";
        string InfoText = "(InfoScreen)";
        string[] Cursor = new string[] { "  ", "->" };
        static readonly string[] SaveTolkens =
        {
            "!",
            "$",
            "#",
            ":",
            "%0",
            "%1",
            "&0",
            "&1",
            "&2"
        };

        /// DEBUGGING /////////////////////////////
        IMyTextSurface[] CockPitScreens = new IMyTextSurface[3];
        List<IMyTextPanel> DebugScreens = new List<IMyTextPanel>();
        string DebugBinStream;
        string DebugBinStatic;
        ClockMode OldWalkState;

        /// SAMPLE OBJECTS ////////////////////////
        static readonly string[] SampleJointNames =
        {
            "L_ANK_R",
            "L_ANK_P",
            "L_KNEE",
            "L_HIP_P",
            "L_HIP_Y",
            "R_HIP_Y",
            "R_HIP_P",
            "R_KNEE",
            "R_ANK_P",
            "R_ANK_R"
        };
        static readonly string[] SampleFeetNames =
        {
            "L_PADS",
            "R_PADS",
            "L_TOES",
            "R_TOES"
        };

        JointSet SampleLegs;
        Sequence SampleWalk;

        /// ENUMS /////////////////////////////////
        public enum JointType
        {
            ROTOR,
            HINGE,
            PISTON
        }
        public enum SequenceMode
        {
            RESTING,
            STANDARD
        }
        public enum LimbMode
        {
            RELEASEING,
            STANDARD,
            INTERRUPTABLE,
            INTERRUPTED
        }
        public enum ClockMode
        {
            PAUSE,
            FOR,
            REV,
        }

        public enum GUIMode
        {
            MAIN,
            INFO,
            LIBRARY
        }
        public enum GUILayer
        {
            JSET,
            SEQUENCE,
            FRAME,
            JOINT
        }
        public enum GUINav
        {
            SCROLL_UP,
            SCROLL_DOWN,
            UP,
            DOWN,
            BACK,
            SELECT
        }

        /// CLASSES ///////////////////////////////
        class Joint
        {
            /// EXTERNALS ///

            public IMyMotorStator Stator;
            public IMyPistonBase Piston;
            public IMyMechanicalConnectionBlock Connection;

            public string Name;
            public int Index;
            //public bool Mirror;

            /// INTERNALS ///

            public JointType Type;
            public float[] LerpPoints = new float[2];
            bool bLerping = false;
            public string LoadedFrame;
            public float AnimTarget;
            public float TrueTarget;
            public int Direction;
            public double CurrentVelocity;
            public double TargetVelocity;
            public float ZBcurrent;

            public Joint(IMyMotorStator stator, int index, string name = "default")//, bool mirror = false) //  bool isRotor = true, int clone = -1
            {
                Stator = stator;
                Connection = Stator;
                stator.Enabled = true;
                Type = Connection.BlockDefinition.ToString().Contains("Hinge") ? JointType.HINGE : JointType.ROTOR;

                if (Type == JointType.ROTOR)
                {
                    stator.LowerLimitDeg = float.MinValue;
                    stator.UpperLimitDeg = float.MaxValue;
                }
                else
                {
                    stator.LowerLimitDeg = -90;
                    stator.UpperLimitDeg = 90;
                }

                Name = name;
                Index = index;

                LoadedFrame = "none";
            }
            public Joint(IMyPistonBase piston, int index, string name = "default")//, bool mirror = false)
            {
                Piston = piston;
                Connection = Stator;
                Name = name;
                Index = index;
                //Mirror = mirror;
                Type = JointType.PISTON;
                LoadedFrame = "none";
            }

            public void UpdateForce(float force)
            {
                if (Type == JointType.PISTON)
                {
                    Piston.SetValueFloat("MaxImpulseAxis", force);
                    Piston.SetValueFloat("MaxImpulseNonAxis", force);
                }
                else
                {
                    Stator.Torque = force;
                    Stator.BrakingTorque = force;
                }
            }
            public void LoadAnimationFrame(JointFrame frame, bool forward = true, bool interrupt = false)
            {
                if (forward)
                {
                    LerpPoints[0] = interrupt? ReturnCurrentStatorPosition() : LerpPoints[1];
                    LerpPoints[1] = frame.LerpPoint;
                }
                else
                {
                    LerpPoints[1] = interrupt? ReturnCurrentStatorPosition() : LerpPoints[0];
                    LerpPoints[0] = frame.LerpPoint;
                }
            }
            public void OverwriteTrueTarget(float value)
            {
                TrueTarget = value;
            }
            public void LerpAnimationFrame(float lerpTime, ref string debugBin)
            {
                if (lerpTime > 1 ||
                    lerpTime < 0)
                    return;

                bLerping = true;
                float target;

                if (Type == JointType.ROTOR)
                {
                    float mag = Math.Abs(LerpPoints[0] - LerpPoints[1]);
                    int dir = (mag > 180) ? Math.Sign(LerpPoints[0] - LerpPoints[1]) : Math.Sign(LerpPoints[1] - LerpPoints[0]);
                    mag = mag > 180 ? 360 - mag : mag;
                    mag *= (lerpTime * dir);

                    target = LerpPoints[0] + mag;
                    target = (target > 360) ? target - 360 : target;
                    target = (target < 0) ? target + 360 : target;
                }
                else
                    target = LerpPoints[0] + ((LerpPoints[1] - LerpPoints[0]) * lerpTime);

                //AnimTarget += target;
                AnimTarget = target;

                //debugBin += $"AT: {AnimTarget}\n";
            }
            public void UpdateJoint(bool targetActive, double delta, ref string debug)
            {
                //UpdateCurrentStatorVelocity(delta, ref debug);
                UpdateTrueTarget(targetActive);
                UpdateTargetStatorVelocity(targetActive);

                switch (Type)
                {
                    case JointType.ROTOR:
                        UpdateRotorVector(ref debug);
                        UpdateStator(targetActive);
                        break;

                    case JointType.HINGE:
                        UpdateHingeVector(ref debug);
                        UpdateStator(targetActive);
                        break;

                    case JointType.PISTON:
                        UpdatePiston(targetActive);
                        break;
                }
            }
            public float ReturnCurrentStatorPosition()
            {
                if (Stator != null)
                    return Stator.Angle * Rad2Pi;

                if (Piston != null)
                    return Piston.CurrentPosition;

                return -100;
            }

            void UpdateTrueTarget(bool active)
            {
                if (!active)
                    return;

                // Add dynamic logic here...
                if (bLerping)
                {
                    TrueTarget = AnimTarget;
                    AnimTarget = 0; // Must clear Animation buffer!!
                }

                bLerping = false;
            }
            void UpdateRotorVector(ref string debugBin)
            {
                float current = (Stator.Angle * Rad2Pi);
                float delta = Math.Abs(TrueTarget - current);
                Direction = (delta > 180) ? Math.Sign(current - TrueTarget) : Math.Sign(TrueTarget - current);
                ZBcurrent = (delta > 180) ? 360 - delta : delta;
                Direction = (Direction == 0) ? 1 : Direction;

                //debugBin += $"{ZBcurrent} : {Direction}\n";
            }
            void UpdateHingeVector(ref string debugBin)
            {
                float current = (Stator.Angle * Rad2Pi);
                ZBcurrent = TrueTarget - current;
                Direction = Math.Sign(ZBcurrent);
                ZBcurrent = Math.Abs(ZBcurrent);

                //debugBin += $"{ZBcurrent} : {Direction}\n";
            }

            void UpdateTargetStatorVelocity(bool active)
            {
                if (active)
                {
                    double scale = ZBcurrent * Scaling;
                    //scale = Math.Pow(scale, 3);
                    TargetVelocity = Direction * scale;
                    //TargetVelocity -= CurrentVelocity;

                    if (scale < Threshold)
                        TargetVelocity = 0;
                }
                else
                    TargetVelocity = 0;

                TargetVelocity = (Math.Abs(TargetVelocity) > MaxSpeed) ? MaxSpeed * Math.Sign(TargetVelocity) : TargetVelocity;
            }
            void UpdateStator(bool active)
            {
                if (Stator == null)
                    return;

                Stator.SetValueFloat("Velocity", (float)TargetVelocity);
            }
            void UpdatePiston(bool active)
            {
                if (Piston == null)
                    return;

                Piston.SetValueFloat("Velocity", (float)TargetVelocity);
            }
        }
        class JointFrame
        {
            public Joint Joint;
            public float LerpPoint; // start and finish (rev & for) points of lerp

            public JointFrame(Joint joint, bool snapping = false) // Snapshot
            {
                Joint = joint;
                float point = Joint.ReturnCurrentStatorPosition();
                if (snapping)
                    point = (int)point;
                LerpPoint = point;
            }

            public JointFrame(Joint joint, float lerpPoint) // Hard-coded
            {
                Joint = joint;
                LerpPoint = lerpPoint;
            }

            public bool ChangeStatorLerpPoint(float value)
            {
                float MIN = 0;
                float MAX = 0;

                switch (Joint.Type)
                {
                    case JointType.ROTOR:
                        MAX = 360;
                        break;

                    case JointType.HINGE:
                        MIN = -90;
                        MAX = 90;
                        break;

                    case JointType.PISTON:
                        MAX = 10;
                        break;
                }

                if (value < MIN ||
                    value > MAX)
                    return false;

                LerpPoint = value;
                return true;
            }
        }
        class JointSet
        {
            public string Name;

            public Foot[] Feet;
            public Joint[] Joints;
            public List<Sequence> Sequences;
            public bool bIgnoreFeet;
            //public int LockedFootIndex;
            //float ForceGradientMin;
            //float ForceGradientMax;
            float TriggerTime;
            bool Triggered;
            public bool bLegs; // ?

            class JointSort : Comparer<Joint>
            {
                public override int Compare(Joint x, Joint y)
                {
                    if (x != null && y != null)
                        return x.Index.CompareTo(y.Index);
                    else
                        return 0;
                }
            }

            public JointSet(string name, Joint[] joints, Foot[] feet = null, List<Sequence> sequences = null, bool ignoreFeet = true)
            {
                Name = name;
                Sequences = sequences;
                if (sequences == null)
                    Sequences = new List<Sequence>();
                else
                    foreach (Sequence seq in Sequences)
                        seq.JointSet = this;

                Joints = joints;
                
                if (feet != null &
                    feet.Length == 2)
                    Feet = feet;

                bLegs = feet != null;

                SortJoints();
                bIgnoreFeet = ignoreFeet;
            }
            public JointSet(string name, int jointCount, Foot[] feet = null, List<Sequence> sequences = null, bool ignoreFeet = true)
            {
                Name = name;
                Sequences = sequences;
                if (sequences == null)
                    Sequences = new List<Sequence>();
                else
                    foreach (Sequence seq in Sequences)
                        seq.JointSet = this;

                Joints = new Joint[jointCount];

                if (feet != null &
                    feet.Length == 2)
                    Feet = feet;

                bLegs = feet != null;

                SortJoints();
                bIgnoreFeet = ignoreFeet;
            }
            public bool ReplaceJoint(IMyTerminalBlock block, int index, string name)
            {
                if (index < 0 ||
                    index >= Joints.Length)
                    return false;

                Joint newJoint = null;
                if (block is IMyMotorStator)
                    newJoint = new Joint((IMyMotorStator)block, index, name);

                if (block is IMyPistonBase)
                    newJoint = new Joint((IMyPistonBase)block, index, name);

                if (newJoint == null)
                    return false;

                Joints[index] = newJoint;
                ReIndexJoints();

                return true;
            }
            public bool CheckStep(float lerpTime)
            {
                bool footCheck = false;
                int lockIndex;
                int unLockIndex;
                if (Feet[0].Locked)
                {
                    footCheck = Feet[1].CheckTouching();
                    lockIndex = 1;
                    unLockIndex = 0;
                }
                else
                {
                    footCheck = Feet[0].CheckTouching();
                    lockIndex = 0;
                    unLockIndex = 1;
                }

                if (!Triggered && footCheck) // Initial contact
                {
                    Triggered = true;
                    TriggerTime = lerpTime;
                    Feet[lockIndex].ToggleGrip();
                }

                if (Triggered && !footCheck) // Stepped away or lost contact
                {
                    Triggered = false;
                    Feet[lockIndex].ToggleGrip(false);
                }

                if (Triggered && // Pressed down for long enough
                    Math.Abs(TriggerTime - lerpTime) >= TriggerCap)
                {
                    Feet[lockIndex].ToggleLock();
                    Feet[unLockIndex].ToggleLock(false);
                    Feet[unLockIndex].ToggleGrip(false);
                    return true; // Lock successful
                }
                return false; // Lock failed
            }
            public bool CheckRelease()
            {
                if (bIgnoreFeet)
                    return true;



                return false;
            }
            public bool InitializeGrip(bool clear = false)
            {
                try
                {
                    if (!clear)
                    {
                        Feet[0].ToggleLock();
                        Feet[0].ToggleGrip();
                    }
                    else
                    {
                        Feet[0].ToggleLock(false);
                        Feet[0].ToggleGrip(false);
                    }
                    Feet[1].ToggleLock(false);
                    Feet[1].ToggleGrip(false);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            public void ZeroJointSet()
            {
                foreach (Joint joint in Joints)
                    joint.OverwriteTrueTarget(0);
            }

            void SortJoints()
            {
                // Option A... onus on user
                Array.Sort(Joints, new JointSort());

                //Joints.Sort((x, y) => x.Index.CompareTo(y.Index));

                // Option B
                /*
                foreach(Joint joint in Joints)
                bool MatchStator(string cubeGridName, IMyMotorStator stator)
                bool MatchPiston(string cubeGridName, IMyPistonBase piston)
                */
            }
            void ReIndexJoints()
            {
                for (int i = 0; i < Joints.Length; i++)
                    if (Joints[i] != null)
                        Joints[i].Index = i;
                
            }
            void UpdateForceGradient()
            {
                float forceGradientRange = Math.Abs(ForceGradientMax - ForceGradientMin);

                for (int i = 0; i < Joints.Length; i++)
                {
                    float difference = forceGradientRange * (i / (Joints.Length - 1));
                    if (Feet[0].Locked)
                        Joints[i].UpdateForce(ForceGradientMin + difference);
                    else
                        Joints[i].UpdateForce(ForceGradientMax - difference);
                }

            }
        }
        class Foot
        {
            public bool Locked;
            public bool Gripping;
            public List<IMyLandingGear> Pads;
            public List<IMyMotorStator> Grips;

            public Foot(bool locked, List<IMyLandingGear> pads, List<IMyMotorStator> grips)
            {
                Locked = locked;
                Pads = pads;
                Grips = grips;
                foreach (IMyLandingGear gear in Pads)
                {
                    gear.AutoLock = false;
                    gear.Enabled = true;
                }

                if (Locked)
                    ToggleLock();
            }

            public bool CheckTouching()
            {
                foreach (IMyLandingGear gear in Pads)
                {
                    if (gear.LockMode == LandingGearMode.ReadyToLock)
                        return true;
                }

                return false;
            }
            public void ToggleGrip(bool gripping = true)
            {
                Gripping = gripping;
                int dir = Gripping ? -1 : 1;
                foreach (IMyMotorStator grip in Grips)
                    grip.SetValueFloat("Velocity", MaxSpeed * dir);
            }
            public void ToggleLock(bool locking = true)
            {
                Locked = locking;
                foreach (IMyLandingGear gear in Pads)
                {
                    if (locking)
                    {
                        gear.AutoLock = true;
                        gear.Lock();
                    }
                        
                    if (!locking)
                    {
                        gear.AutoLock = false;
                        gear.Unlock();
                    }                    
                }
            }
        }
        class KeyFrame
        {
            public string Name;
            public JointFrame[] Jframes;
            //public JointSet LegLogic;

            public KeyFrame(string name, JointFrame[] jFrames)//, JointSet legs)
            {
                Name = name;
                Jframes = jFrames;
                //LegLogic = legs;
            }
        }
        class Sequence
        {
            /// EXTERNALS ///
            public string Name;
            //public int CloneIndex;
            //public bool IsMirrored;
            public List<KeyFrame> Frames;
            public JointSet JointSet;

            // Clock
            public ClockMode ClockMode;
            public bool bFrameLoadForward;
            public float CurrentClockTime;

            // Frames
            int StartFrameIndex;
            public KeyFrame CurrentFrame;
            public int CurrentFrameIndex;

            public Sequence(string name, JointSet set = null, List<KeyFrame> frames = null)//, int cloneIndex = -1, bool isMirrored = false)
            {
                Name = name;

                JointSet = set;
                if (JointSet != null)
                    JointSet.Sequences.Add(this);

                Frames = frames;
                if (Frames == null)
                    Frames = new List<KeyFrame>();

                ClockMode = ClockMode.PAUSE;
                CurrentFrameIndex = 0;
                CurrentClockTime = 0;
            }

            public void UpdateClockMode(ClockMode mode)
            {
                ClockMode = mode;
            }
            public bool InitializeSeq(ClockMode mode, int startIndex)
            {
                if (Frames.Count == 0)
                    return false;

                if (startIndex < 0 ||
                    startIndex >= Frames.Count)
                    return false;

                ClockMode = mode;
                CurrentFrame = Frames[startIndex];
                bool forward = mode == ClockMode.FOR;
                foreach (JointFrame jFrame in CurrentFrame.Jframes)
                {
                    jFrame.Joint.LoadAnimationFrame(jFrame, forward);
                }

                return true;
            }
            public bool DemoKeyFrame(int index, ref string debugBin)
            {
                if (index < 0 ||
                    index >= Frames.Count)
                    return false;

                debugBin += "Made It!\n";

                foreach (JointFrame jFrame in Frames[index].Jframes)
                    jFrame.Joint.OverwriteTrueTarget(jFrame.LerpPoint);

                return true;
            }

            public bool UpdateSequence(ref string debugBin)
            {
                //debugBin += "test call...\n";

                if (CurrentFrame == null)
                    return false;

                UpdateTriggers(ref debugBin);
                LerpFrame(CurrentClockTime, ref debugBin);
                return true;
            }
            public void ToggleSequence(ClockMode mode)
            {
                ClockMode = mode;
            }
            public bool AddKeyFrameSnapshot(int index = -1, string name = null, bool snapping = false)
            {
                if (JointSet == null ||
                    JointSet.Joints.Length == 0)
                    return false;

                if (name == null)
                    name = $"Frame_{Frames.Count}";

                List<JointFrame> newJframes = new List<JointFrame>();
                foreach (Joint joint in JointSet.Joints)
                    newJframes.Add(new JointFrame(joint, snapping));
                KeyFrame newFrame = new KeyFrame(name, newJframes.ToArray());


                if (index == -1)
                    Frames.Add(newFrame);
                else
                    Frames.Insert(index, newFrame);

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

            void UpdateTriggers(ref string debugBin)
            {
                bool load = false;
                bool forward = false;
                bool interrupt = false;

                //debugBin += "start of triggers...\n";

                switch (ClockMode)
                {
                    case ClockMode.PAUSE:

                        return;

                    case ClockMode.FOR:
                        forward = true;
                        CurrentClockTime += ClockIncrmentMag;
                        if (CurrentClockTime >= 1)
                        {
                            CurrentClockTime = 0;
                            CurrentFrameIndex++;
                            CurrentFrameIndex = CurrentFrameIndex >= Frames.Count ? 0 : CurrentFrameIndex;
                            if (!bFrameLoadForward)
                            {
                                CurrentFrameIndex++;
                                CurrentFrameIndex = CurrentFrameIndex >= Frames.Count ? 0 : CurrentFrameIndex;
                            }
                            load = true;
                        }
                        if (!JointSet.bIgnoreFeet &&
                            JointSet.CheckStep(CurrentClockTime))
                        {
                            load = true;

                        }
                        break;

                    case ClockMode.REV:
                        forward = false;
                        CurrentClockTime -= ClockIncrmentMag;
                        if (CurrentClockTime <= 0)
                        {
                            CurrentClockTime = 1;
                            CurrentFrameIndex--;
                            CurrentFrameIndex = CurrentFrameIndex < 0 ? Frames.Count - 1 : CurrentFrameIndex;
                            if (bFrameLoadForward)
                            {
                                CurrentFrameIndex--;
                                CurrentFrameIndex = CurrentFrameIndex < 0 ? Frames.Count - 1 : CurrentFrameIndex;
                            }
                            load = true;
                        }
                        if (!JointSet.bIgnoreFeet &&
                            JointSet.CheckStep(CurrentClockTime))
                        {
                            load = true;

                        }
                        break;
                }

                if (!load)
                    return;
                //debugBin += "Loading...\n";

                LoadFrame(CurrentFrameIndex, forward, interrupt, ref debugBin);
            }
            void LoadFrame(int index, bool forward, bool interrupt, ref string debugBin)
            {
                if (JointSet == null )//||
                    //JointSet.Joints.Length == 0)
                    return;

                if (index >= Frames.Count ||
                    index < 0 ||
                    Frames.Count == 0)
                    return;

                CurrentFrame = Frames[index];


                foreach (JointFrame joint in CurrentFrame.Jframes)
                {
                    if (joint.Joint == null)
                        continue;

                    joint.Joint.LoadAnimationFrame(joint, forward);
                }

                bFrameLoadForward = forward;
            }
            void LerpFrame(float lerpTime, ref string debugBin)
            {
                foreach (JointFrame joint in CurrentFrame.Jframes)
                {
                    if (joint.Joint == null)
                        continue;

                    joint.Joint.LerpAnimationFrame(lerpTime, ref debugBin);
                }
                //debugBin += "Lerped!\n";
            }
        }

        /// SAMPLE CONSTRUCTIONS /////////////////
        bool SampleLegsConstructor()
        {
            JointBin.Clear();
            Foot[] feet = new Foot[2];

            List<IMyLandingGear> pads0 = new List<IMyLandingGear>();
            List<IMyLandingGear> pads1 = new List<IMyLandingGear>();
            List<IMyMotorStator> ankles0 = new List<IMyMotorStator>();
            List<IMyMotorStator> ankles1 = new List<IMyMotorStator>();

            IMyBlockGroup group0 = GridTerminalSystem.GetBlockGroupWithName(SampleFeetNames[0]);
            IMyBlockGroup group1 = GridTerminalSystem.GetBlockGroupWithName(SampleFeetNames[1]);
            IMyBlockGroup group2 = GridTerminalSystem.GetBlockGroupWithName(SampleFeetNames[2]);
            IMyBlockGroup group3 = GridTerminalSystem.GetBlockGroupWithName(SampleFeetNames[3]);

            if (group0 != null)
                group0.GetBlocksOfType(pads0);
            if (group1 != null)
                group1.GetBlocksOfType(pads1);
            if (group2 != null)
                group2.GetBlocksOfType(ankles0);
            if (group3 != null)
                group3.GetBlocksOfType(ankles1);

            if (pads0.Count == 0 ||
                pads1.Count == 0)
            {
                DebugBinStream += "Feet were not found!\n";
                return false;
            }

            if (ankles0.Count == 0 ||
                ankles1.Count == 0)
            {
                DebugBinStream += "Ankles were not found!\n";
                return false;
            }

            feet[0] = new Foot(true, pads0, ankles0);
            feet[1] = new Foot(false, pads1, ankles1);

            SampleLegs = new JointSet("SampleLegs", SampleJointNames.Length, feet);

            for (int i = 0; i < SampleJointNames.Length; i++)
            {
                IMyTerminalBlock nextJoint = GridTerminalSystem.GetBlockWithName(SampleJointNames[i]);
                if (nextJoint == null)
                {
                    DebugBinStream += $"Joint {i}:{SampleJointNames[i]} was not found!\n";
                    return false;
                }
                if (!SampleLegs.ReplaceJoint(nextJoint, i, SampleJointNames[i]))
                {
                    DebugBinStream += $"Joint {i}:{SampleJointNames[i]} was not a stator/piston!\n";
                    return false;
                }
                DebugBinStream += $"Joint {i}:{SampleJointNames[i]} successfully added!\n";
            }


            JointBin.Add(SampleLegs);

            SampleWalk = new Sequence("walking", SampleLegs);
            CurrentWalk = SampleWalk;

            return true;
        }

        /// RUNTIME CONSTRUCTIONS ////////////////
        bool CreateJointSet()
        {
            Echo("yo");

            string groupName = null;
            UserInputString(ref groupName);
            if (groupName == null)
                return false;

            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(groupName);
            if (group == null)
                return false;

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            List<IMyMotorStator> stators = new List<IMyMotorStator>();
            List<IMyPistonBase> pistons = new List<IMyPistonBase>();
            group.GetBlocksOfType(stators);
            group.GetBlocksOfType(pistons);
            blocks.AddRange(stators);
            blocks.AddRange(pistons);
            if (blocks.Count == 0)
                return false;

            JointSet newJointSet = new JointSet(groupName, blocks.Count);

            int i = 0;
            foreach (IMyTerminalBlock block in blocks)
            {
                newJointSet.ReplaceJoint(block, i, block.CustomName);
                i++;
            }
            return true;
        }

        /// PLAYER INPUTS ////////////////////////
        void ToggleAnimations(ClockMode mode)
        {
            foreach (Sequence seq in Animations)
            {
                seq.ToggleSequence(mode);
            }
        }
        void ZeroCurrentWalk()
        {
            if (CurrentWalkSet == null)
                return;

            if (CurrentWalk == null)
                return;

            bWalking = false;
            CurrentWalk.UpdateClockMode(ClockMode.PAUSE);
            CurrentWalkSet.ZeroJointSet();
        }

        /// GUI COMPONENTS ////////////////////////
        void LibraryBuilder()
        {
            MainText = DefaultMainText + "\n\nStandby";
            ButtonLabels[0] = new string[] { "Info", "Library" }; // Main Menu
            ButtonLabels[1] = new string[] { "ScrollUp", "ScrollDown", "Main Menu" }; // Info Panel
            ButtonLabels[2] = new string[] { "UpList", "DownList", "UpDirectory", "OpenDirectory", "NewItem", "DeleteItem", "EditItem", "MainMenu" }; // Library Menu
        }
        void GUIUpdate()
        {
            if (ButtonPanel != null && GUIPanel != null)
            {
                string buttonString = RawButtonStringBuilder(CurrentGUIMode);
                ButtonPanel.WriteText(buttonString, false);

                string[] guiData;
                if (CurrentGUIMode == GUIMode.LIBRARY)
                    guiData = LibraryStringBuilder();
                else
                    guiData = StaticStringBuilder();
                string guiString = FormattedGUIStringBuilder(guiData);
                GUIPanel.WriteText(guiString, false);
            }
        }
        string RawButtonStringBuilder(GUIMode mode)
        {
            string output = "";
            for (int i = 0; i < ButtonLabels[(int)mode].Length; i++)
                output += $"{i + 1} - {ButtonLabels[(int)mode][i]}\n";
            return output;
        }
        string[] StaticStringBuilder()
        {
            string input = (CurrentGUIMode == GUIMode.MAIN) ? MainText : InfoText;
            string[] output = input.Split('\n');
            return output;
        }
        string[] LibraryStringBuilder()
        {
            List<string> stringList = new List<string>();
            stringList.Add("===Library===");
            stringList.Add("=============");
            int layer = (int)CurrentGUILayer;
            int cursor = 0;

            int limbIndex = 0;
            if (JointBin.Count == 0)
            {
                stringList.Add("No limbs loaded!\n");
            }

            for (int jSetIndex = 0; jSetIndex < JointBin.Count; jSetIndex++)
            {
                if (CurrentGUILayer == GUILayer.JSET &&
                          SelObjIndex[0] == limbIndex)
                {
                    CursorIndex = stringList.Count - 1;
                    cursor = 1;
                }
                else
                    cursor = 0;

                stringList.Add(Cursor[cursor] + "|" + limbIndex + ":" + JointBin[jSetIndex].Name);
                limbIndex++;

                if (layer < 1 ||
                    SelObjIndex[0] != jSetIndex)
                    continue;

                int sequenceIndex = 0;
                if (JointBin[jSetIndex].Sequences.Count == 0)
                {
                    stringList.Add(" No sequences found!");
                }

                for (int seqIndex = 0; seqIndex < JointBin[jSetIndex].Sequences.Count; seqIndex++)
                {
                    if (CurrentGUILayer == GUILayer.SEQUENCE &&
                              SelObjIndex[1] == sequenceIndex)
                    {
                        CursorIndex = stringList.Count - 1;
                        cursor = 1;
                    }
                    else
                        cursor = 0;

                    stringList.Add(Cursor[cursor] + " |" + sequenceIndex + ":" + JointBin[jSetIndex].Sequences[seqIndex].Name);
                    sequenceIndex++;

                    if (layer < 2 ||
                        SelObjIndex[1] != seqIndex)
                        continue;

                    if (JointBin[jSetIndex].Sequences[seqIndex].Frames.Count == 0)
                        stringList.Add("  No frames found!");

                    for (int frameIndex = 0; frameIndex < JointBin[jSetIndex].Sequences[seqIndex].Frames.Count; frameIndex++)
                    {
                        if (CurrentGUILayer == GUILayer.FRAME &&
                                  SelObjIndex[2] == frameIndex)
                        {
                            CursorIndex = stringList.Count - 1;
                            cursor = 1;
                        }
                        else
                            cursor = 0;

                        stringList.Add($"{Cursor[cursor]}  |{frameIndex} : {JointBin[jSetIndex].Sequences[seqIndex].Frames[frameIndex].Name}");
                        if (layer < 3 ||
                            SelObjIndex[2] != frameIndex)
                            continue;

                        if (JointBin[jSetIndex].Joints.Length == 0)
                            stringList.Add("   No joints found!");

                        for (int jFrameIndex = 0; jFrameIndex < JointBin[jSetIndex].Sequences[seqIndex].Frames[frameIndex].Jframes.Count(); jFrameIndex++)
                        {
                            if (CurrentGUILayer == GUILayer.JOINT &&
                                      SelObjIndex[3] == jFrameIndex)
                            {
                                CursorIndex = stringList.Count - 1;
                                cursor = 1;
                            }
                            else
                                cursor = 0;

                            stringList.Add($"{Cursor[cursor]}    |{jFrameIndex}:{JointBin[jSetIndex].Sequences[seqIndex].Frames[frameIndex].Jframes[jFrameIndex].Joint.Name}:{JointBin[jSetIndex].Sequences[seqIndex].Frames[frameIndex].Jframes[jFrameIndex].LerpPoint}");
                        }
                    }
                } 
            }

            string[] output = stringList.ToArray();
            return output;
        }
        string FormattedGUIStringBuilder(string[] input)
        {
            string output = "";
            int startIndex = CursorIndex - LineBufferSize;
            startIndex = startIndex < 2 ? 2 : startIndex;
            output += input[0] + "\n";
            output += input[1] + "\n";

            if (bCapLines)
                for (int i = 0; i < input.Length; i++)
                {
                    output += input[i] + "\n";
                }

            else
            for (int i = startIndex; i < startIndex + (2*LineBufferSize) && i < input.Length; i++)
            {
                output += input[i] + "\n";
            }

            return output;
        }
        void ButtonPress(int button)
        {
            switch (CurrentGUIMode)
            {
                case GUIMode.MAIN:
                    MainMenuFunctions(button);
                    break;

                case GUIMode.INFO:
                    InfoMenuFunctions(button);
                    break;

                case GUIMode.LIBRARY:
                    LibraryMenuFunctions(button);
                    break;
            }
            GUIUpdate();
        }
        void MainMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    CurrentGUIMode = GUIMode.INFO;
                    break;

                case 2:
                    CurrentGUIMode = GUIMode.LIBRARY;
                    break;

                case 3:
                    //LimbDetection();
                    //MainText = DefaultMainText + "\n\n" + Limbs.Count + " limbs found";
                    break;
            }
        }
        void InfoMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    GUINavigation(GUINav.UP);
                    break;

                case 2:
                    GUINavigation(GUINav.DOWN);
                    break;

                case 3:
                    CurrentGUIMode = GUIMode.MAIN;
                    break;
            }
        }
        void LibraryMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    GUINavigation(GUINav.UP);
                    break;

                case 2:
                    GUINavigation(GUINav.DOWN);
                    break;

                case 3:
                    GUINavigation(GUINav.BACK);
                    break;

                case 4:
                    GUINavigation(GUINav.SELECT);
                    break;

                case 5:
                    AddItem();
                    break;

                case 6:
                    DeleteItem();
                    break;

                case 7:
                    EditItem();
                    break;

                case 8:
                    CurrentGUIMode = GUIMode.MAIN;
                    break;
            }
        }
        void GUINavigation(GUINav dir)
        {

            switch (dir)
            {
                case GUINav.SCROLL_UP:
                    //ScrollStartIndex -= (ScrollStartIndex == 0) ? 0 : 1;
                    //ScrollStartIndex--;
                    //ScrollStartIndex = ScrollStartIndex < 0 ? indexCap - 1 : ScrollStartIndex;
                    break;
                case GUINav.SCROLL_DOWN:
                    //ScrollStartIndex += 1;
                    //ScrollStartIndex++;
                    //ScrollStartIndex = ScrollStartIndex >= indexCap ? 0 : ScrollStartIndex;
                    break;
                case GUINav.UP:
                    ChangeSelection(false);
                    break;
                case GUINav.DOWN:
                    ChangeSelection();
                    break;
                case GUINav.BACK:
                    ChangeGUILayer(false);
                    break;
                case GUINav.SELECT:
                    ChangeGUILayer(true);
                    break;
            }
        }
        void ChangeGUILayer(bool up)
        {
            int layer = (int)CurrentGUILayer;
            if (up)
                layer += ((int)CurrentGUILayer == 3) ? 0 : 1;
            else
                layer -= ((int)CurrentGUILayer == 0) ? 0 : 1;
            CurrentGUILayer = (GUILayer)layer;
            SelectedLineIndex = SelObjIndex[layer];
        }
        void ChangeSelection(bool incr = true)
        {

            if (incr)
                SelectedLineIndex++;

            else
                SelectedLineIndex--;


            JointSet limb = JointBin[SelObjIndex[0]];
            Sequence seq = null;
            if (limb != null)
                seq = JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]];
            KeyFrame frame = null;
            if (seq != null)
                frame = JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]];

            int seqCount = limb == null ? 0 : limb.Sequences.Count;
            int frameCount = seq == null ? 0 : seq.Frames.Count;
            int jointCount = seq == null ? 0 : seq.JointSet.Joints.Length;

            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    SelectedLineIndex = (SelectedLineIndex >= JointBin.Count) ? 0 : SelectedLineIndex;
                    SelectedLineIndex = (SelectedLineIndex < 0) ? JointBin.Count - 1 : SelectedLineIndex;
                    SelObjIndex[0] = SelectedLineIndex;
                    break;

                case GUILayer.SEQUENCE:
                    SelectedLineIndex = (SelectedLineIndex >= seqCount) ? 0 : SelectedLineIndex;
                    SelectedLineIndex = (SelectedLineIndex < 0) ? seqCount - 1 : SelectedLineIndex;
                    SelObjIndex[1] = SelectedLineIndex;
                    break;

                case GUILayer.FRAME:
                    SelectedLineIndex = (SelectedLineIndex >= frameCount) ? 0 : SelectedLineIndex;
                    SelectedLineIndex = (SelectedLineIndex < 0) ? frameCount - 1 : SelectedLineIndex;
                    SelObjIndex[2] = SelectedLineIndex;
                    break;

                case GUILayer.JOINT:
                    SelectedLineIndex = (SelectedLineIndex >= jointCount) ? 0 : SelectedLineIndex;
                    SelectedLineIndex = (SelectedLineIndex < 0) ? jointCount - 1 : SelectedLineIndex;
                    SelObjIndex[3] = SelectedLineIndex;
                    break;
            }

            JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].DemoKeyFrame(SelObjIndex[2], ref DebugBinStatic);
        }
        void AddItem()
        {
            string name = null;
            UserInputString(ref name);

            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    Echo("yo");
                    CreateJointSet();
                    break;

                case GUILayer.SEQUENCE:
                    if (name == null)
                        name = $"New Sequence {JointBin[SelObjIndex[0]].Sequences.Count}";
                    new Sequence(name, JointBin[SelObjIndex[0]]);
                    break;

                case GUILayer.FRAME:
                    if (name == null)
                        name = $"New Frame {JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames.Count}";
                    int index;
                    if (SelObjIndex[2] == JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames.Count - 1)
                        index = -1;
                    else
                        index = SelObjIndex[2];
                    JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].AddKeyFrameSnapshot(index, name, bSnapping);
                    break;
            }
        }
        void DeleteItem()
        {
            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    JointBin.RemoveAt(SelObjIndex[0]);
                    break;

                case GUILayer.SEQUENCE:
                    JointBin[SelObjIndex[0]].Sequences.RemoveAt(SelObjIndex[1]);
                    break;

                case GUILayer.FRAME:
                    JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].RemoveKeyFrameAtIndex(SelObjIndex[2]);
                    break;
            }

            int a = JointBin.Count;
            if (SelObjIndex[0] >= a && SelObjIndex[0] > 0)
            {
                SelObjIndex[0] = a - 1;
                SelectedLineIndex--;
            }
            if (JointBin[SelObjIndex[0]] == null)
            {
                SelObjIndex[1] = 0;
                SelObjIndex[2] = 0;
                SelObjIndex[3] = 0;
                return;
            }
                
            int b = JointBin[SelObjIndex[0]].Sequences.Count;
            if (SelObjIndex[1] >= b && SelObjIndex[1] > 0)
            {
                SelObjIndex[1] = b - 1;
                SelectedLineIndex--;
            }
            if (JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]] == null)
            {
                SelObjIndex[2] = 0;
                SelObjIndex[3] = 0;
                return;
            }

            int c = JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames.Count;
            if (SelObjIndex[2] >= c && SelObjIndex[2] > 0)
            {
                SelObjIndex[2] = c - 1;
                SelectedLineIndex--;
            }
            if (JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]] == null)
            {
                SelObjIndex[3] = 0;
                return;
            }
        }
        bool UserInputString(ref string buffer)
        {
            try
            {
                StringBuilder myStringBuilder = new StringBuilder();
                CockPitScreens[2].ReadText(myStringBuilder);
                buffer = myStringBuilder.ToString();
                if (buffer == "")
                    buffer = null;
                return true;
            }
            catch
            {
                return false;
            }
        }
        bool UserInputFloat(ref float buffer)
        {
            try
            {
                StringBuilder myStringBuilder = new StringBuilder();
                CockPitScreens[2].ReadText(myStringBuilder);
                buffer = float.Parse(myStringBuilder.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }
        void EditItem()
        {
            float value = 0;
            string name = null;
            UserInputString(ref name);
            if (name != null && name.Contains(":"))
                return;
            bool floatGood = UserInputFloat(ref value);

            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    JointBin[SelObjIndex[0]].Name = name;
                    break;

                case GUILayer.SEQUENCE:
                    JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Name = name;
                    break;

                case GUILayer.FRAME:
                    JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]].Name = name;
                    break;

                case GUILayer.JOINT:
                    if (floatGood)
                    {
                        JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]].Jframes[SelObjIndex[3]].ChangeStatorLerpPoint(value);
                        JointBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].DemoKeyFrame(SelObjIndex[2], ref DebugBinStatic);
                    }
                    else
                        JointBin[SelObjIndex[0]].Joints[SelObjIndex[3]].Name = name;
                    break;
            }
        }

        /// UPATES ///////////////////////////////
        void WalkManager()
        {
            if (CurrentWalk == null)
                return;

            if (bWalking == false)
                return;

            bool updated = CurrentWalk.UpdateSequence(ref DebugBinStream);
            DebugBinStream += updated ? "Walking...\n" : "Sequence Error!\n";
        }
        void AnimationManager()
        {
            if (Animations == null ||
                Animations.Count == 0)
                return;
            foreach (Sequence seq in Animations)
            {
                seq.UpdateSequence(ref DebugBinStream);
            }
        }
        void StatorManager()
        {
            if (JointBin == null ||
                JointBin.Count == 0)
                return;

            if (!bStatorControlActive)
                return;

            foreach (JointSet set in JointBin)
            {
                foreach (Joint joint in set.Joints)
                {
                    joint.UpdateJoint(bTargetActive, Runtime.TimeSinceLastRun.TotalMilliseconds, ref DebugBinStream);
                }
            }
        }
        void DisplayManager()
        {
            if (CockPitScreens[0] == null ||
                CockPitScreens[1] == null ||
                CockPitScreens[2] == null)
                return;

            if (DebugScreens.Count < 3) // MANUALLY UPDATE!!!
                return;

            if (CurrentWalkSet == null ||
                CurrentWalk == null ||
                CurrentWalk.Frames == null)
                return;

            string output = $"CurrentSet Joints(Name:Pos:Vel) | Delta-Time:{Runtime.TimeSinceLastRun.TotalMilliseconds}\n";

            for (int i = 0; i < CurrentWalkSet.Joints.Length; i++)
            {
                output += $"{CurrentWalkSet.Joints[i].Name} : {CurrentWalkSet.Joints[i].ReturnCurrentStatorPosition()} : {CurrentWalkSet.Joints[i].TargetVelocity}\n";
            }
            DebugScreens[2].WriteText(output);

            output = "Sample Walk Frames:\n";
            for (int i = 0; i < CurrentWalk.Frames.Count; i++)
            {
                output += $"{CurrentWalk.Frames[i].Name}:\n";

                for (int j = 0; j < CurrentWalk.Frames[i].Jframes.Length; j++)
                    output += $"- {CurrentWalk.Frames[i].Jframes[j].Joint.Name} : {CurrentWalk.Frames[i].Jframes[j].LerpPoint}\n";

                output += "\n";
            }
            DebugScreens[0].WriteText(output);

            output = "Mech Status:\n\n" +

            $"SampleWalkClockState: {CurrentWalk.ClockMode}\n" +
            $"CurrentWalkClockTime: {CurrentWalk.CurrentClockTime}\n" +
            $"CurrentWalkFrameIndex: {CurrentWalk.CurrentFrameIndex}\n\n" +

            $"Walking: {bWalking}\n" +
            $"TargetActive: {bTargetActive}\n" +
            $"StatorControlActive: {bStatorControlActive}\n" +
            $"Snapping: {bSnapping}";
            CockPitScreens[0].WriteText(output);

            output = "Debug Plus!:\n";
            foreach (Joint joint in CurrentWalkSet.Joints)
            {
                output += $"{joint.Name} : {joint.LerpPoints[0]} : {joint.LerpPoints[1]}\n";
            }
            CockPitScreens[1].WriteText(output);

            output = "Splash Panel!:\n";
            output += $"CurrentLayer: {CurrentGUILayer}\n";
            int k = 0;
            foreach (int index in SelObjIndex)
            {
                output += $"Layer:{(GUILayer)k} | Index:{index}\n";
                k++;
            }

            SplashPanel.WriteText(output);
        }
        void WalkToggle()
        {
            if (bWalking)
                CurrentWalk.UpdateClockMode(OldWalkState);
            else
                CurrentWalk.UpdateClockMode(ClockMode.PAUSE);
        }

        /// ENTRY POINTS ////////////////////////
        public Program()
        {
            try
            {
                Control = (IMyCockpit)GridTerminalSystem.GetBlockWithName(CockpitName);
                IMyBlockGroup panelGroup = GridTerminalSystem.GetBlockGroupWithName(LCDgroupName);
                List<IMyTextPanel> panels = new List<IMyTextPanel>();
                panelGroup.GetBlocksOfType(panels);

                for (int i = 0; i < 3 || i < panels.Count; i++)
                {
                    if (Control != null &&
                        i < 3)
                    {
                        CockPitScreens[i] = Control.GetSurface(i);
                        CockPitScreens[i].ContentType = ContentType.TEXT_AND_IMAGE;
                        CockPitScreens[i].WriteText("");
                    }

                    if (i < panels.Count)
                    {
                        panels[i].ContentType = ContentType.TEXT_AND_IMAGE;
                        panels[i].WriteText("");
                        DebugScreens.Add(panels[i]);
                    }
                }

                ButtonPanel = panels[3];
                SplashPanel = panels[5];
                GUIPanel = panels[4];

                LibraryBuilder();
                GUIUpdate();

                Runtime.UpdateFrequency = UpdateFrequency.Update1;

                DebugBinStream = string.Empty;
                DebugBinStatic = string.Empty;

                bInitialized = true;
            }
            catch
            {
                bInitialized = false;
                return;
            }


            if (!Load(ref DebugBinStatic))
            {
                Echo("Load Failed!");
                SampleLegsConstructor();
                bLoaded = false;
            }
            else
            {
                Echo("Load Success!");
                bLoaded = true;
            }

            DebugScreens[1].WriteText(DebugBinStatic);
            DebugBinStatic = string.Empty;
        }
        public void Main(string argument, UpdateType updateSource)
        {
            if (!bInitialized)
                return;

            DebugScreens[1].WriteText(DebugBinStream);
            DebugBinStream = string.Empty; // MUST HAPPEN!


            switch (argument)
            {
                case "TEST0":
                    Echo(CurrentWalkSet.InitializeGrip().ToString());
                    break;

                case "TEST1":
                    Echo(CurrentWalkSet.InitializeGrip(true).ToString());
                    break;

                case "TOGGLE_STATOR_TARGET":
                    bTargetActive = !bTargetActive;
                    break;

                case "TOGGLE_STATOR_CONTROL":
                    bStatorControlActive = !bStatorControlActive;
                    break;

                case "ANIMATIONS":
                    AnimationState = AnimationState == ClockMode.REV ? ClockMode.PAUSE : AnimationState + 1;
                    ToggleAnimations(AnimationState);
                    break;

                case "INITIALIZE_WALK":
                    CurrentWalk.InitializeSeq(ClockMode.FOR, 0);
                    OldWalkState = ClockMode.FOR;
                    bWalking = true;
                    break;

                case "ZERO_WALK":
                    ZeroCurrentWalk();
                    break;

                case "TOGGLE_WALK_DIR":
                    if (OldWalkState == ClockMode.FOR)
                        OldWalkState = ClockMode.REV;
                    else
                        OldWalkState = ClockMode.FOR;
                    WalkToggle();
                    break;

                case "TOGGLE_WALK_PAUSE":
                    bWalking = !bWalking;
                    WalkToggle();
                    break;

                case "SAVE":
                    Save();
                    break;

                case "LOAD":
                    Load(ref DebugBinStatic);
                    break;

                default:
                    try
                    {
                        if (!argument.Contains("BUTTON:"))
                            break;
                        string code = argument.Split(':')[1];
                        int button = int.Parse(code);
                        ButtonPress(button);
                    }
                    catch
                    {
                        // Was not a GUI button;
                    }
                    break;
            }

            WalkManager();
            AnimationManager();
            StatorManager();
            DisplayManager();
        }
        public bool Load(ref string debugBin)
        {
            /*
Tolkens:
:  - Divider
/n - Entry
&0 - Foot (&0:index)
&1 - Ankle (&1:CustomName)
&2 - Pad (&2:CustomName)
#  - JointSet (#:Name:bIgnoreFeet)
!  - Joint (!:Name:Index:CustomName)
$  - Sequence ($:Name)
%0 - KeyFrame (%0:Name)
%1 - JointFrame (%1:LerpPoint)
 */
            JointBin.Clear();

            string[] load = Me.CustomData.Split('\n');

            debugBin += "data loaded...\n";

            List<Sequence> seqBuffer = new List<Sequence>();
            List<JointFrame> jFrameBuffer = new List<JointFrame>();
            List<KeyFrame> kFrameBuffer = new List<KeyFrame>();
            List<Joint> jointBuffer = new List<Joint>();
            List<Foot> footBuffer = new List<Foot>();
            List<IMyLandingGear> gearBuffer = new List<IMyLandingGear>();
            List<IMyMotorStator> gripBuffer = new List<IMyMotorStator>();

            int debugCounter = 0;

            foreach (string next in load)
            {
                try
                {
                    string[] entry = next.Split(':');

                    switch (entry[0])
                    {
                        case "!":
                            IMyTerminalBlock joint = GridTerminalSystem.GetBlockWithName(entry[3]);
                            if (joint == null)
                                break;
                            if (joint is IMyMotorStator)
                                jointBuffer.Add(new Joint((IMyMotorStator)joint, int.Parse(entry[2]), entry[1]));
                            if (joint is IMyPistonBase)
                                jointBuffer.Add(new Joint((IMyPistonBase)joint, int.Parse(entry[2]), entry[1]));
                            break;

                        case "%1":
                            jFrameBuffer.Add(new JointFrame(jointBuffer[jFrameBuffer.Count], float.Parse(entry[1])));
                            break;

                        case "%0":
                            kFrameBuffer.Add(new KeyFrame(entry[1], jFrameBuffer.ToArray()));
                            jFrameBuffer.Clear();
                            break;

                        case "$":
                            List<KeyFrame> newFrames = new List<KeyFrame>();
                            newFrames.AddRange(kFrameBuffer);
                            seqBuffer.Add(new Sequence(entry[1], null, newFrames));
                            kFrameBuffer.Clear();
                            break;

                        case "&2":
                            IMyLandingGear pad = (IMyLandingGear)GridTerminalSystem.GetBlockWithName(entry[1]);
                            if (pad != null)
                                gearBuffer.Add(pad);
                            break;

                        case "&1":
                            IMyMotorStator grip = (IMyMotorStator)GridTerminalSystem.GetBlockWithName(entry[1]);
                            if (grip != null)
                                gripBuffer.Add(grip);
                            break;

                        case "&0":
                            if (gripBuffer.Count != 0 &&
                                gearBuffer.Count != 0)
                            {
                                List<IMyLandingGear> newGears = new List<IMyLandingGear>();
                                List<IMyMotorStator> newGrips = new List<IMyMotorStator>();
                                newGears.AddRange(gearBuffer);
                                newGrips.AddRange(gripBuffer);

                                footBuffer.Add(new Foot(bool.Parse(entry[2]), newGears, newGrips));
                                gearBuffer.Clear();
                                gripBuffer.Clear();
                            }
                            break;

                        case "#":
                            List<Sequence> newSeqs = new List<Sequence>();
                            newSeqs.AddRange(seqBuffer);
                            JointSet newSet = new JointSet(entry[1], jointBuffer.ToArray(), footBuffer.ToArray(), newSeqs, bool.Parse(entry[2]));

                            JointBin.Add(newSet);

                            seqBuffer.Clear();
                            footBuffer.Clear();
                            jointBuffer.Clear();
                            break;
                    }
                }
                catch
                {
                    return false;
                }
                debugCounter++;
            }

            if (JointBin.Count < 1 ||
                JointBin[0] == null)
                return false;

            if (JointBin[0].Sequences == null)
                return false;

            if (JointBin[0].Sequences.Count < 1)
                return false;

            if (JointBin[0].Sequences[0] != null)
            {
                CurrentWalkSet = JointBin[0];           //  >: |
                CurrentWalk = JointBin[0].Sequences[0]; //  >: |
                return true;
            }

            return false;
        }
        public void Save()
        {
            /*
            Tolkens:
            :  - Divider
            /n - Entry
            #  - JointSet (#:Name:bIgnoreFeet)
            &0 - Foot (&0:index:bLocked)
            &1 - Ankle (&1:CustomName)
            &2 - Pad (&2:CustomName)
            !  - Joint (!:Name:Index:CustomName)
            $  - Sequence ($:Name)
            %0 - KeyFrame (%0:Name)
            %1 - JointFrame (%1:LerpPoint)
             */

            string save = string.Empty;

            foreach (JointSet set in JointBin)
            {
                if (set.bLegs)
                {
                    for (int i = 0; i < set.Feet.Length; i++)
                    {
                        foreach (IMyLandingGear pad in set.Feet[i].Pads)
                            save += $"&2:{pad.CustomName}\n";

                        foreach (IMyMotorStator ankle in set.Feet[i].Grips)
                            save += $"&1:{ankle.CustomName}\n";

                        save += $"&0:{i}:{set.Feet[i].Locked}\n";
                    }
                }

                foreach (Joint joint in set.Joints)
                {
                    save += $"!:{joint.Name}:{joint.Index}:{joint.Connection.CustomName}\n";
                }

                foreach (Sequence seq in set.Sequences)
                {
                    foreach (KeyFrame frame in seq.Frames)
                    {
                        foreach (JointFrame jFrame in frame.Jframes)
                        {
                            save += $"%1:{jFrame.LerpPoint}\n";
                        }
                        save += $"%0:{frame.Name}\n";
                    }
                    save += $"$:{seq.Name}\n";
                }
                save += $"#:{set.Name}:{set.bIgnoreFeet}\n";
            }

            Me.CustomData = save;
        }

        #endregion
    }
}
