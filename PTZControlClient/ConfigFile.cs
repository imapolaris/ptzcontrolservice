using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PTZControlClient
{
    public static class ConfigFile<T> where T : class
    {
        static public T FromFile(string fileName)
        {
            try
            {
                if (!System.IO.File.Exists(fileName))
                {
                    Common.Log.Logger.Default.Warn("未能找到文件：" + fileName);
                    return null;
                }
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    return xs.Deserialize(fs) as T;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Logger.Default.Error("读取异常：" + fileName + Environment.NewLine + ex.Message);
            }
            return null;
        }

        static public bool SaveToFile(string fileName, T config)
        {
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(T));
                    xs.Serialize(fs, config);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Common.Log.Logger.Default.Error("保存异常：" + fileName + Environment.NewLine + ex.Message);
            }

            return false;
        }
    }
}
