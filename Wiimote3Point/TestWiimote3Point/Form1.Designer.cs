namespace TestWiimote3Point
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
            this.lblPosition = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.danTimer = new System.Windows.Forms.Timer(this.components);
            this.worldView1 = new TestWiimote3Point.WorldView();
            this.irSensorsView21 = new TestWiimote3Point.IRSensorsView();
            this.SuspendLayout();
            // 
            // lblPosition
            // 
            this.lblPosition.AutoSize = true;
            this.lblPosition.Location = new System.Drawing.Point(12, 174);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(44, 13);
            this.lblPosition.TabIndex = 1;
            this.lblPosition.Text = "Position";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(12, 12);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // danTimer
            // 
            this.danTimer.Interval = 10;
            this.danTimer.Tick += new System.EventHandler(this.danTimer_Tick);
            // 
            // worldView1
            // 
            this.worldView1.Location = new System.Drawing.Point(159, 41);
            this.worldView1.Name = "worldView1";
            this.worldView1.Size = new System.Drawing.Size(443, 372);
            this.worldView1.TabIndex = 4;
            // 
            // irSensorsView21
            // 
            this.irSensorsView21.Location = new System.Drawing.Point(12, 41);
            this.irSensorsView21.Name = "irSensorsView21";
            this.irSensorsView21.Size = new System.Drawing.Size(141, 130);
            this.irSensorsView21.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 425);
            this.Controls.Add(this.worldView1);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.irSensorsView21);
            this.Controls.Add(this.lblPosition);
            this.Name = "Form1";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPosition;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Timer danTimer;
        private IRSensorsView irSensorsView21;
        private WorldView worldView1;
    }
}

