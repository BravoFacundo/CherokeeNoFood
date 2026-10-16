using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using TMPro;

// Menu navigation: screens with back history, a background gradient per screen and shared screen info (title, back).
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    // None = no gradient, the scene behind is visible. Add new gradients at the end
    public enum GradientType { None, Day, Sunset, Night, Neon, Comic, Black }

    [System.Serializable]
    public class ScreenData
    {
        [FormerlySerializedAs("page")]
        public GameObject screen;
        [FormerlySerializedAs("menuGradient")]
        public GradientType gradient;
        [Tooltip("Shows the shared screen info (dividers, title, back button)")]
        [FormerlySerializedAs("showFrame")]
        public bool showInfo = true;
        public string title;
        [Tooltip("Fork fade variant that fits the title length. Empty = keep current.")]
        public Sprite titleFork;
    }

    [System.Serializable]
    public class GradientEntry
    {
        public GradientType type;
        [Tooltip("Empty = solid color (e.g. Black)")]
        public Sprite sprite;
        public Color color = Color.white;
    }

    [Header("Screens")]
    [FormerlySerializedAs("firstPage"), FormerlySerializedAs("firstMenuPage")]
    [SerializeField] private GameObject firstScreen;
    [Tooltip("Screen shown in overlay mode (pause)")]
    [SerializeField] private GameObject overlayScreen;
    [FormerlySerializedAs("pages")]
    [SerializeField] private List<ScreenData> screens = new();

    [Header("Screens Info (shared by all screens)")]
    [FormerlySerializedAs("pageFrame")]
    [SerializeField] private GameObject screensInfo;
    [FormerlySerializedAs("pageTitle")]
    [SerializeField] private TMP_Text screenTitle;
    [Tooltip("Fork fade image inside Divider_Top")]
    [SerializeField] private Image titleForkImage;

    [Header("Screen Fader")]
    [Tooltip("Optional (Screens_Fade). Black overlay that fades in on start and out before leaving. Can be disabled in the editor.")]
    [SerializeField] private ScreenFader screenFader;

    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Development";
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Gradient")]
    [SerializeField] private Image gradientImage;
    [SerializeField] private float gradientFadeDuration = 0.5f;
    [Tooltip("Each gradient, assigned once")]
    [SerializeField] private List<GradientEntry> gradients = new();

    [Header("Debug")]
    [SerializeField] private GameObject currentScreen;
    private readonly Stack<GameObject> screenHistory = new();
    private Image gradientFadeImage;
    private Coroutine gradientRoutine;
    private GradientType currentGradient = GradientType.None;
    private GradientType? gradientOverride; // Fixed gradient while shown as overlay (pause = stage gradient)

    // Outside the Menu scene the canvas works as the pause overlay: hidden at start, Escape handled by PauseController
    private bool startHidden;
    public event System.Action ResumeRequested;

    public bool IsOpen => currentScreen != null;
    public bool CanGoBack => screenHistory.Count > 0;

    //---------- LIFECYCLE ------------------------------------------------------------------------------------------------------------//

    private void Awake()
    {
        // Only one screens canvas: the one from the scene that just loaded wins (removes any leftover from the previous scene)
        if (instance != null && instance != this) Destroy(instance.gameObject);
        instance = this;
        startHidden = SceneManager.GetActiveScene().name != menuSceneName;

        // Objects can stay disabled in the editor; they are turned on here and stay invisible until used
        if (screenFader != null) screenFader.gameObject.SetActive(true);
        gradientImage.gameObject.SetActive(true); // Screens_Background (alpha 0 below)
        foreach (ScreenData entry in screens)
            if (entry.screen.transform.parent != null) entry.screen.transform.parent.gameObject.SetActive(true); // Screens parent

        // Second image used to crossfade between gradients
        gradientFadeImage = Instantiate(gradientImage, gradientImage.transform.parent);
        gradientFadeImage.name = gradientImage.name + "_Fade"; // e.g. Screens_Background_Fade
        gradientFadeImage.transform.SetSiblingIndex(gradientImage.transform.GetSiblingIndex() + 1);
        SetImageAlpha(gradientFadeImage, 0f);
        SetImageAlpha(gradientImage, 0f);
    }

    private void Start()
    {
        foreach (ScreenData entry in screens) entry.screen.SetActive(false);
        if (startHidden) HideAll();
        else ShowScreen(firstScreen, true); // No fade on the first screen
        if (screenFader != null) screenFader.FadeIn();
    }

    private void Update()
    {
        if (startHidden) return; // The owner handles Escape
        if (screenFader != null && screenFader.IsFading) return;
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
    }

    //---------- NAVIGATION (button events) -------------------------------------------------------------------------------------------//

    public void OpenScreen(GameObject screen)
    {
        if (currentScreen != null)
        {
            screenHistory.Push(currentScreen);
            currentScreen.SetActive(false);
        }
        ShowScreen(screen);
    }

    public void Back()
    {
        if (screenHistory.Count == 0) return;
        currentScreen.SetActive(false);
        ShowScreen(screenHistory.Pop());
    }

    // Overlay: show the overlay screen (pause) with a fixed gradient
    public void ShowOverlay(GradientType gradient) => Show(overlayScreen, gradient);

    // Overlay: open a screen with a fixed gradient (e.g. pause with the stage gradient)
    public void Show(GameObject screen, GradientType gradient)
    {
        gradientOverride = gradient;
        screenHistory.Clear();
        if (currentScreen != null) currentScreen.SetActive(false);
        ShowScreen(screen);
    }

    // Overlay: close everything
    public void Hide()
    {
        if (currentScreen != null) currentScreen.SetActive(false);
        HideAll();
    }

    private void HideAll()
    {
        currentScreen = null;
        gradientOverride = null;
        screenHistory.Clear();
        if (screensInfo != null) screensInfo.SetActive(false);
        SetGradientImmediate(GradientType.None);
    }

    // Button event (Continue playing): PauseController listens and resumes the game
    public void ResumeGame() => ResumeRequested?.Invoke();

    public void StartGame() => LeaveAfterFade(() => SceneManager.LoadScene(gameSceneName));
    public void GoToMenu() => LeaveAfterFade(() => SceneManager.LoadScene(menuSceneName));
    public void ExitGame() => LeaveAfterFade(QuitApplication);

    private void LeaveAfterFade(System.Action leave)
    {
        if (screenFader != null) screenFader.FadeOut(leave);
        else leave();
    }

    private void QuitApplication()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ShowScreen(GameObject screen, bool instant = false)
    {
        screen.SetActive(true);
        currentScreen = screen;

        ScreenData entry = screens.FirstOrDefault(s => s.screen == screen);
        GradientType gradient = gradientOverride ?? (entry != null ? entry.gradient : GradientType.None);
        if (instant) SetGradientImmediate(gradient);
        else FadeGradient(gradient);
        UpdateScreensInfo(entry);

        RebuildLayout(screen);
        if (screensInfo != null && screensInfo.activeSelf) RebuildLayout(screensInfo);
    }

    // Layout groups normally update at the end of the frame; force it so the screen never shows misplaced for a frame
    private void RebuildLayout(GameObject target)
    {
        Canvas.ForceUpdateCanvases(); // Lets TMP texts report their size first
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)target.transform);
    }

    private void UpdateScreensInfo(ScreenData entry)
    {
        if (screensInfo == null) return;
        bool show = entry != null && entry.showInfo;
        screensInfo.SetActive(show);
        if (!show) return;
        if (screenTitle != null) screenTitle.text = entry.title;
        if (titleForkImage != null && entry.titleFork != null) titleForkImage.sprite = entry.titleFork;
    }

    //---------- GRADIENT -------------------------------------------------------------------------------------------------------------//

    private void FadeGradient(GradientType type)
    {
        if (type == currentGradient) return;

        GradientEntry entry = null;
        if (type != GradientType.None)
        {
            entry = gradients.FirstOrDefault(g => g.type == type);
            if (entry == null)
            {
                Debug.LogWarning($"Gradient {type} is not in the gradients list.");
                return;
            }
        }

        currentGradient = type;
        if (gradientRoutine != null) StopCoroutine(gradientRoutine);
        gradientRoutine = StartCoroutine(entry != null ? FadeInGradient(entry) : FadeOutGradient());
    }

    private void SetGradientImmediate(GradientType type)
    {
        if (gradientRoutine != null) StopCoroutine(gradientRoutine);
        gradientRoutine = null;
        currentGradient = type;
        SetImageAlpha(gradientFadeImage, 0f);

        GradientEntry entry = gradients.FirstOrDefault(g => g.type == type);
        if (type == GradientType.None || entry == null)
        {
            SetImageAlpha(gradientImage, 0f);
            return;
        }
        ApplyGradient(gradientImage, entry);
        SetImageAlpha(gradientImage, 1f);
    }

    // Crossfade: the fade image appears over the current gradient, then replaces it
    private IEnumerator FadeInGradient(GradientEntry entry)
    {
        ApplyGradient(gradientFadeImage, entry);
        yield return FadeAlpha(gradientFadeImage, 0f, 1f);

        ApplyGradient(gradientImage, entry);
        SetImageAlpha(gradientImage, 1f);
        SetImageAlpha(gradientFadeImage, 0f);
        gradientRoutine = null;
    }

    // None: the current gradient disappears and the scene behind is visible
    private IEnumerator FadeOutGradient()
    {
        SetImageAlpha(gradientFadeImage, 0f);
        yield return FadeAlpha(gradientImage, gradientImage.color.a, 0f);
        gradientRoutine = null;
    }

    private void ApplyGradient(Image image, GradientEntry entry)
    {
        image.sprite = entry.sprite;
        Color color = entry.color;
        color.a = image.color.a; // Keep current alpha, fades control it
        image.color = color;
    }

    private IEnumerator FadeAlpha(Image image, float from, float to)
    {
        float elapsedTime = 0f;
        while (elapsedTime < gradientFadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            SetImageAlpha(image, Mathf.Lerp(from, to, elapsedTime / gradientFadeDuration));
            yield return null;
        }
        SetImageAlpha(image, to);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}
