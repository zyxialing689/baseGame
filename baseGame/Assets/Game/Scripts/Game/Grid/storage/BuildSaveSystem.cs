using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildSaveSystem
{
    private List<BuildingData> datas = new List<BuildingData>();

    private int idCounter = 1;

    // ⭐ prefab映射
    private Dictionary<string, int> prefabToId = new Dictionary<string, int>();
    private Dictionary<int, string> idToPrefab = new Dictionary<int, string>();

    private int prefabCounter = 1;

    string SavePath => Path.Combine(Application.persistentDataPath, "build.dat");

    // ================= 添加 =================

    public BuildingData AddBuilding(string prefabId, Vector2Int pos, int w, int h)
    {
        int prefabIntId = GetPrefabId(prefabId);

        BuildingData data = new BuildingData()
        {
            instanceId = idCounter++,
            prefabId = prefabId,
            prefabIntId = prefabIntId,
            x = pos.x,
            y = pos.y,
            width = w,
            height = h
        };

        datas.Add(data);
        return data;
    }

    public BuildingData AddBuilding(BuildingData source)
    {
        int prefabIntId = GetPrefabId(source.prefabId);

        BuildingData data = new BuildingData()
        {
            instanceId = source.instanceId,
            prefabId = source.prefabId,
            prefabIntId = prefabIntId,
            x = source.x,
            y = source.y,
            width = source.width,
            height = source.height
        };

        datas.Add(data);

        if (data.instanceId >= idCounter)
        {
            idCounter = data.instanceId + 1;
        }

        return data;
    }

    int GetPrefabId(string prefab)
    {
        if (!prefabToId.TryGetValue(prefab, out int id))
        {
            id = prefabCounter++;
            prefabToId[prefab] = id;
            idToPrefab[id] = prefab;
        }
        return id;
    }

    public List<BuildingData> GetAll()
    {
        return datas;
    }

    // ================= 保存（二进制） =================

    public void Save()
    {
        using (BinaryWriter bw = new BinaryWriter(File.Open(SavePath, FileMode.Create)))
        {
            // ⭐ 写Prefab表
            bw.Write(prefabToId.Count);
            foreach (var kv in prefabToId)
            {
                bw.Write(kv.Value);
                bw.Write(kv.Key);
            }

            // ⭐ 写建筑数量
            bw.Write(datas.Count);

            foreach (var d in datas)
            {
                bw.Write(d.instanceId);
                bw.Write(d.prefabIntId);

                bw.Write((short)d.x);
                bw.Write((short)d.y);
                bw.Write((short)d.width);
                bw.Write((short)d.height);
            }
        }

        Debug.Log("二进制保存完成：" + SavePath);
    }

    // ================= 加载 =================

    public void Load()
    {
        datas.Clear();

        if (!File.Exists(SavePath))
            return;

        using (BinaryReader br = new BinaryReader(File.Open(SavePath, FileMode.Open)))
        {
            // ⭐ 读Prefab表
            int prefabCount = br.ReadInt32();
            prefabToId.Clear();
            idToPrefab.Clear();

            for (int i = 0; i < prefabCount; i++)
            {
                int id = br.ReadInt32();
                string name = br.ReadString();

                prefabToId[name] = id;
                idToPrefab[id] = name;
            }

            // ⭐ 读建筑
            int count = br.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                BuildingData d = new BuildingData();

                d.instanceId = br.ReadInt32();
                d.prefabIntId = br.ReadInt32();

                d.prefabId = idToPrefab[d.prefabIntId];

                d.x = br.ReadInt16();
                d.y = br.ReadInt16();
                d.width = br.ReadInt16();
                d.height = br.ReadInt16();

                datas.Add(d);

                if (d.instanceId >= idCounter)
                    idCounter = d.instanceId + 1;
            }
        }
    }
    public void RemoveBuilding(int instanceId)
    {
        datas.RemoveAll(d => d.instanceId == instanceId);
    }
}
