using UnityEngine;

[ExecuteAlways]
public class CarDamage : MonoBehaviour
{
    public const float SliderMax = 1000f;

    public float maxHp = 1000f;
    public float hp = 1000f;
    public SpriteRenderer sprite;
    public SpriteRenderer kopot;
    public float kopotBrokenAt = 0.2f;
    public float rustAtZero = 1f;
    public float crackAtZero = 1f;
    public float damageBias = 1f;

    private HingeJoint2D kopotHinge;
    private FixedJoint2D kopotFixed;

    public float MaxHp => maxHp;
    public float Hp01 => Mathf.Clamp01(hp / Mathf.Max(maxHp, 1f));

    private void Awake()
    {
        kopotHinge = kopot.GetComponent<HingeJoint2D>();
        kopotFixed = kopot.GetComponent<FixedJoint2D>();
    }

    private void OnEnable()
    {
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    public void SetHp(float value)
    {
        hp = Mathf.Clamp(value, 0f, maxHp);
        Apply();
    }

    public void Damage(float amount)
    {
        SetHp(hp - amount);
    }

    public void Repair(float amount)
    {
        SetHp(hp + amount);
    }

    public void RepairFull()
    {
        SetHp(maxHp);
    }

    private void Apply()
    {
        float t = DamageLook.Curve(Hp01, damageBias);

        DamageLook.Apply(sprite, DamageLook.RustAt(t, rustAtZero), DamageLook.CrackAt(t, crackAtZero));
        DamageLook.Apply(kopot, DamageLook.RustAt(t, rustAtZero), DamageLook.CrackAt(t, crackAtZero));

        ApplyKopotLimits();
    }

    private void ApplyKopotLimits()
    {
        if (Hp01 <= kopotBrokenAt)
        {
            kopotFixed.enabled = false;
            kopotHinge.useLimits = true;
            kopotHinge.limits = new JointAngleLimits2D { min = 283f, max = 359f };
        }
        else
        {
            kopotFixed.enabled = true;
            float angle = kopotHinge.jointAngle;
            kopotHinge.useLimits = true;
            kopotHinge.limits = new JointAngleLimits2D { min = angle, max = angle };
        }
    }
}
