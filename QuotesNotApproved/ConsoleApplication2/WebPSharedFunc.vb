Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports ConsoleApplication2.ORDBData
Imports System.Drawing.Color
Imports System.IO
Imports System.Security.Cryptography
Imports System.Web.Mail
Imports System.Net
Imports System.Text
Imports System.Web.HttpBrowserCapabilities
Imports System.Threading.Thread
Imports ConsoleApplication2.GeneralFunctions.GenFunctions
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlTextWriter



Namespace WebPartnerFunctions

    Public Class WebPSharedFunc

        Public Shared Sub buildNotifyApprover(ByVal strreqID As String, ByVal strAgent As String, ByVal strBU As String, ByVal strAppUserid As String, ByVal strHldStatus As String)
            'this is where we will put in the description of the order per S.Roudriquez
            'pfd
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim strSQLString As String
            Dim strMessage As New Alert
            Dim strappName As String
            Dim strappEmail As String
            Dim strBuyerName As String

            strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                    " LAST_NAME_SRCH," & vbCrLf & _
                    " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                    " FROM PS_ISA_USERS_TBL" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND ISA_EMPLOYEE_ID = '" & strAppUserid & "'"

            Dim dtrAppReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

            If dtrAppReader.HasRows() = True Then
                dtrAppReader.Read()
                strappName = dtrAppReader.Item("FIRST_NAME_SRCH") & " " & dtrAppReader.Item("LAST_NAME_SRCH")
                strappEmail = dtrAppReader.Item("ISA_EMPLOYEE_EMAIL")
                dtrAppReader.Close()
            Else
                dtrAppReader.Close()
                Exit Sub
            End If

            strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                    " LAST_NAME_SRCH," & vbCrLf & _
                    " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                    " FROM PS_ISA_USERS_TBL" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND ISA_EMPLOYEE_ID = '" & strAgent & "'"

            Dim dtrBuyerReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

            If dtrBuyerReader.HasRows() = True Then
                dtrBuyerReader.Read()
                strBuyerName = dtrBuyerReader.Item("FIRST_NAME_SRCH") & " " & dtrBuyerReader.Item("LAST_NAME_SRCH")
                dtrBuyerReader.Close()
            Else
                dtrBuyerReader.Close()
                'Exit Sub
            End If

            If currentApp.Session("IOLServer") = "" Then
                setServer()
            End If

            'steph's request
            'pfd
            '1/02/2009
            ' get the detail line for the the approver quote email
            strSQLString = "SELECT A.ISA_IDENTIFIER," & vbCrLf & _
                    " A.ORDER_NO, A.OPRID_ENTERED_BY," & vbCrLf & _
                    " TO_CHAR(B.ISA_REQUIRED_BY_DT,'YYYY-MM-DD') as REQ_DT," & vbCrLf & _
                    " TO_CHAR(A.ADD_DTTM,'YYYY-MM-DD') as ADD_DT," & vbCrLf & _
                    " B.EMPLID, B.ISA_CUST_CHARGE_CD," & vbCrLf & _
                    " B.ISA_WORK_ORDER_NO, B.ISA_MACHINE_NO," & vbCrLf & _
                    " B.ISA_CUST_NOTES," & vbCrLf & _
                    " B.INV_ITEM_ID, B.DESCR254, B.MFG_ID," & vbCrLf & _
                    " B.ISA_MFG_FREEFORM, B.MFG_ITM_ID," & vbCrLf & _
                    " B.UNIT_OF_MEASURE," & vbCrLf & _
                    " B.VNDR_CATALOG_ID, B.NET_UNIT_PRICE," & vbCrLf & _
                    " (B.ISA_PARENT_IDENT || B.LINE_NBR) as UNIQUEIDENT, B.LINE_NBR," & vbCrLf & _
                    " B.ISA_ORDER_STATUS," & vbCrLf & _
                    " C.QTY_REQ, C.PRICE_REQ, C.ISA_SELL_PRICE," & vbCrLf & _
                    " C.INV_ITEM_TYPE, C.INV_STOCK_TYPE," & vbCrLf & _
                    " TO_CHAR(D.DUE_DT,'YYYY-MM-DD') as DUE_DT" & vbCrLf & _
                    " FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B," & vbCrLf & _
                    " PS_REQ_LINE C, PS_REQ_LINE_SHIP D" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                    " AND A.ORDER_NO = '" & strreqID & "'" & vbCrLf & _
                    " AND A.OPRID_ENTERED_BY = '" & strAgent & "'" & vbCrLf & _
                    " AND A.ORDER_STATUS = 'Q'" & vbCrLf & _
                    " AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                    " AND B.QTY_REQ > 0" & vbCrLf & _
                    " AND A.ORDER_NO = C.REQ_ID" & vbCrLf & _
                    " AND B.LINE_NBR = C.LINE_NBR" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = '" & currentApp.Session("SITEBU") & "'" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
                    " AND C.REQ_ID = D.REQ_ID" & vbCrLf & _
                    " AND C.LINE_NBR = D.LINE_NBR"

            Dim dsOrder As DataSet = ORDBData.GetAdapter(strSQLstring)




            Dim strbodyhead As String
            Dim strbodydetl As String
            Dim txtBody As String
            Dim txtHdr As String
            Dim txtMsg As String

            Dim streBU As String = encryptQueryString(strBU)
            Dim streOrdnum As String = encryptQueryString(strreqID)
            Dim streApper As String = encryptQueryString(strAppUserid)
            Dim streAppTyp As String = encryptQueryString(strHldStatus)
            Dim strhref As String
            Dim stritemid As String

            strhref = currentApp.Session("IOLServer") & "approveorder.aspx?fer=" & streOrdnum & "&op=" & streApper & "&xyz=" & streBU & "&pyt=" & streAppTyp & "&HOME=N"

            Dim Mailer As MailMessage = New MailMessage
            Mailer.From = "Insiteonline@SDI.com"
            Mailer.Cc = ""
            Mailer.Bcc = ""
            strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
            If strHldStatus = "B" Then
                strbodyhead = strbodyhead & "<center><span >In-Site&reg; Online - Request for Budget Approval</span></center>"
            Else
                strbodyhead = strbodyhead & "<center><span >In-Site&reg; Online - Request for Approval</span></center>"
            End If
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

            strbodydetl = "&nbsp;" & vbCrLf
            strbodydetl = strbodydetl & "<div>"
            strbodydetl = strbodydetl & "<p >TO:<span>      </span>" & strappName & "<br>"
            strbodydetl = strbodydetl & "&nbsp;<BR>"
            strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "Order:<span>  </span>" & strreqID & "<br>"
            strbodydetl = strbodydetl & "Item Number:<span>  </span>" & stritemid & "<br>"
            strbodydetl = strbodydetl & "&nbsp;</p>"
            strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The above referenced order has been "
            strbodydetl = strbodydetl & "requested by <b>" & strBuyerName & "</b> "
            If strHldStatus = "B" Then
                strbodydetl = strbodydetl & "and has exceeded the charge code budget limit.  Click the link below or select the ""Approve Budget"" "
            Else
                strbodydetl = strbodydetl & "and needs your approval.  Click the link below or select the ""Approve Orders"" "
            End If
            strbodydetl = strbodydetl & "menu option in In-Site&reg; Online to approve or reject the order.<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "Sincerely,<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "SDI Customer Care<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "Click this <a href='" & strhref & "'>link </a>"
            strbodydetl = strbodydetl & "to APPROVE or REJECT order. </p>"
            strbodydetl = strbodydetl & "</div>"

            Mailer.Body = strbodyhead & strbodydetl
            If DbUrl.Substring(DbUrl.Length - 4).ToUpper = "PLGR" Or _
                DbUrl.Substring(DbUrl.Length - 4).ToUpper = "RPTG" Then
                Mailer.To = "DoNotSendPLGR@sdi.com"
            Else
                Mailer.To = strappEmail
            End If

            If strHldStatus = "B" Then
                Mailer.Subject = "In-Site® Online - Order Number " & strreqID & " needs budget approval"
            Else
                Mailer.Subject = "In-Site® Online - Order Number " & strreqID & " needs approval"
            End If

            Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
            'SmtpMail.Send(Mailer)
            sendemail(Mailer)

            dtrAppReader.Close()
        End Sub

        Public Shared Sub buildNotifyBuyer(ByVal strreqID As String, _
                                            ByVal strAgent As String, _
                                            ByVal strBU As String, _
                                            ByVal strAppUserid As String, _
                                            ByVal strAction As String, _
                                            ByVal strType As String, _
                                            ByVal strMoreAppr As String)
            'strreid = order no
            'strAgent = if quote then SDI buyer else customer buying items
            'strBU = customer BU
            'strAppUserid = person that is approving
            'strAction = either "APPROVE" or "DECLINE"
            'strType = either "quote" or "order"
            Dim strSQLString As String
            Dim strMessage As New Alert
            Dim strBuyerName As String
            Dim strBuyerEmail As String
            Dim strApproverName As String
            Dim strSDIBuyer As String

            'I know the below code looks like a duplication 
            'but I needed to get the SDI buyers email for approvals notifications
            'and I didn't want to break what was already working
            'Bob D - 01/20/2005
            ' Dim objEnterprise As New clsEnterprise(strBU)
            'strSDIBuyer = objEnterprise.NONSKREQEmail

            'If strType = "quote" Then
            '    strSQLString = "SELECT A.ISA_NONSKREQ_EMAIL" & vbCrLf & _
            '       " FROM PS_ISA_ENTERPRISE A" & vbCrLf & _
            '       " WHERE A.ISA_BUSINESS_UNIT = '" & strBU & "'"
            'Else
            '    strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
            '       " LAST_NAME_SRCH," & vbCrLf & _
            '       " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
            '       " FROM PS_ISA_USERS_TBL" & vbCrLf & _
            '       " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
            '       " AND ISA_EMPLOYEE_ID = '" & strAgent & "'"
            'End If

            Dim dtrBuyReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

            ' if no record found for buyer then exit
            If dtrBuyReader.HasRows() = True Then
                dtrBuyReader.Read()
            Else
                dtrBuyReader.Close()
                Exit Sub
            End If
            If strType = "quote" Then
                strBuyerName = dtrBuyReader.Item("ISA_NONSKREQ_EMAIL")
                strBuyerEmail = dtrBuyReader.Item("ISA_NONSKREQ_EMAIL")
            Else
                strBuyerName = dtrBuyReader.Item("FIRST_NAME_SRCH") & _
                " " & dtrBuyReader.Item("LAST_NAME_SRCH")
                strBuyerEmail = dtrBuyReader.Item("ISA_EMPLOYEE_EMAIL")

            End If
            dtrBuyReader.Close()

            strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                    " LAST_NAME_SRCH," & vbCrLf & _
                    " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                    " FROM PS_ISA_USERS_TBL" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND ISA_EMPLOYEE_ID = '" & strAppUserid & "'"


            Dim dtrAppReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

            If dtrAppReader.HasRows() = True Then
                dtrAppReader.Read()
                strApproverName = dtrAppReader.Item("FIRST_NAME_SRCH") & _
                      " " & dtrAppReader.Item("LAST_NAME_SRCH")
            Else
                strApproverName = strAppUserid
            End If

            dtrAppReader.Close()

            Dim strbodyhead As String
            Dim strbodydetl As String
            Dim txtBody As String
            Dim txtHdr As String
            Dim txtMsg As String

            Dim Mailer As MailMessage = New MailMessage
            Mailer.From = "Insiteonline@SDI.com"
            Mailer.cc = ""
            Mailer.Bcc = ""
            strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
            strbodyhead = strbodyhead & "<center><span >In-Site&reg; Online - Request for Approval</span></center>"
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

            strbodydetl = "&nbsp;" & vbCrLf
            strbodydetl = strbodydetl & "<div>"
            strbodydetl = strbodydetl & "<p >TO:<span>      </span>" & strBuyerName & "<br>"
            strbodydetl = strbodydetl & "&nbsp;<BR>"
            strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "Order:<span>  </span>" & strreqID & "<br"
            strbodydetl = strbodydetl & "&nbsp;</p>"
            strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The above referenced " & strType & " has been "
            strbodydetl = strbodydetl & strAction & " by <b>" & strApproverName & "</b>.  "
            If strMoreAppr.ToUpper = "FALSE" Then
                strbodydetl = strbodydetl & "<br>"
            Else
                strbodydetl = strbodydetl & "Additional approvals are still required before order can be processed.<br>"
            End If
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "Sincerely,<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "SDI Customer Care<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "</p>"
            strbodydetl = strbodydetl & "</div>"

            Mailer.body = strbodyhead & strbodydetl
            If DbUrl.Substring(DbUrl.Length - 4).ToUpper = "PLGR" Or _
                DbUrl.Substring(DbUrl.Length - 4).ToUpper = "RPTG" Then
                Mailer.to = "DoNotSendPLGR@sdi.com"
            Else
                Mailer.To = strBuyerEmail
                If strMoreAppr.ToUpper = "FALSE" And _
                    Not strType = "quote" Then
                    Mailer.To = strBuyerEmail & "; " & strSDIBuyer
                End If
            End If

            Mailer.Subject = "In-Site® Online - Order Number " & strreqID & " has been " & strAction
            Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
            'SmtpMail.Send(Mailer)
            sendemail(Mailer)

            dtrAppReader.Close()

        End Sub

        Public Shared Function buildNotifyReceiver(ByVal strBU As String, _
                                            ByVal strReceiver As String, _
                                            ByVal strPO As String, _
                                            ByVal dtgPO As DataGrid) As String
            ' initialize to an empty string since, there were "exit function" calls
            '   - erwin 2009.08.10
            buildNotifyReceiver = ""

            Dim strSQLString As String
            Dim strMessage As New Alert
            Dim strPurchaserName As String
            Dim strPurchaserEmail As String
            Dim strReceiverName As String
            Dim arrEmployeeIDs As ArrayList
            arrEmployeeIDs = New ArrayList

            Dim I As Integer
            Dim X As Integer

            For I = 0 To dtgPO.Items.Count - 1
                If Not Trim(dtgPO.Items(I).Cells(33).Text) = "" Then
                    If Not CType(dtgPO.Items(I).Cells(9).FindControl("txtQTY"), TextBox) Is Nothing Then
                        Dim txtQuantity As TextBox = CType(dtgPO.Items(I).Cells(9).FindControl("txtQTY"), TextBox)
                        If IsNumeric(txtQuantity.Text) Then
                            If Convert.ToInt32(txtQuantity.Text) > 0 Then
                                If arrEmployeeIDs.IndexOf(dtgPO.Items(I).Cells(33).Text) = -1 Then
                                    arrEmployeeIDs.Add((dtgPO.Items(I).Cells(33).Text))
                                End If
                            End If
                        End If
                    End If
                End If
            Next
            If arrEmployeeIDs Is Nothing Then
                Exit Function
            End If

            Dim strbodyhead As String
            Dim strbodydetl As String
            Dim txtBody As String
            Dim txtHdr As String
            Dim txtMsg As String
            Dim dataGridHTML As String = ""
            Dim SBnstk As New StringBuilder
            Dim SWnstk As New StringWriter(SBnstk)
            Dim htmlTWnstk As New System.Web.UI.HtmlTextWriter(SWnstk)
            Dim bolSelectItem As Boolean

            Dim Mailer As MailMessage = New MailMessage
            Mailer.From = "Insiteonline@SDI.com"
            Mailer.Cc = ""
            Mailer.Bcc = ""
            strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
            strbodyhead = strbodyhead & "<center><span >In-Site&reg; Online - Received Order</span></center>"
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

            dtgPO.Columns(0).Visible = False
            dtgPO.Columns(1).Visible = False
            'dtgPO.Columns(8).Visible = False
            dtgPO.Columns(11).Visible = False

            For X = 0 To arrEmployeeIDs.Count - 1

                For I = 0 To dtgPO.Items.Count - 1
                    bolSelectItem = False

                    If dtgPO.Items(I).Cells(33).Text = arrEmployeeIDs(X) Then
                        If Not dtgPO.Items(I).Cells(9).FindControl("txtQTY") Is Nothing Then
                            Dim txtQuantity As TextBox = dtgPO.Items(I).Cells(9).FindControl("txtQTY")
                            If IsNumeric(txtQuantity.Text) Then
                                If Convert.ToInt32(txtQuantity.Text) > 0 Then
                                    bolSelectItem = True
                                End If
                            End If
                        End If
                    End If
                    If bolSelectItem = False Then
                        dtgPO.Items(I).Visible = False
                    End If
                Next

                ' the 'cmpDataGridToExcel.ClearControls(dtgPO)' call f-up the grid and web page!
                '   if this goes through, fine! if not ignore!!! I don't know what this thing is trying to do!?
                '   - erwin 2009.08.19
                'Try
                '    '  cmpDataGridToExcel.ClearControls(dtgPO)
                'Catch ex As Exception
                'End Try

                Try
                    dtgPO.Columns(9).ItemStyle.ForeColor = Red
                    dtgPO.Columns(9).ItemStyle.HorizontalAlign = HorizontalAlign.Center
                    dtgPO.RenderControl(htmlTWnstk)
                Catch ex As Exception
                End Try

                Try
                    dataGridHTML = SBnstk.ToString()
                Catch ex As Exception
                    dataGridHTML = ""
                End Try

                buildNotifyReceiver = dataGridHTML

                'dtgPO.Columns(9).ItemStyle.ForeColor = Red
                'dtgPO.Columns(9).ItemStyle.HorizontalAlign = HorizontalAlign.Center
                'dtgPO.RenderControl(htmlTWnstk)
                'dataGridHTML = SBnstk.ToString()
                'buildNotifyReceiver = dataGridHTML

                strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                   " LAST_NAME_SRCH," & vbCrLf & _
                   " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                   " FROM PS_ISA_USERS_TBL" & vbCrLf & _
                   " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                   " AND ISA_EMPLOYEE_ID = '" & arrEmployeeIDs(X) & "'"

                Dim dtrBuyReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

                ' if no record found for buyer then exit
                If dtrBuyReader.HasRows() = True Then
                    dtrBuyReader.Read()
                Else
                    dtrBuyReader.Close()
                    Exit Function
                End If
                strPurchaserName = dtrBuyReader.Item("FIRST_NAME_SRCH") & _
                " " & dtrBuyReader.Item("LAST_NAME_SRCH")
                strPurchaserEmail = dtrBuyReader.Item("ISA_EMPLOYEE_EMAIL")

                dtrBuyReader.Close()

                strSQLString = "SELECT FIRST_NAME_SRCH," & vbCrLf & _
                    " LAST_NAME_SRCH," & vbCrLf & _
                    " ISA_EMPLOYEE_EMAIL" & vbCrLf & _
                    " FROM PS_ISA_USERS_TBL" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND ISA_EMPLOYEE_ID = '" & strReceiver & "'"

                Dim dtrAppReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

                If dtrAppReader.HasRows() = True Then
                    dtrAppReader.Read()
                    strReceiverName = dtrAppReader.Item("FIRST_NAME_SRCH") & _
                          " " & dtrAppReader.Item("LAST_NAME_SRCH")
                Else
                    strReceiverName = strReceiver
                End If

                dtrAppReader.Close()

                strbodydetl = "&nbsp;" & vbCrLf
                strbodydetl = strbodydetl & "<div>"
                strbodydetl = strbodydetl & "<p >TO:<span>      </span>" & strPurchaserName & "<br>"
                strbodydetl = strbodydetl & "&nbsp;<BR>"
                strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
                strbodydetl = strbodydetl & "&nbsp;<br>"
                strbodydetl = strbodydetl & "Order:<span>  </span>" & strPO & "<br"
                strbodydetl = strbodydetl & "&nbsp;</p>"
                strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The above referenced order has been "
                strbodydetl = strbodydetl & "received by <b>" & strReceiverName & "</b>.  "

                strbodydetl = strbodydetl & "<br>"
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
                If DbUrl.Substring(DbUrl.Length - 4).ToUpper = "PLGR" Or _
                    DbUrl.Substring(DbUrl.Length - 4).ToUpper = "RPTG" Then
                    Mailer.To = "DoNotSendPLGR@sdi.com"
                Else
                    Mailer.To = strPurchaserEmail

                End If

                Mailer.Subject = "In-Site® Online - Order Number " & strPO & " has been received"
                Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
                'SmtpMail.Send(Mailer)
                sendemail(Mailer)

            Next
            dtgPO.Columns(0).Visible = True
            dtgPO.Columns(1).Visible = True
            'dtgPO.Columns(8).Visible = True
            dtgPO.Columns(11).Visible = True


        End Function

        Public Shared Function checkApproval(ByVal strAgent As String, ByVal strBU As String) As DataSet

            Dim strSQLstring As String = "SELECT ISA_IOL_APR_EMP_ID," & vbCrLf & _
                    " ISA_IOL_APR_LIMIT" & vbCrLf & _
                    " FROM PS_ISA_USERS_APPRV" & vbCrLf & _
                    " WHERE ISA_EMPLOYEE_ID = '" & strAgent & "'" & vbCrLf & _
                    " AND BUSINESS_UNIT = '" & strBU & "'"

            checkApproval = ORDBData.GetAdapter(strSQLstring)

        End Function

        Public Shared Function CheckLimits(ByVal strBU As String, ByVal stragent As String, ByVal strreqID As String, ByVal strAppLevel As String, ByVal strAppType As String) As ArrayList

            Dim I As Integer
            Dim X As Integer
            Dim myConnection As OleDbConnection
            Dim myCommand As OleDbCommand
            Dim myParameter As OleDbParameter
            Dim arrParamsOut As ArrayList
            Dim arrParamsAll As ArrayList
            Dim arrAppChgCds As ArrayList
            arrParamsAll = New ArrayList
            arrAppChgCds = New ArrayList
            ' Create connection and set connection string	
            myConnection = New OleDbConnection(DbUrl)

            If strAppType = "B" Then
                arrAppChgCds = getChgCodes(strBU, strreqID)
            Else
                arrAppChgCds.Add("NotChgCode")
            End If

            X = arrAppChgCds.Count - 1

            For I = 0 To X
                arrParamsOut = New ArrayList
                ' Create a new command object
                myCommand = New OleDbCommand

                ' Set the properties of the command so that it uses
                ' our connection, knows the name of the stored proc
                ' to execute, and knows that it is going to be
                ' executing a stored proc and not something else
                myCommand.Connection = myConnection
                If strAppType = "B" Then
                    myCommand.CommandText = "SYSADM.SP_CUSTBUD_APPROVAL2"
                Else
                    myCommand.CommandText = "SYSADM.SP_CUSTEMP_APPROVAL2"
                End If
                myCommand.CommandType = CommandType.StoredProcedure

                ' Create our Business_unit input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "BU"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strBU

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Employee ID input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "EMPID"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = stragent

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Order Number input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "ORDNO"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strreqID

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Approver level type input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "APPTY"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strAppLevel

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                If strAppType = "B" Then
                    ' Create our charge code input parameter
                    myParameter = myCommand.CreateParameter()
                    myParameter.ParameterName = "CHGCD"
                    myParameter.Direction = ParameterDirection.Input
                    myParameter.OleDbType = OleDbType.VarChar
                    myParameter.Value = arrAppChgCds(I)

                    ' Add parameter to our command
                    myCommand.Parameters.Add(myParameter)
                End If

                ' Create our order limit TRUE/FALSE output parameter:

                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "APPLM"
                myParameter.Size = 5
                myParameter.Value = ""
                myParameter.Direction = ParameterDirection.Output
                myParameter.OleDbType = OleDbType.VarChar

                myCommand.Parameters.Add(myParameter)

                ' Create our approver employee ID output parameter:

                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "APPEM"
                myParameter.Size = 8
                myParameter.Value = ""
                myParameter.Direction = ParameterDirection.Output
                myParameter.OleDbType = OleDbType.VarChar

                myCommand.Parameters.Add(myParameter)

                ' Create order status output parameter:
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "APPST"
                myParameter.Size = 1
                myParameter.Value = ""
                myParameter.Direction = ParameterDirection.Output
                myParameter.OleDbType = OleDbType.VarChar

                myCommand.Parameters.Add(myParameter)

                ' Open the connection to the database
                myConnection.Open()

                ' Execute the stored procedure
                myCommand.ExecuteNonQuery()

                'arrParamsOut(0) = myCommand.Parameters("APPLM").Value
                'arrParamsOut(1) = myCommand.Parameters("APPEM").Value
                'arrParamsOut(2) = myCommand.Parameters("APPST").Value
                arrParamsOut.Add(myCommand.Parameters("APPLM").Value)
                arrParamsOut.Add(myCommand.Parameters("APPEM").Value)
                arrParamsOut.Add(myCommand.Parameters("APPST").Value)
                arrParamsOut.Add(arrAppChgCds(I))
                arrParamsAll.Add(arrParamsOut)
                myConnection.Close()

            Next
            CheckLimits = arrParamsAll

        End Function

        Public Shared Function CheckFavExist(ByVal strBU As String, ByVal strAgent As String, ByVal cpitemid As String, ByVal strMFG As String, ByVal strMFGItemid As String) As Boolean

            Dim strSQLString As String
            Dim strNonCat As String
            If cpitemid.Length > 5 Then
                strNonCat = cpitemid.Substring(0, 6)
            Else
                strNonCat = cpitemid
            End If
            If strNonCat = "NONCAT" Then
                strSQLString = "SELECT 'X'" & vbCrLf & _
                            " FROM PS_ISA_CP_FAVS" & vbCrLf & _
                            " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                            " AND EMPLID = '" & strAgent & "'" & vbCrLf & _
                            " AND ISA_MFG_FREEFORM = '" & strMFG.ToUpper & "'" & vbCrLf & _
                            " AND MFG_ITM_ID = '" & strMFGItemid.ToUpper & "'"
            Else
                strSQLString = "SELECT 'X'" & vbCrLf & _
                                " FROM PS_ISA_CP_FAVS" & vbCrLf & _
                                " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                                " AND EMPLID = '" & strAgent & "'" & vbCrLf & _
                                " AND ISA_CP_ITEM_ID = '" & cpitemid & "'"
            End If

            Dim dtrLoginReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
            If dtrLoginReader.Read() Then
                CheckFavExist = True
            Else
                CheckFavExist = False
            End If
            dtrLoginReader.Close()

        End Function

        Public Shared Sub CheckLabel()

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim DymoAddIn As Object, DymoLabels As Object

            On Error Resume Next
            DymoAddIn = CreateObject("DYMO.DymoAddIn")
            DymoLabels = CreateObject("DYMO.DymoLabels")

            'check if successful
            If (DymoAddIn Is Nothing) Or (DymoLabels Is Nothing) Then
                MsgBox("Unable to create OLE objects")
            Else
                currentApp.Session("LBLPRT") = "LBL"
            End If

        End Sub

        Public Shared Function CheckOrderLimits(ByVal strBU As String, ByVal stragent As String, ByVal strreqID As String, ByVal strAppLevel As String) As ArrayList

            Dim myConnection As OleDbConnection
            Dim myCommand As OleDbCommand
            Dim myParameter As OleDbParameter
            Dim arrParamsOut(2) As String
            Dim arrParamsAll As ArrayList
            arrParamsAll = New ArrayList

            ' Create connection and set connection string	
            myConnection = New OleDbConnection(DbUrl)

            ' Create a new command object
            myCommand = New OleDbCommand

            ' Set the properties of the command so that it uses
            ' our connection, knows the name of the stored proc
            ' to execute, and knows that it is going to be
            ' executing a stored proc and not something else
            myCommand.Connection = myConnection
            myCommand.CommandText = "SYSADM.SP_CUSTEMP_APPROVAL2"
            myCommand.CommandType = CommandType.StoredProcedure

            ' Create our Business_unit input parameter
            myParameter = myCommand.CreateParameter()
            myParameter.ParameterName = "BU"
            myParameter.Direction = ParameterDirection.Input
            myParameter.OleDbType = OleDbType.VarChar
            myParameter.Value = strBU

            ' Add parameter to our command
            myCommand.Parameters.Add(myParameter)

            ' Create our Employee ID input parameter
            myParameter = myCommand.CreateParameter()
            myParameter.ParameterName = "EMPID"
            myParameter.Direction = ParameterDirection.Input
            myParameter.OleDbType = OleDbType.VarChar
            myParameter.Value = stragent

            ' Add parameter to our command
            myCommand.Parameters.Add(myParameter)

            ' Create our Order Number input parameter
            myParameter = myCommand.CreateParameter()
            myParameter.ParameterName = "ORDNO"
            myParameter.Direction = ParameterDirection.Input
            myParameter.OleDbType = OleDbType.VarChar
            myParameter.Value = strreqID

            ' Add parameter to our command
            myCommand.Parameters.Add(myParameter)

            ' Create our Approver level type input parameter
            myParameter = myCommand.CreateParameter()
            myParameter.ParameterName = "APPTY"
            myParameter.Direction = ParameterDirection.Input
            myParameter.OleDbType = OleDbType.VarChar
            myParameter.Value = strAppLevel

            ' Add parameter to our command
            myCommand.Parameters.Add(myParameter)

            ' Create our order limit TRUE/FALSE output parameter:

            myParameter = myCommand.CreateParameter()
            myParameter.ParameterName = "APPLM"
            myParameter.Size = 5
            myParameter.Value = ""
            myParameter.Direction = ParameterDirection.Output
            myParameter.OleDbType = OleDbType.VarChar

            myCommand.Parameters.Add(myParameter)

            ' Create our approver employee ID output parameter:

            myParameter = myCommand.CreateParameter()
            myParameter.ParameterName = "APPEM"
            myParameter.Size = 8
            myParameter.Value = ""
            myParameter.Direction = ParameterDirection.Output
            myParameter.OleDbType = OleDbType.VarChar

            myCommand.Parameters.Add(myParameter)

            ' Create order status output parameter:
            myParameter = myCommand.CreateParameter()
            myParameter.ParameterName = "APPST"
            myParameter.Size = 1
            myParameter.Value = ""
            myParameter.Direction = ParameterDirection.Output
            myParameter.OleDbType = OleDbType.VarChar

            myCommand.Parameters.Add(myParameter)

            ' Open the connection to the database
            myConnection.Open()

            ' Execute the stored procedure
            myCommand.ExecuteNonQuery()

            arrParamsOut(0) = myCommand.Parameters("APPLM").Value
            arrParamsOut(1) = myCommand.Parameters("APPEM").Value
            arrParamsOut(2) = myCommand.Parameters("APPST").Value
            arrParamsAll.Add(arrParamsOut)
            CheckOrderLimits = arrParamsAll

        End Function

        Public Shared Function checkOrdersForApproval(ByVal strStatus As String, ByVal strBU As String, ByVal strUserid As String) As Boolean

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim strSQLstring As String
            Dim strApprover As String
            Dim strApprovers As String
            Dim strChgCode As String
            If strStatus = "Q" Then
                If Convert.ToString(currentApp.Session("SHOPPAGE")).Length < 16 Then
                    currentApp.Session("SHOPPAGE") = "SHOPPINGCART.ASPX"
                End If
                If Convert.ToString(currentApp.Session("SHOPPAGE")).ToUpper.Substring(0, 16) = "SHOPPINGCARTSPC1" Then
                    strSQLstring = "SELECT" & vbCrLf & _
                        " A.REQ_ID as ORDER_NO" & vbCrLf & _
                        " FROM PS_ISA_QUICK_REQ_H A, PS_ISA_QUICK_REQ_L B" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = 'ISA00'" & vbCrLf & _
                        " AND A.REQ_STATUS = 'Q'" & vbCrLf & _
                        " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                        " AND A.REQ_ID = B.REQ_ID" & vbCrLf & _
                        " AND B.ISA_QUOTE_STATUS = 'W'" & vbCrLf & _
                        " AND B.EMPLID = '" & strUserid & "'" & vbCrLf & _
                        " AND B.QTY_REQ > 0" & vbCrLf & _
                        " AND B.PRICE_REQ > 0" & vbCrLf & _
                        " AND ROWNUM < 2"
                Else
                    strSQLstring = "SELECT A.ISA_IDENTIFIER" & vbCrLf & _
                        " FROM PS_ISA_ORD_INTFC_H A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                        " AND A.OPRID_ENTERED_BY = '" & strUserid & "'" & vbCrLf & _
                        " AND A.ORDER_STATUS = '" & strStatus & "'"
                End If
            ElseIf strStatus = "W" Then
                strApprovers = GetApprovers(strBU, strUserid, strStatus)
                If strApprovers = "False" Then
                    Return False
                End If
                'Dim objUserTbl As New clsUserTbl(strUserid, strBU)
                'strApprover = objUserTbl.IOLAppEmpID
                strSQLstring = "SELECT A.ISA_IDENTIFIER" & vbCrLf & _
                    " FROM PS_ISA_ORD_INTFC_H A" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                    " AND A.OPRID_APPROVED_BY In (" & strApprovers & ")" & vbCrLf & _
                    " AND A.ORDER_STATUS = '" & strStatus & "'"

                '" AND A.OPRID_APPROVED_BY <> '" & strApprover & "'" & vbCrLf & _
            ElseIf strStatus = "B" Then
                strApprovers = GetApprovers(strBU, strUserid, strStatus)
                If strApprovers = "False" Then
                    Return False
                End If
                'Dim objUserTbl As New clsUserTbl(strUserid, strBU)
                'strApprover = objUserTbl.IOLAppEmpID
                Dim objCustBud As New clsCustBudget(strUserid, strBU)
                strChgCode = objCustBud.ChargeCodes
                strSQLstring = "SELECT DISTINCT(A.ISA_IDENTIFIER)" & vbCrLf & _
                    " FROM PS_ISA_ORD_INTFC_H A," & vbCrLf & _
                    " PS_ISA_BUDGET_APPR C" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                    " AND A.OPRID_APPROVED_BY in (" & strApprovers & ")" & vbCrLf & _
                    " AND A.ORDER_STATUS = '" & strStatus & "'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT_OM = C.BUSINESS_UNIT_OM" & vbCrLf & _
                    " AND A.ORDER_NO = C.ORDER_NO" & vbCrLf & _
                    " AND C.ISA_CUST_CHARGE_CD IN (" & strChgCode & ")" & vbCrLf & _
                    " AND C.OPRID_APPROVED_BY = ' '"
            End If

            Dim dsOrder As DataSet = ORDBData.GetAdapter(strSQLstring)
            If dsOrder.Tables(0).Rows.Count = 0 Then
                checkOrdersForApproval = False
            Else
                checkOrdersForApproval = True
            End If
        End Function

        Public Shared Sub checkPunchINUser(ByVal strSessionID As String)

            Dim objPunchIN As New clsPunchin(strSessionID)
            Dim strUsername As String = GetUserName(objPunchIN.BusinessUnit, _
                                                    objPunchIN.UserAgent)

            If Trim(strUsername) = "" Then
                InsertNewEmp(strSessionID)
            End If

        End Sub

        Public Shared Sub CheckSTKLimits(ByVal strBU As String, ByVal stragent As String, ByVal strreqID As String, ByVal strAppLevel As String, ByVal strAppType As String)

            Dim I As Integer
            Dim X As Integer
            Dim myConnection As OleDbConnection
            Dim myCommand As OleDbCommand
            Dim myParameter As OleDbParameter
            Dim arrParamsOut As ArrayList
            Dim arrParamsAll As ArrayList
            Dim arrAppChgCds As ArrayList
            arrParamsAll = New ArrayList
            arrAppChgCds = New ArrayList
            ' Create connection and set connection string	
            myConnection = New OleDbConnection(DbUrl)

            arrAppChgCds = getChgCodes(strBU, strreqID)

            X = arrAppChgCds.Count - 1

            For I = 0 To X
                ' Create a new command object
                myCommand = New OleDbCommand

                ' Set the properties of the command so that it uses
                ' our connection, knows the name of the stored proc
                ' to execute, and knows that it is going to be
                ' executing a stored proc and not something else
                myCommand.Connection = myConnection
                myCommand.CommandText = "SYSADM.SP_CUSTBUD_APPROVAL_STK"
                myCommand.CommandType = CommandType.StoredProcedure

                ' Create our Business_unit input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "BU"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strBU

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Employee ID input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "EMPID"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = stragent

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Order Number input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "ORDNO"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strreqID

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our Approver level type input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "APPTY"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = strAppLevel

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Create our charge code input parameter
                myParameter = myCommand.CreateParameter()
                myParameter.ParameterName = "CHGCD"
                myParameter.Direction = ParameterDirection.Input
                myParameter.OleDbType = OleDbType.VarChar
                myParameter.Value = arrAppChgCds(I)

                ' Add parameter to our command
                myCommand.Parameters.Add(myParameter)

                ' Open the connection to the database
                myConnection.Open()

                ' Execute the stored procedure
                myCommand.ExecuteNonQuery()

                myConnection.Close()

            Next

        End Sub

        Public Shared Function GenerateHash(ByVal SourceText As String) As String
            'Create an encoding object to ensure the encoding standard for the source text
            Dim Ue As New UnicodeEncoding
            'Retrieve a byte array based on the source text
            Dim ByteSourceText() As Byte = Ue.GetBytes(SourceText)
            'Instantiate an MD5 Provider object
            Dim Md5 As New MD5CryptoServiceProvider
            'Compute the hash value from the source
            Dim ByteHash() As Byte = Md5.ComputeHash(ByteSourceText)
            'And convert it to String format for return
            Return Convert.ToBase64String(ByteHash)
        End Function

        Public Shared Function GenerateRndPassword(ByVal strUserid As String, ByVal bolUpdate As Boolean, ByVal strUseridCode As String) As String

            Dim dteNow As Date
            dteNow = Now().ToString("d")

            Dim strNewPass, strNewPassHash As String
            Dim whatsNext, upper, lower, intCounter

            Randomize()

            For intCounter = 1 To 10
                whatsNext = Int((1 - 0 + 1) * Rnd() + 0)
                If whatsNext = 0 Then
                    'character
                    upper = 90
                    lower = 65
                Else
                    upper = 57
                    lower = 48
                End If
                strNewPass = strNewPass & Chr(Int((upper - lower + 1) * Rnd() + lower))
            Next

            strNewPassHash = GenerateHash(strNewPass)

            If bolUpdate = "TRUE" Then
                Dim sqlstring As String = "UPDATE PS_ISA_USERS_TBL" & vbCrLf & _
                            " SET ISA_PASSWORD_ENCR = '" & strNewPassHash & "'," & vbCrLf & _
                            " LASTUPDOPRID = '" & UCase(strUserid) & "'," & vbCrLf & _
                            " LASTUPDDTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                            " where ISA_EMPLOYEE_ID = '" & UCase(strUserid) & "'"

                Dim rowsaffected As Integer = ExecNonQuery(sqlstring)
                If rowsaffected = 0 Then
                    Return "NOTFOUND"
                End If

                Dim strSQLstring As String = "SELECT A.ISA_USER_ID, A.ISA_EMPLOYEE_ID," & vbCrLf & _
                    " A.ISA_ISOL_PW1, A.ISA_ISOL_PW_DATE1," & vbCrLf & _
                    " A.ISA_ISOL_PW2, A.ISA_ISOL_PW_DATE2," & vbCrLf & _
                    " A.ISA_ISOL_PW3, A.ISA_ISOL_PW_DATE3" & vbCrLf & _
                    " FROM PS_ISA_ISOL_PW A" & vbCrLf & _
                    " WHERE A.ISA_USER_ID = '" & strUseridCode & "'" & vbCrLf & _
                    " AND A.ISA_EMPLOYEE_ID = '" & UCase(strUserid) & "'"

                Dim dtrPWReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
                If dtrPWReader.Read() Then

                    Dim strPWsql As String = "UPDATE PS_ISA_ISOL_PW" & vbCrLf & _
                            " SET ISA_ISOL_PW1 = '" & strNewPassHash & "'," & vbCrLf & _
                            " ISA_ISOL_PW_DATE1 = TO_DATE('" & dteNow & "', 'MM/DD/YYYY')," & vbCrLf & _
                            " ISA_ISOL_PW2 = '" & dtrPWReader.Item("ISA_ISOL_PW1") & "'," & vbCrLf & _
                            " ISA_ISOL_PW_DATE2 = TO_DATE('" & dtrPWReader.Item("ISA_ISOL_PW_DATE1") & "', 'MM/DD/YYYY')," & vbCrLf & _
                            " ISA_ISOL_PW3 = '" & dtrPWReader.Item("ISA_ISOL_PW2") & "'," & vbCrLf & _
                            " ISA_ISOL_PW_DATE3 = TO_DATE('" & dtrPWReader.Item("ISA_ISOL_PW_DATE2") & "', 'MM/DD/YYYY')" & vbCrLf & _
                            " WHERE ISA_USER_ID = '" & strUseridCode & "'" & vbCrLf & _
                            " AND ISA_EMPLOYEE_ID = '" & UCase(strUserid) & "'"

                    rowsaffected = ORDBData.ExecNonQuery(strPWsql)
                    If rowsaffected = 0 Then
                        ' this means that the last 3 passwords are not updated
                        ' and the random password will not work
                        ' but this should never happen
                    End If

                End If
                dtrPWReader.Close()
            End If

            GenerateRndPassword = strNewPass

        End Function

        Public Shared Function getAltApprover(ByVal strBU As String, ByVal strEmpID As String) As String

            Dim strSQLstring As String
            strSQLstring = "SELECT A.ISA_IOL_APR_ALT" & vbCrLf & _
                " FROM PS_ISA_USERS_APPRV A" & vbCrLf & _
                " WHERE A.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                " AND A.ISA_IOL_APR_EMP_ID = '" & strEmpID & "'" & vbCrLf & _
                " UNION SELECT C.ISA_IOL_APR_ALT" & vbCrLf & _
                " FROM PS_ISA_CUST_BUDGET C" & vbCrLf & _
                " WHERE C.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                " AND C.ISA_IOL_APR_EMP_ID = '" & strEmpID & "'"

            getAltApprover = ORDBData.GetScalar(strSQLstring)


        End Function

        Public Shared Function getAnnouncements() As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim I As Integer
            Dim strHeading As String
            Dim strTitle As String
            Dim strDescription As String
            Dim htmlString As System.Web.UI.HtmlTextWriter
            Dim strDBDownMsgPath As String


            strHeading = "<DIV style='DISPLAY: inline; FONT-SIZE: 20pt; WIDTH: 288px; COLOR: blue; FONT-FAMILY: Arial, Sans-Serif; HEIGHT: 38px'" & _
                "ms_positioning='FlowLayout'>SDI Announcements</DIV><br>" & vbCrLf
            Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
            Try
                connection.Open()
                connection.Close()
            Catch ex As Exception
                strTitle = "<DIV style='DISPLAY: inline; FONT-WEIGHT: bold; FONT-FAMILY: Arial, Sans-Serif; HEIGHT: 28px'" & _
                    "ms_positioning='FlowLayout'>" & Now() & _
                    " - Database down for Maintenance.</DIV><br>" & vbCrLf
                strDBDownMsgPath = currentApp.Server.MapPath("") & "\DBDownMsg\DBDownMsg.txt"
                strDescription = WebPartnerFunctions.WebPSharedFunc.getDBDownMsg(strDBDownMsgPath)
                If Trim(strDescription) = "" Then
                    strDescription = "<DIV style='DISPLAY: inline; FONT-FAMILY: Arial; HEIGHT: 15px'" & _
                    "ms_positioning='FlowLayout'>The InSite Online Database is down for Maintenance.  Please try again in a few moments...We apologize for the inconvenience.</DIV><br>" & vbCrLf
                Else
                    strDescription = "<DIV style='DISPLAY: inline; FONT-FAMILY: Arial; HEIGHT: 15px'" & _
                    "ms_positioning='FlowLayout'>" & strDescription & "</DIV><br>"
                End If
                getAnnouncements = "<SPAN>" & strHeading & strTitle & strDescription & "</SPAN>"
                sendErrorEmail("No database connection", "YES", currentApp.Request.ServerVariables("URL"), "Get Announcements - OR connection open")
                Exit Function
            End Try

            Dim strSQLstring As String = "SELECT * FROM announcements" & vbCrLf & _
                        " WHERE     (ExpireDate > '" & Now() & "')" & vbCrLf & _
                        " AND MODULEID = 1" & vbCrLf & _
                        " ORDER BY CreatedDate"

            Dim ds As DataSet = New DataSet
            Dim connString As String
            connString = SQLDBData.DbSQLUrl1
            Dim connectionSQL = New SqlConnection(connString)
            'Dim connection = New SqlConnection(DbSQLUrl)
            Try

                Try
                    connectionSQL.open()
                Catch ex As Exception

                End Try

                Dim Command = New SqlCommand(strSQLstring, connectionSQL)
                Dim dataAdapter As SqlDataAdapter = _
                        New SqlDataAdapter(Command)

                dataAdapter.Fill(ds)
                connectionSQL.close()

            Catch objException As SqlException
                connectionSQL.close()
                getAnnouncements = ""
                Exit Function
            End Try

            If ds.Tables(0).Rows.Count = 0 Then
                getAnnouncements = ""
                Exit Function
            End If

            Dim strAnnouncement As String

            For I = 0 To ds.Tables(0).Rows.Count - 1
                If Not IsDBNull(ds.Tables(0).Rows(I).Item("Description")) Then
                    strAnnouncement = ds.Tables(0).Rows(I).Item("Description")
                End If
                If Not IsDBNull(ds.Tables(0).Rows(I).Item("Description2")) Then
                    strAnnouncement = strAnnouncement & ds.Tables(0).Rows(I).Item("Description2")
                End If
                strTitle = "<DIV style='DISPLAY: inline; FONT-WEIGHT: bold; FONT-FAMILY: Arial, Sans-Serif; HEIGHT: 28px'" & _
                    "ms_positioning='FlowLayout'>" & ds.Tables(0).Rows(I).Item("CreatedDate") & _
                    " - " & ds.Tables(0).Rows(I).Item("Title") & "</DIV><br>" & vbCrLf
                strDescription = "<DIV style='DISPLAY: inline; FONT-FAMILY: Arial; HEIGHT: 15px'" & _
                    "ms_positioning='FlowLayout'>" & strAnnouncement & "</DIV><br>" & vbCrLf
                getAnnouncements = getAnnouncements & strTitle & strDescription

            Next
            getAnnouncements = "<SPAN>" & strHeading & getAnnouncements & "</SPAN>"

        End Function

        Public Shared Function GetApprovers(ByVal strbu As String, ByVal strUserid As String, ByVal strApprType As String) As String

            Dim strSQLstring As String
            Dim strApprovers As String
            Dim strChgCode As String
            Dim decApprvBudgetLimit As Decimal
            Dim dr As OleDbDataReader
            Dim I As Integer = 1
            Dim X As Integer
            Dim decLimit As Decimal

            If strApprType = "W" Then
                'Dim objusertbl As New clsUserTbl(strUserid, strbu)

                'decLimit = objusertbl.IOLAppLimit
                strSQLstring = "SELECT DISTINCT(ISA_IOL_APR_ALT) as ISA_IOL_APR_EMP_ID" & vbCrLf & _
                            " FROM PS_ISA_USERS_APPRV" & vbCrLf & _
                            " WHERE BUSINESS_UNIT = '" & strbu & "'" & vbCrLf

                '" AND ISA_IOL_APR_LIMIT <= " & decLimit
            Else
                strSQLstring = "SELECT DISTINCT(ISA_IOL_APR_ALT) as ISA_IOL_APR_EMP_ID" & vbCrLf & _
                            " FROM PS_ISA_CUST_BUDGET" & vbCrLf & _
                            " WHERE BUSINESS_UNIT = '" & strbu & "'" & vbCrLf

                '" AND ISA_IOL_APR_EMP_ID = '" & strUserid & "'"

            End If

            dr = ORDBData.GetReader(strSQLstring)
            While dr.Read()
                If I > 1 Then
                    strApprovers = strApprovers & ","
                End If
                strApprovers = strApprovers & "'" & dr.Item("ISA_IOL_APR_EMP_ID") & "'"
                I = I + 1
            End While
            dr.Close()
            ' for demo purposes only
            If strUserid.ToUpper = "FIJALKR1" Then

                'strApprovers = strApprovers & ",'FIJALKR1'"
                ' bug 05052008
                strApprovers = strApprovers & "'FIJALKR1'"
            End If
            If InStr(strApprovers, strUserid) = 0 Then
                Return "False"
            End If

            If strApprType = "W" Then
                strSQLstring = "SELECT DISTINCT(ISA_EMPLOYEE_ID) as ISA_EMPLOYEE_ID" & vbCrLf & _
                            " FROM PS_ISA_USERS_APPRV" & vbCrLf & _
                            " WHERE BUSINESS_UNIT = '" & strbu & "'" & vbCrLf & _
                            " AND ISA_IOL_APR_ALT = '" & strUserid & "'" & vbCrLf & _
                            " AND ISA_EMPLOYEE_ID IN (" & strApprovers & ")"
                dr = ORDBData.GetReader(strSQLstring)
                strApprovers = "'" & strUserid & "'"
                While dr.Read()
                    If I > 0 Then
                        strApprovers = strApprovers & ","
                    End If
                    strApprovers = strApprovers & "'" & dr.Item("ISA_EMPLOYEE_ID").ToString & "'"
                    I = I + 1
                End While
                dr.Close()
            Else
                strApprovers = "'" & strUserid & "'"
                strSQLstring = "SELECT ISA_CUST_CHARGE_CD," & vbCrLf & _
                            " ISA_CUST_BUDGET_LM," & vbCrLf & _
                            " ISA_IOL_APR_ALT" & vbCrLf & _
                            " FROM PS_ISA_CUST_BUDGET" & vbCrLf & _
                            " WHERE BUSINESS_UNIT = '" & strbu & "'" & vbCrLf
                Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)

                For I = 0 To ds.Tables(0).Rows.Count - 1
                    If ds.Tables(0).Rows(I).Item("ISA_IOL_APR_ALT") = strUserid Then
                        For X = 0 To ds.Tables(0).Rows.Count - 1
                            If ds.Tables(0).Rows(I).Item("ISA_CUST_CHARGE_CD") = _
                               ds.Tables(0).Rows(X).Item("ISA_CUST_CHARGE_CD") Then
                                If Not ds.Tables(0).Rows(I).Item("ISA_IOL_APR_ALT") = _
                                    ds.Tables(0).Rows(X).Item("ISA_IOL_APR_ALT") Then
                                    If ds.Tables(0).Rows(I).Item("ISA_CUST_BUDGET_LM") > _
                                        ds.Tables(0).Rows(X).Item("ISA_CUST_BUDGET_LM") Then
                                        strApprovers = strApprovers & ",'" & ds.Tables(0).Rows(X).Item("ISA_IOL_APR_ALT") & "'"
                                    End If
                                End If
                            End If
                        Next
                    End If
                Next

            End If
            Return strApprovers

        End Function

        Public Shared Function GetBUbyGroup(ByVal strGroup As String) As String

            If strGroup = "" Then
                strGroup = "0"
            End If
            If strGroup = "CSC Agent" Then
                GetBUbyGroup = "SDI00"
                Exit Function
            End If
            Dim strSQLstring As String = "SELECT     productviewid" & vbCrLf & _
                        " FROM GroupCatalogs" & vbCrLf & _
                        " WHERE(groupID = " & strGroup & ")"

            '" WHERE(groupID = " & strGroup & ") And (classID = 755784)"

            Dim intproductviewid As Integer = SQLDBData.GetSQLScalar(strSQLstring)
            If intproductviewid = 0 Then
                GetBUbyGroup = "0"
                Exit Function
            End If
            strSQLstring = "SELECT isa_business_unit" & vbCrLf & _
                        " FROM ps_isa_enterprise" & vbCrLf & _
                        " WHERE isa_cplus_prodview = '" & intproductviewid & "'"

            GetBUbyGroup = ORDBData.GetScalar(strSQLstring)
            If GetBUbyGroup Is Nothing Then
                GetBUbyGroup = "0"
            End If

        End Function

        Public Shared Sub GetCplusDB(ByVal strProductviewID As String)
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim strSQL As String
            If strProductviewID = "0" Then
                strProductviewID = "11"
            End If


            'Dim connectMS As New SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=ContentPlus;Data Source=ContentPlus")

            ' SQL Statement

            strSQL = "SELECT     classID" & vbCrLf & _
                        " FROM ProductViews" & vbCrLf & _
                        " WHERE(productViewID = " & strProductviewID & ")"

            ' get list of DB's from the CPlusCatalogs.txt file then
            ' try to read the production DB to get classid.  If still
            ' can't find it then go to the maintenance DB

            Dim CatalogFilePath As String = currentApp.Server.MapPath("") & "\CplusCatalogs\CPlusCatalogs.txt"

            Dim arrCatalogs As ArrayList
            arrCatalogs = New ArrayList
            Dim reader As TextReader
            If File.Exists(CatalogFilePath) Then
                reader = File.OpenText(CatalogFilePath)
                While reader.Peek <> -1
                    arrCatalogs.Add(reader.ReadLine())
                End While
                reader.Close()
            End If
            Dim I As Integer
            Dim connString As String
            connString = SQLDBData.DbSQLRepUrl1
            Dim connection As SqlConnection

            Dim command As SqlCommand
            Dim strCplusProdDB As String

            For I = 0 To arrCatalogs.Count - 1
                Try
                    connection = New SqlConnection(connString)
                    connection.ConnectionString = SQLDBData.getRepDBConnectString(connection.ConnectionString, arrCatalogs(I))
                    command = New SqlCommand(strSQL, connection)
                    connection.Open()

                    strCplusProdDB = command.ExecuteScalar()
                    connection.Close()

                    If Not Trim(strCplusProdDB) = "" Then
                        Exit For
                    End If
                Catch ex As Exception

                End Try
            Next

            If Not Trim(strCplusProdDB) = "" Then
                currentApp.Session("CplusDB") = strCplusProdDB
                Exit Sub
            End If
            ' Command, Data Reader  

            ' the connection is to the maintenance database so that the 
            ' production database can be set
            Try
                Dim rdr As SqlDataReader = SQLDBData.GetSQLReader(strSQL)

                currentApp.Session("CplusDB") = " "
                While rdr.Read()
                    'currentApp.Session("CplusDB") = rdr.Item(0) & rdr.Item(1)
                    currentApp.Session("CplusDB") = rdr.Item(0)
                End While
                rdr.Close()

            Catch objException As Exception
                'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                'currentApp.Response.Write("<hr>")
                'currentApp.Response.Write("<li>Message: " & objException.Message)
                'currentApp.Response.Write("<li>Source: " & objException.Source)
                'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                'currentApp.Response.End()
                sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQL)
                currentApp.Response.Redirect("DBErrorPage.aspx")

            End Try

        End Sub

        Public Shared Function getCPlusProddb(ByVal dbfile As String, ByVal strCatalogID As String) As String
            Dim reader As TextReader = File.OpenText(dbfile)
            Dim readerline As String
            Dim I As Integer

            Try
                While reader.Peek <> -1
                    readerline = reader.ReadLine()
                    If Mid(readerline, 7, 10) = ".url=jdbc\" Then
                        For I = 1 To readerline.Length
                            If Mid(readerline, I, 10) = "database\=" Then
                                If Mid(readerline, I + 10, 6) = strCatalogID Then
                                    reader.Close()
                                    Return Mid(readerline, I + 10, 7)
                                End If
                            End If
                        Next
                    End If

                End While
            Catch ex As Exception

            End Try
            reader.Close()
            Return "Error"

        End Function

        Public Shared Function getCPlusProddbOverride(ByVal dbfile As String, ByVal strCatalogID As String) As String
            Dim reader As TextReader
            If File.Exists(dbfile) Then
                reader = File.OpenText(dbfile)
            Else
                Return ""
            End If
            Dim readerline As String
            Dim I As Integer

            Try
                While reader.Peek <> -1
                    readerline = reader.ReadLine()
                    If readerline.Substring(0, 6) = strCatalogID Then
                        reader.Close()
                        Return readerline
                    End If
                End While
            Catch ex As Exception

            End Try
            reader.Close()
            Return ""

        End Function

        Public Shared Function getChgCodes(ByVal strbu As String, ByVal strOrderNo As String) As ArrayList

            Dim I As Integer
            Dim dr As OleDbDataReader
            Dim arrChrCode As ArrayList
            arrChrCode = New ArrayList

            Dim strSQLString As String = "SELECT distinct(B.ISA_CUST_CHARGE_CD) as Charge_code" & vbCrLf & _
                " FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B" & vbCrLf & _
                " WHERE A.BUSINESS_UNIT_OM = '" & strbu & "'" & vbCrLf & _
                " AND A.ORDER_NO = '" & strOrderNo & "'" & vbCrLf & _
                " AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT"

            dr = ORDBData.GetReader(strSQLString)

            While dr.Read()
                arrChrCode.Add(dr.Item("Charge_code"))
            End While

            dr.Close()
            Return arrChrCode

        End Function

        Public Shared Function GetCONAME(ByVal strbu As String) As String

            Dim strSQLString As String = "SELECT (C.DESCR) as B.NAME1" & vbCrLf & _
                    " FROM PS_ISA_ENTERPRISE A, PS_BUS_UNIT_TBL_OM OM, PS_LOCATION_TBL C" & vbCrLf & _
                    " WHERE A.ISA_BUSINESS_UNIT = '" & strbu & "'" & vbCrLf & _
                    " AND A.ISA_BUSINESS_UNIT = OM.BUSINESS_UNIT" & vbCrLf & _
                        " AND OM.LOCATION = C.LOCATION" & vbCrLf & _
                        " AND C.EFFDT =" & vbCrLf & _
                        " (SELECT MAX(C_ED.EFFDT) FROM PS_LOCATION_TBL C_ED" & vbCrLf & _
                        " WHERE C.SETID = C_ED.SETID" & vbCrLf & _
                        " AND C.LOCATION = C_ED.LOCATION" & vbCrLf & _
                        " AND C_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                        " AND ROWNUM < 2"

            GetCONAME = ORDBData.GetScalar(strSQLString)

        End Function

        Public Shared Function GetCplusItemID(ByVal strSDIItemID As String) As String

            Dim strSQLString As String = "SELECT A.ISA_CP_PROD_ID" & vbCrLf & _
                    " FROM PS_ISA_CP_JUNCTION A" & vbCrLf & _
                    " WHERE A.INV_ITEM_ID = '" & strSDIItemID & "'"

            GetCplusItemID = ORDBData.GetScalar(strSQLString)

        End Function

        Public Shared Function getDBDownMsg(ByVal strDBDownMsgPath As String) As String
            Dim reader As TextReader
            If File.Exists(strDBDownMsgPath) Then
                reader = File.OpenText(strDBDownMsgPath)
            Else
                Return ""
            End If
            Dim readerline As String
            Try
                readerline = reader.ReadLine()
            Catch ex As Exception
                Return ""
            End Try
            reader.Close()
            Return readerline

        End Function

        'Public Shared Function getSiteVer(ByVal strProdViewID As String) As String

        '    'Gives us a reference to the current asp.net 
        '    'application executing the method.
        '    Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

        '    currentApp.Session.Remove("CPVersion")
        '    Dim strSQLString As String = "SELECT count(*)" & vbCrLf & _
        '                " FROM classavailableproducts" & vbCrLf & _
        '                " WHERE productviewid = " & strProdViewID & vbCrLf

        '    '" AND (classID = " & currentApp.Session("CPlusDB") & ")"

        '    Dim intRecordCount As Integer = SQLDBData.GetProdSQLScalarSPC(strSQLString)

        '    If intRecordCount > 10 Then
        '        getSiteVer = "Ver1.3"
        '    Else
        '        getSiteVer = "Ver1.2"
        '    End If

        'End Function

        Public Shared Function getShipToDescr(ByVal strShipTo As String) As String

            Dim strSQLstring As String
            strSQLstring = "SELECT A.DESCR" & vbCrLf & _
                " FROM PS_SHIPTO_TBL A" & vbCrLf & _
                " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
                " AND A.SHIPTO_ID = '" & strShipTo & "'" & vbCrLf & _
                " AND A.EFFDT =" & vbCrLf & _
                " (SELECT MAX(A_ED.EFFDT) FROM PS_SHIPTO_TBL A_ED" & vbCrLf & _
                " WHERE A.SETID = A_ED.SETID" & vbCrLf & _
                " AND A.SHIPTO_ID = A_ED.SHIPTO_ID" & vbCrLf & _
                " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                " AND A.EFF_STATUS = 'A'"

            getShipToDescr = ORDBData.GetScalar(strSQLstring)

        End Function

        Public Shared Function getShipToEmpName(ByVal strShipTo As String) As String

            Dim strSQLstring As String
            strSQLstring = "SELECT A.NAME" & vbCrLf & _
                        " FROM PS_ISA_EMP_LOC_XRF A" & vbCrLf & _
                        " WHERE A.EFFDT =" & vbCrLf & _
                        " (SELECT MAX(A_ED.EFFDT) FROM PS_ISA_EMP_LOC_XRF A_ED" & vbCrLf & _
                        " WHERE A.LOCATION = A_ED.LOCATION" & vbCrLf & _
                        " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                        " AND A.LOCATION = '" & strShipTo & "'" & vbCrLf & _
                        " AND A.EFF_STATUS = 'A'"

            getShipToEmpName = ORDBData.GetScalar(strSQLstring)

        End Function

        Public Shared Function GetDisplayName(ByVal name As String, ByVal strBU As String, ByVal strUserid As String) As String
            If IsDBNull(name) Then
                GetDisplayName = GetEmpName(strBU, strUserid)
                'GetDisplayName = "UNKNOWN"
            Else
                GetDisplayName = name
            End If

        End Function

        Public Shared Function GetEmpChgCd(ByVal strBU As String) As String
            Dim strSQLstring As String

            strSQLstring = "SELECT 'E'" & vbCrLf & _
                        " FROM PS_BUS_UNIT_TBL_OM A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                        " AND (A.ISA_CHG_CD_SEG_1 = 'E'" & vbCrLf & _
                        " OR A.ISA_CHG_CD_SEG_2 = 'E'" & vbCrLf & _
                        " OR A.ISA_CHG_CD_SEG_3 = 'E'" & vbCrLf & _
                        " OR A.ISA_CHG_CD_SEG_4 = 'E'" & vbCrLf & _
                        " OR A.ISA_CHG_CD_SEG_5 = 'E')"

            GetEmpChgCd = ORDBData.GetScalar(strSQLstring)

        End Function
        Public Shared Function GetEmpBU(ByVal prodview As String) As String

            Dim strBU As String
            Dim strSQLString As String = "Select ISA_BUSINESS_UNIT" & vbCrLf & _
            " from PS_ISA_ENTERPRISE" & vbCrLf & _
            " where ISA_CPLUS_PRODVIEW = '" & prodview & "'" & vbCrLf

            Dim dtrBUReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
            If dtrBUReader.HasRows Then
                dtrBUReader.Read()
                strBU = dtrBUReader("ISA_BUSINESS_UNIT")
            Else
                strBU = "NOTFN"
            End If

            dtrBUReader.Close()

            Return strBU

        End Function

        Public Shared Function GetEmpID(ByVal siteid As String, ByVal userid As String) As String

            Dim strSQLString As String = "Select A.ISA_EMPLOYEE_ID" & vbCrLf & _
            " from PS_ISA_EMPL_TBL A" & vbCrLf & _
            " where A.ISA_EMPLOYEE_NAME = '" & UCase(userid) & "'" & vbCrLf & _
            " AND A.BUSINESS_UNIT = '" & Trim(siteid) & "'" & vbCrLf

            Dim strUserID As String
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Try
                strUserID = ORDBData.GetScalar(strSQLString)
                Return strUserID
            Catch objException As Exception
                'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                'currentApp.Response.Write("<hr>")
                'currentApp.Response.Write("<li>Message: " & objException.Message)
                'currentApp.Response.Write("<li>Source: " & objException.Source)
                'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                'currentApp.Response.End()
                sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLString)
                currentApp.Response.Redirect("DBErrorPage.aspx")

            End Try

        End Function

        Public Shared Function GetEmpName(ByVal siteid As String, ByVal userid As String) As String

            Dim strSQLString As String = "Select A.ISA_EMPLOYEE_NAME" & vbCrLf & _
            " from PS_ISA_EMPL_TBL A" & vbCrLf & _
            " where A.ISA_EMPLOYEE_ID = '" & UCase(userid) & "'" & vbCrLf & _
            " AND A.BUSINESS_UNIT = '" & Trim(siteid) & "'" & vbCrLf

            Dim strUserID As String
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Try
                strUserID = ORDBData.GetScalar(strSQLString)
                Return strUserID
            Catch objException As Exception
                'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                'currentApp.Response.Write("<hr>")
                'currentApp.Response.Write("<li>Message: " & objException.Message)
                'currentApp.Response.Write("<li>Source: " & objException.Source)
                'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                'currentApp.Response.End()
                sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLString)
                currentApp.Response.Redirect("DBErrorPage.aspx")

            End Try

        End Function

        Public Shared Function getGroupID(ByVal struserid As String) As Integer

            Dim strSQLString As String
            strSQLString = "SELECT groupID" & vbCrLf & _
                    " FROM GroupUsers" & vbCrLf & _
                    " WHERE (userID = '" & struserid & "')"

            Dim strGroupid As String = SQLDBData.GetProdSQLScalar(strSQLString)
            If Trim(strGroupid) = "" Then
                strGroupid = SQLDBData.GetSQLScalar(strSQLString)
            End If
            getGroupID = Convert.ToInt32(strGroupid)

        End Function

        Public Shared Function getIsApprover() As String
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim strSQLstring As String = "select ISA_IOL_APR_ALT" & vbCrLf & _
                        " from ps_isa_users_apprv" & vbCrLf & _
                        " where business_unit = '" & currentApp.Session("BUSUNIT") & "'" & vbCrLf & _
                        " and ISA_IOL_APR_ALT = '" & currentApp.Session("USERID") & "'" & vbCrLf & _
                        " union select ISA_IOL_APR_ALT" & vbCrLf & _
                        " from ps_isa_cust_budget" & vbCrLf & _
                        " where business_unit = '" & currentApp.Session("BUSUNIT") & "'" & vbCrLf & _
                        " and ISA_IOL_APR_ALT = '" & currentApp.Session("USERID") & "'"
            getIsApprover = ORDBData.GetScalar(strSQLstring)
        End Function

        Public Shared Function getLocations() As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim strSQLString As String
            Dim I As Integer
            Dim strLocations As String

            strSQLString = "SELECT A.LOCATION" & vbCrLf & _
                    " FROM PS_ISA_EMP_LOC_XRF A, PS_ISA_SDR_BU_LOC B" & vbCrLf & _
                    " WHERE A.EFFDT =" & vbCrLf & _
                    " (SELECT MAX(A_ED.EFFDT) FROM PS_ISA_EMP_LOC_XRF A_ED" & vbCrLf & _
                    " WHERE A.LOCATION = A_ED.LOCATION" & vbCrLf & _
                    " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf
            If Not currentApp.Session("AGENTUSERID") = "" Then
                strSQLString = strSQLString & "AND A.EMPLID = '" & currentApp.Session("AGENTUSERID") & "'" & vbCrLf
            Else
                strSQLString = strSQLString & "AND A.EMPLID = '" & currentApp.Session("USERID") & "'" & vbCrLf
            End If
            strSQLString = strSQLString & " AND A.LOCATION = B.LOCATION" & vbCrLf & _
                    " AND B.BU_STATUS = '1'" & vbCrLf

            Dim ds As DataSet = ORDBData.GetAdapter(strSQLString)

            If ds.Tables(0).Rows.Count > 0 Then
                For I = 0 To ds.Tables(0).Rows.Count - 1
                    If I = 0 Then
                        strLocations = "("
                    End If
                    strLocations = strLocations & "'" & ds.Tables(0).Rows(I).Item("LOCATION") & "'"
                    If I = ds.Tables(0).Rows.Count - 1 Then
                        strLocations = strLocations & ")"
                    Else
                        strLocations = strLocations & ","
                    End If
                Next
            End If

            Return strLocations

        End Function

        Public Shared Function getMfgID(ByVal strMfgName As String)
            Dim strSQLstring As String
            strSQLstring = "SELECT A.MFG_ID" & vbCrLf & _
                " FROM PS_MANUFACTURER A" & vbCrLf & _
                " WHERE A.DESCR = '" & strMfgName.ToUpper & "'"
            getMfgID = ORDBData.GetScalar(strSQLstring)
        End Function

        Public Shared Function getNSTKreqID() As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim reqIDSQL As String = "Update PS_BUS_UNIT_TBL_PM" & vbCrLf & _
                        " Set REQ_ID_LAST_USED  = " & vbCrLf & _
                        " (REQ_ID_LAST_USED  + 1)" & vbCrLf & _
                        " Where BUSINESS_UNIT = '" & currentApp.Session("SITEBU").ToString & "'" & vbCrLf

            Dim reqSelSQL As String = " Select REQ_ID_LAST_USED " & vbCrLf & _
                        " From PS_BUS_UNIT_TBL_PM" & vbCrLf & _
                        " Where BUSINESS_UNIT = '" & currentApp.Session("SITEBU").ToString & "'" & vbCrLf
            Dim strreqid As String

            Dim rowsaffected As Integer = ExecNonQuery(reqIDSQL)
            If rowsaffected = 0 Then
                Return rowsaffected
            End If

            Dim dtrreqIDReader As OleDbDataReader = ORDBData.GetReader(reqSelSQL)
            dtrreqIDReader.Read()
            strreqid = Right("000000000" & dtrreqIDReader("REQ_ID_LAST_USED"), 10)
            dtrreqIDReader.Close()
            Return strreqid

        End Function

        Public Shared Function getprivhashtable(ByVal strUserid As String, ByVal strbu As String, ByVal strYN As String) As Hashtable

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim hashPrivs As Hashtable
            hashPrivs = New Hashtable

            Dim strSQLstring As String = "SELECT ISA_IOL_OP_NAME," & vbCrLf & _
                " ISA_IOL_OP_TYPE, ISA_IOL_OP_VALUE" & vbCrLf & _
                " FROM PS_ISA_USERS_PRIVS" & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & Convert.ToString(strUserid).ToUpper & "'" & vbCrLf
            If currentApp.Session("USERTYPE") = "ADMINX" Then
                strSQLstring = strSQLstring & " AND BUSINESS_UNIT = '" & currentApp.Session("BUSUNIT") & "'"
            Else
                strSQLstring = strSQLstring & " AND BUSINESS_UNIT = '" & strbu & "'"
            End If

            'If strYN = "Y" Then
            '    strSQLstring = strSQLstring & " AND ISA_IOL_OP_VALUE = 'Y'"
            'End If

            Dim I As Integer
            Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)

            If ds.Tables(0).Rows.Count = 0 Then
                If Not currentApp.Session("EMAIL") = "NOTISOL" Then
                    hashPrivs.Add("NONE", "NONE")
                End If
            Else
                For I = 0 To ds.Tables(0).Rows.Count - 1
                    If strYN = "Y" Then
                        If ds.Tables(0).Rows(I).Item("ISA_IOL_OP_VALUE") = "Y" Then
                            hashPrivs.Add(ds.Tables(0).Rows(I).Item(0), ds.Tables(0).Rows(I).Item(1))
                        End If
                    Else
                        hashPrivs.Add(ds.Tables(0).Rows(I).Item(0), ds.Tables(0).Rows(I).Item(1))
                    End If
                Next
            End If
            getprivhashtable = hashPrivs
        End Function

        Public Shared Function getOrdreqID() As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim strreqid As String
            Dim strOrderNo As String
            Dim bolOrderExist As Boolean
            bolOrderExist = True

            Do Until bolOrderExist = False

                Dim reqLockSQL As String = "UPDATE psprcslock" & vbCrLf & _
                            " SET prcslock = 1"

                Dim reqIDSQL As String = "Update PS_AUTO_NUM_TBL" & vbCrLf & _
                            " SET LAST_AUTO_NBR = (LAST_AUTO_NBR + 1)" & vbCrLf & _
                            " Where SETID = 'MAIN1'" & vbCrLf & _
                            " and NUM_TYPE = 'SORD'" & vbCrLf & _
                            " and BEG_SEQ = '" & currentApp.Session("SITEPREFIX") & "'" & vbCrLf

                Dim reqSelSQL As String = "Select LAST_AUTO_NBR" & vbCrLf & _
                            " FROM PS_AUTO_NUM_TBL" & vbCrLf & _
                            " where SETID = 'MAIN1'" & vbCrLf & _
                            " and NUM_TYPE = 'SORD'" & vbCrLf & _
                            " and BEG_SEQ = '" & currentApp.Session("SITEPREFIX") & "'" & vbCrLf

                ' the req ID now comes from the BUS_UNIT_TBL_PM table (3/28/07) per Scott
                ' changed back to the PS_AUTO_NUM_TBL

                'Dim reqIDSQL As String = "Update PS_BUS_UNIT_TBL_PM" & vbCrLf & _
                '            " SET REQ_ID_LAST_USED = (REQ_ID_LAST_USED + 1)" & vbCrLf & _
                '            " Where BUSINESS_UNIT = '" & currentApp.Session("SITEBU") & "'" & vbCrLf

                'Dim reqSelSQL As String = "Select REQ_ID_LAST_USED" & vbCrLf & _
                '            " FROM PS_BUS_UNIT_TBL_PM" & vbCrLf & _
                '            " Where BUSINESS_UNIT = '" & currentApp.Session("SITEBU") & "'" & vbCrLf

                Dim rowsaffected As Integer = ExecNonQuery(reqLockSQL)
                If rowsaffected = 0 Then
                    Return rowsaffected.ToString
                End If

                Dim rowsaffected1 As Integer = ExecNonQuery(reqIDSQL)
                If rowsaffected = 0 Then
                    Return rowsaffected1.ToString
                End If

                Dim dtrreqIDReader As OleDbDataReader = ORDBData.GetReader(reqSelSQL)
                dtrreqIDReader.Read()
                strreqid = (currentApp.Session("SITEPREFIX") & Right("000000" & dtrreqIDReader("LAST_AUTO_NBR"), 7))
                dtrreqIDReader.Close()
                Dim strSQLstring As String = "SELECT A.REQ_ID" & vbCrLf & _
                " FROM PS_REQ_HDR A" & vbCrLf & _
                " WHERE A.BUSINESS_UNIT = '" & currentApp.Session("SITEBU") & "'" & vbCrLf & _
                " AND A.REQ_ID = '" & strreqid & "'"
                strOrderNo = ORDBData.GetScalar(strSQLstring)
                If Not strOrderNo = strreqid Then
                    bolOrderExist = False
                End If
            Loop
            Return strreqid

        End Function

        Public Shared Function getORO(ByVal prodview As String) As Boolean
            ' currently this function is not being used.  The SQR that
            ' pulls orders from the INTFC table determines if the items
            ' is an ORO.
        End Function

        Public Shared Function getProdViewID(ByVal strAgent As String) As Integer

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim intGroupid As Integer = getGroupID(strAgent)
            If intGroupid = 0 Then
                getProdViewID = 0
                Exit Function
            End If

            Dim strSQLString As String = "SELECT productviewid" & vbCrLf & _
                        " FROM GroupCatalogs" & vbCrLf & _
                        " WHERE groupID = " & intGroupid & vbCrLf

            '" AND (classID = " & currentApp.Session("CPlusDB") & ")"

            Dim strSQLProductviewid As String = SQLDBData.GetProdSQLScalar(strSQLString)
            If Trim(strSQLProductviewid) = "" Then
                strSQLProductviewid = SQLDBData.GetSQLScalar(strSQLString)
            End If
            getProdViewID = Convert.ToInt32(strSQLProductviewid)

        End Function


        Public Shared Function getProdViewIDfromEnterprise(ByVal strSiteID As String) As Integer

            Dim strSQLString As String = "SELECT ISA_CPLUS_PRODVIEW" & vbCrLf & _
                        " FROM ps_isa_enterprise" & vbCrLf & _
                        " WHERE ISA_BUSINESS_UNIT = 'I0" & strSiteID & "'"

            getProdViewIDfromEnterprise = Convert.ToInt32(ORDBData.GetScalar(strSQLString))

        End Function

        Public Shared Function getAdjustedDueDate(ByVal dteDueDate As Date, ByVal strOrderNo As String) As DateTime

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim strQuoteDt As String
            Dim dteNowy As Date = Now().ToString("yyyy-M-d")
            'Dim intReqDays As Integer = DateDiff(DateInterval.Day, dteNowy, dteDueDate)
            'If intReqDays > 5 Then
            '    Return dteReqBy
            'End If
            Dim strSQLstring As String
            strSQLstring = "SELECT TRIM(A.EMAIL_DATETIME) as REQBY_DT" & vbCrLf & _
                        " FROM PS_ISA_REQ_EML_LOG A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & currentApp.Session("SITEBU") & "'" & vbCrLf & _
                        " AND A.REQ_ID = '" & strOrderNo & "'" & vbCrLf & _
                        " AND UPPER( A.ISA_SUBJECT ) LIKE 'QUOTE PROCESS%'"

            strQuoteDt = ORDBData.GetScalar(strSQLstring)
            If Trim(strQuoteDt) = "" Then
                Return dteDueDate
            End If
            Dim dteQuoteDt As Date = strQuoteDt
            If IsDBNull(dteQuoteDt) Then
                Return dteDueDate
                'ElseIf dteQuoteDt Is Nothing Then
                '    Return dteReqBy
            End If

            Dim intDelayDays As Integer = DateDiff(DateInterval.Day, dteQuoteDt, dteNowy)

            If intDelayDays = 0 Then
                Return dteDueDate
            Else
                Return dteDueDate.AddDays(intDelayDays)
            End If

        End Function

        Public Shared Function getShipVia(ByVal strShipType As String, ByRef connectionDB As OleDbConnection) As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim strSQLString As String
            strSQLString = "SELECT DESCRSHORT" & vbCrLf & _
                           " FROM PS_SHIP_METHOD" & vbCrLf & _
                           " where SETID = 'MAIN1'" & vbCrLf & _
                           " and SHIP_TYPE_ID = '" & strShipType & "'"

            Dim Command1 As New OleDbCommand
            Command1 = New OleDbCommand(strSQLString, connectionDB)

            'getShipVia = ORDBData.GetScalar(strSQLString)
            Try
                getShipVia = Command1.ExecuteScalar()
            Catch objException As Exception
                sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLString)
                getShipVia = ""
            End Try

        End Function

        Public Shared Function GetSiteName(ByVal sBU As String) As String
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim strSQLString = "SELECT DESCR" & vbCrLf & _
                " FROM PS_BUS_UNIT_TBL_FS" & vbCrLf & _
                " WHERE BUSINESS_UNIT = '" & sBU & "'" & vbCrLf

            GetSiteName = ORDBData.GetScalar(strSQLString)

        End Function

        Public Shared Function GetSiteBU(ByVal sBU As String) As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim strSiteBU As String

            Dim strSQLstring As String
            strSQLstring = "SELECT A.BUSINESS_UNIT" & vbCrLf & _
                " FROM PS_PO_LOADER_DFL A" & vbCrLf & _
                " WHERE SUBSTR(A.LOADER_BU,2) = '" & sBU.Substring(1, 4) & "'" & vbCrLf
            Try
                Dim dtrPrefixReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
                If dtrPrefixReader.Read() Then
                    strSiteBU = dtrPrefixReader("BUSINESS_UNIT").ToString
                Else
                    dtrPrefixReader.Close()
                    strSQLstring = "SELECT A.TAX_COMPANY" & vbCrLf & _
                                " FROM PS_BUS_UNIT_TBL_OM A" & vbCrLf & _
                                " WHERE A.BUSINESS_UNIT = '" & sBU & "'" & vbCrLf
                    dtrPrefixReader = ORDBData.GetReader(strSQLstring)
                    If dtrPrefixReader.Read() Then
                        strSiteBU = dtrPrefixReader("TAX_COMPANY").ToString
                    End If
                End If
                dtrPrefixReader.Close()

                If Trim(strSiteBU) = "" Then
                    strSiteBU = "ISA00"
                End If

                Return strSiteBU
            Catch objException As Exception
                'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                'currentApp.Response.Write("<hr>")
                'currentApp.Response.Write("<li>Message: " & objException.Message)
                'currentApp.Response.Write("<li>Source: " & objException.Source)
                'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                'currentApp.Response.End()
                sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLstring)
                currentApp.Response.Redirect("DBErrorPage.aspx")

            End Try

        End Function

        Public Shared Function GetSitePrefix1(ByVal sBU As String) As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim strSitePrefix
            Dim strSQLString = "SELECT ISA_SITE_CODE" & vbCrLf & _
                " FROM PS_BUS_UNIT_TBL_OM" & vbCrLf & _
                " WHERE BUSINESS_UNIT = '" & sBU & "'" & vbCrLf
            Try
                Dim dtrPrefixReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
                dtrPrefixReader.Read()
                strSitePrefix = dtrPrefixReader("ISA_SITE_CODE")
                dtrPrefixReader.Close()

                Return strSitePrefix
            Catch objException As Exception
                'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                'currentApp.Response.Write("<hr>")
                'currentApp.Response.Write("<li>Message: " & objException.Message)
                'currentApp.Response.Write("<li>Source: " & objException.Source)
                'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                'currentApp.Response.End()
                sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLString)
                currentApp.Response.Redirect("DBErrorPage.aspx")

            End Try

        End Function

        Public Shared Function getSTKreqID() As String

            'no longer accessed - all order number come from the getNSTKreqID method

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim reqIDSQL As String = "Update PS_AUTO_NUM_TBL" & vbCrLf & _
                        " SET LAST_AUTO_NBR = (LAST_AUTO_NBR + 1)" & vbCrLf & _
                        " Where SETID = 'MAIN1'" & vbCrLf & _
                        " and NUM_TYPE = 'CPOR'" & vbCrLf & _
                        " and BEG_SEQ = '" & currentApp.Session("SITEPREFIX") & "'" & vbCrLf

            Dim reqSelSQL As String = "Select LAST_AUTO_NBR" & vbCrLf & _
                        " FROM PS_AUTO_NUM_TBL" & vbCrLf & _
                        " where SETID = 'MAIN1'" & vbCrLf & _
                        " and NUM_TYPE = 'CPOR'" & vbCrLf & _
                        " and BEG_SEQ = '" & currentApp.Session("SITEPREFIX") & "'" & vbCrLf

            Dim strreqid As String

            Dim rowsaffected As Integer = ExecNonQuery(reqIDSQL)
            If rowsaffected = 0 Then
                Return rowsaffected
            End If

            Dim dtrreqIDReader As OleDbDataReader = ORDBData.GetReader(reqSelSQL)
            dtrreqIDReader.Read()
            strreqid = Right("000000" & dtrreqIDReader("LAST_AUTO_NBR"), 7)
            dtrreqIDReader.Close()
            Return strreqid

        End Function

        Public Shared Function GetUsereplus(ByVal siteid As String, ByVal userid As String) As DataSet

            Dim strSQLString As String = "Select A.BUSINESS_UNIT, A.ISA_EMPLOYEE_ID, A.ISA_EMPLOYEE_NAME," & vbCrLf & _
            " A.ISA_PASSWORD_ENCR, A.ISA_EMPLOYEE_EMAIL, A.ISA_EMPLOYEE_ACTYP," & vbCrLf & _
            " B.CUST_ID, B.ISA_CPLUS_PRODVIEW, (C.DESCR) as NAME1, B.ISA_SITE_EMAIL, A.ISA_SDI_EMPLOYEE, A.PHONE_NUM" & vbCrLf & _
            " from PS_ISA_USERS_TBL A, PS_ISA_ENTERPRISE B," & vbCrLf & _
            " PS_BUS_UNIT_TBL_OM OM, PS_LOCATION_TBL C" & vbCrLf & _
            " where A.ISA_EMPLOYEE_ID = '" & UCase(userid) & "'" & vbCrLf & _
            " AND A.BUSINESS_UNIT = '" & Trim(siteid) & "'" & vbCrLf & _
            " AND A.BUSINESS_UNIT = B.ISA_BUSINESS_UNIT" & vbCrLf & _
            " AND B.ISA_BUSINESS_UNIT = OM.BUSINESS_UNIT" & vbCrLf & _
                        " AND OM.LOCATION = C.LOCATION" & vbCrLf & _
                        " AND C.EFFDT =" & vbCrLf & _
                        " (SELECT MAX(C_ED.EFFDT) FROM PS_LOCATION_TBL C_ED" & vbCrLf & _
                        " WHERE C.SETID = C_ED.SETID" & vbCrLf & _
                        " AND C.LOCATION = C_ED.LOCATION" & vbCrLf & _
                        " AND C_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                        " AND ROWNUM < 2"

            Dim userds As DataSet = New DataSet
            userds = returnUser(strSQLString, "")
            Return userds

        End Function

        Public Shared Function getUserGroup(ByVal strBU As String) As Integer

            Dim strSQLstring As String
            strSQLstring = "SELECT A.ISA_CPLUS_PRODVIEW" & vbCrLf & _
                        " FROM PS_ISA_ENTERPRISE A" & vbCrLf & _
                        " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
                        " AND A.ISA_BUSINESS_UNIT = '" & strBU & "'"

            Dim strProcViewid As String
            strProcViewid = ORDBData.GetScalar(strSQLstring)
            If Trim(strProcViewid) = "" Then
                Return "0"
            End If

            strSQLstring = "SELECT groupID" & vbCrLf & _
                            " FROM GroupCatalogs" & vbCrLf & _
                            " WHERE (productViewID = '" & strProcViewid & "')"

            Dim strGroupID As String
            strGroupID = SQLDBData.GetSQLScalar(strSQLstring)
            If Trim(strGroupID) = "" Then
                Return "0"
            End If
            Return strGroupID

        End Function

        Public Shared Function getUserGroupsName(ByVal intGroupID As Integer) As String
            Dim strSQLstring As String = "SELECT groupName FROM userGroups" & vbCrLf & _
                        " WHERE groupID = " & intGroupID

            getUserGroupsName = SQLDBData.GetProdSQLScalar(strSQLstring)

        End Function

        Public Shared Function GetUserInfo(ByVal strBU As String, ByVal strEmpID As String) As DataSet

            Dim strSQLstring As String
            strSQLstring = "SELECT A.ISA_EMPLOYEE_EMAIL, A.PHONE_NUM" & vbCrLf & _
                    " FROM PS_ISA_USERS_TBL A" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND ISA_EMPLOYEE_ID = '" & strEmpID & "'"

            GetUserInfo = ORDBData.GetAdapter(strSQLstring)

        End Function

        Public Shared Function GetUserlogin(ByVal userName As String, ByVal userPassword As String) As DataSet

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            If userName.ToUpper = "TEMPSDI" Then
                If DbUrl.Substring(DbUrl.Length - 4).ToUpper = "PLGR" Or _
                DbUrl.Substring(DbUrl.Length - 4).ToUpper = "RPTG" Then
                    If Not currentApp.Request.ServerVariables("REMOTE_ADDR").Substring(0, 6) = "172.27" And _
                        Not currentApp.Request.ServerVariables("REMOTE_ADDR").Substring(0, 9) = "192.168.2" And _
                        Not currentApp.Request.ServerVariables("REMOTE_ADDR").Substring(0, 6) = "199.117" And _
                        Not currentApp.Request.ServerVariables("REMOTE_ADDR").Substring(0, 9) = "127.0.0.1" Then
                        userName = "Invalid"
                    End If
                Else
                    userName = "Invalid"
                End If
            End If

            Dim strPasswhash As String = GenerateHash(Trim(userPassword))

            Dim strSQLString As String = "Select A.BUSINESS_UNIT, A.ISA_EMPLOYEE_ID, A.ISA_EMPLOYEE_NAME," & vbCrLf & _
                    " A.ISA_PASSWORD_ENCR, A.ISA_EMPLOYEE_EMAIL, A.ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                    " B.CUST_ID, B.ISA_CPLUS_PRODVIEW, (C.DESCR) as NAME1, B.ISA_SITE_EMAIL, A.ISA_SDI_EMPLOYEE, A.PHONE_NUM," & vbCrLf & _
                    " D.ISA_ISOL_PW_DATE1" & vbCrLf & _
                    " from PS_ISA_USERS_TBL A, PS_ISA_ENTERPRISE B," & vbCrLf & _
                    " PS_BUS_UNIT_TBL_OM OM, PS_LOCATION_TBL C," & vbCrLf & _
                    " PS_ISA_ISOL_PW D" & vbCrLf & _
                    " where A.ISA_EMPLOYEE_ID = '" & UCase(userName) & "'" & vbCrLf
            If Not userPassword = "StopPre" Then
                strSQLString = strSQLString & " AND A.ISA_PASSWORD_ENCR = '" & strPasswhash & "'" & vbCrLf
            End If
            strSQLString = strSQLString & _
            " AND A.ACTIVE_STATUS = 'A'" & vbCrLf & _
            " AND A.BUSINESS_UNIT = B.ISA_BUSINESS_UNIT" & vbCrLf & _
            " AND B.ISA_BUSINESS_UNIT = OM.BUSINESS_UNIT" & vbCrLf & _
                        " AND OM.LOCATION = C.LOCATION" & vbCrLf & _
                        " AND C.EFFDT =" & vbCrLf & _
                        " (SELECT MAX(C_ED.EFFDT) FROM PS_LOCATION_TBL C_ED" & vbCrLf & _
                        " WHERE C.SETID = C_ED.SETID" & vbCrLf & _
                        " AND C.LOCATION = C_ED.LOCATION" & vbCrLf & _
                        " AND C_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                        " AND ROWNUM < 2" & vbCrLf & _
            " AND A.ISA_USER_ID = D.ISA_USER_ID(+)" & vbCrLf & _
            " AND A.ISA_EMPLOYEE_ID = D.ISA_EMPLOYEE_ID(+)"

            Dim strSQLString1 As String = "Select A.BUSINESS_UNIT, A.ISA_EMPLOYEE_ID, A.ISA_EMPLOYEE_NAME," & vbCrLf & _
                    " A.ISA_PASSWORD_ENCR, A.ISA_EMPLOYEE_EMAIL, A.ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                    " A.ISA_SDI_EMPLOYEE, A.PHONE_NUM" & vbCrLf & _
                    " from PS_ISA_USERS_TBL A" & vbCrLf & _
                    " where A.BUSINESS_UNIT = 'SDI00'" & vbCrLf & _
                    " AND A.ISA_EMPLOYEE_ID = '" & UCase(userName) & "'" & vbCrLf
            If Not userPassword = "StopPre" Then
                strSQLString = strSQLString & " AND A.ISA_PASSWORD_ENCR = '" & strPasswhash & "'" & vbCrLf
            End If
            strSQLString = strSQLString & _
                    " AND A.ACTIVE_STATUS = 'A'" & vbCrLf

            Dim userds = New DataSet
            userds = returnUser(strSQLString, strSQLString1)
            Return userds

        End Function

        Public Shared Function GetUserName(ByVal siteid As String, ByVal userid As String) As String

            Dim strSQLString As String = "Select A.ISA_EMPLOYEE_NAME" & vbCrLf & _
            " from PS_ISA_USERS_TBL A" & vbCrLf & _
            " where A.ISA_EMPLOYEE_ID = '" & UCase(userid) & "'" & vbCrLf & _
            " AND A.BUSINESS_UNIT = '" & Trim(siteid) & "'" & vbCrLf

            Dim strUserID As String
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Try
                strUserID = ORDBData.GetScalar(strSQLString)
                Return strUserID
            Catch objException As Exception
                'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                'currentApp.Response.Write("<hr>")
                'currentApp.Response.Write("<li>Message: " & objException.Message)
                'currentApp.Response.Write("<li>Source: " & objException.Source)
                'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                'currentApp.Response.End()
                sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLString)
                currentApp.Response.Redirect("DBErrorPage.aspx")

            End Try

        End Function

        Public Shared Function GetUserpassw(ByVal userName As String) As DataSet

            Dim strSQLString As String = "Select A.ISA_PASSWORD_ENCR, A.ISA_EMPLOYEE_EMAIL, A.ISA_USER_ID" & vbCrLf & _
            " from PS_ISA_USERS_TBL A" & vbCrLf & _
            " where A.ISA_EMPLOYEE_ID = '" & UCase(userName) & "'" & vbCrLf & _
            " AND A.ACTIVE_STATUS = 'A'" & vbCrLf

            GetUserpassw = ORDBData.GetAdapter(strSQLString)

        End Function

        Public Shared Sub GetUserPriv(ByVal siteid As String, ByVal userid As String)

            'Note - on 3/1/2004 the access privileges were obtained from the
            ' ISA_USERS_PRIV table.  So therefore this function is no longer
            ' needed to get the users access privileges.  However, since the 
            ' ISA_WEB_LOGIN table also holds the switch userid for the Call
            ' Center agents, this functio is still being executed until the 
            ' agent field can be removed.

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim strSQLString As String = "SELECT " & vbCrLf & _
                " ISA_WEB_NEWITEM, ISA_WEB_STOCKREQ, ISA_WEB_NONSTCKREQ," & vbCrLf & _
                " ISA_WEB_MODITEM," & vbCrLf & _
                " ISA_WEB_EMP_BU," & vbCrLf & _
                " ISA_WEB_EMP_USERID" & vbCrLf & _
                " FROM PS_ISA_WEB_LOGIN" & vbCrLf & _
                " WHERE BUSINESS_UNIT = '" & siteid & "'" & vbCrLf & _
                " AND ISA_EMPLOYEE_ID = '" & userid & "'" & vbCrLf

            Try
                Dim dtrPrefixReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
                If dtrPrefixReader.Read() Then
                    currentApp.Session("STOCKREQPRIV") = dtrPrefixReader.Item("ISA_WEB_STOCKREQ")
                    currentApp.Session("NONSTCKREQPRIV") = dtrPrefixReader.Item("ISA_WEB_NONSTCKREQ")
                    currentApp.Session("NEWITEMPRIV") = dtrPrefixReader.Item("ISA_WEB_NEWITEM")
                    currentApp.Session("MODITEMPRIV") = dtrPrefixReader.Item("ISA_WEB_MODITEM")
                    currentApp.Session("AGENTUSERBU") = dtrPrefixReader.Item("ISA_WEB_EMP_BU")
                    If currentApp.Session("AGENTUSERBU") = " " Then
                        currentApp.Session("AGENTUSERBU") = ""
                    End If
                    currentApp.Session("AGENTUSERID") = dtrPrefixReader.Item("ISA_WEB_EMP_USERID")
                    If currentApp.Session("AGENTUSERID") = " " Then
                        currentApp.Session("AGENTUSERID") = ""
                    End If
                Else
                    currentApp.Session("STOCKREQPRIV") = "N"
                    currentApp.Session("NONSTCKREQPRIV") = "N"
                    currentApp.Session("NEWITEMPRIV") = "N"
                    currentApp.Session("MODITEMPRIV") = "N"
                    currentApp.Session("AGENTUSERBU") = ""
                    currentApp.Session("AGENTUSERID") = ""

                End If

                dtrPrefixReader.Close()

            Catch objException As Exception
                'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                'currentApp.Response.Write("<hr>")
                'currentApp.Response.Write("<li>Message: " & objException.Message)
                'currentApp.Response.Write("<li>Source: " & objException.Source)
                'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                'currentApp.Response.End()
                sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLString)
                currentApp.Response.Redirect("DBErrorPage.aspx")

            End Try
        End Sub

        Public Shared Function GetMexicologin(ByVal userName As String, ByVal userPassword As String) As DataSet

            Dim strPasswhash As String = GenerateHash(Trim(userPassword))

            Dim strSQLString As String = "Select A.BUSINESS_UNIT, A.ISA_EMPLOYEE_ID, A.LAST_NAME_SRCH," & vbCrLf & _
                    " ISA_SDI_EMPLOYEE, ISA_EMPLOYEE_ACTYP" & vbCrLf & _
                    " from PS_ISA_USERS_TBL A" & vbCrLf & _
                    " where A.BUSINESS_UNIT = 'SDM00'" & vbCrLf & _
                    " AND A.ISA_EMPLOYEE_ID = '" & userName.ToUpper & "'" & vbCrLf & _
                    " AND A.ISA_PASSWORD_ENCR = '" & strPasswhash & "'" & vbCrLf & _
                    " AND A.ACTIVE_STATUS = 'A'" & vbCrLf

            Dim userds = New DataSet
            userds = ORDBData.GetAdapter(strSQLString)
            Return userds

        End Function

        Public Shared Function getVendorLoc(ByVal strVendorID, ByVal strShipto) As String

            Dim strSQLString As String

            strSQLString = "Select A.ORDER_LOC" & vbCrLf & _
                    " from  SYSADM.PS_SHIPTO_VNDR_LOC A" & vbCrLf & _
                    " where A.SETID = 'MAIN1'" & vbCrLf & _
                    " and A.SHIPTO_ID = '" & strShipto & "'" & vbCrLf & _
                    " AND A.VENDOR_SETID = 'MAIN1'" & vbCrLf & _
                    " AND A.VENDOR_ID = '" & strVendorID & "'"
            getVendorLoc = ORDBData.GetScalar(strSQLString)
            If Trim(getVendorLoc) = "" Then
                getVendorLoc = "0"
            End If

        End Function

        Public Shared Function GetVendorlogin(ByVal userName As String, ByVal userPassword As String) As DataSet

            Dim strUserid As String = "0000000000"
            Dim strPasswhash As String = GenerateHash(Trim(userPassword))

            strUserid = strUserid.Substring(0, 10 - userName.Length) & userName

            Dim strSQLString As String = "Select A.BUSINESS_UNIT, A.ISA_EMPLOYEE_ID, A.LAST_NAME_SRCH" & vbCrLf & _
                    " from PS_ISA_USERS_TBL A, PS_ISA_USERS_PRIVS B" & vbCrLf & _
                    " where A.BUSINESS_UNIT = 'ISA00'" & vbCrLf & _
                    " AND A.ISA_EMPLOYEE_ID = '" & strUserid.ToUpper & "'" & vbCrLf & _
                    " AND A.ISA_PASSWORD_ENCR = '" & strPasswhash & "'" & vbCrLf & _
                    " AND A.ACTIVE_STATUS = 'A'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND B.ISA_IOL_OP_TYPE = 'SUP'" & vbCrLf & _
                    " AND B.ISA_IOL_OP_NAME = 'ASN'" & vbCrLf & _
                    " AND B.ISA_IOL_OP_VALUE = 'Y'"

            Dim userds = New DataSet
            userds = ORDBData.GetAdapter(strSQLString)
            Return userds

        End Function

        Public Shared Sub InsertNewEmp(ByVal strSessionID As String)

            Dim objPunchIN As New clsPunchin(strSessionID)

            Dim strUserGroup As String
            Dim strCustID As String
            Dim strRndCplusPassw As String
            Dim strUserType As String
            Dim strSDICust As String
            Dim strSQLString As String
            Dim strSQLPW As String
            Dim strPasswEncrp As String
            Dim strSQLUPD1 As String
            Dim strSQLUPD2 As String
            Dim strPasswUpdate As String
            Dim dteNow As Date
            dteNow = Now().ToString("d")

            Dim strFirst As String
            If Trim(objPunchIN.FirstName) = "" Then
                strFirst = " "
            Else
                strFirst = Trim(objPunchIN.FirstName)
            End If
            strFirst = Replace(strFirst, "'", "''")

            Dim strLast As String
            If Trim(objPunchIN.LastName) = "" Then
                strLast = " "
            Else
                strLast = Trim(objPunchIN.LastName)
            End If
            strLast = Replace(strLast, "'", "''")

            Dim strFullName As String = strLast & "," & strFirst
            Dim strFullName40 As String = strLast & "," & strFirst
            If strFullName.Length > 50 Then
                strFullName = strFullName.Substring(0, 50)
            End If
            If strFullName40.Length > 40 Then
                strFullName40 = strFullName40.Substring(0, 40)
            End If

            Dim strEmail As String
            If Trim(objPunchIN.Email) = "" Then
                strEmail = "customer.service@sdi.com"
            Else
                strEmail = Trim(objPunchIN.Email)
            End If
            If strEmail.Length > 40 Then
                strEmail = strEmail.Substring(0, 40)
            End If
            If Trim(objPunchIN.CUSTID) = "" Then
                strCustID = " "
            Else
                strCustID = Trim(objPunchIN.CUSTID)
            End If

            Dim strPhone As String
            strPhone = "000-000-0000"
            strPhone = Replace(strPhone, "(", "")
            strPhone = Replace(strPhone, ")", "")
            strPhone = Replace(strPhone, " ", "-")

            Dim strUSERID As String = Trim(objPunchIN.UserAgent).ToUpper
            strUSERID = Replace(strUSERID, "'", "''")

            Dim strBU As String = objPunchIN.BusinessUnit
            Dim strPassword = Trim(objPunchIN.SharedSecret)
            strRndCplusPassw = "4Q23FJQQ72"
            strUserType = "USER"
            strSDICust = "C"
            strPasswEncrp = GenerateHash(strPassword)
            'check to see is record already exists in ISA_USERS_TBL


            strSQLString = "SELECT A.ISA_EMPLOYEE_ID" & vbCrLf & _
                        " FROM PS_ISA_USERS_TBL A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                        " AND A.ISA_EMPLOYEE_ID = '" & strUSERID & "'"
            Dim strUserIDExists As String
            Dim bolError As Boolean = False
            Try
                strUserIDExists = ORDBData.GetScalar(strSQLString)
            Catch ex As Exception
                Exit Sub
            End Try

            Dim rowsaffected As Integer
            If Trim(strUserIDExists) = "" Then
                strSQLString = "INSERT INTO PS_ISA_USERS_TBL" & vbCrLf & _
                                " (ISA_USER_ID, ISA_USER_NAME," & vbCrLf & _
                                " ISA_PASSWORD_ENCR, FIRST_NAME_SRCH," & vbCrLf & _
                                " LAST_NAME_SRCH, BUSINESS_UNIT," & vbCrLf & _
                                " ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME, PHONE_NUM," & vbCrLf & _
                                " ISA_DAILY_ALLOW, ISA_EMPLOYEE_PASSW," & vbCrLf & _
                                " ISA_EMPLOYEE_EMAIL, ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                                " CUST_ID, ISA_SESSION," & vbCrLf & _
                                " ISA_LAST_SYNC_DATE, ISA_SDI_EMPLOYEE, ISA_CUST_SERV_FLG," & vbCrLf & _
                                " LASTUPDOPRID, LASTUPDDTTM, ACTIVE_STATUS)" & vbCrLf & _
                                " VALUES(SEQ_ISA_USER_ID_PK.nextval," & vbCrLf & _
                                " '" & strFullName.ToUpper & "'," & vbCrLf & _
                                " '" & strPasswEncrp & "'," & vbCrLf & _
                                " '" & strFirst.ToUpper & "'," & vbCrLf & _
                                " '" & strLast.ToUpper & "'," & vbCrLf & _
                                " '" & strBU & "'," & vbCrLf & _
                                " '" & strUSERID & "'," & vbCrLf & _
                                " '" & strFullName40.ToUpper & "'," & vbCrLf & _
                                " '" & strPhone & "'," & vbCrLf & _
                                " 0, ' ', '" & strEmail & "'," & vbCrLf & _
                                " '" & strUserType & "'," & vbCrLf & _
                                " '0', 0, '', '" & strSDICust & "', ' '," & vbCrLf & _
                                " 'UpdEmpVB'," & vbCrLf & _
                                " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
                                " 'A')"

                Try
                    rowsaffected = ORDBData.ExecNonQuery(strSQLString)
                Catch ex As Exception
                    Exit Sub
                End Try

                If rowsaffected = 0 Then
                    Exit Sub
                End If

            End If

            'check to see is record already exists in ISA_EMPL_TBL

            strSQLString = "SELECT A.ISA_EMPLOYEE_ID" & vbCrLf & _
                        " FROM PS_ISA_EMPL_TBL A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                        " AND A.ISA_EMPLOYEE_ID = '" & strUSERID & "'"
            Dim strEmplIDExists As String
            Dim bolError2 As Boolean = False
            Try
                strEmplIDExists = ORDBData.GetScalar(strSQLString)
            Catch ex As Exception
                bolError2 = True
            End Try

            If Not bolError2 = True And _
                Trim(strEmplIDExists) = "" Then
                strSQLString = "INSERT INTO PS_ISA_EMPL_TBL" & vbCrLf & _
                                " (BUSINESS_UNIT, ISA_EMPLOYEE_ID," & vbCrLf & _
                                " ISA_EMPLOYEE_NAME, ISA_DAILY_ALLOW," & vbCrLf & _
                                " ISA_EMPLOYEE_PASSW, ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                                " ISA_EMPLOYEE_ACTYP, CUST_ID)" & vbCrLf & _
                                " VALUES('" & strBU & "', '" & strUSERID & "'," & vbCrLf & _
                                " '" & strFullName.ToUpper & "'," & vbCrLf & _
                                " 0.00,' ',' ',' ', '" & strCustID & "')" & vbCrLf

                Try
                    rowsaffected = ORDBData.ExecNonQuery(strSQLString)
                Catch ex As Exception
                    bolError2 = True
                End Try

                If rowsaffected = 0 Then
                    bolError2 = True
                End If

            End If

            If Not bolError = True Then
                strSQLString = "INSERT INTO PS_ISA_ISOL_PW" & vbCrLf & _
                           " (ISA_USER_ID, ISA_EMPLOYEE_ID," & vbCrLf & _
                           " ISA_ISOL_PW1, ISA_ISOL_PW_DATE1," & vbCrLf & _
                           " ISA_ISOL_PW2, ISA_ISOL_PW_DATE2," & vbCrLf & _
                           " ISA_ISOL_PW3, ISA_ISOL_PW_DATE3)" & vbCrLf & _
                           " VALUES (SEQ_ISA_USER_ID_PK.CURRVAL," & vbCrLf & _
                           " '" & strUSERID & "'," & vbCrLf & _
                           " '" & strPasswEncrp & "'," & vbCrLf & _
                           " TO_DATE('" & dteNow & "', 'MM/DD/YYYY')," & vbCrLf & _
                           " ' ', '', ' ','')"

                Try
                    rowsaffected = ORDBData.ExecNonQuery(strSQLString)
                Catch ex As Exception
                    bolError = True
                End Try

                If rowsaffected = 0 Then
                    bolError = True
                End If

            End If

            bolError = False
            'check to see if in the usernames table
            strSQLString = "SELECT USERID" & vbCrLf & _
                        " FROM usernames" & vbCrLf & _
                        " WHERE userID = '" & strUSERID & "'"

            Dim strSQLUserIDExists As String
            Try
                strSQLUserIDExists = SQLDBData.GetSQLScalar(strSQLString)
            Catch ex As Exception
                bolError = True
            End Try

            If Trim(strSQLUserIDExists) = "" Then
                strSQLString = "INSERT INTO usernames" & vbCrLf & _
                                        " (userID, password, lastName, firstName," & vbCrLf & _
                                        " emailAddress, crUser, crDate, updUser, updDate)" & vbCrLf & _
                                        " VALUES ('" & strUSERID & "'," & vbCrLf & _
                                        " '" & strRndCplusPassw & "'," & vbCrLf & _
                                        " '" & strLast & "'," & vbCrLf & _
                                        " '" & strFirst & "'," & vbCrLf & _
                                        " '" & strEmail & "'," & vbCrLf & _
                                        " 'UpdEmpVB'," & vbCrLf & _
                                        " '" & Now() & "'," & vbCrLf & _
                                        " 'UpdEmpVB'," & vbCrLf & _
                                        " '" & Now() & "')"

                Try
                    rowsaffected = SQLDBData.ExecNonQuerySQL(strSQLString)
                Catch ex As Exception
                    bolError = True
                End Try

                If rowsaffected = 0 Then
                    bolError = True
                End If

            End If

            'check to see if in the GroupUsers table
            strSQLString = "SELECT USERID" & vbCrLf & _
                        " FROM GroupUsers" & vbCrLf & _
                        " WHERE userID = '" & strUSERID & "'"

            Dim strSQLGroupUsersExists As String
            Try
                strSQLGroupUsersExists = SQLDBData.GetSQLScalar(strSQLString)
            Catch ex As Exception
                bolError = True
            End Try

            If Not bolError = True And _
                Trim(strSQLGroupUsersExists) = "" Then

                strUserGroup = getUserGroup(strBU)

                strSQLString = "INSERT INTO GroupUsers" & vbCrLf & _
                            " (userID, groupID, crUser, crDate, updUser, updDate)" & vbCrLf & _
                            " VALUES ('" & strUSERID & "'," & vbCrLf & _
                            " " & strUserGroup & "," & vbCrLf & _
                            " 'UpdEmpVB'," & vbCrLf & _
                            " '" & Now() & "'," & vbCrLf & _
                            " 'UpdEmpVB'," & vbCrLf & _
                            " '" & Now() & "')"

                Try
                    rowsaffected = SQLDBData.ExecNonQuerySQL(strSQLString)
                Catch ex As Exception
                    bolError = True
                End Try

                If rowsaffected = 0 Then
                    bolError = True
                End If
            End If

            bolError = False
            'check to see if record already exists in ISA_USERS_PRIVS

            strSQLString = "SELECT A.ISA_EMPLOYEE_ID" & vbCrLf & _
                        " FROM PS_ISA_USERS_PRIVS A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                        " AND A.ISA_EMPLOYEE_ID = '" & strUSERID & "'"
            Dim strUserPrivsExists As String
            Try
                strUserPrivsExists = ORDBData.GetScalar(strSQLString)
            Catch ex As Exception
                bolError = True
            End Try

            If Not bolError = True And _
                Trim(strUserPrivsExists) = "" Then
                Dim X As Integer

                Dim strOpName As String
                For X = 0 To 4
                    Select Case X
                        Case 0
                            strOpName = "ADDCART"
                        Case 1
                            strOpName = "PROFILEUPD"
                        Case 2
                            strOpName = "NONCAT"
                        Case 3
                            strOpName = "NSTKHIST"
                        Case 4
                            strOpName = "ORDSTATUS"
                    End Select
                    strSQLString = "INSERT INTO PS_ISA_USERS_PRIVS" & vbCrLf & _
                        " (ISA_EMPLOYEE_ID," & vbCrLf & _
                        " BUSINESS_UNIT," & vbCrLf & _
                        " ISA_IOL_OP_NAME," & vbCrLf & _
                        " ISA_IOL_OP_VALUE," & vbCrLf & _
                        " ISA_IOL_OP_TYPE," & vbCrLf & _
                        " LASTUPDOPRID," & vbCrLf & _
                        " LASTUPDDTTM)" & vbCrLf & _
                        " VALUES('" & strUSERID.ToUpper & "'," & vbCrLf & _
                        " '" & strBU & "'," & vbCrLf & _
                        " '" & strOpName & "'," & vbCrLf & _
                        " 'Y'," & vbCrLf & _
                        " 'IOL'," & vbCrLf & _
                        " 'UpdEmpPI'," & vbCrLf & _
                        " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))"

                    If Not bolError = True Then
                        Try
                            rowsaffected = ORDBData.ExecNonQuery(strSQLString)
                        Catch ex As Exception
                            bolError = True
                        End Try

                        If rowsaffected = 0 Then
                            bolError = True
                        End If

                    End If

                Next
            End If

        End Sub

        Public Shared Function returnUser(ByVal strSQLString As String, ByVal strSQLString1 As String) As DataSet

            'Gives us a reference to the current asp.net 
            'application executing the method.System.Web.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            System.Diagnostics.Trace.Write(strSQLString)

            Dim UserdataSet As DataSet = ORDBData.GetAdapterSpc(strSQLString)

            currentApp.Context.Trace.Warn(strSQLString)

            If UserdataSet.Tables(0).Rows.Count = 1 Then
                currentApp.Session("USERID") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_ID")
                'currentApp.Session("PASSWORD") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_PASSW")
                currentApp.Session("USERTYPE") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_ACTYP")
                currentApp.Session("BUSUNIT") = UserdataSet.Tables(0).Rows(0)("BUSINESS_UNIT")
                currentApp.Session("SITEID") = Right(currentApp.Session("BUSUNIT"), 3)
                currentApp.Session("USERNAME") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_NAME")
                currentApp.Session("EMAIL") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_EMAIL")
                currentApp.Session("PHONE") = UserdataSet.Tables(0).Rows(0)("PHONE_NUM")
                currentApp.Session("CUSTID") = UserdataSet.Tables(0).Rows(0)("CUST_ID")
                currentApp.Session("PRODVIEW") = UserdataSet.Tables(0).Rows(0)("ISA_CPLUS_PRODVIEW")
                currentApp.Session("CONAME") = UserdataSet.Tables(0).Rows(0)("NAME1")

                Dim hashPrivs As Hashtable
                hashPrivs = New Hashtable
                hashPrivs = getprivhashtable(currentApp.Session("USERID"), currentApp.Session("BUSUNIT"), "Y")
                If hashPrivs.ContainsKey("TESTMODE") Then
                    currentApp.Session("TESTMODE") = "TEST"
                End If
                If UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_ID") = "DEMO" Then
                    currentApp.Session("USERNAME") = "USER NAME"
                    currentApp.Session("CONAME") = Left(UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_NAME"), UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_NAME").length - 2)
                End If
                If UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_ID").length > 3 Then
                    If UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_ID").substring(0, 4).toupper = "TEMP" Then
                        currentApp.Session("USERNAME") = "SDI Temp User"
                        currentApp.Session("CONAME") = Left(UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_NAME"), UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_NAME").length - 2)
                    End If
                End If
                currentApp.Session("SITEEMAIL") = UserdataSet.Tables(0).Rows(0)("ISA_SITE_EMAIL")
                currentApp.Session("SITEPREFIX") = GetSitePrefix1(currentApp.Session("BUSUNIT"))
                currentApp.Session("SITEBU") = GetSiteBU(currentApp.Session("BUSUNIT"))
                If UserdataSet.Tables(0).Rows(0)("ISA_SDI_EMPLOYEE") = "S" Then
                    currentApp.Session("SDIEMP") = "SDI"
                Else
                    currentApp.Session("SDIEMP") = "CUST"
                End If
                GetUserPriv(currentApp.Session("BUSUNIT"), currentApp.Session("USERID"))
                currentApp.Session("LBLPRT") = ""
                'Dim objEnterprise As New clsEnterprise(currentApp.Session("BUSUNIT"))

                'currentApp.Session("APPRTYPE") = objEnterprise.OrdApprType
                'If Trim(currentApp.Session("APPRTYPE")) = "" Then
                '    currentApp.Session("APPRTYPE") = "N"
                'End If
                'currentApp.Session("BUDGETFLG") = objEnterprise.OrdBudgetFlg
                'If Trim(currentApp.Session("BUDGETFLG")) = "" Then
                '    currentApp.Session("BUDGETFLG") = "N"
                'End If
                ''currentApp.Session("NSTKONLY") = objEnterprise.ApprNSTKFlag
                'If Trim(currentApp.Session("NSTKONLY")) = "" Then
                '    currentApp.Session("NSTKONLY") = "N"
                'End If
                'currentApp.Session("EmpChgCd") = GetEmpChgCd(currentApp.Session("BUSUNIT"))
                'currentApp.Session("RecvDate") = objEnterprise.ReceivingDate
                'currentApp.Session("COMPANYID") = objEnterprise.CompanyID
                'currentApp.Session("ITEMMODE") = objEnterprise.ItemMode
                ' currentApp.Session("SHOPPAGE") = objEnterprise.ShopCartPage
                ' currentApp.Session("LPPFLAG") = objEnterprise.LPPFlag
                'currentApp.Session("PUNCHOUT") = objEnterprise.ISOLPunchout

                Dim objSDRBULOC As New clsSDRBULOC(currentApp.Session("BUSUNIT"), "BU")
                currentApp.Session("SmallSite") = objSDRBULOC.SmallSiteFlag
                ' currentApp.Session("MobIssuing") = objEnterprise.MobileIssuing
                CheckLabel()
                'the catalog ID GetCplusDB
                GetCplusDB(currentApp.Session("PRODVIEW"))
                'currentApp.Session("CPVersion") = getSiteVer(currentApp.Session("PRODVIEW"))

            ElseIf Not strSQLString1 = "" Then
                UserdataSet.Clear()
                UserdataSet = ORDBData.GetAdapterSpc(strSQLString1)
                If UserdataSet.Tables(0).Rows.Count = 1 Then
                    currentApp.Session("USERID") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_ID")
                    currentApp.Session("USERTYPE") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_ACTYP")
                    currentApp.Session("BUSUNIT") = UserdataSet.Tables(0).Rows(0)("BUSINESS_UNIT")
                    currentApp.Session("SITEID") = Left(currentApp.Session("BUSUNIT"), 3)
                    currentApp.Session("USERNAME") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_NAME")
                    currentApp.Session("EMAIL") = UserdataSet.Tables(0).Rows(0)("ISA_EMPLOYEE_EMAIL")
                    currentApp.Session("PHONE") = UserdataSet.Tables(0).Rows(0)("PHONE_NUM")
                    currentApp.Session("CUSTID") = "AGENT"
                    currentApp.Session("PRODVIEW") = 0
                    currentApp.Session("CONAME") = "CSC Agent"
                    currentApp.Session("SITEBU") = "ISA00"
                    currentApp.Session("SITEEMAIL") = "Insiteonline@SDI.com"
                    currentApp.Session("SITEPREFIX") = "SDI"
                    currentApp.Session("SDIEMP") = "SDI"

                    GetCplusDB(currentApp.Session("PRODVIEW"))
                End If
            End If
            Return UserdataSet

        End Function

        Public Shared Sub sendCostEmail(ByVal strSupplier As String, _
                                        ByVal strBU As String, _
                                        ByVal strAgent As String, _
                                        ByVal strUserName As String, _
                                        ByVal decMoney As Decimal, _
                                        ByVal decCostWmarkup As Decimal, _
                                        ByVal decCatalogPrice As Decimal, _
                                        ByVal decCostSDI As Decimal, _
                                        ByVal dr As DataRow)

            Dim strSQLString As String

            Dim strCostShoppingCart As String
            Dim strbodyhead As String
            Dim strbodydetl As String
            Dim txtBody As String
            Dim txtHdr As String
            Dim txtMsg As String
            Dim bolCostGreater As Boolean

            If decCatalogPrice > 0 And decCostSDI > decCatalogPrice Then
                bolCostGreater = True
            ElseIf decCostSDI > decMoney Or decCostSDI = 0 Then
                bolCostGreater = True
                'strCostShoppingCart = "Call for Price"
            ElseIf decCostWmarkup > decMoney Or decCostWmarkup = 0 Then
                bolCostGreater = True
                'strCostShoppingCart = decMoney
            Else
                bolCostGreater = False
            End If
            strCostShoppingCart = dr(4)

            Dim Mailer = New MailMessage
            Dim strccfirst As String = "pete.doyle"
            Dim strcclast As String = "sdi.com"
            Mailer.From = "Insiteonline@SDI.com"
            Mailer.cc = ""
            Mailer.to = strccfirst & "@" & strcclast
            strbodyhead = "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
            If bolCostGreater = True Then
                strbodyhead = strbodyhead & "<center><span >In-Site&reg; Online - Punchout Catalog Price Compare</span></center>"
            Else
                strbodyhead = strbodyhead & "<center><span >In-Site&reg; Online - Punchout Catalog Item Selected</span></center>"
            End If
            strbodyhead = strbodyhead & "&nbsp;" & vbCrLf
            If bolCostGreater = True Then
                strbodyhead = strbodyhead & "<DIV style='DISPLAY: inline; COLOR: red;'>** WARNING**</DIV>" & vbCrLf
            End If
            strbodydetl = "&nbsp;" & vbCrLf
            strbodydetl = strbodydetl & "<div>"
            strbodydetl = strbodydetl & "<p >TO:<span>      </span>" & strccfirst & "@" & strcclast & "<br>"
            strbodydetl = strbodydetl & "&nbsp;<BR>"
            strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "&nbsp;</p>"
            If bolCostGreater = True Then
                strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The following Punchout item from " & strSupplier & " has an SDI + markup "
                strbodydetl = strbodydetl & "price higher than the catalog price:<BR>"
            Else
                strbodydetl = strbodydetl & "<p style='text-indent:.5in'>The following Punchout item from " & strSupplier & " was selected. "
                strbodydetl = strbodydetl & ":<BR>"
            End If
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "Business Unit = " & strBU & "<br>"
            strbodydetl = strbodydetl & "Employee = " & strAgent & " - " & strUserName & "<br>"
            strbodydetl = strbodydetl & "List price = " & decMoney & "<br>"
            strbodydetl = strbodydetl & "SDI Cost = " & decCostSDI & "<br>"
            strbodydetl = strbodydetl & "SDI Cost & Markup = " & decCostWmarkup & "<br>"
            strbodydetl = strbodydetl & "Catalog Price = " & decCatalogPrice & "<br>"
            strbodydetl = strbodydetl & "Shopping Cart Cost = " & strCostShoppingCart & "<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "<B>Description = </B>" & dr(2) & "<br>"
            strbodydetl = strbodydetl & "<B>Supplier ID = </B>" & dr(7) & "<br>"
            strbodydetl = strbodydetl & "<B>Manufacturer = </B>" & dr(8) & "<br>"
            strbodydetl = strbodydetl & "<B>MFG Part Number = </B>" & dr(9) & "<br>"
            strbodydetl = strbodydetl & "<B>UOM = </B>" & dr(10) & "<br>"
            strbodydetl = strbodydetl & "<B>Stock Type = </B>" & dr(5) & "<br>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "</p>"
            strbodydetl = strbodydetl & "</div>"

            Mailer.body = strbodyhead & strbodydetl

            If bolCostGreater = True Then
                Mailer.Subject = "**Warning** - Punchout SDI Cost > Catalog Cost"
            Else
                Mailer.Subject = "Punchout Catalog Item Selected"
            End If

            Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
            'SmtpMail.Send(Mailer)
            sendemail(Mailer)

        End Sub

        Public Shared Sub sendemail(ByVal mailer As MailMessage)

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim connectionEmail = New OleDbConnection(DbUrl)
            Try
                UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, _
                                                mailer.From, _
                                                mailer.To, _
                                                mailer.Cc, _
                                                mailer.Bcc, _
                                                "N", _
                                                mailer.Body, _
                                                connectionEmail)
            Catch ex As Exception
                currentApp.Response.Write(("The following exception occurred: " + ex.ToString()))
                sendErrorEmail(ex.ToString, "NO", currentApp.Request.ServerVariables("URL"), "sendemail")
                'check the InnerException
                While Not (ex.InnerException Is Nothing)
                    currentApp.Response.Write("--------------------------------")
                    currentApp.Response.Write(("The following InnerException reported: " + ex.InnerException.ToString()))
                    ex = ex.InnerException
                End While
            End Try
        End Sub

        Public Shared Sub sendErrorEmail(ByVal strMessage As String, ByVal strMobile As String, ByVal strURL As String, ByVal strSQL As String)

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            Dim Mailer = New MailMessage
            Dim strccfirst As String = "pete.doyle"
            Dim strccfirst2 As String = "3025591705"
            Dim strcclast As String = "sdi.com"
            Dim strcclast2 As String = "vtext.com"
            Mailer.From = "Insiteonline@SDI.com"
            Mailer.to = strccfirst & "@" & strcclast & "; pete.doyle@sdi.com;"
            Mailer.subject = strURL
            If strMobile = "YES" And _
                Not Trim(currentApp.Request.ServerVariables("remote_addr")) = "" And _
                Not currentApp.Request.ServerVariables("remote_addr") = "65.220.75.12" And _
                Not currentApp.Request.ServerVariables("remote_addr") = "127.0.0.1" And _
                Mailer.cc = strccfirst2 & "@" & strcclast2 Then
            End If
            Dim strSQLString As String

            Dim strbodydetl As String
            strbodydetl = strbodydetl & "<div>"
            strbodydetl = strbodydetl & "<p >" & strMessage & "&nbsp;" & currentApp.Request.ServerVariables("remote_addr") & _
                                    "&nbsp;" & currentApp.Session("USERID") & "&nbsp;" & strSQL
            strbodydetl = strbodydetl & "&nbsp;<BR>"
            strbodydetl = strbodydetl & "Date:<span>    </span>" & Now() & "<BR>"
            strbodydetl = strbodydetl & "&nbsp;<br>"
            strbodydetl = strbodydetl & "&nbsp;</p>"
            strbodydetl = strbodydetl & "</div>"

            Mailer.body = strbodydetl

            Mailer.BodyFormat = System.Web.Mail.MailFormat.Html
            'SmtpMail.Send(Mailer)
            sendemail(Mailer)

            ' just added this
            '   erwin 2008.08.21
            Try
                Dim src As System.Web.Mail.MailMessage = CType(Mailer, System.Web.Mail.MailMessage)
                Dim mail As New System.Web.Mail.MailMessage
                With mail
                    .From = src.From
                    .To = "pete.doyle@sdi.com;"
                    .Subject = src.Subject
                    .Body = src.Body
                    .BodyFormat = src.BodyFormat
                End With
                System.Web.Mail.SmtpMail.Send(mail)
                mail = Nothing
                src = Nothing
            Catch ex As Exception
                ' ignore
            End Try

        End Sub

        Public Shared Sub setServer()
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            If currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "LOCALHOST" Then
                'currentApp.Session("cplusServer") = "http://contentplus.isacs.com:8080/"
                currentApp.Session("cplusServer") = "http://199.117.166.50:8080/"
                'currentApp.Session("cplusServer") = "http://cptest.sdi.com:8085/"
                currentApp.Session("IOLServer") = "http://localhost/insiteonline/"
            ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST" Then
                currentApp.Session("cplusServer") = "http://cptest:8085/"
                currentApp.Session("IOLServer") = "http://cptest/insiteonline/"
            ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "DEXTEST4" Then
                currentApp.Session("cplusServer") = "http://contentplus.isacs.com:8080/"
                currentApp.Session("IOLServer") = "http://dextest4/insiteonline/"
            ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST.SDI.COM" Then
                'currentApp.Session("cplusServer") = "http://cptest.sdi.com:8085/"
                'currentApp.Session("cplusServer") = "http://contentplus.isacs.com:8080/"
                currentApp.Session("cplusServer") = "http://199.117.166.50:8080/"
                currentApp.Session("IOLServer") = "http://cptest.sdi.com/insiteonline/"
            ElseIf currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST.ISACS.COM" Then
                currentApp.Session("cplusServer") = "http://cptest.sdi.com:8085/"
                currentApp.Session("IOLServer") = "http://cptest.isacs.com/insiteonline/"
            Else
                currentApp.Session("cplusServer") = "http://contentplus.isacs.com:8080/"
                currentApp.Session("IOLServer") = "http://" & currentApp.Request.ServerVariables("HTTP_HOST") & "/"
            End If
        End Sub
        Public Shared Function initParms(ByVal parm() As String, _
                                                 ByVal anum As Integer)
            For i As Integer = 0 To (anum - 1)
                If parm(i) = "" Then
                    parm(i) = " "
                End If
            Next
            Return parm
        End Function

        Public Shared Function UpdateBudgetAppr(ByVal strbu As String, ByVal strNSTKreqid As String, ByVal strApprvBy As String) As Boolean

            Dim strSQLstring As String
            Dim strChargeCodes As String
            Dim rowsaffected As Integer
            Dim strMoreApprovals As String
            Dim objChargeCodes As New clsCustBudget(strApprvBy, strbu)
            strChargeCodes = objChargeCodes.ChargeCodes

            strSQLstring = "UPDATE PS_ISA_BUDGET_APPR" & vbCrLf & _
                            " SET OPRID_APPROVED_BY = '" & strApprvBy & "'" & vbCrLf & _
                            " WHERE BUSINESS_UNIT_OM = '" & strbu & "'" & vbCrLf & _
                            " AND ORDER_NO = '" & strNSTKreqid & "'" & vbCrLf & _
                            " AND ISA_CUST_CHARGE_CD IN (" & strChargeCodes & ")"

            rowsaffected = ExecNonQuery(strSQLstring)
            'If rowsaffected = 0 Then
            '    Exit Function
            'End If

            strSQLstring = "SELECT DISTINCT(ISA_CUST_CHARGE_CD)" & vbCrLf & _
                            " FROM PS_ISA_BUDGET_APPR" & vbCrLf & _
                            " WHERE BUSINESS_UNIT_OM = '" & strbu & "'" & vbCrLf & _
                            " AND ORDER_NO = '" & strNSTKreqid & "'" & vbCrLf & _
                            " AND OPRID_APPROVED_BY = ' '"

            strMoreApprovals = ORDBData.GetScalar(strSQLstring)
            If strMoreApprovals = "" Then
                UpdateBudgetAppr = False
            Else
                UpdateBudgetAppr = True
            End If

        End Function

        Public Shared Sub updateOrderStatus(ByVal strbu As String, ByVal strNSTKreqid As String, ByVal strApprvBy As String)

            Dim rowsaffected As Integer
            Dim strSQLstring As String
            Dim strLineStatus As String

            strSQLstring = "SELECT B.REQ_STATUS" & vbCrLf & _
                            " FROM PS_ISA_ORD_INTFC_H A, PS_REQ_HDR B" & vbCrLf & _
                            " WHERE A.BUSINESS_UNIT_OM = '" & strbu & "'" & vbCrLf & _
                            " AND A.ORDER_NO = '" & strNSTKreqid & "'" & vbCrLf & _
                            " AND A.ORDER_NO = B.REQ_ID(+)" & vbCrLf

            strLineStatus = ORDBData.GetScalar(strSQLstring)

            '" AND B.REQ_STATUS(+) = 'Q'"

            If strLineStatus = "" Then
                strLineStatus = "1"
            Else
                strLineStatus = "Q"
            End If

            strSQLstring = "UPDATE SYSADM.PS_ISA_ORD_INTFC_L" & vbCrLf & _
                            " SET ISA_ORDER_STATUS = '" & strLineStatus & "'" & vbCrLf & _
                            " WHERE ISA_PARENT_IDENT =" & vbCrLf & _
                            " (SELECT A.ISA_IDENTIFIER" & vbCrLf & _
                            " FROM PS_ISA_ORD_INTFC_H A" & vbCrLf & _
                            " WHERE(A.BUSINESS_UNIT_OM = '" & strbu & "')" & vbCrLf & _
                            " AND A.ORDER_NO = '" & strNSTKreqid & "')" & vbCrLf & _
                            " AND ISA_ORDER_STATUS IN ('1','2','Q','W','B')"

            rowsaffected = ExecNonQuery(strSQLstring)
            If rowsaffected = 0 Then
                Exit Sub
            End If
            'Dim objEnterpriseTbl As New clsEnterprise(strbu)
            'Dim strLastUPDOprid As String = objEnterpriseTbl.LastUPDOprid

            'If strLastUPDOprid = "XXXXXXXX" Then
            '    strSQLstring = "UPDATE PS_ISA_ORD_INTFC_H" & vbCrLf & _
            '                " SET ORDER_STATUS = 'X'," & vbCrLf & _
            '                " OPRID_APPROVED_BY = '" & strApprvBy & "'" & vbCrLf & _
            '                " WHERE BUSINESS_UNIT_OM = '" & strbu & "'" & vbCrLf & _
            '                " AND ORDER_NO = '" & strNSTKreqid & "'" & vbCrLf
            'Else
            '    strSQLstring = "UPDATE PS_ISA_ORD_INTFC_H" & vbCrLf & _
            '                " SET ORDER_STATUS = 'P'," & vbCrLf & _
            '                " OPRID_APPROVED_BY = '" & strApprvBy & "'" & vbCrLf & _
            '                " WHERE BUSINESS_UNIT_OM = '" & strbu & "'" & vbCrLf & _
            '                " AND ORDER_NO = '" & strNSTKreqid & "'" & vbCrLf
            'End If

            rowsaffected = ExecNonQuery(strSQLstring)
            If rowsaffected = 0 Then
                Exit Sub
            End If

        End Sub

        Public Shared Sub updateReqDueDT(ByVal strBU As String, ByVal strReqID As String, ByVal intLineNbr As Integer, ByVal dteDueDT As DateTime)

            Dim strSQLstring As String
            Dim dteDueDty As String = dteDueDT.ToString("yyyy-M-d")

            strSQLstring = "UPDATE PS_REQ_LINE_SHIP" & vbCrLf & _
                    " SET DUE_DT = TO_DATE('" & dteDueDty & "', 'YYYY/MM/DD')" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND REQ_ID = '" & strReqID & "'" & vbCrLf & _
                    " AND LINE_NBR = " & intLineNbr & vbCrLf

            Dim rowsaffected As Integer = ExecNonQuery(strSQLstring)
            If rowsaffected = 0 Then
                Exit Sub
            End If

        End Sub

        Public Shared Sub WebLog()

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            If currentApp.Session("USERID") = "" Then
                currentApp.Session("USERID") = "EMPTY"
            End If

            If IsDBNull(currentApp.Session("BUSUNIT")) Or _
                currentApp.Session("BUSUNIT") Is Nothing Then
                Exit Sub
            End If

            'Dim strSQLstring As String = "INSERT INTO ps_isa_web_log" & vbCrLf & _
            '        " ( BUSINESS_UNIT, ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME," & vbCrLf & _
            '        " ISA_SDI_EMPLOYEE, ISA_LOG_PAGE, ISA_LOG_URL," & vbCrLf & _
            '        " ISA_LOG_IP, ISA_LOG_BROWSER, ISA_LOG_COOKIES," & vbCrLf & _
            '        " ISA_LOG_JAVASCRIPT, ISA_LOG_JAVAAPPLET," & vbCrLf & _
            '        " ISA_LOG_VBSCRIPT, ISA_LOG_ACTIVEX," & vbCrLf & _
            '        " ISA_LOG_PLATFORM, DT_TIMESTAMP )" & vbCrLf & _
            '        " VALUES ( '" & currentApp.Session("BUSUNIT") & "'," & vbCrLf & _
            '        " '" & currentApp.Session("USERID") & "'," & vbCrLf & _
            '        " '" & currentApp.Session("USERNAME") & "'," & vbCrLf & _
            '        " '" & Left(currentApp.Session("SDIEMP"), 1) & "'," & vbCrLf & _
            '        " '" & Left(currentApp.Request.ServerVariables("SCRIPT_NAME"), 60) & "'," & vbCrLf & _
            '        " '" & Left((currentApp.Request.ServerVariables("HTTP_HOST") & currentApp.Request.ServerVariables("SCRIPT_NAME")), 100) & "'," & vbCrLf & _
            '        " '" & currentApp.Request.ServerVariables("REMOTE_ADDR") & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Browser & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Cookies & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.JavaScript & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.JavaApplets & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.VBScript & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.ActiveXControls & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Platform & "'," & vbCrLf & _
            '        " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))"

            'Dim rowsaffected As Integer

            'Try
            '    Dim connection = New OleDbConnection(DbUrl)
            '    Dim Command = New OleDbCommand(strSQLstring, connection)
            '    connection.open()
            '    rowsaffected = Command.ExecuteNonQuery()
            '    connection.close()
            'Catch objException As Exception
            '    'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            '    'currentApp.Response.Write("<hr>")
            '    'currentApp.Response.Write("<li>Message: " & objException.Message)
            '    'currentApp.Response.Write("<li>Source: " & objException.Source)
            '    'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            '    'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            '    'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            '    'currentApp.Response.End()
            'End Try

            'Sleep(1000)

        End Sub

        Public Shared Sub WebLogEplus(ByVal strurl As String)

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            If currentApp.Session("USERID") = "" Then
                currentApp.Session("USERID") = "EMPTY"
            End If

            If IsDBNull(currentApp.Session("BUSUNIT")) Or _
                currentApp.Session("BUSUNIT") Is Nothing Then
                Exit Sub
            End If

            'Dim strSQLstring As String = "INSERT INTO ps_isa_web_log" & vbCrLf & _
            '        " ( BUSINESS_UNIT, ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME," & vbCrLf & _
            '        " ISA_SDI_EMPLOYEE, ISA_LOG_PAGE, ISA_LOG_URL," & vbCrLf & _
            '        " ISA_LOG_IP, ISA_LOG_BROWSER, ISA_LOG_COOKIES," & vbCrLf & _
            '        " ISA_LOG_JAVASCRIPT, ISA_LOG_JAVAAPPLET," & vbCrLf & _
            '        " ISA_LOG_VBSCRIPT, ISA_LOG_ACTIVEX," & vbCrLf & _
            '        " ISA_LOG_PLATFORM, DT_TIMESTAMP )" & vbCrLf & _
            '        " VALUES ( '" & currentApp.Session("BUSUNIT") & "'," & vbCrLf & _
            '        " '" & currentApp.Session("USERID") & "'," & vbCrLf & _
            '        " '" & currentApp.Session("USERNAME") & "'," & vbCrLf & _
            '        " '" & Left(currentApp.Session("SDIEMP"), 1) & "'," & vbCrLf & _
            '        " '" & Left(strurl, 60) & "'," & vbCrLf & _
            '        " '" & Left((currentApp.Session("cplusServer") & "/" & currentApp.Request.ServerVariables("SCRIPT_NAME")), 100) & "'," & vbCrLf & _
            '        " '" & currentApp.Request.ServerVariables("REMOTE_ADDR") & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Browser & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Cookies & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.JavaScript & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.JavaApplets & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.VBScript & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.ActiveXControls & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Platform & "'," & vbCrLf & _
            '        " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))"

            'Dim rowsaffected As Integer

            'Try
            '    Dim connection = New OleDbConnection(DbUrl)
            '    Dim Command = New OleDbCommand(strSQLstring, connection)
            '    connection.open()
            '    rowsaffected = Command.ExecuteNonQuery()
            '    connection.close()
            'Catch objException As Exception
            '    'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            '    'currentApp.Response.Write("<hr>")
            '    'currentApp.Response.Write("<li>Message: " & objException.Message)
            '    'currentApp.Response.Write("<li>Source: " & objException.Source)
            '    'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            '    'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            '    'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            '    'currentApp.Response.End()
            'End Try

            'Sleep(1000)

        End Sub

    End Class

End Namespace
