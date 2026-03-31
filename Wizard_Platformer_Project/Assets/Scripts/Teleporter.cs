using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform teleportTarget; // Where player will be moved
    private bool canTeleport = true; // Prevent multiple teleports at once

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canTeleport)
        {
            // If player uses CharacterController, reset velocity if needed
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; // Stop any movement before teleport
            }

            other.transform.position = teleportTarget.position;
            canTeleport = false; // Disable until player exits the trigger
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTeleport = true; // Allow teleport again when player leaves
        }
    }
}