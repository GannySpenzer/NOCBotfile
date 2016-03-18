Imports System
Imports System.Data
Imports System.Web
Imports System.Xml
Imports System.Data.OleDb
Imports System.Web.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter
Imports System.IO

Module Module1

    Private m_logger As appLogger = Nothing

    Dim rootDir As String = "C:\CytecMxmIn"
    Dim logpath As String = "C:\CytecMxmIn\LOGS\CytecMaxIssueOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")

    Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start of the Cytec Maximo Issues build XML out")
        Console.WriteLine("")

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
            logpath = sLogPath & "\CytecMaxIssueOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

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
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        ' log verbose DB connection string
        m_logger.WriteInformationLog(rtn & " :: Start of Cytec Maximo Issues build XML process.")
        m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

        Dim bolError As Boolean = BuildIssuesOutXML()

        If bolError = True Then
            SendEmail()
        End If

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending Cytec Maximo Issues build XML process.")

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Function BuildIssuesOutXML() As Boolean
        Dim bolError As Boolean = False
        Dim ds As New DataSet

        Dim rtn As String = "Module1.BuildIssuesOutXML"

        Dim strSqlString As String = "SELECT * FROM SYSADM8.PS_ISA_MXM_MOV_OUT WHERE PROCESS_FLAG != 'Y'"

        Try
            'Dim command As OleDbCommand
            'command = New OleDbCommand(strSqlString, connectOR)
            connectOR.Open()

            ds = ORDBAccess.GetAdapter(strSqlString, connectOR)

            connectOR.Close()

        Catch OleDBExp As OleDbException
            m_logger.WriteErrorLog(rtn & " :: Error reading source table SYSADM8.PS_ISA_MXM_MOV_OUT")
            m_logger.WriteErrorLog("ERROR MSG: " & OleDBExp.Message)
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            Return True
        End Try

        If ds Is Nothing Then
            m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo Issues to process at this time")
            Return False
        Else

            If ds.Tables.Count = 0 Then
                m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo Issues to process at this time")
                Return False
            Else

                If ds.Tables(0).Rows.Count < 1 Then
                    m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo Issues to process at this time")
                    Return False
                End If
            End If
        End If

        Dim I As Integer
        Dim rowsaffected As Integer
        If ds.Tables(0).Rows.Count > 0 Then

            Dim strXMLPath As String = rootDir & "\XMLOUT\CytecMaxIssuesOut" & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & ".xml"
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
            objXMLWriter.WriteStartElement("int:MXINVISSUEInterface")

            'build header - <int:Header 
            'NOT DONE YET

            'build <int:Content

            Dim sdiOrderNo As String = ""
            Dim unccOrderNo As String = ""
            Dim strOrdStatusToCheck As String = ""

            For I = 0 To ds.Tables(0).Rows.Count - 1
                'create XML element/lines 
                ' build <int:MXINVISSUE
                ' build      <int:MATUSETRANS action="Add"

                ' close <int:MXINVISSUE
                ' close      <int:MATUSETRANS action="Add"

                'update current record PROCESS_FLAG to 'Y'

            Next

            objXMLWriter.WriteEndElement()
            objXMLWriter.Flush()
            objXMLWriter.Close()
            Dim strXMLResult As String
            Dim objSR As StreamReader = File.OpenText(strXMLPath)
            strXMLResult = objSR.ReadToEnd()
            objSR.Close()
            objSR = Nothing

            If bolError = True Then
                Return True
            Else
                Return False
            End If
        End If

        Return bolError

    End Function

    Private Sub SendEmail()

        Dim rtn As String = "Module1.SendEmail"

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "webdev@sdi.com"

        'The subject of the email
        email.Subject = "Cytec Maximo Issues build XML Error"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>'Cytec Maximo Issues build XML' process has completed with errors, review log.</td></tr></table>"

        'Send the email and handle any error that occurs
        Try

            UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "", "Y", email.Body, connectOR)
        Catch

            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent")
        End Try

    End Sub

End Module
