using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TestMonster
{
    public string name;
    public string path;
    public int id;
    public Vector2 poses;
    public TestMonster(string name, string path, int id, Vector2 poses)
    {
        this.name = name;
        this.path = path;
        this.id = id;
        this.poses = poses;
    }
}
public class GameCreater : MonoBehaviour
{
    public static GameCreater _instance;

    private Transform back;
    private Transform center;
    private Transform force;
    private Transform sceneUI;
    private Transform hpUI;
    private void Awake()
    {
        _instance = this;
        back = transform.Find("root/back");
        center = transform.Find("root/center");
        force = transform.Find("root/force");
        sceneUI = transform.Find("root/force/sceneUI");
        hpUI = transform.Find("root/force/sceneUI/hp");
    }


    public void CreatePlayer(TestMonster monster,bool greenUI,bool control)
    {
        var obj = PrefabUtils.Instance(monster.path);
        obj.transform.SetParent(center);
       
        var bar = greenUI ? PrefabUtils.Instance("__sceneUI/friend/roleBar") : PrefabUtils.Instance("__sceneUI/enemy/roleBar");
        var agent = obj.GetComponent<AIAgent>();
        agent.startPoses = monster.poses;
        agent.InitAgentData(monster.id, greenUI ? PlayerCamp.PlayerCampA : PlayerCamp.PlayerCampB);
        if (greenUI)
        {
            agent.transform.localScale = GameConst.constXLeft;
        }
        else
        {
            agent.transform.localScale = GameConst.constXRight;
        }
        var aiUI = agent.BindUI(bar);
        aiUI.SetParent(hpUI);
        aiUI.gameObject.layer = 7;
        aiUI.localScale = Vector3.one;
        agent.transform.position = agent.startPoses;
        if (control)
        {
            //obj.AddComponent<AIAttackControl>();
            //obj.AddComponent<AIMovement>();
            //obj.AddComponent<AgentKeyBoard>();
  

            obj.GetComponent<AIAgent>().isStake = true;
            obj.GetComponent<AIAgent>().attrData.SetMaxHp(10000);
            obj.GetComponent<AIAgent>().attrData.SetHp(10000);
        }
    }

}
