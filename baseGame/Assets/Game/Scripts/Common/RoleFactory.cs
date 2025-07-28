using UnityEngine;

public static class RoleFactory
{
     public static void GenerateRoleA(int id)
    {
        var path = ExcelConfig.Get_excel_roledata(id).prefab_path;
        GameObject obj = PrefabUtils.Instance(path);
        obj.transform.position = Vector3.zero;
        obj.GetComponent<AIAgent>().InitAgentData(id, PlayerCamp.PlayerCampA);
    }

    public static void GenerateRoleB(int id)
    {
        var path = ExcelConfig.Get_excel_roledata(id).prefab_path;
        GameObject obj = PrefabUtils.Instance(path);
        obj.transform.position = Vector3.zero;
        obj.GetComponent<AIAgent>().InitAgentData(id, PlayerCamp.PlayerCampB);
    }
}