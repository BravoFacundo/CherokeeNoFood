using System.Collections.Generic;
using UnityEngine;

// Game scene pause: Escape or P opens the pause screen over the game with the stage gradient.
// Uses the screens canvas prefab placed in the scene (it starts hidden outside the Menu scene).
public class PauseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;

    [Header("Stage Gradients")]
    [Tooltip("Gradient behind the pause for each stage (index = stage)")]
    [SerializeField] private List<UIManager.GradientType> stageGradients = new()
    {
        UIManager.GradientType.Day, UIManager.GradientType.Sunset, UIManager.GradientType.Night, UIManager.GradientType.Night
    };

    public bool IsPaused { get; private set; }
    private UIManager uiManager;

    private void Start()
    {
        uiManager = UIManager.instance;
        if (uiManager == null)
        {
            Debug.LogWarning("PauseController: no screens canvas in the scene, pause disabled.");
            return;
        }
        uiManager.ResumeRequested += Resume; // Continue button inside the canvas
    }

    private void OnDestroy()
    {
        if (uiManager != null) uiManager.ResumeRequested -= Resume;
    }

    private void Update()
    {
        if (uiManager == null) return;
        if (!Input.GetKeyDown(KeyCode.Escape) && !Input.GetKeyDown(KeyCode.P)) return;

        if (!IsPaused) Pause();
        else if (uiManager.CanGoBack) uiManager.Back(); // e.g. Configuration > Pause
        else Resume();
    }

    public void Pause()
    {
        IsPaused = true;
        GameManager.instance.SetGameState("Pause", true);
        uiManager.ShowOverlay(GetStageGradient());
    }

    public void Resume()
    {
        IsPaused = false;
        uiManager.Hide();
        GameManager.instance.SetGameState("Pause", false);
    }

    private UIManager.GradientType GetStageGradient()
    {
        int stage = levelManager != null ? levelManager.CurrentLevel : 0;
        return stage < stageGradients.Count ? stageGradients[stage] : UIManager.GradientType.None;
    }
}
