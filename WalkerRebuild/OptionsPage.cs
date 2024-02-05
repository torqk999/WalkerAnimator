//using Sandbox.ModAPI;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {

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


    }
}
