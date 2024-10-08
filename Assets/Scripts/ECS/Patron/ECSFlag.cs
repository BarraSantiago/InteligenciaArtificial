using System;

namespace ECS.Patron
{
    [Flags]
    public enum FlagType
    {
        None = 0,
        Miner = 1 << 0,
        Caravan = 1 << 1,
        Target = 1 << 2,
        TypeD = 1 << 3,
        TypeE = 1 << 4
    }
    
    public class ECSFlag
    {
        private uint entityOwnerID = 0;
        private FlagType flagType;

        public uint EntityOwnerID
        {
            get => entityOwnerID;
            set => entityOwnerID = value;
        }
        
        public FlagType Flag
        {
            get => flagType;
            set => flagType = value;
        }

        protected ECSFlag(FlagType flagType)
        {
            this.flagType = flagType;
        }

        public virtual void Dispose()
        {
        }
    }
}