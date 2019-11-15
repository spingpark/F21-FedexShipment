using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using System.IO;
using System.Configuration;
using System.Collections;
using System.Reflection;

using Microsoft.Win32;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       Basic
     *  Author      JinChul Kim 
     *  Create      9/25/2009
     *  Desc        ���α׷� ����Ŭ����
     *
     *  Program     (PrintLabel) F21.Common\Basic.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     ******************************************************************************************/
    /// <summary>
    /// ������ ��� (Auto / Manually)
    /// </summary>
    public enum PrintMode
    {
        Auto,
        Manually
    }

    public struct UPS_SEND_OUT
    {
        public string PickupDate;
        public string ErrCode;
        public string TrackingNumber;
        public string RecType;
        public string Fieldname;
        public string FieldContents;
        public string ErrMessage;
    }

    /// <summary>
    /// ���� Ŭ����
    /// </summary>
    public class Basic
    {
        private const string c_RegistryKey = @"Software\PrintLabel";

        #region ������Ʈ�� ����
        public static string GetRegistryKey(string key)
        {
            string retVal = string.Empty;
            try
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey(c_RegistryKey);

                if (regKey != null)
                {
                    retVal = regKey.GetValue(key).ToString();
                    regKey.Close();
                }
            }
            catch (Exception ex)
            {
                //retVal = ex.ToString();
            }

            return retVal;
        }

        public static void SetRegistryKey(string key, string value)
        {
            string retVal = string.Empty;
            try
            {
                RegistryKey regKey = Registry.CurrentUser.CreateSubKey(c_RegistryKey);

                if (regKey != null)
                {
                    regKey.SetValue(key, value);
                    regKey.Close();
                }
            }
            catch (Exception ex)
            {
                //retVal = ex.ToString();
            }
        }
        #endregion

        #region �޼ҵ� ��ġ����
        /// <summary>
        /// ������Ʈ�� + Ŭ������ + �޼ҵ�� ����
        /// </summary>
        /// <param name="mb">�޼ҵ�����</param>
        /// <returns></returns>
        public static string GetMethodBase(MethodBase mb)
        {
            return mb.ReflectedType + "." + mb.Name;
        }
        #endregion

        #region ���̺� ��Ű�� ����
        /// <summary>
        /// ��Ű�� ����
        /// </summary>
        /// <param name="SourceTable">���� ���̺�</param>
        /// <param name="CopyTable">������ ���̺�</param>
        /// <returns></returns>
        public static DataTable CopySchema(DataTable SourceTable, DataTable CopyTable)
        {
            if (CopyTable.Columns.Count > 0)
                return null;

            DataColumn col;
            foreach (DataColumn c in SourceTable.Columns)
            {
                col = new DataColumn();
                col.ColumnName = c.ColumnName;
                col.DataType = c.DataType;

                CopyTable.Columns.Add(col);
            }
            return CopyTable;

        }
        #endregion

        #region Ư������ ����
        public static string GetString(string str)
        {
            string strTmp = "";
            string strResult = "";

            for (int i = 0; i < str.Length; i++)
            {
                strTmp = str.Substring(i, 1);

                if (System.Text.RegularExpressions.Regex.IsMatch(strTmp, "^([0-9])+$"))
                    strResult += strTmp;
                else if (System.Text.RegularExpressions.Regex.IsMatch(strTmp, "^([A-Z])+$"))
                    strResult += strTmp;
                else if (System.Text.RegularExpressions.Regex.IsMatch(strTmp, "^([a-z])+$"))
                    strResult += strTmp;
                else
                    strResult += " ";
            }

            strResult = strResult.Trim();
            System.Diagnostics.Debug.WriteLine("[" + strResult + "]");
            
            return strResult;
        }
        #endregion

        #region ������ȿ�� üũ
        /// <summary>
        /// ������ ���� ��ȿ�� üũ
        /// </summary>
        /// <param name="txt">�Է°�</param>
        /// <returns>������ȿ������</returns>
        public static bool IsNumeric(string txt)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(txt.Trim(), "^([0-9])+$"))
                return false;
            else
                return true;
        }

        /// <summary>
        /// �Ҽ��� ������ ���� ��ȿ�� üũ
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static bool IsDecimal(string txt)
        {
            try
            {
                decimal d;

                return decimal.TryParse(txt.Trim(), out d);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region õ���� ���� �޸� �߰�
        /// <summary>
        /// õ���� ���� �޸��� �߰��մϴ�.
        /// </summary>
        /// <param name="strNumeric"></param>
        /// <returns></returns>
        public static string GetNumericFormat(string strNumeric)
        {
            string mark = "";

            if (strNumeric.StartsWith("-"))
            {
                strNumeric = strNumeric.Replace("-", "");
                mark = "-";
            }
            if (!IsNumeric(strNumeric))
                return "";

            strNumeric = ToIntStr(strNumeric);  
             
                    
            string result = strNumeric;
            for (int i = strNumeric.Length - 3; i > 0; i = i - 3)
            {
                result = result.Insert(i, ",");

            }

            return mark + result;

            //return string.Format("{0:#,###}", Convert.ToInt64(strNumeric));

        }
        #endregion

        #region �Ҽ��� ������ ���ڿ��� �ݿø��� ���ڿ��� ��ȯ
        /// <summary>
        /// �Ҽ��� ������ ���ڿ��� �ݿø��� ���ڿ��� ��ȯ
        /// </summary>
        /// <param name="strNum"></param>
        /// <returns></returns>
        public static string ToIntStr(string strNum)
        {
            int retVal = Convert.ToInt32(Convert.ToDecimal(strNum.Trim()));
            return retVal.ToString();
        }
        #endregion

        #region ��üũ
        /// <summary>
        /// �Էµ� �����Ͱ� Null �ΰ�� replaceVal �� ��ȯ�Ѵ�
        /// </summary>
        /// <param name="val"></param>
        /// <param name="replaceVal"></param>
        /// <returns></returns>
        public static string IsNull(object val,string replaceVal)
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
        /// �Էµ� �����Ͱ� Null �ΰ�� "" �� ��ȯ�Ѵ�
        /// </summary>
        /// <param name="val"></param>
        /// <param name="replaceVal"></param>
        /// <returns></returns>
        public static string IsNull(object val)
        {
            if (val == null)
                return "";
            else if (val == DBNull.Value)
                return "";
            else if (val.ToString() == "")
                return "";
            else
                return val.ToString();
        }
        #endregion

        #region GetConnectionString
        public static string GetConnectionString(string conStr, string name)
        {
            //string conStr = conStr;

            string[] strArrays = new string[10];

            char[] ch = new char[1];
            ch[0] = ';';

            string strTmp = "";

            strArrays = conStr.Split(ch);

            foreach (string str in strArrays)
            {
                if (str.StartsWith(name))
                {
                    strTmp = str.Substring(str.LastIndexOf("=") + 1, str.Length - str.LastIndexOf("=") - 1);
                    break;
                }
            }
            //System.Diagnostics.Debug.WriteLine("[" + name + "] " + strTmp);
            return strTmp.Trim();
        }
        #endregion

        public static ArrayList GetConList()
        {
            ArrayList arrDB = new ArrayList();

            foreach (ConnectionStringSettings strConn in ConfigurationManager.ConnectionStrings)
            {
                if( strConn.ConnectionString.StartsWith("data source"))
                    continue;

                arrDB.Add(Encryption.Decrypt(strConn.ConnectionString,true));
            }

            return arrDB;
        }

        public static string GetConList(string name)
        {
            return Encryption.Decrypt(ConfigurationManager.ConnectionStrings[name].ConnectionString,true);
        }

        public static void SaveConStr(string db, string conStr)
        {
            Configuration currentConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (currentConfig.ConnectionStrings.ConnectionStrings[db] != null)
            {
                currentConfig.ConnectionStrings.ConnectionStrings[db].ConnectionString = Encryption.Encrypt(conStr,true);
            }
            else
            {
                currentConfig.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings(db, Encryption.Encrypt(conStr,true)));
            }

            currentConfig.Save();  // ��������� config ���Ͽ� ����
            ConfigurationManager.RefreshSection("connectionStrings");   // ���� ����

        }

        public static string GetIP()
        {
            String myIPAddr = String.Empty;
            System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.Equals(System.Net.Sockets.AddressFamily.InterNetwork) && ip.ToString().StartsWith("10"))
                {
                    myIPAddr = ip.ToString();
                    break;
                }
            }

            return myIPAddr;
        }

    }//END class

    public class StringManager
    {
        string str = "";
        int position = 0;

        string result = "";

        public StringManager(string str)
        {
            this.str = str;
        }

        public string Get(int length)
        {
            if (str.Length >= position + length)
            {
                result = str.Substring(position, length);
                position += length;
            }
            else
            {
                result = TextManager.GetStringByte("", length);
            }

            return result;
        }

    }//END class
        
}//END namespace
