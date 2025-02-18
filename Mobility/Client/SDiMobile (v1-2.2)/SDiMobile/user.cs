using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;


namespace SDiMobile
{
	[Foundation.Preserve (AllMembers = true)]
    public class user
    {
		
        [JsonProperty("ISA_EMPLOYEE_ID")]
        public string UserId { get; set; }

        public string Password { get; set; }

        [JsonProperty("BUSINESS_UNIT")]
        public string BusinessUnit { get; set; }

        [JsonProperty("ISA_EMPLOYEE_NAME")]
        public string DisplayName { get; set; }

        public bool IsAuthenticated { get; set; }

		public string Phone { get; set; }

		public string Email { get; set; }

		public string BusinessUnitName { get; set; }

		public int ProductViewId { get; set; }

        public user()
        {

        }

        public user(string userId, string pw, string bu, string displayName, bool isAuth)
        {
            this.UserId = userId;
            this.Password = pw;
            this.BusinessUnit = bu;
            this.DisplayName = displayName;
            this.IsAuthenticated = isAuth;
        }

    }
}
