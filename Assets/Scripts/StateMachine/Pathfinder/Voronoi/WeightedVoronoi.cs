using System.Collections.Generic;
using UnityEngine;

public class WeightedVoronoi
{
    private readonly List<VoronoiPoint> _points;
    private readonly Dictionary<(int, int), BisectorInfo> _bisectorCache;

    public WeightedVoronoi()
    {
        _points = new List<VoronoiPoint>();
        _bisectorCache = new Dictionary<(int, int), BisectorInfo>();
    }

    public List<VoronoiPoint> Points => _points;

    public void AddPoint(float x, float y, float weight = 1.0f)
    {
        _points.Add(new VoronoiPoint(x, y, weight));
    }

    private float WeightedDistance(VoronoiPoint p1, VoronoiPoint p2)
    {
        return Vector2.Distance(new Vector2(p1.X, p1.Y), new Vector2(p2.X, p2.Y)) - p1.Weight + p2.Weight;
    }

    private BisectorInfo GetBisectorInfo(int i, int j)
    {
        if (_bisectorCache.TryGetValue((i, j), out var cachedInfo))
        {
            return cachedInfo;
        }

        var point = _points[i];
        var otherPoint = _points[j];

        Vector2 midPoint = new Vector2(
            (point.X + otherPoint.X) / 2,
            (point.Y + otherPoint.Y) / 2
        );

        float dx = otherPoint.X - point.X;
        float dy = otherPoint.Y - point.Y;

        Vector2 normal = new Vector2(-dy, dx).normalized;
        float distance = WeightedDistance(point, otherPoint) / 2;
        Vector2 weightedMidPoint = midPoint + normal * distance;

        var bisectorInfo = new BisectorInfo(weightedMidPoint, normal);
        _bisectorCache[(i, j)] = bisectorInfo;
        _bisectorCache[(j, i)] = bisectorInfo;

        return bisectorInfo;
    }

    public List<VoronoiCell> ComputeDiagram(Rect boundingBox)
    {
        List<VoronoiCell> cells = new List<VoronoiCell>();

        for (int i = 0; i < _points.Count; i++)
        {
            VoronoiCell cell = new VoronoiCell(_points[i]);

            for (int j = 0; j < _points.Count; j++)
            {
                if (i == j) continue;

                var bisectorInfo = GetBisectorInfo(i, j);

                Vector2 edgeStart = bisectorInfo.MidPoint + bisectorInfo.Normal * 1000f;
                Vector2 edgeEnd = bisectorInfo.MidPoint - bisectorInfo.Normal * 1000f;

                if (ClipEdge(ref edgeStart, ref edgeEnd, boundingBox))
                {
                    VoronoiEdge edge = new VoronoiEdge(edgeStart, edgeEnd);
                    cell.Edges.Add(edge);
                }
            }

            CloseCell(cell, boundingBox);
            cells.Add(cell);
        }

        return cells;
    }

    private bool ClipEdge(ref Vector2 start, ref Vector2 end, Rect bounds)
    {
        float t0 = 0.0f;
        float t1 = 1.0f;

        Vector2 direction = end - start;

        for (int edge = 0; edge < 4; edge++)
        {
            float p = 0, q = 0, r;

            if (edge == 0) { p = -direction.x; q = start.x - bounds.xMin; }
            if (edge == 1) { p = direction.x; q = bounds.xMax - start.x; }
            if (edge == 2) { p = -direction.y; q = start.y - bounds.yMin; }
            if (edge == 3) { p = direction.y; q = bounds.yMax - start.y; }

            r = q / p;

            if (p == 0 && q < 0) return false;

            if (p < 0)
            {
                if (r > t1) return false;
                else if (r > t0) t0 = r;
            }
            else if (p > 0)
            {
                if (r < t0) return false;
                else if (r < t1) t1 = r;
            }
        }

        end = start + t1 * direction;
        start = start + t0 * direction;
        return true;
    }

private void CloseCell(VoronoiCell cell, Rect boundingBox)
{
    List<VoronoiEdge> closedEdges = new List<VoronoiEdge>();

    for (int i = 0; i < cell.Edges.Count; i++)
    {
        VoronoiEdge edge1 = cell.Edges[i];

        // Ensure the edge is within the bounding box
        Vector2 clippedStart = edge1.Start;
        Vector2 clippedEnd = edge1.End;
        ClipEdge(ref clippedStart, ref clippedEnd, boundingBox);

        // Update the edge with clipped coordinates
        edge1 = new VoronoiEdge(clippedStart, clippedEnd);

        // Adjust the edge according to intersections with other edges in the cell
        for (int j = 0; j < closedEdges.Count; j++)
        {
            VoronoiEdge edge2 = closedEdges[j];

            if (CheckIntersection(edge1.Start, edge1.End, edge2.Start, edge2.End, out Vector2 intersection))
            {
                // Clip both edges to the intersection point
                ClipEdgeToIntersection(ref edge1, intersection);
                ClipEdgeToIntersection(ref edge2, intersection);
                closedEdges[j] = edge2;
            }
        }

        // Add the finalized edge to the closed edges list
        closedEdges.Add(edge1);
    }

    // If the cell's edges do not form a closed loop, close it manually
    EnsureCellClosure(cell, closedEdges, boundingBox);
    
    cell.Edges = closedEdges;
}

private void EnsureCellClosure(VoronoiCell cell, List<VoronoiEdge> edges, Rect boundingBox)
{
    // Identify missing connections and close the loop
    List<VoronoiEdge> newEdges = new List<VoronoiEdge>();
    for (int i = 0; i < edges.Count; i++)
    {
        VoronoiEdge currentEdge = edges[i];
        VoronoiEdge nextEdge = edges[(i + 1) % edges.Count];

        if (Vector2.Distance(currentEdge.End, nextEdge.Start) > Mathf.Epsilon)
        {
            // Create a new edge to close the gap
            Vector2 start = currentEdge.End;
            Vector2 end = nextEdge.Start;

            // Clip this closing edge to the bounding box
            ClipEdge(ref start, ref end, boundingBox);

            if (start != end)
            {
                newEdges.Add(new VoronoiEdge(start, end));
            }
        }
    }

    edges.AddRange(newEdges);
}

private void ClipEdgeToIntersection(ref VoronoiEdge edge, Vector2 intersection)
{
    // Clip the edge to the intersection point
    if (Vector2.Distance(edge.Start, intersection) < Vector2.Distance(edge.End, intersection))
    {
        edge = new VoronoiEdge(edge.Start, intersection);
    }
    else
    {
        edge = new VoronoiEdge(intersection, edge.End);
    }
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
    private void ExtendEdgeToBounds(ref Vector2 pointToExtend, ref Vector2 referencePoint, Rect boundingBox)
    {
        Vector2 direction = pointToExtend - referencePoint;

        if (pointToExtend.x < boundingBox.xMin)
            pointToExtend = new Vector2(boundingBox.xMin, referencePoint.y + (boundingBox.xMin - referencePoint.x) * direction.y / direction.x);
        else if (pointToExtend.x > boundingBox.xMax)
            pointToExtend = new Vector2(boundingBox.xMax, referencePoint.y + (boundingBox.xMax - referencePoint.x) * direction.y / direction.x);

        if (pointToExtend.y < boundingBox.yMin)
            pointToExtend = new Vector2(referencePoint.x + (boundingBox.yMin - referencePoint.y) * direction.x / direction.y, boundingBox.yMin);
        else if (pointToExtend.y > boundingBox.yMax)
            pointToExtend = new Vector2(referencePoint.x + (boundingBox.yMax - referencePoint.y) * direction.x / direction.y, boundingBox.yMax);
    }

    private struct BisectorInfo
    {
        public Vector2 MidPoint;
        public Vector2 Normal;

        public BisectorInfo(Vector2 midPoint, Vector2 normal)
        {
            MidPoint = midPoint;
            Normal = normal;
        }
    }
}