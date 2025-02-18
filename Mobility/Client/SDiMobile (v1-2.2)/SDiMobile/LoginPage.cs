using System;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Forms;
using Worklight;
using Newtonsoft.Json;
using System.Json;


namespace SDiMobile
{
	public class LoginPage : ContentPage
	{

		private Button btnLogin;
		private Entry txtUserId;
		private Entry txtPassword;
		private Label lblMsg;

		private IAppManager appMgr;

		public LoginPage (IAppManager appManager)
		{
			try {
				this.Title = "";

				appMgr = appManager;

				char sPad = " " [0];

				var imgSDI = new Image () {
					Aspect = Aspect.AspectFit,
					HeightRequest = 150,
					WidthRequest = 150
				};
				imgSDI.Source = Device.OnPlatform (
					iOS: ImageSource.FromFile ("SDiLogo.png"),
					Android: ImageSource.FromFile ("SDiLogo.png"),
					WinPhone: ImageSource.FromFile ("SDiLogo.png")
				);
				var appname = new Label () {
					Text = "Ordering",
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 20,
					TextColor = Xamarin.Forms.Color.FromHex ("383838")
				};

				var frame1 = new Frame () {
					Padding = new Thickness (0, 0, 0, 30),
					BackgroundColor = Xamarin.Forms.Color.Transparent,
					Content = new Label () {
						HorizontalOptions = LayoutOptions.CenterAndExpand,
						HeightRequest = 2
					},
				};

				txtUserId = new Entry () {
					Text = "",
					Placeholder = "Username".PadRight (180, sPad),
					HorizontalOptions = LayoutOptions.StartAndExpand
				};

				txtPassword = new Entry () {
					Text = "",
					Placeholder = "Password".PadRight (180, sPad),
					IsPassword = true,
					HorizontalOptions = LayoutOptions.StartAndExpand
				};

				btnLogin = new Button () {
					Text = "Sign In",
					Style = common.ButtonStyleA
				};
				btnLogin.Image = Device.OnPlatform (
					iOS: (FileImageSource)ImageSource.FromFile ("unlocked.png"),
					Android: (FileImageSource)ImageSource.FromFile ("unlocked.png"),
					WinPhone: (FileImageSource)ImageSource.FromFile ("unlocked.png")
				);
				btnLogin.Clicked += (sender, e) => LoginUser ();

				var frame3 = new Frame () {
					Padding = new Thickness (0, 20, 0, 0),
					Content = btnLogin,
					BackgroundColor = Xamarin.Forms.Color.Transparent
				};

				lblMsg = new Label () {
					Text = "",
					HorizontalOptions = LayoutOptions.CenterAndExpand
				};

				Content = new ScrollView () {
					Padding = 20,
					Content = new StackLayout () {
						Padding = new Thickness (20, 60, 20, 20),
						VerticalOptions = LayoutOptions.StartAndExpand,
						Children = { imgSDI, appname, frame1, txtUserId, txtPassword, frame3, lblMsg }
					}
				};
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "LoginPage-LoginPage");
			}

		}

		private async void LoginUser () {
			try {
				this.IsBusy = true;
				lblMsg.Text = "";
				btnLogin.IsEnabled = false;

				string uid = txtUserId.Text.Trim ().ToUpper ();
				string pw = txtPassword.Text;

				if (uid.Length > 0) {
					var wlc = worklightClientInstance.Instance;

					var bCanConnect = await wlc.CanConnect ();

					if (bCanConnect) {
						var authen = new dummyAuthenticator (wlc);
						try {
							bool isValidUser = await authen.authUser (uid, pw);
							if (isValidUser) {
								// update application properties
								userInfo usr = await userInfo.getUserInfo (uid);

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
									LocaldUser.Password = pw;
									LocaldUser.Phone = usr.Phone;
									//LocaldUser.Privs = usr.Privs;
									LocaldUser.ProductViewId = usr.ProductViewId;
									LocaldUser.UniqueUserId = usr.UniqueUserId;
									LocaldUser.UserId = usr.UserId;
									LocaldUser.DeviceID = worklightClientInstance.deviceid;
									SQLiteDataAccess sqliteobj = new SQLiteDataAccess ();
									sqliteobj.CreateItemInLocalDB (LocaldUser, usr.Privs);


									// update application context variables
									usr.Password = pw;
									usr.IsLoggedIn = true;
									App.myInstance.Properties [App.LOGGED_IN_USER] = usr;
									// login user
									this.IsBusy = false;
									appMgr.LoginUser ();
								} else {
									lblMsg.Text = "Unable to retrieve user information : " + uid;
								}
							} else {
								lblMsg.Text = "User Id/password NOT FOUND";
							}
						} catch (Exception) {
							lblMsg.Text = "Unable to retrieve user information";
						}
						authen = null;
					} else {
						lblMsg.Text = "Unable to connect to server. Please check connection.";
					}

					wlc = null;
				} else {
					lblMsg.Text = "Please provide ID and password";
				}

				btnLogin.IsEnabled = true;
				this.IsBusy = false;

			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "LoginPage-LoginUser");
			}
		}
	}
}

