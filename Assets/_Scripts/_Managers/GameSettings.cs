using System;
using UnityEngine;

// Centralized player settings, saved with PlayerPrefs.
// Sliders change values live; consumers subscribe to Changed to apply them.
public static class GameSettings
{
    public static event Action Changed;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string SensitivityKey = "Sensitivity";

    public static float MasterVolume { get; private set; }
    public static float MusicVolume { get; private set; }
    public static float SfxVolume { get; private set; }
    public static float Sensitivity { get; private set; } // Multiplier, 1 = Inspector value

    private static float[] snapshot;

    static GameSettings() => Load();

    public static void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        Sensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        Apply();
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.SetFloat(SensitivityKey, Sensitivity);
        PlayerPrefs.Save();
    }

    // Cancel support: remember values when the configuration page opens
    public static void TakeSnapshot() => snapshot = new[] { MasterVolume, MusicVolume, SfxVolume, Sensitivity };
    public static void RestoreSnapshot()
    {
        if (snapshot == null) return;
        MasterVolume = snapshot[0];
        MusicVolume = snapshot[1];
        SfxVolume = snapshot[2];
        Sensitivity = snapshot[3];
        Apply();
    }

    public static void SetMasterVolume(float value) { MasterVolume = value; Apply(); }
    public static void SetMusicVolume(float value) { MusicVolume = value; Apply(); }
    public static void SetSfxVolume(float value) { SfxVolume = value; Apply(); }
    public static void SetSensitivity(float value) { Sensitivity = value; Apply(); }

    private static void Apply()
    {
        AudioListener.volume = MasterVolume; // Music/SFX split needs an AudioMixer (pending)
        Changed?.Invoke();
    }
}
