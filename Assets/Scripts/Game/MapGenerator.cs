using System;
using System.Collections.Generic;
using Pathfinder;
using Toolbox;
using UnityEngine;
using Utils;

namespace Game
{
    public class MapGenerator : MonoBehaviourSingleton<MapGenerator>
    {
        [Serializable]
        public class PathNode_Visible
        {
            public Node<Vector2> pathNodeType;
            public GameObject prefab;
        }

        [Header("Map")] 
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField, Tooltip("Distance between map nodes")] private float cellSize;
        [SerializeField] private Vector2 originPosition;

        [Header("Path nodes")] 
        [SerializeField] private PathNode_Visible[] pathNodeVisibles;

        [Header("Obstacles")]
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private int obstaclesQuantity;

        [Header("Gold mines")] 
        [SerializeField] private Vector2 Vector2Prefab;
        [SerializeField] private int Vector2sQuantity;

        [Header("Urban center")] 
        //[SerializeField] private UrbanCenter urbanCenterPrefab;
        //private Pathfinding pathfinding;
        //public Pathfinding Pathfinding => pathfinding;
        public static List<Node<Vec2Int>> mines = new List<Node<Vec2Int>>();
        public static List<Node<Vec2Int>> nodes = new List<Node<Vec2Int>>();
        public static List<Vector2> Vector2sBeingUsed = new List<Vector2>();
        public static Vector2 MapDimensions;
        public static float CellSize;
        public static Vector2 OriginPosition;

        public override void Awake()
        {
            base.Awake();
            AStarPathfinder<Node<Vec2Int>>.CellSize = (int)cellSize;
            MapDimensions = new Vector2Int(width, height);
            CellSize = cellSize;
            OriginPosition = originPosition;

            //pathfinding = new Pathfinding(width, height, cellSize, originPosition);
            CreateObstacles();
            CreateVector2s();
            //CreateUrbanCenter();

            /*
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 position = pathfinding.GetGrid().GetWorldPosition(x, y) + (Vector3.one * (cellSize / 2));

                    for (int i = 0; i < pathNodeVisibles.Length; i++)
                    {
                        if (pathfinding.GetNode(x, y).pathNodeType == pathNodeVisibles[i].pathNodeType &&
                            pathNodeVisibles[i].prefab)
                        {
                            GameObject GO = Instantiate(pathNodeVisibles[i].prefab, position, Quaternion.identity,
                                transform);
                            GO.transform.localScale = Vector3.one * cellSize;
                        }
                    }
                }
            }
                */
            
        }

        private void CreateObstacles()
        {
            if (obstaclesQuantity <= 0) return;

            for (int i = 0; i < obstaclesQuantity; i++)
            {
                //CreateEntity(obstaclePrefab, false);
            }
        }

        private void CreateVector2s()
        {
            if (Vector2sQuantity <= 0) return;

            for (int i = 0; i < Vector2sQuantity; i++)
            {
                //GameObject GO = CreateEntity(Vector2Prefab.gameObject);
                //Vector2s.Add(GO.GetComponent<Vector2>());
            }
        }

        //private void CreateUrbanCenter()
        //{
        //    CreateEntity(urbanCenterPrefab.gameObject);
        //}

        /*
        private GameObject CreateEntity(GameObject buildingPrefab, bool walkable = true)
        {
            Vector2Int coords;

            do
            {
                coords = pathfinding.GetGrid().GetRandomGridObject();
            } while (!pathfinding.CheckAvailableNode(coords.x, coords.y)); // Find an available node

            // Create building
            Vector2 position = pathfinding.GetGrid().GetWorldPosition(coords.x, coords.y) +
                               (Vector3.one * (cellSize / 2));
            GameObject GO = Instantiate(buildingPrefab, position, Quaternion.identity, transform);
            if (!walkable)
                pathfinding.GetNode(coords.x, coords.y).SetIsWalkable(!pathfinding.GetNode(coords.x, coords.y).isWalkable);
            GO.transform.localScale = Vector3.one * cellSize;
            return GO;
        }*/

        public void RemoveEmptyMine(Node<Vec2Int> Vector2)
        {
            mines.Remove(Vector2);
        }
    }
}