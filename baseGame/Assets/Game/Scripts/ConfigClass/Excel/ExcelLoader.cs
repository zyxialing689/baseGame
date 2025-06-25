//脚本自动生成，不要进行修改
using UnityEngine;
using Table;

public class ExcelLoader : MonoBehaviour
{
    public void LoadExcel()
    {
        ExcelConfig.Instance.InitExcelData(Config<excel_roledata>.InitConfig());
    }
}
