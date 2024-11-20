using System;
using System.Numerics;
using NeuralNetworkDirectory.ECS;
using NeuralNetworkDirectory.PopulationManager;
using Utils;
using Vector3 = UnityEngine.Vector3;

namespace Pathfinder.Graph
{
    public class GraphManager<TVector, TTransform>
        where TTransform : ITransform<TVector> 
        where TVector : IVector, IEquatable<TVector>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Random random;

        public GraphManager(int width, int height)
        {
            Width = width;
            Height = height;
            random = new Random();
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

        public INode<IVector> GetRandomPosition()
        {
            int x = random.Next(0, Width);
            int y = random.Next(0, Height);
            return DataContainer.graph.NodesType[x, y];
        }

        public SimCoordinate GetNode(Vector3 position)
        {
            int x = (int)position.x;
            int y = (int)position.z;
            return DataContainer.graph.CoordNodes[x, y];
        }

        public void CleanMap()
        {
            foreach (var node in DataContainer.graph.NodesType)
            {
                node.Food = 0;
                node.NodeType = SimNodeType.Empty;
            }
        }
        
        public void InitializePlants(int plantCount)
        {
            for (int i = 0; i < plantCount; i++)
            {
                var plantPosition = DataContainer.gridManager.GetRandomPosition();
                plantPosition.NodeType = SimNodeType.Bush;
                plantPosition.Food = 5;
            }
        }
        
        public static INode<IVector> CoordinateToNode(IVector coordinate)
        {
            if (coordinate.X < 0 || coordinate.Y < 0 || coordinate.X >= DataContainer.graph.MaxX || coordinate.Y >= DataContainer.graph.MaxY)
            {
                return null;
            }

            return DataContainer.graph.NodesType[(int)coordinate.X, (int)coordinate.Y];
        }
    }
}