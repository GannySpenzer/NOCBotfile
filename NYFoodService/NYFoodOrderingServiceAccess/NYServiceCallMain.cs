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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;

namespace NYFoodOrderingServiceAccess
{  

    public class NYServiceCallMain
    {
        public static void Main(string[] args)
        {   

            StreamWriter objStreamWriter;
            string rootDir = "";
            rootDir = ConfigurationManager.AppSettings["LogPath"];
            string logpath = rootDir + "NYFoodOrderingServiceAccess" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.GetHashCode() + ".txt";

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

            log = NYServiceProcess(log);

            log.WriteLine("********************End of NY Food Service Process********************");

            log.Close();

        }

        public static StreamWriter NYServiceProcess(StreamWriter log)
        {
            Boolean errorSomeWhere = false;
            StreamWriter returnLog;
            OleDbConnection connectOR = new OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings["OLEDBconString"]));
            string strVendor = ConfigurationManager.AppSettings["VendorID"];
            string Pass = ConfigurationManager.AppSettings["ApiPassword"];
            Double DateLimit = Convert.ToDouble(ConfigurationManager.AppSettings["EndDateLimit"]);
            DateTime dttm_current = DateTime.Now;
            string date_from = "";
            string date_to = "";
            string mainurl = "";
            XmlDocument xmlResquest = new XmlDocument();
            XmlNode xmlNode;
            string boro = "";
            string strSQLQuery = "";

            string order_id = "";
            string school_id = "";
            string school_name = "";
            string school_address = "";
            string special_instruction = "";
            string delivery_date = "";
            string item_key = "";
            string item_name = "";
            string item_unit = "";
            string ordered_quantity = "";
            Boolean results = false;
            string error_messag = "";

            //date_from = dttm_current.AddDays(-10).ToString("yyyy-MM-dd");
            date_from = dttm_current.ToString("yyyy-MM-dd");
            date_to = dttm_current.AddDays(DateLimit).ToString("yyyy-MM-dd");

            log.WriteLine("------------------------------------------------------------------------------------------");


            //string strStartDate = Convert.ToString(DateTime.Now);
            //string EndDate = ConfigurationManager.AppSettings["EndDateLimit"];


            try
            {


                date_from = "02/01/2020";
                date_to = "12/12/2020";
                SFOrdering.SFWebService objSFOrdering = new SFOrdering.SFWebService();
                log.WriteLine("Calling the SFWebService");
                xmlNode = objSFOrdering.GetOrdersDateRangeXML(strVendor, Pass, date_from, date_to, boro);

                using (XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(xmlNode.OuterXml)))
                {
                    log.WriteLine("Reading the XML Incomming Start");
                    while (reader.Read())
                    {

                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "error_message")
                        {

                            error_messag = reader.ReadString();

                        }
                        else if (reader.NodeType == XmlNodeType.Element && reader.Name == "elements")
                        {
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "order_id")
                                {
                                    order_id = reader.ReadString(); break;
                                }
                            }
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "school_id")
                                {
                                    school_id = reader.ReadString(); break;
                                }
                            }
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "school_name")
                                {
                                    school_name = reader.ReadString(); break;
                                }
                            }
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "school_address")
                                {
                                    school_address = reader.ReadString(); break;
                                }

                            }
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "special_instruction")
                                {
                                    special_instruction = reader.ReadString(); break;
                                }
                            }
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.Name == "delivery_date")
                                {
                                    delivery_date = reader.ReadString(); break;
                                }
                            }

                        }
                        else if (reader.NodeType == XmlNodeType.Element && reader.Name == "details")
                        {

                            item_key = reader.GetAttribute("item_key");
                            item_name = reader.GetAttribute("item_name");
                            item_unit = reader.GetAttribute("item_unit");
                            ordered_quantity = reader.GetAttribute("ordered_quantity");

                        }
                        else
                        {

                            //log.WriteLine(" ");

                        }

                        if (error_messag.Trim() == "" && order_id.Trim() != "" && school_id.Trim() != "" && school_name != "" && special_instruction != ""
                            && delivery_date != "" && item_key != "" && item_name != "" && item_unit != "" && ordered_quantity != "")
                        {

                            DateTime oDate = Convert.ToDateTime(delivery_date);
                            school_name = school_name.Replace("'", "");
                            special_instruction = special_instruction.Replace("'", "");
                            item_name = item_name.Replace("'", "");

                            string strOrderNo = "";
                            string ProcessFlag = "N";
                            try
                            {
                                strSQLQuery = "SELECT ORDER_NO FROM SYSADM8.PS_ISA_NYFS_REQ_IN WHERE ORDER_NO= '" + order_id + "' AND ISA_SCHOOL_CODE= '" + school_id + "' AND ISA_ITEM = '" + item_key + "' AND QTY_REQ = '" + ordered_quantity + "'";

                                strOrderNo = GetScalar(strSQLQuery);

                                if (strOrderNo.Trim() != "")
                                {
                                    ProcessFlag = "B";
                                }
                                else
                                {
                                    ProcessFlag = "N";
                                }
                            }
                            catch (Exception ex)
                            {
                                ProcessFlag = "N";
                            }


                            strSQLQuery = "INSERT INTO SYSADM8.PS_ISA_NYFS_REQ_IN VALUES(SYSDATE,'" + order_id + "',' ', '" + school_id + "','" + school_name + "','" + school_address + "'" + System.Environment.NewLine +
                           ",'" + special_instruction + "',TO_DATE('" + oDate.ToString("MM-dd-yyyy hh:mm:ss") + "', 'MM-dd-yyyy hh:mi:ss'),'" + item_key + "',' ','" + item_name + "','" + item_unit + "',' ','" + ordered_quantity + "','" + ProcessFlag + "')";

                            results = InsertTbl(strSQLQuery);
                            if (results)
                            {
                                log.WriteLine("Transaction details inserted succesfully for Order: " + order_id + " Item :" + item_key + " Process Flag : " + ProcessFlag);
                                log.WriteLine(" ");
                                item_key = ""; item_name = ""; item_unit = ""; ordered_quantity = "";
                            }
                            else
                            {
                                log.WriteLine("Error in inserting the transaction details. Query :" + strSQLQuery);
                                log.WriteLine(" ");
                            }
                        }
                        else
                        {
                            if (error_messag.Trim() != "")
                            {
                                log.WriteLine("Error in reading XML Node. Error : " + error_messag);break;
                            }

                        }

                    }

                }

            }
            catch (Exception ex)
            {

                log.WriteLine("Error in accessing the service. Error : " + ex.Message);
            }
            return log;
        }


       
      
        //ORDB Operations Starts here
        private static Boolean InsertTbl(string insertquery)
        {
            Boolean reslt = false;
            try
            {
                int rowsaffected;
                string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                OleDbConnection cn = new OleDbConnection(connectionString);
                OleDbCommand com = new OleDbCommand();
                cn.Open();
                string strInsertHDRQuery = insertquery;

                com = new OleDbCommand(strInsertHDRQuery, cn);
                rowsaffected = com.ExecuteNonQuery();

                cn.Close();
                cn.Dispose();
                if (rowsaffected == 1)
                {
                    reslt = true;
                }
                else
                {
                    reslt = false;
                }
                return reslt;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string GetScalar(string p_strQuery, bool bGoToErrPage = true)
        {
            // Gives us a reference to the current asp.net 
            // application executing the method.
            //HttpApplication currentApp = HttpContext.Current.ApplicationInstance;
            string strReturn = "";
            OleDbConnection connection = new OleDbConnection(ConfigurationManager.ConnectionStrings["ConString"].ConnectionString);
            try
            {
                OleDbCommand Command = new OleDbCommand(p_strQuery, connection);
                Command.CommandTimeout = 120;
                connection.Open();
                try
                {
                    strReturn = System.Convert.ToString(Command.ExecuteScalar());
                }
                catch (Exception ex32)
                {
                    strReturn = "";
                }
                if (strReturn == null)
                    strReturn = "";
                try
                {
                    Command.Dispose();
                }
                catch (Exception ex1)
                {
                }
                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex2)
                {
                }
            }
            // connection.close()
            catch (Exception objException)
            {
                strReturn = "";

                try
                {
                    connection.Close();
                    connection.Dispose();
                }
                catch (Exception ex)
                {

                    // connection.close()

                }
            }

            return strReturn;
        }



    }









}
