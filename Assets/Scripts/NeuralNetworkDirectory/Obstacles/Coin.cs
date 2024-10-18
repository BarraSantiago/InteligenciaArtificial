using System;
using UnityEngine;

namespace FlappyIa.Obstacles
{
    public class Coin : MonoBehaviour
    {
        private static UnityEngine.Camera camera1;
        public int id;
        public Action<Coin> OnDestroy;

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