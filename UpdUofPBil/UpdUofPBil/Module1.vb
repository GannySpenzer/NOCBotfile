Imports System.IO
Imports System.Xml
Imports System.Text
Imports System.Net
'Imports FTP
Imports System.Data.SqlClient


Module Module1

    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\updUofPBil"
    Dim logpath As String = "C:\updUofPBil\LOGS\UpdUofPBil" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim strSrv As String = CStr(My.Settings("SRV")) 'System.Configuration.ConfigurationSettings.AppSettings("SRV")
    Dim strUID As String = CStr(My.Settings("UID")) 'System.Configuration.ConfigurationSettings.AppSettings("UID")
    Dim strPWD As String = CStr(My.Settings("PWD")) 'System.Configuration.ConfigurationSettings.AppSettings("PWD")
    Dim strICat As String = CStr(My.Settings("ICat")) 'System.Configuration.ConfigurationSettings.AppSettings("ICat")
    Dim strOutFile As String = CStr(My.Settings("OutFilePath")) 'System.Configuration.ConfigurationSettings.AppSettings("OutFilePath")
    Dim connectSQL As New SqlClient.SqlConnection("server=" & strSRV & ";uid=" & strUID & ";pwd=" & strPWD & ";initial catalog='" & strICat & "'")
    Dim bolWarning As Boolean = False
    Dim strRemoteHost As String = "ftp.sdi.com"
    Dim strRemoteUser As String = "ftp.supplypro"
    Dim strRemotePassword As String = "Tv06wsi0"

    Sub Main()

        Console.WriteLine("Start updUofPBil XML in")
        Console.WriteLine("")

        Dim sAddr As String
        Dim ipEntry As IPHostEntry = Dns.GetHostByName(Environment.MachineName)
        Dim IpAddr As IPAddress() = ipEntry.AddressList
        sAddr = IpAddr(0).ToString

        If sAddr.Length > 10 Then
            ' get the first 11 char to identify if we're running local
            If sAddr.Substring(0, 11) = "192.168.240" Then
                strRemoteHost = "lenka"
            End If
        End If

        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
            Console.WriteLine("MkDir " & rootDir)
            Console.WriteLine("")
        End If
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
            Console.WriteLine("MkDir " & rootDir & "\LOGS")
            Console.WriteLine("")
        End If
        If Dir(rootDir & "\XMLIN", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLIN")
            Console.WriteLine("MkDir " & rootDir & "\XMLIN")
            Console.WriteLine("")
        End If
        If Dir(rootDir & "\XMLINProcessed", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLINProcessed")
            Console.WriteLine("MkDir " & rootDir & "\XMLINProcessed")
            Console.WriteLine("")
        End If
        If Dir(rootDir & "\TXTOUT", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\TXTOUT")
            Console.WriteLine("MkDir " & rootDir & "\TXTOUT")
            Console.WriteLine("")
        End If
        If Dir(rootDir & "\TXTOUTProcessed", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\TXTOUTProcessed")
            Console.WriteLine("MkDir " & rootDir & "\TXTOUTProcessed")
            Console.WriteLine("")
        End If
        If Not Right(strOutFile, 1) = "\" Then
            strOutFile = strOutFile & "\"
        End If
        Try
            If Dir(Left(strOutFile, strOutFile.Length - 1), FileAttribute.Directory) = "" Then
                MkDir(Left(strOutFile, strOutFile.Length - 1))
                Console.WriteLine("MkDir " & strOutFile)
                Console.WriteLine("")
            End If
        Catch ex As Exception

        End Try


        Console.WriteLine("Open log " & logpath)
        Console.WriteLine("")
        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Update of updUofPBil IN XML " & Now())

        getFTPfiles()
        processfiles()
        deleteOldFiles()

        objStreamWriter.WriteLine("End of updUofPBil " & Now())
        objStreamWriter.Flush()
        objStreamWriter.Close()

        sendFTPLog(logpath)

    End Sub

    Private Function checkCPW(ByVal strProposal As String) As String
        Dim strSQLstring As String
        Dim sRet As String = ""

        strSQLstring = "SELECT order_type " & vbCrLf & _
                       "FROM ae_p_pro_e " & vbCrLf & _
                       "WHERE (order_type = 'P') " & vbCrLf & _
                       "  AND (proposal = '" & strProposal & "') "

        Try
            'checkCPW = SQLScalar(strSQLstring)
            sRet = SQLScalar(strSQLstring)
        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - selecting from ae_p_pro_e failed for proposal " & strProposal & ex.ToString)
            connectSQL.Close()
            'Exit Function
        End Try

        Return (sRet)
    End Function

    Private Sub deleteOldFiles()
        Console.WriteLine("Delete old files")
        Console.WriteLine("")
        Dim X As Integer
        Dim arrDirs As ArrayList
        arrDirs = New ArrayList
        arrDirs.Add("C:\updUofPBil\LOGS")
        arrDirs.Add("C:\updUofPBil\XMLIN")
        arrDirs.Add("C:\updUofPBil\XMLINProcessed")
        arrDirs.Add("C:\updUofPBil\TXTOUT")
        arrDirs.Add("C:\updUofPBil\TXTOUTProcessed")
        For X = 0 To arrDirs.Count - 1

            Dim dirInfo As DirectoryInfo = New DirectoryInfo(arrDirs(X))
            Dim strFiles As String
            Dim I As Integer
            Dim days As Integer
            strFiles = "*.*"
            objStreamWriter.WriteLine("     Directory - " & arrDirs(X) & "\" & strFiles)

            Try
                Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)

                For I = 0 To aFiles.Length - 1
                    Dim dte As DateTime
                    Try
                        dte = Convert.ToDateTime(aFiles(I).CreationTime)
                        days = DateDiff(DateInterval.Day, dte, DateTime.Now)
                        If days > 30 Then
                            objStreamWriter.WriteLine("         File " & aFiles(I).Name & " dated " & aFiles(I).CreationTime & " has been deleted")
                            File.Delete(aFiles(I).FullName)
                        End If
                    Catch ex As Exception

                    End Try
                Next
            Catch ex As Exception
                objStreamWriter.WriteLine("  Error - Invalid directory name " & dirInfo.FullName)
            End Try

        Next

        objStreamWriter.WriteLine("Finish Delete Old Files " & Now())
    End Sub

    Private Sub getFTPfiles()

        Console.WriteLine("Get file from FTP site")
        Console.WriteLine("")
        Dim ff As clsFTP
        ' Create an instance of the FTP Class.
        ff = New clsFTP

        ' Setup the appropriate properties.

        ff.RemoteHost = strRemoteHost
        ff.RemoteUser = strRemoteUser
        ff.RemotePassword = strRemotePassword

        If (ff.Login()) Then
            Try
                ff.ChangeDirectory("XMLOut")
                ff.SetBinaryMode(True)
                ' get file list
                Dim C As Integer
                Dim arrFTPList As Array
                arrFTPList = ff.GetFileList("updUofPBil*.xml")
                If arrFTPList(0) = "" Then
                    Console.WriteLine(" no input file exist on the FTP site")
                    Console.WriteLine("")
                End If

                For C = 0 To arrFTPList.Length - 1
                    'copy files to XMLIN directory
                    If Not Trim(arrFTPList(C)) = "" Then
                        If Trim(arrFTPList(C)).Length - 1 > Trim(arrFTPList(C)).LastIndexOf("l") Then
                            arrFTPList(C) = Trim(arrFTPList(C)).Substring(0, Trim(arrFTPList(C)).LastIndexOf("l") + 1)
                        End If
                        ff.DownloadFile(Trim(arrFTPList(C)), rootDir & "\XMLIN\" & Trim(arrFTPList(C)))
                        Console.WriteLine("     File has been downloaded from the FTP site.")
                        Console.WriteLine("")
                        objStreamWriter.WriteLine("     " & arrFTPList(C) & " has been downloaded from the FTP site.")
                        If (ff.DeleteFile(arrFTPList(C))) Then
                            objStreamWriter.WriteLine("     " & arrFTPList(C) & " has been deleted from the FTP site.")
                        Else
                            objStreamWriter.WriteLine(" WARNING  " & arrFTPList(C) & " could not be deleted from the FTP site.")
                        End If

                    End If
                Next

            Catch ex As System.Exception
                Console.WriteLine("  ERROR - downloading from FTP site " & ex.ToString)
                Console.WriteLine("")
                objStreamWriter.WriteLine(" ERROR - downloading from FTP site " & ex.ToString)
            Finally
                Try
                    ff.CloseConnection()
                Catch ex As Exception

                End Try
            End Try
        Else
            Console.WriteLine(" ERROR - no connection to FTP site")
            Console.WriteLine("")
            objStreamWriter.WriteLine(" ERROR - no connection to FTP site")
        End If

    End Sub

    Private Sub processfiles()

        Console.WriteLine("process billing input file")
        Console.WriteLine("")
        Dim dirInfo As DirectoryInfo = New DirectoryInfo(rootDir & "\XMLIN\")
        Dim dirInfoOut As DirectoryInfo = New DirectoryInfo(rootDir & "\TXTOUT\")
        Dim strFiles As String
        Dim strFilesOut As String
        Dim strFilesNoData As String
        Dim bolInArray As Boolean
        Dim sr As System.IO.StreamReader
        Dim XMLContent As String
        Dim strXMLError As String
        Dim xmlRequest As New XmlDocument
        Dim I As Integer

        strFiles = "UpdUofPBil*.XML"
        strFilesOut = "UpdUofPBil*.TXT"
        strFilesNoData = "UpdUofPBil*.NoData"
        Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)
        If aFiles.Length = 0 Then
            Console.WriteLine("         No files to process")
            Console.WriteLine("")
            objStreamWriter.WriteLine("          No files to process")
            Dim strXMLPath As String = rootDir & "\TXTOUT\updUofPBil" & Now.Month.ToString & Now.Day.ToString & Now.Year.ToString & ".NoData"
            Dim sw As StreamWriter
            sw = File.CreateText(strXMLPath)
            sw.WriteLine("no data - " & Now)
            sw.Close()
            'exit Sub
        End If
        'Dim fileDate As DateTime = aFiles(0).CreationTime
        'Dim fullFileName As String = aFiles(0).FullName
        'For I = 0 To aFiles.Length - 1
        '    If aFiles(I).CreationTime > fileDate Then
        '        fileDate = aFiles(I).CreationTime
        '        fullFileName = aFiles(I).FullName
        '    End If
        'Next

        Dim root As XmlElement

        Dim X As Integer
        For I = 0 To aFiles.Length - 1
            Console.WriteLine(" Start File " & aFiles(I).Name)
            Console.WriteLine("")
            objStreamWriter.WriteLine(" Start File " & aFiles(I).Name)
            sr = New System.IO.StreamReader(aFiles(I).FullName)
            XMLContent = sr.ReadToEnd()
            XMLContent = Replace(XMLContent, "&", "&amp;")
            XMLContent = Replace(XMLContent, "'", "&#39;")
            sr.Close()
            Try
                xmlRequest.LoadXml(XMLContent)
            Catch ex As Exception
                Console.WriteLine(" ERROR " & ex.message.ToString & " in file " & aFiles(I).Name)
                Console.WriteLine("")
                objStreamWriter.WriteLine(" ERROR " & ex.message.ToString & " in file " & aFiles(I).Name)
                strXMLError = ex.ToString
                Exit Sub
            End Try

            root = xmlRequest.DocumentElement

            If root.FirstChild Is Nothing Then
                objStreamWriter.WriteLine(" ERROR - empty file - " & aFiles(I).Name)
                Exit Sub
            ElseIf Not root.LastChild.Name.ToUpper = "ROW" Then
                objStreamWriter.WriteLine(" ERROR - invalid last child in file - " & aFiles(I).Name)
                Exit Sub
            ElseIf Not xmlRequest.ChildNodes(2).Name = "SDI" Then
                objStreamWriter.WriteLine(" ERROR - invalid xmlRequest.ChildNodes(2).Name in file - " & aFiles(I).Name)
                Exit Sub
            End If

            Dim jobNode As XmlNode
            jobNode = xmlRequest.ChildNodes(2)
            Dim dsRows As New DataSet
            dsRows.ReadXml(New XmlNodeReader(jobNode))
            If dsRows.Tables(0).Rows.Count = 0 Then
                Console.WriteLine(" ERROR - no records loaded to dataset - " & aFiles(I).Name)
                Console.WriteLine("")
                objStreamWriter.WriteLine(" ERROR - no records loaded to dataset - " & aFiles(I).Name)
                Exit Sub
            Else
                objStreamWriter.WriteLine(" processing file - " & aFiles(I).Name)
            End If
            'process SQL updates
            updateInvDB(dsRows, I)
        Next
        For I = 0 To aFiles.Length - 1
            Try
                If File.Exists(rootDir & "\XMLINProcessed\" & aFiles(I).Name) Then
                    File.Delete(rootDir & "\XMLINProcessed\" & aFiles(I).Name)
                End If
                File.Move(aFiles(I).FullName, rootDir & "\XMLINProcessed\" & aFiles(I).Name)
                'File.Delete(aFiles(I).FullName)
                Console.WriteLine(" File " & aFiles(I).Name & " has been moved")
                Console.WriteLine("")
                objStreamWriter.WriteLine(" File " & aFiles(I).Name & " has been moved")
            Catch ex As Exception
                Console.WriteLine("     Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
                Console.WriteLine("")
                objStreamWriter.WriteLine("     Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
            End Try
        Next
        Dim aFilesOut As FileInfo() = dirInfoOut.GetFiles(strFilesOut)
        For I = 0 To aFilesOut.Length - 1
            Try
                If File.Exists(rootDir & "\TXTOUTProcessed\" & aFilesOut(I).Name) Then
                    File.Delete(rootDir & "\TXTOUTProcessed\" & aFilesOut(I).Name)
                End If
                File.Copy(aFilesOut(I).FullName, rootDir & "\TXTOUTProcessed\" & aFilesOut(I).Name)
                Console.WriteLine(" File " & aFilesOut(I).Name & " has been moved")
                Console.WriteLine("")
                objStreamWriter.WriteLine(" File " & aFilesOut(I).Name & " has been moved")
                If strOutFile <> rootDir & "\TXTOUT\" Then
                    File.Copy(aFilesOut(I).FullName, strOutFile & aFilesOut(I).Name)
                    File.Delete(aFilesOut(I).FullName)
                    Console.WriteLine(" File " & aFilesOut(I).Name & " has been moved")
                    Console.WriteLine("")
                    objStreamWriter.WriteLine(" File " & aFilesOut(I).Name & " has been moved")
                End If
            Catch ex As Exception
                Console.WriteLine("     Error " & ex.Message.ToString & " in file " & aFilesOut(I).Name)
                Console.WriteLine("")
                objStreamWriter.WriteLine("     Error " & ex.Message.ToString & " in file " & aFilesOut(I).Name)
            End Try
        Next
        Dim aFilesnodata As FileInfo() = dirInfoOut.GetFiles(strFilesNoData)
        For I = 0 To aFilesnodata.Length - 1
            Try
                If File.Exists(rootDir & "\TXTOUTProcessed\" & aFilesnodata(I).Name) Then
                    File.Delete(rootDir & "\TXTOUTProcessed\" & aFilesnodata(I).Name)
                End If
                File.Copy(aFilesnodata(I).FullName, rootDir & "\TXTOUTProcessed\" & aFilesnodata(I).Name)
                Console.WriteLine(" File " & aFilesnodata(I).Name & " has been moved")
                Console.WriteLine("")
                objStreamWriter.WriteLine(" File " & aFilesnodata(I).Name & " has been moved")
                If strOutFile <> rootDir & "\TXTOUT\" Then
                    File.Copy(aFilesnodata(I).FullName, strOutFile & aFilesnodata(I).Name)
                    File.Delete(aFilesnodata(I).FullName)
                    Console.WriteLine(" File " & aFilesnodata(I).Name & " has been moved")
                    Console.WriteLine("")
                    objStreamWriter.WriteLine(" File " & aFilesnodata(I).Name & " has been moved")
                End If
            Catch ex As Exception
                Console.WriteLine("     Error " & ex.Message.ToString & " in file " & aFilesnodata(I).Name)
                Console.WriteLine("")
                objStreamWriter.WriteLine("     Error " & ex.Message.ToString & " in file " & aFilesnodata(I).Name)
            End Try
        Next

    End Sub

    Private Sub sendFTPLog(ByVal strLogFilePath As String)

        Console.WriteLine("send Log to SDI FTP site")
        Console.WriteLine("")
        Dim ff As clsFTP
        ' Create an instance of the FTP Class.
        ff = New clsFTP

        ' Setup the appropriate properties.

        ff.RemoteHost = strRemoteHost
        ff.RemoteUser = strRemoteUser
        ff.RemotePassword = strRemotePassword

        If (ff.Login()) Then
            Try
                ff.ChangeDirectory("UofPLogs")
                ff.SetBinaryMode(True)
                'copy log to UofPLogs directory
                ff.UploadFile(strLogFilePath)

            Catch ex As System.Exception

            Finally
                Try
                    ff.CloseConnection()
                Catch ex As Exception

                End Try

            End Try
        End If

    End Sub

    Private Sub updateInvDB(ByVal dsRows As DataSet, ByVal intI As Integer)

        'Dim colAppSettings As System.Collections.Specialized.NameValueCollection
        'colAppSettings = System.Configuration.ConfigurationSettings.AppSettings()
        Dim strServer As String = CStr(My.Settings("SRV")) 'colAppSettings("SRV")
        Dim strUid As String = CStr(My.Settings("UID")) 'colAppSettings("UID")
        Dim strPwd As String = CStr(My.Settings("PWD")) 'colAppSettings("PWD")
        Dim strCatalog As String = CStr(My.Settings("ICat")) 'colAppSettings("ICat")
        Dim decProjFeeOld As Decimal = CStr(My.Settings("SmallProjectFeeOld")) 'colAppSettings("SmallProjectFeeOld")
        Dim decProjFeeNew As Decimal = CStr(My.Settings("SmallProjectFeeNew")) 'colAppSettings("SmallProjectFeeNew")
        Dim dteFeeSwitchover As Date = CStr(My.Settings("SmallProjectFeeNewDate")) 'colAppSettings("SmallProjectFeeNewDate")

        Dim bolError As Boolean

        Dim dteJulian As Integer
        Dim dteStart As Date = "01/01/1900"
        dteJulian = DateDiff(DateInterval.Day, dteStart, Now())

        Dim Y As Integer
        Dim dteInvDate As Date
        For Y = 0 To dsRows.Tables(0).Rows.Count - 1
            If Not IsDBNull(dsRows.Tables(0).Rows(Y).Item("SHIP_DT")) Then
                If IsDate(dsRows.Tables(0).Rows(Y).Item("SHIP_DT")) Then
                    If dsRows.Tables(0).Rows(Y).Item("SHIP_DT") > dteInvDate Then
                        dteInvDate = dsRows.Tables(0).Rows(Y).Item("SHIP_DT")
                    End If
                End If
                'If dsRows.Tables(0).Rows(Y).Item("SHIP_DT") > dteInvDate Then
                '    dteInvDate = dsRows.Tables(0).Rows(Y).Item("SHIP_DT")
                'End If
            End If
        Next

        'Dim strXMLPath As String = rootDir & "\TXTOUT\updUofPBil" & Convert.ToString(dteJulian) & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & intI & ".txt"
        Dim strXMLPath As String = rootDir & "\TXTOUT\updUofPBil" & dteInvDate.Month.ToString & dteInvDate.Day.ToString & dteInvDate.Year.ToString & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & intI & ".txt"
        Dim sw As StreamWriter
        Dim strBuilder As New System.Text.StringBuilder
        Dim linea As String
        Dim dt As DataTable
        Dim dtCol As DataColumn
        Dim dtRow As DataRow

        'create table
        dt = New DataTable("BilData")

        'create columns
        dtCol = New DataColumn("trans_date", GetType(Date))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("proposal", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("sort_code", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("craft_code", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("shop_person", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("amount", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("req_no", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("po_num", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("itm_desc", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("inv_no", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("inv_date", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("payment_date", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("cash_no", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("subledger_type", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("batch_no", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("part", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("itm_qty", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("contractor", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("address_code", GetType(String))
        dt.Columns.Add(dtCol)
        dtCol = New DataColumn("uom", GetType(String))
        dt.Columns.Add(dtCol)


        sw = File.CreateText(strXMLPath)
        Dim I As Integer
        Dim X As Integer
        Dim strCPW As String
        Dim decAmount As Decimal
        Dim connectSQL As New SqlConnection("server=" & strServer & ";uid=" & strUid & ";pwd=" & strPwd & ";Initial Catalog=" & strCatalog)

        Try
            connectSQL.Open()
        Catch ex As Exception
            objStreamWriter.WriteLine("     *error - could not open connection to SQL DB " & strUid)
            Console.WriteLine(" *error - could not open connection to SQL DB " & strUid)
            Console.WriteLine("")
            connectSQL.Close()
            Exit Sub
        End Try

        For I = 0 To dsRows.Tables(0).Rows.Count - 1
            'format dt table
            dtRow = dt.NewRow

            ' ship date
            'dtRow("trans_date") = String.Format("{0:d}", dsRows.Tables(0).Rows(I).Item("SHIP_DT"))
            Dim sShipDate As String = String.Format("{0:d}", CDate("1/1/1900"))
            Try
                sShipDate = String.Format("{0:d}", CDate(dsRows.Tables(0).Rows(I).Item("SHIP_DT")))
            Catch ex As Exception
            End Try
            dtRow("trans_date") = sShipDate

            ' w/o and/or charge code
            '   now I got to figure out which one contains valid UPenn's w/o because of process change!?
            Dim sWorkOrder As String = CStr(dsRows.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO")).Trim
            Dim sChargeCode As String = CStr(dsRows.Tables(0).Rows(I).Item("ISA_CUST_CHARGE_CD")).Trim
            Dim bIsFoundWO As Boolean = False
            Dim sUseThisAsWO As String = sWorkOrder

            ' start by looking at what is on the w/o field (strict pattern matching)
            If Not bIsFoundWO Then
                If (sWorkOrder.Length > 0) Then
                    If DoesThisLookLikeUPennWO_1(sWorkOrder) Then
                        sUseThisAsWO = sWorkOrder
                        bIsFoundWO = True
                    End If
                End If
            End If
            ' if w/o field is not filled OR does not look like UPenn's w/o (strict pattern matching) ...
            If Not bIsFoundWO Then
                If (sChargeCode.Length > 0) Then
                    If DoesThisLookLikeUPennWO_1(sChargeCode) Then
                        sUseThisAsWO = sChargeCode
                        bIsFoundWO = True
                    End If
                End If
            End If
            ' more relaxed rule on w/o field
            If Not bIsFoundWO Then
                If (sWorkOrder.Length > 0) Then
                    If DoesThisLookLikeUPennWO_2(sWorkOrder) Then
                        sUseThisAsWO = sWorkOrder
                        bIsFoundWO = True
                    End If
                End If
            End If
            ' more relaxed rule on c/c field
            If Not bIsFoundWO Then
                If (sChargeCode.Length > 0) Then
                    If DoesThisLookLikeUPennWO_2(sChargeCode) Then
                        sUseThisAsWO = sChargeCode
                        bIsFoundWO = True
                    End If
                End If
            End If
            ' if even c/c is either not filled or I still cannot figure out UPenn's w/o, just use w/o field if filled
            If Not bIsFoundWO Then
                If (sWorkOrder.Length > 0) Then
                    sUseThisAsWO = sWorkOrder
                    bIsFoundWO = True
                End If
            End If
            ' if I still cannot come up with the w/o value, let see if c/c field is filled and use that ...
            If Not bIsFoundWO Then
                If (sChargeCode.Length > 0) Then
                    sUseThisAsWO = sChargeCode
                    bIsFoundWO = True
                End If
            End If
            ' if after ALL of those test above and still cannot figure out w/o # to use ...
            If Not bIsFoundWO Then
                ' you're screwed :) ... this will use whatever (probably blank value) is on the ISA_WORK_ORDER_NO field ... good luck!!!
            End If

            'If Convert.ToString(dsRows.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO")).Length > 15 Then
            If sUseThisAsWO.Length > 15 Then
                'dtRow("proposal") = Convert.ToString(dsRows.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO")).Substring(0, 15)
                dtRow("proposal") = sUseThisAsWO.Substring(0, 15)
                dtRow("sort_code") = "001"
                'ElseIf Convert.ToString(dsRows.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO")).Length = 13 Then
            ElseIf sUseThisAsWO.Length = 13 Then
                'If Convert.ToString(dsRows.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO")).Substring(9, 1) = "-" And _
                '   IsNumeric(Convert.ToString(dsRows.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO")).Substring(10, 3)) Then
                If sUseThisAsWO.Substring(9, 1) = "-" And IsNumeric(sUseThisAsWO.Substring(10, 3)) Then
                    dtRow("proposal") = sUseThisAsWO.Substring(0, 9)
                    dtRow("sort_code") = sUseThisAsWO.Substring(10, 3)
                Else
                    dtRow("proposal") = sUseThisAsWO
                    dtRow("sort_code") = "001"
                End If
            Else
                dtRow("proposal") = sUseThisAsWO
                dtRow("sort_code") = "001"
            End If

            dtRow("craft_code") = ""
            dtRow("shop_person") = dsRows.Tables(0).Rows(I).Item("ISA_EMPLOYEE_ID")
            dtRow("req_no") = dsRows.Tables(0).Rows(I).Item("ORDER_NO")
            dtRow("po_num") = dsRows.Tables(0).Rows(I).Item("PO_REF")
            If Convert.ToString(dsRows.Tables(0).Rows(I).Item("DESCR")).Length > 40 Then
                dtRow("itm_desc") = Convert.ToString(dsRows.Tables(0).Rows(I).Item("DESCR")).Substring(0, 40)
            Else
                dtRow("itm_desc") = dsRows.Tables(0).Rows(I).Item("DESCR")
            End If
            dtRow("inv_no") = dsRows.Tables(0).Rows(I).Item("INVOICE")
            'dtRow("inv_date") = String.Format("{0:d}", dsRows.Tables(0).Rows(I).Item("INVOICE_DT"))
            dtRow("inv_date") = String.Format("{0:d}", dsRows.Tables(0).Rows(I).Item("SHIP_DT"))
            dtRow("payment_date") = ""
            'strCPW = checkCPW(dsRows.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO"))
            strCPW = sUseThisAsWO
            If IsNumeric(dsRows.Tables(0).Rows(I).Item("NET_EXTENDED_AMT")) Then
                decAmount = dsRows.Tables(0).Rows(I).Item("NET_EXTENDED_AMT")
            Else
                decAmount = 0
            End If
            If decAmount = 0 Then
                If IsNumeric(dsRows.Tables(0).Rows(I).Item("TL_COST")) Then
                    decAmount = dsRows.Tables(0).Rows(I).Item("TL_COST")
                End If
            End If

            If strCPW = "P" And _
                IsNumeric(decAmount) Then
                'Dim thDay As New System.DateTime(2010, 6, 11)
                Dim thDay As New System.DateTime '(2012, 6, 30)
                thDay = dteFeeSwitchover

                If DateTime.Parse(dtRow("trans_date").ToString).CompareTo(thDay) = -1 Then

                    'If Now.Date.CompareTo(thDay) = -1 Then
                    'jr june 25 2012 percentage went from 3.3% to 3.6%
                    'dtRow("amount") = FormatNumber(decAmount + (decAmount * 0.03), 2)
                    dtRow("amount") = FormatNumber(decAmount + (decAmount * decProjFeeOld), 2)
                Else
                    'dtRow("amount") = FormatNumber(decAmount + (decAmount * 0.033), 2)
                    dtRow("amount") = FormatNumber(decAmount + (decAmount * decProjFeeNew), 2)
                End If

            Else
                dtRow("amount") = FormatNumber(decAmount, 2)
            End If

            dtRow("cash_no") = FormatNumber(dsRows.Tables(0).Rows(I).Item("NET_EXTENDED_AMT"), 2)

            dtRow("subledger_type") = "M"
            dtRow("batch_no") = dsRows.Tables(0).Rows(I).Item("BATCH_NUMBER")
            dtRow("part") = dsRows.Tables(0).Rows(I).Item("PRODUCT_ID")
            dtRow("itm_qty") = FormatNumber(dsRows.Tables(0).Rows(I).Item("QTY"), 2)
            dtRow("contractor") = "297562"
            dtRow("address_code") = "PHILA"
            dtRow("uom") = dsRows.Tables(0).Rows(I).Item("UNIT_OF_MEASURE")
            dt.Rows.Add(dtRow)

        Next

        For I = 0 To dt.Rows.Count - 1
            strBuilder.Append("")

            For X = 0 To (dt.Columns.Count - 1)
                strBuilder.Append(dt.Rows(I).Item(X) & vbTab)
            Next
            linea = strBuilder.ToString
            sw.WriteLine(linea)
            strBuilder.Remove(0, strBuilder.Length)
        Next
        sw.Close()

        connectSQL.Close()
    End Sub

    Private Function SQLScalar(ByVal strSQLString As String) As String

        Dim commandSQLSel As New SqlCommand(strSQLString, connectSQL)

        Try
            If connectSQL.State.ToString = "Closed" Then
                connectSQL.Open()
            End If
            SQLScalar = commandSQLSel.ExecuteScalar()
        Catch sqlDBExp As SqlException
            Console.WriteLine("")
            Console.WriteLine("***SQLDB select error - " & sqlDBExp.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("     Error " & sqlDBExp.ToString)
            Return True
        End Try

    End Function

    Private Function DoesThisLookLikeUPennWO_1(ByVal sInput As String) As Boolean
        Dim sUPennWOPattern As String = "\b\d{2}-\d{6}(-\d{3})?\b"
        'Dim oRegex As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(sInput, sUPennWOPattern)
        'Dim bMatch as Boolean = False
        'If Not (oRegex Is Nothing) Then
        '    bMatch = oRegex.Success
        'End If
        'oRegex = Nothing
        'Return (bMatch)
        Dim bMatch As Boolean = System.Text.RegularExpressions.Regex.IsMatch(sInput, sUPennWOPattern)
        Return (bMatch)
    End Function

    Private Function DoesThisLookLikeUPennWO_2(ByVal sInput As String) As Boolean
        Dim sUPennWOPattern As String = "\b\d{2,}-\d{3,}(-\d{0,3})?\b"
        'Dim oRegex As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(sInput, sUPennWOPattern)
        'Dim bMatch As Boolean = False
        'If Not (oRegex Is Nothing) Then
        '    bMatch = oRegex.Success
        'End If
        'oRegex = Nothing
        Dim bMatch As Boolean = System.Text.RegularExpressions.Regex.IsMatch(sInput, sUPennWOPattern)
        Return (bMatch)
    End Function

End Module
