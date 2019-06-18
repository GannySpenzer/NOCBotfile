Imports System.Data.OleDb
Imports System.IO
Imports System.Xml
Imports System.Xml.XPath
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Xml.Serialization

Public Class UpdEmailOut

    Public Shared Sub UpdEmailOut(ByVal strSubject As String, ByVal strFrom As String, _
                    ByVal strTo As String, ByVal strCC As String, _
                    ByVal strBCC As String, ByVal strEmailType As String, _
                    ByVal strMessage As String, _
                    ByVal connection As OleDbConnection)

        Dim objStreamWriterLogs As StreamWriter

        Dim rootDir As String = "\\sdicwsvsp\UpdEmailOut"
        Try
            rootDir = My.Settings("RootDirString").ToString.Trim
        Catch ex As Exception
            rootDir = "\\sdicwsvsp\UpdEmailOut"
        End Try
        Dim logpath As String = rootDir & "\LOGS\UpdEmailOutLog" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        Dim filepath As String = rootDir & "\FILES\UpdEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

        If Trim(strSubject) = "" Then
            strSubject = "Email from SDiExchange"  '  "Email from SDI, Inc."

        End If
        If Trim(strFrom) = "" Then
            strFrom = "SDIExchange@SDI.com"  '  "Insiteonline@SDI.com"
        End If
        If Trim(strTo) = "" Then
            strTo = "webdev@sdi.com"
            strSubject = strSubject & " - strTo is Empty - goes to WEBDEV"
        End If
        If Trim(strCC) = "" Then
            strCC = " "
        End If
        If Trim(strBCC) = "" Then
            strBCC = "webdev@sdi.com"
        Else
            strBCC = Trim(strBCC) & ";webdev@sdi.com"
        End If
        'If Trim(strDflHead) = "" Then
        '    strDflHead = "Y"
        'End If

        '     PS_ISA_OUTBND_EML
        'ISA_EMAIL_ID                              NOT NULL NUMBER(38)
        'DATETIME_ADDED                                     DATE
        'EMAIL_SUBJECT_LONG                        NOT NULL VARCHAR2(80)
        'ISA_EMAIL_FROM                            NOT NULL VARCHAR2(254
        'ISA_EMAIL_TO                              NOT NULL VARCHAR2(254
        'ISA_EMAIL_CC                              NOT NULL VARCHAR2(254
        'ISA_EMAIL_BCC                             NOT NULL VARCHAR2(254
        'ISA_EMAIL_DFL_HEAD                        NOT NULL VARCHAR2(1)
        'ISA_EMAIL_TXT_FILE --> filepath           NOT NULL VARCHAR2(100
        'ISA_STATUS                                NOT NULL VARCHAR2(1)
        'EMAIL_DATETIME                                     DATE
        'EMAIL_TEXTLONG  --> email body                     LONG

        ' SDIXEMAIL

        'EMAILKEY      NOT NULL NUMBER(12)     -  ISA_EMAIL_ID  
        'EMAILFROM              VARCHAR2(60)   - ISA_EMAIL_FROM 
        'EMAILTO                VARCHAR2(250)  - ISA_EMAIL_TO
        'EMAILSUBJECT           VARCHAR2(250)   - EMAIL_SUBJECT_LONG
        'EMAILCC                VARCHAR2(250)  - ISA_EMAIL_CC
        'EMAILBCC               VARCHAR2(250)  - ISA_EMAIL_BCC 
        'EMAILBODYPATH          VARCHAR2(250)  - ISA_EMAIL_TXT_FILE --> filepath 
        'EMAILTYPE              VARCHAR2(20)   - <EMAILTYPE> - newer param - like "MATSTOCK"
        'EMAILRESENDID - not required -  VARCHAR2(12)   
        'DT_TIMESTAMP  NOT NULL TIMESTAMP(6)    - DATETIME_ADDED --> CURRENT_TIMESTAMP or Now()
        'OPRID                  VARCHAR2(8)   - newer param --> like "VROV1" 
        'EMAILBODY              VARCHAR2(4000)  - EMAIL_TEXTLONG  --> email body
        'BATCH_PRINT --> 'Y'    VARCHAR2(1)  - change to 'N' after emailed - in SendCustEmails
        'BATCH_DTTM - not required here -   DATE  - in SendCustEmails: EMAIL_DATETIME --> CURRENT_TIMESTAMP 

        Dim strSQLstring As String = ""
        Dim strEmailKey As String = ""
        Dim strEmlKeySql As String = "SELECT Email_sequence.nextval FROM DUAL"
        Try
            Dim CommandEml As OleDbCommand = New OleDbCommand(strEmlKeySql, connection)
            If Not connection Is Nothing AndAlso ((connection.State And ConnectionState.Open) = ConnectionState.Open) Then
                connection.Close()
            End If

            connection.Open()
            strEmailKey = CommandEml.ExecuteScalar()
            connection.Close()
        Catch exEmlKey As Exception
            objStreamWriterLogs = File.CreateText(logpath)
            objStreamWriterLogs.WriteLine("Send emails out - get Email_sequence.nextval " & Now())
            objStreamWriterLogs.WriteLine("    error - " & _
                strSubject & " from " & strFrom & " to " & strTo & _
                exEmlKey.Message)
            objStreamWriterLogs.Flush()
            objStreamWriterLogs.Close()
            connection.Close()
            Exit Sub
        End Try
        
        strSQLstring = "INSERT INTO SDIXEMAIL(EMAILKEY,EMAILFROM,EMAILTO,EMAILSUBJECT,EMAILCC,EMAILBCC" & _
            ",EMAILBODY,EMAILBODYPATH,EMAILTYPE,DT_TIMESTAMP, BATCH_PRINT) " & vbCrLf & _
            " VALUES ('" & strEmailKey & "','" & strFrom & "','" & strTo & "','" & strSubject & "','" & strCC & "','" & strBCC & "'," & vbCrLf & _
            ""

        strMessage = Replace(strMessage, "'", " ")
        If Trim(strMessage) = "" Then
            strMessage = " "
        Else
            strMessage = Trim(strMessage)
        End If

        If strMessage.Length > 3999 Then
            strSQLstring = strSQLstring & " ' '," & vbCrLf & _
            " '" & filepath & "'," & vbCrLf & _
            " '" & strEmailType & "', TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
            " 'Y')"
        Else
            strSQLstring = strSQLstring & " '" & strMessage & "'," & vbCrLf & _
           " ' '," & vbCrLf & _
           " '" & strEmailType & "', TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'), " & vbCrLf & _
           " 'Y')"
        End If
        Dim rowsaffected As Integer
        Try
            Dim Command As OleDbCommand = New OleDbCommand(strSQLstring, connection)
            If Not connection Is Nothing AndAlso ((connection.State And ConnectionState.Open) = ConnectionState.Open) Then
                connection.Close()
            End If

            connection.Open()
            rowsaffected = Command.ExecuteNonQuery()
            connection.Close()
        Catch objException As Exception
            objStreamWriterLogs = File.CreateText(logpath)
            objStreamWriterLogs.WriteLine("Send emails out " & Now())
            objStreamWriterLogs.WriteLine("    error - " & _
                strSubject & " from " & strFrom & " to " & strTo & _
                objException.Message)
            objStreamWriterLogs.Flush()
            objStreamWriterLogs.Close()
            connection.Close()
            Exit Sub
        End Try

        objStreamWriterLogs = File.CreateText(filepath)
        Try
            If strMessage.Length > 3999 Then

                objStreamWriterLogs.WriteLine(strMessage)
                objStreamWriterLogs.Flush()
                objStreamWriterLogs.Close()

            End If

        Catch objException As Exception
            objStreamWriterLogs.WriteLine("Send emails out - save to file - " & Now())
            objStreamWriterLogs.WriteLine("    error - " & _
                strSubject & " from " & strFrom & " to " & strTo & vbCrLf & _
                "File Path: " & filepath & vbCrLf & _
                objException.Message)
            objStreamWriterLogs.Flush()
            objStreamWriterLogs.Close()
            Exit Sub
        End Try

    End Sub

    Public Shared Sub UpdEmailOutNoCon(ByVal strSubject As String, ByVal strFrom As String, _
                    ByVal strTo As String, ByVal strCC As String, _
                    ByVal strBCC As String, ByVal strEmailType As String, _
                    ByVal strMessage As String)
        Dim connection As OleDbConnection = New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG")
        UpdEmailOut(strSubject, strFrom, strTo, strCC, strBCC, strEmailType, strMessage, connection)

    End Sub

    Public Sub New()

    End Sub
End Class

