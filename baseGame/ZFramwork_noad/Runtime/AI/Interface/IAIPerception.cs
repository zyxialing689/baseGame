using UnityEngine;

public interface IAIPerception
{
    bool HasTarget();
    Vector3 GetTargetPosition();

    float GetDistanceToTarget(); // ⭐ 就是这个
}