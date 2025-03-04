using System.Collections.Generic;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.Utils;
using UI;
using UnityEngine;

namespace Graph
{
    public class NodeRenderer : MonoBehaviour
    {
        public Mesh nodeMesh;
        public Material emptyMaterial;
        public Material lakeMaterial;
        public Material mineMaterial;
        public Material stumpMaterial;
        public Material watchTowerMaterial;
        public Material treeMaterial;
        public Material townCenterMaterial;
        public Material constructionMaterial;
        public Material mountainMaterial;

        public float cellSize = 1f;

        private Dictionary<NodeTerrain, List<Matrix4x4>> terrainTransforms;
        private Dictionary<NodeTerrain, Material> terrainMaterials;
        private const int BATCH_SIZE = 1023;

        void Awake()
        {
            terrainTransforms = new Dictionary<NodeTerrain, List<Matrix4x4>>();
            terrainMaterials = new Dictionary<NodeTerrain, Material>
            {
                { NodeTerrain.Empty, emptyMaterial },
                { NodeTerrain.Lake, lakeMaterial },
                { NodeTerrain.Mine, mineMaterial },
                { NodeTerrain.Stump, stumpMaterial },
                { NodeTerrain.WatchTower, watchTowerMaterial },
                { NodeTerrain.Tree, treeMaterial },
                { NodeTerrain.TownCenter, townCenterMaterial },
                { NodeTerrain.Construction, constructionMaterial },
                { NodeTerrain.Mountain, mountainMaterial }
            };

            foreach (NodeTerrain terrain in terrainMaterials.Keys)
                terrainTransforms[terrain] = new List<Matrix4x4>();
        }

        void Start()
        {
            foreach (SimNode<IVector> node in DataContainer.Graph.NodesType)
            {
                NodeTerrain terrain = node.NodeTerrain;
                if (!terrainTransforms.ContainsKey(terrain))
                    terrainTransforms[terrain] = new List<Matrix4x4>();

                Vector3 pos = new Vector3(node.GetCoordinate().X, node.GetCoordinate().Y, 0f);
                float scale = cellSize / 5f;
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * scale);

                terrainTransforms[terrain].Add(matrix);
            }
            UiManager.OnNodeUpdate += UpdateNode;
        }

        public void UpdateNode(IVector coord, NodeTerrain oldTerrain, NodeTerrain newTerrain)
        {
            if(oldTerrain == newTerrain) return;
            Vector3 pos = new Vector3(coord.X, coord.Y, 0f);
            float scale = cellSize / 5f;
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * scale);
            
            terrainTransforms[oldTerrain].Remove(matrix);
            terrainTransforms[newTerrain].Add(matrix);
        }

        void Update()
        {
            foreach (KeyValuePair<NodeTerrain, List<Matrix4x4>> kvp in terrainTransforms)
            {
                List<Matrix4x4> matrices = kvp.Value;
                Material mat = terrainMaterials[kvp.Key];
                for (int i = 0; i < matrices.Count; i += BATCH_SIZE)
                {
                    int count = Mathf.Min(BATCH_SIZE, matrices.Count - i);
                    Graphics.DrawMeshInstanced(nodeMesh, 0, mat, matrices.GetRange(i, count));
                }
            }
        }
    }
}