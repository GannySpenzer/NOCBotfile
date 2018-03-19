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
                    ByVal strBCC As String, ByVal strDflHead As String, _
                    ByVal strMessage As String, _
                    ByVal connection As OleDbConnection)

        Dim myloadbalance As LoadBalance_March2018.SDI_loadbalance_IO = New LoadBalance_March2018.SDI_loadbalance_IO
        'Dim myloadbalance As loadbalance_dazzle2.SDI_loadbalance_IO = New loadbalance_dazzle2.SDI_loadbalance_IO
        'Dim nav As XPathNavigator
        'Dim docnav As XPathDocument
        '' open the xml file Pathlocator.xml
        'Try
        '    docnav = New XPathDocument("c:\PathLocator.xml")
        'Catch ex As Exception

        'End Try
        ''docnav = New XPathDocument("PathLocator.xml")
        '' create a navigator to query with xpath
        'nav = docnav.CreateNavigator
        ''intial xpathNavigator to start at the root
        'nav.MoveToRoot()
        ''Move to the first child node (comment field).
        'nav.MoveToFirstChild()
        'Dim root As XPathNavigator = docnav.CreateNavigator()

        'Do
        '    'Find the first element.
        '    If nav.NodeType = XPathNodeType.Element Then
        '        'if children exist
        '        If nav.HasChildren Then

        '            'Move to the first child.
        '            nav.MoveToFirstChild()

        '            'Loop through all the children.
        '            Do
        '                'Display the data.
        '                Console.Write("The XML string for this child ")
        '                Console.WriteLine("is '{0}'", nav.Value)

        '                'Check for attributes.
        '                If nav.HasAttributes Then
        '                    Console.WriteLine("This node has attributes")
        '                End If
        '            Loop While nav.MoveToNext

        '        End If
        '    End If
        'Loop While nav.MoveToNext

        'Pause.
        'Console.ReadLine()


        Dim objStreamWriterLogs As StreamWriter
        'Dim objStreamWriterFiles As StreamWriter

        Dim rootDir As String = "C:\UpdEmailOut"
        Dim logpath As String = "C:\UpdEmailOut\LOGS\UpdEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        Dim filepath As String = "C:\UpdEmailOut\FILES\UpdEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        'If Dir(rootDir, FileAttribute.Directory) = "" Then
        '    MkDir(rootDir)
        'End If
        'If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
        '    MkDir(rootDir & "\LOGS")
        'End If
        'If Dir(rootDir & "\FILES", FileAttribute.Directory) = "" Then
        '    MkDir(rootDir & "\FILES")
        'End If Stat_Change_Email_check_dir_exist
        'Dim myloadbalance_string1 As String
        'myloadbalance_string1 = myloadbalance.   .Stat_Change_Email_check_dir_exist(rootDir)
        'If myloadbalance_string1.Substring(0, 4) = "Error" Then
        '    'send email'
        '    objStreamWriterLogs = File.CreateText(logpath)
        '    objStreamWriterLogs.WriteLine("Send emails out " & Now())
        '    objStreamWriterLogs.WriteLine("    error - creating Path on server" & _
        '        strSubject & " from " & strFrom & " to " & strTo & _
        '        myloadbalance_string1)
        '    objStreamWriterLogs.Flush()
        '    objStreamWriterLogs.Close()
        '    connection.Close()
        '    Exit Sub
        'End If

        'myloadbalance_string = myloadbalance.Stat_Change_Email_Copy(filepath, strMessage)


        If Trim(strSubject) = "" Then
            strSubject = "Email from SDiExchange"  '  "Email from SDI, Inc."
             
        End If
        If Trim(strFrom) = "" Then
            strFrom = "SDIExchange@SDI.com"  '  "Insiteonline@SDI.com"
        End If
        If Trim(strTo) = "" Then
            strTo = " "
        End If
        If Trim(strCC) = "" Then
            strCC = " "
        End If
        If Trim(strBCC) = "" Then
            strBCC = " "
        End If
        If Trim(strDflHead) = "" Then
            strDflHead = "Y"
        End If
        Dim strSQLstring As String

        strSQLstring = "INSERT INTO PS_ISA_OUTBND_EML" & vbCrLf & _
            " ( ISA_EMAIL_ID, DATETIME_ADDED, EMAIL_SUBJECT_LONG, ISA_EMAIL_FROM," & vbCrLf & _
            " ISA_EMAIL_TO, ISA_EMAIL_CC, ISA_EMAIL_BCC," & vbCrLf & _
            " ISA_EMAIL_DFL_HEAD, ISA_EMAIL_TXT_FILE," & vbCrLf & _
            " ISA_STATUS, EMAIL_DATETIME, EMAIL_TEXTLONG)" & vbCrLf & _
            " VALUES (SEQ_ISA_EMAIL_ID_PK.NEXTVAL," & vbCrLf & _
            " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
            " '" & strSubject & "'," & vbCrLf & _
            " '" & strFrom & "'," & vbCrLf & _
            " '" & strTo & "'," & vbCrLf & _
            " '" & strCC & "'," & vbCrLf & _
            " '" & strBCC & "'," & vbCrLf & _
            " '" & strDflHead & "',"
        If strMessage.Length > 3999 Then
            strSQLstring = strSQLstring & " '" & filepath & "'," & vbCrLf & _
            " ' '," & vbCrLf & _
            " ''," & vbCrLf & _
            " ' ')"
        Else
            strSQLstring = strSQLstring & " ' '," & vbCrLf & _
            " ' '," & vbCrLf & _
            " ''," & vbCrLf & _
            " '" & Replace(strMessage, "'", "''") & "')"
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
        '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        ' right here we have to call the web service
        ' to copy to the the server that will house the text file 
        ' the reason is that we have a load balancer that will have three servers
        ' playing the balancing game and if we put it on the c: drive of the server the user is on 
        ' we will have a one in three chance of picking up that email
        ' so we need to direct the text file to one repository server - probably dazzle...
        ' we want to control this by going into IIS and determining where the Web Service is pointing
        Try
            If strMessage.Length > 3999 Then
                ' call webservice 
                'myloadbalance = New loadbalance.SDI_loadbalance_IO

                Dim myloadbalance_string As String
                myloadbalance_string = myloadbalance.Stat_Change_Email_Copy(filepath, strMessage)


                '&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&    
                'objStreamWriterLogs = File.CreateText(filepath)
                'objStreamWriterLogs.WriteLine(strMessage)
                'objStreamWriterLogs.Close()
            End If

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
        'If strMessage.Length > 3999 Then
        '    ' call webservice 
        '    Dim myloadbalance As loadbalance.SDI_loadbalance_IO = New loadbalance.SDI_loadbalance_IO

        '    Dim myloadbalance_string As String
        '    myloadbalance_string = myloadbalance.Stat_Change_Email_Copy(filepath, strMessage)


        '    '&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&    
        '    'objStreamWriterLogs = File.CreateText(filepath)
        '    'objStreamWriterLogs.WriteLine(strMessage)
        '    'objStreamWriterLogs.Close()
        'End If

    End Sub

    Public Shared Sub UpdEmailOutNoCon(ByVal strSubject As String, ByVal strFrom As String, _
                    ByVal strTo As String, ByVal strCC As String, _
                    ByVal strBCC As String, ByVal strDflHead As String, _
                    ByVal strMessage As String)
        Dim connection As OleDbConnection = New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=prod")
        UpdEmailOut(strSubject, strFrom, strTo, strCC, strBCC, strDflHead, strMessage, connection)

    End Sub

    Public Sub New()

    End Sub
End Class

