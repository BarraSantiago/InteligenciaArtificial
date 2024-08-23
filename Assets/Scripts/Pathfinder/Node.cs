public class Node<Coordinate> : INode, INode<Coordinate>
{
    private Coordinate coordinate;
    private int cost;
    
    public void SetCoordinate(Coordinate coordinate)
    {
        this.coordinate = coordinate;
    }

    public Coordinate GetCoordinate()
    {
        return coordinate;
    }

    public bool IsBlocked()
    {
        return false;
    }

    public int GetCost()
    {
        return cost;
    }
}