using UnityEngine;

public class GPUAnimDemoSpawner : MonoBehaviour
{
    public GPUAnimManager manager;
    public int count = 200;
    public Vector3 startPosition = new Vector3(100f, 100f, 0f);
    public int columns = 20;
    public Vector2 spacing = new Vector2(1.5f, 1.5f);
    public Vector2 scaleRange = new Vector2(3f, 4f);
    public Vector2 speedRange = new Vector2(1f, 2f);
    public bool randomStartTime = true;
    public bool clearBeforeSpawn = true;

    private int[] roleIds;

    private void Start()
    {
        Spawn();
    }

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        if (manager == null)
        {
            manager = GetComponent<GPUAnimManager>();
        }

        if (manager == null)
        {
            Debug.LogWarning("GPUAnimDemoSpawner needs a GPUAnimManager.");
            return;
        }

        if (clearBeforeSpawn)
        {
            manager.ClearRoles();
        }

        roleIds = new int[count];
        int safeColumns = Mathf.Max(1, columns);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = startPosition + new Vector3(
                (i % safeColumns) * spacing.x,
                (i / safeColumns) * spacing.y,
                0f
            );

            float scale = Random.Range(scaleRange.x, scaleRange.y);
            int id = manager.CreateRole(position, scale);
            roleIds[i] = id;

            manager.SetSpeed(id, Random.Range(speedRange.x, speedRange.y));
            if (randomStartTime)
            {
                manager.SetAnimTime(id, Random.value * 3f);
            }
        }
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        if (manager != null)
        {
            manager.ClearRoles();
        }
    }
}
