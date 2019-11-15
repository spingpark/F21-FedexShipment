namespace F21.Framework
{
    partial class frmFedEx_Shipments
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
            this.txtCartonId = new System.Windows.Forms.TextBox();
            this.txtWeight = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblPreviousCartonId = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.scaleSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scaleSetToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fedExCloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printLabelForPPBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtCartonId
            // 
            this.txtCartonId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCartonId.Location = new System.Drawing.Point(118, 22);
            this.txtCartonId.Name = "txtCartonId";
            this.txtCartonId.Size = new System.Drawing.Size(200, 21);
            this.txtCartonId.TabIndex = 0;
            this.txtCartonId.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCartonId_KeyDown);
            // 
            // txtWeight
            // 
            this.txtWeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtWeight.Location = new System.Drawing.Point(118, 46);
            this.txtWeight.Name = "txtWeight";
            this.txtWeight.Size = new System.Drawing.Size(200, 21);
            this.txtWeight.TabIndex = 1;
            this.txtWeight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtWeight_KeyDown);
            this.txtWeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtWeight_KeyPress);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.lblPreviousCartonId);
            this.groupBox1.Location = new System.Drawing.Point(44, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(374, 51);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Previous CartonID";
            // 
            // lblPreviousCartonId
            // 
            this.lblPreviousCartonId.AutoSize = true;
            this.lblPreviousCartonId.Location = new System.Drawing.Point(22, 24);
            this.lblPreviousCartonId.Name = "lblPreviousCartonId";
            this.lblPreviousCartonId.Size = new System.Drawing.Size(0, 12);
            this.lblPreviousCartonId.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(10, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "Carton";
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.White;
            this.groupBox2.Controls.Add(this.panel3);
            this.groupBox2.Controls.Add(this.panel2);
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Controls.Add(this.txtUserId);
            this.groupBox2.Controls.Add(this.txtCartonId);
            this.groupBox2.Controls.Add(this.txtWeight);
            this.groupBox2.Location = new System.Drawing.Point(44, 94);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(374, 114);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.Gray;
            this.panel3.Controls.Add(this.label3);
            this.panel3.Location = new System.Drawing.Point(47, 70);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(64, 18);
            this.panel3.TabIndex = 9;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(10, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "User ID";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Gray;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Location = new System.Drawing.Point(47, 46);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(64, 18);
            this.panel2.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(10, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "Weight";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gray;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(47, 22);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(64, 18);
            this.panel1.TabIndex = 8;
            // 
            // txtUserId
            // 
            this.txtUserId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUserId.Location = new System.Drawing.Point(118, 70);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(200, 21);
            this.txtUserId.TabIndex = 2;
            this.txtUserId.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtUserId_KeyDown);
            this.txtUserId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUserId_KeyPress);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scaleSetToolStripMenuItem,
            this.fedExCloseToolStripMenuItem,
            this.printLabelForPPBoxToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(461, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // scaleSetToolStripMenuItem
            // 
            this.scaleSetToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scaleSetToolStripMenuItem1});
            this.scaleSetToolStripMenuItem.Name = "scaleSetToolStripMenuItem";
            this.scaleSetToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.scaleSetToolStripMenuItem.Text = "Setting";
            // 
            // scaleSetToolStripMenuItem1
            // 
            this.scaleSetToolStripMenuItem1.Name = "scaleSetToolStripMenuItem1";
            this.scaleSetToolStripMenuItem1.Size = new System.Drawing.Size(123, 22);
            this.scaleSetToolStripMenuItem1.Text = "Scale Set";
            this.scaleSetToolStripMenuItem1.Click += new System.EventHandler(this.scaleSetToolStripMenuItem1_Click);
            // 
            // fedExCloseToolStripMenuItem
            // 
            this.fedExCloseToolStripMenuItem.Name = "fedExCloseToolStripMenuItem";
            this.fedExCloseToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.fedExCloseToolStripMenuItem.Text = "FedEx Close";
            this.fedExCloseToolStripMenuItem.Click += new System.EventHandler(this.fedExCloseToolStripMenuItem_Click);
            // 
            // printLabelForPPBoxToolStripMenuItem
            // 
            this.printLabelForPPBoxToolStripMenuItem.Name = "printLabelForPPBoxToolStripMenuItem";
            this.printLabelForPPBoxToolStripMenuItem.Size = new System.Drawing.Size(135, 20);
            this.printLabelForPPBoxToolStripMenuItem.Text = "Print Label For PPBox";
            this.printLabelForPPBoxToolStripMenuItem.Click += new System.EventHandler(this.printLabelForPPBoxToolStripMenuItem_Click);
            // 
            // frmFedEx_Shipments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(461, 228);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmFedEx_Shipments";
            this.Text = "FedEx Shipments";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmFedEx_Shipments_FormClosing);
            this.Load += new System.EventHandler(this.frmFedEx_Shipments_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCartonId;
        private System.Windows.Forms.TextBox txtWeight;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblPreviousCartonId;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem scaleSetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scaleSetToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem fedExCloseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printLabelForPPBoxToolStripMenuItem;
    }
}