//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {

        struct JointData
        {
            public static JointData Default = new JointData(RootData.Default, JointTag, -1, -1, 0);

            public RootData Root;
            public string TAG;
            public int FootIndex;
            public int SyncIndex;
            public int GripDirection;

            public JointData(RootData root, string tag, int footIndex, int syncIndex, int gripDirection)
            {
                Root = root;
                TAG = tag;
                FootIndex = footIndex;
                SyncIndex = syncIndex;
                GripDirection = gripDirection;
            }
        }

    }
}
