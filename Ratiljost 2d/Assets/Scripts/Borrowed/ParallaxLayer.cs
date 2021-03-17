using UnityEngine;

/// <summary>
/// Parallax Layer script taken from Unity Lost Crypy Sample Project
/// ‘Lost Crypt - 2D Sample Project | Tutorial Projects | Unity Asset Store’. 2021. [online]. Available at: https://assetstore.unity.com/packages/essentials/tutorial-projects/lost-crypt-2d-sample-project-158673?_ga=2.129466251.609044901.1615473115-1547705872.1602246764 [accessed 13 Mar 2021].
/// </summary>

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] float multiplier = 0.0f;
    [SerializeField] bool horizontalOnly = true;

    private Transform cameraTransform;

    private Vector3 startCameraPos;
    private Vector3 startPos;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        startCameraPos = cameraTransform.position;
        startPos = transform.position;
    }


    private void LateUpdate()
    {
        var position = startPos;
        if (horizontalOnly)
            position.x += multiplier * (cameraTransform.position.x - startCameraPos.x);
        else
            position += multiplier * (cameraTransform.position - startCameraPos);

        transform.position = position;
    }

}

