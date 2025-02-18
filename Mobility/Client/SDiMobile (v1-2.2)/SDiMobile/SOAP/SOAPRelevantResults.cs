using System;
using Newtonsoft.Json;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class SOAPRelevantResults 
	{
		[JsonProperty("Rank")]
		public string Rank { get; set; }
		[JsonProperty("DocId")]
		public string DocId { get; set; }
		[JsonProperty("BCSmpShippableUnitofMeasure")]
		public string BCSmpShippableUnitofMeasure { get; set; }
		[JsonProperty("BCSmpManufacturerID")]
		public string BCSmpManufacturerID { get; set; }
		[JsonProperty("BCSmpManufacturerPartNo")]
		public string BCSmpManufacturerPartNo { get; set; }
		[JsonProperty("BCSmpItemdescription")]
		public string BCSmpItemdescription { get; set; }
		[JsonProperty("BCSmpOracleItemdescription")]
		public string BCSmpOracleItemdescription { get; set; }
		[JsonProperty("BCSmpManufacturerName")]
		public string BCSmpManufacturerName { get; set; }
		[JsonProperty("BCSmpClassid")]
		public string BCSmpClassid { get; set; }
		[JsonProperty("BCSmpClassName")]
		public string BCSmpClassName { get; set; }
		[JsonProperty("BCSmpThumbnailImagefile")]
		public string BCSmpThumbnailImagefile { get; set; }
		[JsonProperty("BCSmpFullImagefile")]
		public string BCSmpFullImagefile { get; set; }
		[JsonProperty("BCSmpItemid")]
		public string BCSmpItemid { get; set; }
		[JsonProperty("BCSmpSupplierShortDesc")]
		public string BCSmpSupplierShortDesc { get; set; }
		[JsonProperty("BCSmpprefixedCustomerItemID")]
		public string BCSmpprefixedCustomerItemID { get; set; }
		[JsonProperty("rowOrder")]
		public int rowOrder { get; set; }
		[JsonProperty("BCSmpProductViewid")]
		public string BCSmpProductViewid { get; set; }
		[JsonProperty("CustomColQOH")]
		public double CustomColQOH { get; set; }
		[JsonProperty("ItemPrice")]
		public double ItemPrice { get; set; }

		public SOAPRelevantResults () {
		}
	}
}

