using System.Collections;
using System.Collections.Generic;
using ExcelParser;
/// <summary>
/// 自动生成类，请不要修改;
/// </summary>
public partial class UserInfoMgr : DataMgrBase<UserInfoMgr> {


	protected override string GetXlsxPath ()
	{
		return "UserInfo.txt";
	}


	protected override System.Type GetBeanType ()
	{
		return typeof(UserInfoBean);
	}


	public UserInfoBean GetDataById(object id)
	{
		IDataBean dataBean = _GetDataById(id);

		if(dataBean!=null)
		{
			return (UserInfoBean)dataBean;
		}else{
			return null;
		}
	}



}
