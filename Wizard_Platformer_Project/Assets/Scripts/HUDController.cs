using UnityEngine;

/// <summary>
/// HUDController: Now a thin relay — routes key collected calls to UIManager.
/// UIManager owns all Canvas references including the key icon.
/// Keep this script on any scene GameObject; LevelKey calls it via FindObjectOfType.
/// </summary>
public class HUDController : MonoBehaviour
{
    /// <summary>
    /// Called by LevelKey when the player collects the key.
    /// Delegates to UIManager which owns the key icon Image and sprites.
    /// </summary>
    public void ShowKeyCollected()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("[HUDController] UIManager not found.");
            return;
        }
        UIManager.Instance.ShowKeyCollected();
    }
}
