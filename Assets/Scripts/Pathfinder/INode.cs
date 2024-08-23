public interface INode
{
    public bool IsBlocked();
    public int GetCost();
    
}

public interface INode<Coordinate> 
{
    public void SetCoordinate(Coordinate coordinateType);
    
    public Coordinate GetCoordinate();
}
