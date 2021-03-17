using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeadZone : MonoBehaviour
{
    public GameObject deathPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            deathPanel.SetActive(true);
            Player2d player = other.gameObject.GetComponent<Player2d>();
            Debug.Assert(player != null);
            player.dead = true;
        }

    }
}
