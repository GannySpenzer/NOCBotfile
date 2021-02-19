Imports System.Data.OleDb
Imports Insiteonline.ORDBData

Imports Insiteonline.VoucherSharedFunctions.VoucherClass
Imports Telerik.Web.UI
Imports Insiteonline.clsPODetails
Imports System 'gez 7/13/2012
Imports System.Data 'gez 7/13/2012
Imports System.Text
'Imports Insiteonlinevendor

Public Class VoucherInfo2_brt
    Inherits System.Web.UI.Page
    Private m_pathSQLs As String = ""
    Private m_vendorInfo As Vendor = Nothing
    Private m_selectedPO As PO = Nothing
    Private m_voucher As Voucher = Nothing
    Private m_oledbCNString As String = ConfigurationSettings.AppSettings("OLEDBconString")
    Private m_currentMiscChargeList As New ArrayList

#Region " Web Form Designer Generated Code "

    ' Dim Statgrid8 As Object

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub
    'Protected WithEvents lblSelectRpt As System.Web.UI.WebControls.Label
    'Protected WithEvents dropSelectReport As System.Web.UI.WebControls.DropDownList
    'Protected WithEvents btnExcel As System.Web.UI.WebControls.Button
    'Protected WithEvents txtSortField As System.Web.UI.WebControls.TextBox
    'Protected WithEvents txtFromDate As System.Web.UI.WebControls.TextBox
    'Protected WithEvents txtToDate As System.Web.UI.WebControls.TextBox
    'Protected WithEvents Label1 As System.Web.UI.WebControls.Label
    'Protected WithEvents txtSearchFrom As System.Web.UI.WebControls.TextBox
    'Protected WithEvents lblErrFromDate As System.Web.UI.WebControls.Label
    'Protected WithEvents Label2 As System.Web.UI.WebControls.Label
    'Protected WithEvents txtSearchTo As System.Web.UI.WebControls.TextBox
    'Protected WithEvents lblErrToDate As System.Web.UI.WebControls.Label
    'Protected WithEvents btnSubmit As System.Web.UI.WebControls.Button
    'Protected WithEvents lblErrdate As System.Web.UI.WebControls.Label
    'Protected WithEvents Panel1 As System.Web.UI.WebControls.Panel
    'Protected WithEvents lblOR As System.Web.UI.WebControls.Label
    'Protected WithEvents Label3 As System.Web.UI.WebControls.Label
    'Protected WithEvents dropSearchBy As System.Web.UI.WebControls.DropDownList
    'Protected WithEvents Label4 As System.Web.UI.WebControls.Label
    'Protected WithEvents txtSearchCriteria As System.Web.UI.WebControls.TextBox
    'Protected WithEvents btnSubmit2 As System.Web.UI.WebControls.Button
    'Protected WithEvents lblErrCriteria As System.Web.UI.WebControls.Label
    'Protected WithEvents Panel2 As System.Web.UI.WebControls.Panel
    'Protected WithEvents lblNoData As System.Web.UI.WebControls.Label
    'Protected WithEvents lblNoPOS As System.Web.UI.WebControls.Label
    'Protected WithEvents Statgrid As System.Web.UI.WebControls.DataGrid
    'Protected WithEvents chbDateRange As System.Web.UI.WebControls.CheckBox
    'Protected WithEvents lblLoadData As System.Web.UI.WebControls.Label
    'Protected WithEvents ltlAlert As System.Web.UI.WebControls.Literal
    'Protected WithEvents chbNoLimit As System.Web.UI.WebControls.CheckBox
    'Protected WithEvents btnGo As System.Web.UI.WebControls.Button
    'Protected WithEvents chbFreeze As System.Web.UI.WebControls.CheckBox
    ' Protected WithEvents Panel3 As System.Web.UI.WebControls.Panel
    'Protected WithEvents chkPaidVoucher As System.Web.UI.WebControls.CheckBox
    'Protected WithEvents chkunpaidVoucher As System.Web.UI.WebControls.CheckBox
    'Protected WithEvents textbox1 As System.Web.UI.WebControls.TextBox 'gez 8/8/2012
    'Protected WithEvents textbox2 As System.Web.UI.WebControls.TextBox 'gez 8/10/2012
    'Protected WithEvents textbox3 As System.Web.UI.WebControls.TextBox 'gez 8/10/2012
    'Protected WithEvents textbox4 As System.Web.UI.WebControls.TextBox 'gez 8/10/2012
    'Protected WithEvents textbox5 As System.Web.UI.WebControls.TextBox 'gez 8/10/2012
    'NOTE: The following placeholder declaration is required by the Web Form Designer.
    ' Protected WithEvents ltlAlert As System.Web.UI.WebControls.Literal
    'Do not delete or move it.
    Private designerPlaceholderDeclaration As System.Object

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Dim strSitePre As String
    Dim strCoName As String
    Dim strHome As String
    Dim hashStatus As Hashtable
    Dim slither As Integer = 0

    'global variables 'gez 8/16/2012
    Dim BRTinvoiceNumber As String
    Dim DateBegin As String
    Dim DateEnd As String
    Dim ImageURL As String
    Dim strname As String
    Dim ColumnToImageService As String
    Dim PaymentDateTime As Boolean = False
    Protected Overrides Sub OnPreInit(ByVal e As EventArgs)

        MyBase.OnPreInit(e)

        'MasterPageFile = "~/MasterPage/SDIXMaster.Master"
        ' MasterPageFile = "~/MasterPage/ISOLOnlineVendor.Master"

    End Sub

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Call Insiteonline.WebPartnerFunctions.WebPSharedFunc.CheckVendorRedirect()

        Session("SCREENNAME") = "VoucherInfo2_brt.aspx"
        Me.Title = "Supplier Invoice Info"

        Dim lblTitle As Label = CType(Master.FindControl("lblVendor"), Label)
        lblTitle.Text = "Invoice Info for " & "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"

        Session("SiteId21") = "I00"
        If Session("SiteId21") Is Nothing Then  '  If Request.QueryString("SITEID") Is Nothing Then
            lblNoPOS.Visible = True
            lblNoPOS.Text = "Siteid parameter is missing."
            lblLoadData.Visible = False
            Panel3.Visible = False
            Exit Sub
        End If

        If Not Page.IsPostBack Then

            setFromToDates()

            If Not Page.IsPostBack Then
                Insiteonline.WebPartnerFunctions.WebPSharedFunc.WebLogVendor()
            End If

            'WebLog("ISA00")

            Session("BUSUNIT_VI2_BRT") = Session("bu_VP")  '  "ISA00"
            If Session("BUSUNIT_VI2_BRT") Is Nothing Then
                Session("BUSUNIT_VI2_BRT") = "ISA00"
            End If
            'Session("USERID_VI2_BRT") = Session("VENDOR_VP")
            dropSelectReport.SelectedIndex = 99 '0

            txtSortField.Text = "PYMNT_ID"
            btnExcel.Visible = False
            chbFreeze.Visible = False

            lblLoadData.Style.Add("visibility", "hidden")
            lblNoData.Visible = False

        End If
        'dropSelectReport.Attributes.Add("onchange", "blinkLoading();")
        btnSubmit.Attributes.Add("onclick", "blinkLoading();")
        btnSubmit2.Attributes.Add("onclick", "blinkLoading();")
        btnGo.Attributes.Add("onclick", "blinkLoading();")
        btnResetDate.Attributes.Add("onclick", "blinkLoading();")
        BRTinvoiceNumber = ""

    End Sub

    Private Sub btnSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSubmit.Click

        'lblErrFromDate.Visible = False
        'lblErrToDate.Visible = False
        lblErrdate.Visible = False
        lblErrCriteria.Visible = False

        Dim bolDateErr As Boolean

        Dim fromDateTimeUS As System.DateTime
        Dim toDateTimeUS As System.DateTime
        Dim format As New System.Globalization.CultureInfo("en-US", True)
        Try
            'fromDateTimeUS = System.DateTime.Parse(txtSearchFrom.Text, format)
            fromDateTimeUS = System.DateTime.Parse(RadDatePickerFrom.SelectedDate, format)
        Catch ex As Exception
            'lblErrFromDate.Visible = True
            'lblErrFromDate.Text = "Invalid from date format"
            bolDateErr = True
        End Try
        Try
            'toDateTimeUS = System.DateTime.Parse(txtSearchTo.Text, format)
            toDateTimeUS = System.DateTime.Parse(RadDatePickerTo.SelectedDate, format)
        Catch ex As Exception
            'lblErrToDate.Visible = True
            'lblErrToDate.Text = "Invalid to date format"
            bolDateErr = True
        End Try
        If bolDateErr = True Then
            Exit Sub
        End If
        If DateTime.Compare(fromDateTimeUS, toDateTimeUS) > 0 Then
            lblErrdate.Visible = True
            lblErrdate.Text = "From date is greater than To date"
            Exit Sub
        End If
        If DateTime.Compare(fromDateTimeUS.AddMonths(3), toDateTimeUS) < 0 Then
            lblErrdate.Visible = True
            lblErrdate.Text = "Date range exceeds 3 months"
            Exit Sub
        End If

        'txtFromDate.Text = Convert.ToDateTime(txtSearchFrom.Text).ToString("d")
        'txtToDate.Text = Convert.ToDateTime(txtSearchTo.Text).ToString("d")
        txtFromDate.Text = Convert.ToDateTime(RadDatePickerFrom.SelectedDate).ToString("d")
        txtToDate.Text = Convert.ToDateTime(RadDatePickerTo.SelectedDate).ToString("d")

        'DateBegin = txtSearchFrom.Text 'gez 7/20/2012 'Date for BRT data
        'DateEnd = txtSearchTo.Text 'gez 7/20/2012 BRT 'Date for BRT data
        DateBegin = Convert.ToDateTime(RadDatePickerFrom.SelectedDate).ToString("d")
        DateEnd = Convert.ToDateTime(RadDatePickerTo.SelectedDate).ToString("d")

        txtSearchCriteria.Text = "search by date"

        BindDataGrid()
        dropSelectReport.SelectedIndex = 0
        'HideSearchParams()

    End Sub

    Private Sub btnSubmit2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSubmit2.Click
        'lblErrFromDate.Visible = False
        'lblErrToDate.Visible = False

        lblErrCriteria.Visible = False
        PaymentDateTime = False
        lblErr.Visible = False
        lblErrdate.Visible = False
        'If Trim(txtSearchCriteria.Text) = "" Then
        '    lblErrCriteria.Visible = True
        '    lblErrCriteria.Text = "No search criteria has been entered"
        '    Exit Sub
        'End If

        If (txtPOSearch.Text.Trim <> "" Or txtSearchCriteria.Text.Trim <> "") And RadDatePickerFrom.SelectedDate Is Nothing And RadDatePickerTo.SelectedDate Is Nothing And RadPaymentFrom.SelectedDate Is Nothing And RadPaymentTo.SelectedDate Is Nothing Then
            chbDateRange.Checked = False
        Else
            chbDateRange.Checked = True
        End If

        Dim dteNow As Date
        dteNow = Now().ToString("d")
        If chbDateRange.Checked = True Then ' selects by date and criteria
            Dim bolDateErr As Boolean
            Dim bolPaymentDateErr As Boolean
            Dim fromDateTimeUS As System.DateTime
            Dim toDateTimeUS As System.DateTime
            Dim fromPaymentDateTimeUS As System.DateTime
            Dim toPaymentDateTimeUS As System.DateTime
            Dim format As New System.Globalization.CultureInfo("en-US", True)
            Try
                'fromDateTimeUS = System.DateTime.Parse(txtSearchFrom.Text, format)
                fromDateTimeUS = System.DateTime.Parse(RadDatePickerFrom.SelectedDate, format)
            Catch ex As Exception
                lblErr.Visible = True
                lblErr.Text = "Invalid date format"
                bolDateErr = True
            End Try
            Try
                'toDateTimeUS = System.DateTime.Parse(txtSearchTo.Text, format)
                toDateTimeUS = System.DateTime.Parse(RadDatePickerTo.SelectedDate, format)
            Catch ex As Exception
                lblErr.Visible = True
                lblErr.Text = "Invalid date format"
                bolDateErr = True
            End Try
            If bolDateErr = True Then
                Try
                    fromPaymentDateTimeUS = System.DateTime.Parse(RadPaymentFrom.SelectedDate, format)
                Catch ex As Exception
                    lblErr.Visible = True
                    lblErr.Text = "Invalid date format"
                    bolPaymentDateErr = True
                End Try
                Try
                    toPaymentDateTimeUS = System.DateTime.Parse(RadPaymentTo.SelectedDate, format)
                Catch ex As Exception
                    lblErr.Visible = True
                    lblErr.Text = "Invalid date format"
                    bolPaymentDateErr = True
                End Try
                If bolPaymentDateErr = True Then
                    Exit Sub
                End If
                If DateTime.Compare(fromPaymentDateTimeUS, toPaymentDateTimeUS) > 0 Then
                    lblErrdate.Visible = True
                    lblErrdate.Text = "From date is greater than To date"
                    Exit Sub
                End If
                If DateTime.Compare(fromDateTimeUS.AddMonths(3), toDateTimeUS) < 0 Then
                    lblErrdate.Visible = True
                    lblErrdate.Text = "Date range exceeds 3 months"
                    Exit Sub
                End If
                txtFromDate.Text = Convert.ToDateTime(RadPaymentFrom.SelectedDate).ToString("d")
                txtToDate.Text = Convert.ToDateTime(RadPaymentTo.SelectedDate).ToString("d")
                PaymentDateTime = True
                lblErr.Visible = False
            End If
            If DateTime.Compare(fromDateTimeUS, toDateTimeUS) > 0 Then
                lblErrdate.Visible = True
                lblErrdate.Text = "From date is greater than To date"
                Exit Sub
            End If
            If DateTime.Compare(fromDateTimeUS.AddMonths(3), toDateTimeUS) < 0 Then
                lblErrdate.Visible = True
                lblErrdate.Text = "Date range exceeds 3 months"
                Exit Sub
            End If
            If PaymentDateTime = False Then
                txtFromDate.Text = Convert.ToDateTime(RadDatePickerFrom.SelectedDate).ToString("d")
                txtToDate.Text = Convert.ToDateTime(RadDatePickerTo.SelectedDate).ToString("d")
                DateBegin = Convert.ToDateTime(RadDatePickerFrom.SelectedDate).ToString("d")
                DateEnd = Convert.ToDateTime(RadDatePickerTo.SelectedDate).ToString("d")
            End If
        Else
            txtFromDate.Text = Now.AddYears(-1).ToString("d")
            txtToDate.Text = dteNow.ToString("d")
            DateBegin = Now.AddYears(-1).ToString("d") 'gez 7/20/2012 'Date for BRT data
            DateEnd = dteNow.ToString("d") 'gez 7/20/2012 BRT 'Date for BRT data

        End If

        'gez 7/20/2012 BRT sort by Invoice ID, set all other values except date to empty
        'If dropSearchBy.SelectedValue = "Invoice ID" Then
        '    BRTinvoiceNumber = txtSearchCriteria.ToString
        'End If

        ''gez 7/20/2012 BRT sort by Payment ID, seat all other values except date to empty
        'If dropSearchBy.SelectedValue = "Payment ID" Then
        '    'code here, don't have payment ID at this stage
        'End If

        ''gez 7/20/2012 BRT sort by Voucher ID, seat all other values except date to empty
        'If dropSearchBy.SelectedValue = "Voucher ID" Then
        '    'code here, 
        'End If

        BindDataGrid()
        dropSelectReport.SelectedIndex = 0
        'HideSearchParams()

    End Sub

    Sub BindDataGrid()

        'removed YA 20181120
        'If Trim(txtSearchCriteria.Text) = "" And _
        '    dropSelectReport.SelectedIndex = 0 Then
        '    'Statgrid_offline.Visible = False'7/6/2012
        '    btnExcel.Visible = False
        '    chbFreeze.Visible = False
        '    Exit Sub
        'End If

        buildpymntstatus()
        'strSitePre = GetSitePrefix(Session("BUSUNIT_VI2_BRT"))

        Dim strSQLString As String
        Dim strLocations As String
        Dim strInvItemID As String


        strSQLString = "SELECT distinct * from (" & vbCrLf &
                       "SELECT " & vbCrLf &
        "  B.REMIT_VENDOR" & vbCrLf &
        " ,A.NAME1" & vbCrLf &
        " ,GRP12.SHIPMENT_NO" & vbCrLf &
        " ,A.PYMNT_ID" & vbCrLf &
        " ,A.PYMNT_ID_REF" & vbCrLf &
        " ,GRP12.PO_ID" & vbCrLf &
        " ,TO_CHAR(A.PYMNT_DT,'MM/DD/YYYY') AS PYMNT_DATE" & vbCrLf &
        " ,B.VOUCHER_ID" & vbCrLf &
        " ,TO_CHAR(B.DUE_DT,'MM/DD/YYYY') AS DUE_DATE" & vbCrLf &
        " ,TO_CHAR(B.DSCNT_DUE_DT,'MM/DD/YYYY') AS DSCNT_DUE_DATE" & vbCrLf &
        " ,B.DSCNT_PAY_AMT" & vbCrLf &
        " ,TO_CHAR(B.SCHEDULED_PAY_DT,'MM/DD/YYYY') AS SCHEDULED_PAY_DATE" & vbCrLf &
        " ,B.PAID_AMT_DSCNT" & vbCrLf &
        " ,B.DESCR254_MIXED" & vbCrLf &
        " ,C.INVOICE_ID" & vbCrLf &
        " ,TO_CHAR(C.INVOICE_DT,'MM/DD/YYYY') AS INVOICE_DATE" & vbCrLf &
        " ,C.VCHR_TTL_LINES " & vbCrLf &
        " ,C.GROSS_AMT " & vbCrLf &
        " ,C.BUSINESS_UNIT " & vbCrLf &
        " ,(SELECT XL.XLATLONGNAME" & vbCrLf &
        " FROM SYSADM8.PSXLATITEM XL " & vbCrLf &
        " WHERE XL.EFFDT = (SELECT MAX(A_ED.EFFDT) FROM SYSADM8.PSXLATITEM A_ED  " & vbCrLf &
        " WHERE(XL.FIELDNAME = A_ED.FIELDNAME)" & vbCrLf &
        " AND XL.FIELDVALUE = A_ED.FIELDVALUE" & vbCrLf &
        " AND A_ED.EFFDT <= SYSDATE" & vbCrLf &
        " )" & vbCrLf &
        " AND XL.FIELDNAME = 'MATCH_STATUS_VCHR'" & vbCrLf &
        " AND XL.FIELDVALUE = C.MATCH_STATUS_VCHR" & vbCrLf &
        " ) AS MATCH_STATUS" & vbCrLf &
        " ,A.PYMNT_STATUS" & vbCrLf &
        " ,GRP12.DEPTID " & vbCrLf &
        " ,GRP12.ACCOUNT" & vbCrLf &
        " FROM " & vbCrLf &
        " SYSADM8.PS_PAYMENT_TBL A" & vbCrLf &
        " ,SYSADM8.PS_PYMNT_VCHR_XREF B" & vbCrLf &
        " ,SYSADM8.PS_VOUCHER C" & vbCrLf &
        " ,( " & vbCrLf &
        " SELECT /*+ INDEX(C PSWVOUCHER) */ DISTINCT" & vbCrLf &
                    " C.BUSINESS_UNIT ," & vbCrLf &
                     "    C.VENDOR_ID ," & vbCrLf &
                      "   C.VOUCHER_ID , " & vbCrLf &
                       "  X.PO_ID, " & vbCrLf &
                        " X.ACCOUNT, " & vbCrLf &
                        " X.DEPTID, " & vbCrLf &
        " ' ' as SHIPMENT_NO" & vbCrLf &
        " FROM SYSADM8.PS_VOUCHER C," & vbCrLf &
        " SYSADM8.PS_DISTRIB_LINE X," & vbCrLf &
        " SYSADM8.PS_VOUCHER_LINE K " & vbCrLf &
        "" & vbCrLf &
        " WHERE C.BUSINESS_UNIT IN ('ISA00','SDM00','SDC00', 'WAL00')" & vbCrLf &
        " AND C.VENDOR_ID = '" & Session("VENDOR_VP") & "'" & vbCrLf
        If PaymentDateTime = False Then
            If Not slither = 0 Then
                Dim slither1 As Integer = slither + 30
                strSQLString = strSQLString &
                                        " AND (C.INVOICE_DT) BETWEEN " & vbCrLf &
                                        " (sysdate) -" & slither1 & vbCrLf &
                                        " and (sysdate)-" & slither & vbCrLf
            Else
                strSQLString = strSQLString &
           " AND (C.INVOICE_DT BETWEEN TO_DATE('" & txtFromDate.Text & "','MM-DD-YYYY')" & vbCrLf &
                              " AND TO_DATE('" & txtToDate.Text & "','MM-DD-YYYY'))" & vbCrLf
            End If
        End If


        If Not Trim(txtPOSearch.Text) = "" Then
            strSQLString = strSQLString & " AND k.po_id = '" & Trim(txtPOSearch.Text) & "'" & vbCrLf
        End If

        strSQLString = strSQLString &
           "And k.po_id <>' '" & vbCrLf &
            "And x.business_unit = c.business_unit" & vbCrLf &
            "And x.voucher_id = c.voucher_id" & vbCrLf &
            "And x.voucher_id = x.voucher_id" & vbCrLf &
            "And x.business_unit = k.business_unit" & vbCrLf &
            "And x.voucher_id= k.voucher_id" & vbCrLf &
            "And  x.voucher_line_num = k.voucher_line_num" & vbCrLf &
               " ) GRP12" & vbCrLf &
               " WHERE C.BUSINESS_UNIT = GRP12.BUSINESS_UNIT " & vbCrLf &
               " AND C.VOUCHER_ID    = GRP12.VOUCHER_ID " & vbCrLf &
               " AND C.VENDOR_ID = GRP12.VENDOR_ID " & vbCrLf &
        "AND C.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf &
        "AND C.VOUCHER_ID    = B.VOUCHER_ID" & vbCrLf
        'If (dropSearchBy.SelectedValue = "Invoice ID" And _
        '    Not Trim(txtSearchCriteria.Text) = "" And _
        '    Not Trim(txtSearchCriteria.Text) = "search by date") Then
        '    strSQLString = strSQLString & " AND C.INVOICE_ID = '" & Trim(txtSearchCriteria.Text) & "'" & vbCrLf

        'End If
        If Not Trim(txtSearchCriteria.Text) = "" Then
            strSQLString = strSQLString & " AND C.INVOICE_ID = '" & Trim(txtSearchCriteria.Text) & "'" & vbCrLf
        End If
        If Not Trim(txtPOSearch.Text) = "" Then
            strSQLString = strSQLString & " AND GRP12.PO_ID = '" & Trim(txtPOSearch.Text) & "'" & vbCrLf
        End If


        'strSQLString = strSQLString & "    AND B.BANK_CD IN ('FUB01','JPMR1','JPMRK','WSF01','PNC01')" & vbCrLf & _
        strSQLString = strSQLString & "    AND B.BANK_SETID    = A.BANK_SETID(+)" & vbCrLf &
 "    AND B.BANK_CD       = A.BANK_CD(+)" & vbCrLf &
 "    AND B.BANK_ACCT_KEY = A.BANK_ACCT_KEY(+)" & vbCrLf &
 "    AND B.PYMNT_ID      = A.PYMNT_ID(+)" & vbCrLf &
          "AND (     'S' <> A.PYMNT_STATUS(+)" & vbCrLf &
          "AND 'V' <> A.PYMNT_STATUS(+)" & vbCrLf &
            "AND 'X' <> A.PYMNT_STATUS(+))" & vbCrLf
        If PaymentDateTime = True Then
            strSQLString = strSQLString &
       " AND (A.PYMNT_DT BETWEEN TO_DATE('" & txtFromDate.Text & "','MM-DD-YYYY')" & vbCrLf &
                          " AND TO_DATE('" & txtToDate.Text & "','MM-DD-YYYY'))" & vbCrLf
        End If

        strSQLString = strSQLString & " UNION " & vbCrLf &
         "select bapx.vendor_id,capx.name1,' ' as shipment_no, " & vbCrLf &
         "' ',' ',aapx.po_id,' ' AS pymnt_date,' ', " & vbCrLf &
         "'' AS due_date,'' AS dscnt_due_date,0,'' AS scheduled_pay_date, " & vbCrLf &
         "0,' ',bapx.invoice_id, " & vbCrLf &
         "to_char(bapx.invoice_dt, 'MM/DD/YYYY')  AS invoice_date, " & vbCrLf &
         "count(1),bapx.gross_amt,aapx.business_unit,'In Process'  AS match_status, " & vbCrLf &
         "'N',dapx.deptid,'210303' " & vbCrLf &
         "from ps_apxvchrline_stg aapx,ps_apxvchr_hdr_stg bapx,  ps_vendor capx, ps_apxvchrdist_stg dapx " & vbCrLf &
         "where " & vbCrLf &
         "aapx.business_unit   = bapx.business_unit " & vbCrLf &
         "and  aapx.vchr_bld_key_c1 = bapx.vchr_bld_key_c1 " & vbCrLf &
         "and  aapx.vchr_bld_key_c2 = bapx.vchr_bld_key_c2 " & vbCrLf &
         "and  aapx.vchr_bld_key_n1 = bapx.vchr_bld_key_n1 " & vbCrLf &
         "and  bapx.vendor_setid    = capx.setid " & vbCrLf &
         "and  bapx.vendor_id       = capx.vendor_id " & vbCrLf &
         "and  dapx.business_unit = bapx.business_unit " & vbCrLf &
         "and  dapx.vchr_bld_key_c1 = bapx.vchr_bld_key_c1 " & vbCrLf &
         "and  dapx.vchr_bld_key_c2 = bapx.vchr_bld_key_c2 " & vbCrLf &
         "and  dapx.vchr_bld_key_n1 = bapx.vchr_bld_key_n1 " & vbCrLf &
         "and  dapx.voucher_line_num = aapx.voucher_line_num " & vbCrLf &
         "and bapx.vendor_id = '" & Session("VENDOR_VP") & "'" & vbCrLf
        If PaymentDateTime = False Then
            If Not slither = 0 Then
                Dim slither1 As Integer = slither + 30
                strSQLString = strSQLString &
                                        " AND (bapx.INVOICE_DT) BETWEEN " & vbCrLf &
                                        " (sysdate) -" & slither1 & vbCrLf &
                                        " and (sysdate)-" & slither & vbCrLf
            Else
                strSQLString = strSQLString &
           " AND (bapx.INVOICE_DT BETWEEN TO_DATE('" & txtFromDate.Text & "','MM-DD-YYYY')" & vbCrLf &
                              " AND TO_DATE('" & txtToDate.Text & "','MM-DD-YYYY')) " & vbCrLf
            End If
        End If

        If Not Trim(txtSearchCriteria.Text) = "" Then
            strSQLString = strSQLString & " AND bapx.INVOICE_ID = '" & Trim(txtSearchCriteria.Text) & "' " & vbCrLf
        End If
        If Not Trim(txtPOSearch.Text) = "" Then
            strSQLString = strSQLString & " AND aapx.PO_ID = '" & Trim(txtPOSearch.Text) & "' " & vbCrLf
        End If

        strSQLString = strSQLString & "and  not exists (select 'x' " & vbCrLf &
         "from ps_voucher eapx " & vbCrLf &
         "where bapx.business_unit = eapx.business_unit " & vbCrLf &
         "and   bapx.invoice_id = eapx.invoice_id " & vbCrLf &
         "and   bapx.vendor_id = eapx.vendor_id) " & vbCrLf &
         "group by bapx.vendor_id,capx.name1,' ',aapx.po_id, " & vbCrLf &
         "bapx.invoice_id,to_char(bapx.invoice_dt, 'MM/DD/YYYY'),aapx.business_unit, " & vbCrLf &
         "'In Process','N',dapx.deptid,'210303',bapx.gross_amt " & vbCrLf

        strSQLString = strSQLString & "UNION " & vbCrLf &
         "select bapx.vendor_id,capx.name1,' ' as shipment_no, " & vbCrLf &
         "' ',' ',aapx.po_id,' ' AS pymnt_date,' ','' AS due_date, " & vbCrLf &
         "'' AS dscnt_due_date,0,'' AS scheduled_pay_date, " & vbCrLf &
         "0,' ',bapx.invoice_id,to_char(bapx.invoice_dt, 'MM/DD/YYYY')  AS invoice_date, " & vbCrLf &
         "count(1),bapx.gross_amt,aapx.business_unit,'In Process'  AS match_status, " & vbCrLf &
         "'N',dapx.deptid,'210303' " & vbCrLf &
         "from ps_vchr_line_stg aapx,ps_vchr_hdr_stg bapx, ps_vendor capx, ps_vchr_dist_stg dapx " & vbCrLf &
         "where  " & vbCrLf &
         "aapx.business_unit   = bapx.business_unit " & vbCrLf &
         "and  aapx.vchr_bld_key_c1 = bapx.vchr_bld_key_c1 " & vbCrLf &
         "and  aapx.vchr_bld_key_c2 = bapx.vchr_bld_key_c2 " & vbCrLf &
         "and  aapx.vchr_bld_key_n1 = bapx.vchr_bld_key_n1 " & vbCrLf &
         "and  bapx.vendor_setid    = capx.setid " & vbCrLf &
         "and  bapx.vendor_id       = capx.vendor_id " & vbCrLf &
         "and  dapx.business_unit = bapx.business_unit " & vbCrLf &
         "and  dapx.vchr_bld_key_c1 = bapx.vchr_bld_key_c1 " & vbCrLf &
         "and  dapx.vchr_bld_key_c2 = bapx.vchr_bld_key_c2" & vbCrLf &
         "and  dapx.vchr_bld_key_n1 = bapx.vchr_bld_key_n1 " & vbCrLf &
         "and  dapx.voucher_line_num = aapx.voucher_line_num " & vbCrLf &
         "and  bapx.vendor_id = '" & Session("VENDOR_VP") & "'" & vbCrLf

        If PaymentDateTime = False Then
            If Not slither = 0 Then
                Dim slither1 As Integer = slither + 30
                strSQLString = strSQLString &
                                        " AND (bapx.INVOICE_DT) BETWEEN " & vbCrLf &
                                        " (sysdate) -" & slither1 & vbCrLf &
                                        " and (sysdate)-" & slither & vbCrLf
            Else
                strSQLString = strSQLString &
           " AND (bapx.INVOICE_DT BETWEEN TO_DATE('" & txtFromDate.Text & "','MM-DD-YYYY')" & vbCrLf &
                              " AND TO_DATE('" & txtToDate.Text & "','MM-DD-YYYY'))" & vbCrLf
            End If
        End If


        If Not Trim(txtSearchCriteria.Text) = "" Then
            strSQLString = strSQLString & " AND bapx.INVOICE_ID = '" & Trim(txtSearchCriteria.Text) & "'" & vbCrLf
        End If
        If Not Trim(txtPOSearch.Text) = "" Then
            strSQLString = strSQLString & " AND aapx.PO_ID = '" & Trim(txtPOSearch.Text) & "'" & vbCrLf
        End If


        strSQLString = strSQLString & "And aapx.po_id <> ' '" & vbCrLf &
            "and  not exists (select 'x' " & vbCrLf &
         "from ps_voucher eapx " & vbCrLf &
         "where bapx.business_unit = eapx.business_unit " & vbCrLf &
         "and   bapx.voucher_id = eapx.voucher_id) " & vbCrLf &
         "group by bapx.vendor_id,capx.name1,' ',aapx.po_id, " & vbCrLf &
         "bapx.invoice_id,to_char(bapx.invoice_dt, 'MM/DD/YYYY'), " & vbCrLf &
         "aapx.business_unit,'In Process','N',dapx.deptid,'210303',bapx.gross_amt" & vbCrLf &
         "" & vbCrLf &
         "" & vbCrLf &
         "" & vbCrLf &
         "" & vbCrLf


        strSQLString = strSQLString + ") WHERE  ROWNUM < 1001" & vbCrLf

        strSQLString = strSQLString & " ORDER BY " & txtSortField.Text

        Dim ds As DataSet = ORDBData.GetAdapter(strSQLString)

        '-------pull in vendor for given vendorID, Which name in database do we use?---
        'Dim VendorsStringA As String = "select name1 from SYSADM8.ps_vendor where vendor_status = 'A' and Vendor_ID = " 
        'Dim VendorsStringA As String = "select name1 from SYSADM8.ps_vendor where vendor_status = 'A' and Vendor_ID = " 'gez 10/9/2012
        'Dim VendorsStringA As String = "select vndr_name_shrt_usr from SYSADM8.ps_vendor where vendor_status = 'A' and Vendor_ID = " 'gez 10/9/2012
        Dim VendorsStringA As String = "select upper(Name1) from SYSADM8.ps_vendor where vendor_status = 'A' and Vendor_ID = " 'GEZ 10/23/2012
        Dim VendorsStringB As String = Session("VENDOR_VP").ToString
        Dim VendorsStringC As String = VendorsStringB.ToString
        Dim SQLstrVendorsString As String = VendorsStringA & "'" & VendorsStringB & "'"
        TextBox4.Text = SQLstrVendorsString
        Dim VendorNameds As DataSet = ORDBData.GetAdapter(SQLstrVendorsString)
        If Not VendorNameds Is Nothing Then
            If VendorNameds.Tables.Count > 0 Then
                If VendorNameds.Tables(0).Rows.Count > 0 Then
                    TextBox4.Text = VendorNameds.Tables(0).Rows(0)(0)
                    Dim VendorName As String = VendorNameds.Tables(0).Rows(0)(0) ' put in webservice when go live
                End If
            End If
        End If
        'note: there are other names such as short, long also may have to take out comas to match what BRT has

        ''--------------------WebserviceSearch - THIS IS NOT USED ANYMORE VR 05/23/2014 (per Ron F.) ---------------
        ''http://apweb.isacs.com:8080/portal/APSearch?wsdl

        'Dim userCredentials As New com.isacs.apwebMy1.UserCredentials() '  com.isacs.apweb.UserCredentials
        'userCredentials.userName = "captiva.admin"
        'userCredentials.password = "Captiva**Admin"

        'Dim criteria As New com.isacs.apwebMy1.apSearchCriteria() '  com.isacs.apweb.apSearchCriteria 'The input message definitions to the searchForInvoices operation are SearchCriteria 
        'criteria.invoiceDateBegin = DateBegin '"4/12/2012"
        'criteria.invoiceDateEnd = DateEnd '"8/12/2012"
        'criteria.invoiceNumber = BRTinvoiceNumber 'need Invoice ID if blank selects all
        'VendorName = VendorName.Replace("&", "&amp;") 'replace & with &amp so matches out put 'may need to change if they fix on thier end'gez 10/24/2012 *new*
        'criteria.vendorName = VendorName ' gez 7/30/2012 turn on when go live 'gez 10/9/2012
        ''criteria.invoiceStatus = "" ' "In Process" '(Paid, Rejected, In Process) I need Rejected and In Process 'turn on when go live
        ''need Payment ID not need for BRT 
        ''criteria.vendorName = "" 'better to use venderID, eterate through each to get VendorID

        'Dim search As New com.isacs.apwebMy1.APSearch() '   com.isacs.apweb.APSearch() 'what need
        'Dim invoiceResults() As com.isacs.apwebMy1.SearchResult '  com.isacs.apweb.SearchResult 'message type

        'Try
        '    Dim S1 As String = search.Url
        '    invoiceResults = search.searchForInvoices(criteria, userCredentials)
        'Catch
        '    'Label5.Text = "Conversion failed."
        'End Try

        ''-----------------displayInvoiceResults-----------
        'If Not invoiceResults Is Nothing Then 'gez 7/20/2012
        '    Dim Column1Value(invoiceResults.Length - 1) As String
        '    For row As Integer = 0 To UBound(Column1Value) - 1

        '        Dim newCustomersRow As DataRow = ds.Tables(0).NewRow() 'show new row
        '        newCustomersRow("VOUCHER_ID") = " " ' invoiceResults(row).purchaseOrderNumber   'SDI VOUCHER_ID   = BRT purchaseOrderNumber
        '        'newCustomersRow("VOUCHER_ID") = invoiceResults(row).vendorName ' gez 10/9/2012 test, uncoment line above
        '        newCustomersRow("NAME1") = invoiceResults(row).vendorName                 'SDI NAME1        = BRT vendorName (hiddin column)
        '        newCustomersRow("INVOICE_ID") = invoiceResults(row).invoiceNumber         'SDI INVOICE_ID   = BRT invoiceNumber 
        '        newCustomersRow("GROSS_AMT") = CInt(invoiceResults(row).invoiceAmount)   'SDI GROSS_AMT    = BRT invoiceAmount (bombs if only one number after decimal)'turn on when live
        '        newCustomersRow("INVOICE_DATE") = invoiceResults(row).invoiceDate         'SDI INVOICE_DATE = BRT invoiceDate 
        '        newCustomersRow("PYMNT_ID_REF") = invoiceResults(row).purchaseOrderNumber 'SDI PYMNT_ID_REF = BRT purchaseOrderNumber

        '        '---BRT return "false", SDI requires " " 
        '        If invoiceResults(row).actualPayDateSpecified = "false" Then
        '            newCustomersRow("SCHEDULED_PAY_DATE") = ""
        '        Else
        '            newCustomersRow("SCHEDULED_PAY_DATE") = invoiceResults(row).actualPayDateSpecified
        '        End If

        '        '---BRT returns "In Process", SDI requires "Matched exception exists"
        '        If invoiceResults(row).status = "In Process" Then
        '            newCustomersRow("MATCH_STATUS") = "Matched exception exists"
        '        Else
        '            newCustomersRow("MATCH_STATUS") = invoiceResults(row).status
        '        End If

        '        '---format columns
        '        newCustomersRow("DSCNT_PAY_AMT") = "0.00" 'no value retrned from BRT for this field
        '        newCustomersRow("PAID_AMT_DSCNT") = "0.00" 'no value retrned from BRT for this field

        '        '---Include all In "Proces" and "Rejected" results from BRT,checkbox true is default   
        '        If Me.chkunpaidVoucher.Checked = True Then '7/30/2012 
        '            If invoiceResults(row).status = "In Process" Then ds.Tables(0).Rows.Add(newCustomersRow) '7/30/2012 
        '            If invoiceResults(row).status = "Rejected" Then ds.Tables(0).Rows.Add(newCustomersRow) '7/30/2012 
        '        End If
        '    Next row
        'End If

        'sort array by date(mix of web service and database)'gez 2/6/2013
        'myDataTable.DefaultView.Sort = "PublishDate ASC"
        'Tables(0)   ds.DefaultViewManager.sort = "INVOICE_DATE"
        '---------------------------------------------------
        '---------------------------------------------------
        '---------------------------------------------------

        If ds.Tables(0).Rows.Count > 0 Then
            Session("VoucherInfoRpt") = ds
            Statgrid.DataSource = ds.Tables(0)
            Statgrid.PageSize = ds.Tables(0).Rows.Count
            Statgrid.DataBind()

            Dim strMessage As New Alert
            If chbNoLimit.Checked = False Then
                If Statgrid.Items.Count = 1000 Then
                    ltlAlert.Text = strMessage.Say("**Warning - results greater than 1000 lines...\nOnly the first 1000 lines are returned.\n" &
                    "Please limit your results by\n changing your 'Select Report' option.")

                End If
            End If

            lblNoData.Visible = False
            Statgrid.Visible = True
            btnExcel.Visible = True
            chbFreeze.Visible = False
            slither = 0

        Else
            lblNoData.Visible = True
            Statgrid.Visible = False
            btnExcel.Visible = False
            chbFreeze.Visible = False
        End If

    End Sub

    Private Sub StatGrid_NeedDataSource(ByVal sender As Object, ByVal e As Telerik.Web.UI.GridNeedDataSourceEventArgs) Handles Statgrid.NeedDataSource
        If Session("VoucherInfoRpt") IsNot Nothing Then
            Dim ds As DataSet = CType(Session("VoucherInfoRpt"), DataSet)
            If ds IsNot Nothing Then
                If ds.Tables.Count > 0 Then
                    Statgrid.DataSource = ds.Tables(0)
                End If
            End If
        End If
    End Sub

    Sub StatGrid_SortCommand(ByVal s As Object, ByVal e As GridSortCommandEventArgs)
        Try

            'GetDataSource()
            ''txtSortField.Text = e.SortExpression
            'BindDataGrid()

        Catch ex As Exception

        End Try
    End Sub


    Sub testStuff()

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ''http://apweb.isacs.com:8080/portal/APGetDetails?wsdl
        ''web service 'getInvoiceDetails' - com.isacs.apweb1
        ''APGetDetailServices
        ''methods: getInvoiceDetails ( ),  getInvoiceProcessingHistory ( ) 
        ''1 Service Found: APGetDetails
        ''1 Dataset Found: APGetDetails
        ''in: <getDetailsCriteria> --> <invoiceNumber>
        ''return: <Invoice> --> <detailLines> --> <detailLine> --> <description>

        'Dim userCredentials2 As New com.isacs.apweb1.UserCredentials
        'userCredentials2.userName = "captiva.admin"
        'userCredentials2.password = "Captiva**Admin"

        'Dim GetDetailsCriteria As New com.isacs.apweb1.APGetDetailsCriteria
        ''GetDetailsCriteria.invoiceNumber = "9300936142"

        ''Dim DetailsService As New com.isacs.apweb1.APGetDetailsService()
        ' ''Dim Results() As com.isacs.apweb1.Invoice ' dims as an array 'message type
        ''Dim Results As New com.isacs.apweb1.Invoice 'dim as object

        ''Dim GetDetails As New com.isacs.apweb1.APGetDetailsService 'Invoice
        ''Results = DetailsService.getInvoiceDetails(GetDetailsCriteria, userCredentials2)

        ''Dim dsInvoice As com.isacs.apweb1.Invoice '

        ''Results = DetailsService.getInvoiceDetails(GetDetailsCriteria, userCredentials2) 'Works put in try catch
        ''Results.vendorID

        ''Dim Invoice As DataSet = DetailsService.getInvoiceDetails(GetDetailsCriteria, userCredentials2)

        ''Dim proxySample As New com.isacs.apweb1.APGetDetailsService
        ''Dim customersDataSet As DataSet = proxySample.getInvoiceDetails(GetDetailsCriteria, userCredentials2)


        ''Dim GetDetails As New com.isacs.apweb1.APGetDetailsService

        ''Dim invoice As New com.isacs.apweb1.APGetDetailsService()
        ''Dim getInvoiceDetails(,,) As com.isacs.apweb1.Invoice  '3d array 'message type

        ''need data set in strings
        ''Dim dsInvoice As DataSet
        ''Invoice = Invoice.getInvoiceDetails(GetDetailsCriteria, userCredentials2) 'dataset

        ''Statgrid8.DataSource = invoice.Tables(0)
        ''Statgrid8.DataBind()
        ''Try
        ''Results = DetailsService.getInvoiceDetails(GetDetailsCriteria, userCredentials2) 'good
        ' ''getInvoiceDetails = invoice.getInvoiceDetails(GetDetailsCriteria, userCredentials2) 'array
        ' ''txtTest.Text = getInvoiceDetails(0).ecmInvoiceID
        ''Catch
        ' ''Label5.Text = "Conversion failed."
        ''End Try
        ''Results.vendorID
        ''put in to array vendorID_aray
        Dim vendorIDArray As String
        ''vendorIDArray = Results.vendorID(1) 'may ne null
        ''vendorIDArray = Results.vendorName ' there is only one result per request, how long to request total

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    End Sub



    Private Sub buildpymntstatus()

        hashStatus = New Hashtable
        Dim strSQLstring As String = "SELECT A.FIELDVALUE, A.XLATLONGNAME" & vbCrLf &
                " FROM SYSADM8.PSXLATITEM A" & vbCrLf &
                " WHERE A.EFFDT =" & vbCrLf &
                " (SELECT MAX(A_ED.EFFDT) FROM SYSADM8.PSXLATITEM A_ED" & vbCrLf &
                " WHERE A.FIELDNAME = A_ED.FIELDNAME" & vbCrLf &
                " AND A.FIELDVALUE = A_ED.FIELDVALUE" & vbCrLf &
                " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf &
                " AND A.FIELDNAME = 'PYMNT_STATUS'" & vbCrLf &
                " AND A.EFF_STATUS = 'A'"

        Dim dr As OleDbDataReader
        dr = ORDBData.GetReader(strSQLstring)
        Do While dr.Read
            hashStatus.Add(dr.Item("FIELDVALUE"), dr.Item("XLATLONGNAME"))
        Loop
        dr.Close()
    End Sub

    Private Sub dropIndexChanged()

        setFromToDates()
        If Not dropSelectReport.SelectedValue = "99" Then
            'txtSearchFrom.Text = ""
            'txtSearchTo.Text = ""
            RadDatePickerFrom.Clear()
            RadDatePickerTo.Clear()
            txtSearchCriteria.Text = ""
            'dropSearchBy.SelectedIndex = 0
            txtSortField.Text = "PYMNT_ID"
            BindDataGrid()
        End If

    End Sub

    Private Sub HideSearchParams()
        Statgrid.Visible = True
        btnExcel.Visible = True
        chbFreeze.Visible = False  '  True
        Panel1.Visible = False
        Panel2.Visible = False
        'lblOR.Visible = False
    End Sub

    Private Sub setFromToDates()

        Dim dteNow As Date
        Dim dtePrev As Date
        dteNow = Now().ToString("d")
        Select Case dropSelectReport.SelectedValue
            Case "30", "X"
                txtFromDate.Text = Now.AddDays(-30).ToString("d")
                txtToDate.Text = dteNow.ToString("d")
                HideSearchParams()
                'gez 7/20/2012 'last 30 days for BRT
                DateBegin = Now.AddDays(-30).ToString("d") ' "12/12/2001"
                DateEnd = dteNow.ToString("d") '"12/12/2022"
            Case "7"
                txtFromDate.Text = Now.AddDays(-7).ToString("d")
                txtToDate.Text = dteNow.ToString("d")
                HideSearchParams()
                'gez 7/20/2012 'last 7 days for BRT
                DateBegin = Now.AddDays(-7).ToString("d") ' "12/12/2001"
                DateEnd = dteNow.ToString("d") '"12/12/2022"
            Case "15"
                txtFromDate.Text = Now.AddDays(-15).ToString("d")
                txtToDate.Text = dteNow.ToString("d")
                HideSearchParams()
                'gez 7/20/2012 ' last 15 days for BRT
                DateBegin = Now.AddDays(-15).ToString("d") ' "12/12/2001"
                DateEnd = dteNow.ToString("d") '"12/12/2022"
            Case "0"
                txtFromDate.Text = Now.Month & "/01/" & Now.Year
                txtToDate.Text = dteNow.ToString("d")
                HideSearchParams()
                'gez 7/20/2012 ' for BRT
                DateBegin = Now.Month & "/01/" & Now.Year ' "12/12/2001"
                DateEnd = dteNow.ToString("d") '"12/12/2022"
            Case "3M"
                txtFromDate.Text = Now.AddMonths(-3).ToString("d")
                txtToDate.Text = dteNow.ToString("d")
                HideSearchParams()
                'gez 7/20/2012 ' for BRT
                DateBegin = Now.AddMonths(-3).ToString("d")
                DateEnd = dteNow.ToString("d")
            Case "6M"
                txtFromDate.Text = Now.AddMonths(-6).ToString("d")
                txtToDate.Text = dteNow.ToString("d")
                HideSearchParams()
                'gez 7/20/2012 ' for BRT
                DateBegin = Now.AddMonths(-6).ToString("d")
                DateEnd = dteNow.ToString("d")
            Case "1Y"
                txtFromDate.Text = Now.AddYears(-1).ToString("d")
                txtToDate.Text = dteNow.ToString("d")
                HideSearchParams()
                'gez 7/20/2012 ' for BRT
                DateBegin = Now.AddYears(-1).ToString("d")
                DateEnd = dteNow.ToString("d")
            Case "S-30"
                slither = 30
                HideSearchParams()
                'gez 7/20/2012 ' BRT "S-30">Next 30 days - 30-60 days
                DateBegin = Now.AddDays(-60).ToString("d")
                DateEnd = Now.AddDays(-30).ToString("d")
            Case "S-60"
                slither = 60
                HideSearchParams()
                'gez 7/20/2012 ' BRT "S-60">Next 30 days - 60-90 days
                DateBegin = Now.AddDays(-90).ToString("d")
                DateEnd = Now.AddDays(-60).ToString("d")
            Case "S-90"
                slither = 90
                HideSearchParams()
                'gez 7/20/2012 ' BRT "S-90">Next 30 days -90-120 days
                DateBegin = Now.AddDays(-120).ToString("d")
                DateEnd = Now.AddDays(-90).ToString("d")
            Case "S-120"
                slither = 120
                HideSearchParams()
                'gez 7/20/2012 ' BRT "S-120">Next 30 days - 120-150 days
                DateBegin = Now.AddDays(-150).ToString("d")
                DateEnd = Now.AddDays(-120).ToString("d")
            Case "S-150"
                slither = 150
                HideSearchParams()
                'gez 7/20/2012 ' BRT "S-150">Next 30 days - 150-180 days
                DateBegin = Now.AddDays(-180).ToString("d")
                DateEnd = Now.AddDays(-150).ToString("d")
            Case "S-180"
                slither = 180
                HideSearchParams()
                'gez 7/20/2012 ' BRT "S-180">Next 30 days - 180-210 days
                DateBegin = Now.AddDays(-210).ToString("d")
                DateEnd = Now.AddDays(-180).ToString("d")
            Case "S-210"
                slither = 210
                HideSearchParams()
                'gez 7/20/2012 ' BRT "S-210">Next 30 days - 210-240 days
                DateBegin = Now.AddDays(-240).ToString("d")
                DateEnd = Now.AddDays(-210).ToString("d")
            Case "-1"
                dteNow = Now.AddMonths(-1)
                dtePrev = New Date(dteNow.Year, dteNow.Month, 1)
                txtFromDate.Text = dtePrev
                DateBegin = dtePrev 'gez 7/20/2012 BRT
                dtePrev = dtePrev.AddMonths(1)
                dtePrev = dtePrev.AddDays(-1)
                txtToDate.Text = dtePrev
                DateEnd = dtePrev 'gez 7/20/2012 BRT
                HideSearchParams()
            Case "99"
                ViewSearchParams()
        End Select
    End Sub
    Private Sub clearVocherSessions()
        mySessionState.pathSQLs = Nothing
        mySessionState.Vendor = Nothing
        mySessionState.SearchPageSelectedPO = Nothing
        mySessionState.VoucherInfo = Nothing
    End Sub
    Private Sub getPOVoucherDetails(strPoNum As String, m_sVendor As String, strPOBU As String)

        m_pathSQLs = System.IO.Path.GetDirectoryName(path:=Request.ServerVariables("PATH_TRANSLATED")) &
                                    "\InsiteOnlineVoucherEntry\SQLs\"
        m_pathSQLs = m_pathSQLs.Replace("insiteonlineVoucher\", "")
        Dim isPOExist As Boolean = False
        Try
            clearVocherSessions()

            mySessionState.pathSQLs = m_pathSQLs

            mySessionState.Vendor = myCommon.GetVendor(m_sVendor, m_pathSQLs, ORDBData.DbUrl)

            isPOExist = LoadVendorPOList(strPoNum, m_sVendor, m_pathSQLs, True)

            If isPOExist Then
                m_selectedPO = mySessionState.SearchPageSelectedPO
                m_vendorInfo = mySessionState.Vendor
                m_pathSQLs = mySessionState.pathSQLs
                m_voucher = mySessionState.VoucherInfo

                'If Not (mySessionState.UnitOfMeasure.Count > 0) Then
                '    BuildUOMList(m_pathSQLs, ORDBData.DbUrl)
                'End If
                '' check/build Vendor Additional Charges reference list
                'If Not (mySessionState.AdditionalVendorCharges.Count > 0) Then
                '    BuildAdditionalVendorCharges()
                'End If

                ' get buyer information
                Dim buyerInfo As Buyer = myCommon.GetPOBuyerInfo(m_selectedPO,
                                                                 m_pathSQLs,
                                                                 m_oledbCNString)
                ' get ship to information
                Dim shipToInfo As ShipTo = myCommon.GetPOShipTo(m_selectedPO.Id,
                                                                m_vendorInfo,
                                                                m_pathSQLs,
                                                                m_oledbCNString)


                m_selectedPO.GetPOLines(m_vendorInfo, m_pathSQLs, ORDBData.DbUrl, True)
                m_selectedPO.Vendor = m_vendorInfo
                m_selectedPO.BuyerInfo = buyerInfo
                m_selectedPO.ShipToInfo = shipToInfo

                m_voucher = Voucher.CreateVoucherForPO(m_selectedPO)

                m_voucher.PO = m_selectedPO
                m_voucher.Vendor = m_vendorInfo
                m_voucher.Buyer = buyerInfo
                m_voucher.ShipTo = shipToInfo
                Session("CurrentPOTotal") = m_voucher.ComputedInvoiceAmount

                mySessionState.VoucherInfo = m_voucher
                'VoucherHeaderInfo()
            Else

            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Function VoucherHeaderInfo(InvoiceID As String, Invoice_DT As String, TaxAmount As Double)
        Try
            ' postback or not, create a copy of misc charges list
            m_voucher = mySessionState.VoucherInfo

            mySessionState.SearchPageSelectedPO = m_voucher.PO

            m_currentMiscChargeList.Clear()

            Dim bIsAdd As Boolean = False
            For Each itm As ItemCharge In mySessionState.AdditionalVendorCharges
                bIsAdd = True
                ' "sales tax" misc charge is ONLY for Kelloggs' PO
                '   - erwin 2009.05.04
                If itm.Id = myCommon.MISC_CHARGE_ID_SALES_TAX Then
                    ''bIsAdd = (m_selectedPO.BusinessUnit = "KEL00")
                    bIsAdd = (m_voucher.PO.BusinessUnit = "KEL00")
                End If
                If bIsAdd Then
                    m_currentMiscChargeList.Add(itm)
                End If
            Next

            ' postback or not ...
            ' (1) always update the invoice Id value from textBox
            m_voucher.InvoiceId = InvoiceID
            ' (2) invoice date
            m_voucher.InvoiceDate = ""

            Try
                Dim dtEntered As Date
                dtEntered = CDate(Invoice_DT.Trim)
                m_voucher.InvoiceDate = dtEntered.ToString("MM/dd/yyyy")
            Catch ex As Exception
            End Try

            Session("InvoiceAmount") = m_voucher.ComputedInvoiceAmount
            m_voucher.InvoiceAmount = m_voucher.ComputedInvoiceAmount
            mySessionState.VoucherInfo = m_voucher

            ' persist (update) PO information across postbacks
            mySessionState.SearchPageSelectedPO = m_selectedPO

            If Session("PODetailViewBU") = "WAL00" Then

                Session("InvoiceTax") = TaxAmount
                m_voucher.SalesTax = TaxAmount
            End If

        Catch ex As Exception

        End Try
    End Function

    Private Sub Statgrid_ItemCommand(sender As Object, e As Telerik.Web.UI.GridCommandEventArgs) Handles Statgrid.ItemCommand
        If e.CommandName = "VoucherConfirmation" Or e.CommandName = "InvoiceConfirmation" Then
            Try
                Dim item As GridDataItem = TryCast(e.Item, GridDataItem)
                Dim InvoiceID As String = item("INVOICE_ID").Text.Trim
                Dim strPO As String = item("PO_ID").Text.Trim
                Dim VendorID As String = Session("VENDOR_VP")
                Dim Invoice_DT As String = item("INVOICE_DATE").Text.Trim
                Dim sqlquery As String = "select VO.VOUCHER_ID, VO.BUSINESS_UNIT, VO.SALETX_AMT from SYSADM8.PS_VOUCHER VO where VO.INVOICE_ID= '" + InvoiceID + "' and VO.VENDOR_ID= '" + VendorID + "'"
                Dim ds As DataSet
                ds = ORDBData.GetAdapterSpc(sqlquery)
                If Not ds Is Nothing Then
                    If ds.Tables(0).Rows.Count() > 0 Then
                        Dim VendorKey As String = ds.Tables(0).Rows(0).Item("VOUCHER_ID").ToString()
                        Dim strPOBU As String = ds.Tables(0).Rows(0).Item("BUSINESS_UNIT").ToString()
                        Dim TaxAmount As Double = ds.Tables(0).Rows(0).Item("SALETX_AMT")
                        Session("ISCARTPANEL") = "Y"
                        Session("PODetailViewBU") = strPOBU
                        getPOVoucherDetails(strPO, VendorID, strPOBU)
                        VoucherHeaderInfo(InvoiceID, Invoice_DT, TaxAmount)
                        Response.Redirect("~/Supplier/InsiteOnlineVoucherEntry/CartConfirm_VP.aspx?doneURL=Supplier/SDIVendor.aspx&REF_URL=" & VendorKey)

                    End If
                End If
            Catch ex As Exception

            End Try

        End If
        If e.CommandName = "Select" Then '8/9/2012 
            Dim item As GridDataItem = TryCast(e.Item, GridDataItem)

            TextBox1.Text = item("INVOICE_ID").Text ' 21 is column, voucherID, hidden, don't forget hidden colmns
            ColumnToImageService = item("INVOICE_ID").Text '  "2770043341"   ' 

            '------------webservice
            'http://apweb.isacs.com:8080/portal/APGetImage?WSDL

            Dim userCredentials3 As New com.isacs.apwebMy2.UserCredentials() '  com.isacs.apweb2.UserCredentials  'ServiceReference1.UserCredentials
            userCredentials3.userName = "captiva.admin"
            userCredentials3.password = "Captiva**Admin"

            Dim imageCriteria2 As New com.isacs.apwebMy2.APGetImageCriteria() '  com.isacs.apweb2.APGetImageCriteria '  ServiceReference1.APGetImageCriteria
            'imageCriteria2.invoiceNumber = "2770043341"   '   "320695398"
            imageCriteria2.invoiceNumber = item("INVOICE_ID").Text
            imageCriteria2.vendorID = Session("VENDOR_VP")

            Dim ImageService As New com.isacs.apwebMy2.APGetImageService() '  com.isacs.apweb2.APGetImageService 'ServiceReference1.APGetImageServiceClient

            Try
                ImageURL = "E" 'empty
                ImageURL = ImageService.getInvoiceImageAsURL(imageCriteria2, userCredentials3)
            Catch
                ImageURL = "E" 'empty
            End Try

            If ImageURL.Length > 1 Then
                '' OLD NOT WORKING SERVICE
                'Response.Redirect("http://apweb.isacs.com:8080/portal/jsp/downloadInvoice2.jsp?fileName=\\apweb.isacs.com\APxPortalTemp" & bstring) '11/5/2012 may be too long to send to image viewer. pdf files are coming up
                'Response.Redirect("http://apweb.isacs.com:8080/portal/jsp/downloadInvoice2.jsp?fileName=" & ImageURL)

                Dim tmp As String = "http://apweb.isacs.com:8080" & ImageURL

                RadWindowShowPDF.NavigateUrl = tmp

                Dim script As String = "function f(){$find(""" + RadWindowShowPDF.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

            Else

                Dim strMessage As New Alert
                ltlAlert.Text = strMessage.Say("**Warning - The system can not find a record with the supplied information.")

            End If

        End If
    End Sub

    Private Sub Statgrid_ItemDataBound(sender As Object, e As Telerik.Web.UI.GridItemEventArgs) Handles Statgrid.ItemDataBound
        Try
            Dim item As GridDataItem = TryCast(e.Item, GridDataItem)
            If e.Item.ItemType = GridItemType.Header Then
                Statgrid.Columns(0).Visible = False '7/6/2012
                Statgrid.Columns(1).Visible = False '7/6/2012

            End If

            If e.Item.ItemType <> GridItemType.Header And e.Item.ItemType <> GridItemType.Footer Then
                '7/6/2012  15 - GROSS_AMT  ;   16 - PYMNT_STATUS
                Dim lnk1payref As LinkButton
                lnk1payref = New LinkButton
                Try
                    lnk1payref = CType(item.FindControl("lnk1payref"), LinkButton)
                Catch ex As Exception
                    lnk1payref = New LinkButton
                    lnk1payref.Text = ""
                End Try

                Dim sStr1 As String = e.Item.Cells(16).Text
                If hashStatus.Contains(sStr1) Then
                    e.Item.Cells(16).Text = hashStatus.Item(sStr1)
                End If

                Dim lnk1 As LinkButton
                lnk1 = New LinkButton
                lnk1 = lnk1payref
                ''Session("PAYREF") = lnk1.Text
                Session("BU_VI2_BRT") = item("BUSINESS_UNIT").Text
                Session("VENDID") = item("REMIT_VENDOR").Text

                'Dim sVendorPath As String = ""  '  "~/Vendor21"
                'sVendorPath = Insiteonline.WebPartnerFunctions.WebPSharedFunc.GetNewVendorPortalPath()
                'lnk1 = sVendorPath & "/insiteonlineVoucher/PayDetail_VP.aspx?siteid=I00&CONAME=" & Session("VENDORNAME_VP") & "&UID=" & Session("VENDOR_VP") & "&IDSPME=VENDOR" & "&PYF=" & Session("PAYREF") & "&BU=" & Session("BU_VI2_BRT") & "&VENDID=" & Session("VENDOR_VP")
                'lnk1.NavigateUrl = "//" & Request.ServerVariables("HTTP_HOST") & Insiteonline.WebPartnerFunctions.WebPSharedFunc.GetVendorWebAppName2() & "/insiteonlineVoucher/PayDetail_VP.aspx?siteid=I00&CONAME=" & Session("VENDORNAME_VP") & "&UID=" & Session("VENDOR_VP") & "&IDSPME=VENDOR" & "&PYF=" & Session("PAYREF") & "&BU=" & Session("BU_VI2_BRT") & "&VENDID=" & Session("VENDOR_VP")
                ''lnk1.Target = "_blank"

                ' MATCH_STATUS - Matched
                If Trim(item("MATCH_STATUS").Text) = "Not Applicable" Then
                    item("MATCH_STATUS").Text = "Matched"
                End If
                Try
                    Dim lbtnVOUCHERID As LinkButton = CType(item.FindControl("lbtnVOUCHERID"), LinkButton)
                    lbtnVOUCHERID.CommandArgument = lbtnVOUCHERID.Text
                    lbtnVOUCHERID.CommandName = "VoucherConfirmation"
                    lbtnVOUCHERID.ToolTip = "Click to view Voucher Confirmation"
                Catch ex As Exception
                End Try

                Try
                    Dim lbtnINVOICEID As LinkButton = CType(item.FindControl("lbtnINVOICEID"), LinkButton)
                    lbtnINVOICEID.CommandArgument = lbtnINVOICEID.Text
                    lbtnINVOICEID.CommandName = "InvoiceConfirmation"
                    lbtnINVOICEID.ToolTip = "Click to view Voucher Confirmation"
                Catch ex As Exception
                End Try
            End If
        Catch ex As Exception

        End Try

    End Sub

    Sub StatGrid_SortCommand(ByVal s As Object, ByVal e As DataGridSortCommandEventArgs)

        'txtSortField.Text = e.SortExpression
        'BindDataGrid()

    End Sub

    Private Sub ViewSearchParams()
        'txtSearchFrom.Text = ""
        'txtSearchTo.Text = ""
        RadDatePickerFrom.Clear()
        RadDatePickerTo.Clear()
        txtSearchCriteria.Text = ""
        'dropSearchBy.SelectedIndex = 0
        'dropSelectReport.SelectedIndex = 0
        lblNoData.Visible = False
        Statgrid.Visible = False
        btnExcel.Visible = False
        chbFreeze.Visible = False
        Panel1.Visible = True
        Panel2.Visible = True
        'lblOR.Visible = True
    End Sub

    Private Sub btnExcel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExcel.Click
        Statgrid.MasterTableView.GetColumn("PaymentIDRef").Visible = False
        Statgrid.ExportSettings.ExportOnlyData = True
        Statgrid.ExportSettings.IgnorePaging = True
        Statgrid.ExportSettings.OpenInNewWindow = True
        Statgrid.ExportSettings.FileName = "buildexcel"
        Statgrid.MasterTableView.ExportToExcel()
        'Dim stringwrite As System.IO.StringWriter
        'stringwrite = cmpDataGridToExcel.DataGridToExcel(Statgrid, Response)

        'Dim strExcel As String = stringwrite.ToString
        'Dim strScript As String = ""

        'Response.Clear()

        'Response.Charset = ""
        ''Response.Cache.SetCacheability(HttpCacheability.NoCache)
        ''set the response mime type for excel
        'Response.ContentType = "application/vnd.ms-excel"
        'Response.AddHeader("Content-Disposition", "attachment; filename=buildexcel.xls")

        'Response.Write(strExcel)

        'Response.End()
    End Sub

    Private Sub btnGo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGo.Click

        If Not Trim(txtSearchCriteria.Text) = "" Then
            txtSearchCriteria.Text = ""
        End If
        dropIndexChanged()
    End Sub

    Private Sub chkunpaidVoucher_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkunpaidVoucher.CheckedChanged
        If Me.chkunpaidVoucher.Checked = True Then
            Me.chkPaidVoucher.Checked = False
        End If
    End Sub

    Private Sub chkPaidVoucher_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkPaidVoucher.CheckedChanged
        If Me.chkPaidVoucher.Checked = True Then
            Me.chkunpaidVoucher.Checked = False
        End If
    End Sub

    Function GetTable() As DataTable
        ' Generate a new DataTable.
        ' ... Add columns.

    End Function

    Private Function C2(p1 As Object, trythis As String) As Integer
        Throw New NotImplementedException
    End Function

    'Sub importVendors()
    '    Dim SQLstrVendorsString As String

    '    Dim ID As String = Session("USERID_VI2_BRT")
    '    Dim VendorsStringA As String = "select vendor_name_short,vndr_name_shrt_usr, name1 from SYSADM8.ps_vendor where vendor_status = 'A' and Vendor_ID = "
    '    Dim VendorsStringB As String = Session("USERID_VI2_BRT")
    '    SQLstrVendorsString = VendorsStringA & VendorsStringB

    'End Sub

    'Public Sub webservices()
    '    'pull the vendor name for the ID listed in the session variable
    '    Dim SQLstrVendorsString As String
    '    Dim VendorsStringA As String = "select upper(Name1) from SYSADM8.ps_vendor where vendor_status = 'A' and Vendor_ID = " 'convert from uppercase
    '    Dim VendorsStringB As String = Session("USERID_VI2_BRT")
    '    SQLstrVendorsString = VendorsStringA & VendorsStringB

    '    WebserviceSearch() 'returns array:invoiceResults

    '    'displayInvoiceResults() 'display to grid
    'End Sub

    'Public Sub WebserviceSearch()
    '    'inputs: dateBegin, dateEnd, vendorName, invoiceStatus , BRTinvoiceNumber 
    '    'output: array:invoiceResults
    '    'http://apweb.isacs.com:8080/portal/APSearch?wsdl

    '    Dim userCredentials As New Insiteonlinevendor.com.isacs.apweb.UserCredentials
    '    userCredentials.userName = "captiva.admin"
    '    userCredentials.password = "Captiva**Admin"

    '    Dim criteria As New Insiteonlinevendor.com.isacs.apweb.apSearchCriteria 'The input message definitions to the searchForInvoices operation are SearchCriteria 
    '    criteria.invoiceDateBegin = DateBegin '"12/12/2001"
    '    criteria.invoiceDateEnd = DateEnd '"12/12/2012"
    '    criteria.invoiceNumber = BRTinvoiceNumber 'need Invoice ID if blank selects all
    '    criteria.vendorName = "" 'SQLstrVendorsString ' gez 7/30/2012 turn on when go live 
    '    criteria.invoiceStatus = "" ' "In Process" '(Paid, Rejected, In Process) I need Rejected and In Process 'turn on when go live
    '    'need Payment ID not need for BRT 
    '    'criteria.vendorName = "" 'better to use venderID, eterate through each to get VendorID

    '    Dim search As New Insiteonlinevendor.com.isacs.apweb.APSearch() 'what need
    '    Dim invoiceResults() As Insiteonlinevendor.com.isacs.apweb.SearchResult 'message type

    '    Try
    '        invoiceResults = search.searchForInvoices(criteria, userCredentials)
    '    Catch
    '        'Label5.Text = "Conversion failed."
    '    End Try

    'End Sub

    'Public Sub displayInvoiceResults() '(ByVal ds As DataSet, ByVal invoiceResults As Array) '
    '    If Not invoiceResults Is Nothing Then 'gez 7/20/2012
    '        Dim Column1Value(invoiceResults.Length - 1) As String
    '        For row As Integer = 0 To UBound(Column1Value) - 1

    '            Dim newCustomersRow As DataRow = ds.Tables(0).NewRow()
    '            newCustomersRow("VOUCHER_ID") = invoiceResults(row).purchaseOrderNumber
    '            'newCustomersRow("NAME1") = imageF 'invoiceResults(row).vendorName 'good
    '            newCustomersRow("INVOICE_ID") = invoiceResults(row).invoiceNumber 'good
    '            newCustomersRow("GROSS_AMT") = invoiceResults(row).invoiceAmount  ' bombs if only one number after decimal
    '            newCustomersRow("INVOICE_DATE") = invoiceResults(row).invoiceDate

    '            'format columns
    '            If invoiceResults(row).actualPayDateSpecified = "false" Then
    '                newCustomersRow("SCHEDULED_PAY_DATE") = ""
    '            Else
    '                newCustomersRow("SCHEDULED_PAY_DATE") = invoiceResults(row).actualPayDateSpecified
    '            End If

    '            If invoiceResults(row).status = "In Process" Then
    '                newCustomersRow("MATCH_STATUS") = "To Be Matched"
    '            Else
    '                newCustomersRow("MATCH_STATUS") = invoiceResults(row).status
    '            End If

    '            'format rows
    '            newCustomersRow("DSCNT_PAY_AMT") = "0.00" 'no value retrned from BRT for this field
    '            newCustomersRow("PAID_AMT_DSCNT") = "0.00" 'no value retrned from BRT for this field

    '            WebserviceImage() 'return a url for each iteration

    '            'format cross ref results
    '            If Me.chkunpaidVoucher.Checked = True Then '7/30/2012 
    '                'only show if "In Process or rejected 
    '                If invoiceResults(row).status = "In Process" Then ds.Tables(0).Rows.Add(newCustomersRow) '7/30/2012 
    '                If invoiceResults(row).status = "Rejected" Then ds.Tables(0).Rows.Add(newCustomersRow) '7/30/2012 
    '            End If
    '        Next row

    '    End If
    'End Sub

    Sub WebserviceImage()
        ' Service URL: http://apweb.isacs.com:8080/portal/APGetImage?wsdl

        Dim userCredentials3 As New Insiteonline.com.isacs.apwebMy2.UserCredentials '  Insiteonlinevendor.com.isacs.apweb2.UserCredentials
        userCredentials3.userName = "captiva.admin"
        userCredentials3.password = "Captiva**Admin"

        Dim imageCriteria2 As New Insiteonline.com.isacs.apwebMy2.APGetImageCriteria '  Insiteonlinevendor.com.isacs.apweb2.APGetImageCriteria
        imageCriteria2.invoiceNumber = "320695398"

        Dim ImageService As New Insiteonline.com.isacs.apwebMy2.APGetImageService  '  Insiteonlinevendor.com.isacs.apweb2.APGetImageService
        'Dim ImageURL As String ' set to global avriable

        Try
            ImageURL = ImageService.getInvoiceImageAsURL(imageCriteria2, userCredentials3)
            'ImageURL = "sdfsdfgdgfd"
        Catch
            TextBox1.Text = "Conversion failed."
        End Try
    End Sub

    Private Sub Statgrid_SortCommand1(sender As Object, e As Telerik.Web.UI.GridSortCommandEventArgs) Handles Statgrid.SortCommand

        txtSortField.Text = e.SortExpression
        BindDataGrid()

    End Sub

    Protected Sub lnk1payref_Click(sender As Object, e As EventArgs)
        Try
            Dim tmp As String = ""
            Dim script As String = ""

            Dim chk As LinkButton = DirectCast(sender, LinkButton)
            Dim itm As GridDataItem = DirectCast(chk.NamingContainer, GridDataItem)
            Dim item As GridDataItem = CType(Statgrid.Items(itm.ItemIndex), GridDataItem)
            Dim PaymentID As String = ""
            PaymentID = item("PYMNT_ID_REF").Text
            Dim sVendorPath As String = ""  '  "~/Vendor21"
            Dim navurl As String = ""
            sVendorPath = Insiteonline.WebPartnerFunctions.WebPSharedFunc.GetNewVendorPortalPath()
            navurl = sVendorPath & "/insiteonlineVoucher/PayDetail_VP.aspx?siteid=I00&CONAME=" & Session("VENDORNAME_VP") & "&UID=" & Session("VENDOR_VP") & "&IDSPME=VENDOR" & "&PYF=" & PaymentID & "&BU=" & Session("BU_VI2_BRT") & "&VENDID=" & Session("VENDOR_VP")

            tmp = navurl


            RadWindowPaymentID.NavigateUrl = tmp
            ''RadWindowPaymentID.OnClientClose = "SubmitNewTicketChangedOnClientClose"  '  "SubmitTicketChangedOnClientClose"

            script = "function f(){$find(""" + RadWindowPaymentID.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub lnkInvid_Click(sender As Object, e As EventArgs)
        Try

        Catch ex As Exception

        End Try
    End Sub

    'PreRender() to change PaymentDate column as Sent To Walmart
    Protected Sub Statgrid_PreRender(sender As Object, e As EventArgs)
        If Session("BU_VI2_BRT") = "WAL00" Then
            For Each col As GridColumn In Statgrid.Columns
                If col.UniqueName = "PayDate" Then col.HeaderText = "Sent To Walmart"
            Next
            Statgrid.Rebind()
        End If

    End Sub

    Private Sub btnResetDate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnResetDate.Click
        Try
            lblErr.Visible = False
            lblErrdate.Visible = False
            RadDatePickerFrom.SelectedDate = Nothing
            RadDatePickerTo.SelectedDate = Nothing
            RadPaymentFrom.SelectedDate = Nothing
            RadPaymentTo.SelectedDate = Nothing
        Catch ex As Exception

        End Try
    End Sub
End Class
