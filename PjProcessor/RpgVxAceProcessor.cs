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
    class RpgVxAceProcessor : AbstractPjProcessor
    {
        public static readonly string SLASH_REPLACE_STRING = "†";
        public static readonly string QUOTE_REPLACE_STRING = "‥";
        public static readonly string OUTPUT_FILE_EXTENSION = ".txt";

        public static readonly string LINE_BREAK_TEMP = "<br>";
        public static readonly string INFO_JSONPATH_HEAD = "json_path";
        public static readonly string INFO_JSON_CLASS_HEAD = "json_class";
        public static readonly string INFO_TYPE_HEAD = "type";
        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("utf-8");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8");
            aOutputEncoding = BuCommon.getEncodingFromString("utf-8");
            aWrapFont = new Font("VL Gothic", 16);
            aMaxWrap = 620;
            aWrapString = "\\n";
            base.loadDefault(forceReload);
        }

        public void export(string dataDir, string outputDir, string typeFilter)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(dataDir, "*.json"))
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
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name|description|nickname)");
                }
                if ("Armors".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name|description)");
                }
                if ("Classes".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name|description)");
                }
                if ("Enemies".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name)");
                }
                if ("Items".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name|description)");
                }
                if ("MapInfos".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name)");
                }
                if ("Skills".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name|description|message1|message2)");
                }
                if ("States".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name|message1|message2|message3|message4)");
                }
                if ("Weapons".Equals(fileName))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"$[*].[*].*"),
                        @"\[\d+\]\[1\]\.(?=name|description)");
                }
                if ("System".Equals(fileName))
                {
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"gameTitle"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"currency_unit"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"armor_types[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"elements[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"skill_types[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"weapon_types[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"terms.basic[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"terms.parameters[*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"terms['equipment types'][*]"),
                        null);
                    exportContent += genCommonExportContent(
                        jTokenRoot.SelectTokens(@"terms.commands[*]"),
                        null);
                }
                if ("CommonEvents".Equals(fileName))
                {
                    string jPath = @"$[*].[1].commands[?(" + typeFilter + ")]";
                    IEnumerable<JToken> gotProps = jTokenRoot.SelectTokens(jPath);

                    foreach (JToken gotProp in gotProps)
                    {
                        JObject[] allGot = JObject.Parse(gotProp.ToString())
                            .Descendants().OfType<JObject>().ToArray();
                        string type = gotProp["type"].ToString();
                        foreach (JObject aGot in allGot)
                        {
                            if (aGot["original   "] == null
                                || aGot["original   "].ToString().Length == 0) { continue; }
                            headerInfo = TransCommon.makeOneInfoStr(false, INFO_JSONPATH_HEAD
                                , gotProp.Path + "." + aGot.Path);
                            exportContent += genExportContent(
                                headerInfo + TransCommon.makeOneInfoStr(true, INFO_TYPE_HEAD, type), aGot);
                        }
                    }
                }
                if ("Troops".Equals(fileName))
                {
                    string jPath = @"$[*].[1].pages[*].commands[?(" + typeFilter + ")]";
                    IEnumerable<JToken> gotProps = jTokenRoot.SelectTokens(jPath);

                    foreach (JToken gotProp in gotProps)
                    {
                        JObject[] allGot = JObject.Parse(gotProp.ToString())
                            .Descendants().OfType<JObject>().ToArray();
                        string type = gotProp["type"].ToString();
                        foreach (JObject aGot in allGot)
                        {
                            if (aGot["original   "] == null
                                || aGot["original   "].ToString().Length == 0) { continue; }
                            headerInfo = TransCommon.makeOneInfoStr(false, INFO_JSONPATH_HEAD
                                , gotProp.Path + "." + aGot.Path);
                            exportContent += genExportContent(
                                headerInfo+ TransCommon.makeOneInfoStr(true, INFO_TYPE_HEAD, type), aGot);
                        }
                    }
                }
                if (Regex.IsMatch(fileName, @"Map\d+"))
                {
                    exportContent = genCommonExportContent(
                        jTokenRoot.SelectTokens(@"display_name"),
                        null);

                    string jPath = @"events[*].[1].pages[*].commands[?(" + typeFilter + ")]";
                    IEnumerable<JToken> gotProps = jTokenRoot.SelectTokens(jPath);

                    foreach (JToken gotProp in gotProps)
                    {
                        JObject[] allGot = JObject.Parse(gotProp.ToString())
                            .Descendants().OfType<JObject>().ToArray();
                        string type = gotProp["type"].ToString();
                        foreach (JObject aGot in allGot)
                        {
                            if (aGot["original   "] == null
                                || aGot["original   "].ToString().Length == 0) { continue; }
                            headerInfo = TransCommon.makeOneInfoStr(false, INFO_JSONPATH_HEAD
                                , gotProp.Path + "." + aGot.Path);
                            exportContent += genExportContent(
                                headerInfo + TransCommon.makeOneInfoStr(true, INFO_TYPE_HEAD, type), aGot);
                        }
                    }
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
                    string orgText = gotProp["original   "].ToString(Newtonsoft.Json.Formatting.None);
                    orgText = orgText.Substring(0, orgText.Length - 1);
                    orgText = orgText.TrimStart('\"');
                    if (orgText.Length == 0) { continue; }

                    tempBlock.Add(TransCommon.TRANS_BLOCK_INFO_HEADER
                        + TransCommon.makeOneInfoStr(false, INFO_JSONPATH_HEAD, gotProp.Path)
                        //+ TransCommon.makeOneInfoStr(true, INFO_JSON_CLASS_HEAD, gotProp["json_class"].ToString())
                        );
                    tempBlock.Add(TransCommon.ORIGINAL_LINE_HEAD + orgText);
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
        private string genExportContent(string headerInfo, JObject jObject)
        {
            string ret = "";
            string orgText = "";
            string fullText = "";
            List<string> tempBlock = new List<string>();

            try
            {
                JArray arr = JArray.Parse(jObject["original   "].ToString(Newtonsoft.Json.Formatting.None));
                IEnumerator<JToken> enumerator = arr.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    if(orgText.Length== 0)
                    {
                        orgText = enumerator.Current.ToString();
                        fullText = enumerator.Current.ToString();
                    }
                    else
                    {
                        orgText += Environment.NewLine + TransCommon.ORIGINAL_LINE_HEAD + enumerator.Current.ToString();
                        fullText+= " " + enumerator.Current.ToString().Trim();
                    }
                }
            }
            catch (Exception)
            {
                orgText = jObject["original   "].ToString(Newtonsoft.Json.Formatting.None);
                orgText = orgText.Substring(0, orgText.Length - 1);
                orgText = orgText.TrimStart('\"');
                fullText = orgText.ToString();
            }

            if (orgText.Length == 0) { return ""; }

            tempBlock.Add(TransCommon.TRANS_BLOCK_INFO_HEADER + headerInfo);
            tempBlock.Add(TransCommon.ORIGINAL_LINE_HEAD + orgText);
            tempBlock.Add(TransCommon.FULL_TEXT_BOX_LINE_HEAD+ fullText);
            tempBlock.Add(TransCommon.TRANSLATED_LINE_HEAD + fullText);
            tempBlock.Add("");

            foreach (string blockLine in tempBlock)
            {
                ret += blockLine + Environment.NewLine;
            }

            return ret;
        }
        public void filterForType(string type, bool isAccept, string regexStr, string inputFile, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                string newFileContent = "";
                List<List<string>> blockList = TransCommon.getBlockText(File.ReadAllLines(filePath, aMediateEncoding));
                foreach (List<string> aBlock in blockList)
                {
                    Dictionary<string, string> headerInfo = TransCommon.getInfoFromString(aBlock[0]);
                    if (!headerInfo.ContainsKey(INFO_TYPE_HEAD)
                        || !headerInfo[INFO_TYPE_HEAD].Equals(type))
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

        public void wrap(string inputDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);
                string fullWrap = "";
                int transLine = -1;

                for (int i = 0; i < inputFileArr.Length; i++)
                {
                    if (inputFileArr[i].StartsWith(TransCommon.TRANSLATED_LINE_HEAD))
                    {
                        string lineContent = inputFileArr[i].Substring(TransCommon.TRANSLATED_LINE_HEAD.Length);
                        lineContent= lineContent.Replace("\\", SLASH_REPLACE_STRING)
                            .Replace("\\\"", "\"")
                            .Replace("††", "**")
                            .Replace("†", "††")
                            .Replace("††n", "†n")
                            .Replace("**", "††");
                        fullWrap = textSizeWrap(
                            lineContent,
                            aWrapFont, aMaxWrap, aWrapString, out _);
                        transLine = i;
                    }
                    if (inputFileArr[i].Length> 0 && !inputFileArr[i].StartsWith("<"))
                    {
                        string lineContent = inputFileArr[i];
                        lineContent = lineContent.Replace("\\", SLASH_REPLACE_STRING)
                            .Replace("\\\"", "\"")
                            .Replace("††", "**")
                            .Replace("†", "††")
                            .Replace("††n", "†n")
                            .Replace("**", "††");
                        fullWrap += aWrapString+ textSizeWrap(lineContent, aWrapFont, aMaxWrap, aWrapString, out _);
                        inputFileArr[i] = "";
                    }
                    if(inputFileArr[i].Length== 0 && fullWrap.Length> 0 && transLine>= 0)
                    {
                        inputFileArr[transLine] = TransCommon.TRANSLATED_LINE_HEAD+ fullWrap;
                    }
                }

                string toFilePath = outputDir + "\\" + Path.GetFileName(fromFilePath);
                File.WriteAllLines(toFilePath, inputFileArr, aMediateEncoding);
            }
        }

        public void importOneFile(string inputFile, string orgFile)
        {
            StringBuilder fullJsonString = new StringBuilder();
            foreach (string aFileLine in File.ReadAllLines(orgFile, aInputEncoding))
            {
                fullJsonString.Append(aFileLine);
            }
            JToken jTokenRoot = JToken.Parse(fullJsonString.ToString());

            List<List<string>> allBlock = TransCommon.getBlockText(File.ReadAllLines(inputFile, aMediateEncoding));
            foreach(List<string> oneBlock in allBlock)
            {
                Dictionary<string, string> info = TransCommon.getInfoFromString(oneBlock[0]);
                string jsonPath = info[INFO_JSONPATH_HEAD];

                JToken transToken = jTokenRoot.SelectToken(jsonPath + ".translation");
                string writeText = TransCommon.getBlockSingleText(oneBlock, TransCommon.TRANSLATED_LINE_HEAD, true);
                writeText = writeText.Replace("\"", QUOTE_REPLACE_STRING);
                if (transToken.Type== JTokenType.Array)
                {
                    string writeContent = "[\""+ writeText + "\"]";
                    JToken toWrite = JToken.Parse(writeContent);
                    jTokenRoot.SelectToken(jsonPath + ".translation").Replace(toWrite);
                }
                else
                {
                    jTokenRoot.SelectToken(jsonPath + ".translation").Replace(writeText);
                }
            }
            File.WriteAllText(orgFile,
                jTokenRoot.ToString(Formatting.None)
                    .Replace(QUOTE_REPLACE_STRING, "\\\"")
                    .Replace(SLASH_REPLACE_STRING, "\\"), aOutputEncoding);
        }


    }
}
