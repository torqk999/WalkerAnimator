using Sandbox.Game.EntityComponents;
//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
     
:&:True:True:True:True:True:True:True:True:True:True
:#:0.6:4.999998:1:5:5:5
LEFT[FOOT]:@:0:0:-1
RIGHT[FOOT]:@:1:0:-1
ALL_JOINTS:S:0:-1:
:%1:0:0:0
:%1:1:0:5
:%1:2:0:259
:%1:3:0:99
:%1:4:0:0
:%1:5:0:0
:%1:6:0:24
:%1:7:0:1
:%1:8:0:24
:%1:9:0:0
Frame A:%0:0:0:1
:%1:0:1:0
:%1:1:1:0
:%1:2:1:314
:%1:3:1:45
:%1:4:1:0
:%1:5:1:0
:%1:6:1:0
:%1:7:1:45
:%1:8:1:45
:%1:9:1:0
Frame B:%0:1:0:1
:%1:0:2:0
:%1:1:2:24
:%1:2:2:0
:%1:3:2:24
:%1:4:2:0
:%1:5:2:0
:%1:6:2:270
:%1:7:2:137
:%1:8:2:45
:%1:9:2:0
Frame C:%0:2:0:1
:%1:0:3:0
:%1:1:3:335
:%1:2:3:0
:%1:3:3:334
:%1:4:3:0
:%1:5:3:0
:%1:6:3:259
:%1:7:3:99
:%1:8:3:359
:%1:9:3:0
Frame D:%0:3:0:1
:%1:0:4:0
:%1:1:4:305
:%1:2:4:305
:%1:3:4:359
:%1:4:4:0
:%1:5:4:0
:%1:6:4:314
:%1:7:4:44
:%1:8:4:359
:%1:9:4:0
Frame E:%0:4:0:1
:%1:0:5:0
:%1:1:5:315
:%1:2:5:222
:%1:3:5:84
:%1:4:5:0
:%1:5:5:0
:%1:6:5:335
:%1:7:5:0
:%1:8:5:335
:%1:9:5:0
Frame F:%0:5:0:1
walking:$:0:0:0.01
ALL_JOINTS:S:LOAD FINISHED 

     
     */



    partial class Program : MyGridProgram
    {
        static Program PROG;
        public Program()
        {
            Echo("Ctor");
            try
            {
                PROG = this;
            }
            catch (Exception ex)
            {
                Echo($"{ex.Message}");
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    Echo($"{ex.Message}");
                }
                return;
            }

            try
            {
                AssignFlightGroup();
                SetupController();
                SetupDroneControl();
                SetupScreens();
                SetupOptions();

                DesignatedPlane = GridTerminalSystem.GetBlockWithName(PlaneCustomName);
                DesignatedPlane = DesignatedPlane == null ? CockPit : DesignatedPlane;
                FREQUENCY = DEF_FREQ;
                Runtime.UpdateFrequency = FREQUENCY;
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
            Echo("Main Proc:");
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
                Static($"Still Saving = {SavingData}\n\n");
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

        const string LCDgroupName = "LCDS";
        const string FlightGroupName = "FLIGHT";
        const string FootSignature = "[FOOT]";
        const string ToeSignature = "[TOE]";
        const string TurnSignature = "[TURN]";
        const string DroneChannel = "MECH";
        const string PlaneCustomName = "EYEBALL";

        #endregion

        #region INTERNAL CONSTS

        // need migrating //
        static bool CapLines = true;
        static bool Snapping = true;
        ////////////////////

        static UpdateFrequency DEF_FREQ = UpdateFrequency.Update1;
        static RootSort SORT = new RootSort();


        const string VersionNumber = "0.6.1";

        const string OptionsTag = "&";
        const string SettingsTag = "#";
        const string FootTag = "@";
        const string SeqTag = "$";
        const string ZframeTag = "%Z";
        const string KframeTag = "%0";
        const string JframeTag = "%1";
        const string JointSetTag = "S";

        const string UnusedTag = "u";
        const string JointTag = "J";
        const string TurnTag = "T";
        const string PlaneTag = "P";
        const string MagnetTag = "M";
        const string GripTag = "G";

        static readonly List<string> JointTags = new List<string> {
            UnusedTag,
            JointTag,
            TurnTag,
            PlaneTag,
            GripTag
        };
        static readonly List<string> MagnetTags = new List<string>{
            UnusedTag,
            MagnetTag
        };

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
        const int JointParamCount = 7;
        const int MagnetParamCount = 5;

        const double MIN_VEL = .1;
        const double DEG2VEL = .5;
        const double RAD2DEG = 180 / Math.PI;
        const double SAFETY = Math.PI / 4;
        #endregion

        #region RESOURCES

        #region MECHANICAL

        static List<IMyFunctionalBlock> FlightGroup = new List<IMyFunctionalBlock>();
        static List<Root> JointBin = new List<Root>();
        static List<Root> MagnetBin = new List<Root>();
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

            {GUIMode.ASSIGN_JOINTS, Assignment },
            {GUIMode.ASSIGN_MAGNETS, Assignment },

            {GUIMode.EDIT, Library      },
            {GUIMode.CREATE, Library    },

            {GUIMode.PILOT, Controls    },
            {GUIMode.OPTIONS, OptionMenu},
        };

        #endregion

        #endregion

        #region LOAD/SAVE

        static bool SavingData = false;
        static bool SaveInit = true;
        static bool JointsSaved = false;
        static bool SequencesSaved = false;

        static bool LoadingData = true;
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


        static Sequence CurrentWalk;
        static JointSet CurrentWalkSet;
        static List<Sequence> Animations = new List<Sequence>();
        static UpdateFrequency FREQUENCY;
        static GUIMode CurrentGUIMode;
        static Vector3D InputRotationBuffer;
        static double InputTurnBuffer = 0;

        static int MoveBuffer = 0;
        static int LastMechWalkInput;
        static int[] LastMenuInput = new int[4];

        static bool WAIT;
        bool Initialized = false;
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

        #region DELEGATES
        public delegate int SaveJob();
        public delegate void ToggleUpdate(bool state);
        public delegate void SettingUpdate(float setting);
        public delegate string[] PageBuilder();
        public delegate void ButtonEvent();

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

        static readonly string[] Cursor = { " ", ">", "[", "]" };

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
        static string BuildCursor(bool selected)
        {
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
            CurrentGUIMode = mode;
            Pages[CurrentGUIMode].SetMode(mode);
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
                StartSave();

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
                        DisplayManagerBuilder.Append($"Planar {joint.Name()} rotation axis: {joint.ReturnRotationAxis()}\n");
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

                    $"CurrentWalkSet: {CurrentWalkSet.Name()}\n" +
                    $"-LockedFootIndex: {CurrentWalkSet.LockedIndex}\n\n" +

                    $"CurrentWalk: {CurrentWalk.Name()}\n" +
                    $"-ClockState: {CurrentWalk.CurrentClockMode}\n" +
                    $"-ClockTime: {CurrentWalk.CurrentClockTime}\n" +
                    $"-StepDelay: {CurrentWalk.StepDelay}\n" +
                    $"-FrameLength: {CurrentWalk.CurrentFrames[0].MySetting.MyValue()}\n" +
                    $"-LoadedFrames: {CurrentWalk.CurrentFrames[0].Name()} || {CurrentWalk.CurrentFrames[1].Name()}\n");
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
                    MoveBuffer = (int)MECH_IX_BUFFER[(int)MechIx.MOVE_Z];

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
                    MoveBuffer = -(int)CockPit.MoveIndicator.Z;
                }

                DebugBinStream.Append($"TurnBuffer(f): {InputTurnBuffer}\n");

                if (LastMechWalkInput != MoveBuffer)
                {
                    LastMechWalkInput = MoveBuffer;
                    CurrentWalk.SetClockMode((ClockMode)MoveBuffer);
                }

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
            if (IgnoreFeet.MyState() ||
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

                        Joint newJoint = NewJoint(jData, (IMyMechanicalConnectionBlock)footBuffer[b]);
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

                        Magnet newMagnet = NewMagnet(jRoot, (IMyLandingGear)footBuffer[b], f);
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
                    newSet.Joints.Add(NewJoint(jData, (IMyMechanicalConnectionBlock)joints[j]));
                }
            }
            return newSet;
        }
        static Joint NewJoint(JointData data, IMyMechanicalConnectionBlock jointBlock)
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
        static KeyFrame NewKeyFrame(AnimationData data, JointSet set)
        {
            KeyFrame newKFrame = new KeyFrame(data);

            for (int i = 0; i < set.Joints.Count; i++)
            {
                if (set.Joints[i] is Piston)
                    continue;

                RootData jfRoot = newKFrame.ParentData("", i);
                AnimationData jfData = new AnimationData(jfRoot, FrameLengthDef);
                newKFrame.Jframes.Add(new JointFrame(jfData, set.GetJoint(i), Snapping));
            }

            return newKFrame;
        }
        static Magnet NewMagnet(RootData root, IMyLandingGear gear, int footIndex)
        {
            Magnet newMagnet = new Magnet(root, gear, footIndex);
            MagnetBin.Add(newMagnet);
            return newMagnet;
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
                    string lineTag = load[i].Split(':')[(int)PARAM.TAG];
                    Static($"TAG: {lineTag}\n");

                    switch (lineTag)
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

                BlockBuffer = GetBlocksFromGroup(SetBuffer.Name());
                if (BlockBuffer == null || BlockBuffer.Count < 1)
                {
                    Static("Nothing to load!\n");
                    return true;
                }

                SetBuffered = true;
            }

            SetBuffer.Sort(eRoot.JOINT);

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

                if (BlockBuffer[i] is IMyMechanicalConnectionBlock)
                {
                    Joint newJoint = LoadJoint((IMyMechanicalConnectionBlock)BlockBuffer[i]);
                    AppendJoint(SetBuffer, newJoint);
                }
            }

            /*>>>*///Initialized = false;

            SetBuffered = false;
            return true;
        }
        void Startup()
        {
            if (!IgnoreSave.MyState())
                StartSave();

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

        static JointSet LoadJointSet(string input, IMyTerminalBlock plane, List<Foot> footBuffer) { return new JointSet(input, plane, footBuffer); }
        static Joint LoadJoint(IMyMechanicalConnectionBlock jointBlock)
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
        static Magnet LoadMagnet(IMyLandingGear gear) { Magnet loadedMagnet = new Magnet(gear); MagnetBin.Add(loadedMagnet); return loadedMagnet; }
        static Foot LoadFoot(string input) { return new Foot(input); }
        static Sequence LoadSequence(string input, JointSet set, List<KeyFrame> buffer) { return new Sequence(input, set, buffer); }
        static KeyFrame LoadKeyFrame(string input, List<JointFrame> buffer) { return new KeyFrame(input, buffer); }
        static JointFrame LoadJointFrame(string input, Joint joint) { return new JointFrame(input, joint); }
        #endregion

        #region SAVE

        static void StartSave()
        {
            SavingData = true;
            SaveInit = false;
            JointsSaved = false;
            SequencesSaved = false;
        }
        int DataSave()
        {
            if (!SaveInit)
                SaveInitializer();

            int setsResult = SaveStack(SaveSet, eRoot.JSET, JsetBin);

            if (setsResult != 1)
                return setsResult;

            return 1;
        }
        int SaveStack(SaveJob job, eRoot element = eRoot.DEFAULT, List<Root> roots = null)
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

            if(!SequencesSaved)
            {
                int seqResult = SaveStack(SaveSequence, eRoot.SEQUENCE, set.Sequences);
                if (seqResult != 1)
                    return seqResult;

                SequencesSaved = true;
            }

            int zeroFrameResult = SaveStack(SaveZeroFrame, eRoot.Z_FRAME);
            if (zeroFrameResult != 1)
            {
                set.GenerateZeroFrame();
                ResetSaveIndex(eRoot.J_FRAME);
                return 0;
            }

            SaveData.Append($"{set.ZeroFrame.SaveData()}\n");

            SaveData.Append($"{JointSetTag}:{set.Name()}:END OF SAVE\n");

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
            {
                Static("No frame to save!\n");
                return -1;
            }
                
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

            SaveInit = true;
        }
        void SaveSettings()
        {
            SaveData.Append($":{SettingsTag}");
            foreach (Setting setting in Settings)
                SaveData.Append($":{setting.MyValue()}");
            SaveData.Append("\n");
        }
        void SaveToggles()
        {
            SaveData.Append($":{OptionsTag}");
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

        static GUIMode GetCurrentGuiMode() { return CurrentGUIMode; }
        static TimeSpan GetGridTimeSinceLastRun() { return PROG.Runtime.TimeSinceLastRun; }
        static void GetGridBlockGroups(List<IMyBlockGroup> groups) { PROG.GridTerminalSystem.GetBlockGroups(groups); }
        static void GetGridBlocksOfType<T>(List<T> blocks) where T : class { blocks = new List<T>(); PROG.GridTerminalSystem.GetBlocksOfType(blocks); }
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
            if (SequencesSaved)
            {
                JointSet savingSet = GetSavingSet();
                if (savingSet == null)
                    return null;

                return savingSet.ZeroFrame;
            }

            Sequence seq = GetSavingSequence();
            if (seq == null)
                return null;
            return  seq.GetKeyFrame(Saving(eRoot.K_FRAME));
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
