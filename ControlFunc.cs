using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Hoshi_Translator
{
    class ControlFunc
    {
        public static void buildComboBoxFromSource(ComboBox cbb, Dictionary<string, string> source, int startKey)
        {
            cbb.DisplayMember = "Value";
            cbb.ValueMember = "Key";
            //cbb.DataSource = new BindingSource(source, null);

            cbb.Items.Clear();
            foreach (var each in source)
            {
                cbb.Items.Add(new KeyValuePair<string, string>(each.Key, each.Value));
            }
            if (cbb.Items.Count > 0 && startKey < cbb.Items.Count)
            {
                cbb.SelectedIndex = startKey < 0 ? 0 : startKey;
            }
        }

        public static void implementTextBoxShortcut(TextBox txt)
        {
            txt.KeyDown += textBox_KeyDown;
        }
        private static void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (e.Control && e.KeyCode == Keys.A)
            {
                txt.SelectAll();
            }
        }

        public static void initTextBoxDropFile(TextBox target)
        {
            target.AllowDrop = true;
            target.DragOver += new DragEventHandler(commonTextBox_DragOver);
            target.DragDrop += new DragEventHandler(commonTextBox_DragDrop);
        }
        private static void commonTextBox_DragDrop(object sender, DragEventArgs e)
        {
            int currentLine = ((TextBox)sender).GetLineFromCharIndex(((TextBox)sender).SelectionStart);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (System.IO.File.Exists(files[0]) || System.IO.Directory.Exists(files[0]))
                {
                    string[] curValue = ((TextBox)sender).Lines;
                    curValue[currentLine] = files[0];
                    ((TextBox)sender).Lines = curValue;
                    ((TextBox)sender).SelectionStart = 0;
                    for (int i = 0; i <= currentLine; i++)
                    {
                        SendKeys.Send("{END}");
                        SendKeys.Send("{END}");
                        SendKeys.Send("{DOWN}");
                    }
                    SendKeys.Send("{HOME}");
                    SendKeys.Send("{HOME}");
                }
            }
        }
        private static void commonTextBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
    }
}
