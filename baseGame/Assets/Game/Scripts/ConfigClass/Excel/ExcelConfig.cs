//脚本自动生成，不要进行修改
using System.Collections.Generic;
using UnityEngine;
using Table;

public class ExcelConfig : Singleton<ExcelConfig>
{
    ExcelData excelData = new ExcelData();
    private ExcelConfig()
    {
    }

    public ExcelData GetExcelData()
    {
        return excelData;
    }

    public void InitExcelData<T>(List<T> list)
    {
        excelData.InitData(list);
    }

    public void LoadAllExcel()
    {
        ExcelLoader excelLoader = new GameObject("ExcelLoader").AddComponent<ExcelLoader>();
        excelLoader.LoadExcel();
    }

    #region 外部调用
    public static excel_roledata Get_excel_roledata(int id)    {
      return  Instance.GetExcelData().excel_roledataMap[id];
    }
    #endregion
}