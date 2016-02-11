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


Public Class QuoteNonStockProcessor

    Private Const LETTER_HEAD_SdiExch As String = "<table><tbody><tr><td style='width:71%;'><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></td>" & _
                                                    "<td><br/><br/><br/><div align='center'><SPAN style='FONT-SIZE: x-large; WIDTH: 256px; FONT-FAMILY: Arial'>SDI Marketplace</SPAN></div>" & _
                                                    "<div align='center'><SPAN>SDiExchange - Request for Quote</SPAN></div></td></tr></tbody></table>" & _
                                                    "<HR width='100%' SIZE='1'>"
    Private Const LETTER_HEAD As String = "<div><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></div>" & _
                                            "<div align=""center""><SPAN style=""FONT-SIZE: x-large; WIDTH: 256px; FONT-FAMILY: Arial"">SDI Marketplace</SPAN></div>" & _
                                            "<div align=""center""><SPAN>In-Site® Online - Request for Quote</SPAN></div><br><br>" & _
                                            "<HR width='100%' SIZE='1'>"
    Private Const LETTER_CONTENT As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                             "The above referenced order contains items that required a price " & _
                                             "quote before processing.&nbsp;&nbsp;To view the quoted price either " & _
                                             "click the link below or select the ""Requestor Approval"" menu option " & _
                                             "in In-Site&reg; Online to approve or decline the order." & _
                                             "<br></p>Sincerely,</p>" & _
                                             "<p>SDI Customer Care</p>"

    Private Const LETTER_CONTENT_SDiExchange As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                             "The above referenced order contains items that required a price " & _
                                             "quote before processing.&nbsp;&nbsp;To view the quoted price either " & _
                                             "click the link below or select the ""Requestor Approval"" menu option " & _
                                             "in SDiExchange to approve or decline the order." & _
                                             "<br></p>Sincerely,</p>" & _
                                             "<p>SDI Customer Care</p>"

    Private Const LETTER_CONTENT_PI As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                                "The above referenced order contains items that required a price " & _
                                                "quote before processing.&nbsp;&nbsp;To view the quoted price, please " & _
                                                "select the ""Requestor Approval"" menu option " & _
                                                "in In-Site&reg; Online to approve or decline the order." & _
                                                "<br></p>Sincerely,</p>" & _
                                                "<p>SDI Customer Care</p>"

    Private Const LETTER_CONTENT_PI_SDiExchange As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                                "The above referenced order contains items that required a price " & _
                                                "quote before processing.&nbsp;&nbsp;To view the quoted price, please " & _
                                                "select the ""Requestor Approval"" menu option " & _
                                                "in SDiExchange to approve or decline the order." & _
                                                "<br></p>Sincerely,</p>" & _
                                                "<p>SDI Customer Care</p>"


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
    Private m_eventLogger As System.Diagnostics.EventLog
    Private Shared m_colMsgs As QuotedNStkItemCollection = New QuotedNStkItemCollection
    Private m_config As System.Xml.XmlDocument
    Private m_arrPunchInBUList As New ArrayList
    Dim m_quoteProp As QuotedNStkItem = New QuotedNStkItem
    Dim boItem As QuotedNStkItem = New QuotedNStkItem

    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\configSetting.xml"


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

    Public Property EventLogger() As System.Diagnostics.EventLog
        Get
            Return m_eventLogger
        End Get
        Set(ByVal Value As System.Diagnostics.EventLog)
            m_eventLogger = Value
        End Set
    End Property

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

            Dim cSQL As String = "" & _
                  "SELECT COUNT(1) AS RECCOUNT " & vbCrLf & _
                  "FROM PS_REQ_HDR A " & vbCrLf & _
                  "WHERE NOT EXISTS (" & vbCrLf & _
                  "                  SELECT 'X' " & vbCrLf & _
                  "                  FROM PS_ISA_REQ_EML_LOG B " & vbCrLf & _
                  "                  WHERE B.BUSINESS_UNIT = A.BUSINESS_UNIT " & vbCrLf & _
                  "                    AND B.REQ_ID = A.REQ_ID " & vbCrLf & _
                  "                 ) " & vbCrLf & _
                  "  AND A.REQ_STATUS = 'Q' " & vbCrLf & _
                  "  AND NOT EXISTS ( " & vbCrLf & _
                  "                  SELECT 'X' " & vbCrLf & _
                  "                  FROM SYSADM.PS_NLINK_CUST_PLNT C " & vbCrLf & _
                  "                  WHERE C.ISA_SAP_PO_PREF = SUBSTR(A.REQ_ID,1,2) " & vbCrLf & _
                  "                    AND C.ISA_SAP_PO_PREF <> ' ' " & vbCrLf & _
                  "                 ) " & vbCrLf & _
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
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
    End Function

    Private Shared Function buildCartforemail(ByVal m_colMsgs As QuotedNStkItemCollection, ByVal ordNumber As String) As DataTable

        Dim dr As DataRow
        Dim I As Integer
        Dim strPrice As String
        Dim strQty As String
        Dim dstcart As DataTable
        dstcart = New DataTable

        dstcart.Columns.Add("Item ID")
        dstcart.Columns.Add("Description")
        dstcart.Columns.Add("Manuf.")
        dstcart.Columns.Add("Manuf. Partnum")
        dstcart.Columns.Add("QTY")
        dstcart.Columns.Add("UOM")
        dstcart.Columns.Add("Price")
        dstcart.Columns.Add("Ext. Price")
        ' dstcart.Columns.Add("Item ID")
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
        Dim SqlRdr As SqlDataReader = Nothing
        Dim strProdVwId As String = String.Empty
        Dim SqlRdr1 As SqlDataReader = Nothing
        Try
            strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_H where ORDER_NO = '" & ordNumber & "'"
            'strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_H where ORDER_NO = 'M220016429'" 
            'strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_H where ORDER_NO = 'M220016427'"
            OrcRdr = GetReader(strOraSelectQuery)
            If OrcRdr.HasRows Then
                OrcRdr.Read()
                ordIdentifier = CType(OrcRdr("ISA_IDENTIFIER"), String).Trim()
                ordBU = CType(OrcRdr("BUSINESS_UNIT_OM"), String).Trim()
            End If

            strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_L where ISA_PARENT_IDENT = '" & ordIdentifier & "'"
            dsOrdLnItems = GetAdapter(strOraSelectQuery)

            If dsOrdLnItems.Tables(0).Rows.Count > 0 Then
                For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows
                    dr = dstcart.NewRow()
                    dr("Item ID") = CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim()
                    dr("Description") = CType(dataRowMain("DESCR254"), String).Trim()

                    If CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim().Contains("NONCAT") Then

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
                            dr("Item ID") = CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim()
                        Catch ex As Exception
                            dr("Item ID") = " "
                        End Try
                        ''If CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim() <> "" Then
                        ''    Try
                        ''        If CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim() > "0" Then
                        ''            'If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                        ''            '    dr("Bin Location") = getBinLoc(CType(SqlRdr("customerItemID"), String).Trim())
                        ''            'Else
                        ''            '    dr("Bin Location") = getBinLoc(Session("SITEPREFIX") & CType(SqlRdr("customerItemID"), String).Trim())
                        ''            'End If
                        ''            dr("Bin Location") = getBinLoc(CType(dataRowMain("VNDR_CATALOG_ID"), String).Trim())
                        ''        Else
                        ''            dr("Bin Location") = " "
                        ''        End If
                        ''    Catch ex As Exception
                        ''        dr("Bin Location") = " "
                        ''    End Try
                        ''Else
                        ''    dr("Bin Location") = " "
                        ''End If
                        Try
                            dr("UOM") = CType(dataRowMain("UNIT_OF_MEASURE"), String).Trim()
                        Catch ex As Exception
                            dr("UOM") = "EA"
                        End Try
                    Else
                        Dim invItemId As String = String.Empty
                        invItemId = CType(dataRowMain("INV_ITEM_ID"), String).Trim()
                        If invItemId.Length > 0 Then
                            invItemId = invItemId.Substring(3)
                        End If

                        strSqlSelectQuery = "SELECT productviewid FROM [dbo].[ProductViews]" & _
                                            "WHERE productviewname LIKE '" & ordBU & "%'"
                        SqlRdr = GetSQLReaderDazzle(strSqlSelectQuery)
                        If SqlRdr.HasRows Then
                            SqlRdr.Read()
                            strProdVwId = CType(SqlRdr("productviewid"), String).Trim()
                        End If

                        strSqlSelectQuery = "Select CAP.customerItemID, CAP.ItemID, CAP.productViewID, SDIT.manufacturername, SDIT.manufacturerpartnumber, SDIT.shippableunitofmeasure " & _
                                                "FROM ClassAvailableProducts CAP " & _
                                                "inner join ScottsdaleItemTable SDIT " & _
                                                "on (CAP.itemID = SDIT.itemID  AND SDIT.classID = CAP.classID) " & _
                                                "where CAP.customerItemID = '" & invItemId & "' and CAP.productViewID = '" & strProdVwId & "'"

                        SqlRdr = GetSQLReaderDazzle(strSqlSelectQuery)
                        If SqlRdr.HasRows Then
                            SqlRdr.Read()
                            Try
                                dr("Manuf.") = CType(SqlRdr("manufacturername"), String).Trim()
                            Catch ex As Exception
                                dr("Manuf.") = " "
                            End Try
                            Try
                                dr("Manuf. Partnum") = CType(SqlRdr("manufacturerpartnumber"), String).Trim()
                            Catch ex As Exception
                                dr("Manuf. Partnum") = " "
                            End Try
                            Try
                                dr("Item ID") = CType(SqlRdr("ItemID"), String).Trim()
                            Catch ex As Exception
                                dr("Item ID") = " "
                            End Try

                            ''If CType(SqlRdr("customerItemID"), String).Trim() <> "" Then
                            ''    Try
                            ''        If CType(SqlRdr("customerItemID"), String).Trim() > "0" Then
                            ''            dr("Bin Location") = getBinLoc(CType(SqlRdr("customerItemID"), String).Trim())
                            ''        Else
                            ''            dr("Bin Location") = " "
                            ''        End If
                            ''    Catch ex As Exception
                            ''        dr("Bin Location") = " "
                            ''    End Try
                            ''Else
                            ''    dr("Bin Location") = " "
                            ''End If
                            Try
                                dr("UOM") = CType(SqlRdr("shippableunitofmeasure"), String).Trim()
                            Catch ex As Exception
                                dr("UOM") = "EA"
                            End Try
                        End If

                    End If


                    Try
                        strQty = CType(dataRowMain("QTY_REQ"), String).Trim()
                        strQty = strQty.Remove(strQty.Length - 2)
                        dr("QTY") = strQty
                        If IsDBNull(CType(dataRowMain("QTY_REQ"), String).Trim()) Or CType(dataRowMain("QTY_REQ"), String).Trim() = " " Then
                            strQty = "0"
                            'Else
                            '    strQty = CType(dataRowMain("QTY_REQ"), String).Trim()
                            '    strQty = strQty.Remove(strQty.Length - 2)
                        End If
                    Catch ex As Exception
                        strQty = "0"
                    End Try
                    strPrice = "0.00"
                    Try
                        strPrice = CDec(CType(dataRowMain("NET_UNIT_PRICE"), String).Trim()).ToString()
                        strPrice = strPrice.Remove(strPrice.Length - 2)
                        If strPrice Is Nothing Then
                            strPrice = "0.00"
                        End If
                    Catch ex As Exception
                        strPrice = "0.00"
                    End Try
                    If CDec(strPrice) = 0 Then
                        ' dr("Price") = "Call for Price"
                        dr("Price") = "0.00"
                    Else
                        strPrice = CDec(strPrice).ToString("f")
                        dr("Price") = strPrice
                    End If
                    Dim ExtPrice As Decimal = CType(Convert.ToDecimal(strQty) * Convert.ToDecimal(strPrice), String)

                    If (ExtPrice.ToString("f") = "0.00") Then
                        dr("Ext. Price") = "0.00"
                    Else
                        dr("Ext. Price") = ExtPrice.ToString("f")
                    End If


                    'dr("Item Chg Code") = CType(dataRowMain("ISA_CUST_CHARGE_CD"), String).Trim()
                    dr("LN") = CType(dataRowMain("LINE_NBR"), String).Trim()

                    'dr("RFQ") = String.Empty
                    'dr("Requestor Name") = String.Empty
                    'dr("Machine Num") = String.Empty
                    'dr("Tax Exempt") = String.Empty
                    'dr("LPP") = String.Empty
                    'dr("PO") = String.Empty
                    'Try
                    '    dr("SerialID") = String.Empty
                    'Catch ex As Exception
                    '    dr("SerialID") = " "
                    'End Try

                    ' New Code here 

                    Dim strCplusItemid As String = String.Empty
                    If IsDBNull(dr("Manuf.")) And Not String.IsNullOrEmpty(dr("Item ID").ToString()) Then
                        Try
                            Dim strSQLString As String = "SELECT A.ISA_CP_PROD_ID" & vbCrLf & _
                                                         " FROM PS_ISA_CP_JUNCTION A" & vbCrLf & _
                                                         " WHERE A.INV_ITEM_ID = '" & dr("Item ID") & "'"
                            Dim OrcRdr1 As OleDb.OleDbDataReader = GetReader(strSQLString)
                            If OrcRdr1.HasRows Then
                                Try
                                    OrcRdr1.Read()
                                    strCplusItemid = OrcRdr1.GetString(0)
                                Catch ex As Exception
                                    strCplusItemid = ""
                                End Try
                            End If
                        Catch ex As Exception
                            strCplusItemid = ""
                        End Try
                        If strCplusItemid.Trim.Length > 0 Then
                            If Convert.ToInt32(strCplusItemid) > 0 Then
                                Dim strSQLstring As String
                                strSQLstring = "SELECT ScottsDaleItemTable.manufacturerPartNumber," & vbCrLf & _
                                                " ScottsDaleItemTable.manufacturerName," & vbCrLf & _
                                                " ScottsDaleItemTable.shippableUnitOfMeasure," & vbCrLf & _
                                                " classes.classname" & vbCrLf & _
                                                " FROM ScottsDaleItemTable, classes" & vbCrLf & _
                                                " WHERE ScottsDaleItemTable.ItemID = " & strCplusItemid.Trim() & vbCrLf & _
                                                " AND ScottsDaleItemTable.classid = classes.classid"
                                SqlRdr = GetSQLReaderDazzle(strSQLstring)
                                If SqlRdr.Read() Then
                                    If IsDBNull(SqlRdr.Item("manufacturerName")) Then
                                        dr("Manuf.") = " "
                                    Else
                                        dr("Manuf.") = SqlRdr.Item("manufacturerName")
                                    End If

                                    If IsDBNull(SqlRdr.Item("manufacturerPartNumber")) Then
                                        dr("Manuf. Partnum") = " "
                                    Else
                                        dr("Manuf. Partnum") = SqlRdr.Item("manufacturerPartNumber")
                                    End If
                                    If IsDBNull(SqlRdr.Item("shippableUnitOfMeasure")) Then
                                        dr("UOM") = " "
                                    Else
                                        dr("UOM") = SqlRdr.Item("shippableUnitOfMeasure")
                                    End If
                                End If
                            End If
                        End If
                    End If

                    dstcart.Rows.Add(dr)
                Next
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
        Try
            'm_eventLogger.WriteEntry("executing business rule(s) ...", EventLogEntryType.Information)
            SendLogger("Logging execution of Quote Non Stock Utility", "Execution started at " & DateTime.Now.ToShortTimeString(), "LOGGER", "Store", "WebDev@sdi.com", String.Empty, String.Empty)
            SetConfigXML()

            m_colMsgs = New QuotedNStkItemCollection

            m_CN.Open()
            Dim SBstk As New StringBuilder
            Dim SWstk As New StringWriter(SBstk)
            Dim htmlTWstk As New HtmlTextWriter(SWstk)
            Dim SBord As New StringBuilder
            If GetQuotedItems() > 0 Then

                If m_colMsgs.Count > 0 Then

                    For Each itmQuoted As QuotedNStkItem In m_colMsgs
                        SBord.Append(itmQuoted.OrderID + ",")
                        If itmQuoted.PriceBlockFlag = "N" Then
                            SendMessages(itmQuoted)
                        Else
                            PriceUpdate(itmQuoted.OrderID)
                            UpdateReqEmailLog(itmQuoted)
                            buildNotifyApprover(itmQuoted)
                        End If
                    Next
                End If
            End If

            SendLogger("Logging execution of Quote Non Stock Utility", "The following Orders were processed " & SBord.ToString().TrimEnd(",") & "by the utility. <br/>Execution is ended at " & DateTime.Now.ToShortTimeString(), "LOGGGER", "Store", "WebDev@sdi.com", String.Empty, String.Empty)

            m_CN.Close()

            m_colMsgs = Nothing

        Catch ex As Exception
            'SendLogger("Quote Non Stock New Utility - Dev", ex, "ERROR")
            Try
                m_CN.Dispose()
                m_CN.Close()
            Catch ex1 As Exception

            End Try
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
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

                ' start processing
                'Execute()
            End If

        Catch ex As Exception
            SendLogger("Error in Quote Non Stock Utility", ex, "ERROR")
            'MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim objException As String
            Dim objExceptionTrace As String

            SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal exception As Exception, ByVal messageType As String, Optional ByVal Query As String = "", Optional ByVal ConString As String = "")
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim objException As String
            Dim objExceptionTrace As String
            Dim StrQuery As String = String.Empty
            Dim ConStr As String = String.Empty
            Try
                objException = "<b> Exception </b> - " & exception.Message & "<br/>"
                objExceptionTrace = "<b> Exception Trace </b> - " & exception.StackTrace & "<br/>"
                StrQuery = "<b> Execution Query </b> - " & Query & "<br/>"
                ConStr = "<b> execution Connection string </b> - " & ConString & "<br/>"
            Catch ex As Exception
            End Try
            SDIEmailService.EmailUtilityServices("Mail", "SDIExchADMIN@sdi.com", "WebDev@sdi.com", subject, String.Empty, String.Empty, objException & objExceptionTrace & Query & ConStr, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())
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
        m_eventLogger = Nothing
        m_colMsgs = Nothing
        m_config = Nothing
    End Sub

    Public Function GetQuotedItems() As Integer
        Dim cHdr As String = "QuoteNonStockProcessor.GetQuotedItems: "
        Try
            'Dim cSQL As String = _
            '"SELECT A.BUSINESS_UNIT AS BUSINESS_UNIT, A.REQ_ID AS REQ_ID, " & _
            '       "A1.LINE_NBR AS LINE_NBR, A1.SOLD_TO_CUST_ID AS SOLD_TO_CUST_ID, " & _
            '       "A2.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL, A2.ISA_EMPLOYEE_NAME AS ISA_EMPLOYEE_NAME, " & _
            '       "A3.ISA_NONSKREQ_EMAIL AS ISA_NONSKREQ_EMAIL, " & _
            '       "A4.OPRID_ENTERED_BY AS ISA_EMPLOYEE_ID, A4.BUSINESS_UNIT_OM AS BUSINESS_UNIT_OM, " & _
            '       "A4.OPRID_MODIFIED_BY AS OPRID_MODIFIED_BY " & _
            '"FROM PS_REQ_HDR A, PS_REQ_LINE A1, PS_ISA_USERS_TBL A2, PS_ISA_ENTERPRISE A3, PS_ISA_ORD_INTFC_H A4 " & _
            '"WHERE (LPAD(A.BUSINESS_UNIT,5,' ') || LPAD(A.REQ_ID,10,' ')) NOT IN " & _
            '      "(SELECT (LPAD(B.BUSINESS_UNIT,5,' ') || LPAD(B.REQ_ID,10,' ')) AS myKEY " & _
            '      " FROM PS_ISA_REQ_EML_LOG B) " & _
            '      "AND A1.BUSINESS_UNIT(+) = A.BUSINESS_UNIT AND A1.REQ_ID(+) = A.REQ_ID " & _
            '      "AND A3.CUST_ID(+) = A1.SOLD_TO_CUST_ID " & _
            '      "AND A4.ORDER_NO(+) = A.REQ_ID AND (A4.ORIGIN = 'IOL' OR A4.ORIGIN = 'MOB') " & _
            '      "AND A4.BUSINESS_UNIT_OM = A2.BUSINESS_UNIT(+) " & _
            '      "AND A4.OPRID_ENTERED_BY = A2.ISA_EMPLOYEE_ID(+) " & _
            '      "AND A.REQ_STATUS = 'Q' " & _
            '      "ORDER BY A1.BUSINESS_UNIT, A1.REQ_ID, A1.LINE_NBR " ISA_PRICE_BLOCK
            Dim cSQL As String = "" & _
                                 "SELECT " & vbCrLf & _
                                 " A.BUSINESS_UNIT AS BUSINESS_UNIT" & vbCrLf & _
                                 ",A.REQ_ID AS REQ_ID,A.BUYER_ID,B.DESCR,B.EMAILID" & vbCrLf & _
                                 ",A1.LINE_NBR AS LINE_NBR" & vbCrLf & _
                                 ",A1.SOLD_TO_CUST_ID AS SOLD_TO_CUST_ID" & vbCrLf & _
                                 ",A2.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                                 ",A2.ISA_EMPLOYEE_NAME AS ISA_EMPLOYEE_NAME" & vbCrLf & _
                                 ",A2.ISA_PRICE_BLOCK AS ISA_PRICE_BLOCK" & vbCrLf & _
                                 ",A3.ISA_NONSKREQ_EMAIL AS ISA_NONSKREQ_EMAIL" & vbCrLf & _
                                 ",A4.OPRID_ENTERED_BY AS ISA_EMPLOYEE_ID" & vbCrLf & _
                                 ",A4.BUSINESS_UNIT_OM AS BUSINESS_UNIT_OM" & vbCrLf & _
                                 ",A4.PROJECT_ID,A4.ORIGIN" & vbCrLf & _
                                 ",A4.OPRID_MODIFIED_BY AS OPRID_MODIFIED_BY,B1.WORK_ORDER_ID,B1.EMAIL_ADDRESS " & vbCrLf & _
                                 "FROM " & vbCrLf & _
                                 " PS_REQ_HDR A" & vbCrLf & _
                                 ",SYSADM.PS_ROLEXLATOPR B" & vbCrLf & _
                                 ",PS_REQ_LINE A1" & vbCrLf & _
                                 ",PS_ISA_USERS_TBL A2" & vbCrLf & _
                                 ",PS_ISA_ENTERPRISE A3" & vbCrLf & _
                                 ",PS_ISA_ORD_INTFC_H A4" & vbCrLf & _
                                 ",SYSADM.PS_ISA_INTFC_H_SUP B1" & vbCrLf & _
                                 "WHERE A.BUSINESS_UNIT = A1.BUSINESS_UNIT" & vbCrLf & _
                                 "  AND A.BUYER_ID = B.ROLEUSER (+)" & vbCrLf & _
                                 " AND A4.BUSINESS_UNIT_OM = B1.BUSINESS_UNIT_OM (+)" & vbCrLf & _
                                 " AND A4.ORDER_NO = B1.ORDER_NO (+)" & vbCrLf & _
                                 "  AND A.REQ_ID = A1.REQ_ID" & vbCrLf & _
                                 "  AND A.REQ_ID = A4.ORDER_NO (+)" & vbCrLf & _
                                 "  AND A4.ORIGIN IN ('IOL','MOB','RFQ')" & vbCrLf & _
                                 "  AND A4.BUSINESS_UNIT_OM = A2.BUSINESS_UNIT (+)" & vbCrLf & _
                                 "  AND A4.OPRID_ENTERED_BY = A2.ISA_EMPLOYEE_ID (+) " & vbCrLf & _
                                 "  AND 'MAIN1' = A3.SETID (+)" & vbCrLf & _
                                 "  AND A1.SOLD_TO_CUST_ID = A3.CUST_ID (+)" & vbCrLf & _
                                 "  AND A.REQ_STATUS = 'Q' " & vbCrLf & _
                                 "  AND NOT EXISTS ( " & vbCrLf & _
                                 "                  SELECT 'X' " & vbCrLf & _
                                 "                  FROM SYSADM.PS_NLINK_CUST_PLNT C " & vbCrLf & _
                                 "                  WHERE C.ISA_SAP_PO_PREF = SUBSTR(A.REQ_ID,1,2) " & vbCrLf & _
                                 "                    AND C.ISA_SAP_PO_PREF <> ' ' " & vbCrLf & _
                                 "                 ) " & vbCrLf & _
                                 "  AND NOT EXISTS (" & vbCrLf & _
                                 "                  SELECT 'X'" & vbCrLf & _
                                 "                  FROM PS_ISA_REQ_EML_LOG B" & vbCrLf & _
                                 "                  WHERE B.BUSINESS_UNIT = A.BUSINESS_UNIT" & vbCrLf & _
                                 "                    AND B.REQ_ID = A.REQ_ID" & vbCrLf & _
                                 "                 )" & vbCrLf & _
                                 " ORDER BY A1.BUSINESS_UNIT, A1.REQ_ID, A1.LINE_NBR " & vbCrLf & _
                                 ""

            Dim rdr As OleDbDataReader
            Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
            Dim connection As OleDbConnection = New OleDbConnection(connectionString)
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

                Dim priceBlock As String = String.Empty

                Dim strBuyerDescr As String = ""
                Dim strBuyerEmail As String = ""

                While rdr.Read
                    cKey = ""
                    bNew = True
                    boItem = Nothing

                    ' get the key for the current record
                    cKey = CType(rdr("BUSINESS_UNIT"), String).Trim.PadLeft(5, CType(" ", Char)) & _
                           CType(rdr("REQ_ID"), String).Trim.PadLeft(10, CType(" ", Char))

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
                    If rdr("ISA_PRICE_BLOCK") Is DBNull.Value Then
                        priceBlock = "N"
                    Else
                        priceBlock = Convert.ToString(rdr("ISA_PRICE_BLOCK"))
                    End If
                    boItem.PriceBlockFlag = priceBlock
                    'Session("PriceBlockFlag") = boItem.PriceBlockFlag

                    ' get business unit ID (if not defined yet)
                    If Not (boItem.BusinessUnitID.Length > 0) Then
                        boItem.BusinessUnitID = CType(rdr("BUSINESS_UNIT"), String).Trim
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
                    If boItem.OrderID.Length > 0 And _
                       boItem.BusinessUnitOM.Length > 0 Then
                        If boItem.BusinessUnitOM.ToUpper = orderNoMapper.UNCC_BUSINESS_UNIT_OM Then
                            boItem.FormattedOrderID = orderNoMapper.FormatOrderNoToShow(sdiOrderNo:=boItem.OrderID, _
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

                    ' grab the "project ID" from the INTFC_H since for RFQ's (origin) this field SHOULD have both the "work order#" and "primary recipient email address"
                    If (boItem.OrderOrigin = "RFQ") Then
                        ' VR 11/20/2014 We are not using PROJECT_ID field anymore
                        'If Not (rdr("PROJECT_ID") Is System.DBNull.Value) Then

                        '    Dim sProjectId As String = ""
                        '    Dim arrProjID() As String = New String() {}

                        '    Try
                        '        sProjectId = CStr(rdr("PROJECT_ID")).Trim
                        '    Catch ex As Exception
                        '    End Try
                        '    If (sProjectId.Length > 0) Then
                        '        arrProjID = Split(sProjectId, "|")
                        '    End If

                        '    If (arrProjID.Length > 0) Then
                        '        If (arrProjID(0).Trim.Length > 0) Then
                        '            workOrderNo = arrProjID(0).Trim
                        '        End If
                        '    End If
                        '    If (arrProjID.Length > 1) Then
                        '        If (arrProjID(1).Trim.Length > 0) Then
                        '            rfqEmailRecipient = arrProjID(1).Trim
                        '        End If
                        '    End If

                        'End If

                        ' VR 11/20/2014 New code based on using new header table SYSADM.PS_ISA_INTFC_H_SUP
                        If Not (rdr("WORK_ORDER_ID") Is System.DBNull.Value) Then
                            workOrderNo = CStr(rdr("WORK_ORDER_ID")).Trim
                        End If
                        If Not (rdr("EMAIL_ADDRESS") Is System.DBNull.Value) Then
                            rfqEmailRecipient = CStr(rdr("EMAIL_ADDRESS")).Trim
                        End If
                    End If

                    ' get Buyer ID (if not defined yet)
                    If Not (boItem.BuyerId.Length > 0) Then
                        Try
                            If Not (rdr("DESCR") Is System.DBNull.Value) Then
                                boItem.BuyerId = CType(rdr("DESCR"), String).Trim
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

                    ' get the very first available VALID recipient of this message (if not yet)
                    If Not boItem.IsPrimaryRecipientExist Then
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
                                boItem.Addressee = boItem.TO.TrimEnd(";"c)
                                ' as well as the employee Id
                                boItem.EmployeeID = boItem.TO.TrimEnd(";"c)
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

            Return m_colMsgs.Count

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
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
                            Dim sSQL As String = "SELECT ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME, ISA_EMPLOYEE_EMAIL " & _
                                                 "FROM PS_ISA_USERS_TBL " & _
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
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
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

    Public Sub SendMessages(ByVal itmQuoted As QuotedNStkItem)
        Dim cHdr As String = "QuoteNonStockProcessor.SendMessages: "
        Try
            If m_colMsgs.Count > 0 Then

                Dim eml As MailMessage
                Dim cmd As OleDbCommand
                Dim cSQL As String = ""
                Dim MailAttachmentName As String()
                Dim MailAttachmentbytes As New List(Of Byte())()
                Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()

                'For Each itmQuoted As QuotedNStkItem In m_colMsgs
                eml = New System.Web.Mail.MailMessage

                ' init properties of the mail message
                eml.From = "SDIExchange@SDI.com"
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
                If m_extendedTO.Count > 0 Then
                    For Each sTo As String In m_extendedTO
                        If Utility.IsValidEmailAdd(sTo) Then
                            eml.To &= sTo & ";"
                        End If
                    Next
                End If

                ' assign recipient CC email address(es)
                If itmQuoted.CC.Length > 0 Then
                    eml.Cc = itmQuoted.CC
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

                ' override for RFQ origin to include (if provided) the work order # on the subject line
                '   - erwin 3/26/2014
                If (itmQuoted.OrderOrigin = "RFQ") Then
                    If (itmQuoted.WorkOrderNumber.Length > 0) Then
                        eml.Subject &= " (Work Order #" & itmQuoted.WorkOrderNumber & ")"
                    End If
                End If

                Dim sWorkOrder As String = ""
                Try
                    sWorkOrder = itmQuoted.WorkOrderNumber.Trim
                Catch ex As Exception
                    sWorkOrder = ""
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
                dstcartSTK = buildCartforemail(m_colMsgs, itmQuoted.OrderID)
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

                Dim bIsPunchInBU As Boolean = (Me.PunchInBusinessUnitList.IndexOf(itmQuoted.BusinessUnitOM) > -1)
                Dim PI_SDI As String = String.Empty
                Dim PI As String = String.Empty
                Dim ContentSDI As String = String.Empty
                Dim Content As String = String.Empty
                If bIsPunchInBU Then
                    If bIsBusUnitSDiExch Then
                        'SdiExchange
                        If IsAscend(itmQuoted.BusinessUnitOM) Then
                            PI_SDI = LETTER_CONTENT_PI_SDiExchange.Replace("Requestor Approval", "Approve Quotes (Ascend)")
                            bShowWorkOrderNo = True
                        Else
                            PI_SDI = LETTER_CONTENT_PI_SDiExchange
                        End If
                        eml.Body = "<HTML>" & _
                                    "<HEAD></HEAD>" & _
                                    "<BODY>" & _
                                        AddNoRecepientExistNote(eml.To) & _
                                        LETTER_HEAD_SdiExch & _
                                        FormHTMLQouteInfo(itmQuoted.Addressee, strShowOrderId, bShowWorkOrderNo, sWorkOrder) & _
                                        PositionGrid(dataGridHTML) & _
                                        PI_SDI & _
                                        AddBuyerInfo(itmQuoted.BuyerId, itmQuoted.BuyerEmail) & _
                                        AddVersionNumber() & _
                                        "<HR width='100%' SIZE='1'>" & _
                                        "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & _
                                    "</BODY>" & _
                               "</HTML>"
                    Else
                        'InsiteOnline
                        If IsAscend(itmQuoted.BusinessUnitOM) Then
                            PI = LETTER_CONTENT_PI.Replace("Requestor Approval", "Approve Quotes (Ascend)")
                            bShowWorkOrderNo = True
                        Else
                            PI = LETTER_CONTENT_PI
                        End If
                        eml.Body = "<HTML>" & _
                                        "<HEAD></HEAD>" & _
                                        "<BODY>" & _
                                            AddNoRecepientExistNote(eml.To) & _
                                            LETTER_HEAD & _
                                            FormHTMLQouteInfo(itmQuoted.Addressee, strShowOrderId, bShowWorkOrderNo, sWorkOrder) & _
                                            PositionGrid(dataGridHTML) & _
                                            PI & _
                                            AddBuyerInfo(itmQuoted.BuyerId, itmQuoted.BuyerEmail) & _
                                            AddVersionNumber() & _
                                            "<HR width='100%' SIZE='1'>" & _
                                            "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & _
                                        "</BODY>" & _
                                   "</HTML>"
                    End If

                Else
                    If bIsBusUnitSDiExch Then
                        bShowApproveViaEmailLink = True
                        'SdiExchange
                        If IsAscend(itmQuoted.BusinessUnitOM) Then
                            ContentSDI = LETTER_CONTENT_SDiExchange.Replace("Requestor Approval", "Approve Quotes (Ascend)")
                            bShowWorkOrderNo = True
                        Else
                            ContentSDI = LETTER_CONTENT_SDiExchange
                        End If
                        eml.Body = "<HTML>" & _
                                    "<HEAD></HEAD>" & _
                                    "<BODY>" & _
                                        AddNoRecepientExistNote(eml.To) & _
                                        LETTER_HEAD_SdiExch & _
                                        FormHTMLQouteInfo(itmQuoted.Addressee, strShowOrderId, bShowWorkOrderNo, sWorkOrder) &
                                        PositionGrid(dataGridHTML) & _
                                        ContentSDI & _
                                        FormHTMLLinkSDiExchange(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM, bShowApproveViaEmailLink) & _
                                        AddBuyerInfo(itmQuoted.BuyerId, itmQuoted.BuyerEmail) & _
                                        AddVersionNumber() & _
                                        "<HR width='100%' SIZE='1'>" & _
                                            "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & _
                                    "</BODY>" & _
                               "</HTML>"
                    Else
                        'InsiteOnline
                        If IsAscend(itmQuoted.BusinessUnitOM) Then
                            Content = LETTER_CONTENT.Replace("Requestor Approval", "Approve Quotes (Ascend)")
                            bShowWorkOrderNo = True
                        Else
                            Content = LETTER_CONTENT
                        End If
                        eml.Body = "<HTML>" & _
                                        "<HEAD></HEAD>" & _
                                        "<BODY>" & _
                                            AddNoRecepientExistNote(eml.To) & _
                                            LETTER_HEAD & _
                                            FormHTMLQouteInfo(itmQuoted.Addressee, strShowOrderId, bShowWorkOrderNo, sWorkOrder) & _
                                            PositionGrid(dataGridHTML) & _
                                            Content & _
                                            FormHTMLLink(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM, bShowApproveViaEmailLink) & _
                                            AddBuyerInfo(itmQuoted.BuyerId, itmQuoted.BuyerEmail) & _
                                            AddVersionNumber() & _
                                            "<HR width='100%' SIZE='1'>" & _
                                            "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & _
                                        "</BODY>" & _
                                   "</HTML>"
                    End If

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

                ' send this email
                ''System.Web.Mail.SmtpMail.Send(message:=eml)
                Try
                    ''SDIEmailService.EmailUtilityServices("MailandStore", "SDIExchADMIN@sdi.com", "sriram.s@avasoft.biz", eml.Subject, String.Empty, String.Empty, eml.Body, "QUOTEAPPROVAL", MailAttachmentName, MailAttachmentbytes.ToArray())
                    SendLogger(eml.Subject, eml.Body, "QUOTEAPPROVAL", "Mail", eml.To, eml.Cc, eml.Bcc)
                Catch ex As Exception

                End Try

                ' build insert SQL command
                cSQL = _
                "INSERT INTO PS_ISA_REQ_EML_LOG " & _
                "(BUSINESS_UNIT, REQ_ID, ISA_RECIPIENT, ISA_SENDER, ISA_SUBJECT, EMAIL_DATETIME) " & _
                "VALUES " & _
                "(" & _
                    "'" & CType(IIf(itmQuoted.BusinessUnitID.Length > 0, itmQuoted.BusinessUnitID, "."), String) & "', " & _
                    "'" & CType(IIf(itmQuoted.OrderID.Length > 0, itmQuoted.OrderID, "."), String) & "', " & _
                    "'" & "TO=" & eml.To & "CC=" & eml.Cc & "BCC=" & eml.Bcc & "', " & _
                    "'" & CType(IIf(eml.From.Length > 0, eml.From, "."), String) & "', " & _
                    "'" & CType(IIf(eml.Subject.Length > 0, eml.Subject, "."), String) & "', " & _
                    "TO_DATE('" & System.DateTime.Now.ToString & "','MM/DD/YYYY HH:MI:SS AM') " & _
                ")"

                ' create a new instance of the command object
                cmd = New OleDbCommand(cmdText:=cSQL, connection:=m_CN)
                cmd.CommandType = CommandType.Text

                ' execute SQL statement againts the connection object
                Try
                    cmd.ExecuteNonQuery()
                Catch ex As Exception
                    m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
                End Try

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
                        SendLogger(eml.Subject, eml.Body, "QUOTEAPPROVAL", "Mail", eml.To, eml.Cc, eml.Bcc)
                    End If
                Catch ex As Exception
                    ' just ignore
                End Try
                'Next

                If Not (cmd Is Nothing) Then
                    cmd.Dispose()
                End If
            End If

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
            'SendLogger("Quote Non Stock New Utility - Dev", ex, "ERROR")
        End Try
    End Sub

    Private Function FormHTMLQouteInfo(ByVal cAddressee As String, ByVal cOrderID As String, Optional ByVal bIsShowWorkOrderNo As Boolean = False, Optional ByVal cWorkOrderNo As String = "") As String
        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLQouteInfo: "
        Try
            Dim cInfoHTML As String = ""

            cInfoHTML &= "<TABLE id=""Table1"" cellSpacing=""1"" cellPadding=""1"" width=""100%"" border=""0"">"
            cInfoHTML &= "       <TR>" & _
                                    "<TD style=""WIDTH: 110px;Font-Weight:Bold"">TO:</TD>" & _
                                    "<TD><B>" & cAddressee & "</B></TD>" & _
                                "</TR>" & _
                                "<TR>" & _
                                    "<TD style=""WIDTH: 110px;Font-Weight:Bold"">Date:</TD>" & _
                                    "<TD>" & DateTime.Now.ToString(format:="MM/dd/yyyy HH:mm:ss") & "</TD>" & _
                                "</TR>" & _
                                "<TR>" & _
                                    "<TD style=""WIDTH: 110px;Font-Weight:Bold"">Order:</TD>" & _
                                    "<TD style=""COLOR: purple"">" & cOrderID & "</TD>" & _
                                "</TR>"
            If bIsShowWorkOrderNo Then
                cInfoHTML &= "  <TR>" & _
                               "<TD style=""WIDTH: 110px;Font-Weight:Bold"">Work Order:</TD>" & _
                               "<TD style=""COLOR: purple"">" & cWorkOrderNo & "</TD>" & _
                               "</TR>"
            End If
            cInfoHTML &= "</TABLE>"

            Return cInfoHTML

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
        Return ""
    End Function

    Private Function FormHTMLLink(ByVal cOrderID As String, ByVal cEmployeeID As String, ByVal cBusinessUnitOM As String, Optional ByVal bShowLink As Boolean = True) As String
        Dim cLink As String = ""
        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLLink: "
        If bShowLink Then
            Try
                Dim m_cURL1 As String = "http://" & ConfigurationManager.AppSettings("WebAppName") & "Approvequote.aspx"
                Dim boEncrypt As New Encryption64

                Dim cParam As String = "?fer=" & boEncrypt.Encrypt(cOrderID, m_cEncryptionKey) & _
                                       "&op=" & boEncrypt.Encrypt(cEmployeeID, m_cEncryptionKey) & _
                                       "&xyz=" & boEncrypt.Encrypt(cBusinessUnitOM, m_cEncryptionKey) & _
                                       "&HOME=N"

                cLink &= "<p>" & _
                            "Click this " & _
                            "<a href=""" & m_cURL1 & cParam & """ target=""_blank"">link</a> " & _
                            " to APPROVE or DECLINE order." & _
                         "</p>"

                boEncrypt = Nothing

                Return cLink

            Catch ex As Exception
                m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
                SendAlertMessage(msg:=cHdr & ex.ToString)
            End Try
        End If
        Return (cLink)
    End Function

    Private Function FormHTMLLinkSDiExchange(ByVal cOrderID As String, ByVal cEmployeeID As String, ByVal cBusinessUnitOM As String, Optional ByVal bShowLink As Boolean = True) As String
        Dim cLink As String = ""
        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLLink: "
        If bShowLink Then
            Try
                Dim m_cURL1 As String = "http://" & ConfigurationManager.AppSettings("WebAppName") & "Approvequote.aspx"
                Dim boEncrypt As New Encryption64

                Dim cParam As String = "?fer=" & boEncrypt.Encrypt(cOrderID, m_cEncryptionKey) & _
                                       "&op=" & boEncrypt.Encrypt(cEmployeeID, m_cEncryptionKey) & _
                                       "&xyz=" & boEncrypt.Encrypt(cBusinessUnitOM, m_cEncryptionKey) & _
                                       "&HOME=N" & _
                                       "&ExchHome23=N"

                cLink &= "<p>" & _
                            "Click this " & _
                            "<a href=""" & m_cURL1 & cParam & """ target=""_blank"">link</a> " & _
                            " to APPROVE or DECLINE order." & _
                         "</p>"

                boEncrypt = Nothing

                Return cLink

            Catch ex As Exception
                m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
                SendAlertMessage(msg:=cHdr & ex.ToString)
            End Try
        End If
        Return (cLink)
    End Function

    Private Function AddBuyerInfo(ByVal strBuyerDescr As String, ByVal strBuyerEmail As String) As String
        Dim cHdr As String = "QuoteNonStockProcessor.AddBuyerInfo: "
        Try
            Dim cInfoHTML As String = ""

            cInfoHTML &= "<BR />"  '<TABLE id=""Tbl111"" cellSpacing=""1"" cellPadding=""1"" width=""100%"" border=""0"">"
            'cInfoHTML &= "       <TR>" & _
            '                        "<TD style=""WIDTH: 110px"">Buyer:</TD>" & _
            '                        "<TD><B>" & strBuyerDescr & "</B></TD>" & _
            '                    "</TR>"
            'cInfoHTML &= "</TABLE>"

            cInfoHTML &= "" & _
                            "Buyer: " & _
                            "<B>" & strBuyerDescr & "</B>" & _
                         ""
            cInfoHTML &= "<br />" & _
                            "Buyer E-mail: " & _
                            "<a href=""mailto:" & strBuyerEmail & """>" & strBuyerEmail & "</a> " & _
                         "<br />"
            cInfoHTML &= "" & _
                            "Phone Number:  888-435-7734 opt. 7 " & _
                         ""

            Return cInfoHTML

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
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
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
        Return (cMsg)
    End Function

    Private Function AddVersionNumber() As String

        Dim cRet As String = "<br><p align=""right""><FONT face=""Bookman Old Style"" size=""1"">v" & _
                             System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
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

            ' insert the body of the message and send the message
            eml.Body = "" & _
                       "[ IMPORTANT ]" & vbCrLf & _
                       vbTab & " Service sent this ALERT message :: " & System.Environment.MachineName & " :: " & Now.ToString & vbCrLf & vbCrLf & _
                       msg & _
                       ""

            eml.Priority = Web.Mail.MailPriority.High
            'System.Web.Mail.SmtpMail.Send(message:=eml)
            SendLogger(eml.Subject, eml.Body, "ALERTMESSAGE", "Mail", eml.To, eml.Cc, "WebDev@sdi.com")
            ' clean up
            eml = Nothing

        Catch ex As Exception
            If (msg Is Nothing) Then
                msg = ""
            End If
            m_eventLogger.WriteEntry(cHdr & ".  " & msg & ".  " & ex.ToString, EventLogEntryType.Error)
        End Try
    End Sub

    Public Sub PriceUpdate(ByVal orderid As String)
        Try
            Dim strUpdateQuery As String = String.Empty
            Dim strSelectQuery As String = String.Empty
            Dim ordSts As String = "W"
            Dim ordIdent As String = String.Empty
            Dim oprEnteredBy As String = String.Empty
            Dim ordBu As String = String.Empty
            Dim rowsaffected As Integer = 0
            Dim OrcRdr As OleDb.OleDbDataReader = Nothing
            Dim ordStsL As String = "W"
            strSelectQuery = "select OPRID_ENTERED_BY, BUSINESS_UNIT_OM,ISA_IDENTIFIER from ps_isa_ord_intfc_h where ORDER_NO = '" & orderid & "'"
            'strSelectQuery = "select OPRID_ENTERED_BY from ps_isa_ord_intfc_h where ORDER_NO = 'A660000880'"
            OrcRdr = GetReader(strSelectQuery)
            If OrcRdr.HasRows Then
                OrcRdr.Read()
                oprEnteredBy = CType(OrcRdr("OPRID_ENTERED_BY"), String).Trim()
                ordBu = CType(OrcRdr("BUSINESS_UNIT_OM"), String).Trim()
                ordIdent = CType(OrcRdr("ISA_IDENTIFIER"), String).Trim()
            End If


            strSelectQuery = "Select isa_order_status,ISA_IDENTIFIER FROM PS_ISA_ORD_INTFC_L WHERE ISA_PARENT_IDENT='" & ordIdent & "'"

            OrcRdr = GetReader(strSelectQuery)
            If OrcRdr.HasRows Then
                While OrcRdr.Read()
                    If OrcRdr.GetString(0).Trim().ToUpper() = "Q" Then
                        strUpdateQuery = " UPDATE PS_ISA_ORD_INTFC_L" & vbCrLf & _
                                         " SET ISA_ORDER_STATUS = '" & ordStsL & "'" & vbCrLf & _
                                         " WHERE ISA_IDENTIFIER = '" & OrcRdr.GetValue(1) & "'"
                        rowsaffected = ExecNonQuery(strUpdateQuery)
                        SDIAuditInsert("PS_ISA_ORD_INTFC_L", orderid, "ISA_ORDER_STATUS", ordStsL, ordBu)
                    End If
                End While
            End If

            strUpdateQuery = "UPDATE PS_ISA_ORD_INTFC_H" & vbCrLf & _
                        " SET OPRID_APPROVED_BY = '" & oprEnteredBy & "'," & vbCrLf & _
                        " ORDER_STATUS = '" & ordSts & "'" & vbCrLf & _
                        " WHERE ORDER_NO = '" & orderid & "'"

            rowsaffected = ExecNonQuery(strUpdateQuery)

            SDIAuditInsert("PS_ISA_ORD_INTFC_H", orderid, "OPRID_APPROVED_BY", oprEnteredBy, ordBu)
            SDIAuditInsert("PS_ISA_ORD_INTFC_H", orderid, "ORDER_STATUS", ordSts, ordBu)

            'strSelectQuery = "select ISA_IDENTIFIER from ps_isa_ord_intfc_h where ORDER_NO = '" & orderid & "'"
            'OrcRdr = GetReader(strSelectQuery)
            'If OrcRdr.HasRows Then
            '    OrcRdr.Read()
            '    ordIdent = CType(OrcRdr("ISA_IDENTIFIER"), String).Trim()
            'End If

            'strUpdateQuery = "UPDATE PS_ISA_ORD_INTFC_L" & vbCrLf & _
            '            " SET ISA_ORDER_STATUS = '" & ordSts & "'" & vbCrLf & _
            '            " WHERE ISA_PARENT_IDENT = '" & ordIdent & "'"
            'rowsaffected = ExecNonQuery(strUpdateQuery)

            'SDIAuditInsert("PS_ISA_ORD_INTFC_L", orderid, "ISA_ORDER_STATUS", ordSts, ordBu)

            SDIApprovalHistoryInsert(ordBu, orderid, oprEnteredBy)

        Catch ex As Exception
            SendLogger("Error in Quoted Non Stock Email Utility", ex, "ERROR")
        End Try
    End Sub

    Public Sub SDIAuditInsert(ByVal sTableName As String, ByVal sOrdNum As String, ByVal sColumnChg As String, ByVal sNewValue As String, ByVal sBU As String)
        Try
            Dim strInsertQuery As String = String.Empty
            Dim rowsaffected As Integer = 0

            strInsertQuery = "INSERT INTO sysadm.ps_isa_SDIXaudit " & vbCrLf & _
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

            rowsaffected = ExecNonQuery(strInsertQuery)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SDIApprovalHistoryInsert(ByVal sBU As String, ByVal sOrderNo As String, ByVal sEnteredBy As String)
        Try
            Dim strInsertQuery As String = String.Empty
            Dim rowsaffected As Integer = 0

            strInsertQuery = "INSERT INTO PS_ISA_APPR_PATH" & vbCrLf & _
                    " (BUSINESS_UNIT_OM, ORDER_NO," & vbCrLf & _
                    " SEQ_NBR," & vbCrLf & _
                    " OPRID_ENTERED_BY, OPRID_APPROVED_BY," & vbCrLf & _
                    " APPR_STATUS, ISA_APPR_TYPE," & vbCrLf & _
                    "  ADD_DTTM, LASTUPDDTTM)" & vbCrLf & _
                    " VALUES" & vbCrLf & _
                    " ('" & sBU & "', '" & sOrderNo & "', '1'," & vbCrLf & _
                    " '" & sEnteredBy & "','NSTKpric'," & vbCrLf & _
                    " 'P', 'Q'," & vbCrLf & _
                    " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
                    " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))" & vbCrLf

            rowsaffected = ExecNonQuery(strInsertQuery)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub UpdateReqEmailLog(ByVal itmQuoted As QuotedNStkItem)

        Dim strInsertQuery As String = String.Empty
        Dim rowsaffected As Integer = 0

        Try

            Dim eml = New System.Web.Mail.MailMessage

            ' init properties of the mail message
            eml.From = "SDIExchange@SDI.com"
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
            If m_extendedTO.Count > 0 Then
                For Each sTo As String In m_extendedTO
                    If Utility.IsValidEmailAdd(sTo) Then
                        eml.To &= sTo & ";"
                    End If
                Next
            End If

            ' assign recipient CC email address(es)
            If itmQuoted.CC.Length > 0 Then
                eml.Cc = itmQuoted.CC
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
                If (itmQuoted.WorkOrderNumber.Length > 0) Then
                    eml.Subject &= " (Work Order #" & itmQuoted.WorkOrderNumber & ")"
                End If
            End If

            strInsertQuery = "INSERT INTO PS_ISA_REQ_EML_LOG " & _
                "(BUSINESS_UNIT, REQ_ID, ISA_RECIPIENT, ISA_SENDER, ISA_SUBJECT, EMAIL_DATETIME) " & _
                "VALUES " & _
                "(" & _
                    "'" & CType(IIf(itmQuoted.BusinessUnitID.Length > 0, itmQuoted.BusinessUnitID, "."), String) & "', " & _
                    "'" & CType(IIf(itmQuoted.OrderID.Length > 0, itmQuoted.OrderID, "."), String) & "', " & _
                    "'" & "TO=" & eml.To & "CC=" & eml.Cc & "BCC=" & eml.Bcc & "', " & _
                    "'" & CType(IIf(eml.From.Length > 0, eml.From, "."), String) & "', " & _
                    "'" & CType(IIf(eml.Subject.Length > 0, eml.Subject, "."), String) & "', " & _
                    "TO_DATE('" & System.DateTime.Now.ToString & "','MM/DD/YYYY HH:MI:SS AM') " & _
                ")"

            rowsaffected = ExecNonQuery(strInsertQuery)

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

    Public Function PositionGrid(ByVal htmlGrid As String) As String
        Try
            Dim content As String = "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & _
                                    "<TR><TD Class='DetailRow' width='100%'>" & htmlGrid & "</TD></TR>" & _
                                    "</TABLE>"
            Return content
        Catch ex As Exception

        End Try
    End Function

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
        Dim strSQLString As String = "" & _
                    "SELECT " & vbCrLf & _
                    " (C.STORAGE_AREA ||" & vbCrLf & _
                    "  C.STOR_LEVEL_1 ||" & vbCrLf & _
                    "  C.STOR_LEVEL_2 ||" & vbCrLf & _
                    "  C.STOR_LEVEL_3 ||" & vbCrLf & _
                    "  C.STOR_LEVEL_4) as binloc " & vbCrLf & _
                    "FROM " & vbCrLf & _
                    " PS_INV_ITEMS B " & vbCrLf & _
                    ",PS_PHYSICAL_INV C " & vbCrLf & _
                    "WHERE B.INV_ITEM_ID = '" & stritemid & "' " & vbCrLf & _
                    "  AND B.EFFDT = (" & vbCrLf & _
                    "                 SELECT MAX(B_ED.EFFDT) FROM PS_INV_ITEMS B_ED " & vbCrLf & _
                    "  	 	          WHERE B.SETID = B_ED.SETID " & vbCrLf & _
                    "  		            AND B.INV_ITEM_ID = B_ED.INV_ITEM_ID " & vbCrLf & _
                    "		            AND B_ED.EFFDT <= SYSDATE" & vbCrLf & _
                    "                ) " & vbCrLf & _
                    "  AND B.INV_ITEM_ID = C.INV_ITEM_ID(+) " & vbCrLf & _
                    " AND C.QTY > 0 " & vbCrLf & _
                    "ORDER BY C.DT_TIMESTAMP DESC " & vbCrLf & _
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
        Dim strSQLString As String
        Dim strappName As String

        Dim strreqID As String = itmQuoted.OrderID
        Dim strBU As String = itmQuoted.BusinessUnitID
        Dim strAppUserid As String = String.Empty
        Dim strAppAltUserid As String = String.Empty
        Dim stritemid As String = String.Empty

        strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                " LAST_NAME_SRCH," & vbCrLf & _
                " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                " FROM PS_ISA_USERS_TBL" & vbCrLf & _
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

        strSQLString = "SELECT A.ISA_IOL_APR_EMP_ID, A.ISA_IOL_APR_ALT " & vbCrLf & _
           " FROM PS_ISA_USERS_APPRV A " & vbCrLf & _
           " WHERE ISA_EMPLOYEE_ID = '" & itmQuoted.EmployeeID & "' " & vbCrLf & _
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
        Dim strHldSts As String = String.Empty
        strSQLString = "Select Isa_Identifier,HOLD_STATUS from PS_ISA_ORD_INTFC_H where Order_No='" & itmQuoted.OrderID & "' "
        dtrAppReader = GetReader(strSQLString)
        If dtrAppReader.HasRows() = True Then
            dtrAppReader.Read()
            Ident = dtrAppReader.Item("Isa_Identifier")
            strHldSts = dtrAppReader.Item("HOLD_STATUS")
        Else
            dtrAppReader.Close()
            Exit Sub
        End If

        strSQLString = "Select INV_ITEM_ID from PS_ISA_ORD_INTFC_L WHERE ISA_PARENT_IDENT='" & Ident & "' "
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
        dstcartSTK = buildCartforemail(m_colMsgs, itmQuoted.OrderID)
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

        strhref = "http://" & ConfigurationManager.AppSettings("WebAppName") & "approveorder.aspx?fer=" & streOrdnum & "&op=" & streApper & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"
        strhrefAlt = "http://" & ConfigurationManager.AppSettings("WebAppName") & "approveorder.aspx?fer=" & streOrdnum & "&op=" & streApperAlt & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"

        If String.Equals(strAppAltUserid.Trim(), strAppUserid.Trim()) Then
            NotifyApprover(strAppUserid, strappName, strreqID, itmQuoted.WorkOrderNumber, stritemid, dataGridHTML, strhref, strHldSts, itmQuoted.BusinessUnitOM)
        Else
            NotifyApprover(strAppUserid, strappName, strreqID, itmQuoted.WorkOrderNumber, stritemid, dataGridHTML, strhref, strHldSts, itmQuoted.BusinessUnitOM)
            NotifyApprover(strAppAltUserid, strappName, strreqID, itmQuoted.WorkOrderNumber, stritemid, dataGridHTML, strhrefAlt, strHldSts, itmQuoted.BusinessUnitOM)
        End If

    End Sub

    Public Shared Sub NotifyApprover(ByVal strappId As String, ByVal strappName As String, ByVal strreqID As String, ByVal WorkOrderNumber As String, ByVal stritemid As String, ByVal dataGridHTML As String, ByVal strhref As String, ByVal strHldSts As String, ByVal BU As String)
        Dim strbodyhead As String
        Dim strbodydetl As String

        Dim MailTo As String = String.Empty
        Dim MailFrom As String = "SDIExchange@SDI.com"
        Dim MailSub As String = String.Empty
        Dim MailBody As String = String.Empty

        Dim AppName As String = String.Empty
        Dim AppMail As String = String.Empty

        Dim strApp As String = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                " LAST_NAME_SRCH," & vbCrLf & _
                " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                " FROM PS_ISA_USERS_TBL" & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & strappId & "'"
        Dim dtrAppReader As OleDbDataReader = GetReader(strApp)
        If dtrAppReader.HasRows() = True Then
            dtrAppReader.Read()
            AppName = dtrAppReader.Item("FIRST_NAME_SRCH") & " " & dtrAppReader.Item("LAST_NAME_SRCH")
            AppMail = dtrAppReader.Item("ISA_EMPLOYEE_EMAIL")
        End If

        If strHldSts.ToUpper = "B" Then
            strbodyhead = "<table width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Arial; font-size: x-large; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center; margin: 0px auto;'>SDiExchange - Request for Budget Approval</span></center></td></tr></tbody></table>" & vbCrLf
            strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        End If
        strbodyhead = "<table width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Arial; font-size: x-large; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center; margin: 0px auto;'>SDiExchange - Request for Approval</span></center></td></tr></tbody></table>" & vbCrLf
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
        If strHldSts.ToUpper = "B" Then
            strbodydetl = strbodydetl & "and has exceeded the charge code budget limit.  Click the link below or select the ""Approve Budget"" "
        Else
            If IsAscend(BU) Then
                strbodydetl = strbodydetl & "and needs your approval.  Click the link below or select the ""Approve Quotes (Ascend)"" "
            Else
                strbodydetl = strbodydetl & "and needs your approval.  Click the link below or select the ""Approve Orders"" "
            End If
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

        MailBody = strbodyhead & strbodydetl
        'MailSub = "SDiExchange - Order Number " & strreqID & " needs approval"
        If strHldSts.ToUpper = "B" Then
            MailSub = "SDiExchange - Order Number " & strreqID & " needs budget approval"
        Else
            MailSub = "SDiExchange - Order Number " & strreqID & " needs approval"
        End If
        MailTo = AppMail
        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()

        SDIEmailService.EmailUtilityServices("MailandStore", MailFrom, MailTo, MailSub, String.Empty, "WebDev@sdi.com", MailBody, "NotifyApprover", MailAttachmentName, MailAttachmentbytes.ToArray())

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
End Class

