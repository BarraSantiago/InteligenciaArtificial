using System.Collections.Generic;
using Pathfinder;
using Pathfinder.Graph;
using StateMachine.Agents.RTS;
using UnityEngine;
using Utils;
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
        [SerializeField] private Vector2 originPosition;

        [Header("Units Config")] 
        [SerializeField] private GameObject minerPrefab;
        [SerializeField] private GameObject caravanPrefab;
        [SerializeField] private int minersQuantity;
        [SerializeField] private int cartsQuantity;

        [Header("Setup")] 
        [SerializeField] private GraphView graphView;
        [SerializeField] private bool validate;

        public static Graph<Node<Vector2>, NodeVoronoi, Vector2> graph;

        private Voronoi<NodeVoronoi, Vector2> voronoi;
        
        private void Start()
        {
            if (!Application.isPlaying)
                return;
            
            graph = new Vector2Graph(mapWidth, mapHeight, nodesSize);
            
            voronoi = new Voronoi<NodeVoronoi, Vector2>();
            MapGenerator<NodeVoronoi, Vector2>.CellSize = nodesSize;
            MapGenerator<NodeVoronoi, Vector2>.MapDimensions = new NodeVoronoi(mapWidth, mapHeight);
            MapGenerator<NodeVoronoi, Vector2>.OriginPosition = new NodeVoronoi(originPosition);


            for (int i = 0; i < minesQuantity; i++)
            {
                Node<Vector2> node = graph.NodesType[Random.Range(0, graph.CoordNodes.Count)];
                node.NodeType = NodeType.Mine;
                node.gold = 100;
                MapGenerator<NodeVoronoi, Vector2>.mines.Add(node);
            }

            int towncenterNode = Random.Range(0, graph.CoordNodes.Count);
            graph.NodesType[towncenterNode].NodeType = NodeType.TownCenter;

            MapGenerator<NodeVoronoi, Vector2>.nodes = graph.NodesType;

            Vector3 townCenterPosition = new Vector3(graph.CoordNodes[towncenterNode].GetCoordinate().x,
                graph.CoordNodes[towncenterNode].GetCoordinate().y);

            voronoi.Init();
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();
            for (int i = 0; i < MapGenerator<NodeVoronoi, Vector2>.mines.Count; i++)
            {
                voronoiNodes.Add(graph.CoordNodes.Find((node => node.GetCoordinate() == MapGenerator<NodeVoronoi, Vector2>.mines[i].GetCoordinate())));
            }
            
            GameObject miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            Miner agent = miner.GetComponent<Miner>();
            agent.currentNode = graph.NodesType[towncenterNode];
            RTSAgent.townCenter = graph.NodesType[towncenterNode];
            agent.voronoi = voronoi;
            agent.Init();
            /*
            GameObject caravan = Instantiate(caravanPrefab, townCenterPosition, Quaternion.identity);
            Caravan agent2 = caravan.GetComponent<Caravan>();
            agent2.currentNode = graph.nodes[towncenterNode];
            agent2.Init();*/
            //voronoi.Init();
            voronoi.SetVoronoi(voronoiNodes);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            voronoi.Draw();

            foreach (var node in graph.NodesType)
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