using System;
using System.Collections.Generic;
using System.Text;

using System.Web.Services.Protocols;
using FedExWebServiceClient.AddressValidationServiceWebReference;

namespace F21.Service
{
    public class AddressValidationServiceClient
    {
        public AddressServiceResponse ValidationServiceClient(ShipServiceInfo clsService, AddressInfo clsAddress)
        {
            string ClassificationType = string.Empty;
            string serviceURL = string.Empty;
                        
            AddressValidationService service = new AddressValidationService();
            F21.Service.AddressServiceResponse fedExResponse = new F21.Service.AddressServiceResponse();
                                    
            try
            {
                if (F21.Framework.ConfigManager.GetAppSetting2("mode").Equals("production", StringComparison.OrdinalIgnoreCase))
                {
                    serviceURL = F21.Framework.ConfigManager.GetAppSetting2("Live_FedExAddressUrl");
                }
                else
                {
                    serviceURL = F21.Framework.ConfigManager.GetAppSetting2("Test_FedExAddressUrl");
                }               


                if(string.IsNullOrEmpty(serviceURL))
                {
                    fedExResponse.isSuccess = false;
                    fedExResponse.ErrorMessage = "Address validation service url is empty.";

                    return fedExResponse;
                }

                AddressValidationRequest request = CreateAddressValidationRequest(clsService, clsAddress);

                // Test URL : https://wsbeta.fedex.com:443/web-services/addressvalidation
                service.Url = serviceURL;

                AddressValidationReply reply = service.addressValidation(request);
                                
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    //FedExAddressClassificationType Enum
                    fedExResponse = ShowAddressValidationReply(reply);

                    fedExResponse.HighestSeverity = reply.HighestSeverity.ToString();
                    fedExResponse.isSuccess = true;
                    fedExResponse.ErrorMessage = "";                    

                }
                else
                {
                    fedExResponse.HighestSeverity = reply.HighestSeverity == null ? "" : reply.HighestSeverity.ToString();
                    fedExResponse.isSuccess = false;

                    foreach (Notification notification in reply.Notifications)
                    {
                        System.Diagnostics.Debug.WriteLine(notification.Message);
                        fedExResponse.ErrorMessage = notification.Message;
                                               
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

            return fedExResponse;
        }

        private static AddressValidationRequest CreateAddressValidationRequest(ShipServiceInfo clsService, AddressInfo clsAddress)
        {
            // Build the AddressValidationRequest
            AddressValidationRequest request = new AddressValidationRequest();
            
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = clsService.AccountKey;
            request.WebAuthenticationDetail.UserCredential.Password = clsService.AccountPassword;
            //request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
            //request.WebAuthenticationDetail.ParentCredential.Key = clsService.AccountKey;
            //request.WebAuthenticationDetail.ParentCredential.Password = clsService.AccountPassword;
            
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = clsService.AccountNumber;
            request.ClientDetail.MeterNumber = clsService.MeterNumber;
                                    
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = clsService.OrderNumber; // The client will get the same value back in the reply

            // Version             
            request.Version = new VersionId(); // Creates the Version element with all child elements populated
            
            request.InEffectAsOfTimestamp = DateTime.Now;
            request.InEffectAsOfTimestampSpecified = true;

            SetAddress(request, clsAddress, clsService.OrderNumber);
            
            return request;

        }
        
        private static void SetAddress(AddressValidationRequest request, AddressInfo clsAddress, String OrderNumber)
        {
            request.AddressesToValidate = new AddressToValidate[1];
            request.AddressesToValidate[0] = new AddressToValidate();
            request.AddressesToValidate[0].ClientReferenceId = OrderNumber;
            request.AddressesToValidate[0].Address = new Address();

            if (string.IsNullOrEmpty(clsAddress.Line2))
            {
                request.AddressesToValidate[0].Address.StreetLines = new String[1] { RemoveDiacritics.RemoveAccent(clsAddress.Line1) };
            }
            else
            {
                request.AddressesToValidate[0].Address.StreetLines = new String[2] { RemoveDiacritics.RemoveAccent(clsAddress.Line1), RemoveDiacritics.RemoveAccent(clsAddress.Line2) };
            }
                        
            request.AddressesToValidate[0].Address.PostalCode = clsAddress.PostalCode;
            request.AddressesToValidate[0].Address.City = RemoveDiacritics.RemoveAccent(clsAddress.City);
            request.AddressesToValidate[0].Address.StateOrProvinceCode = clsAddress.State; 
            request.AddressesToValidate[0].Address.CountryCode = clsAddress.CountryCode; 

            #region  ###Test Address###
            /*
            request.AddressesToValidate = new AddressToValidate[2];
            request.AddressesToValidate[0] = new AddressToValidate();
            request.AddressesToValidate[0].ClientReferenceId = "ClientReferenceId1";
            request.AddressesToValidate[0].Address = new Address();
            request.AddressesToValidate[0].Address.StreetLines = new String[1] { "100 Nickerson RD" };
            request.AddressesToValidate[0].Address.PostalCode = "01752";
            request.AddressesToValidate[0].Address.City = "Marlborough";
            request.AddressesToValidate[0].Address.StateOrProvinceCode = "MA";
            request.AddressesToValidate[0].Address.CountryCode = "US";
            //
            request.AddressesToValidate[1] = new AddressToValidate();
            request.AddressesToValidate[1].ClientReferenceId = "ClientReferenceId2";
            request.AddressesToValidate[1].Address = new Address();
            request.AddressesToValidate[1].Address.StreetLines = new String[2] { "400 S WESTMORELAND AVE", "APT 413" };
            request.AddressesToValidate[1].Address.PostalCode = "90020";
            request.AddressesToValidate[1].Address.City = "LOS ANGELES";
            request.AddressesToValidate[1].Address.CountryCode = "US";

            //request.AddressesToValidate[2] = new AddressToValidate();
            //request.AddressesToValidate[2].ClientReferenceId = "ClientReferenceId3";
            //request.AddressesToValidate[2].Address = new Address();
            //request.AddressesToValidate[2].Address.StreetLines = new String[2] { "3 WATCHMOOR POINT", "WATCHMOOR ROAD" };
            //request.AddressesToValidate[2].Address.PostalCode = "GU153AQ";
            //request.AddressesToValidate[2].Address.City = "CAMBERLEY";
            //request.AddressesToValidate[2].Address.CountryCode = "GB";
            */
            #endregion
        }

        private static AddressServiceResponse ShowAddressValidationReply(AddressValidationReply reply)
        {
            F21.Service.AddressServiceResponse addressResponse = new F21.Service.AddressServiceResponse();

            try
            {
                // AddressValidationReply details:                        
                foreach (AddressValidationResult result in reply.AddressResults)
                {
                    // FedExAddressClassificationType : BUSINESS, MIXED, RESIDENTIAL, UNKNOWN
                    // System.Diagnostics.Debug.WriteLine("Address Id : " + result.ClientReferenceId);

                    addressResponse.ClientReferenceId = result.ClientReferenceId;

                    if (result.ClassificationSpecified)
                    {
                        //System.Diagnostics.Debug.WriteLine("Classification: " + result.Classification);
                        addressResponse.Classification = result.Classification.ToString();
                    }


                    if (result.StateSpecified)
                    {
                        //System.Diagnostics.Debug.WriteLine("State: " + result.State);                 
                        addressResponse.OperationalAddressStateType = result.State.ToString();
                    }

                    //System.Diagnostics.Debug.WriteLine("Proposed Address--");

                    Address address = result.EffectiveAddress;


                    foreach (String street in address.StreetLines)
                    {
                        System.Diagnostics.Debug.WriteLine("   Street: " + street);
                    }

                    if (address.StreetLines != null)
                    {
                        addressResponse.CustomerStreetLines = new string[address.StreetLines.Length];
                        
                        for (int i = 0; i < address.StreetLines.Length; i++)
                        {
                            addressResponse.CustomerStreetLines[i] = address.StreetLines[i];
                        }
                    }
                                        
                    System.Diagnostics.Debug.WriteLine("     City: " + address.City);                    
                    System.Diagnostics.Debug.WriteLine("    ST/PR: " + address.StateOrProvinceCode);
                    System.Diagnostics.Debug.WriteLine("   Postal: " + address.PostalCode);
                    System.Diagnostics.Debug.WriteLine("  Country: " + address.CountryCode);
                    System.Diagnostics.Debug.WriteLine("");

                    addressResponse.City = address.City;
                    addressResponse.StateOrProvinceCode = address.StateOrProvinceCode;
                    addressResponse.PostalCode = address.PostalCode;
                    addressResponse.CountryCode = address.CountryCode;

                    System.Diagnostics.Debug.WriteLine("Address Attributes:");
                    foreach (AddressAttribute attribute in result.Attributes)
                    {
                        // Check Attributes
                        System.Diagnostics.Debug.WriteLine("  " + attribute.Name + ": " + attribute.Value);
                    }
                                        
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return addressResponse;
        }

        
        private static bool usePropertyFile() //Set to true for common properties to be set with getProperty function.
        {
            return getProperty("usefile").Equals("True");
        }
        
        
        private static String getProperty(String propertyname) //Sets common properties for testing purposes.
        {
            try
            {
                String filename = "C:\\F21\\FedExAPI\\filename.txt";
                if (System.IO.File.Exists(filename))
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(filename);
                    do
                    {
                        String[] parts = sr.ReadLine().Split(',');
                        if (parts[0].Equals(propertyname) && parts.Length == 2)
                        {
                            return parts[1];
                        }
                    }
                    while (!sr.EndOfStream);
                }
                Console.WriteLine("Property {0} set to default 'XXX'", propertyname);
                return "XXX";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Property {0} set to default 'XXX'", propertyname);
                return "XXX";
            }
        }
    }
}
