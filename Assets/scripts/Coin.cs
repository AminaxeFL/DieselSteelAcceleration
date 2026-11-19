using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float riseHeight = 0.8f;
    [SerializeField] private float riseTime = 0.4f;
    [SerializeField] private float alphaStep = 50f;
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] private float pickupVolume = 1f;

    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock block;
    private FloatBob bob;
    private Vector3 riseFrom;
    private float riseT;
    private float alpha = 1f;
    private bool collected;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bob = GetComponent<FloatBob>();
        block = new MaterialPropertyBlock();

        CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = 0.18f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        collected = true;
        AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);
        Money.Instance.AddMoney(1f);

        riseFrom = transform.position;
        bob.enabled = false;
    }

    private void Update()
    {
        if (!collected)
            return;

        riseT += Time.deltaTime / Mathf.Max(riseTime, 0.01f);
        float t = Mathf.Clamp01(riseT);
        float easeOut = 1f - (1f - t) * (1f - t);

        Vector3 pos = riseFrom;
        pos.y += Mathf.Lerp(0f, riseHeight, easeOut);
        transform.position = pos;

        int steps = Mathf.Max(1, Mathf.CeilToInt(255f / Mathf.Max(alphaStep, 1f)));
        float q = steps > 1
            ? Mathf.Clamp01(Mathf.Floor(t * steps) / (steps - 1f))
            : t;

        alpha = 1f - q;

        spriteRenderer.GetPropertyBlock(block);
        block.SetFloat(AlphaId, alpha);
        spriteRenderer.SetPropertyBlock(block);

        if (alpha <= 0f)
            Destroy(gameObject);
    }
}
