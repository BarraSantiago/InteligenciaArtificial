namespace Pathfinder
{
    public class Node<Coordinate> : INode, INode<Coordinate>
    {
        private Coordinate coordinate;
    
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
        
        public bool EqualsTo(INode<Coordinate> other)
        {
            return coordinate.Equals(other.GetCoordinate());
        }

        public bool Equals(Coordinate other)
        {
            return coordinate.Equals(other);
        }
    }
}