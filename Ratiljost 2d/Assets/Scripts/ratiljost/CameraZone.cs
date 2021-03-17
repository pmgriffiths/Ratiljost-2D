using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public float forwardYPosition;

    public float forwardZPosition;

    public float forwardMaxDeviation;

    public float forwardOffset;

    public float reverseYPosition;

    public float reverseZPosition;

    public float reverseMaxDeviation;

    public float reverseOffset;

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player2d player = other.gameObject.GetComponent<Player2d>();
            Debug.Assert(player != null);

            if (player.facingDirection == Trackable.Direction.Forwards)
            {
                player.boxedFollowCam.ChangeOffsets(forwardYPosition, forwardZPosition, forwardMaxDeviation, forwardOffset);
            } else
            {
                player.boxedFollowCam.ChangeOffsets(reverseYPosition, reverseZPosition, reverseMaxDeviation, reverseOffset);
            }
        }
    }
}
