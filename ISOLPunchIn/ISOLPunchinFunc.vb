Imports ISOLPunchIn.ORDBData
Imports System.Data.OleDb
Imports System.Threading.Thread
Imports System.Web.Mail
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Public Class ISOLPunchINFunc


    Public Shared Function GenerateHash(ByVal SourceText As String) As String
        'Create an encoding object to ensure the encoding standard for the source text
        Dim Ue As New UnicodeEncoding
        'Retrieve a byte array based on the source text
        Dim ByteSourceText() As Byte = Ue.GetBytes(SourceText)
        'Instantiate an MD5 Provider object
        Dim Md5 As New MD5CryptoServiceProvider
        'Compute the hash value from the source
        Dim ByteHash() As Byte = Md5.ComputeHash(ByteSourceText)
        'And convert it to String format for return
        Return Convert.ToBase64String(ByteHash)
    End Function

    Public Shared Function getDBDownMsg(ByVal strDBDownMsgPath As String) As String
        Dim reader As TextReader
        If File.Exists(strDBDownMsgPath) Then
            reader = File.OpenText(strDBDownMsgPath)
        Else
            Return ""
        End If
        Dim readerline As String
        Try
            readerline = reader.ReadLine()
        Catch ex As Exception
            Return ""
        End Try
        reader.Close()
        Return readerline

    End Function

    Public Shared Sub WebLog(ByVal strUserid As String, _
                            ByVal strSiteBU As String, _
                            ByVal strUsername As String)

        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

        Dim strSQLstring As String = "INSERT INTO ps_isa_web_log" & vbCrLf & _
                " ( BUSINESS_UNIT, ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME," & vbCrLf & _
                " ISA_SDI_EMPLOYEE, ISA_LOG_PAGE, ISA_LOG_URL," & vbCrLf & _
                " ISA_LOG_IP, ISA_LOG_BROWSER, ISA_LOG_COOKIES," & vbCrLf & _
                " ISA_LOG_JAVASCRIPT, ISA_LOG_JAVAAPPLET," & vbCrLf & _
                " ISA_LOG_VBSCRIPT, ISA_LOG_ACTIVEX," & vbCrLf & _
                " ISA_LOG_PLATFORM, DT_TIMESTAMP )" & vbCrLf & _
                " VALUES ( '" & strSiteBU.ToUpper & "'," & vbCrLf & _
                " '" & strUserid.ToUpper & "'," & vbCrLf & _
                " '" & strUsername.ToUpper & "'," & vbCrLf & _
                " 'C'," & vbCrLf & _
                " '" & Left(currentApp.Request.ServerVariables("SCRIPT_NAME"), 60) & "'," & vbCrLf & _
                " '" & Left((currentApp.Request.ServerVariables("HTTP_HOST") & currentApp.Request.ServerVariables("SCRIPT_NAME")), 100) & "'," & vbCrLf & _
                " '" & currentApp.Request.ServerVariables("REMOTE_ADDR") & "'," & vbCrLf & _
                " '" & currentApp.Request.Browser.Browser & "'," & vbCrLf & _
                " '" & currentApp.Request.Browser.Cookies & "'," & vbCrLf & _
                " '" & currentApp.Request.Browser.JavaScript & "'," & vbCrLf & _
                " '" & currentApp.Request.Browser.JavaApplets & "'," & vbCrLf & _
                " '" & currentApp.Request.Browser.VBScript & "'," & vbCrLf & _
                " '" & currentApp.Request.Browser.ActiveXControls & "'," & vbCrLf & _
                " '" & currentApp.Request.Browser.Platform & "'," & vbCrLf & _
                " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))"

        Dim rowsaffected As Integer

        Dim connectionNew1 As OleDbConnection = New OleDbConnection(DbUrl)
        Dim Command21 As OleDbCommand = New OleDbCommand(strSQLstring, connectionNew1)
        Try
            connectionNew1.open()
            rowsaffected = Command21.ExecuteNonQuery()
            connectionNew1.Dispose()
            connectionNew1.close()
        Catch objException As Exception
            'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            'currentApp.Response.Write("<hr>")
            'currentApp.Response.Write("<li>Message: " & objException.Message)
            'currentApp.Response.Write("<li>Source: " & objException.Source)
            'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            'currentApp.Response.End()
        Finally
            If connectionNew1.State = ConnectionState.Open Then
                connectionNew1.Dispose()
                connectionNew1.Close()
            End If
        End Try

        Sleep(1000)

    End Sub

End Class
