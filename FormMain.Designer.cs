
namespace Hoshi_Translator
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openGuideFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openConfigFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.executeCmd = new System.Windows.Forms.Button();
            this.tbxCommand = new System.Windows.Forms.TextBox();
            this.openOutputFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(800, 24);
            this.mainMenuStrip.TabIndex = 0;
            this.mainMenuStrip.Text = "mainMenuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openGuideFileToolStripMenuItem,
            this.openConfigFileToolStripMenuItem,
            this.openOutputFileToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openGuideFileToolStripMenuItem
            // 
            this.openGuideFileToolStripMenuItem.Name = "openGuideFileToolStripMenuItem";
            this.openGuideFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openGuideFileToolStripMenuItem.Text = "Open guide file";
            this.openGuideFileToolStripMenuItem.Click += new System.EventHandler(this.openGuideFileToolStripMenuItem_Click);
            // 
            // openConfigFileToolStripMenuItem
            // 
            this.openConfigFileToolStripMenuItem.Name = "openConfigFileToolStripMenuItem";
            this.openConfigFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openConfigFileToolStripMenuItem.Text = "Open Config File";
            this.openConfigFileToolStripMenuItem.Click += new System.EventHandler(this.openConfigFileToolStripMenuItem_Click);
            // 
            // executeCmd
            // 
            this.executeCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.executeCmd.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.executeCmd.Location = new System.Drawing.Point(13, 405);
            this.executeCmd.Name = "executeCmd";
            this.executeCmd.Size = new System.Drawing.Size(203, 33);
            this.executeCmd.TabIndex = 2;
            this.executeCmd.Text = "Execute command";
            this.executeCmd.UseVisualStyleBackColor = true;
            this.executeCmd.Click += new System.EventHandler(this.executeCmd_Click);
            this.executeCmd.KeyUp += new System.Windows.Forms.KeyEventHandler(this.executeCmd_KeyUp);
            // 
            // tbxCommand
            // 
            this.tbxCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbxCommand.BackColor = System.Drawing.Color.DarkSlateGray;
            this.tbxCommand.Font = new System.Drawing.Font("Arial", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbxCommand.ForeColor = System.Drawing.SystemColors.Window;
            this.tbxCommand.Location = new System.Drawing.Point(13, 28);
            this.tbxCommand.Multiline = true;
            this.tbxCommand.Name = "tbxCommand";
            this.tbxCommand.Size = new System.Drawing.Size(775, 371);
            this.tbxCommand.TabIndex = 1;
            this.tbxCommand.WordWrap = false;
            this.tbxCommand.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbxCommand_KeyUp);
            // 
            // openOutputFileToolStripMenuItem
            // 
            this.openOutputFileToolStripMenuItem.Name = "openOutputFileToolStripMenuItem";
            this.openOutputFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openOutputFileToolStripMenuItem.Text = "Open output file";
            this.openOutputFileToolStripMenuItem.Click += new System.EventHandler(this.openOutputFileToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tbxCommand);
            this.Controls.Add(this.executeCmd);
            this.Controls.Add(this.mainMenuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Hoshi Translator";
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openGuideFileToolStripMenuItem;
        private System.Windows.Forms.Button executeCmd;
        private System.Windows.Forms.TextBox tbxCommand;
        private System.Windows.Forms.ToolStripMenuItem openConfigFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOutputFileToolStripMenuItem;
    }
}

