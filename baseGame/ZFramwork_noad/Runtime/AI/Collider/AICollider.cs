using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTimer;
public class AICollider:MonoBehaviour
{
    public bool isShowGizmos = true;
    public bool isEditor = true;
    public bool isAttack = false;
    public bool openInAreaCheck = false;
    public float attackLine = 0;
    [HideInInspector]
    public PlayerCamp playerCamp;
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public int attackSort = 0;

    public Vector2 skyOffset;

    public Vector2 skySize;

    public Vector2 bodyOffset;

    public Vector2 bodySize;

    public Vector2 groundOffset;

    public Vector2 groundSize;

    public AIBox skyBox;
    public AIBox bodyBox;
    public AIBox groundBox;
    public AIBox attackRangeBox;
    private SpriteRenderer render;
    private SortingGroup buffGroup;
    private SortingGroup emojiGroup;
    private SortingGroup effectGroup;
    private SortingGroup effectBackGroup;
    private Transform emojiTf;
    private Transform effectTf;
    private Transform effectBackTf;
    private SortingGroup shadowGroup;
    private SortingGroup virtualBodyGroup;
    private Transform virtualBodyTransform;
    private Dictionary<AICollider,bool> _enemyColliderMap;
    public QTNodeItem qtnodeItem;
    private float halfQTWidth;
    private float halfQTHeight;
    [HideInInspector]
    public bool isFinished = false;
    [HideInInspector]
    public AIAgent agent;
    public AttackInstance _attackInstance;
    [HideInInspector]
    public Transform shadowTf;
    public void SetAttackInstance(AttackInstance attackInstance)
    {
        _attackInstance = attackInstance;
    }

    private void InitQtRect()
    {

        Vector2 vector2 = bodyBox.size + new Vector2(attackLine*2,0);
        halfQTWidth = vector2.x / 2f;
        halfQTHeight = vector2.y / 2f;
        Vector2 pos = new Vector2(bodyBox.GetCenter().x - halfQTWidth, bodyBox.GetCenter().y- halfQTHeight);
        if (qtnodeItem == null)
        {
            qtnodeItem = new QTNodeItem(new Rect(pos, vector2), this);
        }
        else
        {
            qtnodeItem.UpdateRect(pos, vector2);
        }

    }
    private void InitFlyQtRect()
    {
        Vector2 vector2 = bodyBox.size;
        halfQTWidth = vector2.x / 2f;
        halfQTHeight = vector2.y / 2f;
        Vector2 pos = new Vector2(bodyBox.GetCenter().x - halfQTWidth, bodyBox.GetCenter().y- halfQTHeight);
        qtnodeItem = new QTNodeItem(new Rect(pos, vector2),this);
    }

    public QTNodeItem GetQT()
    {
        return qtnodeItem;
    }

    public void SetVirtualAngle(float quaternion)
    {
        if (virtualBodyTransform == null)
        {
            virtualBodyTransform = transform.GetChild(0);
        }
        virtualBodyTransform.localEulerAngles = new Vector3(0,0, quaternion);
    }

    private void Start()
    {
        agent = GetComponent<AIAgent>();
        _enemyColliderMap = new Dictionary<AICollider, bool>();
        virtualBodyTransform = transform.GetChild(0);
        shadowTf = transform.GetChild(1);
        Transform shadowObj = virtualBodyTransform.Find("Shadow");
        shadowObj.gameObject.AddComponent<SortingGroup>().sortingOrder = 5;
        shadowObj.SetParent(shadowTf);
        shadowObj.transform.localRotation = Quaternion.identity;
        isEditor = false;
        virtualBodyGroup = virtualBodyTransform.GetComponent<SortingGroup>();
        if (virtualBodyTransform.childCount>0)
        {
            var tempTf = virtualBodyTransform.GetChild(0);
            if (tempTf.childCount > 0)
            {
                render = tempTf.GetChild(0).GetComponent<SpriteRenderer>();
            }
   
            if (tempTf.childCount > 1)
            {
                buffGroup = tempTf.GetChild(1).GetComponent<SortingGroup>();
            }

        }

        shadowGroup = shadowObj.GetComponent<SortingGroup>();
        if (agent != null)
        {
            playerCamp = agent.agentData.playerCamp;
        }
        if (!isAttack)
        {
            emojiTf = virtualBodyTransform.GetChild(0).Find("emojiPos");
            effectTf = virtualBodyTransform.GetChild(0).Find("effectPos");
            effectBackTf = virtualBodyTransform.GetChild(0).Find("effectBackPos");
            emojiGroup = emojiTf.GetComponent<SortingGroup>();
            effectGroup = effectTf.GetComponent<SortingGroup>();
            effectBackGroup = effectBackTf.GetComponent<SortingGroup>();
            UpdateColliderData();
            _OnStart();
        }
        isFinished = true;
    }
    public void UpdateColliderData()
    {
        attackLine = agent.agentData.attack_line;
        skyBox = new AIBox(transform, playerCamp, agent.agentData.sky_size, Vector2.zero, transform.position);
        bodyBox = new AIBox(transform, playerCamp, agent.agentData.body_size, agent.agentData.body_offset, transform.position);
        groundBox = new AIBox(transform, playerCamp, agent.agentData.ground_size, agent.agentData.ground_offset, transform.position);
        groundBox.InitGroundFindPos(bodyBox.size);

        float emY = bodyBox.GetCenter().y - groundBox.GetCenter().y+agent.agentData.sky_height;
        float emX = (bodyBox.size.x * 0.5f + attackLine);

        attackRangeBox = new AIBox(transform, playerCamp,new Vector2(emX,emY), new Vector2(-emX / 2, emY / 2f), transform.position);

        InitQtRect();
    }
    public void _Destory()
    {
        ColliderMgr.RemoveCollider(this);
    }

    public void _OnStart()
    {
        ColliderMgr.AddCollider(this);
    }

    private void OnEnable()
    {
        isDead = false;
    }

    private void OnDisable()
    {
        isDead = true;
        if (!isAttack)
        {
            _Destory();
        }
    }

    public void ClearHurtCDMap()
    {
        if (_enemyColliderMap != null)
        {
            _enemyColliderMap.Clear();
        }
   
    }

    public void SetDead(bool death = true)
    {
        isDead = death;
    }

    public void FlyPropInit(AIBox skyBox, AIBox groundBox, AIBox attackBoxs)
    {
        this.skyBox = skyBox;
        this.bodyBox = attackBoxs;
        this.groundBox = groundBox;
        InitFlyQtRect();
    }

    public float tempX;
    public float tempY;
    public float tempZ;

    public void _Update()
    {
     
    }
    //如果是空中阶段，z表示 空中的高度
    public void UpdateAIClollider(float x,float y,float z,Transform shadowTransform = null)
    {
        if (!isFinished) return;
        tempX = x;
        tempY = y;
        tempZ = z;
        if (z > 0)
        {
            skyBox.pos = new Vector2(0,z);
            bodyBox.pos = new Vector2(x,y+z);
            groundBox.pos = new Vector2(x,y);
            virtualBodyTransform.localPosition = Vector3.up * z;
        }
        else
        {
            skyBox.pos = Vector2.zero;
            bodyBox.pos = new Vector2(x,y);
            groundBox.pos = new Vector2(x, y);
            virtualBodyTransform.localPosition = Vector2.zero;
        }
        if (!isAttack)
        {
            attackRangeBox.pos = groundBox.pos;
        }

        Vector2 center = bodyBox.GetCenter();
        qtnodeItem.bounds.position = new Vector2(center.x - halfQTWidth, center.y- halfQTHeight);
        qtnodeItem.UpdateTree();
        if (agent != null)
        {
            agent.UpdateUIPosition(center);
        }
        if (shadowTransform != null)
        {
            shadowTransform.position = groundBox.GetCenter();
        }

        if (render != null)
        {
            render.sortingOrder = 20000 - Mathf.FloorToInt(groundBox.GetCenter().y * 100) + attackSort;
            if (buffGroup != null)
            {
                buffGroup.sortingOrder = render.sortingOrder + 1;
            }
            if (virtualBodyGroup != null)
            {
                virtualBodyGroup.sortingOrder = render.sortingOrder + 1;
            }
            if (emojiGroup != null)
            {
                emojiGroup.sortingOrder = render.sortingOrder + 1;
            }
            if (effectGroup != null)
            {
                effectGroup.sortingOrder = render.sortingOrder + 1;
            }
            if (effectBackGroup != null)
            {
                effectBackGroup.sortingOrder = render.sortingOrder - 1;
            }
        }
        else
        {
            if (virtualBodyGroup != null)
            {
                virtualBodyGroup.sortingOrder = SortUtils.SetSoringBody(groundBox.GetCenter(), attackSort);
            }
        }

        if (shadowGroup != null)
        {
            shadowGroup.sortingOrder = 10000 - Mathf.FloorToInt(groundBox.GetCenter().y * 100) + attackSort;
        }
 

        if (isAttack)
        {
            if (_attackInstance.agentSkill.ignore_collier)
            {
                return;
            }
            if (_attackInstance.onceCheck&&!_attackInstance._onceCheck)
            {
                return;
            }
            if (_attackInstance.onceColliderCheck&&!_attackInstance._onceColliderCheck)
            {
                return;
            }
            AICollider[] enemys = null;
            if (!_attackInstance.agentSkill.is_aoe)
            {
                var targetAgent = _attackInstance.targetAgent;
                if(targetAgent == null||targetAgent.isDead)
                {
                    if(_attackInstance.agentSkill.IsOpenDeathAoe())
                    enemys = ColliderCheck.IsTriggerAllByCamp(this,!_attackInstance.agentSkill.focus_friend);
                }
                else
                {
                    enemys = ColliderCheck.IsTriggerAllByCamp(this, !_attackInstance.agentSkill.focus_friend, targetAgent.aICollider);
                }

            }
            else
            {
                enemys = ColliderCheck.IsTriggerAllByCamp(this,!_attackInstance.agentSkill.focus_friend);
            }
     
            if (enemys != null && enemys.Length > 0)//击中的敌人
            {
                if (_attackInstance.triggerOnce)
                {
                    _attackInstance.TrigerOnceAll(enemys);
                    _attackInstance.triggerOnce = false;
                }
                if (openInAreaCheck)
                {
                    _attackInstance.JudgeOutArea(enemys);
                }
                if (openInAreaCheck)
                {
                    _attackInstance.JudgeInArea(enemys);
                }
                for (int i = 0; i < enemys.Length; i++)
                {
                    AddBeAttack(enemys[i]);
                }
                if (_attackInstance.fllowCollider!=null&&!_attackInstance.fllowCollider.agent.trueDeath&&_attackInstance.fllowCollider.agent.agentData.IsVampirism())
                {
                    int hurt = -Mathf.RoundToInt(_attackInstance.GetDamage() * enemys.Length * _attackInstance.fllowCollider.agent.agentData.vampirism_value);
                    if (hurt > 0)
                    {
                        _attackInstance.fllowCollider.agent.attrData.ChangeHp(hurt,null, _attackInstance.fllowCollider.GetHurtPos());
                    }
                }
                _attackInstance._onceColliderCheck = false;
            }
            _attackInstance._onceCheck = false;
        }

    }

    public Vector3 GetHurtPos()
    {
        return bodyBox.GetCenter();
    }
    public Vector3 GetGroundPos()
    {
        return groundBox.GetCenter();
    }

    public void AddBeAttack(AICollider enemyCollider)
    {
        if (_enemyColliderMap.ContainsKey(enemyCollider))
        {
            return;
        }
        else
        {
            if (_attackInstance!=null)
            {
                _attackInstance.SetAttackEffect(enemyCollider);
            }
            _enemyColliderMap.Add(enemyCollider, true);
            if (bodyBox.hurtCD > 0)
            {
                Timer.Register(bodyBox.hurtCD, () =>
                {
                    if (_enemyColliderMap.ContainsKey(enemyCollider))
                    {
                        _enemyColliderMap.Remove(enemyCollider);
                    }
                }, null, false, false, this);
            }
        }
    }


    private void OnDrawGizmos()
    {
            if (isEditor)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y) + skyOffset, skySize);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y) + bodyOffset, bodySize);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y) + groundOffset, groundSize);

                Gizmos.color = Color.cyan;


                        //   Vector2 vector2 = bodyBox.GetCenter();
                        //float emY = vector2.y - groundBox.GetCenter().y;
                        //float emX = (bodyBox.size.x * 0.5f + attackLine);
                        //Vector2 vectorG2 = groundBox.GetCenter();
                Vector2 vector2 = new Vector2(transform.position.x, transform.position.y) + bodyOffset;
                Vector2 vectorG2 = new Vector2(transform.position.x, transform.position.y) + groundOffset;
                float emY = vector2.y - vectorG2.y;
                float emX = (bodySize.x * 0.5f + attackLine);

                if (transform.localScale.x < 0)
                {
                Gizmos.DrawWireCube(vectorG2 + new Vector2(emX / 2, emY / 2f), new Vector2(emX, emY));
                 }
                else
                {
                Gizmos.DrawWireCube(vectorG2 + new Vector2(-emX / 2, emY / 2f), new Vector2(emX, emY));
                 }


            }

            if (!ZDefine._ShowColliderGizmos || !isFinished&&Application.isPlaying) return;

                if (skyBox != null)
                {
                    Gizmos.color = isAttack?Color.red : Color.blue;
                    Gizmos.DrawWireCube(skyBox.GetCenter(), skyBox.size);
                }
                if (bodyBox != null)
                {
                    Gizmos.color = isAttack ? Color.red : Color.green;
                    Gizmos.DrawWireCube(bodyBox.GetCenter(), bodyBox.size);
                    Gizmos.color = Color.cyan;
                    if (!isAttack)
                    {
                      Gizmos.DrawWireCube(attackRangeBox.GetCenter(), attackRangeBox.size);
                    }
                }
                if (groundBox != null)
                {
                    Gizmos.color = isAttack ? Color.red : Color.yellow;
                    Vector2 vector2 = groundBox.GetCenter();
                    Gizmos.DrawWireCube(vector2, groundBox.size);
                }
            
        
    }
}

public static class SortUtils
{
    public static int SetSoringBody(Vector3 pos,int extraSort = 0)
    {
      return  20001 - Mathf.FloorToInt(pos.y * 100) + extraSort;
    }
}