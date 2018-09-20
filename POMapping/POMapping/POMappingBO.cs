using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POMapping
{
    class POMappingBO
    {


    }

    public class postPoOrderReq
    {
        public PostPoOrders _postpurchaseorder_batch_req;
    }

    public class PostPoOrders
    {
        public List<PostPoOrdersProperties> _postpurchaseorder;
    }

    public class Result
    {
        public string REQUEST_STATUS { get; set; }
    }

    public class PostPoOrdersProperties
    {
        public string XXPMC_SDI_RECORD_ID { get; set; }
        public string PROCESS_CODE { get; set; }
        public string ACTION { get; set; }
        public string ORG_ID { get; set; }
        public string DOCUMENT_TYPE_CODE { get; set; }
        public string CURRENCY_CODE { get; set; }
        public string AGENT_ID { get; set; }
        public string FULL_NAME { get; set; }
        public string VENDOR_ID { get; set; }
        public string VENDOR_SITE_ID { get; set; }
        public string HEADER_SHIP_TO_LOCATION_ID { get; set; }
        public string HEADER_BILL_TO_LOCATION_ID { get; set; }
        public string APPROVAL_STATUS { get; set; }
        public string FREIGHT_CARRIER { get; set; }
        public string FOB { get; set; }
        public string TERMS_ID { get; set; }
        public string REFERENCE_NUM { get; set; }
        public string LINE_NUM { get; set; }
        public string SHIPMENT_NUM { get; set; }
        public string LINE_TYPE_ID { get; set; }
        public string ITEM { get; set; }
        public string ITEM_DESCRIPTION { get; set; }
        public string ITEM_ID { get; set; }
        public string UOM_CODE { get; set; }
        public string QUANTITY { get; set; }
        public string UNIT_PRICE { get; set; }
        public string LINE_SHIP_TO_ORGANIZATION_CODE { get; set; }
        public string LINE_SHIP_TO_LOCATION_ID { get; set; }
        public string LINE_LOC_POPULATED_FLAG { get; set; }
        public string NEED_BY_DATE { get; set; }
        public string PROMISED_DATE { get; set; }
        public string LIST_PRICE_PER_UNIT { get; set; }
        public string ACCRUE_ON_RECEIPT_FLAG { get; set; }
        public string QUANTITY_ORDERED { get; set; }
        public string DESTINATION_ORGANIZATION_ID { get; set; }
        public string SET_OF_BOOKS_ID { get; set; }
        public string CHARGE_ACCOUNT_ID { get; set; }
        public string DISTRIBUTION_NUM { get; set; }
        public string DESTINATION_TYPE_CODE { get; set; }
        public string WIP_ENTITY_ID { get; set; }
        public string WIP_OPERATION_SEQ_NUM { get; set; }
        public string WIP_RESOURCE_SEQ_NUM { get; set; }
        public string ATTRIBUTE1 { get; set; }
        public string ATTRIBUTE10 { get; set; }         
        public string ATTRIBUTE2 { get; set; }        
        public string ATTRIBUTE3 { get; set; }        
        public string ATTRIBUTE4 { get; set; }        
        public string ATTRIBUTE5 { get; set; }        
        public string ATTRIBUTE6 { get; set; }        
        public string ATTRIBUTE7 { get; set; }        
        public string ATTRIBUTE8 { get; set; }        
        public string ATTRIBUTE9 { get; set; }
        public string TRANS_STATUS_DESCRIPTION { get; set; }
        public string TRANSACTION_STATUS { get; set; }
        public string BOM_RESOURCE_ID { get; set; }

    }
}
