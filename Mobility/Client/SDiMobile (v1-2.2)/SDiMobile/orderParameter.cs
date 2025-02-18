using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.Json;

namespace SDiMobile
{
	public class orderParameter
	{
		public orderRequestor Requestor { get; set; }
		public List<orderLineItem> CartItems { get; set; }
		public string OrderOriginCode { get; set; }
	}

	public class orderRequestor 
	{
		public string UserId { get; set; }
		public string Name { get; set; }
		public string BusinessUnitId { get; set; }
		public string BusinessUnitName { get; set; }
		public string Phone { get; set; }
		public string Email { get; set; }
		public string ProductViewId { get; set; }
		public string UniqueId { get; set; }
		public string CustomerId { get; set; }
//		public List<orderRequestorPriv> Privs { get; set; }
//		public string Message { get; set; }
	}

	public class orderRequestorPriv 
	{
		public string PrivType { get; set; }
		public string PrivName { get; set; }
	}

	public class orderLineItem 
	{
		public int LineNo { get; set; }
		public orderPartItem Part { get; set; }
		public string WorkOrderNo { get; set; }
		public string ChargeCode { get; set; }
		public string MachineNo { get; set; }
		public double Quantity { get; set; }

		public orderLineItem() {
			InitMembers();
		}

		private void InitMembers() {
			WorkOrderNo = "|";
			ChargeCode = "|";
			MachineNo = "|";
			Quantity = 0;
		}
	}

	public class orderPartItem 
	{
		public string PartNumber { get; set; }
		public string PartDescription1 { get; set; }
		public int ItemId { get; set; }
		public int ProductviewId { get; set; }
		public string UnitOfMeasure { get; set; }
	}

	public class orderConfirmation 
	{
		[JsonProperty("OrderId")]
		public int OrderId { get; set; }
		[JsonProperty("OrderNo")]
		public string OrderNo { get; set; }
	}
}

