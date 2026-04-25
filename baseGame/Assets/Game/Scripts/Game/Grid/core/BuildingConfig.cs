using UnityEngine;

public class BuildingConfig : MonoBehaviour
{
    [Header("占地")]
    public int width = 1;
    public int height = 1;

    [Header("建造类型")]
    public bool isContinuous = false; // ⭐ 树木用

    public string prefabId;// ⭐ 预制体类型id


    public int extraSort;
    public bool includeChildren = true;
    public bool useYAsDepth = true;
    public float yToDepthScale = 0.01f;
    public float depthOffset;
    private SpriteRenderer[] spriteRenderers;


    public void Refresh()
    {
        spriteRenderers = includeChildren
            ? GetComponentsInChildren<SpriteRenderer>(true)
            : GetComponents<SpriteRenderer>();
    }

    public void SetOrder(Vector3 pos)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            Refresh();
        }

        int sortingOrder = SortUtils.SetSoringBody(pos, extraSort);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].sortingOrder = sortingOrder;
            }
        }

        if (useYAsDepth)
        {
            Vector3 position = transform.position;
            position.z = depthOffset + pos.y * yToDepthScale;
            transform.position = position;
        }
    }
}
