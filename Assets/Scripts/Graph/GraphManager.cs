using System;
using System.Collections.Generic;
using NeuralNetworkDirectory;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.Utils;
using Random = System.Random;

namespace Pathfinder.Graph
{
    public class GraphManager<TVector, TTransform>
        where TTransform : ITransform<TVector>
        where TVector : IVector, IEquatable<TVector>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Random random;
        SimNode<IVector>[,] allNodes;
        List<SimNode<IVector>> emptyNodes = new List<SimNode<IVector>>();

        public GraphManager(int width, int height)
        {
            Width = width;
            Height = height;
            random = new Random();
        }

        public SimNode<IVector> GetRandomPositionInLowerQuarter()
        {
            int x = random.Next(0, Width);
            int y = random.Next(1, Height / 4);
            return DataContainer.Graph.NodesType[x, y];
        }

        public SimNode<IVector> GetRandomPositionInUpperQuarter()
        {
            int x = random.Next(0, Width);
            int y = random.Next(3 * Height / 4, Height - 1);
            return DataContainer.Graph.NodesType[x, y];
        }

        public SimNode<IVector> GetRandomPosition()
        {
            int x = random.Next(0, Width);
            int y = random.Next(0, Height);
            return DataContainer.Graph.NodesType[x, y];
        }

        public void CleanMap()
        {
            allNodes ??= DataContainer.Graph.NodesType;
            foreach (SimNode<IVector> node in allNodes)
            {
                if (node.NodeTerrain == NodeTerrain.Stump)
                {
                    node.NodeTerrain = NodeTerrain.Empty;
                }
            }

            if (emptyNodes.Count < 20)
            {
                foreach (SimNode<IVector> node in allNodes)
                {
                    if (node.NodeTerrain == NodeTerrain.Empty)
                    {
                        emptyNodes.Add(node);
                    }
                }
            }


            Random random = new Random();
            for (int i = emptyNodes.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (emptyNodes[i], emptyNodes[j]) = (emptyNodes[j], emptyNodes[i]);
            }

            for (int i = 0; i < 20; i++)
            {
                emptyNodes[i].NodeTerrain = NodeTerrain.Stump;
            }

            emptyNodes.RemoveRange(0, 20);
            DataContainer.UpdateVoronoi(NodeTerrain.Stump);
        }
    }
}