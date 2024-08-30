using System.Collections.Generic;

public class VoronoiCell
{
    public VoronoiPoint Site { get; }
    public List<VoronoiEdge> Edges { get; set; }

    public VoronoiCell(VoronoiPoint site)
    {
        Site = site;
        Edges = new List<VoronoiEdge>();
    }
}