//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {
        class Toggle : Option
        {
            bool State;
            ToggleUpdate OptionUpdate;

            public Toggle(string name, string describe, bool init, ToggleUpdate update = null) : base(name, describe)
            {
                State = init;
                OptionUpdate = update;
            }

            public override string Current()
            {
                return State.ToString();
            }
            public override void Adjust(bool flip = true)
            {
                State = !State;
                if (OptionUpdate != null)
                    OptionUpdate(State);
            }

            public bool MyState()
            {
                return State;
            }
            public void Change(bool overwrite)
            {
                State = overwrite;
            }
        }


    }
}
