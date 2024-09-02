using Pathfinder;
using UnityEngine;
using VoronoiDiagram;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Map Config")] 
        [SerializeField] private int mapWidth;
        [SerializeField] private int mapHeight;
        [SerializeField] private int minesQuantity;
        [SerializeField] private float nodesSize;

        [Header("Units Config")] 
        [SerializeField] private int minersQuantity;
        [SerializeField] private int cartsQuantity;
        
        [Header("Setup")] 
        [SerializeField] private GraphView graphView;
        [SerializeField] private Voronoi voronoi;
        [SerializeField] private bool validate;
        private Vector2IntGraph<Node<System.Numerics.Vector2>> graph;

        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;
            
            MapGenerator.CellSize = nodesSize;
            MapGenerator.MapDimensions = new Vector2Int(mapWidth, mapHeight);
            
            graph = new Vector2IntGraph<Node<System.Numerics.Vector2>>(mapWidth, mapHeight);
            
            Node<System.Numerics.Vector2> node = graph.nodes[Random.Range(0, graph.nodes.Count)];
            node.NodeType = NodeType.Mine;
            MapGenerator.Vector2s.Add(node);

            node = graph.nodes[Random.Range(0, graph.nodes.Count)];
            node.NodeType = NodeType.Mine;
            MapGenerator.Vector2s.Add(node);

            node = graph.nodes[Random.Range(0, graph.nodes.Count)];
            node.NodeType = NodeType.Mine;
            MapGenerator.Vector2s.Add(node);
            graph.nodes[Random.Range(0, graph.nodes.Count)].NodeType = NodeType.TownCenter;

            voronoi.Init();
            //voronoi.SetVoronoi(MapGenerator.Vector2s);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            //voronoi.Draw();

            foreach (Node<System.Numerics.Vector2> node in graph.nodes)
            {
                Gizmos.color = node.NodeType switch
                {
                    NodeType.Mine => Color.yellow,
                    NodeType.Empty => Color.white,
                    NodeType.TownCenter => Color.blue,
                    NodeType.Blocked => Color.red,
                    _ => Color.white
                };

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().X, node.GetCoordinate().Y), nodesSize);
            }
        }
    }
}