using System.Collections.Generic;
using UnityEngine;

// Game mode screen: the selected mode stays lit and is sent to GameManager. Start uses UIManager.StartGame.
public class ModeSelectorScreen : MonoBehaviour
{
    [System.Serializable]
    public class ModeOption
    {
        public ButtonHover button;
        [Tooltip("0 = Story, 1 = Endless (same order as GameManager.GameMode)")]
        public int mode;
    }

    [Header("Modes")]
    [SerializeField] private List<ModeOption> modes = new();

    // Screen shown: light the mode that is currently selected
    private void OnEnable() => Highlight(GameManager.instance.CurrentGameMode);

    // Button event (each mode button)
    public void SelectMode(int mode)
    {
        GameManager.instance.SetGameMode(mode);
        Highlight(mode);
    }

    private void Highlight(int mode)
    {
        foreach (ModeOption option in modes)
            if (option.button != null) option.button.SetLocked(option.mode == mode);
    }
}
