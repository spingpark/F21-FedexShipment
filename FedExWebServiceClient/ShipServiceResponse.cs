using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace F21.Service
{
    public class ShipServiceResponse
    {
        public bool isSuccess { get; set; }
        public string FedexLabelString { get; set; }
        public string TrackingIdType { get; set; }
        public string FedexTrackingNumber { get; set; }
           
        public string ServiceType { get; set; }

        public string HighestSeverity { get; set; }

        public string NoticeCode { get; set; }
        public string NoticeMessage { get; set; }
        public string NoticeSeverity { get; set; }
        public string NoticeSource { get; set; }

        public string ErrorMessage { get; set; }

        public string FormId { get; set; }

        // Barcode Details
        public string Barcode { get; set; }
        public string BarcodeType { get; set; }
        public string BinaryBarcodeType { get; set; }

        public string UrsaPrefixCode { get; set; }
        public string UrsaSuffixCode { get; set; }
        public string DestinationLocationId { get; set; }
        public string AirportId { get; set; }

        public decimal BillingWeight { get; set; }
        public string BillingWeightUnits { get; set; }

        public string DeliveryDay { get; set; }
        public string DeliveryShortDate { get; set; }
        public string TransitTime { get; set; }

        // Smart_Post
        public string SmartPickupCode { get; set; }
        public bool   SmartMachinable { get; set; }
        public string USPSTrackingNumber { get; set; }
        public string USPSTrackingIdType { get; set; }
        public string USPSFormId { get; set; }
        public string USPSBarcode { get; set; }
        public string USPSBarcodeType { get; set; }
        public string UspsApplicationId { get; set; }
        
        private string[] binaryBarcodesField;
        private string[] stringBarcodesField;
        private string[] stringBarCodesType;
        
        public string[] ResponseBinaryBarcodes
        {
            get
            {
                return this.binaryBarcodesField;
            }
            set
            {
                this.binaryBarcodesField = value;
            }
        }

        public string[] ResponseStringBarcodes
        {
            get
            {
                return this.stringBarcodesField;
            }
            set
            {
                this.stringBarcodesField = value;
            }
        }

        public string[] ResponseStringBarcodesType
        {
            get
            {
                return this.stringBarCodesType;
            }
            set
            {
                this.stringBarCodesType = value;
            }
        }
    }
} 
