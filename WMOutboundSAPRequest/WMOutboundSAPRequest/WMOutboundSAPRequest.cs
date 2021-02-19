Imports System
Imports System.IO
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Web
Imports System.Web.Mail
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading.Thread
Imports Insiteonline.WebPartnerFunctions.WebPSharedFunc
Imports Insiteonline.ShoppingCartMethods.clsShoppingcart
Imports Insiteonline.ORDBData
Imports Insiteonline.BuildingMenus.BuildMenu
Imports System.Web.UI.WebControls
Imports AjaxControlToolkit
Imports Telerik.Web.UI
Imports System.Configuration
Imports System.Linq
Imports System.Collections
Imports System.Drawing.Color
Imports Insiteonline.clsSerialItem
Imports Insiteonline.WebPartnerFunctions.Notifiers
Imports System.Collections.Generic
Imports Insiteonline.UnilogORDBData

'**********************************************************************************************************
'create Flat file to send to Lyons Magnus (I0531, I0532 for now) - VR 08/16/2018
'**********************************************************************************************************
' change Lyons Magnus (I0531, I0532) processing to create 2 orders: one for Priced item and one for Non-Stock - VR 08/23/2018
' 20181204 YA Added ExideErrorHandling to submit, BuildXMLGrid, removed UpdateCartDatatable from page_load & buttonClickEvent (regular processing) - Ticket 145286
' 20190425 YA added new punchout indicator in session paintColor field - Ticket 154071 
' 20190503 YA new getExemptStatus function and changes to TaxExempt fields on order save - Ticket 154676
' 20191218 YA Added fix for in getExemptStatus for cases where entries don't exist in PS_ISA_BUS_UNIT_OM (i.e. Henkel A26-A29)
' 20200228 YA new rules for Stock items - blank vendor_id and blank itm_id_vndr Ticket SDI-8013 
'**********************************************************************************************************

Partial Class shoppingcart
    Inherits System.Web.UI.Page

    Public HOME As String
    Public HOME1 As String
    Public CARTIMG As String

    Protected WithEvents ReqChgCD As System.Web.UI.WebControls.RequiredFieldValidator
    Protected WithEvents ItemID As System.Web.UI.WebControls.Label
    Protected WithEvents ltlAlert As System.Web.UI.WebControls.Literal
    Protected WithEvents TextBox1 As System.Web.UI.WebControls.TextBox
    Protected WithEvents SPAN1 As System.Web.UI.HtmlControls.HtmlGenericControl
    Protected WithEvents RequiredFieldValidator5 As System.Web.UI.WebControls.RequiredFieldValidator

    Protected WithEvents TextBox2 As System.Web.UI.WebControls.TextBox
    'Protected WithEvents BasicMenu As CYBERAKT.WebControls.Navigation.ASPnetMenu 

    Dim strChgCdSep As String

    Dim ShoppingCartcls As Insiteonline.ShoppingCartMethods.clsShoppingcart = New Insiteonline.ShoppingCartMethods.clsShoppingcart

    Private m_logger As ApplicationLogger = Nothing
    Private m_loggerMJ1 As ApplicationLogger = Nothing

    Private Enum dtgCartColumns As Integer
        itemId = 0
        itemDescription = 3
        mfg = 4
        mfgPartNo = 5
        itemUOM = 6
        qtyToOrder = 7
        itemPrice = 8
        itemExtendedPrice = 7
        rfq = 8
        [deleteFlag] = 9
        [templateColumn1] = 10
        itemId_hidden = 11
        lineItemInfo = 12
        supplierId_hidden = 13
        supplierPartNo_hidden = 14
        pricePO_hidden = 15
        baseItemPriceAndCurrency_hidden = 16
        itemAttributes = 17
        uniqNum_hidden = 18
        supplierPartNoAux_hidden = 19
    End Enum

    Private Const SESSION_SITE_CURRENCY As String = "__siteCurrency"
    Private m_siteCurrency As sdiCurrency = Nothing
    'Private m_conversionRate As sdiConversionRate = Nothing

#Region " Web Form Designer Generated Code "

    Private Property style As Object

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

#Region "Public Declarations"

    Dim strProductViewID As String
    Dim strUserid As String
    Dim strPlusItemID As String
    Dim strBU As String
    Dim iParntId As Integer
    Dim strAgent As String
    Dim strTaxFlag As String
    Public strblankrow As String
    Public strblankrowSDM As String
    Public strUndo As String

#End Region

#Region "Page Load"

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ''ltlAlert.Text = str_Message.Say("You couldn't able to access the Shopping cart panel. Since you have not accepted the agreement.")

        ''Variable to check the summary page navigation
        Dim bolIsValidOrder As Boolean = False
        Try
            Session("SCREENNAME") = "shoppingcart.aspx"

            txtSelectedSerialID.Style.Add("visibility", "hidden")

            If (Not Page.IsPostBack) Then
                'Ticket 117759 user can enter order no depending on status of PS_ISA_ENTERPRISE - MAN_ORDER_NO field - 20170626 Yury
                If IsDBNull(Session("MANORDNO")) = False And Trim(Session("MANORDNO")) <> "" Then
                    Panel2.Visible = True
                    If Session("MANORDNO") = "A" Then
                        txtOrderNO.MaxLength = 10
                    Else
                        txtOrderNO.MaxLength = 7
                    End If
                    If Session("ORDLNRENUM") = "Y" Then
                        hdfChangeLineNums.Value = "Y"
                    End If

                Else
                    Panel2.Visible = False
                    hdfChangeLineNums.Value = "N"
                End If
                If Session("ZEUSNOCATALOGSITE") = "Y" Then
                    Panel1.Visible = False
                    FavsMenu.Visible = False
                    cmdEdtMyFavs.Visible = False
                End If

                ''New Zeus Test Flag to hide due date,priority 
                If Session("ZEUSNOCATALOGSITE") = "Y" And Session("ZEUS_SITE") = "Y" Then
                    liReqDate.Visible = False
                    divPriority.Visible = False
                    hdfZeusNonCata.Value = "Y"
                Else
                    hdfZeusNonCata.Value = "N"
                End If
            End If


            'Setting page title
            'Dim titleSPCMaster As HtmlTitle = CType(Master.FindControl("titleSPCMaster"), HtmlTitle)
            'titleSPCMaster.Text = "Shopping Cart"
            Dim lblTitle As Label = CType(Master.FindControl("lblTitle"), Label)
            lblTitle.Text = "Shopping Cart"  '  titleSPCMaster.Text

            'Cart Count Begin
            Dim intItems As Integer
            Dim dstCart3 As DataTable
            If Session("CART") Is Nothing Then
                intItems = 0
            Else
                dstCart3 = Session("Cart")
                dstCart3.AcceptChanges()
            End If
            'Dim _objCookie As HttpCookie = Request.Cookies("ItemsInCart")
            'If (Not _objCookie Is Nothing) Then
            '    If (Not _objCookie("CartCount") Is Nothing) Then
            '        _objCookie("CartCount") = dstCart3.Rows.Count
            '        Response.Cookies.Add(_objCookie)
            '    End If
            'End If

            Dim cartCount As String = Session("ItemsInCart")
            If (Not cartCount Is Nothing) Then
                If dstCart3 IsNot Nothing Then
                    Session("ItemsInCart") = dstCart3.Rows.Count
                End If
            End If
            If Not Session("SearchValue") Is Nothing Then
                lnkbtnBack.Visible = True
            End If
            'Response.Cache.SetCacheability(HttpCacheability.NoCache)

            If Session("SDIEMP") = "" Or Session("USERID") = "" Then
                Session.RemoveAll()
                Response.Redirect("default.aspx")
            End If

            Dim strMessage As New Alert

            Dim hashPrivs As Hashtable
            hashPrivs = New Hashtable

            If Session("AGENTUSERID") = "" Then
                strBU = Session("BUSUNIT")
                strAgent = Session("USERID")
            Else
                strBU = Session("AGENTUSERBU")
                strAgent = Session("AGENTUSERID")
                rblOrderSource.Visible = True
            End If

            ' checks/save/retrieve site currency
            m_siteCurrency = Nothing
            If Page.IsPostBack Then
                ' retrieve from session var
                Try
                    m_siteCurrency = CType(Session(SESSION_SITE_CURRENCY), sdiCurrency)
                Catch ex As Exception
                End Try
            End If
            If (Not Page.IsPostBack) Or
               (m_siteCurrency Is Nothing) Then
                m_siteCurrency = sdiMultiCurrency.getSiteCurrency(strBU)
                Session(SESSION_SITE_CURRENCY) = m_siteCurrency

                txtForContShop.Text = "NO"
            End If

            If IsAEES(Session("BUSUNIT")) Then
                lblEmpid.Text = "Requisitioner Name:"
            End If

            lblQuickqty.Style.Add("visibility", "hidden")
            txtItemChgCodeHide.Style.Add("visibility", "hidden")
            txtItemChgCodeItem.Style.Add("visibility", "hidden")
            txtEmpChgCodeHide.Style.Add("visibility", "hidden")
            txtEmpChgCodeItem.Style.Add("visibility", "hidden")
            txtMachineRowHide.Style.Add("visibility", "hidden")
            txtMachineRowItem.Style.Add("visibility", "hidden")
            txtFavItemIDHide.Style.Add("visibility", "hidden")
            txtEmpNotesHide.Style.Add("visibility", "hidden")
            txtEmpMoteRowItem.Style.Add("visibility", "hidden")

            lnkContShop1.NavigateUrl = Session("DEFAULTPAGE").ToString()
            lnkContShop2.NavigateUrl = Session("DEFAULTPAGE").ToString()

            lblPriceVary.Visible = False
            divPriceVary.Visible = False

            Dim objPunchIN As New clsPunchin(Session.SessionID)

            Dim strCustID As String = ""
            Try
                strCustID = objPunchIN.CUSTID
                If strCustID Is Nothing Then
                    strCustID = ""
                End If
            Catch ex As Exception
                strCustID = ""
            End Try
            If Not Trim(strCustID) = "" Then
                Session("PUNCHIN") = "YES"
            End If

            Dim sWrkOrd1 As String = objPunchIN.WorkOrderA
            If Trim(sWrkOrd1) <> "" Then
                Me.txtWorkOrder.Text = sWrkOrd1
            End If

            If Session("SDIEMP") = "SDI" And Session("AGENTUSERID") = "" Then
                lblEmpid.Text = "Employee ID: (by name)"

            End If
            If lblQuickqty.Text = "C" Then
                lblQuickqty.Text = "0"
            End If
            If lblQuickqty.Text > "0" And Not txtItemID.Text = "" Then
                'ltlAlert.Text = strMessage.Say(txtItemID.Text)
                Dim strCpitemid As String
                If txtItemID.Text.Length < 6 Then
                    strCpitemid = getcpjunc(RTrim(txtItemID.Text.ToUpper))
                    If strCpitemid Is Nothing Or strCpitemid = "0" Then
                        If IsNumeric(txtItemID.Text.ToUpper) And txtItemID.Text.Length = 7 Then
                            strCpitemid = getcpclassavail(RTrim(txtItemID.Text.ToUpper))
                        End If
                    End If
                    If strCpitemid Is Nothing Or strCpitemid = "0" Then
                        strCpitemid = getMasterItm(RTrim(txtItemID.Text.ToUpper))
                    End If
                ElseIf Not txtItemID.Text.ToUpper.Substring(0, 6) = "NONCAT" Then
                    strCpitemid = getcpjunc(RTrim(txtItemID.Text.ToUpper))
                    If strCpitemid Is Nothing Or strCpitemid = "0" Then
                        If IsNumeric(txtItemID.Text.ToUpper) And txtItemID.Text.Length = 7 Then
                            strCpitemid = getcpclassavail(RTrim(txtItemID.Text.ToUpper))
                        End If
                    End If
                    If strCpitemid Is Nothing Or strCpitemid = "0" Then
                        strCpitemid = getMasterItm(RTrim(txtItemID.Text.ToUpper))
                    End If
                End If
                Dim dstquick As DataTable = appendquickgrid(strCpitemid, lblQuickqty.Text, RTrim(txtItemID.Text))
                WriteToUserCart(Session("USERID"), Session("BUSUNIT"))
                buildXMLGrid()
                If dstquick.Rows.Count > 0 Then
                    'dtgCart.DataSource = dstquick
                    'dtgCart.DataBind()
                    rgCart.DataSource = dstquick
                    rgCart.DataBind()
                End If
                lblQuickqty.Text = "0"
                txtItemID.Text = ""
                Response.Redirect("shoppingcart.aspx", False)
            End If

            If rgCart.Items.Count > 0 Then
                If PopulateGridFromHiddenControls() Then

                End If
                'If Not Session("Cart") Is Nothing Then
                '    If Page.IsPostBack Then
                '        updateCartDatatable()
                '    End If

                'End If
            End If
            txtItemChgCodeHide.Text = ""
            txtItemChgCodeItem.Text = ""
            txtEmpChgCodeHide.Text = ""
            txtEmpChgCodeItem.Text = ""

            txtMachineRowHide.Text = ""
            txtMachineRowItem.Text = ""


            If Not Trim(txtFavItemIDHide.Text) = "" Then
                UpdateFavsmenu(txtFavItemIDHide.Text)
            End If
            txtFavItemIDHide.Text = ""
            Session.Remove("NSTKINFO")
            '''''''''''''''zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz
            If Session("PUNCHIN") = "YES" Then
                lblCustNotesMy1.Visible = False

                If Not Page.IsPostBack Then
                    Call HidePunchInFields()
                End If
            End If
            If Not Page.IsPostBack Then
                If strBU = "I0601" Or strBU = "I0602" Then
                    If Session("BuyerCookie") Is Nothing And Session("PUNCHIN") = "YES" Then
                        divCC.Visible = False
                        Session("CHARGECODE") = "N"
                    Else
                        divCC.Visible = True
                    End If
                End If
            End If

            CheckCHGCDWOValidation(False)

            If Not Page.IsPostBack Then
                Session.Remove("xmldata")
                Session.Remove("rfqxml")
                Session.Remove("PunchINxml")
                Session.Remove("xmlToUrl")

                If Session("ShowPrice") = "0" Then
                    spOrderText.Visible = False
                    spOrderValue.Visible = False
                Else
                    spOrderText.Visible = True
                    spOrderValue.Visible = True
                End If

                buildfavsTelerik(FavsMenu)

                BuildDropDownMachine()
                buildXMLGrid()

                hashPrivs = getprivhashtable(strAgent, strBU, "Y")

                If Not hashPrivs.ContainsKey("ADDCART") And
                    Not hashPrivs.ContainsKey("NONE") Then
                    Response.Redirect(Session("DEFAULTPAGE").ToString() & "?CART=NO")
                ElseIf Session("TermsnConditionsAccept") <> "Y" Then
                    Response.Redirect(Session("DEFAULTPAGE").ToString() & "?CART=NO")
                End If
                If hashPrivs.ContainsKey("TESTMODE") Then
                    Session("TESTMODE") = "TEST"
                End If
                WebLog()

                BuildEmpDropDown()
                Customvalidator1.Enabled = False
                Customvalidator2.Enabled = False
                CustomValidator3.Enabled = False
                Customvalidator4.Enabled = False
                CompareValidator2.Enabled = False
                'RequiredFieldValidator1.Enabled = False
                'Requiredfieldvalidator2.Enabled = False
                Requiredfieldvalidator3.Enabled = False
                RequiredFieldValidator4.Enabled = False
                ' Requiredfieldvalidator6.Enabled = False
                'txtReqByDate.Attributes.Add("onchange", "CheckReqByDate()")
                'SetFocus(txtReqByDate)
                'SetFocus(RadDatePickerReqByDate)

                BuildDropDownCust("", strBU)
                BuildDropDownType(strBU)

            End If
            If cmbMachine.Visible = False Then
                strblankrow = "N"
            Else
                strblankrow = "Y"
            End If
            txtOrderTot.Text = Math.Round(getOrderTot(), 2).ToString("####,###,##0.00")
            Dim objEnterprise As New clsEnterprise(strBU)
            If Not objEnterprise.CustPrfxFlag = "Y" Then
                'dropEmpID.AutoPostBack = False
            End If
            If dropShipto.Visible Then
                'check objEnterprise is Ship To required
                Session("ShipToReqd1") = objEnterprise.ShipToFlag()
                If objEnterprise.ShipToFlag() = "Y" Then
                    lblShipto.Text = "*ShipTo:"
                Else
                    lblShipto.Text = "ShipTo:"
                End If
                If Session("PUNCHIN") = "YES" Then
                    lblShipto.Text = "ShipTo:"
                End If
            End If

            Try

                If Not Session("Cart") Is Nothing Then
                    Dim dtCart As DataTable = CType(Session("Cart"), DataTable)
                    ' Dim _objCookie As HttpCookie = New HttpCookie("ItemsInCart")

                    ' _objCookie("CartCount") = dtCart.Rows.Count
                    ' Response.Cookies.Add(_objCookie)
                    Session("ItemsInCart") = dtCart.Rows.Count
                    'lblCheck1.Text = "dtCart - 2: " + Convert.ToString(dtCart.Rows.Count)
                    'lblCheck2.Text = "_objCookie - 2: " + Convert.ToString(_objCookie("CartCount"))

                    Try
                        Dim lnkCartItems As LinkButton = CType(Master.FindControl("lnkCartItems"), LinkButton)
                        lnkCartItems.Text = "  Item(s) in Cart: " & Convert.ToString(Session("ItemsInCart")) '_objCookie("CartCount")
                    Catch ex As Exception

                    End Try
                End If
            Catch ex As Exception

            End Try

            chbPriority.Visible = True
            If Not Session("PUNCHIN") Is Nothing Then
                If Session("PUNCHIN") = "YES" Then
                    If sWrkOrd1 <> "" Then

                    End If
                Else
                    ' code to block ASCEND users logged through DEFAULT.ASPX to use Shopping cart
                    Call BlockAscendUsersShopCart(strBU)
                End If
            Else
                ' code to block ASCEND users logged through DEFAULT.ASPX to use Shopping cart
                Call BlockAscendUsersShopCart(strBU)
            End If

            If Session("CREDITCARDPROCSite") = "Y" Then
                BtnSubmit.Text = "Check Out"
                ''Disabled the submit button if no record in cart
                If Not Session("Cart") Is Nothing Then
                    Dim dtCart As DataTable = CType(Session("Cart"), DataTable)
                    If dtCart.Rows.Count = 0 Then
                        BtnSubmit.Enabled = False
                    End If
                End If
            Else
                BtnSubmit.Text = "Submit Order"
            End If

            'Page.ClientScript.GetPostBackEventReference(BtnSubmit, "onclick")
            'If Not Request.Form("__eventtarget") Is Nothing And Not Request.Form("__eventargument") Is Nothing Then
            '    Dim eventArg As String = Request.Form("__eventargument")
            '    Dim eventTarget As String = Request.Form("__eventtarget")
            '    If eventTarget = "btnSubmit" AndAlso eventArg = "onclick" Then
            '        ButtonClickEvent()
            '        '' Only for the regular updateNSTKDB process when the credit card flag set
            '        If Session("CONFTARGET") = "ShoppingCartPayment.aspx" Then
            '            bolIsValidOrder = True
            '            Exit Try
            '        End If
            '    End If
            'End If
        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart - Page_Load event. User ID: " & Session("USERID") & "; " & vbCrLf &
                          "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                           "Error message: " & ex.Message
            If strErrorFromEmail.Contains("Thread was being aborted") Then
                'do not send error e-mail
            Else
                SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart Page Load Event")
            End If
        End Try
        '' To resolve the thread abort issue when using redirect in try catch bloack
        'If bolIsValidOrder Then
        '    Response.Redirect("ShoppingCartPayment.aspx")
        'End If
    End Sub

    Private Sub BlockAscendUsersShopCart(ByVal strBU1 As String)
        If Session("ISABlockShoppingCart") = "Y" Then
            Session.Remove("Cart")

            'Try
            '    Dim _objCookie As HttpCookie = Request.Cookies("ItemsInCart")
            '    If Not _objCookie Is Nothing Then
            '        _objCookie.Expires = Date.Now.AddDays(-10)
            '        Response.Cookies.Add(_objCookie)
            '        Request.Cookies.Remove("ItemsInCart")
            '    End If
            '    Request.Cookies.Clear()
            'Catch ex As Exception
            '    Request.Cookies.Clear()
            'End Try

            Dim cartCount As String = Session("ItemsInCart")
            If Not cartCount Is Nothing Then
                Session("ItemsInCart") = String.Empty
            End If

            Response.Redirect("//" & Request.ServerVariables("HTTP_HOST") & GetWebAppName1() & "/" & Session("DEFAULTPAGE").ToString() & "?GetAccess21=NoShopCartAccessForCust")

        End If
    End Sub

    Private Function GetUpennMomsBus() As String

        Dim strUPennMomsBUs As String = "~I0206~I0470~I0471~I0472~I0473"
        Try
            strUPennMomsBUs = ConfigurationSettings.AppSettings("UPennMomsBUs")
        Catch ex As Exception
            strUPennMomsBUs = "~I0206~I0470~I0471~I0472~I0473"
        End Try

        Return strUPennMomsBUs
    End Function

#End Region

#Region "Private Events"

    ''Protected Sub btnGo_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnGo.Click
    ''    If txtSearch.Value <> "search..." Then
    ''        Dim bLtrSearchErrorVisible As Boolean = False
    ''        ltrSearchError.Text = ""
    ''        Try
    ''            Call HeaderSearchGoButtonClick(dropSearch.SelectedValue, dropSearchOptions.SelectedValue, txtSearch.Value, _
    ''                bLtrSearchErrorVisible, ltrSearchError.Text, lbtnDidYouMean.Text, hdnSuggestedText.Value)
    ''            ltrSearchError.Visible = bLtrSearchErrorVisible

    ''            If Not String.IsNullOrEmpty(lbtnDidYouMean.Text) Then
    ''                HideShowSpellSuggestions(True)
    ''            End If

    ''            'If Not txtSearch.Value.Trim() = "" Then
    ''            '    Try
    ''            '        updatecriteriaCounter()
    ''            '    Catch ex As Exception
    ''            '    End Try
    ''            '    Session("SearchON") = dropSearch.SelectedValue
    ''            '    Dim CorrectSpelling As Boolean
    ''            '    Dim searchRecordCount As Integer
    ''            '    Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
    ''            '    'Fix for multiple spaces inbetween words. out of memory ..
    ''            '    Dim formattedString As String = Regex.Replace(txtSearch.Value, "\s+", " ")
    ''            '    txtSearch.Value = formattedString
    ''            '    CorrectSpelling = CheckSpellings()
    ''            '    If CorrectSpelling = True Then
    ''            '        Dim queryText As String = String.Format("BCSmpClassName=""{0}""", txtSearch.Value.Trim())
    ''            '        searchRecordCount = GetRecordCount(queryText)

    ''            '        If searchRecordCount > 0 Then
    ''            '            Response.Redirect("http://" & currentApp.Request.ServerVariables("HTTP_HOST") & "/" & Convert.ToString(ConfigurationSettings.AppSettings"WebAppName")) & "/SearchResults.aspx?key=" & HttpUtility.UrlEncode(txtSearch.Value) & "&OPT=" & dropSearchOptions.SelectedValue)
    ''            '        Else
    ''            '            searchRecordCount = GetRecordCount(txtSearch.Value.Trim())

    ''            '            If searchRecordCount > 0 Then
    ''            '                Response.Redirect("http://" & currentApp.Request.ServerVariables("HTTP_HOST") & "/" & Convert.ToString(ConfigurationSettings.AppSettings("WebAppName")) & "/SearchResults.aspx?key=" & HttpUtility.UrlEncode(txtSearch.Value) & "&OPT=" & dropSearchOptions.SelectedValue)
    ''            '            Else
    ''            '                'IF NOT ALL MATCH THEN ANY OF MATCH IS CHECKED - (S.S)
    ''            '                searchRecordCount = GetRecordCount(String.Format("ANY({0})", txtSearch.Value.Trim()))
    ''            '                If searchRecordCount > 0 Then
    ''            '                    Response.Redirect("http://" & currentApp.Request.ServerVariables("HTTP_HOST") & "/" & Convert.ToString(ConfigurationSettings.AppSettings ("WebAppName")) & "/SearchResults.aspx?key=" & HttpUtility.UrlEncode(txtSearch.Value) & "&OPT=" & dropSearchOptions.SelectedValue)
    ''            '                Else
    ''            '                    ltrSearchError.Visible = True
    ''            '                    ltrSearchError.Text = "No items found that match your search criteria"
    ''            '                    Dim pageURL As String
    ''            '                    pageURL = Request.Url.AbsoluteUri
    ''            '                    If pageURL.Contains("SearchResults.aspx") Then
    ''            '                        Response.Redirect("http://" & currentApp.Request.ServerVariables("HTTP_HOST") & "/" & Convert.ToString(ConfigurationSettings.AppSettings("WebAppName")) & "/SearchResults.aspx?key=" & HttpUtility.UrlEncode(txtSearch.Value) & "&OPT=" & dropSearchOptions.SelectedValue)
    ''            '                    End If
    ''            '                    Session.Remove("SearchON")
    ''            '                End If
    ''            '            End If
    ''            '        End If
    ''            '    End If
    ''            'Else
    ''            '    ltrSearchError.Visible = True
    ''            '    ltrSearchError.Text = "Search Criteria Required"
    ''            '    'Dim strMessage As New Alert
    ''            '    'ltlAlert.Text = strMessage.Say("Search Criteria Required")
    ''            'End If
    ''        Catch ex As Exception

    ''        End Try
    ''    End If
    ''End Sub

    'Private Sub btnGetName_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnGetName.Click
    '    getEmpIDName()
    'End Sub

    Private Sub btnRecalc_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRecalc.Click
        If Session("BUSUNIT") = "" Then
            Session.RemoveAll()
            Response.Redirect("default.aspx")
        End If
        If Not Session("Cart") Is Nothing Then
            updateCartDatatable()

            txtForContShop.Text = "YES"
        End If

    End Sub

    Private Sub BtnSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnSubmit.Click

        Dim bolIsValidOrder As Boolean = False
        Session("ShopCWrkOrd") = txtWorkOrder.Text
        ButtonClickEvent()

        If Not Session("CONFTARGET") = "" Then
            Response.Redirect(Session("CONFTARGET"))
        End If

    End Sub

    'Private Sub BtnSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BtnSubmit.Click
    '    ''''''''''''''''''''zzzzzzzzzzzzz
    '    Dim I As Integer
    '    Dim X As Integer
    '    Dim strMessage As New Alert
    '    If Session("SUBMIT") = "TRUE" Then
    '        Session.Remove("SUBMIT")
    '        Exit Sub
    '    End If
    '    Session("SUBMIT") = "TRUE"
    '    If Session("BUSUNIT") = "" Then
    '        Session.RemoveAll()
    '        Response.Redirect("default.aspx")
    '    End If
    '    If dtgCart.Items.Count = 0 Then
    '        ltlAlert.Text = strMessage.Say("No items in cart")
    '        Session.Remove("SUBMIT")
    '        Exit Sub
    '    End If
    '    Customvalidator1.Enabled = True
    '    Customvalidator2.Enabled = True
    '    CustomValidator3.Enabled = True
    '    If dropShipto.Visible Then
    '        Try
    '            If Session("ShipToReqd1") = "Y" Then
    '                Customvalidator4.Enabled = True
    '            Else
    '                Customvalidator4.Enabled = False
    '            End If
    '        Catch ex As Exception
    '            Customvalidator4.Enabled = False
    '        End Try
    '    Else
    '        Customvalidator4.Enabled = False
    '    End If
    '    CompareValidator2.Enabled = True

    '    ''Dim strUPennMomsBUs As String = GetUpennMomsBus()

    '    ''If (strUPennMomsBUs.IndexOf(strBU)) > -1 Then
    '    ''    Requiredfieldvalidator6.Enabled = True
    '    ''    If IsAEES(strBU) Then
    '    ''        RequiredFieldValidator1.Enabled = True
    '    ''    End If
    '    ''Else
    '    ''    If checkLineItemChgCd() = False Then
    '    ''        RequiredFieldValidator1.Enabled = True
    '    ''    End If
    '    ''End If

    '    If IsAlertSerialIDMissing() Then
    '        Exit Sub
    '    End If

    '    Requiredfieldvalidator2.Enabled = True
    '    'If checkLineEmpChgCd() = False Then
    '    Requiredfieldvalidator3.Enabled = True
    '    'End If

    '    If Not Session("AGENTUSERID") = "" Then
    '        RequiredFieldValidator4.Enabled = True
    '    End If

    '    If Session("PUNCHIN") = "YES" Then
    '        RequiredFieldValidator1.Enabled = False
    '    End If
    '    CheckCHGCDWOValidation()

    '    Page.Validate()

    '    'Dim bValid As Boolean = False

    '    'If Not Session("PUNCHIN") = "YES" Then
    '    '    Page.Validate()
    '    '    bValid = IsValid
    '    'Else
    '    '    bValid = True
    '    'End If

    '    ''''''''''''''''''''''zzzzzzzzzzzzzzzzzzzzzzzzzz
    '    If IsValid Then   '  If bValid Then
    '        'check work order for '
    '        If Trim(txtWorkOrder.Text) <> "" Then
    '            txtWorkOrder.Text = Replace(Trim(txtWorkOrder.Text), "'", "''")
    '        End If

    '        Dim objPunchIN As New clsPunchin(Session.SessionID)
    '        Dim strCustID As String = " "  '  objPunchIN.CUSTID
    '        Try
    '            strCustID = objPunchIN.CUSTID
    '        Catch ex As Exception
    '            strCustID = " "
    '        End Try

    '        If Not Trim(strCustID) = "" Then
    '            Dim strPunchOutUrl As String = ""
    '            strPunchOutUrl = objPunchIN.PIURL
    '            Dim response_doc As String = buildPunchINcXML()

    '            'VR 11/14/2013 - changed 01/13/2016
    '            Dim strEmailAscend As String = Session("EMAIL")  '  objPunchIN.Email()
    '            'Dim strWorkOrderA As String = objPunchIN.WorkOrderA()

    '            If objPunchIN.PIStatus = "A" Then
    '                '   (1) check for any "zero-priced" and "priced" item(s)
    '                '   (2) if "zero-priced" item(s) exists, save into INTFC tables
    '                '       2.1. generate req id
    '                '       2.2. save
    '                '   (3) do PIPost ONLY IF "priced" item(s) exists in shopping cart
    '                Dim bIsNeedQuoteItems As Boolean = False
    '                Dim bIsWithPricedItems As Boolean = False
    '                '// checks the shopping cart for any "items that needs quote" and
    '                '//     items that are priced and set flags
    '                ''VR 11/14/2013 Commented out - logic changed
    '                'If strCustID = "90523" Or _
    '                '    strCustID = "90524" Or _
    '                '    strCustID = "90525" Or _
    '                '    strCustID = "90526" Or _
    '                '    strCustID = "90527" Then

    '                '    bIsWithPricedItems = True

    '                'End If
    '                Dim dtCart As DataTable = Nothing
    '                Try
    '                    dtCart = CType(Session("Cart"), DataTable)
    '                Catch ex As Exception
    '                End Try
    '                If Not (dtCart Is Nothing) Then
    '                    ' commented out by VR 11/14/2013
    '                    'If Not (strCustID = "90523" Or _
    '                    '        strCustID = "90524" Or _
    '                    '        strCustID = "90525" Or _
    '                    '        strCustID = "90526" Or _
    '                    '        strCustID = "90527") Then

    '                    ' check/set quote flag and priced item flag
    '                    If dtCart.Rows.Count > 0 Then
    '                        Dim unitPrice As Double = 0
    '                        Dim sItemID As String = ""
    '                        Dim sSupplierPartNumber As String = ""

    '                        For Each row As DataRow In dtCart.Rows
    '                            'check is this item a NONCAT item
    '                            Dim bIsNoncat As Boolean = False
    '                            If Not row(columnName:="ItemID") Is Nothing Then
    '                                Try
    '                                    sItemID = Trim(CType(row(columnName:="ItemID"), String))
    '                                Catch ex As Exception
    '                                    sItemID = ""
    '                                End Try
    '                                If sItemID <> "" Then
    '                                    If sItemID.Length > 5 Then
    '                                        If UCase(Microsoft.VisualBasic.Left(sItemID, 6)) = "NONCAT" Then
    '                                            bIsNoncat = True
    '                                        End If
    '                                    End If
    '                                Else
    '                                    ' is it really possible? If yes what to do?
    '                                End If  '  sItemID <> ""
    '                            End If  '  Not row(dtgCartColumns.itemId) Is Nothing
    '                            ' get unit price for this item line
    '                            unitPrice = 0
    '                            Try
    '                                unitPrice = CDbl(row(columnName:="Price"))
    '                            Catch ex As Exception
    '                                unitPrice = 0
    '                            End Try
    '                            ' check/set item need for quote flag
    '                            If (unitPrice = 0 And bIsNoncat) And (Not bIsNeedQuoteItems) Then
    '                                bIsNeedQuoteItems = True
    '                            End If
    '                            ' check/set priced item flag
    '                            'pfd check here also
    '                            If (unitPrice > 0) And (Not bIsWithPricedItems) Then
    '                                bIsWithPricedItems = True
    '                            End If

    '                            If (unitPrice = 0 And (Not bIsNoncat)) And (Not bIsWithPricedItems) Then
    '                                bIsWithPricedItems = True
    '                            End If
    '                        Next
    '                        'If dtCart.Rows.Count > 0 Then
    '                        '    Dim unitPrice As Double = 0
    '                        '    For Each row As DataRow In dtCart.Rows
    '                        '        ' get unit price for this item line
    '                        '        unitPrice = 0
    '                        '        Try
    '                        '            unitPrice = CDbl(row(columnName:="Price"))
    '                        '        Catch ex As Exception
    '                        '            unitPrice = 0
    '                        '        End Try
    '                        '        ' check/set item need for quote flag
    '                        '        If (unitPrice = 0) And (Not bIsNeedQuoteItems) Then
    '                        '            bIsNeedQuoteItems = True
    '                        '        End If
    '                        '        ' check/set priced item flag
    '                        '        'pfd check here also
    '                        '        If (unitPrice > 0) And (Not bIsWithPricedItems) Then
    '                        '            bIsWithPricedItems = True
    '                        '        End If
    '                        '    Next
    '                        'End If
    '                    End If  '  dtCart.Rows.Count > 0

    '                End If  '  Not (dtCart Is Nothing) 
    '                '// check whether there's any item(s) needing for quote - zero-priced
    '                If bIsNeedQuoteItems Then
    '                    ' (1) generate req Id
    '                    Dim sReqId As String = ""
    '                    If txtOrderNO.Text = "NEXT" Or _
    '                        txtOrderNO.Text = "" Or _
    '                        (txtOrderNO.Text.Substring(0, 1) = " " And txtOrderNO.Text.Length = 1) Then
    '                        sReqId = getOrdreqID()
    '                    Else
    '                        sReqId = Session("SITEPREFIX") & RTrim(txtOrderNO.Text.ToUpper)
    '                    End If
    '                    If sReqId = "0" Then
    '                        lblDBError.Text = "ReqID DB error - Contact Support"
    '                        Exit Sub
    '                    End If
    '                    ' (2) save
    '                    'pfd check here for punchout and itemid and price
    '                    '' old code
    '                    'If Not Session("TESTMODE") = "TEST" Then
    '                    '    updateCartDatatable()
    '                    '    UpdateNSTKDB(sReqId, " ")
    '                    '    updateOrderStatus(strBU, sReqId, strAgent)
    '                    '    buildPrintTable()
    '                    '    buildBuyerConfirmation(stritemid:=sReqId, strApprNeeded:="TRUE")

    '                    '    Session.Remove("gotStock")
    '                    '    Session.Remove("gotNSTK")
    '                    '    Session.Remove("SUBMIT")

    '                    '    Response.Redirect("PunchInRFQConfirm.aspx?REQID=" & sReqId)
    '                    'End If

    '                    'VR - first check if it is not ASCEND - regular processing
    '                    If Not IsAscendCustID(strCustID) Then
    '                        If Not Session("TESTMODE") = "TEST" Then
    '                            updateCartDatatable()
    '                            UpdateNSTKDB(sReqId, " ")
    '                            OrderApprovals.UpdateOrderStatus_SendForProcessing(strBU, sReqId, " ", ApprovalHistory.ApprHistType.OrderApproval, iParntId)
    '                            buildPrintTable()
    '                            buildBuyerConfirmation(stritemid:=sReqId, bApprNeeded:=True)

    '                            Session.Remove("gotStock")
    '                            Session.Remove("gotNSTK")
    '                            Session.Remove("SUBMIT")

    '                            Response.Redirect("PunchInRFQConfirm.aspx?REQID=" & sReqId)
    '                        End If

    '                    Else   'VR - ASCEND processing for 0 priced and NONCAT items only
    '                        If Not Session("TESTMODE") = "TEST" Then
    '                            ' building special cart - Session("CartAscendZero")
    '                            ' for ASCEND for 0 priced only
    '                            updateCartDatatableASCEND_ZeroPriced()
    '                            If Trim(strEmailAscend) = "" Then strEmailAscend = " "
    '                            UpdateNSTKDB_ASCEND(sReqId, " ", strEmailAscend)
    '                            OrderApprovals.UpdateOrderStatus_SendForProcessing(strBU, sReqId, " ", ApprovalHistory.ApprHistType.OrderApproval)
    '                            buildPrintTable()
    '                            buildBuyerConfirmationASCEND(sReqId, "TRUE", strEmailAscend)

    '                            Session.Remove("gotStock")
    '                            Session.Remove("gotNSTK")

    '                            ' do not need to run here code from "PunchInRFQConfirm.aspx"
    '                            '' Response.Redirect("PunchInRFQConfirm.aspx?REQID=" & sReqId)
    '                            If bIsWithPricedItems Then
    '                                'do nothing - it will go to PiPost.aspx
    '                                'using code for priced items
    '                            Else
    '                                ' do not need to run here code from "PunchInRFQConfirm.aspx"
    '                                '' Response.Redirect("PunchInRFQConfirm.aspx?REQID=" & sReqId)
    '                                Response.Redirect("PIPost.aspx?HOME=N&ITEMS=1")
    '                            End If
    '                        End If
    '                    End If  '  Not (strCustID = "90523" Or etc. - i.e. not ASCEND
    '                End If  '  bIsNeedQuoteItems 
    '                '// check whether there's priced items on cart that we need to send back to punch-In system
    '                If bIsWithPricedItems Then
    '                    ''old code
    '                    'Response.Redirect("PIPost.aspx?HOME=N&ITEMS=1")

    '                    'VR - first check if it is not ASCEND - regular processing
    '                    If Not IsAscendCustID(strCustID) Then
    '                        '' if UNCC then build Session("Cartprint") - will be used in PiPost to send confirm. e-mail
    '                        'If strCustID = "90425" Then
    '                        '    If chbPriority.Checked = True Then
    '                        '        Session("PriorForEmail") = "Y"
    '                        '    Else
    '                        '        Session("PriorForEmail") = "N"
    '                        '    End If
    '                        '    buildPrintTable()
    '                        'Else
    '                        '    Session("PriorForEmail") = "N"
    '                        'End If
    '                        Response.Redirect("PIPost.aspx?HOME=N&ITEMS=1")
    '                    Else
    '                        'VR - ASCEND processing for priced items only
    '                        ' building special cart - Session("CartAscendPriced") 
    '                        ' for ASCEND for priced items only
    '                        updateCartDatatableASCEND_Priced()
    '                        ' change code in PIPost.aspx.vb to use this new
    '                        ' special cart for Ascend
    '                        Response.Redirect("PIPost.aspx?HOME=N&ITEMS=1")
    '                    End If
    '                End If
    '            Else
    '                'xmlResponse(response_doc)
    '                ' this is where the crap has come back to be posted back to the site who punched in with this magnificent piece of software.
    '                'buildPunchINcXML creates the response_doc
    '                'PunchINXMLPOST post this in a java script in the ASPX.
    '                Session.RemoveAll()
    '                Session("xmlToUrl") = strPunchOutUrl
    '                Session("PunchINxml") = response_doc
    '                Response.Redirect("PunchINXMLPost.aspx")
    '            End If  '  objPunchIN.PIStatus = "A" 
    '        End If

    '        updateCartDatatable()

    '        If dtgCart.Items.Count = 0 Then
    '            ltlAlert.Text = strMessage.Say("No items in cart")
    '            Exit Sub
    '        End If

    '        ' additional validation for UPenn (I0206) ONLY
    '        '   validate the WORK ORDER NUMBER field
    '        'Dim strUPennMomsBUs As String = GetUpennMomsBus()

    '        'If (strUPennMomsBUs.IndexOf(strBU)) > -1 And _
    '        If Session("CARTWOREQ").Equals("1") And _
    '           Me.txtWorkOrder.Enabled And _
    '           Me.txtWorkOrder.Visible And _
    '           (Not Me.txtWorkOrder.ReadOnly) Then
    '            Dim wo As String = Me.txtWorkOrder.Text.Trim.ToUpper
    '            Dim bIsValidWO As Boolean = True
    '            If wo.Length = 0 Then
    '                If Not Session("CARTWO_CHGCDREQ") Is Nothing Then
    '                    If (Convert.ToString(Session("CARTWO_CHGCDREQ")) = "1" Or Convert.ToString(Session("CARTWO_CHGCDREQ")).ToLower() = "y") Then
    '                    Else
    '                        ltlAlert.Text = strMessage.Say("Work order number should not be blank.")
    '                        'lblDBError.Text = "Work order number required."
    '                        bIsValidWO = False
    '                    End If
    '                End If
    '            Else
    '                If (("~I0206").IndexOf(strBU)) > -1 Then
    '                    ' exclusive validation for UPenn
    '                    If Not (wo.Length = 9 Or wo.Length = 13) Then
    '                        ltlAlert.Text = strMessage.Say("Work order number should be in the '99-999999' or '99-999999-999' format.")
    '                        'lblDBError.Text = "Work order number should be in the '99-999999' or '99-999999-999' format."
    '                        bIsValidWO = False
    '                    Else
    '                        If wo.Length = 9 Then
    '                            Dim oRegEx As System.Text.RegularExpressions.Match = Nothing
    '                            oRegEx = System.Text.RegularExpressions.Regex.Match(input:=wo, pattern:="\d{2}-\d{6}")
    '                            If Not oRegEx.Success Then
    '                                ltlAlert.Text = strMessage.Say("Work order number should be in the '99-999999' or '99-999999-999' format.")
    '                                'lblDBError.Text = "Work order number should be in the '99-999999' or '99-999999-999' format."
    '                                bIsValidWO = False
    '                            End If
    '                            oRegEx = Nothing
    '                        ElseIf wo.Length = 13 Then
    '                            Dim oRegEx As System.Text.RegularExpressions.Match = Nothing
    '                            oRegEx = System.Text.RegularExpressions.Regex.Match(input:=wo, pattern:="\d{2}-\d{6}-\d{3}")
    '                            If Not oRegEx.Success Then
    '                                ltlAlert.Text = strMessage.Say("Work order number should be in the '99-999999' or '99-999999-999' format.")
    '                                'lblDBError.Text = "Work order number should be in the '99-999999' or '99-999999-999' format."
    '                                bIsValidWO = False
    '                            End If
    '                            oRegEx = Nothing
    '                        End If
    '                    End If
    '                End If
    '            End If
    '            If Not bIsValidWO Then
    '                Exit Sub
    '            End If
    '        End If

    '        Dim strreqID As String

    '        If txtOrderNO.Text = "NEXT" Or _
    '            txtOrderNO.Text = "" Or _
    '            (txtOrderNO.Text.Substring(0, 1) = " " And txtOrderNO.Text.Length = 1) Then
    '            strreqID = getOrdreqID()
    '        Else
    '            strreqID = Session("SITEPREFIX") & RTrim(txtOrderNO.Text.ToUpper)
    '        End If

    '        If strreqID = "0" Then
    '            lblDBError.Text = "ReqID DB error - Contact Support"
    '            Exit Sub
    '        End If
    '        Dim decPrice As Decimal = 0.0
    '        Dim strHdrStatus As String = " "
    '        Dim decOrderTotal As Decimal

    '        If Session("SDIEMP") = "SDI" And Session("AGENTUSERID") = "" Then
    '            strAgent = dropEmpID.SelectedItem.Value
    '        End If
    '        If Not Session("TESTMODE") = "TEST" Then
    '            UpdateNSTKDB(strreqID, strHdrStatus)
    '        End If
    '        Sleep(1000)

    '        Dim oApprovalResults As New ApprovalResults

    '        If Not OrderApprovals.SetInitialOrderStatus(strBU, strAgent, strreqID, oApprovalResults) Then
    '            If Not Session("TESTMODE") = "TEST" Then
    '                ltlAlert.Text = strMessage.Say("Error in approval update - please contact crib.")
    '                Exit Sub
    '            End If
    '        End If

    '        buildPrintTable()

    '        If Not Session("TESTMODE") = "TEST" Then
    '            buildBuyerConfirmation(strreqID, oApprovalResults.IsMoreApproversNeeded)

    '            If Not Trim(Session("BUDGETFLG")) = "N" Then
    '                Sleep(1000)
    '                CheckSTKLimits(strBU, strAgent, strreqID, "", "B")
    '            End If

    '            If oApprovalResults.IsMoreApproversNeeded Then
    '                If oApprovalResults.OrderExceededLimit Then
    '                    buildNotifyApprover(strreqID, strAgent, strBU, oApprovalResults.NextOrderApprover, oApprovalResults.NewOrderHeaderStatus)
    '                ElseIf oApprovalResults.IsAnyChargeCodeExceededLimit Then
    '                    For I = 0 To oApprovalResults.BudgetChargeCodesCount - 1
    '                        If oApprovalResults.BudgetExceededLimit(I) Then
    '                            buildNotifyApprover(strreqID, strAgent, strBU, oApprovalResults.NextBudgetApprover(I), oApprovalResults.NewBudgetHeaderStatus(I))
    '                        End If
    '                    Next
    '                End If
    '            End If
    '        End If

    '        ' set flag for "pickOnDemand" process to run 
    '        Try
    '            Dim flgUpdater As IISOLCustomFlagUpdater = New pickOnDemandFlagUpdater
    '            flgUpdater.updateFlag(Me, _
    '                                  New isolCustomFlagUpdaterEventArgs(strBU, ORDBData.DbUrl, ""))
    '            flgUpdater = Nothing
    '        Catch ex As Exception
    '        End Try

    '        Session.Remove("gotStock")
    '        Session.Remove("gotNSTK")
    '        Session.Remove("SUBMIT")
    '        Session("ARRParams") = oApprovalResults
    '        Response.Redirect("CartConfirm.aspx?REQID=" & strreqID)
    '    Else
    '        'SetFocus(BtnSubmit)
    '        Session.Remove("SUBMIT")

    '    End If

    'End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        'Dim _objCookie As HttpCookie = Request.Cookies("ItemsInCart")
        'If (Not _objCookie Is Nothing) Then
        '    If (Not _objCookie("CartCount") Is Nothing) Then
        '        _objCookie("CartCount") = 0
        '        Response.Cookies.Add(_objCookie)
        '    End If
        'End If

        If Session("USERID") = "MORRISJ1" Then
            'log order delete for MORRISJ1
            Dim StrQuery As String = ""
            Dim m_logFilePathJM As String = ""
            Dim m_logLevelJM As System.Diagnostics.TraceLevel = Diagnostics.TraceLevel.Off
            Dim sPath As String = ""
            Try
                sPath = System.Web.Configuration.WebConfigurationManager.AppSettings(name:="NotificationFilePath").Trim

                While (sPath.LastIndexOf("\"c) = (sPath.Length - 1))
                    sPath = sPath.TrimEnd("\"c)
                End While
                m_logFilePathJM = sPath & "\" & Now.Year.ToString("0000") & Now.Month.ToString("00") & Now.Day.ToString("00") & "JM1CancelOrderAudit.log"
                m_logLevelJM = Diagnostics.TraceLevel.Verbose

            Catch ex As Exception

            End Try
            m_loggerMJ1 = New ApplicationLogger(m_logFilePathJM, m_logLevelJM)

            StrQuery = "SELECT COUNT(1) FROM ps_isa_user_cart WHERE ISA_EMPLOYEE_ID ='" & Session("USERID") & "' AND BUSINESS_UNIT = '" & Session("BUSUNIT") & "'"
            Dim strExideAuditCartCnt As String = ORDBData.GetScalar(StrQuery)

            Dim dstExideUserCart As New DataTable
            dstExideUserCart = Session("Cart")

            Dim strExideMissingItem As String = "Order cancelled by  "

            strExideMissingItem += "USERID: " & Session("USERID") & ", "
            strExideMissingItem += "BUSUNIT: " & Session("BUSUNIT") & ", "

            strExideMissingItem += "USERCART Count: " & strExideAuditCartCnt & ", "
            strExideMissingItem += "SESSIONCART Count: " & Convert.ToString(dstExideUserCart.Rows.Count)

            m_loggerMJ1.WriteInformationLog(strExideMissingItem)

            m_loggerMJ1.Dispose()

        End If

        Dim cartCount As String = Session("ItemsInCart")
        If Not cartCount Is Nothing Then
            Session("ItemsInCart") = String.Empty
        End If

        ' New code for Attachments
        Try
            Dim dt As DataTable = CType(Session("cart"), DataTable)
            Dim i As Integer
            For i = 0 To dt.Rows.Count - 1
                If Not IsDBNull(dt.Rows(i).Item("FilePath")) Then 'was Filename Yury
                    Dim FileName As String() = CType(dt.Rows(i).Item("FilePath"), String()) 'was filename Yury
                    Dim FileArr As String() = (From A In FileName Select Path.GetFileName(A)).ToArray()
                    DeleteTempFile("", FileArr)
                End If
            Next
        Catch ex As Exception
        End Try

        'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!!!!!
        DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
        '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        Session.Remove("Cart")
        Session.Remove("gotStock")
        Session.Remove("gotNSTK")
        Session.Remove("TESTMODE")
        'If Not Session("PriorForEmail") Is Nothing Then Session.Remove("PriorForEmail")
        Response.Redirect(Session("DEFAULTPAGE").ToString())
    End Sub

    Private Sub btnQucikItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnQucikItem.Click

        Customvalidator1.Enabled = False
        Customvalidator2.Enabled = False
        CustomValidator3.Enabled = False
        Customvalidator4.Enabled = False
        CompareValidator2.Enabled = False
        RequiredFieldValidator1.Enabled = False
        'Requiredfieldvalidator2.Enabled = False
        Requiredfieldvalidator3.Enabled = False
        RequiredFieldValidator4.Enabled = False
        Requiredfieldvalidator6.Enabled = False
        CustomValidatorWorkOrder.Enabled = False
        CustomValidatorWOCH.Enabled = False
        Dim strMessage As New Alert
        Dim strCplusitemid As String = ""
        Dim strInvItemID As String
        If Session("BUSUNIT") = "" Then
            Session.RemoveAll()
            Response.Redirect("default.aspx")
        End If
        If IsDBNull(txtItemID.Text) Or txtItemID.Text = "" Then
            ltlAlert.Text = strMessage.Say("No Item Number entered for Quick Item Entry")
            Exit Sub
        End If

        If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
            strInvItemID = RTrim(txtItemID.Text)
        ElseIf txtItemID.Text.Length > 2 Then
            If txtItemID.Text.Substring(0, 3) = Session("SITEPREFIX") Then
                strInvItemID = RTrim(txtItemID.Text)
            Else
                strInvItemID = Session("SITEPREFIX") & RTrim(txtItemID.Text)
            End If
        Else
            strInvItemID = Session("SITEPREFIX") & RTrim(txtItemID.Text)
        End If
        If Session("MobIssuing") = "Y" Then
            Dim objclsInvItemID As New clsInvItemID(strInvItemID)
            If objclsInvItemID.InvStockType = "STK" Then
                ltlAlert.Text = strMessage.Say("Stock Items cannot be added to cart.\nPlease use the Mobile Issuing\nprogram to request stock.")
                Exit Sub
            End If
        End If
        If Not Session("Cart") Is Nothing Then
            updateCartDatatable()
        End If
        If Session("AGENTUSERID") = "" Then
            strBU = Session("BUSUNIT")
            strAgent = Session("USERID")
        Else
            strBU = Session("AGENTUSERBU")
            strAgent = Session("AGENTUSERID")
        End If
        If txtItemID.Text.Length < 6 Then
            strCplusitemid = getcpjunc(RTrim(txtItemID.Text.ToUpper))
            If strCplusitemid Is Nothing Or strCplusitemid = "0" Then
                If IsNumeric(txtItemID.Text.ToUpper) And txtItemID.Text.Length = 7 Then
                    strCplusitemid = getcpclassavail(RTrim(txtItemID.Text.ToUpper))
                End If
            End If
            If strCplusitemid Is Nothing Or strCplusitemid = "0" Then
                strCplusitemid = getMasterItm(RTrim(txtItemID.Text.ToUpper))
            End If
            If strCplusitemid Is Nothing Or strCplusitemid = "0" Or strCplusitemid = "" Then
                ltlAlert.Text = strMessage.Say("Invalid Item Number entered for Quick Item Entry")
                Exit Sub
            End If
        ElseIf Not txtItemID.Text.ToUpper.Substring(0, 6) = "NONCAT" Then
            strCplusitemid = getcpjunc(Trim(txtItemID.Text.ToUpper))
            If strCplusitemid Is Nothing Or strCplusitemid = "0" Then
                If IsNumeric(txtItemID.Text.ToUpper) And txtItemID.Text.Length = 7 Then
                    strCplusitemid = getcpclassavail(RTrim(txtItemID.Text.ToUpper))
                End If
            End If
            If strCplusitemid Is Nothing Or strCplusitemid = "0" Then
                strCplusitemid = getMasterItm(RTrim(txtItemID.Text.ToUpper))
            End If
            If strCplusitemid Is Nothing Or strCplusitemid = "0" Or strCplusitemid = "" Then
                ltlAlert.Text = strMessage.Say("Invalid Item Number entered for Quick Item Entry")
                Exit Sub
            End If
        End If
        'Try
        '    If IsDBNull(strCplusitemid) Then
        '        strCplusitemid = " "
        '    End If
        'Catch ex As Exception
        '    strCplusitemid = " "
        'End Try

        ''old pop-up code
        'Dim strScript As String = ""
        'strScript = "<script>"
        'strScript = strScript & "var args = {'callback': SubmitEntryForm}; var strReturn; strReturn=window.showModalDialog('quickconfirm.aspx?itemid=" & RTrim(txtItemID.Text).ToUpper & "&cpitemid=" & strCplusitemid.ToUpper & "',args,'status:no;dialogWidth:720px;dialogHeight:600px;dialogHide:true;help:no;scroll:yes');"

        ''strScript = strScript & "window.showModalDialog ('quickconfirm.aspx',null,'status:no;dialogWidth:370px;dialogHeight:220px;dialogHide:true;help:no;scroll:no');"
        ''strScript = strScript & "var strReturn; strReturn=window.showModalDialog('quickconfirm.aspx?itemid=" & RTrim(txtItemID.Text).ToUpper & "&cpitemid=" &strCplusitemid.ToUpper & "',null,'status:no;dialogWidth:700px;dialogHeight:500px;dialogHide:true;help:no;scroll:yes');if (strReturn != null) document.getElementById('lblQuickqty').value=strReturn;"
        ''strScript = strScript & "alert(strReturn);"
        ''strScript = strScript & "alert(document.frmCartProcess.lblQuickqty.value);"
        ''strScript = strScript & "document.frmCartProcess.submit();"
        'strScript = strScript & "</script>"
        'Page.RegisterStartupScript("ClientScript", strScript)

        ''new RadWindow code
        Dim tmp As String = ""
        'tmp = "http://" & Request.ServerVariables("HTTP_HOST") & GetWebAppName1()
        'tmp += "/quickconfirm.aspx?itemid=" & RTrim(txtItemID.Text).ToUpper & "&cpitemid=" & strCplusitemid.ToUpper & "&FROMRAD=RAD"

        tmp = "quickconfirm.aspx?itemid=" & RTrim(txtItemID.Text).ToUpper & "&cpitemid=" & strCplusitemid.ToUpper & "&FROMRAD=RAD"

        RadWindowChCd.NavigateUrl = tmp
        RadWindowChCd.OnClientClose = "QuickOnClientClose"
        RadWindowChCd.Title = "Quick Item Entry"
        Dim script As String = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

    End Sub

    Private Sub btnChrCD_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnChrCD.Click

        Customvalidator1.Enabled = False
        Customvalidator2.Enabled = False
        CustomValidator3.Enabled = False
        Customvalidator4.Enabled = False
        CompareValidator2.Enabled = False
        RequiredFieldValidator1.Enabled = False
        'Requiredfieldvalidator2.Enabled = False
        Requiredfieldvalidator3.Enabled = False
        RequiredFieldValidator4.Enabled = False
        Requiredfieldvalidator6.Enabled = False
        CustomValidatorWorkOrder.Enabled = False
        CustomValidatorWOCH.Enabled = False
        Dim strMessage As New Alert
        Dim strCplusitemid As String
        If Session("BUSUNIT") = "" Then
            Session.RemoveAll()
            Response.Redirect("default.aspx")
        End If
        If Not Session("Cart") Is Nothing Then
            updateCartDatatable()
        End If
        'Dim strScript As String = ""
        Dim strChgCdUser As String
        strChgCdUser = strAgent
        If Session("SDIEMP") = "SDI" Then
            Dim strEmpId As String = " "
            Try
                If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
                    strEmpId = " "
                Else
                    If Trim(dropEmpID.SelectedItem.Value) <> "" Then
                        strEmpId = Trim(dropEmpID.SelectedItem.Value)
                    Else
                        strEmpId = " "
                    End If
                End If
            Catch ex As Exception
                strEmpId = " "
            End Try
            If Trim(strEmpId) <> "" Then
                strEmpId = Trim(strEmpId)
            Else
                strEmpId = Session("USERID")
            End If
            strChgCdUser = strEmpId  '  dropEmpID.SelectedValue
        End If

        Dim tmp As String = ""
        Dim script As String = ""

        tmp = "buildchrcd.aspx?BU=" & strBU & "&USER=" & strChgCdUser & "&HOME=N&FROMRAD=RAD"
        RadWindowChCd.OffsetElementID = btnChrCD.ClientID
        RadWindowChCd.Left = Convert.ToDouble(hdnTop.Value) - 800
        RadWindowChCd.NavigateUrl = tmp
        RadWindowChCd.OnClientClose = "OnClientClose"
        RadWindowChCd.Title = "Enter Charge Code"
        script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

        'strScript = strScript & "var args = {'callback': __parseChgCd}; var strReturn; strReturn=window.showModalDialog('buildchrcd.aspx?BU=" & strBU & "&USER=" & strChgCdUser & "&HOME=N',args,'status:no;dialogWidth:700px;dialogHeight:500px;dialogHide:true;help:no;scroll:no');"
        'strScript = strScript & "var strReturn; strReturn=window.showModalDialog('buildchrcd.aspx?BU=" & strBU & "&USER=" & strChgCdUser & "&HOME=N',null,'status:no;dialogWidth:700px;dialogHeight:500px;dialogHide:true;help:no;scroll:no');if (strReturn != null) __parseChgCd(strReturn);"

        'strScript = strScript & "var strReturn; strReturn=window.showModalDialog('buildchrcd.aspx?BU=" & strBU & "&USER=" & strChgCdUser & "&HOME=N',null,'status:no;dialogWidth:700px;dialogHeight:500px;dialogHide:true;help:no;scroll:no');if (strReturn != null) document.getElementById('txtChgCD').value=strReturn;"
        'strScript = strScript & "var strReturn; strReturn=window.open('buildchrcd.aspx?BU=" & strBU & "&USER=" & strChgCdUser & "&HOME=N',null,'status:no;dialogWidth:700px;dialogHeight:500px;dialogHide:true;help:no;scroll:no');if (strReturn != null) document.getElementById('txtChgCD').value=strReturn;"
        'strScript = strScript & "document.frmCartProcess.strbodydetlmit();"
        SetFocus(txtWorkOrder)

    End Sub


#End Region

#Region "Private Methods"
    Private Sub ButtonClickEvent()
        Dim sExideList As String = "I0501~I0502~I0503~I0506~I0509~I0510~I0511~I0512~I0513~I0518~I0519~I0520~I0523"
        Dim bAuditExide As Boolean = (sExideList.IndexOf(Session("BUSUNIT").Trim.ToUpper) > -1)
        If bAuditExide Then
            AuditForExide(True)
        End If
        Try
            Dim I As Integer
            Dim X As Integer
            Dim strMessage As New Alert
            If Session("SUBMIT") = "TRUE" Then
                Session.Remove("SUBMIT")
                Exit Sub
            End If
            Session("SUBMIT") = "TRUE"
            If Session("BUSUNIT") = "" Then
                Session.RemoveAll()
                Response.Redirect("default.aspx")
            End If
            If rgCart.Items.Count = 0 Then
                ltlAlert.Text = strMessage.Say("No items in cart")
                Session.Remove("SUBMIT")
                Exit Sub
            End If
            Customvalidator1.Enabled = True
            Customvalidator2.Enabled = True
            CustomValidator3.Enabled = True
            CompareValidator2.Enabled = True

            ' New code 
            revNotes.Enabled = False
            'rfvNotes.Enabled = False


            If dropShipto.Visible Then
                Try
                    If Session("ShipToReqd1") = "Y" Then
                        Customvalidator4.Enabled = True
                    Else
                        Customvalidator4.Enabled = False
                    End If
                Catch ex As Exception
                    Customvalidator4.Enabled = False
                End Try
            Else
                Customvalidator4.Enabled = False
            End If

            If IsAlertSerialIDMissing() Then
                Exit Sub
            End If

            'If IsHeaderDueDateRequired() Then
            '    lblDBError.Text = "ERROR: Due Date is required"
            '    Exit Sub
            'End If

            'Ticket 120483 - line level date validation Yury 20170905
            Dim dateloop As Integer = 0
            Dim DueDateLine As Nullable(Of Date)
            Dim ValStatt As String = "True"
            For dateloop = 0 To rgCart.Items.Count - 1
                Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(dateloop), GridDataItem)
                Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
                Dim RDPDuedate As RadDatePicker = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker)
                If Not RDPDuedate.Visible = False Then
                    DueDateLine = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker).SelectedDate
                    If DateTime.Compare(Convert.ToDateTime(DueDateLine), Now.Date) < 0 Then
                        lblDBError.Text = "Line level Need By date (" & DueDateLine.ToString & ") cannot be less than today."
                        'Customvalidator2.ErrorMessage = "Line level Need By date (" & DueDateLine.ToString & ") cannot be less than today."
                        'Customvalidator2.IsValid = False
                        ValStatt = "False"
                    End If
                End If
                Dim txtDesccVall As String = CType(GridItem.FindControl("txtItemDescc"), TextBox).Text
                Dim txtDesccValidator As RequiredFieldValidator = CType(GridItem.FindControl("RequiredFieldValidatorDec"), RequiredFieldValidator)

                If String.IsNullOrEmpty(txtDesccVall) Then
                    lblTxtDesccErrr.Text = "Description required for the NSTK item."
                    ValStatt = "False"
                End If

            Next

            'NYC Sustainability budgetary limit 
            If Session("ISABudgetaryLimit") = "Y" Then
                If ValStatt = "True" Then
                    Dim strSQLString As String
                    Dim dsEmpName As DataSet
                    Dim dsEmpDetails As DataSet
                    Dim dsEmpTotal As DataSet
                    Try
                        strSQLString = "select ISA_EMPLOYEE_NAME from SDIX_USERS_TBL where ISA_EMPLOYEE_ID= '" + Session("UserID") + "' AND BUSINESS_UNIT= '" + Session("BUSUNIT") + "'"
                        dsEmpName = ORDBData.GetAdapter(strSQLString)
                        If Not dsEmpName Is Nothing Then
                            Dim employeeName As String = dsEmpName.Tables(0).Rows(0).Item("ISA_EMPLOYEE_NAME")

                            strSQLString = "select START_DATE , END_DATE , APPROVAL_LIMIT from SDIX_SITE_BUD_LIMIT where ISA_EMPLOYEE_NAME = '" + employeeName + "' AND ISA_BUSINESS_UNIT= '" + Session("BUSUNIT") + "' And IS_ACTIVE = 'Y'"
                            dsEmpDetails = ORDBData.GetAdapterSpc(strSQLString)
                            If Not dsEmpDetails Is Nothing Then

                                Dim startDate As String = dsEmpDetails.Tables(0).Rows(0).Item("START_DATE")
                                Dim endDate As String = dsEmpDetails.Tables(0).Rows(0).Item("END_DATE")
                                Dim appovalLimit As String = dsEmpDetails.Tables(0).Rows(0).Item("APPROVAL_LIMIT")

                                Dim StrStartDate As String
                                Try
                                    StrStartDate = "TO_DATE('" & startDate & "', 'MM/DD/YYYY')"
                                Catch ex As Exception
                                    StrStartDate = ""
                                End Try

                                Dim StrEndDate As String
                                Try
                                    StrEndDate = "TO_DATE('" & endDate & " 23:59:59', 'MM/DD/YYYY HH24:MI:SS')"
                                Catch ex As Exception
                                    StrEndDate = ""
                                End Try

                                Dim dateRange As Boolean = False
                                If DateTime.Compare(Convert.ToDateTime(startDate), DateTime.Now) < 0 AndAlso DateTime.Compare(DateTime.Now, Convert.ToDateTime(endDate)) < 0 Then
                                    dateRange = True

                                End If

                                If dateRange = True Then
                                    strSQLString = "Select sum(TotalPrice) as TotalPrice from " & vbCrLf &
                                           "(select QTY_REQUESTED, ISA_SELL_PRICE,QTY_REQUESTED*ISA_SELL_PRICE as TotalPrice from PS_ISA_ORD_INTF_LN " & vbCrLf &
                                "where ISA_LINE_STATUS <> 'EXP' AND ADD_DTTM BETWEEN " & StrStartDate & "" & vbCrLf &
                                "And " & StrEndDate & "" & vbCrLf &
                          "And ISA_EMPLOYEE_ID = '" & Session("UserID") & "')" & vbCrLf

                                    dsEmpTotal = ORDBData.GetAdapterSpc(strSQLString)
                                    If Not dsEmpTotal Is Nothing Then
                                        Dim totalPrice As String = "0"
                                        Try
                                            totalPrice = dsEmpTotal.Tables(0).Rows(0).Item("TotalPrice")
                                        Catch ex As Exception
                                            totalPrice = "0"
                                        End Try

                                        Dim currentPrice As String = Math.Round(getOrderTot(), 2).ToString("####,###,##0.00")
                                        Dim UserTotalPrice As Int32 = Math.Round(Convert.ToDecimal(totalPrice) + Convert.ToDecimal(currentPrice))
                                        Dim UserLimitPrice As Int32 = Math.Round(Convert.ToInt32(appovalLimit))

                                        If UserTotalPrice > UserLimitPrice Then
                                            lblDBError.Text = "Order total exceeds budgetary limit, Kindly reach administration team"
                                            buildNotifyBudgetaryUser(Session("UserID"), UserLimitPrice, UserTotalPrice, Session("BUSUNIT"))
                                            ValStatt = "False"
                                        End If
                                    End If
                                End If

                            Else

                            End If

                        Else
                        End If
                    Catch
                    End Try

                End If
            End If

            If ValStatt = "False" Then
                Session.Remove("SUBMIT")
                Exit Sub
            End If

            'Requiredfieldvalidator2.Enabled = True
            'If checkLineEmpChgCd() = False Then
            Requiredfieldvalidator3.Enabled = False ' True
            'End If

            If Not Session("AGENTUSERID") = "" Then
                RequiredFieldValidator4.Enabled = True
            End If

            If Session("PUNCHIN") = "YES" Then
                RequiredFieldValidator1.Enabled = False
            End If
            Dim bResultChgCd As Boolean = False
            CheckCHGCDWOValidation()  '  True, bResultChgCd)
            'If bResultChgCd Then
            '    lblDBError.Text = "ERROR: Order Charge Code is required"
            '    Exit Sub
            'End If

            'Dim bValid As Boolean = False

            Page.Validate()

            'If Not Session("PUNCHIN") = "YES" Then
            '    Page.Validate()
            '    bValid = IsValid
            'Else
            '    bValid = True
            'End If

            ''''''''''''''''''''''zzzzzzzzzzzzzzzzzzzzzzzzzz
            If IsValid Then   '  If bValid Then

                If bAuditExide Then
                    AuditForExide()
                End If

                'check work order for '
                If Trim(txtWorkOrder.Text) <> "" Then
                    txtWorkOrder.Text = Replace(Trim(txtWorkOrder.Text), "'", " ")
                End If

                Dim bIsLyonsProcessing As Boolean = False
                If Insiteonline.VoucherSharedFunctions.VoucherClass.IsLyons(strBU) Then
                    bIsLyonsProcessing = True
                End If

                Session("SCbIsLyonsProcessing") = bIsLyonsProcessing

                Dim bIsNeedQuoteItems As Boolean = False
                Dim bIsWithPricedItems As Boolean = False
                '   (1) check for any "zero-priced" and "priced" item(s)
                Dim dtCart As DataTable = Nothing
                Try
                    dtCart = CType(Session("Cart"), DataTable)
                Catch ex As Exception
                End Try
                If Not (dtCart Is Nothing) Then

                    ' check/set quote flag and priced item flag
                    If dtCart.Rows.Count > 0 Then
                        Dim unitPrice As Double = 0

                        For Each row As DataRow In dtCart.Rows

                            ' get unit price for this item line
                            unitPrice = 0
                            Try
                                unitPrice = CDbl(row(columnName:="Price"))
                                unitPrice = Math.Round(unitPrice, 2).ToString("####,###,##0.00")
                            Catch ex As Exception
                                unitPrice = 0
                            End Try
                            ' check/set item need for quote flag
                            If (unitPrice = 0) And (Not bIsNeedQuoteItems) Then
                                bIsNeedQuoteItems = True
                            End If
                            ' check/set priced item flag
                            'pfd check here also
                            If (unitPrice > 0) And (Not bIsWithPricedItems) Then
                                bIsWithPricedItems = True
                            End If

                        Next

                    End If  '  dtCart.Rows.Count > 0

                End If  '  Not (dtCart Is Nothing) 

                Session("SCbIsWithPricedItems") = bIsWithPricedItems

                Dim objPunchIN As New clsPunchin(Session.SessionID)
                Dim strCustID As String = " "  '  objPunchIN.CUSTID
                Try
                    strCustID = objPunchIN.CUSTID
                Catch ex As Exception
                    strCustID = " "
                End Try

                'VR 11/14/2013 - changed 01/13/2016
                Dim strEmailAscend As String = Session("EMAIL")  '  objPunchIN.Email()

                If Not Trim(strCustID) = "" Then
                    Dim strPunchOutUrl As String = " "
                    strPunchOutUrl = objPunchIN.PIURL
                    Dim response_doc As String = buildPunchINcXML()

                    If objPunchIN.PIStatus = "A" Then
                        '   (2) if "zero-priced" item(s) exists, save into INTFC tables
                        '       2.1. generate req id
                        '       2.2. save
                        '   (3) do PIPost ONLY IF "priced" item(s) exists in shopping cart

                        '// checks the shopping cart for any "items that needs quote" and
                        '//     items that are priced and set flags

                        Dim sReqId As String = ""
                        ' (1) generate req Id
                        If txtOrderNO.Text = "NEXT" Or
                            txtOrderNO.Text = "" Or
                            (txtOrderNO.Text.Substring(0, 1) = " " And txtOrderNO.Text.Length = 1) Then
                            sReqId = getOrdreqID()
                        Else

                            If Session("MANORDNO") = "A" Then 'user enters prefix herself
                                sReqId = Trim(txtOrderNO.Text)
                            Else                                           'prefix is appended
                                sReqId = Session("ORDERPREFIX") & Trim(txtOrderNO.Text.ToUpper)
                                sReqId = Trim(sReqId)
                            End If

                        End If

                        If sReqId = "0" Then
                            lblDBError.Text = "ReqID DB error - Contact Support"
                            Exit Sub
                        End If

                        Dim bIsAscendProcessing As Boolean = False
                        If Session("SPLITPRICEDORDERS") = "Y" Then
                            bIsAscendProcessing = True
                        End If

                        If bIsNeedQuoteItems Then

                            ' (2) save

                            'VR - first check if it is not ASCEND - regular processing
                            If Not bIsAscendProcessing Then
                                If Not Session("TESTMODE") = "TEST" Then
                                    updateCartDatatable()
                                    If bIsWithPricedItems Then
                                        UpdateNSTKDB(sReqId, " ")
                                    Else
                                        UpdateNSTKDB(sReqId, " ", "OrigRFQ")
                                    End If

                                    OrderApprovals.UpdateOrderStatus_SendForProcessing(strBU, sReqId, " ", ApprovalHistory.ApprHistType.OrderApproval, iParntId)
                                    buildPrintTable()
                                    Try
                                        buildBuyerConfirmation(sReqId, True, True)
                                    Catch ex As Exception
                                        Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmation Try Catch. User ID: " & Session("USERID") & "; Req ID: " & sReqId & " ; " & vbCrLf &
                                        "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                                        "Error message: " & ex.Message
                                        If strErrorFromEmail.Contains("Thread was being aborted") Then
                                            'do not send error e-mail
                                        Else
                                            SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmation Try Catch")
                                        End If
                                    End Try

                                    Try
                                        Session.Remove("SUBMIT")
                                        Session.Remove("gotStock")
                                        Session.Remove("gotNSTK")
                                    Catch ex As Exception

                                    End Try

                                    'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!
                                    DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
                                    '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                                    Session("CONFTARGET") = "PunchInRFQConfirm.aspx?REQID=" & sReqId

                                    Exit Sub
                                    ''  Response.Redirect("PunchInRFQConfirm.aspx?REQID=" & sReqId)
                                End If

                            Else   'VR - ASCEND processing for 0 priced and NONCAT items only
                                If Not Session("TESTMODE") = "TEST" Then
                                    ' building special cart - Session("CartAscendZero")
                                    ' for ASCEND for 0 priced only
                                    updateCartDatatableASCEND_ZeroPriced()

                                    If Trim(strEmailAscend) = "" Then strEmailAscend = " "
                                    UpdateNSTKDB_ASCEND(sReqId, " ", strEmailAscend)
                                    OrderApprovals.UpdateOrderStatus_SendForProcessing(strBU, sReqId, " ", ApprovalHistory.ApprHistType.OrderApproval)
                                    buildPrintTable()
                                    Try
                                        buildBuyerConfirmationASCEND(sReqId, "TRUE", strEmailAscend)
                                    Catch ex As Exception
                                        Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmationASCEND Try Catch. User ID: " & Session("USERID") & "; Req ID: " & sReqId & " ; " & vbCrLf &
                                        "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                                        "Error message: " & ex.Message
                                        If strErrorFromEmail.Contains("Thread was being aborted") Then
                                            'do not send error e-mail
                                        Else
                                            SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmationASCEND Try Catch")
                                        End If
                                    End Try

                                    Try
                                        Session.Remove("gotStock")
                                        Session.Remove("gotNSTK")

                                    Catch ex As Exception

                                    End Try

                                    If bIsWithPricedItems Then
                                        'do nothing - it will go to PiPost.aspx
                                        'using code for priced items

                                        'code changes requested by PS Group to move to 9.2 - VR 09/10/2017
                                        Dim sReqIdPriced As String = ""
                                        sReqIdPriced = getOrdreqID()
                                        updateCartDatatableASCEND_Priced()

                                        If Trim(strEmailAscend) = "" Then strEmailAscend = " "
                                        UpdateNSTKDB_ASCEND(sReqIdPriced, " ", strEmailAscend, True)
                                        OrderApprovals.UpdateOrderStatus_SendForProcessing(strBU, sReqIdPriced, " ", ApprovalHistory.ApprHistType.OrderApproval, 0, "ASCNDPRC")

                                        If Session("ISANotifyBuyer") = "Y" Then

                                            'send full confirm e-mail
                                            buildPrintTable()
                                            Try
                                                buildBuyerConfirmationASCEND(sReqIdPriced, "TRUE", strEmailAscend, True)
                                            Catch ex As Exception
                                                Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmationASCEND Try Catch. User ID: " & Session("USERID") & "; Req ID Priced: " & sReqIdPriced & " ; " & vbCrLf &
                                                "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                                                "Error message: " & ex.Message
                                                If strErrorFromEmail.Contains("Thread was being aborted") Then
                                                    'do not send error e-mail
                                                Else
                                                    SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmation Try Catch")
                                                End If
                                            End Try

                                        End If
                                        'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!
                                        DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
                                        '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


                                        Session("CONFTARGET") = "PIPost.aspx?HOME=N&ITEMS=1&REQID=" & sReqIdPriced

                                        Exit Sub
                                        '' Response.Redirect("PIPost.aspx?HOME=N&ITEMS=1&REQID=" & sReqIdPriced)
                                    Else

                                        'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!
                                        DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
                                        '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


                                        If Session("PUNCHINRFQCONFIRM") = "Y" Then

                                            Session("CONFTARGET") = "PunchInRFQConfirm.aspx?REQID=" & sReqId
                                        Else
                                            Session("CONFTARGET") = "PIPost.aspx?HOME=N&ITEMS=1"
                                        End If

                                        Exit Sub
                                        '' Response.Redirect("PIPost.aspx?HOME=N&ITEMS=1")
                                    End If
                                End If
                            End If  '  Not (strCustID = "90523" Or etc. - i.e. not ASCEND 
                        End If  '  bIsNeedQuoteItems 
                        '// check whether there's priced items on cart that we need to send back to punch-In system
                        If bIsWithPricedItems Then

                            'VR - first check if it is not ASCEND - regular processing
                            If Not bIsAscendProcessing Then
                                updateCartDatatable()
                                If bIsNeedQuoteItems Then
                                    UpdateNSTKDB(sReqId, " ", "OrigRFQ", True)
                                Else
                                    UpdateNSTKDB(sReqId, " ", "", True)
                                End If

                                OrderApprovals.UpdateOrderStatus_SendForProcessing(strBU, sReqId, " ", ApprovalHistory.ApprHistType.OrderApproval, iParntId)
                                buildPrintTable()
                                Try
                                    buildBuyerConfirmation(sReqId, True, True)
                                Catch ex As Exception
                                    Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmation Try Catch. User ID: " & Session("USERID") & "; Req ID: " & sReqId & " ; " & vbCrLf &
                                    "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                                    "Error message: " & ex.Message
                                    If strErrorFromEmail.Contains("Thread was being aborted") Then
                                        'do not send error e-mail
                                    Else
                                        SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmation Try Catch")
                                    End If
                                End Try

                                Dim strSqlStrngForU05 As String = ""
                                Dim iRowsAffctdU05 As Integer = 0
                                If UCase(strBU) = "I0256" Then
                                    If Len(sReqId) > 2 Then
                                        If UCase(Microsoft.VisualBasic.Left(sReqId, 3)) = "U05" Then
                                            Try
                                                strSqlStrngForU05 = "UPDATE PS_ISA_ORD_INTF_LN set ISA_LINE_STATUS = ' ' WHERE BUSINESS_UNIT_OM ='I0256' AND ORDER_NO = '" & sReqId & "'"
                                                iRowsAffctdU05 = ORDBData.ExecNonQuery(strSqlStrngForU05, False)
                                            Catch ex As Exception
                                                iRowsAffctdU05 = 0
                                            End Try
                                        End If
                                    End If
                                End If

                                Try
                                    Session.Remove("SUBMIT")
                                    Session.Remove("gotStock")
                                    Session.Remove("gotNSTK")
                                Catch ex As Exception

                                End Try

                                'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!
                                DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
                                '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


                                Session("CONFTARGET") = "PIPost.aspx?HOME=N&ITEMS=1&REQID=" & sReqId

                                Exit Sub
                                ''  Response.Redirect("PIPost.aspx?HOME=N&ITEMS=1&REQID=" & sReqId)
                            Else
                                ''VR - ASCEND processing for priced items only

                                'code changes requested by PS Group to move to 9.2 - VR 09/10/2017
                                Dim sReqIdPriced As String = ""
                                sReqIdPriced = getOrdreqID()
                                updateCartDatatableASCEND_Priced()

                                If Trim(strEmailAscend) = "" Then strEmailAscend = " "
                                UpdateNSTKDB_ASCEND(sReqIdPriced, " ", strEmailAscend, True)
                                OrderApprovals.UpdateOrderStatus_SendForProcessing(strBU, sReqIdPriced, " ", ApprovalHistory.ApprHistType.OrderApproval, 0, "ASCNDPRC")

                                If Session("ISANotifyBuyer") = "Y" Then
                                    'send full confirm e-mail
                                    buildPrintTable()
                                    Try
                                        buildBuyerConfirmationASCEND(sReqIdPriced, "TRUE", strEmailAscend, True)
                                    Catch ex As Exception
                                        Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmationASCEND Try Catch. User ID: " & Session("USERID") & "; Req ID Priced: " & sReqIdPriced & " ; " & vbCrLf &
                                        "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                                        "Error message: " & ex.Message
                                        If strErrorFromEmail.Contains("Thread was being aborted") Then
                                            'do not send error e-mail
                                        Else
                                            SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmation Try Catch")
                                        End If
                                    End Try

                                End If
                                'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!
                                DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
                                '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


                                Session("CONFTARGET") = "PIPost.aspx?HOME=N&ITEMS=1&REQID=" & sReqIdPriced

                                Exit Sub
                                ''  Response.Redirect("PIPost.aspx?HOME=N&ITEMS=1&REQID=" & sReqIdPriced)
                            End If
                        End If
                    Else
                        'xmlResponse(response_doc)
                        ' this is where the crap has come back to be posted back to the site who punched in with this magnificent piece of software.
                        'buildPunchINcXML creates the response_doc
                        'PunchINXMLPOST post this in a java script in the ASPX.

                        'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!
                        DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
                        '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        Session.RemoveAll()
                        Session("xmlToUrl") = strPunchOutUrl
                        Session("PunchINxml") = response_doc


                        Session("CONFTARGET") = "PunchINXMLPost.aspx"

                        Exit Sub
                        ''  Response.Redirect("PunchINXMLPost.aspx")
                    End If  '  objPunchIN.PIStatus = "A" 
                End If  '  If Not Trim(strCustID) = ""  '   - means punchin session

                ' changes for Lyons - 531/532
                If bIsNeedQuoteItems And bIsWithPricedItems And bIsLyonsProcessing Then
                    'special Processing - Lyons

                    Dim sReqId As String = ""
                    ' (1) generate req Id
                    If txtOrderNO.Text = "NEXT" Or
                        txtOrderNO.Text = "" Or
                        (txtOrderNO.Text.Substring(0, 1) = " " And txtOrderNO.Text.Length = 1) Then
                        sReqId = getOrdreqID()
                    Else

                        If Session("MANORDNO") = "A" Then 'user enters prefix herself
                            sReqId = Trim(txtOrderNO.Text)
                        Else                                           'prefix is appended
                            sReqId = Session("ORDERPREFIX") & Trim(txtOrderNO.Text.ToUpper)
                            sReqId = Trim(sReqId)
                        End If

                    End If

                    If sReqId = "0" Then
                        lblDBError.Text = "ReqID DB error - Contact Support"
                        Exit Sub
                    End If

                    '    process 0-priced items
                    ' building special cart - Session("CartAscendZero")
                    ' for 0 priced only
                    updateCartDatatableASCEND_ZeroPriced()

                    UpdateNSTKDB_ASCEND(sReqId, " ", Session("EMAIL"))
                    OrderApprovals.UpdateOrderStatus_SendForProcessing(strBU, sReqId, " ", ApprovalHistory.ApprHistType.OrderApproval)
                    buildPrintTable()
                    Try
                        buildBuyerConfirmationASCEND(sReqId, "TRUE", Session("EMAIL"))
                    Catch ex As Exception
                        Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, Lyons Processing buildBuyerConfirmationASCEND Try Catch. User ID: " & Session("USERID") & "; Req ID: " & sReqId & " ; " & vbCrLf &
                        "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                        "Error message: " & ex.Message
                        If strErrorFromEmail.Contains("Thread was being aborted") Then
                            'do not send error e-mail
                        Else
                            SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, Lyons Processing buildBuyerConfirmationASCEND Try Catch")
                        End If
                    End Try

                    Try
                        Session.Remove("gotStock")
                        Session.Remove("gotNSTK")

                    Catch ex As Exception

                    End Try

                    '    process Priced items

                    'code changes requested by PS Group to move to 9.2 - VR 09/10/2017
                    Dim sReqIdPriced As String = ""
                    sReqIdPriced = getOrdreqID()
                    updateCartDatatableASCEND_Priced()

                    UpdateNSTKDB_ASCEND(sReqIdPriced, " ", Session("EMAIL"), True)

                    Dim oApprovalResults As New ApprovalResults
                    ' set line status to QTC for Lyons and Priced order
                    If Not OrderApprovals.SetInitialOrderStatus(strBU, strAgent, sReqIdPriced, oApprovalResults, True) Then
                        ltlAlert.Text = strMessage.Say("Error in approval update - please contact Help Desk.")
                        Exit Sub
                    End If

                    buildPrintTable()
                    Try
                        buildBuyerConfirmationASCEND(sReqIdPriced, "TRUE", Session("EMAIL"), True)
                    Catch ex As Exception
                        Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, Lyons Processing buildBuyerConfirmationASCEND Try Catch. User ID: " & Session("USERID") & "; Req ID Priced: " & sReqIdPriced & " ; " & vbCrLf &
                        "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                        "Error message: " & ex.Message
                        If strErrorFromEmail.Contains("Thread was being aborted") Then
                            'do not send error e-mail
                        Else
                            SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, Lyons Processing buildBuyerConfirmation Try Catch")
                        End If
                    End Try
                    ' go to Cart Confirm

                    'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!
                    DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
                    '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                    Session.Remove("gotStock")
                    Session.Remove("gotNSTK")
                    Session.Remove("SUBMIT")

                    Session("CONFTARGET") = "CartConfirm.aspx?REQID=" & sReqIdPriced & "&NONSTOCK=" & sReqId
                    Session("ARRParams") = oApprovalResults

                    Exit Sub
                    ''  Response.Redirect("CartConfirm.aspx?REQID=" & sReqIdPriced & "&NONSTOCK=" & sReqId)
                    Exit Sub

                    'END special Processing - Lyons
                Else
                    'regular processing
                    'updateCartDatatable() - ' no longer needed

                    If rgCart.Items.Count = 0 Then
                        ltlAlert.Text = strMessage.Say("No items in cart")
                        Exit Sub
                    End If

                    ' additional validation for UPenn (I0206) ONLY
                    '   validate the WORK ORDER NUMBER field
                    'Dim strUPennMomsBUs As String = GetUpennMomsBus()

                    'If (strUPennMomsBUs.IndexOf(strBU)) > -1 And _
                    If Session("CARTWOREQ").Equals("1") And
                       Me.txtWorkOrder.Enabled And
                       Me.txtWorkOrder.Visible And
                       (Not Me.txtWorkOrder.ReadOnly) Then
                        Dim wo As String = Me.txtWorkOrder.Text.Trim.ToUpper
                        Dim bIsValidWO As Boolean = True
                        If wo.Length = 0 Then
                            If Not Session("CARTWO_CHGCDREQ") Is Nothing Then
                                If (Convert.ToString(Session("CARTWO_CHGCDREQ")) = "1" Or Convert.ToString(Session("CARTWO_CHGCDREQ")).ToLower() = "y") Then
                                Else
                                    ltlAlert.Text = strMessage.Say("Work order number should not be blank.")
                                    'lblDBError.Text = "Work order number required."
                                    bIsValidWO = False
                                End If
                            End If
                        Else
                            If (("~I0206").IndexOf(strBU)) > -1 Then
                                ' exclusive validation for UPenn
                                If Not (wo.Length = 9 Or wo.Length = 13) Then
                                    ltlAlert.Text = strMessage.Say("Work order number should be in the '99-999999' or '99-999999-999' format.")
                                    'lblDBError.Text = "Work order number should be in the '99-999999' or '99-999999-999' format."
                                    bIsValidWO = False
                                Else
                                    If wo.Length = 9 Then
                                        Dim oRegEx As System.Text.RegularExpressions.Match = Nothing
                                        oRegEx = System.Text.RegularExpressions.Regex.Match(input:=wo, pattern:="\d{2}-\d{6}")
                                        If Not oRegEx.Success Then
                                            ltlAlert.Text = strMessage.Say("Work order number should be in the '99-999999' or '99-999999-999' format.")
                                            'lblDBError.Text = "Work order number should be in the '99-999999' or '99-999999-999' format."
                                            bIsValidWO = False
                                        End If
                                        oRegEx = Nothing
                                    ElseIf wo.Length = 13 Then
                                        Dim oRegEx As System.Text.RegularExpressions.Match = Nothing
                                        oRegEx = System.Text.RegularExpressions.Regex.Match(input:=wo, pattern:="\d{2}-\d{6}-\d{3}")
                                        If Not oRegEx.Success Then
                                            ltlAlert.Text = strMessage.Say("Work order number should be in the '99-999999' or '99-999999-999' format.")
                                            'lblDBError.Text = "Work order number should be in the '99-999999' or '99-999999-999' format."
                                            bIsValidWO = False
                                        End If
                                        oRegEx = Nothing
                                    End If
                                End If
                            End If
                        End If
                        If Not bIsValidWO Then
                            Exit Sub
                        End If
                    End If


                    Dim strreqID As String = ""

                    If txtOrderNO.Text = "NEXT" Or
                        txtOrderNO.Text = "" OrElse
                        (txtOrderNO.Text.Substring(0, 1) = " " And txtOrderNO.Text.Length = 1) Then
                        strreqID = getOrdreqID()
                    Else
                        If Session("MANORDNO") = "A" Then 'user enters prefix herself
                            strreqID = Trim(txtOrderNO.Text)
                        Else                                           'prefix is appended
                            strreqID = Session("ORDERPREFIX") & Trim(txtOrderNO.Text.ToUpper)
                            strreqID = Trim(strreqID)
                        End If
                    End If

                    If strreqID = "0" Then
                        lblDBError.Text = "ReqID DB error - Contact Support"
                        Exit Sub
                    End If

                    Dim decPrice As Decimal = 0.0
                    Dim strHdrStatus As String = " "
                    Dim decOrderTotal As Decimal

                    If Session("SDIEMP") = "SDI" And Session("AGENTUSERID") = "" Then
                        Dim strEmpId As String = " "
                        Try
                            If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
                                strEmpId = " "
                            Else
                                If Trim(dropEmpID.SelectedItem.Value) <> "" Then
                                    strEmpId = Trim(dropEmpID.SelectedItem.Value)
                                Else
                                    strEmpId = " "
                                End If
                            End If
                        Catch ex As Exception
                            strEmpId = " "
                        End Try
                        If Trim(strEmpId) <> "" Then
                            strEmpId = Trim(strEmpId)
                        Else
                            strEmpId = Session("USERID")
                        End If
                        strAgent = strEmpId  '  dropEmpID.SelectedItem.Value
                    End If
                    If Not Session("TESTMODE") = "TEST" Then
                        If bIsLyonsProcessing Then
                            If bIsWithPricedItems And bIsNeedQuoteItems Then
                                UpdateNSTKDB(strreqID, strHdrStatus, "OrigRFQ", True)
                            ElseIf bIsWithPricedItems Then
                                UpdateNSTKDB(strreqID, strHdrStatus, "", True)
                            ElseIf bIsNeedQuoteItems Then
                                UpdateNSTKDB(strreqID, strHdrStatus, "OrigRFQ")
                            Else
                                UpdateNSTKDB(strreqID, strHdrStatus)
                            End If

                        Else '  not Lyons processing
                            If bIsNeedQuoteItems And UCase(strBU) = "I0538" Then
                                UpdateNSTKDB(strreqID, strHdrStatus, "OrigRFQ")
                            Else ' processing for everything else
                                UpdateNSTKDB(strreqID, strHdrStatus)
                            End If
                        End If  '  If bIsLyonsProcessing Then

                    End If  '  If Not Session("TESTMODE") = "TEST" Then

                    If Session("CONFTARGET") = "ShoppingCartPayment.aspx" Then
                        Exit Sub
                    End If

                    Dim oApprovalResults As New ApprovalResults
                    Dim bIsInitOrdrStatusSetOK As Boolean = False

                    If bIsLyonsProcessing And bIsWithPricedItems Then
                        ' set line status to QTC for Lyons and Priced order
                        bIsInitOrdrStatusSetOK = OrderApprovals.SetInitialOrderStatus(strBU, strAgent, strreqID, oApprovalResults, True)

                    Else

                        bIsInitOrdrStatusSetOK = OrderApprovals.SetInitialOrderStatus(strBU, strAgent, strreqID, oApprovalResults)

                    End If

                    If Not bIsInitOrdrStatusSetOK Then
                        ltlAlert.Text = strMessage.Say("Error in approval update - please contact Help Desk.")
                        Exit Sub
                    End If

                    buildPrintTable()

                    If Not Session("TESTMODE") = "TEST" Then
                        Try
                            buildBuyerConfirmation(strreqID, oApprovalResults.IsMoreApproversNeeded)
                        Catch ex As Exception
                            Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmation Try Catch. User ID: " & Session("USERID") & "; Req ID: " & strreqID & " ; " & vbCrLf &
                            "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                           "Error message: " & ex.Message
                            If strErrorFromEmail.Contains("Thread was being aborted") Then
                                'do not send error e-mail
                            Else
                                SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, buildBuyerConfirmation Try Catch")
                            End If
                        End Try

                    End If  '  If Not Session("TESTMODE") = "TEST"

                    If bAuditExide Then
                        AuditForExide()
                        m_logger.Dispose()
                    End If

                    'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!
                    DeleteFromUserCart(Session("USERID"), Session("BUSUNIT"))
                    '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                    Session.Remove("gotStock")
                    Session.Remove("gotNSTK")
                    Session.Remove("SUBMIT")

                    Session("CONFTARGET") = "CartConfirm.aspx?REQID=" & strreqID
                    Session("ARRParams") = oApprovalResults
                    '' Response.Redirect("CartConfirm.aspx?REQID=" & strreqID)
                    Exit Sub

                    'END regular processing
                End If

            Else

                Session.Remove("SUBMIT")

            End If  '  If bValid

        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error from ShoppingCart.ButtonClickEvent, main Try Catch. User ID: " & Session("USERID") & "; " & vbCrLf &
                "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                "Error message: " & ex.Message
            If strErrorFromEmail.Contains("Thread was being aborted") Then
                'do not send error e-mail
            Else
                SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.ButtonClickEvent, main Try Catch")
            End If
        End Try
    End Sub

    'Public Shared Sub updatecriteriaCounter()

    '    Dim strSQLstring As String

    '    strSQLstring = "update ISO_Search_Numbers set Search_by_criteria = Search_by_criteria + 1 "
    '    Try
    '        ExecNonQuerySQLSPC3(strSQLstring)
    '    Catch ex As Exception

    '    End Try
    'End Sub

    Private Function buildCartforemailSC(ByVal dstcart1 As DataTable) As DataTable

        Dim dr As DataRow
        Dim I As Integer
        'Dim decNum As Decimal
        Dim strPrice As String
        Dim strQty As String
        Dim dstcart As DataTable
        dstcart = New DataTable
        'Dim friggin As Boolean = False
        dstcart.Columns.Add("Line Number")
        dstcart.Columns.Add("Item Number")
        dstcart.Columns.Add("Description")
        dstcart.Columns.Add("Manuf.")
        dstcart.Columns.Add("Manuf. Partnum")
        dstcart.Columns.Add("QTY")
        dstcart.Columns.Add("UOM")
        dstcart.Columns.Add("Price")
        dstcart.Columns.Add("Ext. Price")
        dstcart.Columns.Add("Estimated Price")
        dstcart.Columns.Add("Item ID")
        dstcart.Columns.Add("Bin Location")
        dstcart.Columns.Add("Item Chg Code")
        dstcart.Columns.Add("Requestor Name")
        dstcart.Columns.Add("RFQ")
        dstcart.Columns.Add("Machine Num")
        dstcart.Columns.Add("Tax Exempt")
        '  dstcart.Columns.Add("LPP")
        dstcart.Columns.Add("PO")
        dstcart.Columns.Add("LN")
        dstcart.Columns.Add("SerialID")
        dstcart.Columns.Add("Due Date")  '  UDueDate
        dstcart.Columns.Add("Attachment")



        For I = 0 To dstcart1.Rows.Count - 1
            dr = dstcart.NewRow()
            dr("Line Number") = dstcart1.Rows(I).Item("LineNo")
            dr("Item Number") = dstcart1.Rows(I).Item("Itemid")


            dr("Description") = dstcart1.Rows(I).Item("ItemDescription")
            Try
                dr("Manuf.") = dstcart1.Rows(I).Item("Manufacturer")
            Catch ex As Exception
                dr("Manuf.") = " "
            End Try

            Try
                dr("Manuf. Partnum") = dstcart1.Rows(I).Item("Manufacturerpartnumber")
            Catch ex As Exception
                dr("Manuf. Partnum") = " "
            End Try

            Try
                dr("UOM") = dstcart1.Rows(I).Item("UOM")
            Catch ex As Exception
                dr("UOM") = "EA"
            End Try

            Try
                dr("QTY") = dstcart1.Rows(I).Item("Quantity")
                If IsDBNull(dstcart1.Rows(I).Item("Quantity")) Or
                    dstcart1.Rows(I).Item("Quantity") = " " Then
                    strQty = "0"
                Else
                    strQty = dstcart1.Rows(I).Item("Quantity")
                End If
            Catch ex As Exception
                strQty = "0"
            End Try

            '   - erwin
            'If IsDBNull(dstcart1.Rows(I).Item("Price")) Or _
            '    Convert.ToDecimal(dstcart1.Rows(I).Item("Price")) = 0.0 Or _
            '    dstcart1.Rows(I).Item("Price") = " " Then
            '    dr("Price") = "Price on Request"
            '    strPrice = "0.00"
            'Else
            '    decNum = dstcart1.Rows(I).Item("Price")
            '    dr("Price") = decNum.ToString("f")
            '    strPrice = dstcart1.Rows(I).Item("Price")
            'End If
            strPrice = "0.00"
            Try
                strPrice = CDec(dstcart1.Rows(I).Item("Price")).ToString
                If strPrice Is Nothing Then
                    strPrice = "0.00"
                End If
            Catch ex As Exception
                strPrice = "0.00"
            End Try
            If CDec(strPrice) = 0 Then
                dr("Price") = "Price on Request"
            Else
                dr("Price") = CDec(strPrice).ToString("f")
            End If

            dr("Ext. Price") = CType(Convert.ToDecimal(strQty) * Convert.ToDecimal(strPrice), String)

            Dim strEstimatedPrice As String = "0.00"
            Try
                If strPrice = "0.00" Then
                    strEstimatedPrice = CDec(dstcart1.Rows(I).Item("PricePO")).ToString
                    If strEstimatedPrice Is Nothing Then
                        strEstimatedPrice = "0.00"
                    End If
                End If

            Catch ex As Exception
                strEstimatedPrice = "0.00"
            End Try
            Try
                If CDec(strEstimatedPrice) = 0 Then
                    dr("Estimated Price") = ""
                Else
                    dr("Estimated Price") = CDec(strEstimatedPrice).ToString("f")
                End If
            Catch ex As Exception

            End Try
            Try
                dr("Item ID") = dstcart1.Rows(I).Item("customerItemID")
            Catch ex As Exception
                dr("Item ID") = " "
            End Try

            If Trim(dr("Item ID")) <> "" Then
                Try
                    If dstcart1.Rows(I).Item("customerItemID") > "0" Then
                        If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                            dr("Bin Location") = getBinLoc(dstcart1.Rows(I).Item("customerItemID"))
                        ElseIf (dstcart1.Rows(I).Item("customerItemID").ToString().Substring(0, 3) = Session("SITEPREFIX")) Then
                            dr("Bin Location") = getBinLoc(dstcart1.Rows(I).Item("customerItemID"))
                        Else
                            dr("Bin Location") = getBinLoc(Session("SITEPREFIX") & dstcart1.Rows(I).Item("customerItemID"))
                        End If
                    Else
                        dr("Bin Location") = " "
                    End If
                Catch ex As Exception
                    dr("Bin Location") = " "
                End Try
            Else
                dr("Bin Location") = " "
            End If

            dr("RFQ") = dstcart1.Rows(I).Item("rfqreq")
            dr("Item Chg Code") = dstcart1.Rows(I).Item("ItemChgCode")
            dr("Requestor Name") = dstcart1.Rows(I).Item("EmpChgCode")
            dr("Machine Num") = dstcart1.Rows(I).Item("MachineRow")
            dr("Tax Exempt") = dstcart1.Rows(I).Item("TaxFlag")
            '     dr("LPP") = dstcart1.Rows(I).Item("LPP")
            dr("PO") = dstcart1.Rows(I).Item("PO")
            dr("LN") = dstcart1.Rows(I).Item("LN")
            Try
                dr("SerialID") = dstcart1.Rows(I).Item("SerialID")
            Catch ex As Exception
                dr("SerialID") = " "
            End Try
            Try
                dr("Due Date") = CDate(dstcart1.Rows(I).Item("UDueDate")).ToString("MM-dd-yyyy")
            Catch ex As Exception
                dr("Due Date") = " "
            End Try

            ' Attachments 
            Try
                dr("Attachment") = dstcart1.Rows(I).Item("BuyerFilePath")
            Catch ex As Exception
                dr("Attachment") = ""
            End Try

            dstcart.Rows.Add(dr)

        Next

        Return dstcart

    End Function

    Private Sub BuildEmpDropDown()

        Dim strSQLString As String = ""

        strSQLString = Insiteonline.WebPartnerFunctions.WebPSharedFunc.GetEmployeeSqlForCarts("ShoppingCart")

        Dim dtrEmpReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
        dropEmpID.DataSource = dtrEmpReader
        dropEmpID.DataValueField = "EMP_ID"
        dropEmpID.DataTextField = "ISA_EMPLOYEE_NAME"
        dropEmpID.DataBind()
        dtrEmpReader.Close()

        dropEmpID.Items.Insert(0, New ListItem(" "))

        If Not dropEmpID.Items.FindByValue(strAgent) Is Nothing Then
            dropEmpID.Items.FindByValue(strAgent).Selected = True
            ''dropEmpID.Text = dropEmpID.SelectedItem.Text
            dropEmpID.Text = dropEmpID.Items.FindByValue(strAgent.ToUpper).Text
        End If

    End Sub


    Private Sub buildBuyerConfirmation(ByVal stritemid As String, ByVal bApprNeeded As Boolean, Optional ByVal bIsPunchin As Boolean = False)
        Dim strCheckBU As String = CType(Session("BUSUNIT"), String)

        Dim strSQLString As String = ""
        Dim friggin As Boolean = True
        Dim strSubjWithReqId As String = "SDI ZEUS - Material Request Confirmation - " & stritemid

        strSQLString = "SELECT ISA_SITE_EMAIL, ISA_STOCKREQ_EMAIL, ISA_NONSKREQ_EMAIL," & vbCrLf &
            " ISA_SITE_PRINTER," & vbCrLf &
            " ISA_STOCK_PRINTER, ISA_NONSTK_PRINTER" & vbCrLf &
            " FROM SYSADM8.PS_ISA_ENTERPRISE" & vbCrLf &
            " WHERE CUST_ID = '" & Session("CUSTID") & "'" & vbCrLf
        Dim dtrEntReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

        If dtrEntReader.HasRows() = True Then
            dtrEntReader.Read()
        Else
            dtrEntReader.Close()
            Dim strMessage As New Alert
            ltlAlert.Text = strMessage.Say("No enterprise record found")
            Exit Sub
        End If
        Dim strPuncher As Boolean = False
        Dim I As Integer
        Dim X As Integer
        Dim strbodyhead As String = ""
        Dim strbodydetl As String = ""
        Dim strItemtype As String = ""
        Dim intGridloop As Integer
        Dim decOrderTot As Decimal
        Dim dr As DataRow
        Dim SBstk As New StringBuilder
        Dim SWstk As New StringWriter(SBstk)
        Dim htmlTWstk As New HtmlTextWriter(SWstk)
        Dim dataGridHTML As String
        Dim SBnstk As New StringBuilder
        Dim SWnstk As New StringWriter(SBnstk)
        Dim htmlTWnstk As New HtmlTextWriter(SWnstk)
        Dim SBall As New StringBuilder
        Dim SWall As New StringWriter(SBall)
        Dim htmlTWall As New HtmlTextWriter(SWall)
        Dim dtgcart As DataGrid
        Dim dstcart1 As New DataTable
        dstcart1 = Session("Cart")
        Dim dstcart2 As New DataTable
        dstcart2 = Session("Cartprint")
        Dim txtBody As String = ""
        Dim txtHdr As String = ""
        Dim txtGrid As String = ""
        Dim txtMsg As String = ""
        Dim strDescr1 As String
        Dim strDescr2 As String
        Dim strDescr3 As String
        Dim strDescr4 As String
        Dim strCustName As String = ""
        Dim strCustEmail As String = " "
        'Dim MailAttachmentName As String()
        'Dim MailAttachmentbytes As New List(Of Byte())()

        Dim objEmployee As New clsUserTbl(strAgent, strBU)
        strCustName = objEmployee.EmployeeName
        strCustEmail = objEmployee.EmployeeEmail
        If Trim(strCustName) = "" Then
            Dim objEmplTbl As New clsEmplTbl(strAgent, strBU)
            strCustName = objEmplTbl.EmployeeName
        End If
        If Trim(strCustName) = "" Then
            strCustName = dstcart2.Rows(0).Item(0)
        End If
        If Trim(strCustEmail) = "" Then
            Dim objUsertbl As New clsUserTbl(Session("USERID"), Session("BUSUNIT"))
            strCustEmail = objUsertbl.EmployeeEmail
        End If

        ' add employee emails
        Dim intCntr1 As Integer = 0
        Dim strEmplEmail As String = ""
        Dim strEmplId As String = ""
        strCustEmail = strCustEmail.Trim.ToUpper
        If dstcart1.Rows.Count > 0 Then
            For intCntr1 = 0 To dstcart1.Rows.Count - 1
                ' get column 'EmpChgCode'
                strEmplId = dstcart1.Rows(intCntr1).Item("EmpChgCode")
                If Trim(strEmplId) <> "" Then
                    Dim objUserTbl As New clsUserTbl(strEmplId, strBU)
                    strEmplEmail = objUserTbl.EmployeeEmail
                    Try
                        If Trim(strEmplEmail) <> "" Then
                            If (strCustEmail.IndexOf(strEmplEmail.Trim.ToUpper) > -1) Then
                                'already in
                            Else
                                strCustEmail = strCustEmail & "; " & strEmplEmail.Trim.ToUpper
                            End If
                        End If

                    Catch ex As Exception

                    End Try

                End If  '  If Trim(strEmplId) <> "" Then
            Next  '  For intCntr1 = 0 To dstcart1.Rows.Count - 1
        End If  '  If dstcart1.Rows.Count > 0 Then

        ''Printer variables
        'Dim strPrinterPath As String = dtrEntReader.Item("ISA_STOCK_PRINTER")
        'Dim strUsername As String = "isacorp\i.print"
        'Dim strPassword As String = "webp0501"

        Dim Mailer As MailMessage = New MailMessage

        Mailer.From = "SDIExchange@SDI.com"
        Mailer.Cc = ""
        Mailer.Bcc = ""
        Dim sListSpecialMy1 As String = ""
        Try
            sListSpecialMy1 = Convert.ToString(ConfigurationSettings.AppSettings("MailToSpecial"))
            If sListSpecialMy1 Is Nothing Then
                sListSpecialMy1 = ""
            End If
        Catch ex As Exception
            sListSpecialMy1 = ""
        End Try
        If Trim(sListSpecialMy1) <> "" Then
            Mailer.Bcc = Trim(sListSpecialMy1)
        End If
        Dim strConnString As String = ORDBData.DbUrl
        Dim connectionEmail As OleDbConnection = New OleDbConnection(strConnString)
        'Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim FromAddress As String = "SDIExchange@SDI.com"
        Dim Mailcc As String = " "
        Dim MailBcc As String = Mailer.Bcc

        If CheckOrderPriority() Then
            strbodyhead = "<span><B>**PRIORITY ORDER**</B></span>"
        End If
        strbodydetl = strbodydetl & "<table width='100%' bgcolor='black'><tbody><tr><td><img src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large; text-align: center; Color:White;'>SDI Marketplace</span></center><center><span style='text-align: center; margin: 0px auto;  Color:White;'>SDI ZEUS - Material Request - Confirmation</span></center></td></tr></tbody></table>" & vbCrLf
        strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<TABLE class='Email_Table' cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span >Item request from </span>&nbsp;"
        strbodydetl = strbodydetl & Session("CONAME") & "</td>" & vbCrLf
        strbodydetl = strbodydetl & "<TD style='COLOR: red'>" & vbCrLf
        If bApprNeeded Then
            If bIsPunchin Then
                strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
            Else
                strbodydetl = strbodydetl & "<span>** ADDITIONAL APPROVALS REQUIRED **</span></td></tr>" & vbCrLf
            End If
        Else
            strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        End If
        strbodydetl = strbodydetl & "<TD colspan=2>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;</td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>SDI Requisition Number:</span> <span 'width:128px;'>&nbsp;" & stritemid & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "Chg. Emp. ID:<span>&nbsp;" & dstcart2.Rows(0).Item(0) & "</span></td></tr>"

        'strbodydetl = strbodydetl & "<span>&nbsp;</span></td>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Name:</span> <span>&nbsp;" & strCustName & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "Chg. Code:<span>&nbsp;" & dstcart2.Rows(0).Item(1) & "</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Email:</span> <span>&nbsp;" & strCustEmail & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Work Order #:</span> <span>&nbsp;" & dstcart2.Rows(0).Item(2) & "</span></td>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Phone#:</span> <span>&nbsp;" & Session("PHONE") & "</span></td>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "Machine Number:<span>&nbsp;" & dstcart2.Rows(0).Item(3) & "</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Request by Date:</span> <span>&nbsp;" & dstcart2.Rows(0).Item(5) & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Submit Date:</span> <span>&nbsp;" & Now() & "</span></td>"
        strbodydetl = strbodydetl & "<TD colspan='2'>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Notes:</span><br>"
        strbodydetl = strbodydetl & "<textarea readonly='readonly' style='width:100%;'>" & dstcart2.Rows(0).Item(4) & "</textarea></td></tr></table>" & vbCrLf
        'Include the OPRID_ENTERED_BY 
        strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>OPR Entered By:</span> <span>&nbsp;" & HttpContext.Current.Session("USERID").ToString() & "</span></td></table>"
        strbodydetl = strbodydetl & "&nbsp;<br>" & vbCrLf

        txtHdr = vbCrLf & "                              IN PLANT STORE PROGRAM"
        txtHdr = txtHdr & vbCrLf & vbCrLf & "                    " & strSubjWithReqId & vbCrLf & vbCrLf

        txtHdr = txtHdr & "Item Request from " & Session("CONAME") & ":" & vbCrLf & vbCrLf
        txtHdr = txtHdr & "   SDI Requisition Number: " & stritemid & vbCrLf
        txtHdr = txtHdr & "   Employee Name: " & strCustName & vbCrLf
        'txtHdr = txtHdr & "   Request by Date: " & dstcart2.Rows(0).Item(5) & vbCrLf
        txtHdr = txtHdr & "   Submit Date: " & Now() & vbCrLf
        txtHdr = txtHdr & "   Chg. Emp. ID: " & dstcart2.Rows(0).Item(0) & vbCrLf
        txtHdr = txtHdr & "   Chg. Code: " & dstcart2.Rows(0).Item(1) & vbCrLf
        txtHdr = txtHdr & "   Work Order #: " & dstcart2.Rows(0).Item(2) & vbCrLf
        'txtHdr = txtHdr & "   Machine Number: " & dstcart2.Rows(0).Item(3) & vbCrLf

        Dim strNotes1 As String = UCase(Left(dstcart2.Rows(0).Item(4), 68))
        Dim strNotes2 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 69, 68))
        Dim strNotes3 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 137, 68))
        Dim strNotes4 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 205, 50))
        txtHdr = txtHdr & "   Notes: " & strNotes1 & vbCrLf
        If Not Trim(strNotes2) = "" Then
            txtHdr = txtHdr & "          " & strNotes2 & vbCrLf
        End If
        If Not Trim(strNotes3) = "" Then
            txtHdr = txtHdr & "          " & strNotes3 & vbCrLf
        End If
        If Not Trim(strNotes4) = "" Then
            txtHdr = txtHdr & "          " & strNotes4 & vbCrLf
        End If
        txtHdr = txtHdr & vbCrLf

        Dim strOROItemID As String
        Dim bolMachineNum As Boolean = False
        Dim nItem_id_Price As Integer = 0

        Dim dstcartSTK As New DataTable

        Try

            dstcartSTK = buildCartforemailSC(dstcart1)

            intGridloop = dstcartSTK.Rows.Count
            X = 0
            decOrderTot = 0

            For I = 0 To intGridloop - 1
                If dstcartSTK.Rows(X).Item("Item ID") = " " Or
                    dstcartSTK.Rows(X).Item("Item ID") = "" Then
                    strOROItemID = dstcartSTK.Rows(X).Item("Item ID")
                ElseIf Convert.ToString(dstcartSTK.Rows(X).Item("Item ID")).Length < 3 Then
                    If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                        strOROItemID = dstcartSTK.Rows(X).Item("Item ID")
                    ElseIf dstcartSTK.Rows(X).Item("Item ID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                        strOROItemID = dstcartSTK.Rows(X).Item("Item ID")
                    Else
                        strOROItemID = Session("SITEPREFIX") & dstcartSTK.Rows(X).Item("Item ID")
                    End If
                ElseIf Convert.ToString(dstcartSTK.Rows(X).Item("Item ID")).Substring(0, 3) = "SDI" Then
                    strOROItemID = dstcartSTK.Rows(X).Item("Item ID")
                ElseIf Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                    strOROItemID = dstcartSTK.Rows(X).Item("Item ID")
                ElseIf dstcartSTK.Rows(X).Item("Item ID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                    strOROItemID = dstcartSTK.Rows(X).Item("Item ID")
                Else
                    strOROItemID = Session("SITEPREFIX") & dstcartSTK.Rows(X).Item("Item ID")
                End If

                Dim itm As sdiItem = sdiItem.GetItemInfo(strOROItemID)

                If dstcartSTK.Rows(X).Item("Item ID") = " " Then
                    dstcartSTK.Rows(X).Delete()
                    'ElseIf getStocktype(strOROItemID) = "ORO" Or _
                ElseIf itm.StockType.Id = "ORO" Or
                    Convert.ToString(dstcartSTK.Rows(X).Item("Item ID")).Substring(0, 3) = "SDI" Then
                    dstcartSTK.Rows(X).Delete()
                Else
                    strDescr1 = UCase(Left(dstcartSTK.Rows(X).Item("Description"), 60))
                    strDescr2 = UCase(Mid(dstcartSTK.Rows(X).Item("Description"), 69, 68))
                    strDescr3 = UCase(Mid(dstcartSTK.Rows(X).Item("Description"), 137, 68))
                    strDescr4 = UCase(Mid(dstcartSTK.Rows(X).Item("Description"), 205, 58))
                    txtGrid = "Item Number: " & dstcartSTK.Rows(X).Item("Item Number") & vbCrLf
                    txtGrid = txtGrid & "     Description: " & strDescr1 & vbCrLf
                    If Not Trim(strDescr2) = "" Then
                        txtGrid = txtGrid & "          " & strDescr2 & vbCrLf
                    End If
                    If Not Trim(strDescr3) = "" Then
                        txtGrid = txtGrid & "          " & strDescr3 & vbCrLf
                    End If
                    If Not Trim(strDescr4) = "" Then
                        txtGrid = txtGrid & "          " & strDescr4 & vbCrLf
                    End If
                    txtGrid = txtGrid & "     Manuf.: " & dstcartSTK.Rows(X).Item("Manuf.") & vbCrLf
                    txtGrid = txtGrid & "     Manuf. Partnum: " & dstcartSTK.Rows(X).Item("Manuf. Partnum") & vbCrLf
                    txtGrid = txtGrid & "     Quantity: " & dstcartSTK.Rows(X).Item("QTY") & vbCrLf
                    txtGrid = txtGrid & "     UOM: " & dstcartSTK.Rows(X).Item("UOM") & vbCrLf
                    'txtGrid = txtGrid & "     Price: " & dstcartSTK.Rows(X).Item("Price") & vbCrLf
                    txtGrid = txtGrid & "     Bin Location: " & dstcartSTK.Rows(X).Item("Bin Location") & vbCrLf & vbCrLf
                    txtBody = txtBody & txtGrid
                    decOrderTot = decOrderTot + Convert.ToDecimal(dstcartSTK.Rows(X).Item("Ext. Price"))
                    If Not Trim(dstcartSTK.Rows(X).Item("Machine Num")) = "" Then
                        bolMachineNum = True
                    End If
                    X = X + 1
                End If

            Next
            dstcartSTK.AcceptChanges()

            '// material request for STOCK items
            '//     received by SYSADM8.PS_ISA_ENTERPRISE.ISA_STOCKREQ_EMAIL
            If dstcartSTK.Rows.Count > 0 Then

                dtgcart = New DataGrid

                dstcartSTK.Columns.Remove("Item ID")
                dstcartSTK.Columns.Remove("RFQ")
                dstcartSTK.Columns.Remove("Tax Exempt")
                If bolMachineNum = False Then
                    dstcartSTK.Columns.Remove("Machine Num")
                End If
                If Not Session("SITEBU") = "SDM00" Then
                    dstcartSTK.Columns.Remove("PO")
                    dstcartSTK.Columns.Remove("LN")
                End If
                'dstcartSTK.Columns.Remove("Item Chg Code")
                'Code for flag based price display
                If Session("ShowPrice") = "0" Then
                    dstcartSTK.Columns.Remove("Price")
                    dstcartSTK.Columns.Remove("Ext. Price")
                End If
                If Session("BUSUNIT") = "SDM00" Or Session("BUSUNIT") = "ISA00" Or Session("BUSUNIT") = "WAL00" Or Session("BUSUNIT") = "I0W01" Then
                    dstcartSTK.Columns.Remove("Price")
                    dstcartSTK.Columns.Remove("Ext. Price")
                    dstcartSTK.Columns.Remove("Estimated Price")
                End If
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

                strItemtype = "<center><span >SDI ZEUS - Material Request - Stock</span></center>"
                Mailer.Body = strbodyhead & strItemtype & strbodydetl
                Mailer.Body = Mailer.Body & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
                Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
                Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
                'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
                Mailer.Body = Mailer.Body & "</TABLE>" & vbCrLf
                Mailer.Body = Mailer.Body & "<HR width='100%' SIZE='1'>" & vbCrLf
                Mailer.Body = Mailer.Body & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />" & vbCrLf
                If Not getDBName() Then
                    Mailer.To = "WebDev@sdi.com;avacorp@sdi.com"
                    ' Mailer.To = dtrEntReader.Item("ISA_STOCKREQ_EMAIL")
                    Mailer.Subject = "<<TEST SITE>> SDI ZEUS - Material Request - Stock - " & stritemid
                Else
                    Mailer.To = dtrEntReader.Item("ISA_STOCKREQ_EMAIL")
                    Mailer.Subject = "SDI ZEUS - Material Request - Stock - " & stritemid
                End If


                Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

                Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
                Dim MailAttachmentName As String()
                Dim MailAttachmentbytes As New List(Of Byte())()

                Try
                    SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "MATSTOCK", MailAttachmentName, MailAttachmentbytes.ToArray())

                Catch ex As Exception
                    Dim strErr As String = ex.Message
                End Try

                'Try
                '    UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, FromAddress, Mailer.To, Mailcc, MailBcc, "MATSTOCK", Mailer.Body, connectionEmail)
                '    Try
                '        connectionEmail.Close()
                '    Catch ex As Exception

                '    End Try
                'Catch exEmailOut As Exception
                '    Try
                '        connectionEmail.Close()
                '    Catch ex As Exception

                '    End Try
                '    Dim strerr As String = ""
                '    strerr = exEmailOut.Message
                'End Try
                'SDIEmailService.EmailUtilityServices("MailandStore", FromAddress, Mailer.To, Mailer.Subject, Mailcc, MailBcc, Mailer.Body, "MATSTOCK", MailAttachmentName, MailAttachmentbytes.ToArray())

                SWstk.Close()
                htmlTWstk.Close()

                txtMsg = txtHdr & txtBody
                txtMsg = txtMsg & vbCrLf & "Approved By:_________________________    Rec'd By:_____________________"

            End If

        Catch ex As Exception
            'error sending stock e-mail
            Dim strErrorFromEmail As String = "Error from ShoppingCart.buildBuyerConfirmation, sending stock e-mail. User ID: " & Session("USERID") & "; " & vbCrLf &
                "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.buildBuyerConfirmation, sending stock e-mail")
        End Try

        Dim dstCartNSTK As New DataTable

        Try

            dstCartNSTK = buildCartforemailSC(dstcart1)
            bolMachineNum = False
            Dim objEnterprise As New clsEnterprise(strBU)
            strTaxFlag = objEnterprise.TaxFlag

            intGridloop = dstCartNSTK.Rows.Count
            X = 0
            decOrderTot = 0
            For I = 0 To intGridloop - 1
                If dstCartNSTK.Rows(X).Item("Item ID") = " " Or
                    dstCartNSTK.Rows(X).Item("Item ID") = "" Then
                    strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                ElseIf Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")).Length < 3 Then
                    If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                        strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                    ElseIf dstCartNSTK.Rows(X).Item("Item ID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                        strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                    Else
                        strOROItemID = Session("SITEPREFIX") & dstCartNSTK.Rows(X).Item("Item ID")
                    End If
                ElseIf Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")).Substring(0, 3) = "SDI" Then
                    strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                ElseIf Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                    strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                ElseIf dstCartNSTK.Rows(X).Item("Item ID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                    strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                Else
                    strOROItemID = Session("SITEPREFIX") & dstCartNSTK.Rows(X).Item("Item ID")
                End If

                Dim itm As sdiItem = sdiItem.GetItemInfo(strOROItemID)
                If (Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")) = " " Or
                                                   Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")) = "") And
                                                      Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price")) <> 0 Then
                    strPuncher = True
                End If
                If dstcart1.Rows(X).Item("paintColor").ToString = "PUNCHOUT" Then
                    strPuncher = True
                End If
                'If (dstCartNSTK.Rows(X).Item("Item ID") = " " Or _
                '    dstCartNSTK.Rows(X).Item("Item ID") = "") _
                '        And (Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price") <> 0)) Then
                '    decOrderTot = decOrderTot + Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price"))
                If dstCartNSTK.Rows(X).Item("Item ID") = " " Or
                    dstCartNSTK.Rows(X).Item("Item ID") = "" _
                         Then
                    decOrderTot = decOrderTot + Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price"))

                    If Not Trim(dstCartNSTK.Rows(X).Item("Machine Num")) = "" Then
                        bolMachineNum = True
                    End If
                    X = X + 1
                    'ElseIf getStocktype(strOROItemID) = "ORO" Or _
                    'ElseIf (Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")) = " " Or _
                    '            Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")) = "") And _
                    '               Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price")) <> 0 Then
                    '    dstCartNSTK.Rows(X).Delete()

                ElseIf itm.StockType.Id = "ORO" Or
                    Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")).Substring(0, 3) = "SDI" Then
                    If Left(dstCartNSTK.Rows(X).Item("Item Number"), 6) = "NONCAT" Then
                        dstCartNSTK.Rows(X).Item("Item Number") = " "
                    End If
                    decOrderTot = decOrderTot + Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price"))
                    If Not Trim(dstCartNSTK.Rows(X).Item("Machine Num")) = "" Then
                        bolMachineNum = True
                    End If
                    X = X + 1
                Else
                    dstCartNSTK.Rows(X).Delete()
                End If

            Next
            dstCartNSTK.AcceptChanges()

            '// material request NON-STOCK
            '//     received by SYSADM8.PS_ISA_ENTERPRISE.ISA_NONSKREQ_EMAIL
            ' send email to the buyer
            If dstCartNSTK.Rows.Count > 0 Then  '  If dstCartNSTK.Rows.Count > 0 And friggin Then

                dtgcart = New DataGrid

                dstCartNSTK.Columns.Remove("Item ID")
                dstCartNSTK.Columns.Remove("Bin Location")
                'dstCartNSTK.Columns.Remove("RFQ")
                If bolMachineNum = False Then
                    dstCartNSTK.Columns.Remove("Machine Num")
                End If
                If Not strTaxFlag = "Y" Then
                    dstCartNSTK.Columns.Remove("Tax Exempt")
                End If
                If Not Session("SITEBU") = "SDM00" Then
                    dstCartNSTK.Columns.Remove("PO")
                    dstCartNSTK.Columns.Remove("LN")
                End If
                'dstcartNSTK.Columns.Remove("Item Chg Code")
                'Code for flag based price display
                If Session("ShowPrice") = "0" Then
                    dstCartNSTK.Columns.Remove("Price")
                    dstCartNSTK.Columns.Remove("Ext. Price")
                End If
                If Session("BUSUNIT") = "SDM00" Or Session("BUSUNIT") = "ISA00" Or Session("BUSUNIT") = "WAL00" Or Session("BUSUNIT") = "I0W01" Then
                    dstCartNSTK.Columns.Remove("Price")
                    dstCartNSTK.Columns.Remove("Ext. Price")
                    dstCartNSTK.Columns.Remove("Estimated Price")
                End If
                dtgcart.DataSource = dstCartNSTK
                dtgcart.DataBind()
                dtgcart.CellPadding = 3
                dtgcart.BorderColor = Gray
                dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
                dtgcart.HeaderStyle.Font.Bold = True
                dtgcart.HeaderStyle.ForeColor = Black
                dtgcart.Width.Percentage(90)

                dtgcart.RenderControl(htmlTWnstk)

                dataGridHTML = SBnstk.ToString()
                strItemtype = "<table width='100%'><tbody><tr><td width='98px'></td><td width='93%'><center><span>SDI ZEUS - Material Request - Non-Stock/ORO Items</span></center></td></tr></tbody></table>"
                If strPuncher = True Then
                    strItemtype = strItemtype + "<br><B><left><span style='font-family:Arial;color: red;font-size:Medium;'> ***NOTE*** THIS IS A PUNCHOUT ORDER!!! </span></B></left>"
                    '"***NOTE*** THIS IS A PUNCHOUT ORDER!!!"
                End If
                strPuncher = False
                Mailer.Body = strbodyhead & strItemtype & strbodydetl
                Mailer.Body = Mailer.Body & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
                Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
                Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
                'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
                Mailer.Body = Mailer.Body & "</TABLE>" & vbCrLf
                Mailer.Body = Mailer.Body & "<HR width='100%' SIZE='1'>" & vbCrLf
                Mailer.Body = Mailer.Body & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />" & vbCrLf
                ''If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Or _
                ''ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "RPTG" Then
                ''    Mailer.to = "DoNotSendPLGR@sdi.com"
                ''    'Mailer.To = dtrEntReader.Item("ISA_NONSKREQ_EMAIL")
                ''Else
                ''    Mailer.To = dtrEntReader.Item("ISA_NONSKREQ_EMAIL")
                ''End If
                ''If chbPriority.Checked = True Then
                ''    If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Or _
                ''    ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "RPTG" Then
                ''        Mailer.to = "DoNotSendPLGR@sdi.com"
                ''        'Mailer.To = dtrEntReader.Item("ISA_SITE_EMAIL")
                ''    Else
                ''        Mailer.To = dtrEntReader.Item("ISA_SITE_EMAIL")
                ''    End If
                ''Else
                ''    If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Or _
                ''    ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "RPTG" Then
                ''        Mailer.to = "DoNotSendPLGR@sdi.com"
                ''        'Mailer.To = dtrEntReader.Item("ISA_NONSKREQ_EMAIL")
                ''    Else
                ''        Mailer.To = dtrEntReader.Item("ISA_NONSKREQ_EMAIL")
                ''    End If

                ''End If

                'Logic Rewrite
                If Not getDBName() Then

                    Mailer.To = "WebDev@sdi.com;avacorp@sdi.com"
                    Mailer.Subject = "<<TEST SITE>> SDI ZEUS - Material Request - Non-Stock/ORO Items - " & stritemid
                Else
                    Mailer.Subject = "SDI ZEUS - Material Request - Non-Stock/ORO Items - " & stritemid
                    Dim strNstkEmailFull As String = ""
                    strNstkEmailFull = Trim(dtrEntReader.Item("ISA_NONSKREQ_EMAIL"))
                    If CheckOrderPriority() Then
                        If Right(strNstkEmailFull, 1) = ";" Then

                        Else
                            strNstkEmailFull = strNstkEmailFull & ";"
                        End If
                        strNstkEmailFull = strNstkEmailFull & dtrEntReader.Item("ISA_SITE_EMAIL")

                    End If
                    Mailer.To = strNstkEmailFull
                End If

                Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

                Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
                Dim MailAttachmentName As String()
                Dim MailAttachmentbytes As New List(Of Byte())()

                Try
                    SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "MATNONSTOCK", MailAttachmentName, MailAttachmentbytes.ToArray())

                Catch ex As Exception
                    Dim strErr As String = ex.Message
                End Try

                'Try
                '    UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, FromAddress, Mailer.To, Mailcc, MailBcc, "MATNONSTOCK", Mailer.Body, connectionEmail)
                '    Try
                '        connectionEmail.Close()
                '    Catch ex As Exception

                '    End Try
                'Catch exEmailOut As Exception
                '    Try
                '        connectionEmail.Close()
                '    Catch ex As Exception

                '    End Try
                '    Dim strerr As String = ""
                '    strerr = exEmailOut.Message
                'End Try
                'SDIEmailService.EmailUtilityServices("MailandStore", FromAddress, Mailer.To, Mailer.Subject, Mailcc, MailBcc, Mailer.Body, "MATNONSTOCK", MailAttachmentName, MailAttachmentbytes.ToArray())

                SWnstk.Close()
                htmlTWnstk.Close()

            End If

        Catch ex As Exception
            'error sending non-stock e-mail
            Dim strErrorFromEmail As String = "Error from ShoppingCart.buildBuyerConfirmation, sending non-stock e-mail. User ID: " & Session("USERID") & "; " & vbCrLf &
                "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.buildBuyerConfirmation, sending non-stock e-mail")
        End Try

        'Send confirmation email to customer
        Dim dstcartAll As New DataTable

        Try

            dstcartAll = buildCartforemailSC(dstcart1)
            bolMachineNum = False
            intGridloop = dstcartAll.Rows.Count
            X = 0
            decOrderTot = 0
            For I = 0 To intGridloop - 1
                If dstcartAll.Rows(X).Item("Item ID") = " " Or
                    dstcartAll.Rows(X).Item("Item ID") = "" Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                ElseIf Convert.ToString(dstcartAll.Rows(X).Item("Item ID")).Length < 3 Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                ElseIf Convert.ToString(dstcartAll.Rows(X).Item("Item ID")).Substring(0, 3) = "SDI" Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                ElseIf Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                ElseIf dstcartAll.Rows(X).Item("Item ID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                Else
                    strOROItemID = Session("SITEPREFIX") & dstcartAll.Rows(X).Item("Item ID")
                End If

                If Left(dstcartAll.Rows(X).Item("Item Number"), 6) = "NONCAT" Then
                    dstcartAll.Rows(X).Item("Item Number") = " "
                End If
                decOrderTot = decOrderTot + Convert.ToDecimal(dstcartAll.Rows(X).Item("Ext. Price"))
                If Not Trim(dstcartAll.Rows(X).Item("Machine Num")) = "" Then
                    bolMachineNum = True
                End If
                X = X + 1
            Next
            dstcartAll.AcceptChanges()

            '// material request CONFIRMATION
            '//     received by CUSTOMER - "strCustEmail"; build at the top

            If dstcartAll.Rows.Count > 0 Then

                dtgcart = New DataGrid

                dstcartAll.Columns.Remove("Item ID")
                dstcartAll.Columns.Remove("Bin Location")
                'dstcartAll.Columns.Remove("RFQ")
                If bolMachineNum = False Then
                    dstcartAll.Columns.Remove("Machine Num")
                End If
                If Not strTaxFlag = "Y" Then
                    dstcartAll.Columns.Remove("Tax Exempt")
                End If

                If Not Session("SITEBU") = "SDM00" Then
                    dstcartAll.Columns.Remove("PO")
                    dstcartAll.Columns.Remove("LN")
                End If
                'Code for flag based price display
                If Session("ShowPrice") = "0" Then
                    dstcartAll.Columns.Remove("Price")
                    dstcartAll.Columns.Remove("Ext. Price")
                End If
                If Session("BUSUNIT") = "SDM00" Or Session("BUSUNIT") = "ISA00" Or Session("BUSUNIT") = "WAL00" Or Session("BUSUNIT") = "I0W01" Then
                    dstcartAll.Columns.Remove("Price")
                    dstcartAll.Columns.Remove("Ext. Price")
                    dstcartAll.Columns.Remove("Estimated Price")
                End If
                dtgcart.DataSource = dstcartAll
                dtgcart.DataBind()
                dtgcart.CellPadding = 3
                dtgcart.BorderColor = Gray
                dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
                dtgcart.HeaderStyle.Font.Bold = True
                dtgcart.HeaderStyle.ForeColor = Black
                dtgcart.Width.Percentage(90)

                dtgcart.RenderControl(htmlTWall)
                Dim Mailbody As String = ""
                Dim MailTo As String = ""
                Dim MailSubject As String = ""

                dataGridHTML = SBall.ToString()

                Mailbody = strbodyhead & strbodydetl
                Mailbody = Mailbody & "<TABLE class='EmailMaterialRequest' cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
                Mailbody = Mailbody + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
                Mailbody = Mailbody + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
                If Session("BUSUNIT") = "SDM00" Or Session("BUSUNIT") = "ISA00" Or Session("BUSUNIT") = "WAL00" Or Session("BUSUNIT") = "I0W01" Then
                    decOrderTot = "0.00"
                    Mailbody = Mailbody + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
                Else
                    If Session("ShowPrice") <> "0" Then
                        Mailbody = Mailbody + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
                    End If
                End If

                Mailbody = Mailbody + "<HR width='100%' SIZE='1'>" & vbCrLf
                Mailbody = Mailbody & "</TABLE>" & vbCrLf
                Mailbody = Mailbody & "<HR width='100%' SIZE='1'>" & vbCrLf
                Mailbody = Mailbody & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />" & vbCrLf
                'Mailer.body = strbodyhead & strItemtype & strbodydetl
                'Mailer.body = Mailer.body & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
                'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
                'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
                'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
                'Mailer.body = Mailer.body & "</TABLE>" & vbCrLf
                If Not getDBName() Then
                    ' Mailer.to = "DoNotSendPLGR@sdi.com"
                    MailSubject = "<<TEST SITE>> " & strSubjWithReqId
                    MailTo = "WebDev@sdi.com;avacorp@sdi.com"
                Else
                    MailSubject = strSubjWithReqId
                    MailTo = strCustEmail
                End If

                Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
                Dim MailAttachmentName As String()
                Dim MailAttachmentbytes As New List(Of Byte())()

                Try
                    SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, MailTo, MailSubject, String.Empty, String.Empty, Mailbody, "MATREQCONFIRM", MailAttachmentName, MailAttachmentbytes.ToArray())

                Catch ex As Exception
                    Dim strErr As String = ex.Message
                End Try

                'Try
                '    UpdEmailOut.UpdEmailOut.UpdEmailOut(MailSubject, FromAddress, MailTo, Mailcc, MailBcc, "MATREQCONFIRM", Mailbody, connectionEmail)
                '    Try
                '        connectionEmail.Close()
                '    Catch ex As Exception

                '    End Try
                'Catch exEmailOut As Exception
                '    Try
                '        connectionEmail.Close()
                '    Catch ex As Exception

                '    End Try
                '    Dim strerr As String = ""
                '    strerr = exEmailOut.Message
                'End Try
                'SDIEmailService.EmailUtilityServices("MailandStore", FromAddress, MailTo, MailSubject, Mailcc, MailBcc, Mailbody, "MATREQCONFIRM", MailAttachmentName, MailAttachmentbytes.ToArray())

                SWall.Close()
                htmlTWall.Close()

            End If

        Catch ex As Exception
            'error sending customer e-mail
            Dim strErrorFromEmail As String = "Error from ShoppingCart.buildBuyerConfirmation, sending customer e-mail. User ID: " & Session("USERID") & "; " & vbCrLf &
                "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error from ShoppingCart.buildBuyerConfirmation, sending customer e-mail")
        End Try

        dtrEntReader.Close()
    End Sub

    Private Sub BuildDropDownType(ByVal strBU As String)

        Dim strSQLstring As String = ""

        Try
            strSQLstring = "SELECT * FROM SDIX_ORDER_TYPES where BUSINESS_UNIT = '" & strBU & "'"

            Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring, False)

            lblType.Visible = False
            dropType.Visible = False
            If Not ds Is Nothing Then
                If ds.Tables.Count > 0 Then
                    If ds.Tables(0).Rows.Count > 0 Then
                        lblType.Visible = True
                        dropType.Visible = True
                        dropType.DataSource = ds
                        dropType.DataValueField = "ORDER_TYPE"
                        dropType.DataTextField = "ORDER_DESC"
                        dropType.DataBind()

                        If Insiteonline.UpgradeMenuStructure.IsExide(strBU) Then
                            If Not dropType.Items.FindByValue("OP") Is Nothing Then
                                dropType.Items.FindByValue("OP").Selected = True
                            End If
                        End If

                    End If
                End If
            End If

        Catch ex As Exception

            lblType.Visible = False
            dropType.Visible = False
        End Try

    End Sub

    Private Sub BuildDropDownCust(ByVal strCustPrefix As String, ByVal strBU As String)

        Dim strSQLstring As String
        Dim defaultShitTo As String = " "

        If Not Trim(strCustPrefix) = "" Then
            Dim strEmpId As String = " "
            Try
                If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
                    strEmpId = " "
                Else
                    If Trim(dropEmpID.SelectedItem.Value) <> "" Then
                        strEmpId = Trim(dropEmpID.SelectedItem.Value)
                    Else
                        strEmpId = " "
                    End If
                End If
            Catch ex As Exception
                strEmpId = " "
            End Try
            If Trim(strEmpId) <> "" Then
                strEmpId = Trim(strEmpId)
            Else
                strEmpId = Session("USERID")
            End If
            strSQLstring = "SELECT ('" & strCustPrefix & "' || C.LOCATION) as CUSTID," & vbCrLf &
                   " ('" & strCustPrefix & "' || C.LOCATION || ' - ' || B.DESCR) as locname" & vbCrLf &
                   " FROM SDIX_EMP_LOC_XRF A, SYSADM8.PS_LOCATION_TBL B, PS_BUS_UNIT_TBL_IN C,SYSADM8.PS_ISA_ENTERPRISE D" & vbCrLf &
                   " WHERE A.EFFDT =" & vbCrLf &
                   " (SELECT MAX(A_ED.EFFDT) FROM SDIX_EMP_LOC_XRF A_ED" & vbCrLf &
                   " WHERE A.LOCATION = A_ED.LOCATION" & vbCrLf &
                   " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf &
                   " AND A.EMPLID = '" & strEmpId & "'" & vbCrLf &
                   " AND A.LOCATION = C.LOCATION AND C.BUSINESS_UNIT=D.ISA_BUSINESS_UNIT " & vbCrLf &
                   " AND D.BU_STATUS = '1'" & vbCrLf &
                   " ORDER BY locname" & vbCrLf
        Else
            '   - erwin 2009.10.30
            Dim bu3digit As String = strBU.Trim
            Try
                bu3digit = bu3digit.Substring(2, 3)
            Catch ex As Exception
            End Try
            strSQLstring = "SELECT A.SHIPTO_ID as CUSTID," & vbCrLf &
                " (A.DESCR || ' - ' || A.SHIPTO_ID) as locname" & vbCrLf &
                " FROM PS_SHIPTO_TBL A" & vbCrLf &
                " WHERE A.EFFDT =" & vbCrLf &
                " (SELECT MAX(A_ED.EFFDT) FROM PS_SHIPTO_TBL A_ED" & vbCrLf &
                " WHERE A.SETID = A_ED.SETID" & vbCrLf &
                " AND A.SHIPTO_ID = A_ED.SHIPTO_ID" & vbCrLf &
                " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf &
                " AND A.EFF_STATUS = 'A'" & vbCrLf &
                " AND A.DS_NETWORK_CODE LIKE '%" & bu3digit & "' ORDER BY A.SHIPTO_ID " & vbCrLf &
                ""

            '" AND A.BUSINESS_UNIT_IN LIKE '%" & bu3digit & "' " & vbCrLf & _
            '""

        End If


        Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)

        If ds.Tables(0).Rows.Count < 2 Then
            dropShipto.Visible = False
            lblShipto.Visible = False
            reqdDiv.Visible = False
            If ds.Tables(0).Rows.Count = 1 Then
                txtShipTo.Text = ds.Tables(0).Rows(0).Item("CUSTID")
            End If
            Exit Sub
        Else
            dropShipto.Visible = True
            lblShipto.Visible = True
        End If
        dropShipto.DataSource = ds
        dropShipto.DataValueField = "CUSTID"
        dropShipto.DataTextField = "locname"
        dropShipto.DataBind()
        dropShipto.Items.Insert(0, "-- Select ShipTo --")

        'select the default ship to location 
        defaultShitTo = Session("ISAShipToID")

        If Not defaultShitTo = "" And Not defaultShitTo = Nothing Then
            txtShipTo.Text = defaultShitTo
            dropShipto.SelectedValue = defaultShitTo
        End If



    End Sub

    Private Sub BuildDropDownMachine()

        Dim strSQLstring As String
        strSQLstring = "SELECT A.ISA_MACHINE_NO" & vbCrLf &
                    " FROM PS_ISA_MACHINE_BU A" & vbCrLf &
                    " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'"

        Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)

        If ds.Tables(0).Rows.Count = 0 Then
            cmbMachine.Visible = False
            txtMachineNum.Visible = True
            txtMachineNum.Style.Add("height", "18px")
        Else
            Session("dsMachine") = ds
            cmbMachine.DataSource = ds
            cmbMachine.DataValueField = "ISA_MACHINE_NO"
            cmbMachine.DataTextField = "ISA_MACHINE_NO"
            cmbMachine.DataBind()

            cmbMachine.Visible = True
            txtMachineNum.Visible = False
        End If

    End Sub

    Private Sub buildPrintTable()
        Const cCartEmpID = 0
        Const cCartChgCD = 1
        Const cCartWrkOrd = 2
        Const cCartMachNum = 3
        Const cCartNotes = 4
        Const cCartReqByDate = 5

        Dim dstCart2 As DataTable
        Dim dr As DataRow

        dstCart2 = New DataTable
        dstCart2.Columns.Add("Cartempid")
        dstCart2.Columns.Add("Cartchgcd")
        dstCart2.Columns.Add("Cartwrkord")
        dstCart2.Columns.Add("Cartmachnum")
        dstCart2.Columns.Add("Cartnotes")
        dstCart2.Columns.Add("Cartreqbydate")

        dr = dstCart2.NewRow()
        Dim strEmpId As String = " "
        Try
            If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
                strEmpId = " "
            Else
                If Trim(dropEmpID.SelectedItem.Text) <> "" Then
                    strEmpId = Trim(dropEmpID.SelectedItem.Text)
                Else
                    strEmpId = " "
                End If
            End If
        Catch ex As Exception
            strEmpId = " "
        End Try
        If Trim(strEmpId) <> "" Then
            strEmpId = Trim(strEmpId)
        Else
            strEmpId = Session("USERNAME")
        End If

        dr(cCartEmpID) = strEmpId  '  dropEmpID.SelectedItem.Text
        dr(cCartChgCD) = txtChgCD.Text
        dr(cCartWrkOrd) = txtWorkOrder.Text
        If txtMachineNum.Visible = True Then
            dr(cCartMachNum) = txtMachineNum.Text
        Else
            dr(cCartMachNum) = cmbMachine.Text
        End If
        dr(cCartNotes) = txtNotes.Text
        'dr(cCartReqByDate) = txtReqByDate.Text
        dr(cCartReqByDate) = RadDatePickerReqByDate.SelectedDate
        dstCart2.Rows.Add(dr)
        Session("Cartprint") = dstCart2

    End Sub

    Private Function buildPunchINcXML() As String
        ''''''''''''''''''''''''zzzzzzzzzzzzzzzzzzzzzzzzzz
        'get save session data
        Dim objPunchIN As New clsPunchin(Session.SessionID)
        Dim I As Integer
        Dim strPrice As String
        Dim intRand As Int64
        Dim strproLog As String

        intRand = (100000001 * Rnd())
        strproLog = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbCrLf
        strproLog = strproLog & "<!DOCTYPE cXML SYSTEM ""http://xml.cxml.org/schemas/cXML/1.1.009/cXML.dtd"">"
        strproLog = strproLog & "<cXML payloadID=""" & Now() & " " & intRand & "@sdi.com"" xml:lang=""en-US"" timestamp=""" & Now & """>" & vbCrLf

        strproLog = strproLog & "<Header>"
        strproLog = strproLog & "<From>"
        strproLog = strproLog & "<Credential domain=""SDI"">"
        strproLog = strproLog & "<Identity>" & objPunchIN.CUSTID & "</Identity>"
        strproLog = strproLog & "</Credential>"
        strproLog = strproLog & "</From>"
        strproLog = strproLog & "<To>"
        strproLog = strproLog & "<Credential domain=""" & objPunchIN.FromDomain & """>"
        strproLog = strproLog & "<Identity>" & objPunchIN.CUSTID & "</Identity>"
        strproLog = strproLog & "</Credential>"
        strproLog = strproLog & "</To>"
        strproLog = strproLog & "<Sender>"
        strproLog = strproLog & "<Credential domain=""" & objPunchIN.SenderDomain & """>"
        strproLog = strproLog & "<Identity>" & objPunchIN.SenderID & "</Identity>"
        strproLog = strproLog & "</Credential>"
        strproLog = strproLog & "<UserAgent>" & objPunchIN.UserAgent & "</UserAgent>"
        strproLog = strproLog & "</Sender>"
        strproLog = strproLog & "</Header>"
        strproLog = strproLog & "<Message>"
        strproLog = strproLog & "<PunchOutOrderMessage>"
        strproLog = strproLog & "<BuyerCookie>" & Session.SessionID.ToUpper & "</BuyerCookie>"
        strproLog = strproLog & "<PunchOutOrderMessageHeader operationAllowed=""edit"">"
        strproLog = strproLog & "<Total>"
        strproLog = strproLog & "<Money currency=""USD"">" & txtOrderTot.Text & "</Money>"
        strproLog = strproLog & "</Total>"
        strproLog = strproLog & "<Shipping>"
        strproLog = strproLog & "<Money currency=""USD"">0.00</Money>"
        strproLog = strproLog & "<Description xml:lang=""en""> Shipping charges are calculated at time of shipment.  This value is  being supplied to maintain compatibility with Ariba EDI customers.</Description>"
        strproLog = strproLog & "</Shipping>"
        strproLog = strproLog & "</PunchOutOrderMessageHeader>"

        For I = 0 To rgCart.Items.Count - 1
            Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
            Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
            If Not CType(nestedview.FindControl("chkDelete"), CheckBox).Checked Then
                strproLog = strproLog & "<ItemIn quantity=""" & Convert.ToString(Convert.ToDecimal(CType(nestedview.FindControl("txtQTY"), TextBox).Text)) & """>"
                strproLog = strproLog & "<ItemID>"
                strproLog = strproLog & "<SupplierPartID>" & CType(nestedview.FindControl("hdfSupPartNum"), HiddenField).Value & "</SupplierPartID>"
                strproLog = strproLog & "<SupplierPartAuxiliaryID>" & CType(nestedview.FindControl("hdfSupPartNumAux"), HiddenField).Value & "</SupplierPartAuxiliaryID>"
                strproLog = strproLog & "</ItemID>"
                strproLog = strproLog & "<ItemDetail>"
                strproLog = strproLog & "<UnitPrice>"
                strPrice = "0.00"
                Try
                    strPrice = CDec(CType(nestedview.FindControl("Price"), Label).Text).ToString
                    If strPrice Is Nothing Then
                        strPrice = "0.00"
                    End If
                Catch ex As Exception
                    strPrice = "0.00"
                End Try

                Dim srtItemID As String = (CType(rgCart.Items.Item(I).Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label).Text).ToString

                strproLog = strproLog & "<Money currency=""USD"">" & strPrice & "</Money>"
                strproLog = strproLog & "</UnitPrice>"
                strproLog = strproLog & "<Description xml:lang=""en"">" & rgCart.Items.Item(I).Cells(dtgCartColumns.itemDescription).Text & "</Description>"
                strproLog = strproLog & "<UnitOfMeasure>" & rgCart.Items.Item(I).Cells(dtgCartColumns.itemUOM).Text & "</UnitOfMeasure>"
                strproLog = strproLog & "<Classification domain=""VendorNumber"">" & srtItemID & "</Classification>"
                strproLog = strproLog & "<ManufacturerPartID>" & rgCart.Items.Item(I).Cells(dtgCartColumns.mfgPartNo).Text & "</ManufacturerPartID>"
                strproLog = strproLog & "<ManufacturerName>" & rgCart.Items.Item(I).Cells(dtgCartColumns.mfg).Text & "</ManufacturerName>"
                strproLog = strproLog & "</ItemDetail>"
                strproLog = strproLog & "</ItemIn>"

            End If
        Next

        'For I = 0 To dtgCart.Items.Count - 1
        '    'If Not CType(dtgCart.Items.Item(I).Cells(9).FindControl("chkDelete"), CheckBox).Checked Then
        '    If Not CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.deleteFlag).FindControl("chkDelete"), CheckBox).Checked Then
        '        'strproLog = strproLog & "<ItemIn quantity=""" & Convert.ToString(Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(5).FindControl("txtQTY"), TextBox).Text)) & """>"
        '        strproLog = strproLog & "<ItemIn quantity=""" & Convert.ToString(Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text)) & """>"
        '        strproLog = strproLog & "<ItemID>"
        '        'strproLog = strproLog & "<SupplierPartID>" & dtgCart.Items.Item(I).Cells(14).Text & "</SupplierPartID>"
        '        strproLog = strproLog & "<SupplierPartID>" & dtgCart.Items.Item(I).Cells(dtgCartColumns.supplierPartNo_hidden).Text & "</SupplierPartID>"
        '        'strproLog = strproLog & "<SupplierPartAuxiliaryID>" & dtgCart.Items.Item(I).Cells(13).Text & "</SupplierPartAuxiliaryID>"
        '        strproLog = strproLog & "<SupplierPartAuxiliaryID>" & dtgCart.Items.Item(I).Cells(dtgCartColumns.supplierId_hidden).Text & "</SupplierPartAuxiliaryID>"
        '        strproLog = strproLog & "</ItemID>"
        '        strproLog = strproLog & "<ItemDetail>"
        '        strproLog = strproLog & "<UnitPrice>"
        '        ''If CType(dtgCart.Items.Item(I).Cells(6).FindControl("Price"), Label).Text = "Price on Request" Then
        '        'If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text = "Price on Request" Then
        '        '    strPrice = "0.00"
        '        'Else
        '        '    'strPrice = CType(dtgCart.Items.Item(I).Cells(6).FindControl("Price"), Label).Text
        '        '    strPrice = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text
        '        'End If
        '        strPrice = "0.00"
        '        Try
        '            strPrice = CDec(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text).ToString
        '            If strPrice Is Nothing Then
        '                strPrice = "0.00"
        '            End If
        '        Catch ex As Exception
        '            strPrice = "0.00"
        '        End Try

        '        Dim srtItemID As String = (CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label).Text).ToString

        '        strproLog = strproLog & "<Money currency=""USD"">" & strPrice & "</Money>"
        '        strproLog = strproLog & "</UnitPrice>"
        '        'strproLog = strproLog & "<Description xml:lang=""en"">" & dtgCart.Items.Item(I).Cells(1).Text & "</Description>"
        '        strproLog = strproLog & "<Description xml:lang=""en"">" & dtgCart.Items.Item(I).Cells(dtgCartColumns.itemDescription).Text & "</Description>"
        '        'strproLog = strproLog & "<UnitOfMeasure>" & dtgCart.Items.Item(I).Cells(4).Text & "</UnitOfMeasure>"
        '        strproLog = strproLog & "<UnitOfMeasure>" & dtgCart.Items.Item(I).Cells(dtgCartColumns.itemUOM).Text & "</UnitOfMeasure>"
        '        'strproLog = strproLog & "<Classification domain=""VendorNumber"">" & dtgCart.Items.Item(I).Cells(0).Text & "</Classification>"

        '        'strproLog = strproLog & "<Classification domain=""VendorNumber"">" & dtgCart.Items.Item(I).Cells(dtgCartColumns.itemId).Text & "</Classification>"
        '        strproLog = strproLog & "<Classification domain=""VendorNumber"">" & srtItemID & "</Classification>"

        '        'strproLog = strproLog & "<ManufacturerPartID>" & dtgCart.Items.Item(I).Cells(3).Text & "</ManufacturerPartID>"
        '        strproLog = strproLog & "<ManufacturerPartID>" & dtgCart.Items.Item(I).Cells(dtgCartColumns.mfgPartNo).Text & "</ManufacturerPartID>"
        '        'strproLog = strproLog & "<ManufacturerName>" & dtgCart.Items.Item(I).Cells(2).Text & "</ManufacturerName>"
        '        strproLog = strproLog & "<ManufacturerName>" & dtgCart.Items.Item(I).Cells(dtgCartColumns.mfg).Text & "</ManufacturerName>"
        '        strproLog = strproLog & "</ItemDetail>"
        '        strproLog = strproLog & "</ItemIn>"
        '    End If

        'Next

        strproLog = strproLog & "</PunchOutOrderMessage>" & vbCrLf
        strproLog = strproLog & "</Message>" & vbCrLf
        strproLog = strproLog & "</cXML>" & vbCrLf

        'Logging all XML files that are punched out to the customer - Vijay (01-23-2013)  ---  Begin

        Dim FILENAME As String = Server.MapPath("") & "\PunchInFiles\PUNCHIN" & Session.SessionID & Now.Ticks.ToString & ".xml"
        Dim fileName1 As String = "C:\inetpub\wwwroot\SDIExchange.PunchINFiles"   '   punchInFilePath from web.config
        Try
            fileName1 = ConfigurationSettings.AppSettings("punchInFilePath")   '   "from web.config"
        Catch ex As Exception
            fileName1 = "C:\inetpub\wwwroot\SDIExchange.PunchINFiles"
        End Try
        fileName1 += "\PUNCHIN" & Session.SessionID & Now.Ticks.ToString & ".xml"

        Dim objFileStream As System.IO.FileStream
        Dim objStreamWriter As System.IO.StreamWriter

        Try
            'objFileStream = System.IO.File.Create(FILENAME)
            objFileStream = System.IO.File.Create(fileName1)
            objStreamWriter = New System.IO.StreamWriter(objFileStream)
            objStreamWriter.WriteLine(strproLog)
        Catch ex As Exception

        Finally
            Try
                objStreamWriter.Close()
            Catch ex As Exception

            End Try
        End Try

        'objStreamWriter.Close()

        'Logging all XML files that are punched out to the customer - Vijay (01-23-2013)  ---  End

        Return strproLog
    End Function

    Private Sub buildXMLGrid()

        Try

            'Dim dsNonCat As DataSet
            'Dim dstCart1 As DataTable
            'Dim dstCart2 As DataTable
            'Dim dstCart3 As DataTable
            'Dim I As Integer
            'Dim X As Integer = 1

            Dim dstCart4 As DataTable
            Dim strRemoveStockFlag As String
            Dim strMessage As New Alert
            Dim bHaveChanges As Boolean = False

            ' 09/23/2008  - find out if we already have this item in sdi inventory -  if we do we want to send them a message 
            '  and delete that item from the grid/shopping cart - there could be more than one item!!!!
            dstCart4 = Session("Cart")
            If Session("MobIssuing") = "Y" Then
                strRemoveStockFlag = RemoveStockItems(dstCart4)
            End If
            Dim arr As New ArrayList
            Dim arrReplaceWith As New ArrayList
            Dim arrConfirmedToDelete As New ArrayList
            Dim arrSTKConfirmedToDelete As New ArrayList
            Dim strLineQty As String = ""
            Dim arrLineQtys As New ArrayList
            Dim bIsReplacingPartsAfterPunchOut As Boolean = False

            If Not dstCart4 Is Nothing Then
                Dim bIsPunchInSession As Boolean = False
                'Try
                '    bIsPunchInSession = CStr(Session("PUNCHIN")).Trim.ToUpper.StartsWith("Y")
                'Catch ex As Exception
                'End Try
                If Not bIsPunchInSession Then
                    Dim I As Integer
                    Dim strOurCatlgPart As String = ""
                    Dim strDeletedParts As String = ""
                    Dim strSTKDeletedParts As String = ""
                    Dim strReplacedParts As String = ""
                    For I = 0 To dstCart4.Rows.Count - 1
                        If Not IsDBNull(dstCart4.Rows(I).Item(5)) Then
                            strLineQty = dstCart4.Rows(I).Item(eCartCol.Quantity).ToString()
                            Dim strUnivPartNo As String = dstCart4.Rows(I).Item(5).ToString()
                            If strUnivPartNo.Length > 24 Then
                                strOurCatlgPart = Right(strUnivPartNo, strUnivPartNo.Length - 24)
                                If strUnivPartNo.Substring(0, 18) = "PUNCHOUT STOCKITEM" Then
                                    'ltlAlert.Text = strMessage.Say("Requested punchout Item " & _
                                    '" has been removed from cart.\nItem " & strOurCatlgPart & _
                                    '" is already in your Catalog.")
                                    If Trim(strSTKDeletedParts) = "" Then
                                        strSTKDeletedParts = strOurCatlgPart
                                    Else
                                        strSTKDeletedParts += " ; " & strOurCatlgPart
                                    End If

                                    arrSTKConfirmedToDelete.Add(I)

                                Else
                                    If strUnivPartNo.Length > 26 Then
                                        ' ORO, same MFG Part/MFG Name AND ORO flag in the Enterprise table
                                        ' will be processed differently from STK item - we'll implement automatic replacement here
                                        strOurCatlgPart = Right(strUnivPartNo, strUnivPartNo.Length - 26)
                                        If strUnivPartNo.Substring(0, 20) = "OROSAMEMFG STOCKITEM" Then
                                            If Trim(strReplacedParts) = "" Then
                                                strReplacedParts = strOurCatlgPart
                                            Else
                                                strReplacedParts += " ; " & strOurCatlgPart
                                            End If

                                            'ltlAlert.Text = strMessage.Say("Requested punchout Item  is already in your Catalog." & _
                                            '" \nThis Catalog item: " & strOurCatlgPart & _
                                            '" - was placed in your Shopping Cart.")
                                            arr.Add(I)
                                            ' add strOurCatlgPart to the other array of the Our Catlg Parts
                                            arrReplaceWith.Add(strOurCatlgPart)
                                            arrLineQtys.Add(strLineQty)
                                        Else
                                            ''for the future - to show pop-up
                                            'If strUnivPartNo.Substring(0, 20) = "SIMILARMFG STOCKITEM" Then
                                            '   strOurCatlgPart = Right(strUnivPartNo, strUnivPartNo.Length - 26)

                                            'End If
                                        End If  '  If strUnivPartNo.Substring(0, 20) = "OROSAMEMFG STOCKITEM" Then
                                    End If  '  If strUnivPartNo.Length > 26 Then

                                End If  '  If strUnivPartNo.Substring(0, 18) = "PUNCHOUT STOCKITEM" Then
                            End If  '  If strUnivPartNo.Length > 24 Then
                        End If
                    Next  '  For I = 0 To dstCart4.Rows.Count - 1
                    'show Alert here
                    Dim strAlrtMsg21 As String = ""
                    If Trim(strDeletedParts) <> "" Then
                        strAlrtMsg21 = "Requested punchout Item(s) " &
                                    " has been removed from cart.\nItem(s): " & strDeletedParts &
                                        " - already in your Catalog.\n"
                    End If
                    If Trim(strSTKDeletedParts) <> "" Then
                        strAlrtMsg21 = "Requested punchout Item(s) " &
                                    " has been removed from cart.\nItem(s): " & strSTKDeletedParts &
                                        " - already in your Catalog.\n"
                    End If
                    If Trim(strReplacedParts) <> "" Then
                        strAlrtMsg21 += "Requested punchout Item(s) already in your Catalog." &
                        " \nThe Catalog item(s): " & strReplacedParts &
                        " - placed in your Shopping Cart."
                    End If
                    If Trim(strAlrtMsg21) <> "" Then
                        ltlAlert.Text = strMessage.Say(strAlrtMsg21)
                    End If

                    Dim iCtr As Integer = 0

                    If arr.Count > 0 Or arrSTKConfirmedToDelete.Count > 0 Then

                        bHaveChanges = True
                    End If
                    'check for array to replace parts and insert them
                    If arrReplaceWith.Count > 0 Then
                        Dim strPartToInsert As String = ""
                        Dim dtShopCartTbl As New DataTable
                        Dim strQtyToInsert As String = ""
                        Dim iDelete As Integer = 0
                        Dim strPartsInOracleButNotInUnilog As String = ""
                        For iCtr = 0 To arrReplaceWith.Count - 1
                            strPartToInsert = arrReplaceWith(iCtr)
                            strQtyToInsert = arrLineQtys(iCtr)
                            iDelete = arr(iCtr)
                            Dim response As String = ""
                            Dim dtSearchResults As DataTable = Nothing

                            'code below is working for both Unilog and SOLR search
                            response = UnilogSearch.GetSearchResults(UnilogSearch.SearchType.customerPartnumber, strPartToInsert, Convert.ToString(Session("CplusDB")), String.Empty, 1)
                            If Session("USESOLRSEARCH") = "Y" Then
                                ' use SOLR search code
                                dtSearchResults = UnilogSearch.ParseSOLRJsonValue(response, Convert.ToString(Session("CplusDB")))
                            Else
                                'do a search on column customer_part_number rather than a general search
                                dtSearchResults = UnilogSearch.ParseUnilogJsonValue(response, Convert.ToString(Session("CplusDB")))

                            End If

                            If Not dtSearchResults Is Nothing Then
                                If dtSearchResults.Rows.Count > 0 Then
                                    Insiteonline.VoucherSharedFunctions.VoucherClass.ConvertUnilogSearchTblToShopngCartTbl(dtSearchResults, strQtyToInsert, strPartToInsert, dstCart4)
                                    dstCart4.AcceptChanges()
                                    'only here should be code to delete Punch Out line
                                    arrConfirmedToDelete.Add(iDelete)
                                Else
                                    'prepare error message - this part in the ORACLE table but not in UNILOG Search
                                    If Trim(strPartsInOracleButNotInUnilog) = "" Then
                                        strPartsInOracleButNotInUnilog = strPartToInsert
                                    Else
                                        strPartsInOracleButNotInUnilog += " ; " & strPartToInsert
                                    End If
                                End If
                            End If

                        Next  '  For iCtr = 0 To arrReplaceWith.Count - 1

                        Dim iN1 As Integer = 0
                        If (arrConfirmedToDelete.Count > 0) Or (Trim(strPartsInOracleButNotInUnilog) <> "" And arrConfirmedToDelete.Count < 1) Then
                            If arrConfirmedToDelete.Count > 0 Then
                                For iCtr = 0 To arrConfirmedToDelete.Count - 1
                                    iN1 = arrConfirmedToDelete(iCtr)
                                    dstCart4.Rows(iN1).Delete()
                                Next
                                dstCart4.AcceptChanges()
                            End If  '  If arrConfirmedToDelete.Count > 0

                            bIsReplacingPartsAfterPunchOut = True
                        End If

                        'send error email to MDM 
                        If Trim(strPartsInOracleButNotInUnilog) <> "" Then
                            Dim strSubject As String = "ERROR: Products not in Catalog for " & Session("BUSUNIT")
                            Dim strErrorMessage As String = "The following items from BU: " & Session("BUSUNIT") & " - are in Peoplesoft but do not appear in the Catalog. " & vbCrLf &
                                "These are ordered in Punch Out and found incomplete in the system: " & vbCrLf &
                                " " & Trim(strPartsInOracleButNotInUnilog)
                            Try
                                SendSDiExchErrorMail(strErrorMessage, strSubject, True)
                            Catch exEml0987 As Exception

                            End Try
                        End If
                    End If  '  If arrReplaceWith.Count > 0 Then

                    Dim in2d As Integer = 0
                    Dim iCtr2 As Integer = 0
                    If arrSTKConfirmedToDelete.Count > 0 Then
                        For iCtr2 = 0 To arrSTKConfirmedToDelete.Count - 1
                            in2d = arrSTKConfirmedToDelete(iCtr2)
                            dstCart4.Rows(in2d).Delete()
                        Next
                        dstCart4.AcceptChanges()
                        bIsReplacingPartsAfterPunchOut = True
                    End If  '  If arrSTKConfirmedToDelete.Count > 0

                End If

            End If

            Session("Cart") = dstCart4

            If ExideErrorHandling(Session("USERID"), Session("BUSUNIT"), "shoppingcart.aspx.vb > buildXMLGrid") = True Then
                Response.Redirect(Session("DEFAULTPAGE").ToString() & "?ShopCartProblem=YES")
            End If

            rgCart.DataSource = Session("Cart")
            rgCart.DataBind()

            arr = Nothing

            If strRemoveStockFlag = "Y" Then
                ltlAlert.Text = strMessage.Say("Stock Items removed from cart.\nPlease use the Mobile Issuing\nprogram to request stock.")

            Else

                If bHaveChanges Then
                    'Save change(s) to the Persistent Cart - VR 05/01/2017
                    ' adding one more flag - bIsReplacingPartsAfterPunchOut
                    WriteToUserCart(Session("USERID"), Session("BUSUNIT"), bIsReplacingPartsAfterPunchOut)
                    '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                End If

            End If
        Catch ex As Exception
            Dim sMyErrorString As String = ""
            sMyErrorString = "Error in ShoppingCart.aspx.vb -> buildXMLGrid: " & ex.Message & " ; " & vbCrLf &
            "UserID = " & Session("USERID") & "; Business Unit = " & Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID")

            SendSDiExchErrorMail(sMyErrorString, "Error in ShoppingCart.aspx.vb -> buildXMLGrid")

        End Try

    End Sub

    'Private Function AddNewColumns(ByVal dstcart As DataTable) As DataTable
    '    Try
    '        If Not dstcart.Columns.Contains("UPriority") Then
    '            dstcart.Columns.Add("UPriority", GetType(Boolean))
    '            dstcart.Columns.Add("UDueDate", GetType(Date))
    '            dstcart.Columns.Add("UType", GetType(String))
    '            dstcart.Columns.Add("UNotes", GetType(String))
    '        End If
    '    Catch ex As Exception
    '    End Try
    '    Return dstcart
    'End Function

    Private Function checkLineItemChgCd() As Boolean

        Dim I As Int16
        For I = 0 To rgCart.Items.Count - 1
            Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
            Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
            'If CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtItemChgCode"), TextBox).Text = " " Or _
            'CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtItemChgCode"), TextBox).Text = "" Then
            If CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text = " " Or
                CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text = "" Then
                checkLineItemChgCd = False
                Exit Function
            End If
        Next
        checkLineItemChgCd = True
    End Function

    Private Function checkLineEmpChgCd() As Boolean

        Dim I As Int16
        For I = 0 To rgCart.Items.Count - 1
            If CType(rgCart.Items.Item(I).FindControl("txtEmpChgCode"), TextBox).Text.Trim() = "" Then
            End If
        Next
        checkLineEmpChgCd = True
        'Dim I As Int16
        'For I = 0 To dtgCart.Items.Count - 1
        '    'If CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtEmpChgCode"), TextBox).Text = " " Or _
        '    'CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtEmpChgCode"), TextBox).Text = "" Then
        '    If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtEmpChgCode"), TextBox).Text = " " Or _
        '        CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtEmpChgCode"), TextBox).Text = "" Then
        '        checkLineEmpChgCd = False
        '        Exit Function
        '    End If
        'Next
        'checkLineEmpChgCd = True
    End Function

    Private Function checkCatLPP(ByVal strOROItemID) As Boolean

        Dim strItemID As String
        Dim strSQLstring = "SELECT A.INV_ITEM_ID" & vbCrLf &
                    " FROM PS_ISA_INIT_DATA A" & vbCrLf &
                    " WHERE A.SETID = 'MAIN1'" & vbCrLf &
                    " AND A.INV_ITEM_ID = '" & strOROItemID & "'"

        strItemID = ORDBData.GetScalar(strSQLstring)
        If Trim(strItemID) = "" Then
            checkCatLPP = False
        Else
            checkCatLPP = True
        End If

    End Function

    Private Sub dropEmpID_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dropEmpID.SelectedIndexChanged
        updateCartDatatable()
        'If Trim(dropEmpID.Text) = "" Then
        '    Exit Sub
        'End If
        'If Not dropEmpID.Items.FindByValue(strAgent) Is Nothing Then
        '    dropEmpID.Items.FindByValue(strAgent).Selected = True
        '    dropEmpID.Text = dropEmpID.SelectedItem.Text
        '    'dropEmpID.Text = dropEmpID.Items.FindByValue(strAgent.ToUpper).Text
        '    'Try
        '    '    txtEmpID2.Text = dropEmpID.SelectedValue
        '    'Catch ex As Exception
        '    'End Try
        'End If

    End Sub

    'Private Sub dtgCart_itemcommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles dtgCart.ItemCommand

    '    Dim tmp As String = ""
    '    Dim script As String = ""
    '    Dim strChgCdUser As String
    '    Dim strMachineRow As String
    '    Dim strScript As String = ""
    '    strChgCdUser = strAgent
    '    If Session("SDIEMP") = "SDI" Then
    '        strChgCdUser = dropEmpID.SelectedValue
    '    End If

    '    If e.CommandName = "ItemChgCode" Then
    '        txtItemChgCodeItem.Text = e.Item.ItemIndex
    '        RequiredFieldValidator1.Enabled = False

    '        tmp = "buildchrcd.aspx?BU=" & strBU & "&LINE=Y&USER=" & strChgCdUser & "&HOME=N&FROMRAD=RAD"

    '        RadWindowChCd.NavigateUrl = tmp
    '        RadWindowChCd.OnClientClose = "OnClientCloseFromGrid"
    '        RadWindowChCd.Title = "Enter Charge Code"
    '        script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
    '        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

    '    ElseIf e.CommandName = "EmpChgCode" Then
    '        txtEmpChgCodeItem.Text = e.Item.ItemIndex
    '        Requiredfieldvalidator3.Enabled = False
    '        'strScript = "<script>"
    '        'strScript = strScript & "var strReturn; strReturn=window.showModalDialog('buildempchgcd.aspx?BU=" & strBU & "&LINE=Y&USER=" & strChgCdUser & "&HOME=N',null,'status:no;dialogWidth:700px;dialogHeight:500px;dialogHide:true;help:no;scroll:no');if (strReturn != null) document.getElementById('txtEmpChgCodeHide').value=strReturn;"
    '        'strScript = strScript & "document.frmCartProcess.submit();"
    '        'strScript = strScript & "</script>"
    '        'Page.RegisterStartupScript("ClientScript", strScript)

    '        ''new RadWindow code  
    '        'tmp = "http://" & Request.ServerVariables("HTTP_HOST") & GetWebAppName1()
    '        'tmp += "/buildempchgcd.aspx?BU=" & strBU & "&LINE=Y&USER=" & strChgCdUser & "&HOME=N&FROMRAD=RAD"

    '        tmp = "buildempchgcd.aspx?BU=" & strBU & "&LINE=Y&USER=" & strChgCdUser & "&HOME=N&FROMRAD=RAD"

    '        RadWindowChCd.NavigateUrl = tmp
    '        RadWindowChCd.OnClientClose = "EmpChCdOnClientClose"
    '        RadWindowChCd.Title = "Employee Charge Code"
    '        script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
    '        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

    '    ElseIf e.CommandName = "MachineRow" Then
    '        txtMachineRowItem.Text = e.Item.ItemIndex

    '        'strScript = "<script>"
    '        'strScript = strScript & "var strReturn; strReturn=window.showModalDialog('buildMachineRow.aspx?BU=" & strBU & "&LINE=Y&MN=" & strMachineRow & "&HOME=N',null,'status:no;dialogWidth:700px;dialogHeight:500px;dialogHide:true;help:no;scroll:no');if (strReturn != null) document.getElementById('txtMachineRowHide').value=strReturn;"

    '        'strScript = strScript & "document.frmCartProcess.submit();"
    '        'strScript = strScript & "</script>"
    '        'Page.RegisterStartupScript("ClientScript", strScript)

    '        ''new RadWindow code  
    '        'tmp = "http://" & Request.ServerVariables("HTTP_HOST") & GetWebAppName1()
    '        'tmp += "/buildMachineRow.aspx?BU=" & strBU & "&LINE=Y&MN=" & strMachineRow & "&HOME=N&FROMRAD=RAD"

    '        tmp = "buildMachineRow.aspx?BU=" & strBU & "&LINE=Y&MN=" & strMachineRow & "&HOME=N&FROMRAD=RAD"

    '        RadWindowChCd.NavigateUrl = tmp
    '        RadWindowChCd.OnClientClose = "MachineOnClientClose"
    '        RadWindowChCd.Title = "Machine Row"
    '        script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
    '        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

    '    ElseIf e.CommandName = "AddtoFavs" Then

    '        'Dim stritemid As String = e.Item.Cells(0).Text
    '        'Dim stritemid As String = e.Item.Cells(dtgCartColumns.itemId).Text

    '        Dim lblItemID As Label = CType(e.Item.Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)
    '        Dim stritemid As String = lblItemID.Text

    '        Dim arrNSTKinfo As New ArrayList(5)
    '        arrNSTKinfo.Insert(0, stritemid)

    '        arrNSTKinfo.Insert(1, e.Item.Cells(dtgCartColumns.itemDescription).Text)
    '        arrNSTKinfo.Insert(2, e.Item.Cells(dtgCartColumns.mfg).Text)
    '        arrNSTKinfo.Insert(3, e.Item.Cells(dtgCartColumns.mfgPartNo).Text)
    '        arrNSTKinfo.Insert(4, e.Item.Cells(dtgCartColumns.itemUOM).Text)
    '        Session("NSTKInfo") = arrNSTKinfo

    '        If lblItemID.Text.ToUpper = e.Item.Cells(dtgCartColumns.itemId_hidden).Text.ToUpper Then
    '            stritemid = getcpjunc(lblItemID.Text.ToUpper)
    '        End If
    '        'strScript = "<script>"
    '        'strScript = strScript & "var strReturn; strReturn=window.showModalDialog('fasvadds.aspx?ItemID=" & stritemid & "&CitemID=" & lblItemID.Text.ToUpper & "&BU=" & strBU & "&USER=" & strAgent & "',null,'status:no;dialogWidth:600px;dialogHeight:400px;dialogHide:true;help:no;scroll:no');if (strReturn != null) document.getElementById('txtFavItemIDHide').value=strReturn;"

    '        'strScript = strScript & "document.frmCartProcess.submit();"
    '        'strScript = strScript & "</script>"
    '        'Page.RegisterStartupScript("ClientScript", strScript)

    '        ''new RadWindow code  
    '        'tmp = "http://" & Request.ServerVariables("HTTP_HOST") & GetWebAppName1()
    '        'tmp += "/fasvadds.aspx?ItemID=" & stritemid & "&CitemID=" & lblItemID.Text.ToUpper & "&BU=" & strBU & "&USER=" & strAgent & "&FROMRAD=RAD"

    '        tmp = "fasvadds.aspx?ItemID=" & stritemid & "&CitemID=" & lblItemID.Text.ToUpper & "&BU=" & strBU & "&USER=" & strAgent & "&FROMRAD=RAD"

    '        RadWindowChCd.NavigateUrl = tmp
    '        RadWindowChCd.OnClientClose = "FavsOnClientClose"
    '        RadWindowChCd.Title = "Add to Favorites"
    '        script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
    '        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

    '        ' Add new item to menu

    '        '    Dim stritemid As String = e.Item.Cells(0).Text
    '        '    Dim strSQLString As String
    '        '    Dim strCategory As String
    '        '    If e.Item.Cells(0).Text.ToUpper = e.Item.Cells(11).Text.ToUpper Then
    '        '        stritemid = getcpjunc(e.Item.Cells(0).Text.ToUpper)
    '        '    End If

    '        '    Dim objCplusItem As New clsCplusItem(Convert.ToInt32(stritemid))
    '        '    strCategory = objCplusItem.classname

    '        '    If WebPartnerFunctions.WebPSharedFunc.CheckFavExist(strBU, strAgent, stritemid) = False Then
    '        '        strSQLString = "INSERT INTO PS_ISA_CP_FAVS" & vbCrLf & _
    '        '                    " ( BUSINESS_UNIT, EMPLID, ISA_FAVS_CATEGORY," & vbCrLf & _
    '        '                    " ISA_CP_ITEM_ID, ISA_CP_FAV_ALT_DSC, ADD_DTTM )" & vbCrLf & _
    '        '                    " VALUES ('" & strBU & "'," & vbCrLf & _
    '        '                    " '" & strAgent & "'," & vbCrLf & _
    '        '                    " '" & strCategory & "'," & vbCrLf & _
    '        '                    " '" & stritemid & "', ' '," & vbCrLf & _
    '        '                    " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
    '        '    Else
    '        '        strSQLString = "UPDATE PS_ISA_CP_FAVS" & vbCrLf & _
    '        '                    " SET ADD_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
    '        '                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
    '        '                    " AND EMPLID = '" & strAgent & "'" & vbCrLf & _
    '        '                    " AND ISA_CP_ITEM_ID = '" & stritemid & "'"
    '        '    End If

    '        '    Dim rowsaffected As Integer = ExecNonQuery(strSQLString)
    '        '    If rowsaffected = 0 Then
    '        '        lblDBError.Text = "Error Updating ISA_CP_FAVS Table"
    '        '        lblDBError.Visible = True
    '        '    End If

    '    End If

    'End Sub

    'Private Sub dtgCart_ItemDataBound(ByVal sender As Object, ByVal e As DataGridItemEventArgs) Handles dtgCart.ItemDataBound
    '    Try

    '        Dim c As CheckBox
    '        Dim l As LinkButton
    '        Dim m As Button
    '        Dim t As TextBox
    '        Dim LPP As TextBox
    '        Dim PO As TextBox
    '        Dim LN As TextBox
    '        Dim divPO As HtmlInputText
    '        Dim divLN As HtmlInputText
    '        Dim SerialID As Label

    '        If e.Item.ItemType = ListItemType.Header Then

    '            Dim objEnterprise As New clsEnterprise(strBU)
    '            strTaxFlag = objEnterprise.TaxFlag
    '            If Session("PUNCHIN") = "YES" Then
    '                'dtgCart.Columns(10).Visible = False
    '                dtgCart.Columns(dtgCartColumns.templateColumn1).Visible = False
    '                'dtgCart.Columns(12).Visible = False
    '                dtgCart.Columns(dtgCartColumns.lineItemInfo).Visible = False
    '            End If

    '            Try
    '                If Not (m_siteCurrency Is Nothing) Then
    '                    e.Item.Cells(dtgCartColumns.itemPrice).Text = "Price<br>(" & m_siteCurrency.Id & ")"
    '                    e.Item.Cells(dtgCartColumns.itemExtendedPrice).Text = "Ext. Price<br>(" & m_siteCurrency.Id & ")"
    '                End If
    '            Catch ex As Exception
    '            End Try



    '        ElseIf e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then

    '            Dim StrImg As String = Convert.ToString(e.Item.Cells(20).Text)

    '            ' New Image Binding code 
    '            If Not String.IsNullOrEmpty(StrImg) And Not StrImg = "&nbsp;" Then
    '                CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("imgProduct"), Image).ImageUrl = StrImg
    '            Else
    '                CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("imgProduct"), Image).Visible = False
    '            End If


    '            'If Not StrImg = "&nbsp;" And File.Exists(StrImg) And Not String.IsNullOrEmpty(StrImg) Then
    '            '    CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("imgProduct"), Image).ImageUrl = StrImg
    '            'Else
    '            '    CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("imgProduct"), Image).Visible = False
    '            'End If


    '            'e.Item.Cells(5).Attributes.Add("onkeypress", "CheckNumeric()")
    '            e.Item.Cells(dtgCartColumns.qtyToOrder).Attributes.Add("onkeypress", "CheckNumeric()")
    '            'l = CType(e.Item.Cells(11).FindControl("cmdFav"), LinkButton)
    '            l = CType(e.Item.Cells(dtgCartColumns.itemId_hidden).FindControl("cmdFav"), LinkButton)
    '            'l.Attributes.Add("onclick", "return getconfirm();")
    '            'LPP = CType(e.Item.Cells(12).FindControl("txtLPP"), TextBox)
    '            LPP = CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("txtLPP"), TextBox)
    '            SerialID = CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("lblSerialID"), Label)
    '            'c = CType(e.Item.Cells(12).FindControl("chbTaxFlag"), CheckBox)
    '            c = CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("chbTaxFlag"), CheckBox)

    '            'PO = CType(e.Item.Cells(12).FindControl("txtPO"), TextBox)
    '            PO = CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("txtPO"), TextBox)
    '            'LN = CType(e.Item.Cells(12).FindControl("txtLN"), TextBox)
    '            LN = CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("txtLN"), TextBox)
    '            If Not Session("SITEBU") = "SDM00" Then
    '                PO.Visible = False
    '                LN.Visible = False
    '                'CType(e.Item.Cells(12).FindControl("lblPO"), Label).Visible = False
    '                CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("lblPO"), Label).Visible = False
    '                'CType(e.Item.Cells(12).FindControl("lblLN"), Label).Visible = False
    '                CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("lblLN"), Label).Visible = False
    '                strblankrowSDM = "N"
    '            Else
    '                strblankrowSDM = "Y"
    '            End If

    '            'If Left(e.Item.Cells(0).Text, 6) = "NONCAT" Then
    '            '    l.Visible = False
    '            'End If

    '            'If Trim(LPP.Text) = "" Then
    '            'Dim pkey() As DataColumn = {Session("Cart").Columns("Itemid")}
    '            'Session("Cart").PrimaryKey = pkey
    '            KillPrimaryKey(Session("Cart"))

    '            ' Retrieve the ID of the product
    '            ''Dim ProductID As System.Int32 = System.Convert.ToInt32(dtgCart.Items.Item(I).Cells(1).Text)
    '            'Dim ProductID As String = e.Item.Cells(0).Text
    '            Dim lblItemID As Label = CType(e.Item.Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)
    '            Dim lblUniqNum As Label = CType(e.Item.Cells(dtgCartColumns.uniqNum_hidden).FindControl("lblUniqNum"), Label)
    '            Dim cmdFav As LinkButton = CType(e.Item.Cells(dtgCartColumns.itemId).FindControl("cmdFav"), LinkButton)
    '            If lblItemID.Text.Contains("NONCAT") Then
    '                cmdFav.Visible = False
    '            End If

    '            Dim ProductID As String = lblItemID.Text
    '            Dim row As DataRow

    '            If Not Session("Cart") Is Nothing Then
    '                Dim iRow As Integer
    '                If FindCartDTRow(Session("Cart"), ProductID, lblUniqNum.Text, iRow) Then
    '                    row = CType(Session("Cart"), DataTable).Rows(iRow)
    '                End If
    '            End If

    '            If Not Session("Cart") Is Nothing Then
    '                'Dim row As DataRow = Session("Cart").Rows.Find(ProductID)
    '                If Not row Is Nothing Then
    '                    Try
    '                        lblUniqNum.Text = row.Item(eCartCol.UniqNum)
    '                    Catch ex As Exception
    '                    End Try
    '                    'lblUniqNum.Text = row.Item(eCartCol.UniqNum)
    '                    If IsDBNull(row.Item("LPPExist")) Then
    '                        row.Item("LPPExist") = "NOT CHECKED"
    '                    End If
    '                    If Not row.Item("LPPExist") = "ALREADY CHECKED" And _
    '                        Not row.Item("LPPExist") = "ALREADY CHECKED AND EXIST" Then
    '                        'LPP.Text = getLPPExist(e.Item)
    '                        row.Item("LPPExist") = "ALREADY CHECKED"
    '                        row.Item("LPP") = LPP.Text
    '                        If Not Trim(LPP.Text) = "" Then
    '                            row.Item("LPPExist") = "ALREADY CHECKED AND EXIST"
    '                        End If
    '                    End If
    '                    'If row.Item("LPPExist") = "ALREADY CHECKED AND EXIST" Then
    '                    '    LPP.ReadOnly = True
    '                    'End If

    '                    If IsDBNull(row.Item("TaxFlag")) Then
    '                        row.Item("TaxFlag") = " "
    '                    End If
    '                    If row.Item("TaxFlag") = "Y" Then
    '                        c.Checked = True
    '                    Else
    '                        c.Checked = False
    '                    End If
    '                    If IsDBNull(row.Item("SupplierPartNumber")) Then
    '                        row.Item("TaxFlag") = " "
    '                    End If
    '                    If IsDBNull(row.Item("PricePO")) Then
    '                        row.Item("TaxFlag") = " "
    '                    End If
    '                End If
    '            End If
    '            'End If
    '            If Not Trim(LPP.Text) = "" Then
    '                LPP.Text = FormatNumber(LPP.Text, 2)
    '            End If
    '            LPP.Attributes.Add("onkeypress", "CheckNumeric()")
    '            If cmbMachine.Visible = False Then
    '                'm = CType(e.Item.Cells(12).FindControl("btnMachineRow"), Button)
    '                m = CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("btnMachineRow"), Button)
    '                't = CType(e.Item.Cells(12).FindControl("txtMachineRow"), TextBox)
    '                t = CType(e.Item.Cells(dtgCartColumns.lineItemInfo).FindControl("txtMachineRow"), TextBox)
    '                'l.Attributes.Add("onclick", "return getconfirm();")
    '                m.Visible = False
    '                t.Visible = False
    '            End If

    '            If Not strTaxFlag = "Y" Then
    '                c.Visible = False
    '            Else
    '                Dim strInvItemId As String = String.Empty
    '                If Not ProductID.Substring(0, 3) = Session("SITEPREFIX") Then
    '                    strInvItemId = Session("SITEPREFIX") & ProductID
    '                Else
    '                    strInvItemId = ProductID
    '                End If
    '                Dim objclsInvItemID As New clsInvItemID(strInvItemId)
    '                If objclsInvItemID.InvStockType = "STK" Then
    '                    c.Visible = False
    '                End If
    '            End If

    '            ' item quantity
    '            Dim qty As Decimal = CDec("0")
    '            Try
    '                Dim txtItemQty As TextBox = CType(e.Item.Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox)
    '                qty = CDec(txtItemQty.Text)

    '                Dim btnItemPickSerial As Button = CType(e.Item.Cells(dtgCartColumns.qtyToOrder).FindControl("btnPickSerialID"), Button)
    '                Dim lblItemSerialID As Label = CType(e.Item.Cells(dtgCartColumns.qtyToOrder).FindControl("lblSerialID"), Label)
    '                Dim bIsSerialized As Boolean = False

    '                If Not Session("Cart") Is Nothing Then
    '                    'Dim row As DataRow = Session("Cart").Rows.Find(lblItemID.Text)
    '                    If Not row Is Nothing Then
    '                        If IsDBNull(row.Item(eCartCol.SerializedCheck)) Then
    '                            If IsSerializedItem(lblItemID.Text) Then
    '                                row.Item(eCartCol.SerializedCheck) = GetSerialCheckText(True)
    '                                If qty > 1 Then
    '                                    txtItemQty.Text = "1"
    '                                    qty = CType(txtItemQty.Text, Decimal)
    '                                    UpdateDataSourceQtyForSerialized(e.Item.ItemIndex, txtItemQty.Text)
    '                                End If
    '                                bIsSerialized = True
    '                            Else
    '                                row.Item(eCartCol.SerializedCheck) = GetSerialCheckText(False)
    '                            End If
    '                            row.Item("SerialID") = SerialID.Text
    '                        ElseIf row.Item(eCartCol.SerializedCheck) = GetSerialCheckText(True) Then
    '                            bIsSerialized = True
    '                        End If
    '                    End If
    '                End If

    '                If bIsSerialized Then
    '                    txtItemQty.Enabled = False
    '                    txtItemQty.BackColor = LightGray
    '                    txtItemQty.ForeColor = Black
    '                    btnItemPickSerial.Visible = True
    '                    lblItemSerialID.Visible = True
    '                Else
    '                    btnItemPickSerial.Visible = False
    '                    lblItemSerialID.Visible = False
    '                End If

    '            Catch ex As Exception
    '            End Try

    '            ' item base price and currency
    '            '   base price is already being binded on catalogTree.aspx - erwin
    '            '   get item's base price; can hold "Price on Request"
    '            Dim itemBasePrice As String = ""
    '            Try
    '                itemBasePrice = CType(e.Item.Cells(dtgCartColumns.baseItemPriceAndCurrency_hidden).FindControl("lblItemBasePrice"), Label).Text
    '                If (itemBasePrice Is Nothing) Then
    '                    itemBasePrice = ""
    '                End If
    '            Catch ex As Exception
    '            End Try
    '            '   get SDI item Id (customerItemId)
    '            Dim sitePrefix As String = ""
    '            Try
    '                sitePrefix = CStr(Session("SITEPREFIX"))
    '                If (sitePrefix Is Nothing) Then
    '                    sitePrefix = ""
    '                End If
    '            Catch ex As Exception
    '            End Try
    '            Dim sdiItemId As String = ""
    '            Try
    '                sdiItemId = sitePrefix & e.Item.Cells(dtgCartColumns.itemId_hidden).Text
    '            Catch ex As Exception
    '            End Try
    '            '   get item's default (first available) currency 
    '            Dim itemBaseCurrency As sdiCurrency = Nothing
    '            Try
    '                itemBaseCurrency = sdiMultiCurrency.getItemBaseCurrency(sdiItemId)
    '                CType(e.Item.Cells(dtgCartColumns.baseItemPriceAndCurrency_hidden).FindControl("lblItemBaseCurrencyCode"), Label).Text = itemBaseCurrency.Id
    '            Catch ex As Exception
    '            End Try

    '            ' check item base currency against site currency
    '            '   convert item price to show if not the same
    '            Try
    '                If itemBasePrice.Trim.Length > 0 Then
    '                    If IsNumeric(itemBasePrice) Then
    '                        If m_siteCurrency.Id <> itemBaseCurrency.Id Then
    '                            Dim convRate As sdiConversionRate = sdiMultiCurrency.getConversionRate(itemBaseCurrency.Id, _
    '                                                                                                   CDec(itemBasePrice), _
    '                                                                                                   m_siteCurrency.Id)
    '                            ' convert price to site's currency; update shown prices
    '                            Dim nPrice As Decimal = Math.Round(convRate.ConvertedAmount, 2)
    '                            Dim nLineTotal As Decimal = Math.Round(nPrice * qty, 2)
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text = nPrice.ToString("####,###,##0.00")
    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("ExtPrice"), Label).Text = nLineTotal.ToString("####,###,##0.00")

    '                            ' show currency symbol; hide currency code
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencySymbol"), Label).Text = m_siteCurrency.Symbol
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencySymbol"), Label).Visible = True
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Text = convRate.TargetCurrencyCode
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Visible = False

    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencySymbol"), Label).Text = m_siteCurrency.Symbol
    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencySymbol"), Label).Visible = True
    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencyCode"), Label).Text = convRate.TargetCurrencyCode
    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencyCode"), Label).Visible = False
    '                        Else
    '                            ' show currency symbol; hide currency code
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencySymbol"), Label).Text = itemBaseCurrency.Symbol
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencySymbol"), Label).Visible = True
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Text = itemBaseCurrency.Id
    '                            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Visible = False

    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencySymbol"), Label).Text = itemBaseCurrency.Symbol
    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencySymbol"), Label).Visible = True
    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencyCode"), Label).Text = itemBaseCurrency.Id
    '                            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencyCode"), Label).Visible = False
    '                        End If
    '                    Else
    '                        ' hide both currency symbol and code, since its not numeric
    '                        CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencySymbol"), Label).Visible = False
    '                        CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Visible = False
    '                    End If
    '                End If
    '            Catch ex As Exception
    '            End Try
    '            'If (itemBasePrice.Trim.Length > 0) Then
    '            '    If IsNumeric(itemBasePrice) Then
    '            '        ' item unit price
    '            '        CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencySymbol"), Label).Text = m_siteCurrency.Symbol
    '            '        CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Text = m_siteCurrency.Id
    '            '        ' item extended price
    '            '        CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencySymbol"), Label).Text = m_siteCurrency.Symbol
    '            '        CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencyCode"), Label).Text = m_siteCurrency.Id
    '            '        ' override
    '            '        If m_siteCurrency.Id <> itemBaseCurrency.Id Then
    '            '            ' item unit price
    '            '            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencySymbol"), Label).Text = itemBaseCurrency.Symbol
    '            '            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Text = itemBaseCurrency.Id
    '            '            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Visible = True
    '            '            ' item extended price
    '            '            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencySymbol"), Label).Text = itemBaseCurrency.Symbol
    '            '            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencyCode"), Label).Text = itemBaseCurrency.Id
    '            '            CType(e.Item.Cells(dtgCartColumns.itemExtendedPrice).FindControl("lblExtPriceCurrencyCode"), Label).Visible = True
    '            '        End If
    '            '    End If
    '            'End If

    '            ' incorrect, should be changing QOH and not the price!
    '            '   - erwin 2010.02.03
    '            '' check if ORO item type
    '            'Dim bIsChkForORO As Boolean = False
    '            'If (itemBasePrice.Trim.Length > 0) Then
    '            '    If IsNumeric(itemBasePrice) Then
    '            '        bIsChkForORO = (CDec(itemBasePrice) = 0)
    '            '    Else
    '            '        bIsChkForORO = True
    '            '    End If
    '            'Else
    '            '    bIsChkForORO = True
    '            'End If
    '            'If bIsChkForORO Then
    '            '    Dim itm As sdiItem = sdiItem.GetItemInfo(ProductID)
    '            '    Select Case itm.StockType.Id.Trim.ToUpper
    '            '        Case "ORO"
    '            '            ' update price column to ORO and hide currency indicators
    '            '            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text = itm.StockType.Id
    '            '            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencySymbol"), Label).Visible = False
    '            '            CType(e.Item.Cells(dtgCartColumns.itemPrice).FindControl("lblItemCurrencyCode"), Label).Visible = False
    '            '    End Select
    '            '    itm = Nothing
    '            'End If

    '            ' item UNSPSC
    '            If Not Session("cart") Is Nothing Then
    '                Try
    '                    'Dim r As DataRow = CType(Session("Cart"), DataTable).Rows.Find(ProductID)
    '                    If Not row Is Nothing Then
    '                        If Not IsDBNull(row("ItemUNSPSC")) Then
    '                            CType(e.Item.Cells(dtgCartColumns.itemAttributes).FindControl("lblItemUNSPSC"), Label).Text = CStr(row("ItemUNSPSC"))
    '                        End If
    '                    End If
    '                Catch ex As Exception
    '                End Try
    '            End If

    '        End If

    '    Catch ex As Exception

    '    End Try
    'End Sub

    Private Function getBinLoc(ByVal stritemid) As String
        'changed to qty >0 rather than hitting the first bin in the array
        Dim strSQLString As String = "" &
                    "SELECT " & vbCrLf &
                    " (C.STORAGE_AREA ||" & vbCrLf &
                    "  C.STOR_LEVEL_1 ||" & vbCrLf &
                    "  C.STOR_LEVEL_2 ||" & vbCrLf &
                    "  C.STOR_LEVEL_3 ||" & vbCrLf &
                    "  C.STOR_LEVEL_4) as binloc " & vbCrLf &
                    "FROM " & vbCrLf &
                    " PS_INV_ITEMS B " & vbCrLf &
                    ",PS_PHYSICAL_INV C " & vbCrLf &
                    "WHERE B.INV_ITEM_ID = '" & stritemid & "' " & vbCrLf &
                    "  AND B.EFFDT = (" & vbCrLf &
                    "                 SELECT MAX(B_ED.EFFDT) FROM PS_INV_ITEMS B_ED " & vbCrLf &
                    "  	 	          WHERE B.SETID = B_ED.SETID " & vbCrLf &
                    "  		            AND B.INV_ITEM_ID = B_ED.INV_ITEM_ID " & vbCrLf &
                    "		            AND B_ED.EFFDT <= SYSDATE" & vbCrLf &
                    "                ) " & vbCrLf &
                    "  AND B.INV_ITEM_ID = C.INV_ITEM_ID(+) " & vbCrLf &
                    " AND C.QTY > 0 " & vbCrLf &
                    "ORDER BY C.DT_TIMESTAMP DESC " & vbCrLf &
                    ""

        Try
            'pfd change the getscalar to getadapter - to creates a dataset to have multiple bin locations to be displayed on the email

            Dim dsbin As DataSet = ORDBData.GetAdapter(strSQLString)
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

            sendErrorEmail(objException.ToString, "NO", Request.ServerVariables("URL"), strSQLString)
            Response.Redirect("DBErrorPage.aspx?HOME=N")
        End Try

    End Function

    'Private Sub getEmpIDName()
    '    If Not dropEmpID.Items.FindByValue(txtEmpID2.Text.ToUpper) Is Nothing Then
    '        dropEmpID.Items.FindByValue(txtEmpID2.Text.ToUpper).Selected = True
    '        dropEmpID.Text = dropEmpID.Items.FindByValue(txtEmpID2.Text.ToUpper).Text
    '        lblInvalidEmpID.Visible = False
    '    Else
    '        dropEmpID.Text = ""
    '        lblInvalidEmpID.Visible = True
    '    End If
    'End Sub

    'Private Function getLPPExist(ByVal item As DataGridItem) As String

    '    Dim strItemIDNew As String
    '    Dim I As Integer
    '    For I = 0 To rgCart.Items.Count - 1
    '        strItemIDNew = (CType(rgCart.Items.Item(I).FindControl("lblItemID"), Label).Text).ToString
    '    Next

    '    Dim strSQLString As String = "SELECT A.ISA_LPP_PRICE" & vbCrLf & _
    '                " FROM PS_ISA_LPP_ITEM A" & vbCrLf & _
    '                " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
    '                " AND (A.INV_ITEM_ID = '" & strItemIDNew & "'" & vbCrLf & _
    '                " OR (A.MFG_ITM_ID = '" & item.Cells(dtgCartColumns.mfgPartNo).Text & "'" & vbCrLf & _
    '                " AND A.ISA_MFG_FREEFORM = '" & item.Cells(dtgCartColumns.mfg).Text & "'))" & vbCrLf
    '    '" AND (A.INV_ITEM_ID = '" & item.Cells(0).Text & "'" & vbCrLf & _
    '    '" AND A.ISA_MFG_FREEFORM = '" & item.Cells(2).Text & "'))" & vbCrLf
    '    '" OR (A.MFG_ITM_ID = '" & item.Cells(3).Text & "'" & vbCrLf & _

    '    Try
    '        getLPPExist = ORDBData.GetScalar(strSQLString)
    '        Return getLPPExist
    '    Catch objException As Exception
    '        'Response.Write("We're sorry, we are experiencing technical problems...")
    '        'Response.Write("<hr>")
    '        'Response.Write("<li>Message: " & objException.Message)
    '        'Response.Write("<li>Source: " & objException.Source)
    '        'Response.Write("<li>Stack Trace: " & objException.StackTrace)
    '        'Response.Write("<li>Target Site: " & objException.TargetSite.Name)
    '        'Response.End()
    '        sendErrorEmail(objException.ToString, "NO", Request.ServerVariables("URL"), strSQLString)
    '        Response.Redirect("DBErrorPage.aspx?HOME=N")
    '    End Try

    'End Function

    Private Function getParentID(ByVal strSQLString) As Integer

        Dim intParentid As Integer

        Try
            intParentid = ORDBData.GetScalar(strSQLString)
            Return intParentid
        Catch objException As Exception
            'Response.Write("We're sorry, we are experiencing technical problems...")
            'Response.Write("<hr>")
            'Response.Write("<li>Message: " & objException.Message)
            'Response.Write("<li>Source: " & objException.Source)
            'Response.Write("<li>Stack Trace: " & objException.StackTrace)
            'Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            'Response.End()
            sendErrorEmail(objException.ToString, "NO", Request.ServerVariables("URL"), strSQLString)
            Response.Redirect("DBErrorPage.aspx")

        End Try

    End Function

    '''''''''''''''''''''''''''''Ticket 154676''''''''''''''''''''''''''
    Private Function getExemptStatus(ByVal strItemID, ByVal strBU) As String
        Try
            Dim arrChrCdRules As ArrayList
            Dim I As Integer
            'Dim bolItemSeq As Boolean = False

            arrChrCdRules = New ArrayList(5)

            ' SQL Statement
            Dim strSQL As String
            strSQL = "SELECT BUSINESS_UNIT," & vbCrLf &
                " ISA_CHG_CD_SEG_1," & vbCrLf &
                " ISA_CHG_CD_SEG_2," & vbCrLf &
                " ISA_CHG_CD_SEG_3," & vbCrLf &
                " ISA_CHG_CD_SEG_4," & vbCrLf &
                " ISA_CHG_CD_SEG_5," & vbCrLf &
                " ISA_CHG_CD_SEP" & vbCrLf &
                " FROM SYSADM8.PS_ISA_BUS_UNIT_OM" & vbCrLf &
                " where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf

            Dim dtrChrCDReader As OleDbDataReader = ORDBData.GetReader(strSQL)

            Try
                If dtrChrCDReader.HasRows() = True Then
                    dtrChrCDReader.Read()
                    arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_1"))
                    arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_2"))
                    arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_3"))
                    arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_4"))
                    arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_5"))

                    'If dtrChrCDReader.Item("ISA_CHG_CD_SEP_FLG") = "Y" Then
                    '    strChgCdSep = dtrChrCDReader.Item("ISA_CHG_CD_SEP")
                    'End If

                    Dim strChgCdSep1 As String = ""
                    Try
                        strChgCdSep1 = dtrChrCDReader.Item("ISA_CHG_CD_SEP")
                        If Trim(strChgCdSep1) = "" Then
                            strChgCdSep1 = ""
                        Else
                            strChgCdSep1 = Trim(strChgCdSep1)
                        End If
                    Catch ex As Exception
                        strChgCdSep1 = ""
                    End Try

                    strChgCdSep = strChgCdSep1

                    'For I = 0 To arrChrCdRules.Count - 1
                    '    If arrChrCdRules(I) = "I" Then
                    '        bolItemSeq = True
                    '    End If
                    'Next
                End If
                dtrChrCDReader.Close()
            Catch objException As Exception
                dtrChrCDReader.Close()
                sendErrorEmail(objException.ToString, "NO", Request.ServerVariables("URL"), strSQL)
                Response.Redirect("DBErrorPage.aspx")

            End Try

            'YA 20191218 Fix for cases where entries don't exist in PS_ISA_BUS_UNIT_OM (i.e. Henkel A26-A29)
            If arrChrCdRules.Count = 0 Then
                Return " "
            End If

            Dim chgCodes As String() = strItemID.ToString.Split(strChgCdSep)
            Dim iTaxGroup As Integer = 0
            Dim strItemChgCD As String = "EXEMP_CERT" 'default

            For I = 0 To UBound(chgCodes) ' 4

                Select Case arrChrCdRules(I).ToString
                    Case "1"
                        strSQL = "SELECT TAX_GROUP FROM SYSADM8.PS_ISA_CHGCD_USR1 where BUSINESS_UNIT = '" & strBU & "' " & vbCrLf &
                            " AND ISA_CHGCD_USR1_SEG = '" & chgCodes(I).ToString & "'"
                    Case "2"
                        strSQL = "SELECT TAX_GROUP FROM SYSADM8.PS_ISA_CHGCD_USR2 where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                            " AND ISA_CHGCD_USR2_SEG = '" & chgCodes(I).ToString & "'"
                    Case "3"
                        strSQL = "SELECT TAX_GROUP FROM SYSADM8.PS_ISA_CHGCD_USR3 where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                            " AND ISA_CHGCD_USR3_SEG = '" & chgCodes(I).ToString & "'"

                        'Case "M"
                        '    strSQL = "SELECT (ISA_MACHINE_NO || ISA_CUST_CHARGE_CD || ' ' || DESCR60) AS ISA_CUST_CHARGE_CD" & vbCrLf & _
                        '                ",ISA_CUST_CHARGE_CD as SEQ" & vbCrLf & _
                        '                " FROM SYSADM8.PS_ISA_MCHNE_CHGCD" & vbCrLf & _
                        '                " where  BUSINESS_UNIT = '" & strBU & "' " & vbCrLf & _
                        '                " ORDER BY (ISA_MACHINE_NO || ISA_CUST_CHARGE_CD)"
                        '    strItemChgCD = ORDBData.GetScalar(strSQL)

                        'Case "E"
                        '    strSQL =" SELECT " & vbCrLf & _
                        '       "  (ISA_CUST_CHARGE_CD || ' ' || DESCR60) AS ISA_CUST_CHARGE_CD" & vbCrLf & _
                        '       " ,ISA_CUST_CHARGE_CD AS SEQ" & vbCrLf & _
                        '       " FROM SYSADM8.PS_ISA_EMPL_CHR_CD" & vbCrLf & _
                        '       " where  BUSINESS_UNIT = '" & strBU  & "'" & vbCrLf & _
                        '       " AND ISA_EMPLOYEE_ID = '" & strUserid & "'" & vbCrLf & _
                        '       " AND ISA_CUST_CHARGE_CD <> ' '" & vbCrLf & _

                    Case "I"
                        strSQL = "SELECT TAX_GROUP FROM SYSADM8.PS_ISA_CHGCD_ITEM where BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                         " AND ISA_CHGCD_ITEM_SEG = '" & chgCodes(I).ToString & "'"

                    Case "P"
                        strSQL = "SELECT TAX_GROUP FROM SYSADM8.PS_ISA_CHGCD_CHLD1 where BUSINESS_UNIT = '" & strBU & "' " & vbCrLf &
                         " AND ISA_CHGCD_CHILD1 = '" & chgCodes(I).ToString & "'"

                    Case "F"
                        strSQL = "SELECT TAX_GROUP FROM SYSADM8.PS_ISA_CHGCD_CHLD2 where BUSINESS_UNIT = '" & strBU & "' " & vbCrLf &
                         " AND ISA_CHGCD_CHILD2 = '" & chgCodes(I).ToString & "'"

                    Case "S"
                        strSQL = "SELECT TAX_GROUP FROM SYSADM8.ps_isa_chgcd_CHLD3 where BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                         " AND ISA_CHGCD_CHILD3 = '" & chgCodes(I).ToString & "'"

                        'Case Else

                        '    strItemChgCD = "N"
                        '    Exit For
                End Select

                strItemChgCD = ORDBData.GetScalar(strSQL)
                If strItemChgCD <> "EXEMP_CERT" Then
                    strItemChgCD = " "
                    Exit For
                End If
            Next
            If strItemChgCD = "EXEMP_CERT" Then strItemChgCD = "Y"

            Return strItemChgCD
        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart.aspx.vb - getExemptStatus function. User ID: " & Session("USERID") & "; " & vbCrLf &
                          "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                           "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart Page Load Event")
        End Try

    End Function


    Private Function getItemChgCD(ByVal strItemID, ByVal strBU) As String

        Dim arrChrCdRules As ArrayList
        Dim I As Integer
        Dim bolItemSeq As Boolean = False

        arrChrCdRules = New ArrayList(5)

        ' SQL Statement
        Dim strSQL As String
        strSQL = "SELECT BUSINESS_UNIT," & vbCrLf &
            " ISA_CHG_CD_SEG_1," & vbCrLf &
            " ISA_CHG_CD_SEG_2," & vbCrLf &
            " ISA_CHG_CD_SEG_3," & vbCrLf &
            " ISA_CHG_CD_SEG_4," & vbCrLf &
            " ISA_CHG_CD_SEG_5," & vbCrLf &
            " ISA_CHG_CD_SEP" & vbCrLf &
            " FROM SYSADM8.PS_ISA_BUS_UNIT_OM" & vbCrLf &
            " where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf
        ' Command, Data Reader  

        Dim dtrChrCDReader As OleDbDataReader = ORDBData.GetReader(strSQL)

        Try
            If dtrChrCDReader.HasRows() = True Then
                dtrChrCDReader.Read()
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_1"))
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_2"))
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_3"))
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_4"))
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_5"))

                'If dtrChrCDReader.Item("ISA_CHG_CD_SEP_FLG") = "Y" Then
                '    strChgCdSep = dtrChrCDReader.Item("ISA_CHG_CD_SEP")
                'End If

                Dim strChgCdSep1 As String = ""
                Try
                    strChgCdSep1 = dtrChrCDReader.Item("ISA_CHG_CD_SEP")
                    If Trim(strChgCdSep1) = "" Then
                        strChgCdSep1 = ""
                    Else
                        strChgCdSep1 = Trim(strChgCdSep1)
                    End If
                Catch ex As Exception
                    strChgCdSep1 = ""
                End Try

                strChgCdSep = strChgCdSep1

                For I = 0 To arrChrCdRules.Count - 1
                    If arrChrCdRules(I) = "I" Then
                        bolItemSeq = True
                    End If
                Next
            End If
            dtrChrCDReader.Close()
        Catch objException As Exception
            dtrChrCDReader.Close()
            sendErrorEmail(objException.ToString, "NO", Request.ServerVariables("URL"), strSQL)
            Response.Redirect("DBErrorPage.aspx")

        End Try
        If bolItemSeq = False Then
            Return ""
        End If
        Dim sqlString = "SELECT ISA_CUST_CHARGE_CD" & vbCrLf &
                " FROM PS_MASTER_ITEM_TBL" & vbCrLf &
                " WHERE INV_ITEM_ID = '" & strItemID & "'"

        Dim strItemChgCD As String = ORDBData.GetScalar(sqlString)
        If IsDBNull(strItemChgCD) Then
            strItemChgCD = ""
        ElseIf strItemChgCD = " " Then
            strItemChgCD = ""
        ElseIf strItemChgCD = "0" Then
            strItemChgCD = ""
        End If

        Return strItemChgCD

    End Function

    Private Function getOrderTot() As Decimal
        'Dim I As Integer
        'Dim strPrice As String
        Dim nTotal As Decimal = 0
        'For I = 0 To dtgCart.Items.Count - 1
        '    'If CType(dtgCart.Items.Item(I).Cells(6).FindControl("Price"), Label).Text = "Price on Request" Then
        '    If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text = "Price on Request" Then
        '        strPrice = "0.00"
        '    Else
        '        'strPrice = CType(dtgCart.Items.Item(I).Cells(6).FindControl("Price"), Label).Text
        '        strPrice = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text
        '    End If

        '    'CType(dtgCart.Items.Item(I).Cells(7).FindControl("ExtPrice"), Label).Text = _
        '    'Convert.ToString(Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(5).FindControl("txtQTY"), TextBox).Text) * _
        '    CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemExtendedPrice).FindControl("ExtPrice"), Label).Text = _
        '        Convert.ToString(Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text) * _
        '        Convert.ToDecimal(strPrice))
        '    'If Not CType(dtgCart.Items.Item(I).Cells(9).FindControl("chkDelete"), CheckBox).Checked Then
        '    If Not CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.deleteFlag).FindControl("chkDelete"), CheckBox).Checked Then
        '        'getOrderTot = getOrderTot + (System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(3).FindControl("txtQTY"), TextBox).Text) _
        '        getOrderTot = getOrderTot + (System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.mfgPartNo).FindControl("txtQTY"), TextBox).Text) _
        '       * System.Convert.ToDecimal(strPrice))
        '    End If
        'Next
        Dim bIsRemoveItem As Boolean = False
        Dim unitPrice As Decimal = 0
        Dim qtyOrdered As Decimal = 0
        Dim nLineTotal As Decimal = 0
        Dim I As Integer

        For I = 0 To rgCart.Items.Count - 1

            Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
            Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)

            bIsRemoveItem = False
            Try
                bIsRemoveItem = CType(nestedview.FindControl("chkDelete"), CheckBox).Checked
            Catch ex As Exception
            End Try
            If Not bIsRemoveItem Then
                ' get item price
                unitPrice = 0
                Try
                    If IsNumeric(CType(nestedview.FindControl("Price"), Label).Text) Then
                        unitPrice = CDec(CType(nestedview.FindControl("Price"), Label).Text)
                    End If
                Catch ex As Exception
                End Try
                ' get quantity
                qtyOrdered = 0
                Try
                    If IsNumeric(CType(nestedview.FindControl("txtQTY"), TextBox).Text) Then
                        qtyOrdered = CDec(CType(nestedview.FindControl("txtQTY"), TextBox).Text)
                    End If
                Catch ex As Exception
                End Try
                ' update extended price
                nLineTotal = Math.Round((unitPrice * qtyOrdered), 2)
                Try
                    CType(nestedview.FindControl("ExtPrice"), Label).Text = nLineTotal.ToString("####,###,##0.00")
                Catch ex As Exception
                End Try
                ' increment order total
                nTotal += nLineTotal
            End If
        Next
        Return (nTotal)
        'For Each itm As System.Web.UI.WebControls.DataGridItem In dtgCart.Items
        '    bIsRemoveItem = False
        '    Try
        '        bIsRemoveItem = CType(itm.Cells(dtgCartColumns.deleteFlag).FindControl("chkDelete"), CheckBox).Checked
        '    Catch ex As Exception
        '    End Try
        '    If Not bIsRemoveItem Then
        '        ' get item price
        '        unitPrice = 0
        '        Try
        '            If IsNumeric(CType(itm.Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text) Then
        '                unitPrice = CDec(CType(itm.Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text)
        '            End If
        '        Catch ex As Exception
        '        End Try
        '        ' get quantity
        '        qtyOrdered = 0
        '        Try
        '            If IsNumeric(CType(itm.Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text) Then
        '                qtyOrdered = CDec(CType(itm.Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text)
        '            End If
        '        Catch ex As Exception
        '        End Try
        '        ' update extended price
        '        nLineTotal = Math.Round((unitPrice * qtyOrdered), 2)
        '        Try
        '            CType(itm.Cells(dtgCartColumns.itemExtendedPrice).FindControl("ExtPrice"), Label).Text = nLineTotal.ToString("####,###,##0.00")
        '        Catch ex As Exception
        '        End Try
        '        ' increment order total
        '        nTotal += nLineTotal
        '    End If
        'Next
    End Function

    'Private Function getStocktype(ByVal strOROItemID) As String
    '    Dim strSQLstring As String = "SELECT A.INV_STOCK_TYPE" & vbCrLf & _
    '        " FROM PS_INV_ITEMS A" & vbCrLf & _
    '        " WHERE A.EFFDT =" & vbCrLf & _
    '        " (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf & _
    '        " WHERE(A.SETID = A_ED.SETID)" & vbCrLf & _
    '        " AND A.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
    '        " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
    '        " AND A.INV_ITEM_ID = '" & strOROItemID & "'"

    '    getStocktype = ORDBData.GetScalar(strSQLstring)
    '    If Trim(getStocktype) = "" Then
    '        getStocktype = "ONCE"
    '    End If

    'End Function

    Private Sub hideShoppingCartFields(Optional ByVal bShow As Boolean = False)
        If bShow Then
            Panel1.Visible = True
        Else
            Panel1.Visible = False
        End If
        Panel2.Visible = False
        lnkEditFavs.Visible = False

    End Sub

    Private Sub HidePunchInFields()
        Panel2.Visible = False
        lnkEditFavs.Visible = False
        divOrdCenterIn.Visible = False
        txtNotes.Visible = False
        divOrdStats1.Visible = False
        liEmpId1.Visible = False
        'To hide * required div and shipto validation for punchin users
        reqdDiv.Visible = False
        Customvalidator4.Visible = False
        ' liEmpId2.Visible = False
        RadDatePickerReqByDate.SelectedDate = DateAdd(DateInterval.Day, 7, DateTime.Now.Date)
        chbPriority.Font.Bold = True
        'Added to display shipto in shopping cart for henkel users alone
        If Insiteonline.VoucherSharedFunctions.VoucherClass.IsHenkel(strBU) Then
            shipToDiv.Visible = True
            'dropShipto.Visible = True
            divOrdLast.Style.Add("float", "Right")
        Else
            shipToDiv.Visible = False
            'dropShipto.Visible = False
        End If
    End Sub

    Private Function RemoveStockItems(ByRef dsCartin As DataTable) As String

        Dim I As Integer
        Dim X As Integer
        Dim strInvItemID As String
        Dim arr As New ArrayList

        If dsCartin Is Nothing Then
            Exit Function
        End If
        X = 0
        For I = 0 To dsCartin.Rows.Count - 1
            If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                strInvItemID = dsCartin.Rows(X).Item("customerItemID")
            ElseIf Not String.IsNullOrEmpty(dsCartin.Rows(X).Item("customerItemID").ToString().Trim()) Then
                If dsCartin.Rows(X).Item("customerItemID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                    strInvItemID = dsCartin.Rows(X).Item("customerItemID")
                Else
                    strInvItemID = Session("SITEPREFIX") & dsCartin.Rows(X).Item("customerItemID")
                End If
            End If
            Dim objclsInvItemID As New clsInvItemID(strInvItemID)
            If objclsInvItemID.InvStockType = "STK" Then
                'dsCartin.Rows(X).Delete()
                arr.Add(I)
                RemoveStockItems = "Y"
            Else
                X = X + 1
            End If
        Next

        If arr.Count > 0 Then
            For n As Integer = (arr.Count - 1) To 0 Step -1
                Dim idx As Integer = CInt(arr(n))
                dsCartin.Rows(idx).Delete()
            Next
        End If
        dsCartin.AcceptChanges()

    End Function

    Private Sub SetFocus(ByVal FocusControl As Control)

        Dim Script As New System.Text.StringBuilder
        Dim ClientID As String = FocusControl.ClientID

        ''With Script
        ''    .Append("<script language='javascript'>")
        ''    .Append("document.getElementById('")
        ''    .Append(ClientID)
        ''    .Append("').focus();")
        ''    .Append("</script>")
        ''End With
        'Script.Append("" & _
        '              "<script language='javascript'>" & vbCrLf & _
        '              " try {" & vbCrLf & _
        '              "   document.getElementById('" & ClientID & " ').focus();" & vbCrLf & _
        '              " };" & vbCrLf & _
        '              " catch ex {" & vbCrLf & _
        '              " };" & vbCrLf & _
        '              "</script>" & vbCrLf & _
        '              "")

        'RegisterStartupScript("setFocus", Script.ToString())

    End Sub

    'Private Sub updateCartDatatable()
    '    Dim I As Integer
    '    Dim dstcart1 As New DataTable
    '    dstcart1 = Session("Cart")

    '    If dstcart1 Is Nothing Then
    '        Exit Sub
    '    End If

    '    'Dim pkey() As DataColumn = {dstcart1.Columns("Itemid")}
    '    'dstcart1.PrimaryKey = pkey

    '    ' Loop through the rows in the datagrid looking for the delete checkbox
    '    If Not dtgCart Is Nothing Then
    '        If dtgCart.Items.Count > 0 Then
    '            For I = 0 To dtgCart.Items.Count - 1

    '                ' Retrieve the ID of the product
    '                ''Dim ProductID As System.Int32 = System.Convert.ToInt32(dtgCart.Items.Item(I).Cells(1).Text)
    '                'Dim ProductID As String = dtgCart.Items.Item(I).Cells(0).Text




    '                dtgCart.EditItemIndex = -1

    '                Dim lblItemID As Label = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)
    '                Dim lblUniqNum As Label = CType(dtgCart.Items(I).FindControl("lblUniqNum"), Label)
    '                'Dim ProductID As String = dtgCart.Items.Item(I).Cells(dtgCartColumns.itemId).Text
    '                Dim ProductID As String = lblItemID.Text

    '                'Dim row As DataRow = dstcart1.Rows.Find(ProductID)
    '                Dim iRow As Integer
    '                If FindCartDTRow(dstcart1, ProductID, lblUniqNum.Text, iRow) Then
    '                    Dim row As DataRow = dstcart1.Rows(iRow)
    '                    ' See if this one needs to be deleted.
    '                    'If CType(dtgCart.Items.Item(I).Cells(9).FindControl("chkDelete"), CheckBox).Checked = True Or _
    '                    'System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(5).FindControl("txtQTY"), TextBox).Text) = 0 Then
    '                    If Not row Is Nothing Then
    '                        If Not Trim(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text) = "" Then
    '                            If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.deleteFlag).FindControl("chkDelete"), CheckBox).Checked = True Or _
    '                                System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text) = 0 Then
    '                                row.Delete()
    '                            Else
    '                                'row.Item("Quantity") = System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(5).FindControl("txtQTY"), TextBox).Text)
    '                                row.Item("Quantity") = System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text)
    '                                'row.Item("ItemChgCode") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtItemChgCode"), TextBox).Text
    '                                row.Item("ItemChgCode") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtItemChgCode"), TextBox).Text
    '                                'row.Item("EmpChgCode") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtEmpChgCode"), TextBox).Text
    '                                row.Item("EmpChgCode") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtEmpChgCode"), TextBox).Text
    '                                'row.Item("MachineRow") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtMachineRow"), TextBox).Text
    '                                row.Item("MachineRow") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtMachineRow"), TextBox).Text
    '                                'row.Item("LPP") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtLPP"), TextBox).Text
    '                                row.Item("LPP") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtLPP"), TextBox).Text
    '                                'row.Item("PO") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtPO"), TextBox).Text
    '                                row.Item("PO") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtPO"), TextBox).Text
    '                                row.Item("LN") = CType(dtgCart.Items.Item(I).FindControl("txtLN"), TextBox).Text
    '                                row.Item("SerialID") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("lblSerialID"), Label).Text
    '                                row.Item("UniqNum") = lblUniqNum.Text
    '                                'If CType(dtgCart.Items.Item(I).Cells(12).FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
    '                                If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
    '                                    row.Item("TaxFlag") = " "
    '                                    'ElseIf CType(dtgCart.Items.Item(I).Cells(12).FindControl("chbTaxFlag"), CheckBox).Checked = True Then
    '                                ElseIf CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("chbTaxFlag"), CheckBox).Checked = True Then
    '                                    row.Item("TaxFlag") = "Y"
    '                                Else
    '                                    row.Item("TaxFlag") = " "
    '                                End If
    '                            End If
    '                        Else
    '                            row.Delete()
    '                        End If
    '                    End If  ' NOT ROW IS NOTHING
    '                End If
    '            Next
    '        End If
    '    End If

    '    ' If we don't commit the changes, trying to reference a row that was deleted will cause an error.
    '    ' Commit/accept the changes so the deleted rows are permanently gone.
    '    dstcart1.AcceptChanges()

    '    dtgCart.DataSource = dstcart1
    '    dtgCart.DataBind()
    '    '''''''''''''' zzzzzzzzzzz  this is where the session("cart") is loaded
    '    Session("Cart") = dstcart1
    '    If dtgCart.Items.Count = 0 Then
    '        txtOrderTot.Text = "0.00"
    '    End If

    'End Sub

    Private Sub updateCartDatatable()
        Dim bolDel As Boolean = False
        Dim I As Integer
        Dim dstcart1 As New DataTable
        dstcart1 = Session("Cart")
        m_strLastShopGridUniqNum = -1 '*******************************

        ' New columns Priority,Due Date,Type,Notes

        If dstcart1 Is Nothing Then
            Exit Sub
        End If
        'lblQuickItem.Text = ""
        If Not rgCart Is Nothing Then
            If rgCart.Items.Count > 0 Then
                For I = 0 To rgCart.Items.Count - 1

                    Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
                    Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)

                    Dim lblItemID As Label = CType(rgCart.Items.Item(I).FindControl("lblItemID"), Label)
                    Dim hdfUniqNum As HiddenField = CType(nestedview.FindControl("hdfUniqNum"), HiddenField)
                    Dim ProductID As String = lblItemID.Text

                    Dim iRow As Integer
                    If FindCartDTRow(dstcart1, ProductID, hdfUniqNum.Value, iRow) Then
                        Dim row As DataRow = dstcart1.Rows(iRow)
                        If Not row Is Nothing Then
                            If Not Trim(CType(nestedview.FindControl("txtQTY"), TextBox).Text) = "" Then
                                If CType(nestedview.FindControl("chkDelete"), CheckBox).Checked = True Or
                                    System.Convert.ToDecimal(CType(nestedview.FindControl("txtQTY"), TextBox).Text) = 0 Then
                                    ' New code for Delete Attachemnts
                                    Try
                                        Dim FileName As String() = CType(row.Item("FilePath"), String()) 'was "filename" - yury
                                        Dim FileArr As String() = (From A In FileName Select Path.GetFileName(A)).ToArray()
                                        DeleteTempFile("", FileArr)
                                    Catch ex As Exception
                                    End Try
                                    'End Here
                                    row.Delete()
                                    bolDel = True
                                Else
                                    row.Item("Quantity") = System.Convert.ToDecimal(CType(nestedview.FindControl("txtQTY"), TextBox).Text)
                                    Dim txtItemChgCode As TextBox = CType(nestedview.FindControl("txtItemChgCode"), TextBox)
                                    If String.IsNullOrEmpty(txtItemChgCode.Text.Trim) Then
                                        row.Item("ItemChgCode") = txtChgCD.Text
                                    Else
                                        row.Item("ItemChgCode") = txtItemChgCode.Text
                                    End If

                                    Dim txtEmpChgCode As TextBox = CType(nestedview.FindControl("txtEmpChgCode"), TextBox)
                                    If String.IsNullOrEmpty(txtEmpChgCode.Text.Trim) Then
                                        row.Item("EmpChgCode") = dropEmpID.SelectedItem.Value
                                    Else
                                        row.Item("EmpChgCode") = txtEmpChgCode.Text
                                    End If

                                    row.Item("ItemDescription") = CType(GridItem.FindControl("txtItemDescc"), TextBox).Text

                                    row.Item("MachineRow") = CType(nestedview.FindControl("txtMachineRow"), TextBox).Text
                                    'row.Item("LPP") = CType(nestedview.FindControl("txtLPP"), TextBox).Text
                                    row.Item("PO") = CType(nestedview.FindControl("txtPO"), TextBox).Text
                                    row.Item("LN") = CType(nestedview.FindControl("txtLN"), TextBox).Text
                                    row.Item("SerialID") = CType(nestedview.FindControl("lblSerialID"), Label).Text
                                    'row.Item("UniqNum") = hdfUniqNum.Value
                                    row.Item("UniqNum") = GetShopGridNextUniqNum() '************************************

                                    If CType(nestedview.FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
                                        row.Item("TaxFlag") = " "
                                    ElseIf CType(nestedview.FindControl("chbTaxFlag"), CheckBox).Checked = True Then
                                        row.Item("TaxFlag") = "Y"
                                    Else
                                        row.Item("TaxFlag") = " "
                                    End If
                                    ' New code 
                                    Dim chkPriority As CheckBox = CType(nestedview.FindControl("chkPriority"), CheckBox)
                                    row.Item("UPriority") = " "
                                    If chkPriority.Checked Then
                                        row.Item("UPriority") = "R" '1
                                    End If

                                    Dim RDPDueDate As RadDatePicker = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker)
                                    row.Item("UDueDate") = " "
                                    If Not RDPDueDate.SelectedDate Is Nothing Then
                                        row.Item("UDueDate") = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker).SelectedDate
                                    Else
                                        If Not RadDatePickerReqByDate.SelectedDate Is Nothing Then
                                            row.Item("UDueDate") = RadDatePickerReqByDate.SelectedDate
                                            RDPDueDate.SelectedDate = RadDatePickerReqByDate.SelectedDate
                                        End If
                                    End If

                                    If dropType.Visible Then
                                        row.Item("UType") = UCase(dropType.SelectedValue)
                                    Else
                                        row.Item("UType") = " "
                                    End If

                                    row.Item("UNotes") = CType(nestedview.FindControl("txtNotes"), TextBox).Text
                                    Dim chkAddToCtlg As CheckBox = CType(nestedview.FindControl("chkAddToCtlg"), CheckBox)
                                    row.Item("AddToCtlgFlag") = " "
                                    If chkAddToCtlg.Checked Then
                                        row.Item("AddToCtlgFlag") = "1"
                                    End If
                                    '' Machine Number 
                                    Dim ddlMachineNo As DropDownList = CType(nestedview.FindControl("ddlMachineNo"), DropDownList)
                                    Dim txtMachineNo As TextBox = CType(nestedview.FindControl("txtMachineNo"), TextBox)
                                    row.Item("MachineNo") = " "
                                    If ddlMachineNo.Visible Then
                                        If Not ddlMachineNo.SelectedValue Is Nothing Then
                                            row.Item("MachineNo") = ddlMachineNo.SelectedValue
                                        End If
                                    End If
                                    If txtMachineNo.Visible Then
                                        If Not txtMachineNo.Text Is Nothing Then
                                            row.Item("MachineNo") = txtMachineNo.Text
                                        End If
                                    End If

                                End If
                            Else
                                row.Delete()
                            End If
                        End If
                    End If
                Next
            End If
        End If
        dstcart1.AcceptChanges()

        Session("Cart") = dstcart1
        'dtgCart.DataSource = dstcart1
        'dtgCart.DataBind()
        If bolDel = True Then
            Response.Redirect("shoppingcart.aspx", False)
        Else
            rgCart.DataSource = dstcart1
            rgCart.DataBind()
            If rgCart.Items.Count = 0 Then
                txtOrderTot.Text = "0.00"
            End If

        End If

        'Yury Ticket 80026 Persistent Cart changes !!!!!!!!!!!!!!!!!!
        WriteToUserCart(Session("USERID"), Session("BUSUNIT"))
        '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    End Sub

    'Private Sub UpdateFavsmenu(ByVal strFavItemid)

    '    Dim I As Integer
    '    Dim Y As Integer
    '    Dim strCustomeritemid
    '    Dim bolItems As Boolean
    '    Dim arrNSTKItem As New ArrayList(5)
    '    arrNSTKItem = Session("NSTKINFO")

    '    Dim objFavorites As New clsFavorites(strBU, strAgent, txtFavItemIDHide.Text, arrNSTKItem(2), arrNSTKItem(3))

    '    Dim strNonCat As String
    '    If Convert.ToString(strFavItemid).Length > 5 Then
    '        strNonCat = Convert.ToString(strFavItemid).Substring(0, 6)
    '    Else
    '        strNonCat = Convert.ToString(strFavItemid)
    '    End If
    '    If strNonCat = "NONCAT" Then
    '        strCustomeritemid = arrNSTKItem(2) & "-" & arrNSTKItem(3)
    '    Else
    '        Dim objcplusitem As New clsCplusItemWProdview(txtFavItemIDHide.Text, Session("CplusDB"))
    '        strCustomeritemid = objcplusitem.customerItemID
    '    End If

    '    If Trim(objFavorites.FavsCategory) = "" Then
    '        Exit Sub
    '    End If
    '    Dim newItem As New Telerik.Web.UI.RadMenuItem
    '    Dim topItem As Telerik.Web.UI.RadMenuItem
    '    Dim topSubItem As Telerik.Web.UI.RadMenuItem

    '    topItem = FavsMenu.FindItemById("Favorites")
    '    If IsNothing(topItem) Then
    '        FavsMenu = buildfavs(FavsMenu)
    '        buildfavsTelerik(FavsMenu)
    '        Exit Sub
    '    End If

    '    newItem.Text = objFavorites.FavsCategory
    '    newItem.ID = objFavorites.FavsCategory
    '     first remove the item - it will be added back below
    '    topSubItem = FavsMenu.FindItemById(strCustomeritemid)
    '    If Not IsNothing(topSubItem) Then
    '        topSubItem.ID = "Delete"
    '        topSubItem.Enabled = False
    '        topSubItem.Visible = False

    '    End If

    '    topItem = FavsMenu.FindItemById(objFavorites.FavsCategory)
    '    setitemlook()
    '    If IsNothing(topItem) Then
    '        newItem.Look.RightIconUrl = "arrow_left_sdi.gif"
    '        FavsMenu.Items(0).Items.Add(newItem)
    '    End If

    '    topItem = FavsMenu.FindItemById(objFavorites.FavsCategory)
    '    If Not IsNothing(topItem) Then
    '        Dim newsubItem As New Telerik.Web.UI.RadMenuItem
    '        newsubItem.Text = strCustomeritemid & " - " & objFavorites.FavsDescription
    '        If strNonCat = "NONCAT" Then
    '            newsubItem.Value = strFavItemid
    '        Else
    '            newsubItem.Value = strCustomeritemid
    '        End If

    '        newsubItem.ID = strCustomeritemid
    '        newsubItem.AutoPostBackOnSelect = True

    '        topItem.Items.Add(newsubItem)
    '    End If

    '    if there are no items at the item level, don't show the category

    '        For I = 0 To FavsMenu.Items(0).Items.Count - 1
    '            bolItems = False
    '            For Y = 0 To FavsMenu.Items(0).Items(I).Items.Count - 1
    '                If Not FavsMenu.Items(0).Items(I).Items(Y).ID = "Delete" Then
    '                    bolItems = True
    '                End If
    '            Next
    '            If bolItems = False Then
    '                FavsMenu.Items(0).Items(I).Enabled = False
    '                FavsMenu.Items(0).Items(I).Visible = False
    '            End If
    '        Next

    'End Sub

    Private Sub UpdateFavsmenu(ByVal strFavItemid)

        Dim I As Integer
        Dim Y As Integer
        Dim strCustomeritemid
        Dim bolItems As Boolean
        Dim arrNSTKItem As New ArrayList(5)
        arrNSTKItem = Session("NSTKINFO")



        Dim strNonCat As String
        If Convert.ToString(strFavItemid).Length > 5 Then
            strNonCat = Convert.ToString(strFavItemid).Substring(0, 6)
        Else
            strNonCat = Convert.ToString(strFavItemid)
        End If
        If strNonCat = "NONCAT" Then
            Dim objFavorites As New clsFavorites(strBU, strAgent, txtFavItemIDHide.Text, arrNSTKItem(11), arrNSTKItem(12))
            strCustomeritemid = arrNSTKItem(2) & "-" & arrNSTKItem(3)
            If Trim(objFavorites.FavsCategory) = "" Then
                Exit Sub
            End If
            Dim newItem As New Telerik.Web.UI.RadMenuItem
            Dim topItem As Telerik.Web.UI.RadMenuItem
            Dim topSubItem As Telerik.Web.UI.RadMenuItem

            topItem = FavsMenu.FindItemByValue("Favorites")
            If IsNothing(topItem) Then
                'FavsMenu = buildfavs(FavsMenu)
                FavsMenu = buildfavsTelerik(FavsMenu)
                Exit Sub
            End If

            newItem.Text = objFavorites.FavsCategory
            newItem.Value = objFavorites.FavsCategory
            ' first remove the item - it will be added back below
            topSubItem = FavsMenu.FindItemByValue(strCustomeritemid)
            If Not IsNothing(topSubItem) Then
                topSubItem.Value = "Delete"
                topSubItem.Enabled = False
                topSubItem.Visible = False

            End If

            topItem = FavsMenu.FindItemByValue(objFavorites.FavsCategory)
            'setitemlook()
            If IsNothing(topItem) Then
                'newItem.Look.RightIconUrl = "arrow_left_sdi.gif"
                FavsMenu.Items(0).Items.Add(newItem)
            End If

            topItem = FavsMenu.FindItemByValue(objFavorites.FavsCategory)
            If Not IsNothing(topItem) Then
                Dim newsubItem As New Telerik.Web.UI.RadMenuItem
                newsubItem.Text = strCustomeritemid & " - " & objFavorites.FavsDescription
                If strNonCat = "NONCAT" Then
                    newsubItem.Value = strFavItemid
                Else
                    newsubItem.Value = strCustomeritemid
                End If

                'newsubItem.Value = strCustomeritemid
                newsubItem.PostBack = True

                topItem.Items.Add(newsubItem)
            End If

            'if there are no items at the item level, don't show the category

            For I = 0 To FavsMenu.Items(0).Items.Count - 1
                bolItems = False
                For Y = 0 To FavsMenu.Items(0).Items(I).Items.Count - 1
                    If Not FavsMenu.Items(0).Items(I).Items(Y).ID = "Delete" Then
                        bolItems = True
                    End If
                Next
                If bolItems = False Then
                    FavsMenu.Items(0).Items(I).Enabled = False
                    FavsMenu.Items(0).Items(I).Visible = False
                End If
            Next
        Else
            Dim objFavorites As New clsFavorites(strBU, strAgent, arrNSTKItem(0), arrNSTKItem(2), arrNSTKItem(3))
            ''Dim objcplusitem As New clsCplusItemWProdview(txtFavItemIDHide.Text, Session("CplusDB"))
            Dim objReader As OleDbDataReader = GetUnilogItem(arrNSTKItem(0))
            'If objReader.Read() Then
            '    strCustomeritemid = objReader.Item("Customer_Part_Number_WO_Prefix")
            'End If
            Try
                If Not objReader Is Nothing Then
                    If objReader.Read() Then
                        strCustomeritemid = objReader.Item("Customer_Part_Number_WO_Prefix")
                    Else
                        strCustomeritemid = " "
                    End If
                Else
                    strCustomeritemid = " "
                End If
                objReader.Close()
            Catch exRD As Exception
                Try
                    objReader.Close()
                Catch ex As Exception

                End Try
                strCustomeritemid = " "
            End Try
            If Trim(objFavorites.FavsCategory) = "" Then
                Exit Sub
            End If
            Dim newItem As New Telerik.Web.UI.RadMenuItem
            Dim topItem As Telerik.Web.UI.RadMenuItem
            Dim topSubItem As Telerik.Web.UI.RadMenuItem

            topItem = FavsMenu.FindItemByValue("Favorites")
            If IsNothing(topItem) Then
                'FavsMenu = buildfavs(FavsMenu)
                FavsMenu = buildfavsTelerik(FavsMenu)
                Exit Sub
            End If

            newItem.Text = objFavorites.FavsCategory
            newItem.Value = objFavorites.FavsCategory
            ' first remove the item - it will be added back below
            topSubItem = FavsMenu.FindItemByValue(strCustomeritemid)
            If Not IsNothing(topSubItem) Then
                topSubItem.Value = "Delete"
                topSubItem.Enabled = False
                topSubItem.Visible = False

            End If

            topItem = FavsMenu.FindItemByValue(objFavorites.FavsCategory)
            'setitemlook()
            If IsNothing(topItem) Then
                'newItem.Look.RightIconUrl = "arrow_left_sdi.gif"
                FavsMenu.Items(0).Items.Add(newItem)
            End If

            topItem = FavsMenu.FindItemByValue(objFavorites.FavsCategory)
            If Not IsNothing(topItem) Then
                Dim newsubItem As New Telerik.Web.UI.RadMenuItem
                newsubItem.Text = arrNSTKItem(0) & " - " & objFavorites.FavsDescription
                If strNonCat = "NONCAT" Then
                    newsubItem.Value = strFavItemid
                Else
                    newsubItem.Value = arrNSTKItem(0)
                End If

                'newsubItem.Value = strCustomeritemid
                newsubItem.PostBack = True

                topItem.Items.Add(newsubItem)
            End If

            'if there are no items at the item level, don't show the category

            For I = 0 To FavsMenu.Items(0).Items.Count - 1
                bolItems = False
                For Y = 0 To FavsMenu.Items(0).Items(I).Items.Count - 1
                    If Not FavsMenu.Items(0).Items(I).Items(Y).ID = "Delete" Then
                        bolItems = True
                    End If
                Next
                If bolItems = False Then
                    FavsMenu.Items(0).Items(I).Enabled = False
                    FavsMenu.Items(0).Items(I).Visible = False
                End If
            Next
        End If
        Response.Redirect(HttpContext.Current.Request.Url.ToString(), True)

    End Sub

    Private Sub AuditForExide(Optional ByVal bIsStart As Boolean = False)
        'audit for exide only to try to determine why shopping cart items are disappearing '''''''''''''''''''''''''''''
        'storing...
        '# in user cart
        '# in session variable 
        '# in ps_isa_ord_intf_ln table
        Dim StrQuery As String = ""
        Dim sExideList As String = "I0501~I0502~I0503~I0506~I0509~I0510~I0511~I0512~I0513~I0518~I0519~I0520~I0523"
        Dim bAuditExide As Boolean = (sExideList.IndexOf(Session("BUSUNIT").Trim.ToUpper) > -1)
        Dim strErrSource As String = ""
        If bAuditExide Then
            Dim m_logFilePath As String = ""
            Dim m_logLevel As System.Diagnostics.TraceLevel = Diagnostics.TraceLevel.Off
            Dim sPath As String = ""
            If bIsStart Then
                Try
                    sPath = System.Web.Configuration.WebConfigurationManager.AppSettings(name:="NotificationFilePath").Trim

                    While (sPath.LastIndexOf("\"c) = (sPath.Length - 1))
                        sPath = sPath.TrimEnd("\"c)
                    End While
                    m_logFilePath = sPath & "\" & Now.Year.ToString("0000") & Now.Month.ToString("00") & Now.Day.ToString("00") & "ExideMissingItemAudit.log"
                    m_logLevel = Diagnostics.TraceLevel.Verbose

                Catch ex As Exception

                End Try
                m_logger = New ApplicationLogger(m_logFilePath, m_logLevel)

            End If

            StrQuery = "SELECT COUNT(1) FROM ps_isa_user_cart WHERE ISA_EMPLOYEE_ID ='" & Session("USERID") & "' AND BUSINESS_UNIT = '" & Session("BUSUNIT") & "'"
            Dim strExideAuditCartCnt As String = ORDBData.GetScalar(StrQuery, False)

            Dim strExideAuditTableCnt As String = ""
            Try
                StrQuery = "SELECT COUNT(1) FROM ps_isa_ord_intf_ln WHERE ORDER_NO ='" & ShoppingCartcls.OrderNo & "' AND BUSINESS_UNIT_OM = '" & Session("BUSUNIT") & "'"
                strExideAuditTableCnt = ORDBData.GetScalar(StrQuery, False)

            Catch ex As Exception
                strExideAuditTableCnt = ""
            End Try

            Dim dstExideUserCart As New DataTable
            dstExideUserCart = Session("Cart")

            Dim strExideMissingItem As String = ""

            strExideMissingItem = "USERID: " & Session("USERID") & ", "
            strExideMissingItem += "BUSUNIT: " & Session("BUSUNIT") & ", "
            If Not bIsStart Then
                strExideMissingItem += "ORDER NO: " & ShoppingCartcls.OrderNo & ", "
            End If
            strExideMissingItem += "USERCART Count: " & strExideAuditCartCnt & ", "
            If Not bIsStart Then
                strExideMissingItem += "SESSIONCART Count: " & Convert.ToString(dstExideUserCart.Rows.Count) & ", "
                If Trim(strExideAuditTableCnt) <> "" Then
                    strExideMissingItem += "INTF_LN Count: " & strExideAuditTableCnt

                End If

            Else
                strExideMissingItem += "SESSIONCART Count: " & Convert.ToString(dstExideUserCart.Rows.Count)
            End If

            m_logger.WriteInformationLog(strExideMissingItem)

            strErrSource = "ShoppingCart > ButtonClickEvent > AuditForExide"
            If bIsStart = True Then
                strErrSource += " > First Call"
            End If
            If ExideErrorHandling(Session("USERID"), Session("BUSUNIT"), strErrSource) = True Then
                Response.Redirect(Session("DEFAULTPAGE").ToString() & "?ShopCartProblem=YES")
            End If

        End If

    End Sub

    Private Sub UpdateNSTKDB(ByVal strNSTKreqid As String, ByVal strHdrStatus As String,
            Optional ByVal strOrigin1 As String = "", Optional ByVal bIsPricedProcess As Boolean = False)

        '' For the credit card process navigate the users to payment summary panel, where we have updateNSTK method to insert the order details.
        '' Added  below control values to the session variable for the header info

        If Session("CREDITCARDPROCSite") = "Y" And Session("SCbIsWithPricedItems") = True Then

            Dim strCartLineNotes As String = ""
            Dim strmachinenum1 As String = ""
            Dim strshipto1 As String = " "
            Dim strpriority As String = "N"
            Dim strtype1 As String = " "
            Dim strEmpId1 As String = " "

            If Not String.IsNullOrEmpty(txtNotes.Text.Trim) Then
                strCartLineNotes = txtNotes.Text.Trim()
            End If

            txtCustRef.Text = Replace(txtCustRef.Text, "'", "")

            If txtMachineNum.Visible = True Then
                strmachinenum1 = txtMachineNum.Text
            Else
                strmachinenum1 = cmbMachine.Text
            End If

            If dropShipto.Visible = True Then
                If dropShipto.SelectedIndex > 0 Then
                    strshipto1 = dropShipto.SelectedValue
                End If
            Else
                If Not Trim(txtShipTo.Text) = "" Then
                    strshipto1 = txtShipTo.Text
                End If
            End If

            If chbPriority.Checked() Then
                strpriority = "Y"
            End If

            ' for user type 
            If dropType.Visible Then
                strtype1 = UCase(dropType.SelectedValue)
            Else
                strtype1 = " "
            End If

            If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
                strEmpId1 = " "
            Else
                If Trim(dropEmpID.SelectedItem.Text) <> "" Then
                    strEmpId1 = Trim(dropEmpID.SelectedItem.Value)
                Else
                    strEmpId1 = " "
                End If
            End If


            '' Shopping cart header session values that not stored in cart table
            Session("SCCustRef") = txtCustRef.Text
            Session("SCCartLineNotes") = strCartLineNotes
            Session("SCMachineNum") = strmachinenum1
            Session("SCWorkOrder") = txtWorkOrder.Text
            Session("SCShipTo") = strshipto1
            Session("SCChangeLineNumsAccpt") = hdfChangeLineNumsAccpt.Value
            Session("SCPriority") = strpriority
            Session("SCUserType") = strtype1
            Session("rgCart") = rgCart
            'Session("SCbIsLyonsProcessing") = bIsLyonsProcessing '' Added in buttonclick
            'Session("SCbIsWithPricedItems") = bIsWithPricedItems '' Added in buttonclick
            Session("SCEmpID") = strEmpId1
            Session("SCChgCD") = txtChgCD.Text
            Session("SCReqByDate") = RadDatePickerReqByDate.SelectedDate

            ''UpdateNSTKDB Parameter session variables 
            Session("SCReqID") = strNSTKreqid
            Session("SCHdrStatus") = strHdrStatus
            Session("SCOrig") = strOrigin1
            Session("SCPricedProcess") = bIsPricedProcess


            Session("CONFTARGET") = "ShoppingCartPayment.aspx"


            ''Response.Redirect("ShoppingCartPayment.aspx")

            ''Response.Redirect("ShoppingCartPayment.aspx", False)
            ''Server.Transfer("ShoppingCartPayment.aspx")
            ''Context.ApplicationInstance.CompleteRequest()

        Else

            ''Regular process
            Dim I As Integer
            Dim X As Integer
            Dim Y As Integer
            Dim bolCatLPP As Boolean = False
            Dim strsql As String
            Dim strsqlPart As String
            Dim strsqlINTFC As String
            Dim strsqlLPP As String
            Dim dstNonCatAttrib As DataTable

            Dim strCplusItemid As String = " "
            Dim decQty As Decimal = 0
            Dim decPrice As Decimal = 0.0
            Dim decPOAmount As Decimal = 0
            Dim strDescription As String = " "
            Dim strShipto As String = " "
            Dim strsupplierID As String = " "
            Dim strsuppliername As String = " "
            Dim strsupplierPartID As String = " "
            Dim strSupplierAuxPartID As String = " "
            Dim strVendorLoc As String = " "
            Dim strUOM As String = " "
            Dim strRFQInd As String = " "
            Dim strMfgName As String = " "
            Dim strMfgID As String = " "
            Dim strMfgPartNum As String = " "
            Dim strMfgNameLPP As String = " "
            Dim strMfgIDLPP As String = " "
            Dim strMfgPartNumLPP As String = " "
            Dim strNotes As String = " "
            Dim strLPPNotes As String = " "
            Dim strEmpID As String = " "
            Dim strLPP As String = " "
            Dim strChbTaxFlag As String = " "
            Dim strTaxFlagText As String = " "
            Dim decLPP As Decimal = 0.0
            Dim strCustPO As String = " "
            Dim intCustLN As Integer = 0
            Dim strChgCD As String = " "
            Dim arrChgCD() As String
            Dim strChgCD_GL As String = " "
            Dim strChgCD_CC As String = " "
            Dim strChgCD_PROJ As String = " "
            Dim strSerialID As String = " "
            Dim strPunchout As String = " " 'YA 20190425

            '*************************************** Tax exempt reminder **************************************
            'code using strchgcd1 references the changes made to the tax exempt issue that may go back in 
            ' when it is needed again.  Remember to make sure the drop down and replace for spaces works
            ' it screwed up SDM and DRS

            Dim strItmChgCD As String = " "
            Dim strWorkOrder As String = " "
            Dim strMachineNum As String = " "
            Dim strOROItemID As String = " "
            Dim dteNow As Date
            Dim rowsaffected As Integer
            Dim strValues As String
            Dim strFields As String
            Dim strAttribLabel As String
            Dim strAttribValue As String
            Dim strLineStatus As String
            Dim intIdentifier As String
            Dim StrMachineNumber As String = ""
            Dim strUNSPSC As String = ""

            Dim bIsLyonsProcessing As Boolean = False
            If Insiteonline.VoucherSharedFunctions.VoucherClass.IsLyons(strBU) Then
                bIsLyonsProcessing = True
            End If

            Dim bIsPunchoutOrder As Boolean = False
            For I = 0 To rgCart.Items.Count - 1
                Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
                Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
                'code to get SupplierID (is Punchout Order?) - origin PCH
                Try
                    If Not IsDBNull(CType(nestedview.FindControl("hdfSupID"), HiddenField).Value) Then
                        strsupplierID = CType(nestedview.FindControl("hdfSupID"), HiddenField).Value
                        strsupplierID = Replace(Trim(strsupplierID), "'", "")
                        If Trim(strsupplierID) <> "" Then
                            If Trim(strsupplierID) <> "&nbsp;" Then
                                bIsPunchoutOrder = True
                            End If
                        End If
                    End If
                Catch ex As Exception
                    bIsPunchoutOrder = False
                End Try
            Next

            Dim sOrign As String = "IOL"
            'If bIsPunchoutOrder And bIsPricedProcess Then
            '    sOrign = "PCH"
            'End If

            'If sOrign <> "PCH" Then
            '    If Trim(strOrigin1) <> "" Then
            '        sOrign = "RFQ"
            '    End If
            'End If

            strLineStatus = "NEW"

            dteNow = Now().ToString("d")

            If strAgent.Length > 10 Then
                strAgent = strAgent.Substring(0, 10)
            End If

            ' New Code for PeopleSoftVersion 9.2 

            ' For User Type 
            Dim StrType As String = " "

            If dropType.Visible Then
                StrType = UCase(dropType.SelectedValue)
            Else
                StrType = " "
            End If

            If Trim(StrType) <> "" Then
                If Len(StrType) > 2 Then
                    StrType = Microsoft.VisualBasic.Left(StrType, 2)
                End If
            End If

            ShoppingCartcls.OrderType = StrType
            ShoppingCartcls.BU_OM = strBU
            ShoppingCartcls.OrderNo = strNSTKreqid.ToUpper
            ShoppingCartcls.CustID = Session("CUSTID").ToString()
            ShoppingCartcls.Origin = sOrign
            ShoppingCartcls.SourceID = " "
            ShoppingCartcls.OrderStatus = "OPN"
            ShoppingCartcls.ReqID = " "

            rowsaffected = ShoppingCartcls.UpdateToHeaderTable()

            If rowsaffected = 0 Then
                lblDBError.Text = "Error Updating INTFC header"
                Exit Sub
            End If

            ' New Code for Update Header Notes field 
            ShoppingCartcls.LINENOTES = " "
            If Not String.IsNullOrEmpty(txtNotes.Text.Trim) Then
                ShoppingCartcls.LINENOTES = txtNotes.Text.Trim()
            End If
            ShoppingCartcls.LineNBR = 0
            ShoppingCartcls.MSGTYPE = "GEN"

            rowsaffected = ShoppingCartcls.UpdateOrderNotes()
            If rowsaffected = 0 Then
                lblDBError.Text = "Error Updating INTFC header Notes"
                Exit Sub
            End If

            Dim intParentID As String = strNSTKreqid

            Dim strSrchFileName As String = ""
            Dim strSrchFileContents As String = ""

            'get path to the App Temp dir
            Dim writerLnsMagn As StreamWriter
            Dim strPathForLM As String = ""
            'create Flat file to send to Lyons Magnus (I0531, I0532 for now) - VR 08/16/2018
            If bIsLyonsProcessing Then
                strBU = UCase(Trim(strBU))
                Select Case strBU
                    Case "I0531"
                        strSrchFileName = "LMW" & "_" & Session("USERID") & "_" & strNSTKreqid.ToUpper & "_" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

                    Case "I0532"
                        strSrchFileName = "LME" & "_" & Session("USERID") & "_" & strNSTKreqid.ToUpper & "_" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

                End Select
                'ConfigurationManager.AppSettings("LyonsMagnusFtpOutbound") & "<File name>"
                strPathForLM = ConfigurationManager.AppSettings("LyonsMagnusFtpOutbound") & strSrchFileName
                writerLnsMagn = New StreamWriter(strPathForLM)
                'add headers
                writerLnsMagn.WriteLine("LOCATION" & vbTab & "QUOTEID" & vbTab & "LINENUM" & vbTab & "PRODUCT_ID" & vbTab & "DESCRIPTION" & vbTab & "QUANTITY" & vbTab & "UOM" & vbTab & "ITEM_PRICE" & vbTab & "CURRENCY" & vbTab & "VENDOR" & vbTab & "VENDORPARTNUM" & vbTab & "MANFID" & vbTab & "MANFPARTNUM" & vbTab & "UNSPSC" & vbTab & "DUEDATE" & vbTab & "GL" & vbTab & "CC" & vbTab & "PROJ" & vbTab & "EMPLOYEE" & vbTab & "LINENOTES")

                strSrchFileContents = "LOCATION" & "|" & "QUOTEID" & "|" & "LINENUM" & "|" & "PRODUCT_ID" & "|" & "DESCRIPTION" & "|" & "QUANTITY" & "|" & "UOM" & "|" & "ITEM_PRICE" & "|" & "CURRENCY" & "|" & "VENDOR" & "|" & "VENDORPARTNUM" & "|" & "MANFID" & "|" & "MANFPARTNUM" & "|" & "UNSPSC" & "|" & "DUEDATE" & "|" & "GL" & "|" & "CC" & "|" & "PROJ" & "|" & "EMPLOYEE" & "|" & "LINENOTES" & vbCrLf

            End If  '  If UCase(Trim(strBU)) = "I0531" Or UCase(Trim(strBU)) = "I0532" Then

            Dim bIsAnyLMLinesWithPrice As Boolean = False
            Dim StrNote As String = " "
            For I = 0 To rgCart.Items.Count - 1
                strCplusItemid = (CType(rgCart.Items.Item(I).FindControl("lblItemID"), Label).Text).ToString
                If Trim(strCplusItemid) = "" Then strCplusItemid = " "
                Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
                Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)

                strPunchout = CType(nestedview.FindControl("hdfPunchout"), HiddenField).Value

                ' Get Product Quatity 
                decQty = System.Convert.ToDecimal(CType(nestedview.FindControl("txtQTY"), TextBox).Text)
                decPrice = 0

                ' Get Product Price
                Try
                    decPrice = CDec(CType(nestedview.FindControl("Price"), Label).Text)
                Catch ex As Exception
                    decPrice = 0
                End Try

                ' Get Item Description 
                strDescription = CType(GridItem.FindControl("txtItemDescc"), TextBox).Text
                'strDescription = rgCart.Items.Item(I).Cells(dtgCartColumns.itemDescription).Text

                decPOAmount = 0
                strsupplierID = " "
                strsuppliername = " "
                strsupplierPartID = " "
                strSupplierAuxPartID = " "
                strUOM = " "
                strRFQInd = " "
                strMfgName = " "
                strMfgPartNum = " "
                strMfgID = " "
                strMfgNameLPP = " "
                strMfgPartNumLPP = " "
                strMfgIDLPP = " "
                StrNote = " "

                ' Getting Manufacturer 
                strMfgNameLPP = rgCart.Items.Item(I).Cells(dtgCartColumns.mfg).Text

                ' Getting Manuf Part No 
                strMfgPartNumLPP = rgCart.Items.Item(I).Cells(dtgCartColumns.mfgPartNo).Text

                Dim strngItemID As String = (CType(rgCart.Items.Item(I).Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label).Text).ToString
                strMfgName = rgCart.Items.Item(I).Cells(dtgCartColumns.mfg).Text
                strMfgPartNum = rgCart.Items.Item(I).Cells(dtgCartColumns.mfgPartNo).Text

                ' Getting UOM 
                strUOM = rgCart.Items.Item(I).Cells(dtgCartColumns.itemUOM).Text

                ' Getting Supplier ID And Part Number

                If Not IsDBNull(CType(nestedview.FindControl("hdfSupID"), HiddenField).Value) Then
                    strsupplierID = CType(nestedview.FindControl("hdfSupID"), HiddenField).Value
                End If

                ' Getting UNSPSC 
                Try
                    If Not IsDBNull(CType(nestedview.FindControl("hdfItemUNSPSC"), HiddenField).Value) Then
                        strUNSPSC = CType(nestedview.FindControl("hdfItemUNSPSC"), HiddenField).Value
                        If Trim(strUNSPSC) = "" Then
                            strUNSPSC = " "
                        End If
                    Else
                        strUNSPSC = " "
                    End If

                Catch ex As Exception
                    strUNSPSC = " "
                End Try

                If Not IsDBNull(CType(nestedview.FindControl("hdfSupPartNum"), HiddenField).Value) Then
                    strsupplierPartID = CType(nestedview.FindControl("hdfSupPartNum"), HiddenField).Value
                End If

                ' Getting the SupplierPartAuxiliaryID

                If Not IsDBNull(CType(nestedview.FindControl("hdfSupPartNumAux"), HiddenField).Value) Then
                    strSupplierAuxPartID = CType(nestedview.FindControl("hdfSupPartNumAux"), HiddenField).Value
                End If

                ' Getting the Price PO

                If Not IsDBNull(CType(nestedview.FindControl("hdfPricePO"), HiddenField).Value) Then
                    If CType(nestedview.FindControl("hdfPricePO"), HiddenField).Value = "&nbsp;" Or String.IsNullOrEmpty(CType(nestedview.FindControl("hdfPricePO"), HiddenField).Value.Trim()) Then
                        decPOAmount = 0.0
                    Else
                        decPOAmount = Convert.ToDecimal(CType(nestedview.FindControl("hdfPricePO"), HiddenField).Value)
                    End If
                End If

                If strsupplierID = "&nbsp;" Or strsupplierID = "null" Or Trim(strsupplierID) = "" Then
                    strsupplierID = " "
                End If
                If strsuppliername = "&nbsp;" Or strsuppliername = "null" Or Trim(strsuppliername) = "" Then
                    strsuppliername = " "
                End If
                If strsupplierPartID = "&nbsp;" Or strsupplierPartID = "null" Or Trim(strsupplierPartID) = "" Then
                    strsupplierPartID = " "
                End If
                If strSupplierAuxPartID = "&nbsp;" Or strSupplierAuxPartID = "null" Or Trim(strSupplierAuxPartID) = "" Then
                    strSupplierAuxPartID = " "
                End If
                If strUOM = "&nbsp;" Or strUOM = "null" Or Trim(strUOM) = "" Then
                    strUOM = " "
                End If
                If strMfgName = "&nbsp;" Or strMfgName = "null" Or Trim(strMfgName) = "" Then
                    strMfgName = " "
                End If
                If strMfgPartNum = "&nbsp;" Or strMfgPartNum = "null" Or Trim(strMfgPartNum) = "" Then
                    strMfgPartNum = " "
                Else

                    If strMfgPartNum.Length > 50 Then
                        strMfgPartNum = strMfgPartNum.Substring(0, 50)
                    End If

                End If

                If CType(nestedview.FindControl("lblRFQ"), Label).Text = "Yes" Then
                    strRFQInd = "Y"
                Else
                    strRFQInd = "N"
                End If

                '  Getting the ITEM ID

                If String.IsNullOrEmpty(CType(nestedview.FindControl("hdfItemID"), HiddenField).Value) Or CType(nestedview.FindControl("hdfItemID"), HiddenField).Value = " " Then
                    strOROItemID = " "
                ElseIf CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.Length < 3 Then
                    If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                        strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
                    ElseIf CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.Substring(0, 3) = Session("SITEPREFIX") Then
                        strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
                    Else
                        strOROItemID = Session("SITEPREFIX") & CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
                    End If
                ElseIf CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.ToUpper.Substring(0, 3) = "SDI" Then
                    strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.ToUpper
                ElseIf Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                    strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.ToUpper
                ElseIf CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.Substring(0, 3) = Session("SITEPREFIX") Then
                    strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
                Else
                    strOROItemID = Session("SITEPREFIX") & CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.ToUpper
                End If

                strNotes = txtNotes.Text.ToUpper
                If strNotes = "" Then
                    strNotes = " "
                End If

                Try
                    If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
                        strEmpID = " "
                    Else
                        If Trim(dropEmpID.SelectedItem.Text) <> "" Then
                            strEmpID = Trim(dropEmpID.SelectedItem.Value)
                        Else
                            strEmpID = " "
                        End If
                    End If
                    ' Getting the Employee Name - line value
                    If Trim(CType(nestedview.FindControl("txtempChgCode"), TextBox).Text) <> "" Then  '  Not CType(nestedview.FindControl("txtempChgCode"), TextBox).Text = " " And Not CType(nestedview.FindControl("txtempChgCode"), TextBox).Text = "" Then
                        strEmpID = Trim(CType(nestedview.FindControl("txtempChgCode"), TextBox).Text)
                    End If
                Catch ex As Exception
                    strEmpID = " "
                End Try

                If strEmpID = "" Then
                    strEmpID = " "
                End If

                strChgCD = txtChgCD.Text

                If Not strOROItemID = " " Then
                    strItmChgCD = getItemChgCD(strOROItemID, strBU)
                    If Not strItmChgCD = "" Then
                        If strChgCD = "" Then
                            strChgCD = strItmChgCD
                        Else
                            strChgCD = strItmChgCD & strChgCdSep & strChgCD
                        End If
                    End If
                End If

                ' Getting the charge code
                If Not CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text = " " And
                    Not CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text = "" Then
                    strChgCD = CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text
                End If
                If strChgCD = "" Then
                    strChgCD = " "
                End If

                strSerialID = ""
                If Not CType(nestedview.FindControl("lblSerialID"), Label).Text = " " And
                    Not CType(nestedview.FindControl("lblSerialID"), Label).Text = "" Then
                    strSerialID = CType(nestedview.FindControl("lblSerialID"), Label).Text
                End If
                If strSerialID = "" Then
                    strSerialID = " "
                End If

                'strLPP = ""
                'If Not CType(nestedview.FindControl("txtLPP"), TextBox).Text = " " And _
                '    Not CType(nestedview.FindControl("txtLPP"), TextBox).Text = "" Then
                '    strLPP = CType(nestedview.FindControl("txtLPP"), TextBox).Text
                'End If

                If CType(nestedview.FindControl("chbTaxflag"), CheckBox).Checked = True Then
                    strChbTaxFlag = "Y"
                    strTaxFlagText = "EXEMPT_CERT"
                Else
                    strChbTaxFlag = " "
                    strTaxFlagText = " "
                End If
                If Trim(strLPP) = "" Then
                    strLPP = "0.00"
                End If
                decLPP = Convert.ToDecimal(strLPP)

                '''''''''''''''''''''''''''''Ticket 154676''''''''''''''''''''''''''
                Dim strExempt_Cert As String = strChbTaxFlag
                If Not Session("CARTTAXEXMPTFLAG") Is Nothing Then
                    If Trim(Session("CARTTAXEXMPTFLAG")) = "" Then
                        strExempt_Cert = getExemptStatus(strChgCD, strBU)
                    End If
                End If
                '''''''''''''''''''''''''''''''''''''''''''''''''''''

                If txtMachineNum.Visible = True Then
                    strMachineNum = txtMachineNum.Text
                Else
                    strMachineNum = cmbMachine.Text
                    If Not CType(nestedview.FindControl("txtMachineRow"), TextBox).Text = " " And
                        Not CType(nestedview.FindControl("txtMachineRow"), TextBox).Text = "" Then
                        strMachineNum = CType(nestedview.FindControl("txtMachineRow"), TextBox).Text
                    End If
                End If
                If Trim(strMachineNum) = "" Then
                    strMachineNum = " "
                End If

                'If Not CType(nestedview.FindControl("txtPO"), TextBox).Text = " " And _
                'Not CType(nestedview.FindControl("txtPO"), TextBox).Text = "" Then
                'strCustPO = CType(nestedview.FindControl("txtPO"), TextBox).Text
                'End If

                'Exide updates
                txtCustRef.Text = Replace(txtCustRef.Text, "'", "")
                If Trim(txtCustRef.Text) = "" Then
                    txtCustRef.Text = " "
                End If
                strCustPO = txtCustRef.Text


                If Not CType(nestedview.FindControl("txtLN"), TextBox).Text = " " And
                   Not CType(nestedview.FindControl("txtLN"), TextBox).Text = "" Then
                    intCustLN = CType(nestedview.FindControl("txtLN"), TextBox).Text
                End If
                ' -------------------------------------------------------------

                ' Retrieve the ID of the product
                Dim lblItemID As Label = CType(rgCart.Items.Item(I).FindControl("lblItemID"), Label)
                Dim lblUniqNum As HiddenField = CType(nestedview.FindControl("hdfUniqNum"), HiddenField)
                Dim ProductID As String = lblItemID.Text
                Dim iRow As Integer
                If FindCartDTRow(Session("Cart"), ProductID, lblUniqNum.Value, iRow) Then
                    Dim row As DataRow = CType(Session("Cart"), DataTable).Rows(iRow)
                    row.Item("ItemChgCode") = strChgCD
                    row.Item("EmpChgCode") = strEmpID
                    row.Item("MachineRow") = strMachineNum
                    row.Item("LPP") = strLPP
                    row.Item("TaxFlag") = strChbTaxFlag
                    row.Item("SerialID") = strSerialID
                End If
                ' -------------------------------------------------------------

                strWorkOrder = txtWorkOrder.Text
                If Trim(strWorkOrder) = "" Then
                    strWorkOrder = " "
                End If

                If (Not Trim(strMfgName) = "") And
                        (Trim(strMfgID) = "") Then
                    strMfgID = getMfgID(strMfgName)
                    If IsDBNull(strMfgID) Then
                        strMfgID = " "
                    ElseIf strMfgID = "" Then
                        strMfgID = " "
                    End If
                End If
                If Not strMfgNameLPP = " " And
                    strMfgIDLPP = " " Then
                    strMfgIDLPP = getMfgID(strMfgNameLPP)
                    If IsDBNull(strMfgIDLPP) Then
                        strMfgIDLPP = " "
                    ElseIf strMfgIDLPP = "" Then
                        strMfgIDLPP = " "
                    End If
                End If
                strShipto = " "
                If dropShipto.Visible = True Then
                    If dropShipto.SelectedIndex > 0 Then
                        strShipto = dropShipto.SelectedValue
                    End If
                Else
                    If Not Trim(txtShipTo.Text) = "" Then
                        strShipto = txtShipTo.Text
                    End If
                End If

                strsupplierID = Replace(strsupplierID, "'", "")
                If Not Trim(strsupplierID) = "" Then
                    strVendorLoc = getVendorLoc(strsupplierID, strShipto)
                End If
                strsupplierPartID = Replace(strsupplierPartID, "'", "")
                If Trim(strsupplierPartID) <> "" Then
                    strsupplierPartID = Trim(strsupplierPartID)
                    If strsupplierPartID.Length > 50 Then
                        strsupplierPartID = strsupplierPartID.Substring(0, 50)
                    End If
                Else
                    strsupplierPartID = " "
                End If

                strSupplierAuxPartID = Replace(strSupplierAuxPartID, "'", "")
                If Trim(strSupplierAuxPartID) <> "" Then
                    strSupplierAuxPartID = Trim(strSupplierAuxPartID)

                    'If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Then
                    If strSupplierAuxPartID.Length > 250 Then
                        strSupplierAuxPartID = strSupplierAuxPartID.Substring(0, 250)
                    End If
                    'Else
                    '    If strSupplierAuxPartID.Length > 60 Then
                    '        strSupplierAuxPartID = strSupplierAuxPartID.Substring(0, 60)
                    '    End If
                    'End If

                Else
                    strSupplierAuxPartID = " "
                End If

                strMfgID = Replace(strMfgID, "'", "")
                If Trim(strMfgID) = "" Then
                    strMfgID = " "
                Else
                    strMfgID = Trim(strMfgID)
                    If strMfgID.Length > 50 Then
                        strMfgID = strMfgID.Substring(0, 50)
                    End If
                End If
                strMfgName = Replace(strMfgName, "'", "")
                If Trim(strMfgName) = "" Then
                    strMfgName = " "
                Else
                    strMfgName = Trim(strMfgName)
                    If strMfgName.Length > 30 Then
                        strMfgName = strMfgName.Substring(0, 30)
                    End If
                End If

                strMfgNameLPP = Replace(strMfgNameLPP, "'", " ")
                strDescription = Replace(Left(strDescription, 254), "&", " and ")
                strDescription = Replace(strDescription, "<", " less than ")
                strDescription = Replace(strDescription, ">", " more than ")
                strDescription = Replace(Left(strDescription, 254), "'", "")
                strDescription = Replace(Left(strDescription, 254), "´", "")
                strDescription = Replace(Left(strDescription, 254), ";", "")
                strDescription = Regex.Replace(strDescription, "\s+", " ")
                strDescription = Left(strDescription, 254)
                strNotes = Replace(strNotes, "'", "")
                strNotes = Left(strNotes, 254)
                strMfgPartNum = Replace(strMfgPartNum, "'", "")
                strMfgPartNumLPP = Replace(strMfgPartNumLPP, "'", "")
                strWorkOrder = Replace(strWorkOrder, "'", "")
                strMachineNum = Replace(strMachineNum, "'", "")
                If Trim(strMfgIDLPP) = "" Then
                    If strMfgNameLPP.Length > 10 Then
                        strMfgIDLPP = strMfgNameLPP.Substring(0, 10)
                    Else
                        strMfgIDLPP = strMfgNameLPP
                    End If
                End If
                ' sometimes the 10 position has a ' and that blows (the program)!!!! pfd 05212009
                strMfgIDLPP = Replace(strMfgIDLPP, "'", " ")
                If Trim(strMfgIDLPP) = "" Then
                    strMfgIDLPP = " "
                End If
                strEmpID = GetEmpID(strBU, strEmpID)
                strEmpID = Replace(strEmpID, "'", " ")
                If Trim(strEmpID) = "" Then
                    strEmpID = " "
                Else
                    strEmpID = Trim(strEmpID)
                    If Len(strEmpID) > 10 Then
                        strEmpID = Microsoft.VisualBasic.Left(strEmpID, 10)
                    End If
                End If

                strsupplierPartID = Replace(strsupplierPartID, "'", "")
                strsupplierPartID = GetSupplierPartId24Char(strsupplierPartID)

                ' New code for Due Date 

                Dim DueDate As Nullable(Of Date)
                DueDate = RadDatePickerReqByDate.SelectedDate
                Try

                    DueDate = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker).SelectedDate
                    If IsDBNull(DueDate) Then
                        DueDate = RadDatePickerReqByDate.SelectedDate
                    End If
                    If DueDate Is Nothing Then
                        DueDate = RadDatePickerReqByDate.SelectedDate
                    End If

                Catch ex As Exception
                    DueDate = RadDatePickerReqByDate.SelectedDate
                End Try

                ShoppingCartcls.ItemID = strOROItemID
                If hdfChangeLineNumsAccpt.Value <> "X" Then
                    ShoppingCartcls.LineNBR = I + 1
                Else
                    ShoppingCartcls.LineNBR = (I + 1) * 10
                End If

                ' Attachment code 
                Dim dt As DataTable = CType(Session("cart"), DataTable)
                dt.Rows(I)("LineNo") = ShoppingCartcls.LineNBR
                dt.AcceptChanges()

                If Not IsDBNull(dt.Rows(I)("FilePath")) Then

                    Dim FilePath As String() = CType(dt.Rows(I)("FilePath"), String())
                    Dim StrFilePath As String = String.Empty
                    For Each f As String In FilePath
                        Dim Result As String = String.Empty
                        Dim FB() As Byte = File.ReadAllBytes(f)
                        Dim Line_NBR As Integer = I
                        Dim SCD_NBR As Integer = 1
                        Dim SiteBU As String = CType(Session("SITEBU"), String)
                        Dim Record As String = Convert.ToString(intParentID) + "-" + Convert.ToString(Line_NBR) + "-" + Convert.ToString(SCD_NBR) + "-" + SiteBU
                        Result = NONCatalogFileUpload(FB, f, Record)
                        StrFilePath = StrFilePath + "<P> <a href='" + Result + "'>" + Path.GetFileName(f) + "</a> </P>"
                    Next

                    ' Delete File from FileSystem
                    Try
                        Dim FileArr As String() = (From A In FilePath Select Path.GetFileName(A)).ToArray() 'yury
                        DeleteTempFile("", FileArr) 'yury
                        ''DeleteTempFile("", FilePath)

                    Catch ex As Exception
                    End Try

                    '  Adding After file uploaded in server. Storing the server file path for including the mail
                    dt.Rows(I)("BuyerFilePath") = StrFilePath
                End If
                dt.AcceptChanges()
                Session("cart") = dt
                strFields = ""
                strValues = ""

                Dim bolLast As Boolean = False

                'strChgCD  
                Dim currloc As Integer
                Dim TaxExempt As String
                Dim StringLength As Integer
                Dim tmpChar As String
                Dim strUnwanted As String = "EXEMP_CERT"
                StringLength = Len(strChgCD)
                For currloc = 1 To StringLength
                    If currloc + 10 <= StringLength Then
                        If Not IsDBNull(strChgCD.Substring(currloc, 10)) Then
                            If strChgCD.Substring(currloc, 10) = strUnwanted Then
                                TaxExempt = "Y"
                            End If
                        End If
                    End If
                Next

                ' New Code - for the new columns in the Shopping Cart - FEB 2017 - AVACORP, VR 
                ' For Priority 
                Dim Prioritychk As CheckBox = CType(nestedview.FindControl("chkPriority"), CheckBox)
                Dim StrPriority As String = " "

                If Prioritychk.Checked() Or chbPriority.Checked() Then
                    StrPriority = "R"
                Else
                    StrPriority = " "
                End If

                ' For User Notes 
                Dim TypeNotes As TextBox = CType(nestedview.FindControl("txtNotes"), TextBox)

                If Not String.IsNullOrEmpty(Trim(TypeNotes.Text)) Then
                    StrNote = Trim(TypeNotes.Text)
                    StrNote = Replace(StrNote, "'", "")
                    If Trim(StrNote) = "" Then
                        StrNote = " "
                    End If
                End If

                ' For Add To Catalog ?
                Dim chkAddToCtlg As CheckBox = CType(nestedview.FindControl("chkAddToCtlg"), CheckBox)
                Dim StrAddToCtlg As String = " "

                If chkAddToCtlg.Checked Then
                    StrAddToCtlg = "1"
                Else
                    StrAddToCtlg = " "
                End If

                If String.IsNullOrEmpty(strOROItemID) Then
                    strOROItemID = " "
                End If
                'If Trim(strsupplierID) = "0" Or String.IsNullOrEmpty(strsupplierID.Trim()) Then
                '    ShoppingCartcls.PULINEFIELD = " "
                'Else
                '    ShoppingCartcls.PULINEFIELD = "P"
                'End If

                ' ''''''''''''''''''''''''''''''''''''''''''''''
                'If strOROItemID.Trim <> "" Then ' strsupplierID = "SDICATALOG" Then
                '    ShoppingCartcls.PULINEFIELD = " "
                'ElseIf strsupplierID.Trim <> "" Then
                '    ShoppingCartcls.PULINEFIELD = "Z"
                '    If strPunchout.Trim = "PUNCHOUT" Then 'supplier id + punchout
                '        ShoppingCartcls.PULINEFIELD = "P"
                '    End If
                'ElseIf strsupplierID.Trim = "" And strOROItemID.Trim = "" And decPrice = 0 Then
                '    ShoppingCartcls.PULINEFIELD = "A"
                'End If
                ' ''''''''''''''''''''''''''''''''''''''''''''''
                '2019/05/01 proposal
                If strOROItemID.Trim <> "" Then ' strsupplierID = "SDICATALOG" Then
                    ShoppingCartcls.PULINEFIELD = "STK" 'isInventoryID function check exact inventory type (ORO, STK, etc)

                    'Ticket SDI-8013 new rules for Stock items
                    If strsupplierID.Trim = "0000" Or strsupplierID.Trim = "" Then
                        strsupplierID = " "
                        strsupplierPartID = " "
                    End If

                ElseIf strsupplierID.Trim <> "" Then
                    ShoppingCartcls.PULINEFIELD = "ZEUS"
                    If strPunchout.Trim = "PUNCHOUT" Then 'supplier id + punchout
                        ShoppingCartcls.PULINEFIELD = "PCH"
                    End If
                ElseIf strsupplierID.Trim = "" And strOROItemID.Trim = "" And decPrice = 0 Then
                    If Session("RFQSITE") = "Y" Then
                        ShoppingCartcls.PULINEFIELD = "RFQ"
                    Else
                        ShoppingCartcls.PULINEFIELD = "NSTK"
                    End If
                End If
                ' '''''''''''''''''''''''''''''''''''''''''''''' 
                'To get the C6 column value for buy again item
                Try
                    Dim strSqlString As String = "SELECT ISA_USER_DEFINED_4 FROM PS_ISA_USER_CART WHERE INV_ITEM_ID = '" + strCplusItemid + "' AND ISA_EMPLOYEE_ID = '" + Session("USERID") + "'"
                    Dim ds As DataSet = ORDBData.GetAdapter(strSqlString)
                    Dim C6_Column As String = ds.Tables(0).Rows(0).Item("ISA_USER_DEFINED_4")
                    If C6_Column <> " " Then
                        If C6_Column = "ZEUS" Or C6_Column = "PCH" Or C6_Column = "RFQ" Or C6_Column = "A" Or C6_Column = "P" Then
                            ShoppingCartcls.PULINEFIELD = "RFQ"
                        End If
                    End If
                Catch ex As Exception
                End Try

                ' Change requested by Donna and Mike - VR 02/02/2017
                If Trim(strCplusItemid) <> "" Then
                    If Len(strCplusItemid) > 5 Then
                        If Microsoft.VisualBasic.Left(UCase(Trim(strCplusItemid)), 6) = "NONCAT" Then
                            strCplusItemid = " "
                        End If
                    End If
                End If

                'To set the default vendor id and price for NSTK item
                If strOROItemID = " " And strsupplierID = " " And decPOAmount = 0 Then
                    strsupplierID = Session("ISACstVendor")
                    decPOAmount = Session("ISACstPrice")
                End If

                If strsupplierID Is Nothing Then
                    strsupplierID = " "
                End If
                If Trim(strsupplierID) = "" Then
                    strsupplierID = " "
                End If

                Dim cmbMachineNo As DropDownList = CType(nestedview.FindControl("ddlMachineNo"), DropDownList)
                Dim txtMachineNo As TextBox = CType(nestedview.FindControl("txtMachineNo"), TextBox)

                StrMachineNumber = " "

                If cmbMachineNo.Visible Then
                    Dim MachineNoSelected As String = cmbMachineNo.SelectedValue

                    If String.IsNullOrEmpty(MachineNoSelected) Then
                    Else
                        StrMachineNumber = MachineNoSelected
                    End If
                End If

                ' To set the part number for Non Catalog item 
                If txtMachineNo.Visible Then
                    StrMachineNumber = txtMachineNo.Text
                End If

                If Trim(StrMachineNumber) = "" Then
                    StrMachineNumber = " "
                End If
                If Trim(strSerialID) = "" Then
                    strSerialID = " "
                End If

                ShoppingCartcls.DueDate = DueDate
                ShoppingCartcls.Qty = decQty
                ShoppingCartcls.ItemID = strOROItemID
                ShoppingCartcls.VendorID = strsupplierID
                ShoppingCartcls.VendorLoc = strVendorLoc
                ShoppingCartcls.VendorITMID = strsupplierPartID
                ShoppingCartcls.VendorAuxITMID = strSupplierAuxPartID
                ShoppingCartcls.VendorCatalogID = strCplusItemid
                ShoppingCartcls.SHIPID = strShipto
                ShoppingCartcls.UOM = strUOM
                ShoppingCartcls.MfdID = strMfgID
                ShoppingCartcls.MfdName = strMfgName
                ShoppingCartcls.PricePO = decPOAmount
                ShoppingCartcls.UnitPrice = decPrice
                ShoppingCartcls.ReqID = strRFQInd
                ShoppingCartcls.TrackingID = strSerialID
                ShoppingCartcls.SERIALID = strSerialID
                ShoppingCartcls.Description = strDescription
                ShoppingCartcls.Notes = strNotes
                ShoppingCartcls.PartNumber = strMfgPartNum
                ShoppingCartcls.CustPO = strCustPO.ToUpper()
                ShoppingCartcls.CustPOLN = intCustLN
                ShoppingCartcls.EmpID = strEmpID
                ShoppingCartcls.ChargeCode = strChgCD
                ShoppingCartcls.WorkOrder = strWorkOrder
                ShoppingCartcls.MachineNo = strMachineNum
                '  ShoppingCartcls.OrderStatus = strLineStatus
                ShoppingCartcls.LineStatus = strLineStatus
                ShoppingCartcls.User = StrAddToCtlg
                ShoppingCartcls.EnterBY = Session("USERID")
                ShoppingCartcls.TAX_EXEMPT = strExempt_Cert ' strChbTaxFlag ''''Ticket 154676
                ShoppingCartcls.TAX_EXEMPT_CERT = " "
                ShoppingCartcls.Priority = StrPriority
                ShoppingCartcls.MachineNumber = StrMachineNumber
                ShoppingCartcls.UNSPSCInfo = strUNSPSC

                rowsaffected = ShoppingCartcls.UpdateToLineTable()
                If rowsaffected = 0 Then
                    lblDBError.Text = "Error Updating INTFC Line"
                    Exit Sub
                End If

                ShoppingCartcls.MSGTYPE = "GEN"
                ShoppingCartcls.LINENOTES = StrNote
                rowsaffected = ShoppingCartcls.UpdateOrderNotes()
                If rowsaffected = 0 Then
                    lblDBError.Text = "Error Updating INTFC Line Notes"
                    Exit Sub
                End If

                Try
                    If chkAddToCtlg.Checked Then
                        Dim strQuery As String = "INSERT INTO SDIX_CATALOG_ADDS(ADDITION_ID,BUSINESS_UNIT,ADDOPRID,ADDTTM,ORDER_NO,LINE_NO,PARENT_IDENT,NOTES,INVITEMID,STATUS)" &
                                                "VALUES(SDIX_CATALOG_ADDS_SEQ.NEXTVAL,'" & strBU & "','" & strAgent & "',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," &
                                                " '" & strNSTKreqid.ToUpper & "','" & I + 1 & "',0,'" & strNotes & "','" & strOROItemID & "',' ')"
                        rowsaffected = ExecNonQuery(strQuery, False)
                    End If

                Catch ex As Exception

                End Try

                'create Flat file to send to Lyons Magnus (I0531, I0532 for now) - VR 08/16/2018
                Dim strLMLoc As String = ""
                Dim strLMItemID As String = ShoppingCartcls.ItemID

                If bIsLyonsProcessing And CType(ShoppingCartcls.UnitPrice, Decimal) > 0 Then
                    Select Case strBU
                        Case "I0531"
                            strLMLoc = "LMW"
                        Case "I0532"
                            strLMLoc = "LME"
                    End Select
                    If Trim(strLMItemID) <> "" Then
                        strLMItemID = Trim(strLMItemID)
                        'If Len(strLMItemID) > 2 Then
                        '    If Microsoft.VisualBasic.Left(strLMItemID, 3) = Session("SITEPREFIX") Then
                        '        If Len(strLMItemID) > 3 Then
                        '            strLMItemID = Mid(strLMItemID, 4)
                        '        End If
                        '    End If
                        'End If
                    Else
                        strLMItemID = " "
                    End If

                    strChgCD_GL = " "
                    strChgCD_CC = " "
                    strChgCD_PROJ = " "
                    Try
                        If Trim(strChgCD) <> "" Then
                            arrChgCD = Split(strChgCD, "-")
                            If arrChgCD.Length > 0 Then
                                Try
                                    strChgCD_GL = arrChgCD(0)
                                Catch exCCDPart1 As Exception
                                    strChgCD_GL = " "
                                End Try

                                Try
                                    strChgCD_CC = arrChgCD(1)
                                Catch exCCDPart2 As Exception
                                    strChgCD_CC = " "
                                End Try

                                Try
                                    strChgCD_PROJ = arrChgCD(2)
                                Catch exCCDPart3 As Exception
                                    strChgCD_PROJ = " "
                                End Try
                            Else
                                strChgCD_GL = " "
                                strChgCD_CC = " "
                                strChgCD_PROJ = " "
                            End If

                        Else
                            strChgCD_GL = " "
                            strChgCD_CC = " "
                            strChgCD_PROJ = " "
                        End If

                    Catch exChg654 As Exception
                        strChgCD_GL = " "
                        strChgCD_CC = " "
                        strChgCD_PROJ = " "
                    End Try

                    Dim strDescrLine As String = ShoppingCartcls.Description
                    Dim strMfgIdLM As String = ShoppingCartcls.MfdID
                    Dim strMfgITMID As String = ShoppingCartcls.PartNumber
                    ' remove vbTab and CR/LF from Description, MfgID and Mfg Part No
                    strDescrLine = Replace(strDescrLine, vbTab, " ")
                    strDescrLine = Replace(strDescrLine, vbCr, " ")
                    strDescrLine = Replace(strDescrLine, vbCrLf, " ")
                    strDescrLine = Replace(strDescrLine, vbLf, " ")

                    strMfgIdLM = Replace(strMfgIdLM, vbTab, " ")
                    strMfgIdLM = Replace(strMfgIdLM, vbCr, " ")
                    strMfgIdLM = Replace(strMfgIdLM, vbCrLf, " ")
                    strMfgIdLM = Replace(strMfgIdLM, vbLf, " ")

                    strMfgITMID = Replace(strMfgITMID, vbTab, " ")
                    strMfgITMID = Replace(strMfgITMID, vbCr, " ")
                    strMfgITMID = Replace(strMfgITMID, vbCrLf, " ")
                    strMfgITMID = Replace(strMfgITMID, vbLf, " ")

                    StrNote = Replace(StrNote, vbTab, " ")
                    StrNote = Replace(StrNote, vbCr, " ")
                    StrNote = Replace(StrNote, vbCrLf, " ")
                    StrNote = Replace(StrNote, vbLf, " ")

                    writerLnsMagn.WriteLine(strLMLoc & vbTab & ShoppingCartcls.OrderNo & vbTab & ShoppingCartcls.LineNBR & vbTab & strLMItemID & vbTab & strDescrLine & vbTab & ShoppingCartcls.Qty.ToString & vbTab & ShoppingCartcls.UOM & vbTab & ShoppingCartcls.UnitPrice.ToString & vbTab & m_siteCurrency.Id & vbTab & ShoppingCartcls.VendorID & vbTab & ShoppingCartcls.VendorITMID & vbTab & strMfgIdLM & vbTab & strMfgITMID & vbTab & strUNSPSC & vbTab & DueDate.ToString() & vbTab & strChgCD_GL & vbTab & strChgCD_CC & vbTab & strChgCD_PROJ & vbTab & strEmpID & vbTab & StrNote)
                    strSrchFileContents += strLMLoc & "|" & ShoppingCartcls.OrderNo & "|" & ShoppingCartcls.LineNBR & "|" & strLMItemID & "|" & strDescrLine & "|" & ShoppingCartcls.Qty.ToString & "|" & ShoppingCartcls.UOM & "|" & ShoppingCartcls.UnitPrice.ToString & "|" & m_siteCurrency.Id & "|" & ShoppingCartcls.VendorID & "|" & ShoppingCartcls.VendorITMID & "|" & strMfgIdLM & "|" & strMfgITMID & "|" & strUNSPSC & "|" & DueDate.ToString() & "|" & strChgCD_GL & "|" & strChgCD_CC & "|" & strChgCD_PROJ & "|" & strEmpID & "|" & StrNote & " " & vbCrLf
                    bIsAnyLMLinesWithPrice = True
                End If  '  If UCase(Trim(strBU)) = "I0531" Or UCase(Trim(strBU)) = "I0532" Then

            Next
            'create Flat file to send to Lyons Magnus (I0531, I0532 for now) - VR 08/16/2018
            If bIsLyonsProcessing Then
                If bIsAnyLMLinesWithPrice Then
                    writerLnsMagn.Close()

                    Try
                        strSrchFileContents = strSrchFileContents.Replace("'", "''")
                        If Len(strSrchFileContents) > 4000 Then
                            strSrchFileContents = Microsoft.VisualBasic.Left(strSrchFileContents, 4000)
                        End If
                        strsql = "INSERT INTO SDIX_OUTBOUND_AUDIT VALUES ('" + Session("BUSUNIT") + "','" + Session("USERID") + "','" + strNSTKreqid.ToUpper + "',sysdate,'" + strSrchFileName + "', '" + strSrchFileContents.Replace("'", "''") + "')"
                        rowsaffected = ExecNonQuery(strsql, False)
                    Catch
                    End Try
                End If

                If Not bIsAnyLMLinesWithPrice Then
                    writerLnsMagn.Dispose()
                    writerLnsMagn = Nothing
                    Try
                        IO.File.Delete(strPathForLM)
                    Catch exDlt As Exception

                    End Try
                End If
            End If  '  If UCase(Trim(strBU)) = "I0531" Or UCase(Trim(strBU)) = "I0532" Then

        End If

    End Sub



    Sub validateDropShipTo(ByVal sender As Object, ByVal args As ServerValidateEventArgs)
        ' check is there any items in dropShipto first
        If dropShipto.Items.Count > 0 Then
            If Trim(dropShipto.SelectedValue) = "" Then

                Customvalidator4.ErrorMessage = "Invalid Ship To"
                Customvalidator4.IsValid = False
                args.IsValid = False
                Exit Sub
            End If

            If dropShipto.SelectedItem.Text = "-- Select ShipTo --" Then
                Customvalidator4.ErrorMessage = "Invalid Ship To"
                Customvalidator4.IsValid = False
                args.IsValid = False
            End If
        End If
    End Sub

    Sub validateDropEmpID(ByVal sender As Object, ByVal args As ServerValidateEventArgs)

        Dim strEmpid As String = ""

        If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
            'check are all lines in Grid have Empl. Name
            Dim bAllHave As Boolean = False
            'Dim strEmpId As String = ""
            If rgCart.Items.Count > 0 Then
                Dim iCtr As Integer = 0
                bAllHave = True
                For iCtr = 0 To rgCart.Items.Count - 1
                    Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(iCtr), GridDataItem)
                    Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
                    Try
                        strEmpid = Trim(CType(nestedview.FindControl("txtempChgCode"), TextBox).Text)
                    Catch ex As Exception
                        strEmpid = ""
                    End Try
                    If Trim(strEmpid) = "" Then
                        bAllHave = False
                    End If
                Next
            Else
                bAllHave = False
            End If
            If Not bAllHave Then
                Customvalidator1.ErrorMessage = "Employee ID is required"
                Customvalidator1.IsValid = False
                args.IsValid = False
            End If

        End If
    End Sub

    Sub validatetxtReqByDate(ByVal sender As Object, ByVal args As ServerValidateEventArgs)
        Try
            If IsHeaderDueDateRequired() Then
                Customvalidator2.ErrorMessage = "Need by date is required"
                Customvalidator2.IsValid = False
                args.IsValid = False
                Exit Sub
            End If
            If DateTime.Compare(Convert.ToDateTime(RadDatePickerReqByDate.SelectedDate), Now.Date) < 0 Then
                Customvalidator2.ErrorMessage = "Need by date cannot be less than today"
                Customvalidator2.IsValid = False
                args.IsValid = False
            End If
        Catch
        End Try
    End Sub


    Sub validateReqfordups(ByVal sender As Object, ByVal args As ServerValidateEventArgs)

        Dim strSQLstring As String
        Dim strOrderNoDB As String 'order no verified against Database
        Dim strBU As String
        If Session("AGENTUSERID") = "" Then
            strBU = Session("BUSUNIT")
        Else
            strBU = Session("AGENTUSERBU")
        End If
        Try
            If txtOrderNO.Text.ToUpper = "NEXT" Or
                txtOrderNO.Text = "" Then
                Exit Sub
            End If
            'If txtOrderNO.Text.Substring(0, 3) = Session("ORDERPREFIX") Then 
            'If txtOrderNO.Text.Substring(0, 3) <> Session("ORDERPREFIX") And Session("MANUALORDERNUMPREFIXUSERENTERS") = "YES" Then
            '    CustomValidator3.ErrorMessage = "Invalid Order Number"
            '    CustomValidator3.IsValid = False
            '    args.IsValid = False
            '    Exit Sub
            'End If
            Dim orderNo As String = String.Empty

            'If txtOrderNO.Text.Length > 3 Then
            '    If txtOrderNO.Text.Substring(0, 3) = Session("ORDERPREFIX") Then
            '        orderNo = RTrim(txtOrderNO.Text.ToUpper)
            '    Else
            '        orderNo = Session("ORDERPREFIX") & RTrim(txtOrderNO.Text.ToUpper)
            '    End If
            'End If
            If Session("MANORDNO") = "P" Or Trim(Session("MANORDNO")) = "" Then
                orderNo = Session("ORDERPREFIX") & Trim(txtOrderNO.Text.ToUpper)
                orderNo = Trim(orderNo)
            Else
                orderNo = Trim(txtOrderNO.Text.ToUpper)
            End If

            ShoppingCartcls.OrderNo = orderNo

            'strSQLstring = "SELECT A.ORDER_NO" & vbCrLf & _
            '    " FROM PS_ISA_ORD_INTFC_H A" & vbCrLf & _
            '    " WHERE A.ORDER_NO = '" & orderNo & "'"

            strOrderNoDB = ShoppingCartcls.CheckOrderNo() ' ORDBData.GetScalar(strSQLstring)

            'If strOrderNo = Session("ORDERPREFIX") & RTrim(txtOrderNO.Text.ToUpper) Then

            'If strOrderNo = Trim(txtOrderNO.Text.ToUpper) Then
            If strOrderNoDB = orderNo Then
                CustomValidator3.ErrorMessage = "Order Number already exist"
                CustomValidator3.IsValid = False
                args.IsValid = False
                Exit Sub
            End If
            strSQLstring = "SELECT A.REQ_ID" & vbCrLf &
                " FROM PS_REQ_HDR A" & vbCrLf &
                " WHERE A.BUSINESS_UNIT = '" & Session("SITEBU") & "'" & vbCrLf &
                " AND A.REQ_ID = '" & orderNo & "'"
            strOrderNoDB = ORDBData.GetScalar(strSQLstring)

            'If strOrderNo = Trim(txtOrderNO.Text.ToUpper) Then
            If strOrderNoDB = orderNo Then
                CustomValidator3.ErrorMessage = "Order Number already exist"
                CustomValidator3.IsValid = False
                args.IsValid = False
            End If

        Catch ex As Exception

        End Try
    End Sub

    'Private Sub updateCartDatatableASCEND_ZeroPriced()
    '    Dim I As Integer
    '    Dim dstcart1 As New DataTable
    '    dstcart1 = Session("Cart")

    '    If dstcart1 Is Nothing Then
    '        Exit Sub
    '    End If

    '    'Dim pkey() As DataColumn = {dstcart1.Columns("Itemid")}
    '    'dstcart1.PrimaryKey = pkey

    '    Dim tblCartAZ As New DataTable
    '    tblCartAZ = dstcart1.Clone()

    '    ' Loop through the rows in the datagrid looking for the delete checkbox
    '    ' and for 0 priced row
    '    Dim iLnNumber As Integer = 1

    '    For I = 0 To dtgCart.Items.Count - 1

    '        ' Retrieve the ID of the product
    '        ''Dim ProductID As System.Int32 = System.Convert.ToInt32(dtgCart.Items.Item(I).Cells(1).Text)
    '        'Dim ProductID As String = dtgCart.Items.Item(I).Cells(0).Text
    '        'Dim ProductID As String = dtgCart.Items.Item(I).Cells(dtgCartColumns.itemId).Text

    '        dtgCart.EditItemIndex = -1

    '        Dim lblItemID As Label = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)
    '        Dim ProductID As String = lblItemID.Text

    '        'Dim row As DataRow = dstcart1.Rows.Find(ProductID)

    '        Dim iRow As Integer
    '        If FindCartDTRow(dstcart1, ProductID, "", iRow) Then
    '            Dim row As DataRow = dstcart1.Rows(iRow)
    '            ' See if this one needs to be deleted.
    '            'If CType(dtgCart.Items.Item(I).Cells(9).FindControl("chkDelete"), CheckBox).Checked = True Or _
    '            'System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(5).FindControl("txtQTY"), TextBox).Text) = 0 Then
    '            If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.deleteFlag).FindControl("chkDelete"), CheckBox).Checked = True Or _
    '                    CDec(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text) = 0 Then
    '                row.Delete()
    '            Else

    '                Dim decPrice As Decimal = 0
    '                Try
    '                    decPrice = CDec(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text)
    '                Catch ex As Exception
    '                    decPrice = 0
    '                End Try
    '                'row.Item("Quantity") = System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(5).FindControl("txtQTY"), TextBox).Text)
    '                row.Item("Quantity") = CDec(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text)
    '                'row.Item("ItemChgCode") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtItemChgCode"), TextBox).Text
    '                row.Item("ItemChgCode") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtItemChgCode"), TextBox).Text
    '                'row.Item("EmpChgCode") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtEmpChgCode"), TextBox).Text
    '                row.Item("EmpChgCode") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtEmpChgCode"), TextBox).Text
    '                'row.Item("MachineRow") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtMachineRow"), TextBox).Text
    '                row.Item("MachineRow") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtMachineRow"), TextBox).Text
    '                'row.Item("LPP") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtLPP"), TextBox).Text
    '                row.Item("LPP") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtLPP"), TextBox).Text
    '                'row.Item("PO") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtPO"), TextBox).Text
    '                row.Item("PO") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtPO"), TextBox).Text
    '                row.Item("LN") = CType(dtgCart.Items.Item(I).FindControl("txtLN"), TextBox).Text
    '                row.Item("SerialID") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("lblSerialID"), Label).Text
    '                'If CType(dtgCart.Items.Item(I).Cells(12).FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
    '                If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
    '                    row.Item("TaxFlag") = " "
    '                    'ElseIf CType(dtgCart.Items.Item(I).Cells(12).FindControl("chbTaxFlag"), CheckBox).Checked = True Then
    '                ElseIf CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("chbTaxFlag"), CheckBox).Checked = True Then
    '                    row.Item("TaxFlag") = "Y"
    '                Else
    '                    row.Item("TaxFlag") = " "
    '                End If
    '                'code to check is it NONCAT item
    '                Dim bIsNoncat As Boolean = False
    '                Dim sItemID As String = ProductID
    '                If sItemID <> "" Then
    '                    If sItemID.Length > 5 Then
    '                        If UCase(Microsoft.VisualBasic.Left(sItemID, 6)) = "NONCAT" Then
    '                            bIsNoncat = True
    '                        End If
    '                    End If
    '                Else
    '                    ' is it really possible? If yes what to do?
    '                End If  '  sItemID <> ""
    '                If (decPrice = 0) And bIsNoncat Then
    '                    row.Item("LN") = iLnNumber.ToString()
    '                    tblCartAZ.ImportRow(row)
    '                    iLnNumber += 1
    '                End If
    '            End If
    '        End If
    '    Next

    '    Dim i1 As Integer = tblCartAZ.Rows.Count
    '    If i1 > 0 Then
    '        Session("CartAscendZero") = tblCartAZ
    '    Else
    '        Session("CartAscendZero") = Nothing
    '    End If

    '    'dtgCart.DataSource = dstcart1
    '    'dtgCart.DataBind()
    '    ''''''''''''''' zzzzzzzzzzz  this is where the session("cart") is loaded
    '    'Session("Cart") = dstcart1
    '    'If dtgCart.Items.Count = 0 Then
    '    '    txtOrderTot.Text = "0.00"
    '    'End If

    'End Sub

    Private Sub updateCartDatatableASCEND_ZeroPriced()
        Dim I As Integer
        Dim dstcart1 As New DataTable
        dstcart1 = Session("Cart")

        If dstcart1 Is Nothing Then
            Exit Sub
        End If

        'Dim pkey() As DataColumn = {dstcart1.Columns("Itemid")}
        'dstcart1.PrimaryKey = pkey

        Dim tblCartAZ As New DataTable
        tblCartAZ = dstcart1.Clone()

        ' Loop through the rows in the datagrid looking for the delete checkbox
        ' and for 0 priced row
        Dim iLnNumber As Integer = 1

        For I = 0 To rgCart.Items.Count - 1

            Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
            Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)

            Dim lblItemID As Label = CType(rgCart.Items.Item(I).FindControl("lblItemID"), Label)
            Dim ProductID As String = lblItemID.Text
            Dim iRow As Integer
            If FindCartDTRow(dstcart1, ProductID, "", iRow) Then
                Dim row As DataRow = dstcart1.Rows(iRow)
                If CType(nestedview.FindControl("chkDelete"), CheckBox).Checked = True Or
                    CDec(CType(nestedview.FindControl("txtQTY"), TextBox).Text) = 0 Then
                    row.Delete()
                Else

                    Dim decPrice As Decimal = 0
                    Try
                        decPrice = CDec(CType(nestedview.FindControl("Price"), Label).Text)
                    Catch ex As Exception
                        decPrice = 0
                    End Try

                    row.Item("Quantity") = System.Convert.ToDecimal(CType(nestedview.FindControl("txtQTY"), TextBox).Text)
                    row.Item("ItemChgCode") = CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text
                    row.Item("EmpChgCode") = CType(nestedview.FindControl("txtEmpChgCode"), TextBox).Text
                    row.Item("ItemDescription") = CType(GridItem.FindControl("txtItemDescc"), TextBox).Text
                    row.Item("MachineRow") = CType(nestedview.FindControl("txtMachineRow"), TextBox).Text
                    'row.Item("LPP") = CType(nestedview.FindControl("txtLPP"), TextBox).Text
                    row.Item("PO") = CType(nestedview.FindControl("txtPO"), TextBox).Text
                    row.Item("LN") = CType(nestedview.FindControl("txtLN"), TextBox).Text
                    row.Item("SerialID") = CType(nestedview.FindControl("lblSerialID"), Label).Text
                    If CType(nestedview.FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
                        row.Item("TaxFlag") = " "
                    ElseIf CType(nestedview.FindControl("chbTaxFlag"), CheckBox).Checked = True Then
                        row.Item("TaxFlag") = "Y"
                    Else
                        row.Item("TaxFlag") = " "
                    End If
                    ' New code 
                    Dim chkPriority As CheckBox = CType(nestedview.FindControl("chkPriority"), CheckBox)
                    row.Item("UPriority") = " "  '  CType(nestedview.FindControl("chkPriority"), CheckBox).Checked
                    If chkPriority.Checked Then
                        row.Item("UPriority") = "R" '1
                    End If
                    Dim RDPDueDate As RadDatePicker = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker)
                    row.Item("UDueDate") = " "
                    If Not RDPDueDate.SelectedDate Is Nothing Then
                        row.Item("UDueDate") = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker).SelectedDate
                    End If

                    If dropType.Visible Then
                        row.Item("UType") = UCase(dropType.SelectedValue)
                    Else
                        row.Item("UType") = " "
                    End If
                    'row.Item("UType") = UCase(txtType.Text)
                    row.Item("UNotes") = CType(nestedview.FindControl("txtNotes"), TextBox).Text
                    Dim chkAddToCtlg As CheckBox = CType(nestedview.FindControl("chkAddToCtlg"), CheckBox)
                    row.Item("AddToCtlgFlag") = " "
                    If chkAddToCtlg.Checked Then
                        row.Item("AddToCtlgFlag") = "1"
                    End If
                    '' Machine Number 
                    Dim ddlMachineNo As DropDownList = CType(nestedview.FindControl("ddlMachineNo"), DropDownList)
                    Dim txtMachineNo As TextBox = CType(nestedview.FindControl("txtMachineNo"), TextBox)
                    row.Item("MachineNo") = " "
                    If ddlMachineNo.Visible Then
                        If Not ddlMachineNo.SelectedValue Is Nothing Then
                            row.Item("MachineNo") = ddlMachineNo.SelectedValue
                        End If
                    End If
                    If txtMachineNo.Visible Then
                        If Not txtMachineNo.Text Is Nothing Then
                            row.Item("MachineNo") = txtMachineNo.Text
                        End If
                    End If

                    'code to check is it NONCAT item
                    Dim bIsNoncat As Boolean = False
                    Dim sItemID As String = ProductID
                    If sItemID <> "" Then
                        If sItemID.Length > 5 Then
                            If UCase(Microsoft.VisualBasic.Left(sItemID, 6)) = "NONCAT" Then
                                bIsNoncat = True
                            End If
                        End If
                    Else
                        ' is it really possible? If yes what to do?
                    End If  '  sItemID <> ""
                    If decPrice = 0 Then
                        row.Item("LN") = iLnNumber.ToString()
                        tblCartAZ.ImportRow(row)
                        iLnNumber += 1
                    End If

                End If
            End If
        Next

        Dim i1 As Integer = tblCartAZ.Rows.Count
        If i1 > 0 Then
            Session("CartAscendZero") = tblCartAZ
        Else
            Session("CartAscendZero") = Nothing
        End If

    End Sub

    Private Sub UpdateNSTKDB_ASCEND(ByVal strNSTKreqid As String, ByVal strHdrStatus As String, ByVal strEmailAscend As String,
                Optional ByVal bIsPricedProcess As Boolean = False)

        Dim I As Integer
        Dim X As Integer
        Dim Y As Integer
        Dim bolCatLPP As Boolean = False
        Dim strsql As String
        Dim strsqlPart As String
        Dim strsqlINTFC As String
        Dim strsqlLPP As String
        Dim dstNonCatAttrib As DataTable
        Dim StrMachineNumber As String = ""
        Dim strUNSPSC As String = ""

        Dim strCplusItemid As String = " "
        Dim decQty As Decimal = 0
        Dim decPrice As Decimal = 0.0
        Dim decPOAmount As Decimal = 0
        Dim strDescription As String = " "
        Dim strShipto As String = " "
        Dim strsupplierID As String = " "
        Dim strsuppliername As String = " "
        Dim strsupplierPartID As String = " "
        Dim strSupplierAuxPartID As String = " "
        Dim strVendorLoc As String = " "
        Dim strUOM As String = " "
        Dim strRFQInd As String = " "
        Dim strMfgName As String = " "
        Dim strMfgID As String = " "
        Dim strMfgPartNum As String = " "
        Dim strMfgNameLPP As String = " "
        Dim strMfgIDLPP As String = " "
        Dim strMfgPartNumLPP As String = " "
        Dim strNotes As String = " "
        Dim strLPPNotes As String = " "
        Dim strEmpID As String = " "
        Dim strLPP As String = " "
        Dim strChbTaxFlag As String = " "
        Dim strTaxFlagText As String = " "
        Dim decLPP As Decimal = 0.0
        Dim strCustPO As String = " "
        Dim IntCustLN As Integer = 0
        Dim strChgCD As String = " "
        Dim arrChgCD() As String
        Dim strChgCD_GL As String = " "
        Dim strChgCD_CC As String = " "
        Dim strChgCD_PROJ As String = " "
        Dim strSerialID As String = " "

        Dim strItmChgCD As String = " "
        Dim strWorkOrder As String = " "
        Dim strMachineNum As String = " "
        Dim strOROItemID As String = " "
        Dim dteNow As Date
        Dim rowsaffected As Integer
        Dim strValues As String
        Dim strFields As String
        Dim strAttribLabel As String
        Dim strAttribValue As String
        Dim strLineStatus As String
        Dim intIdentifier As Integer
        Dim strPunchout As String = " "

        Dim strOrigin As String = "IOL"

        strLineStatus = "NEW"

        Dim bIsLyonsProcessing As Boolean = False
        If Insiteonline.VoucherSharedFunctions.VoucherClass.IsLyons(strBU) Then
            bIsLyonsProcessing = True
        End If

        Dim bIsPunchoutOrder As Boolean = False
        For I = 0 To rgCart.Items.Count - 1
            Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
            Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
            'code to get SupplierID (is Punchout Order?) - origin PCH
            Try
                If Not IsDBNull(CType(nestedview.FindControl("hdfSupID"), HiddenField).Value) Then
                    strsupplierID = CType(nestedview.FindControl("hdfSupID"), HiddenField).Value
                    strsupplierID = Replace(Trim(strsupplierID), "'", "")
                    If Trim(strsupplierID) <> "" Then
                        If Trim(strsupplierID) <> "&nbsp;" Then
                            bIsPunchoutOrder = True
                        End If
                    End If
                End If
            Catch ex As Exception
                bIsPunchoutOrder = False
            End Try
        Next

        'If bIsPunchoutOrder And bIsPricedProcess Then
        '    strOrigin = "PCH"
        'End If

        'If bIsLyonsProcessing And bIsPricedProcess And (strOrigin <> "PCH") Then
        '    strOrigin = "IOL"
        'End If

        dteNow = Now().ToString("d")


        strWorkOrder = txtWorkOrder.Text
        If Trim(strWorkOrder) = "" Then
            strWorkOrder = " "
        End If

        'Insert into the INTFC PRH and PRL tables
        If strAgent.Length > 10 Then
            strAgent = strAgent.Substring(0, 10)
        End If

        ' For User Type 
        Dim StrType As String = " "

        If dropType.Visible Then
            StrType = UCase(dropType.SelectedValue)
        Else
            StrType = " "
        End If

        If Trim(StrType) <> "" Then
            If Len(StrType) > 2 Then
                StrType = Microsoft.VisualBasic.Left(StrType, 2)
            End If
        End If

        ShoppingCartcls.BU_OM = strBU
        ShoppingCartcls.OrderNo = strNSTKreqid.ToUpper
        ShoppingCartcls.CustID = Session("CUSTID").ToString()
        ShoppingCartcls.Origin = strOrigin  '  "IOL"
        ShoppingCartcls.SourceID = " "
        ShoppingCartcls.OrderStatus = strHdrStatus
        ShoppingCartcls.ReqID = " "
        ShoppingCartcls.OrderType = StrType

        rowsaffected = ShoppingCartcls.UpdateToHeaderTable()

        If rowsaffected = 0 Then
            lblDBError.Text = "Error Updating INTFC header"
            Exit Sub
        End If

        ' New Code for Update Header Notes field 
        ShoppingCartcls.LINENOTES = " "
        If Not String.IsNullOrEmpty(txtNotes.Text.Trim) Then
            ShoppingCartcls.LINENOTES = txtNotes.Text.Trim()
        End If
        ShoppingCartcls.LineNBR = 0
        ShoppingCartcls.MSGTYPE = "GEN"

        rowsaffected = ShoppingCartcls.UpdateOrderNotes()
        If rowsaffected = 0 Then
            lblDBError.Text = "Error Updating INTFC header Notes"
            Exit Sub
        End If

        Dim intParentID As String = strNSTKreqid

        Dim strSrchFileName As String = ""

        'get path to the App Temp dir
        Dim writerLnsMagn As StreamWriter
        Dim strPathForLM As String = ""
        'create Flat file to send to Lyons Magnus (I0531, I0532 for now) - VR 08/16/2018
        If bIsLyonsProcessing Then
            strBU = UCase(Trim(strBU))
            Select Case strBU
                Case "I0531"
                    strSrchFileName = "LMW" & "_" & Session("USERID") & "_" & strNSTKreqid.ToUpper & "_" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

                Case "I0532"
                    strSrchFileName = "LME" & "_" & Session("USERID") & "_" & strNSTKreqid.ToUpper & "_" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

            End Select
            'ConfigurationManager.AppSettings("LyonsMagnusFtpOutbound") & "<File name>"
            strPathForLM = ConfigurationManager.AppSettings("LyonsMagnusFtpOutbound") & strSrchFileName
            writerLnsMagn = New StreamWriter(strPathForLM)
            'add headers
            writerLnsMagn.WriteLine("LOCATION" & vbTab & "QUOTEID" & vbTab & "LINENUM" & vbTab & "PRODUCT_ID" & vbTab & "DESCRIPTION" & vbTab & "QUANTITY" & vbTab & "UOM" & vbTab & "ITEM_PRICE" & vbTab & "CURRENCY" & vbTab & "VENDOR" & vbTab & "VENDORPARTNUM" & vbTab & "MANFID" & vbTab & "MANFPARTNUM" & vbTab & "UNSPSC" & vbTab & "DUEDATE" & vbTab & "GL" & vbTab & "CC" & vbTab & "PROJ" & vbTab & "EMPLOYEE" & vbTab & "LINENOTES")

        End If  '  If UCase(Trim(strBU)) = "I0531" Or UCase(Trim(strBU)) = "I0532" Then

        Dim bIsAnyLMLinesWithPrice As Boolean = False
        Dim StrNote As String = " "

        For I = 0 To rgCart.Items.Count - 1

            strCplusItemid = (CType(rgCart.Items.Item(I).FindControl("lblItemID"), Label).Text).ToString
            If Trim(strCplusItemid) = "" Then strCplusItemid = " "

            Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
            Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)

            strPunchout = CType(nestedview.FindControl("hdfPunchout"), HiddenField).Value

            ''code to check is it NONCAT item
            'Dim bIsNoncat As Boolean = False
            'Dim sItemID As String = strCplusItemid
            'If sItemID <> "" Then
            '    If sItemID.Length > 5 Then
            '        If UCase(Microsoft.VisualBasic.Left(sItemID, 6)) = "NONCAT" Then
            '            bIsNoncat = True
            '        End If
            '    End If
            'Else

            'End If  '  sItemID <> ""

            decQty = System.Convert.ToDecimal(CType(nestedview.FindControl("txtQTY"), TextBox).Text)

            decPrice = 0
            Try
                decPrice = CDec(CType(nestedview.FindControl("Price"), Label).Text)
            Catch ex As Exception
                decPrice = 0
            End Try

            ' Get Item Description 
            '          strDescription = rgCart.Items.Item(I).Cells(dtgCartColumns.itemDescription).Text
            strDescription = CType(GridItem.FindControl("txtItemDescc"), TextBox).Text

            decPOAmount = 0
            strsupplierID = " "
            strsuppliername = " "
            strsupplierPartID = " "
            strSupplierAuxPartID = " "
            strUOM = " "
            strRFQInd = " "
            strMfgName = " "
            strMfgPartNum = " "
            strMfgID = " "
            strMfgNameLPP = " "
            strMfgPartNumLPP = " "
            strMfgIDLPP = " "
            StrNote = " "

            ' Getting Manufacturer 
            strMfgNameLPP = rgCart.Items.Item(I).Cells(dtgCartColumns.mfg).Text

            ' Getting Manuf Part No 
            strMfgPartNumLPP = rgCart.Items.Item(I).Cells(dtgCartColumns.mfgPartNo).Text

            Dim strCplusItemid1 As String = ""
            strCplusItemid1 = (CType(rgCart.Items.Item(I).Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label).Text).ToString
            strMfgName = rgCart.Items.Item(I).Cells(dtgCartColumns.mfg).Text
            strMfgPartNum = rgCart.Items.Item(I).Cells(dtgCartColumns.mfgPartNo).Text
            If strMfgPartNum.Length > 50 Then
                strMfgPartNum = strMfgPartNum.Substring(0, 50)
            End If

            ' Getting UOM 
            strUOM = rgCart.Items.Item(I).Cells(dtgCartColumns.itemUOM).Text

            ' Getting UNSPSC 
            Try
                If Not IsDBNull(CType(nestedview.FindControl("hdfItemUNSPSC"), HiddenField).Value) Then
                    strUNSPSC = CType(nestedview.FindControl("hdfItemUNSPSC"), HiddenField).Value
                    If Trim(strUNSPSC) = "" Then
                        strUNSPSC = " "
                    End If
                Else
                    strUNSPSC = " "
                End If

            Catch ex As Exception
                strUNSPSC = " "
            End Try

            'If Left(strCplusItemid1, 6) = "NONCAT" Then

            ' Getting Supplier ID And Part Number

            If Not IsDBNull(CType(nestedview.FindControl("hdfSupID"), HiddenField).Value) Then
                strsupplierID = CType(nestedview.FindControl("hdfSupID"), HiddenField).Value
            End If

            If Not IsDBNull(CType(nestedview.FindControl("hdfSupPartNum"), HiddenField).Value) Then
                strsupplierPartID = CType(nestedview.FindControl("hdfSupPartNum"), HiddenField).Value
            End If

            ' Getting the SupplierPartAuxiliaryID

            If Not IsDBNull(CType(nestedview.FindControl("hdfSupPartNumAux"), HiddenField).Value) Then
                strSupplierAuxPartID = CType(nestedview.FindControl("hdfSupPartNumAux"), HiddenField).Value
            End If

            ' Getting the Price PO

            If Not IsDBNull(CType(nestedview.FindControl("hdfPricePO"), HiddenField).Value) Then
                If CType(nestedview.FindControl("hdfPricePO"), HiddenField).Value = "&nbsp;" Or String.IsNullOrEmpty(CType(nestedview.FindControl("hdfPricePO"), HiddenField).Value.Trim()) Then
                    decPOAmount = 0.0
                Else
                    decPOAmount = Convert.ToDecimal(CType(nestedview.FindControl("hdfPricePO"), HiddenField).Value)
                End If
            End If
            'End If  ' If Left(strCplusItemid1, 6) = "NONCAT" Then

            If strsupplierID = "&nbsp;" Or strsupplierID = "null" Or Trim(strsupplierID) = "" Then
                strsupplierID = " "
            End If
            If strsuppliername = "&nbsp;" Or strsuppliername = "null" Or Trim(strsuppliername) = "" Then
                strsuppliername = " "
            End If
            If strsupplierPartID = "&nbsp;" Or strsupplierPartID = "null" Or Trim(strsupplierPartID) = "" Then
                strsupplierPartID = " "
            End If
            If strSupplierAuxPartID = "&nbsp;" Or strSupplierAuxPartID = "null" Or Trim(strSupplierAuxPartID) = "" Then
                strSupplierAuxPartID = " "
            End If
            If strUOM = "&nbsp;" Or strUOM = "null" Or Trim(strUOM) = "" Then
                strUOM = " "
            End If
            If strMfgName = "&nbsp;" Or strMfgName = "null" Or Trim(strMfgName) = "" Then
                strMfgName = " "
            End If
            If strMfgPartNum = "&nbsp;" Or strMfgPartNum = "null" Or Trim(strMfgPartNum) = "" Then
                strMfgPartNum = " "
            Else

                strMfgPartNum = Replace(strMfgPartNum, "'", "")
                If Trim(strMfgPartNum) <> "" Then
                    strMfgPartNum = Trim(strMfgPartNum)

                    If strMfgPartNum.Length > 50 Then
                        strMfgPartNum = strMfgPartNum.Substring(0, 50)
                    End If
                Else
                    strMfgPartNum = " "
                End If   '  If Trim(strMfgPartNum) <> "" Then

            End If

            If CType(nestedview.FindControl("lblRFQ"), Label).Text = "Yes" Then '  If rgCart.Items.Item(I).Cells(dtgCartColumns.rfq).Text = "Yes" Then
                strRFQInd = "Y"
            Else
                strRFQInd = "N"
            End If

            If String.IsNullOrEmpty(CType(nestedview.FindControl("hdfItemID"), HiddenField).Value) Or CType(nestedview.FindControl("hdfItemID"), HiddenField).Value = " " Then
                strOROItemID = " "  '  CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
            ElseIf CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.Length < 3 Then
                If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                    strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
                ElseIf CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.Substring(0, 3) = Session("SITEPREFIX") Then
                    strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
                Else
                    strOROItemID = Session("SITEPREFIX") & CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
                End If
            ElseIf CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.ToUpper.Substring(0, 3) = "SDI" Then
                strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.ToUpper
            ElseIf Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.ToUpper
            ElseIf CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.Substring(0, 3) = Session("SITEPREFIX") Then
                strOROItemID = CType(nestedview.FindControl("hdfItemID"), HiddenField).Value
            Else
                strOROItemID = Session("SITEPREFIX") & CType(nestedview.FindControl("hdfItemID"), HiddenField).Value.ToUpper
            End If

            strNotes = txtNotes.Text.ToUpper
            If strNotes = "" Then
                strNotes = " "
            End If

            Try
                If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
                    strEmpID = " "
                Else
                    If Trim(dropEmpID.SelectedItem.Text) <> "" Then
                        strEmpID = Trim(dropEmpID.SelectedItem.Value)
                    Else
                        strEmpID = " "
                    End If
                End If
                ' Getting the Employee Name - line level
                If Trim(CType(nestedview.FindControl("txtempChgCode"), TextBox).Text) <> "" Then  '  Not CType(nestedview.FindControl("txtempChgCode"), TextBox).Text = " " And Not CType(nestedview.FindControl("txtempChgCode"), TextBox).Text = "" Then
                    strEmpID = Trim(CType(nestedview.FindControl("txtempChgCode"), TextBox).Text)
                End If
            Catch ex As Exception
                strEmpID = " "
            End Try

            If strEmpID = "" Then
                strEmpID = " "
            End If

            strChgCD = txtChgCD.Text

            If Not strOROItemID = " " Then
                strItmChgCD = getItemChgCD(strOROItemID, strBU)
                If Not strItmChgCD = "" Then
                    If strChgCD = "" Then
                        strChgCD = strItmChgCD
                    Else
                        strChgCD = strItmChgCD & strChgCdSep & strChgCD
                    End If
                End If
            End If

            ' Getting the charge code
            If Not CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text = " " And
                Not CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text = "" Then
                strChgCD = CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text
            End If
            If strChgCD = "" Then
                strChgCD = " "
            End If

            strSerialID = ""
            If Not CType(nestedview.FindControl("lblSerialID"), Label).Text = " " And
                Not CType(nestedview.FindControl("lblSerialID"), Label).Text = "" Then
                strSerialID = CType(nestedview.FindControl("lblSerialID"), Label).Text
            End If
            If strSerialID = "" Then
                strSerialID = " "
            End If

            'strLPP = ""
            'If Not CType(nestedview.FindControl("txtLPP"), TextBox).Text = " " And _
            '    Not CType(nestedview.FindControl("txtLPP"), TextBox).Text = "" Then
            '    strLPP = CType(nestedview.FindControl("txtLPP"), TextBox).Text
            'End If

            If CType(nestedview.FindControl("chbTaxflag"), CheckBox).Checked = True Then
                strChbTaxFlag = "Y"
                strTaxFlagText = "EXEMPT_CERT"
            Else
                strChbTaxFlag = " "
                strTaxFlagText = " "
            End If
            If Trim(strLPP) = "" Then
                strLPP = "0.00"
            End If
            decLPP = Convert.ToDecimal(strLPP)

            '''''''''''''''''''''''''''''Ticket 154676''''''''''''''''''''''''''
            Dim strExempt_Cert As String = strChbTaxFlag
            If Not Session("CARTTAXEXMPTFLAG") Is Nothing Then
                If Trim(Session("CARTTAXEXMPTFLAG")) = "" Then
                    strExempt_Cert = getExemptStatus(strChgCD, strBU)
                End If
            End If
            '''''''''''''''''''''''''''''''''''''''''''''''''''''

            If txtMachineNum.Visible = True Then
                strMachineNum = txtMachineNum.Text
            Else
                strMachineNum = cmbMachine.Text

                If Not CType(nestedview.FindControl("txtMachineRow"), TextBox).Text = " " And
                    Not CType(nestedview.FindControl("txtMachineRow"), TextBox).Text = "" Then
                    strMachineNum = CType(nestedview.FindControl("txtMachineRow"), TextBox).Text
                End If
            End If
            If Trim(strMachineNum) = "" Then
                strMachineNum = " "
            End If

            'If Not CType(nestedview.FindControl("txtPO"), TextBox).Text = " " And _
            'Not CType(nestedview.FindControl("txtPO"), TextBox).Text = "" Then
            'strCustPO = CType(nestedview.FindControl("txtPO"), TextBox).Text
            'End If

            'Exide updates
            txtCustRef.Text = Replace(txtCustRef.Text, "'", "")
            If Trim(txtCustRef.Text) = "" Then
                txtCustRef.Text = " "
            End If
            strCustPO = txtCustRef.Text

            If Not CType(nestedview.FindControl("txtLN"), TextBox).Text = " " And
               Not CType(nestedview.FindControl("txtLN"), TextBox).Text = "" Then
                IntCustLN = CType(nestedview.FindControl("txtLN"), TextBox).Text
            End If
            ' -------------------------------------------------------------

            Dim tblCurrentASCENDCart As DataTable
            If bIsPricedProcess Then
                tblCurrentASCENDCart = CType(Session("CartAscendPriced"), DataTable)
            Else
                tblCurrentASCENDCart = CType(Session("CartAscendZero"), DataTable)
            End If

            ' Retrieve the ID of the product
            Dim ProductID As String = strCplusItemid1
            Dim iRow As Integer
            If FindCartDTRow(tblCurrentASCENDCart, ProductID, "", iRow) Then
                Dim row As DataRow = tblCurrentASCENDCart.Rows(iRow)
                row.Item("ItemChgCode") = strChgCD
                row.Item("EmpChgCode") = strEmpID
                row.Item("MachineRow") = strMachineNum
                row.Item("LPP") = strLPP
                row.Item("TaxFlag") = strChbTaxFlag
                row.Item("SerialID") = strSerialID

                Dim strLineNumAscend As String = row.Item("LN")

                If (Not Trim(strMfgName) = "") And (Trim(strMfgID) = "") Then
                    strMfgID = getMfgID(strMfgName)
                    If IsDBNull(strMfgID) Then
                        strMfgID = " "
                    ElseIf strMfgID = "" Then
                        strMfgID = " "
                    End If
                End If
                If Not strMfgNameLPP = " " And
                    strMfgIDLPP = " " Then
                    strMfgIDLPP = getMfgID(strMfgNameLPP)
                    If IsDBNull(strMfgIDLPP) Then
                        strMfgIDLPP = " "
                    ElseIf strMfgIDLPP = "" Then
                        strMfgIDLPP = " "
                    End If
                End If
                strShipto = " "
                If dropShipto.Visible = True Then
                    If dropShipto.SelectedIndex > 0 Then
                        strShipto = dropShipto.SelectedValue
                    End If
                Else
                    If Not Trim(txtShipTo.Text) = "" Then
                        strShipto = txtShipTo.Text
                    End If
                End If

                '2019/05/01 proposal
                If strOROItemID.Trim <> "" Then ' strsupplierID = "SDICATALOG" Then
                    ShoppingCartcls.PULINEFIELD = "STK" 'isInventoryID function check exact inventory type (ORO, STK, etc)

                    'Ticket SDI-8013 new rules for Stock items
                    If strsupplierID.Trim = "0000" Or strsupplierID.Trim = "" Then
                        strsupplierID = " "
                        strsupplierPartID = " "
                    End If

                ElseIf strsupplierID.Trim <> "" Then
                    ShoppingCartcls.PULINEFIELD = "ZEUS"
                    If strPunchout.Trim = "PUNCHOUT" Then 'supplier id + punchout
                        ShoppingCartcls.PULINEFIELD = "PCH"
                    End If
                ElseIf strsupplierID.Trim = "" And strOROItemID.Trim = "" And decPrice = 0 Then
                    If Session("RFQSITE") = "Y" Then
                        ShoppingCartcls.PULINEFIELD = "RFQ"
                    Else
                        ShoppingCartcls.PULINEFIELD = "NSTK"
                    End If
                End If
                ' ''''''''''''''''''''''''''''''''''''''''''''''
                'To get the C6 column value for buy again item
                Try
                    Dim strSqlString As String = "SELECT ISA_USER_DEFINED_4 FROM PS_ISA_USER_CART WHERE INV_ITEM_ID = '" + strCplusItemid + "' AND ISA_EMPLOYEE_ID = '" + Session("USERID") + "'"
                    Dim ds As DataSet = ORDBData.GetAdapter(strSqlString)
                    Dim C6_Column As String = ds.Tables(0).Rows(0).Item("ISA_USER_DEFINED_4")
                    If C6_Column <> " " Then
                        If C6_Column = "ZEUS" Or C6_Column = "PCH" Or C6_Column = "RFQ" Or C6_Column = "A" Or C6_Column = "P" Then
                            ShoppingCartcls.PULINEFIELD = "RFQ"
                        End If
                    End If
                Catch ex As Exception
                End Try


                strsupplierID = Replace(strsupplierID, "'", "")
                If Not Trim(strsupplierID) = "" Then
                    strVendorLoc = getVendorLoc(strsupplierID, strShipto)
                End If
                strsupplierPartID = Replace(strsupplierPartID, "'", "")
                If Trim(strsupplierPartID) <> "" Then
                    strsupplierPartID = Trim(strsupplierPartID)
                    If strsupplierPartID.Length > 50 Then
                        strsupplierPartID = strsupplierPartID.Substring(0, 50)
                    End If
                Else
                    strsupplierPartID = " "
                End If
                strSupplierAuxPartID = Replace(strSupplierAuxPartID, "'", "")
                If Trim(strSupplierAuxPartID) <> "" Then
                    strSupplierAuxPartID = Trim(strSupplierAuxPartID)
                    'If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Then
                    If strSupplierAuxPartID.Length > 250 Then
                        strSupplierAuxPartID = strSupplierAuxPartID.Substring(0, 250)
                    End If
                    'Else
                    '    If strSupplierAuxPartID.Length > 60 Then
                    '        strSupplierAuxPartID = strSupplierAuxPartID.Substring(0, 60)
                    '    End If
                    'End If
                Else
                    strSupplierAuxPartID = " "
                End If

                strMfgID = Replace(strMfgID, "'", "")
                If Trim(strMfgID) = "" Then
                    strMfgID = " "
                Else
                    strMfgID = Trim(strMfgID)
                    If strMfgID.Length > 50 Then
                        strMfgID = strMfgID.Substring(0, 50)
                    End If
                End If
                strMfgName = Replace(strMfgName, "'", "")
                If Trim(strMfgName) = "" Then
                    strMfgName = " "
                Else
                    strMfgName = Trim(strMfgName)
                    If strMfgName.Length > 30 Then
                        strMfgName = strMfgName.Substring(0, 30)
                    End If
                End If

                strMfgNameLPP = Replace(strMfgNameLPP, "'", "")
                strDescription = Replace(Left(strDescription, 254), "&", " and ")
                strDescription = Replace(strDescription, "<", " less than ")
                strDescription = Replace(strDescription, ">", " more than ")
                strDescription = Replace(Left(strDescription, 254), "'", "")
                strDescription = Replace(Left(strDescription, 254), "´", "")
                strDescription = Replace(Left(strDescription, 254), ";", " ")
                strDescription = Regex.Replace(strDescription, "\s+", " ")
                strDescription = Left(strDescription, 254)
                strNotes = Replace(strNotes, "'", "")
                strNotes = Left(strNotes, 254)
                strMfgPartNumLPP = Replace(strMfgPartNumLPP, "'", "")
                strWorkOrder = Replace(strWorkOrder, "'", "")
                strMachineNum = Replace(strMachineNum, "'", "")
                If Trim(strMfgIDLPP) = "" Then
                    If strMfgNameLPP.Length > 10 Then
                        strMfgIDLPP = strMfgNameLPP.Substring(0, 10)
                    Else
                        strMfgIDLPP = strMfgNameLPP
                    End If
                End If
                ' sometimes the 10 position has a ' and that blows (the program)!!!! pfd 05212009
                strMfgIDLPP = Replace(strMfgIDLPP, "'", " ")
                If Trim(strMfgIDLPP) = "" Then
                    strMfgIDLPP = " "
                End If
                strEmpID = GetEmpID(strBU, strEmpID)
                strEmpID = Replace(strEmpID, "'", "")
                If Trim(strEmpID) = "" Then
                    strEmpID = " "
                Else
                    strEmpID = Trim(strEmpID)
                    If Len(strEmpID) > 10 Then
                        strEmpID = Microsoft.VisualBasic.Left(strEmpID, 10)
                    End If
                End If

                strsupplierPartID = Replace(strsupplierPartID, "'", "")
                strsupplierPartID = GetSupplierPartId24Char(strsupplierPartID)

                ' Due Date

                Dim DueDate As Nullable(Of Date)
                DueDate = RadDatePickerReqByDate.SelectedDate
                Try

                    DueDate = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker).SelectedDate
                    If IsDBNull(DueDate) Then
                        DueDate = RadDatePickerReqByDate.SelectedDate
                    End If
                    If DueDate Is Nothing Then
                        DueDate = RadDatePickerReqByDate.SelectedDate
                    End If

                Catch ex As Exception
                    DueDate = RadDatePickerReqByDate.SelectedDate
                End Try

                ShoppingCartcls.ItemID = strOROItemID
                If hdfChangeLineNumsAccpt.Value <> "X" Then
                    ShoppingCartcls.LineNBR = strLineNumAscend
                Else
                    ShoppingCartcls.LineNBR = strLineNumAscend * 10
                End If

                ' Attachment code 
                'Dim dt As DataTable = CType(Session("CartAscendZero"), DataTable)
                tblCurrentASCENDCart.Rows(iRow)("LineNo") = ShoppingCartcls.LineNBR
                tblCurrentASCENDCart.AcceptChanges()
                If Not IsDBNull(tblCurrentASCENDCart.Rows(iRow)("FilePath")) Then

                    Dim FilePath As String() = CType(tblCurrentASCENDCart.Rows(iRow)("FilePath"), String())
                    Dim StrFilePath As String = String.Empty
                    For Each f As String In FilePath
                        Dim Result As String = String.Empty
                        Dim FB() As Byte = File.ReadAllBytes(f)
                        Dim Line_NBR As Integer = iRow
                        Dim SCD_NBR As Integer = 1
                        Dim SiteBU As String = CType(Session("SITEBU"), String)
                        Dim Record As String = Convert.ToString(intParentID) + "-" + Convert.ToString(Line_NBR) + "-" + Convert.ToString(SCD_NBR) + "-" + SiteBU
                        Result = NONCatalogFileUpload(FB, f, Record)
                        'StrFilePath = StrFilePath + "<P> <a href='" + Result + "'>" + Path.GetFileName(StrFilePath(f)) + "</a> </P>"
                        StrFilePath = StrFilePath + "<P> <a href='" + Result + "'>" + Path.GetFileName(f) + "</a> </P>" 'yury fix for bug 20170424
                    Next

                    ' Delete File from FileSystem
                    Try
                        Dim FileArr As String() = (From A In FilePath Select Path.GetFileName(A)).ToArray() 'yury
                        DeleteTempFile("", FileArr) 'yury
                        'DeleteTempFile("", FilePath)
                    Catch ex As Exception
                    End Try

                    '  Adding After file uploaded in server. Storing the server file path for including the mail
                    tblCurrentASCENDCart.Rows(iRow)("BuyerFilePath") = StrFilePath
                End If

                tblCurrentASCENDCart.AcceptChanges()

                If bIsPricedProcess Then
                    Session("CartAscendPriced") = tblCurrentASCENDCart
                Else
                    Session("CartAscendZero") = tblCurrentASCENDCart
                End If

                strFields = ""
                strValues = ""

                'strChgCD  
                Dim currloc As Integer
                Dim TaxExempt As String
                Dim StringLength As Integer
                Dim tmpChar As String
                Dim strUnwanted As String = "EXEMP_CERT"
                StringLength = Len(strChgCD)
                For currloc = 1 To StringLength
                    If currloc + 10 <= StringLength Then
                        If Not IsDBNull(strChgCD.Substring(currloc, 10)) Then
                            If strChgCD.Substring(currloc, 10) = strUnwanted Then
                                TaxExempt = "Y"
                            End If
                        End If
                    End If
                Next

                ' For Priority 
                Dim Prioritychk As CheckBox = CType(nestedview.FindControl("chkPriority"), CheckBox)
                Dim StrPriority As String = " "

                If Prioritychk.Checked() Or chbPriority.Checked() Then  '  If bLinePriority Then
                    StrPriority = "R" '1
                Else
                    StrPriority = " "
                End If

                ' For User Notes  
                Dim TypeNotes As TextBox = CType(nestedview.FindControl("txtNotes"), TextBox)

                If Not String.IsNullOrEmpty(Trim(TypeNotes.Text)) Then
                    StrNote = Trim(TypeNotes.Text)
                    StrNote = Replace(StrNote, "'", "")
                    If Trim(StrNote) = "" Then
                        StrNote = " "
                    End If
                End If

                ' For Add To Catalog ? 
                Dim chkAddToCtlg As CheckBox = CType(nestedview.FindControl("chkAddToCtlg"), CheckBox)
                Dim StrAddToCtlg As String = " "

                If chkAddToCtlg.Checked Then
                    StrAddToCtlg = "1"
                Else
                    StrAddToCtlg = " "
                End If

                If String.IsNullOrEmpty(strOROItemID) Then
                    strOROItemID = " "
                End If
                If Trim(strsupplierID) = "0" Then
                    strsupplierID = " "
                End If

                'ShoppingCartcls.ItemID = strOROItemID
                'If hdfChangeLineNumsAccpt.Value <> "X" Then
                '    ShoppingCartcls.LineNBR = strLineNumAscend
                'Else
                '    ShoppingCartcls.LineNBR = strLineNumAscend * 10
                'End If

                'To set the default vendor id and price for NSTK item
                If strOROItemID = " " And strsupplierID = " " And decPOAmount = 0 Then
                    strsupplierID = Session("ISACstVendor")
                    decPOAmount = Session("ISACstPrice")
                End If

                If strsupplierID Is Nothing Then
                    strsupplierID = " "
                End If
                If Trim(strsupplierID) = "" Then
                    strsupplierID = " "
                End If

                Dim cmbMachineNo As DropDownList = CType(nestedview.FindControl("ddlMachineNo"), DropDownList)
                Dim txtMachineNo As TextBox = CType(nestedview.FindControl("txtMachineNo"), TextBox)

                StrMachineNumber = " "

                If cmbMachineNo.Visible Then
                    Dim MachineNoSelected As String = cmbMachineNo.SelectedValue

                    If String.IsNullOrEmpty(MachineNoSelected) Then
                    Else
                        StrMachineNumber = MachineNoSelected
                    End If
                End If

                ' To set the part number for Non Catalog item 
                If txtMachineNo.Visible Then
                    StrMachineNumber = txtMachineNo.Text
                End If

                If Trim(StrMachineNumber) = "" Then
                    StrMachineNumber = " "
                End If
                If Trim(strSerialID) = "" Then
                    strSerialID = " "
                End If

                ShoppingCartcls.DueDate = DueDate
                ShoppingCartcls.Qty = decQty
                ShoppingCartcls.ItemID = strOROItemID
                ShoppingCartcls.VendorID = strsupplierID
                ShoppingCartcls.VendorLoc = strVendorLoc
                ShoppingCartcls.VendorITMID = strsupplierPartID
                ShoppingCartcls.VendorAuxITMID = strSupplierAuxPartID
                ShoppingCartcls.VendorCatalogID = strCplusItemid
                ShoppingCartcls.SHIPID = strShipto
                ShoppingCartcls.UOM = strUOM
                ShoppingCartcls.MfdID = strMfgID
                ShoppingCartcls.MfdName = strMfgName
                ShoppingCartcls.PricePO = decPOAmount
                ShoppingCartcls.UnitPrice = decPrice
                ShoppingCartcls.ReqID = strRFQInd
                ShoppingCartcls.SERIALID = strSerialID
                ShoppingCartcls.TrackingID = strSerialID
                ShoppingCartcls.Description = strDescription
                ShoppingCartcls.Notes = strNotes
                ShoppingCartcls.PartNumber = strMfgPartNum
                ShoppingCartcls.CustPO = strCustPO.ToUpper()
                ShoppingCartcls.CustPOLN = IntCustLN
                ShoppingCartcls.EmpID = strEmpID
                ShoppingCartcls.ChargeCode = strChgCD
                ShoppingCartcls.WorkOrder = strWorkOrder
                ShoppingCartcls.MachineNo = strMachineNum
                ShoppingCartcls.LineStatus = strLineStatus
                ShoppingCartcls.User = StrAddToCtlg
                ShoppingCartcls.TAX_EXEMPT = strExempt_Cert ' strChbTaxFlag ''''Ticket 154676
                ShoppingCartcls.TAX_EXEMPT_CERT = " "
                ShoppingCartcls.EnterBY = Session("USERID")
                ShoppingCartcls.Priority = StrPriority
                ShoppingCartcls.MachineNumber = StrMachineNumber
                ShoppingCartcls.UNSPSCInfo = strUNSPSC

                rowsaffected = ShoppingCartcls.UpdateToLineTable()

                If rowsaffected = 0 Then
                    lblDBError.Text = "Error Updating INTFC Line"
                    Exit Sub
                End If

                ShoppingCartcls.MSGTYPE = "GEN"
                ShoppingCartcls.LINENOTES = StrNote
                rowsaffected = ShoppingCartcls.UpdateOrderNotes()
                If rowsaffected = 0 Then
                    lblDBError.Text = "Error Updating INTFC Line Notes"
                    Exit Sub
                End If

                Try

                    If chkAddToCtlg.Checked Then
                        Dim strQuery As String = "INSERT INTO SDIX_CATALOG_ADDS(ADDITION_ID,BUSINESS_UNIT,ADDOPRID,ADDTTM,ORDER_NO,LINE_NO,PARENT_IDENT,NOTES,INVITEMID,STATUS)" &
                                                "VALUES(SDIX_CATALOG_ADDS_SEQ.NEXTVAL,'" & strBU & "','" & strAgent & "',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," &
                                                " '" & strNSTKreqid.ToUpper & "','" & I + 1 & "',0,'" & strNotes & "','" & strOROItemID & "',' ')"
                        rowsaffected = ExecNonQuery(strQuery, False)
                    End If
                Catch ex As Exception

                End Try

                'create Flat file to send to Lyons Magnus (I0531, I0532 for now) - VR 08/16/2018
                Dim strLMLoc As String = ""
                Dim strLMItemID As String = ShoppingCartcls.ItemID

                If bIsLyonsProcessing And CType(ShoppingCartcls.UnitPrice, Decimal) > 0 Then
                    Select Case strBU
                        Case "I0531"
                            strLMLoc = "LMW"
                        Case "I0532"
                            strLMLoc = "LME"
                    End Select
                    If Trim(strLMItemID) <> "" Then
                        strLMItemID = Trim(strLMItemID)
                        'If Len(strLMItemID) > 2 Then
                        '    If Microsoft.VisualBasic.Left(strLMItemID, 3) = Session("SITEPREFIX") Then
                        '        If Len(strLMItemID) > 3 Then
                        '            strLMItemID = Mid(strLMItemID, 4)
                        '        End If
                        '    End If
                        'End If
                    Else
                        strLMItemID = " "
                    End If

                    strChgCD_GL = " "
                    strChgCD_CC = " "
                    strChgCD_PROJ = " "
                    Try
                        If Trim(strChgCD) <> "" Then
                            arrChgCD = Split(strChgCD, "-")
                            If arrChgCD.Length > 0 Then
                                Try
                                    strChgCD_GL = arrChgCD(0)
                                Catch exCCDPart1 As Exception
                                    strChgCD_GL = " "
                                End Try

                                Try
                                    strChgCD_CC = arrChgCD(1)
                                Catch exCCDPart2 As Exception
                                    strChgCD_CC = " "
                                End Try

                                Try
                                    strChgCD_PROJ = arrChgCD(2)
                                Catch exCCDPart3 As Exception
                                    strChgCD_PROJ = " "
                                End Try
                            Else
                                strChgCD_GL = " "
                                strChgCD_CC = " "
                                strChgCD_PROJ = " "
                            End If

                        Else
                            strChgCD_GL = " "
                            strChgCD_CC = " "
                            strChgCD_PROJ = " "
                        End If

                    Catch exChg654 As Exception
                        strChgCD_GL = " "
                        strChgCD_CC = " "
                        strChgCD_PROJ = " "
                    End Try

                    Dim strDescrLine As String = ShoppingCartcls.Description
                    Dim strMfgIdLM As String = ShoppingCartcls.MfdID
                    Dim strMfgITMID As String = ShoppingCartcls.PartNumber
                    ' remove vbTab and CR/LF from Description, MfgID and Mfg Part No
                    strDescrLine = Replace(strDescrLine, vbTab, " ")
                    strDescrLine = Replace(strDescrLine, vbCr, " ")
                    strDescrLine = Replace(strDescrLine, vbCrLf, " ")
                    strDescrLine = Replace(strDescrLine, vbLf, " ")

                    strMfgIdLM = Replace(strMfgIdLM, vbTab, " ")
                    strMfgIdLM = Replace(strMfgIdLM, vbCr, " ")
                    strMfgIdLM = Replace(strMfgIdLM, vbCrLf, " ")
                    strMfgIdLM = Replace(strMfgIdLM, vbLf, " ")

                    strMfgITMID = Replace(strMfgITMID, vbTab, " ")
                    strMfgITMID = Replace(strMfgITMID, vbCr, " ")
                    strMfgITMID = Replace(strMfgITMID, vbCrLf, " ")
                    strMfgITMID = Replace(strMfgITMID, vbLf, " ")

                    StrNote = Replace(StrNote, vbTab, " ")
                    StrNote = Replace(StrNote, vbCr, " ")
                    StrNote = Replace(StrNote, vbCrLf, " ")
                    StrNote = Replace(StrNote, vbLf, " ")

                    writerLnsMagn.WriteLine(strLMLoc & vbTab & ShoppingCartcls.OrderNo & vbTab & ShoppingCartcls.LineNBR & vbTab & strLMItemID & vbTab & strDescrLine & vbTab & ShoppingCartcls.Qty.ToString & vbTab & ShoppingCartcls.UOM & vbTab & ShoppingCartcls.UnitPrice.ToString & vbTab & m_siteCurrency.Id & vbTab & ShoppingCartcls.VendorID & vbTab & ShoppingCartcls.VendorITMID & vbTab & strMfgIdLM & vbTab & strMfgITMID & vbTab & strUNSPSC & vbTab & DueDate.ToString() & vbTab & strChgCD_GL & vbTab & strChgCD_CC & vbTab & strChgCD_PROJ & vbTab & strEmpID & vbTab & StrNote)
                    bIsAnyLMLinesWithPrice = True
                End If  '  If bIsLyonsProcessing Then

            End If  '  If FindCartDTRow(tblCurrentASCENDCart, ProductID, "", iRow) Then 

        Next
        'create Flat file to send to Lyons Magnus (I0531, I0532 for now) - VR 08/16/2018
        If bIsLyonsProcessing Then
            If bIsAnyLMLinesWithPrice Then
                writerLnsMagn.Flush()
            End If

            writerLnsMagn.Dispose()
            writerLnsMagn = Nothing
            If Not bIsAnyLMLinesWithPrice Then
                IO.File.Delete(strPathForLM)
            End If
        End If  '  If bIsLyonsProcessing Then

    End Sub

    Private Sub buildBuyerConfirmationASCEND(ByVal stritemid As String, ByVal strApprNeeded As String,
                Optional ByVal strEmailASCEND As String = "", Optional ByVal bIsPricedProcess As Boolean = False)

        Dim strSQLString As String
        Dim bFriggin As Boolean = True
        Dim strSubjWithReqId As String = "SDI ZEUS - Material Request Confirmation - " & stritemid

        strSQLString = "SELECT ISA_SITE_EMAIL, ISA_STOCKREQ_EMAIL, ISA_NONSKREQ_EMAIL," & vbCrLf &
            " ISA_SITE_PRINTER," & vbCrLf &
            " ISA_STOCK_PRINTER, ISA_NONSTK_PRINTER" & vbCrLf &
            " FROM SYSADM8.PS_ISA_ENTERPRISE" & vbCrLf &
            " WHERE CUST_ID = '" & Session("CUSTID") & "'" & vbCrLf
        Dim dtrEntReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

        If dtrEntReader.HasRows() = True Then
            dtrEntReader.Read()
        Else
            dtrEntReader.Close()
            Dim strMessage As New Alert
            ltlAlert.Text = strMessage.Say("No enterprise record found")
            Exit Sub
        End If
        Dim strPuncher As Boolean = False
        Dim I As Integer
        Dim X As Integer
        Dim strbodyhead As String = ""
        Dim strbodydetl As String = ""
        Dim strItemtype As String = ""
        Dim intGridloop As Integer
        Dim decOrderTot As Decimal
        Dim dr As DataRow
        Dim SBstk As New StringBuilder
        Dim SWstk As New StringWriter(SBstk)
        Dim htmlTWstk As New HtmlTextWriter(SWstk)
        Dim dataGridHTML As String
        Dim SBnstk As New StringBuilder
        Dim SWnstk As New StringWriter(SBnstk)
        Dim htmlTWnstk As New HtmlTextWriter(SWnstk)
        Dim SBall As New StringBuilder
        Dim SWall As New StringWriter(SBall)
        Dim htmlTWall As New HtmlTextWriter(SWall)
        Dim dtgcart As DataGrid
        Dim dstcart1 As New DataTable

        If bIsPricedProcess Then
            dstcart1 = Session("CartAscendPriced")
        Else
            dstcart1 = Session("CartAscendZero")
        End If

        Dim dstcart2 As New DataTable
        dstcart2 = Session("Cartprint")
        Dim txtBody As String = ""
        Dim txtHdr As String = ""
        Dim txtGrid As String = ""
        Dim txtMsg As String = ""
        Dim strDescr1 As String
        Dim strDescr2 As String
        Dim strDescr3 As String
        Dim strDescr4 As String
        Dim strCustName As String = ""
        Dim strCustEmail As String = " "
        Dim strConnString As String = ORDBData.DbUrl
        Dim connectionEmail As OleDbConnection = New OleDbConnection(strConnString)
        'Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        'Dim MailAttachmentName As String()
        'Dim MailAttachmentbytes As New List(Of Byte())()

        Dim objEmployee As New clsUserTbl(strAgent, strBU)
        strCustName = objEmployee.EmployeeName
        If Trim(strCustName) = "" Then
            Dim objEmplTbl As New clsEmplTbl(strAgent, strBU)
            strCustName = objEmplTbl.EmployeeName
        End If
        If Trim(strCustName) = "" Then
            strCustName = dstcart2.Rows(0).Item(0)
        End If
        If Trim(strEmailASCEND) = "" Then
            strCustEmail = objEmployee.EmployeeEmail
            If Trim(strCustEmail) = "" Then
                Dim objUsertbl As New clsUserTbl(Session("USERID"), Session("BUSUNIT"))
                strCustEmail = objUsertbl.EmployeeEmail
            End If
        Else
            strCustEmail = strEmailASCEND
        End If

        ' add employee emails
        Dim intCntr1 As Integer = 0
        Dim strEmplEmail As String = ""
        Dim strEmplId As String = ""
        strCustEmail = strCustEmail.Trim.ToUpper
        If dstcart1.Rows.Count > 0 Then
            For intCntr1 = 0 To dstcart1.Rows.Count - 1
                ' get column 'EmpChgCode'
                strEmplId = dstcart1.Rows(intCntr1).Item("EmpChgCode")
                If Trim(strEmplId) <> "" Then
                    Dim objUserTbl As New clsUserTbl(strEmplId, strBU)
                    strEmplEmail = objUserTbl.EmployeeEmail
                    Try
                        If Trim(strEmplEmail) <> "" Then
                            If (strCustEmail.IndexOf(strEmplEmail.Trim.ToUpper) > -1) Then
                                'already in
                            Else
                                strCustEmail = strCustEmail & "; " & strEmplEmail.Trim.ToUpper
                            End If
                        End If

                    Catch ex As Exception

                    End Try

                End If  '  If Trim(strEmplId) <> "" Then
            Next  '  For intCntr1 = 0 To dstcart1.Rows.Count - 1
        End If  '  If dstcart1.Rows.Count > 0 Then

        ''Printer variables
        'Dim strPrinterPath As String = dtrEntReader.Item("ISA_STOCK_PRINTER")
        'Dim strUsername As String = "isacorp\i.print"
        'Dim strPassword As String = "webp0501"

        Dim Mailer As MailMessage = New MailMessage

        Mailer.From = "SDIExchange@SDI.com"
        Mailer.Cc = ""
        Mailer.Bcc = ""
        Dim sListSpecialMy1 As String = ""
        Try
            sListSpecialMy1 = Convert.ToString(ConfigurationSettings.AppSettings("MailToSpecial"))
            If sListSpecialMy1 Is Nothing Then
                sListSpecialMy1 = ""
            End If
        Catch ex As Exception
            sListSpecialMy1 = ""
        End Try
        If Trim(sListSpecialMy1) <> "" Then
            Mailer.Bcc = Trim(sListSpecialMy1)
        End If

        If CheckOrderPriority(dstcart1) Then
            strbodyhead = "<span ><B>**PRIORITY ORDER**</B></span>"
        End If

        'strbodyhead = strbodyhead & "<span style='height: 182px; width: 101px;'><img src='https://www.sdizeus.com/images/SDILogo_Email.png' /></span><center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
        strbodyhead = strbodyhead & "<table width='100%' bgcolor='black'><tbody><tr><td><img src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI'/></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large; text-align: center; color:white; display: inline-block; margin-top:35px;'>SDI Marketplace</span></center></td></tr></tbody></table>" & vbCrLf
        strbodyhead = strbodyhead & "&nbsp;" & vbCrLf

        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span >Item request from </span>&nbsp;"
        strbodydetl = strbodydetl & Session("CONAME") & "</td>" & vbCrLf
        strbodydetl = strbodydetl & "<TD style='COLOR: red'>" & vbCrLf
        If strApprNeeded = "TRUE" Then
            If bIsPricedProcess Then
                strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
            Else
                strbodydetl = strbodydetl & "<span>** ADDITIONAL APPROVALS REQUIRED **</span></td></tr>" & vbCrLf
            End If
        Else
            strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        End If
        strbodydetl = strbodydetl & "<TD colspan='2'>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;</td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>SDI Requisition Number:</span> <span 'width:128px;'>&nbsp;" & stritemid & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "Chg. Emp. ID:<span>&nbsp;" & dstcart2.Rows(0).Item(0) & "</span></td></tr>"

        strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Name:</span> <span>&nbsp;" & strCustName & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "Chg. Code:<span>&nbsp;" & dstcart2.Rows(0).Item(1) & "</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Email:</span> <span>&nbsp;" & strCustEmail & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Work Order #:</span> <span>&nbsp;" & dstcart2.Rows(0).Item(2) & "</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Phone#: </span><span>&nbsp;" & Session("PHONE") & "</span></td>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "Machine Number:<span>&nbsp;" & dstcart2.Rows(0).Item(3) & "</span></td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
        'strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Request by Date:</span> <span>&nbsp;" & dstcart2.Rows(0).Item(5) & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Submit Date:</span> <span>&nbsp;" & Now() & "</span></td></tr>"
        strbodydetl = strbodydetl & "<TD colspan='2'>" & vbCrLf
        strbodydetl = strbodydetl & "Notes:<br>"
        strbodydetl = strbodydetl & "<textarea readonly='readonly' style='width:100%;'>" & dstcart2.Rows(0).Item(4) & "</textarea></td></tr></table>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;<br>" & vbCrLf

        txtHdr = vbCrLf & "                              IN PLANT STORE PROGRAM"
        txtHdr = txtHdr & vbCrLf & vbCrLf & "                    " & strSubjWithReqId & vbCrLf & vbCrLf

        txtHdr = txtHdr & "Item Request from " & Session("CONAME") & ":" & vbCrLf & vbCrLf
        txtHdr = txtHdr & "   SDI Requisition Number: " & stritemid & vbCrLf
        txtHdr = txtHdr & "   Employee Name: " & strCustName & vbCrLf
        '  txtHdr = txtHdr & "   Request by Date: " & dstcart2.Rows(0).Item(5) & vbCrLf
        txtHdr = txtHdr & "   Submit Date: " & Now() & vbCrLf
        txtHdr = txtHdr & "   Chg. Emp. ID: " & dstcart2.Rows(0).Item(0) & vbCrLf
        txtHdr = txtHdr & "   Chg. Code: " & dstcart2.Rows(0).Item(1) & vbCrLf
        txtHdr = txtHdr & "   Work Order #: " & dstcart2.Rows(0).Item(2) & vbCrLf
        'txtHdr = txtHdr & "   Machine Number: " & dstcart2.Rows(0).Item(3) & vbCrLf

        Dim strNotes1 As String = UCase(Left(dstcart2.Rows(0).Item(4), 68))
        Dim strNotes2 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 69, 68))
        Dim strNotes3 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 137, 68))
        Dim strNotes4 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 205, 50))
        txtHdr = txtHdr & "   Notes: " & strNotes1 & vbCrLf
        If Not Trim(strNotes2) = "" Then
            txtHdr = txtHdr & "          " & strNotes2 & vbCrLf
        End If
        If Not Trim(strNotes3) = "" Then
            txtHdr = txtHdr & "          " & strNotes3 & vbCrLf
        End If
        If Not Trim(strNotes4) = "" Then
            txtHdr = txtHdr & "          " & strNotes4 & vbCrLf
        End If
        txtHdr = txtHdr & vbCrLf

        Dim strOROItemID As String = ""
        Dim bolMachineNum As Boolean = False
        Dim nItem_id_Price As Integer = 0

        'Dim dstcartSTK As New DataTable
        ''1
        'If bIsPricedProcess Then
        '    dstcartSTK = buildCartforemailSC(dstcart1)

        '    intGridloop = dstcartSTK.Rows.Count
        '    X = 0
        '    decOrderTot = 0

        'End If

        Dim dstCartNSTK As New DataTable

        If Not bIsPricedProcess Then

            Try

                dstCartNSTK = buildCartforemailSC(dstcart1)
                bolMachineNum = False
                Dim objEnterprise As New clsEnterprise(strBU)
                strTaxFlag = objEnterprise.TaxFlag

                intGridloop = dstCartNSTK.Rows.Count
                X = 0
                decOrderTot = 0
                For I = 0 To intGridloop - 1
                    If Trim(dstCartNSTK.Rows(X).Item("Item ID")) = "" Then
                        strOROItemID = " "  ' dstCartNSTK.Rows(X).Item("Item ID")
                    ElseIf Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")).Length < 3 Then
                        If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                            strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                        ElseIf dstCartNSTK.Rows(X).Item("Item ID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                            strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                        Else
                            strOROItemID = Session("SITEPREFIX") & dstCartNSTK.Rows(X).Item("Item ID")
                        End If
                    ElseIf Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")).Substring(0, 3) = "SDI" Then
                        strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                    ElseIf Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                        strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                    ElseIf dstCartNSTK.Rows(X).Item("Item ID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                        strOROItemID = dstCartNSTK.Rows(X).Item("Item ID")
                    Else
                        strOROItemID = Session("SITEPREFIX") & dstCartNSTK.Rows(X).Item("Item ID")
                    End If

                    Dim itm As sdiItem = sdiItem.GetItemInfo(strOROItemID)
                    If (Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")) = " " Or
                                                       Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")) = "") And
                                                          Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price")) <> 0 Then
                        strPuncher = True
                    End If
                    If dstcart1.Rows(X).Item("paintColor").ToString = "PUNCHOUT" Then
                        strPuncher = True
                    End If

                    If Trim(dstCartNSTK.Rows(X).Item("Item ID")) = "" Then
                        decOrderTot = decOrderTot + Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price"))

                        If Not Trim(dstCartNSTK.Rows(X).Item("Machine Num")) = "" Then
                            bolMachineNum = True
                        End If
                        X = X + 1

                    ElseIf itm.StockType.Id = "ORO" Or
                        Convert.ToString(dstCartNSTK.Rows(X).Item("Item ID")).Substring(0, 3) = "SDI" Then
                        If Left(dstCartNSTK.Rows(X).Item("Item Number"), 6) = "NONCAT" Then
                            dstCartNSTK.Rows(X).Item("Item Number") = " "
                        End If
                        decOrderTot = decOrderTot + Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price"))
                        If Not Trim(dstCartNSTK.Rows(X).Item("Machine Num")) = "" Then
                            bolMachineNum = True
                        End If
                        X = X + 1
                    ElseIf Convert.ToDecimal(dstCartNSTK.Rows(X).Item("Ext. Price")) = 0 Then
                        X = X + 1
                    Else
                        dstCartNSTK.Rows(X).Delete()
                    End If

                Next
                dstCartNSTK.AcceptChanges()

                '// material request NON-STOCK
                '//     received by SYSADM8.PS_ISA_ENTERPRISE.ISA_NONSKREQ_EMAIL
                ' send email to the buyer
                If dstCartNSTK.Rows.Count > 0 Then

                    dtgcart = New DataGrid

                    dstCartNSTK.Columns.Remove("Item ID")
                    dstCartNSTK.Columns.Remove("Bin Location")
                    'dstCartNSTK.Columns.Remove("RFQ")
                    If bolMachineNum = False Then
                        dstCartNSTK.Columns.Remove("Machine Num")
                    End If
                    If Not strTaxFlag = "Y" Then
                        dstCartNSTK.Columns.Remove("Tax Exempt")
                    End If
                    If Not Session("SITEBU") = "SDM00" Then
                        dstCartNSTK.Columns.Remove("PO")
                        dstCartNSTK.Columns.Remove("LN")
                    End If
                    'dstcartNSTK.Columns.Remove("Item Chg Code")
                    'Code for flag based price display
                    If Session("ShowPrice") = "0" Then
                        dstCartNSTK.Columns.Remove("Price")
                        dstCartNSTK.Columns.Remove("Ext. Price")
                    End If
                    If Session("BUSUNIT") = "SDM00" Or Session("BUSUNIT") = "ISA00" Or Session("BUSUNIT") = "WAL00" Or Session("BUSUNIT") = "I0W01" Then
                        dstCartNSTK.Columns.Remove("Price")
                        dstCartNSTK.Columns.Remove("Ext. Price")
                        dstCartNSTK.Columns.Remove("Estimated Price")
                    End If
                    dtgcart.DataSource = dstCartNSTK
                    dtgcart.DataBind()
                    dtgcart.CellPadding = 3
                    dtgcart.BorderColor = Gray
                    dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
                    dtgcart.HeaderStyle.Font.Bold = True
                    dtgcart.HeaderStyle.ForeColor = Black
                    dtgcart.Width.Percentage(90)

                    dtgcart.RenderControl(htmlTWnstk)

                    dataGridHTML = SBnstk.ToString()
                    strItemtype = "<center><span >SDI ZEUS - Material Request - Non-Stock/ORO Items</span></center>"
                    If strPuncher = True Then
                        strItemtype = strItemtype + "<br><B><left><span style='font-family:Arial;color: red;font-size:Medium;'> ***NOTE*** THIS IS A PUNCHOUT ORDER!!! </span></B></left>"
                        '"***NOTE*** THIS IS A PUNCHOUT ORDER!!!"
                    End If
                    strPuncher = False
                    Mailer.Body = strbodyhead & strItemtype & strbodydetl
                    Mailer.Body = Mailer.Body & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
                    Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
                    Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
                    'Mailer.body = Mailer.body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
                    Mailer.Body = Mailer.Body & "</TABLE>" & vbCrLf
                    Mailer.Body = Mailer.Body & "<HR width='100%' SIZE='1'>" & vbCrLf
                    Mailer.Body = Mailer.Body & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />" & vbCrLf
                    ''If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Or _
                    ''ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "RPTG" Then
                    ''    Mailer.to = "DoNotSendPLGR@sdi.com"
                    ''    'Mailer.to = "VITALY.ROVENSKY@sdi.com"
                    ''    'Mailer.To = dtrEntReader.Item("ISA_NONSKREQ_EMAIL")
                    ''Else
                    ''    Mailer.To = dtrEntReader.Item("ISA_NONSKREQ_EMAIL")
                    ''End If
                    ''If chbPriority.Checked = True Then
                    ''    If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Or _
                    ''    ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "RPTG" Then
                    ''        Mailer.to = "DoNotSendPLGR@sdi.com"
                    ''        'Mailer.to = "VITALY.ROVENSKY@sdi.com"
                    ''        'Mailer.To = dtrEntReader.Item("ISA_SITE_EMAIL")
                    ''    Else
                    ''        Mailer.To = dtrEntReader.Item("ISA_SITE_EMAIL")
                    ''    End If
                    ''Else
                    ''    If ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "PLGR" Or _
                    ''    ORDBData.DbUrl.Substring(ORDBData.DbUrl.Length - 4).ToUpper = "RPTG" Then
                    ''        Mailer.to = "DoNotSendPLGR@sdi.com"
                    ''        'Mailer.to = "VITALY.ROVENSKY@sdi.com"
                    ''        'Mailer.To = dtrEntReader.Item("ISA_NONSKREQ_EMAIL")
                    ''    Else
                    ''        Mailer.To = dtrEntReader.Item("ISA_NONSKREQ_EMAIL")
                    ''    End If

                    ''End If

                    'Logic Rewrite
                    If Not getDBName() Then
                        Mailer.To = "WebDev@sdi.com;avacorp@sdi.com"
                        Mailer.Subject = "<<TEST SITE>> SDI ZEUS - Material Request - Non-Stock/ORO Items - " & stritemid
                    Else
                        Mailer.Subject = "SDI ZEUS - Material Request - Non-Stock/ORO Items - " & stritemid
                        Dim strNstkEmailFull As String = ""
                        strNstkEmailFull = Trim(dtrEntReader.Item("ISA_NONSKREQ_EMAIL"))
                        If CheckOrderPriority(dstcart1) Then
                            If Right(strNstkEmailFull, 1) = ";" Then

                            Else
                                strNstkEmailFull = strNstkEmailFull & ";"
                            End If
                            strNstkEmailFull = strNstkEmailFull & dtrEntReader.Item("ISA_SITE_EMAIL")

                        End If
                        Mailer.To = strNstkEmailFull
                    End If

                    Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

                    Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
                    Dim MailAttachmentName As String()
                    Dim MailAttachmentbytes As New List(Of Byte())()

                    Try
                        SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "MATNONSTOCK", MailAttachmentName, MailAttachmentbytes.ToArray())

                    Catch ex As Exception
                        Dim strErr As String = ex.Message
                    End Try

                    'Try
                    '    UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, Mailer.From, Mailer.To, Mailer.Cc, Mailer.Bcc, "MATNONSTOCK", Mailer.Body, connectionEmail)
                    '    Try
                    '        connectionEmail.Close()
                    '    Catch ex As Exception

                    '    End Try
                    'Catch exEmailOut As Exception
                    '    Try
                    '        connectionEmail.Close()
                    '    Catch ex As Exception

                    '    End Try
                    '    Dim strerr As String = ""
                    '    strerr = exEmailOut.Message
                    'End Try
                    'SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, Mailer.Cc, Mailer.Bcc, Mailer.Body, "MATNONSTOCK", MailAttachmentName, MailAttachmentbytes.ToArray())

                    SWnstk.Close()
                    htmlTWnstk.Close()

                End If

            Catch ex As Exception
                'error sending non-stock e-mail ASCEND
                Dim strErrorFromEmail As String = "Error from ShoppingCart.buildBuyerConfirmation, sending non-stock e-mail. User ID: " & Session("USERID") & "; " & vbCrLf &
                    "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                    "Error message: " & ex.Message
                SendSDiExchErrorMail(strErrorFromEmail, "ASCEND: Error from ShoppingCart.buildBuyerConfirmation, sending non-stock e-mail")
            End Try

        End If  '  If Not bIsPricedProcess Then

        'Send confirmation email to customer
        Dim dstcartAll As New DataTable

        Try

            dstcartAll = buildCartforemailSC(dstcart1)
            bolMachineNum = False
            intGridloop = dstcartAll.Rows.Count
            X = 0
            decOrderTot = 0
            For I = 0 To intGridloop - 1
                If Trim(dstcartAll.Rows(X).Item("Item ID")) = "" Then
                    strOROItemID = " "  '  dstcartAll.Rows(X).Item("Item ID")
                ElseIf Convert.ToString(dstcartAll.Rows(X).Item("Item ID")).Length < 3 Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                ElseIf Convert.ToString(dstcartAll.Rows(X).Item("Item ID")).Substring(0, 3) = "SDI" Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                ElseIf Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                ElseIf dstcartAll.Rows(X).Item("Item ID").ToString().Substring(0, 3) = Session("SITEPREFIX") Then
                    strOROItemID = dstcartAll.Rows(X).Item("Item ID")
                Else
                    strOROItemID = Session("SITEPREFIX") & dstcartAll.Rows(X).Item("Item ID")
                End If

                If Len(dstcartAll.Rows(X).Item("Item Number")) > 5 Then
                    If Left(dstcartAll.Rows(X).Item("Item Number"), 6) = "NONCAT" Then
                        dstcartAll.Rows(X).Item("Item Number") = " "
                    End If
                End If

                decOrderTot = decOrderTot + Convert.ToDecimal(dstcartAll.Rows(X).Item("Ext. Price"))
                If Not Trim(dstcartAll.Rows(X).Item("Machine Num")) = "" Then
                    bolMachineNum = True
                End If
                X = X + 1
            Next
            dstcartAll.AcceptChanges()

            '// material request CONFIRMATION
            '//     received by CUSTOMER - "strCustEmail"; build at the top

            If dstcartAll.Rows.Count > 0 Then

                dtgcart = New DataGrid

                dstcartAll.Columns.Remove("Item ID")
                dstcartAll.Columns.Remove("Bin Location")
                'dstcartAll.Columns.Remove("RFQ")
                If bolMachineNum = False Then
                    dstcartAll.Columns.Remove("Machine Num")
                End If
                If Not strTaxFlag = "Y" Then
                    dstcartAll.Columns.Remove("Tax Exempt")
                End If
                If Not Session("SITEBU") = "SDM00" Then
                    dstcartAll.Columns.Remove("PO")
                    dstcartAll.Columns.Remove("LN")
                End If
                'Code for flag based price display
                If Session("ShowPrice") = "0" Then
                    dstcartAll.Columns.Remove("Price")
                    dstcartAll.Columns.Remove("Ext. Price")
                End If
                If Session("BUSUNIT") = "SDM00" Or Session("BUSUNIT") = "ISA00" Or Session("BUSUNIT") = "WAL00" Or Session("BUSUNIT") = "I0W01" Then
                    dstcartAll.Columns.Remove("Price")
                    dstcartAll.Columns.Remove("Ext. Price")
                    dstcartAll.Columns.Remove("Estimated Price")
                End If
                dtgcart.DataSource = dstcartAll
                dtgcart.DataBind()
                dtgcart.CellPadding = 3
                dtgcart.BorderColor = Gray
                dtgcart.HeaderStyle.BackColor = System.Drawing.Color.LightGray
                dtgcart.HeaderStyle.Font.Bold = True
                dtgcart.HeaderStyle.ForeColor = Black
                dtgcart.Width.Percentage(90)

                dtgcart.RenderControl(htmlTWall)

                dataGridHTML = SBall.ToString()
                strItemtype = "<center><span >SDI ZEUS - Material Request - Confirmation</span></center>"
                Mailer.Body = strbodyhead & strItemtype & strbodydetl
                Mailer.Body = Mailer.Body & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
                Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' width='100%'>" & dataGridHTML & "</TD></TR>"
                Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow'>&nbsp;</TD></TR>"
                If Session("BUSUNIT") = "SDM00" Or Session("BUSUNIT") = "ISA00" Or Session("BUSUNIT") = "WAL00" Or Session("BUSUNIT") = "I0W01" Then
                    decOrderTot = "0.00"
                    Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
                Else
                    If Session("ShowPrice") <> "0" Then
                        Mailer.Body = Mailer.Body + "<TR><TD Class='DetailRow' align='right'>Order Total&nbsp;&nbsp;&nbsp;" & decOrderTot & "&nbsp;&nbsp;&nbsp;</TD></TR>"
                    End If
                End If

                Mailer.Body = Mailer.Body & "</TABLE>" & vbCrLf
                Mailer.Body = Mailer.Body & "<HR width='100%' SIZE='1'>" & vbCrLf
                Mailer.Body = Mailer.Body & "<img src='https://www.sdizeus.com/Images/SDIFooter_Email.png' />" & vbCrLf
                If Not getDBName() Then
                    Mailer.To = "WebDev@sdi.com;avacorp@sdi.com"
                    Mailer.Subject = "<<TEST SITE>> " & strSubjWithReqId

                Else
                    Mailer.To = strCustEmail
                    Mailer.Subject = strSubjWithReqId
                End If


                Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

                Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
                Dim MailAttachmentName As String()
                Dim MailAttachmentbytes As New List(Of Byte())()

                Try
                    SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "MATREQCONFIRM", MailAttachmentName, MailAttachmentbytes.ToArray())

                Catch ex As Exception
                    Dim strErr As String = ex.Message
                End Try

                'Try
                '    UpdEmailOut.UpdEmailOut.UpdEmailOut(Mailer.Subject, Mailer.From, Mailer.To, Mailer.Cc, Mailer.Bcc, "MATREQCONFIRM", Mailer.Body, connectionEmail)
                '    Try
                '        connectionEmail.Close()
                '    Catch ex As Exception

                '    End Try
                'Catch exEmailOut As Exception
                '    Try
                '        connectionEmail.Close()
                '    Catch ex As Exception

                '    End Try
                '    Dim strerr As String = ""
                '    strerr = exEmailOut.Message
                'End Try
                'SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, Mailer.Cc, Mailer.Bcc, Mailer.Body, "MATREQCONFIRM", MailAttachmentName, MailAttachmentbytes.ToArray())

                SWall.Close()
                htmlTWall.Close()

            End If

        Catch ex As Exception
            'error sending customer e-mail ASCEND
            Dim strErrorFromEmail As String = "Error from ShoppingCart.buildBuyerConfirmation, sending customer e-mail. User ID: " & Session("USERID") & "; " & vbCrLf &
                "BU: " & Session("BUSUNIT") & " ; Server: " & Session("WEBSITEID") & " :: " & Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "ASCEND: Error from ShoppingCart.buildBuyerConfirmation, sending customer e-mail")
        End Try

        dtrEntReader.Close()
    End Sub

    'Private Sub updateCartDatatableASCEND_Priced()
    '    Dim I As Integer
    '    Dim dstcart1 As New DataTable
    '    dstcart1 = Session("Cart")

    '    If dstcart1 Is Nothing Then
    '        Exit Sub
    '    End If

    '    'Dim pkey() As DataColumn = {dstcart1.Columns("Itemid")}
    '    'dstcart1.PrimaryKey = pkey

    '    Dim tblCartAP As New DataTable
    '    tblCartAP = dstcart1.Clone()

    '    ' Loop through the rows in the datagrid looking for the delete checkbox
    '    ' and for 0 priced row
    '    For I = 0 To dtgCart.Items.Count - 1

    '        ' Retrieve the ID of the product
    '        ''Dim ProductID As System.Int32 = System.Convert.ToInt32(dtgCart.Items.Item(I).Cells(1).Text)
    '        'Dim ProductID As String = dtgCart.Items.Item(I).Cells(0).Text
    '        'Dim ProductID As String = dtgCart.Items.Item(I).Cells(dtgCartColumns.itemId).Text

    '        dtgCart.EditItemIndex = -1

    '        Dim lblItemID As Label = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)

    '        Dim ProductID As String = lblItemID.Text

    '        'Dim row As DataRow = dstcart1.Rows.Find(ProductID)
    '        'Dim row As DataRow = dtgCart.Items.Item(I)

    '        Dim iRow As Integer
    '        If FindCartDTRow(dstcart1, ProductID, "", iRow) Then
    '            Dim row As DataRow = dstcart1.Rows(iRow)

    '            ' See if this one needs to be deleted.
    '            'If CType(dtgCart.Items.Item(I).Cells(9).FindControl("chkDelete"), CheckBox).Checked = True Or _
    '            'System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(5).FindControl("txtQTY"), TextBox).Text) = 0 Then
    '            If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.deleteFlag).FindControl("chkDelete"), CheckBox).Checked = True Or _
    '                System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text) = 0 Then
    '                row.Delete()
    '            Else

    '                Dim decPrice As Decimal = 0
    '                Try
    '                    decPrice = CDec(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.itemPrice).FindControl("Price"), Label).Text)
    '                Catch ex As Exception
    '                    decPrice = 0
    '                End Try
    '                'row.Item("Quantity") = System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(5).FindControl("txtQTY"), TextBox).Text)
    '                row.Item("Quantity") = System.Convert.ToDecimal(CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.qtyToOrder).FindControl("txtQTY"), TextBox).Text)
    '                'row.Item("ItemChgCode") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtItemChgCode"), TextBox).Text
    '                row.Item("ItemChgCode") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtItemChgCode"), TextBox).Text
    '                'row.Item("EmpChgCode") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtEmpChgCode"), TextBox).Text
    '                row.Item("EmpChgCode") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtEmpChgCode"), TextBox).Text
    '                'row.Item("MachineRow") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtMachineRow"), TextBox).Text
    '                row.Item("MachineRow") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtMachineRow"), TextBox).Text
    '                'row.Item("LPP") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtLPP"), TextBox).Text
    '                row.Item("LPP") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtLPP"), TextBox).Text
    '                'row.Item("PO") = CType(dtgCart.Items.Item(I).Cells(12).FindControl("txtPO"), TextBox).Text
    '                row.Item("PO") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("txtPO"), TextBox).Text
    '                row.Item("LN") = CType(dtgCart.Items.Item(I).FindControl("txtLN"), TextBox).Text
    '                row.Item("SerialID") = CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("lblSerialID"), Label).Text
    '                'If CType(dtgCart.Items.Item(I).Cells(12).FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
    '                If CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
    '                    row.Item("TaxFlag") = " "
    '                    'ElseIf CType(dtgCart.Items.Item(I).Cells(12).FindControl("chbTaxFlag"), CheckBox).Checked = True Then
    '                ElseIf CType(dtgCart.Items.Item(I).Cells(dtgCartColumns.lineItemInfo).FindControl("chbTaxFlag"), CheckBox).Checked = True Then
    '                    row.Item("TaxFlag") = "Y"
    '                Else
    '                    row.Item("TaxFlag") = " "
    '                End If
    '                'code to check is it NONCAT item
    '                Dim bIsNoncat As Boolean = False
    '                Dim sItemID As String = ProductID
    '                If sItemID <> "" Then
    '                    If sItemID.Length > 5 Then
    '                        If UCase(Microsoft.VisualBasic.Left(sItemID, 6)) = "NONCAT" Then
    '                            bIsNoncat = True
    '                        End If
    '                    End If
    '                Else
    '                    ' is it really possible? If yes what to do?
    '                End If  '  sItemID <> ""
    '                If (decPrice = 0) And (Not bIsNoncat) Then
    '                    tblCartAP.ImportRow(row)
    '                End If
    '                If decPrice > 0 Then
    '                    tblCartAP.ImportRow(row)
    '                End If
    '            End If
    '        End If
    '    Next

    '    'Dim pkey() As DataColumn = {dstcart1.Columns("Itemid")}
    '    'dstcart1.PrimaryKey = pkey

    '    Dim i1 As Integer = tblCartAP.Rows.Count
    '    If i1 > 0 Then
    '        Session("CartAscendPriced") = tblCartAP
    '    Else
    '        Session("CartAscendPriced") = Nothing
    '    End If


    '    'dtgCart.DataSource = dstcart1
    '    'dtgCart.DataBind()
    '    ''''''''''''''' zzzzzzzzzzz  this is where the session("cart") is loaded
    '    'Session("Cart") = dstcart1
    '    'If dtgCart.Items.Count = 0 Then
    '    '    txtOrderTot.Text = "0.00"
    '    'End If

    'End Sub

    Dim iLnNumber As Integer = 1

    Private Sub updateCartDatatableASCEND_Priced()
        Dim I As Integer
        Dim dstcart1 As New DataTable
        dstcart1 = Session("Cart")

        If dstcart1 Is Nothing Then
            Exit Sub
        End If

        Dim tblCartAP As New DataTable
        tblCartAP = dstcart1.Clone()

        For I = 0 To rgCart.Items.Count - 1
            Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
            Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)

            Dim lblItemID As Label = CType(rgCart.Items.Item(I).FindControl("lblItemID"), Label)
            Dim ProductID As String = lblItemID.Text
            Dim iRow As Integer
            If FindCartDTRow(dstcart1, ProductID, "", iRow) Then
                Dim row As DataRow = dstcart1.Rows(iRow)
                If CType(nestedview.FindControl("chkDelete"), CheckBox).Checked = True Or
                    CDec(CType(nestedview.FindControl("txtQTY"), TextBox).Text) = 0 Then
                    row.Delete()
                Else

                    Dim decPrice As Decimal = 0
                    Try
                        decPrice = CDec(CType(nestedview.FindControl("Price"), Label).Text)
                    Catch ex As Exception
                        decPrice = 0
                    End Try

                    row.Item("Quantity") = System.Convert.ToDecimal(CType(nestedview.FindControl("txtQTY"), TextBox).Text)
                    row.Item("ItemChgCode") = CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text
                    row.Item("EmpChgCode") = CType(nestedview.FindControl("txtEmpChgCode"), TextBox).Text
                    row.Item("ItemDescription") = CType(GridItem.FindControl("txtItemDescc"), TextBox).Text
                    row.Item("MachineRow") = CType(nestedview.FindControl("txtMachineRow"), TextBox).Text
                    'row.Item("LPP") = CType(nestedview.FindControl("txtLPP"), TextBox).Text
                    row.Item("PO") = CType(nestedview.FindControl("txtPO"), TextBox).Text
                    row.Item("LN") = CType(nestedview.FindControl("txtLN"), TextBox).Text
                    row.Item("SerialID") = CType(nestedview.FindControl("lblSerialID"), Label).Text
                    If CType(nestedview.FindControl("chbTaxFlag"), CheckBox) Is Nothing Then
                        row.Item("TaxFlag") = " "
                    ElseIf CType(nestedview.FindControl("chbTaxFlag"), CheckBox).Checked = True Then
                        row.Item("TaxFlag") = "Y"
                    Else
                        row.Item("TaxFlag") = " "
                    End If
                    ' New code 
                    Dim chkPriority As CheckBox = CType(nestedview.FindControl("chkPriority"), CheckBox)
                    row.Item("UPriority") = " "  '  CType(nestedview.FindControl("chkPriority"), CheckBox).Checked
                    If chkPriority.Checked Then
                        row.Item("UPriority") = "R" '1
                    End If
                    Dim RDPDueDate As RadDatePicker = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker)
                    row.Item("UDueDate") = " "
                    If Not RDPDueDate.SelectedDate Is Nothing Then
                        row.Item("UDueDate") = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker).SelectedDate
                    End If

                    If dropType.Visible Then
                        row.Item("UType") = UCase(dropType.SelectedValue)
                    Else
                        row.Item("UType") = " "
                    End If
                    'row.Item("UType") = UCase(txtType.Text)

                    row.Item("UNotes") = CType(nestedview.FindControl("txtNotes"), TextBox).Text
                    Dim chkAddToCtlg As CheckBox = CType(nestedview.FindControl("chkAddToCtlg"), CheckBox)
                    row.Item("AddToCtlgFlag") = " "
                    If chkAddToCtlg.Checked Then
                        row.Item("AddToCtlgFlag") = "1"
                    End If
                    '' Machine Number 
                    Dim ddlMachineNo As DropDownList = CType(nestedview.FindControl("ddlMachineNo"), DropDownList)
                    Dim txtMachineNo As TextBox = CType(nestedview.FindControl("txtMachineNo"), TextBox)
                    row.Item("MachineNo") = " "
                    If ddlMachineNo.Visible Then
                        If Not ddlMachineNo.SelectedValue Is Nothing Then
                            row.Item("MachineNo") = ddlMachineNo.SelectedValue
                        End If
                    End If
                    If txtMachineNo.Visible Then
                        If Not txtMachineNo.Text Is Nothing Then
                            row.Item("MachineNo") = txtMachineNo.Text
                        End If
                    End If

                    'code to check is it NONCAT item
                    Dim bIsNoncat As Boolean = False
                    Dim sItemID As String = ProductID
                    If sItemID <> "" Then
                        If sItemID.Length > 5 Then
                            If UCase(Microsoft.VisualBasic.Left(sItemID, 6)) = "NONCAT" Then
                                bIsNoncat = True
                            End If
                        End If
                    Else
                        ' is it really possible? If yes what to do?
                    End If  '  sItemID <> ""
                    'If (decPrice = 0) And (Not bIsNoncat) Then
                    '    row.Item("LN") = iLnNumber.ToString()
                    '    tblCartAP.ImportRow(row)
                    '    iLnNumber += 1
                    'End If
                    If decPrice > 0 Then
                        row.Item("LN") = iLnNumber.ToString()
                        tblCartAP.ImportRow(row)
                        iLnNumber += 1
                    End If

                End If
            End If
        Next
        Dim i1 As Integer = tblCartAP.Rows.Count
        If i1 > 0 Then
            Session("CartAscendPriced") = tblCartAP
        Else
            Session("CartAscendPriced") = Nothing
        End If
    End Sub
#End Region

#Region "Public Events"
    Private Sub FavsMenu_ItemSelected(ByVal sender As Object,
        ByVal e As Telerik.Web.UI.RadMenuEventArgs) Handles FavsMenu.ItemClick

        Dim strMessage As New Alert
        Dim strInvItemID As String
        txtItemID.Text = e.Item.Value
        If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
            strInvItemID = txtItemID.Text
        ElseIf txtItemID.Text.Substring(0, 3) = Session("SITEPREFIX") Then
            strInvItemID = txtItemID.Text
        Else
            strInvItemID = Session("SITEPREFIX") & txtItemID.Text
        End If
        If Session("MobIssuing") = "Y" Then
            Dim objclsInvItemID As New clsInvItemID(strInvItemID)
            If objclsInvItemID.InvStockType = "STK" Then
                ltlAlert.Text = strMessage.Say("Stock Items cannot be added to cart.\nPlease use the Mobile Issuing\nprogram to request stock.")
                Exit Sub
            End If
        End If
        btnQucikItem_Click(sender, e)
    End Sub

#End Region

#Region "Public Methods"

    'Public Function GetExtPrice(ByVal price, ByVal qty) As Decimal
    '    If IsDBNull(price) Then
    '        GetExtPrice = 0.0
    '    Else
    '        GetExtPrice = Convert.ToDecimal(price) * Convert.ToDecimal(qty)
    '    End If

    'End Function
    Public Function GetExtPrice(ByVal price As String, ByVal qty As String) As String
        Dim s As String = "0.00"
        Try
            If IsNumeric(price) And IsNumeric(qty) Then
                s = Math.Round((CDec(price) * CDec(qty)), 2).ToString("####,###,##0.00")
            End If
        Catch ex As Exception
        End Try
        Return (s)
    End Function

    Public Function getName(ByVal strUserid) As String

        If IsDBNull(strUserid) Then
            Exit Function
        ElseIf Trim(strUserid) = "" Then
            Exit Function
        End If
        getName = GetEmpName(strBU, strUserid)

    End Function

    'Public Function GetLPP(ByVal price) As String

    '    Dim ret As String = ""
    '    'If IsDBNull(price) Then
    '    '    GetLPP = ""
    '    'ElseIf Trim(price) = "" Then
    '    '    GetLPP = ""
    '    'ElseIf Convert.ToDecimal(price) = 0.0 Then
    '    '    GetLPP = ""
    '    'Else
    '    '    GetLPP = FormatNumber(price, 2)
    '    'End If
    '    Try
    '        If Not (price Is Nothing) Then
    '            If Not IsDBNull(price) Then
    '                If IsNumeric(price) Then
    '                    ret = FormatNumber(price, 2)
    '                End If
    '            End If
    '        End If
    '    Catch ex As Exception
    '    End Try
    '    Return ret

    'End Function

    'Public Function GetPrice(ByVal price) As String
    '    If IsDBNull(price) Then
    '        GetPrice = "Price on Request"
    '    ElseIf Convert.ToDecimal(price) = 0.0 Then
    '        GetPrice = "Price on Request"
    '    Else
    '        GetPrice = FormatNumber(price, 2)
    '        lblPriceVary.Visible = True
    '    End If
    'End Function


    Public Function GetEstimatedPrice(ByVal itemID As String) As String
        Dim s As String = "0.00"
        Try
            Dim dstcart1 As New DataTable
            dstcart1 = Session("Cart")
            Dim price As String = dstcart1.AsEnumerable().
   Where(Function(r) Convert.ToString(r.Field(Of String)("itemid")) = Convert.ToString(itemID)).
   Select(Function(r) Convert.ToString(r.Field(Of String)("PricePo"))).FirstOrDefault()
            If IsDBNull(price) Then
                s = "Price on Request"
            Else
                If IsNumeric(price) Then
                    If CDec(price) = 0 Then
                        s = "Price on Request"
                    Else
                        s = Math.Round(CDec(price), 2).ToString("####,###,##0.00")
                        'lblPriceVary.Visible = True
                        'divPriceVary.Visible = True
                    End If
                End If
            End If
        Catch ex As Exception
        End Try
        Return (s)
    End Function

    Public Function GetEstimatedExtPrice(ByVal itemID As String, ByVal qty As String) As String
        Dim s As String = "0.00"
        Try
            Dim dstcart1 As New DataTable
            dstcart1 = Session("Cart")

            Dim pricepo As String = dstcart1.AsEnumerable().
   Where(Function(r) Convert.ToString(r.Field(Of String)("itemid")) = Convert.ToString(itemID)).
   Select(Function(r) Convert.ToString(r.Field(Of String)("PricePo"))).FirstOrDefault()

            If pricepo = "" Then
                s = "Price on Request"
            ElseIf pricepo = "0" Then
                s = "0.00"
            Else
                If IsNumeric(pricepo) And IsNumeric(qty) Then
                    s = Math.Round((CDec(pricepo) * CDec(qty)), 2).ToString("####,###,##0.00")
                End If

            End If

        Catch ex As Exception
        End Try
        Return (s)
    End Function

    Public Function GetPrice(ByVal price As String) As String
        Dim s As String = "0.00"
        Try
            If IsDBNull(price) Then
                s = "Price on Request"
            Else
                If IsNumeric(price) Then
                    If CDec(price) = 0 Then
                        s = "Price on Request"
                    Else
                        s = Math.Round(CDec(price), 2).ToString("####,###,##0.00")
                        'lblPriceVary.Visible = True
                        'divPriceVary.Visible = True
                    End If
                End If
            End If
        Catch ex As Exception
        End Try
        Return (s)
    End Function

    Public Sub OnCheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If Not Session("Cart") Is Nothing Then
            updateCartDatatable()
        End If
    End Sub

    Public Sub OnMachineNoTextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If Not Session("Cart") Is Nothing Then
            updateCartDatatable()
        End If
    End Sub

    'Public Sub OnTextChangedLPP(ByVal sender As System.Object, ByVal e As System.EventArgs)

    '    Dim LPP As TextBox
    '    LPP = sender
    '    If Not Session("Cart") Is Nothing Then
    '        updateCartDatatable()
    '        '  updateCartDatatable1()
    '    End If
    '    If Not Trim(LPP.Text) = "" Then
    '        LPP.Text = FormatNumber(LPP.Text, 2)
    '    End If
    'End Sub

    Public Sub OnTextChangedPO(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim PO As TextBox
        PO = sender
        If Not Session("Cart") Is Nothing Then
            updateCartDatatable()
            ' updateCartDatatable1()
        End If

    End Sub

    Public Sub OnTextChangedLN(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim LN As TextBox
        LN = sender
        If Not Session("Cart") Is Nothing Then
            updateCartDatatable()
            '    updateCartDatatable1()
        End If

    End Sub

    <Services.WebMethod()>
    Public Shared Function UpdateQtyValue(ByVal ItemID As String, ByVal Qty As String, ByVal LineNum As String) As String
        Dim fldToUpdate As String = ""
        Dim newValue As String = ""
        Try
            If HttpContext.Current.Session("BUSUNIT") = "" Then
                HttpContext.Current.Session.RemoveAll()
                HttpContext.Current.Response.Redirect("default.aspx")
            End If
            Dim TotalPrice As Decimal = 0.0
            Dim ItmPrice As Decimal
            Dim ItmQty As Integer
            Dim Price As Decimal
            If Not HttpContext.Current.Session("Cart") Is Nothing Then
                ' Dim obj As shoppingcart = New shoppingcart()
                'obj.updateCartDatatable()
                Dim dt As DataTable = CType(HttpContext.Current.Session("Cart"), DataTable)
                For Each row As DataRow In dt.Rows
                    If row("ItemId") = ItemID And row("UniqNum") = LineNum Then
                        row("Quantity") = Qty
                        dt.AcceptChanges()
                        fldToUpdate = "QUANTITY"
                        newValue = Qty
                    End If
                    ItmQty = Convert.ToDecimal(row("Quantity"))
                    ItmPrice = Convert.ToDecimal(row("Price"))
                    Price = ItmQty * ItmPrice
                    TotalPrice = TotalPrice + Price
                Next
                HttpContext.Current.Session("Cart") = dt
                'HttpContext.Current.Session("Cart") = dt
                ' obj.txtOrderTot.Text = Math.Round(obj.getOrderTot(), 2).ToString("####,###,##0.00")
                If fldToUpdate = "QUANTITY" Then 'make sure there is a field to update
                    UpdateUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"), ItemID, fldToUpdate, newValue, LineNum)
                End If
            End If
            'WriteToUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"))
            Return TotalPrice
        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart.aspx.vb > UpdateQtyValue function. User ID: " & HttpContext.Current.Session("USERID") & "; " & vbCrLf &
              "BU: " & HttpContext.Current.Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID") & " :: " & HttpContext.Current.Request.ServerVariables("REMOTE_HOST") & vbCrLf &
               "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart.aspx.vb > UpdateQtyValue function.")
        End Try

    End Function

    <Services.WebMethod()>
    Public Shared Function WriteToCartTable(ByVal ItemID As String, ByVal Priority As Boolean, ByVal Field As String, ByVal LineNum As String)
        Dim fldToUpdate As String = ""
        Dim newValue As String = ""
        Try
            If HttpContext.Current.Session("BUSUNIT") = "" Then
                HttpContext.Current.Session.RemoveAll()
                HttpContext.Current.Response.Redirect("default.aspx")
            End If

            If Not HttpContext.Current.Session("Cart") Is Nothing Then
                'Dim obj As shoppingcart = New shoppingcart()
                'obj.updateCartDatatable()
                Dim dt As DataTable = CType(HttpContext.Current.Session("Cart"), DataTable)
                'WI: 859: Added for persistant Shopping Cart - Temp

                If Field.ToUpper().Equals("PRIORITY") Then
                    'Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                    Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("UniqNum") = LineNum And r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                    dr("UPriority") = IIf(Priority = True, "R", " ")
                    dt.AcceptChanges()
                    fldToUpdate = "PRIORITY"
                    newValue = IIf(Priority = True, "R", " ")

                    'For Each row As DataRow In dt.Rows
                    '    If row("ItemId") = ItemID Then
                    '        If Priority Then
                    '            row("UPriority") = "R" '1
                    '        Else
                    '            row("UPriority") = " "
                    '        End If
                    '        dt.AcceptChanges()
                    '    End If
                    'Next
                ElseIf Field.ToUpper().Equals("TAXEXEMPT") Then
                    'Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                    Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("UniqNum") = LineNum And r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                    'Dim drRow() As DataRow = dt.Select("itemID = '" + ItemID + "', lineNum = '" & LineNum & "'")
                    dr("TaxFlag") = IIf(Priority = True, "Y", "0")
                    dt.AcceptChanges()
                    fldToUpdate = "TAX_FLAG"
                    newValue = IIf(Priority = True, "Y", "0")


                    'For Each row As DataRow In dt.Rows
                    '    If row("ItemId") = ItemID Then
                    '        If Priority Then
                    '            row("TaxFlag") = "Y"
                    '        Else
                    '            row("TaxFlag") = "0"
                    '        End If
                    '        dt.AcceptChanges()
                    '    End If
                    'Next

                End If
                HttpContext.Current.Session("Cart") = dt
                UpdateUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"), ItemID, fldToUpdate, newValue, LineNum)

            End If

        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart.aspx.vb > WriteToCartTable function. User ID: " & HttpContext.Current.Session("USERID") & "; " & vbCrLf &
              "BU: " & HttpContext.Current.Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID") & " :: " & HttpContext.Current.Request.ServerVariables("REMOTE_HOST") & vbCrLf &
               "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart.aspx.vb > WriteToCartTable function.")
        End Try

    End Function
    <Services.WebMethod()>
    Public Shared Function UpdateCatalog(ByVal ItemID As String, ByVal Priority As Boolean, ByVal LineNum As String)
        Dim fldToUpdate As String = ""
        Dim newValue As String = ""
        Try
            If HttpContext.Current.Session("BUSUNIT") = "" Then
                HttpContext.Current.Session.RemoveAll()
                HttpContext.Current.Response.Redirect("default.aspx")
            End If

            If Not HttpContext.Current.Session("Cart") Is Nothing Then

                Dim dt As DataTable = CType(HttpContext.Current.Session("Cart"), DataTable)

                'Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("UniqNum") = LineNum And r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                dr("AddToCtlgFlag") = IIf(Priority = True, "1", "0")
                dt.AcceptChanges()
                fldToUpdate = "ADD_TO_CTLG"
                newValue = IIf(Priority = True, "1", "0")

                'For Each row As DataRow In dt.Rows
                '    If row("ItemId") = ItemID Then
                '        If Priority Then
                '            row("AddToCtlgFlag") = "1"
                '        Else
                '            row("AddToCtlgFlag") = "0"
                '        End If
                '        dt.AcceptChanges()
                '    End If
                'Next
                HttpContext.Current.Session("Cart") = dt
                UpdateUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"), ItemID, fldToUpdate, newValue, LineNum)
            End If
            'WriteToUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"))
        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart.aspx.vb > UpdateCatalog function. User ID: " & HttpContext.Current.Session("USERID") & "; " & vbCrLf &
                "BU: " & HttpContext.Current.Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID") & " :: " & HttpContext.Current.Request.ServerVariables("REMOTE_HOST") & vbCrLf &
               "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart.aspx.vb > UpdateCatalog function.")
        End Try

    End Function

    <Services.WebMethod()>
    Public Shared Function UpdateDueDate(ByVal ItemID As String, ByVal DatePicker As String, ByVal LineNum As String)
        Dim fldToUpdate As String = ""
        Dim newValue As String = ""
        Try
            If HttpContext.Current.Session("BUSUNIT") = "" Then
                HttpContext.Current.Session.RemoveAll()
                HttpContext.Current.Response.Redirect("default.aspx")
            End If

            If Not HttpContext.Current.Session("Cart") Is Nothing Then

                If Trim(DatePicker) = "" Then DatePicker = " "

                Dim dt As DataTable = CType(HttpContext.Current.Session("Cart"), DataTable)

                'Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("UniqNum") = LineNum And r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()

                dr("UDueDate") = DatePicker
                dt.AcceptChanges()
                fldToUpdate = "DUE_DATE"
                newValue = DatePicker

                'For Each row As DataRow In dt.Rows
                '    If row("ItemId") = ItemID Then
                '        row("UDueDate") = DatePicker
                '        'If Priority Then
                '        '    row("UPriority") = "1"
                '        'Else
                '        '    row("UPriority") = "0"
                '        'End If
                '        dt.AcceptChanges()
                '    End If
                'Next
                HttpContext.Current.Session("Cart") = dt
                UpdateUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"), ItemID, fldToUpdate, newValue, LineNum)
            End If
            'WriteToUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"))

        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart.aspx.vb > UpdateDueDate function. User ID: " & HttpContext.Current.Session("USERID") & "; " & vbCrLf &
              "BU: " & HttpContext.Current.Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID") & " :: " & HttpContext.Current.Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart.aspx.vb > UpdateDueDate function.")
        End Try

    End Function

    '<Services.WebMethod()> _
    'Public Shared Function UpdateLPP(ByVal ItemID As String, ByVal LPP As String)
    '    If HttpContext.Current.Session("BUSUNIT") = "" Then
    '        HttpContext.Current.Session.RemoveAll()
    '        HttpContext.Current.Response.Redirect("default.aspx")
    '    End If

    '    If Not HttpContext.Current.Session("Cart") Is Nothing Then
    '        Dim dt As DataTable = CType(HttpContext.Current.Session("Cart"), DataTable)
    '        For Each row As DataRow In dt.Rows
    '            If row("ItemId") = ItemID Then
    '                row("LPP") = LPP
    '                dt.AcceptChanges()
    '            End If
    '        Next
    '        HttpContext.Current.Session("Cart") = dt
    '    End If
    '    WriteToUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"))
    'End Function

    <Services.WebMethod()>
    Public Shared Function UpdateMexicanFields(ByVal ItemID As String, ByVal FieldValue As String, ByVal Field As String, ByVal LineNum As String)
        Dim fldToUpdate As String = ""
        Dim newValue As String = ""
        Try
            If HttpContext.Current.Session("BUSUNIT") = "" Then
                HttpContext.Current.Session.RemoveAll()
                HttpContext.Current.Response.Redirect("default.aspx")
            End If

            If Not HttpContext.Current.Session("Cart") Is Nothing Then
                Dim dt As DataTable = CType(HttpContext.Current.Session("Cart"), DataTable)
                'WI: 859: Added for persistant Shopping Cart - Temp
                If Field.ToUpper().Equals("PO") Then

                    'Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                    Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("UniqNum") = LineNum And r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                    dr("PO") = FieldValue
                    dt.AcceptChanges()
                    fldToUpdate = "PO_ID"
                    newValue = FieldValue

                    'For Each row As DataRow In dt.Rows
                    '    If row("ItemId") = ItemID Then
                    '        row("PO") = FieldValue
                    '        'dt.AcceptChanges()
                    '    End If
                    'Next
                    'dt.AcceptChanges()
                ElseIf Field.ToUpper().Equals("LN") Then

                    'Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                    Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("UniqNum") = LineNum And r.Field(Of String)("Itemid") = ItemID).FirstOrDefault()
                    dr("LN") = FieldValue
                    dt.AcceptChanges()
                    fldToUpdate = "PO_LINE"
                    newValue = FieldValue

                    'For Each row As DataRow In dt.Rows
                    '    If row("ItemId") = ItemID Then
                    '        row("LN") = FieldValue
                    '        'dt.AcceptChanges()
                    '    End If
                    'Next
                    'dt.AcceptChanges()
                End If
                HttpContext.Current.Session("Cart") = dt
                UpdateUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"), ItemID, fldToUpdate, newValue, LineNum)
            End If
            'WriteToUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"))

        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart.aspx.vb > UpdateMexicanFields function. User ID: " & HttpContext.Current.Session("USERID") & "; " & vbCrLf &
              "BU: " & HttpContext.Current.Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID") & " :: " & HttpContext.Current.Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart.aspx.vb > UpdateMexicanFields function.")
        End Try

    End Function

#End Region


    Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.PreRender
        Try
            Me.lblSiteBaseCurrencySymbol.Text = ""
            Me.lblSiteBaseCurrencyCode.Text = ""
            If Me.txtOrderTot.Visible Then
                Me.lblSiteBaseCurrencySymbol.Text = m_siteCurrency.Symbol
                Me.lblSiteBaseCurrencyCode.Text = m_siteCurrency.Id
            End If
        Catch ex As Exception
        End Try
    End Sub

    Public Sub btnPickSerialID_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Customvalidator1.Enabled = False
        Customvalidator2.Enabled = False
        CustomValidator3.Enabled = False
        Customvalidator4.Enabled = False
        CompareValidator2.Enabled = False
        RequiredFieldValidator1.Enabled = False
        'Requiredfieldvalidator2.Enabled = False
        Requiredfieldvalidator3.Enabled = False
        RequiredFieldValidator4.Enabled = False
        Requiredfieldvalidator6.Enabled = False
        CustomValidatorWorkOrder.Enabled = False
        CustomValidatorWOCH.Enabled = False

        Dim intIndex As Integer

        'Dim tableCell As TableCell = CType(CType(sender.parent, Object), System.Web.UI.WebControls.TableCell)
        'Dim item As DataGridItem = CType(CType(tableCell.BindingContainer, System.Web.UI.Control), DataGridItem)
        Dim Nesteditem As GridNestedViewItem = DirectCast(sender, System.Web.UI.WebControls.Button).NamingContainer
        Dim item As GridDataItem = TryCast(DirectCast(Nesteditem, GridNestedViewItem).ParentItem, GridDataItem)
        intIndex = item.ItemIndex()

        Session("SerialIDRowIndex") = intIndex

        Dim strProductID As String = CType(item.FindControl("lblItemID"), Label).Text

        'Dim strScript As String = ""
        'strScript = "<script>"
        'strScript = strScript & "var args = {'callback': SubmitSerialSelection}; var strReturn; " & _
        '"strReturn=window.showModalDialog('SerialNumSelection.aspx?prodid=" & strProductID.ToUpper & _
        '"&selecttype=select&showonlyavailable=Y',args,'status:no;dialogWidth:720px;dialogHeight:600px;dialogHide:true;help:no;scroll:yes');"
        'strScript = strScript & "document.forms[0].submit();"
        'strScript = strScript & "</script>"
        'Page.RegisterStartupScript("ClientScript", strScript)

        ''new RadWindow code  
        Dim tmp As String = ""
        Dim script As String = ""
        'tmp = "http://" & Request.ServerVariables("HTTP_HOST") & GetWebAppName1()
        'tmp += "/SerialNumSelection.aspx?prodid=" & strProductID.ToUpper & "&selecttype=select&showonlyavailable=Y&FROMRAD=RAD"

        tmp = "SerialNumSelection.aspx?prodid=" & strProductID.ToUpper & "&selecttype=select&showonlyavailable=Y&FROMRAD=RAD"

        RadWindowChCd.NavigateUrl = tmp
        RadWindowChCd.OnClientClose = "SubmitSerialOnClientClose"
        RadWindowChCd.Title = "Serial Number Select"
        script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

    End Sub

    Private Sub txtSelectedSerialID_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtSelectedSerialID.TextChanged
        '   Dim item As DataGridItem = TryCast(rgCart.Items(CType(Session("SerialIDRowIndex"), Integer)), DataGridItem)
        Dim item As GridDataItem = DirectCast(rgCart.Items.Item(CType(Session("SerialIDRowIndex"), Integer)), GridDataItem)
        Dim NestedItem As GridNestedViewItem = DirectCast(item.ChildItem, GridNestedViewItem)
        Dim lblSerialID As Label = CType(NestedItem.FindControl("lblSerialID"), Label)

        lblSerialID.Text = txtSelectedSerialID.Text

        If Not Session("Cart") Is Nothing Then
            updateCartDatatable()
        End If
    End Sub

    Private Function UpdateLineItemsForCart(ByVal updateField As String)
        Dim fldToUpdate As String = ""
        Dim newValue As String = ""
        Try
            Dim itemID As String = hdnLNItemID.Value
            Dim linenum As String = hdnUniqNum.Value
            If Not itemID Is Nothing Then
                Dim dt As DataTable = CType(HttpContext.Current.Session("Cart"), DataTable)
                Dim drRow() As DataRow = dt.Select("itemID = '" + itemID + "' and UniqNum = '" + linenum + "'")
                If Not drRow Is Nothing And drRow.Length > 0 Then
                    If updateField.ToUpper().Equals("ITEMCHGCODE") Then
                        drRow(0)("ItemChgCode") = txtItemChgCodeHide.Text
                        fldToUpdate = "ITEM_CHG_CODE"
                        newValue = txtItemChgCodeHide.Text
                    ElseIf updateField.ToUpper().Equals("EMPCHGCODE") Then
                        drRow(0)("EmpChgCode") = txtEmpChgCodeHide.Text
                        fldToUpdate = "EMP_CHG_CODE"
                        newValue = txtEmpChgCodeHide.Text
                    End If
                    dt.AcceptChanges()
                    'WriteToUserCart(Session("USERID"), Session("BUSUNIT"))
                    UpdateUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"), itemID, fldToUpdate, newValue, linenum)
                End If
            End If
        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart.aspx.vb > UpdateLineItemsForCart function. User ID: " & HttpContext.Current.Session("USERID") & "; " & vbCrLf &
                "BU: " & HttpContext.Current.Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID") & " :: " & HttpContext.Current.Request.ServerVariables("REMOTE_HOST") & vbCrLf &
                "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart.aspx.vb > UpdateLineItemsForCart function.")

        End Try

    End Function

    Private Function PopulateGridFromHiddenControls() As Boolean
        Dim bChangedContents As Boolean = False
        Dim iItem As Integer

        If txtItemChgCodeHide.Text <> "" And txtItemChgCodeItem.Text <> "" Then
            iItem = Convert.ToInt32(txtItemChgCodeItem.Text)
            'RadGrid Code
            Dim GridItem As GridDataItem = CType(rgCart.Items.Item(iItem), GridDataItem)
            Dim nestedview As GridNestedViewItem = CType(GridItem.ChildItem, GridNestedViewItem)
            CType(nestedview.FindControl("txtItemChgCode"), TextBox).Text = txtItemChgCodeHide.Text
            bChangedContents = True
            'WI: 859: Added for persistant Shopping Cart - Temp
            UpdateLineItemsForCart("ITEMCHGCODE")
        End If

        If txtEmpChgCodeHide.Text <> "" And txtEmpChgCodeItem.Text <> "" Then
            iItem = Convert.ToInt32(txtEmpChgCodeItem.Text)
            'RadGrid Code
            Dim GridItem As GridDataItem = CType(rgCart.Items.Item(iItem), GridDataItem)
            Dim nestedview As GridNestedViewItem = CType(GridItem.ChildItem, GridNestedViewItem)
            CType(nestedview.FindControl("txtEmpChgCode"), TextBox).Text =
                     txtEmpChgCodeHide.Text
            bChangedContents = True
            'WI: 859: Added for persistant Shopping Cart - Temp
            UpdateLineItemsForCart("EMPCHGCODE")
        End If

        If txtMachineRowHide.Text <> "" And txtMachineRowItem.Text <> "" Then
            iItem = Convert.ToInt32(txtMachineRowItem.Text)
            'RadGrid Code
            Dim GridItem As GridDataItem = CType(rgCart.Items.Item(iItem), GridDataItem)
            Dim nestedview As GridNestedViewItem = CType(GridItem.ChildItem, GridNestedViewItem)
            CType(nestedview.FindControl("txtMachineRow"), TextBox).Text =
                     txtMachineRowHide.Text
            bChangedContents = True
        End If

        Return bChangedContents
    End Function

    Private Function IsAscendCustID(ByVal strCustID As String) As Boolean
        Dim bIsAscend As Boolean = False
        If strCustID = "90523" Or
            strCustID = "90524" Or
            strCustID = "90525" Or
            strCustID = "90526" Or
            strCustID = "90527" Then

            bIsAscend = True
        End If
        Return bIsAscend
    End Function

    Private Sub KillPrimaryKey(ByVal dt As DataTable)
        Dim intLocPriKeyCount As Integer = dt.PrimaryKey.Length
        Dim strPrevPriCols(intLocPriKeyCount) As String

        ' store ColumnNames in a string array
        For i As Integer = 0 To intLocPriKeyCount - 1
            strPrevPriCols(i) = dt.PrimaryKey(i).ColumnName
        Next

        ' clear PrimaryKey
        dt.PrimaryKey = Nothing

        ' clear unique settings
        For i As Integer = 0 To intLocPriKeyCount - 1
            dt.Columns(strPrevPriCols(i)).Unique = False
        Next
    End Sub

    'Private Function FindCartDTRow(ByVal dt As DataTable, ByVal strTargetItemID As String, ByVal strTarget2UniqNum As String, ByRef iRow As Integer) As Boolean
    '    Dim bFoundRow As Boolean = False
    '    Dim i As Integer
    '    Dim iRowsWithTargetItemID() As Integer
    '    Dim bRowAlreadyBound() As Boolean

    '    If Not dt Is Nothing Then
    '        If dt.Rows.Count > 0 Then
    '            ' Collect all the row indexes that match the target item ID. There will be more than one
    '            ' match to the target item ID only for serialized items.
    '            iRowsWithTargetItemID = FindCartItemIDRows(dt, strTargetItemID, bRowAlreadyBound)

    '            If Not iRowsWithTargetItemID Is Nothing Then ' At least one matching row.
    '                If iRowsWithTargetItemID.Length = 1 Then
    '                    ' We found only one match so this is the row we want.
    '                    iRow = iRowsWithTargetItemID(0)
    '                    bFoundRow = True
    '                ElseIf iRowsWithTargetItemID.Length > 1 Then
    '                    ' We found more than one matching cart row. There is more than one match only if the item
    '                    ' is serialized. In this case, use the unique number stored in the grid in a hidden
    '                    ' column to determine the correct row.

    '                    If strTarget2UniqNum.Trim() <> "" Then
    '                        ' UniqNum is not blank if the grid has been binded. In this case, just do a simple lookup
    '                        ' in iFoundRows in cart datatable.
    '                        If FindCartUniqNum(dt, strTarget2UniqNum, iRowsWithTargetItemID, iRow) Then
    '                            bFoundRow = True
    '                        End If
    '                    Else
    '                        ' UniqNum is blank. This occurs if the grid is being rebinded and this serialized item ID
    '                        ' has not been binded yet. In this case, look at each grid row and use the first one
    '                        ' that doesn't yet have a unique num stored for it.
    '                        If dtgCart.Items.Count > 0 Then
    '                            i = 0
    '                            While i < dtgCart.Items.Count And Not bFoundRow
    '                                ' Loop through the grid to find matches for the target item ID.
    '                                ' Since we know there are multiple matches to the target item ID, the ultimate
    '                                ' goal of this loop is to filter out the cart rows for the target item ID that
    '                                ' have already been binded and to get the next cart row for the target item ID 
    '                                ' that hasn't been binded.
    '                                Dim strItemID As String = CType(dtgCart.Items(i).FindControl("lblItemID"), Label).Text
    '                                If strItemID = strTargetItemID Then
    '                                    ' For each grid row with a matching target item ID, check if the uniq num 
    '                                    ' matches the blank target uniq num, i.e., check if the grid row has a blank
    '                                    ' uniq num (which it shouldn't).
    '                                    Dim strGridUniqNum As String = CType(dtgCart.Items(i).FindControl("lblUniqNum"), Label).Text
    '                                    ' There should not be a grid row with a blank uniq num but we'll test anyway.
    '                                    ' If there is a match to the uniq num, we'll use that row.
    '                                    If strGridUniqNum.Trim() = strTarget2UniqNum Then
    '                                        iRow = i
    '                                        bFoundRow = True
    '                                    Else
    '                                        ' At this point, we found a grid row with a uniq num that is not blank. This
    '                                        ' means the corresponding cart row was bound to the grid. The next loop finds
    '                                        ' this bound grid row in the array of cart rows that match the target item ID then
    '                                        ' marks that cart row as having been bound.
    '                                        Dim j As Integer = 0
    '                                        Dim bMarkedAsBound As Boolean = False
    '                                        While j < iRowsWithTargetItemID.Length And Not bMarkedAsBound
    '                                            If iRowsWithTargetItemID(j).ToString() = i Then
    '                                                bRowAlreadyBound(j) = True
    '                                                bMarkedAsBound = True
    '                                            End If
    '                                            j = j + 1
    '                                        End While
    '                                    End If
    '                                End If
    '                                i = i + 1
    '                            End While
    '                            ' At this point, if we still haven't found the corresponding cart row, check
    '                            ' through the array of candidate rows (those whose item ID matches the target item ID)
    '                            ' and pick the first one that hasn't already been bound to the grid.
    '                            If Not bFoundRow Then
    '                                i = 0
    '                                While i < bRowAlreadyBound.Length And Not bFoundRow
    '                                    If Not bRowAlreadyBound(i) Then
    '                                        iRow = iRowsWithTargetItemID(i)
    '                                        bFoundRow = True
    '                                    End If
    '                                    i = i + 1
    '                                End While
    '                            End If
    '                        Else
    '                            ' If the grid has had no items bound yet, just use the first row of the cart DT
    '                            ' which matches the target item ID.
    '                            iRow = iRowsWithTargetItemID(0)
    '                            bFoundRow = True
    '                        End If
    '                    End If
    '                End If
    '            End If
    '        End If
    '    End If

    '    Return bFoundRow

    'End Function

    Private Function FindCartDTRow(ByVal dt As DataTable, ByVal strTargetItemID As String, ByVal strTarget2UniqNum As String, ByRef iRow As Integer) As Boolean
        Dim bFoundRow As Boolean = False
        Dim i As Integer
        Dim iRowsWithTargetItemID() As Integer
        Dim bRowAlreadyBound() As Boolean

        If Not dt Is Nothing Then
            If dt.Rows.Count > 0 Then
                ' Collect all the row indexes that match the target item ID. There will be more than one
                ' match to the target item ID only for serialized items.
                iRowsWithTargetItemID = FindCartItemIDRows(dt, strTargetItemID, bRowAlreadyBound)

                If Not iRowsWithTargetItemID Is Nothing Then ' At least one matching row.
                    If iRowsWithTargetItemID.Length = 1 Then
                        ' We found only one match so this is the row we want.
                        iRow = iRowsWithTargetItemID(0)
                        bFoundRow = True
                    ElseIf iRowsWithTargetItemID.Length > 1 Then
                        ' We found more than one matching cart row. There is more than one match only if the item
                        ' is serialized. In this case, use the unique number stored in the grid in a hidden
                        ' column to determine the correct row.

                        If strTarget2UniqNum.Trim() <> "" Then
                            ' UniqNum is not blank if the grid has been binded. In this case, just do a simple lookup
                            ' in iFoundRows in cart datatable.
                            If FindCartUniqNum(dt, strTarget2UniqNum, iRowsWithTargetItemID, iRow) Then
                                bFoundRow = True
                            End If
                        Else
                            ' UniqNum is blank. This occurs if the grid is being rebinded and this serialized item ID
                            ' has not been binded yet. In this case, look at each grid row and use the first one
                            ' that doesn't yet have a unique num stored for it.
                            If rgCart.Items.Count > 0 Then
                                i = 0
                                While i < rgCart.Items.Count And Not bFoundRow
                                    ' Loop through the grid to find matches for the target item ID.
                                    ' Since we know there are multiple matches to the target item ID, the ultimate
                                    ' goal of this loop is to filter out the cart rows for the target item ID that
                                    ' have already been binded and to get the next cart row for the target item ID 
                                    ' that hasn't been binded.
                                    Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(i), GridDataItem)
                                    Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)

                                    Dim strItemID As String = CType(rgCart.Items(i).FindControl("lblItemID"), Label).Text
                                    If strItemID = strTargetItemID Then
                                        ' For each grid row with a matching target item ID, check if the uniq num 
                                        ' matches the blank target uniq num, i.e., check if the grid row has a blank
                                        ' uniq num (which it shouldn't).
                                        Dim strGridUniqNum As String = CType(nestedview.FindControl("hdfUniqNum"), HiddenField).Value
                                        ' There should not be a grid row with a blank uniq num but we'll test anyway.
                                        ' If there is a match to the uniq num, we'll use that row.
                                        If strGridUniqNum.Trim() = strTarget2UniqNum Then
                                            iRow = i
                                            bFoundRow = True
                                        Else
                                            ' At this point, we found a grid row with a uniq num that is not blank. This
                                            ' means the corresponding cart row was bound to the grid. The next loop finds
                                            ' this bound grid row in the array of cart rows that match the target item ID then
                                            ' marks that cart row as having been bound.
                                            Dim j As Integer = 0
                                            Dim bMarkedAsBound As Boolean = False
                                            While j < iRowsWithTargetItemID.Length And Not bMarkedAsBound
                                                If iRowsWithTargetItemID(j).ToString() = i Then
                                                    bRowAlreadyBound(j) = True
                                                    bMarkedAsBound = True
                                                End If
                                                j = j + 1
                                            End While
                                        End If
                                    End If
                                    i = i + 1
                                End While
                                ' At this point, if we still haven't found the corresponding cart row, check
                                ' through the array of candidate rows (those whose item ID matches the target item ID)
                                ' and pick the first one that hasn't already been bound to the grid.
                                If Not bFoundRow Then
                                    i = 0
                                    While i < bRowAlreadyBound.Length And Not bFoundRow
                                        If Not bRowAlreadyBound(i) Then
                                            iRow = iRowsWithTargetItemID(i)
                                            bFoundRow = True
                                        End If
                                        i = i + 1
                                    End While
                                End If
                            Else
                                ' If the grid has had no items bound yet, just use the first row of the cart DT
                                ' which matches the target item ID.
                                iRow = iRowsWithTargetItemID(0)
                                bFoundRow = True
                            End If
                        End If
                    End If
                End If
            End If
        End If

        Return bFoundRow

    End Function

    Private Function FindCartItemIDRows(ByVal dt As DataTable, ByVal strTargetItemID As String, ByRef bRowAlreadyBound() As Boolean) As Integer()
        Dim i As Integer = 0
        Dim iRowsWithTargetItemID() As Integer

        For Each row As DataRow In dt.Rows
            If dt.Rows(i).RowState <> DataRowState.Deleted Then
                If Not String.IsNullOrEmpty(strTargetItemID) Or Not strTargetItemID = " " Then
                    If Not IsDBNull(row.Item(eCartCol.Itemid)) Then

                        If row.Item(eCartCol.Itemid) = strTargetItemID Then
                            If iRowsWithTargetItemID Is Nothing Then
                                ReDim iRowsWithTargetItemID(0)
                                ReDim bRowAlreadyBound(0)
                                bRowAlreadyBound(0) = False
                            Else
                                ReDim Preserve iRowsWithTargetItemID(iRowsWithTargetItemID.Length)
                                ReDim Preserve bRowAlreadyBound(bRowAlreadyBound.Length)
                                bRowAlreadyBound(bRowAlreadyBound.Length - 1) = False
                            End If
                            iRowsWithTargetItemID(iRowsWithTargetItemID.Length - 1) = i
                        End If  '  If row.Item(eCartCol.Itemid) = strTargetItemID Then
                    End If  '  If Not IsDBNull(row.Item(eCartCol.Itemid)) Then
                End If  '  If Not String.IsNullOrEmpty(strTargetItemID) Or Not strTargetItemID = " " Then

            End If  '  If dt.Rows(i).RowState <> DataRowState.Deleted Then
            i = i + 1
        Next

        Return iRowsWithTargetItemID
    End Function

    Private Function FindCartUniqNum(ByVal dt As DataTable, ByVal strTargetUniqNum As String, ByVal iRowsFound() As Integer, ByRef iRow As Integer) As Boolean
        Dim i As Integer = 0
        Dim bFoundRow As Boolean = False

        While i < iRowsFound.Length And Not bFoundRow
            Dim row As DataRow = dt.Rows(iRowsFound(i))
            If row.RowState <> DataRowState.Deleted Then
                If row.Item(eCartCol.UniqNum) = strTargetUniqNum Then
                    bFoundRow = True
                    iRow = iRowsFound(i)
                End If
            End If
            i = i + 1
        End While

        Return bFoundRow
    End Function

    Private Function IsCartSerialIDsSelected() As Boolean
        Dim dt As DataTable = CType(Session("Cart"), DataTable)
        Dim bHaveAllSerialIDs As Boolean = True
        Dim i As Integer = 0

        While i < dt.Rows.Count And bHaveAllSerialIDs
            If dt.Rows(i).Item(eCartCol.SerializedCheck).ToString() = GetSerialCheckText(True) Then
                If dt.Rows(i).Item(eCartCol.SerialID).ToString().Trim() = "" Then
                    bHaveAllSerialIDs = False
                End If
            End If
            i = i + 1
        End While

        Return bHaveAllSerialIDs
    End Function

    Private Function IsHeaderDueDateRequired() As Boolean
        Dim bResult As Boolean = False
        Dim I As Integer = 0
        Dim DueDate As Nullable(Of Date)
        DueDate = RadDatePickerReqByDate.SelectedDate
        Dim DueDateLine As Nullable(Of Date)
        If DueDate Is Nothing Then
            If rgCart.Items.Count > 0 Then
                For I = 0 To rgCart.Items.Count - 1
                    Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
                    Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
                    DueDateLine = CType(nestedview.FindControl("RDPDuedate"), RadDatePicker).SelectedDate
                    If DueDateLine Is Nothing Then
                        bResult = True
                        Exit For
                    Else
                        DueDate = DueDateLine
                    End If
                Next
                If Not bResult Then
                    RadDatePickerReqByDate.SelectedDate = DueDate
                End If
            End If  '  If rgCart.Items.Count > 0 Then
        End If  '  If DueDate Is Nothing Then

        Return bResult

    End Function

    Private Function IsAlertSerialIDMissing() As Boolean
        Dim bAlerted As Boolean = False
        If Not IsCartSerialIDsSelected() Then
            Dim strMessage As New Alert
            bAlerted = True
            ltlAlert.Text = strMessage.Say("All serialized items must be assigned a serial ID.")
        End If

        Return bAlerted
    End Function

    Private Sub UpdateDataSourceQtyForSerialized(ByVal iRow As Integer, ByVal strQty As String)
        Try
            Dim dt As DataTable = CType(rgCart.DataSource, DataTable)
            dt.Rows(iRow).Item(eCartCol.Quantity) = strQty
        Catch ex As Exception
        End Try
    End Sub

    Private Function LimitExpressionsList(ByVal strExpressions As String)
        Dim cMaxExpressions As Integer = 50

        Dim iCount As Integer = CountCharacter(strExpressions, ","c)

        If iCount > cMaxExpressions Then
            Dim ithComma As Integer = GetNthIndex(strExpressions, ","c, cMaxExpressions)
            strExpressions = strExpressions.Substring(0, ithComma)
        End If

        Return strExpressions
    End Function

    Private Function CountCharacter(ByVal strValue As String, ByVal ch As Char) As Integer
        Dim iCnt As Integer = 0
        For Each c As Char In strValue
            If c = ch Then iCnt += 1
        Next
        Return iCnt
    End Function

    Public Function GetNthIndex(ByVal strValue As String, ByVal ch As Char, ByVal iNth As Integer) As Integer
        Dim iCount As Integer = 0
        Dim i As Integer = 0
        Dim bFound As Boolean = False
        Dim iNthIndex As Integer = -1

        While i < strValue.Length - 1 And Not bFound
            If strValue(i) = ch Then
                iCount += 1
                If iCount = iNth Then
                    iNthIndex = i
                    bFound = True
                End If
            End If
            i += 1
        End While

        Return iNthIndex
    End Function

    ''' <summary>
    ''' This method checks for the session variables CARTCHGCDREQ and CARTWOREQ
    ''' and based on that set the mandatory condition for the Charge Code and Work Order
    ''' </summary>
    ''' <remarks></remarks>
    ''' 

    Private Sub CheckCHGCDWOValidation(Optional ByVal bEnable As Boolean = True, Optional ByRef bResultChgCd As Boolean = False)
        If Not bEnable Then
            Customvalidator1.Enabled = False
            Customvalidator2.Enabled = False
            CustomValidator3.Enabled = False
            Customvalidator4.Enabled = False
            CompareValidator2.Enabled = False
            RequiredFieldValidator1.Enabled = False
            'Requiredfieldvalidator2.Enabled = False
            Requiredfieldvalidator3.Enabled = False
            RequiredFieldValidator4.Enabled = False
            Requiredfieldvalidator6.Enabled = False
            CustomValidatorWorkOrder.Enabled = False
            CustomValidatorWOCH.Enabled = False
        End If

        Try
            'By default both these are not mandatory
            RequiredFieldValidator1.Enabled = False
            Requiredfieldvalidator6.Enabled = False
            CustomValidatorWorkOrder.Enabled = False
            CustomValidatorWOCH.Enabled = False
            lblChgCd.Text = "Order Charge Code:"
            lblWorkNo.Text = "Work Order #:"

            Dim I As Integer = 0
            If Session("CHARGECODE") = "Y" Then
                If Not Session("CARTCHGCDREQ") Is Nothing Then
                    If (Convert.ToString(Session("CARTCHGCDREQ")) = "1" Or Convert.ToString(Session("CARTCHGCDREQ")).ToLower() = "y") Then
                        'If bEnable Then
                        '    RequiredFieldValidator1.Enabled = True
                        'Else
                        '    RequiredFieldValidator1.Enabled = False
                        'End If
                        Dim strChgCdItem As String = ""
                        If Trim(txtChgCD.Text) = "" Then
                            If rgCart.Items.Count > 0 Then
                                If bEnable Then
                                    For I = 0 To rgCart.Items.Count - 1
                                        Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
                                        Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
                                        Dim txtChgCdItem As TextBox = CType(nestedview.FindControl("txtItemChgCode"), TextBox)
                                        Try
                                            If Trim(txtChgCdItem.Text) = "" Then
                                                bResultChgCd = True
                                                Exit For
                                            Else
                                                strChgCdItem = Trim(txtChgCdItem.Text)
                                            End If
                                        Catch ex As Exception
                                            bResultChgCd = True
                                        End Try
                                    Next
                                    If Not bResultChgCd Then
                                        txtChgCD.Text = strChgCdItem
                                    End If
                                End If  '  If bEnable Then

                            End If
                        End If
                        lblChgCd.Text = "*Order Charge Code:"
                    End If
                End If
            End If

            If Session("WORKORDER") = "Y" Then
                If Not Session("CARTWOREQ") Is Nothing Then
                    If (Convert.ToString(Session("CARTWOREQ")) = "1" Or Convert.ToString(Session("CARTWOREQ")).ToLower() = "y") Then
                        If bEnable Then
                            Requiredfieldvalidator6.Enabled = True
                        Else
                            Requiredfieldvalidator6.Enabled = False
                        End If
                        lblWorkNo.Text = "*Work Order #:"
                    End If
                End If
                'Work Order validation - task 522 - VR 12/17/2015
                If divOrdCenterIn.Visible Then
                    If Not Session("ValidateWorkOrder") Is Nothing Then
                        If (Convert.ToString(Session("ValidateWorkOrder")) = "Y") Or (Convert.ToString(Session("ValidateWorkOrder")) = "W") Then
                            If bEnable Then
                                'Requiredfieldvalidator6.Enabled = True  '20170301 Ticket 112033 removed Yury
                                CustomValidatorWorkOrder.Enabled = True
                                lblWorkNo.Text = "*Work Order #:"
                            Else
                                'Requiredfieldvalidator6.Enabled = False '20170301 Ticket 112033 removed Yury
                                CustomValidatorWorkOrder.Enabled = False
                            End If
                            'lblWorkNo.Text = "*Work Order #:"           '20170301 Ticket 112033 Removed since work order NOT required Yury
                        End If
                    End If
                End If
            End If

            If Session("WORKORDER") = "Y" AndAlso Session("CHARGECODE") = "Y" Then
                If Not Session("CARTWO_CHGCDREQ") Is Nothing Then
                    If (Convert.ToString(Session("CARTWO_CHGCDREQ")) = "1" Or Convert.ToString(Session("CARTWO_CHGCDREQ")).ToLower() = "y") Then
                        RequiredFieldValidator1.Enabled = False
                        Requiredfieldvalidator6.Enabled = False
                        'If txtChgCD.Text.Trim().Length <= 0 And txtWorkOrder.Text.Trim().Length <= 0 Then
                        '    'CustomValidatorWOCH.Enabled = True
                        'Else
                        '    'CustomValidatorWOCH.Enabled = False
                        'End If
                        If bEnable Then
                            CustomValidatorWOCH.Enabled = True
                        Else
                            CustomValidatorWOCH.Enabled = False
                        End If
                    End If
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub


    Private Function IsOROItem(sItemID As String) As Boolean
        Dim bIsORO As Boolean = False

        Try
            sItemID = sItemID.Trim
            If sItemID.Length > 0 Then
                If Not sItemID.StartsWith("NONCAT") Then
                    Dim sOROItemID As String

                    If sItemID.Length < 3 Then
                        If Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                            sOROItemID = sItemID
                        ElseIf sItemID.Substring(0, 3) = Session("SITEPREFIX") Then
                            sOROItemID = sItemID
                        Else
                            sOROItemID = Session("SITEPREFIX") & sItemID
                        End If
                    ElseIf sItemID.Substring(0, 3) = "SDI" Then
                        sOROItemID = sItemID
                    ElseIf Session("ITEMMODE") = "4" Or Session("ITEMMODE") = "6" Then
                        sOROItemID = sItemID
                    ElseIf sItemID.Substring(0, 3) = Session("SITEPREFIX") Then
                        sOROItemID = sItemID
                    Else
                        sOROItemID = Session("SITEPREFIX") & sItemID
                    End If

                    Dim itm As sdiItem = sdiItem.GetItemInfo(sOROItemID)
                    If itm.StockType.Id = "ORO" Then
                        bIsORO = True
                    End If
                End If
            End If
        Catch ex As Exception
            bIsORO = False
        End Try

        Return bIsORO
    End Function

    Private Sub CustomValidatorWorkOrder_ServerValidate(source As Object, args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles CustomValidatorWorkOrder.ServerValidate
        If divOrdCenterIn.Visible Then
            Try
                If Trim(txtWorkOrder.Text) <> "" Then '20170301 Adding to call only when not blank Ticket 112033 Yury
                    ' check is Enterprise flag set
                    If Not Session("ValidateWorkOrder") Is Nothing Then
                        If (Convert.ToString(Session("ValidateWorkOrder")) = "Y") Or (Convert.ToString(Session("ValidateWorkOrder")) = "W") Then
                            'validate is work order in the list
                            Dim bExists As Boolean = False
                            bExists = CheckIsWoExists()

                            If Not bExists Then
                                CustomValidatorWorkOrder.ErrorMessage = "Enter valid Work Order Number"
                                CustomValidatorWorkOrder.IsValid = False
                                args.IsValid = False
                            End If
                        End If  '  Session("ValidateWorkOrder") = "Y"
                    End If  '  Not Session("ValidateWorkOrder") Is Nothing
                End If
            Catch exMain As Exception
                ' just catch it
            End Try
        End If  '  divOrdCenter.Visible

    End Sub

    Private Function CheckIsWoExists() As Boolean
        'validate is work order in the list
        Dim strWorkOrder As String = Trim(txtWorkOrder.Text)
        Dim strWorkOrderToCompare As String = ""
        strBU = Session("BUSUNIT")
        Dim strSqlString As String = ""
        strSqlString = "SELECT A.CUST_ID,A.ISA_OBJECTID,A.ISA_ATTRLBL1,B.PLANT FROM SYSADM8.PS_NLNK2_OBJCT_VAL A, SYSADM8.PS_NLINK_CUST_PLNT B " &
            "WHERE A.ISA_ATTRVAL1 = B.PLANT AND B.BUSINESS_UNIT_OM = '" & strBU & "'"
        Dim bExists As Boolean = False
        Dim myConn1 As OleDbConnection = New OleDbConnection(DbUrl)
        Try
            Dim Command As OleDbCommand = New OleDbCommand(strSqlString, myConn1)
            Command.CommandTimeout = 120
            myConn1.Open()
            Dim dataAdapter As OleDbDataAdapter =
                    New OleDbDataAdapter(Command)

            Dim UserdataSet As System.Data.DataSet = New System.Data.DataSet()

            dataAdapter.Fill(UserdataSet)
            Dim iMy1 As Integer = 0
            If Not UserdataSet Is Nothing Then
                If UserdataSet.Tables.Count > 0 Then
                    If UserdataSet.Tables(0).Rows.Count > 0 Then
                        For iMy1 = 0 To UserdataSet.Tables(0).Rows.Count - 1
                            strWorkOrderToCompare = ""
                            Try
                                If Not UserdataSet.Tables(0).Rows(iMy1)("ISA_OBJECTID") Is Nothing Then
                                    strWorkOrderToCompare = UserdataSet.Tables(0).Rows(iMy1)("ISA_OBJECTID")
                                End If
                            Catch ex As Exception
                                strWorkOrderToCompare = ""
                            End Try
                            If Trim(strWorkOrderToCompare) <> "" Then
                                strWorkOrderToCompare = Trim(strWorkOrderToCompare)
                                If strWorkOrderToCompare = strWorkOrder Then
                                    bExists = True
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End If
            End If
            Try
                dataAdapter.Dispose()
            Catch ex01 As Exception

            End Try
            Try
                Command.Dispose()
            Catch ex02 As Exception

            End Try
            Try
                myConn1.Dispose()
                myConn1.Close()
            Catch ex03 As Exception

            End Try
        Catch exWO As Exception

        End Try

        Return bExists

    End Function

    Sub ValidateChargeCodeWorkOrderFlag(ByVal sender As Object, ByVal args As ServerValidateEventArgs)
        Try
            If Session("WORKORDER") = "Y" AndAlso Session("CHARGECODE") = "Y" Then
                If Not Session("CARTWO_CHGCDREQ") Is Nothing Then
                    If (Convert.ToString(Session("CARTWO_CHGCDREQ")) = "1" Or Convert.ToString(Session("CARTWO_CHGCDREQ")).ToLower() = "y") Then
                        RequiredFieldValidator1.Enabled = False
                        Requiredfieldvalidator6.Enabled = False
                        If txtChgCD.Text.Trim().Length <= 0 And txtWorkOrder.Text.Trim().Length <= 0 Then
                            Dim bResultChgCd As Boolean = False
                            Dim strChgCdItem As String = ""
                            Dim I As Integer = 0
                            If rgCart.Items.Count > 0 Then
                                For I = 0 To rgCart.Items.Count - 1
                                    Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
                                    Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
                                    Dim txtChgCdItem As TextBox = CType(nestedview.FindControl("txtItemChgCode"), TextBox)
                                    Try
                                        If Trim(txtChgCdItem.Text) = "" Then
                                            bResultChgCd = True
                                            Exit For
                                        Else
                                            strChgCdItem = Trim(txtChgCdItem.Text)
                                        End If
                                    Catch ex As Exception
                                        bResultChgCd = False
                                    End Try
                                Next
                                If Not bResultChgCd Then
                                    'txtChgCD.Text = strChgCdItem
                                Else
                                    args.IsValid = False
                                End If
                            End If  '  If rgCart.Items.Count > 0 Then

                        End If
                    End If
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub


    Protected Sub rgCart_ItemCommand(sender As Object, e As GridCommandEventArgs)
        Dim tmp As String = ""
        Dim script As String = ""
        Dim strChgCdUser As String = ""
        Dim strMachineRow As String = ""
        Dim strScript As String = ""
        strChgCdUser = strAgent

        If Session("SDIEMP") = "SDI" Then
            Dim strEmpId As String = " "
            Try
                If Trim(dropEmpID.SelectedValue) = "" Or Trim(dropEmpID.SelectedItem.Text) = "" Then
                    strEmpId = " "
                Else
                    If Trim(dropEmpID.SelectedItem.Value) <> "" Then
                        strEmpId = Trim(dropEmpID.SelectedItem.Value)
                    Else
                        strEmpId = " "
                    End If
                End If
            Catch ex As Exception
                strEmpId = " "
            End Try
            If Trim(strEmpId) <> "" Then
                strChgCdUser = Trim(strEmpId)
            Else
                strChgCdUser = Session("USERID")
            End If
            'strChgCdUser = dropEmpID.SelectedValue
        End If

        Dim item As GridDataItem = TryCast(DirectCast(e.Item, GridNestedViewItem).ParentItem, GridDataItem)
        Dim lblItemID1 As Label = CType(item.Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)
        Dim nestedview1 As GridNestedViewItem = DirectCast(e.Item, GridNestedViewItem)
        Dim hdfItemID1 As HiddenField = CType(nestedview1.FindControl("hdfItemID"), HiddenField)
        Dim txtItemChgCode As TextBox = CType(nestedview1.FindControl("txtItemChgCode"), TextBox)
        Dim lblUniqNum As HiddenField = CType(e.Item.FindControl("hdfUniqNum"), HiddenField)
        If Not lblUniqNum Is Nothing Then
            hdnUniqNum.Value = lblUniqNum.Value
        End If

        txtEmpChgCodeItem.Text = item.ItemIndex
        txtItemChgCodeItem.Text = item.ItemIndex

        If Not lblItemID1 Is Nothing And lblItemID1.Text.Trim().Length > 0 Then
            hdnLNItemID.Value = lblItemID1.Text
        ElseIf Not hdfItemID1 Is Nothing Then
            hdnLNItemID.Value = hdfItemID1.Value
        End If
        If e.CommandName = "ItemChgCode" Then
            RequiredFieldValidator1.Enabled = False
            tmp = "buildchrcd.aspx?BU=" & strBU & "&LINE=Y&USER=" & strChgCdUser & "&HOME=N&FROMRAD=RAD"
            RadWindowChCd.OffsetElementID = txtItemChgCode.ClientID
            RadWindowChCd.Left = Convert.ToDouble(hdnTop.Value) - 800
            RadWindowChCd.NavigateUrl = tmp
            RadWindowChCd.OnClientClose = "OnClientCloseFromGrid"
            RadWindowChCd.Title = "Enter Charge Code"
            hdnTop.Value = ""
            script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
        ElseIf e.CommandName = "EmpChgCode" Then
            Dim nestedview As GridNestedViewItem = DirectCast(e.Item, GridNestedViewItem)
            Dim hdfItemID As HiddenField = CType(nestedview.FindControl("hdfItemID"), HiddenField)
            Requiredfieldvalidator3.Enabled = False
            tmp = "buildempchgcd.aspx?BU=" & strBU & "&LINE=Y&USER=" & strChgCdUser & "&HOME=N&FROMRAD=RAD"
            RadWindowChCd.OffsetElementID = txtItemChgCode.ClientID
            RadWindowChCd.Left = Convert.ToDouble(hdnTop.Value) - 800
            RadWindowChCd.NavigateUrl = tmp
            RadWindowChCd.OnClientClose = "EmpChCdOnClientClose"
            RadWindowChCd.Title = "Enter Charge Code"
            hdnTop.Value = ""
            script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
        ElseIf e.CommandName = "MachineRow" Then
            txtMachineRowItem.Text = item.ItemIndex
            tmp = "buildMachineRow.aspx?BU=" & strBU & "&LINE=Y&MN=" & strMachineRow & "&HOME=N&FROMRAD=RAD"
            RadWindowChCd.NavigateUrl = tmp
            RadWindowChCd.OnClientClose = "MachineOnClientClose"
            RadWindowChCd.Title = "Machine Row"
            script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
        ElseIf e.CommandName = "AddtoFavs" Then
            Dim nestedview As GridNestedViewItem = DirectCast(e.Item, GridNestedViewItem)
            Dim parentItem As GridDataItem = TryCast(nestedview.ParentItem, GridDataItem)
            Dim lblItemID As Label = CType(parentItem.Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)
            Dim dsORFavs As DataSet
            Dim Manufacturer As String = parentItem("Manufacturer").Text
            Dim Manufacturerpartnumber As String = parentItem("Manufacturerpartnumber").Text
            Dim strSQLstring As String = " select * from ps_isa_user_cart where
inv_item_id ='" & lblItemID.Text & "' and BUSINESS_UNIT ='" & strBU & "' and ISA_EMPLOYEE_ID='" & Session("USERID") & "'"
            Try
                dsORFavs = ORDBData.GetAdapter(strSQLstring)
            Catch ex As Exception

            End Try

            Dim hdfItemID As HiddenField = CType(nestedview.FindControl("hdfItemID"), HiddenField)
            'Dim lblItemID As Label = CType(e.Item.Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)
            Dim stritemid As String = lblItemID.Text
            Dim arrNSTKinfo As New ArrayList(50)
            arrNSTKinfo.Insert(0, dsORFavs.Tables(0).Rows(0).Item("INV_ITEM_ID"))
            arrNSTKinfo.Insert(1, dsORFavs.Tables(0).Rows(0).Item("DESCR254"))
            arrNSTKinfo.Insert(2, dsORFavs.Tables(0).Rows(0).Item("QUANTITY"))
            arrNSTKinfo.Insert(3, dsORFavs.Tables(0).Rows(0).Item("PRICE"))
            arrNSTKinfo.Insert(4, dsORFavs.Tables(0).Rows(0).Item("UNIVERSAL_PART_NUM"))
            arrNSTKinfo.Insert(5, dsORFavs.Tables(0).Rows(0).Item("CP_CUSTOMERITEMID"))
            arrNSTKinfo.Insert(6, dsORFavs.Tables(0).Rows(0).Item("RFQREQ"))
            arrNSTKinfo.Insert(7, dsORFavs.Tables(0).Rows(0).Item("CHKDELETE"))
            arrNSTKinfo.Insert(8, dsORFavs.Tables(0).Rows(0).Item("ITEM_CHG_CODE"))
            arrNSTKinfo.Insert(9, dsORFavs.Tables(0).Rows(0).Item("NYCCAT"))
            arrNSTKinfo.Insert(10, dsORFavs.Tables(0).Rows(0).Item("EMP_CHG_CODE"))
            arrNSTKinfo.Insert(11, dsORFavs.Tables(0).Rows(0).Item("MANUFACTURER"))
            arrNSTKinfo.Insert(12, dsORFavs.Tables(0).Rows(0).Item("CLIENT_MFG_PART"))
            arrNSTKinfo.Insert(13, dsORFavs.Tables(0).Rows(0).Item("UNIT_OF_MEASURE"))
            arrNSTKinfo.Insert(14, dsORFavs.Tables(0).Rows(0).Item("MACHINE_ROW"))
            arrNSTKinfo.Insert(15, dsORFavs.Tables(0).Rows(0).Item("LAST_PRICE_PAID"))
            arrNSTKinfo.Insert(16, dsORFavs.Tables(0).Rows(0).Item("ISA_LPP_EXIST"))
            arrNSTKinfo.Insert(17, dsORFavs.Tables(0).Rows(0).Item("TAX_FLAG"))
            arrNSTKinfo.Insert(18, dsORFavs.Tables(0).Rows(0).Item("SUPPLIER_PART_NUM"))
            arrNSTKinfo.Insert(19, dsORFavs.Tables(0).Rows(0).Item("PRICE_PO"))
            arrNSTKinfo.Insert(20, dsORFavs.Tables(0).Rows(0).Item("PO_ID"))
            arrNSTKinfo.Insert(21, dsORFavs.Tables(0).Rows(0).Item("PO_LINE"))
            arrNSTKinfo.Insert(22, dsORFavs.Tables(0).Rows(0).Item("ISA_UNSPSC_CD"))
            arrNSTKinfo.Insert(23, dsORFavs.Tables(0).Rows(0).Item("IMAGE_NAME"))
            arrNSTKinfo.Insert(24, dsORFavs.Tables(0).Rows(0).Item("CLASS_ID"))
            arrNSTKinfo.Insert(25, dsORFavs.Tables(0).Rows(0).Item("SERIAL_ID"))
            arrNSTKinfo.Insert(26, dsORFavs.Tables(0).Rows(0).Item("SERIAL_CHECK"))
            arrNSTKinfo.Insert(27, dsORFavs.Tables(0).Rows(0).Item("ISA_UNIQ_NBR"))
            arrNSTKinfo.Insert(28, dsORFavs.Tables(0).Rows(0).Item("SUPP_PART_NUM_AUX"))
            arrNSTKinfo.Insert(29, dsORFavs.Tables(0).Rows(0).Item("COLORNAME"))
            arrNSTKinfo.Insert(30, dsORFavs.Tables(0).Rows(0).Item("PRIORITY"))
            arrNSTKinfo.Insert(31, dsORFavs.Tables(0).Rows(0).Item("DUE_DATE"))
            arrNSTKinfo.Insert(32, dsORFavs.Tables(0).Rows(0).Item("ORDER_TYPE_CD"))
            arrNSTKinfo.Insert(33, dsORFavs.Tables(0).Rows(0).Item("ISA_CUST_NOTES"))
            arrNSTKinfo.Insert(34, dsORFavs.Tables(0).Rows(0).Item("FILE_PATH"))
            arrNSTKinfo.Insert(35, dsORFavs.Tables(0).Rows(0).Item("ISA_USER_DEFINED_1"))
            arrNSTKinfo.Insert(36, dsORFavs.Tables(0).Rows(0).Item("ISA_USER_DEFINED_2"))
            arrNSTKinfo.Insert(37, dsORFavs.Tables(0).Rows(0).Item("ISA_USER_DEFINED_3"))
            arrNSTKinfo.Insert(38, dsORFavs.Tables(0).Rows(0).Item("ISA_USER_DEFINED_4"))
            arrNSTKinfo.Insert(39, stritemid)
            arrNSTKinfo.Insert(40, dsORFavs.Tables(0).Rows(0).Item("SUPPLIER_ID"))
            arrNSTKinfo.Insert(41, dsORFavs.Tables(0).Rows(0).Item("ADD_TO_CTLG"))
            Session("NSTKInfo") = arrNSTKinfo
            If lblItemID.Text.ToUpper = hdfItemID.Value.ToUpper Then
                stritemid = getcpjunc(lblItemID.Text.ToUpper)
            End If

            tmp = "fasvadds.aspx?ItemID=" & stritemid & "&CitemID=" & lblItemID.Text.ToUpper & "&BU=" & strBU & "&USER=" & strAgent & "&FROMRAD=RAD"
            RadWindowChCd.NavigateUrl = tmp
            RadWindowChCd.OnClientClose = "FavsOnClientClose"
            RadWindowChCd.Title = "Add to Favorites"
            script = "function f(){$find(""" + RadWindowChCd.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

        ElseIf e.CommandName = "Notes" Then
            txtEmpMoteRowItem.Text = item.ItemIndex
            Dim nestedview As GridNestedViewItem = DirectCast(e.Item, GridNestedViewItem)
            Dim parentItem As GridDataItem = TryCast(nestedview.ParentItem, GridDataItem)
            Dim txtNotes As TextBox = CType(nestedview.FindControl("txtNotes"), TextBox)
            Dim lblItemID As Label = CType(parentItem.Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label)
            'hdnLNItemID.Value = lblItemID.Text
            Dim Notes As String = txtNotes.Text
            If Not String.IsNullOrEmpty(Notes.Trim()) Then
                txtrwwNotes.Text = Notes
            End If
            revNotes.Enabled = True
            'rfvNotes.Enabled = True
            script = "function f(){$find(""" + rwwNotes.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
        End If
    End Sub

    Protected Sub rgCart_ItemDataBound(sender As Object, e As GridItemEventArgs)
        If TypeOf e.Item Is GridDataItem Then

            'CType(e.Item.FindControl("lblItemDescc"), Label).Visible = False
            'CType(e.Item.FindControl("txtItemDescc"), TextBox).Visible = True

            CType(e.Item.FindControl("lblItemDescc"), Label).Visible = True
            CType(e.Item.FindControl("txtItemDescc"), TextBox).Visible = False
            Dim strItemIdVal As String = CType(e.Item.FindControl("lblItemID"), Label).Text

            Dim priceVal As String = DirectCast(e.Item.DataItem, DataRowView)("Price").ToString()
            Dim supplIDVal As String = DirectCast(e.Item.DataItem, DataRowView)("SupplierID").ToString()

            priceVal = FormatNumber(priceVal, 2)

            If Trim(strItemIdVal) <> "" And String.IsNullOrEmpty(Trim(supplIDVal)) And (priceVal = "0" Or priceVal = "0.00") Then
                If Len(strItemIdVal) > 5 Then
                    If Left(strItemIdVal, 6) = "NONCAT" Then

                        CType(e.Item.FindControl("lblItemDescc"), Label).Visible = False
                        CType(e.Item.FindControl("txtItemDescc"), TextBox).Visible = True
                    End If

                End If
            End If
        End If

        If TypeOf e.Item Is GridNestedViewItem Then
            Dim objEnterprise As New clsEnterprise(strBU)
            strTaxFlag = objEnterprise.TaxFlag
            Dim RFQSite As Boolean = False

            Dim priceVal As String = DirectCast(e.Item.DataItem, DataRowView)("Price").ToString()
            Dim supplIDVal As String = DirectCast(e.Item.DataItem, DataRowView)("SupplierID").ToString()

            priceVal = FormatNumber(priceVal, 2)
            Dim strItemIdVal As String = CType(e.Item.FindControl("hdfItemIdMy"), HiddenField).Value

            If Trim(strItemIdVal) <> "" And String.IsNullOrEmpty(Trim(supplIDVal)) And (priceVal = "0" Or priceVal = "0.00") Then
                If Len(strItemIdVal) > 5 Then
                    If Left(strItemIdVal, 6) = "NONCAT" Then

                        RFQSite = True
                    End If

                End If
            End If



            '' To hide the priority and Due date in line level for the items that have price and Supplier value 
            If Session("ZEUSNOCATALOGSITE") = "Y" And Session("ZEUS_SITE") = "Y" Then


                Dim thDD As HtmlTableCell = CType(e.Item.FindControl("thDD"), HtmlTableCell)
                Dim thPrity As HtmlTableCell = CType(e.Item.FindControl("thPrity"), HtmlTableCell)
                Dim tdDD As HtmlTableCell = CType(e.Item.FindControl("tdDD"), HtmlTableCell)
                Dim tdPrity As HtmlTableCell = CType(e.Item.FindControl("tdPrity"), HtmlTableCell)
                Dim thAttchm1 As HtmlTableCell = CType(e.Item.FindControl("thAttchm1"), HtmlTableCell)
                Dim tdAttachm As HtmlTableCell = CType(e.Item.FindControl("tdAttachm"), HtmlTableCell)
                Dim RDPDuedate As RadDatePicker = CType(e.Item.FindControl("RDPDuedate"), RadDatePicker)

                If Not String.IsNullOrEmpty(supplIDVal) And Not (priceVal = "0" Or priceVal = "0.00") Then
                    thDD.Visible = False
                    thPrity.Visible = False
                    tdDD.Visible = False
                    tdPrity.Visible = False
                    tdAttachm.Visible = False
                    thAttchm1.Visible = False
                End If

            End If


            If Session("PUNCHIN") = "YES" Then
                Dim thFav As HtmlTableCell = CType(e.Item.FindControl("thFav"), HtmlTableCell)
                thFav.Visible = False
                Dim tdFav As HtmlTableCell = CType(e.Item.FindControl("tdFav"), HtmlTableCell)
                tdFav.Visible = False
            End If

            Try
                If Not (m_siteCurrency Is Nothing) Then
                    Dim thPrice As HtmlTableCell = CType(e.Item.FindControl("thPrice"), HtmlTableCell)
                    Dim thExtPrice As HtmlTableCell = CType(e.Item.FindControl("thExtPrice"), HtmlTableCell)
                    If RFQSite = True Then
                        thPrice.InnerHtml = "Estimated Price<br>(" & m_siteCurrency.Id & ")"
                        thExtPrice.InnerHtml = "Estimated Ext. Price<br>(" & m_siteCurrency.Id & ")"
                    Else
                        thPrice.InnerHtml = "Price<br>(" & m_siteCurrency.Id & ")"
                        thExtPrice.InnerHtml = "Ext. Price<br>(" & m_siteCurrency.Id & ")"

                    End If

                    'e.Item.Cells(dtgCartColumns.itemPrice).Text = "Price<br>(" & m_siteCurrency.Id & ")"
                    'e.Item.Cells(dtgCartColumns.itemExtendedPrice).Text = "Ext. Price<br>(" & m_siteCurrency.Id & ")"
                End If
            Catch ex As Exception
            End Try

            Try
                If Session("ShowPrice") = "0" Then
                    Dim thPrice As HtmlTableCell = CType(e.Item.FindControl("thPrice"), HtmlTableCell)
                    Dim thExtPrice As HtmlTableCell = CType(e.Item.FindControl("thExtPrice"), HtmlTableCell)
                    Dim tdPrice As HtmlTableCell = CType(e.Item.FindControl("tdPrice"), HtmlTableCell)
                    Dim tdExtPrice As HtmlTableCell = CType(e.Item.FindControl("tdExtPrice"), HtmlTableCell)
                    Dim tdTargetPrice As HtmlTableCell = CType(e.Item.FindControl("tdTargetPrice"), HtmlTableCell)
                    Dim tdTargetExtPrice As HtmlTableCell = CType(e.Item.FindControl("tdTargetExtPrice"), HtmlTableCell)
                    thPrice.Visible = False
                    thExtPrice.Visible = False
                    tdPrice.Visible = False
                    tdExtPrice.Visible = False
                    tdTargetPrice.Visible = False
                    tdTargetExtPrice.Visible = False
                Else
                    Dim thPrice As HtmlTableCell = CType(e.Item.FindControl("thPrice"), HtmlTableCell)
                    Dim thExtPrice As HtmlTableCell = CType(e.Item.FindControl("thExtPrice"), HtmlTableCell)
                    Dim tdPrice As HtmlTableCell = CType(e.Item.FindControl("tdPrice"), HtmlTableCell)
                    Dim tdExtPrice As HtmlTableCell = CType(e.Item.FindControl("tdExtPrice"), HtmlTableCell)
                    Dim tdTargetPrice As HtmlTableCell = CType(e.Item.FindControl("tdTargetPrice"), HtmlTableCell)
                    Dim tdTargetExtPrice As HtmlTableCell = CType(e.Item.FindControl("tdTargetExtPrice"), HtmlTableCell)
                    thPrice.Visible = True
                    thExtPrice.Visible = True
                    If RFQSite = True Then
                        tdPrice.Visible = False
                        tdExtPrice.Visible = False
                        tdTargetPrice.Visible = True
                        tdTargetExtPrice.Visible = True

                    Else
                        tdPrice.Visible = True
                        tdExtPrice.Visible = True
                        tdTargetPrice.Visible = False
                        tdTargetExtPrice.Visible = False
                    End If

                End If
            Catch ex As Exception

            End Try

            ' Image Binding code
            Dim ProdImg As Image = CType(e.Item.FindControl("imgProduct"), Image)
            Dim hdfImg As HiddenField = CType(e.Item.FindControl("hdfImage"), HiddenField)
            Dim StrImg As String = hdfImg.Value

            If Not String.IsNullOrEmpty(StrImg) And Not StrImg = "&nbsp;" Then
                If StrImg.Contains("noimage_new.png") Then
                    ProdImg.Visible = False
                Else
                    ProdImg.ImageUrl = StrImg
                End If
            Else
                ProdImg.Visible = False
            End If

            ' Add Numeric condition for Quantity
            Dim Qtytxt As TextBox = CType(e.Item.FindControl("txtQTY"), TextBox)
            Qtytxt.Attributes.Add("onkeypress", "return CheckNumeric()")

            Dim lblItemID As HiddenField
            Dim bIsNoncat As Boolean = False
            Dim IsViewParttxt As Boolean = False

            Dim strItemIdMy1 As String = CType(e.Item.FindControl("hdfItemIdMy"), HiddenField).Value

            Dim chkAddToCtlg As CheckBox
            Dim Attachments As Button
            chkAddToCtlg = CType(e.Item.FindControl("chkAddToCtlg"), CheckBox)
            Attachments = CType(e.Item.FindControl("btnAttach"), Button)
            Dim thAddToCatlg As HtmlTableCell = CType(e.Item.FindControl("thAddToCatlg"), HtmlTableCell)
            Dim tdAddCtlg As HtmlTableCell = CType(e.Item.FindControl("tdAddCtlg"), HtmlTableCell)

            Dim lblUniqNum As HiddenField = CType(e.Item.FindControl("hdfUniqNum"), HiddenField)

            Attachments.Visible = False
            chkAddToCtlg.Visible = False
            thAddToCatlg.Visible = False
            tdAddCtlg.Visible = False
            If Trim(strItemIdMy1) <> "" Then
                If Len(strItemIdMy1) > 5 Then
                    If Left(strItemIdMy1, 6) = "NONCAT" Then
                        Attachments.Visible = True
                        IsViewParttxt = True
                        lblItemID = CType(e.Item.FindControl("hdfItemIdMy"), HiddenField)
                        bIsNoncat = True
                        If Session("ADDTOCATALOG") = "Y" Then
                            chkAddToCtlg.Visible = True
                            thAddToCatlg.Visible = True
                            tdAddCtlg.Visible = True
                            chkAddToCtlg.Attributes.Add("onclick", "StoreCatalogToCart('" + lblItemID.Value + "', '" + chkAddToCtlg.ClientID + "', '" + lblUniqNum.Value + "')")
                        End If
                    Else
                        lblItemID = CType(e.Item.FindControl("hdfItemID"), HiddenField)
                    End If
                Else
                    lblItemID = CType(e.Item.FindControl("hdfItemID"), HiddenField)
                End If
            Else
                lblItemID = CType(e.Item.FindControl("hdfItemID"), HiddenField)
            End If

            Dim chkTaxExmpt As CheckBox
            Dim l As LinkButton
            Dim m As LinkButton
            Dim t As TextBox
            'Dim LPP As TextBox
            Dim PO As TextBox
            Dim LN As TextBox
            Dim divPO As HtmlInputText
            Dim divLN As HtmlInputText
            Dim SerialID As Label
            'Dim LNNotes As TextBox
            'Dim txtEmpChgCode As TextBox
            ' PO AND LN 
            PO = CType(e.Item.FindControl("txtPO"), TextBox)
            LN = CType(e.Item.FindControl("txtLN"), TextBox)
            l = CType(e.Item.FindControl("cmdFav"), LinkButton)
            'LPP = CType(e.Item.FindControl("txtLPP"), TextBox)
            'txtEmpChgCode = CType(e.Item.FindControl("txtEmpChgCode"), TextBox)

            'LNNotes = CType(e.Item.FindControl("txtNotes"), TextBox)
            SerialID = CType(e.Item.FindControl("lblSerialID"), Label)
            chkTaxExmpt = CType(e.Item.FindControl("chbTaxFlag"), CheckBox)
            'LPP.Attributes.Add("onchange", "StoreLPPToCart('" + lblItemID.Value + "','" + LPP.ClientID + "' )")
            'LNNotes.Attributes.Add("onchange", "StoreLNNotesToCart('" + lblItemID.Value + "','" + LNNotes.ClientID + "' )")
            'txtEmpChgCode.Attributes.Add("onchange", "StoreLPPToCart('" + lblItemID.Value + "','" + LPP.ClientID + "' )")

            If Not Session("SITEBU") = "SDM00" Then
                PO.Visible = False
                LN.Visible = False
                CType(e.Item.FindControl("lblPO"), Label).Visible = False
                CType(e.Item.FindControl("lblLN"), Label).Visible = False
                strblankrowSDM = "N"
            Else
                PO.Attributes.Add("onchange", "StoreMXFileldToCart('" + lblItemID.Value + "','" + PO.ClientID + "', 'PO','" + lblUniqNum.Value + "')")
                LN.Attributes.Add("onchange", "StoreMXFileldToCart('" + lblItemID.Value + "','" + LN.ClientID + "', 'LN','" + lblUniqNum.Value + "')")
                strblankrowSDM = "Y"
            End If

            KillPrimaryKey(Session("Cart"))

            Dim cmdFav As LinkButton = CType(e.Item.FindControl("cmdFav"), LinkButton)
            If bIsNoncat Then  '  If lblItemID.Value.Contains("NONCAT") Then
                cmdFav.Visible = True
            End If


            Dim ProductID As String = lblItemID.Value
            Dim row As DataRow

            If Not Session("Cart") Is Nothing Then
                Dim iRow As Integer
                If FindCartDTRow(Session("Cart"), ProductID, lblUniqNum.Value, iRow) Then
                    row = CType(Session("Cart"), DataTable).Rows(iRow)
                End If
            End If
            If Not Session("Cart") Is Nothing Then

                If Not row Is Nothing Then
                    Try
                        lblUniqNum.Value = row.Item(eCartCol.UniqNum)
                    Catch ex As Exception
                    End Try

                    If IsDBNull(row.Item("LPPExist")) Then
                        row.Item("LPPExist") = "NOT CHECKED"
                    End If
                    If Not row.Item("LPPExist") = "ALREADY CHECKED" And
                        Not row.Item("LPPExist") = "ALREADY CHECKED AND EXIST" Then

                        row.Item("LPPExist") = "ALREADY CHECKED"
                        '    row.Item("LPP") = LPP.Text
                        '    If Not Trim(LPP.Text) = "" Then
                        '        row.Item("LPPExist") = "ALREADY CHECKED AND EXIST"
                        '    End If
                    End If

                    If IsDBNull(row.Item("TaxFlag")) Then
                        row.Item("TaxFlag") = " "
                    End If
                    If row.Item("TaxFlag") = "Y" Then
                        chkTaxExmpt.Checked = True
                    Else
                        chkTaxExmpt.Checked = False
                    End If
                End If  '  If Not row Is Nothing Then
            End If '  If Not Session("Cart") Is Nothing Then

            'If Not Trim(LPP.Text) = "" Then
            '    LPP.Text = FormatNumber(LPP.Text, 2)
            'End If
            'LPP.Attributes.Add("onkeypress", "CheckNumeric()")
            If cmbMachine.Visible = False Then

                m = CType(e.Item.FindControl("btnMachineRow1"), LinkButton)

                t = CType(e.Item.FindControl("txtMachineRow"), TextBox)

                m.Visible = False
                t.Visible = False
            End If

            'new code to show or not chkTaxExmpt - VR 01/13/2017
            chkTaxExmpt.Visible = False
            Try
                If Not Session("CARTTAXEXMPTFLAG") Is Nothing Then
                    If Trim(Session("CARTTAXEXMPTFLAG")) <> "" Then
                        chkTaxExmpt.Visible = True
                        'Dim currentRow As String = Session("rgCartCurrentRow")
                        chkTaxExmpt.Attributes.Add("onclick", "StoreToCart('" + lblItemID.Value + "', '" + chkTaxExmpt.ClientID + "', 'TaxExempt', '" + lblUniqNum.Value + "')")
                        'chkTaxExmpt.Attributes.Add("onclick", "StoreToCart('" + lblItemID.Value + "', '" + chkTaxExmpt.ClientID + "', 'TaxExempt', '" + currentRow + "')")
                    End If
                End If
            Catch ex As Exception
                chkTaxExmpt.Visible = False
            End Try

            ' item quantity
            Dim qty As Decimal = CDec("0")
            Try
                Dim txtItemQty As TextBox = CType(e.Item.FindControl("txtQTY"), TextBox)
                txtItemQty.Attributes.Add("onchange", "StoreQtyToCart('" + lblItemID.Value + "','" + txtItemQty.ClientID + "','" + lblUniqNum.Value + "')")
                qty = CDec(txtItemQty.Text)

                Dim btnItemPickSerial As Button = CType(e.Item.FindControl("btnPickSerialID"), Button)
                Dim lblItemSerialID As Label = CType(e.Item.FindControl("lblSerialID"), Label)
                Dim bIsSerialized As Boolean = False

                If Not Session("Cart") Is Nothing Then

                    If Not row Is Nothing Then
                        If IsDBNull(row.Item(eCartCol.SerializedCheck)) Then
                            If bIsNoncat Then
                                row.Item(eCartCol.SerializedCheck) = GetSerialCheckText(False)
                            Else
                                If IsSerializedItem(lblItemID.Value) Then
                                    row.Item(eCartCol.SerializedCheck) = GetSerialCheckText(True)
                                    If qty > 1 Then
                                        txtItemQty.Text = "1"
                                        qty = CType(txtItemQty.Text, Decimal)
                                        UpdateDataSourceQtyForSerialized(e.Item.ItemIndex, txtItemQty.Text)
                                    End If
                                    bIsSerialized = True
                                Else
                                    row.Item(eCartCol.SerializedCheck) = GetSerialCheckText(False)
                                End If
                            End If

                            row.Item("SerialID") = SerialID.Text
                        ElseIf row.Item(eCartCol.SerializedCheck) = GetSerialCheckText(True) Then
                            bIsSerialized = True
                        End If
                    End If
                End If

                If bIsSerialized Then
                    txtItemQty.Enabled = False
                    txtItemQty.BackColor = LightGray
                    txtItemQty.ForeColor = Black
                    btnItemPickSerial.Visible = True
                    lblItemSerialID.Visible = True
                Else
                    btnItemPickSerial.Visible = False
                    lblItemSerialID.Visible = False
                End If

            Catch ex As Exception
            End Try

            ' item base price and currency
            '   base price is already being binded on catalogTree.aspx - erwin
            '   get item's base price; can hold "Price on Request"
            Dim itemBasePrice As String = ""
            Try
                itemBasePrice = CType(e.Item.FindControl("hdfItemBasePrice"), HiddenField).Value
                If (itemBasePrice Is Nothing) Then
                    itemBasePrice = ""
                End If
            Catch ex As Exception
            End Try
            '   get SDI item Id (customerItemId)
            Dim sitePrefix As String = ""
            Try
                sitePrefix = CStr(Session("SITEPREFIX"))
                If (sitePrefix Is Nothing) Then
                    sitePrefix = ""
                End If
            Catch ex As Exception
            End Try
            Dim sdiItemId As String = ""
            Try
                sdiItemId = sitePrefix & CType(e.Item.FindControl("hdfItemID"), HiddenField).Value
            Catch ex As Exception
            End Try
            '   get item's default (first available) currency 
            Dim itemBaseCurrency As sdiCurrency = Nothing
            Try
                itemBaseCurrency = sdiMultiCurrency.getItemBaseCurrency(sdiItemId)
                CType(e.Item.FindControl("hdfItemBaseCurrencyCode"), HiddenField).Value = itemBaseCurrency.Id
            Catch ex As Exception
            End Try

            ' check item base currency against site currency
            '   convert item price to show if not the same
            Try
                If itemBasePrice.Trim.Length > 0 Then
                    If IsNumeric(itemBasePrice) Then
                        If m_siteCurrency.Id <> itemBaseCurrency.Id Then
                            Dim convRate As sdiConversionRate = sdiMultiCurrency.getConversionRate(itemBaseCurrency.Id,
                                                                                                   CDec(itemBasePrice),
                                                                                                   m_siteCurrency.Id)
                            ' convert price to site's currency; update shown prices
                            Dim nPrice As Decimal = Math.Round(convRate.ConvertedAmount, 2)
                            Dim nLineTotal As Decimal = Math.Round(nPrice * qty, 2)
                            CType(e.Item.FindControl("Price"), Label).Text = nPrice.ToString("####,###,##0.00")
                            CType(e.Item.FindControl("ExtPrice"), Label).Text = nLineTotal.ToString("####,###,##0.00")

                            ' show currency symbol; hide currency code
                            CType(e.Item.FindControl("lblItemCurrencySymbol"), Label).Text = m_siteCurrency.Symbol
                            CType(e.Item.FindControl("lblItemCurrencySymbol"), Label).Visible = True
                            CType(e.Item.FindControl("lblItemCurrencyCode"), Label).Text = convRate.TargetCurrencyCode
                            CType(e.Item.FindControl("lblItemCurrencyCode"), Label).Visible = False

                            CType(e.Item.FindControl("lblExtPriceCurrencySymbol"), Label).Text = m_siteCurrency.Symbol
                            CType(e.Item.FindControl("lblExtPriceCurrencySymbol"), Label).Visible = True
                            CType(e.Item.FindControl("lblExtPriceCurrencyCode"), Label).Text = convRate.TargetCurrencyCode
                            CType(e.Item.FindControl("lblExtPriceCurrencyCode"), Label).Visible = False
                        Else
                            ' show currency symbol; hide currency code
                            CType(e.Item.FindControl("lblItemCurrencySymbol"), Label).Text = itemBaseCurrency.Symbol
                            CType(e.Item.FindControl("lblItemCurrencySymbol"), Label).Visible = True
                            CType(e.Item.FindControl("lblItemCurrencyCode"), Label).Text = itemBaseCurrency.Id
                            CType(e.Item.FindControl("lblItemCurrencyCode"), Label).Visible = False

                            CType(e.Item.FindControl("lblExtPriceCurrencySymbol"), Label).Text = itemBaseCurrency.Symbol
                            CType(e.Item.FindControl("lblExtPriceCurrencySymbol"), Label).Visible = True
                            CType(e.Item.FindControl("lblExtPriceCurrencyCode"), Label).Text = itemBaseCurrency.Id
                            CType(e.Item.FindControl("lblExtPriceCurrencyCode"), Label).Visible = False
                        End If
                    Else
                        ' hide both currency symbol and code, since its not numeric
                        CType(e.Item.FindControl("lblItemCurrencySymbol"), Label).Visible = False
                        CType(e.Item.FindControl("lblItemCurrencyCode"), Label).Visible = False
                    End If
                End If

                If RFQSite = True Then
                    Dim itemEstimatedPrice As String = ""
                    Try
                        itemEstimatedPrice = CType(e.Item.FindControl("hdfEstimatedPrice"), HiddenField).Value
                        If (itemEstimatedPrice Is Nothing) Then
                            itemEstimatedPrice = ""
                        End If
                    Catch ex As Exception
                    End Try
                    If itemBasePrice.Trim.Length > 0 Then
                        If IsNumeric(itemEstimatedPrice) Then
                            ' show currency symbol; hide currency code
                            CType(e.Item.FindControl("lblEstItemCurrencySymbol"), Label).Text = itemBaseCurrency.Symbol
                            CType(e.Item.FindControl("lblEstItemCurrencySymbol"), Label).Visible = True
                            CType(e.Item.FindControl("lblTargetItemCurrencyCode"), Label).Text = itemBaseCurrency.Id
                            CType(e.Item.FindControl("lblTargetItemCurrencyCode"), Label).Visible = False

                            CType(e.Item.FindControl("lblTargetExtPriceCurrencySymbol"), Label).Text = itemBaseCurrency.Symbol
                            CType(e.Item.FindControl("lblTargetExtPriceCurrencySymbol"), Label).Visible = True
                            CType(e.Item.FindControl("lblTargetExtPriceCurrencyCode"), Label).Text = itemBaseCurrency.Id
                            CType(e.Item.FindControl("lblTargetExtPriceCurrencyCode"), Label).Visible = False

                        End If

                    End If

                End If
            Catch ex As Exception
            End Try

            ' item UNSPSC
            If Not Session("cart") Is Nothing Then
                Try
                    If Not row Is Nothing Then
                        If Not IsDBNull(row("ItemUNSPSC")) Then
                            CType(e.Item.FindControl("hdfItemUNSPSC"), HiddenField).Value = CStr(row("ItemUNSPSC"))
                        End If
                    End If
                Catch ex As Exception
                End Try
            End If

            ' Due Date  
            Try
                If Not row Is Nothing Then
                    If Not IsDBNull(row("UDueDate")) Then
                        CType(e.Item.FindControl("RDPDuedate"), RadDatePicker).SelectedDate = CDate(row("UDueDate"))
                    End If
                Else
                    CType(e.Item.FindControl("RDPDuedate"), RadDatePicker).SelectedDate = RadDatePickerReqByDate.SelectedDate
                End If

            Catch ex As Exception
            End Try

            ' Item line Priority 
            Try
                Dim chkPriority As CheckBox = CType(e.Item.FindControl("chkPriority"), CheckBox)
                If Not IsDBNull(row("UPriority")) Then
                    chkPriority.Checked = (row("UPriority") = "R") '1
                    'CType(e.Item.FindControl("chkPriority"), CheckBox).Checked = (row("UPriority") = "1")
                End If
                'Dim currentRow As String = Session("rgCartCurrentRow")
                chkPriority.Attributes.Add("onclick", "StoreToCart('" + lblItemID.Value + "', '" + chkPriority.ClientID + "', 'Priority', '" + lblUniqNum.Value + "')")
                'chkPriority.Attributes.Add("onclick", "StoreToCart('" + lblItemID.Value + "', '" + chkPriority.ClientID + "', 'Priority', '" + currentRow + "')")

            Catch ex As Exception

            End Try

            Try
                Dim DueDate As RadDatePicker = CType(e.Item.FindControl("RDPDuedate"), RadDatePicker)
                If Not DueDate Is Nothing Then
                    DueDate.Attributes.Add("onchange", "StoreDueDateToCart('" + lblItemID.Value + "', '" + DueDate.ClientID + "', '" + lblUniqNum.Value + "')")
                End If
            Catch ex As Exception

            End Try

            ' chkAddToCtlg
            Try
                If Not row Is Nothing Then
                    If Not IsDBNull(row("AddToCtlgFlag")) Then
                        CType(e.Item.FindControl("chkAddToCtlg"), CheckBox).Checked = (row("AddToCtlgFlag") = "1")
                    End If
                End If

            Catch ex As Exception

            End Try

            'Machine No

            Try
                Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
                Dim cmbMachineNo As DropDownList = CType(e.Item.FindControl("ddlMachineNo"), DropDownList)
                Dim PartNoheader As HtmlTableCell = CType(e.Item.FindControl("thMachineNo"), HtmlTableCell)
                Dim tdPartNo As HtmlTableCell = CType(e.Item.FindControl("tdMachinNo"), HtmlTableCell)
                Dim txtMachineNo As TextBox = CType(e.Item.FindControl("txtMachineNo"), TextBox)
                txtMachineNo.Visible = False

                If currentApp.Session("PARTLIST") = "Y" Then

                    Dim dt As DataTable = Session("cart")

                    Dim I As Integer
                    For I = 0 To rgCart.Items.Count - 1

                        Dim srtItemID As String = (CType(rgCart.Items.Item(I).Cells(dtgCartColumns.itemId).FindControl("lblItemID"), Label).Text).ToString
                        If Len(srtItemID) > 2 Then
                            If srtItemID.Substring(0, 3) = currentApp.Session("SITEPREFIX") Then

                            Else
                                srtItemID = currentApp.Session("SITEPREFIX") & srtItemID
                            End If
                        End If

                        cmbMachineNo.Visible = False
                        Dim strSQLstring As String = ""

                        strSQLstring = "SELECT ISA_MACHINE_NO" & vbCrLf &
                                    " FROM PS_ISA_MACHINE_NBR WHERE INV_ITEM_ID= '" & srtItemID & "'"

                        Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring, False)
                        If Not ds Is Nothing Then
                            If ds.Tables.Count > 0 Then
                                If ds.Tables(0).Rows.Count > 0 Then
                                    cmbMachineNo.DataSource = ds
                                    cmbMachineNo.DataValueField = "ISA_MACHINE_NO"
                                    cmbMachineNo.DataTextField = "ISA_MACHINE_NO"
                                    cmbMachineNo.DataBind()
                                    cmbMachineNo.Visible = True
                                End If
                            End If
                        End If

                    Next
                Else
                    cmbMachineNo.Visible = False
                    PartNoheader.Visible = False
                    tdPartNo.Visible = False
                End If

                ' Part Number for NONCAT Items and BU='I0515'
                If bIsNoncat = True And currentApp.Session("PARTLIST") = "Y" And IsViewParttxt = True Then
                    tdPartNo.Visible = True
                    txtMachineNo.Visible = True
                    PartNoheader.Visible = True
                End If

                If Not row Is Nothing Then
                    If Not IsDBNull(row("MachineNo")) Then
                        CType(e.Item.FindControl("ddlMachineNo"), DropDownList).SelectedValue = CType(row("MachineNo"), String)
                    End If
                End If

            Catch ex As Exception

            End Try
            ' Attachments font color 

            Try
                If Not row Is Nothing Then
                    If Not IsDBNull(row("FilePath")) Then
                        Dim FileArr As String() = CType(row("FilePath"), String())
                        If Not FileArr.Length = 0 Then
                            CType(e.Item.FindControl("btnAttach"), Button).ForeColor = Color.Green
                        Else
                            CType(e.Item.FindControl("btnAttach"), Button).ForeColor = Color.White
                        End If
                    Else
                        CType(e.Item.FindControl("btnAttach"), Button).ForeColor = Color.White
                    End If
                End If

            Catch ex As Exception

            End Try

        End If

    End Sub

    Protected Sub btnSubmitNot_Click(sender As Object, e As EventArgs)
        Dim fldToUpdate As String = ""
        Dim newValue As String = ""
        Try
            Customvalidator1.Enabled = False
            Customvalidator2.Enabled = False
            CustomValidator3.Enabled = False
            Customvalidator4.Enabled = False
            CompareValidator2.Enabled = False
            RequiredFieldValidator1.Enabled = False
            'Requiredfieldvalidator2.Enabled = False
            Requiredfieldvalidator3.Enabled = False
            RequiredFieldValidator4.Enabled = False
            Requiredfieldvalidator6.Enabled = False
            CustomValidatorWorkOrder.Enabled = False
            CustomValidatorWOCH.Enabled = False

            Dim EmpNotes As String = String.Empty
            EmpNotes = txtrwwNotes.Text
            txtEmpNotesHide.Text = EmpNotes
            'PopulateGridFromHiddenControls()
            Dim iItem As Integer
            'If txtEmpNotesHide.Text <> "" And txtEmpMoteRowItem.Text <> "" Then
            iItem = Convert.ToInt32(txtEmpMoteRowItem.Text)
            'RadGrid Code
            Dim GridItem As GridDataItem = CType(rgCart.Items.Item(iItem), GridDataItem)
            Dim nestedview As GridNestedViewItem = CType(GridItem.ChildItem, GridNestedViewItem)
            CType(nestedview.FindControl("txtNotes"), TextBox).Text = txtEmpNotesHide.Text
            txtrwwNotes.Text = ""
            'bChangedContents = True
            'End If
            'WI: 859: Added for persistant Shopping Cart - Temp
            Dim dt As DataTable = CType(HttpContext.Current.Session("Cart"), DataTable)
            'Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("Itemid") = hdnLNItemID.Value).FirstOrDefault()
            Dim dr As DataRow = dt.AsEnumerable().Where(Function(r) r.Field(Of String)("UniqNum") = hdnUniqNum.Value And r.Field(Of String)("Itemid") = hdnLNItemID.Value).FirstOrDefault()

            dr("UNotes") = EmpNotes
            dt.AcceptChanges()
            fldToUpdate = "ISA_CUST_NOTES"
            newValue = EmpNotes

            'For Each row As DataRow In dt.Rows
            '    If row("ItemId") = hdnLNItemID.Value Then
            '        row("UNotes") = EmpNotes
            '        dt.AcceptChanges()
            '    End If
            'Next
            HttpContext.Current.Session("Cart") = dt
            'WriteToUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"))
            UpdateUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"), hdnLNItemID.Value, fldToUpdate, newValue, hdnUniqNum.Value)

        Catch ex As Exception
            Dim strErrorFromEmail As String = "Error in ShoppingCart.aspx.vb > btnSubmitNot_Click event. User ID: " & HttpContext.Current.Session("USERID") & "; " & vbCrLf &
                "BU: " & HttpContext.Current.Session("BUSUNIT") & " ; Server: " & HttpContext.Current.Session("WEBSITEID") & " :: " & HttpContext.Current.Request.ServerVariables("REMOTE_HOST") & vbCrLf &
               "Error message: " & ex.Message
            SendSDiExchErrorMail(strErrorFromEmail, "Error in ShoppingCart.aspx.vb > btnSubmitNot_Click event.")
        End Try

    End Sub

    Protected Sub cvrDuedate_ServerValidate(source As Object, args As ServerValidateEventArgs)
        Try
            'If DateTime.Compare(Convert.ToDateTime(txtReqByDate.Text), Now.Date) < 0 Then
            If DateTime.Compare(Convert.ToDateTime(args.Value()), Now.Date) < 0 Then
                Customvalidator2.ErrorMessage = "Request by date cannot be less than today"
                Customvalidator2.IsValid = False
                args.IsValid = False
            End If
        Catch
        End Try
    End Sub


    Private Sub CustomValidtrChargeCode_ServerValidate(source As Object, args As ServerValidateEventArgs) Handles CustomValidtrChargeCode.ServerValidate
        Dim I As Integer = 0
        If Session("CHARGECODE") = "Y" Then
            If Not Session("CARTCHGCDREQ") Is Nothing Then
                If (Convert.ToString(Session("CARTCHGCDREQ")) = "1" Or Convert.ToString(Session("CARTCHGCDREQ")).ToLower() = "y") Then
                    Dim bResultChgCd As Boolean = False
                    Dim strChgCdItem As String = ""
                    If Trim(txtChgCD.Text) = "" Then
                        If rgCart.Items.Count > 0 Then
                            For I = 0 To rgCart.Items.Count - 1
                                Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(I), GridDataItem)
                                Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
                                Dim txtChgCdItem As TextBox = CType(nestedview.FindControl("txtItemChgCode"), TextBox)
                                Try
                                    If Trim(txtChgCdItem.Text) = "" Then
                                        bResultChgCd = True
                                        Exit For
                                    Else
                                        strChgCdItem = Trim(txtChgCdItem.Text)
                                    End If
                                Catch ex As Exception
                                    bResultChgCd = True
                                End Try
                            Next
                            If Not bResultChgCd Then
                                'txtChgCD.Text = strChgCdItem
                            Else
                                CustomValidtrChargeCode.ErrorMessage = "Charge Code Required"
                                CustomValidtrChargeCode.IsValid = False
                                args.IsValid = False
                            End If

                        End If
                    End If
                End If
            End If
        End If
    End Sub

    Public Shared Function NONCatalogFileUpload(ByVal Filebytes() As Byte, ByVal FileName As String, ByVal Record As String) As String
        Try
            Dim retrunURL As String = String.Empty
            Dim o As New sdiFileUploader.fileUploaderSoapClient
            Dim soapAuthHdr As New sdiFileUploader.soapAuthenticationHeader
            Dim soapKeyHdr As New sdiFileUploader.soapKeyHeader
            Dim sMisc As String = ""
            Dim retString As String = ""
            Dim strKeysCat() As String = Split(Record, "-")
            Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
            Dim StrEnv As String = ConfigurationSettings.AppSettings("FileAttachment_Environment").ToString.Trim
            Try
                retString = o.uploadFile(soapAuthHdr, soapKeyHdr, "Others", strKeysCat(0), strKeysCat(1), strKeysCat(2), strKeysCat(3), FileName, Filebytes, StrEnv, sMisc, currentApp.Session("BUSUNIT"), currentApp.Session("USERID"))
            Catch ex As Exception
                retString = ""
            End Try

            o = Nothing

            Dim ret As sdiFileUploadResult = New sdiFileUploadResult

            Dim typClass As Type() = New Type() {GetType(sdiFileUploadResult)}
            Dim formatter As New System.Xml.Serialization.XmlSerializer(GetType(sdiFileUploadResult))
            Dim encoder As New System.Text.ASCIIEncoding
            Dim buff As Byte() = New Byte() {}
            Dim memStream As System.IO.MemoryStream = Nothing

            Try
                buff = encoder.GetBytes(retString)
                memStream = New System.IO.MemoryStream(buff)
                ret = CType(formatter.Deserialize(memStream), sdiFileUploadResult)
                Try
                    memStream.Close()
                Catch ex As Exception
                End Try
            Catch ex As Exception
                ' ignore
            End Try

            memStream = Nothing
            buff = Nothing
            encoder = Nothing
            formatter = Nothing
            typClass = Nothing

            retrunURL = ret.FileURL

            Return retrunURL

        Catch ex As Exception
            Return ""
        End Try
    End Function

    Protected Sub btnAttach_Command(sender As Object, e As CommandEventArgs)
        Try
            ' getting the item row index
            Dim Nestedviewitem As GridNestedViewItem = DirectCast(sender, System.Web.UI.WebControls.Button).NamingContainer
            Dim item As GridDataItem = Nestedviewitem.ParentItem
            Dim rowindex As Integer = Convert.ToInt32(CType(item.FindControl("lblrowIndex"), Label).Text)

            ' Getting Attachments name from  session cart
            Dim dt As DataTable = CType(Session("cart"), DataTable)
            hdnrowindex.Value = rowindex
            If Not IsDBNull(dt.Rows(rowindex).Item("FilePath")) Then
                Dim AttachArr As String() = dt.Rows(rowindex).Item("FilePath")
                Dim Arr As String() = (From A In AttachArr Select Path.GetFileName(A.Substring(21))).ToArray() 'yury
                'Dim Arr As String() = (From A In AttachArr Select Path.GetFileName(A)).ToArray()
                RptAttached.DataSource = Arr
                RptAttached.DataBind()
            Else
                Dim Emptydt As DataTable = Nothing
                RptAttached.DataSource = Emptydt
                RptAttached.DataBind()
            End If
            Dim Script As String = "function f(){$find(""" + RWAttachment.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", Script, True)
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub lkbAttached_Command(sender As Object, e As CommandEventArgs)

        Customvalidator1.Enabled = False
        Customvalidator2.Enabled = False
        CustomValidator3.Enabled = False
        Customvalidator4.Enabled = False
        CompareValidator2.Enabled = False
        RequiredFieldValidator1.Enabled = False
        'Requiredfieldvalidator2.Enabled = False
        Requiredfieldvalidator3.Enabled = False
        RequiredFieldValidator4.Enabled = False
        Requiredfieldvalidator6.Enabled = False
        CustomValidatorWorkOrder.Enabled = False
        CustomValidatorWOCH.Enabled = False

        Try
            Dim Operation As String = e.CommandName()
            Dim cmdFileName As String = e.CommandArgument()

            If Operation = "OPEN" Then
                Dim FilePath As String = ConfigurationManager.AppSettings("NonCatalogFilePath") + cmdFileName
                Dim file As New FileInfo(FilePath)
                If file.Exists Then
                    Response.Clear()
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name)
                    Response.AddHeader("Content-Length", file.Length.ToString())
                    Response.ContentType = "application/octet-stream"
                    Response.WriteFile(file.FullName)
                    Response.[End]()
                End If
            ElseIf Operation = "REMOVE" Then
                Dim rowindex As Integer = Convert.ToInt32(hdnrowindex.Value)
                Dim dt As DataTable = CType(Session("cart"), DataTable)
                Dim FilePathArr As String() = CType(dt.Rows(rowindex).Item("FilePath"), String())
                'Dim FileByteArr As Byte()() = CType(dt.Rows(rowindex).Item("FileInByte"), Byte()())
                Dim Arr As String() = (From A In FilePathArr Select Path.GetFileName(A)).ToArray()
                '                Dim Index As Integer = Array.IndexOf(Arr, cmdFileName)         'yury removed
                Dim index = Array.FindIndex(Arr, Function(c As String) c.Contains(cmdFileName)) 'yury added
                cmdFileName = Path.GetFileName(FilePathArr(index).ToString)                     'yury added
                Dim FilePath As New List(Of String)
                '   Dim FileBytes As New List(Of Byte())

                FilePath = FilePathArr.ToList()
                FilePath.RemoveAt(index)

                'FileBytes = FileByteArr.ToList()
                'FileBytes.RemoveAt(Index)
                Try
                    DeleteTempFile(cmdFileName)
                Catch ex As Exception
                End Try


                dt.Rows(rowindex).Item("FilePath") = FilePath.ToArray()
                '  dt.Rows(rowindex).Item("FileInByte") = FileBytes.ToArray()
                If FilePath.Count = 0 Then
                    rgCart.DataSource = dt
                    rgCart.DataBind()
                End If
                'RptAttached.DataSource = FileName.ToArray
                'RptAttached.DataBind()

            End If
        Catch ex As Exception

        End Try


    End Sub

    Protected Sub btPopupnAttach_Click(sender As Object, e As EventArgs)

        Customvalidator1.Enabled = False
        Customvalidator2.Enabled = False
        CustomValidator3.Enabled = False
        Customvalidator4.Enabled = False
        CompareValidator2.Enabled = False
        RequiredFieldValidator1.Enabled = False
        'Requiredfieldvalidator2.Enabled = False
        Requiredfieldvalidator3.Enabled = False
        RequiredFieldValidator4.Enabled = False
        Requiredfieldvalidator6.Enabled = False
        CustomValidatorWorkOrder.Enabled = False
        CustomValidatorWOCH.Enabled = False

        Try
            Dim RowIndex As Integer = Convert.ToInt32(hdnrowindex.Value)
            Dim FileCount As Integer = AsyncUpload1.UploadedFiles.Count()

            Dim FilePath As New List(Of String)
            '  Dim FileByte As New List(Of Byte())()

            Dim dt As DataTable = CType(Session("cart"), DataTable)
            If Not IsDBNull(dt.Rows(RowIndex).Item("FilePath")) Then
                'FilePath = CType(dt.Rows(RowIndex).Item("FileName"), String()).ToList() 'yury
                FilePath = CType(dt.Rows(RowIndex).Item("FilePath"), String()).ToList() 'yury
                '  FileByte = CType(dt.Rows(RowIndex).Item("FileInByte"), Byte()()).ToList()
            End If

            If FileCount > 0 Then
                For Each f As UploadedFile In AsyncUpload1.UploadedFiles
                    ' Temp Store
                    Dim StrFilePath As String = String.Empty
                    Try
                        Dim DTPreFix As String = System.DateTime.Now.ToString("yyMMdd_hhmmss") 'Yury

                        StrFilePath = ConfigurationManager.AppSettings("NonCatalogFilePath").ToString() + DTPreFix + Path.GetFileName(f.FileName) 'yury

                        'yury 20170331 check if exceeding field length
                        Dim strMessage As New Alert
                        Dim testFilePath = If(FilePath.Count = 0, "", String.Join("|", FilePath))
                        testFilePath += "|" + StrFilePath
                        Dim testFilePathLen As Integer = Len(testFilePath)
                        If testFilePathLen > 254 Then
                            ltlAlert.Text = strMessage.Say("You have exceeded the length of total file names. \nPlease adjust your entry or remove longer file names.")
                            Exit Try
                        End If

                        f.SaveAs(StrFilePath)

                        If Not String.IsNullOrEmpty(StrFilePath.Trim()) Then
                            FilePath.Add(StrFilePath)
                        End If
                        'Dim fs As Stream = uploadedFile.InputStream
                        'Dim br As New BinaryReader(fs)
                        'Dim b As Byte() = br.ReadBytes(Convert.ToInt32(fs.Length))
                        'FileName.Add(uploadedFile.FileName)
                        'FileByte.Add(b)

                    Catch ex As Exception
                    End Try

                Next
            End If
            Session("FilePath") = FilePath.ToArray()

            If FilePath.Any() Then
                dt.Rows(RowIndex).Item("FilePath") = FilePath.ToArray()

                '  dt.Rows(RowIndex).Item("FileInByte") = FileByte.ToArray()
                'RptAttached.DataSource = (From A In FileName.ToArray() Select Path.GetFileName(A)).ToArray()
                'RptAttached.DataBind()
                rgCart.DataSource = dt  '20170407 uncommented Yury
                rgCart.DataBind()       '20170407 uncommented Yury
                'WI: 859: Added for persistant Shopping Cart - Temp
                WriteToUserCart(HttpContext.Current.Session("USERID"), HttpContext.Current.Session("BUSUNIT"))
            End If
        Catch ex As Exception
            RptAttached.DataSource = Nothing
        End Try
    End Sub

    Protected Sub DeleteTempFile(ByVal FileName As String, Optional ByVal FileArr As String() = Nothing)
        Try
            Dim StrFileName As String = FileName
            Dim StrDir As String = ConfigurationManager.AppSettings("NonCatalogFilePath").ToString()
            If Not String.IsNullOrEmpty(StrFileName.Trim()) Then
                File.Delete(StrDir + StrFileName)
            Else
                For Each F As String In FileArr
                    If System.IO.File.Exists(StrDir + F) Then
                        File.Delete(StrDir + F) 'missing dir - yury
                    End If
                Next
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub cmdContShop1_Click(sender As Object, e As EventArgs) Handles cmdContShop1.Click
        CheckContinueShopping()
        Response.Redirect(Session("DEFAULTPAGE").ToString(), True)
    End Sub

    Private Sub CheckContinueShopping()
        Dim confirmValue As String = Request.Form("confirm_value")
        If confirmValue = "Yes" Then
            If Not Session("Cart") Is Nothing Then
                updateCartDatatable()

                txtForContShop.Text = "YES"
            End If
        Else

        End If
    End Sub

    Private Sub cmdContShop2_Click(sender As Object, e As EventArgs) Handles cmdContShop2.Click
        CheckContinueShopping()
        Response.Redirect(Session("DEFAULTPAGE").ToString(), True)
    End Sub

    Private Sub cmdEdtMyFavs_Click(sender As Object, e As EventArgs) Handles cmdEdtMyFavs.Click
        CheckContinueShopping()
        Response.Redirect("favorites.aspx?FP=SC", True)
    End Sub

    Protected Sub lnkbtnBack_Click(sender As Object, e As EventArgs)
        Try
            Response.Redirect("SearchResults.aspx", True)
        Catch ex As Exception

        End Try
    End Sub

    Private Function CheckOrderPriority(Optional ByVal dstCart As DataTable = Nothing) As Boolean
        Dim i As Integer = 0

        CheckOrderPriority = False
        If chbPriority.Checked Then
            CheckOrderPriority = True
        Else  'chbPriority is not checked
            If dstCart Is Nothing Then
                For i = 0 To rgCart.Items.Count - 1
                    Dim GridItem As GridDataItem = DirectCast(rgCart.Items.Item(i), GridDataItem)
                    Dim nestedview As GridNestedViewItem = DirectCast(GridItem.ChildItem, GridNestedViewItem)
                    Dim chkPriority As CheckBox = CType(nestedview.FindControl("chkPriority"), CheckBox)
                    If chkPriority.Checked Then
                        CheckOrderPriority = True
                        Exit For
                    End If
                Next
            Else
                '   dstCart Is NOT Nothing Then
                If dstCart.Rows.Count > 0 Then
                    For i = 0 To dstCart.Rows.Count - 1
                        Dim row As DataRow = dstCart.Rows(i)
                        Try
                            If UCase(Trim(row.Item("UPriority"))) = "R" Then
                                CheckOrderPriority = True
                                Exit For
                            End If
                        Catch ex As Exception

                        End Try
                    Next  '  For i = 0 To dstCart.Rows.Count - 1
                End If  ' If dstCart.Rows.Count > 0 Then
            End If  '   If dstCart Is Nothing Then

        End If

    End Function

    Private Sub RadDatePickerReqByDate_SelectedDateChanged(sender As Object, e As Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs) Handles RadDatePickerReqByDate.SelectedDateChanged
        updateCartDatatable()
    End Sub

    Private Sub txtChgCD_TextChanged(sender As Object, e As EventArgs) Handles txtChgCD.TextChanged
        updateCartDatatable()
    End Sub

    Private Sub txtWorkOrder_TextChanged(sender As Object, e As EventArgs) Handles txtWorkOrder.TextChanged

        If Trim(txtWorkOrder.Text) <> "" Then
            ' check is Enterprise flag set
            If Not Session("ValidateWorkOrder") Is Nothing Then
                If (Convert.ToString(Session("ValidateWorkOrder")) = "W") Then

                    'validate is work order in the list
                    Dim bExists As Boolean = False
                    bExists = CheckIsWoExists()

                    If Not bExists Then

                        Dim strMessage As New Alert
                        ltlAlert.Text = strMessage.Say("Warning!!  This work order number is not currently valid. \n Order submission will still be allowed.")

                    End If

                End If

            End If

        End If

    End Sub

    Protected Sub btnhidden_Click(sender As Object, e As EventArgs)
        Dim bolIsValidOrder As Boolean = False
        Session("ShopCWrkOrd") = txtWorkOrder.Text
        ButtonClickEvent()

        If Not Session("CONFTARGET") = "" Then
            Response.Redirect(Session("CONFTARGET"))
        End If

    End Sub
End Class

