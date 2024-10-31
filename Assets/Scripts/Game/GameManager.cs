using System.Collections.Generic;
using System.Linq;
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
    using GraphType = Graph<RTSNode<Vector2>, NodeVoronoi, Vector2>;

    public class GameManager : MonoBehaviour
    {
        private const int MaxMines = 4;


        public static GraphType Graph;
        public static readonly List<RTSNode<Vector2>> MinesWithMiners = new();
        public static AStarPathfinder<RTSNode<Vector2>, Vector2, NodeVoronoi> CaravanPathfinder;
        public static AStarPathfinder<RTSNode<Vector2>, Vector2, NodeVoronoi> MinerPathfinder;

        [Header("Map Config")] 
        [SerializeField] [Range(4, 150)] private int mapWidth;
        [SerializeField] [Range(4, 150)] private int mapHeight;
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

        private List<RTSNode<Vector2>> CaravanNodes;
        private Color color;
        private List<RTSNode<Vector2>> MinerNodes;
        private int towncenterNode;
        private Vector3 townCenterPosition;
        private Voronoi<NodeVoronoi, Vector2> voronoi;

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

            for (var i = 0; i < minersQuantity; i++) CreateMiner();

            for (var i = 0; i < caravansQuantity; i++) CreateCaravan();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            foreach (var sector in voronoi.SectorsToDraw())
            {
                Handles.color = color;
                var list = new List<Vector3>();
                foreach (var nodeVoronoi in sector.PointsToDraw())
                    list.Add(new Vector3(nodeVoronoi.GetX(), nodeVoronoi.GetY()));
                Handles.DrawAAConvexPolygon(list.ToArray());

                Handles.color = Color.black;
                Handles.DrawPolyLine(list.ToArray());
            }


            foreach (var node in Graph.NodesType)
            {
                Gizmos.color = node.RtsNodeType switch
                {
                    RTSNodeType.Mine => Color.yellow,
                    RTSNodeType.Empty => Color.white,
                    RTSNodeType.TownCenter => Color.blue,
                    RTSNodeType.Forest => Color.green,
                    RTSNodeType.Gravel => Color.gray,
                    RTSNodeType.Blocked => Color.red,
                    _ => Color.white
                };

                Gizmos.DrawSphere(new Vector3(node.GetCoordinate().x, node.GetCoordinate().y), nodesSize / 5);
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

            CaravanNodes = Graph.NodesType.Select(node => new RTSNode<Vector2>(node.GetCoordinate())).ToList();
            MinerNodes = Graph.NodesType.Select(node => new RTSNode<Vector2>(node.GetCoordinate())).ToList();

            UpdateCosts();

            MinerPathfinder = new AStarPathfinder<RTSNode<Vector2>, Vector2, NodeVoronoi>(MinerNodes);
            CaravanPathfinder = new AStarPathfinder<RTSNode<Vector2>, Vector2, NodeVoronoi>(CaravanNodes);

            VoronoiSetup();
        }

        private void UpdateCosts()
        {
            const int midCost = 2;

            foreach (var node in CaravanNodes.Where(node => node.RtsNodeType == RTSNodeType.Forest)) node.SetCost(midCost);

            foreach (var node in MinerNodes.Where(node => node.RtsNodeType == RTSNodeType.Gravel)) node.SetCost(midCost);
        }

        private void OnReachMine(RTSNode<Vector2> rtsNode)
        {
            RemoveEmptyNodes();
            MinesWithMiners.Add(rtsNode);
        }

        private void OnLeaveMine(RTSNode<Vector2> rtsNode)
        {
            MinesWithMiners.Remove(rtsNode);
            RemoveEmptyNodes();
        }

        private void RemoveEmptyNodes()
        {
            MinesWithMiners.RemoveAll(node => node.RtsNodeType == RTSNodeType.Empty);
        }

        private void SetupObstacles()
        {
            const int obscacleChance = 10;
            for (var i = 0; i < Graph.CoordNodes.Count; i++)
                if (Random.Range(0, 100) < obscacleChance)
                {
                    Graph.NodesType[i].RtsNodeType = RTSNodeType.Blocked;
                    Graph.NodesType[i].SetCost(1000);
                }

            for (var i = 0; i < Graph.CoordNodes.Count; i++)
                if (Random.Range(0, 100) < obscacleChance)
                    Graph.NodesType[i].RtsNodeType = RTSNodeType.Forest;
            for (var i = 0; i < Graph.CoordNodes.Count; i++)
                if (Random.Range(0, 100) < obscacleChance)
                    Graph.NodesType[i].RtsNodeType = RTSNodeType.Gravel;
        }

        private void VoronoiSetup()
        {
            var voronoiNodes = new List<NodeVoronoi>();

            foreach (var t in GraphType.mines)
                voronoiNodes.Add(Graph.CoordNodes.Find(node =>
                    node.GetCoordinate() == t.GetCoordinate()));

            voronoi.Init();
            voronoi.SetVoronoi(voronoiNodes);
        }

        private void CreateMines()
        {
            AmountSafeChecks();
            if (GraphType.mines.Count + minesQuantity > (mapWidth + mapHeight) / MaxMines) return;

            for (var i = 0; i < minesQuantity; i++)
            {
                var rand = Random.Range(0, Graph.CoordNodes.Count);
                if (Graph.NodesType[rand].RtsNodeType == RTSNodeType.Mine ||
                    Graph.NodesType[rand].RtsNodeType == RTSNodeType.TownCenter) continue;
                var node = Graph.NodesType[rand];
                node.RtsNodeType = RTSNodeType.Mine;
                node.gold = 100;
                GraphType.mines.Add(node);
            }
        }

        private int CreateTownCenter(out Vector3 townCenterPosition)
        {
            var townCenterNode = Random.Range(0, Graph.CoordNodes.Count);
            Graph.NodesType[townCenterNode].RtsNodeType = RTSNodeType.TownCenter;
            townCenterPosition = new Vector3(Graph.CoordNodes[townCenterNode].GetCoordinate().x,
                Graph.CoordNodes[townCenterNode].GetCoordinate().y);
            return townCenterNode;
        }

        private void AmountSafeChecks()
        {
            const int Min = 0;

            if (minesQuantity < Min) minesQuantity = Min;
            if (minesQuantity > (mapWidth + mapHeight) / MaxMines) minesQuantity = (mapWidth + mapHeight) / MaxMines;
            if (minersQuantity < Min) minersQuantity = Min;
            if (caravansQuantity < Min) caravansQuantity = Min;
        }

        private void CreateCaravan()
        {
            var caravan = Instantiate(caravanPrefab, townCenterPosition, Quaternion.identity);
            var agent = caravan.GetComponent<Caravan>();
            agent.CurrentRtsNode = Graph.NodesType[towncenterNode];
            agent.Voronoi = voronoi;
            agent.Pathfinder = CaravanPathfinder;
            agent.Init();
        }

        private void CreateMiner()
        {
            var miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            var agent = miner.GetComponent<Miner>();
            agent.CurrentRtsNode = Graph.NodesType[towncenterNode];
            RTSAgent.TownCenter = Graph.NodesType[towncenterNode];
            agent.Pathfinder = MinerPathfinder;
            agent.Voronoi = voronoi;
            agent.Init();
        }

        private void Retreat()
        {
            RTSAgent.Retreat = !RTSAgent.Retreat;
        }

        private void RemakeVoronoi()
        {
            var voronoiNodes = new List<NodeVoronoi>();

            Graph.NodesType.ForEach(node =>
            {
                if (node.RtsNodeType == RTSNodeType.Mine && node.gold <= 0) node.RtsNodeType = RTSNodeType.Empty;
            });

            GraphType.mines.RemoveAll(node => node.gold <= 0);


            foreach (var mine in GraphType.mines)
                if (mine.gold > 0)
                    voronoiNodes.Add(Graph.CoordNodes.Find(node => node.GetCoordinate() == mine.GetCoordinate()));

            voronoi.SetVoronoi(voronoiNodes);
        }
    }
}