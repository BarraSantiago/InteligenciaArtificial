using System.Collections.Generic;
using Utils;

namespace Pathfinder
{
    public class VoronoiDiagram
    {
        public List<VoronoiPolygon> Polygons { get; private set; }

        public VoronoiDiagram(List<Node<Vec2Int>> nodes)
        {
            Polygons = GenerateVoronoiPolygons(nodes);
        }

        private List<VoronoiPolygon> GenerateVoronoiPolygons(List<Node<Vec2Int>> nodes)
        {
            // Placeholder for Voronoi diagram generation logic
            // This should be replaced with an actual Voronoi diagram generation algorithm
            List<VoronoiPolygon> polygons = new List<VoronoiPolygon>();

            // Example: Create dummy polygons for each node
            foreach (var node in nodes)
            {
                var polygon = new VoronoiPolygon(node);
                polygons.Add(polygon);
            }

            return polygons;
        }
    }

    public class VoronoiPolygon
    {
        public Node<Vec2Int> Site { get; private set; }
        public List<Vec2Int> Vertices { get; private set; }

        public VoronoiPolygon(Node<Vec2Int> site)
        {
            Site = site;
            Vertices = new List<Vec2Int>();
            // Placeholder for polygon vertices
            // This should be replaced with actual vertices from the Voronoi diagram
        }
    }
}