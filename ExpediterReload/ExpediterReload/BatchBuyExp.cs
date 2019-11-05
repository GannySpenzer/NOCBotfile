using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Web.Services.Protocols;
using OSVCService;
using ExpediterReload;
using ExpediterReload1;
using System.Data;

namespace OSVCService 
{
    public class Batcher
    {

        List<string> ACTION_ITEMS = new List<string>();
        List<string> BUSINESS_UNIT = new List<string>();
        List<string> BUYER_ID = new List<string>();
        List<string> BUYER_TEAM = new List<string>();
        List<string> CLIENT = new List<string>();
        List<string> DESCRIPTION = new List<string>();
        List<string> EXPEDITING_COMMENTS = new List<string>();
        List<string> INVENTORY_BUSINESS_UNIT = new List<string>();
        List<string> ITEM = new List<string>();
        //List<string> LAST_COMMENT_DATE = new List<string>();
        List<DateTime> LAST_COMMENT_DATE = new List<DateTime>();
        List<string> LAST_OPERATOR = new List<string>();
        List<string> LINE_NUMBER = new List<string>();
        //List<string> PO_DATE = new List<string>();
        List<DateTime> PO_DATE = new List<DateTime>();
        List<string> PO_ID = new List<string>();
        List<string> PS_URL = new List<string>();
        List<string> PRIORITY_FLAG = new List<string>();
        List<string> PROBLEM_CODE = new List<string>();
        List<string> SITE_NAME = new List<string>();
        List<string> STATUS_AGE = new List<string>();
        List<string> VENDOR_ID = new List<string>();
        List<string> VENDOR_NAME = new List<string>();
        DateTime dateparse;

        int iLastVal = 0;
        int modValue = 5; //test
        string strResp = "SUCCESS";

        RightNowSyncPortClient _client;

        // InitializeLogger start here
        Logger m_oLogger;
        string sLogPath = Environment.CurrentDirectory;

        DataTable dtResponse = new DataTable();

        //Set the API Username and Password
        public Batcher()
        {
            _client = new RightNowSyncPortClient();
            _client.ClientCredentials.UserName.UserName = "API_User";
            _client.ClientCredentials.UserName.Password = "qD28#!ddnq";
        }

        public void CreateBuyExpBatch(out string sResponse)
        {
            getData();
            if (ACTION_ITEMS.Count > 0)
            {
                buildBatchRequestItems();
            }

            sResponse = strResp;
        }

        public void getData()
        {
            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "ExpediterReload");
            m_oLogger.LogMessage("BatchBuyExp", "Entered BatchBuyExp class");


            //STEP #3 - QUERY TABLE AND POST NEW DATA 
            m_oLogger.LogMessage("ExpediterReload", "Query table started");
            ExpediterReloadDAL objGetExpediterReloadDAL = new ExpediterReloadDAL();
            dtResponse = objGetExpediterReloadDAL.getExpediterData(m_oLogger);
            if (dtResponse.Rows.Count == 0)
            {
                m_oLogger.LogMessage("ExpediterReload", "Query returned no records.");
                return;
            }
            else
                m_oLogger.LogMessage("ExpediterReload", "POST ExpediterReload data started.");
            for (int i = 0; i < dtResponse.Rows.Count; i++)
            {
                DataRow rowInit;
                rowInit = dtResponse.Rows[i];

                try
                {
                    ACTION_ITEMS.Add(rowInit["ACTION_ITEMS"].ToString());
                    BUSINESS_UNIT.Add(rowInit["BUSINESS_UNIT"].ToString());
                    BUYER_ID.Add(rowInit["BUYER_ID"].ToString());
                    BUYER_TEAM.Add("Test");                                     //?????
                    CLIENT.Add(rowInit["CLIENT"].ToString());
                    DESCRIPTION.Add(rowInit["DESCRIPTION"].ToString());
                    EXPEDITING_COMMENTS.Add(rowInit["EXPEDITING_COMMENTS"].ToString());
                    INVENTORY_BUSINESS_UNIT.Add(rowInit["BUSINESS_UNIT_IN"].ToString());
                    ITEM.Add(rowInit["ITEM"].ToString());

                    string LAST_COMMENT_DATEtest = rowInit["LAST_COMMENT_DATE"].ToString();
                    dateparse = DateTime.Parse(LAST_COMMENT_DATEtest);
                    //LAST_COMMENT_DATE.Add(dateparse.ToString("yyyy-MM-ddTHH:mm:ss.000Z"));
                    LAST_COMMENT_DATE.Add(dateparse);

                    LAST_OPERATOR.Add(rowInit["LAST_OPERATOR"].ToString());
                    LINE_NUMBER.Add(rowInit["LINE_NBR"].ToString());

                    string PO_DATEtest = rowInit["PO_DATE"].ToString();
                    dateparse = DateTime.Parse(PO_DATEtest);
                    //PO_DATE.Add(dateparse.ToString("yyyy-MM-ddTHH:mm:ss.000Z"));
                    PO_DATE.Add(dateparse);

                    PO_ID.Add(rowInit["PO_ID"].ToString());
                    PS_URL.Add(" ");                                        //?????
                    PRIORITY_FLAG.Add("Test");                                 //?????
                    PROBLEM_CODE.Add(rowInit["PROBLEM_CODE"].ToString());
                    SITE_NAME.Add("Test");                                     //?????
                    STATUS_AGE.Add("10");
                    VENDOR_ID.Add(rowInit["VENDOR_ID"].ToString());
                    VENDOR_NAME.Add(rowInit["VENDOR"].ToString());
                }
                catch (Exception ex)
                {
                    m_oLogger.LogMessage("ExpeditorReload", "Error trying to parse data at line " + i.ToString(), ex);

                }

            }

            m_oLogger.LogMessage("ExpediterReload", "Query table and parse successful.");



        }

        //You can have up to 100 items in a batch. The function that is part of the batch
        //can have up to 1000 objects so you can essentially have 100,000 records created in one call
        public void buildBatchRequestItems()
        {
            BatchRequestItem[] requestItems = new BatchRequestItem[100];

            //for (int i = 0; i < ACTION_ITEMS.Count() / modValue; i++)
            int i = 0;
            while (iLastVal != ACTION_ITEMS.Count())
            {
                requestItems[i] = createNewBuyExpBatchRequest();
                requestItems[i].CommitAfter = true;
                requestItems[i].CommitAfterSpecified = true;
                //requestItems[1] = createNewBuyExpBatchRequest();
                //requestItems[1].CommitAfter = true;
                //requestItems[1].CommitAfterSpecified = true;
                //requestItems[2] = createNewBuyExpBatchRequest();
                //requestItems[2].CommitAfter = true;
                //requestItems[2].CommitAfterSpecified = true;
                i += 1;
            }

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            submitBatch(requestItems);

        }


        //Submit the batch and read the response if needed
        public void submitBatch(BatchRequestItem[] requestItems)
        {

            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "ExpediterReload");
            m_oLogger.LogMessage("BatchBuyExp", "Entered submitBatch class");

            ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
            clientInfoHeader.AppID = "Batcher";

            APIAccessRequestHeader apiAccessRequestHeader = new APIAccessRequestHeader();

            BatchResponseItem[] batchRes;

            m_oLogger.LogMessage("ExpeditorReload", "BatchBuyExp submitBatch Starting Service Run.");
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
            //    m_oLogger.LogMessage("ExpeditorReload", "BatchBuyExp submitBatch Response: " + newObj.ID.id.ToString());
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
                        System.Console.WriteLine("New BuyExp ID: " + newObj.ID.id);
                        m_oLogger.LogMessage("ExpeditorReload", "BatchBuyExp submitBatch Response: " + newObj.ID.id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                m_oLogger.LogMessage("ExpeditorReload", "BatchBuyExp submitBatch Failure: " + ex.ToString());
            }

        }
        //Create an generic object/record for each row that you want to insert
        //This is where you would set the fields for the table you are populating
        private GenericObject getBuyExpGenericObject(string Action_items,
            string Client, DateTime PO_Date, string Business_Unit, string PO_ID,
            string Line_Number, string Item, string Description, string Problem_Code,
            string Expediting_Comments, string Buyer_ID, string Vendor_ID,
            string Vendor_Name, string Last_Operator, DateTime Last_Comment_Date,
            string Inventory_Business_Unit, string Priority_Flag, int Status_Age,
            string Site_Name, string Buyer_Team, string PS_URL)
        {
            GenericObject go = new GenericObject();

            //Set the object type
            RNObjectType objType = new RNObjectType();
            objType.Namespace = "CO";
            objType.TypeName = "BuyExp";
            go.ObjectType = objType;

            List<GenericField> gfs = new List<GenericField>();
            gfs.Add(createGenericField("Action_Items", ItemsChoiceType.StringValue, Action_items));
            gfs.Add(createGenericField("Client", ItemsChoiceType.StringValue, Client));
            gfs.Add(createGenericField("PO_Date", ItemsChoiceType.DateValue, PO_Date.Date));
            gfs.Add(createGenericField("Business_Unit", ItemsChoiceType.StringValue, Business_Unit));
            gfs.Add(createGenericField("PO_ID", ItemsChoiceType.StringValue, PO_ID));
            gfs.Add(createGenericField("Line_Number", ItemsChoiceType.StringValue, Line_Number));
            gfs.Add(createGenericField("Item", ItemsChoiceType.StringValue, Item));
            gfs.Add(createGenericField("Description", ItemsChoiceType.StringValue, Description));
            gfs.Add(createGenericField("Problem_Code", ItemsChoiceType.StringValue, Problem_Code));
            gfs.Add(createGenericField("Expediting_Comments", ItemsChoiceType.StringValue, Expediting_Comments));
            gfs.Add(createGenericField("Buyer_ID", ItemsChoiceType.StringValue, Buyer_ID));
            gfs.Add(createGenericField("Vendor_ID", ItemsChoiceType.StringValue, Vendor_ID));
            gfs.Add(createGenericField("Vendor_Name", ItemsChoiceType.StringValue, Vendor_Name));
            gfs.Add(createGenericField("Last_Operator", ItemsChoiceType.StringValue, Last_Operator));
            gfs.Add(createGenericField("Last_Comment_Date", ItemsChoiceType.DateTimeValue, Last_Comment_Date));
            gfs.Add(createGenericField("Inventory_Business_Unit", ItemsChoiceType.StringValue, Inventory_Business_Unit));
            gfs.Add(createGenericField("Priority_Flag", ItemsChoiceType.StringValue, Priority_Flag));
            gfs.Add(createGenericField("Status_Age", ItemsChoiceType.IntegerValue, Status_Age));
            gfs.Add(createGenericField("Site_Name", ItemsChoiceType.StringValue, Site_Name));
            gfs.Add(createGenericField("Buyer_Team", ItemsChoiceType.StringValue, Buyer_Team));
            gfs.Add(createGenericField("PS_URL", ItemsChoiceType.StringValue, PS_URL));

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

            GenericObject[] genArray = new GenericObject[99];
            int n = 0;

            try
            {
                //foreach (var item in ACTION_ITEMS)
                //while (iLastVal % modValue != 0 || (iLastVal / modValue) == 0 || iLastVal == ACTION_ITEMS.Count())
                while (iLastVal != ACTION_ITEMS.Count())
                {
                    genArray[n] = getBuyExpGenericObject(ACTION_ITEMS[iLastVal], CLIENT[iLastVal], PO_DATE[iLastVal], BUSINESS_UNIT[iLastVal], PO_ID[iLastVal], LINE_NUMBER[iLastVal], ITEM[iLastVal], DESCRIPTION[iLastVal], PROBLEM_CODE[iLastVal], EXPEDITING_COMMENTS[iLastVal], BUYER_ID[iLastVal], VENDOR_ID[iLastVal], VENDOR_NAME[iLastVal], LAST_OPERATOR[iLastVal], LAST_COMMENT_DATE[iLastVal], INVENTORY_BUSINESS_UNIT[iLastVal], PRIORITY_FLAG[iLastVal], 10, SITE_NAME[iLastVal], BUYER_TEAM[iLastVal], PS_URL[iLastVal]);
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
    }
}
