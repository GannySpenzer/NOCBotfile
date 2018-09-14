using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace POMapping
{
    class POMappingAPIAccess
    {
        /// <summary>
        /// POST Purchase order data to the PMC service
        /// </summary>
        /// <returns></returns>
        public string postPOMappingData()
        {
            var strResponse = " ";
            DataTable dtResponse = new DataTable();
            try
            {          
                POMappingDAL objPOMappingDAL = new POMappingDAL();
                postPoOrderReq objpostPoOrderReq = new postPoOrderReq();
                List<PostPoOrders> objPostPoOrders = new List<PostPoOrders>();
                dtResponse = objPOMappingDAL.getPOMappingData();
                if (dtResponse.Rows.Count != 0)
                {
                    List<PostPoOrdersProperties> target = dtResponse.AsEnumerable()
                        .Select(row => new PostPoOrdersProperties
                        {
                            XXPMC_SDI_RECORD_ID = ((Decimal)(row["ISA_IDENTIFIER"])).ToString(),
                            PROCESS_CODE = "",
                            ACTION = "",
                            ORG_ID = ((Decimal)(row["ISA_ORGANIZ_ID"])).ToString(),
                            DOCUMENT_TYPE_CODE = (String)(row["DOCUMENT"]),
                            CURRENCY_CODE = (String)(row["CURRENCY_CODE"]),
                            AGENT_ID = ((Decimal)(row["ISA_EMPL_NUM"])).ToString(),
                            FULL_NAME = (String)(row["BUYER_NAME"]),
                            VENDOR_ID = ((Decimal)(row["ISA_VENDOR_NUM"])).ToString(),
                            VENDOR_SITE_ID = ((Decimal)(row["ISA_VNDR_SITE_NUM"])).ToString(),
                            HEADER_SHIP_TO_LOCATION_ID = ((Decimal)(row["ISA_SHIP_LOCNUM"])).ToString(),
                            HEADER_BILL_TO_LOCATION_ID = ((Decimal)(row["ISA_BILL_LOCNUM"])).ToString(),
                            APPROVAL_STATUS = (String)(row["BUYER_NAME"]),
                            FREIGHT_CARRIER = "",
                            FOB = "",
                            REFERENCE_NUM = (String)(row["PO_ID"]),
                            LINE_NUM = ((Decimal)(row["ISA_VNDR_SITE_NUM"])).ToString(),
                            SHIPMENT_NUM = "1",
                            LINE_TYPE_ID = "",
                            // ISA_ITEM = (String)(row["ISA_ITEM"]),
                            ITEM_DESCRIPTION = (String)(row["DESCR254"]),
                            ITEM_ID = ((Decimal)(row["ITEM_FIELD_N15_A"])).ToString(),
                            UOM_CODE = (String)(row["ISA_CUSTOMER_UOM"]),
                            QUANTITY = ((Decimal)(row["QTY_PO"])).ToString(),
                            UNIT_PRICE = ((Decimal)(row["PRICE_PO"])).ToString(),
                            LINE_SHIP_TO_ORGANIZATION_CODE = "BMM",
                            LINE_SHIP_TO_LOCATION_ID = ((Decimal)(row["ISA_LN_SHIPTO_NUM"])).ToString(),
                            LINE_LOC_POPULATED_FLAG = (String)(row["ISA_LOC_POP_FLAG"]),
                            NEED_BY_DATE = ((DateTime)(row["ISA_REQUIRED_BY_DT"])).ToString("yyyy/MM/dd"),
                            PROMISED_DATE = ((DateTime)(row["EXPECTED_DATE"])).ToString("yyyy/MM/dd"),
                            ACCRUE_ON_RECEIPT_FLAG = (String)(row["ACCRUE_THIS"]),
                            DESTINATION_ORGANIZATION_ID = ((Decimal)(row["ISA_DEST_ORG_ID"])).ToString(),
                            SET_OF_BOOKS_ID = "",
                            CHARGE_ACCOUNT_ID = ((Decimal)(row["ISA_ACCOUNT_NUM"])).ToString(),
                            DISTRIBUTION_NUM = ((Decimal)(row["ISA_DEST_ORG_ID"])).ToString(),
                            DESTINATION_TYPE_CODE = (String)(row["ISA_DEST_TYPE_CODE"]),
                            WIP_ENTITY_ID = (String)(row["ISA_WORK_ORDER"]),
                            WIP_OPERATION_SEQ_NUM = (String)(row["ACTIVITY_ID"]),
                            //WIP_RESOURCE_SEQ_NUM = ((Decimal)(row["ISA_ACTIVITY_NBR"])).ToString(),
                            BOM_RESOURCE_ID = ((Decimal)(row["ISA_BOM_RESOURCE"])).ToString(),
                            TERMS_ID = "",
                            LIST_PRICE_PER_UNIT = "",
                            QUANTITY_ORDERED = ((Decimal)(row["CURRENT_QTY_ORD"])).ToString()

                        }).ToList();


                    
                    string jsontest = JsonConvert.SerializeObject(new
                     {
                         _postpurchaseorder = target
                     });
                   


                    StringBuilder sb = new StringBuilder();
                   
                    sb.Append("{'_postpurchaseorder_batch_req':");
                    sb.Append(jsontest);
                    sb.Append("}");
                  
                    JObject resultSet = JObject.Parse(sb.ToString());
                   
                    using (var client = new WebClient())
                    {
                        client.Headers.Add("Authorization: Basic YWRtaW46YWRtaW4=");
                        client.Headers.Add("Content-Type:application/json");
                        client.Headers.Add("Accept:application/json");
                        System.Net.ServicePointManager.CertificatePolicy = new AlwaysIgnoreCertPolicy();
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        var result = client.UploadString("https://10.118.13.27:8243/SDIOutboundPurchaseOrderAPI/v1_0 ", resultSet.ToString());

                        var parsed = JObject.Parse(result);
                        strResponse = parsed.SelectToken("REQUEST_STATUS").Value<string>();

                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
            return strResponse;
        }
    }
}
