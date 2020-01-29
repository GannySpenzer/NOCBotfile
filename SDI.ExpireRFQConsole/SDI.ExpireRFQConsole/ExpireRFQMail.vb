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
       
        log.Close()

    End Sub

    Public Function buildstatchgout(log As StreamWriter) As StreamWriter

        Dim bolErrorSomeWhere As Boolean
        Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
        connectOR = New OleDbConnection(connectionString)

        ' method to call the 
        Dim strSQLString As String = ""
        Dim varResultCount As String = "0"
        Dim varResultMode As String = "0"
        Dim Ordlist_25 As New List(Of String)
        Dim Ordlist_30 As New List(Of String)
        Dim Ordlist_31 As New List(Of String)



        'query to fetch the day 25
        'strSQLString = "select * from PS_ISA_ORD_INTF_LN A, PS_ISA_ORD_INTF_HD B" & vbCrLf & _
        '        " where B.ORIGIN = 'RFQ' AND A.Order_NO = B.Order_No AND TO_CHAR(A.Approval_Dttm + 24, 'DD-MON-YYYY') = TO_CHAR(CURRENT_DATE, 'DD-MON-YYYY')"
        strSQLString = "SELECT TO_CHAR(S.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP , TO_CHAR(H.order_date, 'MM-DD-YYYY') AS order_date, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S, ps_isa_ord_intf_hd H WHERE a.isa_line_status='QTS' AND  A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status  and H.ORDER_NO = A.ORDER_NO  AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') = TO_CHAR(SYSDATE - 24, 'DD-MON-YYYY')"


        Dim dtrAppReader As OleDbDataReader = GetReader(strSQLString)

        'To get Employye Email ID
        log.WriteLine("------------------------------------------------------------------------------------------")
        log.WriteLine("Start to fetch the orders that is not approved for more than 25 days")

        If dtrAppReader.HasRows() = True Then

            While dtrAppReader.Read()
                varResultCount = (dtrAppReader.Item(0)).ToString()

                'If varResultCount = "0" Then
                '    varResultMode = "0"
                'Else
                log.WriteLine("OrderNo {0} is not approved by employee {1},notificaton sent to user emailID for confirmation", Convert.ToString(dtrAppReader.Item("ORDER_NO")), Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_ID")))

                strSQLString = "SELECT ISA_EMPLOYEE_EMAIL FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" & dtrAppReader.Item("ISA_EMPLOYEE_ID").ToString() & "' "
                Dim empEmail As String = GetScalar(strSQLString)


                varResultMode = "25"

                Dim BU As String = Convert.ToString(dtrAppReader.Item("Business_Unit_Om"))
                Dim Ord_No As String = Convert.ToString(dtrAppReader.Item("ORDER_NO"))
                Dim OPRID_Appr_By As String = Convert.ToString(dtrAppReader.Item("OPRID_APPROVED_BY"))
                Dim Appr_Dttm As String = Convert.ToString(dtrAppReader.Item("APPROVAL_DTTM"))
                Dim ITM_SETID As String = Convert.ToString(dtrAppReader.Item("ITM_SETID"))
                Dim SELL_Price As String = Convert.ToString(dtrAppReader.Item("ISA_SELL_PRICE"))
                Dim Required_By_Dttm As String = CDate(dtrAppReader.Item("ISA_REQUIRED_BY_DT")).ToString("MM-dd-yyyy")
                Dim Decriptions As String = Convert.ToString(dtrAppReader.Item("DESCR254"))
                Dim Order_date As String = Convert.ToString(dtrAppReader.Item("order_date"))
                'buildNotifyApprover(dtrAppReader.Item("Business_Unit_Om"), dtrAppReader.Item("ORDER_NO"), dtrAppReader.Item("OPRID_APPROVED_BY"),
                '                dtrAppReader.Item("APPROVAL_DTTM"), dtrAppReader.Item("ITM_SETID"), dtrAppReader.Item("ISA_SELL_PRICE"),
                '                dtrAppReader.Item("ISA_REQUIRED_BY_DT"), dtrAppReader.Item("DESCR254"), varResultMode, empEmail, "24")                

                If Not Ordlist_25.Contains(dtrAppReader.Item("ORDER_NO")) Then
                    Ordlist_25.Add(dtrAppReader.Item("ORDER_NO"))
                    buildNotifyApprover(BU, Ord_No, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm,
                                    Decriptions, varResultMode, empEmail, "24", Order_date)
                Else
                    ''Ordlist.Add(dtrAppReader.Item("ORDER_NO"))
                End If

                'End If
            End While
            dtrAppReader.Close()
        Else
            log.WriteLine("No records found to notify the user")
            dtrAppReader.Close()
        End If

        log.WriteLine("---------------------****End of 25 days process****-----------------------")

        'query to fetch the day 30
        'strSQLString = "select * from PS_ISA_ORD_INTF_LN A, PS_ISA_ORD_INTF_HD B" & vbCrLf & _
        '        " where B.ORIGIN = 'RFQ' AND A.Order_NO = B.Order_No AND TO_CHAR(A.Approval_Dttm + 29, 'DD-MON-YYYY') = TO_CHAR(CURRENT_DATE, 'DD-MON-YYYY')"

        log.WriteLine("------------------------------------------------------------------------------------------")
        log.WriteLine("Start to fetch the orders that is not approved for more than 30 days")

        strSQLString = "SELECT TO_CHAR(S.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP , TO_CHAR(H.order_date, 'MM-DD-YYYY') AS order_date, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S, ps_isa_ord_intf_hd H WHERE a.isa_line_status='QTS' AND  A.ORDER_NO=S.ORDER_NO and H.ORDER_NO = A.ORDER_NO  and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') = TO_CHAR(SYSDATE - 29, 'DD-MON-YYYY') order by S.DTTM_STAMP desc"

        dtrAppReader = GetReader(strSQLString)
        If dtrAppReader.HasRows() = True Then

            While dtrAppReader.Read()
                'If varResultCount = "0" Then
                '    varResultMode = "0"
                'Else


                strSQLString = "SELECT ISA_EMPLOYEE_EMAIL FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" & dtrAppReader.Item("ISA_EMPLOYEE_ID").ToString() & "' "
                Dim empEmail1 As String = GetScalar(strSQLString)

                varResultMode = "30"

                log.WriteLine("OrderNo {0} is not approved by employee {1},notificaton sent to user emailID for confirmation", Convert.ToString(dtrAppReader.Item("ORDER_NO")), Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_ID")))

                Dim BU As String = Convert.ToString(dtrAppReader.Item("Business_Unit_Om"))
                Dim Ord_No As String = Convert.ToString(dtrAppReader.Item("ORDER_NO"))
                Dim OPRID_Appr_By As String = Convert.ToString(dtrAppReader.Item("OPRID_APPROVED_BY"))
                Dim Appr_Dttm As String = Convert.ToString(dtrAppReader.Item("APPROVAL_DTTM"))
                Dim ITM_SETID As String = Convert.ToString(dtrAppReader.Item("ITM_SETID"))
                Dim SELL_Price As String = Convert.ToString(dtrAppReader.Item("ISA_SELL_PRICE"))
                Dim Required_By_Dttm As String = CDate(dtrAppReader.Item("ISA_REQUIRED_BY_DT")).ToString("MM-dd-yyyy")
                Dim Decriptions As String = Convert.ToString(dtrAppReader.Item("DESCR254"))
                Dim Order_date As String = Convert.ToString(dtrAppReader.Item("order_date"))
                'buildNotifyApprover(dtrAppReader.Item("Business_Unit_Om"), dtrAppReader.Item("ORDER_NO"), dtrAppReader.Item("OPRID_APPROVED_BY"),
                '                dtrAppReader.Item("APPROVAL_DTTM"), dtrAppReader.Item("ITM_SETID"), dtrAppReader.Item("ISA_SELL_PRICE"),
                '                dtrAppReader.Item("ISA_REQUIRED_BY_DT"), dtrAppReader.Item("DESCR254"), varResultMode, empEmail1, "29")
                'End If
                If Not Ordlist_30.Contains(dtrAppReader.Item("ORDER_NO")) Then
                    Ordlist_30.Add(dtrAppReader.Item("ORDER_NO"))
                    buildNotifyApprover(BU, Ord_No, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm,
                                    Decriptions, varResultMode, empEmail1, "29", Order_date)
                Else
                    ''Ordlist.Add(dtrAppReader.Item("ORDER_NO"))
                End If

            End While
            dtrAppReader.Close()
        Else
            log.WriteLine("No records found to notify the user")
            dtrAppReader.Close()
        End If
        log.WriteLine("---------------------****End of 30 days process****-----------------------")


        log.WriteLine("------------------------------------------------------------------------------------------")
        log.WriteLine("Start to expire orders that is not approved for more than 31 days")

        'query to fetch the day 31
        ''strSQLString = "select * from PS_ISA_ORD_INTF_LN A, PS_ISA_ORD_INTF_HD B" & vbCrLf & _
        ''        " where B.ORIGIN = 'RFQ' AND A.Order_NO = B.Order_No AND TO_CHAR(A.Approval_Dttm + 30, 'DD-MON-YYYY') = TO_CHAR(CURRENT_DATE, 'DD-MON-YYYY')"

        strSQLString = "SELECT TO_CHAR(S.DTTM_STAMP, 'MM-DD-YYYY') AS DTTM_STAMP , TO_CHAR(H.order_date, 'MM-DD-YYYY') AS order_date, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S, ps_isa_ord_intf_hd H WHERE a.isa_line_status='QTS' AND  A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status and H.ORDER_NO = A.ORDER_NO  AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') >= TO_CHAR(SYSDATE - 30, 'DD-MON-YYYY') order by S.DTTM_STAMP desc"

        dtrAppReader = GetReader(strSQLString)
        If dtrAppReader.HasRows() = True Then
            While dtrAppReader.Read()
                'If varResultCount = "0" Then
                '    varResultMode = "0"
                'Else
                varResultMode = "31"
                'buildNotifyApprover(dtrAppReader.Item("Business_Unit_Om"), dtrAppReader.Item("ORDER_NO"), dtrAppReader.Item("OPRID_APPROVED_BY"),
                '                dtrAppReader.Item("APPROVAL_DTTM"), dtrAppReader.Item("ITM_SETID"), dtrAppReader.Item("ISA_SELL_PRICE"),
                '                dtrAppReader.Item("ISA_REQUIRED_BY_DT"), dtrAppReader.Item("DESCR254"), varResultMode)
                Dim strSQLstringUpt As String
                Dim rowsaffectedUpt As Integer
                Dim ff As String = dtrAppReader.Item("ORDER_NO")

                Dim trnsactSession As OleDbTransaction = Nothing
                Dim connection As OleDbConnection = New OleDbConnection(ConfigurationManager.AppSettings("OLEDBconString"))
                Dim rowsAffected As Integer = 0

                'strSQLstringUpt = "UPDATE PS_ISA_ORD_INTF_LN" & vbCrLf & _
                '               " SET ISA_LINE_STATUS = 'EXP'" & vbCrLf & _
                '               " WHERE ORDER_NO = '" & dtrAppReader.Item("ORDER_NO") & "'"

                Dim StatusChk = Convert.ToString(ConfigurationManager.AppSettings("SetStatus"))

                If StatusChk.ToUpper() = "EXP" Or StatusChk.ToUpper() = "YES" Then
                    strSQLstringUpt = "UPDATE PS_ISA_ORD_INTF_LN SET ISA_LINE_STATUS = 'EXP' WHERE ORDER_NO= '" & Convert.ToString(dtrAppReader.Item("ORDER_NO")) & "' AND ISA_INTFC_LN = " & Convert.ToString(dtrAppReader.Item("ISA_INTFC_LN")) & ""
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
                Else

                    strSQLString = "SELECT ISA_EMPLOYEE_EMAIL FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" & dtrAppReader.Item("ISA_EMPLOYEE_ID").ToString() & "' "
                    Dim empEmail1 As String = GetScalar(strSQLString)

                    varResultMode = "31"

                    Dim BU As String = Convert.ToString(dtrAppReader.Item("Business_Unit_Om"))
                    Dim Ord_No As String = Convert.ToString(dtrAppReader.Item("ORDER_NO"))
                    Dim OPRID_Appr_By As String = Convert.ToString(dtrAppReader.Item("OPRID_APPROVED_BY"))
                    Dim Appr_Dttm As String = Convert.ToString(dtrAppReader.Item("APPROVAL_DTTM"))
                    Dim ITM_SETID As String = Convert.ToString(dtrAppReader.Item("ITM_SETID"))
                    Dim SELL_Price As String = Convert.ToString(dtrAppReader.Item("ISA_SELL_PRICE"))
                    Dim Required_By_Dttm As String = CDate(dtrAppReader.Item("ISA_REQUIRED_BY_DT")).ToString("MM-dd-yyyy")
                    Dim Decriptions As String = Convert.ToString(dtrAppReader.Item("DESCR254"))
                    Dim Order_date As String = Convert.ToString(dtrAppReader.Item("order_date"))
                    'buildNotifyApprover(dtrAppReader.Item("Business_Unit_Om"), dtrAppReader.Item("ORDER_NO"), dtrAppReader.Item("OPRID_APPROVED_BY"),
                    '                dtrAppReader.Item("APPROVAL_DTTM"), dtrAppReader.Item("ITM_SETID"), dtrAppReader.Item("ISA_SELL_PRICE"),
                    '                dtrAppReader.Item("ISA_REQUIRED_BY_DT"), dtrAppReader.Item("DESCR254"), varResultMode, empEmail1, "29")
                    'End If
                    If Not Ordlist_31.Contains(dtrAppReader.Item("ORDER_NO")) Then
                        Ordlist_31.Add(dtrAppReader.Item("ORDER_NO"))
                        buildNotifyApprover(BU, Ord_No, OPRID_Appr_By, Appr_Dttm, ITM_SETID, SELL_Price, Required_By_Dttm,
                                        Decriptions, varResultMode, empEmail1, "30", Order_date)
                    Else
                        ''Ordlist.Add(dtrAppReader.Item("ORDER_NO"))
                    End If

                End If

                'End If
            End While
            dtrAppReader.Close()
        Else
            log.WriteLine("No records found to expire order")
            dtrAppReader.Close()
        End If

        log.WriteLine("---------------------****End of 31 day expire process****-----------------------")

        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        Return log

    End Function

    Public Sub buildNotifyApprover(ByVal businessUnit As String, ByVal orderNum As String, ByVal apprvBy As String,
                                    ByVal apprvDate As String, ByVal itmSet As String, ByVal sellPrice As String,
                                    ByVal itmReqDate As String, ByVal itmDescr As String, ByVal mode As String, ByVal empEmail1 As String, ByVal DateInterval As String, ByVal orderDate As String)

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
        dstcartSTK = buildCartforemail(orderNum, StrWO1, DateInterval)
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
        'strbodydetl = strbodydetl & "<p ><span style='font-weight:bold;'>BUSINESS UNIT: </span><span>      </span>" & businessUnit & "<br>"
        'strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<p ><span style='font-weight:bold;'>Order No: </span><span>    </span>" & orderNum & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Orderd by Date: </span><span>  </span>" & orderDate & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Approved Date: </span><span>  </span>" & apprvDate & "<br>"
        'strbodydetl = strbodydetl & "&nbsp;<br>"
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>ITEM SET: </span><span>  </span>" & itmSet & "<br>"
        'strbodydetl = strbodydetl & "&nbsp;<br>"
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>ITEM SELL PRICE: </span><span>  </span>" & sellPrice & "<br>"
        'strbodydetl = strbodydetl & "&nbsp;<br>"
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>ITEM REQUESTED DATE: </span><span>  </span>" & itmReqDate & "<br>"
        'strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Request by Date: </span><span>  </span>" & itmReqDate & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br></p>"
        If mode = "25" Then
            strbodydetl = strbodydetl & "<p>The above referenced order is waiting for your approval and will expire in 5 days. </p>"
        ElseIf mode = "30" Then
            strbodydetl = strbodydetl & "<p>The above referenced order is waiting for your approval and will expire at the end of the day. </p>"
            'ElseIf mode = "31" Then
            '    strbodydetl = strbodydetl & "Expired"
        Else
            strbodydetl = strbodydetl & "<p>The above referenced order is waiting for your approval. </p>"
        End If



        Mailer.Body = strbodyhead & strbodydetl

        Mailer.Body = Mailer.Body & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
        Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
        'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
        Mailer.Body = Mailer.Body & "</TABLE>" & vbCrLf
        Mailer.Body = Mailer.Body & "<HR width='100%' SIZE='1'>" & vbCrLf
        Mailer.Body = Mailer.Body & "<img src='https://www.sdiexchange.com/Images/SDIFooter_Email.png' />" & vbCrLf

        If DbUrl.Substring(DbUrl.Length - 4).ToUpper = "PLGR" Or _
                   DbUrl.Substring(DbUrl.Length - 4).ToUpper = "STAR" Or _
                   DbUrl.Substring(DbUrl.Length - 4).ToUpper = "DEVL" Or _
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
            Dim dataAdapter As OleDbDataAdapter = _
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


    Private Function buildCartforemail(ByVal ordNumber As String, _
                     ByRef strWrkOrder As String, ByRef DateInterval As String) As DataTable

        Dim dr As DataRow
        Dim I As Integer
        Dim strPrice As String
        Dim strQty As String
        Dim dstcart As DataTable
        dstcart = New DataTable

        dstcart.Columns.Add("Item ID")
        dstcart.Columns.Add("Description")
        dstcart.Columns.Add("Manuf.")
        dstcart.Columns.Add("Manuf. Partnum")
        dstcart.Columns.Add("QTY")
        dstcart.Columns.Add("UOM")
        dstcart.Columns.Add("Price")
        dstcart.Columns.Add("Ext. Price")
        ''dstcart.Columns.Add("Approval Date")
        ' dstcart.Columns.Add("Item ID")
        'dstcart.Columns.Add("Bin Location")
        'dstcart.Columns.Add("Item Chg Code")
        'dstcart.Columns.Add("Requestor Name")
        'dstcart.Columns.Add("RFQ")
        'dstcart.Columns.Add("Machine Num")
        'dstcart.Columns.Add("Tax Exempt")
        'dstcart.Columns.Add("LPP")
        'dstcart.Columns.Add("PO")
        dstcart.Columns.Add("LN")
        'dstcart.Columns.Add("SerialID")

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
            'strOraSelectQuery = "select * from SYSADM8.PS_ISA_ORD_INTF_HD where ORDER_NO = '" & ordNumber & "'"
            ''strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_H where ORDER_NO = 'M220016429'" 
            ''strOraSelectQuery = "select * from PS_ISA_ORD_INTFC_H where ORDER_NO = 'M220016427'"
            'OrcRdr = GetReader(strOraSelectQuery)
            'If OrcRdr.HasRows Then
            '    OrcRdr.Read()
            '    ordIdentifier = CType(OrcRdr("ISA_IDENTIFIER"), String).Trim()
            '    ordBU = CType(OrcRdr("BUSINESS_UNIT_OM"), String).Trim()
            'End If

            ''strOraSelectQuery = "select * from SYSADM8.PS_ISA_ORD_INTF_LN where ORDER_NO = '" & ordNumber & "' AND TO_CHAR(Approval_Dttm + 30, 'DD-MON-YYYY') = TO_CHAR(CURRENT_DATE, 'DD-MON-YYYY')"
            If DateInterval = "30" Then
                strOraSelectQuery = "SELECT S.DTTM_STAMP, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S WHERE a.isa_line_status='QTS' AND A.ORDER_NO= '" & ordNumber & "' AND A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') <= TO_CHAR(SYSDATE - '" & DateInterval & "', 'DD-MON-YYYY')"
            Else
                strOraSelectQuery = "SELECT S.DTTM_STAMP, A.* FROM ps_isa_ord_intf_ln A, PS_ISAORDSTATUSLOG S WHERE a.isa_line_status='QTS' AND A.ORDER_NO= '" & ordNumber & "' AND A.ORDER_NO=S.ORDER_NO and A.isa_intfc_ln = S.isa_intfc_ln AND a.isa_line_status = S.isa_line_status AND TO_CHAR(S.DTTM_STAMP, 'DD-MON-YYYY') = TO_CHAR(SYSDATE - '" & DateInterval & "', 'DD-MON-YYYY')"
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


End Module
