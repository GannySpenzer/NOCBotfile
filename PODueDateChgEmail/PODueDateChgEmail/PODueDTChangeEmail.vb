
Imports System.Data
Imports System.Data.OleDb
Imports System.Xml
Imports System.IO
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Reflection
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
    Private Const oraCN_default_DB As String = "Data Source=RPTG"

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
    Private m_oraCNstring As String = "" & _
                                      oraCN_default_provider & _
                                      oraCN_default_creden & _
                                      oraCN_default_DB & _
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

        ''Add any initialization after the InitializeComponent() call
        'InitService()
    End Sub

    ''UserService overrides dispose to clean up the component list.
    'Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
    '    If disposing Then
    '        If Not (components Is Nothing) Then
    '            components.Dispose()
    '        End If
    '    End If
    '    MyBase.Dispose(disposing)
    'End Sub

    Public Sub Main2()

        Dim sMyDbase As String = ""
        Try
            sMyDbase = My.Settings("default_DB").ToString.Trim
        Catch ex As Exception
            sMyDbase = ""
        End Try
        If (sMyDbase.Length > 0) Then
            m_oraCNstring = "" & _
                    oraCN_default_provider & _
                    oraCN_default_creden & _
                    sMyDbase & _
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

            logger.WriteVerboseLog(cHdr & "Connection attempted: " & cn.ConnectionString)

            If cn.State = ConnectionState.Open Then

                logger.WriteVerboseLog(cHdr & "Successful connection to " & cn.ConnectionString)

                If bResult Then
                    logger.WriteVerboseLog(cHdr & "Processing Req Due Date Changes Succeeded. Count: " & clsReqDateChange.ReqCollection.Count.ToString)
                    For Each tempReq As REQ In clsReqDateChange.ReqCollection
                        'send each req to class for email and update poduedtmon

                        If sendEmailForPO(tempReq) Then
                            For Each myline As ReqLine In tempReq.ReqLines
                                flagPOAsProcessed(tempReq.BusinessUnit, tempReq.ReqId, myline.ReqLineNo, _
                                             myline.POLineSched_NBR, myline.newDate, True)

                            Next
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
                        'Try
                        '    currPOLn.EmployeeID = CInt(rdr("ISA_EMPLOYEE_ID"))
                        'Catch ex As Exception
                        '    logger.WriteVerboseLog(("rdr employee id error: " & ex.Message))
                        ' End Try

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
                        Try
                            currPOLnSched.EmployeeID = CStr(rdr("ISA_EMPLOYEE_ID"))
                        Catch ex As Exception
                            logger.WriteErrorLog(("rdr employee ID error: " & ex.Message))
                        End Try

                        If currPO.PurchasingBusinessUnit.Length > 0 And _
                           currPO.POId.Length > 0 And _
                           currPOLn.PurchaseOrderLineNo > 0 And _
                           currPOLnSched.PurchaseOrderLineScheduleNo > 0 And _
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
                                        ' add this schedule to retrieved PO line
                                        '   regardless whether this is a duplicate schedule for the PO line
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
                                            myReq = New REQ(myPOLineSched.InventoryBusinessUnit, myPOLineSched.OrderNo, _
                                                            myPOLineSched.EmployeeID, myPO.POId, myPO.PurchasingBusinessUnit)

                                            colEmailedPOs.Add(myReq, myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo & myPOLineSched.EmployeeID)
                                            'Else
                                            '    myReq = colEmailedPOs.Item(myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo)
                                        End If
                                        myReqLine = New ReqLine(myPOLineSched.OrderLineNo, myPOLineSched.PurchaseOrderDueDate, _
                                                                myPOLine.PurchaseOrderLineNo, myPOLine.Desc, myPOLine.ItemID, _
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
                                'If bIsDateChanged Then
                                '    Exit For
                                'End If
                            End If
                        Next
                        'If bIsDateChanged Then
                        '    'add to collection of PO Lines key order number/ employee number
                        '    If Not colEmailedPOs.Contains(myPO.PurchasingBusinessUnit & myPO.POId) Then
                        '        'check email collection
                        '        ' If Not in there add it
                        '        colEmailedPOs.Add(myPO, myPO.PurchasingBusinessUnit & myPO.POId)
                        '    End If
                        'End If

                    Next


                    logger.WriteInformationLog("Number of Orders / Reqs Requiring Emailing: " & colEmailedPOs.Count.ToString)

                    Dim sBusUnit As String
                    Dim sBusUnitIN As String
                    Dim sPO_ID As String
                    Dim iLineNBR As Integer
                    Dim iSchedNBR As Integer
                    Dim sPODueDate As String


                    For iCollectionIndex As Integer = 1 To colEmailedPOs.Count
                        sBusUnit = ""
                        sPO_ID = ""
                        iLineNBR = 0
                        iSchedNBR = 0
                        sPODueDate = ""
                        sBusUnitIN = ""
                        myReq = colEmailedPOs.Item(iCollectionIndex)

                        If sendEmailForPO(myReq) Then

                            'sBusUnit = myReq.POBusinessUnit
                            'sPO_ID = myReq.POID


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
                                logger.WriteVerboseLog("PROCESSING Business Unit: " & sBusUnit & " PO: " & sPO_ID & _
                                                                                           "PO Line NBR: " & iLineNBR & " PO Sched Line No : " & iSchedNBR)
                                If Not flagPOAsProcessed(sBusUnit, sPO_ID, iLineNBR, iSchedNBR, sPODueDate) Then
                                    logger.WriteErrorLog("Updating PODUEDTMON FAILED for Business Unit: " & sBusUnit & " PO: " & sPO_ID & _
                                                                                        "PO Line NBR: " & iLineNBR & " PO Sched Line No : " & iSchedNBR)
                                End If

                            Next

                        Else
                            logger.WriteErrorLog("Sending Email FAILED and item will be reprocessed for Business Unit: " & myPO.PurchasingBusinessUnit & _
                                                 "  PO:" & myPO.POId)
                        End If


                    Next
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

    'Protected Overrides Sub OnStart(ByVal args() As String)
    '    Dim cHdr As String = m_sCommonMsgText & "OnStart: "
    '    Try
    '        '  load config file

    '        'used for debuging 
    '        'System.Threading.Thread.Sleep(30000)


    '        MyBase.EventLog.WriteEntry(message:=cHdr & "loading " & m_configFile & " config file...", type:=EventLogEntryType.Information)

    '        m_logLevel = TraceLevel.Verbose
    '        Try

    '            m_settingsObj = Configuration.ConfigurationManager.GetSection("appSettings")

    '            'ORACLE CONNECTION 
    '            Try
    '                If Not (m_settingsObj.Item("oraCNstring").ToString Is Nothing) Then
    '                    m_oraCNstring = m_settingsObj.Item("oraCNstring").ToString
    '                End If
    '            Catch ex As Exception
    '            End Try

    '            '' log level
    '            If Not (m_settingsObj.Item("settingLogLevel").ToString Is Nothing) Then
    '                Dim sLvl As String = m_settingsObj.Item("settingLogLevel").ToString.Trim.ToUpper
    '                If sLvl.IndexOf("VERB") > -1 Then
    '                    m_logLevel = TraceLevel.Verbose
    '                ElseIf sLvl.IndexOf("INFO") > -1 Then
    '                    m_logLevel = TraceLevel.Info
    '                ElseIf sLvl.IndexOf("WARN") > -1 Then
    '                    m_logLevel = TraceLevel.Warning
    '                ElseIf sLvl.IndexOf("OFF") > -1 Then
    '                    m_logLevel = TraceLevel.Off
    '                Else
    '                    m_logLevel = TraceLevel.Error
    '                End If
    '            End If
    '        Catch ex As Exception

    '        End Try

    '        '' log path override
    '        Try
    '            If Not (m_settingsObj.Item("logPathOverride").ToString Is Nothing) Then

    '                Dim pathOverride As String = m_settingsObj.Item("logPathOverride").ToString.Trim
    '                If pathOverride.Length > 0 Then
    '                    m_logPath = pathOverride
    '                End If
    '            End If

    '        Catch ex As Exception
    '        End Try

    '        ' this will make sure that our path ends in "\"
    '        m_logPath = m_logPath.TrimEnd("\"c) & "\"

    '        '' processing mode
    '        Try
    '            If Not (m_settingsObj.Item("sitesMode") Is Nothing) Then
    '                Dim iCount As Integer
    '                Dim iSites As Integer
    '                Dim saSite As Array
    '                Dim sMode As String

    '                sMode = m_settingsObj.Item("sitesMode").ToString.Trim.ToUpper
    '                Select Case sMode
    '                    Case "LIST"
    '                        m_siteListMode = processMode.List
    '                    Case "ALL"
    '                        m_siteListMode = processMode.All
    '                End Select
    '                If m_siteListMode = processMode.List Then
    '                    iSites = m_settingsObj.Item("siteListCount")
    '                    For iCount = 1 To iSites
    '                        Dim site As sdiSite = Nothing
    '                        Try
    '                            site = New sdiSite
    '                            saSite = m_settingsObj.Item("site" & iCount.ToString).ToString.Split(";")

    '                            If Not ((saSite(0).ToString Is Nothing) Or (saSite(1).ToString Is Nothing)) Then
    '                                site.Id = saSite(0).ToString.TrimEnd(";")
    '                                site.Name = saSite(1).ToString.Trim

    '                                If site.Id.Length > 0 Then
    '                                    m_siteList.Add(site)
    '                                End If
    '                            End If

    '                        Catch ex As Exception
    '                            MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
    '                        End Try
    '                    Next
    '                End If

    '            End If
    '        Catch ex As Exception

    '        End Try

    '        m_tmrChk = New System.Timers.Timer(1800000) ' DEFAULT IS 30MINUTES OR 1800 SECS OR 1800000 MILLISECONDS
    '        m_tmrChk.Enabled = False

    '        ' search for timer interval property within the config file and use it
    '        ' still using "secs" unit and doesn't have code to compute for different unit (ie., mins, milliseconds,...)
    '        Try
    '            If Not (m_settingsObj.Item("timerInterval").ToString.Trim Is Nothing) Then
    '                Dim nInterval As Integer = CType(m_settingsObj.Item("timerInterval").ToString.Trim, Integer)
    '                If nInterval > 0 Then
    '                    m_normalChkTimeElapse = (nInterval * 1000)
    '                    'm_tmrChk.Interval = (nInterval * 1000)
    '                End If
    '            End If
    '        Catch ex As Exception
    '        End Try

    '        m_emlAlert = InitAlertEmailMsg(m_settingsObj, TraceLevel.Info)

    '        ' send notification that this service starts successfully

    '        'Dim myClient As New System.Net.Mail.SmtpClient
    '        m_emlAlert.Body = "Service STARTED at " & m_sMachineName & " " & System.DateTime.Now.ToString
    '        m_emlAlert.Priority = MailPriority.Normal
    '        '' myClient.Host = "smtp.sdi.com" 'cptest.isacs.com"
    '        'myClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis
    '        ''   myClient.UseDefaultCredentials = True

    '        'myClient.Send(m_emlAlert)

    '        UpdEmailOut.UpdEmailOut.UpdEmailOut(m_emlAlert.Subject, "service.notification@sdi.com", "Michael.Randall@sdi.com", "", "webdev@sdi.com", "N", m_emlAlert.Body, cn)

    '        'myClient = Nothing

    '        m_emlAlert = Nothing

    '        If m_normalChkTimeElapse > 1 Then
    '            m_tmrChk.Interval = m_normalChkTimeElapse

    '        Else
    '            m_tmrChk.Interval = 1800000  'DEFAULT IS 30MINUTES OR 1800 SECS OR 1800000 MILLISECONDS
    '        End If
    '        ' start/enable our timer object


    '        m_tmrChk.Start()


    '    Catch ex As Exception
    '        MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
    '    End Try
    'End Sub

    'Protected Overrides Sub OnStop()
    '    Dim cHdr As String = m_sCommonMsgText & "OnStop: "
    '    Try

    '        ' stop the timer object
    '        m_tmrChk.Stop()

    '        ' check for the instance of the alert email message, create if needed

    '        If m_emlAlert Is Nothing Then
    '            m_emlAlert = InitAlertEmailMsg(m_settingsObj, TraceLevel.Info)
    '        End If

    '        ' insert the body of the message and send the message

    '        m_emlAlert.Body = "Service STOPPED at " & m_sMachineName & " " & System.DateTime.Now.ToString
    '        m_emlAlert.Priority = MailPriority.High

    '        'Dim myClient As New System.Net.Mail.SmtpClient
    '        ''  myClient.Host = "localhost"
    '        ''myClient.UseDefaultCredentials = True
    '        'myClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis
    '        'myClient.Send(m_emlAlert)

    '        'myClient = Nothing

    '        UpdEmailOut.UpdEmailOut.UpdEmailOut(m_emlAlert.Subject, "service.notification@sdi.com", "Michael.Randall@sdi.com", "", "webdev@sdi.com", "N", m_emlAlert.Body, cn)

    '        m_emlAlert = Nothing
    '        m_tmrChk.Close()
    '        m_tmrChk.Dispose()

    '        m_xmlConfig = Nothing

    '    Catch ex As Exception
    '        MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
    '    End Try
    'End Sub

    'Protected Overrides Sub OnShutdown()
    '    Dim cHdr As String = m_sCommonMsgText & "OnShutdown: "
    '    Try
    '        ' stop the timer object
    '        m_tmrChk.Stop()

    '        ' check for the instance of the alert email message, create if needed


    '        If m_emlAlert Is Nothing Then
    '            m_emlAlert = InitAlertEmailMsg(m_settingsObj, TraceLevel.Info)
    '        End If

    '        ' insert the body of the message and send the message
    '        m_emlAlert.Body = "Service receives shutdown message from host." & vbCrLf & _
    '                          m_sMachineName & " " & System.DateTime.Now.ToString
    '        m_emlAlert.Priority = MailPriority.High

    '        'Dim myClient As New System.Net.Mail.SmtpClient
    '        '' myClient.Host = "localhost"
    '        'myClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis
    '        'myClient.Send(m_emlAlert)

    '        UpdEmailOut.UpdEmailOut.UpdEmailOut(m_emlAlert.Subject, "service.notification@sdi.com", "Michael.Randall@sdi.com", "", "webdev@sdi.com", "N", m_emlAlert.Body, cn)

    '        'myClient = Nothing
    '        m_emlAlert = Nothing

    '    Catch ex As Exception
    '        'MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
    '    End Try
    'End Sub

    'Private Sub InitService()

    '    '' add code to initialize objects that will exist with the duration of this service itself


    '    'uncomment this code to debug.
    '    'Dim myArgs(1) As Array

    '    'OnStart(myArgs(1))
    '    'm_logLevel = TraceLevel.Verbose

    'End Sub


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
                                        If (addyLvl.IndexOf("VERB") > -1) Or _
                                           (addyLvl.IndexOf("INFO") > -1) Then
                                            bIsIncludeToNotify = True
                                        End If
                                    Case TraceLevel.Warning
                                        If (addyLvl.IndexOf("VERB") > -1) Or _
                                           (addyLvl.IndexOf("INFO") > -1) Or _
                                           (addyLvl.IndexOf("WARN") > -1) Then
                                            bIsIncludeToNotify = True
                                        End If
                                    Case TraceLevel.Error
                                        If (addyLvl.IndexOf("VERB") > -1) Or _
                                           (addyLvl.IndexOf("INFO") > -1) Or _
                                           (addyLvl.IndexOf("WARN") > -1) Or _
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

    'Public Sub m_tmrChk_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_tmrChk.Elapsed
    '    Dim cHdr As String = m_sCommonMsgText & "m_tmrChk_Elapsed: "
    '    Try
    '        ' stop the timer since this process may take time to accomplish
    '        m_tmrChk.Stop()
    '        Dim myDay As DayOfWeek = Now.DayOfWeek
    '        Dim myHour As Integer = Now.Hour
    '        If myDay <> DayOfWeek.Saturday And myDay <> DayOfWeek.Sunday Then
    '            If myHour > 6 And myHour < 18 Then

    '                Dim logFile As String = m_logPath & Now.ToString("yyyyMMddHHmmss") & ".txt"
    '                logger = New appLogger(logFile, m_logLevel)


    '                logger.WriteLog("PO Due Date Change Check Process Started", TraceLevel.Info)
    '                Dim colEmailedPOs As New Collection

    '                cn = New OleDbConnection(m_oraCNstring)

    '                Dim clsReqDateChange As New ReqDueDateChange(m_oraCNstring)

    '                clsReqDateChange.Logger = logger

    '                Dim bResult As Boolean = False

    '                bResult = clsReqDateChange.ProcessReqDueDateChanges()


    '                Try

    '                    cn.Open()

    '                    logger.WriteVerboseLog(cHdr & "Connection attempted: " & cn.ConnectionString)

    '                    If cn.State = ConnectionState.Open Then

    '                        logger.WriteVerboseLog(cHdr & "Successful connection to " & cn.ConnectionString)

    '                        If bResult Then
    '                            'Debug.Print(clsReqDateChange.ReqCollection.Count.ToString)
    '                            logger.WriteVerboseLog(cHdr & "Processing Req Due Date Changes Succeeded. Count: " & clsReqDateChange.ReqCollection.Count.ToString)
    '                            For Each tempReq As REQ In clsReqDateChange.ReqCollection
    '                                'send each req to class for email and update poduedtmon

    '                                If sendEmailForPO(tempReq) Then
    '                                    For Each myline As ReqLine In tempReq.ReqLines
    '                                        flagPOAsProcessed(tempReq.BusinessUnit, tempReq.ReqId, myline.ReqLineNo, _
    '                                                     myline.POLineSched_NBR, myline.newDate, True)

    '                                    Next
    '                                End If

    '                            Next

    '                            logger.WriteVerboseLog(cHdr & "Processing Reqs Due Date Changes EMAIL Complete")

    '                        Else
    '                            'logger.WriteVerboseLog(cHdr & "Processing Req Due Date Changes FAILED")
    '                            logger.WriteErrorLog(cHdr & "Processing Req Due Date Changes FAILED")

    '                        End If

    '                        'Should we do something with this object: clsReqDateChange?
    '                        ' not done yet

    '                        'process reqs first
    '                        logger.WriteVerboseLog(cHdr & "Processing Req Due Date Changes")

    '                        Dim arr As New ArrayList
    '                        '-- get available PO to check

    '                        Dim cmd As OleDbCommand = cn.CreateCommand
    '                        cmd.CommandText = sqlRetrievePOListToCheck()
    '                        cmd.CommandType = CommandType.Text
    '                        Dim rdr As OleDbDataReader = Nothing
    '                        Try
    '                            rdr = cmd.ExecuteReader
    '                        Catch ex As Exception
    '                        End Try
    '                        If Not (rdr Is Nothing) Then
    '                            Dim currPO As po = Nothing
    '                            Dim currPOLn As poLine = Nothing
    '                            Dim currPOLnSched As poLineSched = Nothing
    '                            logger.WriteVerboseLog("rdr is not Nothing")

    '                            While rdr.Read
    '                                currPO = New po
    '                                Try
    '                                    currPO.PurchasingBusinessUnit = CStr(rdr("BUSINESS_UNIT")).Trim.ToUpper
    '                                    If currPO.PurchasingBusinessUnit Is Nothing Then
    '                                        currPO.PurchasingBusinessUnit = ""
    '                                    End If
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog("rdr BUSINESS_UNIT error: " & ex.Message)
    '                                End Try
    '                                Try
    '                                    currPO.POId = CStr(rdr("PO_ID")).Trim.ToUpper
    '                                    If currPO.Id Is Nothing Then
    '                                        currPO.POId = ""
    '                                    End If
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog("rdr PO_ID error: " & ex.Message)
    '                                End Try
    '                                Try
    '                                    currPO.PurchaseOrderDate = CDate(rdr("PO_DT")).ToString
    '                                    If currPO.PurchaseOrderDate Is Nothing Then
    '                                        currPO.PurchaseOrderDate = ""
    '                                        'Else
    '                                        '    currPO.PurchaseOrderDate = currPO.PurchaseOrderDate
    '                                    End If
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog("rdr po date error: " & ex.Message)
    '                                End Try
    '                                currPOLn = New poLine(currPO)
    '                                currPO.POLines.Add(currPOLn)
    '                                Try
    '                                    currPOLn.PurchaseOrderLineNo = CInt(rdr("LINE_NBR"))
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog(("rdr purchase order LINE_NBR error: " & ex.Message))
    '                                End Try
    '                                Try
    '                                    currPOLn.Desc = CStr(rdr("DESCR254_MIXED"))
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog(("rdr Desc  error: " & ex.Message))
    '                                End Try
    '                                Try
    '                                    currPOLn.ItemID = CStr(rdr("ITEM_ID"))

    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog(("rdr ITEM_ID error: " & ex.Message))
    '                                End Try
    '                                'Try
    '                                '    currPOLn.EmployeeID = CInt(rdr("ISA_EMPLOYEE_ID"))
    '                                'Catch ex As Exception
    '                                '    logger.WriteVerboseLog(("rdr employee id error: " & ex.Message))
    '                                ' End Try

    '                                currPOLnSched = New poLineSched(currPOLn)
    '                                currPOLn.Schedules.Add(currPOLnSched)
    '                                Try
    '                                    currPOLnSched.PurchaseOrderLineScheduleNo = CInt(rdr("SCHED_NBR"))
    '                                Catch ex As Exception
    '                                End Try
    '                                Try
    '                                    currPOLnSched.InventoryBusinessUnit = CStr(rdr("BUSINESS_UNIT_IN")).Trim.ToUpper
    '                                    If currPOLnSched.InventoryBusinessUnit Is Nothing Then
    '                                        currPOLnSched.InventoryBusinessUnit = ""
    '                                    Else
    '                                        If currPOLnSched.InventoryBusinessUnit = "" Then
    '                                            currPOLnSched.InventoryBusinessUnit = CStr(rdr("bu_nstk")).Trim.ToUpper
    '                                            If currPOLnSched.InventoryBusinessUnit Is Nothing Then
    '                                                currPOLnSched.InventoryBusinessUnit = ""
    '                                            End If
    '                                        End If
    '                                    End If
    '                                Catch ex As Exception
    '                                End Try
    '                                Try
    '                                    currPOLnSched.PurchaseOrderDueDate = CStr(rdr("DUE_DT")).Trim
    '                                    If currPOLnSched.PurchaseOrderDueDate Is Nothing Then
    '                                        currPOLnSched.PurchaseOrderDueDate = ""
    '                                        'Else
    '                                        '    currPOLnSched.PurchaseOrderDueDate = Format(currPOLnSched.PurchaseOrderDueDate, "MM/dd/yyyy")
    '                                    End If
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog("rdr purchase due date error: " & ex.Message)
    '                                End Try
    '                                Try
    '                                    currPOLnSched.OriginalPromisedDate = CStr(rdr("ORIG_PROMISE_DT")).Trim
    '                                    If currPOLnSched.OriginalPromisedDate Is Nothing Then
    '                                        currPOLnSched.OriginalPromisedDate = ""
    '                                        'Else
    '                                        '    currPOLnSched.OriginalPromisedDate = Format(currPOLnSched.OriginalPromisedDate, "MM/dd/yyyy")
    '                                    End If
    '                                Catch ex As Exception
    '                                    currPOLnSched.OriginalPromisedDate = ""
    '                                    logger.WriteErrorLog("rdr OriginalPromisedDate error: " & ex.Message)
    '                                End Try

    '                                Try
    '                                    currPOLnSched.OrderNo = CStr(rdr("ORDER_NO"))
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog(("rdr ORDER_NO  error: " & ex.Message))
    '                                End Try
    '                                Try
    '                                    currPOLnSched.OrderLineNo = CStr(rdr("ORDER_INT_LINE_NO"))
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog(("rdr order line No error: " & ex.Message))
    '                                End Try
    '                                Try
    '                                    currPOLnSched.EmployeeID = CStr(rdr("ISA_EMPLOYEE_ID"))
    '                                Catch ex As Exception
    '                                    logger.WriteErrorLog(("rdr employee ID error: " & ex.Message))
    '                                End Try
    '                                'Try
    '                                '    currPOLnSched.OrderNo = CStr(rdr("ORDER_NO")).Trim
    '                                '    If currPOLnSched.OrderNo Is Nothing Then
    '                                '        currPOLnSched.OrderNo = ""
    '                                '    End If
    '                                'Catch ex As Exception
    '                                'End Try
    '                                'Try
    '                                '    currPOLnSched.OrderLineNo = CInt(rdr("ORDER_INT_LINE_NO"))
    '                                'Catch ex As Exception
    '                                'End Try
    '                                If currPO.PurchasingBusinessUnit.Length > 0 And _
    '                                   currPO.POId.Length > 0 And _
    '                                   currPOLn.PurchaseOrderLineNo > 0 And _
    '                                   currPOLnSched.PurchaseOrderLineScheduleNo > 0 And _
    '                                   IsDate(currPOLnSched.PurchaseOrderDueDate) Then
    '                                    Dim bIsFound As Boolean = False
    '                                    For Each o As po In arr
    '                                        If o.Id = currPO.Id Then
    '                                            Dim ln As poLine = o.GetPOLine(currPOLn)
    '                                            If (ln Is Nothing) Then
    '                                                ' add the po line
    '                                                currPOLn.ParentPO = o
    '                                                o.POLines.Add(currPOLn)
    '                                            Else
    '                                                ' add this schedule to retrieved PO line
    '                                                '   regardless whether this is a duplicate schedule for the PO line
    '                                                currPOLnSched.ParentPOLine = ln
    '                                                ln.Schedules.Add(currPOLnSched)
    '                                            End If
    '                                            bIsFound = True
    '                                            Exit For
    '                                        End If
    '                                    Next
    '                                    If Not bIsFound Then
    '                                        arr.Add(currPO)
    '                                    End If

    '                                End If
    '                            End While
    '                        End If
    '                        Try
    '                            rdr.Close()
    '                        Catch ex As Exception
    '                        Finally
    '                            rdr = Nothing
    '                        End Try
    '                        Try
    '                            cmd.Dispose()
    '                        Catch ex As Exception
    '                        Finally
    '                            cmd = Nothing
    '                        End Try
    '                        '-- process POs on the list
    '                        logger.WriteInformationLog("Number of PO's Available for Processing: " & arr.Count.ToString)
    '                        If arr.Count > 0 Then
    '                            Dim iCount As Integer
    '                            Dim iLineCount As Integer
    '                            Dim myPO As po
    '                            Dim myPOLine As poLine
    '                            Dim myReq As REQ
    '                            Dim myReqLine As ReqLine

    '                            Dim bIsDateChanged As Boolean
    '                            For iCount = 0 To arr.Count - 1
    '                                myPO = arr(iCount)
    '                                bIsDateChanged = False
    '                                'logger.WriteVerboseLog("myPO.PurchasingBusinessUnit: " & myPO.PurchasingBusinessUnit & "myPO.POId" & myPO.POId)
    '                                For Each myPOLine In myPO.POLines
    '                                    If myPOLine.Schedules.Count > 0 Then
    '                                        For iLineCount = 0 To myPOLine.Schedules.Count - 1
    '                                            Dim myPOLineSched As New poLineSched
    '                                            myPOLineSched = myPOLine.Schedules.Item(iLineCount)
    '                                            If myPOLineSched.OriginalPromisedDate <> myPOLineSched.PurchaseOrderDueDate Then
    '                                                If Not colEmailedPOs.Contains(myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo & myPOLineSched.EmployeeID) Then
    '                                                    'create new req class
    '                                                    myReq = New REQ(myPOLineSched.InventoryBusinessUnit, myPOLineSched.OrderNo, _
    '                                                                    myPOLineSched.EmployeeID, myPO.POId, myPO.PurchasingBusinessUnit)

    '                                                    colEmailedPOs.Add(myReq, myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo & myPOLineSched.EmployeeID)
    '                                                    'Else
    '                                                    '    myReq = colEmailedPOs.Item(myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo)
    '                                                End If
    '                                                myReqLine = New ReqLine(myPOLineSched.OrderLineNo, myPOLineSched.PurchaseOrderDueDate, _
    '                                                                        myPOLine.PurchaseOrderLineNo, myPOLine.Desc, myPOLine.ItemID, _
    '                                                                        myPOLineSched.PurchaseOrderLineScheduleNo, myPO.PurchasingBusinessUnit, myPO.POId)
    '                                                Try
    '                                                    myReqLine.oldDate = myPOLineSched.OriginalPromisedDate
    '                                                Catch ex As Exception
    '                                                    myReqLine.oldDate = ""
    '                                                    logger.WriteErrorLog("Tried to Assign myReqLine property oldDate: " & ex.Message)
    '                                                End Try

    '                                                colEmailedPOs.Item(myPO.PurchasingBusinessUnit & myPOLineSched.OrderNo & myPOLineSched.EmployeeID).ReqLines.Add(myReqLine)


    '                                            End If
    '                                        Next
    '                                        'If bIsDateChanged Then
    '                                        '    Exit For
    '                                        'End If
    '                                    End If
    '                                Next
    '                                'If bIsDateChanged Then
    '                                '    'add to collection of PO Lines key order number/ employee number
    '                                '    If Not colEmailedPOs.Contains(myPO.PurchasingBusinessUnit & myPO.POId) Then
    '                                '        'check email collection
    '                                '        ' If Not in there add it
    '                                '        colEmailedPOs.Add(myPO, myPO.PurchasingBusinessUnit & myPO.POId)
    '                                '    End If
    '                                'End If

    '                            Next


    '                            logger.WriteInformationLog("Number of Orders / Reqs Requiring Emailing: " & colEmailedPOs.Count.ToString)

    '                            Dim sBusUnit As String
    '                            Dim sBusUnitIN As String
    '                            Dim sPO_ID As String
    '                            Dim iLineNBR As Integer
    '                            Dim iSchedNBR As Integer
    '                            Dim sPODueDate As String


    '                            For iCollectionIndex As Integer = 1 To colEmailedPOs.Count
    '                                sBusUnit = ""
    '                                sPO_ID = ""
    '                                iLineNBR = 0
    '                                iSchedNBR = 0
    '                                sPODueDate = ""
    '                                sBusUnitIN = ""
    '                                myReq = colEmailedPOs.Item(iCollectionIndex)

    '                                If sendEmailForPO(myReq) Then

    '                                    'sBusUnit = myReq.POBusinessUnit
    '                                    'sPO_ID = myReq.POID


    '                                    For Each myReqLine In myReq.ReqLines
    '                                        sBusUnit = myReqLine.POBusinessUnit
    '                                        sPO_ID = myReqLine.POID
    '                                        iLineNBR = myReqLine.POLine_NBR
    '                                        iSchedNBR = myReqLine.POLineSched_NBR
    '                                        sBusUnitIN = myReq.POBusinessUnit
    '                                        sPODueDate = myReqLine.newDate
    '                                        If Not IsDate(sPODueDate) Then
    '                                            sPODueDate = Format(Now, "MM/dd/yyyy")
    '                                        End If
    '                                        logger.WriteVerboseLog("PROCESSING Business Unit: " & sBusUnit & " PO: " & sPO_ID & _
    '                                                                                                   "PO Line NBR: " & iLineNBR & " PO Sched Line No : " & iSchedNBR)
    '                                        If Not flagPOAsProcessed(sBusUnit, sPO_ID, iLineNBR, iSchedNBR, sPODueDate) Then
    '                                            logger.WriteErrorLog("Updating PODUEDTMON FAILED for Business Unit: " & sBusUnit & " PO: " & sPO_ID & _
    '                                                                                                "PO Line NBR: " & iLineNBR & " PO Sched Line No : " & iSchedNBR)
    '                                        End If

    '                                    Next

    '                                Else
    '                                    logger.WriteErrorLog("Sending Email FAILED and item will be reprocessed for Business Unit: " & myPO.PurchasingBusinessUnit & _
    '                                                         "  PO:" & myPO.POId)
    '                                End If


    '                            Next
    '                        End If
    '                    Else        'If cn.State = ConnectionState.Open Then
    '                        logger.WriteErrorLog("Connnection to Oracle FAILED :: [" & m_oraCNstring & "]")
    '                    End If      'If cn.State = ConnectionState.Open Then

    '                    logger.WriteLog("PO Due Date Change Check Process Completed.", TraceLevel.Info)

    '                    Try
    '                        cn.Dispose()
    '                        'cn.Close()
    '                    Catch ex As Exception
    '                    End Try

    '                    Try
    '                        cn.Close()
    '                        'cn.Dispose()
    '                    Catch ex As Exception
    '                    Finally
    '                        cn = Nothing
    '                    End Try

    '                Catch ex1 As Exception
    '                    logger.WriteLog("Not catched error in Timer Elapsed event: " & ex1.Message, TraceLevel.Error)
    '                Finally
    '                    If cn Is Nothing Then
    '                    Else
    '                        If cn.State = ConnectionState.Closed Then
    '                        Else

    '                            Try
    '                                cn.Dispose()
    '                                'cn.Close()
    '                            Catch ex As Exception
    '                            End Try

    '                            Try
    '                                cn.Close()
    '                                'cn.Dispose()
    '                            Catch ex As Exception
    '                            Finally
    '                                cn = Nothing
    '                            End Try

    '                        End If
    '                    End If
    '                End Try ' Try Catch for: If cn.State = ConnectionState.Open Then
    '                logger = Nothing

    '            End If
    '        End If

    '        ' re-start the timer before going out
    '        m_tmrChk.Start()
    '        'MyBase.EventLog.WriteEntry(cHdr & "timer re-started.", EventLogEntryType.Information)

    '    Catch ex As Exception
    '        'MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
    '    Finally
    '        If cn Is Nothing Then
    '        Else
    '            If cn.State = ConnectionState.Closed Then
    '            Else

    '                Try
    '                    cn.Dispose()
    '                    'cn.Close()
    '                Catch ex As Exception
    '                End Try

    '                Try
    '                    cn.Close()
    '                    'cn.Dispose()
    '                Catch ex As Exception
    '                Finally
    '                    cn = Nothing
    '                End Try

    '            End If
    '        End If
    '    End Try
    'End Sub

    Private Function sendEmailForPO(ByVal myReq As REQ) As Boolean
        Dim cHdr As String = m_sCommonMsgText & "sendEmailForPO: "

        Dim bIsSuccessful As Boolean = True
        Dim sEmailBody As String = ""
        Dim eml As New System.Net.Mail.MailMessage
        Dim sEmailTo As String
        Dim saEmailTos As Array
        Dim sOrigDueDate As String = ""

        Dim fromAddress As System.Net.Mail.MailAddress


        sEmailBody = "<HTML><HEAD><META name=GENERATOR content=""MSHTML 8.00.6001.18876""><span style=""background-color: black;""><img src='https://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' /></span></HEAD>" & _
                             "<BODY><CENTER><SPAN style=""WIDTH: 256px; FONT-FAMILY: Arial; FONT-SIZE: x-large"">SDI Marketplace</SPAN></CENTER>" & _
                             "<CENTER><SPAN>SDiExchange -<B> Order Due Date Change</B></SPAN></CENTER>&nbsp;" & _
                              "&nbsp; <DIV><P>Hello SDI Site Rep,<BR></DIV><BR>There has been a Due Date change for Order Number: <B> " & myReq.ReqId.ToString & _
                              "</B><BR><BR><TABLE> <COLGROUP><COL width=""7%"" valign=""top""><COL width=""25%"" valign=""top"">" & _
                              "<COL width=""50%"" valign=""top""><COL width=""9%"" valign=""top""><COL width=""9%"" valign=""top"">" & _
                              "<TR><TD><U>LINE <BR>NUMBER</U></TD><TD><U>MFG -<BR> MFG ITEM NO.</U><TD><U>DESCRIPTION</U></TD>" & _
                              "<TD><U>ORIGINAL <BR>DUE DATE</U></TD><TD><U>NEW <BR>DUE DATE</U></TD></TR><TR></TR>" & vbCrLf

        For Each myLine As ReqLine In myReq.ReqLines
            Try
                sOrigDueDate = myLine.oldDate()
            Catch ex As Exception
                logger.WriteErrorLog("sendEmailForPO: tried to get property oldDate: " & ex.Message)
                sOrigDueDate = ""
            End Try
            sEmailBody &= "<TR><TD>&nbsp;" & myLine.ReqLineNo & "</TD>" & _
                          "<TD>&nbsp;" & myLine.ItemID & "</TD>" & _
                          "<TD>&nbsp;" & myLine.Desc & "</TD>" & _
                          "<TD>&nbsp;" & sOrigDueDate & "</TD>" & _
                          "<TD>&nbsp;" & myLine.newDate & "</TD></TR>"


        Next

        sEmailBody &= vbCrLf & "</TABLE> <br><br>&nbsp;<BR>Sincerely,<BR>SDI Customer " & _
                     "Care<BR>&nbsp;<BR></P></DIV><DIV>&nbsp;</DIV><DIV>&nbsp;</DIV><HR width='100%' SIZE='1'><img src='https://www.sdiexchange.com/Images/SDIFooter_Email.png' /></BODY></HTML>"

        Try

            sEmailTo = GetPOEmailAddress(myReq.BusinessUnit, myReq.EmployeeId)

            ''for testing
            'If Trim(sEmailTo) = "" Then

            '    sEmailTo = "webdev@sdi.com;Benjamin.Heinzerling@sdi.com"
            'End If

            If Trim(sEmailTo) <> "" Then
                If sEmailTo = "NONE" Then
                    logger.WriteVerboseLog("Email not sent due to lack of email addresses. Order Number: " & myReq.ReqId.ToString & ".")
                    logger.WriteVerboseLog("Item will be marked sent for   " & myReq.BusinessUnit.ToString() & "  " & myReq.EmployeeId & ".")
                Else
                    saEmailTos = sEmailTo.Split(";")

                    For iCount As Integer = 0 To UBound(saEmailTos) - 1
                        eml.To.Add(saEmailTos(iCount).ToString)
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

                    eml.Subject = "Order Due Date has Changed"   '  & " - from SDIWEBSRV."

                    eml.From = fromAddress

                    Dim sCNString As String = cn.ConnectionString
                    Dim strDBase As String = "PLGR"
                    If Len(sCNString) > 4 Then
                        strDBase = UCase(Right(sCNString, 4))
                    End If

                    Select Case strDBase
                        Case "STAR", "PLGR", "RPTG", "DEVL"
                            eml.Subject = " TEST SDIX92 - " & eml.Subject
                            eml.To.Add("webdev@sdi.com")
                            sEmailTo = "webdev@sdi.com"
                        Case Else

                    End Select

                    eml.IsBodyHtml = True
                    eml.Body = sEmailBody

                    logger.WriteInformationLog("Sending email to " & eml.To.ToString)

                    Try

                        SendLogger(eml.Subject, sEmailBody, "PODUEDATECHGEMAIL", "Mail", sEmailTo, "", "webdev@sdi.com")

                    Catch ex As Exception
                        bIsSuccessful = False
                        logger.WriteErrorLog("Error Sending Email to " & eml.To.ToString)
                    End Try
                End If
                bIsSuccessful = True
            Else
                logger.WriteVerboseLog(cHdr & vbCrLf & "No Email Address Found for Business Unit: " & myReq.BusinessUnit & "  employee: " & myReq.EmployeeId)
                bIsSuccessful = False
            End If

        Catch ex As Exception
            bIsSuccessful = False
            'MyBase.EventLog.WriteEntry(cHdr & vbCrLf & ex.ToString, EventLogEntryType.Error)
        End Try

        Return (bIsSuccessful)

    End Function

    Private Function SendUserEmail(ByVal myBusinessUnit As String, ByVal myUserID As String) As EventLogEntryType
        Dim cHdr As String = m_sCommonMsgText & "SendUserEmail: "
        Dim sSQL As String
        Dim eletReturn As EventLogEntryType = EventLogEntryType.Error

        Try
            logger.WriteVerboseLog(" SEND USER EMAIL?  GETTING INFO FOR BUS UNIT: " & myBusinessUnit & " USER: " & myUserID)
            sSQL = "" & _
    "                  select isa_employee_id, isa_iol_op_value " & vbCrLf & _
    "                    from ps_isa_users_privs " & vbCrLf & _
    "                   where ISA_IOL_OP_NAME='EMLPODUEDT' and ISA_IOL_OP_TYPE='IOL' " & vbCrLf & _
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
                        Else
                            logger.WriteVerboseLog(" Employee " & myUserID & " is NOT set in the System")
                            eletReturn = EventLogEntryType.FailureAudit
                        End If
                    Else
                        logger.WriteErrorLog(" Employee " & myUserID & " will  NOT receive email due to error")
                        eletReturn = EventLogEntryType.Error
                    End If
                Catch ex As Exception
                    logger.WriteErrorLog(cHdr & " ERROR Executing Reader:  " & ex.Message)
                    eletReturn = EventLogEntryType.Error
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
            sSQL = "" & _
    "                  SELECT DISTINCT E.BUSINESS_UNIT as BUSINESS_UNIT, E.ISA_EMPLOYEE_ID as ISA_EMPLOYEE_ID, E.ISA_EMPLOYEE_EMAIL as ISA_EMPLOYEE_EMAIL  "    '  & _
            sSQL = sSQL & "                     FROM PS_ISA_USERS_TBL E "   '   & _
            If Trim(strBu3) <> "" Then
                strBu3 = Trim(strBu3)

                sSQL = sSQL & "                    WHERE E.BUSINESS_UNIT LIKE '%" & strBu3 & "'" & _
                "              AND  upper(E.ISA_EMPLOYEE_ID) =   upper('" & myEmployeeID & "')"
            Else
                sSQL = sSQL & "                    WHERE E.BUSINESS_UNIT = '" & myBusinessUnit & "'" & _
                "              AND  upper(E.ISA_EMPLOYEE_ID) =   upper('" & myEmployeeID & "')"
            End If
            

            If cn.State = ConnectionState.Open Then

                Dim cmd As OleDbCommand = cn.CreateCommand
                cmd.CommandText = sSQL
                cmd.CommandType = CommandType.Text
                Dim rdr As OleDbDataReader = Nothing

                Try
                    rdr = cmd.ExecuteReader
                Catch ex As Exception
                    logger.WriteErrorLog(cHdr & " ERROR Executing Reader:  " & ex.Message)
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
                                        logger.WriteVerboseLog("Successfully verified by 'Verify Your Rights' block .")
                                        sEmailAddresses &= CStr(rdr("ISA_EMPLOYEE_EMAIL")).Trim & ";"
                                        'sEmailAddresses &= (rdr("ISA_EMPLOYEE_EMAIL")).ToString() & ";"
                                        bReturn = True
                                    Case Is = EventLogEntryType.FailureAudit
                                        logger.WriteVerboseLog("'FailureAudit' returned by 'Verify Your Rights' block .")
                                        If Not bReturn Then
                                            sEmailAddresses = "NONE"
                                            bReturn = True
                                        End If
                                    Case Is = EventLogEntryType.Error
                                        logger.WriteVerboseLog("Error returned by 'Verify Your Rights' block .")
                                        If Not bReturn Then
                                            sEmailAddresses = ""
                                            bReturn = False
                                        End If
                                End Select

                            Catch ex As Exception

                                logger.WriteErrorLog(cHdr & " Error in inner Try Catch of 'GetPOEmailAddress' function")
                                If Not bReturn Then
                                    sEmailAddresses = "NONE"
                                    bReturn = True
                                End If
                            End Try

                        End While
                    Else
                        logger.WriteVerboseLog("Reader doesn't return any rows for " & myBusinessUnit & "  " & myEmployeeID & ".")
                        If Not bReturn Then
                            sEmailAddresses = "NONE"
                            bReturn = True
                        End If
                    End If
                Else
                    logger.WriteErrorLog(cHdr & "Data Reader is Nothing")

                    bReturn = False
                End If

            Else
                logger.WriteVerboseLog("GetPOEmailAddress not connected")
                logger.WriteErrorLog(cHdr & "Database is not connected")
                bReturn = False
            End If

        Catch ex As Exception
            logger.WriteErrorLog(cHdr & " Error in outer Try Catch of 'GetPOEmailAddress' function")
            bReturn = False
        End Try
        'logger.WriteVerboseLog("GetPOEmailAddress1 set return")
        If Not bReturn Then
            sEmailAddresses = ""
        End If
        'logger.WriteVerboseLog("GetPOEmailAddressreturn")

        Return (sEmailAddresses)
    End Function
    Private Function flagPOAsProcessed(ByVal myBusUnit As String, ByVal myPOID As String, _
                                       ByVal myLineNBR As Integer, ByVal mySchedNBR As Integer, _
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
    Private Function sqlInsertISA_PODUEDTMON(ByVal myBusinessUnit As String, ByVal myPOID As String, _
                                       ByVal myLineNBR As Integer, ByVal mySchedNBR As Integer, _
                                       ByVal myPODueDate As String, Optional ByVal IsReq As Boolean = False) As Boolean
        Dim cHdr As String = m_sCommonMsgText & "sqlInsertISA_PODUEDTMON: "
        Dim sSQL As String
        Dim bReturn As Boolean = False

        Try

            sSQL = "" & _
    "                  INSERT INTO SYSADM8.PS_ISA_PODUEDTMON " & vbCrLf & _
    "                  (BUSINESS_UNIT, PO_ID, LINE_NBR, SCHED_NBR, DUE_DT, ADD_DTTM, NOTIFY_DTTM) " & vbCrLf & _
    "                   VALUES " & vbCrLf & _
    "                   ('" & myBusinessUnit & "', '" & myPOID & "', '" & myLineNBR.ToString & "'," & _
    "                     '" & mySchedNBR & "', to_date('" & myPODueDate & "', 'mm/dd/yyyy'), SYSDATE, SYSDATE )"
            If cn.State = ConnectionState.Open Then
                Dim cmd As OleDbCommand = cn.CreateCommand
                cmd.CommandText = sSQL
                cmd.CommandType = CommandType.Text

                Try
                    Dim iRows As Integer
                    iRows = cmd.ExecuteNonQuery()
                    If iRows > 0 Then
                        bReturn = True
                        If IsReq Then
                            logger.WriteVerboseLog(cHdr & "PODUEDTMON Insert Succeeded FOR REQ - Inserted " & iRows.ToString & " Row(s)")
                        Else
                            logger.WriteVerboseLog(cHdr & "PODUEDTMON Insert Succeeded - Inserted " & iRows.ToString & " Row(s)")
                        End If

                    Else
                        bReturn = False
                        If IsReq Then
                            logger.WriteVerboseLog(cHdr & "PODUEDTMON Insert FOR REQ Failed : " & vbCrLf & sSQL)
                        Else
                            logger.WriteVerboseLog(cHdr & "PODUEDTMON Insert Failed : " & vbCrLf & sSQL)
                        End If
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
                        If IsReq Then
                            logger.WriteVerboseLog(cHdr & "PODUEDTMON Update Succeeded For REQ -  Updated " & iRows.ToString & " Row(s)")
                        Else
                            logger.WriteVerboseLog(cHdr & "PODUEDTMON Update Succeeded -  Updated " & iRows.ToString & " Row(s)")
                        End If
                    Else
                        bReturn = False
                        If IsReq Then
                            logger.WriteVerboseLog(cHdr & "PODUEDTMON Update Failed Attemping to Insert FOR REQ.")
                        Else
                            logger.WriteVerboseLog(cHdr & "PODUEDTMON Update Failed Attemping to Insert.")
                        End If

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
            "SELECT BUSINESS_UNIT , PO_ID, PO_DT, LINE_NBR, SCHED_NBR, BUSINESS_UNIT_IN, bu_nstk, ORDER_NO " & vbCrLf & _
            ", ORDER_INT_LINE_NO, ISA_EMPLOYEE_ID,  MFG_ID || ' - ' ||  MFG_ITM_ID AS ITEM_ID, DESCR254_MIXED, ORIG_PROMISE_DT, DUE_DT FROM (" & vbCrLf & _
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
            ",LB.EMPLID AS isa_employee_id " & vbCrLf & _
            ",L.MFG_ID ,L.MFG_ITM_ID " & vbCrLf & _
            ",l.DESCR254_MIXED " & vbCrLf & _
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
            "  AND TRIM(LB.EMPLID) IS NOT NULL "
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
            ",LB.REQ_ID    ,LB.req_line_nbr ,LB.EMPLID " & _
            ",L.MFG_ID ,L.MFG_ITM_ID " & vbCrLf & _
            ",l.DESCR254_MIXED " & vbCrLf & _
            "ORDER BY B.BUSINESS_UNIT, B.PO_ID, A.LINE_NBR, A.SCHED_NBR " & vbCrLf & _
            ") WHERE ((to_char(ORIG_PROMISE_DT) <> to_char(DUE_DT)) or(ORIG_PROMISE_DT is null and  DUE_DT is not null) ) " & vbCrLf & _
            " and due_dt >= sysdate "
            strBuild.Append(s)
            s = ""
            s = strBuild.ToString
        Catch ex As Exception
            s = ""
            logger.WriteErrorLog(cHdr & vbCrLf & ex.ToString)
        End Try
        '   logger.WriteVerboseLog(s)
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

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(subject, "SDIExchADMIN@sdi.com", EmailTo, "", EmailBcc, "N", body, m_CN)

        Catch ex As Exception

        End Try
    End Sub


End Class
