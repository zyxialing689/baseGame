using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffRenderData
{
    public bool have_render;
    public SpriteRenderer sr;
    public BuffRenderData(bool have_render,SpriteRenderer sr)
    {
        this.have_render = have_render;
        this.sr = sr;
    }
}
public class BuffRenderMgr : MonoBehaviour
{
    Dictionary<BuffType, BuffRenderData> buffMaps;
    Dictionary<BuffType, BuffRenderData> showBuffs;
    private SpriteRenderer parentSR;
    private GameObject buffPrefab;
    public bool show_render;
    public void Init()
    {
        show_render = true;
        parentSR = transform.parent.Find("render").GetComponent<SpriteRenderer>();
        buffPrefab = transform.GetChild(0).gameObject;
        buffPrefab.transform.localPosition = parentSR.transform.localPosition;
        buffPrefab.transform.localEulerAngles = parentSR.transform.localEulerAngles;
        buffPrefab.transform.localScale = parentSR.transform.localScale;
        buffMaps = new Dictionary<BuffType, BuffRenderData>();
        showBuffs = new Dictionary<BuffType, BuffRenderData>();
        var buffs = Enum.GetValues(typeof(BuffType));
        //foreach (var item in buffs)
        //{
        //    int buffID = (int)item;
        //    if (buffID <= 0) continue;
        //    var buffTable = ExcelConfig.Get_excel_buffdata(buffID);
        //    if (buffTable.have_render)
        //    {
        //        var obj = Instantiate(buffPrefab, transform);
        //        obj.transform.localPosition = buffPrefab.transform.localPosition;
        //        var sr = obj.GetComponent<SpriteRenderer>();
        //        sr.material = ResHanderManager.Instance.GetMaterial(buffTable.material_path);
        //        buffMaps.Add((BuffType)item, new BuffRenderData(true,sr));
        //    }
        //    else
        //    {
        //        buffMaps.Add((BuffType)item, new BuffRenderData(false, null));
        //    }
        //}
    }

    public void AddBuff(BuffType buffType)
    {
        if (!showBuffs.ContainsKey(buffType))
        {
            BuffRenderData tempData = buffMaps[buffType];
            showBuffs.Add(buffType, tempData);
            if (tempData.have_render)
            {
                tempData.sr.gameObject.SetActive(true);
            }
        }
    }

    public void RemoveBuff(BuffType buffType)
    {
        if (showBuffs.ContainsKey(buffType))
        {
            BuffRenderData tempData = showBuffs[buffType];
            if (tempData.have_render)
            {
                tempData.sr.gameObject.SetActive(false);
            }
            showBuffs.Remove(buffType);
        }
    }

    public void _Update()
    {
        foreach (var item in showBuffs)
        {
            if (item.Value.have_render)
            {
                item.Value.sr.sprite = parentSR.sprite;
                item.Value.sr.enabled = show_render;
            }
           
        }
    }

}
