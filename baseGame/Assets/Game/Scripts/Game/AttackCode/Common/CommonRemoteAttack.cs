using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;

public class CommonRemoteAttack : AttackInstance
{
    private float startVirtualHeight = 0f;
    private float speed = 20;
    private Vector3 baseDir;
    public override void Init()
    {
        aiCollider.skyBox._offset.y -= 0.5f;
        aiCollider.skyBox.offset.y -= 0.5f;

        height = startVirtualHeight;

        baseDir = (targetPos - transform.position+Vector3.up*height).normalized;
        if (transform.position.x > targetPos.x)
        {
           var angle = Vector3.Angle(Vector3.up, baseDir);
            aiCollider.SetVirtualAngle(angle);
        }
        else
        {
            var angle = -Vector3.Angle(Vector3.up, baseDir);
            aiCollider.SetVirtualAngle(angle);
        }

    }
    public override void SetAttackEffect(AICollider enemyCollider)
    {
        if (enemyCollider.agent != null && enemyCollider.agent.attrData != null)
        {
            int hurt = GetDamage();
            enemyCollider.agent.attrData.ChangeHp(hurt, agentSkill, enemyCollider.GetHurtPos());
            //DamageNumberMgr._instance.Spawn(enemyCollider, hurt);
            ReadyDestroy();
        }
    }

    public override void _FixedUpdate()
    {
        transform.Translate(baseDir * Time.fixedDeltaTime*speed);
        aiCollider.UpdateAIClollider(transform.position.x, transform.position.y, height);
    }

}
