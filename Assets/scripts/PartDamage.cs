using UnityEngine;

public abstract class PartDamage : MonoBehaviour
{
    public float maxHp = 1000f;
    public float hp = 1000f;
    public float rustAtZero = 1f;
    public float crackAtZero = 1f;
    public float damageBias = 1f;

    public float MaxHp => maxHp;
    public float Hp01 => Mathf.Clamp01(hp / Mathf.Max(maxHp, 1f));

    protected abstract void Apply();

    protected void LookValues(out float rust, out float crack)
    {
        float wear = DamageLook.Curve(Hp01, damageBias);

        rust = DamageLook.RustAt(wear, rustAtZero);
        crack = DamageLook.CrackAt(wear, crackAtZero);
    }

    protected virtual void OnEnable()
    {
        Apply();
    }

    protected virtual void Update()
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
}
