using System;
using System.Collections.Generic;
using Pathfinder;
using UnityEngine;
using Utils;

namespace Game
{
    public class MapGenerator<TCoordinate, CoordinateType> 
        where TCoordinate : ICoordinate<CoordinateType>, IEquatable<TCoordinate>
        where CoordinateType : IEquatable<CoordinateType>
    {
        [Serializable]
        public class PathNode_Visible
        {
            public Node<TCoordinate> pathNodeType;
            public GameObject prefab;
        }

        [Header("Map")] 
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField, Tooltip("Distance between map nodes")] private float cellSize;
        [SerializeField] private TCoordinate originPosition;

        [Header("Path nodes")] 
        [SerializeField] private PathNode_Visible[] pathNodeVisibles;

        [Header("Obstacles")]
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private int obstaclesQuantity;

        [Header("Gold mines")] 
        [SerializeField] private TCoordinate TCoordinatePrefab;
        [SerializeField] private int TCoordinatesQuantity;

        [Header("Urban center")] 
        //[SerializeField] private UrbanCenter urbanCenterPrefab;
        //private Pathfinding pathfinding;
        //public Pathfinding Pathfinding => pathfinding;
        public static List<Node<CoordinateType>> mines = new List<Node<CoordinateType>>();
        public static List<Node<CoordinateType>> nodes = new List<Node<CoordinateType>>();
        public static List<TCoordinate> TCoordinatesBeingUsed = new List<TCoordinate>();
        public static TCoordinate MapDimensions;
        public static float CellSize;
        public static TCoordinate OriginPosition;

        public void Awake()
        {
            MapDimensions.SetCoordinate(width, height);
            CellSize = cellSize;
            OriginPosition = originPosition;

            //pathfinding = new Pathfinding(width, height, cellSize, originPosition);
            CreateObstacles();
            CreateTCoordinates();
            //CreateUrbanCenter();

            /*
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TCoordinate position = pathfinding.GetGrid().GetWorldPosition(x, y) + (Vector3.one * (cellSize / 2));

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

        private void CreateTCoordinates()
        {
            if (TCoordinatesQuantity <= 0) return;

            for (int i = 0; i < TCoordinatesQuantity; i++)
            {
                //GameObject GO = CreateEntity(TCoordinatePrefab.gameObject);
                //TCoordinates.Add(GO.GetComponent<TCoordinate>());
            }
        }

        //private void CreateUrbanCenter()
        //{
        //    CreateEntity(urbanCenterPrefab.gameObject);
        //}

        /*
        private GameObject CreateEntity(GameObject buildingPrefab, bool walkable = true)
        {
            TCoordinateInt coords;

            do
            {
                coords = pathfinding.GetGrid().GetRandomGridObject();
            } while (!pathfinding.CheckAvailableNode(coords.x, coords.y)); // Find an available node

            // Create building
            TCoordinate position = pathfinding.GetGrid().GetWorldPosition(coords.x, coords.y) +
                               (Vector3.one * (cellSize / 2));
            GameObject GO = Instantiate(buildingPrefab, position, Quaternion.identity, transform);
            if (!walkable)
                pathfinding.GetNode(coords.x, coords.y).SetIsWalkable(!pathfinding.GetNode(coords.x, coords.y).isWalkable);
            GO.transform.localScale = Vector3.one * cellSize;
            return GO;
        }*/

        public void RemoveEmptyMine(Node<CoordinateType> coordinate)
        {
            mines.Remove(coordinate);
        }
    }
}