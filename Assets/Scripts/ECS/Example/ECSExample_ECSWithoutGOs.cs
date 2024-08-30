using System.Collections.Generic;
using System.Threading.Tasks;
using ECS.Implementation;
using UnityEngine;

public class ECSExample_ECSWithoutGOs : MonoBehaviour
{
    public int entityCount = 100;
    public float velocity = 0.1f;
    public GameObject prefab;

    private const int MAX_OBJS_PER_DRAWCALL = 1000;
    private Mesh prefabMesh;
    private Material prefabMaterial;
    private Vector3 prefabScale;

    private List<uint> entities;

    void Start()
    {
        ECSManager.Init();
        entities = new List<uint>();
        for (int i = 0; i < entityCount; i++)
        {
            uint entityID = ECSManager.CreateEntity();
            ECSManager.AddComponent<PositionComponent>(entityID, new PositionComponent(0, -i, 0));
            ECSManager.AddComponent<VelocityComponent>(entityID,
                new VelocityComponent(velocity, Vector3.right.x, Vector3.right.y, Vector3.right.z));
            ECSManager.AddComponent<RotationComponent>(entityID, new RotationComponent(0, 0, 0));
            ECSManager.AddComponent<VelRotationComponent>(entityID,
                new VelRotationComponent(10, Vector3.up.x, Vector3.up.y, Vector3.up.z));
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
            PositionComponent position = ECSManager.GetComponent<PositionComponent>(entities[i]);
            RotationComponent rotation = ECSManager.GetComponent<RotationComponent>(entities[i]);
            drawMatrix[(i / MAX_OBJS_PER_DRAWCALL)][(i % MAX_OBJS_PER_DRAWCALL)]
                .SetTRS(new Vector3(position.X, position.Y, position.Z),
                    Quaternion.Euler(rotation.X, rotation.Y, rotation.Z), prefabScale);
        });
        for (int i = 0; i < drawMatrix.Count; i++)
        {
            Graphics.DrawMeshInstanced(prefabMesh, 0, prefabMaterial, drawMatrix[i]);
        }
    }
}