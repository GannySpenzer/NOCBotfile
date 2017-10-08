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
    Sub Main()
        GenerateApproverMail()
    End Sub

    Private Sub GenerateApproverMail()
        Try
            Dim ApprDT As DataTable = GetApproverMailList()
            For Each row As DataRow In ApprDT.Rows
                Dim strreqID As String = ""
                Dim strAgent As String = ""
                Dim strBU As String = ""
                Dim oApprovalResults As New ApprovalResults
                CheckSTKLimits(strBU, strAgent, strreqID, "", "B")
                If oApprovalResults.IsMoreApproversNeeded Then
                    If oApprovalResults.OrderExceededLimit Then
                        buildNotifyApprover(strreqID, strAgent, strBU, oApprovalResults.NextOrderApprover, oApprovalResults.NewOrderHeaderStatus)
                    Else
                        UpdateOrderStatus(strBU, strreqID)
                        'ElseIf oApprovalResults.IsAnyChargeCodeExceededLimit Then
                        '    For I = 0 To oApprovalResults.BudgetChargeCodesCount - 1
                        '        If oApprovalResults.BudgetExceededLimit(I) Then
                        '            buildNotifyApprover(strreqID, strAgent, strBU, oApprovalResults.NextBudgetApprover(I), oApprovalResults.NewBudgetHeaderStatus(I))
                        '        End If
                        '    Next
                    End If
                End If
            Next
        Catch ex As Exception

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
        Dim dt As DataTable
        Try
            Dim StrAppr As String = "SELECT DISTINCT ORDER_NO,BUSINESS_UNIT_OM,ISA_USER4 FROM SYSADM8.PS_ISA_ORD_INTF_LN WHERE ISA_LINE_STATUS='QTW' AND OPRID_ENTERED_BY = OPRID_APPROVED_BY AND  " & vbCrLf & _
                " BUSINESS_UNIT_OM IN (SELECT ISA_BUSINESS_UNIT FROM SYSADM8.PS_ISA_ENTERPRISE WHERE UPPER(ISA_BYP_RQSTR_APPR)='Y')"
            Dim ds As DataSet = ORDBData.GetAdapterSpc(StrAppr)
            dt = ds.Tables(0)
            Return dt
        Catch ex As Exception
        End Try
    End Function

    Public Sub buildNotifyApprover(ByVal strreqID As String, ByVal strAgent As String, ByVal strBU As String, ByVal strAppUserid As String, ByVal strHldStatus As String)
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
        Dim itemsid As Integer

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

        strhref = ConfigurationSettings.AppSettings("SiteURL11") & "approveorder.aspx?fer=" & streOrdnum & "&op=" & streApper & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"
        Dim StrResult As String = String.Empty

        Dim Mailer As MailMessage = New MailMessage
        Mailer.From = "SDIExchange@SDI.com"
        Mailer.CC = ""
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
            SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, Mailer.CC, Mailer.Bcc, Mailer.Body, "Request to Approver", MailAttachmentName, MailAttachmentbytes.ToArray())
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
        Dim arrParamsOut As ArrayList
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

    Public Sub UpdateOrderStatus(ByVal strbu As String, ByVal strOrderNo As String)
        Try
            Dim StrUpdSts As String = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS='QTC'  WHERE BUSINESS_UNIT_OM='" & strbu & "' AND ORDER_NO='" & strOrderNo & "'"
            Dim rowAffected As Integer = ORDBData.ExecNonQuery(StrUpdSts)
        Catch ex As Exception
        End Try
    End Sub
End Module
