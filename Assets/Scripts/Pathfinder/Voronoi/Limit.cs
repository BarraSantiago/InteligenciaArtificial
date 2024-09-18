using UnityEngine;

namespace VoronoiDiagram
{
    public enum DIRECTION
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }

    public class Limit<TCoordinate>
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
            TCoordinate distance = new TCoordinate(Mathf.Abs(position.x - origin.x) * 2f, Mathf.Abs(position.y - origin.y) * 2f);

            switch (direction)
            {
                case DIRECTION.LEFT:
                    position.x -= distance.x;
                    break;
                case DIRECTION.UP:
                    position.y += distance.y;
                    break;
                case DIRECTION.RIGHT:
                    position.x += distance.x;
                    break;
                case DIRECTION.DOWN:
                    position.y -= distance.y;
                    break;
            }

            return position;
        }
    }
}