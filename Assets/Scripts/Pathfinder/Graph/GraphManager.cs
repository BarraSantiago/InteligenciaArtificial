﻿using System.Numerics;
using NeuralNetworkDirectory.ECS;
using StateMachine.Agents.Simulation;

namespace Pathfinder.Graph
{
    public class GraphManager
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private System.Random random;

        public GraphManager(int width, int height)
        {
            Width = width;
            Height = height;
            random = new System.Random();
        }

        public Vector2 GetRandomPositionInLowerQuarter()
        {
            int x = random.Next(0, Width);
            int y = random.Next(0, Height / 4);
            return new Vector2(x, y);
        }

        public Vector2 GetRandomPositionInUpperQuarter()
        {
            int x = random.Next(0, Width);
            int y = random.Next(3 * Height / 4, Height);
            return new Vector2(x, y);
        }

        public SimNode<UnityEngine.Vector2> GetRandomPosition()
        {
            int x = random.Next(0, Width);
            int y = random.Next(0, Height);
            var node = EcsPopulationManager.CoordinateToNode(SimAgent.graph.CoordNodes[x, y]);
            return node;
        }
    }
}