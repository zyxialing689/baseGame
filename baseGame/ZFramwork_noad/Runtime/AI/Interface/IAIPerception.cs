using UnityEngine;

public interface IAIPerception
{
    bool HasTarget();
    Vector3 GetTargetPosition();
}