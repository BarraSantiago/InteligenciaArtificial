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
    
using GraphType = Graph<Node<Vector2>, NodeVoronoi, Vector2>;
    
    public class GameManager : MonoBehaviour
    {
        [Header("Map Config")] 
        [SerializeField, Range(4,150)] private int mapWidth;
        [SerializeField, Range(4,150)] private int mapHeight;
        [SerializeField] private int minesQuantity;
        [SerializeField] private float nodesSize;
        [SerializeField] private Vector2 originPosition;
        [SerializeField] private Button retreatButton;
        [SerializeField] private Button addMinesButton;

        [Header("Units Config")] 
        [SerializeField] private GameObject minerPrefab;
        [SerializeField] private GameObject caravanPrefab;
        [SerializeField] private int minersQuantity;
        [SerializeField] private int caravansQuantity;


        public static GraphType Graph;
        public static readonly List<Node<Vector2>> MinesWithMiners = new List<Node<Vector2>>();
        public static AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi> Pathfinder; 
        
        private const int MaxMines = 4;
        private Voronoi<NodeVoronoi, Vector2> voronoi;
        private Color color;
        private int towncenterNode;
        private Vector3 townCenterPosition;
        private void Start()
        {
            Miner.OnEmptyMine += RemakeVoronoi;
            Miner.OnReachMine += OnReachMine;
            Miner.OnLeaveMine += OnLeaveMine;

            retreatButton.onClick.AddListener(Retreat);
            addMinesButton.onClick.AddListener(() =>
            {
                CreateMines();
                RemakeVoronoi();
            });
            color.a = 0.2f;

            GraphType.OriginPosition = new NodeVoronoi(originPosition);

            MakeMap();

            for (int i = 0; i < minersQuantity; i++)
            {
                CreateMiner();
            }

            for (int i = 0; i < caravansQuantity; i++)
            {
                CreateCaravan();
            }
        }

        private void MakeMap()
        {
            Graph = new Vector2Graph(mapWidth, mapHeight, nodesSize);
            voronoi = new Voronoi<NodeVoronoi, Vector2>();

            AmountSafeChecks();

            SetupObstacles();
            
            CreateMines();
            
            towncenterNode = CreateTownCenter(out townCenterPosition);

            Pathfinder = new AStarPathfinder<Node<Vector2>, Vector2, NodeVoronoi>(GraphType.NodesType);
            
            VoronoiSetup();
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

        private void RemoveEmptyNodes()
        {
            MinesWithMiners.RemoveAll(node => node.NodeType == NodeType.Empty);
        }
        private void SetupObstacles()
        {
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    GraphType.NodesType[i].NodeType = NodeType.Blocked;
                }
            }
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    GraphType.NodesType[i].NodeType = NodeType.Forest;
                }
            }
            for (int i = 0; i < Graph.CoordNodes.Count; i++)
            {
                if (Random.Range(0, 100) < 10)
                {
                    GraphType.NodesType[i].NodeType = NodeType.Gravel;
                }
            }
        }

        private void VoronoiSetup()
        {
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();

            foreach (var t in GraphType.mines)
            {
                voronoiNodes.Add(Graph.CoordNodes.Find((node =>
                    node.GetCoordinate() == t.GetCoordinate())));
            }

            voronoi.Init();
            voronoi.SetVoronoi(voronoiNodes);
        }

        private void CreateMines()
        {

            AmountSafeChecks();
            if(GraphType.mines.Count + minesQuantity > (mapWidth+mapHeight)/MaxMines) return;
            
            for (int i = 0; i < minesQuantity; i++)
            {
                int rand = Random.Range(0, Graph.CoordNodes.Count);
                if (GraphType.NodesType[rand].NodeType == NodeType.Mine || 
                    GraphType.NodesType[rand].NodeType == NodeType.TownCenter) continue;
                Node<Vector2> node = GraphType.NodesType[rand];
                node.NodeType = NodeType.Mine;
                node.gold = 100;
                GraphType.mines.Add(node);
            }
        }
        
        private int CreateTownCenter(out Vector3 townCenterPosition)
        {
            int townCenterNode = Random.Range(0, Graph.CoordNodes.Count);
            GraphType.NodesType[townCenterNode].NodeType = NodeType.TownCenter;
            townCenterPosition = new Vector3(Graph.CoordNodes[townCenterNode].GetCoordinate().x,
                Graph.CoordNodes[townCenterNode].GetCoordinate().y);
            return townCenterNode;
        }

        private void AmountSafeChecks()
        {
            const int Min = 0;
            
            if (minesQuantity < Min) minesQuantity = Min;
            if (minesQuantity > (mapWidth+mapHeight)/MaxMines) minesQuantity = (mapWidth+mapHeight)/MaxMines;
            if (minersQuantity < Min) minersQuantity = Min;
            if (caravansQuantity < Min) caravansQuantity = Min;
        }

        private void CreateCaravan()
        {
            GameObject caravan = Instantiate(caravanPrefab, townCenterPosition, Quaternion.identity);
            Caravan agent2 = caravan.GetComponent<Caravan>();
            agent2.CurrentNode = GraphType.NodesType[towncenterNode];
            agent2.Voronoi = voronoi;
            agent2.Init();
        }

        private void CreateMiner()
        {
            GameObject miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            Miner agent = miner.GetComponent<Miner>();
            agent.CurrentNode = GraphType.NodesType[towncenterNode];
            RTSAgent.TownCenter = GraphType.NodesType[towncenterNode];
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
            
            GraphType.NodesType.ForEach(node =>
            {
                if (node.NodeType == NodeType.Mine && node.gold <= 0) node.NodeType = NodeType.Empty;
            });
            
            GraphType.mines.RemoveAll(node => node.gold <= 0);

            
            foreach (var mine in GraphType.mines)
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


            foreach (var node in GraphType.NodesType)
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