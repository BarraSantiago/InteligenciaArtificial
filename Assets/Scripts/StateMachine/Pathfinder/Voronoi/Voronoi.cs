using System.Collections.Generic;
using Game;
using UnityEngine;
using Pathfinder;
using Utils;

namespace VoronoiDiagram
{
    public class Voronoi : MonoBehaviour
    {
        private List<Limit> limits = new List<Limit>();
        private List<Sector> sectors = new List<Sector>();

        public void Init()
        {
            InitLimits();
        }

        private void InitLimits()
        {
            // Calculo los limites del mapa con sus dimensiones, distancia entre nodos y punto de origen
            Vector2 mapSize = MapGenerator.MapDimensions * MapGenerator.CellSize;
            Vector2 offset = MapGenerator.OriginPosition;

            limits.Add(new Limit(new Vector2(0f, mapSize.y) + offset, DIRECTION.UP));
            limits.Add(new Limit(new Vector2(mapSize.x, 0f) + offset, DIRECTION.DOWN));

            limits.Add(new Limit(new Vector2(mapSize.x, mapSize.y) + offset, DIRECTION.RIGHT));
            limits.Add(new Limit(new Vector2(0f, 0f) + offset, DIRECTION.LEFT));
        }

        /*
        public void SetVoronoi(List<Node<System.Numerics.Vector2>> goldMines)
        {
            limits.Clear();
            sectors.Clear();
            if (goldMines.Count <= 0) return;

            for (int i = 0; i < goldMines.Count; i++)
            {
                // Agrego las minas de oro como sectores
                sectors.Add(new Sector(goldMines[i]));
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

        public Node<Vector2> GetMineCloser(Vector3 agentPosition)
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
        }*/
    }
}