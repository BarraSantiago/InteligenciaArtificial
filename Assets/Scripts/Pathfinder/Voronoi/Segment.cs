using System.Collections.Generic;
using Pathfinder;
using UnityEngine;

namespace VoronoiDiagram
{
    public class Segment<TCoordinate> where TCoordinate : ICoordinate<TCoordinate>
    {
        private TCoordinate origin;
        private TCoordinate final;
        private TCoordinate direction;
        private TCoordinate mediatrix;
        private List<TCoordinate> intersections = new List<TCoordinate>();

        public TCoordinate Origin { get => origin; }
        public TCoordinate Final { get => final; }
        public TCoordinate Mediatrix { get => mediatrix; }
        public TCoordinate Direction { get => direction; }
        public List<TCoordinate> Intersections { get => intersections; }

        public Segment(TCoordinate origin, TCoordinate final)
        {
            this.origin = origin;
            this.final = final;

            // Mediatriz: la línea perpendicular que pasa por el punto medio:
            // 1. Sumamos la coordenada X del origen y final
            // 2. Dividimos para obtener la coordenada X de la mediatriz
            // 3. Repetimos para las coordenadas Y
            mediatrix.SetCoordinate((origin.GetX() + final.GetX()) / 2, (origin.GetY() + final.GetY()) / 2); 

            // Calculo la direccion del segmento:
            // 1. Calculo el vector perpendicular al vector que va del origen al final
            direction.SetCoordinate(final.GetX() - origin.GetX(), final.GetY() - origin.GetY());
            direction.Perpendicular();
        }
    }
}