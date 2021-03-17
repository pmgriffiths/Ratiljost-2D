using UnityEngine;
using UnityEngine.UI;

namespace PodtheDog.Common
{
    /**
     * Modified version of Unity standard asset FPS counter
     **/

    public class FPSCounter : MonoBehaviour
    {
        private float fpsMeasurePeriod = 0.5f;
        private int fpsAccumulator = 0;
        private float fpsNextPeriod = 0;
        private int currentFps;
        private string display = "{0}";

        private Text fpsText;

        // Use this for initialization
        void Awake()
        {
            // Get text reference
            fpsText = GameObject.Find("FpsValue").GetComponent<Text>();
        }

        private void Start()
        {
            fpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }

        private void Update()
        {
            // measure average frames per second
            fpsAccumulator++;
            if (Time.realtimeSinceStartup > fpsNextPeriod)
            {
                currentFps = (int)(fpsAccumulator / fpsMeasurePeriod);
                fpsAccumulator = 0;
                fpsNextPeriod += fpsMeasurePeriod;
                fpsText.text = string.Format(display, currentFps);
            }
        }
    }
}