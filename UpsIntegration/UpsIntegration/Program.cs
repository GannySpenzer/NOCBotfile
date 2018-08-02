using Spire.Xls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Data.OleDb;
using System.Configuration;

namespace UpsIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            DataTable csvData = new DataTable();
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = "C:\\Logs\\";
            logFilePath = logFilePath + "SyncLog-" + string.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}",DateTime.Now) + "." + "txt";
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine("Started the Sync Operation");

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(@"C:\Users\avacorp\Desktop\QVD_ALT_sdiinc_20180309_060103_419_SDIQVD.csv"))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        csvData.Columns.Add(datecolumn);
                    }
                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }              
                OleDbConnection cn = new OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User Id=sdiexchange;Data Source=PLGR");
                OleDbCommand com = new OleDbCommand();
                string str;
                string PType = "UPS";
                cn.Open();
                int insertCoutn=0, UpdateCount=0;
                        for (int index = 0; index < csvData.Rows.Count - 1; index++)
                        {
                            string TrackingNumber;
                            str = " SELECT * FROM SDIX_TRACKING_DETAILS WHERE  TRACKING_NO = '" + csvData.Rows[index]["TrackingNumber"].ToString() + "' ";
                            com = new OleDbCommand(str, cn);
                            TrackingNumber= Convert.ToString(com.ExecuteScalar());
                            try
                            {
                                if (TrackingNumber.Length != 0)
                                {
                                    str = @"UPDATE SDIX_TRACKING_DETAILS SET PROVIDER_TYPE='" + PType + "', SUBSCRIBER_ID = '" + csvData.Rows[index]["SubscriberID"].ToString() + "',SUBSCRIPTION_NAME= '" + csvData.Rows[index]["SubscriptionName"].ToString() + "',SUBSCRIPTION_NO= '" + csvData.Rows[index]["SubscriptionNumber"].ToString() +
                                                   "' , SUBSCRIPTION_FILE_NAME= '" + csvData.Rows[index]["SubscriptionFileName"].ToString() + "', RECORD_TYPE = '" + csvData.Rows[index]["RecordType"].ToString() + "' ,SHIPPING_NO= '" + csvData.Rows[index]["ShipperNumber"].ToString() + "' WHERE TRACKING_NO ='" + TrackingNumber + "' ";
                                    com = new OleDbCommand(str, cn);
                                    com.ExecuteNonQuery();
                                    UpdateCount = UpdateCount + 1;
                                }
                                else
                                {
                                    str = @"Insert Into SDIX_TRACKING_DETAILS(TRACKING_NO,PROVIDER_TYPE,SUBSCRIBER_ID,SUBSCRIPTION_NAME,SUBSCRIPTION_NO,SUBSCRIPTION_FILE_NAME,RECORD_TYPE,SHIPPING_NO) Values('" + csvData.Rows[index]["TrackingNumber"].ToString() + "', '" + PType + "','" + csvData.Rows[index]["SubscriberID"].ToString() + "','" + csvData.Rows[index]["SubscriptionName"].ToString() +
                                                                                                "','" + csvData.Rows[index]["SubscriptionNumber"].ToString() + "','" + csvData.Rows[index]["SubscriptionFileName"].ToString() + "','" + csvData.Rows[index]["RecordType"].ToString() + "','" + csvData.Rows[index]["ShipperNumber"].ToString() + "')";
                                    com = new OleDbCommand(str, cn);
                                    com.ExecuteNonQuery();
                                    insertCoutn = insertCoutn + 1;

                                }
                            }
                            catch (Exception ex)
                            {
                                log.WriteLine("Error while Synch {0}", ex); 
                                throw ex;
                            }
                          
                         
                        }
                        log.WriteLine("{0} Total number of Records", csvData.Rows.Count);
                        log.WriteLine("{0} number of lines inserted newly into SDIX_TRACKING_DETAILS", insertCoutn );
                        log.WriteLine("{0} number of lines Updated into SDIX_TRACKING_DETAILS", UpdateCount);
                        cn.Close();
                    }            
            catch (System.IndexOutOfRangeException e)  // CS0168
            {
                log.WriteLine("Error while Synch {0}", e);               
                System.Console.WriteLine(e.Message);
                throw new System.ArgumentOutOfRangeException("index parameter is out of range.", e);
            }

            log.Close();
          
        }      

    }
}
