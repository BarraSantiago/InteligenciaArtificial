using System;
using Pathfinder;

namespace VoronoiDiagram
{
    public enum DIRECTION
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    public class Limit<TCoordinate, CoordinateType> 
        where TCoordinate : ICoordinate<CoordinateType>, new()
        where CoordinateType : IEquatable<CoordinateType>
    {
        private TCoordinate origin;
        private DIRECTION direction;

        public Limit(TCoordinate origin, DIRECTION direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public TCoordinate GetMapLimitPosition(TCoordinate position)
        {
            // Calculo de la distancia al limite:
            // 1. Calculo la distancia entre "position" y el origen del límite
            // 2. Tomo el valor absoluto para asegurarme de tener una distancia positiva
            // 3. Multiplico esta distancia por 2 para extender el límite más allá de la distancia original
            TCoordinate distance = new TCoordinate();
            distance.SetCoordinate(Math.Abs(position.GetX() - origin.GetX()) * 2f, Math.Abs(position.GetY() - origin.GetY()) * 2f);
            TCoordinate limit = new TCoordinate();

            switch (direction)
            {
                case DIRECTION.LEFT:
                    limit.SetX(position.GetX() - distance.GetX());
                    break;
                case DIRECTION.UP:
                    limit.SetY(position.GetY() + distance.GetY());
                    break;
                case DIRECTION.RIGHT:
                    limit.SetX(position.GetX() + distance.GetX());
                    break;
                case DIRECTION.DOWN:
                    limit.SetY(position.GetY() - distance.GetY());
                    break;
            }

            return limit;
        }
    }
}