namespace FlockingECS.Component
{
    public class BoidComponent
    {
        public float detectionRadius = 3.0f;
        public float speed = 2.5f;
        public float turnSpeed = 5f;

        private BoidComponent(float speed, float turnSpeed, float detectionRadius)
        {
            this.speed = speed;
            this.turnSpeed = turnSpeed;
            this.detectionRadius = detectionRadius;
        }
    }
}