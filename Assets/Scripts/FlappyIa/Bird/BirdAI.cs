using UnityEngine;

public class BirdAI : BirdBase
{
    int lastCoinId = -1;

    protected override void OnThink(float dt, BirdBehaviour birdBehaviour, Obstacle obstacle, Coin coin)
    {
        float[] inputs = new float[4];
        inputs[0] = (obstacle.transform.position - birdBehaviour.transform.position).x / 10.0f;
        inputs[1] = (obstacle.transform.position - birdBehaviour.transform.position).y / 10.0f;
        inputs[2] = (coin.transform.position - coin.transform.position).x / 10.0f;
        inputs[3] = (coin.transform.position - birdBehaviour.transform.position).y / 10.0f;


        float[] outputs;
        outputs = brain.Synapsis(inputs);
        if (outputs[0] < 0.5f)
        {
            birdBehaviour.Flap();
        }

        if (Vector3.Distance(obstacle.transform.position, birdBehaviour.transform.position) <= 1.0f)
        {
            genome.fitness *= 2;
        }

        if (Vector3.Distance(coin.transform.position, birdBehaviour.transform.position) <= .05f && lastCoinId != coin.id)
        {
            genome.fitness *= 2;
            lastCoinId = coin.id;
        }

        genome.fitness += (100.0f - Vector3.Distance(obstacle.transform.position, birdBehaviour.transform.position));
    }

    protected override void OnDead()
    {
    }

    protected override void OnReset()
    {
        genome.fitness = 0.0f;
    }
}