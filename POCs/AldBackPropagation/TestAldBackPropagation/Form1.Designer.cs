namespace TestAldBackPropagation
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
            this.btnIniciar = new System.Windows.Forms.Button();
            this.aldPlotterPoints1 = new TsExtraControls.AldPlotterPoints();
            this.lblActualError = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnIniciar
            // 
            this.btnIniciar.Location = new System.Drawing.Point(12, 12);
            this.btnIniciar.Name = "btnIniciar";
            this.btnIniciar.Size = new System.Drawing.Size(134, 31);
            this.btnIniciar.TabIndex = 0;
            this.btnIniciar.Text = "Iniciar";
            this.btnIniciar.UseVisualStyleBackColor = true;
            this.btnIniciar.Click += new System.EventHandler(this.btnIniciar_Click);
            // 
            // aldPlotterPoints1
            // 
            //this.aldPlotterPoints1.AxisX = ((TsExtraControls.AldPlotterPoints.AxisConfig)(resources.GetObject("aldPlotterPoints1.AxisX")));
            //this.aldPlotterPoints1.AxisY = ((TsExtraControls.AldPlotterPoints.AxisConfig)(resources.GetObject("aldPlotterPoints1.AxisY")));
            this.aldPlotterPoints1.Location = new System.Drawing.Point(7, 58);
            this.aldPlotterPoints1.Name = "aldPlotterPoints1";
            this.aldPlotterPoints1.Size = new System.Drawing.Size(859, 409);
            this.aldPlotterPoints1.TabIndex = 1;
            this.aldPlotterPoints1.Title = "Aprendizaje de la red";
            this.aldPlotterPoints1.Load += new System.EventHandler(this.aldPlotterPoints1_Load);
            // 
            // lblActualError
            // 
            this.lblActualError.AutoSize = true;
            this.lblActualError.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActualError.Location = new System.Drawing.Point(8, 486);
            this.lblActualError.Name = "lblActualError";
            this.lblActualError.Size = new System.Drawing.Size(105, 20);
            this.lblActualError.TabIndex = 2;
            this.lblActualError.Text = "Actual Error";
            this.lblActualError.Click += new System.EventHandler(this.label1_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(753, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 31);
            this.button1.TabIndex = 3;
            this.button1.Text = "Testear";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(878, 515);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblActualError);
            this.Controls.Add(this.aldPlotterPoints1);
            this.Controls.Add(this.btnIniciar);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnIniciar;
        private TsExtraControls.AldPlotterPoints aldPlotterPoints1;
        private System.Windows.Forms.Label lblActualError;
        private System.Windows.Forms.Button button1;
    }
}

