﻿using Hoshi_Translator.PjProcessor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Hoshi_Translator
{
    public partial class FormMain : Form
    {
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);
        public FormMain()
        {
            InitializeComponent();
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED);

            if (AppProp.getIntProp(AppProp.APP_LOCATION_X) != Int32.MinValue)
            {
                Location = new Point(AppProp.getIntProp(AppProp.APP_LOCATION_X),
                    AppProp.getIntProp(AppProp.APP_LOCATION_Y));
                this.Size = new Size(AppProp.getIntProp(AppProp.INIT_SIZE_X),
                    AppProp.getIntProp(AppProp.INIT_SIZE_Y));
            }
            this.ResizeEnd += new EventHandler(frmMain_resizeEnd);

            ControlFunc.initTextBoxDropFile(tbxCommand);
            ControlFunc.implementTextBoxShortcut(tbxCommand);

            tbxCommand.Text = AppProp.getProp(AppProp.LAST_COMMAND);
            tbxCommand.SelectAll();
        }
        private void frmMain_resizeEnd(Object sender, EventArgs e)
        {
            AppProp.saveProp(AppProp.APP_LOCATION_X, this.Location.X.ToString());
            AppProp.saveProp(AppProp.APP_LOCATION_Y, this.Location.Y.ToString());
            AppProp.saveProp(AppProp.INIT_SIZE_X, this.Size.Width.ToString());
            AppProp.saveProp(AppProp.INIT_SIZE_Y, this.Size.Height.ToString());
        }

        private void executeCmd_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            saveProperty();
            string[] commandArr = tbxCommand.Text.Split(new String[] { Environment.NewLine }, StringSplitOptions.None);
            List<string> oneCommand = new List<string>();
            foreach(string commandLine in commandArr)
            {
                if(commandLine.Length== 0)
                {
                    if(oneCommand.Count> 0)
                    {
                        runCommand(oneCommand.ToArray());
                    }
                    oneCommand.Clear();
                }
                else if(!commandLine.StartsWith(TransCommon.COMMENT_STR))
                {
                    oneCommand.Add(commandLine.Equals("_") ? "" : commandLine);
                }
            }
            if(oneCommand.Count> 0)
            {
                runCommand(oneCommand.ToArray());
            }
            this.WindowState = FormWindowState.Normal;
            FlashWindow.Flash(this, 1);
            MessageBox.Show("Commands executed.");
        }
        private void runCommand(string[] args)
        {
            string pjName = args[0].Trim();
            string action = args[1].Trim();

            switch (pjName)
            {
                case "common":
                    if (action.Equals("test"))
                    {
                        //string sentence = "人を食らう化け物に全てを奪われたあのとき、あたしはヴァンパイアハンターになることを決意した。闘うことが唯一の正義だと信じて。";
                        //WebRequest request = WebRequest.Create("https://ichi.moe/cl/qr/?q=" + sentence + "&r=htr");
                        //request.Credentials = CredentialCache.DefaultCredentials;
                        string inputDir = args[2];
                        string outputDir = args[3];

                        string exportContent = "";
                        Directory.CreateDirectory(outputDir);
                        foreach (string fromFilePath in BuCommon.listFiles(inputDir))
                        {
                            string[] fileContent = File.ReadAllLines(fromFilePath, 
                                BuCommon.getEncodingFromString("utf-8-bom"));
                            exportContent = "";
                            Match tempMatch;
                            for (int i = 0; i < fileContent.Length; i++)
                            {
                                tempMatch = Regex.Match(fileContent[i], @"//<\d+?> ");
                                if (tempMatch.Success)
                                {
                                    string content= fileContent[i].Substring(tempMatch.Length);
                                    if (SiglusProcessor.getConvertedCharName(content) != null) { continue; }
                                    exportContent += content;
                                    exportContent += Environment.NewLine;
                                    exportContent += Environment.NewLine;
                                }
                            }

                            string outputFilePath = String.Format("{0}\\{1}", outputDir, Path.GetFileName(fromFilePath));
                            File.WriteAllText(outputFilePath, exportContent,
                                BuCommon.getEncodingFromString("utf-8-bom"));
                        }
                    }
                    if (action.Equals("font_list"))
                    {
                        List<string> fonts = new List<string>();

                        string fontsNameList = "";
                        foreach (FontFamily font in System.Drawing.FontFamily.Families)
                        {
                            fontsNameList += font.Name + Environment.NewLine;
                        }
                        File.WriteAllText(AppConst.OUTPUT_FILE, fontsNameList
                            , BuCommon.getEncodingFromString("utf-8-bom"));
                        openOutputFileToolStripMenuItem_Click(null, null);
                    }
                    if (action.Equals("replacer"))
                    {
                        string inputDir = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string filterRegex = args[4];
                        string outputDir = args[5];
                        string ruleFile = args.Length > 6 ? args[6] : AppConst.REPLACE_FILE;
                        Directory.CreateDirectory(outputDir);
                        List<KeyValuePair<string, string>> relaceList
                            = BuCommon.getReplaceList(ruleFile, AppConst.REPLACE_SEPARATOR);

                        foreach (string oneFilePath in BuCommon.listFiles(inputDir))
                        {
                            string[] fileContent = File.ReadAllLines(oneFilePath, encoding);

                            for (int i = 0; i < fileContent.Length; i++)
                            {
                                Match regexMatch = Regex.Match(fileContent[i], filterRegex);
                                if (regexMatch.Success)
                                {
                                    string lineContent = fileContent[i];
                                    for (int j = 0; j < relaceList.Count; j++)
                                    {
                                        lineContent = lineContent.Replace(relaceList[j].Key,
                                            @relaceList[j].Value
                                            .Replace("<new_line>", "\r\n")
                                            .Replace("<full_new_line>", "\r\n"));
                                    }
                                    fileContent[i] = lineContent;
                                }
                            }

                            String outputFile = outputDir + "\\" + Path.GetFileName(oneFilePath);
                            File.WriteAllLines(outputFile, fileContent, encoding);
                        }
                    }
                    if (action.Equals("regex_replacer"))
                    {
                        string inputDir = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string filterRegex = args[4];
                        string outputDir = args[5];
                        string ruleFile = args.Length > 6 ? args[6] : AppConst.REGEX_REPLACE_FILE;
                        Directory.CreateDirectory(outputDir);
                        List<KeyValuePair<string, string>> relaceList
                            = BuCommon.getReplaceList(ruleFile, AppConst.REPLACE_SEPARATOR);

                        foreach (string oneFilePath in BuCommon.listFiles(inputDir))
                        {
                            string[] fileContent = File.ReadAllLines(oneFilePath, encoding);

                            for (int i = 0; i < fileContent.Length; i++)
                            {
                                Match regexMatch = Regex.Match(fileContent[i], filterRegex);
                                if (regexMatch.Success)
                                {
                                    string lineContent = fileContent[i];
                                    for (int j = 0; j < relaceList.Count; j++)
                                    {
                                        lineContent = Regex.Replace(lineContent
                                            , @relaceList[j].Key, @relaceList[j].Value
                                            .Replace("<new_line>", "\r\n")
                                            .Replace("<full_new_line>", "\r\n"));
                                    }
                                    fileContent[i] = lineContent;
                                }
                            }

                            String outputFile = outputDir + "\\" + Path.GetFileName(oneFilePath);
                            File.WriteAllLines(outputFile, fileContent, encoding);
                        }
                    }
                    if (action.Equals("remove_org_text"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        Directory.CreateDirectory(outputDir);
                        Encoding encoding = BuCommon.getEncodingFromString("utf-8");
                        foreach (string filePath in BuCommon.listFiles(inputDir, "*.*"))
                        {
                            string fileNewContent = "";
                            foreach (string aLine in File.ReadAllLines(filePath))
                            {
                                if (!aLine.StartsWith(TransCommon.ORIGINAL_LINE_HEAD))
                                {
                                    fileNewContent += aLine + Environment.NewLine;
                                }
                            }
                            string outputPath = String.Format("{0}\\{1}"
                                , outputDir, Path.GetFileName(filePath));
                            File.WriteAllText(outputPath, fileNewContent, encoding);
                        }
                    }
                    if (action.Equals("splittosize"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        long partSize = long.Parse(args[4]);
                        string partName = args[5];

                        long sizeCount = 0;
                        int partCount = 1;

                        foreach (string filePath in BuCommon.listFiles(inputDir))
                        {
                            FileInfo fileInfo = new FileInfo(filePath);
                            sizeCount += fileInfo.Length;
                            if (sizeCount/ 1048576 >= partSize)
                            {
                                sizeCount = fileInfo.Length;
                                partCount++;
                            }
                            String newFilePath = String.Format("{0}\\{1}_part{2}{3}",
                                outputDir, partName, partCount, filePath.Substring(inputDir.Length));
                            if (!Directory.Exists(Path.GetDirectoryName(newFilePath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
                            }
                            File.Copy(filePath, newFilePath, true);
                        }
                    }
                    if (action.Equals("property_and_line_filter"))
                    {
                        string inputFile = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string propName = args[4];
                        string expressionString = args[5];
                        string regexStr = args[6];
                        bool isAccept = args[7].Equals("accept");
                        string outputDir = args[8];
                        TransCommon.propertyAndTextFilter(inputFile, encoding,
                            propName, expressionString, regexStr, isAccept, outputDir);
                    }
                    if (action.Equals("update_translation"))
                    {
                        string fromFile = args[2];
                        Encoding fromEncoding = BuCommon.getEncodingFromString(args[3]);
                        string orgLineHeader = args[4];
                        string transLineHeader = args[5];
                        string toFile = args[6];
                        string outputDir= args[7]; Directory.CreateDirectory(outputDir);
                        bool searchFromBegin = args[8].ToLower().Equals("true");
                        TransCommon.updateTranslation(fromFile, fromEncoding,
                            orgLineHeader, transLineHeader, toFile, outputDir, searchFromBegin);
                    }
                    if (action.Equals("quote_base_on_other"))
                    {
                        string inputDir = args[2];
                        Encoding encoding= BuCommon.getEncodingFromString(args[3].ToLower());
                        string baseHeader = args[4];
                        string toQuoteHeader = args[5];
                        string outputDir = args[6];
                        Directory.CreateDirectory(outputDir);

                        string baseTemp = "";
                        string toQuoteTemp = "";
                        foreach (string fromFilePath in BuCommon.listFiles(inputDir))
                        {
                            string[] fileContent = File.ReadAllLines(fromFilePath, encoding);
                            for (int i = 0; i < fileContent.Length; i++)
                            {
                                if (fileContent[i].StartsWith(baseHeader))
                                {
                                    baseTemp = fileContent[i].Substring(baseHeader.Length);
                                    continue;
                                }
                                if (fileContent[i].StartsWith(toQuoteHeader))
                                {
                                    toQuoteTemp = fileContent[i].Substring(toQuoteHeader.Length);
                                    if (toQuoteTemp.Length == 0) { continue; }
                                    fileContent[i]= toQuoteHeader
                                        + TransCommon.quoteSentenceBaseOnOrg(baseTemp, toQuoteTemp);
                                    continue;
                                }
                            }

                            string outputFile = outputDir + fromFilePath.Substring(inputDir.Length);
                            File.WriteAllLines(outputFile, fileContent, encoding);
                        }
                    }
                    if (action.Equals("wrap"))
                    {
                        string inputDir = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string lineFilterRegex = args[4];
                        string outputDir = args[5];
                        string configFilePath= args.Length > 6 ? args[6] : AppConst.CONFIG_FILE;
                        TransCommon.wrap(inputDir, encoding, lineFilterRegex, configFilePath, outputDir);
                    }
                    if (action.Equals("get_filtered_line"))
                    {
                        string fromFilePath = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string filterRegex= args[4];
                        string takeOutRegex= args[5];
                        bool useDiff= args[6].ToLower().Equals("true");
                        string outputFile = args[7];
                        List<string> outputContent = new List<string>();

                        if (outputFile.Length > 0) { Directory.CreateDirectory(outputFile); }
                        foreach (string oneFilePath in BuCommon.listFiles(fromFilePath))
                        {
                            string[] fileContent = File.ReadAllLines(oneFilePath, encoding);

                            for (int i = 0; i < fileContent.Length; i++)
                            {
                                string line = fileContent[i];
                                if (Regex.IsMatch(line, filterRegex))
                                {
                                    string toTakeOut = Regex.Match(line, takeOutRegex).Value;
                                    if (null == toTakeOut || toTakeOut.Length== 0) { continue; }
                                    if (useDiff && outputContent.Contains(toTakeOut)) { continue; }
                                    outputContent.Add(toTakeOut);
                                }
                            }
                            if (outputFile.Length> 0)
                            {
                                String outputFilePath = outputFile + "\\" + Path.GetFileName(oneFilePath);
                                File.WriteAllLines(outputFilePath, outputContent, encoding);
                                outputContent.Clear();
                            }
                        }
                        if (outputFile.Length== 0)
                        {
                            File.WriteAllLines(AppConst.OUTPUT_FILE, outputContent, encoding);
                        }
                    }
                    if (action.Equals("convert_exported_file_to_excel"))
                    {
                        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                        string inputDir = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string headerListStr = args[4];
                        string columnWidthStr = args[5];
                        string outputDir = args[6];
                        TransCommon.convertExportedFileToExcel(inputDir, encoding, headerListStr
                            , Regex.Split(columnWidthStr, ",")
                                .Select(w => w.Trim().Length== 0 ? 70 : int.Parse(w.Trim())).ToArray()
                            , outputDir);
                    }
                    if (action.Equals("import_excel_file_to_trans"))
                    {
                        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                        string inputDir = args[2];
                        string toCheckHeaderListStr = args[3];
                        string toImportHeader = args[4];
                        string orgDir = args[5];
                        Encoding encoding = BuCommon.getEncodingFromString(args[6]);
                        string outputDir = args[7];
                        TransCommon.importExcelFileToTrans(inputDir,
                            toCheckHeaderListStr, toImportHeader
                            , orgDir, encoding, outputDir);
                    }
                    break;
                case "file":
                    if (action.Equals("take_out"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        Directory.CreateDirectory(outputDir);
                        bool deleteSource= args[4].ToLower().Equals("true");
                        string separator = "(=)";

                        foreach (string inputOneFile in BuCommon.listFiles(inputDir))
                        {
                            string newName = inputOneFile.Substring(inputDir.Length + 1);
                            newName = newName.Replace("\\", separator);
                            if (deleteSource)
                            {
                                File.Move(inputOneFile, outputDir + "\\" + newName);
                            }
                            else
                            {
                                try
                                {
                                    File.Copy(inputOneFile, outputDir + "\\" + newName);
                                }
                                catch { }
                            }
                        }
                    }
                    if (action.Equals("put_in"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        Directory.CreateDirectory(outputDir);
                        bool deleteSource = args[4].ToLower().Equals("true");
                        string separator = "(=)";

                        foreach (string inputOneFile in BuCommon.listFiles(inputDir))
                        {
                            string newName = inputOneFile.Substring(inputDir.Length + 1);
                            newName = newName.Replace(separator, "\\");
                            string newDir = Path.GetDirectoryName(outputDir + "\\" + newName);
                            if (!Directory.Exists(newDir))
                            {
                                Directory.CreateDirectory(newDir);
                            }
                            if (deleteSource)
                            {
                                File.Move(inputOneFile, outputDir + "\\" + newName);
                            }
                            else
                            {
                                File.Copy(inputOneFile, outputDir + "\\" + newName);
                            }
                        }
                    }
                    if (action.Equals("delete"))
                    {
                        string inputDir = args[2];
                        bool isAccept= args[3].ToLower().Equals("accept");
                        string fileNameRegex = args[4];
                        string filePathRegex= args[5];

                        foreach (string filePath in BuCommon.listFiles(inputDir))
                        {
                            if(filePathRegex.Length> 0)
                            {
                                if (isAccept && !Regex.IsMatch(filePath, filePathRegex)) { continue; }
                                if (!isAccept && Regex.IsMatch(filePath, filePathRegex)) { continue; }
                            }
                            string fileFullName = Path.GetFileName(filePath);
                            if(fileNameRegex.Length> 0)
                            {
                                if (isAccept && !Regex.IsMatch(fileFullName, fileNameRegex)) { continue; }
                                if (!isAccept && Regex.IsMatch(fileFullName, fileNameRegex)) { continue; }
                            }
                            File.Delete(filePath);
                        }
                    }
                    if (action.Equals("list"))
                    {
                        string inputDir = args[2];
                        string captureRegex = args[3];
                        string resultRegex = args[4];
                        string content = "";

                        foreach (string filePath in BuCommon.listFiles(inputDir))
                        {
                            Match match = Regex.Match(filePath, captureRegex);
                            if (!match.Success) { continue; }
                            content += Regex.Replace(filePath, captureRegex, resultRegex)+ Environment.NewLine;
                        }
                        File.WriteAllText(AppConst.OUTPUT_FILE, content
                            , BuCommon.getEncodingFromString("utf-8-bom"));
                        openOutputFileToolStripMenuItem_Click(null, null);
                    }
                    if (action.Equals("encode_base64"))
                    {
                        string inputDir = args[2];
                        List<string> allOldFilesPath = new List<string>();

                        foreach (string filePath in BuCommon.listFiles(inputDir))
                        {
                            allOldFilesPath.Add(filePath);
                            string newName = Path.GetFileName(filePath);
                            newName= Convert.ToBase64String(Encoding.UTF8.GetBytes(newName));
                            File.Copy(filePath, Path.GetDirectoryName(filePath) + "\\"+ newName);
                        }
                        foreach (string filePath in allOldFilesPath)
                        {
                            File.Delete(filePath);
                        }
                    }
                    if (action.Equals("rename"))
                    {
                        string inputDir = args[2];
                        string nameFilterRegex = args[3];
                        string replaceToName = args[4];
                        bool deleteSource = args[5].ToLower().Equals("true");
                        string outputDir = args[6];
                        Directory.CreateDirectory(outputDir);

                        foreach (string inputOneFile in BuCommon.listFiles(inputDir))
                        {
                            //string fileDir = Path.GetDirectoryName(inputOneFile);
                            string fileName = Path.GetFileName(inputOneFile);
                            if (!Regex.IsMatch(fileName, nameFilterRegex)) { continue; }

                            //process
                            fileName = Regex.Replace(fileName, nameFilterRegex, replaceToName);

                            string newPath = Path.Combine(outputDir, fileName);
                            if (deleteSource)
                            {
                                File.Move(inputOneFile, newPath);
                            }
                            else
                            {
                                try
                                {
                                    File.Copy(inputOneFile, newPath);
                                }
                                catch { }
                            }
                        }
                    }
                    if (action.Equals("set_date_time"))
                    {
                        string inputDir = args[2];
                        int y = Int32.Parse(args[3]);
                        int m = Int32.Parse(args[4]);
                        int d = Int32.Parse(args[5]);
                        int h = Int32.Parse(args[6]);
                        int min = Int32.Parse(args[7]);
                        int s = Int32.Parse(args[8]);
                        int ms = Int32.Parse(args[9]);

                        foreach (string oneFilePath in BuCommon.listFiles(inputDir))
                        {
                            File.SetCreationTime(oneFilePath,
                                new DateTime(y,m,d,h,min,s,ms));
                            File.SetLastWriteTime(oneFilePath,
                                new DateTime(y, m, d, h, min, s, ms));
                        }
                    }
                    break;
                case "girls_guild":
                    RpgMVProcessor girlsGuildProcessor = new RpgMVProcessor();
                    girlsGuildProcessor.loadDefault(true);
                    if (action.Equals("concat"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];

                        int argsDelta = 1;
                        List<string> patternList = new List<string>();
                        while (3 + argsDelta < args.Length && args[3 + argsDelta].Length > 0)
                        {
                            patternList.Add(@args[3 + argsDelta]);
                            argsDelta++;
                        }
                        girlsGuildProcessor.girlsGuildConcat(inputDir, outputDir, patternList.ToArray());
                    }
                    break;
                case "rpg_mv":
                    RpgMVProcessor rpgMVProcessor = new RpgMVProcessor();
                    rpgMVProcessor.loadDefault(true);
                    if (action.Equals("export"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        string codeFilter = args[4];
                        rpgMVProcessor.export(inputDir, outputDir, codeFilter);
                    }
                    if (action.Equals("filter_for_code"))
                    {
                        string code = args[2];
                        string acceptOrIgnore = args[3];
                        string regexStr = args[4];
                        string inputFile = args[5];
                        string outputDir = args[6];
                        rpgMVProcessor.filterForCode(code, acceptOrIgnore.Equals("accept"), regexStr, inputFile, outputDir);
                    }
                    if (action.Equals("concat"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];

                        int argsDelta = 1;
                        List<string> patternList = new List<string>();
                        while (3 + argsDelta < args.Length && args[3 + argsDelta].Length > 0)
                        {
                            patternList.Add(@args[3 + argsDelta]);
                            argsDelta++;
                        }
                        rpgMVProcessor.concat(inputDir, outputDir, patternList.ToArray());
                    }
                    if (action.Equals("update"))
                    {
                        string fromDir = args[2];
                        string toDir = args[3];
                        string outputDir = args[4];
                        rpgMVProcessor.update(fromDir, toDir, outputDir);
                    }
                    if (action.Equals("wrap"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        string wrapReplaceFilePath = args.Length > 4 ? args[4] : null;
                        rpgMVProcessor.wrap(inputDir, outputDir, wrapReplaceFilePath);
                    }
                    if (action.Equals("import"))
                    {
                        string tranDir = args[2];
                        string orgDir = args[3];
                        string outputDir = args[4];
                        Directory.CreateDirectory(outputDir);
                        foreach (string oneTransFile in BuCommon.listFiles(tranDir))
                        {
                            String orgFilePath = orgDir + "\\" + Path.GetFileNameWithoutExtension(oneTransFile) + ".json";
                            String outputFilePath = outputDir + "\\" + Path.GetFileNameWithoutExtension(oneTransFile) + ".json";
                            rpgMVProcessor.importOneFile(oneTransFile, orgFilePath, outputFilePath);
                        }
                    }
                    if (action.Equals("fill"))
                    {
                        string fillFilePath = args[2];
                        string fullText = "";
                        string transText = "";

                        int argsDelta = 1;
                        while (2 + argsDelta < args.Length && args[2 + argsDelta].Length > 0)
                        {
                            string lineText = args[2 + argsDelta];
                            if (lineText.StartsWith(TransCommon.FULL_TEXT_BOX_LINE_HEAD))
                            {
                                fullText = lineText.Substring(TransCommon.FULL_TEXT_BOX_LINE_HEAD.Length);
                            }
                            if (lineText.StartsWith(TransCommon.TRANSLATED_LINE_HEAD))
                            {
                                transText = lineText.Substring(TransCommon.TRANSLATED_LINE_HEAD.Length);
                            }
                            if (!lineText.StartsWith("<"))
                            {
                                transText += Environment.NewLine + lineText;
                            }
                            argsDelta++;
                        }

                        rpgMVProcessor.fillDuplicate(fullText, transText, fillFilePath);
                    }
                    if (action.Equals("ts_decode") || action.Equals("ts_encode"))
                    {
                        string inputDir = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        byte decodeKey = Byte.Parse(args[4]);
                        string outputDir = args[5];
                        string newExt = args[6];

                        rpgMVProcessor.tsDecode(inputDir, encoding, decodeKey, outputDir, newExt);
                    }
                    if (action.Equals("nightmare_school_export"))
                    {
                        string inputDir = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string outputDir = args[4];

                        Directory.CreateDirectory(outputDir);
                        foreach (string filePath in BuCommon.listFiles(inputDir))
                        {
                            string toFilePath = outputDir + "\\" + Path.GetFileName(filePath);
                            string[] inputFileArr = File.ReadAllLines(filePath, encoding);
                            string exportContent = "";
                            for (int i = 0; i < inputFileArr.Length; i++)
                            {
                                string tempLine = inputFileArr[i].Trim();
                                if (tempLine.Length== 0) { continue; }
                                if (Regex.IsMatch(tempLine, @"@(?!select)")) { continue; }
                                if (tempLine.StartsWith("*")) { continue; }
                                if (tempLine.StartsWith(";")) { continue; }
                                Match charNameMatch = Regex.Match(inputFileArr[i], @"^\[.+?\]");

                                Dictionary<string, string> headerInfo = new Dictionary<string, string>();
                                headerInfo.Add(TransCommon.INFO_LINE_HEAD, (i + 1).ToString());
                                List<string> tempBlock = new List<string>();
                                string blockString = "";
                                tempBlock.Add(TransCommon.genInfoString(headerInfo));
                                tempBlock.Add(TransCommon.ORIGINAL_LINE_HEAD + inputFileArr[i]);
                                if (charNameMatch.Success)
                                {
                                    tempBlock.Add(TransCommon.CHAR_NAME_LINE_HEAD + charNameMatch.Value);
                                    tempBlock.Add(TransCommon.FULL_TEXT_BOX_LINE_HEAD
                                        + inputFileArr[i].Substring(charNameMatch.Length).Replace("\\n", String.Empty));
                                }
                                else if(inputFileArr[i].ToLower().StartsWith("\\cl"))
                                {
                                    tempBlock.Add(TransCommon.CHAR_NAME_LINE_HEAD + "\\CL ");
                                    tempBlock.Add(TransCommon.FULL_TEXT_BOX_LINE_HEAD
                                        + inputFileArr[i].Substring(3).Replace("\\n", String.Empty));
                                }
                                else
                                {
                                    tempBlock.Add(TransCommon.FULL_TEXT_BOX_LINE_HEAD
                                        + inputFileArr[i].Replace("\\n", String.Empty));
                                }
                                tempBlock.Add(TransCommon.TRANSLATED_LINE_HEAD);
                                tempBlock.Add("");

                                foreach (string blockLine in tempBlock)
                                {
                                    blockString += blockLine + Environment.NewLine;
                                }
                                exportContent += blockString;
                            }
                            if(exportContent.Length> 0)
                            {
                                File.WriteAllText(toFilePath, exportContent, encoding);
                            }
                        }
                    }
                    if (action.Equals("nightmare_school_import"))
                    {
                        string tranDir = args[2];
                        string orgDir = args[3];
                        string outputDir = args[4];
                        Encoding encoding = BuCommon.getEncodingFromString("utf-8");
                        Directory.CreateDirectory(outputDir);
                        foreach (string filePath in BuCommon.listFiles(tranDir))
                        {
                            String orgFilePath = orgDir + "\\" + Path.GetFileNameWithoutExtension(filePath) + ".txt";
                            string[] importFileContent = File.ReadAllLines(orgFilePath, encoding);
                            String outputFilePath = outputDir + "\\" + Path.GetFileNameWithoutExtension(filePath) + ".txt";
                            List<List<string>> allBlocks
                                = TransCommon.getBlockText(File.ReadAllLines(filePath, encoding));
                            foreach(List<string> aBlock in allBlocks)
                            {
                                Dictionary<string, string> blockInfo = TransCommon.getInfoFromString(aBlock[0]);
                                string charName = TransCommon.getBlockSingleText(aBlock, TransCommon.CHAR_NAME_LINE_HEAD, false);
                                string sentence = TransCommon.getBlockSingleText(aBlock, TransCommon.TRANSLATED_LINE_HEAD, true);
                                string fullText = TransCommon.getBlockSingleText(aBlock, TransCommon.FULL_TEXT_BOX_LINE_HEAD, true);
                                if (Regex.IsMatch(charName, @"^\[.+?\]"))
                                {
                                    sentence = TransCommon.quoteSentenceBaseOnOrg(fullText, sentence);
                                }
                                importFileContent[Int32.Parse(blockInfo[TransCommon.INFO_LINE_HEAD])- 1]
                                    = charName + sentence;
                            }
                            File.WriteAllLines(outputFilePath, importFileContent, encoding);
                        }
                    }
                    if (action.Equals("decrypt"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        rpgMVProcessor.decrypt(inputDir, outputDir);
                    }
                    break;
                case "rpg_vx_ace":
                    RpgVxAceProcessor rpgVxAceProcessor = new RpgVxAceProcessor();
                    rpgVxAceProcessor.loadDefault(true);
                    if (action.Equals("export"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        string typeFilter = args[4];
                        foreach (string filePath in BuCommon.listFiles(AppConst.RPGM_TRANS_DIR_, "*.rvdata2"))
                        {
                            File.Delete(filePath);
                        }
                        foreach (string filePath in BuCommon.listFiles(AppConst.RPGM_TRANS_DIR_, "*.json"))
                        {
                            File.Delete(filePath);
                        }
                        Directory.CreateDirectory(outputDir + "\\scripts");
                        foreach (string filePath in BuCommon.listFiles(AppConst.RPGM_TRANS_DIR_ + "scripts", "*.*"))
                        {
                            File.Copy(filePath, outputDir + "\\scripts\\" + Path.GetFileName(filePath), true);
                        }
                        if (Directory.Exists(AppConst.RPGM_TRANS_DIR_ + "scripts"))
                        {
                            Directory.Delete(AppConst.RPGM_TRANS_DIR_ + "scripts", true);
                        }
                        foreach (string filePath in BuCommon.listFiles(inputDir))
                        {
                            File.Copy(filePath, AppConst.RPGM_TRANS_DIR_ + filePath.Substring(inputDir.Length + 1), true);
                        }
                        //ruby rmvxace_translator.rb --translate=*.rvdata2 --dest=H:\rpg_port_project\ParanormalSyndromeR\ParanormalSyndromeR\Data
                        using (var proc = new Process())
                        {
                            var startInfo = new ProcessStartInfo(@"ruby");
                            startInfo.WorkingDirectory = AppConst.RPGM_TRANS_DIR_;
                            startInfo.Arguments = "rmvxace_translator.rb --dump=*.rvdata2";
                            startInfo.UseShellExecute = false;
                            startInfo.CreateNoWindow = false;
                            proc.StartInfo = startInfo;
                            proc.Start();
                            proc.WaitForExit();
                        }
                        rpgVxAceProcessor.export(AppConst.RPGM_TRANS_DIR_, outputDir, typeFilter);
                    }
                    if (action.Equals("filter_for_type"))
                    {
                        string type = args[2];
                        string acceptOrIgnore = args[3];
                        string regexStr = args[4];
                        string inputFile = args[5];
                        string outputDir = args[6];
                        rpgVxAceProcessor.filterForType(type, acceptOrIgnore.Equals("accept"), regexStr, inputFile, outputDir);
                    }
                    if (action.Equals("wrap"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        string wrapReplaceFilePath = args.Length > 4 ? args[4] : null;
                        rpgVxAceProcessor.wrap(inputDir, outputDir, wrapReplaceFilePath);
                    }
                    if (action.Equals("import"))
                    {
                        string tranDir = args[2];
                        string outputDir = args[3];
                        Directory.CreateDirectory(outputDir);
                        foreach (string filePath in BuCommon.listFiles(tranDir))
                        {
                            string orgFile = String.Format("{0}{1}.json",
                                AppConst.RPGM_TRANS_DIR_, Path.GetFileNameWithoutExtension(filePath));
                            rpgVxAceProcessor.importOneFile(filePath, orgFile);
                        }
                        using (var proc = new Process())
                        {
                            var startInfo = new ProcessStartInfo(@"ruby");
                            startInfo.WorkingDirectory = AppConst.RPGM_TRANS_DIR_;
                            startInfo.Arguments = "rmvxace_translator.rb --translate=*.rvdata2 --dest="+ outputDir;
                            startInfo.UseShellExecute = false;
                            startInfo.CreateNoWindow = false;
                            proc.StartInfo = startInfo;
                            proc.Start();
                            proc.WaitForExit();
                        }
                    }
                    break;
                case "katawa":
                    if (action.Equals("quote_fix"))
                    {
                        string input = args[2];
                        string output = args[3];
                        Encoding encoding = BuCommon.getEncodingFromString("utf-8");
                        string[] inputs = File.ReadAllLines(input, encoding);

                        for (int i = 0; i < inputs.Length; i++)
                        {
                            if (inputs[i].StartsWith("<trans_text>"))
                            {
                                String temp = inputs[i];
                                String quoteFixed = "";
                                if (!temp.Contains("\"")) { continue; }
                                String[] splited = temp.Split('\"');
                                if (splited.Length % 2 == 0) { continue; }
                                bool openQuote = true;
                                for (int j = 0; j < splited.Length; j++)
                                {
                                    quoteFixed += splited[j];
                                    quoteFixed += (openQuote ? "“" : "”");
                                    openQuote = !openQuote;
                                }
                                if (quoteFixed.Length > 0)
                                {
                                    inputs[i] = quoteFixed.Substring(0, quoteFixed.Length - 1)
                                        .Replace("“, ”", "\", \"")
                                        .Replace("“, u”", "\", u\"");
                                }
                            }

                        }
                        String outputFile = output+ "\\" + Path.GetFileName(input);
                        Directory.CreateDirectory(output);
                        File.WriteAllLines(outputFile, inputs, encoding);
                    }
                    break;
                case "siglus":
                    SiglusProcessor siglusProcessor = new SiglusProcessor();
                    siglusProcessor.loadDefault(true);
                    if (action.Equals("simple_export_jp"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        siglusProcessor.simpleExport(inputFile, outputDir);
                    }
                    if (action.Equals("export"))
                    {
                        string inputFile = args[2];
                        string lang = args[3];
                        string outputDir = args[4];
                        siglusProcessor.export(inputFile, outputDir, lang);
                    }
                    if (action.Equals("import"))
                    {
                        string inputFile = args[2];
                        string orgtDir = args[3];
                        string outputDir = args[4];
                        siglusProcessor.import(inputFile, orgtDir, outputDir);
                    }
                    if (action.Equals("ss_wrap"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        siglusProcessor.ssWrap(inputDir, outputDir);
                    }
                    break;
                case "tyrano":
                    TyranoProcessor tyranoProcessor = new TyranoProcessor();
                    tyranoProcessor.loadDefault(true);
                    if (action.Equals("simple_export"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        tyranoProcessor.simpleExport(inputFile, outputDir);
                    }
                    break;
                case "chaos":
                    ChaosProcessor chaosProcessor = new ChaosProcessor();
                    chaosProcessor.loadDefault(true);
                    if (action.Equals("export"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        chaosProcessor.export(inputFile, outputDir);
                    }
                    if (action.Equals("concat"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        chaosProcessor.concat(inputFile, outputDir);
                    }
                    if (action.Equals("wrap"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        string wrapReplaceFilePath= null;
                        if (args.Length > 4) { wrapReplaceFilePath = args[4]; }
                        chaosProcessor.wrap(inputFile, outputDir, wrapReplaceFilePath);
                    }
                    if (action.Equals("import"))
                    {
                        string inputFile = args[2];
                        string orgDir = args[3];
                        string outputDir = args[4];
                        chaosProcessor.import(inputFile, orgDir, outputDir);
                    }
                    break;
                case "kirikiri_json":
                    KirikiriJsonProcessor kirikiriJsonProcessor = new KirikiriJsonProcessor();
                    kirikiriJsonProcessor.loadDefault(true);
                    if (action.Equals("export"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        string jsonPathFilterRegex = args[4];
                        kirikiriJsonProcessor.export(inputFile, outputDir, jsonPathFilterRegex);
                    }
                    if (action.Equals("import"))
                    {
                        string inputFile = args[2];
                        string orgDir = args[3];
                        string outputDir = args[4];
                        kirikiriJsonProcessor.import(inputFile, orgDir, outputDir);
                    }
                    break;
                case "kirikiri":
                    KirikiriProcessor kirikiriProcessor = new KirikiriProcessor();
                    kirikiriProcessor.loadDefault(true);
                    if (action.Equals("export"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        kirikiriProcessor.export(inputFile, outputDir);
                    }
                    if (action.Equals("concat"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        int argsDelta = 1;
                        List<string> patternList = new List<string>();
                        while (3 + argsDelta < args.Length && args[3 + argsDelta].Length > 0)
                        {
                            patternList.Add(@args[3 + argsDelta]);
                            argsDelta++;
                        }
                        kirikiriProcessor.concat(inputFile, outputDir, patternList.ToArray());
                    }
                    if (action.Equals("concat2"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        kirikiriProcessor.concat2(inputFile, outputDir);
                    }
                    break;
                case "vinahoshi":
                    VinaHoshiProcessor vinaHoshiProcessor = new VinaHoshiProcessor();
                    if (action.Equals("add_video_info"))
                    {
                        string ffmpegPath = args[2];
                        string inputFile = args[3];
                        string outputDir = args[4];
                        vinaHoshiProcessor.addVideoInfoToFileNam(ffmpegPath, inputFile, outputDir);
                    }
                    if (action.Equals("add_image_info"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        vinaHoshiProcessor.addImageInfoToFileNam(inputFile, outputDir);
                    }
                    if (action.Equals("generate_magic_path"))
                    {
                        string inputFile = args[2];
                        vinaHoshiProcessor.generateMagicPath(inputFile);
                        openOutputFileToolStripMenuItem_Click(null, null);
                    }
                    if (action.Equals("parse_one_night_cross"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        vinaHoshiProcessor.parseOneNightCrossScript(inputFile, outputDir);
                    }
                    if (action.Equals("add_kana_to_script"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        vinaHoshiProcessor.addKana(inputFile, outputDir);
                    }
                    break;
                case "saku_uta":
                    SakuUta sakuUta = new SakuUta();
                    sakuUta.loadDefault(true);
                    if (action.Equals("import"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        sakuUta.import(inputFile, outputDir);
                    }
                    break;
                case "sonohana":
                    SonohanaProcessor sonohana = new SonohanaProcessor();
                    sonohana.loadDefault(true);
                    if (action.Equals("export"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        sonohana.export(inputFile, outputDir);
                    }
                    if (action.Equals("export2"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        sonohana.export2(inputFile, outputDir);
                    }
                    if (action.Equals("import"))
                    {
                        string inputFiles = args[2];
                        string orgFiles = args[3];
                        string outputDir = args[4];
                        sonohana.import(inputFiles, orgFiles, outputDir);
                    }
                    break;
                case "propeller":
                    PropellerProcessor propellerProcessor = new PropellerProcessor();
                    propellerProcessor.loadDefault(true);
                    if (action.Equals("simple_export"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        propellerProcessor.simpleExport(inputFile, outputDir);
                    }
                    break;
            }
        }
        private void saveProperty()
        {
            AppProp.saveProp(AppProp.LAST_COMMAND, tbxCommand.Text);
        }

        private void tbxCommand_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                tbxCommand.Focus();
                executeCmd.PerformClick();
            }
            if (e.KeyCode == Keys.Escape)
            {
                if(tbxCommand.Text.Length== 0)
                {
                    Application.Exit();
                }
                else
                {
                    tbxCommand.Clear();
                }
            }
        }

        private void executeCmd_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                tbxCommand.Focus();
                tbxCommand.SelectAll();
            }
        }

        private void openGuideFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(AppConst.GUIDE_FILE))
            {
                System.Diagnostics.Process.Start(AppConst.GUIDE_FILE);
            }
        }

        private void openConfigFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(AppConst.CONFIG_FILE))
            {
                System.Diagnostics.Process.Start(AppConst.CONFIG_FILE);
            }
        }

        private void openOutputFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(AppConst.OUTPUT_FILE))
            {
                System.Diagnostics.Process.Start(AppConst.OUTPUT_FILE);
            }
        }
    }
}
