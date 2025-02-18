using System;
using System.Drawing;

using Xamarin.Forms;


namespace SDiMobile
{
	public class UserAcctMaint : ContentPage
	{
		
//		private Label lblTitle;

		private Entry txtUserId;
		private Button btnSubmit;
		private Label lblUsername;
		private Label lblUserId;
		//private Label lblBusinessUnit;
		private Label lblPhone;
		private Label lblEmail;
		private Button btnResetPassword;

		// mock-up data
		private user usr;

		public UserAcctMaint ()
		{
			try{
			Title = "Reset Password";

//			lblTitle = new Label () {
//				Text = "PASSWORD RESET",
//				HorizontalOptions = LayoutOptions.CenterAndExpand
//			};

//			this.BackgroundImage = "logoSDI_Vert.png";

			char sPad = " " [0];

			txtUserId = new Entry () {
				Text = "",
				Placeholder = "Username".PadRight (180, sPad),
				HorizontalOptions = LayoutOptions.StartAndExpand
			};

			btnSubmit = new Button () {
				Text = "Submit",
				Style = common.ButtonStyleA
			};
			btnSubmit.Image = Device.OnPlatform (
				iOS: (FileImageSource)ImageSource.FromFile ("refresh.png"),
				Android: (FileImageSource)ImageSource.FromFile ("refresh.png"),
				WinPhone: (FileImageSource)ImageSource.FromFile ("refresh.png")
			);
			btnSubmit.Clicked += GetData;

			var frame1 = new Frame () {
				Padding = new Thickness (0,0,0,30),
				BackgroundColor = Xamarin.Forms.Color.Transparent,
				Content = btnSubmit
			};

			lblUsername = new Label () {
				Text = "",
				HorizontalOptions = LayoutOptions.StartAndExpand,
				FontSize = Device.GetNamedSize (NamedSize.Large, typeof (Label))
			};

			lblUserId = new Label () {
				Text = "",
				HorizontalOptions = LayoutOptions.StartAndExpand,
				FontSize = Device.GetNamedSize (NamedSize.Medium, typeof (Label))
			};

			lblPhone = new Label () {
				Text = "",
				HorizontalOptions = LayoutOptions.StartAndExpand,
				FontSize = Device.GetNamedSize (NamedSize.Medium, typeof (Label))
			};

			lblEmail = new Label () {
				Text = "",
				HorizontalOptions = LayoutOptions.StartAndExpand,
				FontSize = Device.GetNamedSize (NamedSize.Medium, typeof (Label)),
				FontAttributes = FontAttributes.Bold
			};

			btnResetPassword = new Button () {
				Text = "Reset Password",
				Style = common.ButtonStyleA
			};
			btnResetPassword.Image = Device.OnPlatform (
				iOS: (FileImageSource)ImageSource.FromFile ("key-2.png"),
				Android: (FileImageSource)ImageSource.FromFile ("key_2.png"),
				WinPhone: (FileImageSource)ImageSource.FromFile ("key-2.png")
			);
			btnResetPassword.IsVisible = false;

			var frame2 = new Frame () {
				Padding = new Thickness (0,16,0,0),
				BackgroundColor = Xamarin.Forms.Color.Transparent,
				Content = btnResetPassword
			};

			Content = new ScrollView () {
				Padding = 20,
				Content = new StackLayout () {
					VerticalOptions = LayoutOptions.CenterAndExpand,
					Children = { txtUserId, frame1, lblUsername, lblUserId, lblPhone, lblEmail, frame2 }
				}
			};
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "UserAcctMaint-UserAcctMaint");
			}
		}

		private void GetData(object sender, EventArgs e) {
			try {
				// mock-up data
				if (usr == null)
					usr = new user () {
						UserId = "BAUTISE1",
						Password = "",
						BusinessUnit = "I0469",
						Phone = "484 343 6913",
						Email = "erwin.bautista@sdi.com",
						DisplayName = "ERWIN BAUTISTA"
					};
				txtUserId.Text = usr.UserId;
				lblUsername.Text = usr.DisplayName;
				lblUserId.Text = usr.UserId + " / " + usr.BusinessUnit;
				lblPhone.Text = usr.Phone;
				lblEmail.Text = usr.Email;
				btnResetPassword.IsVisible = true;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "UserAcctMaint-GetData");
			}
		}

	}
}

