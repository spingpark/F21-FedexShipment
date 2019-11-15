using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace F21.Framework
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ScaleDefaultInfo di = ScaleDefaultInfo.GetInstance();

            //string d = "data source = MIS007; initial catalog = SCMStoreIntranet; persist security info = True; user id = ipuser; password = 1069; MultipleActiveResultSets = True; ";
            //string ddd = Encryption.Encrypt(d, true);

            //Application.Run(new frmFedEx_Shipments());

            if (string.IsNullOrEmpty(di.PortName) || string.IsNullOrEmpty(di.BaudRate)
                || string.IsNullOrEmpty(di.DataBit) || string.IsNullOrEmpty(di.StopBit) || string.IsNullOrEmpty(di.Parity))
            {
                Application.Run(new frmScaleSet());
                Application.Restart();
                //frmScaleSet configForm = new frmScaleSet();
                //configForm.ShowDialog();
                //Application.Restart();
                //return;
            }
            else
            {
                frmFedEx_Shipments frm = new frmFedEx_Shipments();
                string rtn = frm.GetDefaultValues();
                if (!string.IsNullOrWhiteSpace(rtn))
                {
                    MessageBox.Show(rtn);
                    return;
                }

                Application.Run(frm);
                //Application.Run(new frmFedEx_Shipments());
            }

            
        }
    }
}
