using System;
using Xamarin.Forms;


namespace SDiMobile
{
	public class CartToolbarItem : ToolbarItem
	{
		private INavPageChild m_parentPage;

		public CartToolbarItem (INavPageChild pg)
		{
			
			m_parentPage = pg;

			this.Text = "Cart";

			this.Icon = Device.OnPlatform (
				iOS: (FileImageSource)ImageSource.FromFile ("shopcart.png"),
				Android: (FileImageSource)ImageSource.FromFile ("shopcart.png"),
				WinPhone: (FileImageSource)ImageSource.FromFile ("shopcart.png")
			);

			this.Clicked += (object sender, EventArgs e) => GotoShoppingCart ();

		}

		private async void GotoShoppingCart () {
			if (m_parentPage != null) {
				var pg = new ShoppingCartPage ();
				pg.Navigator = m_parentPage.Navigator;
				await m_parentPage.Navigator.PushAsync (pg);
			} 
		}
	}
}

