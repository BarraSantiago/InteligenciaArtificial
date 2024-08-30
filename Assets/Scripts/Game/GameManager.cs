using Pathfinder;
using UnityEngine;
using Utils;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int mapWidth;
        [SerializeField] private int mapHeight;
        [SerializeField] private GraphView graphView;
        private Vector2IntGraph<Node<Vec2Int>> graph;
        private void Start()
        {
            graph = new Vector2IntGraph<Node<Vec2Int>>(mapWidth, mapHeight);
            graphView.Graph = graph;
        }
        
        
    }
}