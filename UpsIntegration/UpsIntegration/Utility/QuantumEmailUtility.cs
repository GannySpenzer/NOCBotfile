using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace UpsIntegration
{
    /** QuantumEmailUtility
     *  Standalone mailer
     *  
     *  Comments:
     *  Is there a global configuration file that holds smtpservername, default emails, etc.?
     *  Break Out for batch emailing?
     *  Use pre-existing email tables to log emails or create new?
     *  What are the credentials/port information?
     *  WebDev@sdi.com, sdiportalsupport@avasoft.biz  
     **/

    class QuantumEmailUtility
    {
        
        private static String smtpserver = "mail.sdi.com"; //email.smtp.com //smtp.sdi.com//taken from COnsoleUtilities/sendCustomEmails/Module1.vb
        private static String username = "";
        private static String password = "";
        private static String cc = "webdev@sdi.com"; //taken from COnsoleUtilities/sendCustomEmails/Module1.vb
        private static String from = "SDIExchADMIN@sdi.com"; //taken from sdixz/global.asax.vb
        private static int port = 25; //taken from COnsoleUtilities/sendCustomEmails/Module1.vb

        /*
         * Quick Access Test Email
         **/
        public static void email(String subject, String message)
        {
            email("michael.randall@sdi.com", "SDIExchADMIN@sdi.com", subject, message);
        }

        public static void email(String to, String subject, String message)
        {
            try
            {
                email(to, from, subject, message);
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
                QuantumUtility.logErrorFile(e.ToString());
            }
        }

        /***
         * Email (multiple parametrs)
         * Use when want to customize to/from
         */
        public static void email(String to, String from, String subject, String message)
        {
            try
            {
                MailMessage mail = new MailMessage(from, to, subject, message);
                SmtpClient smtp = new SmtpClient(smtpserver);
                smtp.Port = port;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = true;
               // smtp.Host = smtpserver;
                /*
                 *  
                 * smtp.Port=587;
                 * smtp.credentials=new System.Net.NetworkCredential(username,password);
                 * smtp.EnableSsl = true;
                 * 
                 * Or use default credentials?
                 * 
                 * 
                 * */
                smtp.Send(mail);
                QuantumUtility.logError(to+from +message);
            }
            catch (Exception e)
            {
                QuantumUtility.logError(e);
                QuantumUtility.logErrorFile(e.ToString());
            }
        }

    }
}
