using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator.PjProcessor
{
    class RpgMVProcessor : AbstractPjProcessor
    {
        public static readonly string SLASH_REPLACE_STRING = "†";
        public static readonly string OUTPUT_FILE_EXTENSION = ".txt";

        public static readonly string LINE_BREAK_TEMP = "<br>";
        public static readonly string INFO_JSONPATH_HEAD = "JsonPath";
        public static readonly string INFO_CODE_HEAD = "code";
        public static readonly string INFO_CODE_TEXT = "401";
        public static readonly string INFO_CODE_NOVEL_TEXT = "405";
        public static readonly string INFO_CODE_CHOICE = "102";
        public static readonly string INFO_CODE_CHAR_FACE = "101";
        public static readonly string INFO_CODE_SCRIPT_COMMAND = "356";
        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("utf-8");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8");
            aOutputEncoding = BuCommon.getEncodingFromString("utf-8");
            aWrapFont = new Font("M+ 1m regular", 16);
            aMaxWrap = 620;
            aWrapString = "†n";
            base.loadDefault(forceReload);
            aRpgFaceWrapMax = aMaxWrap - 126;
            aRpgNoneCodeWrapMax = aMaxWrap + 180;
        }

        public void export(string dataDir, string outputDir, string codeFilter)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(dataDir))
            {
                StringBuilder fullJsonString = new StringBuilder();
                foreach (string aFileLine in File.ReadAllLines(filePath, aInputEncoding))
                {
                    fullJsonString.Append(aFileLine);
                }

                JToken jTokenRoot = JToken.Parse(fullJsonString.ToString());
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string exportContent = "";
                string headerInfo = "";
                if ("Actors".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name|profile|nickname)");
                }
                if ("Armors".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name|description)");
                }
                if ("Classes".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name)");
                }
                if ("Enemies".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name)");
                }
                if ("Items".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name|description)");
                }
                if ("MapInfos".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name)");
                }
                if ("Skills".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name|description|message1|message2)");
                }
                if ("States".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name|note|message1|message2|message3|message4)");
                }
                //if ("Tilesets".Equals(fileName))
                //{
                //    exportContent = genCommonExportContent(
                //        jTokenRoot.SelectTokens(@"$[*].*"),
                //        @"\[\d+\]\.(?=name)");
                //}
                //if ("Tilesets1".Equals(fileName))
                //{
                //    exportContent = genCommonExportContent(
                //        jTokenRoot.SelectTokens(@"$[*].*"),
                //        @"\[\d+\]\.(?=name)");
                //}
                if ("Weapons".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].*"),
                        @"\[\d+\]\.(?=name|description|note)");
                }
                if ("CommonEvents".Equals(fileName))
                {
                    string jPath = @"$[*].list[?("+ codeFilter + ")].parameters[0]";
                    //string pathFilterRegex = @"\[\d+\]\.list\[\d+\]\.parameters\[0\]";
                    IEnumerable<JToken> gotProps = jTokenRoot.SelectTokens(jPath);

                    foreach (JToken gotProp in gotProps)
                    {
                        //if (Regex.IsMatch(gotProp.Path, pathFilterRegex))
                        //{
                        headerInfo = TransCommon.makeOneInfoStr(false, INFO_JSONPATH_HEAD, gotProp.Path);
                        string code = jTokenRoot.SelectToken(gotProp.Path.Replace(".parameters[0]", "") + ".code").ToString();
                        headerInfo += TransCommon.makeOneInfoStr(true, INFO_CODE_HEAD, code);
                        exportContent += genOneBlock(headerInfo, gotProp);
                        //}
                    }
                }
                if ("Troops".Equals(fileName))
                {
                    string jPath = @"$[*].pages[*].list[?(" + codeFilter + ")].parameters[0]";
                    IEnumerable<JToken> gotProps = jTokenRoot.SelectTokens(jPath);

                    foreach (JToken gotProp in gotProps)
                    {
                        headerInfo = TransCommon.makeOneInfoStr(false, INFO_JSONPATH_HEAD, gotProp.Path);
                        string code = jTokenRoot.SelectToken(gotProp.Path.Replace(".parameters[0]", "") + ".code").ToString();
                        headerInfo += TransCommon.makeOneInfoStr(true, INFO_CODE_HEAD, code);
                        exportContent += genOneBlock(headerInfo, gotProp);
                    }
                }
                if (Regex.IsMatch(fileName, @"Map\d+"))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"displayName"),
                        null);
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"events[*].note"),
                        null);

                    string jPath = @"events[*].pages[*].list[?(" + codeFilter + ")].parameters[0]";
                    IEnumerable<JToken> gotProps = jTokenRoot.SelectTokens(jPath);

                    foreach (JToken gotProp in gotProps)
                    {
                        headerInfo = TransCommon.makeOneInfoStr(false, INFO_JSONPATH_HEAD, gotProp.Path);
                        string code = jTokenRoot.SelectToken(gotProp.Path.Replace(".parameters[0]", "") + ".code").ToString();
                        headerInfo += TransCommon.makeOneInfoStr(true, INFO_CODE_HEAD, code);
                        exportContent += genOneBlock(headerInfo, gotProp);
                    }
                }
                if ("System".Equals(fileName))
                {
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"gameTitle"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"currencyUnit"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"armorTypes[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"elements[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"equipTypes[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"skillTypes[*]"),
                        null);
                    //exportContent += genCommonExportContent(
                    //    jTokenRoot.SelectTokens(@"switches[*]"),
                    //    null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"variables[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"weaponTypes[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"terms.basic[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"terms.commands[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"terms.params[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"terms.messages.*"),
                        null);
                }

                if (exportContent.Length > 0)
                {
                    string outputPath = String.Format("{0}\\{1}{2}"
                        , outputDir, Path.GetFileNameWithoutExtension(filePath), OUTPUT_FILE_EXTENSION);
                    File.WriteAllText(outputPath, exportContent, aMediateEncoding);
                }
            }
        }
        private string genCommonExportContent(IEnumerable<JToken> gotProps, string pathFilterRegex)
        {
            string exportContent = "";
            foreach (JToken gotProp in gotProps)
            {
                if (pathFilterRegex == null || Regex.IsMatch(gotProp.Path, pathFilterRegex))
                {
                    List<string> tempBlock = new List<string>();
                    string orgText = gotProp.ToString(Newtonsoft.Json.Formatting.None);
                    if (gotProp.Type == JTokenType.String)
                    {
                        orgText = orgText.Substring(1, orgText.Length - 2);
                    }
                    if (orgText.Length == 0) { continue; }

                    tempBlock.Add(TransCommon.TRANS_BLOCK_INFO_HEADER
                        + TransCommon.makeOneInfoStr(false, INFO_JSONPATH_HEAD, gotProp.Path));
                    tempBlock.Add(TransCommon.ORIGINAL_LINE_HEAD + orgText);
                    tempBlock.Add(TransCommon.FULL_TEXT_BOX_LINE_HEAD);
                    tempBlock.Add(TransCommon.TRANSLATED_LINE_HEAD + orgText);
                    tempBlock.Add("");

                    foreach (string blockLine in tempBlock)
                    {
                        exportContent += blockLine + Environment.NewLine;
                    }
                }
            }

            return exportContent;
        }
        private string genOneBlock(string headerInfo, JToken orgTextToken)
        {
            string ret = "";
            List<string> tempBlock = new List<string>();
            string orgText = orgTextToken.ToString(Newtonsoft.Json.Formatting.None);
            if (orgTextToken.Type== JTokenType.String)
            {
                orgText = orgText.Substring(1, orgText.Length - 2);
            }
            if (orgText.Length == 0) { return ""; }

            tempBlock.Add(TransCommon.TRANS_BLOCK_INFO_HEADER + headerInfo);
            tempBlock.Add(TransCommon.ORIGINAL_LINE_HEAD + orgText);
            tempBlock.Add(TransCommon.FULL_TEXT_BOX_LINE_HEAD);
            tempBlock.Add(TransCommon.TRANSLATED_LINE_HEAD + orgText);
            tempBlock.Add("");

            foreach (string blockLine in tempBlock)
            {
                ret += blockLine + Environment.NewLine;
            }

            return ret;
        }
        public void filterForCode(string code, bool isAccept, string regexStr, string inputFile, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                string newFileContent = "";
                List<List<string>> blockList = TransCommon.getBlockText(File.ReadAllLines(filePath, aMediateEncoding));
                foreach (List<string> aBlock in blockList)
                {
                    Dictionary<string, string> headerInfo = TransCommon.getInfoFromString(aBlock[0]);
                    if (!headerInfo.ContainsKey(INFO_CODE_HEAD)
                        || !headerInfo[INFO_CODE_HEAD].Equals(code))
                    {
                        newFileContent += TransCommon.blockToString(aBlock);
                        continue;
                    }
                    string orgText = aBlock[1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
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
                    File.WriteAllText(outputPath, newFileContent, aMediateEncoding);
                }
            }
        }
        public void update(string fromDir, string toDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(fromDir))
            {
                string toFilePath = toDir + "\\" + Path.GetFileName(fromFilePath);
                if (!File.Exists(toFilePath)) { continue; }
                string[] fromFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);
                string[] toFileArr = File.ReadAllLines(toFilePath, aMediateEncoding);
                int pauseLine = 0;
                for (int i = 0; i < toFileArr.Length; i++)
                {
                    if (toFileArr[i].StartsWith(TransCommon.FULL_TEXT_BOX_LINE_HEAD))
                    {
                        string orgTxt = toFileArr[i].Substring(TransCommon.FULL_TEXT_BOX_LINE_HEAD.Length);
                        for (int j = pauseLine; j < fromFileArr.Length; j++)
                        {
                            if (fromFileArr[j].StartsWith(TransCommon.FULL_TEXT_BOX_LINE_HEAD))
                            {
                                string toCompareOrgTxt = fromFileArr[j].Substring(TransCommon.FULL_TEXT_BOX_LINE_HEAD.Length);
                                if (toCompareOrgTxt.Equals(orgTxt))
                                {
                                    int delta = 1;
                                    while (fromFileArr[j + delta].Length > 0)
                                    {
                                        if (delta == 1)
                                        {
                                            toFileArr[i + 1] = fromFileArr[j + delta].Replace("†n", "<br>").Replace("††", "\\\\");
                                        }
                                        else
                                        {
                                            toFileArr[i + 1] += Environment.NewLine + fromFileArr[j + delta].Replace("†n", "<br>").Replace("††", "\\\\");
                                        }

                                        delta++;
                                    }
                                    pauseLine = j + 1;
                                    break;
                                }
                            }
                        }
                    }
                }
                String outputFile = outputDir + "\\" + Path.GetFileName(fromFilePath);
                File.WriteAllLines(outputFile, toFileArr, aMediateEncoding);
            }
        }
        public void upgrade(string fromDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(fromDir))
            {
                string[] fileLines = File.ReadAllLines(fromFilePath, aMediateEncoding);
                List<string> outLines = new List<string>();
                for (int i = 0; i < fileLines.Length; i++)
                {
                    if (fileLines[i].StartsWith(TransCommon.FULL_TEXT_BOX_LINE_HEAD))
                    {
                        fileLines[i] = TransCommon.FULL_TEXT_BOX_LINE_HEAD
                            + fileLines[i - 1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
                    }
                }
                String outputFile = outputDir + "\\" + Path.GetFileName(fromFilePath);
                File.WriteAllLines(outputFile, fileLines, aMediateEncoding);
            }
        }
        public void concat(string inputDir, string outputDir, string[] charNamePattern)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);
                bool changed = false;
                for (int i = 0; i < inputFileArr.Length; i++)
                {
                    if (inputFileArr[i].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                    {
                        Dictionary<string, string> headerInfo = TransCommon.getInfoFromString(inputFileArr[i]);
                        string orgTxt = inputFileArr[i + 1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
                        inputFileArr[i + 2] = TransCommon.FULL_TEXT_BOX_LINE_HEAD + orgTxt;
                        if (!headerInfo[INFO_JSONPATH_HEAD].EndsWith(".parameters[0]")) { continue; }
                        if (!headerInfo.ContainsKey(INFO_CODE_HEAD)) { continue; }
                        if (!headerInfo[INFO_CODE_HEAD].Equals(INFO_CODE_TEXT)) { continue; }

                        //Char name or face detect, ignore concat sentence if detect char name
                        bool isCharNameLine = false;
                        foreach (string pattern in charNamePattern)
                        {
                            MatchCollection match = Regex.Matches(orgTxt, pattern);
                            if (match.Count == 1 && match[0].Length == orgTxt.Length)
                            {
                                isCharNameLine = true;
                                break;
                            }
                        }
                        if (isCharNameLine)
                        {
                            inputFileArr[i] += TransCommon.makeOneInfoStr(true,
                                    TransCommon.INFO_MODE_HEAD,
                                    TransCommon.INFO_MODE_CHARACTER_NAME);
                            continue;
                        }
                        //Char name or face detect, END

                        string nextJsonPath = guestNextJsonPath(headerInfo[INFO_JSONPATH_HEAD]);
                        int nextIndex = i;
                        for (int j = i + 1; j < inputFileArr.Length; j++)
                        {
                            if (inputFileArr[j].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                            {
                                Dictionary<string, string> headerInfo2 = TransCommon.getInfoFromString(inputFileArr[j]);
                                bool needConcat = true;
                                if (!headerInfo2[INFO_JSONPATH_HEAD].Equals(nextJsonPath)) { needConcat = false; }
                                if (!headerInfo2.ContainsKey(INFO_CODE_HEAD)
                                    || !headerInfo2[INFO_CODE_HEAD].Equals(INFO_CODE_TEXT)) { needConcat = false; }
                                string orgTxt2 = inputFileArr[j + 1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
                                if (!needConcat)
                                {
                                    break;
                                }

                                nextJsonPath = guestNextJsonPath(headerInfo2[INFO_JSONPATH_HEAD]);
                                orgTxt += orgTxt2;
                                nextIndex = j + 1;

                                //Clear trans text if found concat sentence
                                inputFileArr[j + 3] = TransCommon.TRANSLATED_LINE_HEAD;
                            }
                        }

                        inputFileArr[i + 2] = TransCommon.FULL_TEXT_BOX_LINE_HEAD + orgTxt;
                        if (nextIndex > i)
                        {
                            i = nextIndex;
                            changed = true;
                        }
                    }
                }

                //if (changed)
                //{
                String outputFile = outputDir + "\\" + Path.GetFileName(fromFilePath);
                File.WriteAllLines(outputFile, inputFileArr, aMediateEncoding);
                //}
            }
        }
        public void girlsGuildConcat(string inputDir, string outputDir, string[] charNamePattern)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);
                bool changed = false;
                for (int i = 0; i < inputFileArr.Length; i++)
                {
                    if (inputFileArr[i].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                    {
                        Dictionary<string, string> headerInfo = TransCommon.getInfoFromString(inputFileArr[i]);
                        string orgTxt = inputFileArr[i + 1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
                        inputFileArr[i + 2] = TransCommon.FULL_TEXT_BOX_LINE_HEAD + orgTxt;
                        if (Regex.IsMatch(orgTxt, @"{.+?}")) { continue; }
                        if (!headerInfo[INFO_JSONPATH_HEAD].EndsWith(".parameters[0]")) { continue; }
                        if (!headerInfo.ContainsKey(INFO_CODE_HEAD)) { continue; }
                        if (!headerInfo[INFO_CODE_HEAD].Equals(INFO_CODE_TEXT)
                            && !headerInfo[INFO_CODE_HEAD].Equals(INFO_CODE_NOVEL_TEXT)) { continue; }

                        //Char name or face detect, ignore concat sentence if detect char name
                        Match match= null;
                        foreach (string pattern in charNamePattern)
                        {
                            match = Regex.Match(orgTxt, pattern);
                            if (match.Success)
                            {
                                break;
                            }
                        }
                        if (match!= null && match.Success)
                        {
                            inputFileArr[i+ 1] += Environment.NewLine + TransCommon.CHAR_NAME_LINE_HEAD+ match.Value;
                            orgTxt= orgTxt.Substring(match.Value.Length);
                        }
                        //Char name or face detect, END

                        string nextJsonPath = guestNextJsonPath(headerInfo[INFO_JSONPATH_HEAD]);
                        int nextIndex = i;
                        for (int j = i + 1; j < inputFileArr.Length; j++)
                        {
                            if (inputFileArr[j].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                            {
                                Dictionary<string, string> headerInfo2 = TransCommon.getInfoFromString(inputFileArr[j]);
                                bool needConcat = true;
                                if (!headerInfo2[INFO_JSONPATH_HEAD].Equals(nextJsonPath)) { needConcat = false; }
                                if (!headerInfo2.ContainsKey(INFO_CODE_HEAD)
                                    || !headerInfo2[INFO_CODE_HEAD].Equals(INFO_CODE_TEXT)) { needConcat = false; }
                                string orgTxt2 = inputFileArr[j + 1].Substring(TransCommon.ORIGINAL_LINE_HEAD.Length);
                                if (!needConcat)
                                {
                                    break;
                                }

                                nextJsonPath = guestNextJsonPath(headerInfo2[INFO_JSONPATH_HEAD]);
                                orgTxt += orgTxt2;
                                nextIndex = j + 1;

                                //Clear trans text if found concat sentence
                                inputFileArr[j + 3] = TransCommon.TRANSLATED_LINE_HEAD;
                            }
                        }

                        inputFileArr[i + 2] = TransCommon.FULL_TEXT_BOX_LINE_HEAD + orgTxt;
                        inputFileArr[i + 3] = TransCommon.TRANSLATED_LINE_HEAD + orgTxt;
                        if (nextIndex > i)
                        {
                            i = nextIndex;
                            changed = true;
                        }
                    }
                }

                //if (changed)
                //{
                String outputFile = outputDir + "\\" + Path.GetFileName(fromFilePath);
                File.WriteAllLines(outputFile, inputFileArr, aMediateEncoding);
                //}
            }
        }
        public void wrap(string inputDir, string outputDir, string wrapReplaceFilePath)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string toFilePath = outputDir + "\\" + Path.GetFileName(fromFilePath);
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);
                String fileContent = "";

                List<String> blockTemp = new List<string>();
                bool haveCharFace = false;
                string lastJsonPath= "";
                for (int i = 0; i < inputFileArr.Length; i++)
                {
                    if (inputFileArr[i].Length == 0)
                    {
                        if (blockTemp.Count > 0)
                        {
                            Dictionary<string, string> headerInfo;
                            bool needWrap = true;
                            bool haveCodeText = false;
                            String fullText = "";
                            for (int inI = 0; inI < blockTemp.Count; inI++)
                            {
                                if (blockTemp[inI].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                                {
                                    headerInfo = TransCommon.getInfoFromString(blockTemp[0]);
                                    if(!headerInfo[INFO_JSONPATH_HEAD].Equals(guestNextJsonPath(lastJsonPath)))
                                    {
                                        haveCharFace = false;
                                    }
                                    if (headerInfo.ContainsKey(TransCommon.INFO_MODE_HEAD))
                                    {
                                        if (headerInfo[TransCommon.INFO_MODE_HEAD].Equals(TransCommon.INFO_MODE_CHARACTER_NAME))
                                        {
                                            needWrap = false;
                                        }
                                        if (headerInfo[TransCommon.INFO_MODE_HEAD].Equals(TransCommon.INFO_MODE_CHARACTER_FACE))
                                        {
                                            needWrap = false;
                                            haveCharFace = true;
                                        }
                                    }
                                    if (headerInfo.ContainsKey(INFO_CODE_HEAD))
                                    {
                                        haveCodeText = true;
                                        if (headerInfo[INFO_CODE_HEAD].Equals(INFO_CODE_CHAR_FACE))
                                        {
                                            haveCharFace = true;
                                        }
                                        if(!headerInfo[INFO_CODE_HEAD].Equals(INFO_CODE_TEXT)
                                            && !headerInfo[INFO_CODE_HEAD].Equals(INFO_CODE_NOVEL_TEXT))
                                        {
                                            needWrap = false;
                                        }
                                    }
                                    else
                                    {
                                        haveCodeText = false;
                                    }
                                    lastJsonPath = headerInfo[INFO_JSONPATH_HEAD];
                                    continue;
                                }

                                String sentence = "";
                                if (blockTemp[inI].StartsWith(TransCommon.TRANSLATED_LINE_HEAD))
                                {
                                    sentence = blockTemp[inI].Substring(TransCommon.TRANSLATED_LINE_HEAD.Length);
                                }
                                if (!blockTemp[inI].StartsWith("<"))
                                {
                                    sentence = blockTemp[inI];
                                }
                                if (sentence.Contains(LINE_BREAK_TEMP)){needWrap = false;}
                                if (sentence.Contains("<wrap>")){needWrap = false;}

                                sentence = sentence.Replace("\\", SLASH_REPLACE_STRING).Replace(LINE_BREAK_TEMP, aWrapString);
                                if (sentence.Length > 0)
                                {
                                    //Do wrap function here
                                    int wrapMax = aMaxWrap;
                                    if (haveCharFace)
                                    {
                                        wrapMax = aRpgFaceWrapMax;
                                    }
                                    if (!haveCodeText) { wrapMax = aRpgNoneCodeWrapMax; }
                                    if (needWrap) { sentence = textSizeWrap(sentence, aWrapFont, wrapMax, aWrapString, wrapReplaceFilePath, out _); }
                                    fullText += fullText.Length == 0 ? sentence : (aWrapString + sentence);
                                }
                            }
                            fileContent += makeBlockStringForWrap(blockTemp, fullText);
                            blockTemp.Clear();
                        }
                    }
                    else
                    {
                        if (!inputFileArr[i].StartsWith(TransCommon.COMMENT_STR))
                        {
                            blockTemp.Add(inputFileArr[i]);
                        }
                    }
                }

                if (fileContent.Length > 0)
                {
                    File.WriteAllText(toFilePath, fileContent, aMediateEncoding);
                }
            }
        }
        private String makeBlockStringForWrap(List<String> blockLine, String transText)
        {
            String ret = "";
            for (int i = 0; i < blockLine.Count; i++)
            {
                if (blockLine[i].Length> 0 && blockLine[i].StartsWith("<"))
                {
                    if (blockLine[i].StartsWith(TransCommon.TRANSLATED_LINE_HEAD))
                    {
                        ret += TransCommon.TRANSLATED_LINE_HEAD + transText + Environment.NewLine;
                    }
                    else
                    {
                        ret += blockLine[i] + Environment.NewLine;
                    }
                }
            }
            ret += Environment.NewLine;

            return ret;
        }
        public void fillDuplicate(string orgFullText, string transFullText, string toFill)
        {
            if (orgFullText.Length == 0 || transFullText.Length == 0) { return; }
            foreach (string oneFilePath in BuCommon.listFiles(toFill))
            {
                string[] fileContent = File.ReadAllLines(oneFilePath, aMediateEncoding);
                bool filled = false;
                for (int i = 0; i < fileContent.Length; i++)
                {
                    if (fileContent[i].StartsWith(TransCommon.FULL_TEXT_BOX_LINE_HEAD))
                    {
                        string toCompareText = fileContent[i].Substring(TransCommon.FULL_TEXT_BOX_LINE_HEAD.Length);
                        if (toCompareText.Equals(orgFullText))
                        {
                            fileContent[i + 2] = TransCommon.TRANSLATED_LINE_HEAD + transFullText;
                            filled = true;
                        }
                    }
                }
                if (filled)
                {
                    File.WriteAllLines(oneFilePath, fileContent, aMediateEncoding);
                }
            }
        }

        JToken jTokenRootTemp;
        public bool importOneFile(string fromFile, string toFile, string outputFile)
        {
            StringBuilder fullJsonString = new StringBuilder();
            foreach (string aFileLine in File.ReadAllLines(toFile, aInputEncoding))
            {
                fullJsonString.Append(aFileLine);
            }
            jTokenRootTemp = JToken.Parse(fullJsonString.ToString());

            List<string> anImportBlock = new List<string>();
            List<string> removeEmptyLinePath = new List<string>();

            string[] fromFileLines = File.ReadAllLines(fromFile, aMediateEncoding);
            for (int i = 0; i < fromFileLines.Length; i++)
            {
                if (fromFileLines[i].Length == 0 && anImportBlock.Count > 1)
                {
                    removeEmptyLinePath.AddRange(importOneBlock(anImportBlock));
                    anImportBlock.Clear();
                    continue;
                }
                if (fromFileLines[i].Length > 0)
                {
                    anImportBlock.Add(fromFileLines[i]);
                }
            }
            if (anImportBlock.Count > 1)
            {
                removeEmptyLinePath.AddRange(importOneBlock(anImportBlock));
                anImportBlock.Clear();
            }

            removeEmptyLinePath.Reverse();
            foreach (string removeEmptyPath in removeEmptyLinePath)
            {
                JToken jTokenTemp = jTokenRootTemp.SelectToken(removeEmptyPath);
                while(true)
                {
                    if (jTokenTemp.Type == JTokenType.Object
                        && JObject.Parse(jTokenTemp.ToString()).ContainsKey(INFO_CODE_HEAD)) { break; }
                    jTokenTemp = jTokenTemp.Parent;
                }
                jTokenRootTemp.SelectToken(jTokenTemp.Path).Remove();
            }
            File.WriteAllText(outputFile, jTokenRootTemp.ToString(Formatting.None).Replace(SLASH_REPLACE_STRING, "\\"), aOutputEncoding);
            return true;
        }
        private List<string> importOneBlock(List<string> anImportBlock)
        {
            Dictionary<String, string> blockInfo = TransCommon.getInfoFromString(anImportBlock[0]);
            List<string> removeEmptyLinePathList = new List<string>();
            if (blockInfo.ContainsKey(INFO_CODE_HEAD)
                && blockInfo[INFO_CODE_HEAD].Equals(INFO_CODE_CHAR_FACE))
            {
                return removeEmptyLinePathList;
            }
            string charNameOnHead = "";
            for (int i = 0; i < anImportBlock.Count; i++)
            {
                if (anImportBlock[i].StartsWith(TransCommon.TRANS_BLOCK_INFO_HEADER))
                {
                    charNameOnHead = "";
                }
                if (anImportBlock[i].StartsWith(TransCommon.CHAR_NAME_LINE_HEAD))
                {
                    charNameOnHead = anImportBlock[i].Substring(TransCommon.CHAR_NAME_LINE_HEAD.Length);
                }
                if (anImportBlock[i].StartsWith(TransCommon.TRANSLATED_LINE_HEAD))
                {
                    string toWrite = charNameOnHead+ anImportBlock[i].Substring(TransCommon.TRANSLATED_LINE_HEAD.Length);
                    if (toWrite.Length == 0 && anImportBlock[i - 1].Equals(TransCommon.FULL_TEXT_BOX_LINE_HEAD))
                    {
                        //Remove empty line if no text and no full text
                        JToken capturedToken = jTokenRootTemp.SelectToken(blockInfo[INFO_JSONPATH_HEAD]);
                        if (capturedToken == null)
                        {
                            continue;
                        }
                        if (capturedToken.Type == JTokenType.Object)
                        {
                            removeEmptyLinePathList.Add(blockInfo[INFO_JSONPATH_HEAD]);
                            continue;
                        }
                        string objectRemovePath = blockInfo[INFO_JSONPATH_HEAD].Substring(0,
                            blockInfo[INFO_JSONPATH_HEAD].LastIndexOf("."));
                        removeEmptyLinePathList.Add(objectRemovePath);

                        continue;
                    }
                    if ((toWrite.StartsWith("[") && toWrite.EndsWith("]"))
                        || (toWrite.StartsWith("{") && toWrite.EndsWith("}")))
                    {
                        try
                        {
                            jTokenRootTemp.SelectToken(blockInfo[INFO_JSONPATH_HEAD]).Replace(JToken.Parse(toWrite));
                        }
                        catch (Newtonsoft.Json.JsonReaderException)
                        {
                            jTokenRootTemp.SelectToken(blockInfo[INFO_JSONPATH_HEAD]).Replace(toWrite);
                        }
                        //Newtonsoft.Json.JsonReaderException

                    }
                    else
                    {
                        jTokenRootTemp.SelectToken(blockInfo[INFO_JSONPATH_HEAD]).Replace(toWrite);

                    }
                }
            }
            return removeEmptyLinePathList;
        }
        private string guestNextJsonPath(string currentJsonPath)
        {
            if (null == currentJsonPath || currentJsonPath.Length == 0) { return ""; }
            string toMatchExpression = @"(?<!parameters\[)(?<=\[)\d+(?=\])";
            MatchCollection allNumberMatch = Regex.Matches(currentJsonPath, toMatchExpression);
            if (allNumberMatch.Count == 0) { return ""; }
            Match lastNumberMatch= allNumberMatch[allNumberMatch.Count- 1];
            int nextNumber = Int32.Parse(lastNumberMatch.Value) + 1;
            return String.Format("{0}{1}{2}",
                currentJsonPath.Substring(0, lastNumberMatch.Index),
                nextNumber,
                currentJsonPath.Substring(lastNumberMatch.Index+ lastNumberMatch.Length)
            );
        }
        public void tsDecode(string inputDir, Encoding encoding, byte decodeKey, string outputDir, string newExt)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(inputDir))
            {
                string[] decodeFileArr = Regex.Split(File.ReadAllText(filePath, encoding), string.Empty);
                char[] arr = encoding.GetString(encoding.GetBytes(File.ReadAllText(filePath, encoding))).ToCharArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = Convert.ToChar(arr[i] ^ decodeKey);
                }
                string outputPath = String.Format("{0}\\{1}.{2}"
                    , outputDir, Path.GetFileNameWithoutExtension(filePath), newExt);
                File.WriteAllText(outputPath, new String(arr), encoding);
            }
        }


    }
}
