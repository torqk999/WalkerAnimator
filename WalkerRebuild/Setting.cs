//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {

        class Setting : Option
        {

            float
                Value, ValueMax, ValueMin,
                defaultIncr;
            Setting Incrementer;

            public Setting(string name, string describe, float init, float increment, float max = 1, float min = 0, Setting incr = null) : base(name, describe)
            {
                Value = init;
                defaultIncr = increment;
                ValueMax = max;
                ValueMin = min;
                Incrementer = incr;
                Clamp();
            }

            public override string Current()
            {
                return Value.ToString();
            }
            public override void Adjust(bool incr = true)
            {
                float delta = Incrementer == null ? defaultIncr : Incrementer.Value;
                Value += incr ? delta : -delta;
                Clamp();
            }

            public float MyValue()
            {
                return Value;
            }
            public void Change(float overwrite)
            {
                Value = overwrite;
                Clamp();
            }

            void Clamp()
            {
                Value = Value < ValueMin ? ValueMin : Value > ValueMax ? ValueMax : Value;
            }
        }


    }
}
