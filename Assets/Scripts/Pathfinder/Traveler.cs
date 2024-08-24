using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Pathfinder
{
    public class Traveler : MonoBehaviour
    {
        [FormerlySerializedAs("grapfView")] public GraphView graphView;

        //private AStarPathfinder<Node<Vector2Int>> Pathfinder;
        //private DijkstraPathfinder<Node<Vector2Int>> Pathfinder;
        //private DepthFirstPathfinder<Node<Vector2Int>> Pathfinder;
        private BreadthPathfinder<Node<Vec2Int>> Pathfinder;

        private Node<Vec2Int> startNode;
        private Node<Vec2Int> destinationNode;

        void Start()
        {
            Pathfinder = new BreadthPathfinder<Node<Vec2Int>>(graphView.Graph);
            startNode = new Node<Vec2Int>();
            startNode.SetCoordinate(new Vec2Int(Random.Range(0, 10), Random.Range(0, 10)));

            destinationNode = new Node<Vec2Int>();
            destinationNode.SetCoordinate(new Vec2Int(Random.Range(0, 10), Random.Range(0, 10)));

            graphView.startNode = startNode;
            graphView.destinationNode = destinationNode;

            List<Node<Vec2Int>> path = Pathfinder.FindPath(startNode, destinationNode);
            
            graphView.path = path;
            Debug.Log("Start: " + startNode.GetCoordinate());
            Debug.Log("Destination: " + destinationNode.GetCoordinate());
            StartCoroutine(Move(path));
        }

        public IEnumerator Move(List<Node<Vec2Int>> path)
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