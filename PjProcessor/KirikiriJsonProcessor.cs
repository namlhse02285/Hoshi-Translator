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
    class KirikiriJsonProcessor : AbstractPjProcessor
    {
        public static readonly string INFO_JSONPATH_HEAD = "JsonPath";
        public static readonly string OUTPUT_FILE_EXTENSION = ".txt";
        public static readonly string SLASH_REPLACE_STRING = "†";

        public override void loadDefault(bool forceReload)
        {
            aInputEncoding = BuCommon.getEncodingFromString("utf-8");
            aMediateEncoding = BuCommon.getEncodingFromString("utf-8");
            aOutputEncoding = BuCommon.getEncodingFromString("utf-8");
            aWrapFont = new Font("M+ 1m regular", 16);
            aMaxWrap = 620;
            aWrapString = "†r†n";
            base.loadDefault(forceReload);
        }
        public void export(string dataDir, string outputDir, string jsonPathFilterRegex)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string filePath in BuCommon.listFiles(dataDir))
            {
                StringBuilder fullJsonString = new StringBuilder();
                foreach (string aFileLine in File.ReadAllLines(filePath, aInputEncoding))
                {
                    fullJsonString.Append(aFileLine.Trim());
                }

                string exportContent = "";
                JToken jTokenRoot = JToken.Parse(fullJsonString.ToString());
                exportContent += genCommonExportContent(
                    jTokenRoot.SelectTokens(@"scenes[*].texts[*][*]"),
                    jsonPathFilterRegex);
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
                    tempBlock.Add(TransCommon.FULL_TEXT_BOX_LINE_HEAD+ orgText.Replace("\\r\\n"," "));
                    tempBlock.Add(TransCommon.TRANSLATED_LINE_HEAD);
                    tempBlock.Add("");

                    foreach (string blockLine in tempBlock)
                    {
                        exportContent += blockLine + Environment.NewLine;
                    }
                }
            }

            return exportContent;
        }

        public void import(string inputDir, string orgDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string orgFilePath = orgDir;
                if (File.GetAttributes(@orgDir).HasFlag(FileAttributes.Directory))
                {
                    orgFilePath += "\\" + Path.GetFileNameWithoutExtension(fromFilePath) + ".json";
                }
                string outputFile = outputDir + "\\" + Path.GetFileName(orgFilePath);
                string[] inputFileArr = File.ReadAllLines(fromFilePath, aMediateEncoding);

                StringBuilder fullJsonString = new StringBuilder();
                foreach (string aFileLine in File.ReadAllLines(orgFilePath, aInputEncoding))
                {
                    fullJsonString.Append(aFileLine.Trim());
                }
                JToken jTokenRoot = JToken.Parse(fullJsonString.ToString());

                List<List<string>> fileBlocks = TransCommon.getBlockText(inputFileArr);
                foreach (List<string> aBlock in fileBlocks)
                {
                    string transText = TransCommon.getBlockSingleText(
                        aBlock, TransCommon.TRANSLATED_LINE_HEAD, true);
                    if (transText.Length == 0) { continue; }
                    string orgText = TransCommon.getBlockSingleText(
                        aBlock, TransCommon.ORIGINAL_LINE_HEAD, false);
                    Dictionary<string, string> info = TransCommon.getInfoFromString(aBlock[0]);
                    transText = TransCommon.quoteSentenceBaseOnJp(orgText, transText);

                    jTokenRoot.SelectToken(info[INFO_JSONPATH_HEAD]).Replace(transText);
                }
                File.WriteAllText(outputFile, jTokenRoot.ToString(Formatting.None)
                    .Replace(SLASH_REPLACE_STRING, "\\"), aOutputEncoding);
            }
        }
    }
}
