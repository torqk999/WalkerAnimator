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
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        #region MAIN

        /// ANIMATION VARIABLES ////////////////////

        const string BlockGroupSignature = "[LIMB]";
        const string ShipControllerName = "[PILOT]";
        const string ButtonPanelName = "[BUTTONS]";
        const string GUIPanelName = "[GUI]";
        const string DebugPanelName = "[DEBUG]";

        const float Rad2Pi = (float)(180 / Math.PI);
        const float Threshold = .5f;
        const float Scaling = 4f;
        const float MaxSpeed = 10f;

        const float FullTorque = 1000000000;
        //const float FullBreak = 1000000000;

        const float ClockAnimationScale = 0.005f;
        const float ClockFootingScale = 0.010f;

        /// ANIMATION LIBRARIES ///////////////////////

        /// <summary>
        /// First Dimension: ScheduleIndex. This is the set of sequences across all limbs that correspond to one another.
        /// Second Dimension: TargetLimbIndex
        /// Value: SequenceIndex of the corresponding limb to be loaded at this schedule index.
        /// </summary>
        public static readonly int[,] SequenceSchedule = new int[,]
        {
            {0,0},
            {0,0},
            {0,0},
            {0,0},
            {0,0},
            {0,0}
        };

        public static readonly float[,] LegTorqueProfiles = new float[,]
        {

        };

        /// ANIMATION LOGIC ///////////////////////

        bool TestMode = true;

        int Direction = 0;
        bool Auto = false;
        bool Resting = true;
        bool Freeze = false;

        bool SequenceLoaded = false;
        bool FrameLoaded = false;
        int CurrentScheduleIndex = 0;
        int CurrentFrameIndex = 0;

        float ClockCycle = 0;
        int ClockMode = 0;

        int NecessaryFootCollisions = 1;
        bool IgnoreFeet = false;
        List<int> ConnectedFeet = new List<int>();
        List<int> ConnectableFeet = new List<int>();

        ///////////////////////////////////////////
        /// YOU'VE GONE TOO FAR, TURN BACK NOW! ///
        ///////////////////////////////////////////

        /// BLOCK REFERENCES //////////////////////

        IMyShipController ThisMechController;
        List<IMyTextSurface> CockpitScreen = new List<IMyTextSurface>();
        IMyTextPanel DebugPanel;
        IMyTextPanel JointSnapshot;
        IMyTextPanel ButtonPanel;
        IMyTextPanel GUIPanel;

        /// CLASS LISTS ///////////////////////////

        List<Limb> Limbs = new List<Limb>();

        List<Sequence> SampleSequences = new List<Sequence>();

        List<Joint> SampleJointsRight = new List<Joint>();
        List<Joint> SampleJointsLeft = new List<Joint>();

        float[] SampleKnee;
        float[] SampleHip;
        float[] SampleAnkle;

        float StraightRest = 0;

        /// GUI SECTION ///////////////////////////

        GUIMode CurrentGUIMode = GUIMode.MAIN;
        GUILayer CurrentGUILayer = GUILayer.LIMB;
        int[] CurrentGUISelection = new int[4];
        int ScrollStartIndex = 0;
        int TotalLinesCount = 10;
        int[] SelectedObjectIndex = new int[] { 0, 0, 0, 0 };
        int SelectedLineIndex = 0;
        string[][] ButtonLabels = new string[3][];
        string MainText;
        string DefaultMainText =
            "(MainMenu)";
        string InfoText =
            "(InfoScreen)";
        string[] Cursor = new string[] { "  ", "->" };

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
        public enum GUIMode
        {
            MAIN,
            INFO,
            LIBRARY
        }
        public enum GUILayer
        {
            LIMB,
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

        class LogicFrame
        {
            public string Name;
            public int Index;
            public LimbMode StandardMode;
            public LimbMode InterruptMode;
            public int StandardTorqueProfile;
            public int InterruptTorqueProfile;

            public LogicFrame(string name, int index, LimbMode standMode = LimbMode.STANDARD, LimbMode interruptMode = LimbMode.STANDARD, int standardTorque = 0, int interruptTorque = 0)
            {
                Name = name;
                Index = index;
                StandardMode = standMode;
                InterruptMode = interruptMode;
                StandardTorqueProfile = standardTorque;
                InterruptTorqueProfile = interruptTorque;
            }
        }

        class Sequence
        {
            public string Name;
            //public int Index;
            public int StartFrameIndex;
            public int CurrentFrameIndex;
            public List<float[]> KeyFrames; // D1 = Frame, D2 = Joint
            //public List<KeyFrame> Frames;
            //public int[] FrameIndexList;
            //public List<LogicFrame> LogicFrames;
            //public SequenceMode Mode;

            public Sequence(string name, int startFrameIndex) //, int index, int[] frameIndexList, SequenceMode mode = SequenceMode.STANDARD
            {
                Name = name;
                StartFrameIndex = startFrameIndex;
                CurrentFrameIndex = 0;
            }

            public Sequence()
            {
                Name = "NewSequence";
                StartFrameIndex = 0;
                CurrentFrameIndex = 0;
            }

            public Sequence Clone()
            {
                Sequence newSequence = new Sequence(Name, StartFrameIndex);
                return newSequence;
            }
        }
        /// <summary>
        /// This is a Stator (currently rotor, hinge is not yet implemented) that is a part of a parent Limb.
        /// It contains it's own set of animation frames which are called in sequence by it's parent limb.
        /// Use the customData of the stator to create your own set of animation frames using the following format:
        /// 
        /// <para>
        /// <br>string(jointName):int(cloneIndex):bool(IsDynamic?)</br>
        /// <br>string(frameName):int(frameIndex):float(forwardTargetDeg):float(reverseTargetDeg)</br>
        /// </para>
        /// 
        /// <para>eg.: 
        /// <br>Hip:0:False</br>
        /// <br>Step1:0:10:350</br>
        /// <br>Step2:1:350:180</br>
        /// </para>
        /// 
        /// </summary>
        class Joint
        {
            /// EXTERNALS ///

            public IMyMotorStator Stator;
            public IMyPistonBase Piston;

            public string Name;
            public int Index;
            public bool Mirror;

            //public bool IsRotor;

            /// INTERNALS ///

            JointType Type;
            public string LoadedFrame;
            public float AnimTarget;
            public float TrueTarget;
            public int Direction;
            public float Velocity;
            public float ZBcurrent;
            //public float Scale;

            public Joint(IMyMotorStator stator, int index, string name = "default", bool mirror = false, bool isRotor = true) //  bool isRotor = true, int clone = -1
            {
                Stator = stator;
                Name = name;
                Index = index;
                Mirror = mirror;
                Type = (isRotor) ? JointType.ROTOR : JointType.HINGE;
                LoadedFrame = "none";
            }

            public Joint(IMyPistonBase piston, int index, string name = "default", bool mirror = false)
            {
                Piston = piston;
                Name = name;
                Index = index;
                Mirror = mirror;
                Type = JointType.PISTON;
                LoadedFrame = "none";
            }

            /*
            public void UpdateKeyFrames(List<KeyFrame> frames)
            {
                Frames = frames;
            }
            */
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

            /// <summary>
            /// Called by the Parent Limb when advancing to the next animation frame in the animation schedule (Refer to the ScheduleManager for initial call)
            /// </summary>
            /// <param name="index">Indice value associated with frame. Cross check the schedule with your initial frame list.</param>
            /// <param name="forward">Direction of animation. Determines whether you are sweeping towards the end or start point of the given frame.</param>
            public void UpdateAnimTarget(float target)
            {
                AnimTarget = target;
            }
            /// <summary>
            /// Internal Batch Call
            /// </summary>
            /// <param name="active"></param>
            public void UpdateJoint(bool active)
            {
                UpdateTrueTarget();
                switch(Type)
                {
                    case JointType.ROTOR:
                        UpdateRotorVector();
                        break;

                    case JointType.HINGE:
                        UpdateHingeVector();
                        break;
                }
                
                UpdateStator(active);
            }
            /// <summary>
            /// Dynamic offset to the base Animation Target to produce alignment.
            /// </summary>
            public void UpdateTrueTarget()
            {
                // Add dynamic logic here...
                TrueTarget = AnimTarget;
            }
            /// <summary>
            /// The zero-based Current position of the rotor, in terms of the Target vector of the currently loaded animation frame.
            /// Takes the shortest path to the target (Future implementation might include restricted sweep arc for rotors to avoid unusual behaviour... ).
            /// </summary>
            public void UpdateRotorVector()
            {
                float current = (Stator.Angle * Rad2Pi);
                float delta = Math.Abs(TrueTarget - current);
                Direction = (delta > 180) ? Math.Sign(180 - TrueTarget) : Math.Sign(TrueTarget - current);
                ZBcurrent = (delta > 180) ? (360 + Direction * (TrueTarget - current)) : delta;
                Direction = (Direction == 0) ? 1 : Direction;
            }

            public void UpdateHingeVector()
            {
                float current = (Stator.Angle * Rad2Pi);
                ZBcurrent = TrueTarget - current;
                Direction = Math.Sign(ZBcurrent);
            }

            /// <summary>
            /// The velocity final velocity takes in the given sweeping direction set by the LoadFrame(), and a scalar produced from the given zero-based target and current,
            /// in order to deccelerate the Stator and not travel past the intended target angle.
            /// </summary>
            public void UpdateStator(bool active)
            {
                if (Stator != null)
                {
                    //float velocity;
                    if (active)
                    {
                        float scale = ZBcurrent / Scaling;
                        Velocity = Direction * scale;
                        if (scale < Threshold)
                            Velocity = 0;
                        //Scale = scale;
                    }
                    else
                        Velocity = 0;
                    Velocity = (Math.Abs(Velocity) > MaxSpeed) ? MaxSpeed * Math.Sign(Velocity) : Velocity;

                    Stator.SetValueFloat("Velocity", Velocity);
                }
            }
        }
        /// <summary>
        /// This is a group of joints, including a landing gear, and is the root object that is called by the ScheduleLoader().
        /// It contains the necessary methods for updating and implementing animation logic, and frame updates.
        /// Be sure to properly construct each Joint, and each of those joint's Frame lists for animations to execute as intended.
        /// To create a Limb in-game, Create a block group and give it the following naming structure:
        /// 
        /// <para>
        /// string(BlockGroupSignature)string(name):int(index):int(mirrorIndex):string(M)
        /// </para>
        /// 
        /// <para>
        /// ex.1: [ANIM]Leg:0
        /// ex.2: [ANIM]Leg:1:0:M
        /// (A pair of legs, the second will be a mirrored clone of the first)
        /// </para>
        /// 
        /// CloneIndex not required, will default to -1 if unused.
        /// Use the clone index to designate a target limb you wish this to be the clone of.
        /// Joints in both target and source limbs must share respective cloneIndex's for this to implement.
        /// Append the letter 'M' at the end with a colon if it is intended to be a 'mirrored' version.
        /// Mirrored joints reflect across the 0-180 line on rotors.
        /// (Further documentation will be added as I progress).
        /// </summary>
        class Limb
        {
            /// EXTERNALS ///

            public string Name;
            //public int Index;
            public IMyLandingGear Foot;
            public int CloneIndex;
            public bool IsMirrored;

            /// LIBRARIES ///

            public List<Sequence> Sequences;
            public List<Joint> Joints;
            //public TorqueProfiles Torques;
            public float[,] Torques; // D1 = Profile, D2 = Joint

            /// INTERNALS ///

            public int CurrentSequenceIndex;
            public int CurrentFrameIndex;
            public LimbMode CurrentFrameMode;
            public float NetVelocity;

            public Limb(string name, IMyLandingGear foot, int cloneIndex = -1, bool isMirrored = false)// int index, List<Joint> joints, List<Sequence> sequences,
            {
                Name = name;
                //Index = index;
                CloneIndex = cloneIndex;
                Foot = foot;
                IsMirrored = isMirrored;

                Sequences = new List<Sequence>();
                Joints = new List<Joint>();
                //Torques = new TorqueProfiles();
                // TorqueProfiles???

                CurrentSequenceIndex = 0;
                CurrentFrameIndex = 0;
                CurrentFrameMode = LimbMode.STANDARD;
                NetVelocity = 0;
            }

            public void UpdateSequences(List<Sequence> sequences)
            {
                Sequences = sequences;
            }
            public void UpdateJoints(List<Joint> joints)
            {
                joints.Sort((x, y) => x.Index.CompareTo(y.Index));
                Joints = joints;
            }
            public void LoadSequence(int sequenceIndex)
            {
                CurrentSequenceIndex = sequenceIndex;
            }
            public void LoadFrame(int frameIndex)
            {
                CurrentFrameIndex = frameIndex;
                foreach (Joint joint in Joints)
                    joint.UpdateAnimTarget(Sequences[CurrentSequenceIndex].KeyFrames[CurrentFrameIndex][joint.Index]);
            }
            public void LoadTorqueProfile()
            {
                int torqueIndex = (int)CurrentFrameMode;
                foreach (Joint joint in Joints)
                    joint.UpdateForce(Torques[torqueIndex, joint.Index]);
            }
            public void FootManager()
            {
                switch (CurrentFrameMode)
                {
                    case LimbMode.INTERRUPTABLE: // Grab State
                        Foot.AutoLock = false;
                        break;

                    case LimbMode.RELEASEING: // Release State
                        Foot.AutoLock = false;
                        Foot.Unlock();
                        break;

                    case LimbMode.STANDARD: // Stepping State
                        Foot.AutoLock = false;
                        break;
                }
            }
            public void Animate(bool active)
            {
                NetVelocity = 0;
                foreach (Joint next in Joints)
                {
                    next.UpdateJoint(active);
                    NetVelocity += next.Velocity;
                }
            }
            public void CloneSequences(Limb targetLimb)
            {
                Sequences.Clear();
                foreach (Sequence next in targetLimb.Sequences)
                    Sequences.Add(next.Clone());
            }
        }

        /// SAMPLE CONSTRUCTIONS //////////////////
        /*
        void SampleFramesConstructor()
        {
            SampleHip.Clear();
            SampleHip.Add(StraightRest.Clone());
            SampleHip.Add(new KeyFrame("Step1", 1, 280));
            SampleHip.Add(new KeyFrame("Step2", 2, 325));
            SampleHip.Add(new KeyFrame("Lean1", 3, 30));
            SampleHip.Add(new KeyFrame("Lean2", 4, 350));

            SampleKnee.Clear();
            SampleKnee.Add(StraightRest.Clone());
            SampleKnee.Add(new KeyFrame("Step1", 1, 135));
            SampleKnee.Add(new KeyFrame("Step2", 2, 45));
            SampleKnee.Add(new KeyFrame("Lean1", 3, 0));
            SampleKnee.Add(new KeyFrame("Lean2", 4, 60));

            SampleAnkle.Clear();
            SampleAnkle.Add(StraightRest.Clone());
            SampleAnkle.Add(new KeyFrame("Step1", 1, 45));
            SampleAnkle.Add(new KeyFrame("Step2", 2, 15));
            SampleAnkle.Add(new KeyFrame("Lean1", 3, 30));
            SampleAnkle.Add(new KeyFrame("Lean2", 4, 50));
        }
        void SampleSequenceConstructor()
        {
            SampleSequences.Clear();

            int[] RestingLocked = new int[] { };
            int[] RestingUnlocked = new int[] { };
            int[] ForwardLocked = new int[] { };
            int[] ForwardUnlocked = new int[] { };
            int[] ReverseLocked = new int[] { };
            int[] ReverseUnlocked = new int[] { };

            SampleSequences.Add(new Sequence("RestingLocked", 0, RestingLocked, SequenceMode.RESTING));
            SampleSequences.Add(new Sequence("RestingUnlocked", 1, RestingUnlocked, SequenceMode.RESTING));
            SampleSequences.Add(new Sequence("ForwardLocked", 2, ForwardLocked));
            SampleSequences.Add(new Sequence("ForwardUnlocked", 3, ForwardUnlocked));
            SampleSequences.Add(new Sequence("ReverseLocked", 4, ReverseLocked));
            SampleSequences.Add(new Sequence("ReverseUnlocked", 5, ReverseUnlocked));
        }
        void SampleLegsConstructor()
        {
            Limbs.Clear();

            // Right Leg

            IMyMotorStator RightHipStator = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("HIP_R");
            IMyMotorStator RightKneeStator = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("KNEE_R");
            IMyMotorStator RightAnkleStator = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("ANKLE_R");
            IMyLandingGear RightFoot = (IMyLandingGear)GridTerminalSystem.GetBlockWithName("FOOT_R");

            List<KeyFrame> RightHipFrames = new List<KeyFrame>();
            List<KeyFrame> RightKneeFrames = new List<KeyFrame>();
            List<KeyFrame> RightAnkleFrames = new List<KeyFrame>();

            foreach (KeyFrame next in SampleHip)
                RightHipFrames.Add(next.Clone());

            foreach (KeyFrame next in SampleKnee)
                RightKneeFrames.Add(next.Clone());

            foreach (KeyFrame next in SampleAnkle)
                RightAnkleFrames.Add(next.Clone());

            Joint RightHipJoint = new Joint(RightHipStator, 0, "Hip");
            Joint RightKneeJoint = new Joint(RightKneeStator, 1, "Knee");
            Joint RightAnkleJoint = new Joint(RightAnkleStator, 2, "Ankle");

            RightHipJoint.UpdateKeyFrames(RightHipFrames);
            RightKneeJoint.UpdateKeyFrames(RightKneeFrames);
            RightAnkleJoint.UpdateKeyFrames(RightAnkleFrames);

            SampleJointsRight.Clear();
            SampleJointsRight.Add(RightHipJoint);
            SampleJointsRight.Add(RightKneeJoint);
            SampleJointsRight.Add(RightAnkleJoint);

            Limb RightLeg = new Limb("RightLeg", RightFoot);
            RightLeg.UpdateSequences(SampleSequences);
            RightLeg.UpdateJoints(SampleJointsRight);

            Limbs.Add(RightLeg);

            // Left Leg

            IMyMotorStator LeftHipStator = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("HIP_L");
            IMyMotorStator LeftKneeStator = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("KNEE_L");
            IMyMotorStator LeftAnkleStator = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("ANKLE_L");
            IMyLandingGear LeftFoot = (IMyLandingGear)GridTerminalSystem.GetBlockWithName("FOOT_L");

            List<KeyFrame> LeftHipFrames = new List<KeyFrame>();
            List<KeyFrame> LeftKneeFrames = new List<KeyFrame>();
            List<KeyFrame> LeftAnkleFrames = new List<KeyFrame>();

            Joint LeftHipJoint = new Joint(LeftHipStator, 0, "Hip");
            Joint LeftKneeJoint = new Joint(LeftKneeStator, 1, "Knee");
            Joint LeftAnkleJoint = new Joint(LeftAnkleStator, 2, "Ankle");

            SampleJointsLeft.Clear();
            SampleJointsLeft.Add(LeftHipJoint);
            SampleJointsLeft.Add(LeftKneeJoint);
            SampleJointsLeft.Add(LeftAnkleJoint);

            Limb LeftLeg = new Limb("LeftLeg", LeftFoot);
            LeftLeg.UpdateSequences(SampleSequences);
            RightLeg.UpdateJoints(SampleJointsLeft);

            LeftLeg.CloneFrames(RightLeg, true);

            Limbs.Add(LeftLeg);
        }
        */

        /// MANAGERS //////////////////////////////

        /// <summary>
        /// Currently responding to 'W' and 'S' in the designated IMyShipController, as well as the proved "MARCH" switch(argument).
        /// </summary>
        void InputResponse()
        {
            if (ThisMechController != null && !Freeze)
            {
                if (Auto)
                {

                }

                else
                {
                    int newDirection = Math.Sign(-ThisMechController.MoveIndicator.Z);
                    if (Direction != newDirection)
                    {
                        Direction = newDirection;
                        SetSequence(Direction);
                    }
                }
            }
        }
        void FootStatusUpdate()
        {
            ConnectedFeet.Clear();
            ConnectableFeet.Clear();
            int limbIndex = 0;
            foreach (Limb limb in Limbs)
            {
                if (limb.Foot.LockMode == LandingGearMode.Locked)
                    ConnectedFeet.Add(limbIndex);
                if (limb.Foot.LockMode == LandingGearMode.ReadyToLock)
                    ConnectableFeet.Add(limbIndex);
                limbIndex++;
            }
        }
        void InterruptManager()
        {

        }

        /// <summary>
        /// Controls Scheduling loading, and manipulates the ClockMode, as well as calling limbs to animate, and handling Foot collision logic
        /// </summary>
        void ScheduleManager()
        {
            /*
            if (!SequenceLoaded)
            {

                ClockMode = 0;                  // Default Mode
                SequenceMode targetMode = (Resting) ? SequenceMode.RESTING : SequenceMode.STANDARD;
                int firstConnectedLimbIndexFound = ConnectedFeet[0];
                int firstConnectedSequenceIndexFound = Limbs[firstConnectedLimbIndexFound].Sequences.Find(x => x.Mode == targetMode).Index; // Must find "locked resting sequence" first!!! Check Order...
                //int correspondingSequenceScheduleIndex = 0;
                for (int i = SequenceSchedule.GetLowerBound(0); i < SequenceSchedule.GetUpperBound(0); i++)
                {
                    if (SequenceSchedule[i, firstConnectedLimbIndexFound] == firstConnectedSequenceIndexFound)
                    {
                        CurrentScheduleIndex = i;
                        break;
                    }
                }

                SequenceLoader(CurrentScheduleIndex);
                SequenceLoaded = true;
            }*/
            
            if (!FrameLoaded)
            {
                bool frameExists = true;
                foreach (Limb limb in Limbs)
                {
                    if (limb.Sequences[CurrentScheduleIndex].KeyFrames.Count <= CurrentFrameIndex)
                    {
                        frameExists = false;
                        break;
                    }
                }

                if (!frameExists)   // End of sequence animation?? (Should rarely hit this, might work-around)
                {
                    CurrentFrameIndex = 0;
                    SequenceLoaded = false;
                }

                else
                {
                    FrameLoader(CurrentFrameIndex);
                    FrameLoaded = true;
                }
            }
        }

        /// <summary>
        /// Uses the updated states from InputResponse() and ScheduleManager() to advance the internal clock.
        /// </summary>
        void ClockManager()
        {
            if (!Freeze)
            {
                float clockStep = 0;

                switch (ClockMode)
                {
                    case 0: // Animation Clock
                        clockStep = ClockAnimationScale;
                        break;
                    case 1: // Stepping Clock
                        clockStep = ClockFootingScale;
                        break;
                    case 2: // Interrupt Clock
                        clockStep = 0;
                        break;
                }

                ClockCycle += clockStep;

                /*
                if (ClockCycle < 0)
                {
                    ClockCycle = 1;
                    SequenceCurrentIndex -= 1;
                    if (SequenceCurrentIndex == 0)
                        SequenceCurrentIndex = (FrameSchedule.Length / 2) - 1;
                    ScheduleLoaded = false;
                }
                */

                if (ClockCycle > 1)
                {
                    ClockCycle = 0;
                    CurrentFrameIndex += 1;
                    FrameLoaded = false;
                }
            }
        }

        /// GUI COMPONENTS ////////////////////////

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
                string guiString = RawGUIStringBuilder(guiData);
                GUIPanel.WriteText(guiString, false);
            }
        }
        string RawButtonStringBuilder(GUIMode mode)
        {
            string output = "";
            foreach (string label in ButtonLabels[(int)mode])
                output += "- " + label + "\n";
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
            if (Limbs.Count == 0)
                stringList.Add("No limbs loaded!\n");
            foreach (Limb limb in Limbs)
            {
                cursor = (CurrentGUILayer == GUILayer.LIMB &&
                          SelectedObjectIndex[0] == limbIndex) ? 1 : 0;

                stringList.Add(Cursor[cursor] + "|" + limbIndex + ":" + limb.Name);
                limbIndex++;
                if (layer > 0)
                {
                    int sequenceIndex = 0;
                    if (limb.Sequences.Count == 0)
                        stringList.Add(" No sequences found!");
                    foreach (Sequence sequence in limb.Sequences)
                    {
                        cursor = (CurrentGUILayer == GUILayer.SEQUENCE &&
                                  SelectedObjectIndex[1] == sequenceIndex) ? 1 : 0;

                        stringList.Add(Cursor[cursor] + " |" + sequenceIndex + ":" + sequence.Name);
                        sequenceIndex++;
                        if (layer > 1)
                        {
                            if (sequence.KeyFrames.Count == 0)
                                stringList.Add("  No frames found!");
                            for (int frameIndex = 0; frameIndex < sequence.KeyFrames.Count; frameIndex++)
                            {
                                cursor = (CurrentGUILayer == GUILayer.FRAME &&
                                          SelectedObjectIndex[2] == frameIndex) ? 1 : 0;

                                stringList.Add(Cursor[cursor] + "  |" + frameIndex + "\n");
                                if (layer > 2)
                                {
                                    if (limb.Joints.Count == 0)
                                        stringList.Add("   No joints found!");
                                    foreach (Joint joint in limb.Joints)
                                    {
                                        cursor = (CurrentGUILayer == GUILayer.JOINT &&
                                                  SelectedObjectIndex[3] == joint.Index) ? 1 : 0;

                                        stringList.Add(Cursor[cursor] + "    |" + joint.Index + ":" + joint.Name + ":" + sequence.KeyFrames[frameIndex][joint.Index]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            string[] output = stringList.ToArray();
            return output;
        }
        string RawGUIStringBuilder(string[] input)
        {
            string output = "";
            ScrollStartIndex = (input.Length - TotalLinesCount > ScrollStartIndex) ? ScrollStartIndex : input.Length - TotalLinesCount; // Adjust Scroll point to lowest needed point
            ScrollStartIndex = (ScrollStartIndex < 0) ? 0 : ScrollStartIndex;                                                           // Adjust Scroll point to positive range
            ScrollStartIndex = (ScrollStartIndex > input.Length - 1) ? input.Length - 1 : ScrollStartIndex;                              // Adjust Scroll point to be within range of the input
            for (int i = ScrollStartIndex; i < input.Length; i++)
            {
                if (i - ScrollStartIndex > TotalLinesCount)
                    break;
                output += input[i] + "\n";
            }

            return output;
        }
        void ButtonPress(int button)
        {
            if (CurrentGUIMode == GUIMode.MAIN)
                MainMenuFunctions(button);
            else if (CurrentGUIMode == GUIMode.INFO)
                InfoMenuFunctions(button);
            else if (CurrentGUIMode == GUIMode.LIBRARY)
                LibraryMenuFunctions(button);
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
                    LimbDetection();
                    MainText = DefaultMainText + "\n\n" + Limbs.Count + " limbs found";
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
                    CurrentGUIMode = GUIMode.MAIN;
                    break;
            }
        }
        void GUINavigation(GUINav dir)
        {
            int layer;
            switch (dir)
            {
                case GUINav.SCROLL_UP:
                    ScrollStartIndex -= (ScrollStartIndex == 0) ? 0 : 1;
                    break;
                case GUINav.SCROLL_DOWN:
                    ScrollStartIndex += 1;
                    break;
                case GUINav.UP:
                    SelectedLineIndex -= (SelectedLineIndex == 0) ? 0 : 1;
                    ChangeSelection();
                    break;
                case GUINav.DOWN:
                    SelectedLineIndex += 1;
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
            SelectedLineIndex = SelectedObjectIndex[layer];
        }
        void ChangeSelection()
        {
            Limb limb = Limbs[SelectedObjectIndex[0]];
            int frameCount = 0;
            if (limb != null)
                frameCount = Limbs[SelectedObjectIndex[0]].Sequences[SelectedObjectIndex[1]].KeyFrames.Count;
            int jointCount = Limbs[SelectedObjectIndex[0]].Sequences[SelectedObjectIndex[1]].KeyFrames[SelectedObjectIndex[2]].Length;

            switch (CurrentGUILayer)
            {
                case GUILayer.LIMB:
                    SelectedLineIndex = (SelectedLineIndex < Limbs.Count - 1) ? SelectedLineIndex : Limbs.Count - 1;
                    SelectedObjectIndex[0] = SelectedLineIndex;
                    break;
                case GUILayer.SEQUENCE:
                    SelectedLineIndex = (SelectedLineIndex < limb.Sequences.Count - 1) ? SelectedLineIndex : limb.Sequences.Count - 1;
                    SelectedObjectIndex[1] = SelectedLineIndex;
                    break;
                case GUILayer.FRAME:
                    SelectedLineIndex = (SelectedLineIndex <= frameCount) ? SelectedLineIndex : frameCount - 1;
                    SelectedObjectIndex[2] = SelectedLineIndex;
                    break;
                case GUILayer.JOINT:
                    SelectedLineIndex = (SelectedLineIndex <= jointCount) ? SelectedLineIndex : jointCount - 1;
                    SelectedObjectIndex[3] = SelectedLineIndex;
                    break;
            }
        }
        void UpdateSelection()
        {
        }
        void RenameSelection()
        {

        }
        void AddItem()
        {
            switch (CurrentGUILayer)
            {
                case GUILayer.SEQUENCE:
                    Limbs[SelectedObjectIndex[0]].Sequences.Add(new Sequence());
                    break;
                case GUILayer.FRAME:
                    int jointCount = Limbs[SelectedObjectIndex[0]].Joints.Count;
                    Limbs[SelectedObjectIndex[0]].Sequences[SelectedObjectIndex[1]].KeyFrames.Add(new float[jointCount]);
                    break;
            }
        }
        void DeleteItem()
        {
            switch (CurrentGUILayer)
            {

            }
        }

        /// ANIMATION BUILDERS ///////////////////

        void LimbDetection()
        {
            Debug("herro\n", false);
            Limbs.Clear();
            List<IMyBlockGroup> allGroups = new List<IMyBlockGroup>();
            List<IMyBlockGroup> newLimbGroups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(allGroups);
            foreach (IMyBlockGroup nextGroup in allGroups)
                if (nextGroup.Name.Contains(BlockGroupSignature))
                    newLimbGroups.Add(nextGroup);
            Debug("herro\n", true);
            foreach (IMyBlockGroup limbGroup in newLimbGroups)
            {
                Limb newLimb = null;
                Debug("nullInit\n");
                try
                {
                    newLimb = LimbBuilder(limbGroup);
                }
                catch
                {
                    // no change
                }
                Debug("LimbReturned\n");
                if (newLimb == null)
                    continue;

                Debug("continued\n");
                newLimb.Joints = JointDetection(limbGroup);
                newLimb.Sequences = new List<Sequence>();
                Limbs.Add(newLimb);
            }
            Debug("DetectionCompleted\n");
        }

        Limb LimbBuilder(IMyBlockGroup limbGroup)
        {
            Limb newLimb = null;
            List<IMyLandingGear> feet = new List<IMyLandingGear>();
            limbGroup.GetBlocksOfType(feet);

            string cropped = limbGroup.Name.Replace(BlockGroupSignature, String.Empty);
            string[] limbInfo = cropped.Split(':');

            string name = limbInfo[0];
            if (feet[0] == null)
                return newLimb;
            int cloneIndex = 0;
            bool mirror = false;
            //if (foot == null)
            //return newLimb;

            try
            {
                cloneIndex = int.Parse(limbInfo[2]);
            }
            catch
            {
                cloneIndex = -1;
            }
            try
            {
                mirror = (limbInfo[3] == "M") ? true : false;
            }
            catch
            {
                mirror = false;
            }

            Debug("herro\n", true);

            newLimb = new Limb(name, feet[0], cloneIndex, mirror);
            return newLimb;
        }

        List<Joint> JointDetection(IMyBlockGroup limbGroup)
        {
            List<Joint> joints = new List<Joint>();
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            limbGroup.GetBlocksOfType(blocks);
            foreach (IMyTerminalBlock block in blocks)
            {
                Joint newJoint = JointBuilder(block);
                if (newJoint == null)
                    continue;
                joints.Add(newJoint);
            }
            return joints;
        }

        Joint JointBuilder(IMyTerminalBlock block)
        {
            Joint newJoint = null;

            // Format:
            // string(name):int(index):char('M')(isMirrored?)
            string[] jointInfo = block.CustomName.Split(':');

            string name = jointInfo[0];
            int index = 0;
            bool mirror;

            try
            {
                index = int.Parse(jointInfo[1]);
            }
            catch
            {
                return newJoint;
            }
            try
            {
                mirror = (jointInfo[2] == "M") ? true : false;
            }
            catch
            {
                mirror = false;
            }
            if (block is IMyMotorStator)
            {
                if (block is IMyMotorRotor || block is IMyMotorAdvancedRotor)
                    newJoint = new Joint((IMyMotorStator)block, index, name, mirror, true);
            }
                
            if (block is IMyPistonBase)
                newJoint = new Joint((IMyPistonBase)block, index, name, mirror);

            return newJoint;
        }

        /// HELPERS //////////////////////////////

        void BlockDetection()
        {
            ThisMechController = (IMyShipController)GridTerminalSystem.GetBlockWithName(ShipControllerName);

            JointSnapshot = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("LAYOUT");
            ButtonPanel = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(ButtonPanelName);
            GUIPanel = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(GUIPanelName);

            /// Cockpit and Debugging Screens setup

            DebugPanel = (IMyTextPanel)GridTerminalSystem.GetBlockWithName(DebugPanelName);
            Debug("", false);

            if (ThisMechController != null)
            {
                CockpitScreen.Clear();
                IMyCockpit controlCockpit = (IMyCockpit)ThisMechController;
                for (int i = 0; i < 3; i++)
                    CockpitScreen.Add(controlCockpit.GetSurface(i));



                foreach (IMyTextSurface next in CockpitScreen)
                    next.ContentType = ContentType.TEXT_AND_IMAGE;
            }
        }

        void LibraryBuilder()
        {
            MainText = DefaultMainText + "\n\nStandby";
            ButtonLabels[0] = new string[] { "Info", "Library", "Detect" }; // Main Menu
            ButtonLabels[1] = new string[] { "ScrollUp", "ScrollDown", "Main Menu" }; // Info Panel
            ButtonLabels[2] = new string[] { "UpList", "DownList", "UpDirectory", "OpenDirectory", "NewItem", "DeleteItem", "MainMenu" }; // Library Menu
        }

        /// <summary>
        /// All in one animation logic handler
        /// </summary>
        void SetSequence(int index) //bool rest, 
        {
            ClockCycle = 0;
            CurrentScheduleIndex = index;
            SequenceLoaded = false;
            FrameLoaded = false;
        }
        /// <summary>
        /// For precision calls to each limb with desired Schedules. May add multiple animation libraries in the future
        /// </summary>
        /// <param name="index"></param>
        void SequenceLoader(int index)
        {
            int limbIndex = 0;
            foreach (Limb limb in Limbs)
            {
                limb.LoadSequence(SequenceSchedule[CurrentScheduleIndex, limbIndex]);
                limbIndex++;
            }
        }
        void FrameLoader(int index)
        {
            foreach (Limb limb in Limbs)
                limb.LoadFrame(CurrentFrameIndex);
        }

        /// <summary>
        /// For debugging from the cock-pit
        /// </summary>
        void ScreenUpdate()
        {
            if (CockpitScreen[0] != null)
            {
                CockpitScreen[0].WriteText("Step: " + ClockCycle + " | Schedule: " + CurrentScheduleIndex + "\n", false);
                foreach (Limb limb in Limbs)
                {
                    CockpitScreen[0].WriteText(limb.Name + ":\n", true);
                    foreach (Joint next in limb.Joints)
                    {
                        CockpitScreen[0].WriteText(next.Name + " : " + next.LoadedFrame + "\n", true);
                        CockpitScreen[0].WriteText("Direction and Angle: " + next.Direction + " : " + " : " + next.Stator.Angle * Rad2Pi + "\n", true);
                        CockpitScreen[0].WriteText("Magnitude to Target: " + next.ZBcurrent + " : " + next.AnimTarget + "\n", true);
                    }
                }
            }
        }

        void Debug(string debugMessage, bool append = true)
        {
            if (DebugPanel != null)
                DebugPanel.WriteText(debugMessage, append);
        }

        void Snapshot()
        {
            if (JointSnapshot != null)
            {
                JointSnapshot.WriteText("", false);
                foreach (Limb limb in Limbs)
                {
                    JointSnapshot.WriteText(limb.Name + "\n", true);
                    foreach (Joint joint in limb.Joints)
                        JointSnapshot.WriteText(" - " + joint.Name + " : " + joint.Stator.Angle * Rad2Pi + "\n", true);
                    JointSnapshot.WriteText("\n", true);
                }
            }
        }

        /// MAIN ////////////////////////////////

        public Program()
        {

            BlockDetection();
            LibraryBuilder();
            GUIUpdate();

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }
        public void Main(string argument, UpdateType updateSource)
        {
            /// Active animation events. On/Off states are currently handled internally.

            if (!TestMode)
            {
                InputResponse();
                FootStatusUpdate();
                InterruptManager();
                ScheduleManager();
                ClockManager();
                ScreenUpdate();
            }

            /// Run-time switches.

            switch (argument)
            {
                case "MARCH":
                    break;

                case "REVERSE":
                    break;

                case "FREEZE":
                    Freeze = !Freeze;
                    break;

                case "FEET":
                    IgnoreFeet = !IgnoreFeet;
                    break;

                case "UPDATE":
                    //Limbs = SetupLimbs();
                    break;

                case "SNAPSHOT":
                    Snapshot();
                    break;

                case "TESTMODE":
                    TestMode = !TestMode;
                    break;

                default:
                    try
                    {
                        int button = int.Parse(argument);
                        ButtonPress(button);
                    }
                    catch
                    {
                        // Was not a GUI button;
                    }
                    break;
            }
        }
        #endregion
    }
}
