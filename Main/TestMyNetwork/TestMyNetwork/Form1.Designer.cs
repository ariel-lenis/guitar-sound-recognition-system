namespace TestMyNetwork
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnDo = new System.Windows.Forms.Button();
            this.aldPlotterPoints1 = new TsExtraControls.AldPlotterPoints();
            this.btnTrain = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.aldPlotterPoints2 = new TsExtraControls.AldPlotterPoints();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDo
            // 
            this.btnDo.Location = new System.Drawing.Point(12, 12);
            this.btnDo.Name = "btnDo";
            this.btnDo.Size = new System.Drawing.Size(135, 37);
            this.btnDo.TabIndex = 0;
            this.btnDo.Text = "Do!!!";
            this.btnDo.UseVisualStyleBackColor = true;
            this.btnDo.Click += new System.EventHandler(this.btnDo_Click);
            // 
            // aldPlotterPoints1
            // 
            this.aldPlotterPoints1.AxisX = ((TsExtraControls.Extra.AxisConfig)(resources.GetObject("aldPlotterPoints1.AxisX")));
            this.aldPlotterPoints1.AxisY = ((TsExtraControls.Extra.AxisConfig)(resources.GetObject("aldPlotterPoints1.AxisY")));
            this.aldPlotterPoints1.Location = new System.Drawing.Point(12, 224);
            this.aldPlotterPoints1.Name = "aldPlotterPoints1";
            this.aldPlotterPoints1.Size = new System.Drawing.Size(737, 255);
            this.aldPlotterPoints1.TabIndex = 1;
            this.aldPlotterPoints1.Title = "Title";
            this.aldPlotterPoints1.Load += new System.EventHandler(this.aldPlotterPoints1_Load);
            // 
            // btnTrain
            // 
            this.btnTrain.Location = new System.Drawing.Point(12, 55);
            this.btnTrain.Name = "btnTrain";
            this.btnTrain.Size = new System.Drawing.Size(135, 37);
            this.btnTrain.TabIndex = 2;
            this.btnTrain.Text = "Train";
            this.btnTrain.UseVisualStyleBackColor = true;
            this.btnTrain.Click += new System.EventHandler(this.btnTrain_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(12, 98);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(135, 37);
            this.btnTest.TabIndex = 3;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // aldPlotterPoints2
            // 
            this.aldPlotterPoints2.AxisX = ((TsExtraControls.Extra.AxisConfig)(resources.GetObject("aldPlotterPoints2.AxisX")));
            this.aldPlotterPoints2.AxisY = ((TsExtraControls.Extra.AxisConfig)(resources.GetObject("aldPlotterPoints2.AxisY")));
            this.aldPlotterPoints2.Location = new System.Drawing.Point(163, 12);
            this.aldPlotterPoints2.Name = "aldPlotterPoints2";
            this.aldPlotterPoints2.Size = new System.Drawing.Size(586, 206);
            this.aldPlotterPoints2.TabIndex = 4;
            this.aldPlotterPoints2.Title = "Title";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 141);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(135, 38);
            this.button1.TabIndex = 5;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(761, 491);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.aldPlotterPoints2);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.btnTrain);
            this.Controls.Add(this.aldPlotterPoints1);
            this.Controls.Add(this.btnDo);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnDo;
        private TsExtraControls.AldPlotterPoints aldPlotterPoints1;
        private System.Windows.Forms.Button btnTrain;
        private System.Windows.Forms.Button btnTest;
        private TsExtraControls.AldPlotterPoints aldPlotterPoints2;
        private System.Windows.Forms.Button button1;
    }
}

