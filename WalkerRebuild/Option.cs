//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {

        class Option
        {
            public string Name;
            public string[] Description;

            public Option(string name, string describe)
            {
                Name = name;
                Description = describe.Split(' ');
            }

            public virtual string Current()
            {
                return "nothing";
            }
            public virtual void Adjust(bool main = true)
            {

            }
        }

    }
}
