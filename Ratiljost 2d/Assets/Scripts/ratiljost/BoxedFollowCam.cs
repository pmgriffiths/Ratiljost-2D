using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Follow a gameobject but stays confined within a box defined
/// by colliders in specified layer
/// </summary>
public class BoxedFollowCam : MonoBehaviour
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
    /// Vector that the camera moves along
    /// </summary>
    public Vector3 trackingVector = Vector3.right;

    /// <summary>
    /// Direction the camera looks
    /// </summary>
    public Vector3 cameraOrientation = Vector3.zero;

    /// <summary>
    /// How far the tracked object can move before camera tracks
    /// </summary>
    public float maxDeviation = 0.1f;

    public float maxVerticalDeviation = 0.5f;

    /// <summary>
    /// How far we're from tracking the object
    /// </summary>
    public float currentDeviation;

    public float currentVerticalDeviation;

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

    public float targetXOffset = 4f;

    public float targetZOffset = -6;

    public bool allowYTracking = false;

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
    /// Time to stop camera position recovery
    /// </summary>
    private float endRecoveryTime;

    // Start is called before the first frame update
    void Start()
    {
        // transform.rotation = Quaternion.Euler(cameraOrientation);
        trackedDirection = trackedObject.facingDirection;
        transform.position = FindIdealPostion();
        
    }


    // Update is called once per frame
    void Update()
    {
/*        // Only care about deviation in tracking direction
        Vector3 trackedScalePos = Vector3.Scale(trackedObject.transform.position, trackingVector);

        // Find ideal position
        if (trackedDirection != trackedObject.facingDirection)
        {
            // Target has changed direction this update - start recovery
            inFlipRecovery = true;
            trackedDirection = trackedObject.facingDirection;
            endFlipRecoveryTime = Time.time + flipRecoveryTime;
        }

        inFlipRecovery = inFlipRecovery && Time.time < endFlipRecoveryTime;

        float offset = trackedObject.facingDirection == Trackable.Direction.Forwards ? targetOffset : -targetOffset;
        Vector3 idealPos = trackedScalePos + targetOffsetVector + (trackingVector * offset); */
        Vector3 idealPos = FindIdealPostion();
        Vector3 offsetFromIdeal = transform.position - idealPos;
        currentDeviation = offsetFromIdeal.magnitude;

        currentVerticalDeviation = Mathf.Abs(offsetFromIdeal.y - idealPos.y);

        //        Debug.Log("track: " + trackedScalePos + "cur: " + transform.position + " ideal: " + idealPos + " offset: " + offsetFromIdeal + " curDev: " + currentDeviation);

        inFlipRecovery = inFlipRecovery && Time.time < endRecoveryTime;
        inCameraZoneRecovery = inCameraZoneRecovery && Time.time < endRecoveryTime;

        if (currentDeviation > maxDeviation || currentVerticalDeviation > maxVerticalDeviation || inFlipRecovery || inCameraZoneRecovery)
        {
            // TODO: fix this for any vector, not just x

            float offsetX = idealPos.x - transform.position.x;
            float offsetY = idealPos.y - transform.position.y;

            // Debug.Log("offset x: " + offsetX + " offset y: " + offsetY);
            float moveSpeed = inFlipRecovery ? recoverySpeed : maxSpeed;

            // Check for allowed movement
            if (allowedMovement == MoveDirections.All ||
                (allowedMovement == MoveDirections.Positive && offsetX < 0) ||
                (allowedMovement == MoveDirections.Negative && offsetX > 0))
            {
                transform.position = Vector3.MoveTowards(transform.position, idealPos, moveSpeed * Time.deltaTime);
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

        float offset = trackedObject.facingDirection == Trackable.Direction.Forwards ? targetXOffset : -targetXOffset;
        Vector3 idealPos = trackedScalePos + targetOffsetVector + (trackingVector * offset);
        if (allowYTracking)
        {
            idealPos.y = trackedObject.transform.position.y + targetOffsetVector.y;
        }
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

    public void ChangeOffsets(float newY, float newZ, float newDeviation, float newOffset)
    {
        Debug.Log("Moving targer new offset:" + newOffset + "dev: " + newDeviation + " y: " + newY + " z: " + newZ);
        targetOffsetVector.y = newY;
        targetOffsetVector.z = newZ;
        targetXOffset = newOffset;
        maxDeviation = newDeviation;

        inCameraZoneRecovery = true;
        endRecoveryTime = Time.time + recoveryTime;
    }

}
