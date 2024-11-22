using UnityEngine;

namespace FlappyIa.Bird
{
    public class BirdBehaviour : MonoBehaviour
    {
        private const float GRAVITY = 20.0f;
        private const float MOVEMENT_SPEED = 3.0f;
        private const float FLAP_SPEED = 7.5f;

        private Vector3 Speed { get; set; }

        public void Reset()
        {
            Speed = Vector3.zero;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
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

            transform.rotation = Quaternion.AngleAxis(Speed.y * 5f, Vector3.forward);

            transform.position += Speed * dt;
        }
    }
}