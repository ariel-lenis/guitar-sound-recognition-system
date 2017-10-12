namespace AldExtractMidiFromUrl
{
    partial class Form1
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
            this.txtGetMidis = new System.Windows.Forms.TextBox();
            this.btnGetMidis = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.gridUrls = new System.Windows.Forms.DataGridView();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.gridUrls)).BeginInit();
            this.SuspendLayout();
            // 
            // txtGetMidis
            // 
            this.txtGetMidis.Location = new System.Drawing.Point(49, 12);
            this.txtGetMidis.Name = "txtGetMidis";
            this.txtGetMidis.Size = new System.Drawing.Size(617, 20);
            this.txtGetMidis.TabIndex = 0;
            // 
            // btnGetMidis
            // 
            this.btnGetMidis.Location = new System.Drawing.Point(672, 12);
            this.btnGetMidis.Name = "btnGetMidis";
            this.btnGetMidis.Size = new System.Drawing.Size(103, 20);
            this.btnGetMidis.TabIndex = 1;
            this.btnGetMidis.Text = "Get Midi\'s";
            this.btnGetMidis.UseVisualStyleBackColor = true;
            this.btnGetMidis.Click += new System.EventHandler(this.btnGetMidis_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Url:";
            // 
            // btnDownload
            // 
            this.btnDownload.Enabled = false;
            this.btnDownload.Location = new System.Drawing.Point(672, 413);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(103, 23);
            this.btnDownload.TabIndex = 6;
            this.btnDownload.Text = "Download All";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // gridUrls
            // 
            this.gridUrls.AllowUserToAddRows = false;
            this.gridUrls.AllowUserToDeleteRows = false;
            this.gridUrls.AllowUserToOrderColumns = true;
            this.gridUrls.AllowUserToResizeRows = false;
            this.gridUrls.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridUrls.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridUrls.Location = new System.Drawing.Point(12, 38);
            this.gridUrls.Name = "gridUrls";
            this.gridUrls.ReadOnly = true;
            this.gridUrls.RowHeadersVisible = false;
            this.gridUrls.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridUrls.Size = new System.Drawing.Size(763, 369);
            this.gridUrls.TabIndex = 7;
            this.gridUrls.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridUrls_CellContentClick);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 413);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(654, 23);
            this.progressBar1.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(787, 447);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.gridUrls);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGetMidis);
            this.Controls.Add(this.txtGetMidis);
            this.Name = "Form1";
            this.Text = "Download Midi";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridUrls)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtGetMidis;
        private System.Windows.Forms.Button btnGetMidis;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.DataGridView gridUrls;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

