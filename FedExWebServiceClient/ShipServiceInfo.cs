using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Collections.Specialized;
using System.Web.Services.Protocols;
using FedExWebServiceClient.ShipServiceWebReference;
using F21.Framework;

namespace F21.Service
{
    public class ShipServiceInfo
    {
        private ShipServiceType shipServiceType = ShipServiceType.NULL;

        //Account Info
        string accountKey = string.Empty;
        string accountPassword = string.Empty;
        string accountNumber = string.Empty;
        string meterNumber = string.Empty;
        string payorAccountNumber = string.Empty;

        //Shipping Info
        string shipperAddress = string.Empty;
        string shipperAddress2 = string.Empty;
        string shipperCity = string.Empty;
        string shipperPostalCode = string.Empty;
        string shipperStateCode = string.Empty;
        string shipperCountryCode = string.Empty;
        string shipperName = string.Empty;
        string shipperPhoneNumber = string.Empty;

        //Label Info
        string labelImageType = string.Empty;
        string labelFormatType = string.Empty;
        string labelStockType = string.Empty;
        string labelPrintingOrientation = string.Empty;

        string labelType = string.Empty;
        string labelHeight = string.Empty;
        string labelwidth = string.Empty;
        string labelImageCode = string.Empty;

        //Etc Info
        string languageCode = string.Empty;
        string localeCode = string.Empty;
        string currency = string.Empty;
        string weightUnit = string.Empty;
        string dimensionsUnit = string.Empty;
        
        string serviceType = string.Empty;
        string hubId = string.Empty;

        decimal codAmount = 0.00M;
        bool isCOD = false;

                
        decimal packageWeight = 0.00M;

        bool isLive = true;
        string orderNumber = string.Empty;
        //string reference = string.Empty;
        string countryCode = string.Empty;

        public ShipServiceInfo(string i_accountCountry, bool i_isLive, decimal orderWeight, bool isCOD)
        {
            packageWeight = orderWeight;
            isLive = i_isLive;
            //countryCode = i_country;

            DataTable dt = null;
            string strIsLive = string.Empty;   
                     
            try
            {                
                strIsLive = i_isLive ? "Y" : "N";
                //spFedexShippingWebService; 1
                string SQL = "spGetFedExShippingWebService";
                NameValueCollection nvParams = new NameValueCollection();

                nvParams.Add("@Country", i_accountCountry);
                nvParams.Add("@isLive",     strIsLive);
                nvParams.Add("@AccType",    (isCOD ? "COD" : "GENERAL"));

                dt = DataLayer.ExecuteSpDataTable(SQL, nvParams, DBCatalog.SCM21);

                if (dt != null && dt.Rows.Count > 0)
                {
                    accountKey = dt.Rows[0]["Key"].ToString();
                    accountPassword = dt.Rows[0]["Password"].ToString();
                    accountNumber = dt.Rows[0]["AccountNumber"].ToString();
                    meterNumber = dt.Rows[0]["MeterNumber"].ToString();
                    payorAccountNumber = dt.Rows[0]["PayorAccountNumber"].ToString();

                    shipperName = dt.Rows[0]["CompanyName"].ToString();
                    shipperPhoneNumber = dt.Rows[0]["PhoneNumber"].ToString();
                    shipperAddress = dt.Rows[0]["Line1"].ToString();
                    shipperAddress2 = dt.Rows[0]["Line2"].ToString();
                    shipperCity = dt.Rows[0]["City"].ToString();
                    shipperStateCode = dt.Rows[0]["StateCode"].ToString();
                    shipperCountryCode = dt.Rows[0]["CountryCode"].ToString();
                    shipperPostalCode = dt.Rows[0]["PostalCode"].ToString();

                    countryCode = dt.Rows[0]["CountryCode"].ToString();

                    languageCode = dt.Rows[0]["LanguageCode"].ToString();
                    localeCode = dt.Rows[0]["LocaleCode"].ToString();
                    currency = dt.Rows[0]["Currency"].ToString();
                    weightUnit = dt.Rows[0]["WeightUnit"].ToString();
                    dimensionsUnit = dt.Rows[0]["DimensionsUnit"].ToString();
                    labelImageType = dt.Rows[0]["ImageType"].ToString();
                    labelFormatType = dt.Rows[0]["FormatType"].ToString();
                    labelStockType = dt.Rows[0]["StockType"].ToString();
                    labelPrintingOrientation = dt.Rows[0]["PrintingOrientation"].ToString();
                    hubId = dt.Rows[0]["HubId"].ToString();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

            #region Backup TEST
            /*
            accountKey = "LzCOH6BjbLHZc27E";
            accountPassword = "I1iwcQHuvY6UgJWatnQZZdq4c";
            accountNumber = "510087909";
            meterNumber = "118598287";
            payorAccountNumber = "510087909";

            shipperAddress = "Perif. Paseo de la Republica ";
            shipperAddress2 = "";
            shipperCity = "Morelia";
            shipperPostalCode = "58290";
            shipperStateCode = "MI";
            shipperCountryCode = "MX";
            shipperName = "Forever21";
            shipperPhoneNumber = "0805522713";

            labelImageType = "ZPLII";
            labelFormatType = "COMMON2D";
            labelStockType = "STOCK_4X6";
            labelPrintingOrientation = "BOTTOM_EDGE_OF_TEXT_FIRST";

            languageCode = "ES";
            localeCode = "MX";
            currency = "NMP";
            weightUnit = "KG";
            dimensionsUnit = "CM";
             * */
            #endregion
        }


        /// <summary>
        /// Shipping Service Type
        /// </summary>
        public ShipServiceType ShipServiceType
        {
            get
            {
                return shipServiceType;
            }
            set
            {
                shipServiceType = value;
            }

        }

        /// <summary>
        /// FedEx Service Type
        /// </summary>
        public ServiceType ServiceType
        {
            get
            {
                ServiceType iServiceType;

                switch (serviceType)    // F21.Framework.ShipServiceType
                {
                    case "Standard Shipping":
                        iServiceType = ServiceType.STANDARD_OVERNIGHT;
                        break;
                    case "Standard":
                        iServiceType = ServiceType.STANDARD_OVERNIGHT;
                        break;
                    case "Standard Delivery":
                        iServiceType = ServiceType.STANDARD_OVERNIGHT;
                        break;
                    case "Express":
                        iServiceType = ServiceType.FEDEX_EXPRESS_SAVER;
                        break;
                    case "FEDEX_GROUND":
                        iServiceType = ServiceType.FEDEX_GROUND;
                        break;

                    default:
                        iServiceType = ServiceType.FEDEX_GROUND;
                        break;
                }

                return iServiceType;
            }
        }

        /// <summary>
        /// Key
        /// </summary>
        public string AccountKey
        {
            get
            {
                return accountKey;
            }
        }

        /// <summary>
        /// Password
        /// </summary>
        public string AccountPassword
        {
            get
            {
                return accountPassword;
            }
        }

        /// <summary>
        /// Account Number
        /// </summary>
        public string AccountNumber
        {
            get
            {
                return accountNumber;
            }
        }

        /// <summary>
        /// Meter Number
        /// </summary>
        public string MeterNumber
        {
            get
            {
                return meterNumber;
            }
        }

        /// <summary>
        /// Payor Account Number
        /// </summary>
        public string PayorAccountNumber
        {
            get
            {
                return payorAccountNumber;
            }
        }

        /// <summary>
        /// Shipper Address
        /// </summary>
        public string ShipperAddress
        {
            get
            {
                return shipperAddress;
            }
        }

        /// <summary>
        /// Shipper Address2
        /// </summary>
        public string ShipperAddress2
        {
            get
            {
                return shipperAddress2;
            }
        }

        /// <summary>
        /// Shipper City
        /// </summary>
        public string ShipperCity
        {
            get
            {
                return shipperCity;
            }
        }
            
        /// <summary>
        /// Shipper PostalCode
        /// </summary>
        public string ShipperPostalCode
        {
            get
            {
                return shipperPostalCode;
            }
        }
            
        /// <summary>
        /// Shipper StateCode
        /// </summary>
        public string ShipperStateCode
        {
            get
            {
                return shipperStateCode;
            }
        }
            
        /// <summary>
        /// Shipper CountryCode
        /// </summary>
        public string ShipperCountryCode
        {
            get
            {
                return shipperCountryCode;
            }
        }
            
        /// <summary>
        /// Shipper Name
        /// </summary>
        public string ShipperName
        {
            get
            {
                return shipperName;
            }
        }
            
        /// <summary>
        /// Shipper PhoneNumber
        /// </summary>
        public string ShipperPhoneNumber
        {
            get
            {
                return shipperPhoneNumber;
            }
        }

        
        /// <summary>
        /// Label ImageType
        /// </summary>
        public ShippingDocumentImageType LabelImageType
        {
            get
            {
                ShippingDocumentImageType ilabelImageType;

                switch (labelImageType)
                {
                    case "DOC":
                        ilabelImageType = ShippingDocumentImageType.DOC;
                        break;
                    case "DPL":
                        ilabelImageType = ShippingDocumentImageType.DPL;
                        break;
                    case "EPL2":
                        ilabelImageType = ShippingDocumentImageType.EPL2;
                        break;
                    case "PDF":
                        ilabelImageType = ShippingDocumentImageType.PDF;
                        break;
                    case "PNG":
                        ilabelImageType = ShippingDocumentImageType.PNG;
                        break;
                    case "RTF":
                        ilabelImageType = ShippingDocumentImageType.RTF;
                        break;
                    case "TEXT":
                        ilabelImageType = ShippingDocumentImageType.TEXT;
                        break;
                    case "ZPLII":
                        ilabelImageType = ShippingDocumentImageType.ZPLII;
                        break;
                    default:
                        ilabelImageType = ShippingDocumentImageType.ZPLII;
                        break;
                }

                return ilabelImageType;
            }
        }
        
        /// <summary>
        /// Label FormatType
        /// </summary>
        public LabelFormatType LabelFormatType
        {
            get
            {
                LabelFormatType ilabelFormatType;

                switch (labelFormatType)
                {
                    case "01":
                        ilabelFormatType = LabelFormatType.COMMON2D;
                        break;
                    case "02":
                        ilabelFormatType = LabelFormatType.FEDEX_FREIGHT_STRAIGHT_BILL_OF_LADING;
                        break;
                    case "03":
                        ilabelFormatType = LabelFormatType.LABEL_DATA_ONLY;
                        break;
                    case "04":
                        ilabelFormatType = LabelFormatType.VICS_BILL_OF_LADING;
                        break;
                    default:
                        ilabelFormatType = LabelFormatType.COMMON2D;
                        break;
                }

                return ilabelFormatType;
            }
        }
        
        /// <summary>
        /// Label StockType
        /// </summary>
        public LabelStockType LabelStockType
        {
            get
            {
                LabelStockType ilabelStockType;

                switch (labelStockType)
                {
                    case "01":
                        ilabelStockType = LabelStockType.PAPER_4X6;
                        break;
                    case "02":
                        ilabelStockType = LabelStockType.PAPER_4X8;
                        break;
                    case "03":
                        ilabelStockType = LabelStockType.PAPER_4X9;
                        break;
                    case "04":
                        ilabelStockType = LabelStockType.PAPER_6X4;
                        break;
                    case "05":
                        ilabelStockType = LabelStockType.PAPER_7X475;
                        break;
                    case "06":
                        ilabelStockType = LabelStockType.PAPER_85X11_BOTTOM_HALF_LABEL;
                        break;
                    case "07":
                        ilabelStockType = LabelStockType.PAPER_85X11_TOP_HALF_LABEL;
                        break;
                    case "08":
                        ilabelStockType = LabelStockType.PAPER_LETTER;
                        break;
                    case "09":
                        ilabelStockType = LabelStockType.STOCK_4X6;
                        break;
                    case "10":
                        ilabelStockType = LabelStockType.STOCK_4X675_LEADING_DOC_TAB;
                        break;
                    case "11":
                        ilabelStockType = LabelStockType.STOCK_4X675_TRAILING_DOC_TAB;
                        break;
                    case "12":
                        ilabelStockType = LabelStockType.STOCK_4X8;
                        break;
                    case "13":
                        ilabelStockType = LabelStockType.STOCK_4X9_LEADING_DOC_TAB;
                        break;
                    case "14":
                        ilabelStockType = LabelStockType.STOCK_4X9_TRAILING_DOC_TAB;
                        break;
                    default:
                        ilabelStockType = LabelStockType.STOCK_4X6;
                        break;
                }


                return ilabelStockType;
            }
        }
        
        /// <summary>
        /// Label PrintingOrientation
        /// </summary>
        public LabelPrintingOrientationType LabelPrintingOrientation
        {
            get
            {
                LabelPrintingOrientationType ilabelPrintingOrientation;
                
                switch (labelPrintingOrientation)
                {
                    case "01":
                        ilabelPrintingOrientation = LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST;
                        break;
                    case "02":
                        ilabelPrintingOrientation = LabelPrintingOrientationType.TOP_EDGE_OF_TEXT_FIRST;
                        break;
                    default:
                        ilabelPrintingOrientation = LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST;
                        break;

                }

                return ilabelPrintingOrientation;
            }
        }
        
        /// <summary>
        /// LanguageCode
        /// </summary>
        public string LanguageCode
        {
            get
            {
                return languageCode;
            }
        }
        
        /// <summary>
        /// LocaleCode
        /// </summary>
        public string LocaleCode
        {
            get
            {
                return localeCode;
            }
        }
        
        
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency
        {
            get
            {
                return currency;
            }
        }

        /// <summary>
        /// WeightUnit
        /// </summary>
        public WeightUnits WeightUnit
        {
            get
            {
                WeightUnits iweightUnit;

                switch (weightUnit)
                {
                    case "KG":
                        iweightUnit = WeightUnits.KG;
                        break;
                    case "LB":
                        iweightUnit = WeightUnits.LB;
                        break;
                    default:
                        iweightUnit = WeightUnits.LB;
                        break;
                }

                return iweightUnit;
            }
        }
       
        /// <summary>
        /// DimensionsUnit
        /// </summary>
        public LinearUnits DimensionsUnit
        {
            get
            {
                LinearUnits idimensionsUnit;

                switch (dimensionsUnit)
                {
                    case "CM":
                        idimensionsUnit = LinearUnits.CM;
                        break;
                    case "IN":
                        idimensionsUnit = LinearUnits.IN;
                        break;
                    default:
                        idimensionsUnit = LinearUnits.CM;
                        break;
                }

                return idimensionsUnit;
            }
        }



         /// <summary>
        /// Weight Value
        /// </summary>
        public decimal PackageWeight
        {
            get
            {
                decimal dPackageWeight = 0.00M;

                if (decimal.TryParse(packageWeight.ToString(), out dPackageWeight))
                {
                    return dPackageWeight;
                }
               
                return dPackageWeight;
            }
        }

        /// <summary>
        /// IsLive
        /// </summary>
        public bool IsLive
        {
            get
            {
                return isLive;
            }
        }


        public string HubId
        {
            get
            {
                return this.hubId;
            }
        }


        /// <summary>
        /// OrderNumber
        /// </summary>
        public string OrderNumber
        {
            get
            {
                return orderNumber;
            }
            set
            {
                orderNumber = value;
            }
        }
        

        ///// <summary>
        ///// CUSTOMER_REFERENCE
        ///// </summary>
        //public string Reference
        //{
        //    get
        //    {
        //        return reference;
        //    }
        //    set
        //    {
        //        reference = value;
        //    }
        //}


        public decimal CODAmount
        {
            get
            {
                return codAmount;
            }
            set
            {
                codAmount = value;
            }
        }

        public bool IsCOD
        {
            get
            {
                return isCOD;
            }

            set
            {
                isCOD = value;
            }
        }


        /// <summary>
        /// Country Code
        /// </summary>
        public string CountryCode
        {
            get
            {
                return countryCode;
            }
            set
            {
                countryCode = value;
            }
        }

        
    }
}
