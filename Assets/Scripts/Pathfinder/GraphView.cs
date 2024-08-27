using System.Collections.Generic;
using System.Linq;
using MathDebbuger;
using UnityEngine;
using Utils;

namespace Pathfinder
{
    public class GraphView : MonoBehaviour
    {
        [SerializeField] private bool showTransitions = true;
        public Vector2IntGraph<Node<Vec2Int>> Graph;
        public Dictionary<Node<Vec2Int>, List<Transition<Node<Vec2Int>>>> Transitions;
        public Node<Vec2Int> startNode;
        public Node<Vec2Int> destinationNode;
        public List<Node<Vec2Int>> path;
        public int min = -1;
        public int max = 3;

        private Transition<Node<Vec2Int>> _transition;

        public GraphView(int min, int max)
        {
            Graph = new Vector2IntGraph<Node<Vec2Int>>(10, 10);
            Transitions = new Dictionary<Node<Vec2Int>, List<Transition<Node<Vec2Int>>>>();
            this.min = min;
            this.max = max;
        }

        public GraphView(Vector2IntGraph<Node<Vec2Int>> Graph, int min, int max)
        {
            this.Graph = Graph;
            Transitions = new Dictionary<Node<Vec2Int>, List<Transition<Node<Vec2Int>>>>();
            this.min = min;
            this.max = max;
        }

        public GraphView(Vector2IntGraph<Node<Vec2Int>> Graph,
            Dictionary<Node<Vec2Int>, List<Transition<Node<Vec2Int>>>> transitions, Node<Vec2Int> startNode,
            Node<Vec2Int> destinationNode, int min, int max)
        {
            this.Graph = Graph;
            this.Transitions = transitions;
            this.min = min;
            this.max = max;
            this.startNode = startNode;
            this.destinationNode = destinationNode;
        }

        public void UpdateVectors()
        {
            Vector3Debugger.Clear();

            int id = 0;

            foreach (var node in Graph.nodes.Where(node => Transitions != null))
            {
                if (!Transitions.TryGetValue(node, out var transition1)) continue;

                foreach (Transition<Node<Vec2Int>> transition in transition1)
                {
                    _transition = transition;

                    float normalized = (float)(transition.cost - min) / (max - min);

                    float red = normalized;
                    float green = 1 - normalized;

                    Vector3 node1 = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
                    Vector3 node2 = new Vector3(_transition.to.GetCoordinate().x, _transition.to.GetCoordinate().y);

                    Color color = transition.cost == 0 ? Color.blue : new Color(red, green, 0, 0.5f);
                    string vectorId = id.ToString();

                    if (!Vector3Debugger.ContainsVector(vectorId))
                    {
                        Vector3Debugger.AddVector(node1, node2, color, vectorId);
                    }
                    else
                    {
                        Vector3Debugger.UpdateVector(node1, node2, color, vectorId);
                    }

                    id++;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            foreach (Node<Vec2Int> node in Graph.nodes)
            {
                switch (node)
                {
                    case var _ when node.Equals(startNode):
                        Gizmos.color = Color.blue;
                        break;
                    case var _ when node.Equals(destinationNode):
                        Gizmos.color = Color.cyan;
                        break;
                    case var _ when path.Contains(node):
                        Gizmos.color = Color.yellow;
                        break;
                    default:
                        Gizmos.color = node.IsBlocked() ? Color.red : Color.green;
                        break;
                }

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), 0.15f);

                if (!showTransitions) continue;
                if (Transitions == null) continue;
                if (!Transitions.TryGetValue(node, out var transition1)) continue;

                int id = 0;
                foreach (Transition<Node<Vec2Int>> transition in transition1)
                {
                    _transition = transition;

                    float normalized = (float)(transition.cost - min) / (max - min);

                    float red = normalized;
                    float green = 1 - normalized;

                    Vector3 node1 = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
                    Vector3 node2 = new Vector3(_transition.to.GetCoordinate().x, _transition.to.GetCoordinate().y) - node1;

                    Color color = transition.cost == 0 ? Color.blue : new Color(red, green, 0, 0.5f);
                    string vectorId = id.ToString();

                    if (!Vector3Debugger.ContainsVector(vectorId))
                    {
                        Vector3Debugger.AddVector(node1, node2, color, vectorId);
                    }
                    else
                    {
                        Vector3Debugger.UpdateVector(node1, node2, color, vectorId);
                    }

                    id++;
                }
            }
        }
    }
}