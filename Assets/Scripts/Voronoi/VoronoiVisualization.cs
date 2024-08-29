using System.Collections.Generic;
using UnityEngine;

public class VoronoiVisualizer : MonoBehaviour
{
    public WeightedVoronoi voronoi;
    public Rect boundingBox;
    public Material lineMaterial;
    public float pointRadius = 0.1f;

    private void Start()
    {
        // Initialize Voronoi diagram
        voronoi = new WeightedVoronoi();

        // Add some points to the Voronoi diagram
        voronoi.AddPoint(1f, 1f);
        voronoi.AddPoint(4f, 4f);
        voronoi.AddPoint(2f, 0f);
        voronoi.AddPoint(5f, 5f);

        // Set the bounding box
        boundingBox = new Rect(0, 0, 6, 6);
    }

    private void OnDrawGizmos()
    {
        if (voronoi == null) return;

        // Compute the Voronoi diagram
        List<VoronoiCell> cells = voronoi.ComputeDiagram(boundingBox);

        // Draw each Voronoi cell
        foreach (VoronoiCell cell in cells)
        {
            // Draw the site point
            DrawSitePoint(cell.Site);

            // Draw the edges of the Voronoi cell and check for intersections
            for (int i = 0; i < cell.Edges.Count; i++)
            {
                for (int j = i + 1; j < cell.Edges.Count; j++)
                {
                    VoronoiEdge edge1 = cell.Edges[i];
                    VoronoiEdge edge2 = cell.Edges[j];

                    if (CheckIntersection(edge1.Start, edge1.End, edge2.Start, edge2.End, out Vector2 intersection))
                    {
                        ClipEdgeToIntersection(ref edge1, intersection);
                        ClipEdgeToIntersection(ref edge2, intersection);
                    }
                }

                DrawEdge(cell.Edges[i]);
            }
        }
    }

    private void DrawEdge(VoronoiEdge edge)
    {
        if (lineMaterial != null)
        {
            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);
            GL.Color(Color.white);
            GL.Vertex3(edge.Start.x, edge.Start.y, 0);
            GL.Vertex3(edge.End.x, edge.End.y, 0);
            GL.End();
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(new Vector3(edge.Start.x, edge.Start.y, 0), new Vector3(edge.End.x, edge.End.y, 0));
        }

        // Draw points at the start and end of the edge
        DrawEdgePoint(edge.Start);
        DrawEdgePoint(edge.End);
    }

    private void DrawEdgePoint(Vector2 point)
    {
        Gizmos.color = Color.blue; // You can change the color to distinguish from other points
        Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), 0.05f); // Adjust the size as needed
    }

    
    private void DrawSitePoint(VoronoiPoint site)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(site.X, site.Y, 0), pointRadius);
    }

   

    public bool CheckIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float a1 = p2.y - p1.y;
        float b1 = p1.x - p2.x;
        float c1 = a1 * p1.x + b1 * p1.y;

        float a2 = p4.y - p3.y;
        float b2 = p3.x - p4.x;
        float c2 = a2 * p3.x + b2 * p3.y;

        float delta = a1 * b2 - a2 * b1;

        if (Mathf.Abs(delta) < Mathf.Epsilon)
        {
            // Lines are parallel
            return false;
        }

        intersection = new Vector2(
            (b2 * c1 - b1 * c2) / delta,
            (a1 * c2 - a2 * c1) / delta
        );

        // Check if the intersection point is on both line segments
        if (IsBetween(intersection, p1, p2) && IsBetween(intersection, p3, p4))
        {
            return true;
        }

        return false;
    }

    private bool IsBetween(Vector2 point, Vector2 start, Vector2 end)
    {
        return Mathf.Min(start.x, end.x) <= point.x && point.x <= Mathf.Max(start.x, end.x) &&
               Mathf.Min(start.y, end.y) <= point.y && point.y <= Mathf.Max(start.y, end.y);
    }

    public static void ClipEdgeToIntersection(ref VoronoiEdge edge, Vector2 intersection)
    {
        if (Vector2.Distance(edge.Start, intersection) < Vector2.Distance(edge.End, intersection))
        {
            edge = new VoronoiEdge(edge.Start, intersection);
        }
        else
        {
            edge = new VoronoiEdge(intersection, edge.End);
        }
    }
}
