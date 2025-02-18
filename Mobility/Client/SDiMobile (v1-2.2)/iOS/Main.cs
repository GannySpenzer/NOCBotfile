using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.Net.Mail;
using System.Net;

namespace SDiMobile.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			try {
				// if you want to use a different Application Delegate class from "AppDelegate"
				// you can specify it here.
				UIApplication.Main (args, null, "AppDelegate");
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "Application-Main");
				sendemail (ex);
			} 
		}

		public async static void sendemail (Exception ex)
		{
			try {
				var fromAddress = new MailAddress ("xamavaacc2@gmail.com", "SDi Ordering App");
				var toAddress = new MailAddress ("arvinthakash@gmail.com", "SDi Ordering App");
				const string fromPassword = "Avacorp123";
				const string subject = "Ordering App Exception Occurred";
				string body = "Date Time: \n\n\n"
					            + DateTime.Now +
					            "\n\n\n Inner Exception : \n\n\n"
					            + Convert.ToString (ex.InnerException) +
					            "\n\n\n Exception Message : \n\n\n"
					            + ex.Message +
					            "\n\n\n Stack Trace : \n\n\n"
								+ ex.StackTrace;

				var smtp = new SmtpClient {
						Host = "smtp.gmail.com",
						Port = 587,
						EnableSsl = true,
						DeliveryMethod = SmtpDeliveryMethod.Network,
						UseDefaultCredentials = false,
						Credentials = new NetworkCredential (fromAddress.Address, fromPassword)
				};
				using (var message = new MailMessage (fromAddress, toAddress) {
						Subject = subject,
						Body = body
					}) {
						smtp.Send (message);
				}


			} catch (Exception e) {

			}
		}


	}
}

