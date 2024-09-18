using System;
using System.Collections.Generic;
using Game;
using Pathfinder;

namespace VoronoiDiagram
{
    public class Voronoi<TCoordinate>
        where TCoordinate : IEquatable<TCoordinate>, ICoordinate<TCoordinate>, new()
    {
        private List<Limit<TCoordinate>> limits = new List<Limit<TCoordinate>>();
        private List<Sector<TCoordinate>> sectors = new List<Sector<TCoordinate>>();

        public void Init()
        {
            InitLimits();
        }

        private void InitLimits()
        {
            // Calculo los limites del mapa con sus dimensiones, distancia entre nodos y punto de origen
            TCoordinate mapSize = new TCoordinate();
            mapSize.SetCoordinate(MapGenerator<TCoordinate>.MapDimensions);
            mapSize.Multiply(MapGenerator<TCoordinate>.CellSize);
            TCoordinate offset = new TCoordinate();
            offset.SetCoordinate(MapGenerator<TCoordinate>.OriginPosition);

            TCoordinate coordinate = new TCoordinate();
            
            coordinate.SetCoordinate(0, mapSize.GetY());
            coordinate.Add(offset);
            limits.Add(new Limit<TCoordinate>(coordinate, DIRECTION.UP));
            
            coordinate.SetCoordinate(mapSize.GetX(), 0f);
            coordinate.Add(offset);
            limits.Add(new Limit<TCoordinate>(coordinate, DIRECTION.DOWN));
            
            coordinate.SetCoordinate(mapSize.GetX(), mapSize.GetY());
            coordinate.Add(offset);
            limits.Add(new Limit<TCoordinate>(coordinate, DIRECTION.RIGHT));
            
            coordinate.SetCoordinate(0, 0);
            coordinate.Add(offset);
            limits.Add(new Limit<TCoordinate>(coordinate, DIRECTION.LEFT));
        }

        public void SetVoronoi(List<Node<TCoordinate>> goldMines)
        {
            sectors.Clear();
            if (goldMines.Count <= 0) return;

            for (int i = 0; i < goldMines.Count; i++)
            {
                // Agrego las minas de oro como sectores
                sectors.Add(new Sector<TCoordinate>(goldMines[i]));
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
                    sectors[i].AddSegment(goldMines[i].GetCoordinate(), goldMines[j].GetCoordinate());
                }
            }

            for (int i = 0; i < sectors.Count; i++)
            {
                // Calculo las intersecciones
                sectors[i].SetIntersections();
            }
        }

        public Node<TCoordinate> GetMineCloser(TCoordinate agentPosition)
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

        public void Draw()
        {
            if (sectors.Count <= 0) return;

            for (int i = 0; i < sectors.Count; i++)
            {
                sectors[i].Draw();
            }
        }
    }
}