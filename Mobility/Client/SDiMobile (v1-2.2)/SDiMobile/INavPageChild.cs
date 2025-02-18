using System;
using Xamarin.Forms;

namespace SDiMobile
{
	public interface INavPageChild
	{
		INavigation Navigator { get; set; }
		string PageId { get; }
	}
}

