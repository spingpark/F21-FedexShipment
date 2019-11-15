namespace F21.Framework
{
    partial class frmFedExClose
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
            this.dtWorkDate = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbFedExAccount = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnFedExClose = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTotalCloseCount = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dtWorkDate
            // 
            this.dtWorkDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtWorkDate.Location = new System.Drawing.Point(357, 23);
            this.dtWorkDate.Name = "dtWorkDate";
            this.dtWorkDate.Size = new System.Drawing.Size(94, 20);
            this.dtWorkDate.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnFedExClose);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cbFedExAccount);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.dtWorkDate);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(631, 64);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(292, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Work Date";
            // 
            // cbFedExAccount
            // 
            this.cbFedExAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFedExAccount.FormattingEnabled = true;
            this.cbFedExAccount.Location = new System.Drawing.Point(113, 22);
            this.cbFedExAccount.Name = "cbFedExAccount";
            this.cbFedExAccount.Size = new System.Drawing.Size(121, 21);
            this.cbFedExAccount.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "FedEx Account";
            // 
            // btnFedExClose
            // 
            this.btnFedExClose.ForeColor = System.Drawing.Color.Black;
            this.btnFedExClose.Location = new System.Drawing.Point(497, 22);
            this.btnFedExClose.Name = "btnFedExClose";
            this.btnFedExClose.Size = new System.Drawing.Size(103, 23);
            this.btnFedExClose.TabIndex = 26;
            this.btnFedExClose.Text = "FedEx CLOSE";
            this.btnFedExClose.UseVisualStyleBackColor = true;
            this.btnFedExClose.Click += new System.EventHandler(this.btnFedExClose_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtTotalCloseCount);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 64);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(631, 48);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Tag = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "FedEx Total Count";
            this.label3.Visible = false;
            // 
            // txtTotalCloseCount
            // 
            this.txtTotalCloseCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTotalCloseCount.Location = new System.Drawing.Point(135, 19);
            this.txtTotalCloseCount.Name = "txtTotalCloseCount";
            this.txtTotalCloseCount.Size = new System.Drawing.Size(172, 20);
            this.txtTotalCloseCount.TabIndex = 27;
            this.txtTotalCloseCount.Visible = false;
            // 
            // frmFedExClose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(631, 112);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmFedExClose";
            this.Text = "frmFedExClose";
            this.Load += new System.EventHandler(this.frmFedExClose_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dtWorkDate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbFedExAccount;
        private System.Windows.Forms.Button btnFedExClose;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTotalCloseCount;

    }
}