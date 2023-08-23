Imports System.Configuration
Imports System.Data.OleDb
Imports System.IO
Imports System.Web
Imports System.Web.Mail
Imports System.Text
Imports System.Xml
Imports System.Web.UI

Module Module1
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = ConfigurationManager.AppSettings("LogPath")
    Dim logpath As String = rootDir & "OrderStatusSummary" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString")))
    Dim SumryMailTime As String = ConfigurationManager.AppSettings("SummaryMailTime")



    Sub Main()
        Console.WriteLine("Start Orderstatus summary Email send")
        Console.WriteLine("")
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
        objStreamWriter = New StreamWriter(fileStream)
        objStreamWriter.WriteLine("  Send emails out " & Now())
        Dim bolError As Boolean = buildstatchgout()

        If bolError = True Then
            SendEmail()
            objStreamWriter.WriteLine("  Error in Order status summary email " & Now())

        End If
        objStreamWriter.WriteLine("  End of Orderstatus Email send " & Now())
        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub
    'Madhu-INC0025710-Splitting up the order status  summary Email
    Private Function buildstatchgout() As Boolean
        Dim I As Integer
        Dim bolErrorSomeWhere As Boolean

        Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
        connectOR = New OleDbConnection(connectionString)
        Dim dsBU As DataSet
        dsBU = GetBU()

        If Not dsBU Is Nothing Then
            objStreamWriter.WriteLine("Total BU going to Process " + Convert.ToString(dsBU.Tables(0).Rows.Count()))

            Console.WriteLine("----------------------------------- Order Status Summary Emails ------------------------------------------------------------")
            objStreamWriter.WriteLine("-------------------------------------------------------------------------------")
            For I = 0 To dsBU.Tables(0).Rows.Count - 1


                If (dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT") = "I0W01" Or dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT") = "I0631") Then
                    Try
                        Console.WriteLine(Convert.ToString(I + 1) + ".Order Status Email Completed for BU: " + Convert.ToString(dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT")) + "")
                        objStreamWriter.WriteLine(Convert.ToString(I + 1) + ".Order Status Email Completed for BU: " + Convert.ToString(dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT")) + " " & Now())
                        objStreamWriter.WriteLine("--------------------------------------------------------------------------------------")
                        objStreamWriter.WriteLine("  StatChg Email send allstatus emails for Enterprise BU : " & dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT") & " " & Now())
                        buildstatchgout = checkAllStatusWAL(dsBU.Tables(0).Rows(I).Item("BUSINESS_UNIT"))
                    Catch ex As Exception

                    End Try

                End If

                If buildstatchgout = True Then
                    bolErrorSomeWhere = True
                End If
            Next

        End If
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try

        Return bolErrorSomeWhere

    End Function
    Public Function getFromMail(ByVal strBU As String, ByVal connectOR As OleDb.OleDbConnection)
        Dim sqlStringEmailFrom As String = ""
        Dim fromMail As String = ""
        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If
        Try
            connectOR.Open()
            sqlStringEmailFrom = "Select ISA_PURCH_EML_FROM from PS_ISA_BUS_UNIT_PM where BUSINESS_UNIT_PO = '" & strBU & "'"
            fromMail = ORDBAccess.GetScalar(sqlStringEmailFrom, connectOR)
        Catch ex As Exception
            If (strBU = "I0W01" Or strBU = "WAL00") Then
                fromMail = "WalmartPurchasing@sdi.com"
            ElseIf (strBU = "EMC00" Or strBU = "I0631") Then
                fromMail = "Emcorpurchasing@sdi.com"
            Else
                fromMail = "SDIExchange@SDI.com"
            End If
        End Try
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try
        Return fromMail
    End Function

    Private Sub SendEmail()

        Dim SDIEmailService As SDIEmailutilityService.EmailServices = New SDIEmailutilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "webdev@sdi.com"  '  "pete.doyle@sdi.com"

        'The subject of the email
        email.Subject = "Error in the 'Order status Summary Email' Console App."

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
    Public Function getToMail(ByVal StrBu As String, ByVal connectOR As OleDb.OleDbConnection)
        Dim sqlStringEmailFrom As String = ""
        Dim toMail As String = ""
        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If
        Try
            connectOR.Open() 'Mythili -- INC0023448 Adding CC emails
            sqlStringEmailFrom = "Select ISA_PURCH_EML_TO from PS_ISA_BUS_UNIT_PM where BUSINESS_UNIT_PO = '" & StrBu & "'"
            toMail = ORDBAccess.GetScalar(sqlStringEmailFrom, connectOR)
        Catch ex As Exception
            If (StrBu = "I0W01" Or StrBu = "WAL00") Then
                toMail = "WalmartPurchasing@sdi.com"
            ElseIf (StrBu = "EMC00" Or StrBu = "I0631") Then
                toMail = "Emcorpurchasing@sdi.com"
            End If
        End Try
        Try
            connectOR.Close()
        Catch ex As Exception

        End Try
        Return toMail
    End Function

    Private Sub sendCustEmailWal(ByVal dtEmail As DataTable, ByVal EmpID As String, ByVal EmailTo As String, ByVal strBU As String)
        Dim SDIEmailService As SDIEmailutilityService.EmailServices = New SDIEmailutilityService.EmailServices()
        Dim MailAttachmentName As String()
        Dim MailAttachmentbytes As New List(Of Byte())()
        Dim strbodyhead1 As String
        Dim strbodydet1 As String
        Dim dataGridHTML1 As String
        Dim SBnstk1 As New StringBuilder
        Dim SWnstk1 As New StringWriter(SBnstk1)
        Dim htmlTWnstk1 As New HtmlTextWriter(SWnstk1)

        Dim Mailer1 As MailMessage = New MailMessage
        Dim strccfirst1 As String = "erwin.bautista"  '  "pete.doyle"
        Dim strcclast1 As String = "sdi.com"
        Mailer1.Cc = ""
        Dim MailCC As String = ""
        'SP-316 get from email from table - Dhamotharan
        'SDI-40628 Changing Mail id as walmartpurchasing@sdi.com from sdiexchange@sdi.com for Walmart BU.
        'If strBU = "I0W01" Then
        '    Mailer1.From = "WalmartPurchasing@sdi.com"
        'Else
        '    Mailer1.From = "SDIExchange@SDI.com"  '  "Insiteonline@SDI.com"
        'End If
        Try
            Dim BU As String = ""
            If strBU = "I0W01" Then
                BU = "WAL00"
                Mailer1.From = getFromMail(BU, connectOR)
            ElseIf strBU = "I0631" Then
                BU = "EMC00"
                Mailer1.From = getFromMail(BU, connectOR)
            Else
                BU = "ISA00"
                Mailer1.From = getFromMail(BU, connectOR)
            End If
            'Mythili -- INC0023448 Adding CC emails
            MailCC = getToMail(BU, connectOR)
        Catch ex As Exception

        End Try


        Mailer1.Bcc = strccfirst1 & "@" & strcclast1
        strbodyhead1 = "<table width='100%' bgcolor='black'><tbody><tr><td><img data-imagetype='External' src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' vspace='0' hspace='0'></td><td width='100%'><center><span style='font-family:Calibri; font-size:x-large; text-align: center; color:White' > SDI Marketplace</span></center><center><span style='text-align: center; margin:0px auto; color:White'>SDiExchange - Order Status</span></center></td></tr></tbody></table>"
        'strbodyhead1 = "<table width='100%'><tbody><tr><td><img src='http://www.sdiexchange.com/images/SDILogo_Email.png' alt='SDI' width='98px' height='82px' vspace='0' hspace='0' /></td><td width='100%'><br /><br /><br /><br /><br /><br /><center><span style='font-family: Calibri; font-size: 32px; text-align: center;font-weight:bold'>SDI Marketplace</span></center><center style='font-size:18px'><span style='text-align: center; margin: 0px auto; font-family:Calibri;'>SDiExchange - Order Status</span></center></td></tr></tbody></table>"
        strbodyhead1 = strbodyhead1 & "<HR width='100%' SIZE='1'>"
        strbodyhead1 = strbodyhead1 & "&nbsp;" & vbCrLf


        Dim strPurchaserName As String = dtEmail(0).Item("First Name") & " " & dtEmail(0).Item("Last Name")
        Dim strPurchaserEmail As String = EmailTo

        strbodydet1 = "&nbsp;" & vbCrLf
        strbodydet1 = strbodydet1 & "<div>"
        strbodydet1 = strbodydet1 & "<p style='font-family:Calibri;font-size:18px'>Hello " & strPurchaserName & ",</p>"

        Dim dtgEmail1 As WebControls.DataGrid
        dtgEmail1 = New WebControls.DataGrid
        Dim IsPrioAvail As Boolean = False
        Dim IsNonPrioAvail As Boolean = False
        Dim DtCount As Integer
        Dim I As Integer
        Dim dtPrioOrders As New DataTable
        Dim dtNonPrio As New DataTable

        Try
            dtPrioOrders = (From C In dtEmail.AsEnumerable Where C.Field(Of String)("IsPriority") = "R" Or C.Field(Of String)("IsPriority") = "E").CopyToDataTable()
        Catch ex As Exception
        End Try

        Try
            dtNonPrio = (From C In dtEmail.AsEnumerable Where C.Field(Of String)("IsPriority") <> "R" And C.Field(Of String)("IsPriority") <> "E").CopyToDataTable()
        Catch ex As Exception
        End Try

        If ((Not IsDBNull(dtPrioOrders.Rows.Count)) And (Not dtPrioOrders.Rows.Count = 0)) And ((Not IsDBNull(dtNonPrio.Rows.Count)) And (Not dtNonPrio.Rows.Count = 0)) Then
            IsPrioAvail = True
            IsNonPrioAvail = True
            DtCount = 2
        ElseIf ((Not IsDBNull(dtPrioOrders.Rows.Count)) And (Not dtPrioOrders.Rows.Count = 0)) Then
            IsPrioAvail = True
            DtCount = 1
        ElseIf ((Not IsDBNull(dtNonPrio.Rows.Count)) And (Not dtNonPrio.Rows.Count = 0)) Then
            IsNonPrioAvail = True
            DtCount = 1
        End If

        For I = 1 To DtCount
            Dim StoreNumDT As New DataTable
            If (IsPrioAvail = True) Then
                StoreNumDT = dtPrioOrders
            ElseIf (IsNonPrioAvail = True) Then
                StoreNumDT = dtNonPrio
            End If

            If IsPrioAvail Then
                strbodydet1 = strbodydet1 & "<span style='font-family:Calibri;font-size: 21px;margin-bottom:10px;width:100%;float:left'><B>PRIORITY ORDERS</B></span>"
            ElseIf IsNonPrioAvail Then
                strbodydet1 = strbodydet1 & "<span style='font-family:Calibri;font-size: 21px;margin-bottom:10px;width:100%;float:left'><B>STANDARD ORDERS</B></span>"
            End If

            Dim dateAsString As String = DateTime.Now.ToString("MM/dd/yyyy")
            Dim IsProdDB As Boolean = False

            If Not getDBName() Then
                Mailer1.To = "webdev@sdi.com"
                Mailer1.Subject = "<<TEST SITE>>Order Status Summary - " & dateAsString & ""
                'INC0025710-Madhu-Commenting the lines
                ' Mailer1.Cc = MailCC - Commenting since this CC Needs not to be added in test site
            Else
                Mailer1.To = EmailTo
                Mailer1.Subject = "Order Status Summary - " & dateAsString & ""
                Mailer1.Cc = MailCC
                IsProdDB = True
            End If

            Dim K As Integer = 0
            Dim StoreNumArr As String() = StoreNumDT.AsEnumerable().[Select](Function(r) r.Field(Of String)("STORE")).Distinct().ToArray()
            Array.Sort(StoreNumArr)

            For Each StoreNum As String In StoreNumArr

                'SP-316 No store number for EMCOR - Dhamotharan
                Dim NewStoreNumDT As New DataTable
                Try

                    NewStoreNumDT = (From C In StoreNumDT.AsEnumerable Where C.Field(Of String)("STORE") = StoreNum).CopyToDataTable()
                    If strBU <> "I0631" Then
                        'objGenerallLogStreamWriter.WriteLine("Reading order details of Store Num:" + StoreNum & " " & Now())
                        objStreamWriter.WriteLine("Reading order details of Store Num:" + StoreNum & " " & Now())

                        strbodydet1 = strbodydet1 & "<div style='float:left;width:100%;margin-bottom:30px'>"
                        If Not (String.IsNullOrEmpty(StoreNum.Trim())) Then
                            strbodydet1 = strbodydet1 & "<p><span style='background-color:#dbf4f7;padding:10px 15px;border-radius:36px;float:Left();font-size:16.5px;margin-bottom:5px;float:left'><span style='font-weight:bold;font-family:Calibri;color: #0505af;'> Install Store:</span> <span style='font-family:Calibri;color: #0505af;'>&nbsp;" & StoreNum & "</span></span></p>"
                        End If
                    End If
                Catch ex As Exception

                End Try



                Dim WOArr As String() = NewStoreNumDT.AsEnumerable().[Select](Function(r) r.Field(Of String)("Work Order Number")).Distinct().ToArray()

                For Each WONum As String In WOArr
                    Dim WONumDetails As New DataTable
                    WONumDetails = (From C In NewStoreNumDT.AsEnumerable Where C.Field(Of String)("Work Order Number") = WONum).CopyToDataTable()
                    strbodydet1 = strbodydet1 & "<div style='float:Left()'>"
                    strbodydet1 = strbodydet1 & "<p style='width: 100%;font-size:16.5px;font-weight: bold;float:left;margin-bottom:5px'><span style='font-weight:bold; font-family:Calibri'> Work Order Num:</span> <span style='font-family:Calibri;'>&nbsp;" & WONum & "</span></p>"

                    Dim Ordernum As String() = WONumDetails.AsEnumerable().[Select](Function(r) r.Field(Of String)("Order No.")).Distinct().ToArray()

                    For Each orderno As String In Ordernum
                        objStreamWriter.WriteLine("  Reading order details of order " & orderno & " " & Now())
                        Dim OrderDetails As New DataTable

                        If connectOR.State = ConnectionState.Open Then
                            'do nothing
                        Else
                            connectOR.Open()
                        End If

                        Try
                            Dim strQuery As String = "SELECT SYSADM8.ORD_STAT_SUMMARY('" & orderno & "') from dual"
                            Dim OrderStatus As String = ORDBAccess.GetScalar(strQuery, connectOR)

                            Dim OrdStatusArr() As String
                            Dim statusImg As String
                            OrdStatusArr = OrderStatus.Split("^")
                            If OrdStatusArr(3) = 1 Then
                                statusImg = "'https://walmarttest.sdi.com/images/chain0.png'"
                            ElseIf OrdStatusArr(3) = 2 Then
                                statusImg = "'https://walmarttest.sdi.com/images/chain1.png'"
                            ElseIf OrdStatusArr(3) = 3 Then
                                statusImg = "'https://walmarttest.sdi.com/images/chain2.png'"
                            ElseIf OrdStatusArr(3) = 4 Then
                                statusImg = "'https://walmarttest.sdi.com/images/chain3.png'"
                            ElseIf OrdStatusArr(3) = 5 Then
                                statusImg = "'https://walmarttest.sdi.com/images/chain4.png'"
                            Else
                                statusImg = "'https://walmarttest.sdi.com/images/chain6.png'"
                            End If

                            Dim bgColor As String = ""
                            Dim Color As String = ""
                            Dim borderColor As String = ""

                            If OrdStatusArr(4) = "YELLOW" Then
                                bgColor = "Yellow"
                                Color = "dimgrey"
                                borderColor = "Yellow"
                            ElseIf OrdStatusArr(4) = "RED" Then
                                bgColor = "Red"
                                Color = "white"
                                borderColor = "Red"
                            ElseIf OrdStatusArr(4) = "GREEN" Then
                                bgColor = "forestgreen"
                                Color = "white"
                                borderColor = "forestgreen"
                            ElseIf OrdStatusArr(4) = "GRAY" Then
                                bgColor = "darkgray"
                                Color = "white"
                                borderColor = "darkgray"
                            End If

                            OrderDetails = (From C In WONumDetails.AsEnumerable Where C.Field(Of String)("Order No.") = orderno).CopyToDataTable()
                            strbodydet1 = strbodydet1 & "<div style='float:left;width:100%'>"

                            strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin-bottom:9px !important;'><span style='font-weight:bold;font-family:Calibri;'> Order Number:</span> <span style='font-family:Calibri;'>&nbsp;" & orderno & "</span></p> "
                            strbodydet1 = strbodydet1 & "<div style='float: left;width: 100%;padding-left: 17px;margin-bottom:9px !important;'><span><img src =" & statusImg & " alt='SDI' width='50%' height='5%' vspace='0' hspace='0' style='width:280px' /></span>&nbsp;&nbsp;&nbsp;</div><br>"
                            strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin-bottom:9px !important;margin-top:0px'><span style='font-Size:16px; background-color:" & bgColor & ";color: " & Color & ";font-family:Calibri;border-radius:50px; text-align:center;padding:2px 8px 2px 2px;'>&nbsp; " & OrdStatusArr(1) & "</span></p>"

                            Try
                                OrderDetails = OrderDetails.AsEnumerable().GroupBy(Function(row) row.Field(Of String)("Line Number")).Select(Function(group) group.First()).CopyToDataTable()
                            Catch ex As Exception

                            End Try

                            For K = 0 To OrderDetails.Rows.Count - 1

                                If K = 0 Then
                                    If strBU <> "I0631" Then
                                        strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin-bottom:9px !important;margin-top:0px'><span style='font-weight:bold;font-family:Calibri;'> Ship-to Store:</span> <span style='font-family:Calibri;'>&nbsp;" & OrderDetails.Rows(K).Item("Ship To") & "</span></p> "
                                    End If
                                    strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin-bottom:9px !important;margin-top:0px'><span style='font-weight:bold;font-family:Calibri;'> Items Ordered:</span></p> "
                                End If
                                strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin-bottom:1px;margin-top:0px'><span style='font-family:Calibri;font-weight:bold;'> &nbsp;&nbsp; Qty:</span> <span style='font-family:Calibri;'>" & OrderDetails.Rows(K).Item("Qty Ordered") & "</span><span style='font-family:Calibri;'>,&nbsp; " & OrderDetails.Rows(K).Item("Non-Stock Item Description") & "</span></p> "

                                strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin:1px !important;'><span style='font-weight:bold;font-family:Calibri;'> &nbsp;&nbsp; Tracking Number:</span> <span style='font-family:Calibri;'>&nbsp;" & OrderDetails.Rows(K).Item("Tracking No") & "</span></p> "
                                strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin:1px !important;'><span style='font-weight:bold;font-family:Calibri;'> &nbsp;&nbsp; Delivery Date:</span> <span style='font-family:Calibri;'>&nbsp;" & OrderDetails.Rows(K).Item("Delivery Date") & "</span></p> "
                                strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin:1px !important;'><span style='font-weight:bold;font-family:Calibri;'> &nbsp;&nbsp; Supplier Name:</span> <span style='font-family:Calibri;'>&nbsp;" & OrderDetails.Rows(K).Item("Supplier Name") & "</span></p> "
                                strbodydet1 = strbodydet1 & "<p style='float: left;width: 100%;padding-left: 17px;margin-top:1px !important;margin-bottom:20px'><span style='font-weight:bold;font-family:Calibri;'> &nbsp;&nbsp; Shipment Status:</span> <span style='font-family:Calibri;'>&nbsp;" & OrderDetails.Rows(K).Item("Shipment Status") & "</span></p> "

                            Next
                        Catch ex As Exception


                        End Try
                        strbodydet1 = strbodydet1 & "</div>"
                    Next

                    strbodydet1 = strbodydet1 & "</div>"

                Next

                strbodydet1 = strbodydet1 & "</div>"
            Next
            If (IsPrioAvail = True) Then
                IsPrioAvail = False
            ElseIf (IsNonPrioAvail = True) Then
                IsNonPrioAvail = False
            End If

        Next

        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "<span style='font-family:Calibri;font-size:18px'>Sincerely,</span><br>"
        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "<span style='font-family:Calibri;font-size:18px'>SDI Customer Care</span><br>"
        strbodydet1 = strbodydet1 & "&nbsp;<br>"
        strbodydet1 = strbodydet1 & "</p>"
        strbodydet1 = strbodydet1 & "</div>"
        strbodydet1 = strbodydet1 & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodydet1 = strbodydet1 & "<img src='http://www.sdiexchange.com/Images/SDIFooter_Email.png'/>" & vbCrLf

        Mailer1.Body = strbodyhead1 & strbodydet1

        Mailer1.BodyFormat = System.Web.Mail.MailFormat.Html
        Try
            ' objGenerallLogStreamWriter.WriteLine("Sending order summary email to " + Mailer1.To)
            objStreamWriter.WriteLine("Sending order summary email to " + Mailer1.To & " " & Now())
            'Mythili -- INC0023448 Adding CC emails
            SDIEmailService.EmailUtilityServices("MailandStore", Mailer1.From, Mailer1.To, Mailer1.Subject, Mailer1.Cc, "webdev@sdi.com", Mailer1.Body, "StatusChangeEmail1", MailAttachmentName, MailAttachmentbytes.ToArray())
        Catch ex As Exception
            'checkAllStatusWAL = True
            ' objGenerallLogStreamWriter.WriteLine("Error in sending order summary email to " + Mailer1.To)
            objStreamWriter.WriteLine("Error in sending order summary email to " + Mailer1.To & " " & Now())
            objStreamWriter.WriteLine("  Generated Email for the order number " & " " & Now())
        End Try
        objStreamWriter.WriteLine("  Generated Email for the order number " & " " & Now())

    End Sub
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
    Private Function GetBU() As DataSet
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            '' To get teh list of BU 
            Dim getBuQuery As String = "SELECT DISTINCT(ISA_BUSINESS_UNIT) AS BUSINESS_UNIT from PS_ISA_ENTERPRISE"

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
    'INC0025710-Madhu-Added the condition in query to only brig the current linestatus
    Private Function checkAllStatusWAL(ByVal strBU As String) As Boolean
        Dim strSQLstring As String
        Dim ds As New DataSet

        ' stock items will get item id from the ps_isa_ord_intfc_l table  but description from the PS_MASTER_ITEM_TB
        ' non-stock items  has no item-id num and gets description from the ps_isa_ord_intfc_l
        ' PS_ISAORDSTATUSLOG the line number points to the line number in ps_isa_ord_intfc_l
        ' DO NOT SELECT G.ISA_ORDER_STATUS = '6'  WE ARE GETTING IT UP TOP.
        '         '  

        '4/26/2022 Walmart order's status change emails alone won't be sent based on selected email privileges in user profile, removed that condition from below query - Poornima S

        strSQLstring = "( SELECT distinct G.BUSINESS_UNIT_OM, G.BUSINESS_UNIT_OM AS G_BUS_UNIT, D.BUSINESS_UNIT, D.ISA_EMPLOYEE_ID, A.ORDER_NO,B.ISA_WORK_ORDER_NO As WORK_ORDER_NO, B.ISA_INTFC_LN AS line_nbr," & vbCrLf &
                 " B.ISA_EMPLOYEE_ID AS EMPLID, B.ISA_LINE_STATUS as ORDER_TYPE,B.OPRID_ENTERED_BY, B.SHIPTO_ID as SHIPTO,B.ISA_USER2 as STORE," & vbCrLf &
                 " TO_CHAR(G.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM') as DTTM_STAMP, B.ISA_PRIORITY_FLAG As IsPriority, B.ISA_REQUIRED_BY_DT,B.VENDOR_ID," & vbCrLf &
                 "  G.ISA_LINE_STATUS AS ISA_ORDER_STATUS," & vbCrLf


        strSQLstring += " (SELECT E.XLATLONGNAME" & vbCrLf &
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
                 " B.QTY_REQUESTED,B.QTY_RECEIVED,B.UNIT_OF_MEASURE," & vbCrLf &
        " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH" & vbCrLf &
                 " ,A.origin, LD.PO_ID, (select Tracking_Number(B.ORDER_NO,B.ISA_INTFC_LN,B.BUSINESS_UNIT_OM) from dual) as ISA_ASN_TRACK_NO," & vbCrLf &
                 "(SELECT MAX(FDX.DESCR80) FROM PS_ISA_FEDEX_STG FDX WHERE LD.PO_ID=FDX.PO_ID AND LD.REQ_ID=B.ORDER_NO AND FDX.PROCESS_STATUS='C') AS SHIP_STATUS" & vbCrLf & '[1/25/2023]WAL-781 Added shipment status fetching query with order summary query
                 " FROM ps_isa_ord_intf_HD A," & vbCrLf  '   & _
        '[11-28-2022]WW-554 Changed the query to get tracking num from Tracking_Number function since the inventory orders are not available in PS_ISA_ASN_SHIPPED table -- Poornima S
        strSQLstring += " ps_isa_ord_intf_LN B," & vbCrLf &
                 " PS_MASTER_ITEM_TBL C," & vbCrLf &
                 " PS_ISA_USERS_TBL D," & vbCrLf &
                 " PS_ISAORDSTATUSLOG G, PS_PO_LINE_DISTRIB LD" & vbCrLf &
                 " where G.BUSINESS_UNIT_OM = '" & strBU & "' " & vbCrLf &
                 " AND G.BUSINESS_UNIT_OM = A.BUSINESS_UNIT_OM " & vbCrLf &
                 "  AND G.ISA_LINE_STATUS = B.ISA_LINE_STATUS " & vbCrLf &
                 " AND G.BUSINESS_UNIT_OM = D.BUSINESS_UNIT " & vbCrLf     '   & _

        strSQLstring += "  and A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf &
                 " and A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                 " and C.SETID (+) = 'MAIN1'" & vbCrLf &
                 " and C.INV_ITEM_ID(+) = B.INV_ITEM_ID " & vbCrLf &
                 " AND G.ORDER_NO = A.ORDER_NO " & vbCrLf &
                 " AND B.ISA_INTFC_LN = G.ISA_INTFC_LN" & vbCrLf &
                 " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf &
                 " And LD.Req_id (+) = B.order_no AND LD.REQ_LINE_NBR (+) = B.ISA_INTFC_LN" & vbCrLf

        strSQLstring += " AND G.DTTM_STAMP > (TRUNC(sysdate -1) + '" & SumryMailTime & "'/24)" & vbCrLf &
             " AND G.DTTM_STAMP <= (TRUNC(sysdate) + '" & SumryMailTime & "'/24)" & vbCrLf

        strSQLstring += "AND EXISTS (SELECT 'X' FROM PS_ISA_WO_STATUS I " & vbCrLf &
                  "WHERE B.BUSINESS_UNIT_OM = I.BUSINESS_UNIT_OM " & vbCrLf &
                  "AND   B.ISA_WORK_ORDER_NO = I.ISA_WORK_ORDER_NO " & vbCrLf &
                  "AND   I.ISA_WO_STATUS <> 'COMPLETED')" & vbCrLf

        strSQLstring += " AND B.ISA_LINE_STATUS IN ('CRE','QTW','QTC','QTS','CST','VND','APR','QTA','RCF','RCP','DLF','PKA','ASN')" & vbCrLf 'WW-644_PC_SUMMARYMAIL_02 (it should send mail for this status so changed  'G' to 'B'-> from PS_ISA_ORD_INTF_LN table ) (3/3/2023)-Aparna

        strSQLstring += " AND UPPER(B.ISA_EMPLOYEE_ID) = UPPER(D.ISA_EMPLOYEE_ID)" & vbCrLf


        'WW-644_PC_SUMMARYMAIL_01 (to get the mail daily for 'ready for pickup' orders until the status get changed) (3/3/2023)-Aparna
        'reusing the query to union the table which will get the 'PUR' status without date limit
        strSQLstring += " UNION SELECT distinct G.BUSINESS_UNIT_OM, G.BUSINESS_UNIT_OM AS G_BUS_UNIT, D.BUSINESS_UNIT, D.ISA_EMPLOYEE_ID, A.ORDER_NO,B.ISA_WORK_ORDER_NO As WORK_ORDER_NO, B.ISA_INTFC_LN AS line_nbr," & vbCrLf &
                 " B.ISA_EMPLOYEE_ID AS EMPLID, B.ISA_LINE_STATUS as ORDER_TYPE,B.OPRID_ENTERED_BY, B.SHIPTO_ID as SHIPTO,B.ISA_USER2 as STORE," & vbCrLf &
                 " TO_CHAR(G.DTTM_STAMP, 'MM/DD/YYYY HH:MI:SS AM') as DTTM_STAMP, B.ISA_PRIORITY_FLAG As IsPriority, B.ISA_REQUIRED_BY_DT,B.VENDOR_ID," & vbCrLf &
                 "  G.ISA_LINE_STATUS AS ISA_ORDER_STATUS," & vbCrLf


        strSQLstring += " (SELECT E.XLATLONGNAME" & vbCrLf &
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
                 " B.QTY_REQUESTED,B.QTY_RECEIVED,B.UNIT_OF_MEASURE," & vbCrLf &
        " D.FIRST_NAME_SRCH, D.LAST_NAME_SRCH" & vbCrLf &
                 " ,A.origin, LD.PO_ID, (select Tracking_Number(B.ORDER_NO,B.ISA_INTFC_LN,B.BUSINESS_UNIT_OM) from dual) as ISA_ASN_TRACK_NO," & vbCrLf &
                 "(SELECT MAX(FDX.DESCR80) FROM PS_ISA_FEDEX_STG FDX WHERE LD.PO_ID=FDX.PO_ID AND LD.REQ_ID=B.ORDER_NO AND FDX.PROCESS_STATUS='C') AS SHIP_STATUS" & vbCrLf & '[1/25/2023]WAL-781 Added shipment status fetching query with order summary query
                 " FROM ps_isa_ord_intf_HD A," & vbCrLf  '   & _
        strSQLstring += " ps_isa_ord_intf_LN B," & vbCrLf &
                 " PS_MASTER_ITEM_TBL C," & vbCrLf &
                 " PS_ISA_USERS_TBL D," & vbCrLf &
                 " PS_ISAORDSTATUSLOG G, PS_PO_LINE_DISTRIB LD" & vbCrLf &
                 " where G.BUSINESS_UNIT_OM = '" & strBU & "' " & vbCrLf &
                 " AND G.BUSINESS_UNIT_OM = A.BUSINESS_UNIT_OM " & vbCrLf &
                 "  AND G.ISA_LINE_STATUS = B.ISA_LINE_STATUS " & vbCrLf &
                 " AND G.BUSINESS_UNIT_OM = D.BUSINESS_UNIT " & vbCrLf     '   & _

        strSQLstring += "  and A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf &
                 " and A.ORDER_NO = B.ORDER_NO" & vbCrLf &
                 " and C.SETID (+) = 'MAIN1'" & vbCrLf &
                 " and C.INV_ITEM_ID(+) = B.INV_ITEM_ID " & vbCrLf &
                 " AND G.ORDER_NO = A.ORDER_NO " & vbCrLf &
                 " AND B.ISA_INTFC_LN = G.ISA_INTFC_LN" & vbCrLf &
                 " AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT" & vbCrLf &
                 " And LD.Req_id (+) = B.order_no AND LD.REQ_LINE_NBR (+) = B.ISA_INTFC_LN" & vbCrLf
        ''reusing the query to union the table which will get the 'PUR' status without date limit
        'strSQLstring += " AND G.DTTM_STAMP > (TRUNC(sysdate -1) + '" & SumryMailTime & "'/24)" & vbCrLf &
        '     " AND G.DTTM_STAMP <= (TRUNC(sysdate) + '" & SumryMailTime & "'/24)" & vbCrLf

        strSQLstring += " AND EXISTS (SELECT 'X' FROM PS_ISA_WO_STATUS I " & vbCrLf &
                  "WHERE B.BUSINESS_UNIT_OM = I.BUSINESS_UNIT_OM " & vbCrLf &
                  "AND   B.ISA_WORK_ORDER_NO = I.ISA_WORK_ORDER_NO " & vbCrLf &
                  "AND   I.ISA_WO_STATUS <> 'COMPLETED')" & vbCrLf

        strSQLstring += " AND ( ((B.ISA_LINE_STATUS IN ('ASN','DSP')) AND (B.USER1='P' OR B.USER_CHAR2='PU') OR B.ISA_LINE_STATUS IN ('PUR','RPU')))" & vbCrLf 'to get the 'PUR' orders

        strSQLstring += " AND UPPER(B.ISA_EMPLOYEE_ID) = UPPER(D.ISA_EMPLOYEE_ID) )" & vbCrLf &
                  " ORDER BY ORDER_NO, LINE_NBR, DTTM_STAMP" & vbCrLf
        ' this is set up in the user priveleges when giving out the status code priveleges in ISOL under Add/Change User
        ' matches the orserstatus emails set up for with the order status in PS_ISAORDSTATUSLOG
        ' the tenth byte of isa_iol_op_name has the one character g.isa_order_status code
        ' example: substr(emlsubmit1,10) = '1'   order status code 1
        ' We are going to check for priveleges in the upd_email_out program that sends the emails out.

        Try
            objStreamWriter.WriteLine("  checkAllStatusWAL (2) Q1: " & strSQLstring)
            Try
                Dim st As New Stopwatch()
                st.Start()
                ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)
                st.Stop()
                Dim ts As TimeSpan = st.Elapsed
                Dim elapsedTime As String = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10)
                objStreamWriter.WriteLine("Query Execution Time " + elapsedTime)
            Catch ex As Exception
                ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)
                objStreamWriter.WriteLine("Query Execution Time " + Now())
                checkAllStatusWAL = True

            End Try

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            objStreamWriter.WriteLine("     Error - error reading the order details" & " " & Now())
            Return True
            checkAllStatusWAL = True
        End Try

        If Not ds Is Nothing Then
            If ds.Tables.Count > 0 Then
                If IsDBNull(ds.Tables(0).Rows.Count) Or (ds.Tables(0).Rows.Count) = 0 Then
                    Console.WriteLine("Fetched Datas 0")
                    objStreamWriter.WriteLine("     Warning - no status changes to process at this time for All Statuses" & " " & Now())
                    Try
                        connectOR.Close()
                    Catch ex As Exception
                        objStreamWriter.WriteLine("Warning Error in fetching data- " & " " & Now())

                    End Try
                    Return False
                Else
                    Console.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()))
                    objStreamWriter.WriteLine("Fetched Datas " + Convert.ToString(ds.Tables(0).Rows.Count()) & " " & Now())
                End If
            Else
                Console.WriteLine("Fetched Datas 0")
                objStreamWriter.WriteLine(" Tables does not exist")
                Try
                    connectOR.Close()
                Catch ex As Exception

                End Try
                Return False
            End If
        Else
            Console.WriteLine("Fetched Datas 0")
            objStreamWriter.WriteLine("    Dataset is nothing")
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            Return False
        End If

        'Dim rowsaffected As Integer
        'Dim tmpOrderNo As String

        If connectOR.State = ConnectionState.Open Then
            'do nothing
        Else
            connectOR.Open()
        End If
        'Dim strPreOrderno As String
        Dim I As Integer
        'Dim X As Integer
        Dim dsEmail As New DataTable
        Dim dr1 As DataRow
        Dim dsShipTo As DataSet

        'SDI - 23457 added qty ordered, qty received and UOM column for order status email
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
        dsEmail.Columns.Add("Qty Ordered")
        dsEmail.Columns.Add("Qty Received")
        dsEmail.Columns.Add("UOM")
        dsEmail.Columns.Add("STORE")
        dsEmail.Columns.Add("First Name")
        dsEmail.Columns.Add("Last Name")
        dsEmail.Columns.Add("IsPriority")
        dsEmail.Columns.Add("Ship To")
        dsEmail.Columns.Add("Delivery Date")
        dsEmail.Columns.Add("Supplier Name")
        dsEmail.Columns.Add("Shipment Status")
        Try
            strSQLstring = "Select DESCR, SHIPTO_ID FROM PS_SHIPTO_TBL"
            dsShipTo = ORDBAccess.GetAdapter(strSQLstring, connectOR)
        Catch
        End Try
        Dim strdescription As String = " "
        Dim strEmailTo As String = " "
        Dim strEmpID As String = ""
        Dim lstOfString As List(Of String) = New List(Of String)

        Dim EmpIDArr As String() = ds.Tables(0).AsEnumerable().[Select](Function(r) r.Field(Of String)("ISA_EMPLOYEE_ID")).Distinct().ToArray()

        For Each EmpID As String In EmpIDArr
            Try
                Dim OrderDetailDT As New DataTable

                OrderDetailDT = (From C In ds.Tables(0).AsEnumerable Where C.Field(Of String)("ISA_EMPLOYEE_ID") = EmpID).CopyToDataTable()
                'objGenerallLogStreamWriter.WriteLine("Reading order details of Employee: " + EmpID)
                objStreamWriter.WriteLine("Reading order details of Employee:" + EmpID & " " & Now())

                For I = 0 To OrderDetailDT.Rows.Count - 1
                    Dim strStatus_code As String = " "
                    Try
                        strStatus_code = OrderDetailDT.Rows(I).Item("STATUS_CODE")
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

                    objStreamWriter.WriteLine("  CheckAllStatusWAL (3): " & strSQLstring & " " & Now())

                    Command = New OleDbCommand(strSQLstring, connectOR)
                    connectOR.Open()
                    Try
                        strSiteBU = Command.ExecuteScalar
                        connectOR.Close()
                    Catch ex As Exception
                        objStreamWriter.WriteLine("  StatChg Email NSTK send select siteBU for " & strBU & " " & Now())
                        connectOR.Close()
                        strSiteBU = "ISA00"
                    End Try

                    dr1 = dsEmail.NewRow()
                    objStreamWriter.WriteLine("Setting details of order " + OrderDetailDT.Rows(I).Item("ORDER_NO") + "to email datatset" & " " & Now())
                    Dim Dtformat As String = "MM/dd/yyyy"
                    Dim stroderno As String = OrderDetailDT.Rows(I).Item("ORDER_NO")
                    Dim strlineno As String = OrderDetailDT.Rows(I).Item("LINE_NBR")
                    dr1.Item(0) = OrderDetailDT.Rows(I).Item("ORDER_NO")
                    dr1.Item(1) = OrderDetailDT.Rows(I).Item("ORDER_STATUS_DESC")
                    dr1.Item(2) = OrderDetailDT.Rows(I).Item("NONSTOCK_DESCRIPTION")
                    dr1.Item(3) = OrderDetailDT.Rows(I).Item("STOCK_DESCRIPTION")
                    dr1.Item(4) = OrderDetailDT.Rows(I).Item("INV_ITEM_ID")
                    dr1.Item(5) = OrderDetailDT.Rows(I).Item("LINE_NBR")
                    Dim ln_notes As String = ""
                    ln_notes = GetLineNotes(stroderno, strBU, strlineno)
                    dr1.Item(10) = ln_notes
                    connectOR.Open()
                    dr1.Item(6) = OrderDetailDT.Rows(I).Item("DTTM_STAMP")
                    If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
                        connectOR.Close()
                    End If

                    'just get the last character
                    dr1.Item(7) = strStatus_code
                    dr1.Item(8) = OrderDetailDT.Rows(I).Item("WORK_ORDER_NO")
                    dr1.Item(9) = OrderDetailDT.Rows(I).Item("PO_ID")
                    Dim trackingNo As String = ""
                    Try
                        trackingNo = OrderDetailDT.Rows(I).Item("ISA_ASN_TRACK_NO")
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
                        dr1.Item(11) = "-"
                    End If
                    Try
                        dr1.Item(12) = OrderDetailDT.Rows(I).Item("QTY_REQUESTED")
                    Catch ex As Exception
                        dr1.Item(12) = ""
                    End Try

                    Try
                        dr1.Item(13) = OrderDetailDT.Rows(I).Item("QTY_RECEIVED")
                    Catch ex As Exception
                        dr1.Item(13) = ""
                    End Try
                    Try
                        dr1.Item(14) = OrderDetailDT.Rows(I).Item("UNIT_OF_MEASURE")
                    Catch ex As Exception
                        dr1.Item(14) = ""
                    End Try

                    Try
                        dr1.Item(15) = OrderDetailDT.Rows(I).Item("STORE")
                    Catch ex As Exception
                        dr1.Item(15) = ""
                    End Try

                    Try
                        dr1.Item(16) = OrderDetailDT.Rows(I).Item("FIRST_NAME_SRCH")
                    Catch ex As Exception
                        dr1.Item(16) = ""
                    End Try

                    Try
                        dr1.Item(17) = OrderDetailDT.Rows(I).Item("LAST_NAME_SRCH")
                    Catch ex As Exception
                        dr1.Item(17) = ""
                    End Try

                    Try
                        dr1.Item(18) = OrderDetailDT.Rows(I).Item("IsPriority")
                    Catch ex As Exception
                        dr1.Item(18) = ""
                    End Try


                    If OrderDetailDT.Rows(I).Item("SHIPTO").ToString <> "" Then
                        Try
                            Dim Descr As String = dsShipTo.Tables(0).AsEnumerable().
     Where(Function(r) Convert.ToString(r.Field(Of String)("SHIPTO_ID")) = OrderDetailDT.Rows(I).Item("SHIPTO").ToString).
     Select(Function(r) Convert.ToString(r.Field(Of String)("DESCR"))).FirstOrDefault()
                            dr1.Item(19) = Descr + "_" + OrderDetailDT.Rows(I).Item("SHIPTO").ToString
                        Catch
                            dr1.Item(19) = ""
                        End Try

                    End If

                    Try
                        dr1.Item(20) = Convert.ToDateTime(OrderDetailDT.Rows(I).Item("ISA_REQUIRED_BY_DT")).ToString(Dtformat)
                    Catch ex As Exception
                        dr1.Item(20) = "-"
                    End Try

                    Dim vendor As String = OrderDetailDT.Rows(I).Item("VENDOR_ID")
                    Dim Cmd As OleDbCommand
                    Dim vendorName As String

                    strSQLstring = "Select NAME1 from PS_Vendor where VENDOR_ID= '" & vendor & "'"

                    objStreamWriter.WriteLine("  CheckAllStatusWAL (4): " & strSQLstring & " " & Now())


                    Cmd = New OleDbCommand(strSQLstring, connectOR)
                    connectOR.Open()
                    Try
                        vendorName = Cmd.ExecuteScalar
                        dr1.Item(21) = OrderDetailDT.Rows(I).Item("VENDOR_ID") + "- " + vendorName
                        connectOR.Close()
                    Catch ex As Exception
                        objStreamWriter.WriteLine("  Exception in fetching vendor ID " & " " & Now())
                        connectOR.Close()
                        dr1.Item(21) = ""
                    End Try

                    Try
                        If OrderDetailDT.Rows(I).Item("SHIP_STATUS").Trim() <> "" Then
                            dr1.Item(22) = OrderDetailDT.Rows(I).Item("SHIP_STATUS")
                        Else
                            dr1.Item(22) = "-"
                        End If
                    Catch ex As Exception
                        dr1.Item(22) = "-"
                    End Try

                    dsEmail.Rows.Add(dr1)
                    ' "R" nonstock
                    ' "7" stock

                    strEmailTo = OrderDetailDT.Rows(I).Item("ISA_EMPLOYEE_EMAIL")
                    strEmpID = OrderDetailDT.Rows(I).Item("ISA_EMPLOYEE_ID")

                Next

                sendCustEmailWal(dsEmail, strEmpID, strEmailTo, strBU)

                dsEmail.Clear()
            Catch ex As Exception

            End Try
        Next


        If Not connectOR Is Nothing AndAlso ((connectOR.State And ConnectionState.Open) = ConnectionState.Open) Then
            connectOR.Close()
        End If

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




End Module
