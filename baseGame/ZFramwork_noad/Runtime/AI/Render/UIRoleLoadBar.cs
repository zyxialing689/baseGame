using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoleLoadBar : MonoBehaviour
{
    private Image _hp;
    private RectTransform _rt;
    private RectTransform _rtBg;
    private RectTransform _rtProgress;
    private void Awake()
    {
        _hp = transform.Find("progress").GetComponent<Image>();
        _rtBg = transform.Find("bg").GetComponent<RectTransform>();
        _rtProgress = transform.Find("progress").GetComponent<RectTransform>();
        _rt = transform.GetComponent<RectTransform>();
    }
    // Update is called once per frame
     public void UpdateHp(float hpProgress)
    {
        _hp.fillAmount = hpProgress;
    }

    public void UpdateLocalPos(Vector2 pos)
    {
        _rt.anchoredPosition = pos;
    }
    public void UpdateSize(Vector2 size)
    {
        _rtBg.sizeDelta = size;
        _rtProgress.sizeDelta = size;
    }
}
