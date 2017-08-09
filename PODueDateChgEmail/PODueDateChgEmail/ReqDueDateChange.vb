Imports System.Data
Imports System.Data.OleDb
Imports SDI.ApplicationLogger
Imports System.Net.Mail


Public Class ReqDueDateChange

    Private m_cn As OleDbConnection
    Private m_sConnectionString As String
    Private m_colReqCollection As Collection
    Private m_clsLogger As New appLogger
    Private m_sCommonMsgText As String = "Req Due Date Change Class - "

    Public Property Logger() As appLogger
        Get
            Logger = m_clsLogger
        End Get
        Set(ByVal value As appLogger)
            m_clsLogger = value
        End Set
    End Property

    Public Property ReqCollection() As Collection
        Get
            ReqCollection = m_colReqCollection
        End Get
        Set(ByVal value As Collection)
            m_colReqCollection = value
        End Set
    End Property

    Public Property ConnectionString() As String
        Get
            ConnectionString = m_sConnectionString
        End Get
        Set(ByVal value As String)
            m_sConnectionString = value
        End Set
    End Property

    Public Function ProcessReqDueDateChanges() As Boolean
        Dim cHdr As String = m_sCommonMsgText & "ProcessReqDueDate: "
        Dim bReturn As Boolean = True
        Dim sSQL As String = CreateSelect()
        Dim cmd As OleDbCommand = m_cn.CreateCommand
        Dim sTempBu As String
        Dim sTempOrderNo As String
        Dim sTempLineNbr As String
        Dim sTempReqStatus As String
        Dim sTempExpectedDeliveryDate As String
        Dim sTempSchedNbr As String
        Dim sTempDueDate As String
        Dim sTempEmployeeId As String
        Dim sTempEmployeeName As String
        Dim sTempEmployeeEmail As String
        Dim sTempInvItemId As String
        Dim sTempDescr254 As String
        Dim sPreviousBU As String = ""
        Dim sPreviousOrderNo As String = ""
        Dim sPreviousEmployeeID As String = ""
        Dim clsReq As REQ
        Dim clsReqLine As ReqLine


        cmd.CommandText = sSQL
        cmd.CommandType = CommandType.Text

        Try
            'Logger.WriteErrorLog(cHdr & "Executing SQL statement for REQs")
            Logger.WriteVerboseLog(cHdr & "Trying to execute SQL statement for REQs")
            If m_cn.State = ConnectionState.Open Then
                Dim rdr As OleDbDataReader = cmd.ExecuteReader()

                If Not rdr Is Nothing Then
                    If rdr.HasRows Then

                        While rdr.Read

                            sTempBu = ""
                            sTempOrderNo = ""
                            sTempLineNbr = ""
                            sTempReqStatus = ""
                            sTempInvItemId = ""
                            sTempDescr254 = ""
                            sTempExpectedDeliveryDate = ""
                            sTempDueDate = ""
                            sTempEmployeeId = ""
                            sTempEmployeeName = ""
                            sTempEmployeeEmail = ""
                            sTempSchedNbr = ""

                            'Debug.Print(rdr.Item("business_unit_om").ToString)
                            'Debug.Print(rdr.Item("ORDER_NO").ToString)
                            'Debug.Print(rdr.Item("LINE_NBR").ToString)
                            'Debug.Print(rdr.Item("req_status").ToString)
                            'Debug.Print(rdr.Item("inv_item_id").ToString)
                            'Debug.Print(rdr.Item("descr254").ToString)
                            'Debug.Print(rdr.Item("EXPECTED_DELIV_DT").ToString)
                            'Debug.Print(rdr.Item("DUE_DT").ToString)
                            'Debug.Print(rdr.Item("isa_employee_id").ToString)
                            'Debug.Print(rdr.Item("ISA_EMPLOYEE_NAME").ToString)
                            'Debug.Print(rdr.Item("ISA_EMPLOYEE_EMAIL").ToString)

                            sTempBu = rdr.Item("business_unit_om").ToString
                            sTempOrderNo = rdr.Item("ORDER_NO").ToString
                            sTempLineNbr = rdr.Item("LINE_NBR").ToString
                            sTempReqStatus = rdr.Item("req_status").ToString
                            sTempInvItemId = rdr.Item("inv_item_id").ToString
                            sTempDescr254 = rdr.Item("descr254").ToString
                            Try
                                sTempExpectedDeliveryDate = CStr(Format(rdr.Item("EXPECTED_DELIV_DT"), "MM/dd/yyyy"))
                            Catch ex As Exception
                                sTempExpectedDeliveryDate = ""
                            End Try
                            'sTempExpectedDeliveryDate = CStr(Format(rdr.Item("EXPECTED_DELIV_DT"), "MM/dd/yyyy"))
                            sTempSchedNbr = rdr.Item("sched_nbr").ToString
                            Try
                                sTempDueDate = CStr(Format(rdr.Item("DUE_DT"), "MM/dd/yyyy"))
                            Catch ex As Exception
                                sTempDueDate = ""
                            End Try
                            'sTempDueDate = CStr(Format(rdr.Item("DUE_DT"), "MM/dd/yyyy"))
                            sTempEmployeeId = rdr.Item("isa_employee_id").ToString
                            sTempEmployeeName = rdr.Item("ISA_EMPLOYEE_NAME").ToString
                            sTempEmployeeEmail = rdr.Item("ISA_EMPLOYEE_EMAIL").ToString

                            If (sPreviousBU <> sTempBu) Or _
                               (sPreviousOrderNo <> sTempOrderNo) Or _
                               (sPreviousEmployeeID <> sTempEmployeeId) Then

                                If sPreviousBU <> "" Then

                                    If Not clsReq Is Nothing Then
                                        'save req class before making new one
                                        m_colReqCollection.Add(clsReq, clsReq.BusinessUnit & clsReq.ReqId & clsReq.EmployeeId)

                                    End If

                                End If
                                clsReq = New REQ(sTempBu, sTempOrderNo, sTempEmployeeId)
                                clsReq.EmployeeEmail = sTempEmployeeEmail
                                sPreviousBU = sTempBu
                                sPreviousOrderNo = sTempOrderNo
                                sPreviousEmployeeID = sTempEmployeeId
                            End If

                            'add line
                            clsReqLine = New ReqLine
                            clsReqLine.ReqLineNo = sTempLineNbr
                            clsReqLine.POLineSched_NBR = sTempSchedNbr
                            clsReqLine.ItemID = sTempInvItemId
                            clsReqLine.Desc = sTempDescr254
                            clsReqLine.oldDate = sTempExpectedDeliveryDate
                            clsReqLine.newDate = sTempDueDate
                            clsReq.ReqLines.Add(clsReqLine)

                        End While

                        m_colReqCollection.Add(clsReq, clsReq.BusinessUnit & clsReq.ReqId & clsReq.EmployeeId)

                    Else
                        Logger.WriteVerboseLog(cHdr & "rdr has no rows.")
                        bReturn = False
                    End If



                    Try
                        rdr.Close()

                    Catch ex As Exception

                    Finally
                        rdr = Nothing
                    End Try

                End If   '   Not rdr Is Nothing
            Else
                Logger.WriteVerboseLog(cHdr & "Connection is not open.")
                bReturn = False

            End If

            Try
                cmd.Dispose()

            Catch ex As Exception

            Finally
                cmd = Nothing
            End Try

            Try
                m_cn.Dispose()
                m_cn.Close()
                m_cn = Nothing
            Catch ex As Exception

            Finally
                m_cn = Nothing
            End Try

            Logger.WriteErrorLog(cHdr & m_colReqCollection.Count.ToString & " Reqs to be emailed.")

        Catch ex As Exception
            Logger.WriteErrorLog(cHdr & "ERROR Processing REQS: " & ex.Message)
            bReturn = False
        Finally
            If Not m_cn Is Nothing Then
                If m_cn.State = ConnectionState.Closed Then
                    m_cn = Nothing
                Else

                    Try
                        m_cn.Dispose()
                        m_cn.Close()
                        m_cn = Nothing
                    Catch ex As Exception

                    Finally
                        m_cn = Nothing
                    End Try
                End If
            End If
        End Try

        Return bReturn

    End Function

    Public Sub New(ByVal ConnectionString As String)
        m_colReqCollection = New Collection
        m_sConnectionString = ConnectionString
        m_cn = New OleDbConnection
        m_cn.ConnectionString = m_sConnectionString
        Try
            m_cn.Open()
            If m_cn.State = ConnectionState.Open Then
                'Debug.Print("connection successful: connected with - " & m_sConnectionString)
                Logger.WriteInformationLog("connection successful: connected with - " & m_sConnectionString)
            Else
                'Debug.Print("connection FAILED: tried connected with - " & m_sConnectionString)
                Logger.WriteInformationLog("connection FAILED: tried connected with - " & m_sConnectionString)
            End If
        Catch ex As Exception

            Logger.WriteErrorLog("connection FAILED: tried connected with - " & m_sConnectionString)
            Try
                If Not m_cn Is Nothing Then
                    m_cn.Dispose()
                    m_cn.Close()
                    m_cn = Nothing
                End If
            Catch ex1 As Exception
                m_cn = Nothing

                Logger.WriteErrorLog("FAILED to close connection in ReqDueDateChange.vb - New procedure")
            End Try
        End Try

    End Sub

    Public Function CreateSelect() As String
        Dim sSql As String = ""

        sSql = "select intfc_h.business_unit_om, intfc_h.ORDER_NO, intfc_l.LINE_NBR, req_hdr.req_status, " & _
                       "intfc_l.inv_item_id, intfc_l.descr254, trunc(intfc_l.EXPECTED_DELIV_DT) as EXPECTED_DELIV_DT, " & _
                       "req_line.sched_nbr, trunc(req_line.DUE_DT) as DUE_DT, REQ_L.isa_employee_id, E.ISA_EMPLOYEE_NAME, E.ISA_EMPLOYEE_EMAIL " & _
                 "from sysadm8.ps_isa_ord_intfc_h intfc_h, " & _
                      "sysadm8.ps_isa_ord_intfc_l intfc_l, " & _
                      "SYSADM8.PS_REQ_LINE_SHIP req_line, " & _
                      "sysadm8.ps_req_line req_l, " & _
                      "sysadm8.ps_req_hdr req_hdr, " & _
                      "sysadm8.PS_ISA_USERS_TBL E " & _
                  "where intfc_h.isa_identifier = intfc_l.isa_parent_ident " & _
                    "and intfc_h.order_no = req_HDR.req_id " & _
                    "and intfc_h.order_no = req_line.req_id  " & _
                    "and intfc_l.line_nbr = req_line.line_nbr " & _
                    "and req_line.req_id = req_l.req_id " & _
                    "and req_line.line_nbr = req_l.line_nbr " & _
                    "AND UPPER(REQ_L.ISA_EMPLOYEE_ID) = UPPER(E.ISA_EMPLOYEE_ID(+)) " & _
                    "and req_hdr.req_status not in ('X', 'C') " & _
                    "AND intfc_l.EXPECTED_DELIV_DT <> req_line.DUE_DT " & _
                    "and req_l.source_status = 'A' " & _
                    " AND NOT EXISTS ( " & vbCrLf & _
                    "                  SELECT 'X' " & vbCrLf & _
                    "                    FROM SYSADM8.PS_ISA_PODUEDTMON DTMON " & vbCrLf & _
                    "                   WHERE DTMON.BUSINESS_UNIT = intfc_h.Business_unit_om  " & vbCrLf & _
                    "                     AND DTMON.PO_ID = intfc_h.ORDER_NO " & vbCrLf & _
                    "                     AND DTMON.LINE_NBR = intfc_l.LINE_NBR " & vbCrLf & _
                    "                     AND DTMON.SCHED_NBR = req_line.SCHED_NBR " & vbCrLf & _
                    "                     AND (DTMON.NOTIFY_DTTM IS NOT NULL  and trunc(req_line.DUE_DT) = trunc(dtmon.due_dt)) " & vbCrLf & _
                    "                 ) " & _
                    "order by intfc_h.business_unit_om, intfc_h.order_no, REQ_L.ISA_EMPLOYEE_ID, intfc_l.line_nbr"


        Return sSql
    End Function

End Class
