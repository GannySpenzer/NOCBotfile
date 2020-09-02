using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Data;
using CreditCardBillingProcess;

namespace AbbviePOChangeOut
{
    public class AbbviePOProcess
    {
        public static string strSOAPXML = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:req=""http://www.abbvie.com/schemas/mnf/private/v1/Request.xsd"">
                                    <soapenv:Header/>
                                    <soapenv:Body>
                                    <req:PurchaseOrderChangeNotificationReq>
                                    <req:Payload><![CDATA[	<ns0:PurchaseOrder xmlns:ns0=""http://sap.com/xi/EBP"">
                                    {0}{1}
	                                </ns0:PurchaseOrder>  ]]>
	                                </req:Payload>
                                    </req:PurchaseOrderChangeNotificationReq>
                                    </soapenv:Body>
                                 </soapenv:Envelope>";

        public static string strOrderHeaderXML = @"<PurchaseOrderHeader>
				<OrderDate>{0}</OrderDate>
				<PlanningSystemID></PlanningSystemID>
				<OrderIDPlanningSystemAssigned>{1}</OrderIDPlanningSystemAssigned>
				<Description></Description>
			</PurchaseOrderHeader>";

        public static string strOrderLineXML = @"<PurchaseOrderItem>
				<OrderItemIDPlanningSystemAssigned>{0}</OrderItemIDPlanningSystemAssigned>
				<Method></Method>
				<DeliveryDate>{1}</DeliveryDate>
				<StartDate></StartDate>
				<EndDate></EndDate>
				<TradingPartners>
					<Vendor>
						<Identifier>
							<PartnerID></PartnerID>
						</Identifier>
					</Vendor>
					<Requestor>
						<Identifier>
							<PartnerID></PartnerID>
						</Identifier>
					</Requestor>
					<ShipToParty>
						<Identifier>
							<PartnerID></PartnerID>
						</Identifier>
					</ShipToParty>
				</TradingPartners>
				<Quantity>
					<Value>{2}</Value>
					<UoM>{3}</UoM>
				</Quantity>
				<Price>
					<Value>{4}</Value>
					<Currency>{5}</Currency>
					<PriceBasisQuantity>{6}</PriceBasisQuantity>
				</Price>
				<Product>
					<Type>Material</Type>
					<Description>{7}</Description>
				</Product>
				<Category>
					<CategoryID></CategoryID>
				</Category>
				<InternalText>
					<Content></Content>
				</InternalText>
				<UserDefinedExtension>
					<Name></Name>
					<Value></Value>
				</UserDefinedExtension>
				<AccountAssignment>
					<DistributionPercentage></DistributionPercentage>
					<CostObject>
						<GLAccount></GLAccount>
						<CostCenter></CostCenter>
						<OrderNumber/>
						<ProfitCenter></ProfitCenter>
						<WorkBreakdownStructure/>
						<UserDefinedExtension>
							<Name></Name>
							<Value></Value>
						</UserDefinedExtension>
					</CostObject>
				</AccountAssignment>
			</PurchaseOrderItem>";
        static void Main(string[] args)
        {
            StreamWriter objStreamWriter;
            string rootDir = "";
            rootDir = ConfigurationManager.AppSettings["LogPath"];
            string logpath = rootDir + "AbbviePOChangeOut" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.GetHashCode() + ".txt";

            StreamWriter log;
            FileStream fileStream;
            DirectoryInfo logDirInfo;
            FileInfo logFileInfo;
            logFileInfo = new FileInfo(logpath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);

            if (!logDirInfo.Exists)
            {
                logDirInfo.Create();
            }

            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logpath, FileMode.Append);
            }

            log = new StreamWriter(fileStream);

            log.WriteLine("*********************Logs(" + String.Format(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")) + ")***********************************");

            log = AbbvieSDIProcess(log);

            log.WriteLine("********************End of Abbvie PO Change Out Process********************");

            log.Close();
        }

        public static StreamWriter AbbvieSDIProcess(StreamWriter log)
        {
            Boolean errorSomeWhere = false;
            StreamWriter returnLog;
            OleDbConnection connectOR = new OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings["OLEDBconString"]));
            string AbbvieApiURL = ConfigurationManager.AppSettings["AbbvieApiURL"];
            string ApiBasicAuth = ConfigurationManager.AppSettings["ApiBasicAuth"];

            string strSQLQuery = "";

            log.WriteLine("------------------------------------------------------------------------------------------");
            try
            {
                log = processSDiPO(log);
            }
            catch (Exception ex)
            {

                log.WriteLine("Error in accessing the service. Error : " + ex.Message);
            }
            return log;
        }

        public static StreamWriter processSDiPO(StreamWriter log)
        {
            OrderBO objOrderBO = new OrderBO();
            ItemBO objItemBO = new ItemBO();

           
            string AbbvieApiURL = ConfigurationManager.AppSettings["AbbvieApiURL"];
            string ApiBasicAuth = ConfigurationManager.AppSettings["ApiBasicAuth"];
            List<string> orderDetails = new List<string>();
            string strSQLQuery = "";
            try
            {
                strSQLQuery = "SELECT * FROM SYSADM8.PS_ISA_AB_POCHGOUT WHERE PROCESS_FLAG = 'N' AND DATE_PROCESSED IS NULL ORDER BY PO_ID, LINE_NBR";
                DataSet dsPO = ORDBData.GetAdapter(strSQLQuery);

                if (dsPO.Tables != null && dsPO.Tables.Count > 0)
                {
                    if (dsPO.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow rows in dsPO.Tables[0].Rows)
                        {
                            StringBuilder strSOAPBind = new StringBuilder();
                            StringBuilder strOrderBind = new StringBuilder();
                            StringBuilder strLineBind = new StringBuilder();
                            objOrderBO.PO_ID = Convert.ToString(rows["PO_ID"]).Trim();
                            objOrderBO.PO_Line_no = Convert.ToString(rows["LINE_NBR"]).Trim();
                            objOrderBO.OrderDate_0 = Convert.ToString(rows["TRANSACTION_DATE"]).Trim();
                            objOrderBO.OrderID_1 = Convert.ToString(rows["ISA_CUST_PO_NBR"]).Trim();
                            log.WriteLine("------------------------------------------------------------------------------------------");
                            log.WriteLine("PO - " + Convert.ToString(objOrderBO.PO_ID));

                            strOrderBind.AppendFormat(strOrderHeaderXML, objOrderBO.OrderDate_0, objOrderBO.OrderID_1);
                            if (!orderDetails.Contains(Convert.ToString(objOrderBO.PO_ID)))
                            {
                                orderDetails.Add(Convert.ToString(objOrderBO.PO_ID));

                                //To get the PO items from datatable
                                DataTable dt = dsPO.Tables[0].AsEnumerable().Where(myRow => myRow.Field<string>("PO_ID") == Convert.ToString(objOrderBO.PO_ID)).CopyToDataTable();
                                //Foreach
                                string itemItemPOList = string.Empty;
                                foreach (DataRow datarow in dt.Rows)
                                {
                                    objItemBO.OrderItemID_0 = Convert.ToString(datarow["ISA_SAP_PO_LN"]).Trim();
                                    objItemBO.DeliveryDate_1 = Convert.ToDateTime(datarow["DUE_DT"]).ToShortDateString().Trim();
                                    objItemBO.QuantityValue_2 = Convert.ToDecimal(datarow["QTY_PO"]).ToString("0.000").Trim();
                                    objItemBO.QuantityUoM_3 = Convert.ToString(datarow["ISA_CUSTOMER_UOM"]).Trim();
                                    objItemBO.PriceValue_4 = Convert.ToDecimal(datarow["PRICE_PO"]).ToString("0.00").Trim();
                                    objItemBO.PriceCurrency_5 = Convert.ToString(datarow["CURRENCY_CD"]).Trim();
                                    objItemBO.PricePriceBasisQuantity_6 = "1";
                                    objItemBO.ProductDescription_7 = Convert.ToString(datarow["DESCR254"]).Trim();
                                    strLineBind.AppendFormat(strOrderLineXML, objItemBO.OrderItemID_0, objItemBO.DeliveryDate_1, objItemBO.QuantityValue_2, objItemBO.QuantityUoM_3,
                                    objItemBO.PriceValue_4, objItemBO.PriceCurrency_5, objItemBO.PricePriceBasisQuantity_6, objItemBO.ProductDescription_7);
                                    itemItemPOList += strLineBind.ToString();
                                }

                                strSOAPBind.AppendFormat(strSOAPXML, strOrderBind, itemItemPOList);
                                string final = strSOAPBind.ToString();
                                XmlDocument soapEnvelopeXml = CreateSoapEnvelope(final);
                                HttpWebRequest webRequest = CreateWebRequest(AbbvieApiURL, ApiBasicAuth);
                                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);
                                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
                                asyncResult.AsyncWaitHandle.WaitOne();

                                // get the response from the completed web request.
                                string soapResult;
                                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                                {
                                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                                    {
                                        soapResult = rd.ReadToEnd();
                                    }

                                    Console.Write(soapResult);
                                    if (soapResult.Contains("ProcessOrderChangeNotification payload data processed successfully"))
                                    {
                                        log.WriteLine("PO - " + Convert.ToString(objOrderBO.PO_ID) + "Success in Response");
                                        strSQLQuery = "Update SYSADM8.PS_ISA_AB_POCHGOUT set PROCESS_FLAG = 'Y', DATE_PROCESSED =SYSDATE WHERE PO_ID = '" + Convert.ToString(objOrderBO.PO_ID) + "'";
                                        int rowaffected = ORDBData.ExecNonQuery(strSQLQuery);
                                        if (rowaffected == 1)
                                        {
                                            log.WriteLine("PO - " + Convert.ToString(objOrderBO.PO_ID) + "Updated Successfully");
                                        }
                                        else
                                        {
                                            log.WriteLine("PO - " + Convert.ToString(objOrderBO.PO_ID) + "Updated Failed");
                                        }
                                    }
                                    else
                                    {
                                        log.WriteLine("PO - " + Convert.ToString(objOrderBO.PO_ID) + "Response Failed");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("Issue in processSDiPO method" + ex.Message);
            }
            return log;
        }

        private static HttpWebRequest CreateWebRequest(string url, string authValue)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", "Basic " + authValue);
            return webRequest;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }

        private static XmlDocument CreateSoapEnvelope(string final)
        {
            try
            {
                XmlDocument soapEnvelopeDocument = new XmlDocument();
                soapEnvelopeDocument.LoadXml(final);
                return soapEnvelopeDocument;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }

    public class ItemBO
    {
        public string OrderItemID_0 { get; set; }
        public string DeliveryDate_1 { get; set; }
        public string QuantityValue_2 { get; set; }
        public string QuantityUoM_3 { get; set; }
        public string PriceValue_4 { get; set; }
        public string PriceCurrency_5 { get; set; }
        public string PricePriceBasisQuantity_6 { get; set; }
        public string ProductDescription_7 { get; set; }
    }
    public class OrderBO
    {
        //Variables for order purpose
        public string PO_ID { get; set; }
        public string PO_Line_no { get; set; }

        // xml Object need to used based on index
        public string OrderDate_0 { get; set; }
        public string OrderID_1 { get; set; }

    }
}
