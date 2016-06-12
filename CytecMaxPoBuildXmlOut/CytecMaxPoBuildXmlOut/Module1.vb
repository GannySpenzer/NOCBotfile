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
    Private myLoggr1 As appLogger = Nothing
    Private m_timeStamp As String = Now.ToString

    Dim rootDir As String = "C:\CytecMxmIn"
    Dim logpath As String = "C:\CytecMxmIn\LOGS\UpdPoCytecXmlOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\CytecMxmIn\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")

    Dim strOverride As String = ""
    Dim bolWarning As Boolean = False

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start of the Cytec Maximo PO build XML out")
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
            logpath = sLogPath & "\CytecMaxIOH_XmlBuildOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
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
        m_logger.WriteInformationLog(rtn & " :: Start of Cytec Maximo PO build XML process.")
        m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

        Dim bolError As Boolean = BuildPoOutXML()

        If bolError = True Then
            'SendEmail()
        End If

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending Cytec Maximo IOH build XML process.")

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Function BuildPoOutXML() As Boolean
        Dim bolError As Boolean = False
        Dim bolLineError As Boolean = False
        Dim ds As New DataSet

        Dim rtn As String = "Module1.BuildPoOutXML"

        Dim strSqlString As String = "SELECT * FROM SYSADM8.PS_ISA_MXM_PO_OUT WHERE PROCESS_FLAG != 'Y'"

        Try

            ds = ORDBAccess.GetAdapter(strSqlString, connectOR)

        Catch OleDBExp As OleDbException
            m_logger.WriteErrorLog(rtn & " :: Error reading source table SYSADM8.PS_ISA_MXM_PO_OUT")
            m_logger.WriteErrorLog("ERROR MSG: " & OleDBExp.Message)
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            Return True
        End Try

        If ds Is Nothing Then
            m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo POs to process at this time")
            Return False
        Else

            If ds.Tables.Count = 0 Then
                m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo POs to process at this time")
                Return False
            Else

                If ds.Tables(0).Rows.Count < 1 Then
                    m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo POs to process at this time")
                    Return False
                End If
            End If
        End If  '  ds Is Nothing - no Cytec Maximo POs to process

        Dim I As Integer = 0
        Dim rowsaffected As Integer = 0
        If ds.Tables(0).Rows.Count > 0 Then

            Dim strTemplateXml As String = System.AppDomain.CurrentDomain.BaseDirectory.ToString & "CytecMaxPoTemplate.xml"

            For I = 0 To ds.Tables(0).Rows.Count - 1
                'build SEPARATE XML file for each line in data set
                Try

                    Dim docXML As New XmlDocument
                    Dim stringer As theStringer = Nothing
                    Dim node As XmlNode = Nothing
                    Dim attrib As XmlAttribute = Nothing

                    ' load template for XML and replace header variables
                    stringer = New theStringer(Common.LoadPathFile(strTemplateXml))

                    'Dim dateOffset As New DateTimeOffset(Now, TimeZoneInfo.Local.GetUtcOffset(Now))
                    Dim dateOffset As New DateTimeOffset(Now, TimeZone.CurrentTimeZone.GetUtcOffset(Now))
                    m_timeStamp = dateOffset.ToString("s")
                    stringer.Parameters.Add("{__TIMESTAMP}", m_timeStamp)

                    ' load the template
                    docXML.LoadXml(xml:=stringer.ToString)

                    m_logger.WriteVerboseLog(rtn & " :: Template loaded")

                    ' get existing node Request
                    Dim nodeOrderReq As XmlNode = docXML.SelectSingleNode(xpath:="//Envelope//Body//MXPOInterface//Content")

                    Dim strXmlileNameStartWith As String = "CytecMaxPoOut_LineNo_" & I.ToString() & "_"
                Catch ex As Exception

                    bolLineError = True
                    'save line error and description in error files

                    m_logger.WriteErrorLog(rtn & " :: Error while building POs Out XML for the dataset Line No: " & I.ToString())
                    m_logger.WriteErrorLog(ex.Message)
                End Try

            Next  '  For I = 0 To ds.Tables(0).Rows.Count - 1

        End If

        Return bolError


    End Function

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            SendLogger(mailer.Subject, mailer.Body, "CYTECMXMPOBUILDXMLOUT", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim objException As String
            Dim objExceptionTrace As String

            'EmailTo = "vitaly.rovensky@sdi.com"
            SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

End Module
