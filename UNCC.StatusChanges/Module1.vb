Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter
Imports System.Collections.Generic

Module Module1

    Private m_logger As appLogger = Nothing

    ' disabled since logging functionality was ported to SDI.ApplicationLogger
    'Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\INTFCXML"
    Dim logpath As String = "C:\INTFCXML\LOGS\StatusChgUNCC" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR")

    Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start Status XML out")
        Console.WriteLine("")

        '
        ' read confirugation values ...
        '   values on app.config will always win!
        '   -- 3/19/2014 erwin
        '
        '   (1) connection string / db connection
        Dim cnString As String = ""
        Try
            cnString = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
        End Try
        If (cnString.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            connectOR = New OleDbConnection(cnString)
        End If
        '   (2) root directory
        Dim rDir As String = ""
        Try
            rDir = My.Settings("rootDir").ToString.Trim
        Catch ex As Exception
        End Try
        If (rDir.Length > 0) Then
            rootDir = rDir
        End If
        '   (3) log path/file
        Dim sLogPath As String = ""
        Try
            sLogPath = My.Settings("logPath").ToString.Trim
        Catch ex As Exception
        End Try
        If (sLogPath.Length > 0) Then
            logpath = sLogPath & "\StatusChgUNCC" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If
        '   END : 3/19/2014 erwin

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' get/parse param
        Dim param As String() = Command.Split(" ")
        Dim arr As New ArrayList
        For Each s As String In param
            If s.Trim.Length > 0 Then
                arr.Add(s.Trim.ToUpper)
            End If
        Next
        If (arr.IndexOf("/LOG=VERBOSE") > -1) Then
            logLevel = TraceLevel.Verbose
        ElseIf (arr.IndexOf("/LOG=INFO") > -1) Or (arr.IndexOf("/LOG=INFORMATION") > -1) Then
            logLevel = TraceLevel.Info
        ElseIf (arr.IndexOf("/LOG=WARNING") > -1) Or (arr.IndexOf("/LOG=WARN") > -1) Then
            logLevel = TraceLevel.Warning
        ElseIf (arr.IndexOf("/LOG=ERROR") > -1) Then
            logLevel = TraceLevel.Error
        ElseIf (arr.IndexOf("/LOG=OFF") > -1) Or (arr.IndexOf("/LOG=FALSE") > -1) Then
            logLevel = TraceLevel.Off
        Else
            ' don't change default
        End If
        arr = Nothing
        param = Nothing

        ' initialize log
        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        ' log verbose DB connection string
        m_logger.WriteInformationLog(rtn & " :: Start of UNCC status changes process.")
        m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

        ' disabled since logging functionality was ported to SDI.ApplicationLogger
        'objStreamWriter = File.CreateText(logpath)
        'objStreamWriter.WriteLine("  Update of Status Processing XML out " & Now())
        m_logger.WriteInformationLog(rtn & " :: Update of SP Processing XML out.")

        Dim bolError As Boolean = buildStatusXMLOut()

        If bolError = True Then
            SendEmail()
        End If

        ' disabled since logging functionality was ported to SDI.ApplicationLogger
        'objStreamWriter.WriteLine("  End of Status Processing XML out " & Now())
        m_logger.WriteInformationLog(rtn & " :: End of Status Processing XML out.")

        ' disabled since logging functionality was ported to SDI.ApplicationLogger
        'objStreamWriter.Flush()
        'objStreamWriter.Close()

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending UNCC status changes process.")

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Function buildStatusXMLOut() As Boolean

        Dim rtn As String = "Module1.buildStatusXMLOut"

        ' set the end date time
        Dim strSQLstring As String
        Dim dteEndDate As DateTime
        'Dim dteEndDatePlus1 As DateTime
        Dim format As New System.Globalization.CultureInfo("en-US", True)
        strSQLstring = "SELECT" & vbCrLf & _
                    " to_char(MAX( A.DTTM_STAMP), 'MM/DD/YY HH24:MI:SS') as MAXDATE" & vbCrLf & _
                    " FROM PS_ISAORDSTATUSLOG A" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = 'I0256'"

        '" MAX( TO_CHAR(A.DTTM_STAMP,'YYYY-MM-DD-HH24.MI.SS AM')) as MAXDATE" & vbCrLf & _
        Dim dr As OleDbDataReader = Nothing

        Try
            Dim command As OleDbCommand
            Command = New OleDbCommand(strSQLstring, connectOR)
            connectOR.Open()

            dr = command.ExecuteReader
            If dr.HasRows Then
                If dr.Read Then
                    Try
                        dteEndDate = Convert.ToDateTime(dr.Item("MAXDATE"))
                    Catch ex As Exception
                        dteEndDate = Now.ToString
                    End Try

                Else
                    dteEndDate = Now.ToString
                End If
            Else
                dteEndDate = Now.ToString
            End If
            
            dr.Close()
            connectOR.Close()

        Catch OleDBExp As OleDbException
            Try
                If Not dr Is Nothing Then

                    dr.Close()
                End If
                connectOR.Close()
            Catch ex As Exception

            End Try
            'objStreamWriter.WriteLine("     Error - error reading end date FROM PS_ISAORDSTATUSLOG A")
            m_logger.WriteErrorLog(rtn & " :: Error - error reading end date FROM PS_ISAORDSTATUSLOG A")
            Return True
        End Try

        connectOR.Open()
        Dim objEnterprise As New clsEnterprise("I0256", connectOR)
        Dim dteStartDate As DateTime = objEnterprise.SendStartDate
        connectOR.Close()

        'dteEndDatePlus1 = DateAdd(DateInterval.Second, 1, dteEndDate)
        Dim ds As New DataSet
        Dim bolerror As Boolean
        strSQLstring = "SELECT A.ORDER_NO, A.ISA_INTFC_LN AS LINE_NBR," & vbCrLf & _
                            " TO_CHAR(A.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM') as DTTM_STAMP" & vbCrLf & _
                            " ,A.ISA_LINE_STATUS AS ISA_ORDER_STATUS, DECODE(A.ISA_LINE_STATUS,'CRE','1','NEW','2','DSP','3','ORD','3','RSV','3','PKA','4','PKP','4','DLP','5','RCP','5','RCF','6','PKQ','5','DLO','5','DLF','6','PKF','7','CNC','C','QTS','Q','QTW','W','1') AS OLD_ORDER_STATUS" & vbCrLf & _
                            " FROM PS_ISAORDSTATUSLOG A, SYSADM8.PS_ISA_ORD_INTF_HD B" & vbCrLf & _
                            " WHERE A.BUSINESS_UNIT_OM = 'I0256'" & vbCrLf & _
                            " AND B.ORIGIN IN ('INT','IOL') " & vbCrLf & _
                            " AND A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf & _
                            " AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
                            " AND TO_DATE(TO_CHAR(A.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM'), 'MM/DD/YYYY HH:MI:SS AM') > TO_DATE('" & dteStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                            " AND TO_DATE(TO_CHAR(A.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM'), 'MM/DD/YYYY HH:MI:SS AM') <= TO_DATE('" & dteEndDate & "', 'MM/DD/YYYY HH:MI:SS AM')"

        Try
            ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            'objStreamWriter.WriteLine("     Error - error reading transaction FROM PS_ISAORDSTATUSLOG A")
            m_logger.WriteErrorLog(rtn & " :: Error - error reading transaction FROM PS_ISAORDSTATUSLOG A")
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            'objStreamWriter.WriteLine("     Warning - no status changes to process at this time")
            m_logger.WriteWarningLog(rtn & " :: Warning - no status changes to process at this time")
            Return False
        End If
        Dim I As Integer
        Dim rowsaffected As Integer
        Dim dteJulian As Integer
        Dim dteStart As Date = "01/01/1900"
        dteJulian = DateDiff(DateInterval.Day, dteStart, Now())

        Dim strXMLPath As String = rootDir & "\XMLOUT\UNCCSTATUS" & Convert.ToString(dteJulian) & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & ".xml"
        Dim objXMLWriter As XmlTextWriter
        Try
            objXMLWriter = New XmlTextWriter(strXMLPath, Nothing)
            'objStreamWriter.WriteLine("  Writing to file: " & strXMLPath)
            m_logger.WriteInformationLog(rtn & " :: Writing to file: " & strXMLPath)
        Catch objError As Exception
            'objStreamWriter.WriteLine("  Error while accessing document " & strXMLPath & vbCrLf & objError.Message)
            m_logger.WriteErrorLog(rtn & " :: Error while accessing document " & strXMLPath & vbCrLf & objError.Message)
            Return True
        End Try
        objXMLWriter.Formatting = Formatting.Indented
        objXMLWriter.Indentation = 3
        objXMLWriter.WriteStartDocument()
        objXMLWriter.WriteComment("Created on " & Now())
        objXMLWriter.WriteStartElement("DATA")

        Dim sdiOrderNo As String = ""
        Dim unccOrderNo As String = ""
        Dim strOrdStatusToCheck As String = ""

        For I = 0 To ds.Tables(0).Rows.Count - 1

            ' 2009.02.05 - erwin
            '   handle UNCC's work order numbering before sending status change(s) back to Archibus
            sdiOrderNo = ""
            Try
                sdiOrderNo = CStr(ds.Tables(0).Rows(I).Item("ORDER_NO"))
            Catch ex As Exception
            End Try
            unccOrderNo = orderNoMapper.changeToUNCCOrderNo(sdiOrderNo)
            m_logger.WriteVerboseLog(rtn & " :: mapped SDI=" & sdiOrderNo & " back to UNCC=" & unccOrderNo)

            objXMLWriter.WriteStartElement("ISAORDSTATUSLOG")
            'objXMLWriter.WriteElementString("ORDER_NO", ds.Tables(0).Rows(I).Item("ORDER_NO"))
            objXMLWriter.WriteElementString("ORDER_NO", unccOrderNo)
            objXMLWriter.WriteElementString("LINE_NBR", ds.Tables(0).Rows(I).Item("LINE_NBR"))
            objXMLWriter.WriteElementString("DTTM_STAMP", ds.Tables(0).Rows(I).Item("DTTM_STAMP"))
            strOrdStatusToCheck = ds.Tables(0).Rows(I).Item("OLD_ORDER_STATUS")
            'Try
            '    If UCase(Trim(strOrdStatusToCheck)) = "8" Then
            '        strOrdStatusToCheck = "6"
            '    End If
            'Catch ex As Exception
            '    strOrdStatusToCheck = ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS")
            'End Try
            objXMLWriter.WriteElementString("ISA_ORDER_STATUS", strOrdStatusToCheck)

            objXMLWriter.WriteEndElement()

        Next

        ' update date/time stamp (flag) of last set of records processed
        strSQLstring = "UPDATE PS_ISA_ENTERPRISE" & vbCrLf & _
                    " SET ISA_LAST_STAT_SEND = TO_DATE('" & dteEndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                    " WHERE ISA_BUSINESS_UNIT = 'I0256'"

        Try
            Dim Command = New OleDbCommand(strSQLstring, connectOR)
            connectOR.Open()
            rowsaffected = Command.ExecuteNonQuery()
            connectOR.Close()
        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            'objStreamWriter.WriteLine("  Error - updating the Enterprise send date " & OleDBExp.ToString)
            m_logger.WriteErrorLog(rtn & " :: Error - updating the Enterprise send date " & OleDBExp.ToString)
            bolerror = True
        End Try
        objXMLWriter.WriteEndElement()
        objXMLWriter.Flush()
        objXMLWriter.Close()
        Dim strXMLResult As String
        Dim objSR As StreamReader = File.OpenText(strXMLPath)
        strXMLResult = objSR.ReadToEnd()
        objSR.Close()
        objSR = Nothing

        If bolerror = True Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub SendEmail()

        Dim rtn As String = "Module1.SendEmail"

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "bob.dougherty@sdi.com;pete.doyle@sdi.com"

        'The subject of the email
        email.Subject = "UNCCStatusChanges XML OUT Error"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>UNCCStatusChanges has completed with errors, review log.</td></tr></table>"

        'email.Body = email.Body & "<tr><td></td><a href='\\BDougherty_XP-l\logs'>\\BDougherty_XP-l\logs\</a></tr></table></body></html>"

        'Send the email and handle any error that occurs
        Try

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "", "Y", email.Body, connectOR)

            SendLogger(email.Subject, email.Body, "UNCCSTATCHNGS", "Mail", email.To, "", "")

        Catch

            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent")
        End Try

    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            SDIEmailService.EmailUtilityServices(MailType, "SDIExchange@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

End Module


