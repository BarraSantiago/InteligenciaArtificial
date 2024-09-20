using UnityEngine;
using System.Collections.Generic;

public class Voronoi2 : MonoBehaviour
{
    public int pointCount = 50;
    public float width = 10f;
    public float height = 10f;
    public Color lineColor = Color.white;
    public Color pointColor = Color.red;
    private List<Vector2> points;
    private Dictionary<Vector2, List<Vector2>> voronoiCells;

    void Start()
    {
        GeneratePoints();
        ComputeVoronoiCells();
    }

    void GeneratePoints()
    {
        points = new List<Vector2>();
        for (int i = 0; i < pointCount; i++)
        {
            float x = Random.Range(0.1f, width - 0.1f);
            float y = Random.Range(0.1f, height - 0.1f);
            points.Add(new Vector2(x, y));
        }
    }

    void ComputeVoronoiCells()
    {
        voronoiCells = new Dictionary<Vector2, List<Vector2>>();

        Rect bounds = new Rect(0, 0, width, height);

        foreach (Vector2 point in points)
        {
            // Start with a large rectangle as the initial cell
            List<Vector2> cell = new List<Vector2>()
            {
                new Vector2(bounds.xMin, bounds.yMin),
                new Vector2(bounds.xMax, bounds.yMin),
                new Vector2(bounds.xMax, bounds.yMax),
                new Vector2(bounds.xMin, bounds.yMax)
            };

            foreach (Vector2 otherPoint in points)
            {
                if (otherPoint == point)
                    continue;

                // Compute the perpendicular bisector between point and otherPoint
                Vector2 midPoint = (point + otherPoint) / 2f;
                Vector2 direction = (otherPoint - point).normalized;
                Vector2 normal = new Vector2(-direction.y, direction.x);

                // Ensure the normal points towards the site point
                if (Vector2.Dot(normal, point - midPoint) < 0)
                {
                    normal = -normal; // Flip the normal to point towards the site
                }

                // Clip the current cell with this half-plane
                cell = ClipPolygon(cell, midPoint, normal);
                if (cell == null || cell.Count == 0)
                    break; // The cell has been completely clipped away
            }

            if (cell != null && cell.Count >= 3)
            {
                voronoiCells[point] = cell;
            }
        }
    }

    List<Vector2> ClipPolygon(List<Vector2> polygon, Vector2 linePoint, Vector2 lineNormal)
    {
        List<Vector2> outputList = new List<Vector2>();

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector2 A = polygon[i];
            Vector2 B = polygon[(i + 1) % polygon.Count];

            float distanceA = Vector2.Dot(A - linePoint, lineNormal);
            float distanceB = Vector2.Dot(B - linePoint, lineNormal);

            if (distanceA >= 0 && distanceB >= 0)
            {
                // Both points are inside
                outputList.Add(B);
            }
            else if (distanceA >= 0 && distanceB < 0)
            {
                // Edge from inside to outside
                Vector2 intersection = LineIntersection(A, B, linePoint, lineNormal);
                outputList.Add(intersection);
            }
            else if (distanceA < 0 && distanceB >= 0)
            {
                // Edge from outside to inside
                Vector2 intersection = LineIntersection(A, B, linePoint, lineNormal);
                outputList.Add(intersection);
                outputList.Add(B);
            }
            // Else both points are outside, do nothing
        }

        return outputList;
    }

    Vector2 LineIntersection(Vector2 A, Vector2 B, Vector2 linePoint, Vector2 lineNormal)
    {
        Vector2 AB = B - A;
        float t = Vector2.Dot(linePoint - A, lineNormal) / Vector2.Dot(AB, lineNormal);
        return A + t * AB;
    }

    void OnDrawGizmos()
    {
        if (points == null || voronoiCells == null)
            return;

        // Draw points
        Gizmos.color = pointColor;
        foreach (Vector2 point in points)
        {
            Gizmos.DrawSphere(new Vector3(point.x, 0, point.y), 0.1f);
        }

        // Draw Voronoi cells
        Gizmos.color = lineColor;
        foreach (KeyValuePair<Vector2, List<Vector2>> cell in voronoiCells)
        {
            List<Vector2> vertices = cell.Value;
            if (vertices.Count < 2)
                continue;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 A = vertices[i];
                Vector2 B = vertices[(i + 1) % vertices.Count];

                Gizmos.DrawLine(new Vector3(A.x, 0, A.y), new Vector3(B.x, 0, B.y));
            }

            // Optionally draw the centroid
            Vector2 centroid = CalculateCentroid(vertices);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(new Vector3(centroid.x, 0, centroid.y), 0.1f);
            Gizmos.color = lineColor; // Reset color for lines
        }
    }

    Vector2 CalculateCentroid(List<Vector2> pts)
    {
        float accumulatedArea = 0.0f;
        float centerX = 0.0f;
        float centerY = 0.0f;

        for (int i = 0, j = pts.Count - 1; i < pts.Count; j = i++)
        {
            float temp = pts[i].x * pts[j].y - pts[j].x * pts[i].y;
            accumulatedArea += temp;
            centerX += (pts[i].x + pts[j].x) * temp;
            centerY += (pts[i].y + pts[j].y) * temp;
        }

        if (Mathf.Abs(accumulatedArea) < 1E-7f)
            return pts[0]; // Avoid division by zero

        accumulatedArea *= 0.5f;
        centerX /= (6.0f * accumulatedArea);
        centerY /= (6.0f * accumulatedArea);

        return new Vector2(centerX, centerY);
    }
}
