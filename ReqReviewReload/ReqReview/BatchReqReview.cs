using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Web.Services.Protocols;
using OSVCService;
using ReqReviewReload;
using ReqReviewReload1;
using System.Data;
using System.ServiceModel.Channels;
using OSVCService;

namespace OSVCService 
{
    public class Batcher : REQData 
    {

        //public List<string> ACTION_ITEM = new List<string>();
        //public List<string> CLIENT = new List<string>();
        //public List<string> SITE_NAME = new List<string>();
        //public List<string> SHIP_TO_ID = new List<string>();
        //public List<string> REQUESTOR_ID = new List<string>();
        //public List<string> BUYER_ID = new List<string>();
        //public List<DateTime> REQUISITION_DATE = new List<DateTime>();
        //public List<string> REQUISITION_ID = new List<string>();
        //public List<string> LINE_NUMBER = new List<string>();
        //public List<string> INVENTORY_ITEM_ID = new List<string>();
        //public List<int> QTY_REQ = new List<int>();
        //public List<int> QTY_OPEN = new List<int>();
        //public List<string> UNIT_OF_MEASURE = new List<string>();
        //public List<string> PRICE_REQ = new List<string>();
        //public List<string> SELL_PRICE = new List<string>();
        //public List<string> VENDOR_ID= new List<string>();
        //public List<string> VENDOR_NAME = new List<string>();
        //public List<string> PROBLEM_CODE = new List<string>();
        //public List<string> REQ_HOLD_FLAG = new List<string>();
        //public List<DateTime> REQ_LINE_ADD_DATE= new List<DateTime>();
        //public List<string> MANUFACTURER= new List<string>();
        //public List<string> MANUFACTURER_PART_NUMBER = new List<string>();
        //public List<string> DESCRIPTION = new List<string>();
        //public List<string> REQUISITION_COMMENTS= new List<string>();
        //public List<string> CHARGE_CD= new List<string>();
        //public List<string> WORKORDER = new List<string>();
        //public List<string> PRIORITY_FLAG = new List<string>();
        //public List<string> STATUS_AGE = new List<string>();
        //public List<string> PS_URL = new List<string>();
        //public List<string> BUYER_TEAM = new List<string>();

        DateTime dateparse;

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

        public void CreateBuyExpBatch(REQData reqIn, Logger m_oLogger, out string sResponse)
        {
            ACTION_ITEM = reqIn.ACTION_ITEM;
            CLIENT = reqIn.CLIENT;
            SITE_NAME = reqIn.SITE_NAME;
            SHIP_TO_ID  = reqIn.SHIP_TO_ID ;
            REQUESTOR_ID= reqIn.REQUESTOR_ID;
            BUYER_ID = reqIn.BUYER_ID;
            REQUISITION_DATE = reqIn.REQUISITION_DATE ;
            REQUISITION_ID = reqIn.REQUISITION_ID;
            LINE_NUMBER = reqIn.LINE_NUMBER;
            INVENTORY_ITEM_ID = reqIn.INVENTORY_ITEM_ID ;
            QTY_REQ = reqIn.QTY_REQ ;
            QTY_OPEN = reqIn.QTY_OPEN ;
            UNIT_OF_MEASURE = reqIn.UNIT_OF_MEASURE ;
            PRICE_REQ= reqIn.PRICE_REQ ;
            SELL_PRICE  = reqIn.SELL_PRICE ;
            VENDOR_ID = reqIn.VENDOR_ID;
            VENDOR_NAME = reqIn.VENDOR_NAME ;
            PROBLEM_CODE = reqIn.PROBLEM_CODE ;
            REQ_HOLD_FLAG= reqIn.REQ_HOLD_FLAG ;
            REQ_LINE_ADD_DATE = reqIn.REQ_LINE_ADD_DATE ;
            MANUFACTURER = reqIn.MANUFACTURER ;
            MANUFACTURER_PART_NUMBER = reqIn.MANUFACTURER_PART_NUMBER ;
            DESCRIPTION = reqIn.DESCRIPTION ;
            REQUISITION_COMMENTS = reqIn.REQUISITION_COMMENTS ;
            CHARGE_CD = reqIn.CHARGE_CD ;
            WORKORDER = reqIn.WORKORDER ;
            PRIORITY_FLAG  = reqIn.PRIORITY_FLAG ;
            STATUS_AGE = reqIn.STATUS_AGE ;
            PS_URL = reqIn.PS_URL ;
            BUYER_TEAM = reqIn.BUYER_TEAM ;

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
            //m_oLogger = new Logger(sLogPath, "ReqReviewReload");
            m_oLogger.LogMessage("ReqReviewExcep", "Entered ReqReviewExcep class");

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
                m_oLogger.LogMessage("ReqReviewReload", "ReqReviewExcep submitBatch Failure: " + ex.ToString());

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
            //m_oLogger = new Logger(sLogPath, "ReqReviewReload");
            m_oLogger.LogMessage("ReqReviewExcep", "Entered submitBatch class");

            ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
            clientInfoHeader.AppID = "Batcher";

            APIAccessRequestHeader apiAccessRequestHeader = new APIAccessRequestHeader();

            BatchResponseItem[] batchRes;

            WSHttpBinding test = new WSHttpBinding();
            test.TextEncoding = UTF8Encoding.UTF8;


            m_oLogger.LogMessage("ReqReviewReload", "ReqReviewExcep submitBatch Starting Service Run.");
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
            //    m_oLogger.LogMessage("ExpeditorReload", "ReqReviewExcep submitBatch Response: " + newObj.ID.id.ToString());
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
                        //m_oLogger.LogMessage("ExpeditorReload", "ReqReviewExcep submitBatch Response: " + newObj.ID.id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                m_oLogger.LogMessage("ReqReviewReload", "ReqReviewExcep submitBatch Failure: " + ex.ToString());
            }

        }
        //Create an generic object/record for each row that you want to insert
        //This is where you would set the fields for the table you are populating
        private GenericObject getBuyExpGenericObject(string action_item, string client,
            string site_name, string ship_to_id, string requestor_id,
            string buyer_id, DateTime requisition_date, string requisition_id, string line_number,
            string inventory_item_id, int qty_req, int qty_open,
            string unit_of_measure, string price_req, string sell_price,
            string vendor_id, string vendor_name, string problem_code,
            string req_hold_flag, DateTime req_line_add_date, string manufacturer,
            string manufacturer_part_number, string description, string requisition_comments,
            string charge_cd, string workorder, string priority_flag,
            int status_age, string ps_url, string buyer_team)
        {
            GenericObject go = new GenericObject();

            //Set the object type
            RNObjectType objType = new RNObjectType();
            objType.Namespace = "CO";
            objType.TypeName = "ReqReview";
            go.ObjectType = objType;
            
            List<GenericField> gfs = new List<GenericField>();
            gfs.Add(createGenericField("Action_Item", ItemsChoiceType.StringValue, action_item));
            gfs.Add(createGenericField("Client", ItemsChoiceType.StringValue, client));
            gfs.Add(createGenericField("Site_Name", ItemsChoiceType.StringValue, site_name));
            gfs.Add(createGenericField("Ship_to_ID", ItemsChoiceType.StringValue, ship_to_id ));
            gfs.Add(createGenericField("Requestor_ID", ItemsChoiceType.StringValue, requestor_id));
            gfs.Add(createGenericField("Requisition_Date", ItemsChoiceType.DateValue, requisition_date));
            gfs.Add(createGenericField("Requisition_ID", ItemsChoiceType.StringValue, requisition_id  ));
            gfs.Add(createGenericField("Line_Number", ItemsChoiceType.StringValue, line_number ));
            gfs.Add(createGenericField("Inventory_Item_ID", ItemsChoiceType.StringValue , inventory_item_id ));
            gfs.Add(createGenericField("Qty_Req", ItemsChoiceType.IntegerValue , qty_req ));
            gfs.Add(createGenericField("Qty_open", ItemsChoiceType.IntegerValue , qty_open ));
            gfs.Add(createGenericField("Unit_of_measure", ItemsChoiceType.StringValue, unit_of_measure ));
            gfs.Add(createGenericField("Price_Req", ItemsChoiceType.StringValue, price_req ));
            gfs.Add(createGenericField("Sell_Price", ItemsChoiceType.StringValue , sell_price ));
            gfs.Add(createGenericField("Vendor_ID", ItemsChoiceType.StringValue , vendor_id));
            gfs.Add(createGenericField("Vendor_Name", ItemsChoiceType.StringValue , vendor_name ));
            gfs.Add(createGenericField("Problem_Code", ItemsChoiceType.StringValue , problem_code ));
            gfs.Add(createGenericField("Req_Hold_Flag", ItemsChoiceType.StringValue, req_hold_flag ));
            gfs.Add(createGenericField("Req_Line_Add_Date", ItemsChoiceType.DateTimeValue , req_line_add_date ));
            gfs.Add(createGenericField("Manufacturer", ItemsChoiceType.StringValue, manufacturer ));
            gfs.Add(createGenericField("Manufacturer_part_number", ItemsChoiceType.StringValue, manufacturer_part_number ));
            gfs.Add(createGenericField("Description", ItemsChoiceType.StringValue , description));
            gfs.Add(createGenericField("Requisition_Comments", ItemsChoiceType.StringValue , requisition_comments ));
            gfs.Add(createGenericField("Charge_CD", ItemsChoiceType.StringValue, charge_cd));
            gfs.Add(createGenericField("Workorder", ItemsChoiceType.StringValue, workorder ));
            gfs.Add(createGenericField("Priority_Flag", ItemsChoiceType.StringValue, priority_flag ));
            gfs.Add(createGenericField("Status_Age", ItemsChoiceType.IntegerValue , status_age ));
            gfs.Add(createGenericField("PS_URL", ItemsChoiceType.StringValue, ps_url));

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
                    genArray[n] = getBuyExpGenericObject(ACTION_ITEM[iLastVal], CLIENT[iLastVal], SITE_NAME[iLastVal], SHIP_TO_ID [iLastVal], REQUESTOR_ID [iLastVal], BUYER_ID[iLastVal], REQUISITION_DATE [iLastVal],
                        REQUISITION_ID [iLastVal], LINE_NUMBER [iLastVal], INVENTORY_ITEM_ID [iLastVal], QTY_REQ [iLastVal], QTY_OPEN [iLastVal], UNIT_OF_MEASURE[iLastVal], PRICE_REQ[iLastVal],
                        SELL_PRICE[iLastVal], VENDOR_ID[iLastVal], VENDOR_NAME[iLastVal], PROBLEM_CODE[iLastVal], REQ_HOLD_FLAG [iLastVal], REQ_LINE_ADD_DATE [iLastVal], MANUFACTURER [iLastVal],
                        MANUFACTURER_PART_NUMBER [iLastVal], DESCRIPTION[iLastVal], REQUISITION_COMMENTS [iLastVal], CHARGE_CD[iLastVal], WORKORDER[iLastVal], PRIORITY_FLAG[iLastVal],
                        STATUS_AGE[iLastVal], PS_URL[iLastVal], BUYER_TEAM[iLastVal]);
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


