using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    private static readonly int SunAzElId = Shader.PropertyToID("_SkySunAzEl");

    public float hour = 9f;
    public bool advance = true;
    public float daySeconds = 180f;
    public float nightSeconds = 180f;
    public float dayHours = 12f;
    public float elevationSweep = 3.1415927f;
    public float azimuthSweep = 0f;
    public float azimuthStart = 0f;
    public float sunriseHour = 6f;
    public float smoothTime = 0.4f;

    [SerializeField] private Light2D globalLight;
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.18952958f, 0.18952957f, 0.23270428f, 1f);
    public float dayIntensity = 1f;
    public float nightIntensity = 0.1f;
    [SerializeField] private float dayLightLow = -0.02f;
    [SerializeField] private float dayLightHigh = 1f;

    private Color appliedColor;
    private float appliedIntensity;
    private bool appliedValid;
    private Vector2 current;
    private bool ready;

    public float Azimuth { get; private set; }
    public float Elevation { get; private set; }
    public float Phase => Mathf.Repeat(hour - sunriseHour, 24f);
    public float NightHours => Mathf.Max(24f - dayHours, 0.001f);
    public bool IsDay => Phase <= dayHours;
    public float CycleSeconds => daySeconds + nightSeconds;
    public float Daylight => Mathf.SmoothStep(dayLightLow, dayLightHigh, Mathf.Sin(Elevation));
    public float SunY => Mathf.Sin(Elevation);

    public Vector3 SunDirection
    {
        get
        {
            float ca = Mathf.Cos(Elevation);
            return new Vector3(ca * Mathf.Cos(Azimuth), Mathf.Sin(Elevation), ca * Mathf.Sin(Azimuth));
        }
    }

    public string Clock
    {
        get
        {
            int h = Mathf.FloorToInt(Mathf.Repeat(hour, 24f));
            int m = Mathf.FloorToInt(Mathf.Repeat(hour, 1f) * 60f);
            return string.Format("{0:00}:{1:00}", h, m);
        }
    }

    private void OnEnable()
    {
        ready = false;
        appliedValid = false;
    }

    private void Update()
    {
        if (advance && Application.isPlaying)
        {
            float rate = IsDay
                ? dayHours / daySeconds
                : NightHours / nightSeconds;

            hour = Mathf.Repeat(hour + rate * Time.deltaTime, 24f);
        }

        Evaluate(hour, out float targetAz, out float targetEl);

        if (!ready)
        {
            current = new Vector2(targetAz, targetEl);
            ready = true;
        }
        else if (smoothTime <= 0.0001f)
        {
            current = new Vector2(targetAz, targetEl);
        }
        else
        {
            float t = 1f - Mathf.Exp(-Time.deltaTime / smoothTime);

            float deltaAz = Mathf.DeltaAngle(current.x * Mathf.Rad2Deg, targetAz * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            float deltaEl = Mathf.DeltaAngle(current.y * Mathf.Rad2Deg, targetEl * Mathf.Rad2Deg) * Mathf.Deg2Rad;

            current.x += deltaAz * t;
            current.y += deltaEl * t;
        }

        Azimuth = current.x;
        Elevation = current.y;

        Shader.SetGlobalVector(SunAzElId, new Vector4(Azimuth, Elevation, Daylight, 1f));

        ApplyGlobalLight();
    }

    private void ApplyGlobalLight()
    {
        float daylight = Daylight;

        Color c = Color.Lerp(nightColor, dayColor, daylight);
        float intensity = Mathf.Lerp(nightIntensity, dayIntensity, daylight);

        if (appliedValid && Mathf.Abs(intensity - appliedIntensity) < 0.001f
            && (c - appliedColor).maxColorComponent < 0.001f)
            return;

        globalLight.color = c;
        globalLight.intensity = intensity;

        appliedColor = c;
        appliedIntensity = intensity;
        appliedValid = true;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(globalLight);
#endif
    }

    public void Evaluate(float h, out float azimuth, out float elevation)
    {
        float phase = Mathf.Repeat(h - sunriseHour, 24f);

        float day = Mathf.Max(dayHours, 0.001f);
        float night = NightHours;

        if (phase <= day)
            elevation = elevationSweep * (phase / day);
        else
            elevation = elevationSweep * (1f + (phase - day) / night);

        azimuth = azimuthStart + azimuthSweep * (phase / 24f);
    }

    public void SetHour(float value)
    {
        hour = Mathf.Repeat(value, 24f);
    }

    public void AdvanceHours(float amount)
    {
        SetHour(hour + amount);
    }
}
