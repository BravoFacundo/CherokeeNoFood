using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Fades the button text into a neon glow preset while hovered or selected.
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("References")]
    [Tooltip("Empty = first TMP text in children")]
    [SerializeField] private TMP_Text text;
    [Tooltip("TMP material preset with Glow enabled")]
    [SerializeField] private Material hoverMaterial;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Material normalMaterial;
    private Material fadeMaterial; // Instance animated between off and hover values
    private Selectable selectable;
    private Coroutine fadeRoutine;
    private float currentLevel;

    // Hover values (from the preset) and off values (glow invisible)
    private Color glowOn, glowOff, underlayOn, underlayOff;
    private float dilateOn, dilateOff;

    private void Awake()
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>();
        normalMaterial = text.fontSharedMaterial;
        selectable = GetComponent<Selectable>();
        if (hoverMaterial == null) return;

        fadeMaterial = new Material(hoverMaterial) { name = hoverMaterial.name + " (Fade)" };

        glowOn = GetColor(ShaderUtilities.ID_GlowColor);
        glowOff = new Color(glowOn.r, glowOn.g, glowOn.b, 0f);
        underlayOn = GetColor(ShaderUtilities.ID_UnderlayColor);
        underlayOff = new Color(underlayOn.r, underlayOn.g, underlayOn.b, 0f);
        dilateOn = GetFloat(fadeMaterial, ShaderUtilities.ID_FaceDilate);
        dilateOff = GetFloat(normalMaterial, ShaderUtilities.ID_FaceDilate);
    }

    // Reset instantly when the page is hidden, so no button stays lit
    private void OnDisable()
    {
        fadeRoutine = null;
        SetLevel(0f);
    }

    private void OnDestroy()
    {
        if (fadeMaterial != null) Destroy(fadeMaterial);
    }

    public void OnPointerEnter(PointerEventData eventData) => SetHover(true);
    public void OnPointerExit(PointerEventData eventData) => SetHover(false);
    public void OnSelect(BaseEventData eventData) => SetHover(true);     // Keyboard/gamepad navigation
    public void OnDeselect(BaseEventData eventData) => SetHover(false);

    private void SetHover(bool isHovered)
    {
        if (fadeMaterial == null || !isActiveAndEnabled) return;
        if (isHovered && selectable != null && !selectable.interactable) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(isHovered ? 1f : 0f, isHovered ? fadeInDuration : fadeOutDuration));
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = currentLevel;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            SetLevel(Mathf.Lerp(start, target, elapsedTime / duration));
            yield return null;
        }
        SetLevel(target);
        fadeRoutine = null;
    }

    // 0 = normal material, 1 = full hover glow
    private void SetLevel(float level)
    {
        currentLevel = level;
        if (fadeMaterial == null) return;

        if (level <= 0f)
        {
            text.fontSharedMaterial = normalMaterial; // Back to the shared material when idle
            return;
        }

        SetColor(ShaderUtilities.ID_GlowColor, Color.Lerp(glowOff, glowOn, level));
        SetColor(ShaderUtilities.ID_UnderlayColor, Color.Lerp(underlayOff, underlayOn, level));
        if (fadeMaterial.HasProperty(ShaderUtilities.ID_FaceDilate))
            fadeMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, Mathf.Lerp(dilateOff, dilateOn, level));

        if (text.fontSharedMaterial != fadeMaterial) text.fontSharedMaterial = fadeMaterial;
    }

    private Color GetColor(int id) => fadeMaterial.HasProperty(id) ? fadeMaterial.GetColor(id) : Color.clear;
    private void SetColor(int id, Color color) { if (fadeMaterial.HasProperty(id)) fadeMaterial.SetColor(id, color); }
    private float GetFloat(Material material, int id) => material.HasProperty(id) ? material.GetFloat(id) : 0f;
}
