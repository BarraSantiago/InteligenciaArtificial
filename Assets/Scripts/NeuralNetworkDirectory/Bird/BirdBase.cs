using FlappyIa.Bird;
using FlappyIa.Obstacles;
using NeuralNetworkDirectory.GeneticAlg;
using NeuralNetworkDirectory.NeuralNet;
using UnityEngine;

namespace NeuralNetworkDirectory.Bird
{
    public class BirdBase : MonoBehaviour
    {
        public enum State
        {
            Alive,
            Dead
        }

        protected BirdBehaviour birdBehaviour;
        protected NeuralNetwork brain;

        protected Genome genome;

        public State state { get; private set; }

        private void Awake()
        {
            birdBehaviour = GetComponent<BirdBehaviour>();
        }

        public void SetBrain(Genome genome, NeuralNetwork brain)
        {
            this.genome = genome;
            this.brain = brain;
            state = State.Alive;
            birdBehaviour.Reset();
            OnReset();
        }

        public void Flap()
        {
            if (state == State.Alive) birdBehaviour.Flap();
        }

        public void Think(float dt)
        {
            if (state != State.Alive) return;

            Obstacle obstacle = ObstacleManager.Instance.GetNextObstacle(transform.position);

            Coin coin = ObstacleManager.Instance.GetNextCoin(transform.position);


            if (!obstacle || !coin)
                return;

            OnThink(dt, birdBehaviour, obstacle, coin);

            birdBehaviour.UpdateBird(dt);

            if (!(transform.position.y > 5f) && !(transform.position.y < -5f) &&
                !ObstacleManager.Instance.IsColliding(transform.position)) return;

            OnDead();
            state = State.Dead;
        }

        protected virtual void OnDead()
        {
        }

        protected virtual void OnThink(float dt, BirdBehaviour birdBehaviour, Obstacle obstacle, Coin coin)
        {
        }

        protected virtual void OnReset()
        {
        }
    }
}