using UnityEngine;

namespace FlappyIa.Background
{
    public class BackgroundManager : MonoBehaviour
    {
        public GameObject[] frames;
        float lastCameraPos;
        float accumPos = 0;
        private UnityEngine.Camera camera1;

        private static BackgroundManager instance = null;

        public static BackgroundManager Instance
        {
            get
            {
                if (!instance)
                    instance = FindObjectOfType<BackgroundManager>();

                return instance;
            }
        }

        private void Start()
        {
            camera1 = UnityEngine.Camera.main;
        }

        private void Awake()
        {
            instance = this;
        }

        public void Reset()
        {
            this.transform.position = new Vector3(0, 0, 10);
            lastCameraPos = 0;
            accumPos = 0;

            float posx = -4;

            foreach (GameObject go in frames)
            {
                Vector3 pos = go.transform.position;
                pos.x = posx;
                go.transform.position = pos;
                posx += 7.2f;
            }
        }

        void Update()
        {
            float delta = camera1.transform.position.x - lastCameraPos;

            Vector3 parallax = this.transform.position;
            parallax.x += delta * 0.2f;
            this.transform.position = parallax;

            delta -= delta * 0.2f;

            lastCameraPos = camera1.transform.position.x;
            accumPos += delta;

            if (!(accumPos >= 7.2f)) return;
            foreach (GameObject go in frames)
            {
                Vector3 pos = go.transform.position;
                pos.x += 7.2f;
                go.transform.position = pos;
            }
            accumPos -= 7.2f;
        }
    }
}
