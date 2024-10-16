﻿using System;
using System.Collections.Generic;

namespace Pathfinder.Graph
{
    public abstract class Graph<TNodeType, TCoordinateNode, TCoordinateType>
        where TNodeType : INode<TCoordinateType>, new()
        where TCoordinateNode : ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>, new()
    {
        public static List<Node<TCoordinateType>> mines = new();

        public static TCoordinateNode MapDimensions;
        public static TCoordinateNode OriginPosition;
        public static float CellSize;
        public readonly List<TCoordinateNode> CoordNodes = new();
        public readonly List<TNodeType> NodesType = new();

        public Graph(int x, int y, float cellSize)
        {
            MapDimensions = new TCoordinateNode();
            MapDimensions.SetCoordinate(x, y);
            CellSize = cellSize;

            CreateGraph(x, y, cellSize);

            AddNeighbors(cellSize);
        }

        public abstract void CreateGraph(int x, int y, float cellSize);

        private void AddNeighbors(float cellSize)
        {
            var neighbors = new List<INode<TCoordinateType>>();

            for (var i = 0; i < CoordNodes.Count; i++)
            {
                neighbors.Clear();
                for (var j = 0; j < CoordNodes.Count; j++)
                {
                    if (i == j) continue;

                    var isNeighbor =
                        (Approximately(CoordNodes[i].GetX(), CoordNodes[j].GetX()) &&
                         Approximately(Math.Abs(CoordNodes[i].GetY() - CoordNodes[j].GetY()), cellSize)) ||
                        (Approximately(CoordNodes[i].GetY(), CoordNodes[j].GetY()) &&
                         Approximately(Math.Abs(CoordNodes[i].GetX() - CoordNodes[j].GetX()), cellSize)) ||
                        (Approximately(Math.Abs(CoordNodes[i].GetX() - CoordNodes[j].GetX()), cellSize) &&
                         Approximately(Math.Abs(CoordNodes[i].GetY() - CoordNodes[j].GetY()), cellSize));

                    if (isNeighbor) neighbors.Add(NodesType[j]);
                }

                NodesType[i].SetNeighbors(new List<INode<TCoordinateType>>(neighbors));
            }
        }

        public bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < 1e-6f;
        }
    }
}