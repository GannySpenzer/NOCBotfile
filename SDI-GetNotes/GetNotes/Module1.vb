Imports System.IO
Imports System.Data.OleDb
Imports System.Text
Imports System.Configuration
Imports System.Net
Imports Newtonsoft.Json
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Xml
Imports System.Web
Imports System.Web.Mail
Module Module1
    'variable declartions

    Dim objWalSCComments As StreamWriter
    Dim objWalSCWorkOrder As StreamWriter
    Dim objWalmartSC As StreamWriter
    Dim rootDir As String = ConfigurationSettings.AppSettings("rootDir")
    Dim logpath As String = ConfigurationSettings.AppSettings("logpath") & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim WalmartSCWorkOrderPath As String = ConfigurationSettings.AppSettings("WalmartSCWorkOrder") & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection(Convert.ToString(ConfigurationSettings.AppSettings("OLEDBconString")))

    Dim log As StreamWriter
    Dim fileStream As FileStream = Nothing
    Dim logDirInfo As DirectoryInfo = Nothing
    Dim logFileInfo As FileInfo
    'Suvetha - INC0041221 - GetNotes utility needs fix to capture And reprocess errors updating work order
    Dim trycount As String = ConfigurationSettings.AppSettings("TryCount")




    'Main method from where method gets invoked
    Sub Main()

        Console.WriteLine("")

        Dim WorkOrderComments As String = logpath & "_" & String.Format("{0:yyyy-MM-dd_hh-mm-ss-tt}", DateTime.Now) & ".txt"
        logFileInfo = New FileInfo(WorkOrderComments)
        logDirInfo = New DirectoryInfo(logFileInfo.DirectoryName)

        'Checks whether the file exists, if it exists then append, else create the file
        If Not logDirInfo.Exists Then
            logDirInfo.Create()
        End If
        If Not logFileInfo.Exists Then
            fileStream = logFileInfo.Create()
        Else
            fileStream = New FileStream(WorkOrderComments, FileMode.Append)
        End If
        objWalSCComments = New StreamWriter(fileStream)
        objWalSCComments.WriteLine("Start Walmart Service Channel Comments " & Now())
        'Suvetha - INC0041221 - GetNotes utility needs fix to capture And reprocess errors updating work order
        Try
            GetNotes(False)
            GetNotes(True)
        Catch ex As Exception
            objWalSCComments.WriteLine("Error in Utility " & ex.Message & Now())
            SendEmail(ex.Message)

        End Try

        objWalSCComments.WriteLine("Ends " & Now())

        objWalSCComments.Flush()
        objWalSCComments.Close()
    End Sub
    Private Sub SendEmail(Optional ByVal message As String = "")

        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim sTR As String
        Dim MailAttachmentbytes As New List(Of Byte())()
        Dim MailToSpecial As String = ConfigurationSettings.AppSettings("MailToSpecial")
        Dim logpath As String = ConfigurationSettings.AppSettings("logpath")
        Dim email As New MailMessage

        'The email address of the sender
        email.From = "WalmartPurchasing@sdi.com"

        'The email address of the recipient. 
        'email.To = "webdev@sdi.com" '  "pete.doyle@sdi.com"
        If Not getDBName() Then
            email.To = "webdev@sdi.com"

        Else
            email.To = MailToSpecial
        End If
        'The subject of the email
        email.Subject = " Pushing notes to Service channel was completed with Error(s)."

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<html><body><table><tr><td>Service channel notes has completed with errors " + message + " Please check the logs " + logpath + " </td></tr>"

        'email.Body = email.Body & "<tr><td></td><a href='\\BDougherty_XP-l\logs'>\\BDougherty_XP-l\logs\</a></tr></table></body></html>"

        'Send the email and handle any error that occurs
        Try
            'UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, "sriram.s@avasoft.biz", "", "", "Y", email.Body, connectOR)
            SDIEmailService.EmailUtilityServices("MailandStore", email.From, email.To, email.Subject, String.Empty, String.Empty, email.Body, "StatusChangeEmail0", MailAttachmentName, MailAttachmentbytes.ToArray())
        Catch ex As Exception
            objWalSCComments.WriteLine("Error in Sending Email" & ex.Message & Now())

        End Try

    End Sub
    'Suvetha - INC0041221 - GetNotes utility needs fix to capture And reprocess errors updating work order
    'INC0032110	CLONE - As a Sam's Club User, I need my Work Orders validated in Service Channel, need changes for pushing comments to service channel [ get notes ] - Dhamotharan P
    Private Function GetNotes(ByVal Reprocess As Boolean)
        Try
            Dim result As String
            Dim ds As New DataSet
            Dim addminutes As Int16 = Convert.ToInt16(ConfigurationSettings.AppSettings("StartDateNotes"))
            'Dim StartDate As DateTime = Now().AddMinutes(addminutes)
            'Dim EndDate As DateTime = Now()
            Dim StartDate As DateTime = GetStartDate()
            Dim EndDate As DateTime = GetEndDate()
            EndDate.AddSeconds(1)
            Dim sqlstring As String = ""
            sqlstring = "select A.ORDER_NO, A.ISA_INTFC_LN, A.ISA_WORK_ORDER_NO,B.PO_ID,A.ISA_LINE_STATUS," & vbCrLf &
                "C.NOTES_1000,A.ISA_EMPLOYEE_ID, WO.ISA_Install_Cust  from PS_ISA_ORD_INTF_LN A,PS_PO_LINE_DISTRIB B,ps_isa_xpd_comment C, PS_ISA_WO_STATUS WO " & vbCrLf
            If Reprocess Then
                sqlstring += " ,SDIX_UTILITY_PROCESS X" & vbCrLf
            End If
            sqlstring += "where A.business_unit_OM = 'I0W01' AND   A.ISA_LINE_STATUS IN ('DSP','ASN')" & vbCrLf &
                "AND A.BUSINESS_UNIT_PO = B.BUSINESS_UNIT" & vbCrLf &
                "AND A.ORDER_NO = B.REQ_ID" & vbCrLf &
                "AND A.ISA_INTFC_LN = B.REQ_LINE_NBR" & vbCrLf &
                "AND B.BUSINESS_UNIT = C.BUSINESS_UNIT" & vbCrLf &
                "AND A.ISA_WORK_ORDER_NO=WO.ISA_WORK_ORDER_NO(+)" & vbCrLf &
                "And A.BUSINESS_UNIT_OM=WO.BUSINESS_UNIT_OM(+)" & vbCrLf &
                "AND B.PO_ID = C.PO_ID" & vbCrLf &
                "AND B.LINE_NBR= C.LINE_NBR" & vbCrLf &
                "AND C.ISA_PROBLEM_CODE NOT IN ('AK','WS')" & vbCrLf
            If Reprocess Then
                sqlstring += " AND A.order_no= x.order_no(+)" & vbCrLf &
                 " AND x.SC_STATUS = 'Failed'" & vbCrLf &
                 " And TOTAL_TRY_COUNT < '" & trycount & "' " & vbCrLf &
                 " And UTILITY = 'WalmartGetNotes'" & vbCrLf &
                 " ORDER BY  DTTM_STAMP DESC"
            Else
                sqlstring += "AND C.DTTM_STAMP > TO_DATE('" & StartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                "AND C.DTTM_STAMP <= TO_DATE('" & EndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf
            End If
            objWalSCComments.WriteLine("   Supplier comments Query: " & sqlstring)
            objWalSCComments.WriteLine("Start Supplier comment Service Channel " & Now())
            Try
                Dim st As New Stopwatch()
                st.Start()
                ds = ORDBAccess.GetAdapter(sqlstring, connectOR)
                st.Stop()
                Dim ts As TimeSpan = st.Elapsed
                Dim elapsedTime As String = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10)
                objWalSCComments.WriteLine("Query Execution Time " + elapsedTime)
                objWalSCComments.WriteLine("Fetched Datas:" + Convert.ToString(ds.Tables(0).Rows.Count()))

            Catch ex As Exception
                ds = ORDBAccess.GetAdapter(sqlstring, connectOR)
                objWalSCComments.WriteLine("Query Execution Time " + Now())
                objWalSCComments.WriteLine("Fetched Datas:" + Convert.ToString(ds.Tables(0).Rows.Count()))
            End Try
            If Not ds Is Nothing Then
                If ds.Tables.Count > 0 Then
                    If ds.Tables(0).Rows.Count > 0 Then
                        Dim I As Integer
                        For I = 0 To ds.Tables(0).Rows.Count - 1
                            Dim PO_num As String = String.Empty
                            Try
                                Dim Line_Status As String = ds.Tables(0).Rows(I).Item("ISA_LINE_STATUS")
                                PO_num = ds.Tables(0).Rows(I).Item("PO_ID")
                                Dim OrderNum As String = ds.Tables(0).Rows(I).Item("ORDER_NO")
                                Dim WorkOrder As String = ds.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO")
                                Dim Emp_id As String = ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_ID")
                                Dim strComments As String = ds.Tables(0).Rows(I).Item("NOTES_1000")
                                Dim Third_party_comp_id As String = ""
                                Try
                                    Dim Sqlstring2 As String = "select THIRDPARTY_COMP_ID from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & Emp_id & "'"
                                    connectOR.Open()
                                    Third_party_comp_id = ORDBAccess.GetScalar(Sqlstring2, connectOR)
                                    connectOR.Close()
                                Catch ex As Exception
                                    Third_party_comp_id = "0"
                                End Try
                                Dim IsSamsclubWO As Boolean = False
                                Dim StrWO_Type As String = ""
                                Dim SamsClub_Key As String = ConfigurationSettings.AppSettings("SAMSCLUBkey").ToString()
                                If Not IsDBNull(ds.Tables(0).Rows(I).Item("ISA_INSTALL_CUST")) Then
                                    StrWO_Type = ds.Tables(0).Rows(I).Item("ISA_INSTALL_CUST")
                                    objWalSCComments.WriteLine("WO_TYPE: " + Convert.ToString(StrWO_Type) + "Count " + Convert.ToString(I))
                                    If StrWO_Type = SamsClub_Key Then
                                        IsSamsclubWO = True
                                    End If
                                Else
                                    StrWO_Type = " "
                                End If
                                'Dim CredType As String = ""

                                'If Third_party_comp_id <> "100" Then
                                '    CredType = "Walmart"
                                'End If
                                objWalSCComments.WriteLine("PO Number: " + Convert.ToString(PO_num) + "Count " + Convert.ToString(I))
                                result = UpdateNotes(WorkOrder, Third_party_comp_id, strComments, PO_num, OrderNum, IsSamsclubWO)
                                strComments = "Notes: " + strComments
                                Try
                                    LogOrderStatus(OrderNum, strComments, result)
                                Catch ex As Exception
                                    objWalSCComments.WriteLine("Error in LogOrderStatus  " & " " & ex.Message & Now())
                                End Try

                            Catch ex As Exception
                                objWalSCComments.WriteLine("Result- Failed in updating notes for the PO " + ex.Message + PO_num)
                                SendEmail(ex.Message)
                            End Try

                        Next
                        objWalSCComments.WriteLine("/////////////////////////////////////////////////////////////////////////////////////////////")
                    Else
                        objWalSCComments.WriteLine("No data fetched")
                    End If
                End If
            End If
            Try
                If Not Reprocess Then
                    Dim bolerror1 As Boolean
                    bolerror1 = updateLastSendDate("I0W01", EndDate)
                    If bolerror1 = True Then
                        SendEmail()
                    End If
                End If
            Catch ex As Exception
                objWalSCComments.WriteLine("Error in updateLastSendDate " & ex.Message & Now())
            End Try
        Catch ex As Exception
            objWalSCComments.WriteLine("GetNotes" & " " & ex.Message & Now())
            SendEmail(ex.Message)
        End Try
    End Function
    'Suvetha - INC0041221 - GetNotes utility needs fix to capture And reprocess errors updating work order
    Public Function LogOrderStatus(ByVal orderNo As String, ByVal strComments As String, ByVal status As String)
        Dim dsOrders As DataSet = New DataSet()
        Dim strSql As String = String.Empty
        Dim strExecQry As String = String.Empty
        Dim rowsaffected As Integer
        Dim totTryCount As Integer
        Try
            strSql = "SELECT * FROM SDIX_UTILITY_PROCESS WHERE UTILITY = 'WalmartGetNotes' AND SC_STATUS = 'Failed' AND ORDER_NO = '" + orderNo + "'"
            dsOrders = ORDBAccess.GetAdapter(strSql, connectOR)
            If Not dsOrders Is Nothing Then
                If dsOrders.Tables.Count > 0 Then
                    If dsOrders.Tables(0).Rows.Count > 0 Then
                        totTryCount = dsOrders.Tables(0).Rows(0).Item("TOTAL_TRY_COUNT")
                        If totTryCount >= trycount Then
                        Else
                            totTryCount = CInt(dsOrders.Tables(0).Rows(0).Item("TOTAL_TRY_COUNT").ToString) + 1
                            strExecQry = "UPDATE SDIX_UTILITY_PROCESS SET TOTAL_TRY_COUNT = '" & totTryCount & "',SC_STATUS = '" & status & "'," & vbCrLf &
                            "STATUS_CHANGE = '" & strComments & "', UTILITY = 'WalmartGetNotes', LASTUPDTTM = SYSDATE WHERE ORDER_NO = '" & orderNo.ToString() & "'"
                        End If
                    Else
                        totTryCount = 1
                        strExecQry = "INSERT INTO SDIX_UTILITY_PROCESS (" & vbCrLf &
                    "ORDER_NO,TOTAL_TRY_COUNT,STATUS_CHANGE,SC_STATUS,ADD_DTTM,UTILITY,LASTUPDTTM) VALUES" & vbCrLf &
                    "('" + orderNo.ToString() & "','" & totTryCount & "','" & strComments & "','" & status & "',SYSDATE,'WalmartGetNotes',SYSDATE)"
                    End If
                End If
            Else
                objWalSCComments.WriteLine("No data in dsOrders  " & " " & Now())
                Exit Function
            End If

            Try
                Dim Command = New OleDbCommand(strExecQry, connectOR)
                objWalSCComments.WriteLine("  Insert/Update log : " & strExecQry & " " & Now())
                connectOR.Open()
                rowsaffected = Command.ExecuteNonQuery()
                connectOR.Close()
            Catch OleDBExp As OleDbException
                Console.WriteLine("")
                Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                Console.WriteLine("")
                connectOR.Close()
                objWalSCComments.WriteLine("  Error - inserting/updating the log table " & OleDBExp.ToString & " " & Now())
            End Try
        Catch ex As OleDbException
            objWalSCComments.WriteLine("Error in LogOrderStatus  " & " " & ex.Message & Now())
            SendEmail(ex.Message)
        End Try
    End Function

    'INC0032110	CLONE - As a Sam's Club User, I need my Work Orders validated in Service Channel, need changes for pushing comments to service channel [ get notes ] - Dhamotharan P

    Private Function GetStartDate() As Date
        Dim strSQLstring As String = ""
        Dim GetStart_Date As DateTime
        Dim format As New System.Globalization.CultureInfo("en-US", True)

        strSQLstring = "Select TO_CHAR((ISA_NOTES_LAST_DATE), 'MM/DD/YY HH24:MI:SS') AS GETNOTES_LAST_DATE_SEND From SDIX_EMAIL_DETAIL  WHERE ORDER_MFG_BU = 'I0W01'"
        objWalSCComments.WriteLine("  GetStartDate: " & strSQLstring & " " & Now())

        Dim command1 As OleDbCommand
        command1 = New OleDbCommand(strSQLstring, connectOR)
        If connectOR.State = ConnectionState.Open Then
            'do nothing
        Else
            connectOR.Open()
        End If
        Dim objReader As OleDbDataReader = command1.ExecuteReader()
        Try
            If objReader.Read() Then
                If IsDBNull(objReader.Item("GETNOTES_LAST_DATE_SEND")) Then
                    GetStart_Date = Now.AddDays(-1)
                Else
                    GetStart_Date = objReader.Item("GETNOTES_LAST_DATE_SEND")
                End If

            End If
            objReader.Close()
            connectOR.Close()

        Catch OleDBExp As OleDbException
            objWalSCComments.WriteLine("     Error - error reading Start date FROM SDIX_EMAIL_DETAIL " & " " & Now())

            Try
                objReader.Close()
                connectOR.Close()
            Catch exOR As Exception

            End Try
        End Try

        objWalSCComments.WriteLine("  GetStartDate: " & GetStart_Date & " " & Now())
        objWalSCComments.WriteLine("-----------------------------------------------------------------------------")


        Return GetStart_Date
    End Function
    'INC0032110	CLONE - As a Sam's Club User, I need my Work Orders validated in Service Channel, need changes for pushing comments to service channel [ get notes ] - Dhamotharan P

    Private Function GetEndDate() As Date
        Dim strSQLstring As String = ""
        Dim dteEndDate As DateTime = Now

        Dim format As New System.Globalization.CultureInfo("en-US", True)
        strSQLstring = "SELECT to_char(MAX(DTTM_STAMP), 'MM/DD/YY HH24:MI:SS') as MAXDATE FROM ps_isa_xpd_comment WHERE BUSINESS_UNIT = 'WAL00'"

        Dim dr As OleDbDataReader = Nothing

        Try
            objWalSCComments.WriteLine("  GetEndDate: " & strSQLstring & " " & Now())

            Dim command As OleDbCommand
            command = New OleDbCommand(strSQLstring, connectOR)
            If connectOR.State = ConnectionState.Open Then
                'do nothing
            Else
                connectOR.Open()
            End If
            dr = command.ExecuteReader
            Try

                If dr.Read Then
                    dteEndDate = (dr.Item("MAXDATE"))
                    dteEndDate = dteEndDate.AddMinutes(+1)
                Else
                    dteEndDate = Now.ToString
                End If
            Catch ex As Exception
                dteEndDate = Now.ToString
                objWalSCComments.WriteLine("     Error - error reading end date FROM PS_ISAORDSTATUSLOG A" & " " & Now())
            End Try

            dr.Close()
            connectOR.Close()

        Catch OleDBExp As OleDbException
            Try
                dr.Close()
                connectOR.Close()
            Catch exOR As Exception

            End Try
            objWalSCComments.WriteLine("     Error - error reading end date FROM PS_ISAORDSTATUSLOG A" & " " & Now())
        End Try
        objWalSCComments.WriteLine("  GetEndDate: " & dteEndDate & " " & Now())

        Return dteEndDate
    End Function

    Public Function getDBName() As Boolean
        Dim isPRODDB As Boolean = False
        Dim PRODDbList As String = ConfigurationManager.AppSettings("OraPRODDbList").ToString()
        Dim DbUrl As String = ConfigurationManager.AppSettings("OLEDBconString").ToString()
        Try
            DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper()
            isPRODDB = (PRODDbList.IndexOf(DbUrl.Trim.ToUpper) > -1)
        Catch ex As Exception
            isPRODDB = False
        End Try
        Return isPRODDB
    End Function
    'This method is used for sending Success or failure message by validating the http response
    'INC0032110	CLONE - As a Sam's Club User, I need my Work Orders validated in Service Channel, need changes for pushing comments to service channel [ get notes ] - Dhamotharan P
    'Suvetha - INC0041221 - GetNotes utility needs fix to capture And reprocess errors updating work order
    Public Function UpdateNotes(ByVal workOrder As String, Third_party_comp_id As String, Note As String, Ponum As String, Ordernum As String, IsSamsclubWO As Boolean) As String
        Try
            Dim objResult As New resultBO
            Dim APIresponse = String.Empty
            Dim username = String.Empty
            Dim password = String.Empty
            Dim clientkey = String.Empty
            Dim apiurl = String.Empty
            Dim baseurl = String.Empty
            Dim credtype = String.Empty
            Dim grant_type = String.Empty
            Dim SAMSCLUBkey = ConfigurationSettings.AppSettings("SAMSCLUBkey").ToString()
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then

                Dim ds As New DataSet
                If Third_party_comp_id = ConfigurationSettings.AppSettings("CBRECompanyID").ToString() Then
                    ds = GetCredentails("CBRE")
                ElseIf IsSamsclubWO Then
                    ds = GetCredentails(SAMSCLUBkey)
                Else
                    ds = GetCredentails("WALMART")
                End If
                If Not ds Is Nothing Then
                    If ds.Tables.Count > 0 Then
                        If ds.Tables(0).Rows.Count > 0 Then
                            Dim I As Integer
                            For I = 0 To ds.Tables(0).Rows.Count - 1
                                username = ds.Tables(0).Rows(I).Item("Client_ID")
                                password = ds.Tables(0).Rows(I).Item("Client_SECRET")
                                clientkey = ds.Tables(0).Rows(I).Item("Client_KEY")
                                apiurl = ds.Tables(0).Rows(I).Item("TOKENBASEURL")
                                baseurl = ds.Tables(0).Rows(I).Item("BASEURL")
                                credtype = ds.Tables(0).Rows(0).Item("CRED_TYPE")
                                grant_type = ds.Tables(0).Rows(I).Item("GRANT_TYPE")
                            Next
                        End If
                    End If
                End If
                    APIresponse = AuthenticateService(credtype, username, password, clientkey, apiurl, baseurl, grant_type)
                    If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                        If (Not APIresponse.Contains("error_description")) Then
                            Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                            apiurl = baseurl + "/workorders/" + workOrder + "/notes"
                            Dim httpClient As HttpClient = New HttpClient()
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                            httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                            Dim objNoteParam As New UpdateNote
                            objNoteParam.Note = Note
                            objNoteParam.MailedTo = ""
                            objNoteParam.ActionRequired = False
                            objNoteParam.ScheduledDate = Now
                            objNoteParam.Visibility = 0
                            objNoteParam.Actor = ""
                            objNoteParam.NotifyFollowers = False
                            objNoteParam.DoNotSendEmail = True

                            Dim serializedparameter = JsonConvert.SerializeObject(objNoteParam)
                            Dim response = httpClient.PostAsync(apiurl, New StringContent(serializedparameter, Encoding.UTF8, "application/json")).Result
                            Try
                                If response.IsSuccessStatusCode Then
                                    Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                                    objWalSCComments.WriteLine("Result - Success " + Convert.ToString(workorderAPIResponse) + " Work Order-" + workOrder + " PO ID-" + Ponum + " Order No-" + Ordernum + " CredType-" + credtype)
                                'Suvetha - INC0041221 - GetNotes utility needs fix to capture And reprocess errors updating work order
                                Try
                                    objResult = JsonConvert.DeserializeObject(Of resultBO)(workorderAPIResponse)
                                    If objResult.documentId = "" Then
                                        Return "Failed"
                                    Else
                                        Return "Success"
                                    End If
                                Catch ex As Exception
                                End Try
                                Else
                                    objWalSCComments.WriteLine("Result- Failed in API response Work Order-" + workOrder + " PO ID-" + Ponum + " Order No-" + Ordernum + " CredType-" + credtype)
                                    Return "Failed"
                                End If
                            Catch ex As Exception
                                objWalSCComments.WriteLine("Method:UpdateNotes - " + ex.Message)
                            End Try
                        End If
                    Else
                        objWalSCComments.WriteLine("Method:UpdateNotes - " + APIresponse)
                        Return "Failed"
                    End If




                End If
        Catch ex As Exception
            Return "Failed"
            objWalSCComments.WriteLine("Method:UpdateNotes - " + ex.Message)
        End Try

    End Function
    'INC0032110	CLONE - As a Sam's Club User, I need my Work Orders validated in Service Channel, need changes for pushing comments to service channel [ get notes ] - Dhamotharan P

    Private Function GetCredentails(WO_type As String)
        Dim ds As New DataSet
        Dim strQuery As String = ""
        strQuery = "SELECT CLIENT_ID,CLIENT_SECRET,CLIENT_KEY,BASEURL,TOKENBASEURL,CRED_TYPE,GRANT_TYPE FROM SDIX_USERSACCESSTOKEN_TBL where CRED_TYPE= '" & WO_type & "' and BUSINESS_UNIT='I0W01' AND ISACTIVE='Y'"
        ds = ORDBAccess.GetAdapter(strQuery, connectOR)

        Return ds
    End Function
    'INC0032110	CLONE - As a Sam's Club User, I need my Work Orders validated in Service Channel, need changes for pushing comments to service channel [ get notes ] - Dhamotharan P

    Private Function updateLastSendDate(strBU As Object, EndDate As String) As Boolean
        connectOR.Close()
        Dim strSQLstring As String
        'Dim dteEndDate As DateTime
        Dim ds As New DataSet
        Dim bolerror1 As Boolean
        Dim rowsaffected As Integer


        ' The enddate coming from SDIX_EMAIL_DETAIL  is being set back to the original enddate.  The SDIX_EMAIL_DETAIL table
        ' is then updated with the SDIX_EMAIL_DETAIL's endddate and the next time in, the date in the SDIX_EMAIL_DETAIL table is
        ' the startdate.  We increased the enddate a second so we could get all the records from the query.  We were never getting
        ' the last record because of milliseconds were off in the date conversions.  Adding a second we were able to get all
        ' the records in the date range....  If you understand this you have a date to sit with the Dali Lama.. Believe me
        ' it works!!!!!!!!  PFD 4.4.2008
        ' reset the dteEndDate back to original

        'StartDate.AddSeconds(-1)

        strSQLstring = "UPDATE SDIX_EMAIL_DETAIL" & vbCrLf &
                    " SET ISA_NOTES_LAST_DATE = TO_DATE('" & EndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                    " WHERE ORDER_MFG_BU = '" & strBU & "' "

        Try
            Dim Command = New OleDbCommand(strSQLstring, connectOR)
            objWalSCComments.WriteLine("  updateEnterprise (1): " & strSQLstring & " " & Now())
            connectOR.Open()
            rowsaffected = Command.ExecuteNonQuery()
            connectOR.Close()
        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            objWalSCComments.WriteLine("  Error - updating the Enterprise send date " & OleDBExp.ToString & " " & Now())
            bolerror1 = True
        End Try


        If bolerror1 = True Then
            Return True
        Else
            Return False
        End If
    End Function
    'INC0032110	CLONE - As a Sam's Club User, I need my Work Orders validated in Service Channel, need changes for pushing comments to service channel [ get notes ] - Dhamotharan P

    'This method is used for Authentication the credential type
    Public Function AuthenticateService(credType As String, username As String, password As String, clientKey As String, apiurl As String, baseurl As String, grant_type As String) As String
        Try
            Dim httpClient As HttpClient = New HttpClient()
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            'Dim username As String = String.Empty
            'Dim password As String = String.Empty
            'Dim clientKey As String = String.Empty
            'If credType = "CBRE" Then
            '    username = ConfigurationSettings.AppSettings("WMUName")
            '    password = ConfigurationSettings.AppSettings("WMPassword")
            '    clientKey = ConfigurationSettings.AppSettings("WMClientKey")
            'Else
            '    username = ConfigurationSettings.AppSettings("CBREUName")
            '    password = ConfigurationSettings.AppSettings("CBREPassword")
            '    clientKey = ConfigurationSettings.AppSettings("CBREClientKey")
            'End If
            'Dim apiurl As String = ConfigurationSettings.AppSettings("ServiceChannelLoginEndPoint")
            Dim formContent = New FormUrlEncodedContent({New KeyValuePair(Of String, String)("username", username), New KeyValuePair(Of String, String)("password", password), New KeyValuePair(Of String, String)("grant_type", grant_type)})
            httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Basic", clientKey) 'Add("Authorization", "Basic " + clientKey)
            Dim response = httpClient.PostAsync(apiurl, formContent).Result
            If response.IsSuccessStatusCode Then
                Dim APIResponse = response.Content.ReadAsStringAsync().Result
                Return APIResponse
            Else
                Dim APIResponse = response.Content.ReadAsStringAsync().Result
                'Dim eobj As ExceptionHelper = New ExceptionHelper()
                'eobj.writeExceptionMessage(APIResponse, "AuthenticateService")
                If APIResponse.Contains("error_description") Then Return APIResponse
                Return "Server Error"
            End If

        Catch ex As Exception
            objWalmartSC.WriteLine("Method:AuthenticateService - " + ex.Message)
        End Try
        Return "Server Error"
    End Function

End Module


Public Class ValidateUserResponseBO
    Public Property access_token As String
    Public Property refresh_token As String
End Class

Public Class UpdateNote
    Public Property Note As String
    Public Property MailedTo As String
    Public Property ActionRequired As Boolean
    Public Property ScheduledDate As DateTime
    Public Property Visibility As Integer
    Public Property Actor As String
    Public Property NotifyFollowers As Boolean
    Public Property DoNotSendEmail As Boolean
End Class
Public Class OrderStatusDetail
    Public Property message As String
    Public Property orderStatus As String
    Public Property statusDesc As String
    Public Property dueDate As String
End Class
Public Class WOStatus
    Public Property Primary As String
    Public Property Extended As String
    Public Property CanCreateInvoice As String
End Class
Public Class Notes
    Public Property Last As Last
End Class
Public Class Last
    Public Property NoteData As String = String.Empty
End Class

Public Class Location
    Public Property StoreId As String = String.Empty
End Class
Public Class Asset
    Public Property Tag As String = String.Empty
End Class


Public Class WorkOrderDetails
    Public Property Notes As Notes
    Public Property Location As Location
    Public Property Asset As Asset
    Public Property PurchaseNumber As String = String.Empty

End Class

Public Class CheckWo
    Public Property OdataContext As String
    Public Property Status As WOStatus
End Class
Public Class UpdateWorkOrderBO
    Public Property Status As Status
    Public Property Note As String
End Class
Public Class Status
    Public Property Primary As String
    Public Property Extended As String
End Class
Public Class resultBO
    Public Property id As String
    Public Property documentId As String

End Class
