Imports Insiteonline.ORDBData
Imports Insiteonline.WebPartnerFunctions.WebPSharedFunc
Imports Insiteonline.clsPODetails
Imports System.Drawing.Color
Imports Telerik.Web.UI
Imports Insiteonline.clsCrystalReports
Imports System.Collections.Generic
Imports System.Data.OleDb
Imports System
Imports System.Security.Cryptography
Imports System.IO
Imports System.Linq
Imports System.Web.Mail
Imports System.Text
Imports Newtonsoft.Json
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Net
Imports System.Threading.Tasks
Imports System.Web.Script.Serialization

Public Class PODetails
    Inherits System.Web.UI.Page
    Implements IUserInterface
    Private m_logger As ApplicationLogger = Nothing

    ''ASN global varibles
    Public dsShipVia As DataSet
    Public dsPOdata As DataSet
    Public bolDBUpdErr As Boolean = False
    Public dteNowd As Date = CDate(Date.Now().ToString("d"))
    Public dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
    Public dteNowy As String = Now().ToString("yyyy-M-d")
    Dim Row1_default_Track_no As String = " "
    Dim Row1_default_Ship_dt As String = " "
    Dim Row1_default_Ship_VIA As String = "COMMON"
    Dim Row1_default_Ship_VIA_ID As String = " "
    Dim bIsValidInv As Boolean = False

    ''Voucher global variables 

    Private Const VIEWSTATE_PARENT_PAGE As String = "__viewStateForReturnToParent"

    Private m_oledbCNString As String = ConfigurationSettings.AppSettings("OLEDBconString")
    Private m_urlLoginPage As String = ConfigurationSettings.AppSettings("loginPage")
    Private m_urlParent As String = ConfigurationSettings.AppSettings("defaultPage")
    Private m_uomDefaultId As String = ConfigurationSettings.AppSettings("uomDefaultId")
    Private m_urlMe As String = "/InSiteOnlineVoucherEntry/VoucherEntry.aspx"
    Private m_pathSQLs As String = ""
    Private m_vendorInfo As Vendor = Nothing
    Private m_selectedPO As PO = Nothing
    Private m_voucher As Voucher = Nothing
    Private m_mode As mySessionState.eMode = mySessionState.eMode.evReadOnly
    Private m_invoiceExtendedPrice As Double = CDbl("0")
    Private m_invoiceExtendedPrice1 As Double = CDbl("0")
    Private m_invoiceMiscCharges As Double = CDbl("0")

    Private javascriptOnFocus As String() = New String() {"onfocus", "this.select();"}
    Private m_currentMiscChargeList As New ArrayList

    '' Encryption variables
    Private key() As Byte = {}
    Private IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}
    Private sDefaultKey As String = "#sdi.default"

    '' Voucher grid column
    Private Enum eVoucherGridColumn As Integer
        evKey = 0
        evLineNo = 1
        evSchedLineNo = 2
        evSDIItemId = 3
        evMfgItemId = 4
        evVendorItemId = 5
        evItemDescInvoice = 6
        evQtyPO = 7
        evUnitPricePO = 8
        evExtendedPricePO = 9
        evPriorInvoicedQty = 10
        evQtyInvoice = 11
        evUOMInvoice = 12
        evUnitPriceInvoice = 13
        evExtendedPriceInvoice = 14
        evEditUpdateCancel = 15
        evRemove = 16
    End Enum


    Protected Overrides Sub OnPreInit(ByVal e As EventArgs)
        MyBase.OnPreInit(e)

        If Not Request.QueryString("POID") Is Nothing Then
            Dim UtilityPO As String = Request.QueryString("POID")
            If Not UtilityPO.Trim = "" Then
                Me.Page.MasterPageFile = "~/MasterPage/ISOLOnlineVendorBlank.Master"
                '' Me.Page.MasterPageFile = "MasterPage\ISOLOnlineVendor.Master"
            Else
                Me.Page.MasterPageFile = "~/MasterPage/ISOLOnlineVendor.Master"
            End If
        Else
            Me.Page.MasterPageFile = "~/MasterPage/ISOLOnlineVendor.Master"
        End If

    End Sub

    Private Sub Page_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.PreRender
        If tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled And tbStripPODetails.Tabs.FindTabByValue("POVENT").Selected Then

            If Not mySessionState.VendorPOs.Count = 0 Then
                PopulateVoucherDataGrid()

                Dim EditItemIndex As Integer = -1
                If Me.grid_Voucher.EditItems.Count > 0 Then
                    For Each item As GridEditableItem In Me.grid_Voucher.EditItems
                        If item.IsInEditMode Or item.Edit Then
                            EditItemIndex = item.ItemIndex
                        End If
                    Next
                End If

                If EditItemIndex = -1 Then
                    ' set SAVE button state
                    '   button should only be enabled when NO LINE is in edit mode
                    Me.btnVoucSave.Enabled = True

                    ' set SUBMIT button state
                    '   button should only be enabled when NO LINE is in edit mode
                    Me.btnVoucSubmit.Enabled = True
                End If

                '' set SAVE button state
                ''   button should only be enabled when NO LINE is in edit mode
                'Me.btnVoucSave.Enabled = (Me.gridVoucher.EditItemIndex = -1 And _
                '                      Me.gridVoucher.Items.Count > 0)
                '' set SUBMIT button state
                ''   button should only be enabled when NO LINE is in edit mode
                'Me.btnVoucSubmit.Enabled = (Me.gridVoucher.EditItemIndex = -1 And _
                '                        Me.gridVoucher.Items.Count > 0)
            End If
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Session("SCREENNAME") = "PODetails.aspx"
        Me.Title = "PO Details"

        Dim lblTitle As Label = CType(Master.FindControl("lblVendor"), Label)
        If Not lblTitle Is Nothing Then
            Try
                lblTitle.Text = "PO Details for " & "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
            Catch ex As Exception

            End Try
        Else
            Dim lbl_Title As Label = CType(Master.FindControl("lblTitle"), Label)
            lbl_Title.Text = "PO Details " & "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;"
        End If

        Dim strPO As String = ""
        Dim strVendorID As String = ""
        Dim strPODetailTap As String = ""
        Dim strPOBU As String = ""
        Dim strOPRID As String = ""
        Try
            dpInvoiceDate.MinDate = DateTime.Now.AddDays(-3)
        Catch ex As Exception
        End Try
        Try
            '' Session("POFROM") is to tell whether the PO from dashborad or from utility
            If Request.QueryString("") Is Nothing Then
            Else

            End If
            If Not Request.QueryString("POID") Is Nothing Then
                If Not Request.QueryString("POID").Trim = "" Then
                    strPO = Request.QueryString("POID").Trim
                    If strPO.Contains(" ") Then
                        strPO = Replace(strPO, " ", "+")
                    End If

                    strPO = Decrypt(strPO, "bautista")
                    Session("ISEMAILFROMLINK") = "Y"
                End If

                If Not Request.QueryString("vendorid").Trim = "" Then
                    strVendorID = Request.QueryString("vendorid").Trim
                    If strVendorID.Contains(" ") Then
                        strVendorID = Replace(strVendorID, " ", "+")
                    End If
                    strVendorID = Decrypt(strVendorID, "bautista")
                End If

                If Not Request.QueryString("PO_BU").Trim = "" Then
                    strPOBU = Request.QueryString("PO_BU").Trim
                    If strPOBU.Contains(" ") Then
                        strPOBU = Replace(strPOBU, " ", "+")
                    End If
                    strPOBU = Decrypt(strPOBU, "bautista")
                End If

                If Not Request.QueryString("OPR_ID").Trim = "" Then
                    strOPRID = Request.QueryString("OPR_ID").Trim
                    If strOPRID.Contains(" ") Then
                        strOPRID = Replace(strOPRID, " ", "+")
                    End If
                    strOPRID = Decrypt(strOPRID, "bautista")
                End If

                If Not strPO.Trim = "" And Not strPOBU.Trim = "" And Not strVendorID.Trim = "" And Not strOPRID.Trim = "" Then
                    Session("PODetailViewPO") = strPO.Trim
                    Session("PODetailViewBU") = strPOBU.Trim
                    Session("VENDOR_VP") = strVendorID.Trim
                    Session("OPERID") = strOPRID.Trim
                    Session("POFROM") = "POUTY"
                    lblPOHeaderInfo.Visible = False
                    divPOSearch.Visible = False
                Else
                    lblPOHeaderInfo.Visible = True
                    lblPOHeaderInfo.Text = "*Unable to find the given PO number. Please contact SDI"
                    AllControlles("HIDE")
                    Exit Sub
                End If
            ElseIf Not Request.QueryString("HOME") Is Nothing Then
                Session("POFROM") = "POHME"
            ElseIf Not Session("PODetailViewPO") Is Nothing Then
                Session("POFROM") = "PODAB"
            End If

            If Session("POFROM") Is Nothing Then
                '' Access the PO details panel through the home menu link

            End If
            If Not IsPostBack Then

                If Session("POFROM") = "POHME" Then
                    clearPODetailsSessions()
                End If
                '' PO from session or from the utility will goes here 
                If Not Session("PODetailViewPO") Is Nothing Then
                    strPO = Session("PODetailViewPO")
                End If

                If Not strPO.Trim = "" Then
                    LoadPOHeaderDetails()
                Else
                    '' Direct detail page view initial panel load hide the all controlles except the PO text box for searching
                    AllControlles("HIDE")
                End If

            End If

            If Not Page.IsPostBack Then
                Insiteonline.WebPartnerFunctions.WebPSharedFunc.WebLogVendor()
            End If

            'VR 10/10/17
            txtSmallSite.Text = "Y"
            '  getSmallSiteFlag(txtPO.Text)
            txtasnflag.Text = getASNFlag(txtPO.Text)

        Catch ex As Exception

        End Try
    End Sub

    Public Function Decrypt(ByVal stringToDecrypt As String, ByVal sEncryptionKey As String) As String
        Try
            Dim inputByteArray(stringToDecrypt.Length) As Byte
            Dim sKey As String = sEncryptionKey.Trim

            If sKey.Length > 0 Then
                If sKey.Length < 8 Then
                    sKey = sKey.PadRight(8, CType("%", Char))
                End If
            Else
                sKey = sDefaultKey
            End If

            key = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8))

            Dim des As New DESCryptoServiceProvider

            inputByteArray = Convert.FromBase64String(stringToDecrypt)

            Dim ms As New MemoryStream
            Dim cs As New CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write)

            cs.Write(inputByteArray, 0, inputByteArray.Length)
            cs.FlushFinalBlock()

            Return System.Text.Encoding.UTF8.GetString(ms.ToArray())

        Catch e As Exception
            Return ""
        End Try
    End Function

    Public Sub clearPODetailsSessions()
        Session("PODetailViewPO") = Nothing
        Session("PODetailViewBU") = Nothing
        'Session("VENDOR_VP") = Nothing
        'Session("OPERID") = Nothing
    End Sub
    Private Sub btnBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBack.Click

        Response.Redirect("SDIVendor.aspx")

    End Sub

    Public Sub AllControlles(strState As String)
        If strState = "SHOW" Then
            divPOinfo2.Visible = True
            divPOinfo1.Visible = True
            If Session("POFROM") = "POUTY" Then
                divPOSearch.Visible = False
                divPOLink.Visible = True
            Else
                divPOSearch.Visible = True
                divPOLink.Visible = False
            End If
            Table1.Visible = True
        Else
            divPOinfo2.Visible = False
            divPOinfo1.Visible = False
            If Not Session("POFROM") = "POHME" Then
                divPOSearch.Visible = False
                divPOLink.Visible = False
            Else
                divPOSearch.Visible = True
                divPOLink.Visible = False
            End If
            Table1.Visible = False
        End If
    End Sub

    Public Sub LoadPOHeaderDetails()
        Dim strPO As String = ""
        Dim strVendorID As String = ""
        Dim strPODetailTap As String = ""
        Dim dtpo As DataTable = Nothing
        Dim strPOBU As String = ""
        Dim m_pathSQLs As String = System.IO.Path.GetDirectoryName(path:=Request.ServerVariables("PATH_TRANSLATED")) &
                                    "\InsiteOnlineVoucherEntry\SQLs\"
        Try
            strPO = Session("PODetailViewPO")
            strPODetailTap = Session("PODetailTap")
            strVendorID = Session("VENDOR_VP")
            strPOBU = Session("PODetailViewBU")
            txtPO.Text = strPO
            lbltxtPO.Text = strPO
            txtPO_Single.Text = strPO
            lblPOnumber_emaillink.Text = strPO
            txtPOBU.Text = Session("PODetailViewBU")
            txtVendor.Text = Session("VENDOR_VP")

            If Not strPO.Trim = "" Then
                If Session("POFROM") = "POUTY" Then
                    '' Return po details from query
                    dtpo = SinglePODetail(strPO, strVendorID, m_pathSQLs)
                Else
                    '' Retunr po details from session store in dashborad page
                    dtpo = CheckPOFromVendorSessionListPOs(strPO)
                End If
                If Not dtpo Is Nothing Then
                    If dtpo.Rows.Count = 0 Then
                        lblPOHeaderInfo.Visible = True
                        lblPOHeaderInfo.Text = "*Unable to find the given PO number. Please contact SDI"
                        AllControlles("HIDE")
                        Exit Sub
                    Else
                        AllControlles("SHOW")

                        PODetailsTabEnable(dtpo)

                        If Session("POFROM") = "POUTY" Then
                            getPODataSearch(strPO, strVendorID, strPOBU)
                        Else
                            getPOData(strPO, strVendorID, strPOBU, strPODetailTap)
                        End If

                        getShipToDetails(strPO, strVendorID)
                        getBuyerDetails(strPO, strPOBU)
                        getPOPriceLineInfo(strPO)
                        If strPOBU = "WAL00" Then
                            Dim workOrdExists As Boolean = getPOWorkOrdNum(strPO)
                        End If
                    End If
                Else
                    lblPOHeaderInfo.Visible = True
                    lblPOHeaderInfo.Text = "*Unable to find the given PO number. Please contact SDI"
                    AllControlles("HIDE")
                    Exit Sub
                End If
            Else
                AllControlles("HIDE")
                lblPOHeaderInfo.Visible = True
                lblPOHeaderInfo.Text = "*Unable to find the given PO number. Please contact SDI"
                Exit Sub
            End If

        Catch ex As Exception

        End Try
    End Sub

    ''Buyer Info
    Private Sub getBuyerDetails(strPONum As String, strPOBU As String)
        Try
            Dim dtrPOds As DataSet = getBuyerInfo(strPONum, strPOBU)

            If Not dtrPOds.Tables(0).Rows.Count = 0 Then
                txtBuyerInfo.Text = ""
                Dim strBuyerName As String = dtrPOds.Tables(0).Rows(0).Item(0)
                Dim strEmail As String = dtrPOds.Tables(0).Rows(0).Item(3)
                Dim strPhone As String = dtrPOds.Tables(0).Rows(0).Item(1)
                Dim strDept As String = dtrPOds.Tables(0).Rows(0).Item(4)
                Dim strFax As String = dtrPOds.Tables(0).Rows(0).Item(2)

                txtBuyerInfo.Text = txtBuyerInfo.Text + If(strBuyerName.Trim = "", "", "Name : " + dtrPOds.Tables(0).Rows(0).Item(0) + vbCrLf)
                txtBuyerInfo.Text = txtBuyerInfo.Text + If(strEmail.Trim = "", "", "Email : " + dtrPOds.Tables(0).Rows(0).Item(3) + vbCrLf)
                txtBuyerInfo.Text = txtBuyerInfo.Text + If(strPhone.Trim = "", "", "Phone : " + dtrPOds.Tables(0).Rows(0).Item(1) + vbCrLf)
                txtBuyerInfo.Text = txtBuyerInfo.Text + If(strDept.Trim = "", "", "Department ID : " + dtrPOds.Tables(0).Rows(0).Item(4) + vbCrLf)
                txtBuyerInfo.Text = txtBuyerInfo.Text + If(strFax.Trim = "", "", "Fax : " + dtrPOds.Tables(0).Rows(0).Item(2))

            Else
                txtBuyerInfo.Visible = False
            End If
        Catch ex As Exception

        End Try
    End Sub

    ''Shipto Info
    Private Sub getShipToDetails(strPONum As String, strVendor As String)
        Try
            Dim dtrPOds As DataSet = getshipto(strPONum, strVendor)

            If Not dtrPOds.Tables(0).Rows.Count = 0 Then
                txtShiptoID.Text = ""
                txtShiptoID.Text = dtrPOds.Tables(0).Rows(0).Item(0)
                txtShipTO.Text = txtShipTO.Text + dtrPOds.Tables(0).Rows(0).Item(1) + vbCrLf
                txtShipTO.Text = txtShipTO.Text + dtrPOds.Tables(0).Rows(0).Item(2) + vbCrLf
                If Not dtrPOds.Tables(0).Rows(0).Item(3) = " " Then
                    txtShipTO.Text = txtShipTO.Text + dtrPOds.Tables(0).Rows(0).Item(3) + vbCrLf
                End If
                If Not dtrPOds.Tables(0).Rows(0).Item(4) = " " Then
                    txtShipTO.Text = txtShipTO.Text + dtrPOds.Tables(0).Rows(0).Item(4) + vbCrLf
                End If
                If Not dtrPOds.Tables(0).Rows(0).Item(5) = " " Then
                    txtShipTO.Text = txtShipTO.Text + dtrPOds.Tables(0).Rows(0).Item(5) + vbCrLf
                End If
                txtShipTO.Text = txtShipTO.Text + dtrPOds.Tables(0).Rows(0).Item(6) + " "
                txtShipTO.Text = txtShipTO.Text + dtrPOds.Tables(0).Rows(0).Item(7) + " "
                txtShipTO.Text = txtShipTO.Text + dtrPOds.Tables(0).Rows(0).Item(8)
            Else
                txtShipTO.Visible = False
            End If
        Catch ex As Exception

        End Try
    End Sub

    ''PO Total line and price info

    Private Sub getPOPriceLineInfo(strPONum As String)
        Dim strSQLQuery As String = ""
        Dim ds As DataSet
        Try
            strSQLQuery = "SELECT COUNT(*) AS TOTAL_LINE,SUM(PRICE_PO * QTY_PO) AS TOTAL_PRICE FROM PS_PO_LINE_SHIP WHERE PO_ID='" + strPONum + "'"
            ds = ORDBData.GetAdapter(strSQLQuery)

            If Not ds Is Nothing Then
                If Not ds.Tables(0).Rows.Count = 0 Then
                    lblPOLinestxt.Text = ds.Tables(0).Rows(0).Item(0)
                    lblPOTotaltxt.Text = FormatNumber(ds.Tables(0).Rows(0).Item(1), 2)
                Else
                    lblPOLinestxt.Visible = False
                    lblPOTotaltxt.Visible = False
                End If
            Else
                lblPOLinestxt.Visible = False
                lblPOTotaltxt.Visible = False
            End If

        Catch ex As Exception
            lblPOLinestxt.Visible = False
            lblPOTotaltxt.Visible = False
        End Try

    End Sub

    '' Enable PODetails tab's based on the session PO naviaget from dashboard
    Public Sub getPOData(ByVal strPO As String, ByVal strVendorID As String, ByVal strPOBU As String, ByVal bindType As String)
        Dim I As Integer
        Try
            lblPOHeaderInfo.Visible = False
            If bindType = "POCONF" Then
                If Session("NeedDueDateUpdate") = "YES" Then
                    RadMultiPage1.SelectedIndex = 0
                    tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = True
                    tbStripPODetails.Tabs.FindTabByValue("POCON").Selected = True
                    btnSubmit.Text = "Update Due Date"
                    ''    tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO DueDate Update"
                    getPOConfirmDetails(strPO, strVendorID, strPOBU)
                    lblCurrentTab.Text = "*PO Due Date Update*"
                    For I = 0 To dtgPO.Items.Count - 1
                        Dim item As GridDataItem = dtgPO.Items(I)
                        CType(item.FindControl("chkConfirm"), CheckBox).Checked = True
                        CType(item.FindControl("chkConfirm"), CheckBox).Enabled = False
                    Next
                Else
                    RadMultiPage1.SelectedIndex = 0
                    tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Confirm"
                    getPOConfirmDetails(strPO, strVendorID, strPOBU)
                    lblCurrentTab.Text = "*PO Confirm*"
                End If

            ElseIf bindType = "POASN" Then

                RadMultiPage1.SelectedIndex = 1
                tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled = True
                tbStripPODetails.Tabs.FindTabByValue("POASN").Selected = True
                PopulateDropDownList()
                getPOASNDetails(strPO, strVendorID, strPOBU)
                lblCurrentTab.Text = "*ASN*"

            ElseIf bindType = "POINV" Then

                RadMultiPage1.SelectedIndex = 2
                tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled = True
                tbStripPODetails.Tabs.FindTabByValue("POVENT").Selected = True
                getPOVoucherDetails(strPO, strVendorID, strPOBU)
                lblCurrentTab.Text = "*Invoice Entry*"
            ElseIf bindType = "PODUE" Then

            Else
                tbStripPODetails.Visible = False
                lblPOHeaderInfo.Visible = True
                lblPOHeaderInfo.Text = "*Can't able to find the PO details.please contact SDI"
            End If

        Catch ex As Exception

        End Try
    End Sub

    '' Enable PODetails tab's based on the single PO searched in detail page
    Public Sub getPODataSearch(ByVal strPO As String, ByVal strVendorID As String, ByVal strPOBU As String)

        Try
            If Not Session("POFROM") = "POHOME" Then
                If Request.QueryString("home") = "POC" Then
                    If tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled Then
                        RadMultiPage1.SelectedIndex = 0
                        If Session("NeedDueDateUpdate") = "YES" Then
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = True
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Selected = True
                            btnSubmit.Text = "Update Due Date"
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Due Date Update"
                            getPOConfirmDetails(strPO, strVendorID, strPOBU)
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Selected = True
                            lblCurrentTab.Text = "*PO Due Date Update*"

                        Else
                            btnSubmit.Text = "Confirm"
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Confirm"
                            getPOConfirmDetails(strPO, strVendorID, strPOBU)
                            lblCurrentTab.Text = "*PO Confirm*"
                        End If
                    Else
                        If tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled Then
                            RadMultiPage1.SelectedIndex = 1
                            PopulateDropDownList()
                            getPOASNDetails(strPO, strVendorID, strPOBU)
                        ElseIf tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled Then
                            RadMultiPage1.SelectedIndex = 2
                            getPOVoucherDetails(strPO, strVendorID, strPOBU)
                        Else
                            tbStripPODetails.Visible = False
                        End If
                    End If
                ElseIf Request.QueryString("home") = "POASN" Then
                    If tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled Then
                        RadMultiPage1.SelectedIndex = 1
                        PopulateDropDownList()
                        getPOASNDetails(strPO, strVendorID, strPOBU)
                        tbStripPODetails.Tabs.FindTabByValue("POASN").Selected = True

                    Else
                        If tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled Then
                            RadMultiPage1.SelectedIndex = 0
                            If Session("NeedDueDateUpdate") = "YES" Then
                                tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = True
                                tbStripPODetails.Tabs.FindTabByValue("POCON").Selected = True
                                btnSubmit.Text = "Update Due Date"
                                tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Due Date Update"
                                getPOConfirmDetails(strPO, strVendorID, strPOBU)
                                lblCurrentTab.Text = "*PO Due Date Update*"
                            Else
                                btnSubmit.Text = "Confirm"
                                tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Confirm"
                                getPOConfirmDetails(strPO, strVendorID, strPOBU)
                                lblCurrentTab.Text = "*PO Confirm*"
                            End If
                        ElseIf tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled Then
                            RadMultiPage1.SelectedIndex = 2
                            getPOVoucherDetails(strPO, strVendorID, strPOBU)
                            'ElseIf tbStripPODetails.Tabs.FindTabByValue("PODUE").Enabled And tbStripPODetails.Tabs.FindTabByValue("PODUE").Selected Then
                            '    RadMultiPage1.SelectedIndex = 3
                        Else
                            tbStripPODetails.Visible = False
                        End If
                    End If
                ElseIf Request.QueryString("home") = "POVEN" Then
                    If tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled Then
                        RadMultiPage1.SelectedIndex = 2
                        getPOVoucherDetails(strPO, strVendorID, strPOBU)
                        tbStripPODetails.Tabs.FindTabByValue("POVENT").Selected = True

                    Else
                        If tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled Then
                            RadMultiPage1.SelectedIndex = 0
                            If Session("NeedDueDateUpdate") = "YES" Then
                                tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = True
                                tbStripPODetails.Tabs.FindTabByValue("POCON").Selected = True
                                btnSubmit.Text = "Update Due Date"
                                tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Due Date Update"
                                getPOConfirmDetails(strPO, strVendorID, strPOBU)
                                lblCurrentTab.Text = "*PO Due Date Update*"
                            Else
                                btnSubmit.Text = "Confirm"
                                tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Confirm"
                                getPOConfirmDetails(strPO, strVendorID, strPOBU)
                                lblCurrentTab.Text = "*PO Confirm*"
                            End If
                        ElseIf tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled Then
                            RadMultiPage1.SelectedIndex = 1
                            PopulateDropDownList()
                            getPOASNDetails(strPO, strVendorID, strPOBU)
                        ElseIf tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled Then
                            RadMultiPage1.SelectedIndex = 2
                            getPOVoucherDetails(strPO, strVendorID, strPOBU)
                            'ElseIf tbStripPODetails.Tabs.FindTabByValue("PODUE").Enabled And tbStripPODetails.Tabs.FindTabByValue("PODUE").Selected Then
                            '    RadMultiPage1.SelectedIndex = 3
                        Else
                            tbStripPODetails.Visible = False
                        End If
                    End If
                Else
                    If tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled And tbStripPODetails.Tabs.FindTabByValue("POCON").Selected Then
                        RadMultiPage1.SelectedIndex = 0
                        If Session("NeedDueDateUpdate") = "YES" Then
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = True
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Selected = True
                            btnSubmit.Text = "Update Due Date"
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Due Date Update"
                            getPOConfirmDetails(strPO, strVendorID, strPOBU)
                            lblCurrentTab.Text = "*PO Due Date Update*"
                        Else
                            btnSubmit.Text = "Confirm"
                            tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Confirm"
                            getPOConfirmDetails(strPO, strVendorID, strPOBU)
                            lblCurrentTab.Text = "*PO Confirm*"
                        End If
                    ElseIf tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled And tbStripPODetails.Tabs.FindTabByValue("POASN").Selected Then
                        RadMultiPage1.SelectedIndex = 1
                        PopulateDropDownList()
                        getPOASNDetails(strPO, strVendorID, strPOBU)
                    ElseIf tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled And tbStripPODetails.Tabs.FindTabByValue("POVENT").Selected Then
                        RadMultiPage1.SelectedIndex = 2
                        getPOVoucherDetails(strPO, strVendorID, strPOBU)
                        'ElseIf tbStripPODetails.Tabs.FindTabByValue("PODUE").Enabled And tbStripPODetails.Tabs.FindTabByValue("PODUE").Selected Then
                        '    RadMultiPage1.SelectedIndex = 3
                    Else
                        tbStripPODetails.Visible = False
                    End If
                End If
            Else
                If tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled And tbStripPODetails.Tabs.FindTabByValue("POCON").Selected Then
                    RadMultiPage1.SelectedIndex = 0
                    If Session("NeedDueDateUpdate") = "YES" Then
                        tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = True
                        tbStripPODetails.Tabs.FindTabByValue("POCON").Selected = True
                        btnSubmit.Text = "Update Due Date"
                        tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Due Date Update"
                        getPOConfirmDetails(strPO, strVendorID, strPOBU)
                        lblCurrentTab.Text = "*PO Due Date Update*"
                    Else
                        btnSubmit.Text = "Confirm"
                        tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Confirm"
                        getPOConfirmDetails(strPO, strVendorID, strPOBU)
                        lblCurrentTab.Text = "*PO Confirm*"
                    End If
                ElseIf tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled And tbStripPODetails.Tabs.FindTabByValue("POASN").Selected Then
                    RadMultiPage1.SelectedIndex = 1
                    PopulateDropDownList()
                    getPOASNDetails(strPO, strVendorID, strPOBU)
                ElseIf tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled And tbStripPODetails.Tabs.FindTabByValue("POVENT").Selected Then
                    RadMultiPage1.SelectedIndex = 2
                    getPOVoucherDetails(strPO, strVendorID, strPOBU)
                    'ElseIf tbStripPODetails.Tabs.FindTabByValue("PODUE").Enabled And tbStripPODetails.Tabs.FindTabByValue("PODUE").Selected Then
                    '    RadMultiPage1.SelectedIndex = 3
                Else
                    tbStripPODetails.Visible = False
                End If
            End If


        Catch ex As Exception

        End Try
    End Sub

    Protected Sub tbStripPODetails_TabClick(sender As Object, e As RadTabStripEventArgs)
        Dim strPO As String = ""
        Dim strVendorID As String = ""
        Dim strPODetailTap As String = ""
        Dim dtpo As DataTable = Nothing
        Dim strPOBU As String = ""
        RadMultiPage1.RenderSelectedPageOnly = True
        RadMultiPage1.SelectedIndex = tbStripPODetails.SelectedIndex
        ''System.Threading.Thread.Sleep(1000)
        Dim position As New AjaxLoadingPanelBackgroundPosition()
        position = AjaxLoadingPanelBackgroundPosition.Center
        RadAjaxLoadingPanel1.BackgroundTransparency = 25
        lblMessage.Text = ""
        strPO = txtPO.Text
        strPODetailTap = Session("PODetailTap")
        strVendorID = Session("VENDOR_VP")
        strPOBU = txtPOBU.Text
        Select Case e.Tab.Value
            Case "POCON"

                getPOConfirmDetails(strPO, strVendorID, strPOBU)
                Exit Select
            Case "POASN"

                PopulateDropDownList()
                getPOASNDetails(strPO, strVendorID, strPOBU)
                Exit Select
            Case "POVENT"

                getPOVoucherDetails(strPO, strVendorID, strPOBU)
                Exit Select
        End Select
    End Sub

    Private Sub btnSearchPO_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSearchPO.Click
        Dim strPO As String = ""
        Dim dtpo As DataTable = Nothing
        Dim list1 = New List(Of KeyValuePair(Of String, Boolean))
        Dim strConfPO As String = "NO"
        Dim strASNPO As String = "NO"
        Dim strVoucherPO As String = "NO"
        Dim strDueDatePO As String = "NO"
        Dim isconfirmNeed As Boolean = False
        Dim isASNNeed As Boolean = False
        Dim isVoucherNeed As Boolean = False
        Dim isDueNeed As Boolean = False
        Dim isEnableNeed As Boolean = False
        Dim strBU As String = ""
        Dim strVendorID As String = Session("VENDOR_VP")
        txtVendor.Text = strVendorID

        'Added the requested path to the session to pass as parameter for CheckPOFromVendorSessionListPOs ()

        m_pathSQLs = System.IO.Path.GetDirectoryName(path:=Request.ServerVariables("PATH_TRANSLATED")) &
                                    "\InsiteOnlineVoucherEntry\SQLs\"
        mySessionState.pathSQLs = m_pathSQLs



        Try
            strPO = txtPO_Single.Text.Trim
            If strPO = "" Then
                divPOinfo2.Visible = False
                divPOinfo1.Visible = False
                Table1.Visible = False
                lblCurrentTab.Visible = False
                lblSinglePOInfo.Visible = True
                lblSinglePOInfo.Text = "*Please enter a PO number to search"
                Exit Sub
            Else
                dtpo = CheckPOFromVendorSessionListPOs(strPO, m_pathSQLs)

                'Hide the panels if the PO number is not valid

                If Not dtpo Is Nothing Then
                    If dtpo.Rows.Count = 0 Then
                        lblSinglePOInfo.Visible = True
                        lblSinglePOInfo.Text = "*Please enter a valid PO number"
                        tbStripPODetails.Visible = False
                        divPOinfo2.Visible = False
                        divPOinfo1.Visible = False
                        RadAjaxManager1.Visible = False
                        RadMultiPage1.Visible = False
                        lblCurrentTab.Visible = False
                        Exit Sub
                    Else

                        tbStripPODetails.Visible = True
                        divPOinfo2.Visible = True
                        divPOinfo1.Visible = True
                        RadAjaxManager1.Visible = True
                        RadMultiPage1.Visible = True
                        lblCurrentTab.Visible = True

                        PODetailsTabEnable(dtpo)
                        If Session("POFullConfirm") = "YES" Then
                            tbStripPODetails.Visible = False
                            divPOinfo2.Visible = False
                            divPOinfo1.Visible = False
                            RadAjaxManager1.Visible = False
                            RadMultiPage1.Visible = False
                            lblCurrentTab.Visible = False
                            Exit Sub
                        End If
                        strBU = getBUForPO(strPO)
                        txtPO.Text = strPO
                        lbltxtPO.Text = strPO
                        txtPO_Single.Text = strPO
                        txtPOBU.Text = strBU
                        Session("PODetailViewBU") = strBU
                        Session("PODetailViewPO") = strPO
                        getShipToDetails(strPO, strVendorID)
                        getBuyerDetails(strPO, strBU)
                        getPOPriceLineInfo(strPO)
                        AllControlles("SHOW")
                        tbStripPODetails.Visible = True
                        getPODataSearch(strPO, strVendorID, strBU)
                        lblSinglePOInfo.Visible = False
                        lblSinglePOInfo.Text = ""
                        If strBU = "WAL00" Then
                            Dim workOrdExists As Boolean = getPOWorkOrdNum(strPO)
                        End If
                    End If
                Else
                    lblSinglePOInfo.Visible = True
                    lblSinglePOInfo.Text = "*Please enter a valid PO number"
                    tbStripPODetails.Visible = False
                    divPOinfo2.Visible = False
                    divPOinfo1.Visible = False
                    RadAjaxManager1.Visible = False
                    RadMultiPage1.Visible = False
                    lblCurrentTab.Visible = False
                    Exit Sub
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub PODetailsTabEnable(ByVal dtpo As DataTable)
        '' Dim dtpo As DataTable = Nothing
        Dim list1 = New List(Of KeyValuePair(Of String, Boolean))
        Dim strConfPO As String = "NO"
        Dim strASNPO As String = "NO"
        Dim strVoucherPO As String = "NO"
        Dim strDueDatePO As String = "NO"
        Dim isconfirmNeed As Boolean = False
        Dim isASNNeed As Boolean = False
        Dim isVoucherNeed As Boolean = False
        Dim isDueNeed As Boolean = False
        Dim isEnableNeed As Boolean = False
        Session("NeedDueDateUpdate") = Nothing
        Try
            list1 = getPOStatusForDetailsTab(dtpo)

            For Each kvp As KeyValuePair(Of String, Boolean) In list1
                If kvp.Key = "EnableTabs" And kvp.Value = True Then
                    isEnableNeed = True
                End If
                If kvp.Key = "isconfirmNeed" And kvp.Value = True Then
                    isconfirmNeed = True
                End If
                If kvp.Key = "isASNNeed" And kvp.Value = True Then
                    isASNNeed = True
                End If
                If kvp.Key = "isVoucherNeed" And kvp.Value = True Then
                    isVoucherNeed = True
                End If
                If kvp.Key = "isDueNeed" And kvp.Value = True Then
                    isDueNeed = True
                End If
            Next

            '' Diasabling the tabs that's POs already fully confirmed
            If isEnableNeed Then
                If Not isconfirmNeed Then
                    tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = False
                    '' Check if po confitmed and still due date need to update
                    If isDueNeed And isconfirmNeed Then
                        Session("NeedDueDateUpdate") = "YES"
                        tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Due Date Update"
                        tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = True
                        'lblCurrentTab.Text = "*PO Due Date Update*"
                    Else
                        tbStripPODetails.Tabs.FindTabByValue("POCON").Text = "PO Confirm"
                        tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = False
                    End If
                Else
                    tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled = True
                    'lblCurrentTab.Text = "*PO Confirm*"
                End If

                If Not isASNNeed Then
                    tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled = False
                Else
                    tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled = True
                    'lblCurrentTab.Text = "*ASN*"
                End If

                If Not isVoucherNeed Then
                    tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled = False
                Else
                    tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled = True
                    'lblCurrentTab.Text = "*Voucher Entry*"
                End If
                Session("POFullConfirm") = "NO"
            Else
                tbStripPODetails.Visible = False
                lblSinglePOInfo.Visible = True
                lblSinglePOInfo.Text = "*Entered PO is fully confirmed"
                Session("POFullConfirm") = "YES"
            End If

            ''Selecting the default tab that's already enabled
            If isEnableNeed Then
                If tbStripPODetails.Tabs.FindTabByValue("POCON").Enabled Then
                    tbStripPODetails.Tabs.FindTabByValue("POCON").Selected = True
                ElseIf tbStripPODetails.Tabs.FindTabByValue("POASN").Enabled Then
                    tbStripPODetails.Tabs.FindTabByValue("POASN").Selected = True
                    'lblCurrentTab.Text = "*ASN*"
                ElseIf tbStripPODetails.Tabs.FindTabByValue("POVENT").Enabled Then
                    tbStripPODetails.Tabs.FindTabByValue("POVENT").Selected = True
                    'lblCurrentTab.Text = "*Voucher Entry*"
                    'ElseIf tbStripPODetails.Tabs.FindTabByValue("PODUE").Enabled Then
                    '    tbStripPODetails.Tabs.FindTabByValue("PODUE").Selected = True
                End If
            End If

        Catch ex As Exception

        End Try

    End Sub

    '' PO confirm methods starts here
    Private Sub getPOConfirmDetails(strPO As String, strVendor As String, strPOUB As String)
        Dim dsPOdata As DataSet
        Dim strPoNum1 As String = ""
        Try

            '' getShipToDetails(Session("PONUM1"), Session("VENDOR_VP"))

            lblTrkError.Visible = False
            lblDueDtError.Visible = False
            lblDBError.Visible = False
            ltlAlert.Visible = False

            If Session("NeedDueDateUpdate") = "YES" Then
                dsPOdata = getpoinfoDueDate(strPO, strVendor, strPOUB)
            Else
                dsPOdata = getpoinfo(strPO, strVendor, strPOUB)
            End If

            Dim bIsDataPresent As Boolean = False
            Try
                If Not dsPOdata Is Nothing Then
                    If dsPOdata.Tables.Count > 0 Then
                        If dsPOdata.Tables(0).Rows.Count > 0 Then
                            bIsDataPresent = True
                        End If
                    End If
                End If

                If bIsDataPresent Then
                    btnSelectAll.Visible = True
                    dtgPO.Visible = True
                    btnSubmit.Visible = True
                    dtgPO.DataSource = dsPOdata
                    dtgPO.DataBind()
                    lblPODatetxt.Text = dsPOdata.Tables(0).Rows(0).Item("PODT")
                    txtPO_REF.Text = dsPOdata.Tables(0).Rows(0).Item("PO_REF")
                    txtPYMNT_TERMS_CD.Text = dsPOdata.Tables(0).Rows(0).Item("PYMNT_TERMS_CD")
                    txtCNTCT_SEQ_NUM.Text = dsPOdata.Tables(0).Rows(0).Item("CNTCT_SEQ_NUM")
                    txtBILL_LOCATION.Text = dsPOdata.Tables(0).Rows(0).Item("BILL_LOCATION")
                    txtTAX_EXEMPT.Text = dsPOdata.Tables(0).Rows(0).Item("TAX_EXEMPT")
                    txtTAX_EXEMPT_ID.Text = dsPOdata.Tables(0).Rows(0).Item("TAX_EXEMPT_ID")
                    txtCURRENCY_CD_HDR.Text = dsPOdata.Tables(0).Rows(0).Item("CURRENCY_CD_HDR")
                    txtRT_TYPE.Text = dsPOdata.Tables(0).Rows(0).Item("RT_TYPE")

                    If Not Page.IsPostBack Then
                        Insiteonline.WebPartnerFunctions.WebPSharedFunc.WebLogVendor()
                    End If

                    If checkConfirmed(strPO) = True Then
                        'btnAlreadyConfirm.Visible = True
                        'ltlAlert.Visible = True
                        'ltlAlert.Text = "View already confirmed lines for this PO"
                        btnSubmit.Visible = False
                    End If
                    'lblDBError.Text = "Screen Info.  PO #: " & Session("PONUM1") & " -- BU: " & Session("PoConSiteBu") & " -- Supplier ID: " & Session("VENDOR_VP") & vbCrLf
                    'lblDBError.Visible = True
                Else
                    'send and show error info
                    lblDBError.Text = "No info when loading screen. PO #: " & strPO & " -- BU: " & strPOUB & " -- Supplier ID: " & strVendor & vbCrLf
                    lblDBError.Visible = True
                    btnSubmit.Visible = False
                    btn90.Visible = False
                    btnPrintPO.Visible = False
                    btnSelectAll.Visible = False
                    dtgPO.Visible = False
                    sendErrorEmail("'POConfirm_VP' screen has no data.  " & "  Check Connection String for permission problems", "NO", Request.ServerVariables("URL"), "  " & vbCrLf & lblDBError.Text & " -- Session('GetPONUM1'): " & Session("GetPONUM1") & " -- Session('SDIEMP'): " & Session("SDIEMP") & " -- Session('USERID_VP'): " & Session("USERID_VP"))
                End If
            Catch ex As Exception
                'send and show error info
                lblDBError.Text = "Error Loading screen. PO #: " & strPO & " -- BU: " & strPOUB & " -- Supplier ID: " & strVendor & vbCrLf
                lblDBError.Visible = True
                btnSubmit.Visible = False
                btn90.Visible = False
                btnPrintPO.Visible = False
                btnSelectAll.Visible = False
                dtgPO.Visible = False
                sendErrorEmail("'POConfirm_VP' screen error: " & ex.Message & "  Check Connection String for permission problems", "NO", Request.ServerVariables("URL"), "  " & vbCrLf & lblDBError.Text)
            End Try
        Catch ex As Exception
            Dim sMyErrorString As String = ""
            If Not ex Is Nothing Then
                sMyErrorString = ex.ToString
            End If
            sMyErrorString += sMyErrorString &
                       "Error in  -> getPOConfirmDetails" & vbCrLf &
                       "PO = " & strPO & ", Business Unit = " & strPOUB & " ; Server: " & HttpContext.Current.Session("WEBSITEID")
            SendSDiExchErrorMail(sMyErrorString, "Error in PODetails -> getPOConfirmDetails")
        End Try
    End Sub

    Protected Sub dtgPO_ItemDataBound(sender As Object, e As GridItemEventArgs)
        Dim strPODueDateUpdate As String = ""
        Try
            strPODueDateUpdate = Session("NeedDueDateUpdate")

            ''Disabling the text filed in POconfirmation grid for due date update
            If strPODueDateUpdate = "YES" Then
                If TypeOf e.Item Is GridDataItem Then
                    Dim item As GridDataItem = DirectCast(e.Item, GridDataItem)


                    Dim txtDescr254Mixed As TextBox = DirectCast(item.FindControl("txtDescr254Mixed"), TextBox)
                    Dim txtMfgItemID As TextBox = DirectCast(item.FindControl("txtMfgItemID"), TextBox)
                    Dim txtUPCID As TextBox = DirectCast(item.FindControl("txtUPCID"), TextBox)
                    Dim txtMFGID As TextBox = DirectCast(item.FindControl("txtMFGID"), TextBox)
                    Dim txtVndItemID As TextBox = DirectCast(item.FindControl("txtVndItemID"), TextBox)
                    Dim txtQTY As TextBox = DirectCast(item.FindControl("txtQTY"), TextBox)
                    Dim decPrice As TextBox = DirectCast(item.FindControl("decPrice"), TextBox)
                    Dim dpDeliveredDate As RadDatePicker = DirectCast(item.FindControl("dpDeliveredDate"), RadDatePicker)


                    Dim lbltxtDescr As Label = DirectCast(item.FindControl("lbltxtDescr"), Label)
                    Dim lbltxtMfgItemID As Label = DirectCast(item.FindControl("lbltxtMfgItemID"), Label)
                    Dim lbltxtUPCID As Label = DirectCast(item.FindControl("lbltxtUPCID"), Label)
                    Dim lbltxtMFGID As Label = DirectCast(item.FindControl("lbltxtMFGID"), Label)
                    Dim lbltxtVndItemID As Label = DirectCast(item.FindControl("lbltxtVndItemID"), Label)
                    Dim lbltxtQTY As Label = DirectCast(item.FindControl("lbltxtQTY"), Label)
                    Dim lbldecPrice As Label = DirectCast(item.FindControl("lbldecPrice"), Label)


                    txtDescr254Mixed.Visible = False
                    txtMfgItemID.Visible = False
                    txtUPCID.Visible = False
                    txtMFGID.Visible = False
                    txtVndItemID.Visible = False
                    txtQTY.Visible = False
                    decPrice.Visible = False


                    lbltxtDescr.Visible = True
                    lbltxtMfgItemID.Visible = True
                    lbltxtUPCID.Visible = True
                    lbltxtMFGID.Visible = True
                    lbltxtVndItemID.Visible = True
                    lbltxtQTY.Visible = True
                    lbldecPrice.Visible = True


                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSubmit.Click
        Try
            Me.btn90.Visible = False
            lblDueDtError.Visible = False
            lblDBError.Visible = False
            Dim I As Integer
            Dim bolError As Boolean
            Dim bolChecked As Boolean
            Dim bolAllChecked As Boolean
            Dim format As New System.Globalization.CultureInfo("en-US", True)
            Dim myDateTimeUS As System.DateTime
            Dim date2 As Date
            Dim date1 As Date
            Dim ted13 As Date
            Dim ted90 As Date
            bolChecked = False
            bolAllChecked = True

            ltlAlert.Visible = False
            lblDueDtError.Visible = False
            Dim sessdue As String = Session("NeedDueDateUpdate")
            If Not Session("NeedDueDateUpdate") = "YES" Then
                bolError = False
                For I = 0 To dtgPO.Items.Count - 1
                    Dim item As GridDataItem = dtgPO.Items(I)
                    Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)

                    dpDeliveredDate.BackColor = White


                    If CType(item.FindControl("chkConfirm"), CheckBox).Checked = True Then
                        bolChecked = True
                        Try
                            myDateTimeUS = System.DateTime.Parse(Trim(CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate), format)
                        Catch ex As Exception
                            lblDueDtError.Text = "Invalid Due Date."
                            lblDueDtError.Visible = True

                            dpDeliveredDate.BackColor = Red
                            bolError = True
                        End Try

                        If bolError = False Then
                            date1 = Now().ToString("d")
                            date2 = System.DateTime.Parse(Trim(CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate), format)
                            ted90 = date1.AddDays(90)
                            ted13 = date1.AddMonths(13)
                            If Date.Compare(date1, date2) > 0 Then
                                lblDueDtError.Text = "Invalid Due Date."
                                lblDueDtError.Visible = True
                                dpDeliveredDate.BackColor = Red
                                bolError = True
                            ElseIf date2 > ted13 Then
                                lblDueDtError.Text = "Date greater than 13 months."
                                lblDueDtError.Visible = True
                                dpDeliveredDate.BackColor = Red
                                bolError = True
                                Me.btn90.Visible = False

                            ElseIf date2 < ted13 And date2 > ted90 Then
                                'greater_than_ninety_days()
                                Me.btn90.Visible = True
                            End If
                        End If
                    Else
                        bolAllChecked = False
                    End If
                Next
                If bolChecked = False Then
                    bolError = True
                    Dim strMessage As New Alert
                    ltlAlert.Text = "No lines have been selected for confirmation."
                    ltlAlert.Visible = True
                End If
                If bolError = True Then
                    Exit Sub
                End If
                If txtHidepo.Text = txtPO.Text Then
                    Exit Sub
                End If
                If Me.btn90.Visible = True Then
                    Dim strMessage As New Alert
                    Dim bIsValidWO As Boolean
                    ltlAlert.Visible = True
                    ltlAlert.Text = "Delivery date greater than 90 Days. Press the blue....'CONTINUE'.... button (below) to proceed or change the Delivery date."
                    ' ElseIf date2 < ted13 And date2 > ted90 Then           '
                    Session("date2") = date2
                    Session("ted13") = ted13
                    Session("ted90") = ted90
                    Session("bolchk") = bolChecked
                    Session("bolerror") = bolError
                    Session("sender") = sender
                    Session("e") = e
                    Exit Sub
                End If

                txtHidepo.Text = txtPO.Text
                Dim intQueueInst As Integer
                intQueueInst = getNextQueueInst()
                If intQueueInst = 0 Then
                    lblDBError.Text = "Error getting queue number"
                    lblDBError.Visible = True
                    txtHidepo.Text = ""
                    Exit Sub
                End If
                bolError = createECConfirmation(intQueueInst)
                If bolError = True Then
                    lblDBError.Text = "Error updating EC tables"
                    lblDBError.Visible = True
                    txtHidepo.Text = ""
                    Exit Sub
                Else
                    ServiceChannelUpdate()
                    ltlAlert.Text = "Selected PO successfully confirmed."
                    ltlAlert.Visible = True
                    btnSubmit.Visible = False
                End If

            Else
                bolError = False
                For I = 0 To dtgPO.Items.Count - 1
                    Dim item As GridDataItem = dtgPO.Items(I)
                    Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)

                    dpDeliveredDate.BackColor = White

                    If CType(item.FindControl("chkConfirm"), CheckBox).Checked = True Then
                        bolChecked = True
                        Try
                            myDateTimeUS = System.DateTime.Parse(Trim(CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate), format)
                        Catch ex As Exception
                            lblDueDtError.Text = "Invalid Due Date."
                            lblDueDtError.Visible = True

                            dpDeliveredDate.BackColor = Red
                            bolError = True
                        End Try

                        If bolError = False Then
                            date1 = Now().ToString("d")
                            date2 = System.DateTime.Parse(Trim(CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate), format)
                            ted90 = date1.AddDays(90)
                            ted13 = date1.AddMonths(13)
                            If Date.Compare(date1, date2) > 0 Then
                                lblDueDtError.Text = "Invalid Due Date."
                                lblDueDtError.Visible = True
                                dpDeliveredDate.BackColor = Red
                                bolError = True
                            End If
                        End If
                    Else
                        bolAllChecked = False
                    End If
                Next

                If bolChecked = False Then
                    bolError = True
                    Dim strMessage As New Alert
                    ltlAlert.Text = "No lines have been selected for due date update."
                    ltlAlert.Visible = True
                End If
                If bolError = True Then
                    Exit Sub
                End If
                If txtHidepo.Text = txtPO.Text Then
                    Exit Sub
                End If


                txtHidepo.Text = txtPO.Text
                Dim intQueueInst As Integer
                intQueueInst = getNextQueueInst()
                If intQueueInst = 0 Then
                    lblDBError.Text = "Error getting queue number"
                    lblDBError.Visible = True
                    txtHidepo.Text = ""
                    Exit Sub
                End If
                bolError = createECConfirmationDueUpdate(intQueueInst)
                If bolError = True Then
                    lblDBError.Text = "Error updating EC tables"
                    lblDBError.Visible = True
                    txtHidepo.Text = ""
                    Exit Sub
                Else
                    'Added the successful message after updating the PO due date
                    ltlAlert.Text = "Selected PO Due Date successfully updated.Waiting for confirmation"
                    Session("PODueDate") = UpdatePODueDate()
                    ltlAlert.Visible = True
                    btnSubmit.Visible = False
                    ServiceChannelUpdate()
                End If
            End If
        Catch ex As Exception
            Dim sMyErrorString As String = ""
            If Not ex Is Nothing Then
                sMyErrorString = ex.ToString
            End If
            sMyErrorString += sMyErrorString &
                       "Error in  -> btnSubmit_Click" & vbCrLf &
                       "PO = " & txtPO.Text & ", Business Unit = " & txtPOBU.Text & " ; Server: " & HttpContext.Current.Session("WEBSITEID")
            SendSDiExchErrorMail(sMyErrorString, "Error in PODetails -> btnSubmit_Click")
        End Try
    End Sub

    Private Function createECConfirmation(ByVal intQueueInst) As Boolean
        Try
            Dim I As Integer
            Dim strSQLstring As String
            Dim strStatus As String
            Dim bolChanged As Boolean = False
            Dim bolUpdateFailed As Boolean = False
            Dim strDescr As String
            Dim strMfgID As String
            Dim strMfgItemID As String
            Dim strVndItemID As String
            Dim strUPCID As String
            Dim strOrderNo As String = ""
            Dim decQTY As Decimal
            Dim decPrice As Decimal
            Dim strDueDt As Date
            Dim strDueDtOld As Date
            Dim dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
            Dim dteNowd As Date = Now().ToString("d")
            Dim rowsaffected As Integer


            strStatus = "AD"

            strSQLstring = "INSERT INTO PS_ECQUEUE" & vbCrLf &
                        " (ECTRANSID, ECQUEUEINSTANCE, ECTRANSINOUTSW," & vbCrLf &
                        " ECBUSDOCID, ECQUEUESTATUS, ECACTIONCD," & vbCrLf &
                        " ECDRIVERDTTM, ECAPPDTTM, BUSINESS_UNIT," & vbCrLf &
                        " ECENTITYCD_BU, ECCUSTVNDRVAL, ECENTITYCD_EXT," & vbCrLf &
                        " EC_QUEUE_CNTL_NBR)" & vbCrLf &
                        " VALUES('PO ACK', " & intQueueInst & ", 'I'," & vbCrLf &
                        " " & intQueueInst & ", 'L', ' '," & vbCrLf &
                        " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'), '','" & Session("PODetailViewBU") & "'," & vbCrLf &
                        " 'POBU', '" & txtVendor.Text & "', 'VNDR'," & vbCrLf &
                        " ' ')"
            rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            If rowsaffected = 0 Then
                bolUpdateFailed = True
            End If

            For I = 0 To dtgPO.Items.Count - 1
                Dim item As GridDataItem = dtgPO.Items.Item(I)
                If CType(item.FindControl("chkConfirm"), CheckBox).Checked = True Then

                    strOrderNo = txtOrder_No.Text.Trim
                    strMfgItemID = CType(item.FindControl("txtMfgItemID"), TextBox).Text
                    strVndItemID = CType(item.FindControl("txtVndItemID"), TextBox).Text
                    strUPCID = CType(item.FindControl("txtUPCID"), TextBox).Text
                    strMfgID = CType(item.FindControl("txtMfgID"), TextBox).Text
                    strDescr = CType(item.FindControl("txtDescr254Mixed"), TextBox).Text


                    If CType(item.FindControl("txtqty"), TextBox).Text = "" Then
                        decQTY = 0
                    Else
                        decQTY = Convert.ToDecimal(CType(item.FindControl("txtqty"), TextBox).Text)
                    End If
                    If CType(item.FindControl("decPrice"), TextBox).Text = "" Then
                        decPrice = 0
                    Else
                        decPrice = Convert.ToDecimal(CType(item.FindControl("decPrice"), TextBox).Text)
                    End If

                    strDueDt = CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate

                    strDueDtOld = item("DUE_DT").Text

                    If strMfgID = "" Then
                        strMfgID = " "
                    End If
                    If strMfgItemID = "" Then
                        strMfgItemID = " "
                    End If
                    If strVndItemID = "" Then
                        strVndItemID = " "
                    End If
                    If strUPCID = "" Then
                        strUPCID = " "
                    End If

                    'strDescr = Replace(item("DESCR254_MIXED").Text, "'", "")
                    strDescr = Replace(strDescr, "'", "")
                    strVndItemID = Replace(strVndItemID, "'", "''")
                    strMfgID = Replace(strMfgID, "'", "''") 'Replace(item("MFG_ID").Text, "'", "")
                    strMfgItemID = Replace(strMfgItemID, "'", "")
                    strUPCID = Replace(item("UPC_ID").Text, "'", "")

                    Dim strProblemCode As String = "VN"
                    'Dim strDESCR60 As String = "Expeditor - Shipped"
                    Dim stroperid As String = Session("OPERID")

                    strSQLstring = "INSERT INTO PS_ISA_XPD_COMMENT" & vbCrLf &
                " (BUSINESS_UNIT, PO_ID," & vbCrLf &
                " LINE_NBR, SCHED_NBR, " & vbCrLf &
                " ISA_PROBLEM_CODE, NOTES_1000," & vbCrLf &
                " OPRID," & vbCrLf &
                " DTTM_STAMP ) " & vbCrLf &
                " VALUES ('" & Session("PODetailViewBU") & "', '" & txtPO.Text & "'," & vbCrLf &
                " '" & item("LINE_NBR").Text & "', " & item("SCHED_NBR").Text & "," & vbCrLf &
                " '" & strProblemCode & "'," & vbCrLf &
                " '" & strOrderNo & "'," & vbCrLf &
                " '" & stroperid.ToUpper & "'" & vbCrLf &
                ",SYSDATE )" & vbCrLf

                    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                    If rowsaffected = 0 Then
                        bolUpdateFailed = True
                    End If

                    strSQLstring = "INSERT INTO PS_PO_LINE_EC" & vbCrLf &
                            " (EIP_CTL_ID," & vbCrLf &
                            " BUSINESS_UNIT, PO_ID, LINE_NBR," & vbCrLf &
                            " INV_ITEM_ID, DESCR254_MIXED, ITM_ID_VNDR," & vbCrLf &
                            " VNDR_CATALOG_ID, UNIT_OF_MEASURE, MFG_ID," & vbCrLf &
                            " CNTRCT_ID, CNTRCT_LINE_NBR, RFQ_ID," & vbCrLf &
                            " RFQ_LINE_NBR, ACK_STATUS, MFG_ITM_ID," & vbCrLf &
                            " UPN_ID,PROCESS_FLG,PO_LINE_CHNG_COUNT,PO_LINE_BO_COUNT,CATEGORY_ID,VERSION_NBR,CAT_LINE_NBR,RELEASE_NBR)" & vbCrLf &
                            " VALUES('PO ACK" & intQueueInst & "I'," & vbCrLf &
                            " '" & Session("PODetailViewBU") & "', '" & txtPO.Text & "', '" & item("LINE_NBR").Text & "'," & vbCrLf &
                            " '" & item("INV_ITEM_ID").Text & "'," & vbCrLf &
                            " '" & strDescr.ToUpper & "'," & vbCrLf &
                            " '" & strVndItemID & "'," & vbCrLf &
                            " ' '," & vbCrLf &
                            " '" & item("UNIT_OF_MEASURE").Text & "'," & vbCrLf &
                            " '" & strMfgID & "'," & vbCrLf &
                            " ' ',0,' '," & vbCrLf &
                            " 0,'" & strStatus & "'," & vbCrLf &
                            " '" & strMfgItemID & "'," & vbCrLf &
                            " '" & strUPCID & " ',' ',0,0,' ',0,0,0)" & vbCrLf

                    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                    If rowsaffected = 0 Then
                        bolUpdateFailed = True
                    End If

                    strSQLstring = "INSERT INTO PS_PO_LINE_SHIP_EC" & vbCrLf &
                            " (EIP_CTL_ID," & vbCrLf &
                            " BUSINESS_UNIT, PO_ID, LINE_NBR," & vbCrLf &
                            " SCHED_NBR, PRICE_PO, DUE_DT," & vbCrLf &
                            " DUE_TIME, SHIPTO_ID, QTY_PO," & vbCrLf &
                            " MERCHANDISE_AMT, CURRENCY_CD, FREIGHT_TERMS," & vbCrLf &
                            " SHIP_TYPE_ID,REVISION,CUSTOM_PRICE,ZERO_PRICE_IND,PROCESS_FLG,BCKORD_ORG_SCHED,ATTN_TO,STD_ID_NUM_SHIPTO)" & vbCrLf &
                            " VALUES('PO ACK" & intQueueInst & "I'," & vbCrLf &
                            " '" & Session("PODetailViewBU") & "', '" & txtPO.Text & "', '" & item("LINE_NBR").Text & "'," & vbCrLf &
                            " '" & item("SCHED_NBR").Text & "'," & vbCrLf &
                            " " & decPrice & "," & vbCrLf &
                            " TO_DATE('" & strDueDt & "','MM/DD/YYYY')," & vbCrLf &
                            " '', '" & txtShiptoID.Text & "'," & vbCrLf &
                            " " & decQTY & "," & vbCrLf &
                            " " & (decQTY * decPrice) & "," & vbCrLf &
                            " '" & item("CURRENCY_CD").Text & "'," & vbCrLf &
                            " '" & item("FREIGHT_TERMS").Text & "'," & vbCrLf &
                            " '" & item("SHIP_TYPE_ID").Text & "',' ',' ',' ',' ',0,' ',' ')"

                    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                    If rowsaffected = 0 Then
                        bolUpdateFailed = True
                    End If

                End If
            Next

            strSQLstring = "INSERT INTO PS_PO_HDR_EC" & vbCrLf &
                        " (EIP_CTL_ID," & vbCrLf &
                        " BUSINESS_UNIT, PO_ID, CHNG_ORD_BATCH," & vbCrLf &
                        " PO_REF, PYMNT_TERMS_CD, CNTCT_SEQ_NUM," & vbCrLf &
                        " BILL_LOCATION, TAX_EXEMPT, TAX_EXEMPT_ID," & vbCrLf &
                        " CURRENCY_CD, RT_TYPE, ACKNOWLEGE_DT," & vbCrLf &
                        " ACK_STATUS, ACK_RECEIVED_DT, REVIEWED," & vbCrLf &
                        " REVIEWED_DT, OPRID,PAY_TRM_BSE_DT_OPT,DISP_METHOD,PROCESS_FLG,PO_POA_STATUS,PO_ACK_SOURCE,EDX_COMPARE_STATUS,VNDR_UPN_FLG,STD_ID_NUM_VNDRGLN,STD_ID_NUM_BILLTO,COMMENTS)" & vbCrLf &
                        " VALUES('PO ACK" & intQueueInst & "I'," & vbCrLf &
                        " '" & Session("PODetailViewBU") & "', '" & txtPO.Text & "',0," & vbCrLf &
                        " '" & txtPO_REF.Text & "'," & vbCrLf &
                        " '" & txtPYMNT_TERMS_CD.Text & "'," & vbCrLf &
                        " '" & txtCNTCT_SEQ_NUM.Text & "'," & vbCrLf &
                        " '" & txtBILL_LOCATION.Text & "'," & vbCrLf &
                        " '" & txtTAX_EXEMPT.Text & "'," & vbCrLf &
                        " '" & txtTAX_EXEMPT_ID.Text & "'," & vbCrLf &
                        " '" & txtCURRENCY_CD_HDR.Text & "'," & vbCrLf &
                        " '" & txtRT_TYPE.Text & "'," & vbCrLf &
                        " TO_DATE('" & dteNowd & "','MM-DD-YYYY')," & vbCrLf &
                        " '" & strStatus & "'," & vbCrLf &
                        " TO_DATE('" & dteNowd & "','MM-DD-YYYY')," & vbCrLf &
                        " 'N','','" & Session("OPERID") & "',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ')"

            rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            If rowsaffected = 0 Then
                bolUpdateFailed = True
            End If

            If bolUpdateFailed = True Then
                strSQLstring = "DELETE FROM PS_ECQUEUE" & vbCrLf &
                        " WHERE ECTRANSID = 'PO ACK'" & vbCrLf &
                        " AND ECQUEUEINSTANCE = " & intQueueInst & vbCrLf &
                        " AND ECTRANSINOUTSW = 'I'" & vbCrLf

                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

                strSQLstring = "DELETE FROM PS_PO_LINE_EC" & vbCrLf &
                          " WHERE EIP_CTL_ID = 'PO ACK" & intQueueInst & "I'"


                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

                strSQLstring = "DELETE FROM PS_PO_LINE_SHIP_EC" & vbCrLf &
                        " WHERE EIP_CTL_ID = 'PO ACK" & intQueueInst & "I'"


                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

                strSQLstring = "DELETE FROM PS_PO_HDR_EC" & vbCrLf &
                          " WHERE EIP_CTL_ID = 'PO ACK" & intQueueInst & "I'"

                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

            End If

            Return bolUpdateFailed
        Catch ex As Exception
            Dim sMyErrorString As String = ""
            If Not ex Is Nothing Then
                sMyErrorString = ex.ToString
            End If
            sMyErrorString += sMyErrorString &
                       "Error in  -> createECConfirmation" & vbCrLf &
                       "PO = " & txtPO.Text & ", Business Unit = " & txtPOBU.Text & " ; Server: " & HttpContext.Current.Session("WEBSITEID")
            SendSDiExchErrorMail(sMyErrorString, "Error in PODetails -> createECConfirmation")
        End Try

    End Function

    Private Sub btnSelectAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSelectAll.Click
        Try
            Dim I As Integer
            For I = 0 To dtgPO.Items.Count - 1
                Dim item As GridDataItem = dtgPO.Items.Item(I)
                'CType(dtgPO.Items.Item(I).Cells(23).FindControl("chkConfirm"), CheckBox).Checked = True
                CType(item.FindControl("chkConfirm"), CheckBox).Checked = True
            Next
        Catch ex As Exception
        End Try

    End Sub

    Private Sub btnPrintPO_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrintPO.Click

        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim CartConfirmRpt As CrystalDecisions.CrystalReports.Engine.ReportDocument = Nothing
        Dim tmp As String = ""
        Dim script As String = ""
        Dim strPONum As String = ""
        Dim strBU As String = ""
        Dim url As String = ""

        Try
            OpenPurchaseOrderRptBinary(Session("PODetailViewPO"), CartConfirmRpt, url, Session("PODetailViewBU"))

            RadWindowShowPDF.NavigateUrl = "//" & currentApp.Request.ServerVariables("HTTP_HOST") & GetWebAppName1() & "/" & "SaveReport.aspx"
            RadWindowShowPDF.VisibleStatusbar = False
            RadWindowShowPDF.Height = 800
            RadWindowShowPDF.Width = 1000
            Dim script1 As String = "function f(){$find(""" + RadWindowShowPDF.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script1, True)

        Catch ex As Exception

        End Try

    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Response.Redirect("SDIVendor.aspx")
    End Sub

    Private Sub btnASNCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnASNCancel.Click
        Response.Redirect("SDIVendor.aspx")
    End Sub

    Private Sub btn90_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn90.Click

        Dim bError As Boolean = False
        Dim bCheck As Boolean = False
        Try
            If Not Session("bolerror") Is Nothing Then
                bError = CType(Session("bolerror"), Boolean)
            Else
                bError = False
            End If
        Catch ex As Exception
            bError = False
        End Try
        Try
            If Not Session("bolchk") Is Nothing Then
                bCheck = CType(Session("bolchk"), Boolean)
            Else
                bCheck = False
            End If
        Catch ex As Exception
            bCheck = False
        End Try
        'proceed(Session("bolerror"), Session("bolchk"), Session("sender"), Session("e"))
        proceed(bError, bCheck)  '  , Session("sender"), Session("e"))

    End Sub

    Private Function proceed(ByVal bolerror As Boolean, ByVal bolallchecked As Boolean)

        Try
            Dim I As Integer
            'Dim bolError As Boolean
            Dim bolChecked As Boolean
            ' Dim bolAllChecked As Boolean
            Dim format As New System.Globalization.CultureInfo("en-US", True)
            Dim myDateTimeUS As System.DateTime
            Dim date2 As Date
            Dim date1 As Date
            Dim ted13 As Date
            Dim ted90 As Date
            Dim item As GridDataItem = dtgPO.Items(I)
            'Dim txtDueDate As TextBox = CType(item.FindControl("txtDueDate"), TextBox)
            Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)

            If bolerror = False Then
                date1 = Now().ToString("d")
                'date2 = System.DateTime.Parse(Trim(CType(dtgPO.Items(I).Cells(10).FindControl("txtDueDate"), TextBox).Text), format)
                date2 = System.DateTime.Parse(Trim(CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate), format)
                ted90 = date1.AddDays(90)
                ted13 = date1.AddMonths(13)
                If Date.Compare(date1, date2) > 0 Then
                    lblDueDtError.Text = "Invalid Due Date."
                    lblDueDtError.Visible = True
                    'dtgPO.Items(I).Cells(10).BackColor = Red
                    'txtDueDate.BackColor = Red
                    dpDeliveredDate.BackColor = Red
                    bolerror = True
                    Exit Function
                ElseIf date2 > ted13 Then
                    lblDueDtError.Text = "Date greater than 13 months."
                    lblDueDtError.Visible = True
                    'dtgPO.Items(I).Cells(10).BackColor = Red
                    'txtDueDate.BackColor = Red
                    dpDeliveredDate.BackColor = Red
                    bolerror = True
                    Me.btn90.Visible = False
                    Exit Function
                End If
            End If

            'greater_than_ninety_days()


            Me.btn90.Visible = True
            txtHidepo.Text = txtPO.Text
            Dim intQueueInst As Integer
            intQueueInst = getNextQueueInst()
            If intQueueInst = 0 Then
                lblDBError.Text = "Error getting queue number"
                lblDBError.Visible = True
                txtHidepo.Text = ""
                Exit Function
            End If
            bolerror = createECConfirmation(intQueueInst)
            If bolerror = True Then
                lblDBError.Text = "Error updating EC tables"
                lblDBError.Visible = True
                txtHidepo.Text = ""
                Exit Function
            Else
                ltlAlert.Text = "Selected PO sucessfully confirmed."
                ltlAlert.Visible = True
                btn90.Visible = False
                btnSubmit.Visible = False
            End If

            Session("bolchk") = " "
            Session("bolerror") = " "
            Session("sender") = " "
            Session("e") = " "
            Session("date2") = " "
            Session("ted13") = " "
            Session("ted90") = " "
            Me.btn90.Visible = False

        Catch ex As Exception
        End Try
    End Function

    Private Function createECConfirmationDueUpdate(ByVal intQueueInst) As Boolean
        Try
            Dim I As Integer
            Dim strSQLstring As String
            Dim strStatus As String
            Dim bolChanged As Boolean = False
            Dim bolUpdateFailed As Boolean = False
            Dim strDescr As String
            Dim strMfgID As String
            Dim strMfgItemID As String
            Dim strVndItemID As String
            Dim strUPCID As String
            Dim decQTY As Decimal
            Dim decPrice As Decimal
            Dim strDueDt As Date
            Dim strDueDtOld As Date
            Dim dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
            Dim dteNowd As Date = Now().ToString("d")
            Dim rowsaffected As Integer


            strStatus = "AD"

            strSQLstring = "INSERT INTO PS_ECQUEUE" & vbCrLf &
                        " (ECTRANSID, ECQUEUEINSTANCE, ECTRANSINOUTSW," & vbCrLf &
                        " ECBUSDOCID, ECQUEUESTATUS, ECACTIONCD," & vbCrLf &
                        " ECDRIVERDTTM, ECAPPDTTM, BUSINESS_UNIT," & vbCrLf &
                        " ECENTITYCD_BU, ECCUSTVNDRVAL, ECENTITYCD_EXT," & vbCrLf &
                        " EC_QUEUE_CNTL_NBR)" & vbCrLf &
                        " VALUES('PO ACK', " & intQueueInst & ", 'I'," & vbCrLf &
                        " " & intQueueInst & ", 'L', ' '," & vbCrLf &
                        " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'), '','" & Session("PODetailViewBU") & "'," & vbCrLf &
                        " 'POBU', '" & txtVendor.Text & "', 'VNDR'," & vbCrLf &
                        " ' ')"
            rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            If rowsaffected = 0 Then
                bolUpdateFailed = True
            End If

            For I = 0 To dtgPO.Items.Count - 1
                Dim item As GridDataItem = dtgPO.Items.Item(I)
                If CType(item.FindControl("chkConfirm"), CheckBox).Checked = True Then

                    'strMfgItemID = CType(item.FindControl("txtMfgItemID"), TextBox).Text
                    'strVndItemID = CType(item.FindControl("txtVndItemID"), TextBox).Text
                    'strUPCID = CType(item.FindControl("txtUPCID"), TextBox).Text
                    'strMfgID = CType(item.FindControl("txtMfgID"), TextBox).Text
                    'strDescr = CType(item.FindControl("txtDescr254Mixed"), TextBox).Text

                    strMfgItemID = item("MFG_ITM_ID").Text
                    strVndItemID = item("ITM_ID_VNDR").Text
                    strUPCID = item("UPC_ID").Text
                    strMfgID = item("MFG_ID").Text
                    strDescr = item("DESCR254_MIXED").Text

                    decQTY = item("QTY_PO").Text

                    decPrice = item("PRICE_PO").Text

                    'If CType(item.FindControl("txtqty"), TextBox).Text = "" Then
                    '    decQTY = 0
                    'Else
                    '    decQTY = Convert.ToDecimal(CType(item.FindControl("txtqty"), TextBox).Text)
                    'End If
                    'If CType(item.FindControl("decPrice"), TextBox).Text = "" Then
                    '    decPrice = 0
                    'Else
                    '    decPrice = Convert.ToDecimal(CType(item.FindControl("decPrice"), TextBox).Text)
                    'End If

                    strDueDt = CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate

                    strDueDtOld = item("DUE_DT").Text

                    If strMfgID = "" Then
                        strMfgID = " "
                    End If
                    If strMfgItemID = "" Then
                        strMfgItemID = " "
                    End If
                    If strVndItemID = "" Then
                        strVndItemID = " "
                    End If
                    If strUPCID.Trim = "" Then
                        strUPCID = " "
                    End If

                    'strDescr = Replace(item("DESCR254_MIXED").Text, "'", "")
                    strDescr = Replace(strDescr, "'", "")
                    strVndItemID = Replace(strVndItemID, "'", "''")
                    strMfgID = Replace(strMfgID, "'", "''") 'Replace(item("MFG_ID").Text, "'", "")
                    strMfgItemID = Replace(strMfgItemID, "'", "")
                    strUPCID = Replace(strUPCID, "'", "")
                    strSQLstring = "INSERT INTO PS_PO_LINE_EC" & vbCrLf &
                            " (EIP_CTL_ID," & vbCrLf &
                            " BUSINESS_UNIT, PO_ID, LINE_NBR," & vbCrLf &
                            " INV_ITEM_ID, DESCR254_MIXED, ITM_ID_VNDR," & vbCrLf &
                            " VNDR_CATALOG_ID, UNIT_OF_MEASURE, MFG_ID," & vbCrLf &
                            " CNTRCT_ID, CNTRCT_LINE_NBR, RFQ_ID," & vbCrLf &
                            " RFQ_LINE_NBR, ACK_STATUS, MFG_ITM_ID," & vbCrLf &
                            " UPN_ID,PROCESS_FLG,PO_LINE_CHNG_COUNT,PO_LINE_BO_COUNT,CATEGORY_ID,VERSION_NBR,CAT_LINE_NBR,RELEASE_NBR)" & vbCrLf &
                            " VALUES('PO ACK" & intQueueInst & "I'," & vbCrLf &
                            " '" & Session("PODetailViewBU") & "', '" & txtPO.Text & "', '" & item("LINE_NBR").Text & "'," & vbCrLf &
                            " '" & item("INV_ITEM_ID").Text & "'," & vbCrLf &
                            " '" & strDescr.ToUpper & "'," & vbCrLf &
                            " '" & strVndItemID & "'," & vbCrLf &
                            " ' '," & vbCrLf &
                            " '" & item("UNIT_OF_MEASURE").Text & "'," & vbCrLf &
                            " '" & strMfgID & "'," & vbCrLf &
                            " ' ',0,' '," & vbCrLf &
                            " 0,'" & strStatus & "'," & vbCrLf &
                            " '" & strMfgItemID & "'," & vbCrLf &
                            " '" & strUPCID & " ',' ',0,0,' ',0,0,0)" & vbCrLf

                    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                    If rowsaffected = 0 Then
                        bolUpdateFailed = True
                    End If

                    strSQLstring = "INSERT INTO PS_PO_LINE_SHIP_EC" & vbCrLf &
                            " (EIP_CTL_ID," & vbCrLf &
                            " BUSINESS_UNIT, PO_ID, LINE_NBR," & vbCrLf &
                            " SCHED_NBR, PRICE_PO, DUE_DT," & vbCrLf &
                            " DUE_TIME, SHIPTO_ID, QTY_PO," & vbCrLf &
                            " MERCHANDISE_AMT, CURRENCY_CD, FREIGHT_TERMS," & vbCrLf &
                            " SHIP_TYPE_ID,REVISION,CUSTOM_PRICE,ZERO_PRICE_IND,PROCESS_FLG,BCKORD_ORG_SCHED,ATTN_TO,STD_ID_NUM_SHIPTO)" & vbCrLf &
                            " VALUES('PO ACK" & intQueueInst & "I'," & vbCrLf &
                            " '" & Session("PODetailViewBU") & "', '" & txtPO.Text & "', '" & item("LINE_NBR").Text & "'," & vbCrLf &
                            " '" & item("SCHED_NBR").Text & "'," & vbCrLf &
                            " " & decPrice & "," & vbCrLf &
                            " TO_DATE('" & strDueDt & "','MM/DD/YYYY')," & vbCrLf &
                            " '', '" & txtShiptoID.Text & "'," & vbCrLf &
                            " " & decQTY & "," & vbCrLf &
                            " " & (decQTY * decPrice) & "," & vbCrLf &
                            " '" & item("CURRENCY_CD").Text & "'," & vbCrLf &
                            " '" & item("FREIGHT_TERMS").Text & "'," & vbCrLf &
                            " '" & item("SHIP_TYPE_ID").Text & "',' ',' ',' ',' ',0,' ',' ')"

                    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                    If rowsaffected = 0 Then
                        bolUpdateFailed = True
                    End If

                End If
            Next

            strSQLstring = "INSERT INTO PS_PO_HDR_EC" & vbCrLf &
                        " (EIP_CTL_ID," & vbCrLf &
                        " BUSINESS_UNIT, PO_ID, CHNG_ORD_BATCH," & vbCrLf &
                        " PO_REF, PYMNT_TERMS_CD, CNTCT_SEQ_NUM," & vbCrLf &
                        " BILL_LOCATION, TAX_EXEMPT, TAX_EXEMPT_ID," & vbCrLf &
                        " CURRENCY_CD, RT_TYPE, ACKNOWLEGE_DT," & vbCrLf &
                        " ACK_STATUS, ACK_RECEIVED_DT, REVIEWED," & vbCrLf &
                        " REVIEWED_DT, OPRID,PAY_TRM_BSE_DT_OPT,DISP_METHOD,PROCESS_FLG,PO_POA_STATUS,PO_ACK_SOURCE,EDX_COMPARE_STATUS,VNDR_UPN_FLG,STD_ID_NUM_VNDRGLN,STD_ID_NUM_BILLTO,COMMENTS)" & vbCrLf &
                        " VALUES('PO ACK" & intQueueInst & "I'," & vbCrLf &
                        " '" & Session("PODetailViewBU") & "', '" & txtPO.Text & "',0," & vbCrLf &
                        " '" & txtPO_REF.Text & "'," & vbCrLf &
                        " '" & txtPYMNT_TERMS_CD.Text & "'," & vbCrLf &
                        " '" & txtCNTCT_SEQ_NUM.Text & "'," & vbCrLf &
                        " '" & txtBILL_LOCATION.Text & "'," & vbCrLf &
                        " '" & txtTAX_EXEMPT.Text & "'," & vbCrLf &
                        " '" & txtTAX_EXEMPT_ID.Text & "'," & vbCrLf &
                        " '" & txtCURRENCY_CD_HDR.Text & "'," & vbCrLf &
                        " '" & txtRT_TYPE.Text & "'," & vbCrLf &
                        " TO_DATE('" & dteNowd & "','MM-DD-YYYY')," & vbCrLf &
                        " '" & strStatus & "'," & vbCrLf &
                        " TO_DATE('" & dteNowd & "','MM-DD-YYYY')," & vbCrLf &
                        " 'N','','" & Session("OPERID") & "',' ',' ',' ',' ',' ',' ',' ',' ',' ',' ')"

            rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            If rowsaffected = 0 Then
                bolUpdateFailed = True
            End If

            If bolUpdateFailed = True Then
                strSQLstring = "DELETE FROM PS_ECQUEUE" & vbCrLf &
                        " WHERE ECTRANSID = 'PO ACK'" & vbCrLf &
                        " AND ECQUEUEINSTANCE = " & intQueueInst & vbCrLf &
                        " AND ECTRANSINOUTSW = 'I'" & vbCrLf

                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

                strSQLstring = "DELETE FROM PS_PO_LINE_EC" & vbCrLf &
                          " WHERE EIP_CTL_ID = 'PO ACK" & intQueueInst & "I'"


                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

                strSQLstring = "DELETE FROM PS_PO_LINE_SHIP_EC" & vbCrLf &
                        " WHERE EIP_CTL_ID = 'PO ACK" & intQueueInst & "I'"


                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

                strSQLstring = "DELETE FROM PS_PO_HDR_EC" & vbCrLf &
                          " WHERE EIP_CTL_ID = 'PO ACK" & intQueueInst & "I'"

                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

            End If

            Return bolUpdateFailed
        Catch ex As Exception
            Dim sMyErrorString As String = ""
            If Not ex Is Nothing Then
                sMyErrorString = ex.ToString
            End If
            sMyErrorString += sMyErrorString &
                       "Error in  -> createECConfirmationDueUpdate" & vbCrLf &
                       "PO = " & txtPO.Text & ", Business Unit = " & txtPOBU.Text & " ; Server: " & HttpContext.Current.Session("WEBSITEID")
            SendSDiExchErrorMail(sMyErrorString, "Error in PODetails -> createECConfirmationDueUpdate")
        End Try

    End Function

    '' ASN methods Starts here

    Private Sub getPOASNDetails(strPoNum1 As String, m_sVendor As String, strPOBU As String)
        Dim strtxtSmallSite As String = ""
        Dim strSQLstring As String = ""
        Dim decHldQtyRec As Decimal
        Dim strASNReceiving As String = ""
        Dim dsdr As DataRow
        Dim I As Integer = 0
        Dim P As Integer


        Try
            strtxtSmallSite = "Y"
            If txtasnflag.Text.Trim = "" Then
                txtasnflag.Text = getASNFlag(strPoNum1)
            End If
            strASNReceiving = txtasnflag.Text
            dsPOdata = getpoinfoASN(strPoNum1, m_sVendor, strPOBU, strtxtSmallSite, strASNReceiving)
            Dim decHldQtyAccpt As Decimal
            If dsPOdata.Tables.Count > 0 Then
                If dsPOdata.Tables(0).Rows.Count > 0 Then

                    Do While I <= dsPOdata.Tables(0).Rows.Count

                        If I = dsPOdata.Tables(0).Rows.Count Then
                            If dsPOdata.Tables(0).Rows(I - 1).Item("QTY_PO") > decHldQtyRec And
                                decHldQtyRec > 0 Then
                                getNewdatarow((I - 1), decHldQtyRec)
                            End If
                            Exit Do
                        End If

                        If strtxtSmallSite = "Y" Then
                            strSQLstring = "SELECT TO_CHAR(A.ISA_ASN_SHIP_DT,'MM/DD/YYYY') as ISA_ASN_SHIP_DT," & vbCrLf &
                            " A.ISA_ASN_TRACK_NO, A.ISA_ASN_SHIP_VIA," & vbCrLf &
                            " B.DESCR" & vbCrLf &
                            " FROM PS_ISA_ASN_SHIPPED A, PS_SHIP_METHOD B" & vbCrLf &
                            " WHERE A.ISA_SHIP_ID = '" & dsPOdata.Tables(0).Rows(I).Item("ISA_SHIP_ID") & "'" & vbCrLf &
                            " AND A.ISA_ASN_SHIP_VIA = B.SHIP_TYPE_ID(+)"
                        Else
                            strSQLstring = "SELECT TO_CHAR(A.ISA_ASN_SHIP_DT,'MM/DD/YYYY') as ISA_ASN_SHIP_DT," & vbCrLf &
                            " A.ISA_ASN_TRACK_NO, A.ISA_ASN_SHIP_VIA," & vbCrLf &
                            " B.DESCR" & vbCrLf &
                            " FROM PS_ISA_RECV_LN_ASN A, PS_SHIP_METHOD B" & vbCrLf &
                            " WHERE BUSINESS_UNIT = '" & dsPOdata.Tables(0).Rows(I).Item("BUSINESS_UNIT") & "'" & vbCrLf &
                            " AND RECEIVER_ID = '" & dsPOdata.Tables(0).Rows(I).Item("RECEIVER_ID") & "'" & vbCrLf &
                            " AND RECV_LN_NBR = '" & dsPOdata.Tables(0).Rows(I).Item("RECV_LN_NBR") & "'" & vbCrLf &
                            " AND A.ISA_ASN_SHIP_VIA = B.SHIP_TYPE_ID(+)"
                        End If

                        Dim dr As OleDbDataReader = ORDBData.GetReader(strSQLstring)
                        ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                        Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
                        If m_weblogstring = "true" Then
                            'WebLogOpenConn()
                        End If
                        If dr.Read Then
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT") = dr.Item("ISA_ASN_SHIP_DT")
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO") = Trim(dr.Item("ISA_ASN_TRACK_NO"))
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = dr.Item("DESCR")
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID") = dr.Item("ISA_ASN_SHIP_VIA")
                            'if this is the first row.. save for defaults for any row that is empty

                            'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                            If m_weblogstring = "true" Then
                                'WebLogCloseConn()
                            End If
                        Else
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT") = ""
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO") = ""
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = ""
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID") = ""
                        End If
                        dr.Close()

                        If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT")) Then
                            dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT") = "0"
                        End If
                        If I > 0 Then
                            If IsDBNull(dsPOdata.Tables(0).Rows(I - 1).Item("QTY_LN_ACCPT")) Then
                                dsPOdata.Tables(0).Rows(I - 1).Item("QTY_LN_ACCPT") = "0"
                            End If
                        End If

                        If I = 0 Then
                            decHldQtyRec = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        ElseIf dsPOdata.Tables(0).Rows(I).Item("LINE_NBR") =
                                dsPOdata.Tables(0).Rows(I - 1).Item("LINE_NBR") And
                                dsPOdata.Tables(0).Rows(I).Item("SCHED_NBR") =
                                dsPOdata.Tables(0).Rows(I - 1).Item("SCHED_NBR") Then
                            decHldQtyRec = decHldQtyRec + Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        ElseIf dsPOdata.Tables(0).Rows(I - 1).Item("QTY_PO") > decHldQtyRec And
                              decHldQtyRec > 0 Then
                            getNewdatarow((I - 1), decHldQtyRec)
                            I += 1
                            decHldQtyRec = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        Else
                            decHldQtyRec = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        End If
                        I += 1
                    Loop
                    dsPOdata.AcceptChanges()

                    'delete invisible rows
                    Dim strRecvID As String = " "
                    If Not dsPOdata Is Nothing Then
                        If dsPOdata.Tables.Count > 1 Then
                            If dsPOdata.Tables(0).Rows.Count > 0 Then
                                For I = 0 To dsPOdata.Tables(0).Rows.Count - 1
                                    If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("hQTYLNACCPT")) Then
                                        decHldQtyAccpt = 0
                                    Else
                                        decHldQtyAccpt = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("hQTYLNACCPT"))
                                    End If

                                    If decHldQtyAccpt > 0 Then
                                        'row is invisible
                                        Dim rowD As DataRow = dsPOdata.Tables(0).Rows(I)
                                        rowD.Delete()
                                    Else
                                        Try
                                            strRecvID = dsPOdata.Tables(0).Rows(I).Item("RECEIVER_ID")
                                        Catch ex As Exception
                                            strRecvID = " "
                                        End Try
                                        If Trim(strRecvID) <> "" Then
                                            'row is invisible
                                            Dim rowD As DataRow = dsPOdata.Tables(0).Rows(I)
                                            rowD.Delete()
                                        End If
                                    End If
                                Next
                                dsPOdata.AcceptChanges()
                            End If
                        End If
                    End If



                    '' Shipped Qty calculation for the non receiving users
                    If Not txtasnflag.Text = "Y" Then
                        Dim dsRecvTotals As DataSet = New DataSet()
                        Dim dtDistinctRows As DataTable

                        '' Distinct to remove duplicate rows
                        dtDistinctRows = dsPOdata.Tables(0).AsEnumerable().GroupBy(Function(row) row.Field(Of Decimal)("LINE_NBR")).Select(Function(group) group.First()).CopyToDataTable()
                        dsPOdata = New DataSet()
                        dsPOdata.Tables.Add(dtDistinctRows)

                        dsRecvTotals = getAccptRecvTotals(strPoNum1, strPOBU)

                        If Not dsRecvTotals Is Nothing Then
                            If dsRecvTotals.Tables.Count > 0 Then
                                If dsRecvTotals.Tables(0).Rows.Count > 0 Then
                                    Dim Y As Integer = 0
                                    Dim bResExists As Boolean = False
                                    For I = 0 To dsPOdata.Tables(0).Rows.Count - 1
                                        bResExists = False
                                        For Y = 0 To dsRecvTotals.Tables(0).Rows.Count - 1
                                            If dsPOdata.Tables(0).Rows(I).Item("LINE_NBR") = dsRecvTotals.Tables(0).Rows(Y).Item("LINE_NBR") Then

                                                bResExists = True
                                                Exit For
                                            End If
                                        Next
                                        If Not bResExists Then
                                            'no recv lines for this PO line s- go to the next line
                                        Else
                                            'receiving exists for selected PO line - 1) compare with PO Qty and either 2.1) delete PO line or 2.2) update this line
                                            If CType(dsPOdata.Tables(0).Rows(I).Item("QTY_PO"), Decimal) > CType(dsRecvTotals.Tables(0).Rows(Y).Item("LINE_ACCPT_TOTAL"), Decimal) Then
                                                'update this line
                                                dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT") = dsRecvTotals.Tables(0).Rows(Y).Item("LINE_ACCPT_TOTAL")
                                                '' dsPOdata.Tables(0).Rows(I).Item("hQTYLNACCPT") = dsRecvTotals.Tables(0).Rows(Y).Item("LINE_ACCPT_TOTAL")
                                            Else
                                                'delete this line - it is fully receiveds
                                                Dim rowD As DataRow = dsPOdata.Tables(0).Rows(I)
                                                rowD.Delete()
                                            End If
                                        End If
                                    Next
                                    dsPOdata.AcceptChanges()
                                End If
                            End If
                        End If
                    End If

                    Dim GetRecvFlag As String = ""
                    GetRecvFlag = getASNFlag(strPoNum1)
                    Dim S As Integer = 0

                    If GetRecvFlag.Trim = "Y" Then
                        For S = 0 To dsPOdata.Tables(0).Rows.Count - 1
                            Dim RecvQty As String = ""
                            RecvQty = getAccptRecvTblRecvQty(strPoNum1, strPOBU, Convert.ToString(dsPOdata.Tables(0).Rows(S).Item("LINE_NBR")))
                            If Trim(RecvQty) <> "" Then
                                If CType(dsPOdata.Tables(0).Rows(S).Item("QTY_PO"), Decimal) > CType(RecvQty, Decimal) Then
                                    dsPOdata.Tables(0).Rows(S).Item("QTY_LN_ACCPT") = RecvQty
                                Else
                                    'delete this line - it is fully receiveds
                                    Dim rowD As DataRow = dsPOdata.Tables(0).Rows(S)
                                    rowD.Delete()
                                End If
                            Else
                                dsPOdata.Tables(0).Rows(S).Item("QTY_LN_ACCPT") = "0"
                            End If
                        Next
                        dsPOdata.AcceptChanges()
                    End If


                    dtgPOASN.DataSource = dsPOdata
                    dtgPOASN.DataBind()

                    btnASNSubmit.Visible = True
                    btnASNSubmit.Enabled = True
                    lblPOASNSubmitErrorInfo.Visible = False
                    If IsDBNull(dsPOdata.Tables(0).Rows(0).Item("ERS_ACTION")) Then
                        txtERSAction.Text = " "
                    Else
                        txtERSAction.Text = dsPOdata.Tables(0).Rows(0).Item("ERS_ACTION")
                    End If
                    If IsDBNull(dsPOdata.Tables(0).Rows(0).Item("PODT")) Then
                        lblPODatetxt.Text = " "
                    Else
                        lblPODatetxt.Text = dsPOdata.Tables(0).Rows(0).Item("PODT")
                    End If
                    If IsDBNull(dsPOdata.Tables(0).Rows(0).Item("VNDR_LOC")) Then
                        txtvndrLoc.Text = " "
                    Else
                        txtvndrLoc.Text = dsPOdata.Tables(0).Rows(0).Item("VNDR_LOC")
                    End If
                    If Trim(txtvndrLoc.Text) = "" Then
                        txtvndrLoc.Text = "1"
                        'generate e-mail
                        Dim strProblem As String = "Problem in ASNSub: VNDR_LOC changed to '1'. PO: " & strPoNum1 & " Vendor: " & m_sVendor & " ERSAction: " & txtERSAction.Text & " PO BUSINESS UNIT: " & Session("PoSiteBu") & " "
                        Call SendSDiExchErrorMail(strProblem, "Problem in ASNSub.aspx.vb : VNDR_LOC changed to '1'.")
                    End If
                    Session("PODATA") = dsPOdata

                    Call ProcessGrid()

                    SetFocus1(txtCustomerShipping)
                Else
                    dtgPOASN.Visible = False
                    btnASNSubmit.Visible = False
                    lblPOASNSubmitErrorInfo.Visible = True
                    lblPOASNSubmitErrorInfo.Text = "No info when loading screen. PO #: " & strPoNum1 & " -- BU: " & strPOBU & " -- Supplier ID: " & m_sVendor & vbCrLf

                End If  '  dsPOdata.Tables(0).Rows.Count > 0 
            Else
                dtgPOASN.Visible = False
                btnASNSubmit.Visible = False
                lblPOASNSubmitErrorInfo.Visible = True
                lblPOASNSubmitErrorInfo.Text = "No info when loading screen. PO #: " & strPoNum1 & " -- BU: " & strPOBU & " -- Supplier ID: " & m_sVendor & vbCrLf
            End If


        Catch ex As Exception

        End Try
    End Sub

    Private Function getPOWorkOrdNum(ByVal poBU As String) As Boolean
        Dim workOrdAvail As Boolean = False
        Dim strReqID As String = ""
        Dim strWorkOrdNo As String = ""
        Dim strQuery As String = ""
        Try
            strQuery = "SELECT ISA_WORK_ORDER_NO FROM PS_ISA_PO_LINE WHERE PO_ID='" & poBU & "'"
            strWorkOrdNo = ORDBData.GetScalar(strQuery)

            If strWorkOrdNo.Trim.Length > 0 Then
                divWorkOrd.Visible = True
                lblWrkOrdNo.Text = strWorkOrdNo.Trim
                workOrdAvail = True
            End If

        Catch ex As Exception

        End Try
        Return workOrdAvail
    End Function
    Private Function getAccptRecvTotals(ByVal ponum As String, ByVal poBU As String) As DataSet
        Dim strSQLString As String
        Dim dsRecvTotals As DataSet = New DataSet()
        Try
            strSQLString = strSQLString & " SELECT SUM(LINE_SHIP_QTY) as LINE_ACCPT_TOTAL,LINE_NBR  FROM " & vbCrLf &
                   " (SELECT D1.QTY_LN_ACCPT_VUOM AS LINE_SHIP_QTY,D1.LINE_NBR FROM PS_ISA_ASN_SHIPPED D1 " & vbCrLf &
                   " WHERE D1.BUSINESS_UNIT    = '" + poBU + "'  " & vbCrLf &
                   " AND   D1.PO_ID            = '" + ponum + "' ) GROUP BY LINE_NBR " & vbCrLf

            dsRecvTotals = ORDBData.GetAdapter(strSQLString)

        Catch ex As Exception

        End Try

        Return dsRecvTotals

    End Function

    Private Function getAccptRecvTblRecvQty(ByVal ponum As String, ByVal poBU As String, ByVal LineNo As String) As String
        Dim strSQLString As String
        Dim recvtotal As String = ""
        Dim dsRecvTotals As DataSet = New DataSet()
        Try
            strSQLString = "SELECT SUM(QTY_SH_NETRCV_VUOM) as totalRecvQty FROM PS_RECV_LN_SHIP WHERE PO_ID = '" & ponum & "' AND BUSINESS_UNIT = '" & poBU & "' AND LINE_NBR = " & LineNo & ""

            dsRecvTotals = ORDBData.GetAdapter(strSQLString)

            If Not dsRecvTotals Is Nothing Then
                If dsRecvTotals.Tables(0).Rows.Count > 0 Then
                    recvtotal = Convert.ToString(dsRecvTotals.Tables(0).Rows(0).Item("totalRecvQty"))
                Else

                End If
            Else

            End If
        Catch ex As Exception

        End Try

        Return recvtotal

    End Function

    Private Sub processANSGrid()

        Dim strtxtSmallSite As String = ""
        Dim strSQLstring As String = ""
        Dim decHldQtyRec As Decimal
        Dim dsdr As DataRow
        Dim I As Integer = 0
        Dim P As Integer
        Dim T As Integer = 0

        Try
            If Session("PODATA") Is Nothing Then
                Session.RemoveAll()
                If Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "INSITEONLINE.ISACS.COM" Or
                Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "WWW.INSITEONLINE.COM" Or
                Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "INSITEONLINE.SDI.COM" Then
                    Response.Redirect("http://www.SDiExchange.com")
                Else
                    Response.Redirect("http://" & Request.ServerVariables("HTTP_HOST") & GetWebAppName1() & "/default.aspx")
                End If
            End If

            dsPOdata = Session("PODATA")
            If dsPOdata.Tables.Count > 0 Then
                If dsPOdata.Tables(0).Rows.Count > 0 Then
                    Dim item As GridDataItem = dtgPOASN.Items(0)
                    Dim txtQuantity As TextBox = CType(item.FindControl("txtQTY"), TextBox)
                    'Dim txtQuantity1 As TextBox = CType(item.FindControl("txtQTY"), TextBox)
                    Dim decQtyAccpt1 As Decimal

                    For I = 0 To dtgPOASN.Items.Count - 1

                        item = dtgPOASN.Items(I)
                        txtQuantity = CType(item.FindControl("txtQTY"), TextBox)
                        ' we want to make sure that there is nothing in the table for this line and there is a quantity keyed in
                        If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT")) Then
                            decQtyAccpt1 = 0
                        Else
                            decQtyAccpt1 = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        End If
                        'txtQuantity = dtgPO.Items(I).Cells(8).FindControl("txtQTY")

                        If txtQuantity.Text <> "" Then
                            If txtQuantity.Text <> "" And
                                                    decQtyAccpt1 = 0 Then
                                getdefault(I, T)
                                '' T is where we want to default from
                                T = I
                                'arrLncmp(
                                Exit For
                            End If
                            If txtQuantity.Text <> "" And I = 0 And decQtyAccpt1 = 0 Then

                                getdefault(I, T)
                                T = I
                                Exit For
                            End If

                            If txtQuantity.Text <> "" And dsPOdata.Tables(0).Rows(I).Item("QTY_PO") > decQtyAccpt1 And
                                   decQtyAccpt1 > 0 Then
                                ' T is where we want to default from

                                getdefault(I, T)

                                T = I
                                Exit For
                            End If
                        End If

                        '''''''''''''''youch!!!!!!!!!
                    Next



                    For I = 0 To dtgPOASN.Items.Count - 1

                        item = dtgPOASN.Items(I)
                        txtQuantity = CType(item.FindControl("txtQTY"), TextBox)
                        Dim ctrl As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
                        Dim ctrl4 As System.Web.UI.Control = Nothing

                        Dim qty1 As String = ""

                        If Not (txtQuantity Is Nothing) Then
                            If TypeOf txtQuantity Is System.Web.UI.WebControls.TextBox Then
                                qty1 = DirectCast(txtQuantity, System.Web.UI.WebControls.TextBox).Text.Trim
                            End If
                        Else
                            qty1 = txtQuantity.Text.Trim
                        End If

                        If Not (qty1 = "" And (dsPOdata.Tables(0).Rows(I).Item("QTY_PO") =
                            dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))) Then


                            'If (txtquantity.Text = "" And decQtyAccpt1 > 0) Then
                            'If DirectCast(ctrl4, System.Web.UI.WebControls.TextBox).Text = "" And decQtyAccpt1 > 0 Then
                            'Else
                            'If Not dtgPO.Items(I).Cells(8).Text = "" Then
                            'dtgPO.Items(I).Cells(8).Text = FormatNumber(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"), 2)
                            If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")) _
                             Then

                                'ctrl = dtgPO.Items(I).Cells(12).FindControl("txtTrckno")
                                If Not (ctrl Is Nothing) Then
                                    If TypeOf ctrl Is System.Web.UI.WebControls.TextBox Then
                                        DirectCast(ctrl, System.Web.UI.WebControls.TextBox).Text = Row1_default_Track_no
                                    End If
                                End If
                            ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO") = "" _
                                 Then
                                'dtgPO.Items(I).Cells(12).Text = Row1_default_Track_no
                                'ctrl = dtgPO.Items(I).Cells(12).FindControl("txtTrckno")
                                If Not (ctrl Is Nothing) Then
                                    If TypeOf ctrl Is System.Web.UI.WebControls.TextBox Then
                                        DirectCast(ctrl, System.Web.UI.WebControls.TextBox).Text = Row1_default_Track_no
                                    End If
                                End If



                            Else
                                'dtgPO.Items(I).Cells(12).Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")
                                'ctrl = dtgPO.Items(I).Cells(12).FindControl("txtTrckno")
                                If Not (ctrl Is Nothing) Then
                                    If TypeOf ctrl Is System.Web.UI.WebControls.TextBox Then
                                        DirectCast(ctrl, System.Web.UI.WebControls.TextBox).Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")
                                    End If
                                End If
                            End If
                            'Dim ctrl1 As System.Web.UI.Control = Nothing
                            'Dim ctrl2 As System.Web.UI.Control = Nothing
                            'Dim ctrl3 As System.Web.UI.Control = Nothing
                            Dim ctrl1 As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
                            Dim ctrl2 As TextBox = CType(item.FindControl("txtcommon"), TextBox)
                            'Dim ctrl3 As TextBox = CType(item.FindControl("txtShipDate"), TextBox)
                            Dim ctrl3 As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)

                            If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA")) _
                            Then
                                If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) _
                                Then
                                    'ctrl1 = dtgPO.Items(I).Cells(13).FindControl("cmbShipvia")
                                    If Not (ctrl1 Is Nothing) Then
                                        If TypeOf ctrl1 Is System.Web.UI.WebControls.DropDownList Then
                                            CType(ctrl1, System.Web.UI.WebControls.DropDownList).SelectedValue = Row1_default_Ship_VIA
                                        End If
                                    End If

                                    ' dtgPO.Items(I).Cells(13).Text = Row1_default_Ship_VIA
                                    'ctrl2 = dtgPO.Items(I).Cells(23).FindControl("txtcommon")
                                    If Not (ctrl2 Is Nothing) Then
                                        If TypeOf ctrl2 Is System.Web.UI.WebControls.TextBox Then
                                            DirectCast(ctrl2, System.Web.UI.WebControls.TextBox).Text = Row1_default_Ship_VIA_ID
                                        End If
                                    End If

                                    'dtgPO.Items(I).Cells(23).Text = Row1_default_Ship_VIA_ID
                                Else
                                    P = InStr(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), "#")
                                    If P > 0 Then
                                        ctrl2.Text = Left$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), P - 1)
                                        ctrl2.Text = Right$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), (Len _
                                       (dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) - P))
                                    Else
                                        ctrl2.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")
                                        ctrl2.Text = " "
                                    End If
                                End If
                            ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = "" _
                            Then
                                If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) _
                                Then
                                    'ctrl1 = dtgPO.Items(I).Cells(13).FindControl("cmbShipvia")
                                    If Not (ctrl1 Is Nothing) Then
                                        If TypeOf ctrl1 Is System.Web.UI.WebControls.DropDownList Then
                                            CType(ctrl1, System.Web.UI.WebControls.DropDownList).SelectedValue = Row1_default_Ship_VIA
                                        End If
                                    End If
                                    'dtgPO.Items(I).Cells(13).Text = Row1_default_Ship_VIA
                                    'dtgPO.Items(I).Cells(23).Text = Row1_default_Ship_VIA_ID
                                    'ctrl2 = dtgPO.Items(I).Cells(23).FindControl("txtcommon")
                                    If Not (ctrl2 Is Nothing) Then
                                        If TypeOf ctrl2 Is System.Web.UI.WebControls.TextBox Then
                                            DirectCast(ctrl2, System.Web.UI.WebControls.TextBox).Text = Row1_default_Ship_VIA_ID
                                        End If
                                    End If

                                ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID") = "" _
                                    Then
                                    'ctrl1 = dtgPO.Items(I).Cells(13).FindControl("cmbShipvia")
                                    If Not (ctrl1 Is Nothing) Then
                                        If TypeOf ctrl1 Is System.Web.UI.WebControls.DropDownList Then
                                            CType(ctrl1, System.Web.UI.WebControls.DropDownList).SelectedValue = Row1_default_Ship_VIA
                                        End If
                                    End If
                                    'dtgPO.Items(I).Cells(13).Text = Row1_default_Ship_VIA
                                    'dtgPO.Items(I).Cells(23).Text = Row1_default_Ship_VIA_ID
                                    'ctrl2 = dtgPO.Items(I).Cells(23).FindControl("txtcommon")
                                    If Not (ctrl2 Is Nothing) Then
                                        If TypeOf ctrl2 Is System.Web.UI.WebControls.TextBox Then
                                            DirectCast(ctrl2, System.Web.UI.WebControls.TextBox).Text = Row1_default_Ship_VIA_ID
                                        End If
                                    End If
                                Else
                                    P = InStr(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), "#")
                                    If P > 0 _
                                     Then
                                        ctrl1.Text = Left$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), P - 1)
                                        ctrl2.Text = Right$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), (Len _
                                        (dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) - P))
                                    Else
                                        ctrl1.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")
                                        ctrl2.Text = " "
                                    End If
                                End If
                            Else
                                ctrl1.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA")
                                ctrl2.Text = " "
                            End If
                            If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")) Then
                                'ctrl3 = dtgPO.Items(I).Cells(14).FindControl("txtShipDate")
                                If Not (ctrl3 Is Nothing) Then
                                    If TypeOf ctrl3 Is RadDatePicker Then
                                        CType(ctrl3, RadDatePicker).SelectedDate = Row1_default_Ship_dt
                                    End If
                                End If

                            ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT") = "" Then
                                'ctrl3 = dtgPO.Items(I).Cells(14).FindControl("txtShipDate")
                                If Not (ctrl3 Is Nothing) Then
                                    If TypeOf ctrl3 Is RadDatePicker Then
                                        CType(ctrl3, RadDatePicker).SelectedDate = Row1_default_Ship_dt
                                    End If
                                End If
                            Else
                                'ctrl3 = dtgPO.Items(I).Cells(14).FindControl("txtShipDate")
                                If Not (ctrl3 Is Nothing) Then
                                    'If TypeOf ctrl3 Is System.Web.UI.WebControls.TextBox Then
                                    '    DirectCast(ctrl3, System.Web.UI.WebControls.TextBox).Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")
                                    If TypeOf ctrl3 Is RadDatePicker Then
                                        DirectCast(ctrl3, RadDatePicker).SelectedDate = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")
                                    End If
                                End If
                                'dtgPO.Items(I).Cells(14).Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")
                            End If
                            'debug
                        Else
                            Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
                            If (dsPOdata.Tables(0).Rows(I).Item("QTY_PO") = dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT")) And
                               (dsPOdata.Tables(0).Rows(I).Item("QTY_PO") > 0) Then
                                txtQuantity.Text = dsPOdata.Tables(0).Rows(I).Item("QTY_PO")
                                txtTrckno.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")
                                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                                '    If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA")) _
                                '        Then
                                '        If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) _
                                '          Then dtgPO.Items(I).Cells(13).Text = " "
                                '        dtgPO.Items(I).Cells(23).Text = " "

                                '    Else
                                '        P = InStr(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), "#")
                                '        If P > 0 Then
                                '            dtgPO.Items(I).Cells(13).Text = Left$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), P - 1)
                                '            dtgPO.Items(I).Cells(23).Text = Right$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), (Len _
                                '           (dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) - P))
                                '        Else
                                '            dtgPO.Items(I).Cells(13).Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")
                                '            dtgPO.Items(I).Cells(23).Text = " "
                                '        End If
                                '    End If
                                'ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = "" _
                                'Then
                                '        If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) _
                                '        Then
                                '        dtgPO.Items(I).Cells(13).Text = " "
                                '        dtgPO.Items(I).Cells(23).Text = " "
                                '    ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID") = "" _
                                '    Then
                                '        dtgPO.Items(I).Cells(13).Text = " "
                                '        dtgPO.Items(I).Cells(23).Text = " "
                                '    Else
                                '        P = InStr(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), "#")
                                '        If P > 0 _
                                '         Then
                                '            dtgPO.Items(I).Cells(13).Text = Left$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), P - 1)
                                '            dtgPO.Items(I).Cells(23).Text = Right$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), (Len _
                                '            (dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) - P))
                                '        Else
                                '            dtgPO.Items(I).Cells(13).Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")
                                '            dtgPO.Items(I).Cells(23).Text = " "
                                '        End If
                                '    End If
                                '    Else
                                '        dtgPO.Items(I).Cells(13).Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA")
                                '        dtgPO.Items(I).Cells(23).Text = " "
                                'End If
                                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                                'Dim txtShipDate As TextBox = CType(item.FindControl("txtShipDate"), TextBox)
                                Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)
                                Dim cmbShipvia As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
                                Dim txtCommon As TextBox = CType(item.FindControl("txtCommon"), TextBox)
                                'txtShipDate.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")
                                dpDeliveredDate.SelectedDate = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")
                                cmbShipvia.Text = cmbShipvia.Text
                                txtCommon.Text = txtCommon.Text
                            End If
                        End If
                    Next
                End If  '  dsPOdata.Tables(0).Rows.Count > 0
            End If  ' dsPOdata.Tables.Count > 0
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ProcessGrid(Optional ByVal bCheck As Boolean = True)
        Dim I As Integer = 0
        Dim P As Integer
        Dim decHldQtyAccpt As Decimal
        Dim decQtyAccpt As Decimal
        Dim decQtyPO1 As Decimal
        Dim item As GridDataItem = dtgPOASN.Items(0)
        Dim txtQTY As TextBox = CType(item.FindControl("txtQTY"), TextBox)
        For I = 0 To dtgPOASN.Items.Count - 1

            If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("QTY_PO")) Then
                decQtyPO1 = 0
            Else
                decQtyPO1 = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_PO"))
            End If
            If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT")) Then
                decQtyAccpt = 0
            Else
                decQtyAccpt = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
            End If
            If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("hQTYLNACCPT")) Then
                decHldQtyAccpt = 0
            Else
                decHldQtyAccpt = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("hQTYLNACCPT"))
            End If

            item = dtgPOASN.Items(I)
            txtQTY = CType(item.FindControl("txtQTY"), TextBox)
            Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
            Dim drpShipVia As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
            Dim txtCommon As TextBox = CType(item.FindControl("txtCommon"), TextBox)
            Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)

            If decHldQtyAccpt > 0 Then

                txtQTY.Text = FormatNumber(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"), 2)

                If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")) Then
                    txtTrckno.Text = " "
                ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO") = "" Then
                    txtTrckno.Text = " "
                Else
                    txtTrckno.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")

                End If
                If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA")) Then
                    If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) Then
                        drpShipVia.Text = " "
                        txtCommon.Text = " "
                    Else
                        P = InStr(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), "#")
                        If P > 0 Then
                            drpShipVia.Text = Left$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), P - 1)
                            txtCommon.Text = Right$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), (Len(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) - P))
                        Else
                            drpShipVia.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")
                            txtCommon.Text = " "
                        End If
                    End If
                ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = "" Then
                    If dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID") = "" Then
                        drpShipVia.Text = " "
                        txtCommon.Text = " "
                    Else
                        P = InStr(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), "#")
                        If P > 0 Then
                            drpShipVia.Text = Left$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), P - 1)
                            txtCommon.Text = Right$(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID"), (Len(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) - P))
                        Else
                            drpShipVia.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")
                            txtCommon.Text = " "
                        End If
                    End If
                Else
                    drpShipVia.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA")
                    txtCommon.Text = " "
                End If
                Dim strShipDate As String = ""
                Try
                    If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")) Then
                        dpDeliveredDate.Clear()
                    ElseIf Trim(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")) = "" Then
                        dpDeliveredDate.Clear()
                    Else
                        strShipDate = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")
                        dpDeliveredDate.DbSelectedDate = CDate(strShipDate)
                    End If
                Catch ex As Exception
                    dpDeliveredDate.Clear()
                End Try
            Else

                Dim txtQuantity As TextBox = CType(item.FindControl("txtQTY"), TextBox)
                Dim txtTrackingnum As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
                Dim txtShipViaID As TextBox = CType(item.FindControl("txtcommon"), TextBox)

                If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")) Then
                    txtTrackingnum.Text = " "

                ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO") = "" Then
                    txtTrackingnum.Text = " "
                Else
                    txtTrackingnum.Text = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO")
                End If

                drpShipVia.DataBind()

                If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA")) Then
                    ''''''''''''''''''''''''''''''''''''''''''''''''here''''''''''''''''''''''''''''''''''''''''''''''''''''

                    drpShipVia.Items.FindByText("Common").Selected = True
                ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = "" Then
                    drpShipVia.Items.FindByText("Common").Selected = True

                Else
                    If Not drpShipVia.Items.FindByValue(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")) Is Nothing Then
                        drpShipVia.Items.FindByValue(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID")).Selected = True

                        txtShipViaID.Text = " "
                    Else
                        drpShipVia.Items.FindByValue(Row1_default_Ship_VIA).Selected = True
                        drpShipVia.Items.FindByText("Common").Selected = True
                    End If
                End If

                ''''''''''''''''
                If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA")) Then
                    txtShipViaID.Text = " "
                ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = "" Then
                    txtShipViaID.Text = " "
                End If


                ''''''''''''''''''
                Try
                    If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")) Then

                        dpDeliveredDate.DbSelectedDate = dteNowd
                    ElseIf Trim(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")) = "" Then

                        dpDeliveredDate.DbSelectedDate = dteNowd
                    Else
                        Dim dDate1 As Date = dteNowd
                        Try
                            dDate1 = CDate(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT"))
                        Catch ex As Exception
                            dDate1 = dteNowd
                        End Try
                        dpDeliveredDate.DbSelectedDate = dDate1
                    End If
                Catch ex As Exception
                    Try
                        dpDeliveredDate.DbSelectedDate = dteNowd
                    Catch ex1 As Exception

                    End Try
                End Try

                txtQuantity.Attributes.Add("onkeypress", "CheckNumeric()")
                txtQuantity.Attributes.Add("onblur", "CheckQTY(" & System.Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_PO")) & ", " & decQtyAccpt & ", this)")
                'txtTrackingnum.Attributes.Add("onblur", "CheckTrckNo(this)")

                Dim lblQtyPo As Label = CType(item.FindControl("lblQtyPo"), Label)
                Dim lblShippedQty As Label = CType(item.FindControl("lblShippedQty"), Label)
                lblQtyPo.ForeColor = Color.Red
                lblShippedQty.ForeColor = Color.Red

            End If
            Dim txtQTY_text As TextBox = CType(item.FindControl("txtQTY"), TextBox)
            Dim txtTrckno_Text As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
            Dim cmbShipvia_Text As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
            Dim DeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)
            Dim CommonCarrier As TextBox = CType(item.FindControl("txtCommon"), TextBox)

            If bCheck Then
                If txtQTY_text.Text <> "" Then
                    txtTrckno_Text.Visible = False
                    cmbShipvia_Text.Visible = False
                    DeliveredDate.Visible = False
                    CommonCarrier.Visible = False
                    txtQTY_text.Text = ""
                    'item.Visible = False
                Else
                    Dim strRecvID As String = " "
                    Try
                        strRecvID = dsPOdata.Tables(0).Rows(I).Item("RECEIVER_ID")
                    Catch ex As Exception
                        strRecvID = " "
                    End Try
                    If Trim(strRecvID) <> "" Then
                        txtTrckno_Text.Visible = False
                        cmbShipvia_Text.Visible = False
                        DeliveredDate.Visible = False
                        CommonCarrier.Visible = False
                        txtQTY_text.Text = ""
                        'item.Visible = False
                    End If
                End If
            End If

        Next
    End Sub

    Private Sub SetFocus1(ByVal FocusControl As Control)
        Dim Script As New System.Text.StringBuilder
        Dim ClientID As String = FocusControl.ClientID
        With Script
            .Append("<script language='javascript'>")
            .Append("document.getElementById('")
            .Append(ClientID)
            .Append("').focus();")
            .Append("</script>")
        End With

        RegisterStartupScript("setFocus", Script.ToString())

    End Sub

    Sub getdefault(ByVal I As Integer, ByVal T As Integer)
        Try

            Dim item As GridDataItem = dtgPOASN.Items(I)
            'Dim txtShipDate As TextBox = CType(item.FindControl("txtShipDate"), TextBox)
            Dim cmbShipvia As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
            Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)
            Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
            Dim txtCommon As TextBox = CType(item.FindControl("txtCommon"), TextBox)

            T = I
            'the first part will take care of EDI transactions where there is no trckno,shipdate,shipping info
            If Not (dsPOdata.Tables(0).Rows(I).Item("QTY_PO") =
                dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT") _
                And dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT") > 0) Then
                Row1_default_Track_no = CType(txtTrckno, TextBox).Text.ToUpper
                'Row1_default_Ship_dt = CType(txtShipDate, TextBox).Text
                Row1_default_Ship_dt = CType(dpDeliveredDate, RadDatePicker).SelectedDate
                Row1_default_Ship_VIA_ID = CType(txtCommon, TextBox).Text.ToUpper
                Dim shipvia As DropDownList = CType(cmbShipvia, DropDownList)
                Row1_default_Ship_VIA = shipvia.SelectedValue
                If IsDBNull(Row1_default_Ship_VIA) Then
                    Row1_default_Ship_VIA = "COMMON"
                ElseIf Row1_default_Ship_VIA = " " Then
                    Row1_default_Ship_VIA = "COMMON"
                End If
            End If

        Catch ex As Exception

        End Try
    End Sub

    Sub getNewdatarow(ByVal I As Integer, ByVal decHldQtyRec As Decimal)

        Dim dsdr As DataRow = dsPOdata.Tables(0).NewRow

        dsdr.Item("ERS_ACTION") = dsPOdata.Tables(0).Rows(I).Item("ERS_ACTION")
        dsdr.Item("VNDR_LOC") = dsPOdata.Tables(0).Rows(I).Item("VNDR_LOC")
        dsdr.Item("PODT") = dsPOdata.Tables(0).Rows(I).Item("PODT")

        dsdr.Item("LINE_NBR") = dsPOdata.Tables(0).Rows(I).Item("LINE_NBR")
        dsdr.Item("SCHED_NBR") = dsPOdata.Tables(0).Rows(I).Item("SCHED_NBR")
        dsdr.Item("INV_ITEM_ID") = dsPOdata.Tables(0).Rows(I).Item("INV_ITEM_ID")
        dsdr.Item("MFG_ITM_ID") = dsPOdata.Tables(0).Rows(I).Item("MFG_ITM_ID")
        dsdr.Item("ITM_ID_VNDR") = dsPOdata.Tables(0).Rows(I).Item("ITM_ID_VNDR")
        dsdr.Item("DESCR254_MIXED") = dsPOdata.Tables(0).Rows(I).Item("DESCR254_MIXED")
        dsdr.Item("QTY_PO") = dsPOdata.Tables(0).Rows(I).Item("QTY_PO")
        dsdr.Item("SHIPTO_ID") = dsPOdata.Tables(0).Rows(I).Item("SHIPTO_ID")
        dsdr.Item("QTY_LN_ACCPT") = decHldQtyRec
        dsdr.Item("UNIT_OF_MEASURE") = dsPOdata.Tables(0).Rows(I).Item("UNIT_OF_MEASURE")
        dsdr.Item("PRICE_PO") = dsPOdata.Tables(0).Rows(I).Item("PRICE_PO")
        dsdr.Item("MERCHANDISE_AMT") = dsPOdata.Tables(0).Rows(I).Item("MERCHANDISE_AMT")
        dsdr.Item("MFG_ID") = dsPOdata.Tables(0).Rows(I).Item("MFG_ID")
        dsdr.Item("INV_STOCK_TYPE") = dsPOdata.Tables(0).Rows(I).Item("INV_STOCK_TYPE")
        dsdr.Item("hQTYPO") = dsPOdata.Tables(0).Rows(I).Item("hQTYPO")
        dsdr.Item("hQTYLNACCPT") = 0
        dsdr.Item("BUSINESS_UNIT") = ""
        dsdr.Item("RECEIVER_ID") = ""
        dsdr.Item("RECV_LN_NBR") = 0
        dsdr.Item("ISA_SHIP_ID") = 0
        dsdr.Item("hMERCHANDISEAMT") = dsPOdata.Tables(0).Rows(I).Item("hMERCHANDISEAMT")
        'dsdr.Item("ISA_ASN_SHIP_DT") = ""
        If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")) Then
            dsdr.Item("ISA_ASN_SHIP_DT") = ""
        ElseIf dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT") = "&NBSP" Then
            dsdr.Item("ISA_ASN_SHIP_DT") = ""
        Else
            dsdr.Item("ISA_ASN_SHIP_DT") = dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT")
        End If
        dsdr.Item("ISA_ASN_TRACK_NO") = ""
        dsdr.Item("ISA_ASN_SHIP_VIA") = ""

        dsdr.Item("PO_BU") = dsPOdata.Tables(0).Rows(I).Item("PO_BU")
        dsdr.Item("CURRENCY_CD") = dsPOdata.Tables(0).Rows(I).Item("CURRENCY_CD")
        dsdr.Item("SHIP_CURRENCY") = dsPOdata.Tables(0).Rows(I).Item("SHIP_CURRENCY")
        dsdr.Item("SHIP_BASE_CURRENCY") = dsPOdata.Tables(0).Rows(I).Item("SHIP_BASE_CURRENCY")
        dsdr.Item("DIST_CURRENCY") = dsPOdata.Tables(0).Rows(I).Item("DIST_CURRENCY")
        dsdr.Item("DIST_BASE_CURRENCY") = dsPOdata.Tables(0).Rows(I).Item("DIST_BASE_CURRENCY")

        dsPOdata.Tables(0).Rows.InsertAt(dsdr, (I + 1))
        dsPOdata.AcceptChanges()
    End Sub

    Function GetLNAccpt(ByVal decQty) As Decimal
        If IsDBNull(decQty) Then
            Return 0
        Else
            Return Convert.ToDecimal(decQty)
        End If

    End Function

    ''To get ASN Ship-via grid list
    Public Sub PopulateDropDownList()

        Dim strSQLString As String

        strSQLString = "SELECT SHIP_TYPE_ID, DESCRSHORT" & vbCrLf &
                        " FROM PS_SHIP_METHOD" & vbCrLf &
                        " where SETID = 'MAIN1'"

        dsShipVia = ORDBData.GetAdapter(strSQLString)

    End Sub

    Private Sub btnASNSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnASNSubmit.Click

        Dim trnsactSession As OleDbTransaction = Nothing
        Dim connection As OleDbConnection = New OleDbConnection(DbUrl)
        Dim myDateTimeUS As System.DateTime
        Dim format As New System.Globalization.CultureInfo("en-US", True)


        Try

            processANSGrid()

            'If Session("submit") = "F" Then
            '    Response.Redirect(GetNewVendorPortalPath() & "/ASNSub.aspx?PONUM=" & txtPO.Text & "&SHIPID=" & txtCustomerShipping.Text)
            'End If
            'Session("submit") = "F"
            Dim bolError As Boolean
            Dim bolErrUPS As Boolean
            Dim I As Integer = 0
            Dim strMessageText As String = ""

            If dtgPOASN.Items.Count > 0 Then
                Dim item As GridDataItem = dtgPOASN.Items(I)
                'Dim txtQTY As TextBox = CType(item.FindControl("txtQTY"), TextBox)
                ''Dim txtShipDate As TextBox = CType(item.FindControl("txtShipDate"), TextBox)
                'Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)
                'Dim cmbShipvia As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
                'Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
                'Dim txtCommon As TextBox = CType(item.FindControl("txtCommon"), TextBox)

                Dim txtQTY As TextBox
                'Dim txtShipDate As TextBox = CType(item.FindControl("txtShipDate"), TextBox)
                Dim dpDeliveredDate As RadDatePicker
                Dim cmbShipvia As DropDownList
                Dim txtTrckno As TextBox
                Dim txtCommon As TextBox
                Dim AnyitemsEntered As String = ""

                lblPOASNSubmitErrorInfo.Text = ""
                '' lblPOASNSubmitErrorInfo.Text = ""
                For I = 0 To dtgPOASN.Items.Count - 1

                    item = dtgPOASN.Items(I)
                    If item.Visible Then
                        txtQTY = CType(item.FindControl("txtQTY"), TextBox)
                        dpDeliveredDate = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)
                        cmbShipvia = CType(item.FindControl("cmbShipvia"), DropDownList)
                        txtTrckno = CType(item.FindControl("txtTrckno"), TextBox)
                        txtCommon = CType(item.FindControl("txtCommon"), TextBox)

                        If txtQTY.Text.Trim = "0" Then
                            lblPOASNSubmitErrorInfo.Text = "Please enter valid quantity."
                            lblPOASNSubmitErrorInfo.Visible = True
                            Exit Sub
                        Else
                            lblPOASNSubmitErrorInfo.Visible = False
                        End If

                        If Not txtQTY.Text.Trim = "" Then
                            AnyitemsEntered = "Y"
                            Dim QTYReceived = Convert.ToDecimal((item("QTY_PO").Text))
                            Dim QTYAccepted = Convert.ToDecimal((item("QTY_LN_ACCPT").Text))
                            Dim QTYEntered = Convert.ToDecimal((txtQTY.Text))
                            If (QTYEntered + QTYAccepted > QTYReceived) Then
                                lblPOASNSubmitErrorInfo.Text = "Quantity entered is greater than open quantity."
                                lblPOASNSubmitErrorInfo.Visible = True
                                Exit Sub
                            Else
                                lblPOASNSubmitErrorInfo.Visible = False
                            End If
                        End If



                        If txtQTY.Text <> "" Then
                            'txtShipDate.BackColor = White
                            dpDeliveredDate.BackColor = White
                            If (CType(txtQTY, TextBox).Text() = "" Or CType(txtQTY, TextBox).Text() = "0") Then
                                'recv quantity not updated
                            Else


                                If CType(cmbShipvia, DropDownList).SelectedValue = "AIRBORNE" Then
                                    If Trim(CType(txtTrckno, TextBox).Text) = "" Then
                                        lblPOASNSubmitErrorInfo.Text = "Tracking Number is required. "
                                        txtTrckno.BackColor = LightPink
                                        bolError = True

                                    End If
                                ElseIf Left(CType(cmbShipvia, DropDownList).SelectedValue, 5) = "FEDEX" Then
                                    If Trim(CType(txtTrckno, TextBox).Text) = "" Then
                                        lblPOASNSubmitErrorInfo.Text = "Tracking Number is required. "
                                        txtTrckno.BackColor = LightPink
                                        bolError = True
                                    Else
                                        bolErrUPS = checkFEDEXnumber(Replace(CType(txtTrckno, TextBox).Text, " ", ""))
                                        If bolErrUPS = True Then
                                            lblPOASNSubmitErrorInfo.Text = "Not a valid FEDEX tracking number. "
                                            txtTrckno.BackColor = LightPink  '  Red
                                            bolError = True
                                        End If
                                    End If

                                ElseIf Left(CType(cmbShipvia, DropDownList).SelectedValue, 3) = "UPS" Then
                                    bolErrUPS = checkUPSnumber(Replace(CType(txtTrckno, TextBox).Text, " ", ""))
                                    If bolErrUPS = True Then
                                        lblPOASNSubmitErrorInfo.Text = "Not a valid UPS tracking number. "
                                        txtTrckno.BackColor = LightPink  '  Red
                                        bolError = True
                                    End If

                                ElseIf CType(cmbShipvia, DropDownList).SelectedValue = "COMMON" Then
                                    If Trim(CType(txtCommon, TextBox).Text) = "" Then
                                        lblPOASNSubmitErrorInfo.Text = "Additional Common Carrier information is required when selecting COMMON as the shipping method. "
                                        txtCommon.BackColor = LightPink
                                        bolError = True
                                    End If
                                Else
                                    txtTrckno.BackColor = White
                                End If

                            End If
                            Try
                                Dim CurrentDate As Date
                                CurrentDate = System.DateTime.Parse(System.DateTime.Today.Date, format)
                                'myDateTimeUS = System.DateTime.Parse(Trim(CType(txtShipDate, TextBox).Text), format)
                                myDateTimeUS = System.DateTime.Parse(Trim(CType(dpDeliveredDate, RadDatePicker).SelectedDate), format)

                                If Session("PODetailViewBU") = "WAL00" Then
                                Else
                                    If myDateTimeUS >= CurrentDate Then

                                    Else
                                        lblPOASNSubmitErrorInfo.Text = "Delivery date contains past date."
                                        'txtShipDate.BackColor = Red
                                        dpDeliveredDate.BackColor = LightPink  '  Red
                                        bolError = True
                                    End If
                                End If

                            Catch ex As Exception
                                lblPOASNSubmitErrorInfo.Text = "*Invalid Shipping Date."
                                'txtShipDate.BackColor = Red
                                dpDeliveredDate.BackColor = LightPink  '  Red
                                bolError = True
                            End Try
                        End If

                    End If
                Next
                If bolError = True Then
                    'Session("submit") = "T"
                    lblPOASNSubmitErrorInfo.Visible = True
                    Exit Sub
                Else
                    If AnyitemsEntered = "" Then
                        lblPOASNSubmitErrorInfo.Text = "Please enter quantity for the line item"
                        lblPOASNSubmitErrorInfo.Visible = True
                        Exit Sub
                    Else
                        lblPOASNSubmitErrorInfo.Visible = False
                    End If
                End If

                Dim intNextReceiver As Int64
                Dim strNextReceiver As String
                Dim intLines As Integer = 0
                Dim decQty As Decimal
                Dim strPOBU As String
                Dim strSmallSite As String
                Dim strASNSite As String
                Dim ReceivingWebServiceAlreadyRun As Boolean = False


                strSmallSite = txtSmallSite.Text
                strASNSite = txtasnflag.Text


                ' here is how we create receipts
                'Small Site  ASN    
                '_______________________
                'Y            <>Y                  Creates an Xref comments record
                'Y              Y                  RECEIVER is created when the site does receiving
                'N              Y                  Creates an receiver record 
                'N              <>Y                Creates an xref comment record
                '_______________________

                '
                'changes below are to use clsReceiver from SDiExchange
                Dim arrParamdtgLine As ArrayList
                connection.Open()
                trnsactSession = connection.BeginTransaction
                Dim strUserId As String = ""
                Dim strTariffCode As String = ""
                Dim strItemWeight As String = ""
                Try
                    If Not Session("OPERID") Is Nothing Then
                        strUserId = Convert.ToString(Session("OPERID"))
                        If strUserId.Length > 10 Then
                            strUserId = strUserId.Trim.Substring(0, 10)
                        End If
                    Else
                        strUserId = "ASNVEND"
                    End If
                    If Trim(strUserId) = "" Then
                        strUserId = "ASNVEND"
                    End If
                Catch ex As Exception
                    strUserId = "ASNVEND"
                End Try
                'Dim str1 As String = ""

                For I = 0 To dtgPOASN.Items.Count - 1

                    item = dtgPOASN.Items(I)
                    If item.Visible Then
                        txtQTY = CType(item.FindControl("txtQTY"), TextBox)
                        'str1 = item("ISA_ASN_SHIP_DT").Text.ToUpper
                        If txtQTY.Text <> "" Then
                            If (Trim(txtQTY.Text) = "") Or (CType(txtQTY, TextBox).Text() = "0") Then
                                'recv quantity not updated
                            Else
                                'If strSmallSite = "Y" Then
                                '    createASNShipped(I)
                                If strSmallSite = "N" And strASNSite <> "Y" Then
                                    createXrefComment(I)
                                    'createASNShipped(I)
                                ElseIf strSmallSite = "Y" And strASNSite <> "Y" Then

                                    createXrefComment(I)
                                    createASNShipped(I)
                                Else

                                    'Yury '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                                    Try
                                        If ReceivingWebServiceAlreadyRun = False Then
                                            If Insiteonline.clsReceiver.CallReceiverWebService(Session("PODetailViewBU"), txtPO.Text, dtgPOASN, strMessageText, strNextReceiver) = False Then
                                                'If InStr(UCase(strMessageText), "SUCCESS") = 0 Then
                                                trnsactSession.Rollback()
                                                connection.Close()
                                                btnASNSubmit.Enabled = False
                                                btnASNSubmit.Visible = False
                                                lblPOASNSubmitErrorInfo.Visible = True
                                                btnASNCancel.Enabled = True
                                                btnASNCancel.Visible = True
                                                lblPOASNSubmitErrorInfo.Text = "FAILURE: " & strMessageText
                                                Me.btnASNSubmit.Visible = False
                                                'If Not pnlNavPanel Is Nothing Then
                                                'pnlNavPanel.Enabled = True
                                                'pnlNavPanel.Visible = True
                                                'End If
                                                clsSDIAudit.AddRecord("ASNSub.vb", "ButtonSubmitClickEvent", "ISA_RECEIPT WEBSRVC", strUserId, Session("PODetailViewBU"), "POID=" & txtPO.Text, , strMessageText, , "Failure")

                                                Exit Sub
                                            End If

                                            'clsSDIAudit.AddRecord("ASNSub.aspx.vb", "ButtonSubmitClickEvent", "ISA_RECEIPT WEBSRVC", strUserId, Session("PoSiteBu").ToString, strNextReceiver.ToString)
                                            clsSDIAudit.AddRecord("ASNSub.vb", "ButtonSubmitClickEvent", "ISA_RECEIPT WEBSRVC", strUserId, Session("PODetailViewBU"), "POID=" & txtPO.Text, , strMessageText, , strNextReceiver)
                                            ReceivingWebServiceAlreadyRun = True
                                        End If

                                    Catch ex As Exception
                                        trnsactSession.Rollback()
                                        connection.Close()
                                        btnASNSubmit.Enabled = False
                                        btnASNSubmit.Visible = False
                                        btnASNCancel.Enabled = True
                                        btnASNCancel.Visible = True
                                        lblPOASNSubmitErrorInfo.Text = " Error setting up Receiver Record - General Web Service failure - Please contact Help Desk. "
                                        lblPOASNSubmitErrorInfo.Visible = True
                                        Session("SUBMIT21Vita") = Nothing
                                        'If Not pnlNavPanel Is Nothing Then
                                        '    pnlNavPanel.Enabled = True
                                        '    pnlNavPanel.Visible = True
                                        'End If
                                        Dim strMsg1 As String = "Subroutine 'ButtonSubmitClickEvent', class 'ASNSub.aspx.vb' (Web Service call). " & vbCrLf &
                                                    "User ID: = " & strUserId & " ;BUSINESS UNIT = " & Session("PODetailViewBU") & vbCrLf &
                                                    ";PO_ID = " & txtPO.Text & vbCrLf & " ;Error Message: " & ex.Message & vbCrLf & ";server=" & Session("WEBSITEID")
                                        SendSDiExchErrorMail(strMsg1, "Error in  'ASNSub.aspx.vb' (Web Service call). ")
                                    End Try
                                    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''


                                    intLines = intLines + 1
                                    Dim truth As Boolean = True

                                    ' get Ship To ID for the current line
                                    Dim strShiptoIdLine As String = ""
                                    Try
                                        strShiptoIdLine = item("SHIPTO_ID").Text
                                    Catch ex As Exception
                                        strShiptoIdLine = txtShiptoID.Text
                                    End Try
                                    arrParamdtgLine = Insiteonline.clsReceiver.initializeParamsArrRadGrid(item, Session("PODetailViewPO"), Session("VENDOR_VP"), strShiptoIdLine, strUserId, strTariffCode, strItemWeight)
                                    'arrParamdtgLine = initializeParamsArrRadGrid(item, Session("PONUM1"), Session("VENDOR"), txtShiptoID.Text, strUserId, strTariffCode, strItemWeight)

                                    Dim strMy21Error As String = " "
                                    Try

                                        truth = Insiteonline.clsReceiver.createReceiver92(I, strNextReceiver, intLines, arrParamdtgLine, txtERSAction.Text, "R", trnsactSession, connection, " ", strMy21Error)
                                        'truth = createReceiver(I, strNextReceiver, intLines, arrParamdtgLine, txtERSAction.Text, "R", trnsactSession, connection)

                                        If Not truth Then
                                            lblPOASNSubmitErrorInfo.Text = " Error setting up Receiver Record - Please contact Help Desk. Tech. info: " & strMy21Error
                                            lblPOASNSubmitErrorInfo.Visible = True
                                            trnsactSession.Rollback()
                                            btnASNSubmit.Visible = False
                                            trnsactSession = Nothing
                                            connection.Close()
                                            ' and rollback here
                                            Exit Sub
                                        End If
                                    Catch ex As Exception
                                        lblPOASNSubmitErrorInfo.Text = " Error setting up Receiver Record - Please contact Help Desk. "
                                        lblPOASNSubmitErrorInfo.Visible = True

                                        Dim sMyErrorString21 As String = "Subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf &
                                            "User ID: = " & Session("OPERID") & " ;BUSINESS_UNIT_PO = " & Session("PODetailViewBU") & " ;SHIPTO_ID = " & arrParamdtgLine(15) & vbCrLf &
                                            " ;Vendor ID = " & Session("VENDOR_VP") & " ;PO_ID = " & Session("PODetailViewPO") & " ;LINE_NBR = " & intLines.ToString() & vbCrLf & " Error Message = " & ex.Message
                                        SendSDiExchErrorMail(sMyErrorString21, "Error in Subroutine 'createReceiver', class 'clsReceiver.vb'.")
                                        trnsactSession.Rollback()
                                        btnASNSubmit.Visible = False
                                        trnsactSession = Nothing
                                        connection.Close()
                                        ' and rollback here
                                        Exit Sub
                                    End Try
                                End If
                            End If
                        End If

                    End If
                Next
                Dim strPO As String = ""
                strPO = txtPO_Single.Text
                buildASNConfirmation(strPO)

                ''Proforma starts here

                Dim strProFormaFlag = getProformaFlag()
                Dim ListItem As ArrayList = New ArrayList()
                Dim dicPOLineQty As New Dictionary(Of String, String)

                Dim strsql As String = ""
                Dim strSelectSeq As String = ""
                If strProFormaFlag.Trim = "Y" Then

                    '' New log manitenance for generate the proforma invoice number 

                    strSelectSeq = "SELECT SDIX_PROFORMA_SEQ.NEXTVAL FROM DUAL"

                    Dim strInvoiceID As String = ORDBData.GetScalar(strSelectSeq)

                    Dim GetBU_POID As String = "SELECT ISA_BUSINESS_UNIT FROM PS_ISA_ENTERPRISE WHERE DEPTID = (SELECT DEPTID FROM PS_PO_LINE_DISTRIB WHERE PO_ID='" + txtPO.Text + "' AND ROWNUM = 1)"

                    Dim strGetPOBU As String = ORDBData.GetScalar(GetBU_POID)

                    strsql = "INSERT INTO SDIX_PROFORMA_LOG VALUES('" + strInvoiceID + "',' ','" + txtPO.Text + "','" + Session("VENDOR_VP") + "',SYSDATE, '" + strGetPOBU + "')"

                    Dim Command = New OleDbCommand(strsql, connection)

                    Command.transaction = trnsactSession
                    Dim rowsaffected As Integer = Command.ExecuteNonQuery()
                    If rowsaffected = 0 Then
                        'error - roll back and exit Sub
                        trnsactSession.Rollback()
                        connection.Close()
                        lblDBError.Text = "Error Inserting into SDIX_PROFORMA_LOG"
                        Session("SUBMIT21Vita") = Nothing
                        btnCancel.Enabled = True
                        btnCancel.Visible = True
                        btnPrint.Visible = False
                        Exit Sub
                    Else
                        '' New proforma log child table to maintain the line items QTY and Line nuumber

                        strsql = "INSERT ALL "
                        For Each Items As Telerik.Web.UI.GridDataItem In dtgPOASN.Items

                            txtQTY = CType(Items.FindControl("txtQTY"), TextBox)
                            If Not txtQTY Is Nothing And Not txtQTY.Text.Trim = "" And Items.Display Then

                                strSelectSeq = "SELECT SDIX_PROFORMA_LOG_CHI_SEQ.NEXTVAL FROM DUAL"

                                Dim strInvoiceLine As String = ORDBData.GetScalar(strSelectSeq)

                                strsql += "INTO SDIX_PROFORMA_LOG_CHI VALUES('" + strInvoiceLine + "','" + strInvoiceID + "','" + Items("LINE_NBR").Text + "','" + txtQTY.Text.Trim.ToString + "','" + Session("VENDOR_VP") + "',SYSDATE)"

                            End If
                        Next

                        strsql += "SELECT 1 FROM DUAL "


                        Command = New OleDbCommand(strsql, connection)

                        Command.transaction = trnsactSession

                        rowsaffected = Command.ExecuteNonQuery()

                        If rowsaffected = 0 Then
                            'error - roll back and exit Sub
                            trnsactSession.Rollback()
                            connection.Close()
                            lblDBError.Text = "Error Inserting into SDIX_PROFORMA_LOG_CHI"
                            Session("SUBMIT21Vita") = Nothing
                            btnCancel.Enabled = True
                            btnCancel.Visible = True
                            btnPrint.Visible = False
                            Exit Sub
                        Else
                            Session("ASNReceiverID") = strNextReceiver
                            Session("ASNInvoiceID") = strInvoiceID
                            If strProFormaFlag = "Y" Then
                                btnPrint.Visible = True
                            Else
                                btnPrint.Visible = False
                            End If
                            clsSDIAudit.AddRecord("RCVSub.vb", "ButtonSubmitClickEvent", "SDIX_PROFORMA_LOG", Session("VENDOR_VP"), Session("PoSiteBu"), strNextReceiver, "NEW LINE")
                        End If

                    End If
                End If

                btnASNSubmit.Visible = False
                'close connection/commit
                Try
                    trnsactSession.Commit()
                    trnsactSession = Nothing
                    connection.Close()

                    If strASNSite <> "Y" Then
                        lblPOASNSubmitErrorInfo.Text = "Information has been saved successfully." 'display result
                        lblPOASNSubmitErrorInfo.Visible = True
                        PopulateDropDownList()
                        LoadGrid(txtPO.Text, Session("VENDOR_VP"), Session("PODetailViewBU"))
                    Else
                        lblPOASNSubmitErrorInfo.Text = "Information has been saved - receiver ID is " & strNextReceiver 'display result
                        lblPOASNSubmitErrorInfo.Visible = True
                        PopulateDropDownList()
                        LoadGrid(txtPO.Text, Session("VENDOR_VP"), Session("PODetailViewBU"))
                    End If

                Catch ex As Exception
                    trnsactSession = Nothing
                    Try
                        connection.Close()
                    Catch

                    End Try
                    connection = Nothing
                End Try

            End If  '  dtgPO.Items.Count > 0 
        Catch ex As Exception
            trnsactSession.Rollback()
            btnASNSubmit.Visible = False
            trnsactSession = Nothing
            connection.Close()
            btnASNSubmit.Visible = False
            btnASNCancel.Enabled = True
            btnASNCancel.Visible = True
            lblPOASNSubmitErrorInfo.Text = " Error setting up Receiver Record - Please contact Help Desk. "
            lblPOASNSubmitErrorInfo.Visible = True

            Dim sMyErrorString21 As String = "Subroutine 'createReceiver', class 'clsReceiver.vb'. " & vbCrLf &
                "User ID: = " & Session("OPERID") & " ;BUSINESS_UNIT_PO = " & Session("PODetailViewBU") & vbCrLf &
                " ;Session(""SDIEMP"") = " & Session("SDIEMP") & "  ;Vendor ID = " & Session("VENDOR_VP") & " ;PO_ID = " & Session("PODetailViewPO") & vbCrLf & "  Error Message = " & ex.Message
            SendSDiExchErrorMail(sMyErrorString21, "Error in Subroutine 'createReceiver', class 'clsReceiver.vb'.")
        End Try
    End Sub

    ''' <summary>
    ''' To get the Proforma flag from enterprise table
    ''' </summary>
    ''' <param name="strBU"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function getProformaFlag() As String

        Dim strFlag As String = ""
        Dim strSQLQuery As String = ""
        Dim strSQLString As String = ""
        Dim strBusinessUnit As String = ""
        Try
            '' strSQLQuery = "SELECT LN.BUSINESS_UNIT_OM,LN.ORDER_NO,LD.PO_ID FROM PS_ISA_ORD_INTF_LN LN,PS_PO_LINE_DISTRIB LD WHERE LD.PO_ID='NY0T440288' AND LD.REQ_ID = LN.ORDER_NO"
            Dim strBUs As String = GetBusUnits()

            strSQLString = "SELECT DISTINCT C.ISA_BUSINESS_UNIT as SHIPTO_ID, D.DESCR, (C.ISA_BUSINESS_UNIT || ' - ' || D.DESCR) as SiteName" & vbCrLf &
                      " FROM PS_PO_HDR A, PS_PO_LINE_DISTRIB B, PS_ISA_ENTERPRISE C, PS_BUS_UNIT_TBL_FS D, PS_BUS_UNIT_TBL_IN E " & vbCrLf &
                      " WHERE A.BUSINESS_UNIT IN (" & strBUs & ")" & vbCrLf &
                      " AND A.PO_ID = '" + txtPO.Text + "'" & vbCrLf &
                      " --AND TRUNC(A.PO_DT) > TO_DATE('" & Now.AddMonths(-6).ToString("d") & "','MM-DD-YYYY')" & vbCrLf &
                      " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf &
                      " AND A.PO_ID = B.PO_ID" & vbCrLf &
                      " AND B.DEPTID = C.DEPTID " & vbCrLf &
                      " AND C.SETID = 'MAIN1'" & vbCrLf &
                      " AND D.BUSINESS_UNIT = C.ISA_BUSINESS_UNIT" & vbCrLf &
                      " ORDER BY DESCR" & vbCrLf

            strBusinessUnit = ORDBData.GetScalar(strSQLString)


            strSQLQuery = "SELECT PROFORMA_FLAG FROM PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT ='" + strBusinessUnit + "'"
            strFlag = ORDBData.GetScalar(strSQLQuery)

            Return strFlag
        Catch ex As Exception
            Return strFlag = ""
        End Try

    End Function

    Protected Sub btnhidden_Click(sender As Object, e As EventArgs)
        Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
        Dim CartConfirmRpt As CrystalDecisions.CrystalReports.Engine.ReportDocument = Nothing
        Dim strPONum As String = ""
        Dim strReceiverID As String = ""
        Dim txtQTY As TextBox
        Dim ListItem As ArrayList = New ArrayList()
        Dim dicPOLineQty As New Dictionary(Of String, String)
        Try

            Dim url As String = ""

            strPONum = txtPO.Text
            Dim strInvoiceID = Session("ASNInvoiceID")

            OpenProFormRptBinaryASN(strPONum, strInvoiceID, CartConfirmRpt, url)

            RadWindowShowPDF.NavigateUrl = "//" & currentApp.Request.ServerVariables("HTTP_HOST") & GetWebAppName1() & "/" & "SaveReport.aspx"
            RadWindowShowPDF.VisibleStatusbar = False
            RadWindowShowPDF.Height = 800
            RadWindowShowPDF.Width = 1000
            Dim script1 As String = "function f(){$find(""" + RadWindowShowPDF.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script1, True)

        Catch ex As Exception

        End Try
    End Sub

    Private Function checkUPSnumber(ByVal strTracknum As String) As Boolean

        If Len(Trim(strTracknum)) = 11 Then
            'If IsNumeric(Trim(strTracknum)) Then
            '    checkUPSnumber = False
            'Else
            '    checkUPSnumber = True
            'End If
            checkUPSnumber = False
        ElseIf Len(Trim(strTracknum)) = 12 Then
            If Trim(strTracknum).Substring(0, 1).ToUpper = "T" And
                Trim(strTracknum).Substring(11, 1) = """" Then
                'IsNumeric(Trim(strTracknum).Substring(1, 9)) Then
                checkUPSnumber = False
            Else
                checkUPSnumber = True
            End If
        ElseIf Len(Trim(strTracknum)) = 18 Then
            If strTracknum.Substring(0, 1) = "1" And
                strTracknum.Substring(1, 1).ToUpper = "Z" Then
                'IsNumeric(Trim(strTracknum).Substring(2, 16)) Then
                checkUPSnumber = False
            Else
                checkUPSnumber = True
            End If
        ElseIf Len(Trim(strTracknum)) = 9 Then
            checkUPSnumber = False
        Else
            checkUPSnumber = True
        End If

    End Function
    Private Function checkFEDEXnumber(ByVal strTracknum As String) As Boolean

        If Len(Trim(strTracknum)) = 12 Then

            checkFEDEXnumber = False
        ElseIf Len(Trim(strTracknum)) = 13 Then
            checkFEDEXnumber = False
        ElseIf Len(Trim(strTracknum)) = 14 Then
            checkFEDEXnumber = False
        ElseIf Len(Trim(strTracknum)) = 15 Then
            checkFEDEXnumber = False
        Else
            checkFEDEXnumber = True
        End If

    End Function
    Private Sub createXrefComment(ByVal I)
        Try

            Dim item As GridDataItem = dtgPOASN.Items(I)

            Dim strsql As String
            Dim rowsaffected As Integer
            Dim intPoLinenum As Integer = item("LINE_NBR").Text
            Dim intSchedNum As Integer = item("SCHED_NBR").Text
            'Dim strpoUOM As String = dtgPO.Items(I).Cells(9).Text
            Dim strProblemCode As String = "SH"
            Dim strDESCR60 As String = "Expeditor - Shipped"
            Dim stroperid As String = Session("OPERID")
            Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
            Dim strTrckNo As String = Trim(CType(txtTrckno, TextBox).Text)
            Dim strSpace As String = " "
            strDESCR60 = strDESCR60 + strSpace + strTrckNo


            'strsql = "SELECT A.PO_ID " & vbCrLf & _
            '        " FROM PS_ISA_XPD_COMMENT A" & vbCrLf & _
            '        " WHERE A.BUSINESS_UNIT = '" & Session("SITEBU") & "'" & vbCrLf & _
            '        " AND A.PO_ID = '" & txtPO.Text & "'" & vbCrLf & _
            '        " AND A.LINE_NBR = " & intPoLinenum & vbCrLf & _
            '        " AND A.SCHED_NBR = " & intSchedNum & vbCrLf


            'Dim objReader As OleDbDataReader = ORDBData.GetReader(strsql)
            'If Not objReader.Read() Then

            strsql = "INSERT INTO PS_ISA_XPD_COMMENT" & vbCrLf &
                " (BUSINESS_UNIT, PO_ID," & vbCrLf &
                " LINE_NBR, SCHED_NBR, " & vbCrLf &
                " ISA_PROBLEM_CODE, NOTES_1000," & vbCrLf &
                " OPRID," & vbCrLf &
                " DTTM_STAMP ) " & vbCrLf &
                " VALUES ('" & Session("PODetailViewBU") & "', '" & txtPO.Text & "'," & vbCrLf &
                " " & intPoLinenum & ", " & intSchedNum & "," & vbCrLf &
                " '" & strProblemCode & "'," & vbCrLf &
                " '" & strDESCR60 & "'," & vbCrLf &
                " '" & stroperid.ToUpper & "'" & vbCrLf &
                ",SYSDATE )" & vbCrLf

            'End If

            rowsaffected = ExecNonQuery(strsql)
            If rowsaffected = 0 Then
                lblDBError.Text = "PS_ISA_XPD_COMMENT error "
                Exit Sub
            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Function getASNFlag(ByVal strpo) As String

        Dim strSQLstring As String = ""
        Dim teststr As String = Session("PODetailViewBU")
        strSQLstring = "SELECT B.ISA_ASN_RECEIPT As ISA_ASN_SITE" & vbCrLf &
                        " FROM PS_PO_LINE_SHIP A," & vbCrLf &
                        " PS_ISA_ENTERPRISE B," & vbCrLf &
                        " PS_PO_LINE_DISTRIB D " & vbCrLf &
                        " WHERE A.BUSINESS_UNIT = '" & Session("PODetailViewBU") & "'" & vbCrLf &
                        " AND A.PO_ID = '" & strpo & "'" & vbCrLf &
                        " AND A.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf &
                        " AND D.PO_ID = A.PO_ID" & vbCrLf &
                        " AND A.SHIPTO_SETID = B.SETID" & vbCrLf &
                        " AND D.DEPTID = B.DEPTID"

        getASNFlag = ORDBData.GetScalar(strSQLstring)

    End Function

    Private Sub createASNShipped(ByVal I)
        Try

            Dim item As GridDataItem = dtgPOASN.Items(I)
            Dim txtQTY As TextBox = CType(item.FindControl("txtQTY"), TextBox)
            'Dim txtShipDate As TextBox = CType(item.FindControl("txtShipDate"), TextBox)
            Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)
            Dim cmbShipvia As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
            Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
            Dim txtCommon As TextBox = CType(item.FindControl("txtCommon"), TextBox)

            Dim strsql As String = ""
            Dim rowsaffected As Integer
            Dim intPoLinenum As Integer = item("LINE_NBR").Text
            Dim intSchedNum As Integer = item("SCHED_NBR").Text
            Dim strpoUOM As String = item("UNIT_OF_MEASURE").Text
            Dim decQty As Decimal = Convert.ToDecimal(CType(txtQTY, TextBox).Text)
            Dim strTrckNo As String = Trim(CType(txtTrckno, TextBox).Text.ToUpper)
            Dim strShipvia As String = CType(cmbShipvia, DropDownList).SelectedValue
            'Dim strShpDt As Date = CType(txtShipDate, TextBox).Text
            'Dim strShpDttx As String = CType(txtShipDate, TextBox).Text
            Dim strShpDt As Date = CType(dpDeliveredDate, RadDatePicker).SelectedDate
            Dim strShpDttx As String = CType(dpDeliveredDate, RadDatePicker).SelectedDate
            If Not CType(txtCommon, TextBox).Text = " " And
                Not CType(txtCommon, TextBox).Text = "" Then
                strShipvia = strShipvia & "#" & Replace(CType(txtCommon, TextBox).Text.ToUpper, "'", "''")
            End If
            If strTrckNo = "" Then
                strTrckNo = " "
            End If

            strsql = "SELECT A.PO_ID, A.ISA_SHIP_ID" & vbCrLf &
                    " FROM PS_ISA_ASN_SHIPPED A" & vbCrLf &
                    " WHERE A.BUSINESS_UNIT = '" & Session("PODetailViewBU") & "'" & vbCrLf &
                    " AND A.PO_ID = '" & txtPO.Text & "'" & vbCrLf &
                    " AND A.LINE_NBR = " & intPoLinenum & vbCrLf &
                    " AND A.SCHED_NBR = " & intSchedNum & vbCrLf &
                    " AND A.QTY_LN_ACCPT_VUOM = 0" & vbCrLf

            Dim objReader As OleDbDataReader = ORDBData.GetReader(strsql)
            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
            Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
            If m_weblogstring = "true" Then
                'Insiteonlinevendor.WebPartnerFunctions.WebPSharedFunc.WebLogOpenConn()
            End If
            If objReader.Read() Then
                strsql = "UPDATE PS_ISA_ASN_SHIPPED" & vbCrLf &
                        " SET QTY_LN_ACCPT_VUOM = " & decQty & "," & vbCrLf &
                        " ISA_ASN_TRACK_NO = '" & strTrckNo.ToUpper & "'," & vbCrLf &
                        " ISA_ASN_SHIP_VIA = '" & strShipvia & "'," & vbCrLf &
                        " ISA_ASN_SHIP_DT = TO_DATE('" & strShpDt & "','MM/DD/YYYY')," & vbCrLf &
                        " OPRID = '" & Session("OPERID") & "'," & vbCrLf &
                        " OPRID_MODIFIED_BY = 'ASNOPER'," & vbCrLf &
                        " DATETIME_ADDED = TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'), PROCESS_FLAG = 'N'" & vbCrLf &
                        " WHERE BUSINESS_UNIT = '" & Session("PODetailViewBU") & "'" & vbCrLf &
                        " AND PO_ID = '" & txtPO.Text & "'" & vbCrLf &
                        " AND LINE_NBR = " & intPoLinenum & vbCrLf &
                        " AND SCHED_NBR = " & intSchedNum & vbCrLf &
                        " AND QTY_LN_ACCPT_VUOM = 0" & vbCrLf
            Else
                strsql = "INSERT INTO PS_ISA_ASN_SHIPPED" & vbCrLf &
                    " (BUSINESS_UNIT, PO_ID," & vbCrLf &
                    " LINE_NBR, SCHED_NBR, " & vbCrLf &
                    " ISA_SHIP_ID, QTY_LN_ACCPT_VUOM," & vbCrLf &
                    " UNIT_OF_MEASURE," & vbCrLf &
                    " ISA_ASN_TRACK_NO," & vbCrLf &
                    " ISA_ASN_SHIP_VIA," & vbCrLf &
                    " ISA_ASN_SHIP_DT," & vbCrLf &
                    " OPRID, OPRID_MODIFIED_BY," & vbCrLf &
                    " DATETIME_ADDED, PROCESS_FLAG)" & vbCrLf &
                    " VALUES ('" & Session("PODetailViewBU") & "', '" & txtPO.Text & "'," & vbCrLf &
                    " " & intPoLinenum & ", " & intSchedNum & "," & vbCrLf &
                    " SEQ_ISA_SHIP_ID_PK.NEXTVAL," & decQty & "," & vbCrLf &
                    " '" & strpoUOM & "'," & vbCrLf &
                    " '" & strTrckNo.ToUpper & "'," & vbCrLf &
                    " '" & strShipvia & "'," & vbCrLf &
                    " TO_DATE('" & strShpDt & "','MM/DD/YYYY')," & vbCrLf &
                    "'" & Session("OPERID") & "','ASNOPER'," & vbCrLf &
                    " TO_DATE('" & dteNows & "', 'YYYY-MM-DD-HH24.MI.SS'),'N')"
            End If

            rowsaffected = ExecNonQuery(strsql)
            If rowsaffected = 0 Then
                objReader.Close()
                lblDBError.Text += "  Error Updating PS_ISA_ASN_SHIPPED"
                Exit Sub
            End If
            objReader.Close()

        Catch ex As Exception

        End Try
    End Sub

    Private Function ServiceChannelUpdate()
        Dim I As Integer
        If Session("PODetailViewBU") = "WAL00" Then
            Try
                Dim strSQLstrings As String = "select LN.*,LN.ISA_INTFC_LN, LD.PO_ID ,LN.ISA_WORK_ORDER_NO, US.THIRDPARTY_COMP_ID from PS_PO_LINE_DISTRIB LD, ps_isa_ord_intf_LN LN, SDIX_USERS_TBL US" & vbCrLf &
                    " where LD.Req_id = LN.order_no AND LD.REQ_LINE_NBR = LN.ISA_INTFC_LN And US.ISA_EMPLOYEE_ID= LN.OPRID_ENTERED_BY" & vbCrLf &
                    " And LN.BUSINESS_UNIT_OM = 'I0W01' and LD.PO_ID= '" & txtPO.Text & "'" & vbCrLf
                Dim dSet As New DataSet
                dSet = ORDBData.GetAdapter(strSQLstrings)
                If Not dSet Is Nothing Then
                    If dSet.Tables.Count > 0 Then
                        If dSet.Tables(0).Rows.Count > 0 Then
                            Dim OrderNo As String = String.Empty
                            Dim Work_Order As String = String.Empty
                            Dim ThirdParty_ID As String = String.Empty
                            Try
                                OrderNo = dSet.Tables(0).Rows(0).Item("order_no")
                                Work_Order = dSet.Tables(0).Rows(0).Item("ISA_WORK_ORDER_NO")
                                ThirdParty_ID = dSet.Tables(0).Rows(0).Item("THIRDPARTY_COMP_ID")
                            Catch ex As Exception
                                ThirdParty_ID = "0"
                            End Try

                            Dim sPath As String = ""
                            Try
                                Dim m_logFilePath As String = ""
                                Dim m_logLevel As System.Diagnostics.TraceLevel = Diagnostics.TraceLevel.Off
                                sPath = System.Web.Configuration.WebConfigurationManager.AppSettings(name:="ServiceChannelUpdate").Trim
                                While (sPath.LastIndexOf("\"c) = (sPath.Length - 1))
                                    sPath = sPath.TrimEnd("\"c)
                                End While
                                m_logFilePath = sPath & "\" & Now.Year.ToString("0000") & Now.Month.ToString("00") & Now.Day.ToString("00") & "ServiceChannelUpdate.log"
                                m_logLevel = Diagnostics.TraceLevel.Verbose
                                m_logger = New ApplicationLogger(m_logFilePath, m_logLevel)
                            Catch ex As Exception

                            End Try
                            m_logger.WriteInformationLog("Vendor -" + Session("VENDOR_VP") + ", PO ID -" + txtPO.Text)
                            m_logger.WriteInformationLog("Order No -" + OrderNo + ", Work Order -" + Work_Order + ", Third Party ID -" + ThirdParty_ID)
                            If Not String.IsNullOrEmpty(Work_Order) Then
                                Dim apiResponse As String = GetWorkOrderParts(Work_Order, ThirdParty_ID)
                                If Not String.IsNullOrEmpty(apiResponse) Then
                                    Dim objWorkOrder As List(Of WorkOrderParts) = JsonConvert.DeserializeObject(Of List(Of WorkOrderParts))(apiResponse)
                                    Dim deletearrayOfID As New List(Of Int32)
                                    Dim deletearrayOfDesc As New List(Of String)
                                    For I = 0 To dtgPO.Items.Count - 1
                                        Dim item As GridDataItem = dtgPO.Items.Item(I)
                                        If CType(item.FindControl("chkConfirm"), CheckBox).Checked = True Then
                                            Dim Desc As String = CType(item.FindControl("txtDescr254Mixed"), TextBox).Text
                                            For Each objWorkOrders As WorkOrderParts In objWorkOrder
                                                If objWorkOrders.Description.ToLower().Contains(Desc.Trim().ToLower()) And objWorkOrders.SupplierPartId.Contains(OrderNo) Then
                                                    deletearrayOfID.Add(objWorkOrders.id)
                                                    deletearrayOfDesc.Add(objWorkOrders.Description)
                                                End If
                                            Next
                                        End If
                                    Next
                                    m_logger.WriteInformationLog("Delete Count -" + deletearrayOfID.Count.ToString())
                                    If deletearrayOfID.Count > 0 Then
                                        Dim deleteResponse As Boolean = DeleteWorkOrders(Work_Order, deletearrayOfID.ToArray(), ThirdParty_ID)
                                        If deleteResponse = True Then
                                            Dim dsCart As New DataTable
                                            Dim dr As DataRow

                                            dsCart.Columns.Add("ItemDescription")
                                            dsCart.Columns.Add("Quantity")
                                            dsCart.Columns.Add("Price")
                                            dsCart.Columns.Add("UDueDate")
                                            dsCart.Columns.Add("OrderNo")

                                            For I = 0 To dtgPO.Items.Count - 1
                                                Dim item As GridDataItem = dtgPO.Items.Item(I)
                                                If CType(item.FindControl("chkConfirm"), CheckBox).Checked = True Then
                                                    Dim Desc As String = CType(item.FindControl("txtDescr254Mixed"), TextBox).Text
                                                    For Each objdeletearrayOfDesc As Object In deletearrayOfDesc
                                                        If objdeletearrayOfDesc.Contains(Desc) Then
                                                            dr = dsCart.NewRow()
                                                            dr.Item(0) = CType(item.FindControl("txtDescr254Mixed"), TextBox).Text

                                                            If CType(item.FindControl("txtqty"), TextBox).Text = "" Then
                                                                dr.Item(1) = 0
                                                            Else
                                                                dr.Item(1) = Convert.ToDecimal(CType(item.FindControl("txtqty"), TextBox).Text)
                                                            End If
                                                            If CType(item.FindControl("decPrice"), TextBox).Text = "" Then
                                                                dr.Item(2) = 0
                                                            Else
                                                                dr.Item(2) = Convert.ToDecimal(CType(item.FindControl("decPrice"), TextBox).Text)
                                                            End If

                                                            dr.Item(3) = CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate
                                                            dr.Item(4) = OrderNo + ":" + item("LINE_NBR").Text
                                                            dsCart.Rows.Add(dr)
                                                        End If
                                                    Next

                                                End If
                                            Next
                                            If dsCart.Rows.Count > 0 Then
                                                InsertWorkOrder(Work_Order, dsCart, ThirdParty_ID)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                            m_logger.WriteInformationLog("/////////////////////////////////////////////////////////////////////////////////////////////")
                        End If
                    End If
                End If
            Catch ex As Exception
            End Try
        End If
    End Function

    '' PO Due date update method
    Private Function UpdatePODueDate() As Boolean
        Try
            Dim I As Integer
            Dim strSQLstring As String

            Dim bolChanged As Boolean = False
            Dim bolUpdate As Boolean = False

            Dim dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
            Dim dteNowd As Date = Now().ToString("d")
            Dim rowsaffected As Integer

            For I = 0 To dtgPO.Items.Count - 1
                Dim item As GridDataItem = dtgPO.Items.Item(I)
                'If CType(item.FindControl("chkConfirm"), CheckBox).Checked = True Then
                '    strDueDt = CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate
                '    strSQLstring = "UPDATE PS_PO_LINE_SHIP SET DUE_DT =TO_DATE('" & strDueDt & "','MM/DD/YYYY')" & vbCrLf &
                '        " WHERE PO_ID='" + txtPO.Text + "' AND LINE_NBR = '" + item("LINE_NBR").Text + "' "
                '    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

                '    If rowsaffected = 0 Then
                '        bolUpdateFailed = True
                '    End If

                'End If
                strSQLstring = "INSERT INTO PS_ISA_PO_DUEDATE VALUES('" & txtPO.Text & "', '" & Session("VENDOR_VP") & "', SYSDATE)"
                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

                If rowsaffected = 0 Then
                    bolUpdate = True
                End If
            Next

            Return bolUpdate
        Catch ex As Exception

        End Try
    End Function

    Public Function GetWorkOrderParts(ByVal workOrder As String, Session_THIRDPARTY_COMP_ID As String) As String
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then
                Dim APIresponse = String.Empty
                If Session_THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                    APIresponse = AuthenticateService("CBRE")
                Else
                    APIresponse = AuthenticateService("Walmart")
                End If
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
                                m_logger.WriteInformationLog("Method: GetWorkOrderParts- Success")
                                Return workorderAPIResponse
                            Else
                                m_logger.WriteInformationLog("Method: GetWorkOrderParts-" + workorderAPIResponse.ToString())
                                Return String.Empty
                            End If
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                            m_logger.WriteInformationLog("Method: GetWorkOrderParts- Failed")
                            Return String.Empty
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            m_logger.WriteInformationLog("Method: GetWorkOrderParts-" + ex.Message.ToString())
            Return String.Empty
        End Try
    End Function

    Public Sub InsertWorkOrder(workOrder As String, cartDt As DataTable, Session_THIRDPARTY_COMP_ID As String)
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) And cartDt.Rows.Count > 0 Then
                Dim APIresponse = String.Empty
                If Session_THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                    APIresponse = AuthenticateService("CBRE")
                Else
                    APIresponse = AuthenticateService("Walmart")
                End If
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/inventory/parts/bulkPartUsage"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim objInserWorOrdeParts As New InsertWorkOrderPartsBO
                        objInserWorOrdeParts.AddItems = New List(Of AddItem)
                        For Each item As DataRow In cartDt.Rows
                            Dim dueDate As String = String.Empty
                            Try
                                If Not String.IsNullOrEmpty(item("UDueDate").ToString()) Then
                                    dueDate = item("UDueDate").ToString().Split(" ")(0)
                                End If
                            Catch ddEx As Exception
                                dueDate = String.Empty
                            End Try

                            objInserWorOrdeParts.AddItems.Add(New AddItem() With {
                                .RecId = workOrder,
                             .Description = item("ItemDescription") + " " + dueDate,
                             .Quantity = item("Quantity"),
                             .UnitCost = item("Price"),
                             .PartNumber = item("OrderNo")
                            })
                        Next
                        Dim serializedparameter = JsonConvert.SerializeObject(objInserWorOrdeParts)
                        Dim response = httpClient.PostAsync(apiURL, New StringContent(serializedparameter, Encoding.UTF8, "application/json")).Result()
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            m_logger.WriteInformationLog("Method: InsertWorkOrder- Success")
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            m_logger.WriteInformationLog("Method: InsertWorkOrder- " + workorderAPIResponse.ToString())

                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            m_logger.WriteInformationLog("Method: InsertWorkOrder- " + ex.Message.ToString())
        End Try
    End Sub
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
            m_logger.WriteInformationLog("Method: AuthenticateService- " + ex.Message.ToString())
        End Try
        Return "Server Error"
    End Function

    Public Function DeleteWorkOrders(workOrder As String, ByVal objPartParam As Integer(), Session_THIRDPARTY_COMP_ID As String) As Boolean
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then
                Dim APIresponse = String.Empty
                If Session_THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                    APIresponse = AuthenticateService("CBRE")
                Else
                    APIresponse = AuthenticateService("Walmart")
                End If
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/inventory/parts/bulkPartUsage"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim objDelete As New Delete
                        objDelete.DeleteItems = New List(Of DeleteItem)
                        For Each items As Integer In objPartParam
                            objDelete.DeleteItems.Add(New DeleteItem() With {
                                .PartId = items
                                })
                        Next
                        Dim serializedparameter = JsonConvert.SerializeObject(objDelete)
                        Dim response = httpClient.PostAsync(apiURL, New StringContent(serializedparameter, Encoding.UTF8, "application/json")).Result()
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            m_logger.WriteInformationLog("Method: DeleteWorkOrders- Success")
                            Return True
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            m_logger.WriteInformationLog("Method: DeleteWorkOrders-" + workorderAPIResponse.ToString())
                            Return False
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            m_logger.WriteInformationLog("Method: DeleteWorkOrders-" + ex.Message.ToString())
            Return False
        End Try
    End Function

    '' Voucher methods starts here

    Private Sub clearVocherSessions()
        mySessionState.pathSQLs = Nothing
        mySessionState.Vendor = Nothing
        mySessionState.SearchPageSelectedPO = Nothing
        mySessionState.VoucherInfo = Nothing
    End Sub

    Private Sub getPOVoucherDetails(strPoNum As String, m_sVendor As String, strPOBU As String)

        m_pathSQLs = System.IO.Path.GetDirectoryName(path:=Request.ServerVariables("PATH_TRANSLATED")) &
                                    "\InsiteOnlineVoucherEntry\SQLs\"
        Dim isPOExist As Boolean = False
        Try
            clearVocherSessions()

            mySessionState.pathSQLs = m_pathSQLs

            mySessionState.Vendor = myCommon.GetVendor(m_sVendor, m_pathSQLs, ORDBData.DbUrl)

            isPOExist = LoadVendorPOList(strPoNum, m_sVendor, m_pathSQLs)

            If isPOExist Then

                btnVoucSubmit.Visible = True
                divVocherHead.Visible = True
                lblVocherErrInfo.Visible = False
                divVoucAddLine.Visible = True

                m_selectedPO = mySessionState.SearchPageSelectedPO
                m_vendorInfo = mySessionState.Vendor
                m_pathSQLs = mySessionState.pathSQLs
                m_voucher = mySessionState.VoucherInfo

                If Not (mySessionState.UnitOfMeasure.Count > 0) Then
                    BuildUOMList(m_pathSQLs, ORDBData.DbUrl)
                End If
                ' check/build Vendor Additional Charges reference list
                If Not (mySessionState.AdditionalVendorCharges.Count > 0) Then
                    BuildAdditionalVendorCharges()
                End If



                ' get buyer information
                Dim buyerInfo As Buyer = myCommon.GetPOBuyerInfo(m_selectedPO,
                                                                 m_pathSQLs,
                                                                 m_oledbCNString)
                ' get ship to information
                Dim shipToInfo As ShipTo = myCommon.GetPOShipTo(m_selectedPO.Id,
                                                                m_vendorInfo,
                                                                m_pathSQLs,
                                                                m_oledbCNString)


                m_selectedPO.GetPOLines(m_vendorInfo, m_pathSQLs, ORDBData.DbUrl)

                m_selectedPO.Vendor = m_vendorInfo
                m_selectedPO.BuyerInfo = buyerInfo
                m_selectedPO.ShipToInfo = shipToInfo

                m_voucher = Voucher.CreateVoucherForPO(m_selectedPO)

                m_voucher.PO = m_selectedPO
                m_voucher.Vendor = m_vendorInfo
                m_voucher.Buyer = buyerInfo
                m_voucher.ShipTo = shipToInfo

                Session("CurrentPOTotal") = m_voucher.ComputedInvoiceAmount

                Me.txtERSAction.Text = m_selectedPO.ERSAction
                If (m_selectedPO.PODate.Length > 0) Then
                    lblPODatetxt.Text = CDate(m_selectedPO.PODate).ToString(format:="MM/dd/yyyy")
                End If
                If (m_selectedPO.VendorLocationId.Length > 0) Then
                    Me.txtvndrLoc.Text = m_selectedPO.VendorLocationId
                Else
                    Me.txtvndrLoc.Text = "1"
                End If
                mySessionState.VoucherInfo = m_voucher
                VoucherHeaderInfo()


            Else
                btnVoucSubmit.Visible = False
                divVocherHead.Visible = False
                divVoucAddLine.Visible = False
                'lblVocherErrInfo.Text = "No info when loading screen. PO #: " & strPoNum & " -- BU: " & strPOBU & " -- Supplier ID: " & m_sVendor & vbCrLf
                lblVocherErrInfo.Text = "The invoice for this PO #" & strPoNum & " was already submitted" & vbCrLf
                lblVocherErrInfo.Visible = True
            End If

        Catch ex As Exception
            btnVoucSubmit.Visible = False
            divVocherHead.Visible = False
            divVoucAddLine.Visible = False
            'lblVocherErrInfo.Text = "No info when loading screen. PO #: " & strPoNum & " -- BU: " & strPOBU & " -- Supplier ID: " & m_sVendor & vbCrLf
            lblVocherErrInfo.Text = "The invoice for this PO #" & strPoNum & " was already submitted" & vbCrLf
            lblVocherErrInfo.Visible = True
        End Try
    End Sub

    Private Function BuildUOMList(ByVal pathSQLs As String,
                                 ByVal oledbCNString As String) As ArrayList
        ' reset
        mySessionState.UnitOfMeasure.Clear()

        ' add blank entry
        mySessionState.UnitOfMeasure.Add(New UnitOfMeasure(uom:="", shortDesc:="", desc:=""))

        Dim cn As New OleDbConnection(oledbCNString)

        cn.Open()

        If cn.State = ConnectionState.Open Then
            Dim sqlBuilder As New System.Text.StringBuilder
            sqlBuilder.AppendFormat(myCommon.LoadQuery(pathSQLs & "getUOMRefList.txt"))
            Dim sqlQry As String = sqlBuilder.ToString
            sqlBuilder = Nothing

            Dim cmd As OleDbCommand = cn.CreateCommand
            cmd.CommandText = sqlQry
            cmd.CommandType = CommandType.Text

            Dim rdr As OleDbDataReader = cmd.ExecuteReader

            Dim uom As UnitOfMeasure = Nothing

            While rdr.Read

                Dim id As String = ""
                Dim shortDesc As String = ""
                Dim desc As String = ""

                If Not (rdr("UNIT_OF_MEASURE") Is System.DBNull.Value) Then
                    id = CStr(rdr("UNIT_OF_MEASURE")).Trim
                End If
                If Not (rdr("DESCR") Is System.DBNull.Value) Then
                    shortDesc = CStr(rdr("DESCR")).Trim
                End If
                If Not (rdr("DESCRSHORT") Is System.DBNull.Value) Then
                    desc = CStr(rdr("DESCRSHORT"))
                End If

                ' po line
                uom = New UnitOfMeasure(id, shortDesc, desc)

                ' add
                mySessionState.UnitOfMeasure.Add(uom)

            End While

            Try
                rdr.Close()
            Catch ex As Exception
            Finally
                rdr = Nothing
            End Try

            Try
                cmd.Dispose()
            Catch ex As Exception
            Finally
                cmd = Nothing
            End Try
        End If

        Try
            cn.Close()
            cn.Dispose()
        Catch ex As Exception
        Finally
            cn = Nothing
        End Try

        Return mySessionState.UnitOfMeasure
    End Function

    Private Function BuildAdditionalVendorCharges() As ArrayList
        ' reset
        mySessionState.AdditionalVendorCharges.Clear()

        ' add blank entry
        mySessionState.AdditionalVendorCharges.Add(New ItemCharge(id:="", shortDesc:="", desc:="", typeItem:=ItemCharge.eItemChargeType.evMisc))

        Dim pathFile As String = System.IO.Path.GetDirectoryName(path:=Request.ServerVariables("PATH_TRANSLATED")) &
                                 "\InsiteOnlineVoucherEntry\MiscChargeList.xml"

        Const chargesRoot As String = "VendorMiscellaneousCharges"
        Const chargesItem As String = "itemCharge"
        Const idItem As String = "id"
        Const idItemShortDesc As String = "shortDesc"
        Const idItemDesc As String = "desc"
        Const idMaxChargeable As String = "maxChargeable"
        Const idEDI As String = "ediCodes"
        Const idIsEnable As String = "enable"
        Const idVendorChargeRule As String = "vendorChargeRule"

        Dim miscCharge As ItemCharge = Nothing
        Dim doc As New Xml.XmlDocument
        Dim dsMaintTbl As DataSet = Nothing
        Dim drws As DataRow = Nothing
        Try

            doc.Load(pathFile)

            Dim nodeRoot As Xml.XmlNode = doc.SelectSingleNode(xpath:="/" & chargesRoot)

            dsMaintTbl = GetMaintList()

            For Each drws In dsMaintTbl.Tables(0).Rows
                Dim sId As String = ""
                Try
                    sId = Convert.ToString(drws.Item("ISA_LINE_ID"))
                Catch ex As Exception
                End Try
                Dim sShortDesc As String = ""
                Try
                    sShortDesc = Convert.ToString(drws.Item("ISA_DESC_LINE"))
                Catch ex As Exception
                End Try
                Dim sDesc As String = ""
                Try
                    sDesc = Convert.ToString(drws.Item("ISA_DESC_LINE"))
                Catch ex As Exception
                End Try
                Dim nMax As Double = CDbl("0")
                Try
                    ''nMax = Convert.ToDouble(drws.Item("ISA_TOLERANCE_PER"))
                Catch ex As Exception
                End Try
                Dim nTolerance As Double = CDbl("0")
                Try
                    nTolerance = Convert.ToDouble(drws.Item("ISA_TOLERANCE_PER"))
                Catch ex As Exception
                End Try
                Dim bIsEnable As Boolean = False
                Try
                    Dim enableboolval As String = Convert.ToString(drws.Item("ISA_ENABLE"))
                    If enableboolval = "Y" Then
                        bIsEnable = True
                    Else
                        bIsEnable = False
                    End If
                Catch ex As Exception
                End Try
                Dim ediCodes As String = ""
                Try
                    ''ediCodes = nodeChild.Attributes(name:=idEDI).Value
                Catch ex As Exception
                End Try

                If (sId.Trim.Length > 0) Then
                    If bIsEnable Then
                        ' create item
                        miscCharge = New ItemCharge(id:=sId, shortDesc:=sShortDesc, desc:=sDesc, typeItem:=ItemCharge.eItemChargeType.evMisc)
                        miscCharge.MaximumChargeableAmount = nMax
                        miscCharge.EDI_Codes = ediCodes
                        miscCharge.ToleranceValue = nTolerance

                        ' code to load MyCommon.vb properties from MiscChargesList.XML (not web.config) - VR 04/29/2016
                        Dim sIdCode As String = Trim(sId)
                        sShortDesc = UCase(Trim(sShortDesc))

                        Select Case sShortDesc

                        End Select

                        Dim vendorRuleId As String = ""
                        Dim vendorId As String = ""
                        Dim chargeAcctCode As String = ""
                        Dim vendorMaxChargeable As Double = CDbl("0")
                        Dim vendorRuleEnableFlag As Boolean = True

                        Try
                            ''vendorRuleId = vendorRule.Attributes(name:="id").Value
                        Catch ex As Exception
                        End Try
                        Try
                            ''vendorId = vendorRule.Attributes(name:="vendorId").Value
                        Catch ex As Exception
                        End Try
                        Try
                            ''chargeAcctCode = vendorRule.Attributes(name:="chargeAcctCode").Value
                        Catch ex As Exception
                        End Try
                        Try
                            ''vendorRuleEnableFlag = (vendorRule.Attributes(name:="enable").Value.ToUpper = "Y")
                        Catch ex As Exception
                        End Try
                        Try
                            ''vendorMaxChargeable = CDbl(vendorRule.Attributes(name:="maxChargeable").Value)
                        Catch ex As Exception
                        End Try
                        If vendorRuleEnableFlag Then
                            miscCharge.VendorChargeRules.Add(vendorRuleId, New vendorMiscChargeRule(id:=vendorRuleId,
                                                                                                    vendorId:=vendorId,
                                                                                                    chargeAcctCode:=chargeAcctCode,
                                                                                                    maxChargeableAmt:=vendorMaxChargeable))
                        End If
                        mySessionState.AdditionalVendorCharges.Add(miscCharge)
                    End If
                End If
            Next

        Catch ex As Exception

        End Try

        doc = Nothing

        Return mySessionState.AdditionalVendorCharges
    End Function

    Private Sub PopulateVoucherDataGrid()
        'Pure faith

        m_invoiceExtendedPrice = CDbl("0")
        m_invoiceExtendedPrice1 = CDbl("0")
        m_invoiceMiscCharges = CDbl("0")
        m_voucher = mySessionState.VoucherInfo
        ' take out the misc item in the dropdown after selected - only allow 3 misc charges.
        Dim vLine As VoucherLine = Nothing
        For nCtr As Integer = 0 To (m_voucher.Items.Count - 1)
            vLine = CType(m_voucher.Items(nCtr), VoucherLine)
            If vLine.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
                Dim bIsEditingThisLine As Boolean = False
                Dim EditItemIndex As Integer = -1
                If Me.grid_Voucher.EditItems.Count > 0 Then
                    For Each item As GridEditableItem In Me.grid_Voucher.EditItems
                        If item.IsInEditMode Or item.Edit Then
                            EditItemIndex = item.ItemIndex
                        End If
                    Next
                End If
                If EditItemIndex > -1 And
                   nCtr = EditItemIndex Then
                    bIsEditingThisLine = True
                End If
                'If Me.gridVoucher.EditItemIndex > -1 And _
                '   nCtr = Me.gridVoucher.EditItemIndex Then
                '    bIsEditingThisLine = True
                'End If
                If Not bIsEditingThisLine Then
                    For Each itm As ItemCharge In m_currentMiscChargeList
                        If vLine.Item.Id = itm.Id Or itm.Id = "" Or itm.Id.Trim = "" Then
                            m_currentMiscChargeList.Remove(itm)
                            Exit For
                        End If
                    Next
                End If

                If bIsEditingThisLine Then
                    For Each itm As ItemCharge In m_currentMiscChargeList
                        If itm.Id = "" Or itm.Id.Trim = "" Then
                            m_currentMiscChargeList.Remove(itm)
                            Exit For
                        End If
                    Next
                End If

            End If
        Next
        If Session("PODetailViewBU") = "WAL00" Then
            lblTaxAmount.Visible = True
            txtTaxAmount.Visible = True
        End If

        Me.grid_Voucher.DataSource = m_voucher.Items
        Me.grid_Voucher.DataBind()
    End Sub

    Private Sub btnVoucSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVoucSave.Click
        If Session("PODetailViewBU") = "WAL00" Then
            Save(IsSubmit:=False) 'sets process flag to Y
        Else
            Save(IsSubmit:=True) 'sets process flag to N
        End If
    End Sub

    Private Sub btnVoucSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnVoucSubmit.Click
        If Session("PODetailViewBU") = "WAL00" Then
            Save(IsSubmit:=False) 'sets process flag to Y
        Else
            Save(IsSubmit:=True) 'sets process flag to N
        End If
    End Sub

    Private Sub btnVoucCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnVoucCancel.Click
        'Dim parentURL As String = Me.m_urlParent
        'Dim params As String() = Nothing
        'Try
        '    params = mySessionState.PageReturnParams(Me.m_urlParent)
        'Catch ex As Exception
        'End Try
        'If (params.Length > 0) Then
        '    Dim p As String = ""
        '    For Each s As String In params
        '        p &= s & "&"
        '    Next
        '    If p.Length > 0 Then
        '        p = p.TrimEnd(CChar("&"))
        '    End If
        '    parentURL &= "?" & p
        'End If
        'myCommon.Redirect(url:=parentURL)
        clearVocherSessions()
        Response.Redirect("SDIvendor.aspx")
    End Sub

    Private Sub Save(Optional ByVal IsSubmit As Boolean = False)
        Try

            '' TODO:
            ''   (1) validate all lines
            ''   (2) save all lines as a single voucher

            VoucherHeaderInfo()
            If Session("InvoiceTax") > Session("InvoiceAmount") Then
                lblEntryErrMsg.Text = "Invoice tax is greater than Invoice Amount"
                lblEntryErrMsg.Visible = True
                Exit Sub
            End If
            m_voucher = mySessionState.VoucherInfo
            'Dim s_uid As String = Session("USER_ID")

            If ValidatePOVoucher(m_voucher) Then
                'dim s_uid as string = session("USER_ID")
                Dim confTicket As saveConfirmationTicket = myCommon.SavePOVoucherEntry(m_voucher,
                                                                                       m_pathSQLs,
                                                                                       m_oledbCNString,
                                                                                       Me,
                                                                                       IsSubmit)
                If confTicket.IsSaveSuccessful Then
                    'confTicket.ecQueueInstanceId
                    'Dim s As String = "CartConfirm_VP.aspx?" & CartConfirm_VP.PARAM_GOTO_URL & "=posearch.aspx" & CartConfirm_VP.PARAM_Reference_num & confTicket.ecQueueInstanceId
                    'Dim s1 As String = "CartConfirm_VP.aspx?" & CartConfirm_VP.PARAM_GOTO_URL & "=posearch.aspx&" & CartConfirm_VP.PARAM_Reference_num & confTicket.ecQueueInstanceId

                    Dim sVendorPath As String = ""  '  "~/Vendor21"
                    sVendorPath = GetNewVendorPortalPath()
                    ''Dim S2 As String = "~/Supplier'/InsiteOnlineVoucherEntry/CartConfirm_VP.aspx?doneURL=posearch.aspx&REF_URL=" + confTicket.ecQueueInstanceId + ""
                    '' Dim S1 As String = "'"+sVendorPath+'" & "/InsiteOnlineVoucherEntry/CartConfirm_VP.aspx?" & CartConfirm_VP.PARAM_GOTO_URL & "=posearch.aspx&" & CartConfirm_VP.PARAM_Reference_num & "=" & confTicket.ecQueueInstanceId"
                    Session("ISCARTPANEL") = "Y"
                    Response.Redirect("~/Supplier/InsiteOnlineVoucherEntry/CartConfirm_VP.aspx?doneURL=Supplier/SDIVendor.aspx&REF_URL=" & confTicket.ecQueueInstanceId, False)
                    Context.ApplicationInstance.CompleteRequest()
                    ' ...
                Else
                    ' ...
                End If
            End If

        Catch ex As Exception
            Me.ShowProcessErrorMsg(ex.Message)
        End Try
    End Sub

    Private Function VoucherHeaderInfo()
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
            m_voucher.InvoiceId = Me.txtInvoiceNo.Text.Trim
            ' (2) invoice date
            m_voucher.InvoiceDate = ""

            Dim strInvoiceDate As String = ""

            If Not dpInvoiceDate.SelectedDate Is Nothing Then
                strInvoiceDate = dpInvoiceDate.SelectedDate
                txtInvoiceDate1.Text = strInvoiceDate
            End If

            If Me.txtInvoiceDate1.Text.Trim.Length > 0 Then
                Dim dtEntered As Date
                Try
                    If IsDate(Me.txtInvoiceDate1.Text) Then
                        dtEntered = CDate(Me.txtInvoiceDate1.Text.Trim)
                        m_voucher.InvoiceDate = dtEntered.ToString("MM/dd/yyyy")
                    End If
                Catch ex As Exception
                End Try
            End If

            If m_voucher.InvoiceDate.Length > 0 Then
                Me.txtInvoiceDate1.Text = m_voucher.InvoiceDate
            Else
                Me.txtInvoiceDate1.Text = ""
            End If


            ' (3) entered invoice amount (if any)
            Dim enteredInvoiceAmt As Double = 0

            'pfd 02022009 Invoice amount should be 0 and user is forced to enter the Invoice amount to quarantee that it matches the total of the lines 
            ' commented the next few lines out so the Invoice amount will be 0
            If Me.txtInvoiceAmt.Text.Trim.Length > 0 Then
                Try
                    enteredInvoiceAmt = CDbl(Me.txtInvoiceAmt.Text.Trim)
                Catch ex As Exception
                End Try
            End If



            'm_selectedPO.VoucherInvoiceInfo.InvoiceAmount = enteredInvoiceAmt
            m_voucher.InvoiceAmount = enteredInvoiceAmt
            Session("InvoiceAmount") = enteredInvoiceAmt
            'Me.txtInvoiceAmt.Text = m_selectedPO.VoucherInvoiceInfo.InvoiceAmount.ToString(myCommon.FORMAT_CURRENCY_USD)


            'pfd 0 in txtInvoiceAmt.Text - 02022009
            'Me.txtInvoiceAmt.Text = m_voucher.InvoiceAmount.ToString(myCommon.FORMAT_CURRENCY_USD)

            ' set extract to Excel button
            ''Me.btnExcel.Enabled = False

            ' set Submit button
            Me.btnVoucSubmit.Enabled = False

            ' add additional client control attributes
            Me.txtInvoiceAmt.Attributes.Add("onkeypress", "CheckNumeric()")
            Me.txtInvoiceAmt.Attributes.Add(javascriptOnFocus(0), javascriptOnFocus(1))
            Me.txtInvoiceNo.Attributes.Add(javascriptOnFocus(0), javascriptOnFocus(1))

            ' postback or not, clear/hide entry error message container
            Me.lblEntryErrMsg.Text = ""
            Me.lblEntryErrMsg.Visible = False

            Me.lblMsg.Text = ""
            Me.lblMsg.Visible = False

            ' persist voucher information across postbacks
            mySessionState.VoucherInfo = m_voucher

            ' persist (update) PO information across postbacks
            mySessionState.SearchPageSelectedPO = m_selectedPO
            If Session("PODetailViewBU") = "WAL00" Then

                Dim enteredtaxAmt As Double = 0

                If Me.txtTaxAmount.Text.Trim.Length > 0 Then
                    Try
                        enteredtaxAmt = CDbl(Me.txtTaxAmount.Text.Trim)
                    Catch ex As Exception
                    End Try
                End If
                Session("InvoiceTax") = enteredtaxAmt
                m_voucher.SalesTax = enteredtaxAmt

                ' add additional client control attributes
                Me.txtTaxAmount.Attributes.Add("onkeypress", "CheckNumeric()")
                Me.txtTaxAmount.Attributes.Add(javascriptOnFocus(0), javascriptOnFocus(1))
                Me.txtTaxAmount.Attributes.Add(javascriptOnFocus(0), javascriptOnFocus(1))

            End If

            If Not Page.IsPostBack Then
                Insiteonline.WebPartnerFunctions.WebPSharedFunc.WebLogVendor()
            End If

            '' grid
            'PopulateVoucherDataGrid()
            '' set SAVE button state
            ''   button should only be enabled when NO LINE is in edit mode
            'Me.btnVoucSave.Enabled = (Me.gridVoucher.EditItemIndex = -1 And _
            '                      Me.gridVoucher.Items.Count > 0)
            '' set SUBMIT button state
            ''   button should only be enabled when NO LINE is in edit mode
            'Me.btnVoucSubmit.Enabled = (Me.gridVoucher.EditItemIndex = -1 And _
            '                        Me.gridVoucher.Items.Count > 0)
        Catch ex As Exception

        End Try
    End Function

    Private Function ValidatePOVoucher(ByVal voucherInfo As Voucher) As Boolean
        'Dim oledbCNString As String = ConfigurationSettings.AppSettings("OLEDBconString")
        Dim bIsValid As Boolean = False

        Try

            bIsValid = True

            Dim trnsactSession As OleDbTransaction = Nothing

            Dim arrErr As New ArrayList


            'Dim arrWarn As New ArrayList

            ' invoice number
            If Not (voucherInfo.InvoiceId.Trim.Length > 0) Then
                arrErr.Add("Please enter the invoice/reference number.")
                bIsValid = False
            End If

            ' invoice date
            'ppppppppfddddddddd
            If (voucherInfo.InvoiceDate.Trim.Length > 0) Then
                If Not IsDate(voucherInfo.InvoiceDate) Then
                    ' not a valid date string
                    arrErr.Add("Please enter correct date format for date of invoice.")
                    bIsValid = False
                End If
                'Invoice cannot be greater than today
                '-George Z 3.12.2012
                If IsDate(voucherInfo.InvoiceDate) Then
                    If CDate(voucherInfo.InvoiceDate) > Now.Date.AddDays(0) Then
                        ' " TO_DATE('" & strShpDt & "','MM/DD/YYYY')," & vbCrLf & _\
                        Dim ted As String = Now.ToLongDateString()
                        arrErr.Add("Invoice date cannot be greater than " & ted & ".")
                        bIsValid = False
                    End If
                End If
                'George pfd date cannot be less than today 02022009
                '  the next two date checks should reflect the business rules:
                '                    business rule - cannot have a date in the future
                '                    cannot go back more than 6 months - per Scott
                ' there is a bug - we need to compare by yyyyy-mm-dd.
                'we need to create a seession variable to know if the user is an SDI employee or a Vendor
                'right now we think thye user is always a vendor




                ' If CStr(Session("USERID_SAVED")) = "ASNVEND" Then
                'If Convert.ToDateTime(voucherInfo.InvoiceDate) < Now.Date.AddDays(-3) Then
                'voucherInfo.InvoiceDate.ToString()
                'arrErr.Add("Date cannot be less than 3 days from today.")
                'bIsValid = False
                'End If




                ' End If
                ' should not allow to back-date more than a year per Natasha
                '   there was an instance where the year was keyed in as 0209 instead of 2009 and system took it.
                '   - erwin 2010.04.15
                'Public dteNows As String = Now().ToString("yyyy-M-d-HH:mm:ss")
                If IsDate(voucherInfo.InvoiceDate) Then
                    If CDate(voucherInfo.InvoiceDate) < (Now.AddMonths(-6)) Then
                        arrErr.Add("Invoice date cannot be back dated this far.")
                        bIsValid = False
                    End If
                End If
            Else
                ' blank invoice date field
                arrErr.Add("Please enter date of invoice.")
                bIsValid = False
            End If

            ' invoice amount
            '   compare entered invoice amount with total invoice amount based on the lines
            Dim enteredInvoiceAmt As Double = (voucherInfo.InvoiceAmount)   ' poVchr.VoucherInvoiceInfo.InvoiceAmount
            Dim sumInvoiceLineAmt As Double = (voucherInfo.ComputedInvoiceAmount)   ' poVchr.CurrentPOVoucherAmount
            'Dim enteredInvoiceAmt As Double = Math.Round(voucherInfo.InvoiceAmount, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION) ' poVchr.VoucherInvoiceInfo.InvoiceAmount
            'Dim sumInvoiceLineAmt As Double = Math.Round(voucherInfo.ComputedInvoiceAmount, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION) ' poVchr.CurrentPOVoucherAmount
            'txtInvoiceAmt.Text = poVchr.VoucherInvoiceInfo.InvoiceAmount.ToString
            Dim strEnteredInvoiceAmt As String = enteredInvoiceAmt.ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
            Dim strsumInvoiceLineAmt As String = sumInvoiceLineAmt.ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)

            'Me.txtInvoiceAmt.Text = m_selectedPO.VoucherInvoiceInfo.InvoiceAmount.ToString(myCommon.FORMAT_CURRENCY_USD)

            'FORMAT_CURRENCY_USD_DISPLAY
            If Not (strEnteredInvoiceAmt = strsumInvoiceLineAmt) Then
                arrErr.Add("Entered invoice total amount differs calculated invoice total amount.")
                bIsValid = False
            End If

            'If Not (enteredInvoiceAmt = sumInvoiceLineAmt) Then
            '    arrErr.Add("Entered invoice total amount differs calculated invoice total amount.")
            '    bIsValid = False
            'End If

            ' voucher/invoice lines
            If Not (voucherInfo Is Nothing) Then
                Dim arrMsgs As New ArrayList
                Dim arrItmErr As ArrayList
                Dim arrItmWarn As ArrayList
                Dim s As String = ""
                Dim ted As Integer = 0
                For nIdx As Integer = 0 To (voucherInfo.Items.Count - 1)
                    arrItmErr = New ArrayList
                    arrItmWarn = New ArrayList
                    If Not ValidatePOVoucherLine(CType(voucherInfo.Items(nIdx), VoucherLine),
                                                 arrItmErr,
                                                 arrItmWarn,
                                                 voucherInfo) Then
                        bIsValid = False
                    End If
                    If arrItmErr.Count > 0 Or arrItmWarn.Count > 0 Then
                        s = Me.FormatLineItemMessagesAsList(nIdx + 1, arrItmErr, arrItmWarn)
                        arrMsgs.Add(s)
                    End If
                Next
                arrItmWarn = Nothing
                arrItmErr = Nothing
                ' format to HTML list
                If arrMsgs.Count > 0 Then
                    s = ""
                    s &= "Line item message(s)<UL>"
                    For Each itmMsg As String In arrMsgs
                        s &= "<LI>" & itmMsg & "</LI>"
                    Next
                    s &= "</UL>"
                    arrErr.Add(s)
                End If
                arrMsgs = Nothing
            End If

            ' show error message (if any)
            If Not bIsValid Then
                If arrErr.Count > 0 Then
                    Dim msg As String = ""
                    For Each s As String In arrErr
                        msg &= s & "~"
                    Next
                    If msg.Length > 0 Then
                        msg = msg.TrimEnd(CChar("~"))
                    End If
                    Me.ShowEntryErrorMsg(msg.Split(CChar("~")))
                End If
            End If

            arrErr = Nothing

        Catch ex As Exception

            sendErrorEmail(ex.ToString & "  Check Connection String for permission problems", "NO", Request.ServerVariables("URL"), "Error in 'VoucherEntryOld' screen, subroutine 'ValidatePOVoucher'")
        End Try
        If bIsValid Then
            ' bind/re-bind grid to data if necessary
            PopulateVoucherDataGrid()
        End If


        Return bIsValid

    End Function

    Private Function FormatLineItemMessagesAsList(ByVal itemNo As Integer,
                                                ByVal msgErr As ArrayList,
                                                ByVal msgWarn As ArrayList) As String
        Dim ret As String = ""
        Dim s As String = ""
        If msgErr.Count > 0 Or msgWarn.Count > 0 Then
            ret &= "Item " & itemNo.ToString
            ret &= "<UL>"
            ' error messages
            If msgErr.Count > 0 Then
                For Each s In msgErr
                    ret &= "<LI>" & "Correction needed:&nbsp;&nbsp;" & s & "</LI>"
                Next
            End If
            ' warning messages
            If msgErr.Count > 0 Then
                For Each s In msgWarn
                    ret &= "<LI>" & "Warning:&nbsp;&nbsp;" & s & "</LI>"
                Next
            End If
            ret &= "</UL>"
        End If
        Return ret
    End Function

    Private Sub ShowProcessErrorMsg(ByVal msg As String)
        If msg.Length > 0 Then
            Dim txt As String = "<STRONG>ERROR</STRONG>: Please contact SDI Helpdesk at (215)633-1900 extension 5911."
            txt &= "<br>" & msg
            Me.lblEntryErrMsg.Text = txt
            Me.lblEntryErrMsg.Visible = True
        End If
    End Sub

    Private Sub ShowEntryErrorMsg(ByVal msg As String())
        If msg.Length > 0 Then
            Dim txt As String = "<STRONG>Important</STRONG>: Please correct the following.<UL style=""padding-top: 0.5%;"">"
            For Each s As String In msg
                txt &= "<LI style=""list-style-type: square;"">" & s & "</LI>"
            Next
            txt &= "</UL>"
            Me.lblEntryErrMsg.Text = txt
            Me.lblEntryErrMsg.Visible = True
        End If

    End Sub

    Private Function ValidatePOVoucherLine(ByVal voucherLn As VoucherLine,
                                        ByRef arrErr As ArrayList,
                                        ByRef arrWarn As ArrayList,
                                        ByVal vchr As Voucher) As Boolean
        Dim oledbCNString As String = ConfigurationSettings.AppSettings("OLEDBconString")
        Dim bIsValid As Boolean = False
        If (arrErr Is Nothing) Then
            arrErr = New ArrayList
        End If
        If (arrWarn Is Nothing) Then
            arrWarn = New ArrayList
        End If
        If Not (voucherLn Is Nothing) Then
            bIsValid = True

            Dim poLn As POLine = Nothing

            If Not (voucherLn.POLineItem Is Nothing) Then
                poLn = voucherLn.POLineItem
            End If

            Dim voucherInfo As Voucher = mySessionState.VoucherInfo
            Dim strInvoiceid As String = voucherInfo.InvoiceId
            Dim strVendorid As String = voucherInfo.Vendor.Id
            Dim strVendorsetid As String = voucherInfo.Vendor.SetId
            Dim strVendorBU As String = voucherInfo.BusinessUnit
            'Dim strVendorBU As String = voucherInfo.Vendor.BusinessUnit
            Dim trnsactSession As OleDbTransaction = Nothing
            Dim cn As New OleDbConnection(oledbCNString)
            Dim duplicate_Invoice As String = ""


            cn.Open()
            m_pathSQLs = mySessionState.pathSQLs
            If cn.State = ConnectionState.Open Then
                duplicate_Invoice = getInvoiceUnique(m_pathSQLs, cn, strVendorBU, strInvoiceid, strVendorid, strVendorsetid)
                If (duplicate_Invoice Is Nothing) Then
                    'duplicate_Invoice = " "
                    duplicate_Invoice = ""
                End If
                'If Not duplicate_Invoice = " " Then
                If (duplicate_Invoice.Trim.Length > 0) Then
                    arrErr.Add("Duplicate Invoice - Invoice Must Be Unique.")
                    bIsValid = False
                End If
            End If
            ' *** item being charged
            '   (1) should be a valid item from selection IF line being edited is a miscellaneous item
            If voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
                If Not (voucherLn.Item.Id.Trim.Length > 0) Then
                    arrErr.Add("Select a valid miscellaneous charge item.")
                End If
            End If
            '   (2) ONLY Kelloggs PO can be charged with "Sales Tax" misc charge
            '       - erwin 2009.05.04
            If voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then

            End If

            ' *** voucher/invoice quantity
            '   (1) should NOT be greater than PO quantity IF line being edited is PO item - NOT MISC CHARGE
            '   (2) for misc charges, quantity should NOT be zero (0)
            '       can be greater than zero (0) or negative - for credits
            If voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evPOItem Then
                If (poLn Is Nothing) Then
                    arrErr.Add("Cannot locate PO item for this invoice line. [Item: " & voucherLn.Item.Id & "; Description: " & voucherLn.Item.ShortDescription & ".")
                Else
                    If voucherLn.Quantity > poLn.Quantity Then
                        arrErr.Add("Current invoice quantity should NOT be greater than the PO quantity.")
                    End If
                End If
            End If
            If voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Or voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evPOItem Then
                'pfd -----------02022009
                If (voucherLn.Quantity = 0) Or (voucherLn.Quantity < 0) Then
                    arrErr.Add("Current invoice line quantity can NOT be less than or equal to Zero (0). This Voucher Cannot be Processed Please call the Help Desk.")
                    bIsValid = False
                End If
            End If

            If voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evPOItem Then
                ' *** UOM should NOT be blank for evPOItem
                If Not (voucherLn.UOM.Trim.Length > 0) Then
                    arrErr.Add("Select a valid unit of measure for this line.")
                End If
            End If

            ' *** unit price
            ' ...

            ' *** line amount = (qty)(unitPrice) - total price for this item
            '   (1) if FREIGHT (misc vendor charge), do not allow to go over the limit
            '       each misc charge item may have it's maximum chargeable amount ... validate against it. Zero (0) means any amount can be charged
            If voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
                ''If (voucherLn.Item.MaximumChargeableAmount > 0) And _
                ''   (voucherLn.TotalPrice > voucherLn.Item.MaximumChargeableAmount) Then
                ''    If voucherLn.TotalPrice > voucherLn.Item.MaximumChargeableAmount Then
                ''        arrErr.Add("Invoice extended price is over the allowable limit for this item.")
                ''    End If
                ''End If
                ' per Nikki, remove validation for Kelloggs' PO, so vendor can charge any amount
                '   - erwin 2009.08.17
                If vchr.BusinessUnit = "KEL00" Then
                    ' ignore, don't validate misc charge amount!
                Else
                    ' if not Kelloggs' PO, check
                    If (voucherLn.Item.MaximumChargeableAmount > 0) And
                       (voucherLn.TotalPrice > voucherLn.Item.MaximumChargeableAmount) Then
                        If voucherLn.TotalPrice > voucherLn.Item.MaximumChargeableAmount Then
                            arrErr.Add("Invoice extended price is over the allowable limit for this item.")
                        End If
                    End If
                End If
            End If
        End If
        Return bIsValid
    End Function

    Private Shared Function getInvoiceUnique(ByVal pathsqls As String,
                                            ByVal cn As OleDbConnection,
                                            ByVal b_unit As String,
                                            ByVal b_invoice As String,
                                            ByVal b_vendor_id As String,
                                            ByVal b_vendor_setid As String) As String
        ''this is executed to check for duplicate invoices on the voucher or staging tables
        'Dim sCurrentValue As String = " "
        Dim sCurrentValue As String = ""

        If cn.State = ConnectionState.Open Then
            Dim sqlBuilder As System.Text.StringBuilder = Nothing

            '//
            '// get current value
            '//
            sqlBuilder = New System.Text.StringBuilder
            sqlBuilder.AppendFormat(myCommon.LoadQuery(pathsqls & "getInvoiceUnique.txt"), b_unit, b_invoice, b_vendor_id, b_vendor_setid)
            Dim sqlQry As String = sqlBuilder.ToString
            sqlBuilder = Nothing

            Dim cmdQry As OleDbCommand = cn.CreateCommand
            cmdQry.CommandText = sqlQry
            cmdQry.CommandType = CommandType.Text

            Dim rdr As OleDbDataReader = cmdQry.ExecuteReader

            If rdr.Read Then
                If Not (rdr("INVOICE_ID") Is System.DBNull.Value) Then
                    sCurrentValue = CStr(rdr("INVOICE_ID"))
                End If
            End If

            Try
                rdr.Close()
            Catch ex As Exception
            Finally
                rdr = Nothing
            End Try

            Try
                cmdQry.Dispose()
            Catch ex As Exception
            Finally
                cmdQry = Nothing
            End Try
        End If

        Return sCurrentValue
    End Function

    Private Sub btnAddLine_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddLine.Click
        Dim arrErr As New ArrayList
        Dim i As Integer = 0
        ' can only add a new line if there's no line currently being edited
        VoucherHeaderInfo()
        m_voucher = mySessionState.VoucherInfo

        Dim EditItemIndex As Integer = -1
        If Me.grid_Voucher.EditItems.Count > 0 Then
            For Each item As GridEditableItem In Me.grid_Voucher.EditItems
                If item.IsInEditMode Or item.Edit Then
                    EditItemIndex = item.ItemIndex
                End If
            Next
        End If

        Dim voucherLn_Chk As VoucherLine = Nothing
        For Each voucherLn_Chk In m_voucher.Items
            If voucherLn_Chk.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
                If voucherLn_Chk.Item.ShortDescription = " " Then
                    If grid_Voucher.Items.Count >= m_voucher.Items.Count Then
                        grid_Voucher.Items(m_voucher.Items.Count - 1).Edit = True
                    Else
                        m_voucher.Items.Remove(voucherLn_Chk)
                        ''ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "alertMessage", "alert('Record Inserted Successfully')", true);
                        ''ScriptManager.RegisterClientScriptBlock(Page, GetType(Page), "alertMessage", "alert('Add Line should not be clicked mulitiple times')", True)
                        Exit Sub
                    End If
                End If
            End If
        Next

        If EditItemIndex = -1 Then

            Dim voucherLn As VoucherLine = m_voucher.CreateVoucherLineItem
            voucherLn.IsJustAddedLine = True

            ' charge item (misc)
            voucherLn.Item = New ItemCharge(id:="",
                                            shortDesc:="",
                                            desc:="",
                                            typeItem:=ItemCharge.eItemChargeType.evMisc)

            '    ' get the highest line# and increment by 1
            voucherLn.LineNo = (CType(m_voucher.Items(m_voucher.Items.Count - 1), VoucherLine).LineNo + 1)

            voucherLn.Quantity = 1
            voucherLn.UOM = m_uomDefaultId
            If voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then

            End If

            'If m_voucher.Items.Count > 0 Then
            '    ' get the highest line# and increment by 1
            '    voucherLn.LineNo = (CType(m_voucher.Items(m_voucher.Items.Count - 1), VoucherLine).LineNo + 1)
            'Else
            '    ' just start at 1
            '    voucherLn.LineNo = 1
            'End If

            ' add to collection
            m_voucher.Items.Add(voucherLn)

            ' set editable row of grid
            'gridVoucher.EditItemIndex = (m_voucher.Items.Count - 1)


            ' bind/re-bind grid to data if necessary
            'pds 02032009
            PopulateVoucherDataGrid()
            grid_Voucher.Items(m_voucher.Items.Count - 1).Edit = True
        End If

        'arrErr.Add("Only Three Misc lines allowed per Voucher.")
        ' show error message (if any)
        If arrErr.Count > 0 Then
            Dim msg As String = ""
            For Each s As String In arrErr
                msg &= s & "~"
            Next
            If msg.Length > 0 Then
                msg = msg.TrimEnd(CChar("~"))
            End If
            Me.ShowEntryErrorMsg(msg.Split(CChar("~")))
        End If

        arrErr = Nothing

    End Sub

    Public Sub DisplayError(ByVal msg As String, ByVal ex As System.Exception) Implements IUserInterface.DisplayError
        Dim s As String = ""
        s &= "<UL>"
        If Not (msg Is Nothing) Then
            s &= "<LI>[ Message ]<br>" & msg & "</LI>"
        End If
        If Not (ex Is Nothing) Then
            s &= "<LI>[ Extended Message ]<br>" & ex.ToString & "</LI>"
        End If
        s &= "</UL>"
        ShowProcessErrorMsg(s)
    End Sub

    Public Sub DisplayMessage(ByVal msg As String, ByVal msgLevel As System.Diagnostics.TraceLevel) Implements IUserInterface.DisplayMessage
        If msg.Length > 0 Then
            Select Case msgLevel
                Case Diagnostics.TraceLevel.Verbose, Diagnostics.TraceLevel.Info, Diagnostics.TraceLevel.Warning
                    Me.lblMsg.Text = msg
                    Me.lblMsg.Visible = True
            End Select
        End If
    End Sub

    Private Function GetMaintList() As DataSet
        Try
            Dim strMaintQuery As String = "SELECT * FROM SDIX_VOUCHER_AUX_LINE WHERE ISA_SOFT_DELETE = 0"
            Dim dsMaintData As DataSet = ORDBData.GetAdapter(strMaintQuery)
            If dsMaintData.Tables(0).Rows.Count > 0 Then
                Return dsMaintData
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Protected Sub grid_Voucher_NeedDataSource(sender As Object, e As GridNeedDataSourceEventArgs)
        Try
            'PopulateVoucherDataGrid()

        Catch ex As Exception

        End Try
    End Sub

    Protected Sub grid_Voucher_UpdateCommand(sender As Object, e As GridCommandEventArgs)
        Me.lblDBError.Text = " "
        Dim bIsExitEditMode As Boolean = True
        Dim ctrl As System.Web.UI.Control = Nothing
        VoucherHeaderInfo()
        m_voucher = mySessionState.VoucherInfo
        ' get the changed values of this row of the grid (denoted by e.Item) and apply

        'Dim poVchrLine As POVoucherLine = CType(m_selectedPO.Items(e.Item.ItemIndex), POVoucherLine)
        Dim voucherLn As VoucherLine = CType(m_voucher.Items(e.Item.ItemIndex), VoucherLine)

        ' temporary itemCharge object before we overwrite existing
        '   itemCharge that is being edited/added
        Dim itm As ItemCharge = New ItemCharge(id:="",
                                               shortDesc:="",
                                               desc:="",
                                               typeItem:=voucherLn.Item.ItemChargeType)    'should be the same type as what was being edited/added

        Dim qty As Double = CDbl("0")
        Dim price As Double = CDbl("0")
        Dim price_po As Double = CDbl("0")
        Dim uomId As String = ""

        ' line item description
        '   get selected item ONLY for miscellaneous charges
        If voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
            ctrl = e.Item.FindControl(id:="cboCurrentInvItemDescEdit")
            If TypeOf ctrl Is DropDownList Then
                Dim cbo As DropDownList = CType(ctrl, DropDownList)
                If cbo.SelectedIndex > -1 Then
                    Dim id As String = cbo.Items(cbo.SelectedIndex).Value
                    For Each x As ItemCharge In mySessionState.AdditionalVendorCharges
                        If x.Id = id Then
                            itm = myCommon.createItemChargeCopy(itmOrig:=x)
                            Exit For
                        End If
                    Next
                End If
            End If
        End If

        ' quantity
        ctrl = e.Item.FindControl(id:="txtCurrentInvQty")
        If TypeOf ctrl Is TextBox Then
            qty = CDbl(CType(ctrl, TextBox).Text.Trim)
        End If

        Dim arrErr As New ArrayList
        ctrl = e.Item.FindControl(id:="txtCurrentInvQty")
        If TypeOf ctrl Is TextBox Then
            If Not CType(ctrl, TextBox).Text > "0" Then
                arrErr.Add("Invoice Quantity must be greater than 0.")
                bIsExitEditMode = False
            End If
        End If

        ' unit price - editable 
        ctrl = e.Item.FindControl(id:="txtCurrentInvUnitPrice")
        If TypeOf ctrl Is TextBox Then
            'price = CDbl(CType(ctrl, TextBox).Text.Trim)
            Dim sPrice As String = CType(ctrl, TextBox).Text.Trim
            If IsNumeric(sPrice) Then
                price = Math.Round(CDbl(sPrice), myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION)
            End If
        End If

        If Not voucherLn.Item.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
            '
            ' if it is a miscellaneous charge we don't have a static po-price 
            ' and don't validate the price here but later on
            '
            '  po unit price  static
            price_po = voucherLn.POLineItem.Price
            ' PRICE > PRICE + (price_po - PRICE) * 0.01
            If (price > price_po + (price_po * 0.01)) Or ((price - price_po) > 1) Then
                ' need to put in contact call-center to adjust the PO - PFD 02242009
                arrErr.Add("Unit Invoice price is greater than the allowable limit for this item - Please call the Call Center at 888-435-7734  if you need to change the Price for this Item.")
                bIsExitEditMode = False
                price = price_po
            ElseIf price < price_po - (price_po * 0.05) Or (price - price_po) < -1 Then
                arrErr.Add("Unit Invoice price is less than the allowable limit for this item - Please call the Call Center at 888-435-7734 if you need to change the Price for this Item.")
                bIsExitEditMode = False
                price = price_po
            End If

        End If

        ' validate


        ' *** item being charged
        '   (1) should be a valid item from selection IF line being edited is a miscellaneous item
        'pfd check date
        'If itm.ItemChargeType = ItemCharge.eItemChargeType.evMisc And voucherLn.Item.Then Then

        If itm.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
            If Not (itm.Id.Trim.Length > 0) Then
                arrErr.Add("Select a valid miscellaneous charge item.")
                bIsExitEditMode = False
            End If
        End If

        ' *** voucher/invoice quantity
        '   (1) should NOT be greater than PO quantity IF line being edited is PO item - NOT MISC CHARGE
        If itm.ItemChargeType = ItemCharge.eItemChargeType.evPOItem Then
            If qty > voucherLn.POLineItem.Quantity Then
                arrErr.Add("Current invoice quantity should NOT be greater than PO quantity.")
                bIsExitEditMode = False
            End If
        End If
        '   (2) for misc charges, quantity should NOT be zero (0)
        '       can be greater than zero (0) or negative - for credits
        If itm.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
            If (qty = 0) Then
                arrErr.Add("Current invoice quantity should NOT be equal to Zero (0).")
                bIsExitEditMode = False
            End If
            If voucherLn.BusinessUnit <> "KEL00" And itm.ShortDescription = "Un-Planned Delivery Cost" Then
                bIsExitEditMode = False
                arrErr.Add("This is an invalid Miscellaneous Charge Code for this Site.")
            End If
        End If

        ' *** UOM should NOT be blank
        'If Not (uomId.Trim.Length > 0) Then
        '    arrErr.Add("Select a valid unit of measure for this line.")
        '    bIsExitEditMode = False
        'End If

        ' *** unit price
        ' ...

        ' *** (qty)(unitPrice) - total price for this item
        '   (1) if FREIGHT (misc vendor charge), do not allow to go over the limit
        '       each misc charge item may have it's maximum chargeable amount ... validate against it. Zero (0) means any amount can be charged

        If Session("PODetailViewBU") = "WAL00" Then
            itm.ToleranceValue = 200
        End If
        If itm.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
            Dim InvoiceAmt As Double = Session("CurrentPOTotal")
            Dim MiscItemPrice As Double = qty * price
            Dim MiscItemPercentagewithTotalAmt As Double = itm.ToleranceValue / 100 * InvoiceAmt
            Dim OverallTotal As Double = InvoiceAmt + MiscItemPercentagewithTotalAmt

            If MiscItemPrice > MiscItemPercentagewithTotalAmt Then
                arrErr.Add("Invoice extended price is over the allowable limit of " + Convert.ToString(Math.Round(MiscItemPercentagewithTotalAmt, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)) + " price, for this item.")
                bIsExitEditMode = False
            End If
        End If

        ' check/change edit mode
        If bIsExitEditMode Then
            ' reset editable row of grid to NONE
            'gridVoucher.EditItemIndex = -1
            grid_Voucher.Items(e.Item.ItemIndex).Edit = False
            ' unflag
            '   to tell that this item is not newly added
            voucherLn.IsJustAddedLine = False
        Else
            ' stay on this row
            'gridVoucher.EditItemIndex = e.Item.ItemIndex
            'grid_Voucher.Items(e.Item.ItemIndex).Edit = True
            e.Canceled = True
        End If

        ' show error message (if any)
        If arrErr.Count > 0 Then
            Dim msg As String = ""
            For Each s As String In arrErr
                msg &= s & "~"
            Next
            If msg.Length > 0 Then
                msg = msg.TrimEnd(CChar("~"))
            End If
            Me.ShowEntryErrorMsg(msg.Split(CChar("~")))
        End If

        arrErr = Nothing


        If bIsExitEditMode Then

            ' not sure what this code is doing?
            '   - erwin 2009.08.18
            'If itm.ItemChargeType = ItemCharge.eItemChargeType.evPOItem Then
            '    If voucherLn.Price = price Then
            '    End If

            'End If

            ' always update to session var
            If itm.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
                With voucherLn
                    .Item = New ItemCharge(id:=itm.Id,
                                       shortDesc:=itm.ShortDescription,
                                       desc:=itm.Description,
                                       typeItem:=itm.ItemChargeType)
                    .Quantity = qty
                    .Price = price
                    '   .UOM = uomId
                End With
            Else
                With voucherLn
                    .Item = New ItemCharge(id:=voucherLn.Item.Id,
                                       shortDesc:=voucherLn.Item.ShortDescription,
                                       desc:=voucherLn.Item.Description,
                                       typeItem:=itm.ItemChargeType)
                    .Quantity = qty
                    .Price = price
                    '   .UOM = uomId
                End With
            End If

            ' put back
            m_voucher.Items(e.Item.ItemIndex) = voucherLn


            ' here's the check for maximum 3 misc charge lines
            '   and limited to maximum of $200.00 total
            Dim nPOItemLineCount As Integer = 0
            Dim m_miscLines As Integer = 0
            Dim nIdx As Integer = -1
            Dim itm1 As VoucherLine = Nothing
            Dim itm2 As VoucherLine = Nothing
            Dim bIsContinueDelete As Boolean = True
            If m_voucher.Items.Count > 0 Then


                ' get the (1) total PO amount for this voucher - PO lines included on this voucher,
                '   (2) total count of misc charge lines, (3) total amount of charge lines
                '   also, we're going to EXCLUDE "sales tax" from the total amount being validated for misc charges - per Nikki
                '       "sales tax" limit is defined in the xml file + maximum of 10%, whichever is lower
                ' 55  - erwin 2009.05.04
                Dim nMiscChargeCtr As Integer = 0
                Dim dMiscChargeExtAmt As Decimal = 0.0
                'Dim dPOLineExtAmt As Decimal = 0.0
                Dim dSalesTaxExtAmt As Decimal = 0.0
                'Dim dMiscChargeExtAmt As Double = CDbl("0")

                Dim dPOLineExtAmt As Double = CDbl("0")
                'Dim dSalesTaxExtAmt As Double = CDbl("0")
                Dim dGSTAmt As Decimal = 0.0
                Dim dHSTAmt As Decimal = 0.0
                Dim dPSTAmt As Decimal = 0.0
                Dim dQSTAmt As Decimal = 0.0
                Dim dNDAAmt As Decimal = 0.0

                Dim dUPSBLUEAmt As Decimal = 0.0
                Dim dUPSSAMEDAYAmt As Decimal = 0.0
                Dim dUPSREDAmt As Decimal = 0.0
                Dim dFEDEXAmt As Decimal = 0.0
                Dim dSITEOPENINGAmt As Decimal = 0.0
                Dim dHANDLINGCHARGEAmt As Decimal = 0.0
                Dim dCUSTOMCHARGESAmt As Decimal = 0.0
                Dim dDUTIESAmt As Decimal = 0.0
                Dim dUnPlannDelCostAmt As Decimal = 0.0

            End If


        End If
        ' bind/re-bind grid to data if necessary

        PopulateVoucherDataGrid()

    End Sub


    Protected Sub grid_Voucher_ItemDataBound(sender As Object, e As GridItemEventArgs)
        'Me.lblDBError.Text = " "
        Dim arrErr As New ArrayList
        Dim bIsExitEditMode As Boolean = True
        ' Dim voucherLn As VoucherLine = CType(m_voucher.Items(e.Item.ItemIndex), VoucherLine)

        Dim ctrl As System.Web.UI.Control = Nothing
        If e.Item.ItemIndex > -1 Then
            Dim voucherLn As VoucherLine = CType(m_voucher.Items(e.Item.ItemIndex), VoucherLine)
            Dim poLn As POLine = Nothing

            If Not (voucherLn.POLineItem Is Nothing) Then
                poLn = voucherLn.POLineItem
            End If

            Dim ITEM As GridDataItem = TryCast(e.Item, GridDataItem)
            ' line number
            '    show po line number

            If Not (poLn Is Nothing) Then
                ITEM("Ln").Text = poLn.LineNo.ToString
            Else
                ITEM("Ln").Text = "*"
            End If
            Session("lineNumber") = ITEM("Ln").Text
            ' schedule line number
            '   show po schedule line number; voucher does not have this property

            If Not (poLn Is Nothing) Then
                ITEM("Sc").Text = poLn.ScheduleLineNo.ToString
            Else
                ITEM("Sc").Text = "*"
            End If

            Dim sdiItem As InventoryItem = Nothing
            Dim mfgItem As InventoryItem = Nothing
            Dim vndItem As InventoryItem = Nothing

            If Not (voucherLn.POLineItem Is Nothing) Then
                sdiItem = voucherLn.POLineItem.ItemInfo_SDI
                mfgItem = voucherLn.POLineItem.ItemInfo_Manufacturer
                vndItem = voucherLn.POLineItem.ItemInfo_Vendor
            End If

            Dim chargeItem As ItemCharge = voucherLn.Item

            ' sdi definition of the item
            '   show the item Id being charged

            ''e.Item.Cells(eVoucherGridColumn.evSDIItemId).Text = " "  ' default
            ITEM("evSDIItemId").Text = " "  ' default
            If Not (chargeItem Is Nothing) Then
                If (chargeItem.ItemChargeType = ItemCharge.eItemChargeType.evPOItem) Then
                    ITEM("evSDIItemId").Text = chargeItem.Id    ' override value
                End If
            End If
            ' manufacturer definition of the item
            If Not (mfgItem Is Nothing) Then
                ITEM("evMfgItemId").Text = mfgItem.Id
            Else
                ITEM("evMfgItemId").Text = " "
            End If
            ' vendor definition of the item
            If Not (vndItem Is Nothing) Then
                ITEM("evVendorItemId").Text = vndItem.Id
            Else
                ITEM("evVendorItemId").Text = " "
            End If

            ' item/line description
            'ctrl = e.Item.Cells(eVoucherGridColumn.evItemDescInvoice).FindControl(id:="lblCurrentInvItemDesc")
            ctrl = CType(ITEM.FindControl("lblCurrentInvItemDesc"), Label)
            If TypeOf ctrl Is Label Then
                CType(ctrl, Label).Text = chargeItem.ShortDescription
            End If
            'If Me.gridVoucher.EditItemIndex = e.Item.ItemIndex Then
            'If TypeOf e.Item Is GridEditableItem And e.Item.IsInEditMode Then
            Dim testitems = Me.grid_Voucher.EditItems
            Dim testitems1 = Me.grid_Voucher.EditIndexes
            If e.Item.IsInEditMode Then
                If TypeOf e.Item Is GridEditFormInsertItem OrElse TypeOf e.Item Is GridDataInsertItem Then
                    Dim lblInvoiceItem As System.Web.UI.Control = e.Item.FindControl(id:="lblCurrentInvItemDescEdit")
                    Dim cboInvoiceItem As System.Web.UI.Control = e.Item.FindControl(id:="cboCurrentInvItemDescEdit")
                    Dim test As System.Web.UI.Control = e.Item.Cells(eVoucherGridColumn.evEditUpdateCancel).FindControl(id:="EditText")


                    ' current item is editable
                    '   see if this is a regular PO item or misc charges
                    If chargeItem.ItemChargeType = ItemCharge.eItemChargeType.evPOItem Then
                        ' regular PO item, show label
                        '   and hide combo box
                        If Not (lblInvoiceItem Is Nothing) Then
                            If TypeOf lblInvoiceItem Is Label Then
                                CType(lblInvoiceItem, Label).Text = chargeItem.ShortDescription
                                CType(lblInvoiceItem, Label).Visible = True
                            End If
                        End If
                        If Not (cboInvoiceItem Is Nothing) Then
                            If TypeOf cboInvoiceItem Is DropDownList Then
                                CType(cboInvoiceItem, DropDownList).Visible = False
                            End If
                        End If
                    ElseIf chargeItem.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
                        ' misc charges, show/enable combo box
                        ' hide label control
                        ' disable the edit here Pete Doyle

                        If Not (cboInvoiceItem Is Nothing) Then
                            If TypeOf cboInvoiceItem Is DropDownList Then
                                ' bind control to array
                                Dim cbo As DropDownList = CType(cboInvoiceItem, DropDownList)
                                'cbo.DataSource = mySessionState.AdditionalVendorCharges
                                cbo.DataSource = m_currentMiscChargeList
                                cbo.DataTextField = "ShortDescription"
                                cbo.DataValueField = "Id"
                                cbo.DataBind()
                                ' default
                                ' pfd only allow three 
                                If chargeItem.Id.Trim.Length > 0 Then
                                    'For nCtr As Integer = 0 To (mySessionState.AdditionalVendorCharges.Count - 1)
                                    For nCtr As Integer = 0 To (m_currentMiscChargeList.Count - 1)
                                        'If CType(mySessionState.AdditionalVendorCharges(nCtr), ItemCharge).Id = chargeItem.Id Then
                                        If CType(m_currentMiscChargeList(nCtr), ItemCharge).Id = chargeItem.Id Then
                                            cbo.SelectedIndex = nCtr
                                            Exit For
                                        End If
                                    Next
                                End If
                                cbo.Visible = True
                            End If
                        End If
                        If Not (lblInvoiceItem Is Nothing) Then
                            If TypeOf lblInvoiceItem Is Label Then
                                CType(lblInvoiceItem, Label).Visible = False
                            End If
                        End If
                    End If
                Else
                    Dim lblInvoiceItem As System.Web.UI.Control = e.Item.FindControl(id:="lblCurrentInvItemDescEdit")
                    Dim cboInvoiceItem As System.Web.UI.Control = e.Item.FindControl(id:="cboCurrentInvItemDescEdit")
                    Dim test As System.Web.UI.Control = e.Item.Cells(eVoucherGridColumn.evEditUpdateCancel).FindControl(id:="EditText")


                    ' current item is editable
                    '   see if this is a regular PO item or misc charges
                    If chargeItem.ItemChargeType = ItemCharge.eItemChargeType.evPOItem Then
                        ' regular PO item, show label
                        '   and hide combo box
                        If Not (lblInvoiceItem Is Nothing) Then
                            If TypeOf lblInvoiceItem Is Label Then
                                CType(lblInvoiceItem, Label).Text = chargeItem.ShortDescription
                                CType(lblInvoiceItem, Label).Visible = True
                            End If
                        End If
                        If Not (cboInvoiceItem Is Nothing) Then
                            If TypeOf cboInvoiceItem Is DropDownList Then
                                CType(cboInvoiceItem, DropDownList).Visible = False
                            End If
                        End If
                    ElseIf chargeItem.ItemChargeType = ItemCharge.eItemChargeType.evMisc Then
                        ' misc charges, show/enable combo box
                        ' hide label control
                        ' disable the edit here Pete Doyle

                        If Not (cboInvoiceItem Is Nothing) Then
                            If TypeOf cboInvoiceItem Is DropDownList Then
                                ' bind control to array
                                Dim cbo As DropDownList = CType(cboInvoiceItem, DropDownList)
                                'cbo.DataSource = mySessionState.AdditionalVendorCharges
                                cbo.DataSource = m_currentMiscChargeList
                                cbo.DataTextField = "ShortDescription"
                                cbo.DataValueField = "Id"
                                cbo.DataBind()
                                ' default
                                ' pfd only allow three 
                                If chargeItem.Id.Trim.Length > 0 Then
                                    'For nCtr As Integer = 0 To (mySessionState.AdditionalVendorCharges.Count - 1)
                                    For nCtr As Integer = 0 To (m_currentMiscChargeList.Count - 1)
                                        'If CType(mySessionState.AdditionalVendorCharges(nCtr), ItemCharge).Id = chargeItem.Id Then
                                        If CType(m_currentMiscChargeList(nCtr), ItemCharge).Id = chargeItem.Id Then
                                            cbo.SelectedIndex = nCtr
                                            Exit For
                                        End If
                                    Next
                                End If
                                cbo.Visible = True
                            End If
                        End If
                        If Not (lblInvoiceItem Is Nothing) Then
                            If TypeOf lblInvoiceItem Is Label Then
                                CType(lblInvoiceItem, Label).Visible = False
                            End If
                        End If
                    End If
                End If
            End If
            If chargeItem.Id.Trim = "1" Then

                m_voucher.Freight = Math.Round(voucherLn.Price, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)

            End If
            Dim strQuery As String
            strQuery = " SELECT QTY_PO FROM SYSADM8.PS_PO_LINE_SHIP WHERE PO_ID='" + txtPO.Text + "' AND LINE_NBR='" + Session("lineNumber") + "' "
            ITEM("evQtyPO").Text = ORDBData.GetScalar(strQuery)

            ' PO quantity/unit price/extended price
            If Not (poLn Is Nothing) Then
                'ITEM("evQtyPO").Text = poLn.Quantity.ToString(myCommon.FORMAT_QUANTITY)
                'e.Item.Cells(Me.eVoucherGridColumn.evUnitPricePO).Text = poLn.Price.ToString(myCommon.FORMAT_CURRENCY_USD)
                ITEM("evUnitPricePO").Text = Math.Round(poLn.Price, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
                'e.Item.Cells(Me.eVoucherGridColumn.evExtendedPricePO).Text = poLn.TotalPrice.ToString(myCommon.FORMAT_CURRENCY_USD)
                ITEM("evExtendedPricePO").Text = Math.Round(poLn.TotalPrice, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
            Else
                ITEM("evQtyPO").Text = CDbl("0").ToString(myCommon.FORMAT_QUANTITY)
                'e.Item.Cells(Me.eVoucherGridColumn.evUnitPricePO).Text = CDbl("0").ToString(myCommon.FORMAT_CURRENCY_USD)
                ITEM("evUnitPricePO").Text = CDbl("0").ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
                'e.Item.Cells(Me.eVoucherGridColumn.evExtendedPricePO).Text = CDbl("0").ToString(myCommon.FORMAT_CURRENCY_USD)
                ITEM("evExtendedPricePO").Text = CDbl("0").ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
            End If
            ' prior invoiced quantity
            Dim priorInvQty As String
            If Session("PODetailViewBU") = "WAL00" Then
                strQuery = " SELECT SUM(QTY_VCHR) FROM SYSADM8.PS_VCHR_LINE_STG WHERE PO_ID='" + txtPO.Text + "' AND LINE_NBR='" + Session("lineNumber") + "' "
                priorInvQty = ORDBData.GetScalar(strQuery)
            Else
                strQuery = " SELECT SUM(QTY_VCHR) FROM SYSADM8.PS_apxVCHRLINE_STG WHERE PO_ID='" + txtPO.Text + "' AND LINE_NBR='" + Session("lineNumber") + "' "
                priorInvQty = ORDBData.GetScalar(strQuery)
            End If
            If Not priorInvQty.Trim = "" Then
                ITEM("evPriorInvoicedQty").Text = priorInvQty
            Else
                ITEM("evpriorinvoicedqty").Text = CDbl("0").ToString(myCommon.FORMAT_QUANTITY)
            End If

            'If Not (poLn Is Nothing) Then
            '    ITEM("evPriorInvoicedQty").Text = poLn.PriorInvoicedQuantity.ToString(myCommon.FORMAT_QUANTITY)
            'Else
            '    ITEM("evPriorInvoicedQty").Text = CDbl("0").ToString(myCommon.FORMAT_QUANTITY)
            'End If

            ' current invoice quantity
            ctrl = e.Item.FindControl(id:="lblCurrentInvQty")
            If TypeOf ctrl Is Label Then
                'ITEM("evQtyInvoice").Text = voucherLn.Quantity.ToString(myCommon.FORMAT_QUANTITY)
                CType(ctrl, Label).Text = voucherLn.Quantity.ToString(myCommon.FORMAT_QUANTITY)
            End If
            'ITEM("evQtyInvoice").Text = voucherLn.Quantity.ToString(myCommon.FORMAT_QUANTITY)
            ctrl = e.Item.FindControl(id:="txtCurrentInvQty")
            If TypeOf ctrl Is TextBox Then
                CType(ctrl, TextBox).Text = voucherLn.Quantity.ToString(myCommon.FORMAT_QUANTITY)
                'CType(ctrl, TextBox).Attributes.Add(javascriptOnFocus(0), javascriptOnFocus(1))
                CType(ctrl, TextBox).Attributes.Add("onkeypress", "CheckNumeric()")
            End If

            ' current invoice price - ehhh heeehpppfd
            ctrl = e.Item.FindControl(id:="lblCurrentInvUnitPrice")
            If TypeOf ctrl Is Label Then
                'CType(ctrl, Label).Text = (voucherLn.Price).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
                CType(ctrl, Label).Text = Math.Round(voucherLn.Price, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
            End If
            'ITEM("evUnitPriceInvoice").Text = Math.Round(voucherLn.Price, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
            ctrl = e.Item.FindControl(id:="txtCurrentInvUnitPrice")
            If TypeOf ctrl Is TextBox Then
                'CType(ctrl, TextBox).Text = (voucherLn.Price).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
                CType(ctrl, TextBox).Text = Math.Round(voucherLn.Price, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
                'CType(ctrl, TextBox).Attributes.Add(javascriptOnFocus(0), javascriptOnFocus(1))
                CType(ctrl, TextBox).Attributes.Add("onkeypress", "CheckNumeric()")
            End If

            ' unit of measure
            ctrl = e.Item.FindControl(id:="lblCurrentInvUOM")
            If TypeOf ctrl Is Label Then
                CType(ctrl, Label).Text = voucherLn.UOM
            End If
            'ITEM("evUOMInvoice").Text = voucherLn.UOM

            ' current invoice extended price
            'e.Item.Cells(Me.eVoucherGridColumn.evExtendedPriceInvoice).Text = voucherLn.TotalPrice.ToString(myCommon.FORMAT_CURRENCY_USD)
            ITEM("evExtendedPriceInvoice").Text = Math.Round(voucherLn.TotalPrice, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)

            ' edit or update/cancel
            ' ...

            ' remove
            ' ...

            ' footer
            'Select Case e.Item.ItemType
            '    Case ListItemType.AlternatingItem, _
            '         ListItemType.EditItem, _
            '         ListItemType.Item, _
            '         ListItemType.SelectedItem

            'End Select
            m_invoiceExtendedPrice += voucherLn.TotalPrice
        End If

        If m_invoiceExtendedPrice <> 0 Then
            lblTotalPrice.Visible = True
            totalPriceCal.Visible = True
            totalPriceCal.Text = Math.Round(m_invoiceExtendedPrice, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
            'txtTaxAmount_TextChanged(sender, e)


        Else
            lblTotalPrice.Visible = False
            totalPriceCal.Visible = False
        End If

        'Me.gridVoucher.Visible = False

        txtInvoiceAmt.Text = Math.Round(m_invoiceExtendedPrice, myCommon.FORMAT_CURRENCY_DECIMAL_PRECISION).ToString(myCommon.FORMAT_CURRENCY_USD_DISPLAY)
        'txtTaxAmount_TextChanged(sender, e)
        Dim str As Double = 0
        Dim actualAmount As Double = Val(txtTaxAmount.Text)
        'Dim TA As Double = Val(txtTA.Text)
        'Dim percentageAmt As Double = Val(actualAmount * TA) / 100
        str = actualAmount + Session("totalPriceCal")
        txtTotalInvoiceAmt.Text = str.ToString("0.00")
        bIsValidInv = True
        Session("totalPriceCaltext") = txtTotalInvoiceAmt.Text
        txtTotalInvoiceAmt.Text = ""
        str = Nothing
        Session("totalPriceCal") = txtInvoiceAmt.Text
        If bIsValidInv = True Then
            If Session("PODetailViewBU") = "WAL00" Then
                'Dim str As Double = 0
                'str = totalPriceCal.Text + Session("totalPriceCaltext")
                txtTotalInvoiceAmt.Text = Session("totalPriceCaltext")
                'Session.Clear()
            End If
        End If
    End Sub
    Public Sub txtTaxAmount_TextChanged(sender As Object, e As EventArgs)

        Try
            VoucherHeaderInfo()
            'totalPriceCal.Text = Session("totalPriceCal") + Session("InvoiceTax")
            'Session("totalPriceCaltext") = totalPriceCal.Text
            Dim str As Double = 0
            Dim actualAmount As Double = Val(txtTaxAmount.Text)
            'Dim TA As Double = Val(txtTA.Text)
            'Dim percentageAmt As Double = Val(actualAmount * TA) / 100
            str = actualAmount + Session("totalPriceCal")
            txtTotalInvoiceAmt.Text = str.ToString("0.00")
            bIsValidInv = True
            Session("totalPriceCaltext") = txtTotalInvoiceAmt.Text
            txtTotalInvoiceAmt.Text = ""
            str = Nothing
            'Save(IsSubmit:=True)
            'ValidatePOVoucher(m_voucher)
            lblTotalInvoiceAmt.Visible = True
            txtTotalInvoiceAmt.Visible = True
        Catch ex As Exception

        End Try
    End Sub
    Protected Sub grid_Voucher_EditCommand(sender As Object, e As GridCommandEventArgs)

        'take away edit for a misc charge pfd 02132009
        Me.lblDBError.Text = " "
        Dim voucherLn As VoucherLine = Nothing
        VoucherHeaderInfo()
        m_voucher = mySessionState.VoucherInfo
        Try
            voucherLn = CType(m_voucher.Items(e.Item.ItemIndex), VoucherLine)
        Catch ex As Exception
        End Try

        If m_voucher.Items.Count > 0 Then

            'set editable row of grid

            'gridVoucher.EditItemIndex = e.Item.ItemIndex
            lblDBError.Visible = False
            'bind/re-bind grid to data if necessary
            'Me.grid_Voucher.Items(e.Item.ItemIndex).Edit = True
            PopulateVoucherDataGrid()



        End If

        'End If
    End Sub

    Protected Sub grid_Voucher_CancelCommand(sender As Object, e As GridCommandEventArgs)
        Me.lblDBError.Text = " "
        ' set editable row of grid to NONE
        'gridVoucher.EditItemIndex = -1
        ' check/delete if "add new line" was cancelled, auto-remove

        Dim voucherLn As VoucherLine = Nothing
        VoucherHeaderInfo()
        m_voucher = mySessionState.VoucherInfo
        Try
            voucherLn = CType(m_voucher.Items(e.Item.ItemIndex), VoucherLine)
        Catch ex As Exception
        End Try
        If Not (voucherLn Is Nothing) Then
            If voucherLn.IsJustAddedLine Then
                m_voucher.Items.RemoveAt(e.Item.ItemIndex)
            End If
        End If
        ' bind/re-bind grid to data if necessary
        PopulateVoucherDataGrid()
    End Sub

    Protected Sub grid_Voucher_DeleteCommand(sender As Object, e As GridCommandEventArgs)

        'can ONLY delete line(s) if no line is being edited
        Me.lblDBError.Text = " "
        Dim arrErr As New ArrayList
        VoucherHeaderInfo()
        m_voucher = mySessionState.VoucherInfo


        'If Me.gridVoucher.EditItemIndex = -1 Then
        Dim nPOItemLineCount As Integer = 0
        Dim nIdx As Integer = -1
        Dim itm As VoucherLine = Nothing
        Dim bIsContinueDelete As Boolean = True
        If m_voucher.Items.Count > 0 Then
            For nCtr As Integer = 0 To (m_voucher.Items.Count - 1)
                itm = CType(m_voucher.Items(nCtr), VoucherLine)
                If itm.Item.ItemChargeType = ItemCharge.eItemChargeType.evPOItem Then
                    nPOItemLineCount += 1
                    nIdx = nCtr
                End If
            Next
        End If
        If nPOItemLineCount = 1 Then
            If nIdx = e.Item.ItemIndex Then
                bIsContinueDelete = False
            End If
        End If

        If bIsContinueDelete Then
            If m_voucher.Items.Count > 1 Then
                ' m_voucher.Items.

                If e.Item.ItemIndex > -1 Then
                    m_voucher.Items.RemoveAt(e.Item.ItemIndex)
                End If
                ' bind/re-bind grid to data if necessary
                PopulateVoucherDataGrid()
            Else
                arrErr.Add("There must have at least one PO line on the voucher.  You cannot remove this line.")
            End If
        Else
            arrErr.Add("There must have at least one PO line on the voucher. You cannot remove this line. ")
        End If


        ' show error message (if any)
        If arrErr.Count > 0 Then
            Dim msg As String = ""
            For Each s As String In arrErr
                msg &= s & "~"
            Next
            If msg.Length > 0 Then
                msg = msg.TrimEnd(CChar("~"))
            End If
            Me.ShowEntryErrorMsg(msg.Split(CChar("~")))
        End If

        arrErr = Nothing
    End Sub

    Protected Sub dtgPOASN_ItemDataBound1(sender As Object, e As GridItemEventArgs)
        Try
            Dim I As Decimal
            Dim P As Decimal
            If e.Item.ItemType = GridItemType.Item Or e.Item.ItemType = GridItemType.AlternatingItem Then
                Dim item As GridDataItem = TryCast(e.Item, GridDataItem)
                I = CType(item("QTY_PO").Text, Decimal)
                P = CType(item("QTY_LN_ACCPT").Text, Decimal)
                If CType(e.Item.DataItem("QTY_PO"), Decimal) > CType(e.Item.DataItem("QTY_LN_ACCPT"), Decimal) Then  '   If CType(e.Item.DataItem("QTY_PO"), Decimal) > CType(e.Item.DataItem("hQTYRCV"), Decimal) Then
                    Dim lblOpenQty As Label = CType(item.FindControl("lblOpenQTY"), Label)
                    lblOpenQty.Text = String.Format("{0:0.00}", CType(item("QTY_PO").Text, Decimal) - CType(item("QTY_LN_ACCPT").Text, Decimal))  '  m_decTotRCVQtyForOpenQty)
                Else

                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub LoadGrid(strPoNum1 As String, m_sVendor As String, strPOBU As String)
        Dim strtxtSmallSite As String = ""
        Dim strSQLstring As String = ""
        Dim decHldQtyRec As Decimal
        Dim strASNReceiving As String = ""
        Dim dsdr As DataRow
        Dim I As Integer = 0
        Dim P As Integer

        Try
            strtxtSmallSite = "Y"
            If txtasnflag.Text.Trim = "" Then
                txtasnflag.Text = getASNFlag(strPoNum1)
            End If
            strASNReceiving = txtasnflag.Text
            dsPOdata = getpoinfoASN(strPoNum1, m_sVendor, strPOBU, strtxtSmallSite, strASNReceiving)
            Dim decHldQtyAccpt As Decimal
            If dsPOdata.Tables.Count > 0 Then
                If dsPOdata.Tables(0).Rows.Count > 0 Then

                    Do While I <= dsPOdata.Tables(0).Rows.Count

                        If I = dsPOdata.Tables(0).Rows.Count Then
                            If dsPOdata.Tables(0).Rows(I - 1).Item("QTY_PO") > decHldQtyRec And
                                decHldQtyRec > 0 Then
                                getNewdatarow((I - 1), decHldQtyRec)
                            End If
                            Exit Do
                        End If

                        If strtxtSmallSite = "Y" Then
                            strSQLstring = "SELECT TO_CHAR(A.ISA_ASN_SHIP_DT,'MM/DD/YYYY') as ISA_ASN_SHIP_DT," & vbCrLf &
                            " A.ISA_ASN_TRACK_NO, A.ISA_ASN_SHIP_VIA," & vbCrLf &
                            " B.DESCR" & vbCrLf &
                            " FROM PS_ISA_ASN_SHIPPED A, PS_SHIP_METHOD B" & vbCrLf &
                            " WHERE A.ISA_SHIP_ID = '" & dsPOdata.Tables(0).Rows(I).Item("ISA_SHIP_ID") & "'" & vbCrLf &
                            " AND A.ISA_ASN_SHIP_VIA = B.SHIP_TYPE_ID(+)"
                        Else
                            strSQLstring = "SELECT TO_CHAR(A.ISA_ASN_SHIP_DT,'MM/DD/YYYY') as ISA_ASN_SHIP_DT," & vbCrLf &
                            " A.ISA_ASN_TRACK_NO, A.ISA_ASN_SHIP_VIA," & vbCrLf &
                            " B.DESCR" & vbCrLf &
                            " FROM PS_ISA_RECV_LN_ASN A, PS_SHIP_METHOD B" & vbCrLf &
                            " WHERE BUSINESS_UNIT = '" & dsPOdata.Tables(0).Rows(I).Item("BUSINESS_UNIT") & "'" & vbCrLf &
                            " AND RECEIVER_ID = '" & dsPOdata.Tables(0).Rows(I).Item("RECEIVER_ID") & "'" & vbCrLf &
                            " AND RECV_LN_NBR = '" & dsPOdata.Tables(0).Rows(I).Item("RECV_LN_NBR") & "'" & vbCrLf &
                            " AND A.ISA_ASN_SHIP_VIA = B.SHIP_TYPE_ID(+)"
                        End If

                        Dim dr As OleDbDataReader = ORDBData.GetReader(strSQLstring)
                        ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                        Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
                        If m_weblogstring = "true" Then
                            'WebLogOpenConn()
                        End If
                        If dr.Read Then
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT") = dr.Item("ISA_ASN_SHIP_DT")
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO") = Trim(dr.Item("ISA_ASN_TRACK_NO"))
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = dr.Item("DESCR")
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID") = dr.Item("ISA_ASN_SHIP_VIA")
                            'if this is the first row.. save for defaults for any row that is empty

                            'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                            If m_weblogstring = "true" Then
                                'WebLogCloseConn()
                            End If
                        Else
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_DT") = ""
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_TRACK_NO") = ""
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA") = ""
                            dsPOdata.Tables(0).Rows(I).Item("ISA_ASN_SHIP_VIA_ID") = ""
                        End If
                        dr.Close()

                        If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT")) Then
                            dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT") = "0"
                        End If
                        If I > 0 Then
                            If IsDBNull(dsPOdata.Tables(0).Rows(I - 1).Item("QTY_LN_ACCPT")) Then
                                dsPOdata.Tables(0).Rows(I - 1).Item("QTY_LN_ACCPT") = "0"
                            End If
                        End If

                        If I = 0 Then
                            decHldQtyRec = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        ElseIf dsPOdata.Tables(0).Rows(I).Item("LINE_NBR") =
                                dsPOdata.Tables(0).Rows(I - 1).Item("LINE_NBR") And
                                dsPOdata.Tables(0).Rows(I).Item("SCHED_NBR") =
                                dsPOdata.Tables(0).Rows(I - 1).Item("SCHED_NBR") Then
                            decHldQtyRec = decHldQtyRec + Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        ElseIf dsPOdata.Tables(0).Rows(I - 1).Item("QTY_PO") > decHldQtyRec And
                              decHldQtyRec > 0 Then
                            getNewdatarow((I - 1), decHldQtyRec)
                            I += 1
                            decHldQtyRec = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        Else
                            decHldQtyRec = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT"))
                        End If
                        I += 1
                    Loop
                    dsPOdata.AcceptChanges()

                    'delete invisible rows
                    Dim strRecvID As String = " "
                    If Not dsPOdata Is Nothing Then
                        If dsPOdata.Tables.Count > 1 Then
                            If dsPOdata.Tables(0).Rows.Count > 0 Then
                                For I = 0 To dsPOdata.Tables(0).Rows.Count - 1
                                    If IsDBNull(dsPOdata.Tables(0).Rows(I).Item("hQTYLNACCPT")) Then
                                        decHldQtyAccpt = 0
                                    Else
                                        decHldQtyAccpt = Convert.ToDecimal(dsPOdata.Tables(0).Rows(I).Item("hQTYLNACCPT"))
                                    End If

                                    If decHldQtyAccpt > 0 Then
                                        'row is invisible
                                        Dim rowD As DataRow = dsPOdata.Tables(0).Rows(I)
                                        rowD.Delete()
                                    Else
                                        Try
                                            strRecvID = dsPOdata.Tables(0).Rows(I).Item("RECEIVER_ID")
                                        Catch ex As Exception
                                            strRecvID = " "
                                        End Try
                                        If Trim(strRecvID) <> "" Then
                                            'row is invisible
                                            Dim rowD As DataRow = dsPOdata.Tables(0).Rows(I)
                                            rowD.Delete()
                                        End If
                                    End If
                                Next
                                dsPOdata.AcceptChanges()
                            End If
                        End If
                    End If

                    '' Shipped Qty calculation for the non receiving users
                    If Not txtasnflag.Text = "Y" Then
                        Dim dsRecvTotals As DataSet = New DataSet()
                        Dim dtDistinctRows As DataTable

                        '' Distinct to remove duplicate rows
                        dtDistinctRows = dsPOdata.Tables(0).AsEnumerable().GroupBy(Function(row) row.Field(Of Decimal)("LINE_NBR")).Select(Function(group) group.First()).CopyToDataTable()
                        dsPOdata = New DataSet()
                        dsPOdata.Tables.Add(dtDistinctRows)

                        dsRecvTotals = getAccptRecvTotals(strPoNum1, strPOBU)

                        If Not dsRecvTotals Is Nothing Then
                            If dsRecvTotals.Tables.Count > 0 Then
                                If dsRecvTotals.Tables(0).Rows.Count > 0 Then
                                    Dim Y As Integer = 0
                                    Dim bResExists As Boolean = False
                                    For I = 0 To dsPOdata.Tables(0).Rows.Count - 1
                                        bResExists = False
                                        For Y = 0 To dsRecvTotals.Tables(0).Rows.Count - 1
                                            If dsPOdata.Tables(0).Rows(I).Item("LINE_NBR") = dsRecvTotals.Tables(0).Rows(Y).Item("LINE_NBR") Then

                                                bResExists = True
                                                Exit For
                                            End If
                                        Next
                                        If Not bResExists Then
                                            'no recv lines for this PO line s- go to the next line
                                        Else
                                            'receiving exists for selected PO line - 1) compare with PO Qty and either 2.1) delete PO line or 2.2) update this line
                                            If CType(dsPOdata.Tables(0).Rows(I).Item("QTY_PO"), Decimal) > CType(dsRecvTotals.Tables(0).Rows(Y).Item("LINE_ACCPT_TOTAL"), Decimal) Then
                                                'update this line
                                                dsPOdata.Tables(0).Rows(I).Item("QTY_LN_ACCPT") = dsRecvTotals.Tables(0).Rows(Y).Item("LINE_ACCPT_TOTAL")
                                                '' dsPOdata.Tables(0).Rows(I).Item("hQTYLNACCPT") = dsRecvTotals.Tables(0).Rows(Y).Item("LINE_ACCPT_TOTAL")
                                            Else
                                                'delete this line - it is fully receiveds
                                                Dim rowD As DataRow = dsPOdata.Tables(0).Rows(I)
                                                rowD.Delete()
                                            End If
                                        End If
                                    Next
                                    dsPOdata.AcceptChanges()
                                End If
                            End If
                        End If
                    End If

                    Dim GetRecvFlag As String = ""
                    GetRecvFlag = getASNFlag(strPoNum1)
                    Dim S As Integer = 0

                    If GetRecvFlag.Trim = "Y" Then
                        For S = 0 To dsPOdata.Tables(0).Rows.Count - 1
                            Dim RecvQty As String = ""
                            RecvQty = getAccptRecvTblRecvQty(strPoNum1, strPOBU, Convert.ToString(dsPOdata.Tables(0).Rows(S).Item("LINE_NBR")))
                            If Trim(RecvQty) <> "" Then
                                If CType(dsPOdata.Tables(0).Rows(S).Item("QTY_PO"), Decimal) > CType(RecvQty, Decimal) Then
                                    dsPOdata.Tables(0).Rows(S).Item("QTY_LN_ACCPT") = RecvQty
                                Else
                                    'delete this line - it is fully receiveds
                                    Dim rowD As DataRow = dsPOdata.Tables(0).Rows(S)
                                    rowD.Delete()
                                End If
                            Else
                                dsPOdata.Tables(0).Rows(S).Item("QTY_LN_ACCPT") = "0"
                            End If
                        Next
                        dsPOdata.AcceptChanges()
                    End If

                    Dim dtDistinct_Rows As DataTable

                    '' Distinct to remove duplicate rows
                    dtDistinct_Rows = dsPOdata.Tables(0).AsEnumerable().GroupBy(Function(row) row.Field(Of Decimal)("LINE_NBR")).Select(Function(group) group.First()).CopyToDataTable()
                    dsPOdata = New DataSet()
                    dsPOdata.Tables.Add(dtDistinct_Rows)

                    dtgPOASN.DataSource = dsPOdata
                    dtgPOASN.DataBind()


                Else
                    dtgPOASN.Visible = False
                    btnASNSubmit.Visible = False
                    lblPOASNSubmitErrorInfo.Visible = True
                    lblPOASNSubmitErrorInfo.Text = "No info when loading screen. PO #: " & strPoNum1 & " -- BU: " & strPOBU & " -- Supplier ID: " & m_sVendor & vbCrLf

                End If  '  dsPOdata.Tables(0).Rows.Count > 0 
            Else
                dtgPOASN.Visible = False
                btnASNSubmit.Visible = False
                lblPOASNSubmitErrorInfo.Visible = True
                lblPOASNSubmitErrorInfo.Text = "No info when loading screen. PO #: " & strPoNum1 & " -- BU: " & strPOBU & " -- Supplier ID: " & m_sVendor & vbCrLf
            End If
        Catch ex As Exception

        End Try
    End Sub
    Private Sub buildASNConfirmation(ByVal strPO As String)
        'Dim strCheckBU As String = CType(Session("BUSUNIT"), String)

        Dim strSQLString As String = ""
        Dim friggin As Boolean = True
        Dim strSubjWithReqId As String = "SDI ZEUS - ASN Notification - " & strPO

        'strSQLString = "SELECT ISA_SITE_EMAIL, ISA_STOCKREQ_EMAIL, ISA_NONSKREQ_EMAIL," & vbCrLf &
        '    " ISA_SITE_PRINTER," & vbCrLf &
        '    " ISA_STOCK_PRINTER, ISA_NONSTK_PRINTER" & vbCrLf &
        '    " FROM SYSADM8.PS_ISA_ENTERPRISE" & vbCrLf &
        '    " WHERE CUST_ID = '" & Session("CUSTID") & "'" & vbCrLf
        'Dim dtrEntReader As OleDbDataReader = ORDBData.GetReader(strSQLString)

        'If dtrEntReader.HasRows() = True Then
        '    dtrEntReader.Read()
        'Else
        '    dtrEntReader.Close()
        '    Dim strMessage As New Alert
        '    ltlAlert.Text = strMessage.Say("No enterprise record found")
        '    Exit Sub
        'End If
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
        Dim dstcart1 As New DataSet

        Dim dstcart2 As New DataTable

        Dim txtBody As String = ""
        Dim txtHdr As String = ""
        Dim txtGrid As String = ""
        Dim txtMsg As String = ""
        Dim strDescr1 As String
        Dim strDescr2 As String
        Dim strDescr3 As String
        Dim strDescr4 As String
        Dim strCustName As String = ""

        Dim intCntr1 As Integer = 0
        Dim strEmplEmail As String = ""
        Dim strEmplId As String = ""




        Dim Mailer As MailMessage = New MailMessage

        Mailer.From = "SDIExchange@SDI.com"
        Mailer.Cc = ""
        Mailer.Bcc = ""

        Dim strConnString As String = ORDBData.DbUrl
        Dim connectionEmail As OleDbConnection = New OleDbConnection(strConnString)
        'Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
        Dim FromAddress As String = "SDIExchange@SDI.com"
        Dim Mailcc As String = " "
        Dim MailBcc As String = Mailer.Bcc

        'If CheckOrderPriority() Then
        '    strbodyhead = "<span><B>**PRIORITY ORDER**</B></span>"
        'End If
        strbodydetl = strbodydetl & "<table width='100%' bgcolor='black'><tbody><tr><td><img src='https://www.sdizeus.com/images/SDNewLogo_Email.png' alt='SDI' vspace='0' hspace='0' /></td><td width='100%'><center><span style='font-family: Arial; font-size: x-large; text-align: center; Color:White;'>SDI Zeus</span></center><center><span style='text-align: center; margin: 0px auto;  Color:White;'>SDI ZEUS - ASN Notification</span></center></td></tr></tbody></table>" & vbCrLf
        strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<TABLE class='Email_Table' cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        ''strbodydetl = strbodydetl & "<span >Item request from </span>&nbsp;"
        ''strbodydetl = strbodydetl & Session("CONAME") & "</td>" & vbCrLf
        strbodydetl = strbodydetl & "<TD style='COLOR: red'>" & vbCrLf
        'If bApprNeeded Then
        '    If bIsPunchin Then
        '        strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        '    Else
        '        strbodydetl = strbodydetl & "<span>** ADDITIONAL APPROVALS REQUIRED **</span></td></tr>" & vbCrLf
        '    End If
        'Else
        '    strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        'End If
        strbodydetl = strbodydetl & "<TD colspan=2>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;</td></tr>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Vendor ID:</span> <span 'width:128px;'>&nbsp;" & Session("VENDOR_VP") & "</span></td>"
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span style='font-weight:bold;'>PO NO</span> <span>&nbsp;" & strPO & "</span></td></table>"
        strbodydetl = strbodydetl & "&nbsp;<br>" & vbCrLf


        ''''strbodydetl = strbodydetl & "Chg. Emp. ID:<span>&nbsp;" & dstcart2.Rows(0).Item(0) & "</span></td></tr>"

        ''''strbodydetl = strbodydetl & "<span>&nbsp;</span></td>" & vbCrLf
        '''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Name:</span> <span>&nbsp;" & Session("SupplierEmpID") & "</span></td>"
        '''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        ''''strbodydetl = strbodydetl & "Chg. Code:<span>&nbsp;" & dstcart2.Rows(0).Item(1) & "</span></td></tr>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        '''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Email:</span> <span>&nbsp;" & Session("SupplierEmpEmail") & "</span></td>"
        '''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Work Order #:</span> <span>&nbsp;" & Session("SupplierWO") & "</span></td>" & vbCrLf
        '''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Employee Phone#:</span> <span>&nbsp;" & Session("SupplierEmpPh") & "</span></td>" & vbCrLf
        '''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        ''''strbodydetl = strbodydetl & "Machine Number:<span>&nbsp;" & dstcart2.Rows(0).Item(3) & "</span></td></tr>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        ''''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        ''''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Request by Date:</span> <span>&nbsp;" & dstcart2.Rows(0).Item(5) & "</span></td>"
        '''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Submit Date:</span> <span>&nbsp;" & Now() & "</span></td>"
        '''strbodydetl = strbodydetl & "<TD colspan='2'>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>Notes:</span><br>"
        '''strbodydetl = strbodydetl & "<textarea readonly='readonly' style='width:100%;'>" & Session("SupplierNotes") & "</textarea></td></tr></table>" & vbCrLf
        ''''Include the OPRID_ENTERED_BY 
        '''strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
        '''strbodydetl = strbodydetl & "<TD>" & vbCrLf
        '''strbodydetl = strbodydetl & "<span style='font-weight:bold;'>OPR Entered By:</span> <span>&nbsp;" & HttpContext.Current.Session("USERID").ToString() & "</span></td></table>"
        '''strbodydetl = strbodydetl & "&nbsp;<br>" & vbCrLf



        'txtHdr = vbCrLf & "                              IN PLANT STORE PROGRAM"
        'txtHdr = txtHdr & vbCrLf & vbCrLf & "                    " & strSubjWithReqId & vbCrLf & vbCrLf

        'txtHdr = txtHdr & "Item Request from " & Session("CONAME") & ":" & vbCrLf & vbCrLf
        'txtHdr = txtHdr & "   SDI Requisition Number: " & stritemid & vbCrLf
        'txtHdr = txtHdr & "   Employee Name: " & Session("SupplierEmpID") & vbCrLf
        ''txtHdr = txtHdr & "   Request by Date: " & dstcart2.Rows(0).Item(5) & vbCrLf
        'txtHdr = txtHdr & "   Submit Date: " & Now() & vbCrLf
        'txtHdr = txtHdr & "   Chg. Emp. ID: " & dstcart2.Rows(0).Item(0) & vbCrLf
        'txtHdr = txtHdr & "   Chg. Code: " & dstcart2.Rows(0).Item(1) & vbCrLf
        'txtHdr = txtHdr & "   Work Order #: " & Session("SupplierWO") & vbCrLf
        ''txtHdr = txtHdr & "   Machine Number: " & dstcart2.Rows(0).Item(3) & vbCrLf

        'Dim strNotes1 As String = UCase(Left(dstcart2.Rows(0).Item(4), 68))
        'Dim strNotes2 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 69, 68))
        'Dim strNotes3 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 137, 68))
        'Dim strNotes4 As String = UCase(Mid(dstcart2.Rows(0).Item(4), 205, 50))
        'txtHdr = txtHdr & "   Notes: " & Session("SupplierNotes") & vbCrLf
        'If Not Trim(strNotes2) = "" Then
        '    txtHdr = txtHdr & "          " & strNotes2 & vbCrLf
        'End If
        'If Not Trim(strNotes3) = "" Then
        '    txtHdr = txtHdr & "          " & strNotes3 & vbCrLf
        'End If
        'If Not Trim(strNotes4) = "" Then
        '    txtHdr = txtHdr & "          " & strNotes4 & vbCrLf
        'End If
        'txtHdr = txtHdr & vbCrLf

        Dim strOROItemID As String
        Dim bolMachineNum As Boolean = False
        Dim nItem_id_Price As Integer = 0

        Dim dstcartSTK As New DataTable

        Try

            dstcartSTK = buildASNforemailSC(dstcart1)

            intGridloop = dstcartSTK.Rows.Count
            X = 0
            decOrderTot = 0

            For I = 0 To intGridloop - 1



                'Dim itm As sdiItem = sdiItem.GetItemInfo(strOROItemID)

                txtGrid = "Line Number : " & dstcartSTK.Rows(X).Item("Line Number") & vbCrLf
                txtGrid = "SDI Item ID : " & dstcartSTK.Rows(X).Item("SDI ItemID") & vbCrLf

                txtGrid = txtGrid & "     Manuf Item ID : " & dstcartSTK.Rows(X).Item("Mfg Item ID") & vbCrLf
                txtGrid = txtGrid & "     Vendor Item ID : " & dstcartSTK.Rows(X).Item("Vnd Item ID") & vbCrLf
                txtGrid = txtGrid & "     PO QTY : " & dstcartSTK.Rows(X).Item("PO QTY") & vbCrLf
                txtGrid = txtGrid & "     Open QTY : " & dstcartSTK.Rows(X).Item("Open QTY") & vbCrLf
                'txtGrid = txtGrid & "     Price: " & dstcartSTK.Rows(X).Item("Price") & vbCrLf
                txtGrid = txtGrid & "     QTY : " & dstcartSTK.Rows(X).Item("QTY") & vbCrLf & vbCrLf
                txtGrid = txtGrid & "     UOM : " & dstcartSTK.Rows(X).Item("UOM") & vbCrLf & vbCrLf
                txtGrid = txtGrid & "     Price : " & dstcartSTK.Rows(X).Item("Price") & vbCrLf & vbCrLf
                txtGrid = txtGrid & "     Tracking No : " & dstcartSTK.Rows(X).Item("Tracking #") & vbCrLf & vbCrLf
                'Dim strSrc1 As String
                'Dim cLink As String = ""
                'strSrc1 = dstcartSTK.Rows(X).Item("Tracking #")
                'If strSrc1.Contains("1Z") Then
                '    txtGrid = txtGrid & "     Tracking No : " & dstcartSTK.Rows(X).Item("Tracking #") & vbCrLf & vbCrLf
                'Else
                '    Dim m_cURL1 As String = "https://www.fedex.com/apps/fedextrack/?action=track&trackingnumber=" & strSrc1 & "&cntry_code=us&locale=en_US"
                '    'SBnstk.Append("<td align=""center""><a href=""" & m_cURL1 & """ target=""_blank"">" & strSrc1 & "</a></td>")
                '    cLink = "<a href=""" & m_cURL1 & """ target=""_blank"">" & strSrc1 & "</a> "
                '    txtGrid = txtGrid & "     Tracking No : " & cLink & vbCrLf & vbCrLf

                'End If

                txtGrid = txtGrid & "     Ship Via : " & dstcartSTK.Rows(X).Item("Ship via") & vbCrLf & vbCrLf
                txtGrid = txtGrid & "     Delivery Date : " & dstcartSTK.Rows(X).Item("Delivery Date") & vbCrLf & vbCrLf
                txtGrid = txtGrid & "     Common Carrier : " & dstcartSTK.Rows(X).Item("Common Carrier") & vbCrLf & vbCrLf
                txtBody = txtBody & txtGrid
                'decOrderTot = decOrderTot + Convert.ToDecimal(dstcartSTK.Rows(X).Item("Ext. Price"))

                X = X + 1

            Next
            dstcartSTK.AcceptChanges()

            '// material request for STOCK items
            '//     received by SYSADM8.PS_ISA_ENTERPRISE.ISA_STOCKREQ_EMAIL
            If dstcartSTK.Rows.Count > 0 Then

                dtgcart = New DataGrid

                If Session("VENDOR_VP").ToString.ToUpper.Substring(0, 1) = "W" Then
                    dstcartSTK.Columns.Remove("Price")
                    dstcartSTK.Columns.Remove("Ext.Price")
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

                strItemtype = "<center><span >SDI ZEUS - ASN Notification</span></center>"
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
                    Mailer.Subject = "TEST SITE SDI ZEUS - ASN Notification - " & strPO
                Else
                    getvendinfo()
                    Mailer.To = Session("Vendor_EmpEmail")
                    Mailer.Subject = "SDI ZEUS - ASN Notification - " & strPO
                End If


                Mailer.BodyFormat = System.Web.Mail.MailFormat.Html

                Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
                Dim MailAttachmentName As String()
                Dim MailAttachmentbytes As New List(Of Byte())()

                Try
                    SDIEmailService.EmailUtilityServices("MailandStore", Mailer.From, Mailer.To, Mailer.Subject, String.Empty, String.Empty, Mailer.Body, "MATSTOCK", MailAttachmentName, MailAttachmentbytes.ToArray())
                    getvendinfo()
                    If Not Session("Vendor_POEmpID") = Nothing Or Session("Vendor_POEmpID") = "" Then
                        sendNotification(Session("Vendor_POEmpID"), Mailer.Subject, Session("Vendor_POReqID"))
                        sendWebNotification(Session("Vendor_POEmpID"), Mailer.Subject)
                    End If
                Catch ex As Exception
                    Dim strErr As String = ex.Message
                End Try


                SWstk.Close()
                htmlTWstk.Close()


            End If

        Catch ex As Exception
            'error sending stock e-mail
            Dim sMyErrorString21 As String = "buildASNConfirmation'. " & vbCrLf &
                                            "User ID: = " & Session("OPERID") & " ;BUSINESS_UNIT_PO = " & Session("PODetailViewBU") & " ;Vendor ID = " & Session("VENDOR_VP") & " ;PO_ID = " & Session("PODetailViewPO") & " ; Error Message = " & ex.Message
            SendSDiExchErrorMail(sMyErrorString21, "Error in buildASNConfirmation.")
        End Try




    End Sub


    Private Function buildASNforemailSC(ByVal dstcart1 As DataSet) As DataTable

        Dim dr As DataRow
        Dim I As Integer
        Dim J As Integer
        Dim T As Integer
        Dim strTrckNo As String = ""
        Dim strShipvia As String = ""
        Dim strpocom As String = ""
        Dim strShpDt As Date
        Dim bolTrack As Boolean = False
        Dim dstcart As DataTable
        dstcart = New DataTable
        Try
            dstcart.Columns.Add("Line Number")
            dstcart.Columns.Add("SDI ItemID")
            dstcart.Columns.Add("Mfg Item ID")
            dstcart.Columns.Add("Vnd Item ID")
            dstcart.Columns.Add("PO QTY")
            dstcart.Columns.Add("Open QTY")
            dstcart.Columns.Add("QTY")
            dstcart.Columns.Add("UOM")
            dstcart.Columns.Add("Price")
            dstcart.Columns.Add("Ext.Price")
            dstcart.Columns.Add("Tracking #")
            dstcart.Columns.Add("Ship via")
            dstcart.Columns.Add("Delivery Date")
            dstcart.Columns.Add("Common Carrier")



            dstcart1 = Session("PODATA")
            For I = 0 To dtgPOASN.Items.Count - 1
                Dim item As GridDataItem = dtgPOASN.Items(I)
                'Dim Inv As String
                'Dim ctrl As System.Web.UI.Control = Nothing
                'ctrl = item.FindControl(id:="txtTrckno")
                'If TypeOf ctrl Is TextBox Then
                '    Inv = CDbl(CType(ctrl, TextBox).Text.Trim)
                'End If
                Dim txtQTY As TextBox = CType(item.FindControl("txtQTY"), TextBox)
                Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)

                If Not txtQTY.Text.Trim = "" Then

                    'Dim dpDeliveredDate As RadDatePicker = CType(item.FindControl("dpDeliveredDate"), RadDatePicker)
                    'Dim cmbShipvia As DropDownList = CType(item.FindControl("cmbShipvia"), DropDownList)
                    'Dim txtTrckno As TextBox = CType(item.FindControl("txtTrckno"), TextBox)
                    'Dim txtCommon As TextBox = CType(item.FindControl("txtCommon"), TextBox)
                    Dim lblOpenQTY As Label = CType(item.FindControl("lblOpenQTY"), Label)
                    Dim intPoLinenum As Integer = item("LINE_NBR").Text
                    Dim intSchedNum As Integer = item("SCHED_NBR").Text
                    Dim strpoitem As String = item("INV_ITEM_ID").Text
                    Dim strpoUOM As String = item("UNIT_OF_MEASURE").Text
                    Dim strpomfg As String = item("MFG_ITM_ID").Text
                    Dim strpovndr As String = item("ITM_ID_VNDR").Text
                    Dim strpoqty As String = item("QTY_PO").Text
                    Dim decQty As String = Trim(CType(txtQTY, TextBox).Text)
                    Dim strpoopentqty As String = CType(lblOpenQTY, Label).Text

                    strTrckNo = Trim(CType(txtTrckno, TextBox).Text)

                    'strTrckNo = CType(item.FindControl("txtTrckno"), TextBox).Text
                    strShipvia = CType(item.FindControl("cmbShipvia"), DropDownList).SelectedValue
                    strShpDt = CType(item.FindControl("dpDeliveredDate"), RadDatePicker).SelectedDate
                    strpocom = CType(item.FindControl("txtCommon"), TextBox).Text

                    dr = dstcart.NewRow()
                    dr("Price") = dstcart1.Tables(0).Rows(I).Item("PRICE_PO")
                    dr("Ext.Price") = dstcart1.Tables(0).Rows(I).Item("MERCHANDISE_AMT")
                    'End If
                    'If Not txtQTY.Text.Trim = "" Then
                    'getdefault(I, T)



                    'dr = dstcart.NewRow()
                    dr("Line Number") = intPoLinenum
                    Try
                        dr("SDI ItemID") = strpoitem
                    Catch ex As Exception
                        dr("SDI ItemID") = " "
                    End Try

                    Try
                        dr("Mfg Item ID") = strpomfg
                    Catch ex As Exception
                        dr("Mfg Item ID") = " "
                    End Try

                    Try
                        dr("Vnd Item ID") = strpovndr
                    Catch ex As Exception
                        dr("Vnd Item ID") = " "
                    End Try


                    dr("PO QTY") = strpoqty
                    dr("Open QTY") = strpoopentqty
                    dr("QTY") = decQty
                    dr("UOM") = strpoUOM

                    dr("Ship via") = strShipvia
                    Try
                        If strShipvia.Contains("UPS") Then
                            bolTrack = checkUPSnumber(Replace(strTrckNo, " ", ""))
                            If bolTrack = False Then
                                Dim URL As String = "https://wwwapps.ups.com/WebTracking/?action=track&InquiryNumber1=" & strTrckNo & "&cntry_code=us&locale=en_US"


                                Dim m_cURL1 As String = "<a href=""" & URL & """ target=""_blank"">" & strTrckNo & "</a>"
                                dr("Tracking #") = m_cURL1


                            End If
                        Else
                            If strShipvia.Contains("FEDEX") Then
                                bolTrack = checkFEDEXnumber(Replace(strTrckNo, " ", ""))
                                If bolTrack = False Then
                                    Dim URL As String = "https://www.fedex.com/apps/fedextrack/?action=track&trackingnumber=" & strTrckNo & "&cntry_code=us&locale=en_US"

                                    Dim m_cURL1 As String = "<a href=""" & URL & """ target=""_blank"">" & strTrckNo & "</a>"
                                    dr("Tracking #") = m_cURL1

                                Else
                                    dr("Tracking #") = strTrckNo

                                End If
                            Else
                                dr("Tracking #") = strTrckNo
                            End If

                        End If

                    Catch ex As Exception
                        dr("Tracking #") = " "
                    End Try

                    dr("Delivery Date") = strShpDt
                    Try
                        dr("Common Carrier") = strpocom
                    Catch ex As Exception
                        dr("Common Carrier") = " "
                    End Try



                    dstcart.Rows.Add(dr)

                End If

                'If Not txtTrckno Is Nothing Then

                'End If
                'If Not String.IsNullOrEmpty(strTrckNo) Then
                '    strTrckNo = ""
                'End If
            Next
            'Next



        Catch ex As Exception

        End Try
        Return dstcart
    End Function

    Public Sub sendNotification(ByVal Session_UserID As String, ByVal subject As String, ByVal orderNo As String)
        Dim response As String
        Try
            Dim NotificationContent As String = subject
            Dim _notificationResult As New DataSet
            Dim notificationSQLStr = "SELECT DEVICE_INFO FROM SDIX_USER_TOKEN WHERE ISA_EMPLOYEE_ID = '" + Session_UserID + "' AND DEVICE_INFO IS NOT NULL AND LOWER(DEVICE_INFO) <> 'unknown unknown' AND LOWER(DEVICE_INFO)<>'webapp' "
            _notificationResult = ORDBData.GetAdapter(notificationSQLStr)
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
    Public Sub sendWebNotification(ByVal Session_UserID As String, ByVal subject As String)
        Try
            Dim connection As OleDbConnection = New OleDbConnection(ORDBData.DbUrl)
            Dim trnsactSession As OleDbTransaction = Nothing
            Dim _notificationResult As New DataSet
            Dim notificationSQLStr = "select max(NOTIFY_ID) As NOTIFY_ID from SDIX_NOTIFY_QUEUE where USER_ID='" + Session_UserID + "'"
            _notificationResult = ORDBData.GetAdapter(notificationSQLStr)
            Dim NotifyID As Int16 = 1
            If _notificationResult.Tables.Count > 0 Then
                Try
                    NotifyID = _notificationResult.Tables(0).Rows(0).Item("NOTIFY_ID")
                    NotifyID = NotifyID + 1
                Catch ex As Exception
                End Try
            End If
            'connectOR.Open()
            connection.Open()
            trnsactSession = connection.BeginTransaction

            Dim strSQLstring As String = "INSERT INTO SDIX_NOTIFY_QUEUE" & vbCrLf &
        " (NOTIFY_ID, NOTIFY_TYPE, USER_ID,DTTMADDED, STATUS,LINK, HTMLMSG, ATTACHMENTS, TITLE) VALUES ('" & NotifyID & "'," & vbCrLf &
        " 'ASN'," & vbCrLf &
        " '" & Session_UserID & "'," & vbCrLf &
        " sysdate," & vbCrLf &
        " 'N'," & vbCrLf &
         " ' ',' ',' '," & vbCrLf &
        " '" & subject & "')" & vbCrLf

            'Dim command1 As OleDbCommand
            'command1 = New OleDbCommand(strSQLstring)
            Try
                Dim rowsaffected As Integer = 0
                Dim exError As Exception
                If ORDBData.ExecuteNonQuery(trnsactSession, connection, strSQLstring, rowsaffected, exError) Then
                    trnsactSession.Commit()
                Else

                    trnsactSession.Rollback()
                End If

            Catch ex As Exception

            End Try
            Try
                'connectOR.Close()
                connection.Close()
                trnsactSession = Nothing
                connection = Nothing
            Catch ex As Exception

            End Try

        Catch ex As Exception
        End Try
    End Sub
    Private Function getvendinfo() As DataSet

        Dim POEmpEmail As String
        Dim strSQLStringEmail As String


        getreqid()
        getempid()
        Dim SupEmp As String = Session("Vendor_POEmpID")
        strSQLStringEmail = "SELECT ISA_EMPLOYEE_EMAIL FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID = '" + SupEmp + "' AND ACTIVE_STATUS = 'A'"
        Try
            POEmpEmail = ORDBData.GetScalar(strSQLStringEmail)
            Session("Vendor_EmpEmail") = POEmpEmail
        Catch ex As Exception

        End Try


    End Function

    Private Function getreqid() As DataSet

        Dim POReqID As String
        Dim strSQLStringReqID As String

        Dim strPO As String = ""
        strPO = txtPO_Single.Text

        strSQLStringReqID = "SELECT REQ_ID FROM PS_PO_LINE_DISTRIB WHERE PO_ID = '" + strPO + "'"
        Try
            POReqID = ORDBData.GetScalar(strSQLStringReqID)
            Session("Vendor_POReqID") = POReqID
        Catch ex As Exception

        End Try


    End Function

    Private Function getempid() As DataSet

        Dim POEmpID As String = ""
        Dim strSQLStringEmpID As String

        Dim strPOReqID As String = Session("Vendor_POReqID")

        strSQLStringEmpID = " SELECT ISA_EMPLOYEE_ID
 FROM PS_ISA_ORD_INTF_LN WHERE ORDER_NO = '" + strPOReqID + "'"
        Try
            POEmpID = ORDBData.GetScalar(strSQLStringEmpID)
            Session("Vendor_POEmpID") = POEmpID
            'If Not String.IsNullOrEmpty(POEmpID) Then
            '    Session("Vendor_POEmpID") = POEmpID
            'Else
            '    Session("Vendor_POEmpID") = Session("strBuyerEmail")
            'End If

        Catch ex As Exception

        End Try


    End Function

    Public Class DeleteItem
        Public Property PartId As Integer
    End Class

    Public Class Delete
        Public Property DeleteItems As List(Of DeleteItem)
    End Class

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
End Class