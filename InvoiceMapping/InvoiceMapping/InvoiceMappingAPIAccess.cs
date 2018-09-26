using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using InvoiceMapping;

namespace InvoiceMapping
{
    class InvoiceMappingAPIAccess
    {
        public string postInvoiceMappingData(Logger m_oLogger)
        {
            var strResponse = " ";
            DataTable dtResponse = new DataTable();
            try
            {

                InvoiceMappingDAL objInvoiceMappingDAL = new InvoiceMappingDAL();

                m_oLogger.LogMessage("getInvoiceMappingData", "Getting PO Receipt Mapping Data starts here");
                dtResponse = objInvoiceMappingDAL.getInvoiceMappingData(m_oLogger);
                if (dtResponse.Rows.Count != 0)
                {
                    List<InvoiceMappingBO> target = dtResponse.AsEnumerable()
                        .Select(row => new InvoiceMappingBO
                        {
                            XXPMC_SDI_RECORD_ID = ((Decimal)(row["ISA_IDENTIFIER"])).ToString(),
                            PO_NUMBER = ReplacePipe((String)(row["ISA_CUST_PO_ID"])),
                            RECEIPT_NUMBER = ReplacePipe((String)(row["RECEIVER_ID"])),
                            RECEIPT_LINE_NBR = ((Decimal)(row["RECV_LN_NBR"])).ToString(),
                            INVOICE_TYPE_LOOKUP_CODE = ReplacePipe((String)(row["INTFC_REC_TYPE"])),
                            INVOICE_LINE_TYPE_LOOKUP_CODE = ReplacePipe((String)(row["ISA_LINE_TYPE_ID"])),
                            VENDOR_NAME = ReplacePipe((String)(row["VENDOR_NAME"])),
                            VENDOR_SITE_CODE = ReplacePipe((String)(row["VNDR_LOC"])),
                            INVOICE_CURRENCY_CODE = ReplacePipe((String)(row["CURRENCY"])),
                            EXCHANGE_RATE = ((Decimal)(row["EXCHANGE_RATE"])).ToString(),
                            INVOICE_NUM = ReplacePipe((String)(row["INVOICE_ID"])),
                            INVOICE_DATE = ((DateTime)(row["INVOICE_DATE"])).ToString("yyyy/MM/dd"),
                            INVOICE_AMOUNT = ((Decimal)(row["INVOICED_AMT"])).ToString(),
                            INVOICE_QUANTITY = ((Decimal)(row["INVOICED_QTY"])).ToString(),
                            DESCRIPTION = ReplacePipe((String)(row["DESCR254"])),
                            GL_DATE = ((DateTime)(row["TRANSACTION_DT"])).ToString("yyyy/MM/dd"),
                            ACCTS_PAY_CODE_COMBINATION_ID = ReplacePipe((String)(row["PYMNT_MESSAGE_CD"])),
                            DIST_CODE_COMBINATION_ID = ReplacePipe((String)(row["DISTRIB_CODE"])),
                            CALC_TAX_DURING_IMPORT_FLAG = ReplacePipe((String)(row["TAX_FLAG"])),
                            ORGANIZATION_CODE = ReplacePipe((String)(row["PLANT"])),
                            SOURCE = ReplacePipe((String)(row["SOURCE_DOC"])),
                            ATTRIBUTE1 = ReplacePipe((String)(row["ISA_ATTRIBUTE_1"])),
                            ATTRIBUTE2 = ReplacePipe((String)(row["ISA_ATTRIBUTE_2"])),
                            ATTRIBUTE3 = ReplacePipe((String)(row["ISA_ATTRIBUTE_3"])),
                            ATTRIBUTE4 = ReplacePipe((String)(row["ISA_ATTRIBUTE_4"])),
                            ATTRIBUTE5 = ReplacePipe((String)(row["ISA_ATTRIBUTE_5"])),
                            ATTRIBUTE6 = ReplacePipe((String)(row["ISA_ATTRIBUTE_6"])),
                            ATTRIBUTE7 = ReplacePipe((String)(row["ISA_ATTRIBUTE_7"])),
                            ATTRIBUTE8 = ReplacePipe((String)(row["ISA_ATTRIBUTE_8"])),
                            ATTRIBUTE9 = ReplacePipe((String)(row["ISA_ATTRIBUTE_9"])),
                            ATTRIBUTE10 = ReplacePipe((String)(row["ISA_ATTRIBUTE_10"])),
                            TRANS_STATUS_DESCRIPTION = row["ISA_COMMENTS_1333"] == DBNull.Value ? null : (string)(row["ISA_COMMENTS_1333"]),
                            TRANSACTION_STATUS = ReplacePipe((string)(row["STATUS_MSG"])),
                        }).ToList();




                    string jsontest = JsonConvert.SerializeObject(new
                    {
                        _postporeceipt = target
                    });


                    StringBuilder sb = new StringBuilder();
                    sb.Append("{'_postporeceipt_batch_req':");
                    sb.Append(jsontest);
                    sb.Append("}");

                    JObject resultSet = JObject.Parse(sb.ToString());
              
                    using (var client = new WebClient())
                    {
                        m_oLogger.LogMessage("postInvoiceMappingData", "POST InvoiceMapping Data to PMC starts here");
                        m_oLogger.LogMessage("postInvoiceMappingData", "POST InvoiceMapping Data" + resultSet.ToString());
                        m_oLogger.LogMessage("postInvoiceMappingData", "POST InvoiceMapping Data URL : https://10.118.13.27:8243/SDIOutboundPOReceiptAPI/v1_0");

                        client.Headers.Add("Authorization: Basic YWRtaW46YWRtaW4=");
                        client.Headers.Add("Content-Type:application/json");
                        client.Headers.Add("Accept:application/json");
                        System.Net.ServicePointManager.CertificatePolicy = new AlwaysIgnoreCertPolicy();
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                        /// Need to change the URL after service avaialbe from client side
                        var result = client.UploadString("https://10.118.13.27:8243/SDIOutboundPOReceiptAPI/v1_0", resultSet.ToString());
                       

                        var parsed = JObject.Parse(result);
                        strResponse = parsed.SelectToken("REQUEST_STATUS").Value<string>();
                        m_oLogger.LogMessage("postInvoiceMappingData", "POST InvoiceMapping data to PMC server status " + strResponse);

                    }
                }

            }
            catch (Exception ex)
            {
                m_oLogger.LogMessage("postInvoiceMappingData", "Error trying to POST data to PMS server.", ex);
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
