using System.Collections;
using UnityEngine;

public class CharacterMgrTest : MonoBehaviour
{
    public GameObject charater;
    public int count = 1000;

    void Start()
    {

        StartCoroutine(this.test());
    }

    IEnumerator test()
    {
        if (charater != null)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(charater, transform);
                obj.transform.SetParent(transform);
                obj.transform.position = new Vector2(100, 100);
                yield return null;
            }
        }
    }


}