using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace F21.Service
{
    public class AddressServiceResponse
    {
        public bool isSuccess { get; set; }        
        public string ServiceType { get; set; }

        public string HighestSeverity { get; set; }
        public string ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string ClientReferenceId { get; set; }
        public string Classification { get; set; }
        public string OperationalAddressStateType { get; set; }
        
        private string[] streetLinesField;
        public string City { get; set; }
        public string StateOrProvinceCode { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }

        private string[] attribute;        

        //private string[] addressesToValidateField;
        

        public string[] CustomerStreetLines
        {
            get
            {
                return this.streetLinesField;
            }
            set
            {
                this.streetLinesField = value;
            }
        }

        public string[] Attribute
        {
            get 
            {
                return this.attribute;
            }
            set 
            {
                this.attribute = value;

            }
        }




        //public string[] AddressesToValidates
        //{
        //    get
        //    {
        //        return this.addressesToValidateField;
        //    }
        //    set
        //    {
        //        this.addressesToValidateField = value;
        //    }
        //}
    }    
}
