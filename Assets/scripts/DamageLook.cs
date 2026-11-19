using UnityEngine;

public static class DamageLook
{
    private static readonly int RustId = Shader.PropertyToID("_Rust");
    private static readonly int CrackId = Shader.PropertyToID("_Crack");
    private static readonly MaterialPropertyBlock Block = new MaterialPropertyBlock();

    public static float Curve(float hp01, float bias)
    {
        float damage = 1f - Mathf.Clamp01(hp01);
        return Mathf.Pow(damage, bias);
    }

    public static float RustAt(float t, float rustAtZero)
    {
        return Mathf.Lerp(0f, rustAtZero, t);
    }

    public static float CrackAt(float t, float crackAtZero)
    {
        return Mathf.Lerp(0f, crackAtZero, t);
    }

    public static void Apply(SpriteRenderer sprite, float rust, float crack)
    {
        sprite.GetPropertyBlock(Block);
        Block.SetFloat(RustId, rust);
        Block.SetFloat(CrackId, crack);
        sprite.SetPropertyBlock(Block);
    }
}
