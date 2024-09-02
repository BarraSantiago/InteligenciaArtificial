using Pathfinder;
using StateMachine.Agents.RTS;
using UnityEngine;
using VoronoiDiagram;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Map Config")] [SerializeField]
        private int mapWidth;

        [SerializeField] private int mapHeight;
        [SerializeField] private int minesQuantity;
        [SerializeField] private float nodesSize;

        [Header("Units Config")] [SerializeField]
        private GameObject minerPrefab;

        [SerializeField] private int minersQuantity;
        [SerializeField] private int cartsQuantity;

        [Header("Setup")] [SerializeField] private GraphView graphView;
        [SerializeField] private Voronoi voronoi;
        [SerializeField] private bool validate;
        private Vector2IntGraph<Node<System.Numerics.Vector2>> graph;

        private void Start()
        {
            if (!Application.isPlaying)
                return;

            MapGenerator.CellSize = nodesSize;
            MapGenerator.MapDimensions = new Vector2Int(mapWidth, mapHeight);

            graph = new Vector2IntGraph<Node<System.Numerics.Vector2>>(mapWidth, mapHeight);


            Node<System.Numerics.Vector2> node = graph.nodes[Random.Range(0, graph.nodes.Count)];
            node.NodeType = NodeType.Mine;
            node.gold = 100;
            MapGenerator.mines.Add(node);

            node = graph.nodes[Random.Range(0, graph.nodes.Count)];
            node.NodeType = NodeType.Mine;
            node.gold = 100;
            MapGenerator.mines.Add(node);

            node = graph.nodes[Random.Range(0, graph.nodes.Count)];
            node.NodeType = NodeType.Mine;
            node.gold = 100;
            MapGenerator.mines.Add(node);
            int towncenterNode = Random.Range(0, graph.nodes.Count);
            graph.nodes[towncenterNode].NodeType = NodeType.TownCenter;

            MapGenerator.nodes = graph.nodes;

            Vector3 townCenterPosition = new Vector3(graph.nodes[towncenterNode].GetCoordinate().X,
                graph.nodes[towncenterNode].GetCoordinate().Y);


            GameObject miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            RTSAgent agent = miner.GetComponent<RTSAgent>();
            agent.currentNode = graph.nodes[towncenterNode];

            //voronoi.Init();
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