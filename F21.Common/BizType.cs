using System;
using System.Collections.Generic;
using System.Text;

namespace F21.Framework
{
    /******************************************************************************************
     *  Class       BizType
     *  Author      JinChul Kim 
     *  Create      9/25/2009
     *  Desc        타입정의
     *
     *  Program     (PrintLabel) Biz.BizType\BizType.cs
     *  Modify      JinChul Kim, 20090925, Case#(011-6085), FedEx Shipping Add
     * 
     ******************************************************************************************/
    public enum AmountType
    {
        GBP,
        USD
    }

    public enum LabelType
    {
        UPS,
        USPS,
        FedEx,
        TestShip
    }

    public enum Mode
    {
        TEST,
        LIVE
    }

    public enum ShipMethodType
    {
        EMAIL_DELIVERY,
        EXPRESS,
        EXPRESS_DELIVERY,
        PRIORITY_INTERNATIONAL,
        STANDARD,
        STANDARD_DELIVERY,
        STANDARD_SHIPPING,

        /// <summary>
        /// Gift Card Only
        /// </summary>
        USPS_DELIVERY,

        USPS_INTERNATIONAL,

        /// <summary>
        /// APO/FPO
        /// </summary>
        USPS_PRIORITY,

        /// <summary>
        /// US Territories
        /// </summary>
        USPS_PRIORITY_MAIL,

        /// <summary>
        /// AK/HI, APO/FPO, PO Box, US Territories (frre shipping)
        /// </summary>
        USPS_PRIORITY_SHIPPING,

        /// <summary>
        /// Next Day Saver (Regular Order)
        /// Next Day Saver Delivery (Only Gift Card Order)
        /// </summary>
        NEXT_DAY_SAVER,
        NEXT_DAY_SAVER_DELIVERY,

        /// <summary>
        /// [5/16/2016 jh.kim] Added Premier, Premier Shipping
        /// </summary>
        PREMIER,
        PREMIER_SHIPPING,
        SHIPTOSTORE
    }

    public enum ImageFormat
    {
        GIF,
        JPEG,
        EPL2
    }

    public enum PayMethodType
    {
        CC,
        GC,
        EC,
        GCCC,
        ECCC
    }

    //public class ReturnMessage
    //{
    //    public static string Can_not_load_label_info = "Can not load label info";
    //    public static string Already_Printed_label = "Already Printed label";
    //}

    public class ReturnValue
    {
        public string ReturnCode = "";
        public string ReturnMsg = "";
        public Exception ReturnException = new Exception();
    }

    public enum UpdateType
    {
        PickUp,
        Order,
        ALL
    }

    public enum TestMode
    {
        YES,
        NO,
        NULL
    }

    public enum ShipServiceType
    {
        UPS_Domestic_MI,
        UPS_International_MI,
        UPS_Ground,
        UPS_2nd_Day,
        UPS_Next_Day,
        UPS_Next_Day_Saver,
        UPS_Next_Day_Saver_Delivery,
        UPS_Basic,
        UPS_3_Day_Select,
        UPS_Next_Day_Saturday,
        UPS_2nd_Day_Sat,

        FedEx_SmartPost,
        FedEx_Ground,
        FedEx_Express,

        USPS_Priority,
        USPS_Delivery,
        USPS_FirstClass,
        USPS_AIT_Priority,  //[6/14/2016 jh.kim] Added USPS_AIT_Priority
        //USPS_AIT_FirstClass,
        DHL_International,

        UPS_SurePost,
        ShipToStore,
        NULL,

        // [FedExAPI_0.0.1] Added FedEx Ship Service
        /// <summary>
        /// Standard/Standard Shipping
        /// </summary>
        FedEx_Home_Delivery,
        /// <summary>
        /// Express
        /// </summary>
        FedEx_2_Day,
        /// <summary>
        /// Next Day Saver
        /// </summary>
        FedEx_Standard_Overnight,
        /// <summary>
        /// Holiday Upgrade
        /// </summary>
        FedEx_Express_Saver,
        /// <summary>
        /// Holiday Upgrade
        /// </summary>
        FedEx_Priority_Overnight,
        /// <summary>
        /// Extra
        /// </summary>
        FedEx_2_Day_AM,
        /// <summary>
        /// Extra
        /// </summary>
        FedEx_First_Overnight,
    }

    public enum DestinationType
    {
        AK_HI,
        APO_FPO,
        US_Territories,
        PR,
        VI,
        Regular

    }

    public class MasterCode
    {
        public static string ShipMethod = "B041";
        public static string ServiceType = "B042";
        public static string Destination = "B043";
        public static string Exception = "B044";
    }

    public class OrderStatus
    {
        public static string NewOrder = "";
        public static string AutoPrint = "0010";
        public static string Pickup = "0015";
        public static string OutOfStock = "0020";
        public static string Complete = "0025";
        public static string Packed = "0030";
        public static string Shipped = "0060";
        public static string Canceled = "0050";
        public static string Pending = "0005";
    }

    public class ShippingCarrier
    {
        public static string UPS = "0001";
        public static string USPS = "0002";
        public static string FEDEX = "0003";
        public static string DHL = "0004";
    }

    public enum UpsShipSettings
    {
        LabelImage,
        PackType,
        RequestOption,
        ServiceCode,
        ShipmentCharge
    }

    //9/12/2013 Munkyu Kim : Move From PrintLabel/frmPrintLabel.cs
    /// <summary>
    /// Writer      : Jay Shin
    /// Create Date : May 22 2012
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public class PrintLabelLogType
    {
        public static string AddressError = "001";
        public static string CaptureError = "002";
        public static string LabelError = "003";
        public static string EtcError = "004";
        public static string AITError = "005";
    }

    // [FedExAPI_0.0.1] Added FedEx WebService
    /// <summary>
    /// FedEx Close Shipment
    /// </summary>
    public enum CloseType
    {
        GROUND,
        SMART_POST
    }

    // [FedExAPI_0.0.1] Added FedEx WebService
    public enum OrderType
    {
        Customer,
        Replenishment
    }

}//END namespace
