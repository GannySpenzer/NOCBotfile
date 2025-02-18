using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Worklight;
using Newtonsoft.Json;
using System.Json;

namespace SDiMobile
{
	public class partSearcher
	{
		public partSearcher ()
		{
		}

		public async Task<List<partItem>> search(partSearchParam p)
		{
			var parts = new List<partItem> ();

			WorklightResponse res = null;

			var pString = JsonConvert.SerializeObject (p);

			WorklightProcedureInvocationData invocationData = new WorklightProcedureInvocationData (
				                                                  "SDIMobileSearchSOAP", 
				                                                  "SearchService_Search", 
				                                                  new object [] { pString, "" });

			var wlc = worklightClientInstance.Instance;

			res = await wlc.wlcInstance.InvokeProcedure (invocationData);

			if ((res != null) && (res.Success)) {				
				SOAPbaseResponse resSOAP = null;

				JsonObject jsonObj = (JsonObject)res.ResponseJSON;

				if (jsonObj != null) {
					if (jsonObj.ContainsKey ("Envelope")) {
						if (!string.IsNullOrEmpty (jsonObj ["Envelope"].ToString ())) {
							resSOAP = Newtonsoft.Json.JsonConvert.DeserializeObject<SOAPbaseResponse> (jsonObj.ToString ());
						}
					}
				} //if (jsonObj != null)

				if ((resSOAP != null) && (resSOAP.isSuccessful)) {
					try {
						foreach (SOAPRelevantResults foundItem in resSOAP.Envelope.Body.SearchResponse.SearchResult.diffgram.Results.RelevantResults) {
							// create a new part item
							var itm = new partItem () {
								UOM = foundItem.BCSmpShippableUnitofMeasure,
								MfgId = foundItem.BCSmpManufacturerID,
								MfgPartNo = foundItem.BCSmpManufacturerPartNo,
								ItemDescription = foundItem.BCSmpItemdescription,
								MfgName = foundItem.BCSmpManufacturerName,
								ClassId = foundItem.BCSmpClassid,
								ClassName = foundItem.BCSmpClassName,
								ThumbnailImagefile = foundItem.BCSmpThumbnailImagefile,
								FullImagefile = foundItem.BCSmpFullImagefile,
								ItemId = foundItem.BCSmpItemid,
								SupplierShortDescription = foundItem.BCSmpSupplierShortDesc,
								PartNo = foundItem.BCSmpprefixedCustomerItemID,
								ProductViewId = foundItem.BCSmpProductViewid
							};
							// check what item description to show
							if (p.isProdDescRequired)
								itm.ItemDescription = foundItem.BCSmpOracleItemdescription;
							// default item image OR format : thumbnail image
							if (itm.ThumbnailImagefile == null)
								itm.ThumbnailImagefile = "noimage_new.png";
							else if (itm.ThumbnailImagefile.Trim ().Length == 0)
								itm.ThumbnailImagefile = "noimage_new.png";
							else
									//itm.ThumbnailImagefile = "http://cplus_prod.sdi.com:8080/ContentPlus/images///images755784//thumbnails//" + itm.ThumbnailImagefile;
									itm.ThumbnailImagefile = ((string)App.myInstance.Properties [App.URL_IMAGE_THUMBNAIL]) + itm.ThumbnailImagefile;
							// default item image OR format : full image
							if (itm.FullImagefile == null)
								itm.FullImagefile = "noimage_new.png";
							else if (itm.FullImagefile.Trim ().Length == 0)
								itm.FullImagefile = "noimage_new.png";
							else
									//itm.FullImagefile = "http://cplus_prod.sdi.com:8080/ContentPlus/images///images755784//" + itm.FullImagefile;
									itm.FullImagefile = ((string)App.myInstance.Properties [App.URL_IMAGE_FULL]) + itm.FullImagefile;
							// quantity on hand
							if ((foundItem.CustomColQOH != null) && (foundItem.CustomColQOH > 0))
								itm.QuantityOnHand = Math.Round ((decimal)foundItem.CustomColQOH, 2, MidpointRounding.AwayFromZero);
							else
								itm.QuantityOnHand = 0;
							// item price
							if ((foundItem.ItemPrice != null) && (foundItem.ItemPrice > 0))
								itm.Price = Math.Round ((decimal)foundItem.ItemPrice, 2, MidpointRounding.AwayFromZero);
							else
								itm.Price = 0;

							// add to collection
							parts.Add (itm);
						}
					} catch (Exception ex) {
						SQLiteDataAccess LocalDBObj = new SQLiteDataAccess ();
						LocalDBObj.InsertLogInLocalDB (ex, "PartSearcher-search");
					}
				}  //if ( (resSOAP != null) && (resSOAP.isSuccessful)) {

				jsonObj = null;
				resSOAP = null;
			}  //if ((res != null) && (res.Success)) {

			wlc = null;
			res = null;

			return parts;
		}  //public async Task<List<partItem>> search(partSearchParam p)
	}
}

