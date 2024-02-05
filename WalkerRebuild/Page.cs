//using Sandbox.ModAPI;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {

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
                keyPress = AlternateMode && (int)keyPress < 20 ? (GUIKey)((int)keyPress + 10) : keyPress;

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


    }
}
