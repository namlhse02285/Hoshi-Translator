using Hoshi_Translator.PjProcessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
                else
                {
                    oneCommand.Add(commandLine);
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
                        string strTest = "{	Stand(\"st梨深_私服_通常\",\"normal\", 300, @+200);}awkjedhiw . ,";
                        string reg = @"\(.+?\);";
                        MatchCollection tempFuncMatches = Regex.Matches(strTest, @"\(.*?\);");
                        string sentence = strTest;
                        if (tempFuncMatches.Count == 0)
                        {
                            sentence = sentence.Replace(".", "&.").Replace(",", "&,");
                        }
                        else
                        {
                            List<string> splitSentence = new List<string>();
                            int lastIndex = 0;
                            foreach (Match aFuncMatch in tempFuncMatches)
                            {
                                splitSentence.Add(sentence.Substring(lastIndex, aFuncMatch.Index)
                                    .Replace(".", "&.").Replace(",", "&,"));
                                splitSentence.Add(aFuncMatch.Value);
                                lastIndex = aFuncMatch.Index + aFuncMatch.Length;
                            }
                            splitSentence.Add(sentence.Substring(lastIndex)
                                .Replace(".", "&.").Replace(",", "&,"));
                            sentence = String.Join("", splitSentence);
                        }
                        Debug.WriteLine(sentence);
                        //MessageBox.Show(Regex.IsMatch(t1, reg).ToString());
                        //MessageBox.Show(String.Format("[\"{0}\"]", "aaa"));
                        //Directory.Move(@"G:\s\u_all\u122\Watashi no H wa Watashi ni Makasete.", @"G:\s\u_all\u122\a1");
                    }
                    if (action.Equals("font_list"))
                    {
                        List<string> fonts = new List<string>();

                        string fontsNameList = "";
                        foreach (FontFamily font in System.Drawing.FontFamily.Families)
                        {
                            fontsNameList += font.Name + Environment.NewLine;
                        }
                        MessageBox.Show(fontsNameList);
                    }
                    if (action.Equals("replacer"))
                    {
                        string inputDir = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string filterRegex = args[4];
                        string outputDir = args[5];
                        string ruleFile = AppConst.REPLACE_FILE;
                        if (args.Length > 6)
                        {
                            ruleFile = args[6];
                        }
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
                        string ruleFile = AppConst.REGEX_REPLACE_FILE;
                        if (args.Length > 6)
                        {
                            ruleFile = args[6];
                        }
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
                            if (sizeCount >= partSize)
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
                    if (action.Equals("property_and_text_filter"))
                    {
                        string inputFile = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string propName = args[4];
                        string expressionString = args[5];
                        bool isAccept = args[6].Equals("accept");
                        string regexStr = args[7];
                        string outputDir = args[8];
                        TransCommon.propertyAndTextFilter(propName, expressionString,
                            isAccept, regexStr, inputFile, encoding, outputDir);
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
                    if (action.Equals("single_take_out_diff_filtered_line"))
                    {
                        string fromFilePath = args[2];
                        Encoding encoding = BuCommon.getEncodingFromString(args[3]);
                        string filterRegex= args[4];
                        string takeOutRegex= args[5];
                        string outputFile = AppConst.OUTPUT_FILE;
                        if(args.Length> 6)
                        {
                            outputFile = args[6];
                        }
                        List<string> outputContent = new List<string>();
                        foreach (string oneFilePath in BuCommon.listFiles(fromFilePath))
                        {
                            string[] fileContent = File.ReadAllLines(oneFilePath, encoding);

                            for (int i = 0; i < fileContent.Length; i++)
                            {
                                string line = fileContent[i];
                                if (Regex.IsMatch(line, filterRegex))
                                {
                                    string toTakeOut = Regex.Match(line, takeOutRegex).Value;
                                    if (null!= toTakeOut && toTakeOut.Length> 0
                                        && !outputContent.Contains(toTakeOut))
                                    {
                                        outputContent.Add(toTakeOut);
                                    }
                                }
                            }
                            if (!File.Exists(outputFile))
                            {
                                String outputFilePath = outputFile + "\\" + Path.GetFileName(oneFilePath);
                                File.WriteAllLines(outputFilePath, outputContent, encoding);
                                outputContent.Clear();
                            }
                        }
                        if (File.Exists(outputFile))
                        {
                            File.WriteAllLines(outputFile, outputContent, encoding);
                        }
                    }
                    break;
                case "file":
                    if (action.Equals("take_out"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
                        string separator = "(=)";

                        foreach (string inputOneFile in BuCommon.listFiles(inputDir))
                        {
                            string newName = inputOneFile.Substring(inputDir.Length + 1);
                            newName = newName.Replace("\\", separator);
                            File.Move(inputOneFile, outputDir + "\\" + newName);
                        }
                    }
                    if (action.Equals("put_in"))
                    {
                        string inputDir = args[2];
                        string outputDir = args[3];
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
                            File.Move(inputOneFile, outputDir + "\\" + newName);
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
                        string wrapReplaceFilePath = null;
                        if (args.Length > 4) { wrapReplaceFilePath = args[4]; }
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
                                if (tempLine.StartsWith("@")) { continue; }
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
                                    tempBlock.Add(TransCommon.CHAR_NAME_LINE_HEAD + "\\CL");
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
                    if (action.Equals("nightmare_school_wrap"))
                    {

                    }
                    if (action.Equals("nightmare_school_import"))
                    {

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
                        string wrapReplaceFilePath = null;
                        if (args.Length > 4) { wrapReplaceFilePath = args[4]; }
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
                    if (action.Equals("export_eng"))
                    {
                        string inputFile = args[2];
                        string outputDir = args[3];
                        siglusProcessor.exportEng(inputFile, outputDir);
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
    }
}
