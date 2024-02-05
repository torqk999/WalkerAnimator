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

        public Program()
        {
            int count = 0;
            List<IMyTerminalBlock> allBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(allBlocks);
            StringBuilder builder = new StringBuilder();

            foreach(IMyTerminalBlock block in allBlocks)
            {
                string[] data = block.CustomData.Split(':');
                if (data == null || data.Length < 2)
                    continue;

                string buffer = data[0];
                data[0] = data[1];
                data[1] = buffer;

                builder.Clear();

                builder.Append($"{data[0]}");
                for (int i = 1; i < data.Length; i++)
                    builder.Append($":{data[i]}");

                block.CustomData = builder.ToString();
                count++;
            }
            Echo($"{count} blocks modified");
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {

        }
    }
}
