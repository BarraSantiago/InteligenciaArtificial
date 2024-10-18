using UnityEngine;

namespace FlappyIa.Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        public System.Action<Obstacle> OnDestroy;
        public int id;
        private static UnityEngine.Camera camera1;

        private void Start()
        {
            camera1 ??= UnityEngine.Camera.main;
        }
        public void CheckToDestroy()
        {
            if (this.transform.position.x - camera1.transform.position.x < -7.5f)
            {
                if (OnDestroy != null)
                    OnDestroy.Invoke(this);

                Destroy(this.gameObject);
            }

        }
    }
}