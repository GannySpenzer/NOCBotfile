﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Web.Services.Protocols;
using OSVCService;
using PODispatchReload;
using PODispatchReload1;
using System.Data;
using System.ServiceModel.Channels;

namespace OSVCService
{
    public class Batcher : PODData 
    {

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

        public void CreateBuyExpBatch(PODData podIn, Logger m_oLogger, out string sResponse)
        {
            ACTION_ITEM = podIn.ACTION_ITEM;
            CLIENT = podIn.CLIENT;
            VENDOR_NAME = podIn.VENDOR_NAME;
            PO_DATE = podIn.PO_DATE;
            PO_ID = podIn.PO_ID;
            LINE_NUMBER = podIn.LINE_NUMBER;
            ITEM_ID = podIn.ITEM_ID;
            INITIAL_DISP_METHOD = podIn.INITIAL_DISP_METHOD;
            INITIAL_USER = podIn.INITIAL_USER;
            INITIAL_DIS_DTTM = podIn.INITIAL_DIS_DTTM;
            BUYER_ID = podIn.BUYER_ID;
            VENDOR_ID = podIn.VENDOR_ID;
            VENDOR_EMAIL = podIn.VENDOR_EMAIL;
            VENDOR_DEFAULT = podIn.VENDOR_DEFAULT;
            PROBLEM_CODE = podIn.PROBLEM_CODE;
            COMMENTS = podIn.COMMENTS;
            USER_ID= podIn.USER_ID;
            REQ_DISP_OVERRIDE=  podIn.REQ_DISP_OVERRIDE;
            PRIORITY_FLAG = podIn.PRIORITY_FLAG;
            INVENTORY_BUSINESS_UNIT= podIn.INVENTORY_BUSINESS_UNIT;
            HDR_COMMENTS = podIn.HDR_COMMENTS;
            COMMENT_TYPE = podIn.COMMENT_TYPE;
            SITE_NAME = podIn.SITE_NAME;
            PS_URL =  podIn.PS_URL;
            BUYER_TEAM =  podIn.BUYER_TEAM;

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
            //m_oLogger = new Logger(sLogPath, "PODispatchReload");
            m_oLogger.LogMessage("PODispatchExcep", "Entered PODispatchExcep class");

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
                m_oLogger.LogMessage("PODispatchReload", "PODispatchExcep submitBatch Failure: " + ex.ToString());

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
            //m_oLogger = new Logger(sLogPath, "PODispatchReload");
            m_oLogger.LogMessage("PODispatchExcep", "Entered submitBatch class");

            ClientInfoHeader clientInfoHeader = new ClientInfoHeader();
            clientInfoHeader.AppID = "Batcher";

            APIAccessRequestHeader apiAccessRequestHeader = new APIAccessRequestHeader();

            BatchResponseItem[] batchRes;

            WSHttpBinding test = new WSHttpBinding();
            test.TextEncoding = UTF8Encoding.UTF8;


            m_oLogger.LogMessage("PODispatchReload", "PODispatchExcep submitBatch Starting Service Run.");
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
            //    m_oLogger.LogMessage("ExpeditorReload", "PODispatchExcep submitBatch Response: " + newObj.ID.id.ToString());
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
                        //m_oLogger.LogMessage("ExpeditorReload", "PODispatchExcep submitBatch Response: " + newObj.ID.id.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                strResp = "FAILURE";
                m_oLogger.LogMessage("PODispatchReload", "PODispatchExcep submitBatch Failure: " + ex.ToString());
            }

        }
        //Create an generic object/record for each row that you want to insert
        //This is where you would set the fields for the table you are populating
        private GenericObject getBuyExpGenericObject(string action_item, string client,
            string vendor_name, DateTime  po_date, string po_id,
            string line_number, string item_id, string initial_disp_method, string initial_user,
            DateTime  initial_dis_dttm, string Buyer_id, string vendor_id,
            string vendor_email, string vendor_default, string problem_code,
            string comments, string user_id, string req_disp_override,
            string priority_flag, string inventory_business_unit, string hdr_comments,
            string comment_type, string site_name, string ps_url,
            string buyer_team)
        {
            GenericObject go = new GenericObject();

            //Set the object type
            RNObjectType objType = new RNObjectType();
            objType.Namespace = "CO";
            objType.TypeName = "PODispatch";
            go.ObjectType = objType;

            List<GenericField> gfs = new List<GenericField>();
            gfs.Add(createGenericField("Action_Item", ItemsChoiceType.StringValue, action_item ));
            gfs.Add(createGenericField("Client", ItemsChoiceType.StringValue, client));
            gfs.Add(createGenericField("Vendor_name", ItemsChoiceType.StringValue, vendor_name));
            gfs.Add(createGenericField("PO_Date", ItemsChoiceType.DateTimeValue , po_date ));
            gfs.Add(createGenericField("PO_ID", ItemsChoiceType.StringValue, po_id ));
            gfs.Add(createGenericField("Line_Number", ItemsChoiceType.StringValue, line_number ));
            gfs.Add(createGenericField("Item_ID", ItemsChoiceType.StringValue, item_id ));
            gfs.Add(createGenericField("Initial_Disp_Method", ItemsChoiceType.StringValue, initial_disp_method ));
            gfs.Add(createGenericField("Initial_User", ItemsChoiceType.StringValue, initial_user));
            gfs.Add(createGenericField("Initial_Disp_DTTM", ItemsChoiceType.DateTimeValue , initial_dis_dttm ));
            gfs.Add(createGenericField("Vendor_ID", ItemsChoiceType.StringValue, vendor_id));
            gfs.Add(createGenericField("Vendor_Email", ItemsChoiceType.StringValue , vendor_email ));
            gfs.Add(createGenericField("Vendor_Default", ItemsChoiceType.StringValue, vendor_default ));
            gfs.Add(createGenericField("Problem_Code", ItemsChoiceType.StringValue, problem_code ));
            gfs.Add(createGenericField("Comments", ItemsChoiceType.StringValue, comments));
            gfs.Add(createGenericField("User_ID", ItemsChoiceType.StringValue, user_id));
            gfs.Add(createGenericField("Req_Disp_Override", ItemsChoiceType.StringValue, req_disp_override ));
            gfs.Add(createGenericField("Priority_Flag", ItemsChoiceType.StringValue, priority_flag ));
            gfs.Add(createGenericField("Inventory_Business_Unit", ItemsChoiceType.StringValue, inventory_business_unit ));
            gfs.Add(createGenericField("HDR_Comments", ItemsChoiceType.StringValue, hdr_comments ));
            gfs.Add(createGenericField("Comment_Type", ItemsChoiceType.StringValue , comment_type));
            gfs.Add(createGenericField("Site_Name", ItemsChoiceType.StringValue, site_name ));
            gfs.Add(createGenericField("PS_URL", ItemsChoiceType.StringValue , ps_url ));

            //gfs.Add(createGenericField("Buyer_ID", ItemsChoiceType.StringValue, Buyer_id));
            //gfs.Add(createGenericField("Buyer_Team", ItemsChoiceType.StringValue, buyer_team));
            try
            {
                if (Buyer_id.Trim() != "")
                    gfs.Add(createAccountMenuFieldByName("Buyer_ID", Buyer_id));
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
                    genArray[n] = getBuyExpGenericObject(ACTION_ITEM[iLastVal], CLIENT[iLastVal], VENDOR_NAME[iLastVal], PO_DATE[iLastVal], PO_ID[iLastVal], LINE_NUMBER[iLastVal], ITEM_ID[iLastVal],
                        INITIAL_DISP_METHOD [iLastVal], INITIAL_USER[iLastVal], INITIAL_DIS_DTTM[iLastVal], BUYER_ID[iLastVal], VENDOR_ID[iLastVal], VENDOR_EMAIL[iLastVal], VENDOR_DEFAULT[iLastVal],
                        PROBLEM_CODE[iLastVal], COMMENTS[iLastVal], USER_ID[iLastVal], REQ_DISP_OVERRIDE[iLastVal], PRIORITY_FLAG[iLastVal], INVENTORY_BUSINESS_UNIT[iLastVal], HDR_COMMENTS[iLastVal],
                        COMMENT_TYPE[iLastVal], SITE_NAME[iLastVal], PS_URL[iLastVal], BUYER_TEAM[iLastVal]);
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
