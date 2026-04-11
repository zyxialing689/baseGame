using UnityEngine;

public class TestPerception : IAIPerception
{
    public bool HasTarget()
    {
        return true; // 永远有目标（测试用）
    }

    public Vector3 GetTargetPosition()
    {
        return new Vector3(5, 0, 0);
    }
}