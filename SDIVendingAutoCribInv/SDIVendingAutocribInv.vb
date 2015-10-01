Imports System
Imports System.Configuration
Imports System.Linq
Imports System.Xml
Imports System.IO
'Imports System.Data.OracleClient
Imports System.Data.OleDb



Module SDIVendingAutocribInv

    Dim objStreamWriterLog As StreamWriter
    Dim objStreamWriterTransactions As StreamWriter
    Dim rootDir As String = "C:\SDIVendingAutoCribBin"
    Dim logpath As String = "C:\SDIVendingAutoCrib\LOGS\SDIVendingAutoCribBinLog" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim datapath As String = "C:\SDIVendingAutoCrib\DATA\SDIVendingAutoCribBinData" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
    ';Dim connectstr As String = ConfigurationManager.ConnectionStrings("sysdb").ConnectionString.ToString
    Dim connectOR As New OleDbConnection(ConfigurationManager.ConnectionStrings("dbConnect").ConnectionString.ToString)

    'Dim connectOR As New OracleConnection(connectstr)

    '*******************************************************************************************************
    '* Author ------------ Mike Randall
    '* Date -------------- 5/28/13
    '* Function ---------- Console utility to retrieve Autocrib inventory data via GetBins web service and store it in an Oracle table.
    '*
    '* Modification History:
    '*
    '*******************************************************************************************************

    Sub Main()

        Console.WriteLine("Start SDIVendingAutoCrib Bin XML in")
        Console.WriteLine("")

        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
        End If
        If Dir(rootDir & "\DATA", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\DATA")
        End If

        objStreamWriterLog = File.CreateText(logpath)
        objStreamWriterTransactions = File.CreateText(datapath)
        objStreamWriterLog.WriteLine("Update of SDIVendingAutoCrib Bin Processing XML in " & Now())

        Dim bolError As Boolean = buildSDIVendingMachineIN()

        objStreamWriterLog.Flush()
        objStreamWriterLog.Close()
        objStreamWriterTransactions.Flush()
        objStreamWriterTransactions.Close()
        'retrieve transactions
    End Sub

    Private Function buildSDIVendingMachineIN() As Boolean

        Dim handler As net.autocrib.www24.AutoCribWS = New net.autocrib.www24.AutoCribWS

        '   Dim handler As AutoCrib = New AutoCrib

        Console.WriteLine("Starting processing of AutoCrib Bins")
        objStreamWriterLog.WriteLine("Starting processing of AutoCrib Bins")

        Dim SDITransactions As XmlElement
        Try

            Dim dsAutocriEntInfo As DataSet = GetAutoCribEnterpriseInfo()
            Dim autocribDBName As String = String.Empty
            Dim autocribUName As String = String.Empty
            Dim autocribPwd As String = String.Empty
            Dim businessUnit As String = String.Empty

            For I = 0 To dsAutocriEntInfo.Tables(0).Rows.Count - 1
                autocribDBName = dsAutocriEntInfo.Tables(0).Rows(I).Item("ISA_AUTOCRIB_DB")
                autocribUName = dsAutocriEntInfo.Tables(0).Rows(I).Item("ISA_AUTOCRIB_USER")
                autocribPwd = dsAutocriEntInfo.Tables(0).Rows(I).Item("ISA_AUTOCRIB_PWD")
                businessUnit = dsAutocriEntInfo.Tables(0).Rows(I).Item("ISA_BUSINESS_UNIT")
                Try
                    SDITransactions = handler.GetBins(autocribUName, autocribPwd, autocribDBName, "", "", "", "", True)

                    'retrieve transactions
                    If (SDITransactions.InnerXml = "") Then
                        Console.WriteLine("No Inventory returned ")
                        objStreamWriterLog.WriteLine("No Invenntory returned")
                    Else
                        objStreamWriterTransactions.WriteLine(SDITransactions.InnerXml)
                        processBins(SDITransactions, businessUnit)
                    End If

                Catch ex As Exception
                    Console.WriteLine(ex.ToString)
                    objStreamWriterLog.WriteLine(ex.ToString)
                End Try

            Next
        Catch ex As Exception
            Console.WriteLine("Error " & ex.Message)
            objStreamWriterLog.WriteLine("Error " & ex.Message)
            Exit Function
        End Try



    End Function

    Private Sub processBins(ByVal Transactions As XmlNode, ByVal BusinessUnit As String)


        Dim dsRows As New DataSet
        dsRows.ReadXml(New XmlNodeReader(Transactions))
        Dim strItem, strCrib, strBin, strSQL As String
        Dim intOnhand, intPack, rowsAffected As Integer
        Dim dteServerDateTime As DateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm")

        If dsRows.Tables(4).Rows.Count = 0 Then
            Console.WriteLine(" ERROR - no bin records loaded to dataset")
            Console.WriteLine("")
            objStreamWriterLog.WriteLine(" ERROR - no bin records loaded to dataset")
            Exit Sub
        Else
            connectOR.Open()
            '   Dim commandOR1 As New OleDbCommand(strSQL, connectOR)
            Dim commandOR1 As New OleDbCommand(strSQL, connectOR)
            For Each row As DataRow In dsRows.Tables(4).Rows
                strItem = row("Item")
                If strItem.Trim().Length > 18 Then
                    strItem = strItem.Substring(0, 18)
                End If

                strCrib = row("Station")
                If strCrib.Trim().Length > 30 Then
                    strCrib.Substring(0, 30)
                End If

                strBin = row("MyNo")
                If strBin.Trim().Length > 10 Then
                    strBin = strBin.Substring(0, 10)
                End If
                intOnhand = row("OnHand")
                intPack = row("PackQty")

                strSQL = "INSERT INTO SYSADM.ps_isa_autocrb_inv" & vbCrLf & _
                        " (ISA_SPRO_DEVICE, ISA_AUTOCRIB_BIN, INV_ITEM_ID, QTY, PACK_QTY_1, BUSINESS_UNIT, DT_TIMESTAMP)" & vbCrLf & _
                        " VALUES('" & strCrib & "'," & _
                        " '" & strBin & "'," & vbCrLf & _
                        " '" & strItem & "'," & vbCrLf & _
                        " " & intOnhand & "," & vbCrLf & _
                        " " & intPack & "," & vbCrLf & _
                        " '" & BusinessUnit & "'," & vbCrLf & _
                        " TO_DATE('" & dteServerDateTime & "', 'MM/DD/YYYY HH:MI:SS AM'))"

                Try
                    commandOR1.CommandText = strSQL
                    rowsAffected = commandOR1.ExecuteNonQuery()

                Catch OLEDBExp As OleDbException
                    'Catch OracleExp As OracleException
                    Console.WriteLine("")
                    Console.WriteLine("***Oracle Data Access error - " & OLEDBExp.ToString)
                    Console.WriteLine("")
                    Console.WriteLine(commandOR1.CommandText)
                    Console.WriteLine("")
                    objStreamWriterLog.WriteLine(OLEDBExp.ToString)
                    objStreamWriterLog.WriteLine(commandOR1.CommandText)
                    'Exit Sub
                Finally
                    'Do the final cleanup such as closing the Database connection
                End Try
            Next row
            connectOR.Close()
        End If
        '    process Oracle updates

        '    updateVndMchnDB(dsRows, intLastTranID)

    End Sub

    Private Function GetAutoCribEnterpriseInfo() As DataSet

        Dim dsAutocriEntInfo As DataSet = New DataSet()
        connectOR.Open()

        Dim commandOR1 As New OleDbCommand("Select CUST_ID, ISA_AUTOCRIB_DB, ISA_AUTOCRIB_USER, ISA_AUTOCRIB_PWD, ISA_BUSINESS_UNIT " & vbCrLf & _
                                           "FROM sysadm.PS_ISA_ENTERPRISE ENT WHERE ISA_AUTOCRIB_USER IS NOT NULL and ISA_AUTOCRIB_DB <> ' '", connectOR)
        Dim objDataAdapter As New OleDbDataAdapter(commandOR1)

        Try
            objDataAdapter.Fill(dsAutocriEntInfo)
        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            objStreamWriterLog.WriteLine("     Error - error reading Autocrib Credentials FROM Enterprise table")
            Exit Function
        End Try
        connectOR.Close()

        Return dsAutocriEntInfo

    End Function


End Module
