using System.Collections.Generic;
using System.Linq;

namespace Pathfinder
{
    public struct Transition<NodeType>
    {
        public NodeType to;
        public int cost;
    }

    public abstract class Pathfinder<NodeType> where NodeType : INode
    {
        protected ICollection<NodeType> Graph;


        public Dictionary<NodeType, List<Transition<NodeType>>> transitions =
            new Dictionary<NodeType, List<Transition<NodeType>>>();

        public List<NodeType> FindPath(NodeType startNode, NodeType destinationNode)
        {
            Dictionary<NodeType, (NodeType Parent, int AcumulativeCost, int Heuristic)> nodes =
                new Dictionary<NodeType, (NodeType Parent, int AcumulativeCost, int Heuristic)>();

            foreach (NodeType node in Graph)
            {
                nodes.Add(node, (default, 0, 0));
            }

            List<NodeType> openList = new List<NodeType>();
            List<NodeType> closedList = new List<NodeType>();

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                NodeType currentNode = openList[0];
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

                foreach (NodeType neighbor in GetNeighbors(currentNode))
                {
                    if (!nodes.ContainsKey(neighbor) || IsBlocked(neighbor) || closedList.Contains(neighbor))
                    {
                        continue;
                    }

                    int aproxAcumulativeCost = 0;

                    aproxAcumulativeCost += nodes[currentNode].AcumulativeCost;
                    aproxAcumulativeCost += MoveToNeighborCost(currentNode, neighbor);

                    if (openList.Contains(neighbor) && aproxAcumulativeCost >= nodes[neighbor].AcumulativeCost) continue;

                    nodes[neighbor] = (currentNode, aproxAcumulativeCost, Distance(neighbor, destinationNode));

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }

            return null;

            List<NodeType> GeneratePath(NodeType startNode, NodeType goalNode)
            {
                List<NodeType> path = new List<NodeType>();
                NodeType currentNode = goalNode;

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

        protected abstract ICollection<NodeType> GetNeighbors(NodeType node);

        protected abstract int Distance(NodeType A, NodeType B);

        protected abstract bool NodesEquals(NodeType A, NodeType B);

        protected abstract int MoveToNeighborCost(NodeType A, NodeType B);

        protected abstract bool IsBlocked(NodeType node);
    }
}