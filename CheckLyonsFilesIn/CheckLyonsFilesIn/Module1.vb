Imports System
Imports System.Data
Imports System.Windows
Imports System.Threading.Tasks
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
Imports System.Xml
Imports System.Text
Imports System.Net
Imports System.Linq
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter


Module Module1

    Private m_logger As appLogger = Nothing
    'Private myLoggr1 As appLogger = Nothing

    Dim rootDir As String = "C:\LMIn"
    Dim logpath As String = "C:\LMIn\LOGS\CheckFilesIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    'Dim sErrLogPath As String = "C:\SendFileAsAttachm\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    'Dim bolWarning As Boolean = False
    'Private m_arrXMLErrFiles As String
    'Private m_arrErrorsList As String
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR")

    Sub Main()

        Dim rtn As String = "CheckLyonsFilesIn.Main"

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        Dim cnStringORA As String = "Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR"
        Try
            cnStringORA = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
            cnStringORA = ""
        End Try
        If (cnStringORA.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            connectOR = New OleDbConnection(cnStringORA)
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
            logpath = sLogPath & "\CheckFilesIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("Start of processing - " & Now.ToString())
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")


        Dim strDbName As String = ""
        strDbName = connectOR.DataSource.ToUpper
        m_logger.WriteInformationLog(rtn & " Database: " & strDbName)

        ' check processed files dates
        CheckProcessedFileDates()

        m_logger.WriteInformationLog("End of processing - " & Now.ToString())

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

        Try
            connectOR.Close()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub CheckProcessedFileDates()

        Dim rtn As String = "CheckLyonsFilesIn.CheckProcessedFileDates"
        Dim bError As Boolean = False

        m_logger.WriteInformationLog(rtn & " - getting latest Processed file date")

        Dim sInputDir As String = rootDir & "\XMLINProcessed"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = rootDir & "\XMLINProcessed"
        End Try
        Dim dirInfo As DirectoryInfo = New DirectoryInfo(sInputDir)
        Dim strFiles As String = ""
        strFiles = "*.XML"
        Dim aSrcFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim comparer As IComparer = New DateComparer()

        Array.Sort(aSrcFiles, comparer)

        Dim I As Integer = 0
        Dim dLatestCreateDate As Date
        Dim lngDiffValue As Long = 0
        Dim strLastFile As String = ""

        Try
            If aSrcFiles.Length > 0 Then
                dLatestCreateDate = aSrcFiles(aSrcFiles.Length - 1).CreationTime

                strLastFile = "Latest Processed file date: " & dLatestCreateDate.ToString & ". "

                m_logger.WriteInformationLog(rtn & " - " & strLastFile)

                lngDiffValue = DateDiff(DateInterval.Hour, dLatestCreateDate, Now())

                If lngDiffValue > 24 Then
                    SendErrorEmail(True, strLastFile)
                    m_logger.WriteInformationLog(rtn & " - WARNING email sent ")

                End If
            End If  '  If aSrcFiles.Length > 0 Then
        Catch ex As Exception

        End Try


    End Sub

    Private Sub SendErrorEmail(Optional ByVal bIsSendOut As Boolean = False, Optional ByVal strLastFile As String = "")

        Dim rtn As String = "CheckLyonsFilesIn.SendErrorEmail"
        Dim strEmailFrom As String = ""
        Dim strEmailTo As String = ""
        Dim strEmailCC As String = ""
        Dim strEmailBcc As String = ""
        Dim strEmailBody As String = ""
        Dim strEmailSubject As String = ""

        'The email address of the sender
        strEmailFrom = "TechSupport@sdi.com"
        If (CStr(My.Settings(propertyName:="onErrorEmail_From")) <> "") Then
            strEmailFrom = CStr(My.Settings(propertyName:="onErrorEmail_From")).Trim
        End If

        'The email address of the recipient. 
        strEmailTo = "webdev@sdi.com"
        If bIsSendOut Then
            If (CStr(My.Settings(propertyName:="onErrorEmail_To")) <> "") Then
                strEmailTo = CStr(My.Settings(propertyName:="onErrorEmail_To")).Trim
            End If
        End If

        If (CStr(My.Settings(propertyName:="onErrorEmail_CC")) <> "") Then
            strEmailCC = CStr(My.Settings(propertyName:="onErrorEmail_CC")).Trim
        Else
            strEmailCC = ""
        End If

        If (CStr(My.Settings(propertyName:="onErrorEmail_BCC")) <> "") Then
            strEmailBcc = CStr(My.Settings(propertyName:="onErrorEmail_BCC")).Trim
        Else
            strEmailBcc = "webdev@sdi.com"
        End If

        Dim strAddSDILogo As String = ""
        strEmailBody = ""
        strAddSDILogo = "<html><body><img src='https://www.sdiexchange.com/images/SDI_Logo2017_yellow.jpg' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >Check for Lyons Files Sent to SDI</span></center>&nbsp;&nbsp;"

        strEmailBody &= "<table><tr><td>No files from LYONS were received by SDI for the last 24 hours.</td></tr>" & vbCrLf

        If Trim(strLastFile) <> "" Then
            strEmailBody &= "<tr><td>" & Trim(strLastFile) & vbCrLf & "</td></tr>"
        End If

        strEmailSubject = "No LYONS files were received by SDI for the last 24 hours."

        ''VR 12/18/2014 Adding file names and error descriptions in message body
        'Dim sInfoErr As String = ""

        'Try

        '    sInfoErr &= " File name(s) are below.</td></tr>"
        '    If Not m_arrXMLErrFiles Is Nothing Then
        '        If Trim(m_arrXMLErrFiles) <> "" Then
        '            Dim arrErrFiles1() As String = Split(m_arrXMLErrFiles, ",")
        '            Dim arrErrDescr2() As String = Split(m_arrErrorsList, ",")
        '            If arrErrFiles1.Length > 0 Then
        '                For i1 As Integer = 0 To arrErrFiles1.Length - 1
        '                    sInfoErr &= "<tr><td>" & arrErrFiles1(i1) & "</td><td>&nbsp;&nbsp" & arrErrDescr2(i1) & "</td></tr>"
        '                    'strErrListForSFTP &= arrErrFiles1(i1) & " - " & arrErrDescr2(i1) & vbCrLf
        '                Next
        '            End If
        '        End If
        '    End If
        '    strEmailBody &= sInfoErr
        'Catch ex As Exception

        '    strEmailBody &= " review log.</td></tr>"
        'End Try

        strEmailBody &= " <tr><td>Please check LYONS connection to SDI.</td></tr>"
        strEmailBody &= "</table>"

        strEmailBody &= "&nbsp;<br>Sincerely,<br>&nbsp;<br>SDI Customer Care<br>&nbsp;<br></p></div><BR>"
        strEmailBody &= "<br />"


        Dim sApp As String = "" & _
                             System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).Name & _
                             " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                            ""
        Try
            strEmailBody &= "" & _
                          "<p style=""text-align:right;font-size:10px"">" & _
                          sApp & _
                          "</p>" & _
                          ""
        Catch ex As Exception
        End Try

        Dim strEmailBodyEnd As String = "</body></html>"

        strEmailBody = strAddSDILogo & strEmailBody
        strEmailBody &= "" & _
                    "<HR width='100%' SIZE='1'>" & _
                    "<img src='https://www.sdiexchange.com/Images/SDIFooter_Email.png' />"
        strEmailBody &= "<br><P><CENTER><SPAN style='FONT-SIZE: 12pt'><SPAN style='FONT-SIZE: 12pt'><FONT color=teal size=2>The information in this communication, including any attachments, is the property of SDI, Inc,&nbsp;</SPAN>is intended only for the addressee and may contain confidential, proprietary, and/or privileged material. Any review, retransmission, dissemination or other use of, or taking of any action in reliance upon, this information by persons or entities other than the intended recipient is prohibited. If you received this in error, please immediately contact the sender by replying to this email and delete the material from all computers.</FONT></SPAN></CENTER></P>"

        strEmailBody &= "</body></html>"

        If connectOR.DataSource.ToUpper.IndexOf("RPTG") > -1 Or _
           connectOR.DataSource.ToUpper.IndexOf("DEVL") > -1 Or _
           connectOR.DataSource.ToUpper.IndexOf("STAR") > -1 Or _
           connectOR.DataSource.ToUpper.IndexOf("PLGR") > -1 Then
            strEmailTo = "webdev@sdi.com"
            strEmailSubject = " (test run) " & strEmailSubject
        End If

        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()

        Dim bSend As Boolean = False
        Dim strReturn As String = ""
        Try

            strReturn = SendLogger(strEmailSubject, strEmailBody, "CHECKLYONSFILESIN", "Mail", strEmailTo, strEmailCC, strEmailBcc, MailAttachmentName, MailAttachmentbytes.ToArray())
            bSend = True
            'm_arrXMLErrFiles = ""
            'm_arrErrorsList = ""
        Catch ex As Exception
            bSend = False
            strReturn = "Error sending email: " & ex.ToString
        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: " & strReturn)
        End If
    End Sub

    Public Function SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, _
            ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String, _
            ByVal MailAttachmentName() As String, ByVal MailAttachmentbytes()() As Byte) As String

        Dim strReturn As String = ""
        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Try

            'EmailTo = "vitaly.rovensky@sdi.com"
            strReturn = SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception
            strReturn = "Error sending email: " & ex.ToString
        End Try

        SDIEmailService = Nothing

        Return strReturn

    End Function

    Private Class DateComparer
        Implements System.Collections.IComparer

        Public Function Compare(ByVal info1 As Object, ByVal info2 As Object) As Integer Implements System.Collections.IComparer.Compare
            Dim FileInfo1 As System.IO.FileInfo = DirectCast(info1, System.IO.FileInfo)
            Dim FileInfo2 As System.IO.FileInfo = DirectCast(info2, System.IO.FileInfo)

            Dim Date1 As DateTime = FileInfo1.CreationTime
            Dim Date2 As DateTime = FileInfo2.CreationTime

            If Date1 > Date2 Then Return 1
            If Date1 < Date2 Then Return -1
            Return 0
        End Function
    End Class

End Module
