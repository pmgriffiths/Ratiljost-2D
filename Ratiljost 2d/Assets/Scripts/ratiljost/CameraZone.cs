using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public Vector3 targetCameraOffsetForward;

    public Vector3 targetCameraOffsetBack;

    public Vector2 forwardMaxDeviation;

    public Vector2 reverseMaxDeviation;

    public float cameraSize;

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player2d player = other.gameObject.GetComponent<Player2d>();
            Debug.Assert(player != null);

            if (player.facingDirection == Trackable.Direction.Forwards)
            {
                player.boxedFollowCam.ChangeOffsets(targetCameraOffsetForward, forwardMaxDeviation);
            }
            else
            {
                player.boxedFollowCam.ChangeOffsets(targetCameraOffsetBack, reverseMaxDeviation);
            }
        }
    }
}
