using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceMapping
{
    class InvoiceMappingBO
    {
        public string XXPMC_SDI_RECORD_ID { get; set; }
        public string PO_NUMBER { get; set; }
        public string RECEIPT_NUMBER { get; set; }
        public string RECEIPT_LINE_NBR { get; set; }
        public string INVOICE_TYPE_LOOKUP_CODE { get; set; }
        public string INVOICE_LINE_TYPE_LOOKUP_CODE { get; set; }
        public string VENDOR_NAME { get; set; }
        public string VENDOR_SITE_CODE { get; set; }
        public string INVOICE_CURRENCY_CODE { get; set; }
        public string EXCHANGE_RATE { get; set; }
        public string INVOICE_NUM { get; set; }
        public string INVOICE_DATE { get; set; }
        public string INVOICE_AMOUNT { get; set; }
        public string INVOICE_QUANTITY { get; set; }
        public string DESCRIPTION { get; set; }
        public string GL_DATE { get; set; }
        public string ACCTS_PAY_CODE_COMBINATION_ID { get; set; }
        public string DIST_CODE_COMBINATION_ID { get; set; }
        public string CALC_TAX_DURING_IMPORT_FLAG { get; set; }       
        public string ORGANIZATION_CODE { get; set; }
        public string SOURCE { get; set; }
        public string ATTRIBUTE1 { get; set; }    
        public string ATTRIBUTE2 { get; set; }
        public string ATTRIBUTE3 { get; set; }
        public string ATTRIBUTE4 { get; set; }
        public string ATTRIBUTE5 { get; set; }
        public string ATTRIBUTE6 { get; set; }
        public string ATTRIBUTE7 { get; set; }
        public string ATTRIBUTE8 { get; set; }
        public string ATTRIBUTE9 { get; set; }
        public string ATTRIBUTE10 { get; set; }
        public string TRANS_STATUS_DESCRIPTION { get; set; }
        public string TRANSACTION_STATUS { get; set; }
    }
}
