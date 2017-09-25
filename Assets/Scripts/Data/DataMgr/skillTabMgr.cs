using System.Collections;
using System.Collections.Generic;
using ExcelParser;
/// <summary>
/// 自动生成类，请不要修改;
/// </summary>
public partial class skillTabMgr : DataMgrBase<skillTabMgr> {


	protected override string GetXlsxPath ()
	{
		return "skillTab.txt";
	}


	protected override System.Type GetBeanType ()
	{
		return typeof(skillTabBean);
	}


	public skillTabBean GetDataById(object id)
	{
		IDataBean dataBean = _GetDataById(id);

		if(dataBean!=null)
		{
			return (skillTabBean)dataBean;
		}else{
			return null;
		}
	}



}
