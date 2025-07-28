//脚本自动生成，不要进行修改
using System.Collections.Generic;
using Table;

public class ExcelData
{
    public Dictionary<int, excel_roledata> excel_roledataMap = new Dictionary<int, excel_roledata>();
    public List<excel_roledata> excel_roledataList = new List<excel_roledata>();
    public Dictionary<int, excel_roleskill> excel_roleskillMap = new Dictionary<int, excel_roleskill>();
    public List<excel_roleskill> excel_roleskillList = new List<excel_roleskill>();

    public void InitData<T>(List<T> list)
    {
        switch (typeof(T).Name)
        {
            case "excel_roledata":
                excel_roledataList = list as List<excel_roledata>;
                for (int i = 0; i < excel_roledataList.Count; i++)
                {
                    excel_roledataMap.Add(excel_roledataList[i].id, excel_roledataList[i]);
                }
                break;
            case "excel_roleskill":
                excel_roleskillList = list as List<excel_roleskill>;
                for (int i = 0; i < excel_roleskillList.Count; i++)
                {
                    excel_roleskillMap.Add(excel_roleskillList[i].id, excel_roleskillList[i]);
                }
                break;
        }
    }
}
