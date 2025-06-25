using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AICollider))]
public class PathAgent : MonoBehaviour
{
    //public bool IsShowPathGizmos = true;
    public float curTime;
    private ABPath path;
    private Stack<Vector3> movePath;
    public AICollider aICollider;

    protected void Awake()
    {
        movePath = new Stack<Vector3>();
        aICollider = GetComponent<AICollider>();
    }

    public virtual Stack<Vector3> StartPath(Vector3 startPos,Vector3 endPos)
    {
        path = ABPath.Construct(startPos, endPos);
        AstarPath.StartPath(path);
        path.BlockUntilCalculated();
        List<Vector3> vector3s = SmoothPath.SmoothSimple(path.vectorPath);
        movePath.Clear();
        for (int i = vector3s.Count-1; i >=0 ; i--)
        {
            movePath.Push(vector3s[i]);
        }
        return movePath;
    }
    public virtual void StartPath(Vector3 startPos,Vector3 endPos, Action<Stack<Vector3>> callBack = null)
    {
       var temPath = ABPath.Construct(startPos, endPos, (p)=>{
            List<Vector3> vector3s = SmoothPath.SmoothSimple(p.vectorPath);
            Stack<Vector3> tempMovePath = new Stack<Vector3>();
           movePath.Clear();
           for (int i = vector3s.Count - 1; i >= 0; i--)
            {
                    tempMovePath.Push(vector3s[i]);
                     movePath.Push(vector3s[i]);
           }
            callBack?.Invoke(tempMovePath);
        });
        AstarPath.StartPath(temPath);
    }

    public virtual void RestCurStackPath()
    {
        movePath.Clear();
    }

    public virtual Vector2 GetFindPos(Vector3 startPos)
    {
        if(startPos.x - aICollider.groundBox.pos.x>0)
        {
            return aICollider.groundBox.GetFindPos(PointDir.Right);
        }
        else
        {
            return aICollider.groundBox.GetFindPos(PointDir.Left);
        }
    }
    public virtual Vector2 GetFindFarPos(Vector3 startPos)
    {
        if (startPos.x < 0.5f)
        {
            return aICollider.groundBox.GetFindFarPos(PointDir.Right);
        }
        if (startPos.x > (PathFindMgr._instance.maxWidth-0.5f))
        {
            return aICollider.groundBox.GetFindFarPos(PointDir.Left);
        }
        if(startPos.x - aICollider.groundBox.pos.x>0)
        {
            return aICollider.groundBox.GetFindFarPos(PointDir.Right);
        }
        else
        {
            return aICollider.groundBox.GetFindFarPos(PointDir.Left);
        }
    }
    public virtual Vector2 GetFindFarPos(PointDir pointDir)
    {
        return aICollider.groundBox.GetFindFarPos(pointDir);
    }

    private void OnDrawGizmos()
    {
        if (ZDefine._ShowPathGizmos)
        {
            Gizmos.color = Color.green;

            if (movePath != null)
            {
                Vector3[] arrayPath = movePath.ToArray();
                for (int i = 0; i < arrayPath.Length-1; i++)
                {
                    Gizmos.DrawLine(arrayPath[i],arrayPath[i+1]);
                }
            }
            Gizmos.color = Color.yellow;
            if (aICollider != null)
            {
                if (aICollider.groundBox.leftFindPoints != null)
                {
                    for (int i = 0; i < aICollider.groundBox.leftFindPoints.Length; i++)
                    {
                        Gizmos.DrawWireSphere(transform.position + new Vector3(aICollider.groundBox.leftFindPoints[i].x,
                            aICollider.groundBox.leftFindPoints[i].y,0), 0.04f);
                    }
                }
                if (aICollider.groundBox.rightFindPoints != null)
                {
                    for (int i = 0; i < aICollider.groundBox.rightFindPoints.Length; i++)
                    {
                        Gizmos.DrawWireSphere(transform.position + new Vector3(aICollider.groundBox.rightFindPoints[i].x,
                            aICollider.groundBox.rightFindPoints[i].y, 0), 0.04f);
                    }
                }
                if (aICollider.groundBox.leftFarFindPoints != null)
                {
                    for (int i = 0; i < aICollider.groundBox.leftFarFindPoints.Length; i++)
                    {
                        Gizmos.DrawWireSphere(transform.position + new Vector3(aICollider.groundBox.leftFarFindPoints[i].x,
                            aICollider.groundBox.leftFarFindPoints[i].y,0), 0.04f);
                    }
                }
                if (aICollider.groundBox.rightFarFindPoints != null)
                {
                    for (int i = 0; i < aICollider.groundBox.rightFarFindPoints.Length; i++)
                    {
                        Gizmos.DrawWireSphere(transform.position + new Vector3(aICollider.groundBox.rightFarFindPoints[i].x,
                            aICollider.groundBox.rightFarFindPoints[i].y, 0), 0.04f);
                    }
                }
            }
        }
        
    }
}
