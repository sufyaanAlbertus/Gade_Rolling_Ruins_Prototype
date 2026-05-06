using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Default Spawn Point")]
    public Transform defaultSpawnPoint;

    [HideInInspector]
    public Transform currentCheckpoint;

    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // ALWAYS start at default spawn
        if (defaultSpawnPoint != null)
        {
            currentCheckpoint = defaultSpawnPoint;
            transform.position = defaultSpawnPoint.position;
        }
        else
        {
            currentCheckpoint = transform;
        }
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    public void LoseLife()
    {
        GameManager.Instance?.LoseLife();
    }

    public void Respawn()
    {
        Transform spawnPoint = currentCheckpoint != null
            ? currentCheckpoint
            : defaultSpawnPoint;

        if (spawnPoint == null)
        {
            Debug.LogWarning("No spawn point assigned!");
            return;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = spawnPoint.position;
            characterController.enabled = true;
        }
        else
        {
            transform.position = spawnPoint.position;
        }

        Debug.Log("Player respawned at: " + spawnPoint.name);
    }

    // 🔥 IMPORTANT FOR GAME RESTARTS
    public void ResetToDefaultSpawn()
    {
        currentCheckpoint = defaultSpawnPoint;

        if (defaultSpawnPoint != null)
        {
            transform.position = defaultSpawnPoint.position;
        }
    }
}