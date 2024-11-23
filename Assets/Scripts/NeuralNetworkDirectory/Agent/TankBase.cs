using System;
using Agent;
using NeuralNetworkDirectory.GeneticAlg;
using NeuralNetworkDirectory.NeuralNet;
using UnityEngine;

namespace NeuralNetworkDirectory.Agent
{
    public class TankBase : MonoBehaviour
    {
        public static GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private GameObject projectileDirection;
        public float Speed = 10.0f;
        public float RotSpeed = 20.0f;
        public int team;
        public int id;
        public Action<GameObject> OnMineTaken;
        protected GameObject badMine;
        protected int badMinesCount = 0;
        protected NeuralNetwork brain;
        protected float fitnessMod = 1;

        protected Genome genome;
        protected GameObject goodMine;
        protected float[] inputs;
        protected GameObject nearMine;
        
        private int hp = 3;
        private int turnLeftCount;
        private int turnRightCount;

        public int Hp
        {
            get => hp;
            protected set
            {
                hp = value;
                if (hp <= 0) Death();
            }
        }

        public void SetBrain(Genome genome, NeuralNetwork brain)
        {
            this.genome = genome;
            this.brain = brain;
            inputs = new float[brain.InputsCount];
            OnReset();
        }

        public void SetNearestMine(GameObject mine)
        {
            nearMine = mine;
        }

        public void SetGoodNearestMine(GameObject mine)
        {
            goodMine = mine;
        }

        public void SetBadNearestMine(GameObject mine)
        {
            badMine = mine;
        }

        protected bool IsGoodMine(GameObject mine)
        {
            return goodMine == mine;
        }

        protected Vector3 GetDirToMine(GameObject mine)
        {
            return (mine.transform.position - transform.position).normalized;
        }

        protected bool IsCloseToMine(GameObject mine)
        {
            return (transform.position - nearMine.transform.position).sqrMagnitude <= 2.0f;
        }

        protected void SetForces(float leftForce, float rightForce, float dt)
        {
            Vector3 pos = transform.position;
            float rotFactor = Mathf.Clamp(rightForce - leftForce, -1.0f, 1.0f);
            transform.rotation *= Quaternion.AngleAxis(rotFactor * RotSpeed * dt, Vector3.up);
            pos += transform.forward * (Mathf.Abs(rightForce + leftForce) * 0.5f * Speed * dt);
            transform.position = pos;

            if (rightForce > leftForce)
            {
                turnRightCount++;
                turnLeftCount = 0;
            }
            else
            {
                turnLeftCount++;
                turnRightCount = 0;
            }
        }

        protected void Shoot()
        {
            Instantiate(projectilePrefab, transform.position, transform.rotation)
                .GetComponent<TankProjectile>().Launch(projectileDirection.transform.forward, id, team);
        }

        public void Think(float dt)
        {
            const int MAX_TURNS = 100;
            const int MAX_BAD_MINES = 10;
            const float PUNISHMENT = 0.9f;

            OnThink(dt);

            if (IsCloseToMine(nearMine))
            {
                OnTakeMine(nearMine);

                OnMineTaken?.Invoke(nearMine);
            }

            CheckBadBehaviours(MAX_TURNS, MAX_BAD_MINES, PUNISHMENT);
        }

        protected virtual void OnThink(float dt)
        {
        }

        protected virtual void OnTakeMine(GameObject mine)
        {
        }

        protected virtual void OnReset()
        {
        }

        private void CheckBadBehaviours(int maxTurns, int maxBadMines, float punishment)
        {
            if (turnRightCount <= maxTurns && turnLeftCount <= maxTurns && badMinesCount < maxBadMines) return;

            if (turnRightCount > maxTurns)
            {
                DecreaseFitnessMod();
                genome.fitness *= punishment + 0.03f * fitnessMod;
            }
            else if (turnLeftCount > maxTurns)
            {
                DecreaseFitnessMod();
                genome.fitness *= punishment + 0.03f * fitnessMod;
            }

            if (badMinesCount >= maxBadMines)
            {
                DecreaseFitnessMod();
                genome.fitness *= punishment / 2 + 0.03f * fitnessMod;
            }
        }

        protected void IncreaseFitnessMod()
        {
            const float MOD = 1.1f;
            fitnessMod *= MOD;
        }

        protected void DecreaseFitnessMod()
        {
            const float MOD = 0.9f;
            fitnessMod *= MOD;
        }

        public bool TakeDamage(int damage)
        {
            Hp -= damage;
            return hp <= 0;
        }

        private void Death()
        {
            Destroy(this);
        }
    }
}