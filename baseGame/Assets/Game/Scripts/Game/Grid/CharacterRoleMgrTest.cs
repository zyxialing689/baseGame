using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRoleMgrTest : MonoBehaviour
{
    [Header("References")]
    public GpuRoleGpuManager manager;
    public GameObject[] charaterS;
    public int count = 1000;
    private readonly List<GpuRoleAgent> _created = new List<GpuRoleAgent>();

    void Start()
    {
        manager.autoRebuild = false;
        StartCoroutine(this.test());
    }

    IEnumerator test()
    {
        if (charaterS != null && charaterS.Length > 0)
        {
            for (int i = 0; i < count; i++)
            {
                var charater = charaterS[RandomMgr.Range(0, charaterS.Length)];
                GameObject obj = Instantiate(charater, transform);
                obj.transform.position = new Vector2(100, 100);
                // 先 disable 避免 Register 触发拓扑重建

                _created.Add(obj.GetComponent<GpuRoleAgent>());
                _created[i].RandomizeStyle(0.2f, 0.2f);
         
            }

       yield return null;
        }
        manager.autoRebuild = true;
        manager.Rebuild();
    }
}