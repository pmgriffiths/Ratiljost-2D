using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerChange : MonoBehaviour
{
    int fromLayer;

    int toLayer;

    public string toLayerName;

    // Start is called before the first frame update
    void Start()
    {
        fromLayer = gameObject.layer;
        toLayer = LayerMask.NameToLayer(toLayerName);
   }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger enter " + other.tag);
        /// Check for light cone collisions
        if (other.gameObject.CompareTag("LightCone"))
        {
            Debug.Log("Layer change from " + gameObject.layer + " to " + toLayer);
            gameObject.layer = toLayer;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger exit " + other.tag);
        /// Check for light cone collisions
        if (other.gameObject.CompareTag("LightCone"))
        {
            Debug.Log("Layer change back from " + gameObject.layer + " to " + fromLayer);
            gameObject.layer = fromLayer;
        }
    }

}
