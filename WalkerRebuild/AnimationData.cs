//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {
        struct AnimationData
        {
            public RootData Root;
            public float InitValue;

            public static AnimationData Default = new AnimationData(RootData.Default, 0);

            public AnimationData(RootData root, float init)
            {
                Root = root;
                InitValue = init;
            }
        }

    }
}
