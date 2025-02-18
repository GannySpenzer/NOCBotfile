using System;

using Xamarin.Forms;


namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class UserProfilePage : ContentPage, INavPageChild
	{

		public const string PAGE_ID = "UserProfilePage";
		private const string m_pageTitle = "User";

		//private userInfo usr;
		
		private Label lblUsername;
		private Label lblBusinessUnit;
		private Label lblUserId;
		private Label lblPhone;
		private Label lblEmail;
		private Label lblProdView;

		private Button btnLogout;

		#region INavPageChild implementation
		public INavigation Navigator { get; set; }
		public string PageId {
			get { return (PAGE_ID); }
		}
		#endregion

		public UserProfilePage ()
		{
			try {
				this.Title = m_pageTitle;

				NavigationPage.SetTitleIcon (this, Device.OnPlatform (
					iOS: (FileImageSource)ImageSource.FromFile ("SDiHeader.png"),
					Android: null,
					WinPhone: (FileImageSource)ImageSource.FromFile ("SDiM_Icon_2.png")
				));

				this.ToolbarItems.Add (new CartToolbarItem (this));

				//usr = (userInfo)App.Current.Properties [App.LOGGED_IN_USER];

				SQLiteDataAccess sqliteobj = new SQLiteDataAccess ();
				var localuser = sqliteobj.ReadItemInLocalDB ();
				if (localuser == null) {
					localuser = new UserDetailBO ();
				}

				lblUsername = new Label () {
					//Text = usr.Name,
					Text = localuser.Name,
					HorizontalOptions = LayoutOptions.StartAndExpand,
					FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
					FontAttributes = FontAttributes.Bold
				};

				lblBusinessUnit = new Label () {
					//Text = usr.BusinessUnitId + " - " + usr.BusinessUnitName,
					Text = localuser.BusinessUnitId + " - " + localuser.BusinessUnitName,
					HorizontalOptions = LayoutOptions.StartAndExpand,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				lblProdView = new Label () {
					//Text = usr.ProductViewId.ToString () + " (Product View)",
					Text = localuser.ProductViewId.ToString () + " (Product View)",
					HorizontalOptions = LayoutOptions.StartAndExpand,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				lblUserId = new Label () {
					//Text = usr.UserId,
					Text = localuser.UserId,
					HorizontalOptions = LayoutOptions.StartAndExpand,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				lblEmail = new Label () {
					//Text = usr.Email,
					Text = localuser.Email,
					HorizontalOptions = LayoutOptions.StartAndExpand,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				lblPhone = new Label () {
					//Text = usr.Phone,
					Text = localuser.Phone,
					HorizontalOptions = LayoutOptions.StartAndExpand,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				btnLogout = new Button () {
					Text = "Sign Out",
					Style = common.ButtonStyleA
				};
				btnLogout.Image = Device.OnPlatform (
					iOS: (FileImageSource)ImageSource.FromFile ("locked.png"),
					Android: (FileImageSource)ImageSource.FromFile ("locked.png"),
					WinPhone: (FileImageSource)ImageSource.FromFile ("locked.png")
				);
				btnLogout.Clicked += (sender, e) => LogoutEvent ();
				btnLogout.IsEnabled = true;

				var frame3 = new Frame () {
					Padding = new Thickness (0, 30, 0, 0),
					Content = btnLogout,
					BackgroundColor = Color.Transparent
				};

				Content = new ScrollView () {
					Padding = 6,
					Content = new StackLayout () {
						Children = { lblUsername, lblBusinessUnit, lblProdView, lblUserId, lblEmail, lblPhone, frame3 }
					}
				};
			
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "UserProfilePage-UserProfilePage");
			}
		}

		private void LogoutEvent () {
			try {
			App.myInstance.Logout ();
			
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "UserProfilePage-LogoutEvent");
			}
		}

	}
}

