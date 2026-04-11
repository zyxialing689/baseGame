using UnityEngine;
using Pathfinding;

public class ClickMove : MonoBehaviour
{
    public AIPath ai;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 右键移动
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0;

            ai.destination = world;
        }
    }
}