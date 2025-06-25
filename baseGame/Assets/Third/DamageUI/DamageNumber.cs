using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTimer;
using Unity.Collections;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public string path;
    public TextAnimType animType;
    Sequence sequence;
    TextAnimData animData;


    public void InitAnim()
    {
        if (animData == null)
        {
            animData = new TextAnimData(transform, GetComponent<TextMeshProUGUI>(), GetComponent<CanvasGroup>(), animType);
        }
    }
    public void Play()
    {

        InitAnim();
        ResetData();
        sequence = BattleUtils.GetDamageAnim(animData);
        sequence.Play();

        sequence.OnComplete(OnComplete);





        //Timer.Register(1f, OnComplete, null, false, false, this);

    }

    private void ResetData()
    {
        animData.animType = (TextAnimType)Random.Range(0, 1);
        animData.transform.localScale = Vector3.zero;
        animData.canvasGroup.alpha = 1;
    }

    public void OnComplete()
    {
        ZGameObjectPool.Push(path, gameObject);
    }
}
