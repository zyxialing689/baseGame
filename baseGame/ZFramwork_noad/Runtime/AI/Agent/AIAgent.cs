using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTimer;
using Random = UnityEngine.Random;

public class AIAgent : PathAgent
{
    public int testID = -1;
    public int offsetAttackID = 0;
    public float attackSpeed = 1;
    public float moveSpeed = 1;
    public float testSpeed = 1f;
    public float testY;
    public float base_role_height;
    public float role_height = 0;
    public float jump_time = 10;
    private float maxHeight;
    private int stopTime = 10000;
    public bool isOpen = true;
    public bool isStake = false;
    public bool isFrozen = false;
    public bool isDizz = false;
    public bool unmatched = false;
    public bool trueDeath = false;
    public Vector2 startPoses;
    private bool ghost = false;
    [HideInInspector]
    public PlayerCamp playerCamp;
    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    public bool isDead = false;
    public AIAgent attackTarget;
    [HideInInspector]
    public AITeam team;
    private Dictionary<int, AIStateData> _statesType;//当前状态
    public Dictionary<int, AIState> _aiStates;//当前状态执行脚本
    private Dictionary<int, AIStateData> _stateList;//所有Head集合
    private Transform virtualBodyTransform;
    [HideInInspector]
    public Animator animator;
    public AgentTempData agentTempData;
    private Dictionary<string, AnimationClip> animationClipMap;

    [HideInInspector]
    public AgentData agentData;
    [HideInInspector]
    public Transform uiTransform;
    [HideInInspector]
    public AIAttrData attrData;
    private UIRoleLoadBar uIRoleLoadBar;
    private AnimatorEventMono animatorEventMono;
    public BuffRenderMgr buffRenderMgr;
    public Transform emojiTf;
    public Transform effectTf;
    public Transform effectBackTf;
    public SpriteRenderer render;
    public SpriteRenderer buffRender;
    private Dictionary<BuffType, BaseBuff> buffMaps;
    private Dictionary<BuffType, bool> readyClear;
    private Dictionary<BuffType, BuffData> ImmunityBuffMap;
    private Dictionary<string, EffectData> effectKeepMap;
    public RovMovement rovMovement;
    protected new void Awake()
    {
        base.Awake();
        buffMaps = new Dictionary<BuffType, BaseBuff>();
        readyClear = new Dictionary<BuffType, bool>();
        ImmunityBuffMap = new Dictionary<BuffType, BuffData>();
        effectKeepMap = new Dictionary<string, EffectData>();
        InitAgentData(testID);
    }

    public void InitAgentData(int roleId)
    {
        #region 角色表数据
        agentData = new AgentData(roleId);
        #endregion
    }

    public void Clear()
    {
        //if (animationClipMap != null)
        //{
        //    foreach (var item in animationClipMap)
        //    {
        //        DestroyImmediate(item.Value);
        //    }
        //}

        animator.enabled = false;
        animationClipMap.Clear();
    }

    private void Start()
    {
        unmatched = false;
           isDizz = false;
        isFrozen = false;
        trueDeath = false;
        Random.InitState(Mathf.RoundToInt(Time.realtimeSinceStartup));
        #region 初始化预制体
        virtualBodyTransform = transform.GetChild(0).transform;
        emojiTf = virtualBodyTransform.Find("renderPos").Find("emojiPos");
        effectTf = virtualBodyTransform.Find("renderPos").Find("effectPos");
        effectBackTf = virtualBodyTransform.Find("renderPos").Find("effectBackPos");
        buffRenderMgr = virtualBodyTransform.Find("renderPos").Find("buffs").GetComponent<BuffRenderMgr>();
        buffRenderMgr.Init();
        if (!agentData.is_far_hero)
        {
            rovMovement = gameObject.AddComponent<RovMovement>();
            rovMovement.radis = 0.5f;
        }
        animator = virtualBodyTransform.GetComponent<Animator>();
        if (animator == null)
        {
            animator = virtualBodyTransform.Find("renderPos").Find("render").GetComponent<Animator>();
            buffRender = buffRenderMgr.transform.Find("buffRender").GetComponent<SpriteRenderer>();
            render = animator.GetComponent<SpriteRenderer>();
            animatorEventMono = animator.GetComponent<AnimatorEventMono>();
        }
        UpdateAnimator();
        #endregion

        InitData();
    }
    public void UpdateAnimator()
    {
        if (animationClipMap == null)
        {
            animationClipMap = new Dictionary<string, AnimationClip>();
        }
        else
        {
            Clear();
            animator.enabled = false;
        }

        var animationClips = animator.runtimeAnimatorController.animationClips;
        foreach (var item in animationClips)
        {
            if (animationClipMap.ContainsKey(item.name))
            {
                ZLogUtil.LogWarning("有重复动画确保没有问题？");
            }
            else
            {
                animationClipMap.Add(item.name, item);
            }

        }
        animator.Update(Time.fixedDeltaTime);
        animatorEventMono.Init(this);
    }

    static float testn = 1;

    private void InitData()
    {
        maxHeight = 10;

        #region 战斗中统计数据
        agentTempData = new AgentTempData();
        #endregion

        #region AI
        UpdateAI(agentData.ai_path);
        playerCamp = agentData.playerCamp;
        curTime = 0;
        AIMgr.AddPlayer(this);
        #endregion
        for (int i = 0; i < agentData.self_buffs.Count; i++)
        {
            SetBuff(agentData.self_buffs[i]);
        }
    }

    bool haveGroundToSkyAnim = false;
    Transform renderTf;
    [HideInInspector]
    public AIDataJson aIDataJson;
    public void UpdateAI(string path)
    {

        agentTempData.aiTotalTime = curTime;
        moveSpeed = agentData.move_speed*agentTempData.stateMoveSpeed*agentTempData.iceSpeed;
        emojiTf.localPosition = agentData.emoji_pos;
        effectTf.localPosition = agentData.effect_pos;
        effectBackTf.localPosition = agentData.effect_pos;
        base_role_height = agentData.sky_height;
        agentData.ai_path = path;

        _aiStates = new Dictionary<int, AIState>();
        _statesType = new Dictionary<int, AIStateData>();
        _stateList = new Dictionary<int, AIStateData>();
        TextAsset textAsset = TextAssetUtils.GetTextAsset(path);
         aIDataJson = JsonUtility.FromJson<AIDataJson>(textAsset.text);
        List<AIStateData> adjList = aIDataJson.GetHeadList();
        for (int i = 0; i < adjList.Count; i++)
        {
            _stateList.Add(i, adjList[i]);
            _statesType.Add(i, adjList[i]);
        }
        ResetState();
    }

    public void UpdateToSkyAnim()
    {
        if (haveGroundToSkyAnim)
        {
            isDead = true;
            stopTime = 1000;
            if (testY != agentData.sky_height)
            {
                testY = testY+=0.05f;
            }
            //if (renderTf.localPosition != agentData.render_pos)
            //{
            //    renderTf.localPosition = Vector3.MoveTowards(renderTf.localPosition, agentData.render_pos, Time.fixedDeltaTime*2);
            //}
            if (agentData.sky_height - testY < 0.05f)
            {
                testY = agentData.sky_height;
            }
            //if (Vector3.Distance(renderTf.localPosition, agentData.render_pos) < 0.01f)
            //{
            //    renderTf.localPosition = agentData.render_pos;
            //}
            if (testY == agentData.sky_height && renderTf.localPosition == agentData.render_pos)
            {
                haveGroundToSkyAnim = false;
                isDead = false;
            }
            base_role_height = testY;
            role_height = base_role_height;
        }
    }

    private void OnEnable()
    {
        isDead = false;
        if (uiTransform != null)
        {
            uiTransform.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        isDead = true;
        if (uiTransform != null)
        {
            uiTransform.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (uiTransform != null)
        {
            Destroy(uiTransform.gameObject);
        }
        Clear();
        AIMgr.RemovePlayer(this);
    }

    public Transform BindUI(GameObject uiObj = null)//不绑ui就是无敌
    {
        uiTransform = new GameObject(gameObject.name).transform;
        if (uiObj != null)
        {
            uIRoleLoadBar = uiObj.GetComponent<UIRoleLoadBar>();
            attrData = new AIAttrData(this);
            TransformUtils.TransformLocalNormalize(uiObj, uiTransform);
            uIRoleLoadBar.UpdateLocalPos(agentData.ui_hp_offset);
            uIRoleLoadBar.UpdateSize(agentData.ui_hp_size);
        }

        return uiTransform;
    }

    public void UpdateBindUI()
    {
        uIRoleLoadBar.UpdateLocalPos(agentData.ui_hp_offset);
        uIRoleLoadBar.UpdateSize(agentData.ui_hp_size);
    }

    public void UpdateHp(float hpProgress)
    {
        if (uIRoleLoadBar != null)
        {
            uIRoleLoadBar.UpdateHp(hpProgress);
        }
    }
    public void UpdateUIPosition(Vector3 pos)
    {
        if (uiTransform != null)
        {
            uiTransform.position = pos;
        }
    }

    public void TriggerDead()
    {
        if (!trueDeath)
        {
            SetAnimator(true);
            isOpen = false;
            isDead = true;
            trueDeath = true;
            animator.SetInteger("type", 11);
            uiTransform.gameObject.SetActive(false);
            aICollider.SetDead();
            float deathKeepTime = GetAnimationLength(GameAnimationName.animationName_4_Death);
            //Timer.Register(deathKeepTime * 0.5f, () => {
            //}, null, false, false, this);
            if (renderTf != null && agentData.sky_height > 0)
            {
                //renderTf.DOLocalMoveY(-3.8f, 0.5f);
            }
            ClearAllBuff();
            ReleaseEffect();

            EventManager.Instance.Dispatch(Event_Battle_RemovePlayerNum.AutoCreate());
            Destroy(gameObject, deathKeepTime + 3f);
        }

    }

    public void ResetState()
    {
        foreach (var item in _stateList)
        {
            _statesType[item.Key] = item.Value;
            item.Value.SetNextTime(curTime);
            item.Value.SetNextCondtionTime(curTime);
            if (_aiStates.ContainsKey(item.Key))
            {
                _aiStates[item.Key].Exit();
                _aiStates[item.Key] = GetAICode(item.Value);
            }
            else
            {
                _aiStates.Add(item.Key, GetAICode(item.Value));
            }
        }
        agentTempData.isAttacking = false;
    }

    public void _Update()
    {
        animator.speed = agentTempData.iceSpeed;
        foreach (var item in readyClear)
        {
            if (buffMaps.ContainsKey(item.Key))
            {
                buffMaps.Remove(item.Key);
            }
        }
        readyClear.Clear();
        foreach (var item in buffMaps)
        {
            item.Value._Update();
        }
        if (isFrozen)
        {
            buffRenderMgr._Update();
            return;
        }
        buffRenderMgr._Update();

        if (!isOpen|| isStake) {
            return;
        }

        if (attackTarget != null)
        {
            if (attackTarget.isDead)
            {
                attackTarget = null;
            }
        }
        curTime = curTime + Time.deltaTime;
        if (isDizz)
        {
            return;
        }
    
        UpdateSortState();
        if (_aiStates != null)
        {
            for (int i = 0; i < _aiStates.Count; i++)
            {
                if (_aiStates[i] != null)
                {
                    if (_aiStates[i].enableUpdate)
                    {
                        _aiStates[i].UpdateExecute();
                    }
                    else
                    {
                        _aiStates[i].enableUpdate = true;
                        _aiStates[i].Start();
                    }
                }
            }
        }
    }

    public void _FixedUpdate()
    {
        FllowEffectScale();
        if (!isOpen|| isFrozen||isDizz) {
            UpdateDeathStrikeFly();
            return;
        }
        Ghost();

        JumpToSky();
        if (IsInStrikeFly())
        {
            transform.Translate(strikeFlyDir * Time.fixedDeltaTime);
            BoundPosition();
        }
        else
        {
            moveSpeed = agentData.move_speed*agentTempData.stateMoveSpeed* agentTempData.iceSpeed*testSpeed;
        }

        aICollider.UpdateAIClollider(transform.position.x,transform.position.y, role_height, null);
        UpdateToSkyAnim();
        if (_aiStates != null)
        {
            for (int i = 0; i < _aiStates.Count; i++)
            {
                if (_aiStates[i].enableFixedUpdate)
                {
                    _aiStates[i].FixedUpdateExecute();
                }
                else
                {
                    _aiStates[i].enableFixedUpdate = true;
                }
            }
        }
    }

    public int TriggerBeHurt(int chp, AgentSkill agentSkill, HurtType hurtType,bool useBuff = true)
    {
        int hp = chp;
        foreach (var item in buffMaps)
        {
            if (item.Value.trigger_beHurt)
            {
                hp = item.Value.TriggerBeHurt(hp, agentSkill, hurtType);
            }
        }
        if (hp != 0&&agentSkill!=null&& useBuff)
        {
            SetBuffs(agentSkill.buffDatas);
        }
        return hp;
    }

    public void BoundPosition()
    {
        var pos = transform.position;
        if (pos.x < PathFindMgr._instance.minX)
        {
            pos.x = PathFindMgr._instance.minX;
        }
        if (pos.x > PathFindMgr._instance.maxX)
        {
            pos.x = PathFindMgr._instance.maxX;
        }
        if (pos.y < PathFindMgr._instance.minY)
        {
            pos.y = PathFindMgr._instance.minY;
        }
        if (pos.y > PathFindMgr._instance.maxY)
        {
            pos.y = PathFindMgr._instance.maxY;
        }
        transform.position = pos;
    }

    public void JumpToSky()
    {
        if (jump_time < stopTime)
        {
            jump_time = Time.fixedDeltaTime + jump_time;
            role_height = (-(jump_time * (10f / maxHeight) - 1) * jump_time * (10f / maxHeight)) * maxHeight + base_role_height;
            if (role_height < base_role_height)
            {
                role_height = base_role_height;
                jump_time = 10000;
            }
        }

    }
    private void UpdateDeathStrikeFly()
    {
        JumpToSky();
        if (IsInStrikeFly())
        {
            transform.Translate(strikeFlyDir * Time.fixedDeltaTime);
            BoundPosition();
            aICollider.UpdateAIClollider(transform.position.x, transform.position.y, role_height, null);
        }
        else
        {
            moveSpeed = agentData.move_speed * agentTempData.stateMoveSpeed * agentTempData.iceSpeed;
            if (isFrozen|| isDizz)
            {
                aICollider.UpdateAIClollider(transform.position.x, transform.position.y, role_height, null);
            }
        }
    }

    Dictionary<int, AIStateData> nextIndexs = new Dictionary<int, AIStateData>();
    private AIStateData nextCondAIState;
    private void UpdateSortState()
    {
        nextIndexs.Clear();
        nextCondAIState = null;
        foreach (var item in _statesType)
        {
            nextCondAIState = _aiStates[item.Key].TryNextCond();
            if (nextCondAIState!=null)
            {
                nextIndexs.Add(item.Key, nextCondAIState);

            }
            else if (_aiStates[item.Key].TryRestCond())
            {
                AIStateData index = _statesType[item.Key].firstAIState;
                if (index != null) { nextIndexs.Add(item.Key, index); }
    
            }
        }
        foreach (var item in nextIndexs)
        {
            AIStateData nextAisd = item.Value;
            nextAisd.SetNextTime(curTime);
            nextAisd.SetNextCondtionTime(curTime);
            _statesType[item.Key] = nextAisd;
            if (_aiStates.ContainsKey(item.Key))
            {
                _aiStates[item.Key].Exit();
                _aiStates[item.Key] = GetAICode(nextAisd);
            }
            else
            {
                _aiStates.Add(item.Key, GetAICode(nextAisd));
            }
        }
        //if (nextIndexs.Count > 0)
        //{
        //    UpdateSortState();
        //}
       
    }

    private AIState GetAICode(AIStateData value)
    {
        AIState aIState;
        switch (value.aIStateType)
        {
            case AIStateType.Root:
                aIState = new RootState().Init(this, value);
                break;
            case AIStateType.Idle:
                aIState = new IdleState().Init(this, value);
                break;
            case AIStateType.Finding_Fllow:
                aIState = new FindingState().Init(this, value);
                break;
            case AIStateType.Finding_Fllow_Far:
                aIState = new Finding_FarState().Init(this, value);
                break;
            case AIStateType.Finding_Fllow_Leader:
                aIState = new Finding_FLeaderState().Init(this, value);
                break;
            case AIStateType.Finding_Fixed_Point:
                aIState = new FixedPointState().Init(this, value);
                break;
            case AIStateType.common_meleeAttack:
                aIState = new MeleeAttack().Init(this, value);
                break;
            case AIStateType.common_remoteAttack:
                aIState = new RemoteAttack().Init(this, value);
                break;
            case AIStateType.Finding_Patrol:
                aIState = new PatrolState().Init(this, value);
                break;
            case AIStateType.Finding_NormalPatrol:
                aIState = new NormalPatrolState().Init(this, value);
                break;
            case AIStateType.OrderCompose:
                aIState = new OrderState().Init(this, value);
                break;
            case AIStateType.RandomCompose:
                aIState = new RandomState().Init(this, value);
                break;
            case AIStateType.Condition:
                aIState = new ConditionState().Init(this, value);
                break;
            case AIStateType.ResetRoot:
                aIState = new ResetRootState().Init(this, value);
                break;
            case AIStateType.ChangeAI:
                aIState = new ChangeAIState().Init(this, value);
                break;
            case AIStateType.SearchEnemy:
                aIState = new SearchEnemyState().Init(this, value);
                break;
            case AIStateType.SearchFriend:
                aIState = new SearchFriendState().Init(this, value);
                break;
            case AIStateType.CD:
                aIState = new CDState().Init(this, value);
                break;
            case AIStateType.GlobalCD:
                aIState = new CDGlobalState().Init(this, value);
                break;
            case AIStateType.ShareCD:
                aIState = new CDShareState().Init(this, value);
                break;
            case AIStateType.ResetLocalCD:
                aIState = new CDResetLocal().Init(this, value);
                break;
            case AIStateType.ResetGlobalCD:
                aIState = new CDResetGlobal().Init(this, value);
                break;
            case AIStateType.Debug:
                aIState = new DebugState().Init(this, value);
                break;
            default:
                ZLogUtil.LogError("请初始化对应的脚本");
                return null;
        }
        return aIState;
    }
    public AnimationClip GetAnimation(string name)
    {
        ZLogUtil.Log(name);
        return animationClipMap[name];
    }

    public float GetAnimationLength(string name)
    {
        float time = animationClipMap[name].length;
        return time;
    }
    public float GetAnimationIdle2Length(string name)
    {
        float time = 0;
        switch (name)
        {
            case GameAnimationName.animationName_2_Attack_Normal:
                if (animationClipMap.ContainsKey(GameAnimationName.animationName_idle2attack))
                {
                    time += animationClipMap[GameAnimationName.animationName_idle2attack].length;
                }
                break;
            case GameAnimationName.animationName_5_Skill_Normal:
                if (animationClipMap.ContainsKey(GameAnimationName.animationName_idle2skill))
                {
                    time += animationClipMap[GameAnimationName.animationName_idle2skill].length;
                }
                break;
        }
        return time;
    }
    public float GetAnimation2IdleLength(string name)
    {
        float time = 0;
        switch (name)
        {
            case GameAnimationName.animationName_2_Attack_Normal:

                if (animationClipMap.ContainsKey(GameAnimationName.animationName_attack2idle))
                {
                    time += animationClipMap[GameAnimationName.animationName_attack2idle].length;
                }
                break;
            case GameAnimationName.animationName_5_Skill_Normal:

                if (animationClipMap.ContainsKey(GameAnimationName.animationName_skill2idle))
                {
                    time += animationClipMap[GameAnimationName.animationName_skill2idle].length;
                }
                break;
        }
        return time;
    }
    //该对象是否处于击飞
    public bool IsInStrikeFly()
    {
        return role_height > base_role_height;
    }
    //该对象可以被击飞
    public bool CanBeGroundStrikeFly()
    {
        return base_role_height<=0;
    }

    public Vector2 strikeFlyDir;
    //击飞
    internal void StrikeFly(Vector2 dir,float flyHor,float flyUp)
    {
            //if (IsInStrikeFly())
            //{
            //    return;
            //}
           
           if (jump_time >10)
            {
                jump_time = 0;
            }
            else
            {
                if (jump_time > maxHeight * 0.1f * 0.5f)
                {
                    jump_time = maxHeight * 0.1f - jump_time;
                }
            }
           strikeFlyDir = dir * flyHor;
           maxHeight = flyUp;
           moveSpeed = 0;
    }

    #region
    public bool IsInBuff(BuffType buffType)
    {
        if (buffMaps.ContainsKey(buffType))
        {
            return true;
        }
        return false;
    }

    public void SetBuff(int id)
    {
       SetBuff(new BuffData());
    }
    public void SetBuff(int id,float time,bool isOverly)
    {
        var skill = new BuffData();
        skill.duration = time;
        skill.is_overly = isOverly;
        SetBuff(skill);
    }

    public void SetBuff(BuffData buff)
    {
        if (buffMaps.ContainsKey(buff.buffType))
        {
            var temp = buffMaps[buff.buffType];
            buffMaps[buff.buffType].UpdateBuff(buff);
        }
        else
        {
            if (ImmunityBuffMap.ContainsKey(buff.buffType))
            {
                //DamageNumberMgr._instance.Spawn(aICollider.GetHurtPos(), ImmunityBuffMap[buff.buffType].des);
                return;
            }
            var buffData = BuffManager.GetInstance().CreateBuff(this, buff);
            if (buffData != null)
            {
                buffMaps.Add(buff.buffType, buffData);
                if (buff.IsImmunityBuff())
                {
                    if (!ImmunityBuffMap.ContainsKey(buff.conflict_buff_type[0]))
                    {
                        ImmunityBuffMap.Add(buff.conflict_buff_type[0], buff);
                    }
                }
            }

        }
    }

    public void SetBuffs(List<BuffData> buffs)
    {
        if (buffs != null)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                SetBuff(buffs[i]);
            }
        }
    }

    public void CanelBuff(BuffType buffType)
    {
        if (buffMaps.ContainsKey(buffType))
        {
            RemoveImmunityBuff(buffType);
            buffRenderMgr.RemoveBuff(buffType);
            buffMaps[buffType].RemoveBuffEffect();
            if (!readyClear.ContainsKey(buffType))
            {
                readyClear.Add(buffType, true);
            }
        }
    }
    public void ClearAllBuff()
    {
        foreach (var item in buffMaps)
        {
            RemoveImmunityBuff(item.Key);
            buffRenderMgr.RemoveBuff(item.Key);
            item.Value.RemoveBuffEffect();
            if (!readyClear.ContainsKey(item.Key))
            {
                readyClear.Add(item.Key, true);
            }
        }

    }


    private void RemoveImmunityBuff(BuffType buffType)
    {
        BuffType removeType = BuffType.None;
        foreach (var item in ImmunityBuffMap)
        {
            if(buffType == item.Key)
            {
                removeType = item.Key;
            }
        }
        if (ImmunityBuffMap.ContainsKey(removeType))
        {
            ImmunityBuffMap.Remove(removeType);
        }
    }

    #endregion
    #region 移动函数，外部不能直接调用transform
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        BoundPosition();
        aICollider.UpdateAIClollider(transform.position.x, transform.position.y, role_height, null);
    }  
    public void SetCenterPosition(Vector3 pos)
    {
        transform.position = pos - aICollider.bodyBox.GetOffset3();
        BoundPosition();
        aICollider.UpdateAIClollider(transform.position.x, transform.position.y, role_height, null);
    }
    public void Translate(Vector3 dir, float speed)
    {
        transform.Translate(dir * speed);
        BoundPosition();
        aICollider.UpdateAIClollider(transform.position.x, transform.position.y, role_height, null);
    }
    public void SetAnimator(bool hide)
    {
        animator.gameObject.SetActive(hide);
    }

    public void SetInvisible(bool hide)
    {
        if (animator != null)
        {
            SetRenderInvisible(hide);
            effectTf.gameObject.SetActive(hide);
            effectBackTf.gameObject.SetActive(hide);
            uiTransform.gameObject.SetActive(hide);
            aICollider.shadowTf.gameObject.SetActive(hide);
            aICollider.SetDead(!hide);
            isDead = !hide;
            isOpen = hide;
        }

    }

    public void SetRenderInvisible(bool hide)
    {
        render.enabled = hide;
        buffRenderMgr.show_render = hide;
    }
    #endregion

    #region
    private float ghostTime = 0;
    private float ghostInterTime = 0.02f;
    private Color alphaColor = new Color(1,1,1,0);
    private void Ghost()
    {
        if (ghost)
        {
            ghostTime += Time.fixedDeltaTime;
            if (ghostTime > ghostInterTime)
            {
                ghostTime = 0;
                GameObject obj = ZGameObjectPool.Pop(AIConst.constGhost, () =>
                {
                    return PrefabUtils.Instance(AIConst.constGhost);
                });
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                obj.transform.localScale = render.transform.lossyScale;
                obj.transform.position = render.transform.position;
                spriteRenderer.color = Color.white;
                spriteRenderer.sprite = render.sprite;
                spriteRenderer.sortingOrder = render.sortingOrder;
                obj.SetActive(true);
                //spriteRenderer.DOColor(alphaColor, 0.5f).SetUpdate(UpdateType.Manual).OnComplete(()=> {
                //    ZGameObjectPool.Push(AIConst.constGhost, obj);
                //});
  
            }
            SetPosition(Vector3.MoveTowards(transform.position, ghostPos, Time.fixedDeltaTime * ghostSpeed));
            if (transform.position == ghostPos)
            {
                ghost = false;
                animator.enabled = true;
                unmatched = false;
                ghostCallBack?.Invoke();
            }
        }
        else
        {
            ghostTime = 0;
        }
    }
    private float ghostSpeed = 40f;
    private Vector3 ghostPos;
    private Action ghostCallBack;
    public void OpenGhost(Vector3 targetPos,Action callBack,float speed = 40)
    {
        ghostSpeed = speed;
        ghostCallBack = callBack;
        ghost = true;
        unmatched = true;
        animator.enabled = false;
        if (targetPos.x < PathFindMgr._instance.minX)
        {
            targetPos.x = PathFindMgr._instance.minX;
        }
        if (targetPos.x > PathFindMgr._instance.maxX)
        {
            targetPos.x = PathFindMgr._instance.maxX;
        }
        if (targetPos.y < PathFindMgr._instance.minY)
        {
            targetPos.y = PathFindMgr._instance.minY;
        }
        if (targetPos.y > PathFindMgr._instance.maxY)
        {
            targetPos.y = PathFindMgr._instance.maxY;
        }
        ghostPos = targetPos;
    }

    public bool GetGhost()
    {
        return ghost;
    }

   
    #endregion
    #region 跟随特效
    public void ShowEffect(EffectData effectData)
    {
        if (!effectKeepMap.ContainsKey(effectData.path))
        {
            var obj = ZGameObjectPool.Pop(effectData.path, () => {
                return PrefabUtils.Instance(effectData.path);
            });
            effectData.effectObj = obj;
            if (effectData.isForce)
            {
                obj.transform.SetParent(effectTf);
            }
            else
            {
                obj.transform.SetParent(effectBackTf);
            }
            obj.transform.localPosition = effectData.pos;
            obj.transform.localScale = Vector3.one;
            obj.SetActive(true);
            effectKeepMap.Add(effectData.path, effectData);
            //Timer.Register(effectData.keepTime, () => {
            //    obj.transform.SetParent(null);
            //    ZGameObjectPool.Push(effectData.path, obj);
            //}, null, false, false, this);
        }

    }
    public void HideEffect(EffectData effectData)
    {
        if (effectKeepMap.ContainsKey(effectData.path))
        {
            if (effectData.effectObj != null)
            {
                effectData.effectObj.transform.SetParent(null);
                ZGameObjectPool.Push(effectData.path, effectData.effectObj);
            }
            effectKeepMap.Remove(effectData.path);
        }
    }
    public void FllowEffectScale()
    {
        foreach (var item in effectKeepMap)
        {
            item.Value.effectObj.transform.localScale = transform.localScale.x > 0 ? AIConst.constXRight : AIConst.constXLeft;
        }
    }

    public void ReleaseEffect()
    {
        foreach (var item in effectKeepMap)
        {
            item.Value.effectObj.transform.SetParent(null);
            ZGameObjectPool.Push(item.Value.path, item.Value.effectObj);
        }
    }

    #endregion
    public bool IsBreakingSkill()
    {
        return isDizz || trueDeath;
    }
    public bool IsBreakingFllowSkill()
    {
        return isDizz || trueDeath;
    }

    public void ResetSpeed()
    {
        //animator.speed = agentTempData.iceSpeed;
    }



    #region 动画回调
    public Action attackStartFunction;
    public Action attackHalfFunction;
    public Action attackEndFunction;
    public Action skillStartFunction;
    public Action skillEndFunction;
    public Action skillEndEndFunction;
    #endregion

}

public class AgentTempData
{
    public int patrolTimes;
    public bool isAttacking;
    public float stateMoveSpeed;
    public Dictionary<int, float> cdMap;//用于节点cd功能
    public Dictionary<int, GlobalCD> globalCdMap;//用于节点全局cd功能
    public bool absHp;
    public bool findStop;
    public float aiTotalTime;
    public float iceSpeed;//动作缓慢

    public AgentTempData()
    {
        patrolTimes = 0;
        isAttacking = false;
        findStop = false;
        stateMoveSpeed = 1;
        iceSpeed = 1;
        cdMap = new Dictionary<int, float>();
        globalCdMap = new Dictionary<int, GlobalCD>();
        absHp = false;
    }

}

public class GlobalCD{
    public float time;
    public float cd;

    public GlobalCD(float time,float cd)
    {
        this.time = time;
        this.cd = cd;
    }
}
