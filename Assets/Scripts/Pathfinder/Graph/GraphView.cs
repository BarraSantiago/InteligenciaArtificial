using System.Collections.Generic;
using System.Linq;
using MathDebbuger;
using UnityEngine;
using Utils;

namespace Pathfinder
{
    public class GraphView : MonoBehaviour
    {
        /*
        [SerializeField] private bool showTransitions = true;
        public Vector2IntGraph<Node<Vector2>> Graph;
        public Dictionary<Node<Vector2>, List<Transition<Node<Vector2>>>> Transitions;
        public Node<Vector2> startNode;
        public Node<Vector2> destinationNode;
        public List<Node<Vector2>> path;
        public int min = -1;
        public int max = 3;

        private Transition<Node<Vector2>> _transition;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            foreach (Node<Vector2> node in Graph.nodes)
            {
                Gizmos.color = node switch
                {
                    _ when node.EqualsTo(startNode) => Color.blue,
                    _ when node.EqualsTo(destinationNode) => Color.cyan,
                    _ when path.Contains(node) => Color.yellow,
                    _ => node.IsBlocked() ? Color.red : Color.green
                };

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), 0.15f);

                if (!showTransitions) continue;
                if (Transitions == null) continue;
                if (!Transitions.TryGetValue(node, out var transition1)) continue;

                foreach (Transition<Node<Vector2>> transition in transition1)
                {
                    _transition = transition;

                    float normalized = (float)(transition.cost - min) / (max - min);

                    float red = normalized;
                    float green = 1 - normalized;
                    Gizmos.color = transition.cost == 0 ? Color.blue : new Color(red, green, 0, 0.5f);
                    Gizmos.DrawLine(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y),
                        new Vector3(_transition.to.GetCoordinate().x, _transition.to.GetCoordinate().y));
                }
            }
        }*/
    }
}