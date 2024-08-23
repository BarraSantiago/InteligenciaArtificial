using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pathfinder
{
    public class Traveler : MonoBehaviour
    {
        [FormerlySerializedAs("grapfView")] public GraphView graphView;

        //private AStarPathfinder<Node<Vector2Int>> Pathfinder;
        //private DijkstraPathfinder<Node<Vector2Int>> Pathfinder;
        //private DepthFirstPathfinder<Node<Vector2Int>> Pathfinder;
        private BreadthPathfinder<Node<Vector2Int>> Pathfinder;

        private Node<Vector2Int> startNode;
        private Node<Vector2Int> destinationNode;

        void Start()
        {
            Pathfinder = new BreadthPathfinder<Node<Vector2Int>>(graphView.Graph);
            startNode = new Node<Vector2Int>();
            startNode.SetCoordinate(new Vector2Int(Random.Range(0, 10), Random.Range(0, 10)));

            destinationNode = new Node<Vector2Int>();
            destinationNode.SetCoordinate(new Vector2Int(Random.Range(0, 10), Random.Range(0, 10)));

            graphView.startNode = startNode;
            graphView.destinationNode = destinationNode;

            List<Node<Vector2Int>> path = Pathfinder.FindPath(startNode, destinationNode);
            
            graphView.path = path;
            Debug.Log("Start: " + startNode.GetCoordinate());
            Debug.Log("Destination: " + destinationNode.GetCoordinate());
            StartCoroutine(Move(path));
        }

        public IEnumerator Move(List<Node<Vector2Int>> path)
        {
            foreach (Node<Vector2Int> node in path)
            {
                transform.position = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);
                yield return new WaitForSeconds(1.0f);
            }

            Debug.Log("Destination reached");
        }
    }
}