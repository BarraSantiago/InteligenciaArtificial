using UnityEngine;

namespace Pathfinder
{
    public class GraphView : MonoBehaviour
    {
        public Vector2IntGraph<Node<Vector2Int>> Graph;
        public Node<Vector2Int> startNode;
        public Node<Vector2Int> destinationNode;
        void Start()
        {
            Graph = new Vector2IntGraph<Node<Vector2Int>>(10, 10);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            foreach (Node<Vector2Int> node in Graph.nodes)
            {
                if(node.Equals(startNode))
                {
                    Gizmos.color = Color.blue;
                }
                else if(node.Equals(destinationNode))
                {
                    Gizmos.color = Color.cyan;
                }
                else
                {
                    Gizmos.color = node.IsBlocked() ? Color.red : Color.green;
                }

                Gizmos.DrawWireSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), 0.1f);
            }
        }
    }
}
