using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.Windows.Forms;
using System.Configuration;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       ConfigManager
     *  Author      JinChul Kim 
     *  Create      9/25/2009
     *  Desc        Config 파일 읽기, 쓰기
     *
     *  Program     (PrintLabel) Biz.UPS\ConfigManager.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     ******************************************************************************************/
    public class ConfigManager
    {
        XmlDocument xmlDoc;

        string filePath = Application.StartupPath + "\\" + "FedEx_Shipments.exe.config";

        public ConfigManager()
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
        }

        public ConfigManager(string pFilepath)
        {
            xmlDoc = new XmlDocument();
            xmlDoc.Load(pFilepath);
        }

        public void AddKey(string strKey, string strValue)
        {
            XmlNode appSettingsNode =
              xmlDoc.SelectSingleNode("configuration/appSettings");
            try
            {
                if (KeyExists(strKey))
                    throw new ArgumentException("Key name: <" + strKey +
                              "> already exists in the configuration.");
                XmlNode newChild = appSettingsNode.FirstChild.Clone();
                newChild.Attributes["key"].Value = strKey;
                newChild.Attributes["value"].Value = strValue;
                appSettingsNode.AppendChild(newChild);
                //We have to save the configuration in two places, 
                //because while we have a root App.config,
                //we also have an ApplicationName.exe.config.
                xmlDoc.Save(AppDomain.CurrentDomain.BaseDirectory +
                                             "..\\..\\App.config");
                xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Updates a key within the App.config
        public void UpdateKey(string strKey, string newValue)
        {
            if (!KeyExists(strKey))
                throw new ArgumentNullException("Key", "<" + strKey +
                      "> does not exist in the configuration. Update failed.");
            XmlNode appSettingsNode =
               xmlDoc.SelectSingleNode("configuration/appSettings");
            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == strKey)
                {
                    childNode.Attributes["value"].Value = newValue;
                    break;  
                }
            }
            xmlDoc.Save(filePath);
            //xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

        }

        public void DeleteKey(string strKey)
        {
            if (!KeyExists(strKey))
                throw new ArgumentNullException("Key", "<" + strKey +
                      "> does not exist in the configuration. Update failed.");
            XmlNode appSettingsNode =
               xmlDoc.SelectSingleNode("configuration/appSettings");
            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == strKey)
                    appSettingsNode.RemoveChild(childNode);
            }
            xmlDoc.Save(AppDomain.CurrentDomain.BaseDirectory + "..\\..\\App.config");
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        }

        // Determines if a key exists within the App.config
        private bool KeyExists(string strKey)
        {
            XmlNode appSettingsNode =
              xmlDoc.SelectSingleNode("configuration/appSettings");
            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == strKey)
                    return true;
            }
            return false;
        }

        public string GetKeyValue(string strKey)
        {
            string strRetVal = string.Empty;

            XmlNode appSettingsNode =
              xmlDoc.SelectSingleNode("configuration/appSettings");
            // Attempt to locate the requested setting.
            foreach (XmlNode childNode in appSettingsNode)
            {
                if (childNode.Attributes["key"].Value == strKey)
                    strRetVal = childNode.Attributes["value"].Value;
            }
            return strRetVal;
        }

        // Determines if a key exists within the App.config
        public bool SelectNodeKeyExists(string strKey)
        {
            try
            {
                if (xmlDoc.GetElementsByTagName(strKey).Item(0) != null)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetSelectNodeKeyValue(string strKey)
        {
            string strRetVal = string.Empty;

            if (SelectNodeKeyExists(strKey))
                strRetVal = xmlDoc.GetElementsByTagName(strKey).Item(0).InnerText;
            
            return strRetVal;
        }

        public string GetSelectNodePropertyValue(string strKey, string property)
        {
            string strRetVal = string.Empty;
            try
            {
                if (SelectNodeKeyExists(strKey))
                    strRetVal = xmlDoc.GetElementsByTagName(strKey).Item(0).Attributes.GetNamedItem(property).Value;
            }
            catch
            {
                strRetVal = "";
            }
            return strRetVal;
        }

        public string GetAppSetting(string strKey)
        {
            return System.Configuration.ConfigurationManager.AppSettings[strKey].ToString();
        }
        public static String GetAppSetting2(String strKey)
        {
            return System.Configuration.ConfigurationManager.AppSettings[strKey].ToString();
        }
        public void ConfigModifyTest()
        {
            Console.WriteLine(ConfigurationManager.AppSettings["ActiveTab"].ToString());

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["ActiveTab"].Value = "hello";

            config.Save();

            ConfigurationManager.RefreshSection("appSettings");

            Console.WriteLine(ConfigurationManager.AppSettings["ActiveTab"].ToString());
        }

        

    }//END class
}//END namespace
