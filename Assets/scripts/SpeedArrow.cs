using UnityEngine;

public class SpeedArrow : MonoBehaviour
{
    private WheelSpinner car;
    [SerializeField] private float minX = -923.34f;
    [SerializeField] private float maxX = -323f;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    private RectTransform rectTransform;
    private float position01;
    private float position01Velocity;
    private float anchoredY;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Target");
        car = player.GetComponent<WheelSpinner>();

        rectTransform = GetComponent<RectTransform>();
        anchoredY = rectTransform.anchoredPosition.y;

        if (rectTransform.anchorMin != Vector2.zero || rectTransform.anchorMax != Vector2.zero)
        {
            anchoredY = transform.localPosition.y + referenceResolution.y * 0.5f;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
        }

        position01 = Mathf.Clamp01(car.Fuel01);
    }

    private void Update()
    {
        float target = Mathf.Clamp01(car.Fuel01);

        position01 = smoothTime <= 0.0001f
            ? target
            : Mathf.SmoothDamp(position01, target, ref position01Velocity, smoothTime);
        position01 = Mathf.Clamp01(position01);

        float minOffset = minX + referenceResolution.x * 0.5f;
        float maxOffset = maxX + referenceResolution.x * 0.5f;
        float targetX = Mathf.Lerp(minOffset, maxOffset, position01);

        rectTransform.anchoredPosition = new Vector2(targetX, anchoredY);
    }
}
