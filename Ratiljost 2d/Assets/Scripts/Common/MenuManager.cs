using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace PodTheDog.Common
{
    public class MenuManager : MonoBehaviour
    {
        public AudioSource efxSource;                   

        public float pitchRange = .1f;                  // max variation in pitch
        public float volumeRange = 0.4f;                // Max variation in volume
        public float maxDelay = 1.0f;                   // max delay between loops in seconds

        public Light flickeringLight;                   // Light to be altered by audio clip
        public bool flickerLight = false;               // Whether to flicker the light.
        public AnimationCurve flickerCurve;
        public int flickeringResolution = 100;

        private float soundStartTime;
        private float soundDelayTime;
        private float lightMinIntensity;

        public float lightFlickerRange = 0.5f;
        public float maxValDelta = 0.05f;

        public AudioClip tickingNoise;

        public bool efxEnabled = false;

        private bool playing = false;
        private IEnumerator tickingCoroutine;

        void Awake()
        {
 

            tickingCoroutine = PlayTickingSounds();
            flickerCurve = GetVolumeCurve(tickingNoise, flickeringResolution);
            Debug.Log("Flicker curve: " + flickerCurve + " keys " + flickerCurve.length);
            lightMinIntensity = flickeringLight.intensity > lightFlickerRange ? flickeringLight.intensity - lightFlickerRange : 0f;

            Debug.Log("efxSource is enabled: " + efxEnabled);

        }


        // Samples an audio clip to generate an animation curve that approximates to
        // volume levels in the clip.
        AnimationCurve GetVolumeCurve(AudioClip clip, int keyPoints)
        {
            AnimationCurve curve = new AnimationCurve();
            
            float clipLength = clip.length;
            int totalSamples = clip.samples;

            float[] samples = new float[1 + totalSamples / keyPoints];
            int samplesPerKeyPoint = totalSamples / keyPoints;

            int currentPos = 0;
            float sampleTime = clipLength / keyPoints;
            List<float> soundValues = new List<float>();
            float maxVal = 0f;
            float lastVal = 0f;
            for (int i = 0; i < keyPoints; i++)
            {
                clip.GetData(samples, currentPos);
                currentPos += samplesPerKeyPoint;
                // Average the samples
                float val = 0f;
                foreach (float f in samples)
                {
                    // Only care about magnitude so make positive
                    val += f >= 0 ? f : -f;
                }
                val = val / samplesPerKeyPoint;
                // fit to delta
                if (Mathf.Abs(val - lastVal) > maxValDelta)
                {
                    val = lastVal < val ? lastVal + maxValDelta :  lastVal - maxValDelta;
                    val = val < 0 ? 0f : val;
                }
                lastVal = val;
                soundValues.Add(val);
                maxVal = maxVal > val ? maxVal : val;
            }

            // Have list of value need to normalise
            float normFactor = maxVal > 0 ? 1 / maxVal : 1f;
            float curveTime = 0f;
            foreach (float f in soundValues)
            {
                curve.AddKey(new Keyframe(curveTime, f * normFactor));
                // Debug.Log("Adding keyframe time: " + curveTime + " value: " + f * normFactor);
                curveTime += sampleTime;
            }

            return curve;
        }

        public void EnableEfx(bool enabled)
        {
            Debug.Log("Enable EFX: " + enabled);
            efxEnabled = enabled;
        }

        private void Update()
        {
            if (efxEnabled && !playing)
            {
                // Start playing
                playing = true;
                StartCoroutine(tickingCoroutine);

            } else if (!efxEnabled && playing)
            {
                // stop playing
                playing = false;
                StopCoroutine(tickingCoroutine);
            }

            float timeNow = Time.time;
            if (flickeringLight && playing && timeNow >= soundStartTime)
            {
                float timePos = timeNow - soundStartTime;

                float luma = flickerCurve.Evaluate(timePos);
                flickeringLight.intensity = lightMinIntensity + luma;

            }

            if (Input.anyKeyDown)
            {
                // Load the next scene
                SceneManager.LoadScene(1);
            }
        }

        private IEnumerator PlayTickingSounds()
        {
            while (true)
            {
                float pitch = 1 + ((Random.value - 0.5f) * pitchRange);
                float volume = 1 - (Random.value * volumeRange);

                float delay = Random.value * maxDelay;

                efxSource.pitch = pitch;
                efxSource.volume = volume;
                efxSource.clip = tickingNoise;
                soundStartTime = Time.time + delay;
                efxSource.PlayDelayed(delay);

                // Just wait for clip to end
                yield return new WaitForSeconds(efxSource.clip.length + delay);
            }
        }


        private void PlayEfxClip(AudioClip clip)
        {
            if (efxEnabled)
            {
                //Set the clip of our efxSource audio source to the clip passed in as a parameter.
                efxSource.clip = clip;
    
                //Play the clip.
                efxSource.Play();
            }
        }

        //Used to play single sound clips.
        public void PlayTicking()
        {
            PlayEfxClip(tickingNoise);
        }

    }
}
