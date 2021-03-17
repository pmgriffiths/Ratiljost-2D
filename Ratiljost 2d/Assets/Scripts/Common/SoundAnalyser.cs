using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundAnalyser : MonoBehaviour
{

    public AnimationCurve flickerCurve;
    public AnimationCurve FlickerCurve
    {
        get
        {
            if (flickerCurve == null)
            {
                CalculateVolumeCurve();
            }
            return flickerCurve;
        }
        set
        {
            throw new System.NotImplementedException();
        }
    }

    public AudioClip audioClip;

    public int curveResolution = 100;

    public AudioSource audioSource;

    /// <summary>
    /// Max pitch variation
    /// </summary>
    public float pitchRange = .1f;

    /// <summary>
    /// Fixed value for volume
    /// </summary>
    public float volumeLevel;

    /// <summary>
    /// Max volume variation from volumeLevel
    /// </summary>
    public float volumeRange = 0.4f;

    /// <summary>
    /// Max delay in seconds between loops
    /// </summary>
    public float maxDelay = 1.0f;

    /// <summary>
    /// Whether to play the sound clip
    /// </summary>
    public bool playSound = false;

    /// <summary>
    /// Whether the audio is current playing.
    /// </summary>
    private bool playing = false;

    /// <summary>
    /// How quickly the curve can change in each step
    /// </summary>
    public float maxValDelta = 0.05f;

    /// <summary>
    /// Object that wants to be notified of stop / start
    /// </summary>
    public SoundListener soundListener;


    /// <summary>
    /// Co-routine that plays sound
    /// </summary>
    private IEnumerator soundPlayCoroutine;

    void Start()
    {

        Debug.Assert(audioClip != null);
        Debug.Assert(audioSource != null);

        CalculateVolumeCurve();
        soundPlayCoroutine = PlayAudioClip();
    }

    void Update()
    {
        if (playSound && !playing)
        {
            // Start playing
            playing = true;
            StartCoroutine(soundPlayCoroutine);

        }
        else if (!playSound && playing)
        {
            // stop playing
            playing = false;
            StopCoroutine(soundPlayCoroutine);
        }
    }

    private IEnumerator PlayAudioClip()
    {
        audioSource.clip = audioClip;
        while (playSound)
        {
            float pitch = 1 + ((Random.value - 0.5f) * pitchRange);
            float volume = volumeLevel - (Random.value * volumeRange);

            float delay = Random.value * maxDelay;

            audioSource.pitch = pitch;
            audioSource.volume = volume;

            yield return new WaitForSeconds(delay);

            if (soundListener != null)
            {
                soundListener.NotifySoundStart(audioClip.length);
            }

            audioSource.Play();

            // Just wait for clip to end
            yield return new WaitForSeconds(audioClip.length);
            if (soundListener != null)
            {
                soundListener.NotifySoundEnd();
            }
        }
    }

    // Samples an audio clip to generate an animation curve that approximates to
    // volume levels in the clip.
    private void CalculateVolumeCurve()
    {
        flickerCurve = new AnimationCurve();

        if (!(audioClip.loadState == AudioDataLoadState.Loaded))
        {
            Debug.Log("Needed to load audio");
            audioClip.LoadAudioData();
            Debug.Assert(audioClip.loadState == AudioDataLoadState.Loaded, "Audio data failed to load " + audioClip.loadState);
        }

        float clipLength = audioClip.length;
        int totalSamples = audioClip.samples;

        float[] samples = new float[1 + totalSamples / curveResolution];
        int samplesPerKeyPoint = totalSamples / curveResolution;

        int currentPos = 0;
        float sampleTime = clipLength / curveResolution;
        List<float> soundValues = new List<float>();
        float maxVal = 0f;
        float lastVal = 0f;
        for (int i = 0; i < curveResolution; i++)
        {
            audioClip.GetData(samples, currentPos);
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
                val = lastVal < val ? lastVal + maxValDelta : lastVal - maxValDelta;
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
            flickerCurve.AddKey(new Keyframe(curveTime, f * normFactor));
            // Debug.Log("Adding keyframe time: " + curveTime + " value: " + f * normFactor);
            curveTime += sampleTime;
        }

    }
}
