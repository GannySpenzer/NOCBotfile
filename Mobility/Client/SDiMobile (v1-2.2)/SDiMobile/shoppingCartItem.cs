using System;

namespace SDiMobile
{
	public class shoppingCartItem {
		public int LineNo { get; set; }
		public partItem Part { get; set; }
		public string WorkOrderNo { get; set; }
		public string ChargeCode { get; set; }
		public string MachineNo { get; set; }
		public double Quantity { get; set; }

		public shoppingCartItem () {
			InitMembers();
		}

		private void InitMembers () {
			WorkOrderNo = "|";
			ChargeCode = "|";
			MachineNo = "|";
			Quantity = 0;
		}
	}

}

