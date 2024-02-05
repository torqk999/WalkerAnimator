//using Sandbox.ModAPI;
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
    partial class Program
    {

        class TablePage : Page
        {
            public TablePage() : base("Assignment Table")
            {
                Buttons = new Dictionary<GUIKey, Button>
                {
                    {GUIKey.ALPHA_1,    new Button("Main",              ()=> SetGuiMode(GUIMode.MAIN)) },
                    {GUIKey.ALT_1,      new Button("Main",              ()=> SetGuiMode(GUIMode.MAIN)) },

                    {GUIKey.ALPHA_2,    new Button("Assign Magnets",    ()=> SetGuiMode(GUIMode.ASSIGN_MAGNETS)) },
                    {GUIKey.ALT_2,      new Button("Assign Joints",     ()=> SetGuiMode(GUIMode.ASSIGN_JOINTS)) },

                    {GUIKey.ALPHA_3,    new Button("Aquire All Joints", ()=> FindAndAssignAllJoints()) },
                    {GUIKey.ALT_3,      new Button("Aquire All Magnets",()=> FindAndAssignAllMagnets())},

                    {GUIKey.ALPHA_4,    new Button("Add foot",          ()=> TableJsetAdjust(0,1))     },
                    {GUIKey.ALPHA_5,    new Button("Remove foot",       ()=> TableJsetAdjust(0,-1))    },

                    {GUIKey.ALT_LEFT,   new Button("Increase Value",    ()=> TableValueAdjust(1))      },
                    {GUIKey.ALT_RIGHT,  new Button("Decrease Value",    ()=> TableValueAdjust(-1))     },

                    {GUIKey.FORWARD,    new Button("Shift Up",          ()=> TableShift(0, -1))        },
                    {GUIKey.BACKWARD,   new Button("Shift Down",        ()=> TableShift(0, 1))         },
                    {GUIKey.LEFT,       new Button("Shift Left",        ()=> TableShift(-1, 0))        },
                    {GUIKey.RIGHT,      new Button("Shift Right",       ()=> TableShift(1, 0))         },

                    {GUIKey.UP,         new Button("Previous JointSet", ()=> TableJsetAdjust(-1, 0))   },
                    {GUIKey.DOWN,       new Button("Next JointSet",     ()=> TableJsetAdjust(1, 0))    },
                };
                SelectedIndexes = new Dictionary<eRoot, int>
                {
                    {eRoot.JSET, 0},
                    {eRoot.JOINT, 0},
                    {eRoot.MAGNET, 0},
                    {eRoot.PARAM, 0},
                };
                HeaderSize = 3;
            }

            protected override string[] PageBuilder()
            {
                cursorCounter = 0;
                DisplayManagerBuilder.Clear();
                RawBuffer.Clear();

                RawBuffer.Add($"= {Name} =");
                RawBuffer.Add($"[Selected JSET:{GetJointSet(SelectedIndexes[eRoot.JSET])?.Name()}][FootCount:{GetJointSet(SelectedIndexes[eRoot.JSET])?.Feet.Count}]=");

                if (AlternateMode)
                    MagnetPageBuilder();
                else
                    JointPageBuilder();

                return RawBuffer.ToArray();
            }

            void JointPageBuilder()
            {
                for (int i = (int)PARAM.TAG; i < JointParamCount; i++)
                    DisplayManagerBuilder.Append($"[{(PARAM)i}]");

                DisplayManagerBuilder.Append("[Name]");
                RawBuffer.Add(DisplayManagerBuilder.ToString());

                for (int i = 0; i < JointBin.Count; i++)
                    AppendFunctionalToTable((Functional)JointBin[i], i);
            }

            void MagnetPageBuilder()
            {
                for (int i = (int)PARAM.TAG; i < MagnetParamCount; i++)
                    DisplayManagerBuilder.Append($"[{(PARAM)i}]");

                DisplayManagerBuilder.Append("[Name]");
                RawBuffer.Add(DisplayManagerBuilder.ToString());

                for (int i = 0; i < MagnetBin.Count; i++)
                    AppendFunctionalToTable((Functional)MagnetBin[i], i);
            }

            const int PadSize = 2;
            string Pad(int i)
            {
                string output = i.ToString();
                int count = i < 0 ? 1 : 0;
                do
                {
                    count++;
                    i /= 10;
                } while (i > 0);

                for (int j = 0; j < PadSize - count; j++)
                    output += " ";
                return output;
            }
            void AppendFunctionalToTable(Functional joint, int index)
            {
                DisplayManagerBuilder.Clear();

                bool selected = SelectedIndexes[AlternateMode ? eRoot.MAGNET : eRoot.JOINT] == index;
                CursorIndex = selected ? cursorCounter : CursorIndex;
                cursorCounter++;

                DisplayManagerBuilder.Append(BuildCursor(selected));
                DisplayManagerBuilder.Append($"{Pad(index)}");
                List<int> indexes = joint.Indexes();
                int selectedParam = SelectedIndex(eRoot.PARAM);
                int selectedElement = SelectedIndex(AlternateMode ? eRoot.MAGNET : eRoot.JOINT);
                string leftSelect, rightSelect;

                leftSelect = selectedParam == 1 && selectedElement == index ? Cursor[2] : Cursor[0];
                rightSelect = selectedParam == 1 && selectedElement == index ? Cursor[3] : Cursor[0];
                DisplayManagerBuilder.Append($"{leftSelect}[{joint.TAG}]{rightSelect}");

                for (int i = 0; i < indexes.Count; i++)
                {
                    leftSelect = selectedParam == (int)PARAM.uIX + i && selectedElement == index ? Cursor[2] : Cursor[0];
                    rightSelect = selectedParam == (int)PARAM.uIX + i && selectedElement == index ? Cursor[3] : Cursor[0];
                    DisplayManagerBuilder.Append($"{leftSelect}[{Pad(indexes[i])}]{rightSelect}");
                }

                
                DisplayManagerBuilder.Append($" == {joint.Name()}");

                RawBuffer.Add(DisplayManagerBuilder.ToString());

            }

            void FindAndAssignAllJoints()
            {
                GetGridBlocksOfType(ReTagBuffer);

                foreach (IMyMechanicalConnectionBlock joint in ReTagBuffer)
                {
                    Joint freshJoint = LoadJoint(joint);
                    if (!freshJoint.BUILT)
                        freshJoint = NewJoint(JointData.Default, joint);

                    int oldIndex = JointBin.FindIndex(x => ((Joint)x).Connection.EntityId == joint.EntityId);

                    if (oldIndex < 0)
                        JointBin.Add(freshJoint);
                }
            }
            void FindAndAssignAllMagnets()
            {
                GetGridBlocksOfType(ReTagBuffer);

                foreach (IMyLandingGear gear in ReTagBuffer)
                {
                    Magnet freshMagnet = LoadMagnet(gear);
                    if (!freshMagnet.BUILT)
                        freshMagnet = NewMagnet(RootData.Default, gear, -1);

                    int oldIndex = MagnetBin.FindIndex(x => ((Magnet)x).Gear.EntityId == gear.EntityId);

                    if (oldIndex < 0)
                        MagnetBin.Add(freshMagnet);
                }
            }

            void TableJsetAdjust(int deltaIX, int deltaFeet)
            {
                SelectedIndexes[eRoot.JSET] += deltaIX;
                SelectedIndexes[eRoot.JSET] = SelectedIndexes[eRoot.JSET] < 0 ? JsetBin.Count - 1 : SelectedIndexes[eRoot.JSET] >= JsetBin.Count ? 0 : SelectedIndexes[eRoot.JSET];

            }
            void TableShift(int deltaX, int deltaY)
            {
                eRoot currentMode = AlternateMode ? eRoot.MAGNET : eRoot.JOINT;
                int currentCount = AlternateMode ? MagnetBin.Count : JointBin.Count;
                int paramCount = AlternateMode ? MagnetParamCount : JointParamCount;

                SelectedIndexes[currentMode] += deltaY;
                SelectedIndexes[currentMode] = SelectedIndexes[currentMode] < 0 ? currentCount - 1 : SelectedIndexes[currentMode] >= currentCount ? 0 : SelectedIndexes[currentMode];

                SelectedIndexes[eRoot.PARAM] += deltaX;
                SelectedIndexes[eRoot.PARAM] = SelectedIndexes[eRoot.PARAM] < 1 ? paramCount - 1 : SelectedIndexes[eRoot.PARAM] >= paramCount ? 1 : SelectedIndexes[eRoot.PARAM];
            }

            void TableValueAdjust(int deltaValue)
            {
                if (AlternateMode)
                    AdjustMagnetParam(MagnetBin[SelectedIndex(eRoot.MAGNET)] as Magnet, (PARAM)SelectedIndex(eRoot.PARAM), deltaValue);
                else
                    AdjustJointParam(JointBin[SelectedIndex(eRoot.JOINT)] as Joint, (PARAM)SelectedIndex(eRoot.PARAM), deltaValue);
            }
            public override void SetMode(GUIMode mode)
            {
                switch(mode)
                {
                    case GUIMode.ASSIGN_JOINTS:
                        Name = "Assign Joints";
                        AlternateMode = false;
                        break;
            
                    case GUIMode.ASSIGN_MAGNETS:
                        Name = "Assign Magnets";
                        AlternateMode = true;
                        break;
            
                    default:
                        Name = "Assignment Table";
                        break;
                }

                TableShift(0, 0);
            }
            void AdjustJointParam(Joint joint, PARAM targetParam, int deltaValue)
            {
                switch (targetParam)
                {
                    case PARAM.uIX:
                        GetJointSet(SelectedIndex(eRoot.JSET))?.Swap(joint.MyIndex, joint.MyIndex += deltaValue, eRoot.JOINT);
                        break;

                    case PARAM.pIX:
                        joint.ParentIndex += deltaValue;
                        break;

                    case PARAM.fIX:
                        joint.FootIndex += deltaValue;
                        break;

                    case PARAM.GripDirection:
                        joint.GripDirection += deltaValue;
                        joint.GripDirection = joint.GripDirection < -1 ? -1 : joint.GripDirection > 1 ? 1 : joint.GripDirection;
                        break;

                    case PARAM.sIX:
                        joint.SyncIndex += deltaValue;
                        break;

                    case PARAM.TAG:
                        int tagIndex = JointTags.FindIndex(x => x == joint.TAG);
                        if (tagIndex < 0)
                        {
                            joint.TAG = UnusedTag;
                            break;
                        }
                        tagIndex += deltaValue;
                        tagIndex = tagIndex >= JointTags.Count ? 0 : tagIndex < 0 ? JointTags.Count - 1 : tagIndex;
                        joint.TAG = JointTags[tagIndex];
                        break;

                    default:
                        AdjustFunctionalParam(joint, targetParam, deltaValue);
                        break;
                }
            }

            void AdjustMagnetParam(Magnet magnet, PARAM targetParam, int deltaValue)
            {
                switch (targetParam)
                {
                    case PARAM.uIX:
                        magnet.MyIndex += deltaValue;
                        break;

                    case PARAM.pIX:
                        magnet.ParentIndex += deltaValue;
                        break;

                    case PARAM.fIX:
                        magnet.FootIndex += deltaValue;
                        break;

                    case PARAM.TAG:
                        int tagIndex = MagnetTags.FindIndex(x => x == magnet.TAG);
                        if (tagIndex < 0)
                        {
                            magnet.TAG = UnusedTag;
                            break;
                        }
                        tagIndex += deltaValue;
                        tagIndex = tagIndex >= MagnetTags.Count ? 0 : tagIndex < 0 ? MagnetTags.Count - 1 : tagIndex;
                        magnet.TAG = MagnetTags[tagIndex];
                        break;

                    default:
                        AdjustFunctionalParam(magnet, targetParam, deltaValue);
                        break;
                }
            }

            void AdjustFunctionalParam(Functional functional, PARAM targetParam, int deltaValue)
            {
                switch(targetParam)
                {
                    case PARAM.uIX:
                        functional.MyIndex += deltaValue;
                        break;

                    case PARAM.pIX:
                        functional.ParentIndex += deltaValue;
                        break;


                }
            }
        }


    }
}
