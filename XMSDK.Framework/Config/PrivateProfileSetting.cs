using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace XMSDK.Framework.Config
{
    public static class PrivateProfileSetting
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
            int size, string filePath);

        public static bool SaveSetting(string section, string key, string value, string iniFilePath)
        {
            var opStation = WritePrivateProfileString(section, key, value, iniFilePath);
            if (File.Exists(iniFilePath))
            {
                return opStation != 0;
            }

            return false;
        }

        public static string GetSetting(string section, string key, string iniFilePath, string def = "")
        {
            if (!File.Exists(iniFilePath)) return string.Empty;
            var temp = new StringBuilder(1024);
            GetPrivateProfileString(section, key, def, temp, 1024, iniFilePath);
            return temp.ToString();
        }
    }
}