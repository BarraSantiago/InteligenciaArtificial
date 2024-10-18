namespace ECS.Patron
{
    public abstract class ECSComponent
    {
        public uint EntityOwnerID { get; set; } = 0;

        public virtual void Dispose()
        {
        }
    }
}