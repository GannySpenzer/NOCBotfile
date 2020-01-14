using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates; //possibly necessary for upsftp conn

using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
 
namespace UpsIntegration
{
    class QuantumUtility
    {
         /** QuantumUtility
         *  Quick access utility for FTP and error logging
         *   
          **/

        /*
         * Log Error (Exception)
         * Handles exception errors. Currently prints to console. In future, will log to DB table
         */
        public static void logError(Exception e)
        {
            try
            {
                //Write Out to Database Table
                System.Console.WriteLine(e.ToString());
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.Source);
                System.Console.WriteLine(e.StackTrace);
                System.Console.WriteLine(e.TargetSite);
            }
            catch (Exception ee)
            {
                System.Console.WriteLine(ee.ToString());
            }
        }
        public static void logErrorFile(Exception error, String dir = @"C:\temp\error\")
        {
            try
            { 
                String file = stripChars(DateTime.Now.ToShortDateString()) + "quantum.txt";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (!File.Exists(dir + file))
                {
                    using (StreamWriter sw = File.CreateText(dir + file))
                        sw.WriteLine("----------------------------" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "-----------------------------------");
                }
                using (StreamWriter sw = File.AppendText(dir + file))
                {
                    sw.WriteLine(error.ToString());
                    sw.WriteLine(error.Message);
                    sw.WriteLine(error.Source);
                    sw.WriteLine(error.StackTrace);
                    sw.WriteLine(error.TargetSite);
                }
            }
            catch (Exception e)
            {
                logError(e);
            }
        }

        /*
         * Log Error (String)
         * Logs quick error message
         */
        public static void logError(String error)
        {
            System.Console.WriteLine(error);
        }
        public static String returnNull(String val)
        {
            String returnVal = "0";
            try
            {
                if (!String.IsNullOrEmpty(val))
                    return val;

            }
            catch (Exception e)
            {
                logError(e);
            }
            return returnVal;
        }

        public static void logErrorFile(String error, String dir = @"C:\temp\error\")
        {
            try
            { 
                String file = stripChars( DateTime.Now.ToShortDateString() )  +  "quantum.txt";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (!File.Exists(dir + file))
                {
                    using (StreamWriter sw = File.CreateText(dir + file))
                        sw.WriteLine("----------------------------" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "-----------------------------------");
                }
                using (StreamWriter sw = File.AppendText(dir+file))
                {
                    sw.WriteLine(error);
                }
            }
            catch (Exception e)
            {
                logError(e);
            }
        }
 
        /*
         * StripChars: Removes all non-numeric characters 
         *  Question:
         *  Are these numbers possibly POs? 335520065171741, 8593793254 or  0290214800/00315055042 or N22913361-00-001 PICK  from file with packageactivitydate 20191115
         */
        public static String stripChars(String value="", String stripType="ALL")
        {
            String newValue = value;
            try
            {
                newValue = Regex.Replace(newValue, "SIMENTAL", "");

                if (stripType == "ALL")
                    newValue = Regex.Replace(value, "\\D", ""); //Replace all non-digit chars
                if (stripType=="PARTIAL" )
                {
                    newValue = Regex.Replace(newValue, "SDI PO#", "");
                    newValue = Regex.Replace(newValue, @"[/s]+" , "");
                    newValue = Regex.Replace(newValue, "-", "");
                    newValue = Regex.Replace(newValue, "-", "");
                    newValue = Regex.Replace(newValue, "#", "");
                    newValue = Regex.Replace(newValue, ":", "");
                    newValue = Regex.Replace(newValue, ".", "");
                    newValue = Regex.Replace(newValue, "/", "");
                    newValue = Regex.Replace(newValue, "SDI", "");
                    newValue = Regex.Replace(newValue, "ORDER", "");
                    newValue = Regex.Replace(newValue, "REQ", "");
                    newValue = Regex.Replace(newValue, "INVOICE", "");
                    newValue = Regex.Replace(newValue, "ORDER", "");
                    newValue = Regex.Replace(newValue, "MAINT", "");
                    newValue = Regex.Replace(newValue, "CUST", "");
                    newValue = Regex.Replace(newValue, "PURCHASE", "");
                } 
                if (stripType == "PO")
                {
                    newValue = newValue.Replace("MAINT", "");
                    newValue = newValue.Replace("PO; ", "");
                    newValue = newValue.Replace("PO.", "");
                    newValue = newValue.Replace("SDI", "");
                    newValue = newValue.Replace("SDI PO ", "");
                    newValue = newValue.Replace("SDI PO#", "");
                    newValue = newValue.Replace("PO: ", "");
                    newValue = newValue.Replace("PO : ", "PO");
                    newValue = newValue.Replace("PO # ", "PO");
                    newValue = newValue.Replace("PO #", "");
                    newValue = newValue.Replace("PO# ", "PO");
                    newValue = newValue.Replace("PO: ", "PO");
                    newValue = newValue.Replace("PO#", "PO");
                    newValue = newValue.Replace("PO:", "PO");
                    newValue = newValue.Replace("POPO", "PO");
                    newValue = newValue.Replace("PO ", "PO");
                    newValue = newValue.Replace("P.O.", "");
                    newValue = newValue.Replace("REQ#", "");
                    newValue = newValue.Replace("INVOICE", "");
                    newValue = newValue.Replace("ORDER", "");
                    newValue = newValue.Replace("MAINT", "");
                    newValue = newValue.Replace("SDI", "");
                    newValue = newValue.Replace("CUST", "");
                    newValue = Regex.Replace(newValue, "PO", "");
                    newValue = Regex.Replace(newValue, @"[/s]+", "");
                    newValue = Regex.Replace(newValue, "PURCHASE", "");
                }
                if (stripType == "PO2")
                { 
                    newValue = Regex.Replace(newValue, @"[/s]+", "");
                    newValue = newValue.Replace("-", "");
                    newValue = newValue.Replace("; ", "");
                    newValue = newValue.Replace(".", "");
                    newValue = newValue.Replace("#", "");
                    newValue = newValue.Replace("SDI", "");
                    newValue = newValue.Replace("MAINT", "");
                    newValue = newValue.Replace("_", "");
                    newValue = newValue.Replace(",", "");
                    newValue = newValue.Replace(":", "");
                    newValue = newValue.Replace("REQ", "");
                    newValue = newValue.Replace("ORDER", "");
                    newValue = newValue.Replace("PO", "");
                    newValue = newValue.Replace("PT", "");
                    newValue = newValue.Replace("CUST", "");
                    newValue = Regex.Replace(newValue, "PURCHASE", "");
                    newValue = Regex.Replace(newValue, "INVOICE", "");
                 }
                if (stripType == "DATE")
                {
                    newValue = newValue.Replace(" ", "_");
                    newValue = newValue.Replace("-", "_");
                    newValue = newValue.Replace("/", "_");
                    newValue = newValue.Replace(@"\", "_");
                 }

            }
            catch (Exception e)
            {
                logError(e);
            }
            return newValue;
        }

        /* Finds Strings that match the exact pattern  */
        public static string RegSearch(string input, String pattern)
        {
            String newStr = input;
            try
            { 
                  newStr = (new Regex(pattern)).Match(input).Value;
             }
            catch (Exception e)
            {
                logError(e);
            }
            return newStr;
        }

        public static String stripChars(String str,  String[] stripChar)
        {
            String newStr = str;
            try
            {
                for (int i=0; i<stripChar.Length; i++) 
                    newStr = newStr.Replace(stripChar[i],   ""); //Replace all non-digit chars
                 
            }
            catch (Exception e)
            {
                logError(e);
            }
            return newStr;
        }

        /*
         * WinSCP
         * Use if server FTPing into has extra TLS security
         * see winscp.net documentation for more info
         * Verify a reference to the WinSCP/Winscp.dll exists in References
         * Use this for more secure FTP
         * Deliberately using WinSCP prefix  for this method
         */
        public static void winSCP(ftpData fromFtp, ftpData toFtp)
        {
            try
            {
                // Set up session options
                WinSCP.SessionOptions sessionOptions = new WinSCP.SessionOptions
                {
                    Protocol = WinSCP.Protocol.Ftp,
                    HostName = fromFtp.server,
                    UserName = fromFtp.userid,
                    Password = fromFtp.password,
                    FtpSecure = WinSCP.FtpSecure.Explicit,
                }; 

                using (WinSCP.Session session = new WinSCP.Session())
                {
                    // Connect
                    String dir = Directory.GetCurrentDirectory().Substring(0,Directory.GetCurrentDirectory().Length - @"\bin\Debug".Length) + @"\SupportFiles\WinSCP" ;
                      
                  session.ExecutablePath = @dir + @"\WinSCP.exe";
                    session.Open(sessionOptions);
                    WinSCP.RemoteDirectoryInfo directory = session.ListDirectory(fromFtp.directory); 
                    WinSCP.TransferOperationResult dr = null;
                    WinSCP.TransferOptions dloadOpts = new WinSCP.TransferOptions();
                    dloadOpts.TransferMode = WinSCP.TransferMode.Automatic;
                     foreach (WinSCP.RemoteFileInfo file in directory.Files)
                     {
                         if (!file.IsDirectory && file.Length <= fromFtp.filesize && 
                             (file.LastWriteTime.Date == DateTime.Today.Subtract(new TimeSpan(fromFtp.days, 0, 0, 0)) //use to search on a specific date i.e. 5 days ago, etc.
                              //|| (( fromFtp.startDate != DateTime.MinValue && fromFtp.endDate != DateTime.MinValue) &&  file.LastWriteTime.Date >= fromFtp.startDate.Date && file.LastWriteTime.Date <= fromFtp.endDate.Date) //use if you want to search in daterange
                             ))                             
                        {
                            dr = session.GetFiles(file.Name, toFtp.server + toFtp.directory + file.Name, false, null); //note:  just pull down individuals   // dr =    session.GetFiles( "*"  , @"c:\sdi\temp\"     , false, dloadOpts); //note: can also just pull down all files and not individuals 
                            logError(file.Name  + " Downloaded: "  + dr.IsSuccess );   //  logError( dr.IsSuccess + " " + dr.Failures.ToString() + session.SessionLogPath + session.ToString());
                            dr.Check();  //  foreach (WinSCP.TransferEventArgs t in dr.Transfers) logError(t.FileName);

                         }
                    }
                }
            }
            catch (Exception e)
            {
                logError(e);
            }
        }
        /* doFTP: 
         * Identifies multiple files in FTP folder based upon size restrictions, extension name, etc. for FTP.  
         * Uses toClient to download files but users could also call ftpfile
         * Use this for plain FTP
         */ 
        public static void doFtp(ftpData fromFtp, ftpData toFtp)
        {
             try
            {
                //Get FTP Files from initial directory
 	            FtpWebRequest fromDirectoryRequest = (FtpWebRequest)WebRequest.Create("ftp://" + fromFtp.server);
                fromDirectoryRequest.Credentials = new NetworkCredential(fromFtp.userid, fromFtp.password); //(fromFtp.userid + "@" + fromFtp.server).Normalize()

                fromDirectoryRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails; 
                if (fromFtp.extension == "zip") fromDirectoryRequest.UseBinary = true;
                if (fromFtp.extension == "txt" || fromFtp.extension == "csv")
                {
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11;
                    fromDirectoryRequest.UseBinary = false;
                    fromDirectoryRequest.UsePassive = true;
                    fromDirectoryRequest.EnableSsl = true; //use for explicit ftp over tls connection 
                    fromDirectoryRequest.ClientCertificates = new X509Certificate2Collection();
                    // SslStream.AuthenticateAsClient(fromFtp.server, new X509Certificate2Collection(), System.Security.Authentication.SslProtocols.Tls12, false);
                    // ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
                }
                FtpWebResponse fromDirectoryResponse = (FtpWebResponse)fromDirectoryRequest.GetResponse();
                StreamReader fromDirectoryReader = new StreamReader(fromDirectoryResponse.GetResponseStream());

                //Use to copy files down (if using HttpClient)
                WebClient toClient = new WebClient(); // toClient.Credentials = new NetworkCredential(toFtp.userid, toFtp.password); 
                 //use to capture local filenames
     
                long directoryFileSize = 0;
                DateTime filedate = new DateTime();
                 //First Get list of all filenames to pull from directory
                 String directoryListing = fromDirectoryReader.ReadLine(); //Get first directory file entry

                 //Prepare local directory to copy files down
                 cleanDirectory(toFtp.server + "/" + toFtp.directory);

                 while (!string.IsNullOrEmpty(directoryListing))
                 { 
                     //Grab filename from directory info provided
                     logError(directoryListing);
                    var filenameArray = directoryListing.Split(' ');  
                    directoryListing = filenameArray[filenameArray.Count() - 1]; //Grab filename

                     //Only grab file with allowed extension 
                    if (directoryListing.Contains(fromFtp.extension))  
                    {
                        directoryFileSize = Int64.Parse(filenameArray[filenameArray.Count() - 6]); //Grab directory filesize   
                        DateTime.TryParse(filenameArray[filenameArray.Count() - 5] + " " + stripChars(filenameArray[filenameArray.Count() - 4]) + "," + stripChars(filenameArray[filenameArray.Count() - 2]),  out filedate); //grab date

                        //Copy filedown if it is below the specified filesize and within the specified date range
                        if (directoryFileSize <= toFtp.filesize && (filedate <= filedate.Subtract(new TimeSpan(fromFtp.days,0,0,0)) ) )
                        { 
                            toClient.DownloadFile(fromFtp.server + "/" + directoryListing, @toFtp.server + "/" + toFtp.directory + "/" + directoryListing); 
                        }
                    }
                     
                    directoryListing = fromDirectoryReader.ReadLine();
                 }

                 fromDirectoryReader.Close();
                 fromDirectoryResponse.Close(); 
            } 
            catch(Exception e)
            {
                QuantumUtility.logError(e);
            }
        }
        /*
         * CleanDirectory: Removes any data from local directory
         */
        public static void cleanDirectory(String dir)
        {
            try
            {
                //Prepare local directory to copy files down
                if (Directory.Exists(dir))
                {
                    //Clear out temp directory 
                    foreach (System.IO.FileInfo file in (new DirectoryInfo(@dir)).GetFiles())
                        file.Delete();
                    Directory.Delete(dir);
                }
                Directory.CreateDirectory(dir);
            }
            catch (Exception e)
            {
                logError(e);
            }
        }

        /*
         * FtpFile
         * FTPs individual file
         **/
        public static void ftpFile(String filename, ftpData fromFtpData, ftpData toFtpData)
        {
            try
            {
                //Open separate connection to server to pull down one file
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(fromFtpData.server);
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                req.KeepAlive = true;
                req.Credentials = new NetworkCredential(fromFtpData.userid, fromFtpData.password);

                //prepare response to read from ftp server
                FtpWebResponse resp = (FtpWebResponse)req.GetResponse();
                Stream respStr = resp.GetResponseStream();
                StreamReader rdr = new StreamReader(respStr);

                //write to local directory
                using (FileStream writer = new FileStream(toFtpData.server + "/" + toFtpData.directory + "/" + filename, FileMode.Create))
                {
                    int currentPosition = respStr.Read(new byte[2048], 0, 2048);
                    while (currentPosition > 0)
                    {
                        writer.Write(new byte[2048], 0, currentPosition);
                        currentPosition = respStr.Read(new byte[2048], 0, 2048);
                    }
                }
                rdr.Close();
                resp.Close();
            }
            catch (Exception e)
            {
                logError(e);
            }
        }

        /*
         * validateServerCertificate
         * Used to assist with FTP cert on UPS
         * Please see https://stackoverflow.com/questions/41863244/sslstream-and-authentication for more
         * */
        public static bool ValidateServerCertificate(  object sender,   X509Certificate certificate,  X509Chain chain,   SslPolicyErrors sslPolicyErrors)
        {
            bool certBool = false; 
            try
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    certBool = true; 
                }
                else
                    logError(sslPolicyErrors.ToString());  

            }
            catch (Exception e)
            {
                logError(e);
            }
            return certBool;
        }

    }

    /* ftpData: 
     * Class handles all ftp parametrs, to make easier to pass from console over to ftp method
     */
    public class ftpData
    {
        public String server="";
        public String userid="";
        public String password="";
        public String extension = ""; //If using plainFTP, use this to verify you're downloading a file with the needed extension
        public String directory="";
        public DateTime startDate = new DateTime();
        public DateTime endDate = new DateTime();
        public int filesize = 0;
        public int days=0;

        public ftpData()
        {
            this.server="";
            this.userid="";
            this.password="";
            this.filesize = 0; 

        }

        public ftpData(String server, String userid, String password)
        {
            this.server=server;
            this.userid=userid;
            this.password=password;
            this.startDate = DateTime.Now;
        }

        public ftpData(String server, String directory, String userid, String password)
        {
            this.server = server;
            this.directory = directory;
            this.userid = userid;
            this.password = password;
            this.startDate = DateTime.Now;
        }
   
        public ftpData(ftpData ftpdata) : this()
        {
            try
            {
                if (ftpdata.server.Length > 0) this.server = ftpdata.server;
                if (ftpdata.userid.Length > 0) this.userid = ftpdata.userid;
                if (ftpdata.password.Length > 0) this.password = ftpdata.password;
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
            }
        }
    }
}
