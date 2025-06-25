using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RovMovement : MonoBehaviour
{
    public float radis = 1f;
    public Vector2 offset;
    Vector2 lastDirG;
    private float[] daggerDirs = new float[eightDirs.Length];
    private static Vector2[] eightDirs = new Vector2[] {
        Vector2.up ,
        //new Vector2(0.5f,1f).normalized,
        Vector2.one.normalized ,
        //new Vector2(1f,0.5f).normalized,
        Vector2.right,
        //new Vector2(1f,-0.5f).normalized,
        new Vector2(1,-1).normalized,
        //new Vector2(0.5f,-1f).normalized,
        Vector2.down ,
        //new Vector2(-0.5f,-1f).normalized,
        new Vector2(-1,-1).normalized ,
        //new Vector2(-1f,-0.5f).normalized,
        Vector2.left,
        //new Vector2(-1f,0.5f).normalized,
        new Vector2(-1,1).normalized,
        //new Vector2(-0.5f,1f).normalized,
    };

    List<Vector3> cantMove = new List<Vector3>();
    AIAgent self;
    Vector3 moveDir;
    public bool _FixedUpdate(AIAgent self,AIAgent[] targets, Vector2 targetPos)
    {
        this.self = self;
        for (int i = 0; i < daggerDirs.Length; i++)
        {
            daggerDirs[i] = 0;
        }
        for (int j = 0; j < targets.Length; j++)
        {
            if ( targets[j] == null || targets[j].trueDeath|| targets[j].GetComponent<RovMovement>()==null)
            {
                continue;
            }
            for (int i = 0; i < eightDirs.Length; i++)
            {
                Vector2 directionToObstacle = targets[j].GetComponent<RovMovement>().GetCenter() - GetCenter() + eightDirs[i] * radis;
                float dis = directionToObstacle.magnitude;
                float weight = 0;
                if (dis < radis)
                {
                    weight = (radis - dis) / radis; ;
                }
                //float valueToPutIn = result * weight;
                if (weight > daggerDirs[i])
                {
                    daggerDirs[i] = weight;
                }
            }
        }
        Vector2 lastDir = Vector2.zero;
        for (int i = 0; i < eightDirs.Length; i++)
        {
            lastDir += eightDirs[i] * daggerDirs[i];
        }
        Vector2 targetDis = (targetPos - GetCenter());
        Vector2 targetDir = targetDis.normalized;
        lastDir = (lastDir + targetDir).normalized;
        if (Vector2.Dot(targetDir, lastDir) < 0f)
        {
            self.agentTempData.findStop = true;
            return false;
        }
        else
        {
            Vector2 pos = transform.position;
            pos += lastDirG * Time.fixedDeltaTime * self.moveSpeed;
            self.SetPosition(pos);
        }
        lastDirG = lastDir;
        return true;
    }

    Vector2 pos;
    private void OnDrawGizmos()
    {
        if (self != null&& ZDefine._ShowPathGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(GetCenter(), lastDirG);
            //Gizmos.color = Color.green;
            //for (int i = 0; i < interestDirs.Length; i++)
            //{
            //    Gizmos.DrawRay(GetCenter(), eightDirs[i] * interestDirs[i]);
            //}

            Gizmos.color = Color.red;
            for (int i = 0; i < daggerDirs.Length; i++)
            {
                Gizmos.DrawRay(transform.position, eightDirs[i] * daggerDirs[i]);
            }
            Gizmos.color = Color.red;
            for (int i = 0; i < eightDirs.Length; i++)
            {
                Gizmos.DrawSphere(GetCenter() + eightDirs[i] * radis, 0.1f);
            }
        }
    }


    public Vector2 GetCenter()
    {
        pos = transform.position;
        return pos + offset;
    }
}
