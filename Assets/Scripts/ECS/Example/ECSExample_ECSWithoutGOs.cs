using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Implementation;
using ECS.Patron;
using UnityEngine;

public class ECSExample_ECSWithoutGOs : MonoBehaviour
{
    private const int MAX_OBJS_PER_DRAWCALL = 1000;
    public int entityCount = 100;
    public float velocity = 0.1f;
    public GameObject prefab;

    private List<uint> entities;
    private Material prefabMaterial;
    private Mesh prefabMesh;
    private Vector3 prefabScale;
    private ParallelOptions parallelOptions = new()
    {
        MaxDegreeOfParallelism = 32
    };

    private void Start()
    {
        ECSManager.Init();
        entities = new List<uint>();
        for (var i = 0; i < entityCount; i++)
        {
            var entityID = ECSManager.CreateEntity();
            ECSManager.AddComponent(entityID, new PositionComponent<Vector3>(new Vector3(0, -i, 0)));
            ECSManager.AddComponent(entityID,
                new VelocityComponent<Vector3>(velocity, Vector3.right));
            ECSManager.AddComponent(entityID, new RotationComponent(0, 0, 0));
            ECSManager.AddComponent(entityID,
                new VelRotationComponent(10, Vector3.up.x, Vector3.up.y, Vector3.up.z));
            entities.Add(entityID);
        }

        prefabMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        prefabMaterial = prefab.GetComponent<MeshRenderer>().sharedMaterial;
        prefabScale = prefab.transform.localScale;
    }

    private void Update()
    {
        ECSManager.Tick(Time.deltaTime);
    }

    private void LateUpdate()
    {
        var drawMatrix = new List<Matrix4x4[]>();
        var meshes = entities.Count;
        for (var i = 0; i < entities.Count; i += MAX_OBJS_PER_DRAWCALL)
        {
            drawMatrix.Add(new Matrix4x4[meshes > MAX_OBJS_PER_DRAWCALL ? MAX_OBJS_PER_DRAWCALL : meshes]);
            meshes -= MAX_OBJS_PER_DRAWCALL;
        }

        Parallel.For(0, entities.Count, parallelOptions, i =>
        {
            var position = ECSManager.GetComponent<PositionComponent<Vector3>>(entities[i]);
            var rotation = ECSManager.GetComponent<RotationComponent>(entities[i]);
            drawMatrix[i / MAX_OBJS_PER_DRAWCALL][i % MAX_OBJS_PER_DRAWCALL]
                .SetTRS(new Vector3(position.Position.x, position.Position.y, position.Position.z),
                    Quaternion.Euler(rotation.X, rotation.Y, rotation.Z), prefabScale);
        });
        for (var i = 0; i < drawMatrix.Count; i++)
            Graphics.DrawMeshInstanced(prefabMesh, 0, prefabMaterial, drawMatrix[i]);
    }
}