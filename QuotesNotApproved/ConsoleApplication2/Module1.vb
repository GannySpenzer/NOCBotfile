Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
Imports ConsoleApplication2.SQLDBAccess
Imports ConsoleApplication2.WebPartnerFunctions.WebPSharedFunc
Imports System.Web.UI.WebControls
Imports System.Drawing.Color
Imports System.Data.SqlClient
Imports System.Data
Imports System.Threading
Imports Microsoft.VisualBasic
Imports System.Runtime.InteropServices
Imports ConsoleApplication2.AEESAppvFunctions
Imports System.Math
Imports SDI.ApplicationLogger

Module Module1

    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\QuotesNotApproved_threshhold"
    Dim logpath As String = "C:\QuotesNotApproved_threshhold\LOGS\QuotedsNotApproved_threshhold" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=MSDAORA.1;Password=EINTERNET;User ID=EINTERNET;Data Source=prod")
    Dim ORDB As String = "rptg"
    Dim SQLDB As String = "CPLUS_PROD"
    Dim SQLDBPROD As String = "DAZZLE"
    Dim connectCplus As New SqlConnection("server=" & SQLDB & ";uid=einternet;pwd=einternet;Initial Catalog=Contentplus;Data Source=" & SQLDB & "")

    Sub Main()
        Console.WriteLine("Start Aees_orders_threshhold program")
        Console.WriteLine("")
        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
        End If
        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine(" Email the Approver's Boss when last Update Date is greater than 2 days " & Now())
        ' right now hard coded for University of PENN.
        '  need to create an XML with all BU's that will use this program
        Dim bolError As Boolean = getOrdersover2days("I0206")
        If bolError = True Then
            SendEmail()
        End If
        objStreamWriter.WriteLine("  End of QuotesNotApproved_threshhold Email send " & Now())
        objStreamWriter.Flush()
        objStreamWriter.Close()
    End Sub
    Function getOrdersover2days(ByVal strBU As String)
        Dim I As Integer
        Dim strSQLstring As String

        strBU = getbusinessUnit()
        If strBU = "ZN" Then
            strBU = "I0206"
        End If
        ' query goes out and gets orders that are waiting for qupote approval at the site greater than 2 days.
        strSQLstring = "select " & vbCrLf & _
                              " distinct  " & vbCrLf & _
                                    "h.Order_no," & vbCrLf & _
                                    "h.Business_unit_om," & vbCrLf & _
                                    "h.OPRID_APPROVED_BY, " & vbCrLf & _
                                    "h.APPROVAL_DATE," & vbCrLf & _
                                    "P.ISA_employee_id, " & vbCrLf & _
                                    "P.ISA_EMPLOYEE_NAME, " & vbCrLf & _
                                    "A.ISA_IOL_APR_EMP_ID, " & vbCrLf & _
                                    "(select  z.ISA_EMPLOYEE_NAME  from  PS_ISA_USERS_TBL Z" & vbCrLf & _
                                         "where Z.ISA_EMPLOYEE_ID = A.ISA_IOL_APR_EMP_ID ) ONE_LEVEL_UP_Approver " & vbCrLf & _
                             "from" & vbCrLf & _
                                    "PS_isa_ord_intfc_h h, " & vbCrLf & _
                                    "PS_isa_ord_intfc_L  l , " & vbCrLf & _
                                    "PS_isa_users_tbl P," & vbCrLf & _
                                    "PS_ISA_USERS_APPRV A," & vbCrLf & _
                                    "ps_REQ_HDR R " & vbCrLf & _
                             "where" & vbCrLf & _
                                    "h.order_status = 'Q'" & vbCrLf & _
                                     "and R.req_status= 'Q'" & vbCrLf & _
                                     "and R.req_id = h.order_no" & vbCrLf & _
                                    "and l.isa_order_status = '2'" & vbCrLf & _
                                    "and P.isa_employee_id   = H.OPRID_APPROVED_by " & vbCrLf & _
                                    "and P.ISA_EMPLOYEE_ID = A.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    "and H.OPRID_APPROVED_BY = A.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    "and h.isa_identifier = l.isa_parent_ident  " & vbCrLf & _
                                    "and h.business_unit_om in '" & strBU & "'" & vbCrLf & _
                                    "and trunc(H.APPROVAL_DATE)< trunc(sysdate) -2 " & vbCrLf & _
                            "order by H.approval_date desc"
        ''" & strEmpID & "'"
        Dim ds As New DataSet
        Try
            ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)
        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            objStreamWriter.WriteLine("     Error - error reading transaction FROM Interface Tables A")
            Return True
        End Try
        If IsDBNull(ds.Tables(0).Rows.Count) Or (ds.Tables(0).Rows.Count) = 0 Then
            objStreamWriter.WriteLine("     Warning - no orders more than two days needed to be approved")
            Return False
        End If
        ' Dim rowsaffected As Integer


        ' connectOR.Open()

        'Dim I As Integer
        'Dim X As Integer
        Dim dsEmail As New DataTable
        Dim dr1 As DataRow

        dsEmail.Columns.Add("Order_no")
        dsEmail.Columns.Add("ISA_employee_id")
        dsEmail.Columns.Add("ISA_EMPLOYEE_NAME")
        dsEmail.Columns.Add("ISA_IOL_APR_EMP_ID")
        dsEmail.Columns.Add("ONE_LEVEL_UP_Approver")
        dsEmail.Columns.Add("Date_Entered")
        For I = 0 To ds.Tables(0).Rows.Count - 1
            dr1 = dsEmail.NewRow()
            dr1.Item(0) = ds.Tables(0).Rows(I).Item("Order_no")
            dr1.Item(1) = ds.Tables(0).Rows(I).Item("ISA_employee_id")
            dr1.Item(2) = ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_NAME")
            dr1.Item(3) = ds.Tables(0).Rows(I).Item("ISA_IOL_APR_EMP_ID")
            dr1.Item(4) = ds.Tables(0).Rows(I).Item("ONE_LEVEL_UP_Approver")
            dr1.Item(5) = ds.Tables(0).Rows(I).Item("APPROVAL_DATE")
            dsEmail.Rows.Add(dr1)
            If I = ds.Tables(0).Rows.Count - 1 Then
                sendBuyerEmail1(dsEmail, _
                ds.Tables(0).Rows(I).Item("Order_no"), _
                ds.Tables(0).Rows(I).Item("ISA_employee_id"), _
                ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_NAME"), _
                ds.Tables(0).Rows(I).Item("ISA_IOL_APR_EMP_ID"), _
                ds.Tables(0).Rows(I).Item("ONE_LEVEL_UP_Approver"), _
                ds.Tables(0).Rows(I).Item("APPROVAL_DATE"))
                dsEmail.Clear()
                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If
            ElseIf ds.Tables(0).Rows(I + 1).Item("Order_no") <> _
                          ds.Tables(0).Rows(I).Item("Order_no") Then
                sendBuyerEmail1(dsEmail, _
                ds.Tables(0).Rows(I).Item("Order_no"), _
                ds.Tables(0).Rows(I).Item("ISA_employee_id"), _
                ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_NAME"), _
                  ds.Tables(0).Rows(I).Item("ISA_IOL_APR_EMP_ID"), _
                ds.Tables(0).Rows(I).Item("ONE_LEVEL_UP_Approver"), _
                ds.Tables(0).Rows(I).Item("APPROVAL_DATE"))
                dsEmail.Clear()
                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If
            End If
            Dim strorderno As String = ds.Tables(0).Rows(I).Item("Order_no")
            objStreamWriter.WriteLine("Order number: " & strorderno & "   Approver hasn't approved quote in over 48 hours " & Now())
        Next
        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If
        Return False
    End Function


    Private Sub sendBuyerEmail1(ByVal dsEmail As DataTable, _
                          ByVal strorder_no As String, _
                          ByVal stremplid As String, _
                          ByVal strempname As String, _
                          ByVal stroneupid As String, _
                          ByVal strONE_LEVEL_UP_Approver As String, _
                          ByVal strapprdte As String)
        Dim strbodyhead As String
        Dim strbodydetl As String
        Dim txtBody As String
        Dim txtHdr As String
        Dim txtMsg As String
        Dim dataGridHTML As String
        Dim SBnstk As New StringBuilder
        Dim SWnstk As New StringWriter(SBnstk)
        Dim htmlTWnstk As New HtmlTextWriter(SWnstk)
        Dim bolSelectItem As Boolean
        Dim strFirstName As String
        Dim strLastName As String
        Dim stremp_email As String
        Dim strPO_ID As String
        Dim strbuer As String
        Dim dteCompanyID As String
        Dim strempid As String = " "
        Dim Mailer As MailMessage = New MailMessage
        'these need to be put into an xml file
        Dim strccfirst As String = "erwin.bautista"  '  "pete.doyle"
        Dim strcclast As String = "sdi.com"
        Mailer.From = "SDIExchange@SDI.com"  '  "Insiteonline@SDI.com"
        ' Mailer.Cc = "Joe.bowers@sdi.com"
        Mailer.Bcc = strccfirst & "@" & strcclast
        strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
        strbodyhead = strbodyhead & "<center><span >SDiExchange </span></center>"
        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        Dim strSQLstring As String
        Dim ds1 As New DataSet
        Dim i As Integer
        Dim strbu As String
        ds1.Dispose()
        Dim ds As DataSet
        Dim stremail As String = " "
        Dim sEmailCC As String = " "
        'get email address here
        ds = GetUserInfo("I0206", stroneupid)
        If ds.Tables(0).Rows.Count > 0 Then
            Try
                stremail = ds.Tables(0).Rows(0).Item("ISA_EMPLOYEE_EMAIL")
                sEmailCC = ds.Tables(0).Rows(0).Item("isa_site_email")

            Catch ex As Exception
                stremail = "erwin.bautista@sdi.com;Tony.smith@sdi.com"
                sEmailCC = "karyn.crumbock@sdi.com;debbie.stephenson@sdi.com"
            End Try
        Else
            stremail = "erwin.bautista@sdi.com;Tony.smith@sdi.com"
            sEmailCC = "karyn.crumbock@sdi.com;debbie.stephenson@sdi.com"
        End If
        ds.Dispose()
        Dim dtgEmail As WebControls.DataGrid
        dtgEmail = New WebControls.DataGrid
        dtgEmail.DataSource = dsEmail
        dtgEmail.DataBind()
        dtgEmail.CellPadding = 3
        'dtgEmail.Width.Percentage(90)
        'dtgEmail.Width.Percentage(70)
        Unit.Percentage(70)
        dtgEmail.RenderControl(htmlTWnstk)
        dataGridHTML = SBnstk.ToString()
        Dim strPurchaserEmail As String = stremp_email
        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"
        strbodydetl = strbodydetl & "<p >To: " & strONE_LEVEL_UP_Approver & ", &nbsp;<br><br>"
        strbodydetl = strbodydetl & "Order Number: &nbsp;&nbsp;" & strorder_no & "&nbsp"
        strbodydetl = strbodydetl & "has been waiting for a Quote Approval over 48 hours. "
        strbodydetl = strbodydetl & "Please have the Approver log on to <a href= http://www.sdiexchange.com >&nbsp;<B>SDiExchange</B> </a>" & " to approve the Quote.<br>"
        strbodydetl = strbodydetl & "You will receive an email everyday until this Order has a resolution. <br><br>"
        strbodydetl = strbodydetl & "If you have questions please contact SDI&#39;s Help desk at 888-435-7734 "
        strbodydetl = strbodydetl & "&nbsp;</p>"
        strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        strbodydetl = strbodydetl & "</TABLE>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "Sincerely,<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "SDI Customer Care<br>"
        strbodydetl = strbodydetl & "</p>"
        strbodydetl = strbodydetl & "</div>"

        Mailer.Body = strbodyhead & strbodydetl
        If connectOR.DataSource.ToUpper = "RPTG" Or _
            connectOR.DataSource.ToUpper = "DEVL" Or _
            connectOR.DataSource.ToUpper = "PLGR" Then
            Mailer.To = "DoNotSendPLGR@sdi.com"
        Else
            Mailer.To = stremail
        End If
        Mailer.Bcc = "erwin.bautista@sdi.com;vitaly.rovensky@sdi.com"
        Mailer.Cc = sEmailCC  '   ds.Tables(0).Rows(0).Item("ISA_EMPLOYEE_EMAIL")
        '"karyn.crumbock@sdi.com; carol.silverman@sdi.com"

        Mailer.Subject = "SDiExchange - Order #: " & strorder_no & " waiting for quote approval > than 48 hrs"
        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
        'SmtpMail.Send(Mailer)
        'SendEmail(Mailer)
        'Dim YesNOEmail As String

        '[ Public Shared Function GetUserInfo(ByVal strBU As String, ByVal strEmpID As String) As DataSet
        Mailer.To = stremail
        If Mailer.To = "ZN" Then
        Else
            UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, Mailer.From, Mailer.To, Mailer.Cc, Mailer.Bcc, "N", Mailer.Body, connectOR)

        End If
    End Sub ' sendBuyerEmail1
    Private Sub SendEmail()
        Dim email As New MailMessage
        'The email address of the sender
        email.From = "TechSupport@sdi.com"
        'The email address of the recipient. 
        email.To = "erwin.bautista@sdi.com"
        'The subject of the email
        email.Subject = "UPENN updOrdAppr_UPENN Error Alert"
        'The Priority attached and displayed for the email
        email.Priority = System.Net.Mail.MailPriority.High
        'email.BodyFormat = System.Net.Mail.MailMessage
        email.BodyFormat = MailFormat.Html
        email.Body = "<html><body><table><tr><td>updOrdAppr_UPENN has completed with errors, review log.</td></tr>"
        Try
            UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "", "Y", email.Body, connectOR)
        Catch
            objStreamWriter.WriteLine("     Error - the email was not sent")
        End Try

    End Sub
    Private Function getbusinessUnit()
        ' get XML file of sites that require email

        Dim strXMLDir As String = rootDir & "\Business_Unit_Sites_Order_Approver.xml"
        Dim xmldata As New XmlDocument
        Dim sr As System.IO.StreamReader
        Dim XMLContent As String
        Dim jobNode As XmlNode
        sr = New System.IO.StreamReader(strXMLDir)
        XMLContent = sr.ReadToEnd()
        sr.Close()
        xmldata.LoadXml(XMLContent)
        'Dim jj As XmlNode = xmldata.ChildNodes(2)
        jobNode = xmldata.ChildNodes(1)
        Dim dsRows As New DataSet
        dsRows.ReadXml(New XmlNodeReader(jobNode))
        Dim I As Integer
        Dim bolErrorSomeWhere As Boolean
        'Return (stremail)
        ' check email from the ps_ISA_ORD_INTFC_h
        '        <xmlData>
        '           <SITE>
        '			   <SITE_HELP_DESK_CONTACT>tony.smith@sdi.com;erwin.bautista@sdi.com</SITE_HELP_DESK_CONTACT>
        '			   <SITE_MANAGER_CONTACT>joe.bowers@sdi.com</SITE_MANAGER_CONTACT>
        '	           <SITE_BU>I0206</SITE_BU>
        '	        </SITE>   
        '</xmlData>
        Dim br As String = " "
        Try
            For I = 0 To dsRows.Tables(0).Rows.Count - 1
                If br = " " Then
                    br = dsRows.Tables(0).Rows(I).Item("SITE_BU")
                Else
                    br = br + "','" + dsRows.Tables(0).Rows(I).Item("SITE_BU") + "'"
                End If
            Next
        Catch ex As Exception
            Return "ZN"
        End Try
        Return br
    End Function

End Module
