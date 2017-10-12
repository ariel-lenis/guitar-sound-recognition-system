namespace AldMidiWrapper
{
    partial class AldWrapper
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
            this.components = new System.ComponentModel.Container();
            this.panelPlay = new System.Windows.Forms.Panel();
            this.progressFiles = new System.Windows.Forms.ProgressBar();
            this.btnRecordAll = new System.Windows.Forms.Button();
            this.btnStartRecord = new System.Windows.Forms.Button();
            this.panelConfig = new System.Windows.Forms.Panel();
            this.sliderProgress = new System.Windows.Forms.TrackBar();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnUncheckAll = new System.Windows.Forms.Button();
            this.btnCheckAll = new System.Windows.Forms.Button();
            this.cbInstrument = new System.Windows.Forms.ComboBox();
            this.ckListInstruments = new System.Windows.Forms.CheckedListBox();
            this.panelLoad = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.listMidiFiles = new System.Windows.Forms.ListBox();
            this.ckForceInstrument = new System.Windows.Forms.CheckBox();
            this.ckJustFirst = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.panelPlay.SuspendLayout();
            this.panelConfig.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sliderProgress)).BeginInit();
            this.panelLoad.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelPlay
            // 
            this.panelPlay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPlay.Controls.Add(this.progressFiles);
            this.panelPlay.Controls.Add(this.btnRecordAll);
            this.panelPlay.Controls.Add(this.btnStartRecord);
            this.panelPlay.Controls.Add(this.panelConfig);
            this.panelPlay.Enabled = false;
            this.panelPlay.Location = new System.Drawing.Point(352, 315);
            this.panelPlay.Name = "panelPlay";
            this.panelPlay.Size = new System.Drawing.Size(526, 216);
            this.panelPlay.TabIndex = 1;
            // 
            // progressFiles
            // 
            this.progressFiles.Location = new System.Drawing.Point(7, 175);
            this.progressFiles.Name = "progressFiles";
            this.progressFiles.Size = new System.Drawing.Size(508, 35);
            this.progressFiles.TabIndex = 9;
            // 
            // btnRecordAll
            // 
            this.btnRecordAll.Location = new System.Drawing.Point(7, 142);
            this.btnRecordAll.Name = "btnRecordAll";
            this.btnRecordAll.Size = new System.Drawing.Size(509, 27);
            this.btnRecordAll.TabIndex = 8;
            this.btnRecordAll.Text = "Record All";
            this.btnRecordAll.UseVisualStyleBackColor = true;
            this.btnRecordAll.Click += new System.EventHandler(this.btnRecordAll_Click);
            // 
            // btnStartRecord
            // 
            this.btnStartRecord.Location = new System.Drawing.Point(7, 109);
            this.btnStartRecord.Name = "btnStartRecord";
            this.btnStartRecord.Size = new System.Drawing.Size(510, 27);
            this.btnStartRecord.TabIndex = 6;
            this.btnStartRecord.Text = "StartRecord";
            this.btnStartRecord.UseVisualStyleBackColor = true;
            this.btnStartRecord.Click += new System.EventHandler(this.btnStartRecord_Click);
            // 
            // panelConfig
            // 
            this.panelConfig.Controls.Add(this.sliderProgress);
            this.panelConfig.Controls.Add(this.btnStop);
            this.panelConfig.Controls.Add(this.btnPlay);
            this.panelConfig.Location = new System.Drawing.Point(0, 3);
            this.panelConfig.Name = "panelConfig";
            this.panelConfig.Size = new System.Drawing.Size(523, 100);
            this.panelConfig.TabIndex = 7;
            // 
            // sliderProgress
            // 
            this.sliderProgress.Location = new System.Drawing.Point(7, 12);
            this.sliderProgress.Maximum = 1000;
            this.sliderProgress.Name = "sliderProgress";
            this.sliderProgress.Size = new System.Drawing.Size(509, 45);
            this.sliderProgress.TabIndex = 1;
            this.sliderProgress.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.sliderProgress.ValueChanged += new System.EventHandler(this.sliderProgress_ValueChanged);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(94, 63);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(40, 27);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(15, 63);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(73, 27);
            this.btnPlay.TabIndex = 2;
            this.btnPlay.Text = "Play/Pause";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnUncheckAll
            // 
            this.btnUncheckAll.Location = new System.Drawing.Point(429, 277);
            this.btnUncheckAll.Name = "btnUncheckAll";
            this.btnUncheckAll.Size = new System.Drawing.Size(79, 32);
            this.btnUncheckAll.TabIndex = 7;
            this.btnUncheckAll.Text = "None";
            this.btnUncheckAll.UseVisualStyleBackColor = true;
            this.btnUncheckAll.Click += new System.EventHandler(this.btnUncheckAll_Click);
            // 
            // btnCheckAll
            // 
            this.btnCheckAll.Location = new System.Drawing.Point(350, 277);
            this.btnCheckAll.Name = "btnCheckAll";
            this.btnCheckAll.Size = new System.Drawing.Size(79, 32);
            this.btnCheckAll.TabIndex = 6;
            this.btnCheckAll.Text = "All";
            this.btnCheckAll.UseVisualStyleBackColor = true;
            this.btnCheckAll.Click += new System.EventHandler(this.btnCheckAll_Click);
            // 
            // cbInstrument
            // 
            this.cbInstrument.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cbInstrument.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cbInstrument.FormattingEnabled = true;
            this.cbInstrument.Location = new System.Drawing.Point(350, 56);
            this.cbInstrument.Name = "cbInstrument";
            this.cbInstrument.Size = new System.Drawing.Size(301, 21);
            this.cbInstrument.TabIndex = 5;
            this.cbInstrument.SelectedIndexChanged += new System.EventHandler(this.cbInstrument_SelectedIndexChanged);
            this.cbInstrument.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbInstrument_KeyDown);
            // 
            // ckListInstruments
            // 
            this.ckListInstruments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ckListInstruments.FormattingEnabled = true;
            this.ckListInstruments.Location = new System.Drawing.Point(352, 103);
            this.ckListInstruments.Name = "ckListInstruments";
            this.ckListInstruments.Size = new System.Drawing.Size(515, 169);
            this.ckListInstruments.TabIndex = 2;
            this.ckListInstruments.SelectedIndexChanged += new System.EventHandler(this.ckListInstruments_SelectedIndexChanged);
            // 
            // panelLoad
            // 
            this.panelLoad.Controls.Add(this.progressBar1);
            this.panelLoad.Location = new System.Drawing.Point(351, 7);
            this.panelLoad.Name = "panelLoad";
            this.panelLoad.Size = new System.Drawing.Size(527, 46);
            this.panelLoad.TabIndex = 2;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(3, 5);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(518, 38);
            this.progressBar1.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Midi files|*.mid";
            this.openFileDialog1.InitialDirectory = "C:\\Tesis\\Tools\\TestFiles";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "WAV|*.wav";
            this.saveFileDialog1.InitialDirectory = "C:\\Tesis\\Tools\\TestFiles";
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(12, 503);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(332, 28);
            this.btnSelectFolder.TabIndex = 3;
            this.btnSelectFolder.Text = "Select Folder";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // listMidiFiles
            // 
            this.listMidiFiles.FormattingEnabled = true;
            this.listMidiFiles.Location = new System.Drawing.Point(12, 12);
            this.listMidiFiles.Name = "listMidiFiles";
            this.listMidiFiles.Size = new System.Drawing.Size(332, 485);
            this.listMidiFiles.TabIndex = 4;
            this.listMidiFiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listMidiFiles_MouseDoubleClick);
            // 
            // ckForceInstrument
            // 
            this.ckForceInstrument.AutoSize = true;
            this.ckForceInstrument.Location = new System.Drawing.Point(352, 80);
            this.ckForceInstrument.Name = "ckForceInstrument";
            this.ckForceInstrument.Size = new System.Drawing.Size(60, 17);
            this.ckForceInstrument.TabIndex = 8;
            this.ckForceInstrument.Text = "Default";
            this.ckForceInstrument.UseVisualStyleBackColor = true;
            this.ckForceInstrument.CheckedChanged += new System.EventHandler(this.ckForceInstrument_CheckedChanged);
            // 
            // ckJustFirst
            // 
            this.ckJustFirst.AutoSize = true;
            this.ckJustFirst.Location = new System.Drawing.Point(418, 80);
            this.ckJustFirst.Name = "ckJustFirst";
            this.ckJustFirst.Size = new System.Drawing.Size(119, 17);
            this.ckJustFirst.TabIndex = 9;
            this.ckJustFirst.Text = "Just First Instrument";
            this.ckJustFirst.UseVisualStyleBackColor = true;
            this.ckJustFirst.CheckedChanged += new System.EventHandler(this.ckJustFirst_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(675, 59);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 24);
            this.button1.TabIndex = 10;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AldWrapper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 537);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnUncheckAll);
            this.Controls.Add(this.btnCheckAll);
            this.Controls.Add(this.ckJustFirst);
            this.Controls.Add(this.ckForceInstrument);
            this.Controls.Add(this.listMidiFiles);
            this.Controls.Add(this.ckListInstruments);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.panelLoad);
            this.Controls.Add(this.cbInstrument);
            this.Controls.Add(this.panelPlay);
            this.Name = "AldWrapper";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Midi Labeling for Training";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panelPlay.ResumeLayout(false);
            this.panelConfig.ResumeLayout(false);
            this.panelConfig.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sliderProgress)).EndInit();
            this.panelLoad.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelPlay;
        private System.Windows.Forms.TrackBar sliderProgress;
        private System.Windows.Forms.ComboBox cbInstrument;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Panel panelLoad;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.CheckedListBox ckListInstruments;
        private System.Windows.Forms.Button btnStartRecord;
        private System.Windows.Forms.Panel panelConfig;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button btnRecordAll;
        private System.Windows.Forms.Button btnUncheckAll;
        private System.Windows.Forms.Button btnCheckAll;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.ListBox listMidiFiles;
        private System.Windows.Forms.CheckBox ckJustFirst;
        private System.Windows.Forms.CheckBox ckForceInstrument;
        private System.Windows.Forms.ProgressBar progressFiles;
        private System.Windows.Forms.Button button1;
    }
}

