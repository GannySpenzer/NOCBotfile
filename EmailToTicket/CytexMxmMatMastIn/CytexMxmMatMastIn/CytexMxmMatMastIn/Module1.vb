Imports System
Imports System.Data
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Xml
Imports System.Text
Imports System.Net
Imports System.Net.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter

Module Module1

    Private m_logger As appLogger = Nothing
    Private myLoggr1 As appLogger = Nothing

    Dim rootDir As String = "C:\CytecMxmIn"
    Dim logpath As String = "C:\CytecMxmIn\LOGS\UpdMatMastCytecXmlIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\CytecMxmIn\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=RPTG")
    Dim connectSQL As New SqlClient.SqlConnection("server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'")
    Dim strOverride As String
    Dim bolWarning As Boolean = False

    'Private m_arrErrXMLs As New ArrayList
    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Sub Main()

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        Dim cnStringORA As String = "Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=RPTG"

        If (cnStringORA.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            connectOR = New OleDbConnection(cnStringORA)
        End If

        Dim cnStringSQL As String = "server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'"

        If (cnStringSQL.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectSQL.Dispose()
            Catch ex As Exception
            End Try
            connectSQL = Nothing
            connectSQL = New SqlClient.SqlConnection(cnStringSQL)
        End If

        ' initialize logs

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")
        'process received info
        Call ProceesCytecMxmMatMastInfo()

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Sub ProceesCytecMxmMatMastInfo()

        Dim rtn As String = "CytecMxmMatMast.ProceesCytecMxmMatMastInfo"

        Console.WriteLine("Start CytecMxm Mat. Mast. XML in")
        Console.WriteLine("")
        m_logger.WriteInformationLog(rtn & " :: Update of CytecMxm Mat. Mast. IN XML")

        m_logger.WriteVerboseLog(rtn & " :: (Oracle)connection string : [" & connectOR.ConnectionString & "]")
        m_logger.WriteVerboseLog(rtn & " :: (SQL Server)connection string : [" & connectSQL.ConnectionString & "]")

        '// ***
        '// This is the moving of files to the XMLIN folder ...
        '// ***

        Dim dirInfo As DirectoryInfo = New DirectoryInfo("\\ims\SDIWebProcessorsXMLFiles")
        'Dim s1 As String = ""

        Dim strFiles As String
        Dim arrOrders As ArrayList
        arrOrders = New ArrayList

        strFiles = "*.XML"
        Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim I As Integer

        Try
            If aFiles.Length > 0 Then
                For I = 0 To aFiles.Length - 1
                    If aFiles(I).Name.Length > Len("CYTECMXM_MM_XML") - 1 Then
                        If aFiles(I).Name.StartsWith("CYTECMXM_MM_XML") Then
                            File.Copy(aFiles(I).FullName, "C:\CytecMxmIn\XMLIN\" & aFiles(I).Name, True)
                            File.Delete(aFiles(I).FullName)
                            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\CytecMxmIn\XMLIN\" & aFiles(I).Name)

                        End If
                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            m_logger.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\CytecMxmIn\XMLIN\ " & "...")
            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
            m_logger.WriteErrorLog(rtn & " :: End of CytecMxm Mat. Mast. IN XML.")
            'SendEmail()
            Exit Sub
        End Try
    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, mailer.From, mailer.To, mailer.Cc, mailer.Bcc, "N", mailer.Body, connectOR)
        Catch ex As Exception

        End Try
    End Sub

End Module
