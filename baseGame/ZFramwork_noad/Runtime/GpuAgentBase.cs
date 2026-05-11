using UnityEngine;

public abstract class GpuAgentBase : MonoBehaviour
{
    public float scale = 1f;
    public bool visible = true;
    public float baseMoveSpeed = 1f;

    // === Position ===
    public virtual void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    // === Scale ===
    public virtual void SetScale(float s)
    {
        scale = s;
    }

    // === Flip ===
    public virtual void SetFlipX(bool flipped)
    {
    }

    // === Visibility ===
    public virtual void SetVisible(bool value)
    {
        visible = value;
    }

    public void Show() => SetVisible(true);
    public void Hide() => SetVisible(false);

    // === Animation ===
    public abstract void Play(string name);
    public abstract void SetAnimSpeed(float speed);

    public void SetMoveAnimSpeed(float speed)
    {
        float normalizedSpeed = speed / Mathf.Max(0.001f, baseMoveSpeed);
        if (normalizedSpeed < 1)
            SetAnimSpeed(normalizedSpeed);
        else
            SetAnimSpeed(0.8f + 0.2f * normalizedSpeed);
    }
}
