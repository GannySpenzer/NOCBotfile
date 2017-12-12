Imports System.Data.OleDb
Imports System.Configuration
Imports System.Text

Module NotifyUser
    Sub Main()
        Notifications.AddToNotifyQueue()
    End Sub
End Module

Public Class Notifications
    Public Shared Sub AddToNotifyQueue()
        Try
            Dim StrQry As String = "INSERT INTO SDIX_NOTIFY_QUEUE (NOTIFY_ID,NOTIFY_TYPE,USER_ID,DTTMADDED,TITLE,LINK,HTMLMSG,STATUS,ATTACHMENTS)  " & vbCrLf & _
                                   "SELECT NOTIFY_QUEUE_SEQ.NEXTVAL,'" & ConfigurationSettings.AppSettings("NotificationType") & "',OPRIDADDED,TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'), " & vbCrLf & _
                                   "(CASE ACTION WHEN 'FINAL' THEN 'Finalization event was completed at ' || To_char(FINALDTTM,'MM/DD/YYYY HH:MI:SS AM')   " & vbCrLf & _
                                   "ELSE  'Reconciliation event was completed at ' || To_char(FINALDTTM,'MM/DD/YYYY HH:MI:SS AM') END) As Title,' ',' ','N',' '  FROM SDIX_CYCLE_COUNT_FINAL_LOG WHERE FINALDTTM BETWEEN TO_DATE('" & Now().AddMinutes(Convert.ToInt32(-ConfigurationSettings.AppSettings("IntervalTime"))) & "','MM/DD/YYYY HH:MI:SS AM') AND TO_DATE('" & Now() & "','MM/DD/YYYY HH:MI:SS AM') "
            Dim rowAffected As Integer = ORDBData.ExecNonQuery(StrQry)
        Catch ex As Exception
            Dim sMyErrorString As String = String.Empty
            If Not ex Is Nothing Then
                sMyErrorString = ex.ToString
            End If
            sMyErrorString += sMyErrorString & _
                       "Error in NotificationUtility -> AddToNotifyQueue"

            SendSDiExchErrorMail(sMyErrorString)
        End Try
    End Sub
    Public Shared Sub SendSDiExchErrorMail(ByVal strErrorMessage As String, Optional ByVal sSubject As String = "")
        Dim strComeFrom As String = "Unknown"
        Try
            strComeFrom = ConfigurationSettings.AppSettings("serverId").Trim
        Catch ex As Exception
            strComeFrom = "Unknown"
        End Try
        Dim mail As New System.Net.Mail.MailMessage()
        mail.To.Add("webdev@sdi.com")
        mail.To.Add("sdiportalsupport@avasoft.biz")
        mail.From = New System.Net.Mail.MailAddress("SDIExchADMIN@sdi.com", "SDiExchange Admin")
        ' adding server name to the error message
        mail.Body = "Error in SDiExchange  -  " & vbCrLf & strErrorMessage & ". Server Id: " & strComeFrom
        If sSubject.Trim.Length = 0 Then
            sSubject = "Error - SDiExchange"
        End If
        mail.Subject = sSubject
        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
        Try
            SDIEmailService.EmailUtilityServices("MailandStore", mail.From.ToString(), mail.To.ToString(), mail.Subject, String.Empty, String.Empty, mail.Body, "SDIERRMAIL", MailAttachmentName, MailAttachmentbytes.ToArray())
        Catch ex As Exception
            Dim strErr As String = ex.Message
        End Try
    End Sub
End Class
Public Class ORDBData
    Public Shared Function ExecNonQuery(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As Integer
        Dim rowsAffected As Integer = 0
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            rowsAffected = Command.ExecuteNonQuery()
            Try
                Command.Dispose()
            Catch ex As Exception
            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception
            End Try
        Catch objException As Exception
            rowsAffected = 0
            Try
                connection.Close()
            Catch ex As Exception
            End Try
        End Try
        Return rowsAffected
    End Function

    Public Shared ReadOnly Property DbUrl() As String
        Get
            Return ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property
End Class

