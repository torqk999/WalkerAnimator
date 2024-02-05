//using Sandbox.ModAPI;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {

        class ControlPage : Page
        {
            public ControlPage() : base("Controls")
            {
                Buttons = new Dictionary<GUIKey, Button>
                {
                    {GUIKey.ALPHA_1, new Button("Main",                 () => SetGuiMode(GUIMode.MAIN))     },
                    {GUIKey.ALPHA_2, new Button("Unlock Feet",          () => CurrentWalkSet?.UnlockAllFeet()) },
                    {GUIKey.ALPHA_3, new Button("Toggle Orientation",   () => Orientation.Adjust()) },
                    {GUIKey.ALPHA_4, new Button("Toggle Pause",         () => CurrentWalk?.ToggleClockPause()) },
                    {GUIKey.ALPHA_5, new Button("Toggle Direction",     () => CurrentWalk?.ToggleClockDirection()) },
                    {GUIKey.ALPHA_6, new Button("Zero Out Mech/Walk",   () => ZeroOutMech()) },
                };
            }

            protected override string[] PageBuilder()
            {
                List<string> stringList = new List<string>();
                stringList.Add("===Controls===");
                stringList.Add("==============");

                try
                {
                    stringList.Add($"{(CurrentWalkSet == null ? "No legs!" : CurrentWalkSet.Locked ? "Walking..." : "Flying...")}");
                }
                catch
                {
                    stringList.Add("Error!");
                }

                string[] output = stringList.ToArray();
                return output;
            }

            void ZeroOutMech()
            {
                CurrentWalkSet?.ZeroJointSet(CurrentWalkSet.ZeroFrame);
                CurrentWalk?.ZeroSequence();
            }
        }
    }
}
