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
	public class ItemDetailPage : ContentPage, INavPageChild
	{

		public const string PAGE_ID = "ItemDetailPage";
		private const string m_pageTitle = "Item";

		private Label lblItemDesc1;
		private Label lblItemCategory;
		private Label lblUM;
		private Label lblPartNumber;
		private Label lblMfgPartNumber;

		private Label lblQty;
		private Entry txtQty;

		private Label lblWO;
		private Entry txtWO;

		private Label lblCC;
		private Entry txtCC;

		private Button btnBuy;

		private Stepper qtyStepper;

		private partItem iPart;

		private ActivityIndicator indicator;

		#region INavPageChild implementation
		public INavigation Navigator { get; set; }
		public string PageId {
			get { return (PAGE_ID); }
		}
		#endregion

		public ItemDetailPage (partItem itm)
		{
			try {
				this.Title = m_pageTitle;

				NavigationPage.SetTitleIcon (this, Device.OnPlatform (
					iOS: null,//(FileImageSource)ImageSource.FromFile ("SDiM_Icon_2.png"),
					Android: null,
					WinPhone: (FileImageSource)ImageSource.FromFile ("SDiM_Icon_2.png")
				));

				this.ToolbarItems.Add (new CartToolbarItem (this));

				indicator = new ActivityIndicator () {
					HorizontalOptions = LayoutOptions.CenterAndExpand
				};

				iPart = itm;

				char sPad = " " [0];

				lblItemDesc1 = new Label () {
					Text = itm.ItemDescription,
					FontSize = Device.GetNamedSize (NamedSize.Large, typeof(Label)),
					FontAttributes = FontAttributes.Bold
				};

				lblPartNumber = new Label () {
					Text = itm.PartNo,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				lblUM = new Label () {
					Text = itm.UOM,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				lblItemCategory = new Label () {
					Text = itm.ClassName,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				lblMfgPartNumber = new Label () {
					Text = itm.MfgPartNo,
					FontSize = Device.GetNamedSize (NamedSize.Medium, typeof(Label))
				};

				var section0 = new Grid () {
					RowDefinitions = {
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto }
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (240, GridUnitType.Star) },
						new ColumnDefinition { Width = GridLength.Auto }
					}
				};
				// ( column, row) OR ( column, column span, row, row span )
				section0.Children.Add (lblPartNumber, 0, 0);
				section0.Children.Add (lblUM, 1, 0);

				section0.Children.Add (lblItemCategory, 0, 1);

				section0.Children.Add (lblMfgPartNumber, 0, 2);

				var imgPart = new Image () {
					HorizontalOptions = LayoutOptions.CenterAndExpand
				};
				if (itm.FullImagefile.StartsWith ("http://"))
					imgPart.Source = ImageSource.FromUri (new Uri (itm.FullImagefile));
				else
					imgPart.Source = ImageSource.FromFile (itm.FullImagefile);

				int entryFieldLength = 24;

				lblQty = new Label () {
					Text = "Quantity"
				};

				txtQty = new Entry () {
					Text = "0",
					Placeholder = "quantity".PadRight (10, sPad)
				};
				txtQty.TextChanged += OnQuantityChanged;

				lblWO = new Label () {
					Text = "W/O"
				};

				txtWO = new Entry () {
					Text = "",
					Placeholder = "work order #".PadRight (entryFieldLength, sPad)
				};

				lblCC = new Label () {
					Text = "Charge Code"
				};

				txtCC = new Entry () {
					Text = "",
					Placeholder = "charge code".PadRight (entryFieldLength, sPad)
				};

				qtyStepper = new Stepper () {
					Minimum = 0,
					Maximum = 1000000,
					Increment = 1
				};
				try {
					qtyStepper.Value = Convert.ToDouble (txtQty.Text);
				} catch (Exception) {
				}
				;
				qtyStepper.ValueChanged += OnStepperValueChanged;

				var section10 = new Grid () {
					RowDefinitions = {
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto },
						new RowDefinition { Height = GridLength.Auto }
					},
					ColumnDefinitions = {
						new ColumnDefinition { Width = new GridLength (90, GridUnitType.Star) },
						new ColumnDefinition { Width = GridLength.Auto },
						new ColumnDefinition { Width = GridLength.Auto }
					}
				};

				section10.Children.Add (lblQty, 0, 0);
				section10.Children.Add (txtQty, 1, 0);
				section10.Children.Add (qtyStepper, 2, 0);

				section10.Children.Add (lblWO, 0, 1);
				section10.Children.Add (txtWO, 1, 1);
				Grid.SetColumnSpan (txtWO, 2);

				section10.Children.Add (lblCC, 0, 2);
				section10.Children.Add (txtCC, 1, 2);
				Grid.SetColumnSpan (txtCC, 2);

				btnBuy = new Button () {
					Text = "Add To Shopping Cart",
					Style = common.ButtonStyleA
				};
				btnBuy.Clicked += btnBuy_OnClick;

				var itmAttr = new StackLayout () {
					Children = {
						new Label () {
							Text = "AVAILABLE QUANTITY :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.QuantityOnHand.ToString ("N0"),
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "PRICE :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.Price.ToString ("C2"),
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "DESCRIPTION :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.ItemDescription,
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "SUPPLIER DESCRIPTION :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.SupplierShortDescription,
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "PART NUMBER :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.PartNo,
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "UNIT OF MEASURE :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.UOM,
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "CATEGORY :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.ClassName,
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "MANUFACTURER :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.MfgName,
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "MANUFACTURER PART # :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.MfgPartNo,
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
						new Label () {
							Text = "ITEM ID :",
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.Bold,
							FontSize = Device.GetNamedSize (NamedSize.Micro, typeof(Label)),
							TextColor = Color.Blue
						},
						new Label () {
							Text = itm.ItemId.ToString (),
							HorizontalOptions = LayoutOptions.StartAndExpand,
							FontAttributes = FontAttributes.None
						},
					}
				};

				var frame0 = new Frame () {
					Padding = new Thickness (0, 10, 0, 0),
					BackgroundColor = Color.Transparent,
					Content = new Label () {
						HorizontalOptions = LayoutOptions.CenterAndExpand,
						HeightRequest = 2
					}
				};

				Content = new ScrollView () {
					Padding = 6,
					Content = new StackLayout () {
						Children = { lblItemDesc1, indicator, section0, imgPart, section10, btnBuy, frame0, itmAttr }
					}
				};
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ItemDetailPage-ItemDetailPage");
			}

		}

		private void OnStepperValueChanged (object sender, ValueChangedEventArgs e) {
			try {
				txtQty.Text = e.NewValue.ToString ();
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ItemDetailPage-OnStepperValueChanged");
			}
		}

		private void OnQuantityChanged (object sender, TextChangedEventArgs e) {
			try {
				double nQty = 0;
				try {
					nQty = Convert.ToDouble (txtQty.Text);
				} catch (Exception) {
				}
				;
				if (nQty != qtyStepper.Value) {
					qtyStepper.Value = nQty;
				}
				;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ItemDetailPage-OnQuantityChanged");
			}
		}

		private async void btnBuy_OnClick (object sender, EventArgs e) {
			try {
				indicator.IsRunning = true;
				double nQty = 0;
				try {
					nQty = Convert.ToDouble (txtQty.Text);
				} catch (Exception) {
				}
				;
				if (nQty > 0) {
					var cart = ((List<shoppingCartItem>)App.Current.Properties [App.SHOPPING_CART]);

					cart.Add (new shoppingCartItem () {
						Part = iPart,
						Quantity = Convert.ToDouble (txtQty.Text),
						WorkOrderNo = txtWO.Text,
						ChargeCode = txtCC.Text
					});

					// reset some fields (just in case)
					txtQty.Text = "0";
					qtyStepper.Value = 0;
					txtWO.Text = "";
					txtCC.Text = "";

//				await DisplayAlert ("Confirmation", "Part added to shopping cart.", "OK");

					string opt1 = "Review/Submit Order";
					string opt2 = "Continue Shopping";

					string s = await DisplayActionSheet (
						          "Part added to shopping cart. What do you want to do next?", 
						          null, 
						          null, 
						          new string [] { opt2, opt1 });

					indicator.IsRunning = false;

					if (s == opt1) {
						// show shopping cart content
						var detailPage = new ShoppingCartPage ();
						if (Navigator != null) {
							detailPage.Navigator = Navigator;
							await Navigator.PushAsync (detailPage);
						} else {
							await Navigation.PushAsync (detailPage);
						}
					} else {
						if (Navigator != null) {
							await Navigator.PopToRootAsync ();
						} 
					}
				}
				;
				indicator.IsRunning = false;
			} catch (Exception ex) {
				SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
				LocalDBObj.InsertLogInLocalDB (ex, "ItemDetailPage-btnBuy_OnClick");
			}
		}

	}
}

