using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections.Specialized;


namespace F21.Framework
{
    public class ScaleDefaultInfo
    {
        static ScaleDefaultInfo ScaleDefaultSet;

        bool useScale;
        string commend;
        string portName;
        string baudRate;
        string parity;
        string dataBit;
        string stopBit;
        string scaleType;
        bool defaultWeight;
        string barcodeSize;

        ConfigManager cm;

        public static ScaleDefaultInfo GetInstance()
        {
            if (ScaleDefaultSet == null)
                ScaleDefaultSet = new ScaleDefaultInfo();

            return ScaleDefaultSet;
        }

        public ScaleDefaultInfo()
        {
            string strUseScale = Basic.GetRegistryKey("UseScale");
            if (string.IsNullOrEmpty(strUseScale))
                useScale = true;
            else
                useScale = Convert.ToBoolean(Basic.GetRegistryKey("UseScale"));
            commend = Basic.GetRegistryKey("Commend");
            portName = Basic.GetRegistryKey("PortName");
            baudRate = Basic.GetRegistryKey("BaudRate");
            Parity = Basic.GetRegistryKey("Parity");
            dataBit = Basic.GetRegistryKey("DataBit");
            StopBit = Basic.GetRegistryKey("StopBit");
            ScaleType = Basic.GetRegistryKey("ScaleType");
            string strDefaultWeight = Basic.GetRegistryKey("DefaultWeight");
            if (string.IsNullOrEmpty(strDefaultWeight))
                defaultWeight = false;
            else
                defaultWeight = Convert.ToBoolean(Basic.GetRegistryKey("DefaultWeight"));
            barcodeSize = Basic.GetRegistryKey("BarcodeSize");
        }

        public void Save()
        {
            Basic.SetRegistryKey("UseScale", UseScale.ToString());
            Basic.SetRegistryKey("Commend", Commend.ToString());
            Basic.SetRegistryKey("PortName", PortName.ToString());
            Basic.SetRegistryKey("BaudRate", BaudRate.ToString());
            Basic.SetRegistryKey("Parity", Parity.ToString());
            Basic.SetRegistryKey("DataBit", DataBit.ToString());
            Basic.SetRegistryKey("StopBit", StopBit.ToString());
            Basic.SetRegistryKey("ScaleType", ScaleType.ToString());
            Basic.SetRegistryKey("DefaultWeight", DefaultWeight.ToString());
            Basic.SetRegistryKey("BarcodeSize", BarcodeSize.ToString());
        }

        public bool UseScale
        {
            get
            {
                return useScale;
            }
            set
            {
                useScale = value;
            }
        }

        public string Commend
        {
            get
            {
                return commend;
            }
            set
            {
                commend = value;
            }
        }

        public string PortName
        {
            get
            {
                return portName;
            }
            set
            {
                portName = value;
            }
        }

        public string BaudRate
        {
            get
            {
                return baudRate;
            }
            set
            {
                baudRate = value;
            }
        }

        public string Parity
        {
            get
            {
                return parity;
            }
            set
            {
                parity = value;
            }
        }

        public string DataBit
        {
            get
            {
                return dataBit;
            }
            set
            {
                dataBit = value;
            }
        }

        public string StopBit
        {
            get
            {
                return stopBit;
            }
            set
            {
                stopBit = value;
            }
        }

        public bool DefaultWeight
        {
            get
            {
                return defaultWeight;
            }
            set
            {
                defaultWeight = value;
            }
        }

        public string BarcodeSize
        {
            get
            {
                return barcodeSize;
            }
            set
            {
                barcodeSize = value;
            }
        }

        public string ScaleType
        {
            get
            {
                string rtnCode = string.Empty;
                switch (scaleType)
                {
                    case "IQ Plus": rtnCode = "0001"; break;
                    case "DIGI": rtnCode = "0002"; break;
                    case "B-TEK": rtnCode = "0003"; break;
                    case "Pennsylvania Scale": rtnCode = "0004"; break;
                    case "Mettler Toledo": rtnCode = "0005"; break;
                    default: rtnCode = scaleType; break;
                }
                return rtnCode;
            }
            set
            {   
                scaleType = value;
            }
        }
    }
}
