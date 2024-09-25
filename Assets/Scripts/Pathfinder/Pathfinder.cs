using System;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinder
{
    public struct Transition<NodeType>
    {
        public NodeType to;
        public int cost;
    }

    public abstract class Pathfinder<TNodeType, TCoordinateType, TCoordinate>
        where TNodeType : INode<TCoordinateType>
        where TCoordinateType : IEquatable<TCoordinateType>
        where TCoordinate : ICoordinate<TCoordinateType>, new()
    {
        protected ICollection<TNodeType> Graph;

        public Dictionary<TNodeType, List<Transition<TNodeType>>> transitions =
            new Dictionary<TNodeType, List<Transition<TNodeType>>>();

        public List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode)
        {
            Dictionary<TNodeType, (TNodeType Parent, int AcumulativeCost, int Heuristic)> nodes =
                new Dictionary<TNodeType, (TNodeType Parent, int AcumulativeCost, int Heuristic)>();

            foreach (TNodeType node in Graph)
            {
                nodes.Add(node, (default, 0, 0));
            }

            TCoordinate startCoor = new TCoordinate();
            startCoor.SetCoordinate(startNode.GetCoordinate());
            List<TNodeType> openList = new List<TNodeType>();
            List<TNodeType> closedList = new List<TNodeType>();

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                TNodeType currentNode = openList[0];
                int currentIndex = 0;

                for (int i = 1; i < openList.Count; i++)
                {
                    if (nodes[openList[i]].AcumulativeCost + nodes[openList[i]].Heuristic >=
                        nodes[currentNode].AcumulativeCost + nodes[currentNode].Heuristic) continue;

                    currentNode = openList[i];
                    currentIndex = i;
                }

                openList.RemoveAt(currentIndex);
                closedList.Add(currentNode);

                if (NodesEquals(currentNode, destinationNode))
                {
                    return GeneratePath(startNode, destinationNode);
                }

                foreach (TNodeType neighbor in GetNeighbors(currentNode))
                {
                    if (!nodes.ContainsKey(neighbor) || IsBlocked(neighbor) || closedList.Contains(neighbor))
                    {
                        continue;
                    }

                    int aproxAcumulativeCost = 0;

                    aproxAcumulativeCost += nodes[currentNode].AcumulativeCost;
                    aproxAcumulativeCost += MoveToNeighborCost(currentNode, neighbor);

                    if (openList.Contains(neighbor) && aproxAcumulativeCost >= nodes[neighbor].AcumulativeCost)
                        continue;

                    TCoordinate neighborCoor = new TCoordinate();
                    neighborCoor.SetCoordinate(neighbor.GetCoordinate());

                    nodes[neighbor] = (currentNode, aproxAcumulativeCost, Distance(neighborCoor, startCoor));

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }

            return null;

            List<TNodeType> GeneratePath(TNodeType startNode, TNodeType goalNode)
            {
                List<TNodeType> path = new List<TNodeType>();
                TNodeType currentNode = goalNode;

                while (!NodesEquals(currentNode, startNode))
                {
                    path.Add(currentNode);

                    foreach (var node in nodes.Keys.ToList().Where(node => NodesEquals(currentNode, node)))
                    {
                        currentNode = nodes[node].Parent;
                        break;
                    }
                }

                path.Reverse();
                return path;
            }
        }

        protected abstract ICollection<INode<TCoordinateType>> GetNeighbors(TNodeType node);

        protected abstract int Distance(TCoordinate tCoordinate, TCoordinate coordinate);

        protected abstract bool NodesEquals(TNodeType A, TNodeType B);

        protected abstract int MoveToNeighborCost(TNodeType A, TNodeType B);

        protected abstract bool IsBlocked(TNodeType node);
    }
}