using UnityEngine;

public class BuildTestInput : MonoBehaviour
{
    public GameObject house;
    public GameObject tree;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            BuildSystem.Instance.StartBuild(house, 2, 2, false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            BuildSystem.Instance.StartBuild(tree, 1, 1, true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BuildSystem.Instance.StopBuild();
        }
    }
}