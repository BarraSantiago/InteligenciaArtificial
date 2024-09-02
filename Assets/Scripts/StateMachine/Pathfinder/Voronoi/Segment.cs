using System.Collections.Generic;
using UnityEngine;

namespace VoronoiDiagram
{
    public class Segment
    {
        private Vector2 origin;
        private Vector2 final;
        private Vector2 direction;
        private Vector2 mediatrix;
        private List<Vector2> intersections = new List<Vector2>();

        public Vector2 Origin { get => origin; }
        public Vector2 Final { get => final; }
        public Vector2 Mediatrix { get => mediatrix; }
        public Vector2 Direction { get => direction; }
        public List<Vector2> Intersections { get => intersections; }

        public Segment(Vector2 origin, Vector2 final)
        {
            this.origin = origin;
            this.final = final;

            // Mediatriz: la línea perpendicular que pasa por el punto medio:
            // 1. Sumamos la coordenada X del origen y final
            // 2. Dividimos para obtener la coordenada X de la mediatriz
            // 3. Repetimos para las coordenadas Y
            mediatrix = new Vector2((origin.x + final.x) / 2, (origin.y + final.y) / 2);

            // Calculo la direccion del segmento:
            // 1. Calculo el vector perpendicular al vector que va del origen al final
            direction = Vector2.Perpendicular(new Vector2(final.x - origin.x, final.y - origin.y));
        }
    }
}