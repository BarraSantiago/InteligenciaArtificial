using System;
using System.Threading;
using System.Threading.Tasks;
using NeuralNetworkDirectory;
using NeuralNetworkLib.DataManagement;
using NeuralNetworkLib.Utils;
using UnityEngine;

namespace Graph
{
    public class AgentsRenderer : MonoBehaviour
    {
        [Header("Population Setup")] [SerializeField] private Mesh carnivoreMesh;
        [SerializeField] private Material carnivoreMat;
        [SerializeField] private Mesh herbivoreMesh;
        [SerializeField] private Material herbivoreMat;
        [SerializeField] private Mesh cartMesh;
        [SerializeField] private Material cartMat;
        [SerializeField] private Mesh builderMesh;
        [SerializeField] private Material builderMat;
        [SerializeField] private Mesh gathererMesh;
        [SerializeField] private Material gathererMat;
        
        private ParallelOptions parallelOptions;
        private Matrix4x4[] carnivoreMatrices;
        private Matrix4x4[] herbivoreMatrices;
        private Matrix4x4[] builderMatrices;
        private Matrix4x4[] cartMatrices;
        private Matrix4x4[] gathererMatrices;
        private const int maxBuildersCarts = 18;
        private const int maxGatherers = 18;
        private readonly object _renderLock = new object();

        private void Awake()
        {
            carnivoreMatrices = new Matrix4x4[8];
            herbivoreMatrices = new Matrix4x4[16];
            builderMatrices = new Matrix4x4[maxBuildersCarts];
            cartMatrices = new Matrix4x4[maxBuildersCarts];
            gathererMatrices = new Matrix4x4[maxGatherers];
            
            parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = (int)(Environment.ProcessorCount * 0.8f)
            };
        }

        public void Render()
        {
            if (!EcsPopulationManager.isRunning) return;

            int carnivoreIndex = 0;
            int herbivoreIndex = 0;
            int carIndex = 0;
            int buiIndex = 0;
            int gatIndex = 0;

            Parallel.ForEach(DataContainer.Animals.Keys, parallelOptions, id =>
            {
                IVector pos = DataContainer.Animals[id].Transform.position;
                Vector3 position = new Vector3(pos.X, pos.Y);
                Matrix4x4.Translate(position);

                switch (DataContainer.Animals[id].agentType)
                {
                    case AgentTypes.Carnivore:
                        int carnIndex = Interlocked.Increment(ref carnivoreIndex) - 1;
                        if (carnIndex < carnivoreMatrices.Length)
                        {
                            carnivoreMatrices[carnIndex].SetTRS(position, Quaternion.identity, Vector3.one);
                        }

                        break;
                    case AgentTypes.Herbivore:
                        int herbIndex = Interlocked.Increment(ref herbivoreIndex) - 1;
                        if (herbIndex < herbivoreMatrices.Length)
                        {
                            herbivoreMatrices[herbIndex].SetTRS(position, Quaternion.identity, Vector3.one);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            Parallel.ForEach(DataContainer.TcAgents.Keys, parallelOptions, id =>
            {
                IVector pos = DataContainer.TcAgents[id].Transform.position;
                Vector3 position = new Vector3(pos.X, pos.Y);
                Matrix4x4 matrix = Matrix4x4.Translate(position);

                switch (DataContainer.TcAgents[id].AgentType)
                {
                    case AgentTypes.Builder:
                        int builderIndex = Interlocked.Increment(ref buiIndex) - 1;
                        if (builderIndex < builderMatrices.Length)
                        {
                            builderMatrices[builderIndex] = matrix;
                        }

                        break;

                    case AgentTypes.Cart:
                        int cartIndex = Interlocked.Increment(ref carIndex) - 1;
                        if (cartIndex < cartMatrices.Length)
                        {
                            cartMatrices[cartIndex] = matrix;
                        }

                        break;

                    case AgentTypes.Gatherer:
                        int gathererIndex = Interlocked.Increment(ref gatIndex) - 1;
                        if (gathererIndex < gathererMatrices.Length)
                        {
                            gathererMatrices[gathererIndex] = matrix;
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });

            lock (_renderLock)
            {
                if (carnivoreMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(carnivoreMesh, 0, carnivoreMat, carnivoreMatrices);

                if (herbivoreMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(herbivoreMesh, 0, herbivoreMat, herbivoreMatrices);

                if (builderMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(builderMesh, 0, builderMat, builderMatrices);

                if (gathererMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(gathererMesh, 0, gathererMat, gathererMatrices);

                if (cartMatrices.Length > 0)
                    Graphics.DrawMeshInstanced(cartMesh, 0, cartMat, cartMatrices);
            }
        }
    }
}