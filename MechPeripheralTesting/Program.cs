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
        #region MAIN
        enum MechIx
        {
            EVENT,
            MOVE_Z,
            MOVE_X,
            YAW,
            PITCH,
            ROLL,
        }
        bool TransmissionEnabled;
        const string DroneChannel = "DRONE";
        IMyTextSurface Surface;
        StringBuilder Buffer = new StringBuilder();
        double[] MSG_IX_BUFFER = new double[Enum.GetValues(typeof(MechIx)).Length];
        void ScreenParse()
        {
            for (int i = 0; i < MSG_IX_BUFFER.Length; i++)
                MSG_IX_BUFFER[i] = 0;

            Buffer.Clear();
            Surface.ReadText(Buffer);
            string[] lines = Buffer.ToString().Split('\n');
            
            foreach(string line in lines)
            {
                Echo($"dataLine: {line}\n");
                try
                {
                    string[] blocks = line.Split(':');
                    MechIx dataIndex = (MechIx)Enum.Parse(typeof(MechIx), blocks[0]);
                    MSG_IX_BUFFER[(int)dataIndex] = double.Parse(blocks[1]);
                }
                catch { Echo($"Bad Data"); return; }
            }

            Echo("Good Data");
        }

        void Transmission()
        {
            if (!TransmissionEnabled)
                return;

            IGC.SendBroadcastMessage(DroneChannel, ImmutableArray.Create(MSG_IX_BUFFER), TransmissionDistance.CurrentConstruct);
            Echo("Message Sent!");
        }

        void UserArgs(string argument)
        {
            switch(argument)
            {
                case "ToggleTransmit":
                    TransmissionEnabled = !TransmissionEnabled;
                    break;
            }
        }

        public Program()
        {
            Surface = Me.GetSurface(0);
            Surface.ContentType = ContentType.TEXT_AND_IMAGE;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            UserArgs(argument);
            ScreenParse();
            Transmission();
            Echo($"Transmitting: {TransmissionEnabled}");
        }
        #endregion
    }
}
