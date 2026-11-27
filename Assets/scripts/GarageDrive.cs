using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GarageDrive : MonoBehaviour
{
    public static bool EngineStarted { get; private set; }

    [SerializeField] private GameObject garageCollisionEntry;
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private float hudFadeDuration = 0.5f;
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private float transitionDelay = 2f;

    private bool started;

    private void Awake()
    {
        EngineStarted = false;
    }

    private void OnDestroy()
    {
        EngineStarted = false;
    }

    public void StartDrive()
    {
        if (started)
            return;

        started = true;
        EngineStarted = true;
        Destroy(garageCollisionEntry);
        StartCoroutine(FadeHud());
        StartCoroutine(TransitionToMainScene());
    }

    private IEnumerator TransitionToMainScene()
    {
        yield return new WaitForSeconds(transitionDelay);
        sceneLoader.PerviousScene();
    }

    private IEnumerator FadeHud()
    {
        foreach (SpriteButton button in hudRoot.GetComponentsInChildren<SpriteButton>(true))
            button.enabled = false;

        SpriteRenderer[] sprites = hudRoot.GetComponentsInChildren<SpriteRenderer>(true);
        TMP_Text[] texts = hudRoot.GetComponentsInChildren<TMP_Text>(true);
        Graphic[] graphics = hudRoot.GetComponentsInChildren<Graphic>(true);
        Light2D[] lights = hudRoot.GetComponentsInChildren<Light2D>(true);

        float[] spriteAlphas = new float[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            spriteAlphas[i] = sprites[i].color.a;

        float[] textAlphas = new float[texts.Length];
        for (int i = 0; i < texts.Length; i++)
            textAlphas[i] = texts[i].color.a;

        float[] graphicAlphas = new float[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
            graphicAlphas[i] = graphics[i].color.a;

        float[] lightIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
            lightIntensities[i] = lights[i].intensity;

        float elapsed = 0f;
        float duration = Mathf.Max(hudFadeDuration, 0.01f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < sprites.Length; i++)
            {
                Color c = sprites[i].color;
                c.a = spriteAlphas[i] * fade;
                sprites[i].color = c;
            }

            for (int i = 0; i < texts.Length; i++)
            {
                Color c = texts[i].color;
                c.a = textAlphas[i] * fade;
                texts[i].color = c;
            }

            for (int i = 0; i < graphics.Length; i++)
            {
                Color c = graphics[i].color;
                c.a = graphicAlphas[i] * fade;
                graphics[i].color = c;
            }

            for (int i = 0; i < lights.Length; i++)
                lights[i].intensity = lightIntensities[i] * fade;

            yield return null;
        }
    }
}
