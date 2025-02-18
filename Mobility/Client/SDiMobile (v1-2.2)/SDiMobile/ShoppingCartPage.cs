using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Worklight;
using Newtonsoft.Json;
using System.Json;


namespace SDiMobile
{
	public class ShoppingCartPage : ContentPage, INavPageChild
	{

		public const string PAGE_ID = "ShoppingCartPage";
		private const string m_pageTitle = "Cart";

		private List<shoppingCartItem> cart;

		private Button btnSubmit;
		private Button btnClearShoppingCart;
		private Label lblItemCount;
		private Grid section0;

		private ActivityIndicator indicator;

		#region INavPageChild implementation
		public INavigation Navigator { get; set; }
		public string PageId {
			get { return (PAGE_ID); }
		}
		#endregion

		public ShoppingCartPage ()
		{
			try {

				this.Title = m_pageTitle;

				NavigationPage.SetTitleIcon (this, Device.OnPlatform (
					iOS: (FileImageSource)ImageSource.FromFile ("SDiHeader.png"),
					Android: null,
					WinPhone: (FileImageSource)ImageSource.FromFile ("SDiM_Icon_2.png")
				));

				indicator = new ActivityIndicator () {
					HorizontalOptions = LayoutOptions.CenterAndExpand
				};

				cart = (List<shoppingCartItem>)App.Current.Properties [App.SHOPPING_CART];

				var lblItemCountTitle = new Label () {
					Text = "Item(s) in cart : ",
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				lblItemCount = new Label () {
					Text = cart.Count.ToString (),
					FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label))
				};

				section0 = new Grid () {
					RowDefinitions = {
						new RowDefinition () { Height = GridLength.Auto }
					},
					ColumnDefinitions = {
						new ColumnDefinition () { Width = new GridLength (90, GridUnitType.Star) },
						new ColumnDefinition () { Width = GridLength.Auto }
					}
				};

				section0.Children.Add (lblItemCountTitle, 0, 0);
				section0.Children.Add (lblItemCount, 1, 0);

				btnSubmit = new Button () {
					Text = "Submit Shopping Cart",
					Style = common.ButtonStyleA
				};
				btnSubmit.Clicked += (sender, e) => SubmitShoppingCart ();
				btnSubmit.IsEnabled = (cart.Count > 0);

				var itemList = new ScrollView () {
					Content = RefreshShoppingCartItemList ()
				};

				Content = new StackLayout () {
					Padding = 6,
					Children = { section0, btnSubmit, indicator, itemList }
				};
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ShoppingCartPage-ShoppingCartPage");
			}
		}

		private Layout RefreshShoppingCartItemList () {
			var lo = new StackLayout ();
			try {
				// create a stacklayout for holding item list (grid type)

				foreach (shoppingCartItem cartItem in cart) {
					var section10 = new Grid () {
						RowDefinitions = {
							new RowDefinition () { Height = GridLength.Auto },
							new RowDefinition () { Height = GridLength.Auto },
							new RowDefinition () { Height = GridLength.Auto },
							new RowDefinition () { Height = GridLength.Auto }
						},
						ColumnDefinitions = {
							new ColumnDefinition () { Width = new GridLength (40, GridUnitType.Star) },
							new ColumnDefinition () { Width = GridLength.Auto },
							new ColumnDefinition () { Width = GridLength.Auto }
						}
					};

					var lstVw = new ListView () {
						ItemsSource = new List<partItem> () { cartItem.Part },
						VerticalOptions = LayoutOptions.FillAndExpand,
						BackgroundColor = Color.Transparent,
						RowHeight = 80
					};

					var cell = new DataTemplate (typeof(ImageCell));

					cell.SetBinding (TextCell.TextProperty, "TextDisplay1");
					cell.SetBinding (TextCell.DetailProperty, "TextDisplay2");
					cell.SetBinding (ImageCell.ImageSourceProperty, "FullImagefile");

					lstVw.ItemTemplate = cell;

					var lblWOTitle = new Label () {
						Text = "W/O"
					};

					var lblWO = new Label () {
						Text = cartItem.WorkOrderNo
					};

					var lblCCTitle = new Label () {
						Text = "Charge Code"
					};

					var lblCC = new Label () {
						Text = cartItem.ChargeCode
					};

					var txtQty = new Entry () {
						Text = cartItem.Quantity.ToString ()
					};
//				txtQty.TextChanged += (sender, e) => {
//					double nQty = 0;
//					try {
//						nQty = Convert.ToDouble(txtQty.Text);
//					}
//					catch(Exception) { };
//					if (nQty != qtyStepper.Value) {
//						qtyStepper.Value = nQty;
//					};
//				};

					var stepQty = new Stepper () {
						Minimum = 0,
						Maximum = 1000000,
						Increment = 1
					};
					try {
						stepQty.Value = Convert.ToDouble (txtQty.Text);
					} catch (Exception) {
					}
					;
					stepQty.ValueChanged += (sender, e) => {
						txtQty.Text = e.NewValue.ToString ();
					};

					var btnRemove = new Button () {
						Text = "Remove",
						Style = common.ButtonStyleB
					};
					btnRemove.Clicked += (sender, e) => OnRemoveItem (cartItem, cart);

					section10.Children.Add (lstVw, 0, 0);
					Grid.SetColumnSpan (lstVw, 3);

					section10.Children.Add (lblWOTitle, 0, 1);
					section10.Children.Add (lblWO, 2, 1);
					Grid.SetColumnSpan (lblWOTitle, 2);

					section10.Children.Add (lblCCTitle, 0, 2);
					section10.Children.Add (lblCC, 2, 2);
					Grid.SetColumnSpan (lblCCTitle, 2);

					section10.Children.Add (txtQty, 0, 3);
					section10.Children.Add (stepQty, 1, 3);
					section10.Children.Add (btnRemove, 2, 3);

					lo.Children.Add (section10);
				}
				;

				btnClearShoppingCart = new Button () {
					Text = "Clear Shopping Cart",
					Style = common.ButtonStyleA
				};
				btnClearShoppingCart.Clicked += (sender, e) => ClearShoppingCart ();
				btnClearShoppingCart.IsVisible = (cart.Count > 0);

				lo.Children.Add (btnClearShoppingCart);

			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ShoppingCartPage-RefreshShoppingCartItemList");
			}
			return lo;
		}

		private async void SubmitShoppingCart () {
			try {
				if ((cart != null) && (cart.Count > 0)) {
					indicator.IsRunning = true;

					orderParameter p = new orderParameter ();

					// order origin Id
					p.OrderOriginCode = (string)App.Current.Properties [App.ORDER_ORIGIN];

					// requestor
					userInfo usr = (userInfo)App.Current.Properties [App.LOGGED_IN_USER];
					p.Requestor = new orderRequestor () {
						UserId = usr.UserId,
						Name = usr.Name,
						BusinessUnitId = usr.BusinessUnitId,
						BusinessUnitName = usr.BusinessUnitName,
						Phone = usr.Phone,
						Email = usr.Email,
						ProductViewId = usr.ProductViewId.ToString (),
						UniqueId = usr.UniqueUserId.ToString (),
						CustomerId = usr.CustomerId
					};

					// order line item(s)
					p.CartItems = new List<orderLineItem> () { };

					int i2 = 0;

					foreach (shoppingCartItem itm in cart) {
						i2 += 1;

						p.CartItems.Add (
							new orderLineItem () {
								LineNo = i2,
								ChargeCode = itm.ChargeCode,
								WorkOrderNo = itm.WorkOrderNo,
								Quantity = itm.Quantity,
								Part = new orderPartItem () {
									ItemId = Convert.ToInt32 (itm.Part.ItemId),
									PartDescription1 = itm.Part.ItemDescription,
									PartNumber = itm.Part.PartNo,
									ProductviewId = Convert.ToInt32 (itm.Part.ProductViewId),
									UnitOfMeasure = itm.Part.UOM
								}
							}
						);
					}

					var wlc = worklightClientInstance.Instance;

					WorklightResponse res = null;

					var pString = JsonConvert.SerializeObject (p);

					WorklightProcedureInvocationData invocationData = new WorklightProcedureInvocationData (
						                                                 "SDIMobileREST", 
						                                                 "submitOrder", 
						                                                 new object[] { pString });

					res = await wlc.wlcInstance.InvokeProcedure (invocationData);

					if ((res != null) && (res.Success)) {
						orderConfirmation ordConf = null;

						JsonObject jsonObj = (JsonObject)res.ResponseJSON;

						if (jsonObj != null) {
							if (jsonObj.ContainsKey ("resultSet")) {
								if (!string.IsNullOrEmpty (jsonObj ["resultSet"].ToString ())) {
									try {
										string jsonString = common.CheckCleanJSONString (jsonObj ["resultSet"].ToString ());
										ordConf = (orderConfirmation)Newtonsoft.Json.JsonConvert.DeserializeObject (
											jsonString, 
											typeof(orderConfirmation), 
											new JsonSerializerSettings {
												NullValueHandling = NullValueHandling.Ignore,
												MissingMemberHandling = MissingMemberHandling.Ignore
											}
										);
									} catch (Exception) {
									}
								}
							}
						}

						indicator.IsRunning = false;

						if (ordConf != null) {
							string msg = "Your order was successfully submitted. Your order confirmation # is " + ordConf.OrderNo;
							await DisplayAlert ("Order Confirmation", msg, "OK");
						}
					} else {
						Console.WriteLine ("order submit FAILED");
					}

					res = null;
					wlc = null;
					usr = null;
					p = null;

					ClearShoppingCart ();

					indicator.IsRunning = false;

					if (Navigator != null) {
						await Navigator.PopToRootAsync ();
					} 
				}
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ShoppingCartPage-SubmitShoppingCart");
			}
		}

		private void ClearShoppingCart () {
			try{
			if ((cart != null) && (cart.Count > 0)) {
				// clear items in shopping cart
				cart.Clear ();
				// update item count
				lblItemCount.Text = cart.Count.ToString ();
				// refresh view
				var itemList = new ScrollView () {
					Content = RefreshShoppingCartItemList ()
				};
				Content = new StackLayout () {
					Padding = 6,
					Children = { section0, btnSubmit, itemList }
				};
			}
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ShoppingCartPage-ClearShoppingCart");
			}
		}

		private void OnRemoveItem (shoppingCartItem item, List<shoppingCartItem> items) {
			try {
				// remove item from collection
				items.Remove (item);
				// update item count
				lblItemCount.Text = items.Count.ToString ();
//			// remove view
//			int i = ((StackLayout)Content).Children.Count;
//			((StackLayout)Content).Children [i-1] = RefreshShoppingCartItemList ();
				// refresh view
				var itemList = new ScrollView () {
					Content = RefreshShoppingCartItemList ()
				};
				Content = new StackLayout () {
					Padding = 6,
					Children = { section0, btnSubmit, itemList }
				};
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ShoppingCartPage-OnRemoveItem");
			}
		}

	}
}

