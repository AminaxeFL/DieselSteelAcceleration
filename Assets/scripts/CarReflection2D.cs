using UnityEngine;

public class CarReflection2D : MonoBehaviour
{
    [SerializeField] private LayerMask carLayers = 1 << 6;
    [SerializeField] private int textureHeight = 256;

    private Renderer targetRenderer;
    private Camera captureCamera;
    private RenderTexture reflectionTexture;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int ReflectionTexId = Shader.PropertyToID("_ReflectionTex");

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        Bounds bounds = targetRenderer.bounds;
        float halfHeight = bounds.size.y * 0.5f;
        int width = Mathf.Max(1, Mathf.RoundToInt(textureHeight * (bounds.size.x / Mathf.Max(halfHeight, 0.001f))));

        reflectionTexture = new RenderTexture(width, textureHeight, 0, RenderTextureFormat.ARGB32);
        reflectionTexture.filterMode = FilterMode.Bilinear;
        reflectionTexture.wrapMode = TextureWrapMode.Clamp;
        reflectionTexture.Create();

        GameObject camObject = new GameObject("CarReflectionCamera");
        camObject.transform.SetParent(transform, false);
        captureCamera = camObject.AddComponent<Camera>();
        captureCamera.orthographic = true;
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
        captureCamera.cullingMask = carLayers;
        captureCamera.targetTexture = reflectionTexture;
        captureCamera.depth = -100f;

        UpdateCameraBounds();

        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture(ReflectionTexId, reflectionTexture);
        targetRenderer.SetPropertyBlock(propertyBlock);
    }

    private void LateUpdate()
    {
        UpdateCameraBounds();
    }

    private void UpdateCameraBounds()
    {
        Bounds bounds = targetRenderer.bounds;
        float halfHeight = bounds.size.y * 0.5f;

        captureCamera.transform.position = new Vector3(
            bounds.center.x,
            bounds.center.y + halfHeight * 0.5f,
            transform.position.z - 10f
        );
        captureCamera.transform.rotation = Quaternion.identity;
        captureCamera.orthographicSize = halfHeight * 0.5f;
        captureCamera.aspect = bounds.size.x / Mathf.Max(halfHeight, 0.001f);
        captureCamera.cullingMask = carLayers;
    }

    private void OnDestroy()
    {
        reflectionTexture.Release();
        Destroy(reflectionTexture);
    }
}
