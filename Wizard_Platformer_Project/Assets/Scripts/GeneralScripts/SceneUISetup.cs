using UnityEngine;

/// <summary>
/// SceneUISetup: Attach this to any persistent manager GameObject 
/// in Advanced/Expert scenes (NOT Beginner — that scene already has 
/// the main menu so the full UIManager handles everything).
///
/// This lightweight script just ensures the HUD shows when the scene 
/// loads mid-game (player arrived via LevelExit) and resets properly.
///
/// You do NOT need this on the Beginner scene.
/// </summary>
public class SceneUISetup : MonoBehaviour
{
    [Header("Scene Info")]
    public string sceneName = "AdvancedLevel"; // just for debug logging

    private void Start()
    {
        // Small delay so UIManager.Start() runs first
        Invoke(nameof(SetupScene), 0.05f);
    }

    private void SetupScene()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning($"[SceneUISetup] {sceneName}: No GameManager found. " +
                             "Start from BeginnerLevel or add GameManager prefab.");
            return;
        }

        // Game is mid-run (came from LevelExit) — show HUD
        if (GameManager.Instance.IsRunning())
        {
            UIManager.Instance?.ShowPlayerUI();
            Debug.Log($"[SceneUISetup] {sceneName}: Game running — HUD shown.");
        }
    }
}
