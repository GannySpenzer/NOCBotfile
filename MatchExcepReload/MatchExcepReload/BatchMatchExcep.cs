using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Web.Services.Protocols;
using OSVCService;
using MatchExcepReload;
using MatchExcepReload1;
using System.Data;
using System.ServiceModel.Channels;

namespace OSVCService
{
    public class Batcher : MatchExcepReloadDAL 
    {

        public List<string> CLIENT = new List<string>();
        List<string> BUYER_TEAM = new List<string>();
        List<string> SITE = new List<string>();
        List<string> PS_URL = new List<string>();
        List<string> ME_ROLE = new List<string>();
        List<string> SHIPTO_DESC = new List<string>();
        List<string> SHIPTO_ID = new List<string>();
        List<string> ASSIGNED_TO = new List<string>();
        List<string> TASK_TYPE = new List<string>();
        List<int> ME_LINES = new List<int>();
        List<int> DAYS_OVERALL = new List<int>();
        List<string> OVERALL_AGING = new List<string>();
        List<DateTime> REPORTING_DATE = new List<DateTime>();
        List<string> MATCH_RULE = new List<string>();
        List<string> SUPPLIER_ID= new List<string>();
        List<string> SUPPLIER_NAME = new List<string>();
        List<string> BUYER_ID = new List<string>();
        List<string> PO_BUSINESS_UNIT = new List<string>();
        List<string> PO_NO = new List<string>();
        List<string> DISPATCH_METHOD = new List<string>();
        List<string> INVOICE_ID = new List<string>();
        List<DateTime> INVOICE_DATE= new List<DateTime>();
        List<string> TOTAL_INVOICED_AMT = new List<string>();
        List<DateTime > SCAN_DATE = new List<DateTime >();
        List<DateTime> TASK_DATE= new List<DateTime>();
        List<int> TASK_DAYS = new List<int>();
        List<string> TASK_AGING = new List<string>();
        List<DateTime> DATE_ASSIGNED= new List<DateTime>();
        List<int> DAYS_ASSIGNED = new List<int>();
        List<string> ASSIGNED_AGING = new List<string>();

        DateTime dateparse;

        int iLastVal = 0;
        int modValue = 1000;
        string strResp = "SUCCESS";

        int dtResponseRowsCount = 0;

        RightNowSyncPortClient _client;

        // InitializeLogger start here
        //Logger m_oLogger;
        //string sLogPath = Environment.CurrentDirectory;

        DataTable dtResponse = new DataTable();

        //Set the API Username and Password
        public Batcher(string strauth, string strpass)
        {
            _client = new RightNowSyncPortClient();

            _client.ClientCredentials.UserName.UserName = strauth;
            _client.ClientCredentials.UserName.Password = strpass;
        }

        public void CreateBuyExpBatch(MEData medIn, out string sResponse)
        {
            //getData();
            //if (dtResponseRowsCount > 0)
            //{
            CLIENT = medIn.CLIENT;
            BUYER_TEAM = medIn.BUYER_TEAM;
            SITE = medIn.SITE;
            PS_URL = medIn.PS_URL;
            ME_ROLE = medIn.ME_ROLE;
            SHIPTO_DESC = medIn.SHIPTO_DESC;
            SHIPTO_ID = medIn.SHIPTO_ID;
            ASSIGNED_TO = medIn.ASSIGNED_TO ;
            TASK_TYPE = medIn.TASK_TYPE;
            ME_LINES = medIn.ME_LINES;
            DAYS_OVERALL = medIn.DAYS_OVERALL;
            OVERALL_AGING = medIn.OVERALL_AGING;
            REPORTING_DATE = medIn.REPORTING_DATE;
            MATCH_RULE = medIn.MATCH_RULE;
            SUPPLIER_ID = medIn.SUPPLIER_ID;
            SUPPLIER_NAME = medIn.SUPPLIER_NAME;
            BUYER_ID = medIn.BUYER_ID;
            PO_BUSINESS_UNIT = medIn.PO_BUSINESS_UNIT;
            PO_NO = medIn.PO_NO;
            DISPATCH_METHOD = medIn.DISPATCH_METHOD;
            INVOICE_ID = medIn.INVOICE_ID;
            INVOICE_DATE = medIn.INVOICE_DATE;
            TOTAL_INVOICED_AMT = medIn.TOTAL_INVOICED_AMT;
            SCAN_DATE = medIn.SCAN_DATE;
            TASK_DATE = medIn.TASK_DATE;
            TASK_DAYS = medIn.TASK_DAYS;
            TASK_AGING = medIn.TASK_AGING;
            DATE_ASSIGNED = medIn.DATE_ASSIGNED;
            DAYS_ASSIGNED = medIn.DAYS_ASSIGNED;
            ASSIGNED_AGING = medIn.ASSIGNED_AGING;

            dtResponseRowsCount = ASSIGNED_AGING.Count();

            buildBatchRequestItems();
            //}
            sResponse = strResp;
        }


        //You can have up to 100 items in a batch. The function that is part of the batch
        //can have up to 1000 objects so you can essentially have 100,000 records created in one call
        public void buildBatchRequestItems()
        {
            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "MatchExcepReload");
            m_oLogger.LogMessage("BatchMatchExcep", "Entered BatchMatchExcep class");

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

                submitBatch(requestItems);
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                m_oLogger.LogMessage("MatchExcepReload", "BatchMatchExcep submitBatch Failure: " + ex.ToString());

            }
        }


        //Submit the batch and read the response if needed
        public void submitBatch(BatchRequestItem[] requestItems)
        {
            Logger m_oLogger;
            string sLogPath = Environment.CurrentDirectory;
            if (!sLogPath.EndsWith(@"\"))
                sLogPath += @"\";
            sLogPath += "Logs";
            m_oLogger = new Logger(sLogPath, "MatchExcepReload");
            m_oLogger.LogMessage("BatchMatchExcep", "Entered submitBatch class");

            ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
            clientInfoHeader.AppID = "Batcher";

            APIAccessRequestHeader apiAccessRequestHeader = new APIAccessRequestHeader();

            BatchResponseItem[] batchRes;

            WSHttpBinding test = new WSHttpBinding();
            test.TextEncoding = UTF8Encoding.UTF8;
            

            m_oLogger.LogMessage("ExpeditorReload", "BatchMatchExcep submitBatch Starting Service Run.");
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
            //    m_oLogger.LogMessage("ExpeditorReload", "BatchMatchExcep submitBatch Response: " + newObj.ID.id.ToString());
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
                        m_oLogger.LogMessage("ExpeditorReload", "BatchMatchExcep submitBatch Response: " + newObj.ID.id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                m_oLogger.LogMessage("ExpeditorReload", "BatchMatchExcep submitBatch Failure: " + ex.ToString());
            }

        }
        //Create an generic object/record for each row that you want to insert
        //This is where you would set the fields for the table you are populating
        private GenericObject getBuyExpGenericObject(string Client,
            string Buyer_Team, string Site, string PS_url, string Me_Role,
            string Shipto_Desc, string Shipto_ID, string Assigned_To, string Task_Type,
            int Me_Lines, int Days_Overall, string Overall_Aging,
            DateTime   Reporting_Date, string Match_Rule, string Supplier_ID,
            string Supplier_Name , string Buyer_ID, string PO_Business_Unit,
            string PO_No, string Dispatch_Method, string Invoice_ID, 
            DateTime Invoice_Date, string Total_Invoiced_Amt, DateTime Scan_Date,
            DateTime Task_Date, int Task_Days, string Task_Aging, DateTime Date_Assigned,
            int Days_Assigned, string Assigned_Aging)
        {
            GenericObject go = new GenericObject();

            //Set the object type
            RNObjectType objType = new RNObjectType();
            objType.Namespace = "CO";
            objType.TypeName = "MatchExcep";
            go.ObjectType = objType;

            List<GenericField> gfs = new List<GenericField>();
            gfs.Add(createGenericField("Client", ItemsChoiceType.StringValue, Client));
            gfs.Add(createGenericField("Buyer_Team", ItemsChoiceType.StringValue , Buyer_Team ));
            gfs.Add(createGenericField("Site", ItemsChoiceType.StringValue , Site));
            gfs.Add(createGenericField("PS_URL", ItemsChoiceType.StringValue, PS_url ));
            gfs.Add(createGenericField("ME_Role", ItemsChoiceType.StringValue, Me_Role ));
            gfs.Add(createGenericField("Shipto_Desc", ItemsChoiceType.StringValue, Shipto_Desc ));
            gfs.Add(createGenericField("Shipto_ID", ItemsChoiceType.StringValue, Shipto_ID ));
            gfs.Add(createGenericField("Assigned_To", ItemsChoiceType.StringValue, Assigned_To ));
            gfs.Add(createGenericField("Task_Type", ItemsChoiceType.StringValue, Task_Type ));
            gfs.Add(createGenericField("ME_Lines", ItemsChoiceType.IntegerValue , Me_Lines));
            gfs.Add(createGenericField("Days_Overall", ItemsChoiceType.IntegerValue , Days_Overall ));
            gfs.Add(createGenericField("Overall_Aging", ItemsChoiceType.StringValue, Overall_Aging ));
            gfs.Add(createGenericField("Reporting_Date", ItemsChoiceType.DateValue   , Reporting_Date ));
            gfs.Add(createGenericField("Match_Rule", ItemsChoiceType.StringValue, Match_Rule ));
            gfs.Add(createGenericField("Supplier_ID", ItemsChoiceType.StringValue , Supplier_ID ));

            gfs.Add(createGenericField("Supplier_Name", ItemsChoiceType.StringValue, Supplier_Name )); 
            gfs.Add(createGenericField("Buyer_ID", ItemsChoiceType.StringValue, Buyer_ID ));
            gfs.Add(createGenericField("PO_Business_Unit", ItemsChoiceType.StringValue ,PO_Business_Unit ));
            gfs.Add(createGenericField("PO_No", ItemsChoiceType.StringValue, PO_No ));
            gfs.Add(createGenericField("Dispatch_Method", ItemsChoiceType.StringValue, Dispatch_Method ));
            gfs.Add(createGenericField("Invoice_ID", ItemsChoiceType.StringValue, Invoice_ID ));
            gfs.Add(createGenericField("Invoice_Date", ItemsChoiceType.DateValue, Invoice_Date ));
            gfs.Add(createGenericField("Total_Invoiced_Amt", ItemsChoiceType.StringValue     , Total_Invoiced_Amt ));
            gfs.Add(createGenericField("Scan_Date", ItemsChoiceType.DateValue , Scan_Date ));
            gfs.Add(createGenericField("Task_Date", ItemsChoiceType.DateValue , Task_Date));
            gfs.Add(createGenericField("Task_Days", ItemsChoiceType.IntegerValue , Task_Days));
            gfs.Add(createGenericField("Task_Aging", ItemsChoiceType.StringValue, Task_Aging));
            gfs.Add(createGenericField("Date_Assigned", ItemsChoiceType.DateValue , Date_Assigned));
            gfs.Add(createGenericField("Days_Assigned", ItemsChoiceType.IntegerValue , Days_Assigned));
            gfs.Add(createGenericField("Assigned_Aging", ItemsChoiceType.StringValue, Assigned_Aging));
            

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
                    genArray[n] = getBuyExpGenericObject(CLIENT [iLastVal], BUYER_TEAM [iLastVal], SITE [iLastVal], PS_URL [iLastVal], ME_ROLE [iLastVal], SHIPTO_DESC [iLastVal], SHIPTO_ID [iLastVal], 
                        ASSIGNED_TO [iLastVal], TASK_TYPE [iLastVal], ME_LINES [iLastVal], DAYS_OVERALL [iLastVal], OVERALL_AGING [iLastVal], REPORTING_DATE [iLastVal], MATCH_RULE [iLastVal], 
                        SUPPLIER_ID [iLastVal], SUPPLIER_NAME [iLastVal], BUYER_ID [iLastVal], PO_BUSINESS_UNIT[iLastVal] , PO_NO [iLastVal], DISPATCH_METHOD [iLastVal], INVOICE_ID [iLastVal], 
                        INVOICE_DATE[iLastVal], TOTAL_INVOICED_AMT [iLastVal],SCAN_DATE[iLastVal], TASK_DATE[iLastVal],TASK_DAYS[iLastVal],TASK_AGING[iLastVal],DATE_ASSIGNED[iLastVal],
                        DAYS_ASSIGNED[iLastVal], ASSIGNED_AGING[iLastVal]);
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
