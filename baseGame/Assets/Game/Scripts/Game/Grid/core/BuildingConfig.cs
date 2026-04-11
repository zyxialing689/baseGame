using UnityEngine;

public class BuildingConfig : MonoBehaviour
{
    [Header("占地")]
    public int width = 1;
    public int height = 1;

    [Header("建造类型")]
    public bool isContinuous = false; // ⭐ 树木用

    public string prefabId;// ⭐ 预制体类型id

}