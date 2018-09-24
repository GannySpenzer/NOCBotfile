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
        Logger m_oLogger;
        // public string postPOMappingData()
        public string postPOMappingData(Logger m_oLogger)
        {
            var strResponse = " ";
            DataTable dtResponse = new DataTable();
            try
            {
                string json = @"{
	'_postpurchaseorder_batch_req': {
		'_postpurchaseorder': [{
			'XXPMC_SDI_RECORD_ID': '100',
			'PROCESS_CODE': 'TEST1',
			'ACTION': 'TEST1',
			'ORG_ID': '100',
			'DOCUMENT_TYPE_CODE': 'TEST1',
			'CURRENCY_CODE': 'TEST1',
			'AGENT_ID': '100',
			'FULL_NAME': 'TEST1',
			'VENDOR_ID': '100',
			'VENDOR_SITE_ID': '100',
			'HEADER_SHIP_TO_LOCATION_ID': '100',
			'HEADER_BILL_TO_LOCATION_ID': '100',
			'APPROVAL_STATUS': 'TEST1',
			'FREIGHT_CARRIER': 'TEST1',
			'FOB': 'TEST1',
			'TERMS_ID': '100',
			'REFERENCE_NUM': 'TEST1',
			'LINE_NUM': '100',
			'SHIPMENT_NUM': '100',
			'LINE_TYPE_ID': '100',
			'ITEM': 'TEST1',
			'ITEM_DESCRIPTION': 'TEST1',
			'ITEM_ID': '100',
			'UOM_CODE': 'ABC',
			'QUANTITY': '100',
			'UNIT_PRICE': '100',
			'LINE_SHIP_TO_ORGANIZATION_CODE': 'ABC',
			'LINE_SHIP_TO_LOCATION_ID': '100',
			'LINE_LOC_POPULATED_FLAG': 'Y',
			'NEED_BY_DATE': '2018/07/03 21:02:44',
			'PROMISED_DATE': '2018/07/03 21:02:44',
			'LIST_PRICE_PER_UNIT': '100',
			'ACCRUE_ON_RECEIPT_FLAG': 'Y',
			'QUANTITY_ORDERED': '100',
			'DESTINATION_ORGANIZATION_ID': '100',
			'SET_OF_BOOKS_ID': '100',
			'CHARGE_ACCOUNT_ID': '100',
			'DISTRIBUTION_NUM': '100',
			'DESTINATION_TYPE_CODE': 'TEST1',
			'WIP_ENTITY_ID': '100',
			'WIP_OPERATION_SEQ_NUM': '100',
			'WIP_RESOURCE_SEQ_NUM': '100',
			'BOM_RESOURCE_ID': '100'
		}]
	}
}";

                POMappingDAL objPOMappingDAL = new POMappingDAL();
                postPoOrderReq objpostPoOrderReq = new postPoOrderReq();
                List<PostPoOrders> objPostPoOrders = new List<PostPoOrders>();
                m_oLogger.LogMessage("getPOMappingData", "Getting PO Mapping Data starts here");
                //dtResponse = objPOMappingDAL.getPOMappingData(m_oLogger);
                dtResponse = objPOMappingDAL.getPOMappingData(m_oLogger);
                if (dtResponse.Rows.Count != 0)
                {
                    List<PostPoOrdersProperties> target = dtResponse.AsEnumerable()
                        .Select(row => new PostPoOrdersProperties
                        {
                            XXPMC_SDI_RECORD_ID = ((Decimal)(row["ISA_IDENTIFIER"])).ToString(),
                            PROCESS_CODE = ReplacePipe((String)(row["ISA_PROCESS_CODE"])),
                            ACTION = ReplacePipe((String)(row["ISA_ACTION_CODE"])),
                            ORG_ID = ((Decimal)(row["ISA_ORGANIZ_ID"])).ToString(),
                            DOCUMENT_TYPE_CODE = ReplacePipe((String)(row["DOCUMENT"])),
                            CURRENCY_CODE = ReplacePipe((String)(row["CURRENCY_CODE"])),
                            AGENT_ID = ((Decimal)(row["ISA_EMPL_NUM"])).ToString(),
                            FULL_NAME = ReplacePipe((String)(row["BUYER_NAME"])),
                            VENDOR_ID = ((Decimal)(row["ISA_VENDOR_NUM"])).ToString(),
                            VENDOR_SITE_ID = ((Decimal)(row["ISA_VNDR_SITE_NUM"])).ToString(),
                            HEADER_SHIP_TO_LOCATION_ID = ((Decimal)(row["ISA_SHIP_LOCNUM"])).ToString(),
                            HEADER_BILL_TO_LOCATION_ID = ((Decimal)(row["ISA_BILL_LOCNUM"])).ToString(),
                            APPROVAL_STATUS = ReplacePipe((String)(row["ISA_APPROVAL_STAT"])),
                            FREIGHT_CARRIER = "",
                            FOB = "",
                            REFERENCE_NUM = ReplacePipe((String)(row["PO_ID"])),
                            LINE_NUM = ((Decimal)(row["LINE_NBR"])).ToString(),
                            SHIPMENT_NUM = "1",
                            LINE_TYPE_ID = "",
                            // ISA_ITEM = (String)(row["ISA_ITEM"]),
                            ITEM_DESCRIPTION = ReplacePipe((String)(row["DESCR254"])),
                            ITEM_ID = ((Decimal)(row["ITEM_FIELD_N15_A"])).ToString(),
                            UOM_CODE = ReplacePipe((String)(row["ISA_CUSTOMER_UOM"])),
                            QUANTITY = ((Decimal)(row["QTY_PO"])).ToString(),
                            UNIT_PRICE = ((Decimal)(row["PRICE_PO"])).ToString(),
                            LINE_SHIP_TO_ORGANIZATION_CODE = "BMM",
                            LINE_SHIP_TO_LOCATION_ID = ((Decimal)(row["ISA_LN_SHIPTO_NUM"])).ToString(),
                            LINE_LOC_POPULATED_FLAG = ReplacePipe((String)(row["ISA_LOC_POP_FLAG"])),
                            NEED_BY_DATE = ((DateTime)(row["ISA_REQUIRED_BY_DT"])).ToString("yyyy/MM/dd"),
                            PROMISED_DATE = ((DateTime)(row["EXPECTED_DATE"])).ToString("yyyy/MM/dd"),
                            ACCRUE_ON_RECEIPT_FLAG = ReplacePipe((String)(row["ACCRUE_THIS"])),
                            DESTINATION_ORGANIZATION_ID = ((Decimal)(row["ISA_DEST_ORG_ID"])).ToString(),
                            SET_OF_BOOKS_ID = "",
                            CHARGE_ACCOUNT_ID = ((Decimal)(row["ISA_ACCOUNT_NUM"])).ToString(),
                            DISTRIBUTION_NUM = ((Decimal)(row["ISA_DEST_ORG_ID"])).ToString(),
                            DESTINATION_TYPE_CODE = ReplacePipe((String)(row["ISA_DEST_TYPE_CODE"])),
                            WIP_ENTITY_ID = ReplacePipe((String)(row["ISA_WORK_ORDER"])),
                            WIP_OPERATION_SEQ_NUM = ReplacePipe((String)(row["ACTIVITY_ID"])),
                            // WIP_RESOURCE_SEQ_NUM = ((Decimal)(row["ISA_ACTIVITY_NBR"])).ToString(),
                            WIP_RESOURCE_SEQ_NUM = "",
                            ATTRIBUTE1 = ReplacePipe((String)(row["ISA_ATTRIBUTE_1"])),
                            ATTRIBUTE10 = ReplacePipe((String)(row["ISA_ATTRIBUTE_10"])),
                            ATTRIBUTE2 = ReplacePipe((String)(row["ISA_ATTRIBUTE_2"])),
                            ATTRIBUTE3 = ReplacePipe((String)(row["ISA_ATTRIBUTE_3"])),
                            ATTRIBUTE4 = ReplacePipe((String)(row["ISA_ATTRIBUTE_4"])),
                            ATTRIBUTE5 = ReplacePipe((String)(row["ISA_ATTRIBUTE_5"])),
                            ATTRIBUTE6 = ReplacePipe((String)(row["ISA_ATTRIBUTE_6"])),
                            ATTRIBUTE7 = ReplacePipe((String)(row["ISA_ATTRIBUTE_7"])),
                            ATTRIBUTE8 = ReplacePipe((String)(row["ISA_ATTRIBUTE_8"])),
                            ATTRIBUTE9 = ReplacePipe((String)(row["ISA_ATTRIBUTE_9"])),
                            //TRANS_STATUS_DESCRIPTION = (string)(row["ISA_COMMENTS_1333"]),
                            //TRANS_STATUS_DESCRIPTION = null,
                            TRANS_STATUS_DESCRIPTION = row["ISA_COMMENTS_1333"] == DBNull.Value ? null : (string)(row["ISA_COMMENTS_1333"]),
                            TRANSACTION_STATUS = ReplacePipe((string)(row["STATUS_MSG"])),
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
                    //JObject rss = JObject.Parse(json);
                    using (var client = new WebClient())
                    {
                        m_oLogger.LogMessage("postPOMappingData", "POST PO Mapping Data to PMC starts here");
                        client.Headers.Add("Authorization: Basic YWRtaW46YWRtaW4=");
                        client.Headers.Add("Content-Type:application/json");
                        client.Headers.Add("Accept:application/json");
                        System.Net.ServicePointManager.CertificatePolicy = new AlwaysIgnoreCertPolicy();
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        m_oLogger.LogMessage("postPOMappingData", "POST POMapping Data" + resultSet.ToString());
                        m_oLogger.LogMessage("postPOMappingData", "POST POMapping Data URL : https://10.118.13.27:8243/SDIOutboundPurchaseOrderAPI/v1_0");

                        var result = client.UploadString("https://10.118.13.27:8243/SDIOutboundPurchaseOrderAPI/v1_0", resultSet.ToString());
                        // var result1 = client.UploadString("https://10.118.13.27:8243/SDIOutboundPurchaseOrderAPI/v1_0", rss.ToString());
                        var parsed = JObject.Parse(result);
                        strResponse = parsed.SelectToken("REQUEST_STATUS").Value<string>();
                        m_oLogger.LogMessage("postPOMappingData", "POST POMapping data to PMC server status " + strResponse);
                    }
                }

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("postPOMappingData", "Error trying to POST data to PMS server.", ex);
            }
            return strResponse;
        }

        public string ReplacePipe(string x)
        {
            x = x.Trim();
            return x == "|" ? null : x;
        }

    }
}
