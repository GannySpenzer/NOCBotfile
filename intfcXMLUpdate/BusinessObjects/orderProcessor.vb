Imports System
Imports System.Xml
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient

' *** Added Stock Item Information: UOM, Descr, MFG and MFG Item ID - VR - 06/07/2018  ***
' *** code to change Orig Order if Order No is starting with U05    - VR - 07/16/2018  ***

Public Class orderProcessor

    Implements IDisposable

    Public Sub New()

    End Sub

    Private m_logger As SDI.ApplicationLogger.IApplicationLogger = New noAppLogger
    Private myLogger1 As SDI.ApplicationLogger.appLogger = New SDI.ApplicationLogger.appLogger()

    Public Property [Logger]() As SDI.ApplicationLogger.IApplicationLogger
        Get
            Return m_logger
        End Get
        Set(ByVal Value As SDI.ApplicationLogger.IApplicationLogger)
            m_logger = Value
        End Set
    End Property

    Public Property [MyLogger]() As SDI.ApplicationLogger.appLogger
        Get
            Return myLogger1
        End Get
        Set(ByVal Value As SDI.ApplicationLogger.appLogger)
            myLogger1 = Value
        End Set
    End Property

    Private m_requestOrder As order = Nothing

    Public Property [OrderRequest]() As order
        Get
            Return m_requestOrder
        End Get
        Set(ByVal Value As order)
            m_requestOrder = Value
        End Set
    End Property

    '// collection of "order" object type
    '//     that carries SDI order numbers
    Private m_arrAssocOrders As New ArrayList

    Public ReadOnly Property [sdiAssociatedOrders]() As ArrayList
        Get
            If (m_arrAssocOrders Is Nothing) Then
                m_arrAssocOrders = New ArrayList
            End If
            Return m_arrAssocOrders
        End Get
    End Property

    '// container for processMsg object type
    Private m_arrMsgs As New ArrayList

    Public ReadOnly Property ProcessMessages() As ArrayList
        Get
            If (m_arrMsgs Is Nothing) Then
                m_arrMsgs = New ArrayList
            End If
            Return m_arrMsgs
        End Get
    End Property

    Private m_cnStringORA As String = ""
    Private m_cnStringSQL As String = ""

    Public Property [oleConnectionString]() As String
        Get
            Return m_cnStringORA
        End Get
        Set(ByVal Value As String)
            m_cnStringORA = Value
        End Set
    End Property

    Public Property [sqlConnectionString]() As String
        Get
            Return m_cnStringSQL
        End Get
        Set(ByVal Value As String)
            m_cnStringSQL = Value
        End Set
    End Property

    '// collection of type myItem object
    Private m_arrSpecialEmpList As New ArrayList

    Public ReadOnly Property [SpecialEmployeeList]() As ArrayList
        Get
            If (m_arrSpecialEmpList Is Nothing) Then
                m_arrSpecialEmpList = New ArrayList
            End If
            Return m_arrSpecialEmpList
        End Get
    End Property

    Public Function IsPrioritizeOrderBaseOnEmp(ByVal empId As String) As Boolean
        Dim ret As Boolean = False
        If empId.Trim.Length > 0 Then
            For Each emp As myItem In Me.SpecialEmployeeList
                If emp.Id.Trim.ToUpper = empId.Trim.ToUpper Then
                    ret = True
                    Exit For
                End If
            Next
        End If
        Return ret
    End Function

    Public Function ValidateOrderRequest() As Boolean
        Dim bIsValid As Boolean = True

        '// should do all validations and
        '//     enumerate all problems with this order request and list them down!
        '// flag will ONLY going to be set as FALSE for any error encountered!

        ' validate business unit
        If Me.OrderRequest.BusinessUnit.Length = 0 Then
            bIsValid = False
            Me.ProcessMessages.Add(New processMsg(msg:="NO Business Unit Id.", lvl:=TraceLevel.Error))
        End If

        ' validate customer Id
        If (Me.OrderRequest.SiteInfo.CustID Is Nothing) Then
            bIsValid = False
            Me.ProcessMessages.Add(New processMsg(msg:="UNABLE to retrieve Customer Id.", lvl:=TraceLevel.Error))
        ElseIf (Me.OrderRequest.SiteInfo.CustID.Trim.Length = 0) Then
            bIsValid = False
            Me.ProcessMessages.Add(New processMsg(msg:="Retrieved Customer Id is blank.", lvl:=TraceLevel.Error))
        End If
        '' validate order status
        'If (Me.OrderRequest.OrderStatus.Length = 0) Then
        '    bIsValid = False
        '    Me.ProcessMessages.Add(New processMsg(msg:="Order Status is blank.", lvl:=TraceLevel.Error))
        'End If
        ' validate site prefix
        If (Me.OrderRequest.SitePrefix.Length = 0) Then
            bIsValid = False
            Me.ProcessMessages.Add(New processMsg(msg:="Site Prefix is blank.", lvl:=TraceLevel.Error))
        End If

        ' validate order#
        If (Me.OrderRequest.OrderNo_Group.Length = 0) Then
            bIsValid = False
            Me.ProcessMessages.Add(New processMsg(msg:="NO Order Number on this order request.", lvl:=TraceLevel.Error))
        End If

        ' validate employee Id
        If (Me.OrderRequest.EmployeeId.Length = 0) Then
            bIsValid = False
            Me.ProcessMessages.Add(New processMsg(msg:="NO Employee Id on this order request.", lvl:=TraceLevel.Error))
        End If

        ' validate order line(s) - at least 1
        If (Me.OrderRequest.OrderLines.Count < 1) Then
            bIsValid = False
            Me.ProcessMessages.Add(New processMsg(msg:="Retrieved Customer Id is blank.", lvl:=TraceLevel.Error))
        End If

        ' validate ref Ordr NO
        If Me.OrderRequest.InvalidRefNo <> "" Then
            bIsValid = False
            Me.ProcessMessages.Add(New processMsg(Me.OrderRequest.InvalidRefNo, TraceLevel.Error))
        End If

        Return bIsValid
    End Function

    Public Function SaveOrderRequest() As Boolean
        Dim bIsSaved As Boolean = False
        Dim rtn As String = "orderProcessor.SaveOrderRequest"

        Dim cancellableLineStats As String = "12QWBC"
        Dim cnORA As New OleDbConnection(Me.oleConnectionString)

        m_logger.WriteInformationLog(msg:=rtn & "::opening connection - [" & cnORA.ConnectionString & "]")
        cnORA.Open()

        If cnORA.State = ConnectionState.Open Then
            Dim trnsactSession As OleDbTransaction = cnORA.BeginTransaction
            m_logger.WriteInformationLog(msg:=rtn & "::created transaction object")
            Dim sql21 As String = ""
            Dim sOrderNo As String = Me.OrderRequest.OrderNo_Group
            Dim sLineNo As String = ""
            Dim strRefNO As String = ""
            Dim bIsOrigOrderUpdateNeeded As Boolean = False

            Try

                Dim iCnt As Integer = 0
                Dim sdiAssocOrder As order = Nothing
                Dim cmd As OleDbCommand = Nothing
                Dim i As Integer = -1
                m_logger.WriteVerboseLog(msg:=rtn & "::started processing " & Me.OrderRequest.OrderNo_Group & " order request containing " & Me.OrderRequest.OrderLines.Count.ToString & " line(s).")

                For Each itm As orderLine In Me.OrderRequest.OrderLines
                    sql21 = ""
                    sLineNo = itm.OrderLineNo.ToString()
                    If itm.TargetOperation = orderLine.eTargetOperation.AddOrderLine Then

                        '//  [ADD]

                        ' get existing order/order line(s) (if exist)
                        sdiAssocOrder = Me.GetSDIOrderForLine(bu:=itm.ParentOrder.BusinessUnit, _
                                                              grpOrderNo:=itm.ParentOrder.OrderNo_Group, _
                                                              orderLineNo:=itm.OrderLineNo)
                        If (sdiAssocOrder Is Nothing) Then
                            '// order/order line NOT FOUND

                            ' check header Id, if greater than 0, then that means
                            '   that the header had already been saved and that we can use that number 
                            '   for saving our order line(s), generate sdi order # for this add if not yet.... erwin
                            If Not (Me.OrderRequest.Id > 0) Then
                                '// save order header
                                ' generate sdi order # for this add
                                Me.OrderRequest.OrderNo_SDI = Me.GetNextSDIOrderNo(Me.OrderRequest.OrderNo_Group, Me.sdiAssociatedOrders)
                                ' insert
                                ' see if it is a punchin or not - reference number coming from the quote table which means it is an f-en punchin

                                Try
                                    If itm.ReferenceNo.Length > 0 Then
                                        strRefNO = itm.ReferenceNo.Trim.ToString
                                    Else
                                        strRefNO = ""
                                    End If
                                Catch ex As Exception

                                End Try

                                ' code to change Orig Order if Order No is starting with U05 - VR 07/16/2018
                                If Trim(strRefNO) <> "" Then
                                    If Len(strRefNO) > 3 Then
                                        If UCase(Microsoft.VisualBasic.Left(strRefNO, 3)) = "U05" Then
                                            bIsOrigOrderUpdateNeeded = True
                                        End If
                                    End If
                                End If

                                sql21 = buildHeaderINTFCInsertSQL(Me.OrderRequest, strRefNO)
                                cmd = cnORA.CreateCommand
                                cmd.CommandText = sql21
                                cmd.CommandType = CommandType.Text
                                cmd.Transaction = trnsactSession
                                m_logger.WriteVerboseLog(msg:=rtn & "::   executing : " & sql21)
                                i = cmd.ExecuteNonQuery()
                                m_logger.WriteVerboseLog(msg:=rtn & "::   affected record(s) = " & i.ToString)
                                cmd = Nothing
                                ' re-query id
                                sql21 = "" & _
                                      "SELECT A.ORDER_NO " & vbCrLf & _
                                      "FROM SYSADM8.PS_ISA_ORD_INTF_HD A " & vbCrLf & _
                                      "WHERE " & vbCrLf & _
                                      "      A.BUSINESS_UNIT_OM = '" & Me.OrderRequest.BusinessUnit & "' " & vbCrLf & _
                                      "  AND A.ORDER_NO = '" & Me.OrderRequest.OrderNo_SDI & "' " & vbCrLf & _
                                      ""
                                cmd = cnORA.CreateCommand
                                cmd.CommandText = sql21
                                cmd.CommandType = CommandType.Text
                                cmd.Transaction = trnsactSession    ' can't do without the transaction object even on SELECT
                                m_logger.WriteVerboseLog(msg:=rtn & "::  executing : " & sql21)
                                Dim rdr As OleDbDataReader = cmd.ExecuteReader()
                                If Not (rdr Is Nothing) Then
                                    If rdr.Read Then
                                        Me.OrderRequest.Id = CType(rdr("ORDER_NO"), Long)
                                    End If
                                End If
                                rdr.Close()
                                rdr = Nothing
                                cmd = Nothing
                                m_logger.WriteInformationLog(msg:=rtn & "::created order header " & _
                                                             Me.OrderRequest.OrderNo_SDI & "/" & _
                                                             Me.OrderRequest.Id.ToString & " for " & _
                                                             Me.OrderRequest.OrderNo_Group & ".")
                            End If

                            ' save order line
                            sql21 = buildLineINTFCInsertSQL(Me.OrderRequest, itm)
                            cmd = cnORA.CreateCommand
                            cmd.CommandText = sql21
                            cmd.CommandType = CommandType.Text
                            cmd.Transaction = trnsactSession
                            m_logger.WriteVerboseLog(msg:=rtn & "::   executing : " & sql21)
                            i = cmd.ExecuteNonQuery
                            m_logger.WriteVerboseLog(msg:=rtn & "::   affected record(s) = " & i.ToString)
                            cmd = Nothing
                            m_logger.WriteInformationLog(msg:=rtn & "::   saved order line " & _
                                                         itm.OrderLineNo & " for " & _
                                                         itm.ParentOrder.OrderNo_SDI & " order request.")

                        Else
                            ' order/order line already EXISTS
                            ' ... WARNING that this order line was already existing for this order (group)
                            Dim nLnId As Integer = -1
                            For Each o1 As orderLine In sdiAssocOrder.OrderLines
                                If o1.OrderLineNo = itm.OrderLineNo Then
                                    nLnId = o1.Id
                                    Exit For
                                End If
                            Next
                            m_logger.WriteWarningLog(msg:=rtn & "::   order line " & _
                                                     itm.OrderLineNo.ToString & "/" & nLnId.ToString & " for " & _
                                                     itm.ParentOrder.OrderNo_SDI & " already exists and CANNOT BE ADDED.")
                        End If

                        'set user activity for this employee: orderReqLine.EmployeeId.Trim.ToUpper
                        Dim strEmplIdM1 As String = ""
                        If iCnt = 0 Then
                            Try
                                If itm.EmployeeId.Trim.ToUpper <> "" Then
                                    strEmplIdM1 = itm.EmployeeId.Trim.ToUpper
                                    SetLastActivityDate(strEmplIdM1, "I0256", Me.oleConnectionString)
                                End If
                            Catch ex As Exception

                            End Try
                        End If
                        
                        iCnt = iCnt + 1

                    ElseIf itm.TargetOperation = orderLine.eTargetOperation.CancelOrderLine Then

                        '// [CANCEL]

                        ' get existing order/order line (if exist)
                        sdiAssocOrder = Me.GetSDIOrderForLine(bu:=itm.ParentOrder.BusinessUnit, _
                                                              grpOrderNo:=itm.ParentOrder.OrderNo_Group, _
                                                              orderLineNo:=itm.OrderLineNo)
                        Dim sdiAssocOrderLine As orderLine = Nothing
                        If Not (sdiAssocOrder Is Nothing) Then
                            For Each lne As orderLine In sdiAssocOrder.OrderLines
                                If lne.OrderLineNo = itm.OrderLineNo Then
                                    sdiAssocOrderLine = lne
                                End If
                            Next
                        End If
                        If (sdiAssocOrder Is Nothing) Then
                            ' order/order line NOT FOUND
                            ' ... WARNING that this order line is not existing and cannot be cancelled
                            m_logger.WriteWarningLog(msg:=rtn & "::   order line " & _
                                                     itm.OrderLineNo.ToString & " for " & _
                                                     itm.ParentOrder.OrderNo_Group & " NOT FOUND - CANNOT CANCEL ORDER LINE.")
                        Else
                            ' order/order line already EXISTS
                            '... check/cancel line
                            If Not (sdiAssocOrderLine Is Nothing) Then
                                If cancellableLineStats.IndexOf(sdiAssocOrderLine.OrderLineStatus) > -1 Then
                                    '// can ONLY cancel existing order line IF
                                    '//     1 - Submitted; 2 - Processing Order; Q - Waiting Quote; W - Waiting Order Approval
                                    '//     B - Waiting Budget Approval; C - Cancelled
                                    ' line
                                    sql21 = "" & _
                                          "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN " & vbCrLf & _
                                          "SET " & vbCrLf & _
                                          " ISA_LINE_STATUS = 'QTR' " & vbCrLf & _
                                          ",QTY_REQUESTED = 0 " & vbCrLf & _
                                          ",LASTUPDDTTM = TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                                          "WHERE " & vbCrLf & _
                                          "      BUSINESS_UNIT_OM = '" & sdiAssocOrder.BusinessUnit.ToString & "' " & vbCrLf & _
                                          "   AND   ORDER_NO = '" & sdiAssocOrder.Id.ToString & "' " & vbCrLf & _
                                          "  AND ISA_INTFC_LN = '" & sdiAssocOrderLine.OrderLineNo.ToString & "' " & vbCrLf & _
                                          ""
                                    cmd = cnORA.CreateCommand
                                    cmd.CommandText = sql21
                                    cmd.CommandType = CommandType.Text
                                    cmd.Transaction = trnsactSession
                                    m_logger.WriteVerboseLog(msg:=rtn & "::executing : " & sql21)
                                    i = cmd.ExecuteNonQuery()
                                    m_logger.WriteVerboseLog(msg:=rtn & "::affected record(s) = " & i.ToString)
                                    cmd = Nothing
                                    ' header status
                                    sql21 = "" & _
                                          "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD " & vbCrLf & _
                                          "SET " & vbCrLf & _
                                          "ORDER_STATUS  = 'P' " & vbCrLf & _
                                          ",LASTUPDDTTM = TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                                          "WHERE " & vbCrLf & _
                                          "      BUSINESS_UNIT_OM = '" & sdiAssocOrder.BusinessUnit.ToString & "' " & vbCrLf & _
                                          "   AND   ORDER_NO = '" & sdiAssocOrder.Id.ToString & "' " & vbCrLf & _
                                          ""
                                    cmd = cnORA.CreateCommand
                                    cmd.CommandText = sql21
                                    cmd.CommandType = CommandType.Text
                                    cmd.Transaction = trnsactSession
                                    m_logger.WriteVerboseLog(msg:=rtn & "::executing : " & sql21)
                                    i = cmd.ExecuteNonQuery()
                                    m_logger.WriteVerboseLog(msg:=rtn & "::affected record(s) = " & i.ToString)
                                    cmd = Nothing
                                    ' log
                                    m_logger.WriteInformationLog(msg:=rtn & "::   order line #" & _
                                                                 itm.OrderLineNo.ToString & "(" & sdiAssocOrderLine.OrderLineNo.ToString & ") for " & _
                                                                 sdiAssocOrder.OrderNo_SDI & "(" & sdiAssocOrder.Id.ToString & ")/" & itm.ParentOrder.OrderNo_Group & _
                                                                 " flagged as CANCELLED.")
                                Else
                                    ' order/order line WAS FOUND but CANNOT CANCEL at this point
                                    ' ... WARNING that this order line's state CANNOT be cancelled
                                    m_logger.WriteWarningLog(msg:=rtn & "::   order line " & _
                                                             itm.OrderLineNo.ToString & " for " & _
                                                             sdiAssocOrder.OrderNo_SDI & "/" & itm.ParentOrder.OrderNo_Group & _
                                                             " CANNOT BE CANCELLED AT THIS TIME - status=" & sdiAssocOrderLine.OrderLineStatus & ".")
                                End If
                            Else
                                ' order line NOT FOUND ???
                                m_logger.WriteWarningLog(msg:=rtn & "::   order line " & _
                                                         itm.OrderLineNo.ToString & " for " & _
                                                         itm.ParentOrder.OrderNo_Group & " NOT FOUND - CANNOT CANCEL ORDER LINE.")
                            End If
                        End If

                    End If

                Next

                trnsactSession.Commit()
                bIsSaved = True
                m_logger.WriteInformationLog(msg:=rtn & "::transaction committed.")

                'code to change Orig Order if Order No is starting with U05
                If bIsOrigOrderUpdateNeeded Then
                    If Not cnORA.State = ConnectionState.Open Then
                        cnORA.Open()
                    End If
                    ' change orig order header status to space
                    Dim iMe As Integer = 0
                    Dim strSqlStr32 As String = "UPDATE SYSADM8.PS_ISA_ORD_INTF_HD SET ORDER_STATUS = ' ' WHERE BUSINESS_UNIT_OM = 'I0256' AND ORDER_NO = '" & strRefNO & "'"
                    cmd = cnORA.CreateCommand
                    cmd.CommandText = strSqlStr32
                    cmd.CommandType = CommandType.Text
                    Try
                        iMe = cmd.ExecuteNonQuery
                    Catch ex32 As Exception

                    End Try
                    cmd = Nothing
                    ' change orig order line status to space
                    iMe = 0
                    strSqlStr32 = "UPDATE SYSADM8.PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = ' ' WHERE BUSINESS_UNIT_OM = 'I0256' AND ORDER_NO = '" & strRefNO & "'"
                    cmd = cnORA.CreateCommand
                    cmd.CommandText = strSqlStr32
                    cmd.CommandType = CommandType.Text
                    Try
                        iMe = cmd.ExecuteNonQuery
                    Catch ex32 As Exception

                    End Try
                    cmd = Nothing
                End If  '  If bIsOrigOrderUpdateNeeded Then

                sdiAssocOrder = Nothing

            Catch ex As Exception
                ' Something blew up!
                m_logger.WriteErrorLog(msg:=rtn & "::" & ex.ToString)
                'to do: save erred SQL to a new text file
                If Trim(sql21) <> "" Then
                    myLogger1.WriteErrorLog(rtn & ":: Rollback! Order Group No: " & sOrderNo & " :: Order Line No: " & sLineNo & " :: SQL: " & sql21)
                End If
                Me.ProcessMessages.Add(New processMsg(msg:=ex.tostring, lvl:=TraceLevel.Error))
                Try
                    ' rollback!!!
                    m_logger.WriteErrorLog(msg:=rtn & "::rolling back transactions.")
                    trnsactSession.Rollback()
                Catch exRollBack As Exception
                End Try
            End Try
            trnsactSession = Nothing
        Else
            ' cannot open ORA connection!
            m_logger.WriteErrorLog(msg:=rtn & "::CANNOT OPEN CONNECTION.")
            Me.ProcessMessages.Add(New processMsg(msg:="CANNOT OPEN CONNECTION", lvl:=TraceLevel.Error))
        End If

        m_logger.WriteVerboseLog(msg:=rtn & "::closing and disposing connection object.")
        Try
            cnORA.Close()
            cnORA.Dispose()
        Catch ex As Exception
        Finally
            cnORA = Nothing
        End Try

        Return bIsSaved
    End Function

    Public Sub SetLastActivityDate(ByVal sUserID As String, ByVal sBU As String, ByVal strConnString As String, Optional ByVal bIsMultiSite As Boolean = False)
        Dim strSQLstring As String = ""
        Dim cmd As OleDbCommand = Nothing

        If Not bIsMultiSite Then
            strSQLstring = "UPDATE SDIX_USERS_TBL " & vbCrLf & _
                " SET last_activity = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                " WHERE isa_employee_id = '" & sUserID & "' " & vbCrLf & _
                " AND business_unit = '" & sBU & "' "
        Else
            strSQLstring = "UPDATE SDIX_USERS_TBL " & vbCrLf & _
            " SET last_activity = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
            " WHERE isa_employee_id = '" & sUserID & "' " & vbCrLf
        End If

        Dim rowsaffected As Integer = 0
        Dim cnORA As New OleDbConnection(strConnString)

        cnORA.Open()
        Try
            cmd = cnORA.CreateCommand
            cmd.CommandText = strSQLstring
            cmd.CommandType = CommandType.Text
            rowsaffected = cmd.ExecuteNonQuery()

            cnORA.Close()
        Catch ex21 As Exception
            Try
                cnORA.Close()
            Catch ex As Exception

            End Try
        End Try

    End Sub



#Region " IDisposable Implementation "

    Public Sub Dispose() Implements System.IDisposable.Dispose
        Try
            m_requestOrder.Dispose()
        Catch ex As Exception
        Finally
            m_requestOrder = Nothing
        End Try
    End Sub

#End Region

    Public Overloads Sub ReadOrderRequest(ByVal orderReqXMLString As String, Optional ByVal sOraConnString As String = "")
        Dim doc As New XmlDocument
        doc.LoadXml(orderReqXMLString)
        ParseOrderRequest(doc, sOraConnString)
    End Sub

    Public Overloads Sub ReadOrderRequest(ByVal orderReqXML As XmlDocument, Optional ByVal sOraConnString As String = "")
        ParseOrderRequest(orderReqXML, sOraConnString)
    End Sub

    Private Sub ParseOrderRequest(ByVal orderReqXML As XmlDocument, Optional ByVal sOraConnString As String = "")
        Dim connectOR As New OleDbConnection()
        Try
            If Trim(sOraConnString) <> "" Then
                connectOR.ConnectionString = sOraConnString
            Else
                connectOR.ConnectionString = "Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR"
            End If
        Catch ex As Exception
            connectOR.ConnectionString = "Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR"
        End Try
        
        Dim rtn As String = "orderProcessor.ParseOrderRequest"
        If Not (orderReqXML Is Nothing) Then

            Me.OrderRequest = New order

            '// this routine expects an XML in the following format ... not a very good format for parent/child
            '// <DATA>
            '//     <PS_ISA_ORD_INTFC_H></PS_ISA_ORD_INTFC_H>
            '//     <PS_ISA_ORD_INTFC_L></PS_ISA_ORD_INTFC_L>
            '//     <PS_ISA_ORD_INTFC_L></PS_ISA_ORD_INTFC_L>
            '// </DATA>

            Dim nodeData As XmlNode = orderReqXML.SelectSingleNode("/DATA")

            If Not (nodeData Is Nothing) Then
                If nodeData.ChildNodes.Count > 0 Then
                    Dim nHdr As Integer = 0
                    For Each itm As XmlNode In nodeData.ChildNodes
                        If itm.NodeType = XmlNodeType.Element And itm.Name.Trim.ToUpper = "PS_ISA_ORD_INTFC_H" Then

                            ' check for multiple header
                            nHdr += 1
                            If nHdr > 1 Then
                                ' this will throw an exception
                                '   and will abort loop
                                Throw New ApplicationException(message:=rtn & "::order request XML carries more than a single header element.")
                            End If

                            ' had to browse through each node of the "header" to "upperCase" them for comparison

                            If itm.ChildNodes.Count > 0 Then
                                For Each hdrElem As XmlNode In itm.ChildNodes
                                    If hdrElem.NodeType = XmlNodeType.Element Then
                                        Select Case hdrElem.Name.Trim.ToUpper
                                            Case "BUSINESS_UNIT"
                                                Try
                                                    Me.OrderRequest.BusinessUnit = hdrElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                            Case "PRIORITY"
                                                Try
                                                    Me.OrderRequest.PriorityCode = hdrElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                            Case "ORDER_NO"
                                                Me.OrderRequest.OrderNo_Group = hdrElem.InnerText.Trim
                                                Me.OrderRequest.OrderNo_SDI = hdrElem.InnerText.Trim
                                            Case "LOCATION"
                                                ' the header location seems to be something for the line note field
                                                Try
                                                    Me.OrderRequest.Location = stringEncoder.escapeString(hdrElem.InnerText.Trim)
                                                Catch ex As Exception
                                                End Try
                                            Case "ORDER_STATUS"
                                                '  Me.OrderRequest.OrderStatus = hdrElem.InnerText.Trim
                                                Me.OrderRequest.TargetOperation = orderLine.GetTargetOperation(opId:=hdrElem.InnerText.Trim)

                                            Case "ORDER_DATE"
                                                Try
                                                    Me.OrderRequest.OrderDate = hdrElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                            Case "EMPLID"
                                                Try
                                                    Me.OrderRequest.EmployeeId = hdrElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                        End Select
                                    End If
                                Next
                            End If

                        ElseIf itm.NodeType = XmlNodeType.Element And itm.Name.Trim.ToUpper = "PS_ISA_ORD_INTFC_L" Then

                            If itm.ChildNodes.Count > 0 Then
                                'this is where the lines are created
                                Dim lne As orderLine = Me.OrderRequest.CreateOrderLine(orderLineNo:=-1)

                                For Each lneElem As XmlNode In itm.ChildNodes
                                    If lneElem.NodeType = XmlNodeType.Element Then
                                        Try
                                            connectOR.Close()
                                        Catch ex As Exception

                                        End Try
                                        Select Case lneElem.Name.Trim.ToUpper
                                            Case "REFERENCE_NUMBER"
                                                Try
                                                    lne.ReferenceNo = (lneElem.InnerText.Trim)
                                                Catch ex As Exception
                                                End Try
                                            Case "REFERENCE_LINE_NUMBER"
                                                Try
                                                    lne.ReferenceNoLine = (lneElem.InnerText.Trim)
                                                Catch ex As Exception

                                                End Try
                                            Case "SHIPTO_ID"
                                                Try
                                                    If lne.ReferenceNo.Length > 0 Then
                                                        connectOR.Open()
                                                        Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)
                                                        lne.ShipToId = objquoteRn.SHIPTO_ID
                                                        If Trim(lne.ShipToId) = "" Then
                                                            lne.ShipToId = (lneElem.InnerText.Trim)
                                                        Else
                                                            lne.ShipToId = stringEncoder.escapeString(lne.ShipToId)
                                                        End If
                                                    Else
                                                        lne.ShipToId = (lneElem.InnerText.Trim)
                                                    End If

                                                Catch ex As Exception
                                                    lne.ShipToId = (lneElem.InnerText.Trim)
                                                End Try

                                            Case "LINE_NBR"
                                                Try
                                                    lne.OrderLineNo = CInt(lneElem.InnerText.Trim)
                                                Catch ex As Exception
                                                End Try
                                            Case "BUSINESS_UNIT"
                                                lne.BusinessUnit = lneElem.InnerText.Trim
                                                ' check if header DOES NOT CARRY A BUSINESS_UNIT
                                                '   take this line's business unit
                                                If (Me.OrderRequest.BusinessUnit.Trim.Length = 0) And _
                                                   (lne.BusinessUnit.Trim.Length > 0) Then
                                                    Me.OrderRequest.BusinessUnit = lne.BusinessUnit
                                                End If
                                            Case "PRIORITY"
                                                ' on the header, but check for overrides
                                                Try
                                                    lne.PriorityCode = lneElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                                ' check if header DOES NOT CARRY A PRIORITY CODE
                                                '   take this line's priority code
                                                If (Me.OrderRequest.PriorityCode.Trim.Length = 0) And _
                                                   (lne.PriorityCode.Trim.Length > 0) Then
                                                    Me.OrderRequest.PriorityCode = lne.PriorityCode
                                                End If
                                            Case "LOCATION"
                                                ' header have location field BUT for order line notes field
                                                '   BobD seems to be expecting it from this element on the "L" node
                                                Try
                                                    lne.Location = lneElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                            Case "ORDER_STATUS"
                                                ' on the header, but check for overrides
                                                lne.OrderLineStatus = (lneElem.InnerText.Trim)
                                                Try
                                                    If lne.ReferenceNo.Length > 0 Then
                                                        lne.OrderLineStatus = "P"
                                                        Me.OrderRequest.OrderStatus = "P"
                                                        
                                                    End If
                                                Catch ex As Exception

                                                End Try
                                                
                                            Case "ORDER_DATE"
                                                ' on the header, but check for overrides
                                                'Try
                                                '    lne.OrderDate = lneElem.InnerText.Trim
                                                'Catch ex As Exception
                                                'End Try
                                            Case "INV_ITEM_ID"
                                                Try
                                                    lne.InventoryItemId = lneElem.InnerText.Trim.ToUpper
                                                Catch ex As Exception
                                                End Try
                                            Case "MFG_ID"
                                                lne.ManufacturerId = (lneElem.InnerText.Trim)
                                                Try
                                                    If lne.ReferenceNo.Length > 0 Then

                                                        connectOR.Open()
                                                        Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)
                                                        'stringEncoder.escapeString(lneElem.InnerText.Trim)
                                                        lne.ManufacturerId = objquoteRn.MFG_ID
                                                        If Trim(lne.ManufacturerId) = "" Then
                                                            lne.ManufacturerId = (lneElem.InnerText.Trim)
                                                        Else
                                                            lne.ManufacturerId = stringEncoder.escapeString(lne.ManufacturerId)
                                                        End If

                                                    End If
                                                Catch ex As Exception
                                                    lne.ManufacturerId = (lneElem.InnerText.Trim)
                                                End Try


                                            Case "MFG_ITM_ID"
                                                lne.ManufacturerItemId = (lneElem.InnerText.Trim)
                                                Try
                                                    If lne.ReferenceNo.Length > 0 Then

                                                        connectOR.Open()
                                                        Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)
                                                        lne.ManufacturerItemId = objquoteRn.MFG_ITM_ID
                                                        If Trim(lne.ManufacturerItemId) = "" Then
                                                            lne.ManufacturerItemId = (lneElem.InnerText.Trim)
                                                        Else
                                                            lne.ManufacturerItemId = stringEncoder.escapeString(lne.ManufacturerItemId)
                                                        End If

                                                    End If
                                                Catch ex As Exception
                                                    lne.ManufacturerItemId = (lneElem.InnerText.Trim)
                                                End Try

                                                'Case "VNDR_CATALOG_ID"
                                                '    Try
                                                '        If lne.ReferenceNo.Length > 0 Then
                                                '            connectOR.Open()
                                                '            Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)
                                                '            lne.VNDR_CATALOG_ID = objquoteRn.VNDR_CATALOG_ID
                                                '            connectOR.Close()
                                                '        Else
                                                '            lne.VNDR_CATALOG_ID =  (lneElem.InnerText.Trim)
                                                '        End If
                                                '    Catch ex As Exception

                                                '    End Try

                                            Case "ITM_ID_VNDR"
                                                Try
                                                    lne.ITM_ID_VNDR = (lneElem.InnerText.Trim)
                                                    lne.ITM_ID_VNDR_AUX = " "
                                                    If lne.ReferenceNo.Length > 0 Then

                                                        connectOR.Open()
                                                        Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)
                                                        lne.ITM_ID_VNDR = objquoteRn.ITM_ID_VNDR
                                                        lne.ITM_ID_VNDR_AUX = objquoteRn.ITM_ID_VNDR_AUX
                                                        lne.Location = objquoteRn.VNDR_LOC
                                                        If Trim(lne.ITM_ID_VNDR) = "" Then
                                                            lne.ITM_ID_VNDR = (lneElem.InnerText.Trim)
                                                        End If
                                                        If Trim(lne.ITM_ID_VNDR_AUX) = "" Then
                                                            lne.ITM_ID_VNDR_AUX = " "
                                                        End If
                                                        
                                                    End If
                                                Catch ex As Exception
                                                    lne.ITM_ID_VNDR = (lneElem.InnerText.Trim)
                                                    lne.ITM_ID_VNDR_AUX = " "
                                                End Try

                                            Case "QTY"
                                                Dim strrefno As String = " "
                                                lne.Quantity = CDbl(lneElem.InnerText.Trim)
                                                If lne.ReferenceNo.Length > 0 Then
                                                    Try

                                                        connectOR.Open()
                                                        Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)
                                                        lne.Quantity = objquoteRn.QTY_REQ
                                                        If Trim(lne.Quantity) = "" Then
                                                            lne.Quantity = (lneElem.InnerText.Trim)
                                                        End If
                                                        Me.OrderRequest.InvalidRefNo = objquoteRn.ISA_Invalid_Ref_NO
                                                        If Trim(objquoteRn.NET_UNIT_PRICE) <> "" Then
                                                            If IsNumeric(objquoteRn.NET_UNIT_PRICE) Then
                                                                lne.NetUnitPrice = CDbl(objquoteRn.NET_UNIT_PRICE)
                                                            End If
                                                        End If
                                                        If Trim(objquoteRn.Net_PRICE_PO) <> "" Then
                                                            If IsNumeric(objquoteRn.Net_PRICE_PO) Then
                                                                lne.NetPOPrice = CDbl(objquoteRn.Net_PRICE_PO)
                                                            End If
                                                        End If
                                                    Catch ex As Exception
                                                        lne.Quantity = (lneElem.InnerText.Trim)
                                                    End Try
                                                    
                                                End If

                                            Case "VENDOR_ID"

                                                lne.Vendor_id = (lneElem.InnerText.Trim)
                                                If lne.ReferenceNo.Length > 0 Then
                                                    Try

                                                        connectOR.Open()
                                                        Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)
                                                        lne.Vendor_id = objquoteRn.Vendor_ID
                                                        Me.OrderRequest.InvalidRefNo = objquoteRn.ISA_Invalid_Ref_NO
                                                        If Trim(lne.Vendor_id) = "" Then
                                                            lne.Vendor_id = (lneElem.InnerText.Trim)
                                                        End If
                                                    Catch ex As Exception
                                                        lne.Vendor_id = (lneElem.InnerText.Trim)
                                                    End Try
                                                    
                                                End If
                                                'see if this xml has a reference number
                                                'if so get the pricing info from the quote table 
                                                'by this reference numbe line
                                                '
                                            Case "DUE_DATE"
                                                'see if this xml has a reference number
                                                'if so get the pricing info from the quote table 
                                                'by this reference numbe line
                                                '
                                                lne.DueDate = (lneElem.InnerText.Trim)
                                                If lne.ReferenceNo.Length > 0 Then
                                                    Try

                                                        connectOR.Open()
                                                        Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)

                                                        If Trim(objquoteRn.ISA_REQUIRED_BY_DT) <> "" Then
                                                            If IsDate(objquoteRn.ISA_REQUIRED_BY_DT) Then
                                                                lne.DueDate = CDate(objquoteRn.ISA_REQUIRED_BY_DT)
                                                            End If
                                                        End If
                                                    Catch ex As Exception
                                                        lne.DueDate = (lneElem.InnerText.Trim)
                                                    End Try
                                                    
                                                End If

                                            Case "EMPLID"
                                                lne.EmployeeId = lneElem.InnerText.Trim
                                                ' check if header DOES NOT CARRY A EMPLID
                                                '   take this line's EMPLID
                                                'pfd


                                                If (Me.OrderRequest.EmployeeId.Trim.Length = 0) And _
                                                   (lne.EmployeeId.Trim.Length > 0) Then
                                                    'Me.OrderRequest.EmployeeId = lne.EmployeeId.Substring(0, 8)
                                                    Me.OrderRequest.EmployeeId = lne.EmployeeId
                                                    If Me.OrderRequest.EmployeeId.Length > 8 Then
                                                        Me.OrderRequest.EmployeeId = Me.OrderRequest.EmployeeId.Substring(0, 8)
                                                    End If

                                                End If

                                            Case "NET_UNIT_PRICE"

                                                lne.NetUnitPrice = CDbl(lneElem.InnerText.Trim)
                                                lne.NetPOPrice = CDbl(lneElem.InnerText.Trim)
                                                Dim strrefno As String = " "
                                                If lne.ReferenceNo.Length > 0 Then
                                                    Try
                                                        connectOR.Open()

                                                        Dim objquoteRn As New clsQuote(lne.ReferenceNo, lne.ReferenceNoLine, connectOR)

                                                        Me.OrderRequest.InvalidRefNo = objquoteRn.ISA_Invalid_Ref_NO
                                                        If Trim(objquoteRn.NET_UNIT_PRICE) <> "" Then
                                                            If IsNumeric(objquoteRn.NET_UNIT_PRICE) Then
                                                                lne.NetUnitPrice = CDbl(objquoteRn.NET_UNIT_PRICE)
                                                            End If
                                                        End If
                                                        If Trim(objquoteRn.Net_PRICE_PO) <> "" Then
                                                            If IsNumeric(objquoteRn.Net_PRICE_PO) Then
                                                                lne.NetPOPrice = CDbl(objquoteRn.Net_PRICE_PO)
                                                            End If
                                                        End If
                                                    Catch ex As Exception
                                                        lne.NetUnitPrice = CDbl(lneElem.InnerText.Trim)
                                                        lne.NetPOPrice = CDbl(lneElem.InnerText.Trim)
                                                    End Try
                                               
                                                End If
                                            Case "EXTENDED_AMT"

                                                lne.ExtendedAmount = CDbl(lneElem.InnerText.Trim)
                                                
                                            Case "ISA_WORK_ORDER_NO"
                                                Try
                                                    lne.WorkOrderNo = lneElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                                'Case "SHIPTO_ID"
                                                '    Try
                                                '        lne.ShipToId = lneElem.InnerText.Trim
                                                '    Catch ex As Exception
                                                '    End Try
                                            Case "ISA_CUST_CHARGE_CD"
                                                Try
                                                    lne.CustomerChargeCode = lneElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                                If lne.CustomerChargeCode.Length = 0 Then
                                                    lne.CustomerChargeCode = "USE WORK ORDER"
                                                End If
                                            Case "ISA_ACCOUNT_CODE"
                                                Try
                                                    lne.Accountcode = lneElem.InnerText.Trim
                                                Catch ex As Exception
                                                End Try
                                            Case "COST"
                                                
                                                Try
                                                    lne.Cost = CDbl(lneElem.InnerText.Trim)
                                                Catch ex As Exception
                                                End Try
                                            Case "UNIT_OF_MEASURE"
                                                
                                                lne.UOM = lneElem.InnerText.Trim.ToUpper
                                            Case "DESCR_254MIXED"
                                                Try
                                                    lne.InventoryItemDescription = stringEncoder.escapeString(lneElem.InnerText.Trim)
                                                Catch ex As Exception
                                                End Try


                                        End Select
                                    End If
                                Next  '  For Each lneElem As XmlNode In itm.ChildNodes

                                ' add order line
                                '   don't care if multiple/dupe orderLine #
                                If lne.OrderLineNo > 0 Then
                                    Me.OrderRequest.OrderLines.Add(lne)
                                End If
                                lne = Nothing
                            End If

                        End If
                    Next   '  For Each itm As XmlNode In nodeData.ChildNodes  - header and line nodes
                End If
            End If
            m_logger.WriteVerboseLog(msg:=rtn & "::parsed XML order request.")

            '// get site prefix
            Me.OrderRequest.SitePrefix = Me.GetSitePrefix(bu:=Me.OrderRequest.BusinessUnit, _
                                                          cnString:=Me.oleConnectionString)
            m_logger.WriteVerboseLog(msg:=rtn & "::retrieved site prefix - " & Me.OrderRequest.SitePrefix & ".")

            '// get site info (clsEnterprise) based on business unit
            Dim cnORA As New OleDbConnection(Me.oleConnectionString)
            cnORA.Open()
            If cnORA.State = ConnectionState.Open Then
                Me.OrderRequest.SiteInfo = New clsEnterprise(Me.OrderRequest.BusinessUnit, cnORA)
            End If
            Try
                cnORA.Close()
                cnORA.Dispose()
            Catch ex As Exception
            Finally
                cnORA = Nothing
            End Try
            m_logger.WriteVerboseLog(msg:=rtn & "::retrieved site information.")

            '// load "special" employee list
            m_arrSpecialEmpList = LoadSpecialEmployeeList()
            m_logger.WriteVerboseLog(msg:=rtn & "::loaded special employee list that order gets prioritized.")

            Dim o As orderLine = Nothing

            '// check/get priority code for each order line
            '//     if priority flag is not set, check if employee Id is on "special" list
            For Each o In Me.OrderRequest.OrderLines
                If o.PriorityCode.Trim.Length = 0 Then
                    o.PriorityCode = "0"
                    If Me.IsPrioritizeOrderBaseOnEmp(o.EmployeeId) Then
                        o.PriorityCode = "1"  '  "Y"
                    End If
                End If
            Next

            '// check if ANY of the order line was flagged as "priority",
            '//     this order will be prioritized
            If Not (Me.OrderRequest.PriorityCode.Trim.ToUpper = "Y") Then
                For Each o In Me.OrderRequest.OrderLines
                    If o.PriorityCode.Trim.ToUpper = "1" Then
                        Me.OrderRequest.PriorityCode = "Y"
                        Exit For
                    End If
                Next
            End If
            m_logger.WriteVerboseLog(msg:=rtn & "::checked/adjusted priority level of this order request.")

            '// check for any order line that does not have any line no
            '//     assign if none ... start at "100"
            For i As Integer = 0 To (Me.OrderRequest.OrderLines.Count - 1)
                o = CType(Me.OrderRequest.OrderLines(i), orderLine)
                If o.OrderLineNo = -1 Then
                    o.OrderLineNo = (i + 100)
                End If
                o = Nothing
            Next
            m_logger.WriteVerboseLog(msg:=rtn & "::checked/adjusted any order line that doesn't carry a line no code.")

            '// check/prefix each order line's item id (if necessary)
            For Each o In Me.OrderRequest.OrderLines
                If o.InventoryItemId.Length > 0 Then
                    If o.InventoryItemId = "NON-STOCK-PART" Then
                        o.InventoryItemId = ""    'blank-out, we'll handle on saving
                    Else
                        Dim bIsPrefixed As Boolean = False
                        Try
                            bIsPrefixed = (o.InventoryItemId.Substring(0, 3) = Me.OrderRequest.SitePrefix)
                        Catch ex As Exception
                        End Try
                        If Not bIsPrefixed Then
                            ' prefix item Id if not yet
                            o.InventoryItemId = Me.OrderRequest.SitePrefix & o.InventoryItemId
                        End If
                    End If
                End If
            Next
            m_logger.WriteVerboseLog(msg:=rtn & "::checked/adjusted/pre-fixed order line item Id with site prefix (for any stock item request).")

            '// load existing associated sdi orders (if any)
            m_arrAssocOrders = LoadAssociatedSDIOrders(Me.OrderRequest.BusinessUnit, _
                                                       Me.OrderRequest.OrderNo_Group, _
                                                       Me.oleConnectionString)
            m_logger.WriteVerboseLog(msg:=rtn & "::loaded (if any) associated/existing order/order line for this request.")
            If m_arrAssocOrders.Count > 0 Then
                'pink
                For Each ord As order In m_arrAssocOrders
                    m_logger.WriteVerboseLog(msg:=rtn & "::order # " & ord.OrderNo_SDI & "(" & ord.Id.ToString & ")/" & ord.OrderNo_Group & " retrieved; status=" & ord.OrderStatus & ".")
                    For Each i As orderLine In ord.OrderLines
                        m_logger.WriteVerboseLog(msg:=rtn & "::   with order line # " & i.OrderLineNo.ToString & "(" & i.Id.ToString & "); status=" & i.OrderLineStatus & ".")
                    Next
                Next
            Else
                m_logger.WriteVerboseLog(msg:=rtn & "::no associated SDI order/order line exists for this request - " & Me.OrderRequest.OrderNo_Group & ".")
            End If

        End If
    End Sub

    Private Function GetSitePrefix(ByVal bu As String, ByVal cnString As String) As String

        Dim rtn As String = "orderProcessor.GetSitePrefix"
        Dim sitePrefix As String = ""

        Dim cn As New OleDbConnection(connectionString:=cnString)

        cn.Open()

        If cn.State = ConnectionState.Open Then
            'Dim sql As String = "" & _
            '                    "SELECT ISA_SITE_CODE " & vbCrLf & _
            '                    "FROM PS_BUS_UNIT_TBL_OM " & vbCrLf & _
            '                    "WHERE BUSINESS_UNIT = '" & bu & "' " & vbCrLf & _
            '                    ""
            Dim sql As String = "" & _
                                "SELECT ISA_CUST_PREFIX " & vbCrLf & _
                                "FROM SYSADM8.PS_ISA_ENTERPRISE " & vbCrLf & _
                                "WHERE ISA_BUSINESS_UNIT = '" & bu & "' " & vbCrLf & _
                                ""
            Dim cmd As OleDbCommand = cn.CreateCommand
            cmd.CommandText = sql
            cmd.CommandType = CommandType.Text
            Dim rdr As OleDbDataReader = cmd.ExecuteReader
            If Not (rdr Is Nothing) Then
                If rdr.Read Then
                    sitePrefix = CStr(rdr("ISA_CUST_PREFIX"))
                End If
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
        End If

        Try
            cn.Close()
            cn.Dispose()
        Catch ex As Exception
        Finally
            cn = Nothing
        End Try

        Return sitePrefix
    End Function

    Private Function LoadSpecialEmployeeList() As ArrayList
        Dim arr As New ArrayList

        ' always get the folder for the executing assembly and
        '   use the same folder as default location for our log file
        Dim cfg As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & _
                            "\PriorityCust.xml"

        Dim doc As XmlDocument = Nothing

        If System.IO.File.Exists(path:=cfg) Then
            doc = New XmlDocument
            doc.Load(filename:=cfg)
        End If

        If Not (doc Is Nothing) Then

            '// expects an XML format like ...
            '// <xmlData>
            '// 	<DefaultPriority>
            '//		    <EMPID>JLCONN</EMPID>
            '//		    <EMPNAME>CONN,JOHN L.</EMPNAME>
            '// 	<DefaultPriority>
            '// 	...
            '// </xmlData>

            Dim nodeData As XmlNode = doc.SelectSingleNode("/xmlData")

            If Not (nodeData Is Nothing) Then
                If nodeData.ChildNodes.Count > 0 Then
                    Dim sId As String = ""
                    Dim sName As String = ""
                    For Each node As XmlNode In nodeData.ChildNodes
                        If node.NodeType = XmlNodeType.Element And _
                           node.Name.Trim.ToUpper = "DEFAULTPRIORITY" Then
                            sId = ""
                            Try
                                sId = node.SelectSingleNode("EMPID").InnerText.Trim.ToUpper
                            Catch ex As Exception
                            End Try
                            sName = ""
                            Try
                                sName = node.SelectSingleNode("EMPNAME").InnerText.Trim
                            Catch ex As Exception
                            End Try
                            If sId.Length > 0 Then
                                Dim emp As myItem = New myItem(id:=sId, name:=sName)
                                arr.Add(emp)
                            End If
                        End If
                    Next
                End If
            End If

            nodeData = Nothing

        End If

        doc = Nothing

        Return arr
    End Function

    Private Function LoadAssociatedSDIOrders(ByVal bu As String, ByVal grpOrderNo As String, ByVal cnORAString As String) As ArrayList
        Dim arr As New ArrayList
        Dim rtn As String = "orderProcessor.LoadAssociatedSDIOrders"

        bu = bu.Trim
        grpOrderNo = grpOrderNo.Trim
        cnORAString = cnORAString.Trim

        If bu.Length > 0 And grpOrderNo.Length > 0 And cnORAString.Length > 0 Then

            Dim cnORA As New OleDbConnection(cnORAString)

            cnORA.Open()
            If cnORA.State = ConnectionState.Open Then

                ' build UNCC's order # sequence
                Dim sNo As String() = unccSplitOrderNo(orderNo:=grpOrderNo)
                Dim sOrderList As String = ""
                For nIdx As Integer = 1 To 99
                    sOrderList &= "'" & nIdx.ToString(Format:="00") & sNo(1) & "',"
                Next
                If sOrderList.Length > 0 Then
                    sOrderList = sOrderList.TrimEnd(CChar(","))
                End If

                ' build query string
                Dim sql As String = "" & _
                                    "SELECT " & vbCrLf & _
                                    "  A.ORDER_NO AS HDR_ID " & vbCrLf & _
                                    ", A.BUSINESS_UNIT_OM " & vbCrLf & _
                                    ", A.ORDER_NO " & vbCrLf & _
                                    ", B.OPRID_ENTERED_BY " & vbCrLf & _
                                    ", B.ORDER_NO AS LNE_ID " & vbCrLf & _
                                    ", B.ISA_LINE_STATUS AS LNE_STATUS " & vbCrLf & _
                                    ", B.ISA_INTFC_LN AS LINE_NBR " & vbCrLf & _
                                    ", B.QTY_REQUESTED AS QTY_REQ " & vbCrLf & _
                                    ", B.ITM_SETID " & vbCrLf & _
                                    ", B.INV_ITEM_ID " & vbCrLf & _
                                    ", B.DESCR254 AS INV_ITEM_DESC " & vbCrLf & _
                                    ", B.UNIT_OF_MEASURE " & vbCrLf & _
                                    ", B.SHIPTO_ID " & vbCrLf & _
                                    ", B.MFG_ID " & vbCrLf & _
                                    ", B.ISA_MFG_FREEFORM " & vbCrLf & _
                                    ", B.MFG_ITM_ID" & vbCrLf & _
                                    ", B.ISA_EMPLOYEE_ID AS EMPLID " & vbCrLf & _
                                    ", B.ISA_CUST_CHARGE_CD " & vbCrLf & _
                                    ", B.ISA_WORK_ORDER_NO " & vbCrLf & _
                                    "FROM " & vbCrLf & _
                                    "  PS_ISA_ORD_INTF_HD A " & vbCrLf & _
                                    ", PS_ISA_ORD_INTF_LN B " & vbCrLf & _
                                    "WHERE A.BUSINESS_UNIT_OM = '" & bu & "' " & vbCrLf & _
                                    "  AND A.ORDER_NO IN (" & sOrderList & ") " & vbCrLf & _
                                    "  AND A.ORIGIN IN ('INT','IOL') " & vbCrLf & _
                                    "  AND A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM " & vbCrLf & _
                                    "  AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                                    "ORDER BY A.BUSINESS_UNIT_OM, A.ORDER_NO, B.ISA_INTFC_LN" & vbCrLf & _
                                    ""

                Dim cmd As OleDbCommand = cnORA.CreateCommand

                cmd.CommandText = sql
                cmd.CommandType = CommandType.Text

                Dim rdr As OleDbDataReader = Nothing

                Try
                    rdr = cmd.ExecuteReader
                Catch ex As Exception
                    m_logger.WriteVerboseLog(msg:=rtn & "::" & sql)
                    m_logger.WriteErrorLog(msg:=rtn & "::" & ex.ToString)
                End Try

                If Not (rdr Is Nothing) Then
                    Dim sdiOrd As order = Nothing
                    Dim sdiOrdLn As orderLine = Nothing
                    Dim nHdrId As Integer = -1
                    Dim sHdrBU As String = ""
                    Dim sHdrStat As String = ""
                    Dim nLneId As Integer = -1
                    Dim sdiOrderNo As String = ""
                    Dim currentOrderNo As String = ""
                    Dim nLineNo As Integer = -1
                    Dim sLineStatId As String = ""
                    While rdr.Read
                        nHdrId = -1
                        Try
                            nHdrId = CInt(rdr("HDR_ID"))
                        Catch ex As Exception
                        End Try
                        nLneId = -1
                        Try
                            nLneId = CInt(rdr("LNE_ID"))
                        Catch ex As Exception
                        End Try
                        sdiOrderNo = ""
                        Try
                            sdiOrderNo = CStr(rdr("ORDER_NO"))
                        Catch ex As Exception
                        End Try
                        nLineNo = -1
                        Try
                            nLineNo = CInt(rdr("LINE_NBR"))
                        Catch ex As Exception
                        End Try
                        sHdrBU = ""
                        Try
                            sHdrBU = CStr(rdr("BUSINESS_UNIT_OM")).Trim
                        Catch ex As Exception
                        End Try
                        sHdrStat = ""
                        Try
                            sHdrStat = " "  '  CStr(rdr("HDR_STATUS")).Trim
                        Catch ex As Exception
                        End Try
                        sLineStatId = ""
                        Try
                            sLineStatId = CStr(rdr("LNE_STATUS")).Trim
                        Catch ex As Exception
                        End Try
                        If Not (currentOrderNo = sdiOrderNo) Then
                            sdiOrd = New order
                            sdiOrd.OrderNo_Group = grpOrderNo
                            sdiOrd.OrderNo_SDI = sdiOrderNo
                            sdiOrd.Id = nHdrId
                            sdiOrd.OrderStatus = " "  ' sHdrStat
                            sdiOrd.BusinessUnit = sHdrBU
                            arr.Add(sdiOrd)
                            currentOrderNo = sdiOrderNo
                        End If
                        If Not (sdiOrd Is Nothing) And _
                           nLineNo > 0 Then
                            sdiOrdLn = sdiOrd.CreateOrderLine(nLineNo)
                            sdiOrdLn.Id = nLneId
                            sdiOrdLn.BusinessUnit = sHdrBU
                            sdiOrdLn.OrderLineStatus = sLineStatId
                            sdiOrd.OrderLines.Add(sdiOrdLn)
                        End If
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

            End If

            Try
                cnORA.Close()
                cnORA.Dispose()
            Catch ex As Exception
            Finally
                cnORA = Nothing
            End Try

        End If

        Return arr
    End Function

    '// retrieves SDI order for a given line #
    Private Function GetSDIOrderForLine(ByVal bu As String, _
                                        ByVal grpOrderNo As String, _
                                        ByVal orderLineNo As Integer) As order
        Dim sdiOrder As order = Nothing
        If Me.sdiAssociatedOrders.Count > 0 Then
            For Each ord As order In Me.sdiAssociatedOrders
                If ord.BusinessUnit = bu And ord.OrderNo_Group = grpOrderNo Then
                    Dim bIsFoundLine As Boolean = False
                    If ord.OrderLines.Count > 0 Then
                        For Each lne As orderLine In ord.OrderLines
                            If lne.OrderLineNo = orderLineNo Then
                                bIsFoundLine = True
                                Exit For
                            End If
                        Next
                    End If
                    If bIsFoundLine Then
                        sdiOrder = ord
                        Exit For
                    End If
                End If
            Next
        End If
        Return sdiOrder
    End Function

    Private Function GetNextSDIOrderNo(ByVal grpOrderNo As String, ByVal arrExistingOrders As ArrayList) As String
        Dim sOrderNo As String = ""

        Dim i As Integer = 0
        Dim sNo As String() = unccSplitOrderNo(orderNo:=grpOrderNo)

        If arrExistingOrders.Count > 0 Then
            ' get maximum seq #
            Dim nSeq As Integer = -1
            For Each o As order In arrExistingOrders
                nSeq = -1
                Try
                    nSeq = CInt(unccSplitOrderNo(o.OrderNo_SDI)(0))
                Catch ex As Exception
                End Try
                If nSeq > i Then
                    i = nSeq
                End If
            Next
            ' increment
            i += 1
        Else
            i += 1
        End If

        sOrderNo = i.ToString(Format:="00") & sNo(1)

        Return sOrderNo
    End Function

    Private Function unccSplitOrderNo(ByVal orderNo As String) As String()
        Dim s As String() = New String() {"", ""}
        ' first 2 chars/digit
        Try
            s(0) = orderNo.Substring(0, 2)
        Catch ex As Exception
            s(0) = ""
        End Try
        ' remaining chars/digit
        Try
            s(1) = orderNo.Substring(2, orderNo.Length - 2)
        Catch ex As Exception
            s(1) = ""
        End Try
        Return s
    End Function

    Private Function buildHeaderINTFCInsertSQL(ByVal orderReq As order, ByVal strRefNO As String) As String
        Dim sql As String = ""
        'here is where we need to make sure we have a punchin and the right status
        ' if there is a reference number coming from the quote table it is an f-en punchin
        Dim strOrdstat As String = orderReq.OrderStatus.ToString.ToUpper
         
        If Not strRefNO.Length > 0 Then
            strOrdstat = "S"
        Else
            strOrdstat = "P"
        End If
        'BUSINESS_UNIT_OM,ORDER_NO,BILL_TO_CUST_ID,ORIGIN,SOURCE_ID,ORDER_DATE,ORDER_STATUS,
        'REQUESTOR_ID,ISA_ORDER_TYPE,ADD_DTTM,LASTUPDDTTM

        sql = "" & _
              "INSERT INTO SYSADM8.PS_ISA_ORD_INTF_HD " & vbCrLf & _
              "( " & vbCrLf & _
              " BUSINESS_UNIT_OM " & vbCrLf & _
              ",ORDER_NO " & vbCrLf & _
              ",BILL_TO_CUST_ID " & vbCrLf & _
              ",ORIGIN " & vbCrLf & _
              ",SOURCE_ID " & vbCrLf & _
              ",ORDER_DATE " & vbCrLf & _
              ",ORDER_STATUS " & vbCrLf & _
              ",REQUESTOR_ID " & vbCrLf & _
              ",ISA_ORDER_TYPE " & vbCrLf & _
              ",ADD_DTTM " & vbCrLf & _
              ",LASTUPDDTTM " & vbCrLf & _
              ") " & vbCrLf & _
              "VALUES " & vbCrLf & _
              "( " & vbCrLf & _
              " '" & orderReq.BusinessUnit & "' " & vbCrLf & _
              ",'" & orderReq.OrderNo_SDI & "' " & vbCrLf & _
              ",'" & orderReq.SiteInfo.CustID & "' " & vbCrLf & _
              ",'INT' " & vbCrLf & _
              ",' ' " & vbCrLf & _
              ",TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
              ",'1' " & vbCrLf & _
              ",'" & orderReq.EmployeeId.ToUpper & "' " & vbCrLf & _
              ",' ' " & vbCrLf & _
              ",TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
              ",TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
              ") " & vbCrLf & _
              ""
        Return sql

        '",OPRID_ENTERED_BY " & vbCrLf & _
        '",OPRID_MODIFIED_BY " & vbCrLf & _
        '",OPRID_APPROVED_BY " & vbCrLf & _

        '",'" & orderReq.EmployeeId.ToUpper & "' " & vbCrLf & _
        '",'" & orderReq.EmployeeId.ToUpper & "' " & vbCrLf & _
        '",' ' " & vbCrLf & _
    End Function

    Private Function buildLineINTFCInsertSQL(ByVal orderReq As order, _
                                             ByVal orderReqLine As orderLine) As String
        Dim sql As String = ""
        Dim cnORA As New OleDbConnection(Me.oleConnectionString)

        Dim dtRequired As DateTime = Now
        Try
            dtRequired = CDate(orderReqLine.DueDate)
        Catch ex As Exception
        End Try

        Dim itemId As String = CStr(IIf(orderReqLine.InventoryItemId.Trim.Length = 0, " ", orderReqLine.InventoryItemId.Trim.ToUpper))
        If itemId.Trim.Length > 0 Then
            If itemId = "NON-STOCK-PART" Then
                itemId = " "
            Else
                If itemId.Length > 3 Then
                    If Not (itemId.Substring(0, 3) = orderReq.SitePrefix) Then
                        itemId = orderReq.SitePrefix & itemId
                    End If
                Else
                    itemId = orderReq.SitePrefix & itemId
                End If
            End If
            If (itemId.Trim.Length > 0) Then
                itemId = stringEncoder.formStringForSQL(itemId, 18)
            End If
        End If

        Dim strOracleSql1 As String = ""
        Dim strOrclUom As String = " "
        Dim strOrclDescr As String = " "
        Dim strOrclMfg As String = " "
        Dim strOrclMfgName As String = " "
        Dim strOrclMfgItemId As String = " "
        If Trim(itemId) <> "" Then
            itemId = Trim(itemId)
            'get Inv Item infor from Oracle tables
            strOracleSql1 = "SELECT B.INV_ITEM_ID, B.UNIT_MEASURE_STD, B.DESCR60, D.MFG_ID, D.MFG_ITM_ID, G.DESCR60 AS MFG_DESCR FROM PS_MASTER_ITEM_TBL B, PS_ITEM_MFG D, PS_MANUFACTURER G " & vbCrLf & _
                " WHERE B.INV_ITEM_ID = '" & itemId & "' AND B.INV_ITEM_ID = D.INV_ITEM_ID and D.MFG_ID = G.MFG_ID(+)"
            Dim dsOrcl1 As DataSet = ORDBAccess.GetAdapter(strOracleSql1, cnORA)

            If Not dsOrcl1 Is Nothing Then
                If dsOrcl1.Tables.Count > 0 Then
                    If dsOrcl1.Tables(0).Rows.Count > 0 Then
                        Try
                            strOrclUom = dsOrcl1.Tables(0).Rows(0).Item("UNIT_MEASURE_STD")
                        Catch ex As Exception
                            strOrclUom = " "
                        End Try
                        Try
                            strOrclDescr = dsOrcl1.Tables(0).Rows(0).Item("DESCR60")
                            strOrclDescr = Replace(strOrclDescr, "'", "")
                            strOrclDescr = Replace(strOrclDescr, ",", " ")
                        Catch ex As Exception
                            strOrclDescr = " "
                        End Try
                        Try
                            strOrclMfg = dsOrcl1.Tables(0).Rows(0).Item("MFG_ID")
                            strOrclMfg = Replace(strOrclMfg, "'", "")
                            strOrclMfg = Replace(strOrclMfg, ",", " ")
                        Catch ex As Exception
                            strOrclMfg = " "
                        End Try
                        Try
                            strOrclMfgName = dsOrcl1.Tables(0).Rows(0).Item("MFG_DESCR")
                            strOrclMfgName = Replace(strOrclMfgName, "'", "")
                            strOrclMfgName = Replace(strOrclMfgName, ",", " ")
                        Catch ex As Exception
                            strOrclMfgName = " "
                        End Try
                        Try
                            strOrclMfgItemId = dsOrcl1.Tables(0).Rows(0).Item("MFG_ITM_ID")
                            strOrclMfgItemId = Replace(strOrclMfgItemId, "'", "")
                            strOrclMfgItemId = Replace(strOrclMfgItemId, ",", " ")
                        Catch ex As Exception
                            strOrclMfgItemId = " "
                        End Try

                    End If
                End If
            End If

        End If

        Dim itemDesc As String = CStr(IIf(orderReqLine.InventoryItemDescription.Trim.Length = 0, " ", orderReqLine.InventoryItemDescription.Trim.ToUpper))
        If (itemDesc.Trim.Length > 0) Then
            itemDesc = stringEncoder.formStringForSQL(itemDesc, 254)
        Else
            If Trim(strOrclDescr) <> "" Then
                itemDesc = Trim(strOrclDescr)
                itemDesc = stringEncoder.formStringForSQL(itemDesc, 254)
            End If
        End If


        Dim customerId As String = CStr(IIf(orderReq.SiteInfo.CustID.Trim.Length = 0, " ", orderReq.SiteInfo.CustID.Trim.ToUpper))
        If (customerId.Trim.Length > 0) Then
            customerId = stringEncoder.formStringForSQL(customerId)
        End If

        Dim uom As String = CStr(IIf(orderReqLine.UOM.Trim.Length = 0, " ", orderReqLine.UOM.Trim.ToUpper))
        If (uom.Trim.Length > 0) Then
            uom = stringEncoder.formStringForSQL(uom, 3)
        Else
            If Trim(strOrclUom) <> "" Then
                uom = Trim(strOrclUom)
                uom = stringEncoder.formStringForSQL(uom, 3)
            End If
        End If

        Dim mfgId As String = CStr(IIf(orderReqLine.ManufacturerId.Trim.Length = 0, " ", orderReqLine.ManufacturerId.Trim.ToUpper))
        If (mfgId.Trim.Length > 0) Then
            mfgId = stringEncoder.formStringForSQL(mfgId, 10)
        Else
            If Trim(strOrclMfg) <> "" Then
                mfgId = Trim(strOrclMfg)
                mfgId = stringEncoder.formStringForSQL(mfgId, 10)
            End If
        End If

        Dim mfgFreeForm As String = CStr(IIf(orderReqLine.ManufacturerId.Trim.Length = 0, " ", orderReqLine.ManufacturerId.Trim.ToUpper))
        If (mfgFreeForm.Trim.Length > 0) Then
            mfgFreeForm = stringEncoder.formStringForSQL(mfgFreeForm, 30)
        Else
            If Trim(strOrclMfgName) <> "" Then
                mfgFreeForm = Trim(strOrclMfgName)
                mfgFreeForm = stringEncoder.formStringForSQL(mfgFreeForm, 30)
            End If
        End If

        Dim mfgPartNo As String = CStr(IIf(orderReqLine.ManufacturerItemId.Trim.Length = 0, " ", orderReqLine.ManufacturerItemId.Trim.ToUpper))
        If (mfgPartNo.Trim.Length > 0) Then
            mfgPartNo = stringEncoder.formStringForSQL(mfgPartNo, 35)
        Else
            If Trim(strOrclMfgItemId) <> "" Then
                mfgPartNo = Trim(strOrclMfgItemId)
                mfgPartNo = stringEncoder.formStringForSQL(mfgPartNo, 35)
            End If
        End If

        '' store the LOCATION in the NOTES field along with the suggested cost
        'Dim notes As String = " "
        

        'notes = orderReqLine.Location.Trim.ToUpper
        'If (orderReqLine.Cost > 0) Then
        '    notes &= " SUGGESTED COST - " & orderReqLine.Cost.ToString
        'End If
        'notes = CStr(IIf(notes.Trim.Length = 0, " ", notes.Trim))
        'If (notes.Trim.Length > 0) Then
        '    notes = stringEncoder.formStringForSQL(notes, 254)
        'End If

        Dim empId As String = CStr(IIf(orderReqLine.EmployeeId.Trim.Length = 0, " ", orderReqLine.EmployeeId.Trim.ToUpper))
        If (empId.Trim.Length > 0) Then
            If empId.Trim.Length > 8 Then
                empId = empId.Trim.Substring(0, 8)
            End If

            empId = stringEncoder.formStringForSQL(empId, 8)
        End If

        Dim chargeCode As String = CStr(IIf(orderReqLine.CustomerChargeCode.Trim.Length = 0, " ", orderReqLine.CustomerChargeCode.Trim.ToUpper))
        If (chargeCode.Trim.Length > 0) Then
            chargeCode = stringEncoder.formStringForSQL(chargeCode, 40)
        Else
            chargeCode = "USE WORK ORDER"
        End If
        ' reference number to determine if it is a Punchin/Punchout

        Dim strlineordstat As String = CStr(IIf(orderReqLine.OrderLineStatus.Trim.Length = 0, " ", orderReqLine.OrderLineStatus.Trim.ToUpper))
        'If (uom.Trim.Length > 0) Then
        '    strlineordstat = stringEncoder.formStringForSQL(strlineordstat, 1)
        'End If
        'Try
        '    If Not strlineordstat = "P" Then
        '        strlineordstat = "1"

        '    End If
        'Catch ex As Exception
        '    strlineordstat = "1"
        'End Try
        strlineordstat = "NEW"

        Dim workOrderNo As String = CStr(IIf(orderReqLine.WorkOrderNo.Trim.Length = 0, " ", orderReqLine.WorkOrderNo.Trim.ToUpper))
        If (workOrderNo.Trim.Length > 0) Then
            workOrderNo = stringEncoder.formStringForSQL(workOrderNo, 20)
        End If
        '   Here we need to deterimine if it is  a Punchin/Punchout record and here we need to make the isa_ = 'Z'
        Dim strVendid As String = orderReqLine.Vendor_id.ToString
        Dim strItmIDVndr As String = orderReqLine.ITM_ID_VNDR.ToString
        Dim strItmIDVndr_AUX As String = orderReqLine.ITM_ID_VNDR_AUX.ToString

        'Dim shipTo As String = CStr(IIf(orderReqLine.Location.Trim.Length = 0, " ", orderReqLine.Location.Trim.ToUpper))

        ' we now get shipto id from quote or if not punchin there is no shipto_id" "  
        'Dim shipTo As String = CStr(IIf(orderReqLine.Location.Trim.Length = 0, "", orderReqLine.Location.Trim.ToUpper))
        Dim shipto As String = CStr(IIf(orderReqLine.ShipToId.Trim.Length = 0, " ", orderReqLine.ShipToId.Trim.ToUpper))
        If (shipto.Trim.Length > 0) Then
            shipto = stringEncoder.formStringForSQL(shipto, 10)
        End If
        'Dim strVenloc As String = CStr(IIf(orderReqLine.Location.Trim.Length = 0, " ", orderReqLine.Location.Trim.ToUpper))
        'If (strVenloc.Trim.Length > 0) Then
        '    strVenloc = stringEncoder.formStringForSQL(strVenloc, 10)
        'End If




        Try
            If Trim(strVendid) = "" Then
                strVendid = " "
            End If
        Catch ex As Exception
            strVendid = " "
        End Try
        Try
            If Trim(strItmIDVndr) = "" Then
                strItmIDVndr = " "
            End If
        Catch ex As Exception
            strItmIDVndr = " "
        End Try
        Try
            If Trim(strItmIDVndr_AUX) = "" Then
                strItmIDVndr_AUX = " "
            Else
                strItmIDVndr_AUX = Trim(strItmIDVndr_AUX)
                If Len(strItmIDVndr_AUX) > 60 Then
                    strItmIDVndr_AUX = Microsoft.VisualBasic.Left(strItmIDVndr_AUX, 60)
                End If
            End If
        Catch ex As Exception
            strItmIDVndr_AUX = " "
        End Try

        'BUSINESS_UNIT_OM,ORDER_NO,ISA_INTFC_LN,BUSINESS_UNIT_IN,BUSINESS_UNIT_PO,REF_ORDER_NO,REF_LINE_NBR,SHIP_TO_CUST_ID,
        'OPRID_APPROVED_BY,APPROVAL_DTTM - not null - ,ITM_SETID,INV_ITEM_ID,CANCEL_DTTM - not null - ,CONTRACT_NUM,CONTRACT_LINE_NUM2,
        'TXN_CURRENCY_CD,CURRENCY_CD_BASE,PRICE_VNDR,ISA_SELL_PRICE,ISA_REQUIRED_BY_DT,EXPECTED_DATE,UNIT_OF_MEASURE,
        'QTY_REQUESTED,QTY_ORDERED_BASE,ISA_LINE_STATUS,CHANGE_FLAG,TAX_EXEMPT,TAX_EXEMPT_CERT,TAX_GROUP,ISA_EMPLOYEE_ID,
        'ISA_PRIORITY_FLAG,NEEDS_REPAIR_SW,PRIORITY_NBR,VENDOR_SETID,VENDOR_ID,ITM_ID_VNDR,MFG_ID,ISA_MFG_FREEFORM,MFG_ITM_ID,
        'ISA_NONCAT_KEY,INSPECT_CD,DESCR254,ISA_UNLOADING_PT,DELIVERED_TO,PROCESS_INSTANCE,CUSTOMER_PO,ISA_CUST_PO_LINE,
        'ACCOUNT,ISA_WORK_ORDER_NO,ISA_ACTIVITY_NBR,ISA_MACHINE_NO,NETWORK_ID,ISA_WBS_ELMNT,PROJECT_ID,ISA_CUST_CHARGE_CD,
        'ISA_QUOTE_REF,ISA_INTFC_LN_TYPE,ADD_DTTM,LASTUPDDTTM,OPRID_ENTERED_BY,OPRID_MODIFIED_BY,SHIPTO_ID,LOCATION2,
        'QTY_RECEIVED,QTY_SHIPPED,KIT_PRESENT_FLG,ISA_KIT_ID,LINE_FIELD_C6,RFQ_IND,ISA_PICK_COMPLETE,ISA_SUGGESTED_VNDR,
        'STORAGE_AREA,STOR_LEVEL_1,STOR_LEVEL_2,STOR_LEVEL_3,STOR_LEVEL_4,HAZARDOUS_SW,ISA_ITEM_CAT,DELIVERED_FLG,ISA_LANE_ID,
        'ISA_STOP_NBR,BUYER_ID,SERIAL_ID,USER_CHAR1,USER_CHAR2,USER_CHAR3,USER_DATE1,USER_DATE2,USER_AMT1,USER_AMT2,
        'USER_DTTM1,USER1,ISA_USER1,ISA_USER2,ISA_USER3,ISA_USER4,ISA_USER5,ERROR_FLAG

        Dim strNetUnitPrice As String = orderReqLine.NetUnitPrice.ToString
        Dim strSDIXPrice As String = "0"
        If Trim(itemId) <> "" Then
            itemId = Trim(itemId)
            Try
                strSDIXPrice = GetSDIXPrice(itemId)
                If Trim(strSDIXPrice) <> "" Then
                    If Trim(strSDIXPrice) <> "0" Then
                        strSDIXPrice = Trim(strSDIXPrice)
                        If IsNumeric(strSDIXPrice) Then
                            strNetUnitPrice = strSDIXPrice
                        End If
                    End If
                End If
            Catch ex As Exception

            End Try

        End If  '   If Trim(itemId) <> "" Then

        Dim strNetPoPrice As String = orderReqLine.NetPOPrice.ToString

        Dim StrDueDate As String = dtRequired.ToString

        sql = "INSERT INTO SYSADM8.PS_ISA_ORD_INTF_LN (BUSINESS_UNIT_OM,ORDER_NO,ISA_INTFC_LN,BUSINESS_UNIT_IN,BUSINESS_UNIT_PO," & _
                    "REF_ORDER_NO,REF_LINE_NBR,SHIP_TO_CUST_ID,OPRID_APPROVED_BY,ITM_SETID," & _
                    "INV_ITEM_ID,CONTRACT_NUM,CONTRACT_LINE_NUM2,TXN_CURRENCY_CD,CURRENCY_CD_BASE,PRICE_VNDR,ISA_SELL_PRICE," & _
                    "UNIT_OF_MEASURE,QTY_REQUESTED,QTY_ORDERED_BASE,ISA_LINE_STATUS,CHANGE_FLAG,TAX_EXEMPT,TAX_EXEMPT_CERT," & _
                    "TAX_GROUP,ISA_EMPLOYEE_ID,ISA_PRIORITY_FLAG,NEEDS_REPAIR_SW,PRIORITY_NBR,VENDOR_SETID,VENDOR_ID," & _
                    "ITM_ID_VNDR,MFG_ID,ISA_MFG_FREEFORM,MFG_ITM_ID,ISA_NONCAT_KEY,INSPECT_CD,DESCR254,ISA_UNLOADING_PT," & _
                    "DELIVERED_TO,PROCESS_INSTANCE,CUSTOMER_PO,ISA_CUST_PO_LINE,ACCOUNT,ISA_WORK_ORDER_NO,ISA_ACTIVITY_NBR,ISA_MACHINE_NO," & _
                    "NETWORK_ID,ISA_WBS_ELMNT,PROJECT_ID,ISA_CUST_CHARGE_CD,ISA_QUOTE_REF,ISA_INTFC_LN_TYPE,ADD_DTTM,OPRID_ENTERED_BY," &
                    "SHIPTO_ID,LOCATION2,QTY_RECEIVED,QTY_SHIPPED,KIT_PRESENT_FLG,ISA_KIT_ID,LINE_FIELD_C6,RFQ_IND,ISA_PICK_COMPLETE," & _
                    "ISA_SUGGESTED_VNDR,STORAGE_AREA,STOR_LEVEL_1,STOR_LEVEL_2,STOR_LEVEL_3,STOR_LEVEL_4,HAZARDOUS_SW,ISA_ITEM_CAT," & _
                    "DELIVERED_FLG,ISA_LANE_ID,BUYER_ID,SERIAL_ID,USER_CHAR1,USER_CHAR2,USER_CHAR3,USER_AMT1,USER_AMT2,USER_AMT3,USER1," & _
                    "ISA_USER1,ISA_USER2,ISA_USER3,ISA_USER4,ISA_USER5,ERROR_FLAG,OPRID_MODIFIED_BY,ISA_STOP_NBR"

        If Not String.IsNullOrEmpty(StrDueDate) Then
            sql = sql + ",ISA_REQUIRED_BY_DT"
        End If
        sql = sql + ")"

        Dim strRefOrdNo As String = orderReqLine.ReferenceNo
        If Trim(strRefOrdNo) = "" Then
            strRefOrdNo = " "
        End If

        Dim intRefOrdNoLine As Integer = 0
        If Trim(orderReqLine.ReferenceNoLine) <> "" Then
            intRefOrdNoLine = CType(orderReqLine.ReferenceNoLine, Integer)
        End If

        sql = sql & "VALUES ('I0256','" & orderReq.OrderNo_SDI.ToString & "','" & orderReqLine.OrderLineNo.ToString & "',' ',' '," & _
            "'" & strRefOrdNo & "'," & intRefOrdNoLine & ",'" & customerId & "',' ','MAIN1'," & _
            "'" & itemId & "',' ',0,' ',' ','" & strNetPoPrice & "','" & strNetUnitPrice & "'," & _
            "'" & uom & "'," & orderReqLine.Quantity.ToString & ",0,'" & strlineordstat & "',' ',' ',' '," & _
            "' ','" & empId & "', '" & orderReqLine.PriorityCode & "',' ',0,'MAIN1','" & strVendid & "'," & _
            "'" & strItmIDVndr & "','" & mfgId & "','" & mfgFreeForm & "','" & mfgPartNo & "',' ','N','" & itemDesc & "',' '," & _
            "' ',0,' ','0',' ','" & workOrderNo & "',' ',' '," & _
            "' ',' ',' ','" & chargeCode & "',' ',' ',TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM'),'" & empId & "'," & _
            "'" & shipto & "',' ','" & orderReqLine.Quantity.ToString & "',0,' ',' ',' ','N',' '," & _
            "' ',' ',' ',' ',' ',' ',' ',' '," & _
            "' ',0,' ',' ',' ',' ',' ',0,0,0,' ',' ',' ',' ',' ','" & strItmIDVndr_AUX & "',' ',' ',0"

        If Not String.IsNullOrEmpty(StrDueDate) Then
            sql = sql + ",TO_DATE('" & StrDueDate.ToString & "', 'MM/DD/YYYY HH:MI:SS AM')"
        End If
        sql = sql + ")"

        Return sql
    End Function

    Private Function GetSDIXPrice(ByVal strInvItemId As String) As String

        Dim cnORA As New OleDbConnection(Me.oleConnectionString)
        Dim strPrice As String = "0"
        Dim strSQLstring As String = ""
        strSQLstring = "SELECT PRICE FROM SYSADM8.PS_ISA_SDIEX_PRCBU WHERE INV_ITEM_ID = '" & strInvItemId & "' AND BUSINESS_UNIT = 'I0256'"

        Try
            strPrice = ORDBAccess.GetScalar(strSQLstring, cnORA)
            If Trim(strPrice) = "" Then
                strPrice = "0"
            End If
        Catch ex As Exception
            strPrice = "0"
        End Try

        Return strPrice

    End Function
End Class
