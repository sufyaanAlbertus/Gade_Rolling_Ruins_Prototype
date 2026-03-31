using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    public Transform currentCheckpoint; // Last checkpoint reached
    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (currentCheckpoint == null)
        {
            currentCheckpoint = transform; // Start at player’s initial position
        }
    }

    public void Respawn()
    {
        if (characterController != null)
        {
            characterController.enabled = false; // Disable to move player
            transform.position = currentCheckpoint.position;
            characterController.enabled = true; // Re-enable after moving
        }
        else
        {
            transform.position = currentCheckpoint.position;
        }
    }
}