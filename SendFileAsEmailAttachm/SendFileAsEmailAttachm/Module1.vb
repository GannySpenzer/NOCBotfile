Imports System
Imports System.Data
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
Imports System.Xml
Imports System.Text
Imports System.Net
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter

Module Module1

    Private m_logger As appLogger = Nothing
    Private myLoggr1 As appLogger = Nothing

    Dim rootDir As String = "C:\SendFileAsAttachm"
    Dim logpath As String = "C:\SendFileAsAttachm\LOGS\SendFileAsAttachm" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\SendFileAsAttachm\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Sub Main()

        Dim rtn As String = "Module1.Main"

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

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
            logpath = sLogPath & "\SendFileAsAttachm" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        ' initialize logs

        myLoggr1 = New SDI.ApplicationLogger.appLogger(sErrLogPath, TraceLevel.Error)
        myLoggr1.WriteErrorLog("Error Log Created")

        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("Start of processing - " & Now.ToString())
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        ' process input files
        '

        myLoggr1.WriteErrorLog("End of processing - " & Now.ToString())

        ' destroy logger object
        Try
            myLoggr1.Dispose()
            'm_POConfirm_Logger.Dispose()
        Catch ex As Exception
        Finally
            myLoggr1 = Nothing
            'm_POConfirm_Logger = Nothing
        End Try

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Sub SendEmailWithAttachm(ByVal strEmail As String, ByVal strName As String, Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "KLATencorFromPOToINTF.SendEmail"
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
        strEmailTo = "vitaly.rovensky@sdi.com"
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
        strAddSDILogo = "<html><body><img src='https://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >KLA-Tencor From PO To INTF Error</span></center>&nbsp;&nbsp;"

        strEmailBody &= "<table><tr><td>KLA-Tencor From PO To INTF tables has completed with "
        'If bolWarning = True Then
        '    strEmailBody &= "warnings,"
        '    strEmailSubject = " KLA-Tencor From PO To INTF tables Warning"
        'Else
        strEmailBody &= "errors;"
        strEmailSubject = " KLA-Tencor From PO To INTF tables Error"
        'End If

        'VR 12/18/2014 Adding file names and error descriptions in message body
        Dim sInfoErr As String = ""
        Dim strErrListForSFTP As String = ""
        Try

            sInfoErr &= " XML file name(s) are below.</td></tr>"
            If Not m_arrXMLErrFiles Is Nothing Then
                If Trim(m_arrXMLErrFiles) <> "" Then
                    Dim arrErrFiles1() As String = Split(m_arrXMLErrFiles, ",")
                    Dim arrErrDescr2() As String = Split(m_arrErrorsList, ",")
                    If arrErrFiles1.Length > 0 Then
                        For i1 As Integer = 0 To arrErrFiles1.Length - 1
                            sInfoErr &= "<tr><td>" & arrErrFiles1(i1) & "</td><td>&nbsp;&nbsp" & arrErrDescr2(i1) & "</td></tr>"
                            strErrListForSFTP &= arrErrFiles1(i1) & " - " & arrErrDescr2(i1) & vbCrLf
                        Next
                    End If
                End If
            End If
            strEmailBody &= sInfoErr
        Catch ex As Exception

            strEmailBody &= " review log.</td></tr>"
        End Try

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

        'If connectOR.DataSource.ToUpper.IndexOf("RPTG") > -1 Or _
        '   connectOR.DataSource.ToUpper.IndexOf("DEVL") > -1 Or _
        '   connectOR.DataSource.ToUpper.IndexOf("STAR") > -1 Or _
        '   connectOR.DataSource.ToUpper.IndexOf("PLGR") > -1 Then
        '    strEmailTo = "WEBDEV@sdi.com"
        '    strEmailSubject = " (test run) " & strEmailSubject
        'End If

        Dim bSend As Boolean = False
        Try

            SendLogger(strEmailSubject, strEmailBody, "KLATENCORTOINTF", "Mail", strEmailTo, strEmailCC, strEmailBcc)
            bSend = True
            m_arrXMLErrFiles = ""
            m_arrErrorsList = ""
        Catch ex As Exception

        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent.")
        End If
    End Sub

    Private Sub SendErrorEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "KLATencorFromPOToINTF.SendEmail"
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
        strEmailTo = "vitaly.rovensky@sdi.com"
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
        strAddSDILogo = "<html><body><img src='https://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >KLA-Tencor From PO To INTF Error</span></center>&nbsp;&nbsp;"

        strEmailBody &= "<table><tr><td>KLA-Tencor From PO To INTF tables has completed with "
        'If bolWarning = True Then
        '    strEmailBody &= "warnings,"
        '    strEmailSubject = " KLA-Tencor From PO To INTF tables Warning"
        'Else
        strEmailBody &= "errors;"
        strEmailSubject = " KLA-Tencor From PO To INTF tables Error"
        'End If

        'VR 12/18/2014 Adding file names and error descriptions in message body
        Dim sInfoErr As String = ""
        Dim strErrListForSFTP As String = ""
        Try

            sInfoErr &= " XML file name(s) are below.</td></tr>"
            If Not m_arrXMLErrFiles Is Nothing Then
                If Trim(m_arrXMLErrFiles) <> "" Then
                    Dim arrErrFiles1() As String = Split(m_arrXMLErrFiles, ",")
                    Dim arrErrDescr2() As String = Split(m_arrErrorsList, ",")
                    If arrErrFiles1.Length > 0 Then
                        For i1 As Integer = 0 To arrErrFiles1.Length - 1
                            sInfoErr &= "<tr><td>" & arrErrFiles1(i1) & "</td><td>&nbsp;&nbsp" & arrErrDescr2(i1) & "</td></tr>"
                            strErrListForSFTP &= arrErrFiles1(i1) & " - " & arrErrDescr2(i1) & vbCrLf
                        Next
                    End If
                End If
            End If
            strEmailBody &= sInfoErr
        Catch ex As Exception

            strEmailBody &= " review log.</td></tr>"
        End Try

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

        'If connectOR.DataSource.ToUpper.IndexOf("RPTG") > -1 Or _
        '   connectOR.DataSource.ToUpper.IndexOf("DEVL") > -1 Or _
        '   connectOR.DataSource.ToUpper.IndexOf("STAR") > -1 Or _
        '   connectOR.DataSource.ToUpper.IndexOf("PLGR") > -1 Then
        '    strEmailTo = "WEBDEV@sdi.com"
        '    strEmailSubject = " (test run) " & strEmailSubject
        'End If

        Dim bSend As Boolean = False
        Try

            SendLogger(strEmailSubject, strEmailBody, "KLATENCORTOINTF", "Mail", strEmailTo, strEmailCC, strEmailBcc)
            bSend = True
            m_arrXMLErrFiles = ""
            m_arrErrorsList = ""
        Catch ex As Exception

        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent.")
        End If
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)

        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            'EmailTo = "vitaly.rovensky@sdi.com"
            SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

End Module
