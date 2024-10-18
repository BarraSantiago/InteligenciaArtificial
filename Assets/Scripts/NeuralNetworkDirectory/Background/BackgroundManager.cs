using UnityEngine;

namespace FlappyIa.Background
{
    public class BackgroundManager : MonoBehaviour
    {
        private static BackgroundManager instance;
        public GameObject[] frames;
        private float accumPos;
        private UnityEngine.Camera camera1;
        private float lastCameraPos;

        public static BackgroundManager Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<BackgroundManager>();

                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        public void Reset()
        {
            transform.position = new Vector3(0, 0, 10);
            lastCameraPos = 0;
            accumPos = 0;

            float posx = -4;

            foreach (var go in frames)
            {
                var pos = go.transform.position;
                pos.x = posx;
                go.transform.position = pos;
                posx += 7.2f;
            }
        }

        private void Start()
        {
            camera1 = UnityEngine.Camera.main;
        }

        private void Update()
        {
            var delta = camera1.transform.position.x - lastCameraPos;

            var parallax = transform.position;
            parallax.x += delta * 0.2f;
            transform.position = parallax;

            delta -= delta * 0.2f;

            lastCameraPos = camera1.transform.position.x;
            accumPos += delta;

            if (!(accumPos >= 7.2f)) return;
            foreach (var go in frames)
            {
                var pos = go.transform.position;
                pos.x += 7.2f;
                go.transform.position = pos;
            }

            accumPos -= 7.2f;
        }
    }
}