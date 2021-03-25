using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Follow a gameobject but stays confined within a box defined
/// by colliders in specified layer
/// </summary>
public class BoxedFollowCam2d : MonoBehaviour
{

    /// <summary>
    /// Object to track
    /// </summary>
    public Trackable trackedObject;

    /// <summary>
    /// Maximum speed that camera tracks object
    /// </summary>
    public float maxSpeed = 5f;

    /// <summary>
    /// Vector that the camera moves along left/right
    /// </summary>
    private Vector3 trackingVector = new Vector3(1, 1, 0);

    /// <summary>
    /// Vector that the camera moves along up/down
    /// </summary>
    private Vector3 trackingVectorY = Vector3.up;

    /// <summary>
    /// Direction the camera looks
    /// </summary>
    public Vector3 cameraOrientation = Vector3.zero;

    /// <summary>
    /// How far the tracked object can move before camera tracks
    /// </summary>
    public Vector2 maxDeviation;

    /// <summary>
    /// Constraint max height for camera
    /// </summary>
    public float maxY;

    /// <summary>
    /// Constrain min Y for camera
    /// </summary>
    public float minY;

    /// <summary>
    /// How far we're from tracking the object
    /// </summary>
    public float currentXDev;

    public float currentYDev;

    /// <summary>
    ///  How we stop camera moving beyond bounds
    /// </summary>
    private enum MoveDirections{ All, Negative, Positive };

    /// <summary>
    /// Allowable movement
    /// </summary>
    private MoveDirections allowedMovement = MoveDirections.All;

    /// <summary>
    /// Position for camera to aim for in the facing direction of the tracked
    /// object
    /// </summary>
    public Vector3 targetOffsetVector = new Vector3(0f, 1f, -5f);

    /// <summary>
    /// Has the target flipped direction recently
    /// </summary>
    public bool inFlipRecovery;

    /// <summary>
    /// Have we changed camaera zones recently
    /// </summary>
    public bool inCameraZoneRecovery;

    /// <summary>
    /// How long to move to position after a flip or zone change
    /// </summary>
    public float recoveryTime = 2f;

    /// <summary>
    /// Move camera faster for flips + zone changes
    /// </summary>
    public float recoverySpeed = 5f;

    public Trackable.Direction trackedDirection;

    /// <summary>
    /// Target size for child cameras
    /// </summary>
    public float cameraSize;

    /// <summary>
    /// How fast the cameras move to the target size;
    /// </summary>
    public float cameraSizeChangeFactor;

    /// <summary>
    /// Cameras controlled by the script
    /// </summary>
    public List<Camera> childCameras;

    /// <summary>
    /// Time to stop camera position recovery
    /// </summary>
    private float endRecoveryTime;

    // Start is called before the first frame update
    void Start()
    {
        // transform.rotation = Quaternion.Euler(cameraOrientation);
        trackedDirection = trackedObject.facingDirection;
        transform.position = FindIdealPostion();

        childCameras = new List<Camera>();
        foreach (Camera camera in GetComponentsInChildren<Camera>())
        {
            childCameras.Add(camera);
            camera.orthographicSize = cameraSize;
        }
    }


    // Update is called once per frame
    void Update()
    {

        Vector3 idealPos = FindIdealPostion();
        Vector3 offsetFromIdeal = transform.position - idealPos;
//        currentXDev = offsetFromIdeal.magnitude;
        currentXDev = offsetFromIdeal.x - idealPos.x;
        currentYDev = offsetFromIdeal.y - idealPos.y;

        Debug.Log("track: " + trackedObject.transform.position + "cur: " + transform.position + " ideal: " + idealPos + " offset: " + offsetFromIdeal + " curXDev: " + currentXDev + " curY: " + currentYDev);

        inFlipRecovery = inFlipRecovery && Time.time < endRecoveryTime;
        inCameraZoneRecovery = inCameraZoneRecovery && Time.time < endRecoveryTime;

        if (Mathf.Abs(currentXDev) > maxDeviation.x || Mathf.Abs(currentYDev) > maxDeviation.y || inFlipRecovery || inCameraZoneRecovery)
        {
            // TODO: fix this for any vector, not just x


            // Debug.Log("offset x: " + offsetX + " offset y: " + offsetY);
            float moveSpeed = inFlipRecovery ? recoverySpeed : maxSpeed;

            // Check for allowed movement
            if (allowedMovement == MoveDirections.All)
            {
                transform.position = Vector3.MoveTowards(transform.position, idealPos, moveSpeed * Time.deltaTime);
            }
        }

        // Migrate the cameras into position
        float currentCameraSize = childCameras[0].orthographicSize;
        if (!Mathf.Approximately(currentCameraSize, cameraSize))
        {
            float nextCameraSize = currentCameraSize - ((currentCameraSize - cameraSize) * Time.deltaTime * cameraSizeChangeFactor);
            foreach (Camera camera in childCameras)
            {
                camera.orthographicSize = nextCameraSize;
            }
        }
    }

    private Vector3 FindIdealPostion()
    {
        // Only care about deviation in tracking direction
        Vector3 trackedScalePos = Vector3.Scale(trackedObject.transform.position, trackingVector);

        // Find ideal position
        if (trackedDirection != trackedObject.facingDirection)
        {
            // Target has changed direction this update - start recovery
            inFlipRecovery = true;
            trackedDirection = trackedObject.facingDirection;
            endRecoveryTime = Time.time + recoveryTime;
        }

        inFlipRecovery = inFlipRecovery && Time.time < endRecoveryTime;

        
        float offsetX = trackedObject.facingDirection == Trackable.Direction.Forwards ? targetOffsetVector.x : -targetOffsetVector.x;
        float offsetY = targetOffsetVector.y;

        Vector3 idealPos = trackedScalePos;
        idealPos.x += offsetX;
        idealPos.y += offsetY;
        idealPos.z = targetOffsetVector.z;

        return idealPos;
    }


    /// <summary>
    /// Hit a bound - find direction and stop movement in that direction
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit barrier");
        Vector3 closestBound = Vector3.Scale(other.ClosestPoint(transform.position), trackingVector);
        Vector3 ourScalePos = Vector3.Scale(transform.position, trackingVector);

        if (ourScalePos.magnitude > closestBound.magnitude)
        {
            allowedMovement = MoveDirections.Negative;
        } else
        {
            allowedMovement = MoveDirections.Positive;

        }

    }

    /// <summary>
    /// Reset to allow all movemnet. NB. Assumes we havent moved through the bound
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Left barrier");
        allowedMovement = MoveDirections.All;
    }

    public void ChangeOffsets(Vector3 newTargetOffet, Vector2 newDeviation)
    {
        Debug.Log("Moving target new offset:" + newTargetOffet + "dev: " + newDeviation);
        targetOffsetVector = newTargetOffet;
        maxDeviation = newDeviation;

        inCameraZoneRecovery = true;
        endRecoveryTime = Time.time + recoveryTime;
    }

}
