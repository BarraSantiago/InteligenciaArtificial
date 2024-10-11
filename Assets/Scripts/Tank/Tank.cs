using UnityEngine;

namespace Tank
{
    public class Tank : TankBase
    {
        const float MAX_FITNESS = 2;
        float fitness = 0;

        protected override void OnReset()
        {
            fitness = 1;
            fitnessMod = 1;
        }

        protected override void OnThink(float dt)
        {
            Vector3 dirToMine = GetDirToMine(goodMine);
            Vector3 dirToBadMine = GetDirToMine(badMine);

            inputs[0] = dirToMine.x;
            inputs[1] = dirToMine.z;
            inputs[2] = dirToBadMine.x;
            inputs[3] = dirToBadMine.z;
            inputs[4] = transform.forward.x;
            inputs[5] = transform.forward.z;

            float[] output = brain.Synapsis(inputs);

            SetForces(output[0], output[1], dt);
        }

        protected override void OnTakeMine(GameObject mine)
        {
            const int REWARD = 10;
            const float PUNISHMENT = 0.85f;
        
            if (IsGoodMine(mine))
            {
                IncreaseFitnessMod();
                if (fitnessMod > MAX_FITNESS) fitnessMod = MAX_FITNESS;

                fitness += REWARD * fitnessMod;
                badMinesCount = 0;
            }
            else
            {
                DecreaseFitnessMod();
                fitness *= PUNISHMENT + 0.04f * fitnessMod;
                badMinesCount++;
            }

            genome.fitness = fitness;
        }
    }
}