using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Collections.Specialized;
using F21.Framework;

namespace F21.Service
{
    public class AddressInfo
    {
        string cartonId = string.Empty;
        string receiveName = string.Empty;
        string organization = string.Empty;
        string line1 = string.Empty;
        string line2 = string.Empty;
        string line3 = string.Empty;
        string line4 = string.Empty;        
        string line = string.Empty;
        string city = string.Empty;
        string state = string.Empty;
        string countryCode = string.Empty;
        string countryName = string.Empty;
        string postalCode = string.Empty;
        string tel = string.Empty;
        string mobile = string.Empty;
        string phoneNumber = string.Empty;
        //string orderNumber = string.Empty;
        string email = string.Empty;


        public AddressInfo(string iCartonId, string iWeight, string iUserId, bool isLive)
        {

            DataTable dt = null;
            string strSQL = string.Empty;
            NameValueCollection nvParams = null;

            try
            {
                //spFedExShipments
                strSQL = "spGetFedExCartonInfo";
                nvParams = new NameValueCollection();
                nvParams.Add("@CartonID", iCartonId);
                nvParams.Add("@Weight", iWeight);
                nvParams.Add("@UserID", iUserId);

                dt = DataLayer.ExecuteSpDataTable(strSQL, nvParams, DBCatalog.SCM21);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                    {
                        cartonId = iCartonId;
                        //receiveName = IsNull(dt.Rows[0]["ReceiveName"], ""); 
                        receiveName = IsNull(dt.Rows[0]["BannerName"], ""); //2016.10.27 - receiveName을 StoreId가 아닌 Forever21로 수정 (minkyu.r)
                        organization = IsNull(dt.Rows[0]["Organization"], "");
                        line1 = IsNull(dt.Rows[0]["Line"], "");
                        line = line1;
                        line2 = IsNull(dt.Rows[0]["Line2"], "");
                        city = IsNull(dt.Rows[0]["City"], "");
                        state = IsNull(dt.Rows[0]["State"], "");
                        countryCode = IsNull(dt.Rows[0]["CountryCode"], "");
                        countryName = IsNull(dt.Rows[0]["CountryName"], "");
                        postalCode = IsNull(dt.Rows[0]["PostalCode"], "");
                        tel = IsNull(dt.Rows[0]["TEL"], "");
                        mobile = IsNull(dt.Rows[0]["MOBILE"], "");
                        email = IsNull(dt.Rows[0]["Email"], "");          
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
            
            /*
            orderAddressID = "";
            receiveName = "Paulina Martinez";
            organization = "";
            line = "Av Francisco I. Madero Pte";
            city = "Morelia";
            state = "MI";
            countryCode = "MX";
            countryName = "Mexico";
            postalCode = "58000";
            tel = "01800900";
            mobile = "01800900";
             */
        }

        /// <summary>
        /// IsNull Validation
        /// </summary>
        /// <param name="val">Value</param>
        /// <param name="replaceVal">Replace Value</param>
        /// <returns></returns>
        private string IsNull(object val, string replaceVal)
        {
            if (val == null)
                return replaceVal;
            else if (val == DBNull.Value)
                return replaceVal;
            else if (val.ToString() == "")
                return replaceVal;
            else
                return val.ToString();
        }

        /// <summary>
        /// CartonId
        /// </summary>
        public string CartonId
        {
            get
            {
                return cartonId;
            }
        }

        /// <summary>
        /// Receive Name
        /// </summary>
        public string ReceiveName
        {
            get
            {
                return receiveName;
            }
        }

        /// <summary>
        /// Organization
        /// </summary>
        public string Organization
        {
            set
            {
                organization = value;

            }
            get
            {
                return organization;
            }
        }

        /// <summary>
        /// Address 1
        /// </summary>
        public string Line1
        {
            get
            {
                return line1;
            }
        }

        /// <summary>
        /// Address 2
        /// </summary>
        public string Line2
        {
            get
            {
                return line2;
            }
        }

        /// <summary>
        /// Address 3
        /// </summary>
        public string Line3
        {
            get
            {
                return line3;
            }
        }

        /// <summary>
        /// Address 4
        /// </summary>
        public string Line4
        {
            get
            {
                return line4;
            }
        }




        /// <summary>
        /// Address
        /// </summary>
        public string Line
        {
            get
            {
                return line;
            }
        }

        /// <summary>
        /// City
        /// </summary>
        public string City
        {
            get
            {
                return city;
            }
        }

        /// <summary>
        /// State
        /// </summary>
        public string State
        {
            get
            {
                return state;
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
        }

        /// <summary>
        /// Country Name
        /// </summary>
        public string CountryName
        {
            get
            {
                return countryName;
            }
        }

        /// <summary>
        /// Postal Code
        /// </summary>
        public string PostalCode
        {
            get
            {
                return postalCode;
            }
        }

        /// <summary>
        /// Telephone Number
        /// </summary>
        public string Tel
        {
            get
            {
                return tel;
            }
        }

        /// <summary>
        /// Cell Phone Number
        /// </summary>
        public string Mobile
        {
            get
            {
                return mobile;
            }
        }

        /// <summary>
        /// Phone Number
        /// </summary>
        public string PhoneNumber
        {
            get
            {
                if (Tel == "") { return Mobile; }
                else { return Tel; }
            }
        }
        
        /// <summary>
        /// OrderNumber
        /// </summary>
        //public string OrderNumber
        //{
        //    get
        //    {
        //        return orderNumber;
        //    }
        //}

        public string Email
        {
            get
            {
                return email;
            }
        }

        public bool IsResidence
        {
            get
            {
                if (organization.Equals("1",StringComparison.OrdinalIgnoreCase))
                    return true;
                else
                    return false;
            }
        }
    }
}
