using EmptyKeys.UserInterface.Generated.EditFactionIconView_Bindings;
using Sandbox.Game.Debugging;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GUI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

//using Sandbox.Game.Entities.Cube;

using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ObjectBuilders.VisualScripting;
using VRage.Sync;
using VRageMath;

namespace IngameScript
{

    #region TODO
    /* Emergency override? (Threshold, Reaction Protocol) || FIXED
     * Adjustable rotor limits? (Max_Speed, Max_Accel) || BUILT
     * Piston API!!! (can't avoid it much longer Sam...) || LOW PRIORITY
     * Joint Syncing (for piston/redundant linking) || LOW PRIORITY
     * Setup distribution (limit block constructions, refer to grid manager) || BUILT
     * Force differentials? (Assist servo actuation, perhaps through lerp itself?) || BUILT (stable)
     * Custom Frame Lengths (Variable lerp time for the animation clock) || BUILT
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
        #region USER CONSTS
        enum Screen
        {
            INPUT = 0,
            SPLASH = 1,
            CONTROLS = 2,

            DIAGNOSTICS = 5,
            MECH_STATUS = 8,
            DEBUG_TEST = 7,
            DEBUG_STREAM = 6,
            DEBUG_STATIC = 9,
        }

        const string LCDgroupName       = "LCDS";
        const string FlightGroupName    = "FLIGHT";
        const string FootSignature      = "[FOOT]";
        const string ToeSignature       = "[TOE]";
        const string TurnSignature      = "[TURN]";
        const string DroneChannel       = "MECH";
        const string PlaneCustomName    = "EYEBALL";

        #endregion

        #region INTERNAL CONSTS

        // need migrating //
        static bool CapLines = true;
        static bool Snapping = true;
        ////////////////////

        static UpdateFrequency DEF_FREQ = UpdateFrequency.Update1;

        const string VersionNumber = "0.6.1";

        const string OptionsTag = "&";
        const string SettingsTag = "#";
        const string FootTag = "@";
        const string SeqTag = "$";
        const string ZframeTag = "%Z";
        const string KframeTag = "%0";
        const string JframeTag = "%1";
        const string JointSetTag = "S";
        const string JointTag = "J";
        const string TurnTag = "T";
        const string PlaneTag = "P";
        const string MagnetTag = "M";
        const string GripTag = "G";

        const float PlaneScalar = .1f;
        const float TurnScalar = .2f;

        const float ForceMin = .6f;

        const float ClockIncrmentMag = 0.0005f;
        const float ClockSpeedDef = 0.005f;
        const float ClockSpeedMin = 0.001f;
        const float ClockSpeedCap = 0.020f;

        const float FrameIncrmentMag = 0.1f;
        const float FrameLengthDef = 1f;
        const float FrameLengthMin = 0.1f;
        const float FrameLengthCap = 10f;

        const float LookScalar = 0.005f;
        const float RollScalar = 0.05f;

        const int DroneTimeOut = 100;
        const int ReleaseCount = 50;
        const int DataTransferCap = 20;
        const int StaticDebugCharCap = 10000;

        const int LineTotalCount = 12;
        const int CharTotalCount = 30;
        const int DefaultDressingSize = 2;

        const double MIN_VEL = .1;
        const double DEG2VEL = .5;
        const double RAD2DEG = 180 / Math.PI;
        const double SAFETY = Math.PI / 4;
        #endregion

        #region RESOURCES

        #region MECHANICAL

        static List<IMyFunctionalBlock> FlightGroup = new List<IMyFunctionalBlock>();
        static List<Root> JointBin = new List<Root>();
        static List<Root> JsetBin = new List<Root>();

        #endregion

        #region OPTIONS

        static Toggle DroneOverride;

        static Toggle IgnoreSave;
        static Toggle IgnoreFeet;
        static Toggle AutoSave;
        static Toggle AutoDemo;
        static Toggle Strafing;
        static Toggle StatorTarget;
        static Toggle StatorControl;
        static Toggle Orientation;
        static Toggle Descriptions;

        static Setting StepThreshold;
        static Setting FrameThreshold;
        static Setting MaxAcceleration;
        static Setting MaxSpeed;
        static Setting MouseSensitivity;

        static Setting SnappingIncrement;
        static Setting FrameDurationIncrement;
        static Setting SequenceSpeedIncrement;

        static List<Option> Options;
        static List<Toggle> Toggles;
        static List<Setting> Settings;

        #endregion

        #region MENUS

        static LibraryPage Library = new LibraryPage();
        static TablePage Assignment = new TablePage();
        static ControlPage Controls = new ControlPage();
        static OptionsPage OptionMenu = new OptionsPage();
        static GenericPage GeneralMenu = new GenericPage();

        static Dictionary<GUIMode, Page> Pages = new Dictionary<GUIMode, Page> {
            {GUIMode.MAIN, GeneralMenu  },
            {GUIMode.INFO, GeneralMenu  },

            {GUIMode.EDIT, Library      },
            {GUIMode.CREATE, Library    },

            {GUIMode.PILOT, Controls    },
            {GUIMode.OPTIONS, OptionMenu},
            {GUIMode.ASSIGN, Assignment },
        };

        #endregion

        #endregion

        #region LOGIC VARS

        #region LOAD/SAVE

        static bool SavingData = false;
        static bool LoadingData = true;
        static bool SaveInit = false;
        static bool JointsSaved = false;
        static bool SetBuffered = false;
        static bool BuildingJoints = false;
        static bool JointsBuilt = false;

        string LoadSetDataBuffer;
        static int LoadJointIndex;
        static int LoadCustomDataIndex;
        static int TransferCount = 0;
        static int[] SaveObjectIndex = new int[Enum.GetNames(typeof(eRoot)).Length];

        static JointSet SetBuffer;
        static List<IMyTerminalBlock> ReTagBuffer = new List<IMyTerminalBlock>();
        static List<IMyTerminalBlock> BlockBuffer;
        static List<Foot> FeetBuffer = new List<Foot>();
        static List<JointFrame> jFrameBuffer = new List<JointFrame>();
        static List<KeyFrame> kFrameBuffer = new List<KeyFrame>();
        static List<Root> sequenceBuffer = new List<Root>();

        #endregion

        #region RUNTIME

        static IMyCockpit CockPit;
        static RootSort SORT = new RootSort();
        static Program PROG;

        static Sequence CurrentWalk;
        static JointSet CurrentWalkSet;
        static List<Sequence> Animations = new List<Sequence>();
        static UpdateFrequency PROG_FREQ;
        GUIMode _CurrentGUIMode;
        static Vector3D InputRotationBuffer;
        static double InputTurnBuffer = 0;

        static int MoveBuffer = 0;
        static int LastMechWalkInput;
        static int[] LastMenuInput = new int[4];

        static bool WAIT;
        static bool Initialized = false;
        //static bool EditorToggle = false;
        static bool WithinTargetThreshold = false;

        #endregion

        #region DRONE

        IMyTerminalBlock DesignatedPlane;
        IMyBroadcastListener DroneEar;
        int DroneTimeOutClock = 0;
        double[] MECH_IX_BUFFER = new double[Enum.GetValues(typeof(MechIx)).Length];
        bool DroneControlled => DroneTimeOutClock > 0;
        #endregion

        #endregion

        #region ENUMS
        enum MechAction
        {
            DEFAULT,
            WAIT,
            ZERO_OUT,
            RELEASE,
        }
        enum MechIx
        {
            ACTION,
            MOVE_Z,
            MOVE_X,
            YAW,
            PITCH,
            ROLL,
        }

        enum ClockMode
        {
            REV = -1,
            PAUSE = 0,
            FOR = 1,
        }
        enum eRoot
        {
            JSET = 0,
            SEQUENCE = 1,
            K_FRAME = 2,
            J_FRAME = 3,

            FOOT = 4,
            TOE = 5,
            MAGNET = 6,
            JOINT = 7,

            PARAM = 8,
        }
        const int ParamCount = 6;
        enum PARAM
        {
            TAG = 0,
            Name = 1,

            uIX = 2, // unique index (my)
            pIX = 3, // parent
            fIX = 4, // foot
            lIX = 5, // lock
            sIX = 6, // sync

            SettingInit = 4,
            GroupName = 4,
            GripDirection = 5,
        }

        enum GUIMode
        {
            MAIN,
            INFO,
            ASSIGN,
            CREATE,
            EDIT,
            PILOT,
            OPTIONS
        }
        public enum GUIKey
        {
            ALPHA_0,
            ALPHA_1,
            ALPHA_2,
            ALPHA_3,
            ALPHA_4,
            ALPHA_5,
            ALPHA_6,
            ALPHA_7,
            ALPHA_8,
            ALPHA_9,

            ALT_0,
            ALT_1,
            ALT_2,
            ALT_3,
            ALT_4,
            ALT_5,
            ALT_6,
            ALT_7,
            ALT_8,
            ALT_9,

            RIGHT,
            LEFT,
            UP,
            DOWN,
            BACKWARD,
            FORWARD,
            ALT_RIGHT,
            ALT_LEFT, 
        }
        #endregion

        #region DELEGATES
        public delegate int SaveJob();
        public delegate void ToggleUpdate(bool state);
        public delegate void SettingUpdate(float setting);
        public delegate string[] PageBuilder();
        public delegate void ButtonEvent();
        #endregion

        #region OBJECTS

        #region DATA
        struct RootData
        {
            public static RootData Default = new RootData("Un-named", -1, -1, true);

            public string Name;
            public int MyIndex;
            public int ParentIndex;
            public bool Overwrite;

            public RootData(string name, int myIndex, int parentIndex, bool overwrite = false)
            {
                Name = name;
                MyIndex = myIndex;
                ParentIndex = parentIndex;
                Overwrite = overwrite;
            }
        }
        struct JointData
        {
            public static JointData Default = new JointData(RootData.Default, JointTag, -1, -1, 0);

            public RootData Root;
            public string TAG;
            public int FootIndex;
            public int SyncIndex;
            public int GripDirection;

            public JointData(RootData root, string tag, int footIndex, int syncIndex, int gripDirection)
            {
                Root = root;
                TAG = tag;
                FootIndex = footIndex;
                SyncIndex = syncIndex;
                GripDirection = gripDirection;
            }
        }
        struct PageData
        {
            public static readonly PageData Default = new PageData(0, 0, DefaultDressingSize);

            public int CursorIndex;
            public int LineBufferSize;
            public int HeaderSize;

            public PageData(int cursorIndex, int lineBufferSize, int headerSize)
            {
                CursorIndex = cursorIndex;
                LineBufferSize = lineBufferSize;
                HeaderSize = headerSize;
            }
        }
        #endregion

        #region LOGIC
        class Option
        {
            public string Name;
            public string[] Description;

            public Option(string name, string describe)
            {
                Name = name;
                Description = describe.Split(' ');
            }

            public virtual string Current()
            {
                return "nothing";
            }
            public virtual void Adjust(bool main = true)
            {

            }
        }
        class Toggle : Option
        {
            bool State;
            ToggleUpdate OptionUpdate;

            public Toggle(string name, string describe, bool init, ToggleUpdate update = null) : base(name, describe)
            {
                State = init;
                OptionUpdate = update;
            }

            public override string Current()
            {
                return State.ToString();
            }
            public override void Adjust(bool flip = true)
            {
                State = !State;
                if (OptionUpdate != null)
                    OptionUpdate(State);
            }

            public bool MyState()
            {
                return State;
            }
            public void Change(bool overwrite)
            {
                State = overwrite;
            }
        }
        class Setting : Option
        {

            float
                Value, ValueMax, ValueMin,
                defaultIncr;
            Setting Incrementer;

            public Setting(string name, string describe, float init, float increment, float max = 1, float min = 0, Setting incr = null) : base(name, describe)
            {
                Value = init;
                defaultIncr = increment;
                ValueMax = max;
                ValueMin = min;
                Incrementer = incr;
                Clamp();
            }

            public override string Current()
            {
                return Value.ToString();
            }
            public override void Adjust(bool incr = true)
            {
                float delta = Incrementer == null ? defaultIncr : Incrementer.Value;
                Value += incr ? delta : -delta;
                Clamp();
                //if (OptionUpdate != null)
                //OptionUpdate(Value);
            }

            public float MyValue()
            {
                return Value;
            }
            public void Change(float overwrite)
            {
                Value = overwrite;
                Clamp();
            }

            void Clamp()
            {
                Value = Value < ValueMin ? ValueMin : Value > ValueMax ? ValueMax : Value;
            }
        }

        class Root
        {
            public RootSort MySort;
            public string Name;
            public string TAG;
            public int MyIndex;
            public int ParentIndex;
            public bool BUILT;

            public Root()
            {
                StaticDlog("Root Constructor:");
                MySort = SORT;
                BUILT = true;
            }

            public Root(RootData data)
            {
                MySort = SORT;
                Name = data.Name;
                MyIndex = data.MyIndex;
                ParentIndex = data.ParentIndex;
                BUILT = true;
            }

            public RootData ParentData(string name, int index = -1)
            {
                return new RootData(name, index, MyIndex);
            }
            public void StaticDlog(string input, bool newLine = true)
            {
                Static($"{input}{(newLine ? "\n" : "")}");
            }
            public void StreamDlog(string input, bool newLine = true)
            {
                DebugBinStream.Append($"{input}{(newLine ? "\n" : "")}");
            }

            protected bool Load(string input)//, Option option = null)
            {
                StaticDlog($"Load Root String: {input}");
                return Load(input.Split(':'));//, option);
            }
            protected virtual bool Load(string[] data)//, Option option = null)
            {
                StaticDlog("Root Load:");
                try
                {

                    TAG = data[(int)PARAM.TAG];

                    Name = data[(int)PARAM.Name];

                    MyIndex = int.Parse(data[(int)PARAM.uIX]);

                    ParentIndex = int.Parse(data[(int)PARAM.pIX]);

                    return true;
                }
                catch { return false; }
            }
            public string SaveData()
            {
                string[] buffer = new string[Enum.GetNames(typeof(PARAM)).Length];
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = "";

                saveData(buffer);

                RootDataBuilder.Clear();

                for (int i = 0; i < buffer.Length; i++)
                    RootDataBuilder.Append($"{buffer[i]}:");

                return RootDataBuilder.ToString();
            }
            protected virtual void saveData(string[] saveBuffer)
            {
                saveBuffer[(int)PARAM.TAG] = TAG;
                saveBuffer[(int)PARAM.Name] = Name;
                saveBuffer[(int)PARAM.uIX] = MyIndex.ToString();
                saveBuffer[(int)PARAM.pIX] = ParentIndex.ToString();
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
        class RootSort : Comparer<Root>
        {
            public override int Compare(Root x, Root y)
            {
                if (x != null && y != null)
                    return x.MyIndex.CompareTo(y.MyIndex);
                else
                    return 0;
            }
        }
        #endregion

        #region MENUS
        class LibraryPage : Page
        {
            public eRoot CurrentDirectoryLayer = eRoot.JSET;

            public LibraryPage() : base("Library")
            {
                Buttons = new Dictionary<GUIKey, Button>
                {
                    {GUIKey.RIGHT   ,   new Button("Enter Directory"   ,    ()=>ChangeDirectory(false)) },
                    {GUIKey.LEFT    ,   new Button("Parent Directory"  ,    ()=>ChangeDirectory(true)) },
                    {GUIKey.FORWARD ,   new Button("Up Directory"      ,    ()=>SelectElement(-1)) },
                    {GUIKey.BACKWARD,   new Button("Down Directory"    ,    ()=>SelectElement(1)) },

                    {GUIKey.ALPHA_1,    new Button("Main",                  () => SetGuiMode(GUIMode.MAIN)) },
                    {GUIKey.ALPHA_2,    new Button("Toggle T-Pose Edit",    () => {/*EditorToggle = !EditorToggle*/}) },
                    {GUIKey.ALPHA_3,    new Button("Creator",               () => SetGuiMode(GUIMode.CREATE)) },
                    {GUIKey.ALPHA_4,    new Button("Increment",             () => AdjustValue(GetSelectedAnim(),true))},
                    {GUIKey.ALPHA_5,    new Button("Decrement",             () => AdjustValue(GetSelectedAnim(),false))},
                    {GUIKey.ALPHA_6,    new Button("Overwrite",             () => EditValue(GetSelectedAnim()))},
                    {GUIKey.ALPHA_7,    new Button("Toggle Stator Control", () => StatorTarget.Adjust())},

                    {GUIKey.ALT_1,      new Button("Main",                  () => SetGuiMode(GUIMode.MAIN)) },
                    {GUIKey.ALT_2,      new Button("Toggle T-Pose Edit",    () => {/*EditorToggle = !EditorToggle*/}) },
                    {GUIKey.ALT_3,      new Button("Editor",                () => SetGuiMode(GUIMode.EDIT))},
                    {GUIKey.ALT_4,      new Button("Load",                  () => LoadItem())},
                    {GUIKey.ALT_5,      new Button("Insert",                () => InsertItem())},
                    {GUIKey.ALT_6,      new Button("Add",                   () => InsertItem(false))},
                    {GUIKey.ALT_7,      new Button("Delete",                () => {DeleteItem(); LibraryUpdate();})},
                    {GUIKey.ALT_8,      new Button("ChangeName",            () => EditName())}
                };
                SelectedIndexes = new Dictionary<eRoot, int>
                {
                    {eRoot.JSET,0},
                    {eRoot.SEQUENCE,0},
                    {eRoot.K_FRAME,0},
                    {eRoot.J_FRAME,0}
                };
            }

            public override void SetMode(GUIMode mode)
            {
                switch (mode)
                {
                    case GUIMode.CREATE:
                        Name = "Creator";
                        AlternateMode = true;
                        break;

                    case GUIMode.EDIT:
                        Name = "Editor";
                        AlternateMode = false;
                        break;

                    default:
                        Name = "Library";
                        break;
                }
            }
            protected override string[] PageBuilder()
            {
                cursorCounter = 0;
                RawBuffer.Clear();
                RawBuffer.Add($"= Library = [StatorTarget:{StatorTarget.MyState()}]=");

                Animation anim = null;
                switch (CurrentDirectoryLayer)
                {
                    case eRoot.SEQUENCE:
                        anim = GetSelectedSequence();
                        break;

                    case eRoot.K_FRAME:
                        anim = GetSelectedKeyFrame();
                        break;

                    case eRoot.J_FRAME:
                        anim = GetSelectedJointFrame();
                        break;
                }
                if (anim != null)
                {
                    RawBuffer.Add($"= {anim.MySetting.Name} : {anim.MySetting.MyValue()} =");
                    if (Descriptions.MyState())
                        LineWrapper(RawBuffer, anim.MySetting.Description, CharTotalCount);
                }
                RawBuffer.Add($"=================");

                HeaderSize = RawBuffer.Count;

                try
                {
                    if (JsetBin.Count == 0)
                    {
                        RawBuffer.Add("No limbs loaded!");
                    }

                    for (int jSetIndex = 0; jSetIndex < JsetBin.Count; jSetIndex++)
                    {
                        if (JsetBin[jSetIndex] == null)
                        {
                            RawBuffer.Add("Null Set!");
                            continue;
                        }

                        AppendLibraryItem(eRoot.JSET, jSetIndex, JsetBin[jSetIndex].Name);

                        if ((int)CurrentDirectoryLayer < 1 || SelectedIndex(eRoot.JSET) != jSetIndex)
                            continue;

                        CursorIndex = cursorCounter;
                        JointSet set = GetJointSet(jSetIndex);

                        JsetStringBuilder(set);
                    }
                }
                catch
                {
                    RawBuffer.Add("FAIL POINT!\n");
                }

                string[] output = RawBuffer.ToArray();
                return output;
            }
            public override int SelectedCount()
            {
                return SelectedCount(CurrentDirectoryLayer);
            }
            public override int SelectedCount(eRoot selection)
            {
                switch (CurrentDirectoryLayer)
                {
                    case eRoot.JSET:
                        return JsetBin.Count;

                    case eRoot.SEQUENCE:
                        if (GetSelectedSet() == null)
                            return 0;
                        return GetSelectedSet().Sequences.Count;

                    case eRoot.K_FRAME:
                        if (GetSelectedSequence() == null)
                            return 0;
                        return GetSelectedSequence().Frames.Count;

                    case eRoot.J_FRAME:
                        if (GetSelectedKeyFrame() == null)
                            return 0;
                        return GetSelectedKeyFrame().Jframes.Count;

                    default:
                        return 0;
                }
            }
            public override Root SelectedRoot()
            {
                JointSet jSet = GetJointSet(SelectedIndex(eRoot.JSET));

                if (jSet == null || CurrentDirectoryLayer == eRoot.JSET)
                    return jSet;

                Sequence sequence = jSet.GetSequence(SelectedIndex(eRoot.SEQUENCE));

                if (sequence == null || CurrentDirectoryLayer == eRoot.SEQUENCE)
                    return sequence;

                KeyFrame keyFrame = sequence.GetKeyFrame(SelectedIndex(eRoot.K_FRAME));

                if (keyFrame == null || CurrentDirectoryLayer == eRoot.K_FRAME)
                    return keyFrame;

                JointFrame jFrame = keyFrame.GetJointFrameByFrameIndex(SelectedIndex(eRoot.J_FRAME));

                return jFrame;
            }
            void SelectElement(int adjust = 0)
            {
                int count = SelectedCount();
                int index = SelectedIndex(CurrentDirectoryLayer) + adjust;
                index = index >= count ? 0 : index < 0 ? count - 1 : index;
                SelectedIndexes[CurrentDirectoryLayer] = index;
            }
            void ChangeDirectory(bool up)
            {
                int layer = (int)CurrentDirectoryLayer;
                layer += up ? -1 : 1;
                layer = layer < (int)eRoot.JSET ? (int)eRoot.J_FRAME : layer > (int)eRoot.J_FRAME ? (int)eRoot.JSET : layer;
                //layer += EditorToggle ? up ? 3 : -3 : up ? 1 : -1;
                //layer = layer > 3 ? 0 : layer < 0 ? 3 : layer;
                //
                //if (up)
                //    layer += ((int)CurrentDirectoryLayer == 3) ? 0 : 1;
                //else
                //    layer -= ((int)CurrentDirectoryLayer == 0) ? 0 : 1;
                CurrentDirectoryLayer = (eRoot)layer;
                LibraryUpdate();
                //LibrarySelection(up ? -1 : 1);
            }
            void DemoSelectedFrame()
            {
                if (AutoDemo.MyState())
                {
                    try
                    {
                        GetSelectedSequence().DemoKeyFrame(SelectedIndex(eRoot.K_FRAME));
                    }
                    catch
                    {
                        Static("Failed to demo keyFrame!\n");
                    }
                }
            }
            void LoadItem()
            {
                if (CurrentDirectoryLayer == eRoot.J_FRAME) // do nothing
                    return;

                CurrentWalkSet = GetSelectedSet();

                if (CurrentDirectoryLayer == eRoot.JSET)
                    return;

                LoadWalk(CurrentWalkSet.GetSequence(SelectedIndex(eRoot.SEQUENCE)));

                if (CurrentDirectoryLayer == eRoot.SEQUENCE)
                    return;

                if (CurrentWalk != null)
                    CurrentWalk.DemoKeyFrame(SelectedIndex(eRoot.K_FRAME));
            }

            void InsertSet(string name, bool add)
            {
                if (name == null)
                    return;

                //PROG.LibraryBuilder.Add("Inserting set...");

                int index = SelectedIndex(0);
                index += add ? 1 : 0;

                if (index >= JsetBin.Count)
                {
                    JsetBin.Add(NewJointSet(name, JsetBin.Count));
                    return;
                }

                JsetBin.Insert(index, NewJointSet(name, index));
                ReIndexSets();

                //PROG.LibraryBuilder.Add("Inserted!");
            }
            void ReIndexSets()
            {
                for (int i = 0; i < JsetBin.Count; i++)
                    JsetBin[i].MyIndex = i;
            }
            void AdjustValue(Animation anim, bool increase)
            {
                if (anim == null)
                    return;

                anim.MySetting.Adjust(increase);
            }
            void EditValue(Animation anim)
            {
                if (anim == null)
                    return;

                float value;
                if (!UserInputFloat(out value))
                    return;

                anim.MySetting.Change(value);
            }
            void EditName()
            {
                string name = null;
                UserInputString(ref name);
                if (name == null)
                    return;

                Root root = SelectedRoot();
                if (root == null)
                    return;

                root.Name = name;
            }
            void InsertItem(bool add = true)
            {
                string name = null;
                int index = -1;
                UserInputString(ref name);
                JointSet set;
                Sequence seq;
                switch (CurrentDirectoryLayer)
                {
                    case eRoot.JSET:
                        InsertSet(name, add);
                        break;

                    case eRoot.SEQUENCE:

                        set = GetSelectedSet();
                        index = SelectedIndex(eRoot.SEQUENCE);
                        index += add ? 1 : 0;

                        if (name == null)
                            name = $"New Sequence";

                        RootData seqRoot = set.ParentData(name, index);
                        set.Insert(index, new Sequence(seqRoot, set));
                        break;

                    case eRoot.K_FRAME:

                        set = GetSelectedSet();
                        seq = set.GetSequence(SelectedIndex(eRoot.SEQUENCE));
                        index = SelectedIndex(eRoot.K_FRAME);
                        index += add ? 1 : 0;

                        if (name == null)
                            name = $"New Frame";

                        seq.AddKeyFrameSnapshot(index, name, Snapping);
                        break;
                }

                LibraryUpdate();
            }
            void DeleteItem()
            {
                switch (CurrentDirectoryLayer)
                {
                    case eRoot.JSET:
                        JsetBin.RemoveAt(SelectedIndex(eRoot.JSET));
                        break;

                    case eRoot.SEQUENCE:
                        GetSelectedSet().Sequences.RemoveAt(SelectedIndex(eRoot.SEQUENCE));
                        break;

                    case eRoot.K_FRAME:
                        GetSelectedSequence().RemoveKeyFrameAtIndex(SelectedIndex(eRoot.K_FRAME));
                        break;
                }

                int a = JsetBin.Count;

                if (SelectedIndex(eRoot.JSET) >= a && a > 0)
                {
                    SelectedIndexes[eRoot.JSET] = a - 1;
                }

                if (a == 0 || JsetBin[SelectedIndex(eRoot.JSET)] == null)
                {
                    SelectedIndexes[eRoot.SEQUENCE] = 0;
                    SelectedIndexes[eRoot.K_FRAME] = 0;
                    SelectedIndexes[eRoot.J_FRAME] = 0;
                    goto End;
                }

                int b = GetSelectedSet().Sequences.Count;
                if (SelectedIndex(eRoot.SEQUENCE) >= b && b > 0)
                {
                    SelectedIndexes[eRoot.SEQUENCE] = b - 1;
                }

                if (b == 0 || GetSelectedSequence() == null)
                {
                    SelectedIndexes[eRoot.K_FRAME] = 0;
                    SelectedIndexes[eRoot.J_FRAME] = 0;
                    goto End;
                }

                int c = GetSelectedSequence().Frames.Count;
                if (SelectedIndex(eRoot.J_FRAME) >= c && c > 0)
                {
                    SelectedIndexes[eRoot.J_FRAME] = c - 1;
                }

                if (c == 0 || GetSelectedKeyFrame() == null)
                {
                    SelectedIndexes[eRoot.J_FRAME] = 0;
                    goto End;
                }

            End:

                LibraryUpdate();
            }

            void LibraryUpdate()
            {
                int[] counts = new int[4];
                JointSet limb = null;
                Sequence seq = null;
                KeyFrame frame = null;

                if (JsetBin.Count > SelectedIndex(eRoot.JSET))
                    limb = GetSelectedSet();

                if (limb != null && limb.Sequences.Count > SelectedIndex(eRoot.SEQUENCE))
                    seq = limb.GetSequence(SelectedIndex(eRoot.SEQUENCE));

                if (seq != null && seq.Frames.Count > SelectedIndex(eRoot.K_FRAME))
                    frame = seq.GetKeyFrame(SelectedIndex(eRoot.K_FRAME));

                if (limb == null && CurrentDirectoryLayer > eRoot.JSET)
                    CurrentDirectoryLayer = eRoot.JSET;

                else if (seq == null && CurrentDirectoryLayer > eRoot.SEQUENCE)
                    CurrentDirectoryLayer = eRoot.SEQUENCE;

                else if (frame == null && CurrentDirectoryLayer > eRoot.K_FRAME)
                    CurrentDirectoryLayer = eRoot.K_FRAME;

                SelectElement();
            }

            void AppendLibraryItem(eRoot layer, int index, string itemName)
            {
                DisplayManagerBuilder.Clear();

                for (int i = 0; i < (int)layer; i++) // LayerIndent
                    DisplayManagerBuilder.Append(" ");

                // Selection Logic
                bool selected = CurrentDirectoryLayer == layer && SelectedIndexes[layer] == index;
                CursorIndex = selected ? cursorCounter : CursorIndex;
                cursorCounter++;

                // Cursor Logic
                DisplayManagerBuilder.Append(BuildCursor(selected));
                DisplayManagerBuilder.Append($"{index}:{itemName}");

                switch (layer)
                {
                    case eRoot.J_FRAME:
                        try
                        {
                            DisplayManagerBuilder.Append($"[{GetSelectedSet().Joints[index].TAG}]");
                        }
                        catch { }
                        break;

                    case eRoot.JSET:
                        try
                        {
                            DisplayManagerBuilder.Append($"{(GetSelectedSet() == CurrentWalkSet ? ":[LOADED]" : "")}");
                        }
                        catch { }
                        break;
                }


                RawBuffer.Add(DisplayManagerBuilder.ToString());
            }
            void JsetStringBuilder(JointSet set)
            {
                if (set.Sequences.Count == 0)
                    RawBuffer.Add(" No sequences found!");

                for (int seqIndex = 0; seqIndex < set.Sequences.Count; seqIndex++)
                {
                    AppendLibraryItem(eRoot.SEQUENCE, seqIndex, set.Sequences[seqIndex].Name);

                    if ((int)CurrentDirectoryLayer < 2 || SelectedIndex(eRoot.SEQUENCE) != seqIndex)
                        continue;

                    CursorIndex = cursorCounter;
                    Sequence seq = set.GetSequence(seqIndex);
                    SequenceStringBuilder(seq);
                }
            }
            void SequenceStringBuilder(Sequence seq)
            {
                if (seq.Frames.Count == 0)
                    RawBuffer.Add("  No frames found!");

                for (int kFrameIndex = 0; kFrameIndex < seq.Frames.Count; kFrameIndex++)
                {
                    AppendLibraryItem(eRoot.K_FRAME, kFrameIndex, seq.Frames[kFrameIndex].Name);

                    if ((int)CurrentDirectoryLayer < 3 || SelectedIndex(eRoot.K_FRAME) != kFrameIndex)
                        continue;

                    CursorIndex = cursorCounter;
                    KeyFrame kFrame = seq.GetKeyFrame(kFrameIndex);
                    KframeStringBuilder(kFrame);
                }
            }
            void KframeStringBuilder(KeyFrame kFrame)
            {
                if (kFrame.Jframes.Count == 0)
                    RawBuffer.Add("   No j-Frames found!");

                for (int jFrameIndex = 0; jFrameIndex < kFrame.Jframes.Count(); jFrameIndex++)
                {
                    JointFrame jFrame = kFrame.GetJointFrameByFrameIndex(jFrameIndex);
                    JframeStringBuilder(jFrame);
                }
            }
            void JframeStringBuilder(JointFrame jFrame)
            {
                AppendLibraryItem(eRoot.J_FRAME, jFrame.MyIndex, $"{jFrame.Joint.Connection.CustomName}:{jFrame.MySetting.MyValue()}");
            }
        }
        class TablePage : Page
        {
            public TablePage() : base("Assignment Table")
            {
                Buttons = new Dictionary<GUIKey, Button>
                {
                    {GUIKey.ALPHA_1,    new Button("Main",              ()=> SetGuiMode(GUIMode.MAIN)) },
                    {GUIKey.ALPHA_2,    new Button("Aquire All Joints", ()=> FindAndAssignAllJoints()) },

                    {GUIKey.ALPHA_3,    new Button("Add foot",          ()=> TableJsetAdjust(0,1))     },
                    {GUIKey.ALPHA_4,    new Button("Remove foot",       ()=> TableJsetAdjust(0,-1))    },
                    {GUIKey.ALT_LEFT,   new Button("Previous JointSet", ()=> TableJsetAdjust(-1, 0))   },
                    {GUIKey.ALT_RIGHT,  new Button("Next JointSet",     ()=> TableJsetAdjust(1, 0))    },

                    {GUIKey.FORWARD,    new Button("Shift Up",          ()=> TableShift(0, 1, 0))      },
                    {GUIKey.BACKWARD,   new Button("Shift Down",        ()=> TableShift(0, -1, 0))     },
                    {GUIKey.LEFT,       new Button("Shift Left",        ()=> TableShift(-1, 0, 0))     },
                    {GUIKey.RIGHT,      new Button("Shift Right",       ()=> TableShift(1, 0, 0))      },

                    {GUIKey.UP,         new Button("Increase Value",    ()=> TableShift(0,0,1))        },
                    {GUIKey.DOWN,       new Button("Decrease Value",    ()=> TableShift(0,0,-1))       }
                };
                SelectedIndexes = new Dictionary<eRoot, int>
                {
                    {eRoot.JSET, 0},
                    {eRoot.JOINT, 0},
                    {eRoot.PARAM, 0},
                };
            }

            protected override string[] PageBuilder()
            {
                cursorCounter = 0;
                RawBuffer.Clear();
                RawBuffer.Add("= Assignment =");
                RawBuffer.Add($"[Selected JSET:{GetJointSet(SelectedIndexes[eRoot.JSET])?.Name}][FootCount:{GetJointSet(SelectedIndexes[eRoot.JSET])?.Feet.Count}]=");


                DisplayManagerBuilder.Clear();

                for (int i = 0; i < 6; i++)
                    DisplayManagerBuilder.Append($"[{(PARAM)i}]");

                RawBuffer.Add(DisplayManagerBuilder.ToString());

                for (int i = 0; i < JointBin.Count; i++)
                    AppendJointToTable((Joint)JointBin[i], i);

                return RawBuffer.ToArray();
            }

            void AppendJointToTable(Joint joint, int index)
            {
                DisplayManagerBuilder.Clear();

                bool selected = SelectedIndexes[eRoot.JOINT] == index;
                CursorIndex = selected ? cursorCounter : CursorIndex;
                cursorCounter++;

                DisplayManagerBuilder.Append(BuildCursor(selected));
                DisplayManagerBuilder.Append($"{index}:{joint.SaveData()}");

                RawBuffer.Add(DisplayManagerBuilder.ToString());

            }
            void FindAndAssignAllJoints()
            {
                ReTagBuffer.Clear();

                GetGridBlocksOfType(ReTagBuffer);

                foreach (IMyMechanicalConnectionBlock joint in ReTagBuffer)
                {
                    Joint freshJoint = NewJoint(joint, JointData.Default);
                    int oldIndex = JointBin.FindIndex(x => x.MyIndex == freshJoint.MyIndex);
                    if (freshJoint.MyIndex > -1 && oldIndex > -1)
                        JointBin[oldIndex] = freshJoint;
                    else
                        JointBin.Add(freshJoint);
                }
            }
            void TableJsetAdjust(int deltaIX, int deltaFeet)
            {
                SelectedIndexes[eRoot.JSET] += deltaIX;
                SelectedIndexes[eRoot.JSET] = SelectedIndexes[eRoot.JSET] < 0 ? JsetBin.Count - 1 : SelectedIndexes[eRoot.JSET] >= JsetBin.Count ? 0 : SelectedIndexes[eRoot.JSET];

            }
            void TableShift(int deltaX, int deltaY, int deltaValue)
            {
                SelectedIndexes[eRoot.JOINT] += deltaY;
                SelectedIndexes[eRoot.JOINT] = SelectedIndexes[eRoot.JOINT] < 0 ? JointBin.Count - 1 : SelectedIndexes[eRoot.JOINT] >= JointBin.Count ? 0 : SelectedIndexes[eRoot.JOINT];

                SelectedIndexes[eRoot.PARAM] += deltaX;
                SelectedIndexes[eRoot.PARAM] = SelectedIndexes[eRoot.PARAM] < 0 ? ParamCount - 1: SelectedIndexes[eRoot.PARAM] >= ParamCount ? 0 : SelectedIndexes[eRoot.PARAM];
            }
        }
        class OptionsPage : Page
        {
            public OptionsPage() : base("Options")
            {
                Buttons = new Dictionary<GUIKey, Button>
                {
                    { GUIKey.RIGHT,    new Button("Adjust Up",  ()=>AdjustOption(true))         },
                    { GUIKey.LEFT,     new Button("Adjust Down",()=>AdjustOption(false))        },
                    { GUIKey.FORWARD,  new Button("Menu Up",    ()=>SelectOption(true))         },
                    { GUIKey.BACKWARD, new Button("Menu Down",  ()=>SelectOption(false))        },

                    { GUIKey.ALPHA_1,  new Button("Main",       () => SetGuiMode(GUIMode.MAIN)) },
                };
                SelectedIndexes = new Dictionary<eRoot, int>
                { { eRoot.PARAM,0} };
            }
            protected override string[] PageBuilder()
            {
                List<string> stringList = new List<string>();
                stringList.Add("===Options===");
                if (Descriptions.MyState())
                    LineWrapper(stringList, Options[SelectedIndexes[eRoot.PARAM]].Description, CharTotalCount);
                stringList.Add($"=================");
                HeaderSize = stringList.Count;

                try
                {
                    for (int i = 0; i < Options.Count; i++)
                        AppendOptionItem(i, stringList, Options[i]);
                }
                catch
                {
                    stringList.Add("Error!");
                }

                string[] output = stringList.ToArray();
                return output;
            }
            void AppendOptionItem(int index, List<string> rawStrings, Option option)
            {
                DisplayManagerBuilder.Clear();

                bool selected = index == SelectedIndexes[eRoot.PARAM];

                DisplayManagerBuilder.Append(BuildCursor(selected));
                DisplayManagerBuilder.Append($"{option.Name}:{option.Current()}");

                rawStrings.Add(DisplayManagerBuilder.ToString());
            }
            void SelectOption(bool up)
            {
                SelectedIndexes[eRoot.PARAM] += up ? -1 : 1;
                SelectedIndexes[eRoot.PARAM] = SelectedIndexes[eRoot.PARAM] >= Options.Count ? 0 : SelectedIndexes[eRoot.PARAM] < 0 ? Options.Count - 1 : SelectedIndexes[eRoot.PARAM];
                CursorIndex = SelectedIndexes[eRoot.PARAM];
            }
            void AdjustOption(bool up)
            {
                Options[SelectedIndexes[eRoot.PARAM]].Adjust(up);
            }
        }
        class GenericPage : Page
        {
            public GenericPage() : base("Default Page")
            {
                HeaderSize = 2;

                Buttons = new Dictionary<GUIKey, Button>
                {
                    {GUIKey.ALPHA_1, new Button("Controls",         ()=> SetGuiMode( GUIMode.PILOT  ))  },
                    {GUIKey.ALPHA_2, new Button("Information",      ()=> SetGuiMode( GUIMode.INFO   ))  },
                    {GUIKey.ALPHA_3, new Button("Library",          ()=> SetGuiMode( GUIMode.CREATE ))  },
                    {GUIKey.ALPHA_4, new Button("Options",          ()=> SetGuiMode( GUIMode.OPTIONS))  },
                    {GUIKey.ALPHA_5, new Button("Assignment",       ()=> SetGuiMode( GUIMode.ASSIGN ))  },
                    {GUIKey.ALPHA_6, new Button("Save CustomData",  ()=> WriteCustomData())             },
                    {GUIKey.ALPHA_7, new Button("Save ActiveData",  ()=> SavingData = true)             },
                    {GUIKey.ALPHA_8, new Button("Reload ActiveData",()=> LoadingData = true)            },

                    {GUIKey.ALT_1,   new Button("Main",             ()=> SetGuiMode(GUIMode.MAIN))      },

                    {GUIKey.FORWARD, new Button("Scroll Up",        ()=> Scroll(true))                  },
                    {GUIKey.BACKWARD,new Button("Scroll Down",      ()=> Scroll(false))                 },
                };
            }
            void Scroll(bool up)
            {
                CursorIndex += up ? -1 : 1;
                CursorIndex = CursorIndex < 1 ? 0 : CursorIndex >= RawBuffer.Count ? RawBuffer.Count - 1 : CursorIndex;
            }

            public override void SetMode(GUIMode mode)
            {
                switch (mode)
                {
                    case GUIMode.MAIN:
                        Name = "Main Menu";
                        AlternateMode = false;
                        break;

                    case GUIMode.INFO:
                        Name = "Information";
                        AlternateMode = true;
                        break;

                    default:
                        Name = "Default Page";
                        break;
                }
            }
            protected override string[] PageBuilder()
            {
                string input = AlternateMode ? InfoText : MainText;

                string[] output = input.Split('\n');
                RawBuffer = output.ToList();
                //LineBufferSize = output.Length;
                return output;
            }
        }
        class ControlPage : Page
        {
            public ControlPage() : base("Controls")
            {
                Buttons = new Dictionary<GUIKey, Button>
                {
                    {GUIKey.ALPHA_1, new Button("Main",                 () => SetGuiMode(GUIMode.MAIN))     },
                    {GUIKey.ALPHA_2, new Button("Unlock Feet",          () => CurrentWalkSet?.UnlockAllFeet()) },
                    {GUIKey.ALPHA_3, new Button("Toggle Orientation",   () => Orientation.Adjust()) },
                    {GUIKey.ALPHA_4, new Button("Toggle Pause",         () => CurrentWalk?.ToggleClockPause()) },
                    {GUIKey.ALPHA_5, new Button("Toggle Direction",     () => CurrentWalk?.ToggleClockDirection()) },
                    {GUIKey.ALPHA_6, new Button("Zero Out Mech/Walk",   () => ZeroOutMech()) },
                };
            }

            protected override string[] PageBuilder()
            {
                List<string> stringList = new List<string>();
                stringList.Add("===Controls===");
                stringList.Add("==============");

                try
                {
                    stringList.Add($"{(CurrentWalkSet == null ? "No legs!" : CurrentWalkSet.Locked ? "Walking..." : "Flying...")}");
                }
                catch
                {
                    stringList.Add("Error!");
                }

                string[] output = stringList.ToArray();
                return output;
            }

            void ZeroOutMech()
            {
                CurrentWalkSet?.ZeroJointSet(CurrentWalkSet.ZeroFrame);
                CurrentWalk?.ZeroSequence();
            }
        }
        class Page
        {
            public string Name;

            public int CursorIndex;
            protected int cursorCounter;
            public int LineBufferSize;
            public int HeaderSize;

            protected bool AlternateMode;

            protected Dictionary<GUIKey, Button> Buttons;
            protected Dictionary<eRoot, int> SelectedIndexes;
            
            protected List<string> RawBuffer = new List<string>();

            public Page(string name)
            {
                Name = name;
            }
            public bool TriggerButton(GUIKey keyPress)
            {
                if (!Buttons.ContainsKey(keyPress))
                    return false;

                Buttons[keyPress].Trigger();
                return true;
            }
            public virtual void SetMode(GUIMode mode) { }
            protected virtual string[] PageBuilder() { return null; }
            public int SelectedIndex(eRoot selection) { return SelectedIndexes.ContainsKey(selection) ? SelectedIndexes[selection] : -1; }
            public virtual int SelectedCount(eRoot selection) { return 0; }
            public virtual int SelectedCount() { return 0; }
            public virtual Root SelectedRoot() { return null; }

            public void UpdatePage()
            {
                ButtonStringBuilder();
                FormattedSplashStringBuilder(PageBuilder());
            }
            void ButtonStringBuilder()
            {
                ButtonBuilder.Clear();
                ButtonBuilder.Append($"= Inputs: [{GetCurrentGuiMode()}] =\n");

                int offset = AlternateMode ? 10 : 0;

                for (int i = 0; i <= 10; i++)
                    if (Buttons.ContainsKey((GUIKey)(i + offset)))
                        ButtonBuilder.Append($"{i} - {Buttons[(GUIKey)(i + offset)].Name}\n");

                offset = 20;

                for (int i = 0; i <= InputLabels.Length; i++)
                    if (Buttons.ContainsKey((GUIKey)(i + offset)))
                        ButtonBuilder.Append($"{InputLabels[i]} - {Buttons[(GUIKey)(i + offset)].Name}\n");

            }
            void FormattedSplashStringBuilder(string[] input)
            {
                if (input == null)
                    return;

                SplashBuilder.Clear();
                LineBufferSize = LineTotalCount - HeaderSize;
                int startIndex = CursorIndex - (LineBufferSize / 2);
                startIndex = startIndex < 0 ? 0 : startIndex;

                for (int i = 0; i < HeaderSize; i++)
                    SplashBuilder.Append($"{input[i]}\n");

                if (!CapLines || LineBufferSize < 1)
                    for (int i = HeaderSize; i < input.Length; i++)
                        SplashBuilder.Append(input[i] + "\n");
                else
                    for (int i = startIndex; i < startIndex + LineBufferSize && i + HeaderSize < input.Length; i++)
                        SplashBuilder.Append(input[i + HeaderSize] + "\n");
            }

        }
        class Button
        {
            public string Name;
            ButtonEvent Event;
            public Button(string name, ButtonEvent @event)
            {
                Name = name;
                Event = @event;
            }
            public void Trigger() { Event(); }
        }
        #endregion

        #region MECHANICAL
        class Functional : Root
        {
            IMyFunctionalBlock FuncBlock;
            public Functional(IMyFunctionalBlock funcBlock) : base () { FuncBlock = funcBlock; }
            public Functional(IMyFunctionalBlock funcBlock, RootData data) : base (data) { FuncBlock = funcBlock; }

            public bool Save()
            {
                if (FuncBlock == null)
                    return false;

                FuncBlock.CustomData = SaveData();
                return true;
            }
        }
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
                StaticDlog("Force Set!");
            }
            public bool IsAlive()
            {
                try { return Connection.IsWorking; }
                catch { return false; }
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
                //return Connection.GetMaximum<float>("Torque");
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
            //public override void UpdatePlaneDisplacement()
            //{
            //    
            //}

        }
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
                //base.LerpAnimationFrame(lerpTime);

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
        }
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
                //base.LerpAnimationFrame(lerpTime);

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

        class Magnet : Functional
        {
            public int FootIndex;
            public IMyLandingGear Gear;
            public Magnet(RootData root, IMyLandingGear gear, int footIndex) : base(gear, root)
            {
                TAG = MagnetTag;
                FootIndex = footIndex;
                Gear = gear;
            }
            public Magnet(IMyLandingGear gear) : base(gear)
            {
                Gear = gear;
                BUILT = Load(gear == null ? null : gear.CustomData);
            }

            protected override bool Load(string[] data)//, Option option = null)
            {
                if (!base.Load(data))
                    return false;

                try { FootIndex = int.Parse(data[(int)PARAM.fIX]); }
                catch { FootIndex = -1; }
                return true;
            }


            protected override void saveData(string[] buffer)
            {
                buffer[(int)PARAM.fIX] = FootIndex.ToString();
            }

            public void InitializeGear()
            {
                Gear.AutoLock = false;
                Gear.Enabled = true;
            }

            public void ToggleLock(bool locking)
            {
                Gear.AutoLock = locking;
                if (locking)
                {
                    Gear.Lock();
                    Gear.AutoLock = true;
                }
                else
                {
                    Gear.Unlock();
                    Gear.AutoLock = false;
                }

            }
            public bool IsAlive()
            {
                try { return Gear.IsWorking; }
                catch { return false; }
            }
            public bool IsTouching()
            {
                return Gear.LockMode == LandingGearMode.ReadyToLock;
            }
            public bool IsLocked()
            {
                return Gear.LockMode == LandingGearMode.Locked;
            }
        }
        #endregion

        #region ANIMATION
        class JointSet : Root
        {
            public string GroupName;

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
                GroupName = groupName;
                GenerateZeroFrame();
            }

            public JointSet(string input, IMyTerminalBlock plane, List<Foot> buffer) : base()
            {
                Plane = plane;
                Feet.AddRange(buffer);
                BUILT = Load(input);
            }

            public void GenerateZeroFrame()
            {
                //PROG.LibraryBuilder.Add("Generating Zero Frame!");
                ZeroFrame = NewKeyFrame(ParentData("Zero Frame"), this);
                //PROG.LibraryBuilder.Add("Generated!");
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
            protected override bool Load(string[] data)//, Option option = null)
            {
                if (!base.Load(data))
                    return false;

                try { GroupName = data[(int)PARAM.GroupName]; }
                catch { GroupName = null; }
                return true;
            }
            protected override void saveData(string[] buffer)
            {
                buffer[(int)PARAM.GroupName] = GroupName;
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

                //StreamDlog("New Lock Candidate...");
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
                        //TogglePlaneing(false);
                        SnapShotPlane();
                        //safety = true;
                        break;
                    }

                foreach (Foot foot in Feet)
                {
                    if (foot != null)
                    {
                        //StreamDlog($"Updateing: {foot.Name}");
                        foot.GenerateAxisMagnitudes(Plane.WorldMatrix);
                        for (int i = 0; i < foot.Planars.Count; i++)
                            if (foot.Planars[i] != null)
                            {
                                Joint plane = foot.GetPlanar(i);
                                //StreamDlog($"Correcting: {plane.Name}\n" +
                                //    $"Planeing: {plane.Planeing}");

                                if (safety)
                                {
                                    //StreamDlog("Safety break");
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
                                    //StreamDlog("Planeing");
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
        
        class Foot : Root
        {
            public List<Root> Toes = new List<Root>();
            public List<Root> Planars = new List<Root>();
            public List<Root> Magnets = new List<Root>();

            public int LockIndex;
            public bool Locked = false;
            public bool Planeing;
            public Vector3 PlanarRatio;

            public Foot(RootData data) : base(data)
            {
                TAG = FootTag;
            }

            public Foot(string input) : base()
            {
                StaticDlog("Foot Constructor:");
                BUILT = Load(input);
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
                    magnet.InitializeGear();
            }
            public void ToggleLock(bool locking = true)
            {
                foreach (Magnet magnet in Magnets)
                    magnet.ToggleLock(locking);

                Locked = locking;
                ToggleGrip(locking);
                ToggleForce(locking);
                UpdateFootPlaneing(Planeing);
            }
            public bool CheckTouching()
            {
                bool result = false;
                foreach (Magnet magnet in Magnets)
                    if (magnet.IsAlive() && magnet.IsTouching())
                    {
                        result = true;
                        break;
                    }

                //StreamDlog($"Foot {MyIndex} Is Touching?: {result}");
                return result;
            }
            public bool CheckLocked()
            {
                bool result = false;
                foreach (Magnet gear in Magnets)
                    if (gear.IsAlive() && gear.IsLocked())
                    {
                        result = true;
                        break;
                    }

                //StreamDlog($"Foot {MyIndex} Is Locked?: {result}");
                return result;
            }
            void ToggleGrip(bool gripping = true)
            {
                foreach (Joint toe in Toes)
                    toe.Gripping = gripping;
            }
            void ToggleForce(bool maxForce)
            {
                foreach (Joint planar in Planars)
                    planar.SetForce(maxForce);
            }
            public void UpdateFootPlaneing(bool toggle)
            {
                Planeing = toggle;

                foreach (Joint plane in Planars)
                    if (plane != null)
                    {
                        plane.Planeing = (Locked || (plane.TAG == TurnTag && Strafing.MyState())) && Planeing;
                    }
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

            protected override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try { LockIndex = int.Parse(data[(int)PARAM.lIX]); }
                catch { LockIndex = -1; }
                return true;
            }

            protected override void saveData(string[] buffer)
            {
                buffer[(int)PARAM.lIX] = LockIndex.ToString();
            }
        }
        class Animation : Root
        {
            public Setting MySetting;

            public Animation(RootData data) : base(data)
            {

            }

            public Animation() : base()
            {
                
            }

            public virtual void GenerateSetting(float init)
            {

            }
            protected override bool Load(string[] data)//, Option option = null)
            {
                if (!base.Load(data))
                    return false;

                StaticDlog($"Anim Load:{TAG}");

                return true;
            }
            protected override void saveData(string[] buffer)
            {
                buffer[(int)PARAM.SettingInit] = (MySetting == null ? 0 : MySetting.MyValue()).ToString();
            }
        }
        class JointFrame : Animation
        {
            public Joint Joint;

            public JointFrame(RootData root, Joint joint, bool snapping = true) : base(root) // Snapshot
            {
                TAG = JframeTag;
                Joint = joint;
                GenerateSetting((float)Joint.CurrentPosition());
                if (snapping)
                {
                    MySetting.Change((int)MySetting.MyValue());
                }
            }
            public JointFrame(string input, Joint joint) : base()
            {
                Joint = joint;
                BUILT = Load(input);
            }
            protected override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try { GenerateSetting(float.Parse(data[(int)PARAM.SettingInit])); }
                catch { GenerateSetting(0); }

                return true;
            }
            public override void GenerateSetting(float init)
            {
                Static($"jFrame {Name} GeneratingSetting...\n");
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
        class KeyFrame : Animation
        {
            public List<Root> Jframes = new List<Root>();

            public KeyFrame(RootData root, List<Root> jFrames = null) : base(root)
            {
                TAG = KframeTag;
                Jframes = jFrames != null ? jFrames : new List<Root>();
                GenerateSetting(FrameLengthDef);
            }
            public KeyFrame(string input, List<JointFrame> buffer) : base()
            {
                Jframes.AddRange(buffer);
                BUILT = Load(input);
            }
            protected override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try { GenerateSetting(float.Parse(data[(int)PARAM.SettingInit])); }
                catch { GenerateSetting(0); }

                return true;
            }
            public JointFrame GetJointFrameByJointIndex(int index)
            {
                return GetJointFrameByFrameIndex(Jframes.FindIndex(x => x.MyIndex == index));
            }

            public JointFrame GetJointFrameByFrameIndex(int index)
            {
                if (index < 0 || index >= Jframes.Count)
                    return null;
                return (JointFrame)Jframes[index];
            }
            public override void ReIndex()
            {
                //for (int i = 0; i < Jframes.Count; i++)
                //    Jframes[i].MyIndex = i;

                Jframes.Sort(MySort);
            }
            public override void GenerateSetting(float init)
            {
                MySetting = new Setting("Frame Length", "The time that will be displaced between this frame, and the one an index ahead",
                    init, FrameIncrmentMag, FrameLengthCap, FrameLengthMin, FrameDurationIncrement); //Inverse for accelerated effect
            }
        }
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

            public Sequence(RootData root, JointSet set = null) : base(root)
            {
                TAG = SeqTag;
                JointSet = set;
                GenerateSetting(ClockSpeedDef);
            }
            public Sequence(string input, JointSet set, List<KeyFrame> buffer) : base()
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
                //for (int i = 0; i < Frames.Count; i++)
                //    Frames[i].MyIndex = i;

                Frames.Sort(MySort);
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

                KeyFrame newKFrame = NewKeyFrame(ParentData(name, index), JointSet);

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
        #endregion

        #endregion

        #region GUI

        #region GUI RESOURCES
        static List<IMyTextSurface> Screens = new List<IMyTextSurface>();
        static StringBuilder DebugBinStream = new StringBuilder();
        static StringBuilder DebugBinStatic = new StringBuilder();
        static StringBuilder DisplayManagerBuilder = new StringBuilder();
        static StringBuilder ButtonBuilder = new StringBuilder();
        static StringBuilder SplashBuilder = new StringBuilder();
        static StringBuilder InputReader = new StringBuilder();
        static StringBuilder SaveData = new StringBuilder();
        static StringBuilder RootDataBuilder = new StringBuilder();
        #endregion

        #region GUI CONSTS

        static readonly string[] InputLabels =
        {
            "right",
            "left",
            "up",
            "down",
            "backward",
            "forward",
            "roll right",
            "roll left"
        };

        static readonly string[] Cursor = { "  ", "->" };

        #region INFO PANELS
        static readonly string MainText = $"Mech Control {VersionNumber}\n\n" +
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
        #endregion

        #endregion

        #region GUI BUILDERS
        string MatrixToStringA(MatrixD matrix, string digits = "")
        {
            return
                $"R: {matrix.M11.ToString(digits)} : {matrix.M12.ToString(digits)} : {matrix.M13.ToString(digits)}\n" +
                $"U: {matrix.M21.ToString(digits)} : {matrix.M22.ToString(digits)} : {matrix.M23.ToString(digits)}\n" +
                $"F: {matrix.M31.ToString(digits)} : {matrix.M32.ToString(digits)} : {matrix.M33.ToString(digits)}\n" +
                $"T: {matrix.M41.ToString(digits)} : {matrix.M42.ToString(digits)} : {matrix.M43.ToString(digits)}\n" +
                $"S: {matrix.M14.ToString(digits)} : {matrix.M24.ToString(digits)} : {matrix.M34.ToString(digits)}\n";
        }
        static string MatrixToStringB(MatrixD matrix, string digits = "#.##")
        {
            return
                $"R:{matrix.Right.X.ToString(digits)}|{matrix.Right.Y.ToString(digits)}|{matrix.Right.Z.ToString(digits)}\n" +
                $"U:{matrix.Up.X.ToString(digits)}|{matrix.Up.Y.ToString(digits)}|{matrix.Up.Z.ToString(digits)}\n" +
                $"F:{matrix.Forward.X.ToString(digits)}|{matrix.Forward.Y.ToString(digits)}|{matrix.Forward.Z.ToString(digits)}\n" +
                $"T:{matrix.Translation.X.ToString(digits)}|{matrix.Translation.Y.ToString(digits)}|{matrix.Translation.Z.ToString(digits)}\n";
        }
        static string BuildCursor(bool selected)//, int count)
        {
            //int cursor;
            //if (selected)
            //{
            //    CursorIndex = count - 1;
            //    cursor = 1;
            //}
            //else
            //    cursor = 0;

            return $"{Cursor[selected ? 1 : 0]}";
        }
        static void LineWrapper(List<string> buffer, string[] words, int charMax)
        {
            string newLine = string.Empty;
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > charMax)
                {
                    buffer.Add($"{words[i]}\n");
                    newLine = string.Empty;
                    continue;
                }
                if (words[i].Length + newLine.Length > charMax)
                {
                    buffer.Add(newLine);
                    newLine = "";
                }
                newLine += $"{words[i]} ";
            }
            if (newLine != string.Empty)
                buffer.Add(newLine);
        }
        #endregion

        #region GUI INPUT
        static void SetGuiMode(GUIMode mode)
        {
            PROG._CurrentGUIMode = mode;
            Pages[PROG._CurrentGUIMode].SetMode(mode);
            if (CockPit != null)
            {
                CockPit.ControlThrusters = mode == GUIMode.PILOT;
                foreach (IMyTerminalBlock flightBlock in FlightGroup)
                    if (flightBlock is IMyGyro)
                        (flightBlock as IMyGyro).GyroOverride = mode != GUIMode.PILOT;
            }
        }
        void ButtonPress(GUIKey keyPress)
        {
            GetCurrentPage().TriggerButton(keyPress);

            if (AutoSave.MyState())
                SavingData = true;

            GUIUpdate();
        }
        void GUIUpdate()
        {
            Static("GUI Update\n");

            GetCurrentPage().UpdatePage();
        }

        static bool UserInputString(ref string buffer)
        {
            try
            {
                InputReader.Clear();
                GetSurface(Screen.INPUT).ReadText(InputReader);
                buffer = InputReader.ToString();
                if (buffer == "")
                    buffer = null;

                return true;
            }
            catch
            {
                return false;
            }
        }
        static bool UserInputFloat(out float buffer)
        {
            try
            {
                InputReader.Clear();
                GetSurface(Screen.INPUT).ReadText(InputReader);
                buffer = float.Parse(InputReader.ToString());
                return true;
            }
            catch
            {
                buffer = 0;
                return false;
            }
        }
        #endregion

        #region GUI OUTPUTS
        static bool CheckStaticLimit(string append)
        {
            return DebugBinStatic.Length + append.Length > StaticDebugCharCap;
        }
        static bool Write(Screen screen, StringBuilder input, bool append = true)
        {
            return Write(screen, input.ToString(), append);
        }
        static bool Write(Screen screen, string input, bool append = true)
        {
            IMyTextSurface surface = GetSurface(screen);

            if (surface != null)
                surface.WriteText(input, append);

            return surface != null;
        }
        static bool Static(string input, bool append = true)
        {
            if (!append || CheckStaticLimit(input))
                DebugBinStatic.Clear();
            DebugBinStatic.Append(input);
            return Write(Screen.DEBUG_STATIC, DebugBinStatic, false);
        }
        bool ClearStatic()
        {
            return Static("", false);
        }

        bool MenuSystem(Screen gui, Screen buttons)
        {
            return Write(gui, SplashBuilder, false) && Write(buttons, ButtonBuilder, false);
        }
        bool Diagnostics(Screen panel)
        {
            DisplayManagerBuilder.Clear();
            try
            {
                DisplayManagerBuilder.Append(
                    $"Control: {CockPit != null}\n" +
                    $"Designated: {DesignatedPlane != null}\n" +
                    $"PlaneBlock: {CurrentWalkSet.Plane != null}\n");
                Vector3 B = CurrentWalkSet.PlaneBuffer;
                Vector3 T = CurrentWalkSet.TurnBuffer;
                Foot F = CurrentWalkSet.GetFoot(CurrentWalkSet.LockedIndex);
                Foot U = CurrentWalkSet.GetFoot(0);

                DisplayManagerBuilder.Append($"RawInput:\n{CockPit.RotationIndicator.Y}:{CockPit.RotationIndicator.X}:{CockPit.RollIndicator}\n");

                DisplayManagerBuilder.Append($"LockedFinals:\n");
                if (F != null)
                    for (int i = 0; i < 3; i++)
                        DisplayManagerBuilder.Append($"{(F.GetPlanar(i) == null ? "null" : F.GetPlanar(i).ActiveTarget.ToString())}\n");
                else
                    DisplayManagerBuilder.Append("No locked foot!\n");

                DisplayManagerBuilder.Append($"LockedIndex: {CurrentWalkSet.LockedIndex}\n");
                DisplayManagerBuilder.Append($"0 indexFinals:\n");
                for (int i = 0; i < U.Planars.Count; i++)
                    DisplayManagerBuilder.Append($"Planar {i} (Anim/Active): {U.GetPlanar(i).AnimTarget}/{U.GetPlanar(i).ActiveTarget}\n");

                for (int i = 0; i < CurrentWalkSet.Feet.Count; i++)
                {
                    Foot foot = CurrentWalkSet.GetFoot(i);
                    DisplayManagerBuilder.Append($"Foot {i} Locked: {foot.Locked}\n");
                    for (int j = 0; j < foot.Planars.Count; j++)
                    {
                        Joint joint = foot.GetPlanar(j);
                        DisplayManagerBuilder.Append($"Planar {joint.Name} rotation axis: {joint.ReturnRotationAxis()}\n");
                    }
                }
            }
            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }

            return Write(panel, DisplayManagerBuilder, false);
        }
        bool MechStatus(Screen panel)
        {
            DisplayManagerBuilder.Clear();

            try
            {
                DisplayManagerBuilder.Append(
                     "Mech Status:\n" +
                    $"-Planeing: {Orientation.MyState()}\n\n" +

                    $"CurrentWalkSet: {CurrentWalkSet.Name}\n" +
                    $"-LockedFootIndex: {CurrentWalkSet.LockedIndex}\n\n" +

                    $"CurrentWalk: {CurrentWalk.Name}\n" +
                    $"-ClockState: {CurrentWalk.CurrentClockMode}\n" +
                    $"-ClockTime: {CurrentWalk.CurrentClockTime}\n" +
                    $"-StepDelay: {CurrentWalk.StepDelay}\n" +
                    $"-FrameLength: {CurrentWalk.CurrentFrames[0].MySetting.MyValue()}\n" +
                    $"-LoadedFrames: {CurrentWalk.CurrentFrames[0].Name} || {CurrentWalk.CurrentFrames[1].Name}\n");
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }

            return Write(panel, DisplayManagerBuilder, false);
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
            bool result = Write(panel, DisplayManagerBuilder, false);
            DebugBinStream.Clear();
            return result;
        }
        #endregion

        #endregion

        #region RUNTIME

        #region UPATES
        void ControlInput()
        {
            if (CockPit == null)
            {
                Echo("No Control!");
                InputRotationBuffer = Vector3.Zero;
                return;
            }

            if (GetCurrentGuiMode() == GUIMode.PILOT)
            {
                if (!DroneOverride.MyState() && DroneControlled && !WAIT)
                {
                    DebugBinStream.Append("Buffering drone input...\n");

                    InputRotationBuffer.X = -MECH_IX_BUFFER[(int)MechIx.PITCH];
                    InputRotationBuffer.Y = -MECH_IX_BUFFER[(int)MechIx.YAW];
                    InputRotationBuffer.Z = -MECH_IX_BUFFER[(int)MechIx.ROLL];
                    InputTurnBuffer = -MECH_IX_BUFFER[(int)MechIx.MOVE_X];
                    MoveBuffer = -(int)MECH_IX_BUFFER[(int)MechIx.MOVE_Z];

                    DebugBinStream.Append("Buffering Complete!\n");
                }
                else
                {
                    InputRotationBuffer.Y = LookScalar * -CockPit.RotationIndicator.Y;
                    InputRotationBuffer.X = LookScalar * -CockPit.RotationIndicator.X;

                    InputRotationBuffer.X = InputRotationBuffer.X < MouseSensitivity.MyValue() ? InputRotationBuffer.X : MouseSensitivity.MyValue();
                    InputRotationBuffer.Y = InputRotationBuffer.Y < MouseSensitivity.MyValue() ? InputRotationBuffer.Y : MouseSensitivity.MyValue();

                    InputRotationBuffer.Z = RollScalar * -CockPit.RollIndicator;
                    InputTurnBuffer = CockPit.MoveIndicator.X;
                    MoveBuffer = (int)CockPit.MoveIndicator.Z;
                }

                DebugBinStream.Append($"TurnBuffer(f): {InputTurnBuffer}\n");

                if (LastMechWalkInput != MoveBuffer)
                {
                    LastMechWalkInput = MoveBuffer;
                    CurrentWalk.SetClockMode((ClockMode)MoveBuffer);
                }

                /*switch (MoveBuffer) //Walking
                {
                    case 1:
                        if (LastMechWalkInput == -1)
                            break;
                        LastMechWalkInput = -1;
                        CurrentWalk.SetClockMode(ClockMode.REV);
                        break;

                    case 0:
                        if (LastMechWalkInput == 0)
                            break;
                        LastMechWalkInput = 0;
                        CurrentWalk.SetClockMode(ClockMode.PAUSE);
                        break;

                    case -1:
                        if (LastMechWalkInput == 1)
                            break;
                        LastMechWalkInput = 1;
                        CurrentWalk.SetClockMode(ClockMode.FOR);
                        break;
                }*/

                DebugBinStream.Append("Walk input digested\n");
            }
            else
            {
                DebugBinStream.Append("Menu Navigation...\n");

                int[] menuMove = {
                    (int)CockPit.MoveIndicator.X,
                    (int)CockPit.MoveIndicator.Y,
                    (int)CockPit.MoveIndicator.Z,
                    (int)CockPit.RollIndicator
                };
                
                for (int i = 0; i < LastMenuInput.Length; i++)
                {
                    DebugBinStream.Append($" Axis {i}: {menuMove[i]}\n");

                    if (LastMenuInput[i] == menuMove[i])
                        continue;

                    LastMenuInput[i] = menuMove[i];

                    if (menuMove[i] != 0)
                    {
                        GUIKey nav = (GUIKey)((int)GUIKey.RIGHT + (i * 2) + (menuMove[i] < 0 ? 1 : 0));
                        Static($"move: {nav}\n");
                        ButtonPress(nav);
                    }
                }
            }
        }
        
        void DroneCommand(MechAction action)
        {
            switch (action)
            {
                case MechAction.WAIT:
                    WAIT = true;
                    break;

                case MechAction.DEFAULT:
                    WAIT = false;
                    break;
            }
        }
        void RuntimeArguments(string argument)
        {
            if (argument == null ||
                argument == "")
                return;

            switch (argument)
            {
                case "CLEAR":
                    ClearStatic();
                    break;

                default:
                    if (!argument.Contains("BUTTON:"))
                        break;

                    string code = argument.Split(':')[1];

                    int button = 0;
                    if (int.TryParse(code, out button))
                        ButtonPress((GUIKey)button);

                    break;
            }
        }

        void WalkManager()
        {
            if (!StatorControl.MyState() ||
                CurrentWalkSet == null ||
                CurrentWalk == null)
                return;

            WithinTargetThreshold = CurrentWalkSet.UpdateJoints();
            CurrentWalkSet.UpdatePlanars();
            CurrentWalk.UpdateSequence(false);
        }
        void AnimationManager()
        {
            if (!StatorControl.MyState() ||
                Animations == null ||
                Animations.Count == 0)
                return;
            
            foreach (Sequence seq in Animations)
                seq.UpdateSequence(true);
        }
        void FeetManager()
        {
            if (//!StatorControl.MyState() ||
                IgnoreFeet.MyState() ||
                CurrentWalkSet == null)
                return;

            if (CurrentWalkSet.UpdateFootLockStatus())
            {
                ToggleFlight(!CurrentWalkSet.Locked);
                CurrentWalkSet.TogglePlaneing(Orientation.MyState());
                GUIUpdate();
            }

            foreach (Foot foot in CurrentWalkSet.Feet)
                foreach (Joint toe in foot.Toes)
                    toe.UpdateJoint(StatorTarget.MyState());
        }
        void DisplayManager()
        {
            Echo($"CockpitScreens: {MenuSystem(Screen.SPLASH, Screen.CONTROLS)}");
            Echo($"Diagnostics: {Diagnostics(Screen.DIAGNOSTICS)}");
            Echo($"MechStatus: {MechStatus(Screen.MECH_STATUS)}");
            Echo($"DebugStream: {DebugStream(Screen.DEBUG_STREAM)}");
        }
        void DroneMessageHandler()
        {
            DroneTimeOutClock--;
            DroneTimeOutClock = DroneTimeOutClock < 0 ? 0 : DroneTimeOutClock;

            if (DroneEar.HasPendingMessage)
            {

                while (DroneEar.HasPendingMessage)
                {
                    MyIGCMessage message = DroneEar.AcceptMessage(); //Always digest to clear stack.

                    try
                    {
                        ImmutableArray<double> rawData = (ImmutableArray<double>)message.Data;


                        for (int i = 0; i < MECH_IX_BUFFER.Length; i++)
                        {
                            MECH_IX_BUFFER[i] = rawData[i] == double.NaN ? 0 : rawData[i];
                        }


                        DroneCommand((MechAction)MECH_IX_BUFFER[(int)MechIx.ACTION]);

                        DroneTimeOutClock = DroneTimeOut;

                    }
                    catch
                    {
                        DebugBinStream.Append("FAIL-POINT!\n");
                    }
                }
            }

            else
            {
                for (int i = 0; i < MECH_IX_BUFFER.Length; i++)
                    MECH_IX_BUFFER[i] = 0;

            }


        }
        #endregion

        #region GLOBAL EVENTS
        static void LoadWalk(Sequence walk)
        {
            CurrentWalk = walk;

            if (CurrentWalk != null)
            {
                CurrentWalk.OverrideSet();
                CurrentWalk.ZeroSequence();
            }
        }
        void ToggleOrientation(bool enable)
        {
            CurrentWalkSet.TogglePlaneing(enable);
        }
        void ToggleFlight(bool enable)
        {
            foreach (IMyFunctionalBlock flightBlock in FlightGroup)
                flightBlock.Enabled = enable;
        }
        static void WriteCustomData()
        {
            PROG.Me.CustomData = SaveData.ToString();
        }
        bool CheckCallLimit()
        {
            TransferCount++;
            bool capped = TransferCount >= DataTransferCap;
            if (capped)
                Write(Screen.DEBUG_STATIC, "Calls Maxed!\n");
            return capped;
        }
        #endregion

        #region INITIALIZERS
        void SetupScreens()
        {
            IMyBlockGroup panelGroup = GridTerminalSystem.GetBlockGroupWithName(LCDgroupName);
            List<IMyTextSurface> buffer = new List<IMyTextSurface>();
            if (panelGroup != null)
                panelGroup.GetBlocksOfType(buffer);
            Screens.AddRange(buffer);

            for (int i = 0; i < Screens.Count; i++)
            {
                if (i < Screens.Count)
                {
                    Screens[i].ContentType = ContentType.TEXT_AND_IMAGE;
                    Screens[i].WriteText($"Index:{i}");
                }
            }
        }
        void SetupOptions()
        {
            Options = new List<Option>();
            SetupSettings();
            SetupToggles();
        }
        void SetupSettings()
        {
            StepThreshold = new Setting("Step Threshold", "How long into a keyframe (%) the mech must walk for before a foot can re-attach again.",
                0.6f, 0.05f);

            FrameThreshold = new Setting("Frame Threshold", "The maxium allowed tolerance for joint deviation between clock-triggered frame loads.",
                10f, 0.1f, 20f, 0.5f);

            MaxAcceleration = new Setting("Max Stator Acceleration", "Fastest rate (RPM) at which the joint stators will change their velocity per operation tick.",
                0.3f, 0.1f, 1f);

            MaxSpeed = new Setting("Max Stator Speed", "Top speed (RPM) that any stator will be allowed to move at.",
                10f, 0.5f, 20f);

            MouseSensitivity = new Setting("Mouse Sensitivity", "Maximum input value from the mouse",
                1f, 0.1f, 5f, 0.1f);

            SnappingIncrement = new Setting("Snapping Increment", "Amount (Deg) by which jointFrames will be adjusted per press.",
                5, 1, 45);

            FrameDurationIncrement = new Setting("Frame Duration Increment", "Amount by which the keyFrames duration will be adjusted per press",
                0.1f, 0.01f, 2f, 0.1f);

            SequenceSpeedIncrement = new Setting("Sequence ClockSpeed Increment", "Amount by which the sequences clock speeds will be adjusted per press",
                0.1f, 0.01f, 2f, 0.1f);

            Settings = new List<Setting>
            {
                StepThreshold,
                FrameThreshold,
                MaxAcceleration,
                MaxSpeed,
                MouseSensitivity,
                SnappingIncrement,
                FrameDurationIncrement,
                SequenceSpeedIncrement
            };

            Options.AddRange(Settings);
        }
        void SetupToggles()
        {
            DroneOverride = new Toggle("Drone Override", "Prevents drone controller from controlling the mech. Messages will still be recieved. Note the drone controller only works in pilot mode.", false);

            IgnoreSave = new Toggle("Ignore Save", "Prevents the CustomData of the PB from being over-written auto-matically (eg. recompile/game save)", true);
            IgnoreFeet = new Toggle("Ignore Feet", "Prevents the use of anything relating to feet, including locking and plane actuation.", false);
            AutoSave = new Toggle("Auto Save", "Will auto-matically save any changes to the mechs library after any change is made.", true);
            AutoDemo = new Toggle("Auto Demo", "Will auto-matically change the stator targets to the key-frame that gets selected", true);
            Strafing = new Toggle("Strafing", "Enables whether the mech can turn its lifted foot.", true);
            StatorTarget = new Toggle("Stator Target", "Enables whether Stators are actively fed a new target every operation tick", true);
            StatorControl = new Toggle("Stator Control", "Enables whether Stators are affected at all by the program", true);
            Descriptions = new Toggle("Descriptions", "Enables in-game descriptions of all options.", true);
            Orientation = new Toggle("Orientation", "Enables the use to orient the mech using the controller inputs of the cockpit", true, ToggleOrientation);

            Toggles = new List<Toggle>
            {
                DroneOverride,

                IgnoreSave,
                IgnoreFeet,
                AutoSave,
                AutoDemo,
                Strafing,
                StatorTarget,
                StatorControl,
                Descriptions,
                Orientation
            };

            Options.AddRange(Toggles);
        }
        void AssignFlightGroup()
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(FlightGroupName);
            if (group == null)
                return;

            group.GetBlocksOfType(FlightGroup);
        }
        void SetupController()
        {
            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType(cockpits);
            if (cockpits.Count < 1)
                return;

            CockPit = cockpits[0];
            for (int i = 0; i < CockPit.SurfaceCount; i++)
                Screens.Add(CockPit.GetSurface(i));
        }
        void SetupDroneControl()
        {
            DroneEar = IGC.RegisterBroadcastListener(DroneChannel);
        }
        #endregion

        #region ENTRY POINTS
        public Program()
        {
            PROG = this;

            try
            {
                AssignFlightGroup();
                SetupController();
                SetupDroneControl();
                SetupScreens();
                SetupOptions();

                DesignatedPlane = GridTerminalSystem.GetBlockWithName(PlaneCustomName);
                DesignatedPlane = DesignatedPlane == null ? CockPit : DesignatedPlane;
                PROG_FREQ = DEF_FREQ;
                Runtime.UpdateFrequency = PROG_FREQ;
                Initialized = true;
            }
            catch
            {
                Initialized = false;
                return;
            }

            SetGuiMode(GUIMode.MAIN);
            GUIUpdate();
        }
        public void Main(string argument, UpdateType updateSource)
        {
            Echo($"Initialized: {Initialized}");

            if (!Initialized)
                return;

            TransferCount = 0;

            if (BuildingJoints)
            {
                BuildingJoints = !LoadJoints();
                JointsBuilt = !BuildingJoints;
                return;
            }

            if (LoadingData)
            {
                int loadResult = DataLoad();
                LoadingData = loadResult == 0;
                return;
            }

            if (SavingData)
            {
                int saveResult = DataSave();
                SavingData = saveResult == 0;
                Static($"Still Saving = {saveResult}\n\n");
                return;
            }

            RuntimeArguments(argument);
            try
            {
                DroneMessageHandler();
                ControlInput();
                FeetManager();
                WalkManager();
                AnimationManager();
                DisplayManager();
            }
            catch
            {
                DebugBinStream.Append("FAIL-POINT!");
                Write(Screen.DEBUG_STREAM, DebugBinStream);
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }

            DebugBinStream.Clear(); // MUST HAPPEN!
        }
        public void Save()
        {
            if (IgnoreSave.MyState())
                return;

            WriteCustomData();
        }
        #endregion

        #endregion

        #region CONSTRUCTIONS
        static JointSet NewJointSet(string groupName, int index)
        {
            Static("Making new set...\n");
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            List<IMyBlockGroup> feet = new List<IMyBlockGroup>();
            IMyBlockGroup fullSet = null;
            GetGridBlockGroups(groups);

            foreach (IMyBlockGroup group in groups)
            {
                if (group.Name == groupName)
                    fullSet = group;

                else if (group.Name.Contains(FootSignature))
                    feet.Add(group);
            }

            if (fullSet == null)
                return null;

            RootData setRoot = new RootData(groupName, index, -1);
            JointSet newSet = new JointSet(setRoot, CockPit, groupName);
            List<IMyTerminalBlock> joints = GetBlocksFromGroup(fullSet);
            int jointIndex = 0;
            List<IMyTerminalBlock> footBuffer;
            RootData jRoot;
            JointData jData = new JointData();

            for (int f = 0; f < feet.Count; f++)
            {
                footBuffer = GetBlocksFromGroup(feet[f]);
                if (footBuffer.Count < 1)
                    continue;

                Foot newFoot = new Foot(newSet.ParentData(feet[f].Name, f));
                newSet.Feet.Add(newFoot);
                jData.FootIndex = f;
                int toeIndex = 0;

                for (int b = 0; b < footBuffer.Count; b++)
                {
                    joints.Remove(footBuffer[b]); // Remove redundancies

                    bool toe = footBuffer[b].CustomName.Contains(ToeSignature);
                    bool turn = footBuffer[b].CustomName.Contains(TurnSignature);

                    jRoot = newSet.ParentData(footBuffer[b].CustomName);
                    Static($"Name: {jRoot.Name}\n");

                    if (footBuffer[b] is IMyMechanicalConnectionBlock)
                    {
                        jRoot.MyIndex = toe ? toeIndex : jointIndex;
                        jRoot.Name = $"[{(toe ? "GRIPPER" : turn ? "TURN" : "PLANE")}]";
                        jData.Root = jRoot;
                        jData.TAG = toe ? GripTag : turn ? TurnTag : PlaneTag;

                        Static($"FootTag: {jData.TAG}\n");

                        jData.GripDirection = toe ? Math.Sign(footBuffer[b].GetValueFloat("Velocity")) : 0;

                        Joint newJoint = NewJoint((IMyMechanicalConnectionBlock)footBuffer[b], jData);
                        AppendJoint(newSet, newJoint);

                        if (toe)
                            toeIndex++;
                        else
                            jointIndex++;
                    }
                    if (footBuffer[b] is IMyLandingGear)
                    {
                        jRoot.MyIndex = newFoot.Magnets.Count;
                        jRoot.Name = "[MAGNET]";

                        Magnet newMagnet = new Magnet(jRoot, (IMyLandingGear)footBuffer[b], f); //NewMagnet((IMyLandingGear)footBuffer[b], jRoot, f);
                        AppendMagnet(newSet, newMagnet);
                    }
                }
            }

            jData.FootIndex = -1;
            jData.GripDirection = 0;

            for (int j = 0; j < joints.Count; j++)
            {
                if (joints[j] is IMyMechanicalConnectionBlock)
                {
                    jRoot = newSet.ParentData("[JOINT]", jointIndex);
                    jData.Root = jRoot;
                    jData.TAG = JointTag;

                    Static($"GenericTag: {jData.TAG}\n");

                    jointIndex++;
                    newSet.Joints.Add(NewJoint((IMyMechanicalConnectionBlock)joints[j], jData));
                }
            }
            return newSet;
        }
        static Joint NewJoint(IMyMechanicalConnectionBlock jointBlock, JointData data)
        {
            Joint newJoint = null;

            if (jointBlock is IMyPistonBase)
                newJoint = new Piston((IMyPistonBase)jointBlock, data);

            if (jointBlock is IMyMotorStator)
            {
                if (jointBlock.BlockDefinition.ToString().Contains("Hinge"))
                    newJoint = new Hinge((IMyMotorStator)jointBlock, data);
                else
                    newJoint = new Rotor((IMyMotorStator)jointBlock, data);
            }

            if (newJoint != null)
                JointBin.Add(newJoint);

            return newJoint;
        }
        static KeyFrame NewKeyFrame(RootData root, JointSet set)
        {
            KeyFrame newKFrame = new KeyFrame(root);

            for (int i = 0; i < set.Joints.Count; i++)
            {
                if (set.Joints[i] is Piston)
                    continue;

                RootData jfRoot = newKFrame.ParentData(set.Joints[i].Name, i);
                newKFrame.Jframes.Add(new JointFrame(jfRoot, set.GetJoint(i), Snapping));
            }

            return newKFrame;
        }
        static void AppendMagnet(JointSet set, Magnet magnet)
        {
            if (magnet == null ||
                set == null)
                return;

            Foot foot = set.GetFoot(magnet.FootIndex);
            if (foot != null)
                foot.Magnets.Add(magnet);
        }
        static void AppendJoint(JointSet set, Joint joint)
        {
            if (joint == null ||
                set == null)
                return;

            if (joint.TAG != GripTag)
                set.Joints.Add(joint);

            Foot foot = set.GetFoot(joint.FootIndex);

            if (foot != null)
            {
                if (joint.TAG == GripTag)
                    foot.Toes.Add(joint);

                if (joint.TAG == PlaneTag || joint.TAG == TurnTag)
                    foot.Planars.Add(joint);
            }
        }
        #endregion

        #region LOAD
        int DataLoad()
        {
            JsetBin.Clear();

            string[] load = Me.CustomData.Split('\n');

            Static($"Load Lines Length: {load.Length}\n");

            for (int i = LoadCustomDataIndex; i < load.Length; i++)
            {
                LoadCustomDataIndex = i;
                if (CheckCallLimit())
                    return 0;

                try
                {
                    Static("next load line...\n");
                    string opCode = load[i].Split(':')[0];
                    Static($"op-code: {opCode}\n");

                    switch (opCode)
                    {
                        // OPTIONS //
                        case OptionsTag:
                            Static("options:");
                            LoadToggles(load[i]);
                            Static(" loaded!\n");
                            break;

                        case SettingsTag:
                            Static("Settings:");
                            LoadSettings(load[i]);
                            Static(" loaded!\n");
                            break;

                        // BLOCKS //
                        case FootTag:
                            Static("constructing foot...\n");
                            Foot newFoot = LoadFoot(load[i]);//new Foot(this);
                            //newFoot.Load();
                            if (newFoot.BUILT)
                            {
                                FeetBuffer.Add(newFoot);
                                Static("foot constructed!\n");
                            }
                            else
                                Static("foot construction failed!\n");
                            break;

                        case JointSetTag:

                            if (JointsBuilt)
                            {
                                Static("completing set...\n");
                                SetBuffer.Sequences.AddRange(sequenceBuffer);
                                FeetBuffer.Clear();
                                sequenceBuffer.Clear();
                                JointsBuilt = false;
                                JsetBin.Add(SetBuffer);
                                SetBuffer = null;
                                break;
                            }

                            Static("constructing set...\n");
                            BuildingJoints = true;
                            LoadJointIndex = 0;
                            LoadSetDataBuffer = load[i];
                            LoadCustomDataIndex++;
                            return 0;

                        // SEQUENCES //
                        case JframeTag:
                            Static("jFrame:");
                            JointFrame newJframe = LoadJointFrame(load[i], SetBuffer.GetJoint(jFrameBuffer.Count));
                            if (newJframe.BUILT)
                            {
                                jFrameBuffer.Add(newJframe);
                                Static(" added!:\n");
                            }
                            else
                                Static(" failed!:\n");
                            break;

                        case KframeTag:
                            Static("kFrame:");
                            KeyFrame newKframe = LoadKeyFrame(load[i], jFrameBuffer);
                            if (newKframe.BUILT)
                            {
                                kFrameBuffer.Add(newKframe);
                                jFrameBuffer.Clear();
                                Static(" added!:\n");
                            }
                            else
                                Static(" failed!:\n");
                            break;

                        case ZframeTag:
                            Static("Zero Frame:");
                            KeyFrame zeroFrame = LoadKeyFrame(load[i], jFrameBuffer);
                            if (zeroFrame.BUILT)
                            {
                                SetBuffer.ZeroFrame = zeroFrame;
                                jFrameBuffer.Clear();
                                Static(" added!:\n");
                            }
                            else
                                Static(" failed!:\n");
                            break;

                        case SeqTag:
                            Static("sequence:\n");
                            Sequence newSeq = LoadSequence(load[i], SetBuffer, kFrameBuffer);
                            if (newSeq.BUILT)
                            {
                                sequenceBuffer.Add(newSeq);
                                kFrameBuffer.Clear();
                                Static(" added!:\n");
                            }
                            else
                                Static(" failed!:\n");
                            break;

                        default:
                            break;
                    }
                }
                catch
                {
                    Static("Fail Point!\n");
                    JsetBin.Clear();
                    return -1;
                }
            }

            Startup();
            return 1;
        }
        void LoadSettings(string input)
        {
            string[] data = input.Split(':');
            for (int i = 0; i < Settings.Count; i++)
            {
                try { Settings[i].Change(float.Parse(data[i + 1])); }
                catch { }
            }

        }
        void LoadToggles(string input)
        {
            string[] data = input.Split(':');
            for (int i = 0; i < Toggles.Count; i++)
            {
                try { Toggles[i].Change(bool.Parse(data[i + 1])); }
                catch { }
            }
        }
        bool LoadJoints()
        {
            if (!SetBuffered)
            {
                SetBuffer = LoadJointSet(LoadSetDataBuffer, DesignatedPlane, FeetBuffer);
                if (SetBuffer == null)
                {
                    Static("Set load failed!\n");
                    return true;
                }

                BlockBuffer = GetBlocksFromGroup(SetBuffer.GroupName);
                if (BlockBuffer == null || BlockBuffer.Count < 1)
                {
                    Static("Nothing to load!\n");
                    return true;
                }

                SetBuffered = true;
            }

            SetBuffer.Sort();

            for (int i = LoadJointIndex; i < BlockBuffer.Count; i++)
            {
                Echo($"Buffering:{i}");
                LoadJointIndex = i;
                if (CheckCallLimit())
                    return false;

                Static($"Loading joint: {BlockBuffer[i].CustomName} || {BlockBuffer[i].CustomData}\n");

                if (BlockBuffer[i] is IMyLandingGear)
                {
                    Magnet newMagnet = LoadMagnet((IMyLandingGear)BlockBuffer[i]);
                    AppendMagnet(SetBuffer, newMagnet);
                }

                if (BlockBuffer[i] is IMyPistonBase ||
                    BlockBuffer[i] is IMyMotorStator)
                {
                    Joint newJoint = LoadJoint((IMyMechanicalConnectionBlock)BlockBuffer[i]);
                    AppendJoint(SetBuffer, newJoint);
                }
            }

            SetBuffered = false;
            return true;
        }
        void Startup()
        {
            SavingData = !IgnoreSave.MyState();

            if (JsetBin.Count < 1 ||
                JsetBin[0] == null)
                return;

            Static("Starting up...\n");

            CurrentWalkSet = GetJointSet(0);

            CurrentWalkSet.SyncJoints();
            CurrentWalkSet.SnapShotPlane();
            CurrentWalkSet.InitFootStatus();
            CurrentWalkSet.TogglePlaneing(Orientation.MyState());

            ToggleFlight(!CurrentWalkSet.Locked);

            if (CurrentWalkSet.Sequences == null ||
                CurrentWalkSet.Sequences.Count < 1)
                return;

            Static("Loading walk...\n");

            LoadWalk(CurrentWalkSet.GetSequence(0));
        }

        JointSet LoadJointSet(string input, IMyTerminalBlock plane, List<Foot> footBuffer) { return new JointSet(input, plane, footBuffer); }
        Joint LoadJoint(IMyMechanicalConnectionBlock jointBlock)
        {
            Joint loadedJoint = null;

            if (jointBlock is IMyPistonBase)
                loadedJoint = new Piston((IMyPistonBase)jointBlock);

            if (jointBlock is IMyMotorStator)
            {
                if (jointBlock.BlockDefinition.ToString().Contains("Hinge"))
                    loadedJoint = new Hinge((IMyMotorStator)jointBlock);
                else
                    loadedJoint = new Rotor((IMyMotorStator)jointBlock);
            }

            if (loadedJoint != null)
                JointBin.Add(loadedJoint);

            return loadedJoint;
        }
        Magnet LoadMagnet(IMyLandingGear gear) { return new Magnet(gear); }
        Foot LoadFoot(string input) { return new Foot(input); }
        Sequence LoadSequence(string input, JointSet set, List<KeyFrame> buffer) { return new Sequence(input, set, buffer); }
        KeyFrame LoadKeyFrame(string input, List<JointFrame> buffer) { return new KeyFrame(input, buffer); }
        JointFrame LoadJointFrame(string input, Joint joint) { return new JointFrame(input, joint); }
        #endregion

        #region SAVE

        int DataSave()
        {
            if (!SaveInit)
                SaveInitializer();

            int setsResult = SaveStack(SaveSet, eRoot.JSET, JsetBin);
            if (setsResult != 1)
                return setsResult;

            SaveInit = false;
            return 1;
        }
        int SaveStack(SaveJob job, eRoot element, List<Root> roots = null)
        {
            Static($"Enter Stack: {element}\n");

            if (roots == null)
                return job(); // Let job handle fail condition

            if (Saving(element) >= roots.Count) // Finished save loop
            {
                Static($"Skip Stack: {element}\n");
                return 1;
            }


            for (int index = Saving(element); index < roots.Count; index++)
            {
                SetSaveIndex(element, index);
                if (CheckCallLimit()) // Call stack limiter
                {
                    Static($"Call break {element}\n");
                    return 0;
                }


                int result = job();
                if (result != 1)
                {
                    Static($"Save break {element} : {result}\n");
                    return result; // Save break
                }


                if (index == roots.Count - 1)
                    Static($"Save Stack {element} Complete!\n");
            }

            IncrementSaveIndex(element);
            Static($"Exit Stack: {element}\n");
            return 1;
        }
        int SaveSet()
        {
            JointSet set = GetSavingSet();
            if (set == null)
                return -1;

            if (!JointsSaved)
            {
                int feetResult = SaveStack(SaveFoot, eRoot.FOOT, set.Feet);
                if (feetResult != 1)
                    return feetResult;

                int jointsResult = SaveStack(SaveJoint, eRoot.JOINT, set.Joints);
                if (jointsResult != 1)
                    return jointsResult;

                SaveData.Append($"{set.SaveData()}\n");
                JointsSaved = true;
            }

            {
                int seqResult = SaveStack(SaveSequence, eRoot.SEQUENCE, set.Sequences);
                if (seqResult != 1)
                    return seqResult;
            }

            int zeroFrameResult = SaveStack(SaveZeroFrame, eRoot.K_FRAME);
            if (zeroFrameResult != 1)
            {
                set.GenerateZeroFrame();
                //return 0;
            }

            SaveData.Append($"{JointSetTag}:{set.Name}:END OF SAVE\n");

            ResetSaveIndex(eRoot.FOOT);
            ResetSaveIndex(eRoot.JOINT);
            ResetSaveIndex(eRoot.SEQUENCE);

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

            SaveData.Append($"{foot.SaveData()}\n");

            ResetSaveIndex(eRoot.TOE);
            ResetSaveIndex(eRoot.MAGNET);

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

            int frameResult = SaveStack(SaveAnimFrame, eRoot.K_FRAME, seq.Frames);
            if (frameResult != 1)
                return frameResult;

            SaveData.Append($"{seq.SaveData()}\n");

            ResetSaveIndex(eRoot.K_FRAME);

            return 1;
        }
        int SaveZeroFrame()
        {
            return SaveKeyFrame(GetSavingSet().ZeroFrame);
        }
        int SaveAnimFrame()
        {
            return SaveKeyFrame(GetSavingKeyFrame());
        }
        int SaveKeyFrame(KeyFrame frame)
        {
            //KeyFrame frame = zeroFrame ?  : GetSavingKeyFrame();
            if (frame == null)
                return -1;

            int frameResult = SaveStack(SaveJointFrame, eRoot.J_FRAME, frame.Jframes);
            if (frameResult != 1)
                return frameResult;

            SaveData.Append($"{frame.SaveData()}\n");

            ResetSaveIndex(eRoot.J_FRAME);

            return 1;
        }
        int SaveJointFrame()
        {
            JointFrame frame = GetSavingJointFrame();
            if (frame == null)
                return -1;

            SaveData.Append($"{frame.SaveData()}\n");
            return 1;
        }
        static int Saving(eRoot root)
        {
            return SaveObjectIndex[(int)root];
        }

        void SaveInitializer()
        {
            SaveData.Clear();

            SaveToggles();
            SaveSettings();
            ResetSaveIndex(eRoot.JSET);

            Static("Save Init complete!\n");

            JointsSaved = false;
            SaveInit = true;
        }
        void SaveSettings()
        {
            SaveData.Append($"{SettingsTag}");
            foreach (Setting setting in Settings)
                SaveData.Append($":{setting.MyValue()}");
            SaveData.Append("\n");
        }
        void SaveToggles()
        {
            SaveData.Append($"{OptionsTag}");
            foreach (Toggle option in Toggles)
                SaveData.Append($":{option.MyState()}");
            SaveData.Append("\n");
        }
        void IncrementSaveIndex(eRoot root)
        {
            SaveObjectIndex[(int)root]++;
        }
        void SetSaveIndex(eRoot root, int index)
        {
            SaveObjectIndex[(int)root] = index;
        }
        void ResetSaveIndex(eRoot root)
        {
            SaveObjectIndex[(int)root] = 0;
        }
        #endregion

        #region GETTERS

        static GUIMode GetCurrentGuiMode() { return PROG._CurrentGUIMode; }
        static TimeSpan GetGridTimeSinceLastRun() { return PROG.Runtime.TimeSinceLastRun; }
        static void GetGridBlockGroups(List<IMyBlockGroup> groups) { PROG.GridTerminalSystem.GetBlockGroups(groups); }
        static void GetGridBlocksOfType<T>(List<T> blocks) where T : class { PROG.GridTerminalSystem.GetBlocksOfType(blocks); }
        static IMyTextSurface GetSurface(Screen screen) { try { return Screens[(int)screen]; } catch { return null; } }
        static List<IMyTerminalBlock> GetBlocksFromGroup(string groupName)
        {
            if (groupName == null)
                return null;

            IMyBlockGroup group = PROG.GridTerminalSystem.GetBlockGroupWithName(groupName);
            if (group == null)
                return null;

            return GetBlocksFromGroup(group);
        }
        static List<IMyTerminalBlock> GetBlocksFromGroup(IMyBlockGroup group)
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);
            return blocks;
        }
        static JointSet GetJointSet(int index) { return index < 0 || index > JsetBin.Count ? null : (JointSet)JsetBin[index]; }
        static JointSet GetSavingSet() { return GetJointSet(Saving(eRoot.JSET)); }
        static JointSet GetSelectedSet()
        {
            int? check = GetCurrentPage()?.SelectedIndex(eRoot.JSET);
            return check.HasValue ? GetJointSet(check.Value) : null;
        }
        static Animation GetSelectedAnim() { return GetCurrentPage()?.SelectedRoot() as Animation; }
        static Sequence GetSavingSequence()
        {
            JointSet set = GetSavingSet();
            if (set == null)
                return null;

            return set.GetSequence(Saving(eRoot.SEQUENCE));
        }
        static Sequence GetSelectedSequence()
        {
            JointSet set = GetSelectedSet();
            if (set == null)
                return null;
            int? check = GetCurrentPage()?.SelectedIndex(eRoot.SEQUENCE);
            return check.HasValue ? set.GetSequence(check.Value) : null;
        }
        static KeyFrame GetSavingKeyFrame()
        {
            Sequence seq = GetSavingSequence();
            if (seq == null)
                return null;
            return seq.GetKeyFrame(Saving(eRoot.K_FRAME));
        }
        static KeyFrame GetSelectedKeyFrame()
        {
            Sequence seq = GetSelectedSequence();
            if (seq == null)
                return null;
            int? check = GetCurrentPage()?.SelectedIndex(eRoot.K_FRAME);
            return check.HasValue ? seq.GetKeyFrame(check.Value) : null;
        }
        static JointFrame GetSavingJointFrame()
        {
            KeyFrame frame = GetSavingKeyFrame();
            if (frame == null)
                return null;
            return frame.GetJointFrameByFrameIndex(Saving(eRoot.J_FRAME));
        }
        static JointFrame GetSelectedJointFrame()
        {
            KeyFrame frame = GetSelectedKeyFrame();
            if (frame == null)
                return null;
            int? check = GetCurrentPage()?.SelectedIndex(eRoot.J_FRAME);
            return check.HasValue ? frame.GetJointFrameByFrameIndex(check.Value) : null;
        }
        static Foot GetSavingFoot()
        {
            JointSet set = GetSavingSet();
            if (set == null)
                return null;
            return set.GetFoot(Saving(eRoot.FOOT));
        }
        static Joint GetSavingJoint()
        {
            JointSet set = GetSavingSet();
            if (set == null)
                return null;
            return set.GetJoint(Saving(eRoot.JOINT));
        }
        static Joint GetSavingToe()
        {
            Foot foot = GetSavingFoot();
            if (foot == null)
                return null;
            return foot.GetToe(Saving(eRoot.TOE));
        }
        static Magnet GetSavingMagnet()
        {
            Foot foot = GetSavingFoot();
            if (foot == null)
                return null;
            return foot.GetMagnet(Saving(eRoot.MAGNET));
        }
        static Page GetCurrentPage()
        {
            return Pages[GetCurrentGuiMode()];
        }
        
        #endregion

    }
}
