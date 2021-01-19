Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
Imports System.Configuration
Imports System.Linq
Imports System.Collections.Generic
Imports System.Drawing.Color
Imports System.Net
Imports Newtonsoft.Json
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading.Tasks
Imports System.Web.Script.Serialization

Module Module1

    Dim objStreamWriter As StreamWriter
    Dim objGenerallLogStreamWriter As StreamWriter
    Dim objWalmartSC As StreamWriter
    Dim rootDir As String = "C:\StatChg"
    Dim logpath As String = "C:\StatChg\LOGS\StatChgEmailOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim Generallogpath As String = "C:\StatChg\LOGS\GeneralStatChgEmailOutLog" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim WalmartSC As String = "C:\StatChg\LOGS\WalmartSC" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString")))

    Sub Main()

        Console.WriteLine("Start StatChg Email send")
        Console.WriteLine("")

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("  Send emails out " & Now())

        objGenerallLogStreamWriter = File.CreateText(Generallogpath)
        objGenerallLogStreamWriter.WriteLine("Started Writing the Logs " & Now())

        objWalmartSC = File.CreateText(WalmartSC)
        objWalmartSC.WriteLine("Started Walmart Service Channel " & Now())

        Dim bolError As Boolean = buildstatchgout()

        If bolError = True Then
            SendEmail()
        End If


        objStreamWriter.WriteLine("  End of StatChg Email send " & Now())
        objGenerallLogStreamWriter.WriteLine("Ends " & Now())
        objWalmartSC.WriteLine("Ends " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()
        objGenerallLogStreamWriter.Flush()
        objGenerallLogStreamWriter.Close()

        objWalmartSC.Flush()
        objWalmartSC.Close()

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
        Dim jj As XmlNode = xmldata.ChildNodes(2)
        jobNode = xmldata.ChildNodes(1)
        Dim dsRows As New DataSet
        dsRows.ReadXml(New XmlNodeReader(jobNode))

        Dim I As Integer
        Dim bolErrorSomeWhere As Boolean

        Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
        connectOR = New OleDbConnection(connectionString)


        ' check stock
        For I = 0 To dsRows.Tables(0).Rows.Count - 1
            If dsRows.Tables(0).Rows(I).Item("SITESTK") = "Y" Then
                Console.WriteLine(Convert.ToString(I + 1) + ".Verifying/Updating StatChg Email for Stock - BU: " + Convert.ToString(dsRows.Tables(0).Rows(I).Item("SITEBU")) + "")
                objGenerallLogStreamWriter.WriteLine(Convert.ToString(I + 1) + ".Verifying/Updating StatChg Email for Stock - BU: " + Convert.ToString(dsRows.Tables(0).Rows(I).Item("SITEBU")) + "")
                objStreamWriter.WriteLine("--------------------------------------------------------------------------------------")
                objStreamWriter.WriteLine("  StatChg Email send stock emails for " & dsRows.Tables(0).Rows(I).Item("SITEBU"))
                buildstatchgout = checkStock(dsRows.Tables(0).Rows(I).Item("SITEBU"), dsRows.Tables(0).Rows(I).Item("SITESTART"))
                'buildstatchgout = False
                If buildstatchgout = True Then
                    bolErrorSomeWhere = True
                End If
            End If
        Next

        ' check non-stock
        For I = 0 To dsRows.Tables(0).Rows.Count - 1
            If dsRows.Tables(0).Rows(I).Item("SITENSTK") = "Y" Then
                Console.WriteLine(Convert.ToString(I + 1) + ".Verifying/Updating StatChg Email for Non-Stock - BU: " + Convert.ToString(dsRows.Tables(0).Rows(I).Item("SITEBU")) + "")
                objGenerallLogStreamWriter.WriteLine(Convert.ToString(I + 1) + ".Verifying/Updating StatChg Email for Non-Stock - BU: " + Convert.ToString(dsRows.Tables(0).Rows(I).Item("SITEBU")) + "")
                objStreamWriter.WriteLine("--------------------------------------------------------------------------------------")
                objStreamWriter.WriteLine("  StatChg Email send nonstock emails for " & dsRows.Tables(0).Rows(I).Item("SITEBU"))
                buildstatchgout = checkNonStock(dsRows.Tables(0).Rows(I).Item("SITEBU"), dsRows.Tables(0).Rows(I).Item("SITESTART"))
                If buildstatchgout = True Then
                    bolErrorSomeWhere = True
                End If
            End If
        Next


        For I = 0 To dsRows.Tables(0).Rows.Count - 1
            Console.WriteLine(Convert.ToString(I + 1) + ".Order Status Email Completed for BU: " + Convert.ToString(dsRows.Tables(0).Rows(I).Item("SITEBU")) + "")
            objGenerallLogStreamWriter.WriteLine(Convert.ToString(I + 1) + ".Order Status Email Completed for BU: " + Convert.ToString(dsRows.Tables(0).Rows(I).Item("SITEBU")) + "")
            objStreamWriter.WriteLine("--------------------------------------------------------------------------------------")
            objStreamWriter.WriteLine("  StatChg Email send allstatus emails for XML Site : " & dsRows.Tables(0).Rows(I).Item("SITEBU"))
            buildstatchgout = checkAllStatus_7(dsRows.Tables(0).Rows(I).Item("SITEBU"), dsRows.Tables(0).Rows(I).Item("SITESTART"))
            If buildstatchgout = True Then
                bolErrorSomeWhere = True
            End If
        Next


        '' Blocked the email sending for the checkStock,checkNonStock to all sites.It's only for the XML Sites
        Dim dsBU As DataSet
        dsBU = GetBU()

        If Not dsBU Is Nothing Then
            objGenerallLogStreamWriter.WriteLine("Total BU going to Process " + Convert.ToString(dsBU.Tables(0).Rows.Count()))

            Console.WriteLine("----------------------------------- New Profile Status Emails ------------------------------------------------------------")
            objGenerallLogStreamWriter.WriteLine("-------------------------------------------------------------------------------")
            For I = 0 To dsBU.Tables(0).Rows.Count - 1
                Console.WriteLine(Convert.ToString(I + 1) + ".Order Status Email Completed for BU: " + Convert.ToString(dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT")) + "")
                objGenerallLogStreamWriter.WriteLine(Convert.ToString(I + 1) + ".Order Status Email Completed for BU: " + Convert.ToString(dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT")) + "")
                objStreamWriter.WriteLine("--------------------------------------------------------------------------------------")
                objStreamWriter.WriteLine("  StatChg Email send allstatus emails for Enterprise BU : " & dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT"))
                buildstatchgout = checkAllStatusNew(dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT"))
                If buildstatchgout = True Then
                    bolErrorSomeWhere = True
                End If
            Next
        Else

        End If
        '7 is stock
        'R is non-stock
        objGenerallLogStreamWriter.WriteLine("-------------------------------------------------------------------------------")
        bolErrorSomeWhere = buildNotifyReceiver("7")
        objGenerallLogStreamWriter.WriteLine("-------------------------------------------------------------------------------")
        bolErrorSomeWhere = buildNotifyReceiver("R")

        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        Return bolErrorSomeWhere

    End Function

    Private Function GetBU() As DataSet
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            '' To get teh list of BU if the privilage was set to any site
            Dim getBuQuery As String = "SELECT DISTINCT(E.ISA_BUSINESS_UNIT) AS BUSINESS_UNIT from PS_ISA_USERS_PRIVS P,PS_ISA_ENTERPRISE E  where E.ISA_BUSINESS_UNIT = P.BUSINESS_UNIT AND " & vbCrLf &
                "P.ISA_IOL_OP_NAME in ('EMAILCRE01','EMAILQTW02','EMAILQTC03','EMAILQTS04','EMAILCST05','EMAILVND06','EMAILAPR07','EMAILQTA08'," & vbCrLf &
                "'EMAILQTR09','EMAILRFA10','EMAILRFR11','EMAILRFC12','EMAILRCF13','EMAILRCP14','EMAILCNC15','EMAILDLF16')"

            Dim Command As OleDbCommand = New OleDbCommand(getBuQuery, connectOR)
            If connectOR.State = ConnectionState.Open Then
                'do nothing
            Else
                connectOR.Open()
            End If

            Dim dataAdapter As OleDbDataAdapter =
                        New OleDbDataAdapter(Command)
            Try
                dataAdapter.Fill(ds)
                connectOR.Close()
            Catch ex As Exception

            End Try
            If Not ds Is Nothing Then
                If ds.Tables(0).Rows.Count() > 0 Then
                    Return ds
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Function checkNonStock(ByVal strBU As String, ByVal dtrStartDate As String) As Boolean

        Dim dteStrDate As DateTime
        Dim dteEndDate As DateTime

        dteStrDate = Now.AddDays(-1).ToString("MM/dd/yyyy")
        'dteStrDate = Today.AddHours(-12)
        'dteEndDate = Now
        'we could run this twice a day noon and midnight.

        Dim strSiteBU As String
        Dim strSQLstring As String
        Dim Command As OleDbCommand

        strSQLstring = "SELECT /*+ USE_NL(A B C D E) */ A.ORDER_NO, B.ISA_INTFC_LN AS LINE_NBR, E.RECEIVER_ID," & vbCrLf &
                " E.RECV_LN_NBR, A.BUSINESS_UNIT_OM, B.ISA_EMPLOYEE_ID AS EMPLID, E.DESCR254_MIXED, A.Origin" & vbCrLf &
                " FROM PS_ISA_ORD_INTF_HD A, PS_ISA_ORD_INTF_LN B," & vbCrLf &
                " PS_ISA_USERS_TBL C, PS_PO_LINE_DISTRIB D," & vbCrLf &
                " PS_RECV_LN_SHIP E" & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf &
                " AND A.ADD_DTTM > TO_DATE('" & dteStrDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                " AND A.ADD_DTTM > TO_DATE('" & dtrStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                " AND A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                " AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf &
                " AND upper(B.ISA_EMPLOYEE_ID) = upper(C.ISA_EMPLOYEE_ID)" & vbCrLf &
                " AND A.ORDER_NO = D.REQ_ID" & vbCrLf &
                " AND B.ISA_INTFC_LN = D.REQ_LINE_NBR" & vbCrLf &
                " AND D.BUSINESS_UNIT = E.BUSINESS_UNIT" & vbCrLf &
                " AND D.PO_ID = E.PO_ID" & vbCrLf &
                " AND D.LINE_NBR = E.LINE_NBR" & vbCrLf &
                " AND E.QTY_SH_ACCPT > 0" & vbCrLf &
                " AND NOT EXISTS (SELECT 'X'" & vbCrLf &
                " FROM PS_RTV_LN F" & vbCrLf &
                " WHERE F.BUSINESS_UNIT_PO = D.BUSINESS_UNIT" & vbCrLf &
                " AND F.PO_ID = D.PO_ID" & vbCrLf &
                " AND F.LINE_NBR = D.LINE_NBR" & vbCrLf &
                " AND F.QTY_LN_RETRN_SUOM = E.QTY_SH_ACCPT" & vbCrLf &
                " AND F.RETURN_REASON = 'MTX')" & vbCrLf &
                " AND NOT EXISTS (SELECT /*+ USE_NL(A G) */ 'X'" & vbCrLf &
                " FROM PS_ISA_ORDSTAT_EML G" & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = G.BUSINESS_UNIT_OM" & vbCrLf &
                " AND A.ORDER_NO = G.ORDER_NO" & vbCrLf &
                " AND B.ISA_INTFC_LN = G.LINE_NBR" & vbCrLf &
                " AND E.RECEIVER_ID = G.RECEIVER_ID" & vbCrLf &
                " AND E.RECV_LN_NBR = G.RECV_LN_NBR" & vbCrLf &
                " AND G.ISA_LINE_STATUS = 'RET')" & vbCrLf &
                " ORDER BY ORDER_NO, LINE_NBR, RECEIVER_ID, RECV_LN_NBR"

        objStreamWriter.WriteLine("  CheckNonStock (2): " & strSQLstring)

        Command = New OleDbCommand(strSQLstring, connectOR)
        If connectOR.State = ConnectionState.Open Then
            'do nothing
        Else
            connectOR.Open()
        End If
        Dim dataAdapter As OleDbDataAdapter =
                    New OleDbDataAdapter(Command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("  StatChg Email NSTK send select orders for " & strBU)
            connectOR.Close()
            Return True
        End Try
        ' don't process UNCC status change emails - done in another program UNCCSTATUSCHANGEEMAIL
        If ds.Tables(0).Rows.Count = 0 Or strBU = "I0256" Then
            Console.WriteLine("Fetched Datas:0")
            objGenerallLogStreamWriter.WriteLine("Fetched Datas:0")
            objStreamWriter.WriteLine("  StatChg Email NSTK send select orders = 0 for" & strBU)
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            Return False
        Else
            Console.WriteLine("Fetched Datas:" + Convert.ToString(ds.Tables(0).Rows.Count()))
            objGenerallLogStreamWriter.WriteLine("Fetched Datas:" + Convert.ToString(ds.Tables(0).Rows.Count()))
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
            Dim strorigin As String = ds.Tables(0).Rows(I).Item("Origin")

            Dim ChkExistsLog As Boolean = ChkExistnLog(strBUSINESSUNITOM, strORDERNO, strINTFCLINENUM, "0", "0", strEMPLID, "RET")
            connectOR.Open()

            If Not ChkExistsLog Then
                strSQLstring = "INSERT INTO PS_ISA_ORDSTAT_EML" & vbCrLf &
                        " VALUES ('" & strBUSINESSUNITOM & "'," & vbCrLf &
                        " '" & strORDERNO & "'," & vbCrLf &
                        " '" & strINTFCLINENUM & "'," & vbCrLf &
                        " '0'," & vbCrLf &
                        " '0'," & vbCrLf &
                        " '" & strRECEIVERID & "'," & vbCrLf &
                        " '" & strRECVLNNBR & "'," & vbCrLf &
                        " '" & strEMPLID & "'," & vbCrLf &
                        " 'RET', '')" & vbCrLf


                Dim command1 As OleDbCommand
                command1 = New OleDbCommand(strSQLstring, connectOR)
                Try
                    Dim rowsaffected As Integer
                    rowsaffected = command1.ExecuteNonQuery
                    If Not rowsaffected = 1 Then
                        objStreamWriter.WriteLine("  StatChg Email NSTK send insert orders for " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " &
                         ds.Tables(0).Rows(I).Item("LINE_NBR") & " " &
                                            ds.Tables(0).Rows(I).Item("RECEIVER_ID") & " " &
                                            ds.Tables(0).Rows(I).Item("RECV_LN_NBR"))
                        checkNonStock = True
                    End If
                    command1.Dispose()
                Catch ex As Exception
                    objStreamWriter.WriteLine("  StatChg Email NSTK send insert error for " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " &
                        ds.Tables(0).Rows(I).Item("LINE_NBR") & " " &
                        ds.Tables(0).Rows(I).Item("RECEIVER_ID") & " " &
                        ds.Tables(0).Rows(I).Item("RECV_LN_NBR"))
                    objStreamWriter.WriteLine(ex.ToString)
                    checkNonStock = True
                End Try
            Else
                objStreamWriter.WriteLine("  StatChg Email NSTK send insert already exists in PS_ISA_ORDSTAT_EML table for order no " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " &
                        ds.Tables(0).Rows(I).Item("LINE_NBR") & " " &
                        ds.Tables(0).Rows(I).Item("RECEIVER_ID") & " " &
                        ds.Tables(0).Rows(I).Item("RECV_LN_NBR"))
                checkNonStock = True
            End If
        Next
        objStreamWriter.WriteLine("  StatChg Email NSTK send select orders = " & ds.Tables(0).Rows.Count & " for" & strBU)

        Try
            connectOR.Close()
        Catch ex As Exception

        End Try


    End Function

    Private Function checkStock(ByVal strbu As String, ByVal dtrStartDate As String) As Boolean
        ' the union all in the sql below - diferences between top and bot
        ' bot " AND E.CONFIRMED_FLAG = 'Y'" & vbCrLf & _
        ' 
        Dim dteStrDate As DateTime
        dteStrDate = Now.AddDays(-1).ToString("MM/dd/yyyy")
        Dim strSQLstring As String
        strSQLstring = "SELECT /*+ index(D,PSWIN_DEMAND) */ B.ORDER_NO, B.ISA_INTFC_LN AS INTFC_LINE_NUM, B.ISA_INTFC_LN AS ORDER_INT_LINE_NO, D.DEMAND_LINE_NO," & vbCrLf &
                " B.BUSINESS_UNIT_OM, B.ISA_EMPLOYEE_ID AS EMPLID, E.DESCR60, E.INV_ITEM_ID, A.Origin" & vbCrLf &
                " FROM PS_ISA_ORD_INTF_HD A, PS_ISA_ORD_INTF_LN B," & vbCrLf &
                " PS_ISA_USERS_TBL C, SYSADM8.PS_IN_DEMAND D," & vbCrLf &
                " PS_MASTER_ITEM_TBL E" & vbCrLf &
                " WHERE A.BUSINESS_UNIT_OM = '" & strbu & "'" & vbCrLf &
                " AND A.ADD_DTTM > TO_DATE('" & dteStrDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                " AND A.ADD_DTTM > TO_DATE('" & dtrStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                " AND A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                " AND B.INV_ITEM_ID <> ' '" & vbCrLf &
                " AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT" & vbCrLf &
                " AND UPPER(B.ISA_EMPLOYEE_ID) =UPPER(C.ISA_EMPLOYEE_ID)" & vbCrLf &
                " AND B.ISA_INTFC_LN = D.ORDER_INT_LINE_NO" & vbCrLf &
                " AND B.ORDER_NO = D.ORDER_NO" & vbCrLf &
                " AND E.SETID = 'MAIN1'" & vbCrLf &
                " AND D.INV_ITEM_ID = E.INV_ITEM_ID" & vbCrLf &
                " AND D.IN_FULFILL_STATE IN ('60','50')" & vbCrLf &
                " AND D.DEMAND_SOURCE = 'OM'" & vbCrLf &
                " AND D.QTY_PICKED > 0" & vbCrLf &
                " AND NOT EXISTS (SELECT 'X'" & vbCrLf &
                " FROM PS_ISA_ORDSTAT_EML F" & vbCrLf &
                " WHERE F.BUSINESS_UNIT_OM = D.SOURCE_BUS_UNIT" & vbCrLf &
                " AND F.ORDER_NO = D.ORDER_NO" & vbCrLf &
                " AND F.DEMAND_LINE_NO = D.DEMAND_LINE_NO" & vbCrLf &
                " AND F.ORDER_INT_LINE_NO = D.ORDER_INT_LINE_NO" & vbCrLf &
                " AND F.ISA_LINE_STATUS IN ('PKP', 'PKF'))" & vbCrLf &
                " ORDER BY ORDER_NO, INTFC_LINE_NUM, DEMAND_LINE_NO"

        objStreamWriter.WriteLine("  CheckStock: " & strSQLstring)

        Dim Command As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)
        If connectOR.State = ConnectionState.Open Then
            'do nothing
        Else
            connectOR.Open()
        End If

        Dim dataAdapter As OleDbDataAdapter =
                    New OleDbDataAdapter(Command)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("Error in StatChg Email send select orders for " & strbu)
            connectOR.Close()
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            Console.WriteLine("Fetched Datas:0")
            objGenerallLogStreamWriter.WriteLine("Fetched Datas:0")
            objStreamWriter.WriteLine("  StatChg Email send select orders = 0 for" & strbu)
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try

            Return False
        Else
            Console.WriteLine("Fetched Datas:" + Convert.ToString(ds.Tables(0).Rows.Count()))
            objGenerallLogStreamWriter.WriteLine("Fetched Datas:" + Convert.ToString(ds.Tables(0).Rows.Count()))
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
            Dim strorigin As String = ds.Tables(0).Rows(I).Item("Origin")

            Dim ChkExistsLog As Boolean = ChkExistnLog(strBUSINESSUNITOM, strORDERNO, strINTFCLINENUM, strORDERINTLINENO, strDEMANDLINENO, strEMPLID, "7")
            connectOR.Open()
            If Not ChkExistsLog Then
                strSQLstring = "INSERT INTO PS_ISA_ORDSTAT_EML" & vbCrLf &
                        " VALUES ('" & strBUSINESSUNITOM & "'," & vbCrLf &
                        " '" & strORDERNO & "'," & vbCrLf &
                        " '" & strINTFCLINENUM & "'," & vbCrLf &
                        " '" & strORDERINTLINENO & "'," & vbCrLf &
                        " '" & strDEMANDLINENO & "'," & vbCrLf &
                        " ' ',0," & vbCrLf &
                        " '" & strEMPLID & "'," & vbCrLf &
                         " '7', '')" & vbCrLf

                Dim command1 As OleDbCommand
                command1 = New OleDbCommand(strSQLstring, connectOR)
                Try
                    Dim rowsaffected As Integer
                    rowsaffected = command1.ExecuteNonQuery
                    If Not rowsaffected = 1 Then
                        objStreamWriter.WriteLine("  StatChg Email send insert error for " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " &
                        ds.Tables(0).Rows(I).Item("INTFC_LINE_NUM") & " " &
                        ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO") & " " &
                        ds.Tables(0).Rows(I).Item("DEMAND_LINE_NO"))
                        checkStock = True
                    End If
                    command1.Dispose()
                Catch ex As Exception
                    objStreamWriter.WriteLine("  StatChg Email send insert orders for " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " &
                        ds.Tables(0).Rows(I).Item("INTFC_LINE_NUM") & " " &
                        ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO") & " " &
                        ds.Tables(0).Rows(I).Item("DEMAND_LINE_NO"))
                    checkStock = True
                End Try
            Else
                objStreamWriter.WriteLine("  StatChg Email send insert already exists in PS_ISA_ORDSTAT_EML table for order no " & ds.Tables(0).Rows(I).Item("ORDER_NO") & " " &
                        ds.Tables(0).Rows(I).Item("INTFC_LINE_NUM") & " " &
                        ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO") & " " &
                        ds.Tables(0).Rows(I).Item("DEMAND_LINE_NO"))
                checkStock = True
            End If
        Next

        objStreamWriter.WriteLine("  StatChg Email STK send select orders = " & ds.Tables(0).Rows.Count & " for" & strbu)
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

    End Function

    Private Function ChkExistnLog(ByVal strbu As String, ByVal strordno As String, ByVal ln_no As String, ByVal ordintln As String, ByVal dmnd_ln As String, ByVal strEmpId As String,
                                  ByVal ln_status As String) As Boolean
        Dim strquery As String = "SELECT * FROM PS_ISA_ORDSTAT_EML WHERE business_unit_om = '" + strbu + "' AND ORDER_NO='" + strordno + "' AND LINE_NBR = " + ln_no + " AND ORDER_INT_LINE_NO = " + ordintln + " AND DEMAND_LINE_NO = " + dmnd_ln + " AND EMPLID='" + strEmpId + "' AND ISA_LINE_STATUS = '" + ln_status + "'"
        Dim ChkExists As Boolean = False
        Try
            'If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            '    connectOR.Close()
            'End If

            Dim dSet As New DataSet
            dSet = ORDBAccess.GetAdapter(strquery, connectOR)

            If Not dSet Is Nothing Then
                If dSet.Tables(0).Rows.Count > 0 Then
                    ChkExists = True
                Else

                End If
            Else

            End If
            'If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            '    connectOR.Close()
            'End If
        Catch ex As Exception

        End Try
        Return ChkExists
    End Function

    Private Function buildNotifyReceiver(ByVal strOrderStatus As String) As Boolean

        Select Case strOrderStatus
            Case "7"
                objGenerallLogStreamWriter.WriteLine("Stock Notification StatEmail")
                buildNotifyReceiver = buildNotifySTKReady()
            Case "R"
                objGenerallLogStreamWriter.WriteLine("Non-Stock Notification StatEmail")
                buildNotifyReceiver = buildNotifyNSTKReady()
        End Select
    End Function

    Private Function buildNotifyNSTKReady() As Boolean

        Dim strSQLString As String
        Dim bolErrorR As Boolean

        ' get all stock lines that have been picked
        strSQLString = "SELECT A.BUSINESS_UNIT_OM, A.ORDER_NO, L.ISA_WORK_ORDER_NO as WORK_ORDER_NO, A.LINE_NBR," & vbCrLf &
                " A.EMPLID, A.ISA_LINE_STATUS AS ISA_ORDER_STATUS," & vbCrLf &
                " B.INV_ITEM_ID, B.QTY_SH_ACCPT AS QTY_LN_ACCPT," & vbCrLf &
                " B.DESCR254_MIXED , D.ISA_EMPLOYEE_EMAIL," & vbCrLf &
                " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH, B.PO_ID, H.Origin " & vbCrLf &
                " FROM PS_ISA_ORDSTAT_EML A," & vbCrLf &
                " PS_RECV_LN_SHIP B," & vbCrLf &
                " PS_ISA_USERS_TBL D," & vbCrLf &
                " ps_isa_ord_intf_hD H ," & vbCrLf &
                " ps_isa_ord_intf_lN L" & vbCrLf &
                " WHERE A.EMAIL_DATETIME Is NULL " & vbCrLf &
                " and H.order_no=A.order_no " & vbCrLf &
                " and H.business_unit_om=A.BUSINESS_UNIT_OM " & vbCrLf &
                " and H.business_unit_om=D.BUSINESS_UNIT " & vbCrLf &
                " and H.business_unit_om = L.business_unit_om " & vbCrLf &
                " and H.order_no = L.order_no " & vbCrLf &
                " AND A.ISA_LINE_STATUS = 'RET'" & vbCrLf &
                " AND A.RECEIVER_ID = B.RECEIVER_ID" & vbCrLf &
                " AND A.RECV_LN_NBR = B.RECV_LN_NBR" & vbCrLf &
                " and L.ISA_INTFC_LN = B.RECV_LN_NBR" & vbCrLf &
                " and L.ISA_INTFC_LN = A.RECV_LN_NBR" & vbCrLf &
                " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf &
                " AND UPPER(A.EMPLID) = UPPER(D.ISA_EMPLOYEE_ID)" & vbCrLf &
                " "

        Dim Command = New OleDbCommand(strSQLString, connectOR)
        If connectOR.State = ConnectionState.Open Then
            'do nothing
        Else
            connectOR.Open()
        End If
        Dim dataAdapter As OleDbDataAdapter =
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
            Console.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
            objGenerallLogStreamWriter.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
            connectOR.Open()
            Dim strPreOrderno As String
            Dim I As Integer
            Dim X As Integer

            Dim dsEmail As New DataTable
            Dim dr As DataRow
            dsEmail.Columns.Add("Order No.")
            dsEmail.Columns.Add("Line Number")
            dsEmail.Columns.Add("Description")
            dsEmail.Columns.Add("Qty")
            dsEmail.Columns.Add("Work Order Number")
            dsEmail.Columns.Add("P.O. #: ")
            dsEmail.Columns.Add("Line Notes")
            For I = 0 To ds.Tables(0).Rows.Count - 1

                dr = dsEmail.NewRow()
                dr.Item(0) = ds.Tables(0).Rows(I).Item("ORDER_NO")
                dr.Item(1) = ds.Tables(0).Rows(I).Item("LINE_NBR")
                dr.Item(2) = ds.Tables(0).Rows(I).Item("DESCR254_MIXED")
                dr.Item(3) = ds.Tables(0).Rows(I).Item("QTY_LN_ACCPT")
                dr.Item(4) = ds.Tables(0).Rows(I).Item("WORK_ORDER_NO")
                dr.Item(5) = ds.Tables(0).Rows(I).Item("PO_ID")
                'WORK_ORDER_NO

                Dim ln_notes As String = ""
                ln_notes = GetLineNotes(Convert.ToString(ds.Tables(0).Rows(I).Item("ORDER_NO")), Convert.ToString(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM")), Convert.ToString(ds.Tables(0).Rows(I).Item("LINE_NBR")))
                dr.Item(6) = ln_notes
                connectOR.Open()
                dsEmail.Rows.Add(dr)
                'take this code down below so u can have multiple Order num's per email.
                Dim strEmail_test As String = ";tom.rapp@sdi.com"

                Dim strEmailTo As String = ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL")
                Dim strOrderNo As String = ds.Tables(0).Rows(I).Item("ORDER_NO")
                Dim strBu As String = ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM")

                '' check is processed order is ASCEND order
                'Dim bIsAscend As Boolean = False
                'If Trim(strBu) <> "" Then
                '    bIsAscend = IsBuAscend(strBu)
                'End If

                'If bIsAscend Then
                '    Dim strAscendEmail As String = GetAscendEmailAddress(strBu, strOrderNo, connectOR)
                '    If Not strAscendEmail Is Nothing Then
                '        If Trim(strAscendEmail) <> "" Then
                '            strEmailTo = strAscendEmail
                '        End If
                '    End If

                'End If

                If I = ds.Tables(0).Rows.Count - 1 Then
                    sendCustEmail(dsEmail,
                        strOrderNo,
                        ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"),
                        ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"),
                        strEmailTo,
                        strBu, ds.Tables(0).Rows(I).Item("Origin"))

                    dsEmail.Clear()
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If
                    connectOR.Open()
                    buildNotifyNSTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"),
                        ds.Tables(0).Rows(I).Item("ORDER_NO"),
                        ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"), ds.Tables(0).Rows(I).Item("Origin"))

                ElseIf ds.Tables(0).Rows(I + 1).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I + 1).Item("ORDER_NO") <>
                   ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I).Item("ORDER_NO") Then
                    sendCustEmail(dsEmail,
                        strOrderNo,
                        ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"),
                        ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"),
                        strEmailTo,
                        strBu, ds.Tables(0).Rows(I).Item("Origin"))
                    dsEmail.Clear()
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If
                    connectOR.Open()
                    buildNotifyNSTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"),
                        ds.Tables(0).Rows(I).Item("ORDER_NO"),
                        ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"), ds.Tables(0).Rows(I).Item("Origin"))
                End If
                strPreOrderno = ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I).Item("ORDER_NO")
                Try
                    connectOR.Close()
                Catch ex As Exception

                End Try
            Next
            objStreamWriter.WriteLine("  StatChg Build Notify Non STK, total orders = " & ds.Tables(0).Rows.Count)
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
        Else
            Console.WriteLine("Fetched Datas 0")
            objGenerallLogStreamWriter.WriteLine("Fetched Datas 0")
        End If
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try
    End Function

    Private Function buildNotifySTKReady() As Boolean

        Dim strSQLString As String
        Dim bolError7 As Boolean
        Dim bolErrorR As Boolean
        'zzzzzzzzzzzzzzzzzzzzzzzzzzz
        ' get all stock lines that have been picked
        strSQLString = "SELECT A.BUSINESS_UNIT_OM, A.ORDER_NO,L.ISA_WORK_ORDER_NO as WORK_ORDER_NO, A.LINE_NBR," & vbCrLf &
                " A.EMPLID, A.ISA_LINE_STATUS AS ISA_ORDER_STATUS," & vbCrLf &
                " B.INV_ITEM_ID, B.QTY_PICKED, B.QTY_BACKORDER," & vbCrLf &
                " C.DESCR60, D.ISA_EMPLOYEE_EMAIL," & vbCrLf &
                " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH," & vbCrLf &
                " C.INV_ITEM_ID, H.Origin " & vbCrLf &
                " FROM PS_ISA_ORDSTAT_EML A," & vbCrLf &
                " SYSADM8.PS_IN_DEMAND B," & vbCrLf &
                " PS_MASTER_ITEM_TBL C," & vbCrLf &
                " PS_ISA_USERS_TBL D," & vbCrLf &
                " ps_isa_ord_intf_hD H ," & vbCrLf &
                " ps_isa_ord_intf_lN L " & vbCrLf &
                " WHERE A.EMAIL_DATETIME Is NULL" & vbCrLf &
                " AND A.ISA_LINE_STATUS = 'PKF'" & vbCrLf &
                " AND B.DEMAND_SOURCE = 'OM'" & vbCrLf &
                 " and H.order_no=A.order_no " & vbCrLf &
                " and H.business_unit_om=A.BUSINESS_UNIT_OM " & vbCrLf &
                " and H.business_unit_om=D.BUSINESS_UNIT " & vbCrLf &
                " and H.business_unit_om = L.business_unit_om " & vbCrLf &
                " and H.order_no = L.order_no " & vbCrLf &
                " AND B.SOURCE_BUS_UNIT = A.BUSINESS_UNIT_OM" & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO" & vbCrLf &
                " AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO" & vbCrLf &
                " AND C.SETID = 'MAIN1'" & vbCrLf &
                " AND B.INV_ITEM_ID = C.INV_ITEM_ID" & vbCrLf &
                " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf &
                " and L.ISA_INTFC_LN = A.line_nbr" & vbCrLf &
                " AND (UPPER(A.EMPLID)) = UPPER(D.ISA_EMPLOYEE_ID)" & vbCrLf &
                " AND B.IN_FULFILL_STATE IN ('60','50')" & vbCrLf &
                " AND B.DEMAND_SOURCE = 'OM'" & vbCrLf &
                " AND B.QTY_PICKED > 0" & vbCrLf

        Dim Command As OleDbCommand = New OleDbCommand(strSQLString, connectOR)
        If connectOR.State = ConnectionState.Open Then
            'do nothing
        Else
            connectOR.Open()
        End If
        Dim dataAdapter As OleDbDataAdapter =
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
            objGenerallLogStreamWriter.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
            Console.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
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
            dsEmail.Columns.Add("Order Number")
            dsEmail.Columns.Add("Work Order Number")
            dsEmail.Columns.Add("Line Notes")
            For I = 0 To ds.Tables(0).Rows.Count - 1

                dr = dsEmail.NewRow()
                dr.Item(0) = ds.Tables(0).Rows(I).Item("INV_ITEM_ID")
                dr.Item(1) = ds.Tables(0).Rows(I).Item("DESCR60")
                dr.Item(2) = ds.Tables(0).Rows(I).Item("QTY_PICKED")
                dr.Item(3) = ds.Tables(0).Rows(I).Item("QTY_BACKORDER")
                dr.Item(4) = ds.Tables(0).Rows(I).Item("ORDER_NO")
                dr.Item(5) = ds.Tables(0).Rows(I).Item("WORK_ORDER_NO")
                Dim ln_notes As String = ""
                ln_notes = GetLineNotes(Convert.ToString(ds.Tables(0).Rows(I).Item("ORDER_NO")), Convert.ToString(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM")), Convert.ToString(ds.Tables(0).Rows(I).Item("LINE_NBR")))
                dr.Item(6) = ln_notes
                connectOR.Open()

                decQtyOrdered = getQtyOrdered(ds.Tables(0).Rows(I).Item("ORDER_NO"),
                                              ds.Tables(0).Rows(I).Item("LINE_NBR"),
                                              connectOR)

                If (decQtyOrdered > 0) And
                    decQtyOrdered <> (ds.Tables(0).Rows(I).Item("QTY_PICKED") +
                                    ds.Tables(0).Rows(I).Item("QTY_BACKORDER")) Then
                    decQtyShipped = getQtyShipped(ds.Tables(0).Rows(I).Item("ORDER_NO"),
                                              ds.Tables(0).Rows(I).Item("LINE_NBR"),
                                              connectOR)
                    If decQtyShipped > 0 Then
                        dr.Item(3) = Format((decQtyOrdered - decQtyShipped), "0.####")
                    End If

                End If
                dsEmail.Rows.Add(dr)
                Dim strEmail_test As String = ";tom.rapp@sdi.com"

                Dim strEmailTo As String = ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL")
                Dim strOrderNo As String = ds.Tables(0).Rows(I).Item("ORDER_NO")
                Dim strBu As String = ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM")

                If I = ds.Tables(0).Rows.Count - 1 Then
                    sendCustEmail(dsEmail,
                        strOrderNo,
                        ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"),
                        ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"),
                        strEmailTo,
                        strBu,
                        ds.Tables(0).Rows(I).Item("Origin"))
                    dsEmail.Clear()
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If
                    connectOR.Open()
                    buildNotifySTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"),
                        ds.Tables(0).Rows(I).Item("ORDER_NO"),
                        ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"), ds.Tables(0).Rows(I).Item("Origin"))

                ElseIf ds.Tables(0).Rows(I + 1).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I + 1).Item("ORDER_NO") <>
                   ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I).Item("ORDER_NO") Then
                    sendCustEmail(dsEmail,
                        strOrderNo,
                        ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"),
                        ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"),
                        strEmailTo,
                        strBu,
                        ds.Tables(0).Rows(I).Item("Origin"))
                    dsEmail.Clear()
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If
                    connectOR.Open()
                    buildNotifySTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"),
                        ds.Tables(0).Rows(I).Item("ORDER_NO"),
                        ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"), ds.Tables(0).Rows(I).Item("Origin"))
                    'buildNotifySTKReady = updateSendEmailTbl(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"), _
                    '                       ds.Tables(0).Rows(I).Item("ORDER_NO"), _
                    '                       ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"))
                    Try
                        connectOR.Close()
                    Catch ex As Exception

                    End Try
                End If
                strPreOrderno = ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                   & ds.Tables(0).Rows(I).Item("ORDER_NO")
            Next
            objStreamWriter.WriteLine("  StatChg Build Notify STK, total orders = " & ds.Tables(0).Rows.Count)
            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
        Else
            Console.WriteLine("Fetched Datas 0")
            objGenerallLogStreamWriter.WriteLine("Fetched Datas 0")
        End If
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try
    End Function

    Private Function getQtyOrdered(ByVal strOrderNo As String,
                                ByVal intLineNbr As Integer,
                                ByVal connectOR As OleDb.OleDbConnection) As Decimal

        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If
        connectOR.Open()
        Dim strSQLstring As String
        strSQLstring = "SELECT SUM( B.QTY_ORDERED)" & vbCrLf &
                " FROM PS_ISA_ORD_INTF_LN A, PS_ORD_LINE B" & vbCrLf &
                " WHERE A.ORDER_NO = '" & strOrderNo & "'" & vbCrLf &
                " AND A.ISA_INTFC_LN = " & intLineNbr & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                " AND A.ISA_INTFC_LN = B.ORDER_INT_LINE_NO"

        Try
            getQtyOrdered = ORDBAccess.GetScalar(strSQLstring, connectOR)
        Catch ex As Exception
            getQtyOrdered = 0
        End Try

    End Function

    Private Function getQtyShipped(ByVal strOrderNo As String,
                                ByVal intLineNbr As Integer,
                                ByVal connectOR As OleDb.OleDbConnection) As Decimal

        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If
        connectOR.Open()
        Dim strSQLstring As String
        strSQLstring = "SELECT SUM( B.QTY_PICKED)" & vbCrLf &
                " FROM PS_ISA_ORD_INTF_LN A, SYSADM8.PS_IN_DEMAND B" & vbCrLf &
                " WHERE A.ORDER_NO = '" & strOrderNo & "'" & vbCrLf &
                " AND A.ISA_INTFC_LN = " & intLineNbr & vbCrLf &
                " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                " AND A.ISA_INTFC_LN = B.ORDER_INT_LINE_NO" & vbCrLf &
                " AND B.IN_FULFILL_STATE IN ('60','50')" & vbCrLf &
                " AND B.DEMAND_SOURCE = 'OM'" & vbCrLf &
                " AND B.QTY_PICKED > 0" & vbCrLf &
                ""
        Try
            getQtyShipped = ORDBAccess.GetScalar(strSQLstring, connectOR)
        Catch ex As Exception
            getQtyShipped = 0
        End Try

    End Function

    Private Sub sendCustEmail(ByVal dsEmail As DataTable,
                        ByVal strOrderNo As String,
                        ByVal strFirstName As String,
                        ByVal strLastName As String,
                        ByVal strEmail As String,
                        ByVal strbu As String,
                        ByVal strOrigin As String)

        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
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
        Dim strccfirst As String = "erwin.bautista"   '  "pete.doyle"
        Dim strcclast As String = "sdi.com"
        Mailer.From = "SDIExchange@SDI.com"  '  "Insiteonline@SDI.com"
        Mailer.Cc = ""
        Mailer.Bcc = strccfirst & "@" & strcclast
        'strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
        'strbodyhead = strbodyhead & "<center><span >SDiExchange - Order Status</span></center>"
        'strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        strbodyhead = "<table width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='82px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Arial; font-size: x-large; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center; margin: 0px auto;'>SDiExchange - Order Status</span></center></td></tr></tbody></table>"
        strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>"
        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        Dim dtgEmail As WebControls.DataGrid
        dtgEmail = New WebControls.DataGrid

        dtgEmail.DataSource = dsEmail
        dtgEmail.DataBind()
        dtgEmail.BorderColor = Gray
        dtgEmail.HeaderStyle.BackColor = System.Drawing.Color.LightGray
        dtgEmail.HeaderStyle.Font.Bold = True
        dtgEmail.HeaderStyle.ForeColor = Black
        WebControls.Unit.Percentage(90)
        dtgEmail.CellPadding = 3
        'dtgEmail.Width.Percentage(90)

        'dtgPO.Columns(9).ItemStyle.HorizontalAlign = HorizontalAlign.Center
        dtgEmail.RenderControl(htmlTWnstk)
        dataGridHTML = SBnstk.ToString()

        ''Get Order Notes here
        Dim Ord_notes As String = ""
        Ord_notes = GetOrderNotes(strOrderNo, strbu)

        Dim strPurchaserName As String = strFirstName &
            " " & strLastName
        Dim ted As String = ";erwin.bautista@sdi.com"  '  ";pete.doyle@sdi.com"
        Dim strPurchaserEmail As String = strEmail
        'dim strPurchaserEmail As String = strEmail
        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"
        strbodydetl = strbodydetl & "<p >Hello " & strPurchaserName & ",<br>"
        strbodydetl = strbodydetl & "&nbsp;<BR>"
        If strbu = "I0260" Or strbu = "I0206" Then
            If Not strOrigin = "MIS" Then
                strbodydetl = strbodydetl & "Your SDiExchange Order Number " & strOrderNo & " has been Processed and Delivered.<br>"
            Else
                strbodydetl = strbodydetl & "Your SDiExchange Order Number " & strOrderNo & " has been Picked and is Ready for Pickup at the SDI Storeroom.<br>"
            End If
        Else
            strbodydetl = strbodydetl & "Your SDiExchange Order Number " & strOrderNo & " has been Processed and Ready for Pickup.<br>"
        End If
        strbodydetl = strbodydetl & "&nbsp;<BR>"
        If Not Ord_notes Is Nothing Then
            If Not (String.IsNullOrEmpty(Ord_notes.Trim())) Then
                strbodydetl = strbodydetl & "Customer Notes: " & Ord_notes & " <br> "
            End If
        End If
        strbodydetl = strbodydetl & "Order contents:<br>"
        strbodydetl = strbodydetl & "&nbsp;<BR>"

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
        strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodydetl = strbodydetl & "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & vbCrLf
        Mailer.Body = strbodyhead & strbodydetl
        If Not getDBName() Then
            Mailer.To = "webdev@sdi.com"
        Else
            Mailer.To = strPurchaserEmail
        End If
        Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
        If strbu = "I0260" Or strbu = "I0206" Then
            If Not strOrigin = "MIS" Then
                If Not getDBName() Then
                    Mailer.Subject = "<<TEST SITE>>SDiExchange - Order Status " & strOrderNo & " has been Delivered"
                Else
                    Mailer.Subject = "SDiExchange - Order Status " & strOrderNo & " has been Delivered"
                End If
            Else
                If Not getDBName() Then
                    Mailer.Subject = "<<TEST SITE>>SDiExchange - Order Status " & strOrderNo & " Picked & Ready for Pickup @ SDI Storeroom"
                Else
                    Mailer.Subject = "SDiExchange - Order Status " & strOrderNo & " Picked & Ready for Pickup @ SDI Storeroom"
                End If

            End If
            SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, String.Empty, "webdev@sdi.com", Mailer.Body, "StatusChangeEmail0", MailAttachmentName, MailAttachmentbytes.ToArray())
        Else
            If Not getDBName() Then
                Mailer.Subject = "<<TEST SITE>>SDiExchange - Order Status " & strOrderNo & " is Ready for Pickup"
            Else
                Mailer.Subject = "SDiExchange - Order Status " & strOrderNo & " is Ready for Pickup"
            End If

            SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, String.Empty, "webdev@sdi.com", Mailer.Body, "StatusChangeEmail0", MailAttachmentName, MailAttachmentbytes.ToArray())
        End If
    End Sub

    Private Sub SendEmail()

        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "webdev@sdi.com"  '  "pete.doyle@sdi.com"

        'The subject of the email
        email.Subject = "Error in the 'Status Change Email' Console App."

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<html><body><table><tr><td>StatusChangeEmail has completed with errors, review log (C:\StatChg) </td></tr>"

        'email.Body = email.Body & "<tr><td></td><a href='\\BDougherty_XP-l\logs'>\\BDougherty_XP-l\logs\</a></tr></table></body></html>"

        'Send the email and handle any error that occurs
        Try
            'UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, "sriram.s@avasoft.biz", "", "", "Y", email.Body, connectOR)
            SDIEmailService.EmailUtilityServices("MailandStore", email.From, email.To, email.Subject, String.Empty, String.Empty, email.Body, "StatusChangeEmail0", MailAttachmentName, MailAttachmentbytes.ToArray())
        Catch
            objStreamWriter.WriteLine("     Error - the email was not sent")
        End Try

    End Sub

    'Private Sub sendemail(ByVal mailer As MailMessage)

    '    Try
    '        SmtpMail.Send(mailer)
    '    Catch ex As Exception
    '        objStreamWriter.WriteLine("     Error - in the sendemail to customer SUB")
    '    End Try
    'End Sub

    Private Function updateSendEmailTbl(ByVal strBU As String, ByVal strOrderNo As String, ByVal strOrderStatus As String, ByVal strorigin As String) As Boolean

        Dim strSQLstring As String
        Dim rowsaffected As Integer
        strSQLstring = "UPDATE PS_ISA_ORDSTAT_EML" & vbCrLf &
                       " SET EMAIL_DATETIME = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                       " WHERE BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf &
                       " AND ORDER_NO = '" & strOrderNo & "'" & vbCrLf &
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

    Private Function IsBuAscend(ByVal strBu As String) As Boolean

        ' check is processed order is ASCEND order
        Dim bIsAscend As Boolean = False
        Dim strAscendBuList As String = "I0440,I0441,I0442,I0443,I0444"
        If Trim(strBu) <> "" Then
            Try
                If strAscendBuList.IndexOf(strBu.Trim().ToUpper()) > -1 Then
                    bIsAscend = True
                End If
            Catch ex As Exception
                bIsAscend = False
            End Try
        End If

        Return bIsAscend

    End Function

    Private Function checkAllStatus_7(ByVal strBU As String, ByVal dtrStartDate As String) As Boolean
        Dim strSQLstring As String
        Dim dteEndDate As DateTime = Now

        Dim format As New System.Globalization.CultureInfo("en-US", True)
        strSQLstring = "SELECT" & vbCrLf &
            " to_char(MAX( A.DTTM_STAMP), 'MM/DD/YY HH24:MI:SS') as MAXDATE" & vbCrLf &
            " FROM PS_ISAORDSTATUSLOG A" & vbCrLf &
             " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "' "

        Dim dr As OleDbDataReader = Nothing

        Try
            objStreamWriter.WriteLine("  CheckAllStatus_7 (1): " & strSQLstring)

            Dim command As OleDbCommand
            command = New OleDbCommand(strSQLstring, connectOR)
            If connectOR.State = ConnectionState.Open Then
                'do nothing
            Else
                connectOR.Open()
            End If
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
        Dim objEnterprise As New clsEnterprise(strBU, connectOR)
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


        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        Dim ds As New DataSet
        Dim bolerror1 As Boolean

        ' check is processed order is ASCEND order
        Dim bIsAscend As Boolean = False
        If Trim(strBU) <> "" Then
            bIsAscend = IsBuAscend(strBU)
        End If

        dteEndDate.AddSeconds(1)

        ' stock items will get item id from the ps_isa_ord_intfc_l table  but description from the PS_MASTER_ITEM_TB
        ' non-stock items  has no item-id num and gets description from the ps_isa_ord_intfc_l
        ' PS_ISAORDSTATUSLOG the line number points to the line number in ps_isa_ord_intfc_l
        ' DO NOT SELECT G.ISA_ORDER_STATUS = '6'  WE ARE GETTING IT UP TOP.
        '         '  

        strSQLstring = "SELECT H.ISA_IOL_OP_NAME as STATUS_CODE, TBL.* FROM (SELECT distinct G.BUSINESS_UNIT_OM, G.BUSINESS_UNIT_OM AS G_BUS_UNIT, D.BUSINESS_UNIT, D.ISA_EMPLOYEE_ID, A.ORDER_NO,B.ISA_WORK_ORDER_NO As WORK_ORDER_NO, B.ISA_INTFC_LN AS line_nbr," & vbCrLf &
                 " B.ISA_EMPLOYEE_ID AS EMPLID, B.ISA_LINE_STATUS as ORDER_TYPE," & vbCrLf &
                 " TO_CHAR(G.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM') as DTTM_STAMP, " & vbCrLf   '  & _


        strSQLstring += "  G.ISA_LINE_STATUS AS ISA_ORDER_STATUS, DECODE(G.ISA_LINE_STATUS,'CRE','1','NEW','2','DSP','3','ORD','3','RSV','3','PKA','4','PKP','4','DLP','5','RCP','5','RCF','6','PKQ','5','DLO','5','DLF','6','PKF','7','CNC','C','QTS','Q','QTW','W','1') AS OLD_ORDER_STATUS," & vbCrLf &
                 " (SELECT E.XLATLONGNAME" & vbCrLf &
                                " FROM XLATTABLE E" & vbCrLf &
                                " WHERE E.EFFDT =" & vbCrLf &
                                " (SELECT MAX(E_ED.EFFDT) FROM XLATTABLE E_ED" & vbCrLf &
                                " WHERE(E.FIELDNAME = E_ED.FIELDNAME)" & vbCrLf &
                                " AND E.FIELDVALUE = E_ED.FIELDVALUE" & vbCrLf &
                                " AND E_ED.EFFDT <= SYSDATE)" & vbCrLf &
                                " AND E.FIELDNAME = 'ISA_LINE_STATUS'" & vbCrLf &
                                " AND E.FIELDVALUE = G.ISA_LINE_STATUS) as ORDER_STATUS_DESC, " & vbCrLf &
                 " B.DESCR254 As NONSTOCK_DESCRIPTION, C.DESCR60 as STOCK_DESCRIPTION, D.ISA_EMPLOYEE_EMAIL," & vbCrLf &
                 " B.INV_ITEM_ID as INV_ITEM_ID," & vbCrLf &
                 " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH" & vbCrLf &
                 " ,A.origin" & vbCrLf &
                 " FROM ps_isa_ord_intf_HD A," & vbCrLf  '   & _

        strSQLstring += " ps_isa_ord_intf_LN B," & vbCrLf &
                 " PS_MASTER_ITEM_TBL C," & vbCrLf &
                 " PS_ISA_USERS_TBL D," & vbCrLf &
                 " PS_ISAORDSTATUSLOG G " & vbCrLf &
                 " where G.BUSINESS_UNIT_OM = '" & strBU & "' " & vbCrLf &
                 " AND G.BUSINESS_UNIT_OM = A.BUSINESS_UNIT_OM " & vbCrLf &
                 " AND G.BUSINESS_UNIT_OM = D.BUSINESS_UNIT " & vbCrLf     '   & _

        strSQLstring += "  and A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf &
                 " and A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                 " and C.SETID (+) = 'MAIN1'" & vbCrLf &
                 " and C.INV_ITEM_ID(+) = B.INV_ITEM_ID " & vbCrLf &
                 " AND G.ORDER_NO = A.ORDER_NO " & vbCrLf &
                 " AND B.ISA_INTFC_LN = G.ISA_INTFC_LN" & vbCrLf &
                 " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf &
                 " AND G.DTTM_STAMP > TO_DATE('" & dteStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                 " AND G.DTTM_STAMP <= TO_DATE('" & dteEndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                 " AND UPPER(B.ISA_EMPLOYEE_ID) = UPPER(D.ISA_EMPLOYEE_ID)) TBL, PS_ISA_USERS_PRIVS H " & vbCrLf &
                 " WHERE H.BUSINESS_UNIT = TBL.BUSINESS_UNIT " & vbCrLf &
                 " AND TBL.EMPLID = H.ISA_EMPLOYEE_ID " & vbCrLf &
                 " AND SUBSTR(H.ISA_IOL_OP_NAME,10) = TBL.OLD_ORDER_STATUS " & vbCrLf &
                 " AND H.ISA_IOL_OP_VALUE = 'Y' " & vbCrLf &
                  " ORDER BY ORDER_NO, LINE_NBR, DTTM_STAMP"
        ' this is set up in the user priveleges when giving out the status code priveleges in ISOL under Add/Change User
        ' matches the orserstatus emails set up for with the order status in PS_ISAORDSTATUSLOG
        ' the tenth byte of isa_iol_op_name has the one character g.isa_order_status code
        ' example: substr(emlsubmit1,10) = '1'   order status code 1
        ' We are going to check for priveleges in the upd_email_out program that sends the emails out.

        Try
            objStreamWriter.WriteLine("  CheckAllStatus_7 (2) Q1: " & strSQLstring)

            ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            objStreamWriter.WriteLine("     Error - error reading transaction FROM PS_ISAORDSTATUSLOG A")
            Return True
        End Try

        If IsDBNull(ds.Tables(0).Rows.Count) Or (ds.Tables(0).Rows.Count) = 0 Then
            Console.WriteLine("Fetched Datas 0")
            objGenerallLogStreamWriter.WriteLine("Fetched Datas 0")
            objStreamWriter.WriteLine("     Warning - no status changes to process at this time for All Statuses")
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            Return False
        Else
            Console.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
            objGenerallLogStreamWriter.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
        End If

        Dim rowsaffected As Integer
        Dim tmpOrderNo As String

        If connectOR.State = ConnectionState.Open Then
            'do nothing
        Else
            connectOR.Open()
        End If
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
        dsEmail.Columns.Add("Line Number")
        dsEmail.Columns.Add("Time")
        dsEmail.Columns.Add("Status Code")
        dsEmail.Columns.Add("Work Order Number")
        dsEmail.Columns.Add("PO #")
        dsEmail.Columns.Add("Line Notes")

        Dim strdescription As String = " "
        Dim strEmailTo As String = " "

        For I = 0 To ds.Tables(0).Rows.Count - 1
            Dim strStatus_code As String = " "
            Try
                strStatus_code = ds.Tables(0).Rows(I).Item("STATUS_CODE")
                strStatus_code = strStatus_code.Substring(9)

            Catch ex As Exception
                strStatus_code = " "
            End Try
            Dim strSiteBU As String
            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
            Dim Command As OleDbCommand

            strSQLstring = "SELECT A.BUSINESS_UNIT" & vbCrLf &
                    " FROM PS_REQ_LOADER_DFL A" & vbCrLf &
                    " WHERE A.LOADER_BU = '" & strBU & "'" & vbCrLf

            objStreamWriter.WriteLine("  CheckAllStatus_7 (3): " & strSQLstring)

            Command = New OleDbCommand(strSQLstring, connectOR)
            connectOR.Open()
            Try
                strSiteBU = Command.ExecuteScalar
                connectOR.Close()
            Catch ex As Exception
                objStreamWriter.WriteLine("  StatChg Email NSTK send select siteBU for " & strBU)
                connectOR.Close()
                strSiteBU = "ISA00"
            End Try

            dr1 = dsEmail.NewRow()
            Dim stroderno As String = ds.Tables(0).Rows(I).Item("ORDER_NO")
            Dim strlineno As String = ds.Tables(0).Rows(I).Item("LINE_NBR")
            dr1.Item(0) = ds.Tables(0).Rows(I).Item("ORDER_NO")
            dr1.Item(1) = ds.Tables(0).Rows(I).Item("ORDER_STATUS_DESC")
            dr1.Item(2) = ds.Tables(0).Rows(I).Item("NONSTOCK_DESCRIPTION")
            dr1.Item(3) = ds.Tables(0).Rows(I).Item("STOCK_DESCRIPTION")
            dr1.Item(4) = ds.Tables(0).Rows(I).Item("INV_ITEM_ID")
            dr1.Item(5) = ds.Tables(0).Rows(I).Item("LINE_NBR")
            Dim ln_notes As String = ""
            ln_notes = GetLineNotes(stroderno, strBU, strlineno)
            dr1.Item(10) = ln_notes
            connectOR.Open()
            dr1.Item(6) = ds.Tables(0).Rows(I).Item("DTTM_STAMP")
            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
            Dim strpo_id As String = getpo_id(stroderno, strlineno, strBU, strSiteBU)
            'dr1.Item(7) = ds.Tables(0).Rows(I).Item("STATUS_CODE")
            'just get the last character
            dr1.Item(7) = strStatus_code
            dr1.Item(8) = ds.Tables(0).Rows(I).Item("WORK_ORDER_NO")
            dr1.Item(9) = strpo_id
            dsEmail.Rows.Add(dr1)

            ' "R" nonstock
            ' "7" stock

            If ds.Tables(0).Rows(I).Item("Origin") = "MIS" And strBU = "I0206" Then
                strdescription = "PICKED"
            Else
                Try
                    strdescription = ds.Tables(0).Rows(I).Item("ORDER_STATUS_DESC")
                Catch ex As Exception
                    strdescription = "Err_line_" & I.ToString()
                End Try

            End If
            strEmailTo = ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL")

            If I = ds.Tables(0).Rows.Count - 1 Then

                sendCustEmail1(dsEmail,
                ds.Tables(0).Rows(I).Item("ORDER_NO"),
                dteCompanyID,
                dteCustID,
                ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"),
                strdescription,
                ds.Tables(0).Rows(I).Item("INV_ITEM_ID"),
                ds.Tables(0).Rows(I).Item("LINE_NBR"),
                ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"),
                ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"),
                strEmailTo,
                ds.Tables(0).Rows(I).Item("Origin"),
                strBU)

                dsEmail.Clear()
                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If
            ElseIf ds.Tables(0).Rows(I + 1).Item("BUSINESS_UNIT_OM") _
                          & ds.Tables(0).Rows(I + 1).Item("ORDER_NO") <>
                          ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                          & ds.Tables(0).Rows(I).Item("ORDER_NO") Then

                sendCustEmail1(dsEmail,
               ds.Tables(0).Rows(I).Item("ORDER_NO"),
               dteCompanyID,
               dteCustID,
               ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"),
               strdescription,
               ds.Tables(0).Rows(I).Item("INV_ITEM_ID"),
               ds.Tables(0).Rows(I).Item("LINE_NBR"),
               ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"),
               ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"),
               strEmailTo,
               ds.Tables(0).Rows(I).Item("Origin"),
               strBU)

                dsEmail.Clear()
                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If
            End If
        Next

        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If

        'If strBU <> "I0256" Then
        bolerror1 = updateEnterprise(strBU, dteEndDate)
        'End If
    End Function

    Private Function checkAllStatusNew(ByVal strBU As String) As Boolean
        Dim strSQLstring As String
        Dim dteEndDate As DateTime = Now

        Dim format As New System.Globalization.CultureInfo("en-US", True)
        strSQLstring = "SELECT" & vbCrLf &
            " to_char(MAX( A.DTTM_STAMP), 'MM/DD/YY HH24:MI:SS') as MAXDATE" & vbCrLf &
            " FROM PS_ISAORDSTATUSLOG A" & vbCrLf &
             " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "' "

        Dim dr As OleDbDataReader = Nothing

        Try
            objStreamWriter.WriteLine("  checkAllStatusNew (1): " & strSQLstring)

            Dim command As OleDbCommand
            command = New OleDbCommand(strSQLstring, connectOR)
            If connectOR.State = ConnectionState.Open Then
                'do nothing
            Else
                connectOR.Open()
            End If
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
        Dim objEnterprise As New clsEnterprise(strBU, connectOR)
        Dim dteCustID As String = objEnterprise.CustID
        Dim dteCompanyID As String = objEnterprise.CompanyID
        Dim dteStartDate As DateTime = objEnterprise.SendStartDate
        Dim dteSiteEmail As String = objEnterprise.SiteEmail
        Dim dteSTKREQEmail As String = objEnterprise.STKREQEmail
        Dim dteNONSKREQEmail As String = objEnterprise.NONSKREQEmail

        'If strBU = "I0W01" Then
        'dteStartDate = dteStartDate.AddMinutes(+1)
        'dteStartDate = dteStartDate.AddHours(-36)
        'End If

        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        Dim ds As New DataSet
        Dim bolerror1 As Boolean

        ' check is processed order is ASCEND order
        Dim bIsAscend As Boolean = False
        If Trim(strBU) <> "" Then
            bIsAscend = IsBuAscend(strBU)
        End If

        dteEndDate.AddSeconds(1)

        If strBU = "I0W01" Then
            Try
                UpdateWalmartSourceCode(dteStartDate, dteEndDate, strBU)
            Catch

            End Try
        End If

        ' stock items will get item id from the ps_isa_ord_intfc_l table  but description from the PS_MASTER_ITEM_TB
        ' non-stock items  has no item-id num and gets description from the ps_isa_ord_intfc_l
        ' PS_ISAORDSTATUSLOG the line number points to the line number in ps_isa_ord_intfc_l
        ' DO NOT SELECT G.ISA_ORDER_STATUS = '6'  WE ARE GETTING IT UP TOP.
        '         '  

        strSQLstring = "SELECT H.ISA_IOL_OP_NAME as STATUS_CODE, TBL.* FROM (SELECT distinct G.BUSINESS_UNIT_OM, G.BUSINESS_UNIT_OM AS G_BUS_UNIT, D.BUSINESS_UNIT, D.ISA_EMPLOYEE_ID, A.ORDER_NO,B.ISA_WORK_ORDER_NO As WORK_ORDER_NO, B.ISA_INTFC_LN AS line_nbr," & vbCrLf &
                 " B.ISA_EMPLOYEE_ID AS EMPLID, B.ISA_LINE_STATUS as ORDER_TYPE,B.OPRID_ENTERED_BY, B.SHIPTO_ID as SHIPTO," & vbCrLf &
                 " TO_CHAR(G.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM') as DTTM_STAMP, " & vbCrLf   '  & _


        strSQLstring += "  G.ISA_LINE_STATUS AS ISA_ORDER_STATUS, DECODE(G.ISA_LINE_STATUS,'CRE','01','QTW','02','QTC','03','QTS','04','CST','05','VND','06','APR','07','QTA','08','QTR','09','RFA','10','RFR','11','RFC','12','RCF','13','RCP','14','CNC','15','DLF','16') AS OLD_ORDER_STATUS," & vbCrLf &
                 " (SELECT E.XLATLONGNAME" & vbCrLf &
                                " FROM XLATTABLE E" & vbCrLf &
                                " WHERE E.EFFDT =" & vbCrLf &
                                " (SELECT MAX(E_ED.EFFDT) FROM XLATTABLE E_ED" & vbCrLf &
                                " WHERE(E.FIELDNAME = E_ED.FIELDNAME)" & vbCrLf &
                                " AND E.FIELDVALUE = E_ED.FIELDVALUE" & vbCrLf &
                                " AND E_ED.EFFDT <= SYSDATE)" & vbCrLf &
                                " AND E.FIELDNAME = 'ISA_LINE_STATUS'" & vbCrLf &
                                " AND E.FIELDVALUE = G.ISA_LINE_STATUS) as ORDER_STATUS_DESC, " & vbCrLf &
                 " B.DESCR254 As NONSTOCK_DESCRIPTION, C.DESCR60 as STOCK_DESCRIPTION, D.ISA_EMPLOYEE_EMAIL," & vbCrLf &
                 " B.INV_ITEM_ID as INV_ITEM_ID," & vbCrLf &
        " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH" & vbCrLf &
                 " ,A.origin, LD.PO_ID, SH.ISA_ASN_TRACK_NO" & vbCrLf &
                 " FROM ps_isa_ord_intf_HD A," & vbCrLf  '   & _

        strSQLstring += " ps_isa_ord_intf_LN B," & vbCrLf &
                 " PS_MASTER_ITEM_TBL C," & vbCrLf &
                 " PS_ISA_USERS_TBL D," & vbCrLf &
                 " PS_ISAORDSTATUSLOG G, PS_ISA_ASN_SHIPPED SH, PS_PO_LINE_DISTRIB LD" & vbCrLf &
                 " where G.BUSINESS_UNIT_OM = '" & strBU & "' " & vbCrLf &
                 " AND G.BUSINESS_UNIT_OM = A.BUSINESS_UNIT_OM " & vbCrLf &
                 " AND G.BUSINESS_UNIT_OM = D.BUSINESS_UNIT " & vbCrLf     '   & _

        strSQLstring += "  and A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf &
                 " and A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                 " and C.SETID (+) = 'MAIN1'" & vbCrLf &
                 " and C.INV_ITEM_ID(+) = B.INV_ITEM_ID " & vbCrLf &
                 " AND G.ORDER_NO = A.ORDER_NO " & vbCrLf &
                 " AND B.ISA_INTFC_LN = G.ISA_INTFC_LN" & vbCrLf &
                 " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf &
                 " AND SH.PO_ID (+) = LD.PO_ID And SH.LINE_NBR (+) = LD.LINE_NBR And SH.SCHED_NBR (+) = LD.SCHED_NBR And LD.Req_id (+) = B.order_no AND LD.REQ_LINE_NBR (+) = B.ISA_INTFC_LN" & vbCrLf &
                 " AND G.DTTM_STAMP > TO_DATE('" & dteStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                 " AND G.DTTM_STAMP <= TO_DATE('" & dteEndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                 " AND UPPER(B.ISA_EMPLOYEE_ID) = UPPER(D.ISA_EMPLOYEE_ID)) TBL, PS_ISA_USERS_PRIVS H " & vbCrLf &
                 " WHERE H.BUSINESS_UNIT = TBL.BUSINESS_UNIT " & vbCrLf &
                 " AND TBL.EMPLID = H.ISA_EMPLOYEE_ID " & vbCrLf &
                 " AND SUBSTR(H.ISA_IOL_OP_NAME,9) = TBL.OLD_ORDER_STATUS " & vbCrLf &
                 " AND H.ISA_IOL_OP_VALUE = 'Y' " & vbCrLf &
                  " ORDER BY ORDER_NO, LINE_NBR, DTTM_STAMP"
        ' this is set up in the user priveleges when giving out the status code priveleges in ISOL under Add/Change User
        ' matches the orserstatus emails set up for with the order status in PS_ISAORDSTATUSLOG
        ' the tenth byte of isa_iol_op_name has the one character g.isa_order_status code
        ' example: substr(emlsubmit1,10) = '1'   order status code 1
        ' We are going to check for priveleges in the upd_email_out program that sends the emails out.

        Try
            objStreamWriter.WriteLine("  checkAllStatusNew (2) Q1: " & strSQLstring)

            ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            objStreamWriter.WriteLine("     Error - error reading transaction FROM PS_ISAORDSTATUSLOG A")
            Return True
        End Try

        If IsDBNull(ds.Tables(0).Rows.Count) Or (ds.Tables(0).Rows.Count) = 0 Then
            Console.WriteLine("Fetched Datas 0")
            objGenerallLogStreamWriter.WriteLine("Fetched Datas 0")
            objStreamWriter.WriteLine("     Warning - no status changes to process at this time for All Statuses")
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            Return False
        Else
            Console.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
            objGenerallLogStreamWriter.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
        End If

        Dim rowsaffected As Integer
        Dim tmpOrderNo As String

        If connectOR.State = ConnectionState.Open Then
            'do nothing
        Else
            connectOR.Open()
        End If
        Dim strPreOrderno As String
        Dim I As Integer
        Dim X As Integer
        Dim dsEmail As New DataTable
        Dim dr1 As DataRow
        Dim dsShipTo As DataSet

        dsEmail.Columns.Add("Order No.")
        dsEmail.Columns.Add("Status")
        dsEmail.Columns.Add("Non-Stock Item Description")
        dsEmail.Columns.Add("Stock Item Description")
        dsEmail.Columns.Add("Item ID")
        dsEmail.Columns.Add("Line Number")
        dsEmail.Columns.Add("Time")
        dsEmail.Columns.Add("Status Code")
        dsEmail.Columns.Add("Work Order Number")
        dsEmail.Columns.Add("PO #")
        dsEmail.Columns.Add("Line Notes")
        dsEmail.Columns.Add("Tracking No")
        If strBU = "I0W01" Then
            dsEmail.Columns.Add("Ship To")
            Try
                strSQLstring = "SELECT DESCR,SHIPTO_ID FROM PS_SHIPTO_TBL"
                dsShipTo = ORDBAccess.GetAdapter(strSQLstring, connectOR)
            Catch
            End Try
        End If
        Dim strdescription As String = " "
        Dim strEmailTo As String = " "
        Dim strEmpID As String = ""
        Dim OrderStatusURL As String = ConfigurationManager.AppSettings("OrderStatusURL")
        Dim OrderStatusToken As String = ConfigurationManager.AppSettings("OrderStatusToken")
        Dim lstOfString As List(Of String) = New List(Of String)
        For I = 0 To ds.Tables(0).Rows.Count - 1
            Dim strStatus_code As String = " "
            Try
                strStatus_code = ds.Tables(0).Rows(I).Item("STATUS_CODE")
                strStatus_code = strStatus_code.Substring(9)

            Catch ex As Exception
                strStatus_code = " "
            End Try
            Dim strSiteBU As String
            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
            Dim Command As OleDbCommand

            strSQLstring = "SELECT A.BUSINESS_UNIT" & vbCrLf &
                    " FROM PS_REQ_LOADER_DFL A" & vbCrLf &
                    " WHERE A.LOADER_BU = '" & strBU & "'" & vbCrLf

            objStreamWriter.WriteLine("  CheckAllStatus_7 (3): " & strSQLstring)

            Command = New OleDbCommand(strSQLstring, connectOR)
            connectOR.Open()
            Try
                strSiteBU = Command.ExecuteScalar
                connectOR.Close()
            Catch ex As Exception
                objStreamWriter.WriteLine("  StatChg Email NSTK send select siteBU for " & strBU)
                connectOR.Close()
                strSiteBU = "ISA00"
            End Try

            dr1 = dsEmail.NewRow()
            Dim stroderno As String = ds.Tables(0).Rows(I).Item("ORDER_NO")
            Dim strlineno As String = ds.Tables(0).Rows(I).Item("LINE_NBR")
            dr1.Item(0) = ds.Tables(0).Rows(I).Item("ORDER_NO")
            dr1.Item(1) = ds.Tables(0).Rows(I).Item("ORDER_STATUS_DESC")
            dr1.Item(2) = ds.Tables(0).Rows(I).Item("NONSTOCK_DESCRIPTION")
            dr1.Item(3) = ds.Tables(0).Rows(I).Item("STOCK_DESCRIPTION")
            dr1.Item(4) = ds.Tables(0).Rows(I).Item("INV_ITEM_ID")
            dr1.Item(5) = ds.Tables(0).Rows(I).Item("LINE_NBR")
            Dim ln_notes As String = ""
            ln_notes = GetLineNotes(stroderno, strBU, strlineno)
            dr1.Item(10) = ln_notes
            connectOR.Open()
            dr1.Item(6) = ds.Tables(0).Rows(I).Item("DTTM_STAMP")
            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
            'Dim strpo_id As String = getpo_id(stroderno, strlineno, strBU, strSiteBU)
            'dr1.Item(7) = ds.Tables(0).Rows(I).Item("STATUS_CODE")
            'just get the last character
            dr1.Item(7) = strStatus_code
            dr1.Item(8) = ds.Tables(0).Rows(I).Item("WORK_ORDER_NO")
            dr1.Item(9) = ds.Tables(0).Rows(I).Item("PO_ID")
            Dim trackingNo As String = ""
            Try
                trackingNo = ds.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")
            Catch ex As Exception
                trackingNo = ""
            End Try


            If Not String.IsNullOrEmpty(trackingNo) Then
                If trackingNo.Contains("1Z") Then
                    Dim URL As String = "http://wwwapps.ups.com/WebTracking/processInputRequest?HTMLVersion=5.0&sort_by=status&term_warn=yes&tracknums_displayed=5&TypeOfInquiryNumber=T&loc=en_US&InquiryNumber1=" & trackingNo & "&InquiryNumber2=&InquiryNumber3=&InquiryNumber4=&InquiryNumber5=&AgreeToTermsAndConditions=yes&track.x=25&track.y=9','','"
                    Dim m_cURL1 As String = "<a href=""" & URL & """ target=""_blank"">" & trackingNo & "</a>"
                    dr1.Item(11) = m_cURL1
                Else
                    Dim URL As String = "https://www.fedex.com/apps/fedextrack/?action=track&trackingnumber=" & trackingNo & "&cntry_code=us&locale=en_US"
                    Dim m_cURL1 As String = "<a href=""" & URL & """ target=""_blank"">" & trackingNo & "</a>"
                    dr1.Item(11) = m_cURL1
                End If
            Else
                dr1.Item(11) = ""
            End If
            If strBU = "I0W01" Then
                If ds.Tables(0).Rows(I).Item("SHIPTO").ToString <> "" Then
                    Try
                        Dim Descr As String = dsShipTo.Tables(0).AsEnumerable().
 Where(Function(r) Convert.ToString(r.Field(Of String)("SHIPTO_ID")) = ds.Tables(0).Rows(I).Item("SHIPTO").ToString).
 Select(Function(r) Convert.ToString(r.Field(Of String)("DESCR"))).FirstOrDefault()
                        dr1.Item(12) = Descr + "_" + ds.Tables(0).Rows(I).Item("SHIPTO").ToString
                    Catch
                        dr1.Item(12) = ""
                    End Try

                End If
            End If
            dsEmail.Rows.Add(dr1)
            ' "R" nonstock
            ' "7" stock
            If ds.Tables(0).Rows(I).Item("Origin") = "MIS" And strBU = "I0206" Then
                strdescription = "PICKED"
            Else
                Try
                    strdescription = ds.Tables(0).Rows(I).Item("ORDER_STATUS_DESC")
                Catch ex As Exception
                    strdescription = "Err_line_" & I.ToString()
                End Try

            End If
            strEmailTo = ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_EMAIL")
            strEmpID = ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_ID")

            If I = ds.Tables(0).Rows.Count - 1 Then

                sendCustEmail1(dsEmail,
                ds.Tables(0).Rows(I).Item("ORDER_NO"),
                dteCompanyID,
                dteCustID,
                ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"),
                strdescription,
                ds.Tables(0).Rows(I).Item("INV_ITEM_ID"),
                ds.Tables(0).Rows(I).Item("LINE_NBR"),
                ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"),
                ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"),
                strEmailTo,
                ds.Tables(0).Rows(I).Item("Origin"),
                strBU, strEmpID)

                dsEmail.Clear()
                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If
            ElseIf ds.Tables(0).Rows(I + 1).Item("BUSINESS_UNIT_OM") _
                          & ds.Tables(0).Rows(I + 1).Item("ORDER_NO") <>
                          ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") _
                          & ds.Tables(0).Rows(I).Item("ORDER_NO") Then

                sendCustEmail1(dsEmail,
               ds.Tables(0).Rows(I).Item("ORDER_NO"),
               dteCompanyID,
               dteCustID,
               ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS"),
               strdescription,
               ds.Tables(0).Rows(I).Item("INV_ITEM_ID"),
               ds.Tables(0).Rows(I).Item("LINE_NBR"),
               ds.Tables(0).Rows(I).Item("FIRST_NAME_SRCH"),
               ds.Tables(0).Rows(I).Item("LAST_NAME_SRCH"),
               strEmailTo,
               ds.Tables(0).Rows(I).Item("Origin"),
               strBU, strEmpID)

                dsEmail.Clear()
                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                End If
            End If
        Next

        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If

        'If strBU <> "I0256" Then
        bolerror1 = updateEnterprise(strBU, dteEndDate)
        'End If
    End Function

    Private Function UpdateWalmartSourceCode(ByVal dteStartDate As Date, ByVal dteEndDate As Date, ByVal strBU As String)
        Try
            Dim ds As New DataSet
            Dim strSQLstring As String = String.Empty
            strSQLstring = "SELECT distinct G.BUSINESS_UNIT_OM, G.BUSINESS_UNIT_OM AS G_BUS_UNIT, D.BUSINESS_UNIT, D.ISA_EMPLOYEE_ID, A.ORDER_NO,B.ISA_WORK_ORDER_NO As WORK_ORDER_NO, B.ISA_INTFC_LN AS line_nbr," & vbCrLf &
                " B.ISA_EMPLOYEE_ID AS EMPLID, B.ISA_LINE_STATUS as ORDER_TYPE,B.OPRID_ENTERED_BY," & vbCrLf &
                " TO_CHAR(G.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM') as DTTM_STAMP, " & vbCrLf   '  & _


            strSQLstring += "  G.ISA_LINE_STATUS AS ISA_ORDER_STATUS, DECODE(G.ISA_LINE_STATUS,'CRE','01','QTW','02','QTC','03','QTS','04','CST','05','VND','06','APR','07','QTA','08','QTR','09','RFA','10','RFR','11','RFC','12','RCF','13','RCP','14','CNC','15','DLF','16') AS OLD_ORDER_STATUS," & vbCrLf &
                     " (SELECT E.XLATLONGNAME" & vbCrLf &
                                    " FROM XLATTABLE E" & vbCrLf &
                                    " WHERE E.EFFDT =" & vbCrLf &
                                    " (SELECT MAX(E_ED.EFFDT) FROM XLATTABLE E_ED" & vbCrLf &
                                    " WHERE(E.FIELDNAME = E_ED.FIELDNAME)" & vbCrLf &
                                    " AND E.FIELDVALUE = E_ED.FIELDVALUE" & vbCrLf &
                                    " AND E_ED.EFFDT <= SYSDATE)" & vbCrLf &
                                    " AND E.FIELDNAME = 'ISA_LINE_STATUS'" & vbCrLf &
                                    " AND E.FIELDVALUE = G.ISA_LINE_STATUS) as ORDER_STATUS_DESC, " & vbCrLf &
                     " B.DESCR254 As NONSTOCK_DESCRIPTION, C.DESCR60 as STOCK_DESCRIPTION, D.ISA_EMPLOYEE_EMAIL," & vbCrLf &
                     " B.INV_ITEM_ID as INV_ITEM_ID," & vbCrLf &
            " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH" & vbCrLf &
                     " ,A.origin, LD.PO_ID, SH.ISA_ASN_TRACK_NO" & vbCrLf &
                     " FROM ps_isa_ord_intf_HD A," & vbCrLf  '   & _

            strSQLstring += " ps_isa_ord_intf_LN B," & vbCrLf &
                     " PS_MASTER_ITEM_TBL C," & vbCrLf &
                     " PS_ISA_USERS_TBL D," & vbCrLf &
                     " PS_ISAORDSTATUSLOG G, PS_ISA_ASN_SHIPPED SH, PS_PO_LINE_DISTRIB LD" & vbCrLf &
                     " where G.BUSINESS_UNIT_OM = '" & strBU & "' " & vbCrLf &
                     " AND G.BUSINESS_UNIT_OM = A.BUSINESS_UNIT_OM " & vbCrLf &
                     " AND G.BUSINESS_UNIT_OM = D.BUSINESS_UNIT " & vbCrLf     '   & _

            strSQLstring += "  and A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf &
                     " and A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                     " and C.SETID (+) = 'MAIN1'" & vbCrLf &
                     " and C.INV_ITEM_ID(+) = B.INV_ITEM_ID " & vbCrLf &
                     " AND G.ORDER_NO = A.ORDER_NO " & vbCrLf &
                     " AND B.ISA_INTFC_LN = G.ISA_INTFC_LN" & vbCrLf &
                     " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf &
                     " AND SH.PO_ID (+) = LD.PO_ID And SH.LINE_NBR (+) = LD.LINE_NBR And SH.SCHED_NBR (+) = LD.SCHED_NBR And LD.Req_id (+) = B.order_no AND LD.REQ_LINE_NBR (+) = B.ISA_INTFC_LN" & vbCrLf &
                     " AND G.DTTM_STAMP > TO_DATE('" & dteStartDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                     " AND G.DTTM_STAMP <= TO_DATE('" & dteEndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                     " AND UPPER(B.ISA_EMPLOYEE_ID) = UPPER(D.ISA_EMPLOYEE_ID)" & vbCrLf &
                      " ORDER BY ORDER_NO, LINE_NBR, DTTM_STAMP"

            Try
                objWalmartSC.WriteLine("  UpdateWalmartSourceCode Q1: " & strSQLstring)

                ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)
                Dim I As Integer
                Dim lstOfString As List(Of String) = New List(Of String)
                For I = 0 To ds.Tables(0).Rows.Count - 1

                    Try
                        Dim OrderNo As String = ds.Tables(0).Rows(I).Item("ORDER_NO")
                        If Not lstOfString.Contains(OrderNo) Then
                            objWalmartSC.WriteLine("Order No: " + Convert.ToString(OrderNo))
                            lstOfString.Add(OrderNo)
                            Dim WorkOrder As String = ds.Tables(0).Rows(I).Item("WORK_ORDER_NO")
                            objWalmartSC.WriteLine("WorkOrder No: " + Convert.ToString(WorkOrder))
                            Dim EnteredBy As String = ds.Tables(0).Rows(I).Item("OPRID_ENTERED_BY")
                            If Not String.IsNullOrEmpty(WorkOrder) Then
                                Dim strSQLQuery As String = "select THIRDPARTY_COMP_ID from SDIX_USERS_TBL where ISA_EMPLOYEE_ID='" & EnteredBy & "' "
                                Dim dsUser As DataSet = ORDBAccess.GetAdapter(strSQLQuery, connectOR)
                                Dim Order As String()
                                If dsUser.Tables.Count > 0 Then
                                    Dim THIRDPARTY_COMP_ID As String = String.Empty
                                    Try
                                        THIRDPARTY_COMP_ID = dsUser.Tables(0).Rows(I).Item("THIRDPARTY_COMP_ID")
                                    Catch ex As Exception
                                        THIRDPARTY_COMP_ID = "0"
                                    End Try
                                    Dim OrderStatusDetail As New OrderStatusDetail
                                    Dim orderDetail As String = OrdrStatus(OrderNo)
                                    objWalmartSC.WriteLine("Current Order Status: " + Convert.ToString(orderDetail))
                                    If orderDetail.Trim() <> "" Then
                                        Order = orderDetail.Split("^"c)
                                        OrderStatusDetail.orderStatus = Order(0)
                                        OrderStatusDetail.statusDesc = Order(1)
                                        OrderStatusDetail.dueDate = Order(2)
                                        OrderStatusDetail.message = "Success"
                                        objWalmartSC.WriteLine("Order No: " + Convert.ToString(OrderNo) + "Status" + Convert.ToString(OrderStatusDetail.statusDesc))
                                        If OrderStatusDetail.message = "Success" Then
                                            If OrderStatusDetail.statusDesc = "Delivered" Or OrderStatusDetail.statusDesc = "En Route from Vendor" Then
                                                Dim CheckWOStatus As String = CheckWorkOrderStatus(WorkOrder, THIRDPARTY_COMP_ID)
                                                objWalmartSC.WriteLine("CheckWOStatus: " + Convert.ToString(CheckWOStatus))
                                                If CheckWOStatus.ToUpper() <> "COMPLETED" And CheckWOStatus <> "Failed" Then
                                                    Dim WOStatus As String = If(OrderStatusDetail.statusDesc = "Delivered", "PARTS DELIVERED", "PARTS SHIPPED")
                                                    If CheckWOStatus <> WOStatus Then
                                                        Dim PurchaseNo As String = PurchaseOrderNo(WorkOrder, THIRDPARTY_COMP_ID)
                                                        If PurchaseNo <> "Failed" Then
                                                            If Not String.IsNullOrEmpty(THIRDPARTY_COMP_ID) Then
                                                                If THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                                                                    UpdateWorkOrderStatus(WorkOrder, "CBRE", WOStatus)
                                                                    UpdateWorkOrderStatus(PurchaseNo, "Walmart", WOStatus)
                                                                Else
                                                                    UpdateWorkOrderStatus(WorkOrder, "Walmart", WOStatus)
                                                                End If
                                                            Else
                                                                UpdateWorkOrderStatus(WorkOrder, "Walmart", WOStatus)
                                                            End If

                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Catch ex As Exception
                        objWalmartSC.WriteLine("Method:checkAllStatusNew - " + ex.Message)
                    End Try
                Next

            Catch OleDBExp As OleDbException
                Console.WriteLine("")
                Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                Console.WriteLine("")
                connectOR.Close()
                objWalmartSC.WriteLine("     Error - error reading transaction FROM PS_ISAORDSTATUSLOG A")
                Return True
            End Try

            If IsDBNull(ds.Tables(0).Rows.Count) Or (ds.Tables(0).Rows.Count) = 0 Then
                Console.WriteLine("Fetched Datas 0")
                objWalmartSC.WriteLine("Fetched Datas 0")
                objWalmartSC.WriteLine("     Warning - no status changes to process at this time for All Statuses")
                Try
                    connectOR.Close()
                Catch ex As Exception

                End Try
                Return False
            Else
                Console.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
                objWalmartSC.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
            End If
        Catch ex As Exception

        End Try
    End Function

    Function GetOrderNotes(ByVal OrderNo As String, ByVal BU As String) As String

        Dim strSQLstring As String = ""
        Dim Order_notes As String = ""

        strSQLstring = "SELECT ISA_LINE_NOTES FROM SYSADM8.PS_ISA_ORDLN_NOTE WHERE ORDER_NO ='" & OrderNo & "' AND ISA_INTFC_LN = 0 AND BUSINESS_UNIT_OM = '" & BU & "'"

        Try
            objStreamWriter.WriteLine("  GetOrderNotes: " & strSQLstring)

            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
            connectOR.Open()
            Order_notes = ORDBAccess.GetScalar(strSQLstring, connectOR)
            connectOR.Close()
        Catch ex As Exception
            Try
                connectOR.Close()
            Catch ex3 As Exception

            End Try
        End Try
        Return Order_notes
    End Function

    Function GetLineNotes(ByVal OrderNo As String, ByVal BU As String, ByVal strLnNotes As String) As String

        Dim strSQLstring As String = ""
        Dim Order_notes As String = ""
        Dim ds As DataSet

        strSQLstring = "SELECT ISA_LINE_NOTES FROM SYSADM8.PS_ISA_ORDLN_NOTE WHERE ORDER_NO = '" + OrderNo + "' AND business_unit_om = '" + BU + "' AND isa_intfc_ln = " + strLnNotes + ""

        Try
            objStreamWriter.WriteLine("  GetOrderNotes: " & strSQLstring)

            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
            connectOR.Open()
            ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)
            If Not ds Is Nothing Then
                If ds.Tables(0).Rows.Count > 0 Then
                    Order_notes = Convert.ToString(ds.Tables(0).Rows(0).Item("ISA_LINE_NOTES"))
                Else

                End If
            Else

            End If
        Catch ex As Exception

        End Try
        Return Order_notes
    End Function

    Private Function GetAscendEmailAddress(ByVal strBu As String, ByVal strOrderNo As String, ByVal connectOR As OleDbConnection) As String
        Dim strAscendEmail As String = ""

        Try
            'GET ASCEND E-MAIL ADDRESS FOR THIS ORDER
            Dim strAscendSql As String = ""
            strAscendSql += " select AB.EMAIL_ADDRESS AS ASCEND_EMAIL_ADDRESS, AB.WORK_ORDER_ID, A.BUSINESS_UNIT_OM," & vbCrLf
            strAscendSql += " A.ORDER_NO FROM ps_isa_ord_intfc_H A," & vbCrLf
            strAscendSql += " sysadm.PS_ISA_INTFC_H_SUP AB" & vbCrLf
            strAscendSql += " where A.BUSINESS_UNIT_OM = AB.BUSINESS_UNIT_OM" & vbCrLf
            strAscendSql += " AND A.ORDER_NO = AB.ORDER_NO" & vbCrLf
            strAscendSql += " AND A.BUSINESS_UNIT_OM='" & strBu & "'" & vbCrLf
            strAscendSql += " AND A.ORDER_NO='" & strOrderNo & "'" & vbCrLf


            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If

            Dim dSet As New DataSet
            dSet = ORDBAccess.GetAdapter(strAscendSql, connectOR)
            If Not dSet Is Nothing Then
                If dSet.Tables.Count > 0 Then
                    If dSet.Tables(0).Rows.Count > 0 Then
                        If Not dSet.Tables(0).Rows(0).Item("ASCEND_EMAIL_ADDRESS") Is Nothing Then
                            If Trim(dSet.Tables(0).Rows(0).Item("ASCEND_EMAIL_ADDRESS")) <> "" Then
                                strAscendEmail = dSet.Tables(0).Rows(0).Item("ASCEND_EMAIL_ADDRESS")
                            End If
                        End If
                    End If
                End If
            End If

            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
        Catch ex As Exception
            strAscendEmail = ""
            Try

                If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                    connectOR.Close()
                Else
                    Try
                        connectOR.Close()
                    Catch ex2 As Exception

                    End Try
                End If
            Catch ex1 As Exception

            End Try
        End Try
        Return strAscendEmail
    End Function

    Private Sub sendCustEmail1(ByVal dsEmail As DataTable,
                          ByVal strOrderNo As String,
                          ByVal strCustID As String,
                          ByVal strCompanyID As String,
                          ByVal strOrderStatus As String,
                          ByVal strOrderStatDesc As String,
                          ByVal strInvID As String,
                          ByVal strLineNbr As String,
                          ByVal strFirstName As String,
                          ByVal strLastName As String,
                          ByVal strEmail As String, ByVal strorgin As String,
                          ByVal strBU As String, Optional ByVal strEmpID As String = "")

        Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
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
        Dim strccfirst1 As String = "erwin.bautista"  '  "pete.doyle"
        Dim strcclast1 As String = "sdi.com"
        Mailer1.From = "SDIExchange@SDI.com"  '  "Insiteonline@SDI.com"
        Mailer1.Cc = ""
        Mailer1.Bcc = strccfirst1 & "@" & strcclast1
        strbodyhead1 = "<table width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='82px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Arial; font-size: x-large; text-align: center;'>SDI Marketplace</span></center><center><span style='text-align: center; margin: 0px auto;'>SDiExchange - Order Status</span></center></td></tr></tbody></table>"
        strbodyhead1 = strbodyhead1 & "<HR width='100%' SIZE='1'>"
        strbodyhead1 = strbodyhead1 & "&nbsp;" & vbCrLf

        Dim dtgEmail1 As WebControls.DataGrid
        dtgEmail1 = New WebControls.DataGrid

        dsEmail = DuplicateRemoval(dsEmail)

        dtgEmail1.DataSource = dsEmail
        dtgEmail1.DataBind()
        dtgEmail1.BorderColor = Gray
        dtgEmail1.HeaderStyle.BackColor = System.Drawing.Color.LightGray
        dtgEmail1.HeaderStyle.Font.Bold = True
        dtgEmail1.HeaderStyle.ForeColor = Black
        WebControls.Unit.Percentage(90)
        dtgEmail1.CellPadding = 3
        'dtgEmail1.Width.Percentage(90)

        'dtgPO.Columns(9).ItemStyle.HorizontalAlign = HorizontalAlign.Center
        dtgEmail1.RenderControl(htmlTWnstk1)
        dataGridHTML1 = SBnstk1.ToString()

        ''Get Order Notes here
        Dim Ord_notes As String = ""
        Ord_notes = GetOrderNotes(strOrderNo, strBU)

        'Dim strPurchaserName As String = strCustID
        Dim strPurchaserName As String = strFirstName &
           " " & strLastName
        'Dim ted As String = ";erwin.bautista@sdi.com"
        Dim strPurchaserEmail As String = strEmail
        'Dim strPurchaserEmail As String = strEmail
        strbodydet1 = "&nbsp;" & vbCrLf
        strbodydet1 = strbodydet1 & "<div>"
        strbodydet1 = strbodydet1 & "<p >Hello " & strPurchaserName & ",</p>"
        'strbodydet1 = strbodydet1 & "&nbsp;<BR>" 
        strbodydet1 = strbodydet1 & "<p style='font-weight:bold;'>Order Number: " & strOrderNo & " </p> "

        If Not Ord_notes Is Nothing Then
            If Not (String.IsNullOrEmpty(Ord_notes.Trim())) Then
                strbodydet1 = strbodydet1 & "<p style='font-weight:bold;'>Customer Notes: " & Ord_notes & " </p> "
            End If
        End If

        strbodydet1 = strbodydet1 & "<p style='font-weight:bold;'>Order contents: </p>"
        'strbodydet1 = strbodydet1 & "&nbsp;<BR>"
        ' strbodydet1 = strbodydet1 & "Order Status:  " & strOrderStatDesc & " <br>"
        'strbodydet1 = strbodydet1 & "Order Number:  " & strOrderNo & " <br>"
        ' strbodydet1 = strbodydet1 & "Line Number:  " & strLineNbr & " <br>"
        strbodydet1 = strbodydet1 & "&nbsp;"
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
        strbodydet1 = strbodydet1 & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodydet1 = strbodydet1 & "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & vbCrLf

        Mailer1.Body = strbodyhead1 & strbodydet1
        Dim strPushNoti As String = ""
        If Not getDBName() Then
            Mailer1.To = "webdev@sdi.com"
            Mailer1.Subject = "<<TEST SITE>>SDiExchange - Order Status records for Order Number: " & strOrderNo

            strPushNoti = "<<TEST SITE>>Order Number: " + strOrderNo + " - Status Modified To  " + strOrderStatDesc + " . Please check the details in order status menu."
        Else
            Mailer1.To = strPurchaserEmail
            Mailer1.Subject = "SDiExchange - Order Status records for Order Number: " & strOrderNo

            strPushNoti = "Order Number: " + strOrderNo + " - Status Modified To " + strOrderStatDesc + ". Please check the details in order status menu."
        End If

        If Not strEmpID.Trim = "" Then
            sendNotification(strEmpID, strPushNoti, strOrderNo)
            Dim Title As String = "Order Number: " + strOrderNo + " - Status Modified To " + strOrderStatDesc + ""
            sendWebNotification(strEmpID, Title)
        End If
        Mailer1.BodyFormat = System.Web.Mail.MailFormat.Html

        SDIEmailService.EmailUtilityServices("MailandStore", Mailer1.From, Mailer1.To, Mailer1.Subject, String.Empty, "webdev@sdi.com", Mailer1.Body, "StatusChangeEmail1", MailAttachmentName, MailAttachmentbytes.ToArray())

    End Sub

    Public Sub sendWebNotification(ByVal Session_UserID As String, ByVal subject As String)
        Try
            Dim _notificationResult As New DataSet
            Dim notificationSQLStr = "select max(NOTIFY_ID) As NOTIFY_ID from SDIX_NOTIFY_QUEUE where USER_ID='" + Session_UserID + "'"
            _notificationResult = ORDBAccess.GetAdapter(notificationSQLStr, connectOR)
            Dim NotifyID As Int16 = 1
            If _notificationResult.Tables.Count > 0 Then
                Try
                    NotifyID = _notificationResult.Tables(0).Rows(0).Item("NOTIFY_ID")
                    NotifyID = NotifyID + 1
                Catch ex As Exception
                End Try
            End If
            connectOR.Open()
            Dim strSQLstring As String = "INSERT INTO SDIX_NOTIFY_QUEUE" & vbCrLf &
        " (NOTIFY_ID, NOTIFY_TYPE, USER_ID,DTTMADDED, STATUS,LINK, HTMLMSG, ATTACHMENTS, TITLE) VALUES ('" & NotifyID & "'," & vbCrLf &
        " 'ORD'," & vbCrLf &
        " '" & Session_UserID & "'," & vbCrLf &
        " sysdate," & vbCrLf &
        " 'N'," & vbCrLf &
         " ' ',' ',' '," & vbCrLf &
        " '" & subject & "')" & vbCrLf

            Dim command1 As OleDbCommand
            command1 = New OleDbCommand(strSQLstring, connectOR)
            Try
                Dim rowsaffected As Integer
                rowsaffected = command1.ExecuteNonQuery
                If Not rowsaffected = 1 Then

                End If
                command1.Dispose()
            Catch ex As Exception

            End Try
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try

        Catch ex As Exception
        End Try
    End Sub

    Private Function DuplicateRemoval(ByVal dt As DataTable)
        Dim dtbl As DataTable = New DataTable()
        dtbl = dt.Clone()
        dtbl.Rows.Clear()
        Dim list As New List(Of String) From {"01", "02", "03", "04",
                                              "05", "06", "07", "08",
                                              "09", "10", "11", "12",
                                              "13", "14", "15", "16"}
        Try
            Dim dr As DataRow
            For Each dr In dt.Rows
                Dim dts As DataTable = dt.AsEnumerable().Where(Function(x) x.Item("Line Number") = dr.Item("Line Number")).CopyToDataTable()

                If dts.Rows.Count() > 1 Then
                    Dim boolvalue As Boolean = False
                    Dim Nw_dts As DataTable = New DataTable()
                    Try
                        Nw_dts = dtbl.AsEnumerable().Where(Function(x) x.Item("Line Number") = dr.Item("Line Number")).CopyToDataTable()
                    Catch ex As Exception

                    End Try

                    If Nw_dts.Rows.Count() = 1 Then

                    Else
                        For Each drs As DataRow In dts.Rows
                            If list.Contains(drs.Item("Status Code")) Then
                                dtbl.ImportRow(drs)
                                boolvalue = True
                                Exit For
                            Else
                                boolvalue = False
                            End If
                        Next
                        If Not boolvalue Then
                            dtbl.ImportRow(dts.Rows(0))
                        End If
                    End If
                Else
                    dtbl.ImportRow(dr)
                End If
            Next
        Catch ex As Exception

        End Try
        Return dtbl
    End Function

    Private Function updateEnterprise(ByVal strBU As String, ByVal dteEndDate As DateTime) As Boolean
        connectOR.Close()
        Dim strSQLstring As String
        'Dim dteEndDate As DateTime
        Dim ds As New DataSet
        Dim bolerror1 As Boolean
        Dim dteJulian As Integer
        Dim dteStart As Date = "01/01/1900"
        Dim rowsaffected As Integer
        dteJulian = DateDiff(DateInterval.Day, dteStart, Now())

        Dim strXMLPath As String = rootDir & "\XMLOUT\ORDERSTATUS" & Convert.ToString(dteJulian) & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & ".xml"
        Dim objXMLWriter As XmlTextWriter


        ' The enddate coming from PS_ISAORDERSTATUSLOG  is being set back to the original enddate.  The PS_ISA_enterprise table
        ' is then updated with the PS_ISAORDERSTATUSLOG's endddate and the next time in, the date in the PS_ISA_enterprise table is
        ' the startdate.  We increased the enddate a second so we could get all the records from the query.  We were never getting
        ' the last record because of milliseconds were off in the date conversions.  Adding a second we were able to get all
        ' the records in the date range....  If you understand this you have a date to sit with the Dali Lama.. Believe me
        ' it works!!!!!!!!  PFD 4.4.2008
        ' reset the dteEndDate back to original

        dteEndDate.AddSeconds(-1)

        strSQLstring = "UPDATE PS_ISA_ENTERPRISE" & vbCrLf &
                    " SET ISA_LAST_STAT_SEND = TO_DATE('" & dteEndDate & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                    " WHERE ISA_BUSINESS_UNIT = '" & strBU & "' "

        Try
            Dim Command = New OleDbCommand(strSQLstring, connectOR)
            connectOR.Open()
            rowsaffected = Command.ExecuteNonQuery()
            connectOR.Close()
        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            objStreamWriter.WriteLine("  Error - updating the Enterprise send date " & OleDBExp.ToString)
            bolerror1 = True
            objXMLWriter.WriteEndElement()
            objXMLWriter.Flush()
            objXMLWriter.Close()
            Dim strXMLResult As String
            Dim objSR As StreamReader = File.OpenText(strXMLPath)
            strXMLResult = objSR.ReadToEnd()
            objSR.Close()
            objSR = Nothing
        End Try


        If bolerror1 = True Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Function getDBName() As Boolean
        Dim isPRODDB As Boolean = False
        Dim PRODDbList As String = ConfigurationManager.AppSettings("OraPRODDbList").ToString()
        Dim DbUrl As String = ConfigurationManager.AppSettings("OLEDBconString").ToString()
        Try
            DbUrl = DbUrl.Substring(DbUrl.Length - 4).ToUpper()
            isPRODDB = (PRODDbList.IndexOf(DbUrl.Trim.ToUpper) > -1)
        Catch ex As Exception
            isPRODDB = False
        End Try
        Return isPRODDB
    End Function

    Private Function getpo_id(ByVal strorderno As String, ByVal strlineno As String, ByVal strBU As String, ByVal strSiteBU As String) As String
        Dim I As Integer
        Dim strpo_no As String
        Dim strSQLstring As String = "SELECT PO_ID B " & vbCrLf &
                         "from " & vbCrLf &
                           "ps_po_line_distrib   " & vbCrLf &
                             " WHERE req_id= '" & strorderno & "' " & vbCrLf &
                              " AND req_line_nbr = " & CType(strlineno, Integer) & " " & vbCrLf &
                              "and Business_unit= '" & strSiteBU & "' " '& vbCrLf & _
        '" AND BUSINESS_UNIT_IN = '" & strBU & "' "

        Try
            objStreamWriter.WriteLine("  GetPO_ID: " & strSQLstring)

            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
            connectOR.Open()
            strpo_no = ORDBAccess.GetScalar(strSQLstring, connectOR)
            connectOR.Close()
        Catch ex As Exception
            Try
                connectOR.Close()
            Catch ex3 As Exception

            End Try
            strpo_no = 0
        End Try
        'Dim ds1 As DataSet = ORDBAccess.GetScalar(strSQLstring, connectOR)
        'strpo_no = ds1.Tables(0).Rows(I).Item("PO_ID")
        Return strpo_no
        '
    End Function

    Public Sub sendNotification(ByVal Session_UserID As String, ByVal subject As String, ByVal orderNo As String)
        Dim response As String
        Try
            Dim NotificationContent As String = subject
            Dim _notificationResult As New DataSet
            Dim notificationSQLStr = "SELECT DEVICE_INFO FROM SDIX_USER_TOKEN WHERE ISA_EMPLOYEE_ID = '" + Session_UserID + "' AND DEVICE_INFO IS NOT NULL AND LOWER(DEVICE_INFO) <> 'unknown unknown' AND LOWER(DEVICE_INFO)<>'webapp' "
            _notificationResult = ORDBAccess.GetAdapter(notificationSQLStr, connectOR)
            If _notificationResult.Tables.Count > 0 Then
                If _notificationResult.Tables(0).Rows.Count > 0 Then
                    Dim getTokenID As String() = _notificationResult.Tables(0).AsEnumerable().[Select](Function(r) r.Field(Of String)("DEVICE_INFO")).ToArray()
                    Dim serverKey As String = ConfigurationManager.AppSettings("serverKey")
                    Dim senderId As String = ConfigurationManager.AppSettings("senderId")
                    Dim tRequest As WebRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send")
                    tRequest.Method = "post"
                    tRequest.ContentType = "application/json"
                    Dim webObject As New WebRequestFcmData
                    With webObject
                        .registration_ids = getTokenID
                        .notification.body = subject
                        .notification.sound = "Enabled"
                        .data.orderid = orderNo
                    End With
                    Dim serializer = New JavaScriptSerializer()
                    Dim json = serializer.Serialize(webObject)
                    Dim byteArray As Byte() = Encoding.UTF8.GetBytes(json)
                    tRequest.Headers.Add(String.Format("Authorization: key={0}", serverKey))
                    tRequest.Headers.Add(String.Format("Sender: id={0}", senderId))

                    tRequest.ContentLength = byteArray.Length

                    Using dataStream As Stream = tRequest.GetRequestStream()
                        dataStream.Write(byteArray, 0, byteArray.Length)

                        Using tResponse As WebResponse = tRequest.GetResponse()

                            Using dataStreamResponse As Stream = tResponse.GetResponseStream()

                                Using tReader As StreamReader = New StreamReader(dataStreamResponse)
                                    Dim sResponseFromServer As String = tReader.ReadToEnd()
                                    response = sResponseFromServer
                                End Using
                            End Using
                        End Using
                    End Using
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Public Function PurchaseOrderNo(ByVal workOrder As String, THIRDPARTY_COMP_ID As String) As String
        Try
            Dim APIresponse = String.Empty
            Dim objWorkOrderDetails As New WorkOrderDetails
            'Commented the CBRE Authentication for getting work order details
            If Not String.IsNullOrEmpty(THIRDPARTY_COMP_ID) Then
                If THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                    APIresponse = AuthenticateService("CBRE")
                Else
                    APIresponse = AuthenticateService("Walmart")
                End If
            Else
                APIresponse = AuthenticateService("Walmart")
            End If
            ' APIresponse = Await AuthenticateService(Walmart)
            If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                If (Not APIresponse.Contains("error_description")) Then
                    Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                    Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/" + workOrder
                    Dim httpClient As HttpClient = New HttpClient()
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                    httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                    Dim response = httpClient.GetAsync(apiURL).Result
                    If response.IsSuccessStatusCode Then
                        Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                        If workorderAPIResponse <> "[]" And Not String.IsNullOrEmpty(workorderAPIResponse) And Not String.IsNullOrWhiteSpace(workorderAPIResponse) Then
                            objWorkOrderDetails = JsonConvert.DeserializeObject(Of WorkOrderDetails)(workorderAPIResponse)
                            Return objWorkOrderDetails.PurchaseNumber
                            objWalmartSC.WriteLine("Method: PurchaseOrderNo() Result-" + Convert.ToString(objWorkOrderDetails.PurchaseNumber))
                        Else
                            objWalmartSC.WriteLine("Method: PurchaseOrderNo() Result- Failed in API response")
                            Return "Failed"
                        End If
                    Else
                        Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                        objWalmartSC.WriteLine("Method: PurchaseOrderNo() Result- Failed in API response")
                        Return "Failed"
                    End If
                End If
            End If
        Catch ex As Exception
            Return "Failed"
            objWalmartSC.WriteLine("Method:PurchaseOrderNo - " + ex.Message)
        End Try
    End Function
    Public Function CheckWorkOrderStatus(ByVal workOrder As String, THIRDPARTY_COMP_ID As String) As String
        Try
            Dim APIresponse As String = String.Empty
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then
                If Not String.IsNullOrEmpty(THIRDPARTY_COMP_ID) Then
                    If THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                        APIresponse = AuthenticateService("CBRE")
                    Else
                        APIresponse = AuthenticateService("Walmart")
                    End If
                End If
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/odata/" + "/workorders(" + workOrder + ")?$select=Status"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim response = httpClient.GetAsync(apiURL).Result
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().result
                            Dim objCheckWo As CheckWo = JsonConvert.DeserializeObject(Of CheckWo)(workorderAPIResponse)
                            Return objCheckWo.Status.Primary
                            objWalmartSC.WriteLine("Method: CheckWorkOrderStatus() Result-" + Convert.ToString(objCheckWo.Status.Extended))
                        Else
                            objWalmartSC.WriteLine("Method: CheckWorkOrderStatus() Result- Failed in API response")
                            Return "Failed"
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            Return "Failed"
            objWalmartSC.WriteLine("Method:CheckWorkOrderStatus - " + ex.Message)
        End Try
    End Function

    Public Function UpdateWorkOrderStatus(ByVal workOrder As String, credType As String, status As String) As String
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then
                Dim APIresponse = AuthenticateService(credType)
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/" + workOrder + "/status"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        'Dim response = httpClient.GetAsync(apiURL).Result
                        Dim objPartParam As New UpdateWorkOrderBO
                        objPartParam.Note = String.Empty
                        objPartParam.Status = New Status
                        objPartParam.Status.Primary = "In Progress"
                        objPartParam.Status.Extended = status
                        Dim serializedparameter = JsonConvert.SerializeObject(objPartParam)
                        Dim response = httpClient.PutAsync(apiURL, New StringContent(serializedparameter, Encoding.UTF8, "application/json")).Result
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().result
                            objWalmartSC.WriteLine("Result-" + Convert.ToString(workorderAPIResponse))
                            Return "Success"
                        Else
                            objWalmartSC.WriteLine("Result- Failed in API response")
                            Return "Failed"
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            Return "Failed"
            objWalmartSC.WriteLine("Method:UpdateWorkOrderStatus - " + ex.Message)
        End Try
    End Function

    Public Function AuthenticateService(credType As String) As String
        Try
            Dim httpClient As HttpClient = New HttpClient()
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            Dim username As String = String.Empty
            Dim password As String = String.Empty
            Dim clientKey As String = String.Empty
            If credType = "Walmart" Then
                username = ConfigurationManager.AppSettings("WMUName")
                password = ConfigurationManager.AppSettings("WMPassword")
                clientKey = ConfigurationManager.AppSettings("WMClientKey")
            Else
                username = ConfigurationManager.AppSettings("CBREUName")
                password = ConfigurationManager.AppSettings("CBREPassword")
                clientKey = ConfigurationManager.AppSettings("CBREClientKey")
            End If
            Dim apiurl As String = ConfigurationManager.AppSettings("ServiceChannelLoginEndPoint")
            Dim formContent = New FormUrlEncodedContent({New KeyValuePair(Of String, String)("username", username), New KeyValuePair(Of String, String)("password", password), New KeyValuePair(Of String, String)("grant_type", "password")})
            httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Basic", clientKey) 'Add("Authorization", "Basic " + clientKey)
            Dim response = httpClient.PostAsync(apiurl, formContent).Result
            If response.IsSuccessStatusCode Then
                Dim APIResponse = response.Content.ReadAsStringAsync().Result
                Return APIResponse
            Else
                Dim APIResponse = response.Content.ReadAsStringAsync().Result
                'Dim eobj As ExceptionHelper = New ExceptionHelper()
                'eobj.writeExceptionMessage(APIResponse, "AuthenticateService")
                If APIResponse.Contains("error_description") Then Return APIResponse
                Return "Server Error"
            End If

        Catch ex As Exception
            objWalmartSC.WriteLine("Method:AuthenticateService - " + ex.Message)
        End Try
        Return "Server Error"
    End Function

    Public Function OrdrStatus(orderno As String) As String
        Try
            If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                connectOR.Close()
            End If
            connectOR.Open()
            Dim orderDetail As String = String.Empty
            Dim qString As String = "select sysadm8.ord_stat_summary('" + orderno + "') from dual"
            orderDetail = ORDBAccess.GetScalar(qString, connectOR)
            Return orderDetail
        Catch ex As Exception
            objWalmartSC.WriteLine("Method: OrdrStatus(): " + Convert.ToString(ex.Message))
        End Try

    End Function

    Public Function GetWorkOrderParts(ByVal workOrder As String) As String
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then
                Dim APIresponse = AuthenticateService("Walmart")
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/" + workOrder + "/parts"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim response = httpClient.GetAsync(apiURL).Result
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                            If workorderAPIResponse <> "[]" And Not String.IsNullOrEmpty(workorderAPIResponse) And Not String.IsNullOrWhiteSpace(workorderAPIResponse) Then
                                'Return workorderAPIResponse
                                Dim objWorkOrder As List(Of WorkOrderParts) = JsonConvert.DeserializeObject(Of List(Of WorkOrderParts))(workorderAPIResponse)
                                Dim deletearrayOfID As New List(Of Int32)
                                Dim objWorkOrderParts As WorkOrderParts = objWorkOrder.FirstOrDefault()
                                If Not objWorkOrderParts Is Nothing Then
                                    deletearrayOfID.Add(objWorkOrderParts.id)
                                    DeleteWorkOrder(workOrder, deletearrayOfID.ToArray())
                                End If
                            Else
                                'Dim eobj As ExceptionHelper = New ExceptionHelper()
                                'eobj.writeExceptionMessage(workorderAPIResponse, "GetWorkOrderParts")
                                Return String.Empty
                            End If
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                            'Dim eobj As ExceptionHelper = New ExceptionHelper()
                            'eobj.writeExceptionMessage(workorderAPIResponse, "GetWorkOrderParts")
                            Return String.Empty
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
        Return String.Empty
    End Function

    Public Function DeleteWorkOrder(ByVal workOrder As String, ByVal objPartParam As Integer()) As String
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) And objPartParam.Count() > 0 Then
                Dim APIresponse = AuthenticateService("Walmart")
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim querySting As String = String.Empty
                        For Each items As Integer In objPartParam
                            querySting = String.Concat(querySting, "ids=" + items.ToString() + "&")
                        Next
                        querySting = querySting.Substring(0, querySting.LastIndexOf("&"))
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/" + workOrder + "/parts?" + querySting + ""
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim response = httpClient.DeleteAsync(apiURL).Result
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                            'Dim eobj As ExceptionHelper = New ExceptionHelper()
                            'eobj.writeExceptionMessage(workorderAPIResponse, "DeleteWorkOrder")
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            'Dim eobj As ExceptionHelper = New ExceptionHelper()
            'eobj.writeException(ex)
        End Try
    End Function

    Public Sub InsertWorkOrder(workOrder As String)
        Try


            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then
                Dim APIresponse = AuthenticateService("Walmart")
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/inventory/parts/bulkPartUsage"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim objInserWorOrdeParts As New InsertWorkOrderPartsBO
                        objInserWorOrdeParts.AddItems = New List(Of AddItem)



                        objInserWorOrdeParts.AddItems.Add(New AddItem() With {
                                .RecId = workOrder,
                             .Description = "SCTest",
                             .Quantity = "1",
                             .UnitCost = "1",
                             .PartNumber = "WM00455570:1"
                            })

                        Dim serializedparameter = JsonConvert.SerializeObject(objInserWorOrdeParts)
                        Dim response = httpClient.PostAsync(apiURL, New StringContent(serializedparameter, Encoding.UTF8, "application/json")).Result()
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            'UpdateWorkOrderStatus(workOrder, Walmart)
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()

                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            Throw ex
        End Try
    End Sub
End Module

Public Class AddItem
    Public Property RecId As String = String.Empty
    Public Property Quantity As Double
    Public Property UnitCost As Double
    Public Property UseDate As String = DateTime.Now.ToString()
    Public Property PartNumber As String = String.Empty
    Public Property Description As String = String.Empty
End Class

Public Class InsertWorkOrderPartsBO
    Public Property AddItems As List(Of AddItem)
    Public Property UpdateItems As List(Of Object) = New List(Of Object)
    Public Property DeleteItems As List(Of Object) = New List(Of Object)
    Public Property IsLocalTime As Boolean = True
End Class

Public Class WorkOrderParts
    Public Property id As Integer
    Public Property Quantity As Double
    Public Property Description As String = String.Empty
    Public Property Price As Double
    Public Property SupplierPartId As String = String.Empty
End Class

Public Class WebRequestFcmData
    Public Property registration_ids As String()
    Public Property notification As New NotificationData
    Public Property data As New dataBO
End Class

Public Class dataBO
    Public Property orderid As String
End Class

Public Class NotificationData
    Public Property body As String
    Public Property title As String = "ZEUS"
    Public Property sound As String
End Class

Public Class OrderStatusDetail
    Public Property message As String
    Public Property orderStatus As String
    Public Property statusDesc As String
    Public Property dueDate As String
End Class

Public Class ValidateUserResponseBO
    Public Property access_token As String
    Public Property refresh_token As String
End Class

Public Class UpdateWorkOrderBO
    Public Property Status As Status
    Public Property Note As String
End Class

Public Class Status
    Public Property Primary As String
    Public Property Extended As String
End Class
Public Class Location
    Public Property StoreId As String = String.Empty
End Class

Public Class Notes
    Public Property Last As Last
End Class

Public Class Last
    Public Property NoteData As String = String.Empty
End Class

Public Class Asset
    Public Property Tag As String = String.Empty
End Class

Public Class WorkOrderDetails
    Public Property Notes As Notes
    Public Property Location As Location
    Public Property Asset As Asset
    Public Property PurchaseNumber As String = String.Empty

End Class

Public Class WOStatus
    Public Property Primary As String
    Public Property Extended As String
    Public Property CanCreateInvoice As String
End Class


Public Class CheckWo
    Public Property OdataContext As String
    Public Property Status As WOStatus
End Class
