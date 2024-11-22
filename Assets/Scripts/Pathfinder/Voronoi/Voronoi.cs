using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinder.Graph;

namespace Pathfinder.Voronoi
{
    public class Voronoi<TCoordinate, TCoordinateType>
        where TCoordinate : IEquatable<TCoordinate>, ICoordinate<TCoordinateType>, new()
        where TCoordinateType : IEquatable<TCoordinateType>, new()
    {
        private readonly List<Limit<TCoordinate, TCoordinateType>> limits = new();
        private readonly List<Sector<TCoordinate, TCoordinateType>> sectors = new();

        public void Init()
        {
            InitLimits();
        }

        private void InitLimits()
        {
            // Calculo los limites del mapa con sus dimensiones, distancia entre nodos y punto de origen
            TCoordinate mapSize = new TCoordinate();
            mapSize.SetCoordinate(
                Graph<RTSNode<TCoordinateType>, TCoordinate, TCoordinateType>.MapDimensions.GetCoordinate());
            mapSize.Multiply(Graph<RTSNode<TCoordinateType>, TCoordinate, TCoordinateType>.CellSize);
            TCoordinate offset = new TCoordinate();
            offset.SetCoordinate(
                Graph<RTSNode<TCoordinateType>, TCoordinate, TCoordinateType>.OriginPosition.GetCoordinate());


            TCoordinate coordinateUp = new TCoordinate();
            coordinateUp.SetCoordinate(0, mapSize.GetY());
            coordinateUp.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinateUp, Direction.Up));

            TCoordinate coordinateDown = new TCoordinate();
            coordinateDown.SetCoordinate(mapSize.GetX(), 0f);
            coordinateDown.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinateDown, Direction.Down));

            TCoordinate coordinateRight = new TCoordinate();
            coordinateRight.SetCoordinate(mapSize.GetX(), mapSize.GetY());
            coordinateRight.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinateRight, Direction.Right));

            TCoordinate coordinateLeft = new TCoordinate();
            coordinateLeft.SetCoordinate(0, 0);
            coordinateLeft.Add(offset.GetCoordinate());
            limits.Add(new Limit<TCoordinate, TCoordinateType>(coordinateLeft, Direction.Left));
        }

        public void SetVoronoi(List<TCoordinate> goldMines)
        {
            sectors.Clear();
            if (goldMines.Count <= 0) return;

            foreach (TCoordinate mine in goldMines)
            {
                // Agrego las minas de oro como sectores
                RTSNode<TCoordinateType> node = new RTSNode<TCoordinateType>();
                node.SetCoordinate(mine.GetCoordinate());
                sectors.Add(new Sector<TCoordinate, TCoordinateType>(node));
            }

                // Agrega los limites a cada sector
            foreach (Sector<TCoordinate, TCoordinateType> sector in sectors)
                sector.AddSegmentLimits(limits);

            for (int i = 0; i < goldMines.Count; i++)
            for (int j = 0; j < goldMines.Count; j++)
            {
                // Agrego los segmentos entre cada sector (menos entre si mismo)
                if (i == j) continue;
                sectors[i].AddSegment(goldMines[i], goldMines[j]);
            }

            foreach (Sector<TCoordinate, TCoordinateType> sector in sectors)
                // Calculo las intersecciones
                sector.SetIntersections();
        }

        public RTSNode<TCoordinateType> GetMineCloser(TCoordinate agentPosition)
        {
            // Calculo que mina esta mas cerca a x position
            return sectors != null
                ? (from sector in sectors
                    where sector.CheckPointInSector(agentPosition)
                    select sector.Mine).FirstOrDefault()
                : null;
        }

        public List<Sector<TCoordinate, TCoordinateType>> SectorsToDraw()
        {
            return sectors;
        }
    }
}