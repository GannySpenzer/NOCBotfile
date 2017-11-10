Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
 Imports System.Web.UI.WebControls
 Imports System.Data.SqlClient
Imports System.Data
 Imports System.Threading
Imports Microsoft.VisualBasic
 Imports System.Security.Cryptography
 Imports System.Net
 Imports System.Web.HttpBrowserCapabilities
Imports System.Threading.Thread
Imports System.Collections.Generic

Module Module1
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\ContainerLoad"
    Dim logpath As String = "C:\ContainerLoad\LOGS\ContainerLoad" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=PLGR")

    Sub Main()

        Dim cnStringORA As String = "Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=PLGR"
        Try
            cnStringORA = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
            cnStringORA = ""
        End Try
        If (cnStringORA.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            connectOR = New OleDbConnection(cnStringORA)
        End If

        'As what we have talked about last night. 
        ' Can you write a quick report for Moveway that will send alert
        ' of what had been “missed / not loaded” for the previous day’s
        ' (and earlier if any) operation? 
        ' Here() 's a quick specs and give me a call for any questions.

        '•	Report will automatically be emailed to Moveway – mreyes@moveway.com.
        '  Need this to be configurable.  I think his boss will also be interested on this.
        '   We also need to be on the BCC so we can audit.
        '	It will include the following fields from PS_ISA_LOAD_INT table – ONLY 1 table.
        'o	Load ID
        'o	Stop No
        'o	School Code/Stop ID/Ship To ID
        'o	Container ID
        'o	Container Type
        '•	Report will include anything that was not loaded from previous day(s) load …
        '   ie., if today is 01/11, any container not loaded for loads 01/10 and earlier will be included on the email.  
        '   The load ID is on the YYMMDDxxxx format where “xxxx” is a sequential number starting from 1.
        '•	Since we ran a schedule job that “finalizes” everything that got loaded for previous day’s operation
        ' at about 8:30 AM (this is to prevent previous day’s loads to come back down to the device),
        ' we() 'll schedule this report to run every 9:00 AM.



        Console.WriteLine("Start ContainerLoad")
        Console.WriteLine("")

        'If Dir(rootDir, FileAttribute.Directory) = "" Then
        '    MkDir(rootDir)
        'End If
        'If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
        '    MkDir(rootDir & "\LOGS")
        'End If

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Sending email to Moveway out " & Now())

        Dim bolError As Boolean = buildSendContainerOut()

        If bolError = True Then
            'SendErrEmail()
        End If
        objStreamWriter.WriteLine("End of buildSendContainerOut Program " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub
    Function buildSendContainerOut() As Boolean
        Dim strSQLstring As String = ""
        Dim dteStrDate As Date = Now.AddMonths(-3).ToString
        'Now().ToString("yyyy-M-d")
        Dim dteStr As String = Now().ToString("yyMMdd")
        Dim d As String = dteStrDate.ToString

        strSQLstring = "SELECT " & vbCrLf & _
        " Load_id " & vbCrLf & _
        ", SHIPTO_ID" & vbCrLf & _
        ", ORDER_NO" & vbCrLf & _
        ", SHIP_CNTR_ID" & vbCrLf & _
        ", LOAD_OPRID" & vbCrLf & _
        ", DRIVER_ID" & vbCrLf & _
        ", PICK_CONFIRM_DTTM" & vbCrLf & _
        ", BNK_LOAD_DTTM" & vbCrLf & _
        ", ISA_SCAN_OFF_DTTM" & vbCrLf & _
        ", ISA_DEL_COMP_DTTM" & vbCrLf & _
        ", ISA_STOP_COMP_DTTM" & vbCrLf & _
        ", ISA_LATTITUDE" & vbCrLf & _
        ", ISA_LONGITUDE" & vbCrLf & _
        ", SHIPTO_ID_RESP" & vbCrLf & _
        ", PACKING_VOLUME" & vbCrLf & _
        ", PACKING_WEIGHT" & vbCrLf & _
        ", ISA_LANE_ID" & vbCrLf & _
        ", ISA_STOP_NBR" & vbCrLf & _
        ", ISA_LOAD_BEGINDTTM" & vbCrLf & _
        ", ISA_LOAD_endDTTM" & vbCrLf & _
        ", ISA_RT_BEGINDTTM" & vbCrLf & _
        ", ISA_RT_ENDDTTM" & vbCrLf & _
        ", CONTAINER_TYPE" & vbCrLf & _
        ", ISA_LOAD_ENDDTTM" & vbCrLf & _
        ", ENTRY_TYPE" & vbCrLf & _
        ", ISA_LOAD_STATUS" & vbCrLf & _
        ", ISA_TRUCK_ID" & vbCrLf & _
        ", NOTES_1000" & vbCrLf & _
        "  from " & vbCrLf & _
        "  PS_ISA_LOAD_INT " & vbCrLf & _
        "  where " & vbCrLf & _
        "  isa_load_status  in ('N','U','H')" & vbCrLf & _
        "  AND   substr(Load_id,0,6) >  '110101' and substr(Load_id,0,6) < '" & dteStr & "'" & vbCrLf & _
        "  order by load_id, isa_stop_nbr,SHIP_CNTR_ID,CONTAINER_TYPE"

        Dim Command As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)
        connectOR.Open()
        Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("     error selecting from Container Load - Table PS_ISA_Load_INIT")
            objStreamWriter.WriteLine("         " & ex.Message)
            connectOR.Close()
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            objStreamWriter.WriteLine("  sendCustEmails select emails = 0")
            connectOR.Close()
            Return False
        End If

        connectOR.Open()
        'Dim strPreOrderno As String
        Dim I As Integer
        'Dim X As Integer
        Dim dsEmail As New DataTable
        Dim dr1 As DataRow


        dsEmail.Columns.Add("Load ID")
        dsEmail.Columns.Add("Stop number")
        dsEmail.Columns.Add("Container Type")
        dsEmail.Columns.Add("School Code")
        dsEmail.Columns.Add("Container_ID")
        dsEmail.Columns.Add("Order Number")
        dsEmail.Columns.Add("Load Status")


        For I = 0 To ds.Tables(0).Rows.Count - 1

            Try
                dr1 = dsEmail.NewRow()
                dr1.Item(0) = ds.Tables(0).Rows(I).Item("Load_id")
                dr1.Item(1) = ds.Tables(0).Rows(I).Item("isa_stop_nbr")
                dr1.Item(2) = ds.Tables(0).Rows(I).Item("container_type")
                dr1.Item(3) = ds.Tables(0).Rows(I).Item("shipto_id")
                dr1.Item(4) = ds.Tables(0).Rows(I).Item("SHIP_CNTR_ID")
                dr1.Item(5) = ds.Tables(0).Rows(I).Item("ORDER_NO")
                dr1.Item(6) = ds.Tables(0).Rows(I).Item("isa_load_status")
                dsEmail.Rows.Add(dr1)

            Catch ex As Exception

            End Try


            If I = ds.Tables(0).Rows.Count - 1 Then


                sendEmailtoAgent(dsEmail)

                dsEmail.Clear()
                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If


            End If

        Next

        objStreamWriter.WriteLine("  Container Load e-mail sent. Number of lines in grid = " & ds.Tables(0).Rows.Count)

        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If

    End Function
    
    Private Sub sendEmailtoAgent(ByVal dsEmail As DataTable)

        '  we want to read the emailSites xml to get the userid and the business unit then read user table to load the email address and the employee name.

        Dim Mailer As MailMessage = New MailMessage
        Dim strBodyhead As String
        Dim strbodydetl As String
        Dim strAssignedtoname As String = "Mike Rayes"
        Dim strToLastName As String = "Rayes"
        Dim strToFirstName As String = "Mike"
        Dim strfromLastName As String = "Rovensky"
        Dim strfromFirstName As String = "Vitaly"

        Dim strbodyhead1 As String
        Dim strbodydet1 As String
        Dim txtBody1 As String
        Dim txtHdr1 As String
        Dim txtMsg1 As String
        Dim dataGridHTML1 As String
        Dim SBnstk1 As New StringBuilder
        Dim SWnstk1 As New StringWriter(SBnstk1)
        Dim htmlTWnstk1 As New HtmlTextWriter(SWnstk1)
        'Dim bolSelectItem1 As Boolean

         
        'Dim Mailer1 As MailMessage = New MailMessage
        Dim strccfirst1 As String = "vitaly.rovensky"  '  "erwin.bautista"
        Dim strcclast1 As String = "sdi.com"
        Mailer.From = "SDIExchange@SDI.com"
        Mailer.Cc = ""
        Mailer.Bcc = strccfirst1 & "@" & strcclast1
        strbodyhead1 = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
        strbodyhead1 = strbodyhead1 & "<center><span >SDiExchange - List of Containers Pending Load  </span></center>"
        strbodyhead1 = strbodyhead1 & "&nbsp;" & vbCrLf
        'List of Containers Pending Load

        Dim dtgEmail1 As WebControls.DataGrid
        dtgEmail1 = New WebControls.DataGrid

        dtgEmail1.DataSource = dsEmail
        dtgEmail1.DataBind()

        dtgEmail1.CellPadding = 3
        'dtgEmail1.Width.Percentage(90)

        'dtgPO.Columns(9).ItemStyle.HorizontalAlign = HorizontalAlign.Center
        dtgEmail1.RenderControl(htmlTWnstk1)
        dataGridHTML1 = SBnstk1.ToString()

        'Dim strPurchaserName As String = strCustID
        'Dim strPurchaserName As String = strFirstName & _
        '   " " & strLastName
        Dim ted As String = ";vitaly.rovensky@sdi.com"
        'Dim strPurchaserEmail As String = strEmail & ted
        'Dim strPurchaserEmail As String = strEmail
        strbodydet1 = "&nbsp;" & vbCrLf
        strbodydet1 = strbodydet1 & "<div>"
        'strbodydet1 = strbodydet1 & "<p >Hello " & strPurchaserName & ",<br>"
        'strbodydet1 = strbodydet1 & "&nbsp;<BR>"
        'strbodydet1 = strbodydet1 & "Order Number: " & strOrderNo & " <br> "
        'strbodydet1 = strbodydet1 & "Order contents:<br>"
        'strbodydet1 = strbodydet1 & "&nbsp;<BR>"
        ' strbodydet1 = strbodydet1 & "Order Status:  " & strOrderStatDesc & " <br>"
        'strbodydet1 = strbodydet1 & "Order Number:  " & strOrderNo & " <br>"
        ' strbodydet1 = strbodydet1 & "Line Number:  " & strLineNbr & " <br>"
        strbodydet1 = strbodydet1 & "&nbsp;</p>"
        strbodydet1 = strbodydet1 & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydet1 = strbodydet1 + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML1 & "</TD></TR>"
        strbodydet1 = strbodydet1 + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        strbodydet1 = strbodydet1 & "</TABLE>" & vbCrLf

        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "Sincerely,<br>"
        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "SDI Customer Care<br>"
        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "</p>"
        strbodydet1 = strbodydet1 & "</div>"




        Mailer.Body = strbodyhead1 & strbodydet1

        Mailer.To = "Peter.Casale@sdi.com;Michael.Marrinan@sdi.com;vitaly.rovensky@sdi.com"
        Dim sTO As String = ""
        Try
            sTO = CStr(My.Settings("sendEmailToAgent_TO")).Trim
        Catch ex As Exception
        End Try
        If (sTO.Trim.Length > 0) Then
            Mailer.To = sTO
        End If

        If connectOR.DataSource.ToUpper = "RPTG" Or _
            connectOR.DataSource.ToUpper = "DEVL" Or _
            connectOR.DataSource.ToUpper = "STAR" Or _
            connectOR.DataSource.ToUpper = "PLGR" Then
            Mailer.To = "webdev@sdi.com"
        Else
            
        End If

        Mailer.Subject = "SDiExchange - List of Containers Pending Load  " & Now.Month & "/" & Now.Day & "/" & Now.Year

        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
       

        Mailer.Body = strbodyhead1 & strbodydet1
        ' ++++++++++++++++++++++++++++++++++++++++++

        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

        Dim bSend As Boolean = False
        Try

            SendLogger(Mailer.Subject, Mailer.Body, "CONTAINERLOAD", "Mail", Mailer.To, "", Mailer.Bcc)
            bSend = True
        Catch ex As Exception

        End Try

        'UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, Mailer.From, Mailer.To, "", Mailer.Bcc, "N", Mailer.Body, connectOR)

        ''    <setting name="sendEmailToAgent_TO" serializeAs="String">
        ''            <value>Michael.Marrinan@sdi.com;vitaly.rovensky@sdi.com</value>
        ''    </setting>
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            SDIEmailService.EmailUtilityServices(MailType, "SDIExchange@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

End Module
