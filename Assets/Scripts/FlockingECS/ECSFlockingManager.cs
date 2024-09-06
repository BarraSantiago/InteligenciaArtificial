using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Implementation;
using FlockingECS.Component;
using UnityEngine;
using Vector3 = global::System.Numerics.Vector3;

namespace FlockingECS
{
    public class ECSFlockingManager : MonoBehaviour
    {
        public int entityCount = 100;
        public float velocity = 0.1f;
        public GameObject prefab;

        private const int MAX_OBJS_PER_DRAWCALL = 1000;
        private Mesh prefabMesh;
        private Material prefabMaterial;
        private UnityEngine.Vector3 prefabScale;

        private List<uint> entities;

        void Start()
        {
            ECSManager.Init();
            entities = new List<uint>();
            for (int i = 0; i < entityCount; i++)
            {
                uint entityID = ECSManager.CreateEntity();
                ECSManager.AddComponent(entityID, new PositionComponent<Vector3>(new Vector3(0, -i, 0)));
                ECSManager.AddComponent(entityID,
                    new FlockComponent<Vector3>(new Vector3(), new Vector3(), new Vector3(), new Vector3()));
                entities.Add(entityID);
            }

            prefabMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            prefabMaterial = prefab.GetComponent<MeshRenderer>().sharedMaterial;
            prefabScale = prefab.transform.localScale;
        }

        void Update()
        {
            ECSManager.Tick(Time.deltaTime);
        }

        void LateUpdate()
        {
            List<Matrix4x4[]> drawMatrix = new List<Matrix4x4[]>();
            int meshes = entities.Count;
            for (int i = 0; i < entities.Count; i += MAX_OBJS_PER_DRAWCALL)
            {
                drawMatrix.Add(new Matrix4x4[meshes > MAX_OBJS_PER_DRAWCALL ? MAX_OBJS_PER_DRAWCALL : meshes]);
                meshes -= MAX_OBJS_PER_DRAWCALL;
            }

            Parallel.For(0, entities.Count, i =>
            {
                PositionComponent<Vector3> position = ECSManager.GetComponent<PositionComponent<Vector3>>(entities[i]);
                RotationComponent rotation = ECSManager.GetComponent<RotationComponent>(entities[i]);
                drawMatrix[(i / MAX_OBJS_PER_DRAWCALL)][(i % MAX_OBJS_PER_DRAWCALL)]
                    .SetTRS(new UnityEngine.Vector3(position.Position.X, position.Position.Y, position.Position.Z),
                        Quaternion.Euler(rotation.X, rotation.Y, rotation.Z), prefabScale);
            });
            for (int i = 0; i < drawMatrix.Count; i++)
            {
                Graphics.DrawMeshInstanced(prefabMesh, 0, prefabMaterial, drawMatrix[i]);
            }
        }
    }
}