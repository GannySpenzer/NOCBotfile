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
    Dim rootDir As String = "C:\Program Files (x86)\SDI\SDI_CustInfoMailer"
    Dim logpath As String = "C:\Program Files (x86)\SDI\SDI_CustInfoMailer\LOGS" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString")))

    Sub Main()

        Console.WriteLine("Start Cust Info Email Notification")
        Console.WriteLine("")

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("  Send emails out " & Now())

        Dim bolError As Boolean = buildstatchgout()

        If bolError = True Then
            'SendEmail()
        End If


        objStreamWriter.WriteLine("  End of Cust Info Email Notification " & Now())
        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub

    Private Function buildstatchgout() As Boolean

        Dim bolErrorSomeWhere As Boolean
        Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
        connectOR = New OleDbConnection(connectionString)

        ' method to call the 
        Dim strSQLString As String = ""
        Dim varResultCount As String = "0"
        Dim varResultMode As String = "0"

        'query to fetch the line status as CST        
        'strSQLString = "select * from PS_ISA_ORD_INTF_LN where isa_LINE_STATUS='CST'"

        strSQLString = "select oln.BUSINESS_UNIT_OM, oln.ORDER_NO, oln.ISA_WORK_ORDER_NO " & vbCrLf & _
                        " , usr.ISA_EMPLOYEE_EMAIL , usr.PHONE_NUM, oln.OPRID_ENTERED_BY " & vbCrLf & _
                        " , oln.ISA_REQUIRED_BY_DT, usr.ISA_EMPLOYEE_NAME" & vbCrLf & _
                        "from PS_ISA_ORD_INTF_LN oln " & vbCrLf & _
                        "join sdix_users_tbl usr on usr.ISA_EMPLOYEE_ID = oln.ISA_EMPLOYEE_ID" & vbCrLf & _
                        "where oln.isa_LINE_STATUS='CST'" & vbCrLf & _
                        " group by oln.BUSINESS_UNIT_OM, oln.ORDER_NO, oln.ISA_WORK_ORDER_NO " & vbCrLf & _
                        " , usr.ISA_EMPLOYEE_EMAIL , usr.PHONE_NUM, oln.OPRID_ENTERED_BY " & vbCrLf & _
                        " , oln.ISA_REQUIRED_BY_DT, usr.ISA_EMPLOYEE_NAME"

        Dim dtrAppReader As OleDbDataReader = GetReader(strSQLString)
        If dtrAppReader.HasRows() = True Then
            While dtrAppReader.Read()
                varResultCount = (dtrAppReader.Item(0)).ToString()
                'Dim apprvBy As String = ""
                'Dim apprvDat As String = ""
                'apprvBy = dtrAppReader.Item("OPRID_APPROVED_BY").ToString
                'apprvDat = dtrAppReader.Item("APPROVAL_DTTM").ToString

                buildNotifyApprover(dtrAppReader.Item("BUSINESS_UNIT_OM"), dtrAppReader.Item("ORDER_NO"),
                                    dtrAppReader.Item("ISA_WORK_ORDER_NO"), dtrAppReader.Item("ISA_REQUIRED_BY_DT"),
                                    dtrAppReader.Item("OPRID_ENTERED_BY"), dtrAppReader.Item("ISA_EMPLOYEE_EMAIL"),
                                    dtrAppReader.Item("PHONE_NUM"),
                                    dtrAppReader.Item("ISA_EMPLOYEE_NAME"), dtrAppReader.Item("ISA_EMPLOYEE_NAME"))

            End While
            dtrAppReader.Close()
        Else
            dtrAppReader.Close()
        End If
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        Return bolErrorSomeWhere

    End Function

    Public Sub buildNotifyApprover(ByVal businessUnit As String, ByVal orderNum As String,
                                   ByVal workOrderNo As String, ByVal ReqDate As String,
                                   ByVal OPREntBy As String, ByVal EmpEmail As String,
                                   ByVal phoneNum As String,
                                   ByVal EmpNme As String, ByVal SubmitDate As String)



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

        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<div>"
        strbodydetl = strbodydetl & "<p >Employee Name:" & EmpNme & ",<br>"
        strbodydetl = strbodydetl & "&nbsp;<br>"

        'strbodydetl = strbodydetl & "<TD colspan=2>" & vbCrLf
        'strbodydetl = strbodydetl & "&nbsp;</td></tr>" & vbCrLf
        ''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        ''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>SDI Requisition Number:</span> <span 'width:128px;'>&nbsp;" & stritemid & "</span></td>"
        'strbodydetl = strbodydetl & "<tr><TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Name:</span> <span>&nbsp;" & EmpNme & "</span></td>"
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Email:</span> <span>&nbsp;" & EmpEmail & "</span></td>"
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Phone#:</span> <span>&nbsp;" & phoneNum & "</span></td>" & vbCrLf
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Work Order #:</span> <span>&nbsp;" & workOrderNo & "</span></td>" & vbCrLf
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Request by Date:</span> <span>&nbsp;" & ReqDate & "</span></td>"
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Submit Date:</span> <span>&nbsp;" & Now() & "</span></td></tr>"
        ''strbodydetl = strbodydetl & "<TD colspan='2'>" & vbCrLf
        ''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Notes:</span><br>"
        ''strbodydetl = strbodydetl & "<textarea readonly='readonly' style='width:100%;'>" & dstcart2.Rows(0).Item(4) & "</textarea></td>" & vbCrLf
        ''strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        ''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        ''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>OPR Entered By:</span> <span>&nbsp;" & OPREntBy & "</span></td></table>"
        'strbodydetl = strbodydetl & "&nbsp;<br>" & vbCrLf
        
        'strbodydetl = strbodydetl & "<p>Processing for Order " & orderNum & " is currently on hold awaiting customer supplied information. Please provide the necessary information so processing can resume.</p>"
        strbodydetl = strbodydetl & "<p>Processing for " & orderNum & " order is currently suspended, awaiting additional customer information. Please contact your buyer.</P>"
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
            Mailer.To = "WebDev@sdi.com;avacorp@sdi.com"
            Mailer.Subject = "<<TEST SITE>> SDiExchange - Cust Info for the Item Status - CST - " & orderNum
        Else
            Mailer.To = EmpEmail
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
