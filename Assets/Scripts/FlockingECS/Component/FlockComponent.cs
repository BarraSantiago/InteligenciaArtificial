using ECS.Patron;

namespace FlockingECS.Component
{
    public class FlockComponent<TVector> : ECSComponent
    {
        public FlockComponent(TVector alignment, TVector cohesion, TVector separation, TVector direction)
        {
            Alignment = alignment;
            Cohesion = cohesion;
            Separation = separation;
            Direction = direction;
        }

        public TVector Alignment { get; set; }
        public TVector Cohesion { get; set; }
        public TVector Separation { get; set; }
        public TVector Direction { get; set; }

        public float DetectionRadius { get; set; }
    }
}