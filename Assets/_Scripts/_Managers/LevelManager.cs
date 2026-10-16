using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private GameObject currentBackground;
    
    [Header("Configuration")]
    [SerializeField] private float transitionDuration;
    [SerializeField] private float windStrength;
    [SerializeField] private float windFrequency;
    [SerializeField] private List<Vector3> lightRotations;
    [SerializeField] private List<Color> lightColors;

    [Header("References")]
    [SerializeField] private Transform backgroundParent;
    [SerializeField] private Light directionalLight;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> backgroundsPrefabs;

    public int CurrentLevel => currentLevel;

    public float GetWindOffset()
    {
        return Mathf.Sin(Time.time * windFrequency) * windStrength;
    }

    private void Start()
    {
        currentBackground = backgroundParent.GetChild(0).gameObject;
        
        directionalLight.transform.eulerAngles = lightRotations[currentLevel];
        directionalLight.color = lightColors[currentLevel];

        //ChangeLevelBackground(3);
    }

    public void ChangeLevelBackground(int level)
    {
        level = Mathf.Clamp(level, 1, backgroundsPrefabs.Count) - 1;
        GameObject newScene = Instantiate(backgroundsPrefabs[level], currentBackground.transform.position, Quaternion.identity, backgroundParent);

        ObjectHanging[] objectsHanging = newScene.GetComponentsInChildren<ObjectHanging>();
        foreach (var _object in objectsHanging)
        {
            _object.levelManager = this;
        }

        SetAlpha(newScene, 0f);
        StartCoroutine(FadeInNewBackground(newScene, level));
    }
    private void SetAlpha(GameObject newBackground, float alpha)
    {
        SpriteRenderer[] sprites = newBackground.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in sprites)
        {
            Color color = sprite.color;
            color.a = alpha;
            sprite.color = color;
        }
    }
    private IEnumerator FadeInNewBackground(GameObject newBackground, int newLevel)
    {
        float elapsedTime = 0f;

        Vector3 initialRotation = directionalLight.transform.eulerAngles;
        Color initialColor = directionalLight.color;

        Vector3 targetRotation = lightRotations[newLevel];
        Color targetColor = lightColors[newLevel];

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            SetAlpha(newBackground, t);

            directionalLight.transform.eulerAngles = Vector3.Lerp(initialRotation, targetRotation, t);
            directionalLight.color = Color.Lerp(initialColor, targetColor, t);

            yield return null;
        }
        SetAlpha(newBackground, 1f);

        directionalLight.transform.eulerAngles = targetRotation;
        directionalLight.color = targetColor;

        Destroy(currentBackground);
        currentBackground = newBackground;
        currentLevel = newLevel;
    }
}
