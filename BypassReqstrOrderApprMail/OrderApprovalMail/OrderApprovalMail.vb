Imports System.Data.OleDb
Imports System.Configuration
Imports System.Web.UI
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web.UI.WebControls
Imports SDI.ApplicationLogger

Module OrderApprovalMail

    Private m_logger As appLogger = Nothing

    Dim logpath As String = "C:\Program Files (x86)\SDI\SendEmailQTWBypassReqstr\LOGS\BypassReqstrNonStckEml" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Sub Main()

        '    log path/file
        Dim sLogPath As String = "C:\Program Files (x86)\SDI\SendEmailQTWBypassReqstr\LOGS"
        Try
            sLogPath = My.Settings("logMyPath").ToString.Trim
        Catch ex As Exception

        End Try

        If (sLogPath.Length > 0) Then
            logpath = sLogPath & "\BypassReqstrNonStckEml" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' initialize log
        m_logger = New appLogger(logpath, logLevel)

        ' log verbose
        m_logger.WriteVerboseLog(" Start of Bypass Requestor Non Stock Email Utility process: " & Now())

        SendMailToApprover()

        ' log verbose
        m_logger.WriteVerboseLog(" End of Bypass Requestor Non Stock Email Utility process: " & Now())

    End Sub

    Public Function GetPrice(ByVal OrderID As String) As Decimal
        Dim StrQry As String = "SELECT L.ISA_SELL_PRICE,L.BUSINESS_UNIT_OM,L.QTY_REQUESTED FROM " & vbCrLf & _
            "SYSADM8.PS_ISA_ORD_INTF_LN L WHERE L.ORDER_NO='" & OrderID & "' AND L.INV_ITEM_ID=' '"

        Dim ds As DataSet = ORDBData.GetAdapterSpc(StrQry)
        Dim i As Integer = 0
        Dim OrdTotal As Decimal = 0
        For Each row As DataRow In ds.Tables(0).Rows
            Dim decNetPrice As Decimal = 0
            Dim decVndPrc As Decimal = 0
            Dim decSellPrc As Decimal = 0
            Dim StrItemTyp As String = " "
            Dim strStockTyp As String = " "
            Dim StrBU As String = " "
            Dim QtrReq As Decimal = 0
            Dim ItmPrice As Decimal = 0

            If Not IsDBNull(ds.Tables(0).Rows(i)("ISA_SELL_PRICE")) Then
                Try
                    decSellPrc = CDec(ds.Tables(0).Rows(i)("ISA_SELL_PRICE"))
                Catch ex As Exception
                    decSellPrc = 0
                End Try

            End If

            If Not IsDBNull(ds.Tables(0).Rows(i)("QTY_REQUESTED")) Then
                Try
                    QtrReq = CDec(ds.Tables(0).Rows(i)("QTY_REQUESTED"))
                Catch ex As Exception
                    QtrReq = 0
                End Try

            End If

            ItmPrice = decSellPrc
            OrdTotal = OrdTotal + (ItmPrice * QtrReq)

            i = i + 1
        Next

        Return OrdTotal

    End Function

    Public Sub SendMailToApprover()
        Try
            Dim dt As DataTable = ApprovalOrders.GetQuotedOrders()

            Dim ApprOrd As New ApprovalOrders
            Dim iCnt As Integer = 0
            Dim strOrdersList As String = ""

            If Not dt Is Nothing Then
                If dt.Rows.Count > 0 Then

                    ' log verbose
                    m_logger.WriteVerboseLog(" Total Number of QTW orders to process: " & dt.Rows.Count.ToString & " ------ " & Now())

                    For Each row As DataRow In dt.Rows
                        'If iCnt = 4 Then
                        '    Exit For
                        'End If
                        Dim StrBu As String = row("BUSINESS_UNIT_OM")
                        Dim StrOrderNo As String = row("ORDER_NO")
                        Dim StrUserID As String = row("ISA_EMPLOYEE_ID")
                        Dim StrFinalApprType As String = row("ISA_CUSTINT_APPRVL")
                        Dim strFinalApprValue As String = "QTA"
                        Try
                            If UCase(Trim(StrFinalApprType)) = "Y" Then
                                strFinalApprValue = "QTC"
                            Else
                                strFinalApprValue = "QTA"
                            End If
                        Catch ex As Exception
                            strFinalApprValue = "QTA"
                        End Try
                        Dim decApprvThres As Decimal = 0
                        Try
                            decApprvThres = CType(row("APPRVALTHRESHOLD"), Decimal)
                        Catch ex As Exception
                            decApprvThres = 0
                        End Try
                        Dim decOrderTotal As Decimal = 0
                        'get Order Total value
                        decOrderTotal = GetPrice(StrOrderNo)

                        If decApprvThres > 0 Then
                            If decOrderTotal > decApprvThres Then
                                ApprOrd.SendMail(StrBu, StrOrderNo, StrUserID, strFinalApprValue)
                                If ApprovalOrders.IsNYC(StrBu) Then
                                    m_logger.WriteVerboseLog(" Order Total is more than Threshold. E-mail was not sent for NYC order No: " & StrOrderNo & " ------ " & Now())

                                Else
                                    m_logger.WriteVerboseLog(" Order Total is more than Threshold. E-mail sent for order No: " & StrOrderNo & " ------ " & Now())

                                End If
                               
                            Else
                                'set to QTC or QTA
                                Dim strUpdateLineStatusFinal As String = ""
                                strUpdateLineStatusFinal = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = '" & strFinalApprValue & "', OPRID_APPROVED_BY = 'SDIX_W', APPROVAL_DTTM = SYSDATE " & vbCrLf & _
                                    "WHERE BUSINESS_UNIT_OM = '" & StrBu & "' AND ORDER_NO = '" & StrOrderNo & "' " & vbCrLf & _
                                    "AND ISA_LINE_STATUS = 'QTW'"

                                Dim iRowsAffctd As Integer = 0
                                Try
                                    iRowsAffctd = ORDBData.ExecNonQuery(strUpdateLineStatusFinal, False)

                                    If iRowsAffctd > 0 Then
                                        SDIAuditInsert("PS_ISA_ORD_INTF_LN", StrOrderNo, "ISA_LINE_STATUS", strFinalApprValue, StrBu)

                                    End If

                                Catch ex As Exception

                                End Try
                                m_logger.WriteVerboseLog(" Order Total is Less than Threshold. Order line status is set to: " & strFinalApprValue & ". E-mail was not sent for order No: " & StrOrderNo & " ------ " & Now())
                                'ApprovalOrders.flagOrderAsProcessed(StrOrderNo, StrBu)
                            End If
                        Else
                            ApprOrd.SendMail(StrBu, StrOrderNo, StrUserID, strFinalApprValue)
                            
                            If ApprovalOrders.IsNYC(StrBu) Then
                                m_logger.WriteVerboseLog(" No Threshold. E-mail was not sent for NYC order No: " & StrOrderNo & " ------ " & Now())

                            Else
                                m_logger.WriteVerboseLog(" No Threshold. E-mail was sent for order No: " & StrOrderNo & " ------ " & Now())

                            End If
                        End If

                        iCnt = iCnt + 1
                        If Trim(strOrdersList) = "" Then
                            strOrdersList = StrOrderNo
                        Else
                            strOrdersList = strOrdersList & ", " & StrOrderNo
                        End If
                    Next
                    If Trim(strOrdersList) <> "" Then

                        ' log verbose
                        m_logger.WriteVerboseLog(" List of Orders processed by Bypass Requestor Non Stock Email Utility process: " & Now())
                        m_logger.WriteVerboseLog(strOrdersList)
                        m_logger.WriteVerboseLog("-------------------")

                    End If
                End If
            End If

        Catch ex As Exception
            Dim sMyErrorString As String = String.Empty
            If Not ex Is Nothing Then
                sMyErrorString = ex.ToString
            End If
            sMyErrorString += sMyErrorString & _
                       "Error in Loaner.vb -> UpdateReceivingItems"
            '' "UserID = " & HttpContext.Current.Session("USERID") & ", Business Unit = " & HttpContext.Current.Session("BUSUNIT")
            ErrorMail.SendSDiExchErrorMail(sMyErrorString)
        End Try
    End Sub

    Public Sub SDIAuditInsert(ByVal sTableName As String, ByVal sOrdNum As String, ByVal sColumnChg As String, ByVal sNewValue As String, ByVal sBU As String)
        Try
            Dim strInsertQuery As String = String.Empty
            Dim rowsaffected As Integer = 0

            strInsertQuery = "INSERT INTO ps_isa_SDIXaudit " & vbCrLf & _
                " ( " & vbCrLf & _
                " descr, rcdsrc, table_name " & vbCrLf & _
                ", key_01, key_02, key_03 " & vbCrLf & _
                ", columnchg, newvalue, oldvalue " & vbCrLf & _
                ", oprid, server_name " & vbCrLf & _
                ", dt_timestamp " & vbCrLf & _
                ", business_unit, isa_udf1, isa_udf2, isa_udf3 " & vbCrLf & _
                " ) " & vbCrLf & _
                " VALUES (" & vbCrLf & _
                " 'NSTK Email', 'SDINonStockEmailUtility.exe', '" & sTableName & "' " & vbCrLf & _
                ", '" & sOrdNum & "', ' ', ' ' " & vbCrLf & _
                ", '" & sColumnChg & "', '" & sNewValue & "', ' ' " & vbCrLf & _
                ", 'NSTKpric', 'C2' " & vbCrLf & _
                ", TO_DATE('" & Now.ToString("MM/dd/yyyy HH:mm:ss") & "', 'MM/DD/YYYY HH24:MI:SS') " & vbCrLf & _
                ", '" & sBU & "', ' ', ' ', ' ' " & vbCrLf & _
                " )"

            rowsaffected = ORDBData.ExecNonQuery(strInsertQuery)

        Catch ex As Exception

        End Try
    End Sub

End Module


Public Class ErrorMail
    Public Shared Sub SendSDiExchErrorMail(ByVal strErrorMessage As String, Optional ByVal sSubject As String = "")

        'Gives us a reference to the current asp.net 
        'application executing the method.
        '   Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

        Dim strComeFrom As String = "Unknown"
        Try
            strComeFrom = ConfigurationSettings.AppSettings("serverId").Trim
        Catch ex As Exception
            strComeFrom = "Unknown"
        End Try

        Dim mail As New System.Net.Mail.MailMessage()

        mail.To.Add("webdev@sdi.com")
        mail.To.Add("sdiportalsupport@avasoft.biz")

        mail.From = New System.Net.Mail.MailAddress("SDIExchADMIN@sdi.com", "SDiExchange Admin")

        ' adding server name to the error message
        mail.Body = "Error in SDiExchange  -  " & vbCrLf & strErrorMessage & ". Server Id: " & strComeFrom

        If sSubject.Trim.Length = 0 Then
            sSubject = "Error - SDiExchange"
        End If
        mail.Subject = sSubject

        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()

        Try
            SDIEmailService.EmailUtilityServices("MailandStore", mail.From.ToString(), mail.To.ToString(), mail.Subject, String.Empty, String.Empty, mail.Body, "SDIERRMAIL", MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception
            Dim strErr As String = ex.Message
        End Try

    End Sub
End Class
Public Class ApprovalOrders
    Private m_oApprovalDetails As ApprovalDetails
    Public Shared Function GetQuotedOrders() As DataTable
        Dim dt As DataTable = Nothing
        Try
            Dim StrQry As String = "SELECT DISTINCT A.ORDER_NO, A.BUSINESS_UNIT_OM, A.ISA_EMPLOYEE_ID,B.ISA_CUSTINT_APPRVL " & vbCrLf & _
                " , DECODE(B.APPRVALTHRESHOLD, NULL, 0, B.APPRVALTHRESHOLD) AS APPRVALTHRESHOLD " & vbCrLf & _
                " FROM PS_ISA_ORD_INTF_LN A, PS_ISA_ENTERPRISE B " & vbCrLf & _
                " WHERE A.ISA_LINE_STATUS = 'QTW' AND  A.INV_ITEM_ID = ' ' AND A.OPRID_APPROVED_BY = A.ISA_EMPLOYEE_ID " & vbCrLf & _
                " AND B.BU_STATUS = 1 AND B.ISA_BYP_RQSTR_APPR = 'Y'  AND B.ISA_BUSINESS_UNIT = A.BUSINESS_UNIT_OM" & vbCrLf & _
                                    "   AND NOT EXISTS (" & vbCrLf & _
                                    "                   SELECT 'X'" & vbCrLf & _
                                    "                   FROM PS_ISA_ORDSTAT_EML G" & vbCrLf & _
                                    "                   WHERE A.BUSINESS_UNIT_OM = G.BUSINESS_UNIT_OM" & vbCrLf & _
                                    "                     AND A.ORDER_NO = G.ORDER_NO" & vbCrLf & _
                                    "                     AND G.ISA_LINE_STATUS = 'QTW'" & vbCrLf & _
                                    "                  )" & vbCrLf

            Dim ds As DataSet = ORDBData.GetAdapterSpc(StrQry)
            dt = ds.Tables(0)
            Return dt
        Catch ex As Exception
            Return dt
        End Try
    End Function

    Public Shared Sub flagOrderAsProcessed(ByVal strOrdrNo As String, ByVal strBusUnit As String)

        Dim I As Integer = 0
        Dim strSQLstring As String = ""
        Dim rowsaffected As Integer = 0
        Dim strGetOrder As String = ""
        strGetOrder = "Select BUSINESS_UNIT_OM, ISA_LINE_STATUS, ISA_EMPLOYEE_ID, ORDER_NO,ISA_INTFC_LN FROM SYSADM8.PS_ISA_ORD_INTF_LN WHERE ORDER_NO='" & strOrdrNo & "' AND BUSINESS_UNIT_OM = '" & strBusUnit & "'"
        Dim dsOrder As System.Data.DataSet = New System.Data.DataSet
        Dim strLineNo As String = ""
        Dim strEmpld As String = ""

        dsOrder = ORDBData.GetAdapterSpc(strGetOrder)

        Try
            If Not dsOrder Is Nothing Then
                If dsOrder.Tables(0).Rows.Count > 0 Then

                    For I = 0 To dsOrder.Tables(0).Rows.Count - 1
                        strBusUnit = dsOrder.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM")
                        strEmpld = dsOrder.Tables(0).Rows(I).Item("ISA_EMPLOYEE_ID")
                        strLineNo = dsOrder.Tables(0).Rows(I).Item("ISA_INTFC_LN")

                        strSQLstring = "" & _
                                               "INSERT INTO PS_ISA_ORDSTAT_EML " & vbCrLf & _
                                               "(" & vbCrLf & _
                                               " BUSINESS_UNIT_OM" & vbCrLf & _
                                               ",ORDER_NO " & vbCrLf & _
                                               ",LINE_NBR " & vbCrLf & _
                                               ",ORDER_INT_LINE_NO " & vbCrLf & _
                                               ",DEMAND_LINE_NO " & vbCrLf & _
                                               ",RECEIVER_ID " & vbCrLf & _
                                               ",RECV_LN_NBR " & vbCrLf & _
                                               ",EMPLID " & vbCrLf & _
                                               ",ISA_LINE_STATUS " & vbCrLf & _
                                               ",EMAIL_DATETIME " & vbCrLf & _
                                               ")" & vbCrLf & _
                                               "VALUES " & vbCrLf & _
                                               "(" & vbCrLf & _
                                               " '" & strBusUnit & "' " & vbCrLf & _
                                               ",'" & strOrdrNo & "' " & vbCrLf & _
                                               ",'" & strLineNo & "' " & vbCrLf & _
                                               ",'" & strLineNo & "' " & vbCrLf & _
                                               ",'" & strLineNo & "' " & vbCrLf & _
                                               ",' ' " & vbCrLf & _
                                               ",'0' " & vbCrLf & _
                                               ",'" & strEmpld & "' " & vbCrLf & _
                                               ",'QTW' " & vbCrLf & _
                                               ",SYSDATE " & vbCrLf & _
                                               ")" & vbCrLf & _
                                               ""
                        Try
                            rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                        Catch ex As Exception

                        End Try

                    Next

                End If
            End If
        Catch ex As Exception

        End Try

    End Sub

    Public Sub SendMail(ByVal BU As String, ByVal OrderNo As String, ByVal EnterBY As String, ByVal strFinalApprValue As String)
        Try
            m_oApprovalDetails = New ApprovalDetails(BU, EnterBY, EnterBY, OrderNo)
            Dim oApprovalResults As ApprovalResults
            If CheckLimitsOrder(m_oApprovalDetails, oApprovalResults, IsAEES(m_oApprovalDetails.BU)) Then
                Dim strAppMessage() As String
                Dim bIsNYC As Boolean = False
                If oApprovalResults.OrderExceededLimit Then
                    bIsNYC = IsNYC(BU)
                    If Not bIsNYC Then ' do not send e-mail to NYC (web.config based list)
                        strAppMessage = New String(0) {}
                        strAppMessage(0) = Notifiers.NotifyApprover(m_oApprovalDetails.ReqID, m_oApprovalDetails.ApproverID, _
                            m_oApprovalDetails.BU, oApprovalResults.NextOrderApprover, oApprovalResults.NewOrderHeaderStatus, "")

                    End If
                    
                Else
                    'order limit not exceeded - set to strFinalApprValue (QTC or QTA) - and do not send e-mail
                    SetFinalStatus(OrderNo, strFinalApprValue)
                End If
                flagOrderAsProcessed(OrderNo, BU)
            End If
        Catch ex As Exception
        End Try
    End Sub

    Public Sub SetFinalStatus(ByVal orderid As String, ByVal Linestatus As String)
        Try
            Dim strUpdateQuery As String = String.Empty
            Dim strSelectQuery As String = String.Empty
            Dim ordSts As String = "W"
            Dim ordIdent As String = String.Empty
            Dim oprEnteredBy As String = String.Empty
            Dim ordBu As String = String.Empty
            Dim rowsaffected As Integer = 0
            Dim OrcRdr As OleDb.OleDbDataReader = Nothing
            Dim ordStsL As String = "QTW"

            strSelectQuery = "Select BUSINESS_UNIT_OM, ISA_LINE_STATUS, OPRID_ENTERED_BY, ORDER_NO FROM SYSADM8.PS_ISA_ORD_INTF_LN WHERE ORDER_NO='" & orderid & "'"

            OrcRdr = ORDBData.GetReader(strSelectQuery)
            If OrcRdr.HasRows Then
                While OrcRdr.Read()
                    ordBu = CType(OrcRdr("BUSINESS_UNIT_OM"), String).Trim()
                    oprEnteredBy = CType(OrcRdr("OPRID_ENTERED_BY"), String).Trim()
                    If OrcRdr("ISA_LINE_STATUS").Trim().ToUpper() = "QTW" Then  '  If OrcRdr.GetString(0).Trim().ToUpper() = "QTS" Then
                        strUpdateQuery = " UPDATE SYSADM8.PS_ISA_ORD_INTF_LN" & vbCrLf & _
                                         " SET ISA_LINE_STATUS = '" & Linestatus & "'" & vbCrLf & _
                                         " WHERE ORDER_NO = '" & orderid & "' AND ISA_LINE_STATUS = 'QTW'"
                        rowsaffected = ORDBData.ExecNonQuery(strUpdateQuery)
                        SDIAuditInsert("PS_ISA_ORD_INTF_LN", orderid, "ISA_LINE_STATUS", Linestatus, ordBu)
                        Exit While
                    End If
                End While
            End If

            strUpdateQuery = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD" & vbCrLf & _
                        " SET ORDER_STATUS = '" & ordSts & "'" & vbCrLf & _
                        " WHERE ORDER_NO = '" & orderid & "'"

            rowsaffected = ORDBData.ExecNonQuery(strUpdateQuery)

            SDIAuditInsert("PS_ISA_ORD_INTF_HD", orderid, "ORDER_STATUS", ordSts, ordBu)

        Catch ex As Exception
            ' SendLogger("Error in Quoted Non Stock Email Utility, PriceUpdate", ex, "ERROR")
        End Try
    End Sub

    Private Shared Function CheckLimitsOrder(ByRef oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults, Optional ByVal bIsMultiCurrency As Boolean = False) As Boolean
        Dim bSuccessful As Boolean = False
        Try
            oApprovalResults = New ApprovalResults()
            If bIsMultiCurrency Then
                Const cPriorDaysAgo As Integer = 0 ' prior days (in number) ago
                bSuccessful = MultiCurrencyOrder.CustEmp(oApprovalDetails, _
                    MultiCurrencyOrder.getSiteCurrency(oApprovalDetails.BU).Id, _
                    cPriorDaysAgo, oApprovalResults)
            Else
                bSuccessful = SingleCurrencyOrder.CustEmp(oApprovalDetails, oApprovalResults)
            End If
            Return bSuccessful
        Catch ex As Exception
            Return bSuccessful
        End Try
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

    Public Shared Function IsNYC(ByVal sBU As String) As Boolean
        Dim bIsNYC As Boolean = False

        Dim sSP1AccessString As String = "I0230~I0231~I0260"
        Try
            sSP1AccessString = UCase(ConfigurationSettings.AppSettings("NYCSitesList").ToString)
        Catch ex As Exception
            sSP1AccessString = "I0230~I0231~I0260"
        End Try

        Try
            bIsNYC = (sSP1AccessString.IndexOf(sBU.Trim.ToUpper) > -1)
        Catch ex As Exception
            bIsNYC = False
        End Try

        Return bIsNYC

    End Function

End Class

Public Class MultiCurrencyOrder
    Private Shared m_cDeletedLine As String = "QTR"
    Private Shared m_cApproverDelimiter As String = "||"
    Public Shared Function CustEmp(oApprovalDetails As ApprovalDetails, sBaseCurrCd As String, iExDaysAgo As Integer, _
     oApprovalResults As ApprovalResults) As Boolean

        Dim bSuccessful As Boolean = True ' Processing is successful by default
        Dim sOrdApprType As String = "O"
        Dim dblTotalCost As Double

        oApprovalResults.UpdateEmployeeResults(False, "P", oApprovalDetails.ApproverID)

        Dim oEnterprise As New clsEnterprise(oApprovalDetails.BU)
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
                        If GetReqLineCount(oApprovalDetails, iReqLnCount) Then
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
                        Dim strSQLstring As String = ""
                        Dim rowsaffected As Integer = 0
                        If RetrieveOrderLimAndNextApprv(oApprovalDetails.BU, oApprovalDetails.EnteredByID, oApprovalDetails.ApproverID, dblApprLimit, oApprovalResults.NextOrderApprover, oApprovalResults.ErrorInApproval) Then
                            If oApprovalResults.ErrorInApproval = ApprovalResults.ApprovalError.NoError Then
                                If dblTotalCost > dblApprLimit And oApprovalResults.NextOrderApprover.Trim.Length > 0 Then
                                    oApprovalResults.OrderExceededLimit = True
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
    Private Shared Function GetTotalCost(dsData As DataSet, sBaseCurrCd As String, iExDaysAgo As Integer) As Double
        Dim dblTotalCost As Double = 0.0
        For Each dr As DataRow In dsData.Tables(0).Rows
            Dim dblSubTotal As Double
            dblSubTotal = CType(dr.Item("SUBTOTAL").ToString(), Double)
            dblTotalCost = dblTotalCost + dblSubTotal
        Next
        Return dblTotalCost
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
        ds = ORDBData.GetAdapterSpc(strSQLstring)

        Return bSuccess
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
        ds = ORDBData.GetAdapterSpc(strSQLstring)

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

        ds = ORDBData.GetAdapterSpc(strSQLstring)

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

        ds = ORDBData.GetAdapterSpc(strSQLstring)

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

        ds = ORDBData.GetAdapterSpc(strSQLstring)

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

        ds = ORDBData.GetAdapterSpc(strSQLstring)

        Return bSuccess
    End Function
    Private Shared Function GetReqLineCount(oApprovalDetails As ApprovalDetails, ByRef iReqLnCount As Integer) As Boolean
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
            ds = ORDBData.GetAdapterSpc(strSQLstring)
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

                Dim ds As DataSet = ORDBData.GetAdapterSpc(strSQLstring)

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
Public Class ORDBData
    Public Shared Function GetAdapterSpc(ByVal p_strQuery As String) As DataSet
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)
            dataAdapter.Fill(UserdataSet)
            Try
                dataAdapter.Dispose()
            Catch ex As Exception
            End Try
            Try
                Command.Dispose()
            Catch ex As Exception
            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
        Catch objException As Exception
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception
            End Try
            Dim StrEx As String = objException.ToString & "  Check Connection String for permission problems - OrderApprovalMailUtility," & p_strQuery & " --- from ORDBData.vb, GetAdapterSpc. User supposed to see DBErrorPage.aspx"
            ErrorMail.SendSDiExchErrorMail(StrEx)

        End Try

        Return UserdataSet
    End Function

    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As Integer

        Dim rowsAffected As Integer = 0
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)

        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            rowsAffected = Command.ExecuteNonQuery()
            Try
                Command.Dispose()
            Catch ex As Exception

            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
        Catch objException As Exception
            rowsAffected = 0
            Try
                connection.Close()
            Catch ex As Exception

            End Try
        End Try

        Return rowsAffected
    End Function

    Public Shared Function GetReader(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As OleDbDataReader
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim datareader As OleDbDataReader
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As Exception
            Try
                connection.Close()
                connection.Dispose()
            Catch ex As Exception

            End Try
            'Dim sMsg66 As String = LCase(objException.ToString)
            'If bGoToErrPage Then
            '    sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User supposed to see DBErrorPage.aspx")
            '    Call ProcessError(sMsg66)
            'Else
            '    sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetReader. User will not see DBErrorPage.aspx")
            'End If
        End Try

    End Function
    Public Shared Function GetScalar(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As String
        Dim strReturn As String = ""
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Try
                strReturn = CType(Command.ExecuteScalar(), String)
            Catch ex32 As Exception
                strReturn = ""
            End Try
            If strReturn Is Nothing Then
                strReturn = ""
            End If
            Try
                Command.Dispose()
            Catch ex1 As Exception
            End Try
            Try
                connection.Close()
                connection.Dispose()
            Catch ex2 As Exception
            End Try
        Catch objException As Exception
            strReturn = ""
            Try
                connection.Close()
                connection.Dispose()
            Catch ex As Exception
                ''connection.close()
                'sendErrorEmail(objException.ToString & "  Check Connection String for permission problems", "NO", currentApp.Request.ServerVariables("URL"), p_strQuery & " --- from ORDBData.vb, GetScalar. User supposed to see DBErrorPage.aspx")
                'Dim sMsg66 As String = LCase(objException.ToString)
                'If bGoToErrPage Then
                '    Call ProcessError(sMsg66)
                'End If
            End Try
        End Try
        Return strReturn
    End Function
    Public Shared ReadOnly Property DbUrl() As String
        Get
            Return ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property
End Class

Public Class ApprovalDetails

    Public Sub New(sBU As String, sEnteredByID As String, sApproverID As String, sReqID As String)
        m_sBU = sBU
        m_sEnteredByID = sEnteredByID
        m_sApproverID = sApproverID
        m_sReqID = sReqID
        Dim oEnterprise As New clsEnterprise(sBU)
        m_sNSTKOnlyFlag = oEnterprise.ApprNSTKFlag
    End Sub

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

    Private m_sReqID As String
    Public ReadOnly Property ReqID As String
        Get
            Return m_sReqID
        End Get
    End Property

    Private m_sNSTKOnlyFlag As String
    Public ReadOnly Property NSTKOnlyFlag As String
        Get
            Return m_sNSTKOnlyFlag
        End Get
    End Property
End Class
Public Class ApprovalResults
    Public Enum ApprovalError
        NoError = 0
        InvalidApprovalChain
    End Enum

    Private m_oEmployee As EmployeeLevel
    Private m_oChargeCode As ChargeCodeLevel
    Private m_bNeedsQuote As Boolean
    Private m_eError As ApprovalError

    Public Sub New()
        m_oEmployee = New EmployeeLevel
        m_oChargeCode = New ChargeCodeLevel
        m_bNeedsQuote = False
        m_eError = ApprovalError.NoError
    End Sub

    Public Sub UpdateEmployeeResults(bExceededLimit, sNewHeaderStatus, sNextApproverID)
        m_oEmployee.ExceededLimit = bExceededLimit
        m_oEmployee.NewHeaderStatus = sNewHeaderStatus
        m_oEmployee.NextApproverID = sNextApproverID
    End Sub

    Public Property OrderExceededLimit() As Boolean
        Get
            Return m_oEmployee.ExceededLimit
        End Get
        Set(value As Boolean)
            m_oEmployee.ExceededLimit = value
        End Set
    End Property

    Public Property ErrorInApproval As ApprovalError
        Get
            Return m_eError
        End Get
        Set(value As ApprovalError)
            m_eError = value
        End Set
    End Property

    Public Function IsAnyChargeCodeExceededLimit() As Boolean
        Return m_oChargeCode.IsAnyExceededLimit
    End Function

    Public Property NextOrderApprover() As String
        Get
            Return m_oEmployee.NextApproverID
        End Get
        Set(value As String)
            m_oEmployee.NextApproverID = value
        End Set
    End Property

    Public Property NewOrderHeaderStatus() As String
        Get
            Return m_oEmployee.NewHeaderStatus
        End Get
        Set(value As String)
            m_oEmployee.NewHeaderStatus = value
        End Set
    End Property

    Public Function OrderChargeCode() As String
        Return m_oEmployee.ChargeCode
    End Function

    Public Function IsMoreApproversNeeded() As Boolean
        Dim bMoreApproversNeeded As Boolean = False

        If m_oEmployee.ExceededLimit Then
            bMoreApproversNeeded = True
        ElseIf m_oChargeCode.IsAnyExceededLimit Then
            bMoreApproversNeeded = True
        End If

        Return bMoreApproversNeeded
    End Function

    Public Function BudgetChargeCodesCount() As Integer
        Return m_oChargeCode.ChargeCodesCount
    End Function

    Public Property BudgetExceededLimit(iIndex As Integer) As Boolean
        Get
            Dim bExceeded As Boolean = False

            If IsValidBudgetIndex(iIndex) Then
                bExceeded = m_oChargeCode.ExceededLimit(iIndex)
            End If

            Return bExceeded
        End Get
        Set(value As Boolean)
            If IsValidBudgetIndex(iIndex) Then
                m_oChargeCode.ExceededLimit(iIndex) = value
            End If
        End Set
    End Property

    Public Property NextBudgetApprover(iIndex As Integer) As String
        Get
            Dim sApproverID As String = ""

            If IsValidBudgetIndex(iIndex) Then
                sApproverID = m_oChargeCode.NextApproverID(iIndex)
            End If

            Return sApproverID
        End Get
        Set(value As String)
            If IsValidBudgetIndex(iIndex) Then
                m_oChargeCode.NextApproverID(iIndex) = value
            End If
        End Set
    End Property

    Public Property NewBudgetHeaderStatus(iIndex As Integer) As String
        Get
            Dim sHeaderStatus As String = ""

            If IsValidBudgetIndex(iIndex) Then
                sHeaderStatus = m_oChargeCode.NewHeaderStatus(iIndex)
            End If

            Return sHeaderStatus
        End Get
        Set(value As String)
            If IsValidBudgetIndex(iIndex) Then
                m_oChargeCode.NewHeaderStatus(iIndex) = value
            End If
        End Set
    End Property

    Public Function BudgetChargeCode(iIndex As Integer) As String
        Dim sChargeCode As String = ""

        If IsValidBudgetIndex(iIndex) Then
            sChargeCode = m_oChargeCode.ChargeCode(iIndex)
        End If

        Return sChargeCode
    End Function

    Private Function IsValidBudgetIndex(iIndex As Integer) As Boolean
        Dim bValidIndex As Boolean = False

        If iIndex >= 0 And iIndex < m_oChargeCode.ChargeCodesCount Then
            bValidIndex = True
        End If

        Return bValidIndex
    End Function

    Public Sub InitBudgetChargeCodes(arrChgCodes As ArrayList)
        m_oChargeCode = New ChargeCodeLevel(arrChgCodes)
    End Sub

    Public Property NeedsQuote As Boolean
        Get
            Return m_bNeedsQuote
        End Get
        Set(value As Boolean)
            m_bNeedsQuote = value
        End Set
    End Property

    Private Class EmployeeLevel
        Private m_bExceededLimit As Boolean
        Private m_sNextApproverID As String
        Private m_sNewHeaderStatus As String
        Private m_sChgCode As String

        Public Sub New()
            m_bExceededLimit = False
            m_sNextApproverID = ""
            m_sNewHeaderStatus = ""
            m_sChgCode = "NotChgCode"
        End Sub

        Public Property ExceededLimit As Boolean
            Get
                Return m_bExceededLimit
            End Get
            Set(value As Boolean)
                m_bExceededLimit = value
            End Set
        End Property

        Public Property NewHeaderStatus As String
            Get
                Return m_sNewHeaderStatus
            End Get
            Set(value As String)
                m_sNewHeaderStatus = value
            End Set
        End Property

        Public Property NextApproverID As String
            Get
                Return m_sNextApproverID
            End Get
            Set(value As String)
                m_sNextApproverID = value
            End Set
        End Property

        Public Property ChargeCode As String
            Get
                Return m_sChgCode
            End Get
            Set(value As String)
                m_sChgCode = value
            End Set
        End Property
    End Class

    Private Class ChargeCodeLevel
        Private m_bExceededLimit() As Boolean
        Private m_sNextApproverID() As String
        Private m_sNewHeaderStatus() As String
        Private m_arrChgCode As ArrayList

        Public Sub New()
            m_bExceededLimit = New Boolean(0) {}
            m_bExceededLimit(0) = False

            m_sNextApproverID = New String(0) {}
            m_sNextApproverID(0) = ""

            m_sNewHeaderStatus = New String(0) {}
            m_sNewHeaderStatus(0) = ""

            m_arrChgCode = New ArrayList
            m_arrChgCode.Add("")
        End Sub

        Public Sub New(arrChargeCodes As ArrayList)
            m_arrChgCode = New ArrayList
            m_arrChgCode.Clear()
            m_arrChgCode.AddRange(arrChargeCodes)

            m_bExceededLimit = New Boolean(m_arrChgCode.Count - 1) {}
            m_sNextApproverID = New String(m_arrChgCode.Count - 1) {}
            m_sNewHeaderStatus = New String(m_arrChgCode.Count - 1) {}

        End Sub

        Public Function ChargeCodesCount() As Integer
            Return m_arrChgCode.Count
        End Function

        Public Function IsAnyExceededLimit() As Boolean
            Dim bExceeded As Boolean = False
            Dim I As Integer = 0

            While I < m_bExceededLimit.Length And Not bExceeded
                If m_bExceededLimit(I) Then
                    bExceeded = True
                End If
                I = I + 1
            End While

            Return bExceeded
        End Function

        Public Property ExceededLimit(iIndex As Integer) As Boolean
            Get
                Return m_bExceededLimit(iIndex)
            End Get
            Set(value As Boolean)
                m_bExceededLimit(iIndex) = value
            End Set
        End Property

        Public Property NextApproverID(iIndex As Integer) As String
            Get
                Return m_sNextApproverID(iIndex)
            End Get
            Set(value As String)
                m_sNextApproverID(iIndex) = value
            End Set
        End Property

        Public Property NewHeaderStatus(iIndex As Integer) As String
            Get
                Return m_sNewHeaderStatus(iIndex)
            End Get
            Set(value As String)
                m_sNewHeaderStatus(iIndex) = value
            End Set
        End Property

        Public ReadOnly Property ChargeCode(iIndex As Integer) As String
            Get
                Return m_arrChgCode(iIndex)
            End Get
        End Property
    End Class
End Class
Public Class EmployeeLevel
    Private m_bExceededLimit As Boolean
    Private m_sNextApproverID As String
    Private m_sNewHeaderStatus As String
    Private m_sChgCode As String

    Public Sub New()
        m_bExceededLimit = False
        m_sNextApproverID = ""
        m_sNewHeaderStatus = ""
        m_sChgCode = "NotChgCode"
    End Sub

    Public Property ExceededLimit As Boolean
        Get
            Return m_bExceededLimit
        End Get
        Set(value As Boolean)
            m_bExceededLimit = value
        End Set
    End Property

    Public Property NewHeaderStatus As String
        Get
            Return m_sNewHeaderStatus
        End Get
        Set(value As String)
            m_sNewHeaderStatus = value
        End Set
    End Property

    Public Property NextApproverID As String
        Get
            Return m_sNextApproverID
        End Get
        Set(value As String)
            m_sNextApproverID = value
        End Set
    End Property

    Public Property ChargeCode As String
        Get
            Return m_sChgCode
        End Get
        Set(value As String)
            m_sChgCode = value
        End Set
    End Property
End Class
Public Class clsEnterprise
    Private strApprNSTKFlag As String
    Public ReadOnly Property ApprNSTKFlag() As String
        Get
            Return strApprNSTKFlag
        End Get
    End Property
    Public Sub New(ByVal BusinessUnit As String)
        Dim strSQLstring As String = ""

        strSQLstring = "SELECT A.ISA_APPR_NSTK_FLAG" & vbCrLf

        strSQLstring = strSQLstring & " FROM sysadm8.PS_ISA_ENTERPRISE A, sysadm8.PS_CUSTOMER B" & vbCrLf & _
                " WHERE A.ISA_BUSINESS_UNIT = '" & BusinessUnit & "'" & vbCrLf & _
                " AND A.SETID = B.SETID(+)" & vbCrLf & _
                " AND A.CUST_ID = B.CUST_ID(+)" & vbCrLf


        Dim objReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)

        If objReader.Read() Then
            strApprNSTKFlag = objReader.Item("ISA_APPR_NSTK_FLAG")
        End If
    End Sub
End Class
Public Class sdiCurrency
    Public Sub New(ByVal id As String, ByVal desc As String)
        m_Id = id
        m_desc = desc
    End Sub
    Private m_Id As String = "USD"

    Public Property [Id]() As String
        Get
            Return m_Id
        End Get
        Set(ByVal value As String)
            m_Id = value
        End Set
    End Property

    Private m_desc As String = ""

    Public Property [Description]() As String
        Get
            Return m_desc
        End Get
        Set(ByVal value As String)
            m_desc = value
        End Set
    End Property

    Private m_bIsKnownCurrency As Boolean = False

    Public Property [IsKnownCurrency]() As Boolean
        Get
            Return m_bIsKnownCurrency
        End Get
        Set(ByVal value As Boolean)
            m_bIsKnownCurrency = value
        End Set
    End Property

    Private m_shortDesc As String = ""

    Public Property [ShortDescription]() As String
        Get
            Return m_shortDesc
        End Get
        Set(ByVal value As String)
            m_shortDesc = value
        End Set
    End Property

    Private m_symbol As String = "$"

    Public Property [Symbol]() As String
        Get
            Return m_symbol
        End Get
        Set(ByVal value As String)
            m_symbol = value
        End Set
    End Property

    Private m_country As String = ""

    Public Property [Country]() As String
        Get
            Return m_country
        End Get
        Set(ByVal value As String)
            m_country = value
        End Set
    End Property

End Class
Public Class sdiCommon

    Public Const APP_CONNECTION_STRING_ORA As String = "oraCNstring"
    Public Const DEFAULT_CURRENCY_CODE As String = "USD"
    Public Const DEFAULT_CURRENCY_SYMBOL As String = "$"

End Class
Public Class SingleCurrencyOrder
    Private Shared m_cDeletedLine As String = "QTR"
    Private Shared m_cApproverDelimiter As String = "||"
    Public Shared Function CustEmp(oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults, Optional IsLineApproval As Boolean = False) As Boolean
        Dim bSuccess As Boolean = True ' Processing is successful by default
        Dim bCalcNSTKOnly As Boolean
        Dim strSQLstring As String = ""
        oApprovalResults.UpdateEmployeeResults(False, "P", oApprovalDetails.ApproverID)
        If oApprovalDetails.NSTKOnlyFlag = "Y" Then
            bCalcNSTKOnly = True
        Else
            bCalcNSTKOnly = False
        End If
        Dim sOrdApprType As String = "O"
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

    Private Shared Function RetrieveNetUnitPrice_NSTK_O(sBU As String, sOrdNo As String, sChgCd As String, _
                                                 ByRef dblNetUnitPrice As Double) As Boolean
        Dim bSuccess As Boolean = False
        Dim strSQLstring As String

        strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) AS dblNetUnitPrice " & vbCrLf & _
            " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
            " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
            " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf & _
            " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
            " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
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

        strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf & _
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
        strSQLstring &= " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
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

        strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf & _
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

        strSQLstring &= " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf & _
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

        strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf & _
            " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf & _
            " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf & _
            " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf & _
            " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
            " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
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

        strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf & _
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
            " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
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

        strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf & _
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
            " AND B.QTY_REQUESTED > 0 AND B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
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
            ds = ORDBData.GetAdapterSpc(strSQLstring)
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

                Dim ds As DataSet = ORDBData.GetAdapterSpc(strSQLstring)

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

Public Class Notifiers
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
    Public Shared Sub buildNotifyApprover(ByVal strreqID As String, ByVal strAgent As String, ByVal strBU As String, ByVal strAppUserid As String, ByVal strHldStatus As String, Optional ByVal IsLineApproval As Boolean = False, Optional ByVal LineNBR As Integer = 0)
        Dim strSQLString As String = ""
        Dim strappName As String = ""
        Dim strappEmail As String = ""
        Dim strBuyerName As String = ""
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
        
        ' Created common method and moved code 
        Dim dataGridHTML As String
        If Not IsLineApproval Then
            LineNBR = 0
        End If
        dataGridHTML = GetApproverQuoteItems(strreqID, strBU, strAgent, LineNBR)
        Dim strwo As String = " "
        Dim strbodyhead As String
        Dim strbodydetl As String
        'Dim txtBody As String
        'Dim txtHdr As String
        'Dim txtMsg As String
        Dim streBU As String = encryptQueryString(strBU)
        Dim streOrdnum As String = encryptQueryString(strreqID)
        Dim streApper As String = encryptQueryString(strAppUserid)
        Dim streAppTyp As String = encryptQueryString(strHldStatus)
        Dim strhref As String
        Dim stritemid As String = ""
        strhref = ConfigurationSettings.AppSettings("WebAppName") & "NeedApprove.aspx?fer=" & streOrdnum & "&op=" & streApper & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"
        Dim StrResult As String = String.Empty
        'Dim Mailer As MailMessage = New MailMessage
        Dim StrFrom As String = ""
        Dim StrTo As String = ""
        Dim StrCC As String = ""
        Dim StrBcc As String = ""
        Dim StrBody As String = ""
        Dim StrSubject As String = ""
        StrFrom = "SDIExchange@SDI.com"
        StrCC = ""
        StrBcc = "webdev@sdi.com"  '  ;Tony.Smith@sdi.com"
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

        StrBody = strbodyhead & strbodydetl
        If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "STAR" Or _
            ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "RPTG" Or _
            ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Or _
            ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "DEVL" Then
            StrTo = "webdev@sdi.com;SDIportalsupport@avasoft.com"
            StrSubject = " <<TEST>> SDiExchange - Order Number " & strreqID & " needs approval"
        Else
            StrTo = strappEmail
            StrSubject = "SDiExchange - Order Number " & strreqID & " needs approval"
        End If
        
        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
        Try
            SDIEmailService.EmailUtilityServices("MailandStore", StrFrom, StrTo, StrSubject, StrCC, StrBcc, StrBody, "Request to Approver", MailAttachmentName, MailAttachmentbytes.ToArray())
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
    Private Shared Function GetApproverQuoteItems(ByVal strreqID As String, ByVal strBU As String, ByVal strAgent As String, Optional ByVal LineNBR As Integer = 0) As String
        Try
            Dim strSQLString As String = "SELECT ' ' AS ISA_IDENTIFIER," & vbCrLf & _
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
            " AND B.ISA_LINE_STATUS = 'QTW'" & vbCrLf & _
            " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
            " AND B.QTY_REQUESTED > 0" & vbCrLf & _
            " AND A.ORDER_NO = C.REQ_ID" & vbCrLf & _
            " AND B.ISA_INTFC_LN = C.LINE_NBR" & vbCrLf & _
            " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
            " AND C.REQ_ID = D.REQ_ID" & vbCrLf & _
            " AND C.LINE_NBR = D.LINE_NBR" & vbCrLf & _
            " AND C.BUSINESS_UNIT = F.BUSINESS_UNIT (+)" & vbCrLf & _
            " AND C.REQ_ID = F.REQ_ID (+)" & vbCrLf & _
            " AND C.LINE_NBR = F.LINE_NBR (+)"

            If Not LineNBR = 0 Then
                strSQLString = strSQLString + " AND B.ISA_INTFC_LN =" & LineNBR
            End If
            Dim dtgcart As DataGrid
            Dim SBstk As New StringBuilder
            Dim SWstk As New StringWriter(SBstk)
            Dim htmlTWstk As New HtmlTextWriter(SWstk)
            Dim dataGridHTML As String
            'Dim itemsid As Integer
            dtgcart = New DataGrid
            Dim dsOrder As DataSet = ORDBData.GetAdapterSpc(strSQLString)
            Dim dtOrder As DataTable = New DataTable()
            dtOrder = dsOrder.Tables(0)
            Dim dstcartSTK As New DataTable()
            dstcartSTK = buildCartforemail(dtOrder, strreqID)
            'Code for line items
            dtgcart.DataSource = dstcartSTK
            dtgcart.DataBind()
            dtgcart.CellPadding = 3
            dtgcart.BorderColor = System.Drawing.Color.Gray
            dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
            dtgcart.HeaderStyle.Font.Bold = True
            dtgcart.HeaderStyle.ForeColor = System.Drawing.Color.Black
            dtgcart.Width.Percentage(90)
            dtgcart.RenderControl(htmlTWstk)
            dataGridHTML = SBstk.ToString()
            Return dataGridHTML
        Catch ex As Exception
        End Try
    End Function
    Private Shared Function buildCartforemail(ByVal dstcart1 As DataTable, ByVal ordNumber As String) As DataTable
        Dim dr As DataRow
        'Dim I As Integer
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
        ' Dim SqlRdr As SqlDataReader = Nothing
        Dim strProdVwId As String = String.Empty

        Try
            If dstcart1.Rows.Count > 0 Then
                For Each dataRowMain As DataRow In dstcart1.Rows
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
                        dr("QTY") = CType(dataRowMain("QTY_REQ"), String).Trim()
                        If IsDBNull(CType(dataRowMain("QTY_REQ"), String).Trim()) Or CType(dataRowMain("QTY_REQ"), String).Trim() = " " Then
                            strQty = "0"
                        Else
                            strQty = CType(dataRowMain("QTY_REQ"), String).Trim()
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
            ds = ORDBData.GetAdapterSpc(strSQLstring)
            If ds.Tables(0).Rows.Count > 0 Then
                strOrigApproverID = ds.Tables(0).Rows(0).Item("isa_iol_apr_emp_id").ToString
            End If

        Catch ex As Exception
        End Try

        Return strOrigApproverID
    End Function
    Public Shared Function encryptQueryString(ByVal strQueryString) As String
        Dim oES As Encryption64 = New Encryption64
        Return oES.Encrypt(strQueryString, "b?50$#@!")
    End Function
End Class

Public Class Encryption64
    Private key() As Byte = {}
    Private IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}
    Public Function Encrypt(ByVal stringToEncrypt As String, ByVal SEncryptionKey As String) As String
        Try
            key = System.Text.Encoding.UTF8.GetBytes(Left(SEncryptionKey, 8))
            Dim des As New DESCryptoServiceProvider
            Dim inputByteArray() As Byte = Encoding.UTF8.GetBytes(stringToEncrypt)
            Dim ms As New MemoryStream
            Dim cs As New CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write)
            cs.Write(inputByteArray, 0, inputByteArray.Length)
            cs.FlushFinalBlock()
            Return Convert.ToBase64String(ms.ToArray())
        Catch e As Exception
            Return e.Message
        End Try
    End Function
End Class

Public Class clsUserTbl
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

    Private strEmployeeEmail As String
    Public ReadOnly Property EmployeeEmail() As String
        Get
            Return strEmployeeEmail
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
            strFirstNameSrch = objReader.Item("FIRST_NAME_SRCH")
            strLastNameSrch = objReader.Item("LAST_NAME_SRCH")
            strEmployeeEmail = objReader.Item("ISA_EMPLOYEE_EMAIL")
        End If
    End Sub
End Class