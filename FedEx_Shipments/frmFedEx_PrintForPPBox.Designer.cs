namespace F21.Framework
{
    partial class frmFedEx_PrintForPPBox
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
            this.dtSearchDate = new System.Windows.Forms.DateTimePicker();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtItemCode = new System.Windows.Forms.TextBox();
            this.txtPONumber = new System.Windows.Forms.TextBox();
            this.rbItemCode = new System.Windows.Forms.RadioButton();
            this.rbPONumber = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rbPPAPX = new System.Windows.Forms.RadioButton();
            this.rbPPFedex = new System.Windows.Forms.RadioButton();
            this.rbShoes = new System.Windows.Forms.RadioButton();
            this.btnReload = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dtSearchDate
            // 
            this.dtSearchDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtSearchDate.Location = new System.Drawing.Point(61, 28);
            this.dtSearchDate.Name = "dtSearchDate";
            this.dtSearchDate.Size = new System.Drawing.Size(94, 20);
            this.dtSearchDate.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnClose);
            this.groupBox1.Controls.Add(this.btnPrint);
            this.groupBox1.Controls.Add(this.groupBox5);
            this.groupBox1.Controls.Add(this.groupBox4);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.btnReload);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(750, 173);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // btnClose
            // 
            this.btnClose.ForeColor = System.Drawing.Color.Black;
            this.btnClose.Location = new System.Drawing.Point(639, 129);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(103, 23);
            this.btnClose.TabIndex = 31;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.ForeColor = System.Drawing.Color.Black;
            this.btnPrint.Location = new System.Drawing.Point(511, 129);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(103, 23);
            this.btnPrint.TabIndex = 30;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtItemCode);
            this.groupBox5.Controls.Add(this.txtPONumber);
            this.groupBox5.Controls.Add(this.rbItemCode);
            this.groupBox5.Controls.Add(this.rbPONumber);
            this.groupBox5.Location = new System.Drawing.Point(444, 20);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(298, 93);
            this.groupBox5.TabIndex = 29;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Search Criteria";
            // 
            // txtItemCode
            // 
            this.txtItemCode.Location = new System.Drawing.Point(125, 53);
            this.txtItemCode.Name = "txtItemCode";
            this.txtItemCode.Size = new System.Drawing.Size(137, 20);
            this.txtItemCode.TabIndex = 5;
            // 
            // txtPONumber
            // 
            this.txtPONumber.Location = new System.Drawing.Point(125, 27);
            this.txtPONumber.Name = "txtPONumber";
            this.txtPONumber.Size = new System.Drawing.Size(137, 20);
            this.txtPONumber.TabIndex = 4;
            // 
            // rbItemCode
            // 
            this.rbItemCode.AutoSize = true;
            this.rbItemCode.Location = new System.Drawing.Point(39, 54);
            this.rbItemCode.Name = "rbItemCode";
            this.rbItemCode.Size = new System.Drawing.Size(70, 17);
            this.rbItemCode.TabIndex = 3;
            this.rbItemCode.TabStop = true;
            this.rbItemCode.Text = "ItemCode";
            this.rbItemCode.UseVisualStyleBackColor = true;
            // 
            // rbPONumber
            // 
            this.rbPONumber.AutoSize = true;
            this.rbPONumber.Location = new System.Drawing.Point(39, 28);
            this.rbPONumber.Name = "rbPONumber";
            this.rbPONumber.Size = new System.Drawing.Size(80, 17);
            this.rbPONumber.TabIndex = 2;
            this.rbPONumber.TabStop = true;
            this.rbPONumber.Text = "PO Number";
            this.rbPONumber.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.dtSearchDate);
            this.groupBox4.Location = new System.Drawing.Point(229, 20);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(210, 93);
            this.groupBox4.TabIndex = 28;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Search Date";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.rbPPAPX);
            this.groupBox3.Controls.Add(this.rbPPFedex);
            this.groupBox3.Controls.Add(this.rbShoes);
            this.groupBox3.Location = new System.Drawing.Point(13, 20);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(210, 93);
            this.groupBox3.TabIndex = 27;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Packing Method";
            // 
            // rbPPAPX
            // 
            this.rbPPAPX.AutoSize = true;
            this.rbPPAPX.Location = new System.Drawing.Point(39, 66);
            this.rbPPAPX.Name = "rbPPAPX";
            this.rbPPAPX.Size = new System.Drawing.Size(90, 17);
            this.rbPPAPX.TabIndex = 2;
            this.rbPPAPX.Text = "PP Box (APX)";
            this.rbPPAPX.UseVisualStyleBackColor = true;
            // 
            // rbPPFedex
            // 
            this.rbPPFedex.AutoSize = true;
            this.rbPPFedex.Checked = true;
            this.rbPPFedex.Location = new System.Drawing.Point(39, 43);
            this.rbPPFedex.Name = "rbPPFedex";
            this.rbPPFedex.Size = new System.Drawing.Size(99, 17);
            this.rbPPFedex.TabIndex = 1;
            this.rbPPFedex.TabStop = true;
            this.rbPPFedex.Text = "PP Box (FedEx)";
            this.rbPPFedex.UseVisualStyleBackColor = true;
            // 
            // rbShoes
            // 
            this.rbShoes.AutoSize = true;
            this.rbShoes.Location = new System.Drawing.Point(39, 20);
            this.rbShoes.Name = "rbShoes";
            this.rbShoes.Size = new System.Drawing.Size(55, 17);
            this.rbShoes.TabIndex = 0;
            this.rbShoes.Text = "Shoes";
            this.rbShoes.UseVisualStyleBackColor = true;
            this.rbShoes.Visible = false;
            // 
            // btnReload
            // 
            this.btnReload.ForeColor = System.Drawing.Color.Black;
            this.btnReload.Location = new System.Drawing.Point(402, 129);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(103, 23);
            this.btnReload.TabIndex = 26;
            this.btnReload.Text = "ReLoad";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dataGridView1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 173);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(750, 412);
            this.groupBox2.TabIndex = 24;
            this.groupBox2.TabStop = false;
            this.groupBox2.Tag = "";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 16);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(744, 393);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridView1_CellBeginEdit);
            this.dataGridView1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView1_CellPainting);
            // 
            // frmFedEx_PrintForPPBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(750, 585);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmFedEx_PrintForPPBox";
            this.Text = "Print Label For PPBox";
            this.Load += new System.EventHandler(this.frmFedEx_PrintForPPBox_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dtSearchDate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtItemCode;
        private System.Windows.Forms.TextBox txtPONumber;
        private System.Windows.Forms.RadioButton rbItemCode;
        private System.Windows.Forms.RadioButton rbPONumber;
        private System.Windows.Forms.RadioButton rbPPAPX;
        private System.Windows.Forms.RadioButton rbPPFedex;
        private System.Windows.Forms.RadioButton rbShoes;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnPrint;

    }
}