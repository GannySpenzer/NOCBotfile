using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Worklight;

namespace SDiMobile
{
	public static class common
	{
		public static string CheckCleanJSONString (string src) {
			string tgt = src;
			if ((src != null) && (src.Trim ().Length > 0)) {
				tgt = tgt.TrimStart ('"');
				tgt = tgt.TrimEnd ('"');
				tgt = tgt.Replace ("\\", "");
			}
			return tgt;
		}

		public static Style ButtonStyleA {
			get {
				Style x = new Style (typeof(Button)) {
					Setters = {
						new Setter {Property = Button.BackgroundColorProperty, Value = Color.FromHex ("fbaa12")},
						new Setter {Property = Button.TextColorProperty, Value = Color.White},
						new Setter {Property = Button.FontSizeProperty, Value = Device.GetNamedSize(NamedSize.Large, typeof(Button))}
					}
				};
				return x;
			}
		}

		public static Style ButtonStyleB {
			get {
				Style x = new Style (typeof(Button)) {
					Setters = {
						new Setter {Property = Button.BackgroundColorProperty, Value = Color.FromHex ("fbaa12")},
						new Setter {Property = Button.TextColorProperty, Value = Color.White},
						new Setter {Property = Button.FontSizeProperty, Value = Device.GetNamedSize(NamedSize.Small, typeof(Button))}
					}
				};
				return x;
			}
		}
	}
}
