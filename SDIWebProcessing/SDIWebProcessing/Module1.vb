Imports System
Imports System.IO
Imports System.Data.OleDb
Imports System.Xml
Imports System.Text
Imports System.Web
Imports System.Net
Imports System.Net.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter

Module Module1

    Private m_logger As appLogger = Nothing

    Dim rootDir As String = "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles"
    Dim logpath As String = "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\LOGS\SDIWebXMLIn" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\configSetting.xml"
    Dim objStreamWriter As StreamWriter
    Dim connectOR As New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=DEVL")
    Dim connectSQL As New SqlClient.SqlConnection("server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'")

    Private m_arrErrXMLs As New ArrayList

    Sub Main()

        Dim rtn As String = "Module1.Main"

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Info

        Console.WriteLine("Start SDI Web XML Processing")
        Console.WriteLine("")

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Started SDI Web XML Processing " & Now())

        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
        End If
        If Dir(rootDir & "\XMLINProcessed", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLINProcessed")
        End If
        If Dir(rootDir & "\BadXML", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\BadXML")
        End If

        m_xmlConfig = New XmlDocument
        m_xmlConfig.Load(filename:=m_configFile)

        Dim cnString As String = ""
        Try
            ' retrieve the source DB connection string to use
            If Not (m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText Is Nothing) Then
                cnString = m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText.Trim
            End If
        Catch ex As Exception
            cnString = ""
        End Try

        If Trim(cnString) <> "" Then
            connectOR.ConnectionString = cnString
        End If

        ' COPY
        Dim dirInfo As DirectoryInfo
        'debug
        dirInfo = New DirectoryInfo("C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\")

        Dim strFiles As String
        Dim arrOrders As ArrayList
        arrOrders = New ArrayList

        strFiles = "*.XML"
        Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        Dim I As Integer

        Try
            If aFiles.Length > 0 Then
                For I = 0 To aFiles.Length - 1
                    File.Copy(aFiles(I).FullName, "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\XMLIN\" & aFiles(I).Name, True)
                    File.Delete(aFiles(I).FullName)
                    objStreamWriter.WriteLine(rtn & " :: " & aFiles(I).FullName & " moved to " & "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\XMLIN\" & aFiles(I).Name)
                Next
            Else
                objStreamWriter.WriteLine(rtn & " :: No files to copy from " & dirInfo.FullName)
                'Exit Sub  ' commented out for debug only
            End If
        Catch ex As Exception
            objStreamWriter.WriteLine(rtn & " :: Error moving file(s) from " & dirInfo.FullName & " to C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\XMLIN\ " & "...")
            objStreamWriter.WriteLine(rtn & " :: " & ex.ToString)
            objStreamWriter.WriteLine(rtn & " :: End of SDI Web XML Processing." & Now())
            objStreamWriter.Flush()
            objStreamWriter.Close()
            'SendEmail()  ' commented out for debug only
            Exit Sub
        End Try

        'CHECK
        dirInfo = New DirectoryInfo("C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\XMLIN\")
        Dim sr As System.IO.StreamReader
        Dim XMLContent As String = ""
        Dim strXMLError As String = ""
        Dim bolError As Boolean

        Dim xmlRequest As New XmlDocument

        strFiles = "*.XML"
        aFiles = dirInfo.GetFiles(strFiles)
        Dim root As XmlElement

        Dim sXMLFilename As String = ""

        Try
            For I = 0 To aFiles.Length - 1
                sXMLFilename = aFiles(I).Name

                objStreamWriter.WriteLine(" Start File " & aFiles(I).Name)
                sr = New System.IO.StreamReader(aFiles(I).FullName)
                XMLContent = sr.ReadToEnd()
                XMLContent = Replace(XMLContent, "&", "&amp;")
                sr.Close()

                Try
                    xmlRequest.LoadXml(XMLContent)
                Catch ex As Exception
                    Console.WriteLine("")
                    Console.WriteLine("***error - " & ex.ToString)
                    Console.WriteLine("")
                    objStreamWriter.WriteLine("     Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
                    strXMLError = ex.ToString
                    bolError = True
                End Try
                If Trim(strXMLError) = "" Then
                    root = xmlRequest.DocumentElement
                    'firstchild.name = PS_ISA_ORD_INTFC_H

                    ' use this but change node names
                    ' for shipping - <Request><ShipNoticeRequest>  ("Request")("ShipNoticeRequest")
                    ' for confirmation -  <Request><CohfirmationRequest>  ("Request")("ConfirmationRequest")

                    If Not root("Request")("ConfirmationRequest") Is Nothing Then
                        strXMLError = ""
                        'process confirmation request
                    Else
                        If Not root("Request")("ShipNoticeRequest") Is Nothing Then
                            strXMLError = ""
                            'process shipping request
                        Else
                            If Not root("Request")("OrderRequest") Is Nothing Then
                                strXMLError = ""
                                'process shipping request
                            Else
                                strXMLError = "Wrong XML structure"
                                bolError = True
                                ' wrong XML
                            End If
                        End If
                    End If
                    'If root.FirstChild Is Nothing Then
                    '    strXMLError = "empty XML file - root.FirstChild Is Nothing "
                    'ElseIf root.LastChild.Name.ToUpper = "PS_ISA_ORD_INTFC_L" Or _
                    '    root.LastChild.Name.ToUpper = "EMP_ROW" Then
                    '    strXMLError = ""
                    'Else
                    '    strXMLError = "Missing last root.LastChild Element"
                    'End If

                    'If Trim(strXMLError) = "" Then
                    '    If root.LastChild.Name.ToUpper = "PS_ISA_ORD_INTFC_L" Then
                    ' 'need to create new sub to do similar
                    '        ''strXMLError = UpdIntfcTable(aFiles(I).FullName)
                    '    ElseIf root.LastChild.Name.ToUpper = "EMP_ROW" Then
                    ' 'again: need to create new sub to do similar
                    '        ''strXMLError = UpdEmpTable(aFiles(I).FullName)
                    '    End If
                    'End If

                    If Trim(strXMLError) = "" Or _
                        Trim(strXMLError) = "Invalid Status" Then
                        Try
                            File.Move(aFiles(I).FullName, "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\XMLINProcessed\" & aFiles(I).Name)
                            If Trim(strXMLError) = "Invalid Status" Then
                                objStreamWriter.WriteLine(" not status I or C in File " & aFiles(I).Name)
                                'm_logger.WriteInformationLog(rtn & " :: not status I or C in File " & aFiles(I).Name)
                            End If
                            objStreamWriter.WriteLine(" End - File " & aFiles(I).Name & " has been moved")
                            'm_logger.WriteInformationLog(rtn & " :: End - File " & aFiles(I).Name & " has been moved")
                        Catch ex As Exception
                            objStreamWriter.WriteLine("     Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
                            'm_logger.WriteErrorLog(rtn & " :: Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
                        End Try
                        File.Delete(aFiles(I).FullName)
                    Else
                        objStreamWriter.WriteLine("**")
                        objStreamWriter.WriteLine("     Error " & strXMLError & " in file " & aFiles(I).Name)
                        objStreamWriter.WriteLine("**")
                        'm_logger.WriteErrorLog(rtn & " :: ** ")
                        'm_logger.WriteErrorLog(rtn & " :: Error " & strXMLError & " in file " & aFiles(I).Name)
                        'm_logger.WriteErrorLog(rtn & " :: ** ")
                        bolError = True
                        Try
                            File.Move(aFiles(I).FullName, "C:\inetpub\wwwroot\SDIWebIn\SDIWebProcessorsXMLFiles\BadXML\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                        Catch ex As Exception
                            objStreamWriter.WriteLine("**")
                            objStreamWriter.WriteLine("     Error " & ex.Message & " in file " & aFiles(I).Name)
                            objStreamWriter.WriteLine("**")
                            'm_logger.WriteErrorLog(rtn & " :: ** ")
                            'm_logger.WriteErrorLog(rtn & " :: Error " & ex.Message & " in file " & aFiles(I).Name)
                            'm_logger.WriteErrorLog(rtn & " :: ** ")
                        End Try
                    End If

                End If

                ' if there's an error, capture the filename of the XML
                If (Trim(strXMLError) <> "") Or bolError Then
                    m_arrErrXMLs.Add(sXMLFilename)
                End If

                strXMLError = ""
            Next
        Catch ex As Exception
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error " & ex.Message & " in file " & aFiles(I).Name)
            objStreamWriter.WriteLine("**")
            m_arrErrXMLs.Add(sXMLFilename)
        End Try

        If Not bolError Then

        Else
            ' there's an error - process it
        End If
        objStreamWriter.WriteLine(rtn & " :: End of SDI Web XML Processing." & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub

End Module
