using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Web.Services.Protocols;
using OSVCService;
using SAErrorReload;
using SAErrorReload1;
using System.Data;
using System.ServiceModel.Channels;
using OSVCService;

namespace OSVCService
{
    public class Batcher : SAEData
    {

        //DateTime dateparse;

        int iLastVal = 0;
        int modValue = 1000;
        string strResp = "SUCCESS";

        int dtResponseRowsCount = 0;

        RightNowSyncPortClient _client;
        List<AccountInfo> _acctInfo = new List<AccountInfo>();

        // InitializeLogger start here
        public Logger m_oLogger;
        //string sLogPath = Environment.CurrentDirectory;

        DataTable dtResponse = new DataTable();

        //Set the API Username and Password
        public Batcher(string strauth, string strpass)
        {
            _client = new RightNowSyncPortClient();

            _client.ClientCredentials.UserName.UserName = strauth;
            _client.ClientCredentials.UserName.Password = strpass;
        }

        public void CreateBuyExpBatch(SAEData saeIn, Logger m_oLogger, out string sResponse)
        {
            ACTION_ITEM = saeIn.ACTION_ITEM;
            CLIENT = saeIn.CLIENT;
            DESCRIPTION = saeIn.DESCRIPTION;
            BUYER_ID = saeIn.BUYER_ID;
            ITEM  = saeIn.ITEM;
            STOCK_TYPE = saeIn.STOCK_TYPE;
            STAGE_STATUS = saeIn.STAGE_STATUS;
            MESSAGE = saeIn.MESSAGE;
            REQ_ID = saeIn.REQ_ID;
            REQ_LINE = saeIn.REQ_LINE;
            REQ_DATE = saeIn.REQ_DATE;
            VENDOR_ID = saeIn.VENDOR_ID;
            VENDOR_NAME = saeIn.VENDOR_NAME;
            REQUISITION_PRICE = saeIn.REQUISITION_PRICE;
            SOURCE_DATE = saeIn.SOURCE_DATE;
            TODAYS_DATE = saeIn.TODAYS_DATE;
            DATE_LAST_MODIFIED = saeIn.DATE_LAST_MODIFIED;
            DAYS_SINCE_SOURCE_DATE = saeIn.DAYS_SINCE_SOURCE_DATE;
            DAYS_SINCE_LAST_MODIFIED = saeIn.DAYS_SINCE_LAST_MODIFIED;
            EXCEPTION_DATE = saeIn.EXCEPTION_DATE;
            EXCEPTION_NUM_DAYS = saeIn.EXCEPTION_NUM_DAYS;
            SHIPTO_ID = saeIn.SHIPTO_ID;
            PRIORITY_FLAG = saeIn.PRIORITY_FLAG;
            SITE_NAME = saeIn.SITE_NAME;
            PS_URL = saeIn.PS_URL;
            BUYER_TEAM = saeIn.BUYER_TEAM;


            dtResponseRowsCount = ACTION_ITEM.Count();

            buildBatchRequestItems(m_oLogger);

            sResponse = strResp;
        }


        //You can have up to 100 items in a batch. The function that is part of the batch
        //can have up to 1000 objects so you can essentially have 100,000 records created in one call
        public void buildBatchRequestItems(Logger m_oLogger)
        {
            //Logger m_oLogger;
            //string sLogPath = Environment.CurrentDirectory;
            //if (!sLogPath.EndsWith(@"\"))
            //    sLogPath += @"\";
            //sLogPath += "Logs";
            //m_oLogger = new Logger(sLogPath, "SAErrorReload");
            m_oLogger.LogMessage("BatchSAError", "Entered buildBatchRequestItems class");

            try
            {


                BatchRequestItem[] requestItems = new BatchRequestItem[100];

                //for (int i = 0; i < ACTION_ITEMS.Count() / modValue; i++)
                int p = 0;
                while (iLastVal != dtResponseRowsCount)
                {
                    requestItems[p] = createNewBuyExpBatchRequest();
                    requestItems[p].CommitAfter = true;
                    requestItems[p].CommitAfterSpecified = true;
                    //requestItems[1] = createNewBuyExpBatchRequest();
                    //requestItems[1].CommitAfter = true;
                    //requestItems[1].CommitAfterSpecified = true;
                    //requestItems[2] = createNewBuyExpBatchRequest();
                    //requestItems[2].CommitAfter = true;
                    //requestItems[2].CommitAfterSpecified = true;
                    p += 1;
                }

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                submitBatch(requestItems, m_oLogger);
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                m_oLogger.LogMessage("SAErrorReload", "BatchSAError submitBatch Failure: " + ex.ToString());

            }
        }


        //Submit the batch and read the response if needed
        public void submitBatch(BatchRequestItem[] requestItems, Logger m_oLogger)
        {
            //Logger m_oLogger;
            //string sLogPath = Environment.CurrentDirectory;
            //if (!sLogPath.EndsWith(@"\"))
            //    sLogPath += @"\";
            //sLogPath += "Logs";
            //m_oLogger = new Logger(sLogPath, "SAErrorReload");
            m_oLogger.LogMessage("BatchSAError", "Entered submitBatch class");

            ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
            clientInfoHeader.AppID = "Batcher";

            APIAccessRequestHeader apiAccessRequestHeader = new APIAccessRequestHeader();

            BatchResponseItem[] batchRes;

            WSHttpBinding test = new WSHttpBinding();
            test.TextEncoding = UTF8Encoding.UTF8;


            m_oLogger.LogMessage("BatchSAError", "SAError submitBatch Starting Service Run.");
            _client.Batch(clientInfoHeader, apiAccessRequestHeader, requestItems, out batchRes);


            //If you need to get the response for each batch
            //CreateResponseMsg createResponseMsg0 = (CreateResponseMsg)batchRes[0].Item;
            //CreateResponseMsg createResponseMsg1 = (CreateResponseMsg)batchRes[1].Item;
            //CreateResponseMsg createResponseMsg2 = (CreateResponseMsg)batchRes[2].Item;

            //RNObject[] createdBuyExp = createResponseMsg0.RNObjectsResult;
            //foreach (RNObject obj in createdBuyExp)
            //{
            //    GenericObject newObj = (GenericObject)obj;
            //    System.Console.WriteLine("New BuyExp ID: " + newObj.ID.id);
            //    m_oLogger.LogMessage("ExpeditorReload", "SAErrorExcep submitBatch Response: " + newObj.ID.id.ToString());
            //}

            try
            {
                for (int i = 0; i < batchRes.Count(); i++)
                {

                    CreateResponseMsg createResponseMsg0 = (CreateResponseMsg)batchRes[i].Item;
                    RNObject[] createdBuyExp = createResponseMsg0.RNObjectsResult;
                    foreach (RNObject obj in createdBuyExp)
                    {
                        GenericObject newObj = (GenericObject)obj;
                        //System.Console.WriteLine("New BuyExp ID: " + newObj.ID.id);
                        //m_oLogger.LogMessage("ExpeditorReload", "SAErrorExcep submitBatch Response: " + newObj.ID.id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                m_oLogger.LogMessage("BatchSAError", "SAError submitBatch Failure: " + ex.ToString());
            }

        }
        //Create an generic object/record for each row that you want to insert
        //This is where you would set the fields for the table you are populating
        private GenericObject getBuyExpGenericObject(string action_item, string client,
            string description, string buyer_id, string item,
            string stock_type, string stage_status, string message, string req_id,
            string req_line, DateTime  req_date, string vendor_id,
            string vendor_name, string requisition_price, DateTime source_date,
            DateTime  todays_date, DateTime date_last_modified , int days_since_source_date,
            int days_since_last_modified, DateTime exception_date, int exception_num_days,
            string shipto_id, string priority_flag, string site_name,
            string ps_url, string buyer_team)
        {
            GenericObject go = new GenericObject();

            //Set the object type
            RNObjectType objType = new RNObjectType();
            objType.Namespace = "CO";
            objType.TypeName = "SAError";
            go.ObjectType = objType;

            List<GenericField> gfs = new List<GenericField>();
            gfs.Add(createGenericField("Action_Item", ItemsChoiceType.StringValue, action_item));
            gfs.Add(createGenericField("Client", ItemsChoiceType.StringValue, client));
            gfs.Add(createGenericField("Description", ItemsChoiceType.StringValue, description));
            gfs.Add(createGenericField("Item", ItemsChoiceType.StringValue, item));
            gfs.Add(createGenericField("Stock_Type", ItemsChoiceType.StringValue, stock_type ));
            gfs.Add(createGenericField("Stage_Status", ItemsChoiceType.StringValue , stage_status ));
            gfs.Add(createGenericField("Message", ItemsChoiceType.StringValue, message));
            gfs.Add(createGenericField("Req_ID", ItemsChoiceType.StringValue, req_id));
            gfs.Add(createGenericField("Req_Line", ItemsChoiceType.StringValue, req_line ));
            gfs.Add(createGenericField("Req_Date", ItemsChoiceType.DateValue , req_date ));
            gfs.Add(createGenericField("Vendor_ID", ItemsChoiceType.StringValue , vendor_id ));
            gfs.Add(createGenericField("Vendor_Name", ItemsChoiceType.StringValue, vendor_name ));
            gfs.Add(createGenericField("Requisition_Price", ItemsChoiceType.StringValue, requisition_price ));
            gfs.Add(createGenericField("Source_Date", ItemsChoiceType.DateValue , source_date ));
            gfs.Add(createGenericField("Todays_Date", ItemsChoiceType.DateValue , todays_date ));
            gfs.Add(createGenericField("Date_Last_Modified", ItemsChoiceType.DateValue , date_last_modified ));
            gfs.Add(createGenericField("Days_Since_Source_Date", ItemsChoiceType.IntegerValue , days_since_source_date ));
            gfs.Add(createGenericField("Days_Since_Last_Modified", ItemsChoiceType.IntegerValue , days_since_last_modified ));
            gfs.Add(createGenericField("Exception_Date", ItemsChoiceType.DateValue  , exception_date ));
            gfs.Add(createGenericField("Exception_Number_days", ItemsChoiceType.IntegerValue , exception_num_days ));
            gfs.Add(createGenericField("Shipto_ID", ItemsChoiceType.StringValue , shipto_id ));
            gfs.Add(createGenericField("Priority_Flag", ItemsChoiceType.StringValue, priority_flag ));
            gfs.Add(createGenericField("Site_Name", ItemsChoiceType.StringValue, site_name ));
            gfs.Add(createGenericField("PS_URL", ItemsChoiceType.StringValue, ps_url ));

            //if (price_req.Length > 10)
            //{
            //    Console.Write(price_req.ToString());
            //}
            //gfs.Add(createGenericField("Buyer_ID", ItemsChoiceType.NamedIDValue, buyer_id));
            //gfs.Add(createGenericField("Buyer_Team", ItemsChoiceType.NamedIDValue , buyer_team));
            try
            {
                if (buyer_id.Trim() != "")
                    gfs.Add(createAccountMenuFieldByName("Buyer_ID", buyer_id));
            }
            catch (Exception ex)
            {
            }

            try
            {
                if (buyer_team.Trim() != "")
                    gfs.Add(createAccountMenuFieldByName("Buyer_Team", buyer_team));
            }
            catch (Exception ex)
            {
            }


            go.GenericFields = gfs.ToArray();

            return go;
        }

        //Loop through your data in this function and set up to 1000 records per request.
        private BatchRequestItem createNewBuyExpBatchRequest()
        {
            BatchRequestItem createItem = new BatchRequestItem();

            CreateMsg createMsg = new CreateMsg();
            CreateProcessingOptions createProcessingOptions = new CreateProcessingOptions();
            createProcessingOptions.SuppressExternalEvents = false;
            createProcessingOptions.SuppressRules = false;
            createMsg.ProcessingOptions = createProcessingOptions;

            //Create a list of generic objects and return those to the batch processor
            //You can have up to 1000 objects

            GenericObject[] genArray = new GenericObject[modValue];
            int n = 0;

            try
            {
                //foreach (var item in ACTION_ITEMS)
                //while (iLastVal % modValue != 0 || (iLastVal / modValue) == 0 || iLastVal == ACTION_ITEMS.Count())
                while (iLastVal != dtResponseRowsCount)
                {
                    genArray[n] = getBuyExpGenericObject(ACTION_ITEM[iLastVal], CLIENT[iLastVal], DESCRIPTION [iLastVal], BUYER_ID [iLastVal], ITEM [iLastVal], STOCK_TYPE [iLastVal], STAGE_STATUS [iLastVal],
                        MESSAGE [iLastVal], REQ_ID [iLastVal], REQ_LINE [iLastVal], REQ_DATE [iLastVal], VENDOR_ID [iLastVal], VENDOR_NAME [iLastVal], REQUISITION_PRICE [iLastVal],
                        SOURCE_DATE [iLastVal], TODAYS_DATE [iLastVal], DATE_LAST_MODIFIED [iLastVal], DAYS_SINCE_SOURCE_DATE [iLastVal], DAYS_SINCE_LAST_MODIFIED [iLastVal], EXCEPTION_DATE [iLastVal], EXCEPTION_NUM_DAYS [iLastVal],
                        SHIPTO_ID [iLastVal], PRIORITY_FLAG [iLastVal], SITE_NAME [iLastVal], PS_URL [iLastVal], BUYER_TEAM [iLastVal]);
                    //GenericObject go1 = getBuyExpGenericObject("Test1","Test1",DateTime.Now,"Test1","Test1","Test1","Test1","Test1","Test1","Test1","Test1","Test1","Test1","Test1",DateTime.Now,"Test1","Test1",0,"Test1","Test1","Test1");
                    //GenericObject go2 = getBuyExpGenericObject("Test2", "Test2", DateTime.Now, "Test2", "Test2", "Test2", "Test2", "Test2", "Test2", "Test2", "Test2", "Test2", "Test2", "Test2", DateTime.Now, "Test2", "Test2", 0, "Test2", "Test2", "Test2");

                    n += 1;
                    iLastVal += 1;

                    if (n == modValue)
                        break;

                }

                RNObject[] createBuyerExp = new RNObject[n];
                RNObject createBuyerExpOne = new RNObject();
                for (int i = 0; i < n; i++)
                {
                    createBuyerExpOne = genArray[i];
                    createBuyerExp[i] = (RNObject)createBuyerExpOne;
                }


                createMsg.RNObjects = createBuyerExp;

                createItem.Item = createMsg;

                return createItem;
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                return createItem;
            }

        }

        //private void callRNObject(GenericObject[] arrays )
        //{
        //   RNObject[] createBuyerExp = new RNObject[] { arrays };
        //}

        //Helper function used by the API to create the fields
        private GenericField createGenericField(string Name, ItemsChoiceType itemsChoiceType, object Value)
        {
            GenericField gf = new GenericField();
            gf.name = Name;
            gf.DataValue = new DataValue();
            gf.DataValue.ItemsElementName = new ItemsChoiceType[] { itemsChoiceType };
            gf.DataValue.Items = new object[] { Value };
            return gf;
        }

        private GenericField createMenuGenericField(string fieldName, long fieldID)
        {
            GenericField gf = new GenericField();

            NamedID fid = new NamedID();
            fid.ID = new ID();
            fid.ID.idSpecified = true;
            fid.ID.id = fieldID;

            gf = createGenericField(fieldName, ItemsChoiceType.NamedIDValue, fid);

            return gf;
        }

        private GenericField createAccountMenuFieldByName(string fieldName, string accountName)
        {

            GenericField gf = new GenericField();


            if (_acctInfo.Count < 1)
            {
                QueryAccountObjectsSample();
            }

            AccountInfo ai = _acctInfo.Find(a => a.LookupName == accountName.Replace(".", " "));

            gf = createMenuGenericField(fieldName, ai.id);

            return gf;
        }

        public void QueryAccountObjectsSample()
        {
            ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
            clientInfoHeader.AppID = "Basic Objects Sample";
            APIAccessRequestHeader apiAccessRequestHeader = new APIAccessRequestHeader();

            String queryString = "Select Account FROM Account;";

            Account accountTemplate = new Account();

            RNObject[] objectTemplates = new RNObject[] { accountTemplate };

            try
            {
                QueryResultData[] queryObjects;
                _client.QueryObjects(clientInfoHeader, apiAccessRequestHeader, queryString, objectTemplates, 10000, out queryObjects);
                RNObject[] rnObjects = queryObjects[0].RNObjectsResult;

                foreach (RNObject obj in rnObjects)
                {
                    Account acct = (Account)obj;

                    _acctInfo.Add(new AccountInfo() { id = acct.ID.id, LookupName = acct.LookupName });
                    System.Console.WriteLine("Account ID: " + acct.ID.id + " Name: " + acct.LookupName);

                }

            }
            catch (FaultException ex)
            {
                Console.WriteLine(ex.Code);
                Console.WriteLine(ex.Message);
            }
            //catch (SoapException ex)
            //{
            //    Console.WriteLine(ex.Code);
            //    Console.WriteLine(ex.Message);
            //}
        }
    }

    public class AccountInfo
    {
        public long id { get; set; }
        public string LookupName { get; set; }
    }
}


