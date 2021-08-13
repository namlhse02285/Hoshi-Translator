using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator
{
    class BuCommon
    {
        public static Encoding getEncodingFromString(string str)
        {
            if (str.Equals("utf-8-bom"))
            {
                return new System.Text.UTF8Encoding(true);
            }
            if (str.Equals("utf-8"))
            {
                return new System.Text.UTF8Encoding(false);
            }
            else
            {
                return Encoding.GetEncoding(str);
            }
        }

        public static int timeStringToMs(string timeStr)
        {//Format HH:MM:SS.UU
            if(null== timeStr) { return 0; }
            string[] splitedTimeStr = Regex.Split(timeStr, ":");
            int ret = 0;
            try
            {
                int hours = Int32.Parse(splitedTimeStr[0]) * 60 * 60 * 1000;
                int minutes = Int32.Parse(splitedTimeStr[1]) * 60 * 1000;
                int seconds = Convert.ToInt32(Double.Parse(splitedTimeStr[2]) * 1000);
                ret= hours + minutes + seconds;
            }
            catch { return 0; }
            return ret;
        }

        public static string convertCharacter(string input, List<char> fromList, List<char> toList)
        {
            StringBuilder toWriteIn = new StringBuilder();

            char[] allCharInLine = input.ToCharArray();
            for (int j = 0; j < allCharInLine.Length; j++)
            {
                int indexOfViChar = fromList.IndexOf(allCharInLine[j]);
                if (indexOfViChar >= 0)
                {
                    toWriteIn.Append(toList.ElementAt(indexOfViChar));
                }
                else
                {
                    toWriteIn.Append(allCharInLine[j]);
                }
            }

            return toWriteIn.ToString();
        }

        public static string[] listFiles(string filePath)
        {
            return listFiles(filePath, "*.*");
        }
        public static string[] listFiles(string filePath, string partern)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath)) { return new string[] {  }; }
            FileAttributes fileAttributes = File.GetAttributes(@filePath);
            if (fileAttributes.HasFlag(FileAttributes.Directory))
            {
                return Directory.GetFiles(filePath, partern, SearchOption.AllDirectories);
            }
            else
            {
                return new string[] { filePath };
            }
        }

        public static List<KeyValuePair<string, string>> getReplaceList(string replaceRuleFilePath, string REPLACE_SEPARATOR)
        {
            List<KeyValuePair<string, string>> replaceList = new List<KeyValuePair<string, string>>();
            if (!File.Exists(replaceRuleFilePath)) { return replaceList; }
            string[] replaceListRaw = @File.ReadAllLines(replaceRuleFilePath);//AppConst.RPG_WRAP_FILE

            for (int i = 0; i < replaceListRaw.Length; i++)
            {
                if (!replaceListRaw[i].Contains(REPLACE_SEPARATOR))
                {
                    continue;
                }
                int sepIndex = replaceListRaw[i].IndexOf(REPLACE_SEPARATOR);
                if (sepIndex < 1) { continue; }
                string replaceFrom = @replaceListRaw[i].Substring(0, sepIndex);
                string replaceTo = @replaceListRaw[i].Substring(sepIndex + REPLACE_SEPARATOR.Length);

                replaceList.Add(new KeyValuePair<string, string>(replaceFrom, replaceTo));
            }

            return replaceList;
        }

        public static string ExcelColumnIndexToName(int Index)
        {
            string range = string.Empty;
            if (Index < 0) return range;
            int a = 26;
            int x = (int)Math.Floor(Math.Log((Index) * (a - 1) / a + 1, a));
            Index -= (int)(Math.Pow(a, x) - 1) * a / (a - 1);
            for (int i = x + 1; Index + i > 0; i--)
            {
                range = ((char)(65 + Index % a)).ToString() + range;
                Index /= a;
            }
            return range;
        }

        public static double stringToNumber(string inputString)
        {
            try
            {
                return double.Parse(inputString);
            }
            catch (Exception)
            {
                double ret = 1;
                byte[] bArray = Encoding.UTF8.GetBytes(inputString);
                foreach (byte aByte in bArray)
                {
                    ret += aByte;
                }
                return (ret* bArray[0]);
            }
        }
    }
}
