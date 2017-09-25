using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;

namespace ExcelParser
{
    /// <summary>
    /// 第一列为key
    /// /数据管理的基类;
    /// 必要功能，读取文件。写入缓存数组;
    /// virtual  获取文件路径  
    /// virtual  获取对象类型
    /// virtual  根据下标获取对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataMgrBase<T> : IDataMgrBase where T : class, new()
    {
        private static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
                return _instance;
            }
        }



        public Dictionary<object, IDataBean> idDataDic = new Dictionary<object, IDataBean>();


        bool isInit = false;

        /// <summary>
        /// Inits the data.
        /// </summary>
        public void loadDataFile(string prePath )
        {
            if (prePath == null)
            {
                throw new Exception("没有根据平台指定前置 路径 "); 
            }
            if (isInit)
            {
                return;
            }
            //这个路径应当在 是解压之后存放的  Application.persistentDataPath 中  ;
            string filePath = prePath + "/"+GetXlsxPath();
            Type dataBeanType = GetBeanType();

//            FileInfo info = new FileInfo (filePath);
//            if (info == null)
//            {
//                return;
//            }
//            StreamReader fiSR = info.OpenText ();
            string dataTxt = File.ReadAllText(filePath);

            //第一行是属性类型
            //第二行是注释
            //第三行才是 标题
            dataTxt = dataTxt.Replace("\r", "");
            string[] hList = dataTxt.Split('\n');
            if (hList.Length > 3)
            {
                string[] types = hList[0].Split('\t');
                string[] titles = hList[2].Split('\t');


                for (int col = 3; col < hList.Length; col++)
                {
                    IDataBean dataBean = null;
                    object key = null;

                    string[] vals = hList[col].Split('\t');

                    if (vals.Length != titles.Length)
                    {
                        continue;
                    }


                    dataBean = (IDataBean) Activator.CreateInstance(dataBeanType);
                    for (int row = 0; row < titles.Length; row++)
                    {
                        string titleName = titles[row];

                        if (string.IsNullOrEmpty(titleName))
                        {
                            continue;
                        }

                        string typeStr = types[row];
                        string valStr = vals[row];


                        if (string.IsNullOrEmpty(typeStr))
                        {
                            continue;
                        }

                        string propertyName = titleName.Substring(0, 1).ToUpper() + titleName.Substring(1);

                        PropertyInfo prop =
                            dataBeanType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

                        object val = Convert.ChangeType(valStr, prop.PropertyType);

                        prop.SetValue(dataBean, val, null);

                        //set dictionary id
                        if (row == 0)
                        {
                            key = val;
                        }
                    }

                    if (dataBean != null)
                    {
                        idDataDic.Add(key, dataBean);
                    }
                }
            }

            isInit = true;
        }


        /// <summary>
        /// Gets the xlsx txt path. Need overwrite.
        /// </summary>
        /// <returns>The xlsx path.</returns>
        protected virtual string GetXlsxPath()
        {
            return "";
        }


        /// <summary>
        /// Gets the type of the bean.Need overwrite.
        /// </summary>
        /// <returns>The bean type.</returns>
        protected virtual Type GetBeanType()
        {
            return null;
        }

        public IDataBean _GetDataById(object id)
        {
            if (idDataDic.ContainsKey(id))
            {
                return idDataDic[id];
            }
            else
            {
                return null;
            }
        }
    }
}