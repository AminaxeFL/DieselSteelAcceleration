using UnityEngine;

public class EngineLoop : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private WheelSpinner car;

    public float volume = 1f;
    public float gainFromSpeed = 1.5f;
    public float speedForMaxGain = 40f;
    public float volumeSmoothTime = 0.3f;
    public float pitchGain = 0.4f;
    public float pitchSmoothTime = 0.1f;
    [SerializeField] private bool persistAcrossScenes = true;

    private float volumeVelocity;
    private float pitchVelocity;
    private float fuelFadeTimer;
    private bool wasRunning;

    private void Awake()
    {
        if (persistAcrossScenes)
            DontDestroyOnLoad(gameObject);

        SetupSource();
        Play();
        source.volume = 0f;
    }

    private void SetupSource()
    {
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.pitch = 1f;
    }

    public void Play()
    {
        SetupSource();
        if (!source.isPlaying)
            source.Play();
    }

    private float CurrentSpeed()
    {
        return car.Chassis.linearVelocity.magnitude;
    }

    private void Update()
    {
        if (!source.isPlaying)
            source.Play();

        float t = Mathf.Clamp01(CurrentSpeed() / Mathf.Max(speedForMaxGain, 0.01f));
        float peak = Mathf.Clamp01(volume);
        float gain = Mathf.Max(gainFromSpeed, 0f);
        float idle = peak / (1f + gain);

        bool hasFuel = car.Fuel01 > 0.001f;
        bool running = hasFuel && car.canGo;

        if (running)
        {
            fuelFadeTimer = 0f;
            wasRunning = true;

            source.volume = Mathf.SmoothDamp(
                source.volume,
                Mathf.Lerp(idle, peak, t),
                ref volumeVelocity,
                Mathf.Max(volumeSmoothTime, 0.01f));

            source.pitch = Mathf.SmoothDamp(
                source.pitch,
                1f + Mathf.Max(pitchGain, 0f) * t,
                ref pitchVelocity,
                Mathf.Max(pitchSmoothTime, 0.01f));
        }
        else
        {
            if (wasRunning)
            {
                fuelFadeTimer = 0f;
                wasRunning = false;
            }

            fuelFadeTimer += Time.deltaTime;
            float fadeTime = 3f;
            float fade = 1f - Mathf.Clamp01(fuelFadeTimer / fadeTime);

            source.volume = Mathf.SmoothDamp(
                source.volume,
                Mathf.Lerp(0f, Mathf.Lerp(idle, peak, t), fade),
                ref volumeVelocity,
                Mathf.Max(volumeSmoothTime, 0.01f));

            source.pitch = Mathf.SmoothDamp(
                source.pitch,
                1f + Mathf.Max(pitchGain, 0f) * t * fade,
                ref pitchVelocity,
                Mathf.Max(pitchSmoothTime, 0.01f));

            if (fade <= 0f && source.volume < 0.01f)
                source.Pause();
        }
    }
}
