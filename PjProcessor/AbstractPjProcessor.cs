using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Hoshi_Translator.PjProcessor
{
    abstract class AbstractPjProcessor
    {
        protected Property aProp;
        protected Encoding aInputEncoding;
        protected Encoding aMediateEncoding;
        protected Encoding aOutputEncoding;
        protected Font aWrapFont;
        protected int aMaxWrap;
        protected string aWrapString;

        public virtual void loadDefault(bool forceReload)
        {
            if (aProp == null || forceReload)
            {
                aProp = new Property(AppConst.CONFIG_FILE);
                aProp.reload();

                if (aProp.get(Property.Common.INPUT_ENCODING) != null && aProp.get(Property.Common.INPUT_ENCODING).Length > 0)
                    aInputEncoding = BuCommon.getEncodingFromString(aProp.get(Property.Common.INPUT_ENCODING));
                if (aProp.get(Property.Common.MEDIATE_ENCODING) != null && aProp.get(Property.Common.MEDIATE_ENCODING).Length > 0)
                    aMediateEncoding = BuCommon.getEncodingFromString(aProp.get(Property.Common.MEDIATE_ENCODING));
                if (aProp.get(Property.Common.OUTPUT_ENCODING) != null && aProp.get(Property.Common.OUTPUT_ENCODING).Length > 0)
                    aOutputEncoding = BuCommon.getEncodingFromString(aProp.get(Property.Common.OUTPUT_ENCODING));
                if (aProp.get(Property.Common.WRAP_FONT_NAME) != null && aProp.get(Property.Common.WRAP_FONT_NAME).Length > 0
                    && aProp.get(Property.Common.WRAP_FONT_SIZE) != null && aProp.get(Property.Common.WRAP_FONT_SIZE).Length > 0)
                    aWrapFont = new Font(aProp.get(Property.Common.WRAP_FONT_NAME), float.Parse(aProp.get(Property.Common.WRAP_FONT_SIZE)));
                if (aProp.get(Property.Common.WRAP_MAX) != null && aProp.get(Property.Common.WRAP_MAX).Length > 0)
                    aMaxWrap = Int32.Parse(aProp.get(Property.Common.WRAP_MAX));
                if (aProp.get(Property.Common.WRAP_STRING) != null && aProp.get(Property.Common.WRAP_STRING).Length > 0)
                {
                    aWrapString = aProp.get(Property.Common.WRAP_STRING);
                }
            }
            if (aWrapString != null)
            {
                if (aWrapString.Contains("<singlenewline>"))
                {
                    aWrapString = aWrapString.Replace("<singlenewline>", "\n");
                }
                if (aWrapString.Contains("<fullnewline>"))
                {
                    aWrapString = aWrapString.Replace("<fullnewline>", "\r\n");
                }
            }
        }

        public virtual void replace(string inputFile, string outputDir, string replaceFilePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputDir));

            List<KeyValuePair<string, string>> replaceList
                = BuCommon.getReplaceList(replaceFilePath, AppConst.REPLACE_SEPARATOR);
            string[] fileList = BuCommon.listFiles(inputFile);

            foreach (string aFilePath in fileList)
            {
                string[] inputLines = File.ReadAllLines(aFilePath);
                for (int i = 0; i < inputLines.Length; i++)
                {
                    string aLine = inputLines[i];
                    if (!aLine.StartsWith(TransCommon.TRANSLATED_LINE_HEAD) && aLine.StartsWith("<"))
                    {
                        continue;
                    }
                    inputLines[i]= aLine.Replace(replaceList[i].Key, replaceList[i].Value);
                }
                string outputPath = String.Format("{0}\\{1}", outputDir, Path.GetFileName(aFilePath));
                File.WriteAllLines(outputPath, inputLines, aMediateEncoding);
            }
        }

        class RPGMidWrap
        {
            public int index { get; set; }
            public string org { get; set; }
            public string replaceTo { get; set; }
            public RPGMidWrap(int _index, string _org, string _replaceTo)
            {
                index = _index;
                org = _org;
                replaceTo = _replaceTo;
            }
        }
        public static string textSizeWrap(string preInput, Font font, int maxPixel, string orgWrapChar, string wrapReplaceFilePath, out int outLineCount)
        {
            outLineCount = 1;
            if (maxPixel <= 0) { return preInput; }
            string wrapChar = "‡";
            int spaceSize = TextRenderer.MeasureText("aaa bbb", font).Width
                 - TextRenderer.MeasureText("aaabbb", font).Width;
            List<KeyValuePair<string, string>> patternList = getReplaceList(
                wrapReplaceFilePath == null || !File.Exists(wrapReplaceFilePath)
                    ? AppConst.REPLACE_WHEN_WRAP_FILE : wrapReplaceFilePath);
            List<RPGMidWrap> replaceList = new List<RPGMidWrap>();
            foreach (KeyValuePair<String, string> patternKeyValue in patternList)
            {
                foreach (Match m in Regex.Matches(preInput, @patternKeyValue.Key))
                {
                    if (m.Value.Length > 0)
                    {
                        replaceList.Add(new RPGMidWrap(m.Index, m.Value, @patternKeyValue.Value));
                    }
                }
            }
            replaceList.Sort((x, y) => x.index.CompareTo(y.index));
            string input = preInput;
            foreach (KeyValuePair<string, string> patternKeyValue in patternList)
            {
                input = Regex.Replace(input, @patternKeyValue.Key, @patternKeyValue.Value);
            }
            Size textSize = TextRenderer.MeasureText(input, font);
            if (textSize.Width <= maxPixel)
            {
                return preInput;
            }

            List<string> separatedInput = input.Split(' ').ToList();
            StringBuilder tempRet = new StringBuilder();
            StringBuilder tempLine = new StringBuilder();
            tempLine.Append(separatedInput.ElementAt(0));
            for (int i = 1; i < separatedInput.Count; i++)
            {
                string tempNext = tempLine.ToString() + ' ' + separatedInput.ElementAt(i);
                Size tempTextSize = TextRenderer.MeasureText(tempNext, font);
                if (tempTextSize.Width > maxPixel)
                {
                    if (tempRet.Length > 0)
                    {
                        tempRet.Append(wrapChar);
                        outLineCount++;
                    }
                    tempRet.Append(tempLine);

                    tempLine = new StringBuilder(separatedInput.ElementAt(i));
                }
                else
                {
                    tempLine.Append(' ').Append(separatedInput.ElementAt(i));
                }
            }

            outLineCount++;
            tempRet.Append(wrapChar).Append(tempLine);

            string ret = tempRet.ToString();
            foreach (RPGMidWrap wrapInfo in replaceList)
            {
                ret = ret.Substring(0, wrapInfo.index)
                    + wrapInfo.org
                    + ret.Substring(wrapInfo.index + wrapInfo.replaceTo.Length);
            }
            return ret.Replace(wrapChar, orgWrapChar);

        }

        public static List<KeyValuePair<string, string>> getReplaceList(string replaceRuleFilePath)
        {
            List<KeyValuePair<string, string>> replaceList = new List<KeyValuePair<string, string>>();
            string[] replaceListRaw = @File.ReadAllLines(replaceRuleFilePath);//AppConst.RPG_WRAP_FILE

            for (int i = 0; i < replaceListRaw.Length; i++)
            {
                if (replaceListRaw[i].Length == 0
                    || replaceListRaw[i].StartsWith("//")
                    || !replaceListRaw[i].Contains(AppConst.REPLACE_SEPARATOR))
                {
                    continue;
                }
                int sepIndex = replaceListRaw[i].IndexOf(AppConst.REPLACE_SEPARATOR);
                if (sepIndex < 1) { continue; }
                string replaceFrom = @replaceListRaw[i].Substring(0, sepIndex);
                string replaceTo = @replaceListRaw[i].Substring(sepIndex + AppConst.REPLACE_SEPARATOR.Length);

                replaceList.Add(new KeyValuePair<string, string>(replaceFrom, replaceTo));
            }

            return replaceList;
        }

        public string convertCharacter(string input, List<char> fromList, List<char> toList)
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

        public string formatTransString(string preInput)
        {
            string ret = preInput;
            if (ret.EndsWith("」")) { return ret; }
            if (ret.EndsWith("』")) { return ret; }
            if (ret.EndsWith("）")) { return ret; }

            //if(!ret.EndsWith(".")
            //    && !ret.EndsWith("!")
            //    && !ret.EndsWith("?")
            //    && !ret.EndsWith("-")
            //    && !ret.EndsWith("―")
            //    && !ret.EndsWith("♪")) 
            //{ 
            //    ret += "."; 
            //}

            if (ret.StartsWith("「")) { ret += "」"; }
            if (ret.StartsWith("『")) { ret += "』"; }
            if (ret.StartsWith("（")) { ret += "）"; }
            return ret;
        }


    }
}
