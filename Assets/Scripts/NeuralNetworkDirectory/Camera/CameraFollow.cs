using UnityEngine;

namespace FlappyIa.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        private static CameraFollow instance;
        public GameObject agent;

        public static CameraFollow Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<CameraFollow>();

                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        public void Reset()
        {
            transform.position = new Vector3(0, 0, transform.position.z);
        }

        public void UpdateCamera()
        {
            var follow = Vector3.zero;
            /*
            if (PopulationManager.Instance)
            {
                if (PopulationManager.Instance.GetBestAgent())
                    follow = PopulationManager.Instance.GetBestAgent().transform.position;
                else
                    return;
            }
            else
                follow = agent.transform.position;*/

            var pos = transform.position;
            pos.x = follow.x;
            transform.position = pos;
        }
    }
}