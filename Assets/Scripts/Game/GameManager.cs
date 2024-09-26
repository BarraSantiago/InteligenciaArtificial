using System;
using System.Collections.Generic;
using Pathfinder;
using Pathfinder.Graph;
using Pathfinder.Voronoi;
using StateMachine.Agents.RTS;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [Header("Map Config")] 
        [SerializeField, Range(4,150)] private int mapWidth;
        [SerializeField, Range(4,150)] private int mapHeight;
        [SerializeField] private int minesQuantity;
        [SerializeField] private float nodesSize;
        [SerializeField] private Vector2 originPosition;
        [SerializeField] private Button retreatButton;

        [Header("Units Config")] 
        [SerializeField] private GameObject minerPrefab;
        [SerializeField] private GameObject caravanPrefab;
        [SerializeField] private int minersQuantity;
        [SerializeField] private int caravansQuantity;


        public static Graph<Node<Vector2>, NodeVoronoi, Vector2> Graph;
        public static List<Node<Vector2>> MinesWithMiners = new List<Node<Vector2>>();
        public static AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi> Pathfinder; 
        
        private Voronoi<NodeVoronoi, Vector2> voronoi;
        private Color color;

        private void Start()
        {
            Miner.OnEmptyMine += RemakeVoronoi;
            Miner.OnReachMine += OnReachMine;
            Miner.OnLeaveMine += OnLeaveMine;

            retreatButton.onClick.AddListener(Retreat);

            color.a = 0.2f;

            Graph<Node<Vector2>, NodeVoronoi, Vector2>.OriginPosition = new NodeVoronoi(originPosition);

            Graph = new Vector2Graph(mapWidth, mapHeight, nodesSize);
            voronoi = new Voronoi<NodeVoronoi, Vector2>();

            AmountSafeChecks();

            SetupObstacles();
            
            int towncenterNode = CreateMines(out var townCenterPosition);

            Pathfinder = new AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi>(Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType);
            
            VoronoiSetup();

            for (int i = 0; i < minersQuantity; i++)
            {
                CreateMiner(townCenterPosition, towncenterNode);
            }

            for (int i = 0; i < caravansQuantity; i++)
            {
                CreateCaravan(townCenterPosition, towncenterNode);
            }
        }

        private void OnReachMine(Node<Vector2> node)
        {
            RemoveEmptyNodes();
            MinesWithMiners.Add(node);
        }

        private void OnLeaveMine(Node<Vector2> node)
        {
            MinesWithMiners.Remove(node);
            RemoveEmptyNodes();
        }
        public void RemoveEmptyNodes()
        {
            MinesWithMiners.RemoveAll(node => node.NodeType == NodeType.Empty);
        }
        private void SetupObstacles()
        {
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[i].NodeType = NodeType.Blocked;
                }
            }
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[i].NodeType = NodeType.Forest;
                }
            }
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[i].NodeType = NodeType.Gravel;
                }
            }
        }

        private void VoronoiSetup()
        {
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();

            foreach (var t in Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines)
            {
                voronoiNodes.Add(Graph.CoordNodes.Find((node =>
                    node.GetCoordinate() == t.GetCoordinate())));
            }

            voronoi.Init();
            voronoi.SetVoronoi(voronoiNodes);
        }

        private int CreateMines(out Vector3 townCenterPosition)
        {
            for (int i = 0; i < minesQuantity; i++)
            {
                int rand = Random.Range(0, Graph.CoordNodes.Count);
                if (Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[rand].NodeType == NodeType.Mine) continue;
                Node<Vector2> node = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[rand];
                node.NodeType = NodeType.Mine;
                node.gold = 100;
                Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines.Add(node);
            }

            int towncenterNode = Random.Range(0, Graph.CoordNodes.Count);

            Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode].NodeType = NodeType.TownCenter;

            townCenterPosition = new Vector3(Graph.CoordNodes[towncenterNode].GetCoordinate().x,
                Graph.CoordNodes[towncenterNode].GetCoordinate().y);
            return towncenterNode;
        }

        private void AmountSafeChecks()
        {
            const int Min = 0;
            const int Max = 3;
            if (minesQuantity < Min) minesQuantity = Min;
            if (minesQuantity > (mapWidth+mapHeight)/Max) minesQuantity = (mapWidth+mapHeight)/Max;
            if (minersQuantity < Min) minersQuantity = Min;
            if (caravansQuantity < Min) caravansQuantity = Min;
        }

        private void CreateCaravan(Vector3 townCenterPosition, int towncenterNode)
        {
            GameObject caravan = Instantiate(caravanPrefab, townCenterPosition, Quaternion.identity);
            Caravan agent2 = caravan.GetComponent<Caravan>();
            agent2.CurrentNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            agent2.Voronoi = voronoi;
            agent2.Init();
        }

        private void CreateMiner(Vector3 townCenterPosition, int towncenterNode)
        {
            GameObject miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            Miner agent = miner.GetComponent<Miner>();
            agent.CurrentNode = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            RTSAgent.TownCenter = Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType[towncenterNode];
            agent.Voronoi = voronoi;
            agent.Init();
        }

        private void Retreat()
        {
            RTSAgent.Retreat = !RTSAgent.Retreat;
        }

        private void RemakeVoronoi()
        {
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();
            Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType.ForEach(node =>
            {
                if (node.NodeType == NodeType.Mine && node.gold <= 0) node.NodeType = NodeType.Empty;
            });
            foreach (var mine in Graph<Node<Vector2>, NodeVoronoi, Vector2>.mines)
            {
                if (mine.gold > 0)
                    voronoiNodes.Add(Graph.CoordNodes.Find(node => node.GetCoordinate() == mine.GetCoordinate()));
            }

            voronoi.SetVoronoi(voronoiNodes);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            
            foreach (var sector in voronoi.SectorsToDraw())
            {
                Handles.color = color;
                List<Vector3> list = new List<Vector3>();
                foreach (var nodeVoronoi in sector.PointsToDraw())
                    list.Add(new Vector3(nodeVoronoi.GetX(), nodeVoronoi.GetY()));
                Handles.DrawAAConvexPolygon(list.ToArray());

                Handles.color = Color.black;
                Handles.DrawPolyLine(list.ToArray());
            }


            foreach (var node in Graph<Node<Vector2>, NodeVoronoi, Vector2>.NodesType)
            {
                Gizmos.color = node.NodeType switch
                {
                    NodeType.Mine => Color.yellow,
                    NodeType.Empty => Color.white,
                    NodeType.TownCenter => Color.blue,
                    NodeType.Forest => Color.green,
                    NodeType.Gravel => Color.gray,
                    NodeType.Blocked => Color.red,
                    _ => Color.white
                };

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), nodesSize/5);
            }
        }
    }
}