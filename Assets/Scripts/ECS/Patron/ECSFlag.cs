namespace ECS.Patron
{
    public class ECSFlag
    {
        private uint entityOwnerID = 0;

        public uint EntityOwnerID
        {
            get => entityOwnerID;
            set => entityOwnerID = value;
        }

        protected ECSFlag()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}