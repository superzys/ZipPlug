using System.Collections;
using ExcelParser;

/// <summary>
/// 自动生成类。不要修改
/// 数据表的第一列为key
/// </summary>
public class UserInfoBean : IDataBean {

    private int id;
    /// <summary>
    /// ID
    /// </summary>
    public int Id {
        get {
             return id;
        }
        set {
             id = value;
        }
    }

    private string name;
    /// <summary>
    /// 名称
    /// </summary>
    public string Name {
        get {
             return name;
        }
        set {
             name = value;
        }
    }

    private int sex;
    /// <summary>
    /// 性别
    /// </summary>
    public int Sex {
        get {
             return sex;
        }
        set {
             sex = value;
        }
    }

    private int role;
    /// <summary>
    /// 资源
    /// </summary>
    public int Role {
        get {
             return role;
        }
        set {
             role = value;
        }
    }
}
