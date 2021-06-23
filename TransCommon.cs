using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator
{
    class TransCommon
    {
        public static readonly string TRANS_BLOCK_INFO_HEADER = "<translate_info>";

        public static readonly string INFO_MODE_HEAD = "mode";
        public static readonly string INFO_MODE_CHARACTER_NAME = "character_name";
        public static readonly string INFO_MODE_CHARACTER_FACE = "character_face";

        public static readonly string COMMENT_STR = "//";
        public static readonly string INFO_SEPARATOR_STR = ";";
        public static readonly string INFO_PARAM_SEPARATOR_STR = ":";
        public static readonly string CHAR_NAME_LINE_HEAD = "<char_name>";
        public static readonly string ORIGINAL_LINE_HEAD = "<org__text>";
        public static readonly string FULL_TEXT_BOX_LINE_HEAD = "<full_text>";
        public static readonly string TRANSLATED_LINE_HEAD = "<tran_text>";
        public static readonly string JP_LINE_HEAD = "<jap__text>";
        public static readonly string ENG_LINE_HEAD = "<eng__text>";
        public static readonly string VI_LINE_HEAD = "<viet_text>";
        public static readonly string EDITED_LINE_HEAD = "<edit_text>";

        public static string genInfoString(Dictionary<string, string> prop)
        {
            StringBuilder ret = new StringBuilder(TRANS_BLOCK_INFO_HEADER);
            for (int i = 0; i < prop.Count; i++)
            {
                if (i != 0)
                {
                    ret.Append(INFO_SEPARATOR_STR+ " ");
                }
                ret.Append(prop.ElementAt(i).Key).Append(INFO_PARAM_SEPARATOR_STR).Append(prop.ElementAt(i).Value);
            }
            return ret.ToString();
        }
        public static string getBlockSingleText(List<string> block, string header, bool spaceBetween)
        {
            string ret = "";
            foreach (string aLine in block)
            {
                if(null== header && !aLine.StartsWith("<"))
                {
                    ret += (spaceBetween && ret.Length> 0 ? " " : "") + aLine;
                }
                if (aLine.StartsWith(header))
                {
                    return aLine.Substring(header.Length);
                }
            }

            return ret;
        }
        public static List<List<string>> getBlockText(string[] fromContent)
        {
            List<List<string>> ret = new List<List<string>>();
            List<string> blockTemp = new List<string>();
            foreach (string aLine in fromContent)
            {
                if(aLine.Length== 0)
                {
                    if(blockTemp.Count> 0)
                    {
                        ret.Add(blockTemp.ToList());
                        blockTemp.Clear();
                    }
                }
                else
                {
                    if (!aLine.StartsWith(TransCommon.COMMENT_STR))
                    {
                        blockTemp.Add(aLine);
                    }
                }
            }
            if (blockTemp.Count > 0)
            {
                ret.Add(blockTemp.ToList());
                blockTemp.Clear();
            }
            return ret;
        }
        public static string blockToString(List<string> blockString)
        {
            string ret = "";
            foreach(string aLine in blockString)
            {
                if(aLine.Length> 0)
                {
                    ret += aLine+ Environment.NewLine;
                }
            }
            ret += Environment.NewLine;
            return ret;
        }
        public static Dictionary<string, string> getInfoFromString(string transHead)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            if (!transHead.StartsWith(TRANS_BLOCK_INFO_HEADER)) { return ret; }
            string info = transHead.Substring(TRANS_BLOCK_INFO_HEADER.Length);
            string[] allPropStr = info.Split(new string[] { INFO_SEPARATOR_STR }, StringSplitOptions.None);
            for (int i = 0; i < allPropStr.Length; i++)
            {
                int sepIndex = allPropStr[i].IndexOf(INFO_PARAM_SEPARATOR_STR);
                if (sepIndex < 1) { continue; }
                ret.Add(allPropStr[i].Substring(0, sepIndex).TrimStart(),
                    allPropStr[i].Substring(sepIndex + INFO_PARAM_SEPARATOR_STR.Length).TrimStart());
            }

            return ret;
        }
        public static string makeOneInfoStr(bool addSeparator, string infoHeader, string value)
        {
            return (addSeparator ? INFO_SEPARATOR_STR : "") + infoHeader + INFO_PARAM_SEPARATOR_STR+ " " + value;
        }

        public static void propertyAndTextFilter(string propName, string expressionString,
            bool isAccept, string regexStr, string inputFile, Encoding encoding, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            Expression expression = new Expression(expressionString);
            if (Regex.IsMatch(expressionString, @"'.+?'"))
            {
                MatchCollection matchCollection = Regex.Matches(expressionString, @"'.+?'");
                foreach (Match aMatch in matchCollection)
                {
                    expressionString = expressionString.Replace(
                        aMatch.Value, BuCommon.stringToNumber(aMatch.Value.Trim('\'')).ToString());
                }
                expression = new Expression(expressionString);
            }
            expression.defineArgument(propName, 0);

            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                string newFileContent = "";
                List<List<string>> blockList = TransCommon.getBlockText(File.ReadAllLines(filePath, encoding));
                foreach (List<string> aBlock in blockList)
                {
                    Dictionary<string, string> headerInfo = TransCommon.getInfoFromString(aBlock[0]);
                    if (!headerInfo.ContainsKey(propName))
                    {
                        newFileContent += TransCommon.blockToString(aBlock);
                        continue;
                    }
                    expression.setArgumentValue(propName, BuCommon.stringToNumber(headerInfo[propName]));
                    double result = expression.calculate();
                    if (expression.calculate()== 0)
                    {
                        newFileContent += TransCommon.blockToString(aBlock);
                        continue;
                    }
                    string orgText = getBlockSingleText(aBlock, TransCommon.ORIGINAL_LINE_HEAD, false);
                    if (isAccept && Regex.IsMatch(orgText, regexStr))
                    {
                        newFileContent += TransCommon.blockToString(aBlock);
                        continue;
                    }
                    if (!isAccept && !Regex.IsMatch(orgText, regexStr))
                    {
                        newFileContent += TransCommon.blockToString(aBlock);
                        continue;
                    }
                }
                if (newFileContent.Length > 0)
                {
                    string outputPath = String.Format("{0}\\{1}", outputDir, Path.GetFileName(filePath));
                    File.WriteAllText(outputPath, newFileContent, encoding);
                }
            }
        }


    }
}
