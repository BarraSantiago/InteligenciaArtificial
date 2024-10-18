using System;
using UnityEngine;

namespace FlappyIa.Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        private static UnityEngine.Camera camera1;
        public int id;
        public Action<Obstacle> OnDestroy;

        private void Start()
        {
            camera1 ??= UnityEngine.Camera.main;
        }

        public void CheckToDestroy()
        {
            if (transform.position.x - camera1.transform.position.x < -7.5f)
            {
                if (OnDestroy != null)
                    OnDestroy.Invoke(this);

                Destroy(gameObject);
            }
        }
    }
}