using TMPro;
using UnityEngine;

public class DamageNumberMgr : MonoBehaviour
{
    //public List<DamageNumber> damageNumbers;
    public static DamageNumberMgr _instance;
    private Transform uiParent;
    void Awake()
    {
        _instance = this;
    }

    public void SetUIParent(Transform tf)
    {
        uiParent = tf;
    }
    string path = "__hp/hurtHp";
    public void Init()
    {
        ZGameObjectPool.Init(path, () =>
        {
            var obj = PrefabUtils.Instance(path);
            obj.transform.SetParent(uiParent);
            obj.transform.position = new Vector3(-100, 0, 0);
            obj.GetComponent<TextMeshProUGUI>().text = "";
            obj.GetComponent<DamageNumber>().InitAnim();
            obj.SetActive(false);

            return obj;
        });
    }
    public void Spawn(Vector3 pos, string des)
    {
        GameObject obj = ZGameObjectPool.Pop(path, () =>
        {
            return PrefabUtils.Instance(path);
        });
        obj.transform.SetParent(uiParent);
        obj.transform.position = pos;

        obj.GetComponent<TextMeshProUGUI>().color = Color.white;
        obj.GetComponent<TextMeshProUGUI>().text = des;

        obj.SetActive(true);
        obj.GetComponent<DamageNumber>().Play();
    }
    public void Spawn(Vector3 pos, float number)
    {
        GameObject obj = ZGameObjectPool.Pop(path, () =>
        {
            return PrefabUtils.Instance(path);
        });
        obj.transform.SetParent(uiParent);
        obj.transform.position = pos;
        if (number < 0)
        {
            obj.GetComponent<TextMeshProUGUI>().color = Color.red;
            obj.GetComponent<TextMeshProUGUI>().text = number.ToString();
        }
        else
        {

            obj.GetComponent<TextMeshProUGUI>().color = Color.green;
            obj.GetComponent<TextMeshProUGUI>().text = "+" + number.ToString();
        }

        obj.SetActive(true);
        obj.GetComponent<DamageNumber>().Play();
    }
    public void Spawn(AICollider aICollider, float number)
    {
        GameObject obj = ZGameObjectPool.Pop(path, () =>
        {
            return PrefabUtils.Instance(path);
        });
        obj.transform.SetParent(uiParent);
        obj.transform.position = aICollider.GetHurtPos();
        if (number < 0)
        {
            obj.GetComponent<TextMeshProUGUI>().color = Color.red;
            obj.GetComponent<TextMeshProUGUI>().text = number.ToString();
        }
        else 
        { 

            obj.GetComponent<TextMeshProUGUI>().color = Color.green;
            obj.GetComponent<TextMeshProUGUI>().text = "+"+number.ToString();
        }

        obj.SetActive(true);
        obj.GetComponent<DamageNumber>().Play();
    }
}
