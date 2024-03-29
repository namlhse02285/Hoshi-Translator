﻿using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Hoshi_Translator
{
    class TransCommon
    {
        public static readonly string TRANS_BLOCK_INFO_HEADER = "<translate_info>";
        public static readonly string EXPORT_FILE_EXTENSION = ".txt";

        public static readonly string INFO_LINE_HEAD = "line";

        public static readonly string INFO_NOTE_HEAD = "note";

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

        public static void importExcelFileToTrans(string inputFile, string toCheckHeaderListStr,
             string toImportHeader, string orgDir, Encoding encoding, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            List<string> toCheckHeaderList = Regex.Split(toCheckHeaderListStr, ",").ToList();
            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                ExcelPackage workbook = new ExcelPackage(new FileInfo(filePath));
                ExcelWorksheet worksheet = workbook.Workbook.Worksheets[
                    Path.GetFileNameWithoutExtension(filePath)];
                string orgFilePath = Path.Combine(orgDir,
                    Path.GetFileNameWithoutExtension(filePath)+ EXPORT_FILE_EXTENSION);
                if (!File.Exists(orgFilePath)) { continue; }
                string outputFilePath = Path.Combine(outputDir,
                    Path.GetFileNameWithoutExtension(filePath)+ EXPORT_FILE_EXTENSION);
                string importedContent = "";

                Dictionary<string, int> headerMapWithIndex = new Dictionary<string, int>();
                int tempExcelIndex = 0;
                int emptyExcelColumnsMax = 5;
                while (true)
                {
                    string headerCellTemp = BuCommon.ExcelColumnIndexToName(tempExcelIndex) + "1";
                    string excelHeaderTemp = worksheet.Cells[headerCellTemp].Text;
                    if (excelHeaderTemp.Length == 0)
                    {
                        emptyExcelColumnsMax--;
                        if (emptyExcelColumnsMax < 0) { break; }
                    }
                    else
                    {
                        headerMapWithIndex.Add(excelHeaderTemp, tempExcelIndex);
                    }
                    tempExcelIndex++;
                }

                List<List<string>> allBlocks = getBlockText(File.ReadAllLines(orgFilePath, encoding));
                int excelSearchingIndexRow = 2;
                for (int i = 0; i < allBlocks.Count; i++)
                {
                    List<string> outputBlock = allBlocks[i].ToList();
                    bool found = true;
                    foreach (string aHeader in toCheckHeaderList)
                    {
                        string excelContent = String.Empty;
                        try{
                            excelContent = worksheet.Cells[
                            BuCommon.ExcelColumnIndexToName(headerMapWithIndex[aHeader])
                                + excelSearchingIndexRow.ToString()].Text;
                        } catch(KeyNotFoundException) {
                            excelContent = "key_not_found";
                            found = false;
                            continue;
                        }
                        if (getBlockSingleText(allBlocks[i], aHeader, false)!= excelContent)
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                    {
                        for (int j = 0; j < outputBlock.Count; j++)
                        {
                            if (outputBlock[j].StartsWith(toImportHeader))
                            {
                                string excelContent = worksheet.Cells[
                                    BuCommon.ExcelColumnIndexToName(headerMapWithIndex[toImportHeader])
                                        + excelSearchingIndexRow.ToString()].Text;
                                outputBlock[j] = toImportHeader + excelContent;
                                break;
                            }
                        }
                    }
                    importedContent += blockToString(outputBlock);
                    excelSearchingIndexRow++;
                }
                File.WriteAllText(outputFilePath, importedContent, encoding);
            }
        }
        public static void convertExportedFileToExcel(string inputFile,
            Encoding encoding, string headerListStr, int[] listColumnWidth, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            List<string> headerList = Regex.Split(headerListStr, ",").ToList();
            foreach (string filePath in BuCommon.listFiles(inputFile))
            {
                List<List<string>> allBlocks = getBlockText(File.ReadAllLines(filePath, encoding));
                FileInfo newExcelFileInfo = new FileInfo(
                    outputDir + "\\" + Path.GetFileNameWithoutExtension(filePath) + ".xlsx");
                if (newExcelFileInfo.Exists) { newExcelFileInfo.Delete(); }
                ExcelPackage workbook = new ExcelPackage(newExcelFileInfo);
                workbook.Workbook.Properties.Title = Path.GetFileName(filePath);
                ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add(Path.GetFileNameWithoutExtension(filePath));
                //worksheet.DefaultColWidth = 70;

                int cellColumn = 0;
                int cellRow = 1;
                headerList.ForEach((oneHeader) =>{
                    worksheet.Cells[BuCommon.ExcelColumnIndexToName(cellColumn)
                        + cellRow.ToString()].Value = oneHeader;
                    cellColumn++;
                });
                worksheet.View.FreezePanes(2, 1);

                foreach (List<string> oneBlock in allBlocks)
                {
                    cellRow++;
                    worksheet.Row(cellRow).Style.Font.SetFromFont(new Font("Arial", 16));
                    bool isTakeThisTextOfHeader = false;
                    foreach (string oneBlockLine in oneBlock)
                    {
                        string tempCell= "A2";
                        Match headerMatch = Regex.Match(oneBlockLine, @"^<.+?>");
                        if (headerMatch.Success)
                        {
                            isTakeThisTextOfHeader = false;
                            for (int i = 0; i < headerList.Count; i++)
                            {
                                if (headerList[i].Equals(headerMatch.Value))
                                {
                                    tempCell = BuCommon.ExcelColumnIndexToName(i) + cellRow.ToString();
                                    worksheet.Cells[tempCell].Style.WrapText = true;
                                    worksheet.Cells[tempCell].Value = oneBlockLine.Substring(headerMatch.Length);
                                    isTakeThisTextOfHeader = true;
                                    break;
                                }
                            }
                        }
                        else if(isTakeThisTextOfHeader)
                        {
                            worksheet.Cells[tempCell].Value += " "+ oneBlockLine;
                        }
                    }
                }

                for (int i = 0; i < listColumnWidth.Length; i++)
                {
                    worksheet.Column(1+ i).Width = listColumnWidth[i];
                }
                workbook.Save();
            }
        }
        public static string getBlockSingleText(List<string> block, string header, bool spaceBetween)
        {
            if (null == header) { return ""; }
            string ret = "";
            bool isTextOfHead = false;
            foreach (string aLine in block)
            {
                if(aLine.StartsWith("<"))
                {
                    if (aLine.StartsWith(header))
                    {
                        isTextOfHead = true;
                        ret += aLine.Substring(header.Length);
                    }
                    else
                    {
                        isTextOfHead = false;
                    }
                }
                else if (isTextOfHead)
                {
                    ret += (spaceBetween && ret.Length > 0 ? " " : "") + aLine;
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

        public static void propertyAndTextFilter(string inputFile, Encoding encoding,
            string propName, string propFilterRegexStr,
            string lineFilterRegexStr, bool isAccept, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
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
                    string propValue = headerInfo[propName];
                    if (!Regex.IsMatch(propValue, propFilterRegexStr))
                    {
                        newFileContent += TransCommon.blockToString(aBlock);
                        continue;
                    }
                    bool found = false;
                    foreach(string aLine in aBlock)
                    {
                        if (Regex.IsMatch(aLine, lineFilterRegexStr)) { found = true; break; }
                    }
                    if ((isAccept && found) || (!isAccept && !found))
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

        public static string formatJpSentence(string input, string example)
        {
            string addToHead = "";
            string addToTail = "";

            if(example== null)
            {
                if (input.StartsWith("「") && !input.EndsWith("」")) { addToTail = "」"; }
                if (!input.StartsWith("「") && input.EndsWith("」")) { addToHead = "「"; }
                if (input.StartsWith("『") && !input.EndsWith("』")) { addToTail = "』"; }
                if (!input.StartsWith("『") && input.EndsWith("』")) { addToHead = "『"; }
                if (input.StartsWith("（") && !input.EndsWith("）")) { addToTail = "）"; }
                if (!input.StartsWith("（") && input.EndsWith("）")) { addToHead = "（"; }
            }
            else
            {
                if (example.StartsWith("「") && !input.StartsWith("「")) { addToHead = "「"; }
                if (example.StartsWith("『") && !input.StartsWith("『")) { addToHead = "『"; }
                if (example.StartsWith("（") && !input.StartsWith("（")) { addToHead = "（"; }
                if (example.EndsWith("」") && !input.EndsWith("」")) { addToTail = "」"; }
                if (example.EndsWith("』") && !input.EndsWith("』")) { addToTail = "』"; }
                if (example.EndsWith("）") && !input.EndsWith("）")) { addToTail = "）"; }
            }

            return addToHead+ input + addToTail;
        }

        public static void updateTranslation(string fromDir, Encoding encoding
            , string orgLineHeader, string transLineHeader, string toDir, string outputDir, bool searchFromBegin)
        {
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(fromDir))
            {
                string toFilePath = toDir;
                if (File.GetAttributes(@toDir).HasFlag(FileAttributes.Directory))
                {
                    toFilePath += "\\" + Path.GetFileName(fromFilePath);
                }
                if (!File.Exists(toFilePath)) { continue; }
                List<List<string>> fromBlocks = getBlockText(File.ReadAllLines(fromFilePath, encoding));
                List<List<string>> toBlocks = getBlockText(File.ReadAllLines(toFilePath, encoding));
                //getBlockSingleText
                string newFileContent = "";

                int pauseBlockIndex = 0;
                for (int i = 0; i < toBlocks.Count; i++)
                {
                    string toOrgTxt = getBlockSingleText(toBlocks[i], orgLineHeader, false);
                    bool found = false;
                    string note = "";
                    for (int j = pauseBlockIndex; j < fromBlocks.Count; j++)
                    {
                        string fromOrgTxt = getBlockSingleText(fromBlocks[j], orgLineHeader, false);
                        string fromTransTxt = getBlockSingleText(fromBlocks[j], transLineHeader, false);
                        Dictionary<string, string> transInfo = getInfoFromString(fromBlocks[j][0]);
                        note = transInfo.ContainsKey(INFO_NOTE_HEAD) ? transInfo[INFO_NOTE_HEAD] : "";

                        if (fromOrgTxt.Equals(toOrgTxt))
                        {
                            int transLineIndex = -1;
                            for (int k = 0; k < toBlocks[i].Count; k++)
                            {
                                if (toBlocks[i][k].StartsWith(transLineHeader))
                                {
                                    toBlocks[i][k] = transLineHeader + fromTransTxt;
                                    transLineIndex = k;
                                    break;
                                }
                            }
                            for (int k = 0; k < fromBlocks[j].Count; k++)
                            {
                                if (!fromBlocks[j][k].StartsWith("<"))
                                {
                                    toBlocks[i][transLineIndex] += Environment.NewLine + fromBlocks[j][k];
                                }
                            }
                            pauseBlockIndex = searchFromBegin ? 0 : (j + 1);
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        if(null!= note && note.Length> 0) toBlocks[i][0] +=
                            INFO_SEPARATOR_STR +
                            INFO_NOTE_HEAD +
                            INFO_PARAM_SEPARATOR_STR +
                            note;
                    }
                    else
                    {
                        toBlocks[i][0] +=
                            INFO_SEPARATOR_STR +
                            INFO_NOTE_HEAD +
                            INFO_PARAM_SEPARATOR_STR
                            + "not_found";
                    }
                    newFileContent += blockToString(toBlocks[i]);
                }
                String outputFile = outputDir + "\\" + Path.GetFileName(toFilePath);
                File.WriteAllText(outputFile, newFileContent, encoding);
            }
        }
        public static void wrap(string inputDir, Encoding encoding,
            string lineFilterRegex, string configFilePath, string outputDir)
        {
            Property aProp = new Property(configFilePath);
            aProp.reload();
            Font wrapFont = new Font(aProp.get(Property.Common.WRAP_FONT_NAME),
                float.Parse(aProp.get(Property.Common.WRAP_FONT_SIZE)));
            int maxWrap = Int32.Parse(aProp.get(Property.Common.WRAP_MAX));
            string wrapString = aProp.get(Property.Common.WRAP_STRING)
                .Replace("<singlenewline>", "\n")
                .Replace("<fullnewline>", "\r\n");
            Directory.CreateDirectory(outputDir);
            foreach (string fromFilePath in BuCommon.listFiles(inputDir))
            {
                string toFilePath = outputDir + "\\" + Path.GetFileName(fromFilePath);
                string[] inputFileArr = File.ReadAllLines(fromFilePath, encoding);

                for (int i = 0; i < inputFileArr.Length; i++)
                {
                    if (Regex.IsMatch(inputFileArr[i], lineFilterRegex))
                    {
                        inputFileArr[i] = PjProcessor.AbstractPjProcessor.textSizeWrap(
                            inputFileArr[i].Split(new string[] { wrapString }, StringSplitOptions.None),
                            wrapFont, maxWrap, wrapString, null,  out _);
                    }
                }
                File.WriteAllLines(toFilePath, inputFileArr, encoding);
            }
        }
        public static string quoteSentenceBaseOnOrg(string baseText, string sentence)
        {
            string ret = sentence;
            if (baseText.StartsWith("「") && !sentence.StartsWith("「")) { ret= "「" + ret; }
            if (baseText.StartsWith("『") && !sentence.StartsWith("『")) { ret= "『" + ret; }
            if (baseText.StartsWith("（") && !sentence.StartsWith("（")) { ret= "（" + ret; }
            if (baseText.StartsWith("“") && !sentence.StartsWith("“")) { ret= "“" + ret; }
            if (baseText.StartsWith("\"") && !sentence.StartsWith("\"")) { ret= "\"" + ret; }
            if (baseText.EndsWith("」") && !sentence.EndsWith("」")) { ret+= "」"; }
            if (baseText.EndsWith("』") && !sentence.EndsWith("』")) { ret+= "』"; }
            if (baseText.EndsWith("）") && !sentence.EndsWith("）")) { ret+= "）"; }
            if (baseText.EndsWith("”") && !sentence.EndsWith("”")) { ret+= "”"; }
            if (baseText.EndsWith("\"") && !sentence.EndsWith("\"")) { ret+= "\""; }
            return ret;
        }
        public static string htmlStyleGetProperty(string command, string propName)
        {//getPropertyFromCommand
            return Regex.Match(command, "(?<=" + propName + "=\").+?(?=\")").Value;
        }
        public static int htmlStyleGetIntProperty(string command, string propName)
        {
            string value = Regex.Match(command, "(?<=" + propName + "=\")\\d+?(?=\")").Value;
            return Int32.Parse(value);
        }


    }
}
