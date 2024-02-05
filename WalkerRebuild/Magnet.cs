//using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Linq;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {

        class Magnet : Functional
        {
            public int FootIndex;
            public IMyLandingGear Gear;
            public Magnet(RootData root, IMyLandingGear gear, int footIndex) : base(gear, root)
            {
                TAG = MagnetTag;
                FootIndex = footIndex;
                Gear = gear;
            }
            public Magnet(IMyLandingGear gear) : base(gear)
            {
                Gear = gear;
                BUILT = Load(gear == null ? null : gear.CustomData);
            }

            public override List<int> Indexes()
            {
                List<int> indexes = base.Indexes();

                indexes.Add(FootIndex);

                return indexes;
            }

            protected override bool Load(string[] data)
            {
                if (!base.Load(data))
                    return false;

                try { FootIndex = int.Parse(data[(int)PARAM.fIX]); }
                catch { FootIndex = -1; }
                return true;
            }


            protected override void saveData(string[] buffer)
            {
                base.saveData(buffer);
                buffer[(int)PARAM.fIX] = FootIndex.ToString();
            }

            public void InitializeGear()
            {
                Gear.AutoLock = false;
                Gear.Enabled = true;
            }

            public void ToggleLock(bool locking)
            {
                Gear.AutoLock = locking;
                if (locking)
                {
                    Gear.Lock();
                    Gear.AutoLock = true;
                }
                else
                {
                    Gear.Unlock();
                    Gear.AutoLock = false;
                }

            }
            public bool IsAlive()
            {
                try { return Gear.IsWorking; }
                catch { return false; }
            }
            public bool IsTouching()
            {
                return Gear.LockMode == LandingGearMode.ReadyToLock;
            }
            public bool IsLocked()
            {
                return Gear.LockMode == LandingGearMode.Locked;
            }
        }

    }
}
