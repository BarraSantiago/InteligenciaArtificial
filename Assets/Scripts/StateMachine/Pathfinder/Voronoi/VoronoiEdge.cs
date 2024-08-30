using UnityEngine;

public class VoronoiEdge
{
    public Vector2 Start { get; }
    public Vector2 End { get; }

    public VoronoiEdge(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }
}