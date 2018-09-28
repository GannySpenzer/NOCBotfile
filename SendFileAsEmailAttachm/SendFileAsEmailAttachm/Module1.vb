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

    Dim bolWarning As Boolean = False
    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR")

    Sub Main()

        Dim rtn As String = "SendFileAsAttachm.Main"

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
            logpath = sLogPath & "\SendFileAsAttachm" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

            sErrLogPath = sLogPath & "\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
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
        ProceesFilesToBeSent()

        myLoggr1.WriteErrorLog("End of processing - " & Now.ToString())

        ' destroy logger object
        Try
            myLoggr1.Dispose()
        Catch ex As Exception
        Finally
            myLoggr1 = Nothing
        End Try

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

    Sub ProceesFilesToBeSent()

        Dim rtn As String = "SendFileAsAttachm.ProceesFilesToBeSent"
        Dim bError As Boolean = False

        '// ***
        '// This is the moving of files to the XLSXIN folder ...
        '// ***

        Dim sInputDir As String = rootDir & "\XLSXINSource"
        Try
            sInputDir = My.Settings("inputDirectory").ToString.Trim
        Catch ex As Exception
            sInputDir = rootDir & "\XLSXINSource"
        End Try
        Dim dirInfo As DirectoryInfo = New DirectoryInfo(sInputDir)

        Dim strFiles As String = ""
        Dim arrOrders As ArrayList
        arrOrders = New ArrayList

        strFiles = "*.XLSX"
        Dim aSrcFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim I As Integer
        Dim strDirIn As String = "C:\SendFileAsAttachm\XLSXIN\"
        Try
            strDirIn = My.Settings("DirectoryIn").ToString.Trim
        Catch ex As Exception
            strDirIn = "C:\SendFileAsAttachm\XLSXIN\"
        End Try

        Try
            If aSrcFiles.Length > 0 Then
                For I = 0 To aSrcFiles.Length - 1
                    If aSrcFiles(I).Name.Length > 10 Then
                        File.Copy(aSrcFiles(I).FullName, strDirIn & aSrcFiles(I).Name, True)
                        'File.Delete(aSrcFiles(I).FullName)
                        m_logger.WriteInformationLog(rtn & " :: " & aSrcFiles(I).FullName & " moved to " & strDirIn & aSrcFiles(I).Name)

                    End If
                Next
            Else
                m_logger.WriteInformationLog(rtn & " :: No files to copy from " & dirInfo.FullName)
            End If
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to " & strDirIn & " ...")
            myLoggr1.WriteErrorLog(rtn & " :: " & ex.ToString)
            bError = True
            Dim strXMLError As String = ex.Message
            If Trim(m_arrXMLErrFiles) = "" Then
                m_arrXMLErrFiles = aSrcFiles(I).FullName
            Else
                m_arrXMLErrFiles &= "," & aSrcFiles(I).FullName
            End If
            If Trim(strXMLError) <> "" Then
                If Len(strXMLError) > 250 Then
                    strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                End If
                If Trim(m_arrErrorsList) = "" Then
                    m_arrErrorsList = strXMLError
                Else
                    m_arrErrorsList &= "," & strXMLError
                End If
            Else
                If Trim(m_arrErrorsList) = "" Then
                    m_arrErrorsList = "No Error Description"
                Else
                    m_arrErrorsList &= "," & "No Error Description"
                End If
            End If
        End Try

        If bError Then
            SendErrorEmail(True)
        End If

        Try
            bError = False
            bError = PrepareAndSendEmails()
        Catch ex As Exception
            myLoggr1.WriteErrorLog(rtn & " :: " & ex.ToString)
            bError = True
        End Try

        If bError Then
            SendErrorEmail(True)
        End If
       
    End Sub

    Private Async Function CopyFileAsync(ByVal strStartFile As String, ByVal strEndFile As String) As Task(Of Boolean)

        Dim SourceStream As New FileStream(strStartFile, FileMode.Open, FileAccess.Read)
        Dim DestinationStream As New FileStream(strEndFile, FileMode.Create, FileAccess.Write)

        Using (SourceStream)
            Using (DestinationStream)
                Await SourceStream.CopyToAsync(DestinationStream)

            End Using
        End Using

    End Function

    Private Function PrepareAndSendEmails() As Boolean

        Dim rtn As String = "SendFileAsAttachm.PrepareAndSendEmails"
        Dim bolError As Boolean = False

        m_logger.WriteInformationLog(rtn & " :: Start analyzing input files")

        Dim strDirIn As String = "C:\SendFileAsAttachm\XLSXIN\"
        Try
            strDirIn = My.Settings("DirectoryIn").ToString.Trim
        Catch ex As Exception
            strDirIn = "C:\SendFileAsAttachm\XLSXIN\"
        End Try

        Dim dirInfo As DirectoryInfo = New DirectoryInfo(strDirIn)
        Dim strFiles As String = ""
        Dim strXMLError As String = ""
        Dim bLineError As Boolean = False

        Dim I As Integer

        strFiles = "*.XLSX"
        Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)

        Dim sInputFilename As String = ""
        Dim strSupplierID As String = ""
        Dim strSqlStrng1 As String = ""
        Dim strEmail As String = ""
        Dim strName As String = ""

        Try
            m_arrXMLErrFiles = ""
            m_arrErrorsList = ""
            bolError = False

            If aFiles.Length > 0 Then
                For I = 0 To aFiles.Length - 1

                    sInputFilename = aFiles(I).Name
                    bLineError = False
                    strXMLError = ""
                    strEmail = ""
                    strName = ""

                    m_logger.WriteInformationLog(rtn & " :: Start File " & sInputFilename)
                    If Len(sInputFilename) > 10 Then
                        If Mid(sInputFilename, 11, 1) = "_" Then
                            strSupplierID = Microsoft.VisualBasic.Left(sInputFilename, 10)
                            'get supplier info from SDIX_EMAIL_SRC table and send email
                            strSqlStrng1 = "SELECT * FROM SDIX_EMAIL_SRC  WHERE SUPPLIER_ID = '" & strSupplierID & "'"
                            Dim OrderListDataSet As DataSet = Nothing
                            Dim bIsSupplIdFound As Boolean = False

                            Try
                                Dim Command As OleDbCommand = New OleDbCommand(strSqlStrng1, connectOR)
                                Command.CommandTimeout = 120
                                connectOR.Open()
                                Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(Command)

                                OrderListDataSet = New System.Data.DataSet()

                                dataAdapter.Fill(OrderListDataSet)

                                If Not OrderListDataSet Is Nothing Then
                                    If OrderListDataSet.Tables.Count > 0 Then
                                        If OrderListDataSet.Tables(0).Rows.Count > 0 Then
                                            bIsSupplIdFound = True
                                        End If
                                    End If
                                End If  '  If Not OrderListDataSet Is Nothing Then

                                If bIsSupplIdFound Then
                                    ' strName = OrderListDataSet.Tables(0).Rows(0).Item("CONTACT_NAME").ToString()
                                    strEmail = OrderListDataSet.Tables(0).Rows(0).Item("CONTACT_EMAIL").ToString()
                                    If Trim(strEmail) <> "" Then
                                        Dim bSuccess As Boolean = False
                                        Dim strErrorMsg As String = ""
                                        'send email 
                                        bSuccess = SendEmailWithAttachm(strEmail, strSupplierID, aFiles(I).FullName, aFiles(I).Name, strErrorMsg)

                                        If bSuccess Then
                                            File.Copy(aFiles(I).FullName, "\\christina2012\dfs\Public\NYC_Custodial\OTD\Archive\" & aFiles(I).Name, True)

                                            ''copy to Archive directory
                                            'Dim bolError1 As Task(Of Boolean)
                                            'Dim strStartFile As String = aFiles(I).FullName
                                            'Dim strEndFile As String = "\\christina2012\dfs\Public\NYC_Custodial\OTD\Archive\" & aFiles(I).Name
                                            'bolError1 = CopyFileAsync(strStartFile, strEndFile)

                                            m_logger.WriteInformationLog(rtn & " :: File emailed successfully:  " & sInputFilename)
                                        Else
                                            'copy to Bad Directory
                                            strXMLError = "Email was not sent for this Supplier ID: " & strSupplierID & vbCrLf
                                            'add error message
                                            strXMLError = strXMLError & ""
                                            m_logger.WriteInformationLog(rtn & " :: " & strXMLError)
                                            If Trim(m_arrXMLErrFiles) = "" Then
                                                m_arrXMLErrFiles = aFiles(I).Name
                                            Else
                                                m_arrXMLErrFiles &= "," & aFiles(I).Name
                                            End If
                                            If Trim(strXMLError) <> "" Then
                                                If Len(strXMLError) > 250 Then
                                                    strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                                                End If
                                                If Trim(m_arrErrorsList) = "" Then
                                                    m_arrErrorsList = strXMLError
                                                Else
                                                    m_arrErrorsList &= "," & strXMLError
                                                End If
                                            Else
                                                If Trim(m_arrErrorsList) = "" Then
                                                    m_arrErrorsList = "Check Log for the Error Description"
                                                Else
                                                    m_arrErrorsList &= "," & "Check Log for the Error Description"
                                                End If
                                            End If
                                            'move file to BadXML folder
                                            File.Copy(aFiles(I).FullName, rootDir & "\BadXLSX\" & aFiles(I).Name, True)
                                            'File.Delete(aFiles(I).FullName)
                                            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & rootDir & "\BadXLSX\" & aFiles(I).Name)

                                            bolError = True
                                        End If
                                        ' BadFile path: \\christina2012\dfs\Public\NYC_Custodial\OTD\BadXLSX
                                    Else
                                        'copy to bad dir
                                        strXMLError = "No CONTACT_NAME or CONTACT_EMAIL in SDIX_EMAIL_SRC for this Supplier ID: " & strSupplierID
                                        m_logger.WriteInformationLog(rtn & " :: " & strXMLError)
                                        If Trim(m_arrXMLErrFiles) = "" Then
                                            m_arrXMLErrFiles = aFiles(I).Name
                                        Else
                                            m_arrXMLErrFiles &= "," & aFiles(I).Name
                                        End If
                                        If Trim(strXMLError) <> "" Then
                                            If Len(strXMLError) > 250 Then
                                                strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                                            End If
                                            If Trim(m_arrErrorsList) = "" Then
                                                m_arrErrorsList = strXMLError
                                            Else
                                                m_arrErrorsList &= "," & strXMLError
                                            End If
                                        Else
                                            If Trim(m_arrErrorsList) = "" Then
                                                m_arrErrorsList = "Check Log for the Error Description"
                                            Else
                                                m_arrErrorsList &= "," & "Check Log for the Error Description"
                                            End If
                                        End If
                                        'move file to BadXML folder
                                        File.Copy(aFiles(I).FullName, rootDir & "\BadXLSX\" & aFiles(I).Name, True)
                                        'File.Delete(aFiles(I).FullName)
                                        m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & rootDir & "\BadXLSX\" & aFiles(I).Name)

                                        bolError = True
                                    End If
                                Else
                                    'no Suppl ID info
                                    strXMLError = "No info in SDIX_EMAIL_SRC for this Supplier ID: " & strSupplierID
                                    m_logger.WriteInformationLog(rtn & " :: " & strXMLError)
                                    'If Trim(m_arrXMLErrFiles) = "" Then
                                    '    m_arrXMLErrFiles = aFiles(I).Name
                                    'Else
                                    '    m_arrXMLErrFiles &= "," & aFiles(I).Name
                                    'End If
                                    'If Trim(strXMLError) <> "" Then
                                    '    If Len(strXMLError) > 250 Then
                                    '        strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                                    '    End If
                                    '    If Trim(m_arrErrorsList) = "" Then
                                    '        m_arrErrorsList = strXMLError
                                    '    Else
                                    '        m_arrErrorsList &= "," & strXMLError
                                    '    End If
                                    'Else
                                    '    If Trim(m_arrErrorsList) = "" Then
                                    '        m_arrErrorsList = "Check Log for the Error Description"
                                    '    Else
                                    '        m_arrErrorsList &= "," & "Check Log for the Error Description"
                                    '    End If
                                    'End If
                                   
                                    'bolError = True
                                End If
                                connectOR.Close()
                            Catch exDS As Exception
                                Try
                                    connectOR.Close()
                                Catch ex As Exception

                                End Try
                                strXMLError = exDS.ToString()
                                If Trim(m_arrXMLErrFiles) = "" Then
                                    m_arrXMLErrFiles = aFiles(I).Name
                                Else
                                    m_arrXMLErrFiles &= "," & aFiles(I).Name
                                End If
                                If Trim(strXMLError) <> "" Then
                                    If Len(strXMLError) > 250 Then
                                        strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                                    End If
                                    If Trim(m_arrErrorsList) = "" Then
                                        m_arrErrorsList = strXMLError
                                    Else
                                        m_arrErrorsList &= "," & strXMLError
                                    End If
                                Else
                                    If Trim(m_arrErrorsList) = "" Then
                                        m_arrErrorsList = "Check Log for the Error Description"
                                    Else
                                        m_arrErrorsList &= "," & "Check Log for the Error Description"
                                    End If
                                End If
                                'move file to BadXML folder
                                File.Copy(aFiles(I).FullName, rootDir & "\BadXLSX\" & aFiles(I).Name, True)

                                m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & rootDir & "\BadXLSX\" & aFiles(I).Name)

                                bolError = True
                            End Try
                        End If  '  If Mid(sInputFilename, 11, 1) = "_" Then
                    Else
                        'copy file to BadXLSX directory
                        m_logger.WriteInformationLog(rtn & " :: Short File Name " & sInputFilename)
                        'File.Copy(aFiles(I).FullName, "C:\SendFileAsAttachm\BadXLSX\" & aFiles(I).Name, True)
                        ''File.Delete(aFiles(I).FullName)
                        'm_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\SendFileAsAttachm\BadXLSX\" & aFiles(I).Name)

                        'bolError = True
                    End If
                Next  '  For I = 0 To aFiles.Length - 1

            End If  '  If aFiles.Length > 0 Then

        Catch ex As Exception
            strXMLError = ex.ToString()
            If Trim(m_arrXMLErrFiles) = "" Then
                m_arrXMLErrFiles = aFiles(I).Name
            Else
                m_arrXMLErrFiles &= "," & aFiles(I).Name
            End If
            If Trim(strXMLError) <> "" Then
                If Len(strXMLError) > 250 Then
                    strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
                End If
                If Trim(m_arrErrorsList) = "" Then
                    m_arrErrorsList = strXMLError
                Else
                    m_arrErrorsList &= "," & strXMLError
                End If
            Else
                If Trim(m_arrErrorsList) = "" Then
                    m_arrErrorsList = "Check Log for the Error Description"
                Else
                    m_arrErrorsList &= "," & "Check Log for the Error Description"
                End If
            End If
            'move file to BadXML folder
            File.Copy(aFiles(I).FullName, rootDir & "\BadXLSX\" & aFiles(I).Name, True)
            'File.Delete(aFiles(I).FullName)
            m_logger.WriteInformationLog(rtn & " :: " & aFiles(I).FullName & " moved to " & rootDir & "\BadXLSX\" & aFiles(I).Name)

            bolError = True
        End Try

        aFiles = Nothing

        'Dim aFiles1 As FileInfo() = dirInfo.GetFiles(strFiles)
        'Dim i1 As Integer = 0
        'Try

        '    If aFiles1.Length > 0 Then
        '        For i1 = 0 To aFiles1.Length - 1
        '            Try

        '                File.Delete(aFiles1(i1).FullName)
        '            Catch exFileDel As Exception
        '                strXMLError = "Error deleting file: " & aFiles1(i1).Name
        '                If Trim(m_arrXMLErrFiles) = "" Then
        '                    m_arrXMLErrFiles = aFiles1(i1).Name
        '                Else
        '                    m_arrXMLErrFiles &= "," & aFiles1(i1).Name
        '                End If
        '                If Trim(strXMLError) <> "" Then
        '                    If Len(strXMLError) > 250 Then
        '                        strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
        '                    End If
        '                    If Trim(m_arrErrorsList) = "" Then
        '                        m_arrErrorsList = strXMLError
        '                    Else
        '                        m_arrErrorsList &= "," & strXMLError
        '                    End If
        '                Else
        '                    If Trim(m_arrErrorsList) = "" Then
        '                        m_arrErrorsList = "Check Log for the Error Description"
        '                    Else
        '                        m_arrErrorsList &= "," & "Check Log for the Error Description"
        '                    End If
        '                End If

        '                bolError = True

        '            End Try

        '        Next
        '    End If

        'Catch exDel As Exception
        '    strXMLError = "Error deleting files: " & exDel.ToString
        '    If Trim(m_arrXMLErrFiles) = "" Then
        '        m_arrXMLErrFiles = aFiles1(i1).Name
        '    Else
        '        m_arrXMLErrFiles &= "," & aFiles1(i1).Name
        '    End If
        '    If Trim(strXMLError) <> "" Then
        '        If Len(strXMLError) > 250 Then
        '            strXMLError = Microsoft.VisualBasic.Left(strXMLError, 250)
        '        End If
        '        If Trim(m_arrErrorsList) = "" Then
        '            m_arrErrorsList = strXMLError
        '        Else
        '            m_arrErrorsList &= "," & strXMLError
        '        End If
        '    Else
        '        If Trim(m_arrErrorsList) = "" Then
        '            m_arrErrorsList = "Check Log for the Error Description"
        '        Else
        '            m_arrErrorsList &= "," & "Check Log for the Error Description"
        '        End If
        '    End If

        '    bolError = True
        'End Try

        ''If bolError Then
        ''    SendErrorEmail(True)
        ''End If

        Return bolError

    End Function

    Private Function SendEmailWithAttachm(ByVal strEmail As String, ByVal strSupplierId As String, _
            ByVal strFullPath As String, ByVal strFileName As String, _
            Optional ByRef strErrMsg As String = "") As Boolean

        Dim rtn As String = "SendFileAsAttachm.SendEmailWithAttachm"
        Dim strEmailFrom As String = ""
        Dim strEmailTo As String = ""
        Dim strEmailCC As String = ""
        Dim strEmailBcc As String = ""
        Dim strEmailBody As String = ""
        Dim strEmailSubject As String = ""
        Dim bIsValidEmail As Boolean = True

        'The email address of the sender
        strEmailFrom = "TechSupport@sdi.com"
        If (CStr(My.Settings(propertyName:="onErrorEmail_From")) <> "") Then
            strEmailFrom = CStr(My.Settings(propertyName:="onErrorEmail_From")).Trim
        End If

        'The email address of the recipient. 
        strEmailTo = "vitaly.rovensky@sdi.com"
        If strEmail.Contains(";") Then
            'several emails
            Dim arrEmails() As String = Split(strEmail, ";")
            Dim i32 As Integer = 0
            If arrEmails.Length > 0 Then
                For i32 = 0 To arrEmails.Length - 1
                    If Trim(arrEmails(i32)) <> "" Then
                        If IsValidEmail(arrEmails(i32)) Then
                        Else
                            bIsValidEmail = False
                        End If
                    End If

                Next
            End If
            If bIsValidEmail Then
                strEmailTo = strEmail
            Else
                strEmailTo = "jan.hines@sdi.com"
                If (CStr(My.Settings(propertyName:="onWrongEmail_To")) <> "") Then
                    strEmailTo = CStr(My.Settings(propertyName:="onWrongEmail_To")).Trim
                End If
            End If
        Else
            If IsValidEmail(strEmail) Then
                strEmailTo = strEmail
            Else
                strEmailTo = "jan.hines@sdi.com"
                If (CStr(My.Settings(propertyName:="onWrongEmail_To")) <> "") Then
                    strEmailTo = CStr(My.Settings(propertyName:="onWrongEmail_To")).Trim
                End If
                bIsValidEmail = False
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
        'strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center>&nbsp;&nbsp;"

        If bIsValidEmail Then
            strEmailBody &= "<table><tr><td>Attached is your weekly OTD Report from SDI, Inc.   If you have any questions, please contact Jan Hines at 267-525-5936 "
            strEmailSubject = " ON TIME DELIVERY SDI/ NYC DOE CUSTODIAL/TOOLBOX Week of " & Now.Date.ToString

        Else
            strEmailBody &= "<table><tr><td>Attached Weekly OTD Report from SDI, Inc. was not sent because email address on file is not valid: " & strEmail & " for Supplier ID: " & strSupplierId
            strEmailSubject = "Error - wrong email - ON TIME DELIVERY SDI/ NYC DOE CUSTODIAL/TOOLBOX Week of " & Now.Date.ToString

        End If

        'VR 12/18/2014 Adding file names and error descriptions in message body
        Dim sInfoErr As String = ""
        Dim strErrListForSFTP As String = ""
        'Try

        '    sInfoErr &= " XML file name(s) are below.</td></tr>"
        '    If Not m_arrXMLErrFiles Is Nothing Then
        '        If Trim(m_arrXMLErrFiles) <> "" Then
        '            Dim arrErrFiles1() As String = Split(m_arrXMLErrFiles, ",")
        '            Dim arrErrDescr2() As String = Split(m_arrErrorsList, ",")
        '            If arrErrFiles1.Length > 0 Then
        '                For i1 As Integer = 0 To arrErrFiles1.Length - 1
        '                    sInfoErr &= "<tr><td>" & arrErrFiles1(i1) & "</td><td>&nbsp;&nbsp" & arrErrDescr2(i1) & "</td></tr>"
        '                    strErrListForSFTP &= arrErrFiles1(i1) & " - " & arrErrDescr2(i1) & vbCrLf
        '                Next
        '            End If
        '        End If
        '    End If
        '    strEmailBody &= sInfoErr
        'Catch ex As Exception

        '    strEmailBody &= " review log.</td></tr>"
        'End Try

        strEmailBody &= "</table>"

        strEmailBody &= "&nbsp;<br>Sincerely,<br>&nbsp;<br>SDI Customer Care<br>&nbsp;<br></p></div><BR>"
        strEmailBody &= "<br />"


        'Dim sApp As String = "" & _
        '                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).Name & _
        '                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
        '                    ""
        'Try
        '    strEmailBody &= "" & _
        '                  "<p style=""text-align:right;font-size:10px"">" & _
        '                  sApp & _
        '                  "</p>" & _
        '                  ""
        'Catch ex As Exception
        'End Try

        'Dim strEmailBodyEnd As String = "</body></html>"

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
            If (CStr(My.Settings(propertyName:="onErrorEmail_To")) <> "") Then
                strEmailTo = CStr(My.Settings(propertyName:="onErrorEmail_To")).Trim
            End If
            strEmailSubject = " (test run) " & strEmailSubject
        End If

        'code for attachment
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
        Dim Attachments As New List(Of String)

        Attachments.Add(strFileName)
        MailAttachmentName = Attachments.ToArray()

        Dim fInfo As New FileInfo(strFullPath)
        Dim numBytes As Long = fInfo.Length

        Dim fStream As New FileStream(strFullPath, FileMode.Open, FileAccess.Read)
        Dim br As New BinaryReader(fStream)
        Dim data As Byte() = br.ReadBytes(CInt(numBytes))
        MailAttachmentbytes.Add(data)

        Dim bSend As Boolean = False
        Dim strReturn As String = ""
        Try

            strReturn = SendLogger(strEmailSubject, strEmailBody, "SENDFILEASATTACHM", "Mail", strEmailTo, strEmailCC, strEmailBcc, MailAttachmentName, MailAttachmentbytes.ToArray())
            bSend = True
        Catch ex As Exception
            bSend = False
            strErrMsg = "Error sending email: " & ex.ToString
        End Try

        Attachments = Nothing
        MailAttachmentName = Nothing
        MailAttachmentbytes = Nothing
        fInfo = Nothing
        fStream = Nothing
        br = Nothing
        data = Nothing

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: " & strErrMsg)
        End If

        Return bSend

    End Function

    Private Function IsValidEmail(ByVal SingleEmailAdd As String) As Boolean
        Try
            Dim bValid As Boolean = False

            If SingleEmailAdd.Trim.Length > 0 Then
                Dim cParts As String() = SingleEmailAdd.Split(CType("@", Char))

                If cParts.Length = 2 Then
                    If cParts(1).Length > 0 Then
                        If cParts(1).Contains(".") Then
                            If cParts(0).Length > 0 Then
                                bValid = True
                            Else
                                bValid = False
                            End If
                        Else
                            bValid = False
                        End If
                    Else
                        bValid = False
                    End If
                Else
                    bValid = False
                End If
            Else
                bValid = False
            End If

            Return bValid

        Catch ex As Exception
            Throw New Exception("IsValidSingleAdd::" & ex.ToString)
        End Try
    End Function

    Private Sub SendErrorEmail(Optional ByVal bIsSendOut As Boolean = False)

        Dim rtn As String = "SendFileAsAttachm.SendErrorEmail"
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
        strAddSDILogo = "<html><body><img src='https://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='182px' vspace='0' hspace='0' />"
        strEmailBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI, Inc</span></center><center><span >Send File as email Attachment Error</span></center>&nbsp;&nbsp;"

        strEmailBody &= "<table><tr><td>Send File as email Attachment has completed with "
      
        strEmailBody &= "errors."
        strEmailSubject = "Send File as email Attachment Error"
        
        'VR 12/18/2014 Adding file names and error descriptions in message body
        Dim sInfoErr As String = ""
        'Dim strErrListForSFTP As String = ""
        Try

            sInfoErr &= " File name(s) are below.</td></tr>"
            If Not m_arrXMLErrFiles Is Nothing Then
                If Trim(m_arrXMLErrFiles) <> "" Then
                    Dim arrErrFiles1() As String = Split(m_arrXMLErrFiles, ",")
                    Dim arrErrDescr2() As String = Split(m_arrErrorsList, ",")
                    If arrErrFiles1.Length > 0 Then
                        For i1 As Integer = 0 To arrErrFiles1.Length - 1
                            sInfoErr &= "<tr><td>" & arrErrFiles1(i1) & "</td><td>&nbsp;&nbsp" & arrErrDescr2(i1) & "</td></tr>"
                            'strErrListForSFTP &= arrErrFiles1(i1) & " - " & arrErrDescr2(i1) & vbCrLf
                        Next
                    End If
                End If
            End If
            strEmailBody &= sInfoErr
        Catch ex As Exception

            strEmailBody &= " review log.</td></tr>"
        End Try

        'strEmailBody &= " Please review log.</td></tr>"
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

            strReturn = SendLogger(strEmailSubject, strEmailBody, "SENDFILEASATTACHM", "Mail", strEmailTo, strEmailCC, strEmailBcc, MailAttachmentName, MailAttachmentbytes.ToArray())
            bSend = True
            m_arrXMLErrFiles = ""
            m_arrErrorsList = ""
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

End Module
