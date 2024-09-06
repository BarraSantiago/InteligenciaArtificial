namespace FlockingECS.Component
{
    public class BoidComponent
    {
        public float speed = 2.5f;
        public float turnSpeed = 5f;
        public float detectionRadius = 3.0f;
        
        BoidComponent(float speed, float turnSpeed, float detectionRadius)
        {
            this.speed = speed;
            this.turnSpeed = turnSpeed;
            this.detectionRadius = detectionRadius;
        }
    }
}