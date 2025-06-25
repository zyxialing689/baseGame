using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TextAnimData
{
    public Transform transform;
    public TextMeshProUGUI text;
    public CanvasGroup canvasGroup;
    public TextAnimType animType;


    public TextAnimData(Transform tf,TextMeshProUGUI text,CanvasGroup cg,TextAnimType tay)
    {
        transform = tf;
        this.text = text;
        canvasGroup = cg;
        animType = tay;
    }
}


public class TextAnimParameter
{
    public float fadeOutVale;
    public float scaleToSize;
    public float scaleBackSize;
    public Vector3 moveToPos;
    public float moveToTime;
    public Vector3 moveBackPos;
    public float scaleToTime;
    public float scaleBackTime;
    public float moveBackTime;
    public float dealy;

    public float fadeOutTime;
}

public class BattleUtils:Singleton<BattleUtils>
{
    Dictionary<TextAnimType, TextAnimParameter> _animMap;

    private BattleUtils()
    {
        _animMap = new Dictionary<TextAnimType, TextAnimParameter>();

        AddType1();
        AddType2();

    }

    private void AddType1()
    {
        var anim = new TextAnimParameter();
        anim.scaleToSize = 1.2f;
        anim.scaleToTime = 0.15f;
        anim.scaleBackSize = 0f;
        anim.scaleBackTime = 0.2f;
        anim.fadeOutVale = 0;
        anim.fadeOutTime = 0.2f;
        anim.moveToPos = new Vector3(0.5f, 0.8f, 0);
        anim.moveToTime = 0.1f;
        anim.moveBackPos = new Vector3(0, 0, 0);
        anim.moveBackTime = 0;
        anim.dealy = 0.4f;
        _animMap.Add(TextAnimType.type1, anim);
    }

    private void AddType2()
    {
        var anim = new TextAnimParameter();
        anim.scaleToSize = 1.2f;
        anim.scaleToTime = 0.153f;
        anim.scaleBackSize = 0f;
        anim.scaleBackTime = 0.15f;
        anim.fadeOutVale = 0;
        anim.fadeOutTime = 0.5f;
        anim.moveToPos = new Vector3(0, 1f, 0);
        anim.moveToTime = 0.15f;
        anim.moveBackPos = new Vector3(0, 0, 0);
        anim.moveBackTime = 0;
        anim.dealy = 0.5f;
        _animMap.Add(TextAnimType.type2, anim);
    }

    public static Sequence GetDamageAnim(TextAnimData textAnimData,bool autoKill = true)
    {

        var animData = Instance._animMap[textAnimData.animType];
        animData.moveToPos.x = Random.Range(-0.4f,0.4f);
        animData.moveToPos.y = Random.Range(0.8f,1.2f);
        var sequence = DOTween.Sequence().SetAutoKill(autoKill).SetUpdate(UpdateType.Manual);
        sequence.Append(textAnimData.transform.DOScale(animData.scaleToSize, animData.scaleToTime));
        sequence.Join(textAnimData.transform.DOMove(textAnimData.transform.position+ animData.moveToPos,animData.moveToTime));
        sequence.AppendInterval(animData.dealy);
        sequence.Append(textAnimData.transform.DOScale(animData.scaleBackSize, animData.scaleBackTime));
        if(animData.moveBackTime>0)
        sequence.Join(textAnimData.transform.DOMove(textAnimData.transform.position + animData.moveBackPos, animData.moveBackTime));
 
        if (textAnimData.canvasGroup != null)
        {
            sequence.Append(textAnimData.canvasGroup.DOFade(animData.fadeOutVale, animData.fadeOutTime));
        }

        return sequence;
    }

}