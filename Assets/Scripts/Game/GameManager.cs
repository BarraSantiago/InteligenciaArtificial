using System;
using Pathfinder;
using StateMachine.Agents.RTS;
using UnityEngine;
using Utils;
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
        [SerializeField] private Vector2 originPosition;

        [Header("Units Config")]
        [SerializeField] private GameObject minerPrefab;
        [SerializeField] private GameObject caravanPrefab;
        [SerializeField] private int minersQuantity;
        [SerializeField] private int cartsQuantity;

        [Header("Setup")]
        [SerializeField] private GraphView graphView;
        [SerializeField] private Voronoi<NodeVoronoi> voronoi;
        [SerializeField] private bool validate;
        
        private Vector2IntGraph<Node<NodeVoronoi>> graph;
        
        private void Start()
        {
            if (!Application.isPlaying)
                return;

            MapGenerator<NodeVoronoi>.CellSize = nodesSize;
            MapGenerator<NodeVoronoi>.MapDimensions = new NodeVoronoi(mapWidth, mapHeight);
            MapGenerator<NodeVoronoi>.OriginPosition = new NodeVoronoi(originPosition);

            graph = new Vector2IntGraph<NodeVoronoi>(mapWidth, mapHeight, nodesSize);

            for (int i = 0; i < minesQuantity; i++)
            {
                Node<NodeVoronoi> node = graph.nodes[Random.Range(0, graph.nodes.Count)];
                node.NodeType = NodeType.Mine;
                node.gold = 100;
                MapGenerator<NodeVoronoi>.mines.Add(node);
            }
            
            int towncenterNode = Random.Range(0, graph.nodes.Count);
            graph.nodes[towncenterNode].NodeType = NodeType.TownCenter;

            MapGenerator<NodeVoronoi>.nodes = graph.nodes;

            Vector3 townCenterPosition = new Vector3(graph.nodes[towncenterNode].GetCoordinate().x,
                graph.nodes[towncenterNode].GetCoordinate().y);

            voronoi.Init();
            voronoi.SetVoronoi(MapGenerator<NodeVoronoi>.mines);

            GameObject miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            Miner agent = miner.GetComponent<Miner>();
            agent.currentNode = graph.nodes[towncenterNode];
            RTSAgent.townCenter = graph.nodes[towncenterNode];
            agent.voronoi = voronoi;
            agent.Init();
            /*
            GameObject caravan = Instantiate(caravanPrefab, townCenterPosition, Quaternion.identity);
            Caravan agent2 = caravan.GetComponent<Caravan>();
            agent2.currentNode = graph.nodes[towncenterNode];
            agent2.Init();*/
            //voronoi.Init();
            //voronoi.SetVoronoi(MapGenerator.Vector2s);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            voronoi.Draw();

            foreach (Node<Vec2Int> node in graph.nodes)
            {
                Gizmos.color = node.NodeType switch
                {
                    NodeType.Mine => Color.yellow,
                    NodeType.Empty => Color.white,
                    NodeType.TownCenter => Color.blue,
                    NodeType.Blocked => Color.red,
                    _ => Color.white
                };

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), nodesSize);
            }
        }
    }
}