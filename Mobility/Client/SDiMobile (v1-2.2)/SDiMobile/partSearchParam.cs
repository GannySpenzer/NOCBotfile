using System;

namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
	public class partSearchParam
	{
		// actual parameter names for SearchService_Search method
		public int startPage;
		public int itemCount;
		public int prodviewID;
		public string queryText;
		public bool isProdDescRequired;
		public string userID;
		public string businessUnit;
		public string deviceID;

		public partSearchParam ()
		{
			SQLiteDataAccess sqliteobj = new SQLiteDataAccess();
			var localuser = sqliteobj.ReadItemInLocalDB ();
			if (localuser == null) {
				localuser = new UserDetailBO ();
			}
			this.userID = localuser.UserId;
			this.businessUnit = localuser.BusinessUnitId;
			this.deviceID = localuser.DeviceID;
		}

		public partSearchParam(int itemIndex, int itemCount, int productViewId, string searchString, bool isClientDesc) {
			InitMembers(itemIndex, itemCount, productViewId, searchString, isClientDesc);
		}

		private void InitMembers(
			int itemIndex,
			int itemCnt,
			int productViewId,
			string searchString, 
			bool isClientDescription) {
			this.startPage = itemIndex;
			this.itemCount = itemCnt;
			this.prodviewID = productViewId;
			this.queryText = searchString;
			this.isProdDescRequired = isClientDescription;
		}
	}
}

