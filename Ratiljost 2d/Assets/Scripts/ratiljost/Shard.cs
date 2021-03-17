using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shard : SoundListener
{
    /// <summary>
    /// whether light is flickering now
    /// </summary>
    public bool flickeringLight;


    /// <summary>
    /// When the flickering started
    /// </summary>
    public float flickerStartTime;

    /// <summary>
    /// When the flickering ends
    /// </summary>
    public float flickerEndTime;

    /// <summary>
    /// Curve that defines flickering
    /// </summary>
    public AnimationCurve flickerCurve;

    /// <summary>
    /// Bottom level intensity for light
    /// </summary>
    public float baseIntensity;

    /// <summary>
    /// Flicker factor
    /// </summary>
    public float lightFlickerIntensity;

    /// <summary>
    /// Light to flicker
    /// </summary>
    public Light shardLight;

    /// <summary>
    /// How much to vary the scale by
    /// </summary>
    public float scaleChange;

    public Lamp.LampColours shardColour;

    private Vector3 baseScale;

    private void Start()
    {
        SoundAnalyser soundAnalyser = GetComponent<SoundAnalyser>();
        flickerCurve = soundAnalyser.FlickerCurve;
        baseScale = transform.localScale;
    }

    void Update()
    {

        if (flickeringLight)
        {
            float timePos = Time.time - flickerStartTime;

            if (timePos <= flickerEndTime)
            {
                float luma = flickerCurve.Evaluate(timePos);
                shardLight.intensity = baseIntensity + luma * lightFlickerIntensity;
                transform.localScale = baseScale + (baseScale * luma * scaleChange);
            }
            else
            {
                shardLight.intensity = baseIntensity;
                flickeringLight = false;
                transform.localScale = baseScale;
            }
        } 
    }


    public override void NotifySoundStart(float clipLength)
    {
        flickerStartTime = Time.time;
        flickerEndTime = flickerStartTime + clipLength;
        flickeringLight = true;
    }

    public override void NotifySoundEnd()
    {
        flickeringLight = false;
    }
}
