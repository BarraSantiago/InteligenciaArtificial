using System.Collections.Generic;
using System.Linq;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.GraphDirectory.Voronoi;
using UI;
using UnityEngine;

// Ensure this gives access to Site<Point2D> and Point2D

namespace Graph
{
    public class VoronoiRenderer : MonoBehaviour
    {
        [Header("Voronoi Settings")] 
        [SerializeField] private UiManager uiManager;
        [SerializeField] private int voronoiToDraw = 1;
        [SerializeField] private Color siteColor = Color.cyan;
        [SerializeField] private Color cellColor = Color.magenta;
        [SerializeField] private float siteMarkerSize = 2f;

        [Header("Material Settings")] 
        [SerializeField] private Material lineMaterial;

        private bool drawVoronoi = true;

        private void Start()
        {
            uiManager.onVoronoiUpdate += (index) => voronoiToDraw = index;
            uiManager.onDrawVoronoi += () => drawVoronoi = !drawVoronoi;
        }


        private void OnRenderObject()
        {
            if (!drawVoronoi) return;
            
            if (lineMaterial == null)
            {
                Debug.LogWarning("Line material not set on VoronoiRenderer.");
                return;
            }

            // Set the material pass (typically pass 0 for an unlit shader)
            lineMaterial.SetPass(0);

            // Push current matrix, so that any changes here don't affect other rendering.
            GL.PushMatrix();
            // Use the object's transform matrix so that lines are drawn in world space.
            GL.MultMatrix(transform.localToWorldMatrix);

            // Validate that the Voronoi data exists.
            if (DataContainer.Voronois == null ||
                voronoiToDraw < 0 ||
                voronoiToDraw >= DataContainer.Voronois.Length ||
                DataContainer.Voronois[voronoiToDraw] == null)
            {
                GL.PopMatrix();
                return;
            }

            // Draw each site.
            foreach (Site<Point2D> site in DataContainer.Voronois[voronoiToDraw].Sites)
            {
                Vector3 sitePos = new Vector3((float)site.Position.X, (float)site.Position.Y, 0f);

                // Draw the site as a cross.
                GL.Begin(GL.LINES);
                GL.Color(siteColor);
                // Horizontal line.
                GL.Vertex(sitePos + Vector3.left * siteMarkerSize);
                GL.Vertex(sitePos + Vector3.right * siteMarkerSize);
                // Vertical line.
                GL.Vertex(sitePos + Vector3.up * siteMarkerSize);
                GL.Vertex(sitePos + Vector3.down * siteMarkerSize);
                GL.End();

                // Draw the Voronoi cell if available.
                if (site.CellPolygon != null && site.CellPolygon.Count > 1)
                {
                    // Convert the polygon points to Vector3.
                    List<Vector3> polyPoints = site.CellPolygon
                        .Select(p => new Vector3((float)p.X, (float)p.Y, 0f))
                        .ToList();

                    GL.Begin(GL.LINES);
                    GL.Color(cellColor);
                    for (int i = 0; i < polyPoints.Count; i++)
                    {
                        Vector3 from = polyPoints[i];
                        Vector3 to = polyPoints[(i + 1) % polyPoints.Count];
                        GL.Vertex(from);
                        GL.Vertex(to);
                    }

                    GL.End();
                }
            }

            // Pop the matrix to restore state.
            GL.PopMatrix();
        }
    }
}