//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {

        class Button
        {
            public string Name;
            ButtonEvent Event;
            public Button(string name, ButtonEvent @event)
            {
                Name = name;
                Event = @event;
            }
            public void Trigger() { Event(); }
        }

    }
}
