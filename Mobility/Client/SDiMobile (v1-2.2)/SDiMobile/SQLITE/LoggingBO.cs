using System;
using SQLite;

namespace SDiMobile
{
	[Table("Logging")]
	public class LoggingBO
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int LogID
		{
			get;
			set;
		}

		[Column("UserID")]
		public string UserID { get; set; }
		[Column("BuisnessUnit")]
		public string BuisnessUnit { get; set; }
		[Column("DeviceID")]
		public string DeviceID { get; set; }
		[Column("ExceptionMessage")]
		public string ExceptionMessage { get; set; }
		[Column("StackTrace")]
		public string StackTrace { get; set; }
		[Column("Screen")]
		public string Screen { get; set; }
		[Column("LogTime")]
		public DateTime LogTime { get; set; }

	}
}

