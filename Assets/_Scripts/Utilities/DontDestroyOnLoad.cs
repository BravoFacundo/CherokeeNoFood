using UnityEngine;

// Deprecated: nothing should persist between scenes except GameManager (it handles itself).
// Each scene has its own screens canvas and HUD. This component no longer persists anything:
// it only warns which objects still have it, so it can be removed. Then delete this script.
public class DontDestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        Debug.LogWarning($"[DontDestroyOnLoad] '{name}' still has this deprecated component. Remove it (click this message to select the object).", this);
    }
}
