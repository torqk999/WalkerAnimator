//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {

        struct RootData
        {
            public static RootData Default = new RootData("Un-named", -1, -1, true);

            public string Name;
            public int MyIndex;
            public int ParentIndex;
            public bool Overwrite;

            public RootData(string name, int myIndex, int parentIndex, bool overwrite = false)
            {
                Name = name;
                MyIndex = myIndex;
                ParentIndex = parentIndex;
                Overwrite = overwrite;
            }
        }


    }
}
