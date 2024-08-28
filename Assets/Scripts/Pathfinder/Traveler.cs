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

        private AStarPathfinder<Node<Vec2Int>> Pathfinder;
        //private DijkstraPathfinder<Node<Vector2Int>> Pathfinder;
        //private DepthFirstPathfinder<Node<Vector2Int>> Pathfinder;
        //private BreadthPathfinder<Node<Vec2Int>> Pathfinder;

        private Node<Vec2Int> startNode;
        private Node<Vec2Int> destinationNode;

        void Start()
        {
            Pathfinder = new AStarPathfinder<Node<Vec2Int>>(graphView.Graph.nodes);
            startNode = new Node<Vec2Int>();
            startNode.SetCoordinate(new Vec2Int(Random.Range(0, 10), Random.Range(0, 10)));

            destinationNode = new Node<Vec2Int>();
            destinationNode.SetCoordinate(new Vec2Int(Random.Range(0, 10), Random.Range(0, 10)));

            List<Node<Vec2Int>> path = Pathfinder.FindPath(startNode, destinationNode);

            graphView.Transitions = Pathfinder.transitions;
            graphView.startNode = startNode;
            graphView.destinationNode = destinationNode;
            graphView.path = path;
            
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