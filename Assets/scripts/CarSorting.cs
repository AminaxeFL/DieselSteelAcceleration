using System.Collections.Generic;
using UnityEngine;

public class CarSorting : MonoBehaviour
{
    public int depthOrder = -2000;
    public int wheelDepthOffset = 1;
    public SpriteRenderer helm;
    public SpriteRenderer neck;
    public int helmDepthOffset = 1;
    public int neckDepthOffset = 2;
    public SpriteRenderer springRight;
    public SpriteRenderer springLeft;
    public int springDepthOffset = 3;
    public SpriteRenderer kopot;
    public int kopotDepthOffset = 1;
    public string smokeObjectName = "smokeParticles";
    public int smokeDepthOffset = -1;

    public int SmokeSortingOrder => depthOrder + smokeDepthOffset;

    private readonly List<SpriteRenderer> bodyRenderers = new List<SpriteRenderer>();
    private readonly List<SpriteRenderer> wheelRenderers = new List<SpriteRenderer>();

    private void Awake()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        CollectRenderers();
        ApplyDepth();
    }

    public void SetDepth(int order)
    {
        depthOrder = order;
        ApplyDepth();
    }

    private void CollectRenderers()
    {
        bodyRenderers.Clear();
        wheelRenderers.Clear();

        bodyRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>(true));

        foreach (Rigidbody2D wheel in FindWheels())
            wheelRenderers.AddRange(wheel.GetComponentsInChildren<SpriteRenderer>(true));

        bodyRenderers.RemoveAll(r => wheelRenderers.Contains(r));

        RemoveWithChildren(helm);
        RemoveWithChildren(neck);
        RemoveWithChildren(springRight);
        RemoveWithChildren(springLeft);
        RemoveWithChildren(kopot);
        RemoveTransformWithChildren(FindTransformByName(transform, smokeObjectName));
    }

    private void RemoveTransformWithChildren(Transform root)
    {
        foreach (SpriteRenderer r in root.GetComponentsInChildren<SpriteRenderer>(true))
            bodyRenderers.Remove(r);
    }

    private static Transform FindTransformByName(Transform root, string target)
    {
        foreach (Transform child in root)
        {
            if (string.Equals(child.name, target, System.StringComparison.OrdinalIgnoreCase))
                return child;
        }

        return root;
    }

    private void RemoveWithChildren(SpriteRenderer root)
    {
        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            bodyRenderers.Remove(renderer);
    }

    private void ApplyDepth()
    {
        int bodyOrder = depthOrder;
        int wheelOrder = depthOrder - wheelDepthOffset;

        foreach (SpriteRenderer r in bodyRenderers)
            r.sortingOrder = bodyOrder;

        foreach (SpriteRenderer r in wheelRenderers)
            r.sortingOrder = wheelOrder;

        helm.sortingOrder = bodyOrder - helmDepthOffset;
        neck.sortingOrder = bodyOrder - neckDepthOffset;
        springRight.sortingOrder = bodyOrder - springDepthOffset;
        springLeft.sortingOrder = bodyOrder - springDepthOffset;
        kopot.sortingOrder = bodyOrder - kopotDepthOffset;

        ApplySmokeDepth(bodyOrder);
    }

    private void ApplySmokeDepth(int bodyOrder)
    {
        Transform smoke = FindTransformByName(transform, smokeObjectName);
        foreach (SpriteRenderer r in smoke.GetComponentsInChildren<SpriteRenderer>(true))
            r.sortingOrder = bodyOrder + smokeDepthOffset;
    }

    private Rigidbody2D[] FindWheels()
    {
        WheelSpinner spinner = GetComponent<WheelSpinner>();
        List<Rigidbody2D> wheelsList = new List<Rigidbody2D>();
        foreach (WheelJoint2D joint in spinner.GetComponentsInChildren<WheelJoint2D>(true))
            wheelsList.Add(joint.connectedBody);
        return wheelsList.ToArray();
    }
}
