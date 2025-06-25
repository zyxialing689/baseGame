using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCheck
{
    public static List<AICollider> aIColliders = new List<AICollider>();
    public static bool IsTrigger(AIBox box1,AIBox box2) {

        var center1 = box1.GetCenter();
        var center2 = box2.GetCenter();

        if (Mathf.Abs(center1.x - center2.x) < box1.size.x * 0.5f + box2.size.x * 0.5f &&
            Mathf.Abs(center1.y - center2.y) < box1.size.y * 0.5f + box2.size.y * 0.5f
            )
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bodyBox1">攻击方</param>
    /// <param name="bodyBox2">被攻击方</param>
    /// <param name="isRight">右侧方向</param>
    /// <returns></returns>
    public static bool AttackRangeTrigger(AICollider aICollider1, AICollider aICollider2)
    {
        var center1 = aICollider1.attackRangeBox;
        var center2 = aICollider2.bodyBox;
        return IsTrigger(center1,center2);
    }
    public static AICollider[] IsTriggerAllByCamp(AICollider selfCollider, bool isEnemy = true, AICollider enemy = null)
    {
        aIColliders.Clear();
        bool isFriend = false;
        if (enemy == null)
        {
           List<QTNodeItem> enemys = QuadTreeMgr._instance.GetInsideNode(selfCollider.qtnodeItem.bounds);
            for (int i = 0; i < enemys.Count; i++)
            {
                isFriend = isEnemy ? enemys[i].collider.playerCamp != selfCollider.playerCamp : enemys[i].collider.playerCamp == selfCollider.playerCamp;
                if ( 
                    !enemys[i].collider.isAttack
                    && isFriend
                    && !enemys[i].collider.isDead
                    &&  AICondition.IsBeHurtTrigger(selfCollider, enemys[i].collider)
                   )
                {
                    aIColliders.Add(enemys[i].collider);
                }
            }
        }
        else
        {
            isFriend = isEnemy ? selfCollider.playerCamp != enemy.playerCamp : selfCollider.playerCamp == enemy.playerCamp;
            if (
                !enemy.isAttack
                && isFriend
                && AICondition.IsBeHurtTrigger(selfCollider, enemy))
            {
                aIColliders.Add(enemy);
            }
        }
        return aIColliders.ToArray();
    }


}
