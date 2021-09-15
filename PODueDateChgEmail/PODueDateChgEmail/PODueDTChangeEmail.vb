Imports System.Data.OleDb
Imports System.Xml
Imports System.Collections.Specialized
Imports SDI.ApplicationLogger
Imports System.Net.Mail
Imports System.Text

Public Class PODueDTChangeEmail

    Private Enum processMode As Integer
        [List] = 0
        [All] = 1
    End Enum

    Private Const oraCN_default_provider As String = "Provider=OraOLEDB.Oracle.1;"
    Private Const oraCN_default_creden As String = "User ID=sdiexchange;Password=sd1exchange;"
    Private Const oraCN_default_DB As String = "Data SourceF=FSTST"

    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\PODueDTChangeEmail.exe.config"
    ' Private m_configFile As String = "C:\Documents and Settings\joe.rank\My Documents\Visual Studio 2008\Projects\PODueDTChangeEmail\obj\Debug\PODueDTChangeEmail.exe.config"
    Private m_sCommonMsgText As String = "SDI PO Due Date Change Monitor."
    Private m_emlAlert As System.Net.Mail.MailMessage
    Private m_sMachineName As String = System.Environment.MachineName
    Private m_logPath As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\logs\"
    Private m_logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose
    Private m_normalChkTimeElapse As Integer = 1 '1800     '30 mins
    Private m_svrDownChkTimeElapse As Integer = 1 '600     '10 mins
    Private m_oraCNstring As String = "" &
                                      oraCN_default_provider &
                                      oraCN_default_creden &
                                      oraCN_default_DB &
                                      ""
    Private m_siteList As New ArrayList
    Private m_siteListMode As processMode = processMode.List
    Private m_settingsObj As New NameValueCollection
    Private cn As OleDbConnection
    Private logger As appLogger

    'Public WithEvents m_tmrChk As System.Timers.Timer = Nothing


#Region " Component Designer generated code "

    Public Sub New()
        MyBase.New()

        ' This call is required by the Component Designer.
        InitializeComponent()

    End Sub

    Public Sub Main2()

        Dim sMyDbase As String = ""

        Try
            sMyDbase = My.Settings("default_DB").ToString.Trim
        Catch ex As Exception
            sMyDbase = ""
        End Try
        If Trim(sMyDbase) <> "" Then
            m_oraCNstring = "" &
                    oraCN_default_provider &
                    oraCN_default_creden &
                    sMyDbase &
                    ""
        End If

        Dim cHdr As String = m_sCommonMsgText & " Main1 "
        Dim logFile As String = m_logPath & Now.ToString("yyyyMMddHHmmss") & ".txt"
        logger = New appLogger(logFile, m_logLevel)


        logger.WriteLog("PO Due Date Change Check Process Started", TraceLevel.Info)
        Dim colEmailedPOs As New Collection

        cn = New OleDbConnection(m_oraCNstring)

        Dim clsReqDateChange As New ReqDueDateChange(m_oraCNstring)

        clsReqDateChange.Logger = logger

        Dim bResult As Boolean = False

        bResult = clsReqDateChange.ProcessReqDueDateChanges()

        Try

            cn.Open()

            If cn.State = ConnectionState.Open Then

                logger.WriteVerboseLog(cHdr & "Successful connection to " & cn.ConnectionString)

                If bResult Then
                    logger.WriteVerboseLog(cHdr & "Processing Req Due Date Changes Succeeded. Count: " & clsReqDateChange.ReqCollection.Count.ToString)
                    For Each tempReq As REQ In clsReqDateChange.ReqCollection
                        'send each req to class for email and update poduedtmon
                        If Not tempReq.EmployeeId Is Nothing Then
                            If tempReq.EmployeeId.Trim() <> "" Then
                                If sendEmailForPO(tempReq) Then
                                    For Each myline As ReqLine In tempReq.ReqLines
                                        flagPOAsProcessed(tempReq.BusinessUnit, tempReq.ReqId, myline.ReqLineNo,
                                                     myline.POLineSched_NBR, myline.newDate, True)

                                    Next
                                    logger.WriteVerboseLog("///////////////////////////////////////////////////////////////////////////////////////////////////////////////")
                                End If
                            End If
                        End If
                    Next

                    logger.WriteVerboseLog(cHdr & "Processing Reqs Due Date Changes EMAIL Complete")

                Else
                    logger.WriteErrorLog(cHdr & "Processing Req Due Date Changes either FAILED 0r there no rows to process")

                End If

                'Should we do something with this object: clsReqDateChange?
                ' not done yet

                'process reqs first
                logger.WriteVerboseLog(cHdr & "Processing PO Due Date Changes")

                Dim arr As New ArrayList
                '-- get available PO to check

                Dim cmd As OleDbCommand = cn.CreateCommand
                cmd.CommandText = sqlRetrievePOListToCheck()
                cmd.CommandType = CommandType.Text
                Dim rdr As OleDbDataReader = Nothing
                Try
                    rdr = cmd.ExecuteReader
                Catch ex As Exception
                End Try
                If Not (rdr Is Nothing) Then
                    Dim currPO As po = Nothing
                    Dim currPOLn As poLine = Nothing
                    Dim currPOLnSched As poLineSched = Nothing
                    logger.WriteVerboseLog("rdr is not Nothing")

                    While rdr.Read
                        currPO = New po
                        Try
                            currPO.PurchasingBusinessUnit = CStr(rdr("BUSINESS_UNIT")).Trim.ToUpper
                            If currPO.PurchasingBusinessUnit Is Nothing Then
                                currPO.PurchasingBusinessUnit = ""
                            End If
                        Catch ex As Exception
                            logger.WriteErrorLog("rdr BUSINESS_UNIT error: " & ex.Message)
                        End Try
                        Try
                            currPO.POId = CStr(rdr("PO_ID")).Trim.ToUpper
                            If currPO.Id Is Nothing Then
                                currPO.POId = ""
                            End If
                        Catch ex As Exception
                            logger.WriteErrorLog("rdr PO_ID error: " & ex.Message)
                        End Try
                        Try
                            currPO.PurchaseOrderDate = CDate(rdr("PO_DT")).ToString
                            If currPO.PurchaseOrderDate Is Nothing Then
                                currPO.PurchaseOrderDate = ""
                                'Else
                                '    currPO.PurchaseOrderDate = currPO.PurchaseOrderDate
                            End If
                        Catch ex As Exception
                            logger.WriteErrorLog("rdr po date error: " & ex.Message)
                        End Try
                        currPOLn = New poLine(currPO)
                        currPO.POLines.Add(currPOLn)
                        Try
                            currPOLn.PurchaseOrderLineNo = CInt(rdr("LINE_NBR"))
                        Catch ex As Exception
                            logger.WriteErrorLog(("rdr purchase order LINE_NBR error: " & ex.Message))
                        End Try
                        Try
                            currPOLn.Desc = CStr(rdr("DESCR254_MIXED"))
                        Catch ex As Exception
                            logger.WriteErrorLog(("rdr Desc  error: " & ex.Message))
                        End Try
                        Try
                            currPOLn.ItemID = CStr(rdr("ITEM_ID"))

                        Catch ex As Exception
                            logger.WriteErrorLog(("rdr ITEM_ID error: " & ex.Message))
                        End Try

                        currPOLnSched = New poLineSched(currPOLn)
                        currPOLn.Schedules.Add(currPOLnSched)
                        Try
                            currPOLnSched.PurchaseOrderLineScheduleNo = CInt(rdr("SCHED_NBR"))
                        Catch ex As Exception
                        End Try
                        Try
                            currPOLnSched.InventoryBusinessUnit = CStr(rdr("BUSINESS_UNIT_IN")).Trim.ToUpper
                            If currPOLnSched.InventoryBusinessUnit Is Nothing Then
                                currPOLnSched.InventoryBusinessUnit = ""
                            Else
                                If currPOLnSched.InventoryBusinessUnit = "" Then
                                    currPOLnSched.InventoryBusinessUnit = CStr(rdr("bu_nstk")).Trim.ToUpper
                                    If currPOLnSched.InventoryBusinessUnit Is Nothing Then
                                        currPOLnSched.InventoryBusinessUnit = ""
                                    End If
                                End If
                            End If
                        Catch ex As Exception
                        End Try
                        Try
                            currPOLnSched.PurchaseOrderDueDate = CStr(rdr("DUE_DT")).Trim
                            If currPOLnSched.PurchaseOrderDueDate Is Nothing Then
                                currPOLnSched.PurchaseOrderDueDate = ""
                                'Else
                                '    currPOLnSched.PurchaseOrderDueDate = Format(currPOLnSched.PurchaseOrderDueDate, "MM/dd/yyyy")
                            End If
                        Catch ex As Exception
                            logger.WriteErrorLog("rdr purchase due date error: " & ex.Message)
                        End Try
                        Try
                            currPOLnSched.OriginalPromisedDate = CStr(rdr("ORIG_PROMISE_DT")).Trim
                            If currPOLnSched.OriginalPromisedDate Is Nothing Then
                                currPOLnSched.OriginalPromisedDate = ""
                                'Else
                                '    currPOLnSched.OriginalPromisedDate = Format(currPOLnSched.OriginalPromisedDate, "MM/dd/yyyy")
                            End If
                        Catch ex As Exception
                            currPOLnSched.OriginalPromisedDate = ""
                            logger.WriteErrorLog("rdr OriginalPromisedDate error: " & ex.Message)
                        End Try

                        Try
                            currPOLnSched.OrderNo = CStr(rdr("ORDER_NO"))
                        Catch ex As Exception
                            logger.WriteErrorLog(("rdr ORDER_NO  error: " & ex.Message))
                        End Try
                        Try
                            currPOLnSched.OrderLineNo = CStr(rdr("ORDER_INT_LINE_NO"))
                        Catch ex As Exception
                            logger.WriteErrorLog(("rdr order line No error: " & ex.Message))
                        End Try

                        Dim cmdEmpl As OleDbCommand = cn.CreateCommand
                        cmdEmpl.CommandText = "SELECT intfc_l.ISA_EMPLOYEE_ID FROM SYSADM8.ps_isa_ord_intf_lN intfc_l " & vbCrLf &
                            " WHERE intfc_l.ORDER_NO = '" & currPOLnSched.OrderNo & "' " & vbCrLf &
                            " AND intfc_l.ISA_INTFC_LN = " & currPOLnSched.OrderLineNo & " "
                        cmdEmpl.CommandType = CommandType.Text

                        Try
                            currPOLnSched.EmployeeID = cmdEmpl.ExecuteScalar
                        Catch ex As Exception
                            logger.WriteErrorLog(("rdr employee ID error: " & ex.Message))
                        End Try
                        cmdEmpl = Nothing

                        If currPO.PurchasingBusinessUnit.Length > 0 And
                           currPO.POId.Length > 0 And
                           currPOLn.PurchaseOrderLineNo > 0 And
                           currPOLnSched.PurchaseOrderLineScheduleNo > 0 And
                           IsDate(currPOLnSched.PurchaseOrderDueDate) Then
                            Dim bIsFound As Boolean = False
                            For Each o As po In arr
                                If o.Id = currPO.Id Then
                                    Dim ln As poLine = o.GetPOLine(currPOLn)
                                    If (ln Is Nothing) Then
                                        ' add the po line
                                        currPOLn.ParentPO = o
                                        o.POLines.Add(currPOLn)
                                    Else
                                        currPOLnSched.ParentPOLine = ln
                                        ln.Schedules.Add(currPOLnSched)
                                    End If
                                    bIsFound = True
                                    Exit For
                                End If
                            Next
                            If Not bIsFound Then
                                arr.Add(currPO)
                            End If

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
                '-- process POs on the list
                logger.WriteInformationLog("Number of PO's Available for Processing: " & arr.Count.ToString)
                If arr.Count > 0 Then
                    Dim iCount As Integer
                    Dim iLineCount As Integer
                    Dim myPO As po
                    Dim myPOLine As poLine
                    Dim myReq As REQ
                    Dim myReqLine As ReqLine

                    Dim bIsDateChanged As Boolean
                    For iCount = 0 To arr.Count - 1
                        myPO = arr(iCount)
                        bIsDateChanged = False
                        'logger.WriteVerboseLog("myPO.PurchasingBusinessUnit: " & myPO.PurchasingBusinessUnit & "myPO.POId" & myPO.POId)
                        For Each myPOLine In myPO.POLines
                            If myPOLine.Schedules.Count > 0 Then
                                For iLineCount = 0 To myPOLine.Schedules.Count - 1
                                    Dim myPOLineSched As New poLineSched
                                    myPOLineSched = myPOLine.Schedules.Item(iLineCount)
                                    If myPOLineSched.OriginalPromisedDate <> myPOLineSched.PurchaseOrderDueDate Then
                                        If Not colEmailedPOs.Contains(myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo & myPOLineSched.EmployeeID) Then
                                            'create new req class
                                            myReq = New REQ(myPOLineSched.InventoryBusinessUnit, myPOLineSched.OrderNo,
                                                            myPOLineSched.EmployeeID, myPO.POId, myPO.PurchasingBusinessUnit)

                                            colEmailedPOs.Add(myReq, myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo & myPOLineSched.EmployeeID)
                                            'Else
                                            '    myReq = colEmailedPOs.Item(myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo)
                                        End If
                                        myReqLine = New ReqLine(myPOLineSched.OrderLineNo, myPOLineSched.PurchaseOrderDueDate,
                                                                myPOLine.PurchaseOrderLineNo, myPOLine.Desc, myPOLine.ItemID,
                                                                myPOLineSched.PurchaseOrderLineScheduleNo, myPO.PurchasingBusinessUnit, myPO.POId)
                                        Try
                                            myReqLine.oldDate = myPOLineSched.OriginalPromisedDate
                                        Catch ex As Exception
                                            myReqLine.oldDate = ""
                                            logger.WriteErrorLog("Tried to Assign myReqLine property oldDate: " & ex.Message)
                                        End Try

                                        colEmailedPOs.Item(myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo & myPOLineSched.EmployeeID).ReqLines.Add(myReqLine)


                                    End If
                                Next
                            End If
                        Next
                    Next


                    logger.WriteInformationLog("Number of Orders / Reqs Requiring Emailing: " & colEmailedPOs.Count.ToString)

                    Dim sBusUnit As String
                    Dim sBusUnitIN As String
                    Dim sPO_ID As String
                    Dim iLineNBR As Integer
                    Dim iSchedNBR As Integer
                    Dim sPODueDate As String
                    Dim PO_ID As String = String.Empty

                    For iCollectionIndex As Integer = 1 To colEmailedPOs.Count
                        sBusUnit = ""
                        sPO_ID = ""
                        iLineNBR = 0
                        iSchedNBR = 0
                        sPODueDate = ""
                        sBusUnitIN = ""
                        myReq = colEmailedPOs.Item(iCollectionIndex)

                        If Not myReq.EmployeeId Is Nothing Then

                            If myReq.EmployeeId.Trim() <> "" Then
                                If sendEmailForPO(myReq) Then

                                    For Each myReqLine In myReq.ReqLines
                                        sBusUnit = myReqLine.POBusinessUnit
                                        sPO_ID = myReqLine.POID
                                        iLineNBR = myReqLine.POLine_NBR
                                        iSchedNBR = myReqLine.POLineSched_NBR
                                        sBusUnitIN = myReq.POBusinessUnit
                                        sPODueDate = myReqLine.newDate
                                        If Not IsDate(sPODueDate) Then
                                            sPODueDate = Format(Now, "MM/dd/yyyy")
                                        End If
                                        logger.WriteVerboseLog("PROCESSING Business Unit: " & sBusUnit & " PO: " & sPO_ID &
                                                                                                   "PO Line NBR: " & iLineNBR & " PO Sched Line No : " & iSchedNBR)
                                        If Not flagPOAsProcessed(sBusUnit, sPO_ID, iLineNBR, iSchedNBR, sPODueDate) Then
                                            logger.WriteErrorLog("Updating PODUEDTMON FAILED for Business Unit: " & sBusUnit & " PO: " & sPO_ID &
                                                                                                "PO Line NBR: " & iLineNBR & " PO Sched Line No : " & iSchedNBR)
                                        End If

                                    Next
                                    logger.WriteVerboseLog("///////////////////////////////////////////////////////////////////////////////////////////////////////////////")
                                Else
                                    logger.WriteErrorLog("Sending Email FAILED and item will be reprocessed for Business Unit: " & myReq.POBusinessUnit &
                                                         "  PO:" & myReqLine.POID)
                                End If
                            End If
                            If myReq.POBusinessUnit = "WAL00" Then
                                PO_ID += "'" & myReqLine.POID.Trim & "'" & ","
                            End If
                        End If
                    Next
                    If Not PO_ID = String.Empty Then
                        PO_ID = PO_ID.Substring(0, PO_ID.Length - 1)
                        Dim clsPODueDateUpdate As New PODueDateUpdate_SC()
                        clsPODueDateUpdate.PODueDateUpdate(PO_ID)
                    End If
                End If
            Else        'If cn.State = ConnectionState.Open Then
                logger.WriteErrorLog("Connnection to Oracle FAILED :: [" & m_oraCNstring & "]")
            End If      'If cn.State = ConnectionState.Open Then

            logger.WriteLog("PO Due Date Change Check Process Completed.", TraceLevel.Info)

            Try
                cn.Dispose()
                'cn.Close()
            Catch ex As Exception
            End Try

            Try
                cn.Close()
                'cn.Dispose()
            Catch ex As Exception
            Finally
                cn = Nothing
            End Try

        Catch ex1 As Exception
            logger.WriteLog("Not catched error in Timer Elapsed event: " & ex1.Message, TraceLevel.Error)
        Finally
            If cn Is Nothing Then
            Else
                If cn.State = ConnectionState.Closed Then
                Else

                    Try
                        cn.Dispose()
                        'cn.Close()
                    Catch ex As Exception
                    End Try

                    Try
                        cn.Close()
                        'cn.Dispose()
                    Catch ex As Exception
                    Finally
                        cn = Nothing
                    End Try

                End If
            End If
        End Try ' Try Catch for: If cn.State = ConnectionState.Open Then
        logger = Nothing
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  Do not modify it
    ' using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        '
        ''PODueDTChangeEmail
        ''
        'Me.ServiceName = "PODueDTChangeEmail"

    End Sub

#End Region

    Private Function InitAlertEmailMsg(ByRef m_settingsObj As Object, ByVal lvl As System.Diagnostics.TraceLevel) As MailMessage
        Dim cHdr As String = m_sCommonMsgText & "InitAlertEmailMsg: "
        Dim eml As New System.Net.Mail.MailMessage
        Dim emlNew As New System.Net.Mail.MailMessage
        Dim fromAddress As System.Net.Mail.MailAddress


        Try


            Try

                If Not (m_settingsObj.Item("notifySubject").ToString Is Nothing) Then

                    eml.Subject = m_settingsObj.Item("notifySubject").ToString.Trim
                End If
            Catch ex As Exception
            End Try

            ' get sender email address (automated)
            Try
                If Not m_settingsObj.Item("notifyFrom").ToString Is Nothing Then
                    fromAddress = New System.Net.Mail.MailAddress(m_settingsObj.Item("notifyFrom").ToString, m_settingsObj.Item("notifyFrom").ToString)
                End If
            Catch ex As Exception
                fromAddress = New System.Net.Mail.MailAddress("service.notification@sdi.com", "service.notification@sdi.com")
            End Try
            eml.From = fromAddress


            ' get email address list on whom will receives this notification
            Try
                If Not (m_settingsObj.Item("statusNotifyCount") Is Nothing) Then
                    Dim iCount As Integer
                    Dim iRecipients As Integer
                    Dim saEmail As Array
                    Dim bIsIncludeToNotify As Boolean = False
                    Dim sTo As String = ""
                    Dim addyLvl As String = ""

                    iRecipients = m_settingsObj.Item("statusNotifyCount")
                    For iCount = 1 To iRecipients
                        Try

                            saEmail = m_settingsObj.Item("statusNotify1" & iCount.ToString).ToString.Split(";")
                            If Not (saEmail(1).ToString Is Nothing) Then
                                addyLvl = "ERROR"
                                bIsIncludeToNotify = False
                                Try
                                    addyLvl = saEmail(2).ToString.Trim.ToUpper
                                Catch ex As Exception
                                    addyLvl = "ERROR"
                                End Try
                                Select Case lvl
                                    Case TraceLevel.Verbose
                                        bIsIncludeToNotify = True
                                    Case TraceLevel.Info
                                        If (addyLvl.IndexOf("VERB") > -1) Or
                                           (addyLvl.IndexOf("INFO") > -1) Then
                                            bIsIncludeToNotify = True
                                        End If
                                    Case TraceLevel.Warning
                                        If (addyLvl.IndexOf("VERB") > -1) Or
                                           (addyLvl.IndexOf("INFO") > -1) Or
                                           (addyLvl.IndexOf("WARN") > -1) Then
                                            bIsIncludeToNotify = True
                                        End If
                                    Case TraceLevel.Error
                                        If (addyLvl.IndexOf("VERB") > -1) Or
                                           (addyLvl.IndexOf("INFO") > -1) Or
                                           (addyLvl.IndexOf("WARN") > -1) Or
                                           (addyLvl.IndexOf("ERR") > -1) Then
                                            bIsIncludeToNotify = True
                                        End If
                                End Select

                            End If
                            If (saEmail(1).ToString.Trim.Length > 0) And bIsIncludeToNotify Then
                                sTo &= saEmail(1).ToString.Trim & ";"
                            End If
                            If sTo.Trim.Length > 0 Then
                                sTo = sTo.TrimEnd(";"c)
                            End If

                            eml.To.Add(sTo)

                        Catch ex As Exception
                            'MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
                        End Try
                    Next

                End If
            Catch ex As Exception
                'MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
            End Try



        Catch ex As Exception
            'MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
        End Try

        Return eml

    End Function

    Private Function sendEmailForPO(ByVal myReq As REQ) As Boolean
        Dim cHdr As String = m_sCommonMsgText & "sendEmailForPO: "

        Dim bIsSuccessful As Boolean = True
        Dim sEmailBody As String = ""
        Dim eml As New System.Net.Mail.MailMessage
        Dim sEmailTo As String
        Dim saEmailTos As Array
        Dim sOrigDueDate As String = ""

        Dim fromAddress As System.Net.Mail.MailAddress


        sEmailBody = "<HTML><HEAD><META name=GENERATOR content=""MSHTML 8.00.6001.18876""><span style=""background-color: black;""><img src='https://www.sdiezeus.com/images/SDNewLogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></span></HEAD>" &
                             "<BODY><CENTER><SPAN style=""WIDTH: 256px; FONT-FAMILY: Arial; FONT-SIZE: x-large"">SDI Marketplace</SPAN></CENTER>" &
                             "<CENTER><SPAN>SDI ZEUS -<B> Order Due Date Change</B></SPAN></CENTER>&nbsp;" &
                              "&nbsp; <DIV><P>Hello SDI Site Rep,<BR></DIV><BR>There has been a Due Date change for Order Number: <B> " & myReq.ReqId.ToString &
                              "</B><BR><BR><TABLE> <COLGROUP><COL width=""7%"" valign=""top""><COL width=""21%"" valign=""top""><COL width=""11%"" valign=""top""><COL width=""7%"" valign=""top"">" &
                              "<COL width=""36%"" valign=""top""><COL width=""9%"" valign=""top""><COL width=""9%"" valign=""top"">" &
                              "<TR><TD><U>LINE <BR>NUMBER</U></TD><TD><U>MFG -<BR> MFG ITEM NO.</U></TD><TD><U>ITEM ID</U></TD><TD><U>PO ID</U></TD><TD><U>PO LINE <BR>NUMBER</U></TD><TD><U>WORK ORDER</U></TD><TD><U>DESCRIPTION</U></TD>" &
                              "<TD><U>ORIGINAL <BR>DUE DATE</U></TD><TD><U>NEW <BR>DUE DATE</U></TD></TR><TR></TR>" & vbCrLf

        Dim wordOrder As String = String.Empty
        Dim itemID As String = String.Empty
        Dim shipTo As String = String.Empty
        For Each myLine As ReqLine In myReq.ReqLines
            Try
                Dim cmdEmpl As OleDbCommand = cn.CreateCommand
                cmdEmpl.CommandText = "SELECT intfc_l.SHIPTO_ID, intfc_l.INV_ITEM_ID, intfc_l.ISA_WORK_ORDER_NO FROM SYSADM8.ps_isa_ord_intf_lN intfc_l " & vbCrLf &
                            " WHERE intfc_l.ORDER_NO = '" & myReq.ReqId.ToString & "' " & vbCrLf &
                            " AND intfc_l.ISA_INTFC_LN = " & myLine.ReqLineNo & " "
                cmdEmpl.CommandType = CommandType.Text
                Dim rdr As OleDbDataReader = Nothing

                rdr = cmdEmpl.ExecuteReader
                If Not (rdr Is Nothing) Then

                    While rdr.Read
                        itemID = CStr(rdr("INV_ITEM_ID")).Trim.ToUpper
                        wordOrder = CStr(rdr("ISA_WORK_ORDER_NO")).Trim.ToUpper
                        shipTo = CStr(rdr("SHIPTO_ID")).Trim.ToUpper
                    End While
                End If
            Catch ex As Exception
            End Try
            Try
                sOrigDueDate = myLine.oldDate()
            Catch ex As Exception
                logger.WriteErrorLog("sendEmailForPO: tried to get property oldDate: " & ex.Message)
                sOrigDueDate = ""
            End Try
            sEmailBody &= "<TR><TD>&nbsp;" & myLine.ReqLineNo & "</TD>" &
                          "<TD>&nbsp;" & myLine.ItemID & "</TD>" &
                          "<TD>&nbsp;" & itemID & "</TD>" &
                          "<TD>&nbsp;" & myLine.POID & "</TD>" &
                          "<TD>&nbsp;" & myLine.POLine_NBR & "</TD>" &
                          "<TD>&nbsp;" & wordOrder & "</TD>" &
                          "<TD>&nbsp;" & myLine.Desc & "</TD>" &
                          "<TD>&nbsp;" & sOrigDueDate & "</TD>" &
                          "<TD>&nbsp;" & myLine.newDate & "</TD></TR>"


        Next

        sEmailBody &= vbCrLf & "</TABLE> <br><br>&nbsp;<BR>Sincerely,<BR>SDI Customer " &
                     "Care<BR>&nbsp;<BR></P></DIV><DIV>&nbsp;</DIV><DIV>&nbsp;</DIV><HR width='100%' SIZE='1'><img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' /></BODY></HTML>"

        Try

            sEmailTo = GetPOEmailAddress(myReq.BusinessUnit, myReq.EmployeeId)

            If Trim(sEmailTo) <> "" Then
                If sEmailTo = "NONE" Then
                    logger.WriteVerboseLog("Email not sent due to lack of email addresses. Order Number: " & myReq.ReqId.ToString & ".")
                    logger.WriteVerboseLog("Item will be marked sent for   " & myReq.BusinessUnit.ToString() & "  " & myReq.EmployeeId & ".")
                Else
                    saEmailTos = sEmailTo.Split(";")

                    For iCount As Integer = 0 To UBound(saEmailTos) - 1
                        If Trim(saEmailTos(iCount)) <> "" Then
                            eml.To.Add(saEmailTos(iCount).ToString)
                        End If

                    Next

                    Try
                        If Not m_settingsObj.Item("notifyFrom").ToString Is Nothing Then
                            fromAddress = New System.Net.Mail.MailAddress(m_settingsObj.Item("notifyFrom").ToString)
                        Else
                            fromAddress = New System.Net.Mail.MailAddress("service.notification@sdi.com")
                        End If
                    Catch ex As Exception
                        fromAddress = New System.Net.Mail.MailAddress("service.notification@sdi.com")
                    End Try
                    'WAL-533: Email subject lines changes for Walmart BU -->Change done by- Venkat
                    If myReq.BusinessUnit = "I0W01" OrElse myReq.POBusinessUnit = "WAL00" Then
                        eml.Subject = "Status Update - Due Date Change - Store #" & shipTo & " - WO #" & wordOrder & ""
                    Else
                        eml.Subject = "Order Due Date has Changed. Order Number: " & myReq.ReqId.ToString & ". PO_ID: " & myReq.POID.ToString & ""
                    End If

                    eml.From = fromAddress

                    Dim sCNString As String = m_oraCNstring
                    Dim strDBase As String = "PLGR"
                    If Len(sCNString) > 4 Then
                        strDBase = UCase(Right(sCNString, 4))
                    End If

                    Select Case strDBase
                        Case "STAR", "PLGR", "RPTG", "DEVL", "STST", "SUAT"
                            eml.Subject = " TEST SDI ZEUS - " & eml.Subject
                            eml.To.Clear()
                            eml.To.Add("webdev@sdi.com")
                            sEmailTo = "webdev@sdi.com"
                        Case Else

                    End Select

                    eml.IsBodyHtml = True
                    eml.Body = sEmailBody

                    Try
                        SendLogger(eml.Subject, sEmailBody, "PODUEDATECHGEMAIL", "Mail", sEmailTo, " ", "webdev@sdi.com")
                        logger.WriteVerboseLog("Order Due Date has Changed. Order Number: " & myReq.ReqId.ToString & ". PO_ID: " & myReq.POID.ToString & "")
                        bIsSuccessful = True

                    Catch ex As Exception
                        bIsSuccessful = False
                        logger.WriteVerboseLog("Error Sending Email to: " & myReq.ReqId.ToString & ". PO_ID: " & myReq.POID.ToString & "")
                    End Try
                End If
            Else
                logger.WriteVerboseLog("No Email Address Found: " & myReq.ReqId.ToString & ". PO_ID: " & myReq.POID.ToString & "")
                bIsSuccessful = False
            End If

        Catch ex As Exception
            bIsSuccessful = False
        End Try

        Return (bIsSuccessful)

    End Function

    Private Function SendUserEmail(ByVal myBusinessUnit As String, ByVal myUserID As String) As EventLogEntryType
        Dim cHdr As String = m_sCommonMsgText & "SendUserEmail: "
        Dim sSQL As String
        Dim eletReturn As EventLogEntryType = EventLogEntryType.Error

        Try
            logger.WriteVerboseLog(" SEND USER EMAIL?  GETTING INFO FOR BUS UNIT: " & myBusinessUnit & " USER: " & myUserID)
            sSQL = "" &
    "                  select isa_employee_id, isa_iol_op_value " & vbCrLf &
    "                    from ps_isa_users_privs " & vbCrLf &
    "                   where ISA_IOL_OP_NAME='EMLPODUEDT' and ISA_IOL_OP_TYPE='IOL' " & vbCrLf &
    "                            AND BUSINESS_UNIT='" & myBusinessUnit & "' AND upper(isa_employee_id)= upper('" & myUserID & "')"

            If cn.State = ConnectionState.Open Then
                Dim cmd As OleDbCommand = cn.CreateCommand
                cmd.CommandText = sSQL
                cmd.CommandType = CommandType.Text
                Dim rdr As OleDbDataReader = Nothing
                Try
                    rdr = cmd.ExecuteReader


                    If Not (rdr Is Nothing) Then
                        If rdr.HasRows Then
                            While rdr.Read

                                Try
                                    If CStr(rdr("isa_iol_op_value")).Trim = "Y" Then
                                        logger.WriteVerboseLog(" Employee " & myUserID & " is set to receive emails")
                                        eletReturn = EventLogEntryType.SuccessAudit
                                    Else
                                        logger.WriteVerboseLog(" Employee " & myUserID & " is set to NOT receive emails")
                                        eletReturn = EventLogEntryType.FailureAudit
                                    End If
                                Catch ex As Exception
                                    logger.WriteErrorLog(cHdr & " Error in inner Try Catch of 'SendUserEmail' function")
                                End Try

                            End While
                        Else  '  no rows
                            ' check is user has a Role. If Yes, read Role Details - not the privs table
                            Dim iRoleNumber As Integer = 0
                            iRoleNumber = GetUserAccessRole(myUserID, myBusinessUnit)
                            If iRoleNumber = 0 Then
                                logger.WriteVerboseLog(" Employee " & myUserID & " is NOT set in the System")
                                eletReturn = EventLogEntryType.FailureAudit
                            Else
                                'user has a Role. Get role details
                                rdr.Close()
                                rdr = Nothing
                                sSQL = "SELECT * FROM SDIX_ROLEDETAIL where ROLENUM = " & iRoleNumber & ""
                                Dim cmdEml As OleDbCommand = cn.CreateCommand
                                cmdEml.CommandText = sSQL
                                cmdEml.CommandType = CommandType.Text
                                Dim bIsPrivExists As Boolean = False

                                rdr = cmdEml.ExecuteReader

                                If Not (rdr Is Nothing) Then
                                    If rdr.HasRows() Then
                                        While rdr.Read
                                            If CStr(rdr("ALIAS_NAME")).Trim = "EMLPODUEDT" Then
                                                bIsPrivExists = True
                                                Exit While
                                            End If
                                        End While
                                    End If
                                End If
                                If bIsPrivExists Then
                                    logger.WriteVerboseLog(" Employee " & myUserID & " is set to receive emails")
                                    eletReturn = EventLogEntryType.SuccessAudit
                                Else
                                    logger.WriteVerboseLog(" Employee " & myUserID & " is set to NOT receive emails")
                                    eletReturn = EventLogEntryType.FailureAudit
                                End If
                            End If ' If iRoleNumber = 0 Then

                        End If
                    Else
                        logger.WriteErrorLog(" Employee " & myUserID & " will  NOT receive email due to error")
                        eletReturn = EventLogEntryType.Error
                    End If
                    rdr.Close()
                Catch ex As Exception
                    Try
                        rdr.Close()
                    Catch exCls As Exception

                    End Try

                    logger.WriteErrorLog(cHdr & " ERROR Executing Reader:  " & ex.Message)
                    eletReturn = EventLogEntryType.Error
                Finally
                    rdr = Nothing
                End Try
            Else
                logger.WriteErrorLog(cHdr & " Connection is not Open")
                eletReturn = EventLogEntryType.Error
            End If

        Catch ex As Exception
            logger.WriteErrorLog(cHdr & " Error in outer Try Catch of 'SendUserEmail' function")
            eletReturn = EventLogEntryType.Error
        End Try

        Return (eletReturn)
    End Function

    Public Function GetUserAccessRole(ByVal employeeID As String, ByVal businessUnit As String) As Integer

        Dim connection As New OleDbConnection(m_oraCNstring)
        Dim p_strQuery As String = ""
        Dim oleCommand As New OleDbCommand()
        Dim returnParam As Integer = 0
        Try
            p_strQuery = "SELECT NVL(ROLENUM, 0) FROM SDIX_USERS_TBL  WHERE ISA_EMPLOYEE_ID = '" + employeeID + "' AND BUSINESS_UNIT = '" + businessUnit + "'"
            oleCommand = New OleDbCommand(p_strQuery, connection)
            oleCommand.CommandTimeout = 120
            connection.Open()
            Try
                returnParam = CType(oleCommand.ExecuteScalar(), Integer)
            Catch ex32 As Exception
                returnParam = 0
            End Try
            Try
                connection.Close()
            Catch ex1 As Exception

            End Try
        Catch objException As Exception
            returnParam = 0
            If connection.State = ConnectionState.Open Then
                connection.Close()
            End If
        End Try

        Return returnParam
    End Function

    Private Function GetPOEmailAddress(ByVal myBusinessUnit As String, ByVal myEmployeeID As String) As String
        Dim cHdr As String = m_sCommonMsgText & "GetPOEmailAddress: "
        Dim sSQL As String
        Dim sEmailAddresses As String = ""
        Dim bReturn As Boolean = False
        Dim strBu3 As String = ""
        Try
            strBu3 = myBusinessUnit.Substring(2, 3)
        Catch ex As Exception
            strBu3 = ""
        End Try

        If Trim(myEmployeeID) = "" Then
            Return ""
        End If

        Try
            logger.WriteVerboseLog("GetPOEmailAddress(" & myBusinessUnit & "  " & myEmployeeID & ")")
            sSQL = "" &
    "                  SELECT DISTINCT E.BUSINESS_UNIT as BUSINESS_UNIT, E.ISA_EMPLOYEE_ID as ISA_EMPLOYEE_ID, E.ISA_EMPLOYEE_EMAIL as ISA_EMPLOYEE_EMAIL  "    '  & _
            sSQL = sSQL & "                     FROM PS_ISA_USERS_TBL E "   '   & _
            If Trim(strBu3) <> "" Then
                strBu3 = Trim(strBu3)

                sSQL = sSQL & "                    WHERE E.BUSINESS_UNIT LIKE '%" & strBu3 & "'" &
                "              AND  upper(E.ISA_EMPLOYEE_ID) =   upper('" & myEmployeeID & "')"
            Else
                sSQL = sSQL & "                    WHERE upper(E.ISA_EMPLOYEE_ID) =   upper('" & myEmployeeID & "')"
            End If


            If cn.State = ConnectionState.Open Then

                Dim cmd As OleDbCommand = cn.CreateCommand
                cmd.CommandText = sSQL
                cmd.CommandType = CommandType.Text
                Dim rdr As OleDbDataReader = Nothing

                Try
                    rdr = cmd.ExecuteReader
                Catch ex As Exception

                End Try

                If Not (rdr Is Nothing) Then
                    If rdr.HasRows Then
                        While rdr.Read

                            Try
                                Dim myreturn As EventLogEntryType
                                ' "SendUserEmail" function is 'Verify Your Rights' block IN REALITY
                                myreturn = SendUserEmail(CStr(rdr("BUSINESS_UNIT")).Trim, CStr(rdr("ISA_EMPLOYEE_ID")).Trim)
                                'myreturn = SendUserEmail(myBusinessUnit, myEmployeeID)
                                Select Case myreturn
                                    Case Is = EventLogEntryType.SuccessAudit
                                        sEmailAddresses &= CStr(rdr("ISA_EMPLOYEE_EMAIL")).Trim & ";"
                                        'sEmailAddresses &= (rdr("ISA_EMPLOYEE_EMAIL")).ToString() & ";"
                                        bReturn = True
                                    Case Is = EventLogEntryType.FailureAudit
                                        If Not bReturn Then
                                            sEmailAddresses = "NONE"
                                            bReturn = True
                                        End If
                                    Case Is = EventLogEntryType.Error
                                        If Not bReturn Then
                                            sEmailAddresses = ""
                                            bReturn = False
                                        End If
                                End Select

                            Catch ex As Exception
                                If Not bReturn Then
                                    sEmailAddresses = "NONE"
                                    bReturn = True
                                End If
                            End Try

                        End While
                    Else
                        If Not bReturn Then
                            sEmailAddresses = "NONE"
                            bReturn = True
                        End If
                    End If
                Else
                    bReturn = False
                End If

            Else
                bReturn = False
            End If

        Catch ex As Exception
            bReturn = False
        End Try
        If Not bReturn Then
            sEmailAddresses = ""
        End If
        Return (sEmailAddresses)
    End Function
    Private Function flagPOAsProcessed(ByVal myBusUnit As String, ByVal myPOID As String,
                                       ByVal myLineNBR As Integer, ByVal mySchedNBR As Integer,
                                       ByVal myPODueDate As String, Optional ByVal IsReq As Boolean = False) As Boolean
        Dim cHdr As String = m_sCommonMsgText & "flagPOAsProcessed: "
        Dim bIsSuccessful As Boolean = True

        Try

            ' update isa_poduedate table 
            If sqlUpdateISA_PODUEDTMON(myBusUnit, myPOID, myLineNBR, mySchedNBR, myPODueDate, IsReq) Then
                'logger.WriteVerboseLog(cHdr & "PODUEDTMON Sucessfully Updated")
                bIsSuccessful = True
            Else
                If sqlInsertISA_PODUEDTMON(myBusUnit, myPOID, myLineNBR, mySchedNBR, myPODueDate, IsReq) Then
                    bIsSuccessful = True

                    ' logger.WriteVerboseLog(cHdr & "Sucessfully Inserted")
                Else
                    logger.WriteErrorLog(cHdr & "Both Insert AND Update FAILED")
                    bIsSuccessful = False
                End If
            End If
        Catch ex As Exception
            bIsSuccessful = False
            logger.WriteErrorLog(cHdr & vbCrLf & ex.ToString)
        End Try

        Return (bIsSuccessful)
    End Function
    Private Function sqlInsertISA_PODUEDTMON(ByVal myBusinessUnit As String, ByVal myPOID As String,
                                       ByVal myLineNBR As Integer, ByVal mySchedNBR As Integer,
                                       ByVal myPODueDate As String, Optional ByVal IsReq As Boolean = False) As Boolean
        Dim cHdr As String = m_sCommonMsgText & "sqlInsertISA_PODUEDTMON: "
        Dim sSQL As String
        Dim bReturn As Boolean = False

        Try
            sSQL = "" &
            "                  INSERT INTO SYSADM8.PS_ISA_PODUEDTMON " & vbCrLf &
            "                  (BUSINESS_UNIT, PO_ID, LINE_NBR, SCHED_NBR, DUE_DT, ADD_DTTM, NOTIFY_DTTM ) " & vbCrLf &
            "                   VALUES " & vbCrLf &
            "                   ('" & myBusinessUnit & "', '" & myPOID & "', '" & myLineNBR.ToString & "'," &
            "                     '" & mySchedNBR & "', to_date('" & myPODueDate & "', 'mm/dd/yyyy'), SYSDATE, SYSDATE )"
            If cn.State = ConnectionState.Open Then
                Dim cmd As OleDbCommand = cn.CreateCommand
                cmd.CommandText = sSQL
                cmd.CommandType = CommandType.Text

                Try
                    Dim iRows As Integer
                    'Dim row
                    iRows = cmd.ExecuteNonQuery()

                    If iRows > 0 Then
                        bReturn = True
                        logger.WriteVerboseLog("PODUEDTMON Insert Succeeded - Inserted " & myPOID & "")
                    Else
                        bReturn = False
                        logger.WriteVerboseLog("PODUEDTMON Insert Failed : " & vbCrLf & sSQL)
                    End If

                Catch ex As Exception
                    bReturn = False
                    logger.WriteErrorLog(cHdr & "PODUEDTMON Insert Failed" & vbCrLf & sSQL)
                End Try
            Else
                bReturn = False
            End If
        Catch ex As Exception
            bReturn = False
        End Try
        Return (bReturn)
    End Function
    Private Function sqlUpdateISA_PODUEDTMON(ByVal myBusinessUnit As String, ByVal myPOID As String, _
                                           ByVal myLineNBR As Integer, ByVal mySchedNBR As Integer, _
                                           ByVal myPODueDate As String, Optional ByVal IsReq As Boolean = False) As Boolean
        Dim cHdr As String = m_sCommonMsgText & "sqlUpdateISA_PODUEDTMON: "
        Dim sSQL As String
        Dim bReturn As Boolean = False

        Try

            sSQL = "" & _
    "                  UPDATE SYSADM8.PS_ISA_PODUEDTMON " & vbCrLf & _
    "                     SET DUE_DT = to_date('" & myPODueDate & "', 'mm/dd/yyyy'), ADD_DTTM = SYSDATE, NOTIFY_DTTM = SYSDATE " & vbCrLf & _
    "                  WHERE BUSINESS_UNIT = '" & myBusinessUnit & "' AND PO_ID = '" & myPOID & "' " & vbCrLf & _
    "                    AND LINE_NBR= '" & myLineNBR & "'  AND SCHED_NBR = '" & mySchedNBR & "'"
            If cn.State = ConnectionState.Open Then
                Dim cmd As OleDbCommand = cn.CreateCommand
                cmd.CommandText = sSQL
                cmd.CommandType = CommandType.Text

                Try
                    Dim iRows As Integer
                    iRows = cmd.ExecuteNonQuery()
                    If iRows > 0 Then
                        bReturn = True
                        logger.WriteVerboseLog("PODUEDTMON Update Succeeded -  Updated " & myPOID & "")
                    Else
                        bReturn = False
                        logger.WriteVerboseLog("PODUEDTMON Update Failed - " & myPOID & "")
                    End If


                Catch ex As Exception
                    bReturn = False
                    logger.WriteErrorLog(cHdr & "PODUEDTMON Update Failed - IsReq =  " & IsReq & ": " & ex.Message & vbCrLf & sSQL)
                End Try
            Else
                bReturn = False
            End If
        Catch ex As Exception
            bReturn = False
        End Try
        Return (bReturn)
    End Function
    '// retrieve PO list to check due date change on
    Private Function sqlRetrievePOListToCheck() As String
        Dim cHdr As String = m_sCommonMsgText & "sqlRetrievePOListToCheck: "
        Dim strBuild As New StringBuilder
        Dim s As String

        Try
            s = "" & _
            " SELECT BUSINESS_UNIT, PO_ID, PO_DT, LINE_NBR, SCHED_NBR, BUSINESS_UNIT_IN, bu_nstk, ORDER_NO " & vbCrLf & _
            ", ORDER_INT_LINE_NO, " & vbCrLf & _
            " ' ' AS ISA_EMPLOYEE_ID,  " & vbCrLf & _
            "MFG_ID || ' - ' ||  MFG_ITM_ID AS ITEM_ID, DESCR254_MIXED, SHIPTO_ID, " & vbCrLf & _
            "ORIG_PROMISE_DT, DUE_DT FROM (" & vbCrLf & _
            "SELECT " & vbCrLf & _
            " B.BUSINESS_UNIT " & vbCrLf & _
            ",B.PO_ID " & vbCrLf & _
            ",B.PO_DT " & vbCrLf & _
            ",A.LINE_NBR " & vbCrLf & _
            ",A.SCHED_NBR " & vbCrLf & _
            ",A.BUSINESS_UNIT_IN " & vbCrLf & _
            ",LB.business_unit_in as bu_nstk " & vbCrLf & _
            ",LB.REQ_ID  AS ORDER_NO  " & vbCrLf & _
            ",LB.req_line_nbr AS ORDER_INT_LINE_NO " & vbCrLf & _
            ",L.MFG_ID ,L.MFG_ITM_ID " & vbCrLf & _
            ",l.DESCR254_MIXED, A.SHIPTO_ID " & vbCrLf & _
            ",MAX(trunc(A.ORIG_PROM_DT)) AS ORIG_PROMISE_DT " & vbCrLf & _
            ",MAX(trunc(A.DUE_DT)) AS DUE_DT " & vbCrLf & _
            "FROM " & vbCrLf & _
            " SYSADM8.PS_PO_LINE_SHIP A " & vbCrLf & _
            ",SYSADM8.PS_PO_HDR B " & vbCrLf & _
            ",SYSADM8.PS_PO_LINE L " & vbCrLf & _
            ",sysadm8.PS_PO_LINE_DISTRIB LB " & vbCrLf & _
            ",sysadm8.PS_REQ_LINE RL " & vbCrLf & _
            "WHERE B.PO_STATUS NOT IN ('X','C') " & vbCrLf & _
            "  AND B.PO_DT > LAST_DAY(ADD_MONTHS(SYSDATE, -12)) " & vbCrLf & _
            "  AND B.BUSINESS_UNIT = A.BUSINESS_UNIT " & vbCrLf & _
            "  AND B.PO_ID = A.PO_ID " & vbCrLf & _
            "  AND A.DUE_DT > LAST_DAY(ADD_MONTHS(SYSDATE,-3)) " & vbCrLf & _
            "  AND A.BUSINESS_UNIT = L.BUSINESS_UNIT " & vbCrLf & _
            "  AND A.PO_ID = L.PO_ID " & vbCrLf & _
            "  AND A.LINE_NBR = L.LINE_NBR " & vbCrLf & _
            "  AND A.BUSINESS_UNIT = LB.BUSINESS_UNIT " & vbCrLf & _
            "  And A.PO_ID = LB.PO_ID " & vbCrLf & _
            "  AND A.LINE_NBR = LB.LINE_NBR " & vbCrLf & _
            "  and lb.business_unit_REQ = rl.business_unit " & vbCrLf & _
            "  and lb.req_id = rl.req_id " & vbCrLf & _
            "  AND lb.REQ_line_nbr = rl.LINE_NBR " & vbCrLf & _
            "  AND l.unit_of_measure <> 'DO' " & vbCrLf & _
            "  AND A.CANCEL_STATUS <> 'X' " & vbCrLf & _
            "  AND L.CANCEL_STATUS <> 'X' " & vbCrLf & _
            "   "
            '"  AND A.BUSINESS_UNIT_IN <> ' ' " & vbCrLf & _
            '"  AND B.BUSINESS_UNIT = 'ISA00' " & vbCrLf & _
            '"  AND B.PO_ID = '0001989271' "
            ' VR 07/03/13 changed lines (original)
            ' 1272  '  "  and lb.business_unit = rl.business_unit " & vbCrLf & _
            ' 1274  '  "  AND lb.line_nbr = rl.LINE_NBR " & vbCrLf & _
            strBuild.Insert(0, s, 1)
            If m_siteList.Count > 0 Then
                Dim myTempSite As New sdiSite
                s = " AND substr(A.SHIPTO_ID, 0, 5) IN ("
                For iCount As Integer = 0 To m_siteList.Count - 1
                    'myTempSite = m_siteList.Item(iCount).
                    s = s & "'" & m_siteList.Item(iCount).Id.ToString & "', "

                Next
                s = s.Trim.TrimEnd(",")
                s = s & " ) "
                strBuild.Append(s)
            End If
            ''  "                   ' AND DTMON.NOTIFY_DTTM IS NOT NULL " & vbCrLf & _
            s = ""
            s = " AND NOT EXISTS ( " & vbCrLf & _
            "                  SELECT 'X' " & vbCrLf & _
            "                  FROM SYSADM8.PS_ISA_PODUEDTMON DTMON " & vbCrLf & _
            "                  WHERE DTMON.BUSINESS_UNIT = A.BUSINESS_UNIT " & vbCrLf & _
            "                    AND DTMON.PO_ID = A.PO_ID " & vbCrLf & _
            "                    AND DTMON.LINE_NBR = A.LINE_NBR " & vbCrLf & _
            "                    AND DTMON.SCHED_NBR = A.SCHED_NBR " & vbCrLf & _
            "                    AND (DTMON.NOTIFY_DTTM IS NOT NULL  and trunc(A.DUE_DT) = trunc(dtmon.due_dt)) " & vbCrLf & _
                        "                 ) " & vbCrLf & _
                        " and not exists (Select 'X' FROM SYSADM8.PS_RECV_LN_SHIP RCV WHERE A.BUSINESS_UNIT = RCV.BUSINESS_UNIT " & vbCrLf & _
                                          "AND A.PO_ID = RCV.PO_ID  AND A.LINE_NBR = RCV.LINE_NBR ) " & vbCrLf & _
            "GROUP BY " & vbCrLf & _
            " B.BUSINESS_UNIT " & vbCrLf & _
            ",B.PO_ID " & vbCrLf & _
            ",B.PO_DT " & vbCrLf & _
            ",A.LINE_NBR " & vbCrLf & _
            ",A.SCHED_NBR " & vbCrLf & _
            ",A.BUSINESS_UNIT_IN " & vbCrLf & _
            ", LB.business_unit_in " & vbCrLf & _
            ",A.ORDER_NO " & vbCrLf & _
            ",A.ORDER_INT_LINE_NO " & vbCrLf & _
            ",LB.REQ_ID    ,LB.req_line_nbr  " & _
            ",L.MFG_ID ,L.MFG_ITM_ID " & vbCrLf & _
            ",l.DESCR254_MIXED, A.SHIPTO_ID " & vbCrLf & _
            "ORDER BY B.BUSINESS_UNIT, B.PO_ID, A.LINE_NBR, A.SCHED_NBR " & vbCrLf & _
            ") WHERE ((to_char(ORIG_PROMISE_DT) <> to_char(DUE_DT)) or(ORIG_PROMISE_DT is null and  DUE_DT is not null) ) " & vbCrLf & _
            " and due_dt >= sysdate  "
            strBuild.Append(s)
            s = ""
            s = strBuild.ToString
        Catch ex As Exception
            s = ""
            logger.WriteErrorLog(cHdr & vbCrLf & ex.ToString)
        End Try
        '   logger.WriteVerboseLog(s)   '  and SHIPTO_ID LIKE '%EL_PAS%'
        Return (s)
    End Function


    Public Shared Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim objException As String
            Dim objExceptionTrace As String

            SDIEmailService.EmailUtilityServices(MailType, "SDIExchange@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub


End Class
