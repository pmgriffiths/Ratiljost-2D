using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour
{

    public Light spotLight;

    public Light areaLight;

    /// <summary>
    /// Available colours. NB. Must be same length as lampColourmap
    /// </summary>
    public List<Color> lampColours = new List<Color>();

    /// <summary>
    /// Maps the LampColour enum to the array of lamp colours. NB
    /// Must be same length as lampColours
    /// </summary>
    public List<LampColours> lampColourKeys;

    /// <summary>
    /// Number of degrees the lamp can be deflected/rotated
    /// </summary>
    public float maxDeflection = 30f;

    /// <summary>
    /// How many degrees the lamp can be deflected in any given second
    /// </summary>
    public float lampRotationSpeed = 10f;

    // Record of original position
    private Quaternion baseRotation;

    // Current deflection from original
    private float currentDeflection;

    // Original rotation z value
    private float originalZ;

    /// <summary>
    /// Current colour
    /// </summary>
    private Color currentColour;
    public Color CurrentColour
    {
       get { return colourMap[currentEnumColour]; }
    }


    /// <summary>
    /// current enumeration lamp colour
    /// </summary>
    public LampColours currentEnumColour;

    private int colourCount = 0;

    /// <summary>
    /// Hard coded list of available colours TODO: unhardcode
    /// </summary>
    public enum LampColours {  WHITE = 0, RED, GREEN, BLUE, OFF};

    /// <summary>
    /// List of shard colours available to the lamp
    /// </summary>
    List<LampColours> availableColours;

    private Dictionary<LampColours, Color> colourMap;

    /// <summary>
    /// List of our available colours
    /// </summary>

    public LampUI lampUI;

    // Start is called before the first frame update
    void Start()
    {
        // Save base rotation
        baseRotation = transform.rotation;
        originalZ = baseRotation.eulerAngles.z;

        currentDeflection = 0;

        Debug.Assert(lampColours.Count > 0);
        Debug.Assert(lampColours.Count == lampColourKeys.Count);

        colourMap = new Dictionary<LampColours, Color>();
        for (int i=0; i < lampColours.Count; i++)
        {
            colourMap.Add(lampColourKeys[i], lampColours[i]);
        }
        colourMap.Add(LampColours.OFF, Color.black);
        availableColours = new List<LampColours>();
        availableColours.Add(LampColours.OFF);
        SetLampColours();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddColour(LampColours colour)
    {
        Debug.Assert(colourMap.ContainsKey(colour));
        
        if (!availableColours.Contains(colour))
        {
            availableColours.Add(colour);
            lampUI.SetShards(availableColours);
        }
    }

    public void nextColour()
    {
        colourCount = (colourCount + 1) % availableColours.Count;
        SetLampColours();
    }

    private void SetLampColours()
    {
        currentEnumColour = availableColours[colourCount];

        switch (currentEnumColour)
        {
            case LampColours.OFF:
                spotLight.enabled = false;
                areaLight.enabled = false;
                break;

            default:
                // Assume we have a colour
                spotLight.enabled = true;
                areaLight.enabled = true;
                currentColour = colourMap[currentEnumColour];
                spotLight.color = CurrentColour;
                areaLight.color = CurrentColour;
                break;
        }

        lampUI.SetCurrentLampColour(currentEnumColour);
    }

    /// <summary>
    /// Move the lamp upwards / downwards on analogue scale
    /// </summary>
    /// <param name="deflection">Direction to deflect lamp - range -1 to 1. Negative
    /// values move lamp downwards, positive up</param>
    /// <param name="deltaTime">how much of a second to move by - normally Time.deltaTime</param>
    /// <returns>Whether the lamp has reached max deflection</returns>
    public bool DeflectLamp(float deflection, float deltaTime)
    {
        Debug.Assert(deltaTime != 0);
        bool atLimit = false;

        float newDeflection = currentDeflection + (deflection * lampRotationSpeed * deltaTime);
        if (Mathf.Abs(newDeflection) > maxDeflection)
        {
            newDeflection = newDeflection > maxDeflection ? maxDeflection : -maxDeflection;
            atLimit = true;
        }

        float deflectionDelta = newDeflection - currentDeflection;
        currentDeflection += deflectionDelta;
        transform.Rotate(Vector3.forward, deflectionDelta);

        // transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.parent.rotation.z +  originalZ + currentDeflection);

        return atLimit;
    }
}
