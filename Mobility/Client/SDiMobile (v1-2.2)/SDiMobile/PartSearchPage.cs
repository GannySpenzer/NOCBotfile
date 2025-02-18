using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Forms;
using Worklight;


namespace SDiMobile
{
	public class PartSearchPage : ContentPage, INavPageChild
	{

		// constructors
		public PartSearchPage () {
			try{
			InitMembers ();
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "PartSearchPage-PartSearchPage");
			}
		}

		public PartSearchPage (string searchString)
		{
			try {
				InitMembers (searchString);
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "PartSearchPage-PartSearchPage");
			}
		}

		public const string PAGE_ID = "PartSearchPage";
		private const string m_pageTitle = "Search";

		private SearchBar sbPart;
		private partSearchParam p;
		private bool isCanRequestMore;

		private Label lblMsg;
		private StackLayout lo;
		private InfiniteListView iView;

		#region INavPageChild implementation
		public INavigation Navigator { get; set; }
		public string PageId { 
			get { return (PAGE_ID); }
		}
		#endregion

		private void InitMembers (string searchString = "") {
			try {
				//this.Title = m_pageTitle;
				this.Title = Device.OnPlatform (
					iOS: m_pageTitle,
					Android: null,
					WinPhone: m_pageTitle
				);


				NavigationPage.SetTitleIcon (this, Device.OnPlatform (
					iOS: (FileImageSource)ImageSource.FromFile ("SDiHeader.png"),
					Android: null,
					WinPhone: (FileImageSource)ImageSource.FromFile ("SDiM_Icon_2.png")
				));

				this.ToolbarItems.Add (new CartToolbarItem (this));
				SQLiteDataAccess sqliteobj = new SQLiteDataAccess ();
				var localuser = sqliteobj.ReadItemInLocalDB ();

				p = new partSearchParam () {
					startPage = 1,
					itemCount = ((int)App.myInstance.Properties [App.PART_SEARCH_MAX_RETURN_COUNT]),
					//prodviewID = Convert.ToInt32 (((userInfo)App.myInstance.Properties [App.LOGGED_IN_USER]).ProductViewId),
					prodviewID = localuser.ProductViewId,
					queryText = "",
					isProdDescRequired = false,
				};

				isCanRequestMore = false;

				sbPart = new SearchBar () {
					Placeholder = "search criteria ..."
				};
				sbPart.SearchButtonPressed += (sender, e) => doPartSearch (sbPart.Text);

				lblMsg = new Label () {
					Text = "",
					FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label)),
					HorizontalOptions = LayoutOptions.StartAndExpand
				};

				var frame1 = new Frame () {
					Padding = new Thickness (0, 0, 0, 14),
					BackgroundColor = Color.Transparent,
					Content = lblMsg
				};

				var cell = new DataTemplate (typeof(ImageCell));

				cell.SetBinding (TextCell.TextProperty, "TextDisplay1");
				cell.SetBinding (TextCell.DetailProperty, "TextDisplay2");
				cell.SetBinding (ImageCell.ImageSourceProperty, "FullImagefile");

				iView = new InfiniteListView () {
					ItemsSource = null,

					VerticalOptions = LayoutOptions.FillAndExpand,
					BackgroundColor = Color.Transparent,
					RowHeight = 80,
					SeparatorColor = Color.Transparent,

					ItemTemplate = cell,

					LoadMoreCommand = new Command (LoadMoreItems)
				};
				//iView.ItemSelected += SelectedItemChanged;
				iView.ItemTapped += IView_ItemTapped;

				lo = new StackLayout () {
					Padding = 6
				};
				lo.Children.Add (sbPart);
				lo.Children.Add (frame1);
				lo.Children.Add (iView);

				Content = lo;

				if ((searchString != null) && (searchString.Length > 0)) {
					sbPart.Text = searchString;
					doPartSearch (searchString);
				}
				;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "PartSearchPage-InitMembers");
			}

		}

		async void IView_ItemTapped (object sender, ItemTappedEventArgs e)
		{
			try{
				var detailPage = new ItemDetailPage (e.Item as partItem);
					if (Navigator != null) {
						detailPage.Navigator = Navigator;
						await Navigator.PushAsync (detailPage);
					} else {
						await Navigation.PushAsync (detailPage);
					}
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "PartSearchPage-IView_ItemTapped");
			}
		}

		private void SearchTextChangeEventHandler (object sender, EventArgs e) {
			// TODO : implement realtime search - async
		}

		private async void doPartSearch (string searchString)
		{
			try {
				lblMsg.Text = "searching ...";

				// check/sync user B/U and other information
				//		refresh search param (prod view ID) if we've updated user info
				if ((App.myInstance.Properties.ContainsKey (App.LOGGED_IN_USER)) && (App.myInstance.Properties [App.LOGGED_IN_USER] != null)) {
					userInfo usr = (userInfo)App.myInstance.Properties [App.LOGGED_IN_USER];
					bool isUpdated = await usr.checkUpdateInfo ();
					if (isUpdated) {
						p = new partSearchParam () {
							startPage = 1,
							itemCount = ((int)App.myInstance.Properties [App.PART_SEARCH_MAX_RETURN_COUNT]),
							prodviewID = Convert.ToInt32 (((userInfo)App.myInstance.Properties [App.LOGGED_IN_USER]).ProductViewId),
							queryText = "",
							isProdDescRequired = false,
						};
					}
				}

				string s = "";

				if (p.queryText != searchString) {
					p.queryText = searchString;
					p.startPage = 1;

					isCanRequestMore = true;

					var itmList = new List<partItem> () { };

					partSearcher o = null;

					o = new partSearcher ();

					try {
						List<partItem> itms = await o.search (p);
						if ((itms != null) && (itms.Count > 0)) {
							foreach (partItem itm in itms) {
								itmList.Add (itm);
							}
						}
						if (itms != null) {
							isCanRequestMore = (itms.Count == p.itemCount);
						}
						if ((itms == null) || (itms.Count == 0)) {
							s = "No item found with specified criteria";
						}
					} catch (Exception ex) { 
						#if DEBUG
						Console.WriteLine ("error :: " + ex.ToString ());
						#endif
						s = "unable to perform search. pls check connection.";
					}

					o = null;


					iView.ItemsSource = itmList;

					if (itmList.Count > 0)
						iView.ScrollTo (itmList [0], ScrollToPosition.Start, true);
				}

				lblMsg.Text = s;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "PartSearchPage-doPartSearch");
			}
		}

		private async void LoadMoreItems () {
			try{
			if (isCanRequestMore) {
				lblMsg.Text = "searching ...";

				string s = "";

				p.startPage = p.startPage + p.itemCount;

				var itmList = new List<partItem> () { };

				foreach (partItem itm in ((List<partItem>)iView.ItemsSource)) {
					itmList.Add (itm);
				};

				partSearcher o = null;

				o = new partSearcher ();

				try
				{
					List<partItem> itms = await o.search(p);
					if ((itms != null) && (itms.Count > 0)) {
						foreach (partItem itm in itms) {
							itmList.Add (itm);
						}
					}
					if (itms != null) {
						isCanRequestMore = (itms.Count == p.itemCount);
					}
				}
				catch (Exception)
				{
					s = "unable to perform search. pls check connection.";
				}

				o = null;

				iView.ItemsSource = itmList;
				lblMsg.Text = s;
			}
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "PartSearchPage-LoadMoreItems");
			}
		}

		private async void SelectedItemChanged(object sender, SelectedItemChangedEventArgs e) {
			try {
				if (e.SelectedItem != null) {
					var detailPage = new ItemDetailPage (e.SelectedItem as partItem);
					((ListView)sender).SelectedItem = null;
					if (Navigator != null) {
						detailPage.Navigator = Navigator;
						await Navigator.PushAsync (detailPage);
					} else {
						await Navigation.PushAsync (detailPage);
					}
				}
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "PartSearchPage-SelectedItemChanged");
			}
		}

	}
}

