using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightReactive : MonoBehaviour
{

    private List<BoxCollider> boxColliders;

    private List<Renderer> ourRenderers;

    private List<MaterialPropertyBlock> propertyBlocks;

    bool colorUpdated = false;

    private Color baseColor = Color.black;

    public Color litColour = Color.red;

    bool fading = false;

    private float fadeStartTime;

    public GameObject platform;

    public GameObject sparkles;

    /// <summary>
    /// Seconds for tile for fade back to base color
    /// </summary>
    public float timeToFade = 1f;

    List<Color> hitColors = new List<Color>();
    Color currentColor;
    float currentHue;

    // How bright the light is
    private float luminosity;

    public void Awake()
    {
        ourRenderers = new List<Renderer>();
        propertyBlocks = new List<MaterialPropertyBlock>();

        foreach (Renderer renderer in platform.GetComponentsInChildren<Renderer>())
        {
            ourRenderers.Add(renderer);
            propertyBlocks.Add(new MaterialPropertyBlock());
        }

        currentColor = baseColor;
        EnableRenders(false);
        colorUpdated = false;

    }

    private void Update()
    {
        if (colorUpdated)
        {
            EnableRenders(true);
            UpdateColors();
        }
        else if (fading)
        {
            FadeColors();
        }
    }

    private void UpdateColors()
    {
        if (colorUpdated)
        {
            SetRenderColors(currentColor);
            Color.RGBToHSV(currentColor, out currentHue, out _, out _);
            Debug.Log("Current colour updated to " + currentColor);
            colorUpdated = false;
        }
    }

    private void FadeColors()
    {
        if (fading)
        {
            float t = (Time.time - fadeStartTime) / timeToFade;
            Color targetColor = baseColor;
            Color nextColor = Color.Lerp(currentColor, targetColor, t);

            SetRenderColors(nextColor);
            if (t >= 1)
            {
                EnableRenders(false);
                fading = false;
                hitColors.Clear();
                currentColor = targetColor;
            }
        }
    }

    private void EnableRenders(bool enable)
    {
        foreach (Renderer renderer in ourRenderers)
        {
            renderer.enabled = enable;
        }
        platform.SetActive(enable);
        sparkles.SetActive(!enable);
    }

    /// <summary>
    /// Sets the color of all child renderers
    /// </summary>
    /// <param name="color"></param>
    private void SetRenderColors(Color color)
    {
        /// TODO : find out why this is 0 only, not all blocks
        int r = 0;
        foreach (Renderer renderer in ourRenderers)
        {
            MaterialPropertyBlock propertyBlock = propertyBlocks[r];
            renderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propertyBlock);
            propertyBlocks[r] = propertyBlock;
        }
    }

    /// <summary>
    /// Check for light cone collisions
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("LightCone"))
        {

            // Find lamp colour to see if it matches
            Lamp lampTrigger = other.gameObject.GetComponentInParent<Lamp>();
            Debug.Assert(lampTrigger != null);

            Color lampColour = lampTrigger.CurrentColour;
            //            Debug.Log("Lamp: " + lampColour + " lit: " + litColour);

            bool currentlyLit = CompareColours(litColour, currentColor);
            bool colourMatch = CompareColours(lampColour, litColour);

            if (colourMatch)
            {
                fading = false;
                if (!currentlyLit)
                {
                    // Lamp light marches and we need to change to lit colour
                    Debug.Log("Lamp light matches and we need to change to lit colour");
                    fading = false;
                    colorUpdated = true;
                    currentColor = litColour;
                }
            } else if (!colourMatch && currentlyLit && !fading)
            {
                // Check for change of colour from lit colour
                Debug.Log("Lamp light doesn't match, need to fade");
                fadeStartTime = Time.time;
                fading = true;
            }
        }
    }


    /// <summary>
    /// Determines whether two colors are similar enought to be same. 
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    public bool CompareColours(Color first, Color second)
    {
        // Ignore alpha and compare RBG value for now
        bool same = (first.r == second.r) && (first.g == second.g) && (first.b == second.b);
        return same;

    }


    /// <summary>
    /// Check for light cone collisions
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("LightCone"))
        {
            Debug.Log("Exit light cone");
            // stop fade
            fading = true;
            fadeStartTime = Time.time;
        }
    }

}
