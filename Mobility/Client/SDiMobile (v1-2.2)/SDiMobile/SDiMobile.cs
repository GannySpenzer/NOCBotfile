using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

using Xamarin.Forms;
using Worklight;


namespace SDiMobile
{
	public class App : Application, IAppManager
	{
		
		// constants
		public const string AUTHENTICATION_REALM_ID = "SingleStepAuthRealm";
		public const string AUTHENTICATION_REALM = "authenticationRealm";

		public const string SHOPPING_CART = "shoppingCart";

		public const string ORDER_ORIGIN_ID = "MIS";
		public const string ORDER_ORIGIN = "orderOriginCode";

		public const string LOGGED_IN_USER = "loggedInUser";

		public const int PART_SEARCH_MAX_RETURN_COUNT_VALUE = 10;
		public const string PART_SEARCH_MAX_RETURN_COUNT = "partSearchMaxReturnCount";

		public const bool SHOW_CLIENT_ITEM_DESC_VALUE = false;
		public const string SHOW_CLIENT_ITEM_DESC = "showClientItemDescription";

		public const string URL_IMAGE_THUMBNAIL_ADDY = "http://cplus_prod.sdi.com:8080/ContentPlus/images///images755784//thumbnails//";
		public const string URL_IMAGE_THUMBNAIL = "thumbnailImageURL";

		public const string URL_IMAGE_FULL_ADDY = "http://cplus_prod.sdi.com:8080/ContentPlus/images///images755784//";
		public const string URL_IMAGE_FULL = "fullImageURL";

		//// members
		//public wlcInstance WorklightClientInstance { get; set; }

		// current instance of this application
		public static App myInstance;

		// constructor(s)
		public App()
		{
			myInstance = this;
		}

		protected override void OnStart ()
		{
			try {

				/* settings
			 */

				// authentication realm
				if (!this.Properties.ContainsKey (AUTHENTICATION_REALM)) {
					this.Properties.Add (AUTHENTICATION_REALM, AUTHENTICATION_REALM_ID);
				} else if ((this.Properties [AUTHENTICATION_REALM] == null) || (((string)this.Properties [AUTHENTICATION_REALM]).Trim ().Length == 0)) {
					this.Properties [AUTHENTICATION_REALM] = AUTHENTICATION_REALM_ID;
				}

				// order origin ID
				if (!this.Properties.ContainsKey (ORDER_ORIGIN)) {
					this.Properties.Add (ORDER_ORIGIN, ORDER_ORIGIN_ID);
				} else if ((this.Properties [ORDER_ORIGIN] == null) || (((string)this.Properties [ORDER_ORIGIN]).Trim ().Length == 0)) {
					this.Properties [ORDER_ORIGIN] = ORDER_ORIGIN_ID;
				}

				// part search maximum number of returned record in a trip
				if (!this.Properties.ContainsKey (PART_SEARCH_MAX_RETURN_COUNT)) {
					this.Properties.Add (PART_SEARCH_MAX_RETURN_COUNT, PART_SEARCH_MAX_RETURN_COUNT_VALUE);
				} else if ((this.Properties [PART_SEARCH_MAX_RETURN_COUNT] == null) || (((int)this.Properties [PART_SEARCH_MAX_RETURN_COUNT]) < 1)) {
					this.Properties [PART_SEARCH_MAX_RETURN_COUNT] = PART_SEARCH_MAX_RETURN_COUNT_VALUE;
				}

				// either show catalog item description of P/S (client) item description
				//		default : show catalog item description
				if (!this.Properties.ContainsKey (SHOW_CLIENT_ITEM_DESC)) {
					this.Properties.Add (SHOW_CLIENT_ITEM_DESC, SHOW_CLIENT_ITEM_DESC_VALUE);
				} else if (this.Properties [SHOW_CLIENT_ITEM_DESC] == null) {
					this.Properties [SHOW_CLIENT_ITEM_DESC] = SHOW_CLIENT_ITEM_DESC_VALUE;
				}

				// image URLs
				//		(1) thumbnail images
				if (!this.Properties.ContainsKey (URL_IMAGE_THUMBNAIL)) {
					this.Properties.Add (URL_IMAGE_THUMBNAIL, URL_IMAGE_THUMBNAIL_ADDY);
				} else if ((this.Properties [URL_IMAGE_THUMBNAIL] == null) || (((string)this.Properties [URL_IMAGE_THUMBNAIL]).Trim ().Length == 0)) {
					this.Properties [URL_IMAGE_THUMBNAIL] = URL_IMAGE_THUMBNAIL_ADDY;
				}
				//		(2) full images
				if (!this.Properties.ContainsKey (URL_IMAGE_FULL)) {
					this.Properties.Add (URL_IMAGE_FULL, URL_IMAGE_FULL_ADDY);
				} else if ((this.Properties [URL_IMAGE_FULL] == null) || (((string)this.Properties [URL_IMAGE_FULL]).Trim ().Length == 0)) {
					this.Properties [URL_IMAGE_FULL] = URL_IMAGE_FULL_ADDY;
				}

				// remember who's logged in
				//		this gets set upon successful login
				if ((!this.Properties.ContainsKey (LOGGED_IN_USER)) || (this.Properties [LOGGED_IN_USER] == null)) {
					this.Properties.Add (LOGGED_IN_USER, new userInfo ());
				}

				////debug
				//this.Properties [LOGGED_IN_USER] = new userInfo ();

				// shopping cart instance
				if ((!this.Properties.ContainsKey (SHOPPING_CART)) || (this.Properties [SHOPPING_CART] == null)) {
					this.Properties [SHOPPING_CART] = new List<shoppingCartItem> () { };
				}

				/* launch initial page of the app
			 * 		decide whether our user is logged in or not
			 */

				// main page or the login page?
				bool isUserLoggedIn = false;

				try {
					isUserLoggedIn = ((userInfo)this.Properties [LOGGED_IN_USER]).IsLoggedIn;

				} catch (Exception ex) {
					#if DEBUG
					Console.WriteLine ("error :: " + ex.ToString ());
					#endif	
				}

				SQLiteDataAccess sqliteobj = new SQLiteDataAccess ();
				var localuser = sqliteobj.ReadItemInLocalDB ();
				//if (isUserLoggedIn)
				if (localuser != null && localuser.IsLoggedIn == true) {
					getuserdetail ();
					MainPage = new AppMainPage (this);
				} else
					MainPage = new LoginModalPage (this);
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "SDiMobile-OnStart");
			}

		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}

		#region IAppManager implementation
		public void Logout () {
			try {
				// reset who's currently logged in
				//		so next time this app runs, it will present the login page
				if ((!this.Properties.ContainsKey (LOGGED_IN_USER)) || (this.Properties [LOGGED_IN_USER] == null))
					this.Properties.Add (LOGGED_IN_USER, new userInfo ());
				this.Properties [LOGGED_IN_USER] = new userInfo ();

				// go back to the login page
				MainPage = new LoginModalPage (this);
				SQLiteDataAccess sqliteobj = new SQLiteDataAccess ();
				sqliteobj.DeleteLocalDatabase ();
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "SDiMobile-Logout");
			}
		}

		public void LoginUser () {
			try {
				// show main page
				//		the actual saving/remembering of user currently logged in is on the LoginPage
				MainPage = new AppMainPage (this);
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "SDiMobile-LoginUser");
			}
		}

		public async void getuserdetail()
		{
			try {
				var wlc = worklightClientInstance.Instance;
				SQLiteDataAccess sqliteobj = new SQLiteDataAccess ();
				var currentuser = sqliteobj.ReadItemInLocalDB ();
				var bCanConnect = await wlc.CanConnect ();

				if (bCanConnect) {
					ServerLogWriter logwriter = new ServerLogWriter();
					logwriter.WriteLogsToServer();
					var authen = new dummyAuthenticator (wlc);
					try {
						bool isValidUser = await authen.authUser (currentuser.UserId, currentuser.Password);
						if (isValidUser) {
							userInfo usr = await userInfo.getUserInfo (currentuser.UserId);
							if ((usr != null) && (usr.BusinessUnitId != null) && (usr.BusinessUnitId.Trim ().Length > 0)) {
								UserDetailBO LocaldUser = new UserDetailBO ();
								LocaldUser.BusinessUnitId = usr.BusinessUnitId;
								LocaldUser.BusinessUnitName = usr.BusinessUnitName;
								LocaldUser.CustomerId = usr.CustomerId;
								LocaldUser.Email = usr.Email;
								LocaldUser.IsLoggedIn = true;
								LocaldUser.LastInfoSyncDTTM = usr.LastInfoSyncDTTM;
								LocaldUser.Message = usr.Message;
								LocaldUser.Name = usr.Name;
								LocaldUser.Password = currentuser.Password;
								LocaldUser.Phone = usr.Phone;
								//LocaldUser.Privs = usr.Privs;
								LocaldUser.ProductViewId = usr.ProductViewId;
								LocaldUser.UniqueUserId = usr.UniqueUserId;
								LocaldUser.UserId = usr.UserId;
								LocaldUser.DeviceID = worklightClientInstance.deviceid;
								sqliteobj.CreateItemInLocalDB (LocaldUser, usr.Privs);

								usr.Password = currentuser.Password;
								usr.IsLoggedIn = true;
								App.myInstance.Properties [App.LOGGED_IN_USER] = usr;
								// login user

							} else {
								MainPage = new LoginModalPage (this);
							}
						} else {
							MainPage = new LoginModalPage (this);
						}

					} catch (Exception ex) {
						SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
						LocalDBObj.InsertLogInLocalDB (ex, "SDiMobile-getuserdetail-Resp");
					}
//					throw new Exception("Test exception");
				}
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "SDiMobile-getuserdetail");
			}
			
		}
		#endregion

	}	// class
}	// namespace

