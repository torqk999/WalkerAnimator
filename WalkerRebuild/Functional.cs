//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        class Functional : Root
        {
            IMyFunctionalBlock FuncBlock;
            public Functional(IMyFunctionalBlock funcBlock) : base() { FuncBlock = funcBlock; }
            public Functional(IMyFunctionalBlock funcBlock, RootData data) : base(data) { FuncBlock = funcBlock; }

            public virtual List<int> Indexes()
            {
                List<int> indexes = new List<int> {
                    MyIndex,
                    ParentIndex,
                };
                return indexes;
            }

            public override string Name()
            {
                return FuncBlock?.CustomName;
            }

            public override void SetName(string newName)
            {
                if (FuncBlock != null)
                    FuncBlock.CustomName = newName;
            }

            public bool Save()
            {
                if (FuncBlock == null)
                    return false;

                FuncBlock.CustomData = SaveData();
                return true;
            }
        }

    }
}
