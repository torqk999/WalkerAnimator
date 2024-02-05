//using Sandbox.ModAPI;

using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        class Root
        {
            public string TAG;
            public int MyIndex;
            public int ParentIndex;
            public bool BUILT;
            string[] SaveBuffer = new string[JointParamCount];

            public virtual string Name()
            {
                return "[ROOT]";
            }

            public virtual void SetName(string newName)
            {
                Static("No name value to set!\n");
            }

            public Root()
            {
                BUILT = true;
            }

            public Root(RootData data)
            {
                SetName(data.Name);
                MyIndex = data.MyIndex;
                ParentIndex = data.ParentIndex;
                BUILT = true;
            }

            public RootData ParentData(string name = null, int index = -1)
            {
                return new RootData(name, index, MyIndex);
            }
            public void StaticDlog(string input, bool newLine = true)
            {
                Static($"{input}{(newLine ? "\n" : "")}");
            }
            public void StreamDlog(string input, bool newLine = true)
            {
                DebugBinStream.Append($"{input}{(newLine ? "\n" : "")}");
            }

            protected bool Load(string input)
            {
                return Load(input.Split(':'));
            }
            protected virtual bool Load(string[] data)
            {
                try
                {
                    SetName(data[(int)PARAM.Name]);
                    TAG = data[(int)PARAM.TAG];
                    MyIndex = int.Parse(data[(int)PARAM.uIX]);
                    ParentIndex = int.Parse(data[(int)PARAM.pIX]);

                    return true;
                }
                catch { return false; }
            }
            public string[] SaveDataArray()
            {
                saveData(SaveBuffer);
                return SaveBuffer;
            }
            public string SaveData()
            {
                saveData(SaveBuffer);

                RootDataBuilder.Clear();

                for (int i = 0; i < SaveBuffer.Length; i++)
                    RootDataBuilder.Append($"{SaveBuffer[i]}:");

                return RootDataBuilder.ToString();
            }
            protected virtual void saveData(string[] saveBuffer)
            {
                saveBuffer[(int)PARAM.Name] = Name();
                saveBuffer[(int)PARAM.TAG] = TAG;
                saveBuffer[(int)PARAM.uIX] = MyIndex.ToString();
                saveBuffer[(int)PARAM.pIX] = ParentIndex.ToString();
            }
            public virtual void Insert(Root root, int index = -1) { }
            public virtual void Remove(int index, eRoot type = eRoot.DEFAULT) { }
            public virtual void Swap(int target, int delta, eRoot type = eRoot.DEFAULT) { }
            public virtual void ReParent(int target, int parentDes, eRoot type = eRoot.DEFAULT) { }
            public virtual void ReIndex(eRoot type = eRoot.DEFAULT) { }
            public virtual void Sort(eRoot type = eRoot.DEFAULT) { }

            protected void insert(List<Root> roots, Root root, int index)
            {
                if (index < 0 || index >= roots.Count)
                {
                    root.MyIndex = roots.Count;
                    roots.Add(root);
                    return;
                }

                roots.Insert(index, root);

                for (int i = index + 1; i < roots.Count; i++)
                    roots[i].MyIndex = i;
            }
            protected void remove(List<Root> roots, int index)
            {
                if (index < 0 || index >= roots.Count)
                    return;

                roots.RemoveAt(index);
                for (int i = index + 1; i < roots.Count; i++)
                    roots[i].MyIndex = i;
            }
            protected void swap(List<Root> roots, int target, int destination)
            {
                if (roots == null || target < 0 || target >= roots.Count)
                    return;

                while (destination < 0 || destination >= roots.Count)
                    destination += destination < 0 ? roots.Count : destination >= roots.Count ? -roots.Count : 0;
                
                Root buffer = roots[destination];
                int bufferIndex = buffer.MyIndex;

                roots[destination] = roots[target];
                roots[target] = buffer;

                roots[destination].MyIndex = destination;
                roots[target].MyIndex = target;
            }
            protected void reParent(List<Root> targetRoots, List<Root> destRoots, int targetIndex, int desIndex)
            {

            }
            protected void reIndex(List<Root> roots)
            {
                for (int i = 0; i < roots.Count; i++)
                    roots[i].MyIndex = i;
            }

        }

    }
}
