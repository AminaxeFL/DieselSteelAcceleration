using System.Collections.Generic;
using UnityEngine;

public class CarShade : MonoBehaviour
{
    private static readonly List<CarShade> Registry = new List<CarShade>();
    private static int registryVersion;

    public float shadeStep = 0.05f;
    public float shadeMax = 0.8f;
    public float transitionTime = 1.5f;
    public float restartEpsilon = 0.02f;
    public float rowTolerance = 1.5f;
    public float neighborRange = 0f;
    public float recheckInterval = 0.12f;
    public float recheckMoveStep = 0.5f;

    private struct Part
    {
        public SpriteRenderer renderer;
        public Color baseColor;
    }

    private readonly List<Part> parts = new List<Part>();
    private float shade;
    private float fromShade;
    private float toShade;
    private float transitionT = 1f;
    private WheelSpinner car;
    private SpriteRenderer bodyRenderer;

    private int aheadCount;
    private int checkedVersion = -1;
    private int checkedFrontness;
    private float recheckTimer;
    private Vector2 checkedPosition;
    private bool hasCheckedPosition;

    private void Awake()
    {
        car = GetComponent<WheelSpinner>();
        Transform chassis = car.transform;
        bodyRenderer = chassis.GetComponent<SpriteRenderer>();

        CollectParts(chassis);
    }

    private void OnEnable()
    {
        if (Registry.Contains(this))
            return;

        Registry.Add(this);
        registryVersion++;
    }

    private void OnDisable()
    {
        if (!Registry.Remove(this))
            return;

        registryVersion++;
    }

    private void CollectParts(Transform chassis)
    {
        parts.Clear();

        foreach (SpriteRenderer r in chassis.GetComponentsInChildren<SpriteRenderer>(true))
            parts.Add(new Part { renderer = r, baseColor = r.color });

        foreach (WheelJoint2D joint in car.GetComponentsInChildren<WheelJoint2D>(true))
        {
            Rigidbody2D wheel = joint.connectedBody;
            foreach (SpriteRenderer r in wheel.GetComponentsInChildren<SpriteRenderer>(true))
                parts.Add(new Part { renderer = r, baseColor = r.color });
        }
    }

    private void LateUpdate()
    {
        recheckTimer -= Time.deltaTime;

        if (NeedsRecheck())
            RecountAhead();

        float target = Mathf.Min(aheadCount * shadeStep, shadeMax);

        if (Mathf.Abs(target - toShade) > restartEpsilon)
        {
            fromShade = shade;
            toShade = target;
            transitionT = 0f;
        }

        if (transitionT < 1f)
        {
            transitionT = Mathf.Min(1f, transitionT + Time.deltaTime / Mathf.Max(transitionTime, 0.01f));
            float eased = Mathf.SmoothStep(0f, 1f, transitionT);
            shade = Mathf.Lerp(fromShade, toShade, eased);
        }
        else
        {
            shade = toShade;
        }

        ApplyShade();
    }

    private bool NeedsRecheck()
    {
        if (checkedVersion != registryVersion)
            return true;

        if (MyFrontness() != checkedFrontness)
            return true;

        if (hasCheckedPosition && recheckMoveStep > 0f)
        {
            Vector2 position = transform.position;

            if ((position - checkedPosition).sqrMagnitude > recheckMoveStep * recheckMoveStep)
                return true;
        }

        return recheckTimer <= 0f;
    }

    private void RecountAhead()
    {
        aheadCount = CountAhead();

        checkedVersion = registryVersion;
        checkedFrontness = MyFrontness();
        checkedPosition = transform.position;
        hasCheckedPosition = true;
        recheckTimer = Mathf.Max(recheckInterval, 0.02f);
    }

    private int CountAhead()
    {
        Bounds mine = WorldBounds();
        if (IsEmpty(mine))
            return 0;

        int myFront = MyFrontness();
        float myY = mine.center.y;
        Vector2 myPosition = transform.position;
        float rangeSqr = neighborRange * neighborRange;
        int ahead = 0;

        for (int i = 0; i < Registry.Count; i++)
        {
            CarShade other = Registry[i];

            if (other == this || !other.isActiveAndEnabled)
                continue;

            if (other.MyFrontness() <= myFront)
                continue;

            if (neighborRange > 0f)
            {
                Vector2 delta = (Vector2)other.transform.position - myPosition;

                if (delta.sqrMagnitude > rangeSqr)
                    continue;
            }

            Bounds theirs = other.WorldBounds();
            if (IsEmpty(theirs))
                continue;

            if (Mathf.Abs(theirs.center.y - myY) > rowTolerance)
                continue;

            ahead++;
        }

        return ahead;
    }

    private void ApplyShade()
    {
        float k = 1f - shade;
        foreach (Part p in parts)
        {
            Color c = p.baseColor;
            c.r *= k;
            c.g *= k;
            c.b *= k;
            p.renderer.color = c;
        }
    }

    private int MyFrontness()
    {
        return bodyRenderer.sortingOrder;
    }

    private Bounds WorldBounds()
    {
        bool any = false;
        Bounds b = default;

        foreach (Part p in parts)
        {
            if (!p.renderer.enabled)
                continue;

            if (!any)
            {
                b = p.renderer.bounds;
                any = true;
            }
            else
            {
                b.Encapsulate(p.renderer.bounds);
            }
        }

        return b;
    }

    private static bool IsEmpty(Bounds b)
    {
        return b.size.x <= 0f || b.size.y <= 0f;
    }
}
