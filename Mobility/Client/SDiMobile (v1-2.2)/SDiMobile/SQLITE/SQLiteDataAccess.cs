using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;

namespace SDiMobile
{
	[Table("UserPrivilege")]
	public class UserPrivilege
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id
		{
			get;
			set;
		}

		[Column("PrivType")]
		public string PrivType { get; set; }
		[Column("PrivName")]
		public string PrivName { get; set; }
	}

	[Table("UserDetail")]
	public class UserDetailBO
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id
		{
			get;
			set;
		}

		[Column("UserId")]
		public string UserId { get; set; }

		[Column("Name")]
		public string Name { get; set; }

		[Column("BusinessUnitId")]
		public string BusinessUnitId { get; set; }

		[Column("BusinessUnitName")]
		public string BusinessUnitName { get; set; }

		[Column("Phone")]
		public string Phone { get; set; }

		[Column("Email")]
		public string Email { get; set; }

		[Column("ProductViewId")]
		public int ProductViewId { get; set; }

		[Column("UniqueUserId")]
		public int UniqueUserId { get; set; }

		[Column("CustomerId")]
		public string CustomerId { get; set; }

//		[Column("Privs")]
//		public List<userPriv> Privs { get; set; }

		[Column("Message")]
		public string Message { get; set; }

		[Column("Password")]
		public string Password { get; set; }

		[Column("IsLoggedIn")]
		public bool IsLoggedIn { get; set; }

		[Column("LastInfoSyncDTTM")]
		public DateTime LastInfoSyncDTTM { get; set; }

		[Column("DeviceID")]
		public string DeviceID { get; set; }

	}
	public class SQLiteDataAccess
	{
		/// <summary>
		/// To check whether the table exists in the database
		/// </summary>
		/// <typeparam name="T">The table which is to be checked</typeparam>
		/// <param name="DatabasePath">Local Path of the database location</param>
		/// <returns>whether the table exists</returns>
		private bool TableExists<T>(SQLiteConnection DatabasePath)
		{
			try {
				SQLite.TableMapping map = new TableMapping (typeof(T)); // Instead of mapping to a specific table just map the whole database type
				object[] ps = new object[0]; // An empty parameters object since I never worked out how to use it properly! (At least I'm honest)

				Int32 tableCount = DatabasePath.Query (map, "SELECT * FROM sqlite_master WHERE type = 'table' AND name = '" + map.TableName + "'", ps).Count; // Executes the query from which we can count the results
				if (tableCount == 0) {
					return false;
				} else if (tableCount == 1) {
					return true;
				} else {
					throw new Exception ("More than one table by the name of " + map.TableName + " exists in the database.", null);
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// To create an item in localDB
		/// </summary>
		/// <param name="UserObj">User details record to be created in the database</param>
		public void CreateItemInLocalDB(UserDetailBO UserObj,List<userPriv> privillege)
		{
			try {
				string databasepath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/SDIOrdering.db3";
				SQLiteConnection database = new SQLiteConnection (databasepath);
				if (TableExists<UserDetailBO> (database)) {
					//database.DeleteAll<UserDetailBO> ();
					database.Execute ("DROP TABLE IF EXISTS " + "UserDetail");
				}

				database.CreateTable<UserDetailBO> ();
				if (TableExists<UserPrivilege> (database)) {
					database.DeleteAll<UserPrivilege> ();
				} else {
					database.CreateTable<UserPrivilege> ();
				}

				database.Insert (UserObj);
				if (privillege != null)
					foreach (var priv in privillege) {
						database.Insert (new UserPrivilege{ PrivName = priv.PrivName, PrivType = priv.PrivType });
					}
			} catch (Exception ex) {
				throw ex;
			}
		}

		/// <summary>
		/// To read the record present in the local database
		/// </summary>
		/// <returns>The user object containing current user details </returns>
		public UserDetailBO ReadItemInLocalDB()
		{
			try {
				string databasepath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/SDIOrdering.db3";
				using (SQLiteConnection database = new SQLiteConnection (databasepath)) {
					if (TableExists<UserDetailBO> (database))
						return database.Table<UserDetailBO> ().FirstOrDefault ();
					else
						return null;
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		public List<userPriv> ReadUserPrivInLocalDB()
		{
			try {
				string databasepath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/SDIOrdering.db3";
				using (SQLiteConnection database = new SQLiteConnection (databasepath)) {
					if (TableExists<UserPrivilege> (database)) {
						var userprivlist = (database.Table<UserPrivilege> ().Select (x => new userPriv {
							PrivName = x.PrivName,
							PrivType = x.PrivType
						}).ToList ());
						return userprivlist;
					} else
						return null;
				}
			} catch (Exception ex) {	
				throw ex;
			}
		}

		public void UpdateItemInLocalDB(UserDetailBO usr)
		{
			try {
				string databasepath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/SDIOrdering.db3";
				using (SQLiteConnection database = new SQLiteConnection (databasepath)) {
					UserDetailBO LocaldUser = database.Table<UserDetailBO> ().FirstOrDefault ();
					LocaldUser.BusinessUnitId = usr.BusinessUnitId;
					LocaldUser.BusinessUnitName = usr.BusinessUnitName;
					LocaldUser.CustomerId = usr.CustomerId;
					LocaldUser.Email = usr.Email;
					LocaldUser.IsLoggedIn = true;
					LocaldUser.LastInfoSyncDTTM = usr.LastInfoSyncDTTM;
					LocaldUser.Message = usr.Message;
					LocaldUser.Name = usr.Name;
					LocaldUser.Password = usr.Password;
					LocaldUser.Phone = usr.Phone;
					//LocaldUser.Privs = usr.Privs;
					LocaldUser.ProductViewId = usr.ProductViewId;
					LocaldUser.UniqueUserId = usr.UniqueUserId;
					LocaldUser.UserId = usr.UserId;
					LocaldUser.DeviceID = worklightClientInstance.deviceid;
					database.Commit ();
				}
			} catch (Exception ex) {
				throw ex;
			}
		}

		public bool DeleteLocalDatabase()
		{
			bool result = false;
			try
			{
				string databasepath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/SDIOrdering.db3";
				using (SQLiteConnection database = new SQLiteConnection(databasepath))
				{
					if (TableExists<UserDetailBO>(database))
					{
						database.Execute("DROP TABLE IF EXISTS " + "UserDetail");
						result = true;
					}
					if (TableExists<UserPrivilege>(database))
					{
						database.Execute("DROP TABLE IF EXISTS " + "UserPrivilege");
						result = true;
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			return result;
		}
		public void InsertLogInLocalDB(Exception exp,string screen)
		{
			try {
				string databasepath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/SDIOrdering.db3";
				SQLiteConnection database = new SQLiteConnection (databasepath);
				if (TableExists<LoggingBO> (database)) {
				} else {
					database.CreateTable<LoggingBO> ();
				}

				var user = ReadItemInLocalDB ();
				LoggingBO log = new LoggingBO ();
				log.BuisnessUnit = user.BusinessUnitId;
				log.DeviceID = user.DeviceID;
				log.ExceptionMessage = exp.Message;
				log.LogTime = DateTime.Now;
				log.Screen = screen;
				log.StackTrace = exp.StackTrace;
				log.UserID = user.UserId;

				database.Insert (log);
			} catch (Exception ex) {
				throw ex;
			}
		}
		public List<LoggingBO> ReadAllLogsInLocalDB()
		{
			try {
				string databasepath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/SDIOrdering.db3";
				using (SQLiteConnection database = new SQLiteConnection (databasepath)) {
					if (TableExists<LoggingBO> (database))
						return database.Table<LoggingBO> ().ToList ();
					else
						return null;
				}
			} catch (Exception ex) {
				throw ex;
			}
		}
		public bool DeleteLocalLogs()
		{
			bool result = false;
			try {
				string databasepath = Environment.GetFolderPath (Environment.SpecialFolder.Personal) + "/SDIOrdering.db3";
				using (SQLiteConnection database = new SQLiteConnection (databasepath)) {
					if (TableExists<LoggingBO> (database)) {
						database.Execute ("DROP TABLE IF EXISTS " + "Logging");
						result = true;
					}
				}
			} catch (Exception ex) {
				throw ex;
			}
			return result;
		}
	}
}

