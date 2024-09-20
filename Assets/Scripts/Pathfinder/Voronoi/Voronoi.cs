using System;
using System.Collections.Generic;
using Game;
using VoronoiDiagram;

namespace Pathfinder.Voronoi
{
    public class Voronoi<TCoordinate, TCoordinateType>
        where TCoordinate : IEquatable<TCoordinate>, ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>
    {
        private List<Limit<TCoordinate, TCoordinateType>> limits = new List<Limit<TCoordinate,TCoordinateType>>();
        private List<Sector<TCoordinate,TCoordinateType>> sectors = new List<Sector<TCoordinate,TCoordinateType>>();

        public void Init()
        {
            InitLimits();
        }

        private void InitLimits()
        {
            // Calculo los limites del mapa con sus dimensiones, distancia entre nodos y punto de origen
            TCoordinate mapSize = new TCoordinate();
            mapSize.SetCoordinate(MapGenerator<TCoordinate, TCoordinateType>.MapDimensions.GetCoordinate());
            mapSize.Multiply(MapGenerator<TCoordinate, TCoordinateType>.CellSize);
            TCoordinate offset = new TCoordinate();
            offset.SetCoordinate(MapGenerator<TCoordinate, TCoordinateType>.OriginPosition.GetCoordinate());

            TCoordinate coordinate = new TCoordinate();
            
            coordinate.SetCoordinate(0, mapSize.GetY());
            coordinate.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinate, Direction.Up));
            
            coordinate.SetCoordinate(mapSize.GetX(), 0f);
            coordinate.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinate, Direction.Down));
            
            coordinate.SetCoordinate(mapSize.GetX(), mapSize.GetY());
            coordinate.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinate, Direction.Right));
            
            coordinate.SetCoordinate(0, 0);
            coordinate.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinate, Direction.Left));
        }

        public void SetVoronoi(List<TCoordinate> goldMines)
        {
            sectors.Clear();
            if (goldMines.Count <= 0) return;

            for (int i = 0; i < goldMines.Count; i++)
            {
                // Agrego las minas de oro como sectores
                Node<TCoordinateType> node = new Node<TCoordinateType>();
                node.SetCoordinate(goldMines[i].GetCoordinate());
                sectors.Add(new Sector<TCoordinate, TCoordinateType>(node));
            }

            for (int i = 0; i < sectors.Count; i++)
            {
                // Agrego los limites a cada sector
                sectors[i].AddSegmentLimits(limits);
            }

            for (int i = 0; i < goldMines.Count; i++)
            {
                for (int j = 0; j < goldMines.Count; j++)
                {
                    // Agrego los segmentos entre cada sector (menos entre si mismo)
                    if (i == j) continue;
                    sectors[i].AddSegment(goldMines[i], goldMines[j]);
                }
            }

            for (int i = 0; i < sectors.Count; i++)
            {
                // Calculo las intersecciones
                sectors[i].SetIntersections();
            }
        }

        public Node<TCoordinateType> GetMineCloser(TCoordinate agentPosition)
        {
            // Calculo que mina esta mas cerca a x position
            if (sectors != null)
            {
                for (int i = 0; i < sectors.Count; i++)
                {
                    if (sectors[i].CheckPointInSector(agentPosition))
                    {
                        return sectors[i].Mine;
                    }
                }
            }

            return null;
        }

        public List<Sector<TCoordinate,TCoordinateType>> SectorsToDraw()
        {
            return sectors;
        }
    }
}