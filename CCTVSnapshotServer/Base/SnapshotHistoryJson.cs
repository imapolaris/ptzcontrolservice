using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVSnapshotServer.Base
{
    public class SnapshotHistoryJson
    {
        public static readonly SnapshotHistoryJson Instance = new SnapshotHistoryJson();
        const string jsonName = "snapshot.history";
        string _dir = null;
        SnapshotHistoryInfo _history = new SnapshotHistoryInfo();
        //List<SnapshotInfo> _snapshotInfo = new List<SnapshotInfo>();
        string FileName { get { return Path.Combine(_dir, jsonName); } }

        private void init(string dir)
        {
            _dir = dir;
            loadHistory();
        }

        public void Add(SnapshotInfo info)
        {
            if (info == null)
                return;
            var dir = new FileInfo(info.FileName).DirectoryName;
            if (dir != _dir)
                init(dir);
            _history.History.Add(info);
            save();
        }

        private void loadHistory()//获取当前Json列表
        {
            _history = new SnapshotHistoryInfo();
            try
            {
                FileInfo fi = new FileInfo(FileName);
                if (fi.Exists)
                {
                    using (StreamReader sr = fi.OpenText())
                    {
                        string jStr = sr.ReadToEnd();
                        try
                        {
                            _history = JsonConvert.DeserializeObject<SnapshotHistoryInfo>(jStr);
                        }
                        catch
                        {
                            _history.History = JsonConvert.DeserializeObject<List<SnapshotInfo>>(jStr);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Log.Logger.Default.Error("读取历史数据失败！", ex);
            }
        }

        public void Mux(string id1, string id2)
        {
            Common.Log.Logger.Default.Warn($"目标融合：{id1}, {id2} - {DateTime.Now.TimeOfDay}\n");
            if (_history.AddMux(id1, id2, DateTime.Now))
                save();
        }

        private void save()
        {
            try
            {
                FileInfo fi = new FileInfo(FileName);
                if (!fi.Directory.Exists)
                    fi.Directory.Create();
                using (FileStream fs = new FileStream(fi.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        string str = JsonConvert.SerializeObject(_history);
                        sw.Write(str);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Log.Logger.Default.Error("保存历史数据失败！", ex);
            }
        }
    }
}