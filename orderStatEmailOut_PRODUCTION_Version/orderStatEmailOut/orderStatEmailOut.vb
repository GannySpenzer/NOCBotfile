Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail


Module orderStatEmailOut

    Public Enum eOrderStatuses As Integer
        Unknown = -1
        Saved = 0
        Submitted = 1
        ProcessingOrder = 2
        Ordered = 3
        Picking = 4
        PartiallyShipped = 5
        Shipped = 6
        WaitingBudgetApproval = 7
        WaitingOrderApproval = 8
        Cancelled = 9
    End Enum


    Private m_logger As ApplicationLogger = Nothing

    Private Const oraCN_default_provider As String = "Provider=OraOLEDB.Oracle.1;"
    Private Const oraCN_default_creden As String = "User ID=sdiexchange;Password=sd1exchange;"
    Private Const oraCN_default_DB As String = "Data Source=RPTG;"

    Dim rootDir As String = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName)
    Dim logpath As String = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\LOGS\shipStatEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Private m_oraCN As OleDbConnection = Nothing

    Private m_oraCNstring As String = "" & _
                                      oraCN_default_provider & _
                                      oraCN_default_creden & _
                                      oraCN_default_DB & _
                                      ""


    Sub Main()

        Dim rtn As String = "orderStatEmailOut.Main"

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' params
        Dim param As Hashtable = ParseCommandParams(Command)
        If param.ContainsKey(key:="/LOG") Then
            Dim lvl As String = CStr(param(key:="/LOG")).Trim.ToUpper
            If lvl = "VERB" Or lvl = "VERBOSE" Then
                logLevel = TraceLevel.Verbose
            ElseIf lvl = "INFO" Or lvl = "INFORMATION" Then
                logLevel = TraceLevel.Info
            ElseIf lvl = "WARN" Or lvl = "WARNING" Then
                logLevel = TraceLevel.Warning
            ElseIf lvl = "ERR" Or lvl = "ERROR" Then
                logLevel = TraceLevel.Error
            ElseIf lvl = "OFF" Then
                logLevel = TraceLevel.Off
            Else
                ' don't change default
            End If
        End If
        If param.ContainsKey(key:="/DB") Then
            Dim db As String = CStr(param(key:="/DB")).Trim.ToUpper
            m_oraCNstring = "" & _
                            oraCN_default_provider & _
                            oraCN_default_creden & _
                            "Data Source=" & db & ";" & _
                            ""
        End If
        param = Nothing

        ' connection
        m_oraCN = New OleDbConnection(m_oraCNstring)

        ' initialize log
        m_logger = New ApplicationLogger(logpath, logLevel)

        ' process
        Try
            ' log verbose DB connection string
            m_logger.WriteInformationLog("" & _
                                         System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                         " v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                         "")
            m_logger.WriteInformationLog(rtn & " :: START: shipped email notification process.")
            m_logger.WriteVerboseLog(rtn & " :: connection string : [" & m_oraCN.ConnectionString & "]")

            runProcess()

            ' final log for this run
            m_logger.WriteInformationLog(rtn & " :: END: shipped email notification process.")
        Catch ex As Exception
            m_logger.WriteErrorLog(rtn & "::" & ex.ToString)
        End Try

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Function ParseCommandParams(ByVal s As String) As Hashtable
        Dim arr As New Hashtable
        s = s.Trim
        Dim sections As String() = s.Split(CChar(" "))
        For Each keyVal As String In sections
            If keyVal.Trim.Length > 0 Then
                Dim s1 As String() = keyVal.Trim.Split(CChar("="))
                ' key
                Dim id As String = ""
                Try
                    id = s1(0).Trim.ToUpper
                Catch ex As Exception
                End Try
                ' value
                Dim val As String = ""
                Try
                    val = s1(1).Trim.ToUpper
                Catch ex As Exception
                End Try
                ' check/add
                If id.Length > 0 Then
                    If arr.ContainsKey(id) Then
                        ' OVERRIDE
                        arr(id) = val
                    Else
                        ' ADD
                        arr.Add(id, val)
                    End If
                End If
            End If
        Next
        Return arr
    End Function

    Private Sub runProcess()

        Try
            m_oraCN.Open()
        Catch ex As Exception
        End Try

        If m_oraCN.State = ConnectionState.Open Then
            Dim xmlData As New XmlDocument
            xmlData.Load(filename:=rootDir & "\siteConfiguration.xml")
            Dim appSettings As Hashtable = LoadAppSetting(xmlData)
            Dim arr As ArrayList = LoadSitesToProcess(xmlData)
            xmlData = Nothing

            ' do for each site
            If arr.Count > 0 Then
                Dim site As ISiteSetting = Nothing
                For Each o As Object In arr
                    site = CType(o, ISiteSetting)

                    Dim bIsProcessSite As Boolean = True

                    ' check for site's start date if earlier than the system date/time
                    If bIsProcessSite Then
                        bIsProcessSite = (site.StartDate <= Now)
                    End If

                    If bIsProcessSite Then
                        m_logger.WriteInformationLog("processing (" & site.SiteId & ") " & site.SiteName & " ...")
                        ' do for each status to check for this site
                        For Each checkStat As statusToCheck In site.StatusesToCheck
                            Select Case CType(checkStat.StatusId, eOrderStatuses)
                                Case eOrderStatuses.Shipped
                                    m_logger.WriteVerboseLog("checking " & eOrderStatuses.Shipped.ToString & " order(s) ...")
                                    Dim e As New orderShippedStatusEventArgs
                                    e.oledbCN = m_oraCN
                                    e.Logger = m_logger
                                    e.AppSettings = appSettings
                                    CType(o, ICanCheckOrderShippedStatus).sendOrderShippedStatusEmail(Nothing, e)
                                    CType(o, ICanCheckNSTKOrderShippedStatus).sendNSTKOrderShippedStatusEmail(Nothing, e)
                                Case Else
                                    ' unknown status, do nothing
                            End Select
                        Next
                    End If
                Next
            End If

            arr = Nothing
            appSettings = Nothing
        Else
            ' unable to open connection to DB
        End If  'If m_oraCN.State = ConnectionState.Open Then

        Try
            m_oraCN.Close()
            m_oraCN.Dispose()
        Catch ex As Exception
        Finally
            m_oraCN = Nothing
        End Try

    End Sub

    '// this routine will return a key/value pair (hashtable)
    Private Function LoadAppSetting(ByVal xmlConfig As XmlDocument) As Hashtable
        Dim arr As New Hashtable
        If Not (xmlConfig Is Nothing) Then
            Dim ndAppSettings As XmlNode = xmlConfig.SelectSingleNode("configuration/appSettings")
            If Not (ndAppSettings Is Nothing) Then
                Dim sId As String = ""
                Dim sValue As String = ""
                For Each ndAdd As XmlNode In ndAppSettings.ChildNodes
                    If ndAdd.NodeType = XmlNodeType.Element And _
                       ndAdd.Name.Trim.ToUpper = "ADD" Then
                        sId = ""
                        Try
                            sId = ndAdd.Attributes("id").Value.Trim
                        Catch ex As Exception
                        End Try
                        sValue = ""
                        Try
                            sValue = ndAdd.Attributes("value").Value.Trim
                        Catch ex As Exception
                        End Try
                        If sId.Length > 0 And sValue.Length > 0 Then
                            If Not arr.ContainsKey(sId) Then
                                arr.Add(key:=sId, value:=sValue)
                            End If
                        End If
                    End If
                Next
            End If
        End If  'If Not (xmlConfig Is Nothing) Then
        Return (arr)
    End Function

    '// this routine expects siteConfiguration.xml xml document
    '//     returning array that contains ISiteSetting object type
    Private Function LoadSitesToProcess(ByVal xmlConfig As XmlDocument) As ArrayList
        Dim arr As New ArrayList
        If Not (xmlConfig Is Nothing) Then
            Dim ndSiteSettings As XmlNode = xmlConfig.SelectSingleNode("configuration/siteSettings")
            If Not (ndSiteSettings Is Nothing) Then
                Dim s1 As String() = New String() {}
                Dim s2 As String = ""
                Dim siteId As String = ""
                Dim startDT As String = ""
                Dim siteDesc As String = ""
                Dim bccForAll As String = ""
                Dim bccForNSTK As String = ""
                Dim bccForNY0A As String = ""
                Dim site As ISiteSetting = Nothing
                For Each ndSite As XmlNode In ndSiteSettings.ChildNodes
                    If ndSite.NodeType = XmlNodeType.Element And ndSite.Name.ToUpper = "SITE" Then
                        siteId = ""
                        Try
                            siteId = ndSite.Attributes("id").Value.Trim.ToUpper
                        Catch ex As Exception
                        End Try
                        siteDesc = ""
                        Try
                            siteDesc = ndSite.Attributes("desc").Value.Trim
                        Catch ex As Exception
                        End Try
                        startDT = ""
                        Try
                            startDT = CDate(ndSite.Attributes("startDT").Value.Trim).ToString
                        Catch ex As Exception
                        End Try
                        bccForAll = ""
                        Try
                            'bccForAll = ndSite.Attributes("bccAll").Value.Trim
                            s2 = ""
                            s1 = ndSite.Attributes("bccAll").Value.Trim.Split(";"c)
                            If s1.Length > 0 Then
                                For Each s As String In s1
                                    If s.Trim.Length > 0 Then
                                        s2 &= s & ";"
                                    End If
                                Next
                            End If
                            bccForAll = s2
                        Catch ex As Exception
                        End Try
                        bccForNSTK = ""
                        Try
                            'bccForNSTK = ndSite.Attributes("bccAddForNSTK").Value.Trim
                            s2 = ""
                            s1 = ndSite.Attributes("bccAddForNSTK").Value.Trim.Split(";"c)
                            If s1.Length > 0 Then
                                For Each s As String In s1
                                    If s.Trim.Length > 0 Then
                                        s2 &= s & ";"
                                    End If
                                Next
                            End If
                            bccForNSTK = s2
                        Catch ex As Exception
                        End Try
                        bccForNY0A = ""
                        Try
                            'bccForNY0A = ndSite.Attributes("bccAddForNY0A").Value.Trim
                            s2 = ""
                            s1 = ndSite.Attributes("bccAddForNY0A").Value.Trim.Split(";"c)
                            If s1.Length > 0 Then
                                For Each s As String In s1
                                    If s.Trim.Length > 0 Then
                                        s2 &= s & ";"
                                    End If
                                Next
                            End If
                            bccForNY0A = s2
                        Catch ex As Exception
                        End Try
                        If siteId.Length > 0 Then
                            site = Nothing
                            Select Case siteId
                                Case "I0260"
                                    site = New orderStatusCheckerI0260(siteId)
                                Case Else
                                    ' defined in XML but not here!?
                                    '   create "orderStatusCheckerXXXXX" for this site
                            End Select
                            If Not (site Is Nothing) Then
                                arr.Add(site)

                                site.SiteName = siteDesc
                                If IsDate(startDT) Then
                                    site.StartDate = CDate(startDT)
                                End If
                                site.bccForAll = bccForAll
                                site.bccForNSTK = bccForNSTK
                                site.bccForNY0A = bccForNY0A

                                Dim statId As String = ""
                                Dim statDesc As String = ""
                                Dim statStartDT As String = ""
                                Dim stat As statusToCheck = Nothing

                                For Each ndStat As XmlNode In ndSite.ChildNodes
                                    If ndStat.NodeType = XmlNodeType.Element And ndStat.Name.ToUpper = "ORDERSTATUSTOCHECK" Then
                                        statId = ""
                                        Try
                                            statId = ndStat.Attributes("id").Value.Trim
                                        Catch ex As Exception
                                        End Try
                                        statDesc = ""
                                        Try
                                            statDesc = ndStat.Attributes("desc").Value.Trim
                                        Catch ex As Exception
                                        End Try
                                        statStartDT = ""
                                        Try
                                            statStartDT = CDate(ndStat.Attributes("startDT").Value.Trim).ToString
                                        Catch ex As Exception
                                        End Try
                                        If statId.Length > 0 Then
                                            stat = statusToCheck.GetStatusUsingName(statusName:=statId)
                                            stat.StartDate = site.StartDate
                                            If statStartDT.Length > 0 Then
                                                If IsDate(statStartDT) Then
                                                    stat.StartDate = CDate(statStartDT)
                                                End If
                                            End If
                                            site.StatusesToCheck.Add(stat)
                                            Dim privId As String = ""
                                            Dim privDesc As String = ""
                                            For Each ndUserPrivsForStat As XmlNode In ndStat.ChildNodes
                                                If ndUserPrivsForStat.NodeType = XmlNodeType.Element And ndUserPrivsForStat.Name.ToUpper = "USEREMAILPRIVFORSTATUS" Then
                                                    privId = ""
                                                    Try
                                                        privId = ndUserPrivsForStat.Attributes("id").Value.Trim.ToUpper
                                                    Catch ex As Exception
                                                    End Try
                                                    privDesc = ""
                                                    Try
                                                        privDesc = ndUserPrivsForStat.Attributes("desc").Value.Trim
                                                    Catch ex As Exception
                                                    End Try
                                                    If privId.Length > 0 Then
                                                        stat.UserEmailPrivsForStatus.Add(New myIdDesc(privId, privDesc))
                                                    End If
                                                End If
                                            Next
                                        End If
                                    End If  'If ndStat.NodeType = XmlNodeType.Element And ndStat.Name.ToUpper = "ORDERSTATUSTOCHECK" Then
                                Next
                            End If
                        End If  'If siteId.Length > 0 Then
                    End If  'If ndSite.NodeType = XmlNodeType.Element And ndSite.Name.ToUpper = "SITE" Then
                Next
            End If  'If Not (ndSiteSettings Is Nothing) Then
        End If
        Return (arr)
    End Function

    Private Function getOrderWithStatusChanges(ByVal bu As String, _
                                               ByVal dtStart As DateTime, _
                                               ByVal dtEnd As DateTime) As ArrayList
        Dim arr As New ArrayList

        Dim sql As String = "" & vbCrLf & _
                            "SELECT" & vbCrLf & _
                            ",A.BUSINESS_UNIT_OM" & vbCrLf & _
                            ",A.ORDER_NO" & vbCrLf & _
                            "FROM PS_ISAORDSTATUSLOG A" & vbCrLf & _
                            "WHERE (A.DTTM_STAMP BETWEEN TO_DATE('" & dtStart.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS') AND TO_DATE('" & dtEnd.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS'))" & vbCrLf & _
                            "  AND A.BUSINESS_UNIT_OM = '" & bu & "'" & vbCrLf & _
                            "GROUP BY A.BUSINESS_UNIT_OM, A.ORDER_NO" & vbCrLf & _
                            ""
        Dim cmd As OleDbCommand = m_oraCN.CreateCommand
        cmd.CommandText = sql
        cmd.CommandType = CommandType.Text

        Dim rdr As OleDbDataReader = Nothing

        Try
            rdr = cmd.ExecuteReader
        Catch ex As Exception
        End Try

        If Not (rdr Is Nothing) Then
            Dim sBU As String = ""
            Dim sOrder As String = ""
            While rdr.Read
                sBU = ""
                Try
                    sBU = CStr(rdr("BUSINESS_UNIT_OM")).Trim.ToUpper
                Catch ex As Exception
                End Try
                sOrder = ""
                Try
                    sOrder = CStr(rdr("ORDER_NO")).Trim.ToUpper
                Catch ex As Exception
                End Try
                If sBU.Length > 0 And sOrder.Length > 0 Then
                    arr.Add(New changedOrder(sBU, sOrder))
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

        Return (arr)
    End Function

End Module
