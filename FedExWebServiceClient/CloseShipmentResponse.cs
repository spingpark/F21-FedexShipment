using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace F21.Service
{
    public class CloseShipmentResponse
    {
        public bool isSuccess { get; set; }

        public string HighestSeverity { get; set; }

        public string NoticeCode { get; set; }
        public string NoticeMessage { get; set; }
        public string NoticeSeverity { get; set; }
        public string NoticeSource { get; set; }
        public string ErrorMessage { get; set; }

        public string TransactionId { get; set; }

        public string DocumentName { get; set; }

    }
}
