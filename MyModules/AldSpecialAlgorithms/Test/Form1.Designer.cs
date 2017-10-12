namespace Test
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
            this.components = new System.ComponentModel.Container();
            this.btnIniciar = new System.Windows.Forms.Button();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.plotData = new ZedGraph.ZedGraphControl();
            this.plotFFT = new ZedGraph.ZedGraphControl();
            this.SuspendLayout();
            // 
            // btnIniciar
            // 
            this.btnIniciar.Location = new System.Drawing.Point(12, 12);
            this.btnIniciar.Name = "btnIniciar";
            this.btnIniciar.Size = new System.Drawing.Size(149, 25);
            this.btnIniciar.TabIndex = 0;
            this.btnIniciar.Text = "Iniciar";
            this.btnIniciar.UseVisualStyleBackColor = true;
            this.btnIniciar.Click += new System.EventHandler(this.button1_Click);
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Location = new System.Drawing.Point(411, 312);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(8, 8);
            this.hScrollBar1.TabIndex = 1;
            // 
            // plotData
            // 
            this.plotData.Location = new System.Drawing.Point(12, 52);
            this.plotData.Name = "plotData";
            this.plotData.ScrollGrace = 0D;
            this.plotData.ScrollMaxX = 0D;
            this.plotData.ScrollMaxY = 0D;
            this.plotData.ScrollMaxY2 = 0D;
            this.plotData.ScrollMinX = 0D;
            this.plotData.ScrollMinY = 0D;
            this.plotData.ScrollMinY2 = 0D;
            this.plotData.Size = new System.Drawing.Size(855, 268);
            this.plotData.TabIndex = 2;
            // 
            // plotFFT
            // 
            this.plotFFT.Location = new System.Drawing.Point(12, 326);
            this.plotFFT.Name = "plotFFT";
            this.plotFFT.ScrollGrace = 0D;
            this.plotFFT.ScrollMaxX = 0D;
            this.plotFFT.ScrollMaxY = 0D;
            this.plotFFT.ScrollMaxY2 = 0D;
            this.plotFFT.ScrollMinX = 0D;
            this.plotFFT.ScrollMinY = 0D;
            this.plotFFT.ScrollMinY2 = 0D;
            this.plotFFT.Size = new System.Drawing.Size(855, 268);
            this.plotFFT.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(879, 605);
            this.Controls.Add(this.plotFFT);
            this.Controls.Add(this.plotData);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.btnIniciar);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnIniciar;
        private System.Windows.Forms.HScrollBar hScrollBar1;
        private ZedGraph.ZedGraphControl plotData;
        private ZedGraph.ZedGraphControl plotFFT;
    }
}

