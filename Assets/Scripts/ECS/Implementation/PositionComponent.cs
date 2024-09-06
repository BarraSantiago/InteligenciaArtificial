using ECS.Patron;

public class PositionComponent<T> : ECSComponent
{
    public T Position;

    public PositionComponent(T position)
    {
        Position = position;
    }
}