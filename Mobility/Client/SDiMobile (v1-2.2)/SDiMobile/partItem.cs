using System;
using System.Collections.Generic;


namespace SDiMobile
{

	[Foundation.Preserve (AllMembers = true)]
	public class partItem
	{

		public partItem () {
		}

		public string UOM { get; set; }
		public string MfgId { get; set; }
		public string MfgPartNo { get; set; }
		public string ItemDescription { get; set; }
		public string MfgName { get; set; }
		public string ClassId { get; set; }
		public string ClassName { get; set; }
		public string ThumbnailImagefile { get; set; }
		public string FullImagefile { get; set; }
		public string ItemId { get; set; }
		public string SupplierShortDescription { get; set; }
		public string PartNo { get; set; }
		public string ProductViewId { get; set; }

		public string TextDisplay1 { 
			get { 
				return this.SupplierShortDescription;
			}
		}
		public string TextDisplay2 { 
			get { 
				return (this.PartNo + ":" + this.ItemDescription);
			} 
		}

		public decimal QuantityOnHand { get; set; }
		public decimal Price { get; set; }

	}
		
}

