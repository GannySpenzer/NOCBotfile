Imports System.Data.OleDb
Imports SDI.UNCC.WorkOrderAdapter
Imports System.Configuration
Imports System.Web.Mail
Imports System.Collections.Generic
Imports System.Web.UI.WebControls
Imports System.IO
Imports System.Web.UI
Imports System.Text
Imports System.Drawing.Color
Imports System.Data.SqlClient
Imports System.Xml
Imports System.Web
Imports SDI.ApplicationLogger
Imports SDINonStockEmailUtility.ORDBData
Imports System.Collections.Specialized
Imports System.Security.Cryptography
Imports System.Math
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports Newtonsoft.Json
Imports System.Web.Script.Serialization
Imports System.Globalization
Imports SDINonStockEmailUtility.OrderApprovals
Imports SDINonStockEmailUtility.QuoteNonStockProcessor

Public Class QuoteNonStockProcessor
    Private m_siteCurrency As sdiCurrency = Nothing
    Private m_oApprovalDetails As ApprovalDetails

    Private m_logger As appLogger = Nothing
    'Private Const LETTER_HEAD_SdiExch As String = "<table width='100%' bgcolor='black'><tbody><tr><td><img src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' vspace='0' hspace='0' /></td>" &
    '                                                "<td width='100%'><br/><br/><br/><div align='center'><SPAN style='FONT-SIZE: x-large; WIDTH: 256px; FONT-FAMILY: Arial; Color: White;'>SDI Marketplace</SPAN></div>" &
    '                                                "<div align='center'><SPAN style='FONT-FAMILY: Arial; Color: White;'>SDI ZEUS - Request for Quote</SPAN></div></td></tr></tbody></table>" &
    '                                                "<HR width='100%' SIZE='1'>"

    'Private Const LETTER_HEAD_SdiExch_QTW As String = "<table width='100%' bgcolor='black'><tbody><tr><td><img src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' vspace='0' hspace='0' /></td>" &
    '                                                "<td width='100%'><br/><br/><br/><div align='center'><SPAN style='FONT-SIZE: x-large; WIDTH: 256px; FONT-FAMILY: Arial; Color: White;'>SDI Marketplace</SPAN></div>" &
    '                                                "<div align='center'><SPAN style='FONT-FAMILY: Arial; Color: White;'>SDI ZEUS - Request for Approval</SPAN></div></td></tr></tbody></table>" &
    '                                                "<HR width='100%' SIZE='1'>"

    'Private Const LETTER_HEAD As String = "<div><img src='https://www.sdizeus.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></div>" &
    '                                        "<div align=""center""><SPAN style=""FONT-SIZE: x-large; WIDTH: 256px; FONT-FAMILY: Arial"">SDI Marketplace</SPAN></div>" &
    '                                        "<div align=""center""><SPAN>In-Site® Online - Request for Quote</SPAN></div><br><br>" &
    '                                        "<HR width='100%' SIZE='1'>"

    'Private Const LETTER_CONTENT As String = "<p style=""TEXT-INDENT: 25pt"">" &
    '                                         "The above referenced order contains items that required a price " &
    '                                         "quote before processing.&nbsp;&nbsp;To view the quoted price either " &
    '                                         "click the link below or select the ""Requestor Approval"" menu option " &
    '                                         "in In-Site&reg; Online to approve or decline the order." &
    '                                         "<br></p>Sincerely,</p>" &
    '                                         "<p>SDI Customer Care</p>"

    'Private Const LETTER_CONTENT_SDiExchange As String = "<p style=""TEXT-INDENT: 25pt"">" &
    '                                         "The above referenced order contains items that required a price " &
    '                                         "quote before processing.&nbsp;&nbsp;To view the quoted price either " &
    '                                         "click the link below or select the ""Requestor Approval"" menu option " &
    '                                         "in SDI ZEUS to approve or decline the order." &
    '                                         "<br></p>Sincerely,</p>" &
    '                                         "<p>SDI Customer Care</p>"

    'Private Const LETTER_CONTENT_SDiExchange_QTW As String = "<p style=""TEXT-INDENT: 25pt"">" &
    '                                         "The above referenced order has needs your approval " &
    '                                         "Click the link below or select the ""Approve Orders"" menu option " &
    '                                         "in SDI ZEUS to approve or decline the order." &
    '                                         "<br></p>Sincerely,</p>" &
    '                                         "<p>SDI Customer Care</p>"


    'Private Const LETTER_CONTENT_PI As String = "<p style=""TEXT-INDENT: 25pt"">" &
    '                                            "The above referenced order contains items that required a price " &
    '                                            "quote before processing.&nbsp;&nbsp;To view the quoted price, please " &
    '                                            "select the ""Requestor Approval"" menu option " &
    '                                            "in In-Site&reg; Online to approve or decline the order." &
    '                                            "<br></p>Sincerely,</p>" &
    '                                            "<p>SDI Customer Care</p>"

    'Private Const LETTER_CONTENT_PI_SDiExchange As String = "<p style=""TEXT-INDENT: 25pt"">" &
    '                                            "The above referenced order contains items that required a price " &
    '                                            "quote before processing.&nbsp;&nbsp;To view the quoted price, please " &
    '                                            "select the ""Requestor Approval"" menu option " &
    '                                            "in SDI ZEUS to approve or decline the order." &
    '                                            "<br></p>Sincerely,</p>" &
    '                                            "<p>SDI Customer Care</p>"


    Private Shared m_CN As OleDbConnection
    Private Shared m_cEncryptionKey As String
    Private m_defaultFROM As String
    Private m_extendedTO As ArrayList
    Private m_extendedCC As ArrayList
    Private m_extendedBCC As ArrayList
    Private m_defaultSubject As String
    Private m_defaultToRecepient As ArrayList
    Private m_cURL As String
    Private m_cURL_SDiExch As String
    Private m_cList_BU_SDiExch As String
    'Private m_eventLogger As System.Diagnostics.EventLog
    Private Shared m_colMsgs As QuotedNStkItemCollection = New QuotedNStkItemCollection
    Private m_config As System.Xml.XmlDocument
    Private m_arrPunchInBUList As New ArrayList
    Dim m_quoteProp As QuotedNStkItem = New QuotedNStkItem
    Dim boItem As QuotedNStkItem = New QuotedNStkItem

    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\configSetting.xml"

    Dim logpath As String = "C:\Program Files (x86)\SDI\SDINonStockEmailUtility\LOGS\NonStockEmailUtil" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Public Shared Function IsUOC(ByVal sBU As String) As Boolean

        Dim bIsUOC As Boolean = False

        Try
            If sBU = "I0535" Then
                bIsUOC = True
            Else
                bIsUOC = False
            End If
        Catch ex As Exception
            bIsUOC = False
        End Try
        Return bIsUOC

    End Function

    Public ReadOnly Property DBConnection() As OleDbConnection
        Get
            Return m_CN
        End Get
    End Property

    Public Property EncryptionKey() As String
        Get
            Return m_cEncryptionKey
        End Get
        Set(ByVal Value As String)
            m_cEncryptionKey = Value
        End Set
    End Property

    'Public Property EventLogger() As System.Diagnostics.EventLog
    '    Get
    '        Return m_eventLogger
    '    End Get
    '    Set(ByVal Value As System.Diagnostics.EventLog)
    '        m_eventLogger = Value
    '    End Set
    'End Property

    Public Property defaultMsgFROM() As String
        Get
            Return m_defaultFROM
        End Get
        Set(ByVal Value As String)
            m_defaultFROM = Value
        End Set
    End Property

    Public Property defaultToRecepient() As ArrayList
        Get
            Return m_defaultToRecepient
        End Get
        Set(ByVal Value As ArrayList)
            m_defaultToRecepient = Value
        End Set
    End Property

    Public Property xTO() As ArrayList
        Get
            Return m_extendedTO
        End Get
        Set(ByVal Value As ArrayList)
            m_extendedTO = Value
        End Set
    End Property

    Public Property xCC() As ArrayList
        Get
            Return m_extendedCC
        End Get
        Set(ByVal Value As ArrayList)
            m_extendedCC = Value
        End Set
    End Property

    Public Property xBCC() As ArrayList
        Get
            Return m_extendedBCC
        End Get
        Set(ByVal Value As ArrayList)
            m_extendedBCC = Value
        End Set
    End Property

    Public Property SubjectLine() As String
        Get
            Return m_defaultSubject
        End Get
        Set(ByVal Value As String)
            m_defaultSubject = Value
        End Set
    End Property

    Public Property UrlSDiExch() As String
        Get
            Return m_cURL_SDiExch
        End Get
        Set(ByVal Value As String)
            m_cURL_SDiExch = Value
        End Set
    End Property

    Public Property ListBUsSDiExch() As String
        Get
            Return m_cList_BU_SDiExch
        End Get
        Set(ByVal Value As String)
            m_cList_BU_SDiExch = Value
        End Set
    End Property

    Public Property URL() As String
        Get
            Return m_cURL
        End Get
        Set(ByVal Value As String)
            m_cURL = Value
        End Set
    End Property

    Public Property Config() As System.Xml.XmlDocument
        Get
            Return m_config
        End Get
        Set(ByVal Value As System.Xml.XmlDocument)
            m_config = Value
        End Set
    End Property

    Public ReadOnly Property PunchInBusinessUnitList() As ArrayList
        Get
            If (m_arrPunchInBUList Is Nothing) Then
                m_arrPunchInBUList = New ArrayList
            End If
            Return m_arrPunchInBUList
        End Get
    End Property

    '
    ' process to evaluate/check and returns TRUE or FALSE whether to do execution process
    '
    Public Function Evaluate() As Boolean
        Dim cHdr As String = "QuoteNonStockProcessor.Evaluate: "
        Try
            ''''-ss.m_eventLogger.WriteEntry("evaluating busines rule(s) ...", EventLogEntryType.Information)
            'Return True

            Dim cSQL As String = "" &
                  "SELECT COUNT(1) AS RECCOUNT " & vbCrLf &
                  "FROM SYSADM8.PS_ISA_ORD_INTF_LN A " & vbCrLf &
                  "WHERE NOT EXISTS (" & vbCrLf &
                  "                  SELECT 'X' " & vbCrLf &
                  "                  FROM PS_ISA_REQ_EML_LOG B " & vbCrLf &
                  "                  WHERE B.BUSINESS_UNIT = A.BUSINESS_UNIT_OM " & vbCrLf &
                  "                    AND B.REQ_ID = A.ORDER_NO " & vbCrLf &
                  "                 ) " & vbCrLf &
                  "  AND A.ISA_LINE_STATUS = 'QTS' " & vbCrLf &
                  "  AND NOT EXISTS ( " & vbCrLf &
                  "                  SELECT 'X' " & vbCrLf &
                  "                  FROM SYSADM8.PS_NLINK_CUST_PLNT C " & vbCrLf &
                  "                  WHERE C.ISA_SAP_PO_PREF = SUBSTR(A.ORDER_NO,1,2) " & vbCrLf &
                  "                    AND C.ISA_SAP_PO_PREF <> ' ' " & vbCrLf &
                  "                 ) " & vbCrLf &
                  ""

            Dim nRecs As Integer = 0
            Dim rdr As OleDbDataReader
            Dim cmd As OleDbCommand = m_CN.CreateCommand

            With cmd
                .CommandText = cSQL
                .CommandType = CommandType.Text
            End With

            m_CN.Open()

            rdr = cmd.ExecuteReader

            If rdr.HasRows Then
                If rdr.Read Then
                    nRecs = CType(rdr.Item(0), Integer)
                    'm_eventLogger.WriteEntry(nRecs.ToString & " record(s) to process.", EventLogEntryType.Information)
                End If
            Else
                'm_eventLogger.WriteEntry("no record to process.", EventLogEntryType.Information)
            End If

            rdr.Close()

            rdr = Nothing
            cmd = Nothing

            m_CN.Close()

            Return (nRecs > 0)

        Catch ex As Exception
            'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)

            m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
    End Function

    Private Shared Function buildCartforemail(ByVal m_colMsgs As QuotedNStkItemCollection, ByVal ordNumber As String,
                        ByRef strWrkOrder As String, ByVal BU As String, Optional ByVal EmployeeID As String = "") As DataTable

        Dim dr As DataRow
        Dim I As Integer
        Dim strPrice As String
        Dim strQty As String
        Dim dstcart As DataTable
        dstcart = New DataTable
        Dim AltPartFlag As Boolean = False 'Mythili - SDI-37570,to add alt part num in material request email
        dstcart.Columns.Add("LN")
        dstcart.Columns.Add("Item ID")
        dstcart.Columns.Add("Description")
        dstcart.Columns.Add("Manuf.")
        dstcart.Columns.Add("Manuf. Partnum")
        dstcart.Columns.Add("Alt. Partnum")  'Mythili - SDI-37570,to add alt part num in material request email
        dstcart.Columns.Add("QTY")
        dstcart.Columns.Add("UOM")
        dstcart.Columns.Add("WorkOrder")
        dstcart.Columns.Add("ChargeCode")
        'dstcart.Columns.Add("Price")
        'dstcart.Columns.Add("Ext. Price")
        'dstcart.Columns.Add("Base Price")
        'dstcart.Columns.Add("Ext Base Price")
        dstcart.Columns.Add("Item USD Price")
        dstcart.Columns.Add("Ext. USD Price")
        'Madhu-16592-Display columns for ECI BU 
        Try
            If IsECI(BU) Then
                dstcart.Columns.Add("Item PO Price")
                dstcart.Columns.Add("Item PO Ext. Price")
            End If
        Catch ex As Exception
        End Try
        'Added Supplier & Supplier part num in approval email for WAL orders - WW-287 & WAL-632 Poornima S
        If BU = "I0W01" Then
            dstcart.Columns.Add("Supplier")
            dstcart.Columns.Add("Supplier Part Number")
        End If



        Dim strOraSelectQuery As String = String.Empty
        Dim ordIdentifier As String = String.Empty
        Dim ordBU As String = String.Empty
        Dim OrcRdr As OleDb.OleDbDataReader = Nothing
        Dim dsOrdLnItems As DataSet = New DataSet
        Dim strSqlSelectQuery As String = String.Empty
        Dim SqlRdr As SqlDataReader = Nothing
        Dim strProdVwId As String = String.Empty
        Dim SqlRdr1 As SqlDataReader = Nothing
        Dim SQLSTRINGQuery As String = String.Empty
        Dim currencyds As DataSet = New DataSet
        Try
            strOraSelectQuery = "select * from SYSADM8.PS_ISA_ORD_INTF_LN A where A.ORDER_NO = '" & ordNumber & "' AND A.ISA_LINE_STATUS in ('QTS','QTW') AND NOT EXISTS (SELECT 'X' FROM PS_ISA_REQ_EML_LOG B1 WHERE B1.REQ_ID = A.ORDER_NO AND B1.LINE_NO= A.ISA_INTFC_LN AND B1.PRICE= A.ISA_SELL_PRICE)"
            dsOrdLnItems = GetAdapter(strOraSelectQuery)
            Try
                SQLSTRINGQuery = "" & "SELECT" & vbCrLf &
                                  "INTFC.ISA_INTFC_LN" & vbCrLf &
                                  ",DECODE(DECODE(INTFC.NET_UNIT_PRICE,null, 0,INTFC.NET_UNIT_PRICE),INTFC.NET_UNIT_PRICE, D.PRICE_REQ) || ' ' ||DECODE(TRIM(F.CURRENCY_CD), NULL, D.CURRENCY_CD, F.CURRENCY_CD) AS BASECURRENCY" & vbCrLf &
                                  "FROM" & vbCrLf &
                                  "(" & vbCrLf &
                                  " Select " & vbCrLf &
                                  " A.BUSINESS_UNIT_OM " & vbCrLf &
                                  " ,A.ORDER_NO " & vbCrLf &
                                  " ,B.ISA_INTFC_LN " & vbCrLf &
                                    " ,B.INV_ITEM_ID " & vbCrLf &
                                    " ,B.ISA_SELL_PRICE AS NET_UNIT_PRICE " & vbCrLf &
                                  " From SYSADM8.PS_ISA_ORD_INTF_HD A " & vbCrLf &
                                  " ,SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf &
                                  " ,PS_BUS_UNIT_TBL_OM C " & vbCrLf &
                                   " WHERE A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                              " And A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT " & vbCrLf &
                              " AND A.BUSINESS_UNIT_OM = '" & BU & "' " & vbCrLf &
                              " AND A.ORDER_NO = '" & ordNumber & "' " & vbCrLf &
                               "  And B.ISA_LINE_STATUS IN ('QTS','QTW')" & vbCrLf &
                                    ")" & vbCrLf & " INTFC " & vbCrLf &
                              ",PS_REQ_LINE D" & vbCrLf &
                           ",SYSADM8.PS_ISA_SDIEX_PRCBU P " & vbCrLf &
                            ", SYSADM8.PS_ISA_REQ_BI_INFO F " & vbCrLf &
                           "WHERE INTFC.ORDER_NO = D.REQ_ID(+)" & vbCrLf &
                           "And INTFC.ISA_INTFC_LN = D.LINE_NBR (+) " & vbCrLf &
                          " And TRIM(INTFC.INV_ITEM_ID) = P.INV_ITEM_ID(+) " & vbCrLf &
                          " And D.BUSINESS_UNIT = F.BUSINESS_UNIT (+) " & vbCrLf &
                          " And D.REQ_ID = F.REQ_ID (+) " & vbCrLf &
                           " And D.LINE_NBR = F.LINE_NBR (+) " & vbCrLf &
                             " And INTFC.BUSINESS_UNIT_OM = P.BUSINESS_UNIT (+)" & vbCrLf & ""
                currencyds = ORDBData.GetAdapter(SQLSTRINGQuery)
            Catch ex As Exception

            End Try

            If dsOrdLnItems.Tables(0).Rows.Count > 0 Then
                Dim intMy21 As Integer = 0
                Dim BaseCurrency As String = String.Empty
                Dim vendorName As String = String.Empty
                For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows
                    Try
                        BaseCurrency = currencyds.Tables(0).AsEnumerable().
   Where(Function(r) Convert.ToString(r.Field(Of Decimal)("ISA_INTFC_LN")) = Convert.ToString(dataRowMain("ISA_INTFC_LN"))).
   Select(Function(r) Convert.ToString(r.Field(Of String)("BASECURRENCY"))).
   FirstOrDefault()
                    Catch ex As Exception

                    End Try
                    Try
                        Dim vendor As String = CType(dataRowMain("VENDOR_ID"), String).Trim()
                        SQLSTRINGQuery = "Select NAME1 from PS_Vendor where VENDOR_ID= '" & vendor & "'"
                        vendorName = ORDBData.GetScalar(SQLSTRINGQuery)
                    Catch ex As Exception

                    End Try

                    Dim ExtBasePrice As String = String.Empty
                    Dim ExtBaseCurrency As String = String.Empty
                    Try
                        Dim strarr() As String
                        strarr = BaseCurrency.Split(" "c)
                        ExtBasePrice = strarr(0)
                        ExtBaseCurrency = strarr(1)
                    Catch ex As Exception

                    End Try
                    'code to get work order id
                    If intMy21 = 0 Then
                        strWrkOrder = " "
                        Try
                            strWrkOrder = CType(dataRowMain("ISA_WORK_ORDER_NO"), String)
                            If strWrkOrder Is Nothing Then
                                strWrkOrder = " "
                            Else
                                If Trim(strWrkOrder) = "" Then
                                    strWrkOrder = " "
                                End If
                            End If
                        Catch ex As Exception
                            strWrkOrder = " "
                        End Try
                    End If
                    intMy21 = intMy21 + 1  ' end code to get work order id

                    dr = dstcart.NewRow()
                    dr("LN") = CType(dataRowMain("ISA_INTFC_LN"), String).Trim()
                    dr("Item ID") = CType(dataRowMain("INV_ITEM_ID"), String).Trim()
                    dr("Description") = CType(dataRowMain("DESCR254"), String).Trim()
                    dr("WorkOrder") = CType(dataRowMain("ISA_WORK_ORDER_NO"), String).Trim()
                    dr("ChargeCode") = CType(dataRowMain("ISA_CUST_CHARGE_CD"), String).Trim()
                    Try
                        dr("Manuf.") = If(CType(dataRowMain("MFG_ID"), String).Trim() <> "", CType(dataRowMain("MFG_ID"), String).Trim(), CType(dataRowMain("ISA_MFG_FREEFORM"), String).Trim())
                    Catch ex As Exception
                        dr("Manuf.") = " "
                    End Try
                    Try
                        dr("Manuf. Partnum") = CType(dataRowMain("MFG_ITM_ID"), String).Trim()
                    Catch ex As Exception
                        dr("Manuf. Partnum") = " "
                    End Try
                    Try
                        dr("Item ID") = CType(dataRowMain("INV_ITEM_ID"), String).Trim()
                    Catch ex As Exception
                        dr("Item ID") = " "
                    End Try
                    Dim alt_part_num As String = " "  'Mythili - SDI-37570,to add alt part num in material request email
                    Try
                        Dim item_id As String = Trim(dr("Item ID"))
                        Try
                            If item_id IsNot Nothing Then
                                If Trim(item_id) <> "" Then

                                    Dim sqlstring As String = " "
                                    sqlstring = "select SUB_ITM_ID from PS_SUBSTITUTE_ITM where INV_ITEM_ID like '%" + item_id + "'"
                                    alt_part_num = ORDBData.GetScalar(sqlstring)
                                    dr("Alt. Partnum") = alt_part_num
                                End If
                            End If
                        Catch ex As Exception

                        End Try
                        'If Trim(alt_part_num) <> "" Then
                        '    ' AltPartFlag = True
                        '    'dr("Alt. Partnum") = alt_part_num
                        'End If
                    Catch ex As Exception

                    End Try
                    Try
                        dr("UOM") = CType(dataRowMain("UNIT_OF_MEASURE"), String).Trim()
                    Catch ex As Exception
                        dr("UOM") = "EA"
                    End Try

                    Try
                        strQty = CType(dataRowMain("QTY_REQUESTED"), String).Trim()
                        strQty = strQty.Remove(strQty.Length - 2)
                        dr("QTY") = strQty
                        If IsDBNull(CType(dataRowMain("QTY_REQUESTED"), String).Trim()) Or CType(dataRowMain("QTY_REQUESTED"), String).Trim() = " " Then
                            strQty = "0"
                        End If
                    Catch ex As Exception
                        strQty = "0"
                    End Try

                    strPrice = "0.00"
                    Dim itemPrice As Decimal = 0
                    Try
                        If BU = "I0W01" Then
                            strPrice = CDec(CType(dataRowMain("PRICE_VNDR"), String).Trim()).ToString()
                        Else
                            strPrice = CDec(CType(dataRowMain("ISA_SELL_PRICE"), String).Trim()).ToString()
                        End If
                        itemPrice = CType(strPrice, Decimal)
                        If CDec(strPrice) = 0 Then
                            dr("Item USD Price") = "Price on Request"
                        Else
                            dr("Item USD Price") = itemPrice.ToString("####,###,##0.0000")
                        End If
                    Catch ex As Exception

                    End Try
                    Try
                        Dim itemExtPrice As Decimal = 0
                        itemExtPrice = CType(strPrice, Decimal) * Convert.ToDecimal(strQty)
                        dr("Ext. USD Price") = itemExtPrice.ToString("####,###,##0.00")
                    Catch ex As Exception
                    End Try
                    'Madhu-16592-Display columns for ECI BU 
                    Try
                        If IsECI(BU) Then
                            Try
                                dr("Item PO Price") = FormatNumber(CDec(ExtBasePrice).ToString("f"), 4) & " " & ExtBaseCurrency.ToString()
                            Catch ex As Exception

                            End Try
                            Try
                                dr("Item PO Ext. Price") = FormatNumber(CType(Convert.ToDecimal(strQty) * Convert.ToDecimal(ExtBasePrice), String), 2) & " " & ExtBaseCurrency.ToString()
                            Catch ex As Exception

                            End Try
                        End If
                    Catch ex As Exception
                    End Try
                    dr("LN") = CType(dataRowMain("ISA_INTFC_LN"), String).Trim()

                    'Added Supplier & Supplier part num in approval email for WAL orders - WW-287 & WAL-632 Poornima S
                    If BU = "I0W01" Then
                        Try
                            dr("Supplier") = CType(dataRowMain("VENDOR_ID"), String).Trim() + "- " + vendorName
                        Catch ex As Exception
                        End Try

                        Try
                            dr("Supplier Part Number") = CType(dataRowMain("MFG_ITM_ID"), String).Trim()
                        Catch ex As Exception
                        End Try
                    End If
                    'WAL-1044 making price 0 for third party[change by Vishalini]
                    If BU = "I0W01" Then
                        Try
                            Dim SqlStringThirdPartCompid As String = "select THIRDPARTY_COMP_ID from sdix_users_tbl where THIRDPARTY_COMP_ID <> '0' and THIRDPARTY_COMP_ID is not null and ISA_EMPLOYEE_ID = '" & EmployeeID & "'"
                            Dim THIRDPARTY_COMP_ID As String = ORDBData.GetScalar(SqlStringThirdPartCompid)
                            If THIRDPARTY_COMP_ID <> "" Then
                                dr("Item USD Price") = "0"
                                dr("Ext. USD Price") = "0"
                            End If
                        Catch ex As Exception

                        End Try

                    End If


                    dstcart.Rows.Add(dr)
                Next
                'If AltPartFlag = False Then  'Mythili - SDI-37570,to add alt part num in material request email
                '    dstcart.Columns.Remove("Alt. Partnum")
                'End If
            End If
        Catch ex As Exception

        End Try
        Return dstcart

    End Function

    '
    ' executes process
    '
    Public Sub Execute()
        Dim cHdr As String = "QuoteNonStockProcessor.Execute: "
        Dim trackOrderNo As String = ConfigurationManager.AppSettings("TrackOrderNo")

        '    log path/file
        Dim sLogPath As String = ""
        Try
            sLogPath = My.Settings("logMyPath").ToString.Trim
        Catch ex As Exception
        End Try
        If (sLogPath.Length > 0) Then
            logpath = sLogPath & "\NonStockEmailUtil" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' initialize log
        m_logger = New appLogger(logpath, logLevel)

        ' log verbose
        m_logger.WriteVerboseLog(" Start of Quote Non Stock Email Utility process: " & Now())

        Try
            If Not trackOrderNo Is Nothing And trackOrderNo.ToLower().Equals("true") Then
                SendLogger("Logging execution of Quote Non Stock Email Utility", "Execution started at " & DateTime.Now.ToShortTimeString(), "LOGGER", "Mail", "WebDev@sdi.com", String.Empty, String.Empty)
            End If

            SetConfigXML()

            m_colMsgs = New QuotedNStkItemCollection

            m_CN.Open()
            Dim SBstk As New StringBuilder
            Dim SWstk As New StringWriter(SBstk)
            Dim htmlTWstk As New HtmlTextWriter(SWstk)
            Dim SBord As New StringBuilder

            If GetQuotedItems() > 0 Then
                ' Dim iCnt As Integer = 0
                If m_colMsgs.Count > 0 Then
                    '  iCnt = 0
                    For Each itmQuoted As QuotedNStkItem In m_colMsgs
                        SBord.Append(itmQuoted.OrderID + ",")
                        Dim TtlPrice As Decimal = GetPrice(itmQuoted.OrderID)
                        If itmQuoted.PriceBlockFlag = "N" Then
                            If itmQuoted.ApprovalLimit > 0 Then
                                If TtlPrice > itmQuoted.ApprovalLimit Then
                                    SendMessages(itmQuoted, itmQuoted.BusinessUnitID)
                                Else
                                    ' set line status to QTC or QTA (itmQuoted.LineStatus) and add Audit record
                                    Dim strUpdateLineStatusFinal As String = ""
                                    strUpdateLineStatusFinal = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = '" & itmQuoted.LineStatus & "', OPRID_APPROVED_BY = 'SDIX', APPROVAL_DTTM = SYSDATE " & vbCrLf &
                                    "WHERE BUSINESS_UNIT_OM = '" & itmQuoted.BusinessUnitOM & "' AND ORDER_NO = '" & itmQuoted.OrderID & "' " & vbCrLf &
                                    "AND ISA_LINE_STATUS = 'QTS'"

                                    Dim iRowsAffctd As Integer = 0
                                    Try
                                        iRowsAffctd = ORDBData.ExecNonQuery(strUpdateLineStatusFinal, False)

                                        If iRowsAffctd > 0 Then
                                            SDIAuditInsert("PS_ISA_ORD_INTF_LN", itmQuoted.OrderID, "ISA_LINE_STATUS", itmQuoted.LineStatus, itmQuoted.BusinessUnitOM)

                                        End If

                                    Catch ex As Exception

                                    End Try

                                    'Dim oApprovalDetails As ApprovalDetails = New ApprovalDetails(itmQuoted.BusinessUnitOM, itmQuoted.EmployeeID, itmQuoted.EmployeeID, itmQuoted.OrderID)
                                    'Dim strAppMessage() As String
                                    'If OrderApprovals.ApproveQuote(oApprovalDetails, strAppMessage, itmQuoted.LineStatus) Then

                                    'End If
                                End If
                            Else
                                SendMessages(itmQuoted, itmQuoted.BusinessUnitID)
                            End If
                            If itmQuoted.EmployeeID = "MAIKU49404" Then
                                Try
                                    Dim strSQLstring As String = "SELECT ISA_EMPLOYEE_EMAIL from SDIX_USERS_TBL where ISA_EMPLOYEE_ID= 'TAYOW50428'"
                                    Dim EmailID As String = ORDBData.GetScalar(strSQLstring)
                                    If EmailID <> "" Then
                                        itmQuoted.EmployeeID = "TAYOW50428"
                                        itmQuoted.Addressee = "TAYLOR OWENS"
                                        'WAL-1054 Changing InsourceAssets@walmart.com from Taylor.Owens@walmart.com[Change By vishalini]
                                        If Not getDBName() Then
                                            itmQuoted.TO = EmailID
                                        Else
                                            Dim Email_To As String = ConfigurationManager.AppSettings("InsourceAssetsEmailId")
                                            itmQuoted.TO = Email_To
                                        End If
                                        SendMessages(itmQuoted, itmQuoted.BusinessUnitID)
                                    End If
                                Catch ex As Exception
                                End Try
                            End If

                        Else
                            If itmQuoted.ApprovalLimit > 0 Then
                                If TtlPrice > itmQuoted.ApprovalLimit Then
                                    PriceUpdate(itmQuoted.OrderID, "QTW")
                                Else
                                    PriceUpdate(itmQuoted.OrderID, itmQuoted.LineStatus) ' set to 'QTC' or 'QTA'
                                End If
                            Else
                                PriceUpdate(itmQuoted.OrderID, "QTW")
                            End If

                            UpdateReqEmailLog(itmQuoted, itmQuoted.BusinessUnitOM)
                            buildNotifyApprover(itmQuoted)
                        End If
                    Next
                End If
            End If

            If Not trackOrderNo Is Nothing And trackOrderNo.ToLower().Equals("true") Then
                If m_colMsgs.Count > 0 Then
                    SendLogger("Logging execution of Quote Non Stock Utility", "The following Orders were processed: " & SBord.ToString().TrimEnd(",") & " by Quote Non Stock Email Utility. <br/>Execution is ended at " & DateTime.Now.ToShortTimeString(), "LOGGER", "Mail", "WebDev@sdi.com", String.Empty, String.Empty)
                Else
                    SendLogger("Logging execution of Quote Non Stock Utility", "No Orders were found. Nothing was processed by Quote Non Stock Email Utility. ", "LOGGER", "Mail", "WebDev@sdi.com", String.Empty, String.Empty)
                End If
            End If
            If m_colMsgs.Count > 0 Then
                m_logger.WriteVerboseLog("The following Orders were processed: " & SBord.ToString().TrimEnd(",") & " by Quote Non Stock Email Utility. ")
            Else
                m_logger.WriteVerboseLog("No Orders were found. Nothing was processed by Quote Non Stock Email Utility. ")
            End If
            m_CN.Close()
            m_colMsgs = Nothing
        Catch ex As Exception
            Try
                m_CN.Dispose()
                m_CN.Close()
            Catch ex1 As Exception
            End Try
            m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
        m_logger.WriteVerboseLog(" End of Quote Non Stock Email Utility process: " & Now())
    End Sub

    Public Sub SetConfigXML()
        Try


            m_xmlConfig = New XmlDocument
            m_xmlConfig.Load(filename:=m_configFile)

            Dim cnString As String = ""
            If Not (m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText Is Nothing) Then
                cnString = m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText.Trim
            End If


            If Not (m_xmlConfig Is Nothing) Then
                m_config = m_xmlConfig
            Else
                m_config = New System.Xml.XmlDocument
            End If

            If Evaluate() Then
                Dim xmlEmailElement As XmlElement = Nothing

                ' get encryption key for email use
                If Not (m_xmlConfig("configuration")("s").Attributes("id").InnerText Is Nothing) Then
                    If m_xmlConfig("configuration")("s").Attributes("id").InnerText.Trim.Length > 0 Then
                        EncryptionKey = m_xmlConfig("configuration")("s").Attributes("id").InnerText.Trim
                    End If
                End If

                ' get "email" node of the configuration file
                If Not (m_xmlConfig("configuration")("email") Is Nothing) Then
                    xmlEmailElement = m_xmlConfig("configuration")("email")
                End If

                If Not (xmlEmailElement Is Nothing) Then
                    ' get the default sender properties
                    If Not (xmlEmailElement("defaultFrom").Attributes("addy").InnerText Is Nothing) Then
                        defaultMsgFROM = xmlEmailElement("defaultFrom").Attributes("addy").InnerText.Trim
                    End If

                    ' get additional recipient (as TO) email addresses
                    If Not (xmlEmailElement("additionalTo").ChildNodes Is Nothing) Then
                        If xmlEmailElement("additionalTo").ChildNodes.Count > 0 Then
                            For Each toItem As XmlNode In xmlEmailElement("additionalTo").ChildNodes
                                If toItem.Name = "toItem" And Not (toItem.Attributes("addy").InnerText Is Nothing) Then
                                    If toItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                        xTO.Add(toItem.Attributes("addy").InnerText.Trim)
                                    End If
                                End If
                            Next
                        End If
                    End If

                    ' get default recipient addresses just in-case there's no valid
                    If Not (xmlEmailElement("noRecepientDefaultTo").ChildNodes Is Nothing) Then
                        If xmlEmailElement("noRecepientDefaultTo").ChildNodes.Count > 0 Then
                            For Each toItem As XmlNode In xmlEmailElement("noRecepientDefaultTo").ChildNodes
                                If toItem.Name = "toItem" And Not (toItem.Attributes("addy").InnerText Is Nothing) Then
                                    If toItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                        defaultToRecepient.Add(toItem.Attributes("addy").InnerText.Trim)
                                    End If
                                End If
                            Next
                        End If
                    End If

                    ' get additional recipient (as CC) email addresses
                    If Not (xmlEmailElement("additionalCc").ChildNodes Is Nothing) Then
                        If xmlEmailElement("additionalCc").ChildNodes.Count > 0 Then
                            For Each ccItem As XmlNode In xmlEmailElement("additionalCc").ChildNodes
                                If ccItem.Name = "ccItem" And Not (ccItem.Attributes("addy").InnerText Is Nothing) Then
                                    If ccItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                        xCC.Add(ccItem.Attributes("addy").InnerText.Trim)
                                    End If
                                End If
                            Next
                        End If
                    End If

                    ' get additional recipient (as BCC) email addresses
                    If Not (xmlEmailElement("additionalBcc").ChildNodes Is Nothing) Then
                        If xmlEmailElement("additionalBcc").ChildNodes.Count > 0 Then
                            For Each bccItem As XmlNode In xmlEmailElement("additionalBcc").ChildNodes
                                If bccItem.Name = "bccItem" And Not (bccItem.Attributes("addy").InnerText Is Nothing) Then
                                    If bccItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                        xBCC.Add(bccItem.Attributes("addy").InnerText.Trim)
                                    End If
                                End If
                            Next
                        End If
                    End If

                    ' get the default subject for this email(s)
                    If Not (xmlEmailElement("defaultSubject").Attributes("text").InnerText Is Nothing) Then
                        SubjectLine = xmlEmailElement("defaultSubject").Attributes("text").InnerText.Trim
                    End If

                    ' get the URL to use for approving this quoted item
                    If Not (xmlEmailElement("body").Attributes("linkURL").InnerText Is Nothing) Then
                        URL = xmlEmailElement("body").Attributes("linkURL").InnerText.Trim
                    End If

                    ' get the URL for SDiExchange to use for approving this quoted item
                    If Not (xmlEmailElement("bodyExch").Attributes("linkUrlSdiExch").InnerText Is Nothing) Then
                        UrlSDiExch = xmlEmailElement("bodyExch").Attributes("linkUrlSdiExch").InnerText.Trim
                    End If

                    ' get Bus Units list for SDiExchange to use for creating SDiExchange e-mail and link
                    If Not (xmlEmailElement("busUnits").Attributes("listForSdiExch").InnerText Is Nothing) Then
                        ListBUsSDiExch = xmlEmailElement("busUnits").Attributes("listForSdiExch").InnerText.Trim
                    End If
                End If

                ' list of punch-In business units
                xmlEmailElement = Nothing
                If Not (m_xmlConfig("configuration")("punchInBUs") Is Nothing) Then
                    xmlEmailElement = m_xmlConfig("configuration")("punchInBUs")
                End If

                If Not (xmlEmailElement Is Nothing) Then
                    If Not (xmlEmailElement.ChildNodes Is Nothing) Then
                        For Each bu As XmlNode In xmlEmailElement.ChildNodes
                            If bu.Name = "punchInBU" And Not (bu.Attributes("id").InnerText Is Nothing) Then
                                If bu.Attributes("id").InnerText.Trim.Length > 0 Then
                                    If PunchInBusinessUnitList.IndexOf(bu.Attributes("id").InnerText.Trim.ToUpper) = -1 Then
                                        PunchInBusinessUnitList.Add(bu.Attributes("id").InnerText.Trim.ToUpper)
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If

            End If

        Catch ex As Exception
            SendLogger("Error in Quote Non Stock Email Utility", ex, "ERROR")
            'MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
            m_logger.WriteVerboseLog("SetConfigXML.  Error:  " & ex.ToString)
        End Try
    End Sub
    'Madhu-INC0015106-Removed avacorp in Email flow
    Public Shared Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String, Optional ByVal sBU As String = "", Optional ByVal REQBU As String = "")
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim From As String = String.Empty
            If Not getDBName() Then
                If REQBU <> "I0635" And EmailTo = "webdev@sdi.com" Then
                    EmailTo = "webdev@sdi.com"
                End If
            End If
            'SDI-40628 Changing email
            'SP-316 Purchasing Email from address change[By Vishalini]
            From = getpurchasingEmailFrom(sBU)
            SDIEmailService.EmailUtilityServices(MailType, From, EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

    'SP-316 Purchasing Email from address change[By Vishalini]
    Public Shared Function getpurchasingEmailFrom(ByVal sBU As String) As String
        Dim sqlStringEmailFrom As String = ""
        Dim PurchasingEmailFrom As String = ""
        Try
            sqlStringEmailFrom = "Select ISA_PURCH_EML_FROM from PS_ISA_BUS_UNIT_PM where BUSINESS_UNIT_PO ='" & sBU & "'"
            PurchasingEmailFrom = ORDBData.GetScalar(sqlStringEmailFrom)
        Catch ex As Exception

        End Try
        Return PurchasingEmailFrom
    End Function
    'Mythili -- INC0023448 Adding CC emails
    Public Shared Function getpurchasingEmailTo(ByVal sBU As String) As String
        Dim sqlStringEmailTo As String = ""
        Dim PurchasingEmailTo As String = ""
        Try
            sqlStringEmailTo = "Select ISA_PURCH_EML_TO from PS_ISA_BUS_UNIT_PM where BUSINESS_UNIT_PO ='" & sBU & "'"
            PurchasingEmailTo = ORDBData.GetScalar(sqlStringEmailTo)
        Catch ex As Exception

        End Try
        Return PurchasingEmailTo
    End Function

    Public Shared Sub SendLogger(ByVal subject As String, ByVal exception As Exception, ByVal messageType As String, Optional ByVal Query As String = "", Optional ByVal ConString As String = "", Optional ByVal sBU As String = "")
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim objException As String = ""
            Dim objExceptionTrace As String = ""
            Dim StrQuery As String = String.Empty
            Dim ConStr As String = String.Empty
            Try
                objException = "<b> Exception </b> - " & exception.Message & "<br/>"
                objExceptionTrace = "<b> Exception Trace </b> - " & exception.StackTrace & "<br/>"
                StrQuery = "<b> Execution Query </b> - " & Query & "<br/>"
                ConStr = "<b> Database Connection string </b> - " & ConString & "<br/>"
            Catch ex As Exception
            End Try
            'SDI-40628 Changing email
            Dim From As String = String.Empty
            'SP-316 Purchasing Email from address change[By Vishalini]
            From = getpurchasingEmailFrom(sBU)

            SDIEmailService.EmailUtilityServices("Mail", From, "WebDev@sdi.com", subject, String.Empty, String.Empty, objException & objExceptionTrace & Query & ConStr, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())


            'Dim strBody As String = objException & objExceptionTrace & Query & ConStr

        Catch ex As Exception

        End Try
    End Sub
    Public Sub New(ByVal cnString As String)
        InitMembers()

        If cnString.Length > 0 Then
            m_CN = New OleDbConnection(cnString)
        End If
    End Sub

    Private Sub InitMembers()
        ' default key just in-case config file was missing it
        m_cEncryptionKey = "bautista"

        m_defaultFROM = ""

        m_extendedTO = New ArrayList
        m_extendedCC = New ArrayList
        m_extendedBCC = New ArrayList

        m_defaultSubject = ""
        m_defaultToRecepient = New ArrayList
        m_cURL = ""
        m_cURL_SDiExch = ""
        m_cList_BU_SDiExch = ""

        m_CN = Nothing
        'm_eventLogger = Nothing
        m_colMsgs = Nothing
        m_config = Nothing
    End Sub
    'INC0037100 - Email To for Needs approvals from Non stock email utility should come from the ISA_USERS1 based upon Zeus 2 site flag = 'Y' - Dhamo
    Public Function GetQuotedItems() As Integer
        Dim cHdr As String = "QuoteNonStockProcessor.GetQuotedItems: "
        Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
        Dim connection As OleDbConnection = New OleDbConnection(connectionString)
        Dim StartDate As New String(Convert.ToString(ConfigurationManager.AppSettings("StartDate")))
        Try

            Dim cSQL As String = "" &
                                 "SELECT " & vbCrLf &
                                 " L.ISA_LINE_STATUS As LINESTATUS, A.BUSINESS_UNIT AS BUSINESS_UNIT" & vbCrLf &
                                 ",A.REQ_ID AS REQ_ID,A1.BUYER_ID,B.DESCR,B.EMAILID,L.SHIPTO_ID AS SHIPTO" & vbCrLf &
                                 ",A1.LINE_NBR AS LINE_NBR" & vbCrLf &
                                 ",A4.BILL_TO_CUST_ID AS SOLD_TO_CUST_ID" & vbCrLf &
                                 ",A2.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL" & vbCrLf &
                                 ",A2.ISA_EMPLOYEE_NAME AS ISA_EMPLOYEE_NAME" & vbCrLf &
                                 ",A2.ISA_PRICE_BLOCK AS ISA_PRICE_BLOCK" & vbCrLf &
                                 ",A3.ISA_NONSKREQ_EMAIL AS ISA_NONSKREQ_EMAIL" & vbCrLf &
                                 ",L.ISA_EMPLOYEE_ID AS ISA_EMPLOYEE_ID,L.isa_user1 AS APPROVER" & vbCrLf &
                                 ",A4.BUSINESS_UNIT_OM AS BUSINESS_UNIT_OM" & vbCrLf &
                                 ",L.ISA_PRIORITY_FLAG,A4.ORIGIN" & vbCrLf &
                                 ",L.OPRID_MODIFIED_BY AS OPRID_MODIFIED_BY, L.ISA_WORK_ORDER_NO AS WORK_ORDER_ID,L.ISA_USER2 AS STORE , A3.APPRVALTHRESHOLD AS APPROVAL_LIMIT " & vbCrLf &
                                 ",A3.ISA_CUSTINT_APPRVL,A3.ZEUS_SITE " & vbCrLf &
                                 "FROM " & vbCrLf &
                                 " PS_REQ_HDR A" & vbCrLf &
                                 ",SYSADM8.PS_ROLEXLATOPR B" & vbCrLf &
                                 ",PS_REQ_LINE A1" & vbCrLf &
                                 ",PS_ISA_USERS_TBL A2" & vbCrLf &
                                 ",PS_ISA_ENTERPRISE A3" & vbCrLf &
                                 ",SYSADM8.PS_ISA_ORD_INTF_HD A4, SYSADM8.PS_ISA_ORD_INTF_LN L, sysadm8.ps_isa_req_bi_info I  " & vbCrLf &
                                 " WHERE A.BUSINESS_UNIT = A1.BUSINESS_UNIT" & vbCrLf &
                                 "  AND A1.BUYER_ID = B.ROLEUSER (+)" & vbCrLf &
                                 "  AND A.REQ_ID = A1.REQ_ID" & vbCrLf &
                                 "  AND A.REQ_ID = A4.ORDER_NO" & vbCrLf &
                                 "  AND A1.BUSINESS_UNIT = I.BUSINESS_UNIT" & vbCrLf &
                                 "  AND A1.REQ_ID = I.REQ_ID" & vbCrLf &
                                 "  AND A1.line_nbr = I.line_nbr" & vbCrLf &
                                 "  AND I.BUSINESS_UNIT_OM = L.BUSINESS_UNIT_OM" & vbCrLf &
                                 "  AND A.REQ_ID = L.ORDER_NO" & vbCrLf &
                                 "  AND A1.line_nbr = L.ISA_INTFC_LN" & vbCrLf &
                                 "  AND A4.ORIGIN IN ('IOL','MOB','RFQ','IAP','PCH','INT','INV','CPI')" & vbCrLf &
                                 "  AND L.ISA_EMPLOYEE_ID = A2.ISA_EMPLOYEE_ID (+) " & vbCrLf &
                                 "  AND 'MAIN1' = A3.SETID (+)" & vbCrLf &
                                 "  AND A4.BILL_TO_CUST_ID = A3.CUST_ID (+)" & vbCrLf &
                                 "  AND L.OPRID_APPROVED_BY in (' ',L.ISA_EMPLOYEE_ID,'AUTOSYS')" & vbCrLf &
                                 "  AND L.ISA_LINE_STATUS in ('QTS','QTW')" & vbCrLf &
                                 "  and trunc(L.ADD_DTTM) >= TO_DATE('" & StartDate & "','DD-MM-YY')" & vbCrLf &
                                 "  AND (A3.ZEUS_SITE = 'N' OR (A3.ZEUS_SITE = 'Y' AND ((L.ISA_LINE_STATUS = 'QTS') OR (L.ISA_LINE_STATUS = 'QTW'" & vbCrLf &
                                 "  And NOT EXISTS ( SELECT 1 FROM PS_ISA_ORD_INTF_LN SL WHERE SL.ISA_LINE_STATUS IN ('QTS','CRE','IPR','NEW') AND L.ORDER_NO = SL.ORDER_NO)))))" & vbCrLf &
                                 "  AND NOT EXISTS ( " & vbCrLf &
                                 "                  SELECT 'X' " & vbCrLf &
                                 "                  FROM SYSADM8.PS_NLINK_CUST_PLNT C " & vbCrLf &
                                 "                  WHERE C.ISA_SAP_PO_PREF = SUBSTR(A.REQ_ID,1,2) " & vbCrLf &
                                 "                    AND C.ISA_SAP_PO_PREF <> ' ' " & vbCrLf &
                                 "                 ) " & vbCrLf &
                                 "  AND NOT EXISTS (" & vbCrLf &
                                 "                  SELECT 'X'" & vbCrLf &
                                 "                  FROM PS_ISA_REQ_EML_LOG B1" & vbCrLf &
                                 "                  WHERE B1.BUSINESS_UNIT = A.BUSINESS_UNIT" & vbCrLf &
                                 "                    AND B1.REQ_ID = A.REQ_ID" & vbCrLf &
                                 "                    AND B1.LINE_NO= A1.LINE_NBR" & vbCrLf &
                                 "                    AND B1.PRICE= L.ISA_SELL_PRICE" & vbCrLf &
                                 "                 )" & vbCrLf &
                                 " ORDER BY A1.BUSINESS_UNIT, A1.REQ_ID, A1.LINE_NBR " & vbCrLf &
                                 ""

            Dim rdr As OleDbDataReader
            'Dim cmd As OleDbCommand = m_CN.CreateCommand
            connection.Open()

            'With cmd
            '    .CommandText = cSQL
            '    .CommandType = CommandType.Text
            'End With

            Dim Command As OleDbCommand = New OleDbCommand(cSQL, connection)
            Command.CommandTimeout = 120

            rdr = Command.ExecuteReader(CommandBehavior.CloseConnection)

            If rdr.HasRows Then
                Dim cKey As String
                Dim bNew As Boolean

                Dim sModifiedByID As String = ""

                Dim workOrderNo As String = ""
                Dim rfqEmailRecipient As String = ""

                Dim priceBlock As String = "N"

                Dim strBuyerDescr As String = ""
                Dim strBuyerEmail As String = ""
                Dim appLimit As Double = 0
                Dim strPriority As String = " "
                Dim lineStatus As String = " "
                Dim store As String = " "


                While rdr.Read
                    cKey = ""
                    bNew = True
                    boItem = Nothing

                    ' get the key for the current record
                    cKey = CType(rdr("BUSINESS_UNIT"), String).Trim.PadLeft(5, CType(" ", Char)) &
                    CType(rdr("REQ_ID"), String).Trim.PadLeft(10, CType(" ", Char))

                    lineStatus = CType(rdr("LINESTATUS"), String).Trim
                    ' check if current key exist or be in a new message instance
                    If m_colMsgs.Count > 0 Then
                        bNew = True
                        For Each oItm As QuotedNStkItem In m_colMsgs
                            If cKey = oItm.ID Then
                                boItem = oItm
                                bNew = False
                                Exit For
                            End If
                        Next
                    Else
                        bNew = True
                    End If

                    ' add a new instance of item object into our collection
                    ' object with the current key
                    If bNew Then
                        boItem = New QuotedNStkItem
                        boItem.ID = cKey

                        '' assign defined TO
                        'If m_extendedTO.Count > 0 Then
                        '    For Each sTO As String In m_extendedTO
                        '        If SDI.WinServices.Utility.IsValidEmailAdd(sTO) Then
                        '            boItem.TO &= sTO & ";"
                        '        End If
                        '    Next
                        'End If

                        ' assign defined CC
                        If m_extendedCC.Count > 0 Then
                            For Each sCC As String In m_extendedCC
                                If Utility.IsValidEmailAdd(sCC) Then
                                    boItem.CC &= sCC & ";"
                                End If
                            Next
                        End If

                        ' assign defined BCC
                        If m_extendedBCC.Count > 0 Then
                            For Each sBCC As String In m_extendedBCC
                                If Utility.IsValidEmailAdd(sBCC) Then
                                    boItem.BCC &= sBCC & ";"
                                End If
                            Next
                        End If

                        ' add into our collection object
                        m_colMsgs.Add(boItem)
                    End If

                    'get price block flag value
                    Try
                        If rdr("ISA_PRICE_BLOCK") Is DBNull.Value Then
                            priceBlock = "N"
                        Else
                            priceBlock = Convert.ToString(rdr("ISA_PRICE_BLOCK"))
                            If Trim(priceBlock) = "" Then
                                priceBlock = "N"
                            End If
                        End If
                    Catch ex As Exception
                        priceBlock = "N"
                    End Try
                    If Trim(priceBlock) = "" Then
                        priceBlock = "N"
                    End If

                    boItem.PriceBlockFlag = priceBlock
                    'Session("PriceBlockFlag") = boItem.PriceBlockFlag

                    ' get business unit ID (if not defined yet)
                    If Not (boItem.BusinessUnitID.Length > 0) Then
                        boItem.BusinessUnitID = CType(rdr("BUSINESS_UNIT"), String).Trim
                    End If
                    'WAL-533 get shipto and assign change by madhu
                    If Not (boItem.ShipTo.Length > 0) Then
                        boItem.ShipTo = CType(rdr("SHIPTO"), String).Trim
                    End If
                    'Set the zeus_site flag

                    If Not (boItem.Zeussiteflag.Length > 0) Then
                        boItem.Zeussiteflag = CType(rdr("ZEUS_SITE"), String).Trim
                    End If
                    ' get the first Business Unit OM available
                    If Not (boItem.BusinessUnitOM.Length > 0) Then
                        If Not (rdr("BUSINESS_UNIT_OM") Is System.DBNull.Value) Then
                            boItem.BusinessUnitOM = CType(rdr("BUSINESS_UNIT_OM"), String).Trim
                        End If
                    End If

                    ' get order ID (if not defined yet)
                    If Not (boItem.OrderID.Length > 0) Then
                        boItem.OrderID = CType(rdr("REQ_ID"), String).Trim
                        boItem.FormattedOrderID = boItem.OrderID    'default to show original order Id
                    End If

                    ' 2009.02.06; handle UNCC's change on Order Number being used by SDI vs original UNCC work order number
                    If boItem.OrderID.Length > 0 And
                       boItem.BusinessUnitOM.Length > 0 Then
                        If boItem.BusinessUnitOM.ToUpper = orderNoMapper.UNCC_BUSINESS_UNIT_OM Then
                            boItem.FormattedOrderID = orderNoMapper.FormatOrderNoToShow(sdiOrderNo:=boItem.OrderID,
                                                                                        unccOrderNo:=orderNoMapper.changeToUNCCOrderNo(orderNo:=boItem.OrderID))
                        End If
                    End If

                    ' get customer ID
                    If Not (boItem.CustomerID.Length > 0) Then
                        If Not (rdr("SOLD_TO_CUST_ID") Is System.DBNull.Value) Then
                            boItem.CustomerID = CType(rdr("SOLD_TO_CUST_ID"), String).Trim
                        End If
                    End If

                    ' get the sender email address
                    If Not (boItem.FROM.Length > 0) Then
                        If Not (rdr("ISA_NONSKREQ_EMAIL") Is System.DBNull.Value) Then
                            'If SDI.WinServices.Utility.IsValidEmailAdd(CType(rdr("ISA_NONSKREQ_EMAIL"), String)) Then
                            '    boItem.FROM = CType(rdr("ISA_NONSKREQ_EMAIL"), String).Trim
                            'End If
                            Dim arr As ArrayList = Utility.ExtractValidEmails(CType(rdr("ISA_NONSKREQ_EMAIL"), String).Trim)
                            If arr.Count > 0 Then
                                ' just pick up the very first valid email
                                boItem.FROM = CType(arr(0), String)
                            End If
                            arr = Nothing
                        End If
                    End If

                    ' grab the order origin
                    If Not (boItem.OrderOrigin.Length > 0) Then
                        Dim orderOrigin As String = ""
                        Try
                            orderOrigin = CStr(rdr("ORIGIN")).Trim.ToUpper
                        Catch ex As Exception
                        End Try
                        If (orderOrigin.Length > 0) Then
                            boItem.OrderOrigin = orderOrigin
                        End If
                    End If
                    'WAL-533 checking the wal00 bu condition inorder to avoid workorder passing empty in subject lines-madhu
                    If (boItem.OrderOrigin = "RFQ" Or boItem.BusinessUnitID = "WAL00") Then

                        workOrderNo = " "
                        Try
                            If Not (rdr("WORK_ORDER_ID") Is System.DBNull.Value) Then
                                workOrderNo = CStr(rdr("WORK_ORDER_ID")).Trim
                            End If
                        Catch ex As Exception
                            workOrderNo = " "
                        End Try

                        rfqEmailRecipient = " "
                        If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then

                            Dim arr As ArrayList = Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
                            If arr.Count > 0 Then
                                For Each sAdd As String In arr
                                    rfqEmailRecipient &= sAdd & ";"
                                Next
                                rfqEmailRecipient = rfqEmailRecipient.Substring(0, rfqEmailRecipient.Length - 1)
                            End If
                            arr = Nothing
                        End If
                    End If

                    store = " "
                    Try
                        If boItem.BusinessUnitID = "WAL00" Then
                            If Not (rdr("STORE") Is System.DBNull.Value) Then
                                store = CStr(rdr("STORE")).Split("-").LastOrDefault().Trim()
                            End If
                        End If
                    Catch ex As Exception
                        store = " "
                    End Try

                    ' get Buyer ID (if not defined yet)
                    If Not (boItem.BuyerId.Length > 0) Then
                        Try
                            If Not (rdr("DESCR") Is System.DBNull.Value) Then
                                boItem.BuyerId = CType(rdr("DESCR"), String).Trim
                            End If
                        Catch ex As Exception

                        End Try
                    End If

                    ' get Priority
                    If Trim(boItem.Priority) = "" Then
                        Try
                            If Not (rdr("ISA_PRIORITY_FLAG") Is System.DBNull.Value) Then
                                boItem.Priority = CType(rdr("ISA_PRIORITY_FLAG"), String).Trim
                            End If
                        Catch ex As Exception

                        End Try
                    End If

                    ' get Buyer E-mail (if not defined yet)
                    If Not (boItem.BuyerEmail.Length > 0) Then
                        Try
                            If Not (rdr("EMAILID") Is System.DBNull.Value) Then
                                boItem.BuyerEmail = CType(rdr("EMAILID"), String).Trim
                            End If
                        Catch ex As Exception

                        End Try
                    End If
                    boItem.Status = lineStatus
                    ' get the very first available VALID recipient of this message (if not yet)
                    If Not boItem.IsPrimaryRecipientExist Then
                        If lineStatus = "QTS" Then
                            ' get the employee ID
                            If Not (rdr("ISA_EMPLOYEE_ID") Is System.DBNull.Value) Then
                                boItem.EmployeeID = CType(rdr("ISA_EMPLOYEE_ID"), String).Trim
                            End If

                            ' get the addressee (name)
                            If Not (rdr("ISA_EMPLOYEE_NAME") Is System.DBNull.Value) Then
                                'boItem.Addressee = CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim
                                Dim cAddressee As String = Utility.FormatAddessee(CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim)
                                If Not (cAddressee.Trim.Length > 0) Then
                                    cAddressee = CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim
                                End If
                                boItem.Addressee = cAddressee
                            End If

                            ' get the email address
                            If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                                'If SDI.WinServices.Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                                '    boItem.TO = CType(rdr("ISA_EMPLOYEE_EMAIL"), String)
                                'End If
                                Dim arr As ArrayList = Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
                                If arr.Count > 0 Then
                                    For Each sAdd As String In arr
                                        boItem.TO &= sAdd & ";"
                                    Next
                                End If
                                arr = Nothing
                            End If
                            'INC0037100 - Email To for Needs approvals from Non stock email utility should come from the ISA_USERS1 based upon Zeus 2 site flag = 'Y' - Dhamo
                        Else
                            Dim strOrigApproverID As String = String.Empty
                            If boItem.BusinessUnitID = "WAL00" Or rdr("ZEUS_SITE") = "Y" Then
                                strOrigApproverID = rdr("APPROVER").trim
                            Else
                                strOrigApproverID = GetOriginalApprover(rdr("BUSINESS_UNIT_OM").trim, rdr("ISA_EMPLOYEE_ID").trim)

                            End If
                            If strOrigApproverID.Trim <> "" Then
                                Dim strSQLString As String = "SELECT FIRST_NAME_SRCH," & vbCrLf &
                   " LAST_NAME_SRCH," & vbCrLf &
                   " ISA_EMPLOYEE_EMAIL" & vbCrLf &
                   " FROM SDIX_USERS_TBL" & vbCrLf &
                   " WHERE ISA_EMPLOYEE_ID = '" & strOrigApproverID & "'"

                                Dim dtrAppReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

                                If dtrAppReader.HasRows() = True Then
                                    dtrAppReader.Read()
                                    boItem.EmployeeID = strOrigApproverID
                                    boItem.Addressee = dtrAppReader.Item("FIRST_NAME_SRCH") & " " & dtrAppReader.Item("LAST_NAME_SRCH")
                                    boItem.TO = dtrAppReader.Item("ISA_EMPLOYEE_EMAIL")
                                    dtrAppReader.Close()
                                Else
                                    dtrAppReader.Close()
                                End If
                            End If
                        End If

                        ' 3/26/2014 to accomodate Ascend Reqs (meaning IF ORIGIN IS "RFQ")
                        '   we'll have to override current "email TO", "addressee" and "employee" fields
                        '   - erwin
                        If (boItem.OrderOrigin = "RFQ") Then
                            If (rfqEmailRecipient.Length > 0) Then
                                boItem.TO = ""
                                Dim arr As ArrayList = Utility.ExtractValidEmails(rfqEmailRecipient)
                                If arr.Count > 0 Then
                                    For Each sAdd As String In arr
                                        boItem.TO &= sAdd & ";"
                                    Next
                                End If
                                arr = Nothing
                                ' since we won't have this addressee on our table, we'll use the email they provided as the addressee
                                If Trim(boItem.Addressee) = "" Then
                                    boItem.Addressee = boItem.TO.TrimEnd(";"c)
                                End If

                                '' as well as the employee Id
                                'boItem.EmployeeID = boItem.TO.TrimEnd(";"c)
                            End If
                        End If
                    Else
                        ' add this email address into the CC field if not same as the
                        ' primary recipient of this email
                        Dim cAdd As String = ""

                        If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                            'If SDI.WinServices.Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                            '    cAdd = CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim
                            'End If
                            Dim arr As ArrayList = Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
                            If arr.Count > 0 Then
                                For Each sAdd As String In arr
                                    cAdd &= sAdd & ";"
                                Next
                                cAdd = cAdd.Substring(0, cAdd.Length - 1)
                            End If
                            arr = Nothing
                        End If

                        If cAdd.Length > 0 And Not (cAdd = boItem.TO) Then
                            If boItem.CC.Length > 0 Then
                                If boItem.CC.IndexOf(cAdd) = -1 Then
                                    boItem.CC &= cAdd & ";"
                                End If
                            Else
                                boItem.CC &= cAdd & ";"
                            End If
                        End If
                    End If

                    ' accumulate unique OPRID_MODIFIED_BY field
                    ' we will use them just in case a valid primary recipient is not present
                    ' per BobD
                    sModifiedByID = ""
                    Try
                        If Not (rdr("OPRID_MODIFIED_BY") Is System.DBNull.Value) Then
                            sModifiedByID = CType(rdr("OPRID_MODIFIED_BY"), String).Trim
                        End If
                    Catch ex As Exception
                        sModifiedByID = ""
                    End Try
                    If sModifiedByID.Length > 0 Then
                        If boItem.BackupRecipientIDs.IndexOf(sModifiedByID) = -1 Then
                            boItem.BackupRecipientIDs.Add(sModifiedByID)
                        End If
                    End If

                    ' 3/26/2014 addition rule to handle Ascend reqs
                    '   Ascend's order in INTFC comes in with origin = 'RFQ' (instead of IOL or MOB) and work Order No comes from new header table SYSADM.PS_ISA_INTFC_H_SUP
                    '   - erwin
                    If Not (boItem.WorkOrderNumber.Length > 0) Then
                        boItem.WorkOrderNumber = workOrderNo
                    End If

                    If Not (boItem.Store.Length > 0) Then
                        boItem.Store = store
                    End If

                    ' New code for Approval Limit

                    If Not (rdr("APPROVAL_LIMIT") Is System.DBNull.Value) Then
                        boItem.ApprovalLimit = CDec(rdr("APPROVAL_LIMIT"))
                    Else
                        boItem.ApprovalLimit = 0
                    End If

                    If Not (rdr("ISA_CUSTINT_APPRVL") Is System.DBNull.Value) Then
                        If CType(rdr("ISA_CUSTINT_APPRVL"), String).Trim.ToUpper = "Y" Then
                            boItem.LineStatus = "QTC"
                        Else
                            boItem.LineStatus = "QTA"
                        End If
                    End If

                    If Trim(boItem.Priority) = "" Then
                        boItem.Priority = " "
                    End If

                    ' put back the item into our collection object
                    m_colMsgs(boItem.IndexInCollection) = boItem
                End While

                boItem = Nothing
            End If

            rdr.Close()

            rdr = Nothing
            'cmd = Nothing

            ' double check and/or search for primary recipient if not defined
            CheckSearchPrimaryRecipient()

            If connection.State = ConnectionState.Closed Then

            Else
                Try
                    connection.Close()
                Catch exCon As Exception

                End Try
            End If

            Return m_colMsgs.Count

        Catch ex As Exception
            Try
                connection.Close()
            Catch exErr As Exception

            End Try
            'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)

            m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
            SendAlertMessage(msg:=cHdr & ex.ToString)
            Return 0
        End Try

    End Function
    Public Shared Function GetOriginalApprover(ByVal strBU As String, ByVal strLastApprover As String) As String
        Dim strOrigApproverID As String = ""

        Try
            Dim ds As DataSet
            Dim strSQLstring As String


            strSQLstring = "SELECT isa_iol_apr_emp_id" & vbCrLf &
                   " FROM SDIX_USERS_APPRV" & vbCrLf &
                   " WHERE isa_employee_id = '" & strLastApprover & "'" & vbCrLf
            If strBU <> "I0W01" Then
                strSQLstring &= " AND business_unit = '" & strBU & "'"
            End If

            ds = ORDBData.GetAdapter(strSQLstring)
            If ds.Tables(0).Rows.Count > 0 Then
                strOrigApproverID = ds.Tables(0).Rows(0).Item("isa_iol_apr_emp_id").ToString
            End If

        Catch ex As Exception
        End Try

        Return strOrigApproverID
    End Function

    Private Shared Function GetWalmartApprover(ByVal order_no As String) As String
        Dim strOrigApproverID As String = ""

        Try
            Dim ds As DataSet
            Dim strSQLstring As String


            strSQLstring = "SELECT ISA_USER1" & vbCrLf &
                   " FROM PS_ISA_ORD_INTF_LN" & vbCrLf &
                   " WHERE order_no = '" & order_no & "'" & vbCrLf


            ds = ORDBData.GetAdapter(strSQLstring)
            If ds.Tables(0).Rows.Count > 0 Then
                strOrigApproverID = ds.Tables(0).Rows(0).Item("ISA_USER1").ToString
            End If

        Catch ex As Exception
        End Try

        Return strOrigApproverID
    End Function
    'Madhu-ZEUS-138-Adding the Zeus 2.0 Link to redirect to if the flag is turned on
    Public Shared Function GetURL(Optional ByVal cBusinessUnitOM As String = "", Optional ByVal LineStatus As String = "") As String
        Dim sRet As String = ""
        Dim finalurl As String = ""
        Dim IsZeus2 As String = ""
        Dim Zeus2 As Boolean = False
        Dim strquery As String = ""
        Dim strquery1 As String = ""
        Dim dtapprover As DataSet
        Dim sWebAppName As String = "ims.sdi.com:8080/sdiconnect/"
        Dim sCNString As String = m_CN.ConnectionString
        Dim strDBase As String = "STAR"
        If Len(sCNString) > 4 Then
            strDBase = UCase(Right(sCNString, 4))
        End If
        Try
            sWebAppName = ConfigurationManager.AppSettings("WebAppName")
            If Trim(sWebAppName) = "" Then
                sWebAppName = "ims.sdi.com:8080/sdiconnect/"
            End If
        Catch ex As Exception
            sWebAppName = "ims.sdi.com:8080/sdiconnect/"
        End Try

        Select Case strDBase
            Case "PROD", "SPRD"
                sRet = "https://www.sdizeus.com/"
            Case "SUAT", "SNBX"
                sRet = "http://zeustest.sdi.com:8083/ZEUSUAT/"
            Case "STST", "RPTG"
                sRet = "http://zeustest.sdi.com:8083/ZEUSRPTG/"
            Case "DEVL"
                sRet = "https://zeustest.sdi.com/"
            Case Else
                sRet = "http://zeustest.sdi.com:8083/ZEUSUAT/"
        End Select
        If LineStatus = "QTW" Then
            finalurl = sRet & "approveorder.aspx"
        Else
            finalurl = sRet & "Approvequote.aspx"
        End If


        Try

            strquery1 = "SELECT ZEUS_SITE FROM PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT= '" & cBusinessUnitOM & "'"
            IsZeus2 = ORDBData.GetScalar(strquery1)
            If IsZeus2 = "Y" Then
                Zeus2 = True
            End If
        Catch ex As Exception
        End Try

        If cBusinessUnitOM = "I0W01" Or Zeus2 = True Then
            Try
                strquery = "SELECT SITENAME,TEST_SITE,DEMO_SITE,PROD_SITE FROM SDIX_SITE_URL WHERE SITENAME IN ('ZEUS2','WAL')"
                dtapprover = ORDBData.GetAdapter(strquery)
            Catch ex As Exception
            End Try


            If Zeus2 Then
                Select Case strDBase
                    Case "DEVL"
                        sRet = dtapprover.Tables(0).Rows(0).Item("TEST_SITE")
                    Case "PROD", "SPRD"
                        sRet = dtapprover.Tables(0).Rows(0).Item("PROD_SITE")
                    Case Else
                        sRet = dtapprover.Tables(0).Rows(0).Item("DEMO_SITE")
                End Select
            Else
                Select Case strDBase
                    Case "DEVL"
                        sRet = dtapprover.Tables(0).Rows(1).Item("TEST_SITE")
                    Case "PROD", "SPRD"
                        sRet = dtapprover.Tables(0).Rows(1).Item("PROD_SITE")
                    Case Else
                        sRet = dtapprover.Tables(0).Rows(1).Item("DEMO_SITE")
                End Select
            End If
            If LineStatus = "QTW" Then
                finalurl = sRet & "needapprove"
            Else
                finalurl = sRet & "Approvequote"
            End If
        End If

        Return finalurl
    End Function

    Private Sub CheckSearchPrimaryRecipient()
        Dim cHdr As String = "QuoteNonStockProcessor.CheckSearchPrimaryRecipient: "
        Try
            If m_colMsgs.Count > 0 Then
                For Each boItem As QuotedNStkItem In m_colMsgs
                    If Not boItem.IsPrimaryRecipientExist Then

                        '
                        ' NO Primary Recipient
                        ' so let's try resolving it with backup employee IDs we gathered (other source - see above)
                        '

                        Dim sList As String = CreateListFromArray(boItem.BackupRecipientIDs)

                        If sList.Length > 0 Then
                            Dim sSQL As String = "SELECT ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME, ISA_EMPLOYEE_EMAIL " &
                                                 "FROM PS_ISA_USERS_TBL " &
                                                 "WHERE ISA_EMPLOYEE_ID IN (" & sList & ") "

                            Dim rdr As OleDbDataReader
                            Dim cmd As New OleDbCommand

                            With cmd
                                .CommandText = sSQL
                                .CommandType = CommandType.Text
                                .Connection = m_CN
                            End With

                            'm_eventLogger.WriteEntry(cHdr & "executing -> " & sSQL, EventLogEntryType.Information)
                            rdr = cmd.ExecuteReader

                            If rdr.HasRows Then
                                While rdr.Read
                                    If Not boItem.IsPrimaryRecipientExist Then
                                        ' should still use the OPRID_ENTERED_BY field value
                                        ' erwin
                                        '' get the employee ID
                                        'If Not (rdr("ISA_EMPLOYEE_ID") Is System.DBNull.Value) Then
                                        '    boItem.EmployeeID = CType(rdr("ISA_EMPLOYEE_ID"), String).Trim
                                        'End If

                                        ' get the addressee (name)
                                        If Not (rdr("ISA_EMPLOYEE_NAME") Is System.DBNull.Value) Then
                                            Dim cAddressee As String = Utility.FormatAddessee(CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim)
                                            If Not (cAddressee.Trim.Length > 0) Then
                                                cAddressee = CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim
                                            End If
                                            boItem.Addressee = cAddressee
                                        End If

                                        ' get the email address
                                        If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                                            'If SDI.WinServices.Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                                            '    boItem.TO = CType(rdr("ISA_EMPLOYEE_EMAIL"), String)
                                            'End If
                                            Dim arr As ArrayList = Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
                                            If arr.Count > 0 Then
                                                For Each sAdd As String In arr
                                                    boItem.TO &= sAdd & ";"
                                                Next
                                            End If
                                            arr = Nothing
                                        End If
                                    Else
                                        ' add this email address into the CC field if not same as the
                                        ' primary recipient of this email
                                        Dim cAdd As String = ""

                                        If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                                            'If SDI.WinServices.Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                                            '    cAdd = CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim
                                            'End If
                                            Dim arr As ArrayList = Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
                                            If arr.Count > 0 Then
                                                For Each sAdd As String In arr
                                                    cAdd &= sAdd & ";"
                                                Next
                                                cAdd = cAdd.Substring(0, cAdd.Length - 1)
                                            End If
                                            arr = Nothing
                                        End If

                                        If cAdd.Length > 0 And Not (cAdd = boItem.TO) Then
                                            If boItem.CC.Length > 0 Then
                                                If boItem.CC.IndexOf(cAdd) = -1 Then
                                                    boItem.CC &= cAdd & ";"
                                                End If
                                            Else
                                                boItem.CC &= cAdd & ";"
                                            End If
                                        End If
                                    End If
                                End While
                            Else
                                'm_eventLogger.WriteEntry(cHdr & "no record was found.", EventLogEntryType.Information)
                            End If

                            rdr.Close()
                            rdr = Nothing

                            cmd = Nothing
                        Else
                            ' list is EMPTY
                            ' so don't bother and don't do anything.
                            ' this message will just not have a proper TO email and addressee
                        End If

                    End If
                Next
            End If

        Catch ex As Exception
            'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)

            m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
    End Sub

    Private Function CreateListFromArray(ByVal arr As ArrayList) As String
        Dim sList As String = ""

        If Not (arr Is Nothing) Then
            If arr.Count > 0 Then
                ' build the list
                For Each sItem As String In arr
                    If sItem.Trim.Length > 0 Then
                        sList &= "'" & sItem & "',"
                    End If
                Next
                ' trim off the last "," char
                If sList.Length > 0 Then
                    sList = sList.Substring(0, sList.Length - 1)
                End If
            End If
        End If

        Return sList
    End Function

    Public Sub SendMessages(ByVal itmQuoted As QuotedNStkItem, Optional ByVal sBU As String = "")
        Dim cHdr As String = "QuoteNonStockProcessor.SendMessages: "
        Try
            If m_colMsgs.Count > 0 Then

                Dim eml As MailMessage
                Dim cmd As OleDbCommand
                Dim cSQL As String = ""
                Dim LineStatus As String = ""
                Dim MailAttachmentName As String()
                Dim MailAttachmentbytes As New List(Of Byte())()
                Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()

                'For Each itmQuoted As QuotedNStkItem In m_colMsgs
                eml = New System.Web.Mail.MailMessage

                ' init properties of the mail message
                'SDI-40628 Changing Mail id as walmartpurchasing@sdi.com from sdiexchange@sdi.com for Walmart BU.
                'SP-316 Purchasing Email from address change[By Vishalini]
                eml.From = getpurchasingEmailFrom(sBU)
                eml.To = ""
                eml.Cc = ""
                eml.Bcc = ""
                eml.Subject = ""
                eml.Body = ""

                LineStatus = itmQuoted.Status
                ' assign sender email address from item object 
                ' or assign the default automated sender
                If itmQuoted.FROM.Length > 0 Then
                    eml.From = itmQuoted.FROM
                Else
                    eml.From = m_defaultFROM
                End If

                ' assign recipient TO email address(es) from records
                ' and add any defined TOs within the configuration file
                If itmQuoted.TO.Length > 0 Then
                    eml.To = itmQuoted.TO
                End If
                If Trim(eml.To) <> "" Then
                    If Right(Trim(eml.To), 1) = ";" Then
                        ' OK, do nothing
                    Else
                        eml.To = Trim(eml.To) & ";"
                    End If
                End If

                If m_extendedTO.Count > 0 Then
                    For Each sTo As String In m_extendedTO
                        If Utility.IsValidEmailAdd(sTo) Then
                            eml.To &= sTo & ";"
                        End If
                    Next
                End If

                ' assign recipient CC email address(es)
                'Mythili -- INC0023448 Adding CC emails
                If sBU = "EMC00" Or sBU = "WAL00" Or sBU = "AMC00" Or sBU = "BOE00" Then
                    If itmQuoted.CC.Length > 0 And getDBName() Then
                        eml.Cc = itmQuoted.CC + ";" + getpurchasingEmailTo(sBU)
                    ElseIf itmQuoted.CC.Length = 0 And getDBName() Then
                        eml.Cc = getpurchasingEmailTo(sBU)
                    End If
                Else
                    If itmQuoted.CC.Length > 0 And getDBName() Then
                        eml.Cc = itmQuoted.CC
                    End If
                End If

                ' assign recipient BCC email address(es)
                If itmQuoted.BCC.Length > 0 Then
                    eml.Bcc = itmQuoted.BCC
                Else
                    eml.Bcc = "WebDev@sdi.com"
                End If

                ' assign the subject of this email
                ' or use the default subject line from the configuration file (most probably is)
                If itmQuoted.Subject.Length > 0 Then
                    eml.Subject = itmQuoted.Subject
                Else
                    eml.Subject = m_defaultSubject
                End If

                ' add the order ID on the subject line of this email
                ' - 2009.02.05; handle UNCC's change on order number
                'If itmQuoted.OrderID.Length > 0 Then eml.Subject &= " - " & itmQuoted.OrderID
                If itmQuoted.FormattedOrderID.Length > 0 Then
                    eml.Subject &= " - " & itmQuoted.FormattedOrderID
                ElseIf itmQuoted.OrderID.Length > 0 Then
                    eml.Subject &= " - " & itmQuoted.OrderID
                End If
                'WAL-533 Subject Line email change for walmart-change by madhu
                Try
                    If LineStatus = "QTW" Then
                        eml.Subject = "Order # "
                        eml.Subject &= itmQuoted.OrderID & " needs approval"
                    End If
                    Dim Business_unit As String = itmQuoted.BusinessUnitID
                    If Business_unit = "WAL00" Then
                        eml.Subject = "Action Needed - Approval Required" & " - Store #" & itmQuoted.Store & " Need approval" & " - WO #" & itmQuoted.WorkOrderNumber
                    End If
                Catch ex As Exception
                End Try

                ' for now, showing the work order # is synonymous to origin "RFQ"
                Dim bShowWorkOrderNo As Boolean = (itmQuoted.OrderOrigin = "RFQ")

                ' also, for now, we will hide the "approve via email" link when the origin is RFQ
                Dim bShowApproveViaEmailLink As Boolean = True
                If (itmQuoted.OrderOrigin = "RFQ") Then
                    bShowApproveViaEmailLink = False
                End If

                Dim strShowOrderId As String = ""
                If itmQuoted.FormattedOrderID.Length > 0 Then
                    strShowOrderId = itmQuoted.FormattedOrderID
                ElseIf itmQuoted.OrderID.Length > 0 Then
                    strShowOrderId = itmQuoted.OrderID
                End If

                cHdr = cHdr & "VR Start my code.  "

                Dim bIsBusUnitSDiExch As Boolean = False

                ' VR 11/20/2014 Eliminating the IF - everybody on SDiExchange now
                bIsBusUnitSDiExch = True
                cHdr = cHdr & "VR Middle my code.  "

                'BuildLineitem Grid for mail
                Dim dtgcart As DataGrid
                Dim SBstk As New StringBuilder
                Dim SWstk As New StringWriter(SBstk)
                Dim htmlTWstk As New HtmlTextWriter(SWstk)
                Dim dataGridHTML As String = String.Empty
                Dim dstcartSTK As New DataTable
                Dim StrWO1 As String = " "
                Dim BU As String = itmQuoted.BusinessUnitOM
                dstcartSTK = buildCartforemail(m_colMsgs, itmQuoted.OrderID, StrWO1, BU, itmQuoted.EmployeeID)
                If Trim(StrWO1) = "" Then
                    StrWO1 = " "
                End If
                itmQuoted.WorkOrderNumber = StrWO1

                If dstcartSTK.Rows.Count > 0 Then
                    dtgcart = New DataGrid
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
                End If

                ' override for RFQ origin to include (if provided) the work order # on the subject line
                '   - erwin 3/26/2014
                'As per the New UI And recent orders were from 2019 so commented the code
                'If (itmQuoted.OrderOrigin = "RFQ") Then
                '    If Trim(itmQuoted.WorkOrderNumber) <> "" Then  '  If (itmQuoted.WorkOrderNumber.Length > 0) Then
                '        eml.Subject &= " (Work Order # " & itmQuoted.WorkOrderNumber & ")"
                '    End If
                'End If

                Dim sWorkOrder As String = ""
                Try
                    sWorkOrder = itmQuoted.WorkOrderNumber.Trim
                Catch ex As Exception
                    sWorkOrder = ""
                End Try

                'Dim bIsPunchInBU As Boolean = (Me.PunchInBusinessUnitList.IndexOf(itmQuoted.BusinessUnitOM) > -1)
                Dim bIsPunchInBU As Boolean = False
                Dim dsPunchin As DataSet
                Try

                    Dim strSqlString As String = "SELECT * FROM SDIX_PUNCHIN_USERS where ISA_ASCEND_PROCESS= 'Y' AND ISA_BUSINESS_UNIT= '" & BU & "'"
                    dsPunchin = ORDBData.GetAdapter(strSqlString)
                    If dsPunchin.Tables(0).Rows.Count = 1 Then
                        bIsPunchInBU = True
                    End If

                Catch ex As Exception

                End Try

                Dim PI_SDI As String = String.Empty
                Dim PI As String = String.Empty
                Dim ContentSDI As String = String.Empty
                Dim Content As String = String.Empty
                Dim strOrderTotal As String = ""

                If LineStatus = "QTW" Then
                    Dim cGetNSTKOnly As Boolean = False
                    Dim strType As String = "approve"
                    Dim intLnID As Integer
                    m_siteCurrency = sdiMultiCurrency.getSiteCurrency(BU)
                    Dim concatenatedLineNumbers As String = ""
                    Dim LnrLst As String = ""

                    Try
                        If dstcartSTK IsNot Nothing Then
                            If dstcartSTK.Rows.Count > 0 Then
                                For Each dr As DataRow In dstcartSTK.Rows

                                    If dr("LN") IsNot Nothing AndAlso dr("LN").ToString().Trim() <> "" Then
                                        intLnID = dr("LN")
                                        Try
                                            concatenatedLineNumbers &= intLnID & ","
                                        Catch ex As Exception

                                        End Try
                                    End If
                                Next
                                ' Remove the trailing comma
                                If concatenatedLineNumbers <> "" Then
                                    concatenatedLineNumbers = concatenatedLineNumbers.Substring(0, concatenatedLineNumbers.Length - 1)
                                End If
                                Try
                                    If concatenatedLineNumbers = "" Then
                                        If IsAEES(BU) Then
                                            strOrderTotal = "$" & OrderTotals.getOrderTotalMail(BU, itmQuoted.OrderID, m_siteCurrency, cGetNSTKOnly, strType, concatenatedLineNumbers) & " " & m_siteCurrency.Id.ToString
                                        Else
                                            strOrderTotal = "$" & OrderTotals.getOrderTotalMail(BU, itmQuoted.OrderID, m_siteCurrency, cGetNSTKOnly, strType, concatenatedLineNumbers) & " " & m_siteCurrency.Id.ToString

                                        End If
                                    Else
                                        If IsAEES(BU) Then
                                            strOrderTotal = "$" & OrderTotals.getOrderTotalMail(BU, itmQuoted.OrderID, m_siteCurrency, cGetNSTKOnly, strType, concatenatedLineNumbers) & " " & m_siteCurrency.Id.ToString
                                        Else
                                            strOrderTotal = "$" & OrderTotals.getOrderTotalMail(BU, itmQuoted.OrderID, m_siteCurrency, cGetNSTKOnly, strType, concatenatedLineNumbers) & " " & m_siteCurrency.Id.ToString
                                        End If
                                    End If
                                Catch ex As Exception
                                End Try


                            End If

                        End If

                    Catch ex As Exception
                    End Try
                End If
                If bIsPunchInBU Then
                    'SdiExchange
                    'WorkOrder should show for all the BU's
                    bShowWorkOrderNo = True
                    'PI_SDI = LETTER_CONTENT_PI_SDiExchange
                    eml.Body = "<HTML>" &
                                        BodyStyle() &
                                        AddNoRecepientExistNote(eml.To) &
                                        EmailBodyHead(itmQuoted.Addressee, itmQuoted.OrderID, BU, LineStatus, itmQuoted.Zeussiteflag, itmQuoted.EmployeeID) &
                                        FormHTMLLink(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM, LineStatus, bShowApproveViaEmailLink) &
                                       FormHTMLQouteInfo(itmQuoted.Addressee, strShowOrderId, bShowWorkOrderNo, sWorkOrder, itmQuoted.Priority, LineStatus, strOrderTotal) &
                                        PositionGrid(dstcartSTK, LineStatus, BU) &
                                        EmailBodyClosure() &
                                        bindFooterMail(BU) &
                                          "</table>" &
                                    "</BODY>" &
                               "</HTML>"


                Else
                    bShowApproveViaEmailLink = True
                    bShowWorkOrderNo = True
                    Dim LetterHead As String = ""

                    eml.Body = "<HTML>" &
                                    BodyStyle() &
                                        AddNoRecepientExistNote(eml.To) &
                                        EmailBodyHead(itmQuoted.Addressee, itmQuoted.OrderID, BU, LineStatus, itmQuoted.Zeussiteflag, itmQuoted.EmployeeID) &
                                         FormHTMLLink(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM, LineStatus, bShowApproveViaEmailLink) &
                                       FormHTMLQouteInfo(itmQuoted.Addressee, strShowOrderId, bShowWorkOrderNo, sWorkOrder, itmQuoted.Priority, LineStatus, strOrderTotal) &
                                        PositionGrid(dstcartSTK, LineStatus, BU) &
                                        EmailBodyClosure() &
                                        bindFooterMail(BU) &
                                                                "</table>" &
                                    "</BODY>" &
                               "</HTML>"
                End If

                cHdr = cHdr & "VR End my code.  'eml.Body' is: " & eml.Body.ToString()
                ' we need to check for a blank TO field and should return (send) this
                ' auto mail to the sender's attention.  Besides we already added the notice for
                ' the body of message
                If Not (eml.To.Trim.Length > 0) Then
                    eml.To = eml.From
                End If

                ' check if there's no valid recepient still, then we need to send this to 
                '   the default "no valid recepient" recepient based off of our config file.
                If Not (eml.To.Trim.Length > 0) Then
                    If m_defaultToRecepient.Count > 0 Then
                        If Trim(eml.To) <> "" Then
                            If Right(Trim(eml.To), 1) = ";" Then
                                ' OK, do nothing
                            Else
                                eml.To = Trim(eml.To) & ";"
                            End If
                        End If
                        For Each sTo As String In m_defaultToRecepient
                            If Utility.IsValidEmailAdd(sTo) Then
                                eml.To &= sTo & ";"
                            End If
                        Next
                    End If
                End If

                ' email is of HTML format
                eml.BodyFormat = Web.Mail.MailFormat.Html

                ' quick fix for Ascend
                '   take off CC since Ascend orders were being put in into INTFC table as "ASCENDCA" and it's email address is the CC
                '   that customer is complaining about - erwin 2014.04.29
                If (itmQuoted.OrderOrigin = "RFQ") Then
                    eml.Cc = ""
                End If

                Dim sCNString As String = m_CN.ConnectionString
                Dim strDBase As String = "STAR"
                If Len(sCNString) > 4 Then
                    strDBase = UCase(Right(sCNString, 4))
                End If
                Try
                    If Not getDBName() Then
                        'WAL-533 Subject Line email change for walmart - change by madhu
                        Dim Business_Unit1 As String = itmQuoted.BusinessUnitID
                        'This change is speicific for walmart so including the BU Condition
                        If Business_Unit1 = "WAL00" Then
                            eml.Subject = "TEST ZEUS - Action Needed - Approval Required" & " - Store #" & itmQuoted.Store & " - WO #" & itmQuoted.WorkOrderNumber
                        Else
                            eml.Subject = " TEST ZEUS - " & eml.Subject
                        End If
                        If itmQuoted.BusinessUnitOM = "I0635" Then
                            If Not (eml.To.Trim.Length > 0) Then
                                eml.To = eml.From
                            End If

                            ' check if there's no valid recepient still, then we need to send this to 
                            '   the default "no valid recepient" recepient based off of our config file.
                            If Not (eml.To.Trim.Length > 0) Then
                                If m_defaultToRecepient.Count > 0 Then
                                    If Trim(eml.To) <> "" Then
                                        If Right(Trim(eml.To), 1) = ";" Then
                                            ' OK, do nothing
                                        Else
                                            eml.To = Trim(eml.To) & ";"
                                        End If
                                    End If
                                    For Each sTo As String In m_defaultToRecepient
                                        If Utility.IsValidEmailAdd(sTo) Then
                                            eml.To &= sTo & ";"
                                        End If
                                    Next
                                End If
                            End If
                            eml.To = eml.To + ";" + "webdev@sdi.com"
                        Else
                            eml.To = "webdev@sdi.com"
                        End If

                        eml.Cc = ""
                        eml.Bcc = ""
                    Else
                    End If
                Catch ex As Exception
                End Try
                'If Trim(itmQuoted.Priority) <> "" Then
                '    If Trim(itmQuoted.Priority) = "R" Then
                '        eml.Subject = eml.Subject & " - PRIORITY"
                '    End If
                'End If
                ' send this email
                Try

                    SendLogger(eml.Subject, eml.Body, "QUOTEAPPROVAL", "Mail", eml.To, eml.Cc, eml.Bcc, itmQuoted.BusinessUnitID, itmQuoted.BusinessUnitOM)
                    sendNotification(itmQuoted.EmployeeID, eml.Subject, itmQuoted.OrderID, itmQuoted.BusinessUnitOM, itmQuoted.Priority, LineStatus)
                    sendWebNotification(itmQuoted.EmployeeID, eml.Subject, itmQuoted.Priority, LineStatus)
                Catch ex As Exception

                End Try

                If itmQuoted.EmployeeID <> "MAIKU49404" Then

                    Dim strOraSelectQuery As String = "select * from SYSADM8.PS_ISA_ORD_INTF_LN A where A.ORDER_NO = '" & itmQuoted.OrderID & "' AND A.ISA_LINE_STATUS in ('QTS','QTW') AND NOT EXISTS (SELECT 'X' FROM PS_ISA_REQ_EML_LOG B1 WHERE B1.REQ_ID = A.ORDER_NO AND B1.LINE_NO= A.ISA_INTFC_LN AND B1.PRICE= A.ISA_SELL_PRICE)"
                    Dim dsOrdLnItems As DataSet = GetAdapter(strOraSelectQuery)

                    If dsOrdLnItems.Tables(0).Rows.Count > 0 Then

                        For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows
                            Try

                                Dim strOraSelectQuery1 As String = "select * from PS_ISA_REQ_EML_LOG A where A.REQ_ID = '" & itmQuoted.OrderID & "' AND LINE_NO= '" & dataRowMain("ISA_INTFC_LN") & "'"
                                Dim ds As DataSet = GetAdapter(strOraSelectQuery1)

                                If ds.Tables(0).Rows.Count() = 0 Then
                                    ' build insert SQL command
                                    cSQL =
                            "INSERT INTO PS_ISA_REQ_EML_LOG " &
                            "(BUSINESS_UNIT, REQ_ID, ISA_RECIPIENT, ISA_SENDER, ISA_SUBJECT, EMAIL_DATETIME, LINE_NO, PRICE) " &
                            "VALUES " &
                            "(" &
                                "'" & CType(IIf(itmQuoted.BusinessUnitID.Length > 0, itmQuoted.BusinessUnitID, "."), String) & "', " &
                                "'" & CType(IIf(itmQuoted.OrderID.Length > 0, itmQuoted.OrderID, "."), String) & "', " &
                                "'" & "TO=" & eml.To & "CC=" & eml.Cc & "BCC=" & eml.Bcc & "', " &
                                "'" & CType(IIf(eml.From.Length > 0, eml.From, "."), String) & "', " &
                                "'" & CType(IIf(eml.Subject.Length > 0, eml.Subject, "."), String) & "', " &
                                "TO_DATE('" & System.DateTime.Now.ToString & "','MM/DD/YYYY HH:MI:SS AM'), " &
                                "'" & dataRowMain("ISA_INTFC_LN") & "'," &
                               "'" & dataRowMain("ISA_SELL_PRICE") & "'" &
                            ")"

                                Else
                                    cSQL = "UPDATE PS_ISA_REQ_EML_LOG set PRICE='" & dataRowMain("ISA_SELL_PRICE") & "' where REQ_ID= '" & itmQuoted.OrderID & "' and LINE_NO= '" & dataRowMain("ISA_INTFC_LN") & "'"
                                End If
                                'Dim rowsaffected As Int16 = ExecNonQuery(cSQL)
                                If m_CN.State = ConnectionState.Open Then

                                Else
                                    m_CN.Open()
                                End If
                                ' create a new instance of the command object
                                cmd = New OleDbCommand(cmdText:=cSQL, connection:=m_CN)
                                cmd.CommandType = CommandType.Text

                                ' execute SQL statement againts the connection object
                                Try
                                    cmd.ExecuteNonQuery()
                                Catch ex As Exception
                                    'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
                                    Try
                                        m_CN.Close()
                                    Catch exErr2 As Exception

                                    End Try
                                    m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
                                End Try
                            Catch ex As Exception

                            End Try
                        Next
                    End If
                    ' this code is for UNCC buyer tracking purposes
                    '   let's create a copy and send it to sender.
                    '   - erwin 20081022
                    Try
                        ' look for this email address for the sender - FacilityMaintNonStoc@sdi.com
                        '   recipient will be sender, CC will be blank, but BCC will stay as is
                        If eml.From.ToUpper.IndexOf("FACILITYMAINTNONSTOC") > -1 Then
                            eml.To = eml.From
                            eml.Cc = ""
                            eml.Subject &= " (copy)"
                            ''System.Web.Mail.SmtpMail.Send(message:=eml)
                            ''SDIEmailService.EmailUtilityServices("MailandStore", "madhuvanthy.u@avasoft.biz", "madhuvanthy.u@avasoft.biz", eml.Subject, String.Empty, String.Empty, eml.Body, "SDIERR", MailAttachmentName, MailAttachmentbytes.ToArray())
                            SendLogger(eml.Subject, eml.Body, "QUOTEAPPROVAL", "Mail", eml.To, eml.Cc, eml.Bcc, itmQuoted.BusinessUnitID, itmQuoted.BusinessUnitOM)

                        End If
                    Catch ex As Exception
                        ' just ignore
                    End Try
                    'Next

                    If Not (cmd Is Nothing) Then
                        cmd.Dispose()
                    End If
                    Try
                        m_CN.Close()
                    Catch ex111 As Exception

                    End Try

                End If
            End If

        Catch ex As Exception
            'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)

            Try
                m_CN.Close()
            Catch ex112 As Exception

            End Try
            m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
            SendAlertMessage(msg:=cHdr & ex.ToString)
            'SendLogger("Quote Non Stock New Utility - Dev", ex, "ERROR")
        End Try
    End Sub
    Public Shared Function BodyStyle() As String
        Dim strStyle As String = ""

        Try
            strStyle &= "<HEAD>"
            strStyle &= "<meta charset='UTF-8'>"
            strStyle &= "<meta name='viewport' content='width=device-width, initial-scale=1.0'>"
            strStyle &= "</HEAD>"
            strStyle &= "<BODY style='background-color: #ebebeb; font-family: Arial;'> "
            strStyle &= "<style>"
            strStyle &= "@media only screen and (max-width: 840px) {"
            strStyle &= ".table-grid {"
            strStyle &= "display: grid;}"
            strStyle &= ".table-grid td {"
            strStyle &= "width: 100% !important;}}"
            strStyle &= "</style>"
            strStyle &= "<table style='background-color:#ffffff; margin:auto; border-collapse:collapse' width='100%' align='center' border='0' cellpadding='0' cellspacing='0'>"

        Catch ex As Exception
        End Try
        Return strStyle
    End Function

    Public Shared Function EmailBodyHead(ByVal IsaEmployeeName As String, ByVal orderNo As String, ByVal Bu As String, ByVal Linestatus As String, ByVal Zeussite As String, ByVal Employeeid As String) As String
        Dim OrderNum As String = orderNo
        Dim strBu As String = Bu
        Dim bodyHead As String = ""
        Dim img = ConfigurationSettings.AppSettings("ZeusMailImg")
        Dim format As New System.Globalization.CultureInfo("en-US", True)

        Try
            bodyHead &= "<thead>"
            bodyHead &= "<tr style='padding: 20px 0px; background-color: #151723;'>"
            bodyHead &= "<th style='text-align:left;' colspan='2'>"

            bodyHead &= "<img src=" & img & " alt='sdi-logo' style='width: 200px; padding: 14px 24px;'>"
            bodyHead &= "</th>"
            bodyHead &= "</tr>"
            bodyHead &= "</thead>"
            bodyHead &= "<tbody>"
            bodyHead &= "<tr>"
            bodyHead &= "<td style='padding: 0px 24px;'>"
            bodyHead &= "<table style='width: 100%; border-collapse: collapse;'>"
            bodyHead &= "<tbody>"
            bodyHead &= "<tr style='font-size: 16px;'>"
            bodyHead &= "<td colspan='2'>"
            bodyHead &= "<p style='font-size:18px;margin-bottom:10px;font-weight:bold;margin-top: 0;padding-top: 24px;'>"
            Dim strappName As String = ""
            Dim strSQLstring As String = ""
            Dim IsaUser1 As String = ""
            If Linestatus = "QTW" Then
                Try
                    If Zeussite = "Y" Or strBu = "I0W01" Then
                        IsaUser1 = GetWalmartApprover(OrderNum)
                    Else
                        IsaUser1 = GetOriginalApprover(strBu, Employeeid)
                    End If
                Catch ex As Exception
                End Try
            Else
                IsaUser1 = Employeeid
            End If
            Try
                strSQLstring = "SELECT FIRST_NAME_SRCH," & vbCrLf &
                " LAST_NAME_SRCH" & vbCrLf &
                " FROM PS_ISA_USERS_TBL" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & IsaUser1 & "'"

                Dim dtrAppReader As OleDbDataReader = GetReader(strSQLstring)
                If dtrAppReader.HasRows() = True Then
                    dtrAppReader.Read()
                    strappName = dtrAppReader.Item("FIRST_NAME_SRCH") & " " & dtrAppReader.Item("LAST_NAME_SRCH")
                    Dim txtInfoLocal As TextInfo = New CultureInfo("en-US", False).TextInfo
                    strappName = txtInfoLocal.ToTitleCase(strappName.ToLower())
                    bodyHead &= " Hello " & strappName & "!"
                    dtrAppReader.Close()
                Else
                    dtrAppReader.Close()
                End If

            Catch ex As Exception
            End Try


            bodyHead &= "</p>"
        Catch ex As Exception
        End Try
        Return bodyHead
    End Function
    Public Function EmailBodyClosure()
        Dim strclose As String = ""
        strclose &= "<tr style = 'font-size: 14px;'>"
        strclose &= "<td style='width: 41%; padding: 0px;'>"
        strclose &= "<p style='font-weight: 500; margin: 40px 0px 6px 0px;'>Thanks,</p>"
        strclose &= "</td>"
        strclose &= "</tr>"
        strclose &= "<tr style = 'font-size: 14px;' >"
        strclose &= "<td style='width: 41%; padding: 0px;'>"
        strclose &= "<p style='font-weight: 500;white-space : nowrap; margin: 4px 0px 40px 0px;'>SDI Customer Care</p>"
        strclose &= "</td>"
        strclose &= "</tr>"
        Return strclose
    End Function
    Public Function bindFooterMail(BU As String) As String
        Dim strFooter As String = ""
        Try
            Dim strQuery As String = "SELECT * FROM SDIX_BU_CONTACT_DETAILS WHERE BUSINESS_UNIT = '" & BU & "' OR (BUSINESS_UNIT = 'SDI' AND NOT " & "EXISTS (SELECT 1 FROM SDIX_BU_CONTACT_DETAILS WHERE BUSINESS_UNIT = '" & BU & "'))"
            Dim footerDS As DataSet = ORDBData.GetAdapter(strQuery)

            If footerDS IsNot Nothing AndAlso footerDS.Tables.Count > 0 AndAlso footerDS.Tables(0).Rows.Count > 0 Then
                Dim phoneNum As String = footerDS.Tables(0).Rows(0)("PHONE_NUM").ToString().Trim()
                Dim emailID As String = footerDS.Tables(0).Rows(0)("EMAIL_ID").ToString().Trim()
                Dim visibility As String = If(Not IsDBNull(footerDS.Tables(0).Rows(0)("PHONE_EMAIL_VISIBLE")), footerDS.Tables(0).Rows(0)("PHONE_EMAIL_VISIBLE").ToString(), "")

                Dim strPhone As String = If(phoneNum <> "" AndAlso visibility.Contains("P"), "<a href='tel:" & phoneNum & "' style='color: blue; margin: 0; text-decoration: none;'>Call us @ " & phoneNum & "</a> ", "")
                Dim strEmail As String = If(emailID <> "" AndAlso visibility.Contains("E"), "<a href='mailto:" & emailID & "' style ='color: blue; margin: 0; text-decoration: none; margin-right: 12px;'> Contact SDI customer care </a> ", "")

                If strPhone <> "" OrElse strEmail <> "" Then
                    strFooter = "<tfoot><tr><td style='background-color: #F8F8F8; -webkit-print-color-adjust: exact; padding: 10px 5px; font-size: 12px; text-align: center;' colspan='2'>" & strPhone & If(strPhone <> "" AndAlso strEmail <> "", "<span style='border-right:2px solid #C2C2C2; margin:0px 10px;'></span>", "") & strEmail & "</td></tr></tfoot>"
                End If
            End If
        Catch ex As Exception
            ' Handle exception if needed
        End Try
        Return strFooter
    End Function
    'IPM-18-Madhu-Modifyingg the title for IPM
    'INC0042603  - Changed .net maui push notification since the Xamarin push notification is outdated.
    Public Sub sendNotification(ByVal Session_UserID As String, ByVal subject As String, ByVal orderNo As String, Optional ByVal strbu As String = " ", Optional priority As String = "", Optional LineStatus As String = "", Optional ByVal redirectscreen As String = " ", Optional ByVal CurrentApprover As String = " ")
        Dim Response As String = String.Empty
        Dim URL1 As String = String.Empty
        Try
            Try
                If LineStatus = "QTW" And Trim(priority) = "R" Then
                    subject = subject & " " & "*PRIORITY*"
                End If
            Catch ex As Exception
            End Try
            Try
                If strbu = "I0W01" Then
                    URL1 = ConfigurationManager.AppSettings("WalmartPushNotification")
                Else
                    URL1 = ConfigurationManager.AppSettings("PushNotification")
                End If
            Catch ex As Exception
            End Try
            Dim client = New HttpClient()
            client.DefaultRequestHeaders.Accept.Clear()
            client.DefaultRequestHeaders.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
            Dim notification As NotificationBO = New NotificationBO()
            notification.subject = subject
            notification.SessionUser = Session_UserID
            notification.orderno = orderNo
            notification.redirectscreen = redirectscreen
            notification.ApproverName = CurrentApprover
            Dim Serializedparameter = JsonConvert.SerializeObject(notification)
            Dim res As HttpResponseMessage = client.PostAsync(URL1, New StringContent(Serializedparameter, Encoding.UTF8, "application/json")).Result
            Try
                If res.StatusCode = HttpStatusCode.OK Then
                    Response = res.Content.ReadAsStringAsync().Result
                Else
                    Response = "Error"
                End If
            Catch ex As Exception
            End Try
        Catch ex As Exception
            Dim d = ex.Message
        End Try
    End Sub
    'INC0042603  - Changed .net maui push notification since the Xamarin push notification is outdated.

    Public Class NotificationBO

        Public Property SessionUser As String
        Public Property subject As String
        Public Property orderno As String
        Public Property redirectscreen As String
        Public Property ApproverName As String = " "

    End Class
    Public Sub sendWebNotification(ByVal Session_UserID As String, ByVal subject As String, Optional ByVal priority As String = "", Optional Linestatus As String = "")

        Try
            If Linestatus = "QTW" And Trim(priority) = "R" Then
                subject = subject & " " & "*PRIORITY*"
            End If
        Catch ex As Exception

        End Try
        Try
            Dim _notificationResult As New DataSet
            Dim notificationSQLStr = "select max(NOTIFY_ID) As NOTIFY_ID from SDIX_NOTIFY_QUEUE where USER_ID='" + Session_UserID + "'"
            _notificationResult = ORDBData.GetAdapter(notificationSQLStr)
            'A1QA-235 Change for Notification Id[Change By vishalini]
            Dim NotifyID As Int64 = 1
            If _notificationResult.Tables.Count > 0 Then
                Try
                    NotifyID = _notificationResult.Tables(0).Rows(0).Item("NOTIFY_ID")
                    NotifyID = NotifyID + 1
                Catch ex As Exception
                End Try
            End If

            Dim strSQLstring As String = "INSERT INTO SDIX_NOTIFY_QUEUE" & vbCrLf &
        " (NOTIFY_ID, NOTIFY_TYPE, USER_ID,DTTMADDED, STATUS,LINK, HTMLMSG, ATTACHMENTS, TITLE) VALUES ('" & NotifyID & "'," & vbCrLf &
        " 'AQO'," & vbCrLf &
        " '" & Session_UserID & "'," & vbCrLf &
        " sysdate," & vbCrLf &
        " 'N'," & vbCrLf &
         " ' ',' ',' '," & vbCrLf &
        " '" & subject & "')" & vbCrLf
            Try
                Dim rowsaffected As Integer
                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            Catch ex As Exception

            End Try
        Catch ex As Exception
        End Try
    End Sub

    Private Function FormHTMLQouteInfo(ByVal cAddressee As String, ByVal cOrderID As String, Optional ByVal bIsShowWorkOrderNo As Boolean = False,
                Optional ByVal cWorkOrderNo As String = "", Optional ByVal strPriority As String = "", Optional ByVal LineStatus As String = "", Optional ByVal ordtotal As String = "") As String

        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLQouteInfo: "

        Dim strOrderPriorty As String = " "
        If Trim(strPriority) <> " " Then
            If Trim(strPriority) = "R" Then
                strOrderPriorty = "*PRIORITY*"
            Else
                strOrderPriorty = ""
            End If
        End If

        If String.IsNullOrEmpty(cOrderID) Then
            cOrderID = "-"
        End If

        If String.IsNullOrEmpty(cWorkOrderNo) Then
            cWorkOrderNo = "-"
        End If

        If String.IsNullOrEmpty(cAddressee) Then
            Try
                If Not (cAddressee.Trim.Length > 0) Then
                    Dim parts() As String = cAddressee.Split(","c)
                    cAddressee = parts(1).Trim() & " " & parts(0).Trim()
                Else
                    cAddressee = "-"
                End If
            Catch ex As Exception
            End Try
        End If


        Try

            Dim cInfoHTML As String = ""
            Dim additionalInfoHTML As String = ""

            cInfoHTML &= "<tr style='font-size: 13px;'>"
            cInfoHTML &= "<td style='padding: 0px 12px;' colspan='2'>"
            cInfoHTML &= "<table style='width: 100%; border-collapse: collapse;'>"
            cInfoHTML &= "<tbody>"

            Try
                If LineStatus = "QTW" Then
                    additionalInfoHTML &= "<tr class='table-grid'>"
                    additionalInfoHTML &= "<td>"
                    additionalInfoHTML &= "<p style='font-weight: 600; margin: 10px 0px 14px 0px;'> Order # : <span style='color: #595959; font-weight: 500;'>" & cOrderID & "</span> </p>"
                    additionalInfoHTML &= "</td>"
                    Try
                        If bIsShowWorkOrderNo Then
                            additionalInfoHTML &= "<td>"
                            additionalInfoHTML &= "<p style='font-weight: 600; margin: 10px 0px 14px 0px;'> Work Order : <span style='color: #595959; font-weight: 500;'>" & cWorkOrderNo & "</span> </p>"
                            additionalInfoHTML &= "</td>"
                        End If
                    Catch ex As Exception
                    End Try
                    additionalInfoHTML &= "</tr>"
                    additionalInfoHTML &= "<tr class='table-grid'>"
                    additionalInfoHTML &= "<td>"
                    additionalInfoHTML &= "<p style='font-weight: 600; margin: 10px 0px 14px 0px;'> Order Total : <span style='color: #595959; font-weight: 500;'>" & ordtotal & "</span> </p>"
                    additionalInfoHTML &= "</td>"
                    If Trim(strPriority) = "R" Then
                        additionalInfoHTML &= "<td>"
                        additionalInfoHTML &= "<p style='font-weight: 600; margin: 10px 0px 14px 0px;'> Priority : <span style='color: #595959; font-weight: 500;'>" & strOrderPriorty & "</span> </p>"
                        additionalInfoHTML &= "</td>"
                    End If
                    additionalInfoHTML &= "</tr>"


                Else
                    additionalInfoHTML &= "<tr class='table-grid'>"
                    additionalInfoHTML &= "<td>"
                    additionalInfoHTML &= "<p style='font-weight: 600; margin: 10px 0px 14px 0px;'> Date : <span style='color: #595959; font-weight: 500;'>" & DateTime.Now.ToString(format:="MM/dd/yyyy") & "</span> </p>"
                    additionalInfoHTML &= "</td>"
                    additionalInfoHTML &= "<td>"
                    additionalInfoHTML &= "<p style='font-weight: 600; margin: 10px 0px 14px 0px;'> Order # : <span style='color: #595959; font-weight: 500;'>" & cOrderID & "</span> </p>"
                    additionalInfoHTML &= "</td>"
                    additionalInfoHTML &= "</tr>"
                    additionalInfoHTML &= "<td>"
                    Try
                        If bIsShowWorkOrderNo Then
                            additionalInfoHTML &= "<p style='font-weight: 600; margin: 10px 0px 14px 0px;'> Work Order : <span style='color: #595959; font-weight: 500;'>" & cWorkOrderNo & "</span> </p>"
                            additionalInfoHTML &= "</td>"
                        End If
                    Catch ex As Exception
                    End Try
                    additionalInfoHTML &= "</tr>"
                End If
            Catch ex As Exception
            End Try

            cInfoHTML &= additionalInfoHTML

            cInfoHTML &= "</tbody>"
            cInfoHTML &= "</table>"
            cInfoHTML &= "</td>"
            cInfoHTML &= "</tr>"


            Return cInfoHTML
        Catch ex As Exception
            m_logger.WriteVerboseLog(cHdr & ".  Error  " & ex.ToString)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
        'Return cInfoHTML
    End Function

    Private Function FormHTMLLink(ByVal cOrderID As String, ByVal cEmployeeID As String, ByVal cBusinessUnitOM As String, ByVal LineStatus As String, Optional ByVal bShowLink As Boolean = True) As String
        Dim cLink As String = ""
        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLLink: "
        If bShowLink Then
            Try
                'Dim m_cURL1 As String = "http://" & ConfigurationManager.AppSettings("WebAppName") & "Approvequote.aspx"
                'Dim m_cURL1 As String = GetURL() & "Approvequote.aspx"
                Dim m_cURL1 As String = GetURL(cBusinessUnitOM, LineStatus)

                Dim boEncrypt As New Encryption64

                Dim cParam As String = "?fer=" & boEncrypt.Encrypt(cOrderID, m_cEncryptionKey) &
                                       "&op=" & boEncrypt.Encrypt(cEmployeeID, m_cEncryptionKey) &
                                       "&xyz=" & boEncrypt.Encrypt(cBusinessUnitOM, m_cEncryptionKey) &
                                       "&HOME=N"

                If LineStatus = "QTW" Then
                    cLink = "<p style='color: #000; margin: 0px 0px 18px 0px; line-height: 24px;'>" &
                    "The below referenced order has been requested by " & cEmployeeID & " and needs your approval.Click the " &
                    "<a href=""" & m_cURL1 & cParam & """ style='text-decoration: none; color: #3090FF;'>link</a> " &
                    "or select the ""Approve Orders"" menu option in SDI ZEUS to approve or reject the order." &
                    "</p>"
                Else
                    cLink = "<p style='color: #000; margin: 0px 0px 18px 0px; line-height: 24px;'> " &
                    "The below referenced order contains items that required a price quote before processing.To view quoted price either click the " &
                    "<a href=""" & m_cURL1 & cParam & """ style='text-decoration none; color: #3090FF;'>link</a> " &
                    "or select the ""Requestor Approvals"" menu option in SDI ZEUS to approve or decline the order. " &
                   "</p>"

                End If
                boEncrypt = Nothing
                cLink &= "</td>"
                cLink &= "</tr>"
                Return cLink

            Catch ex As Exception
                'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)

                m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
                SendAlertMessage(msg:=cHdr & ex.ToString)
            End Try
        End If
        Return (cLink)
    End Function
    'Madhu-Zeus-138-Adding the ZEUS 2.0 LINK If Flag is turned on
    Private Function FormHTMLLinkSDiExchange(ByVal cOrderID As String, ByVal cEmployeeID As String, ByVal cBusinessUnitOM As String, ByVal LineStatus As String, Optional ByVal bShowLink As Boolean = True) As String
        Dim cLink As String = ""
        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLLink: "
        Dim m_cURL1 As String = ""
        If bShowLink Then
            Try
                m_cURL1 = GetURL(cBusinessUnitOM, LineStatus)


                Dim boEncrypt As New Encryption64

                Dim cParam As String = "?fer=" & boEncrypt.Encrypt(cOrderID, m_cEncryptionKey) &
                                       "&op=" & boEncrypt.Encrypt(cEmployeeID, m_cEncryptionKey) &
                                       "&xyz=" & boEncrypt.Encrypt(cBusinessUnitOM, m_cEncryptionKey) &
                                       "&HOME=N" &
                                       "&ExchHome23=N"

                cLink &= "<p>" &
                            "Click this " &
                            "<a href=""" & m_cURL1 & cParam & """ target=""_blank"">link</a> " &
                            " to APPROVE or DECLINE order." &
                         "</p>"

                boEncrypt = Nothing

                Return cLink

            Catch ex As Exception
                'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)

                m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
                SendAlertMessage(msg:=cHdr & ex.ToString)
            End Try
        End If
        Return (cLink)
    End Function


    Private Function AddBuyerInfo(ByVal strBuyerDescr As String, ByVal strBuyerEmail As String, ByVal LineStatus As String) As String
        Dim cHdr As String = "QuoteNonStockProcessor.AddBuyerInfo: "
        Try
            Dim cInfoHTML As String = ""

            cInfoHTML &= "<BR />"  '<TABLE id=""Tbl111"" cellSpacing=""1"" cellPadding=""1"" width=""100%"" border=""0"">"
            'cInfoHTML &= "       <TR>" & _
            '                        "<TD style=""WIDTH: 110px"">Buyer:</TD>" & _
            '                        "<TD><B>" & strBuyerDescr & "</B></TD>" & _
            '                    "</TR>"
            'cInfoHTML &= "</TABLE>"

            cInfoHTML &= "" &
                            "Buyer: " &
                            "<B>" & strBuyerDescr & "</B>" &
                         ""
            cInfoHTML &= "<br />" &
                            "Buyer E-mail: " &
                            "<a href=""mailto:" & strBuyerEmail & """>" & strBuyerEmail & "</a> " &
                         "<br />"
            cInfoHTML &= "" ' &
            ' "Phone Number:  888-435-7734 " &
            '""
            If LineStatus = "QTW" Then
                cInfoHTML = ""
            End If
            Return cInfoHTML

        Catch ex As Exception
            'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)

            m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
        Return ""
    End Function

    Private Function AddNoRecepientExistNote(ByVal sTO As String) As String
        Dim cMsg As String = ""
        Dim cHdr As String = "QuoteNonStockProcessor.AddNoRecepientExistNote: "
        Try

            If Not (sTO.Trim.Length > 0) Then
                cMsg &= "<p style=""COLOR: red""><b>IMPORTANT:</b>&nbsp;&nbsp;<i>Please be advised that this message <b>DOES NOT</b> contain valid recipient.</i></p>"
            End If

            Return cMsg

        Catch ex As Exception
            'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)

            m_logger.WriteVerboseLog(cHdr & ".  Error:  " & ex.ToString)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
        Return (cMsg)
    End Function

    Private Function AddVersionNumber() As String

        Dim cRet As String = "<br><p align=""right""><FONT face=""Bookman Old Style"" size=""1"">v" &
                             System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString &
                             "</FONT></p>"
        Return cRet
    End Function

    Private Sub SendAlertMessage(ByVal msg As String)
        Dim cHdr As String = "QuoteNonStockProcessor.SendAlertMessage: "
        Try

            Dim eml As New System.Web.Mail.MailMessage
            Dim SrvcNotification As System.Xml.XmlElement = Nothing

            eml.Subject = ""
            eml.From = ""
            eml.To = ""
            eml.Cc = ""
            eml.Bcc = ""
            eml.Body = ""

            If Not (m_config("configuration")("serviceNotification") Is Nothing) Then
                SrvcNotification = m_config("configuration")("serviceNotification")
            End If

            If Not (SrvcNotification Is Nothing) Then
                ' get the subject line for this service notification messages
                If Not (SrvcNotification.Attributes("notifySubject").InnerText Is Nothing) Then
                    eml.Subject = SrvcNotification.Attributes("notifySubject").InnerText.Trim
                End If

                ' get sender email address (automated)
                If Not (SrvcNotification.Attributes("notifyFrom").InnerText Is Nothing) Then
                    eml.From = SrvcNotification.Attributes("notifyFrom").InnerText.Trim
                End If

                ' get email address list on whom will receives this notification
                If Not (SrvcNotification.ChildNodes Is Nothing) Then
                    If SrvcNotification.ChildNodes.Count > 0 Then
                        For Each nodeTO As System.Xml.XmlNode In SrvcNotification.ChildNodes
                            If nodeTO.Name = "statusNotify" And Not (nodeTO.Attributes("addy").InnerText Is Nothing) Then
                                If nodeTO.Attributes("addy").InnerText.Trim.Length > 0 Then
                                    eml.To &= nodeTO.Attributes("addy").InnerText.Trim & ";"
                                End If
                            End If
                        Next
                    End If
                End If
            End If

            If (msg Is Nothing) Then
                msg = ""
            End If

            If Trim(eml.To) = "" Then
                eml.To = "WEBDEV@SDI.COM"
            End If
            ' insert the body of the message and send the message
            eml.Body = "" &
                       "[ IMPORTANT ]" & vbCrLf &
                       vbTab & " Service sent this ALERT message :: " & System.Environment.MachineName & " :: " & Now.ToString & vbCrLf & vbCrLf &
                       msg &
                       ""

            eml.Priority = Web.Mail.MailPriority.High

            SendLogger(eml.Subject, eml.Body, "ALERTMESSAGE", "Mail", eml.To, eml.Cc, "WebDev@sdi.com")
            ' clean up
            eml = Nothing

        Catch ex As Exception
            If (msg Is Nothing) Then
                msg = ""
            End If
            'm_eventLogger.WriteEntry(cHdr & ".  " & msg & ".  " & ex.ToString, EventLogEntryType.Error)

            m_logger.WriteVerboseLog(cHdr & ".  " & msg & ".  " & ex.ToString)
        End Try
    End Sub

    Public Sub PriceUpdate(ByVal orderid As String, ByVal Linestatus As String)
        Try
            Dim strUpdateQuery As String = ""
            Dim strSelectQuery As String = ""
            Dim ordSts As String = "W"
            Dim ordIdent As String = ""
            Dim oprEnteredBy As String = ""
            Dim ordBu As String = ""
            Dim rowsaffected As Integer = 0
            Dim OrcRdr As OleDb.OleDbDataReader = Nothing
            'Dim ordStsL As String = "QTW"
            Dim strEmploId As String = ""

            strSelectQuery = "Select BUSINESS_UNIT_OM, ISA_LINE_STATUS, OPRID_ENTERED_BY, ORDER_NO, ISA_EMPLOYEE_ID FROM SYSADM8.PS_ISA_ORD_INTF_LN WHERE ORDER_NO='" & orderid & "'"

            OrcRdr = GetReader(strSelectQuery)
            If OrcRdr.HasRows Then
                While OrcRdr.Read()
                    ordBu = CType(OrcRdr("BUSINESS_UNIT_OM"), String).Trim()
                    oprEnteredBy = CType(OrcRdr("OPRID_ENTERED_BY"), String).Trim()
                    strEmploId = CType(OrcRdr("ISA_EMPLOYEE_ID"), String).Trim()
                    If OrcRdr("ISA_LINE_STATUS").Trim().ToUpper() = "QTS" Then  '  If OrcRdr.GetString(0).Trim().ToUpper() = "QTS" Then
                        strUpdateQuery = " UPDATE SYSADM8.PS_ISA_ORD_INTF_LN" & vbCrLf &
                                         " SET ISA_LINE_STATUS = '" & Linestatus & "', OPRID_APPROVED_BY = '" & strEmploId & "', APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                                         " WHERE ORDER_NO = '" & orderid & "'"
                        rowsaffected = ExecNonQuery(strUpdateQuery)
                        SDIAuditInsert("PS_ISA_ORD_INTF_LN", orderid, "ISA_LINE_STATUS", Linestatus, ordBu)
                    End If
                End While
            End If

            strUpdateQuery = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD" & vbCrLf &
                        " SET ORDER_STATUS = '" & ordSts & "'" & vbCrLf &
                        " WHERE ORDER_NO = '" & orderid & "'"

            rowsaffected = ExecNonQuery(strUpdateQuery)

            SDIAuditInsert("PS_ISA_ORD_INTF_LN", orderid, "OPRID_APPROVED_BY", oprEnteredBy, ordBu)
            SDIAuditInsert("PS_ISA_ORD_INTF_HD", orderid, "ORDER_STATUS", ordSts, ordBu)

            SDIApprovalHistoryInsert(ordBu, orderid, oprEnteredBy)

        Catch ex As Exception
            SendLogger("Error in Quoted Non Stock Email Utility, PriceUpdate", ex, "ERROR")
        End Try
    End Sub

    Public Sub SDIAuditInsert(ByVal sTableName As String, ByVal sOrdNum As String, ByVal sColumnChg As String, ByVal sNewValue As String, ByVal sBU As String)
        Try
            Dim strInsertQuery As String = String.Empty
            Dim rowsaffected As Integer = 0

            strInsertQuery = "INSERT INTO ps_isa_SDIXaudit " & vbCrLf &
                " ( " & vbCrLf &
                " descr, rcdsrc, table_name " & vbCrLf &
                ", key_01, key_02, key_03 " & vbCrLf &
                ", columnchg, newvalue, oldvalue " & vbCrLf &
                ", oprid, server_name " & vbCrLf &
                ", dt_timestamp " & vbCrLf &
                ", business_unit, isa_udf1, isa_udf2, isa_udf3 " & vbCrLf &
                " ) " & vbCrLf &
                " VALUES (" & vbCrLf &
                " 'NSTK Email', 'SDINonStockEmailUtility.exe', '" & sTableName & "' " & vbCrLf &
                ", '" & sOrdNum & "', ' ', ' ' " & vbCrLf &
                ", '" & sColumnChg & "', '" & sNewValue & "', ' ' " & vbCrLf &
                ", 'NSTKpric', 'C2' " & vbCrLf &
                ", TO_DATE('" & Now.ToString("MM/dd/yyyy HH:mm:ss") & "', 'MM/DD/YYYY HH24:MI:SS') " & vbCrLf &
                ", '" & sBU & "', ' ', ' ', ' ' " & vbCrLf &
                " )"

            rowsaffected = ORDBData.ExecNonQuery(strInsertQuery)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SDIApprovalHistoryInsert(ByVal sBU As String, ByVal sOrderNo As String, ByVal sEnteredBy As String)
        Try
            Dim strInsertQuery As String = String.Empty
            Dim rowsaffected As Integer = 0

            ' We need to check if the record doesn't already exist when trying to insert it.
            ' If it already exists, we get a unique constraint error so we'll avoid the error
            ' with this insert statement. The record could already exist if we processed the 
            ' order with this utility previously but a buyer goes into PeopleSoft and changes
            ' the status back to "Q".
            strInsertQuery = "INSERT INTO PS_ISA_APPR_PATH" & vbCrLf &
                    " (BUSINESS_UNIT_OM, ORDER_NO," & vbCrLf &
                    " SEQ_NBR," & vbCrLf &
                    " OPRID_ENTERED_BY, OPRID_APPROVED_BY," & vbCrLf &
                    " APPR_STATUS, ISA_APPR_TYPE," & vbCrLf &
                    "  ADD_DTTM, LASTUPDDTTM)" & vbCrLf &
                    " SELECT " & vbCrLf &
                    "   '" & sBU & "', '" & sOrderNo & "', '1'," & vbCrLf &
                    "    '" & sEnteredBy & "','NSTKpric'," & vbCrLf &
                    "    'P', 'Q'," & vbCrLf &
                    "    TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf &
                    "    TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                    " FROM dual " & vbCrLf &
                    " WHERE NOT EXISTS " & vbCrLf &
                    "   (SELECT 'X' FROM PS_ISA_APPR_PATH " & vbCrLf &
                    "       WHERE business_unit_om = '" & sBU & "' " & vbCrLf &
                    "       AND order_no = '" & sOrderNo & "' " & vbCrLf &
                    "       AND seq_nbr = '1'" & vbCrLf &
                    "       AND appr_status = 'P' " & vbCrLf &
                    "       AND isa_appr_type = 'Q')"

            rowsaffected = ExecNonQuery(strInsertQuery)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub UpdateReqEmailLog(ByVal itmQuoted As QuotedNStkItem, Optional ByVal sBU As String = "")

        Dim strInsertQuery As String = String.Empty
        Dim rowsaffected As Integer = 0
        Dim cmd As OleDbCommand
        Dim cSQL As String = ""

        Try

            Dim eml = New System.Web.Mail.MailMessage

            ' init properties of the mail message
            'SDI-40628 Changing Mail id as walmartpurchasing@sdi.com from sdiexchange@sdi.com for Walmart BU.
            'SP-316 Purchasing Email from address change[By Vishalini]
            eml.From = getpurchasingEmailFrom(sBU)
            eml.To = ""
            eml.Cc = ""
            eml.Bcc = ""
            eml.Subject = ""
            eml.Body = ""

            ' assign sender email address from item object 
            ' or assign the default automated sender
            If itmQuoted.FROM.Length > 0 Then
                eml.From = itmQuoted.FROM
            Else
                eml.From = m_defaultFROM
            End If

            ' assign recipient TO email address(es) from records
            ' and add any defined TOs within the configuration file
            If itmQuoted.TO.Length > 0 Then
                eml.To = itmQuoted.TO
            End If
            If Trim(eml.To) <> "" Then
                If Right(Trim(eml.To), 1) = ";" Then
                    ' OK, do nothing
                Else
                    eml.To = Trim(eml.To) & ";"
                End If
            End If
            If m_extendedTO.Count > 0 Then
                For Each sTo As String In m_extendedTO
                    If Utility.IsValidEmailAdd(sTo) Then
                        eml.To &= sTo & ";"
                    End If
                Next
            End If

            ' assign recipient CC email address(es)
            'Mythili -- INC0023448 Adding CC emails
            If sBU = "EMC00" Or sBU = "WAL00" Then
                If itmQuoted.CC.Length > 0 And getDBName() Then
                    eml.Cc = itmQuoted.CC + ";" + getpurchasingEmailTo(sBU)
                ElseIf itmQuoted.CC.Length = 0 And getDBName() Then
                    eml.Cc = getpurchasingEmailTo(sBU)
                End If
            Else
                If itmQuoted.CC.Length > 0 And getDBName() Then
                    eml.Cc = itmQuoted.CC
                End If
            End If

            ' assign recipient BCC email address(es)
            If itmQuoted.BCC.Length > 0 Then
                eml.Bcc = itmQuoted.BCC
            Else
                eml.Bcc = "WebDev@sdi.com"
            End If

            ' assign the subject of this email
            ' or use the default subject line from the configuration file (most probably is)
            If itmQuoted.Subject.Length > 0 Then
                eml.Subject = itmQuoted.Subject
            Else
                eml.Subject = m_defaultSubject
            End If

            If itmQuoted.FormattedOrderID.Length > 0 Then
                eml.Subject &= " - " & itmQuoted.FormattedOrderID
            ElseIf itmQuoted.OrderID.Length > 0 Then
                eml.Subject &= " - " & itmQuoted.OrderID
            End If

            ' override for RFQ origin to include (if provided) the work order # on the subject line
            '   - erwin 3/26/2014
            If (itmQuoted.OrderOrigin = "RFQ") Then
                If Trim(itmQuoted.WorkOrderNumber) <> "" Then  '  If (itmQuoted.WorkOrderNumber.Length > 0) Then
                    eml.Subject &= " (Work Order #" & itmQuoted.WorkOrderNumber & ")"
                End If
            End If
            Dim strOraSelectQuery As String = "select * from SYSADM8.PS_ISA_ORD_INTF_LN A where A.ORDER_NO = '" & itmQuoted.OrderID & "' AND A.ISA_LINE_STATUS in ('QTS','QTW') AND NOT EXISTS (SELECT 'X' FROM PS_ISA_REQ_EML_LOG B1 WHERE B1.REQ_ID = A.ORDER_NO AND B1.LINE_NO= A.ISA_INTFC_LN AND B1.PRICE= A.ISA_SELL_PRICE)"
            Dim dsOrdLnItems As DataSet = GetAdapter(strOraSelectQuery)
            If Not getDBName() Then
                If sBU = "I0635" Then
                    eml.To = eml.To + ";" + "webdev@sdi.com"
                End If
            End If
            If dsOrdLnItems.Tables(0).Rows.Count > 0 Then

                For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows

                    Try
                        Dim strOraSelectQuery1 As String = "select * from PS_ISA_REQ_EML_LOG A where A.REQ_ID = '" & itmQuoted.OrderID & "' AND LINE_NO= '" & dataRowMain("ISA_INTFC_LN") & "'"
                        Dim ds As DataSet = GetAdapter(strOraSelectQuery1)

                        If ds.Tables(0).Rows.Count() = 0 Then
                            cSQL = "INSERT INTO PS_ISA_REQ_EML_LOG " &
                "(BUSINESS_UNIT, REQ_ID, ISA_RECIPIENT, ISA_SENDER, ISA_SUBJECT, EMAIL_DATETIME, LINE_NO, PRICE) " &
                "VALUES " &
                "(" &
                    "'" & CType(IIf(itmQuoted.BusinessUnitID.Length > 0, itmQuoted.BusinessUnitID, "."), String) & "', " &
                    "'" & CType(IIf(itmQuoted.OrderID.Length > 0, itmQuoted.OrderID, "."), String) & "', " &
                    "'" & "TO=" & eml.To & "CC=" & eml.Cc & "BCC=" & eml.Bcc & "', " &
                    "'" & CType(IIf(eml.From.Length > 0, eml.From, "."), String) & "', " &
                    "'" & CType(IIf(eml.Subject.Length > 0, eml.Subject, "."), String) & "', " &
                    "TO_DATE('" & System.DateTime.Now.ToString & "','MM/DD/YYYY HH:MI:SS AM'), " &
                    "'" & dataRowMain("ISA_INTFC_LN") & "'," &
                    "'" & dataRowMain("ISA_SELL_PRICE") & "'" &
                ")"
                        Else
                            cSQL = "UPDATE PS_ISA_REQ_EML_LOG set PRICE='" & dataRowMain("ISA_SELL_PRICE") & "' where REQ_ID= '" & itmQuoted.OrderID & "' and LINE_NO= '" & dataRowMain("ISA_INTFC_LN") & "'"
                        End If
                    Catch ex As Exception

                    End Try
                    If m_CN.State = ConnectionState.Open Then

                    Else
                        m_CN.Open()
                    End If
                    ' create a new instance of the command object
                    cmd = New OleDbCommand(cmdText:=cSQL, connection:=m_CN)
                    cmd.CommandType = CommandType.Text

                    ' execute SQL statement againts the connection object
                    Try
                        cmd.ExecuteNonQuery()
                    Catch ex As Exception
                        'm_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
                        Try
                            m_CN.Close()
                        Catch exErr2 As Exception

                        End Try
                    End Try
                Next
            End If

        Catch ex As Exception
            SendLogger("Error in Quoted Non Stock Email Utility", ex, "ERROR", strInsertQuery, m_CN.ConnectionString)
        End Try
    End Sub

    Public Function ExecNonQuery(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As Integer
        Try

            Dim rowsAffected As Integer
            Dim cmd As OleDbCommand = m_CN.CreateCommand
            cmd = New OleDbCommand(cmdText:=p_strQuery, connection:=m_CN)
            cmd.CommandType = CommandType.Text
            rowsAffected = cmd.ExecuteNonQuery()
            Try
                cmd.Dispose()
            Catch ex As Exception
            End Try

            Return rowsAffected
        Catch objException As Exception
            SendLogger("Error in Quoted Non Stock Email Utility", objException, "ERROR", p_strQuery, m_CN.ConnectionString)
        End Try
    End Function

    Public Shared Function GetReader(ByVal p_strQuery As String) As OleDbDataReader
        Try

            Dim cmd As OleDbCommand = m_CN.CreateCommand
            cmd = New OleDbCommand(cmdText:=p_strQuery, connection:=m_CN)
            cmd.CommandType = CommandType.Text
            If m_CN.State = ConnectionState.Closed Then
                m_CN.Open()
            End If


            Dim datareader As OleDbDataReader
            datareader = cmd.ExecuteReader(CommandBehavior.CloseConnection)
            Try
                cmd.Dispose()
            Catch ex As Exception

            End Try
            Return datareader
        Catch objException As Exception
        End Try
    End Function

    Public Shared Function GetAdapter(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As DataSet
        Try
            Dim cmd As OleDbCommand = m_CN.CreateCommand
            cmd = New OleDbCommand(cmdText:=p_strQuery, connection:=m_CN)
            cmd.CommandType = CommandType.Text
            Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(cmd)
            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()
            dataAdapter.Fill(UserdataSet)
            Try
                cmd.Dispose()
            Catch ex As Exception

            End Try
            Return UserdataSet
        Catch objException As Exception
        End Try
    End Function

    Public Function PositionGrid(ByVal dstcartSTK As DataTable, Optional ByVal LineStatus As String = "", Optional ByVal sBU As String = "") As String

        Dim Item_ID As String = ""
        Dim Descr As String = ""
        Dim MfgID As String = ""
        Dim Mfg_Itm_ID As String = ""
        Dim Qty As String = ""
        Dim UOM As String = ""
        Dim Price As String = ""
        Dim ExtPrice As String = ""
        Dim POPrice As String = ""
        Dim POExtPrice As String = ""
        Dim WorkOrder As String = ""
        Dim ChargeCode As String = ""
        Dim Supplier As String = ""
        Dim SupplierPartNum As String = ""
        Dim AltpartNum As String = ""
        Dim content As String = ""




        Try
            If dstcartSTK IsNot Nothing Then
                If dstcartSTK.Rows.Count > 0 Then
                    Dim currentRow As Integer = 0
                    Dim totalRows As Integer = dstcartSTK.Rows.Count
                    content &= "<tr style = 'font-size: 16px;' >"
                    content &= "<td style ='padding: 18px 12px; background-color: #F9F9F9;' colspan='2'>"
                    content &= "<p style='margin-top: 0px; margin-bottom: 0px; font-weight: 600;'>Item Details</p>"
                    content &= "</td>"
                    content &= "</tr>"
                    For Each dr As DataRow In dstcartSTK.Rows
                        Try
                            If dr("Item ID") Is DBNull.Value OrElse dr("Item ID") Is Nothing OrElse dr("Item ID").Trim() = "" Then
                                Item_ID = "-"
                            Else
                                Item_ID = dr("Item ID").ToString()
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            If dr("Description") Is DBNull.Value OrElse dr("Description") Is Nothing OrElse dr("Description").Trim() = "" Then
                                Descr = "-"
                            Else
                                Descr = dr("Description").ToString()
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            If dr("Manuf.") Is DBNull.Value OrElse dr("Manuf.") Is Nothing OrElse dr("Manuf.").Trim() = "" Then
                                MfgID = "-"
                            Else
                                MfgID = dr("Manuf.").ToString()
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            If dr("Manuf. Partnum") Is DBNull.Value OrElse dr("Manuf. Partnum") Is Nothing OrElse dr("Manuf. Partnum").Trim() = "" Then
                                Mfg_Itm_ID = "-"
                            Else
                                Mfg_Itm_ID = dr("Manuf. Partnum").ToString()
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            If dr("QTY") Is DBNull.Value OrElse dr("QTY") Is Nothing OrElse dr("QTY").Trim() = "" Then
                                Qty = "-"
                            Else
                                Qty = dr("QTY").ToString()
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            If dr("UOM") Is DBNull.Value OrElse dr("UOM") Is Nothing OrElse dr("UOM").Trim() = "" Then
                                UOM = "-"
                            Else
                                UOM = dr("UOM").ToString()
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            If dr("WorkOrder") Is DBNull.Value OrElse dr("WorkOrder") Is Nothing OrElse dr("WorkOrder").Trim() = "" Then
                                WorkOrder = "-"
                            Else
                                WorkOrder = dr("WorkOrder").ToString()
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            If dr("ChargeCode") Is DBNull.Value OrElse dr("ChargeCode") Is Nothing OrElse dr("ChargeCode").Trim() = "" Then
                                ChargeCode = "-"
                            Else
                                ChargeCode = dr("ChargeCode").ToString()
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            If dr("Alt. Partnum") Is DBNull.Value OrElse dr("Alt. Partnum") Is Nothing OrElse dr("Alt. Partnum").ToString().Trim() = "" Then
                                AltpartNum = "-"
                            Else
                                AltpartNum = dr("Alt. Partnum").ToString()
                            End If
                        Catch ex As Exception
                        End Try

                        Try
                            If IsECI(sBU) Then
                                Try
                                    If dr("Item PO Price") Is DBNull.Value OrElse dr("Item PO Price") Is Nothing OrElse dr("Item PO Price").Trim() = "" Then
                                        POPrice = "-"
                                    Else
                                        POPrice = dr("Item PO Price").ToString()
                                    End If
                                Catch ex As Exception
                                End Try
                                Try
                                    If dr("Item PO Ext. Price") Is DBNull.Value OrElse dr("Item PO Ext. Price") Is Nothing OrElse dr("Item PO Ext. Price").Trim() = "" Then
                                        POExtPrice = "-"
                                    Else
                                        POExtPrice = dr("Item PO Ext. Price").ToString()
                                    End If
                                Catch ex As Exception
                                End Try
                            End If

                            Try
                                If dr("Item USD Price") Is DBNull.Value OrElse dr("Item USD Price") Is Nothing OrElse dr("Item USD Price").Trim() = "" Then
                                    Price = "-"
                                Else
                                    Price = dr("Item USD Price").ToString()
                                End If
                            Catch ex As Exception
                            End Try
                            Try
                                If dr("Ext. USD Price") Is DBNull.Value OrElse dr("Ext. USD Price") Is Nothing OrElse dr("Ext. USD Price").Trim() = "" Then
                                    ExtPrice = "-"
                                Else
                                    ExtPrice = dr("Ext. USD Price").ToString()
                                End If
                            Catch ex As Exception
                            End Try
                        Catch ex As Exception
                        End Try
                        Try
                            If sBU = "I0W01" Then
                                Try
                                    If dr("Supplier") Is DBNull.Value OrElse dr("Supplier") Is Nothing OrElse dr("Supplier").Trim() = "" Then
                                        Supplier = "-"
                                    Else
                                        Supplier = dr("Supplier").ToString()
                                    End If
                                Catch ex As Exception
                                End Try
                                Try
                                    If dr("Supplier Part Number") Is DBNull.Value OrElse dr("Supplier Part Number") Is Nothing OrElse dr("Supplier Part Number").Trim() = "" Then
                                        SupplierPartNum = "-"
                                    Else
                                        SupplierPartNum = dr("Supplier Part Number").ToString()
                                    End If
                                Catch ex As Exception
                                End Try
                            End If
                        Catch ex As Exception
                        End Try

                        Try
                            content &= "<tr style='font-size: 13px;'>"
                            content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                            content &= "<p style=font-weight: 600; margin: 10px 0px;>Item ID :</p>"
                            content &= "</td>"
                            content &= "<td style='padding:0px 12px; '>"
                            content &= "<p style= 'color: #595959; margin: 10px 0px;' >" & Item_ID & "</p>"
                            content &= "</td>"
                            content &= "</tr>"
                            Try
                                content &= "<tr style='font-size: 13px;'>"
                                content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                content &= "<p style=font-weight: 600; margin: 10px 0px;>Description :</p>"
                                content &= "</td>"
                                content &= "<td style='padding:0px 12px; '>"
                                content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & Descr & "</p>"
                                content &= "</td>"
                                content &= "</tr>"

                                content &= "<tr style='font-size: 13px;'>"
                                content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                content &= "<p style=font-weight: 600; margin: 10px 0px;> Manufacturer :</p>"
                                content &= "</td>"
                                content &= "<td style='padding:0px 12px; '>"
                                content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & MfgID & "</p>"
                                content &= "</td>"
                                content &= "</tr>"
                            Catch ex As Exception
                            End Try
                            Try
                                content &= "<tr style='font-size: 13px;'>"
                                content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                content &= "<p style=font-weight: 600; margin: 10px 0px;>Manufacturer part # :</p>"
                                content &= "</td>"
                                content &= "<td style='padding:0px 12px; '>"
                                content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & Mfg_Itm_ID & "</p>"
                                content &= "</td>"
                                content &= "</tr>"
                            Catch ex As Exception
                            End Try
                            'if Alt part #
                            Try
                                content &= "<tr style='font-size: 13px;'>"
                                content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                content &= "<p style=font-weight: 600; margin: 10px 0px;>Alternate part # :</p>"
                                content &= "</td>"
                                content &= "<td style='padding:0px 12px; '>"
                                content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & AltpartNum & "</p>"
                                content &= "</td>"
                                content &= "</tr>"
                            Catch ex As Exception
                            End Try
                            Try
                                content &= "<tr style='font-size: 13px;'>"
                                content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                content &= "<p style=font-weight: 600; margin: 10px 0px;>Quantity :</p>"
                                content &= "</td>"
                                content &= "<td style='padding:0px 12px; '>"
                                content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & Qty & "</p>"
                                content &= "</td>"
                                content &= "</tr>"
                            Catch ex As Exception
                            End Try

                            Try
                                content &= "<tr style='font-size: 13px;'>"
                                content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                content &= "<p style=font-weight: 600; margin: 10px 0px;>UOM :</p>"
                                content &= "</td>"
                                content &= "<td style='padding:0px 12px; '>"
                                content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & UOM & "</p>"
                                content &= "</td>"
                                content &= "</tr>"
                            Catch ex As Exception

                            End Try

                            Try
                                content &= "<tr style='font-size: 13px;'>"
                                content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                content &= "<p style=font-weight: 600; margin: 10px 0px;>Price :</p>"
                                content &= "</td>"
                                content &= "<td style='padding:0px 12px; '>"
                                content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & "$" & Price & "</p>"
                                content &= "</td>"
                                content &= "</tr>"
                            Catch ex As Exception
                            End Try

                            Try
                                content &= "<tr style='font-size: 13px;'>"
                                content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                content &= "<p style=font-weight: 600; margin: 10px 0px;>Ext. price :</p>"
                                content &= "</td>"
                                content &= "<td style='padding:0px 12px; '>"
                                content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & "$" & ExtPrice & "</p>"
                                content &= "</td>"
                                content &= "</tr>"
                            Catch ex As Exception
                            End Try


                            Try
                                If IsECI(sBU) Then
                                    Try
                                        content &= "<tr style='font-size: 13px;'>"
                                        content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                        content &= "<p style=font-weight: 600; margin: 10px 0px;>Item PO Price:</p>"
                                        content &= "</td>"
                                        content &= "<td style='padding:0px 12px; '>"
                                        content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & POPrice & "</p>"
                                        content &= "</td>"
                                        content &= "</tr>"
                                        content &= "<tr style='font-size: 13px;'>"
                                        content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                        content &= "<p style=font-weight: 600; margin: 10px 0px;>Item PO Ext Price:</p>"
                                        content &= "</td>"
                                        content &= "<td style='padding:0px 12px; '>"
                                        content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & POExtPrice & "</p>"
                                        content &= "</td>"
                                        content &= "</tr>"
                                    Catch ex As Exception

                                    End Try

                                End If
                            Catch ex As Exception
                            End Try

                            Try
                                If LineStatus = "QTW" Then
                                    content &= "<tr style='font-size: 13px;'>"
                                    content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                    content &= "<p style=font-weight: 600; margin: 10px 0px;>Work order :</p>"
                                    content &= "</td>"
                                    content &= "<td style='padding:0px 12px; '>"
                                    content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & WorkOrder & "</p>"
                                    content &= "</td>"
                                    content &= "</tr>"

                                    content &= "<tr style='font-size: 13px;'>"
                                    content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                    content &= "<p style=font-weight: 600; margin: 10px 0px;>Item Charge code :</p>"
                                    content &= "</td>"
                                    content &= "<td style='padding:0px 12px; '>"
                                    content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & ChargeCode & "</p>"
                                    content &= "</td>"
                                    content &= "</tr>"
                                End If
                            Catch ex As Exception
                            End Try

                            Try
                                If sBU = "I0W01" Then
                                    content &= "<tr style='font-size: 13px;'>"
                                    content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                    content &= "<p style=font-weight: 600; margin: 10px 0px;>Supplier :</p>"
                                    content &= "</td>"
                                    content &= "<td style='padding:0px 12px; '>"
                                    content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & Supplier & "</p>"
                                    content &= "</td>"
                                    content &= "</tr>"

                                    content &= "<tr style='font-size: 13px;'>"
                                    content &= "<td style='width: 41%; padding: 0px 12px; vertical-align: top;'>"
                                    content &= "<p style=font-weight: 600; margin: 10px 0px;>Supplier part # :</p>"
                                    content &= "</td>"
                                    content &= "<td style='padding:0px 12px; '>"
                                    content &= "<p style= 'color: #595959; margin: 10px 0px;'>" & SupplierPartNum & "</p>"
                                    content &= "</td>"
                                    content &= "</tr>"
                                End If
                            Catch ex As Exception
                            End Try

                            If currentRow < totalRows - 1 Then
                                content &= "<tr> <td colspan='2' style='border-bottom: #DFDFDF 1px solid;'></td> </tr>" & vbCrLf
                            End If
                            currentRow += 1

                        Catch ex As Exception
                        End Try
                    Next
                    Return content
                End If
            End If
        Catch ex As Exception
        End Try
    End Function

    Public Class OrderTotals
        Private Shared Function GetOrderTotal(ByVal iBU As String, ByVal orderNo As String,
        ByVal siteCurrency As sdiCurrency, bNSTKonly As Boolean) As Decimal
            Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
            Dim orderTotal As Decimal = 0
            Try
                Dim cn As New OleDbConnection(ORDBData.DbUrl)
                cn.Open()
                If cn.State = ConnectionState.Open Then
                    Dim sql As String = ""
                    'sql = "" & _
                    '      "SELECT " & vbCrLf & _
                    '      " INTFC.BUSINESS_UNIT_OM" & vbCrLf & _
                    '      ",INTFC.BUSINESS_UNIT_BI" & vbCrLf & _
                    '      ",INTFC.ORDER_NO" & vbCrLf & _
                    '      ",INTFC.LINE_NBR" & vbCrLf & _
                    '      ",INTFC.INV_ITEM_ID" & vbCrLf & _
                    '      ",DECODE(D.CURRENCY_CD, NULL, '" & siteCurrency.Id & "', D.CURRENCY_CD) AS CURRENCY_CD" & vbCrLf & _
                    '      ",DECODE(DECODE(D.PRICE_REQ, NULL, 0, D.PRICE_REQ), 0, INTFC.NET_UNIT_PRICE, D.PRICE_REQ) AS NET_UNIT_PRICE" & vbCrLf & _
                    '      ",DECODE(DECODE(D.QTY_REQ, NULL, 0, D.QTY_REQ), 0, INTFC.QTY_REQ, D.QTY_REQ) AS QTY_REQ" & vbCrLf & _
                    '      ",INTFC.ORDER_STATUS, D.ISA_SELL_PRICE " & vbCrLf & _
                    '      "FROM " & vbCrLf & _
                    '      " (" & vbCrLf & _
                    '      "  SELECT" & vbCrLf & _
                    '      "   A.BUSINESS_UNIT_OM" & vbCrLf & _
                    '      "  ,C.BUSINESS_UNIT_BI" & vbCrLf & _
                    '      "  ,A.ORDER_NO" & vbCrLf & _
                    '      "  ,B.LINE_NBR" & vbCrLf & _
                    '      "  ,A.ORDER_STATUS" & vbCrLf & _
                    '      "  ,B.INV_ITEM_ID" & vbCrLf & _
                    '      "  ,B.ISA_ORDER_STATUS" & vbCrLf & _
                    '      "  ,B.QTY_REQ" & vbCrLf & _
                    '      "  ,B.NET_UNIT_PRICE" & vbCrLf & _
                    '      "  ,A.OPRID_ENTERED_BY" & vbCrLf & _
                    '      "  ,A.OPRID_APPROVED_BY" & vbCrLf & _
                    '      "  FROM PS_ISA_ORD_INTFC_H A" & vbCrLf & _
                    '      "  ,PS_ISA_ORD_INTFC_L B" & vbCrLf & _
                    '      "  ,PS_BUS_UNIT_TBL_OM C" & vbCrLf & _
                    '      "  WHERE A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                    '      "    AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf & _
                    '      "    AND A.BUSINESS_UNIT_OM = '" & iBU & "' " & vbCrLf & _
                    '      "    AND A.ORDER_NO = '" & orderNo & "' " & vbCrLf
                    'If bNSTKonly Then
                    '    sql = sql & _
                    '        " AND NOT EXISTS (SELECT 'X'" & vbCrLf & _
                    '        " FROM PS_INV_ITEMS C" & vbCrLf & _
                    '        " WHERE C.EFFDT =" & vbCrLf & _
                    '        " (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED" & vbCrLf & _
                    '        " WHERE(C.SETID = C_ED.SETID)" & vbCrLf & _
                    '        " AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID" & vbCrLf & _
                    '        " AND C_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                    '        " AND C.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
                    '        " AND C.INV_STOCK_TYPE = 'STK')" & vbCrLf
                    'End If
                    'sql = sql & " ) INTFC" & vbCrLf & _
                    '      ",PS_REQ_LINE D" & vbCrLf & _
                    '      "WHERE INTFC.ORDER_NO = D.REQ_ID (+)" & vbCrLf & _
                    '      "  AND INTFC.LINE_NBR = D.LINE_NBR (+)" & vbCrLf & _
                    '      "ORDER BY INTFC.BUSINESS_UNIT_OM" & vbCrLf & _
                    '      ",INTFC.ORDER_NO" & vbCrLf & _
                    '      ",INTFC.LINE_NBR" & vbCrLf & _
                    '      ""

                    'sql = "" & _
                    '    "SELECT " & vbCrLf & _
                    '    " INTFC.BUSINESS_UNIT_OM" & vbCrLf & _
                    '    ",INTFC.BUSINESS_UNIT_BI" & vbCrLf & _
                    '    ",INTFC.ORDER_NO" & vbCrLf & _
                    '    ",INTFC.LINE_NBR" & vbCrLf & _
                    '    ",INTFC.INV_ITEM_ID" & vbCrLf & _
                    '    ",DECODE(D.CURRENCY_CD, NULL, '" & siteCurrency.Id & "', D.CURRENCY_CD) AS CURRENCY_CD" & vbCrLf & _
                    '    ",DECODE(DECODE(D.PRICE_REQ, NULL, 0, D.PRICE_REQ), 0, INTFC.NET_UNIT_PRICE, D.PRICE_REQ) AS NET_UNIT_PRICE" & vbCrLf & _
                    '    ",DECODE(DECODE(D.QTY_REQ, NULL, 0, D.QTY_REQ), 0, INTFC.QTY_REQ, D.QTY_REQ) AS QTY_REQ" & vbCrLf & _
                    '    ",INTFC.ORDER_STATUS, D.ISA_SELL_PRICE " & vbCrLf & _
                    '    "FROM " & vbCrLf & _
                    '    " (" & vbCrLf & _
                    '    "  SELECT" & vbCrLf & _
                    '    "   A.BUSINESS_UNIT_OM" & vbCrLf & _
                    '    "  ,C.BUSINESS_UNIT_BI" & vbCrLf & _
                    '    "  ,A.ORDER_NO" & vbCrLf & _
                    '    "  ,B.LINE_NBR" & vbCrLf & _
                    '    "  ,A.ORDER_STATUS" & vbCrLf & _
                    '    "  ,B.INV_ITEM_ID" & vbCrLf & _
                    '    "  ,B.ISA_ORDER_STATUS" & vbCrLf & _
                    '    "  ,B.QTY_REQ" & vbCrLf & _
                    '    "  ,B.NET_UNIT_PRICE" & vbCrLf & _
                    '    "  ,A.OPRID_ENTERED_BY" & vbCrLf & _
                    '    "  ,A.OPRID_APPROVED_BY" & vbCrLf & _
                    '    "  FROM PS_ISA_ORD_INTFC_H A" & vbCrLf & _
                    '    "  ,PS_ISA_ORD_INTFC_L B" & vbCrLf & _
                    '    "  ,PS_BUS_UNIT_TBL_OM C" & vbCrLf & _
                    '    "  WHERE A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                    '    "    AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf & _
                    '    "    AND A.BUSINESS_UNIT_OM = '" & iBU & "' " & vbCrLf & _
                    '    "    AND A.ORDER_NO = '" & orderNo & "' " & vbCrLf
                    'If bNSTKonly Then
                    '    sql = sql & _
                    '        " AND NOT EXISTS (SELECT 'X'" & vbCrLf & _
                    '        " FROM PS_INV_ITEMS C" & vbCrLf & _
                    '        " WHERE C.EFFDT =" & vbCrLf & _
                    '        " (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED" & vbCrLf & _
                    '        " WHERE(C.SETID = C_ED.SETID)" & vbCrLf & _
                    '        " AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID" & vbCrLf & _
                    '        " AND C_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                    '        " AND C.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
                    '        " AND C.INV_STOCK_TYPE = 'STK')" & vbCrLf
                    'End If
                    'sql = sql & " ) INTFC" & vbCrLf & _
                    '      ",PS_REQ_LINE D" & vbCrLf & _
                    '      "WHERE INTFC.ORDER_NO = D.REQ_ID (+)" & vbCrLf & _
                    '      "  AND INTFC.LINE_NBR = D.LINE_NBR (+)" & vbCrLf & _
                    '      "ORDER BY INTFC.BUSINESS_UNIT_OM" & vbCrLf & _
                    '      ",INTFC.ORDER_NO" & vbCrLf & _
                    '      ",INTFC.LINE_NBR" & vbCrLf & _
                    '      ""


                    ' PeopleSoft 9.2 Changes

                    sql = "" &
                   "SELECT " & vbCrLf &
                   " INTFC.BUSINESS_UNIT_OM" & vbCrLf &
                   ",INTFC.BUSINESS_UNIT_BI" & vbCrLf &
                   ",INTFC.ORDER_NO" & vbCrLf &
                   ",INTFC.ISA_INTFC_LN" & vbCrLf &
                   ",INTFC.INV_ITEM_ID" & vbCrLf &
                   ",DECODE(D.CURRENCY_CD, NULL, '" & siteCurrency.Id & "', D.CURRENCY_CD) AS CURRENCY_CD" & vbCrLf &
                   ",DECODE(DECODE(D.PRICE_REQ, NULL, 0, D.PRICE_REQ), 0, INTFC.ISA_SELL_PRICE, D.PRICE_REQ) AS NET_UNIT_PRICE" & vbCrLf &
                   ",DECODE(DECODE(D.QTY_REQ, NULL, 0, D.QTY_REQ), 0, INTFC.QTY_REQUESTED, D.QTY_REQ) AS QTY_REQ" & vbCrLf &
                   ",INTFC.ORDER_STATUS, ISA_SELL_PRICE " & vbCrLf &
                   "FROM " & vbCrLf &
                   " (" & vbCrLf &
                   "  SELECT" & vbCrLf &
                   "   A.BUSINESS_UNIT_OM" & vbCrLf &
                   "  ,C.BUSINESS_UNIT_BI" & vbCrLf &
                   "  ,A.ORDER_NO" & vbCrLf &
                   "  ,B.ISA_INTFC_LN" & vbCrLf &
                   "  ,A.ORDER_STATUS" & vbCrLf &
                   "  ,B.INV_ITEM_ID" & vbCrLf &
                   "  ,B.ISA_LINE_STATUS" & vbCrLf &
                   "  ,B.QTY_REQUESTED" & vbCrLf &
                   "  ,B.ISA_SELL_PRICE" & vbCrLf &
                   "  ,B.OPRID_ENTERED_BY" & vbCrLf &
                   "  ,B.OPRID_APPROVED_BY" & vbCrLf &
                   "  FROM SYSADM8.PS_ISA_ORD_INTF_HD A" & vbCrLf &
                   "  ,SYSADM8.PS_ISA_ORD_INTF_LN B" & vbCrLf &
                   "  ,PS_BUS_UNIT_TBL_OM C" & vbCrLf &
                   "  WHERE A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                   "    AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf &
                   "    AND A.BUSINESS_UNIT_OM = '" & iBU & "' " & vbCrLf &
                   "    AND A.ORDER_NO = '" & orderNo & "' " & vbCrLf
                    If bNSTKonly Then
                        sql = sql &
                        " AND NOT EXISTS (SELECT 'X'" & vbCrLf &
                        " FROM PS_INV_ITEMS C" & vbCrLf &
                        " WHERE C.EFFDT =" & vbCrLf &
                        " (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED" & vbCrLf &
                        " WHERE(C.SETID = C_ED.SETID)" & vbCrLf &
                        " AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID" & vbCrLf &
                        " AND C_ED.EFFDT <= SYSDATE)" & vbCrLf &
                        " AND C.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf &
                        " AND C.INV_STOCK_TYPE = 'STK')" & vbCrLf
                    End If
                    sql = sql & " ) INTFC" & vbCrLf &
                      ",PS_REQ_LINE D" & vbCrLf &
                      "WHERE INTFC.ORDER_NO = D.REQ_ID (+)" & vbCrLf &
                      "  AND INTFC.ISA_INTFC_LN = D.LINE_NBR (+)" & vbCrLf &
                      "ORDER BY INTFC.BUSINESS_UNIT_OM" & vbCrLf &
                      ",INTFC.ORDER_NO" & vbCrLf &
                      ",INTFC.ISA_INTFC_LN" & vbCrLf &
                      ""
                    Dim ds As DataSet = ORDBData.GetAdapter(sql)
                    'currentApp.Session("sesds") = ds


                    Dim cmd As OleDbCommand = cn.CreateCommand
                    cmd.CommandText = sql
                    cmd.CommandType = CommandType.Text
                    Dim rdr As OleDbDataReader = Nothing
                    Try
                        rdr = cmd.ExecuteReader
                    Catch ex As Exception
                    End Try
                    If Not (rdr Is Nothing) Then
                        Dim sdiItemId As String = ""
                        Dim itm As sdiItem = Nothing
                        Dim prc As sdiItemPrice = Nothing
                        Dim qty As Decimal = 0
                        Dim unitPrice As Decimal = 0
                        Dim itemCurrencyCd As String = ""
                        Dim convRate As sdiConversionRate = Nothing
                        Dim convUnitPrice As Decimal = 0
                        Dim bGetNonAEESPrice As Boolean = True
                        Dim sOrderStatus As String = ""
                        Dim decSellPrice As Decimal = 0
                        While rdr.Read
                            sdiItemId = ""
                            Try
                                sdiItemId = CStr(rdr("INV_ITEM_ID")).Trim.ToUpper
                            Catch ex As Exception
                            End Try
                            If sdiItemId.Length > 0 Then
                                itm = sdiItem.GetItemInfo(sdiItemId)
                            Else
                                itm = New sdiItem(sdiItemId, "")
                            End If
                            itm.IBusinessUnit = iBU
                            qty = 0
                            Try
                                'qty = CDec(rdr("QTY_REQUESTED"))
                                qty = CDec(rdr("QTY_REQ"))
                            Catch ex As Exception
                            End Try
                            unitPrice = 0
                            Try
                                sOrderStatus = CStr(rdr("ORDER_STATUS"))
                                If sOrderStatus.Trim.ToUpper = "Q" Then
                                    decSellPrice = CDec(rdr("ISA_SELL_PRICE"))
                                    If decSellPrice > 0 Then
                                        unitPrice = decSellPrice
                                    Else
                                        unitPrice = CDec(rdr("ISA_SELL_PRICE"))
                                    End If
                                Else
                                    unitPrice = CDec(rdr("ISA_SELL_PRICE"))
                                End If
                            Catch ex As Exception
                            End Try
                            itemCurrencyCd = ""
                            Try
                                itemCurrencyCd = CStr(rdr("CURRENCY_CD"))
                            Catch ex As Exception
                            End Try
                            If itm.Id.Length > 0 Then
                                prc = itm.GetItemPrice
                            Else
                                prc = New sdiItemPrice(unitPrice, New sdiCurrency(itemCurrencyCd, ""))
                            End If
                            ' conversion rate
                            'convRate = sdiMultiCurrency.getConversionRate(itemCurrencyCd, _
                            '                                              unitPrice, _
                            '                                              m_siteCurrency.Id)
                            ' to speed up the load of the orders on the initial screen when all the orders are displayed
                            ' the reason why all the orders are displayed with price total is so they can approve all the orders at once
                            ' the problem is that when you have multiple lines on a stock order there is a call to the currency web-service
                            ' everytime which is a killer.  In this code we want to see if we already have the currency multiplier or if
                            ' we need to call the service at all.
                            ' if we grab it once per order it will correct the timeout issue at AEES and life will be good again South of the border
                            ' and we can all go on a siesta!!!!! Pitchers of Corona for all!
                            'convRate = sdiMultiCurrency.getConversionRate(prc.Currency.Id, _
                            '                                              prc.UnitPrice, _
                            '                                              m_siteCurrency.Id)
                            bGetNonAEESPrice = True
                            Dim decPrice As Decimal
                            Dim str1 As String = iBU
                            If OrderApprovals.IsAEES(iBU) And Trim(sdiItemId) <> "" Then
                                Try
                                    decPrice = CType(GetPriceFromSDiExPrice(sdiItemId), Decimal)
                                    bGetNonAEESPrice = False
                                Catch ex As Exception
                                End Try
                            End If
                            If bGetNonAEESPrice Then
                                decPrice = unitPrice
                                'decPrice = prc.UnitPrice
                            End If

                            If convRate Is Nothing Then
                                convRate = sdiMultiCurrency.getConversionRate(prc.Currency.Id,
                                                                          decPrice,
                                                                          siteCurrency.Id)
                            Else
                                If convRate.SourceCurrencyCode = prc.Currency.Id _
                            And convRate.TargetCurrencyCode = siteCurrency.Id _
                            And convRate.Multiplier <> 0 Then
                                    convRate.SourceAmount = decPrice
                                    convRate.ConvertedAmount = decPrice * convRate.Multiplier / convRate.Divider
                                Else
                                    convRate = sdiMultiCurrency.getConversionRate(prc.Currency.Id,
                                                                          decPrice,
                                                                          siteCurrency.Id)
                                End If
                            End If


                            ' line total
                            convUnitPrice = Math.Round(convRate.ConvertedAmount, 2)
                            ' accumulate grand total
                            orderTotal += (convUnitPrice * qty)
                        End While
                    End If
                    Try
                        rdr.Close()
                    Catch ex As Exception
                    Finally
                        rdr = Nothing
                    End Try
                    Try
                        cmd.Dispose()
                    Catch ex As Exception
                    Finally
                        cmd = Nothing
                    End Try
                Else
                    ' no database connection
                    ' ...192.168.253.17
                End If
                Try
                    cn.Close()
                    cn.Dispose()
                Catch ex As Exception
                Finally
                    cn = Nothing
                End Try
            Catch ex As Exception
            End Try
            Return (orderTotal)
        End Function
        Public Shared Function getOrderTotalMail(ByVal Ibu As String, ByVal orderNo As String,
        ByVal siteCurrency As sdiCurrency, sNSTKOnly As String, strType As String, Optional ByVal intLnID As String = "", Optional ByVal IsAscendLogic As String = "") As Decimal

            Dim orderTotal As Decimal = 0
            Dim lineStatus As String = ""
            'Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
            '' To retrun the order total based on the line status.


            Try
                Dim cn As New OleDbConnection(ORDBData.DbUrl)
                cn.Open()
                If cn.State = ConnectionState.Open Then
                    Dim sql As String = ""
                    Dim ECIBoolean As Boolean = True

                    If IsAscendLogic <> "Y" Then
                        sql = "" &
                      "SELECT " & vbCrLf &
                      " INTFC.BUSINESS_UNIT_OM" & vbCrLf &
                      ",INTFC.BUSINESS_UNIT_BI" & vbCrLf &
                      ",INTFC.ORDER_NO" & vbCrLf &
                      ",INTFC.ISA_INTFC_LN, INTFC.NET_UNIT_PRICE AS NET_UNT_PRC_NOT_AEES" & vbCrLf &
                      ",INTFC.INV_ITEM_ID, F.INV_ITEM_TYPE, F.INV_STOCK_TYPE" & vbCrLf
                        If IsECI(Ibu, ECIBoolean) Then
                            sql = sql &
                    ",DECODE(TRIM(F.CURRENCY_CD), NULL, D.CURRENCY_CD, F.CURRENCY_CD) AS CURRENCY_CD" & vbCrLf &
                      ",DECODE(DECODE(INTFC.NET_UNIT_PRICE,null, 0,INTFC.NET_UNIT_PRICE),INTFC.NET_UNIT_PRICE, D.PRICE_REQ) AS NET_UNIT_PRICE" & vbCrLf
                        Else
                            sql = sql &
                      ",DECODE(TRIM(P.CURRENCY_CD), NULL, DECODE(TRIM(F.CURRENCY_CD), NULL, D.CURRENCY_CD, F.CURRENCY_CD), P.CURRENCY_CD) AS CURRENCY_CD" & vbCrLf &
                      ",DECODE(D.PRICE_REQ, NULL, DECODE(P.PRICE, NULL, INTFC.NET_UNIT_PRICE, P.PRICE), D.PRICE_REQ) AS NET_UNIT_PRICE" & vbCrLf
                        End If
                        sql = sql & ",DECODE(DECODE(D.QTY_REQ, NULL, 0, D.QTY_REQ), 0, D.QTY_REQ, INTFC.QTY_REQUESTED) AS QTY_REQUESTED" & vbCrLf &
                      "FROM " & vbCrLf &
                      " (" & vbCrLf &
                      "  SELECT" & vbCrLf &
                      "   A.BUSINESS_UNIT_OM" & vbCrLf &
                      "  ,C.BUSINESS_UNIT_BI" & vbCrLf &
                      "  ,A.ORDER_NO" & vbCrLf &
                      "  ,B.ISA_INTFC_LN" & vbCrLf &
                      "  ,A.ORDER_STATUS" & vbCrLf &
                      "  ,B.INV_ITEM_ID" & vbCrLf &
                      "  ,B.ISA_LINE_STATUS" & vbCrLf &
                      "  ,B.QTY_REQUESTED" & vbCrLf &
                      "  ,B.ISA_SELL_PRICE AS NET_UNIT_PRICE" & vbCrLf &
                      "  ,B.OPRID_ENTERED_BY" & vbCrLf &
                      "  ,B.OPRID_APPROVED_BY" & vbCrLf &
                      "  FROM SYSADM8.PS_ISA_ORD_INTF_HD A" & vbCrLf &
                      "  ,SYSADM8.PS_ISA_ORD_INTF_LN B" & vbCrLf &
                      "  ,PS_BUS_UNIT_TBL_OM C" & vbCrLf &
                      "  WHERE A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                      "    AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf &
                      "    AND A.BUSINESS_UNIT_OM = '" & Ibu & "' " & vbCrLf &
                      "    AND A.ORDER_NO = '" & orderNo & "' " & vbCrLf &
                      "   AND B.ISA_LINE_STATUS IN ('QTS','QTW') " & vbCrLf
                        If intLnID <> "" Then
                            sql = sql &
                        "    AND B.ISA_INTFC_LN IN (" & intLnID & ") " & vbCrLf
                        End If

                        If sNSTKOnly Then
                            sql = sql &
                            " AND NOT EXISTS (SELECT 'X'" & vbCrLf &
                            " FROM PS_INV_ITEMS C" & vbCrLf &
                            " WHERE C.EFFDT =" & vbCrLf &
                            " (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED" & vbCrLf &
                            " WHERE(C.SETID = C_ED.SETID)" & vbCrLf &
                            " AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID" & vbCrLf &
                            " AND C_ED.EFFDT <= SYSDATE)" & vbCrLf &
                            " AND C.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf &
                            " AND C.INV_STOCK_TYPE = 'STK')" & vbCrLf
                        End If

                        '20171026 - removed INTFC.BUSINESS_UNIT_BI = D.BUSINESS_UNIT (+)" & vbCrLf & _
                        sql = sql & " ) INTFC" & vbCrLf &
                       ",PS_REQ_LINE D" & vbCrLf &
                       ",SYSADM8.PS_ISA_SDIEX_PRCBU P, SYSADM8.PS_ISA_REQ_BI_INFO F " & vbCrLf &
                       "WHERE INTFC.ORDER_NO = D.REQ_ID (+)" & vbCrLf &
                       "  AND INTFC.ISA_INTFC_LN = D.LINE_NBR (+)" & vbCrLf &
                       "  AND TRIM(INTFC.INV_ITEM_ID) = P.INV_ITEM_ID(+) " & vbCrLf &
                       " AND D.BUSINESS_UNIT = F.BUSINESS_UNIT (+) " & vbCrLf &
                       " AND D.REQ_ID = F.REQ_ID (+) " & vbCrLf &
                       " AND D.LINE_NBR = F.LINE_NBR (+) " & vbCrLf &
                        "AND INTFC.BUSINESS_UNIT_OM = P.BUSINESS_UNIT (+) " & vbCrLf &
                       "ORDER BY INTFC.BUSINESS_UNIT_OM" & vbCrLf &
                       ",INTFC.ORDER_NO" & vbCrLf &
                       ",INTFC.ISA_INTFC_LN" & vbCrLf &
                       ""
                    Else
                        sql = "" &
                      "SELECT " & vbCrLf &
                      " INTFC.BUSINESS_UNIT_OM" & vbCrLf &
                      ",INTFC.BUSINESS_UNIT_BI" & vbCrLf &
                      ",INTFC.ORDER_NO" & vbCrLf &
                      ",INTFC.ISA_INTFC_LN, INTFC.NET_UNIT_PRICE AS NET_UNT_PRC_NOT_AEES" & vbCrLf &
                      ",INTFC.INV_ITEM_ID, F.INV_ITEM_TYPE, F.INV_STOCK_TYPE" & vbCrLf
                        If IsECI(Ibu, ECIBoolean) Then
                            sql = sql &
                    ",DECODE(TRIM(F.CURRENCY_CD), NULL, D.CURRENCY_CD, F.CURRENCY_CD) AS CURRENCY_CD" & vbCrLf &
                      ",DECODE(DECODE(INTFC.NET_UNIT_PRICE,null, 0,INTFC.NET_UNIT_PRICE),INTFC.NET_UNIT_PRICE, D.PRICE_REQ) AS NET_UNIT_PRICE" & vbCrLf
                        Else
                            sql = sql &
                      ",DECODE(TRIM(P.CURRENCY_CD), NULL, DECODE(TRIM(F.CURRENCY_CD), NULL, D.CURRENCY_CD, F.CURRENCY_CD), P.CURRENCY_CD) AS CURRENCY_CD" & vbCrLf &
                      ",DECODE(D.PRICE_REQ, NULL, DECODE(P.PRICE, NULL, INTFC.NET_UNIT_PRICE, P.PRICE), D.PRICE_REQ) AS NET_UNIT_PRICE" & vbCrLf
                        End If
                        sql = sql & ",DECODE(DECODE(D.QTY_REQ, NULL, 0, D.QTY_REQ), 0, D.QTY_REQ, INTFC.QTY_REQUESTED) AS QTY_REQUESTED" & vbCrLf &
                      "FROM " & vbCrLf &
                      " (" & vbCrLf &
                      "  SELECT" & vbCrLf &
                      "   A.BUSINESS_UNIT_OM" & vbCrLf &
                      "  ,C.BUSINESS_UNIT_BI" & vbCrLf &
                      "  ,A.ORDER_NO" & vbCrLf &
                      "  ,B.ISA_INTFC_LN" & vbCrLf &
                      "  ,A.ORDER_STATUS" & vbCrLf &
                      "  ,B.INV_ITEM_ID" & vbCrLf &
                      "  ,B.ISA_LINE_STATUS" & vbCrLf &
                      "  ,B.QTY_REQUESTED" & vbCrLf &
                      "  ,B.ISA_SELL_PRICE AS NET_UNIT_PRICE" & vbCrLf &
                      "  ,B.OPRID_ENTERED_BY" & vbCrLf &
                      "  ,B.OPRID_APPROVED_BY" & vbCrLf &
                      "  FROM SYSADM8.PS_ISA_ORD_INTF_HD A" & vbCrLf &
                      "  ,SYSADM8.PS_ISA_ORD_INTF_LN B" & vbCrLf &
                      "  ,PS_BUS_UNIT_TBL_OM C" & vbCrLf &
                      "  WHERE A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                      "    AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf &
                      "    AND A.BUSINESS_UNIT_OM = '" & Ibu & "' " & vbCrLf &
                      "    AND A.ORDER_NO = '" & orderNo & "' " & vbCrLf
                        If intLnID <> "" Then
                            sql = sql &
                        "    AND B.ISA_INTFC_LN IN (" & intLnID & ") " & vbCrLf
                        End If

                        If sNSTKOnly Then
                            sql = sql &
                            " AND NOT EXISTS (SELECT 'X'" & vbCrLf &
                            " FROM PS_INV_ITEMS C" & vbCrLf &
                            " WHERE C.EFFDT =" & vbCrLf &
                            " (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED" & vbCrLf &
                            " WHERE(C.SETID = C_ED.SETID)" & vbCrLf &
                            " AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID" & vbCrLf &
                            " AND C_ED.EFFDT <= SYSDATE)" & vbCrLf &
                            " AND C.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf &
                            " AND C.INV_STOCK_TYPE = 'STK')" & vbCrLf
                        End If

                        '20171026 - removed INTFC.BUSINESS_UNIT_BI = D.BUSINESS_UNIT (+)" & vbCrLf & _
                        sql = sql & " ) INTFC" & vbCrLf &
                       ",PS_REQ_LINE D" & vbCrLf &
                       ",SYSADM8.PS_ISA_SDIEX_PRCBU P, SYSADM8.PS_ISA_REQ_BI_INFO F " & vbCrLf &
                       "WHERE INTFC.ORDER_NO = D.REQ_ID (+)" & vbCrLf &
                       "  AND INTFC.ISA_INTFC_LN = D.LINE_NBR (+)" & vbCrLf &
                       "  AND TRIM(INTFC.INV_ITEM_ID) = P.INV_ITEM_ID(+) " & vbCrLf &
                       " AND D.BUSINESS_UNIT = F.BUSINESS_UNIT (+) " & vbCrLf &
                       " AND D.REQ_ID = F.REQ_ID (+) " & vbCrLf &
                       " AND D.LINE_NBR = F.LINE_NBR (+) " & vbCrLf &
                        "AND INTFC.BUSINESS_UNIT_OM = P.BUSINESS_UNIT (+) " & vbCrLf &
                       "ORDER BY INTFC.BUSINESS_UNIT_OM" & vbCrLf &
                       ",INTFC.ORDER_NO" & vbCrLf &
                       ",INTFC.ISA_INTFC_LN" & vbCrLf &
                       ""
                    End If
                    Try
                        Dim ds As DataSet = ORDBData.GetAdapter(sql)
                        'currentApp.Session("sesds") = ds
                    Catch ex As Exception

                    End Try

                    Dim cmd As OleDbCommand = cn.CreateCommand
                    cmd.CommandText = sql
                    cmd.CommandType = CommandType.Text
                    Dim rdr As OleDbDataReader = Nothing
                    Try
                        rdr = cmd.ExecuteReader
                    Catch ex As Exception
                    End Try
                    If Not (rdr Is Nothing) Then
                        Dim sdiItemId As String = ""
                        Dim qty As Decimal = 0
                        Dim unitPrice As Decimal = 0
                        Dim itemCurrencyCd As String = ""
                        Dim convRate As sdiConversionRate = Nothing
                        Dim convUnitPrice As Decimal = 0
                        While rdr.Read
                            sdiItemId = ""
                            Try
                                sdiItemId = CStr(rdr("INV_ITEM_ID")).Trim.ToUpper
                            Catch ex As Exception
                            End Try
                            qty = 0
                            Try
                                qty = CDec(rdr("QTY_REQUESTED"))
                            Catch ex As Exception
                            End Try
                            unitPrice = 0
                            Try
                                unitPrice = CDec(rdr("NET_UNIT_PRICE"))
                            Catch ex As Exception
                            End Try
                            itemCurrencyCd = ""
                            Try
                                itemCurrencyCd = CStr(rdr("CURRENCY_CD"))
                            Catch ex As Exception
                            End Try

                            '' item type

                            ' itemType - Container.Dataitem("INV_ITEM_TYPE")

                            Dim itmType As String = ""
                            Try
                                itmType = CStr(rdr("INV_ITEM_TYPE")).Trim.ToUpper
                            Catch ex As Exception
                                itmType = ""
                            End Try
                            '' item stock type
                            Dim itmStockType As String = ""

                            ' stockType - Container.Dataitem("INV_STOCK_TYPE")

                            Try
                                itmStockType = CStr(rdr("INV_STOCK_TYPE")).Trim.ToUpper
                            Catch ex As Exception
                                itmStockType = ""
                            End Try

                            If Not IsAEES(Ibu) Then
                                unitPrice = CDec(rdr("NET_UNT_PRC_NOT_AEES"))
                            Else
                                Dim bGetNonAEESPrice As Boolean = True
                                If IsAEES(Ibu) And Trim(sdiItemId) <> "" Then
                                    Try
                                        If Not IsECI(Ibu, ECIBoolean) Then
                                            Dim strNewPrice As String = GetPriceFromSDiExPrice(sdiItemId, "NO")
                                            unitPrice = CType(strNewPrice, Decimal)
                                        End If
                                        bGetNonAEESPrice = False

                                    Catch ex As Exception

                                    End Try
                                End If
                                If bGetNonAEESPrice Then


                                    unitPrice = QuoteNonStockProcessor.ApplyMarkup(unitPrice, itmType, itmStockType, Ibu)

                                End If
                            End If

                            Dim decPrice As Decimal
                            decPrice = unitPrice

                            If convRate Is Nothing Then
                                convRate = sdiMultiCurrency.getConversionRate(itemCurrencyCd,
                                                                          decPrice,
                                                                          siteCurrency.Id)
                            Else
                                If convRate.SourceCurrencyCode = itemCurrencyCd _
                            And convRate.TargetCurrencyCode = siteCurrency.Id _
                            And convRate.Multiplier <> 0 Then
                                    convRate.SourceAmount = decPrice
                                    convRate.ConvertedAmount = decPrice * convRate.Multiplier / convRate.Divider
                                Else
                                    convRate = sdiMultiCurrency.getConversionRate(itemCurrencyCd,
                                                                          decPrice,
                                                                          siteCurrency.Id)
                                End If
                            End If


                            '' line total
                            'convUnitPrice = Math.Round(convRate.ConvertedAmount, 2)
                            ' accumulate grand total
                            orderTotal += Math.Round(convRate.ConvertedAmount * qty, 2)
                        End While
                    End If
                    Try
                        rdr.Close()
                    Catch ex As Exception
                    Finally
                        rdr = Nothing
                    End Try
                    Try
                        cmd.Dispose()
                    Catch ex As Exception
                    Finally
                        cmd = Nothing
                    End Try
                Else
                    ' no database connection
                    ' ...192.168.253.17
                End If
                Try
                    cn.Close()
                    cn.Dispose()
                Catch ex As Exception
                Finally
                    cn = Nothing
                End Try
            Catch ex As Exception
            End Try

            Return (orderTotal)
        End Function

    End Class

    Public Shared Function GetPriceFromSDiExPrice(ByVal strItemID As String, Optional ByVal strToCheck As String = "YES") As String
        ' Note: This function was moved here from clsShoppingCart.vb and renamed from GetNewPrice.

        ' Purpose: Retrieves the price from the new price table for the given item ID.
        ' Adds the site prefix to the item ID if needed before executing the query.
        ' Return value is formatted to 2 decimal places. If a valid value is not read
        ' from the table, defaults to zero.

        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim strDBPrice As String = ""
        Dim ds As DataSet
        Dim bGotPrice As Boolean = False

        'do lookup using item ID with prefix so make sure item ID has prefix
        If Not strItemID.Substring(0, 3) = currentApp.Session("ITEMMODE") Then
            If strToCheck = "YES" Then
                strItemID = PrependSitePrefix(currentApp.Session("ITEMMODE"), strItemID, currentApp.Session("SITEPREFIX"))
            End If
        End If

        ds = GetDataFromSDiExPrice(strItemID)
        Try
            If ds.Tables(0).Rows.Count > 0 Then
                strDBPrice = ds.Tables(0).Rows(0).Item("PRICE")
                If strDBPrice IsNot Nothing Then
                    If IsNumeric(strDBPrice) Then
                        If CType(strDBPrice, Decimal) > 0 Then
                            bGotPrice = True
                        End If
                    End If
                End If
            End If
        Catch ex As Exception

        End Try

        If Not bGotPrice Then
            strDBPrice = "0.00"
        End If

        strDBPrice = String.Format("{0:####,###,##0.0000}", CType(strDBPrice, Decimal))

        Return strDBPrice

    End Function

    Public Shared Function GetDataFromSDiExPrice(ByVal strItemIDs As String) As DataSet

        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim sBusUnit As String = currentApp.Session("BUSUNIT")

        If (strItemIDs Is Nothing) Then
            strItemIDs = ""
        End If

        Dim ds As DataSet = Nothing

        strItemIDs = strItemIDs.Replace("'", "")

        Dim sList As String = ""
        Dim itemList As String() = strItemIDs.Trim.Split(","c)

        For Each s As String In itemList
            If (s.Trim.Length > 0) Then
                sList &= "'" & s.Trim & "',"
            End If
        Next

        sList = sList.TrimEnd(",")
        If (sList.Trim.Length = 0) Then
            ' just so the query below does not grab any unwanted item/s' price/qty and currency if passed value is blank
            sList = "'~'"
        End If
        ' got another field from table SYSADM8.PS_ISA_SDIEX_PRCBU: FIRM_PRICE_FLG - in STAR only - VR 08/07/2018
        'one more field from table SYSADM8.PS_ISA_SDIEX_PRCBU: STATUS_FLG ?? - in STAR only - VR 09/04/2018
        Dim strSQLstring As String = "" & vbCrLf &
             "SELECT " & vbCrLf &
             "  INV_ITEM_ID " & vbCrLf &
             ", PRICE " & vbCrLf &
             ", QTY QTY_ONHAND " & vbCrLf &
             ", CURRENCY_CD, FIRM_PRICE_FLG, SOURCE_FLAG " & vbCrLf &
             " FROM SYSADM8.PS_ISA_SDIEX_PRCBU " & vbCrLf &
             " WHERE INV_ITEM_ID IN (" & sList & ")" & vbCrLf &
             " AND BUSINESS_UNIT='" & sBusUnit & "'" & vbCrLf &
             ""

        Try
            ds = ORDBData.GetAdapter(strSQLstring)
        Catch ex As Exception
        End Try
        If (ds Is Nothing) Then
            ds = New DataSet
        End If

        Return ds
    End Function


    Public Shared Function PrependSitePrefix(ByVal sItemMode As String, ByVal strItemID As String, ByVal strSitePrefix As String) As String
        If sItemMode = "4" Or sItemMode = "6" Then

        Else

            strItemID = strItemID.ToUpper

            ' all Visteon items are setup with "COC" item prefix (in INV_ITEMS table) but with different
            '   PS_BUS_UNIT_TBL_OM.ISA_SITE_CODE. Given that they don't follow the same "item prefixing" convention
            '   this is to get around this issue - erwin 2008.09.04
            Select Case strSitePrefix
                Case "C00"
                    strItemID = "COC" & strItemID
                Case "O00"
                    strItemID = "COC" & strItemID
                Case "S00"
                    strItemID = "COC" & strItemID
                Case "F00"
                    strItemID = "COC" & strItemID
                Case Nothing

                Case Else
                    If Trim(strItemID).Length >= 2 Then
                        If Not strItemID.Trim().StartsWith(strSitePrefix) Then
                            'If the item has the site prefix,  leave it - mr 8/5/14
                            strItemID = strSitePrefix & strItemID
                        End If
                    End If
            End Select
        End If

        Return strItemID
    End Function

    <Serializable()>
    Public Class sdiConversionRate

        Public Sub New()

        End Sub

        Public Sub New(ByVal sourceCurrencyCode As String, ByVal targetCurrencyCode As String)
            m_srcCurrencyCode = sourceCurrencyCode
            m_tgtCurrencyCode = targetCurrencyCode
        End Sub

        Public Sub New(ByVal sourceCurrencyCode As String,
                   ByVal sourceAmount As Decimal,
                   ByVal targetCurrencyCode As String,
                   ByVal convertedAmount As Decimal)
            m_srcCurrencyCode = sourceCurrencyCode
            m_srcAmount = sourceAmount
            m_tgtCurrencyCode = targetCurrencyCode
            m_tgtAmount = convertedAmount
        End Sub

        Public Sub New(ByVal sourceCurrencyCode As String,
                   ByVal targetCurrencyCode As String,
                   ByVal dt As DateTime)
            m_srcCurrencyCode = sourceCurrencyCode
            m_tgtCurrencyCode = targetCurrencyCode
            m_dt = dt
        End Sub

        Private m_srcCurrencyCode As String = ""

        Public Property [SourceCurrencyCode]() As String
            Get
                Return m_srcCurrencyCode
            End Get
            Set(ByVal value As String)
                m_srcCurrencyCode = value
            End Set
        End Property

        Private m_tgtCurrencyCode As String = ""

        Public Property [TargetCurrencyCode]() As String
            Get
                Return m_tgtCurrencyCode
            End Get
            Set(ByVal value As String)
                m_tgtCurrencyCode = value
            End Set
        End Property

        Private m_multiplier As Decimal = CDec("0")

        Public Property [Multiplier]() As Decimal
            Get
                Return m_multiplier
            End Get
            Set(ByVal value As Decimal)
                m_multiplier = value
            End Set
        End Property

        Private m_divider As Decimal = CDec("1")

        Public Property [Divider]() As Decimal
            Get
                Return m_divider
            End Get
            Set(ByVal value As Decimal)
                m_divider = value
            End Set
        End Property

        Private m_bIsKnownConversion As Boolean = False

        Public Property [IsKnownConversionRate]() As Boolean
            Get
                Return m_bIsKnownConversion
            End Get
            Set(ByVal value As Boolean)
                m_bIsKnownConversion = value
            End Set
        End Property

        Private m_dt As DateTime = Now

        Public Property [RateDateTimeStamp]() As DateTime
            Get
                Return m_dt
            End Get
            Set(ByVal value As DateTime)
                m_dt = value
            End Set
        End Property

        Private m_srcAmount As Decimal = CDec("0")

        Public Property [SourceAmount]() As Decimal
            Get
                Return m_srcAmount
            End Get
            Set(ByVal value As Decimal)
                m_srcAmount = value
            End Set
        End Property

        Private m_tgtAmount As Decimal = CDec("0")

        Public Property [ConvertedAmount]() As Decimal
            Get
                Return m_tgtAmount
            End Get
            Set(ByVal value As Decimal)
                m_tgtAmount = value
            End Set
        End Property

    End Class


    Public Shared Function GetSQLReaderDazzle(ByVal p_strQuery As String) As SqlDataReader

        Dim connString As String
        connString = "server=DAZZLE;uid=sa;pwd=sdiadmin;initial catalog=755784;"
        Dim connection As SqlConnection = New SqlConnection(connString)

        Try
            Dim Command As SqlCommand = New SqlCommand(p_strQuery, connection)
            connection.Open()
            Dim datareader As SqlDataReader
            datareader = Command.ExecuteReader(CommandBehavior.CloseConnection)
            Return datareader
        Catch objException As SqlException
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception
                '   SendLogger("Quote Non Stock New Utility - Dev", ex, "ERROR")
            End Try
        End Try
    End Function

    Private Function getBinLoc(ByVal stritemid As String) As String
        'changed to qty >0 rather than hitting the first bin in the array
        Dim strSQLString As String = "" &
                    "SELECT " & vbCrLf &
                    " (C.STORAGE_AREA ||" & vbCrLf &
                    "  C.STOR_LEVEL_1 ||" & vbCrLf &
                    "  C.STOR_LEVEL_2 ||" & vbCrLf &
                    "  C.STOR_LEVEL_3 ||" & vbCrLf &
                    "  C.STOR_LEVEL_4) as binloc " & vbCrLf &
                    "FROM " & vbCrLf &
                    " PS_INV_ITEMS B " & vbCrLf &
                    ",PS_PHYSICAL_INV C " & vbCrLf &
                    "WHERE B.INV_ITEM_ID = '" & stritemid & "' " & vbCrLf &
                    "  AND B.EFFDT = (" & vbCrLf &
                    "                 SELECT MAX(B_ED.EFFDT) FROM PS_INV_ITEMS B_ED " & vbCrLf &
                    "  	 	          WHERE B.SETID = B_ED.SETID " & vbCrLf &
                    "  		            AND B.INV_ITEM_ID = B_ED.INV_ITEM_ID " & vbCrLf &
                    "		            AND B_ED.EFFDT <= SYSDATE" & vbCrLf &
                    "                ) " & vbCrLf &
                    "  AND B.INV_ITEM_ID = C.INV_ITEM_ID(+) " & vbCrLf &
                    " AND C.QTY > 0 " & vbCrLf &
                    "ORDER BY C.DT_TIMESTAMP DESC " & vbCrLf &
                    ""

        Try
            Dim dsbin As DataSet = GetAdapter(strSQLString)
            If Not (dsbin Is Nothing) Then
                If dsbin.Tables(0).Rows.Count > 0 Then
                    For Each row As DataRow In dsbin.Tables(0).Rows
                        getBinLoc = getBinLoc + "<BR>" + CStr(row.Item("BINLOC"))
                    Next
                Else
                    getBinLoc = " "
                End If
            Else
                getBinLoc = " "
            End If
        Catch objException As Exception
            SendLogger("Error in Quoted Non Stock Email Utility", objException, "ERROR")
        End Try
    End Function

    Public Shared Sub buildNotifyApprover(ByVal itmQuoted As QuotedNStkItem)
        Dim strSQLString As String = ""
        Dim strappName As String = ""

        Dim strreqID As String = itmQuoted.OrderID
        Dim strBU As String = itmQuoted.BusinessUnitID
        Dim strAppUserid As String = String.Empty
        Dim strAppAltUserid As String = String.Empty
        Dim stritemid As String = String.Empty

        strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf &
                " LAST_NAME_SRCH," & vbCrLf &
                " ISA_EMPLOYEE_EMAIL" & vbCrLf &
                " FROM PS_ISA_USERS_TBL" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & itmQuoted.EmployeeID & "'"

        Dim dtrAppReader As OleDbDataReader = GetReader(strSQLString)

        If dtrAppReader.HasRows() = True Then
            dtrAppReader.Read()
            strappName = dtrAppReader.Item("FIRST_NAME_SRCH") & " " & dtrAppReader.Item("LAST_NAME_SRCH")
            dtrAppReader.Close()
        Else
            dtrAppReader.Close()
            Exit Sub
        End If

        strSQLString = "SELECT A.ISA_IOL_APR_EMP_ID, A.ISA_IOL_APR_ALT " & vbCrLf &
           " FROM PS_ISA_USERS_APPRV A " & vbCrLf &
           " WHERE ISA_EMPLOYEE_ID = '" & itmQuoted.EmployeeID & "' " & vbCrLf &
           " AND BUSINESS_UNIT = '" & itmQuoted.BusinessUnitOM & "'"

        dtrAppReader = GetReader(strSQLString)
        If dtrAppReader.HasRows() = True Then
            dtrAppReader.Read()
            strAppAltUserid = dtrAppReader.Item("ISA_IOL_APR_EMP_ID")
            strAppUserid = dtrAppReader.Item("ISA_IOL_APR_ALT")
        Else
            dtrAppReader.Close()
            Exit Sub
        End If
        Dim Ident As String = String.Empty
        Dim strHldSts As String = " "  '  String.Empty

        strSQLString = "Select INV_ITEM_ID from SYSADM8.PS_ISA_ORD_INTF_LN WHERE Order_No='" & itmQuoted.OrderID & "' "
        dtrAppReader = GetReader(strSQLString)
        If dtrAppReader.HasRows() = True Then
            dtrAppReader.Read()
            stritemid = dtrAppReader.Item("INV_ITEM_ID")
        Else
            dtrAppReader.Close()
            Exit Sub
        End If

        'BuildLineitem Grid for mail
        Dim dtgcart As DataGrid
        Dim SBstk As New StringBuilder
        Dim SWstk As New StringWriter(SBstk)
        Dim htmlTWstk As New HtmlTextWriter(SWstk)
        Dim dataGridHTML As String = String.Empty
        Dim dstcartSTK As New DataTable
        Dim StrWO1 As String = " "
        dstcartSTK = buildCartforemail(m_colMsgs, itmQuoted.OrderID, StrWO1, itmQuoted.BusinessUnitOM, itmQuoted.EmployeeID)
        If Trim(StrWO1) = "" Then
            StrWO1 = " "
        End If
        itmQuoted.WorkOrderNumber = StrWO1

        If dstcartSTK.Rows.Count > 0 Then
            dtgcart = New DataGrid
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
        End If

        Dim boEncrypt As New Encryption64
        Dim streBU As String = boEncrypt.Encrypt(itmQuoted.BusinessUnitOM, m_cEncryptionKey)        '
        Dim streOrdnum As String = boEncrypt.Encrypt(strreqID, m_cEncryptionKey)
        Dim streApper As String = boEncrypt.Encrypt(strAppAltUserid, m_cEncryptionKey)
        Dim streApperAlt As String = boEncrypt.Encrypt(strAppUserid, m_cEncryptionKey)
        Dim streAppTyp As String = boEncrypt.Encrypt("W", m_cEncryptionKey)
        Dim strhref As String
        Dim strhrefAlt As String
        'Madhu-WW-453-Passing Business unit for  Price Block Flag scenario - [If WAL- Redirect to Walmart Testsite else Zeus]
        'Madhu-Zeus-138-Adding the ZEUS 2.0 LINK If Flag is turned on

        strhref = GetURL(itmQuoted.BusinessUnitOM, itmQuoted.LineStatus) & "?fer=" & streOrdnum & "&op=" & streApper & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"
        strhrefAlt = GetURL(itmQuoted.BusinessUnitOM, itmQuoted.LineStatus) & "?fer=" & streOrdnum & "&op=" & streApperAlt & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"

        If String.Equals(strAppAltUserid.Trim(), strAppUserid.Trim()) Then
            NotifyApprover(strAppUserid, strappName, strreqID, itmQuoted.WorkOrderNumber, stritemid, dataGridHTML, strhref, strHldSts, itmQuoted.BusinessUnitOM, itmQuoted.BusinessUnitID)
        Else
            NotifyApprover(strAppUserid, strappName, strreqID, itmQuoted.WorkOrderNumber, stritemid, dataGridHTML, strhref, strHldSts, itmQuoted.BusinessUnitOM, itmQuoted.BusinessUnitID)
            NotifyApprover(strAppAltUserid, strappName, strreqID, itmQuoted.WorkOrderNumber, stritemid, dataGridHTML, strhrefAlt, strHldSts, itmQuoted.BusinessUnitOM, itmQuoted.BusinessUnitID)
        End If

    End Sub

    Public Shared Sub NotifyApprover(ByVal strappId As String, ByVal strappName As String, ByVal strreqID As String, ByVal WorkOrderNumber As String, ByVal stritemid As String, ByVal dataGridHTML As String, ByVal strhref As String, ByVal strHldSts As String, ByVal BU As String, ByVal PurchasingBU As String)
        Dim strbodyhead As String
        Dim strbodydetl As String

        Dim MailTo As String = String.Empty
        'SDI-40628 Changing Mail id as walmartpurchasing@sdi.com from sdiexchange@sdi.com for Walmart BU.
        Dim MailFrom As String
        'SP-316 Purchasing Email from address change[By Vishalini]
        'Madhu-ZEUS-138-Pass the correct purchasse BU
        MailFrom = getpurchasingEmailFrom(PurchasingBU)
        Dim MailSub As String = String.Empty
        Dim MailBody As String = String.Empty

        Dim AppName As String = String.Empty
        Dim AppMail As String = String.Empty

        Dim strApp As String = "SELECT FIRST_NAME_SRCH," & vbCrLf &
                " LAST_NAME_SRCH," & vbCrLf &
                " ISA_EMPLOYEE_EMAIL" & vbCrLf &
                " FROM PS_ISA_USERS_TBL" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & strappId & "'"
        Dim dtrAppReader As OleDbDataReader = GetReader(strApp)
        If dtrAppReader.HasRows() = True Then
            dtrAppReader.Read()
            AppName = dtrAppReader.Item("FIRST_NAME_SRCH") & " " & dtrAppReader.Item("LAST_NAME_SRCH")
            AppMail = dtrAppReader.Item("ISA_EMPLOYEE_EMAIL")
        End If

        strbodyhead = "<table width='100%'><tbody><tr><td><img src='https://www.sdizeus.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Arial; font-size: x-large; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center; margin: 0px auto;'>SDI ZEUS - Request for Approval</span></center></td></tr></tbody></table>" & vbCrLf
        strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"
        strbodydetl = strbodydetl & "<p ><span style='font-weight:bold;'>TO: </span><span>      </span>" & AppName & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<BR>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Date: </span><span>    </span>" & Now() & "<BR>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Order: </span><span>  </span>" & strreqID & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Work Order: </span><span>  </span>" & WorkOrderNumber & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Item Number:</span><span>  </span>" & stritemid & "<br>"
        strbodydetl = strbodydetl & "&nbsp;</p>"
        strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The above referenced order has been "
        strbodydetl = strbodydetl & "requested by <b>" & strappName & "</b> "

        If IsAscend(BU) Then
            strbodydetl = strbodydetl & "and needs your approval.  Click the link below or select the ""Approve Quotes (Ascend)"" "
        Else
            strbodydetl = strbodydetl & "and needs your approval.  Click the link below or select the ""Approve Orders"" "
        End If

        strbodydetl = strbodydetl & "menu option in SDI ZEUS to approve or reject the order.<br>"
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
        strbodydetl = strbodydetl & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />" & vbCrLf

        MailBody = strbodyhead & strbodydetl


        MailSub = "SDI ZEUS - Order Number " & strreqID & " needs approval"

        MailTo = AppMail
        'Madhu-ZEUS-138-Pass Purchase BU In Send Logger Method

        SendLogger(MailSub, MailBody, "NotifyApprover", "MailandStore", MailTo, String.Empty, "WebDev@sdi.com", PurchasingBU)

    End Sub

    Public Shared Function IsAscend(ByVal sBU As String) As Boolean

        Dim bIsAscend As Boolean = False
        Dim sAscendBUList As String = "~I0440~I0441~I0442~I0443~I0444"
        Try
            bIsAscend = (sAscendBUList.IndexOf(sBU.Trim.ToUpper) > -1)
        Catch ex As Exception
            bIsAscend = False
        End Try
        Return bIsAscend

    End Function
    'Madhu-16592-Display columns for ECI BU 
    Public Shared Function IsECI(ByVal sBU As String, Optional ByVal ECIBoolean As Boolean = True) As Boolean

        Dim bIsECI As Boolean = False
        Dim sECIBUList As String = ""

        Try
            sECIBUList = UCase(ConfigurationSettings.AppSettings("ECIList").ToString)
            If ECIBoolean = True Then
                sECIBUList &= "~I0966"
            End If
        Catch ex As Exception
        End Try

        Try
            bIsECI = (sECIBUList.IndexOf(sBU.Trim.ToUpper) > -1)
        Catch ex As Exception
            bIsECI = False
        End Try
        Return bIsECI
    End Function


    Public Function GetPrice(ByVal OrderID As String) As Decimal
        Dim StrQry As String = "SELECT L.ISA_SELL_PRICE,L.BUSINESS_UNIT_OM,L.QTY_REQUESTED FROM " & vbCrLf &
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

            'If Not IsDBNull(ds.Tables(0).Rows(i)("NET_UNIT_PRICE")) Then
            '    decNetPrice = ds.Tables(0).Rows(i)("NET_UNIT_PRICE")
            'End If

            'If Not IsDBNull(ds.Tables(0).Rows(i)("PRICE_REQ")) Then
            '    decVndPrc = ds.Tables(0).Rows(i)("PRICE_REQ")
            'End If

            If Not IsDBNull(ds.Tables(0).Rows(i)("ISA_SELL_PRICE")) Then
                Try
                    decSellPrc = CDec(ds.Tables(0).Rows(i)("ISA_SELL_PRICE"))
                Catch ex As Exception
                    decSellPrc = 0
                End Try

            End If

            'If Not IsDBNull(ds.Tables(0).Rows(i)("ISA_SELL_PRICE")) Then
            '    decSellPrc = ds.Tables(0).Rows(i)("ISA_SELL_PRICE")
            'End If

            'If Not IsDBNull(ds.Tables(0).Rows(i)("BUSINESS_UNIT_OM")) Then
            '    StrBU = ds.Tables(0).Rows(i)("BUSINESS_UNIT_OM")
            'End If

            If Not IsDBNull(ds.Tables(0).Rows(i)("QTY_REQUESTED")) Then
                Try
                    QtrReq = CDec(ds.Tables(0).Rows(i)("QTY_REQUESTED"))
                Catch ex As Exception
                    QtrReq = 0
                End Try

            End If

            'If decSellPrc > 0 Then
            '    ItmPrice = decSellPrc
            'Else
            '    ItmPrice = ApplyMarkup(decVndPrc, StrItemTyp, strStockTyp, StrBU)
            'End If
            'If ItmPrice = 0 Then
            '    ItmPrice = decNetPrice
            'End If

            ItmPrice = decSellPrc
            OrdTotal = OrdTotal + (ItmPrice * QtrReq)

            i = i + 1
        Next

        Return OrdTotal

    End Function

    Public Shared Function ApplyMarkup(ByVal decVndPrc, ByVal strItemTyp, ByVal strStockTyp, ByVal StrBU) As Decimal
        Dim strSQLstring As String
        Dim strMarkup As String
        Dim decMarkup As Decimal
        strSQLstring = "SELECT A.ISA_MARKUP_RATE" & vbCrLf &
                " FROM PS_ISA_PRICE_RULE A" & vbCrLf &
                " WHERE A.BUSINESS_UNIT = '" & StrBU & "'" & vbCrLf &
                " AND A.ORDER_GRP = '" & strStockTyp & "'" & vbCrLf &
                " AND A.INV_ITEM_TYPE = '" & strItemTyp & "'"

        strMarkup = ORDBData.GetScalar(strSQLstring)
        If strMarkup = "" Then
            strMarkup = "0"
        End If
        decMarkup = FormatNumber(Round(CDbl(decVndPrc) + (CDbl(decVndPrc) * (CDbl(strMarkup) / 100)), 2), 2)
        Return decMarkup

    End Function

End Class


Public Class WebRequestFcmData
    Public Property registration_ids As String()
    Public Property notification As New NotificationData
    Public Property data As New dataBO
End Class
Public Class NotificationData
    Public Property body As String
    Public Property title As String = "ZEUS"
    Public Property sound As String
End Class
Public Class dataBO
    Public Property orderid As String
End Class
'Checklimit begin

'OrderApprovals.vb

Public Class OrderApprovals

    Private Shared m_cDeletedLine As String = "QTR"  '  "D"
    Private Shared m_cClassFileName As String = "OrderApprovals.vb"
    Private Shared m_cApproverDelimiter As String = "||"
    Private Shared m_cClassName As String = "OrderApprovals."

    'Approval process starts hear

    Public Shared Function ApproveQuote(oApprovalDetails As ApprovalDetails, ByRef strAppMessage() As String, ByVal LineStatus As String) As Boolean
        Dim bSuccessful As Boolean = False

        Dim trnsactSession As OleDbTransaction = Nothing
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)

        Try

            connection.Open()
            trnsactSession = connection.BeginTransaction

            If LoadLineItemsIfNeeded(trnsactSession, connection, oApprovalDetails) Then
                If UpdateReqWO(trnsactSession, connection, oApprovalDetails) Then
                    Dim bUpdateError As Boolean = False
                    Dim I As Integer = 0

                    While I < oApprovalDetails.LineDetails.Count And Not bUpdateError
                        Dim oLineDetails As ApprovalDetails.OrderLineDetails
                        oLineDetails = CType(oApprovalDetails.LineDetails(I), ApprovalDetails.OrderLineDetails)

                        If Not UpdateLineItem_ApproveOrderPrepQty(trnsactSession, connection, oApprovalDetails, oLineDetails, LineStatus) Then
                            bUpdateError = True
                        End If

                        If Not bUpdateError Then
                            If UpdateVndrUOMPr(trnsactSession, connection, oApprovalDetails, oLineDetails) Then
                                If oLineDetails.NewDueDt <> oLineDetails.CurrDueDt Then
                                    If Not UpdateReqDueDT(trnsactSession, connection, oApprovalDetails, oLineDetails) Then
                                        bUpdateError = True
                                    End If
                                End If
                            Else
                                bUpdateError = True
                            End If
                        End If
                        I = I + 1
                    End While

                    If Not bUpdateError Then
                        Dim oApprovalResults As ApprovalResults
                        If CheckLimits(trnsactSession, connection, oApprovalDetails, oApprovalResults, LineStatus) Then
                            If RecordApprovalHistory(trnsactSession, connection, oApprovalDetails, ApprovalHistory.ApprHistStatus.Pending, ApprovalHistory.ApprHistType.QuoteApproval) Then
                                Dim bError As Boolean = False
                                If Not oApprovalResults.IsMoreApproversNeeded Then
                                    If Not UpdateOrderStatus_FullyApprove(trnsactSession, connection, oApprovalDetails,
                                        ApprovalHistory.ApprHistType.QuoteApproval, LineStatus) Then
                                        bError = True
                                    End If
                                End If

                                If Not bError Then
                                    buildNotifyBuyer(oApprovalDetails.ReqID, oApprovalDetails.WorkOrder,
                                        oApprovalDetails.EnteredByID, oApprovalDetails.BU, oApprovalDetails.ApproverID,
                                        "APPROVED", "quote", oApprovalResults.IsMoreApproversNeeded)

                                    If oApprovalResults.OrderExceededLimit Then
                                        strAppMessage = New String(0) {}
                                        strAppMessage(0) = NotifyApprover(oApprovalDetails.ReqID, oApprovalDetails.ApproverID,
                                            oApprovalDetails.BU, oApprovalResults.NextOrderApprover, oApprovalResults.NewOrderHeaderStatus, "")

                                    ElseIf oApprovalResults.IsAnyChargeCodeExceededLimit Then
                                        strAppMessage = New String(oApprovalResults.BudgetChargeCodesCount - 1) {}
                                        For I = 0 To strAppMessage.Length - 1
                                            If oApprovalResults.BudgetExceededLimit(I) Then
                                                strAppMessage(I) = NotifyApprover(oApprovalDetails.ReqID, oApprovalDetails.ApproverID,
                                                    oApprovalDetails.BU, oApprovalResults.NextBudgetApprover(I),
                                                    oApprovalResults.NewBudgetHeaderStatus(I), oApprovalResults.BudgetChargeCode(I))
                                            End If
                                            I = I + 1
                                        Next
                                    Else
                                        RecordApprovalHistory(trnsactSession, connection, oApprovalDetails, ApprovalHistory.ApprHistStatus.Approve, ApprovalHistory.ApprHistType.OrderApproval)
                                    End If

                                    trnsactSession.Commit()
                                    connection.Close()
                                    trnsactSession = Nothing
                                    connection = Nothing

                                    bSuccessful = True
                                End If
                            End If
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            Try
                trnsactSession.Rollback()
                connection.Close()
                trnsactSession = Nothing
                connection = Nothing
            Catch ex22 As Exception

            End Try
        End Try

        Return bSuccessful
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
                            oApprovalDetails.AddLineDetailsForOrder(iLineNbr,
                                                                    decQtyReq,
                                                                    row.Item("INV_STOCK_TYPE").ToString,
                                                                    row.Item("ISA_CUST_CHARGE_CD").ToString,
                                                                    decUnitPrice,
                                                                    row.Item("INV_ITEM_ID").ToString,
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
            HandleApprovalError(trnsactSession, connection, ex, oApprovalDetails)
        End Try

        Return bSuccess
    End Function

    Private Shared Function UpdateReqWO(trnsactSession As OleDbTransaction, connection As OleDbConnection,
                                       oApprovalDetails As ApprovalDetails) As Boolean

        Const cCaller As String = "OrderApprovals.UpdateReqWO"

        Dim bSuccessful As Boolean = False
        Dim strSQLstring As String
        Dim rowsaffected As Integer

        Try
            'update SYSADM8.PS_ISA_REQ_BI_INFO with the WOrk_order#

            Dim sWorkOrder As String = oApprovalDetails.WorkOrder.Replace("'", "''")

            strSQLstring = "UPDATE SYSADM8.PS_ISA_REQ_BI_INFO L" & vbCrLf &
          " SET L.ISA_WORK_ORDER_NO = '" & sWorkOrder & "' where " & vbCrLf &
          " L.REQ_ID = '" & oApprovalDetails.ReqID & "'"

            If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                If rowsaffected = 0 Then
                    bSuccessful = False
                    HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                Else
                    bSuccessful = True
                End If
            End If

        Catch ex As Exception
            bSuccessful = False
            HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
        End Try

        Return bSuccessful
    End Function

    Private Shared Function UpdateLineItem_ApproveOrderPrepQty(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
       oApprovalDetails As ApprovalDetails, oLineDetails As ApprovalDetails.OrderLineDetails, ByVal Linestatus As String) As Boolean

        Const cCaller As String = "OrderApprovals.UpdateLineItem_ApproveOrderPrepQty"

        Dim bSuccessful As Boolean = False
        Dim strSQLstring As String
        Dim rowsaffected As Integer

        Try

            strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN" & vbCrLf &
                " SET QTY_REQUESTED = " & oLineDetails.QtyReq

            If oLineDetails.DeleteItem Then
                strSQLstring &= ", ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
            Else
                strSQLstring &= ", ISA_LINE_STATUS = '" & Linestatus & "' " & vbCrLf 'Yury 20170920
            End If

            Dim sWorkOrder As String = oApprovalDetails.WorkOrder.Replace("'", "''")

            strSQLstring &= ", ISA_SELL_PRICE = " & oLineDetails.UnitPrice & vbCrLf &
            ", ISA_WORK_ORDER_NO = '" & sWorkOrder & "' "

            strSQLstring &= " WHERE ORDER_NO = " & vbCrLf &
                " (SELECT A.ORDER_NO" & vbCrLf &
                "       FROM SYSADM8.PS_ISA_ORD_INTF_HD A" & vbCrLf &
                "       WHERE A.BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "'" & vbCrLf &
                "       AND A.ORDER_NO = '" & oApprovalDetails.ReqID & "' "

            strSQLstring &= " ) " & vbCrLf &
                " AND ISA_INTFC_LN = " & oLineDetails.LineNbr

            rowsaffected = ORDBData.ExecNonQuery(strSQLstring, False)
            If rowsaffected = 0 Then
                bSuccessful = False
                HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
            Else
                bSuccessful = True

                Dim sErrorDetails As String = ""
                Const cFuncDescr As String = "Approve order"
                Const cTableName As String = "SYSADM8.PS_ISA_ORD_INTF_LN"
                Dim sISALineStatusChg As String = ""

                If oLineDetails.DeleteItem Then
                    sISALineStatusChg = m_cDeletedLine
                    If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName, "Approve Quote", cTableName,
                    oApprovalDetails.ApproverID, oApprovalDetails.BU, oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sKey02:=oLineDetails.LineNbr,
                    sNewValue:=sISALineStatusChg, sErrorDetails:=sErrorDetails) Then
                        bSuccessful = True
                    Else
                        HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        bSuccessful = False
                    End If
                End If

            End If



        Catch ex As Exception
            bSuccessful = False
            HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
        End Try

        Return bSuccessful
    End Function


    Private Shared Function UpdateVndrUOMPr(trnsactSession As OleDbTransaction, connection As OleDbConnection,
                               oApprovalDetails As ApprovalDetails, oLineDetails As ApprovalDetails.OrderLineDetails) As Boolean

        Const cCaller As String = "OrderApprovals.UpdateVndrUOMPr"

        Dim bSuccessful As Boolean = True ' The default is a success
        Dim strSQLstring As String
        Dim rowsaffected As Integer

        Try
            strSQLstring = "SELECT B.INV_ITEM_ID" & vbCrLf &
                    " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B" & vbCrLf &
                    " WHERE A.BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "'" & vbCrLf &
                    " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                    " AND A.ORDER_NO = '" & oApprovalDetails.ReqID & "'" & vbCrLf &
                    " AND B.ISA_INTFC_LN = " & oLineDetails.LineNbr & vbCrLf &
                    " AND B.ISA_SELL_PRICE = 0.0000"

            Dim strInvItemID As String = ORDBData.GetScalar(strSQLstring)
            If strInvItemID IsNot Nothing Then
                If strInvItemID.Trim <> "" Then
                    strSQLstring = "SELECT A.INV_STOCK_TYPE" & vbCrLf &
                                " FROM PS_INV_ITEMS A" & vbCrLf &
                                " WHERE A.EFFDT =" & vbCrLf &
                                " (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf &
                                " WHERE(A.SETID = A_ED.SETID)" & vbCrLf &
                                " AND A.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf &
                                " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf &
                                " AND A.SETID = 'MAIN1'" & vbCrLf &
                                " AND A.INV_ITEM_ID = '" & strInvItemID & "'"

                    Dim strStockType As String = ORDBData.GetScalar(strSQLstring)

                    If strStockType = "ORO" Then
                        strSQLstring = "SELECT A.VENDOR_SETID, A.VENDOR_ID," & vbCrLf &
                                    " A.UNIT_OF_MEASURE, A.PRICE_REQ" & vbCrLf &
                                    " FROM PS_REQ_LINE A" & vbCrLf &
                                    " WHERE A.BUSINESS_UNIT = '" & oApprovalDetails.SiteBU & "'" & vbCrLf &
                                    " AND A.REQ_ID = '" & oApprovalDetails.ReqID & "'" & vbCrLf &
                                    " AND A.LINE_NBR = " & oLineDetails.LineNbr & vbCrLf

                        Dim dsReq As DataSet = ORDBData.GetAdapter(strSQLstring)
                        If dsReq.Tables(0).Rows.Count > 0 Then
                            Dim dteNowy As String = Now().ToString("yyyy-M-d")
                            strSQLstring = "INSERT INTO PS_ITM_VNDR_UOM_PR" & vbCrLf &
                                        "(SETID, INV_ITEM_ID," & vbCrLf &
                                        " VENDOR_SETID," & vbCrLf &
                                        " VENDOR_ID," & vbCrLf &
                                        " VNDR_LOC, UNIT_OF_MEASURE," & vbCrLf &
                                        " CURRENCY_CD, QTY_MIN," & vbCrLf &
                                        " EFFDT," & vbCrLf &
                                        " EFF_STATUS, PRICE_VNDR," & vbCrLf &
                                        " UNIT_PRC_TOL," & vbCrLf &
                                        " EXT_PRC_TOL, USE_STD_TOLERANCES, PCT_UNIT_PRC_TOL," & vbCrLf &
                                        " PCT_EXT_PRC_TOL, QTY_RECV_TOL_PCT)" & vbCrLf &
                                        " Values ('MAIN1', '" & strInvItemID & "'," & vbCrLf &
                                        " '" & dsReq.Tables(0).Rows(0).Item("VENDOR_SETID") & "'," & vbCrLf &
                                        " '" & dsReq.Tables(0).Rows(0).Item("VENDOR_ID") & "'," & vbCrLf &
                                        " '1', '" & dsReq.Tables(0).Rows(0).Item("UNIT_OF_MEASURE") & "'," & vbCrLf &
                                        " 'USD', 1.0000, " & vbCrLf &
                                        " TO_DATE('" & dteNowy & "', 'YYYY/MM/DD')," & vbCrLf &
                                        " 'A', '" & dsReq.Tables(0).Rows(0).Item("PRICE_REQ") & "'," & vbCrLf &
                                        " 0.00000," & vbCrLf &
                                        " 0.00000, 'Y', 0.00," & vbCrLf &
                                        " 0.00, 0.00)" & vbCrLf

                            ' Execute without determining if a row was inserted. Even if the insert fails,
                            ' it is not considered enough of an error to stop quote approval.
                            Dim exError As Exception
                            ExecuteNonQuery(trnsactSession, connection, strSQLstring, rowsaffected, exError)
                        End If
                    End If
                End If
            End If

        Catch ex As Exception
            bSuccessful = False
            HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
        End Try

        Return bSuccessful
    End Function

    Private Shared Function UpdateReqDueDT(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
       oApprovalDetails As ApprovalDetails, oLineDetails As ApprovalDetails.OrderLineDetails) As Boolean

        Const cCaller As String = "OrderApprovals.UpdateReqDueDT"

        Dim bSuccessful As Boolean = False
        Dim strSQLstring As String
        Dim rowsaffected As Integer

        Try
            Dim dteDueDty As String = oLineDetails.NewDueDt.ToString("yyyy-M-d")
            strSQLstring = "UPDATE SYSADM8.PS_REQ_LINE_SHIP" & vbCrLf &
                    " SET DUE_DT = TO_DATE('" & dteDueDty & "', 'YYYY/MM/DD')" & vbCrLf &
                    " WHERE BUSINESS_UNIT = '" & oApprovalDetails.SiteBU & "'" & vbCrLf &
                    " AND REQ_ID = '" & oApprovalDetails.ReqID & "'" & vbCrLf &
                    " AND LINE_NBR = " & oLineDetails.LineNbr & vbCrLf

            ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller)
            If rowsaffected = 0 Then
                ' This is not considered enough of an error to stop the quote approval
            End If

            bSuccessful = True
        Catch ex As Exception
            bSuccessful = False
            HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
        End Try

        Return bSuccessful
    End Function

    Private Shared Function RecordApprovalHistory(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
       oApprovalDetails As ApprovalDetails, eApprHist As ApprovalHistory.ApprHistStatus,
       eApprType As ApprovalHistory.ApprHistType) As Boolean

        Dim sErrorDetails As String = ""
        Dim bSuccess As Boolean = False

        If ApprovalHistory.RecordApprovalType(trnsactSession, connection, eApprHist, eApprType, oApprovalDetails, sErrorDetails) Then
            bSuccess = True
        Else
            HandleApprovalError(trnsactSession, connection, m_cClassName & "RecordApprovalHistory", oApprovalDetails, sErrorDetails)
        End If

        Return bSuccess
    End Function

    Private Shared Function UpdateOrderStatus_FullyApprove(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
    oApprovalDetails As ApprovalDetails, eApprType As ApprovalHistory.ApprHistType, ByVal Linestatus As String) As Boolean

        Dim bSuccess As Boolean = False

        'If UpdateLineItem_FullyApproved(trnsactSession, connection, oApprovalDetails, eApprType) Then
        If UpdateHeader_FullyApproved(trnsactSession, connection, oApprovalDetails, eApprType, Linestatus) Then
            bSuccess = True
        End If
        'End If

        Return bSuccess
    End Function

    Private Shared Function UpdateHeader_FullyApproved(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
           ByVal oApprovalDetails As ApprovalDetails, eApprType As ApprovalHistory.ApprHistType, ByVal Linestatus As String) As Boolean

        Const cCaller As String = "OrderApprovals.UpdateHeader_FullyApproved"

        Dim bSuccess As Boolean = False
        Dim strSQLstring As String
        Dim rowsaffected As Integer
        Dim sNewLineStatus As String = ""
        sNewLineStatus = GetFullyApprovedStatus(oApprovalDetails.BU)

        Try
            Dim objEnterpriseTbl As New clsEnterprise(oApprovalDetails.BU)
            Dim strLastUPDOprid As String = objEnterpriseTbl.LastUPDOprid
            Dim sNewStatus As String = ""
            If strLastUPDOprid = "XXXXXXXX" Then
                sNewStatus = "X"
            Else

                sNewStatus = "P"
            End If

            strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD  " & vbCrLf &
                " SET " & vbCrLf &
                " LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf &
                ", ORDER_STATUS = '" & sNewStatus & "' " & vbCrLf &
                " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf &
                " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

            If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                If rowsaffected = 0 Then
                    bSuccess = False
                    HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                Else
                    rowsaffected = 0
                    strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN " & vbCrLf &
                   " SET " & vbCrLf &
                   " OPRID_APPROVED_BY = '" & oApprovalDetails.ApproverID & "' " & vbCrLf &
                   ", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf &
                   ", ISA_LINE_STATUS = '" & Linestatus & "' " & vbCrLf &
                   " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf &
                   " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                    If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                        If rowsaffected = 0 Then
                            bSuccess = False
                            HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                        Else
                            Dim sErrorDetails As String = ""
                            If sNewStatus = "|~|" Then
                                bSuccess = True
                            Else
                                Dim sFuncDescr As String = ""
                                If eApprType = ApprovalHistory.ApprHistType.QuoteApproval Then
                                    sFuncDescr = "Approve quote"
                                Else
                                    sFuncDescr = "Approve order"
                                End If

                                Dim sOprID As String = oApprovalDetails.ApproverID.Trim
                                If sOprID.Length = 0 Then

                                    sOprID = oApprovalDetails.ApproverID
                                End If

                                If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName,
                                    sFuncDescr, "SYSADM8.PS_ISA_ORD_INTF_HD", sOprID, oApprovalDetails.BU,
                                    oApprovalDetails.ReqID, sColumnChg:="ORDER_STATUS", sNewValue:=sNewStatus, sErrorDetails:=sErrorDetails) Then

                                    bSuccess = True
                                Else
                                    HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            bSuccess = False
            HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
        End Try

        Return bSuccess
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

    Public Shared Function GetPriorityFromIntfLn(ByVal strreqID As String, ByVal strBU As String) As String

        Dim strSQLString As String = ""
        Dim strPriority As String = " "
        strSQLString = "SELECT ISA_PRIORITY_FLAG FROM SYSADM8.PS_ISA_ORD_INTF_LN WHERE order_no = '" & strreqID & "' " & vbCrLf &
            " AND business_unit_om = '" & strBU & "'"
        Dim dtrPriorityOrder As OleDbDataReader = ORDBData.GetReader(strSQLString, False)
        If dtrPriorityOrder.HasRows() Then
            While dtrPriorityOrder.Read()
                Try
                    If dtrPriorityOrder.Item("ISA_PRIORITY_FLAG").ToString().Trim().ToUpper() = "R" Then
                        strPriority = "*PRIORITY*"
                    End If
                Catch ex As Exception
                    strPriority = " "
                End Try
            End While

        End If
        dtrPriorityOrder.Close()

        Return strPriority

    End Function
    'Madhu-INC0015106-Removed avacorp in Email flow

    Public Shared Sub buildNotifyBuyer(ByVal strreqID As String,
                                ByVal strwo As String,
                                ByVal strAgent As String,
                                ByVal strBU As String,
                                ByVal strAppUserid As String,
                                ByVal strAction As String,
                                ByVal strType As String,
                                ByVal bMoreAppr As Boolean)
        'strreid = order no
        'strAgent = if quote then SDI buyer else customer buying items
        'strBU = customer BU
        'strAppUserid = person that is approving
        'strAction = either "APPROVE" or "DECLINE"
        'strType = either "quote" or "order"
        Dim strSQLString As String
        Dim strMessage As New Alert
        Dim strBuyerName As String
        Dim strBuyerEmail As String
        Dim strApproverName As String
        Dim strSDIBuyer As String

        'I know the below code looks like a duplication 
        'but I needed to get the SDI buyers email for approvals notifications
        'and I didn't want to break what was already working
        'Bob D - 01/20/2005
        Dim objEnterprise As New clsEnterprise(strBU)
        strSDIBuyer = objEnterprise.NONSKREQEmail

        If strType = "quote" Then
            strSQLString = "SELECT A.ISA_NONSKREQ_EMAIL" & vbCrLf &
               " FROM PS_ISA_ENTERPRISE A" & vbCrLf &
               " WHERE A.ISA_BUSINESS_UNIT = '" & strBU & "'"
        Else
            strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf &
               " LAST_NAME_SRCH," & vbCrLf &
               " ISA_EMPLOYEE_EMAIL" & vbCrLf &
               " FROM SDIX_USERS_TBL" & vbCrLf &
               " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
               " AND ISA_EMPLOYEE_ID = '" & strAgent & "'"
        End If

        Dim dtrBuyReader As OleDbDataReader = ORDBData.GetReader(strSQLString, False)

        ' if no record found for buyer then exit
        If dtrBuyReader.HasRows() = True Then
            dtrBuyReader.Read()
        Else
            dtrBuyReader.Close()
            Exit Sub
        End If
        If strType = "quote" Then
            strBuyerName = dtrBuyReader.Item("ISA_NONSKREQ_EMAIL")
            strBuyerEmail = dtrBuyReader.Item("ISA_NONSKREQ_EMAIL")
        Else
            strBuyerName = dtrBuyReader.Item("FIRST_NAME_SRCH") &
            " " & dtrBuyReader.Item("LAST_NAME_SRCH")
            strBuyerEmail = dtrBuyReader.Item("ISA_EMPLOYEE_EMAIL")

        End If
        dtrBuyReader.Close()

        strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf &
                " LAST_NAME_SRCH," & vbCrLf &
                " ISA_EMPLOYEE_EMAIL" & vbCrLf &
                " FROM SDIX_USERS_TBL" & vbCrLf &
                " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                " AND ISA_EMPLOYEE_ID = '" & strAppUserid & "'"


        Dim dtrAppReader As OleDbDataReader = ORDBData.GetReader(strSQLString, False)

        If dtrAppReader.HasRows() = True Then
            dtrAppReader.Read()
            strApproverName = dtrAppReader.Item("FIRST_NAME_SRCH") &
                  " " & dtrAppReader.Item("LAST_NAME_SRCH")
        Else
            strApproverName = strAppUserid
        End If

        dtrAppReader.Close()

        Dim strPriority As String = ""
        strPriority = GetPriorityFromIntfLn(strreqID, strBU)

        'strSQLString = "SELECT ISA_PRIORITY_FLAG FROM SYSADM8.PS_ISA_ORD_INTF_LN WHERE order_no = '" & strreqID & "' " & vbCrLf & _
        '    " AND business_unit_om = '" & strBU & "'"
        'Dim dtrPriorityOrder As OleDbDataReader = ORDBData.GetReader(strSQLString)
        'If dtrPriorityOrder.HasRows() Then
        '    dtrPriorityOrder.Read()
        '    Try
        '        If dtrPriorityOrder.Item("ISA_PRIORITY_FLAG").ToString().Trim().ToUpper() = "R" Then
        '            strPriority = "*PRIORITY*"
        '        End If
        '    Catch ex As Exception
        '    End Try
        'End If
        'dtrPriorityOrder.Close()

        Dim strbodyhead As String = ""
        Dim strbodydetl As String = ""
        'Dim txtBody As String
        'Dim txtHdr As String
        'Dim txtMsg As String

        Dim MailFrom As String = ""
        Dim MailTo As String = ""
        Dim MailCc As String = " "
        Dim MailBcc As String = " "
        Dim MailSub As String = ""
        Dim MailBody As String = ""
        'Dim Mailbody As String
        'Dim Mailer As MailMessage = New MailMessage
        'Mailer.From = "SDIExchange@SDI.com"
        'Mailer.Cc = ""
        'Mailer.Bcc = ""
        'SDI-40628 Changing Mail id as walmartpurchasing@sdi.com from sdiexchange@sdi.com for Walmart BU.
        'SP-316 Purchasing Email from address change
        Dim sqlStringEmailFrom As String = ""
        sqlStringEmailFrom = "Select ISA_PURCH_EML_FROM from PS_ISA_BUS_UNIT_PM where BUSINESS_UNIT_PO ='" & strBU & "'"
        MailFrom = ORDBData.GetScalar(sqlStringEmailFrom)
        'strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
        'strbodyhead = strbodyhead & "<center><span >SDiExchange - Request for Approval</span></center>"
        'strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        strbodyhead = "<table bgcolor='black' Width='100%'><tbody><tr><td style='width:1%;'><img src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td>" & vbCrLf
        strbodyhead = strbodyhead & "<td style='width:50% ;'><center><span style='font-weight:bold;color:white;font-size:24px;'>SDI Marketplace</span></center>" & vbCrLf
        strbodyhead = strbodyhead & "<center><span style='color:white;'>SDI ZEUS - Request for Approval</span></center></td></tr></tbody></table>" & vbCrLf
        strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>"

        ' strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = "<div>"
        strbodydetl = strbodydetl & "<p >TO:<span>      </span>" & strBuyerName & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<BR>"
        strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "Order:<span>  </span>" & strreqID & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "Work Order:<span>  </span>" & strwo & "<br>"
        strbodydetl = strbodydetl & "&nbsp;</p>"
        strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The above referenced " & strType & " has been "
        strbodydetl = strbodydetl & strAction
        If strPriority.Trim().Length > 0 Then
            strbodydetl = strbodydetl & " " & strPriority
        End If
        strbodydetl = strbodydetl & " by <b>" & strApproverName & "</b>.  "
        If Not bMoreAppr Then
            strbodydetl = strbodydetl & "<br>"
        Else
            strbodydetl = strbodydetl & "Additional approvals are still required before order can be processed.<br>"
        End If
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "Sincerely,<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "SDI Customer Care<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "</p>"
        strbodydetl = strbodydetl & "</div>"
        strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodydetl = strbodydetl & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />"

        ' Mailer.Body = strbodyhead & strbodydetl
        MailBody = strbodyhead & strbodydetl
        If Not getDBName() Then
            ' Mailer.To = "DoNotSendPLGR@sdi.com"
            MailTo = "WebDev@sdi.com"
            MailSub = "<<TEST SITE>> Order # " & strreqID & " has been " & strAction
        Else
            'Mailer.To = strBuyerEmail
            MailTo = strBuyerEmail
            MailSub = "Order # " & strreqID & " has been " & strAction
            If Not bMoreAppr And
                Not strType = "quote" Then
                'Mailer.To = strBuyerEmail & "; " & strSDIBuyer
                MailTo = strBuyerEmail & "; " & strSDIBuyer
            End If
        End If


        If strPriority.Trim().Length > 0 Then
            ' Mailer.Subject = Mailer.Subject & " " & strPriority
            MailSub = MailSub & " " & strPriority
        End If

        Dim EmailOut As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
        Try
            EmailOut.EmailUtilityServices("Mail", MailFrom, MailTo, MailSub, MailCc, MailBcc, MailBody, "REQAPPROVAL", MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception
            Dim strErr As String = ex.Message
        End Try

        dtrAppReader.Close()

    End Sub

    Public Class Alert
        Public Function Say(ByVal Message As String) As String
            ' Format string properly
            Message = Message.Replace("'", "\'")
            Message = Message.Replace(Convert.ToChar(10), "\n")
            Message = Message.Replace(Convert.ToChar(13), "")
            ' Display as JavaScript alert
            'ltlAlert.Text
            Dim strMessage As String = "alert('" & Message & "')"
            Return strMessage
        End Function

    End Class

    Public Shared Function NotifyApprover(ByVal strReqID As String, ByVal strCurrentUserID As String, ByVal strBU As String, ByVal strNextApproverID As String,
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

    Private Shared Function GetOriginalApprover(ByVal strBU As String, ByVal strLastApprover As String, ByVal strAlternateApproverID As String) As String
        Dim strOrigApproverID As String = ""

        Try
            Dim ds As DataSet
            Dim strSQLstring As String

            strSQLstring = "SELECT isa_iol_apr_emp_id" & vbCrLf &
                " FROM SDIX_USERS_APPRV" & vbCrLf &
                " WHERE isa_iol_apr_alt = '" & strAlternateApproverID & "'" & vbCrLf &
                " AND isa_employee_id = '" & strLastApprover & "'" & vbCrLf
            If strBU <> "I0W01" Then
                strSQLstring &= " AND business_unit = '" & strBU & "'"
            End If
            ds = ORDBData.GetAdapter(strSQLstring)
            If ds.Tables(0).Rows.Count > 0 Then
                strOrigApproverID = ds.Tables(0).Rows(0).Item("isa_iol_apr_emp_id").ToString
            End If

        Catch ex As Exception
        End Try

        Return strOrigApproverID
    End Function
    'Madhu-INC0015106-Removed avacorp in Email flow

    Public Shared Sub buildNotifyApprover(ByVal strreqID As String, ByVal strAgent As String, ByVal strBU As String, ByVal strAppUserid As String, ByVal strHldStatus As String)
        'this is where we will put in the description of the order per S.Roudriquez
        'pfd
        'Gives us a reference to the current asp.net 
        'application executing the method.


        Dim strSQLString As String
        Dim strappName As String
        Dim strappEmail As String
        Dim strBuyerName As String = " "

        strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf &
                " LAST_NAME_SRCH," & vbCrLf &
                " ISA_EMPLOYEE_EMAIL" & vbCrLf &
                " FROM SDIX_USERS_TBL" & vbCrLf &
                " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
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

        strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf &
                " LAST_NAME_SRCH," & vbCrLf &
                " ISA_EMPLOYEE_EMAIL" & vbCrLf &
                " FROM SDIX_USERS_TBL" & vbCrLf &
                " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
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
        '    setServer()
        'End If



        'steph's request
        'pfd
        '1/02/2009
        ' get the detail line for the the approver quote email
        strSQLString = "SELECT ' ' AS ISA_IDENTIFIER," & vbCrLf &
                " A.ORDER_NO, B.OPRID_ENTERED_BY," & vbCrLf &
                " TO_CHAR(B.ISA_REQUIRED_BY_DT,'YYYY-MM-DD') as REQ_DT," & vbCrLf &
                " TO_CHAR(A.ADD_DTTM,'YYYY-MM-DD') as ADD_DT," & vbCrLf &
                " B.ISA_EMPLOYEE_ID, B.ISA_CUST_CHARGE_CD," & vbCrLf &
                " B.ISA_WORK_ORDER_NO, B.ISA_MACHINE_NO," & vbCrLf &
                " ' ' AS ISA_CUST_NOTES," & vbCrLf &
                " B.INV_ITEM_ID, B.DESCR254, B.MFG_ID," & vbCrLf &
                " B.ISA_MFG_FREEFORM, B.MFG_ITM_ID," & vbCrLf &
                " B.UNIT_OF_MEASURE," & vbCrLf &
                " ' ' AS VNDR_CATALOG_ID, B.ISA_SELL_PRICE," & vbCrLf &
                " (B.ORDER_NO || B.ISA_INTFC_LN) as UNIQUEIDENT, B.ISA_INTFC_LN," & vbCrLf &
                " B.ISA_LINE_STATUS," & vbCrLf &
                " C.QTY_REQ, C.PRICE_REQ, F.ISA_SELL_PRICE," & vbCrLf &
                " F.INV_ITEM_TYPE, F.INV_STOCK_TYPE," & vbCrLf &
                " TO_CHAR(D.DUE_DT,'YYYY-MM-DD') as DUE_DT" & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B," & vbCrLf &
                " PS_REQ_LINE C, PS_REQ_LINE_SHIP D, SYSADM8.PS_ISA_REQ_BI_INFO F" & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf &
                " AND A.ORDER_NO = '" & strreqID & "'" & vbCrLf &
                " AND B.OPRID_ENTERED_BY = '" & strAgent & "'" & vbCrLf &
                " AND B.ISA_LINE_STATUS = 'QTS'" & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                " AND B.QTY_REQUESTED > 0" & vbCrLf &
                " AND A.ORDER_NO = C.REQ_ID" & vbCrLf &
                " AND B.ISA_INTFC_LN = C.LINE_NBR" & vbCrLf &
                " AND C.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf &
                " AND C.REQ_ID = D.REQ_ID" & vbCrLf &
                " AND C.LINE_NBR = D.LINE_NBR" & vbCrLf &
                " AND C.BUSINESS_UNIT = F.BUSINESS_UNIT (+)" & vbCrLf &
                " AND C.REQ_ID = F.REQ_ID (+)" & vbCrLf &
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
        Dim Employeeid As String = dtOrder.Rows(0).Item("ISA_EMPLOYEE_ID")

        Dim dstcartSTK As New DataTable()

        dstcartSTK = buildCartforemail(dtOrder, strreqID, Employeeid)



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
        Dim stritemid As String = " "
        Dim strPriority As String = " "
        strPriority = GetPriorityFromIntfLn(strreqID, strBU)

        strhref = ConfigurationManager.AppSettings("WebAppName") & "approveorder.aspx?fer=" & streOrdnum & "&op=" & streApper & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"
        Dim StrResult As String = String.Empty

        Dim Mailer As MailMessage = New MailMessage
        'SDI-40628 Changing Mail id as walmartpurchasing@sdi.com from sdiexchange@sdi.com for Walmart BU.
        'SP-316 Purchasing Email from address change
        Dim sqlStringEmailFrom As String = ""
        sqlStringEmailFrom = "Select ISA_PURCH_EML_FROM from PS_ISA_BUS_UNIT_PM where BUSINESS_UNIT_PO ='" & strBU & "'"
        Mailer.From = ORDBData.GetScalar(sqlStringEmailFrom)
        Mailer.Cc = ""
        Mailer.Bcc = "webdev@sdi.com"  '  ;Tony.Smith@sdi.com"

        'strbodyhead = "<table width='100%'><tbody><tr><td><img src='http://www.sdizeus.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Arial; font-size: x-large; text-align: center;'>SDI Marketplace</span></center></td></tr></tbody></table>" & vbCrLf
        If strHldStatus = "B" Then
            ' strbodyhead = strbodyhead & "<center><span >SDiExchange - Request for Budget Approval</span></center>"

            strbodyhead = "<table bgcolor='black' width='100%'><tbody><tr><td><img src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' style=padding: 10px 0;' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large;color:white; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center;color:white; margin: 0px auto;'>SDI ZEUS - Request for Budget Approval</span></center></td></tr></tbody></table>" & vbCrLf
            strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        Else
            'strbodyhead = strbodyhead & "<center><span >SDiExchange - Request for Approval</span></center>"

            strbodyhead = "<table bgcolor='black' width='100%'><tbody><tr><td><img src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large;color:white; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center;color:white; margin: 0px auto;'>SDI ZEUS - Request for Approval</span></center></td></tr></tbody></table>" & vbCrLf
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
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Item Number:</span><span>  </span>" & stritemid & "<br>"
        If Trim(strPriority) <> "" Then
            strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Priority:</span><span>  </span><span style='color:red;'> " & strPriority & " </span><br>"
        End If
        strbodydetl = strbodydetl & "&nbsp;</p>"
        strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The above referenced order has been "
        strbodydetl = strbodydetl & "requested by <b>" & strBuyerName & "</b> "
        If strHldStatus = "B" Then
            strbodydetl = strbodydetl & "and has exceeded the charge code budget limit.  Click the link below or select the ""Approve Budget"" "
        Else
            strbodydetl = strbodydetl & "and needs your approval.  Click the link below or select the ""Approve Orders"" "
        End If
        strbodydetl = strbodydetl & "menu option in SDI ZEUS to approve or reject the order.<br>"
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
        strbodydetl = strbodydetl & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />" & vbCrLf

        Mailer.Body = strbodyhead & strbodydetl
        If Not getDBName() Then
            Mailer.To = "webdev@sdi.com"
        Else
            Mailer.To = strappEmail
        End If
        Dim Notify_Key As String = String.Empty
        If strHldStatus = "B" Then
            Mailer.Subject = "SDI ZEUS - Order Number " & strreqID & " needs budget approval"
            Notify_Key = "Order Number " & strreqID & " needs budget approval"
        Else
            Mailer.Subject = "SDI ZEUS - Order Number " & strreqID & " needs approval"
            Notify_Key = "Order Number " & strreqID & " needs approval"
        End If

        If Trim(strPriority) <> "" Then
            Mailer.Subject = Mailer.Subject & " " & strPriority
        End If
        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
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

    Public Shared Function GetWebAppName1() As String
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

    Private Shared Function buildCartforemail(ByVal dstcart1 As DataTable, ByVal ordNumber As String, ByVal EmployeeId As String) As DataTable


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
        'dstcart.Columns.Add("Bin Location")
        'dstcart.Columns.Add("Item Chg Code")
        'dstcart.Columns.Add("Requestor Name")
        'dstcart.Columns.Add("RFQ")
        'dstcart.Columns.Add("Machine Num")
        'dstcart.Columns.Add("Tax Exempt")
        'dstcart.Columns.Add("LPP")
        'dstcart.Columns.Add("PO")
        dstcart.Columns.Add("LN")
        'dstcart.Columns.Add("SerialID")

        Dim strOraSelectQuery As String = String.Empty
        Dim ordIdentifier As String = String.Empty
        Dim ordBU As String = String.Empty
        Dim OrcRdr As OleDb.OleDbDataReader = Nothing
        Dim dsOrdLnItems As DataSet = New DataSet
        Dim strSqlSelectQuery As String = String.Empty
        Dim unilogRdr As OleDb.OleDbDataReader = Nothing
        Dim SqlRdr As SqlDataReader = Nothing
        Dim strProdVwId As String = String.Empty

        Try
            strOraSelectQuery = "select * from SYSADM8.PS_ISA_ORD_INTF_HD where ORDER_NO = '" & ordNumber & "'"
            'strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_H where ORDER_NO = 'M220016571'"
            OrcRdr = ORDBData.GetReader(strOraSelectQuery)
            If OrcRdr.HasRows Then
                OrcRdr.Read()
                ordIdentifier = CType(OrcRdr("ORDER_NO"), String).Trim()
                ordBU = CType(OrcRdr("BUSINESS_UNIT_OM"), String).Trim()
            End If
            OrcRdr.Close()

            strOraSelectQuery = "select * from SYSADM8.PS_ISA_ORD_INTF_LN where ORDER_NO = '" & ordIdentifier & "'"
            dsOrdLnItems = ORDBData.GetAdapter(strOraSelectQuery)

            If dsOrdLnItems.Tables(0).Rows.Count > 0 Then
                For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows
                    dr = dstcart.NewRow()
                    dr("Item Number") = ""
                    'CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim()
                    dr("Description") = CType(dataRowMain("DESCR254"), String).Trim()

                    'If CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim().Contains("NONCAT") Then

                    Try
                        dr("Manuf.") = If(CType(dataRowMain("MFG_ID"), String).Trim() <> "", CType(dataRowMain("MFG_ID"), String).Trim(), CType(dataRowMain("ISA_MFG_FREEFORM"), String).Trim())
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
                    'Else
                    '    Dim invItemId As String = String.Empty
                    '    invItemId = CType(dataRowMain("INV_ITEM_ID"), String).Trim()
                    '    'If invItemId.Length > 0 Then
                    '    '    invItemId = invItemId.Substring(3)
                    '    'End If

                    '    strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_L where INV_ITEM_ID = '" & invItemId & "'"
                    '    OrcRdr = GetReader(strOraSelectQuery)

                    '    If OrcRdr.HasRows Then
                    '        OrcRdr.Read()
                    '        Try
                    '            dr("Manuf.") = CType(OrcRdr("ISA_MFG_FREEFORM"), String).Trim()
                    '        Catch ex As Exception
                    '            dr("Manuf.") = " "
                    '        End Try
                    '        Try
                    '            dr("Manuf. Partnum") = CType(OrcRdr("MFG_ITM_ID"), String).Trim()
                    '        Catch ex As Exception
                    '            dr("Manuf. Partnum") = " "
                    '        End Try
                    '        Try
                    '            dr("Item ID") = CType(OrcRdr("VNDR_CATALOG_ID"), String).Trim()
                    '        Catch ex As Exception
                    '            dr("Item ID") = " "
                    '        End Try


                    '        Try
                    '            dr("UOM") = CType(OrcRdr("UNIT_OF_MEASURE"), String).Trim()
                    '        Catch ex As Exception
                    '            dr("UOM") = "EA"
                    '        End Try
                    '    End If

                    'End If


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


                    'WAL-1044 making price 0 for third party[change by Vishalini]
                    If ordBU = "I0W01" Then
                        Try
                            Dim SqlStringThirdPartCompid As String = "select THIRDPARTY_COMP_ID from sdix_users_tbl where THIRDPARTY_COMP_ID <> '0' and THIRDPARTY_COMP_ID is not null and ISA_EMPLOYEE_ID = '" & EmployeeId & "'"
                            Dim THIRDPARTY_COMP_ID As String = ORDBData.GetScalar(SqlStringThirdPartCompid)
                            If THIRDPARTY_COMP_ID <> "" Then
                                dr("Item USD Price") = "0"
                                dr("Ext. USD Price") = "0"
                            End If
                        Catch ex As Exception

                        End Try

                    End If
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

    Public Shared Sub setServer()
        'VR 12/28/2014 get values from web.config first
        Dim sIOLServer As String = ""
        Try
            sIOLServer = ConfigurationSettings.AppSettings("WebAppName")
            If sIOLServer Is Nothing Then
                sIOLServer = ""
            End If
        Catch ex As Exception
            sIOLServer = ""
        End Try
        Dim sCplusServer As String = ""
        Try
            sCplusServer = ConfigurationSettings.AppSettings("cplusServerforDefault")
            If sCplusServer Is Nothing Then
                sCplusServer = ""
            End If
        Catch ex As Exception
            sCplusServer = ""
        End Try
        'Gives us a reference to the current asp.net 
        'application executing the method.

        'Dim sAppPath As String = currentApp.Request.ApplicationPath
        'If Trim(sIOLServer) <> "" And (sCplusServer <> "") Then
        '    currentApp.Session("IOLServer") = sIOLServer
        '    currentApp.Session("cplusServer") = sCplusServer
        '    Exit Sub
        'End If

        'If currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "LOCALHOST" Then
        '    currentApp.Session("cplusServer") = "http://199.117.166.50:8080/"
        '    currentApp.Session("IOLServer") = "http://localhost/insiteonline/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST" Then
        '    currentApp.Session("cplusServer") = "http://cptest:8085/"
        '    currentApp.Session("IOLServer") = "http://cptest/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "DEXTEST4" Then
        '    currentApp.Session("cplusServer") = "http://contentplus.isacs.com:8080/"
        '    currentApp.Session("IOLServer") = "http://dextest4/insiteonline/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST.SDI.COM" Then
        '    currentApp.Session("cplusServer") = "http://199.117.166.50:8080/"
        '    currentApp.Session("IOLServer") = "http://cptest.sdi.com/insiteonline/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST.ISACS.COM" Then
        '    currentApp.Session("cplusServer") = "http://cptest.sdi.com:8085/"
        '    currentApp.Session("IOLServer") = "http://cptest.isacs.com/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "10.2.2.20" Then
        '    currentApp.Session("cplusServer") = "http://contentplus.isacs.com:8080/"
        '    currentApp.Session("IOLServer") = "http://10.2.2.20/ISOL/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "69.55.5.220" Then
        '    currentApp.Session("cplusServer") = "http://contentplus.isacs.com:8080/"
        '    currentApp.Session("IOLServer") = "http://69.55.5.220/ISOL/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPREPLACE_COPY" Then
        '    currentApp.Session("cplusServer") = "http://199.117.166.50:8080/"
        '    currentApp.Session("IOLServer") = "http://CPREPLACE_COPY/ISOL/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "SDIEXCHTEST.SDI.COM" Then
        '    currentApp.Session("cplusServer") = "http://199.117.166.50:8080/"
        '    currentApp.Session("IOLServer") = "https://sdiexchtest.sdi.com/"
        'ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "192.168.253.61" Then
        '    currentApp.Session("cplusServer") = "http://199.117.166.50:8080/"
        '    'currentApp.Session("IOLServer") = "http://" & currentApp.Request.ServerVariables("HTTP_HOST") & "/" & Convert.ToString(ConfigurationSettings.AppSettings("WebAppName")) & "/"
        '    currentApp.Session("IOLServer") = "http://" & currentApp.Request.ServerVariables("HTTP_HOST") & GetWebAppName1() & "/"
        '    ''TODO: to remove this session variable
        '    'currentApp.Session("ISOLServer") = "http://192.168.253.61/ISOL/"
        'Else
        '    currentApp.Session("cplusServer") = "http://contentplus.isacs.com:8080/"
        '    If sAppPath = "/" Then
        '        currentApp.Session("IOLServer") = "http://" & currentApp.Request.ServerVariables("HTTP_HOST") & currentApp.Request.ApplicationPath   '  & "/"
        '    Else
        '        currentApp.Session("IOLServer") = "http://" & currentApp.Request.ServerVariables("HTTP_HOST") & currentApp.Request.ApplicationPath & "/"
        '    End If

        'End If
    End Sub

    Public Shared Function EncryptQueryString(ByVal queryString As NameValueCollection) As String
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

    Private Const QUERY_STRING_DELIMITER As Char = "&"
    Private Shared _cryptoProvider As RijndaelManaged
    '128 bit encyption: DO NOT CHANGE!!!!    
    Private Shared ReadOnly Key As Byte() = {18, 19, 6, 24, 37, 22, 4, 22, 17, 7, 11, 9, 13, 12, 6, 23}
    Private Shared ReadOnly IV As Byte() = {14, 2, 15, 7, 5, 9, 12, 8, 4, 47, 16, 12, 1, 32, 29, 18}

    Public Shared Function EncryptQueryString(ByVal queryString As String) As String
        Return "?" + HttpUtility.UrlEncode(Encrypt(queryString))

    End Function

    Public Shared Function Encrypt(ByVal unencryptedString As String) As String
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

    Private Shared Function RetrieveLineItems(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, oApprovalDetails As ApprovalDetails, ByRef dsLineItems As DataSet) As Boolean

        Const cCaller As String = "OrderApprovals.RetrieveLineItems"

        Dim bSuccess As Boolean = False
        Dim strSQLstring As String = ""

        Try
            'strSQLstring = "SELECT L.line_nbr, L.qty_req, L.net_unit_price, L.inv_item_id, L.isa_cust_charge_cd, C.inv_stock_type " & vbCrLf & _
            '    " FROM SYSADM8.PS_ISA_ORD_INTF_HD  H, SYSADM8.PS_ISA_ORD_INTF_LN  L, ps_inv_items C " & vbCrLf & _
            '    " WHERE H.isa_identifier = L.isa_parent_ident " & vbCrLf & _
            '    "       AND H.order_no = '" & oApprovalDetails.ReqID & "' " & vbCrLf & _
            '    "       AND H.business_unit_om = '" & oApprovalDetails.BU & "' " & vbCrLf & _
            '    "       AND C.setid(+) = L.itm_setid " & vbCrLf & _
            '    "       AND C.inv_item_id(+) = L.inv_item_id " & vbCrLf & _
            '    "       AND L.inv_item_id != ' ' " & vbCrLf & _
            '    "       AND C.EFFDT = " & vbCrLf & _
            '    "		        (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf & _
            '    "               WHERE C.SETID = A_ED.SETID" & vbCrLf & _
            '    "               AND C.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
            '    "               AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
            '    " UNION " & vbCrLf & _
            '    " SELECT L.line_nbr, L.qty_req, L.net_unit_price, L.inv_item_id, L.isa_cust_charge_cd, 'NSTK' AS inv_stock_type " & vbCrLf & _
            '    " FROM ps_isa_ord_intfc_H H, ps_isa_ord_intfc_L L " & vbCrLf & _
            '    " WHERE H.isa_identifier = L.isa_parent_ident " & vbCrLf & _
            '    "       AND H.order_no = '" & oApprovalDetails.ReqID & "' " & vbCrLf & _
            '    "       AND H.business_unit_om = '" & oApprovalDetails.BU & "' " & vbCrLf & _
            '    "       AND L.inv_item_id = ' ' "

            strSQLstring = "SELECT L.ISA_INTFC_LN, L.QTY_REQUESTED, L.ISA_SELL_PRICE, L.INV_ITEM_ID, L.ISA_CUST_CHARGE_CD,C.inv_stock_type " & vbCrLf &
               " FROM SYSADM8.PS_ISA_ORD_INTF_LN  L, ps_inv_items C " & vbCrLf &
               " WHERE " &
               "       L.order_no = '" & oApprovalDetails.ReqID & "' " & vbCrLf &
               "       AND L.business_unit_om = '" & oApprovalDetails.BU & "' " & vbCrLf &
               "       AND C.setid(+) = L.ITM_SETID " & vbCrLf &
               "       AND C.inv_item_id(+) = L.inv_item_id " & vbCrLf &
               "       AND L.inv_item_id != ' ' " & vbCrLf &
               "       AND C.EFFDT = " & vbCrLf &
               "		        (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf &
               "               WHERE C.SETID = A_ED.SETID" & vbCrLf &
               "               AND C.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf &
               "               AND A_ED.EFFDT <= SYSDATE)" & vbCrLf &
               " UNION " & vbCrLf &
               " SELECT L.ISA_INTFC_LN, L.QTY_REQUESTED, L.ISA_SELL_PRICE, L.INV_ITEM_ID, L.ISA_CUST_CHARGE_CD , 'NSTK' AS inv_stock_type" & vbCrLf &
               " FROM  SYSADM8.PS_ISA_ORD_INTF_LN L WHERE " & vbCrLf &
               "       L.order_no = '" & oApprovalDetails.ReqID & "' " & vbCrLf &
               "       AND L.business_unit_om = '" & oApprovalDetails.BU & "' " & vbCrLf &
               "       AND L.inv_item_id = ' ' "


            dsLineItems = ORDBData.GetAdapter(strSQLstring, False)
            'If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
            bSuccess = True

        Catch ex As Exception
            bSuccess = False
            HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
        End Try

        Return bSuccess
    End Function

    Private Shared Sub HandleApprovalError(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
                                       strSQLstring As String, ex As Exception, oApprovalDetails As ApprovalDetails, sCaller As String)
        Dim sErrorDetails As String
        sErrorDetails = "Caller=" & sCaller & vbCrLf &
            " SQL=" & strSQLstring & vbCrLf &
            " ex.Message=" & ex.Message & vbCrLf &
            " ex.StackTrace=" & ex.StackTrace

        RollBackTransfer(trnsactSession, connection, sErrorDetails, oApprovalDetails)
    End Sub

    Private Shared Sub HandleApprovalError(trnsactSession As OleDbTransaction, connection As OleDbConnection,
                                         ex As Exception, oApprovalDetails As ApprovalDetails)
        Dim sErrorDetails As String
        sErrorDetails = "ex.Message=" & ex.Message & " ex.StackTrace=" & ex.StackTrace

        RollBackTransfer(trnsactSession, connection, sErrorDetails, oApprovalDetails)
    End Sub

    Private Shared Sub HandleApprovalError(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
                                          sMethodName As String, rowsaffected As Integer, strSQLstring As String,
                                          oApprovalDetails As ApprovalDetails)
        Dim sErrorDetails As String
        sErrorDetails = sMethodName & ": rowsaffected=" & rowsaffected.ToString & " strSQLstring=" & strSQLstring

        RollBackTransfer(trnsactSession, connection, sErrorDetails, oApprovalDetails)

    End Sub

    Private Shared Sub HandleApprovalError(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
                                          sMethodName As String, oApprovalDetails As ApprovalDetails, sErrorDetails As String)
        sErrorDetails = sMethodName & ": " & sErrorDetails

        RollBackTransfer(trnsactSession, connection, sErrorDetails, oApprovalDetails)
    End Sub

    Private Shared Sub RollBackTransfer(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
                                       sErrorDetails As String, oApprovalDetails As ApprovalDetails)
        Try
            trnsactSession.Rollback()
            connection.Close()

            Const cSubject As String = "Approval Error"

            Dim sErrorMessage As String
            sErrorMessage = sErrorDetails & vbCrLf &
                " ApproverID: " & oApprovalDetails.ApproverID & vbCrLf &
                " BU: " & oApprovalDetails.BU & vbCrLf &
                " ReqID: " & oApprovalDetails.ReqID & vbCrLf &
                " Database: " & DbUrl.Substring(DbUrl.Length - 4).ToUpper

            Try
                SendSDiExchErrorMail(sErrorMessage, cSubject)
            Catch exErr As Exception
                Dim strErr As String = exErr.Message
            End Try

        Catch ex1 As Exception
        End Try

        trnsactSession = Nothing
        connection = Nothing
    End Sub

    Private Shared Function RetrieveNetOrderPrice(sBU As String, sOrdNo As String, ByRef dblNetOrderPrice As Double,
                                     Optional sChgCd As String = "|~|") As Boolean
        Dim bSuccess As Boolean = False
        Dim strSQLstring As String

        Try
            strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) AS NET_ORDR_PRICE_VAR " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf &
                " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf &
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

    Public Shared Function CheckLimits(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
       oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults, ByVal LineStatus As String) As Boolean

        Dim bSuccess As Boolean = False

        If CheckLimitsOrder(trnsactSession, connection, oApprovalDetails, oApprovalResults, IsAEES(oApprovalDetails.BU), LineStatus) Then

            bSuccess = True
        Else
            bSuccess = False
        End If

        Return bSuccess
    End Function

    Private Shared Function CheckLimitsOrder(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
        ByRef oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults, Optional ByVal bIsMultiCurrency As Boolean = False, Optional ByVal LineStatus As String = "") As Boolean

        Dim bSuccessful As Boolean = False

        Try
            oApprovalResults = New ApprovalResults()

            If bIsMultiCurrency Then
                Const cPriorDaysAgo As Integer = 0 ' prior days (in number) ago
                bSuccessful = MultiCurrencyOrder.CustEmp(trnsactSession, connection, oApprovalDetails,
                    sdiMultiCurrency.getSiteCurrency(oApprovalDetails.BU).Id,
                    cPriorDaysAgo, oApprovalResults, LineStatus)
            Else
                bSuccessful = SingleCurrencyOrder.CustEmp(trnsactSession, connection, oApprovalDetails, oApprovalResults)
            End If

        Catch ex As Exception
            bSuccessful = False
            HandleApprovalError(trnsactSession, connection, ex, oApprovalDetails)

        End Try

        CheckLimitsOrder = bSuccessful

    End Function

    Private Class MultiCurrencyOrder

        Public Shared Function CustEmp(trnsactSession As OleDbTransaction, connection As OleDbConnection,
           oApprovalDetails As ApprovalDetails, sBaseCurrCd As String, iExDaysAgo As Integer,
           oApprovalResults As ApprovalResults, ByVal LineStatus As String) As Boolean

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
                                        If Not UpdateHeader_ApproveOrder(trnsactSession, connection, oApprovalDetails, oApprovalResults, LineStatus) Then
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

        Private Shared Function UpdateHeader_ApproveOrder(trnsactSession As OleDbTransaction, connection As OleDbConnection,
          ByVal oApprovalDetails As ApprovalDetails, ByRef oApprovalResults As ApprovalResults, ByVal Linestatus As String) As Boolean

            Const cNewHeaderStatus As String = "QTW"
            Const cCaller As String = "MultiCurrencyOrder.UpdateHeader_ApproveOrder"
            Const cNewLineStatus As String = "QTW"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String
            Dim rowsaffected As Integer

            Try
                oApprovalResults.NewOrderHeaderStatus = cNewHeaderStatus

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD " & vbCrLf &
                    " SET " & vbCrLf &
                    " LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf &
                    ", ORDER_STATUS = '" & cNewHeaderStatus & "' " & vbCrLf &
                    " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf &
                    " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        Dim sErrorDetails As String = ""
                        If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName,
                                "Approve order", "SYSADM8.PS_ISA_ORD_INTF_HD", oApprovalDetails.ApproverID, oApprovalDetails.BU,
                                oApprovalDetails.ReqID, sColumnChg:="ORDER_STATUS", sNewValue:=cNewHeaderStatus, sErrorDetails:=sErrorDetails) Then

                            bSuccess = True
                        Else
                            HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        End If
                    End If
                End If

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN " & vbCrLf &
                    " SET " & vbCrLf &
                    " OPRID_APPROVED_BY = '" & oApprovalDetails.ApproverID & "' " & vbCrLf &
                    ", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf &
                    ", ISA_LINE_STATUS = '" & Linestatus & "' " & vbCrLf &
                    " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf &
                    " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        Dim sErrorDetails As String = ""
                        If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName,
                                "Approve order", "SYSADM8.PS_ISA_ORD_INTF_LN", oApprovalDetails.ApproverID, oApprovalDetails.BU,
                                oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sNewValue:=cNewLineStatus, sErrorDetails:=sErrorDetails) Then

                            bSuccess = True
                        Else
                            HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        End If
                    End If
                End If

            Catch ex As Exception
                bSuccess = False
                HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_NSTK_O(ByRef ds As DataSet, sBU As String, sOrdNo As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT  R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf &
                " FROM  SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf &
                " WHERE  A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND  A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf &
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                " AND  NOT EXISTS (SELECT 'X' " & vbCrLf &
                "   FROM PS_INV_ITEMS C " & vbCrLf &
                "   WHERE C.EFFDT = (SELECT MAX(C_ED.EFFDT) " & vbCrLf &
                "       FROM PS_INV_ITEMS C_ED " & vbCrLf &
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf &
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf &
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf &
                "   AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf &
                "   AND C.INV_STOCK_TYPE = 'STK') " & vbCrLf &
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf &
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf &
                " GROUP BY R.CURRENCY_CD"
            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_NSTK_D(ByRef ds As DataSet, sBU As String, sOrdNo As String, sEnteredBy As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND " & vbCrLf &
                " ( " & vbCrLf &
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredBy & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf &
                "     AND TRUNC(A.ADD_DTTM) = TRUNC(SYSDATE) " & vbCrLf &
                "   ) " & vbCrLf &
                "   OR " & vbCrLf &
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' ) " & vbCrLf &
                " ) " & vbCrLf &
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                " AND  NOT EXISTS (SELECT 'X' " & vbCrLf &
                "   FROM  PS_INV_ITEMS C " & vbCrLf &
                "   WHERE  C.EFFDT = (SELECT MAX(C_ED.EFFDT) " & vbCrLf &
                "       FROM PS_INV_ITEMS C_ED " & vbCrLf &
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf &
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf &
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf &
                "   AND C.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf &
                "   AND C.INV_STOCK_TYPE = 'STK') " & vbCrLf &
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf &
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf &
                " GROUP BY R.CURRENCY_CD "
            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_NSTK_M(ByRef ds As DataSet, sBU As String, sOrdNo As String, sEnteredBy As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND " & vbCrLf &
                " ( " & vbCrLf &
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredBy & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf &
                "     AND TO_CHAR(A.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf &
                "   ) " & vbCrLf &
                "   OR " & vbCrLf &
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf &
                "   ) " & vbCrLf &
                " ) " & vbCrLf &
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                " AND  NOT EXISTS (SELECT 'X' " & vbCrLf &
                "   FROM  PS_INV_ITEMS C " & vbCrLf &
                "   WHERE  C.EFFDT = (SELECT MAX(C_ED.EFFDT) " & vbCrLf &
                "       FROM PS_INV_ITEMS C_ED " & vbCrLf &
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf &
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf &
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf &
                "   AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf &
                "   AND C.INV_STOCK_TYPE = 'STK') " & vbCrLf &
                " AND  A.ORDER_NO=R.REQ_ID " & vbCrLf &
                " AND  B.ISA_INTFC_LN=R.LINE_NBR " & vbCrLf &
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

        Private Shared Function GetReqLineCount(trnsactSession As OleDbTransaction, connection As OleDbConnection,
                                               oApprovalDetails As ApprovalDetails, ByRef iReqLnCount As Integer) As Boolean

            Const cCaller As String = "MultiCurrencyOrder.GetReqLineCount"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            Try
                iReqLnCount = 0

                strSQLstring = "SELECT  COUNT(1) " & vbCrLf &
                    " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf &
                    " WHERE  A.BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf &
                    " AND  A.ORDER_NO = '" & oApprovalDetails.ReqID & "' " & vbCrLf &
                    " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                    " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                    " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf &
                    " AND  B.ISA_INTFC_LN = R.LINE_NBR"

                Dim sReqLnCount As String
                sReqLnCount = ORDBData.GetScalar(strSQLstring)
                iReqLnCount = CType(sReqLnCount, Integer)
                bSuccess = True

            Catch ex As Exception
                bSuccess = False
                HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess

        End Function

        Private Shared Function RetrieveTotalCost_NoReq_O(sBU As String, sOrdNo As String) As Double
            Dim dblTotalCost As Double = 0
            Dim strSQLstring As String

            strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "'"

            Dim sTotalCost As String
            sTotalCost = ORDBData.GetScalar(strSQLstring)
            dblTotalCost = CType(sTotalCost, Double)

            Return dblTotalCost
        End Function

        Private Shared Function RetrieveTotalCost_NoReq_D(sBU As String, sEnteredByID As String, sOrdNo As String) As Double
            Dim dblTotalCost As Double = 0
            Dim strSQLstring As String

            strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND " & vbCrLf &
                " ( " & vbCrLf &
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf &
                "     AND TRUNC(A.ADD_DTTM) = TRUNC(SYSDATE) " & vbCrLf &
                "   ) " & vbCrLf &
                "   OR " & vbCrLf &
                "   (     A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf &
                "   ) " & vbCrLf &
                " ) " & vbCrLf &
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND  B.QTY_REQUESTED > 0 AND B.ISA_LINE_STATUS = '" & m_cDeletedLine & "'"

            Dim sTotalCost As String
            sTotalCost = ORDBData.GetScalar(strSQLstring)
            dblTotalCost = CType(sTotalCost, Double)

            Return dblTotalCost
        End Function

        Private Shared Function RetrieveTotalCost_NoReq_M(sBU As String, sEnteredByID As String) As Double
            Dim dblTotalCost As Double = 0
            Dim strSQLstring As String

            strSQLstring = "SELECT SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND " & vbCrLf &
                " ( " & vbCrLf &
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf &
                "     AND TO_CHAR(A.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf &
                "   ) " & vbCrLf &
                "   OR " & vbCrLf &
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = V_ORDNO " & vbCrLf &
                "   ) " & vbCrLf &
                " ) " & vbCrLf &
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND  B.QTY_REQUESTED > 0 AND B.ISA_LINE_STATUS = '" & m_cDeletedLine & "'"

            Dim sTotalCost As String
            sTotalCost = ORDBData.GetScalar(strSQLstring)
            dblTotalCost = CType(sTotalCost, Double)

            Return dblTotalCost
        End Function

        Private Shared Function RetrieveSumByCurr_STK_O(ByRef ds As DataSet, sBU As String, sOrdNo As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT  R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf &
                " FROM  SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf &
                " WHERE  A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND  A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf &
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf &
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf &
                " GROUP BY R.CURRENCY_CD"

            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_STK_D(ByRef ds As DataSet, sBU As String, sEnteredByID As String, sOrdNo As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND " & vbCrLf &
                " ( " & vbCrLf &
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf &
                "     AND TRUNC(B.ADD_DTTM) = TRUNC(SYSDATE) " & vbCrLf &
                "   ) " & vbCrLf &
                "   OR " & vbCrLf &
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf &
                "   ) " & vbCrLf &
                " ) " & vbCrLf &
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf &
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf &
                " GROUP BY R.CURRENCY_CD"

            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function RetrieveSumByCurr_STK_M(ByRef ds As DataSet, sBU As String, sEnteredByID As String, sOrdNo As String) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "SELECT R.CURRENCY_CD, SUM(B.ISA_SELL_PRICE * B.QTY_REQUESTED) SUBTOTAL " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD  A, SYSADM8.PS_ISA_ORD_INTF_LN  B, PS_REQ_LINE R " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "' " & vbCrLf &
                " AND " & vbCrLf &
                " ( " & vbCrLf &
                "   ( B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') " & vbCrLf &
                "     AND TO_CHAR(A.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf &
                "   ) " & vbCrLf &
                "   OR " & vbCrLf &
                "   ( A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "' " & vbCrLf &
                "   ) " & vbCrLf &
                " ) " & vbCrLf &
                " AND  A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND  B.QTY_REQUESTED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                " AND  A.ORDER_NO = R.REQ_ID " & vbCrLf &
                " AND  B.ISA_INTFC_LN = R.LINE_NBR " & vbCrLf &
                " GROUP BY R.CURRENCY_CD"

            ds = ORDBData.GetAdapter(strSQLstring)

            Return bSuccess
        End Function

        Private Shared Function RetrieveOrderLimAndNextApprv(BU As String, sEnteredBy As String, sApproverID As String, ByRef dblApprLimit As Double,
                                                    ByRef sNextApproverID As String, ByRef eErrorInApprovals As ApprovalResults.ApprovalError) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String = ""

            Try
                Dim sChainToCurrentApprover As String = GetCurrentApprovalChain(BU, sEnteredBy, sApproverID)

                strSQLstring = "SELECT A.ISA_IOL_APR_LIMIT, A.ISA_IOL_APR_ALT " & vbCrLf &
                " FROM SDIX_USERS_APPRV A " & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & sApproverID & "' " & vbCrLf &
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
                    strSQLstring = "SELECT A.isa_employee_ID AS Approver, A.isa_iol_apr_limit AS Limit, A.isa_iol_apr_alt AS NextApprover " & vbCrLf &
                        " , A.business_unit AS BU " & vbCrLf &
                        " FROM SDIX_USERS_APPRV A WHERE A.isa_employee_id = '" & sNextApprover & "' " & vbCrLf &
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

    Private Class SingleCurrencyOrder

        Public Shared Function CustEmp(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection,
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

        Private Shared Function RetrieveNetUnitPrice(bCalcNSTKOnly As Boolean, strOrdApprType As String,
                                                    sBU As String, sEnteredByID As String, sReqID As String,
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

        Private Shared Function RetrieveOrderLimAndNextApprv(BU As String, sEnteredBy As String, sApproverID As String, ByRef dblApprLimit As Double,
                                                    ByRef sNextApproverID As String, ByRef eErrorInApprovals As ApprovalResults.ApprovalError) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String = ""

            Try
                Dim sChainToCurrentApprover As String = GetCurrentApprovalChain(BU, sEnteredBy, sApproverID)

                strSQLstring = "SELECT A.ISA_IOL_APR_LIMIT, A.ISA_IOL_APR_ALT " & vbCrLf &
                " FROM SDIX_USERS_APPRV A " & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & sApproverID & "' " & vbCrLf &
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

        Private Shared Function UpdateHeader_ApproveOrder(trnsactSession As OleDbTransaction, connection As OleDbConnection,
          ByVal oApprovalDetails As ApprovalDetails, ByRef sNewStatus As String) As Boolean

            Const cNewHeaderStatus As String = "QTW"
            Const cCaller As String = "SingleCurrencyOrder.UpdateHeader_ApproveOrder"
            Const cNewLineStatus As String = "QTW"

            Dim bSuccess As Boolean = False
            Dim strSQLstring As String
            Dim rowsaffected As Integer

            Try
                sNewStatus = cNewHeaderStatus

                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD  " & vbCrLf &
                   " SET " & vbCrLf &
                   " ORDER_STATUS = '" & sNewStatus & "' " & vbCrLf &
                   ", LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf &
                   " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf &
                   " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        Dim sErrorDetails As String = ""
                        If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName,
                                "Approve budget", "SYSADM8.PS_ISA_ORD_INTF_HD", oApprovalDetails.ApproverID, oApprovalDetails.BU,
                                oApprovalDetails.ReqID, sColumnChg:="ORDER_STATUS", sNewValue:=sNewStatus, sErrorDetails:=sErrorDetails) Then

                            bSuccess = True
                        Else
                            HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        End If
                    End If
                End If

                'strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN  " & vbCrLf & _
                '" SET " & vbCrLf & _
                '" OPRID_APPROVED_BY = '" & oApprovalDetails.ApproverID & "' " & vbCrLf & _
                '", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                '", ISA_LINE_STATUS = '" & cNewLineStatus & "' " & vbCrLf & _
                '" WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf & _
                '" AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"
                strSQLstring = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN  " & vbCrLf &
                " SET " & vbCrLf &
                " OPRID_APPROVED_BY = '" & oApprovalDetails.ApproverID & "' " & vbCrLf &
                ", APPROVAL_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf &
                " WHERE BUSINESS_UNIT_OM = '" & oApprovalDetails.BU & "' " & vbCrLf &
                " AND ORDER_NO = '" & oApprovalDetails.ReqID & "'"

                If ExecuteSQL(trnsactSession, connection, strSQLstring, oApprovalDetails, rowsaffected, cCaller) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        HandleApprovalError(trnsactSession, connection, cCaller, rowsaffected, strSQLstring, oApprovalDetails)
                    Else
                        Dim sErrorDetails As String = ""
                        If clsSDIAudit.AddRecord(trnsactSession, connection, m_cClassFileName,
                                "Approve budget", "SYSADM8.PS_ISA_ORD_INTF_LN", oApprovalDetails.ApproverID, oApprovalDetails.BU,
                                oApprovalDetails.ReqID, sColumnChg:="ISA_LINE_STATUS", sNewValue:=cNewLineStatus, sErrorDetails:=sErrorDetails) Then

                            bSuccess = True
                        Else
                            HandleApprovalError(trnsactSession, connection, cCaller, oApprovalDetails, sErrorDetails)
                        End If
                    End If
                End If
            Catch ex As Exception
                bSuccess = False
                HandleApprovalError(trnsactSession, connection, strSQLstring, ex, oApprovalDetails, cCaller)
            End Try

            Return bSuccess
        End Function

        Private Shared Function RetrieveNetUnitPrice_NSTK_O(sBU As String, sOrdNo As String, sChgCd As String,
                                                  ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) AS dblNetUnitPrice " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf &
                " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf &
                " AND B.QTY_RECEIVED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf
            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If
            strSQLstring &= " AND NOT EXISTS (SELECT 'X' " & vbCrLf &
                "   FROM PS_INV_ITEMS C " & vbCrLf &
                "   WHERE C.EFFDT = " & vbCrLf &
                "       (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED " & vbCrLf &
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf &
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf &
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf &
                " AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf &
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

        Private Shared Function RetrieveNetUnitPrice_NSTK_D(sBU As String, sOrdNo As String, sEnteredByID As String, sChgCd As String,
                                                     ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf &
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
            strSQLstring &= "   AND A.ORDER_NO = '" & sOrdNo & "')) " & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO "
            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If
            strSQLstring &= " AND B.QTY_RECEIVED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                " AND NOT EXISTS (SELECT 'X' " & vbCrLf &
                "   FROM PS_INV_ITEMS C " & vbCrLf &
                "   WHERE C.EFFDT = " & vbCrLf &
                "       (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED " & vbCrLf &
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf &
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf &
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf &
                "   AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf &
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

        Private Shared Function RetrieveNetUnitPrice_NSTK_M(sBU As String, sOrdNo As String, sEnteredByID As String, sChgCd As String,
                                                     ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf &
                " AND " & vbCrLf &
                " ("

            If sChgCd = "|~|" Then
                strSQLstring &= "   (B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' '))" & vbCrLf &
                "   OR " & vbCrLf &
                "   (A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "')"
            Else
                strSQLstring &= "   A.ORDER_STATUS in ('O','P',' ')" & vbCrLf &
                "   OR " & vbCrLf &
                "   (A.ORDER_STATUS IN ('W','B') AND A.ORDER_NO = '" & sOrdNo & "')"
            End If

            strSQLstring &= vbCrLf &
                " ) " & vbCrLf &
                " AND TO_CHAR(B.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO "

            If sChgCd <> "|~|" Then
                strSQLstring &= " AND B.ISA_CUST_CHARGE_CD = '" & sChgCd & "'"
            End If

            strSQLstring &= " AND B.QTY_RECEIVED > 0 AND NOT B.ISA_LINE_STATUS = '" & m_cDeletedLine & "' " & vbCrLf &
                " AND NOT EXISTS (SELECT 'X' " & vbCrLf &
                "   FROM PS_INV_ITEMS C " & vbCrLf &
                "   WHERE C.EFFDT = " & vbCrLf &
                "       (SELECT MAX(C_ED.EFFDT) FROM PS_INV_ITEMS C_ED " & vbCrLf &
                "       WHERE C.SETID = C_ED.SETID " & vbCrLf &
                "       AND C.INV_ITEM_ID = C_ED.INV_ITEM_ID " & vbCrLf &
                "       AND C_ED.EFFDT <= SYSDATE) " & vbCrLf &
                " AND C.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf &
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

        Private Shared Function RetrieveNetUnitPrice_STK_O(sBU As String, sOrdNo As String, sChgCd As String,
                                                           ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf &
                " AND A.ORDER_NO = '" & sOrdNo & "'" & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf &
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

        Private Shared Function RetrieveNetUnitPrice_STK_D(sBU As String, sOrdNo As String, sEnteredByID As String, sChgCd As String,
                                                           ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf &
                " AND " & vbCrLf &
                "("
            If sChgCd = "|~|" Then
                strSQLstring &= "  (B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ') AND TO_CHAR(A.ADD_DTTM) = TO_CHAR(SYSDATE)) " & vbCrLf &
                "  OR " & vbCrLf &
                "  (A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "')"
            Else
                strSQLstring &= "  (A.ORDER_STATUS in ('O','P',' ') AND TO_CHAR(A.ADD_DTTM) = TO_CHAR(SYSDATE)) " & vbCrLf &
                "  OR " & vbCrLf &
                "  (A.ORDER_STATUS IN ('W','B') AND A.ORDER_NO = '" & sOrdNo & "')"

            End If
            strSQLstring &= ") " & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf &
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

        Private Shared Function RetrieveNetUnitPrice_STK_M(sBU As String, sOrdNo As String, sEnteredByID As String, sChgCd As String,
                                                           ByRef dblNetUnitPrice As Double) As Boolean
            Dim bSuccess As Boolean = False
            Dim strSQLstring As String

            strSQLstring = "Select SUM(B.ISA_SELL_PRICE * B.QTY_RECEIVED) " & vbCrLf &
                " FROM SYSADM8.PS_ISA_ORD_INTF_HD A, SYSADM8.PS_ISA_ORD_INTF_LN B " & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf &
                " AND " & vbCrLf &
                " ("
            If sChgCd = "|~|" Then
                strSQLstring &= "   (B.OPRID_ENTERED_BY = '" & sEnteredByID & "' AND A.ORDER_STATUS in ('O','P',' ')) " & vbCrLf &
                "   OR " & vbCrLf &
                "   (A.ORDER_STATUS IN ('W','B',' ') AND A.ORDER_NO = '" & sOrdNo & "') "
            Else
                strSQLstring &= "    A.ORDER_STATUS in ('O','P',' ') " & vbCrLf &
                "   OR " & vbCrLf &
                "   (A.ORDER_STATUS IN ('W','B') AND A.ORDER_NO = '" & sOrdNo & "') "
            End If
            strSQLstring &= " ) " & vbCrLf &
                " AND TO_CHAR(B.ADD_DTTM, 'MM') = TO_CHAR(SYSDATE, 'MM') " & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf &
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

        Private Shared Function GetCurrentApprovalChain(sBU As String, sEnteredBy As String, sCurrentApprover As String) As String
            Dim strSQLstring As String
            Dim bFinalApprover As Boolean = False
            Dim sNextApprover As String = sEnteredBy
            Dim sApproverList As String = m_cApproverDelimiter & sEnteredBy.Trim.ToUpper & m_cApproverDelimiter

            If sEnteredBy <> sCurrentApprover Then
                While Not bFinalApprover
                    strSQLstring = "SELECT A.isa_employee_ID AS Approver, A.isa_iol_apr_limit AS Limit, A.isa_iol_apr_alt AS NextApprover " & vbCrLf &
                        " , A.business_unit AS BU " & vbCrLf &
                        " FROM SDIX_USERS_APPRV A WHERE A.isa_employee_id = '" & sNextApprover & "' " & vbCrLf &
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
    'Madhu-INC0015106-Removed avacorp in Email flow

    Public Shared Sub SendSDiExchErrorMail(ByVal strErrorMessage As String, Optional ByVal sSubject As String = "")

        'Gives us a reference to the current asp.net 
        'application executing the method.

        Dim strComeFrom As String = "Unknown"
        Try
            strComeFrom = System.Configuration.ConfigurationSettings.AppSettings("serverId").Trim
        Catch ex As Exception
            strComeFrom = "Unknown"
        End Try

        Dim mail As New System.Net.Mail.MailMessage()

        mail.To.Add("webdev@sdi.com")

        mail.From = New System.Net.Mail.MailAddress("SDIExchADMIN@sdi.com", "SDiExchange Admin")

        ' adding server name to the error message
        mail.Body = "Error in SDI ZEUS  -  " & vbCrLf & strErrorMessage & ". Server Id: " & strComeFrom

        If sSubject.Trim.Length = 0 Then
            sSubject = "Error - SDI ZEUS"
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

    Public Shared Function ExecuteSQL(ByRef trnsactSession As OleDbTransaction, ByRef connection As OleDbConnection, strSQLstring As String,
                                    oApprovalDetails As ApprovalDetails, ByRef rowsaffected As Integer, sCaller As String) As Boolean
        Dim bSuccess As Boolean = False
        Dim exError As Exception

        If ExecuteNonQuery(trnsactSession, connection, strSQLstring, rowsaffected, exError) Then
            bSuccess = True
        Else
            bSuccess = False

            HandleApprovalError(trnsactSession, connection, strSQLstring, exError, oApprovalDetails, sCaller)
        End If

        Return bSuccess
    End Function

    Public Shared Function IsAEES(ByVal sBU As String) As Boolean
        Dim bIsAEES As Boolean = False

        Dim sSP1AccessString As String = "I0913~I0914~I0915~I0916~I0917~I0918~I0919~I0920~I0921~I0922~I0923~I0924~I0925~I0926~I0940~I0967~I0968~I0969~I0970~I0971~I0972~I0973~I0974"
        Try
            sSP1AccessString = UCase(ConfigurationSettings.AppSettings("AEESsitesList").ToString)
        Catch ex As Exception
            sSP1AccessString = "I0913~I0914~I0915~I0916~I0917~I0918~I0919~I0920~I0921~I0922~I0923~I0924~I0925~I0926~I0940~I0967~I0968~I0969~I0970~I0971~I0972~I0973~I0974"
        End Try
        Dim sAEESsites As String = sSP1AccessString  '  "I0913~I0914~I0915~I0916~I0917~I0918~I0919~I0920~I0921~I0922~I0923~I0924~I0925~I0926~I0940"
        Try
            bIsAEES = (sAEESsites.IndexOf(sBU.Trim.ToUpper) > -1)
        Catch ex As Exception
            bIsAEES = False
        End Try
        Return bIsAEES
    End Function

End Class

'sdiMultiCurrency.vb

Public Class sdiMultiCurrency

    ' prevent this class to get instantiated
    Private Sub New()

    End Sub
    Public Shared Function getConversionRate(ByVal sourceCurrencyCode As String,
                                             ByVal sourceAmount As Decimal,
                                             ByVal targetCurrencyCode As String) As sdiConversionRate
        Dim convRate As sdiConversionRate = New sdiConversionRate(sourceCurrencyCode, targetCurrencyCode)
        'convRate = New sdiConversionRate(sourceCurrencyCode, targetCurrencyCode)
        convRate.SourceAmount = sourceAmount
        convRate.IsKnownConversionRate = False

        If Not (sourceCurrencyCode Is Nothing) Then
            sourceCurrencyCode = sourceCurrencyCode.Trim.ToUpper
        End If
        If Not (targetCurrencyCode Is Nothing) Then
            targetCurrencyCode = targetCurrencyCode.Trim.ToUpper
        End If

        Dim dtTransaction As DateTime = Now

        Try
            Dim strSQLString As String = "SELECT " & vbCrLf &
                                    " CURRTO.RATE_MULT " & vbCrLf &
                                    ",CURRTO.RATE_DIV " & vbCrLf &
                                    "FROM SYSADM8.PS_RT_RATE_TBL CURRTO " & vbCrLf &
                                    "WHERE CURRTO.EFFDT = ( " & vbCrLf &
                                    "                      SELECT MAX(A1.EFFDT) " & vbCrLf &
                                    "                      FROM SYSADM8.PS_RT_RATE_TBL A1 " & vbCrLf &
                                    "                      WHERE A1.RT_RATE_INDEX = CURRTO.RT_RATE_INDEX " & vbCrLf &
                                    "                        AND A1.TERM = CURRTO.TERM " & vbCrLf &
                                    "                        AND A1.FROM_CUR = CURRTO.FROM_CUR " & vbCrLf &
                                    "                        AND A1.TO_CUR = CURRTO.TO_CUR " & vbCrLf &
                                    "                        AND A1.RT_TYPE = CURRTO.RT_TYPE " & vbCrLf &
                                    "                        AND A1.EFFDT <= TO_DATE('" & dtTransaction.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS') " & vbCrLf &
                                    "                     )" & vbCrLf &
                                    "  AND CURRTO.FROM_CUR = '" & sourceCurrencyCode & "' " & vbCrLf &
                                    "  AND CURRTO.TO_CUR = '" & targetCurrencyCode & "' " & vbCrLf &
                                    "  AND CURRTO.RT_RATE_INDEX = 'MODEL'" & vbCrLf &
                                    "  AND CURRTO.RT_TYPE = 'CRRNT' "

            'Code to get conversion details
            Dim dsCurrencyConvert As New DataSet
            dsCurrencyConvert = ORDBData.GetAdapterSpc(strSQLString)

            'Code to generate sdiConversionRate object
            convRate = GetSDIConversionRateObject(dsCurrencyConvert, convRate, sourceAmount)
        Catch ex As Exception

        End Try

        Return (convRate)
    End Function
    Public Shared Function getItemBaseCurrency(ByVal itemId As String) As sdiCurrency

        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        'Dim sBusUnit As String = currentApp.Session("BUSUNIT")
        'Dim strBu3 As String = ""
        'Try
        '    strBu3 = sBusUnit.Substring(2, 3)
        'Catch ex As Exception
        '    strBu3 = ""
        'End Try

        Dim curr As sdiCurrency = New sdiCurrency(id:=sdiCommon.DEFAULT_CURRENCY_CODE, desc:="")
        curr.IsKnownCurrency = False
        Dim strCurrencyCode As String
        Dim strSQLString As String

        Try
            If itemId.Substring(0, 3) = currentApp.Session("SITEPREFIX") Then
                itemId = itemId
            Else
                itemId = currentApp.Session("SITEPREFIX") & itemId
            End If
            strSQLString = String.Format("select CURRENCY_CD from SYSADM8.PS_ISA_SDIEX_PRCBU where INV_ITEM_ID = '{0}' AND BUSINESS_UNIT = '{1}'", itemId, Convert.ToString(currentApp.Session("BUSUNIT")))

            'If Trim(strBu3) <> "" Then
            '    strBu3 = Trim(strBu3)
            '    If Len(strBu3) = 3 Then
            '        strSQLString = "select CURRENCY_CD from SYSADM8.PS_ISA_SDIEX_PRICE where INV_ITEM_ID = '" & itemId & "' AND ISA_SITE_ID = '" & strBu3 & "'"
            '    End If
            'End If
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

    Public Shared Function GetSDIConversionRateObject(ByVal dsConvRateInfo As DataSet, ByVal objsdiConversionrate As sdiConversionRate, ByVal sourceAmt As Decimal) As sdiConversionRate

        Dim convRate As sdiConversionRate = Nothing
        convRate = New sdiConversionRate(objsdiConversionrate.SourceCurrencyCode, objsdiConversionrate.TargetCurrencyCode)
        convRate.SourceAmount = sourceAmt
        convRate.IsKnownConversionRate = False

        Try
            Dim sMultiplier As String = ""
            Dim sDivider As String = ""
            If Not dsConvRateInfo Is Nothing And dsConvRateInfo.Tables(0).Rows.Count > 0 Then
                sMultiplier = dsConvRateInfo.Tables(0).Rows(0)("RATE_MULT")
                sDivider = dsConvRateInfo.Tables(0).Rows(0)("RATE_DIV")

                If Not (sMultiplier Is Nothing) Then
                    If IsNumeric(sMultiplier) Then
                        convRate.Multiplier = CDec(sMultiplier)
                    End If
                End If

                If Not (sDivider Is Nothing) Then
                    If IsNumeric(sDivider) Then
                        convRate.Divider = CDec(sDivider)
                    End If
                End If
                convRate.IsKnownConversionRate = True
                convRate.RateDateTimeStamp = Now
                convRate.ConvertedAmount = CDec(((convRate.SourceAmount) * convRate.Multiplier) / convRate.Divider)
            End If
        Catch ex As Exception

        End Try
        Return (convRate)
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
            strSQLString = String.Format("SELECT A.CURRENCY_CD_BASE AS BASE_CURRENCY FROM PS_BUS_UNIT_TBL_OM A  " & vbCrLf &
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
            Dim strSQLString As String = "SELECT " & vbCrLf &
                                        " CD.CURRENCY_CD " & vbCrLf &
                                        ",CD.DESCRSHORT " & vbCrLf &
                                        ",CD.DESCR " & vbCrLf &
                                        ",CD.CUR_SYMBOL " & vbCrLf &
                                        ",CD.COUNTRY " & vbCrLf &
                                        "FROM SYSADM8.PS_CURRENCY_CD_TBL CD " & vbCrLf &
                                        "WHERE CD.EFF_STATUS = 'A' " & vbCrLf &
                                        "  AND CD.CURRENCY_CD = '" & currencyCode & "' " & vbCrLf &
                                        "  AND CD.EFFDT = ( " & vbCrLf &
                                        "                  SELECT MAX(A1.EFFDT) " & vbCrLf &
                                        "                  FROM SYSADM8.PS_CURRENCY_CD_TBL A1 " & vbCrLf &
                                        "                  WHERE A1.CURRENCY_CD = CD.CURRENCY_CD " & vbCrLf &
                                        "                    AND A1.EFF_STATUS = CD.EFF_STATUS " & vbCrLf &
                                        "                    AND A1.EFFDT <= SYSDATE " & vbCrLf &
                                        "                 ) " & vbCrLf &
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



Public Class sdiCurrency

    Public Sub New()

    End Sub

    Public Sub New(ByVal id As String, ByVal desc As String)
        m_Id = id
        m_desc = desc
    End Sub

    Private m_symbol As String = "$"

    Public Property [Symbol]() As String
        Get
            Return m_symbol
        End Get
        Set(ByVal value As String)
            m_symbol = value
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

    Private m_country As String = ""

    Public Property [Country]() As String
        Get
            Return m_country
        End Get
        Set(ByVal value As String)
            m_country = value
        End Set
    End Property

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

End Class

Public Class sdiCommon

    Public Const APP_CONNECTION_STRING_ORA As String = "oraCNstring"
    Public Const DEFAULT_CURRENCY_CODE As String = "USD"
    Public Const DEFAULT_CURRENCY_SYMBOL As String = "$"

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

    Public Property ErrorInApproval As ApprovalError
        Get
            Return m_eError
        End Get
        Set(value As ApprovalError)
            m_eError = value
        End Set
    End Property

    Public Property NextOrderApprover() As String
        Get
            Return m_oEmployee.NextApproverID
        End Get
        Set(value As String)
            m_oEmployee.NextApproverID = value
        End Set
    End Property

    Public Property OrderExceededLimit() As Boolean
        Get
            Return m_oEmployee.ExceededLimit
        End Get
        Set(value As Boolean)
            m_oEmployee.ExceededLimit = value
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

        Public Function ChargeCodesCount() As Integer
            Return m_arrChgCode.Count
        End Function

        Public Property ExceededLimit(iIndex As Integer) As Boolean
            Get
                Return m_bExceededLimit(iIndex)
            End Get
            Set(value As Boolean)
                m_bExceededLimit(iIndex) = value
            End Set
        End Property

        Public ReadOnly Property ChargeCode(iIndex As Integer) As String
            Get
                Return m_arrChgCode(iIndex)
            End Get
        End Property

        Public Property NewHeaderStatus(iIndex As Integer) As String
            Get
                Return m_sNewHeaderStatus(iIndex)
            End Get
            Set(value As String)
                m_sNewHeaderStatus(iIndex) = value
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

    End Class


    Public Function IsMoreApproversNeeded() As Boolean
        Dim bMoreApproversNeeded As Boolean = False

        If m_oEmployee.ExceededLimit Then
            bMoreApproversNeeded = True
        ElseIf m_oChargeCode.IsAnyExceededLimit Then
            bMoreApproversNeeded = True
        End If

        Return bMoreApproversNeeded
    End Function

    Public Function IsAnyChargeCodeExceededLimit() As Boolean
        Return m_oChargeCode.IsAnyExceededLimit
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

End Class

'ApprovalDetails.vb

Public Class ApprovalDetails

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


    Private m_oLineDetails As New ArrayList
    Public ReadOnly Property LineDetails As ArrayList
        Get
            Return m_oLineDetails
        End Get
    End Property

    Private m_bOROTreatedLikeSTK As String
    Public ReadOnly Property OROTreatedLikeSTK As Boolean
        Get
            Return m_bOROTreatedLikeSTK
        End Get
    End Property

    Private m_sSDIEmp As String
    Public ReadOnly Property SDIEmp As String
        Get
            Return m_sSDIEmp
        End Get
    End Property


    Private m_sNSTKOnlyFlag As String
    Public ReadOnly Property NSTKOnlyFlag As String
        Get
            Return m_sNSTKOnlyFlag
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

    Public Shared Function GetSiteBU(ByVal sBU As String, Optional ByVal strTaxCompany As String = "", Optional ByVal strSiteBu1 As String = "") As String

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim strSiteBU As String
        Dim dtrPrefixReader As OleDbDataReader = Nothing
        Dim strSQLstring As String
        Try
            If Trim(strSiteBu1) <> "" Then
                strSiteBU = Trim(strSiteBu1)
            Else
                strSQLstring = "SELECT A.BUSINESS_UNIT" & vbCrLf &
                    " FROM PS_REQ_LOADER_DFL A" & vbCrLf &
                    " WHERE SUBSTR(A.LOADER_BU,2) = '" & sBU.Substring(1, 4) & "'" & vbCrLf
                dtrPrefixReader = ORDBData.GetReader(strSQLstring)
                If dtrPrefixReader.Read() Then
                    strSiteBU = dtrPrefixReader("BUSINESS_UNIT").ToString
                Else
                    If strTaxCompany = "" Then
                        dtrPrefixReader.Close()
                        strSQLstring = "SELECT A.TAX_COMPANY" & vbCrLf &
                                    " FROM PS_BUS_UNIT_TBL_OM A" & vbCrLf &
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
        End Try

    End Function

    Public Shared Function GetSitePrefix(ByVal sBU As String) As String

        'Gives us a reference to the current asp.net 
        'application executing the method.

        Dim strSitePrefix As String = "XXX"

        'Try
        '    strSitePrefix = currentApp.Session("SITEPREFIX")
        'Catch ex As Exception
        '    strSitePrefix = "XXX"
        'End Try

        'Dim strSQLString = "SELECT ISA_SITE_CODE" & vbCrLf & _
        '    " FROM PS_BUS_UNIT_TBL_OM" & vbCrLf & _
        '    " WHERE BUSINESS_UNIT = '" & sBU & "'" & vbCrLf
        Dim strSQLString = "select ISA_CUST_PREFIX " & vbCrLf &
             " FROM sysadm8.PS_ISA_ENTERPRISE" & vbCrLf &
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

    Public Class OrderLineDetails

        Public Sub New(iLineNbr As Integer, sCurrLineStatus As String, decQtyReq As Decimal, decUnitPrice As Decimal,
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

        Public Sub New(iLineNbr As Integer, sStockType As String, decQtyReq As Decimal, sItemChgCd As String, decUnitPrice As Decimal,
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

    Public Sub AddLineDetailsForOrder(iLineNbr As Integer, decQtyReq As Decimal, sStockType As String, sItemChgCd As String,
                              decUnitPrice As Decimal, sInvItemID As String, bDeleteItem As Boolean)

        Dim oLineDetails As New OrderLineDetails(iLineNbr, sStockType, decQtyReq, sItemChgCd, decUnitPrice, sInvItemID, bDeleteItem)

        m_oLineDetails.Add(oLineDetails)
    End Sub

End Class

' clsUserTbl.vb

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

        strSQLstring = "SELECT A.ISA_USER_ID, A.FIRST_NAME_SRCH," & vbCrLf &
                        " A.LAST_NAME_SRCH," & vbCrLf &
                        " A.ISA_PASSWORD_ENCR," & vbCrLf &
                        " A.BUSINESS_UNIT," & vbCrLf &
                        " A.ISA_EMPLOYEE_NAME," & vbCrLf &
                        " A.ISA_SDI_EMPLOYEE," & vbCrLf &
                        " A.PHONE_NUM," & vbCrLf &
                        " A.ISA_EMPLOYEE_EMAIL," & vbCrLf &
                        " A.ISA_EMPLOYEE_ACTYP," & vbCrLf &
                        " B.ISA_IOL_APR_EMP_ID," & vbCrLf &
                        " B.ISA_IOL_APR_LIMIT," & vbCrLf &
                        " C.ISA_TRACK_USR_NAME," & vbCrLf &
                        " C.ISA_TRACK_USR_PSSW," & vbCrLf &
                        " C.ISA_TRACK_USR_GUID," & vbCrLf &
                        " C.ISA_TRACK_TO_DATE," & vbCrLf &
                        " A.ISA_CUST_SERV_FLG" & vbCrLf &
                        " FROM SDIX_USERS_TBL A," & vbCrLf &
                        " SDIX_USERS_APPRV B, SDIX_SDITRACK_USERS_TBL C " & vbCrLf &
                        " WHERE A.ISA_EMPLOYEE_ID = '" & Employee_ID & "'" & vbCrLf &
                        " AND A.ACTIVE_STATUS = 'A'" & vbCrLf
        If Not Business_unit = "" Then
            strSQLstring = strSQLstring & " AND A.BUSINESS_UNIT = '" & Business_unit & "'" & vbCrLf
        End If
        strSQLstring = strSQLstring &
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)" & vbCrLf &
                    " AND A.ISA_EMPLOYEE_ID = B.ISA_EMPLOYEE_ID(+)" & vbCrLf &
                    " AND A.BUSINESS_UNIT = C.BUSINESS_UNIT(+)" & vbCrLf &
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

'clsEnterprise.vb

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

    Private strIsaRecvPrice As String  '  ISA_RECV_PRICE
    Public ReadOnly Property ShowRecvPrice() As String
        Get
            Return strIsaRecvPrice
        End Get
    End Property

    Private strIsaSdiexchApprv As String  '  ISA_SDIEXCH_APPRVL
    Public ReadOnly Property IsaSdixchApprv() As String
        Get
            Return strIsaSdiexchApprv
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

        strSQLstring = strSQLstring & " FROM sysadm8.PS_ISA_ENTERPRISE A, sysadm8.PS_CUSTOMER B" & vbCrLf &
                " WHERE A.ISA_BUSINESS_UNIT = '" & BusinessUnit & "'" & vbCrLf &
                " AND A.SETID = B.SETID(+)" & vbCrLf &
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
                strIsaRecvPrice = " "
                strIsaRecvPrice = objReader.Item("ISA_RECV_PRICE")
                If strIsaRecvPrice Is Nothing Then
                    strIsaRecvPrice = " "
                Else
                    If Trim(strIsaRecvPrice) = "" Then
                        strIsaRecvPrice = " "
                    End If
                End If
            Catch ex As Exception
                strIsaRecvPrice = " "
            End Try
            If Trim(strIsaRecvPrice) = "" Then
                strIsaRecvPrice = " "
            End If

            Try
                strIsaSdiexchApprv = " "
                strIsaSdiexchApprv = objReader.Item("ISA_SDIEXCH_APPRVL")
                If strIsaSdiexchApprv Is Nothing Then
                    strIsaSdiexchApprv = " "
                Else
                    If Trim(strIsaSdiexchApprv) = "" Then
                        strIsaSdiexchApprv = " "
                    End If
                End If
            Catch ex As Exception
                strIsaSdiexchApprv = " "
            End Try
            If Trim(strIsaSdiexchApprv) = "" Then
                strIsaSdiexchApprv = " "
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


    Private Sub SetFlag_OROTreatedLikeSTK(sOROTreatedLikeSTK As String)
        If sOROTreatedLikeSTK.ToUpper = "Y" Then
            m_bOROTreatedLikeSTK = True
        Else
            m_bOROTreatedLikeSTK = False
        End If
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

End Class


'clsSDIAudit.vb

Public Class clsSDIAudit

    Private Shared m_bGotSchema As Boolean = False
    Private Shared m_dtSchema As DataTable

    Public Shared Function AddRecord(trnsactSession As OleDbTransaction, connection As OleDbConnection,
                                sSourceProgram As String, sFunctionDesc As String, sTableName As String,
                                sOprID As String, sBU As String, sKey01 As String, Optional sColumnChg As String = " ",
                                Optional sOldValue As String = " ", Optional sNewValue As String = " ",
                                Optional sKey02 As String = " ", Optional sKey03 As String = " ",
                                Optional sUDF1 As String = " ", Optional sUDF2 As String = " ",
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
            If GetInsertCommand(sSourceProgram, sFunctionDesc, sTableName, sOprID, sBU, sColumnChg, sOldValue, sNewValue,
                                sKey01, sKey02, sKey03, sUDF1, sUDF2, sUDF3, strSQLstring, sErrorDetails) Then
                Try
                    Dim cmd As OleDbCommand = New OleDbCommand(strSQLstring, connection)
                    cmd.CommandTimeout = 120
                    cmd.Transaction = trnsactSession
                    rowsaffected = cmd.ExecuteNonQuery()
                Catch ex As Exception
                    bError = True
                    sErrorDetails = GetExceptionDetails(ex, strSQLstring, sOprID, sBU)
                End Try

                If Not bError Then
                    If rowsaffected = 1 Then
                        bAddSuccess = True
                    Else
                        sErrorDetails = GetErrorDetails("InterUnitReceipts.aspx: AddRecord(OleDbTransaction...)", rowsaffected, strSQLstring, sOprID, sBU)
                    End If
                End If
            End If
        Catch ex As Exception
            sErrorDetails = GetExceptionDetails(ex, strSQLstring, sOprID, sBU)
        End Try

        Return bAddSuccess
    End Function

    Private Shared Function GetInsertCommand(sSourceProgram As String, sFunctionDesc As String, sTableName As String,
                                             sOprID As String, sBU As String, sColumnChg As String, sOldValue As String,
                                             sNewValue As String, sKey01 As String, sKey02 As String, sKey03 As String,
                                             sUDF1 As String, sUDF2 As String, sUDF3 As String, ByRef strSQLstring As String,
                                             ByRef sErrorDetails As String) As Boolean
        Dim bGotCommand As Boolean = False
        Dim sServer As String = ""
        m_bGotSchema = False

        Try
            ' We are going to truncate instead of return an error. We don't
            ' want to abort the primary function (interunit receipts, etc) 
            ' just for an audit record.


            sServer = "SDIMOBILE03"

            If TruncateData(sSourceProgram, sFunctionDesc, sTableName, sOprID, sBU, sColumnChg, sOldValue,
                                             sNewValue, sKey01, sKey02, sKey03, sUDF1, sUDF2, sUDF3, sServer, sErrorDetails) Then

                'Yury Ticket 117944 20170609
                'strSQLstring = "INSERT INTO SYSADM8.ps_isa_SDIXaudit " & vbCrLf & _
                strSQLstring = "INSERT INTO SDIX_audit " & vbCrLf &
                " ( " & vbCrLf &
                " descr, rcdsrc, table_name " & vbCrLf &
                ", key_01, key_02, key_03 " & vbCrLf &
                ", columnchg, newvalue, oldvalue " & vbCrLf &
                ", oprid, server_name " & vbCrLf &
                ", dt_timestamp " & vbCrLf &
                ", business_unit, isa_udf1, isa_udf2, isa_udf3 " & vbCrLf &
                " ) " & vbCrLf &
                " VALUES (" & vbCrLf &
                " '" & sFunctionDesc & "', '" & sSourceProgram & "', '" & sTableName & "' " & vbCrLf &
                ", '" & sKey01 & "', '" & sKey02 & "', '" & sKey03 & "' " & vbCrLf &
                ", '" & sColumnChg & "', '" & sNewValue & "', '" & sOldValue & "' " & vbCrLf &
                ", '" & sOprID & "', '" & sServer & "' " & vbCrLf &
                ", TO_DATE('" & Now.ToString("MM/dd/yyyy HH:mm:ss") & "', 'MM/DD/YYYY HH24:MI:SS') " & vbCrLf &
                ", '" & sBU & "', '" & sUDF1 & "', '" & sUDF2 & "', '" & sUDF3 & "' " & vbCrLf &
                " )"

                strSQLstring = strSQLstring.Replace(", ''", ", ' '") ' make sure nulls aren't written to table

                bGotCommand = True
            End If
        Catch ex As Exception
            bGotCommand = False
            sErrorDetails = GetExceptionDetails(ex, strSQLstring, sOprID, sBU)
        End Try

        Return bGotCommand
    End Function
    Private Shared Function TruncateData(ByRef sSourceProgram As String, ByRef sFunctionDesc As String, ByRef sTableName As String,
                                            ByRef sOprID As String, ByRef sBU As String, ByRef sColumnChg As String,
                                            ByRef sOldValue As String, ByRef sNewValue As String, ByRef sKey01 As String,
                                            ByRef sKey02 As String, ByRef sKey03 As String, ByRef sUDF1 As String,
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
            sErrorDetails = GetExceptionDetails(ex, "", sOprID, sBU)
        End Try

        Return bSuccess
    End Function

    Private Shared Sub TruncateField(ByRef sData As String, iMaxLength As Integer)
        Try
            If Trim(sData) <> "" Then
                sData = Trim(sData)
                If sData.Length > iMaxLength Then
                    sData = sData.Substring(0, iMaxLength)
                End If
                If Trim(sData) = "" Then
                    sData = " "
                End If
            Else
                sData = " "
            End If
        Catch ex As Exception
            sData = " "
        End Try

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

    Private Shared Function GetExceptionDetails(ex As Exception, Optional strSQLstring As String = "", Optional OprID As String = " ", Optional StrBU As String = " ") As String
        Dim sErrorDetails As String

        sErrorDetails = "ex.Message=" & ex.Message & vbCrLf &
            "; USERID=" & OprID & vbCrLf &
            "; BU=" & StrBU
        If strSQLstring.Trim.Length > 0 Then
            sErrorDetails &= "; strSQLstring=" & strSQLstring
        End If
        sErrorDetails &= "; ex.StackTrace=" & ex.StackTrace

        Return sErrorDetails
    End Function

    Private Shared Function GetErrorDetails(sFunction As String, rowsaffected As Integer, strSQLstring As String, ByVal OprID As String, ByVal StrBU As String) As String
        Dim sErrorDetails As String

        sErrorDetails = sFunction & vbCrLf &
            "; rowsaffected=" & rowsaffected.ToString & vbCrLf &
            "; USERID=" & OprID & vbCrLf &
            "; BU=" & StrBU & vbCrLf &
            "; strSQLstring=" & strSQLstring

        Return sErrorDetails
    End Function

End Class

'WebPSharedFunc.vb
Public Class WebPSharedFunc

    'Public Shared Sub sendErrorEmail(ByVal strMessage As String, ByVal strMobile As String, ByVal strURL As String, ByVal strSQL As String)

    '    'Gives us a reference to the current asp.net 
    '    'application executing the method.

    '    Dim Mailer As MailMessage = New MailMessage
    '    Dim strccfirst As String = System.Configuration.ConfigurationManager.AppSettings("MailToName") '  "michael.randall"
    '    Dim strccfirst2 As String = "3025591705"
    '    Dim strcclast As String = "sdi.com"
    '    Dim strcclast2 As String = "vtext.com"
    '    Mailer.From = "SDIExchADMIN@SDI.com"

    '    Dim strComeFrom As String = "Unknown"
    '    Try
    '        strComeFrom = System.Configuration.ConfigurationSettings.AppSettings("serverId").Trim
    '    Catch ex As Exception
    '        strComeFrom = "Unknown"
    '    End Try
    '    strSQL += ". Data Source = " & Right(DbUrl, 4) & vbCrLf & _
    '        "BU: " & currentApp.Session("BUSUNIT") & ". Server Id: " & strComeFrom
    '    Dim bSendToMeOnly As Boolean = False
    '    Dim strHasNothing As String = "Session('USERID')  Is Nothing" & vbCrLf & _
    '        "Session('SDIEMP')  Is Nothing."
    '    If strMessage.Contains(strHasNothing) Then
    '        bSendToMeOnly = True
    '    End If

    '    Dim strHasInvaldViewState As String = "Invalid viewstate."
    '    If strMessage.Contains(strHasInvaldViewState) Then
    '        bSendToMeOnly = True
    '    End If

    '    Dim strHasWebResource As String = "/WebResource.axd"
    '    If strURL.Contains(strHasWebResource) Then
    '        bSendToMeOnly = True
    '    End If

    '    Dim strCollectionWasModified As String = "Collection was modified; enumeration operation may not execute."
    '    If strMessage.Contains(strCollectionWasModified) Then
    '        bSendToMeOnly = True
    '    End If

    '    Dim strMailtoList1 As String = strccfirst & "@" & strcclast & ";" & System.Configuration.ConfigurationManager.AppSettings("MailToList")
    '    Try
    '        If LCase(strMessage).Contains("transaction or savepoint rollback required") Then
    '            ' add Brian Akom and Wenjia to a Mailer.to list
    '            strMailtoList1 = strMailtoList1 & ";Brian.Akom@sdi.com;Wenjia.Zhang@sdi.com"
    '        End If
    '    Catch ex As Exception

    '    End Try

    '    If bSendToMeOnly Then
    '        Mailer.To = "webdev@sdi.com"
    '    Else
    '        Mailer.To = strMailtoList1
    '    End If

    '    Mailer.Subject = strURL  '   & "&nbsp;" & currentApp.Request.ServerVariables("remote_addr")
    '    If strMobile = "YES" And _
    '        Not Trim(currentApp.Request.ServerVariables("remote_addr")) = "" And _
    '        Not currentApp.Request.ServerVariables("remote_addr") = "65.220.75.12" And _
    '        Not currentApp.Request.ServerVariables("remote_addr") = "127.0.0.1" And _
    '        Mailer.Cc = strccfirst2 & "@" & strcclast2 Then
    '    End If
    '    'Dim strSQLString As String

    '    Dim strbodydetl As String
    '    strbodydetl = strbodydetl & "<div>"
    '    strbodydetl = strbodydetl & "<p >" & strMessage & "&nbsp;" & currentApp.Request.ServerVariables("remote_addr") & _
    '                            "&nbsp;" & currentApp.Session("USERID") & "&nbsp;" & strSQL
    '    strbodydetl = strbodydetl & "&nbsp;<BR>"
    '    strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
    '    strbodydetl = strbodydetl & "&nbsp;<br>"
    '    strbodydetl = strbodydetl & "&nbsp;</p>"
    '    strbodydetl = strbodydetl & "</div>"

    '    Mailer.Body = strbodydetl

    '    Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

    '    Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
    '    Dim MailAttachmentName As String()
    '    Dim MailAttachmentbytes As New List(Of Byte())()

    '    Try
    '        SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From.ToString(), Mailer.To.ToString(), Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "SDIERR", MailAttachmentName, MailAttachmentbytes.ToArray())

    '    Catch ex As Exception
    '        Try
    '            SDIEmailService.EmailUtilityServices("Mail", Mailer.From.ToString(), Mailer.To.ToString(), Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "SDIERR", MailAttachmentName, MailAttachmentbytes.ToArray())

    '            Dim exceptionString As String
    '            Dim serverName As String = String.Empty
    '            If Not currentApp.Server.MachineName Is Nothing Then
    '                serverName = currentApp.Server.MachineName
    '            Else
    '                serverName = strComeFrom
    '            End If
    '            exceptionString = "Exception Message - " + ex.Message + "<br />" + "Exception Trace - " + ex.StackTrace + "<br />"
    '            SDIEmailService.EmailUtilityServices("Mail", Mailer.From.ToString(), Mailer.To.ToString(), "Error in Email Utility - " + serverName, String.Empty, String.Empty, exceptionString, "SDIERR", MailAttachmentName, MailAttachmentbytes.ToArray())
    '        Catch exs1 As Exception
    '        End Try
    '    End Try

    'End Sub

    'Madhu-INC0015106-Removed avacorp in Email flow

    Public Shared Sub SendSDiExchErrorMail(ByVal strErrorMessage As String, Optional ByVal sSubject As String = "")

        'Gives us a reference to the current asp.net 
        'application executing the method.

        Dim strComeFrom As String = "Unknown"
        Try
            strComeFrom = System.Configuration.ConfigurationSettings.AppSettings("serverId").Trim
        Catch ex As Exception
            strComeFrom = "Unknown"
        End Try

        Dim mail As New System.Net.Mail.MailMessage()

        mail.To.Add("webdev@sdi.com")

        mail.From = New System.Net.Mail.MailAddress("SDIExchADMIN@sdi.com", "SDiExchange Admin")

        ' adding server name to the error message
        mail.Body = "Error in SDI ZEUS  -  " & vbCrLf & strErrorMessage & ". Server Id: " & strComeFrom

        If sSubject.Trim.Length = 0 Then
            sSubject = "Error - SDI ZEUS"
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
Public Class sdiItemPrice

    Public Sub New()

    End Sub

    Public Sub New(ByVal unitPrice As Decimal,
                   ByVal priceCurrency As sdiCurrency)
        m_unitPrice = unitPrice
        m_currency = priceCurrency
    End Sub

    Private m_unitPrice As Decimal = 0
    Public Property [UnitPrice]() As Decimal
        Get
            Return m_unitPrice
        End Get
        Set(ByVal Value As Decimal)
            m_unitPrice = Value
        End Set
    End Property

    Private m_currency As sdiCurrency = Nothing
    Public Property [Currency]() As sdiCurrency
        Get
            Return m_currency
        End Get
        Set(ByVal Value As sdiCurrency)
            m_currency = Value
        End Set
    End Property

End Class


Public Class ApprovalHistory

    Public Enum ApprHistType As Integer
        QuoteApproval 'Q
        OrderApproval 'W
        BudgetaryApproval 'B
        DepartmentApproval 'D
    End Enum

    Public Enum ApprHistStatus As Integer
        Approve 'A
        Pending 'P
        Decline 'D
    End Enum


    Public Shared Function RecordApprovalType(trnsactSession As OleDbTransaction, connection As OleDbConnection,
                                              apprStatus As ApprHistStatus, apprType As ApprHistType,
                                              oApprovalDetails As ApprovalDetails, ByRef sErrorDetails As String) As Boolean

        Const cMethodName As String = "ApprovalHistory.RecordApprovalType"

        Dim bSuccess As Boolean = False
        Dim strSQLstring As String = ""
        Dim rowsaffected As Integer
        Dim exError As Exception

        Dim sApprovalTypeCode As String = GetApprovalTypeCode(apprType)
        Dim sApprovalStatus As String = GetApprovalStatus(apprStatus)

        Try
            Dim I As Integer = 0
            While I < oApprovalDetails.LineDetails.Count
                Dim oLineDetails As ApprovalDetails.OrderLineDetails
                oLineDetails = CType(oApprovalDetails.LineDetails(I), ApprovalDetails.OrderLineDetails)
                strSQLstring = GetApprHistInsertSQL(oApprovalDetails.BU, oApprovalDetails.ReqID, oApprovalDetails.EnteredByID,
                                                oApprovalDetails.ApproverID, sApprovalStatus, sApprovalTypeCode, oLineDetails.LineNbr)
                If ExecuteNonQuery(trnsactSession, connection, strSQLstring, rowsaffected, exError) Then
                    If rowsaffected = 0 Then
                        bSuccess = False
                        sErrorDetails = cMethodName & ": rowsaffected=" & rowsaffected.ToString & " strSQLstring=" & strSQLstring
                    Else
                        bSuccess = True
                    End If
                Else
                    bSuccess = False
                    sErrorDetails = cMethodName & ": strSQLstring=" & strSQLstring & " ex.Message=" & exError.Message & " ex.StackTrace=" & exError.StackTrace
                End If
                I = I + 1
            End While

        Catch ex As Exception
            bSuccess = False
            sErrorDetails = cMethodName & ": strSQLstring=" & strSQLstring & " ex.Message=" & ex.Message & " ex.StackTrace=" & ex.StackTrace
        End Try

        Return bSuccess
    End Function

    Public Shared Function GetApprovalTypeCode(apprType As ApprHistType) As String
        Dim sTypeCode As String = ""

        If apprType = ApprHistType.QuoteApproval Then
            sTypeCode = "Q"
        ElseIf apprType = ApprHistType.BudgetaryApproval Then
            sTypeCode = "B"
        ElseIf apprType = ApprHistType.OrderApproval Then
            sTypeCode = "W"
        ElseIf apprType = ApprHistType.DepartmentApproval Then
            sTypeCode = "D"
        End If

        Return sTypeCode
    End Function

    Private Shared Function GetApprovalStatus(apprStatus As ApprHistStatus) As String
        Dim sStatusCode As String = ""

        If apprStatus = ApprovalHistory.ApprHistStatus.Pending Then
            sStatusCode = "P"
        ElseIf apprStatus = ApprovalHistory.ApprHistStatus.Decline Then
            sStatusCode = "D"
        ElseIf apprStatus = ApprHistStatus.Approve Then
            sStatusCode = "A"
        End If

        Return sStatusCode
    End Function


    Private Shared Function GetApprHistInsertSQL(sBU As String, sOrderNo As String, sEnteredBy As String,
                                                 sApprovedBy As String, sApprovalStatus As String,
                                                 sApprovalTypeCode As String, Optional ByVal sLineNbr As Integer = 0) As String
        Dim strSQLstring As String
        Try
            '  If sLineNbr = 0 Then
            strSQLstring = " INSERT INTO SDIX_APPR_PATH" & vbCrLf &
            " (BUSINESS_UNIT_OM, ORDER_NO," & vbCrLf &
            " SEQ_NBR," & vbCrLf &
            " OPRID_ENTERED_BY, OPRID_APPROVED_BY," & vbCrLf &
            " APPR_STATUS, ISA_APPR_TYPE," & vbCrLf &
            "  ADD_DTTM, LASTUPDDTTM,ISA_LINE_NO)" & vbCrLf &
            " VALUES" & vbCrLf &
            " ('" & sBU & "', '" & sOrderNo & "'," & vbCrLf &
            " (select (DECODE(max(B.SEQ_NBR),null,0,max(B.SEQ_NBR)) + 1)" & vbCrLf &
            " FROM SDIX_APPR_PATH B" & vbCrLf &
            " WHERE B.BUSINESS_UNIT_OM = '" & sBU & "'" & vbCrLf &
            " AND B.ORDER_NO = '" & sOrderNo & "')," & vbCrLf &
            " '" & sEnteredBy & "','" & sApprovedBy & "'," & vbCrLf &
            " '" & sApprovalStatus & "', '" & sApprovalTypeCode & "'," & vbCrLf &
            " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf &
            " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & sLineNbr & ")" & vbCrLf
        Catch ex As Exception
        End Try

        Return strSQLstring
    End Function


End Class
'Imports System.Data.OleDb

Public Class sdiItem

    Public Sub New()

    End Sub

    Public Sub New(ByVal id As String, ByVal desc As String)
        m_id = id
        m_desc = desc
    End Sub

    Public Sub New(ByVal id As String, ByVal desc As String, ByVal typ As sdiItemStockType)
        m_id = id
        m_desc = desc
        m_stockType = typ
    End Sub

    Private m_id As String = ""

    Public Property [Id]() As String
        Get
            Return m_id
        End Get
        Set(ByVal Value As String)
            m_id = Value
        End Set
    End Property

    Private m_commodityCode As String = ""

    Public Property [CommodityCode]() As String
        Get
            Return m_commodityCode
        End Get
        Set(ByVal Value As String)
            m_commodityCode = Value
        End Set
    End Property

    Private m_desc As String = ""

    Public Property [Description]() As String
        Get
            Return m_desc
        End Get
        Set(ByVal Value As String)
            m_desc = Value
        End Set
    End Property

    Private m_stockType As New sdiItemStockType

    Public Property [StockType]() As sdiItemStockType
        Get
            Return m_stockType
        End Get
        Set(ByVal Value As sdiItemStockType)
            m_stockType = Value
        End Set
    End Property

    Private m_invType As String = ""

    Public Property [InventoryType]() As String
        Get
            Return m_invType
        End Get
        Set(ByVal Value As String)
            m_invType = Value
        End Set
    End Property

    Private m_IBU As String = ""

    Public Property [IBusinessUnit]() As String
        Get
            Return m_IBU
        End Get
        Set(ByVal Value As String)
            m_IBU = Value
        End Set
    End Property

    Private m_prodViewId As String = ""

    Public Property [ProductViewId]() As String
        Get
            Return m_prodViewId
        End Get
        Set(ByVal Value As String)
            m_prodViewId = Value
        End Set
    End Property

    Public Overloads Shared Function GetItemInfo(ByVal sdiItemId As String) As sdiItem
        Return (RetrieveItemInfo(sdiItemId))
    End Function

    Private Shared Function RetrieveItemInfo(ByVal sdiItemId As String) As sdiItem
        Dim itm As New sdiItem(sdiItemId, "")
        Try
            sdiItemId = sdiItemId.Trim.ToUpper
            '            Dim sql As String = "" & _
            '"SELECT " & vbCrLf & _
            '" A.INV_ITEM_ID " & vbCrLf & _
            '",A.COMMODITY_CD " & vbCrLf & _
            '",A.INV_ITEM_TYPE " & vbCrLf & _
            '",A.DESCR254 AS ITEM_DESC " & vbCrLf & _
            '",A.INV_STOCK_TYPE " & vbCrLf & _
            '",B.DESCR AS STOCK_TYPE_DESC " & vbCrLf & _
            '",B.SETID AS STOCK_TYPE_SETID " & vbCrLf & _
            '",B.STOCK_OWNER AS STOCK_TYPE_OWNER " & vbCrLf & _
            '"FROM " & vbCrLf & _
            '" PS_INV_ITEMS A " & vbCrLf & _
            '",(" & vbCrLf & _
            '"  SELECT " & vbCrLf & _
            '"   B1.SETID " & vbCrLf & _
            '"  ,B1.INV_STOCK_TYPE " & vbCrLf & _
            '"  ,B1.DESCR " & vbCrLf & _
            '"  ,B1.STOCK_OWNER " & vbCrLf & _
            '"  FROM PS_STOCK_TYPE_INV B1 " & vbCrLf & _
            '"  WHERE B1.SETID = 'MAIN1' " & vbCrLf & _
            '"    AND B1.EFF_STATUS = 'A' " & vbCrLf & _
            '"    AND B1.EFFDT = (" & vbCrLf & _
            '"                    SELECT MAX(B11.EFFDT) " & vbCrLf & _
            '"                    FROM PS_STOCK_TYPE_INV B11 " & vbCrLf & _
            '"                    WHERE B11.SETID = B1.SETID " & vbCrLf & _
            '"                      AND B11.INV_STOCK_TYPE = B1.INV_STOCK_TYPE " & vbCrLf & _
            '"                      AND B11.EFF_STATUS = B1.EFF_STATUS " & vbCrLf & _
            '"                      AND B11.EFFDT <= SYSDATE " & vbCrLf & _
            '"                   )" & vbCrLf & _
            '" ) B " & vbCrLf & _
            '"WHERE A.SETID = 'MAIN1' " & vbCrLf & _
            '"  AND A.EFF_STATUS = 'A' " & vbCrLf & _
            '"  AND A.INV_ITEM_ID = '" & sdiItemId & "' " & vbCrLf & _
            '"  AND A.EFFDT = (" & vbCrLf & _
            '"                    SELECT MAX(A1.EFFDT) " & vbCrLf & _
            '"                    FROM PS_INV_ITEMS A1 " & vbCrLf & _
            '"                    WHERE A1.SETID = A.SETID " & vbCrLf & _
            '"                      AND A1.INV_ITEM_ID = A.INV_ITEM_ID " & vbCrLf & _
            '"                      AND A1.EFF_STATUS = A.EFF_STATUS " & vbCrLf & _
            '"                      AND A1.EFFDT <= SYSDATE " & vbCrLf & _
            '"                )" & vbCrLf & _
            '"  AND A.SETID = B.SETID (+) " & vbCrLf & _
            '"  AND A.INV_STOCK_TYPE = B.INV_STOCK_TYPE (+) " & vbCrLf & _
            '                                " "
            Dim sql As String = "" &
"SELECT A.INV_ITEM_ID ,A.COMMODITY_CD ,A.INV_ITEM_TYPE ,A.DESCR254 AS ITEM_DESC ,A.INV_STOCK_TYPE ,B.DESCR AS STOCK_TYPE_DESC, " & vbCrLf &
" B.SETID AS STOCK_TYPE_SETID ,B.STOCK_OWNER AS STOCK_TYPE_OWNER " & vbCrLf &
" FROM SYSADM8.PS_INV_ITEMS A ,  " & vbCrLf &
" SYSADM8.ps_master_item_tbl c,  " & vbCrLf &
" (SELECT B1.SETID ,B1.INV_STOCK_TYPE ,B1.DESCR ,B1.STOCK_OWNER  " & vbCrLf &
" FROM SYSADM8.PS_STOCK_TYPE_INV B1  " & vbCrLf &
"  WHERE B1.SETID = 'MAIN1'  " & vbCrLf &
" AND   B1.EFF_STATUS = 'A'  " & vbCrLf &
" AND   B1.EFFDT = ( SELECT MAX(B11.EFFDT) " & vbCrLf &
" FROM SYSADM8.PS_STOCK_TYPE_INV B11 " & vbCrLf &
" WHERE B11.SETID = B1.SETID " & vbCrLf &
" AND   B11.INV_STOCK_TYPE = B1.INV_STOCK_TYPE " & vbCrLf &
"  AND B11.EFFDT <= SYSDATE ) ) B " & vbCrLf &
"   WHERE A.SETID = 'MAIN1'  " & vbCrLf &
"  AND   A.INV_ITEM_ID = '" & sdiItemId & "' " & vbCrLf &
"  AND A.SETID = B.SETID (+)  " & vbCrLf &
"  AND A.INV_STOCK_TYPE = B.INV_STOCK_TYPE (+)  AND A.EFFDT = C.ITM_STATUS_EFFDT " & vbCrLf &
"  and a.setid = c.setid " & vbCrLf &
"  and a.inv_item_id= c.inv_item_id " & vbCrLf &
"    and c.ITM_STATUS_CURRENT = '1' " & vbCrLf


            Dim rdr As OleDb.OleDbDataReader = Nothing
            Try
                rdr = ORDBData.GetReader(sql)
            Catch ex As Exception
            End Try
            If Not (rdr Is Nothing) Then
                If rdr.Read Then
                    If Not (rdr("INV_ITEM_ID") Is System.DBNull.Value) Then
                        itm.Id = CStr(rdr("INV_ITEM_ID"))
                    End If
                    If Not (rdr("COMMODITY_CD") Is System.DBNull.Value) Then
                        itm.CommodityCode = CStr(rdr("COMMODITY_CD"))
                    End If
                    If Not (rdr("ITEM_DESC") Is System.DBNull.Value) Then
                        itm.Description = CStr(rdr("ITEM_DESC"))
                    End If
                    If Not (rdr("INV_ITEM_TYPE") Is System.DBNull.Value) Then
                        itm.InventoryType = CStr(rdr("INV_ITEM_TYPE"))
                    End If
                    If Not (rdr("INV_STOCK_TYPE") Is System.DBNull.Value) Then
                        itm.StockType.Id = CStr(rdr("INV_STOCK_TYPE"))
                    End If
                    If Not (rdr("STOCK_TYPE_DESC") Is System.DBNull.Value) Then
                        itm.StockType.Description = CStr(rdr("STOCK_TYPE_DESC"))
                    End If
                    If Not (rdr("STOCK_TYPE_SETID") Is System.DBNull.Value) Then
                        itm.StockType.SetId = CStr(rdr("STOCK_TYPE_SETID"))
                    End If
                    If Not (rdr("STOCK_TYPE_OWNER") Is System.DBNull.Value) Then
                        itm.StockType.StockOwner = CStr(rdr("STOCK_TYPE_OWNER"))
                    End If
                    itm.StockType.IsKnownType = (itm.StockType.Id.Trim.Length > 0 And itm.StockType.Description.Trim.Length > 0)
                End If
                Try
                    rdr.Close()
                Catch ex As Exception
                End Try
            End If
            rdr = Nothing
        Catch ex As Exception
        End Try
        Return (itm)
    End Function

    Private m_arrPriceCurrency As ArrayList = Nothing

    Public ReadOnly Property [PriceCurrencyList]() As ArrayList
        Get
            If (m_arrPriceCurrency Is Nothing) Then
                m_arrPriceCurrency = New ArrayList
            End If
            Return (m_arrPriceCurrency)
        End Get
    End Property

    Public Function GetItemPrice() As sdiItemPrice
        ' always reset the price/currency list each time this method runs
        Me.PriceCurrencyList.Clear()
        ' get price
        Dim itemPr As New sdiItemPrice
        Try
            Dim catalogCNstring As String = SQLDBData.getRepDBConnectString(SQLDBData.DbSQLRepUrl1, "755784")  '   _
            '  HttpContext.Current.ApplicationInstance.Session("CplusDbSQL"))
            Dim bIsCanCheck As Boolean = (Me.IBusinessUnit.Length > 0 And Me.Id.Length > 0) Or
                                         (Me.ProductViewId.Length > 0 And Me.CommodityCode.Length > 0)
            If bIsCanCheck Then
                Dim itm As sdiItemX = Nothing
                'Dim itemBaseCurrency As sdiCurrency = Nothing
                'If (Me.ProductViewId.Length > 0 And Me.CommodityCode.Length > 0) Then
                '    ' use product view Id and commodity code values to get item info
                '    itm = sdiItemCrossReference.getItemInfo(CInt(Me.ProductViewId),
                '                                            CInt(Me.CommodityCode),
                '                                            ORDBData.DbUrl,
                '                                            catalogCNstring)
                'Else
                '    ' use business unit and sdi item Id combination to get item info
                '    itm = sdiItemCrossReference.getItemInfo(Me.IBusinessUnit,
                '                                            Me.Id,
                '                                            ORDBData.DbUrl,
                '                                            catalogCNstring)
                'End If
                If Not (itm Is Nothing) Then
                    'itemBaseCurrency = sdiMultiCurrency.getItemBaseCurrency(itm.sdiItemId)
                    itemPr.UnitPrice = itm.CPlusPromoPrice
                    itemPr.Currency = sdiMultiCurrency.getItemBaseCurrency(itm.sdiItemId)
                End If
            End If
        Catch ex As Exception
        End Try
        ' add itemPr to list
        Me.PriceCurrencyList.Add(itemPr)
        ' return
        Return (itemPr)
    End Function

End Class


Public Class sdiItemCrossReference

    ' prevent others from creating instance of this object
    Private Sub New()
    End Sub


End Class


#Region " sdiItemX object "

Public Class sdiItemX

    Public Sub New()

    End Sub

    Public Sub New(ByVal iBU As String,
                   ByVal sdiItemCode As String)
        m_ibu = iBU
        m_sdiItemId = sdiItemCode
    End Sub

    Public Sub New(ByVal cplusProdViewId As Integer,
                   ByVal cplusItemCode As Integer)
        m_cplusProdViewId = cplusProdViewId
        m_cplusItemId = cplusItemCode
    End Sub

    Private m_sdiItemId As String = ""
    Public Property [sdiItemId]() As String
        Get
            Return m_sdiItemId
        End Get
        Set(ByVal Value As String)
            m_sdiItemId = Value
        End Set
    End Property

    Private m_ibu As String = ""
    Public Property [IBusinessUnit]() As String
        Get
            Return m_ibu
        End Get
        Set(ByVal Value As String)
            m_ibu = Value
        End Set
    End Property

    Private m_cplusProdViewId As Integer = 0
    Public Property [ProductViewId]() As Integer
        Get
            Return m_cplusProdViewId
        End Get
        Set(ByVal Value As Integer)
            m_cplusProdViewId = Value
        End Set
    End Property

    Private m_cplusItemId As Integer = 0
    Public Property [CPlusItemId]() As Integer
        Get
            Return m_cplusItemId
        End Get
        Set(ByVal Value As Integer)
            m_cplusItemId = Value
        End Set
    End Property

    Private m_cplusCatId As Integer = 0
    Public Property [CPlusCatalogId]() As Integer
        Get
            Return m_cplusCatId
        End Get
        Set(ByVal Value As Integer)
            m_cplusCatId = Value
        End Set
    End Property

    Private m_cplusClassId As Integer = 0
    Public Property [CPlusClassId]() As Integer
        Get
            Return m_cplusClassId
        End Get
        Set(ByVal Value As Integer)
            m_cplusClassId = Value
        End Set
    End Property

    Private m_cplusCustItemId As String = ""
    Public Property [CPlusCustomerItemId]() As String
        Get
            Return m_cplusCustItemId
        End Get
        Set(ByVal Value As String)
            m_cplusCustItemId = Value
        End Set
    End Property

    Private m_cplusItemDesc As String = ""
    Public Property [CPlusItemDescription]() As String
        Get
            Return m_cplusItemDesc
        End Get
        Set(ByVal Value As String)
            m_cplusItemDesc = Value
        End Set
    End Property

    Private m_cplusSupShortDesc As String = ""
    Public Property [CPlusSupplierShortDescription]() As String
        Get
            Return m_cplusSupShortDesc
        End Get
        Set(ByVal Value As String)
            m_cplusSupShortDesc = Value
        End Set
    End Property

    Private m_cplusSupLongDesc As String = ""
    Public Property [CPlusSupplierLongDescription]() As String
        Get
            Return m_cplusSupLongDesc
        End Get
        Set(ByVal Value As String)
            m_cplusSupLongDesc = Value
        End Set
    End Property

    Private m_cplusPromoPrice As Decimal = 0
    Public Property [CPlusPromoPrice]() As Decimal
        Get
            Return m_cplusPromoPrice
        End Get
        Set(ByVal Value As Decimal)
            m_cplusPromoPrice = Value
        End Set
    End Property

    Private m_cplusShippableUOM As String = ""
    Public Property [CPlusShippableUOM]() As String
        Get
            Return m_cplusShippableUOM
        End Get
        Set(ByVal Value As String)
            m_cplusShippableUOM = Value
        End Set
    End Property

    Private m_bIsInCPlus As Boolean = False
    Public Property [IsInContentPlus]() As Boolean
        Get
            Return m_bIsInCPlus
        End Get
        Set(ByVal Value As Boolean)
            m_bIsInCPlus = Value
        End Set
    End Property

    ' collection of d/t (key) and ApplicationException type of object
    Private m_ex As Hashtable = Nothing
    Public ReadOnly Property [LatestProcessExceptionList]() As Hashtable
        Get
            If (m_ex Is Nothing) Then
                m_ex = New Hashtable
            End If
            Return (m_ex)
        End Get
    End Property

    Public Sub ClearExceptionList()
        m_ex.Clear()
    End Sub

    Private m_cpJuncTrigFlag As String = ""
    Public Property [CPJunctionTriggerFlag]() As String
        Get
            Return m_cpJuncTrigFlag
        End Get
        Set(ByVal Value As String)
            m_cpJuncTrigFlag = Value
        End Set
    End Property

    Private m_cpJuncSiteId As String = ""
    Public Property [CPJunctionSiteId]() As String
        Get
            Return m_cpJuncSiteId
        End Get
        Set(ByVal Value As String)
            m_cpJuncSiteId = Value
        End Set
    End Property

    Private m_bIsInInSite As Boolean = False
    Public Property [IsInInSite]() As Boolean
        Get
            Return m_bIsInInSite
        End Get
        Set(ByVal Value As Boolean)
            m_bIsInInSite = Value
        End Set
    End Property

End Class
Public Class SQLDBData




    Public Shared Function getRepDBConnectString(ByVal strConnectString, ByVal strCatalogID) As String

        getRepDBConnectString = strConnectString & strCatalogID


    End Function

    Public Shared ReadOnly Property DbSQLRepUrl1() As String

        Get
            Return ConfigurationSettings.AppSettings("SQLDBReplicateConString1")
        End Get
    End Property


End Class
Public Class sdiItemStockType

    Public Sub New()

    End Sub

    Public Sub New(ByVal id As String, ByVal desc As String)
        m_id = id
        m_desc = desc
    End Sub

    Public Sub New(ByVal id As String, ByVal desc As String, ByVal bIsKnownType As Boolean)
        m_id = id
        m_desc = desc
        m_bIsKnownType = bIsKnownType
    End Sub

    Private m_setId As String = "MAIN1"

    Public Property [SetId]() As String
        Get
            Return m_setId
        End Get
        Set(ByVal Value As String)
            m_setId = Value
        End Set
    End Property

    ' default to "ONCE"
    Private m_id As String = "ONCE"

    Public Property [Id]() As String
        Get
            Return m_id
        End Get
        Set(ByVal Value As String)
            m_id = Value
        End Set
    End Property

    Private m_stockOwner As String = ""

    Public Property [StockOwner]() As String
        Get
            Return m_stockOwner
        End Get
        Set(ByVal Value As String)
            m_stockOwner = Value
        End Set
    End Property

    Private m_desc As String = ""

    Public Property [Description]() As String
        Get
            Return m_desc
        End Get
        Set(ByVal Value As String)
            m_desc = Value
        End Set
    End Property

    Private m_bIsKnownType As Boolean = False

    Public Property [IsKnownType]() As Boolean
        Get
            Return m_bIsKnownType
        End Get
        Set(ByVal Value As Boolean)
            m_bIsKnownType = Value
        End Set
    End Property

End Class
#End Region








