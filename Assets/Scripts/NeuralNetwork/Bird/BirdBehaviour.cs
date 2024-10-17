using UnityEngine;

namespace FlappyIa.Bird
{
    public class BirdBehaviour : MonoBehaviour
    {
        const float GRAVITY = 20.0f;
        const float MOVEMENT_SPEED = 3.0f;
        const float FLAP_SPEED = 7.5f;

        private Vector3 Speed
        {
            get;
            set;
        }

        public void Reset()
        {
            Speed = Vector3.zero;
            this.transform.position = Vector3.zero;
            this.transform.rotation = Quaternion.identity;
        }

        public void Flap()
        {
            Vector3 newSpeed = Speed;
            newSpeed.y = FLAP_SPEED;
            Speed = newSpeed;
        }

        public void UpdateBird(float dt)
        {
            Vector3 newSpeed = Speed;
            newSpeed.x = MOVEMENT_SPEED;
            newSpeed.y -= GRAVITY * dt;
            Speed = newSpeed;

            this.transform.rotation = Quaternion.AngleAxis(Speed.y * 5f, Vector3.forward);

            this.transform.position += Speed * dt;
        }

    }
}