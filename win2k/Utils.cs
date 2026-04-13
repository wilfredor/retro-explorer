using System;

namespace ex_plorer
{
    public static class Utils
    {
        private const long EB = 1152921504606846976L;
        private const long PB = 1125899906842624L;
        private const long TB = 1099511627776L;
        private const long GB = 1073741824L;
        private const long MB = 1048576L;
        private const long KB = 1024L;

        public static string ReadableFileSize(long val)
        {
            long num = ((val < 0) ? (-val) : val);
            string arg;
            double num2;
            if (num >= 1152921504606846976L)
            {
                arg = "EB";
                num2 = val >> 50;
            }
            else if (num >= 1125899906842624L)
            {
                arg = "PB";
                num2 = val >> 40;
            }
            else if (num >= 1099511627776L)
            {
                arg = "TB";
                num2 = val >> 30;
            }
            else if (num >= 1073741824)
            {
                arg = "GB";
                num2 = val >> 20;
            }
            else if (num >= 1048576)
            {
                arg = "MB";
                num2 = val >> 10;
            }
            else
            {
                if (num < 1024)
                {
                    return val.ToString("0") + " B";
                }
                arg = "KB";
                num2 = val;
            }
            num2 /= 1024.0;
            return num2.ToString("0.###") + " " + arg;
        }

        public static string FileSizeInKB(long val)
        {
            long num = (val + 1023) / 1024;
            return num.ToString("N0") + " KB";
        }

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null) return true;
            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i])) return false;
            }
            return true;
        }
    }
}
