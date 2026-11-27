using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SpriteButton : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float hoverBrightness = 0.85f;
    [SerializeField] private float holdBrightness = 0.65f;
    [SerializeField] private float clickBrightness = 0.5f;
    [SerializeField] private float clickFlashDuration = 0.08f;

    public bool interactable = true;
    public UnityEvent onClick;
    public UnityEvent onHold;
    public UnityEvent onUnhold;

    private SpriteRenderer boundsSprite;
    private SpriteRenderer[] renderers;
    private Color[] baseColors;
    private bool isHovered;
    private bool isHeld;
    private float clickTimer;

    private void Awake()
    {
        if (!targetCamera)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            targetCamera = canvas ? canvas.worldCamera : Camera.main;
        }

        boundsSprite = GetComponent<SpriteRenderer>();
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
            baseColors[i] = renderers[i].color;
    }

    private void OnDisable()
    {
        isHovered = false;
        isHeld = false;
        clickTimer = 0f;
        ApplyBrightness(1f);
    }

    private void Update()
    {
        if (!interactable)
        {
            isHovered = false;
            isHeld = false;
            clickTimer = 0f;
            ApplyBrightness(1f);
            return;
        }

        Pointer pointer = Pointer.current;
        Vector2 screenPos = pointer.position.ReadValue();
        isHovered = IsPointerOver(targetCamera, screenPos);

        if (pointer.press.wasPressedThisFrame && isHovered)
        {
            isHeld = true;
            onHold.Invoke();
        }

        if (isHeld && pointer.press.wasReleasedThisFrame)
        {
            isHeld = false;
            onUnhold.Invoke();

            if (isHovered)
            {
                clickTimer = clickFlashDuration;
                onClick.Invoke();
            }
        }

        if (clickTimer > 0f)
            clickTimer -= Time.deltaTime;

        float brightness = 1f;
        if (clickTimer > 0f)
            brightness = clickBrightness;
        else if (isHeld)
            brightness = holdBrightness;
        else if (isHovered)
            brightness = hoverBrightness;

        ApplyBrightness(brightness);
    }

    private bool IsPointerOver(Camera cam, Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane plane = new Plane(-transform.forward, transform.position);

        if (!plane.Raycast(ray, out float distance))
            return false;

        Vector3 localPoint = transform.InverseTransformPoint(ray.GetPoint(distance));
        localPoint.z = boundsSprite.localBounds.center.z;
        return boundsSprite.localBounds.Contains(localPoint);
    }

    private void ApplyBrightness(float multiplier)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Color c = baseColors[i];
            renderers[i].color = new Color(c.r * multiplier, c.g * multiplier, c.b * multiplier, c.a);
        }
    }
}
