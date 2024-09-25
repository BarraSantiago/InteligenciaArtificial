using System.Collections.Generic;
using Pathfinder;
using Pathfinder.Graph;
using Pathfinder.Voronoi;
using StateMachine.Agents.RTS;
using UnityEditor;
using UnityEngine;
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


        public static Graph<Node<Vector2>, NodeVoronoi, Vector2> graph;
        
        private Voronoi<NodeVoronoi, Vector2> voronoi;
        private Color color;
        private void Start()
        {
            Miner.OnEmptyMine += RemakeVoronoi;
            if (!Application.isPlaying)
                return;
            
            color.a = 0.2f;
            graph = new Vector2Graph(mapWidth, mapHeight, nodesSize);
            
            voronoi = new Voronoi<NodeVoronoi, Vector2>();
            MapGenerator<NodeVoronoi, Vector2>.CellSize = nodesSize;
            MapGenerator<NodeVoronoi, Vector2>.MapDimensions = new NodeVoronoi(mapWidth, mapHeight);
            MapGenerator<NodeVoronoi, Vector2>.OriginPosition = new NodeVoronoi(originPosition);


            for (int i = 0; i < minesQuantity; i++)
            {
                int rand = Random.Range(0, graph.CoordNodes.Count);
                Node<Vector2> node = graph.NodesType[rand];
                node.NodeType = NodeType.Mine;
                node.gold = 100;
                MapGenerator<NodeVoronoi, Vector2>.mines.Add(node);
            }

            int towncenterNode = Random.Range(0, graph.CoordNodes.Count);
            graph.NodesType[towncenterNode].NodeType = NodeType.TownCenter;


            Vector3 townCenterPosition = new Vector3(graph.CoordNodes[towncenterNode].GetCoordinate().x,
                graph.CoordNodes[towncenterNode].GetCoordinate().y);

            MapGenerator<NodeVoronoi, Vector2>.nodes = graph.NodesType;
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();
            for (int i = 0; i < MapGenerator<NodeVoronoi, Vector2>.mines.Count; i++)
            {
                voronoiNodes.Add(graph.CoordNodes.Find((node => node.GetCoordinate() == MapGenerator<NodeVoronoi, Vector2>.mines[i].GetCoordinate())));
            }
            voronoi.Init();
            
            GameObject miner = Instantiate(minerPrefab, townCenterPosition, Quaternion.identity);
            Miner agent = miner.GetComponent<Miner>();
            agent.currentNode = graph.NodesType[towncenterNode];
            RTSAgent.townCenter = graph.NodesType[towncenterNode];
            agent.voronoi = voronoi;
            agent.Init();
            
            GameObject caravan = Instantiate(caravanPrefab, townCenterPosition, Quaternion.identity);
            Caravan agent2 = caravan.GetComponent<Caravan>();
            agent2.currentNode = graph.NodesType[towncenterNode];
            agent2.voronoi = voronoi;
            agent2.Init();
            //voronoi.Init();
            voronoi.SetVoronoi(voronoiNodes);
        }

        private void RemakeVoronoi()
        {
            List<NodeVoronoi> voronoiNodes = new List<NodeVoronoi>();
            foreach (var mine in MapGenerator<NodeVoronoi, Vector2>.mines)
            {
                if(mine.gold > 0)
                    voronoiNodes.Add(graph.CoordNodes.Find(node => node.GetCoordinate() == mine.GetCoordinate()));
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
                foreach (var nodeVoronoi in sector.PointsToDraw()) list.Add(new Vector3(nodeVoronoi.GetX(), nodeVoronoi.GetY()));
                Handles.DrawAAConvexPolygon(list.ToArray());

                Handles.color = Color.black;
                Handles.DrawPolyLine(list.ToArray());
            }

            voronoi.SectorsToDraw();

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