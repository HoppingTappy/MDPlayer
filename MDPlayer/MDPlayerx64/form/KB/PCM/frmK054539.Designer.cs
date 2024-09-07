﻿#if X64
using MDPlayerx64.Properties;
#else
using MDPlayer.Properties;
#endif

namespace MDPlayer.form
{
    partial class frmK054539
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmK054539));
            pbScreen = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pbScreen).BeginInit();
            SuspendLayout();
            // 
            // pbScreen
            // 
            pbScreen.Location = new Point(0, 0);
            pbScreen.Margin = new Padding(4);
            pbScreen.Name = "pbScreen";
            pbScreen.Size = new Size(611, 330);
            pbScreen.TabIndex = 1;
            pbScreen.TabStop = false;
            pbScreen.MouseClick += pbScreen_MouseClick;
            // 
            // frmGA20
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.ControlDarkDark;
            ClientSize = new Size(611, 330);
            Controls.Add(pbScreen);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(5);
            MaximizeBox = false;
            Name = "frmK054539";
            Text = "K054539";
            FormClosed += frmK054539_FormClosed;
            Load += frmK054539_Load;
            Resize += frmK054539_Resize;
            ((System.ComponentModel.ISupportInitialize)pbScreen).EndInit();
            ResumeLayout(false);
        }

        #endregion

        public System.Windows.Forms.PictureBox pbScreen;
    }
}