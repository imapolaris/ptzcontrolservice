using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CCTVSnapshotServer.Util
{
    public sealed class LogHelperEx
    {
        private static readonly string filePath = @".\log.txt"; // 日志文件绝对路径

        /// <summary>
        /// 写入日志文件
        /// </summary>
        /// <param name="content">内容</param>
        public static void WriteLog(string content)
        {
            string strDir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(strDir))
            {
                Directory.CreateDirectory(strDir);
            }

            string strDate = string.Format("=================={0}==================", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            content = string.Format("{0}", content);

            using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.Unicode))
            {
                sw.WriteLine(strDate);
                sw.WriteLine(content);

                sw.Flush();
                sw.Close();
            }
        }

        /// <summary>
        /// 将异常信息写入日志文件
        /// </summary>
        /// <param name="ex">异常</param>
        public static void WriteLog(System.Exception ex)
        {
            StringBuilder strBuilder = new StringBuilder();

            while (ex != null)
            {
                strBuilder.Append(ex.Message + Environment.NewLine);

                ex = ex.InnerException;
            }

            WriteLog(strBuilder.ToString());
        }
    }
}
