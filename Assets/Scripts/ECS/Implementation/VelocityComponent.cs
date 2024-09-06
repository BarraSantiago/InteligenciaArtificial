using ECS.Patron;

public class VelocityComponent<TVector> : ECSComponent
{
    public float velocity;

    public TVector direction;

    public VelocityComponent(float velocity, TVector direction)
    {
        this.velocity = velocity;
        this.direction = direction;
    }
}
