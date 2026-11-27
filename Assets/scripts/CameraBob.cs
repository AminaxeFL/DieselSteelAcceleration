using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [SerializeField] private float positionAmount = 0.04f;
    [SerializeField] private float positionSpeed = 0.25f;
    [SerializeField] private float rotationAmount = 0.8f;
    [SerializeField] private float rotationSpeed = 0.25f;
    [SerializeField] private float smoothTime = 0.35f;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 positionVelocity;
    private float seedX;
    private float seedY;
    private float seedRotZ;

    private void Awake()
    {
        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
        seedX = Random.Range(0f, 100f);
        seedY = Random.Range(0f, 100f);
        seedRotZ = Random.Range(0f, 100f);
    }

    private void LateUpdate()
    {
        float posTime = Time.time * positionSpeed;
        float offsetX = (Mathf.PerlinNoise(seedX, posTime) * 2f - 1f) * positionAmount;
        float offsetY = (Mathf.PerlinNoise(seedY, posTime) * 2f - 1f) * positionAmount;

        float rotTime = Time.time * rotationSpeed;
        float rotZ = (Mathf.PerlinNoise(seedRotZ, rotTime) * 2f - 1f) * rotationAmount;

        Vector3 targetPosition = startPosition + new Vector3(offsetX, offsetY, 0f);
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, rotZ);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref positionVelocity, smoothTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, 1f - Mathf.Exp(-Time.deltaTime / smoothTime));
    }
}
