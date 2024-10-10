using FlappyIa.GeneticAlg;
using FlappyIa.NeuralNet;
using FlappyIa.Obstacles;
using UnityEngine;

namespace FlappyIa.Bird
{
    public class BirdBase : MonoBehaviour
    {
        public enum State
        {
            Alive,
            Dead
        }

        public State state
        {
            get; private set;
        }

        protected Genome genome;
        protected NeuralNetwork brain;
        protected BirdBehaviour birdBehaviour;

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
        
            Obstacle obstacle = ObstacleManager.Instance.GetNextObstacle(this.transform.position);
            
            Coin coin = ObstacleManager.Instance.GetNextCoin(this.transform.position);
            

            if (!obstacle || !coin)
                return;

            OnThink(dt, birdBehaviour, obstacle, coin);

            birdBehaviour.UpdateBird(dt);

            if (!(this.transform.position.y > 5f) && !(this.transform.position.y < -5f) &&
                !ObstacleManager.Instance.IsColliding(this.transform.position)) return;
        
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
