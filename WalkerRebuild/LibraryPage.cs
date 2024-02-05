//using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    partial class Program
    {
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

                        AppendLibraryItem(eRoot.JSET, jSetIndex, JsetBin[jSetIndex].Name());

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

                DemoSelectedFrame();
            }
            void ChangeDirectory(bool up)
            {
                int layer = (int)CurrentDirectoryLayer;
                layer += up ? -1 : 1;
                layer = layer < (int)eRoot.JSET ? (int)eRoot.J_FRAME : layer > (int)eRoot.J_FRAME ? (int)eRoot.JSET : layer;

                CurrentDirectoryLayer = (eRoot)layer;
                LibraryUpdate();
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


                int index = SelectedIndex(0);
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

                root.SetName(name);
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
                        AnimationData seqData = new AnimationData(seqRoot, ClockSpeedDef);
                        set.Insert(new Sequence(seqData, set), index);
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
                    AppendLibraryItem(eRoot.SEQUENCE, seqIndex, set.Sequences[seqIndex].Name());

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
                    AppendLibraryItem(eRoot.K_FRAME, kFrameIndex, seq.Frames[kFrameIndex].Name());

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
    }
}
