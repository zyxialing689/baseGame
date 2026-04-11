using UnityEngine;

public class TestMove : IAIMove
{
    public void MoveTo(Vector3 pos)
    {
        Debug.Log("【Move模块】移动到: " + pos);
    }
}