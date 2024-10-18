using System;

namespace VoronoiDiagram
{
    public class IntersectionPoint<TCoordinate>
        where TCoordinate : IEquatable<TCoordinate>
    {
        public IntersectionPoint(TCoordinate position)
        {
            Position = position;
            Angle = 0f;
        }

        public TCoordinate Position { get; }

        public float Angle { get; set; }
    }
}