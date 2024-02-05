//using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    partial class Program
    {
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
                    {GUIKey.ALPHA_5, new Button("Assignment",       ()=> SetGuiMode( GUIMode.ASSIGN_JOINTS ))  },
                    {GUIKey.ALPHA_6, new Button("Save CustomData",  ()=> WriteCustomData())             },
                    {GUIKey.ALPHA_7, new Button("Save ActiveData",  ()=> StartSave())                   },
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
                return output;
            }
        }

    }
}
