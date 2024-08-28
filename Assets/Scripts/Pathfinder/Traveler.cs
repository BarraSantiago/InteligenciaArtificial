using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Pathfinder
{
    enum PathfinderType
    {
        AStar,
        Dijkstra,
        Breath,
        Depth
    }

    public class Traveler : MonoBehaviour
    {
        [SerializeField] private PathfinderType _pathfinderType;
        public GraphView graphView;

        private Pathfinder<Node<Vec2Int>> Pathfinder;
        private Vector2IntGraph<Node<Vec2Int>> _graph = new Vector2IntGraph<Node<Vec2Int>>(10,10);
        private Node<Vec2Int> startNode;
        private Node<Vec2Int> destinationNode;

        private void Awake()
        {
            graphView.Graph = _graph;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;

            StartPath();
        }

        private void StartPath()
        {
            graphView.Graph = _graph;

            Pathfinder<Node<Vec2Int>> pathfinder = _pathfinderType switch
            {
                PathfinderType.AStar => new AStarPathfinder<Node<Vec2Int>>(_graph.nodes),
                PathfinderType.Dijkstra => new DijkstraPathfinder<Node<Vec2Int>>(_graph.nodes),
                PathfinderType.Breath => new BreadthPathfinder<Node<Vec2Int>>(_graph.nodes),
                PathfinderType.Depth => new DepthFirstPathfinder<Node<Vec2Int>>(_graph.nodes),
                _ => new AStarPathfinder<Node<Vec2Int>>(_graph.nodes)
            };

            startNode = new Node<Vec2Int>();
            startNode.SetCoordinate(new Vec2Int(Random.Range(0, 10), Random.Range(0, 10)));

            destinationNode = new Node<Vec2Int>();
            destinationNode.SetCoordinate(new Vec2Int(Random.Range(0, 10), Random.Range(0, 10)));

            List<Node<Vec2Int>> path = pathfinder.FindPath(startNode, destinationNode);

            graphView.Transitions = pathfinder.transitions;
            graphView.startNode = startNode;
            graphView.destinationNode = destinationNode;
            graphView.path = path;

            transform.position = new Vector3(startNode.GetCoordinate().x, startNode.GetCoordinate().y);

            StartCoroutine(Move(path));
        }

        private IEnumerator Move(List<Node<Vec2Int>> path)
        {
            foreach (Node<Vec2Int> node in path)
            {
                transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
                yield return new WaitForSeconds(1.0f);
            }

            Debug.Log("Destination reached");
        }
    }
}