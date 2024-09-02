using System.Collections;
using System.Collections.Generic;
using Pathfinder;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StateMachine.Pathfinder
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
        /*
        [SerializeField] private PathfinderType _pathfinderType;
        public GraphView graphView;

        private Pathfinder<Node<Vector2>> Pathfinder;
        private Vector2IntGraph<Node<System.Numerics.Vector2>> _graph = new Vector2IntGraph<Node<Vector2>>(10,10);
        private Node<Vector2> startNode;
        private Node<Vector2> destinationNode;

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

            Pathfinder<Node<Vector2>> pathfinder = _pathfinderType switch
            {
                PathfinderType.AStar => new AStarPathfinder<Node<Vector2>>(_graph.nodes),
                PathfinderType.Dijkstra => new DijkstraPathfinder<Node<Vector2>>(_graph.nodes),
                PathfinderType.Breath => new BreadthPathfinder<Node<Vector2>>(_graph.nodes),
                PathfinderType.Depth => new DepthFirstPathfinder<Node<Vector2>>(_graph.nodes),
                _ => new AStarPathfinder<Node<Vector2>>(_graph.nodes)
            };

            startNode = new Node<Vector2>();
            startNode.SetCoordinate(new Vector2(Random.Range(0, 10), Random.Range(0, 10)));

            destinationNode = new Node<Vector2>();
            destinationNode.SetCoordinate(new Vector2(Random.Range(0, 10), Random.Range(0, 10)));

            List<Node<Vector2>> path = pathfinder.FindPath(startNode, destinationNode);

            graphView.Transitions = pathfinder.transitions;
            graphView.startNode = startNode;
            graphView.destinationNode = destinationNode;
            graphView.path = path;

            transform.position = new Vector3(startNode.GetCoordinate().x, startNode.GetCoordinate().y);

            StartCoroutine(Move(path));
        }

        private IEnumerator Move(List<Node<Vector2>> path)
        {
            foreach (Node<Vector2> node in path)
            {
                transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
                yield return new WaitForSeconds(1.0f);
            }

            Debug.Log("Destination reached");
        }*/
    }
}