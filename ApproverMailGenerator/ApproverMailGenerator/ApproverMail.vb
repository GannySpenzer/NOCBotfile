Imports ApproverMailGenerator.ORDBData
Imports System.Data.OleDb
Imports System.Web
Imports System.Web.Mail
Imports System.Net
Imports System.Collections.Specialized
Imports System.Text
Imports System.IO
Imports System.Security.Cryptography
Imports System.Configuration
Imports System.Web.UI.WebControls
Imports System.Web.UI
Imports System.Drawing.Color

Module ApproverMail

    Private Const QUERY_STRING_DELIMITER As Char = "&"
    Private _cryptoProvider As RijndaelManaged
    Private ReadOnly Key As Byte() = {18, 19, 6, 24, 37, 22, 4, 22, 17, 7, 11, 9, 13, 12, 6, 23}
    Private ReadOnly IV As Byte() = {14, 2, 15, 7, 5, 9, 12, 8, 4, 47, 16, 12, 1, 32, 29, 18}
    Private m_cClassFileName As String = "OrderApprovals.vb"
    Private m_cClassName As String = "OrderApprovals."
    Private m_cDeletedLine As String = "QTR"  '  "D"
    Private m_cApproverDelimiter As String = "||"
    Sub Main()
        GenerateApproverMail()
    End Sub

    Private Sub GenerateApproverMail()
        Dim strError As String = ""
        Try
            Dim ApprDT As DataTable = GetApproverMailList()
            Try
                Dim strreqID As String = ""
                Dim strUserId As String = ""
                Dim strBU As String = ""
                Dim strAgent As String = ""
                If Not ApprDT Is Nothing Then
                    If ApprDT.Rows.Count > 0 Then
                        For Each row As DataRow In ApprDT.Rows
                            'ORDER_NO,BUSINESS_UNIT_OM,ISA_USER4,ISA_EMPLOYEE_ID
                            strreqID = row("ORDER_NO").ToString()
                            strUserId = row("ISA_EMPLOYEE_ID").ToString()
                            strBU = row("BUSINESS_UNIT_OM").ToString()

                            Dim oApprovalResults As New ApprovalResults

                            OrderApprovals.SetInitialOrderStatus(strBU, strUserId, strreqID, oApprovalResults)




                            'CheckSTKLimits(strBU, strUserId, strreqID, "", "W")
                            'If oApprovalResults.IsMoreApproversNeeded Then
                            '    If oApprovalResults.OrderExceededLimit Then
                            '        buildNotifyApprover(strreqID, strUserId, strBU, oApprovalResults.NextOrderApprover, oApprovalResults.NewOrderHeaderStatus)
                            '    Else
                            '        UpdateOrderStatus(strBU, strreqID)
                            '        'ElseIf oApprovalResults.IsAnyChargeCodeExceededLimit Then
                            '        '    For I = 0 To oApprovalResults.BudgetChargeCodesCount - 1
                            '        '        If oApprovalResults.BudgetExceededLimit(I) Then
                            '        '            buildNotifyApprover(strreqID, strAgent, strBU, oApprovalResults.NextBudgetApprover(I), oApprovalResults.NewBudgetHeaderStatus(I))
                            '        '        End If
                            '        '    Next
                            '    End If
                            'End If
                        Next
                    Else
                        ' no orders to approve
                    End If
                Else
                    ' no orders to approve
                End If
            Catch ex As Exception
                ' error trying to approve
            End Try

        Catch ex As Exception
            ' error trying to approve
        End Try
    End Sub

    'Private Function VaildateSite(ByVal StrBU As String)
    '    Dim IsVaild As Boolean = False
    '    Try
    '        Dim StrSite As String = "SELECT ISA_BYP_RQSTR_APPR FROM SYSADM8.PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT='" & StrBU & "'"
    '        Dim StrRQSTR As String = ORDBData.GetScalar(StrSite)
    '        If StrRQSTR.ToUpper() = "Y" Then
    '            IsVaild = True
    '        End If
    '        Return IsVaild
    '    Catch ex As Exception
    '        Return False
    '    End Try
    'End Function

    Private Function GetApproverMailList() As DataTable
        Dim dt As DataTable = Nothing
        Try
            Dim StrAppr As String = "SELECT DISTINCT ORDER_NO,BUSINESS_UNIT_OM,ISA_USER4,ISA_EMPLOYEE_ID FROM SYSADM8.PS_ISA_ORD_INTF_LN WHERE ISA_LINE_STATUS='QTW' AND ISA_EMPLOYEE_ID = OPRID_APPROVED_BY AND  " & vbCrLf & _
                " BUSINESS_UNIT_OM IN (SELECT ISA_BUSINESS_UNIT FROM SYSADM8.PS_ISA_ENTERPRISE WHERE UPPER(ISA_BYP_RQSTR_APPR)='Y')"
            Dim ds As DataSet = ORDBData.GetAdapterSpc(StrAppr)
            If Not ds Is Nothing Then
                If ds.Tables.Count > 0 Then
                    dt = ds.Tables(0)
                Else
                    dt = Nothing
                End If

            Else
                dt = Nothing
            End If

            Return dt
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    'Public Sub buildNotifyApprover(ByVal strreqID As String, ByVal strAgent As String, ByVal strBU As String, ByVal strAppUserid As String, ByVal strHldStatus As String)
    '    'this is where we will put in the description of the order per S.Roudriquez
    '    'pfd
    '    'Gives us a reference to the current asp.net 
    '    'application executing the method.
    '    'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

    '    Dim strSQLString As String
    '    Dim strappName As String
    '    Dim strappEmail As String
    '    Dim strBuyerName As String

    '    strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
    '            " LAST_NAME_SRCH," & vbCrLf & _
    '            " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
    '            " FROM SDIX_USERS_TBL" & vbCrLf & _
    '            " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
    '            " AND ISA_EMPLOYEE_ID = '" & strAppUserid & "'"

    '    Dim dtrAppReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

    '    If dtrAppReader.HasRows() = True Then
    '        dtrAppReader.Read()
    '        strappName = dtrAppReader.Item("FIRST_NAME_SRCH") & " " & dtrAppReader.Item("LAST_NAME_SRCH")
    '        strappEmail = dtrAppReader.Item("ISA_EMPLOYEE_EMAIL")
    '        dtrAppReader.Close()
    '    Else
    '        dtrAppReader.Close()
    '        Exit Sub
    '    End If

    '    strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
    '            " LAST_NAME_SRCH," & vbCrLf & _
    '            " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
    '            " FROM SDIX_USERS_TBL" & vbCrLf & _
    '            " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
    '            " AND ISA_EMPLOYEE_ID = '" & strAgent & "'"

    '    Dim dtrBuyerReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

    '    If dtrBuyerReader.HasRows() = True Then
    '        dtrBuyerReader.Read()
    '        strBuyerName = dtrBuyerReader.Item("FIRST_NAME_SRCH") & " " & dtrBuyerReader.Item("LAST_NAME_SRCH")
    '        dtrBuyerReader.Close()
    '    Else
    '        dtrBuyerReader.Close()
    '    End If

    '    'If currentApp.Session("IOLServer") = "" Then
    '    '    WebPSharedFunc.setServer()
    '    'End If

    '    'steph's request
    '    'pfd
    '    '1/02/2009
    '    ' get the detail line for the the approver quote email
    '    strSQLString = "SELECT ' ' AS ISA_IDENTIFIER," & vbCrLf & _
    '            " A.ORDER_NO, B.OPRID_ENTERED_BY," & vbCrLf & _
    '            " TO_CHAR(B.ISA_REQUIRED_BY_DT,'YYYY-MM-DD') as REQ_DT," & vbCrLf & _
    '            " TO_CHAR(A.ADD_DTTM,'YYYY-MM-DD') as ADD_DT," & vbCrLf & _
    '            " B.ISA_EMPLOYEE_ID, B.ISA_CUST_CHARGE_CD," & vbCrLf & _
    '            " B.ISA_WORK_ORDER_NO, B.ISA_MACHINE_NO," & vbCrLf & _
    '            " ' ' AS ISA_CUST_NOTES," & vbCrLf & _
    '            " B.INV_ITEM_ID, B.DESCR254, B.MFG_ID," & vbCrLf & _
    '            " B.ISA_MFG_FREEFORM, B.MFG_ITM_ID," & vbCrLf & _
    '            " B.UNIT_OF_MEASURE," & vbCrLf & _
    '            " ' ' AS VNDR_CATALOG_ID, B.ISA_SELL_PRICE," & vbCrLf & _
    '            " (B.ORDER_NO || B.ISA_INTFC_LN) as UNIQUEIDENT, B.ISA_INTFC_LN," & vbCrLf & _
    '            " B.ISA_LINE_STATUS," & vbCrLf & _
    '            " C.QTY_REQ, C.PRICE_REQ, F.ISA_SELL_PRICE," & vbCrLf & _
    '            " F.INV_ITEM_TYPE, F.INV_STOCK_TYPE," & vbCrLf & _
    '            " TO_CHAR(D.DUE_DT,'YYYY-MM-DD') as DUE_DT" & vbCrLf & _
    '            " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B," & vbCrLf & _
    '            " PS_REQ_LINE C, PS_REQ_LINE_SHIP D, SYSADM8.PS_ISA_REQ_BI_INFO F" & vbCrLf & _
    '            " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
    '            " AND A.ORDER_NO = '" & strreqID & "'" & vbCrLf & _
    '            " AND B.OPRID_ENTERED_BY = '" & strAgent & "'" & vbCrLf & _
    '            " AND B.ISA_LINE_STATUS = 'QTW'" & vbCrLf & _
    '            " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
    '            " AND B.QTY_REQUESTED > 0" & vbCrLf & _
    '            " AND A.ORDER_NO = C.REQ_ID" & vbCrLf & _
    '            " AND B.ISA_INTFC_LN = C.LINE_NBR" & vbCrLf & _
    '            " AND C.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
    '            " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
    '            " AND C.REQ_ID = D.REQ_ID" & vbCrLf & _
    '            " AND C.LINE_NBR = D.LINE_NBR" & vbCrLf & _
    '            " AND C.BUSINESS_UNIT = F.BUSINESS_UNIT (+)" & vbCrLf & _
    '            " AND C.REQ_ID = F.REQ_ID (+)" & vbCrLf & _
    '            " AND C.LINE_NBR = F.LINE_NBR (+)"

    '    Dim dtgcart As DataGrid
    '    Dim SBstk As New StringBuilder
    '    Dim SWstk As New StringWriter(SBstk)
    '    Dim htmlTWstk As New HtmlTextWriter(SWstk)
    '    Dim dataGridHTML As String
    '    'Dim itemsid As Integer

    '    dtgcart = New DataGrid

    '    Dim dsOrder As DataSet = ORDBData.GetAdapter(strSQLString)

    '    Dim dtOrder As DataTable = New DataTable()

    '    dtOrder = dsOrder.Tables(0)

    '    Dim dstcartSTK As New DataTable()

    '    dstcartSTK = buildCartforemail(dtOrder, strreqID)



    '    'Code for line items
    '    dtgcart.DataSource = dstcartSTK
    '    dtgcart.DataBind()
    '    dtgcart.CellPadding = 3
    '    dtgcart.BorderColor = Gray
    '    dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
    '    dtgcart.HeaderStyle.Font.Bold = True
    '    dtgcart.HeaderStyle.ForeColor = Black
    '    dtgcart.Width.Percentage(90)

    '    dtgcart.RenderControl(htmlTWstk)

    '    dataGridHTML = SBstk.ToString()

    '    Dim strwo As String = " "

    '    Dim strbodyhead As String
    '    Dim strbodydetl As String
    '    Dim txtBody As String
    '    Dim txtHdr As String
    '    Dim txtMsg As String

    '    Dim streBU As String = EncryptQueryString(strBU)
    '    Dim streOrdnum As String = EncryptQueryString(strreqID)
    '    Dim streApper As String = EncryptQueryString(strAppUserid)
    '    Dim streAppTyp As String = EncryptQueryString(strHldStatus)
    '    Dim strhref As String
    '    Dim stritemid As String

    '    strhref = ConfigurationSettings.AppSettings("SiteURL11") & "NeedApprove.aspx?fer=" & streOrdnum & "&op=" & streApper & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"
    '    Dim StrResult As String = String.Empty

    '    Dim Mailer As MailMessage = New MailMessage
    '    Mailer.From = "SDIExchange@SDI.com"
    '    Mailer.CC = ""
    '    Mailer.Bcc = "webdev@sdi.com"  '  ;Tony.Smith@sdi.com"

    '    'strbodyhead = "<table width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Arial; font-size: x-large; text-align: center;'>SDI Marketplace</span></center></td></tr></tbody></table>" & vbCrLf
    '    If strHldStatus = "B" Then
    '        ' strbodyhead = strbodyhead & "<center><span >SDiExchange - Request for Budget Approval</span></center>"

    '        strbodyhead = "<table bgcolor='black' width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style=padding: 10px 0;' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large;color:white; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center;color:white; margin: 0px auto;'>SDiExchange - Request for Budget Approval</span></center></td></tr></tbody></table>" & vbCrLf
    '        strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
    '        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
    '    Else
    '        'strbodyhead = strbodyhead & "<center><span >SDiExchange - Request for Approval</span></center>"

    '        strbodyhead = "<table bgcolor='black' width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large;color:white; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center;color:white; margin: 0px auto;'>SDiExchange - Request for Approval</span></center></td></tr></tbody></table>" & vbCrLf
    '        strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
    '        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
    '    End If
    '    strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

    '    strbodydetl = "&nbsp;" & vbCrLf
    '    strbodydetl = strbodydetl & "<div>"
    '    strbodydetl = strbodydetl & "<p ><span style='font-weight:bold;'>TO: </span><span>      </span>" & strappName & "<br>"
    '    strbodydetl = strbodydetl & "&nbsp;<BR>"
    '    strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Date: </span><span>    </span>" & Now() & "<BR>"
    '    strbodydetl = strbodydetl & "&nbsp;<br>"
    '    strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Order: </span><span>  </span>" & strreqID & "<br>"
    '    strbodydetl = strbodydetl & "&nbsp;<br>"
    '    strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Work Order: </span><span>  </span>" & strwo & "<br>"
    '    strbodydetl = strbodydetl & "&nbsp;<br>"
    '    strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Item Number:</span><span>  </span>" & stritemid & "<br>"
    '    strbodydetl = strbodydetl & "&nbsp;</p>"
    '    strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The above referenced order has been "
    '    strbodydetl = strbodydetl & "requested by <b>" & strBuyerName & "</b> "
    '    If strHldStatus = "B" Then
    '        strbodydetl = strbodydetl & "and has exceeded the charge code budget limit.  Click the link below or select the ""Approve Budget"" "
    '    Else
    '        strbodydetl = strbodydetl & "and needs your approval.  Click the link below or select the ""Approve Orders"" "
    '    End If
    '    strbodydetl = strbodydetl & "menu option in SDiExchange to approve or reject the order.<br>"
    '    strbodydetl = strbodydetl & "&nbsp;<br>"

    '    strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
    '    strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
    '    strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
    '    strbodydetl = strbodydetl & "</TABLE>" & vbCrLf
    '    strbodydetl = strbodydetl & "&nbsp;<br>"
    '    strbodydetl = strbodydetl & "Sincerely,<br>"
    '    strbodydetl = strbodydetl & "&nbsp;<br>"
    '    strbodydetl = strbodydetl & "SDI Customer Care<br>"
    '    strbodydetl = strbodydetl & "&nbsp;<br>"
    '    strbodydetl = strbodydetl & "Click this <a href='" & strhref & "' target='_blank'>link</a>&nbsp;"
    '    strbodydetl = strbodydetl & "to APPROVE or REJECT order. </p>"
    '    strbodydetl = strbodydetl & "</div>"
    '    strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
    '    strbodydetl = strbodydetl & "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & vbCrLf

    '    Mailer.Body = strbodyhead & strbodydetl
    '    If DbUrl.Substring(DbUrl.Length - 4).ToUpper = "STAR" Or _
    '        DbUrl.Substring(DbUrl.Length - 4).ToUpper = "RPTG" Or _
    '        DbUrl.Substring(DbUrl.Length - 4).ToUpper = "DEVL" Then

    '        Mailer.To = "webdev@sdi.com;SDIportalsupport@avasoft.biz"
    '    Else
    '        Mailer.To = strappEmail
    '    End If
    '    Dim Notify_Key As String = String.Empty
    '    If strHldStatus = "B" Then
    '        Mailer.Subject = "SDiExchange - Order Number " & strreqID & " needs budget approval"
    '        Notify_Key = "Order Number " & strreqID & " needs budget approval"
    '    Else
    '        Mailer.Subject = "SDiExchange - Order Number " & strreqID & " needs approval"
    '        Notify_Key = "Order Number " & strreqID & " needs approval"
    '    End If

    '    '   Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
    '    'WebPSharedFunc.sendemail(Mailer)
    '    Dim SDIEmailService As EmailUtilityServices.EmailServices = New EmailUtilityServices.EmailServices()
    '    Dim MailAttachmentName As String()
    '    Dim MailAttachmentbytes As New List(Of Byte())()

    '    Try
    '        SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, Mailer.CC, Mailer.Bcc, Mailer.Body, "Request to Approver", MailAttachmentName, MailAttachmentbytes.ToArray())
    '    Catch ex As Exception
    '        Dim strErr As String = ex.Message
    '    End Try
    '    ' Code to Add Notifications queue table 
    '    Try
    '        Dim Notify_Type As String = "APPRV"
    '        Dim Notify_User As String = strappName
    '        '    StrResult = NotifyClass.AddToNotifyQueueTable(Notify_Type, Notify_User, Notify_Key, strhref, String.Empty)
    '    Catch ex As Exception
    '    End Try

    '    dtrAppReader.Close()
    'End Sub

    Public Function EncryptQueryString(ByVal queryString As NameValueCollection) As String
        'create a string for each value in the query string passed in
        Dim tempQueryString As String = ""

        Dim index As Integer

        For index = 0 To queryString.Count - 1
            tempQueryString += queryString.GetKey(index) + "=" + queryString(index)
            If index = queryString.Count - 1 Then

            Else
                tempQueryString += QUERY_STRING_DELIMITER
            End If
        Next

        Return EncryptQueryString(tempQueryString)

    End Function

    Public Function EncryptQueryString(ByVal queryString As String) As String
        Return "?" + HttpUtility.UrlEncode(Encrypt(queryString))

    End Function

    Public Function Encrypt(ByVal unencryptedString As String) As String
        Dim bytIn As Byte() = ASCIIEncoding.ASCII.GetBytes(unencryptedString)

        'Create a Memory Stream
        Dim ms As MemoryStream = New MemoryStream()

        'Create Crypto Stream that encrypts a stream
        Dim cs As CryptoStream = New CryptoStream(ms, _cryptoProvider.CreateEncryptor(Key, IV), CryptoStreamMode.Write)

        'Write content into Memory Stream
        cs.Write(bytIn, 0, bytIn.Length)
        cs.FlushFinalBlock()

        Dim bytOut As Byte() = ms.ToArray()

        Return Convert.ToBase64String(bytOut)

    End Function

    Public Function GetWebAppName1() As String
        Dim sReturn As String = ""
        Dim sWebAppName As String = ""
        Try
            sWebAppName = Convert.ToString(ConfigurationSettings.AppSettings("WebAppName"))
            If sWebAppName Is Nothing Then
                sWebAppName = ""
            End If
        Catch ex As Exception
            sWebAppName = ""
        End Try
        If sWebAppName = "" Then
            sReturn = ""
        Else
            sReturn = "/" & sWebAppName
        End If
        Return sReturn
    End Function

    Private Function buildCartforemail(ByVal dstcart1 As DataTable, ByVal ordNumber As String) As DataTable

        Dim dr As DataRow
        Dim I As Integer
        Dim strPrice As String
        Dim strQty As String
        Dim dstcart As DataTable
        dstcart = New DataTable

        dstcart.Columns.Add("Item Number")
        dstcart.Columns.Add("Description")
        dstcart.Columns.Add("Manuf.")
        dstcart.Columns.Add("Manuf. Partnum")
        dstcart.Columns.Add("QTY")
        dstcart.Columns.Add("UOM")
        dstcart.Columns.Add("Price")
        dstcart.Columns.Add("Ext. Price")
        dstcart.Columns.Add("Item ID")
        dstcart.Columns.Add("LN")

        Dim strOraSelectQuery As String = String.Empty
        Dim ordIdentifier As String = String.Empty
        Dim ordBU As String = String.Empty
        Dim OrcRdr As OleDb.OleDbDataReader = Nothing
        Dim dsOrdLnItems As DataSet = New DataSet
        Dim strSqlSelectQuery As String = String.Empty
        Dim unilogRdr As OleDb.OleDbDataReader = Nothing
        Dim strProdVwId As String = String.Empty

        Try
            strOraSelectQuery = "select * from SYSADM8.PS_ISA_ORD_INTF_HD where ORDER_NO = '" & ordNumber & "'"
            'strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_H where ORDER_NO = 'M220016571'"
            OrcRdr = GetReader(strOraSelectQuery)
            If OrcRdr.HasRows Then
                OrcRdr.Read()
                ordIdentifier = CType(OrcRdr("ORDER_NO"), String).Trim()
                ordBU = CType(OrcRdr("BUSINESS_UNIT_OM"), String).Trim()
            End If
            OrcRdr.Close()

            strOraSelectQuery = "select * from SYSADM8.PS_ISA_ORD_INTF_LN where ORDER_NO = '" & ordIdentifier & "'"
            dsOrdLnItems = GetAdapter(strOraSelectQuery)

            If dsOrdLnItems.Tables(0).Rows.Count > 0 Then
                For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows
                    dr = dstcart.NewRow()
                    dr("Item Number") = ""
                    'CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim()
                    dr("Description") = CType(dataRowMain("DESCR254"), String).Trim()

                    'If CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim().Contains("NONCAT") Then

                    Try
                        dr("Manuf.") = CType(dataRowMain("ISA_MFG_FREEFORM"), String).Trim()
                    Catch ex As Exception
                        dr("Manuf.") = " "
                    End Try
                    Try
                        dr("Manuf. Partnum") = CType(dataRowMain("MFG_ITM_ID"), String).Trim()
                    Catch ex As Exception
                        dr("Manuf. Partnum") = " "
                    End Try
                    Try
                        dr("Item ID") = ""
                        'CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim()
                    Catch ex As Exception
                        dr("Item ID") = " "
                    End Try

                    Try
                        dr("UOM") = CType(dataRowMain("UNIT_OF_MEASURE"), String).Trim()
                    Catch ex As Exception
                        dr("UOM") = "EA"
                    End Try
                    Try
                        dr("QTY") = CType(dataRowMain("QTY_REQUESTED"), String).Trim()
                        If IsDBNull(CType(dataRowMain("QTY_REQUESTED"), String).Trim()) Or CType(dataRowMain("QTY_REQUESTED"), String).Trim() = " " Then
                            strQty = "0"
                        Else
                            strQty = CType(dataRowMain("QTY_REQUESTED"), String).Trim()
                        End If
                    Catch ex As Exception
                        strQty = "0"
                    End Try
                    strPrice = "0.00"
                    Try
                        strPrice = CDec(CType(dataRowMain("ISA_SELL_PRICE"), String).Trim()).ToString
                        If strPrice Is Nothing Then
                            strPrice = "0.00"
                        End If
                    Catch ex As Exception
                        strPrice = "0.00"
                    End Try
                    If CDec(strPrice) = 0 Then
                        dr("Price") = "Call for Price"
                    Else
                        dr("Price") = CDec(strPrice).ToString("f")
                    End If
                    dr("Ext. Price") = CType(Convert.ToDecimal(strQty) * Convert.ToDecimal(strPrice), String)

                    'dr("Item Chg Code") = CType(dataRowMain("ISA_CUST_CHARGE_CD"), String).Trim()
                    dr("LN") = CType(dataRowMain("ISA_INTFC_LN"), String).Trim()



                    dstcart.Rows.Add(dr)
                Next
            End If
        Catch ex17 As Exception
            Try
                OrcRdr.Close()
            Catch ex As Exception

            End Try
        End Try
        Return dstcart

    End Function

    Public Sub CheckSTKLimits(ByVal strBU As String, ByVal stragent As String, ByVal strreqID As String, ByVal strAppLevel As String, ByVal strAppType As String)

        Dim I As Integer
        Dim X As Integer
        Dim myConnection As OleDbConnection
        Dim myCommand As OleDbCommand
        Dim myParameter As OleDbParameter
        'Dim arrParamsOut As ArrayList
        Dim arrParamsAll As ArrayList
        Dim arrAppChgCds As ArrayList
        arrParamsAll = New ArrayList
        arrAppChgCds = New ArrayList
        ' Create connection and set connection string	
        myConnection = New OleDbConnection(DbUrl)

        arrAppChgCds = getChgCodes(strBU, strreqID)

        X = arrAppChgCds.Count - 1
        Try
            ' Open the connection to the database
            myConnection.Open()

            For I = 0 To X
                ' Create a new command object
                myCommand = New OleDbCommand

                ' Set the properties of the command so that it uses
                ' our connection, knows the name of the stored proc
                ' to execute, and knows that it is going to be
                ' executing a stored proc and not something else
                myCommand.Connection = myConnection
                myCommand.CommandText = "SYSADM8.SP_CUSTBUD_APPROVAL_STK"
                myCommand.CommandType = CommandType.StoredProcedure

                ' Create our Business_unit input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "BU"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strBU

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Employee ID input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "EMPID"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = stragent

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Order Number input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "ORDNO"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strreqID

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Approver level type input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "APPTY"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strAppLevel

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our charge code input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "CHGCD"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = arrAppChgCds(I)

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                '' Open the connection to the database
                'myConnection.Open()

                ' Execute the stored procedure
                myCommand.ExecuteNonQuery()

            Next

            myConnection.Close()
        Catch
            Try
                myConnection.Close()
            Catch ex As Exception

            End Try
        End Try

    End Sub

    Public Function getChgCodes(ByVal strbu As String, ByVal strOrderNo As String) As ArrayList

        Dim I As Integer
        Dim dr As OleDbDataReader
        Dim arrChrCode As ArrayList
        arrChrCode = New ArrayList

        Dim strSQLString As String = "SELECT distinct(B.ISA_CUST_CHARGE_CD) as Charge_code" & vbCrLf & _
            " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B" & vbCrLf & _
            " WHERE A.BUSINESS_UNIT_OM = '" & strbu & "'" & vbCrLf & _
            " AND A.ORDER_NO = '" & strOrderNo & "'" & vbCrLf & _
            " AND A.ORDER_NO = B.ORDER_NO"

        dr = ORDBData.GetReader(strSQLString)

        While dr.Read()
            arrChrCode.Add(dr.Item("Charge_code"))
        End While

        dr.Close()
        Return arrChrCode

    End Function

    'Public Sub UpdateOrderStatus(ByVal strbu As String, ByVal strOrderNo As String)
    '    Try
    '        Dim StrUpdSts As String = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS='QTC'  WHERE BUSINESS_UNIT_OM='" & strbu & "' AND ORDER_NO='" & strOrderNo & "'"
    '        Dim rowAffected As Integer = ORDBData.ExecNonQuery(StrUpdSts)
    '    Catch ex As Exception
    '    End Try
    'End Sub

    Public Class OrderApprovals

        Private Shared m_bGotSchema As Boolean = False
        Private Shared m_dtSchema As DataTable

        Public Shared Function SetInitialOrderStatus(strBU As String, sApproverID As String, strReqID As String, _
            ByRef oApprovalResults As ApprovalResults) As Boolean

            'Gives us a reference to the current asp.net 
            'application executing the method.

            Dim bSuccess As Boolean = False
            Dim trnsactSession As OleDbTransaction = Nothing
            Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
            Dim bIsError As Boolean = False

            Try

                connection.Open()
                trnsactSession = connection.BeginTransaction

                Dim oApprovalDetails As New ApprovalDetails(strBU, sApproverID, sApproverID, strReqID)

                If LoadLineItemsIfNeeded(trnsactSession, connection, oApprovalDetails) Then

                    oApprovalResults.NeedsQuote = False
                    Dim i As Integer = 0
                    'check new flag: ISA_BYP_RQSTR_APPR -  value
                    'If Session("BypassReqstrAppr") = "A" Then
                    '    oApprovalResults.NeedsQuote = False
                    'Else
                    ' OROs with 0 price and NSTKs need to be quoted so send them to PS regardless of order limit
                    While i < oApprovalDetails.LineDetails.Count And Not oApprovalResults.NeedsQuote
                        Dim oLineDetails As ApprovalDetails.OrderLineDetails

                        oLineDetails = CType(oApprovalDetails.LineDetails(i), ApprovalDetails.OrderLineDetails)


                        If oLineDetails.StockType = "NSTK" Then
                            oApprovalResults.NeedsQuote = True
                        ElseIf oLineDetails.StockType = "ORO" Then
                            If oLineDetails.UnitPrice = 0 Then
                                ' If the price is zero, send the order to PeopleSoft for a quote
                                ' regardless of the Enterprise flag that treats the order like
                                ' STK or NSTK.
                                oApprovalResults.NeedsQuote = True
                            Else

                                If Not oApprovalDetails.OROTreatedLikeSTK Then
                                    ' If OROs are supposed to be treated like NSTK, send them to PeopleSoft for
                                    ' a quote. If OROs are supposed to be treated like STK, don't force this item
                                    ' to PeopleSoft.
                                    oApprovalResults.NeedsQuote = True
                                End If
                            End If
                        End If

                        i = i + 1
                    End While

                    '  End If

                    '' If we don't need a quote, check if the limits are exceeded.

                    If Not oApprovalResults.NeedsQuote Then
                        If Not CheckLimits(trnsactSession, connection, oApprovalDetails, oApprovalResults) Then
                            bIsError = True
                        End If
                    End If

                    If Not bIsError Then
                        If oApprovalResults.OrderExceededLimit Then
                            NotifyApprover(oApprovalDetails.ReqID, oApprovalDetails.ApproverID, _
                                oApprovalDetails.BU, oApprovalResults.NextOrderApprover, oApprovalResults.NewOrderHeaderStatus, "")
                        Else
                            UpdateOrderStatus(oApprovalDetails.BU, oApprovalDetails.ReqID, oApprovalDetails)
                            bSuccess = True
                        End If
                    End If
                End If

                'If bIsError Then
                '    bSuccess = False
                'Else
                '    bSuccess = True
                'End If

                If bSuccess Then
                    trnsactSession.Commit()
                    connection.Close()
                    trnsactSession = Nothing
                    connection = Nothing
                Else
                    trnsactSession.Rollback()
                    connection.Close()
                    trnsactSession = Nothing
                    connection = Nothing
                End If

            Catch ex As Exception
                bSuccess = False
                Try
                    trnsactSession.Rollback()
                    connection.Close()
                    trnsactSession = Nothing
                    connection = Nothing
                Catch ex54 As Exception

                End Try
            End Try

            Return bSuccess
        End Function

        Public Shared Sub UpdateOrderStatus(ByVal BU As String, ByVal OrderNo As String, ByVal oApprovalDetails As ApprovalDetails)
            Try
                Dim bSuccess As Boolean = False
                Dim StrLineSts As String = GetFullyApprovedStatus(BU)
                Dim StrUpdQry As String = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS='" & StrLineSts & "' WHERE BUSINESS_UNIT_OM ='" & BU & "' AND ORDER_NO='" & OrderNo & "'"
                Dim trnsactSession As OleDbTransaction
                Dim connection As OleDbConnection
                Dim rowsaffected As Integer
                Dim exError As Exception

                If ExecuteNonQuery(trnsactSession, connection, StrUpdQry, rowsaffected, exError) Then
                    bSuccess = True
                    AddRecord(trnsactSession, connection, m_cClassFileName, "Utility-OrderStatus", "PS_ISA_ORD_INTF_LN", oApprovalDetails.ApproverID, oApprovalDetails.BU, "ISA_LINE_STATUS", 0, "", "")
                Else
                    bSuccess = False
                End If
            Catch ex As Exception

            End Try
        End Sub

        Public Shared Function AddRecord(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
                                 sSourceProgram As String, sFunctionDesc As String, sTableName As String, _
                                 sOprID As String, sBU As String, sKey01 As String, Optional sColumnChg As String = " ", _
                                 Optional sOldValue As String = " ", Optional sNewValue As String = " ", _
                                 Optional sKey02 As String = " ", Optional sKey03 As String = " ", _
                                 Optional sUDF1 As String = " ", Optional sUDF2 As String = " ", _
                                 Optional sUDF3 As String = " ", Optional ByRef sErrorDetails As String = "") As Boolean
            ' sSourceProgram: e.g., InterUnitReceipts.aspx
            ' sFunctionDesc: e.g., Location update; e.g., Receive interunit inventory - insert record
            ' sTableName: e.g., ps_inv_recv_hdr
            ' sOprID: e.g., Session("USERID")
            ' sBU: e.g., Session("BUSUNIT")
            ' keys: help identify the function such as itemID, receiverID, or other identifiers

            Dim bAddSuccess As Boolean = False
            Dim strSQLstring As String = ""
            Dim rowsaffected As Integer = 0
            Dim bError As Boolean = False

            Try
                If GetInsertCommand(sSourceProgram, sFunctionDesc, sTableName, sOprID, sBU, sColumnChg, sOldValue, sNewValue, _
                                    sKey01, sKey02, sKey03, sUDF1, sUDF2, sUDF3, strSQLstring, sErrorDetails) Then
                    Try
                        Dim cmd As OleDbCommand = New OleDbCommand(strSQLstring, connection)
                        cmd.CommandTimeout = 120
                        cmd.Transaction = trnsactSession
                        rowsaffected = cmd.ExecuteNonQuery()
                    Catch ex As Exception
                        bError = True
                        sErrorDetails = GetExceptionDetails(ex, strSQLstring)
                    End Try

                    If Not bError Then
                        If rowsaffected = 1 Then
                            bAddSuccess = True
                        Else
                            sErrorDetails = GetErrorDetails("InterUnitReceipts.aspx: AddRecord(OleDbTransaction...)", rowsaffected, strSQLstring)
                        End If
                    End If
                End If
            Catch ex As Exception
                sErrorDetails = GetExceptionDetails(ex, strSQLstring)
            End Try

            Return bAddSuccess
        End Function

        Private Shared Function GetInsertCommand(sSourceProgram As String, sFunctionDesc As String, sTableName As String, _
                                           sOprID As String, sBU As String, sColumnChg As String, sOldValue As String, _
                                           sNewValue As String, sKey01 As String, sKey02 As String, sKey03 As String, _
                                           sUDF1 As String, sUDF2 As String, sUDF3 As String, ByRef strSQLstring As String, _
                                           ByRef sErrorDetails As String) As Boolean
            Dim bGotCommand As Boolean = False
            Dim sServer As String = ""
            m_bGotSchema = False

            Try
                ' We are going to truncate instead of return an error. We don't
                ' want to abort the primary function (interunit receipts, etc) 
                ' just for an audit record.

                Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
                sServer = currentApp.Session("WEBSITEID")

                If TruncateData(sSourceProgram, sFunctionDesc, sTableName, sOprID, sBU, sColumnChg, sOldValue, _
                                                 sNewValue, sKey01, sKey02, sKey03, sUDF1, sUDF2, sUDF3, sServer, sErrorDetails) Then

                    'Yury Ticket 117944 20170609
                    'strSQLstring = "INSERT INTO SYSADM8.ps_isa_SDIXaudit " & vbCrLf & _
                    strSQLstring = "INSERT INTO SDIX_audit " & vbCrLf & _
                    " ( " & vbCrLf & _
                    " descr, rcdsrc, table_name " & vbCrLf & _
                    ", key_01, key_02, key_03 " & vbCrLf & _
                    ", columnchg, newvalue, oldvalue " & vbCrLf & _
                    ", oprid, server_name " & vbCrLf & _
                    ", dt_timestamp " & vbCrLf & _
                    ", business_unit, isa_udf1, isa_udf2, isa_udf3 " & vbCrLf & _
                    " ) " & vbCrLf & _
                    " VALUES (" & vbCrLf & _
                    " '" & sFunctionDesc & "', '" & sSourceProgram & "', '" & sTableName & "' " & vbCrLf & _
                    ", '" & sKey01 & "', '" & sKey02 & "', '" & sKey03 & "' " & vbCrLf & _
                    ", '" & sColumnChg & "', '" & sNewValue & "', '" & sOldValue & "' " & vbCrLf & _
                    ", '" & sOprID & "', '" & sServer & "' " & vbCrLf & _
                    ", TO_DATE('" & Now.ToString("MM/dd/yyyy HH:mm:ss") & "', 'MM/DD/YYYY HH24:MI:SS') " & vbCrLf & _
                    ", '" & sBU & "', '" & sUDF1 & "', '" & sUDF2 & "', '" & sUDF3 & "' " & vbCrLf & _
                    " )"

                    strSQLstring = strSQLstring.Replace(", ''", ", ' '") ' make sure nulls aren't written to table

                    bGotCommand = True
                End If
            Catch ex As Exception
                bGotCommand = False
                sErrorDetails = GetExceptionDetails(ex, strSQLstring)
            End Try

            Return bGotCommand
        End Function

        Private Shared Function TruncateData(ByRef sSourceProgram As String, ByRef sFunctionDesc As String, ByRef sTableName As String, _
                                            ByRef sOprID As String, ByRef sBU As String, ByRef sColumnChg As String, _
                                            ByRef sOldValue As String, ByRef sNewValue As String, ByRef sKey01 As String, _
                                            ByRef sKey02 As String, ByRef sKey03 As String, ByRef sUDF1 As String, _
                                            ByRef sUDF2 As String, ByRef sUDF3 As String, ByRef sServerName As String,
                                            ByRef sErrorDetails As String) As Boolean
            Dim bSuccess As Boolean = False

            Try
                If GetSchema(sErrorDetails) Then
                    If m_dtSchema IsNot Nothing Then
                        If m_dtSchema.Rows.Count > 0 Then
                            For Each dr As DataRow In m_dtSchema.Rows
                                Dim sColumnName As String = dr.Item("COLUMN_NAME").ToString.ToUpper
                                Dim sMaxLength As String = dr.Item("CHARACTER_MAXIMUM_LENGTH").ToString
                                If sMaxLength <> "" Then
                                    Dim iMaxLength As Integer = CType(dr.Item("CHARACTER_MAXIMUM_LENGTH").ToString, Integer)

                                    Select Case sColumnName
                                        Case "DESCR"
                                            TruncateField(sFunctionDesc, iMaxLength)
                                        Case "RCDSRC"
                                            TruncateField(sSourceProgram, iMaxLength)
                                        Case "TABLE_NAME"
                                            TruncateField(sTableName, iMaxLength)
                                        Case "KEY_01"
                                            TruncateField(sKey01, iMaxLength)
                                        Case "KEY_02"
                                            TruncateField(sKey02, iMaxLength)
                                        Case "KEY_03"
                                            TruncateField(sKey03, iMaxLength)
                                        Case "COLUMNCHG"
                                            TruncateField(sColumnChg, iMaxLength)
                                        Case "NEWVALUE"
                                            TruncateField(sNewValue, iMaxLength)
                                        Case "OLDVALUE"
                                            TruncateField(sOldValue, iMaxLength)
                                        Case "OPRID"
                                            TruncateField(sOprID, iMaxLength)
                                        Case "SERVER_NAME"
                                            TruncateField(sServerName, iMaxLength)
                                        Case "BUSINESS_UNIT"
                                            TruncateField(sBU, iMaxLength)
                                        Case "ISA_UDF1"
                                            TruncateField(sUDF1, iMaxLength)
                                        Case "ISA_UDF2"
                                            TruncateField(sUDF2, iMaxLength)
                                        Case "ISA_UDF3"
                                            TruncateField(sUDF3, iMaxLength)
                                    End Select
                                End If
                            Next
                        End If
                    End If

                    bSuccess = True
                End If

            Catch ex As Exception
                sErrorDetails = GetExceptionDetails(ex)
            End Try

            Return bSuccess
        End Function

        Private Shared Sub TruncateField(ByRef sData As String, iMaxLength As Integer)
            If sData.Length > iMaxLength Then
                sData = sData.Substring(0, iMaxLength)
            End If
        End Sub

        Private Shared Function GetSchema(ByRef sErrorDetails As String) As Boolean
            Dim bSuccess As Boolean = False

            Try
                If m_bGotSchema Then
                    bSuccess = True
                Else
                    Dim restrictions(3) As String
                    'restrictions(2) = UCase("ps_isa_SDIXaudit")   'Yury Ticket 117944 20170609
                    restrictions(2) = UCase("SDIX_audit")          'Yury Ticket 117944 20170609
                    Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
                    connection.Open()
                    m_dtSchema = connection.GetSchema("Columns", restrictions)
                    connection.Close()
                    m_bGotSchema = True
                    bSuccess = True
                End If
            Catch ex As Exception
                sErrorDetails = GetExceptionDetails(ex)
            End Try

            Return bSuccess
        End Function

        Private Shared Function GetExceptionDetails(ex As Exception, Optional strSQLstring As String = "") As String
            Dim sErrorDetails As String
            Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

            sErrorDetails = "ex.Message=" & ex.Message & vbCrLf & _
                "; USERID=" & currentApp.Session("USERID").ToString & vbCrLf & _
                "; BU=" & currentApp.Session("BUSUNIT").ToString
            If strSQLstring.Trim.Length > 0 Then
                sErrorDetails &= "; strSQLstring=" & strSQLstring
            End If
            sErrorDetails &= "; ex.StackTrace=" & ex.StackTrace

            Return sErrorDetails
        End Function

        Private Shared Function GetErrorDetails(sFunction As String, rowsaffected As Integer, strSQLstring As String) As String
            Dim sErrorDetails As String
            Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

            sErrorDetails = sFunction & vbCrLf & _
                "; rowsaffected=" & rowsaffected.ToString & vbCrLf & _
                "; USERID=" & currentApp.Session("USERID").ToString & vbCrLf & _
                "; BU=" & currentApp.Session("BUSUNIT").ToString & vbCrLf & _
                "; strSQLstring=" & strSQLstring

            Return sErrorDetails
        End Function

        Public Shared Function GetFullyApprovedStatus(ByVal strBu As String) As String

            Dim sFullyApprovedStatus As String = ""

            Dim strQuery As String = "select ISA_CUSTINT_APPRVL from SYSADM8.PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT='" & strBu & "' "
            Dim strCustApprl As String = ""
            Try
                strCustApprl = ORDBData.GetScalar(strQuery, False)
                If Trim(strCustApprl) <> "" Then
                    If UCase(Trim(strCustApprl)) = "Y" Then
                        sFullyApprovedStatus = "QTC"
                    Else
                        sFullyApprovedStatus = "QTA"
                    End If
                Else
                    sFullyApprovedStatus = "QTA"
                End If
            Catch ex As Exception
                sFullyApprovedStatus = "QTA"
            End Try

            Return sFullyApprovedStatus

        End Function

        Public Shared Function NotifyApprover(ByVal strReqID As String, ByVal strCurrentUserID As String, ByVal strBU As String, ByVal strNextApproverID As String, _
                                     ByVal strHldStatus As String, ByVal strChargeCode As String) As String
            Dim strAppMessage As String = ""

            Try
                buildNotifyApprover(strReqID, strCurrentUserID, strBU, strNextApproverID, strHldStatus)

                Dim strOrigApproverID As String = GetOriginalApprover(strBU, strCurrentUserID, strNextApproverID)
                Dim bProcessOrigApprover As Boolean = False
                If strOrigApproverID.Trim.ToUpper <> strNextApproverID.Trim.ToUpper Then
                    bProcessOrigApprover = True
                End If

                If bProcessOrigApprover Then
                    ' If the original approver has an alternate, the notification will go to the alternate
                    ' per the code above. Make sure to send the notification to the original approver
                    ' as well.
                    buildNotifyApprover(strReqID, strCurrentUserID, strBU, strOrigApproverID, strHldStatus)
                End If

                Dim strAppName As String
                Dim strAppEmail As String
                Dim strOrigAppName As String
                Dim strOrigAppEmail As String

                Dim objUserTbl As New clsUserTbl(strNextApproverID, strBU)
                strAppName = objUserTbl.FirstNameSrch & " " & objUserTbl.LastNameSrch
                strAppEmail = objUserTbl.EmployeeEmail

                If bProcessOrigApprover Then
                    objUserTbl = New clsUserTbl(strOrigApproverID, strBU)
                    strOrigAppName = objUserTbl.FirstNameSrch & " " & objUserTbl.LastNameSrch
                    strOrigAppEmail = objUserTbl.EmployeeEmail
                End If

                objUserTbl = Nothing

                If strHldStatus = "B" Then
                    strAppMessage = "Budget limit exceeded for \n" & strChargeCode & "\n"
                Else
                    strAppMessage = "Order limit exceeded. \n"
                End If

                strAppMessage &= "Order has been emailed to \n" & strAppName & "\nemail - " & strAppEmail
                If bProcessOrigApprover Then
                    strAppMessage &= "\nand\n" & strOrigAppName & "\nemail - " & strOrigAppEmail
                End If
                strAppMessage &= "\nfor approval."

            Catch ex As Exception
            End Try

            Return strAppMessage
        End Function

        Private Shared Function LoadLineItemsIfNeeded(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, ByRef oApprovalDetails As ApprovalDetails) As Boolean
            Dim bSuccess As Boolean = False

            Try
                ' If the order lines are not passed in, look them up.
                ' This would happen in the case of shopping cart, for example.
                ' If this happens, set the delete flag to false
                If oApprovalDetails.LineDetails.Count = 0 Then
                    Dim dsLineItems As New DataSet
                    If RetrieveLineItems(trnsactSession, connection, oApprovalDetails, dsLineItems) Then
                        If dsLineItems.Tables(0).Rows.Count > 0 Then
                            Const cDoNotDeleteItem As Boolean = False
                            For Each row As DataRow In dsLineItems.Tables(0).Rows
                                Dim iLineNbr As Integer = CType(row.Item("ISA_INTFC_LN").ToString, Integer)
                                Dim decQtyReq As Decimal = CType(row.Item("QTY_REQUESTED").ToString, Decimal)
                                Dim decUnitPrice As Decimal = CType(row.Item("ISA_SELL_PRICE").ToString, Decimal)
                                oApprovalDetails.AddLineDetailsForOrder(iLineNbr, _
                                                                        decQtyReq, _
                                                                        row.Item("INV_STOCK_TYPE").ToString, _
                                                                        row.Item("ISA_CUST_CHARGE_CD").ToString, _
                                                                        decUnitPrice, _
                                                                        row.Item("INV_ITEM_ID").ToString, _
                                                                        cDoNotDeleteItem)
                            Next
                        End If
                        bSuccess = True
                    Else
                        bSuccess = False
                    End If
                Else
                    ' If the lines are already loaded, it's a success.
                    bSuccess = True
                End If

            Catch ex As Exception
                bSuccess = False
                '   HandleApprovalError(trnsactSession, connection, ex, oApprovalDetails)
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveLineItems(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, oApprovalDetails As ApprovalDetails, ByRef dsLineItems As DataSet) As Boolean

            Const cCaller As String = "OrderApprovals.RetrieveLineItems"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String = ""

            Try

                strSQLstring = "SELECT L.ISA_INTFC_LN, L.QTY_REQUESTED, L.ISA_SELL_PRICE, L.INV_ITEM_ID, L.ISA_CUST_CHARGE_CD,C.inv_stock_type " & vbCrLf & _
                   " FROM SYSADM8.PS_ISA_ORD_INTF_LN  L, ps_inv_items C " & vbCrLf & _
                   " WHERE " & _
                   "       L.order_no = '" & oApprovalDetails.ReqID & "' " & vbCrLf & _
                   "       AND L.business_unit_om = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                   "       AND C.setid(+) = L.ITM_SETID " & vbCrLf & _
                   "       AND C.inv_item_id(+) = L.inv_item_id " & vbCrLf & _
                   "       AND L.inv_item_id != ' ' " & vbCrLf & _
                   "       AND C.EFFDT = " & vbCrLf & _
                   "		        (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf & _
                   "               WHERE C.SETID = A_ED.SETID" & vbCrLf & _
                   "               AND C.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
                   "               AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                   " UNION " & vbCrLf & _
                   " SELECT L.ISA_INTFC_LN, L.QTY_REQUESTED, L.ISA_SELL_PRICE, L.INV_ITEM_ID, L.ISA_CUST_CHARGE_CD , 'NSTK' AS inv_stock_type" & vbCrLf & _
                   " FROM  SYSADM8.PS_ISA_ORD_INTF_LN L WHERE " & vbCrLf & _
                   "       L.order_no = '" & oApprovalDetails.ReqID & "' " & vbCrLf & _
                   "       AND L.business_unit_om = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                   "       AND L.inv_item_id = ' ' "


                dsLineItems = ORDBData.GetAdapter(strSQLstring, False)
                'If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                bSuccess = True

            Catch ex As Exception
                bSuccess = False
                '     HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function UpdateOrderStatus_ApproveInitialOrder(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, _
          oApprovalDetails As ApprovalDetails, eApprType As ApprovalHistory.ApprHistType, sFuncDescr As String, _
          ByVal intParentID As Integer, Optional ByVal strFrom As String = "", Optional ByVal strForAscnd As String = "") As Boolean

            Dim bSuccess As Boolean = False

            If UpdateLineItem_ApproveInitialOrder(trnsactSession, connection, oApprovalDetails, eApprType, intParentID, sFuncDescr, strForAscnd) Then
                If UpdateHeader_ApproveInitialOrder(trnsactSession, connection, oApprovalDetails, eApprType, sFuncDescr, strFrom) Then
                    bSuccess = True
                End If
            End If

            Return bSuccess
        End Function

        Private Shared Function UpdateLineItem_ApproveInitialOrder(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, _
           ByVal oApprovalDetails As ApprovalDetails, eApprType As ApprovalHistory.ApprHistType, _
           intParentID As Integer, Optional sFuncDescr As String = "", Optional ByVal strFor As String = "") As Boolean

            Const cCaller As String = "OrderApprovals.UpdateLineItem_ApproveInitialOrder"

            Dim bSuccess As Boolean = False
            Dim rowsaffected As Integer
            Dim strSQLstring As String
            Dim sReqStatus As String

            Try

                sReqStatus = GetReqStatus(oApprovalDetails.BU, oApprovalDetails.ReqID)

                Dim sNewLineStatus As String = ""
                If sReqStatus = "" Then
                    sNewLineStatus = "NEW"
                Else
                    sNewLineStatus = "QTS" ' "Q"
                End If

                If Trim(strFor) <> "" Then
                    If UCase(Trim(strFor)) = "ASCNDPRC" Then
                        sNewLineStatus = "QTC"
                    End If
                End If

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN  " & vbCrLf & _
                    " SET " & vbCrLf & _
                    " ISA_LINE_STATUS = '" & sNewLineStatus & "' "

                If intParentID = 0 Then
                    strSQLstring &= " WHERE ORDER_NO = " & vbCrLf & _
                        " (SELECT A.ORDER_NO" & vbCrLf & _
                        "   FROM SYSADM8.PS_ISA_ORD_INTF_HD A" & vbCrLf & _
                        "   WHERE A.BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                        "   AND A.ORDER_NO = '" & oApprovalDetails.ReqID & "' "
                    strSQLstring &= ") "
                Else
                    strSQLstring &= " WHERE ORDER_NO = " & intParentID
                End If

                strSQLstring &= " AND ISA_LINE_STATUS IN ('1','2','Q','W','B','NEW','QTS','CRE','QTW') "

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        ' HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        If sFuncDescr.Length = 0 Then
                            If eApprType = ApprovalHistory.ApprHistType.QuoteApproval Then
                                sFuncDescr = "Approve quote"
                            Else
                                sFuncDescr = "Approve order"
                            End If
                        End If
                        Dim sErrorDetails As String = ""

                        Dim sOprID As String = oApprovalDetails.ApproverID.Trim
                        If sOprID.Length = 0 Then
                            'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
                            'sOprID = currentApp.Session("USERID")
                        End If

                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '    sFuncDescr, "SYSADM8.PS_ISA_ORD_INTF_LN", sOprID, oApprovalDetails.BU, _
                        '    oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sNewValue:=sNewLineStatus, _
                        '    sErrorDetails:=sErrorDetails) Then

                        '    bSuccess = True
                        'Else
                        '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        '    bSuccess = False
                        'End If
                    End If
                End If
            Catch ex As Exception
                bSuccess = False
                '  HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function GetReqStatus(sBU As String, sReqID As String) As String
            Dim strLineStatus As String = ""
            Dim strSQLstring As String = ""

            Try
                strSQLstring = "SELECT B.REQ_STATUS" & vbCrLf & _
                    " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, PS_REQ_HDR B" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                    " AND A.ORDER_NO = '" & sReqID & "'" & vbCrLf & _
                    " AND A.ORDER_NO = B.REQ_ID(+)" & vbCrLf

                strLineStatus = ORDBData.GetScalar(strSQLstring, False)

            Catch ex As Exception
                strLineStatus = ""
            End Try

            Return strLineStatus
        End Function

        Public Shared Function ExecuteSQL(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, strSQLstring As String, _
                                   oApprovalDetails As ApprovalDetails, ByRef rowsaffected As Integer, sCaller As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim exError As Exception

            If ExecuteNonQuery(trnsactSession, connection, strSQLstring, rowsaffected, exError) Then
                bSuccess = True
            Else
                bSuccess = False

                '       HandleApprovalError(trnsactSession, connection, strSQLstring, exError, oApprovalDetails, sCaller)
            End If

            Return bSuccess
        End Function

        Private Shared Function UpdateHeader_ApproveInitialOrder(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, _
         ByVal oApprovalDetails As ApprovalDetails, eApprType As ApprovalHistory.ApprHistType, _
         Optional sFuncDescr As String = "", Optional ByVal strFrom As String = "") As Boolean

            Const cCaller As String = "OrderApprovals.UpdateHeader_ApproveInitialOrder"
            Const cApproverID As String = " "

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String = ""
            Dim rowsaffected As Integer
            Dim strEmployeeId As String = oApprovalDetails.ApproverID
            If Trim(strEmployeeId) = "" Then
                strEmployeeId = " "
            End If
            If Trim(strFrom) = "" Then
                strEmployeeId = " "
            End If

            Try
                Dim objEnterpriseTbl As New clsEnterprise(oApprovalDetails.BU)
                Dim strLastUPDOprid As String = objEnterpriseTbl.LastUPDOprid
                Dim sNewStatus As String
                'If strLastUPDOprid = "XXXXXXXX" Then
                '    sNewStatus = "X"
                'Else
                '    sNewStatus = "P"
                'End If
                sNewStatus = "1"

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD " & vbCrLf & _
                    " SET " & vbCrLf & _
                    " LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                    ", ORDER_STATUS = '" & sNewStatus & "' " & vbCrLf & _
                    " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                    " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"


                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        '      HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        rowsaffected = 0
                        strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN  " & vbCrLf & _
                            " SET " & vbCrLf & _
                            " OPRID_APPROVED_BY = '" & strEmployeeId & "' " & vbCrLf & _
                            ", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                            " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                            " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"
                        If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                            If rowsaffected = 0 Then
                                bSuccess = False
                                '        HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                            Else
                                Dim sErrorDetails As String = ""
                                If sNewStatus = "|~|" Then
                                    bSuccess = True
                                Else
                                    If sFuncDescr.Length = 0 Then
                                        If eApprType = ApprovalHistory.ApprHistType.QuoteApproval Then
                                            sFuncDescr = "Approve quote"
                                        Else
                                            sFuncDescr = "Approve order"
                                        End If
                                    End If

                                    Dim sOprID As String = oApprovalDetails.ApproverID.Trim
                                    If sOprID.Length = 0 Then
                                        'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
                                        'sOprID = currentApp.Session("USERID")
                                    End If

                                    'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                                    '    sFuncDescr, "PS_ISA_ORD_INTF_HD", sOprID, oApprovalDetails.BU, _
                                    '    oApprovalDetails.ReqID, sColumnChg:="ORDER_STATUS", sNewValue:=sNewStatus, sErrorDetails:=sErrorDetails) Then

                                    '    bSuccess = True
                                    'Else
                                    '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                                    'End If
                                End If
                            End If
                        End If
                    End If
                End If




            Catch ex As Exception
                bSuccess = False
                '       HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function CheckLimits(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, _
            oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults) As Boolean

            Dim bSuccess As Boolean = False

            If CheckLimitsOrder(trnsactSession, connection, oApprovalDetails, oApprovalResults, IsAEES(oApprovalDetails.BU)) Then

                bSuccess = True
            Else
                bSuccess = False
            End If

            Return bSuccess
        End Function

        Private Shared Function CheckLimitsOrder(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, _
             ByRef oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults, Optional ByVal bIsMultiCurrency As Boolean = False) As Boolean

            Dim bSuccessful As Boolean = False

            Try
                oApprovalResults = New ApprovalResults()

                If bIsMultiCurrency Then
                    Const cPriorDaysAgo As Integer = 0 ' prior days (in number) ago
                    bSuccessful = MultiCurrencyOrder.CustEmp(trnsactSession, connection, oApprovalDetails, _
                        sdiMultiCurrency.getSiteCurrency(oApprovalDetails.BU).Id, _
                        cPriorDaysAgo, oApprovalResults)
                Else
                    bSuccessful = SingleCurrencyOrder.CustEmp(trnsactSession, connection, oApprovalDetails, oApprovalResults)
                End If

            Catch ex As Exception
                bSuccessful = False
                '                HandleApprovalError(trnsactSession, connection, ex, oApprovalDetails)
            End Try

            CheckLimitsOrder = bSuccessful

        End Function

        Private Shared Function RetrieveNetOrderPrice(sBU As String, sOrdNo As String, ByRef dblNetOrderPrice As Double, _
                               Optional sChgCd As String = "|~|") As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            Try
                strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) AS NET_ORDR_PRICE_VAR " & vbCrLf & _
                    " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                    " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf & _
                    " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                    " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' "
                If sChgCd <> "|~|" Then
                    strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
                End If
                Dim sNetOrderPrice As String
                sNetOrderPrice = ORDBData.GetScalar(strSQLstring)
                Try
                    dblNetOrderPrice = CType(sNetOrderPrice, Double)
                    bSuccess = True
                Catch ex As Exception
                    dblNetOrderPrice = 0
                End Try
            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Public Shared Function IsAEES(ByVal sBU As String) As Boolean
            Dim bIsAEES As Boolean = False

            Dim sSP1AccessString As String = "I0913~I0914~I0915~I0916~I0917~I0918~I0919~I0920~I0921~I0922~I0923~I0924~I0925~I0926~I0940"
            Try
                sSP1AccessString = UCase(ConfigurationSettings.AppSettings("AEESsitesList").ToString)
            Catch ex As Exception
                sSP1AccessString = "I0913~I0914~I0915~I0916~I0917~I0918~I0919~I0920~I0921~I0922~I0923~I0924~I0925~I0926~I0940"
            End Try
            Dim sAEESsites As String = sSP1AccessString  '  "I0913~I0914~I0915~I0916~I0917~I0918~I0919~I0920~I0921~I0922~I0923~I0924~I0925~I0926~I0940"
            Try
                bIsAEES = (sAEESsites.IndexOf(sBU.Trim.ToUpper) > -1)
            Catch ex As Exception
                bIsAEES = False
            End Try
            Return bIsAEES
        End Function

        Private Shared Function GetOriginalApprover(ByVal strBU As String, ByVal strLastApprover As String, ByVal strAlternateApproverID As String) As String
            Dim strOrigApproverID As String = ""

            Try
                Dim ds As DataSet
                Dim strSQLstring As String

                strSQLstring = "SELECT isa_iol_apr_emp_id" & vbCrLf & _
                    " FROM SDIX_USERS_APPRV" & vbCrLf & _
                    " WHERE isa_iol_apr_alt = '" & strAlternateApproverID & "'" & vbCrLf & _
                    " AND isa_employee_id = '" & strLastApprover & "'" & vbCrLf & _
                    " AND business_unit = '" & strBU & "'"
                ds = ORDBData.GetAdapter(strSQLstring)
                If ds.Tables(0).Rows.Count > 0 Then
                    strOrigApproverID = ds.Tables(0).Rows(0).Item("isa_iol_apr_emp_id").ToString
                End If

            Catch ex As Exception
            End Try

            Return strOrigApproverID
        End Function

        Public Shared Sub buildNotifyApprover(ByVal strreqID As String, ByVal strAgent As String, ByVal strBU As String, ByVal strAppUserid As String, ByVal strHldStatus As String)
            'this is where we will put in the description of the order per S.Roudriquez
            'pfd
            'Gives us a reference to the current asp.net 
            'application executing the method.
            'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

            Dim strSQLString As String
            Dim strappName As String
            Dim strappEmail As String
            Dim strBuyerName As String

            strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                    " LAST_NAME_SRCH," & vbCrLf & _
                    " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                    " FROM SDIX_USERS_TBL" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND ISA_EMPLOYEE_ID = '" & strAppUserid & "'"

            Dim dtrAppReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

            If dtrAppReader.HasRows() = True Then
                dtrAppReader.Read()
                strappName = dtrAppReader.Item("FIRST_NAME_SRCH") & " " & dtrAppReader.Item("LAST_NAME_SRCH")
                strappEmail = dtrAppReader.Item("ISA_EMPLOYEE_EMAIL")
                dtrAppReader.Close()
            Else
                dtrAppReader.Close()
                Exit Sub
            End If

            strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                    " LAST_NAME_SRCH," & vbCrLf & _
                    " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                    " FROM SDIX_USERS_TBL" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND ISA_EMPLOYEE_ID = '" & strAgent & "'"

            Dim dtrBuyerReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

            If dtrBuyerReader.HasRows() = True Then
                dtrBuyerReader.Read()
                strBuyerName = dtrBuyerReader.Item("FIRST_NAME_SRCH") & " " & dtrBuyerReader.Item("LAST_NAME_SRCH")
                dtrBuyerReader.Close()
            Else
                dtrBuyerReader.Close()
            End If

            'If currentApp.Session("IOLServer") = "" Then
            '    WebPSharedFunc.setServer()
            'End If

            'steph's request
            'pfd
            '1/02/2009
            ' get the detail line for the the approver quote email
            strSQLString = "SELECT ' ' AS ISA_IDENTIFIER," & vbCrLf & _
                    " A.ORDER_NO, B.OPRID_ENTERED_BY," & vbCrLf & _
                    " TO_CHAR(B.ISA_REQUIRED_BY_DT,'YYYY-MM-DD') as REQ_DT," & vbCrLf & _
                    " TO_CHAR(A.ADD_DTTM,'YYYY-MM-DD') as ADD_DT," & vbCrLf & _
                    " B.ISA_EMPLOYEE_ID, B.ISA_CUST_CHARGE_CD," & vbCrLf & _
                    " B.ISA_WORK_ORDER_NO, B.ISA_MACHINE_NO," & vbCrLf & _
                    " ' ' AS ISA_CUST_NOTES," & vbCrLf & _
                    " B.INV_ITEM_ID, B.DESCR254, B.MFG_ID," & vbCrLf & _
                    " B.ISA_MFG_FREEFORM, B.MFG_ITM_ID," & vbCrLf & _
                    " B.UNIT_OF_MEASURE," & vbCrLf & _
                    " ' ' AS VNDR_CATALOG_ID, B.ISA_SELL_PRICE," & vbCrLf & _
                    " (B.ORDER_NO || B.ISA_INTFC_LN) as UNIQUEIDENT, B.ISA_INTFC_LN," & vbCrLf & _
                    " B.ISA_LINE_STATUS," & vbCrLf & _
                    " C.QTY_REQ, C.PRICE_REQ, F.ISA_SELL_PRICE," & vbCrLf & _
                    " F.INV_ITEM_TYPE, F.INV_STOCK_TYPE," & vbCrLf & _
                    " TO_CHAR(D.DUE_DT,'YYYY-MM-DD') as DUE_DT" & vbCrLf & _
                    " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B," & vbCrLf & _
                    " PS_REQ_LINE C, PS_REQ_LINE_SHIP D, SYSADM8.PS_ISA_REQ_BI_INFO F" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                    " AND A.ORDER_NO = '" & strreqID & "'" & vbCrLf & _
                    " AND B.OPRID_ENTERED_BY = '" & strAgent & "'" & vbCrLf & _
                    " AND B.ISA_LINE_STATUS = 'QTW'" & vbCrLf & _
                    " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                    " AND B.QTY_REQUESTED > 0" & vbCrLf & _
                    " AND A.ORDER_NO = C.REQ_ID" & vbCrLf & _
                    " AND B.ISA_INTFC_LN = C.LINE_NBR" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
                    " AND C.REQ_ID = D.REQ_ID" & vbCrLf & _
                    " AND C.LINE_NBR = D.LINE_NBR" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = F.BUSINESS_UNIT (+)" & vbCrLf & _
                    " AND C.REQ_ID = F.REQ_ID (+)" & vbCrLf & _
                    " AND C.LINE_NBR = F.LINE_NBR (+)"

            Dim dtgcart As DataGrid
            Dim SBstk As New StringBuilder
            Dim SWstk As New StringWriter(SBstk)
            Dim htmlTWstk As New HtmlTextWriter(SWstk)
            Dim dataGridHTML As String
            'Dim itemsid As Integer

            dtgcart = New DataGrid

            Dim dsOrder As DataSet = ORDBData.GetAdapter(strSQLString)

            Dim dtOrder As DataTable = New DataTable()

            dtOrder = dsOrder.Tables(0)

            Dim dstcartSTK As New DataTable()

            dstcartSTK = buildCartforemail(dtOrder, strreqID)



            'Code for line items
            dtgcart.DataSource = dstcartSTK
            dtgcart.DataBind()
            dtgcart.CellPadding = 3
            dtgcart.BorderColor = Gray
            dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
            dtgcart.HeaderStyle.Font.Bold = True
            dtgcart.HeaderStyle.ForeColor = Black
            dtgcart.Width.Percentage(90)

            dtgcart.RenderControl(htmlTWstk)

            dataGridHTML = SBstk.ToString()

            Dim strwo As String = " "

            Dim strbodyhead As String
            Dim strbodydetl As String
            Dim txtBody As String
            Dim txtHdr As String
            Dim txtMsg As String

            Dim streBU As String = EncryptQueryString(strBU)
            Dim streOrdnum As String = EncryptQueryString(strreqID)
            Dim streApper As String = EncryptQueryString(strAppUserid)
            Dim streAppTyp As String = EncryptQueryString(strHldStatus)
            Dim strhref As String
            Dim stritemid As String

            strhref = ConfigurationSettings.AppSettings("SiteURL11") & "NeedApprove.aspx?fer=" & streOrdnum & "&op=" & streApper & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"
            Dim StrResult As String = String.Empty

            Dim Mailer As MailMessage = New MailMessage
            Mailer.From = "SDIExchange@SDI.com"
            Mailer.Cc = ""
            Mailer.Bcc = "webdev@sdi.com"  '  ;Tony.Smith@sdi.com"

            'strbodyhead = "<table width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Arial; font-size: x-large; text-align: center;'>SDI Marketplace</span></center></td></tr></tbody></table>" & vbCrLf
            If strHldStatus = "B" Then
                ' strbodyhead = strbodyhead & "<center><span >SDiExchange - Request for Budget Approval</span></center>"

                strbodyhead = "<table bgcolor='black' width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style=padding: 10px 0;' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large;color:white; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center;color:white; margin: 0px auto;'>SDiExchange - Request for Budget Approval</span></center></td></tr></tbody></table>" & vbCrLf
                strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
                strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
            Else
                'strbodyhead = strbodyhead & "<center><span >SDiExchange - Request for Approval</span></center>"

                strbodyhead = "<table bgcolor='black' width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large;color:white; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center;color:white; margin: 0px auto;'>SDiExchange - Request for Approval</span></center></td></tr></tbody></table>" & vbCrLf
                strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
                strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
            End If
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

            strbodydetl = "&nbsp;" & vbCrLf
            strbodydetl = strbodydetl & "<div>"
            strbodydetl = strbodydetl & "<p ><span style='font-weight:bold;'>TO: </span><span>      </span>" & strappName & "<br>"
            strbodydetl = strbodydetl & "&nbsp;<BR>"
            strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Date: </span><span>    </span>" & Now() & "<BR>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Order: </span><span>  </span>" & strreqID & "<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Work Order: </span><span>  </span>" & strwo & "<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Item Number:</span><span>  </span>" & stritemid & "<br>"
            strbodydetl = strbodydetl & "&nbsp;</p>"
            strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The above referenced order has been "
            strbodydetl = strbodydetl & "requested by <b>" & strBuyerName & "</b> "
            If strHldStatus = "B" Then
                strbodydetl = strbodydetl & "and has exceeded the charge code budget limit.  Click the link below or select the ""Approve Budget"" "
            Else
                strbodydetl = strbodydetl & "and needs your approval.  Click the link below or select the ""Approve Orders"" "
            End If
            strbodydetl = strbodydetl & "menu option in SDiExchange to approve or reject the order.<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"

            strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
            strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
            strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
            strbodydetl = strbodydetl & "</TABLE>" & vbCrLf
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "Sincerely,<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "SDI Customer Care<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "Click this <a href='" & strhref & "' target='_blank'>link</a>&nbsp;"
            strbodydetl = strbodydetl & "to APPROVE or REJECT order. </p>"
            strbodydetl = strbodydetl & "</div>"
            strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
            strbodydetl = strbodydetl & "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & vbCrLf

            Mailer.Body = strbodyhead & strbodydetl
            If DbUrl.Substring(DbUrl.Length - 4).ToUpper = "STAR" Or _
                DbUrl.Substring(DbUrl.Length - 4).ToUpper = "RPTG" Or _
                DbUrl.Substring(DbUrl.Length - 4).ToUpper = "DEVL" Then

                Mailer.To = "webdev@sdi.com;SDIportalsupport@avasoft.biz"
            Else
                Mailer.To = strappEmail
            End If
            Dim Notify_Key As String = String.Empty
            If strHldStatus = "B" Then
                Mailer.Subject = "SDiExchange - Order Number " & strreqID & " needs budget approval"
                Notify_Key = "Order Number " & strreqID & " needs budget approval"
            Else
                Mailer.Subject = "SDiExchange - Order Number " & strreqID & " needs approval"
                Notify_Key = "Order Number " & strreqID & " needs approval"
            End If

            '   Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
            'WebPSharedFunc.sendemail(Mailer)
            Dim SDIEmailService As EmailUtilityServices.EmailServices = New EmailUtilityServices.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            Try
                SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, Mailer.Cc, Mailer.Bcc, Mailer.Body, "Request to Approver", MailAttachmentName, MailAttachmentbytes.ToArray())
            Catch ex As Exception
                Dim strErr As String = ex.Message
            End Try
            ' Code to Add Notifications queue table 
            Try
                Dim Notify_Type As String = "APPRV"
                Dim Notify_User As String = strappName
                '    StrResult = NotifyClass.AddToNotifyQueueTable(Notify_Type, Notify_User, Notify_Key, strhref, String.Empty)
            Catch ex As Exception
            End Try

            dtrAppReader.Close()
        End Sub

    End Class


    Public Class sdiMultiCurrency
        Public Shared Function getSiteCurrency(ByVal siteBU As String) As sdiCurrency

            Dim curr As sdiCurrency = New sdiCurrency(id:=sdiCommon.DEFAULT_CURRENCY_CODE, desc:="")
            curr.IsKnownCurrency = False
            Dim strCurrencyCode As String
            Try
                Dim bu3digit As String = "00000" & siteBU.Trim.ToUpper
                bu3digit = siteBU.Substring(siteBU.Length - 3, 3)

                'Code to get currency code
                Dim strSQLString As String
                strSQLString = String.Format("SELECT A.CURRENCY_CD_BASE AS BASE_CURRENCY FROM PS_BUS_UNIT_TBL_OM A  " & vbCrLf & _
                                "WHERE A.BUSINESS_UNIT LIKE '%{0}' AND A.CURRENCY_CD_BASE <> ' ' GROUP BY A.CURRENCY_CD_BASE ", bu3digit)
                strCurrencyCode = Trim(ORDBData.GetScalar(strSQLString))

                'Code to get currency details
                Dim dsCurrencyInfo As New DataSet
                dsCurrencyInfo = GetCurrency(strCurrencyCode)

                'Code to generate sdicurrency object
                curr = GetSDICurrencyObject(dsCurrencyInfo)
            Catch ex As Exception

            End Try
            Return (curr)
        End Function
        Public Shared Function GetCurrency(ByVal currencyCode As String) As DataSet
            Dim dsCurrencyInfo As New DataSet
            Try
                Dim strSQLString As String = "SELECT " & vbCrLf & _
                                        " CD.CURRENCY_CD " & vbCrLf & _
                                        ",CD.DESCRSHORT " & vbCrLf & _
                                        ",CD.DESCR " & vbCrLf & _
                                        ",CD.CUR_SYMBOL " & vbCrLf & _
                                        ",CD.COUNTRY " & vbCrLf & _
                                        "FROM SYSADM8.PS_CURRENCY_CD_TBL CD " & vbCrLf & _
                                        "WHERE CD.EFF_STATUS = 'A' " & vbCrLf & _
                                        "  AND CD.CURRENCY_CD = '" & currencyCode & "' " & vbCrLf & _
                                        "  AND CD.EFFDT = ( " & vbCrLf & _
                                        "                  SELECT MAX(A1.EFFDT) " & vbCrLf & _
                                        "                  FROM SYSADM8.PS_CURRENCY_CD_TBL A1 " & vbCrLf & _
                                        "                  WHERE A1.CURRENCY_CD = CD.CURRENCY_CD " & vbCrLf & _
                                        "                    AND A1.EFF_STATUS = CD.EFF_STATUS " & vbCrLf & _
                                        "                    AND A1.EFFDT <= SYSDATE " & vbCrLf & _
                                        "                 ) " & vbCrLf & _
                                        "ORDER BY CD.CURRENCY_CD "

                dsCurrencyInfo = ORDBData.GetAdapterSpc(strSQLString)
            Catch ex As Exception

            End Try
            Return dsCurrencyInfo
        End Function

        Public Shared Function GetSDICurrencyObject(ByVal dsCurrencyInfo As DataSet) As sdiCurrency
            Dim curr As sdiCurrency = New sdiCurrency(id:=sdiCommon.DEFAULT_CURRENCY_CODE, desc:="")
            curr.IsKnownCurrency = False

            Try
                If Not dsCurrencyInfo Is Nothing And dsCurrencyInfo.Tables(0).Rows.Count = 1 Then
                    curr.Id = dsCurrencyInfo.Tables(0).Rows(0)("CURRENCY_CD")
                    curr.Description = dsCurrencyInfo.Tables(0).Rows(0)("DESCR")
                    curr.ShortDescription = dsCurrencyInfo.Tables(0).Rows(0)("DESCRSHORT")
                    curr.Symbol = dsCurrencyInfo.Tables(0).Rows(0)("CUR_SYMBOL")
                    curr.Country = dsCurrencyInfo.Tables(0).Rows(0)("COUNTRY")
                    curr.IsKnownCurrency = True
                End If
            Catch ex As Exception

            End Try
            Return (curr)
        End Function
    End Class

    Private Class SingleCurrencyOrder
        Private Shared m_cClassName As String = "OrderApprovals.SingleCurrencyOrder."

        Public Shared Function CustEmp(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, _
            oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults) As Boolean

            Dim bSuccess As Boolean = True ' Processing is successful by default
            Dim bCalcNSTKOnly As Boolean
            Dim strSQLstring As String

            oApprovalResults.UpdateEmployeeResults(False, "P", oApprovalDetails.ApproverID)

            If oApprovalDetails.NSTKOnlyFlag = "Y" Then
                bCalcNSTKOnly = True
            Else
                bCalcNSTKOnly = False
            End If

            Dim sOrdApprType As String = "O"
            'Dim oEnterprise As New clsEnterprise(oApprovalDetails.BU)
            'sOrdApprType = oEnterprise.OrdApprType
            If sOrdApprType.Trim.Length > 0 Then
                Dim dblNetOrderPrice As Double = 0
                RetrieveNetOrderPrice(oApprovalDetails.BU, oApprovalDetails.ReqID, dblNetOrderPrice)
                If dblNetOrderPrice > 0 Then
                    Dim dblNetUnitPrice As Double
                    If RetrieveNetUnitPrice(bCalcNSTKOnly, sOrdApprType, oApprovalDetails.BU, oApprovalDetails.EnteredByID, oApprovalDetails.ReqID, dblNetUnitPrice) Then
                        If dblNetUnitPrice > 0 Then
                            Dim dblApprLimit As Double
                            If RetrieveOrderLimAndNextApprv(oApprovalDetails.BU, oApprovalDetails.EnteredByID, oApprovalDetails.ApproverID, dblApprLimit, oApprovalResults.NextOrderApprover, oApprovalResults.ErrorInApproval) Then
                                If oApprovalResults.ErrorInApproval = ApprovalResults.ApprovalError.NoError Then
                                    If dblNetUnitPrice > dblApprLimit And oApprovalResults.NextOrderApprover.Trim.Length > 0 Then
                                        oApprovalResults.OrderExceededLimit = True

                                        'If UpdateLineItem_ApproveOrder(trnsactSession, connection, oApprovalDetails) Then
                                        If Not UpdateHeader_ApproveOrder(trnsactSession, connection, oApprovalDetails, oApprovalResults.NewOrderHeaderStatus) Then
                                            bSuccess = False
                                        End If
                                        'Else
                                        '    bSuccess = False
                                        'End If
                                    Else
                                        oApprovalResults.OrderExceededLimit = False
                                    End If
                                Else
                                    bSuccess = False
                                End If
                            Else
                                bSuccess = False
                            End If
                        End If
                    Else
                        bSuccess = False
                    End If
                End If
            End If

            Return bSuccess
        End Function

        Private Shared Function UpdateHeader_ApproveOrder(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
            ByVal oApprovalDetails As ApprovalDetails, ByRef sNewStatus As String) As Boolean

            Const cNewHeaderStatus As String = "QTW"
            Const cCaller As String = "SingleCurrencyOrder.UpdateHeader_ApproveOrder"
            Const cNewLineStatus As String = "QTW"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String
            Dim rowsaffected As Integer

            Try
                sNewStatus = cNewHeaderStatus

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD  " & vbCrLf & _
                   " SET " & vbCrLf & _
                   " ORDER_STATUS = '" & sNewStatus & "' " & vbCrLf & _
                   ", LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                   " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                   " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        '       HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        'Dim sErrorDetails As String = ""
                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '        "Approve budget", "SYSADM8.PS_ISA_ORD_INTF_HD", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                        '        oApprovalDetails.ReqID, sColumnChg:="ORDER_STATUS", sNewValue:=sNewStatus, sErrorDetails:=sErrorDetails) Then

                        '    bSuccess = True
                        'Else
                        '    '   HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        'End If
                    End If
                End If

                'strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN  " & vbCrLf & _
                '" SET " & vbCrLf & _
                '" OPRID_APPROVED_BY = '" & oApprovalDetails.ApproverID & "' " & vbCrLf & _
                '", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                '", ISA_LINE_STATUS = '" & cNewLineStatus & "' " & vbCrLf & _
                '" WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                '" AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"
                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN  " & vbCrLf & _
                " SET " & vbCrLf & _
                " OPRID_APPROVED_BY = '" & oApprovalDetails.ApproverID & "' " & vbCrLf & _
                ", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        'HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        'Dim sErrorDetails As String = ""
                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '        "Approve budget", "SYSADM8.PS_ISA_ORD_INTF_LN", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                        '        oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sNewValue:=cNewLineStatus, sErrorDetails:=sErrorDetails) Then

                        '    bSuccess = True
                        'Else
                        '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        'End If
                    End If
                End If
            Catch ex As Exception
                bSuccess = False
                '       HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function UpdateLineItem_ApproveOrder(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
            ByVal oApprovalDetails As ApprovalDetails) As Boolean

            Const cWaitingOrderApprovalStatus As String = "QTW"  '  "W"
            Const cCaller As String = "SingleCurrencyOrder.UpdateLineItem_ApproveOrder"

            Dim bSuccess As Boolean = False
            Dim rowsaffected As Integer
            Dim strSQLstring As String

            Try
                Dim bErrorUpdating As Boolean = False
                Dim i As Integer = 0
                While i < oApprovalDetails.LineDetails.Count And Not bErrorUpdating
                    Dim oLineDetails As ApprovalDetails.OrderLineDetails
                    oLineDetails = CType(oApprovalDetails.LineDetails(i), ApprovalDetails.OrderLineDetails)
                    If Not oLineDetails.DeleteItem Then
                        If Not (oApprovalDetails.NSTKOnlyFlag = "Y" And oLineDetails.StockType = "STK") Then
                            strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN" & vbCrLf & _
                                " SET " & vbCrLf & _
                                " ISA_LINE_STATUS = '" & cWaitingOrderApprovalStatus & "' " & vbCrLf & _
                                " WHERE ORDER_NO = " & vbCrLf & _
                                " (SELECT A.ORDER_NO" & vbCrLf & _
                                "   FROM SYSADM8.PS_ISA_ORD_INTF_HD  A" & vbCrLf & _
                                "   WHERE A.BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                                "   AND A.ORDER_NO = '" & oApprovalDetails.ReqID & "' " & vbCrLf & _
                                " ) " & vbCrLf & _
                                " AND ISA_LINE_STATUS IN ('1', '2', 'Q', 'W', 'B', 'V', 'U','QTS','QTW','NEW','CRE') " & vbCrLf & _
                                " AND ISA_INTFC_LN = " & oLineDetails.LineNbr
                            If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                                'Dim sErrorDetails As String = ""
                                'If Not clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                                '"Approve order", "SYSADM8.PS_ISA_ORD_INTF_LN", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                                'oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sNewValue:=cWaitingOrderApprovalStatus, _
                                'sErrorDetails:=sErrorDetails, sKey02:=oLineDetails.LineNbr) Then
                                '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                                '    bErrorUpdating = True
                                'End If
                            Else
                                bErrorUpdating = True
                            End If
                        End If
                    End If
                    i = i + 1
                End While

                If bErrorUpdating Then
                    bSuccess = False
                Else
                    bSuccess = True
                End If
            Catch ex As Exception
                bSuccess = False
                '   HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Public Shared Function CustBud(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
            oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults, iChgCodeIndex As Integer) As Boolean

            Dim dblNetUnitPrice As Double
            Dim dblNetOrderPrice As Double = 0
            Dim dblCustBudgetLim As Double
            Dim bCalcNSTKOnly As Boolean = False
            Dim bSuccessful As Boolean = True ' processing is successful by default unless we encounter an issue below
            Dim strSQLstring As String = False

            oApprovalResults.NextBudgetApprover(iChgCodeIndex) = oApprovalDetails.ApproverID

            If oApprovalDetails.NSTKOnlyFlag = "Y" Then
                bCalcNSTKOnly = True
            Else
                bCalcNSTKOnly = False
            End If

            oApprovalResults.BudgetExceededLimit(iChgCodeIndex) = False

            Dim sOrdApprType As String = "O"
            'Dim oEnterprise As New clsEnterprise(oApprovalDetails.BU)
            'If oEnterprise.OrdApprType.Trim.Length > 0 Then

            RetrieveNetOrderPrice(oApprovalDetails.BU, oApprovalDetails.ReqID, dblNetOrderPrice, oApprovalResults.BudgetChargeCode(iChgCodeIndex))
            If dblNetOrderPrice > 0 Then

                RetrieveNetUnitPrice(bCalcNSTKOnly, sOrdApprType, oApprovalDetails.BU, oApprovalDetails.ApproverID, oApprovalDetails.ReqID, dblNetUnitPrice, oApprovalResults.BudgetChargeCode(iChgCodeIndex))
                If dblNetUnitPrice > 0 Then
                    If IsBudgetLimAndNextApprv(oApprovalDetails.BU, oApprovalResults.BudgetChargeCode(iChgCodeIndex), oApprovalDetails.ApproverID, dblNetUnitPrice, dblCustBudgetLim, oApprovalResults.NextBudgetApprover(iChgCodeIndex)) Then
                        If dblNetUnitPrice > dblCustBudgetLim Then
                            oApprovalResults.BudgetExceededLimit(iChgCodeIndex) = True
                            If UpdateLineItem_ApproveBudget(trnsactSession, connection, oApprovalDetails) Then
                                If UpdateHeader_ApproveBudget(trnsactSession, connection, oApprovalDetails, oApprovalResults.NewBudgetHeaderStatus(iChgCodeIndex)) Then
                                    Dim sBusinessUnit_OM As String
                                    sBusinessUnit_OM = GetBudgetApprBU_OM(oApprovalDetails.BU, oApprovalDetails.ReqID, oApprovalResults.BudgetChargeCode(iChgCodeIndex))
                                    If sBusinessUnit_OM.Trim.Length = 0 Then
                                        If Not InsertBudgetAppr(trnsactSession, connection, oApprovalDetails, oApprovalResults.BudgetChargeCode(iChgCodeIndex)) Then
                                            bSuccessful = False
                                        End If
                                    End If
                                Else
                                    bSuccessful = False
                                End If
                            Else
                                bSuccessful = False
                            End If
                        End If
                    End If
                End If
            End If

            'End If  'If oEnterprise.OrdApprType.Trim.Length > 0 Then

            Return bSuccessful
        End Function

        Private Shared Function UpdateHeader_ApproveBudget(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
            ByVal oApprovalDetails As ApprovalDetails, ByRef sNewStatus As String) As Boolean

            Const cNewHeaderStatus As String = "B"
            Const cCaller As String = "SingleCurrencyOrder.UpdateHeader_ApproveBudget"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String
            Dim rowsaffected As Integer

            Try
                sNewStatus = cNewHeaderStatus

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD  " & vbCrLf & _
                    " SET " & vbCrLf & _
                    " ORDER_STATUS = '" & sNewStatus & "' " & vbCrLf & _
                    ", LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                    " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                    " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        '      HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        'Dim sErrorDetails As String = ""
                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '        "Approve budget", "ps_isa_ord_intfc_H", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                        '        oApprovalDetails.ReqID, sColumnChg:="ORDER_STATUS", sNewValue:=sNewStatus, sErrorDetails:=sErrorDetails) Then

                        '    bSuccess = True
                        'Else
                        '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        'End If
                    End If
                End If

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN  " & vbCrLf & _
                " SET " & vbCrLf & _
                " OPRID_APPROVED_BY = '" & oApprovalDetails.ApproverID & "' " & vbCrLf & _
                ", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                ", ISA_LINE_STATUS = '" & sNewStatus & "' " & vbCrLf & _
                " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        '   HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        Dim sErrorDetails As String = ""
                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '        "Approve budget", "ps_isa_ord_intfc_H", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                        '        oApprovalDetails.ReqID, sColumnChg:="ORDER_STATUS", sNewValue:=sNewStatus, sErrorDetails:=sErrorDetails) Then

                        '    bSuccess = True
                        'Else
                        '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        'End If
                    End If
                End If

            Catch ex As Exception
                bSuccess = False
                '  HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function UpdateLineItem_ApproveBudget(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
            ByVal oApprovalDetails As ApprovalDetails) As Boolean

            Const cCaller As String = "SingleCurrencyOrder.UpdateLineItem_ApproveBudget"

            Dim bSuccess As Boolean = False
            Dim rowsaffected As Integer
            Dim strSQLstring As String

            Try
                Const cNewLineStatus As String = "B"

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN " & vbCrLf & _
                    " SET " & vbCrLf & _
                    " ISA_LINE_STATUS = '" & cNewLineStatus & "' " & vbCrLf & _
                    " WHERE ORDER_NO = " & vbCrLf & _
                    " (SELECT A.ORDER_NO" & vbCrLf & _
                    "   FROM SYSADM8.PS_ISA_ORD_INTF_HD  A" & vbCrLf & _
                    "   WHERE A.BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                    "   AND A.ORDER_NO = '" & oApprovalDetails.ReqID & "' " & vbCrLf & _
                    " ) " & vbCrLf & _
                    " AND ISA_LINE_STATUS IN ('1', '2', 'Q', 'W', 'B', 'V','NEW','QTS','CRE','QTW')"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        '     HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        'Dim sErrorDetails As String = ""
                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '"Approve budget", "SYSADM8.PS_ISA_ORD_INTF_LN", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                        'oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sNewValue:=cNewLineStatus, _
                        'sErrorDetails:=sErrorDetails) Then
                        '    bSuccess = True
                        'Else
                        '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        '    bSuccess = False
                        'End If
                    End If
                End If
            Catch ex As Exception
                bSuccess = False
                '      HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function InsertBudgetAppr(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
                                                 oApprovalDetails As ApprovalDetails, sChgCd As String) As Boolean

            Const cCaller As String = "SingleCurrencyOrder.InsertBudgetAppr"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String
            Dim rowsaffected As Integer

            Try
                strSQLstring = "INSERT INTO PS_ISA_BUDGET_APPR " & vbCrLf & _
                    " SET " & vbCrLf & _
                    " (BUSINESS_UNIT_OM, ORDER_NO, ISA_CUST_CHARGE_CD, " & vbCrLf & _
                    " OPRID_APPROVED_BY, ADD_DTTM, LASTUPDDTTM) " & vbCrLf & _
                    " Values " & vbCrLf & _
                    " ('" & oApprovalDetails.BU & "', '" & oApprovalDetails.ReqID & "', '" & sChgCd & "', ' ', sysdate, '') "

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        '  HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        bSuccess = True
                    End If
                End If
            Catch ex As Exception
                bSuccess = False
                ' HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function GetBudgetApprBU_OM(sBU As String, sReqID As String, sChgCd As String) As String
            Dim sBusinessUnit_OM As String
            Dim strSQLstring As String

            strSQLstring = "Select BUSINESS_UNIT_OM " & vbCrLf & _
                " FROM PS_ISA_BUDGET_APPR " & vbCrLf & _
                " WHERE BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND ORDER_NO = '" & sReqID & "' " & vbCrLf & _
                " AND ISA_CUST_CHARGE_CD = '" & sChgCd & "'"

            sBusinessUnit_OM = ORDBData.GetScalar(strSQLstring)

            Return sBusinessUnit_OM
        End Function

        Private Shared Function IsBudgetLimAndNextApprv(sBU As String, sChgCd As String, sApproverID As String, _
                                                              dblNetUnitPrice As Double, ByRef dblChgCdLimit As Double, _
                                                              ByRef sNextApproverID As String) As Boolean
            Dim bIsLimit As Boolean = False
            Dim strSQLstring As String

            Try
                strSQLstring = "SELECT MAX(B.ISA_CUST_BUDGET_LM), ISA_IOL_APR_ALT " & vbCrLf & _
                    " FROM PS_ISA_CUST_BUDGET B " & vbCrLf & _
                    " WHERE B.BUSINESS_UNIT = '" & sBU & "'" & vbCrLf & _
                    " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'" & vbCrLf & _
                    " AND B.ISA_CUST_BUDGET_LM <= " & dblNetUnitPrice & vbCrLf & _
                    " AND ISA_IOL_APR_ALT <> '" & sApproverID & "'" & vbCrLf & _
                    " GROUP BY ISA_IOL_APR_ALT"
                Dim ds As DataSet
                ds = ORDBData.GetAdapter(strSQLstring)
                If ds.Tables(0).Rows.Count > 1 Then
                    Dim dr As DataRow
                    dr = ds.Tables(0).Rows(0)

                    Dim sCustBudgetLm As String
                    sCustBudgetLm = dr.Item("ISA_CUST_BUDGET_LM").ToString
                    dblChgCdLimit = CType(sCustBudgetLm, Double)

                    sNextApproverID = dr.Item("ISA_IOL_APR_ALT").ToString

                    bIsLimit = True
                End If

            Catch ex As Exception
            End Try

            Return bIsLimit
        End Function

        Private Shared Function RetrieveNetUnitPrice_NSTK_O(sBU As String, sOrdNo As String, sChgCd As String, _
                                                     ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) AS dblNetUnitPrice " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND B.QTY_RECEIVED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If
            strSQLstring &= " AND NOT EXISTS (SELECT 'X' " & vbCrLf & _
                "   FROM PS_INV_ITEMS C " & vbCrLf & _
                "   WHERE C.EFFDT = " & vbCrLf & _
                "       (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED " & vbCrLf & _
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf & _
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf & _
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf & _
                " AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf & _
                " AND C.INV_STOCK_TYPE = 'STK') "

            Dim sNetUnitPriceVar As String
            sNetUnitPriceVar = ORDBData.GetScalar(strSQLstring)
            Try
                dblNetUnitPrice = CType(sNetUnitPriceVar, Double)
                bSuccess = True
            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetUnitPrice_NSTK_D(sBU As String, sOrdNo As String, sEnteredByID As String, sChgCd As String, _
                                                     ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                " AND ((A.ORDER_STATUS in ('O','P',' ') "
            If sChgCd = "|~|" Then
                strSQLstring &= " AND B.OPRID_ENTERED_BY = '" & sEnteredByID & "'"
            End If
            strSQLstring &= " AND TO_CHAR(A.ADD_DTTM) = TO_CHAR(SYSDATE)) "
            If sChgCd = "|~|" Then
                strSQLstring &= "   OR (A.ORDER_STATUS IN ('W','B',' ') "
            Else
                strSQLstring &= "   OR (A.ORDER_STATUS IN ('W','B') "
            End If
            strSQLstring &= "   AND A.ORDER_NO = '" & sOrdNo & "')) " & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO "
            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If
            strSQLstring &= " AND B.QTY_RECEIVED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                " AND NOT EXISTS (SELECT 'X' " & vbCrLf & _
                "   FROM PS_INV_ITEMS C " & vbCrLf & _
                "   WHERE C.EFFDT = " & vbCrLf & _
                "       (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED " & vbCrLf & _
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf & _
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf & _
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf & _
                "   AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf & _
                "   AND C.INV_STOCK_TYPE = 'STK')"

            Dim sNetUnitPriceVar As String
            sNetUnitPriceVar = ORDBData.GetScalar(strSQLstring)
            Try
                dblNetUnitPrice = CType(sNetUnitPriceVar, Double)
                bSuccess = True
            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetUnitPrice_NSTK_M(sBU As String, sOrdNo As String, sEnteredByID As String, sChgCd As String, _
                                                     ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                " AND " & vbCrLf & _
                " ("

            If sChgCd = "|~|" Then
                strSQLstring &= "   (B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' '))" & vbCrLf & _
                "   OR " & vbCrLf & _
                "   (A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "')"
            Else
                strSQLstring &= "   A.ORDER_STATUS in ('O','P',' ')" & vbCrLf & _
                "   OR " & vbCrLf & _
                "   (A.ORDER_STATUS IN ('W','B') AND A.ORDER_NO = '" & sOrdNo & "')"
            End If

            strSQLstring &= vbCrLf & _
                " ) " & vbCrLf & _
                " AND TO_CHAR(B.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO "

            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If

            strSQLstring &= " AND B.QTY_RECEIVED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                " AND NOT EXISTS (SELECT 'X' " & vbCrLf & _
                "   FROM PS_INV_ITEMS C " & vbCrLf & _
                "   WHERE C.EFFDT = " & vbCrLf & _
                "       (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED " & vbCrLf & _
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf & _
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf & _
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf & _
                " AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf & _
                " AND C.INV_STOCK_TYPE = 'STK')"

            Dim sNetUnitPriceVar As String
            sNetUnitPriceVar = ORDBData.GetScalar(strSQLstring)
            Try
                dblNetUnitPrice = CType(sNetUnitPriceVar, Double)
                bSuccess = True
            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetUnitPrice_STK_O(sBU As String, sOrdNo As String, sChgCd As String, _
                                                           ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND B.QTY_RECEIVED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If

            Dim sNetUnitPriceVar As String
            sNetUnitPriceVar = ORDBData.GetScalar(strSQLstring)
            Try
                dblNetUnitPrice = CType(sNetUnitPriceVar, Double)
                bSuccess = True
            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetUnitPrice_STK_D(sBU As String, sOrdNo As String, sEnteredByID As String, sChgCd As String, _
                                                           ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                " AND " & vbCrLf & _
                "("
            If sChgCd = "|~|" Then
                strSQLstring &= "  (B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') AND TO_CHAR(A.ADD_DTTM) = TO_CHAR(SYSDATE)) " & vbCrLf & _
                "  OR " & vbCrLf & _
                "  (A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "')"
            Else
                strSQLstring &= "  (A.ORDER_STATUS in ('O','P',' ') AND TO_CHAR(A.ADD_DTTM) = TO_CHAR(SYSDATE)) " & vbCrLf & _
                "  OR " & vbCrLf & _
                "  (A.ORDER_STATUS IN ('W','B') AND A.ORDER_NO = '" & sOrdNo & "')"

            End If
            strSQLstring &= ") " & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND B.QTY_RECEIVED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If

            Dim sNetUnitPriceVar As String
            sNetUnitPriceVar = ORDBData.GetScalar(strSQLstring)
            Try
                dblNetUnitPrice = CType(sNetUnitPriceVar, Double)
                bSuccess = True
            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetUnitPrice_STK_M(sBU As String, sOrdNo As String, sEnteredByID As String, sChgCd As String, _
                                                           ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                " AND " & vbCrLf & _
                " ("
            If sChgCd = "|~|" Then
                strSQLstring &= "   (B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ')) " & vbCrLf & _
                "   OR " & vbCrLf & _
                "   (A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "') "
            Else
                strSQLstring &= "    A.ORDER_STATUS in ('O','P',' ') " & vbCrLf & _
                "   OR " & vbCrLf & _
                "   (A.ORDER_STATUS IN ('W','B') AND A.ORDER_NO = '" & sOrdNo & "') "
            End If
            strSQLstring &= " ) " & vbCrLf & _
                " AND TO_CHAR(B.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND B.QTY_RECEIVED > 0 AND B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If

            Dim sNetUnitPriceVar As String
            sNetUnitPriceVar = ORDBData.GetScalar(strSQLstring)
            Try
                dblNetUnitPrice = CType(sNetUnitPriceVar, Double)
                bSuccess = True
            Catch ex As Exception
            End Try

            Return bSuccess

        End Function

        Public Shared Function ExecuteSQL(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, strSQLstring As String, _
                                   oApprovalDetails As ApprovalDetails, ByRef rowsaffected As Integer, sCaller As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim exError As Exception

            If ExecuteNonQuery(trnsactSession, connection, strSQLstring, rowsaffected, exError) Then
                bSuccess = True
            Else
                bSuccess = False

                'HandleApprovalError(trnsactSession, connection, strSQLstring, exError, oApprovalDetails, sCaller)
            End If

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetUnitPrice(bCalcNSTKOnly As Boolean, strOrdApprType As String, _
                                               sBU As String, sEnteredByID As String, sReqID As String, _
                                               ByRef dblNetUnitPrice As Double, Optional strChgCd As String = "|~|") As Boolean
            Dim bSuccess As Boolean = False

            Try
                If bCalcNSTKOnly Then
                    If strOrdApprType = "O" Then
                        RetrieveNetUnitPrice_NSTK_O(sBU, sReqID, strChgCd, dblNetUnitPrice)
                    ElseIf strOrdApprType = "D" Then
                        RetrieveNetUnitPrice_NSTK_D(sBU, sReqID, sEnteredByID, strChgCd, dblNetUnitPrice)
                    ElseIf strOrdApprType = "M" Then
                        RetrieveNetUnitPrice_NSTK_M(sBU, sReqID, sEnteredByID, strChgCd, dblNetUnitPrice)
                    End If
                Else
                    If strOrdApprType = "O" Then
                        RetrieveNetUnitPrice_STK_O(sBU, sReqID, strChgCd, dblNetUnitPrice)
                    ElseIf strOrdApprType = "D" Then
                        RetrieveNetUnitPrice_STK_D(sBU, sReqID, sEnteredByID, strChgCd, dblNetUnitPrice)
                    ElseIf strOrdApprType = "M" Then
                        RetrieveNetUnitPrice_STK_M(sBU, sReqID, sEnteredByID, strChgCd, dblNetUnitPrice)
                    End If
                End If

                bSuccess = True
            Catch ex As Exception
                bSuccess = False
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetOrderPrice(sBU As String, sOrdNo As String, ByRef dblNetOrderPrice As Double, _
                                   Optional sChgCd As String = "|~|") As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            Try
                strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) AS NET_ORDR_PRICE_VAR " & vbCrLf & _
                    " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                    " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf & _
                    " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                    " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' "
                If sChgCd <> "|~|" Then
                    strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
                End If
                Dim sNetOrderPrice As String
                sNetOrderPrice = ORDBData.GetScalar(strSQLstring)
                Try
                    dblNetOrderPrice = CType(sNetOrderPrice, Double)
                    bSuccess = True
                Catch ex As Exception
                    dblNetOrderPrice = 0
                End Try
            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveOrderLimAndNextApprv(BU As String, sEnteredBy As String, sApproverID As String, ByRef dblApprLimit As Double, _
                                                   ByRef sNextApproverID As String, ByRef eErrorInApprovals As ApprovalResults.ApprovalError) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String = ""

            Try
                Dim sChainToCurrentApprover As String = GetCurrentApprovalChain(BU, sEnteredBy, sApproverID)

                strSQLstring = "SELECT A.ISA_IOL_APR_LIMIT, A.ISA_IOL_APR_ALT " & vbCrLf & _
                " FROM SDIX_USERS_APPRV A " & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & sApproverID & "' " & vbCrLf & _
                " AND BUSINESS_UNIT = '" & BU & "'"

                Dim ds As DataSet
                ds = ORDBData.GetAdapter(strSQLstring)
                If ds.Tables(0).Rows.Count = 1 Then
                    Dim dr As DataRow
                    dr = ds.Tables(0).Rows(0)

                    If sChainToCurrentApprover.IndexOf(m_cApproverDelimiter & dr.Item("isa_iol_apr_alt").ToString.Trim.ToUpper & m_cApproverDelimiter) >= 0 Then
                        ' This means we already encountered this next approver earlier in the chain so we're in a loop with approvers.
                        ' We need to let the user know that the approval chain in incorrect.
                        eErrorInApprovals = ApprovalResults.ApprovalError.InvalidApprovalChain
                        bSuccess = False
                    Else
                        Dim sAprLimit As String
                        sAprLimit = dr.Item("isa_iol_apr_limit").ToString
                        dblApprLimit = CType(sAprLimit, Double)

                        sNextApproverID = dr.Item("isa_iol_apr_alt").ToString
                        bSuccess = True
                    End If
                Else
                    dblApprLimit = 0
                    sNextApproverID = ""

                    ' Even if a next approver is not found, this is still a successful search.
                    bSuccess = True
                End If

            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function GetCurrentApprovalChain(sBU As String, sEnteredBy As String, sCurrentApprover As String) As String
            Dim strSQLstring As String
            Dim bFinalApprover As Boolean = False
            Dim sNextApprover As String = sEnteredBy
            Dim sApproverList As String = m_cApproverDelimiter & sEnteredBy.Trim.ToUpper & m_cApproverDelimiter

            If sEnteredBy <> sCurrentApprover Then
                While Not bFinalApprover
                    strSQLstring = "SELECT A.isa_employee_ID AS Approver, A.isa_iol_apr_limit AS Limit, A.isa_iol_apr_alt AS NextApprover " & vbCrLf & _
                        " , A.business_unit AS BU " & vbCrLf & _
                        " FROM SDIX_USERS_APPRV A WHERE A.isa_employee_id = '" & sNextApprover & "' " & vbCrLf & _
                        " AND A.business_unit = '" & sBU & "'"

                    Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)

                    If ds.Tables(0).Rows.Count > 0 Then
                        Dim drOrig As DataRow = ds.Tables(0).Rows(0)

                        sNextApprover = drOrig.Item("NextApprover").ToString
                        If sNextApprover = sCurrentApprover Then
                            ' we reached the current approver so now we have the whole chain up to this current approver
                            bFinalApprover = True
                        ElseIf drOrig.Item("Approver").ToString.Trim.ToUpper = drOrig.Item("NextApprover").ToString.Trim.ToUpper Then
                            ' the current approver and the next approver are the same in the record, we're in a loop so break out
                            bFinalApprover = True
                        ElseIf sApproverList.IndexOf(m_cApproverDelimiter & drOrig.Item("NextApprover").ToString.Trim.ToUpper & m_cApproverDelimiter) >= 0 Then
                            ' the approver has already been encountered so now we'll get into a loop unless we break out here
                            bFinalApprover = True
                        End If
                        sApproverList &= m_cApproverDelimiter & drOrig.Item("NextApprover").ToString.Trim.ToUpper & m_cApproverDelimiter
                    Else
                        bFinalApprover = True
                    End If
                End While
            End If

            Return sApproverList
        End Function
    End Class

    Private Class MultiCurrencyOrder
        Private Shared m_cClassName As String = "OrderApprovals.MultiCurrencyOrder."

        Public Shared Function CustEmp(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
            oApprovalDetails As ApprovalDetails, sBaseCurrCd As String, iExDaysAgo As Integer, _
            oApprovalResults As ApprovalResults) As Boolean

            Dim bSuccessful As Boolean = True ' Processing is successful by default
            Dim sOrdApprType As String = "O"
            Dim dblTotalCost As Double

            oApprovalResults.UpdateEmployeeResults(False, "P", oApprovalDetails.ApproverID)

            Dim oEnterprise As New clsEnterprise(oApprovalDetails.BU)
            'sOrdApprType = oEnterprise.OrdApprType

            If sOrdApprType.Trim.Length > 0 Then
                If RetrieveNetOrderPrice(oApprovalDetails.BU, oApprovalDetails.ReqID, dblTotalCost) Then
                    If dblTotalCost > 0 Then
                        dblTotalCost = 0

                        Dim dsData As New DataSet
                        Dim bError As Boolean = False

                        If oEnterprise.ApprNSTKFlag = "Y" Then 'consider NSTK only; exclude STK
                            If sOrdApprType = "O" Then
                                RetrieveSumByCurr_NSTK_O(dsData, oApprovalDetails.BU, oApprovalDetails.ReqID)
                                dblTotalCost = GetTotalCost(dsData, sBaseCurrCd, iExDaysAgo)

                            ElseIf sOrdApprType = "D" Then
                                RetrieveSumByCurr_NSTK_D(dsData, oApprovalDetails.BU, oApprovalDetails.ReqID, oApprovalDetails.EnteredByID)
                                dblTotalCost = GetTotalCost(dsData, sBaseCurrCd, iExDaysAgo)

                            ElseIf sOrdApprType = "M" Then
                                RetrieveSumByCurr_NSTK_M(dsData, oApprovalDetails.BU, oApprovalDetails.ReqID, oApprovalDetails.EnteredByID)
                                dblTotalCost = GetTotalCost(dsData, sBaseCurrCd, iExDaysAgo)

                            End If
                        Else ' consider NSTK and STK
                            Dim iReqLnCount As Integer
                            If GetReqLineCount(trnsactSession, connection, oApprovalDetails, iReqLnCount) Then
                                If iReqLnCount = 0 Then
                                    If sOrdApprType = "O" Then
                                        dblTotalCost = RetrieveTotalCost_NoReq_O(oApprovalDetails.BU, oApprovalDetails.ReqID)
                                    ElseIf sOrdApprType = "D" Then
                                        dblTotalCost = RetrieveTotalCost_NoReq_D(oApprovalDetails.BU, oApprovalDetails.EnteredByID, oApprovalDetails.ReqID)
                                    ElseIf sOrdApprType = "M" Then
                                        dblTotalCost = RetrieveTotalCost_NoReq_M(oApprovalDetails.BU, oApprovalDetails.EnteredByID)
                                    End If
                                Else
                                    If sOrdApprType = "O" Then
                                        RetrieveSumByCurr_STK_O(dsData, oApprovalDetails.BU, oApprovalDetails.ReqID)
                                        dblTotalCost = GetTotalCost(dsData, sBaseCurrCd, iExDaysAgo)

                                    ElseIf sOrdApprType = "D" Then
                                        RetrieveSumByCurr_STK_D(dsData, oApprovalDetails.BU, oApprovalDetails.EnteredByID, oApprovalDetails.ReqID)
                                        dblTotalCost = GetTotalCost(dsData, sBaseCurrCd, iExDaysAgo)

                                    ElseIf sOrdApprType = "M" Then
                                        RetrieveSumByCurr_STK_M(dsData, oApprovalDetails.BU, oApprovalDetails.EnteredByID, oApprovalDetails.ReqID)
                                        dblTotalCost = GetTotalCost(dsData, sBaseCurrCd, iExDaysAgo)

                                    End If
                                End If
                            Else
                                bError = True
                                bSuccessful = False
                            End If
                        End If

                        If Not bError And dblTotalCost > 0 Then
                            Dim dblApprLimit As Double = 0
                            Dim strSQLstring As String
                            Dim rowsaffected As Integer

                            If RetrieveOrderLimAndNextApprv(oApprovalDetails.BU, oApprovalDetails.EnteredByID, oApprovalDetails.ApproverID, dblApprLimit, oApprovalResults.NextOrderApprover, oApprovalResults.ErrorInApproval) Then
                                If oApprovalResults.ErrorInApproval = ApprovalResults.ApprovalError.NoError Then
                                    If dblTotalCost > dblApprLimit And oApprovalResults.NextOrderApprover.Trim.Length > 0 Then
                                        oApprovalResults.OrderExceededLimit = True

                                        'If UpdateLineItem_ApproveOrder(trnsactSession, connection, oApprovalDetails) Then
                                        If Not UpdateHeader_ApproveOrder(trnsactSession, connection, oApprovalDetails, oApprovalResults) Then
                                            bSuccessful = False
                                        End If
                                        'Else
                                        '    bSuccessful = False
                                        'End If
                                    Else
                                        oApprovalResults.OrderExceededLimit = False
                                    End If
                                Else
                                    bSuccessful = False
                                End If
                            Else
                                bSuccessful = False
                            End If
                        End If
                    End If
                Else
                    bSuccessful = False
                End If
            End If

            Return bSuccessful
        End Function

        Private Shared Function UpdateHeader_ApproveOrder(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
            ByVal oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults) As Boolean

            Const cNewHeaderStatus As String = "QTW"
            Const cCaller As String = "MultiCurrencyOrder.UpdateHeader_ApproveOrder"
            Const cNewLineStatus As String = "QTW"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String
            Dim rowsaffected As Integer

            Try
                oApprovalResults.NewOrderHeaderStatus = cNewHeaderStatus

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD " & vbCrLf & _
                    " SET " & vbCrLf & _
                    " LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                    ", ORDER_STATUS = '" & cNewHeaderStatus & "' " & vbCrLf & _
                    " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                    " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        '     HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        'Dim sErrorDetails As String = ""
                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '        "Approve order", "SYSADM8.PS_ISA_ORD_INTF_HD", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                        '        oApprovalDetails.ReqID, sColumnChg:="ORDER_STATUS", sNewValue:=cNewHeaderStatus, sErrorDetails:=sErrorDetails) Then

                        '    bSuccess = True
                        'Else
                        '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        'End If
                    End If
                End If

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN " & vbCrLf & _
                    " SET " & vbCrLf & _
                    " OPRID_APPROVED_BY = '" & oApprovalDetails.ApproverID & "' " & vbCrLf & _
                    ", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                    ", ISA_LINE_STATUS = '" & cNewLineStatus & "' " & vbCrLf & _
                    " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                    " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        ' HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        'Dim sErrorDetails As String = ""
                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '        "Approve order", "SYSADM8.PS_ISA_ORD_INTF_LN", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                        '        oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sNewValue:=cNewLineStatus, sErrorDetails:=sErrorDetails) Then

                        '    bSuccess = True
                        'Else
                        '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        'End If
                    End If
                End If

            Catch ex As Exception
                bSuccess = False
                '   HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function UpdateLineItem_ApproveOrder(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
            ByVal oApprovalDetails As ApprovalDetails) As Boolean

            Const cNewLineStatus As String = "QTW"  '  "W"
            Const cCaller As String = "MultiCurrencyOrder.UpdateLineItem_ApproveOrder"

            Dim bSuccess As Boolean = False
            Dim rowsaffected As Integer
            Dim strSQLstring As String

            Try
                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN" & vbCrLf & _
                    " SET " & vbCrLf & _
                    " ISA_LINE_STATUS = '" & cNewLineStatus & "' " & vbCrLf & _
                    " WHERE ORDER_NO = " & vbCrLf & _
                    " (SELECT A.ORDER_NO" & vbCrLf & _
                    "   FROM SYSADM8.PS_ISA_ORD_INTF_HD A" & vbCrLf & _
                    "   WHERE A.BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                    "   AND A.ORDER_NO = '" & oApprovalDetails.ReqID & "' " & vbCrLf & _
                    " ) " & vbCrLf & _
                    " AND ISA_LINE_STATUS IN ('1','2','Q','W','B','QTS','QTW','NEW','CRE') "

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        ' This is not necessarily an error
                        bSuccess = True
                    Else
                        Dim sErrorDetails As String = ""
                        'If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, _
                        '"Approve order", "SYSADM8.PS_ISA_ORD_INTF_LN ", oApprovalDetails.ApproverID, oApprovalDetails.BU, _
                        'oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sNewValue:=cNewLineStatus, _
                        'sErrorDetails:=sErrorDetails) Then
                        '    bSuccess = True
                        'Else
                        '    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        '    bSuccess = False
                        'End If
                    End If
                End If
            Catch ex As Exception
                bSuccess = False
                ' HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_NSTK_O(ByRef ds As DataSet, sBU As String, sOrdNo As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT  R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf & _
                " FROM  SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf & _
                " WHERE  A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND  A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf & _
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                " AND  NOT EXISTS (SELECT 'X' " & vbCrLf & _
                "   FROM PS_INV_ITEMS C " & vbCrLf & _
                "   WHERE C.EFFDT = (SELECT MAX(C_ED.EFFDT) " & vbCrLf & _
                "       FROM PS_INV_ITEMS C_ED " & vbCrLf & _
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf & _
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf & _
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf & _
                "   AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf & _
                "   AND C.INV_STOCK_TYPE = 'STK') " & vbCrLf & _
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf & _
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf & _
                " GROUP BY R.CURRENCY_CD"
            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function GetTotalCost(dsData As DataSet, sBaseCurrCd As String, iExDaysAgo As Integer) As Double
            Dim dblTotalCost As Double = 0.0

            For Each dr As DataRow In dsData.Tables(0).Rows
                Dim dblSubTotal As Double
                dblSubTotal = CType(dr.Item("SUBTOTAL").ToString(), Double)

                ' Remove test for the need to convert. The converted price from the Requestor Approval
                ' will be written to net_unit_price of interface L when the request is approved.
                ' Items not needing a quote will already have their converted price written to net_unit_price.

                'If dr.Item("CURRENCY_CD").ToString = sBaseCurrCd Then
                dblTotalCost = dblTotalCost + dblSubTotal
                'Else
                'Dim drExRate As DataRow
                'GetExchangeRate(dr.Item("CURRENCY_CD").ToString, sBaseCurrCd, iExDaysAgo, drExRate)
                'Dim dblExRate As Double
                'dblExRate = CType(drExRate.Item("EXRATE").ToString, Double)
                'dblTotalCost = dblTotalCost + (dblSubTotal * dblExRate)
                'End If
            Next

            Return dblTotalCost
        End Function

        Private Shared Function RetrieveSumByCurr_NSTK_D(ByRef ds As DataSet, sBU As String, sOrdNo As String, sEnteredBy As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND " & vbCrLf & _
                " ( " & vbCrLf & _
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredBy & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf & _
                "     AND TRUNC(A.ADD_DTTM) = TRUNC(SYSDATE) " & vbCrLf & _
                "   ) " & vbCrLf & _
                "   OR " & vbCrLf & _
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' ) " & vbCrLf & _
                " ) " & vbCrLf & _
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                " AND  NOT EXISTS (SELECT 'X' " & vbCrLf & _
                "   FROM  PS_INV_ITEMS C " & vbCrLf & _
                "   WHERE  C.EFFDT = (SELECT MAX(C_ED.EFFDT) " & vbCrLf & _
                "       FROM PS_INV_ITEMS C_ED " & vbCrLf & _
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf & _
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf & _
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf & _
                "   AND C.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
                "   AND C.INV_STOCK_TYPE = 'STK') " & vbCrLf & _
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf & _
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf & _
                " GROUP BY R.CURRENCY_CD "
            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_NSTK_M(ByRef ds As DataSet, sBU As String, sOrdNo As String, sEnteredBy As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND " & vbCrLf & _
                " ( " & vbCrLf & _
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredBy & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf & _
                "     AND TO_CHAR(A.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf & _
                "   ) " & vbCrLf & _
                "   OR " & vbCrLf & _
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf & _
                "   ) " & vbCrLf & _
                " ) " & vbCrLf & _
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                " AND  NOT EXISTS (SELECT 'X' " & vbCrLf & _
                "   FROM  PS_INV_ITEMS C " & vbCrLf & _
                "   WHERE  C.EFFDT = (SELECT MAX(C_ED.EFFDT) " & vbCrLf & _
                "       FROM PS_INV_ITEMS C_ED " & vbCrLf & _
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf & _
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf & _
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf & _
                "   AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf & _
                "   AND C.INV_STOCK_TYPE = 'STK') " & vbCrLf & _
                " AND  A.ORDER_NO=R.REQ_ID " & vbCrLf & _
                " AND  B.ISA_INTFC_LN=R.LINE_NBR " & vbCrLf & _
                " GROUP BY R.CURRENCY_CD"

            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function GetReqLineCount(trnsactSession As OleDbTransaction, connection As OleDbConnection, _
                                                oApprovalDetails As ApprovalDetails, ByRef iReqLnCount As Integer) As Boolean

            Const cCaller As String = "MultiCurrencyOrder.GetReqLineCount"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            Try
                iReqLnCount = 0

                strSQLstring = "SELECT  COUNT(1) " & vbCrLf & _
                    " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf & _
                    " WHERE  A.BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                    " AND  A.ORDER_NO = '" & oApprovalDetails.ReqID & "' " & vbCrLf & _
                    " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                    " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                    " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf & _
                    " AND  B.ISA_INTFC_LN = R.LINE_NBR"

                Dim sReqLnCount As String
                sReqLnCount = ORDBData.GetScalar(strSQLstring)
                iReqLnCount = CType(sReqLnCount, Integer)
                bSuccess = True

            Catch ex As Exception
                bSuccess = False
                '   HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess

        End Function

        Private Shared Function RetrieveTotalCost_NoReq_O(sBU As String, sOrdNo As String) As Double
            Dim dblTotalCost As Double = 0
            Dim strSQLstring As String

            strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "'"

            Dim sTotalCost As String
            sTotalCost = ORDBData.GetScalar(strSQLstring)
            dblTotalCost = CType(sTotalCost, Double)

            Return dblTotalCost
        End Function

        Private Shared Function RetrieveTotalCost_NoReq_D(sBU As String, sEnteredByID As String, sOrdNo As String) As Double
            Dim dblTotalCost As Double = 0
            Dim strSQLstring As String

            strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND " & vbCrLf & _
                " ( " & vbCrLf & _
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf & _
                "     AND TRUNC(A.ADD_DTTM) = TRUNC(SYSDATE) " & vbCrLf & _
                "   ) " & vbCrLf & _
                "   OR " & vbCrLf & _
                "   (     A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf & _
                "   ) " & vbCrLf & _
                " ) " & vbCrLf & _
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND  B.QTY_REQUESTED > 0 AND B.ISA_LINE_STATUS = '" & m_cDeletedLine & "'"

            Dim sTotalCost As String
            sTotalCost = ORDBData.GetScalar(strSQLstring)
            dblTotalCost = CType(sTotalCost, Double)

            Return dblTotalCost
        End Function

        Private Shared Function RetrieveTotalCost_NoReq_M(sBU As String, sEnteredByID As String) As Double
            Dim dblTotalCost As Double = 0
            Dim strSQLstring As String

            strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND " & vbCrLf & _
                " ( " & vbCrLf & _
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf & _
                "     AND TO_CHAR(A.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf & _
                "   ) " & vbCrLf & _
                "   OR " & vbCrLf & _
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = V_ORDNO " & vbCrLf & _
                "   ) " & vbCrLf & _
                " ) " & vbCrLf & _
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND  B.QTY_REQUESTED > 0 AND B.ISA_LINE_STATUS = '" & m_cDeletedLine & "'"

            Dim sTotalCost As String
            sTotalCost = ORDBData.GetScalar(strSQLstring)
            dblTotalCost = CType(sTotalCost, Double)

            Return dblTotalCost
        End Function

        Private Shared Function RetrieveSumByCurr_STK_O(ByRef ds As DataSet, sBU As String, sOrdNo As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT  R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf & _
                " FROM  SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf & _
                " WHERE  A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND  A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf & _
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf & _
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf & _
                " GROUP BY R.CURRENCY_CD"

            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_STK_D(ByRef ds As DataSet, sBU As String, sEnteredByID As String, sOrdNo As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND " & vbCrLf & _
                " ( " & vbCrLf & _
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf & _
                "     AND TRUNC(B.ADD_DTTM) = TRUNC(SYSDATE) " & vbCrLf & _
                "   ) " & vbCrLf & _
                "   OR " & vbCrLf & _
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf & _
                "   ) " & vbCrLf & _
                " ) " & vbCrLf & _
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf & _
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf & _
                " GROUP BY R.CURRENCY_CD"

            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_STK_M(ByRef ds As DataSet, sBU As String, sEnteredByID As String, sOrdNo As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf & _
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf & _
                " AND " & vbCrLf & _
                " ( " & vbCrLf & _
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf & _
                "     AND TO_CHAR(A.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf & _
                "   ) " & vbCrLf & _
                "   OR " & vbCrLf & _
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf & _
                "   ) " & vbCrLf & _
                " ) " & vbCrLf & _
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf & _
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf & _
                " GROUP BY R.CURRENCY_CD"

            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function GetExchangeRate(sFromCurrCd As String, sToCurrCd As String, iExchDaysAgo As Integer, _
                                        ByRef dr As DataRow) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String
            Dim ds As New DataSet

            Try
                strSQLstring = "SELECT A.RATE_MULT/A.RATE_DIV EXRATE " & vbCrLf & _
                " FROM SYSADM8.PS_RT_RATE_TBL A " & vbCrLf & _
                " WHERE A.EFFDT = (  SELECT MAX(A1.EFFDT) " & vbCrLf & _
                "   FROM SYSADM8.PS_RT_RATE_TBL A1 " & vbCrLf & _
                "   WHERE A1.RT_RATE_INDEX = A.RT_RATE_INDEX " & vbCrLf & _
                "   AND A1.TERM          = A.TERM " & vbCrLf & _
                "   AND A1.FROM_CUR      = A.FROM_CUR " & vbCrLf & _
                "   AND A1.TO_CUR        = A.TO_CUR " & vbCrLf & _
                "   AND A1.RT_TYPE       = A.RT_TYPE " & vbCrLf & _
                "   AND A1.EFFDT        <= (SYSDATE - " & iExchDaysAgo & ") " & vbCrLf & _
                "   ) " & vbCrLf & _
                " AND A.FROM_CUR = '" & sFromCurrCd & "' " & vbCrLf & _
                " AND A.TO_CUR = '" & sToCurrCd & "' " & vbCrLf & _
                " AND A.RT_RATE_INDEX = 'MODEL' " & vbCrLf & _
                " AND A.RT_TYPE = 'CRRNT'"

                ds = ORDBData.GetAdapter(strSQLstring)
                If ds.Tables(0).Rows.Count > 0 Then
                    dr = ds.Tables(0).Rows(0)
                End If

                bSuccess = True

            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Public Shared Function ExecuteSQL(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, strSQLstring As String, _
                                    oApprovalDetails As ApprovalDetails, ByRef rowsaffected As Integer, sCaller As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim exError As Exception

            If ExecuteNonQuery(trnsactSession, connection, strSQLstring, rowsaffected, exError) Then
                bSuccess = True
            Else
                bSuccess = False

                '    HandleApprovalError(trnsactSession, connection, strSQLstring, exError, oApprovalDetails, sCaller)
            End If

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetOrderPrice(sBU As String, sOrdNo As String, ByRef dblNetOrderPrice As Double, _
                                        Optional sChgCd As String = "|~|") As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            Try
                strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) AS NET_ORDR_PRICE_VAR " & vbCrLf & _
                    " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
                    " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf & _
                    " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                    " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' "
                If sChgCd <> "|~|" Then
                    strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
                End If
                Dim sNetOrderPrice As String
                sNetOrderPrice = ORDBData.GetScalar(strSQLstring)
                Try
                    dblNetOrderPrice = CType(sNetOrderPrice, Double)
                    bSuccess = True
                Catch ex As Exception
                    dblNetOrderPrice = 0
                End Try
            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveOrderLimAndNextApprv(BU As String, sEnteredBy As String, sApproverID As String, ByRef dblApprLimit As Double, _
                                                   ByRef sNextApproverID As String, ByRef eErrorInApprovals As ApprovalResults.ApprovalError) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String = ""

            Try
                Dim sChainToCurrentApprover As String = GetCurrentApprovalChain(BU, sEnteredBy, sApproverID)

                strSQLstring = "SELECT A.ISA_IOL_APR_LIMIT, A.ISA_IOL_APR_ALT " & vbCrLf & _
                " FROM SDIX_USERS_APPRV A " & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & sApproverID & "' " & vbCrLf & _
                " AND BUSINESS_UNIT = '" & BU & "'"

                Dim ds As DataSet
                ds = ORDBData.GetAdapter(strSQLstring)
                If ds.Tables(0).Rows.Count = 1 Then
                    Dim dr As DataRow
                    dr = ds.Tables(0).Rows(0)

                    If sChainToCurrentApprover.IndexOf(m_cApproverDelimiter & dr.Item("isa_iol_apr_alt").ToString.Trim.ToUpper & m_cApproverDelimiter) >= 0 Then
                        ' This means we already encountered this next approver earlier in the chain so we're in a loop with approvers.
                        ' We need to let the user know that the approval chain in incorrect.
                        eErrorInApprovals = ApprovalResults.ApprovalError.InvalidApprovalChain
                        bSuccess = False
                    Else
                        Dim sAprLimit As String
                        sAprLimit = dr.Item("isa_iol_apr_limit").ToString
                        dblApprLimit = CType(sAprLimit, Double)

                        sNextApproverID = dr.Item("isa_iol_apr_alt").ToString
                        bSuccess = True
                    End If
                Else
                    dblApprLimit = 0
                    sNextApproverID = ""

                    ' Even if a next approver is not found, this is still a successful search.
                    bSuccess = True
                End If

            Catch ex As Exception
            End Try

            Return bSuccess
        End Function

        Private Shared Function GetCurrentApprovalChain(sBU As String, sEnteredBy As String, sCurrentApprover As String) As String
            Dim strSQLstring As String
            Dim bFinalApprover As Boolean = False
            Dim sNextApprover As String = sEnteredBy
            Dim sApproverList As String = m_cApproverDelimiter & sEnteredBy.Trim.ToUpper & m_cApproverDelimiter

            If sEnteredBy <> sCurrentApprover Then
                While Not bFinalApprover
                    strSQLstring = "SELECT A.isa_employee_ID AS Approver, A.isa_iol_apr_limit AS Limit, A.isa_iol_apr_alt AS NextApprover " & vbCrLf & _
                        " , A.business_unit AS BU " & vbCrLf & _
                        " FROM SDIX_USERS_APPRV A WHERE A.isa_employee_id = '" & sNextApprover & "' " & vbCrLf & _
                        " AND A.business_unit = '" & sBU & "'"

                    Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)

                    If ds.Tables(0).Rows.Count > 0 Then
                        Dim drOrig As DataRow = ds.Tables(0).Rows(0)

                        sNextApprover = drOrig.Item("NextApprover").ToString
                        If sNextApprover = sCurrentApprover Then
                            ' we reached the current approver so now we have the whole chain up to this current approver
                            bFinalApprover = True
                        ElseIf drOrig.Item("Approver").ToString.Trim.ToUpper = drOrig.Item("NextApprover").ToString.Trim.ToUpper Then
                            ' the current approver and the next approver are the same in the record, we're in a loop so break out
                            bFinalApprover = True
                        ElseIf sApproverList.IndexOf(m_cApproverDelimiter & drOrig.Item("NextApprover").ToString.Trim.ToUpper & m_cApproverDelimiter) >= 0 Then
                            ' the approver has already been encountered so now we'll get into a loop unless we break out here
                            bFinalApprover = True
                        End If
                        sApproverList &= m_cApproverDelimiter & drOrig.Item("NextApprover").ToString.Trim.ToUpper & m_cApproverDelimiter
                    Else
                        bFinalApprover = True
                    End If
                End While
            End If

            Return sApproverList
        End Function
    End Class

    Public Class ApprovalDetails
        Private m_oLineDetails As New ArrayList
        Public ReadOnly Property LineDetails As ArrayList
            Get
                Return m_oLineDetails
            End Get
        End Property

        Private m_sNSTKOnlyFlag As String
        Public ReadOnly Property NSTKOnlyFlag As String
            Get
                Return m_sNSTKOnlyFlag
            End Get
        End Property

        Private m_bOROTreatedLikeSTK As String
        Public ReadOnly Property OROTreatedLikeSTK As Boolean
            Get
                Return m_bOROTreatedLikeSTK
            End Get
        End Property

        Private m_sBU As String
        Public ReadOnly Property BU As String
            Get
                Return m_sBU
            End Get
        End Property

        Private m_sEnteredByID As String
        Public Property EnteredByID As String
            Get
                Return m_sEnteredByID
            End Get
            Set(value As String)
                m_sEnteredByID = value
            End Set
        End Property

        Private m_sApproverID As String
        Public ReadOnly Property ApproverID As String
            Get
                Return m_sApproverID
            End Get
        End Property

        Private m_sSDIEmp As String
        Public ReadOnly Property SDIEmp As String
            Get
                Return m_sSDIEmp
            End Get
        End Property

        Private m_sSiteBU As String
        Public ReadOnly Property SiteBU As String
            Get
                Return m_sSiteBU
            End Get
        End Property

        Private m_sSitePrefix As String
        Public ReadOnly Property SitePrefix As String
            Get
                Return m_sSitePrefix
            End Get
        End Property

        Private m_sReqID As String
        Public ReadOnly Property ReqID As String
            Get
                Return m_sReqID
            End Get
        End Property

        Private m_sWO As String = " "
        Public Property WorkOrder As String
            Get
                Return m_sWO
            End Get
            Set(value As String)
                m_sWO = value
            End Set
        End Property

        Public Sub New(sBU As String, sEnteredByID As String, sApproverID As String, sReqID As String)
            m_sBU = sBU
            m_sEnteredByID = sEnteredByID
            m_sApproverID = sApproverID
            m_sReqID = sReqID

            m_sSiteBU = GetSiteBU(sBU)
            m_sSitePrefix = GetSitePrefix(sBU)

            Dim oUser As New clsUserTbl(sApproverID, sBU)
            m_sSDIEmp = oUser.SDICUSTFlag

            Dim oEnterprise As New clsEnterprise(sBU)
            m_sNSTKOnlyFlag = oEnterprise.ApprNSTKFlag
            m_bOROTreatedLikeSTK = oEnterprise.OROTreatedLikeSTK
        End Sub

        Public Shared Function GetSiteBU(ByVal sBU As String, Optional ByVal strTaxCompany As String = "", Optional ByVal strSiteBu1 As String = "") As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            '     Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
            Dim strSiteBU As String
            Dim dtrPrefixReader As OleDbDataReader = Nothing
            Dim strSQLstring As String
            Try
                If Trim(strSiteBu1) <> "" Then
                    strSiteBU = Trim(strSiteBu1)
                Else
                    strSQLstring = "SELECT A.BUSINESS_UNIT" & vbCrLf & _
                        " FROM PS_REQ_LOADER_DFL A" & vbCrLf & _
                        " WHERE SUBSTR(A.LOADER_BU,2) = '" & sBU.Substring(1, 4) & "'" & vbCrLf
                    dtrPrefixReader = ORDBData.GetReader(strSQLstring)
                    If dtrPrefixReader.Read() Then
                        strSiteBU = dtrPrefixReader("BUSINESS_UNIT").ToString
                    Else
                        If strTaxCompany = "" Then
                            dtrPrefixReader.Close()
                            strSQLstring = "SELECT A.TAX_COMPANY" & vbCrLf & _
                                        " FROM PS_BUS_UNIT_TBL_OM A" & vbCrLf & _
                                        " WHERE A.BUSINESS_UNIT = '" & sBU & "'" & vbCrLf
                            dtrPrefixReader = ORDBData.GetReader(strSQLstring)
                            If dtrPrefixReader.Read() Then
                                strSiteBU = dtrPrefixReader("TAX_COMPANY").ToString
                            End If
                        Else
                            strSiteBU = Trim(strTaxCompany)
                        End If
                    End If
                    dtrPrefixReader.Close()
                End If

                If Trim(strSiteBU) = "" Then
                    strSiteBU = "ISA00"
                End If

                Return strSiteBU
            Catch objException As Exception
                Try
                    dtrPrefixReader.Close()
                Catch ex As Exception

                End Try
                'sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLstring)
                'currentApp.Response.Redirect("DBErrorPage.aspx")

            End Try

        End Function

        Public Shared Function GetSitePrefix(ByVal sBU As String) As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            '  Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

            Dim strSitePrefix As String = "XXX"

            'Try
            '    strSitePrefix = currentApp.Session("SITEPREFIX")
            'Catch ex As Exception
            '    strSitePrefix = "XXX"
            'End Try

            'Dim strSQLString = "SELECT ISA_SITE_CODE" & vbCrLf & _
            '    " FROM PS_BUS_UNIT_TBL_OM" & vbCrLf & _
            '    " WHERE BUSINESS_UNIT = '" & sBU & "'" & vbCrLf
            Dim strSQLString = "select ISA_CUST_PREFIX " & vbCrLf & _
                 " FROM sysadm8.PS_ISA_ENTERPRISE" & vbCrLf & _
                 " WHERE ISA_BUSINESS_UNIT = '" & sBU & "' "


            Dim dtrPrefixReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
            Try
                If dtrPrefixReader.Read() Then
                    strSitePrefix = dtrPrefixReader("ISA_CUST_PREFIX")
                Else
                    strSitePrefix = "XXX"
                End If
                dtrPrefixReader.Close()

            Catch objException As Exception
                Try
                    dtrPrefixReader.Close()
                Catch ex As Exception

                End Try
                strSitePrefix = "XXX"
                'sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLString)
                'currentApp.Response.Redirect("DBErrorPage.aspx") 
            End Try

            Return strSitePrefix

        End Function

        Public Sub AddLineDetailsForOrder(iLineNbr As Integer, decQtyReq As Decimal, sStockType As String, sItemChgCd As String, _
                             decUnitPrice As Decimal, sInvItemID As String, bDeleteItem As Boolean)

            Dim oLineDetails As New OrderLineDetails(iLineNbr, sStockType, decQtyReq, sItemChgCd, decUnitPrice, sInvItemID, bDeleteItem)

            m_oLineDetails.Add(oLineDetails)
        End Sub

        Public Class OrderLineDetails

            Public Sub New(iLineNbr As Integer, sCurrLineStatus As String, decQtyReq As Decimal, decUnitPrice As Decimal, _
                           dtCurrDueDt As DateTime, dtNewDueDt As DateTime, sInvItemID As String, sStockType As String, bDeleteItem As Boolean)
                m_iLineNbr = iLineNbr
                m_sCurrLineStatus = sCurrLineStatus
                m_decQtyReq = decQtyReq
                m_decUnitPrice = decUnitPrice
                m_dtCurrDueDt = dtCurrDueDt
                m_dtNewDueDt = dtNewDueDt
                m_sStockType = sStockType
                m_bDeleteItem = bDeleteItem

                m_sItemChgCd = ""
            End Sub

            Public Sub New(iLineNbr As Integer, sStockType As String, decQtyReq As Decimal, sItemChgCd As String, decUnitPrice As Decimal, _
                           sInvItemID As String, bDeleteItem As Boolean)

                m_iLineNbr = iLineNbr
                m_decQtyReq = decQtyReq
                m_decUnitPrice = decUnitPrice
                m_sStockType = sStockType
                m_sItemChgCd = sItemChgCd
                m_bDeleteItem = bDeleteItem

                m_sCurrLineStatus = ""
                m_dtCurrDueDt = Now ' just to give a valid value
                m_dtNewDueDt = m_dtCurrDueDt ' just to give a valid value
            End Sub

            Private m_iLineNbr As Integer
            Public ReadOnly Property LineNbr As Integer
                Get
                    Return m_iLineNbr
                End Get
            End Property

            Private m_sStockType As String
            Public ReadOnly Property StockType As String
                Get
                    Return m_sStockType
                End Get
            End Property

            Private m_sCurrLineStatus As String
            Public ReadOnly Property CurrLineStatus As String
                Get
                    Return m_sCurrLineStatus
                End Get
            End Property

            Private m_sItemChgCd As String
            Public ReadOnly Property ItemChgCd As String
                Get
                    Return m_sItemChgCd
                End Get
            End Property

            Private m_decQtyReq As Decimal
            Public Property QtyReq As Decimal
                Get
                    Return m_decQtyReq
                End Get
                Set(value As Decimal)
                    m_decQtyReq = value
                End Set
            End Property

            Private m_decUnitPrice As Decimal
            Public ReadOnly Property UnitPrice As Decimal
                Get
                    Return m_decUnitPrice
                End Get
            End Property

            Private m_dtCurrDueDt As DateTime
            Public ReadOnly Property CurrDueDt As DateTime
                Get
                    Return m_dtCurrDueDt
                End Get
            End Property

            Private m_dtNewDueDt As DateTime
            Public ReadOnly Property NewDueDt As DateTime
                Get
                    Return m_dtNewDueDt
                End Get
            End Property

            Private m_sInvItemID As String
            Public ReadOnly Property InvItemID As String
                Get
                    Return m_sInvItemID
                End Get
            End Property

            Private m_bDeleteItem As Boolean = False
            Public ReadOnly Property DeleteItem As Boolean
                Get
                    Return m_bDeleteItem
                End Get
            End Property
        End Class

    End Class

    Public Class clsUserTbl
        Public Const ActiveStatus_Active As String = "A" ' the user's status is active
        Public Const ActiveStatus_FailedLogin As String = "F" ' the user's status is that the last time they tried to log in, they failed 3 attempts at the password so they're temporarily inactive until the help desk activates them again
        Public Const ActiveStatus_Inactive As String = "I" ' the user's status is inactive

        Private intUniqueUserID As String
        Public ReadOnly Property UniqueUserID() As Integer
            Get
                Return intUniqueUserID
            End Get
        End Property

        Private strFirstNameSrch As String
        Public ReadOnly Property FirstNameSrch() As String
            Get
                Return strFirstNameSrch
            End Get
        End Property

        Private strLastNameSrch As String
        Public ReadOnly Property LastNameSrch() As String
            Get
                Return strLastNameSrch
            End Get
        End Property

        Private strPasswordEncr As String
        Public ReadOnly Property PasswordEncr() As String
            Get
                Return strPasswordEncr
            End Get
        End Property

        Private strBusinessUnit As String
        Public ReadOnly Property BusinessUnit() As String
            Get
                Return strBusinessUnit
            End Get
        End Property

        Private strEmployeeName As String
        Public ReadOnly Property EmployeeName() As String
            Get
                Return strEmployeeName
            End Get
        End Property

        Private strPhoneNum As String
        Public ReadOnly Property PhoneNum() As String
            Get
                Return strPhoneNum
            End Get
        End Property

        Private strEmployeeEmail As String
        Public ReadOnly Property EmployeeEmail() As String
            Get
                Return strEmployeeEmail
            End Get
        End Property

        Private strEmployeeActyp As String
        Public ReadOnly Property EmployeeActyp() As String
            Get
                Return strEmployeeActyp
            End Get
        End Property

        Private strIOLAppEmpID As String
        Public ReadOnly Property IOLAppEmpID() As String
            Get
                Return strIOLAppEmpID
            End Get
        End Property

        Private decIOLAppLimit As Decimal
        Public ReadOnly Property IOLAppLimit() As String
            Get
                Return decIOLAppLimit
            End Get
        End Property

        Private strCustSrvFlag As String
        Public ReadOnly Property CustSrvFlag() As String
            Get
                Return strCustSrvFlag
            End Get
        End Property

        Private strSDICUSTFlag As String
        Public ReadOnly Property SDICUSTFlag() As String
            Get
                Return strSDICUSTFlag
            End Get
        End Property

        Private strTrackUserName As String
        Public ReadOnly Property TrackUserName As String
            Get
                Return strTrackUserName
            End Get
        End Property

        Private strTrackUserPassword As String
        Public ReadOnly Property TrackUserPassword As String
            Get
                Return strTrackUserPassword
            End Get
        End Property

        Private strTrackUserGUID As String
        Public ReadOnly Property TrackUserGUID As String
            Get
                Return strTrackUserGUID
            End Get
        End Property

        Private strTrackToDate As String
        Public ReadOnly Property TrackToDate As String
            Get
                Return strTrackToDate
            End Get
        End Property

        Public Sub New(ByVal Employee_ID As String, ByVal Business_unit As String)
            Dim strSQLstring As String

            strSQLstring = "SELECT A.ISA_USER_ID, A.FIRST_NAME_SRCH," & vbCrLf & _
                            " A.LAST_NAME_SRCH," & vbCrLf & _
                            " A.ISA_PASSWORD_ENCR," & vbCrLf & _
                            " A.BUSINESS_UNIT," & vbCrLf & _
                            " A.ISA_EMPLOYEE_NAME," & vbCrLf & _
                            " A.ISA_SDI_EMPLOYEE," & vbCrLf & _
                            " A.PHONE_NUM," & vbCrLf & _
                            " A.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                            " A.ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                            " B.ISA_IOL_APR_EMP_ID," & vbCrLf & _
                            " B.ISA_IOL_APR_LIMIT," & vbCrLf & _
                            " C.ISA_TRACK_USR_NAME," & vbCrLf & _
                            " C.ISA_TRACK_USR_PSSW," & vbCrLf & _
                            " C.ISA_TRACK_USR_GUID," & vbCrLf & _
                            " C.ISA_TRACK_TO_DATE," & vbCrLf & _
                            " A.ISA_CUST_SERV_FLG" & vbCrLf & _
                            " FROM SDIX_USERS_TBL A," & vbCrLf & _
                            " SDIX_USERS_APPRV B, SDIX_SDITRACK_USERS_TBL C " & vbCrLf & _
                            " WHERE A.ISA_EMPLOYEE_ID = '" & Employee_ID & "'" & vbCrLf & _
                            " AND A.ACTIVE_STATUS = 'A'" & vbCrLf
            If Not Business_unit = "" Then
                strSQLstring = strSQLstring & " AND A.BUSINESS_UNIT = '" & Business_unit & "'" & vbCrLf
            End If
            strSQLstring = strSQLstring & _
                        " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)" & vbCrLf & _
                        " AND A.ISA_EMPLOYEE_ID = B.ISA_EMPLOYEE_ID(+)" & vbCrLf & _
                        " AND A.BUSINESS_UNIT = C.BUSINESS_UNIT(+)" & vbCrLf & _
                        " AND A.ISA_EMPLOYEE_ID = C.ISA_EMPLOYEE_ID(+)"

            Dim objReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
            If objReader.Read() Then
                intUniqueUserID = objReader.Item("ISA_USER_ID")
                strFirstNameSrch = objReader.Item("FIRST_NAME_SRCH")
                strLastNameSrch = objReader.Item("LAST_NAME_SRCH")
                strPasswordEncr = objReader.Item("ISA_PASSWORD_ENCR")
                strBusinessUnit = objReader.Item("BUSINESS_UNIT")
                strEmployeeName = objReader.Item("ISA_EMPLOYEE_NAME")
                strPhoneNum = objReader.Item("PHONE_NUM")
                strEmployeeEmail = objReader.Item("ISA_EMPLOYEE_EMAIL")
                strEmployeeActyp = objReader.Item("ISA_EMPLOYEE_ACTYP")
                If IsDBNull(objReader.Item("ISA_IOL_APR_EMP_ID")) Then
                    strIOLAppEmpID = "NOAPPRVR"
                Else
                    strIOLAppEmpID = objReader.Item("ISA_IOL_APR_EMP_ID")
                End If
                If IsDBNull(objReader.Item("ISA_IOL_APR_LIMIT")) Then
                    decIOLAppLimit = 1000000
                Else
                    decIOLAppLimit = objReader.Item("ISA_IOL_APR_LIMIT")
                End If
                strCustSrvFlag = objReader.Item("ISA_CUST_SERV_FLG")
                strSDICUSTFlag = objReader.Item("ISA_SDI_EMPLOYEE")
                strTrackUserName = GetItem(objReader, "ISA_TRACK_USR_NAME")
                strTrackUserPassword = GetItem(objReader, "ISA_TRACK_USR_PSSW")
                strTrackUserGUID = GetItem(objReader, "ISA_TRACK_USR_GUID")
                strTrackToDate = GetItem(objReader, "ISA_TRACK_TO_DATE")
            End If
            objReader.Close()
        End Sub

        Private Function GetItem(objReader As OleDbDataReader, strColName As String) As String
            Dim sRetVal As String = ""
            Try
                If Not IsDBNull(objReader.Item(strColName)) Then
                    sRetVal = objReader.Item(strColName)
                    If sRetVal Is Nothing Then
                        sRetVal = ""
                    Else
                        If Trim(sRetVal) = "" Then
                            sRetVal = ""
                        Else
                            sRetVal = Trim(sRetVal)
                        End If
                    End If
                Else
                    sRetVal = ""
                End If
            Catch ex As Exception
                sRetVal = ""
            End Try

            Return sRetVal
        End Function
    End Class

    Public Class clsEnterprise
        Private strCompanyID As String
        Public ReadOnly Property CompanyID() As String
            Get
                Return strCompanyID
            End Get
        End Property

        Private strProductviewID As String
        Public ReadOnly Property ProductviewID() As String
            Get
                Return strProductviewID
            End Get
        End Property

        Private intLastItemID As Int32
        Public ReadOnly Property LastItemID() As Int32
            Get
                Return intLastItemID
            End Get
        End Property

        Private intItemIDLen As Integer
        Public ReadOnly Property ItemIDLen() As Int32
            Get
                Return intItemIDLen
            End Get
        End Property

        Private intItemIDMaxLen As Integer
        Public ReadOnly Property ItemIDMaxLen() As Int32
            Get
                Return intItemIDMaxLen
            End Get
        End Property

        Private strItemMode As String
        Public ReadOnly Property ItemMode() As String
            Get
                Return strItemMode
            End Get
        End Property

        Private strSiteEmail As String
        Public ReadOnly Property SiteEmail() As String
            Get
                Return strSiteEmail
            End Get
        End Property

        Private strNONSKREQEmail As String
        Public ReadOnly Property NONSKREQEmail() As String
            Get
                Return strNONSKREQEmail
            End Get
        End Property

        Private strItemAddEmail As String
        Public ReadOnly Property ItemAddEmail() As String
            Get
                Return strItemAddEmail
            End Get
        End Property

        Private strItemAddPrinter As String
        Public ReadOnly Property ItemAddPrinter() As String
            Get
                Return strItemAddPrinter
            End Get
        End Property

        Private strLastUPDOprid As String
        Public ReadOnly Property LastUPDOprid() As String
            Get
                Return strLastUPDOprid
            End Get
        End Property

        Private strOrdApprType As String
        Public ReadOnly Property OrdApprType() As String
            Get
                Return strOrdApprType
            End Get
        End Property

        Private strOrdBudgetFlg As String
        Public ReadOnly Property OrdBudgetFlg() As String
            Get
                Return strOrdBudgetFlg
            End Get
        End Property

        Private strApprNSTKFlag As String
        Public ReadOnly Property ApprNSTKFlag() As String
            Get
                Return strApprNSTKFlag
            End Get
        End Property

        Private strReceivingDate As String
        Public ReadOnly Property ReceivingDate() As String
            Get
                Return strReceivingDate
            End Get
        End Property

        Private strReceivingType As String
        Public ReadOnly Property ReceivingType() As String
            Get
                Return strReceivingType
            End Get
        End Property


        Private strCustPrfxFlag As String
        Public ReadOnly Property CustPrfxFlag() As String
            Get
                Return strCustPrfxFlag
            End Get
        End Property

        Private strCustPrefix As String
        Public ReadOnly Property CustPrefix() As String
            Get
                Return strCustPrefix
            End Get
        End Property

        Private strShopCartPage As String
        Public ReadOnly Property ShopCartPage() As String
            Get
                Return strShopCartPage
            End Get
        End Property

        Private strLPPFlag As String
        Public ReadOnly Property LPPFlag() As String
            Get
                Return strLPPFlag
            End Get
        End Property

        Private strShipToFlag As String
        Public ReadOnly Property ShipToFlag() As String
            Get
                Return strShipToFlag
            End Get
        End Property

        Private strTaxFlag As String
        Public ReadOnly Property TaxFlag() As String
            Get
                Return strTaxFlag
            End Get
        End Property

        Private strISOLPunchout As String
        Public ReadOnly Property ISOLPunchout() As String
            Get
                Return strISOLPunchout
            End Get
        End Property

        Private strValidateWorkorder As String
        Public ReadOnly Property ValidateWorkorder() As String
            Get
                Return strValidateWorkorder
            End Get
        End Property

        Private strRfqOnlySite As String
        Public ReadOnly Property RfqOnlySite() As String
            Get
                Return strRfqOnlySite
            End Get
        End Property

        ' ISA_CUSTINT_APPRVL
        Private strIsaCustintApprv As String
        Public ReadOnly Property IsaCustintApprv() As String
            Get
                Return strIsaCustintApprv
            End Get
        End Property

        Private strBypsReqstrApprv As String
        Public ReadOnly Property BypsReqstrApprv() As String
            Get
                Return strBypsReqstrApprv
            End Get
        End Property

        Private strDeptIdN1 As String
        Public ReadOnly Property DeptIdN() As String
            Get
                Return strDeptIdN1
            End Get
        End Property

        Private strRcvBarCodeDymo As String
        Public ReadOnly Property RcvBarCodeDymo() As String
            Get
                Return strRcvBarCodeDymo
            End Get
        End Property

        Private strIsaCartTaxFlag As String
        Public ReadOnly Property CartTaxExemptFlag() As String
            Get
                Return strIsaCartTaxFlag
            End Get
        End Property

        Private strIsaTreeHold As String
        Public ReadOnly Property CatlgTreeHoldFlag() As String
            Get
                Return strIsaTreeHold
            End Get
        End Property

        Private strOroPunchChk As String
        Public ReadOnly Property OROPunchChkFlag() As String
            Get
                Return strOroPunchChk
            End Get
        End Property

        Private strMobilePicking As String
        Public ReadOnly Property MobilePicking() As String
            Get
                Return strMobilePicking
            End Get
        End Property

        Private strMobilePutaway As String
        Public ReadOnly Property MobilePutaway() As String
            Get
                Return strMobilePutaway
            End Get
        End Property

        Private strMobileIssuing As String
        Public ReadOnly Property MobileIssuing() As String
            Get
                Return strMobileIssuing
            End Get
        End Property

        Private intPWDays As Integer
        Public ReadOnly Property PWDays() As Integer
            Get
                Return intPWDays
            End Get
        End Property
        'strIOH = objReader.Item("ISA_USE_ORO_IOH")
        Private strIOH As String
        Public ReadOnly Property IOH() As String
            Get
                Return strIOH
            End Get
        End Property

        Private strCustName As String
        Public ReadOnly Property CustName() As String
            Get
                Return strCustName
            End Get
        End Property

        Private strBU As String
        Public ReadOnly Property BusinesUnit() As String
            Get
                Return strBU
            End Get
        End Property

        Private strCustid As String
        Public ReadOnly Property CustID() As String
            Get
                Return strCustid
            End Get
        End Property

        Private strSitePrinter As String
        Public ReadOnly Property SitePrinter() As String
            Get
                Return strSitePrinter
            End Get
        End Property

        Private strTrackDBType As String
        Public ReadOnly Property TrackDBType As String
            Get
                Return strTrackDBType
            End Get
        End Property

        Private strTrackDBGUID As String
        Public ReadOnly Property TrackDBGUID As String
            Get
                Return strTrackDBGUID
            End Get
        End Property

        Private strTrackDBUser As String
        Public ReadOnly Property TrackDBUser As String
            Get
                Return strTrackDBUser
            End Get
        End Property

        Private strTrackDBPassword As String
        Public ReadOnly Property TrackDBPassword As String
            Get
                Return strTrackDBPassword
            End Get
        End Property

        Private strTrackDBCust As String
        Public ReadOnly Property TrackDBCust As String
            Get
                Return strTrackDBCust
            End Get
        End Property

        Private m_bOROTreatedLikeSTK As Boolean
        Public ReadOnly Property OROTreatedLikeSTK As Boolean
            Get
                Return m_bOROTreatedLikeSTK
            End Get
        End Property

        Private strIsaKitPrint As String
        Public ReadOnly Property IsaKitPrint As String
            Get
                Return strIsaKitPrint
            End Get
        End Property

        Private strManOrdNo As String
        Public ReadOnly Property ManOrdNo As String
            Get
                Return strManOrdNo
            End Get
        End Property
        Private strOrdLnRenum As String
        Public ReadOnly Property OrdLnRenum As String
            Get
                Return strOrdLnRenum
            End Get
        End Property
        Public Sub New(ByVal BusinessUnit As String)
            Dim strSQLstring As String = ""

            strSQLstring = "SELECT A.*, B.NAME1" & vbCrLf

            strSQLstring = strSQLstring & " FROM sysadm8.PS_ISA_ENTERPRISE A, sysadm8.PS_CUSTOMER B" & vbCrLf & _
                    " WHERE A.ISA_BUSINESS_UNIT = '" & BusinessUnit & "'" & vbCrLf & _
                    " AND A.SETID = B.SETID(+)" & vbCrLf & _
                    " AND A.CUST_ID = B.CUST_ID(+)" & vbCrLf


            Dim objReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
            If objReader.Read() Then
                strCompanyID = objReader.Item("ISA_COMPANY_ID")
                strBU = objReader.Item("ISA_BUSINESS_UNIT")
                strCustid = objReader.Item("CUST_ID")
                strProductviewID = objReader.Item("ISA_CPLUS_PRODVIEW")
                intLastItemID = objReader.Item("ISA_LASTITEMID")
                intItemIDLen = objReader.Item("ISA_ITEMID_LEN")
                intItemIDMaxLen = objReader.Item("ISA_TOTAL_FLD_SIZE")
                strItemAddEmail = objReader.Item("ISA_ITEMADD_EMAIL")
                strSiteEmail = objReader.Item("ISA_SITE_EMAIL")
                strNONSKREQEmail = objReader.Item("ISA_NONSKREQ_EMAIL")
                strSitePrinter = objReader.Item("ISA_SITE_PRINTER")
                strItemAddPrinter = objReader.Item("ISA_ITMADD_PRINTER")
                strLastUPDOprid = objReader.Item("LASTUPDOPRID")
                strItemMode = objReader.Item("ISA_ITEMID_MODE")
                strOrdApprType = objReader.Item("ISA_ORD_APPR_TYPE")
                strOrdBudgetFlg = objReader.Item("ISA_ORD_BUDGET_FLG")
                strApprNSTKFlag = objReader.Item("ISA_APPR_NSTK_FLAG")
                If IsDBNull(objReader.Item("ISA_RECEIVING_DATE")) Then
                    strReceivingDate = "NO"
                ElseIf Trim(objReader.Item("ISA_RECEIVING_DATE")) = "" Then
                    strReceivingDate = "NO"
                Else
                    strReceivingDate = objReader.Item("ISA_RECEIVING_DATE")
                End If
                If IsDBNull(objReader.Item("ISA_RECEIVING_TYPE")) Then
                    strReceivingType = "R"
                Else
                    strReceivingType = objReader.Item("ISA_RECEIVING_TYPE")
                End If
                If Trim(objReader.Item("ISA_SHOPCART_PAGE")) = "" Then
                    strShopCartPage = "ShoppingCart.aspx"
                Else
                    strShopCartPage = objReader.Item("ISA_SHOPCART_PAGE")
                End If
                strLPPFlag = objReader.Item("ISA_LPP_FLAG")
                strTaxFlag = objReader.Item("ISA_ISOL_TAX_FLAG")
                strCustPrfxFlag = objReader.Item("ISA_CUST_PRFX_FLAG")
                strCustPrefix = objReader.Item("ISA_CUST_PREFIX")
                strISOLPunchout = objReader.Item("ISA_ISOL_PUNCHOUT")
                strMobilePicking = objReader.Item("ISA_MOBILE_PICKING")
                strMobilePutaway = objReader.Item("ISA_MOBILE_PUTAWAY")
                strMobileIssuing = objReader.Item("ISA_MOBILE_ISSUE")
                intPWDays = objReader.Item("ISA_PW_EXPIRE_DAYS")
                strIOH = objReader.Item("ISA_USE_ORO_IOH")
                strShipToFlag = objReader.Item("SHIP_TO_FLG")
                strCustName = objReader.Item("NAME1")
                strTrackDBType = GetItem(objReader, "ISA_TRACK_DB_TYPE")
                strTrackDBGUID = GetItem(objReader, "ISA_TRACK_DB_GUID")
                strTrackDBUser = GetItem(objReader, "ISA_TRACK_DB_USR")
                strTrackDBPassword = GetItem(objReader, "ISA_TRACK_DB_PSSW")
                strTrackDBCust = GetItem(objReader, "ISA_TRACK_DB_CUST")
                strIsaKitPrint = GetItem(objReader, "ISA_KIT_PRINTING")
                Try
                    strRcvBarCodeDymo = "N"
                    strRcvBarCodeDymo = objReader.Item("ISA_RCVBARCODE")
                    If strRcvBarCodeDymo Is Nothing Then
                        strRcvBarCodeDymo = "N"
                    Else
                        If Trim(strRcvBarCodeDymo) = "" Then
                            strRcvBarCodeDymo = "N"
                        End If
                    End If
                Catch ex As Exception
                    strRcvBarCodeDymo = "N"
                End Try
                Try
                    strIsaCartTaxFlag = " "
                    strIsaCartTaxFlag = objReader.Item("ISA_CART_TAX_FLAG")
                    If strIsaCartTaxFlag Is Nothing Then
                        strIsaCartTaxFlag = " "
                    Else
                        If Trim(strIsaCartTaxFlag) = "" Then
                            strIsaCartTaxFlag = " "
                        End If
                    End If
                Catch ex As Exception
                    strIsaCartTaxFlag = " "
                End Try
                If Trim(strIsaCartTaxFlag) = "" Then
                    strIsaCartTaxFlag = " "
                End If
                Try
                    strIsaTreeHold = " "
                    strIsaTreeHold = objReader.Item("ISA_TREE_HOLD")
                    If strIsaTreeHold Is Nothing Then
                        strIsaTreeHold = " "
                    Else
                        If Trim(strIsaTreeHold) = "" Then
                            strIsaTreeHold = " "
                        End If
                    End If
                Catch ex As Exception
                    strIsaTreeHold = " "
                End Try
                If Trim(strIsaTreeHold) = "" Then
                    strIsaTreeHold = " "
                End If

                Try
                    strOroPunchChk = " "
                    strOroPunchChk = objReader.Item("ORO_PUNCH_CHK")
                    If strOroPunchChk Is Nothing Then
                        strOroPunchChk = " "
                    Else
                        If Trim(strOroPunchChk) = "" Then
                            strOroPunchChk = " "
                        End If
                    End If
                Catch ex As Exception
                    strOroPunchChk = " "
                End Try
                If Trim(strOroPunchChk) = "" Then
                    strOroPunchChk = " "
                End If

                Try
                    strValidateWorkorder = "N"
                    strValidateWorkorder = objReader.Item("ISA_VALIDATE_WO")
                    If strValidateWorkorder Is Nothing Then
                        strValidateWorkorder = "N"
                    Else
                        If Trim(strValidateWorkorder) = "" Then
                            strValidateWorkorder = "N"
                        End If
                    End If
                Catch ex As Exception
                    strValidateWorkorder = "N"
                End Try
                strRfqOnlySite = "N"
                Try
                    strRfqOnlySite = objReader.Item("ISA_RFQ_ONLY")
                    If strRfqOnlySite Is Nothing Then
                        strRfqOnlySite = "N"
                    Else
                        If Trim(strRfqOnlySite) = "" Then
                            strRfqOnlySite = "N"
                        End If
                    End If
                Catch ex As Exception
                    strRfqOnlySite = "N"
                End Try

                strIsaCustintApprv = "N"
                Try
                    strIsaCustintApprv = objReader.Item("ISA_CUSTINT_APPRVL")
                    If strIsaCustintApprv Is Nothing Then
                        strIsaCustintApprv = "N"
                    Else
                        If Trim(strIsaCustintApprv) = "" Then
                            strIsaCustintApprv = "N"
                        End If
                    End If
                Catch ex As Exception
                    strIsaCustintApprv = "N"
                End Try

                strBypsReqstrApprv = "N"
                Try
                    strBypsReqstrApprv = objReader.Item("ISA_BYP_RQSTR_APPR")
                    If strBypsReqstrApprv Is Nothing Then
                        strBypsReqstrApprv = "N"
                    Else
                        If Trim(strBypsReqstrApprv) = "" Then
                            strBypsReqstrApprv = "N"
                        End If
                    End If
                Catch ex As Exception
                    strBypsReqstrApprv = "N"
                End Try

                strDeptIdN1 = "0"
                Try
                    strDeptIdN1 = objReader.Item("DEPTID")
                    If strDeptIdN1 Is Nothing Then
                        strDeptIdN1 = "0"
                    Else
                        If Trim(strDeptIdN1) = "" Then
                            strDeptIdN1 = "0"
                        End If
                    End If
                Catch ex As Exception
                    strDeptIdN1 = "0"
                End Try

                '---------------------------------------------------------------
                Try
                    strManOrdNo = " "
                    strManOrdNo = objReader.Item("ISA_MAN_ORDER_NO")
                    If strManOrdNo Is Nothing Then
                        strManOrdNo = " "
                    Else
                        If Trim(strManOrdNo) = "" Or Trim(strManOrdNo) = "N" Or Trim(strManOrdNo) = "n" Then
                            strManOrdNo = " "
                        End If
                    End If
                Catch ex As Exception
                    strManOrdNo = " "
                End Try
                If Trim(strManOrdNo) = "" Then
                    strManOrdNo = " "
                End If
                Try
                    strOrdLnRenum = " "
                    strOrdLnRenum = objReader.Item("ISA_ORDLNRENUM")
                    If strOrdLnRenum Is Nothing Then
                        strOrdLnRenum = " "
                    Else
                        If Trim(strOrdLnRenum) = "" Then
                            strOrdLnRenum = " "
                        End If
                    End If
                Catch ex As Exception
                    strOrdLnRenum = " "
                End Try
                If Trim(strOrdLnRenum) = "" Then
                    strOrdLnRenum = " "
                End If

                Dim sOROTreatedLikeSTK As String = GetItem(objReader, "ISA_ORO_AS_STOCK")
                SetFlag_OROTreatedLikeSTK(sOROTreatedLikeSTK)
            End If
            objReader.Close()
        End Sub
        Private Function GetItem(objReader As OleDbDataReader, strColName As String)
            Dim sRetVal As String = ""
            Try
                If Not IsDBNull(objReader.Item(strColName)) Then
                    sRetVal = objReader.Item(strColName)
                    If sRetVal Is Nothing Then
                        sRetVal = ""
                    Else
                        If Trim(sRetVal) = "" Then
                            sRetVal = ""
                        Else
                            sRetVal = Trim(sRetVal)
                        End If
                    End If
                Else
                    sRetVal = ""
                End If
            Catch ex As Exception
                sRetVal = ""
            End Try

            Return sRetVal
        End Function
        Private Sub SetFlag_OROTreatedLikeSTK(sOROTreatedLikeSTK As String)
            If sOROTreatedLikeSTK.ToUpper = "Y" Then
                m_bOROTreatedLikeSTK = True
            Else
                m_bOROTreatedLikeSTK = False
            End If
        End Sub
    End Class

    Public Class OrderLineDetails

        Public Sub New(iLineNbr As Integer, sCurrLineStatus As String, decQtyReq As Decimal, decUnitPrice As Decimal, _
                       dtCurrDueDt As DateTime, dtNewDueDt As DateTime, sInvItemID As String, sStockType As String, bDeleteItem As Boolean)
            m_iLineNbr = iLineNbr
            m_sCurrLineStatus = sCurrLineStatus
            m_decQtyReq = decQtyReq
            m_decUnitPrice = decUnitPrice
            m_dtCurrDueDt = dtCurrDueDt
            m_dtNewDueDt = dtNewDueDt
            m_sStockType = sStockType
            m_bDeleteItem = bDeleteItem

            m_sItemChgCd = ""
        End Sub

        Public Sub New(iLineNbr As Integer, sStockType As String, decQtyReq As Decimal, sItemChgCd As String, decUnitPrice As Decimal, _
                       sInvItemID As String, bDeleteItem As Boolean)

            m_iLineNbr = iLineNbr
            m_decQtyReq = decQtyReq
            m_decUnitPrice = decUnitPrice
            m_sStockType = sStockType
            m_sItemChgCd = sItemChgCd
            m_bDeleteItem = bDeleteItem

            m_sCurrLineStatus = ""
            m_dtCurrDueDt = Now ' just to give a valid value
            m_dtNewDueDt = m_dtCurrDueDt ' just to give a valid value
        End Sub

        Private m_iLineNbr As Integer
        Public ReadOnly Property LineNbr As Integer
            Get
                Return m_iLineNbr
            End Get
        End Property

        Private m_sStockType As String
        Public ReadOnly Property StockType As String
            Get
                Return m_sStockType
            End Get
        End Property

        Private m_sCurrLineStatus As String
        Public ReadOnly Property CurrLineStatus As String
            Get
                Return m_sCurrLineStatus
            End Get
        End Property

        Private m_sItemChgCd As String
        Public ReadOnly Property ItemChgCd As String
            Get
                Return m_sItemChgCd
            End Get
        End Property

        Private m_decQtyReq As Decimal
        Public Property QtyReq As Decimal
            Get
                Return m_decQtyReq
            End Get
            Set(value As Decimal)
                m_decQtyReq = value
            End Set
        End Property

        Private m_decUnitPrice As Decimal
        Public ReadOnly Property UnitPrice As Decimal
            Get
                Return m_decUnitPrice
            End Get
        End Property

        Private m_dtCurrDueDt As DateTime
        Public ReadOnly Property CurrDueDt As DateTime
            Get
                Return m_dtCurrDueDt
            End Get
        End Property

        Private m_dtNewDueDt As DateTime
        Public ReadOnly Property NewDueDt As DateTime
            Get
                Return m_dtNewDueDt
            End Get
        End Property

        Private m_sInvItemID As String
        Public ReadOnly Property InvItemID As String
            Get
                Return m_sInvItemID
            End Get
        End Property

        Private m_bDeleteItem As Boolean = False
        Public ReadOnly Property DeleteItem As Boolean
            Get
                Return m_bDeleteItem
            End Get
        End Property
    End Class

    Public Class ApprovalHistory
        Public Enum ApprHistType As Integer
            QuoteApproval 'Q
            OrderApproval 'W
            BudgetaryApproval 'B
            DepartmentApproval 'D
        End Enum
    End Class

    Public Class sdiCommon

        Public Const APP_CONNECTION_STRING_ORA As String = "oraCNstring"
        Public Const DEFAULT_CURRENCY_CODE As String = "USD"
        Public Const DEFAULT_CURRENCY_SYMBOL As String = "$"

    End Class

End Module

