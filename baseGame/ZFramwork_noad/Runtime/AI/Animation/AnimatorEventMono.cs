using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEventMono : MonoBehaviour
{
    AIAgent agent;
    public void Init(AIAgent agent)
    {
        this.agent = agent;
        AnimationClip animationClip = agent.GetAnimation(GameAnimationName.animationName_2_Attack_Normal);
        float animLength = agent.GetAnimationLength(GameAnimationName.animationName_2_Attack_Normal);
        animationClip.events = null;
        var clipEvent = new AnimationEvent();
        clipEvent.time = animLength*0.1f;
        clipEvent.functionName = "AttackStart";
        animationClip.AddEvent(clipEvent);

        var clipEvent2 = new AnimationEvent();
        clipEvent2.time = animLength*0.5f;
        clipEvent2.functionName = "AttackHalf";
        animationClip.AddEvent(clipEvent2);

        var clipEvent3 = new AnimationEvent();
        clipEvent3.time = animLength*0.8f;
        clipEvent3.functionName = "AttackEnd";
        animationClip.AddEvent(clipEvent3);



        animationClip = agent.GetAnimation(GameAnimationName.animationName_idle2skill);
        animLength = agent.GetAnimationLength(GameAnimationName.animationName_idle2skill);
        animationClip.events = null;
        clipEvent = new AnimationEvent();
        clipEvent.time = animLength * 0.8f;
        clipEvent.functionName = "SkillStart";
        animationClip.AddEvent(clipEvent);


        animationClip = agent.GetAnimation(GameAnimationName.animationName_skill2idle);
        animLength = agent.GetAnimationLength(GameAnimationName.animationName_skill2idle);
        animationClip.events = null;
        clipEvent = new AnimationEvent();
        clipEvent.time = animLength * 0.1f;
        clipEvent.functionName = "SkillEnd";
        animationClip.AddEvent(clipEvent);
        clipEvent = new AnimationEvent();
        clipEvent.time = animLength * 0.8f;
        clipEvent.functionName = "SkillEndEnd";
        animationClip.AddEvent(clipEvent);


        agent.animator.Rebind();
    }

    public void SkillStart()
    {
        ZLogUtil.LogError("SkillStart");
        agent.skillStartFunction?.Invoke();

    }
    public void SkillEnd()
    {
        ZLogUtil.LogError("SkillEnd");
        agent.skillEndFunction?.Invoke();
        agent.skillEndFunction = null;
    }
    public void SkillEndEnd()
    {
        ZLogUtil.LogError("SkillEndEnd");
        agent.skillEndEndFunction?.Invoke();
    }
    public void AttackStart()
    {
        ZLogUtil.LogError("AttackStart");
        agent.attackStartFunction?.Invoke();
        agent.attackStartFunction = null;
    }
    public void AttackHalf()
    {
        ZLogUtil.LogError("AttackHalf");
        agent.attackHalfFunction?.Invoke();
        agent.attackHalfFunction = null;
    }
    public void AttackEnd()
    {
        ZLogUtil.LogError("AttackEnd");
        agent.attackEndFunction?.Invoke();

    }

}
