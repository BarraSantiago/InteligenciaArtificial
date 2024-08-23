using UnityEngine;

public class GrapfView : MonoBehaviour
{
    public Vector2IntGraph<Node<Vector2Int>> Graph;

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
            if (node.IsBlocked())
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            
            Gizmos.DrawWireSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), 0.1f);
        }
    }
}
