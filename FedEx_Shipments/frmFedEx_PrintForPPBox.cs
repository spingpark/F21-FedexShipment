using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;
using F21.Service;
using System.Reflection;
using System.Text.RegularExpressions;

namespace F21.Framework
{
    public partial class frmFedEx_PrintForPPBox : Form
    {
        string PrinterName = string.Empty;
        string AccountCountry = string.Empty;
        NameValueCollection nvZPLReplace = null;
        NameValueCollection nvOnTracZPLReplace = null;
        string UserId = "FedExPPBox";

        public frmFedEx_PrintForPPBox()
        {
            InitializeComponent();
        }

        private void frmFedEx_PrintForPPBox_Load(object sender, EventArgs e)
        {
           
            rbPONumber.Checked = true;
            dtSearchDate.Value = DateTime.Now;
            
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void Search()
        {
            string warehouse = string.Empty;
            string packingType = string.Empty;
            string searchDate = string.Empty;
            string searchType = string.Empty;
            string orderNumber = string.Empty;
            string userId = string.Empty;

            if (rbShoes.Checked)
                packingType = PackingMethod.Shoes;
            else if (rbPPFedex.Checked)
                packingType = PackingMethod.PPBoxFedEx;
            else if (rbPPAPX.Checked)
                packingType = PackingMethod.PPBoxAPX;
            else
                packingType = string.Empty;

            if (rbPONumber.Checked)
            {
                searchType = SearchType.PO;
                orderNumber = txtPONumber.Text.Trim();
            }
            else if (rbItemCode.Checked)
            {
                searchType = SearchType.ItemCode;
                orderNumber = txtItemCode.Text.Trim();
            }
            else
            {
                searchType = string.Empty;
                orderNumber = string.Empty;
            }

            if(string.IsNullOrEmpty(packingType))
            {
                MessageBox.Show("Please select Packing Method.");
                return;
            }

            if (string.IsNullOrEmpty(searchType))
            {
                MessageBox.Show("Please select Search Criteria.");
                return;
            }

            if (!packingType.Equals("0") && string.IsNullOrEmpty(orderNumber))
            {
                MessageBox.Show("Please input PO Number or Itemcode.");
                return;

            }

            warehouse = sPrintInfo.Warehouse;
            searchDate = dtSearchDate.Value.ToShortDateString();

            string retVal = string.Empty;
            DataTable dt = null;

            try
            {
                string AccountCountry = string.Empty;
                //string SQL = "spFedExShipments;6";
                string SQL = "spGetFedExPPBoxSearch";
                NameValueCollection nvParams = new NameValueCollection();
                nvParams.Add("@DataOwnerID", warehouse); 
                nvParams.Add("@PackingType", packingType); //0 : Shoes, 1 : PPBox(FedEx), 2 : PPBox(APX)
                nvParams.Add("@SearchDate", searchDate);   //Search Date
                nvParams.Add("@SearchType", searchType);   //0 : PO, 1 : ItemCode
                nvParams.Add("@OrderNumber", orderNumber); //OderNumber / ItemCode
                nvParams.Add("@UserID", UserId);

                dt = DataLayer.ExecuteSpDataTable(SQL, nvParams, DBCatalog.SCM21);

                if (dt != null && dt.Rows.Count > 0)
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.Columns.Clear();
                    dataGridView1.Rows.Clear();

                    addCheckBox();
                    dataGridView1.DataSource = dt;
                    dataGridView1.CurrentCell = this.dataGridView1[1, 0];

                    addStatusColumn();
                }
                else
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.Columns.Clear();
                    dataGridView1.Rows.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("[Search Error] {0}", ex.Message));
                return;
            }
        }

        private void addCheckBox()
        {   
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Check";
            checkBoxColumn.Name = "check";
            checkBoxColumn.TrueValue = "true";

            dataGridView1.Columns.Add(checkBoxColumn);
            dataGridView1.Columns[0].Width = 50;
        }

        private void addStatusColumn()
        {
            dataGridView1.Columns.Add("Status", "Status");
            dataGridView1.Columns[7].Width = 80;
        }

        //Check Box외에는 수정 불가
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex != 0) e.Cancel = true;
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex == -1)
            {
                e.PaintBackground(e.ClipBounds, false);

                Point pt = e.CellBounds.Location;  // where you want the bitmap in the cell

                int nChkBoxWidth = 15;
                int nChkBoxHeight = 15;
                int offsetx = (e.CellBounds.Width - nChkBoxWidth) / 2;
                int offsety = (e.CellBounds.Height - nChkBoxHeight) / 2;

                pt.X += offsetx;
                pt.Y += offsety;

                CheckBox cb = new CheckBox();
                cb.Size = new Size(nChkBoxWidth, nChkBoxHeight);
                cb.Location = pt;
                cb.CheckedChanged += new EventHandler(gvSheetListCheckBox_CheckedChanged);

                ((DataGridView)sender).Controls.Add(cb);

                e.Handled = true;
            }

        }

        private void gvSheetListCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (((CheckBox)sender).Checked)
                    row.Cells[0].Value = "true";
                else
                    row.Cells[0].Value = "false";

                //row.Cells[0].Value = ((CheckBox)sender).Checked;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            string store = string.Empty;
            string cartonId = string.Empty;
            string weight = string.Empty;
            string status = string.Empty;
            int cntFail = 0;
            int cntSuccess = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell chkchecking = row.Cells[0] as DataGridViewCheckBoxCell;
                if (chkchecking.Value != null && chkchecking.Value.ToString() == chkchecking.TrueValue.ToString())
                {
                    store = row.Cells["store"].Value.ToString();
                    cartonId = row.Cells["cartonid"].Value.ToString();
                    weight = row.Cells["weight"].Value.ToString();

                    status = PrintShippingLabelPre(cartonId, weight);

                    row.Cells["status"].Value = status;

                    if (status.Equals(Status.Error))
                        cntFail++;
                    else
                        cntSuccess++;
                }
            }

            if (cntFail > 0)
                MessageBox.Show(string.Format("[Fail : {0} / Success : {1}] Please check status.", cntFail, cntSuccess));
            else
                MessageBox.Show(string.Format("Print Success [{0}]", cntSuccess));
        }
        
        #region FedEx Label Print
        private string PrintShippingLabelPre(string CartonId, string Weight)
        {
            string result = string.Empty;
            string retVal = string.Empty;
            string retZPL = string.Empty;
            string retShippingCompany = string.Empty;
            bool isReprint = false;

            string cartonId = CartonId;
            string weight = Weight;
            weight = Math.Round(Convert.ToDecimal(weight)).ToString();

            string userId = UserId;

            // 8/19/16, SeungJong Doh, Change Carton for MCID - temporary until SCM21
            // 09/13/2016, Yangsub Lee, Carton CTC인지 체크(12자리 포함)
            if (cartonId.Trim().Length == 19 || cartonId.Trim().Length == 12)
            {
                string myCartonID = this.GetNewCartonID(cartonId);
                if (myCartonID.Trim().Length == 12) cartonId = myCartonID.Trim();
                //NameValueCollection nv
                // 09/13/2016, Yangsub Lee, MCID Scan시 CartonID가 없는 경우 지행 불가.
                if (myCartonID.Trim().Length == 0)
                {
                    //MessageBox.Show("The MCID Code is not Post. Please check");
                    //txtCartonId.Focus();
                    retVal = Status.Error;
                    return retVal;
                }
            }

            //## 01. Check CartonId
            result = getCartonIdInfo(cartonId, weight, userId, out retZPL, out retShippingCompany, ConfigManager.GetAppSetting2("BarcodeKind"));

            if ((result.Equals(ResultStatus.NoFedex) || result.Equals(ResultStatus.Reprint)) && (!string.IsNullOrEmpty(retZPL)))
            {
                if (result.Equals(ResultStatus.Reprint))
                    isReprint = true;


                NameValueCollection nvZPLReplacer = nvZPLReplace;
                if (retShippingCompany != null && retShippingCompany.Equals("OnTrac"))
                {
                    nvZPLReplacer = nvOnTracZPLReplace;
                }
                //2016.09.12 - ZPL Replace 
                if ((result.Equals(ResultStatus.Reprint)) && (nvZPLReplace.Count > 0))
                {
                    foreach (string value in nvZPLReplace)
                    {
                        retZPL = retZPL.Replace(value, nvZPLReplace[value]);
                    }
                    retZPL = retZPL.Replace("@CartonID", cartonId);
                }

                //## 02. PrintLabel

                if (retShippingCompany.Equals("OnTrac") || retShippingCompany.Equals("Fedex"))
                {
                    retZPL = retZPL.Replace("@BarcodeKind", ConfigManager.GetAppSetting2("BarcodeKind").Equals("Y") ? "^BC,80,Y,N,N,A" : "^B3,N,80,Y,N");

                    if (ConfigManager.GetAppSetting2("BarcodeKind").Equals("Y"))
                    {
                        if (cartonId.Length == 12)
                            retZPL = retZPL.Replace("@BarcodeSize", "4");
                        else
                            retZPL = retZPL.Replace("@BarcodeSize", "3");
                    }
                    else
                    {
                        retZPL = retZPL.Replace("@BarcodeSize", "3");
                    }

                }



                retVal = PrintShippingLabel(retZPL, cartonId, weight, "", isReprint, retShippingCompany);
            }
            else if (retShippingCompany.Equals("OnTrac"))
            {
                retVal = OnTrackService(cartonId, weight, userId);
            }
            else
            {
                //## 02. FedEx WebService - PrintLabel
                retVal = FedExService(cartonId, weight, userId);
            }

            if (string.IsNullOrEmpty(retVal))
            {
                retVal = Status.Success;
                return retVal;
            }
            else
            {
                retVal = Status.Error;
                return retVal;
            }
        }

        #region OnTrackService
        private OnTrac.Shipper GetShipperInformation(Boolean isLive, ref String url)
        {
            OnTrac.Shipper shipper = new OnTrac.Shipper();
            try
            {
                //String spName = "spOnTracShippingWebService";
                String spName = "spGetOnTracShippingWebService";
                NameValueCollection nvParam = new NameValueCollection();
                nvParam.Add("@Country", AccountCountry);
                nvParam.Add("@isLive", isLive ? "Y" : "N");

                DataTable dtResult = DataLayer.ExecuteSpDataTable(spName, nvParam, DBCatalog.SCM21);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    shipper.Name = dtResult.Rows[0]["CompanyName"].ToString();
                    shipper.Addr1 = dtResult.Rows[0]["Line1"].ToString();
                    shipper.City = dtResult.Rows[0]["City"].ToString();
                    shipper.State = dtResult.Rows[0]["StateCode"].ToString();
                    shipper.Zip = dtResult.Rows[0]["PostalCode"].ToString();
                    shipper.Phone = dtResult.Rows[0]["PhoneNumber"].ToString();
                    shipper.Contact = dtResult.Rows[0]["CompanyName"].ToString();
                    url = url.Replace("{account}", dtResult.Rows[0]["AccountNumber"].ToString());
                    url = url.Replace("{password}", dtResult.Rows[0]["Password"].ToString());
                }
            }
            catch (Exception ex)
            {
            }
            return shipper;
        }

        private OnTrac.Consignee GetConsigneeInformation(OnTrac.Shipment shipment, String cartonId, String weight, String userId, Boolean isLive)
        {
            OnTrac.Consignee consignee = new OnTrac.Consignee();
            try
            {
                F21.Service.AddressInfo clsAddress = new F21.Service.AddressInfo(cartonId, weight, userId, isLive);
                consignee.Name = clsAddress.ReceiveName;
                consignee.Addr1 = clsAddress.Line1;
                consignee.Addr2 = clsAddress.Line2;
                consignee.Addr3 = "";
                consignee.City = clsAddress.City;
                consignee.State = clsAddress.State;
                consignee.Zip = clsAddress.PostalCode;
                consignee.Phone = clsAddress.PhoneNumber;
                consignee.Contact = "STORE MNGR";
                shipment.ShipEmail = clsAddress.Email;
            }
            catch (Exception ex)
            {
            }
            return consignee;
        }

        private void setShipmentDefailInformation(OnTrac.Shipment shipment, String cartonId, String weight, String userId)
        {
            try
            {
                shipment.UID = cartonId;
                shipment.Service = "C";
                shipment.SignatureRequired = false;
                shipment.Residential = false;
                shipment.SaturdayDel = false;
                shipment.Instructions = "";
                shipment.CODType = "NONE";
                shipment.Weight = weight;

                //String sp = "spOnTracShippingWebService;2";
                String sp = "spGetOnTracShippingDetail";
                NameValueCollection nvParam = new NameValueCollection();
                nvParam.Add("@CartonID", cartonId);

                DataTable dt = DataLayer.ExecuteSpDataTable(sp, nvParam, DBCatalog.SCM21);
                if (dt != null && dt.Rows.Count > 0)
                {
                    shipment.Service = OnTrac.OnTracUtility.getOnTracServiceCodeFromService(dt.Rows[0]["Service"].ToString());
                    shipment.SignatureRequired = (dt.Rows[0]["SignatureRequired"].Equals("Y")) ? true : false;
                    shipment.Residential = (dt.Rows[0]["Residential"].ToString().Equals("Y")) ? true : false;
                    shipment.SaturdayDel = (dt.Rows[0]["SaturdayDel"].ToString().Equals("Y")) ? true : false;
                    shipment.Instructions = dt.Rows[0]["Instruction"].ToString();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private OnTrac.Shipment createShipment(String cartonId, String weight, String userId, ref String url, Boolean isLive)
        {
            OnTrac.Shipment shipment = new OnTrac.Shipment();
            try
            {
                OnTrac.Shipper shipper = GetShipperInformation(isLive, ref url);
                OnTrac.Consignee consignee = GetConsigneeInformation(shipment, cartonId, weight, userId, isLive);
                shipment.shipper = shipper;
                shipment.consignee = consignee;
                setShipmentDefailInformation(shipment, cartonId, weight, userId);
                shipment.LabelType = 9;
                shipment.ShipDate = DateTime.Now.ToString("yyyy-MM-dd");
                shipment.DIM = new OnTrac.DIM { Width = 0, Height = 0 };
                shipment.Reference = cartonId;
            }
            catch (Exception ex)
            {
            }
            return shipment;
        }

        private String OnTrackService(String cartonId, String weight, String userId)
        {
            String retVal = String.Empty;
            Result result = null;
            bool onTracLive = false;
            if (ConfigManager.GetAppSetting2("mode").Equals("production"))
            {
                onTracLive = true;
            }
            result = new Result();
            result.RetVal = "";
            String url = String.Empty;
            if (onTracLive)
            {
                url = ConfigManager.GetAppSetting2("OnTrac_Live");
            }
            else
            {
                url = ConfigManager.GetAppSetting2("OnTrac_Test");
            }

            OnTrac.OnTracShipmentRequest request = new OnTrac.OnTracShipmentRequest();
            OnTrac.Shipment shipment = createShipment(cartonId, weight, userId, ref url, onTracLive);
            OnTrac.Shipments shipments = new OnTrac.Shipments();
            shipments.addShipments(shipment);
            request.Shipments = shipments;


            result = OnTrac_Request(request, cartonId, url, onTracLive);

            return retVal;
        }

        private String SaveOnTracResponse(String CartonId, OnTrac.Shipment shipment, OnTrac.ResponseShipment response, Boolean isLive)
        {
            String retVal = String.Empty;
            try
            {
                //String sp = "spOnTracAPIClass";
                String sp = "spSetOnTracDayEndInsert";
                NameValueCollection nvParam = new NameValueCollection();
                nvParam.Add("@OrderNumber", CartonId);
                nvParam.Add("@IsLive", (isLive) ? "Y" : "N");
                nvParam.Add("@OnTracTrackingNumber", response.Tracking);
                nvParam.Add("@ShippingMethodName", "");
                nvParam.Add("@ServiceType", OnTrac.OnTracUtility.getOnTracServiceFromServiceCode(shipment.Service));
                nvParam.Add("@PackageWeight", shipment.Weight);
                nvParam.Add("@ImageType", "ZPLII");
                nvParam.Add("@ReceiveName", Basic.IsNull(shipment.consignee.Name, ""));
                nvParam.Add("@ReceiveOrganization", "");
                nvParam.Add("@ReceiveLine1", Basic.IsNull(shipment.consignee.Addr1, ""));
                nvParam.Add("@ReceiveLine2", Basic.IsNull(shipment.consignee.Addr2, ""));
                nvParam.Add("@ReceiveCity", Basic.IsNull(shipment.consignee.City, ""));
                nvParam.Add("@ReceiveState", Basic.IsNull(shipment.consignee.State, ""));
                nvParam.Add("@ReceiveCountryCode", "US");
                nvParam.Add("@ReceiveCountryName", "USA");
                nvParam.Add("@ReceivePostalCode", Basic.IsNull(shipment.consignee.Zip, ""));
                nvParam.Add("@ReceiveTel", Basic.IsNull(shipment.consignee.Phone, ""));
                nvParam.Add("@LabelString", response.Label);
                nvParam.Add("@TransitDays", response.TransitDays);
                nvParam.Add("@ExpectedDeliveryDate", String.Format("{0}-{1}-{2}", response.ExpectedDeliveryDate.Substring(0, 4), response.ExpectedDeliveryDate.Substring(4, 2), response.ExpectedDeliveryDate.Substring(6, 2)));
                nvParam.Add("@CommitTime", response.CommitTime);
                nvParam.Add("@ServiceChrg", Basic.IsNull(response.ServiceChrg, "0"));
                nvParam.Add("@BaseCharge", Basic.IsNull(response.BaseCharge, "0"));
                nvParam.Add("@CODCharge", Basic.IsNull(response.CODCharge, "0"));
                nvParam.Add("@DeclaredCharge", Basic.IsNull(response.DeclaredCharge, "0"));
                nvParam.Add("@AdditionalCharges", Basic.IsNull(response.AdditionalCharges, "0"));
                nvParam.Add("@SaturdayCharge", Basic.IsNull(response.SaturdayCharge, "0"));
                nvParam.Add("@FuelChrg", Basic.IsNull(response.FuelChrg, "0"));
                nvParam.Add("@TotalChrg", Basic.IsNull(response.TotalChrg, "0"));
                nvParam.Add("@TariffChrg", Basic.IsNull(response.TarrifChrg, "0"));
                nvParam.Add("@SortCode", response.SortCode);
                nvParam.Add("@RateZone", response.RateZone);
                nvParam.Add("@SignatureRequired", (shipment.SignatureRequired) ? "Y" : "N");
                nvParam.Add("@SaturdayRequired", (shipment.SaturdayDel) ? "Y" : "N");
                nvParam.Add("@Residential", (shipment.Residential) ? "Y" : "N");
                nvParam.Add("@Instruction", shipment.Instructions);
                nvParam.Add("@RegEmpId", "OnTracPPBox");
                nvParam.Add("@IsSuccess", (response.Error.Equals("")) ? "Y" : "N");
                nvParam.Add("@Reference1", shipment.Reference);

                retVal = DataLayer.ExecuteSpSql(sp, nvParam, DBCatalog.SCM21);
            }
            catch (Exception ex)
            {
                retVal = ex.Message;
            }
            return retVal;
        }

        private Result OnTrac_Request(OnTrac.OnTracShipmentRequest request, String orderNumber, String url, Boolean isLive)
        {
            Result result = new Result();
            try
            {
                OnTrac.OnTracWebRequest webRequest = new OnTrac.OnTracWebRequest();
                OnTrac.OnTracResponse response = webRequest.RequestShipments(request, orderNumber, url);

                if (!response._error.Equals(""))
                {
                    result.RetVal = response._error;
                    return result;
                }

                String retVal = SaveOnTracResponse(orderNumber, request.Shipments.ShipmentsList[0], response._shipments[0], isLive);
                if (!retVal.Equals(""))
                {
                    result.RetVal = retVal;
                    return result;
                }

                result.Label = new LabelInfo()
                {
                    LabelString = response._shipments[0].Label,
                    OrderNumber = orderNumber
                };
                result.TrackingNumber = response._shipments[0].Tracking;

                if (nvOnTracZPLReplace != null && nvOnTracZPLReplace.Count > 0)
                {
                    foreach (String key in nvOnTracZPLReplace)
                    {
                        response._shipments[0].Label = response._shipments[0].Label.Replace(key, nvOnTracZPLReplace[key]);
                    }
                    response._shipments[0].Label = response._shipments[0].Label.Replace("@CaronID", orderNumber);

                    if (ConfigManager.GetAppSetting2("BarcodeKind").Equals("Y"))
                    {
                        if (orderNumber.Length == 12)
                            response._shipments[0].Label = response._shipments[0].Label.Replace("@BarcodeSize", "4");
                        else
                            response._shipments[0].Label = response._shipments[0].Label.Replace("@BarcodeSize", "3");
                    }
                    else
                    {
                        response._shipments[0].Label = response._shipments[0].Label.Replace("@BarcodeSize", "3");
                    }


                    response._shipments[0].Label = response._shipments[0].Label.Replace("@BarcodeKind", ConfigManager.GetAppSetting2("BarcodeKind").Equals("Y") ? "^BC,80,Y,N,N,A" : "^B3,N,80,Y,N");

                }

                retVal = PrintShippingLabel(response._shipments[0].Label, orderNumber, request.Shipments.ShipmentsList[0].Weight, response._shipments[0].Tracking, false, "OnTrac");
            }
            catch (Exception ex)
            {
                result.RetVal = ex.Message;
            }
            return result;
        }


        #endregion

        private string PrintShippingLabel(string zplString, string cartonId, string weight, string fedExTrackingNumber, bool isReprint, String Service)
        {
            string retVal = string.Empty;

            try
            {
                String folder = "";
                if (Service.Equals("Fedex"))
                {
                    folder = "C:\\F21\\FedExAPI\\" + DateTime.Now.ToString("yyyyMM");
                }
                else if (Service.Equals("OnTrac"))
                {
                    folder = "C:\\F21\\OnTracAPI\\" + DateTime.Now.ToString("yyyyMM");
                }
                else
                {
                    folder = "C:\\F21\\Else\\" + DateTime.Now.ToString("yyyyMM");
                }

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string filePath = folder + "\\" + cartonId + ".txt";

                if (File.Exists(filePath))
                    File.Delete(filePath);

                TextManager.WriteLine(zplString, filePath, Encoding.ASCII);

                System.Threading.Thread.Sleep(500);

                File.Copy(filePath, PrinterName, true);

                if (Service.Equals("Fedex") || Service.Equals("OnTrac"))
                {
                    SavePrintPath(filePath, cartonId, fedExTrackingNumber, isReprint, Service);
                }

                SavePrintInfo(cartonId, weight);
                
            }
            catch (Exception ex)
            {
                retVal = "[FedEx Label Print Issue] " + ex.Message;
            }

            return retVal;
        }
        #endregion End : FedEx Label Print
        
        #region Fedex Service
        private string FedExService(string cartonId, string strWeight, string userId)
        {
            string retVal = string.Empty;
            Result result = null;
            bool fedExIsLive = false;

            if (ConfigManager.GetAppSetting2("mode").Equals("production"))
            {
                fedExIsLive = true;
            }

            result = new Result();
            result.RetVal = "";

            decimal weight = 0;
            weight = Convert.ToDecimal(strWeight);

            F21.Service.ShipServiceInfo clsShipService = null;
            F21.Service.AddressInfo clsAddress = null;

            clsShipService = new F21.Service.ShipServiceInfo(AccountCountry, fedExIsLive, weight, false);
            clsAddress = new F21.Service.AddressInfo(cartonId, strWeight, userId, fedExIsLive);

            if (clsAddress != null)
            {
                if (!string.IsNullOrEmpty(clsAddress.CartonId))
                {
                    clsShipService.OrderNumber = cartonId;
                    clsShipService.IsCOD = false;
                    clsShipService.CODAmount = 0.00M;
                    clsShipService.ShipServiceType = ShipServiceType.FedEx_Ground;

                    result = Fedex_Request(cartonId, clsShipService, clsAddress);

                    if (result.RetVal != "")
                    {
                        MessageBox.Show(result.RetVal);
                        retVal = string.Format("[{0}] {1}", cartonId, result.RetVal);
                    }
                }
                else
                {
                    MessageBox.Show("[ERROR] Invalid Order Address.");
                }
            }

            return retVal;
        }

        public Result Fedex_Request(string orderNumber, F21.Service.ShipServiceInfo clsShipService, F21.Service.AddressInfo clsAddress)
        {
            string strResponse = string.Empty;
            Result fedexResult = null;
            LabelInfo fedexLabel = null;
            F21.Service.ShipClient shipClient = null;
            F21.Service.ShipServiceResponse shipResponse = new F21.Service.ShipServiceResponse();

            try
            {
                fedexResult = new Result();
                fedexLabel = new LabelInfo();

                shipClient = new F21.Service.ShipClient();
                shipResponse = shipClient.ShipClientService(clsShipService, clsAddress);

                // Save Error Log Table
                if (!shipResponse.isSuccess)    // false
                {
                    //retVal = shipResponse.ErrorMessage;
                    SetServiceErrorLog(orderNumber, clsShipService, shipResponse, shipResponse.ErrorMessage);
                }

                // Check FedexTrackingNumber (If Smart_Post service type)
                if (!string.IsNullOrEmpty(shipResponse.FedexTrackingNumber) && !string.IsNullOrEmpty(shipResponse.FedexLabelString))
                {
                    // Save FedEx label details
                    strResponse = SetSave(orderNumber, clsShipService, clsAddress, shipResponse);

                    if (strResponse != "")
                    {
                        throw new Exception(strResponse);
                    }

                    fedexLabel.LabelString = shipResponse.FedexLabelString;  // ZPL label string
                    fedexLabel.OrderNumber = clsShipService.OrderNumber; // Order Number  
                    fedexResult.Label = fedexLabel;
                    fedexResult.TrackingNumber = shipResponse.FedexTrackingNumber;

                    //2016.08.30 - ZPL Replace 
                    if (nvZPLReplace.Count > 0)
                    {
                        foreach (string value in nvZPLReplace)
                        {
                            shipResponse.FedexLabelString = shipResponse.FedexLabelString.Replace(value, nvZPLReplace[value]);
                        }
                        shipResponse.FedexLabelString = shipResponse.FedexLabelString.Replace("@CartonID", orderNumber);

                        if (ConfigManager.GetAppSetting2("BarcodeKind").Equals("Y"))
                        {
                            if (orderNumber.Length == 12)
                                shipResponse.FedexLabelString = shipResponse.FedexLabelString.Replace("@BarcodeSize", "4");
                            else
                                shipResponse.FedexLabelString = shipResponse.FedexLabelString.Replace("@BarcodeSize", "3");
                        }
                        else
                        {
                            shipResponse.FedexLabelString = shipResponse.FedexLabelString.Replace("@BarcodeSize", "3");
                        }


                        shipResponse.FedexLabelString = shipResponse.FedexLabelString.Replace("@BarcodeKind", ConfigManager.GetAppSetting2("BarcodeKind").Equals("Y") ? "^BC,80,Y,N,N,A" : "^B3,N,80,Y,N");

                    }

                    string returnValue = PrintShippingLabel(shipResponse.FedexLabelString, orderNumber, clsShipService.PackageWeight.ToString(), shipResponse.FedexTrackingNumber, false, "Fedex");

                    if (returnValue != "")
                    {
                        // Save print issue.
                        //CreateErrorLog(order.OrderNumber, PrintLabelLogType.LabelError, "[FedEx Label Printing Issue] " + returnValue);
                        fedexResult.RetVal = "[FedEx Label Printing Issue] " + returnValue;
                    }
                    else
                    {
                        fedexResult.RetVal = "";
                    }
                }
                else if (!string.IsNullOrEmpty(shipResponse.FedexTrackingNumber) && string.IsNullOrEmpty(shipResponse.FedexLabelString))   // Only Exists TrackingNumber
                {
                    SetServiceErrorLog(orderNumber, clsShipService, shipResponse, shipResponse.ErrorMessage);

                    if (strResponse != "")
                    {
                        throw new Exception(strResponse);
                    }

                    fedexLabel.LabelString = "";                                // ZPL label string
                    fedexLabel.OrderNumber = clsShipService.OrderNumber;        // Order Number
                    fedexResult.Label = fedexLabel;

                    fedexResult.TrackingNumber = shipResponse.FedexTrackingNumber;
                    fedexResult.RetVal = "FedEx label zpl string and tracking number are null value.";
                }
                else
                {
                    fedexLabel.LabelString = Basic.IsNull(shipResponse.FedexLabelString, "");
                    fedexLabel.OrderNumber = clsShipService.OrderNumber;
                    fedexResult.Label = fedexLabel;
                    fedexResult.TrackingNumber = Basic.IsNull(shipResponse.FedexTrackingNumber, "");
                    fedexResult.RetVal = "[ERROR : Thers is no FedEx response data.]\r\n" + shipResponse.ErrorMessage; // result
                }

                return fedexResult;
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                SetServiceErrorLog(orderNumber, clsShipService, shipResponse, ex.Detail.LastChild.InnerText);

                throw ex;
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                SetServiceErrorLog(orderNumber, clsShipService, shipResponse, ex.Message);

                throw ex;
            }
            catch (Exception ex)
            {
                SetServiceErrorLog(orderNumber, clsShipService, shipResponse, ex.Message);

                throw ex;
            }
            finally
            {
                if (shipClient != null) shipClient = null;
                if (shipResponse != null) shipResponse = null;
            }
        }

        #endregion Fedex Service
        
        #region DB
        /// <summary>
        /// Get new carton ID for 19 digit MC ID
        /// </summary>
        /// <param name="cartonId">MC ID</param>
        /// <returns>12 digit carton ID</returns>
        private string GetNewCartonID(string cartonId)
        {
            string retval = cartonId;

            NameValueCollection nv = new NameValueCollection();
            nv.Add("@DataOwnerID", "0000");
            nv.Add("@CartonID", cartonId);
            nv.Add("@NewCartonID", "");

            //retval = DataLayer.ExecuteSpSql("spKR_UPSIntegration;130", nv, DBCatalog.SCM21);
            retval = DataLayer.ExecuteSpSql("spGetFedExScanCarton", nv, DBCatalog.SCM21);

            return retval;
        }

        private string getDefaultWeight(string cartonId)
        {
            string retval = cartonId;

            NameValueCollection nv = new NameValueCollection();
            nv.Add("@CartonID", cartonId);

            retval = DataLayer.ExecuteSpDataTable("spFedExShipments;2", nv, DBCatalog.SCM21).Rows[0][0].ToString();

            return retval;
        }

        public string getCartonIdInfo(string cartonId, string weight, string userId, out string strZPL, out string ShippingCompany, string barcodeKind)
        {
            string retVal = string.Empty;

            string resultZPL = string.Empty;
            string resultStatus = string.Empty;
            strZPL = string.Empty;
            ShippingCompany = string.Empty;

            //2016.08.26 - Add DefaultWeight/BarCodeSize (minkyu.r)
            string BarcodeSize = string.Empty;
            try
            {
                ScaleDefaultInfo di = ScaleDefaultInfo.GetInstance();
                BarcodeSize = di.BarcodeSize.ToString();
            }
            catch { }

            if (string.IsNullOrEmpty(BarcodeSize))
                BarcodeSize = "0";

            DataTable dt = null;
            string strSQL = string.Empty;
            NameValueCollection nvParams = null;

            try
            {
                //strSQL = "spFedExShipments";
                strSQL = "spGetFedExCartonInfo";
                nvParams = new NameValueCollection();
                nvParams.Add("@CartonID", cartonId);
                nvParams.Add("@Weight", weight);
                nvParams.Add("@UserID", userId);
                nvParams.Add("@Type", FedExShipmentsType.FedEx);
                nvParams.Add("@BarcodeSizeType", BarcodeSize);
                nvParams.Add("@BarcodeKind", barcodeKind);

                dt = DataLayer.ExecuteSpDataTable(strSQL, nvParams, DBCatalog.SCM21);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        resultZPL = dt.Rows[0]["ResultZPL"].ToString();
                        resultStatus = dt.Rows[0]["ResultStatus"].ToString();
                        ShippingCompany = dt.Rows[0]["ShippingCompany"].ToString();
                        if (resultStatus.Equals(ResultStatus.NoFedex) || resultStatus.Equals(ResultStatus.Reprint))
                        {
                            strZPL = resultZPL;
                            retVal = resultStatus;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dt != null) dt.Dispose();
                if (nvParams != null) nvParams.Clear();
            }

            return retVal;
        }

        /// <summary>
        /// Set FedEx shippig label information.
        /// </summary>
        /// <param name="clsOrder">Order Infomation</param>
        /// <param name="clsShipService">ShipService Information</param>
        /// <param name="clsAddress">Address Information</param>
        /// <returns></returns>
        private string SetSave(string orderNumber, ShipServiceInfo clsShipService, F21.Service.AddressInfo clsAddress, ShipServiceResponse shipResp)
        {
            NameValueCollection nvParams = null;
            string RetVal = string.Empty;
            string strSQL = string.Empty;

            try
            {
                //strSQL = "spFedExAPIClass;1";
                strSQL = "spSetFedExDayEndInsert";

                nvParams = new NameValueCollection();
                nvParams.Add("@OrderNumber", orderNumber);
                nvParams.Add("@IsLive", fnCvtYN(clsShipService.IsLive));
                nvParams.Add("@HighestSeverity", shipResp.HighestSeverity);
                nvParams.Add("@FedexTrackingNumber", shipResp.FedexTrackingNumber.Trim());
                nvParams.Add("@ShippingMethodName", "");
                nvParams.Add("@ServiceType", clsShipService.ServiceType.ToString());
                nvParams.Add("@PackageWeight", clsShipService.PackageWeight.ToString());
                //nvParams.Add("@CountryCode", clsShipService.CountryCode);
                //nvParams.Add("@LanguageCode", clsShipService.LanguageCode);
                //nvParams.Add("@LocaleCode", clsShipService.LocaleCode);
                //nvParams.Add("@Currency", clsShipService.Currency);
                //nvParams.Add("@WeightUnit", clsShipService.WeightUnit.ToString());
                //nvParams.Add("@DimensionsUnit", clsShipService.DimensionsUnit.ToString());
                nvParams.Add("@ImageType", clsShipService.LabelImageType.ToString());
                //nvParams.Add("@FormatType", clsShipService.LabelFormatType.ToString());
                //nvParams.Add("@StockType", clsShipService.LabelStockType.ToString());
                //nvParams.Add("@PrintingOrientation", clsShipService.LabelPrintingOrientation.ToString());
                //nvParams.Add("@ReceiveOrderAddressID", clsAddress.OrderAddressID);
                nvParams.Add("@ReceiveName", clsAddress.ReceiveName);
                nvParams.Add("@ReceiveOrganization", clsAddress.Organization);
                nvParams.Add("@ReceiveLine1", clsAddress.Line1);
                nvParams.Add("@ReceiveLine2", clsAddress.Line2);
                nvParams.Add("@ReceiveCity", clsAddress.City);
                nvParams.Add("@ReceiveState", clsAddress.State);
                nvParams.Add("@ReceiveCountryCode", clsAddress.CountryCode);
                nvParams.Add("@ReceiveCountryName", clsAddress.CountryName);
                nvParams.Add("@ReceivePostalCode", clsAddress.PostalCode);
                nvParams.Add("@ReceiveTel", clsAddress.Tel);
                //nvParams.Add("@ReceiveMOBILE", clsAddress.Mobile);
                nvParams.Add("@FormId", Basic.IsNull(shipResp.FormId, ""));
                nvParams.Add("@Barcode", Basic.IsNull(shipResp.Barcode, ""));
                nvParams.Add("@BarcodeType", Basic.IsNull(shipResp.BarcodeType, ""));
                //nvParams.Add("@BinaryBarcodeType", Basic.IsNull(shipResp.BinaryBarcodeType, ""));
                nvParams.Add("@UrsaPrefixCode", Basic.IsNull(shipResp.UrsaPrefixCode, ""));
                nvParams.Add("@UrsaSuffixCode", Basic.IsNull(shipResp.UrsaSuffixCode, ""));
                nvParams.Add("@DestinationLocationId", Basic.IsNull(shipResp.DestinationLocationId, ""));
                nvParams.Add("@AirportId", Basic.IsNull(shipResp.AirportId, ""));
                nvParams.Add("@TransitTime", Basic.IsNull(shipResp.TransitTime));
                nvParams.Add("@PickupCode", Basic.IsNull(shipResp.SmartPickupCode));
                nvParams.Add("@Machinable", fnCvtYN(shipResp.SmartMachinable));
                nvParams.Add("@UspsApplicationId", Basic.IsNull(shipResp.UspsApplicationId));

                nvParams.Add("@IsSuccess", fnCvtYN(shipResp.isSuccess));

                nvParams.Add("@NoticeCode", Basic.IsNull(shipResp.NoticeCode));
                nvParams.Add("@NoticeMessage", Basic.IsNull(shipResp.NoticeMessage));
                nvParams.Add("@NoticeSeverity", Basic.IsNull(shipResp.NoticeSeverity));
                nvParams.Add("@NoticeSource", Basic.IsNull(shipResp.NoticeSource));
                nvParams.Add("@ErrorMessage", Basic.IsNull(shipResp.ErrorMessage, ""));

                nvParams.Add("@LabelString", Basic.IsNull(shipResp.FedexLabelString, ""));
                nvParams.Add("@RegEmpId", UserId);

                RetVal = DataLayer.ExecuteSpSql(strSQL, nvParams, DBCatalog.SCM21);


                return RetVal;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save Shipping Service Error Log
        /// </summary>
        /// <param name="clsService"></param>
        /// <param name="clsAddress"></param>
        private void SetServiceErrorLog(string orderNumber, ShipServiceInfo clsService, ShipServiceResponse shipResp, string ErrorMessage)
        {
            NameValueCollection nvParams = null;
            string strSQL = string.Empty;
            string RetVal = string.Empty;
            string strResultCode = string.Empty;
            string strResultMessage = string.Empty;
            string strErrorMessage = string.Empty;
            bool strIsSuccess = false;

            try
            {
                //strSQL = "spFedExAPIClass;2";
                strSQL = "spSetFedExDayEndErrorInsert";

                if (shipResp == null)
                {
                    strResultCode = string.Empty;
                    strResultMessage = string.Empty;
                    strErrorMessage = string.Empty;
                }
                else
                {
                    strIsSuccess = shipResp.isSuccess;
                    strResultCode = (shipResp.NoticeCode == null ? "" : shipResp.NoticeCode);
                    strResultMessage = (shipResp.NoticeMessage == null ? "" : shipResp.NoticeMessage);
                    strErrorMessage = (shipResp.ErrorMessage == null ? ErrorMessage : shipResp.ErrorMessage);
                }

                nvParams = new NameValueCollection();
                nvParams.Add("@OrderNumber", orderNumber);
                nvParams.Add("@ServiceType", clsService.ServiceType.ToString());
                nvParams.Add("@PackageWeight", clsService.PackageWeight.ToString());
                nvParams.Add("@IsLive", fnCvtYN(clsService.IsLive));
                nvParams.Add("@CountryCode", clsService.CountryCode);
                nvParams.Add("@LanguageCode", clsService.LanguageCode);
                nvParams.Add("@LocaleCode", clsService.LocaleCode);
                nvParams.Add("@Currency", clsService.Currency);
                nvParams.Add("@WeightUnit", clsService.WeightUnit.ToString());
                nvParams.Add("@DimensionsUnit", clsService.DimensionsUnit.ToString());
                nvParams.Add("@ImageType", clsService.LabelImageType.ToString());
                nvParams.Add("@FormatType", clsService.LabelFormatType.ToString());
                nvParams.Add("@StockType", clsService.LabelStockType.ToString());
                nvParams.Add("@PrintingOrientation", clsService.LabelPrintingOrientation.ToString());
                nvParams.Add("@isSuccess", fnCvtYN(strIsSuccess));
                nvParams.Add("@ResultCode", strResultCode);
                nvParams.Add("@ResultMessage", strResultMessage);
                nvParams.Add("@ErrorMessage", strErrorMessage);
                nvParams.Add("@RegEmpId", UserId);

                RetVal = DataLayer.ExecuteSpSql(strSQL, nvParams, DBCatalog.SCM21);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SavePrintInfo(string cartonId, string weight)
        {
            NameValueCollection nvParams = null;
            string strSQL = string.Empty;
            string RetVal = string.Empty;

            try
            {
                //strSQL = "spKR_UPSIntegration;108";
                strSQL = "spSetFedExSCMCartonWeight";
                nvParams = new NameValueCollection();
                nvParams.Add("@DataOwnerID", sPrintInfo.Warehouse);
                nvParams.Add("@CartonID", cartonId);
                nvParams.Add("@Transport", sPrintInfo.Transport);
                nvParams.Add("@Weight", weight);
                nvParams.Add("@PrintTime", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
                nvParams.Add("@Width", sPrintInfo.Width);
                nvParams.Add("@Length", sPrintInfo.Width);
                nvParams.Add("@Height", sPrintInfo.Width);

                RetVal = DataLayer.ExecuteSpSql(strSQL, nvParams, DBCatalog.SCM21);

                if (RetVal != "")
                {
                    throw new Exception(RetVal);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SavePrintPath(string filePath, string orderNumber, string trackingNumber, bool isReprint, String service)
        {
            NameValueCollection nvParams = null;
            string strSQL = string.Empty;
            string RetVal = string.Empty;

            try
            {
                if (isReprint)
                {
                    if (service.Equals("Fedex"))
                    {
                        //strSQL = "spFedExAPIClass;6";
                        strSQL = "spSetFedExDayEndRePrint";
                    }
                    else if (service.Equals("OnTrac"))
                    {
                        //strSQL = "spOnTracAPIClass;3";
                        strSQL = "spSetOntracDayEndRePrint";
                    }
                    nvParams = new NameValueCollection();
                    nvParams.Add("@OrderNumber", orderNumber);
                }
                else
                {
                    if (service.Equals("Fedex"))
                    {
                        //strSQL = "spFedExAPIClass;4";
                        strSQL = "spSetFedExDayEndPrint";
                    }
                    else if (service.Equals("OnTrac"))
                    {
                        //strSQL = "spOnTracAPIClass;2";
                        strSQL = "spSetOntracDayEndPrint";
                    }
                    nvParams = new NameValueCollection();
                    nvParams.Add("@OrderNumber", orderNumber);
                    nvParams.Add("@FilePath", filePath);
                    nvParams.Add("@TrackingNumber", trackingNumber);
                }

                RetVal = DataLayer.ExecuteSpSql(strSQL, nvParams, DBCatalog.SCM21);

                if (RetVal != "")
                {
                    throw new Exception(RetVal);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static DataTable GetZPLReplace(String shippingService, out string RetVal)
        {
            NameValueCollection nvParams = null;
            string strSQL = string.Empty;
            RetVal = string.Empty;
            DataTable dt = null;
            try
            {
                if (shippingService.Equals("Fedex"))
                {
                    //strSQL = "spFedExZPLReplace";
                    strSQL = "spGetFedExZPLReplace";
                }
                else if (shippingService.Equals("OnTrac"))
                {
                    //strSQL = "spOnTracZPLReplace";
                    strSQL = "spGetOnTracZPLReplace";
                }
                nvParams = new NameValueCollection();

                dt = DataLayer.ExecuteSpDataTable(strSQL, nvParams, DBCatalog.SCM21);

            }
            catch (Exception ex)
            {
                RetVal = ex.Message;
            }
            finally
            {
                if (dt != null) dt.Dispose();
                if (nvParams != null) nvParams.Clear();
            }

            return dt;
        }
        #endregion DB
        
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

        public class PackingMethod
        {
            public static string Shoes = "0";
            public static string PPBoxFedEx = "1";
            public static string PPBoxAPX = "2";
        }

        public class SearchType
        {
            public static string PO = "0";
            public static string ItemCode = "1";
        }

        public class ResultStatus
        {
            public static string NoFedex = "1";
            public static string Reprint = "2";
            public static string Error = "3";
            public static string Success = "0";
            public static string OnTrac = "4";
        }

        public class Status
        {
            public static string Error = "Error";
            public static string Success = "Success";
        }

        public class sPrintInfo
        {
            //public static string Warehouse = "0001";
            public static string Warehouse = "0000";
            public static string Transport = "";
            public static string Width = "1";
        }

        public class FedExShipmentsType
        {
            public static string FedEx = "0";
            public static string Panda = "1";
        }

        public class Result
        {
            public string RetVal { get; set; }
            public LabelInfo Label { get; set; }
            public string TrackingNumber { get; set; }
        }

        public class LabelInfo
        {
            protected string labelString = "";

            public string OrderNumber { get; set; }

            public string LabelString
            {
                get { return labelString; }
                set { labelString = value; }
            }
        }

        public string GetDefaultValues()
        {
            string retVal = string.Empty;

            try
            {
                System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
                PrinterName = (string)settingsReader.GetValue("PrinterName", typeof(String));
                AccountCountry = (string)settingsReader.GetValue("AccountCountry", typeof(String));

                if (string.IsNullOrEmpty(PrinterName) || string.IsNullOrEmpty(AccountCountry))
                    retVal = "[GetDefaultValues Error]";
            }
            catch (Exception ex)
            {
                retVal = "[GetDefaultValues Error] " + ex.Message;
            }

            if (string.IsNullOrEmpty(retVal))
            {
                string OldZPL = string.Empty;
                string NewZPL = string.Empty;

                //2016.08.30 - ZPL Replace 
                nvZPLReplace = new NameValueCollection();
                DataTable ZPLReplaceDt = GetZPLReplace("Fedex", out retVal);
                if (ZPLReplaceDt == null || ZPLReplaceDt.Rows.Count == 0)
                    return "[GetDefaultValues Error] " + "ZPLReplace List Load Fail";

                if (ZPLReplaceDt != null)
                {
                    foreach (DataRow dr in ZPLReplaceDt.Rows)
                    {
                        OldZPL = dr["OldZPL"].ToString();
                        NewZPL = dr["NewZPL"].ToString();
                        nvZPLReplace.Add(OldZPL, NewZPL);
                    }
                }

                //2017 03 13 -OnTrac ZPL Replace
                nvOnTracZPLReplace = new NameValueCollection();
                DataTable ZPLOnTracReplaceDt = GetZPLReplace("OnTrac", out retVal);
                if (ZPLOnTracReplaceDt == null || ZPLOnTracReplaceDt.Rows.Count == 0)
                    return "[GetDefaultValues Error] " + "ZPLReplace List Load Fail";
                if (ZPLOnTracReplaceDt != null)
                {
                    foreach (DataRow dr in ZPLOnTracReplaceDt.Rows)
                    {
                        OldZPL = dr["OldZPL"].ToString();
                        NewZPL = dr["NewZPL"].ToString();
                        nvZPLReplace.Add(OldZPL, NewZPL);
                    }
                }

                if (!string.IsNullOrEmpty(retVal))
                {
                    retVal = "[GetDefaultValues Error] " + retVal;
                }
            }

            return retVal;
        }
        #endregion UTIL

    }   

}
