Imports System
Imports System.Configuration
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Xml
Imports System.Collections
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
'Imports Oracle.DataAccess.Client
'Imports Oracle.Web

'Modification Log
'* 10/30/14 -  Mike Randall -  added update of new work_order_no column in TRX table
'* 02/04/14 -  Mike Randall - program redesigned to get list of sites & credentials from enterprise table (Avacorp).
'* 10/16/13 -  Mike Randall - Replace MyTime and Date with system date/time.  Remove Oracle driver due to run errors
'* 07/03/13 -  Mike Randall - Replace deprecated Microsoft database drive with Oracle Driver for Win 7 compatability.
'* 07/05/13 -  Corrected missing time by including MyTime in from AC data and adding to date field.
'* 07/05/13 -  Swapped Server date/time from AutoCrib with sysdate for reconciliation ease.


'Imports System.Net.Mail

'Imports SDIVendingAutoCrib.WebPartnerFunctions.WebPSharedFunc ' GEZ 5/29/2012
'Imports System.Web.UI


Module SDIVendingAutocrib

    Dim objStreamWriterLog As StreamWriter
    Dim objStreamWriterTransactions As StreamWriter
    Dim rootDir As String = "C:\SDIVendingAutoCrib"
    Dim logpath As String = "C:\SDIVendingAutoCrib\LOGS\SDIVendingAutoCribLog" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim datapath As String = "C:\SDIVendingAutoCrib\DATA\SDIVendingAutoCribData" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"

    'Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=" & strPWD & ";User ID=" & strUID & ";Data Source=RPTG")
    'Dim connectOR As New OleDbConnection(ConfigurationManager.ConnectionStrings("dbConnect").ConnectionString.ToString)
    Dim connectOR As New OleDbConnection(ConfigurationManager.ConnectionStrings("dbConnect").ConnectionString.ToString)


    Sub Main()

        Console.WriteLine("Start SDIVendingAutoCrib XML in")
        Console.WriteLine("")

        'If Dir(rootDir, FileAttribute.Directory) = "" Then
        '    MkDir(rootDir)
        'End If
        'If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
        '    MkDir(rootDir & "\LOGS")
        'End If
        'If Dir(rootDir & "\DATA", FileAttribute.Directory) = "" Then
        '    MkDir(rootDir & "\DATA")
        'End If

        objStreamWriterLog = File.CreateText(logpath)
        objStreamWriterTransactions = File.CreateText(datapath)
        objStreamWriterLog.WriteLine("Update of SDIVendingAutoCrib Processing XML in " & Now())

        Dim bolError As Boolean = buildSDIVendingMachineIN()

        'SendEmail(bolError)

        objStreamWriterLog.WriteLine("End of SDIVendingAutoCrib Processing XML out " & Now())
        objStreamWriterLog.Flush()
        objStreamWriterLog.Close()
        objStreamWriterTransactions.Flush()
        objStreamWriterTransactions.Close()

    End Sub

    Private Function buildSDIVendingMachineIN() As Boolean

        Dim handler As net.autocrib.www24.AutoCribWS = New net.autocrib.www24.AutoCribWS

        Console.WriteLine("Starting processing of AutoCrib ")
        objStreamWriterLog.WriteLine("Starting processing of AutoCrib ")

        Dim intLastTranID As Integer
        Dim SDITransactions As XmlElement
        connectOR = New OleDbConnection(ConfigurationManager.ConnectionStrings("dbConnect").ConnectionString.ToString)

        Try
            Dim dsAutocriEntInfo As DataSet = GetAutoCribEnterpriseInfo()
            Dim autocribDBName As String = String.Empty
            Dim autocribUName As String = String.Empty
            Dim autocribPwd As String = String.Empty
            Dim businessUnit As String = String.Empty

            Dim trans As OleDbTransaction = connectOR.BeginTransaction
            Dim cmd = connectOR.CreateCommand
            cmd.CommandText = "UPDATE SYSADM8.PS_ISA_LOCK_TBL Set oprid = 'CRIBTRX' Where isa_application = 'AUTOCRIB'"

            cmd.CommandType = CommandType.Text
            cmd.Transaction = trans
            Dim results As Integer = -1

            Try
                results = cmd.ExecuteNonQuery
            Catch ex As Exception
                trans.Rollback()
                Console.WriteLine("Error processing lock table - rolling back" & intLastTranID)
            End Try

            For I = 0 To dsAutocriEntInfo.Tables(0).Rows.Count - 1
                autocribDBName = dsAutocriEntInfo.Tables(0).Rows(I).Item("ISA_AUTOCRIB_DB").ToString
                autocribUName = dsAutocriEntInfo.Tables(0).Rows(I).Item("ISA_AUTOCRIB_USER").ToString
                autocribPwd = dsAutocriEntInfo.Tables(0).Rows(I).Item("ISA_AUTOCRIB_PWD").ToString
                businessUnit = dsAutocriEntInfo.Tables(0).Rows(I).Item("ISA_BUSINESS_UNIT").ToString

                intLastTranID = getLastTranID(businessUnit, trans)

                SDITransactions = handler.GetTransactions(autocribUName, autocribPwd, "", "", intLastTranID, "", "", "", "", "", "", "", "", "", "", "", autocribDBName, "")

                'retrieve(transactions)
                Try
                    Console.WriteLine("the last tran ID being used is " & intLastTranID)
                    objStreamWriterLog.WriteLine("the last tran ID being used is " & intLastTranID)
                    If (SDITransactions.InnerXml = "") Then
                        Console.WriteLine("No transactions for the given last tran ID " & intLastTranID)
                        objStreamWriterLog.WriteLine("No transactions for the given last tran ID " & intLastTranID)
                    Else
                        objStreamWriterTransactions.WriteLine(SDITransactions.InnerXml)

                        processTransactions(SDITransactions, intLastTranID, businessUnit, trans)
                    End If

                Catch ex As Exception
                    Console.WriteLine(ex.ToString)
                    objStreamWriterLog.WriteLine(ex.ToString)
                End Try

            Next
            Try
                trans.Commit()
            Catch ex As Exception
                trans.Rollback()
            End Try
            connectOR.Close()
        Catch ex As Exception

            Console.WriteLine("Error " & ex.Message)
            objStreamWriterLog.WriteLine("Error " & ex.Message)
            Try
                connectOR.Close()
            Catch exConOr As Exception

            End Try
        End Try

    End Function

    Public Function getLastTranID(ByVal strBU As String, ByRef trans As OleDbTransaction) As Integer

        Dim intLastTranID As Integer = 0
        Dim cmd = connectOR.CreateCommand
        cmd.CommandType = CommandType.Text
        cmd.Transaction = trans

        Dim cmdstr As String
        cmdstr = "Select NVL(MAX(A.TRANSACTION_NBR),0) FROM sysadm8.PS_ISA_AUTOCRB_TRX A WHERE BUSINESS_UNIT = '" & strBU & "'"
        cmd.CommandText = cmdstr

        ' Dim commandOR1 As New OleDbCommand("Select NVL(MAX(A.TRANSACTION_NBR),0)" & vbCrLf & _
        '        " FROM sysadm.PS_ISA_AUTOCRB_TRX A WHERE BUSINESS_UNIT = '" + strBU + "'", connectOR)

        Try
            '   intLastTranID = commandOR1.ExecuteScalar()
            intLastTranID = cmd.ExecuteScalar()
        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            'connectOR.Close()
            objStreamWriterLog.WriteLine("     Error - error reading Last transaction ID FROM Transaction table")
            Exit Function
        End Try

        Return intLastTranID

    End Function

    Private Sub processTransactions(ByVal Transactions As XmlNode, ByVal intLastTranID As Integer, ByVal businessUnit As String, ByVal trans As Object)

        Dim dsRows As New DataSet
        dsRows.ReadXml(New XmlNodeReader(Transactions))
        If dsRows.Tables(0).Rows.Count = 0 Then
            Console.WriteLine(" ERROR - no records loaded to dataset")
            Console.WriteLine("")
            objStreamWriterLog.WriteLine(" ERROR - no records loaded to dataset")
            Exit Sub
        Else
            objStreamWriterLog.WriteLine(" processing xml data")
        End If
        'process Oracle updates

        updateVndMchnDB(dsRows, intLastTranID, businessUnit, trans)

    End Sub

    Private Sub updateVndMchnDB(ByVal dsRows As DataSet, ByVal intLastTranID As Integer, ByVal businessUnit As String, ByVal trans As Object)

        Dim intTranNo As Integer = 0
        Dim dteMyDate As DateTime
        Dim dteMyTime As Date
        Dim strStation As String = " "
        Dim strBin As String = " "
        Dim strType As String = " "
        Dim strItem As String = " "
        Dim intItemType As Integer = 0
        Dim intQuantity As Integer = 0
        Dim intPackQty As Integer = 0
        Dim strEmp As String = " "
        Dim strDepartment As String = " "
        Dim strJob As String = " "
        Dim strMachine As String = " "
        Dim strReason As String = " "
        Dim strOperation As String = " "
        Dim strSupplier As String = " "
        Dim strPoNo As String = " "
        Dim strTagNo As String = " "
        Dim strComment As String = " "
        Dim intBurnQty As Integer = 0
        Dim strLot As String = " "
        Dim strSerial As String = " "
        Dim intCurOnHand As Integer = 0
        Dim intCurBurn As Integer = 0
        Dim strPickTicketNo As String = " "
        Dim strPSlip As String = " "
        Dim decMAvgPrice As Decimal = 0.0
        Dim decMAvgCost As Decimal = 0.0
        Dim strExternalPO As String = " "
        Dim strItemRFID As String = " "
        Dim decStandardPrice As Decimal = 0.0
        Dim decAdjStdPrice As Decimal = 0.0
        Dim strVendorItem As String = " "
        Dim dteServerDateTime As DateTime
        Dim intBONo As Integer = 0

        Dim rowsAffected As Integer
        Dim curtime, curdate As String
        '   connectOR.Open()
        If Not dsRows.Tables("TranTypeRequest") Is Nothing Then

        Else
            '  dsRows.Tables("TranTypeRequest") Is Nothing
            objStreamWriterLog.WriteLine("Function updateVndMchnDB, dsRows.Tables(""TranTypeRequest"") Is Nothing for BU: " & businessUnit)
            Exit Sub
        End If
        'compare last Tran ID and delete if less than or equal to
        Dim Y As Integer = dsRows.Tables("TranTypeRequest").Rows.Count
        Dim X As Integer = 0

        Do Until Y = 0
            If dsRows.Tables("TranTypeRequest").Rows(X).Item("TranNo") > intLastTranID Then
                X = X + 1
            Else
                dsRows.Tables("TranTypeRequest").Rows(X).Delete()
            End If
            Y = Y - 1
        Loop

        objStreamWriterLog.WriteLine("Number of rows to be inserted is " & X)
        objStreamWriterLog.WriteLine("for BU: " & businessUnit)

        dsRows.AcceptChanges()

        If dsRows.Tables("TranTypeRequest").Rows.Count > 0 Then

            For I = 0 To dsRows.Tables("TranTypeRequest").Rows.Count - 1
                dteMyDate = New DateTime
                dteMyTime = New DateTime

                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("TranNo")) = "" Then
                    intTranNo = dsRows.Tables("TranTypeRequest").Rows(I).Item("TranNo")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("MyDate")) = "" Then
                    dteMyDate = dsRows.Tables("TranTypeRequest").Rows(I).Item("MyDate").ToString
                    dteMyTime = dsRows.Tables("TranTypeRequest").Rows(I).Item("MyTime").ToString
                    dteMyTime = (dteMyTime.ToString("HH:mm"))
                    '  Mike Randall 7/5

                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Station")) = "" Then
                    strStation = dsRows.Tables("TranTypeRequest").Rows(I).Item("Station")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Bin")) = "" Then
                    strBin = dsRows.Tables("TranTypeRequest").Rows(I).Item("Bin")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Type")) = "" Then
                    strType = dsRows.Tables("TranTypeRequest").Rows(I).Item("Type")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Item")) = "" Then
                    strItem = dsRows.Tables("TranTypeRequest").Rows(I).Item("Item")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("ItemType")) = "" Then
                    intItemType = dsRows.Tables("TranTypeRequest").Rows(I).Item("ItemType")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Quantity")) = "" Then
                    intQuantity = dsRows.Tables("TranTypeRequest").Rows(I).Item("Quantity")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("PackQty")) = "" Then
                    intPackQty = dsRows.Tables("TranTypeRequest").Rows(I).Item("PackQty")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Emp")) = "" Then
                    strEmp = dsRows.Tables("TranTypeRequest").Rows(I).Item("Emp")
                End If
                If strEmp.Length > 10 Then
                    strEmp = strEmp.Substring(0, 10)
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Department")) = "" Then
                    strDepartment = dsRows.Tables("TranTypeRequest").Rows(I).Item("Department")
                End If

                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Job")) = "" Then
                    strJob = dsRows.Tables("TranTypeRequest").Rows(I).Item("Job")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Machine")) = "" Then
                    strMachine = dsRows.Tables("TranTypeRequest").Rows(I).Item("Machine")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Reason")) = "" Then
                    strReason = dsRows.Tables("TranTypeRequest").Rows(I).Item("Reason")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Operation")) = "" Then
                    strOperation = dsRows.Tables("TranTypeRequest").Rows(I).Item("Operation")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Supplier")) = "" Then
                    strSupplier = dsRows.Tables("TranTypeRequest").Rows(I).Item("Supplier")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("PoNo")) = "" Then
                    strPoNo = dsRows.Tables("TranTypeRequest").Rows(I).Item("PoNo")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("TagNo")) = "" Then
                    strTagNo = dsRows.Tables("TranTypeRequest").Rows(I).Item("TagNo")
                End If

                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Comment")) = "" Then
                    strComment = dsRows.Tables("TranTypeRequest").Rows(I).Item("Comment")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("BurnQty")) = "" Then
                    intBurnQty = dsRows.Tables("TranTypeRequest").Rows(I).Item("BurnQty")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Lot")) = "" Then
                    strLot = dsRows.Tables("TranTypeRequest").Rows(I).Item("Lot")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("Serial")) = "" Then
                    strSerial = dsRows.Tables("TranTypeRequest").Rows(I).Item("Serial")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("CurOnHand")) = "" Then
                    intCurOnHand = dsRows.Tables("TranTypeRequest").Rows(I).Item("CurOnHand")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("CurBurn")) = "" Then
                    intCurBurn = dsRows.Tables("TranTypeRequest").Rows(I).Item("CurBurn")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("PickTicketNo")) = "" Then
                    strPickTicketNo = dsRows.Tables("TranTypeRequest").Rows(I).Item("PickTicketNo")
                End If

                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("PSlip")) = "" Then
                    strPSlip = dsRows.Tables("TranTypeRequest").Rows(I).Item("PSlip")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("MAvgPrice")) = "" Then
                    decMAvgPrice = dsRows.Tables("TranTypeRequest").Rows(I).Item("MAvgPrice")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("MAvgCost")) = "" Then
                    decMAvgCost = dsRows.Tables("TranTypeRequest").Rows(I).Item("MAvgCost")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("ExternalPO")) = "" Then
                    strExternalPO = dsRows.Tables("TranTypeRequest").Rows(I).Item("ExternalPO")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("ItemRFID")) = "" Then
                    strItemRFID = dsRows.Tables("TranTypeRequest").Rows(I).Item("ItemRFID")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("StandardPrice")) = "" Then
                    decStandardPrice = dsRows.Tables("TranTypeRequest").Rows(I).Item("StandardPrice")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("AdjStdPrice")) = "" Then
                    decAdjStdPrice = dsRows.Tables("TranTypeRequest").Rows(I).Item("AdjStdPrice")
                End If

                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("VendorItem")) = "" Then
                    strVendorItem = dsRows.Tables("TranTypeRequest").Rows(I).Item("VendorItem")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("ServerDateTime")) = "" Then
                    dteServerDateTime = dsRows.Tables("TranTypeRequest").Rows(I).Item("ServerDateTime")
                End If
                If Not Trim(dsRows.Tables("TranTypeRequest").Rows(I).Item("BONo")) = "" Then
                    intBONo = dsRows.Tables("TranTypeRequest").Rows(I).Item("BONo")
                End If
                Dim strSQL As String = ""
                Try
                    If Trim(strSupplier) <> "" Then
                        strSupplier = Trim(strSupplier)
                        If Len(strSupplier) > 10 Then
                            strSupplier = Microsoft.VisualBasic.Left(strSupplier, 10)
                        End If
                    Else
                        strSupplier = " "
                    End If
                Catch ex As Exception
                    strSupplier = " "
                End Try
                
                strSQL = "INSERT INTO SYSADM8.PS_ISA_AUTOCRB_TRX" & vbCrLf & _
                            " (TRANSACTION_NBR,DT_TIMESTAMP, ISA_SPRO_DEVICE," & vbCrLf & _
                            " ISA_AUTOCRIB_BIN,ISA_BIN_TYPE,INV_ITEM_ID," & vbCrLf & _
                            " INV_ITEM_TYPE,QTY,PACK_QTY_1," & vbCrLf & _
                            " ISA_EMPLOYEE_ID,ISA_CUST_CHARGE_CD,isa_work_order_no,ISA_MACHINE_NO," & vbCrLf & _
                            " ISA_RECEIPT_CODE,ISA_TRANS_TYPE,VENDOR_ID," & vbCrLf & _
                            " PO_ID,TAG_NUMBER,COMMENT_TEXT," & vbCrLf & _
                            " ISA_BURN_QTY,LOT_ID,SERIAL_ID," & vbCrLf & _
                            " QTY_ONHAND,ISA_CUR_BURN,ISA_TICKETNO," & vbCrLf & _
                            " PACKSLIP_NO,AVG_INV_PRICE,TL_COST," & vbCrLf & _
                            " CUSTOMER_PO,ISA_ITEM_RFID,STANDARD_PRICE," & vbCrLf & _
                            " ADJUSTED_PRICE,ITM_ID_VNDR,MAINT_DTTM," & vbCrLf & _
                            " QTY_BACKORDER,PROCESS_STATUS,BUSINESS_UNIT_OM," & vbCrLf & _
                            " ORDER_NO, BUSINESS_UNIT)" & vbCrLf & _
                            " VALUES('" & intTranNo & "',"
                If dteMyDate = Nothing Then
                    strSQL &= "NULL, "
                Else
                    curdate = DateTime.Now.ToShortDateString
                    curtime = DateTime.Now.ToString("HH:mm:ss")

                    strSQL &= " TO_DATE('" & curdate & " " & curtime & "', 'MM/DD/YYYY hh24:mi:ss'),"

                    '  Mike Randall 10/16/14 - replace autocrib time with a timestamp
                    '  Mike Randall 7/30/15 - truncate job to 20 chars and item id to 18
                    '  Mike Randall 12/23/15 - truncate bin to 10 chars to match column limit
                    '  VR 04/29/2016 - truncate bin to 12 chars to match column limit
                End If
                strSQL &= " '" & strStation & "'," & vbCrLf & _
                            " '" & Left(strBin, 12) & "'," & vbCrLf & _
                            " '" & strType & "'," & vbCrLf & _
                            " '" & Left(strItem, 18) & "'," & vbCrLf & _
                            " " & intItemType & "," & vbCrLf & _
                            " " & intQuantity & "," & vbCrLf & _
                            " " & intPackQty & "," & vbCrLf & _
                            " '" & strEmp & "'," & vbCrLf & _
                            " '" & strDepartment & "'," & vbCrLf & _
                            " '" & Left(strJob, 20) & "'," & vbCrLf & _
                            " '" & strMachine & "'," & vbCrLf & _
                            " '" & strReason & "'," & vbCrLf & _
                            " '" & strOperation & "'," & vbCrLf & _
                            " '" & strSupplier & "'," & vbCrLf & _
                            " '" & strPoNo & "'," & vbCrLf & _
                            " '" & strTagNo & "'," & vbCrLf & _
                            " '" & strComment & "'," & vbCrLf & _
                            " " & intBurnQty & "," & vbCrLf & _
                            " '" & strLot & "'," & vbCrLf & _
                            " '" & strSerial & "'," & vbCrLf & _
                            " " & intCurOnHand & "," & vbCrLf & _
                            " " & intCurBurn & "," & vbCrLf & _
                            " '" & strPickTicketNo & "'," & vbCrLf & _
                            " '" & strPSlip & "'," & vbCrLf & _
                            " " & decMAvgPrice & "," & vbCrLf & _
                            " " & decMAvgCost & "," & vbCrLf & _
                            " '" & strExternalPO & "'," & vbCrLf & _
                            " '" & strItemRFID & "'," & vbCrLf & _
                            " " & decStandardPrice & "," & vbCrLf & _
                            " " & decAdjStdPrice & "," & vbCrLf & _
                            " '" & strVendorItem & "'," & vbCrLf & _
                            " TO_DATE('" & DateTime.Now.ToString("MM/dd/yyyy HH:mm") & "', 'MM/DD/YYYY HH24:MI:SS')," & vbCrLf & _
                            " " & intBONo & "," & vbCrLf & _
                            "'N',' ',' ','" & businessUnit & "')"


                Dim cmd = connectOR.CreateCommand
                cmd.CommandType = CommandType.Text
                cmd.Transaction = trans
                cmd.CommandText = strSQL




                '   Dim commandOR1 As New OleDbCommand(strSQL, connectOR)

                Try

                    rowsAffected = cmd.ExecuteNonQuery()

                Catch OLEDBExp As OleDbException
                    Console.WriteLine("")
                    Console.WriteLine("***OLEDB error - " & OLEDBExp.ToString)
                    Console.WriteLine("")
                    Console.WriteLine(cmd.CommandText)
                    Console.WriteLine("")
                    objStreamWriterLog.WriteLine(OLEDBExp.ToString)
                    objStreamWriterLog.WriteLine(cmd.CommandText)
                    Exit Sub
                Finally
                    'Do the final cleanup such as closing the Database connection
                End Try
            Next

        End If


    End Sub

    Private Function GetAutoCribEnterpriseInfo() As DataSet

        Dim dsAutocriEntInfo As DataSet = New DataSet()

        Dim commandOR1 As New OleDbCommand("Select CUST_ID, ISA_AUTOCRIB_DB, ISA_AUTOCRIB_USER, ISA_AUTOCRIB_PWD, ISA_BUSINESS_UNIT " & vbCrLf & _
                                           "FROM SYSADM8.PS_ISA_ENTERPRISE ENT WHERE ISA_AUTOCRIB_USER IS NOT NULL and ISA_AUTOCRIB_DB <> ' '", connectOR)

        connectOR.Open()

        Dim objDataAdapter As New OleDbDataAdapter(commandOR1)

        Try
            objDataAdapter.Fill(dsAutocriEntInfo)
        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            'connectOR.Close()
            objStreamWriterLog.WriteLine("     Error - error reading Autocrib Credentials FROM Enterprise table")
            Exit Function
        End Try
        '    connectOR.Close()

        Return dsAutocriEntInfo

    End Function


End Module