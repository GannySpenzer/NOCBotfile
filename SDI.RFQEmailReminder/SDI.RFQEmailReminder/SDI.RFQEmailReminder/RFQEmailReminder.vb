Imports System.Configuration
Imports System.IO
Imports System.Data.OleDb
Imports System.Xml
Imports System.Web
Imports System.Text
Imports System.Web.UI
Imports System.Web.Mail
Imports System.Drawing
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls
Module RFQEmailReminder
    Dim rootDir As String = ConfigurationManager.AppSettings("LogPath")
    Dim logpath As String = rootDir & "RFQExpireNotification" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString")))
    Dim configPath As New String(Convert.ToString(ConfigurationManager.AppSettings("configPath")))
    Dim todayDate As String = String.Format("{0:yyyy/MM/dd}", DateTime.Now)
    Dim Oldlist_1 As New List(Of String)
    Dim Oldlist_2 As New List(Of String)
    Dim Oldlist_3 As New List(Of String)
    Dim Oldlist_4 As New List(Of String)
    Dim Oldlist_5 As New List(Of String)
    Dim sourcedt As Date
    Dim expiredt As Date
    Dim todaydt As Date
    Dim ds As DataSet
    Dim data As DataTable
    Dim dr As DataRow
    Dim myList As New List(Of String)
    Dim OrderNoList As New List(Of String)

    Sub Main()
        Console.WriteLine("Start Expire RFQ Email Notification")
        Dim log As StreamWriter
        Dim fileStream As FileStream = Nothing
        Dim logDirInfo As DirectoryInfo = Nothing
        Dim logFileInfo As FileInfo = Nothing
        logFileInfo = New FileInfo(logpath)
        logDirInfo = New DirectoryInfo(logFileInfo.DirectoryName)
        If Not logDirInfo.Exists Then logDirInfo.Create()

        If Not logFileInfo.Exists Then
            fileStream = logFileInfo.Create()
        Else
            fileStream = New FileStream(logpath, FileMode.Append)
        End If

        log = New StreamWriter(fileStream)
        log.WriteLine("*********************Logs(" + String.Format(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")) + ")***********************************")
        log = buildrfqemail(log)
        Try
            log.WriteLine("********************End of Expire RFQ Email Notification Utility********************")
            Console.WriteLine("********************End of Expire RFQ Email Notification Utility********************")
            log.Close()
        Catch ex As Exception

        End Try

    End Sub
    Public Function buildrfqemail(log As StreamWriter) As StreamWriter

        Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
        connectOR = New OleDbConnection(connectionString)


        ' method to call the 
        Dim strSQLString As String = ""
        Dim strSQLString1 As String = ""
        Dim strSQLString2 As String = ""
        Dim strSQLString3 As String = ""
        Dim strSQLstringUpt As String
        Dim strOneString As String = "0"
        Dim varResultCount As String = "0"
        Dim varResultMode As String = "0"
        Dim Ordlist_31 As New List(Of String)
        Dim OrderNo As String = " "
        Dim LineNo As String = " "
        Dim Expiredatedata As String = String.Empty
        Dim EmployeeId As String = " "
        Dim Linestatus As String = " "
        Dim approverid As String = String.Empty
        Dim I As Integer
        Dim expdatevalue As String

        strSQLString = "SELECT Q.EXPIRE_DT,TO_CHAR(LG.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP ,TO_CHAR(HD.order_date, 'MM/DD/YYYY') AS order_date,TO_CHAR(LN.ISA_REQUIRED_BY_DT, 'MM/DD/YYYY') AS ISA_REQUIRED_BY_DT, LN.* " & vbCrLf &
       " From PS_ISA_ORD_INTF_LN LN,PS_ISA_ORD_INTF_HD HD,PS_ISAORDSTATUSLOG LG,SYSADM8.PS_ISA_ENTERPRISE EN,PS_ISA_NSTK_QUOTE Q WHERE LN.ORDER_NO = HD.ORDER_NO AND " & vbCrLf &
          "LN.ISA_LINE_STATUS IN 'QTS' AND LG.ORDER_NO = LN.ORDER_NO AND LN.ORDER_NO = Q.REQ_ID AND  LN.BUSINESS_UNIT_PO = Q.BUSINESS_UNIT AND LN.ISA_INTFC_LN = Q.LINE_NBR AND LN.VENDOR_ID = Q.VENDOR_ID " & vbCrLf &
           "AND EN.ISA_QUOTE_EXP_DAYS = '1' AND LN.BUSINESS_UNIT_OM = EN.ISA_BUSINESS_UNIT AND LG.ISA_INTFC_LN = LN.ISA_INTFC_LN AND Q.EXPIRE_DT IS NOT NULL AND LG.ISA_LINE_STATUS = LN.ISA_LINE_STATUS " & vbCrLf &
           " ORDER BY LG.DTTM_STAMP desc" & vbCrLf

        Dim dtrAppReader As OleDbDataReader = GetReader(strSQLString)
        Dim dtQTS = New DataTable
        dtQTS.Columns.Add("Order Number")
        dtQTS.Columns.Add("Expiry date")
        If dtrAppReader.HasRows() = True Then

            While dtrAppReader.Read()
                OrderNo = Convert.ToString(dtrAppReader.Item("ORDER_NO"))
                Expiredatedata = Convert.ToString(dtrAppReader.Item("EXPIRE_DT"))

                Try
                    Dim row = (From d In dtQTS.AsEnumerable Where d.Field(Of String)("Expiry date") = Expiredatedata And d.Field(Of String)("Order Number") = OrderNo)
                    If row.Any() Then

                    Else
                        log.WriteLine("------------------------------------------------QTS--------------------------------------------------------------------")
                        log.WriteLine("--------------------------------OrderNo:" + Convert.ToString(OrderNo) + "--Expiredatedata:" + Convert.ToString(Expiredatedata) + "-----------------------------------------------")
                        dr = dtQTS.NewRow()
                        dr("Order Number") = OrderNo
                        dr("Expiry date") = Expiredatedata
                        dtQTS.Rows.Add(dr)
                        LineNo = Convert.ToString(dtrAppReader.Item("ISA_INTFC_LN"))
                        EmployeeId = Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_ID"))
                        Dim BU As String = Convert.ToString(dtrAppReader.Item("Business_Unit_Om"))
                        Dim OPRID_Appr_By As String = Convert.ToString(dtrAppReader.Item("OPRID_APPROVED_BY"))
                        Dim Appr_Dttm As String = Convert.ToString(dtrAppReader.Item("APPROVAL_DTTM"))
                        Dim ITM_SETID As String = Convert.ToString(dtrAppReader.Item("ITM_SETID"))
                        Dim SELL_Price As String = Convert.ToString(dtrAppReader.Item("ISA_SELL_PRICE"))
                        Dim Required_By_Dttm As String = Convert.ToString(dtrAppReader.Item("ISA_REQUIRED_BY_DT"))
                        Dim Decriptions As String = Convert.ToString(dtrAppReader.Item("DESCR254"))
                        Dim Order_date As String = Convert.ToString(dtrAppReader.Item("order_date"))
                        Dim Vendor_id As String = Convert.ToString(dtrAppReader.Item("VENDOR_ID"))
                        Linestatus = Convert.ToString(dtrAppReader.Item("ISA_LINE_STATUS"))
                        todaydt = Date.Parse(todayDate)
                        strSQLString1 = "select TO_CHAR(LN.ADD_DTTM,'YYYY/MM/DD') AS SOURCE_DATE from PS_ISA_ORD_INTF_LN LN where order_no= '" & OrderNo & "' and ISA_INTFC_LN= '" & LineNo & "'"
                        Dim Sourcedate As String = GetScalar(strSQLString1)
                        sourcedt = Date.Parse(Sourcedate)
                        strSQLString2 = "select TO_CHAR(A.EXPIRE_DT,'YYYY/MM/DD') AS EXPIRE_DT from PS_ISA_NSTK_QUOTE A, PS_REQ_LINE B " & vbCrLf &
                                    "where A.req_id = '" & OrderNo & "' and A.Line_nbr= '" & LineNo & "'" & vbCrLf &
                       "And A.Business_unit = B.Business_unit And A.Req_id = B.Req_id And A.VENDOR_ID = '" & Vendor_id & "' AND A.VENDOR_ID = B.VENDOR_ID And A.Line_nbr = B.Line_nbr" & vbCrLf
                        Dim Expiredate As String = GetScalar(strSQLString2)
                        log.WriteLine("OrderNo:" + Convert.ToString(OrderNo) + "--Sourcedate:" + Convert.ToString(sourcedt) + "--Expiredate:" + Convert.ToString(Expiredate))
                        If Expiredate = "" Or Sourcedate = "" Then

                        Else
                            ''INC0013167-Madhu-Cancel status additional changes
                            expiredt = Date.Parse(Expiredate)
                            If Expiredate < todayDate Then
                                Dim strOraSelectQuery As String = "SELECT S.DTTM_STAMP, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S,PS_ISA_NSTK_QUOTE Q" & vbCrLf &
"WHERE a.isa_line_status IN ('QTS','QTW') AND A.ORDER_NO= '" & OrderNo & "' AND A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln " & vbCrLf &
"AND a.isa_line_status = S.isa_line_status AND A.ORDER_NO=Q.REQ_ID AND A.ISA_INTFC_LN = Q.LINE_NBR AND TO_CHAR(Q.EXPIRE_DT,'YYYY/MM/DD')= '" & Expiredate & "'" & vbCrLf

                                Dim dsOrdLnItems As DataSet = GetAdapter(strOraSelectQuery)

                                If dsOrdLnItems.Tables(0).Rows.Count > 0 Then
                                    For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows
                                        OrderNo = CType(dataRowMain("ORDER_NO"), String)
                                        LineNo = CType(dataRowMain("ISA_INTFC_LN"), String)
                                        Dim trnsactSession As OleDbTransaction = Nothing
                                        Dim connection As OleDbConnection = New OleDbConnection(ConfigurationManager.AppSettings("OLEDBconString"))
                                        Dim rowsAffected As Integer = 0
                                        strSQLstringUpt = "UPDATE PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = 'CNC',CHANGE_FLAG = 'Y',PROCESS_INSTANCE = 0,LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'),OPRID_MODIFIED_BY = 'AVACORP'  WHERE ISA_LINE_STATUS = 'QTS' AND ORDER_NO= '" & OrderNo & "' AND ISA_INTFC_LN= '" & LineNo & "' "
                                        Try
                                            connection.Open()
                                            trnsactSession = connection.BeginTransaction
                                            Dim Command As OleDbCommand = New OleDbCommand(strSQLstringUpt, connection)
                                            Command.Transaction = trnsactSession
                                            'Command.CommandTimeout = 120
                                            rowsAffected = Command.ExecuteNonQuery()
                                            If Not rowsAffected = 0 Then
                                                log.WriteLine("Updated the order status to ""CNC"" for OrderNo {0}, LineNo {1}, that is not approved by employee {2}", OrderNo, LineNo, EmployeeId)
                                            Else
                                                log.WriteLine("Error in updating order status to ""CNC"" for OrderNo {0},LineNo {1}, that is not approved by employee {2} ", OrderNo, LineNo, EmployeeId)
                                            End If
                                            Try
                                                Command.Dispose()
                                            Catch ex As Exception

                                            End Try

                                            trnsactSession.Commit()
                                            connection.Close()
                                            trnsactSession = Nothing
                                            connection = Nothing
                                        Catch objException As Exception
                                            log.WriteLine("Error in updating order status to ""CNC"" for OrderNo {0} ", OrderNo)
                                            rowsAffected = 0
                                            Try
                                                trnsactSession.Rollback()
                                                connection.Close()
                                                trnsactSession = Nothing
                                                connection = Nothing
                                            Catch ex As Exception

                                            End Try
                                        End Try
                                    Next
                                End If
                            Else
                                Dim One As Int64 = 1
                                Dim Diff As System.TimeSpan = expiredt.Subtract(sourcedt)
                                Dim Diff1 As System.TimeSpan = todaydt.Subtract(sourcedt)
                                Dim difOfSrcExpDt As Int64 = Diff.TotalDays
                                Dim difOfSrcCurDt As Int64 = Diff1.TotalDays
                                Dim fiftyPerdiff As Int64
                                Dim seventyfivePerdiff As Int64
                                Console.WriteLine("Total Days:" + Convert.ToString(difOfSrcExpDt))
                                fiftyPerdiff = difOfSrcExpDt / 2
                                seventyfivePerdiff = difOfSrcExpDt * 3 / 4
                                Dim DtofFiftypercent As Date = sourcedt.AddDays(fiftyPerdiff)
                                Dim Dtofseventyfivepercent As Date = sourcedt.AddDays(seventyfivePerdiff)
                                log.WriteLine("OrderNo:" + Convert.ToString(OrderNo) + "--Fiftypercent:" + Convert.ToString(DtofFiftypercent) + "--seventyfivepercent:" + Convert.ToString(Dtofseventyfivepercent) + "--expiredt:" + Convert.ToString(expiredt))
                                If todaydt = DtofFiftypercent Then
                                    log.WriteLine("Start to fetch the old orders that is not approved for 50% completion of expiry date")
                                    commonMethod(fiftyPerdiff, OrderNo, EmployeeId, varResultCount, log, "fiftypercent", "Day", difOfSrcExpDt, BU, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm, Decriptions, Order_date, Expiredate, Linestatus)
                                    log.WriteLine("---------------------****End of 50% of process****-----------------------")
                                ElseIf todaydt = Dtofseventyfivepercent Then
                                    log.WriteLine("Start to fetch the old orders that is not approved for 75% completion of expiry date")
                                    commonMethod(seventyfivePerdiff, OrderNo, EmployeeId, varResultCount, log, "seventyfivepercent", "Day", difOfSrcExpDt, BU, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm, Decriptions, Order_date, Expiredate, Linestatus)
                                    log.WriteLine("---------------------****End of 75% of process****-----------------------")
                                ElseIf todaydt = expiredt Then
                                    log.WriteLine("Start to fetch the old orders that is not approved for last date of expiry date")
                                    commonMethod(One, OrderNo, EmployeeId, varResultCount, log, "lastday", "lastDay", One, BU, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm, Decriptions, Order_date, Expiredate, Linestatus)
                                    log.WriteLine("---------------------****End of last day of process****-----------------------")
                                Else
                                    log.WriteLine(Convert.ToString(OrderNo) + "No records found to send reminder email for order")
                                End If
                            End If
                        End If
                    End If

                Catch ex As Exception

                    log.WriteLine(Convert.ToString(OrderNo) + "Issue Occured")
                End Try

            End While

            dtrAppReader.Close()
        Else
            log.WriteLine("No records found to expire order- QTS")
            dtrAppReader.Close()
        End If

        'For QTW Orders

        strSQLString3 = "SELECT Q.EXPIRE_DT, TO_CHAR(LG.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP ,TO_CHAR(HD.order_date, 'MM/DD/YYYY') AS order_date,TO_CHAR(LN.ISA_REQUIRED_BY_DT, 'MM/DD/YYYY') AS ISA_REQUIRED_BY_DT, LN.* " & vbCrLf &
       " From PS_ISA_ORD_INTF_LN LN,PS_ISA_ORD_INTF_HD HD,PS_ISAORDSTATUSLOG LG,SYSADM8.PS_ISA_ENTERPRISE EN,PS_ISA_NSTK_QUOTE Q WHERE LN.ORDER_NO = HD.ORDER_NO AND " & vbCrLf &
          "LN.ISA_LINE_STATUS = 'QTW' AND LG.ORDER_NO = LN.ORDER_NO AND LN.ORDER_NO = Q.REQ_ID AND  LN.BUSINESS_UNIT_PO = Q.BUSINESS_UNIT AND LN.ISA_INTFC_LN = Q.LINE_NBR AND LN.VENDOR_ID = Q.VENDOR_ID " & vbCrLf &
           "AND EN.ISA_QUOTE_EXP_DAYS = '1' AND LN.BUSINESS_UNIT_OM = EN.ISA_BUSINESS_UNIT AND LG.ISA_INTFC_LN = LN.ISA_INTFC_LN AND Q.EXPIRE_DT IS NOT NULL AND LG.ISA_LINE_STATUS = LN.ISA_LINE_STATUS " & vbCrLf &
           "ORDER BY LG.DTTM_STAMP desc" & vbCrLf

        Dim dtrAppReader1 As OleDbDataReader = GetReader(strSQLString3)
        Dim dtQTW = New DataTable
        dtQTW.Columns.Add("Order Number")
        dtQTW.Columns.Add("Expiry date")
        log.WriteLine("-----------------------------------------------------------QTW------------------------------------------------------------------------")
        log.WriteLine("--------------------------------OrderNo:" + Convert.ToString(OrderNo) + "--Expiredatedata:" + Convert.ToString(Expiredatedata) + "-----------------------------------------------")
        If dtrAppReader1.HasRows() = True Then
            While dtrAppReader1.Read()
                OrderNo = Convert.ToString(dtrAppReader1.Item("ORDER_NO"))
                Expiredatedata = Convert.ToString(dtrAppReader1.Item("EXPIRE_DT"))
                Try
                    Dim row = (From d In dtQTW.AsEnumerable Where d.Field(Of String)("Expiry date") = Expiredatedata And d.Field(Of String)("Order Number") = OrderNo)
                    If row.Any() Then

                    Else
                        dr = dtQTW.NewRow()
                        dr("Order Number") = OrderNo
                        dr("Expiry date") = Expiredatedata
                        dtQTW.Rows.Add(dr)
                        LineNo = Convert.ToString(dtrAppReader1.Item("ISA_INTFC_LN"))
                        EmployeeId = Convert.ToString(dtrAppReader1.Item("ISA_EMPLOYEE_ID"))
                        Dim BU As String = Convert.ToString(dtrAppReader1.Item("Business_Unit_Om"))
                        Dim OPRID_Appr_By As String = Convert.ToString(dtrAppReader1.Item("OPRID_APPROVED_BY"))
                        Dim Appr_Dttm As String = Convert.ToString(dtrAppReader1.Item("APPROVAL_DTTM"))
                        Dim ITM_SETID As String = Convert.ToString(dtrAppReader1.Item("ITM_SETID"))
                        Dim SELL_Price As String = Convert.ToString(dtrAppReader1.Item("ISA_SELL_PRICE"))
                        Dim Required_By_Dttm As String = Convert.ToString(dtrAppReader1.Item("ISA_REQUIRED_BY_DT"))
                        Dim Decriptions As String = Convert.ToString(dtrAppReader1.Item("DESCR254"))
                        Dim Order_date As String = Convert.ToString(dtrAppReader1.Item("order_date"))
                        Dim Vendor_id As String = Convert.ToString(dtrAppReader1.Item("VENDOR_ID"))
                        Linestatus = Convert.ToString(dtrAppReader1.Item("ISA_LINE_STATUS"))
                        todaydt = Date.Parse(todayDate)

                        strSQLString1 = "select TO_CHAR(LN.ADD_DTTM,'YYYY/MM/DD') AS SOURCE_DATE from PS_ISA_ORD_INTF_LN LN where order_no= '" & OrderNo & "' and ISA_INTFC_LN= '" & LineNo & "'"

                        Dim Sourcedate As String = GetScalar(strSQLString1)
                        sourcedt = Date.Parse(Sourcedate)

                        strSQLString2 = "select TO_CHAR(A.EXPIRE_DT,'YYYY/MM/DD') AS EXPIRE_DT from PS_ISA_NSTK_QUOTE A, PS_REQ_LINE B " & vbCrLf &
                                    "where A.req_id = '" & OrderNo & "' and A.Line_nbr= '" & LineNo & "'" & vbCrLf &
                       "And A.Business_unit = B.Business_unit And A.Req_id = B.Req_id And A.VENDOR_ID = '" & Vendor_id & "' AND A.VENDOR_ID = B.VENDOR_ID And A.Line_nbr = B.Line_nbr" & vbCrLf

                        Dim Expiredate As String = GetScalar(strSQLString2)


                        If Expiredate = "" Or Sourcedate = "" Then

                        Else
                            'INC0013167-Madhu-Cancel status additional changes
                            expiredt = Date.Parse(Expiredate)
                            If Expiredate < todayDate Then
                                Dim strOraSelectQuery As String = "SELECT S.DTTM_STAMP, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S,PS_ISA_NSTK_QUOTE Q" & vbCrLf &
"WHERE a.isa_line_status IN ('QTS','QTW') AND A.ORDER_NO= '" & OrderNo & "' AND A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln " & vbCrLf &
"AND a.isa_line_status = S.isa_line_status AND A.ORDER_NO=Q.REQ_ID AND A.ISA_INTFC_LN = Q.LINE_NBR AND TO_CHAR(Q.EXPIRE_DT,'YYYY/MM/DD')= '" & Expiredate & "'" & vbCrLf

                                Dim dsOrdLnItems As DataSet = GetAdapter(strOraSelectQuery)
                                If dsOrdLnItems.Tables(0).Rows.Count > 0 Then
                                    For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows
                                        OrderNo = CType(dataRowMain("ORDER_NO"), String)
                                        LineNo = CType(dataRowMain("ISA_INTFC_LN"), String)
                                        Dim trnsactSession As OleDbTransaction = Nothing
                                        Dim connection As OleDbConnection = New OleDbConnection(ConfigurationManager.AppSettings("OLEDBconString"))
                                        Dim rowsAffected As Integer = 0
                                        strSQLstringUpt = "UPDATE PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = 'CNC',CHANGE_FLAG = 'Y',PROCESS_INSTANCE = 0,LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'),OPRID_MODIFIED_BY = 'AVACORP'  WHERE ISA_LINE_STATUS = 'QTW' AND ORDER_NO= '" & OrderNo & "' AND ISA_INTFC_LN= '" & LineNo & "'"
                                        Try
                                            connection.Open()
                                            trnsactSession = connection.BeginTransaction
                                            Dim Command As OleDbCommand = New OleDbCommand(strSQLstringUpt, connection)
                                            Command.Transaction = trnsactSession
                                            'Command.CommandTimeout = 120
                                            rowsAffected = Command.ExecuteNonQuery()
                                            If Not rowsAffected = 0 Then
                                                log.WriteLine("Updated the order status to ""CNC"" for OrderNo {0}, LineNo {1}, that is not approved by employee {2}", OrderNo, LineNo, EmployeeId)
                                            Else
                                                log.WriteLine("Updated the order status to ""CNC"" for OrderNo {0}, LineNo {1}, that is not approved by employee {2}", OrderNo, LineNo, EmployeeId)
                                            End If
                                            Try
                                                Command.Dispose()
                                            Catch ex As Exception

                                            End Try

                                            trnsactSession.Commit()
                                            connection.Close()
                                            trnsactSession = Nothing
                                            connection = Nothing
                                        Catch objException As Exception
                                            rowsAffected = 0
                                            Try
                                                trnsactSession.Rollback()
                                                connection.Close()
                                                trnsactSession = Nothing
                                                connection = Nothing
                                            Catch ex As Exception

                                            End Try
                                        End Try
                                    Next
                                End If


                            Else
                                Dim One As Int64 = 1
                                Dim Diff As System.TimeSpan = expiredt.Subtract(sourcedt)
                                Dim Diff1 As System.TimeSpan = todaydt.Subtract(sourcedt)
                                Dim difOfSrcExpDt As Int64 = Diff.TotalDays
                                Dim difOfSrcCurDt As Int64 = Diff1.TotalDays
                                Dim fiftyPerdiff As Int64
                                Dim seventyfivePerdiff As Int64
                                Console.WriteLine("Total Days:" + Convert.ToString(difOfSrcExpDt))
                                fiftyPerdiff = difOfSrcExpDt / 2
                                seventyfivePerdiff = difOfSrcExpDt * 3 / 4
                                Dim DtofFiftypercent As Date = sourcedt.AddDays(fiftyPerdiff)
                                Dim Dtofseventyfivepercent As Date = sourcedt.AddDays(seventyfivePerdiff)

                                log.WriteLine("OrderNo:" + Convert.ToString(OrderNo) + "--Fiftypercent:" + Convert.ToString(DtofFiftypercent) + "--seventyfivepercent:" + Convert.ToString(Dtofseventyfivepercent) + "--expiredt:" + Convert.ToString(expiredt))
                                If todaydt = DtofFiftypercent Then
                                    log.WriteLine("Start to fetch the old orders that is not approved for 50% completion of expiry date")
                                    commonMethod(fiftyPerdiff, OrderNo, EmployeeId, varResultCount, log, "fiftypercent", "Day", difOfSrcExpDt, BU, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm, Decriptions, Order_date, Expiredate, Linestatus, approverid)
                                    log.WriteLine("---------------------****End of 50% of process****-----------------------")
                                ElseIf todaydt = Dtofseventyfivepercent Then
                                    log.WriteLine("Start to fetch the old orders that is not approved for 75% completion of expiry date")
                                    commonMethod(seventyfivePerdiff, OrderNo, EmployeeId, varResultCount, log, "seventyfivepercent", "Day", difOfSrcExpDt, BU, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm, Decriptions, Order_date, Expiredate, Linestatus, approverid)
                                    log.WriteLine("---------------------****End of 75% of process****-----------------------")
                                ElseIf todaydt = expiredt Then
                                    log.WriteLine("Start to fetch the old orders that is not approved for last date of expiry date")
                                    commonMethod(One, OrderNo, EmployeeId, varResultCount, log, "lastday", "lastDay", One, BU, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm, Decriptions, Order_date, Expiredate, Linestatus, approverid)
                                    log.WriteLine("---------------------****End of last day of process****-----------------------")
                                Else
                                    log.WriteLine(Convert.ToString(OrderNo) + "No records found to send reminder email for order")
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception
                    log.WriteLine(Convert.ToString(OrderNo) + "Issue Occured")
                End Try
            End While
            dtrAppReader.Close()
        Else
            log.WriteLine("No records found to expire order- QTW")
            dtrAppReader.Close()
        End If
        Return log

    End Function

    Public Sub commonMethod(ByVal Value As String, ByVal OrderNo As String, ByVal EmployeeId As String, ByVal varResultCount As String, log As StreamWriter,
                            ByVal type As String, ByVal Email As String, ByVal endCount As Int16, ByVal BU As String,
                            ByVal OPRID_Appr_By As String, ByVal Appr_Dttm As String, ByVal ITM_SETID As String, ByVal SELL_Price As String, ByVal Required_By_Dttm As String, ByVal Decriptions As String, ByVal Order_date As String, ByVal Expiredate As String, Optional ByVal LineStatus As String = " ", Optional ByVal approverid As String = " ")
        Dim strSQLString As String = " "
        Dim apprEmail As String = " "
        Dim empEmail As String
        Try
            If LineStatus = "QTW" Then
                Try
                    Dim sPrimaryApprover As String = String.Empty
                    Dim sAlternateApprover As String = String.Empty
                    Dim apprid As String = String.Empty
                    Dim strSQLString5 As String = "SELECT  A.ISA_IOL_APR_EMP_ID AS NextApprover, A.isa_iol_apr_alt AS AltNextApprover " & vbCrLf &
                              " FROM SDIX_USERS_APPRV A WHERE A.isa_employee_id = '" & OPRID_Appr_By & "' And A.business_unit = '" & BU & "' "
                    Dim dsApprovers As New DataSet
                    dsApprovers = GetAdapter(strSQLString5)
                    If dsApprovers.Tables(0).Rows.Count > 0 Then
                        sPrimaryApprover = dsApprovers.Tables(0).Rows(0).Item("NextApprover").ToString.Trim

                        sAlternateApprover = dsApprovers.Tables(0).Rows(0).Item("AltNextApprover").ToString.Trim
                        If sPrimaryApprover <> OPRID_Appr_By Then
                            If sPrimaryApprover <> sAlternateApprover Then

                                apprid = sPrimaryApprover & ","

                                apprid &= sAlternateApprover
                                approverid = sPrimaryApprover
                            Else
                                approverid = sPrimaryApprover
                            End If
                        End If

                    End If
                    log.WriteLine("OrderNo {0} is not approved by employee {1}, budgetory approver {2} ,notificaton sent to user emailID for confirmation", OrderNo, EmployeeId, approverid)

                Catch ex As Exception

                End Try

            Else
                log.WriteLine("OrderNo {0} is not approved by employee {1},notificaton sent to user emailID for confirmation", OrderNo, EmployeeId)
            End If

            strSQLString = "SELECT ISA_EMPLOYEE_EMAIL FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" & EmployeeId & "' "
            empEmail = GetScalar(strSQLString)
            If LineStatus = "QTW" Then
                strSQLString = "SELECT ISA_EMPLOYEE_EMAIL FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" & approverid & "' "
                apprEmail = GetScalar(strSQLString)
            End If

            If type = "fiftypercent" Or type = "seventyfivepercent" Or type = "lastday" Then
                buildNotifyApprover(BU, OrderNo, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm,
                                                        Decriptions, Value, empEmail, endCount, Order_date, type, Email, endCount, Expiredate, apprEmail)
                log.WriteLine("OrderNo:" + Convert.ToString(OrderNo) + "Mail Sent Successfully")
            End If
        Catch ex As Exception
            log.WriteLine("OrderNo:" + Convert.ToString(OrderNo) + "Issue in common method")
        End Try



    End Sub
    'Madhu-INC0015106-Removed avacorp in Email flow
    Public Sub buildNotifyApprover(ByVal businessUnit As String, ByVal orderNum As String, ByVal apprvBy As String,
                                    ByVal apprvDate As String, ByVal itmSet As String, ByVal sellPrice As String,
                                    ByVal itmReqDate As String, ByVal itmDescr As String, ByVal mode As String,
                                   ByVal empEmail1 As String, ByVal DateInterval As String, ByVal orderDate As String,
                                   ByVal type As String, ByVal Email As String, ByVal endcount As Int16, ByVal Expiredate As String, Optional ByVal apprEmail As String = " ")

        Dim strbodyhead As String
        Dim strbodydetl As String


        ''Build Line item Grid
        Dim dtgcart As DataGrid
        Dim SBstk As New StringBuilder
        Dim SWstk As New StringWriter(SBstk)
        Dim htmlTWstk As New HtmlTextWriter(SWstk)
        Dim dataGridHTML As String = String.Empty
        Dim dstcartSTK As New DataTable
        Dim StrWO1 As String = " "
        dstcartSTK = buildCartforemail(orderNum, StrWO1, DateInterval, type, businessUnit, Expiredate)
        If Trim(StrWO1) = "" Then
            StrWO1 = " "
        End If
        '' itmQuoted.WorkOrderNumber = StrWO1

        If dstcartSTK.Rows.Count > 0 Then
            dtgcart = New DataGrid
            dtgcart.DataSource = dstcartSTK
            dtgcart.DataBind()
            dtgcart.CellPadding = 3
            dtgcart.BorderColor = Color.Gray
            dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
            dtgcart.HeaderStyle.Font.Bold = True
            dtgcart.HeaderStyle.ForeColor = Color.Black
            dtgcart.Width.Percentage(90)
            dtgcart.RenderControl(htmlTWstk)
            dataGridHTML = SBstk.ToString()
        End If

        Dim Mailer As MailMessage = New MailMessage
        Dim FromAddress As String = "SDIExchange@SDI.com"
        Dim Mailcc As String = ""
        'Dim MailBcc As String = "webdev@sdi.com;Tony.Smith@sdi.com"
        Dim MailBcc As String = "WebDev@sdi.com"

        strbodyhead = "<table bgcolor='black' Width='100%'><tbody><tr><td style='width:1%;'><img src='http://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' style='padding: 10px 0;' vspace='0' hspace='0' /></td>" & vbCrLf
        strbodyhead = strbodyhead & "<td style='width:50% ;'><center><span style='font-weight:bold;color:white;font-size:24px;'>SDI Marketplace</span></center>" & vbCrLf
        strbodyhead = strbodyhead & "<center><span style='color:white;'>SDiExchange - Customer Approval Required Notification</span></center></td></tr></tbody></table>" & vbCrLf
        strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>"
        strbodydetl = strbodydetl & "<span style='COLOR: red'>** ORDER APPROVAL REQUIRED **</span>" & vbCrLf
        strbodydetl = "<div>"

        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"

        strbodydetl = strbodydetl & "<p ><span style='font-weight:bold;'>Order No: </span><span>    </span>" & orderNum & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Ordered by Date: </span><span>  </span>" & orderDate & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Request by Date: </span><span>  </span>" & itmReqDate & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br></p>"


        If Email = "lastDay" Then
            strbodydetl = strbodydetl & "<p><span style='font-weight:bold;COLOR: red'>WARNING:  QUOTE NEEDS APPROVAL AND WILL EXPIRE AT MIDNIGHT (EST). IF PARTS ARE NEEDED AFTER QUOTE HAS EXPIRED, PLEASE SUBMIT A NEW REQUISITION FOR A NEW QUOTE. </span></p>"
        Else
            strbodydetl = strbodydetl & "<p><span style='font-weight:bold;COLOR: red'>WARNING:  QUOTE NEEDS APPROVAL AND WILL EXPIRE AT MIDNIGHT (EST) on (" & Expiredate & ")</span> </p>"
        End If

        Mailer.Body = strbodyhead & strbodydetl
        Mailer.Body = Mailer.Body & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
        Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
        Mailer.Body = Mailer.Body & "</TABLE>" & vbCrLf
        Mailer.Body = Mailer.Body & "<HR width='100%' SIZE='1'>" & vbCrLf
        Mailer.Body = Mailer.Body & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />" & vbCrLf




        If DbUrl.Substring(DbUrl.Length - 4).ToUpper = "PLGR" Or
                   DbUrl.Substring(DbUrl.Length - 4).ToUpper = "STAR" Or
                   DbUrl.Substring(DbUrl.Length - 4).ToUpper = "DEVL" Or
                    DbUrl.Substring(DbUrl.Length - 4).ToUpper = "SNBX" Or
                    DbUrl.Substring(DbUrl.Length - 5).ToUpper = "FSTST" Or
                     DbUrl.Substring(DbUrl.Length - 5).ToUpper = "FSUAT" Or
               DbUrl.Substring(DbUrl.Length - 4).ToUpper = "RPTG" Then
            Mailer.To = "WebDev@sdi.com"

            Mailer.Subject = "<<TEST SITE>> SDiExchange - Customer Approval Required For Order " & orderNum
        Else
            Mailer.To = empEmail1
            'INC0015228 Send email for requestor along with budgetory approver[Change By vishalini]
            If apprEmail <> " " Then
                Mailer.To = empEmail1 + ";" + apprEmail
            End If

            Mailer.Bcc = "WebDev@sdi.com"
            Mailer.Subject = "SDiExchange - Customer Approval Required For Order - " & orderNum
        End If

        'Mailer.IsBodyHtml = True
        Dim connectionEmail As OleDbConnection = New OleDbConnection((Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString"))))
        'connectionEmail.Open()
        Try

            Dim SDIEmailService As EmailService1.EmailServices = New EmailService1.EmailServices()
            SDIEmailService.EmailUtilityServices("MailandStore", "SDIExchange@SDI.com", Mailer.To.ToString(), Mailer.Subject, "", "", Mailer.Body, "OrderApproval", Nothing, Nothing)


            Try
                connectionEmail.Close()
            Catch ex As Exception

            End Try

        Catch exEmailOut As Exception
            Try
                connectionEmail.Close()
            Catch ex As Exception

            End Try
            Dim strerr As String = ""
            strerr = exEmailOut.Message
        End Try

    End Sub
    Private Function buildCartforemail(ByVal ordNumber As String,
                     ByRef strWrkOrder As String, ByRef DateInterval As String,
                                       ByVal type As String, ByVal businessUnit As String, ByVal Enddate As String) As DataTable

        Dim dr As DataRow
        Dim I As Integer
        Dim strPrice As String
        Dim strQty As String
        Dim dstcart As DataTable
        dstcart = New DataTable
        Dim isAddExpectedDate As Boolean = False

        dstcart.Columns.Add("Item ID")
        dstcart.Columns.Add("Description")
        dstcart.Columns.Add("Manuf.")
        dstcart.Columns.Add("Manuf. Partnum")
        dstcart.Columns.Add("QTY")
        dstcart.Columns.Add("UOM")
        dstcart.Columns.Add("Price")
        dstcart.Columns.Add("Ext. Price")
        dstcart.Columns.Add("LN")
        dstcart.Columns.Add("Expected Delivery Date")
        dstcart.Columns.Add("Requested Due Date")

        Dim strOraSelectQuery As String = String.Empty
        Dim ordIdentifier As String = String.Empty
        Dim ordBU As String = String.Empty
        Dim OrcRdr As OleDb.OleDbDataReader = Nothing
        Dim dsOrdLnItems As DataSet = New DataSet
        Dim strSqlSelectQuery As String = String.Empty
        Dim SqlRdr As SqlDataReader = Nothing
        Dim strProdVwId As String = String.Empty
        Dim SqlRdr1 As SqlDataReader = Nothing
        Try

            strOraSelectQuery = "SELECT S.DTTM_STAMP, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S,PS_ISA_NSTK_QUOTE Q" & vbCrLf &
"WHERE a.isa_line_status IN ('QTS','QTW') AND A.ORDER_NO= '" & ordNumber & "' AND A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln " & vbCrLf &
"AND a.isa_line_status = S.isa_line_status AND A.ORDER_NO=Q.REQ_ID AND A.ISA_INTFC_LN = Q.LINE_NBR AND TO_CHAR(Q.EXPIRE_DT,'YYYY/MM/DD')= '" & Enddate & "'" & vbCrLf


            dsOrdLnItems = GetAdapter(strOraSelectQuery)

            If dsOrdLnItems.Tables(0).Rows.Count > 0 Then
                Dim intMy21 As Integer = 0
                For Each dataRowMain As DataRow In dsOrdLnItems.Tables(0).Rows
                    'code to get work order id
                    If intMy21 = 0 Then
                        strWrkOrder = " "
                        Try
                            strWrkOrder = CType(dataRowMain("ISA_WORK_ORDER_NO"), String)
                            If strWrkOrder Is Nothing Then
                                strWrkOrder = " "
                            Else
                                If Trim(strWrkOrder) = "" Then
                                    strWrkOrder = " "
                                End If
                            End If
                        Catch ex As Exception
                            strWrkOrder = " "
                        End Try
                    End If
                    intMy21 = intMy21 + 1  ' end code to get work order id

                    dr = dstcart.NewRow()
                    dr("Item ID") = CType(dataRowMain("INV_ITEM_ID"), String).Trim()
                    dr("Description") = CType(dataRowMain("DESCR254"), String).Trim()

                    Try
                        dr("Manuf.") = CType(dataRowMain("ISA_MFG_FREEFORM"), String).Trim()
                    Catch ex As Exception
                        dr("Manuf.") = " "
                    End Try
                    Try
                        dr("Manuf. Partnum") = CType(dataRowMain("MFG_ITM_ID"), String).Trim()
                    Catch ex As Exception
                        dr("Manuf. Partnum") = " "
                    End Try
                    Try
                        dr("Item ID") = CType(dataRowMain("INV_ITEM_ID"), String).Trim()
                    Catch ex As Exception
                        dr("Item ID") = " "
                    End Try

                    Try
                        dr("UOM") = CType(dataRowMain("UNIT_OF_MEASURE"), String).Trim()
                    Catch ex As Exception
                        dr("UOM") = "EA"
                    End Try

                    Try
                        strQty = CType(dataRowMain("QTY_REQUESTED"), String).Trim()
                        strQty = strQty.Remove(strQty.Length - 2)
                        dr("QTY") = strQty
                        If IsDBNull(CType(dataRowMain("QTY_REQUESTED"), String).Trim()) Or CType(dataRowMain("QTY_REQUESTED"), String).Trim() = " " Then
                            strQty = "0"
                            'Else
                            '    strQty = CType(dataRowMain("QTY_REQ"), String).Trim()
                            '    strQty = strQty.Remove(strQty.Length - 2)
                        End If
                    Catch ex As Exception
                        strQty = "0"
                    End Try
                    strPrice = "0.00"
                    Try
                        strPrice = CDec(CType(dataRowMain("ISA_SELL_PRICE"), String).Trim()).ToString()
                        strPrice = strPrice.Remove(strPrice.Length - 2)
                        If strPrice Is Nothing Then
                            strPrice = "0.00"
                        End If
                    Catch ex As Exception
                        strPrice = "0.00"
                    End Try
                    If CDec(strPrice) = 0 Then
                        ' dr("Price") = "Call for Price"
                        dr("Price") = "0.00"
                    Else
                        strPrice = CDec(strPrice).ToString("f")
                        dr("Price") = strPrice
                    End If
                    Dim ExtPrice As Decimal = CType(Convert.ToDecimal(strQty) * Convert.ToDecimal(strPrice), String)

                    If (ExtPrice.ToString("f") = "0.00") Then
                        dr("Ext. Price") = "0.00"
                    Else
                        dr("Ext. Price") = ExtPrice.ToString("f")
                    End If

                    dr("LN") = CType(dataRowMain("ISA_INTFC_LN"), String).Trim()
                    Try
                        dr("Expected Delivery Date") = CDate(dataRowMain("ISA_REQUIRED_BY_DT")).ToString("MM/dd/yyyy")
                    Catch ex As Exception
                        dr("Expected Delivery Date") = " "
                    End Try
                    Try
                        Dim strSQLStringReq As String = "select TO_CHAR(REQ.SOURCE_DATE,'MM/DD/YYYY') AS SOURCE_DATE from PS_REQ_LINE REQ,PS_ISA_ORD_INTF_LN LN " & vbCrLf &
             "WHERE REQ.REQ_ID = LN.ORDER_NO AND REQ.BUSINESS_UNIT = LN.BUSINESS_UNIT_PO AND REQ.LINE_NBR = LN.ISA_INTFC_LN " & vbCrLf &
                "AND REQ.req_id = '" & ordNumber & "' AND REQ.LINE_NBR = '" & dr("LN") & "' " & vbCrLf
                        dr("Requested Due Date") = GetScalar(strSQLStringReq)
                    Catch ex As Exception
                        dr("Requested Due Date") = " "
                    End Try
                    dstcart.Rows.Add(dr)
                Next
            End If
        Catch ex As Exception

        End Try
        Return dstcart

    End Function
    Public ReadOnly Property DbUrl() As String
        Get
            Return ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property
    Public Function GetReader(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As OleDbDataReader

        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim datareader As OleDbDataReader
            datareader = Command.ExecuteReader()
            Return datareader
        Catch objException As Exception
            Try
                connection.Close()
                connection.Dispose()
            Catch ex As Exception

            End Try
            Dim sMsg66 As String = LCase(objException.ToString)

        End Try

    End Function
    Public Function GetAdapter(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True, Optional ByVal bThrowBackError As Boolean = False) As DataSet


        'Gives us a reference to the current asp.net 
        'application executing the method.
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter =
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()

            dataAdapter.Fill(UserdataSet)
            Try
                dataAdapter.Dispose()
            Catch ex As Exception

            End Try
            Try
                Command.Dispose()
            Catch ex As Exception

            End Try
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
            'connection.close()
            Return UserdataSet
        Catch objException As Exception
            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception

            End Try
            'connection.close()

        End Try

    End Function
    Public Function GetScalar(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True) As String


        Dim strReturn As String = ""
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Try

            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Try
                strReturn = CType(Command.ExecuteScalar(), String)
            Catch ex32 As Exception
                strReturn = ""
            End Try
            If strReturn Is Nothing Then
                strReturn = ""
            End If
            Try
                Command.Dispose()
            Catch ex1 As Exception

            End Try
            Try
                connection.Close()
                connection.Dispose()
            Catch ex2 As Exception

            End Try
            'connection.close()
        Catch objException As Exception
            strReturn = ""

            Try
                connection.Close()
                connection.Dispose()
            Catch ex As Exception
            End Try

        End Try

        Return strReturn

    End Function
End Module
