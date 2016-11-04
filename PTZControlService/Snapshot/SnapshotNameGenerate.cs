using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public static class SnapshotNameGenerate
    {
        public static string GetFullName(DynamicTarget target, DateTime time)
        {
            string name = GetShortName(target);
            string fileName = System.IO.Path.Combine(ConfigSettings.ShotshopPath, GetJpgName(name, time));
            return fileName;
        }

        public static string GetJpgName(string targetName, DateTime time)
        {
            return string.Format("{0}_{1}.jpg", targetName, time.ToString("HHmmssfff"));
        }

        public static string GetShortName(DynamicTarget target)
        {
            string shortName = getFileName(target);
            string name = $"{DateTime.Now.Year}\\{DateTime.Now.Month}\\{DateTime.Now.Day}\\{getFileName(target)}";
            return name;
        }

        private static string getFileName(DynamicTarget target)
        {
            string name = target.Name;
            if (isInValidName(name))
            {
                name = target.Id;
                if (name.Substring(0, 9).Equals("SCUNION.."))
                    name = name.Substring(9);
            }
            name = name.Replace("  ", " ");
            if (name.Last() == ' ')
                name = name.Substring(0, name.Length - 1);
            Common.Log.Logger.Default.Info($"目标位置更新...ID:{target.Id}。名称:{name} Lon{Math.Round(target.Lon, 6)},Lat:{Math.Round(target.Lat, 6)},Sog:{Math.Round(target.Sog, 1)},Cog:{Math.Round(target.Cog, 2)},"
                + $" Heading:{target.Heading},Length:{target.Length},Width:{target.Width},Time:{target.Time}");
            return filterFileName(name);
        }

        static bool isInValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;
            return name.All(c => (c <= '9' && c >= '0'));//纯数字，为雷达识别目标编号
        }

        private static readonly char[] InvalidFileNameChars = new char[]{'\0','\u0001','\u0002','\u0003','\u0004','\u0005', '\u0006',
            '"','<','>','|',':','*','?','\\','/',
            '\a','\b','\t','\n','\v','\f','\r',
            '\u000e',
            '\u000f',
            '\u0010',
            '\u0011',
            '\u0012',
            '\u0013',
            '\u0014',
            '\u0015',
            '\u0016',
            '\u0017',
            '\u0018',
            '\u0019',
            '\u001a',
            '\u001b',
            '\u001c',
            '\u001d',
            '\u001e',
            '\u001f'
        };

        static string filterFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "";
            return InvalidFileNameChars.Aggregate(fileName, (current, c) => current.Replace(c + "", ""));
        }
    }
}
