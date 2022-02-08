Imports System.Configuration
Imports System.Data.OleDb
Imports System.IO
Imports System.Web.Mail
Imports System.Drawing.Color
Imports System.Data.SqlClient
Imports System.Web.UI.WebControls
Imports System.Text
Imports System.Web.UI

Module CustInfoMail

    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = ConfigurationManager.AppSettings("LogPath")
    Dim logpath As String = rootDir & "CUSTInfoNoticationLog" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString")))

    Sub Main()

        Console.WriteLine("Start Cust Info Email Notification")
        Console.WriteLine("")

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

        log.WriteLine("********************End of CUST info Email Notification Utility********************")

        log.Close()


    End Sub

    Private Function buildstatchgout(log As StreamWriter) As StreamWriter

        Dim bolErrorSomeWhere As Boolean
        Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
        connectOR = New OleDbConnection(connectionString)

        ' method to call the 
        Dim strSQLString As String = ""
        Dim varResultCount As String = "0"
        Dim varResultMode As String = "0"

        'query to fetch the line status as CST        
        'strSQLString = "select * from PS_ISA_ORD_INTF_LN where isa_LINE_STATUS='CST'"

        log.WriteLine("------------------------------------------------------------------------------------------")
        log.WriteLine("Start to fetch the orders that is need customer infromation, by using ISA_LINE_STATUS='CST' IN order line table")

        strSQLString = "select oln.BUSINESS_UNIT_OM, oln.ORDER_NO,oln.isa_intfc_ln, oln.ISA_WORK_ORDER_NO " & vbCrLf &
                        " , usr.ISA_EMPLOYEE_EMAIL , usr.PHONE_NUM, oln.OPRID_ENTERED_BY " & vbCrLf &
                        "  , oln.ISA_REQUIRED_BY_DT, usr.ISA_EMPLOYEE_NAME,usr.ISA_EMPLOYEE_ID,ohd.order_date ,RLN.buyer_id " & vbCrLf &
                        "from PS_ISA_ORD_INTF_LN oln " & vbCrLf &
                        "join sdix_users_tbl usr on usr.ISA_EMPLOYEE_ID = oln.ISA_EMPLOYEE_ID join PS_ISA_ORD_INTF_HD ohd on ohd.ORDER_NO = oln.ORDER_NO join PS_REQ_LINE RLN ON RLN.REQ_ID = oln.ORDER_NO AND RLN.LINE_NBR = oln.isa_intfc_ln " & vbCrLf &
                        "where oln.isa_LINE_STATUS='CST'" & vbCrLf &
                        " group by oln.BUSINESS_UNIT_OM, oln.ORDER_NO, oln.ISA_WORK_ORDER_NO " & vbCrLf &
                        " , usr.ISA_EMPLOYEE_EMAIL , usr.PHONE_NUM, oln.OPRID_ENTERED_BY " & vbCrLf &
                        " , oln.ISA_REQUIRED_BY_DT, usr.ISA_EMPLOYEE_NAME, usr.ISA_EMPLOYEE_ID, ohd.order_date,oln.isa_intfc_ln,RLN.buyer_id"

        Dim dtrAppReader As OleDbDataReader = GetReader(strSQLString)
        If dtrAppReader.HasRows() = True Then
            Dim OrderNo As List(Of String) = New List(Of String)()
            While dtrAppReader.Read()

                Dim strEmpName As String = ""
                Dim strORderNo As String = ""
                Dim strEmail As String = ""
                Dim Required_By_Dttm As String = ""
                Dim strWorkONo As String = ""
                Dim strOrdDate As String = ""
                Dim strBuyer As String = ""
                Dim strUserId As String = ""

                Dim ORDER_NO As String = Convert.ToString(dtrAppReader.Item("ORDER_NO"))


                If Not OrderNo.Contains(Convert.ToString(ORDER_NO)) Then
                        OrderNo.Add(Convert.ToString(ORDER_NO))

                        If Not IsDBNull(dtrAppReader.Item("ISA_EMPLOYEE_NAME")) Then
                            strEmpName = Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_NAME"))
                        End If

                        If Not IsDBNull(dtrAppReader.Item("ISA_EMPLOYEE_ID")) Then
                            strUserId = Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_ID"))
                        End If


                        If Not IsDBNull(dtrAppReader.Item("ORDER_NO")) Then
                            strORderNo = Convert.ToString(dtrAppReader.Item("ORDER_NO"))
                        End If

                        If Not IsDBNull(dtrAppReader.Item("ISA_EMPLOYEE_EMAIL")) Then
                            strEmail = Convert.ToString(dtrAppReader.Item("ISA_EMPLOYEE_EMAIL"))
                        End If


                        If Not IsDBNull(dtrAppReader.Item("ISA_REQUIRED_BY_DT")) Then
                            Required_By_Dttm = CDate(dtrAppReader.Item("ISA_REQUIRED_BY_DT")).ToString("MM-dd-yyyy")
                        End If

                        If Not IsDBNull(dtrAppReader.Item("ISA_WORK_ORDER_NO")) Then
                            strWorkONo = Convert.ToString(dtrAppReader.Item("ISA_WORK_ORDER_NO"))
                        End If

                        If Not IsDBNull(dtrAppReader.Item("order_date")) Then
                            strOrdDate = CDate(dtrAppReader.Item("order_date")).ToString("MM-dd-yyyy")
                        End If

                        If Not IsDBNull(dtrAppReader.Item("buyer_id")) Then
                            strBuyer = Convert.ToString(dtrAppReader.Item("buyer_id"))
                        End If


                        log.WriteLine("OrderNo {0} is need more information from customer {1}", strORderNo, strEmpName)
                        'SDI-41322 Email Notification turn off for user LOWE,ASHLEY
                        Try
                            Dim EmpId As String = ""
                            Dim IsDontNotifierCustomer As Boolean = False

                            EmpId = Convert.ToString(ConfigurationManager.AppSettings("DoNotNotifyCustomer"))

                            IsDontNotifierCustomer = (EmpId.IndexOf(strUserId.Trim.ToUpper) > -1)

                            If IsDontNotifierCustomer = "True" Then
                                'Don't Send notification
                            Else
                                buildNotifyApprover(strORderNo, strEmail, strEmpName, Required_By_Dttm, strWorkONo, strOrdDate, strBuyer)
                            End If
                        Catch ex As Exception
                            buildNotifyApprover(strORderNo, strEmail, strEmpName, Required_By_Dttm, strWorkONo, strOrdDate, strBuyer)
                        End Try

                End If
            End While
            dtrAppReader.Close()
        Else
            log.WriteLine("No record found to notify the employees")
            dtrAppReader.Close()
        End If
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        Return log

    End Function

    Public Sub buildNotifyApprover(ByVal orderNum As String, ByVal EmpEmail As String, ByVal EmpNme As String, ByVal Required_By_Dttm As String, ByVal strWorkONo As String, ByVal strOrdDate As String, ByVal strBuyer As String)

        Dim strPuncher As Boolean = False
        Dim I As Integer
        Dim X As Integer
        Dim strbodyhead As String = ""
        Dim strbodydetl As String = ""
        Dim strItemtype As String = ""
        Dim intGridloop As Integer
        Dim decOrderTot As Decimal

        Dim dr As DataRow
        Dim dstcart2 As New DataTable
        Dim txtHdr As String = ""


        ''Build Line item Grid
        Dim dtgcart As DataGrid
        Dim SBstk As New StringBuilder
        Dim SWstk As New StringWriter(SBstk)
        Dim htmlTWstk As New HtmlTextWriter(SWstk)
        Dim dataGridHTML As String = String.Empty
        Dim dstcartSTK As New DataTable
        Dim StrWO1 As String = " "
        dstcartSTK = buildCartforemail(orderNum, StrWO1)
        If Trim(StrWO1) = "" Then
            StrWO1 = " "
        End If
        '' itmQuoted.WorkOrderNumber = StrWO1

        If dstcartSTK.Rows.Count > 0 Then
            dtgcart = New DataGrid
            dtgcart.DataSource = dstcartSTK
            dtgcart.DataBind()
            dtgcart.CellPadding = 3
            dtgcart.BorderColor = Gray
            dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
            dtgcart.HeaderStyle.Font.Bold = True
            dtgcart.HeaderStyle.ForeColor = Black
            dtgcart.Width.Percentage(90)
            dtgcart.RenderControl(htmlTWstk)
            dataGridHTML = SBstk.ToString()
        End If

        Dim Mailer As MailMessage = New MailMessage
        Dim FromAddress As String = "SDIExchange@SDI.com"
        Dim Mailcc As String = ""
        Dim MailBcc As String = "webdev@sdi.com;Tony.Smith@sdi.com;avacorp@sdi.com"

        '' strbodyhead = "<span><B>**PRIORITY ORDER**</B></span>"

        strbodyhead = "<table width='100%' bgcolor='black'><tbody><tr><td><img src='https://www.sdiexchange.com/images/SDNewLogo_Email.png' alt='SDI' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large; text-align: center; Color:White;'>SDI Marketplace</span></center><center><span style='text-align: center; margin: 0px auto;  Color:White;'>SDiExchange - Customer Info Notification</span></center></td></tr></tbody></table>" & vbCrLf
        strbodyhead = strbodyhead & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
        strbodyhead = strbodyhead & "<TABLE class='Email_Table' cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = "<div>"

        'strbodydetl = "&nbsp;" & vbCrLf
        'strbodydetl = strbodydetl & "<div>"
        'strbodydetl = strbodydetl & "<p>Employee Name:" & EmpNme & ",<br>"
        'strbodydetl = strbodydetl & "&nbsp;<br></p>"

        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"
        strbodydetl = strbodydetl & "<p ><span style='font-weight:bold;'>Employee Name: </span><span>   </span>" & EmpNme & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Order No: </span><span>    </span>" & orderNum & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Work Order No: </span><span>   </span>" & strWorkONo & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Orderd by Date: </span><span>  </span>" & strOrdDate & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Request by Date: </span><span> </span>" & Required_By_Dttm & "<br>"
        strbodydetl = strbodydetl & "&nbsp;<br></p>"

        'strbodydetl = strbodydetl & "<p>Processing for Order " & orderNum & " is currently on hold awaiting customer supplied information. Please provide the necessary information so processing can resume.</p>"
        strbodydetl = strbodydetl & "<p>Processing for this order is currently in hold, awaiting additional customer information. Please contact your buyer " & strBuyer & ".</P>"
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
                   DbUrl.Substring(DbUrl.Length - 4).ToUpper = "STST" Or
               DbUrl.Substring(DbUrl.Length - 4).ToUpper = "RPTG" Then
            Mailer.To = "WebDev@sdi.com;avacorp@sdi.com"
            Mailer.Subject = "<<TEST SITE>> SDiExchange - Customer Information Required For Order - " & orderNum
        Else
            Mailer.To = EmpEmail
            Mailer.Bcc = "WebDev@sdi.com;avacorp@sdi.com"
            Mailer.Subject = "SDiExchange - Customer Information Required For Order - " & orderNum
        End If


        Mailer.BodyFormat = MailFormat.Html
        '' Mailer.IsBodyHtml = True
        Dim connectionEmail As OleDbConnection = New OleDbConnection((Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString"))))
        connectionEmail.Open()
        Try
            Dim SDIEmailService As EmailService.EmailServices = New EmailService.EmailServices()
            SDIEmailService.EmailUtilityServices("MailandStore", "SDIExchange@SDI.com", Mailer.To.ToString(), Mailer.Subject, "", "", Mailer.Body, "CustInfoMailer", Nothing, Nothing)

            ''UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, FromAddress, Mailer.To.ToString(), Mailcc, MailBcc, "CustInfoMailer", Mailer.Body, connectionEmail)
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

        txtHdr = txtHdr & vbCrLf

        Dim strOROItemID As String
        Dim bolMachineNum As Boolean = False
        Dim nItem_id_Price As Integer = 0

    End Sub

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




    Private Function getBinLoc(ByVal stritemid) As String
        'changed to qty >0 rather than hitting the first bin in the array
        Dim strSQLString As String = "" & _
                    "SELECT " & vbCrLf & _
                    " (C.STORAGE_AREA ||" & vbCrLf & _
                    "  C.STOR_LEVEL_1 ||" & vbCrLf & _
                    "  C.STOR_LEVEL_2 ||" & vbCrLf & _
                    "  C.STOR_LEVEL_3 ||" & vbCrLf & _
                    "  C.STOR_LEVEL_4) as binloc " & vbCrLf & _
                    "FROM " & vbCrLf & _
                    " PS_INV_ITEMS B " & vbCrLf & _
                    ",PS_PHYSICAL_INV C " & vbCrLf & _
                    "WHERE B.INV_ITEM_ID = '" & stritemid & "' " & vbCrLf & _
                    "  AND B.EFFDT = (" & vbCrLf & _
                    "                 SELECT MAX(B_ED.EFFDT) FROM PS_INV_ITEMS B_ED " & vbCrLf & _
                    "  	 	          WHERE B.SETID = B_ED.SETID " & vbCrLf & _
                    "  		            AND B.INV_ITEM_ID = B_ED.INV_ITEM_ID " & vbCrLf & _
                    "		            AND B_ED.EFFDT <= SYSDATE" & vbCrLf & _
                    "                ) " & vbCrLf & _
                    "  AND B.INV_ITEM_ID = C.INV_ITEM_ID(+) " & vbCrLf & _
                    " AND C.QTY > 0 " & vbCrLf & _
                    "ORDER BY C.DT_TIMESTAMP DESC " & vbCrLf & _
                    ""

        Try
            'pfd change the getscalar to getadapter - to creates a dataset to have multiple bin locations to be displayed on the email

            Dim dsbin As DataSet = GetAdapter(strSQLString)
            If Not (dsbin Is Nothing) Then
                If dsbin.Tables(0).Rows.Count > 0 Then
                    For Each row As DataRow In dsbin.Tables(0).Rows
                        getBinLoc = getBinLoc + "<BR>" + row.Item("BINLOC")
                    Next
                Else
                    getBinLoc = " "
                End If
            Else
                getBinLoc = " "
            End If
            ';;;;
            ' before bitch at Siemens
            'getBinLoc = ORDBData.GetScalar(strSQLString)
            'Return getBinLoc
            ';;;;
        Catch objException As Exception

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
                      ByRef strWrkOrder As String) As DataTable

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


            strOraSelectQuery = "select * from SYSADM8.PS_ISA_ORD_INTF_LN where ORDER_NO = '" & ordNumber & "' AND ISA_LINE_STATUS='CST' "
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

End Module
