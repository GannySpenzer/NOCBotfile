Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
Imports System.Collections.Generic

Module Module1

    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\StatChg"
    Dim logpath As String = "C:\StatChg\LOGS\StatChgEmailOutUNCC" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR")

    Sub Main()

        Console.WriteLine("Start StatChg Email send")
        Console.WriteLine("")

        'read settings

        '   (1) connection string / db connection
        Dim cnString As String = ""
        Try
            cnString = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
        End Try
        If (cnString.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            connectOR = New OleDbConnection(cnString)
        End If
        '   (2) root directory
        Dim rDir As String = ""
        Try
            rDir = My.Settings("myDr1").ToString.Trim
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
            logpath = sLogPath & "\StatChgEmailOutUNCC" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("  Send emails out " & Now())

        Dim bolError As Boolean = buildstatchgout()
        If bolError = True Then
            SendEmail()
        End If


        objStreamWriter.WriteLine("  End of StatChg Email send " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub

    Private Function buildstatchgout() As Boolean

        ' get XML file of sites that require email
        Dim strXMLDir As String = rootDir & "\EmailSites.xml"
        Dim xmldata As New XmlDocument
        Dim sr As System.IO.StreamReader
        Dim XMLContent As String
        Dim jobNode As XmlNode
        sr = New System.IO.StreamReader(strXMLDir)
        XMLContent = sr.ReadToEnd()
        sr.Close()
        xmldata.LoadXml(XMLContent)
        jobNode = xmldata.ChildNodes(1)
        Dim dsRows As New DataSet
        dsRows.ReadXml(New XmlNodeReader(jobNode))
        Dim I As Integer
        Dim bolErrorSomeWhere As Boolean

        ' check stock
        For I = 0 To dsRows.Tables(0).Rows.Count - 1
            If dsRows.Tables(0).Rows(I).Item("SITESTK") = "Y" And dsRows.Tables(0).Rows(I).Item("SITEBU") = "I0256" Then
                objStreamWriter.WriteLine("  StatChg Email send stock emails for " & dsRows.Tables(0).Rows(I).Item("SITEBU"))
                buildstatchgout = checkStock(dsRows.Tables(0).Rows(I).Item("SITEBU"), dsRows.Tables(0).Rows(I).Item("SITESTART"))
                buildstatchgout = False
                If buildstatchgout = True Then
                    bolErrorSomeWhere = True
                End If
            End If
        Next

        ' check non-stock
        For I = 0 To dsRows.Tables(0).Rows.Count - 1
            If dsRows.Tables(0).Rows(I).Item("SITENSTK") = "Y" And dsRows.Tables(0).Rows(I).Item("SITEBU") = "I0256" Then
                objStreamWriter.WriteLine("  StatChg Email send nonstock emails for " & dsRows.Tables(0).Rows(I).Item("SITEBU"))
                buildstatchgout = checkNonStock(dsRows.Tables(0).Rows(I).Item("SITEBU"), dsRows.Tables(0).Rows(I).Item("SITESTART"))
                If buildstatchgout = True Then
                    bolErrorSomeWhere = True
                End If
            End If
        Next

        For I = 0 To dsRows.Tables(0).Rows.Count - 1
            If dsRows.Tables(0).Rows(I).Item("ALLSTATUS") = "Y" And dsRows.Tables(0).Rows(I).Item("SITEBU") = "I0256" Then
                objStreamWriter.WriteLine("  StatChg Email send allstatus emails for " & dsRows.Tables(0).Rows(I).Item("SITEBU"))
                buildstatchgout = checkAllStatus_7(dsRows.Tables(0).Rows(I).Item("SITEBU"), dsRows.Tables(0).Rows(I).Item("SITESTART"))
                If buildstatchgout = True Then
                    bolErrorSomeWhere = True
                End If
            End If
        Next


        '7 is stock
        'R is non-stock

        bolErrorSomeWhere = buildNotifyReceiver("7")
        bolErrorSomeWhere = buildNotifyReceiver("R")
        Return bolErrorSomeWhere

    End Function

    Private Function checkNonStock(ByVal strBU As String, ByVal dtrStartDate As String) As Boolean

        Dim dteStrDate As DateTime
        dteStrDate = Now.AddMonths(-3).ToString
        Dim strSiteBU As String
        Dim strSQLstring As String
        Dim Command As OleDbCommand

        strSQLstring = "SELECT A.BUSINESS_UNIT" & vbCrLf & _
                " FROM PS_REQ_LOADER_DFL A" & vbCrLf & _
                " WHERE A.LOADER_BU = 'I0256'" & vbCrLf

        Command = New OleDbCommand(strSQLstring, connectOR)
        connectOR.Open()
        Try
            strSiteBU = Command.ExecuteScalar
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("  StatChg Email NSTK send select siteBU for I0256 ")
            connectOR.Close()
            Return True
        End Try

        strSQLstring = "SELECT DISTINCT A.ORDER_NO, B.ISA_INTFC_LN AS LINE_NBR, E.RECEIVER_ID," & vbCrLf & _
                " E.RECV_LN_NBR, A.BUSINESS_UNIT_OM, B.ISA_EMPLOYEE_ID AS EMPLID, E.DESCR254_MIXED" & vbCrLf & _
                " FROM PS_ISA_ORD_INTF_HD A, PS_ISA_ORD_INTF_LN B," & vbCrLf & _
                " PS_ISA_USERS_TBL C, PS_PO_LINE_DISTRIB D," & vbCrLf & _
                " PS_RECV_LN_SHIP E" & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = 'I0256'" & vbCrLf & _
                " AND A.ADD_DTTM > TO_DATE('" & dteStrDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                " AND A.ADD_DTTM > TO_DATE('" & dtrStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                " AND A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                " AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf & _
                " AND B.ISA_EMPLOYEE_ID = C.ISA_EMPLOYEE_ID" & vbCrLf & _
                " AND D.BUSINESS_UNIT = 'ISA00'" & vbCrLf & _
                " AND A.ORDER_NO = D.REQ_ID" & vbCrLf & _
                " AND B.ISA_INTFC_LN = D.REQ_LINE_NBR" & vbCrLf & _
                " AND D.BUSINESS_UNIT = E.BUSINESS_UNIT" & vbCrLf & _
                " AND D.PO_ID = E.PO_ID" & vbCrLf & _
                " AND D.LINE_NBR = E.LINE_NBR" & vbCrLf & _
                " AND E.QTY_SH_ACCPT > 0" & vbCrLf & _
                " AND NOT EXISTS (SELECT 'X'" & vbCrLf & _
                " FROM PS_RTV_LN F" & vbCrLf & _
                " WHERE F.BUSINESS_UNIT_PO = D.BUSINESS_UNIT" & vbCrLf & _
                " AND F.PO_ID = D.PO_ID" & vbCrLf & _
                " AND F.LINE_NBR = D.LINE_NBR" & vbCrLf & _
                " AND F.QTY_LN_RETRN_SUOM = E.QTY_SH_ACCPT" & vbCrLf & _
                " AND F.RETURN_REASON = 'MTX')" & vbCrLf & _
                " AND NOT EXISTS (SELECT 'X'" & vbCrLf & _
                " FROM PS_ISA_ORDSTAT_EML G" & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = G.BUSINESS_UNIT_OM" & vbCrLf & _
                " AND A.ORDER_NO = G.ORDER_NO" & vbCrLf & _
                " AND B.ISA_INTFC_LN = G.LINE_NBR" & vbCrLf & _
                " AND E.RECEIVER_ID = G.RECEIVER_ID" & vbCrLf & _
                " AND E.RECV_LN_NBR = G.RECV_LN_NBR" & vbCrLf & _
                " AND G.ISA_LINE_STATUS = 'RET')" & vbCrLf & _
                " ORDER BY ORDER_NO, LINE_NBR, RECEIVER_ID, RECV_LN_NBR"

        Command = New OleDbCommand(strSQLstring, connectOR)
        connectOR.Open()
        Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("  StatChg Email NSTK send select orders for I0256")
            connectOR.Close()
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            objStreamWriter.WriteLine("  StatChg Email NSTK send select orders = 0 for I0256")
            connectOR.Close()
            Return False
        End If

        'insert into the PS_ISA_ORDSTAT_EML table

        Dim I As Integer

        connectOR.Open()
        For I = 0 To ds.Tables(0).Rows.Count - 1
            Dim strBUSINESSUNITOM As String = ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM")
            Dim strORDERNO As String = ds.Tables(0).Rows(I).Item("ORDER_NO")
            Dim strINTFCLINENUM As String = ds.Tables(0).Rows(I).Item("LINE_NBR")
            Dim strRECEIVERID As String = ds.Tables(0).Rows(I).Item("RECEIVER_ID")
            Dim strRECVLNNBR As String = ds.Tables(0).Rows(I).Item("RECV_LN_NBR")
            Dim strEMPLID As String = ds.Tables(0).Rows(I).Item("EMPLID")
            Dim strDesc254 As String = ds.Tables(0).Rows(I).Item("DESCR254_MIXED")

            strSQLstring = "INSERT INTO PS_ISA_ORDSTAT_EML" & vbCrLf & _
                        " VALUES ('" & strBUSINESSUNITOM & "'," & vbCrLf & _
                        " '" & strORDERNO & "'," & vbCrLf & _
                        " '" & strINTFCLINENUM & "'," & vbCrLf & _
                        " '0'," & vbCrLf & _
                        " '0'," & vbCrLf & _
                        " '" & strRECEIVERID & "'," & vbCrLf & _
                        " '" & strRECVLNNBR & "'," & vbCrLf & _
                        " '" & strEMPLID & "'," & vbCrLf & _
                        " 'RET', '')" & vbCrLf
                         

            Dim command1 As OleDbCommand
            command1 = New OleDbCommand(strSQLstring, connectOR)
            Try
                Dim rowsaffected As Integer
                rowsaffected = command1.ExecuteNonQuery
                If Not rowsaffected = 1 Then
                    objStreamWriter.WriteLine("  StatChg Email NSTK send insert orders for " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " & _
                     ds.Tables(0).Rows(I).Item("LINE_NBR") & " " & _
                                        ds.Tables(0).Rows(I).Item("RECEIVER_ID") & " " & _
                                        ds.Tables(0).Rows(I).Item("RECV_LN_NBR"))
                    checkNonStock = True
                End If
                command1.Dispose()
            Catch ex As Exception
                objStreamWriter.WriteLine("  StatChg Email NSTK send insert error for " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " & _
                    ds.Tables(0).Rows(I).Item("LINE_NBR") & " " & _
                    ds.Tables(0).Rows(I).Item("RECEIVER_ID") & " " & _
                    ds.Tables(0).Rows(I).Item("RECV_LN_NBR"))
                objStreamWriter.WriteLine(ex.ToString)
                checkNonStock = True
            End Try
        Next
        objStreamWriter.WriteLine("  StatChg Email NSTK send select orders = " & ds.Tables(0).Rows.Count & " for I0256")

        connectOR.Close()


    End Function

    Private Function checkStock(ByVal strbu As String, ByVal dtrStartDate As String) As Boolean
        ' the union all in the sql below - diferences between top and bot
        ' bot " AND E.CONFIRMED_FLAG = 'Y'" & vbCrLf & _
        ' 
        Dim dteStrDate As DateTime
        dteStrDate = Now.AddMonths(-3).ToString
        Dim strSQLstring As String
        strSQLstring = "SELECT B.ORDER_NO, B.ISA_INTFC_LN AS INTFC_LINE_NUM, B.ISA_INTFC_LN AS ORDER_INT_LINE_NO, D.DEMAND_LINE_NO," & vbCrLf & _
                " B.BUSINESS_UNIT_OM, B.ISA_EMPLOYEE_ID AS EMPLID, E.DESCR60, E.INV_ITEM_ID" & vbCrLf & _
                " FROM PS_ISA_ORD_INTF_HD A, PS_ISA_ORD_INTF_LN B," & vbCrLf & _
                " PS_ISA_USERS_TBL C, SYSADM8.PS_IN_DEMAND D," & vbCrLf & _
                " PS_MASTER_ITEM_TBL E" & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = 'I0256'" & vbCrLf & _
                " AND A.ADD_DTTM > TO_DATE('" & dteStrDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                " AND A.ADD_DTTM > TO_DATE('" & dtrStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                " AND A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                " AND B.INV_ITEM_ID <> ' '" & vbCrLf & _
                " AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf & _
                " AND B.ISA_EMPLOYEE_ID = C.ISA_EMPLOYEE_ID" & vbCrLf & _
                " AND B.ISA_INTFC_LN = D.ORDER_INT_LINE_NO" & vbCrLf & _
                " AND B.ORDER_NO = D.ORDER_NO" & vbCrLf & _
                " AND E.SETID = 'MAIN1'" & vbCrLf & _
                " AND D.INV_ITEM_ID = E.INV_ITEM_ID" & vbCrLf & _
                " AND D.IN_FULFILL_STATE IN ('60','50')" & vbCrLf & _
                " AND D.DEMAND_SOURCE = 'OM'" & vbCrLf & _
                " AND D.QTY_PICKED > 0" & vbCrLf & _
                " AND NOT EXISTS (SELECT 'X'" & vbCrLf & _
                " FROM PS_ISA_ORDSTAT_EML F" & vbCrLf & _
                " WHERE F.BUSINESS_UNIT_OM = D.SOURCE_BUS_UNIT" & vbCrLf & _
                " AND F.ORDER_NO = D.ORDER_NO" & vbCrLf & _
                " AND F.DEMAND_LINE_NO = D.DEMAND_LINE_NO" & vbCrLf & _
                " AND F.ORDER_INT_LINE_NO = D.ORDER_INT_LINE_NO" & vbCrLf & _
                " AND F.ISA_LINE_STATUS IN ('PKP', 'PKF'))" & vbCrLf & _
                " ORDER BY ORDER_NO, INTFC_LINE_NUM, DEMAND_LINE_NO"

        Dim Command = New OleDbCommand(strSQLstring, connectOR)
        connectOR.Open()
        Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("  StatChg Email send select orders for I0256")
            connectOR.Close()
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            objStreamWriter.WriteLine("  StatChg Email send select orders = 0 for I0256")
            connectOR.Close()
            Return False
        End If

        'insert into the PS_ISA_ORDSTAT_EML table

        Dim I As Integer

        connectOR.Open()
        For I = 0 To ds.Tables(0).Rows.Count - 1
            Dim strBUSINESSUNITOM As String = ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM")
            Dim strORDERNO As String = ds.Tables(0).Rows(I).Item("ORDER_NO")
            Dim strINTFCLINENUM As String = ds.Tables(0).Rows(I).Item("INTFC_LINE_NUM")
            Dim strORDERINTLINENO As String = ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")
            Dim strDEMANDLINENO As String = ds.Tables(0).Rows(I).Item("DEMAND_LINE_NO")
            Dim strEMPLID As String = ds.Tables(0).Rows(I).Item("EMPLID")
            Dim strDesc254S As String = ds.Tables(0).Rows(I).Item("DESCR60")


            strSQLstring = "INSERT INTO PS_ISA_ORDSTAT_EML" & vbCrLf & _
                        " VALUES ('" & strBUSINESSUNITOM & "'," & vbCrLf & _
                        " '" & strORDERNO & "'," & vbCrLf & _
                        " '" & strINTFCLINENUM & "'," & vbCrLf & _
                        " '" & strORDERINTLINENO & "'," & vbCrLf & _
                        " '" & strDEMANDLINENO & "'," & vbCrLf & _
                        " ' ',0," & vbCrLf & _
                        " '" & strEMPLID & "'," & vbCrLf & _
                         " 'PKF', '')" & vbCrLf
                   
            Dim command1 As OleDbCommand
            command1 = New OleDbCommand(strSQLstring, connectOR)
            Try
                Dim rowsaffected As Integer
                rowsaffected = command1.ExecuteNonQuery
                If Not rowsaffected = 1 Then
                    objStreamWriter.WriteLine("  StatChg Email send insert error for " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " & _
                    ds.Tables(0).Rows(I).Item("INTFC_LINE_NUM") & " " & _
                    ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO") & " " & _
                    ds.Tables(0).Rows(I).Item("DEMAND_LINE_NO"))
                    checkStock = True
                End If
                command1.Dispose()
            Catch ex As Exception
                objStreamWriter.WriteLine("  StatChg Email send insert orders for " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " & _
                    ds.Tables(0).Rows(I).Item("INTFC_LINE_NUM") & " " & _
                    ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO") & " " & _
                    ds.Tables(0).Rows(I).Item("DEMAND_LINE_NO"))
                checkStock = True
            End Try
        Next

        objStreamWriter.WriteLine("  StatChg Email STK send select orders = " & ds.Tables(0).Rows.Count & " for I0256")
        connectOR.Close()

    End Function

    Private Function buildNotifyReceiver(ByVal strOrderStatus) As Boolean

        Select Case strOrderStatus
            Case "7"
                buildNotifyReceiver = buildNotifyStkReady()
            Case "R"
                buildNotifyReceiver = buildNotifyNSTKReady()
        End Select
    End Function

    Private Function buildNotifyNSTKReady() As Boolean

        Dim strSQLString As String
        Dim bolErrorR As Boolean

        ' get all stock lines that have been picked
        strSQLString = "SELECT A.BUSINESS_UNIT_OM, A.ORDER_NO, A.LINE_NBR," & vbCrLf & _
                " A.EMPLID, A.ISA_LINE_STATUS AS ISA_ORDER_STATUS," & vbCrLf & _
                " B.INV_ITEM_ID, B.QTY_SH_ACCPT AS QTY_LN_ACCPT," & vbCrLf & _
                " B.DESCR254_MIXED , D.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH" & vbCrLf & _
                " FROM PS_ISA_ORDSTAT_EML A," & vbCrLf & _
                " PS_RECV_LN_SHIP B," & vbCrLf & _
                " PS_ISA_USERS_TBL D" & vbCrLf & _
                " WHERE A.EMAIL_DATETIME Is NULL" & vbCrLf & _
                " AND A.ISA_LINE_STATUS = 'RET'" & vbCrLf & _
                " AND (SELECT DFL.BUSINESS_UNIT" & vbCrLf & _
                " FROM PS_REQ_LOADER_DFL DFL" & vbCrLf & _
                " WHERE DFL.LOADER_BU = A.BUSINESS_UNIT_OM" & vbCrLf & _
                " AND ROWNUM = '1') = B.BUSINESS_UNIT" & vbCrLf & _
                " AND A.RECEIVER_ID = B.RECEIVER_ID" & vbCrLf & _
                " AND A.RECV_LN_NBR = B.RECV_LN_NBR" & vbCrLf & _
                " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf & _
                " AND A.BUSINESS_UNIT_OM = 'I0256' " & vbCrLf & _
                " AND A.EMPLID = D.ISA_EMPLOYEE_ID" & vbCrLf & _
                " ORDER BY 1, 2, 3, 4, 5" & vbCrLf

        Dim Command = New OleDbCommand(strSQLString, connectOR)
        connectOR.Open()
        Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("  StatChg Email NSTK send PS_ISA_ORDSTAT_EML R error")
            connectOR.Close()
            buildNotifyNSTKReady = True
            bolErrorR = True
        End Try

        If bolErrorR = False And ds.Tables(0).Rows.Count > 0 Then
            connectOR.Open()
            Dim strPreOrderno As String
            Dim I As Integer
            Dim X As Integer

            Dim dsEmail As New DataTable
            Dim dr As DataRow
            dsEmail.Columns.Add("Order No.")
            dsEmail.Columns.Add("Line Nbr")
            dsEmail.Columns.Add("Description")
            dsEmail.Columns.Add("Qty")
            For I = 0 To ds.Tables(0).Rows.Count - 1

                dr = dsEmail.NewRow()
                dr.Item(0) = ds.Tables(0).Rows(I).Item("ORDER_NO")
                dr.Item(1) = ds.Tables(0).Rows(I).Item("LINE_NBR")
                dr.Item(2) = ds.Tables(0).Rows(I).Item("DESCR254_MIXED")
                dr.Item(3) = ds.Tables(0).Rows(I).Item("QTY_LN_ACCPT")
                dsEmail.Rows.Add(dr)
                'take this code down below so u can have multiple Order num's per email.

                If I = ds.Tables(0).Rows.Count - 1 Then
                    sendCustEmail(dsEmail, _
                        ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                        ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"), _
                        ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"), _
                        ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL"))

                    dsEmail.Clear()
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If
                    connectOR.Open()
                    buildNotifyNSTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"), _
                        ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                        ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS")) 

                ElseIf ds.Tables(0).Rows(I + 1).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I + 1).Item("ORDER_NO") <> _
                   ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I).Item("ORDER_NO") Then
                    sendCustEmail(dsEmail, _
                        ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                        ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"), _
                        ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"), _
                        ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL"))
                    dsEmail.Clear()
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If
                    connectOR.Open()
                    buildNotifyNSTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"), _
                        ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                        ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS")) 
                End If
                strPreOrderno = ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I).Item("ORDER_NO")
            Next
            objStreamWriter.WriteLine("  StatChg Email Build Notify Non STK. Total: " & ds.Tables(0).Rows.Count & " for I0256")
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try

        End If

    End Function

    Private Function buildNotifySTKReady() As Boolean

        Dim strSQLString As String
        Dim bolError7 As Boolean
        Dim bolErrorR As Boolean

        ' get all stock lines that have been picked
        strSQLString = "SELECT A.BUSINESS_UNIT_OM, A.ORDER_NO, A.LINE_NBR," & vbCrLf & _
                " A.EMPLID, A.ISA_LINE_STATUS AS ISA_ORDER_STATUS," & vbCrLf & _
                " B.INV_ITEM_ID, B.QTY_PICKED, B.QTY_BACKORDER," & vbCrLf & _
                " C.DESCR60, D.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH," & vbCrLf & _
                " C.INV_ITEM_ID" & vbCrLf & _
                " FROM PS_ISA_ORDSTAT_EML A," & vbCrLf & _
                " SYSADM8.PS_IN_DEMAND B," & vbCrLf & _
                " PS_MASTER_ITEM_TBL C," & vbCrLf & _
                " PS_ISA_USERS_TBL D" & vbCrLf & _
                " WHERE A.EMAIL_DATETIME Is NULL" & vbCrLf & _
                " AND A.ISA_LINE_STATUS = 'PKF'" & vbCrLf & _
                " AND B.DEMAND_SOURCE = 'OM'" & vbCrLf & _
                " AND B.SOURCE_BUS_UNIT = A.BUSINESS_UNIT_OM" & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO" & vbCrLf & _
                " AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO" & vbCrLf & _
                " AND C.SETID = 'MAIN1'" & vbCrLf & _
                " AND B.INV_ITEM_ID = C.INV_ITEM_ID" & vbCrLf & _
                " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf & _
                " AND A.BUSINESS_UNIT_OM = 'I0256'" & vbCrLf & _
                " AND A.EMPLID = D.ISA_EMPLOYEE_ID" & vbCrLf & _
                " AND B.IN_FULFILL_STATE IN ('60','50')" & vbCrLf & _
                " AND B.DEMAND_SOURCE = 'OM'" & vbCrLf & _
                " AND B.QTY_PICKED > 0" & vbCrLf

        Dim Command = New OleDbCommand(strSQLString, connectOR)
        connectOR.Open()
        Dim dataAdapter As OleDbDataAdapter = _
                    New OleDbDataAdapter(Command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("  StatChg Email send PS_ISA_ORDSTAT_EML 7 error")
            connectOR.Close()
            buildNotifySTKReady = True
            bolError7 = True
        End Try

        If bolError7 = False And ds.Tables(0).Rows.Count > 0 Then
            connectOR.Open()
            Dim strPreOrderno As String
            Dim I As Integer
            Dim X As Integer
            Dim decQtyOrdered As Decimal
            Dim decQtyShipped As Decimal

            Dim dsEmail As New DataTable
            Dim dr As DataRow
            dsEmail.Columns.Add("Item ID")
            dsEmail.Columns.Add("Description")
            dsEmail.Columns.Add("Picked Qty.")
            dsEmail.Columns.Add("Back Ordered")
            For I = 0 To ds.Tables(0).Rows.Count - 1

                dr = dsEmail.NewRow()
                dr.Item(0) = ds.Tables(0).Rows(I).Item("INV_ITEM_ID")
                dr.Item(1) = ds.Tables(0).Rows(I).Item("DESCR60")
                dr.Item(2) = ds.Tables(0).Rows(I).Item("QTY_PICKED")
                dr.Item(3) = ds.Tables(0).Rows(I).Item("QTY_BACKORDER")
                
                decQtyOrdered = getQtyOrdered(ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                                              ds.Tables(0).Rows(I).Item("LINE_NBR"), _
                                              connectOR)

                If (decQtyOrdered > 0) And _
                    decQtyOrdered <> (ds.Tables(0).Rows(I).Item("QTY_PICKED") + _
                                    ds.Tables(0).Rows(I).Item("QTY_BACKORDER")) Then
                    decQtyShipped = getQtyShipped(ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                                              ds.Tables(0).Rows(I).Item("LINE_NBR"), _
                                              connectOR)
                    If decQtyShipped > 0 Then
                        dr.Item(3) = Format((decQtyOrdered - decQtyShipped), "0.####")
                    End If

                End If
                dsEmail.Rows.Add(dr)
                If I = ds.Tables(0).Rows.Count - 1 Then
                    sendCustEmail(dsEmail, _
                        ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                        ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"), _
                        ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"), _
                        ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL"))
                    dsEmail.Clear()
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If
                    connectOR.Open()
                    buildNotifySTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"), _
                        ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                        ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS")) 

                ElseIf ds.Tables(0).Rows(I + 1).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I + 1).Item("ORDER_NO") <> _
                   ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I).Item("ORDER_NO") Then
                    sendCustEmail(dsEmail, _
                        ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                        ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"), _
                        ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"), _
                        ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL"))
                    dsEmail.Clear()
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If
                    connectOR.Open()
                    buildNotifySTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"), _
                        ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                        ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS")) 

                End If
                strPreOrderno = ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I).Item("ORDER_NO")
            Next
            objStreamWriter.WriteLine("  StatChg Email Build Notify STK. Total: " & ds.Tables(0).Rows.Count & " for I0256")
            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
        End If

    End Function

    Private Function getQtyOrdered(ByVal strOrderNo As String, _
                                ByVal intLineNbr As Integer, _
                                ByVal connectOR As OleDb.OleDbConnection) As Decimal

        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If
        connectOR.Open()
        Dim strSQLstring As String
        strSQLstring = "SELECT SUM( B.QTY_ORDERED)" & vbCrLf & _
                " FROM PS_ISA_ORD_INTF_LN A, PS_ORD_LINE B" & vbCrLf & _
                " WHERE A.ORDER_NO = '" & strOrderNo & "'" & vbCrLf & _
                " AND A.ISA_INTFC_LN = " & intLineNbr & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                " AND A.ISA_INTFC_LN = B.ORDER_INT_LINE_NO"

        Try
            getQtyOrdered = ORDBAccess.GetScalar(strSQLstring, connectOR)
        Catch ex As Exception
            getQtyOrdered = 0
        End Try

    End Function

    Private Function getQtyShipped(ByVal strOrderNo As String, _
                                ByVal intLineNbr As Integer, _
                                ByVal connectOR As OleDb.OleDbConnection) As Decimal

        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If
        connectOR.Open()
        Dim strSQLstring As String
        strSQLstring = "SELECT SUM( B.QTY_PICKED)" & vbCrLf & _
                " FROM PS_ISA_ORD_INTF_LN A, SYSADM8.PS_IN_DEMAND B" & vbCrLf & _
                " WHERE A.ORDER_NO = '" & strOrderNo & "'" & vbCrLf & _
                " AND A.ISA_INTFC_LN = " & intLineNbr & vbCrLf & _
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                " AND A.ISA_INTFC_LN = B.ORDER_INT_LINE_NO" & vbCrLf & _
                " AND B.IN_FULFILL_STATE IN ('60','50')" & vbCrLf & _
                " AND B.DEMAND_SOURCE = 'OM'" & vbCrLf & _
                " AND B.QTY_PICKED > 0" & vbCrLf & _
                ""
        Try
            getQtyShipped = ORDBAccess.GetScalar(strSQLstring, connectOR)
        Catch ex As Exception
            getQtyShipped = 0
        End Try

    End Function

    Private Sub sendCustEmail(ByVal dsEmail As DataTable, _
                        ByVal strOrderNo As String, _
                        ByVal strFirstName As String, _
                        ByVal strLastName As String, _
                        ByVal strEmail As String)

        Dim strbodyhead As String
        Dim strbodydetl As String
        Dim txtBody As String
        Dim txtHdr As String
        Dim txtMsg As String
        Dim dataGridHTML As String
        Dim SBnstk As New StringBuilder
        Dim SWnstk As New StringWriter(SBnstk)
        Dim htmlTWnstk As New HtmlTextWriter(SWnstk)
        Dim bolSelectItem As Boolean

        Dim Mailer As MailMessage = New MailMessage
        Dim strccfirst As String = "vitaly.rovensky"
        Dim strcclast As String = "sdi.com"
        Mailer.From = "SDIExchange@SDI.com"
        Mailer.Cc = ""
        Mailer.Bcc = strccfirst & "@" & strcclast
        strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
        strbodyhead = strbodyhead & "<center><span >SDiExchange - Order Status</span></center>"
        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

        Dim dtgEmail As WebControls.DataGrid
        dtgEmail = New WebControls.DataGrid

        dtgEmail.DataSource = dsEmail
        dtgEmail.DataBind()

        dtgEmail.CellPadding = 3
        'dtgEmail.Width.Percentage(90)

        'dtgPO.Columns(9).ItemStyle.HorizontalAlign = HorizontalAlign.Center
        dtgEmail.RenderControl(htmlTWnstk)
        dataGridHTML = SBnstk.ToString()

        Dim strPurchaserName As String = strFirstName & _
            " " & strLastName
        Dim strPurchaserEmail As String = strEmail
        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"
        strbodydetl = strbodydetl & "<p >Hello " & strPurchaserName & ",<br>"
        strbodydetl = strbodydetl & "&nbsp;<BR>"
        strbodydetl = strbodydetl & "Your SDiExchange Order Number " & strOrderNo & " has been processed and ready for pickup.<br>"
        strbodydetl = strbodydetl & "&nbsp;<BR>"
        strbodydetl = strbodydetl & "Order contents:<br>"
        strbodydetl = strbodydetl & "&nbsp;</p>"
        strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
        strbodydetl = strbodydetl + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        strbodydetl = strbodydetl & "</TABLE>" & vbCrLf

        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "Sincerely,<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "SDI Customer Care<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "</p>"
        strbodydetl = strbodydetl & "</div>"

        Mailer.Body = strbodyhead & strbodydetl
        If connectOR.DataSource.ToUpper = "RPTG" Or _
            connectOR.DataSource.ToUpper = "DEVL" Or _
            connectOR.DataSource.ToUpper = "STAR" Or _
            connectOR.DataSource.ToUpper = "PLGR" Then
            Mailer.To = "WEBDEV@sdi.com"
            Mailer.Subject = " <<TEST>> SDiExchange - Order Status " & strOrderNo & " is ready for pickup"
        Else
            Mailer.To = strPurchaserEmail
            Mailer.Subject = "SDiExchange - Order Status " & strOrderNo & " is ready for pickup"
        End If

        'Mailer.Subject = "SDiExchange - Order Status " & strOrderNo & " is ready for pickup"
        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
        
        'UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, Mailer.From, Mailer.To, "", "", "N", Mailer.Body, connectOR)

        SendLogger(Mailer.Subject, Mailer.Body, "UNCCSTTCHNGEML", "Mail", Mailer.To, "", "")
    End Sub

    Private Sub SendEmail()

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "vtaly.rovensky@sdi.com"

        'The subject of the email
        email.Subject = "UNCC StatChgEmailOut XML OUT Error"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<html><body><table><tr><td>UNCC_StatusChangeEmail has completed with errors, review log.</td></tr>"

        'email.Body = email.Body & "<tr><td></td><a href='\\BDougherty_XP-l\logs'>\\BDougherty_XP-l\logs\</a></tr></table></body></html>"

        'Send the email and handle any error that occurs
        Try

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "", "Y", email.Body, connectOR)

            SendLogger(email.Subject, email.Body, "UNCCSTTCHNGEML", "Mail", email.To, "", "")

        Catch
            objStreamWriter.WriteLine("     Error - the email was not sent")
        End Try

    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()

            SDIEmailService.EmailUtilityServices(MailType, "SDIExchange@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

    Private Sub Sendemail1(ByVal mailer As MailMessage)

        Try
            'SmtpMail.Send(mailer)
        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - in the sendemail to customer SUB")
        End Try
    End Sub

    Private Function updateSendEmailTbl(ByVal strBU As String, ByVal strOrderNo As String, ByVal strOrderStatus As String) As Boolean

        Dim strSQLstring As String
        Dim rowsaffected As Integer
        strSQLstring = "UPDATE PS_ISA_ORDSTAT_EML" & vbCrLf & _
                       " SET EMAIL_DATETIME = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                       " WHERE BUSINESS_UNIT_OM = 'I0256'" & vbCrLf & _
                       " AND ORDER_NO = '" & strOrderNo & "'" & vbCrLf & _
                       " AND ISA_LINE_STATUS = '" & strOrderStatus & "'"

        Dim Command1 As OleDbCommand
        Command1 = New OleDbCommand(strSQLstring, connectOR)
        Try
            rowsaffected = Command1.ExecuteNonQuery
            If rowsaffected = 0 Then
                objStreamWriter.WriteLine("**")
                objStreamWriter.WriteLine("     Error - 0 PS_ISA_ORDSTAT_EML tbl for order " & strOrderNo)
                objStreamWriter.WriteLine("**")
                updateSendEmailTbl = True
            End If
        Catch OleDBExp As OleDbException
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error - updating PS_ISA_ORDSTAT_EML tbl for order " & strOrderNo)
            objStreamWriter.WriteLine("**")
            updateSendEmailTbl = True
        End Try
        Command1.Dispose()
    End Function
    
    Private Function checkAllStatus_7(ByVal strBU As String, ByVal dtrStartDate As String) As Boolean
        Dim strSQLstring As String
        Dim dteEndDate As DateTime
        Dim format As New System.Globalization.CultureInfo("en-US", True)
        strSQLstring = "SELECT" & vbCrLf & _
            " to_char(MAX( A.DTTM_STAMP), 'MM/DD/YY HH24:MI:SS') as MAXDATE" & vbCrLf & _
            " FROM PS_ISAORDSTATUSLOG A" & vbCrLf & _
             " WHERE A.BUSINESS_UNIT_OM = 'I0256'" & vbCrLf

        Dim dr As OleDbDataReader = Nothing
        Try
            Dim command As OleDbCommand
            command = New OleDbCommand(strSQLstring, connectOR)
            connectOR.Open()

            dr = command.ExecuteReader
            Try

                If dr.Read Then
                    dteEndDate = (dr.Item("MAXDATE"))
                Else
                    dteEndDate = Now.ToString
                End If
            Catch ex As Exception
                dteEndDate = Now.ToString
            End Try

            dr.Close()
            connectOR.Close()

        Catch OleDBExp As OleDbException
            Try
                dr.Close()
                connectOR.Close()
            Catch exOR As Exception

            End Try
            objStreamWriter.WriteLine("     Error - error reading end date FROM PS_ISAORDSTATUSLOG A")
            Return True
        End Try

        connectOR.Open()
        Dim objEnterprise As New clsEnterprise("I0256", connectOR)
        Dim dteCustID As String = objEnterprise.CustID
        Dim dteCompanyID As String = objEnterprise.CompanyID
        Dim dteStartDate As DateTime = objEnterprise.SendStartDate
        Dim dteSiteEmail As String = objEnterprise.SiteEmail
        Dim dteSTKREQEmail As String = objEnterprise.STKREQEmail
        Dim dteNONSKREQEmail As String = objEnterprise.NONSKREQEmail

        'If strBU = "I0256" Then
        '    dteStartDate = dteStartDate.AddMinutes(-31)
        '    'dteStartDate = dteStartDate.AddHours(-15)
        'End If

        connectOR.Close()

        Dim ds As New DataSet
        Dim bolerror1 As Boolean


        dteEndDate.AddSeconds(1)

        ' stock items will get item id from the ps_isa_ord_intfc_l table  but description from the PS_MASTER_ITEM_TB
        ' non-stock items  has no item-id num and gets description from the ps_isa_ord_intfc_l
        ' PS_ISAORDSTATUSLOG the line number points to the line number in ps_isa_ord_intfc_l
        '         '  

        strSQLstring = "SELECT * FROM (SELECT A.BUSINESS_UNIT_OM, G.BUSINESS_UNIT_OM AS G_BUS_UNIT, D.BUSINESS_UNIT, D.ISA_EMPLOYEE_ID, A.ORDER_NO, B.ISA_INTFC_LN AS line_nbr," & vbCrLf & _
                 " B.ISA_EMPLOYEE_ID AS EMPLID, B.ISA_LINE_STATUS as ORDER_TYPE," & vbCrLf & _
                 " TO_CHAR(G.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM') as DTTM_STAMP, " & vbCrLf & _
                 " G.ISA_LINE_STATUS, DECODE(G.ISA_LINE_STATUS,'CRE','1','NEW','2','DSP','3','ORD','3','RSV','3','PKA','4','PKP','4','DLP','5','RCP','5','RCF','5','PKQ','5','DLO','5','DLF','6','PKF','7','CNC','C','QTS','Q','QTW','W','1') AS OLD_ORDER_STATUS," & vbCrLf & _
                 " (SELECT E.XLATLONGNAME" & vbCrLf & _
                                " FROM XLATTABLE E" & vbCrLf & _
                                " WHERE E.EFFDT =" & vbCrLf & _
                                " (SELECT MAX(E_ED.EFFDT) FROM XLATTABLE E_ED" & vbCrLf & _
                                " WHERE(E.FIELDNAME = E_ED.FIELDNAME)" & vbCrLf & _
                                " AND E.FIELDVALUE = E_ED.FIELDVALUE" & vbCrLf & _
                                " AND E_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                                " AND E.FIELDNAME = 'ISA_LINE_STATUS'" & vbCrLf & _
                                " AND E.FIELDVALUE = G.ISA_LINE_STATUS) as ORDER_STATUS_DESC, " & vbCrLf & _
                 " B.DESCR254 As NONSTOCK_DESCRIPTION, C.DESCR60 as STOCK_DESCRIPTION, D.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                 " B.INV_ITEM_ID as INV_ITEM_ID," & vbCrLf & _
                 " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH" & vbCrLf & _
                 " FROM ps_isa_ord_intf_HD A," & vbCrLf & _
                 " ps_isa_ord_intf_LN B," & vbCrLf & _
                 " PS_MASTER_ITEM_TBL C," & vbCrLf & _
                 " PS_ISA_USERS_TBL D," & vbCrLf & _
                 " PS_ISAORDSTATUSLOG G " & vbCrLf & _
                 " where G.BUSINESS_UNIT_OM = 'I0256'" & vbCrLf & _
                 " AND G.BUSINESS_UNIT_OM = A.BUSINESS_UNIT_OM " & vbCrLf & _
                 " AND G.BUSINESS_UNIT_OM = D.BUSINESS_UNIT " & vbCrLf & _
                 " and A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf & _
                 " and A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                 " and C.SETID (+) = 'MAIN1'" & vbCrLf & _
                 " and C.INV_ITEM_ID(+) = B.INV_ITEM_ID " & vbCrLf & _
                 " AND G.ORDER_NO = A.ORDER_NO " & vbCrLf & _
                 " AND B.ISA_INTFC_LN = G.ISA_INTFC_LN" & vbCrLf & _
                 " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf & _
                 " AND G.DTTM_STAMP > TO_DATE('" & dteStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                 " AND G.DTTM_STAMP <= TO_DATE('" & dteEndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                 " AND B.ISA_EMPLOYEE_ID = D.ISA_EMPLOYEE_ID) TBL, PS_ISA_USERS_PRIVS H " & vbCrLf & _
                 " WHERE TBL.G_BUS_UNIT = H.BUSINESS_UNIT " & vbCrLf & _
                 " AND H.BUSINESS_UNIT = TBL.BUSINESS_UNIT " & vbCrLf & _
                 " AND TBL.EMPLID = H.ISA_EMPLOYEE_ID " & vbCrLf & _
                 " AND SUBSTR(H.ISA_IOL_OP_NAME,10) = TBL.OLD_ORDER_STATUS " & vbCrLf & _
                 " AND H.ISA_IOL_OP_VALUE = 'Y' " & vbCrLf & _
                 " AND H.ISA_EMPLOYEE_ID = TBL.ISA_EMPLOYEE_ID" & vbCrLf
        ' the tenth byte of isa_iol_op_name has the one character g.isa_order_status code
        ' example: substr(emlsubmit1,10) = '1'   order status code 1

        Try
            ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)

        Catch OleDBExp As OleDbException
            connectOR.Close()
            objStreamWriter.WriteLine("     Error - error reading transaction FROM PS_ISAORDSTATUSLOG A")
            Return True
        End Try


        If IsDBNull(ds.Tables(0).Rows.Count) Or (ds.Tables(0).Rows.Count) = 0 Then
            objStreamWriter.WriteLine("     Warning - no status changes to process at this time for All statuses")
            Return False
        End If

        Dim rowsaffected As Integer
        Dim tmpOrderNo As String

        connectOR.Open()
        Dim strPreOrderno As String
        Dim I As Integer
        Dim X As Integer
        Dim dsEmail As New DataTable
        Dim dr1 As DataRow


        dsEmail.Columns.Add("Order No.")
        dsEmail.Columns.Add("Status")
        dsEmail.Columns.Add("Non-Stock Item Description")
        dsEmail.Columns.Add("Stock Item Description")
        dsEmail.Columns.Add("Item ID")
        dsEmail.Columns.Add("Line Nbr")
        dsEmail.Columns.Add("Time")


        For I = 0 To ds.Tables(0).Rows.Count - 1



            dr1 = dsEmail.NewRow()
            dr1.Item(0) = ds.Tables(0).Rows(I).Item("ORDER_NO")
            dr1.Item(1) = ds.Tables(0).Rows(I).Item("ORDER_STATUS_DESC")
            dr1.Item(2) = ds.Tables(0).Rows(I).Item("NONSTOCK_DESCRIPTION")
            dr1.Item(3) = ds.Tables(0).Rows(I).Item("STOCK_DESCRIPTION")
            dr1.Item(4) = ds.Tables(0).Rows(I).Item("INV_ITEM_ID")
            dr1.Item(5) = ds.Tables(0).Rows(I).Item("LINE_NBR")
            dr1.Item(6) = ds.Tables(0).Rows(I).Item("DTTM_STAMP")

            dsEmail.Rows.Add(dr1)

            ' "R" nonstock
            ' "7" stock
            Dim strEmail_test As String

            If I = ds.Tables(0).Rows.Count - 1 Then

                strEmail_test = ";vitaly.rovensky@sdi.com"
                sendCustEmail1(dsEmail, _
                ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                dteCompanyID, _
                dteCustID, _
                ds.Tables(0).Rows(I).Item("ORDER_TYPE"), _
                ds.Tables(0).Rows(I).Item("ORDER_STATUS_DESC"), _
                 ds.Tables(0).Rows(I).Item("INV_ITEM_ID"), _
                ds.Tables(0).Rows(I).Item("LINE_NBR"), _
                 ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"), _
                ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"), _
                ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL"))

                dsEmail.Clear()
                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If
            ElseIf ds.Tables(0).Rows(I + 1).Item("BUSINESS_UNIT_OM") _
                          & ds.Tables(0).Rows(I + 1).Item("ORDER_NO") <> _
                          ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                          & ds.Tables(0).Rows(I).Item("ORDER_NO") Then

                sendCustEmail1(dsEmail, _
               ds.Tables(0).Rows(I).Item("ORDER_NO"), _
               dteCompanyID, _
               dteCustID, _
               ds.Tables(0).Rows(I).Item("ORDER_TYPE"), _
               ds.Tables(0).Rows(I).Item("ORDER_STATUS_DESC"), _
               ds.Tables(0).Rows(I).Item("INV_ITEM_ID"), _
               ds.Tables(0).Rows(I).Item("LINE_NBR"), _
                ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"), _
                ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"), _
               ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL"))
                
                dsEmail.Clear()


                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If

            End If

        Next
        objStreamWriter.WriteLine("  StatChg Email send allstatus emails = " & ds.Tables(0).Rows.Count & " for I0256")


        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If

    End Function

    Private Sub sendCustEmail1(ByVal dsEmail As DataTable, _
                          ByVal strOrderNo As String, _
                          ByVal strCustID As String, _
                          ByVal strCompanyID As String, _
                          ByVal strOrderStatus As String, _
                          ByVal strOrderStatDesc As String, _
                          ByVal strInvID As String, _
                          ByVal strLineNbr As String, _
                          ByVal strFirstName As String, _
                          ByVal strLastName As String, _
                          ByVal strEmail As String)



        Dim strbodyhead1 As String
        Dim strbodydet1 As String
        Dim txtBody1 As String
        Dim txtHdr1 As String
        Dim txtMsg1 As String
        Dim dataGridHTML1 As String
        Dim SBnstk1 As New StringBuilder
        Dim SWnstk1 As New StringWriter(SBnstk1)
        Dim htmlTWnstk1 As New HtmlTextWriter(SWnstk1)
        Dim bolSelectItem1 As Boolean

        Dim Mailer1 As MailMessage = New MailMessage
        Dim strccfirst1 As String = "vitaly.rovensky"
        Dim strcclast1 As String = "sdi.com"
        Mailer1.From = "SDIExchange@SDI.com"
        Mailer1.Cc = ""
        Mailer1.Bcc = strccfirst1 & "@" & strcclast1
        strbodyhead1 = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
        strbodyhead1 = strbodyhead1 & "<center><span >SDiExchange - Order Status</span></center>"
        strbodyhead1 = strbodyhead1 & "&nbsp;" & vbCrLf

        Dim dtgEmail1 As WebControls.DataGrid
        dtgEmail1 = New WebControls.DataGrid

        dtgEmail1.DataSource = dsEmail
        dtgEmail1.DataBind()

        dtgEmail1.CellPadding = 3
        'dtgEmail1.Width.Percentage(90)

        'dtgPO.Columns(9).ItemStyle.HorizontalAlign = HorizontalAlign.Center
        dtgEmail1.RenderControl(htmlTWnstk1)
        dataGridHTML1 = SBnstk1.ToString()

        'Dim strPurchaserName As String = strCustID
        Dim strPurchaserName As String = strFirstName & _
           " " & strLastName

        Dim strPurchaserEmail As String = strEmail
        strbodydet1 = "&nbsp;" & vbCrLf
        strbodydet1 = strbodydet1 & "<div>"
        strbodydet1 = strbodydet1 & "<p >Hello " & strPurchaserName & ",<br>"
        'strbodydet1 = strbodydet1 & "&nbsp;<BR>"
        strbodydet1 = strbodydet1 & "Order Number: " & strOrderNo & " <br> "
        strbodydet1 = strbodydet1 & "Order contents:<br>"
        'strbodydet1 = strbodydet1 & "&nbsp;<BR>"
        ' strbodydet1 = strbodydet1 & "Order Status:  " & strOrderStatDesc & " <br>"
        'strbodydet1 = strbodydet1 & "Order Number:  " & strOrderNo & " <br>"
        ' strbodydet1 = strbodydet1 & "Line Number:  " & strLineNbr & " <br>"
        strbodydet1 = strbodydet1 & "&nbsp;</p>"
        strbodydet1 = strbodydet1 & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydet1 = strbodydet1 + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML1 & "</TD></TR>"
        strbodydet1 = strbodydet1 + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        strbodydet1 = strbodydet1 & "</TABLE>" & vbCrLf

        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "Sincerely,<br>"
        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "SDI Customer Care<br>"
        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "</p>"
        strbodydet1 = strbodydet1 & "</div>"

        Mailer1.Body = strbodyhead1 & strbodydet1
        If connectOR.DataSource.ToUpper = "RPTG" Or _
            connectOR.DataSource.ToUpper = "DEVL" Or _
            connectOR.DataSource.ToUpper = "STAR" Or _
            connectOR.DataSource.ToUpper = "PLGR" Then

            Mailer1.To = "webdev@sdi.com"
            Mailer1.Subject = " <<TEST>> SDiExchange - Order Status records for ORDER NUMBER: " & strOrderNo
        Else

            Mailer1.To = strEmail
            Mailer1.Subject = "SDiExchange - Order Status records for ORDER NUMBER: " & strOrderNo
        End If

        'Mailer1.Subject = "SDiExchange - Order Status records for ORDER NUMBER: " & strOrderNo
        Mailer1.BodyFormat = System.Web.Mail.MailFormat.Html
        
        'UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, Mailer.From, Mailer.To, "", "", "N", Mailer.Body, connectOR)

        SendLogger(Mailer1.Subject, Mailer1.Body, "UNCCSTTCHNGEML", "Mail", Mailer1.To, "", "")

    End Sub
    'Private Function updateEnterprise(ByVal strBU As String, ByVal dteEndDate As DateTime) As Boolean
    '    connectOR.Close()
    '    Dim strSQLstring As String
    '    'Dim dteEndDate As DateTime
    '    Dim ds As New DataSet
    '    Dim bolerror1 As Boolean
    '    Dim dteJulian As Integer
    '    Dim dteStart As Date = "01/01/1900"
    '    Dim rowsaffected As Integer
    '    dteJulian = DateDiff(DateInterval.Day, dteStart, Now())

    '    Dim strXMLPath As String = rootDir & "\XMLOUT\ORDERSTATUS" & Convert.ToString(dteJulian) & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & ".xml"
    '    Dim objXMLWriter As XmlTextWriter


    '    ' The enddate coming from PS_ISAORDERSTATUSLOG  is being set back to the original enddate.  The PS_ISA_enterprise table
    '    ' is then updated with the PS_ISAORDERSTATUSLOG's endddate and the next time in, the date in the PS_ISA_enterprise table is
    '    ' the startdate.  We increased the enddate a second so we could get all the records from the query.  We were never getting
    '    ' the last record because of milliseconds were off in the date conversions.  Adding a second we were able to get all
    '    ' the records in the date range....  If you understand this you have a date to sit with the Dali Lama.. Believe me
    '    ' it works!!!!!!!!  PFD 4.4.2008
    '    ' reset the dteEndDate back to original

    '    dteEndDate.AddSeconds(-1)

    '    strSQLstring = "UPDATE PS_ISA_ENTERPRISE" & vbCrLf & _
    '                " SET ISA_LAST_STAT_SEND = TO_DATE('" & dteEndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
    '                " WHERE ISA_BUSINESS_UNIT = '" & strBU & "' "



    '    Try
    '        Dim Command = New OleDbCommand(strSQLstring, connectOR)
    '        connectOR.Open()
    '        rowsaffected = Command.ExecuteNonQuery()
    '        connectOR.Close()
    '    Catch OleDBExp As OleDbException
    '        Console.WriteLine("")
    '        Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
    '        Console.WriteLine("")
    '        connectOR.Close()
    '        objStreamWriter.WriteLine("  Error - updating the Enterprise send date " & OleDBExp.ToString)
    '        bolerror1 = True
    '        objXMLWriter.WriteEndElement()
    '        objXMLWriter.Flush()
    '        objXMLWriter.Close()
    '        Dim strXMLResult As String
    '        Dim objSR As StreamReader = File.OpenText(strXMLPath)
    '        strXMLResult = objSR.ReadToEnd()
    '        objSR.Close()
    '        objSR = Nothing
    '    End Try


    '    If bolerror1 = True Then
    '        Return True
    '    Else
    '        Return False
    '    End If
    'End Function
End Module
