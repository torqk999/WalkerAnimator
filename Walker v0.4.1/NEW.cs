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

     */



    partial class Program : MyGridProgram
    {
        #region LOL

        /*
        string lazyBoiDebugBin = string.Empty;
        IMyTextSurface whatWentWrongScreen;

        void MyBuggyFunction(string usualArg, ref string ohLawdHeDebuggin)
        {
            try
            {
                ohLawdHeDebuggin += "You have stepped into your fanctian!\n";

                Object undefined = null;
                // Do Stuff

                ohLawdHeDebuggin += $"yo watch this ma! {undefined}\n";

                ohLawdHeDebuggin += $"Did you make it here yet?\n";
            }
            catch
            {
                ohLawdHeDebuggin += "FAILED! :D\n";
            }
        }

        void Program()
        {
            whatWentWrongScreen = Me.GetSurface(0);
            whatWentWrongScreen.ContentType = ContentType.TEXT_AND_IMAGE;
            whatWentWrongScreen.WriteText("");
        }

        public void Main(string argument, UpdateType updateSource)
        {
            whatWentWrongScreen.WriteText(lazyBoiDebugBin);
            lazyBoiDebugBin = string.Empty; // FOR THE LOVE OF GOD DO NOT REMOVE THIS LINE!!
        }
        */

        #endregion

        //const double TWO_PI = (2 * Math.PI);
        //const double ONE_RPM = TWO_PI / 60;
        //const double ROTOR_SPEED_CHK = TWO_PI * 0.9d;
        //const float Rad2Pi = (float)(180 / Math.PI);

        /*            void ReIndexJoints()
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
                    public void UpdateForce(float force)
            {
                if (Jtype == JointType.PISTON)
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

        bool SequenceFramePrimer(IMyTextSurface panel)
        {
            if (panel == null)
                return false;

            DisplayManagerBuilder.Clear();

            try
            {
                DisplayManagerBuilder.Append("Sample Walk Frames:\n");
                for (int i = 0; i < CurrentWalk.Frames.Count; i++)
                {
                    DisplayManagerBuilder.Append($"{CurrentWalk.Frames[i].Name}:\n");

                    for (int j = 0; j < CurrentWalk.Frames[i].Jframes.Length; j++)
                        DisplayManagerBuilder.Append($"- {CurrentWalk.Frames[i].Jframes[j].Joint.Name} : {CurrentWalk.Frames[i].Jframes[j].LerpPoint}\n");

                    DisplayManagerBuilder.Append("\n");
                }
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }
            panel.WriteText(DisplayManagerBuilder);
            return true;
        }

        bool JointLiveStatus(IMyTextSurface panel)
        {
            if (panel == null)
                return false;

            DisplayManagerBuilder.Clear();

            try
            {
                DisplayManagerBuilder.Append($"CurrentSet Joints(Name:Pos:Vel) | {CurrentWalkSet.Joints.Length}\n");

                for (int i = 0; i < CurrentWalkSet.Joints.Length; i++)
                {
                    DisplayManagerBuilder.Append($"{CurrentWalkSet.Joints[i].Name} : {CurrentWalkSet.Joints[i].ReturnCurrentStatorPosition()} : {CurrentWalkSet.Joints[i].TargetVelocity}\n");
                }
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }
            panel.WriteText(DisplayManagerBuilder);
            return true;
        }

        void TogglePlaneDirection(int dir)
        {
            try
            {
                foreach (Foot foot in CurrentWalkSet.Feet)
                    if (foot.Locked && foot.Planes[dir] != null)
                        foot.Planes[dir].AuxDirection *= -1;
            }
            catch
            {
                DebugBinStatic.Append($"Plane direction {dir} toggle failed");
            }
        }
         */

        #region MAIN
        const string SampleLegsGroupName = "ALL_JOINTS";
        const string CockpitName = "PILOT";
        const string LCDgroupName = "LCDS";

        const float Threshold = .02f;
        const float VScalar = .5f;
        const float MaxAccel = 0.3f;
        const float MaxSpeed = 6f;
        const float ClockIncrmentMag = 0.0005f;
        const float ClockSpeedDef = 0.005f;
        const float ClockSpeedMin = 0.001f;
        const float ClockSpeedMax = 0.020f;
        const float TriggerCap = 0.6f;
        const float LookScalar = 0.005f;

        const int SaveBlockCountSize = 6;
        const double RAD2DEG = 180 / Math.PI;

        IMyCockpit Control;
        Vector3 RotationBuffer;

        List<JointSet> JsetBin = new List<JointSet>();
        List<Sequence> Animations;

        Sequence CurrentWalk;
        JointSet CurrentWalkSet;

        ClockMode AnimationState;
        ClockMode OldWalkState;

        bool bIgnoreSave = true;
        bool bForceSave = false;
        bool bInitialized = false;
        bool bWalking = false;
        bool bPlaneing = false;
        bool bSnapping = true;
        bool bLoaded = false;
        bool bAutoDemo = false;
        bool bStatorTarget = true;
        bool bStatorControl = true;

        float Snapping = 5;
        int LastMechInput;
        int[] LastLibraryInput = new int[2];

        #region GUI VARS
        GUIMode CurrentGUIMode = GUIMode.MAIN;
        GUILayer CurrentGUILayer = GUILayer.JSET;

        bool bCapLines = true;
        int CursorIndex = 0;
        int LineBufferSize = 6;
        int[] SelObjIndex = new int[] { 0, 0, 0, 0 };
        int SelectedLineIndex = 0;

        static readonly string[] MainMenuButtons =
        {
            "Info",
            "Library",
            "Controls",
            "Options"
        };
        static readonly string[] InfoMenuButtons =
        {
            "ScrollUp",
            "ScrollDown",
            "Main Menu"
        };
        static readonly string[] LibraryMenuButtons =
        {
            "ChangeSnapping",
            "Decrement",
            "Increment",
            "LoadItem",
            "NewItem",
            "DeleteItem",
            "EditItem",
            "MainMenu",
            "UpList",
            "DownList",
            "UpDirectory",
            "OpenDirectory"
        };
        static readonly string[] ControlMenuButtons =
        {
            "Lock left foot",
            "Lock right foot",
            "Unlock both feet",
            "SnapShot plane",
            "Toggle pause",
            "Toggle direction",
            "Initialize walk",
            "Zero out mech",
            "Main Menu"
        };
        static readonly string[] OptionsMenuButtons =
        {
            "Toggle ignore feet",
            "Toggle ignore save",
            "Toggle ignore plane",
            "Toggle stator control",
            "Toggle stator target",
            "Toggle auto demo",
            "Increase speed",
            "Decrease speed",
            "Main Menu"
        };
        static readonly string[] InputLabels =
        {
            "w",
            "s",
            "a",
            "d"
        };

        string MainText = "Mech Control v0.4.4";
        string InfoText = "(InfoScreen)";
        string[] Cursor = new string[] { "  ", "->" };
        #endregion

        #region STRING BUILDERS & SCREENS
        IMyTextSurface[] CockPitScreens = new IMyTextSurface[3];
        List<IMyTextPanel> DebugScreens = new List<IMyTextPanel>();
        StringBuilder DebugBinStream;
        StringBuilder DebugBinStatic;
        StringBuilder DisplayManagerBuilder;
        StringBuilder ButtonBuilder;
        StringBuilder GUIBuilder;
        StringBuilder SaveData;
        #endregion

        #region SAMPLE OBJECTS
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
        #endregion

        #region ENUMS
        public enum JointType
        {
            ROTOR,
            HINGE,
            PISTON
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
            LIBRARY,
            CONTROL,
            OPTIONS
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
        #endregion

        #region CLASSES
        class Joint
        {
            public IMyMotorStator Stator;
            public IMyPistonBase Piston;
            public IMyMechanicalConnectionBlock Connection;

            public string Name;
            public int Index;
            public bool IsGrip;

            public JointType Jtype;
            public float[] LerpPoints = new float[2];
            public string LoadedFrame;

            // Controlled by linked foot
            public bool Planeing = false;
            public bool Gripping = false;
            public int AuxDirection;
            //////////////////////////////

            public double PlaneTarget;
            public double AnimTarget;
            public double ActiveTarget;

            public int Direction;
            public double CorrectionMag;
            public double StatorVelocity;
            public double LiteralVelocity;

            double OldVelocity;
            float LastPosition;
            DateTime LastTime;

            public Joint(IMyMechanicalConnectionBlock mechBlock, int index, string name = "default", bool isGrip = false, int auxDir = 1)
            {
                Connection = mechBlock;
                mechBlock.Enabled = true;
                IsGrip = isGrip;
                AuxDirection = auxDir;
                Jtype = (mechBlock is IMyPistonBase) ? JointType.PISTON : Connection.BlockDefinition.ToString().Contains("Hinge") ? JointType.HINGE : JointType.ROTOR;

                switch (Jtype)
                {
                    case JointType.ROTOR:
                        Stator = (IMyMotorStator)mechBlock;
                        Stator.LowerLimitDeg = float.MinValue;
                        Stator.UpperLimitDeg = float.MaxValue;
                        break;

                    case JointType.HINGE:
                        Stator = (IMyMotorStator)mechBlock;
                        if (IsGrip)
                            break;
                        Stator.LowerLimitDeg = -90;
                        Stator.UpperLimitDeg = 90;
                        break;

                    case JointType.PISTON:
                        Piston = (IMyPistonBase)mechBlock;
                        break;
                }

                Name = name;
                Index = index;
                LoadedFrame = "none";
            }

            public void LoadAnimationFrame(JointFrame frame, bool forward = true, bool interrupt = false)
            {
                if (forward)
                {
                    LerpPoints[0] = interrupt ? ReturnCurrentStatorPosition() : LerpPoints[1];
                    LerpPoints[1] = frame.LerpPoint;
                }
                else
                {
                    LerpPoints[1] = interrupt ? ReturnCurrentStatorPosition() : LerpPoints[0];
                    LerpPoints[0] = frame.LerpPoint;
                }
            }
            public void OverwriteAnimTarget(float value)
            {
                AnimTarget = value;
            }
            public void LerpAnimationFrame(float lerpTime, ref StringBuilder debugBin)
            {
                if (lerpTime > 1 ||
                    lerpTime < 0)
                    return;

                float target;

                if (Jtype == JointType.ROTOR)
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

                AnimTarget = target;
            }
            public void UpdateJoint(bool activeTargetTracking, double delta, ref StringBuilder debugStream)
            {
                UpdateLiteralVelocity();
                UpdateActiveTarget(ref debugStream, activeTargetTracking);

                switch (Jtype)
                {
                    case JointType.ROTOR:
                        UpdateRotorVector(ref debugStream);
                        UpdateStatorVelocity(ref debugStream, activeTargetTracking);
                        UpdateStator(activeTargetTracking);
                        break;

                    case JointType.HINGE:
                        UpdateHingeVector(ref debugStream);
                        UpdateStatorVelocity(ref debugStream, activeTargetTracking);
                        UpdateStator(activeTargetTracking);
                        break;

                    case JointType.PISTON:
                        UpdatePistonVector(ref debugStream);
                        UpdateStatorVelocity(ref debugStream, activeTargetTracking);
                        UpdatePiston(activeTargetTracking);
                        break;
                }
            }
            public float ReturnCurrentStatorPosition()
            {
                if (Stator != null)
                    return (float)(Stator.Angle * RAD2DEG);

                if (Piston != null)
                    return Piston.CurrentPosition;

                return -100;
            }

            void UpdateLiteralVelocity()
            {
                float currentPosition = ReturnCurrentStatorPosition();
                DateTime now = DateTime.Now;

                LiteralVelocity = ((currentPosition - LastPosition) / 360) / (now - LastTime).TotalMinutes;

                LastTime = now;
                LastPosition = currentPosition;
            }
            void UpdateActiveTarget(ref StringBuilder debugBin, bool active)
            {
                if (!active)
                    return;

                ActiveTarget = AnimTarget;

                if (Planeing)
                {
                    ActiveTarget += PlaneTarget;
                    switch (Jtype)
                    {
                        case JointType.ROTOR:
                            ActiveTarget = ActiveTarget % 360;
                            ActiveTarget = ActiveTarget < 0 ? ActiveTarget + 360 : ActiveTarget;
                            break;

                        case JointType.HINGE:
                            ActiveTarget = ActiveTarget % 360;
                            ActiveTarget = ActiveTarget > 180 ? ActiveTarget - 360 : ActiveTarget;
                            ActiveTarget = ActiveTarget > 90 ? 90 : ActiveTarget;
                            break;

                        case JointType.PISTON:
                            ActiveTarget = ActiveTarget > 10 ? 10 : ActiveTarget;
                            ActiveTarget = ActiveTarget < 0 ? 0 : ActiveTarget;
                            break;
                    }
                }
            }
            void UpdateRotorVector(ref StringBuilder debugBin)
            {
                double current = (Stator.Angle * RAD2DEG);

                double delta = Math.Abs(ActiveTarget - current);
                Direction = (delta > 180) ? Math.Sign(current - ActiveTarget) : Math.Sign(ActiveTarget - current);
                CorrectionMag = (delta > 180) ? 360 - delta : delta;
                Direction = (Direction == 0) ? 1 : Direction;
            }
            void UpdateHingeVector(ref StringBuilder debugBin)
            {
                CorrectionMag = ActiveTarget - (Stator.Angle * RAD2DEG);
                Direction = Math.Sign(CorrectionMag);
                CorrectionMag = Math.Abs(CorrectionMag);
            }
            void UpdatePistonVector(ref StringBuilder debugBin)
            {
                CorrectionMag = ActiveTarget - Piston.CurrentPosition;
                Direction = Math.Sign(CorrectionMag);
                CorrectionMag = Math.Abs(CorrectionMag);
            }
            void UpdateStatorVelocity(ref StringBuilder debugStream, bool active)
            {
                if (active)
                {
                    OldVelocity = StatorVelocity;
                    if (IsGrip)
                    {
                        StatorVelocity = MaxSpeed * AuxDirection * (Gripping ? 1 : -1);
                    }
                    else
                    {
                        double scale = CorrectionMag * VScalar;
                        //scale = Math.Pow(scale, 3);
                        StatorVelocity = Direction * scale;

                        if (scale < Threshold)
                            StatorVelocity = 0;

                        StatorVelocity = (Math.Abs(StatorVelocity - OldVelocity) > MaxAccel) ? OldVelocity + (MaxAccel * Math.Sign(StatorVelocity - OldVelocity)) : StatorVelocity;
                        StatorVelocity = (Math.Abs(StatorVelocity) > MaxSpeed) ? MaxSpeed * Math.Sign(StatorVelocity) : StatorVelocity;
                    }
                }
                else
                    StatorVelocity = 0;
            }
            void UpdateStator(bool active)
            {
                if (Stator == null)
                    return;

                Stator.SetValueFloat("Velocity", (float)StatorVelocity);
            }
            void UpdatePiston(bool active)
            {
                if (Piston == null)
                    return;

                Piston.SetValueFloat("Velocity", (float)StatorVelocity);
            }
        }
        class JointFrame
        {
            public Joint Joint;
            public float LerpPoint;

            public JointFrame(Joint joint, bool snapping = false) // Snapshot
            {
                Joint = joint;
                float point = Joint.ReturnCurrentStatorPosition();
                if (snapping)
                    point = (int)point;
                LerpPoint = point;
            }

            public JointFrame(Joint joint, float lerpPoint) // User-Written
            {
                Joint = joint;
                LerpPoint = lerpPoint;
            }

            public bool ChangeStatorLerpPoint(float value)
            {
                float MIN = 0;
                float MAX = 0;

                switch (Joint.Jtype)
                {
                    case JointType.ROTOR:
                        //value = value > 360 ? value - 360 : value;
                        value %= 360;
                        value = value < 0 ? value + 360 : value;
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
            public string GroupName;
            public int Index;

            public IMyTerminalBlock Plane;
            public MatrixD CurrentPlane;
            public MatrixD BufferPlane;
            public Vector3 CorrectBuffer;
            public Vector3 Xbuff, Ybuff, Zbuff;

            public Foot[] Feet;
            public Joint[] Joints;
            public List<Sequence> Sequences;

            public bool bIgnoreFeet;

            public bool Triggered = true;
            public double TriggerTime;

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

            public JointSet(string name, string groupName, int index, Joint[] joints, Foot[] feet, IMyTerminalBlock plane = null, List<Sequence> sequences = null, bool ignoreFeet = true)
            {
                Name = name;
                GroupName = groupName;
                Index = index;
                Sequences = sequences;
                Joints = joints;
                Feet = feet;
                Plane = plane;
                bIgnoreFeet = ignoreFeet;

                if (Feet == null ||
                    Feet.Length != 2) //    >: |
                    Feet = new Foot[2];

                if (sequences == null)
                    Sequences = new List<Sequence>();
                else
                    foreach (Sequence seq in Sequences)
                        seq.JointSet = this;

                SortJoints();
            }
            public JointSet(string name, string groupName, int index, int jointCount, IMyTerminalBlock plane = null, bool ignoreFeet = true)
            {
                Name = name;
                GroupName = groupName;
                Index = index;
                Plane = plane;
                bIgnoreFeet = ignoreFeet;

                Joints = new Joint[jointCount];
                Feet = new Foot[2];
                Sequences = new List<Sequence>();

                SortJoints();
            }
            public void UnlockFeet()
            {
                foreach (Foot foot in Feet)
                    foot.ToggleLock(false);
            }
            public void UpdateFootLockStatus(ref StringBuilder debugBin)
            {
                foreach (Foot foot in Feet)
                    foot.CheckLocked(ref debugBin);
            }
            public bool CheckStep(float lerpTime, bool forward, ref StringBuilder debugBin)
            {
                bool footCheck = false;
                int lockIndex;
                int unLockIndex;

                // determine currently locked and checking feet
                if (Feet[0].Locked)
                {
                    footCheck = Feet[1].CheckTouching(ref debugBin);
                    lockIndex = 1;
                    unLockIndex = 0;
                }
                else
                {
                    footCheck = Feet[0].CheckTouching(ref debugBin);
                    lockIndex = 0;
                    unLockIndex = 1;
                }

                // reset for the RS latch
                if (Triggered && Math.Abs(TriggerTime - lerpTime) >= TriggerCap)
                {
                    Triggered = false;
                }

                if (!Triggered && footCheck) // Initial contact
                {
                    Triggered = true;
                    TriggerTime = forward ? 0 : 1;

                    Feet[lockIndex].ToggleLock();
                    Feet[unLockIndex].ToggleLock(false);

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
            public bool InitializeGrip(ref StringBuilder debugBin, bool left = true)
            {
                try
                {
                    if (left)
                    {
                        Feet[0].ToggleLock(true);
                        Feet[1].ToggleLock(false);
                    }
                    else
                    {
                        Feet[0].ToggleLock(false);
                        Feet[1].ToggleLock(true);
                    }

                    return true;
                }
                catch
                {
                    debugBin.Append("Fail!\n");
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
                if (Plane != null)
                    CurrentPlane = Plane.WorldMatrix;
            }
            public void TogglePlaneing(bool toggle)
            {
                foreach (Foot foot in Feet)
                {
                    foot.TogglePlaneing(toggle);
                }
            }

            void SortJoints()
            {
                // Option A... onus on user
                Array.Sort(Joints, new JointSort());

                // Option B...
                /*
                foreach(Joint joint in Joints)
                bool MatchStator(string cubeGridName, IMyMotorStator stator)
                bool MatchPiston(string cubeGridName, IMyPistonBase piston)
                */
            }
            void TransformVectorRelative(ref MatrixD S, Vector3 D, ref Vector3 RV) // S = sourceBearing, D = WorldVectorDelta
            {
                /* X,Y,Z = Normalized Vector Unit Coefficients
                 * x,y,z = Delta Target Vector (raw World GPS)
                 * a,b,c = Normalized X vector components (relative x,y,z)
                 * d,e,f = Normalized Y ''
                 * g,h,i = Normalized Z ''
                 * 
                 * Keen Implementation:
                 *      : Column1 : Column2 : Column3
                 * Row 1: Right.x , Right.y , Right.z
                 * Row 2: Up.x    , Up.y    , Up.z
                 * Row 3: Back.x  , Back.y  , Back.z
                */

                // Z = (d(bz - cy) + e(cx - az) + f(ay - bx)) / (d(bi - ch) + e(cg - ai) + f(ah - bg))

                // Z =         (d     * ((b     * z)   - (c     * y))   + e     * ((c     * x)   - (a     * z))   + f     * ((a     * y)   - (b     * x)))   / (d     * ((b     * i)     - (c     * h))     + e     * ((c     * g)     - (a     * i))     + f     * ((a     * h)     - (b     * g)))
                RV.Z = (float)((S.M21 * ((S.M12 * D.Z) - (S.M13 * D.Y)) + S.M22 * ((S.M13 * D.X) - (S.M11 * D.Z)) + S.M23 * ((S.M11 * D.Y) - (S.M12 * D.X))) / (S.M21 * ((S.M12 * S.M33) - (S.M13 * S.M32)) + S.M22 * ((S.M13 * S.M31) - (S.M11 * S.M33)) + S.M23 * ((S.M11 * S.M32) - (S.M12 * S.M31))));


                // Y = (Z(hc - ib) + zb - yc) / (fb - ec)
                // Y = (Z(gb - ha) + ya - xb) / (ea - db)

                // Y =         (Z    * ((h     * c)     - (i     * b))     + (z   * b)     - (y   * c))     / ((f     * b)     - (e     * c))
                RV.Y = (float)((RV.Z * ((S.M32 * S.M13) - (S.M33 * S.M12)) + (D.Z * S.M12) - (D.Y * S.M13)) / ((S.M23 * S.M12) - (S.M22 * S.M13)));

                // X = (x - (Yd + Zg)) / a
                // X = (y - (Ye + Zh)) / b
                // X = (z - (Yf + Zi)) / c

                // X =         (x   - ((Y    * d)     + (Z    * g)))     / a
                RV.X = (float)((D.X - ((RV.Y * S.M21) + (RV.Z * S.M31))) / S.M11);

            }
            public void UpdatePlane(ref StringBuilder debugBinStream, ref Vector3 rotBuffer)
            {
                if (Plane == null)
                    return;

                // Player input
                PlayerInput(ref CurrentPlane, ref BufferPlane, ref rotBuffer);

                // Auto-plane
                TransformMatrixRelative(Plane.WorldMatrix, ref CurrentPlane, ref BufferPlane);
                MatrixToRotations(ref BufferPlane, ref CorrectBuffer);
                //CorrectBuffer = 2 * CorrectBuffer;
                //double dampener;

                foreach (Foot foot in Feet)
                {
                    if (foot == null)
                        continue;

                    if (foot.Planes[0] != null)
                    {
                        //dampener = MaxSpeed - Math.Abs(foot.Planes[0].StatorVelocity - foot.Planes[0].LiteralVelocity);
                        //dampener = dampener < 0 ? 0 : dampener;
                        foot.Planes[0].PlaneTarget = CorrectBuffer.X * foot.Planes[0].AuxDirection;// * (dampener / MaxSpeed);
                    }

                    if (foot.Planes[1] != null)
                    {
                        //dampener = MaxSpeed - Math.Abs(foot.Planes[1].StatorVelocity - foot.Planes[1].LiteralVelocity);
                        //dampener = dampener < 0 ? 0 : dampener;
                        foot.Planes[1].PlaneTarget = CorrectBuffer.Y * foot.Planes[1].AuxDirection;// * (dampener / MaxSpeed);
                    }

                    if (foot.Planes[2] != null)
                    {
                        //dampener = MaxSpeed - Math.Abs(foot.Planes[2].StatorVelocity - foot.Planes[2].LiteralVelocity);
                        //dampener = dampener < 0 ? 0 : dampener;
                        foot.Planes[2].PlaneTarget = CorrectBuffer.Z * foot.Planes[2].AuxDirection;// * (dampener / MaxSpeed);
                    }
                }
            }
            void TransformMatrixRelative(MatrixD S, ref MatrixD T, ref MatrixD R)
            {
                TransformVectorRelative(ref S, T.Right, ref Xbuff);
                TransformVectorRelative(ref S, T.Up, ref Ybuff);
                TransformVectorRelative(ref S, T.Forward, ref Zbuff);

                R.Right = Xbuff;
                R.Up = Ybuff;
                R.Forward = Zbuff;
            }
            void MatrixToRotations(ref MatrixD S, ref Vector3 rots)
            {
                rots.X = (float)(Math.Atan2(S.Forward.Y, -S.Forward.Z) * RAD2DEG);
                rots.Y = (float)(Math.Atan2(S.Forward.X, -S.Forward.Z) * RAD2DEG);
                rots.Z = (float)(Math.Atan2(S.Up.X, S.Up.Y) * RAD2DEG);
            }
            void PlayerInput(ref MatrixD S, ref MatrixD B, ref Vector3 rots)
            {
                B = MatrixD.CreateRotationX(rots.X);
                S = MatrixD.Multiply(B, S);
                B = MatrixD.CreateRotationY(-rots.Y);
                S = MatrixD.Multiply(B, S);
                B = MatrixD.CreateRotationZ(rots.Z);
                S = MatrixD.Multiply(B, S);
            }
        }
        class Foot
        {
            public bool Locked;
            public bool Planeing; // Toggled throough user controls

            public IMyLandingGear[] Pads;

            public Joint[] Toes;
            public Joint[] Planes;

            public Foot(IMyLandingGear[] pads, Joint[] toes, Joint pitch = null, Joint yaw = null, Joint roll = null)
            {
                Locked = false;
                Pads = pads;
                Toes = toes;
                Planes = new Joint[3];
                Planes[0] = pitch;
                Planes[1] = yaw;
                Planes[2] = roll;

                foreach (IMyLandingGear gear in Pads)
                {
                    gear.AutoLock = false;
                    gear.Enabled = true;
                }

                if (Locked)
                    ToggleLock();
            }
            public void ToggleLock(bool locking = true)
            {
                ToggleGrip(locking);

                Locked = locking;
                UpdateFeetPlaneing();

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
            public void TogglePlaneing(bool planeing = true)
            {
                Planeing = planeing;
                UpdateFeetPlaneing();
            }
            /*public void SnapShotPlane()
            {
                Pitch.SnapShotPlane();
                Roll.SnapShotPlane();
            }*/
            public bool CheckTouching(ref StringBuilder debug)
            {
                foreach (IMyLandingGear gear in Pads)
                {
                    if (gear.LockMode == LandingGearMode.ReadyToLock)
                        return true;
                }

                return false;
            }
            public bool CheckLocked(ref StringBuilder debug)
            {
                foreach (IMyLandingGear gear in Pads)
                {
                    if (gear.LockMode == LandingGearMode.Locked)
                    {
                        ToggleLock(true);
                        return true;
                    }
                }

                ToggleLock(false);
                return false;
            }
            void ToggleGrip(bool gripping = true)
            {
                //int dir = gripping ? -1 : 1;
                //for (int i = 0; i < Toes.Length; i++)
                foreach (Joint toe in Toes)
                    toe.Gripping = gripping;
                //Grips[i].SetValueFloat("Velocity", MaxSpeed * dir);// * GripDirections[i]);  //  >: |
            }
            void UpdateFeetPlaneing()
            {
                foreach (Joint plane in Planes)
                    if (plane != null)
                        plane.Planeing = Locked && Planeing;
            }
        }
        class KeyFrame
        {
            public string Name;
            public JointFrame[] Jframes;

            public KeyFrame(string name, JointFrame[] jFrames)//, JointSet legs)
            {
                Name = name;
                Jframes = jFrames;
            }
        }
        class Sequence
        {
            /// EXTERNALS ///
            public string Name;
            public List<KeyFrame> Frames;
            public JointSet JointSet;

            // Clock
            public ClockMode ClockMode;
            public bool bFrameLoadForward;
            public float CurrentClockSpeed;
            public float CurrentClockTime;

            // Frames
            //int StartFrameIndex;
            public KeyFrame CurrentFrame;
            public int CurrentFrameIndex;

            public Sequence(string name, JointSet set = null, List<KeyFrame> frames = null, float clockSpeed = ClockSpeedDef)
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
                CurrentClockSpeed = clockSpeed;
            }

            public void ZeroSequence(ref StringBuilder debugBin)
            {
                LoadFrame(0, true, false, ref debugBin);
                ClockMode = ClockMode.PAUSE;
                CurrentClockTime = 0;
            }
            public void UpdateClockSpeed(bool increase = true)
            {
                if (increase)
                {
                    CurrentClockSpeed += ClockIncrmentMag;
                    CurrentClockSpeed = CurrentClockSpeed > ClockSpeedMax ? ClockSpeedMax : CurrentClockSpeed;
                }
                else
                {
                    CurrentClockSpeed -= ClockIncrmentMag;
                    CurrentClockSpeed = CurrentClockSpeed < ClockSpeedMin ? ClockSpeedMin : CurrentClockSpeed;
                }
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
            public bool DemoKeyFrame(int index, ref StringBuilder debugBin)
            {
                if (index < 0 ||
                    index >= Frames.Count)
                    return false;

                foreach (JointFrame jFrame in Frames[index].Jframes)
                    jFrame.Joint.OverwriteAnimTarget(jFrame.LerpPoint);

                return true;
            }

            public bool UpdateSequence(ref StringBuilder debugBin)
            {
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
            public bool AddKeyFrameSnapshot(ref StringBuilder debugBin, int index = -1, string name = null, bool snapping = false)
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

            void UpdateTriggers(ref StringBuilder debugBin)
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
                        CurrentClockTime += CurrentClockSpeed;
                        if (CurrentClockTime >= 1 ||
                            (!JointSet.bIgnoreFeet &&
                            JointSet.CheckStep(CurrentClockTime, forward, ref debugBin)))
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
                        CurrentClockTime -= CurrentClockSpeed;
                        if (CurrentClockTime <= 0 ||
                            !JointSet.bIgnoreFeet &&
                            JointSet.CheckStep(CurrentClockTime, forward, ref debugBin))
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

                LoadFrame(CurrentFrameIndex, forward, interrupt, ref debugBin);
            }
            void LoadFrame(int index, bool forward, bool interrupt, ref StringBuilder debugBin)
            {
                if (JointSet == null)//||
                    //JointSet.Joints.Length == 0)
                    return;

                if (index >= Frames.Count ||
                    index < 0 ||
                    Frames.Count == 0)
                    return;

                CurrentFrame = Frames[index];
                CurrentClockTime = forward ? 0 : 1;

                foreach (JointFrame joint in CurrentFrame.Jframes)
                {
                    if (joint.Joint == null)
                        continue;

                    joint.Joint.LoadAnimationFrame(joint, forward, interrupt);
                }

                bFrameLoadForward = forward;
            }
            void LerpFrame(float lerpTime, ref StringBuilder debugBin)
            {
                foreach (JointFrame joint in CurrentFrame.Jframes)
                {
                    if (joint.Joint == null)
                        continue;

                    joint.Joint.LerpAnimationFrame(lerpTime, ref debugBin);
                }
            }
        }
        #endregion

        #region CONSTRUCTIONS
        bool SampleLegsConstructor(ref StringBuilder debugBin)
        {
            JsetBin.Clear();
            JointSet SampleLegs;
            Sequence SampleWalk;

            Foot[] feet = new Foot[2];

            List<IMyLandingGear> pads0 = new List<IMyLandingGear>();
            List<IMyLandingGear> pads1 = new List<IMyLandingGear>();
            List<IMyMotorStator> toes0 = new List<IMyMotorStator>();
            List<IMyMotorStator> toes1 = new List<IMyMotorStator>();
            List<Joint> toeBuffer0 = new List<Joint>();
            List<Joint> toeBuffer1 = new List<Joint>();
            int[][] gDir = new int[2][];
            gDir[0] = new int[4];
            gDir[1] = new int[4];

            IMyBlockGroup group0 = GridTerminalSystem.GetBlockGroupWithName(SampleFeetNames[0]);
            IMyBlockGroup group1 = GridTerminalSystem.GetBlockGroupWithName(SampleFeetNames[1]);
            IMyBlockGroup group2 = GridTerminalSystem.GetBlockGroupWithName(SampleFeetNames[2]);
            IMyBlockGroup group3 = GridTerminalSystem.GetBlockGroupWithName(SampleFeetNames[3]);

            if (group0 != null)
                group0.GetBlocksOfType(pads0);
            if (group1 != null)
                group1.GetBlocksOfType(pads1);
            if (group2 != null)
                group2.GetBlocksOfType(toes0);
            if (group3 != null)
                group3.GetBlocksOfType(toes1);

            if (pads0.Count == 0 ||
                pads1.Count == 0)
            {
                debugBin.Append("Pads were not found!\n");
                return false;
            }

            if (toes0.Count == 0 ||
                toes1.Count == 0)
            {
                debugBin.Append("Toes were not found!\n");
                return false;
            }

            SampleLegs = new JointSet("SampleLegs", SampleLegsGroupName, 0, SampleJointNames.Length, Control, false);
            CurrentWalkSet = SampleLegs;

            for (int i = 0; i < SampleJointNames.Length; i++)
            {
                IMyTerminalBlock nextJoint = GridTerminalSystem.GetBlockWithName(SampleJointNames[i]);
                if (nextJoint == null)
                {
                    debugBin.Append($"Joint {i}:{SampleJointNames[i]} was not found!\n");
                    return false;
                }

                if (nextJoint is IMyMechanicalConnectionBlock)
                {
                    SampleLegs.Joints[i] = new Joint((IMyMechanicalConnectionBlock)nextJoint, i, SampleJointNames[i]);
                    debugBin.Append($"Joint {i}:{SampleJointNames[i]} successfully added!\n");
                }
            }

            foreach (IMyMotorStator toe in toes0)
                toeBuffer0.Add(new Joint(toe, -1, "LeftToe", true, -1));
            foreach (IMyMotorStator toe in toes1)
                toeBuffer1.Add(new Joint(toe, -1, "RightToe", true, -1));

            //0,1,4 : 5,8,9 : r,p,y

            feet[0] = new Foot(pads0.ToArray(), toeBuffer0.ToArray(), SampleLegs.Joints[1], SampleLegs.Joints[4], SampleLegs.Joints[0]);
            feet[1] = new Foot(pads1.ToArray(), toeBuffer1.ToArray(), SampleLegs.Joints[8], SampleLegs.Joints[5], SampleLegs.Joints[9]);

            SampleLegs.Feet = feet;

            JsetBin.Add(SampleLegs);

            SampleWalk = new Sequence("walking", SampleLegs);
            CurrentWalk = SampleWalk;

            return true;
        }
        JointSet ConstructJointSet(ref StringBuilder debugBin, IMyShipController control, int setIndex, string blockGroupName = null, string name = null, bool ignoreFeet = true)
        {
            JointSet output = null;

            if (control == null)
            {
                return output;
            }

            if (blockGroupName == null)
                UserInputString(ref blockGroupName);

            if (blockGroupName == null)
            {
                return output;
            }

            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(blockGroupName);
            if (group == null)
            {
                return output;
            }

            if (name == null)
                name = blockGroupName;

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);

            List<Joint> jointBuffer = new List<Joint>();

            // foot buffering
            Foot[] feetBuffer = new Foot[2];
            List<IMyLandingGear>[] toes = new List<IMyLandingGear>[2];
            List<Joint>[] grips = new List<Joint>[2];
            List<int>[] gripDir = new List<int>[2];
            for (int i = 0; i < 2; i++)
            {
                toes[i] = new List<IMyLandingGear>();
                grips[i] = new List<Joint>();
                gripDir[i] = new List<int>();
            }
            Joint[] pitch = new Joint[2];
            Joint[] yaw = new Joint[2];
            Joint[] roll = new Joint[2];

            foreach (IMyTerminalBlock block in blocks)
            {
                if (block is IMyLandingGear)
                {
                    BuildToe(ref debugBin, ref toes, setIndex, (IMyLandingGear)block);
                }

                if (block is IMyPistonBase ||
                    block is IMyMotorStator)
                {
                    BuildJoint(ref debugBin, ref jointBuffer, ref grips, ref pitch, ref yaw, ref roll, setIndex, (IMyMechanicalConnectionBlock)block);
                }
            }

            Foot[] footBuffer = new Foot[2];

            footBuffer[0] = new Foot(toes[0].ToArray(), grips[0].ToArray(), pitch[0], yaw[0], roll[0]);
            footBuffer[1] = new Foot(toes[1].ToArray(), grips[1].ToArray(), pitch[1], yaw[1], roll[1]);

            return new JointSet(name, blockGroupName, setIndex, jointBuffer.ToArray(), footBuffer, control, null, ignoreFeet);
        }
        void BuildToe(ref StringBuilder debugBin, ref List<IMyLandingGear>[] toeBuffer, int setIndex, IMyLandingGear gear)
        {
            string[] data = gear.CustomData.Split(':');
            if (data.Length != SaveBlockCountSize)
                return;

            if (data[0] != "T")
                return;

            string name = data[1];

            int parentIndex;
            if (!int.TryParse(data[2], out parentIndex))
                return;

            if (parentIndex != setIndex)
                return;

            int IDindex;
            if (!int.TryParse(data[3], out IDindex))
                IDindex = -1; // un-needed atm

            int footIndex;
            if (!int.TryParse(data[4], out footIndex))
                return;

            int direction;
            if (!int.TryParse(data[5], out direction))
                return;

            if (footIndex < 0 ||
                footIndex >= toeBuffer.Length)
                return;

            toeBuffer[footIndex].Add(gear);
        }
        void BuildJoint(ref StringBuilder debugBin, ref List<Joint> jBuffer, ref List<Joint>[] gBuffer, ref Joint[] pitch, ref Joint[] yaw, ref Joint[] roll, int setIndex, IMyMechanicalConnectionBlock jointBlock)
        {
            string[] data = jointBlock.CustomData.Split(':');
            if (data.Length != SaveBlockCountSize)
                return;

            /* toe-pads = T:name:sInd:uInd:fInd:n/a

                  joint = J:name:sInd:uInd:n/a :n/a
                   grip = G:name:sInd:uInd:fInd:dir
                   roll = R:name:sInd:uInd:fInd:dir
                  pitch = P:name:sInd:uInd:fInd:dir
                    yaw = Y:name:sInd:uInd:fInd:dir */

            string name = data[1];

            int parentIndex;
            if (!int.TryParse(data[2], out parentIndex))
                return;

            if (parentIndex != setIndex)
                return;

            int IDindex;
            if (!int.TryParse(data[3], out IDindex))
                return;

            int footIndex;
            if (!int.TryParse(data[4], out footIndex))
                return;

            int direction;
            if (!int.TryParse(data[5], out direction))
                return;

            Joint newJoint = null;

            switch (data[0])
            {
                case "P":
                    if (footIndex < 0 || footIndex >= pitch.Length)
                        return;
                    newJoint = new Joint(jointBlock, IDindex, name, false, direction);
                    pitch[footIndex] = newJoint;
                    jBuffer.Add(newJoint);
                    break;

                case "Y":
                    if (footIndex < 0 || footIndex >= yaw.Length)
                        return;
                    newJoint = new Joint(jointBlock, IDindex, name, false, direction);
                    yaw[footIndex] = newJoint;
                    jBuffer.Add(newJoint);
                    break;

                case "R":
                    if (footIndex < 0 || footIndex >= roll.Length)
                        return;
                    newJoint = new Joint(jointBlock, IDindex, name, false, direction);
                    roll[footIndex] = newJoint;
                    jBuffer.Add(newJoint);
                    break;

                case "J":
                    newJoint = new Joint(jointBlock, IDindex, name);
                    jBuffer.Add(newJoint);
                    break;

                case "G":
                    newJoint = new Joint(jointBlock, IDindex, name, true, direction);
                    gBuffer[footIndex].Add(newJoint);
                    break;

                default:
                    debugBin.Append("Invalid joint code!\n");
                    break;
            }
        }
        #endregion

        #region PLAYER INPUTS
        void ControlInput()
        {
            if (Control == null)
            {
                RotationBuffer = Vector3.Zero;
                return;
            }

            switch (CurrentGUIMode)
            {
                case GUIMode.LIBRARY:
                    switch ((int)Control.MoveIndicator.Z)
                    {
                        case -1:
                            if (LastLibraryInput[0] == -1)
                                break;
                            LastLibraryInput[0] = -1;
                            GUINavigation(GUINav.UP);
                            GUIUpdate();
                            break;

                        case 0:
                            if (LastLibraryInput[0] == 0)
                                break;
                            LastLibraryInput[0] = 0;
                            break;

                        case 1:
                            if (LastLibraryInput[0] == 1)
                                break;
                            LastLibraryInput[0] = 1;
                            GUINavigation(GUINav.DOWN);
                            GUIUpdate();
                            break;
                    }
                    switch ((int)Control.MoveIndicator.X)
                    {
                        case -1:
                            if (LastLibraryInput[1] == -1)
                                break;
                            LastLibraryInput[1] = -1;
                            GUINavigation(GUINav.BACK);
                            GUIUpdate();
                            break;

                        case 0:
                            if (LastLibraryInput[1] == 0)
                                break;
                            LastLibraryInput[1] = 0;
                            break;

                        case 1:
                            if (LastLibraryInput[1] == 1)
                                break;
                            LastLibraryInput[1] = 1;
                            GUINavigation(GUINav.SELECT);
                            GUIUpdate();
                            break;
                    }
                    break;

                case GUIMode.CONTROL:
                    RotationBuffer.X = LookScalar * Control.RotationIndicator.X;
                    RotationBuffer.Y = LookScalar * Control.RotationIndicator.Y;
                    RotationBuffer.Z = LookScalar * Control.RollIndicator;
                    switch ((int)Control.MoveIndicator.Z)
                    {
                        case 1:
                            if (LastMechInput == -1)
                                break;
                            LastMechInput = -1;
                            CurrentWalk.UpdateClockMode(ClockMode.REV);
                            bWalking = true;
                            break;

                        case 0:
                            if (LastMechInput == 0)
                                break;
                            LastMechInput = 0;
                            CurrentWalk.UpdateClockMode(ClockMode.PAUSE);
                            bWalking = false;
                            break;

                        case -1:
                            if (LastMechInput == 1)
                                break;
                            LastMechInput = 1;
                            CurrentWalk.UpdateClockMode(ClockMode.FOR);
                            bWalking = true;
                            break;
                    }
                    break;
            }
        }
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

            CurrentWalkSet.UpdateFootLockStatus(ref DebugBinStatic);
            CurrentWalkSet.ZeroJointSet();
        }
        void WalkToggle()
        {
            if (bWalking)
                CurrentWalk.UpdateClockMode(OldWalkState);
            else
                CurrentWalk.UpdateClockMode(ClockMode.PAUSE);
        }
        #endregion

        #region GUI METHODS
        void GUIUpdate(IMyTextPanel panel = null)
        {
            //if (ButtonPanel != null && GUIPanel != null)
            //{
            ButtonBuilder.Clear();
            ButtonBuilder.Append(RawButtonStringBuilder(CurrentGUIMode));

            string[] guiData = null;
            switch (CurrentGUIMode)
            {
                case GUIMode.LIBRARY:
                    guiData = LibraryStringBuilder(ref DebugBinStatic);
                    DemoSelectedFrame();
                    break;

                case GUIMode.INFO:
                    guiData = StaticStringBuilder(false);
                    break;

                case GUIMode.MAIN:
                    guiData = StaticStringBuilder();
                    break;

                case GUIMode.CONTROL:
                    guiData = ControlStringBuilder();
                    break;

                case GUIMode.OPTIONS:
                    guiData = OptionsStringBuilder();
                    break;
            }

            GUIBuilder.Clear();
            GUIBuilder.Append(FormattedGUIStringBuilder(guiData));
            DebugScreens[1].WriteText(DebugBinStatic);
            //}
        }
        bool DemoSelectedFrame()
        {
            try
            {
                if (bAutoDemo)
                    JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].DemoKeyFrame(SelObjIndex[2], ref DebugBinStatic);
                return true;
            }
            catch
            {
                return false;
            }
        }
        string RawButtonStringBuilder(GUIMode mode)
        {
            string output = "";

            switch (mode)
            {
                case GUIMode.MAIN:
                    for (int i = 0; i < MainMenuButtons.Length; i++)
                    {
                        output += $"{i + 1} - {MainMenuButtons[i]}\n";
                    }
                    break;

                case GUIMode.INFO:
                    for (int i = 0; i < InfoMenuButtons.Length; i++)
                        output += $"{i + 1} - {InfoMenuButtons[i]}\n";

                    break;

                case GUIMode.LIBRARY:
                    for (int i = 0; i < LibraryMenuButtons.Length; i++)
                    {
                        string header = i < 8 ? (i + 1).ToString() : InputLabels[i - 8];
                        output += $"{header} - {LibraryMenuButtons[i]}\n";
                    }
                    break;

                case GUIMode.CONTROL:
                    for (int i = 0; i < ControlMenuButtons.Length; i++)
                    {
                        output += $"{i + 1} - {ControlMenuButtons[i]}\n";
                    }
                    break;

                case GUIMode.OPTIONS:
                    for (int i = 0; i < OptionsMenuButtons.Length; i++)
                    {
                        output += $"{i + 1} - {OptionsMenuButtons[i]}\n";
                    }
                    break;
            }

            return output;
        }
        string[] StaticStringBuilder(bool main = true)
        {
            string input = main ? MainText : InfoText;
            string[] output = input.Split('\n');
            return output;
        }
        string[] LibraryStringBuilder(ref StringBuilder debugBin)
        {
            List<string> stringList = new List<string>();
            stringList.Add("======Library======");
            stringList.Add($"===(Snapping:{Snapping})===");
            int layer = (int)CurrentGUILayer;
            int cursor = 0;

            try
            {
                if (JsetBin.Count == 0)
                {
                    stringList.Add("No limbs loaded!\n");
                }

                for (int jSetIndex = 0; jSetIndex < JsetBin.Count; jSetIndex++)
                {
                    debugBin.Append($"jSet: {jSetIndex}\n");

                    if (CurrentGUILayer == GUILayer.JSET &&
                              SelObjIndex[0] == jSetIndex)
                    {
                        CursorIndex = stringList.Count - 1;
                        cursor = 1;
                    }
                    else
                        cursor = 0;

                    stringList.Add(Cursor[cursor] + "|" + jSetIndex + ":" + JsetBin[jSetIndex].Name);

                    if (layer < 1 ||
                        SelObjIndex[0] != jSetIndex)
                        continue;

                    if (JsetBin[jSetIndex].Sequences.Count == 0)
                    {
                        stringList.Add(" No sequences found!");
                    }

                    for (int seqIndex = 0; seqIndex < JsetBin[jSetIndex].Sequences.Count; seqIndex++)
                    {
                        if (CurrentGUILayer == GUILayer.SEQUENCE &&
                                  SelObjIndex[1] == seqIndex)
                        {
                            CursorIndex = stringList.Count - 1;
                            cursor = 1;
                        }
                        else
                            cursor = 0;

                        stringList.Add(Cursor[cursor] + " |" + seqIndex + ":" + JsetBin[jSetIndex].Sequences[seqIndex].Name);
                        if (layer < 2 ||
                            SelObjIndex[1] != seqIndex)
                            continue;

                        if (JsetBin[jSetIndex].Sequences[seqIndex].Frames.Count == 0)
                            stringList.Add("  No frames found!");

                        for (int frameIndex = 0; frameIndex < JsetBin[jSetIndex].Sequences[seqIndex].Frames.Count; frameIndex++)
                        {
                            if (CurrentGUILayer == GUILayer.FRAME &&
                                      SelObjIndex[2] == frameIndex)
                            {
                                CursorIndex = stringList.Count - 1;
                                cursor = 1;
                            }
                            else
                                cursor = 0;

                            stringList.Add($"{Cursor[cursor]}  |{frameIndex} : {JsetBin[jSetIndex].Sequences[seqIndex].Frames[frameIndex].Name}");
                            if (layer < 3 ||
                                SelObjIndex[2] != frameIndex)
                                continue;

                            if (JsetBin[jSetIndex].Joints.Length == 0)
                                stringList.Add("   No joints found!");

                            for (int jFrameIndex = 0; jFrameIndex < JsetBin[jSetIndex].Sequences[seqIndex].Frames[frameIndex].Jframes.Count(); jFrameIndex++)
                            {
                                if (CurrentGUILayer == GUILayer.JOINT &&
                                          SelObjIndex[3] == jFrameIndex)
                                {
                                    CursorIndex = stringList.Count - 1;
                                    cursor = 1;
                                }
                                else
                                    cursor = 0;

                                stringList.Add($"{Cursor[cursor]}    |{jFrameIndex}:{JsetBin[jSetIndex].Sequences[seqIndex].Frames[frameIndex].Jframes[jFrameIndex].Joint.Name}:{JsetBin[jSetIndex].Sequences[seqIndex].Frames[frameIndex].Jframes[jFrameIndex].LerpPoint}");
                            }
                        }
                    }
                }
            }
            catch
            {
                debugBin.Append("FAIL POINT!\n");
            }

            string[] output = stringList.ToArray();
            return output;
        }
        string[] OptionsStringBuilder()
        {
            List<string> stringList = new List<string>();
            stringList.Add("===Options===");
            stringList.Add("=============");

            try
            {
                stringList.Add($"Ignoring Feet: {CurrentWalkSet.bIgnoreFeet}");
                stringList.Add($"Ignoring Save: {bIgnoreSave}");
                stringList.Add($"Auto demo: {bAutoDemo}");
                stringList.Add($"Planeing: {bPlaneing}");
                stringList.Add($"Stator control: {bStatorControl}");
                stringList.Add($"Stator target: {bStatorTarget}");
                stringList.Add($"Lerp Speed: {CurrentWalk.CurrentClockSpeed}");
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
                stringList.Add($"Walk-state: {CurrentWalk.ClockMode}");
                stringList.Add($"Frame index: {CurrentWalk.CurrentFrameIndex}");
                stringList.Add($"Lerp Speed: {CurrentWalk.CurrentClockSpeed}");
                stringList.Add($"Left foot locked: {CurrentWalkSet.Feet[0].Locked}");
                stringList.Add($"Right foot locked: {CurrentWalkSet.Feet[1].Locked}");
            }
            catch
            {
                stringList.Add("Error!");
            }

            string[] output = stringList.ToArray();
            return output;
        }
        string FormattedGUIStringBuilder(string[] input)
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

            if (!bCapLines)
                for (int i = 2; i < input.Length; i++)
                    output += input[i] + "\n";
            else
                for (int i = startIndex; i < startIndex + (2 * LineBufferSize) && i < input.Length; i++)
                    output += input[i] + "\n";

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

                case GUIMode.CONTROL:
                    ControlMenuFunctions(button);
                    break;

                case GUIMode.OPTIONS:
                    OptionsMenuFunctions(button);
                    break;
            }
            GUIUpdate();
        }
        void OptionsMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    CurrentWalkSet.bIgnoreFeet = !CurrentWalkSet.bIgnoreFeet;
                    break;

                case 2:
                    bIgnoreSave = !bIgnoreSave;
                    break;

                case 3:
                    bPlaneing = !bPlaneing;
                    CurrentWalkSet.TogglePlaneing(bPlaneing);
                    break;

                case 4:
                    bStatorControl = !bStatorControl;
                    break;

                case 5:
                    bStatorTarget = !bStatorTarget;
                    break;

                case 6:
                    bAutoDemo = !bAutoDemo;
                    break;

                case 7:
                    CurrentWalk.UpdateClockSpeed();
                    break;

                case 8:
                    CurrentWalk.UpdateClockSpeed(false);
                    break;

                case 9:
                    CurrentGUIMode = GUIMode.MAIN;
                    break;
            }
        }
        void ControlMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    DebugBinStatic.Append($"Lock Left Foot Success: {CurrentWalkSet.InitializeGrip(ref DebugBinStatic)}\n");
                    DebugScreens[1].WriteText(DebugBinStatic.ToString());
                    break;

                case 2:
                    DebugBinStatic.Append($"Lock Right Foot Success: {CurrentWalkSet.InitializeGrip(ref DebugBinStatic, false)}\n");
                    DebugScreens[1].WriteText(DebugBinStatic.ToString());
                    break;

                case 3:
                    CurrentWalkSet.UnlockFeet();
                    break;

                case 4:
                    CurrentWalkSet.SnapShotPlane();
                    break;

                case 5:
                    bWalking = !bWalking;
                    WalkToggle();
                    break;

                case 6:
                    if (OldWalkState == ClockMode.FOR)
                        OldWalkState = ClockMode.REV;
                    else
                        OldWalkState = ClockMode.FOR;
                    WalkToggle();
                    break;

                case 7:
                    CurrentWalk.InitializeSeq(ClockMode.FOR, 0);
                    OldWalkState = ClockMode.FOR;
                    bWalking = true;
                    break;

                case 8:
                    ZeroCurrentWalk();
                    break;

                case 9:
                    CurrentGUIMode = GUIMode.MAIN;
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
                    CurrentGUIMode = GUIMode.LIBRARY;
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
        void LibraryMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    ChangeSnappingValue();
                    break;

                case 2:
                    IncrementItem(false);
                    break;

                case 3:
                    IncrementItem(true);
                    break;

                case 4:
                    LoadItem();
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

            RefreshSelection();
        }
        void RefreshSelection()
        {
            JointSet limb = null;
            if (JsetBin.Count > SelObjIndex[0])
                limb = JsetBin[SelObjIndex[0]];

            Sequence seq = null;
            if (limb != null && limb.Sequences.Count > SelObjIndex[1])
                seq = limb.Sequences[SelObjIndex[1]];

            KeyFrame frame = null;
            if (seq != null && seq.Frames.Count > SelObjIndex[2])
                frame = seq.Frames[SelObjIndex[2]];

            int seqCount = limb == null ? 0 : limb.Sequences.Count;
            int frameCount = seq == null ? 0 : seq.Frames.Count;
            int jointCount = limb == null ? 0 : limb.Joints.Length;

            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    SelectedLineIndex = SelectedLineIndex >= JsetBin.Count ? 0 : SelectedLineIndex;
                    SelectedLineIndex = SelectedLineIndex < 0 ? JsetBin.Count - 1 : SelectedLineIndex;
                    SelectedLineIndex = SelectedLineIndex < 0 ? 0 : SelectedLineIndex;
                    SelObjIndex[0] = SelectedLineIndex;
                    break;

                case GUILayer.SEQUENCE:
                    SelectedLineIndex = SelectedLineIndex >= seqCount ? 0 : SelectedLineIndex;
                    SelectedLineIndex = SelectedLineIndex < 0 ? seqCount - 1 : SelectedLineIndex;
                    SelectedLineIndex = SelectedLineIndex < 0 ? 0 : SelectedLineIndex;
                    SelObjIndex[1] = SelectedLineIndex;
                    break;

                case GUILayer.FRAME:
                    SelectedLineIndex = SelectedLineIndex >= frameCount ? 0 : SelectedLineIndex;
                    SelectedLineIndex = SelectedLineIndex < 0 ? frameCount - 1 : SelectedLineIndex;
                    SelectedLineIndex = SelectedLineIndex < 0 ? 0 : SelectedLineIndex;
                    SelObjIndex[2] = SelectedLineIndex;
                    break;

                case GUILayer.JOINT:
                    SelectedLineIndex = SelectedLineIndex >= jointCount ? 0 : SelectedLineIndex;
                    SelectedLineIndex = SelectedLineIndex < 0 ? jointCount - 1 : SelectedLineIndex;
                    SelectedLineIndex = SelectedLineIndex < 0 ? 0 : SelectedLineIndex;
                    SelObjIndex[3] = SelectedLineIndex;
                    break;
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
        void LoadItem()
        {
            if (CurrentGUILayer == GUILayer.JOINT) // do nothing
                return;

            CurrentWalkSet = JsetBin[SelObjIndex[0]];
            if (CurrentGUILayer == GUILayer.JSET)
                return;

            CurrentWalk = CurrentWalkSet.Sequences[SelObjIndex[1]];
            if (CurrentGUILayer == GUILayer.SEQUENCE)
                return;

            CurrentWalk.DemoKeyFrame(SelObjIndex[2], ref DebugBinStatic);
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
                        JsetBin[SelObjIndex[0]].Name = name;
                        break;

                    case GUILayer.SEQUENCE:
                        JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Name = name;
                        break;

                    case GUILayer.FRAME:
                        JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]].Name = name;
                        break;

                    case GUILayer.JOINT:
                        if (floatGood)
                        {
                            JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]].Jframes[SelObjIndex[3]].ChangeStatorLerpPoint(value);
                        }
                        else
                            JsetBin[SelObjIndex[0]].Joints[SelObjIndex[3]].Name = name;
                        break;
                }
            }
            catch
            {
                // I dunno, I'm tired bruh
            }
        }
        void AddItem()
        {
            string name = null;
            UserInputString(ref name);

            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    JointSet newSet = ConstructJointSet(ref DebugBinStatic, Control, JsetBin.Count); // build new set index's based on existing count for now, will need re-indexing when a set is removed
                    if (newSet != null)
                        JsetBin.Add(newSet);
                    break;

                case GUILayer.SEQUENCE:
                    if (name == null)
                        name = $"New Sequence {JsetBin[SelObjIndex[0]].Sequences.Count}";
                    new Sequence(name, JsetBin[SelObjIndex[0]]);
                    break;

                case GUILayer.FRAME:
                    if (name == null)
                        name = $"New Frame {JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames.Count}";

                    int index;
                    if (SelObjIndex[2] >= JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames.Count)
                        index = -1;
                    else
                        index = SelObjIndex[2];

                    DebugBinStatic.Append($"Frame Generation Success: {JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].AddKeyFrameSnapshot(ref DebugBinStatic, index, name, bSnapping)}\n");
                    break;
            }

            RefreshSelection();
            DebugScreens[1].WriteText(DebugBinStatic.ToString());
        }
        void DeleteItem()
        {
            switch (CurrentGUILayer)
            {
                case GUILayer.JSET:
                    JsetBin.RemoveAt(SelObjIndex[0]);
                    break;

                case GUILayer.SEQUENCE:
                    JsetBin[SelObjIndex[0]].Sequences.RemoveAt(SelObjIndex[1]);
                    break;

                case GUILayer.FRAME:
                    JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].RemoveKeyFrameAtIndex(SelObjIndex[2]);
                    break;
            }

            int a = JsetBin.Count;
            if (SelObjIndex[0] >= a && a > 0)
            {
                SelObjIndex[0] = a - 1;
                SelectedLineIndex--;
            }
            if (a == 0 || JsetBin[SelObjIndex[0]] == null)
            {
                SelObjIndex[1] = 0;
                SelObjIndex[2] = 0;
                SelObjIndex[3] = 0;
                return;
            }

            int b = JsetBin[SelObjIndex[0]].Sequences.Count;
            if (SelObjIndex[1] >= b && b > 0)
            {
                SelObjIndex[1] = b - 1;
                SelectedLineIndex--;
            }
            if (b == 0 || JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]] == null)
            {
                SelObjIndex[2] = 0;
                SelObjIndex[3] = 0;
                return;
            }

            int c = JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames.Count;
            if (SelObjIndex[2] >= c && c > 0)
            {
                SelObjIndex[2] = c - 1;
                SelectedLineIndex--;
            }
            if (c == 0 || JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]] == null)
            {
                SelObjIndex[3] = 0;
                return;
            }
        }
        void ChangeSnappingValue()
        {
            UserInputFloat(ref Snapping);
        }
        void IncrementItem(bool incr = true)
        {
            if (CurrentGUILayer != GUILayer.JOINT)
                return;

            JointFrame jFrame = JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]].Jframes[SelObjIndex[3]];

            float newLerpPoint = incr ? jFrame.LerpPoint + Snapping : jFrame.LerpPoint - Snapping;
            jFrame.ChangeStatorLerpPoint(newLerpPoint);
        }
        #endregion

        #region UPATES 

        void WalkManager()
        {
            if (CurrentWalkSet == null)
                return;

            CurrentWalkSet.UpdatePlane(ref DebugBinStream, ref RotationBuffer);

            if (CurrentWalk == null ||
                bWalking == false)
                return;

            CurrentWalk.UpdateSequence(ref DebugBinStream);
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
            if (JsetBin == null ||
                JsetBin.Count == 0)
                return;

            if (!bStatorControl)
                return;

            foreach (JointSet set in JsetBin)
            {
                foreach (Joint joint in set.Joints)
                {
                    joint.UpdateJoint(bStatorTarget, Runtime.TimeSinceLastRun.TotalMilliseconds, ref DebugBinStream);
                }

                foreach (Foot foot in set.Feet)
                    foreach (Joint toe in foot.Toes)
                    {
                        toe.UpdateJoint(bStatorTarget, Runtime.TimeSinceLastRun.TotalMilliseconds, ref DebugBinStream);
                    }
            }
        }
        void DisplayManager()
        {
            try
            {
                Echo($"GUIstatus: {GUIstatus(DebugScreens[2], DebugScreens[1])}");
                Echo($"PlaneLiveStatus: {PlaneingLiveStatus(DebugScreens[4])}");
                Echo($"MechStatus: {MechStatus(CockPitScreens[0])}");
                Echo($"DebugPlus!: {DebugPlus(DebugScreens[3])}");
                Echo($"SplashScreen: {SplashScreen(CockPitScreens[1])}");
            }
            catch
            {
                Echo("Missing Screen!");
            }
        }
        #endregion

        #region DISPLAY
        bool GUIstatus(IMyTextSurface gui, IMyTextSurface buttons)
        {
            if (gui == null)
                return false;

            gui.WriteText(GUIBuilder);
            if (buttons != null)
                buttons.WriteText(ButtonBuilder);

            return true;
        }
        bool PlaneingLiveStatus(IMyTextSurface panel)
        {
            if (panel == null)
                return false;

            DisplayManagerBuilder.Clear();
            try
            {
                DisplayManagerBuilder.Append($"Current Plane Block: {CurrentWalkSet.Plane.CustomName}\n");

                MatrixD T = CurrentWalkSet.CurrentPlane;
                MatrixD L = CurrentWalkSet.Plane.WorldMatrix;
                Vector3 B = CurrentWalkSet.CorrectBuffer;

                DisplayManagerBuilder.Append(
                    $"Forward Target:\n{T.Forward.X:0.###}:{T.Forward.Y:0.###}:{T.Forward.Z:0.###}\n" +
                    $"Forward Literal:\n{L.Forward.X:0.###}:{L.Forward.Y:0.###}:{L.Forward.Z:0.###}\n" +
                    $"Right Target:\n{T.Right.X:0.###}:{T.Right.Y:0.###}:{T.Right.Z:0.###}\n" +
                    $"Right Literal:\n{L.Right.X:0.###}:{L.Right.Y:0.###}:{L.Right.Z:0.###}\n" +
                    $"Up Target:\n{T.Up.X:0.###}:{T.Up.Y:0.###}:{T.Up.Z:0.###}\n" +
                    $"Up Literal:\n{L.Up.X:0.###}:{L.Up.Y:0.###}:{L.Up.Z:0.###}\n" +
                    $"Corrections:\n{B.X:0.###}:{B.Y:0.###}:{B.Z:0.###}\n" +
                    $"Finals:\n{CurrentWalkSet.Feet[0].Planes[0].ActiveTarget:0.###}\n" +
                    $"{CurrentWalkSet.Feet[0].Planes[1].ActiveTarget:0.###}\n" +
                    $"{CurrentWalkSet.Feet[0].Planes[2].ActiveTarget:0.###}\n");
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }
            panel.WriteText(DisplayManagerBuilder);
            return true;
        }
        bool MechStatus(IMyTextSurface panel)
        {
            if (panel == null)
                return false;

            DisplayManagerBuilder.Clear();

            try
            {
                DisplayManagerBuilder.Append("Mech Status:\n\n");

                DisplayManagerBuilder.Append($"SampleWalkClockState: {CurrentWalk.ClockMode}\n");
                DisplayManagerBuilder.Append($"CurrentWalkClockTime: {CurrentWalk.CurrentClockTime}\n");
                DisplayManagerBuilder.Append($"CurrentWalkFrameIndex: {CurrentWalk.CurrentFrameIndex}\n");
                DisplayManagerBuilder.Append($"CurrentLegsIgnoringFeet: {CurrentWalkSet.bIgnoreFeet}\n\n");

                DisplayManagerBuilder.Append($"Walking: {bWalking}\n");
                DisplayManagerBuilder.Append($"TargetActive: {bStatorTarget}\n");
                DisplayManagerBuilder.Append($"StatorControlActive: {bStatorControl}\n");
                DisplayManagerBuilder.Append($"Snapping: {bSnapping}\n");
                DisplayManagerBuilder.Append($"SnappingValue: {Snapping}\n\n");

                DisplayManagerBuilder.Append($"Planeing: {bPlaneing}\n");
                DisplayManagerBuilder.Append($"IgnoreSave: {bIgnoreSave}");

                CockPitScreens[0].WriteText(DisplayManagerBuilder.ToString());
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }
            panel.WriteText(DisplayManagerBuilder.ToString());
            return true;
        }
        bool DebugPlus(IMyTextSurface panel)
        {
            if (panel == null)
                return false;

            DisplayManagerBuilder.Clear();

            try
            {
                DisplayManagerBuilder.Append("Debug Plus!:\n");

                DisplayManagerBuilder.Append($"\n=-=-=-=-=-=-=\n{DebugBinStream.ToString()}");
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }
            panel.WriteText(DisplayManagerBuilder.ToString());

            return true;
        }
        bool SplashScreen(IMyTextSurface panel)
        {
            if (panel == null)
                return false;

            DisplayManagerBuilder.Clear();

            try
            {
                DisplayManagerBuilder.Append("Splash Panel!:\n");
                DisplayManagerBuilder.Append($"CurrentLayer: {CurrentGUILayer}\n");
                DisplayManagerBuilder.Append($"CurrentMode: {CurrentGUIMode}\n");

                int k = 0;
                foreach (int index in SelObjIndex)
                {
                    DisplayManagerBuilder.Append($"Layer:{(GUILayer)k} | Index:{index}\n");
                    k++;
                }
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }
            panel.WriteText(DisplayManagerBuilder.ToString());
            return true;
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
                GUIBuilder = new StringBuilder();
                SaveData = new StringBuilder();

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

                Runtime.UpdateFrequency = UpdateFrequency.Update1;
                bInitialized = true;
            }
            catch
            {
                bInitialized = false;
                return;
            }

            DebugBinStatic.Clear();

            if (!Load(ref DebugBinStatic))
            {
                DebugBinStatic.Append("Load Failed!\n");
                SampleLegsConstructor(ref DebugBinStatic);

                bLoaded = false;
            }
            else
            {
                DebugBinStatic.Append("Load Success!\n");
                bLoaded = true;
            }

            DebugScreens[1].WriteText(DebugBinStatic.ToString());

            GUIUpdate();

            DebugScreens[1].WriteText(DebugBinStatic);

            ZeroCurrentWalk();
        }
        public void Main(string argument, UpdateType updateSource)
        {
            if (!bInitialized)
                return;

            DebugBinStream.Clear(); // MUST HAPPEN!

            switch (argument)
            {
                case "LOCK_LEFT":
                    DebugBinStatic.Append($"Lock Left Foot Success: {CurrentWalkSet.InitializeGrip(ref DebugBinStatic)}");
                    DebugScreens[1].WriteText(DebugBinStatic.ToString());
                    break;

                case "LOCK_RIGHT":
                    DebugBinStatic.Append($"Lock Right Foot Success: {CurrentWalkSet.InitializeGrip(ref DebugBinStatic, false)}");
                    DebugScreens[1].WriteText(DebugBinStatic.ToString());
                    break;

                case "UNLOCK_FEET":
                    CurrentWalkSet.UnlockFeet();
                    break;

                case "TOGGLE_FEET":
                    CurrentWalkSet.bIgnoreFeet = !CurrentWalkSet.bIgnoreFeet;
                    break;

                case "SNAPSHOT_PLANE":
                    CurrentWalkSet.SnapShotPlane();
                    break;

                case "TOGGLE_PLANE":
                    bPlaneing = !bPlaneing;
                    CurrentWalkSet.TogglePlaneing(bPlaneing);
                    break;

                case "TOGGLE_SAVE":
                    bIgnoreSave = !bIgnoreSave;
                    break;

                case "RESET":
                    SampleLegsConstructor(ref DebugBinStatic);
                    break;

                case "TOGGLE_STATOR_TARGET":
                    bStatorTarget = !bStatorTarget;
                    break;

                case "TOGGLE_STATOR_CONTROL":
                    bStatorControl = !bStatorControl;
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

                case "INCREASE_SPEED":
                    CurrentWalk.UpdateClockSpeed();
                    break;

                case "DECREASE_SPEED":
                    CurrentWalk.UpdateClockSpeed(false);
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
                    bForceSave = true;
                    Save();
                    bForceSave = false;
                    break;

                case "LOAD":
                    Load(ref DebugBinStatic);
                    DebugScreens[1].WriteText(DebugBinStatic.ToString());
                    break;

                case "CLEAR_DEBUG":
                    DebugBinStatic.Clear();
                    DebugScreens[1].WriteText("");
                    break;

                default:
                    if (!argument.Contains("BUTTON:"))
                        break;
                    string code = argument.Split(':')[1];
                    int button = 0;
                    if (int.TryParse(code, out button))
                        ButtonPress(button);
                    break;
            }

            ControlInput();
            WalkManager();
            AnimationManager();
            StatorManager();
            DisplayManager();
        }
        public bool Load(ref StringBuilder debugBin)
        {
            /*
Tolkens:
:  - Divider
/n - Entry

&  - Options (&:IgnoreSave:AutoDemo:Planeing:StatorControl:StatorTarget)
#  - JointSet (#:Index:GroupName:Name:IgnoreFeet)
$  - Sequence ($:Name:LerpSpeed)
%0 - KeyFrame (%0:Name)
%1 - JointFrame (%1:LerpPoint)
 */
            JsetBin.Clear();

            string[] load = Me.CustomData.Split('\n');

            List<JointFrame> jFrameBuffer = new List<JointFrame>();
            List<KeyFrame> kFrameBuffer = new List<KeyFrame>();
            JointSet current = null;

            int debugCounter = 0;

            foreach (string next in load)
            {
                try
                {
                    string[] entry = next.Split(':');

                    switch (entry[0])
                    {
                        case "&":
                            bIgnoreSave = bool.Parse(entry[1]);
                            bAutoDemo = bool.Parse(entry[2]);
                            bPlaneing = bool.Parse(entry[3]);
                            bStatorControl = bool.Parse(entry[4]);
                            bStatorTarget = bool.Parse(entry[5]);
                            break;

                        case "#":
                            current = ConstructJointSet(ref DebugBinStatic, Control, int.Parse(entry[1]), entry[2], entry[3], bool.Parse(entry[4]));
                            if (current == null)
                                break;
                            JsetBin.Add(current);
                            break;

                        case "%1":
                            jFrameBuffer.Add(new JointFrame(current.Joints[jFrameBuffer.Count], float.Parse(entry[1])));
                            break;

                        case "%0":
                            kFrameBuffer.Add(new KeyFrame(entry[1], jFrameBuffer.ToArray()));
                            jFrameBuffer.Clear();
                            break;

                        case "$":
                            List<KeyFrame> newFrames = new List<KeyFrame>();
                            newFrames.AddRange(kFrameBuffer);
                            new Sequence(entry[1], current, newFrames, float.Parse(entry[2]));
                            kFrameBuffer.Clear();
                            break;
                    }
                }
                catch
                {
                    DebugBinStatic.Append("Fail Point!\n");
                    JsetBin.Clear();
                    return false;
                }
                debugCounter++;
            }

            if (JsetBin.Count < 1 ||
                JsetBin[0] == null)
                return false;

            if (JsetBin[0].Sequences == null)
                return false;

            if (JsetBin[0].Sequences.Count < 1)
                return false;

            if (JsetBin[0].Sequences[0] != null)
            {
                CurrentWalkSet = JsetBin[0];           //  >: |
                CurrentWalk = JsetBin[0].Sequences[0]; //  >: |
                return true;
            }

            return false;
        }
        public void Save()
        {
            /*
            Tolkens:
            :  - Divider
            &  - Options (&:IgnoreSave:AutoDemo:Planeing:StatorControl:StatorTarget)
            #  - JointSet (#:Index:GroupName:Name:IgnoreFeet)
            $  - Sequence ($:Name:LerpSpeed)
            %0 - KeyFrame (%0:Name)
            %1 - JointFrame (%1:LerpPoint)


             */

            /* toe-pads = T:name:sInd:uInd:fInd:n/a

                  joint = J:name:sInd:uInd:n/a :n/a
                   grip = G:name:sInd:uInd:fInd:dir
                   roll = R:name:sInd:uInd:fInd:dir
                  pitch = P:name:sInd:uInd:fInd:dir
                    yaw = Y:name:sInd:uInd:fInd:dir */

            if (bIgnoreSave && !bForceSave)
                return;

            SaveData.Clear();

            SaveData.Append($"&:{bIgnoreSave}:{bAutoDemo}:{bPlaneing}:{bStatorControl}:{bStatorTarget}\n");

            foreach (JointSet set in JsetBin)
            {
                foreach (Joint joint in set.Joints)
                    joint.Connection.CustomData = $"J:{joint.Name}:{set.Index}:{joint.Index}:-1:0";

                for (int i = 0; i < set.Feet.Length; i++)
                {
                    if (set.Feet[i] == null)
                        continue;

                    if (set.Feet[i].Planes[0] != null)
                        set.Feet[i].Planes[0].Connection.CustomData = $"P:{set.Feet[i].Planes[0].Name}:{set.Index}:{set.Feet[i].Planes[0].Index}:{i}:{set.Feet[i].Planes[0].AuxDirection}";

                    if (set.Feet[i].Planes[1] != null)
                        set.Feet[i].Planes[1].Connection.CustomData = $"Y:{set.Feet[i].Planes[1].Name}:{set.Index}:{set.Feet[i].Planes[1].Index}:{i}:{set.Feet[i].Planes[1].AuxDirection}";

                    if (set.Feet[i].Planes[2] != null)
                        set.Feet[i].Planes[2].Connection.CustomData = $"R:{set.Feet[i].Planes[2].Name}:{set.Index}:{set.Feet[i].Planes[2].Index}:{i}:{set.Feet[i].Planes[2].AuxDirection}";

                    for (int j = 0; j < set.Feet[i].Toes.Length; j++)
                        set.Feet[i].Toes[j].Connection.CustomData = $"G:{set.Feet[i].Toes[j].Name}:{set.Index}:{set.Feet[i].Toes[j].Index}:{i}:{set.Feet[i].Toes[j].AuxDirection}";

                    for (int j = 0; j < set.Feet[i].Pads.Length; j++)
                        set.Feet[i].Pads[j].CustomData = $"T:{set.Feet[i].Pads[j].Name}:{set.Index}:{j}:{i}:0";
                }

                SaveData.Append($"#:{set.Index}:{set.GroupName}:{set.Name}:{set.bIgnoreFeet}\n");

                foreach (Sequence seq in set.Sequences)
                {
                    foreach (KeyFrame frame in seq.Frames)
                    {
                        foreach (JointFrame jFrame in frame.Jframes)
                        {
                            SaveData.Append($"%1:{jFrame.LerpPoint}\n");
                        }
                        SaveData.Append($"%0:{frame.Name}\n");
                    }
                    SaveData.Append($"$:{seq.Name}:{seq.CurrentClockSpeed}\n");
                }
            }
            Me.CustomData = SaveData.ToString();
        }

        #endregion
        #endregion
    }
}
