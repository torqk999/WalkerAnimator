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

    //SHA256:Gjr4HXe4MUH4BlceqwS7uN4QFotJRKzGo0VdsjjcQm4 clark_thomson2001@yahoo.com

    partial class Program : MyGridProgram
    {
        #region MAIN

        #region CONSTS
        const string CockpitName = "PILOT";
        const string LCDgroupName = "LCDS";
        const string Digits = "0.###";

        const float Threshold = .02f;
        const float VelocityScalar = .5f;
        const float CorrectionScalar = .1f;

        const float MaxAccel = 0.3f;
        const float MaxSpeed = 10f;
        const float ClockIncrmentMag = 0.0005f;
        const float ClockSpeedDef = 0.005f;
        const float ClockSpeedMin = 0.001f;
        const float ClockSpeedMax = 0.020f;
        const float TriggerCap = 0.6f;
        const float LookScalar = 0.005f;
        const float RollScalar = 0.05f;

        const int SaveBlockCountSize = 5;
        const double RAD2DEG = 180 / Math.PI;
        #endregion

        #region REFS

        IMyCockpit Control;
        
        List<JointSet> JsetBin = new List<JointSet>();
        List<Sequence> Animations;

        Sequence CurrentWalk;
        JointSet CurrentWalkSet;

        #endregion

        #region LOGIC

        bool IgnoreFeet = false;
        bool IgnoreSave = true;
        bool ForceSave = false;
        bool Initialized = false;
        bool Planeing = false;
        bool Snapping = true;
        bool AutoDemo = false;
        bool StatorTarget = true;
        bool StatorControl = true;

        Vector3 RotationBuffer;
        float SnapValue = 5;
        int LastMechInput;
        int[] LastLibraryInput = new int[2];

        #endregion

        #region GUI VARS
        GUIMode CurrentGUIMode = GUIMode.MAIN;
        GUILayer CurrentGUILayer = GUILayer.JSET;
        GUILayer JsetGUILayer = GUILayer.SEQUENCE;

        bool CapLines = true;
        int CursorIndex = 0;
        int LineBufferSize = 6;
        int[] SelObjIndex = new int[Enum.GetNames(typeof(GUILayer)).Length];

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
            "Change Snapping",
            "Decrement",
            "Increment",
            "Load Item",
            "New/Move Item",
            "Delete Item",
            "Edit Item",
            "Main Menu",

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
        static readonly string[][] AllButtons =
        {
            MainMenuButtons,
            InfoMenuButtons,
            CreateMenuButtons,
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

        const string MainText = "Mech Control v0.5";
        const string InfoText = "(InfoScreen)";
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
            CONTROL,
            OPTIONS
        }
        public enum GUILayer
        {
            JSET = 0,
            FOOT = 1,
            SUB_FOOT = 2,
            SEQUENCE = 1,
            FRAME = 2,
            JOINT = 3
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
            SPLASH = 2,
            CONTROLS = 1,
            MECH_STATUS = 0
        }
        #endregion

        #region CLASSES & STRUCTS
        struct SetData
        {
            public int Index;
            public string Name;
            public string GroupName;
            public IMyTerminalBlock Plane;

            public SetData(string input, IMyTerminalBlock plane = null)
            {
                try
                {
                    string[] data = input.Split(':');
                    Index = int.Parse(data[1]);
                    GroupName = data[2];
                    Name = data[3];
                }
                catch
                {
                    Index = -1;
                    GroupName = "null";
                    Name = "null";
                }

                Plane = plane;
            }

            public string Save()
            {
                return $"#:{Index}:{GroupName}:{Name}\n";
            }
        }
        struct JointData
        {
            public char TAG;
            public string Name;
            public int ParentIndex;
            public int IDindex;
            public int FootIndex;

            public JointData(string input)
            {
                try
                {
                    string[] data = input.Split(':');
                    TAG = data[0][0];
                    Name = data[1];
                    ParentIndex = int.Parse(data[2]);
                    IDindex = int.Parse(data[3]);
                    FootIndex = int.Parse(data[4]);
                }
                catch
                {
                    TAG = 'N';
                    Name = "Null";
                    ParentIndex = -1;
                    IDindex = -1;
                    FootIndex = -1;
                }
            }
            public string Save()
            {
                return $"{TAG}:{Name}:{ParentIndex}:{IDindex}:{FootIndex}";
            }
        }
        struct FootData
        {
            public bool Parsed;
            public int ParentIndex;
            public int FootIndex;
            public string PadsGroupName;
            public string GripsGroupName;
            public string PlanarsGroupName;

            public FootData(string input)
            {
                try
                {
                    string[] data = input.Split(':');
                    ParentIndex = int.Parse(data[1]);
                    FootIndex = int.Parse(data[2]);
                    PadsGroupName = data[3];
                    GripsGroupName = data[4];
                    PlanarsGroupName = data[5];
                    Parsed = true;
                }
                catch
                {
                    ParentIndex = -1;
                    FootIndex = -1;
                    PadsGroupName = "null";
                    GripsGroupName = "null";
                    PlanarsGroupName = "null";
                    Parsed = false;
                }
            }

            public string Save()
            {
                return $"@:{ParentIndex}:{FootIndex}:{PadsGroupName}:{GripsGroupName}:{PlanarsGroupName}\n";
            }
        }

        class Joint
        {
            public JointData Data;
            public IMyMechanicalConnectionBlock Connection;

            public double[] LerpPoints = new double[2];
            public bool Planeing = false;
            public bool Gripping = false;

            //////////////////////////////

            public double PlaneCorrection;
            public double AnimTarget;
            public double ActiveTarget;

            public int GripDirection;
            public int CorrectionDir;
            public double CorrectionMag;
            public double StatorVelocity;
            public double LiteralVelocity;
            public Vector3 PlanarDots;

            double OldVelocity;
            double LastPosition;
            DateTime LastTime;

            public Joint(IMyMechanicalConnectionBlock mechBlock, JointData data)
            {
                Connection = mechBlock;
                mechBlock.Enabled = true;
                Data = data;
            }

            public void SaveData()
            {
                if (Connection != null)
                    Connection.CustomData = Data.Save();
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
            public void OverwriteAnimTarget(double value)
            {
                AnimTarget = value;
            }

            public void UpdateJoint(bool activeTargetTracking, double delta, ref StringBuilder debugStream)
            {
                UpdateLiteralVelocity();
                if (!activeTargetTracking)
                {
                    UpdateStatorVelocity(ref debugStream, activeTargetTracking);
                    UpdateStator();
                    return;
                }

                ActiveTarget = AnimTarget;

                UpdateCorrectionDisplacement(ref debugStream);

                if (Planeing)
                {
                    UpdatePlaneDisplacement(ref debugStream);
                    UpdateCorrectionDisplacement(ref debugStream);
                }

                UpdateStatorVelocity(ref debugStream, activeTargetTracking);
                UpdateStator();
            }
            void UpdateLiteralVelocity()
            {
                double currentPosition = ReturnCurrentStatorPosition();
                DateTime now = DateTime.Now;

                LiteralVelocity = ((currentPosition - LastPosition) / 360) / (now - LastTime).TotalMinutes;

                LastTime = now;
                LastPosition = currentPosition;
            }
            void UpdateStatorVelocity(ref StringBuilder debugStream, bool active)
            {
                if (active)
                {
                    OldVelocity = StatorVelocity;
                    if (Data.TAG == 'G')
                    {
                        StatorVelocity = MaxSpeed * (Gripping ? -1 : 1); // Needs changing!
                    }
                    else
                    {
                        double scale = CorrectionMag * VelocityScalar;
                        StatorVelocity = CorrectionDir * scale;

                        if (scale < Threshold)
                            StatorVelocity = 0;

                        StatorVelocity = (Math.Abs(StatorVelocity - OldVelocity) > MaxAccel) ? OldVelocity + (MaxAccel * Math.Sign(StatorVelocity - OldVelocity)) : StatorVelocity;
                        StatorVelocity = (Math.Abs(StatorVelocity) > MaxSpeed) ? MaxSpeed * Math.Sign(StatorVelocity) : StatorVelocity;
                    }
                }
                else
                    StatorVelocity = 0;
            }

            public void UpdatePlanarDot(MatrixD plane)
            {
                PlanarDots.X = Vector3.Dot(ReturnRotationAxis(), Vector3.Right);// plane.Right);//
                PlanarDots.Y = Vector3.Dot(ReturnRotationAxis(), Vector3.Up);// plane.Up);//
                PlanarDots.Z = Vector3.Dot(ReturnRotationAxis(), Vector3.Forward);// plane.Forward);//
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

            public virtual void LerpAnimationFrame(float lerpTime, ref StringBuilder debugBin)
            {
                if (lerpTime > 1 ||
                    lerpTime < 0)
                    return;
            }
            public virtual void UpdatePlaneDisplacement(ref StringBuilder debugBin)
            {
                if (!Planeing)
                    return;

                PlaneCorrection -= (CorrectionMag * CorrectionDir);
                ActiveTarget += PlaneCorrection;
            }
            public virtual void UpdateCorrectionDisplacement(ref StringBuilder debugBin)
            {

            }
            public virtual void UpdateStator()
            {

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
            public override void LerpAnimationFrame(float lerpTime, ref StringBuilder debugBin)
            {
                base.LerpAnimationFrame(lerpTime, ref debugBin);

                AnimTarget = LerpPoints[0] + ((LerpPoints[1] - LerpPoints[0]) * lerpTime);
            }
            public override void UpdateCorrectionDisplacement(ref StringBuilder debugBin)
            {
                CorrectionMag = ActiveTarget - PistonBase.CurrentPosition;
                CorrectionDir = Math.Sign(CorrectionMag);
                CorrectionMag = Math.Abs(CorrectionMag);
            }
            public override void UpdatePlaneDisplacement(ref StringBuilder debugBin)
            {
                base.UpdatePlaneDisplacement(ref debugBin);

                ActiveTarget = ActiveTarget > 10 ? 10 : ActiveTarget;
                ActiveTarget = ActiveTarget < 0 ? 0 : ActiveTarget;
            }
            public override void UpdateStator()
            {
                base.UpdateStator();

                PistonBase.SetValueFloat("Velocity", (float)StatorVelocity);
            }
        }
        class Rotor : Joint
        {
            public IMyMotorStator Stator;

            public Rotor(IMyMotorStator stator, JointData data) : base(stator, data)
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
            public override void LerpAnimationFrame(float lerpTime, ref StringBuilder debugBin)
            {
                base.LerpAnimationFrame(lerpTime, ref debugBin);

                double mag = Math.Abs(LerpPoints[0] - LerpPoints[1]);
                int dir = (mag > 180) ? Math.Sign(LerpPoints[0] - LerpPoints[1]) : Math.Sign(LerpPoints[1] - LerpPoints[0]);
                mag = mag > 180 ? 360 - mag : mag;
                mag *= (lerpTime * dir);

                AnimTarget = LerpPoints[0] + mag;
                AnimTarget = (AnimTarget > 360) ? AnimTarget - 360 : AnimTarget;
                AnimTarget = (AnimTarget < 0) ? AnimTarget + 360 : AnimTarget;
            }
            public override void UpdateCorrectionDisplacement(ref StringBuilder debugBin)
            {
                double current = (Stator.Angle * RAD2DEG);

                double delta = Math.Abs(ActiveTarget - current);
                CorrectionDir = (delta > 180) ? Math.Sign(current - ActiveTarget) : Math.Sign(ActiveTarget - current);
                CorrectionMag = (delta > 180) ? 360 - delta : delta;
            }
            public override void UpdatePlaneDisplacement(ref StringBuilder debugBin)
            {
                base.UpdatePlaneDisplacement(ref debugBin);

                ActiveTarget = ActiveTarget % 360;
                ActiveTarget = ActiveTarget < 0 ? ActiveTarget + 360 : ActiveTarget;
            }
            public override void UpdateStator()
            {
                base.UpdateStator();

                Stator.SetValueFloat("Velocity", (float)StatorVelocity);
            }
        }
        class Hinge : Joint
        {
            public IMyMotorStator Stator;

            public Hinge(IMyMotorStator stator, JointData data) : base(stator, data)
            {
                Stator = stator;
            }
            public override Vector3 ReturnRotationAxis()
            {
                return Stator.WorldMatrix.Up;// Down;
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
            public override void LerpAnimationFrame(float lerpTime, ref StringBuilder debugBin)
            {
                base.LerpAnimationFrame(lerpTime, ref debugBin);

                AnimTarget = LerpPoints[0] + ((LerpPoints[1] - LerpPoints[0]) * lerpTime);
            }
            public override void UpdateCorrectionDisplacement(ref StringBuilder debugBin)
            {
                CorrectionMag = ActiveTarget - (Stator.Angle * RAD2DEG);
                CorrectionDir = Math.Sign(CorrectionMag);
                CorrectionMag = Math.Abs(CorrectionMag);
            }
            public override void UpdatePlaneDisplacement(ref StringBuilder debugBin)
            {
                base.UpdatePlaneDisplacement(ref debugBin);

                ActiveTarget = ActiveTarget % 360;
                ActiveTarget = ActiveTarget > 180 ? ActiveTarget - 360 : ActiveTarget;
                ActiveTarget = ActiveTarget > 90 ? 90 : ActiveTarget;
            }
            public override void UpdateStator()
            {
                base.UpdateStator();

                Stator.SetValueFloat("Velocity", (float)StatorVelocity);
            }
        }
        class JointFrame
        {
            public Joint Joint;
            public double LerpPoint;

            public JointFrame(Joint joint, bool snapping = false) // Snapshot
            {
                Joint = joint;
                double point = Joint.ReturnCurrentStatorPosition();
                if (snapping)
                    point = (int)point;
                LerpPoint = point;
            }
            public JointFrame(Joint joint, float lerpPoint) // User-Written
            {
                Joint = joint;
                LerpPoint = lerpPoint;
            }
            public void ChangeStatorLerpPoint(double value)
            {
                LerpPoint = Joint.ClampTargetValue(value);
            }
        }
        class JointSet
        {
            public SetData Data;

            public List<Foot> Feet = new List<Foot>();
            public List<Joint> Joints = new List<Joint>();
            public List<Sequence> Sequences = new List<Sequence>();

            public MatrixD TargetPlane;
            public MatrixD BufferPlane;
            public Vector3D CorrectBuffer;

            public bool Triggered = true;
            public int Stepping;
            public int Releasing;

            class JointSort : Comparer<Joint>
            {
                public override int Compare(Joint x, Joint y)
                {
                    if (x != null && y != null)
                        return x.Data.IDindex.CompareTo(y.Data.IDindex);
                    else
                        return 0;
                }
            }
            public JointSet(SetData data)
            {
                Data = data;
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
                float triggerTime = forward ? lerpTime : 1 - lerpTime;

                // reset for the RS latch
                if (Triggered)
                {
                    if (triggerTime >= TriggerCap)
                        Triggered = false;
                    else
                        return false;
                }

                bool footCheck = false;

                // determine currently locked and checking foot
                if (Feet[Stepping].CheckTouching(ref debugBin))
                    Feet[Stepping].ToggleLock();

                if (Feet[Stepping].CheckLocked(ref debugBin))
                    footCheck = true;

                if (footCheck) // Initial contact
                {
                    Triggered = true;

                    if (Stepping != -1)
                    {
                        Stepping = Stepping + 1 >= Feet.Count ? 0 : Stepping + 1;
                    }

                    if (Releasing != -1)
                    {
                        Feet[Releasing].ToggleLock(false);
                        Releasing = Releasing + 1 >= Feet.Count ? 0 : Releasing + 1;
                    }

                    return true; // Lock successful
                }

                return false; // Lock failed
            }

            public bool InitFootStatus(ref StringBuilder debugBin)
            {
                if (Feet[0].CheckTouching(ref debugBin) ||
                    Feet[0].CheckLocked(ref debugBin))
                {
                    InitializeGrip(ref debugBin);
                    return true;
                }
                if (Feet[1].CheckTouching(ref debugBin) ||
                    Feet[1].CheckLocked(ref debugBin))
                {
                    InitializeGrip(ref debugBin, false);
                    return true;
                }
                debugBin.Append("Neither Touching!\n");
                return false;
            }
            public bool InitializeGrip(ref StringBuilder debugBin, bool left = true)
            {
                try
                {
                    int unlock = left ? 1 : 0;
                    int locking = left ? 0 : 1;

                    Feet[locking].ToggleLock();
                    Releasing = locking;
                    Feet[unlock].ToggleLock(false);
                    Stepping = unlock;

                    string side = left ? "Left" : "Right";
                    debugBin.Append($"{side} foot locked!\n");

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
                if (Data.Plane == null)
                    return;

                TargetPlane = Data.Plane.WorldMatrix;
            }
            public void TogglePlaneing(bool toggle)
            {
                foreach (Foot foot in Feet)
                {
                    foot.Planeing = toggle;
                    foot.UpdateFootPlaneing();
                }
            }

            public void SortJoints()
            {
                // Option A... onus on user
                //Array.Sort(Data.Joints.ToArray(), new JointSort());
                Joints.Sort(new JointSort());

                // Option B...
                /*
                foreach(Joint joint in Joints)
                bool MatchStator(string cubeGridName, IMyMotorStator stator)
                bool MatchPiston(string cubeGridName, IMyPistonBase piston)
                */
            }
            public void UpdatePlane(ref StringBuilder debugBinStream, ref Vector3 playerInput)
            {
                if (Data.Plane == null)
                    return;

                playerInput *= CorrectionScalar;

                BufferPlane = MatrixD.CreateFromYawPitchRoll(playerInput.X, playerInput.Y, playerInput.Z);

                TargetPlane = MatrixD.Multiply(BufferPlane, TargetPlane);

                BufferPlane = Data.Plane.WorldMatrix;

                BufferPlane = MatrixD.Multiply(MatrixD.Invert(TargetPlane), BufferPlane);

                MatrixD.GetEulerAnglesXYZ(ref BufferPlane, out CorrectBuffer);

                foreach (Foot foot in Feet)
                {
                    if (foot != null)
                    {
                        foot.GenerateAxisMagnitudes(ref debugBinStream, Data.Plane.WorldMatrix);
                        for (int i = 0; i < foot.Planars.Count; i++)
                            if (foot.Planars[i] != null)
                            {
                                foot.Planars[i].PlaneCorrection = GeneratePlaneCorrection(ref debugBinStream, foot.Planars[i], foot.PlanarRatio, CorrectBuffer);
                            }
                    }      
                }       
            }
            double GeneratePlaneCorrection(ref StringBuilder debugBin, Joint joint, Vector3 planarRatios, Vector3 angleCorrections)
            {
                if (joint == null)
                    return 0;

                double output = 0;
                for (int i = 0; i < 3; i++)
                {
                    double planarsum = joint.PlanarDots.GetDim(i) * planarRatios.GetDim(i) * (angleCorrections.GetDim(i) * RAD2DEG);

                    // x = + / y = + / z = -
                    output = i == 2? output - planarsum : output + planarsum;
                }
                return output;
            }
        }
        class Foot
        {
            public FootData Data;

            public List<Joint> Toes = new List<Joint>();
            public List<Joint> Planars = new List<Joint>();
            public List<IMyLandingGear> Pads = new List<IMyLandingGear>();

            public bool Locked = false;
            //public bool Stepping = false;
            //public bool Releasing = false;
            public bool Planeing;
            public Vector3 PlanarRatio;

            public Foot(FootData data)
            {
                Data = data;
            }
            public void GearInit()
            {
                foreach (IMyLandingGear gear in Pads)
                {
                    if (gear == null)
                        continue;

                    gear.AutoLock = false;
                    gear.Enabled = true;
                }
            }
            public void ToggleLock(bool locking = true)
            {
                ToggleGrip(locking);

                Locked = locking;
                UpdateFootPlaneing();

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
                        Locked = true;
                        return true;
                    }
                }

                Locked = false;
                return false;
            }
            void ToggleGrip(bool gripping = true)
            {
                foreach (Joint toe in Toes)
                    toe.Gripping = gripping;
            }
            public void UpdateFootPlaneing()
            {
                foreach (Joint plane in Planars)
                    if (plane != null)
                        plane.Planeing = Locked && Planeing;
            }
            public void GenerateAxisMagnitudes(ref StringBuilder debugBin, MatrixD plane)
            {
                PlanarRatio = Vector3.Zero;

                for (int i = 0; i < Planars.Count; i++)
                {
                    if (Planars[i] == null)
                        continue;

                    Planars[i].UpdatePlanarDot(plane);
                    for (int j = 0; j < 3; j++)
                    {
                        PlanarRatio.SetDim(j, PlanarRatio.GetDim(j) + Math.Abs(Planars[i].PlanarDots.GetDim(j)));
                    }
                }

                for (int i = 0; i < 3; i++)
                    PlanarRatio.SetDim(i, 1 / PlanarRatio.GetDim(i));
            }
        }
        class KeyFrame
        {
            public string Name;
            public JointFrame[] Jframes;

            public KeyFrame(string name, JointFrame[] jFrames)
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
            public ClockMode RisidualClockMode;
            public ClockMode CurrentClockMode;
            public bool bFrameLoadForward;
            public float CurrentClockSpeed;
            public float CurrentClockTime;

            // Frames
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

                CurrentClockMode = ClockMode.PAUSE;
                RisidualClockMode = ClockMode.FOR; // default for now
                CurrentFrameIndex = 0;
                CurrentClockTime = 0;
                CurrentClockSpeed = clockSpeed;
            }

            public void ZeroSequence(ref StringBuilder debugBin)
            {
                LoadFrame(0, true, false, ref debugBin);
                RisidualClockMode = CurrentClockMode;
                CurrentClockMode = ClockMode.PAUSE;
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

            public bool InitializeSeq(ref StringBuilder debugBin)
            {
                if (Frames.Count == 0)
                    return false;

                CurrentFrame = Frames[0];

                foreach (JointFrame jFrame in CurrentFrame.Jframes)
                {
                    jFrame.Joint.LoadAnimationFrame(jFrame);
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
            public bool UpdateSequence(ref StringBuilder debugBin, bool ignoreFeet = true)
            {
                if (CurrentFrame == null ||
                    CurrentClockMode == ClockMode.PAUSE)
                    return false;

                UpdateTriggers(ref debugBin, ignoreFeet);
                LerpFrame(CurrentClockTime, ref debugBin);
                return true;
            }
            public bool AddKeyFrameSnapshot(ref StringBuilder debugBin, int index = -1, string name = null, bool snapping = false)
            {
                if (JointSet == null ||
                    JointSet.Joints.Count == 0)
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

            void UpdateTriggers(ref StringBuilder debugBin, bool ignoreFeet)
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
                        CurrentClockTime += CurrentClockSpeed;
                        if (CurrentClockTime >= 1 ||
                            (!ignoreFeet && JointSet.CheckStep(CurrentClockTime, forward, ref debugBin)))
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
                            (!ignoreFeet && JointSet.CheckStep(CurrentClockTime, forward, ref debugBin)))
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
                if (JointSet == null)
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
        JointSet ConstructJointSet(ref StringBuilder debugBin, SetData setData, List<Foot> feet)
        {
            JointSet newSet = new JointSet(setData);
            newSet.Feet.AddRange(feet);

            if (setData.GroupName == null ||
                setData.GroupName == "null")
                UserInputString(ref setData.GroupName);

            if (setData.GroupName == null)
                return null;

            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(setData.GroupName);
            if (group == null)
                return null;

            if (setData.Name == null)
                setData.Name = setData.GroupName;

            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);

            foreach (IMyTerminalBlock block in blocks)
            {
                if (block is IMyLandingGear)
                {
                    BuildToePad(newSet, (IMyLandingGear)block);
                }

                if (block is IMyPistonBase ||
                    block is IMyMotorStator)
                {
                    BuildJoint(ref debugBin, newSet, (IMyMechanicalConnectionBlock)block);
                }
            }

            newSet.SortJoints();
            return newSet;
        }
        bool CheckData(out JointData data, IMyTerminalBlock block, int setIndex)
        {
            data = new JointData();

            string[] raw = block.CustomData.Split(':');
            if (raw.Length < SaveBlockCountSize)
                return false;

            data.TAG = raw[0][0];
            data.Name = raw[1];

            if (!int.TryParse(raw[2], out data.ParentIndex))
                return false;

            if (data.ParentIndex != setIndex)
                return false;

            if (!int.TryParse(raw[3], out data.IDindex))
                data.IDindex = -1; // un-needed atm

            if (!int.TryParse(raw[4], out data.FootIndex))
                data.FootIndex = -1;

            return true;
        }
        void BuildToePad(JointSet set, IMyLandingGear gear)
        {
            JointData data;

            if (!CheckData(out data, gear, set.Data.Index)
                || data.FootIndex < 0
                || data.FootIndex >= set.Feet.Count)
                return;

            Foot foot = set.Feet[data.FootIndex];

            if (foot != null)
                foot.Pads.Add(gear);
        }
        void BuildJoint(ref StringBuilder debugBin, JointSet set, IMyMechanicalConnectionBlock jointBlock)
        {
            JointData jointData;
            debugBin.Append($"Checking: {jointBlock.CustomName}\n");

            if (!CheckData(out jointData, jointBlock, set.Data.Index))
            {
                debugBin.Append("CheckFailed!\n");
                return;
            }

            Joint newJoint = JointConstructor(jointBlock, jointData);
            Foot foot = jointData.FootIndex < 0 || jointData.FootIndex >= set.Feet.Count ? null : set.Feet[jointData.FootIndex];

            switch (jointData.TAG)
            {
                case 'P':
                    set.Joints.Add(newJoint);
                    if (foot == null)
                        return;
                    foot.Planars.Add(newJoint);
                    break;

                case 'J':
                    set.Joints.Add(newJoint);
                    break;

                case 'G':
                    if (foot == null)
                        return;
                    foot.Toes.Add(newJoint);
                    break;

                default:
                    debugBin.Append("Invalid joint code!\n");
                    return;
            }
            debugBin.Append("Joint added!\n");
        }
        Joint JointConstructor(IMyMechanicalConnectionBlock jointBlock, JointData data)
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
                $"R:{matrix.Right  .X.ToString(digits)}|{matrix.Right  .Y.ToString(digits)}|{matrix.Right  .Z.ToString(digits)}\n" +
                $"U:{matrix.Up     .X.ToString(digits)}|{matrix.Up     .Y.ToString(digits)}|{matrix.Up     .Z.ToString(digits)}\n" +
                $"F:{matrix.Forward.X.ToString(digits)}|{matrix.Forward.Y.ToString(digits)}|{matrix.Forward.Z.ToString(digits)}\n";
        }
        
        void AppendLibraryItem(GUILayer layer, int index, List<string> rawStrings, string itemName)
        {
            int cursor;
            if (CurrentGUILayer == layer &&
                SelObjIndex[(int)layer] == index)
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

            rawStrings.Add(DisplayManagerBuilder.ToString());
        }
        string[] LibraryStringBuilder()
        {
            List<string> stringList = new List<string>();
            stringList.Add("======Library======");
            stringList.Add($"===(Snapping:{SnapValue})===");
            int layer = (int)CurrentGUILayer;

            try
            {
                if (JsetBin.Count == 0)
                {
                    stringList.Add("No limbs loaded!\n");
                }

                for (int jSetIndex = 0; jSetIndex < JsetBin.Count; jSetIndex++)
                {
                    AppendLibraryItem(GUILayer.JSET, jSetIndex, stringList, JsetBin[jSetIndex].Data.Name);

                    if (layer < 1 || SelObjIndex[0] != jSetIndex)
                        continue;

                    if (JsetBin[jSetIndex].Sequences.Count == 0)
                        stringList.Add(" No sequences found!");

                    for (int seqIndex = 0; seqIndex < JsetBin[jSetIndex].Sequences.Count; seqIndex++)
                    {
                        AppendLibraryItem(GUILayer.SEQUENCE, seqIndex, stringList, JsetBin[jSetIndex].Sequences[seqIndex].Name);

                        if (layer < 2 || SelObjIndex[1] != seqIndex)
                            continue;

                        if (JsetBin[jSetIndex].Sequences[seqIndex].Frames.Count == 0)
                            stringList.Add("  No frames found!");

                        for (int kFrameIndex = 0; kFrameIndex < JsetBin[jSetIndex].Sequences[seqIndex].Frames.Count; kFrameIndex++)
                        {
                            AppendLibraryItem(GUILayer.FRAME, kFrameIndex, stringList, JsetBin[jSetIndex].Sequences[seqIndex].Frames[kFrameIndex].Name);

                            if (layer < 3 || SelObjIndex[2] != kFrameIndex)
                                continue;

                            if (JsetBin[jSetIndex].Joints.Count == 0)
                                stringList.Add("   No joints found!");

                            for (int jFrameIndex = 0; jFrameIndex < JsetBin[jSetIndex].Sequences[seqIndex].Frames[kFrameIndex].Jframes.Count(); jFrameIndex++)
                            {
                                AppendLibraryItem(GUILayer.JOINT, jFrameIndex, stringList, JsetBin[jSetIndex].Sequences[seqIndex].Frames[kFrameIndex].Jframes[jFrameIndex].Joint.Data.Name + ':' + JsetBin[jSetIndex].Sequences[seqIndex].Frames[kFrameIndex].Jframes[jFrameIndex].LerpPoint);
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
                stringList.Add($"Ignoring Save: {IgnoreSave}");
                stringList.Add($"Ignoring Feet: {IgnoreFeet}");
                stringList.Add($"Auto demo: {AutoDemo}");
                stringList.Add($"Planeing: {Planeing}");
                stringList.Add($"Stator control: {StatorControl}");
                stringList.Add($"Stator target: {StatorTarget}");
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
                stringList.Add($"Walk-state: {CurrentWalk.CurrentClockMode}");
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
        string ButtonStringBuilder(GUIMode mode)
        {
            DisplayManagerBuilder.Clear();

            for (int i = 0; i < AllButtons[(int)mode].Length; i++)
            {
                string header = mode == GUIMode.CREATE && i > 7 ? InputLabels[i - 8] : (i + 1).ToString();
                DisplayManagerBuilder.Append($"{header} - {AllButtons[(int)mode][i]}\n");
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

            if (!CapLines && CurrentGUIMode == GUIMode.CREATE)
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
                    IgnoreFeet = !IgnoreFeet;
                    break;

                case 2:
                    IgnoreSave = !IgnoreSave;
                    ForceSave = true;
                    Save();
                    ForceSave = false;
                    break;

                case 3:
                    Planeing = !Planeing;
                    CurrentWalkSet.TogglePlaneing(Planeing);
                    break;

                case 4:
                    StatorControl = !StatorControl;
                    break;

                case 5:
                    StatorTarget = !StatorTarget;
                    break;

                case 6:
                    AutoDemo = !AutoDemo;
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
            if (CurrentWalkSet == null ||
                CurrentWalk == null)
                return;

            switch (button)
            {
                case 1:
                    CurrentWalkSet.InitializeGrip(ref DebugBinStatic);
                    break;

                case 2:
                    CurrentWalkSet.InitializeGrip(ref DebugBinStatic, false);
                    break;

                case 3:
                    CurrentWalkSet.UnlockFeet();
                    break;

                case 4:
                    CurrentWalkSet.SnapShotPlane();
                    break;

                case 5:
                    CurrentWalk.ToggleClockPause();
                    break;

                case 6:
                    CurrentWalk.ToggleClockDirection();
                    break;

                case 7:
                    CurrentWalk.InitializeSeq(ref DebugBinStatic);
                    break;

                case 8:
                    CurrentWalkSet.ZeroJointSet();
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
        void LibraryMenuFunctions(int button)
        {
            switch (button)
            {
                case 1:
                    ChangeSnappingValue();
                    break;

                case 2:
                    IncrementLerpPoint(false);
                    break;

                case 3:
                    IncrementLerpPoint(true);
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

                case GUIMode.CREATE:
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
            DebugScreens[(int)Screen.DEBUG_STATIC].WriteText(DebugBinStatic.ToString());
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
            JointSet limb = null;
            if (JsetBin.Count > SelObjIndex[0])
                limb = JsetBin[SelObjIndex[0]];

            Sequence seq = null;
            if (limb != null && limb.Sequences.Count > SelObjIndex[1])
                seq = limb.Sequences[SelObjIndex[1]];

            int[] counts = new int[4];

            counts[0] = limb == null ? 0 : JsetBin.Count;
            counts[1] = limb == null ? 0 : limb.Sequences.Count;
            counts[2] = seq == null ? 0 : seq.Frames.Count;
            counts[3] = limb == null ? 0 : limb.Joints.Count;

            int layer = (int)CurrentGUILayer;

            SelObjIndex[layer] += incr;
            SelObjIndex[layer] = counts[layer] == 0 || SelObjIndex[layer] >= counts[layer] ? 0 : SelObjIndex[layer];
            SelObjIndex[layer] = SelObjIndex[layer] < 0 ? counts[layer] - 1 : SelObjIndex[layer];
        }
        void GUIUpdate()
        {
            ButtonBuilder.Clear();
            ButtonBuilder.Append(ButtonStringBuilder(CurrentGUIMode));

            string[] guiData = null;
            switch (CurrentGUIMode)
            {
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
            if (AutoDemo)
                JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].DemoKeyFrame(SelObjIndex[2], ref DebugBinStatic);
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
                        JsetBin[SelObjIndex[0]].Data.Name = name;
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
                            JsetBin[SelObjIndex[0]].Joints[SelObjIndex[3]].Data.Name = name;
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
                    /*SetData set = new SetData(); // <<<< NEEDS CHANGING!!! // build new set's index based on existing count for now, will need re-indexing when a set is removed
                    JointSet newSet = ConstructJointSet(ref DebugBinStatic, set); 
                    if (newSet != null)
                        JsetBin.Add(newSet);*/
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

                    DebugBinStatic.Append($"Frame Generation Success: {JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].AddKeyFrameSnapshot(ref DebugBinStatic, index, name, Snapping)}\n");
                    break;
            }

            RefreshSelection();
            DebugScreens[(int)Screen.DEBUG_STATIC].WriteText(DebugBinStatic.ToString());
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
            }

            if (c == 0 || JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]] == null)
            {
                SelObjIndex[3] = 0;
                return;
            }
        }
        void ChangeSnappingValue()
        {
            UserInputFloat(ref SnapValue);
        }
        void IncrementLerpPoint(bool incr = true)
        {
            if (CurrentGUILayer != GUILayer.JOINT)
                return;

            JointFrame jFrame = JsetBin[SelObjIndex[0]].Sequences[SelObjIndex[1]].Frames[SelObjIndex[2]].Jframes[SelObjIndex[3]];

            double newLerpPoint = incr ? jFrame.LerpPoint + SnapValue : jFrame.LerpPoint - SnapValue;
            jFrame.ChangeStatorLerpPoint(newLerpPoint);
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

            switch (CurrentGUIMode)
            {
                case GUIMode.CREATE:

                    int z = (int)Control.MoveIndicator.Z;
                    int x = (int)Control.MoveIndicator.X;

                    if (LastLibraryInput[0] != z)
                    {
                        LastLibraryInput[0] = z;
                        if (z != 0)
                        {
                            GUINav nav = z < 0 ? GUINav.UP : GUINav.DOWN;
                            GUINavigation(nav);
                        }
                    }
                    if (LastLibraryInput[1] != x)
                    {
                        LastLibraryInput[1] = x;
                        if (x != 0)
                        {
                            GUINav nav = x < 0 ? GUINav.BACK : GUINav.SELECT;
                            GUINavigation(nav);
                        }
                    }
                    break;

                case GUIMode.CONTROL:

                    RotationBuffer.X = LookScalar * -Control.RotationIndicator.Y;
                    RotationBuffer.Y = LookScalar * Control.RotationIndicator.X;
                    RotationBuffer.Z = RollScalar * -Control.RollIndicator;

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

            CurrentWalkSet.UpdatePlane(ref DebugBinStream, ref RotationBuffer);

            if (CurrentWalk == null)
                return;

            CurrentWalk.UpdateSequence(ref DebugBinStream, IgnoreFeet);
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

            if (!StatorControl)
                return;

            foreach (JointSet set in JsetBin)
            {
                set.UpdateFootLockStatus(ref DebugBinStream);

                foreach (Foot foot in set.Feet)
                    foreach (Joint toe in foot.Toes)
                    {
                        toe.UpdateJoint(StatorTarget, Runtime.TimeSinceLastRun.TotalMilliseconds, ref DebugBinStream);
                    }

                foreach (Joint joint in set.Joints)
                {
                    joint.UpdateJoint(StatorTarget, Runtime.TimeSinceLastRun.TotalMilliseconds, ref DebugBinStream);
                }
            }
        }
        void DisplayManager()
        {
            try
            {
                Echo($"GUIstatus: {GUIstatus(DebugScreens[(int)Screen.SPLASH], DebugScreens[(int)Screen.CONTROLS])}");
                Echo($"Diagnostics: {Diagnostics(DebugScreens[(int)Screen.DIAGNOSTICS])}");
                Echo($"MechStatus: {MechStatus(CockPitScreens[(int)Screen.MECH_STATUS])}");
                Echo($"DebugStream: {DebugStream(DebugScreens[(int)Screen.DEBUG_STREAM])}");
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

            gui.WriteText(SplashBuilder);
            if (buttons != null)
                buttons.WriteText(ButtonBuilder);

            return true;
        }
        bool Diagnostics(IMyTextSurface panel)
        {
            if (panel == null)
                return false;

            DisplayManagerBuilder.Clear();
            try
            {
                Vector3 B = CurrentWalkSet.CorrectBuffer;
                List<Joint> P = CurrentWalkSet.Feet[1].Planars;

                DisplayManagerBuilder.Append(
                    $"RawInput:\n{Control.RotationIndicator.Y}:{Control.RotationIndicator.X}:{Control.RollIndicator}\n" +
                    $"Corrections:\n{B.X:0.###}:{B.Y:0.###}:{B.Z:0.###}\n" +
                    $"Finals:\n{P[0].ActiveTarget:0.###}\n{P[1].ActiveTarget:0.###}\n{P[2].ActiveTarget:0.###}\n" +
                    $"Stepping: {CurrentWalkSet.Stepping}\n" +
                    $"Releasing: {CurrentWalkSet.Releasing}\n");

                for(int i = 0; i < CurrentWalkSet.Feet.Count; i++)
                {
                    DisplayManagerBuilder.Append($"Foot:{i} | Locked: {CurrentWalkSet.Feet[i].Locked}\n");
                }
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
                DisplayManagerBuilder.Append(
                     "Mech Status:\n\n" +
                    $"SampleWalkClockState: {CurrentWalk.CurrentClockMode}\n" +
                    $"CurrentWalkClockTime: {CurrentWalk.CurrentClockTime}\n" +
                    $"CurrentWalkFrameIndex: {CurrentWalk.CurrentFrameIndex}\n" +
                    $"TargetActive: {StatorTarget}\n" +
                    $"StatorControlActive: {StatorControl}\n" +
                    $"Snapping: {Snapping}\n" +
                    $"SnappingValue: {SnapValue}\n\n" +
                    $"Planeing: {Planeing}\n" +
                    $"IgnoreSave: {IgnoreSave}");
            }

            catch
            { DisplayManagerBuilder.Append("FAIL POINT!"); }

            panel.WriteText(DisplayManagerBuilder.ToString());

            return true;
        }
        bool DebugStream(IMyTextSurface panel)
        {
            if (panel == null)
                return false;

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
                SplashBuilder = new StringBuilder();
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
                Initialized = true;
            }
            catch
            {
                Initialized = false;
                return;
            }

            if (!Load(ref DebugBinStatic))
            {
                DebugBinStatic.Append("Load Failed!\n");
            }
            else
            {
                CurrentWalkSet.InitFootStatus(ref DebugBinStatic);
                CurrentWalkSet.ZeroJointSet();
                DebugBinStatic.Append("Load Success!\n");
            }

            GUIUpdate();

            DebugScreens[(int)Screen.DEBUG_STATIC].WriteText(DebugBinStatic);
        }
        public void Main(string argument, UpdateType updateSource)
        {
            if (!Initialized)
                return;

            switch (argument)
            {
                case "SAVE":
                    ForceSave = true;
                    Save();
                    ForceSave = false;
                    break;

                case "LOAD":
                    Load(ref DebugBinStatic);
                    DebugScreens[(int)Screen.DEBUG_STATIC].WriteText(DebugBinStatic.ToString());
                    break;

                case "CLEAR":
                    DebugBinStatic.Clear();
                    DebugScreens[(int)Screen.DEBUG_STATIC].WriteText("");
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
            StatorManager();
            DisplayManager();

            DebugBinStream.Clear(); // MUST HAPPEN!
        }
        /* Tolkens:

        :  - Divider
        &  - Options  (&:IgnoreSave:AutoDemo:Planeing:StatorControl:StatorTarget)
        @  - Foot     (@:sInd:uInd:PadNames:GripNames:PlanarNames)
        #  - JointSet (#:Index:GroupName:Name:IgnoreFeet)
        $  - Sequence ($:Name:LerpSpeed)
        %0 - KeyFrame (%0:Name)
        %1 - JointFrame (%1:LerpPoint)

        toe-pads = T:name:sInd:uInd:fInd
           joint = J:name:sInd:uInd:n/a 
            grip = G:name:sInd:uInd:fInd
        */
        public bool Load(ref StringBuilder debugBin)
        {
            JsetBin.Clear();

            string[] load = Me.CustomData.Split('\n');

            debugBin.Append($"Load Lines Length: {load.Length}\n");

            List<JointFrame> jFrameBuffer = new List<JointFrame>();
            List<KeyFrame> kFrameBuffer = new List<KeyFrame>();
            List<Foot> footBuffer = new List<Foot>();
            JointSet current = null;

            int debugCounter = 0;

            foreach (string next in load)
            {
                try
                {
                    debugBin.Append("next load line...\n");
                    string[] entry = next.Split(':');
                    debugBin.Append($"op-code: {entry[0]}\n");

                    switch (entry[0])
                    {
                        case "&":
                            debugBin.Append("options:\n");
                            IgnoreSave = bool.Parse(entry[1]);
                            IgnoreFeet = bool.Parse(entry[2]);
                            AutoDemo = bool.Parse(entry[3]);
                            Planeing = bool.Parse(entry[4]);
                            StatorControl = bool.Parse(entry[5]);
                            StatorTarget = bool.Parse(entry[6]);
                            debugBin.Append("options loaded!\n");
                            break;

                        case "@":
                            debugBin.Append("constructing foot...\n");
                            FootData data = new FootData(next);
                            if (data.Parsed)
                            {
                                footBuffer.Add(new Foot(data));
                                debugBin.Append("foot constructed!\n");
                            }
                            else
                                debugBin.Append("foot construction failed!\n");
                            break;

                        case "#":
                            debugBin.Append("constructing set...\n");
                            SetData set = new SetData(next, Control);
                            current = ConstructJointSet(ref debugBin, set, footBuffer);
                            if (current == null)
                            {
                                debugBin.Append("set construction failed!\n");
                                return false;
                            }
                            JsetBin.Add(current);
                            footBuffer.Clear();
                            break;

                        case "%1":
                            debugBin.Append("jFrame:\n");
                            jFrameBuffer.Add(new JointFrame(current.Joints[jFrameBuffer.Count], float.Parse(entry[1])));
                            debugBin.Append("added!:\n");
                            break;

                        case "%0":
                            debugBin.Append("kFrame:\n");
                            kFrameBuffer.Add(new KeyFrame(entry[1], jFrameBuffer.ToArray()));
                            jFrameBuffer.Clear();
                            debugBin.Append("added!:\n");
                            break;

                        case "$":
                            debugBin.Append("sequence:\n");
                            List<KeyFrame> newFrames = new List<KeyFrame>();
                            newFrames.AddRange(kFrameBuffer);
                            new Sequence(entry[1], current, newFrames, float.Parse(entry[2]));
                            kFrameBuffer.Clear();
                            break;
                    }
                }
                catch
                {
                    debugBin.Append("Fail Point!\n");
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
                CurrentWalk.InitializeSeq(ref debugBin);
                return true;
            }

            return false;
        }
        public void Save()
        {
            if (IgnoreSave && !ForceSave)
                return;

            SaveData.Clear();

            SaveData.Append($"&:{IgnoreSave}:{IgnoreFeet}:{AutoDemo}:{Planeing}:{StatorControl}:{StatorTarget}\n");

            foreach (JointSet set in JsetBin)
            {
                for (int i = 0; i < set.Feet.Count; i++)
                {
                    if (set.Feet[i] == null)
                        continue;

                    SaveData.Append(set.Feet[i].Data.Save());

                    foreach (Joint toeGrip in set.Feet[i].Toes)
                        if (toeGrip != null)
                            toeGrip.SaveData();

                    for (int j = 0; j < set.Feet[i].Pads.Count; j++)
                        set.Feet[i].Pads[j].CustomData = $"T:{set.Feet[i].Pads[j].Name}:{set.Data.Index}:{j}:{i}:0";
                }

                foreach (Joint joint in set.Joints)
                    if (joint != null)
                        joint.SaveData();

                SaveData.Append(set.Data.Save());

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
