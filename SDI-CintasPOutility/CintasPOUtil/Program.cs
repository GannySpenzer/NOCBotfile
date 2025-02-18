using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Net.Http;
using System.IO;

namespace CintasPOUtil
{
    class Program
    {
        static StreamWriter log;
        static StreamWriter xmllog;
        static string currentpoid = "";
        static HttpClient client = new HttpClient();
        static Boolean IsProd;
        static FileStream fileStream;
        public static string DbUrl
        {
            get
            {
                if (ConfigurationSettings.AppSettings["CURRENTDB"] == ConfigurationSettings.AppSettings["PRDDB"])
                    return ConfigurationSettings.AppSettings["OLEDBFSPRDconString"];
                else
                    return ConfigurationSettings.AppSettings["OLEDBDEVLconString"];
            }
        }
        
        static void Main(string[] args)
        {
           
            try
            {

                IsProd = ConfigurationSettings.AppSettings["CURRENTDB"] == ConfigurationSettings.AppSettings["PRDDB"];
                StreamWriter objStreamWriter;
                string rootDir = "";
                rootDir = ConfigurationSettings.AppSettings["LogPath"];
                rootDir += "\\";
                string logpath = rootDir + "CintasPOUtillogs\\" + DateTime.Now.Year+"\\" + DateTime.Now.Month.ToString()+"\\"+ DateTime.Now.Day+ "\\" +"CintasPOUtil"+DateTime.Now.GetHashCode() + ".txt";

                
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

                string querystring = "select distinct(PO_ID) from SYSADM8.ps_isa_po_disp_xml";
                DataSet result = new DataSet();
                result = GetFromDB(querystring);

                foreach (DataRow row in result.Tables[0].Rows)
                {
                    currentpoid = row["PO_ID"] as string;
                    CreateXML(Convert.ToString(row["PO_ID"]));
                }
            }
            catch (Exception ex)
            {
                log = new StreamWriter(fileStream);
                log.WriteLineAsync("Exception occurred in MAIN " + ex.ToString());
                log.WriteLine("*******END OF UPDATE********");
            }
            finally
            {
                log = new StreamWriter(fileStream);
                log.WriteLine("*********************END OF UPDATE*********************");
                log.Close();
            }
        }

        public static string GenPayloadID()
        {

            return Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds) + "." + Convert.ToString(new Random().Next(100000, 999999)) + "@sdi.com";
        }

        public static DataSet GetFromDB(string querystring)
        {
            try
            {
                OleDbConnection connection = new OleDbConnection(DbUrl);
                connection.Open();
                OleDbCommand Command = new OleDbCommand(querystring, connection);
                Command.CommandTimeout = 120;
                OleDbDataAdapter adapter = new OleDbDataAdapter(Command);
                DataSet result = new DataSet();
                adapter.Fill(result);
                return result;
            }
            catch (Exception ex)
            {
                log.WriteLine($"Exception n GetFromDB method // CurrentPOID {currentpoid} " + ex.ToString());
                log.WriteLine("******* *******");
                return new DataSet();
            }

        }

        public async static void CreateXML(string poid)
        {
            try
            {

                string querystring = "SELECT * FROM ps_isa_po_disp_xml X , PS_PO_LINE_DISTRIB D,PS_ISA_ORD_INTF_LN L WHERE x.po_id=D.PO_ID AND X.line_nbr = D.line_nbr AND D.req_id = l.order_no AND x.line_nbr = l.isa_intfc_ln AND x.po_id='" + poid + "' AND L.vendor_id='W000000154'";

                DataSet result = new DataSet();
                result = GetFromDB(querystring);
                double total = 0;

                // to find the total value of the order 
                foreach (DataRow row in result.Tables[0].Rows)
                {
                    total = total + Convert.ToDouble(row["PRICE_PO"]);
                }

                var row1 = result.Tables[0].Rows[0];

                //get user details from the table for the user 
                DataSet user = new DataSet();
                user = GetFromDB("Select * from sdix_users_tbl where ISA_EMPLOYEE_ID='" + row1["OPRID_ENTERED_BY"] + "'");
                var userrow = user.Tables[0].Rows[0];

                //this part is for header. To be included once the header data is finalised
                var cxmlnode = new XElement("cXML", new XAttribute("version", "1.2.014"), new XAttribute("payloadID", GenPayloadID()), new XAttribute("timestamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")));
                var headernode = new XElement("Header");
                var fromnode = new XElement("From", new XElement("Credential", new XAttribute("domain", "NetworkId"), new XElement("Identity", "CORP_WLM")));
                var tonode = new XElement("To", new XElement("Credential", new XAttribute("domain", "NetworkId"), new XElement("Identity", "CINTAS")));
                var sendernode = new XElement("Sender", new XElement("Credential", new XAttribute("domain", "NetworkId"), new XElement("Identity", "ProcurementSystem"), new XElement("SharedSecret", "WLM88**")));
                var useragentnode = new XElement("UserAgent", userrow["ISA_EMPLOYEE_ID"]);
                sendernode.Add(useragentnode);
                headernode.Add(fromnode);
                headernode.Add(tonode);
                headernode.Add(sendernode);

                cxmlnode.Add(headernode);

                var requestnode = new XElement("Request", new XAttribute("deploymentMode", IsProd ? "production" : "test"));
                var orderreqnode = new XElement("OrderRequest");
                var ordreqheadnode = new XElement("OrderRequestHeader", new XAttribute("orderID", row1["PO_ID"]), new XAttribute("orderDate", DateTime.Parse(Convert.ToString(row1["PO_DT"])).ToString("yyyy-MM-ddTHH:mm:sszzz")), new XAttribute("orderType", "regular"), new XAttribute("type", "new"));
                var totalnode = new XElement("Total", new XElement("Money", new XAttribute("currency", "USD"), Math.Round(total, 2)));
                ordreqheadnode.Add(totalnode);


                //building the complete billto node
                var billtonode = new XElement("BillTo");
                var billaddressnode = new XElement("Address", new XAttribute("isoCountryCode", row1["COUNTRY_2CHAR"]), new XAttribute("addressID", row1["BILLTO_ID"]), new XElement("Name", row1["BILLTO_NAME"]));
                var billpostalnode = new XElement("PostalAddress");
                billpostalnode.Add(new XElement("DeliverTo", row1["BILLTO_ADD1"]));
                billpostalnode.Add(new XElement("Street", row1["BILLTO_ADD2"]));
                billpostalnode.Add(new XElement("City", row1["BILLTO_CITY"]));
                billpostalnode.Add(new XElement("State", row1["BILLTO_STATE"]));
                billpostalnode.Add(new XElement("PostalCode", row1["BILLTO_POSTAL"]));
                billpostalnode.Add(new XElement("Country", new XAttribute("isoCountryCode", row1["COUNTRY_2CHAR"]), row1["COUNTRY_2CHAR"]));
                billaddressnode.Add(billpostalnode);
                billaddressnode.Add(new XElement("Email", userrow["ISA_EMPLOYEE_EMAIL"]));
                var billphonenode = new XElement("Phone");
                var billtelepnonenode = new XElement("TelephoneNumber");
                billtelepnonenode.Add(new XElement("CountryCode", new XAttribute("isoCountryCode", row1["COUNTRY_2CHAR"])));
                billtelepnonenode.Add(new XElement("AreaOrCityCode"));
                billtelepnonenode.Add(new XElement("Number", "" + row1["BILLTO_PHONE2"] + "-" + row1["BILLTO_PHONE3"]));
                billtelepnonenode.Add(new XElement("Extension", row1["BILLTO_PHONE1"]));
                billphonenode.Add(billtelepnonenode);
                billaddressnode.Add(billphonenode);
                billaddressnode.Add(new XElement("URL"));
                billtonode.Add(billaddressnode);


                //building the complete shipto node
                var shiptonode = new XElement("ShipTo");
                var addressnode = new XElement("Address", new XAttribute("isoCountryCode", row1["COUNTRY_2CHAR"]), new XAttribute("addressID", row1["SHIPTO_ID"]), new XElement("Name", row1["ADDRESS1"]));
                var postalnode = new XElement("PostalAddress");
                postalnode.Add(new XElement("DeliverTo", userrow["ISA_USER_NAME"]));
                postalnode.Add(new XElement("Street", row1["ADDRESS2"]));
                postalnode.Add(new XElement("City", row1["CITY"]));
                postalnode.Add(new XElement("State", row1["STATE"]));
                postalnode.Add(new XElement("PostalCode", row1["POSTAL"]));
                postalnode.Add(new XElement("Country", new XAttribute("isoCountryCode", row1["COUNTRY_2CHAR"]), row1["COUNTRY_2CHAR"]));
                addressnode.Add(postalnode);
                addressnode.Add(new XElement("Email", userrow["ISA_EMPLOYEE_EMAIL"]));
                var phonenode = new XElement("Phone");
                var telepnonenode = new XElement("TelephoneNumber");
                telepnonenode.Add(new XElement("CountryCode", new XAttribute("isoCountryCode", row1["COUNTRY_2CHAR"])));
                telepnonenode.Add(new XElement("AreaOrCityCode"));
                telepnonenode.Add(new XElement("Number", row1["PHONE"]));
                //telepnonenode.Add(new XElement("Extension")); not sure about this one
                phonenode.Add(telepnonenode);
                addressnode.Add(phonenode);
                addressnode.Add(new XElement("URL"));
                shiptonode.Add(addressnode);


                //building the complete contact node
                var contactnode = new XElement("Contact", new XAttribute("role", "buyer"));

                //var conaddressnode = new XElement("Address", new XAttribute("addressID", ""), new XElement("Name", userrow["ISA_USER_NAME"]));
                var conpostalnode = new XElement("PostalAddress");
                conpostalnode.Add(new XElement("DeliverTo", row1["ADDRESS1"]));
                conpostalnode.Add(new XElement("Street", row1["ADDRESS2"]));
                conpostalnode.Add(new XElement("City", row1["CITY"]));
                conpostalnode.Add(new XElement("State", row1["STATE"]));
                conpostalnode.Add(new XElement("PostalCode", row1["POSTAL"]));
                conpostalnode.Add(new XElement("Country", new XAttribute("isoCountryCode", row1["COUNTRY_2CHAR"]), row1["COUNTRY_2CHAR"]));
                //conaddressnode.Add(postalnode);
                contactnode.Add(new XElement("Name", userrow["ISA_USER_NAME"]));

                var conphonenode = new XElement("Phone");
                var contelepnonenode = new XElement("TelephoneNumber");
                contelepnonenode.Add(new XElement("CountryCode", new XAttribute("isoCountryCode", row1["COUNTRY_2CHAR"])));
                contelepnonenode.Add(new XElement("AreaOrCityCode"));
                contelepnonenode.Add(new XElement("Number", userrow["PHONE_NUM"]));
                //telepnonenode.Add(new XElement("Extension")); not sure about this one
                conphonenode.Add(contelepnonenode);
                contactnode.Add(conpostalnode);
                contactnode.Add(new XElement("Email", userrow["ISA_EMPLOYEE_EMAIL"]));
                //contactnode.Add(conaddressnode);//address node added from shipto addrress node but email alone replaced witht teh user email id
                contactnode.Add(conphonenode);



                ordreqheadnode.Add(shiptonode);
                ordreqheadnode.Add(billtonode);
                ordreqheadnode.Add(contactnode);
                orderreqnode.Add(ordreqheadnode);

                foreach (DataRow row in result.Tables[0].Rows)
                {

                    var itemoutnode = new XElement("ItemOut", new XAttribute("quantity", Convert.ToInt16(row["QTY_PO"])), new XAttribute("agreementItemNumber", row["ITM_ID_VNDR"]), new XAttribute("requestedDeliveryDate",string.IsNullOrWhiteSpace(Convert.ToString(row1["ISA_REQUIRED_BY_DT"]))?" ":DateTime.Parse(Convert.ToString(row1["ISA_REQUIRED_BY_DT"])).ToString("yyyy-MM-ddTHH:mm:sszzz")));

                    var itemidnode = new XElement("ItemID");
                    itemidnode.Add(new XElement("SupplierPartID", row["ITM_ID_VNDR"]));
                    itemidnode.Add(new XElement("SupplierPartAuxiliaryID", row["ISA_USER5"]));
                    itemoutnode.Add(itemidnode);

                    var itemdetailnode = new XElement("ItemDetail");
                    //have to add unti price node in the itmedetial before this
                    itemdetailnode.Add(new XElement("PO_LINE_NUMBER", row["LINE_NBR"]));
                    itemdetailnode.Add(new XElement("UnitPrice", new XElement("Money", new XAttribute("currency", "USD"), Math.Round(Convert.ToDouble(row["PRICE_PO"]), 2))));
                    itemdetailnode.Add(new XElement("Description", row["DESCR254_MIXED"]));
                    itemdetailnode.Add(new XElement("UnitOfMeasure", row["UNIT_OF_MEASURE"]));
                    itemdetailnode.Add(new XElement("Classification"));
                    itemdetailnode.Add(new XElement("ManufacturerPartID", row["MFG_ITM_ID"]));
                    itemdetailnode.Add(new XElement("ManufacturerName", row["ISA_MFG_FREEFORM"]));

                    itemoutnode.Add(itemdetailnode);

                    orderreqnode.Add(itemoutnode);
                }

                requestnode.Add(orderreqnode);
                cxmlnode.Add(requestnode);
                var str = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\n" + cxmlnode.ToString();
                log.WriteLine("*********************POID - (" + poid + ")***********************************");
                log.WriteLine("");
                log.WriteLine(str);
                log.WriteLine("*****************************END OF XML*************************************");
                var taskResult = DispatchPO(str, poid);
                Console.WriteLine("d");
                string rootDir = "";
                rootDir = ConfigurationSettings.AppSettings["LogPath"];
                rootDir += "\\";
                string logpath = rootDir + poid + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.GetHashCode() + ".txt";
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


            }
            catch (Exception ex)
            {
                log.WriteLine($"Exception in CreateXML // Current POID{currentpoid} " + ex.ToString());
                log.WriteLine("******* *******");
            }
        }

        public static string DispatchPO(string xml, string poid)
        {
            try
            {
                string url = "";
                if (IsProd)
                    url = "https://shop.mycintas.com/punchout/cxml/order";
                else
                    url = "https://shoppre.mycintas.com/punchout/cxml/order";
                var response = client.PostAsync(url, new StringContent(xml, Encoding.UTF8, "application/xml")).Result;
                string querystring = "update PS_PO_DISPATCHED set EIP_CTL_ID ='" + response.StatusCode.ToString() + "' where PO_ID='" + poid + "'";
                OleDbConnection connection = new OleDbConnection(DbUrl);
                connection.Open();
                OleDbTransaction transaction = connection.BeginTransaction();
                OleDbCommand Command = new OleDbCommand(querystring, connection);
                Command.Transaction = transaction;
                Command.CommandTimeout = 120;
                Command.ExecuteNonQuery();
                transaction.Commit();
                if (response.IsSuccessStatusCode)
                {                    
                    var body = response.Content.ReadAsStringAsync().Result;
                    if (body.ToLower().Trim().Contains("200") && body.ToLower().Trim().Contains("ok"))
                    {
                        log.WriteLine("success " + currentpoid);
                        log.WriteLine("******* *******");
                    }
                    else
                    {
                        Console.WriteLine("Fail " + currentpoid);
                        log.WriteLine($"Not contains 200****Exception in DispatchPO POST probably failed // Current POID {currentpoid} " + body.ToLower());
                        log.WriteLine("******* *******");
                        return "fail";
                    }
                }
                else
                {
                    Console.WriteLine("Fail " + currentpoid);
                    log.WriteLine($"Unsuccessful response****Exception in DispatchPO POST probably failed // Current POID {currentpoid} " + response.Content.ReadAsStringAsync().Result);
                    log.WriteLine("******* *******");
                    return "fail";
                }
                return "over";
            }
            catch (Exception ex)
            {
                log.WriteLine($"Exception in DispatchPO // Current POID {currentpoid} " + ex.ToString());
                log.WriteLine("******* *******");
                return "exception";
            }
        }


    }
}
