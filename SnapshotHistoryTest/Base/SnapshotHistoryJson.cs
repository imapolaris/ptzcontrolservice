using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapshotHistoryTest.Base
{
    public static class SnapshotHistoryJson
    {
        public static SnapshotHistoryInfo ReadHistory(string FileName)//获取当前Json列表
        {
            SnapshotHistoryInfo history = new SnapshotHistoryInfo();
            FileInfo fi = new FileInfo(FileName);
            if (fi.Exists)
            {
                using (StreamReader sr = fi.OpenText())
                {
                    string jStr = sr.ReadToEnd();
                    try
                    {
                        history = JsonConvert.DeserializeObject<SnapshotHistoryInfo>(jStr);
                    }
                    catch
                    {
                        history.History = JsonConvert.DeserializeObject<List<SnapshotInfo>>(jStr);
                    }
                }
            }
            return history;
        }

        //private void save()
        //{
        //    FileInfo fi = new FileInfo(FileName);
        //    if (!fi.Directory.Exists)
        //        fi.Directory.Create();
        //    using (FileStream fs = new FileStream(fi.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
        //    {
        //        using (StreamWriter sw = new StreamWriter(fs))
        //        {
        //            string str = JsonConvert.SerializeObject(_snapshotInfo);
        //            sw.Write(str);
        //        }
        //    }
        //}
    }
}
