using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Web.Services.Protocols;
using OSVCService;
using POOverrideReload;
using POOverrideReload1;
using System.Data;
using System.ServiceModel.Channels;
using OSVCService;

namespace OSVCService
{
    public class Batcher : POOverrideReloadDAL
    {

        public List<string> ACTION_ITEM = new List<string>();
        public List<string> CLIENT = new List<string>();
        public List<string> SITE = new List<string>();
        public List<string> BUSINESS_UNIT = new List<string>();
        public List<string> PO_ID = new List<string>();
        public List<string> LINE_NUMBER = new List<string>();
        public List<DateTime> DATE_ACKNOWLEDGED = new List<DateTime>();
        public List<string> VENDOR_ID = new List<string>();
        public List<string> VENDOR_NAME = new List<string>();
        public List<DateTime> PO_DATE = new List<DateTime>();
        public List<string> BUYER_ID = new List<string>();
        public List<string> OPERATOR_ID = new List<string>();
        public List<string> SHIPTO_ID = new List<string>();
        public List<string> ITEM_ID = new List<string>();
        public List<int> PO_QUANTITY = new List<int>();
        public List<int> QTY_ACKNOWLEDGED = new List<int>();
        public List<int> PO_PRICE = new List<int>();
        public List<int> PRICE_ACKNOWLEDGED = new List<int>();
        public List<string> CURRENCY = new List<string>();
        public List<string> CURRENCY_ACKNOWLEDGED = new List<string>();
        public List<string> UNIT_MEASURE = new List<string>();
        public List<string> UOM_ACKNOWLEDGED = new List<string>();
        public List<DateTime> PO_DUE_DATE = new List<DateTime>();
        public List<DateTime> DUE_DATE_ACKNOWLEDGED = new List<DateTime>();
        public List<string> PRICE_UPDATE_BYPASS = new List<string>();
        public List<string> PRICE_UPDATE_OVERRIDE = new List<string>();
        public List<string> DUE_DATE_BYPASS = new List<string>();
        public List<string> DUE_DATE_OVERRIDE = new List<string>();
        public List<string> QTY_UPDATE_BYPASS = new List<string>();
        public List<string> QTY_OVERRIDE_STATUS = new List<string>();
        public List<string> REVIEW_FLAG = new List<string>();
        public List<string> PS_URL = new List<string>();
        public List<string> BUYER_TEAM = new List<string>();

        DateTime dateparse;

        int iLastVal = 0;
        int modValue = 1000;
        string strResp = "SUCCESS";

        int dtResponseRowsCount = 0;

        RightNowSyncPortClient _client;

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

        public void CreateBuyExpBatch(PODData podIn, Logger m_oLogger, out string sResponse)
        {
            ACTION_ITEM = podIn.ACTION_ITEM;
            CLIENT = podIn.CLIENT;
            SITE = podIn.SITE;
            BUSINESS_UNIT = podIn.BUSINESS_UNIT;
            PO_ID = podIn.PO_ID;
            LINE_NUMBER = podIn.LINE_NUMBER;
            DATE_ACKNOWLEDGED = podIn.DATE_ACKNOWLEDGED;
            VENDOR_ID = podIn.VENDOR_ID;
            VENDOR_NAME = podIn.VENDOR_NAME;
            PO_DATE = podIn.PO_DATE;
            BUYER_ID = podIn.BUYER_ID;
            OPERATOR_ID = podIn.OPERATOR_ID;
            SHIPTO_ID = podIn.SHIPTO_ID;
            ITEM_ID = podIn.ITEM_ID;
            PO_QUANTITY = podIn.PO_QUANTITY;
            QTY_ACKNOWLEDGED = podIn.QTY_ACKNOWLEDGED;
            PO_PRICE = podIn.PO_PRICE;
            PRICE_ACKNOWLEDGED = podIn.PRICE_ACKNOWLEDGED;
            CURRENCY = podIn.CURRENCY;
            CURRENCY_ACKNOWLEDGED = podIn.CURRENCY_ACKNOWLEDGED;
            UNIT_MEASURE = podIn.UNIT_MEASURE;
            UOM_ACKNOWLEDGED = podIn.UOM_ACKNOWLEDGED;
            PO_DUE_DATE = podIn.PO_DUE_DATE;
            DUE_DATE_ACKNOWLEDGED = podIn.DUE_DATE_ACKNOWLEDGED;
            PRICE_UPDATE_BYPASS = podIn.PRICE_UPDATE_BYPASS;
            PRICE_UPDATE_OVERRIDE = podIn.PRICE_UPDATE_OVERRIDE;
            DUE_DATE_BYPASS = podIn.DUE_DATE_BYPASS;
            DUE_DATE_OVERRIDE = podIn.DUE_DATE_OVERRIDE;
            QTY_UPDATE_BYPASS = podIn.QTY_UPDATE_BYPASS;
            QTY_OVERRIDE_STATUS = podIn.QTY_OVERRIDE_STATUS;
            REVIEW_FLAG = podIn.REVIEW_FLAG;
            PS_URL = podIn.PS_URL;
            BUYER_TEAM = podIn.BUYER_TEAM;

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
            //m_oLogger = new Logger(sLogPath, "POOverrideReload");
            m_oLogger.LogMessage("POOverrideExcep", "Entered POOverrideExcep class");

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
                m_oLogger.LogMessage("POOverrideReload", "POOverrideExcep submitBatch Failure: " + ex.ToString());

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
            //m_oLogger = new Logger(sLogPath, "POOverrideReload");
            m_oLogger.LogMessage("POOverrideExcep", "Entered submitBatch class");

            ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
            clientInfoHeader.AppID = "Batcher";

            APIAccessRequestHeader apiAccessRequestHeader = new APIAccessRequestHeader();

            BatchResponseItem[] batchRes;

            WSHttpBinding test = new WSHttpBinding();
            test.TextEncoding = UTF8Encoding.UTF8;


            m_oLogger.LogMessage("POOverrideReload", "POOverrideExcep submitBatch Starting Service Run.");
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
            //    m_oLogger.LogMessage("ExpeditorReload", "POOverrideExcep submitBatch Response: " + newObj.ID.id.ToString());
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
                        //m_oLogger.LogMessage("ExpeditorReload", "POOverrideExcep submitBatch Response: " + newObj.ID.id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                m_oLogger.LogMessage("POOverrideReload", "POOverrideExcep submitBatch Failure: " + ex.ToString());
            }

        }
        //Create an generic object/record for each row that you want to insert
        //This is where you would set the fields for the table you are populating
        private GenericObject getBuyExpGenericObject(string action_item, string client,
            string site, string business_unit, string po_id,
            string line_number, DateTime  date_acknowledged, string vendor_id, string vendor_name,
            DateTime po_date, string buyer_id, string operator_id,
            string shipto_id, string item_id, int po_quantity,
            int qty_acknowledged, int po_price, int price_acknowledged,
            string currency, string currency_acknowledged, string unit_measure,
            string uom_acknowledged, DateTime  po_due_date, DateTime due_date_acknowledged,
            string price_update_bypass, string price_update_override, 
            string due_date_bypassed, string due_date_override, string qty_update_bypass,
            string qty_override_status, string review_flag, string ps_url, string buyer_team)
        {
            GenericObject go = new GenericObject();

            //Set the object type
            RNObjectType objType = new RNObjectType();
            objType.Namespace = "CO";
            objType.TypeName = "POOverride";
            go.ObjectType = objType;

            List<GenericField> gfs = new List<GenericField>();
            gfs.Add(createGenericField("Action_Item", ItemsChoiceType.StringValue, action_item));
            gfs.Add(createGenericField("Client", ItemsChoiceType.StringValue, client));
            gfs.Add(createGenericField("Site", ItemsChoiceType.StringValue, site));
            gfs.Add(createGenericField("Business_Unit", ItemsChoiceType.StringValue , business_unit));
            gfs.Add(createGenericField("PO_ID", ItemsChoiceType.StringValue , po_id));
            gfs.Add(createGenericField("Line_Number", ItemsChoiceType.StringValue, line_number));
            gfs.Add(createGenericField("Date_Acknowledged", ItemsChoiceType.DateTimeValue , date_acknowledged ));
            gfs.Add(createGenericField("Vendor_ID", ItemsChoiceType.StringValue, vendor_id));
            gfs.Add(createGenericField("Vendor_Name", ItemsChoiceType.StringValue, vendor_name ));
            gfs.Add(createGenericField("PO_Date", ItemsChoiceType.DateValue, po_date ));
            gfs.Add(createGenericField("Buyer_ID", ItemsChoiceType.StringValue, buyer_id ));
            gfs.Add(createGenericField("Operator_ID", ItemsChoiceType.StringValue, operator_id));
            gfs.Add(createGenericField("Shipto_ID", ItemsChoiceType.StringValue, shipto_id ));
            gfs.Add(createGenericField("Item_ID", ItemsChoiceType.StringValue, item_id ));
            gfs.Add(createGenericField("PO_Quantity", ItemsChoiceType.IntegerValue , po_quantity ));
            gfs.Add(createGenericField("Qty_Acknowledged", ItemsChoiceType.IntegerValue , qty_acknowledged ));
            gfs.Add(createGenericField("PO_Price", ItemsChoiceType.IntegerValue , po_price ));
            gfs.Add(createGenericField("Price_Acknowledged", ItemsChoiceType.IntegerValue , price_acknowledged));
            gfs.Add(createGenericField("Currency", ItemsChoiceType.StringValue, currency ));
            gfs.Add(createGenericField("Currency_Acknowledged", ItemsChoiceType.StringValue, currency_acknowledged ));
            gfs.Add(createGenericField("Unit_Measure", ItemsChoiceType.StringValue, unit_measure ));
            gfs.Add(createGenericField("UoM_Acknowledged", ItemsChoiceType.StringValue, uom_acknowledged ));
            gfs.Add(createGenericField("PO_Due_Date", ItemsChoiceType.DateValue , po_due_date ));
            gfs.Add(createGenericField("Due_Date_Acknowledged", ItemsChoiceType.DateValue , due_date_acknowledged ));
            gfs.Add(createGenericField("Price_Update_Bypass", ItemsChoiceType.StringValue, price_update_bypass ));
            gfs.Add(createGenericField("Price_Update_Override", ItemsChoiceType.StringValue, price_update_override ));
            gfs.Add(createGenericField("Due_Date_Bypass", ItemsChoiceType.StringValue, due_date_bypassed  ));
            gfs.Add(createGenericField("Due_Date_Override", ItemsChoiceType.StringValue, due_date_override ));
            gfs.Add(createGenericField("Qty_Update_Bypass", ItemsChoiceType.StringValue, qty_update_bypass ));
            gfs.Add(createGenericField("Qty_Override_Status", ItemsChoiceType.StringValue, qty_override_status ));
            gfs.Add(createGenericField("Review_Flag", ItemsChoiceType.StringValue, review_flag));
            gfs.Add(createGenericField("PS_URL", ItemsChoiceType.StringValue, ps_url ));
            gfs.Add(createGenericField("Buyer_Team", ItemsChoiceType.StringValue, buyer_team));


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
                    genArray[n] = getBuyExpGenericObject(ACTION_ITEM[iLastVal], CLIENT[iLastVal], SITE[iLastVal], BUSINESS_UNIT[iLastVal], PO_ID[iLastVal], LINE_NUMBER[iLastVal], DATE_ACKNOWLEDGED [iLastVal],
                        VENDOR_ID[iLastVal], VENDOR_NAME[iLastVal], PO_DATE[iLastVal], BUYER_ID [iLastVal], OPERATOR_ID[iLastVal], SHIPTO_ID[iLastVal], ITEM_ID[iLastVal],
                        PO_QUANTITY[iLastVal], QTY_ACKNOWLEDGED [iLastVal], PO_PRICE[iLastVal], PRICE_ACKNOWLEDGED [iLastVal], CURRENCY[iLastVal], CURRENCY_ACKNOWLEDGED [iLastVal], UNIT_MEASURE[iLastVal],
                        UOM_ACKNOWLEDGED [iLastVal], PO_DUE_DATE[iLastVal], DUE_DATE_ACKNOWLEDGED [iLastVal], PRICE_UPDATE_BYPASS [iLastVal], PRICE_UPDATE_OVERRIDE [iLastVal], DUE_DATE_BYPASS[iLastVal],
                        DUE_DATE_OVERRIDE[iLastVal], QTY_UPDATE_BYPASS[iLastVal], QTY_OVERRIDE_STATUS[iLastVal], REVIEW_FLAG[iLastVal], PS_URL[iLastVal], BUYER_TEAM[iLastVal]);
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
