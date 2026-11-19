using UnityEngine;

[ExecuteAlways]
public class WheelDamage : MonoBehaviour
{
    public const float SliderMax = 500f;

    public float maxHp = 500f;
    public float hp = 500f;
    public SpriteRenderer[] wheels;
    public float rustAtZero = 1f;
    public float crackAtZero = 0f;
    public float damageBias = 1f;

    public float MaxHp => maxHp;
    public float Hp01 => Mathf.Clamp01(hp / Mathf.Max(maxHp, 1f));

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
        float rust = DamageLook.RustAt(t, rustAtZero);
        float crack = DamageLook.CrackAt(t, crackAtZero);

        for (int i = 0; i < wheels.Length; i++)
        {
            DamageLook.Apply(wheels[i], rust, crack);
        }
    }
}
