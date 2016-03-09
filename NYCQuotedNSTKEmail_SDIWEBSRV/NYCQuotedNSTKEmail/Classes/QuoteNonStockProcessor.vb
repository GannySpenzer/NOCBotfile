Imports System.Data.OleDb

Public Class QuoteNonStockProcessor

    Private Const LETTER_HEAD As String = "<div align=""center""><SPAN style=""FONT-SIZE: x-large; WIDTH: 256px; FONT-FAMILY: Arial"">SDI Marketplace</SPAN></div>" & _
                                          "<div align=""center""><SPAN>SDiExchange - Request for Quote</SPAN></div><br><br>"
    Private Const LETTER_CONTENT As String = "<p style=""TEXT-INDENT: 25pt"">" & _
                                             "The above referenced order contains items that required a price " & _
                                             "quote before processing.&nbsp;&nbsp;To view the quoted price either " & _
                                             "click the link below or select the ""Approve Quotes"" menu option " & _
                                             "in SDiExchange to approve or decline the order." & _
                                             "<br></p>Sincerely,</p>" & _
                                             "<p>SDI Customer Care</p>"
    'Private Const QUOTED_STATUS As String = "W"
    Private Const QUOTED_STATUS As String = "Q"

    Private m_CN As OleDbConnection
    Private m_cEncryptionKey As String
    Private m_defaultFROM As String
    Private m_extendedTO As ArrayList
    Private m_extendedCC As ArrayList
    Private m_extendedBCC As ArrayList
    Private m_defaultSubject As String
    Private m_cURL As String
    Private m_eventLogger As System.Diagnostics.EventLog
    Private m_colMsgs As QuotedNStkItemCollection

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

    Public Property URL() As String
        Get
            Return m_cURL
        End Get
        Set(ByVal Value As String)
            m_cURL = Value
        End Set
    End Property

    '
    ' process to evaluate/check and returns TRUE or FALSE whether to do execution process
    '
    Public Function Evaluate() As Boolean
        Try
            m_eventLogger.WriteEntry("evaluating busines rule(s) ...", EventLogEntryType.Information)

            'Dim cSQL As String = _
            '"SELECT COUNT(*) AS RECCOUNT  " & _
            '"FROM PS_ISA_QUICK_REQ_H A " & _
            '"WHERE (LPAD(A.BUSINESS_UNIT, 5, ' ') || LPAD(A.REQ_ID, 10, ' ')) NOT IN " & _
            '      "(SELECT (LPAD(B.BUSINESS_UNIT, 5, ' ') || LPAD(B.REQ_ID, 10, ' ')) AS myKEY " & _
            '      " FROM PS_ISA_REQ_EML_LOG B) " & _
            '      "AND A.REQ_STATUS = '" & QUOTED_STATUS & "' "
            ''"SELECT COUNT(1) AS RECCOUNT " & _
            ''"FROM PS_ISA_QUICK_REQ_H HDR, " & _
            ''     "PS_ISA_QUICK_REQ_L LNE " & _
            ''"WHERE (LPAD(HDR.BUSINESS_UNIT_OM,5,' ') || LPAD(HDR.REQ_ID,10,' ')) NOT IN " & _
            ''      "(SELECT (LPAD(LOG.BUSINESS_UNIT,5,' ') || LPAD(LOG.REQ_ID,10,' ')) AS myKEY " & _
            ''      " FROM PS_ISA_REQ_EML_LOG LOG) " & _
            ''      "AND LNE.BUSINESS_UNIT(+) = HDR.BUSINESS_UNIT AND LNE.REQ_ID(+) = HDR.REQ_ID " & _
            ''      "AND LNE.ISA_QUOTE_STATUS = '" & QUOTED_STATUS & "' "
            Dim cSQL As String = "" & _
                                 "SELECT COUNT(1) AS RECCOUNT " & vbCrLf & _
                                 "FROM PS_ISA_QUICK_REQ_H A " & vbCrLf & _
                                 "WHERE NOT EXISTS (" & vbCrLf & _
                                 "                  SELECT 'X'" & vbCrLf & _
                                 "                  FROM PS_ISA_REQ_EML_LOG B" & vbCrLf & _
                                 "                  WHERE B.BUSINESS_UNIT = A.BUSINESS_UNIT_OM" & vbCrLf & _
                                 "                    AND B.REQ_ID = A.REQ_ID" & vbCrLf & _
                                 "                 )" & vbCrLf & _
                                 "  AND A.REQ_STATUS = '" & QUOTED_STATUS & "'" & vbCrLf & _
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
                End If
            End If

            rdr.Close()

            rdr = Nothing
            cmd = Nothing

            m_CN.Close()

            Return (nRecs > 0)

        Catch ex As Exception
            m_eventLogger.WriteEntry(ex.ToString, EventLogEntryType.Error)
        End Try
    End Function

    '
    ' executes process
    '
    Public Sub Execute()
        Try
            m_eventLogger.WriteEntry("executing business rule(s) ...", EventLogEntryType.Information)

            m_colMsgs = New QuotedNStkItemCollection

            m_CN.Open()

            If GetQuotedItems() > 0 Then
                SendMessages()
            End If

            m_CN.Close()

            m_colMsgs = Nothing

        Catch ex As Exception
            m_eventLogger.WriteEntry(ex.ToString, EventLogEntryType.Error)
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
        m_cURL = ""

        m_CN = Nothing
        m_eventLogger = Nothing
        m_colMsgs = Nothing
    End Sub

    Private Function GetQuotedItems() As Integer
        Try
            'Dim cSQL As String = _
            '"SELECT HDR.BUSINESS_UNIT_OM AS BUSINESS_UNIT_OM, " & _
            '       "HDR.REQ_ID AS REQ_ID, " & _
            '       "HDR.BUSINESS_UNIT AS BUSINESS_UNIT, " & _
            '       "LNE.LINE_NBR AS LINE_NBR, " & _
            '       "LNE.EMPLID AS ISA_EMPLOYEE_ID, " & _
            '       "USERS.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL, " & _
            '       "USERS.ISA_EMPLOYEE_NAME AS ISA_EMPLOYEE_NAME, " & _
            '       "ENT.ISA_NONSKREQ_EMAIL AS ISA_NONSKREQ_EMAIL " & _
            '"FROM PS_ISA_QUICK_REQ_H HDR, " & _
            '     "PS_ISA_QUICK_REQ_L LNE, " & _
            '     "PS_ISA_USERS_TBL USERS, " & _
            '     "PS_ISA_ENTERPRISE ENT " & _
            '"WHERE (LPAD(HDR.BUSINESS_UNIT_OM,5,' ') || LPAD(HDR.REQ_ID,10,' ')) NOT IN " & _
            '      "(SELECT (LPAD(LOG.BUSINESS_UNIT,5,' ') || LPAD(LOG.REQ_ID,10,' ')) AS myKEY " & _
            '      " FROM PS_ISA_REQ_EML_LOG LOG) " & _
            '      "AND LNE.BUSINESS_UNIT(+) = HDR.BUSINESS_UNIT AND LNE.REQ_ID(+) = HDR.REQ_ID " & _
            '      "AND ENT.ISA_BUSINESS_UNIT(+) = ('I0' || SUBSTR(HDR.BUSINESS_UNIT_OM,3,3)) " & _
            '      "AND LNE.EMPLID = USERS.ISA_EMPLOYEE_ID(+) " & _
            '      "AND HDR.REQ_STATUS = '" & QUOTED_STATUS & "' " & _
            '      "ORDER BY LNE.BUSINESS_UNIT, LNE.REQ_ID, LNE.LINE_NBR "
            ''"SELECT HDR.BUSINESS_UNIT_OM AS BUSINESS_UNIT_OM, " & _
            ''       "HDR.REQ_ID AS REQ_ID, " & _
            ''       "HDR.BUSINESS_UNIT AS BUSINESS_UNIT, " & _
            ''       "LNE.LINE_NBR AS LINE_NBR, " & _
            ''       "LNE.EMPLID AS ISA_EMPLOYEE_ID, " & _
            ''       "USERS.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL, " & _
            ''       "USERS.ISA_EMPLOYEE_NAME AS ISA_EMPLOYEE_NAME, " & _
            ''       "ENT.ISA_NONSKREQ_EMAIL AS ISA_NONSKREQ_EMAIL " & _
            ''"FROM PS_ISA_QUICK_REQ_H HDR, " & _
            ''     "PS_ISA_QUICK_REQ_L LNE, " & _
            ''     "PS_ISA_USERS_TBL USERS, " & _
            ''     "PS_ISA_ENTERPRISE ENT " & _
            ''"WHERE (LPAD(HDR.BUSINESS_UNIT_OM,5,' ') || LPAD(HDR.REQ_ID,10,' ')) NOT IN " & _
            ''      "(SELECT (LPAD(LOG.BUSINESS_UNIT,5,' ') || LPAD(LOG.REQ_ID,10,' ')) AS myKEY " & _
            ''      " FROM PS_ISA_REQ_EML_LOG LOG) " & _
            ''      "AND LNE.BUSINESS_UNIT(+) = HDR.BUSINESS_UNIT AND LNE.REQ_ID(+) = HDR.REQ_ID " & _
            ''      "AND ENT.ISA_BUSINESS_UNIT(+) = ('I0' || SUBSTR(HDR.BUSINESS_UNIT_OM,3,3)) " & _
            ''      "AND LNE.EMPLID = USERS.ISA_EMPLOYEE_ID(+) " & _
            ''      "AND LNE.ISA_QUOTE_STATUS = '" & QUOTED_STATUS & "' " & _
            ''      "ORDER BY LNE.BUSINESS_UNIT, LNE.REQ_ID, LNE.LINE_NBR "
            Dim cSQL As String = "" & _
                                 "SELECT " & vbCrLf & _
                                 " HDR.BUSINESS_UNIT_OM AS BUSINESS_UNIT_OM" & vbCrLf & _
                                 ",HDR.REQ_ID AS REQ_ID" & vbCrLf & _
                                 ",HDR.BUSINESS_UNIT AS BUSINESS_UNIT" & vbCrLf & _
                                 ",LNE.LINE_NBR AS LINE_NBR" & vbCrLf & _
                                 ",LNE.EMPLID AS ISA_EMPLOYEE_ID" & vbCrLf & _
                                 ",USERS.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                                 ",USERS.ISA_EMPLOYEE_NAME AS ISA_EMPLOYEE_NAME" & vbCrLf & _
                                 ",ENT.ISA_NONSKREQ_EMAIL AS ISA_NONSKREQ_EMAIL" & vbCrLf & _
                                 "FROM " & vbCrLf & _
                                 " PS_ISA_QUICK_REQ_H HDR" & vbCrLf & _
                                 ",PS_ISA_QUICK_REQ_L LNE" & vbCrLf & _
                                 ",PS_ISA_USERS_TBL USERS" & vbCrLf & _
                                 ",PS_ISA_ENTERPRISE ENT " & vbCrLf & _
                                 "WHERE HDR.BUSINESS_UNIT = LNE.BUSINESS_UNIT" & vbCrLf & _
                                 "  AND HDR.REQ_ID = LNE.REQ_ID" & vbCrLf & _
                                 "  AND LNE.EMPLID = USERS.ISA_EMPLOYEE_ID (+)" & vbCrLf & _
                                 "  AND ('I0' || SUBSTR(HDR.BUSINESS_UNIT_OM,3,3)) = ENT.ISA_BUSINESS_UNIT (+) " & vbCrLf & _
                                 "  AND HDR.REQ_STATUS = '" & QUOTED_STATUS & "' " & vbCrLf & _
                                 "  AND NOT EXISTS (" & vbCrLf & _
                                 "                  SELECT 'X'" & vbCrLf & _
                                 "                  FROM PS_ISA_REQ_EML_LOG LOG" & vbCrLf & _
                                 "                  WHERE LOG.BUSINESS_UNIT = HDR.BUSINESS_UNIT_OM" & vbCrLf & _
                                 "                    AND LOG.REQ_ID = HDR.REQ_ID" & vbCrLf & _
                                 "                 )" & vbCrLf & _
                                 "ORDER BY LNE.BUSINESS_UNIT, LNE.REQ_ID, LNE.LINE_NBR" & vbCrLf & _
                                 ""

            Dim rdr As OleDbDataReader
            Dim cmd As OleDbCommand = m_CN.CreateCommand

            With cmd
                .CommandText = cSQL
                .CommandType = CommandType.Text
            End With

            rdr = cmd.ExecuteReader

            If rdr.HasRows Then
                Dim cKey As String
                Dim bNew As Boolean
                Dim boItem As QuotedNStkItem

                While rdr.Read
                    cKey = ""
                    bNew = True
                    boItem = Nothing

                    ' get the key for the current record
                    cKey = CType(rdr("BUSINESS_UNIT_OM"), String).Trim.PadLeft(5, CType(" ", Char)) & _
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

                    ' get business unit ID (if not defined yet)
                    If Not (boItem.BusinessUnitID.Length > 0) Then
                        boItem.BusinessUnitID = CType(rdr("BUSINESS_UNIT"), String).Trim
                    End If

                    ' get order ID (if not defined yet)
                    If Not (boItem.OrderID.Length > 0) Then
                        boItem.OrderID = CType(rdr("REQ_ID"), String).Trim
                    End If

                    ' get the first Business Unit OM available
                    If Not (boItem.BusinessUnitOM.Length > 0) Then
                        If Not (rdr("BUSINESS_UNIT_OM") Is System.DBNull.Value) Then
                            boItem.BusinessUnitOM = CType(rdr("BUSINESS_UNIT_OM"), String).Trim
                        End If
                    End If

                    ' get customer ID
                    If Not (boItem.CustomerID.Length > 0) Then
                        If Not (rdr("ISA_EMPLOYEE_ID") Is System.DBNull.Value) Then
                            boItem.CustomerID = CType(rdr("ISA_EMPLOYEE_ID"), String).Trim
                        End If
                    End If

                    ' get the sender email address
                    If Not (boItem.FROM.Length > 0) Then
                        If Not (rdr("ISA_NONSKREQ_EMAIL") Is System.DBNull.Value) Then
                            If Utility.IsValidEmailAdd(CType(rdr("ISA_NONSKREQ_EMAIL"), String)) Then
                                boItem.FROM = CType(rdr("ISA_NONSKREQ_EMAIL"), String).Trim
                            End If
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
                            Dim cAddressee As String = Utility.FormatAddessee(CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim)
                            If Not (cAddressee.Trim.Length > 0) Then
                                cAddressee = CType(rdr("ISA_EMPLOYEE_NAME"), String).Trim
                            End If
                            boItem.Addressee = cAddressee
                        End If

                        ' get the email address
                        If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                            If Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                                boItem.TO = CType(rdr("ISA_EMPLOYEE_EMAIL"), String)
                            End If
                        End If
                    Else
                        ' add this email address into the CC field if not same as the
                        ' primary recipient of this email
                        Dim cAdd As String = ""

                        If Not (rdr("ISA_EMPLOYEE_EMAIL") Is System.DBNull.Value) Then
                            If Utility.IsValidEmailAdd(CType(rdr("ISA_EMPLOYEE_EMAIL"), String)) Then
                                cAdd = CType(rdr("ISA_EMPLOYEE_EMAIL"), String).Trim
                            End If
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

                    ' put back the item into our collection object
                    m_colMsgs(boItem.IndexInCollection) = boItem
                End While

                boItem = Nothing
            End If

            rdr.Close()

            rdr = Nothing
            cmd = Nothing

            Return m_colMsgs.Count

        Catch ex As Exception
            m_eventLogger.WriteEntry(ex.ToString, EventLogEntryType.Error)
        End Try
    End Function

    Private Sub SendMessages()
        Try
            If m_colMsgs.Count > 0 Then
                Dim eml As System.Web.Mail.MailMessage
                Dim cmd As OleDbCommand
                Dim cSQL As String = ""

                For Each itmQuoted As QuotedNStkItem In m_colMsgs
                    eml = New System.Web.Mail.MailMessage

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
                    Else
                        eml.To = ""
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
                    End If

                    ' assign the subject of this email
                    ' or use the default subject line from the configuration file (most probably is)
                    If itmQuoted.Subject.Length > 0 Then
                        eml.Subject = itmQuoted.Subject
                    Else
                        eml.Subject = m_defaultSubject
                    End If

                    ' add the order ID on the subject line of this email
                    If itmQuoted.OrderID.Length > 0 Then eml.Subject &= " - " & itmQuoted.OrderID

                    ' build body of this email message
                    'vbCrLf & _
                    '"FROM=" & itmQuoted.FROM & "<br>" & vbCrLf & _
                    '"default FROM=" & m_defaultFROM & "<br>" & vbCrLf & _
                    '"TO=" & itmQuoted.TO & "<br>" & vbCrLf & _
                    '"BCC=" & itmQuoted.BCC & "<br>" & vbCrLf & _
                    '"key=" & m_cEncryptionKey & "<br>" & vbCrLf & _
                    '"URL=" & m_cURL & "<br>" & vbCrLf & _
                    eml.Body = "<HTML>" & _
                                    "<HEAD></HEAD>" & _
                                    "<BODY>" & _
                                        AddNoRecepientExistNote(eml.To) & _
                                        LETTER_HEAD & _
                                        FormHTMLQouteInfo(itmQuoted.Addressee, itmQuoted.OrderID) & _
                                        LETTER_CONTENT & _
                                        FormHTMLLink(itmQuoted.OrderID, itmQuoted.EmployeeID, itmQuoted.BusinessUnitOM) & _
                                        AddVersionNumber() & _
                                    "</BODY>" & _
                               "</HTML>"

                    ' we need to check for a blank TO field and should return (send) this
                    ' auto mail to the sender's attention.  Besides we already added the notice for
                    ' the body of message
                    If Not (eml.To.Trim.Length > 0) Then
                        eml.To = eml.From
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
                        "'" & CType(IIf(itmQuoted.BusinessUnitOM.Length > 0, itmQuoted.BusinessUnitOM, "."), String) & "', " & _
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
                        m_eventLogger.WriteEntry(ex.ToString, EventLogEntryType.Error)
                    End Try
                Next

                If Not (cmd Is Nothing) Then
                    cmd.Dispose()
                End If
            End If

        Catch ex As Exception
            m_eventLogger.WriteEntry(ex.ToString, EventLogEntryType.Error)
        End Try
    End Sub

    Private Function FormHTMLQouteInfo(ByVal cAddressee As String, ByVal cOrderID As String) As String
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
            m_eventLogger.WriteEntry(ex.ToString, EventLogEntryType.Error)
        End Try
    End Function

    Private Function FormHTMLLink(ByVal cOrderID As String, ByVal cEmployeeID As String, ByVal cBusinessUnitOM As String) As String
        Try
            Dim boEncrypt As New Encryption64

            Dim cParam As String = "?fer=" & boEncrypt.Encrypt(cOrderID, m_cEncryptionKey) & _
                                   "&op=" & boEncrypt.Encrypt(cEmployeeID, m_cEncryptionKey) & _
                                   "&xyz=" & boEncrypt.Encrypt(cBusinessUnitOM, m_cEncryptionKey) & _
                                   "&HOME=N"
            Dim cLink As String = ""

            cLink &= "<p>" & _
                        "Click this " & _
                        "<a href=""" & m_cURL & cParam & """>link</a> " & _
                        " to APPROVE or DECLINE order." & _
                     "</p>"

            boEncrypt = Nothing

            Return cLink

        Catch ex As Exception
            m_eventLogger.WriteEntry(ex.ToString, EventLogEntryType.Error)
        End Try
    End Function

    Private Function AddNoRecepientExistNote(ByVal sTO As String) As String
        Try
            Dim cMsg As String = ""

            If (sTO Is Nothing) Then
                sTO = ""
            End If

            If Not (sTO.Trim.Length > 0) Then
                cMsg &= "<p style=""COLOR: red""><b>IMPORTANT:</b>&nbsp;&nbsp;<i>Please be advised that this message does <b>NOT</b> contain valid recipient.</i></p>"
            End If

            Return cMsg

        Catch ex As Exception
            m_eventLogger.WriteEntry(ex.ToString, EventLogEntryType.Error)
        End Try
    End Function

    Private Function AddVersionNumber() As String
        Dim cRet As String = "<br><p align=""right""><FONT face=""Bookman Old Style"" size=""1"">v" & _
                             System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                             "</FONT></p>"
        Return cRet
    End Function

End Class
