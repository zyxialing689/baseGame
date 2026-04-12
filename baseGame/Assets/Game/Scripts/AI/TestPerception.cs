using UnityEngine;

public class TestPerception : IAIPerception
{
    public bool HasTarget()
    {
        return true; // 永远成立（方便测试）
    }

    public float GetDistanceToTarget()
    {
        return 1f;
    }

    public Vector3 GetTargetPosition()
    {
        return Vector3.zero;
    }
}