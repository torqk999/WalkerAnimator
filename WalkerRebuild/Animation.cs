//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {
        class Animation : Root
        {
            protected string _name;
            public Setting MySetting;

            public Animation(AnimationData data) : base(data.Root)
            {
                GenerateSetting(data.InitValue);
            }

            public Animation(string input) : base()
            {
                //BUILT = Load(input);
            }

            public override string Name()
            {
                return _name;
            }

            public override void SetName(string newName)
            {
                _name = newName;
            }

            public virtual void GenerateSetting(float init)
            {

            }
            protected override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try {
                    GenerateSetting(float.Parse(data[(int)PARAM.SettingInit]));
                }
                catch {
                    GenerateSetting(0);
                }

                return true;
            }
            protected override void saveData(string[] buffer)
            {
                base.saveData(buffer);
                buffer[(int)PARAM.SettingInit] = (MySetting == null ? 0 : MySetting.MyValue()).ToString();
            }
        }

    }
}
