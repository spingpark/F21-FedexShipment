using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace F21.Framework
{
    public partial class frmScaleSet : Form
    {
        ScaleDefaultInfo si;

        public frmScaleSet()
        {
            InitializeComponent();
        }

        private void ScaleSet_Load(object sender, EventArgs e)
        {
            cbPortName.Items.Add("");
            cbPortName.Items.Add("COM1");
            cbPortName.Items.Add("COM2");
            cbPortName.Items.Add("COM3");
            cbPortName.Items.Add("COM4");
            cbPortName.Items.Add("COM5");
            cbPortName.Items.Add("COM6");
            cbPortName.Items.Add("COM7");
            cbPortName.Items.Add("COM8");
            cbPortName.Items.Add("COM9");

            cbBaudRate.Items.Add("");
            cbBaudRate.Items.Add("9600");
            cbBaudRate.Items.Add("4800");
            cbBaudRate.Items.Add("2400");
            cbBaudRate.Items.Add("1200");

            cbParity.Items.Add("");
            cbParity.Items.Add("Odd");
            cbParity.Items.Add("Even");
            cbParity.Items.Add("None");

            cbDataBit.Items.Add("");
            cbDataBit.Items.Add("8");
            cbDataBit.Items.Add("7");
            cbDataBit.Items.Add("6");

            cbStopBit.Items.Add("");
            cbStopBit.Items.Add("1");
            cbStopBit.Items.Add("2");

            cbScaleType.Items.Add("");
            cbScaleType.Items.Add("IQ Plus");
            cbScaleType.Items.Add("DIGI");
            cbScaleType.Items.Add("B-TEK");
            cbScaleType.Items.Add("Pennsylvania Scale");
            cbScaleType.Items.Add("Mettler Toledo");

            SetComboBox();
        }

        private void SetComboBox()
        {
            si = ScaleDefaultInfo.GetInstance();

            cbUseScale.Checked = si.UseScale;
            txtCommend.Text = si.Commend;
            cbPortName.Text = si.PortName;
            cbBaudRate.Text = si.BaudRate;
            cbParity.Text = si.Parity;
            cbDataBit.Text = si.DataBit;
            cbStopBit.Text = si.StopBit;
            if(!string.IsNullOrEmpty(si.ScaleType))
                cbScaleType.SelectedIndex = Convert.ToInt32(si.ScaleType);
            cbDefaultWeight.Checked = si.DefaultWeight;
            if (si.BarcodeSize.Equals("0"))
                rbBY3.Checked = true;
            else if (si.BarcodeSize.Equals("1"))
                rbBY4.Checked = true;
            
            if (cbUseScale.Checked)
                cbDefaultWeight.Visible = false;
            else
                cbDefaultWeight.Visible = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("do you want to save?", "save", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            if (!string.IsNullOrEmpty(txtCommend.Text.Trim()))
            {
                si.UseScale = cbUseScale.Checked;
                si.Commend = txtCommend.Text;
                si.PortName = cbPortName.Text;
                si.BaudRate = cbBaudRate.Text;
                si.Parity = cbParity.Text;
                si.DataBit = cbDataBit.Text;
                si.StopBit = cbStopBit.Text;
                si.ScaleType = cbScaleType.Text;
                si.DefaultWeight = cbDefaultWeight.Checked;
                if(rbBY3.Checked)
                    si.BarcodeSize = "0";
                else if (rbBY4.Checked)
                    si.BarcodeSize = "1";

                si.Save();

                MessageBox.Show("Success");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cbUseScale_CheckedChanged(object sender, EventArgs e)
        {
            if (cbUseScale.Checked)
                cbDefaultWeight.Visible = false;
            else
                cbDefaultWeight.Visible = true;
        }
    }
}
