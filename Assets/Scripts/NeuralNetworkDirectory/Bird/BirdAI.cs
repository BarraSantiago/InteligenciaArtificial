using FlappyIa.Obstacles;
using NeuralNetworkDirectory.Bird;
using UnityEngine;

namespace FlappyIa.Bird
{
    public class BirdAI : BirdBase
    {
        private int lastCoinId = -1;
        private int lastObstacleId = -1;

        protected override void OnThink(float dt, BirdBehaviour birdBehaviour, Obstacle obstacle, Coin coin)
        {
            float[] inputs = new float[4];
            inputs[0] = (obstacle.transform.position - birdBehaviour.transform.position).x / 10.0f;
            inputs[1] = (obstacle.transform.position - birdBehaviour.transform.position).y / 10.0f;
            inputs[2] = (coin.transform.position - coin.transform.position).x / 10.0f;
            inputs[3] = (coin.transform.position - birdBehaviour.transform.position).y / 10.0f;


            float[] outputs;
            outputs = brain.Synapsis(inputs);
            if (outputs[0] < 0.5f) birdBehaviour.Flap();

            Vector3 diff = obstacle.transform.position - birdBehaviour.transform.position;
            float sqrDistance = diff.sqrMagnitude;

            if (sqrDistance <= 1.0f * 1.0f && lastObstacleId != obstacle.id)
            {
                genome.fitness += 1500;
                lastObstacleId = obstacle.id;
            }

            Vector2 distance = coin.transform.position - birdBehaviour.transform.position;

            if (distance.sqrMagnitude <= 3f && lastCoinId != coin.id)
            {
                genome.fitness += 2500;
                lastCoinId = coin.id;
            }

            genome.fitness += 100.0f - Vector3.Distance(obstacle.transform.position, birdBehaviour.transform.position);
        }

        protected override void OnDead()
        {
        }

        protected override void OnReset()
        {
            genome.fitness = 0.0f;
        }
    }
}