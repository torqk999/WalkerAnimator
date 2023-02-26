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
        IMyCameraBlock EyeBall;
        IMyTextSurface Surface;
        IMySensorBlock Ear;
        IMyOreDetector Nose;
        StringBuilder Builder;

        void foo()
        {
            List<MyDetectedEntityInfo> pings = new List<MyDetectedEntityInfo>();
            Ear.DetectedEntities(pings);
            Builder.Clear();

            Builder.Append($"{Ear.CustomName}:{Ear.DetailedInfo}:\n\n");
            MyDetectedEntityInfo info = EyeBall.Raycast(100);
            Builder.Append($"{info.Name}:{info.Type}:{info.Relationship}:{info.EntityId}\n");

            foreach (MyDetectedEntityInfo ping in pings)
            {
                //Builder.Append($"{ping.Name}:{ping.Type}:{ping.Relationship}:{ping.EntityId}\n");
            }
            Surface.WriteText(Builder);
        }

        Program()
        {
            Builder = new StringBuilder();
            Surface = Me.GetSurface(0);
            Surface.ContentType = ContentType.TEXT_AND_IMAGE;
            Ear = (IMySensorBlock)GridTerminalSystem.GetBlockWithName("Sensor");
            Nose = (IMyOreDetector)GridTerminalSystem.GetBlockWithName("Ore Detector");
            EyeBall = (IMyCameraBlock)GridTerminalSystem.GetBlockWithName("Camera");
            EyeBall.EnableRaycast = true;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            foo();
        }
     
     */


    /*int loadCounter = 0;

            for(int set = SaveObjectIndex[(int)eRoot.JSET]; set <= JsetBin.Count; set++)
            {
                if (CheckCallLimit())
                    return 0;

                for (int foot = SaveObjectIndex[(int)eRoot.FOOT]; set <= JsetBin[set].Feet.Count; foot++)
                {
                    SaveObjectIndex[(int)eRoot.FOOT] = foot;
                    if (CheckCallLimit())
                        return 0;

                    for (int toe = SaveObjectIndex[(int)eRoot.TOE]; toe < JsetBin[set].GetFoot(foot).Toes.Count; toe++)
                    {
                        SaveObjectIndex[(int)eRoot.TOE] = toe;
                        if (CheckCallLimit())
                            return 0;

                        JsetBin[set].GetFoot(foot).Toes[toe].Save();
                    }
                    SaveObjectIndex[(int)eRoot.TOE]++;

                    for (int mag = SaveObjectIndex[(int)eRoot.MAGNET]; mag < JsetBin[set].GetFoot(foot).Magnets.Count; mag++)
                    {
                        SaveObjectIndex[(int)eRoot.MAGNET] = mag;
                        if (CheckCallLimit())
                            return 0;

                        JsetBin[set].GetFoot(foot).Magnets[mag].Save();
                    }
                    SaveObjectIndex[(int)eRoot.MAGNET]++;

                    SaveData.Append($"{JsetBin[set].Feet[foot].SaveData()}\n");
                }
                SaveObjectIndex[(int)eRoot.FOOT]++;

                for (int joint = SaveObjectIndex[(int)eRoot.JOINT]; set < JsetBin[set].Joints.Count; set++)
                {
                    SaveObjectIndex[(int)eRoot.JOINT] = joint;
                    if (CheckCallLimit())
                        return 0;

                    JsetBin[set].Joints[joint].Save();
                }
                SaveObjectIndex[(int)eRoot.JOINT]++;

                if (!SetSaved)
                {
                    SaveData.Append(JsetBin[set].SaveData());
                    SetSaved = true;
                }

                for(int seq = SaveObjectIndex[(int)eRoot.SEQUENCE]; seq < JsetBin[set].Sequences.Count; seq++)
                {
                    SaveObjectIndex[(int)eRoot.SEQUENCE] = seq;
                    if (CheckCallLimit())
                        return 0;

                    foreach (KeyFrame frame in seq.Frames)
                    {
                        foreach (JointFrame jFrame in frame.Jframes)
                        {
                            SaveData.Append(jFrame.SaveData());
                        }
                        SaveData.Append($"{frame.SaveData()}\n");
                    }

                    SaveData.Append(JsetBin[set].Sequences[seq].SaveData());
                }
                SaveObjectIndex[(int)eRoot.SEQUENCE]++;

                for (int i = 0; i < SaveObjectIndex.Length; i++)
                    SaveObjectIndex[i] = 0;

                for (int i = 0; i < SaveFootIndex.Length; i++)
                    SaveFootIndex[i] = 0;

                
                SaveData.Append($"{JointSetTag}:{set.Name}:LOAD FINISHED\n");
            }*/


    //SHA256:Gjr4HXe4MUH4BlceqwS7uN4QFotJRKzGo0VdsjjcQm4 clark_thomson2001@yahoo.com
    #region TODO
    /* Emergency override? (Threshold, Reaction Protocol) || FIXED
     * Adjustable rotor limits? (Max_Speed, Max_Accel) || BUILT
     * Piston API!!! (can't avoid it much longer Sam...) || LOW PRIORITY
     * Joint Syncing (for piston/redundant linking) || LOW PRIORITY
     * Setup distribution (limit block constructions, refer to grid manager) || HIGH PRIORITY
     * Force differentials? (Assist servo actuation, perhaps through lerp itself?) || HIGH PRIORITY
     * Custom Frame Lengths (Variable lerp time for the animation clock) || Pending...
     */
    #endregion

    /*
        %1::0:0:0
        %1::1:0:5
        %1::2:0:259
        %1::3:0:99
        %1::4:0:0
        %1::5:0:0
        %1::6:0:24
        %1::7:0:1
        %1::8:0:24
        %1::9:0:0
        %0:Frame A:0:0
        %1::0:1:0
        %1::1:1:0
        %1::2:1:314
        %1::3:1:45
        %1::4:1:0
        %1::5:1:0
        %1::6:1:0
        %1::7:1:45
        %1::8:1:45
        %1::9:1:0
        %0:Frame B:1:0
        %1::0:2:0
        %1::1:2:24
        %1::2:2:0
        %1::3:2:24
        %1::4:2:0
        %1::5:2:0
        %1::6:2:270
        %1::7:2:137
        %1::8:2:45
        %1::9:2:0
        %0:Frame C:2:0
        %1::0:3:0
        %1::1:3:335
        %1::2:3:0
        %1::3:3:334
        %1::4:3:0
        %1::5:3:0
        %1::6:3:259
        %1::7:3:99
        %1::8:3:359
        %1::9:3:0
        %0:Frame D:3:0
        %1::0:4:0
        %1::1:4:305
        %1::2:4:305
        %1::3:4:359
        %1::4:4:0
        %1::5:4:0
        %1::6:4:314
        %1::7:4:44
        %1::8:4:359
        %1::9:4:0
        %0:Frame E:4:0
        %1::0:5:0
        %1::1:5:315
        %1::2:5:222
        %1::3:5:84
        %1::4:5:0
        %1::5:5:0
        %1::6:5:335
        %1::7:5:0
        %1::8:5:335
        %1::9:5:0
        %0:Frame F:5:0
         $:walking:0:0:0.01
        */
    partial class Program : MyGridProgram
    {
        #region MAIN

        #region CONSTS
        UpdateFrequency PROG_FREQ = UpdateFrequency.None; 
        const string LCDgroupName = "LCDS";
        const string FlightGroupName = "FLIGHT";
        const string FootSignature = "[FOOT]";
        const string ToeSignature = "[TOE]";
        const string TurnSignature = "[TURN]";
        const string Digits = "0.###";

        const string OptionsTag = "&";
        const string SettingsTag = "#";
        const string FootTag = "@";
        const string SeqTag = "$";
        const string KframeTag = "%0";
        const string JframeTag = "%1";
        const string JointSetTag = "S";
        const string JointTag = "J";
        const string TurnTag = "T";
        const string PlaneTag = "P";
        const string MagnetTag = "M";
        const string GripTag = "G";

        const float VelThreshLimit = .02f;
        const float DisThreshLimit = 5f;
        const float DEG2VEL = .5f;
        const float PlaneScalar = .1f;
        const float TurnScalar = .2f;

        const float MouseIncrementMag = 0.1f;
        const float MouseSenseMin = 0.1f;
        const float MouseSenseCap = 5f;
        const float MouseSenseDef = 1f;

        const float MaxAccelIncrementMag = 0.1f;
        const float MaxAccelDef = 0.3f;
        const float MaxAccelCap = 1f;

        const float MaxSpeedIncrementMag = 0.5f;
        const float MaxSpeedDef = 10f;
        const float MaxSpeedCap = 20f;

        const float StepThreshIncrementMag = 0.05f;
        const float StepThresholdDef = 0.6f;

        const float SnapValueDef = 5;
        const float SnapValueCap = 45;

        const float ClockIncrmentMag = 0.0005f;
        const float ClockSpeedDef = 0.005f;
        const float ClockSpeedMin = 0.001f;
        const float ClockSpeedCap = 0.020f;

        const float LookScalar = 0.005f;
        const float RollScalar = 0.05f;

        const int ReleaseCount = 50;
        const int DataTransferCap = 20;

        const double RAD2DEG = 180 / Math.PI;
        const double SAFETY = Math.PI / 4;
        #endregion

        #region REFS

        IMyCockpit Control;

        List<IMyFunctionalBlock> FlightGroup = new List<IMyFunctionalBlock>();
        List<Root> JsetBin = new List<Root>();
        List<Sequence> Animations = new List<Sequence>();

        Sequence CurrentWalk;
        JointSet CurrentWalkSet;
        RootSort SORT = new RootSort();

        JointSet SetBuffer;
        List<IMyTerminalBlock> BlockBuffer;
        List<Foot> FeetBuffer = new List<Foot>();
        List<JointFrame> jFrameBuffer = new List<JointFrame>();
        List<KeyFrame> kFrameBuffer = new List<KeyFrame>();
        List<Sequence> sequenceBuffer = new List<Sequence>();

        #endregion

        #region LOGIC

        GUIMode CurrentGUIMode = GUIMode.MAIN;
        GUILayer CurrentGUILayer = GUILayer.JSET;
        //GUILayer JsetGUILayer = GUILayer.SEQUENCE;

        string SetDataBuffer;
        int LoadJointIndex;
        int LoadCustomDataIndex;
        int TransferCount = 0;
        int CursorIndex = 0;
        int CurrentSetting = 0;
        int LineBufferSize = 6;
        int[] SaveObjectIndex = new int[Enum.GetNames(typeof(eRoot)).Length];
        int[] SelectedObjectIndex = new int[Enum.GetNames(typeof(GUILayer)).Length];

        bool SavingData = false;
        bool LoadingData = true;
        bool DataInit = false;
        bool SetSaved = false;
        bool BuildingJoints = false;
        bool CapLines = true;
        bool Snapping = true;
        bool Initialized = false;
        bool Flying = true;
        bool Planeing = false;
        bool ForceSave = false;
        bool WithinTargetThreshold = false;

        public enum Option
        {
            IGNORE_SAVE,
            IGNORE_FEET,
            AUTO_DEMO,
            //AUTO_SAVE,
            STATOR_TARGET,
            STATOR_CONTROL
        }
        public bool[] Options = new bool[Enum.GetNames(typeof(Option)).Length];
        bool Check(Option op)
        {
            return Options[(int)op];
        }
        void Toggle(Option op)
        {
            Options[(int)op] = !Options[(int)op];
        }
        string SaveOptions()
        {
            string output = $"{OptionsTag}";
            foreach (bool option in Options)
                output += $":{option}";
            return output;
        }
        void LoadOptions(string input)
        {
            string[] data = input.Split(':');
            for (int i = 0; i < Options.Length; i++)
                Options[i] = bool.Parse(data[i + 1]);
        }

        public List<Setting> AllSettings;
        public List<Setting> Settings;

        Setting StepThreshold;
        Setting MaxAcceleration;
        Setting MaxSpeed;
        Setting MouseSensitivity;
        Setting SnappingValue;

        void SetupSettings()
        {
            StepThreshold = new Setting("Step Threshold", StepThresholdDef, StepThreshIncrementMag);
            MaxAcceleration = new Setting("Max Stator Acceleration", MaxAccelDef, MaxAccelIncrementMag, MaxAccelCap);
            MouseSensitivity = new Setting("Mouse Sensitivity", MouseSenseDef, MouseIncrementMag, MouseSenseCap, MouseSenseMin);
            MaxSpeed = new Setting("Max Stator Speed", MaxSpeedDef, MaxSpeedIncrementMag, MaxSpeedCap);
            SnappingValue = new Setting("Snapping Increment", SnapValueDef, 1, SnapValueCap);

            Settings = new List<Setting>
            {
                StepThreshold,
                MaxAcceleration,
                MaxSpeed,
                MouseSensitivity,
                SnappingValue,
            };

            AllSettings = new List<Setting>();

            AllSettings.AddRange(Settings);
        }
        string SaveSettings()
        {
            string output = $"{SettingsTag}";
            foreach (Setting setting in Settings)
                output += $":{setting.Current()}";
            return output;
        }
        void LoadSettings(string input)
        {
            string[] data = input.Split(':');
            for (int i = 0; i < Settings.Count; i++)
                Settings[i].Change(float.Parse(data[i + 1]));
        }

        Vector3 RotationBuffer;
        float TurnBuffer = 0;
        int LastMechInput;
        int ReleaseTimer = 0;
        int[] LastMenuInput = new int[2];

        #endregion

        #region GUI VARS
        static readonly string[] MainMenuButtons =
        {
            "Info",
            "Library",
            "Controls",
            "Options"
        };
        static readonly string[] InfoMenuButtons =
        {
            "Scroll Up",
            "Scroll Down",
            "Main Menu"
        };
        static readonly string[] CreateMenuButtons =
        {
            
            "Load Item",
            "Add Item",
            "Insert Item",
            "Delete Item",
            "Rename Item",
            "Toggle Stator Target",
            "Edit Menu",
            "Main Menu",

            /*"Up List",
            "Down List",
            "Up Directory",
            "Open Directory"*/
        };
        static readonly string[] EditorMenuButtons =
        {
            "Increment",
            "Decrement",
            "Snapping Increase",
            "Snapping Decrease",
            "Overwrite Value",
            "Set Snapping",
            "Create Menu",
            "Main Menu"
        };
        static readonly string[] LibraryMenuInputs =
        {
            "Up List",
            "Down List",
            "Up Directory",
            "Open Directory"
        };
        static readonly string[] ControlMenuButtons =
        {
            "Lock left foot",
            "Lock right foot",
            "Unlock both feet",
            "Toggle Planeing",
            "Toggle Pause",
            "Toggle Direction",
            "Zero out mech",
            "Main Menu"
        };
        static readonly string[] OptionsMenuButtons =
        {
            "Toggle ignore feet",
            "Toggle ignore save",
            "Toggle stator control",
            "Toggle stator target",
            "Toggle auto demo",
            "Main Menu",
        };
        static readonly string[] OptionMenuInputs =
        {
            "Next Setting",
            "Prev Setting",
            "Increase Setting",
            "Decrease Setting"
        };

        static readonly string[][] AllButtons =
        {
            MainMenuButtons,
            InfoMenuButtons,
            CreateMenuButtons,
            EditorMenuButtons,
            ControlMenuButtons,
            OptionsMenuButtons
        };

        static readonly string[] InputLabels =
        {
            "w",
            "s",
            "a",
            "d"
        };

        const string MainText = "Mech Control v0.5\n\n" +
            "This system requires 1 hotbar.\n" +
            "Please enter into each button\n" +
            "the following RuntimeArgument\n" +
            "for the PB:\n\n" +

            "BUTTON:n\n\n" +

            "(n >= 1 && n < 10)";

        const string InfoText = "CustomData Tolkens:\n\n" +

        ":  - Divider\n" +
        "&  - Options(&:IgnoreSave:AutoDemo:Planeing:StatorControl:StatorTarget)\n" +
        "@  - Foot(@:sInd:uInd:PadNames:GripNames:PlanarNames)\n" +
        "#  - JointSet (#:Index:GroupName:Name:IgnoreFeet)\n" +
        "$  - Sequence($:Name:LerpSpeed)\n" +
        "%0 - KeyFrame(%0:Name)\n" +
        "%1 - JointFrame(%1:LerpPoint)\n\n" +

        "pad   = T:name:sInd:uInd:fInd (requires landing gear)\n" +
        "joint = J:name:sInd:uInd:n/a  (requires joint candidate)\n" +
        "grip  = G:name:sInd:uInd:fInd (requires joint candidate)\n\n" +

        "Candidates: Rotors/Hinges (Pistons WIP!)\n\n" +

        "Must be in Control Mode to move mech\n" +
        "SnapShot the plane atleast once, and have only one foot locked\n" +
        "When ready, go to options and toggle on planeing (turn off and on if not working)\n\n" +

        "GoodLuck!";
        static readonly string[] Cursor = { "  ", "->" };
        #endregion

        #region STRING BUILDERS & SCREENS
        IMyTextSurface[] CockPitScreens = new IMyTextSurface[3];
        List<IMyTextPanel> DebugScreens = new List<IMyTextPanel>();
        StringBuilder DebugBinStream;
        StringBuilder DebugBinStatic;
        StringBuilder DisplayManagerBuilder;
        StringBuilder ButtonBuilder;
        StringBuilder SplashBuilder;
        StringBuilder SaveData;
        #endregion

        #region ENUMS
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
            CREATE,
            EDIT,
            CONTROL,
            OPTIONS
        }
        public enum eRoot
        {
            JSET,
            FOOT,
            TOE,
            MAGNET,
            JOINT,
            SEQUENCE,
            K_FRAME,
            J_FRAME
        }
        public enum GUILayer
        {
            JSET = 0,
            SEQUENCE = 1,
            K_FRAME = 2,
            J_FRAME = 3
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
        public enum Screen
        {
            DEBUG_TEST = 3,
            DEBUG_STREAM = 4,
            DEBUG_STATIC = 5,
            DIAGNOSTICS = 0,
            SPLASH = 1,
            CONTROLS = 0,
            MECH_STATUS = 2
        }
        #endregion

        #region CLASSES & STRUCTS
        public struct RootData
        {
            public string Name;
            public int MyIndex;
            public int ParentIndex;
            public Program Program;
            public bool Overwrite;

            public RootData(string name, int myIndex, int parentIndex, Program program, bool overwrite = false)
            {
                Name = name;
                MyIndex = myIndex;
                ParentIndex = parentIndex;
                Program = program;
                Overwrite = overwrite;
            }
        }
        public struct JointData
        {
            public RootData Root;
            public string TAG;
            public int FootIndex;
            public int GripDirection;

            public JointData(RootData root, string tag, int footIndex, int gripDirection)
            {
                Root = root;
                TAG = tag;
                FootIndex = footIndex;
                GripDirection = gripDirection;
            }
        }

        public class Setting
        {
            public string Name;

            float
                Value,
                Increment,
                Max, Min;

            public Setting(string name, float init, float increment, float max = 1, float min = 0)
            {
                Name = name;
                Value = init;
                Increment = increment;
                Max = max;
                Min = min;
            }

            public float Current()
            {
                return Value;
            }

            public void Adjust(bool incr)
            {
                Value += incr? Increment : -Increment;
                Clamp();
            }

            public void Change(float overwrite)
            {
                Value = overwrite;
                Clamp();
            }

            void Clamp()
            {
                Value = Value < Min ? Min : Value > Max ? Max : Value;
            }
        }
        public class Root
        {
            public Program Program;
            public RootSort MySort;
            public string Name;
            public string TAG;
            public int MyIndex;
            public int ParentIndex;
            public bool BUILT;

            public Root(string input, Program program)
            {
                Program = program;
                StaticDlog("Root Constructor:");
                MySort = Program.SORT;
                BUILT = Load(input);
            }

            public Root(RootData data)
            {
                Program = data.Program;
                MySort = Program.SORT;
                Name = data.Name;
                MyIndex = data.MyIndex;
                ParentIndex = data.ParentIndex;
                BUILT = true;
            }

            public RootData Parent(string name, int index = -1)
            {
                return new RootData(name, index, MyIndex, Program);
            }
            public void StaticDlog(string input, bool newLine = true)
            {
                Program.DebugBinStatic.Append($"{input}{(newLine ? "\n" : "")}");
                Program.Write(Screen.DEBUG_STATIC, Program.DebugBinStatic);
            }
            public void StreamDlog(string input, bool newLine = true)
            {
                Program.DebugBinStream.Append($"{input}{(newLine ? "\n" : "")}");
            }
            public virtual bool Save()
            {
                return true;
            }
            public bool Load(string input)
            {
                return Load(input.Split(':'));
            }
            public virtual bool Load(string[] data)
            {
                StaticDlog("Root Load:");

                try
                {
                    TAG = data[0];
                    Name = data[1];
                    MyIndex = int.Parse(data[2]);
                    ParentIndex = int.Parse(data[3]);
                    return true;
                }
                catch { return false; }
            }
            public virtual string SaveData()
            {
                return $"{TAG}:{Name}:{MyIndex}:{ParentIndex}";
            }
            public virtual void Insert(int index, Root root)
            {

            }
            public virtual void ReIndex()
            {

            }
            public virtual void Sort()
            {

            }
        }
        public class RootSort : Comparer<Root>
        {
            public override int Compare(Root x, Root y)
            {
                if (x != null && y != null)
                    return x.MyIndex.CompareTo(y.MyIndex);
                else
                    return 0;
            }
        }

        class Joint : Root
        {
            public int FootIndex;
            public int GripDirection;
            public IMyMechanicalConnectionBlock Connection;

            // Instance
            public double[] LerpPoints = new double[2];
            public bool Planeing = false;
            public bool Gripping = false;

            // Raw Target Values
            public double PlaneCorrection;
            public double AnimTarget;
            public double ActiveTarget;

            // Ze Maths
            public int CorrectionDir;
            public double CorrectionMag;
            public double StatorVelocity;
            public double LiteralVelocity; // Not used atm? but it works! : D
            public Vector3 PlanarDots;

            // SlerpAlerpin
            double OldVelocity;
            double LastPosition;

            public Joint(IMyMechanicalConnectionBlock mechBlock, JointData data) : base(data.Root)
            {
                Connection = mechBlock;
                Connection.Enabled = true;
                FootIndex = data.FootIndex;
                GripDirection = data.GripDirection;
                TAG = data.TAG;
            }

            public Joint(Program program, IMyMechanicalConnectionBlock mechBlock) : base(mechBlock.CustomData, program)
            {
                Connection = mechBlock;
                Connection.Enabled = true;
            }

            public override bool Save()
            {
                if (Connection == null)
                    return false;

                Connection.CustomData = SaveData();
                return true;
            }
            public override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try
                {
                    FootIndex = int.Parse(data[4]);
                    GripDirection = int.Parse(data[5]);
                    return true;
                }
                catch { return false; }
            }
            public override string SaveData()
            {
                return $"{base.SaveData()}:{FootIndex}:{GripDirection}";
            }
            public void LoadAnimationFrame(JointFrame frame, bool forward = true, bool interrupt = false)
            {
                int a = forward ? 0 : 1;
                int b = forward ? 1 : 0;

                LerpPoints[a] = interrupt ? ReturnCurrentStatorPosition() : LerpPoints[b];
                LerpPoints[b] = frame.LerpPoint;
            }
            public void OverwriteAnimTarget(double value)
            {
                AnimTarget = value;
            }

            public bool UpdateJoint(bool activeTargetTracking)
            {
                UpdateLiteralVelocity();
                if (!activeTargetTracking)
                {
                    UpdateStatorVelocity(activeTargetTracking);
                    return true;
                }

                ActiveTarget = AnimTarget;

                UpdateCorrectionDisplacement();

                if (Planeing)
                {
                    UpdatePlaneDisplacement();
                    UpdateCorrectionDisplacement();
                }

                UpdateStatorVelocity(activeTargetTracking);
                return DisThreshold();
            }
            void UpdateLiteralVelocity()
            {
                double currentPosition = ReturnCurrentStatorPosition();
                LiteralVelocity = ((currentPosition - LastPosition) / 360) / Program.Runtime.TimeSinceLastRun.TotalMinutes;
                LastPosition = currentPosition;
            }
            void UpdateStatorVelocity(bool active)
            {
                if (active)
                {
                    OldVelocity = StatorVelocity;
                    if (TAG == "G")
                    {
                        StatorVelocity = Program.MaxSpeed.Current() * (Gripping ? -1 : 1); // Needs changing!
                    }
                    else
                    {
                        double scale = CorrectionMag * DEG2VEL;
                        StatorVelocity = CorrectionDir * scale;

                        if (VelThreshold(scale))
                        //if (scale < VelThreshLimit)
                            StatorVelocity = 0;

                        StatorVelocity = (Math.Abs(StatorVelocity - OldVelocity) > Program.MaxAcceleration.Current()) ? OldVelocity + (Program.MaxAcceleration.Current() * Math.Sign(StatorVelocity - OldVelocity)) : StatorVelocity;
                        StatorVelocity = (Math.Abs(StatorVelocity) > Program.MaxSpeed.Current()) ? Program.MaxSpeed.Current() * CorrectionDir : StatorVelocity;
                    }
                }
                else
                    StatorVelocity = 0;

                UpdateStator();
            }
            public bool VelThreshold(double scale)
            {
                return scale < VelThreshLimit;
            }
            public bool DisThreshold()
            {
                return CorrectionMag < DisThreshLimit;
            }
            public void UpdatePlanarDot(MatrixD plane)
            {
                PlanarDots.X = Vector3.Dot(ReturnRotationAxis(), plane.Right);
                PlanarDots.Y = Vector3.Dot(ReturnRotationAxis(), plane.Up);
                PlanarDots.Z = Vector3.Dot(ReturnRotationAxis(), plane.Backward);
            }
            public virtual double ReturnCurrentStatorPosition()
            {
                return -100;
            }
            public virtual Vector3 ReturnRotationAxis()
            {
                return Vector3.Zero;
            }
            public virtual double ClampTargetValue(double target)
            {
                return 0;
            }

            public virtual void LerpAnimationFrame(float lerpTime)
            {
                if (lerpTime > 1 ||
                    lerpTime < 0)
                    return;
            }
            public virtual void UpdatePlaneDisplacement()
            {
                if (!Planeing)
                    return;

                //PlaneCorrection = Math.Abs(PlaneCorrection) > CorrectionMag ? PlaneCorrection - (CorrectionMag * CorrectionDir) : PlaneCorrection;
                PlaneCorrection -= (CorrectionMag * CorrectionDir);
                //PlaneCorrection = PlaneCorrection > 0 ? PlaneCorrection : 0;
                ActiveTarget += PlaneCorrection;
            }
            public virtual void UpdateCorrectionDisplacement()
            {

            }
            public virtual void UpdateStator()
            {
                Connection.SetValueFloat("Velocity", (float)StatorVelocity);
            }
        }
        class Piston : Joint
        {
            public IMyPistonBase PistonBase;
            public IMyMotorStator Reference;

            public Piston(IMyPistonBase pistonBase, JointData data) : base(pistonBase, data)
            {
                PistonBase = pistonBase;
            }
            public Piston(Program program, IMyPistonBase pistonBase) : base(program, pistonBase)
            {
                PistonBase = pistonBase;
            }

            public override double ReturnCurrentStatorPosition()
            {
                return Reference.Angle * RAD2DEG;
            }
            public override double ClampTargetValue(double target)
            {
                target = target < 0 ? 0 : target;
                target = target > 10 ? 10 : target;
                return target;
            }
            public override void LerpAnimationFrame(float lerpTime)
            {
                base.LerpAnimationFrame(lerpTime);

                AnimTarget = LerpPoints[0] + ((LerpPoints[1] - LerpPoints[0]) * lerpTime);
            }
            public override void UpdateCorrectionDisplacement()
            {
                CorrectionMag = ActiveTarget - PistonBase.CurrentPosition;
                CorrectionDir = Math.Sign(CorrectionMag);
                CorrectionMag = Math.Abs(CorrectionMag);
            }
            public override void UpdatePlaneDisplacement()
            {
                base.UpdatePlaneDisplacement();

                ActiveTarget = ActiveTarget > 10 ? 10 : ActiveTarget;
                ActiveTarget = ActiveTarget < 0 ? 0 : ActiveTarget;
            }
            public override void UpdateStator()
            {
                base.UpdateStator();

                //PistonBase.SetValueFloat("Velocity", (float)StatorVelocity);
            }
        }
        class Rotor : Joint
        {
            public IMyMotorStator Stator;

            public Rotor(IMyMotorStator stator, JointData data) : base(stator, data)
            {
                Stator = stator;
            }

            public Rotor(Program program, IMyMotorStator stator) : base(program, stator)
            {
                Stator = stator;
            }

            public override Vector3 ReturnRotationAxis()
            {
                return Stator.WorldMatrix.Down;
            }
            public override double ReturnCurrentStatorPosition()
            {
                return Stator.Angle * RAD2DEG;
            }
            public override double ClampTargetValue(double target)
            {
                target %= 360;
                target = target < 0 ? target + 360 : target;
                return target;
            }
            public override void LerpAnimationFrame(float lerpTime)
            {
                base.LerpAnimationFrame(lerpTime);

                double mag = Math.Abs(LerpPoints[0] - LerpPoints[1]);
                int dir = (mag > 180) ? Math.Sign(LerpPoints[0] - LerpPoints[1]) : Math.Sign(LerpPoints[1] - LerpPoints[0]);

                mag = mag > 180 ? 360 - mag : mag;
                mag *= (lerpTime * dir);

                //mag = mag > 180 ? 360 - mag : -mag;
                //mag *= lerpTime;

                AnimTarget = LerpPoints[0] + mag;
                AnimTarget = (AnimTarget > 360) ? AnimTarget - 360 : AnimTarget;
                //AnimTarget %= 360;
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
            public override void UpdateStator()
            {
                base.UpdateStator();

                //Stator.SetValueFloat("Velocity", (float)StatorVelocity);
            }
        }
        class Hinge : Joint
        {
            public IMyMotorStator Stator;

            public Hinge(IMyMotorStator stator, JointData data) : base(stator, data)
            {
                Stator = stator;
            }
            public Hinge(Program program, IMyMotorStator stator) : base(program, stator)
            {
                Stator = stator;
            }
            public override Vector3 ReturnRotationAxis()
            {
                return Stator.WorldMatrix.Up;
            }
            public override double ReturnCurrentStatorPosition()
            {
                return Stator.Angle * RAD2DEG;
            }
            public override double ClampTargetValue(double target)
            {
                target = target < -90 ? -90 : target;
                target = target > 90 ? 90 : target;
                return target;
            }
            public override void LerpAnimationFrame(float lerpTime)
            {
                base.LerpAnimationFrame(lerpTime);

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
            public override void UpdateStator()
            {
                base.UpdateStator();

                //Stator.SetValueFloat("Velocity", (float)StatorVelocity);
            }
        }
        
        class JointSet : Root
        {
            public string GroupName;

            public IMyTerminalBlock Plane;
            public List<Root> Feet = new List<Root>();
            public List<Root> Joints = new List<Root>();
            public List<Root> Sequences = new List<Root>();

            public MatrixD TargetPlane;
            public MatrixD TurnPlane;
            public MatrixD BufferPlane;
            public Vector3D PlaneBuffer;
            public Vector3D TurnBuffer;

            public bool StepTrigger = true;
            public int Stepping;
            public int Releasing;

            public JointSet(RootData root, IMyTerminalBlock plane, string groupName) : base(root)
            {
                TAG = JointSetTag;
                Plane = plane;
                GroupName = groupName;
            }

            public JointSet(string input, Program program, IMyTerminalBlock plane) : base(input, program)
            {
                Plane = plane;
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
            public override void Insert(int index, Root root)
            {
                if (root is Sequence)
                {
                    Sequence seq = (Sequence)root;
                    if (index >= Sequences.Count)
                    {
                        seq.MyIndex = Sequences.Count;
                        Sequences.Add(seq);
                        return;
                    }
                    Sequences.Insert(index, seq);
                    ReIndex();
                }
            }
            public override void ReIndex()
            {
                for (int i = 0; i < Joints.Count; i++)
                    Joints[i].MyIndex = i;

                for (int i = 0; i < Sequences.Count; i++)
                    Sequences[i].MyIndex = i;
            }
            public override void Sort()
            {
                Joints.Sort(MySort);
                Sequences.Sort(MySort);
            }
            public override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try
                {
                    GroupName = data[4];
                    return true;
                }
                catch { return false; }
            }
            public override string SaveData()
            {
                return $"{base.SaveData()}:{GroupName}\n";
            }

            public void UnlockFeet()
            {
                foreach (Foot foot in Feet)
                    foot.ToggleLock(false);
            }
            public bool UpdateFootLockStatus()
            {
                Foot first = null;

                foreach (Foot foot in Feet)
                    if (foot.CheckLocked())
                    {
                        first = foot;
                        break;
                    }

                if (first != null)
                    foreach (Foot foot in Feet)
                        if (foot.CheckLocked() &&
                            foot != first)
                            foot.ToggleLock(false);

                return first != null;
            }
            public bool TouchDown()
            {
                if (GetFoot(0).CheckTouching())
                {
                    InitializeGrip();
                    return true;
                }
                if (GetFoot(1).CheckTouching())
                {
                    InitializeGrip(false);
                    return true;
                }
                return false;
            }

            public bool CheckStep(float lerpTime, bool forward)
            {
                float triggerTime = forward ? lerpTime : 1 - lerpTime;

                // reset for the RS latch
                if (StepTrigger)
                {
                    if (triggerTime >= Program.StepThreshold.Current())
                        StepTrigger = false;
                    else
                        return false;
                }

                bool footCheck = false;

                // determine currently locked and checking foot
                if (GetFoot(Stepping).CheckTouching())
                    GetFoot(Stepping).ToggleLock();

                if (GetFoot(Stepping).CheckLocked())
                    footCheck = true;

                if (footCheck) // Initial contact
                {
                    StepTrigger = true;

                    if (Stepping != -1)
                    {
                        Stepping = Stepping + 1 >= Feet.Count ? 0 : Stepping + 1;
                    }

                    if (Releasing != -1)
                    {
                        GetFoot(Releasing).ToggleLock(false);
                        Releasing = Releasing + 1 >= Feet.Count ? 0 : Releasing + 1;
                    }

                    return true; // Lock successful
                }
                return false; // Lock failed
            }
            public bool InitFootStatus()
            {
                foreach (Foot foot in Feet)
                    foot.GearInit();

                if (GetFoot(0).CheckTouching() ||
                    GetFoot(0).CheckLocked())
                {
                    InitializeGrip();
                    return true;
                }
                if (GetFoot(1).CheckTouching() ||
                    GetFoot(1).CheckLocked())
                {
                    InitializeGrip(false);
                    return true;
                }
                return false;
            }
            public bool InitializeGrip(bool left = true)
            {
                try
                {
                    int unlock = left ? 1 : 0;
                    int locking = left ? 0 : 1;

                    GetFoot(locking).ToggleLock();
                    Releasing = locking;
                    GetFoot(unlock).ToggleLock(false);
                    Stepping = unlock;

                    string side = left ? "Left" : "Right";

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
                    joint.OverwriteAnimTarget(0);
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
                if (toggle)
                    SnapShotPlane();

                foreach (Foot foot in Feet)
                    foot.UpdateFootPlaneing(toggle);
            }
            void UpdatePlaneBuffer(Vector3 playerInput)
            {
                playerInput *= PlaneScalar;

                BufferPlane = MatrixD.CreateFromYawPitchRoll(playerInput.X, playerInput.Y, playerInput.Z);

                TargetPlane = MatrixD.Multiply(BufferPlane, TargetPlane);

                BufferPlane = MatrixD.Multiply(TargetPlane, MatrixD.Invert(Plane.WorldMatrix));

                MatrixD.GetEulerAnglesXYZ(ref BufferPlane, out PlaneBuffer);
            }
            void UpdateTurnBuffer(float playerTurn)
            {
                playerTurn *= TurnScalar;

                TurnBuffer.Y = playerTurn;
            }
            public bool UpdatePlanars(Vector3 playerInput, float playerTurn)
            {
                if (Plane == null)
                    return false;

                UpdatePlaneBuffer(playerInput);
                UpdateTurnBuffer(playerTurn);

                bool safety = false;
                for (int i = 0; i < 3; i++)
                    if (Math.Abs(PlaneBuffer.GetDim(i)) > SAFETY)
                    {
                        //TogglePlaneing(false);
                        SnapShotPlane();
                        safety = true;
                    }

                foreach (Foot foot in Feet)
                {
                    if (foot != null)
                    {
                        foot.GenerateAxisMagnitudes(Plane.WorldMatrix);
                        for (int i = 0; i < foot.Planars.Count; i++)
                            if (foot.Planars[i] != null)
                            {
                                if (safety)
                                {
                                    foot.GetPlanar(i).PlaneCorrection = 0;
                                    continue;
                                }

                                if (foot.Planars[i].TAG == TurnTag && !foot.Locked)
                                {
                                    foot.GetPlanar(i).PlaneCorrection = GeneratePlaneCorrection(foot.GetPlanar(i), foot.PlanarRatio, TurnBuffer);
                                }

                                else
                                {
                                    foot.GetPlanar(i).PlaneCorrection = GeneratePlaneCorrection(foot.GetPlanar(i), foot.PlanarRatio, -PlaneBuffer);
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
        class Magnet : Root
        {
            public int FootIndex;
            public IMyLandingGear Gear;
            public Magnet(RootData root, IMyLandingGear gear, int footIndex) : base(root)
            {
                TAG = MagnetTag;
                FootIndex = footIndex;
                Gear = gear;
            }
            public Magnet(Program program, IMyLandingGear gear) : base(gear.CustomData, program)
            {
                Gear = gear;
            }

            public override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try
                {
                    FootIndex = int.Parse(data[4]);
                    return true;
                }
                catch { return false; }
            }

            public override bool Save()
            {
                if (Gear == null)
                    return false;

                Gear.CustomData = SaveData();
                return true;
            }

            public override string SaveData()
            {
                return $"{base.SaveData()}:{FootIndex}";
            }

            public void GearInit()
            {
                Gear.AutoLock = false;
                Gear.Enabled = true;
            }

            public void ToggleLock(bool locking)
            {
                Gear.AutoLock = locking;
                if (locking)
                    Gear.Lock();
                else
                    Gear.Unlock();
            }

            public bool CheckTouching()
            {
                return Gear.LockMode == LandingGearMode.ReadyToLock;
            }
        }
        class Foot : Root
        {
            public List<Root> Toes = new List<Root>();
            public List<Root> Planars = new List<Root>();
            public List<Root> Magnets = new List<Root>();

            public bool Locked = false;
            public bool Planeing;
            public Vector3 PlanarRatio;

            public Foot(RootData data) : base(data)
            {
                TAG = FootTag;
            }

            public Foot(string input, Program program) : base(input, program)
            {
                StaticDlog("Foot Constructor:");
            }

            public Joint GetToe(int index)
            {
                if (index < 0 || index >= Toes.Count)
                    return null;
                return (Joint)Toes[index];
            }
            public Joint GetPlanar(int index)
            {
                if (index < 0 || index >= Planars.Count)
                    return null;
                return (Joint)Planars[index];
            }
            public Magnet GetMagnet(int index)
            {
                if (index < 0 || index >= Magnets.Count)
                    return null;
                return (Magnet)Magnets[index];
            }
            public void GearInit()
            {
                foreach (Magnet magnet in Magnets)
                    magnet.GearInit();
            }
            public void ToggleLock(bool locking = true)
            {
                foreach (Magnet magnet in Magnets)
                    magnet.ToggleLock(locking);

                Locked = locking;
                ToggleGrip(locking);
                UpdateFootPlaneing(Planeing);
            }
            public bool CheckTouching()
            {
                foreach (Magnet magnet in Magnets)
                    if (magnet.CheckTouching())
                        return true;

                return false;
            }
            public bool CheckLocked()
            {
                foreach (Magnet gear in Magnets)
                    if (gear.Gear.LockMode == LandingGearMode.Locked)
                    {
                        ToggleLock();
                        return true;
                    }

                ToggleLock(false);
                return false;
            }
            void ToggleGrip(bool gripping = true)
            {
                foreach (Joint toe in Toes)
                    toe.Gripping = gripping;
            }
            public void UpdateFootPlaneing(bool toggle)
            {
                Planeing = toggle;

                foreach (Joint plane in Planars)
                    if (plane != null)
                        plane.Planeing = (Locked || plane.TAG == TurnTag) && Planeing;
            }
            public void GenerateAxisMagnitudes(MatrixD plane)
            {
                PlanarRatio = Vector3.Zero;

                for (int i = 0; i < Planars.Count; i++)
                {
                    if (Planars[i] == null)
                        continue;

                    GetPlanar(i).UpdatePlanarDot(plane);
                    for (int j = 0; j < 3; j++)
                    {
                        PlanarRatio.SetDim(j, PlanarRatio.GetDim(j) + Math.Abs(GetPlanar(i).PlanarDots.GetDim(j)));
                    }
                }

                for (int i = 0; i < 3; i++)
                    PlanarRatio.SetDim(i, 1 / PlanarRatio.GetDim(i));
            }
        }
        class JointFrame : Root
        {
            public Joint Joint;
            public double LerpPoint;

            public JointFrame(RootData root, Joint joint, bool snapping = true) : base(root) // Snapshot
            {
                TAG = JframeTag;
                Joint = joint;
                double point = Joint.ReturnCurrentStatorPosition();
                if (snapping)
                {
                    point = (int)point;
                }
                LerpPoint = point;
            }
            public JointFrame(string input, Program program, Joint joint) : base(input, program)
            {
                StaticDlog("Jframe Constructor:");
                Joint = joint;
            }
            public void ChangeStatorLerpPoint(double value)
            {
                LerpPoint = Joint.ClampTargetValue(value);
            }
            public override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try
                {
                    LerpPoint = double.Parse(data[4]);
                    return true;
                }
                catch { return false; }
            }

            public override string SaveData()
            {
                return $"{base.SaveData()}:{LerpPoint}\n";
            }
        }
        class KeyFrame : Root
        {
            public List<Root> Jframes = new List<Root>();
            public JointFrame GetJointFrame(int index)
            {
                if (index < 0 || index >= Jframes.Count)
                    return null;
                return (JointFrame)Jframes[index];
            }
            public KeyFrame(RootData root, List<Root> jFrames = null) : base(root)
            {
                TAG = KframeTag;
                if (jFrames != null)
                    Jframes = jFrames;
            }
            public KeyFrame(string input, Program program, List<JointFrame> buffer) : base(input, program)
            {
                Jframes.AddRange(buffer);
            }

            public override void ReIndex()
            {
                Jframes.Sort(MySort);
            }
        }
        class Sequence : Root
        {
            /// EXTERNALS ///
            public List<Root> Frames = new List<Root>();
            public JointSet JointSet;
            public Setting ClockSpeed;
            public KeyFrame CurrentFrame;

            // Logic
            public ClockMode RisidualClockMode = ClockMode.FOR;
            public ClockMode CurrentClockMode = ClockMode.PAUSE;
            public float CurrentClockTime = 0;
            public int CurrentFrameIndex = 0;
            public bool bFrameLoadForward;

            public Sequence(RootData root, JointSet set = null) : base(root)
            {
                TAG = SeqTag;
                JointSet = set;
                SetClock(ClockSpeedDef);
            }
            public Sequence(string input, Program program, JointSet set, List<KeyFrame> buffer) : base(input, program)
            {
                JointSet = set;
                Frames.AddRange(buffer);
            }

            public KeyFrame GetKeyFrame(int index)
            {
                if (index < 0 || index >= Frames.Count)
                    return null;
                return (KeyFrame)Frames[index];
            }
            public override void Insert(int index, Root root)
            {
                if (root is KeyFrame)
                {
                    KeyFrame key = (KeyFrame)root;
                    if (index >= Frames.Count)
                    {
                        key.MyIndex = Frames.Count;
                        Frames.Add(key);
                        return;
                    }

                    Frames.Insert(index, key);
                    ReIndex();
                }
            }
            public override void ReIndex()
            {
                for (int i = 0; i < Frames.Count; i++)
                    Frames[i].MyIndex = i;
            }
            void SetClock(float init)
            {
                ClockSpeed = new Setting("Clock Speed", init, ClockIncrmentMag, ClockSpeedCap, ClockSpeedMin);
            }
            public override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                StaticDlog("Seq Load:");

                try
                {
                    SetClock(float.Parse(data[4]));
                    return true;
                }
                catch { return false; }
            }
            public override string SaveData()
            {
                return $"{base.SaveData()}:{ClockSpeed.Current()}\n";
            }
            public void ZeroSequence()
            {
                LoadFrame(0, true, false);
                RisidualClockMode = CurrentClockMode;
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

            public bool InitializeSeq()
            {
                if (Frames.Count == 0)
                    return false;

                CurrentFrame = GetKeyFrame(0);

                foreach (JointFrame jFrame in CurrentFrame.Jframes)
                {
                    jFrame.Joint.LoadAnimationFrame(jFrame);
                }

                return true;
            }
            public bool DemoKeyFrame(int index)
            {
                StaticDlog($"Demo key frame: {index}");

                if (index < 0 ||
                    index >= Frames.Count)
                    return false;

                StaticDlog($"Good Index");

                foreach (JointFrame jFrame in GetKeyFrame(index).Jframes)
                {
                    StaticDlog($"Joint: {jFrame.Joint != null}");
                    jFrame.Joint.OverwriteAnimTarget(jFrame.LerpPoint);
                }
                    

                return true;
            }
            public bool UpdateSequence(bool ignoreFeet = true)
            {
                if (CurrentFrame == null ||
                    CurrentClockMode == ClockMode.PAUSE)
                    return false;

                UpdateTriggers(ignoreFeet);
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

                KeyFrame newKFrame = new KeyFrame(Parent(name, index));

                for (int i = 0; i < JointSet.Joints.Count; i++)
                {
                    RootData jfRoot = newKFrame.Parent(JointSet.Joints[i].Name, i);
                    newKFrame.Jframes.Add(new JointFrame(jfRoot, JointSet.GetJoint(i), snapping));
                }

                Insert(index, newKFrame);
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

            void UpdateTriggers(bool ignoreFeet)
            {
                bool load = false;
                bool forward = false;
                bool interrupt = false;

                switch (CurrentClockMode)
                {
                    case ClockMode.PAUSE:

                        return;

                    case ClockMode.FOR:
                        forward = true;
                        CurrentClockTime += ClockSpeed.Current();
                        CurrentClockTime = CurrentClockTime < 1 ? CurrentClockTime : 1;
                        if ((CurrentClockTime == 1 && Program.WithinTargetThreshold)||
                            (!ignoreFeet && JointSet.CheckStep(CurrentClockTime, forward)))
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
                        break;

                    case ClockMode.REV:
                        forward = false;
                        CurrentClockTime -= ClockSpeed.Current();
                        CurrentClockTime = CurrentClockTime > 0 ? CurrentClockTime : 0;
                        if ((CurrentClockTime == 0 && Program.WithinTargetThreshold) ||
                            (!ignoreFeet && JointSet.CheckStep(CurrentClockTime, forward)))
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
                        break;
                }

                if (!load)
                    return;

                LoadFrame(CurrentFrameIndex, forward, interrupt);
            }
            void LoadFrame(int index, bool forward, bool interrupt)
            {
                if (JointSet == null)
                    return;

                if (index >= Frames.Count ||
                    index < 0 ||
                    Frames.Count == 0)
                    return;

                CurrentFrame = GetKeyFrame(index);
                CurrentClockTime = forward ? 0 : 1;

                foreach (JointFrame jFrame in CurrentFrame.Jframes)
                {
                    if (jFrame.Joint == null)
                        continue;

                    jFrame.Joint.LoadAnimationFrame(jFrame, forward, interrupt);
                }

                bFrameLoadForward = forward;
            }
            void LerpFrame(float lerpTime)
            {
                foreach (JointFrame joint in CurrentFrame.Jframes)
                {
                    if (joint.Joint == null)
                        continue;

                    joint.Joint.LerpAnimationFrame(lerpTime);
                }
            }
        }
        #endregion

        #region CONSTRUCTIONS
        void AssignFlightGroup()
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(FlightGroupName);
            if (group == null)
                return;

            group.GetBlocksOfType(FlightGroup);
        }
        void AssignController()
        {
            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType(cockpits);
            if (cockpits.Count > 0)
                Control = cockpits[0];
        }
        JointSet LoadSetBlockGroup(string input, IMyTerminalBlock plane, List<Foot> footBuffer)
        {
            Echo("Load joint set");
            JointSet newSet = new JointSet(input, this, plane);
            if (!newSet.BUILT)
                return null;

            if (newSet.GroupName == null)
                return null;

            BlockBuffer = BlockGroupGetter(newSet.GroupName);
            if (BlockBuffer == null)
                return null;

            newSet.Feet.AddRange(footBuffer);

            Echo("Loaded");
            return newSet;
        }

        bool LoadJoints()
        {
            if (SetBuffer == null)
            {
                SetBuffer = LoadSetBlockGroup(SetDataBuffer, Control, FeetBuffer);

                if (SetBuffer == null ||
                BlockBuffer == null)
                {
                    Write(Screen.DEBUG_STATIC, "Set load failed!\n");
                    return true;
                }

                if (BlockBuffer.Count < 1)
                {
                    Write(Screen.DEBUG_STATIC, "Nothing to load!\n");
                    return true;
                }
            }

            for (int i = LoadJointIndex; i < BlockBuffer.Count ; i++)
            {
                Echo($"Buffering:{i}");
                LoadJointIndex = i;
                if (CheckCallLimit())
                    return false;

                if (BlockBuffer[i] is IMyLandingGear)
                {
                    LoadMagnet(SetBuffer, (IMyLandingGear)BlockBuffer[i]);
                }

                if (BlockBuffer[i] is IMyPistonBase ||
                    BlockBuffer[i] is IMyMotorStator)
                {
                    LoadJoint(SetBuffer, (IMyMechanicalConnectionBlock)BlockBuffer[i]);
                }
            }

            SetBuffer.Sort();
            return true;
        }

        JointSet NewJointSet(string groupName, int index)
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            List<IMyBlockGroup> feet = new List<IMyBlockGroup>();
            IMyBlockGroup fullSet = null;
            GridTerminalSystem.GetBlockGroups(groups);

            foreach (IMyBlockGroup group in groups)
            {
                if (group.Name == groupName)
                    fullSet = group;

                else if (group.Name.Contains(FootSignature))
                    feet.Add(group);
            }

            if (fullSet == null)
                return null;

            RootData setRoot = new RootData(groupName, index, -1, this);
            JointSet newSet = new JointSet(setRoot, Control, groupName);
            List<IMyTerminalBlock> joints = BlockGroupGetter(fullSet);
            int jointIndex = 0;
            List<IMyTerminalBlock> footBuffer;
            RootData jRoot;
            JointData jData = new JointData();

            for (int f = 0; f < feet.Count; f++)
            {
                footBuffer = BlockGroupGetter(feet[f]);
                if (footBuffer.Count < 1)
                    continue;

                Foot newFoot = new Foot(newSet.Parent(feet[f].Name, f));
                newSet.Feet.Add(newFoot);
                jData.FootIndex = f;
                int toeIndex = 0;

                for (int b = 0; b < footBuffer.Count; b++)
                {
                    joints.Remove(footBuffer[b]); // Remove redundancies

                    bool toe = footBuffer[b].CustomName.Contains(ToeSignature);
                    bool turn = footBuffer[b].CustomName.Contains(TurnSignature);

                    jRoot = newSet.Parent(footBuffer[b].CustomName);
                    DebugBinStatic.Append($"Name: {jRoot.Name}\n");

                    if (footBuffer[b] is IMyMechanicalConnectionBlock)
                    {
                        jRoot.MyIndex = toe ? toeIndex : jointIndex;
                        jRoot.Name = $"[{(toe ? "GRIPPER" : turn ? "TURN" : "PLANE")}]";
                        jData.Root = jRoot;
                        jData.TAG = toe ? GripTag : turn ? TurnTag : PlaneTag;

                        DebugBinStatic.Append($"FootTag: {jData.TAG}\n");

                        jData.GripDirection = toe ? Math.Sign(footBuffer[b].GetValueFloat("Velocity")) : 0;

                        BuildJoint(newSet, (IMyMechanicalConnectionBlock)footBuffer[b], jData);

                        if (toe)
                            toeIndex++;
                        else
                            jointIndex++;
                    }
                    if (footBuffer[b] is IMyLandingGear)
                    {
                        jRoot.MyIndex = newFoot.Magnets.Count;
                        jRoot.Name = "[MAGNET]";
                        BuildMagnet(newSet, (IMyLandingGear)footBuffer[b], jRoot, f);
                    }
                }
            }

            jData.FootIndex = -1;
            jData.GripDirection = 0;

            for (int j = 0; j < joints.Count; j++)
            {
                if (joints[j] is IMyMechanicalConnectionBlock)
                {
                    jRoot = newSet.Parent("[JOINT]", jointIndex);
                    jData.Root = jRoot;
                    jData.TAG = JointTag;

                    DebugBinStatic.Append($"GenericTag: {jData.TAG}\n");

                    jointIndex++;
                    newSet.Joints.Add(JointBuilder((IMyMechanicalConnectionBlock)joints[j], jData));
                }
            }
            return newSet;
        }
        List<IMyTerminalBlock> BlockGroupGetter(string groupName)
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(groupName);
            if (group == null)
                return null;

            return BlockGroupGetter(group);
        }
        List<IMyTerminalBlock> BlockGroupGetter(IMyBlockGroup group)
        {

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);
            return blocks;
        }
        void LoadMagnet(JointSet set, IMyLandingGear gear)
        {
            if (gear == null)
                return;

            Magnet newMagnet = new Magnet(this, gear);
            if (!newMagnet.BUILT)
                return;

            Foot foot = newMagnet.FootIndex < 0 || newMagnet.FootIndex >= set.Feet.Count ? null : set.GetFoot(newMagnet.FootIndex);
            if (foot == null)
                return;

            foot.Magnets.Add(newMagnet);
        }
        void BuildMagnet(JointSet set, IMyLandingGear gear, RootData data, int footIndex)
        {
            if (gear == null)
                return;

            Foot foot = footIndex < 0 || footIndex >= set.Feet.Count ? null : set.GetFoot(footIndex);
            if (foot == null)
                return;

            Magnet newMagnet = new Magnet(data, gear, footIndex);

            foot.Magnets.Add(newMagnet);
        }
        void LoadJoint(JointSet set, IMyMechanicalConnectionBlock jointBlock)
        {
            if (jointBlock == null)
                return;

            Joint newJoint = JointLoader(jointBlock);
            if (!newJoint.BUILT)
                return;

            Foot foot = newJoint.FootIndex < 0 || newJoint.FootIndex >= set.Feet.Count ? null : set.GetFoot(newJoint.FootIndex);

            AppendJoint(set, foot, newJoint);
        }
        void BuildJoint(JointSet set, IMyMechanicalConnectionBlock jointBlock, JointData data)
        {
            if (jointBlock == null)
                return;

            Foot foot = data.FootIndex < 0 || data.FootIndex >= set.Feet.Count ? null : set.GetFoot(data.FootIndex);
            Joint newJoint = JointBuilder(jointBlock, data);

            AppendJoint(set, foot, newJoint);
        }
        void AppendJoint(JointSet set, Foot foot, Joint joint)
        {
            if (joint.TAG != GripTag)
                set.Joints.Add(joint);

            if (foot != null)
            {
                if (joint.TAG == GripTag)
                    foot.Toes.Add(joint);
                if (joint.TAG == PlaneTag || joint.TAG == TurnTag)
                    foot.Planars.Add(joint);
            }
        }
        Joint JointLoader(IMyMechanicalConnectionBlock jointBlock)
        {
            if (jointBlock is IMyPistonBase)
                return new Piston(this, (IMyPistonBase)jointBlock);

            if (!(jointBlock is IMyMotorStator))
                return null;

            if (jointBlock.BlockDefinition.ToString().Contains("Hinge"))
                return new Hinge(this, (IMyMotorStator)jointBlock);

            return new Rotor(this, (IMyMotorStator)jointBlock);
        }
        Joint JointBuilder(IMyMechanicalConnectionBlock jointBlock, JointData data)
        {
            if (jointBlock is IMyPistonBase)
                return new Piston((IMyPistonBase)jointBlock, data);

            if (!(jointBlock is IMyMotorStator))
                return null;

            if (jointBlock.BlockDefinition.ToString().Contains("Hinge"))
                return new Hinge((IMyMotorStator)jointBlock, data);

            return new Rotor((IMyMotorStator)jointBlock, data);
        }
        #endregion

        #region GUI METHODS

        public static string MatrixToString(MatrixD matrix, string digits)
        {
            return
                $"R:{matrix.Right.X.ToString(digits)}|{matrix.Right.Y.ToString(digits)}|{matrix.Right.Z.ToString(digits)}\n" +
                $"U:{matrix.Up.X.ToString(digits)}|{matrix.Up.Y.ToString(digits)}|{matrix.Up.Z.ToString(digits)}\n" +
                $"F:{matrix.Forward.X.ToString(digits)}|{matrix.Forward.Y.ToString(digits)}|{matrix.Forward.Z.ToString(digits)}\n";
        }

        void AppendLibraryItem(GUILayer layer, int index, List<string> rawStrings, string itemName)
        {
            int cursor;
            if (CurrentGUILayer == layer &&
                SelectedObjectIndex[(int)layer] == index)
            {
                CursorIndex = rawStrings.Count - 1;
                cursor = 1;
            }
            else
                cursor = 0;

            DisplayManagerBuilder.Clear();

            DisplayManagerBuilder.Append(Cursor[cursor]);

            for (int i = 0; i < (int)layer; i++)
                DisplayManagerBuilder.Append(" ");

            DisplayManagerBuilder.Append("|" + index + ":" + itemName);
            switch (layer)
            {
                case GUILayer.J_FRAME:
                    try
                    {
                        DisplayManagerBuilder.Append($"[{GetSelectedSet().Joints[index].TAG}]");
                    }
                    catch { }
                    break;

                case GUILayer.JSET:
                    try
                    {
                        DisplayManagerBuilder.Append($"{(JsetBin[SelectedObjectIndex[0]] == CurrentWalkSet ? ":[LOADED]" : "")}");
                    }
                    catch { }
                    break;
            }


            rawStrings.Add(DisplayManagerBuilder.ToString());
        }
        string[] LibraryStringBuilder()
        {
            List<string> stringList = new List<string>();
            stringList.Add("======Library======");
            stringList.Add($"===(Snapping:{SnappingValue.Current()})===");
            int layer = (int)CurrentGUILayer;

            try
            {
                if (JsetBin.Count == 0)
                {
                    stringList.Add("No limbs loaded!\n");
                }

                for (int jSetIndex = 0; jSetIndex < JsetBin.Count; jSetIndex++)
                {
                    AppendLibraryItem(GUILayer.JSET, jSetIndex, stringList, JsetBin[jSetIndex].Name);

                    if (layer < 1 || Selected(GUILayer.JSET) != jSetIndex)
                        continue;

                    JointSet set = GetJointSet(jSetIndex);
                    if (set.Sequences.Count == 0)
                        stringList.Add(" No sequences found!");

                    for (int seqIndex = 0; seqIndex < set.Sequences.Count; seqIndex++)
                    {
                        AppendLibraryItem(GUILayer.SEQUENCE, seqIndex, stringList, set.Sequences[seqIndex].Name);

                        if (layer < 2 || Selected(GUILayer.SEQUENCE) != seqIndex)
                            continue;

                        Sequence seq = set.GetSequence(seqIndex);
                        if (seq.Frames.Count == 0)
                            stringList.Add("  No frames found!");

                        for (int kFrameIndex = 0; kFrameIndex < seq.Frames.Count; kFrameIndex++)
                        {
                            AppendLibraryItem(GUILayer.K_FRAME, kFrameIndex, stringList, seq.Frames[kFrameIndex].Name);

                            if (layer < 3 || Selected(GUILayer.K_FRAME) != kFrameIndex)
                                continue;

                            if (set.Joints.Count == 0)
                                stringList.Add("   No joints found!");

                            for (int jFrameIndex = 0; jFrameIndex < seq.GetKeyFrame(kFrameIndex).Jframes.Count(); jFrameIndex++)
                            {
                                JointFrame jFrame = seq.GetKeyFrame(kFrameIndex).GetJointFrame(jFrameIndex);
                                AppendLibraryItem(GUILayer.J_FRAME, jFrameIndex, stringList,$"{jFrame.Joint.Connection.CustomName}:{jFrame.LerpPoint}");
                            }
                        }
                    }
                }
            }
            catch
            {
                stringList.Add("FAIL POINT!\n");
            }

            string[] output = stringList.ToArray();
            return output;
        }
        string[] MainStringBuilder(bool main = true)
        {
            string input = main ? MainText : InfoText;
            string[] output = input.Split('\n');
            return output;
        }
        string[] OptionsStringBuilder()
        {
            List<string> stringList = new List<string>();
            stringList.Add("===Options===");
            stringList.Add("=============");

            try
            {
                stringList.Add($"Ignoring Save: {Options[(int)Option.IGNORE_SAVE]}");
                stringList.Add($"Ignoring Feet: {Options[(int)Option.IGNORE_FEET]}");
                stringList.Add($"Auto demo: {Options[(int)Option.AUTO_DEMO]}");
                stringList.Add($"Stator control: {Options[(int)Option.STATOR_CONTROL]}");
                stringList.Add($"Stator target: {Options[(int)Option.STATOR_TARGET]}\n" +
                    "==========================");
                if (AllSettings[CurrentSetting] == null)
                    stringList.Add("No active Setting");
                else
                {
                    stringList.Add($"Current Setting: {AllSettings[CurrentSetting].Name}");
                    stringList.Add($"Current Value: {AllSettings[CurrentSetting].Current()}");
                }
            }
            catch
            {
                stringList.Add("Error!");
            }

            string[] output = stringList.ToArray();
            return output;
        }
        string[] ControlStringBuilder()
        {
            List<string> stringList = new List<string>();
            stringList.Add("===Controls===");
            stringList.Add("==============");

            try
            {
                stringList.Add($"Walk-state: {CurrentWalk.CurrentClockMode}");
                stringList.Add($"Frame index: {CurrentWalk.CurrentFrameIndex}");
                stringList.Add($"Clock Time: {CurrentWalk.CurrentClockTime}");
                stringList.Add($"Left foot locked: {CurrentWalkSet.GetFoot(0).Locked}");
                stringList.Add($"Right foot locked: {CurrentWalkSet.GetFoot(1).Locked}");
            }
            catch
            {
                stringList.Add("Error!");
            }

            string[] output = stringList.ToArray();
            return output;
        }
        string ButtonStringBuilder(GUIMode mode)
        {
            DisplayManagerBuilder.Clear();

            for (int i = 0; i < AllButtons[(int)mode].Length; i++)
            {
                DisplayManagerBuilder.Append($"{i + 1} - {AllButtons[(int)mode][i]}\n");
            }

            if (mode == GUIMode.CREATE ||
                mode == GUIMode.EDIT)
                for (int i = 0; i < LibraryMenuInputs.Length; i++)
                {
                    DisplayManagerBuilder.Append($"{InputLabels[i]} - {LibraryMenuInputs[i]}\n");
                }

            if(mode == GUIMode.OPTIONS)
                for (int i = 0; i < OptionMenuInputs.Length; i++)
                {
                    DisplayManagerBuilder.Append($"{InputLabels[i]} - {OptionMenuInputs}\n");
                }

            return DisplayManagerBuilder.ToString();
        }
        string FormattedSplashStringBuilder(string[] input)
        {
            string output = "";
            int startIndex = CursorIndex - LineBufferSize;
            startIndex = startIndex < 2 ? 2 : startIndex;

            try
            {
                output += input[0] + "\n";
                output += input[1] + "\n";
            }
            catch
            {
                return output;
            }

            if (!CapLines || CurrentGUIMode != GUIMode.CREATE)
                for (int i = 2; i < input.Length; i++)
                    output += input[i] + "\n";
            else
                for (int i = startIndex; i < startIndex + (2 * LineBufferSize) && i < input.Length; i++)
                    output += input[i] + "\n";

            return output;
        }

        void OptionsMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    Toggle(Option.IGNORE_FEET);
                    break;

                case 2:
                    Toggle(Option.IGNORE_SAVE);
                    ForceSave = true;
                    Save();
                    ForceSave = false;
                    break;

                case 3:
                    Toggle(Option.STATOR_CONTROL);
                    break;

                case 4:
                    Toggle(Option.STATOR_TARGET);
                    break;

                case 5:
                    Toggle(Option.AUTO_DEMO);
                    break;

                case 6:
                    CurrentGUIMode = GUIMode.MAIN;
                    break;
            }
        }
        void ControlMenuFunctions(int button)
        {
            if (button == 8)
            {
                CurrentGUIMode = GUIMode.MAIN;
                return;
            }

            if (CurrentWalkSet == null ||
                CurrentWalk == null)
                return;

            switch (button)
            {
                case 1:
                    CurrentWalkSet.InitializeGrip();
                    break;

                case 2:
                    CurrentWalkSet.InitializeGrip(false);
                    break;

                case 3:
                    ReleaseTimer = ReleaseCount;
                    Planeing = false;
                    CurrentWalkSet.TogglePlaneing(Planeing);
                    CurrentWalkSet.UnlockFeet();
                    break;

                case 4:
                    Planeing = !Planeing;
                    CurrentWalkSet.TogglePlaneing(Planeing);
                    break;

                case 5:
                    CurrentWalk.ToggleClockPause();
                    break;

                case 6:
                    CurrentWalk.ToggleClockDirection();
                    break;

                case 7:
                    CurrentWalkSet.ZeroJointSet();
                    break;
            }
        }
        void MainMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    CurrentGUIMode = GUIMode.INFO;
                    break;

                case 2:
                    CurrentGUIMode = GUIMode.CREATE;
                    break;

                case 3:
                    CurrentGUIMode = GUIMode.CONTROL;
                    break;

                case 4:
                    CurrentGUIMode = GUIMode.OPTIONS;
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
        void CreateMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    LoadItem();
                    break;

                case 2:
                    InsertItem();
                    break;

                case 3:
                    InsertItem(false); //add
                    break;

                case 4:
                    DeleteItem();
                    break;

                case 5:
                    EditItem();
                    break;

                case 6:
                    Toggle(Option.STATOR_TARGET);
                    break;

                case 7:
                    CurrentGUIMode = GUIMode.EDIT;
                    break;

                case 8:
                    CurrentGUIMode = GUIMode.MAIN;
                    break;
            }
        }
        void EditMenuFunctions(int button)
        {
            switch(button)
            {
                case 1:
                    IncrementLerpPoint(true);
                    break;

                case 2:
                    IncrementLerpPoint(false);
                    break;

                case 3:
                    SnappingValue.Adjust(true);
                    break;

                case 4:
                    SnappingValue.Adjust(false);
                    break;

                case 5:
                    ChangeSnappingValue();
                    break;

                case 6:
                    EditItem();
                    break;

                case 7:
                    CurrentGUIMode = GUIMode.CREATE;
                    break;

                case 8:
                    CurrentGUIMode = GUIMode.MAIN;
                    break;
            }
        }

        void ButtonPress(int button)
        {
            DebugBinStatic.Append($"Button:{button}\n");
            switch (CurrentGUIMode)
            {
                case GUIMode.MAIN:
                    MainMenuFunctions(button);
                    break;

                case GUIMode.INFO:
                    InfoMenuFunctions(button);
                    break;

                case GUIMode.CREATE:
                    CreateMenuFunctions(button);
                    break;

                case GUIMode.EDIT:
                    EditMenuFunctions(button);
                    break;

                case GUIMode.CONTROL:
                    ControlMenuFunctions(button);
                    break;

                case GUIMode.OPTIONS:
                    OptionsMenuFunctions(button);
                    break;
            }
            GUIUpdate();
            Write(Screen.DEBUG_STATIC, DebugBinStatic);
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
                    RefreshSelection(-1);
                    break;
                case GUINav.DOWN:
                    RefreshSelection(1);
                    break;
                case GUINav.BACK:
                    ChangeGUILayer(false);
                    break;
                case GUINav.SELECT:
                    ChangeGUILayer(true);
                    break;
            }
            GUIUpdate();
        }
        void ChangeGUILayer(bool up)
        {
            int layer = (int)CurrentGUILayer;
            if (up)
                layer += ((int)CurrentGUILayer == 3) ? 0 : 1;
            else
                layer -= ((int)CurrentGUILayer == 0) ? 0 : 1;
            CurrentGUILayer = (GUILayer)layer;
            RefreshSelection();
        }
        void RefreshSelection(int incr = 0)
        {
            int[] counts = new int[4];
            JointSet limb = null;
            Sequence seq = null;
            KeyFrame frame = null;

            if (JsetBin.Count > SelectedObjectIndex[0])
                limb = GetSelectedSet();

            if (limb != null && limb.Sequences.Count > Selected(GUILayer.SEQUENCE))
                seq = limb.GetSequence(Selected(GUILayer.SEQUENCE));

            if (seq != null && seq.Frames.Count > Selected(GUILayer.K_FRAME))
                frame = seq.GetKeyFrame(Selected(GUILayer.K_FRAME));

            if (limb == null && CurrentGUILayer > GUILayer.JSET)
                CurrentGUILayer = GUILayer.JSET;

            else if (seq == null && CurrentGUILayer > GUILayer.SEQUENCE)
                CurrentGUILayer = GUILayer.SEQUENCE;

            else if (frame == null && CurrentGUILayer > GUILayer.K_FRAME)
                CurrentGUILayer = GUILayer.K_FRAME;

            AdjustSelect(CurrentGUILayer, incr, GetSelectedCount());
        }
        void GUIUpdate()
        {
            ButtonBuilder.Clear();
            ButtonBuilder.Append(ButtonStringBuilder(CurrentGUIMode));

            string[] guiData = null;
            switch (CurrentGUIMode)
            {
                case GUIMode.EDIT:
                case GUIMode.CREATE:
                    guiData = LibraryStringBuilder();
                    DemoSelectedFrame();
                    break;

                case GUIMode.INFO:
                    guiData = MainStringBuilder(false);
                    break;

                case GUIMode.MAIN:
                    guiData = MainStringBuilder();
                    break;

                case GUIMode.CONTROL:
                    guiData = ControlStringBuilder();
                    break;

                case GUIMode.OPTIONS:
                    guiData = OptionsStringBuilder();
                    break;
            }

            SplashBuilder.Clear();
            SplashBuilder.Append(FormattedSplashStringBuilder(guiData));
        }
        void ChangeSetting(bool increase)
        {
            int dir = increase ? 1 : -1;
            int count = AllSettings.Count;

            Static($"Changing Setting{count}:{dir}\n");
            CurrentSetting =
                CurrentSetting + dir >= count ? 0 :
                CurrentSetting + dir < 0 ? count - 1 :
                CurrentSetting + dir;
            GUIUpdate();
        }
        void AdjustSetting(bool increase)
        {
            if (AllSettings[CurrentSetting] == null)
                return;
            AllSettings[CurrentSetting].Adjust(increase);
            GUIUpdate();
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

        void DemoSelectedFrame()
        {
            if (Check(Option.AUTO_DEMO))
            {
                Static("Demo Selected frame!\n");
                try
                {
                    GetSelectedSequence().DemoKeyFrame(Selected(GUILayer.K_FRAME));
                }
                catch
                {

                }
            }
        }
        void LoadItem()
        {
            if (CurrentGUILayer == GUILayer.J_FRAME) // do nothing
                return;

            CurrentWalkSet = GetSelectedSet();

            if (CurrentGUILayer == GUILayer.JSET)
                return;

            LoadWalk(CurrentWalkSet.GetSequence(SelectedObjectIndex[1]));

            if (CurrentGUILayer == GUILayer.SEQUENCE)
                return;

            if (CurrentWalk != null)
                CurrentWalk.DemoKeyFrame(SelectedObjectIndex[2]);
        }
        void LoadWalk(Sequence walk)
        {
            if (CurrentWalk != null)
            {
                AllSettings.Remove(CurrentWalk.ClockSpeed);
            }
            
            CurrentWalk = walk;

            if (CurrentWalk != null)
            {
                AllSettings.Add(CurrentWalk.ClockSpeed);
                CurrentWalk.InitializeSeq();
            }
        }
        void InsertSet(string name, bool add)
        {
            if (name == null)
                return;

            int index = SelectedObjectIndex[0];
            index += add ? 1 : 0;

            if (index >= JsetBin.Count)
            {
                JsetBin.Add(NewJointSet(name, JsetBin.Count));
                return;
            }

            JsetBin.Insert(index, NewJointSet(name, index));
            ReIndexSets();
        }
        void ReIndexSets()
        {
            for (int i = 0; i < JsetBin.Count; i++)
                JsetBin[i].MyIndex = i;
        }
        void EditItem()
        {
            float value = 0;
            string name = null;
            UserInputString(ref name);
            if (name != null && name.Contains(":"))
                return;
            bool floatGood = UserInputFloat(ref value);

            try
            {
                switch (CurrentGUILayer)
                {
                    case GUILayer.JSET:
                        JsetBin[SelectedObjectIndex[0]].Name = name;
                        break;

                    case GUILayer.SEQUENCE:
                        GetSelectedSequence().Name = name;
                        break;

                    case GUILayer.K_FRAME:
                        GetSelectedKeyFrame().Name = name;
                        break;

                    case GUILayer.J_FRAME:
                        /*if (floatGood)
                        {
                            JsetBin[SelectedObjectIndex[0]].GetSequence(SelectedObjectIndex[1]).GetKeyFrame(SelectedObjectIndex[2]).GetJointFrame(SelectedObjectIndex[3]).ChangeStatorLerpPoint(value);
                        }
                        else
                            JsetBin[SelectedObjectIndex[0]].Joints[SelectedObjectIndex[3]].Name = name;*/
                        break;
                }
            }
            catch
            {
                // I dunno, I'm tired bruh
            }
        }
        void InsertItem(bool add = true)
        {
            string name = null;
            int index = -1;
            UserInputString(ref name);
            JointSet set;
            Sequence seq;
            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    InsertSet(name, add);
                    break;

                case GUILayer.SEQUENCE:

                    set = GetSelectedSet();
                    index = SelectedObjectIndex[1];
                    index += add ? 1 : 0;

                    if (name == null)
                        name = $"New Sequence";

                    RootData seqRoot = set.Parent(name, index);
                    set.Insert(index, new Sequence(seqRoot, set));
                    break;

                case GUILayer.K_FRAME:

                    set = GetSelectedSet();
                    seq = set.GetSequence(SelectedObjectIndex[1]);
                    index = SelectedObjectIndex[2];
                    index += add ? 1 : 0;

                    if (name == null)
                        name = $"New Frame";

                    seq.AddKeyFrameSnapshot(index, name, Snapping);
                    break;
            }

            RefreshSelection();
            Write(Screen.DEBUG_STATIC, DebugBinStatic);
        }
        void DeleteItem()
        {
            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    JsetBin.RemoveAt(Selected(GUILayer.JSET));
                    break;

                case GUILayer.SEQUENCE:
                    GetSelectedSet().Sequences.RemoveAt(Selected(GUILayer.SEQUENCE));
                    break;

                case GUILayer.K_FRAME:
                    GetSelectedSequence().RemoveKeyFrameAtIndex(Selected(GUILayer.K_FRAME));
                    break;
            }

            int a = JsetBin.Count;

            if (SelectedObjectIndex[0] >= a && a > 0)
            {
                SelectedObjectIndex[0] = a - 1;
            }

            if (a == 0 || JsetBin[SelectedObjectIndex[0]] == null)
            {
                SelectedObjectIndex[1] = 0;
                SelectedObjectIndex[2] = 0;
                SelectedObjectIndex[3] = 0;
                return;
            }

            int b = GetSelectedSet().Sequences.Count;
            if (SelectedObjectIndex[1] >= b && b > 0)
            {
                SelectedObjectIndex[1] = b - 1;
            }

            if (b == 0 || GetSelectedSequence() == null)
            {
                SelectedObjectIndex[2] = 0;
                SelectedObjectIndex[3] = 0;
                return;
            }

            int c = GetSelectedSequence().Frames.Count;
            if (SelectedObjectIndex[2] >= c && c > 0)
            {
                SelectedObjectIndex[2] = c - 1;
            }

            if (c == 0 || GetSelectedKeyFrame() == null)
            {
                SelectedObjectIndex[3] = 0;
                return;
            }
        }
        void ChangeSnappingValue()
        {
            float input = 0;
            if (!UserInputFloat(ref input))
                return;
            SnappingValue.Change(input);
        }
        void IncrementLerpPoint(bool incr = true)
        {
            if (CurrentGUILayer != GUILayer.J_FRAME)
                return;

            JointFrame jFrame = GetSelectedJointFrame();

            double newLerpPoint = incr ? jFrame.LerpPoint + SnappingValue.Current() : jFrame.LerpPoint - SnappingValue.Current();
            jFrame.ChangeStatorLerpPoint(newLerpPoint);
        }

        int Selected(GUILayer layer)
        {
            return SelectedObjectIndex[(int)layer];
        }
        void AdjustSelect(GUILayer layer, int adjust, int count)
        {
            SetSelect(layer, Selected(layer) + adjust, count);
        }
        void SetSelect(GUILayer layer, int index, int count)
        {
            SelectedObjectIndex[(int)layer] = index >= count ? 0 : index < 0 ? count - 1 : index;
        }
        #endregion

        #region UPATES
        void ControlInput()
        {
            if (Control == null)
            {
                Echo("No Control!");
                RotationBuffer = Vector3.Zero;
                return;
            }

            int z = (int)Control.MoveIndicator.Z;
            int x = (int)Control.MoveIndicator.X;

            switch (CurrentGUIMode)
            {
                case GUIMode.CREATE:
                case GUIMode.EDIT:
                    if (LastMenuInput[0] != z)
                    {
                        LastMenuInput[0] = z;
                        if (z != 0)
                        {
                            GUINav nav = z < 0 ? GUINav.UP : GUINav.DOWN;
                            GUINavigation(nav);
                        }
                    }
                    if (LastMenuInput[1] != x)
                    {
                        LastMenuInput[1] = x;
                        if (x != 0)
                        {
                            GUINav nav = x < 0 ? GUINav.BACK : GUINav.SELECT;
                            GUINavigation(nav);
                        }
                    }
                    break;

                case GUIMode.OPTIONS:
                    if (LastMenuInput[0] != z)
                    {
                        LastMenuInput[0] = z;
                        if (z != 0)
                        {
                            Static("Z Move!\n");
                            ChangeSetting(z > 0);
                        }
                    }
                    if (LastMenuInput[1] != x)
                    {
                        LastMenuInput[1] = x;
                        if (x != 0)
                        {
                            Static("X Move!\n");
                            AdjustSetting(x > 0);
                        }
                    }
                    break;

                case GUIMode.CONTROL:

                    RotationBuffer.X = LookScalar * -Control.RotationIndicator.Y;
                    RotationBuffer.Y = LookScalar * -Control.RotationIndicator.X;

                    RotationBuffer.X = RotationBuffer.X < MouseSensitivity.Current() ? RotationBuffer.X : MouseSensitivity.Current();
                    RotationBuffer.Y = RotationBuffer.Y < MouseSensitivity.Current() ? RotationBuffer.Y : MouseSensitivity.Current();

                    RotationBuffer.Z = RollScalar * -Control.RollIndicator;
                    TurnBuffer = Control.MoveIndicator.X;

                    DebugBinStream.Append($"TurnBuffer(f): {TurnBuffer}\n");

                    switch ((int)Control.MoveIndicator.Z) //Walking
                    {
                        case 1:
                            if (LastMechInput == -1)
                                break;
                            LastMechInput = -1;
                            CurrentWalk.SetClockMode(ClockMode.REV);
                            break;

                        case 0:
                            if (LastMechInput == 0)
                                break;
                            LastMechInput = 0;
                            CurrentWalk.SetClockMode(ClockMode.PAUSE);
                            break;

                        case -1:
                            if (LastMechInput == 1)
                                break;
                            LastMechInput = 1;
                            CurrentWalk.SetClockMode(ClockMode.FOR);
                            break;
                    }
                    break;
            }
        }
        void WalkManager()
        {
            if (CurrentWalkSet == null)
                return;

            if (Planeing)
                Planeing = CurrentWalkSet.UpdatePlanars(RotationBuffer, TurnBuffer);

            if (CurrentWalk == null)
                return;

            CurrentWalk.UpdateSequence(Check(Option.IGNORE_FEET));
        }
        void AnimationManager()
        {
            if (Animations == null ||
                Animations.Count == 0)
                return;

            foreach (Sequence seq in Animations)
                seq.UpdateSequence();

        }
        void PrimarySetManager()
        {
            if (CurrentWalkSet == null ||
                !Check(Option.STATOR_CONTROL))
                return;

            WithinTargetThreshold = true;

            foreach (Joint joint in CurrentWalkSet.Joints)
            {
                if (!joint.UpdateJoint(Check(Option.STATOR_TARGET)))
                {
                    //WithinTargetThreshold = false;
                }

            }
                
            if (Check(Option.IGNORE_FEET))
                return;

            Flying = !CurrentWalkSet.UpdateFootLockStatus();
            ReleaseTimer -= 1;
            ReleaseTimer = ReleaseTimer < 0 ? 0 : ReleaseTimer;

            if (Flying && ReleaseTimer <= 0 &&
                CurrentWalkSet.TouchDown() &&
                CurrentWalkSet.UpdateFootLockStatus())
            {
                Planeing = true;
                CurrentWalkSet.TogglePlaneing(Planeing);
            }

            foreach (IMyFunctionalBlock flightBlock in FlightGroup)
                flightBlock.Enabled = Flying;

            foreach (Foot foot in CurrentWalkSet.Feet)
                foreach (Joint toe in foot.Toes)
                    toe.UpdateJoint(Check(Option.STATOR_TARGET));
        }
        void DisplayManager()
        {
            Echo($"GUIstatus: {GUIstatus(Screen.SPLASH, Screen.CONTROLS)}");
            Echo($"Diagnostics: {Diagnostics(Screen.DIAGNOSTICS)}");
            Echo($"MechStatus: {MechStatus(Screen.MECH_STATUS)}");
            Echo($"DebugStream: {DebugStream(Screen.DEBUG_STREAM)}");
        }
        bool Cockpit(Screen screen, StringBuilder input, bool append = false)
        {
            return Cockpit(screen, input.ToString(), append);
        }
        bool Cockpit(Screen screen, string input, bool append = false)
        {
            int index = (int)screen;
            if (index >= CockPitScreens.Length ||
                index < 0)
                return false;

            CockPitScreens[index].WriteText(input, append);
            return true;
        }
        bool Write(Screen screen, StringBuilder input, bool append = true)
        {
            return Write(screen, input.ToString(), append);
        }
        bool Write(Screen screen, string input, bool append = true)
        {
            int index = (int)screen;
            if (index >= DebugScreens.Count ||
                index < 0)
                return false;

            DebugScreens[index].WriteText(input, append);
            return true;
        }
        bool Static(string input, bool append = true)
        {
            if (!append)
                DebugBinStatic.Clear();
            DebugBinStatic.Append(input);
            return Write(Screen.DEBUG_STATIC, DebugBinStatic);
        }
        #endregion

        #region DISPLAY
        bool GUIstatus(Screen gui, Screen buttons)
        {
            return Cockpit(gui, SplashBuilder) && Cockpit(buttons, ButtonBuilder);
        }
        bool Diagnostics(Screen panel)
        {
            DisplayManagerBuilder.Clear();
            try
            {
                DisplayManagerBuilder.Append($"Control: {Control != null}\n");
                Vector3 B = CurrentWalkSet.PlaneBuffer;
                Vector3 T = CurrentWalkSet.TurnBuffer;
                Foot F = CurrentWalkSet.GetFoot(1);

                string derp = "hi";
                derp += " there";

                DisplayManagerBuilder.Append($"RawInput:\n{Control.RotationIndicator.Y}:{Control.RotationIndicator.X}:{Control.RollIndicator}\n");
                DisplayManagerBuilder.Append($"P_Corrections:\n{B.X:0.###}:{B.Y:0.###}:{B.Z:0.###}\n");
                DisplayManagerBuilder.Append($"T_Corrections:\n{T.X:0.###}:{T.Y:0.###}:{T.Z:0.###}\n");
                DisplayManagerBuilder.Append($"Finals:\n{F.GetPlanar(0).ActiveTarget:0.###}\n{F.GetPlanar(1).ActiveTarget:0.###}\n{F.GetPlanar(2).ActiveTarget:0.###}\n");
                DisplayManagerBuilder.Append($"Stepping: {CurrentWalkSet.Stepping}\n");
                DisplayManagerBuilder.Append($"Releasing: {CurrentWalkSet.Releasing}\n");

                for (int i = 0; i < CurrentWalkSet.Feet.Count; i++)
                {
                    DisplayManagerBuilder.Append($"Foot:{i} | Locked: {CurrentWalkSet.GetFoot(i).Locked}\n");
                }
            }
            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }

            return Write(panel, DisplayManagerBuilder);
        }
        bool MechStatus(Screen panel)
        {
            DisplayManagerBuilder.Clear();

            try
            {
                DisplayManagerBuilder.Append(
                     "Mech Status:\n\n" +
                    $"SampleWalkClockState: {CurrentWalk.CurrentClockMode}\n" +
                    $"CurrentWalkClockTime: {CurrentWalk.CurrentClockTime}\n" +
                    $"CurrentWalkFrameIndex: {CurrentWalk.CurrentFrameIndex}\n" +
                    $"Planeing: {Planeing}\n" +
                    $"Flying: {Flying}");
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }

            return Write(panel, DisplayManagerBuilder);
        }
        bool DebugStream(Screen panel)
        {
            DisplayManagerBuilder.Clear();

            try
            {
                DisplayManagerBuilder.Append(
                    ">>>>>>>>>>>>>>>>>>>\n" +
                    ">>> DebugStream >>>\n" +
                    ">>>>>>>>>>>>>>>>>>>\n");

                DisplayManagerBuilder.Append($"{DebugBinStream}");
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }
            return Write(panel, DisplayManagerBuilder);
        }
        #endregion

        #region ENTRY POINTS
        public Program()
        {
            try
            {
                DebugBinStream = new StringBuilder();
                DebugBinStatic = new StringBuilder();
                DisplayManagerBuilder = new StringBuilder();
                ButtonBuilder = new StringBuilder();
                SplashBuilder = new StringBuilder();
                SaveData = new StringBuilder();

                SetupSettings();
                AssignFlightGroup();
                AssignController();
                IMyBlockGroup panelGroup = GridTerminalSystem.GetBlockGroupWithName(LCDgroupName);
                List<IMyTextPanel> panels = new List<IMyTextPanel>();
                if (panelGroup != null)
                    panelGroup.GetBlocksOfType(panels);

                for (int i = 0; i < 3 || i < panels.Count; i++)
                {
                    if (Control != null &&
                        i < 3)
                    {
                        try
                        {
                            CockPitScreens[i] = Control.GetSurface(i);
                            CockPitScreens[i].ContentType = ContentType.TEXT_AND_IMAGE;
                            CockPitScreens[i].WriteText("");
                        }
                        catch
                        {
                            DebugBinStatic.Append("Incorrect CockpitType!\n" +
                                "MechStatus and UserInput unavailable...\n");
                        }
                    }

                    if (i < panels.Count)
                    {
                        panels[i].ContentType = ContentType.TEXT_AND_IMAGE;
                        panels[i].WriteText("");
                        DebugScreens.Add(panels[i]);
                    }
                }

                Runtime.UpdateFrequency = PROG_FREQ;
                Initialized = true;
            }
            catch
            {
                Initialized = false;
                return;
            }

            /*if (Load() == -1)
            {
                DebugBinStatic.Append("Load Failed!\n");
            }
            else
            {
                Startup();
                DebugBinStatic.Append("Load Success!\n");
            }*/

            GUIUpdate();

            Write(Screen.DEBUG_STATIC, DebugBinStatic);
        }
        public void Main(string argument, UpdateType updateSource)
        {
            if (!Initialized)
                return;

            if (BuildingJoints)
            {
                BuildingJoints = !LoadJoints();
                Write(Screen.DEBUG_STATIC, DebugBinStatic);
                return;
            }

            if (SavingData)
            {
                int saveResult = DataSave();
                SavingData = saveResult == 0;
                Write(Screen.DEBUG_STATIC, DebugBinStatic);
                return;
            }

            if (LoadingData)
            {
                int loadResult = Load();
                LoadingData = loadResult == 0;
                Write(Screen.DEBUG_STATIC, DebugBinStatic);
                return;
            }

            switch (argument)
            {
                case "SAVE_WRITE":
                    ForceSave = true;
                    Save();
                    ForceSave = false;
                    break;

                case "SAVE_DATA":
                    SavingData = true;
                    break;

                case "LOAD":
                    LoadingData = true;
                    break;

                case "CLEAR":
                    DebugBinStatic.Clear();
                    Write(Screen.DEBUG_STATIC, "", false);
                    break;

                default:
                    Echo("Default arg");
                    if (!argument.Contains("BUTTON:"))
                        break;
                    Echo("BUTTON arg");
                    string code = argument.Split(':')[1];
                    int button = 0;
                    if (int.TryParse(code, out button))
                    {
                        Echo($"BUTTON code: {button}");
                        ButtonPress(button);
                    }

                    break;
            }

            ControlInput();
            WalkManager();
            PrimarySetManager();
            DisplayManager();

            DebugBinStream.Clear(); // MUST HAPPEN!
        }
        public int Load()
        {
            JsetBin.Clear();

            string[] load = Me.CustomData.Split('\n');

            DebugBinStatic.Append($"Load Lines Length: {load.Length}\n");

            for (int i = LoadCustomDataIndex; i < load.Length; i++)
            {
                LoadCustomDataIndex = i;
                if (CheckCallLimit())
                    return 0;

                try
                {    
                    DebugBinStatic.Append("next load line...\n");
                    string[] entry = load[i].Split(':');
                    DebugBinStatic.Append($"op-code: {entry[0]}\n");

                    switch (entry[0])
                    {
                        // OPTIONS //
                        case OptionsTag:
                            DebugBinStatic.Append("options:");
                            LoadOptions(load[i]);
                            DebugBinStatic.Append(" loaded!\n");
                            break;

                        case SettingsTag:
                            DebugBinStatic.Append("Settings:");
                            LoadSettings(load[i]);
                            DebugBinStatic.Append(" loaded!\n");
                            break;

                        // BLOCKS //
                        case FootTag:
                            DebugBinStatic.Append("constructing foot...\n");
                            Foot newFoot = new Foot(load[i], this);
                            if (newFoot.BUILT)
                            {
                                FeetBuffer.Add(newFoot);
                                DebugBinStatic.Append("foot constructed!\n");
                            }
                            else
                                DebugBinStatic.Append("foot construction failed!\n");
                            break;

                        case JointSetTag:
                            if (BuildingJoints)
                            {
                                DebugBinStatic.Append("completing set...\n");
                                SetBuffer.Sequences.AddRange(sequenceBuffer);
                                FeetBuffer.Clear();
                                sequenceBuffer.Clear();
                                BuildingJoints = false;
                                break;
                            }

                            DebugBinStatic.Append("constructing set...\n");

                            BuildingJoints = true;
                            SetDataBuffer = load[i];
                            return 0;

                        // SEQUENCES //
                        case JframeTag:
                            DebugBinStatic.Append("jFrame:");
                            JointFrame newJframe = new JointFrame(load[i], this, SetBuffer.GetJoint(jFrameBuffer.Count));
                            if (newJframe.BUILT)
                            {
                                jFrameBuffer.Add(newJframe);
                                DebugBinStatic.Append(" added!:\n");
                            }
                            else
                                DebugBinStatic.Append(" failed!:\n");
                            break;

                        case KframeTag:
                            DebugBinStatic.Append("kFrame:");
                            KeyFrame newKframe = new KeyFrame(load[i], this, jFrameBuffer);
                            if (newKframe.BUILT)
                            {
                                kFrameBuffer.Add(newKframe);
                                jFrameBuffer.Clear();
                                DebugBinStatic.Append(" added!:\n");
                            }
                            else
                                DebugBinStatic.Append(" failed!:\n");
                            break;

                        case SeqTag:
                            DebugBinStatic.Append("sequence:\n");
                            Sequence newSeq = new Sequence(load[i], this, SetBuffer, kFrameBuffer);
                            if (newSeq.BUILT)
                            {
                                sequenceBuffer.Add(newSeq);
                                kFrameBuffer.Clear();
                                DebugBinStatic.Append(" added!:\n");
                            }
                            else
                                DebugBinStatic.Append(" failed!:\n");
                            break;

                        default:
                            break;
                    }
                }
                catch
                {
                    DebugBinStatic.Append("Fail Point!\n");
                    JsetBin.Clear();
                    return -1;
                }
            }

            Startup();
            return 1;
        }
        void Startup()
        {
            if (JsetBin.Count < 1 ||
                JsetBin[0] == null)
                return;

            CurrentWalkSet = GetJointSet(0);
            Planeing = CurrentWalkSet.InitFootStatus();
            CurrentWalkSet.TogglePlaneing(Planeing);

            if (CurrentWalkSet.Sequences == null ||
                CurrentWalkSet.Sequences.Count < 1)
                return;

            LoadWalk(CurrentWalkSet.GetSequence(0));
        }
        bool CheckCallLimit()
        {
            TransferCount++;
            TransferCount = TransferCount < DataTransferCap ? TransferCount : 0;
            return TransferCount == 0;
        }
        #endregion

        #region SAVING
        delegate int SaveJob();

        int DataSave()
        {
            if (!DataInit)
                SaveInit();

            return SaveStack(SaveSet, eRoot.JSET, JsetBin);
        }
        int SaveStack(SaveJob job, eRoot element, List<Root> roots)
        {
            if (Saving(element) >= roots.Count)
                return 1;

            for (int index = Saving(element); index < roots.Count; index++)
            {
                SetSave(element, index);
                if (CheckCallLimit())
                    return 0;

                int result = job();
                if (result != 1)
                    return result;
            }

            IncrementSave(element);
            return 1;
        }
        int SaveSet()
        {
            JointSet set = GetSavingSet();
            if (set == null)
                return -1;

            int footResult = SaveStack(SaveFoot, eRoot.FOOT, set.Feet);
            if (footResult != 1)
                return footResult;

            int jointResult = SaveStack(SaveJoint, eRoot.JOINT, set.Joints);
            if (jointResult != 1)
                return jointResult;

            if (!SetSaved)
            {
                SaveData.Append(set.SaveData());
                SetSaved = true;
            }

            int seqResult = SaveStack(SaveSequence, eRoot.SEQUENCE, set.Sequences);
            if (seqResult != 1)
                return seqResult;

            ResetSave(eRoot.FOOT);
            ResetSave(eRoot.JOINT);
            ResetSave(eRoot.SEQUENCE);
            SaveData.Append($"{JointSetTag}:{set.Name}:LOAD FINISHED\n");
            return 1;
        }
        int SaveFoot()
        {
            Foot foot = GetSavingFoot();
            if (foot == null)
                return -1;

            int toeResult = SaveStack(SaveToe, eRoot.TOE, foot.Toes);
            if (toeResult != 1)
                return toeResult;

            int magResult = SaveStack(SaveMag, eRoot.MAGNET, foot.Magnets);
            if (magResult != 1)
                return magResult;

            ResetSave(eRoot.TOE);
            ResetSave(eRoot.MAGNET);
            SaveData.Append($"{foot.SaveData()}\n");
            return 1;
        }
        int SaveJoint()
        {
            Joint joint = GetSavingJoint();
            return joint == null ? -1 : joint.Save() ? 1 : -1;
        }
        int SaveToe()
        {
            Joint toe = GetSavingToe();
            return toe == null ? -1 : toe.Save() ? 1 : -1;
        }
        int SaveMag()
        {
            Magnet mag = GetSavingMagnet();
            return mag == null ? -1 : mag.Save() ? 1 : -1;
        }
        int SaveSequence()
        {
            Sequence seq = GetSavingSequence();
            if (seq == null)
                return -1;

            int frameResult = SaveStack(SaveKeyFrame, eRoot.K_FRAME, seq.Frames);
            if (frameResult != 1)
                return frameResult;

            ResetSave(eRoot.K_FRAME);
            SaveData.Append(seq.SaveData());
            return 1;
        }
        int SaveKeyFrame()
        {
            KeyFrame frame = GetSavingKeyFrame();
            if (frame == null)
                return -1;

            int frameResult = SaveStack(SaveJointFrame, eRoot.J_FRAME, frame.Jframes);
            if (frameResult != 1)
                return frameResult;

            ResetSave(eRoot.J_FRAME);
            SaveData.Append(frame.SaveData());
            return 1;
        }
        int SaveJointFrame()
        {
            JointFrame frame = GetSavingJointFrame();
            if (frame == null)
                return -1;

            SaveData.Append(frame.SaveData());
            return 1;
        }
        int Saving(eRoot root)
        {
            return SaveObjectIndex[(int)root];
        }

        public void Save()
        {
            if (Check(Option.IGNORE_SAVE) && !ForceSave)
                return;

            Me.CustomData = SaveData.ToString();
        }
        void SaveInit()
        {
            SaveData.Clear();

            SaveData.Append($"{SaveOptions()}\n");
            SaveData.Append($"{SaveSettings()}\n");

            DataInit = true;
        }
        void IncrementSave(eRoot root)
        {
            SaveObjectIndex[(int)root]++;
        }
        void SetSave(eRoot root, int index)
        {
            SaveObjectIndex[(int)root] = index;
        }
        void ResetSave(eRoot root)
        {
            SaveObjectIndex[(int)root] = 0;
        }
        #endregion

        #region GETTERS
        JointSet GetJointSet(int index)
        {
            if (index < 0 || index > JsetBin.Count)
                return null;
            return (JointSet)JsetBin[index];
        }
        JointSet GetSavingSet()
        {
            return GetJointSet(Saving(eRoot.JSET));
        }
        JointSet GetSelectedSet()
        {
            return GetJointSet(Selected(GUILayer.JSET));
        }
        Sequence GetSavingSequence()
        {
            JointSet set = GetSavingSet();
            if (set == null)
                return null;
            return set.GetSequence(Saving(eRoot.JOINT));
        }
        Sequence GetSelectedSequence()
        {
            JointSet set = GetSelectedSet();
            if (set == null)
                return null;
            return set.GetSequence(Selected(GUILayer.SEQUENCE));
        }
        KeyFrame GetSavingKeyFrame()
        {
            Sequence seq = GetSavingSequence();
            if (seq == null)
                return null;
            return seq.GetKeyFrame(Saving(eRoot.K_FRAME));
        }
        KeyFrame GetSelectedKeyFrame()
        {
            Sequence seq = GetSelectedSequence();
            if (seq == null)
                return null;
            return seq.GetKeyFrame(Selected(GUILayer.K_FRAME));
        }
        JointFrame GetSavingJointFrame()
        {
            KeyFrame frame = GetSavingKeyFrame();
            if (frame == null)
                return null;
            return frame.GetJointFrame(Saving(eRoot.J_FRAME));
        }
        JointFrame GetSelectedJointFrame()
        {
            KeyFrame frame = GetSelectedKeyFrame();
            if (frame == null)
                return null;
            return frame.GetJointFrame(Selected(GUILayer.J_FRAME));
        }
        Foot GetSavingFoot()
        {
            JointSet set = GetSavingSet();
            if (set == null)
                return null;
            return set.GetFoot(Saving(eRoot.FOOT));
        }
        Joint GetSavingJoint()
        {
            JointSet set = GetSavingSet();
            if (set == null)
                return null;
            return set.GetJoint(Saving(eRoot.JOINT));
        }
        Joint GetSavingToe()
        {
            Foot foot = GetSavingFoot();
            if (foot == null)
                return null;
            return foot.GetToe(Saving(eRoot.TOE));
        }
        Magnet GetSavingMagnet()
        {
            Foot foot = GetSavingFoot();
            if (foot == null)
                return null;
            return foot.GetMagnet(Saving(eRoot.MAGNET));
        }
        int GetSelectedCount()
        {
            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    return JsetBin.Count;

                case GUILayer.SEQUENCE:
                    if (GetSelectedSet() == null)
                        return 0;
                    return GetSelectedSet().Sequences.Count;

                case GUILayer.K_FRAME:
                    if (GetSelectedSequence() == null)
                        return 0;
                    return GetSelectedSequence().Frames.Count;

                case GUILayer.J_FRAME:
                    if (GetSelectedKeyFrame() == null)
                        return 0;
                    return GetSelectedKeyFrame().Jframes.Count;

                default:
                    return 0;
            }
        }
        #endregion

        #endregion
    }
}
