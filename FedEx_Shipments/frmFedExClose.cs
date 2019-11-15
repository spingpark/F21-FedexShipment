using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Reflection;
using F21.Service;

namespace F21.Framework
{
    public partial class frmFedExClose : Form
    {
        public frmFedExClose()
        {
            InitializeComponent();
        }

        private void frmFedExClose_Load(object sender, EventArgs e)
        {
            GetAccountInfo();
        }

        private string GetAccountInfo()
        {
            string retVal = string.Empty;
            DataTable dt = null;

            try
            {
                string AccountCountry = string.Empty;
                //string SQL = "spFedexShippingWebService;2";
                string SQL = "spGetFedExShippingWebServiceLive";
                NameValueCollection nvParams = new NameValueCollection();

                dt = DataLayer.ExecuteSpDataTable(SQL, nvParams, DBCatalog.SCM21);

                if (dt != null && dt.Rows.Count > 0)
                {
                    for(int i=0;i<dt.Rows.Count;i++)
                    {
                        AccountCountry = dt.Rows[i]["Country"].ToString();
                        if(!string.IsNullOrEmpty(AccountCountry))
                            cbFedExAccount.Items.Add(AccountCountry);
                    }

                    cbFedExAccount.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                retVal = string.Format("[GetAccountInfo Error] {0}",ex.Message);
            }

            return retVal;
        }

        private void btnFedExClose_Click(object sender, EventArgs e)
        {
            string retVal = string.Empty;
            string retVal2 = string.Empty;
            bool isSuccess = false;

            retVal = FedExCloseShipment(out isSuccess);

            if (!string.IsNullOrEmpty(retVal))
            {
                MessageBox.Show(retVal, "FedEx_Ground Close Service.");
            }

            if(isSuccess)
            {
                //2016.08.18 - 일단 CloseDate 업뎃 주석처리 (minkyu.r)
                //string pickupDate = dtWorkDate.Value.ToString("yyyy-MM-dd");

                //retVal2 = FedExAPI_StatusUpdate(pickupDate);

                //if (!string.IsNullOrEmpty(retVal2))
                //{
                //    MessageBox.Show(retVal2, "FedEx_Ground Close Service.");
                //}
                //else
                //{
                //    MessageBox.Show("FedEx_Ground Close Success.", "FedEx_Ground");
                //}

                MessageBox.Show("FedEx_Ground Close Success.", "FedEx_Ground");
            }
        }

        private string FedExCloseShipment(out bool isSuccess)
        {
            string retVal = string.Empty;
            string retVal2 = string.Empty;
            string AccountCountry = cbFedExAccount.Text;
            isSuccess = false;

            bool fedExIsLive = false;

            F21.Service.ShipServiceInfo closeServiceInfo = null;
            F21.Service.CloseShipmentResponse closeResponse = null;
            F21.Service.CloseShipmentClient closeService = null;
            
            try
            {
                if (ConfigManager.GetAppSetting2("mode").Equals("production"))
                {
                    fedExIsLive = true;
                }

                decimal packageWeight = 0.00M;
                closeServiceInfo = new F21.Service.ShipServiceInfo(AccountCountry, fedExIsLive, packageWeight, false);

                closeService = new CloseShipmentClient();
                closeResponse = closeService.CloseShipmentService(closeServiceInfo);

                isSuccess = closeResponse.isSuccess;

                if (closeResponse.ErrorMessage != "")
                {
                    retVal = string.Format("[FedExCloseShipment Error] {0}", Basic.IsNull(closeResponse.ErrorMessage, ""));
                }

                // Save FedEx Close Shipment Data
                string retValue2 = SaveFedExClose(closeServiceInfo, closeResponse);

                if (retVal != "")
                {
                    if (retVal2 != "")
                        retVal += "\r\n [Save Error] : " + retVal2;
                }
                else if (retVal2 != "")
                {
                    retVal = "[FedExCloseShipment Save Error] : " + retVal2;
                }
            }
            catch (Exception ex)
            {
                retVal = string.Format("[FedExCloseShipment Error] {0}", ex.Message);
            }

            return retVal;
        }

        private string FedExAPI_StatusUpdate(string pickupDate)
        {
            string strSQL = "";
            string retVal = "";

            NameValueCollection nvParam = new NameValueCollection();
            
            try
            {
                //strSQL = "spFedExAPIClass;7";
                strSQL = "spSetFedExDayEndCloseUpdate";
                nvParam = new NameValueCollection();
                nvParam.Add("@PickupTime", pickupDate);
                nvParam.Add("@CloseType", ShipServiceType.FedEx_Ground.ToString());
                nvParam.Add("@CloseEmpId", EmpInfo.EmpId);

                retVal = DataLayer.ExecuteSpSql(strSQL, nvParam, DBCatalog.SCM21);

                if (retVal != "")
                {
                    retVal = Basic.GetMethodBase(MethodBase.GetCurrentMethod()) + " : " + retVal;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retVal;
        }

        private string SaveFedExClose(ShipServiceInfo clsShipService, CloseShipmentResponse closeResponse)
        {
            string retVal = string.Empty;

            NameValueCollection nvParams = null;
            string RetVal = string.Empty;
            string strSQL = string.Empty;

            try
            {
                //strSQL = "spFedExAPIClass;8";
                strSQL = "spSetFedExDayEndCloseInsert";

                nvParams = new NameValueCollection();
                nvParams.Add("@AccountNumber", Basic.IsNull(clsShipService.AccountNumber));
                nvParams.Add("@IsLive", fnCvtYN(clsShipService.IsLive));
                nvParams.Add("@ServiceType", ShipServiceType.FedEx_Ground.ToString());
                nvParams.Add("@HighestSeverity", closeResponse.HighestSeverity);
                nvParams.Add("@TransactionId", Basic.IsNull(closeResponse.TransactionId));
                nvParams.Add("@HubId", Basic.IsNull(clsShipService.HubId));
                nvParams.Add("@DestinationCountryCode", Basic.IsNull(clsShipService.CountryCode));
                nvParams.Add("@IsSuccess", fnCvtYN(closeResponse.isSuccess));
                nvParams.Add("@NoticeCode", Basic.IsNull(closeResponse.NoticeCode));
                nvParams.Add("@NoticeMessage", Basic.IsNull(closeResponse.NoticeMessage));
                nvParams.Add("@NoticeSeverity", Basic.IsNull(closeResponse.NoticeSeverity));
                nvParams.Add("@NoticeSource", Basic.IsNull(closeResponse.NoticeSource));
                nvParams.Add("@ErrorMessage", Basic.IsNull(closeResponse.ErrorMessage));
                nvParams.Add("@FilePath", Basic.IsNull(closeResponse.DocumentName));
                nvParams.Add("@Version", this.GetVersion());
                nvParams.Add("@IPAddress", Basic.GetIP());
                nvParams.Add("@StationNo", Basic.GetRegistryKey("StationNo"));
                nvParams.Add("@ReqEmpId", Basic.IsNull(EmpInfo.EmpId));

                RetVal = DataLayer.ExecuteSpSql(strSQL, nvParams, DBCatalog.SCM21);

                return RetVal;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retVal;
        }

        #region UTIL

        private string fnCvtYN(bool value)
        {
            string retVal = string.Empty;

            if (value)
                retVal = "Y";
            else
                retVal = "N";

            return retVal;
        }


        private string GetVersion()
        {
            string version = string.Empty;
            try
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                    version = "[" + System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() + "]";
                else
                    version = "[" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "]";
            }
            catch
            {
                // Not ClickOnce Deployment 
                version = "[" + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "]";
            }
            return version;
        }

        #endregion UTIL
    }   

}
