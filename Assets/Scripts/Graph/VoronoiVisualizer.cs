using System.Collections.Generic;
using System.Linq;
using NeuralNetworkDirectory;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.GraphDirectory.Voronoi;
using UI;
using UnityEngine;


namespace Graph
{
    public class VoronoiRenderer : MonoBehaviour
    {
        [Header("Voronoi Settings")] [SerializeField]
        private int voronoiToDraw = 0;

        [SerializeField] private Color siteColor = Color.cyan;
        [SerializeField] private Color cellColor = Color.magenta;
        [SerializeField] private float siteMarkerSize = 2f;

        [Header("Material Settings")] [SerializeField]
        private Material lineMaterial;

        [SerializeField] private UiManager uiManager;
        private bool drawVoronoi = true;

        private void Start()
        {
            uiManager.onVoronoiUpdate += (index) => voronoiToDraw = index;
            uiManager.onDrawVoronoi += () => drawVoronoi = !drawVoronoi;
        }


        private void OnPostRender()
        {
            if (!EcsPopulationManager.isRunning) return;
            if (!drawVoronoi) return;
            if (lineMaterial == null)
            {
                Debug.LogError("VoronoiRenderer: Line material is not assigned.");
                return;
            }

            // Use the line material.
            lineMaterial.SetPass(0);

            // Save the current GL state.
            GL.PushMatrix();

            // Use the current camera's projection and view matrices.
            Camera cam = Camera.current;
            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam != null)
            {
                GL.LoadProjectionMatrix(cam.projectionMatrix);
                GL.modelview = cam.worldToCameraMatrix;
            }
            else
            {
                Debug.LogError("VoronoiRenderer: No active camera found.");
            }

            // Validate the Voronoi data.
            if (DataContainer.Voronois == null || voronoiToDraw < 0 || voronoiToDraw >= DataContainer.Voronois.Length ||
                DataContainer.Voronois[voronoiToDraw] == null)
            {
                GL.PopMatrix();
                return;
            }

            // Begin drawing lines.
            GL.Begin(GL.LINES);

            foreach (Site<Point2D> site in DataContainer.Voronois[voronoiToDraw].Sites)
            {
                // Convert site position to Vector3.
                Vector3 sitePos = new Vector3((float)site.Position.X, (float)site.Position.Y, 0f);

                // Draw a cross at the site.
                GL.Color(siteColor);
                GL.Vertex(sitePos + Vector3.left * siteMarkerSize);
                GL.Vertex(sitePos + Vector3.right * siteMarkerSize);
                GL.Vertex(sitePos + Vector3.up * siteMarkerSize);
                GL.Vertex(sitePos + Vector3.down * siteMarkerSize);

                // Draw the Voronoi cell if it exists.
                if (site.CellPolygon != null && site.CellPolygon.Count > 1)
                {
                    GL.Color(cellColor);
                    List<Vector3> polyPoints = site.CellPolygon
                        .Select(p => new Vector3((float)p.X, (float)p.Y, 0f))
                        .ToList();
                    for (int i = 0; i < polyPoints.Count; i++)
                    {
                        Vector3 from = polyPoints[i];
                        Vector3 to = polyPoints[(i + 1) % polyPoints.Count];
                        GL.Vertex(from);
                        GL.Vertex(to);
                    }
                }
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}