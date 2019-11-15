using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using System.Web.Services.Protocols;
using System.Globalization;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Soap;
using FedExWebServiceClient.ShipServiceWebReference;
using F21.Framework;

namespace F21.Service
{
    public class ShipClient
    {        
        public ShipServiceResponse ShipClientService(ShipServiceInfo clsService, AddressInfo clsAddress)
        {
            FedExWebServiceClient.ShipServiceWebReference.ShipService service = new FedExWebServiceClient.ShipServiceWebReference.ShipService();
            ShipServiceResponse shipResponse = new ShipServiceResponse();
                       
            string iResultCode = string.Empty;
            string iResultMessage = string.Empty;
            string iErrorMessage = string.Empty;
            string retValue = string.Empty;            
            string strApiUrl = string.Empty;

            string noticeCode = string.Empty;
            string noticeMessage = string.Empty;
            string noticeSeverity = string.Empty;
            string noticeSource = string.Empty;


            // WebService Url Setting 
            retValue = GetWebserviceUrl(out strApiUrl);
            if (!string.IsNullOrEmpty(retValue))
            {
                shipResponse.isSuccess = false;
                shipResponse.ErrorMessage = "[ERROR] Ship Webservice url is empty.\r\n  Error Message : " + retValue;

                return shipResponse;
            }

            try
            {
                bool isCodShipment = clsService.IsCOD; // Don't use cash on delivery in US.                

                // Create Shipment Request
                ProcessShipmentRequest request = CreateShipmentRequest(clsService, clsAddress, isCodShipment);

                // Webservice URL
                service.Url = strApiUrl;

                // Call the ship web service passing in a ProcessShipmentRequest and returning a ProcessShipmentReply

                System.Diagnostics.Debug.WriteLine(Encoding.Default.GetString(serializeToSoap(request)));
                
                ProcessShipmentReply reply = service.processShipment(request);

                // Call ShowNotifications(reply)
                NameValueCollection nvcNotification = ShowNotifications(reply, out iResultMessage);
                foreach (string nvKey in nvcNotification.AllKeys)
                {
                    switch (nvKey)
                    {
                        case "Code":
                            shipResponse.NoticeCode = nvcNotification[nvKey].ToString();                            
                            break;
                        case "Message":
                            shipResponse.NoticeMessage = nvcNotification[nvKey].ToString();
                            break;
                        case "Severity":
                            shipResponse.NoticeSeverity = nvcNotification[nvKey].ToString();
                            break;
                        case "Source":
                            shipResponse.NoticeSource = nvcNotification[nvKey].ToString();
                            break;
                        default:
                            break;
                    }
                }

                // SUCCESS, NOTE, WARNING
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    shipResponse.HighestSeverity = reply.HighestSeverity.ToString();

                    //retVal = ShowNotifications(reply, out iResultCode, out iResultMessage);
                    // Call ShowNotifications(reply)

                    shipResponse = ShowShipmentReply(isCodShipment, reply, clsService, shipResponse, out retValue);
                    
                    //shipResponse.ResultCode = iResultCode;
                    //shipResponse.ResultMessage = iResultMessage;

                    if (retValue.Equals(string.Empty))
                    {
                        shipResponse.isSuccess = true;
                    }
                    else
                    {
                        // Check FedexTrackingNumber (If Smart_Post service type)
                        if (!string.IsNullOrEmpty(shipResponse.FedexTrackingNumber) && !string.IsNullOrEmpty(shipResponse.FedexLabelString))
                        {
                            shipResponse.isSuccess = true;  // if exists Tracking number and Label string.
                        }
                        else
                        {
                            shipResponse.isSuccess = false;
                        }

                        shipResponse.ErrorMessage = "[ShowShipmentReply Error] " + retValue;
                    }
                }
                else // ERROR, FAILURE
                {
                    shipResponse.HighestSeverity = Basic.IsNull(reply.HighestSeverity.ToString(), "Empty");

                    shipResponse.isSuccess = false;
                    shipResponse.ErrorMessage = "[" + shipResponse.HighestSeverity + "] Notification Code: " + shipResponse.NoticeCode + ",\r\nNotification Message: " + shipResponse.NoticeMessage;

                    //retVal = ShowNotifications(reply, out iResultMessage);
                    
                    //if (retVal == "")
                    //{
                    //    shipResponse.ResultCode = iResultCode;
                    //    shipResponse.ResultMessage = iResultMessage;

                    //    shipResponse.isSuccess = false;
                    //    shipResponse.ErrorMessage = "[" + shipResponse.HighestSeverity + "] ResultCode: " + shipResponse.NoticeCode + ", ResultMsg: " + shipResponse.NoticeMessage;
                    //}
                    //else
                    //{
                    //    shipResponse.isSuccess = false;
                    //    shipResponse.ErrorMessage = retVal;
                    //}
                }

                //shipResponse.NoticeCode = noticeCode;
                //shipResponse.NoticeMessage = noticeMessage;
                //shipResponse.NoticeSeverity = noticeSeverity;
                //shipResponse.NoticeSource = noticeSource;                
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw ex;
            }
            catch (System.ServiceModel.CommunicationException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (service != null) service.Dispose();
            }

            return shipResponse;
        }

        private static ProcessShipmentRequest CreateShipmentRequest(ShipServiceInfo clsService, AddressInfo clsAddress, bool isCodShipment)
        {
            // Build the ShipmentRequest
            ProcessShipmentRequest request = new ProcessShipmentRequest();

            // Set WebAuthenticationCredential
            request.WebAuthenticationDetail = SetWebAuthenticationDetail(clsService.AccountKey, clsService.AccountPassword);
                        
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber  = clsService.AccountNumber;
            request.ClientDetail.MeterNumber    = clsService.MeterNumber;

            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = clsService.OrderNumber; // The client will get the same value back in the response
            
            request.Version = new VersionId();

            // GROUND_HOME_DELIVERY ISSUE
            //RateRequest request = new RateRequest();

            //Set Shipment Details
            SetShipmentDetails(request, clsService);

            // Set Sender
            SetSender(request, clsService);

            // Set Recipient
            SetRecipient(request, clsAddress, clsService.ServiceType);

            // Set Payment
            SetPayment(request, clsService.PayorAccountNumber, clsService.CountryCode); // Payor CountryCode

            // Set Label Details
            SetLabelDetails(request, clsService);

            // Set Package Line Items
            SetPackageLineItems(request, clsService, isCodShipment, clsAddress.CartonId.Substring(2,4));
            
            // FedEx_SmartPost
            if (clsService.ServiceType == ServiceType.SMART_POST)
            {
                SetSmartPostDetails(request, clsService.PackageWeight, clsService.HubId);
            }

            return request;
        }

        /// <summary>
        /// Set Web Authentication Detail
        /// </summary>
        /// <param name="accountKey"></param>
        /// <param name="accountPassword"></param>
        /// <returns></returns>
        private static WebAuthenticationDetail SetWebAuthenticationDetail(string accountKey, string accountPassword)
        {
            WebAuthenticationDetail wad = new WebAuthenticationDetail();
            wad.UserCredential = new WebAuthenticationCredential();
            wad.UserCredential.Key = accountKey;
            wad.UserCredential.Password = accountPassword;

            //wad.ParentCredential = new WebAuthenticationCredential();
            //wad.ParentCredential.Key = accountKey;
            //wad.ParentCredential.Password = accountPassword;
            
            return wad;
        }

        private static void SetShipmentDetails(ProcessShipmentRequest request, ShipServiceInfo clsService)
        {

            //2016.08.23 - Changed ShipTimestamp (minkyu.r)
            DateTime time_1 = DateTime.Now;
            string cutOfftime = DateTime.Now.ToShortDateString() + " 22:00";
            DateTime time_2 = DateTime.Parse(cutOfftime);
            int timeCompare = DateTime.Compare(time_1, time_2);

            request.RequestedShipment = new RequestedShipment();

            //FedEx Service Issue. (The server is on central time so it thinks you are sending a ship request for “yesterday”)
            //[ERROR] Notification Code: 1935,  Notification Message: Invalid date entered
            if (timeCompare > 0)
            {
                request.RequestedShipment.ShipTimestamp = DateTime.Now.AddHours(+2);
            }
            else
            {
                request.RequestedShipment.ShipTimestamp = DateTime.Now;
            }

            //request.RequestedShipment.ShipTimestamp = DateTime.Now;     // Ship date and time
            request.RequestedShipment.DropoffType   = DropoffType.REGULAR_PICKUP;

            request.RequestedShipment.ServiceType = clsService.ServiceType;   // Service types are GROUND_HOME_DELIVERY, STANDARD_OVERNIGHT, PRIORITY_OVERNIGHT, ...
            request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING; // Packaging type FEDEX_BOK, FEDEX_PAK, FEDEX_TUBE, YOUR_PACKAGING, ...                                    
            request.RequestedShipment.PackageCount = "1";

            // 멕시코 소스코드 참조하여 추가함.
            request.RequestedShipment.TotalWeight = new Weight(); // Total weight information
            request.RequestedShipment.TotalWeight.Value = clsService.PackageWeight;     //1;
            request.RequestedShipment.TotalWeight.Units = clsService.WeightUnit;        // WeightUnits.LB;
                                                                
            // Set HAL
            //bool isHALShipment = false;
            //if (isHALShipment)
            //    SetHAL(request);
        }


        private static void SetSender(ProcessShipmentRequest request, ShipServiceInfo clsService)
        {            
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Contact = new Contact();
            request.RequestedShipment.Shipper.Contact.CompanyName = clsService.ShipperName;
            request.RequestedShipment.Shipper.Contact.PhoneNumber = clsService.ShipperPhoneNumber;
            
            request.RequestedShipment.Shipper.Address = new Address();
            if (string.IsNullOrEmpty(clsService.ShipperAddress2))
            {
                request.RequestedShipment.Shipper.Address.StreetLines = new string[1] { clsService.ShipperAddress };
            }
            else
            {
                request.RequestedShipment.Shipper.Address.StreetLines = new string[2] { clsService.ShipperAddress, clsService.ShipperAddress2 };
            }

            request.RequestedShipment.Shipper.Address.City = clsService.ShipperCity;
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = clsService.ShipperStateCode;
            request.RequestedShipment.Shipper.Address.PostalCode = clsService.ShipperPostalCode;
            request.RequestedShipment.Shipper.Address.CountryCode = clsService.ShipperCountryCode; 
        }

        private static void SetRecipient(ProcessShipmentRequest request, AddressInfo clsAddress, ServiceType shipServiceType)
        {            
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Contact = new Contact();
            request.RequestedShipment.Recipient.Contact.PersonName = RemoveDiacritics.RemoveAccent(clsAddress.ReceiveName);
            //request.RequestedShipment.Recipient.Contact.CompanyName = "Recipient Company Name";
            request.RequestedShipment.Recipient.Contact.PhoneNumber = clsAddress.PhoneNumber;
                        
            request.RequestedShipment.Recipient.Address = new Address();

            if (string.IsNullOrEmpty(clsAddress.Line2))
            {
                request.RequestedShipment.Recipient.Address.StreetLines = new string[1] { RemoveDiacritics.RemoveAccent(clsAddress.Line1) };
            }
            else
            {
                request.RequestedShipment.Recipient.Address.StreetLines = new string[2] { RemoveDiacritics.RemoveAccent(clsAddress.Line1), RemoveDiacritics.RemoveAccent(clsAddress.Line2) };
            }
            
            request.RequestedShipment.Recipient.Address.City = RemoveDiacritics.RemoveAccent(clsAddress.City); 
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = clsAddress.State; 
            request.RequestedShipment.Recipient.Address.PostalCode = clsAddress.PostalCode; 
            request.RequestedShipment.Recipient.Address.CountryCode = clsAddress.CountryCode; 

            // ***체크 사항 (V17 에는 없는 값임 -  Ground 와 Home Delivery 를 체크 하는 값인지 확인)
            if (ServiceType.GROUND_HOME_DELIVERY == shipServiceType)
            {
                request.RequestedShipment.Recipient.Address.Residential = true;     // ShipWebServiceClient\Express 에서도 사용 함.
                request.RequestedShipment.Recipient.Address.ResidentialSpecified = true;    // Test

                //request.RequestedShipment.SpecialServicesRequested.HomeDeliveryPremiumDetail = new HomeDeliveryPremiumDetail();
                //request.RequestedShipment.SpecialServicesRequested.HomeDeliveryPremiumDetail.HomeDeliveryPremiumType = HomeDeliveryPremiumType.DATE_CERTAIN;
                //request.RequestedShipment.SpecialServicesRequested.HomeDeliveryPremiumDetail.Date = DateTime.Now;
                //request.RequestedShipment.SpecialServicesRequested.HomeDeliveryPremiumDetail.PhoneNumber = clsAddress.PhoneNumber;

            }            
        }

        private static void SetPayment(ProcessShipmentRequest request, string payorAccountNumber, string countryCode)
        {
            
            request.RequestedShipment.ShippingChargesPayment = new Payment();
            request.RequestedShipment.ShippingChargesPayment.PaymentType = PaymentType.SENDER;

            request.RequestedShipment.ShippingChargesPayment.Payor = new Payor();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty = new Party();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.AccountNumber = payorAccountNumber;

            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Contact = new Contact();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Address = new Address();
            request.RequestedShipment.ShippingChargesPayment.Payor.ResponsibleParty.Address.CountryCode = countryCode; 
        }

        private static void SetLabelDetails(ProcessShipmentRequest request, ShipServiceInfo clsService)
        {   
            request.RequestedShipment.LabelSpecification = new LabelSpecification();
            request.RequestedShipment.LabelSpecification.ImageType = clsService.LabelImageType;         // ShippingDocumentImageType.ZPLII; // Use this line for a PDF label
            request.RequestedShipment.LabelSpecification.ImageTypeSpecified = true;                        
            request.RequestedShipment.LabelSpecification.LabelFormatType = clsService.LabelFormatType;  // LabelFormatType.COMMON2D;
                        
            request.RequestedShipment.LabelSpecification.LabelStockType = clsService.LabelStockType;    // LabelStockType.STOCK_4X6;
            request.RequestedShipment.LabelSpecification.LabelStockTypeSpecified = true;
            request.RequestedShipment.LabelSpecification.LabelPrintingOrientation = clsService.LabelPrintingOrientation; // LabelPrintingOrientationType.BOTTOM_EDGE_OF_TEXT_FIRST;
            request.RequestedShipment.LabelSpecification.LabelPrintingOrientationSpecified = true;

            // *** 체크사항 (There is no below sorucecode on ship version v17)            
            request.RequestedShipment.LabelSpecification.CustomerSpecifiedDetail = new CustomerSpecifiedLabelDetail();
            request.RequestedShipment.LabelSpecification.CustomerSpecifiedDetail.TermsAndConditionsLocalization = new Localization();
            request.RequestedShipment.LabelSpecification.CustomerSpecifiedDetail.TermsAndConditionsLocalization.LanguageCode = clsService.LanguageCode;     // "EN";
            request.RequestedShipment.LabelSpecification.CustomerSpecifiedDetail.TermsAndConditionsLocalization.LocaleCode = clsService.LocaleCode;         // "US";
            
            /*
            Requests customer-specific barcode on FedEx Ground and FedEx Home Delivery labels.
            Valid values are:
            􀁸 NONE
            􀁸 COMMON_2D
            􀁸 SSCC_18
            􀁸 USPS
            Note: USPS is applicable for FedEx SmartPost shipments.
            request.RequestedShipment.LabelSpecification.CustomerSpecifiedDetail.SecondaryBarcode = SecondaryBarcodeType.COMMON_2D;            
            */
        }

        private static void SetPackageLineItems(ProcessShipmentRequest request, ShipServiceInfo clsService, bool isCodShipment,string departmentID)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[1];
            request.RequestedShipment.RequestedPackageLineItems[0] = new RequestedPackageLineItem();
            request.RequestedShipment.RequestedPackageLineItems[0].SequenceNumber = "1";

            // Package weight information
            request.RequestedShipment.RequestedPackageLineItems[0].Weight = new Weight();
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Value = clsService.PackageWeight; // decimal
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Units = clsService.WeightUnit;    // WeightUnits.LB;

            // CustomerReferences
            // Jinbeom.p 2019.01.14 Customer DepartmentID 추가
            //request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences = new CustomerReference[2];
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences = new CustomerReference[3];
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[0] = new CustomerReference();
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[0].CustomerReferenceType = CustomerReferenceType.P_O_NUMBER;
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[0].Value = clsService.OrderNumber;    

            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[1] = new CustomerReference();
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[1].CustomerReferenceType = CustomerReferenceType.CUSTOMER_REFERENCE;
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[1].Value = clsService.OrderNumber;

            // Jinbeom.p 2019.01.14 Customer DepartmentID 추가
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[2] = new CustomerReference();
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[2].CustomerReferenceType = CustomerReferenceType.DEPARTMENT_NUMBER;
            request.RequestedShipment.RequestedPackageLineItems[0].CustomerReferences[2].Value = departmentID;

            // Set COD (Cash On Delivery)   
            if (isCodShipment) SetCOD(request, clsService);
        }

        private static void SetHAL(ProcessShipmentRequest request)
        {
            request.RequestedShipment.SpecialServicesRequested = new ShipmentSpecialServicesRequested();
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes = new ShipmentSpecialServiceType[1];
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes[0] = ShipmentSpecialServiceType.HOLD_AT_LOCATION;
            //
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail = new HoldAtLocationDetail();
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.PhoneNumber = "9011234567";
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress = new ContactAndAddress();
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Contact = new Contact();
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Contact.PersonName = "Tester";
            //
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Address = new Address();
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Address.StreetLines = new string[1];
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Address.StreetLines[0] = "45 Noblestown Road";
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Address.City = "Pittsburgh";
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Address.StateOrProvinceCode = "NJ";
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Address.PostalCode = "15220";
            request.RequestedShipment.SpecialServicesRequested.HoldAtLocationDetail.LocationContactAndAddress.Address.CountryCode = "US";
        }

        private static void SetCOD(ProcessShipmentRequest request, ShipServiceInfo clsService)
        {
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested = new PackageSpecialServicesRequested();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes = new PackageSpecialServiceType[1];
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes[0] = PackageSpecialServiceType.COD;
            //
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail = new CodDetail();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CollectionType = CodCollectionType.GUARANTEED_FUNDS;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount = new Money();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount.Amount = clsService.CODAmount;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount.Currency = clsService.Currency;
        }
        
        private static void SetSmartPostDetails(ProcessShipmentRequest request, decimal packageWeight, string hubId)
        {
            request.RequestedShipment.SmartPostDetail = new SmartPostShipmentDetail();

            // Concerning the Indicia, use ‘PARCEL_SELECT’ for anything that’s 1-70 lbs. Anything less than 1 lbs. use ‘PRESORTED_STANDARD’.            
            if (packageWeight < 1.00M)
            {
                request.RequestedShipment.SmartPostDetail.Indicia = SmartPostIndiciaType.PRESORTED_STANDARD; // (less than 1 LB)
            }
            else
            {
                request.RequestedShipment.SmartPostDetail.Indicia = SmartPostIndiciaType.PARCEL_SELECT;     // PARCEL_SELECT (1 LB through 70 LBS)
            }
            request.RequestedShipment.SmartPostDetail.IndiciaSpecified = true;
            request.RequestedShipment.SmartPostDetail.AncillaryEndorsement = SmartPostAncillaryEndorsementType.ADDRESS_CORRECTION;            
            request.RequestedShipment.SmartPostDetail.AncillaryEndorsementSpecified = true;
            request.RequestedShipment.SmartPostDetail.HubId = hubId; // 5902 LACA Los Angeles  

            /*
            5902 LACA Los Angeles (FedEx Live Environment)
            5531 NBWI New Berlin  (FedEx Test Environment)
            Note: Include only the numeric HubID value in your request.
            HubID 5531 should be used in the FedEx Test Environment.
            */
        }
                        
        private static ShipServiceResponse ShowShipmentReply(bool isCodShipment, ProcessShipmentReply reply, ShipServiceInfo clsService, ShipServiceResponse objResponse, out string retVal)
        {
            //F21.Service.ShipServiceResponse objResponse = new F21.Service.ShipServiceResponse();
            retVal = string.Empty;

            try
            {
                // Carrier code types : FDXC(Cargo), FDXE(Express), FDXG(Ground), FXCC(Custom Critical), FXFX(Freight), FXSP - ? (Reference FedEx Document)
                // SmartPost Reply details
                if (clsService.ServiceType == ServiceType.SMART_POST)
                {
                    if (reply.CompletedShipmentDetail.SmartPostDetail.PickUpCarrierSpecified)
                    {
                        objResponse.SmartPickupCode = reply.CompletedShipmentDetail.SmartPostDetail.PickUpCarrier.ToString();
                    }

                    if (reply.CompletedShipmentDetail.SmartPostDetail.MachinableSpecified)
                    {
                        objResponse.SmartMachinable = reply.CompletedShipmentDetail.SmartPostDetail.Machinable;
                    }
                }

                // Details for each package  
                if (reply.CompletedShipmentDetail.CompletedPackageDetails != null)
                {
                    System.Diagnostics.Debug.WriteLine("CompletedPackageDetails Array Length : " + reply.CompletedShipmentDetail.CompletedPackageDetails.Length);
                    foreach (CompletedPackageDetail packageDetail in reply.CompletedShipmentDetail.CompletedPackageDetails)
                    {                        
                        //Get Fedex TrackingNumber  (ShowTrackingDetails)
                        if (packageDetail.TrackingIds != null)
                        {
                            System.Diagnostics.Debug.WriteLine("TrackingIds Array Length : " + packageDetail.TrackingIds.Length);
                            for (int i = 0; i < packageDetail.TrackingIds.Length; i++)
                            {
                                // TrackingIdType (EXPRESS,FEDEX,FREIGHT,GROUND,USPS)
                                // Update ShipService version from v15 to v17
                                objResponse.TrackingIdType = packageDetail.TrackingIds[i].TrackingIdType.ToString();
                                objResponse.FedexTrackingNumber = packageDetail.TrackingIds[i].TrackingNumber;
                                objResponse.FormId = packageDetail.TrackingIds[i].FormId;
                                objResponse.UspsApplicationId = Basic.IsNull(packageDetail.TrackingIds[i].UspsApplicationId, "");
                                /*  (Comment out the below sourcecode)
                                if (packageDetail.TrackingIds[i].TrackingIdType == TrackingIdType.USPS)
                                {
                                    objResponse.USPSTrackingIdType = packageDetail.TrackingIds[i].TrackingIdType.ToString();
                                    objResponse.USPSTrackingNumber = packageDetail.TrackingIds[i].TrackingNumber;
                                    objResponse.USPSFormId = packageDetail.TrackingIds[i].FormId;
                                }
                                else
                                {
                                    objResponse.TrackingIdType = packageDetail.TrackingIds[i].TrackingIdType.ToString();
                                    objResponse.FedexTrackingNumber = packageDetail.TrackingIds[i].TrackingNumber;
                                    objResponse.FormId = packageDetail.TrackingIds[i].FormId;
                                }
                                */
                            }
                        }
                        else
                        {
                            // Tracking Number Issue
                            retVal = "[ERROR] packageDetail.TrackingIds is null.";                            
                        }

                        //Get LabelString (ShowShipmentLabels)                        
                        if (packageDetail.Label.Parts != null)
                        {
                            if (clsService.LabelImageType == ShippingDocumentImageType.ZPLII)
                            {
                                // Get ZPL Label Strig
                                objResponse.FedexLabelString = ShowShipmentLabels(packageDetail);
                            }
                            else if (clsService.LabelImageType == ShippingDocumentImageType.PDF)
                            {
                                objResponse.FedexLabelString = ShowShipmentLabels3(isCodShipment, reply.CompletedShipmentDetail, packageDetail);
                            }
                            else
                            {
                                objResponse.FedexLabelString = ShowShipmentLabels(packageDetail);
                            }
                        }
                        else
                        {
                            retVal = retVal + " [ERROR] packageDetail.Label.Parts is null.";     
                        }

                        // *** Check functon (ShowPackageRateDetails) Don't use package rate detalis information
                        // PackageRateDetails 에러가 종종 발생하기 때문에 주석 처리, TrackingNumber, ZplString Value 가 존재하면 정상 처리함.
                        /* (Comment out the below sourcecode)
                        if (packageDetail.PackageRating.PackageRateDetails != null)
                        {
                            System.Diagnostics.Debug.WriteLine("PackageRateDetails Array Length :" + packageDetail.PackageRating.PackageRateDetails.Length);
                            foreach (PackageRateDetail ratedPackage in packageDetail.PackageRating.PackageRateDetails)
                            {
                                System.Diagnostics.Debug.WriteLine("Billing weight :" + ratedPackage.BillingWeight.Value.ToString());
                                System.Diagnostics.Debug.WriteLine("Billing weight Units :" + ratedPackage.BillingWeight.Units.ToString());

                                objResponse.BillingWeight = ratedPackage.BillingWeight.Value;
                                objResponse.BillingWeightUnits = ratedPackage.BillingWeight.Units.ToString();

                                System.Diagnostics.Debug.WriteLine("Base charge : " + ratedPackage.BaseCharge.Amount + "-" + ratedPackage.BaseCharge.Currency);
                                System.Diagnostics.Debug.WriteLine("Total surcharge : " + ratedPackage.TotalSurcharges.Amount + "-" + ratedPackage.TotalSurcharges.Currency);

                                if (ratedPackage.Surcharges != null)
                                {
                                    foreach (Surcharge surcharge in ratedPackage.Surcharges)
                                    {
                                        System.Diagnostics.Debug.WriteLine("Surcharge : " + surcharge.SurchargeType + " - " + surcharge.Amount.Amount + " - " + surcharge.Amount.Currency);
                                    }
                                }

                                if (ratedPackage.NetCharge != null)
                                {
                                    System.Diagnostics.Debug.WriteLine("Net charge : " + ratedPackage.NetCharge.Amount + "-" + ratedPackage.NetCharge.Currency);
                                }
                            }
                        }
                        */
                        //---------------------------------------------------------------------------------------------------------------------------------------

                        // Get Barcode (ShowBarcodeDetails)
                        if (packageDetail.OperationalDetail.Barcodes != null)
                        {
                            if (packageDetail.OperationalDetail.Barcodes.StringBarcodes != null)
                            {
                                System.Diagnostics.Debug.WriteLine("StringBarcodes Array Length :" + packageDetail.OperationalDetail.Barcodes.StringBarcodes.Length);
                                for (int i = 0; i < packageDetail.OperationalDetail.Barcodes.StringBarcodes.Length; i++)
                                {                                    
                                    //List<string> list = new List<string>();                                    

                                    // String Barcode Types : ADDRESS, ASTRA, FEDEX_1D, GROUND, POSTAL,USPS
                                    // Update ShipService version from v15 to v17
                                    objResponse.Barcode = packageDetail.OperationalDetail.Barcodes.StringBarcodes[i].Value;
                                    objResponse.BarcodeType = packageDetail.OperationalDetail.Barcodes.StringBarcodes[i].Type.ToString();
                                    /* (Comment out the below sourcecode)
                                    if (packageDetail.OperationalDetail.Barcodes.StringBarcodes[i].Type == StringBarcodeType.USPS)
                                    {
                                        objResponse.USPSBarcode = packageDetail.OperationalDetail.Barcodes.StringBarcodes[i].Value;
                                        objResponse.USPSBarcodeType = packageDetail.OperationalDetail.Barcodes.StringBarcodes[i].Type.ToString();
                                    }
                                    else
                                    {
                                        objResponse.Barcode = packageDetail.OperationalDetail.Barcodes.StringBarcodes[i].Value;
                                        objResponse.BarcodeType = packageDetail.OperationalDetail.Barcodes.StringBarcodes[i].Type.ToString();
                                    }                                    
                                    */
                                    //Object reference not set to an instance of an object.
                                    //objResponse.ResponseStringBarcodes[i] = packageDetail.OperationalDetail.Barcodes.StringBarcodes[i].Value;
                                }
                            }

                            // Don't use BinaryBarcodes (Comment out the BinaryBarcodes sourcecode)
                            /*
                            if (packageDetail.OperationalDetail.Barcodes.BinaryBarcodes != null)
                            {
                                System.Diagnostics.Debug.WriteLine("BinaryBarcodes Array Length :" + packageDetail.OperationalDetail.Barcodes.BinaryBarcodes.Length);
                                for (int i = 0; i < packageDetail.OperationalDetail.Barcodes.BinaryBarcodes.Length; i++)
                                {
                                    objResponse.BinaryBarcodeType = packageDetail.OperationalDetail.Barcodes.BinaryBarcodes[i].Type.ToString();
                                }
                            }
                            */
                        }                        
                    }
                }                
                
                // ShowPackageRouteDetails
                if (reply.CompletedShipmentDetail.OperationalDetail != null)
                {
                    ShipmentOperationalDetail routingDetail = reply.CompletedShipmentDetail.OperationalDetail;
                    objResponse.UrsaPrefixCode = routingDetail.UrsaPrefixCode;
                    objResponse.UrsaSuffixCode = routingDetail.UrsaSuffixCode;
                    objResponse.DestinationLocationId = routingDetail.DestinationLocationId;
                    objResponse.AirportId = routingDetail.AirportId;
                                     

                    if (routingDetail.TransitTimeSpecified)
                    {
                        objResponse.TransitTime = routingDetail.TransitTime.ToString();
                    }
                    
                    /*
                    if (routingDetail.DeliveryDaySpecified)
                    {
                        string DeliveryDay = routingDetail.DeliveryDay.ToString();
                    }

                    if (routingDetail.DeliveryDateSpecified)
                    {
                        string DeliveryShortDate = routingDetail.DeliveryDate.ToShortDateString();
                    }
                    */

                    //throw new Exception("Test Error");
                }                                
            }
            catch (Exception ex)
            {
                retVal = retVal + "[ShowShipmentReply Error] " + ex.Message;                
            }

            return objResponse;
        }


        //private static NameValueCollection ShowNotifications(ProcessShipmentReply reply, out string resultCode, out string resultMessage)
        private static NameValueCollection ShowNotifications(ProcessShipmentReply reply, out string resultMessage)
        {
            NameValueCollection nvNotification = new NameValueCollection();
            //resultCode = string.Empty;
            resultMessage = string.Empty;

            try
            {
                if (reply.Notifications[0] != null)
                {
                    Notification notification = reply.Notifications[0];

                    nvNotification.Add("Code", Basic.IsNull(notification.Code, ""));
                    nvNotification.Add("Message", Basic.IsNull(notification.Message, ""));
                    nvNotification.Add("Severity", Basic.IsNull(notification.Severity, "")); //ERROR,FAILURE,NOTE,SUCCESS,WARNING
                    nvNotification.Add("Source", Basic.IsNull(notification.Source, ""));

                    /*
                    resultCode = notification.Code;
                    resultMessage = notification.Message;
                    string NoticeSeverity = notification.Severity.ToString(); 
                    string NoticeSource = notification.Source;
                    */
                }
                else
                {
                    resultMessage = "reply.Notifications[0] is null.";
                }
            }
            catch (Exception ex)
            {
                resultMessage = "[Error ShowNotifications] " + ex.Message;                  
            }

            return nvNotification;
        }

        private static string ShowShipmentLabels(CompletedPackageDetail packageDetail)
        {
            string zplString = string.Empty;            
            
            if (packageDetail.Label.Parts[0].Image != null)
            {
                // Save outbound shipping label
                byte[] labelBuffer = packageDetail.Label.Parts[0].Image;
                
                //strTempLabel = System.Text.Encoding.Default.GetString(labelBuffer);                        
                zplString = System.Text.Encoding.UTF8.GetString(labelBuffer);            
            }

            return zplString;
        }


        //TXT
        private static string ShowShipmentLabels2(bool isCodShipment, CompletedShipmentDetail completedShipmentDetail, CompletedPackageDetail packageDetail)
        {
            string retVal = string.Empty;
            retVal = "";

            if (null != packageDetail.Label.Parts[0].Image)
            {
                // Save outbound shipping label
                string LabelPath = @"c:\\FedEx\\";

                if (!Directory.Exists(LabelPath))
                    Directory.CreateDirectory(LabelPath);

                string LabelFileName = LabelPath + packageDetail.TrackingIds[0].TrackingNumber + ".txt";
                retVal = SaveLabel(LabelFileName, packageDetail.Label.Parts[0].Image, true);

                if (isCodShipment)
                {
                    // Save COD Return label
                    LabelFileName = LabelPath + completedShipmentDetail.AssociatedShipments[0].TrackingId.TrackingNumber + "CR" + ".txt";
                    retVal = SaveLabel(LabelFileName, completedShipmentDetail.AssociatedShipments[0].Label.Parts[0].Image, true);
                }
            }

            return retVal;
        }

        //PDF
        private static string ShowShipmentLabels3(bool isCodShipment, CompletedShipmentDetail completedShipmentDetail, CompletedPackageDetail packageDetail)
        {
            string retVal = string.Empty;
            retVal = "";

            if (null != packageDetail.Label.Parts[0].Image)
            {
                // Save outbound shipping label
                string LabelPath = @"c:\\FedEx\\"; 

                if (!Directory.Exists(LabelPath))
                    Directory.CreateDirectory(LabelPath);

                string LabelFileName = LabelPath + packageDetail.TrackingIds[0].TrackingNumber + ".pdf";
                retVal = SaveLabel(LabelFileName, packageDetail.Label.Parts[0].Image, false);

                if (isCodShipment)
                {
                    // Save COD Return label
                    LabelFileName = LabelPath + completedShipmentDetail.AssociatedShipments[0].TrackingId.TrackingNumber + "CR.pdf";
                    retVal = SaveLabel(LabelFileName, completedShipmentDetail.AssociatedShipments[0].Label.Parts[0].Image, false);
                }
            }

            return retVal;
        }

        private static string SaveLabel(string labelFileName, byte[] labelBuffer, bool isSave)
        {
            string strTempLabel = string.Empty;

            if (isSave)
            {
                //strTempLabel = System.Text.Encoding.Default.GetString(labelBuffer);
                strTempLabel = System.Text.Encoding.UTF8.GetString(labelBuffer);
            }
            else
            {
                strTempLabel = "";
            }

            // Save label buffer to file
            FileStream LabelFile = new FileStream(labelFileName, FileMode.Create);
            LabelFile.Write(labelBuffer, 0, labelBuffer.Length);
            LabelFile.Close();

            // Display label in Acrobat
            DisplayLabel(labelFileName);

            return strTempLabel;
        }

        private static void DisplayLabel(string labelFileName)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(labelFileName);
            info.UseShellExecute = true;
            info.Verb = "open";
            System.Diagnostics.Process.Start(info);
        }


        //WebService Url Setting
        private string GetWebserviceUrl(out string strApiUrl)
        {
            string retVal = string.Empty;
            strApiUrl = "";

            try
            {
                string strMode = string.Empty;
                string strUrl = string.Empty;

                System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
                strMode = (string)settingsReader.GetValue("mode", typeof(String));

                if (strMode.Equals(AppMode.LIVE))
                    strApiUrl = (string)settingsReader.GetValue(ApiUrl.LIVE, typeof(String));
                else
                    strApiUrl = (string)settingsReader.GetValue(ApiUrl.TEST, typeof(String));

                
            }
            catch (Exception ex)
            {
                retVal = "[GetWebserviceUrl Error] " + ex.Message;
            }

            return retVal;
        }

        private static string ConvertToUnicode(string value)
        {
            string result = "";

            if (!string.IsNullOrEmpty(value))
            {
                // Changed below
                //byte[] bytes = Encoding.Default.GetBytes(value);
                //result = Encoding.UTF8.GetString(bytes);
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                result = Encoding.UTF8.GetString(bytes);
            }                      

            return result;
        }


        public class AppMode
        {
            public static string TEST = "test";
            public static string LIVE = "production";
        }

        public class ApiUrl
        {
            public static string TEST = "Test_FedExShipUrl";
            public static string LIVE = "Live_FedExShipUrl";
        }


        #region serializeToSoap
        public static byte[] serializeToSoap(object graph)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                SoapFormatter formatter = new SoapFormatter();

                //BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, graph);
                //
                byte[] soapBytes = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(soapBytes, 0, Convert.ToInt32(stream.Length));
                //
                stream.Close();
                return soapBytes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return new byte[0];
        }
        #endregion

    }
}
