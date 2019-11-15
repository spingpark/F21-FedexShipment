using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using System.Web.Services.Protocols;
using F21.Framework;
using System.Globalization;
using System.Collections.Specialized;
using FedExWebServiceClient.CloseWebReference;


namespace F21.Service
{
    public class CloseShipmentClient
    {
        private const String LabelPath = "C:\\F21\\FedExClose\\";

        public CloseShipmentResponse CloseShipmentService(ShipServiceInfo clsService)
        {
            FedExWebServiceClient.CloseWebReference.CloseService service = new CloseService();
            F21.Service.CloseShipmentResponse closeResponse = new CloseShipmentResponse();

            string iResultCode = string.Empty;
            string iResultMessage = string.Empty;
            string iErrorMessage = string.Empty;
            string retValue = string.Empty;
            string serviceURL = string.Empty;
            string transactionId = string.Empty;

            string noticeCode = string.Empty;
            string noticeMessage = string.Empty;
            string noticeSeverity = string.Empty;
            string noticeSource = string.Empty;
            
            try
            {
                if (F21.Framework.ConfigManager.GetAppSetting2("mode").Equals("production", StringComparison.OrdinalIgnoreCase))
                {
                    serviceURL = F21.Framework.ConfigManager.GetAppSetting2("Live_FedExCloseUrl");
                }
                else
                {
                    serviceURL = F21.Framework.ConfigManager.GetAppSetting2("Test_FedExCloseUrl");
                }

                // Webservice URL
                service.Url = serviceURL;
                
                NameValueCollection nvcNotification = null;

                #region Ground Close
                GroundCloseWithDocumentsRequest request = CreateGroundCloseWithDocumentsRequest(clsService, out transactionId);
                closeResponse.TransactionId = Basic.IsNull(transactionId);
                //GroundCloseRequest request = CreateGroundCloseRequest();
                
                // [8/8/2016 jh.kim] Serialize To Soap 
                System.Diagnostics.Debug.WriteLine(Encoding.Default.GetString(Serialize.serializeToSoap(request)));

                // Call the Close web service passing in a GroundCloseWithDocumentsRequest and returning a GroundCloseDocumentsReply
                GroundCloseDocumentsReply reply = service.groundCloseWithDocuments(request);

                // Call ShowNotifications(reply)
                nvcNotification = ShowNotifications(reply, out iResultMessage);

                // SUCCESS, NOTE, WARNING
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    closeResponse.HighestSeverity = reply.HighestSeverity.ToString();

                    // Call ShowGroundCloseDocumentsReply(reply)
                    closeResponse = ShowGroundCloseDocumentsReply(reply, closeResponse);

                    if (nvcNotification["Code"].ToString() == "0000" && nvcNotification["Message"].ToString() == "Success")
                    {
                        closeResponse.isSuccess = true;
                        closeResponse.ErrorMessage = Basic.IsNull(closeResponse.ErrorMessage, "");
                    }
                    else
                    {
                        closeResponse.isSuccess = false;

                        if (closeResponse.ErrorMessage == "")
                        {
                            closeResponse.ErrorMessage = "[FedEx Close Service : " + closeResponse.HighestSeverity + "]\r\nNotification Code: " + nvcNotification["Code"].ToString() + ",\r\nNotification Message: " + nvcNotification["Message"].ToString();
                        }
                        else
                        {
                            closeResponse.ErrorMessage = "[FedEx Close Service : " + closeResponse.HighestSeverity + "]\r\nNotification Code: " + nvcNotification["Code"].ToString() + ",\r\nNotification Message: " + nvcNotification["Message"].ToString() + "\r\n" + closeResponse.ErrorMessage;
                        }
                    }
                }
                else // ERROR, FAILURE
                {
                    closeResponse.HighestSeverity = Basic.IsNull(reply.HighestSeverity.ToString(), "Empty");

                    closeResponse.isSuccess = false;
                    closeResponse.ErrorMessage = "[" + closeResponse.HighestSeverity + "] Notification Code: " + nvcNotification["Code"].ToString() + ",\r\nNotification Message: " + nvcNotification["Message"].ToString();
                }
                #endregion Ground Close


                foreach (string nvKey in nvcNotification.AllKeys)
                {
                    switch (nvKey)
                    {
                        case "Code":
                            closeResponse.NoticeCode = nvcNotification[nvKey].ToString();
                            break;
                        case "Message":
                            closeResponse.NoticeMessage = nvcNotification[nvKey].ToString();
                            break;
                        case "Severity":
                            closeResponse.NoticeSeverity = nvcNotification[nvKey].ToString();
                            break;
                        case "Source":
                            closeResponse.NoticeSource = nvcNotification[nvKey].ToString();
                            break;
                        default:
                            break;
                    }
                }                
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

            return closeResponse;
        }

        private static GroundCloseWithDocumentsRequest CreateGroundCloseWithDocumentsRequest(ShipServiceInfo clsService, out string transactionId)
        {
            // Build the CloseWithDocumentsRequest
            GroundCloseWithDocumentsRequest request = new GroundCloseWithDocumentsRequest();

            //Set WebAuthenticationCredential
            request.WebAuthenticationDetail = SetWebAuthenticationDetail(clsService.AccountKey, clsService.AccountPassword);

            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = clsService.AccountNumber;
            request.ClientDetail.MeterNumber = clsService.MeterNumber;

            request.TransactionDetail = new TransactionDetail();
            string customerTransactionId = "NWH_GroundClose_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            request.TransactionDetail.CustomerTransactionId = customerTransactionId;
            transactionId = customerTransactionId;      // out string

            request.Version = new VersionId();

            //2016.08.23 - Changed CloseDate (minkyu.r)
            //[ERROR] Notification Code: 1935,  Notification Message: Invalid date entered
            DateTime time_1 = DateTime.Now;
            string cutOfftime = DateTime.Now.ToShortDateString() + " 22:00";
            DateTime time_2 = DateTime.Parse(cutOfftime);
            int time = DateTime.Compare(time_1, time_2);

            if (time > 0)
            {
                request.CloseDate = DateTime.Now.AddHours(-2);
            }
            else
            {
                request.CloseDate = DateTime.Now;
            }
            //request.CloseDate = DateTime.Now;   //Convert.ToDateTime("2016-08-06"); 

            request.CloseDateSpecified = true;
            request.CloseDocumentSpecification = new CloseDocumentSpecification();
            //request.CloseDocumentSpecification.CloseDocumentTypes = new CloseDocumentType[1] { CloseDocumentType.OP_950 };
            //request.CloseDocumentSpecification.Op950Detail = new Op950Detail();
            //request.CloseDocumentSpecification.Op950Detail.Format = new CloseDocumentFormat();
            //request.CloseDocumentSpecification.Op950Detail.Format.ImageType = ShippingDocumentImageType.PDF;
            //request.CloseDocumentSpecification.Op950Detail.Format.ImageTypeSpecified = true;

            request.CloseDocumentSpecification.CloseDocumentTypes = new CloseDocumentType[1] { CloseDocumentType.MANIFEST };
            request.CloseDocumentSpecification.ManifestDetail = new ManifestDetail();
            request.CloseDocumentSpecification.ManifestDetail.Format = new CloseDocumentFormat();
            request.CloseDocumentSpecification.ManifestDetail.Format.ImageType = ShippingDocumentImageType.TEXT; //ShippingDocumentImageType.PDF;
            request.CloseDocumentSpecification.ManifestDetail.Format.ImageTypeSpecified = true;

            return request;
        }

        private static SmartPostCloseRequest CreateSmartPostCloseRequest(ShipServiceInfo clsService)
        {
            // Build the SmartPostCloseRequest
            SmartPostCloseRequest request = new SmartPostCloseRequest();
            
            request.WebAuthenticationDetail = SetWebAuthenticationDetail(clsService.AccountKey, clsService.AccountPassword);
            
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = clsService.AccountNumber;
            request.ClientDetail.MeterNumber = clsService.MeterNumber;
           
            request.TransactionDetail = new TransactionDetail();

            string customerTransactionId = "SmartPostClose_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            request.TransactionDetail.CustomerTransactionId = customerTransactionId;
            
            request.Version = new VersionId();

            request.HubId = clsService.HubId;
            
            // Request Jieun cci
            //request.CustomerManifestId = "XXX"; // Replace "XXX" with the CustomerManifestId

            request.DestinationCountryCode = "US"; // SmartPost is available in the US
            request.PickUpCarrier = CarrierCodeType.FXSP; // SmartPost uses FXSP carrier code type
            request.PickUpCarrierSpecified = true;
            
            return request;
        }

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

        private static CloseShipmentResponse ShowGroundCloseDocumentsReply(GroundCloseDocumentsReply reply, CloseShipmentResponse closeResponse)
        {
            string documentName = string.Empty;

            try
            {
                if (reply.CloseDocuments != null)
                {
                    System.Diagnostics.Debug.WriteLine("GroundCloseDocumentsReply details:");
                    int iterative = 0;

                    foreach (CloseDocument document in reply.CloseDocuments)
                    {
                        documentName = SaveDocument(document, iterative);
                        closeResponse.DocumentName = Basic.IsNull(documentName, ""); 
                    }
                }
                else
                {
                    closeResponse.ErrorMessage = "[ERROR] Ground CloseDocuments is null.";                    
                }
            }
            catch (Exception ex)
            {
                closeResponse.ErrorMessage = "[ERROR ShowGroundCloseDocumentsReply] " + ex.Message;
            }
            return closeResponse;
            
        }

        private static CloseShipmentResponse ShowSmartPostCloseReply(SmartPostCloseReply reply, CloseShipmentResponse shipResponse)
        {
            System.Diagnostics.Debug.WriteLine("SmartPost Close Reply details:");
            System.Diagnostics.Debug.WriteLine("SmartPost Close was processed.");

            return shipResponse;
        }

        private static void ShowGroundCloseReply(GroundCloseReply reply)
        {            
            if (reply.Manifest != null)
            {
                System.Diagnostics.Debug.WriteLine(reply.Manifest.FileName); 

                SaveManifest(reply.Manifest.FileName, reply.Manifest.File);
            }
        }

        private static void SaveManifest(String manifestFileName, byte[] manifestFile)
        {
            // Save manifest to file
            if (!Directory.Exists(LabelPath)) Directory.CreateDirectory(LabelPath);

            String LabelFileName = LabelPath + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + manifestFileName + ".txt";
            FileStream LabelFile = new FileStream(LabelFileName, FileMode.Create);
            LabelFile.Write(manifestFile, 0, manifestFile.Length);
            LabelFile.Close();
            DisplayManifest(LabelFileName);
        }

        private static string SaveDocument(CloseDocument document, int iterative)
        {
            String DocumentExtension = ".pdf";
            String DocumentIdentifier = "";
            String DocumentType = "";
            
            // Save manifest to file
            if (document.TypeSpecified)
            {
                DocumentType = document.Type.ToString();
                if (document.Type == CloseDocumentType.MANIFEST)
                {
                    DocumentExtension = ".txt"; // Note: pdf error
                    DocumentIdentifier = document.ShippingCycle;
                }
                else
                {
                    DocumentIdentifier = iterative.ToString();
                    iterative += 1;
                }
            }

            //[8/8/2016 jh.kim] Create Directory 
            if (!Directory.Exists(LabelPath)) Directory.CreateDirectory(LabelPath);

            String DocumentName = LabelPath + "GROUND_" + DocumentType + "_" + DateTime.Now.ToString("yyyyMMddHHmmss")+ "_" + DocumentIdentifier + DocumentExtension;
            FileStream DocumentFile = new FileStream(DocumentName, FileMode.Create);
            
            foreach (ShippingDocumentPart part in document.Parts)
            {
                DocumentFile.Write(part.Image, 0, part.Image.Length);
            }

            DocumentFile.Close();
            DisplayManifest(DocumentName);

            return DocumentName;
        }

        private static void DisplayManifest(string DocumentName)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(DocumentName.ToString());
            info.UseShellExecute = true;
            info.Verb = "open";
            
            System.Diagnostics.Process.Start(info);
        }

        // FedEx_Ground / Smart_Post
        private static NameValueCollection ShowNotifications(GroundCloseDocumentsReply reply, out string resultMessage)
        {
            NameValueCollection nvNotification = new NameValueCollection();            
            resultMessage = string.Empty;

            try
            {
                if (reply.Notifications[0] != null)
                {
                    Notification notification = reply.Notifications[0];

                    nvNotification.Add("Code", Basic.IsNull(notification.Code, ""));
                    nvNotification.Add("Message", Basic.IsNull(notification.Message, ""));
                    nvNotification.Add("Severity", Basic.IsNull(notification.Severity, ""));    //ERROR,FAILURE,NOTE,SUCCESS,WARNING
                    nvNotification.Add("Source", Basic.IsNull(notification.Source, ""));

                }
                else
                {
                    resultMessage = "[Error Notifications] reply.Notifications[0] is null.";
                }
            }
            catch (Exception ex)
            {
                resultMessage = "[Error ShowNotifications] " + ex.Message;
            }

            return nvNotification;
        }

        // Smart_Post
        private static NameValueCollection ShowNotifications(SmartPostCloseReply reply, out string resultMessage)
        {
            NameValueCollection nvNotification = new NameValueCollection();
            resultMessage = string.Empty;

            try
            {
                if (reply.Notifications[0] != null)
                {
                    Notification notification = reply.Notifications[0];

                    nvNotification.Add("Code", Basic.IsNull(notification.Code, ""));
                    nvNotification.Add("Message", Basic.IsNull(notification.Message, ""));
                    nvNotification.Add("Severity", Basic.IsNull(notification.Severity, ""));    //ERROR,FAILURE,NOTE,SUCCESS,WARNING
                    nvNotification.Add("Source", Basic.IsNull(notification.Source, ""));

                }
                else
                {
                    resultMessage = "[Notifications] reply.Notifications[0] is null.";
                }
            }
            catch (Exception ex)
            {
                resultMessage = "[Error ShowNotifications] " + ex.Message;
            }

            return nvNotification;
        }
    }
}