using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using UnityEditor;
using Pathfinder;
using Utils;

namespace VoronoiDiagram
{
    public class Sector
    {
        private Node<Vec2Int> mine;
        private Color color;
        private List<Segment> segments = new List<Segment>();
        private List<Vector2> intersections = new List<Vector2>();
        private List<Node<Vec2Int>> nodesInsideSector = new List<Node<Vec2Int>>();
        private List<Vector3> points;
        private static readonly Vector2 WrongPoint = new Vector2(-1, -1);
        public Node<Vec2Int> Mine
        {
            get => mine;
        }

        public Sector(Node<Vec2Int> mine)
        {
            this.mine = mine;
            //color = Random.ColorHSV();
            color.a = 0.2f;
        }

        #region SEGMENTS

        public void AddSegmentLimits(List<Limit> limits)
        {
            // Calculo los segmentos con los limites del mapa
            for (int i = 0; i < limits.Count; i++)
            {
                Vector2 origin =
                    new Vector2(mine.GetCoordinate().x, mine.GetCoordinate().y); // Obtengo la posicion de la mina
                Vector2 final = limits[i].GetMapLimitPosition(origin); // Obtengo la posicion final del segmento
                segments.Add(new Segment(origin, final));
            }
        }

        public void AddSegment(Vector2 origin, Vector2 final)
        {
            segments.Add(new Segment(origin, final));
        }

        #endregion

        #region INTERSECTION

        public void SetIntersections()
        {
            intersections.Clear();

            // Calculo las intersecciones entre cada segmento (menos entre si mismo)
            for (int i = 0; i < segments.Count; i++)
            {
                for (int j = 0; j < segments.Count; j++)
                {
                    if (i == j) continue;

                    // Obtengo la interseccion
                    Vector2 intersectionPoint = GetIntersection(segments[i], segments[j]);
                    
                    if(intersectionPoint == WrongPoint) continue;

                    // Chequeo si esa interseccion ya existe
                    if (intersections.Contains(intersectionPoint)) continue;

                    // Calculo la distancia maxima entre la interseccion y el punto de oriden del segmento
                    float maxDistance = Vector2.Distance(intersectionPoint, segments[i].Origin);

                    // Determino si la interseccion es valida
                    bool checkValidPoint = false;
                    for (int k = 0; k < segments.Count; k++)
                    {
                        // Recorro todos los segmentos de nuevo para verificar si hay otro
                        // segmento más cercano a la intersección que el segmento actual

                        // Esto es porque cada interseccion debe representar un punto donde los dos segmentos son los mas cercanos
                        // entre si, ya que cada punto en el plano pertenece a la region de voronoi de su segmento mas cercano
                        if (k == i || k == j) continue;
                        if (CheckIfHaveAnotherPositionCloser(intersectionPoint, segments[k].Final, maxDistance))
                        {
                            checkValidPoint = true; // Interseccion no valida
                            break;
                        }
                    }

                    if (!checkValidPoint)
                    {
                        intersections.Add(intersectionPoint);
                        segments[i].Intersections.Add(intersectionPoint);
                        segments[j].Intersections.Add(intersectionPoint);
                    }
                }
            }

            // Cada segmento debe tener exactamente dos intersecciones con otros segmentos, sino no es valido
            segments.RemoveAll((s) => s.Intersections.Count != 2);

            // Ordeno las intersecciones de acuerdo al angulo con respecto a un centro
            // determinado, sino los segmentos no se conectan bien y hay errores
            SortIntersections();

            // Creo un conjunto de puntos para definir los limites de los sectores
            SetPointsInSector();
        }

        public Vector2 GetIntersection(Segment seg1, Segment seg2) // Calculo la interseccion entre 2 segmentos definidos por 4 puntos
        {
            Vector2 intersection = Vector2.zero;

            // Punto medio de seg1
            Vector2 p1 = seg1.Mediatrix;
            // Calculo p2 extendiendo el segmento en su direccion por la longitud
            Vector2 p2 = seg1.Mediatrix + seg1.Direction * MapGenerator.MapDimensions.magnitude;

            Vector2 p3 = seg2.Mediatrix;
            Vector2 p4 = seg2.Mediatrix +
                         seg2.Direction * MapGenerator.MapDimensions.magnitude; // (Magnitud es la longitud del vector)

            // Chequeo si los dos segmentos son paralelos, si es asi no hay interseccion
            if (((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x)) == 0)
            {
                return WrongPoint;
            }
            else
            {
                // Para calcular las coordenadas de la interseccion uso la formula de interseccion de dos lineas en el plano:
                /*
                   1.Ecuaciones de lineas en el plano:                             Ax + By = C1      y      Ax + By = C2
                   2. Calculo el determinante principal (D):                               D = A1 * B2 - A2 * B1
                   3. Calcula el determinante x (Dx):                                      Dx = C1 * B2 - C2 * B1
                   4. Calcula el determinante y (Dy):                                      Dy = A1 * C2 - A2 * C1
                   5. Calcula las coordenadas del punto de intersección (x, y):      x = Dx / D      y      y = Dy / D

                   A1 = p1.x - p2.x
                   B1 = p1.y - p2.y
                   A2 = p3.x - p4.x
                   B2 = p3.y - p4.y
                   C1 = p1.x * p2.y - p1.y * p2.x
                   C2 = p3.x * p4.y - p3.y * p4.x
                */
                intersection.x =
                    ((p1.x * p2.y - p1.y * p2.x) * (p3.x - p4.x) - (p1.x - p2.x) * (p3.x * p4.y - p3.y * p4.x)) /
                    ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));
                intersection.y =
                    ((p1.x * p2.y - p1.y * p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x * p4.y - p3.y * p4.x)) /
                    ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));
                return intersection;
            }
        }

        private bool CheckIfHaveAnotherPositionCloser(Vector2 intersectionPoint, Vector2 pointEnd, float maxDistance)
        {
            return (Vector2.Distance(intersectionPoint, pointEnd) < maxDistance);
        }

        private void SortIntersections()
        {
            List<IntersectionPoint> intersectionPoints = new List<IntersectionPoint>();

            for (int i = 0; i < intersections.Count; i++)
            {
                // Agrego las intersecciones a la lista
                intersectionPoints.Add(new IntersectionPoint(intersections[i]));
            }

            // Calculo los valores maximos y minimos de X e Y de las intersecciones para determinar el punto central (centroide)
            float minX = intersectionPoints[0].Position.x;
            float maxX = intersectionPoints[0].Position.x;
            float minY = intersectionPoints[0].Position.y;
            float maxY = intersectionPoints[0].Position.y;

            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersectionPoints[i].Position.x < minX) minX = intersectionPoints[i].Position.x;
                if (intersectionPoints[i].Position.x > maxX) maxX = intersectionPoints[i].Position.x;
                if (intersectionPoints[i].Position.y < minY) minY = intersectionPoints[i].Position.y;
                if (intersectionPoints[i].Position.y > maxY) maxY = intersectionPoints[i].Position.y;
            }

            Vector2 center = new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);

            // Calculo el angulo de cada interseccion con respecto al punto central:
            // calculo el angulo en radianes entre el punto de interseccion y un punto central con el eje horizontal
            for (int i = 0; i < intersectionPoints.Count; i++)
            {
                Vector2 pos = intersectionPoints[i].Position;
                intersectionPoints[i].Angle = Mathf.Acos((pos.x - center.x) /
                                                         Mathf.Sqrt(Mathf.Pow(pos.x - center.x, 2f) +
                                                                    Mathf.Pow(pos.y - center.y, 2f)));

                // Si la coordenada Y de la interseccion es mayor que la coordenada Y del centro, ajusto el angulo para
                // garantizar que este en el rango correct (0 a 2pi radianes)
                if (pos.y > center.y) intersectionPoints[i].Angle = Mathf.PI + Mathf.PI - intersectionPoints[i].Angle;
            }

            // Ordeno las interseccion en funcion de sus angulos (ascendente, en sentido anti-horario)
            intersectionPoints = intersectionPoints.OrderBy(p => p.Angle).ToList();
            intersections.Clear();

            for (int i = 0; i < intersectionPoints.Count; i++)
            {
                intersections.Add(intersectionPoints[i].Position);
            }
        }

        private void SetPointsInSector()
        {
            points = new List<Vector3>();
            for (int i = 0; i < intersections.Count; i++)
            {
                // Asigno cada interseccion como un punto
                var vec3 = new Vector3(intersections[i].x, intersections[i].y, 0f);

                points.Add(vec3);
            }

            // Se crea un punto adicional que es igual al primer punto, para completar el limite del ultimo sector
            points.Add(points[0]);
        }

        #endregion

        public bool CheckPointInSector(Vector3 position) // Calculo si "position" esta dentro de un sector del diagrama
        {
            if (points == null) return false;

            bool inside = false;

            // Inicializo "point" con el ultimo punto (^1) de la matriz "points"
            Vector2 point = points[^1];

            for (int i = 0; i < points.Count; i++)
            {
                // Guardo el valor X e Y del punto anterior y el punto actual
                float previousX = point.x;
                float previousY = point.y;
                point = points[i];

                // (El operador ^ alterna el valor del bool)
                // Calculo si "position" cruza o no una línea formada por dos puntos consecutivos en el polígono:
                // 1. Verifico si "position" esta por debajo de los puntos actual y anterior en el eje vertical (1 sola comparacion es V = V)
                // 2. Verifico si "position" esta a la izquierda de la linea que conecta los puntos actual y anterior
                bool condition1 = point.y > position.y ^ previousY > position.y;
                bool condition2 = (position.x - point.x) <
                                  (position.y - point.y) * (previousX - point.x) / (previousY - point.y);

                // Si ambas condiciones son verdaderas, el punto está fuera del polígono
                inside ^= condition1 && condition2;
            }

            return inside;
        }

        public List<Node<Vec2Int>> GetNodesInSector(List<Node<Vec2Int>> allNodes)
        {
            List<Node<Vec2Int>> nodesInSector = new List<Node<Vec2Int>>();

            foreach (Node<Vec2Int> node in allNodes)
            {
                Vector3 nodePosition = new Vector3(node.GetCoordinate().x, node.GetCoordinate().y);

                if (CheckPointInSector(nodePosition))
                {
                    nodesInSector.Add(node);
                }
            }

            return nodesInSector;
        }

        public int CalculateTotalWeight(List<Node<Vec2Int>> nodesInSector)
        {
            int totalWeight = 0;

            foreach (Node<Vec2Int> node in nodesInSector)
            {
                // TODO totalWeight += node.GetPathNodeCost();
                totalWeight += 1;
            }

            return totalWeight;
        }

        public void Draw()
        {
            Handles.color = color;
            Handles.DrawAAConvexPolygon(points.ToArray());

            Handles.color = Color.black;
            Handles.DrawPolyLine(points.ToArray());
        }
    }
}