Imports System.Data.OleDb
Imports SDI.UNCC.WorkOrderAdapter


Public Class QuoteNonStockProcessor

    Private Const LETTER_HEAD_SdiExch As String = "<div align=""center""><SPAN style=""FONT-SIZE: x-large; WIDTH: 256px; FONT-FAMILY: Arial"">SDI Marketplace</SPAN></div>" & _
                                          "<div align=""center""><SPAN>SDiExchange - Request for Quote</SPAN></div><br><br>"
    Private Const LETTER_HEAD As String = "<div align=""center""><SPAN style=""FONT-SIZE: x-large; WIDTH: 256px; FONT-FAMILY: Arial"">SDI Marketplace</SPAN></div>" & _
                                          "<div align=""center""><SPAN>In-Site® Online - Request for Quote</SPAN></div><br><br>"
    Private Const LETTER_CONTENT As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                             "The above referenced order contains items that required a price " & _
                                             "quote before processing.&nbsp;&nbsp;To view the quoted price either " & _
                                             "click the link below or select the ""Approve Quotes"" menu option " & _
                                             "in In-Site&reg; Online to approve or decline the order." & _
                                             "<br></p>Sincerely,</p>" & _
                                             "<p>SDI Customer Care</p>"
    Private Const LETTER_CONTENT_SDiExchange As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                             "The above referenced order contains items that required a price " & _
                                             "quote before processing.&nbsp;&nbsp;To view the quoted price either " & _
                                             "click the link below or select the ""Approve Quotes"" menu option " & _
                                             "in SDiExchange to approve or decline the order." & _
                                             "<br></p>Sincerely,</p>" & _
                                             "<p>SDI Customer Care</p>"
    Private Const LETTER_CONTENT_PI As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                                "The above referenced order contains items that required a price " & _
                                                "quote before processing.&nbsp;&nbsp;To view the quoted price, please " & _
                                                "select the ""Approve Quotes"" menu option " & _
                                                "in In-Site&reg; Online to approve or decline the order." & _
                                                "<br></p>Sincerely,</p>" & _
                                                "<p>SDI Customer Care</p>"
    Private Const LETTER_CONTENT_PI_SDiExchange As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                                "The above referenced order contains items that required a price " & _
                                                "quote before processing.&nbsp;&nbsp;To view the quoted price, please " & _
                                                "select the ""Approve Quotes"" menu option " & _
                                                "in SDiExchange to approve or decline the order." & _
                                                "<br></p>Sincerely,</p>" & _
                                                "<p>SDI Customer Care</p>"

    Private m_CN As OleDbConnection
    Private m_cEncryptionKey As String
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
    Private m_colMsgs As SDI.WinServices.QuotedNStkItemCollection
    Private m_config As System.Xml.XmlDocument
    Private m_arrPunchInBUList As New ArrayList


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
            m_eventLogger.WriteEntry("evaluating busines rule(s) ...", EventLogEntryType.Information)

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

    '
    ' executes process
    '
    Public Sub Execute()
        Dim cHdr As String = "QuoteNonStockProcessor.Execute: "
        Try
            m_eventLogger.WriteEntry("executing business rule(s) ...", EventLogEntryType.Information)

            m_colMsgs = New SDI.WinServices.QuotedNStkItemCollection

            m_CN.Open()

            If GetQuotedItems() > 0 Then
                SendMessages()
            End If

            m_CN.Close()

            m_colMsgs = Nothing

        Catch ex As Exception
            Try
                m_CN.Dispose()
                m_CN.Close()
            Catch ex1 As Exception

            End Try
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
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

    Private Function GetQuotedItems() As Integer
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
            '      "ORDER BY A1.BUSINESS_UNIT, A1.REQ_ID, A1.LINE_NBR "
            Dim cSQL As String = "" & _
                                 "SELECT " & vbCrLf & _
                                 " A.BUSINESS_UNIT AS BUSINESS_UNIT" & vbCrLf & _
                                 ",A.REQ_ID AS REQ_ID" & vbCrLf & _
                                 ",A1.LINE_NBR AS LINE_NBR" & vbCrLf & _
                                 ",A1.SOLD_TO_CUST_ID AS SOLD_TO_CUST_ID" & vbCrLf & _
                                 ",A2.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                                 ",A2.ISA_EMPLOYEE_NAME AS ISA_EMPLOYEE_NAME" & vbCrLf & _
                                 ",A3.ISA_NONSKREQ_EMAIL AS ISA_NONSKREQ_EMAIL" & vbCrLf & _
                                 ",A4.OPRID_ENTERED_BY AS ISA_EMPLOYEE_ID" & vbCrLf & _
                                 ",A4.BUSINESS_UNIT_OM AS BUSINESS_UNIT_OM" & vbCrLf & _
                                 ",A4.PROJECT_ID,A4.ORIGIN" & vbCrLf & _
                                 ",A4.OPRID_MODIFIED_BY AS OPRID_MODIFIED_BY " & vbCrLf & _
                                 "FROM " & vbCrLf & _
                                 " PS_REQ_HDR A" & vbCrLf & _
                                 ",PS_REQ_LINE A1" & vbCrLf & _
                                 ",PS_ISA_USERS_TBL A2" & vbCrLf & _
                                 ",PS_ISA_ENTERPRISE A3" & vbCrLf & _
                                 ",PS_ISA_ORD_INTFC_H A4" & vbCrLf & _
                                 "WHERE A.BUSINESS_UNIT = A1.BUSINESS_UNIT" & vbCrLf & _
                                 "  AND A.REQ_ID = A1.REQ_ID" & vbCrLf & _
                                 "  AND A.REQ_ID = A4.ORDER_NO (+)" & vbCrLf & _
                                 "  AND A4.ORIGIN IN ('IOL','MOB','RFQ')" & vbCrLf & _
                                 "  AND A4.BUSINESS_UNIT_OM = A2.BUSINESS_UNIT (+)" & vbCrLf & _
                                 "  AND A4.OPRID_ENTERED_BY = A2.ISA_EMPLOYEE_ID (+) " & vbCrLf & _
                                 "  AND 'MAIN1' = A3.SETID (+)" & vbCrLf & _
                                 "  AND A1.SOLD_TO_CUST_ID = A3.CUST_ID (+)" & vbCrLf & _
                                 "  AND A.REQ_STATUS = 'Q' " & vbCrLf & _
                                 "  AND NOT EXISTS (" & vbCrLf & _
                                 "                  SELECT 'X'" & vbCrLf & _
                                 "                  FROM PS_ISA_REQ_EML_LOG B" & vbCrLf & _
                                 "                  WHERE B.BUSINESS_UNIT = A.BUSINESS_UNIT" & vbCrLf & _
                                 "                    AND B.REQ_ID = A.REQ_ID" & vbCrLf & _
                                 "                 )" & vbCrLf & _
                                 "ORDER BY A1.BUSINESS_UNIT, A1.REQ_ID, A1.LINE_NBR " & vbCrLf & _
                                 ""

            Dim rdr As OleDbDataReader
            Dim cmd As OleDbCommand = m_CN.CreateCommand

            With cmd
                .CommandText = cSQL
                .CommandType = CommandType.Text
            End With

            'm_eventLogger.WriteEntry(cHdr & "executing -> " & cSQL, EventLogEntryType.Information)
            rdr = cmd.ExecuteReader

            If rdr.HasRows Then
                Dim cKey As String
                Dim bNew As Boolean
                Dim boItem As QuotedNStkItem
                Dim sModifiedByID As String = ""

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
                        For Each oItm As SDI.WinServices.QuotedNStkItem In m_colMsgs
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
                                If SDI.WinServices.Utility.IsValidEmailAdd(sCC) Then
                                    boItem.CC &= sCC & ";"
                                End If
                            Next
                        End If

                        ' assign defined BCC
                        If m_extendedBCC.Count > 0 Then
                            For Each sBCC As String In m_extendedBCC
                                If SDI.WinServices.Utility.IsValidEmailAdd(sBCC) Then
                                    boItem.BCC &= sBCC & ";"
                                End If
                            Next
                        End If

                        ' add into our collection object
                        m_colMsgs.Add(boItem)
                    End If

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
                                                                                        unccOrderNo:=orderNoMapper.changeToUNCCOrderNo(orderno:=boItem.OrderID))
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
                            Dim arr As ArrayList = SDI.WinServices.Utility.ExtractValidEmails(CType(rdr("ISA_NONSKREQ_EMAIL"), String).Trim)
                            If arr.Count > 0 Then
                                ' just pick up the very first valid email
                                boItem.FROM = CType(arr(0), String)
                            End If
                            arr = Nothing
                        End If
                    End If

                    If Trim(UCase(CType(rdr("ORIGIN"), String))) = "RFQ" Then
                        If Not (rdr("PROJECT_ID") Is System.DBNull.Value) Then
                            Dim sProjectId As String = CType(rdr("PROJECT_ID"), String)
                            Dim arrProjID() As String = Split(sProjectId, "|")

                            boItem.TO &= arrProjID(1)

                        End If
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
                            Dim cAddressee As String = SDI.WinServices.Utility.FormatAddessee(CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim)
                            If Not (cAddressee.Trim.Length > 0) Then
                                cAddressee = CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim
                            End If
                            boItem.Addressee = cAddressee
                        End If

                        ' get the email address
                        If Trim(UCase(CType(rdr("ORIGIN"), String))) = "RFQ" Then

                        Else
                            If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                                'If SDI.WinServices.Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                                '    boItem.TO = CType(rdr("ISA_EMPLOYEE_EMAIL"), String)
                                'End If
                                Dim arr As ArrayList = SDI.WinServices.Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
                                If arr.Count > 0 Then
                                    For Each sAdd As String In arr
                                        boItem.TO &= sAdd & ";"
                                    Next
                                End If
                                arr = Nothing
                            End If
                        End If
                        'If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                        '    'If SDI.WinServices.Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                        '    '    boItem.TO = CType(rdr("ISA_EMPLOYEE_EMAIL"), String)
                        '    'End If
                        '    Dim arr As ArrayList = SDI.WinServices.Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
                        '    If arr.Count > 0 Then
                        '        For Each sAdd As String In arr
                        '            boItem.TO &= sAdd & ";"
                        '        Next
                        '    End If
                        '    arr = Nothing
                        'End If
                    Else
                        ' add this email address into the CC field if not same as the
                        ' primary recipient of this email
                        Dim cAdd As String = ""

                        If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                            'If SDI.WinServices.Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                            '    cAdd = CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim
                            'End If
                            Dim arr As ArrayList = SDI.WinServices.Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
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

                    ' put back the item into our collection object
                    m_colMsgs(boItem.IndexInCollection) = boItem
                End While

                boItem = Nothing
            End If

            rdr.Close()

            rdr = Nothing
            cmd = Nothing

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
                                            Dim cAddressee As String = SDI.WinServices.Utility.FormatAddessee(CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim)
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
                                            Dim arr As ArrayList = SDI.WinServices.Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
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
                                            Dim arr As ArrayList = SDI.WinServices.Utility.ExtractValidEmails(CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim)
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

    Private Sub SendMessages()
        Dim cHdr As String = "QuoteNonStockProcessor.SendMessages: "
        Try
            If m_colMsgs.Count > 0 Then
                Dim eml As System.Web.Mail.MailMessage
                Dim cmd As OleDbCommand
                Dim cSQL As String = ""

                For Each itmQuoted As QuotedNStkItem In m_colMsgs
                    eml = New System.Web.Mail.MailMessage

                    ' init properties of the mail message
                    eml.From = ""
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
                            If SDI.WinServices.Utility.IsValidEmailAdd(sTo) Then
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

                    '' build body of this email message
                    ''vbCrLf & _
                    ''"FROM=" & itmQuoted.FROM & "<br>" & vbCrLf & _
                    ''"default FROM=" & m_defaultFROM & "<br>" & vbCrLf & _
                    ''"TO=" & itmQuoted.TO & "<br>" & vbCrLf & _
                    ''"BCC=" & itmQuoted.BCC & "<br>" & vbCrLf & _
                    ''"key=" & m_cEncryptionKey & "<br>" & vbCrLf & _
                    ''"URL=" & m_cURL & "<br>" & vbCrLf & _
                    '' check if this quote is for a punch-In site BU (if exists in our array/list)
                    ''   they get a different format of the letter and no link
                    'eml.Body = "<HTML>" & _
                    '                "<HEAD></HEAD>" & _
                    '                "<BODY>" & _
                    '                    AddNoRecepientExistNote(eml.To) & _
                    '                    LETTER_HEAD & _
                    '                    FormHTMLQouteInfo(itmQuoted.Addressee, itmQuoted.OrderID) & _
                    '                    LETTER_CONTENT & _
                    '                    FormHTMLLink(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM) & _
                    '                    AddVersionNumber() & _
                    '                "</BODY>" & _
                    '           "</HTML>"
                    Dim bShowOrderId As String = ""
                    If itmQuoted.FormattedOrderID.Length > 0 Then
                        bShowOrderId = itmQuoted.FormattedOrderID
                    ElseIf itmQuoted.OrderID.Length > 0 Then
                        bShowOrderId = itmQuoted.OrderID
                    End If
                    cHdr = cHdr & "VR Start my code.  "
                    Dim bIsBusUnitSDiExch As Boolean = False
                    Try
                        Dim arrBUsForSdiExch() As String = Split(Me.ListBUsSDiExch, ",")
                        If arrBUsForSdiExch.Length > 0 Then
                            If Array.IndexOf(arrBUsForSdiExch, itmQuoted.BusinessUnitOM) > -1 Then
                                bIsBusUnitSDiExch = True
                            End If
                        End If
                    Catch ex As Exception
                        bIsBusUnitSDiExch = False
                    End Try
                    cHdr = cHdr & "VR Middle my code.  "
                    Dim bIsPunchInBU As Boolean = (Me.PunchInBusinessUnitList.IndexOf(itmQuoted.BusinessUnitOM) > -1)
                    If bIsPunchInBU Then
                        If bIsBusUnitSDiExch Then
                            'SdiExchange
                            eml.Body = "<HTML>" & _
                                        "<HEAD></HEAD>" & _
                                        "<BODY>" & _
                                            AddNoRecepientExistNote(eml.To) & _
                                            LETTER_HEAD_SdiExch & _
                                            FormHTMLQouteInfo(itmQuoted.Addressee, bShowOrderId) & _
                                            LETTER_CONTENT_PI_SDiExchange & _
                                            AddVersionNumber() & _
                                        "</BODY>" & _
                                   "</HTML>"
                        Else
                            'InsiteOnline
                            eml.Body = "<HTML>" & _
                                            "<HEAD></HEAD>" & _
                                            "<BODY>" & _
                                                AddNoRecepientExistNote(eml.To) & _
                                                LETTER_HEAD & _
                                                FormHTMLQouteInfo(itmQuoted.Addressee, bShowOrderId) & _
                                                LETTER_CONTENT_PI & _
                                                AddVersionNumber() & _
                                            "</BODY>" & _
                                       "</HTML>"
                        End If
                        'eml.Body = "<HTML>" & _
                        '                "<HEAD></HEAD>" & _
                        '                "<BODY>" & _
                        '                    AddNoRecepientExistNote(eml.To) & _
                        '                    LETTER_HEAD & _
                        '                    FormHTMLQouteInfo(itmQuoted.Addressee, bShowOrderId) & _
                        '                    LETTER_CONTENT_PI & _
                        '                    AddVersionNumber() & _
                        '                "</BODY>" & _
                        '           "</HTML>"
                    Else
                        If bIsBusUnitSDiExch Then
                            'SdiExchange
                            eml.Body = "<HTML>" & _
                                        "<HEAD></HEAD>" & _
                                        "<BODY>" & _
                                            AddNoRecepientExistNote(eml.To) & _
                                            LETTER_HEAD_SdiExch & _
                                            FormHTMLQouteInfo(itmQuoted.Addressee, bShowOrderId) & _
                                            LETTER_CONTENT_SDiExchange & _
                                            FormHTMLLinkSDiExchange(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM) & _
                                            AddVersionNumber() & _
                                        "</BODY>" & _
                                   "</HTML>"
                        Else
                            'InsiteOnline
                            eml.Body = "<HTML>" & _
                                            "<HEAD></HEAD>" & _
                                            "<BODY>" & _
                                                AddNoRecepientExistNote(eml.To) & _
                                                LETTER_HEAD & _
                                                FormHTMLQouteInfo(itmQuoted.Addressee, bShowOrderId) & _
                                                LETTER_CONTENT & _
                                                FormHTMLLink(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM) & _
                                                AddVersionNumber() & _
                                            "</BODY>" & _
                                       "</HTML>"
                        End If
                        'eml.Body = "<HTML>" & _
                        '                "<HEAD></HEAD>" & _
                        '                "<BODY>" & _
                        '                    AddNoRecepientExistNote(eml.To) & _
                        '                    LETTER_HEAD & _
                        '                    FormHTMLQouteInfo(itmQuoted.Addressee, bShowOrderId) & _
                        '                    LETTER_CONTENT & _
                        '                    FormHTMLLink(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM) & _
                        '                    AddVersionNumber() & _
                        '                "</BODY>" & _
                        '           "</HTML>"
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
                                If SDI.WinServices.Utility.IsValidEmailAdd(sTo) Then
                                    eml.To &= sTo & ";"
                                End If
                            Next
                        End If
                    End If

                    ' email is of HTML format
                    eml.BodyFormat = Web.Mail.MailFormat.Html

                    ' send this email
                    System.Web.Mail.SmtpMail.Send(message:=eml)

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
                            System.Web.Mail.SmtpMail.Send(message:=eml)
                        End If
                    Catch ex As Exception
                        ' just ignore
                    End Try
                Next

                If Not (cmd Is Nothing) Then
                    cmd.Dispose()
                End If
            End If

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
    End Sub

    Private Function FormHTMLQouteInfo(ByVal cAddressee As String, ByVal cOrderID As String) As String
        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLQouteInfo: "
        Try
            Dim cInfoHTML As String = ""

            cInfoHTML &= "<TABLE id=""Table1"" cellSpacing=""1"" cellPadding=""1"" width=""100%"" border=""0"">" & _
                                "<TR>" & _
                                    "<TD style=""WIDTH: 80px"">TO:</TD>" & _
                                    "<TD><B>" & cAddressee & "</B></TD>" & _
                                "</TR>" & _
                                "<TR>" & _
                                    "<TD style=""WIDTH: 80px"">Date:</TD>" & _
                                    "<TD>" & DateTime.Now.ToString(Format:="MM/dd/yyyy HH:mm:ss") & "</TD>" & _
                                "</TR>" & _
                                "<TR>" & _
                                    "<TD style=""WIDTH: 80px"">Order:</TD>" & _
                                    "<TD style=""COLOR: purple"">" & cOrderID & "</TD>" & _
                                "</TR>" & _
                         "</TABLE>"

            Return cInfoHTML

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
    End Function

    Private Function FormHTMLLink(ByVal cOrderID As String, ByVal cEmployeeID As String, ByVal cBusinessUnitOM As String) As String
        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLLink: "
        Try
            Dim boEncrypt As New SDI.WinServices.Encryption64

            Dim cParam As String = "?fer=" & boEncrypt.Encrypt(cOrderID, m_cEncryptionKey) & _
                                   "&op=" & boEncrypt.Encrypt(cEmployeeID, m_cEncryptionKey) & _
                                   "&xyz=" & boEncrypt.Encrypt(cBusinessUnitOM, m_cEncryptionKey) & _
                                   "&HOME=N"
            Dim cLink As String = ""

            cLink &= "<p>" & _
                        "Click this " & _
                        "<a href=""" & m_cURL & cParam & """ target=""_blank"">link</a> " & _
                        " to APPROVE or DECLINE order." & _
                     "</p>"

            boEncrypt = Nothing

            Return cLink

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
    End Function

    Private Function FormHTMLLinkSDiExchange(ByVal cOrderID As String, ByVal cEmployeeID As String, ByVal cBusinessUnitOM As String) As String
        Dim cHdr As String = "QuoteNonStockProcessor.FormHTMLLink: "
        Try
            Dim boEncrypt As New SDI.WinServices.Encryption64

            Dim cParam As String = "?fer=" & boEncrypt.Encrypt(cOrderID, m_cEncryptionKey) & _
                                   "&op=" & boEncrypt.Encrypt(cEmployeeID, m_cEncryptionKey) & _
                                   "&xyz=" & boEncrypt.Encrypt(cBusinessUnitOM, m_cEncryptionKey) & _
                                   "&HOME=N" & _
                                   "&ExchHome23=N"
            Dim cLink As String = ""

            cLink &= "<p>" & _
                        "Click this " & _
                        "<a href=""" & m_cURL_SDiExch & cParam & """ target=""_blank"">link</a> " & _
                        " to APPROVE or DECLINE order." & _
                     "</p>"

            boEncrypt = Nothing

            Return cLink

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
    End Function

    Private Function AddNoRecepientExistNote(ByVal sTO As String) As String
        Dim cHdr As String = "QuoteNonStockProcessor.AddNoRecepientExistNote: "
        Try
            Dim cMsg As String = ""

            If Not (sTO.Trim.Length > 0) Then
                cMsg &= "<p style=""COLOR: red""><b>IMPORTANT:</b>&nbsp;&nbsp;<i>Please be advice that this message <b>DOES NOT</b> contain valid recepient.</i></p>"
            End If

            Return cMsg

        Catch ex As Exception
            m_eventLogger.WriteEntry(cHdr & ex.ToString, EventLogEntryType.Error)
            SendAlertMessage(msg:=cHdr & ex.ToString)
        End Try
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
            Dim SrvcNotification As System.Xml.XmlElement

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
            System.Web.Mail.SmtpMail.Send(message:=eml)

            ' clean up
            eml = Nothing

        Catch ex As Exception
            If (msg Is Nothing) Then
                msg = ""
            End If
            m_eventLogger.WriteEntry(cHdr & ".  " & msg & ".  " & ex.ToString, EventLogEntryType.Error)
        End Try
    End Sub

End Class
