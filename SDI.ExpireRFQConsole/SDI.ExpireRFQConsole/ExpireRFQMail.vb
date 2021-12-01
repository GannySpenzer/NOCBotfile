Imports System.Configuration
Imports System.IO
Imports System.Data.OleDb
Imports System.Xml
Imports System.Web
Imports System.Web.UI.WebControls
Imports System.Text
Imports System.Web.UI
Imports System.Web.Mail
Imports System.Drawing
Imports System.Data.SqlClient

Module ExpireRFQMail
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = ConfigurationManager.AppSettings("LogPath")
    Dim logpath As String = rootDir & "RFQExpireNotification" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString")))
    Dim configPath As New String(Convert.ToString(ConfigurationManager.AppSettings("configPath")))
    Dim todayDate As String = String.Format("{0:dd/MM/yyyy}", DateTime.Now)
    Dim getDate As New String(Convert.ToString(ConfigurationManager.AppSettings("Date")))
    Dim Day1 As New String(Convert.ToString(ConfigurationManager.AppSettings("Day1")))
    Dim Day2 As New String(Convert.ToString(ConfigurationManager.AppSettings("Day2")))
    Dim Day3 As New String(Convert.ToString(ConfigurationManager.AppSettings("Day3")))
    Dim Ordlist_10 As New List(Of String)
    Dim Oldlist_1 As New List(Of String)

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
        log = buildstatchgout(log)
        log.WriteLine("********************End of Expire RFQ Email Notification Utility********************")
        Console.WriteLine("********************End of Expire RFQ Email Notification Utility********************")
        log.Close()

    End Sub

    Public Function buildstatchgout(log As StreamWriter) As StreamWriter

        Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
        connectOR = New OleDbConnection(connectionString)


        ' method to call the 
        Dim strSQLString As String = ""
        Dim strOneString As String = "0"
        Dim strTenString As String = Day1
        Dim strTwentyString As String = Day2
        Dim strThirtyString As String = Day3
        Dim varResultCount As String = "0"
        Dim varResultMode As String = "0"
        Dim Ordlist_31 As New List(Of String)


        If (getDate = "01/01/2001") Then
            Try
                Dim config As Configuration = ConfigurationManager.OpenExeConfiguration(configPath & "\SDI.ExpireRFQConsole.exe")
                ConfigurationManager.RefreshSection("appSettings")
                config.AppSettings.Settings("Date").Value = todayDate
                config.Save(ConfigurationSaveMode.Modified)
                ConfigurationManager.RefreshSection("appSettings")
                log.WriteLine("Start to fetch the old orders that is not approved")
                Console.WriteLine("Start to fetch the old orders that is not approved")
                commonMethod(strOneString, strSQLString, varResultCount, log, "Old", "", "", "Day", 31)
                log.WriteLine("---------------------****End of 1 days process****-----------------------")
                Console.WriteLine("---------------------****End of 1 days process****-----------------------")
            Catch ex As Exception
            End Try

        Else
            Try
                Dim newDate As Date = Date.ParseExact(todayDate, "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo)
                Dim oldDate As Date = Date.ParseExact(getDate, "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo)

                Dim Diff1 As System.TimeSpan = newDate.Subtract(oldDate)
                Dim days As Int64 = Diff1.TotalDays
                Console.WriteLine("Total Days:" + Convert.ToString(days))
                If days = 10 Then
                    log.WriteLine("Start to fetch the old orders that is not approved for more than 10 days")
                    commonMethod(strTenString, strSQLString, varResultCount, log, "Old", "", "", "Day", 30)
                    log.WriteLine("---------------------****End of 10 days process****-----------------------")

                ElseIf days = 20 Then
                    log.WriteLine("Start to fetch the old orders that is not approved for more than 20 days")
                    commonMethod(strTwentyString, strSQLString, varResultCount, log, "Old", "", "", "Day", 30)
                    log.WriteLine("---------------------****End of 20 days process****-----------------------")

                ElseIf days = 30 Then
                    log.WriteLine("Start to fetch the old orders that is not approved for more than 30 days")
                    commonMethod(strThirtyString, strSQLString, varResultCount, log, "Old", "", "", "lastDay", 30)
                    log.WriteLine("---------------------****End of 30 days process****-----------------------")
                ElseIf days = 31 Then

                    strSQLString = "SELECT TO_CHAR(LG.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP ,TO_CHAR(HD.order_date, 'MM-DD-YYYY') AS order_date, LN.*  " & vbCrLf &
                " FROM PS_ISA_ORD_INTF_LN LN,PS_ISA_ORD_INTF_HD HD,PS_ISAORDSTATUSLOG LG WHERE LN.ORDER_NO = HD.ORDER_NO AND " & vbCrLf &
                " LN.ISA_LINE_STATUS = 'QTS' AND LG.ORDER_NO = LN.ORDER_NO AND LG.ISA_INTFC_LN = LN.ISA_INTFC_LN AND LG.ISA_LINE_STATUS = LN.ISA_LINE_STATUS " & vbCrLf &
                " AND LG.DTTM_STAMP  <= TO_DATE(TO_DATE('" & oldDate & "', 'DD/MM/YYYY') - 30) AND NOT EXISTS (SELECT * FROM PS_ISAORDSTATUSLOG A WHERE LG.ORDER_NO =  A.ORDER_NO AND " & vbCrLf &
                " LG.ISA_INTFC_LN = A.ISA_INTFC_LN AND A.ISA_LINE_STATUS='QTS' AND  A.DTTM_STAMP >= TO_DATE(TO_DATE('" & oldDate & "', 'DD/MM/YYYY') - 30)) ORDER BY LG.DTTM_STAMP desc" & vbCrLf


                    Dim dtrAppReaders As OleDbDataReader = GetReader(strSQLString)

                    If dtrAppReaders.HasRows() = True Then
                        While dtrAppReaders.Read()

                            varResultMode = "31"
                            Dim strSQLstringUpt As String
                            Dim ff As String = dtrAppReaders.Item("ORDER_NO")

                            Dim trnsactSession As OleDbTransaction = Nothing
                            Dim connection As OleDbConnection = New OleDbConnection(ConfigurationManager.AppSettings("OLEDBconString"))
                            Dim rowsAffected As Integer = 0

                            strSQLstringUpt = "UPDATE PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = 'EXP' WHERE ISA_LINE_STATUS = 'QTS' AND ORDER_NO= '" & Convert.ToString(dtrAppReaders.Item("ORDER_NO")) & "' AND ISA_INTFC_LN = " & Convert.ToString(dtrAppReaders.Item("ISA_INTFC_LN")) & ""
                            Try
                                connection.Open()
                                trnsactSession = connection.BeginTransaction
                                Dim Command As OleDbCommand = New OleDbCommand(strSQLstringUpt, connection)
                                Command.Transaction = trnsactSession
                                Command.CommandTimeout = 120
                                rowsAffected = Command.ExecuteNonQuery()
                                If Not rowsAffected = 0 Then
                                    log.WriteLine("Updated the order status to ""EXP"" for OrderNo {0} that is not approved by employee {1}", Convert.ToString(dtrAppReaders.Item("ORDER_NO")), Convert.ToString(dtrAppReaders.Item("ISA_EMPLOYEE_ID")))
                                Else
                                    log.WriteLine("Error in updating order status to ""EXP"" for OrderNo {0} that is not approved by employee {1} ", Convert.ToString(dtrAppReaders.Item("ORDER_NO")), Convert.ToString(dtrAppReaders.Item("ISA_EMPLOYEE_ID")))
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
                        End While
                        dtrAppReaders.Close()
                    Else
                        log.WriteLine("No records found to expire order")
                        dtrAppReaders.Close()
                    End If

                End If
            Catch ex As Exception
            End Try
        End If

        strSQLString = "select business_unit, date_value_1, date_value_2, date_value_3 from SDIX_RFQ_EXPIRE_BU"
        Dim dtrAppReader As OleDbDataReader = GetReader(strSQLString)
        Dim UNIT As String = ""
        Dim List_BU As String = ""
        If dtrAppReader.HasRows() = True Then
            While dtrAppReader.Read()
                Try
                    Dim business_unit As String = dtrAppReader.Item("business_unit")
                    Dim date_value_1 As Int16 = dtrAppReader.Item("date_value_1")
                    Dim date_value_2 As Int16 = dtrAppReader.Item("date_value_2")
                    Dim date_value_3 As Int16 = dtrAppReader.Item("date_value_3")
                    UNIT = UNIT + "," + "'" + business_unit + "'"

                    log.WriteLine("Business Unit: " + Convert.ToString(business_unit))
                    Console.WriteLine("Business Unit: " + Convert.ToString(business_unit))
                    log.WriteLine("Start to fetch the orders that is not approved for more than " + Convert.ToString(date_value_1) + " days")
                    commonMethod(date_value_1 - 1, strSQLString, varResultCount, log, "New", business_unit, "", "Day", date_value_3)
                    log.WriteLine("---------------------****End of " + Convert.ToString(date_value_1) + " days process****-----------------------")

                    log.WriteLine("Start to fetch the orders that is not approved for more than " + Convert.ToString(date_value_2) + " days")
                    commonMethod(date_value_2 - 1, strSQLString, varResultCount, log, "New", business_unit, "", "Day", date_value_3)
                    log.WriteLine("---------------------****End of " + Convert.ToString(date_value_2) + " days process****-----------------------")

                    log.WriteLine("Start to fetch the orders that is not approved for more than " + Convert.ToString(date_value_3) + " days")
                    commonMethod(date_value_3 - 1, strSQLString, varResultCount, log, "New", business_unit, "", "lastDay", 30)
                    log.WriteLine("---------------------****End of " + Convert.ToString(date_value_3) + " days process****-----------------------")

                    log.WriteLine("Start to expire orders that is not approved for more than " + Convert.ToString(date_value_3) + " days")
                    strSQLString = "SELECT TO_CHAR(S.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP , TO_CHAR(H.order_date, 'MM-DD-YYYY') AS order_date, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S, ps_isa_ord_intf_hd H WHERE a.isa_line_status='QTS' AND  A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status  and H.ORDER_NO = A.ORDER_NO  AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') = TO_CHAR(SYSDATE - '" & Convert.ToString(date_value_3) & "', 'DD-MON-YYYY')And A.BUSINESS_UNIT_OM='" & business_unit & "' order by S.DTTM_STAMP desc"

                    Dim dtrAppReaders As OleDbDataReader = GetReader(strSQLString)

                    If dtrAppReaders.HasRows() = True Then
                        While dtrAppReaders.Read()

                            Dim strSQLstringUpt As String
                            Dim ff As String = dtrAppReaders.Item("ORDER_NO")
                            Dim fff As String = ""
                            Dim trnsactSession As OleDbTransaction = Nothing
                            Dim connection As OleDbConnection = New OleDbConnection(ConfigurationManager.AppSettings("OLEDBconString"))
                            Dim rowsAffected As Integer = 0
                            strSQLstringUpt = "UPDATE PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = 'EXP' WHERE ISA_LINE_STATUS = 'QTS' AND ORDER_NO= '" & Convert.ToString(dtrAppReaders.Item("ORDER_NO")) & "' AND ISA_INTFC_LN = " & Convert.ToString(dtrAppReaders.Item("ISA_INTFC_LN")) & ""
                            Try
                                connection.Open()
                                trnsactSession = connection.BeginTransaction
                                Dim Command As OleDbCommand = New OleDbCommand(strSQLstringUpt, connection)
                                Command.Transaction = trnsactSession
                                Command.CommandTimeout = 120
                                rowsAffected = Command.ExecuteNonQuery()
                                If Not rowsAffected = 0 Then
                                    log.WriteLine("Updated the order status to ""EXP"" for OrderNo {0} that is not approved by employee {1}", Convert.ToString(dtrAppReaders.Item("ORDER_NO")), Convert.ToString(dtrAppReaders.Item("ISA_EMPLOYEE_ID")))
                                Else
                                    log.WriteLine("Error in updating order status to ""EXP"" for OrderNo {0} that is not approved by employee {1} ", Convert.ToString(dtrAppReaders.Item("ORDER_NO")), Convert.ToString(dtrAppReaders.Item("ISA_EMPLOYEE_ID")))
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
                        End While
                        dtrAppReaders.Close()
                    Else
                        log.WriteLine("No records found to expire order")
                        dtrAppReaders.Close()
                    End If
                    log.WriteLine("---------------------****End of '" & Convert.ToString(date_value_3) & "' day expire process****-----------------------")
                Catch ex As Exception

                End Try
            End While
            dtrAppReader.Close()
            List_BU = UNIT.Remove(0, 1)
        End If


        log.WriteLine("------------------------------------------------------------------------------------------")

        log.WriteLine("Start to fetch the new orders that is not approved")
        Console.WriteLine("Start to fetch the new orders that is not approved")
        log.WriteLine("Start to fetch the orders that is not approved for more than 10 days")
        commonMethod(strTenString, strSQLString, varResultCount, log, "New", "", List_BU, "Day", 30)
        log.WriteLine("---------------------****End of 10 days process****-----------------------")

        log.WriteLine("Start to fetch the orders that is not approved for more than 20 days")
        commonMethod(strTwentyString, strSQLString, varResultCount, log, "New", "", List_BU, "Day", 30)
        log.WriteLine("---------------------****End of 20 days process****-----------------------")

        log.WriteLine("Start to fetch the orders that is not approved for more than 30 days")
        commonMethod(strThirtyString, strSQLString, varResultCount, log, "New", "", List_BU, "lastDay", 30)
        log.WriteLine("---------------------****End of 30 days process****-----------------------")

        If getDate <> "01/01/2001" Then
            log.WriteLine("------------------------------------------------------------------------------------------")
            log.WriteLine("Start to expire orders that is not approved for more than 31 days")

            strSQLString = "SELECT TO_CHAR(LG.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP ,TO_CHAR(HD.order_date, 'MM-DD-YYYY') AS order_date, LN.*  " & vbCrLf &
                " FROM PS_ISA_ORD_INTF_LN LN,PS_ISA_ORD_INTF_HD HD,PS_ISAORDSTATUSLOG LG WHERE LN.ORDER_NO = HD.ORDER_NO AND " & vbCrLf &
                " LN.ISA_LINE_STATUS = 'QTS' AND LG.ORDER_NO = LN.ORDER_NO AND LG.ISA_INTFC_LN = LN.ISA_INTFC_LN AND LG.ISA_LINE_STATUS = LN.ISA_LINE_STATUS " & vbCrLf &
                " AND LG.DTTM_STAMP  <= TO_DATE(SYSDATE - 30) AND NOT EXISTS (SELECT * FROM PS_ISAORDSTATUSLOG A WHERE LG.ORDER_NO =  A.ORDER_NO AND " & vbCrLf &
                " LG.ISA_INTFC_LN = A.ISA_INTFC_LN AND A.ISA_LINE_STATUS='QTS' AND  A.DTTM_STAMP >= TO_DATE(SYSDATE - 30)) ORDER BY LG.DTTM_STAMP desc" & vbCrLf


            dtrAppReader = GetReader(strSQLString)
            If dtrAppReader.HasRows() = True Then
                While dtrAppReader.Read()

                    varResultMode = "31"
                    Dim strSQLstringUpt As String
                    Dim ff As String = dtrAppReader.Item("ORDER_NO")

                    Dim DTTM_STAMP As Date = dtrAppReader.Item("DTTM_STAMP")
                    Dim oldDate As Date = Date.ParseExact(getDate, "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo)
                    Dim staticDate As Date = oldDate.AddDays(-30)
                    If staticDate < DTTM_STAMP Then
                        Dim fff As String = ""
                        Dim trnsactSession As OleDbTransaction = Nothing
                        Dim connection As OleDbConnection = New OleDbConnection(ConfigurationManager.AppSettings("OLEDBconString"))
                        Dim rowsAffected As Integer = 0

                        strSQLstringUpt = "UPDATE PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = 'EXP' WHERE ISA_LINE_STATUS = 'QTS' AND ORDER_NO= '" & Convert.ToString(dtrAppReader.Item("ORDER_NO")) & "' AND ISA_INTFC_LN = " & Convert.ToString(dtrAppReader.Item("ISA_INTFC_LN")) & ""
                        Try
                            connection.Open()
                            trnsactSession = connection.BeginTransaction
                            Dim Command As OleDbCommand = New OleDbCommand(strSQLstringUpt, connection)
                            Command.Transaction = trnsactSession
                            Command.CommandTimeout = 120
                            rowsAffected = Command.ExecuteNonQuery()
                            If Not rowsAffected = 0 Then
                                log.WriteLine("Updated the order status to ""EXP"" for OrderNo {0} that is not approved by employee {1}", Convert.ToString(dtrAppReader.Item("ORDER_NO")), Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_ID")))
                            Else
                                log.WriteLine("Error in updating order status to ""EXP"" for OrderNo {0} that is not approved by employee {1} ", Convert.ToString(dtrAppReader.Item("ORDER_NO")), Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_ID")))
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

                    End If

                End While
                dtrAppReader.Close()
            Else
                log.WriteLine("No records found to expire order")
                dtrAppReader.Close()
            End If

            log.WriteLine("---------------------****End of 31 day expire process****-----------------------")
        End If

        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        Return log

    End Function


    Public Sub commonMethod(ByVal Value As String, ByVal strSQLString As String, ByVal varResultCount As String, log As StreamWriter,
                            ByVal type As String, ByVal businessUnit As String, ByVal List_BU As String, ByVal Email As String, ByVal endCount As Int16)
        Try
            If businessUnit = "" Then
                If type = "Old" Then
                    Dim conformDate As String = If(getDate = "01/01/2001", todayDate, getDate)
                    strSQLString = "SELECT TO_CHAR(LG.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP ,TO_CHAR(HD.order_date, 'MM-DD-YYYY') AS order_date, LN.*  " & vbCrLf &
                " FROM PS_ISA_ORD_INTF_LN LN,PS_ISA_ORD_INTF_HD HD,PS_ISAORDSTATUSLOG LG WHERE LN.ORDER_NO = HD.ORDER_NO AND " & vbCrLf &
                " LN.ISA_LINE_STATUS = 'QTS' AND LG.ORDER_NO = LN.ORDER_NO AND LG.ISA_INTFC_LN = LN.ISA_INTFC_LN AND LG.ISA_LINE_STATUS = LN.ISA_LINE_STATUS " & vbCrLf &
                " AND LG.DTTM_STAMP  <= TO_DATE(TO_DATE('" & conformDate & "', 'DD/MM/YYYY') - 30) AND NOT EXISTS (SELECT * FROM PS_ISAORDSTATUSLOG A WHERE LG.ORDER_NO =  A.ORDER_NO AND " & vbCrLf &
                " LG.ISA_INTFC_LN = A.ISA_INTFC_LN AND A.ISA_LINE_STATUS='QTS' AND  A.DTTM_STAMP >= TO_DATE(TO_DATE('" & conformDate & "', 'DD/MM/YYYY') - 30)) ORDER BY LG.DTTM_STAMP desc" & vbCrLf

                Else
                    If List_BU = "" Then
                        strSQLString = "SELECT TO_CHAR(S.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP , TO_CHAR(H.order_date, 'MM-DD-YYYY') AS order_date, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S, ps_isa_ord_intf_hd H WHERE a.isa_line_status='QTS' AND  A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status  and H.ORDER_NO = A.ORDER_NO  AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') = TO_CHAR(SYSDATE - '" & Value & "', 'DD-MON-YYYY') order by S.DTTM_STAMP desc"
                    Else
                        strSQLString = "SELECT TO_CHAR(S.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP , TO_CHAR(H.order_date, 'MM-DD-YYYY') AS order_date, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S, ps_isa_ord_intf_hd H WHERE a.isa_line_status='QTS' AND  A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status  and H.ORDER_NO = A.ORDER_NO  AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') = TO_CHAR(SYSDATE - '" & Value & "', 'DD-MON-YYYY') And A.BUSINESS_UNIT_OM NOT IN (" & List_BU & ") order by S.DTTM_STAMP desc"
                    End If
                End If
            Else
                strSQLString = "SELECT TO_CHAR(S.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP , TO_CHAR(H.order_date, 'MM-DD-YYYY') AS order_date, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S, ps_isa_ord_intf_hd H WHERE a.isa_line_status='QTS' AND  A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status  and H.ORDER_NO = A.ORDER_NO  AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') = TO_CHAR(SYSDATE - '" & Value & "', 'DD-MON-YYYY')And A.BUSINESS_UNIT_OM='" & businessUnit & "' order by S.DTTM_STAMP desc"
            End If
            Dim dtrAppReader As OleDbDataReader = GetReader(strSQLString)
            Dim Required_By_Dttm As String = String.Empty

            'To get Employye Email ID
            If dtrAppReader.HasRows() = True Then

                While dtrAppReader.Read()
                    Try
                        varResultCount = (dtrAppReader.Item(0)).ToString()
                        log.WriteLine("OrderNo {0} is not approved by employee {1},notificaton sent to user emailID for confirmation", Convert.ToString(dtrAppReader.Item("ORDER_NO")), Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_ID")))

                        strSQLString = "SELECT ISA_EMPLOYEE_EMAIL FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" & dtrAppReader.Item("ISA_EMPLOYEE_ID").ToString() & "' "
                        Dim empEmail As String = GetScalar(strSQLString)

                        Dim BU As String = Convert.ToString(dtrAppReader.Item("Business_Unit_Om"))
                        Dim Ord_No As String = Convert.ToString(dtrAppReader.Item("ORDER_NO"))
                        Dim OPRID_Appr_By As String = Convert.ToString(dtrAppReader.Item("OPRID_APPROVED_BY"))
                        Dim Appr_Dttm As String = Convert.ToString(dtrAppReader.Item("APPROVAL_DTTM"))
                        Dim ITM_SETID As String = Convert.ToString(dtrAppReader.Item("ITM_SETID"))
                        Dim SELL_Price As String = Convert.ToString(dtrAppReader.Item("ISA_SELL_PRICE"))
                        Try
                            Required_By_Dttm = CDate(dtrAppReader.Item("ISA_REQUIRED_BY_DT")).ToString("MM-dd-yyyy")
                        Catch ex As Exception
                        End Try
                        Dim Decriptions As String = Convert.ToString(dtrAppReader.Item("DESCR254"))
                        Dim Order_date As String = Convert.ToString(dtrAppReader.Item("order_date"))
                        If type = "New" Then

                            If Not Ordlist_10.Contains(dtrAppReader.Item("ORDER_NO")) Then

                                Ordlist_10.Add(dtrAppReader.Item("ORDER_NO"))
                                buildNotifyApprover(BU, Ord_No, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm,
                                            Decriptions, Value + 1, empEmail, Value, Order_date, type, Email, endCount)
                            End If
                        Else
                            If Not Oldlist_1.Contains(dtrAppReader.Item("ORDER_NO")) Then

                                Oldlist_1.Add(dtrAppReader.Item("ORDER_NO"))
                                buildNotifyApprover(BU, Ord_No, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm,
                                            Decriptions, Value + 1, empEmail, Value, Order_date, type, Email, endCount)

                            End If
                        End If
                    Catch ex As Exception

                    End Try

                End While
                dtrAppReader.Close()
            Else
                log.WriteLine("No records found to notify the user")
                dtrAppReader.Close()
            End If
        Catch ex As Exception
        End Try

    End Sub

    Public Sub buildNotifyApprover(ByVal businessUnit As String, ByVal orderNum As String, ByVal apprvBy As String,
                                    ByVal apprvDate As String, ByVal itmSet As String, ByVal sellPrice As String,
                                    ByVal itmReqDate As String, ByVal itmDescr As String, ByVal mode As String,
                                   ByVal empEmail1 As String, ByVal DateInterval As String, ByVal orderDate As String,
                                   ByVal type As String, ByVal Email As String, ByVal endCount As Int16)

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
        dstcartSTK = buildCartforemail(orderNum, StrWO1, DateInterval, type, businessUnit)
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
        Dim MailBcc As String = "webdev@sdi.com;Tony.Smith@sdi.com"

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
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Orderd by Date: </span><span>  </span>" & orderDate & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Request by Date: </span><span>  </span>" & itmReqDate & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br></p>"

        Dim value = endCount - Convert.ToInt16(mode)
        If Email = "lastDay" Then
            strbodydetl = strbodydetl & "<p>The above referenced order is waiting for your approval and will expire at the end of the day. </p>"
        Else
            strbodydetl = strbodydetl & "<p>The above referenced order is waiting for your approval and will expire in " & value & " days. </p>"
        End If

        Mailer.Body = strbodyhead & strbodydetl
        Mailer.Body = Mailer.Body & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
        Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
        Mailer.Body = Mailer.Body & "</TABLE>" & vbCrLf
        Mailer.Body = Mailer.Body & "<HR width='100%' SIZE='1'>" & vbCrLf
        Mailer.Body = Mailer.Body & "<img src='https://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & vbCrLf

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
            Mailer.Bcc = "WebDev@sdi.com"
            Mailer.Subject = "SDiExchange - Customer Approval Required For Order - " & orderNum
        End If

        'Mailer.IsBodyHtml = True
        Dim connectionEmail As OleDbConnection = New OleDbConnection((Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString"))))
        'connectionEmail.Open()
        Try

            Dim SDIEmailService As EmailService.EmailServices = New EmailService.EmailServices()
            SDIEmailService.EmailUtilityServices("MailandStore", "SDIExchange@SDI.com", Mailer.To.ToString(), Mailer.Subject, "", "", Mailer.Body, "OrderApproval", Nothing, Nothing)

            ''UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, FromAddress, Mailer.To.ToString(), Mailcc, MailBcc, "OrderApproval", Mailer.Body.ToString(), connectionEmail)
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

    Public ReadOnly Property DbUrl() As String
        Get
            Return ConfigurationSettings.AppSettings("OLEDBconString")
        End Get
    End Property

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


    Private Function buildCartforemail(ByVal ordNumber As String,
                     ByRef strWrkOrder As String, ByRef DateInterval As String,
                                       ByVal type As String, ByVal businessUnit As String) As DataTable

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
            If type = "New" Then
                If DateInterval = "30" Then
                    strOraSelectQuery = "SELECT S.DTTM_STAMP, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S WHERE a.isa_line_status='QTS' AND A.ORDER_NO= '" & ordNumber & "' AND A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') <= TO_CHAR(SYSDATE - '" & DateInterval & "', 'DD-MON-YYYY')"
                Else
                    strOraSelectQuery = "SELECT S.DTTM_STAMP, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S WHERE a.isa_line_status='QTS' AND A.ORDER_NO= '" & ordNumber & "' AND A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') = TO_CHAR(SYSDATE - '" & DateInterval & "', 'DD-MON-YYYY')"
                End If
            Else
                strOraSelectQuery = "SELECT S.DTTM_STAMP, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S WHERE a.isa_line_status='QTS' AND A.ORDER_NO= '" & ordNumber & "' AND A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status"
            End If

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
                        Dim strSQLStringReq As String = "select SOURCE_DATE from ps_Req_line where Req_Id = '" & ordNumber & "' and rownum < 2"
                        dr("Requested Due Date") = CDate(GetScalar(strSQLStringReq)).ToString("MM/dd/yyyy")
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

    Public Function Get_Adapter(ByVal p_strQuery As String, Optional ByVal bGoToErrPage As Boolean = True, Optional ByVal bThrowBackError As Boolean = False) As DataSet
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Dim UserdataSet As DataSet = New DataSet()

        Try
            Dim Command As OleDbCommand = New OleDbCommand(p_strQuery, connection)
            Command.CommandTimeout = 120
            connection.Open()
            Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(Command)
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

        Catch objException As Exception

            Try
                connection.Dispose()
                connection.Close()
            Catch ex As Exception
            End Try
        End Try

        Return UserdataSet
    End Function

    Public Function IsAscend(ByVal sBU As String) As Boolean
        Dim bIsAscend As Boolean = False
        '<add key="ASCEND_BU_List" value="~I0440~I0441~I0442~I0443~I0444"/>
        Dim sAscendBUList As String = "~I0440~I0441~I0442~I0443~I0444"
        Try
            sAscendBUList = UCase(ConfigurationSettings.AppSettings("ASCEND_BU_List").ToString)
        Catch ex As Exception
            sAscendBUList = "~I0440~I0441~I0442~I0443~I0444"
        End Try

        Try
            bIsAscend = (sAscendBUList.IndexOf(sBU.Trim.ToUpper) > -1)
        Catch ex As Exception
            bIsAscend = False
        End Try
        Return bIsAscend
    End Function
End Module
