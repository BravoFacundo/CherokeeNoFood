using UnityEngine;
using UnityEngine.UI;

// Configuration screen: sliders preview settings live, Apply saves them,
// leaving the screen without applying (Back or Escape) restores the previous values.
public class SettingsScreen : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [Tooltip("Multiplier of the Inspector sensitivity (e.g. slider range 0.25 to 4)")]
    [SerializeField] private Slider sensitivitySlider;

    private bool applied;

    private void Awake()
    {
        if (masterVolumeSlider) masterVolumeSlider.onValueChanged.AddListener(GameSettings.SetMasterVolume);
        if (musicVolumeSlider) musicVolumeSlider.onValueChanged.AddListener(GameSettings.SetMusicVolume);
        if (sfxVolumeSlider) sfxVolumeSlider.onValueChanged.AddListener(GameSettings.SetSfxVolume);
        if (sensitivitySlider) sensitivitySlider.onValueChanged.AddListener(GameSettings.SetSensitivity);
    }

    // Screen shown: remember current values and show them on the sliders
    private void OnEnable()
    {
        applied = false;
        GameSettings.TakeSnapshot();
        if (masterVolumeSlider) masterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        if (musicVolumeSlider) musicVolumeSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
        if (sfxVolumeSlider) sfxVolumeSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
        if (sensitivitySlider) sensitivitySlider.SetValueWithoutNotify(GameSettings.Sensitivity);
    }

    // Screen hidden without applying: undo the live preview
    private void OnDisable()
    {
        if (!applied) GameSettings.RestoreSnapshot();
    }

    // Button event (Apply)
    public void Apply()
    {
        GameSettings.Save();
        applied = true;
        UIManager.instance.Back();
    }
}
