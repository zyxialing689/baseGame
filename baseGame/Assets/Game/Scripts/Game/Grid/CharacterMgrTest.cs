using System.Collections;
using UnityEngine;

public class CharacterMgrTest : MonoBehaviour
{
    public GameObject[] charaterS;
    public int count = 1000;

    void Start()
    {

        StartCoroutine(this.test());
    }

    IEnumerator test()
    {
        if (charaterS != null&&charaterS.Length>0)
        {
            for (int i = 0; i < count; i++)
            {
                var charater = charaterS[RandomMgr.Range(0,charaterS.Length)];
                GameObject obj = Instantiate(charater, transform);
                obj.transform.SetParent(transform);
                obj.transform.position = new Vector2(100, 100);
                yield return null;
            }
        }
    }


}