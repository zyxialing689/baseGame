using UnityEngine;

public class BuildTestInput : MonoBehaviour
{
    public static BuildTestInput instance;
    public GameObject house;
    public GameObject tree;
    public GameObject house2;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // ===== 建造 =====

        // if (Input.GetKeyDown(KeyCode.Alpha1))
        // {
        //     Debug.Log("进入建筑模式");
        //     BuildSystem.Instance.StartBuild(house);
        // }

        // if (Input.GetKeyDown(KeyCode.Alpha2))
        // {
        //     Debug.Log("进入树木模式");
        //     BuildSystem.Instance.StartBuild(tree);
        // }
        // if (Input.GetKeyDown(KeyCode.Alpha3))
        // {
        //     Debug.Log("进入树木模式");
        //     BuildSystem.Instance.StartBuild(house2);
        // }

        // // 右键退出建造
        // if (Input.GetMouseButtonDown(1))
        // {
        //     BuildSystem.Instance.StopBuild();
        //     BuildSystem.Instance.StopDelete();
        //     Debug.Log("退出建造/删除模式");
        // }

        // // ===== 删除模式 =====

        // if (Input.GetKeyDown(KeyCode.Delete))
        // {
        //     BuildSystem.Instance.StartDelete(false); // 单次删除
        //     Debug.Log("进入单次删除模式");
        // }

        // if (Input.GetKeyDown(KeyCode.Delete) && Input.GetKeyDown(KeyCode.LeftAlt))
        // {
        //     BuildSystem.Instance.StartDelete(true); // 连续删除
        //     Debug.Log("进入连续删除模式");
        // }

        // // ===== 存档 =====

        // if (Input.GetKeyDown(KeyCode.S))
        // {
        //     BuildSystem.Instance.SaveSystem.Save();
        //     Debug.Log("手动保存");
        // }
    }
}