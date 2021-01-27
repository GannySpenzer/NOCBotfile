Imports Insiteonline.WebPartnerFunctions.WebPSharedFunc
Imports System.Data.OleDb
Imports Insiteonline.BuildingMenus.BuildMenu
Imports System.Data.SqlClient
Imports Telerik.Web.UI
Imports System.Linq
Imports Insiteonline.clsBreadcrumbTrailGenerator
Imports Insiteonline.com.tf7.www1
Imports Insiteonline.com.tf7.www
Imports System.Text
Imports Insiteonline.webp

Imports System.Configuration
Imports Insiteonline.WebPartnerFunctions
Imports Insiteonline.UnilogSearch
Imports Insiteonline.UnilogORDBData
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports System.Net
Imports System.Drawing.Color

'**********************************************************************************************************
' got another field from table SYSADM8.PS_ISA_SDIEX_PRCBU: FIRM_PRICE_FLG - in STAR only - VR 08/07/2018
'**********************************************************************************************************

Public Class ItemDetailNew
    Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Protected WithEvents ltlAlert As System.Web.UI.WebControls.Literal

    Protected WithEvents lblmessage As System.Web.UI.WebControls.Label
    Protected WithEvents HyperLinkLoc As System.Web.UI.WebControls.HyperLink


    Protected WithEvents Label4 As System.Web.UI.WebControls.Label
    Protected WithEvents Label5 As System.Web.UI.WebControls.Label
    Protected WithEvents Label6 As System.Web.UI.WebControls.Label
    Protected WithEvents Label7 As System.Web.UI.WebControls.Label
    Protected WithEvents Label8 As System.Web.UI.WebControls.Label
    Protected WithEvents Label9 As System.Web.UI.WebControls.Label
    Protected WithEvents lblNoitems As System.Web.UI.WebControls.Label
    'Protected WithEvents lnkAttachments As System.Web.UI.WebControls.HyperLink


    'NOTE: The following placeholder declaration is required by the Web Form Designer.
    'Do not delete or move it.
    Private designerPlaceholderDeclaration As System.Object
    Dim bCosign As Boolean = False


    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Dim strbu As String

    Protected Overrides Sub OnPreInit(ByVal e As EventArgs)
        MyBase.OnPreInit(e)
        Try
            If Not Request.QueryString("HOME") Is Nothing Then
                If CType(Request.QueryString("HOME"), String) = "N" Then
                    MasterPageFile = "ISOLLogin.master"
                Else
                    MasterPageFile = "~/MasterPage/SDIXMaster.Master"
                End If
            Else
                MasterPageFile = "~/MasterPage/SDIXMaster.Master"
            End If
        Catch ex As Exception
            MasterPageFile = "~/MasterPage/SDIXMaster.Master"
        End Try
    End Sub

    Dim dsTrackEnterprise As DataSet
    Dim dsTrackUserPrivs As DataSet
    Dim dsUserAddedToTrack As DataSet
    Dim strTrackDBType As String
    Dim strTrackDBGuid As String
    Dim strTrackDBUser As String
    Dim strTrackDBPwd As String
    Dim strTrackDBCustId As String
    Dim strTrckUsrName As String
    Dim strTrckUsrGuid As String

    Dim trackFlag = "Not Tracked"
    Dim dsLotNumber1 As DataSet
    Dim lc As String
    Dim sno As String

    Public Shared itemId As String

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim strError As String = ""
        Try
            If Not Page.IsPostBack Then

                sdiTabStrip.FindTabByValue("TST").Visible = False
                'Code for SDI Track Starts 
                Try
                    '     GetTangoUsernamePwd()
                    strTrckUsrName = ""
                    strTrckUsrGuid = ""
                    Try
                        strTrckUsrName = CType(Session("SDITRACKUSRNAME"), String)
                    Catch ex As Exception
                        strTrckUsrName = ""
                    End Try
                    Try
                        strTrckUsrGuid = CType(Session("SDITRACKPWD"), String)
                    Catch ex As Exception
                        strTrckUsrGuid = ""
                    End Try
                    lblsditrackErr.Visible = False
                    Try
                        strTrackDBType = CType(Session("TrackDBType"), String)
                    Catch ex As Exception
                        strTrackDBType = ""
                    End Try
                    Try
                        strTrackDBUser = CType(Session("TrackDBUser"), String)
                    Catch ex As Exception
                        strTrackDBUser = ""
                    End Try
                    Try
                        strTrackDBPwd = CType(Session("TrackDBPassword"), String)
                    Catch ex As Exception
                        strTrackDBPwd = ""
                    End Try
                    Try
                        strTrackDBGuid = CType(Session("TrackDBGUID"), String)
                    Catch ex As Exception
                        strTrackDBGuid = ""
                    End Try
                    Try
                        strTrackDBCustId = CType(Session("TrackDBCust"), String)
                    Catch ex As Exception
                        strTrackDBCustId = ""
                    End Try
                    ' New Changes for SDiTrack - Item Details Page 
                    Dim sditrackvalue As String = CType(Session("Trackvalue"), String)
                    Dim strPrivilege As String = ""
                    If (Trim(strTrackDBType) <> "") And (Not String.IsNullOrEmpty(sditrackvalue)) And (Trim(sditrackvalue) = "Y") Then
                        sdiTabStrip.FindTabByValue("TST").Visible = True
                    Else
                        sdiTabStrip.FindTabByValue("TST").Visible = False
                    End If
                    If Trim(strTrckUsrName) = "" Then
                        sdiTabStrip.FindTabByValue("TST").Visible = False
                    Else
                        btnSetAsTrack.Visible = True
                        btnSentToTrack.Visible = True
                        lblsditrackErr.Visible = False
                    End If
                    'Code for SDI Track Ends
                Catch ex As Exception
                    lblsditrackErr.Visible = True
                    lblsditrackErr.Text = "SDiTrack Issue. Please call Help Desk"
                    sdiTabStrip.FindTabByValue("TST").Visible = True
                End Try
                Dim lblTitle As Label = CType(Master.FindControl("lblTitle"), Label)
                If Session("Noncat") = "Noncat" Then
                    lblTitle.Text = "Marketplace Item Details"
                    Me.Title = "Marketplace Item Details"
                    sdiTabStrip.FindTabByValue("CAT").Text = "Marketplace Detail"
                Else
                    lblTitle.Text = "Catalog Item Details"
                    sdiTabStrip.FindTabByValue("CAT").Text = "Catalog Detail"
                End If
                'If Session("USESOLRSEARCH") = "Y" Then
                '    lblTitle.Text = "Catalog Item Details (SOLR)"
                'End If
                Session("SCREENNAME") = "ItemDetailNew.aspx"
            End If
            Response.Cache.SetCacheability(HttpCacheability.NoCache)
            Response.Cache.AppendCacheExtension("no-store, must-revalidate")
            Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate")
            Response.AppendHeader("Pragma", "no-cache")
            Response.AppendHeader("Expires", "Mon, 26 Jul 1997 05:00:00 GMT")
            Response.AppendHeader("Vary", "*")
            Dim strMessage As New Alert
            strbu = Session("BUSUNIT")

            'Try
            '    Dim strDefltUrl As String = ""
            '    strDefltUrl = VirtualPathUtility.ToAbsolute("~/sessiontimeout.aspx")
            '    Response.AppendHeader("Refresh", Convert.ToString(Session.Timeout * 60) & "; URL=" & strDefltUrl)
            'Catch ex As Exception
            'End Try

            If Session("SDIEMP") = "" Or Session("USERID") = "" Then
                Session.RemoveAll()
                Response.Redirect("default.aspx")
            End If
           
            If Not Page.IsPostBack Then

                If Session("NYCMAINT") = "Y" Then
                    txtQty.Visible = False
                    imgBtnAddToCart.Visible = False
                Else
                    txtQty.Visible = True
                    imgBtnAddToCart.Visible = True
                End If

                Try
                    If Not Request.QueryString("HOME") Is Nothing Then
                        If CType(Request.QueryString("HOME"), String) = "N" Then

                            Dim myMenu1 As Telerik.Web.UI.RadMenu = CType(Master.FindControl("MinimumMenu"), Telerik.Web.UI.RadMenu)
                            myMenu1.Visible = False

                            txtQty.Visible = False
                            imgBtnAddToCart.Visible = False

                        End If
                    End If

                Catch exMnu As Exception

                End Try

                WebLog()
                RadMultiPage1.RenderSelectedPageOnly = True
                Session.Remove("classid")
                txtItemID.Text = Request.QueryString("ItemID")
                panelSTK.Visible = True
                PanelHDR.Visible = True
                SDIUpdatePanel.Visible = True
                If txtItemID.Text.Length > 3 Then
                    If Not Session("SITEPREFIX") Is Nothing Then
                        Try
                            If txtItemID.Text.Substring(0, 3) = Session("SITEPREFIX") Then
                                txtItemID.Text = txtItemID.Text.Substring(3)
                            End If
                        Catch ex As Exception

                        End Try
                    End If
                End If
                strError += " " & Request.QueryString("ItemID") & "  ;    " & txtItemID.Text & "  ;  "  ' do not delete! tracing run-time error area
                If Session("ShowPrice") = "0" Then
                    lblpricetext.Visible = False
                    lblPrice.Visible = False
                    lblCurrencyCode.Visible = False
                Else
                    lblpricetext.Visible = True
                    lblPrice.Visible = True
                    lblCurrencyCode.Visible = True
                End If
                Dim dtSearchResults As DataTable
                Dim response As String
                If String.IsNullOrEmpty(Request.QueryString("ItemAttributeId")) Then
                    If Session("Noncat") = "Noncat" Then
                        sdiTabStrip.Tabs.FindTabByValue("CAT").Text = "Marketplace Item Details"
                        sdiTabStrip.Tabs(1).Visible = False
                        RadMultiPage1.PageViews(1).Visible = False
                        sdiTabStrip.Tabs(2).Visible = False
                        RadMultiPage1.PageViews(2).Visible = False
                        sdiTabStrip.Tabs(3).Visible = False
                        RadMultiPage1.PageViews(3).Visible = False
                        sdiTabStrip.Tabs(4).Visible = False
                        RadMultiPage1.PageViews(4).Visible = False
                        sdiTabStrip.Tabs(5).Visible = False
                        RadMultiPage1.PageViews(5).Visible = False
                        sdiTabStrip.Tabs(6).Visible = False
                        RadMultiPage1.PageViews(6).Visible = False
                        sdiTabStrip.Tabs(7).Visible = False
                        RadMultiPage1.PageViews(7).Visible = False
                        sdiTabStrip.Tabs(8).Visible = False
                        RadMultiPage1.PageViews(8).Visible = False
                        dtSearchResults = Session("UnilogSearch")
                        dtSearchResults = (From dt In dtSearchResults.AsEnumerable() Where dt.Field(Of String)("clientPartNumber") = Session("clientPartNumber")).CopyToDataTable()
                    Else
                        Dim manfPNo As String = Request.QueryString("MfgPartNo")
                        Dim searchVal As String = ""
                        'manfPNo = manfPNo.Replace(" ", "")
                        If manfPNo.Trim <> "" Then
                            searchVal = Request.QueryString("itemid") & " " & Chr(34) & manfPNo & Chr(34)
                        Else
                            searchVal = Request.QueryString("itemid")
                        End If
                        response = UnilogSearch.GetSearchResults(UnilogSearch.SearchType.customerPartnumber, searchVal, Convert.ToString(Session("CplusDB")), String.Empty, 1)
                        dtSearchResults = UnilogSearch.ParseUnilogJsonValue(response, Convert.ToString(Session("CplusDB")))
                    End If

                    Session("UnilogSearch") = dtSearchResults
                Else
                    sdiTabStrip.Tabs(1).Visible = False
                    RadMultiPage1.PageViews(1).Visible = False
                    sdiTabStrip.Tabs(2).Visible = False
                    RadMultiPage1.PageViews(2).Visible = False
                    sdiTabStrip.Tabs(3).Visible = False
                    RadMultiPage1.PageViews(3).Visible = False
                    sdiTabStrip.Tabs(4).Visible = False
                    RadMultiPage1.PageViews(4).Visible = False
                    sdiTabStrip.Tabs(5).Visible = False
                    RadMultiPage1.PageViews(5).Visible = False
                    sdiTabStrip.Tabs(6).Visible = False
                    RadMultiPage1.PageViews(6).Visible = False
                    sdiTabStrip.Tabs(7).Visible = False
                    RadMultiPage1.PageViews(7).Visible = False
                    sdiTabStrip.Tabs(8).Visible = False
                    RadMultiPage1.PageViews(8).Visible = False
                    dtSearchResults = Session("UnilogSearch")
                    Dim strItmAttr As String = Request.QueryString("ItemAttributeId")
                    dtSearchResults = (From dt In dtSearchResults.AsEnumerable() Where dt.Field(Of String)("ItemAttributeId") = strItmAttr).CopyToDataTable()
                End If
                'do a search on column customer_part_number rather than a general search

                If dtSearchResults.Rows.Count > 0 Then
                    Dim bFound As Boolean = False
                    Dim iRowIndex As Integer = 0
                    Dim Manufacturepartnumber As String = ""
                    Dim sPrefixedTargetItemID As String = PrependSitePrefix(Session("ITEMMODE").ToString, Request.QueryString("itemid").ToString.Trim.ToUpper, Session("SITEPREFIX").ToString)
                    While Not bFound And iRowIndex < dtSearchResults.Rows.Count
                        itemId = Convert.ToString(dtSearchResults.Rows(iRowIndex)("customerPartnumber"))
                        Manufacturepartnumber = Convert.ToString(dtSearchResults.Rows(iRowIndex)("manfPartNumber"))
                        If itemId.Trim.ToUpper = sPrefixedTargetItemID And Manufacturepartnumber.Trim.ToUpper = Request.QueryString("MfgPartNo") Then
                            bFound = True
                        Else
                            iRowIndex = iRowIndex + 1
                        End If
                    End While
                    If iRowIndex < dtSearchResults.Rows.Count Then
                        itemId = Convert.ToString(dtSearchResults.Rows(iRowIndex)("customerPartnumber"))
                        If String.IsNullOrEmpty(itemId) Then
                            itemId = Convert.ToString(dtSearchResults.Rows(iRowIndex)("customerPartnumber_CatalogZero"))
                        End If
                    Else
                        itemId = Convert.ToString(dtSearchResults.Rows(dtSearchResults.Rows.Count - 1)("customerPartnumber"))
                        If String.IsNullOrEmpty(itemId) Then
                            itemId = Convert.ToString(dtSearchResults.Rows(dtSearchResults.Rows.Count - 1)("customerPartnumber_CatalogZero"))
                        End If
                        iRowIndex = dtSearchResults.Rows.Count - 1
                    End If

                    lblproductidtext.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("customerPartnumber_CatalogZero"))
                    lblClientDesc.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("clientDesc"))
                    lblDescriptionText.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("longdescriptionone"))
                    lblFeaturestext.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("description"))
                    lblManufacturerPartNumber.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("manfPartNumber"))
                    txtMFGPartNumber.Text = dtSearchResults.Rows(iRowIndex)("manfPartNumber").ToString
                    lblManufacturerName.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("manufacturer"))
                    txtMFG.Text = dtSearchResults.Rows(iRowIndex)("manufacturer").ToString
                    txtClientPartNumber.Text = dtSearchResults.Rows(iRowIndex)("clientPartNumber").ToString
                    ' <Supplier Name>
                    Dim strSupplierName As String = ""
                    strSupplierName = dtSearchResults.Rows(iRowIndex)("supplierName").ToString
                    If Trim(strSupplierName) <> "" Then
                        Select Case strSupplierName
                            Case "ORS NASCO"
                                ' show disclaimer
                                lblDisclaimerText.Visible = True
                            Case Else
                                ' make label invisible
                                lblDisclaimerText.Visible = False
                        End Select
                    Else
                        ' make label invisible
                        lblDisclaimerText.Visible = False
                    End If
                    Dim ExtDesc1 As String = ""
                    Try
                        ExtDesc1 = Convert.ToString(dtSearchResults.Rows(iRowIndex)("ExtndDesc"))
                    Catch ex As Exception
                        ExtDesc1 = ""
                    End Try
                    btnShowExtndOraDesc.Visible = ExtDesc1.Length > 1
                    lblDescriptionhdr.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("longdescriptionone"))
                    lblItemID.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("customerPartnumber_CatalogZero"))
                    lblMfg.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("manufacturer"))
                    lblUOM.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("UOM"))
                    lblMfgID.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("manfPartNumber"))
                    ltrOraExtendDesc.Text = ExtDesc1
                    If Not Session("Noncat") = "Noncat" Then
                        lblCatalogPath.Text = GetCatalogPath(Convert.ToString(dtSearchResults.Rows(iRowIndex)("UNSPSC")))
                    Else
                        lblCatalogPath.Text = ""
                    End If
                    Dim Getunspsc As String = Convert.ToString(dtSearchResults.Rows(iRowIndex)("UNSPSC"))
                    If Not String.IsNullOrEmpty(Getunspsc.Trim()) Then
                        lblUnspsc.Text = Getunspsc
                        tdUnspsc.Visible = True
                    End If
                    Dim ThumbnailImage As String = Convert.ToString(dtSearchResults.Rows(iRowIndex)("ThumbnailImagefile"))
                    ' New Code for Product Image Binding 
                    'If UnilogORDBData.ImageVaild(ThumbnailImage, "DETAIL_PAGE") Then
                    '    Image1.ImageUrl = ConfigurationManager.AppSettings("itemImageServerDomain") & "DETAIL_PAGE/" & ThumbnailImage
                    '    Image1.Style.Add("cursor", "hand")
                    'Else
                    '    Image1.Visible = False
                    '    imagecontent_close.Style.Add("display", "none")
                    '    prod_content_fullview.Attributes.Add("class", "Prod-details-withoutImg")
                    'End If

                    ' New code for Multi Image view
                    If Not String.IsNullOrEmpty(ThumbnailImage.Trim()) Then
                        Dim ProdImg As String() = ThumbnailImage.Split(",")
                        Dim Image As New Telerik.Web.UI.ImageGalleryItem
                        Try
                            For Each I As String In ProdImg
                                If UnilogORDBData.ImageVaild(I, "DETAIL_PAGE") Then
                                    Image.ImageUrl = ConfigurationManager.AppSettings("itemImageServerDomain") & "DETAIL_PAGE/" & I
                                    'Image.Description = "Product Image"
                                    Image.ThumbnailUrl = ConfigurationManager.AppSettings("itemImageServerDomain") & "DETAIL_PAGE/" & I
                                    rigProdImg.Items.Add(Image)
                                ElseIf ThumbnailImage.Contains("amazon") Then
                                    Image.ImageUrl = ThumbnailImage
                                    Image.ThumbnailUrl = ThumbnailImage
                                    rigProdImg.Items.Add(Image)
                                ElseIf ThumbnailImage.Contains("sdilogo-notavail2") Then
                                    Image.ImageUrl = "Images\SDI-NoImage-New.png"
                                    Image.ThumbnailUrl = ThumbnailImage
                                    rigProdImg.Items.Add(Image)
                                ElseIf Session("Noncat") = "Noncat" Then

                                    Image.ImageUrl = ThumbnailImage
                                    Image.ThumbnailUrl = ThumbnailImage
                                    rigProdImg.Items.Add(Image)
                                Else
                                    Image.ImageUrl = "Images\noimage_new.png"
                                    Image.ThumbnailUrl = "Images\noimage_new.png"
                                    rigProdImg.Items.Add(Image)
                                End If
                            Next
                        Catch
                            Image.ImageUrl = "Images\noimage_new.png"
                            Image.ThumbnailUrl = "Images\noimage_new.png"
                            rigProdImg.Items.Add(Image)
                        End Try
                    Else
                        rigProdImg.Visible = False
                        imagecontent_close.Style.Add("display", "none")
                        prod_content_fullview.Attributes.Add("class", "Prod-details-withoutImg")
                    End If
                    Dim AttrTable As String = "<table>"
                    Dim bIsAttribsHere As Boolean = False
                    Dim strAttrName As String = Convert.ToString(dtSearchResults.Rows(iRowIndex)("AttributeName"))
                    If Not String.IsNullOrEmpty(strAttrName) Then
                        Dim AttrName As String() = Convert.ToString(dtSearchResults.Rows(iRowIndex)("AttributeName")).TrimEnd("|").Split("|")
                        Dim AttrValue As String() = Convert.ToString(dtSearchResults.Rows(iRowIndex)("AttributeValue")).TrimEnd("|").Split("|")
                        Try
                            If AttrName.Length > 0 Then
                                bIsAttribsHere = True
                                For i As Integer = 0 To AttrName.Length - 1
                                    Dim AttValue As String() = AttrValue(i).Split(" ")
                                    Dim val As String = Convert.ToDouble(AttValue(0)).ToString("####,###,##0.00")
                                    AttrValue(i) = val + " " + AttValue(1)
                                    AttrTable += "<tr> <td>" & AttrName(i).Trim() & "</td> <td>:</td> <td>" & AttrValue(i).Trim() & "</td> </tr>"
                                Next
                                AttrTable += "</table>"
                                PlaceHolder1.Controls.Add(New LiteralControl(AttrTable))
                            End If  '  If AttrName.Length > 0 Then
                        Catch ex As Exception

                        End Try
                    End If

                    ' End here
                    If String.IsNullOrEmpty(Request.QueryString("ItemAttributeId")) Then
                        If Session("USESOLRSEARCH") = "Y" Then
                            'do nothing
                        Else
                            'Unilog code
                            If Not bIsAttribsHere Then

                                Dim results = JObject.Parse(response)
                                Dim obj As Object = results("data")("rows")(iRowIndex)
                                Dim Attr As String = obj.ToString().TrimStart("{").TrimEnd("}").Trim()
                                Dim AttrArray As String() = Attr.Split(",")
                                Dim Attribute As String()
                                For i As Integer = 0 To AttrArray.Length - 1
                                    If Trim(AttrArray(i)) <> "" Then
                                        Try
                                            AttrArray(i) = Trim(AttrArray(i))
                                            If Len(AttrArray(i)) > 1 Then
                                                If AttrArray(i).Trim().Remove(0, 1).Substring(0, 1) = "_" Then
                                                    Attribute = AttrArray(i).Trim().Split(":")
                                                    If Attribute.Length > 1 Then
                                                        If Not Attribute(0).Trim().Replace("""", "").Replace("_", "").Trim() = "Manufacturer Name" And Not Attribute(0).Trim().Replace("""", "").Replace("_", "").Trim() = "Manufacturer Part Number" Then
                                                            AttrTable += "<tr> <td>" & Attribute(0).Trim().Replace("""", "").Replace("_", "") & "</td> <td>:</td> <td>" & Attribute(1).Replace("[", "").Replace("]", "").Trim().Replace("""", "") & "</td> </tr>"
                                                        End If
                                                    End If  '  If Attribute.Length > 1 Then
                                                End If  '  If AttrArray(i).Trim().Remove(0, 1).Substring(0, 1) = "_" Then
                                            End If  '  If Len(AttrArray(i)) > 1 Then
                                        Catch ex As Exception

                                        End Try
                                    End If  '  If Trim(AttrArray(i)) <> "" Then
                                Next  '  For i As Integer = 0 To AttrArray.Length - 1
                                AttrTable += "</table>"
                                PlaceHolder1.Controls.Add(New LiteralControl(AttrTable))

                            End If  '  If Not bIsAttribsHere Then

                        End If
                        If Session("Noncat") = "Noncat" Then
                            lblpricetext.Text = Session("Price")
                            lblCurrencyCode.Text = Session("lblItemCurrencyCode")
                            lblCategory.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("UNSPSCCategory"))
                            ''  lblQOH.Text = Session("QtyOnHand")
                            setQuantiy(Session("QtyOnHand"))
                            lblDisclaimer.Text = ""
                        Else
                            Dim ds As DataSet
                            ds = GetDataFromSDiExPrice(itemId)
                            If Not ds Is Nothing And ds.Tables.Count > 0 And ds.Tables(0).Rows.Count > 0 Then
                                If ds.Tables(0).Rows(0).Item("PRICE") > 0 Then
                                    Dim price As String = ds.Tables(0).Rows(0).Item("PRICE")
                                    lblpricetext.Text = String.Format("{0:####,###,##0.00}", CType(price, Decimal))

                                    ' got another field from table SYSADM8.PS_ISA_SDIEX_PRCBU: FIRM_PRICE_FLG - in STAR only - VR 08/07/2018
                                    lblDisclaimer.Visible = False
                                    If UCase(Trim(ds.Tables(0).Rows(0).Item("FIRM_PRICE_FLG"))) = "N" Then
                                        'lblpricetext.BackColor = Yellow
                                        lblDisclaimer.Visible = True
                                    End If
                                    lblCurrencyCode.Text = ds.Tables(0).Rows(0).Item("CURRENCY_CD")
                                Else
                                    lblpricetext.Text = "Price on Request"
                                    lblCurrencyCode.Text = String.Empty
                                End If
                            Else
                                lblpricetext.Text = "Price on Request"
                                lblCurrencyCode.Text = String.Empty
                            End If
                            If lblpricetext.Text = "Price on Request" Then
                                lblDisclaimer.Visible = False
                            End If
                            strError += " -1- " & lblproductidtext.Text & "  ;  "  ' do not delete! tracing run-time error area
                            GetQOHNew(lblproductidtext.Text, strbu)
                            strError += " -2- " ' do not delete! tracing run-time error area
                            buildheaderdisplay(lblproductidtext.Text, "Item Detail")
                            strError += " -3- " ' do not delete! tracing run-time error area
                            panelSTK.Style.Add("Style", "Z-INDEX: 103; LEFT: 184px; POSITION: absolute; TOP: 208px")
                            strError += " -4- " & vbCrLf ' do not delete! tracing run-time error area
                            ' End Here
                        End If
                    Else
                        If Session("Noncat") = "Noncat" Then
                            lblpricetext.Text = Session("Price")
                            lblCurrencyCode.Text = Session("lblItemCurrencyCode")
                            lblCategory.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("UNSPSCCategory"))
                            ''  lblQOH.Text = Session("QtyOnHand")
                            setQuantiy(Session("QtyOnHand"))
                            lblDisclaimer.Text = ""
                        Else
                            lblpricetext.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("ItemPrice"))
                            lblCurrencyCode.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("CurrencyCd"))
                            lblCategory.Text = Convert.ToString(dtSearchResults.Rows(iRowIndex)("UNSPSCCategory"))
                            Dim strQOH As String = Convert.ToString(dtSearchResults.Rows(iRowIndex)("CustomColQOH"))
                            setQuantiy(strQOH)
                        End If
                    End If

                End If


            End If
        Catch ex As Exception
            Dim sMyErr1 As String = ex.ToString
            Dim bSendEmail As Boolean = True
            If sMyErr1.Length > 0 Then
                Try
                    If sMyErr1.Contains("Thread was being aborted") Then
                        bSendEmail = False
                    End If
                Catch ex21 As Exception

                End Try
            End If
            If bSendEmail Then
                Try
                    strError += " BU: " & strbu & "  User: " & Session("USERID") & vbCrLf & "  Site Prefix: "
                    If Not Session("SITEPREFIX") Is Nothing Then
                        Try
                            strError += Session("SITEPREFIX")
                        Catch ex2 As Exception
                            strError += " - error retrieving Session variable 'SITEPREFIX' -"
                        End Try
                    End If
                Catch ex1 As Exception
                    strError += "  Error building this error message."
                End Try
                sendErrorEmail(ex.ToString & "  Check Connection String for permission problems", "NO", Request.ServerVariables("URL"), strError)
            End If
        End Try
    End Sub

#Region "itemDetail"

    Private Sub GetQOHNew(ByVal sItemID, ByVal strBU)
        Dim strSQLString As String
        Dim strProdID As String
        Dim strInvItemID As String
        If itemId.Substring(0, 3) = Session("SITEPREFIX") Then
            strInvItemID = itemId
        ElseIf sItemID.Substring(0, 3) = Session("SITEPREFIX") Then
            strInvItemID = sItemID
        Else
            strInvItemID = Session("SITEPREFIX") & sItemID
        End If
        Dim nQOH As Decimal = 0
        Dim strQOH As String = " "
        Dim rdr As OleDb.OleDbDataReader
        strSQLString = "SELECT  " & vbCrLf &
                     "  I.BUSINESS_UNIT" & vbCrLf &
                      " FROM PS_BU_ITEMS_INV I" & vbCrLf &
                      " WHERE I.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf
        rdr = ORDBData.GetReader(strSQLString)
        If Not (rdr Is Nothing) Then
            While rdr.Read
                Try
                    Dim sdiBU As String = CStr(rdr("BUSINESS_UNIT")).Substring(0, 2)
                    If sdiBU = "V0" Then
                        bCosign = True
                    Else
                        bCosign = False
                    End If
                Catch ex As Exception
                    bCosign = False
                End Try
            End While
        Else
            bCosign = False
        End If
        rdr.Close()
        Session("Cosigned") = bCosign
        lblQOHHead.Visible = True
        lblQOH.Visible = True
        setQuantiy(GetQtyOnHandFromSDiExPrice(itemId))
        'Dim itemQOH As Double = Convert.ToDouble(GetQtyOnHandFromSDiExPrice(itemId))
        'lblQOH.Text = Math.Round(itemQOH)

    End Sub

    Private Sub setQuantiy(strQty As String)

        Dim itemQOH As Double = 0
        Try
            'Based on the lead time the business days will be binded in the label for catalog items
            If IsNumeric(strQty) And Not Session("Noncat") = "Noncat" Then
                itemQOH = Convert.ToDouble(strQty)
                itemQOH = Math.Round(itemQOH)
                If itemQOH = 0 Then
                    lblQOH.Visible = False
                    lblQOH.Text = "0"
                    If lblQOH.Text = "0" Or lblQOH.Text = "N/A" Then
                        lblQOH.Text = ""
                        Dim leadTime As String = GetItemID(itemId)
                        If leadTime > "0" Then
                            lblQOHtext.Text = "Usually ships within " + leadTime + " days"
                            lblQOHtext.Visible = True
                            imgInfoIcon.Visible = False
                        End If
                    End If
                Else
                    lblQOH.Visible = True
                    lblQOH.Text = itemQOH
                    imgInfoIcon.Visible = False
                End If
            Else
                lblQOH.Visible = False
                lblQOH.Text = "0"
                lblQOHtext.Text = "Usually ships within 5 business days"
                lblQOHtext.Visible = True
                imgInfoIcon.Visible = False
            End If
        Catch ex As Exception

        End Try

    End Sub
    'Getting lead time for the particular item 
    Public Function GetItemID(ByVal sItemID As String) As String
        Dim strSQLString As String = ""
        Dim strItemCode As String = ""
        Dim strItemId As String = sItemID

        Try
            strSQLString = "SELECT STD_LEAD FROM PS_PURCH_ITEM_ATTR WHERE INV_ITEM_ID='" & strItemId & "'"
            strItemCode = ORDBData.GetScalar(strSQLString)

        Catch ex As Exception
            strItemCode = ""
        End Try
        Return strItemCode
    End Function


    Private Sub createItemXML()
        Dim strXMLstring As String = ""
        Dim strAgent As String = ""
        Dim strprice As String = ""
        Dim strQty As String = ""
        Dim dtShopCartTb2 As New DataTable
        dtShopCartTb2 = Session("Cart")
        Dim intItems As Integer = 0
        Dim NonCat As Integer = 0
        If Not dtShopCartTb2 Is Nothing Then
            For NonCat = 0 To dtShopCartTb2.Rows.Count - 1
                If Convert.ToString(dtShopCartTb2.Rows(NonCat).Item("itemid")).Length > 6 Then
                    If Convert.ToString(dtShopCartTb2.Rows(NonCat).Item("itemid")).Substring(0, 7) = "NONCAT-" Then
                        intItems = Convert.ToInt32(Convert.ToString(dtShopCartTb2.Rows(NonCat).Item("itemid")).Substring(7))
                    End If
                End If
            Next
        End If
        If Session("Noncat") = "Noncat" Then
            intItems += 1
            txtClientPartNumber.Text = "NONCAT-" & intItems
        End If

        strAgent = Session("USERID")
        strXMLstring = "<ShoppingCartInformation>"
        strXMLstring = strXMLstring & "<userName>"
        strXMLstring = strXMLstring & strAgent.ToUpper
        strXMLstring = strXMLstring & "</userName>"
        strQty = Convert.ToInt32(txtQty.Text).ToString()
        strXMLstring = strXMLstring & "<itemID>"
        strXMLstring = strXMLstring & txtClientPartNumber.Text
        strXMLstring = strXMLstring & "</itemID>"
        strXMLstring = strXMLstring & "<Quantity>"
        strXMLstring = strXMLstring & strQty
        strXMLstring = strXMLstring & "</Quantity>"
        strXMLstring = strXMLstring & "<itemDescription>"
        strXMLstring = strXMLstring & Replace(Replace(lblDescriptionText.Text, "&", "&amp;"), "'", "&apos;")
        strXMLstring = strXMLstring & "</itemDescription>"
        strXMLstring = strXMLstring & "<universalPartNumber>"
        strXMLstring = strXMLstring & txtuniversalpartnumber.Text
        strXMLstring = strXMLstring & "</universalPartNumber>"
        strXMLstring = strXMLstring & "<customerItemId>"
        strXMLstring = strXMLstring & lblproductidtext.Text
        strXMLstring = strXMLstring & "</customerItemId>"
        strXMLstring = strXMLstring & "<Image>"
        If rigProdImg.Visible Then
            strXMLstring = strXMLstring & DirectCast(rigProdImg.Items(0), Telerik.Web.UI.ImageGalleryItem).ImageUrl
        Else
            strXMLstring = strXMLstring & ""
        End If

        strXMLstring = strXMLstring & "</Image>"
        strXMLstring = strXMLstring & "<ClassID>"
        strXMLstring = strXMLstring & Request.QueryString("classid")
        strXMLstring = strXMLstring & "</ClassID>"
        If lblpricetext.Text.ToUpper = "PRICE ON REQUEST" Then
            strprice = "0.00"
        Else
            strprice = Convert.ToDecimal(lblpricetext.Text)
        End If
        strXMLstring = strXMLstring & "<price>"
        strXMLstring = strXMLstring & strprice
        strXMLstring = strXMLstring & "</price>"
        strXMLstring = strXMLstring & "<supplierId>"
        strXMLstring = strXMLstring & "</supplierId>"
        strXMLstring = strXMLstring & "<productViewID>"
        strXMLstring = strXMLstring & Session("PRODVIEW")
        strXMLstring = strXMLstring & "</productViewID>"
        strXMLstring = strXMLstring & "<manufacturer>"
        strXMLstring = strXMLstring & Replace(Replace(txtMFG.Text, "&", "&amp;"), "'", "&apos;")
        strXMLstring = strXMLstring & "</manufacturer>"
        strXMLstring = strXMLstring & "<manufacturerPartNumber>"
        strXMLstring = strXMLstring & Replace(Replace(txtMFGPartNumber.Text, "&", "&amp;"), "'", "&apos;")
        strXMLstring = strXMLstring & "</manufacturerPartNumber>"
        strXMLstring = strXMLstring & "<uom>"
        strXMLstring = strXMLstring & lblUOM.Text
        strXMLstring = strXMLstring & "</uom>"
        strXMLstring = strXMLstring & "<ItemUNSPSC>"
        strXMLstring = strXMLstring & lblUnspsc.Text
        strXMLstring = strXMLstring & "</ItemUNSPSC>"
        strXMLstring = strXMLstring & "</ShoppingCartInformation>"
        Session("xmldata") = strXMLstring
    End Sub

    Private Sub imgBtnAddToCart_Click(ByVal sender As System.Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles imgBtnAddToCart.Click

        Dim alertMsg As String
        Dim alertScript As String

        Try
            If Trim(txtQty.Text) = "" Then
                txtQty.Text = "0"
            End If
            Dim bMoreThanOne As Boolean = True
            'Try
            '    If IsNumeric(txtQty.Text) Then
            '        If CType(txtQty.Text, Decimal) < 1 Then
            '            bMoreThanOne = False
            '        Else
            '            bMoreThanOne = True
            '        End If
            '    End If
            'Catch ex As Exception
            '    bMoreThanOne = False
            'End Try
            If ((txtQty.Text = "Qty") OrElse (Not bMoreThanOne)) Then
                '' ltlAlert.Text = strMessage.Say("No items were added to your cart")
                alertMsg = String.Format("No items were added to your cart")
                alertScript = String.Format("<script type='text/javascript'>alert('{0}');</script>", alertMsg)
                ScriptManager.RegisterStartupScript(Page, Page.GetType, "Alert", alertScript, False)

            Else
                createItemXML()
                Session("xmlToUrl") = "shopredirect.aspx"
                Response.Redirect("XMLPost.aspx")
            End If
        Catch ex As Exception

        End Try

    End Sub

    Public Shared Function GetCatalogPath(ByVal UNSPSC As String) As String
        Try
            Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
            Dim strInvItemID As String = String.Empty
            Dim StrDesc As String = String.Empty
            Dim ItmDesc As String = String.Empty
            Dim DescColumn As String = "DISPLAY_DESC"
            If currentApp.Session("SITELANG").ToString().ToUpper() = "S" Then
                DescColumn = "DISPLAY_DESC_SPANISH"
            End If
            If Not String.IsNullOrEmpty(UNSPSC.Trim()) Then
                StrDesc = "Select ('SDI Catalog >' || S." & DescColumn & " || ' >' || F." & DescColumn & " || ' >' || C." & DescColumn & " || ' >' || M." & DescColumn & ") As Catalogpath From  Sdix_Unspsc_Segment S,Sdix_Unspsc_Family F," & vbCrLf & _
                                        "SDIX_UNSPSC_CLASS C,SDIX_UNSPSC_COMMODITY M Where S.SEGMENTID= SUBSTR(F.FAMILYID,0,2) AND F.FAMILYID= SUBSTR(C.CLASSID,0,4) AND C.CLASSID=SUBSTR(M.COMMODITYID,0,6)" & vbCrLf & _
                                        "AND M.COMMODITYID=" & UNSPSC
                Dim objReader As OleDbDataReader = ORDBData.GetReader(StrDesc)
                If objReader.Read() Then
                    ItmDesc = objReader.Item("Catalogpath")
                End If
                objReader.Close()
            End If
            Return ItmDesc
        Catch ex As Exception
        End Try
    End Function
#End Region

#Region "ItemInventory"

    Private Sub BuildDetailDisplay(ByVal sItemID As String, ByVal strBU As String)
        Dim strSQLString As String
        Dim strProdID As String
        Dim strInvItemID As String
        strInvItemID = BuildCustPartNumWithPrefix(sItemID)

        PanelHDR.Visible = True
        panelSTK.Visible = True
        panelInv.Visible = True
        PanelUse.Visible = False
        panelIss.Visible = False
        PanelSrc.Visible = False
        PanelLoc.Visible = False
        panelNo.Visible = False

        strSQLString = "SELECT B.INV_ITEM_ID," & vbCrLf & _
        " B11.ISA_CUST_CHARGE_CD," & vbCrLf & _
        " C.ISA_SET_CHARGE_CD," & vbCrLf & _
        " C.ISA_ETAG_CODE," & vbCrLf & _
        " C.ISA_PDS_CODE," & vbCrLf & _
        " C.ISA_MACHINE_NO," & vbCrLf & _
        " C.ISA_DRAWING_CODE," & vbCrLf & _
        " E.INSPECT_CD," & vbCrLf & _
        " F.HAZ_CLASS_CD, " & vbCrLf & _
        " E.STD_LEAD " & vbCrLf & _
        " FROM PS_MASTER_ITEM_TBL B, PS_ISA_MASTER_ITEM B11, " & vbCrLf & _
        " PS_ISA_SET_CHRG_CD C," & vbCrLf & _
        " PS_ITEM_MFG D," & vbCrLf & _
        " PS_PURCH_ITEM_ATTR E," & vbCrLf & _
        " PS_INV_ITEMS F" & vbCrLf & _
        " WHERE B.SETID = 'MAIN1'" & vbCrLf & _
        " AND B.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
        " AND B.SETID = C.SETID(+)" & vbCrLf & _
        " AND B.INV_ITEM_ID = C.INV_ITEM_ID(+)" & vbCrLf & _
        " AND '" & Session("CUSTID") & "' = C.CUST_ID(+)" & vbCrLf & _
        " AND B.SETID = D.SETID(+)" & vbCrLf & _
        " AND B.INV_ITEM_ID = D.INV_ITEM_ID(+)" & vbCrLf & _
        " AND B.SETID = B11.SETID(+)" & vbCrLf & _
        " AND B.INV_ITEM_ID = B11.INV_ITEM_ID(+)" & vbCrLf & _
        " AND 'Y' = D.PREFERRED_MFG(+)" & vbCrLf & _
        " AND B.SETID = E.SETID(+)" & vbCrLf & _
        " AND B.INV_ITEM_ID = E.INV_ITEM_ID(+)" & vbCrLf & _
        " AND B.SETID = F.SETID(+)" & vbCrLf & _
        " AND B.INV_ITEM_ID = F.INV_ITEM_ID(+)" & vbCrLf & _
        " AND F.EFFDT = " & vbCrLf & _
        "		(SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf & _
        "      WHERE F.SETID = A_ED.SETID" & vbCrLf & _
        "      AND F.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
        "      AND A_ED.EFFDT <= SYSDATE)" & vbCrLf
        Dim dtrDetailReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
        If (dtrDetailReader.Read()) Then
            strProdID = dtrDetailReader.Item("INV_ITEM_ID")
            If Not IsDBNull(dtrDetailReader.Item("ISA_CUST_CHARGE_CD")) Then
                lblIssueChgCode.Text = dtrDetailReader.Item("ISA_CUST_CHARGE_CD")
            End If
            If Not IsDBNull(dtrDetailReader.Item("ISA_SET_CHARGE_CD")) Then
                lblFirstChgCode.Text = dtrDetailReader.Item("ISA_SET_CHARGE_CD")
            End If
            If Not dtrDetailReader.Item("INSPECT_CD") Is Nothing Then
                Try
                    If dtrDetailReader.Item("INSPECT_CD") = "Y" Then
                        lblInspectReq.Text = "YES"
                        lblRegrind.Text = "YES"
                    Else
                        lblInspectReq.Text = "NO"
                        lblRegrind.Text = "NO"
                    End If
                Catch ex As Exception
                    lblInspectReq.Text = "NO"
                    lblRegrind.Text = "NO"
                End Try
            Else
                lblInspectReq.Text = "NO"
                lblRegrind.Text = "NO"
            End If
            If Not IsDBNull(dtrDetailReader.Item("HAZ_CLASS_CD")) Then
                lblHazardCd.Text = dtrDetailReader.Item("HAZ_CLASS_CD")
            End If
            If Not IsDBNull(dtrDetailReader.Item("ISA_ETAG_CODE")) Then
                lblETAG.Text = dtrDetailReader.Item("ISA_ETAG_CODE")
            End If
            If Not IsDBNull(dtrDetailReader.Item("ISA_PDS_CODE")) Then
                lblPDS.Text = dtrDetailReader.Item("ISA_PDS_CODE")
            End If
            If Not IsDBNull(dtrDetailReader.Item("ISA_DRAWING_CODE")) Then
                lblDrawingCD.Text = dtrDetailReader.Item("ISA_DRAWING_CODE")
            End If
            If Not IsDBNull(dtrDetailReader.Item("ISA_MACHINE_NO")) Then
                lblMacCtlNum.Text = dtrDetailReader.Item("ISA_MACHINE_NO")
            End If
            lblCrtSpr.Text = GetCSData(strBU)
            lblLeadtime.Text = dtrDetailReader.Item("STD_LEAD") '20160227 Ticket 106368 Yury
            lblMachineNum.Text = getmachineNum(strInvItemID)
            dtrDetailReader.Close()
            panelNo.Visible = False
        Else
            dtrDetailReader.Close()
            panelSTK.Visible = True
            PanelUse.Visible = False
            panelIss.Visible = False
            PanelSrc.Visible = False
            PanelLoc.Visible = False
            panelNo.Visible = True
            lblErrorMsg.Text = "No detail data available for this item."

        End If

        'xxxxxx
        '  business Rules for Ascends Items A66
        '             We are getting qty on hand and bin location from the table sysadm.ps_isa_oro_ioh
        '             if the item is not in sysadm.ps_isa_oro_ioh then qty-on hand = 'ORO'
        '             if there is an item the type can be ORO, STK - show detail
        '             if the item is in sysadm.ps_isa_oro_ioh then qty-on hand = the number in that field
        '             the record comes back as a 'V' business Unit - which means it is a co-signment item then we by pass this table and treat like 
        '             a regular item in the catalog.
        '             Dim strQOH As String
        Dim nQOH As Decimal = 0
        Dim strQOH As String = " "
        Dim rdr As OleDb.OleDbDataReader
        strSQLString = "SELECT  " & vbCrLf & _
                     "  I.BUSINESS_UNIT" & vbCrLf & _
                      " FROM PS_BU_ITEMS_INV I" & vbCrLf & _
                      " WHERE I.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf
        rdr = ORDBData.GetReader(strSQLString)
        If Not (rdr Is Nothing) Then
            While rdr.Read
                Try
                    Dim sdiBU As String = CStr(rdr("BUSINESS_UNIT")).Substring(0, 2)
                    If sdiBU = "V0" Then
                        bCosign = True
                    Else
                        bCosign = False
                    End If
                Catch ex As Exception
                    bCosign = False
                End Try
            End While
        Else
            bCosign = False
        End If
        rdr.Close()
        Session("Cosigned") = bCosign
        Dim objEnterprise As New clsEnterprise(strBU)
        Dim ted As Boolean
        If (objEnterprise.IOH = "Y" And bCosign) Then
            ' if it is a from ioh table but it is a stock cosign item calculate the normal way
            ted = True
        Else
            ted = False
        End If

        Dim dtrBUItemsReader As OleDbDataReader
        If objEnterprise.IOH <> "Y" Or ted Then
            strSQLString = "SELECT I.BUSINESS_UNIT," & vbCrLf & _
                       " I.NO_REPLENISH_FLG," & vbCrLf & _
                       " I.INV_ITEM_ID," & vbCrLf & _
                       " trunc(I.REORDER_POINT,0) as REORDER_POINT," & vbCrLf & _
                       " trunc(I.QTY_MAXIMUM,0) as QTY_MAXIMUM," & vbCrLf & _
                       " trunc(I.QTY_AVAILABLE,0) as QTY_ONHAND" & vbCrLf & _
                       " FROM PS_BU_ITEMS_INV I" & vbCrLf & _
                       " WHERE I.INV_ITEM_ID = '" & strProdID & "'" & vbCrLf & _
                       " AND SUBSTR(I.BUSINESS_UNIT,3) = '" & Right(strBU, 3) & "'" & vbCrLf
        Else
            strSQLString = "SELECT I.BUSINESS_UNIT," & vbCrLf & _
                                  " 'NA' as  NO_REPLENISH_FLG," & vbCrLf & _
                                  " I.INV_ITEM_ID," & vbCrLf & _
                                  " 'NA' as REORDER_POINT," & vbCrLf & _
                                  " 'NA' as QTY_MAXIMUM," & vbCrLf & _
                                  " trunc(I.QTY_ONHAND,0) as QTY_ONHAND" & vbCrLf & _
                                  " FROM sysadm8.ps_isa_oro_ioh I" & vbCrLf & _
                                  " WHERE I.INV_ITEM_ID = '" & strProdID & "'" & vbCrLf & _
                                  " AND SUBSTR(I.BUSINESS_UNIT,3) = '" & Right(strBU, 3) & "'" & vbCrLf

        End If
        dtrBUItemsReader = ORDBData.GetReader(strSQLString)
        BuItemsGrid.DataSource = dtrBUItemsReader
        BuItemsGrid.DataBind()
        dtrBUItemsReader.Close()

    End Sub

    Private Sub buildheaderdisplay(ByVal strInvItemID, ByVal strPage)
        Try
            strInvItemID = BuildCustPartNumWithPrefix(strInvItemID)
            Dim strSQLString As String
            'Dim strProdID As String           
            strSQLString = "SELECT " & vbCrLf & _
                " E.IM_MODIFIER," & vbCrLf & _
                 " E.IM_NOUN," & vbCrLf & _
                 " B.CATEGORY_ID," & vbCrLf & _
                 " B.INV_ITEM_GROUP," & vbCrLf & _
                " D.INV_ITEM_TYPE " & vbCrLf & _
                " FROM PS_MASTER_ITEM_TBL B," & vbCrLf & _
                " PS_ITEM_MFG C," & vbCrLf & _
                " PS_INV_ITEMS D," & vbCrLf & _
                " PS_ISA_MASTER_ITEM E" & _
                " WHERE B.SETID = 'MAIN1'" & vbCrLf & _
                " AND B.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                " AND B.SETID = C.SETID(+)" & vbCrLf & _
                " AND B.INV_ITEM_ID = C.INV_ITEM_ID(+)" & vbCrLf & _
                " AND 'Y' = C.PREFERRED_MFG(+)" & vbCrLf & _
                " AND B.INV_ITEM_ID = D.INV_ITEM_ID" & vbCrLf & _
                " AND B.INV_ITEM_ID=E.INV_ITEM_ID AND B.SETID=E.SETID" & _
                " AND D.EFFDT =" & vbCrLf & _
                    "	(SELECT MAX(D_ED.EFFDT) FROM PS_INV_ITEMS D_ED" & vbCrLf & _
                "   WHERE D.SETID = D_ED.SETID" & vbCrLf & _
                "   AND D.INV_ITEM_ID = D_ED.INV_ITEM_ID" & vbCrLf & _
                "   AND D_ED.EFFDT <= SYSDATE)" & vbCrLf
            Dim s As String = ""
            Dim dtrDetailReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
            If (dtrDetailReader.Read()) Then
                If Not IsDBNull(dtrDetailReader.Item("IM_NOUN")) Then
                    If dtrDetailReader.Item("IM_NOUN").ToString <> "NO NOUN" Then
                        lblNoun.Text = dtrDetailReader.Item("IM_NOUN")
                        lblNounHdr.Visible = True
                    Else
                        lblNounHdr.Visible = False
                        lblNoun.Text = ""
                    End If
                End If
                If Not IsDBNull(dtrDetailReader.Item("IM_MODIFIER")) Then
                    If dtrDetailReader.Item("IM_MODIFIER").ToString <> "NO MODIFIER" Then
                        lblModifier.Text = dtrDetailReader.Item("IM_MODIFIER")
                        lblModifierHdr.Visible = True
                    Else
                        lblModifier.Text = ""
                        lblModifierHdr.Visible = False
                    End If
                End If
                If Not IsDBNull(dtrDetailReader.Item("CATEGORY_ID")) Then
                    lblCategory.Text = dtrDetailReader.Item("CATEGORY_ID")
                End If
                If Not IsDBNull(dtrDetailReader.Item("INV_ITEM_GROUP")) Then
                    lblItmGroup.Text = dtrDetailReader.Item("INV_ITEM_GROUP")
                End If
                If Not IsDBNull(dtrDetailReader.Item("INV_ITEM_TYPE")) Then
                    lblItmTpe1.Text = dtrDetailReader.Item("INV_ITEM_TYPE")
                End If
                panelNo.Visible = False
            Else
                panelNo.Visible = True
                lblErrorMsg.Text = "No detail data available for this item."
                lblSisterSiteError.Visible = True
            End If
            dtrDetailReader.Close()
        Catch ex As Exception

        End Try
    End Sub

    Public Function getmachineNum(ByVal strMachNum) As String
        Try
            Dim I As Integer
            Dim strSQLString As String = "SELECT ISA_MACHINE_NO" & vbCrLf & _
                " FROM PS_ISA_MACHINE_NBR" & vbCrLf & _
                " WHERE setid = 'MAIN1'" & vbCrLf & _
                " AND inv_item_id = '" & strMachNum & "'"

            Dim dsMachNum As DataSet = ORDBData.GetAdapter(strSQLString)
            getmachineNum = ""
            If dsMachNum.Tables(0).Rows.Count > 0 Then
                For I = 0 To dsMachNum.Tables(0).Rows.Count - 1
                    If I > 0 Then
                        getmachineNum = getmachineNum & ","
                    End If
                    getmachineNum = getmachineNum & dsMachNum.Tables(0).Rows(I).Item(0)
                Next
            End If
            Return getmachineNum
        Catch ex As Exception

        End Try
    End Function
    Public Function GetOwnedBy(ByVal ownedby As Object) As String
        Try
            Select Case Left(ownedby, 1)
                Case "I", "B", "V"
                    Return ("SDI (" & ownedby & ")")
                Case Else
                    Return ("Customer (" & ownedby & ")")
            End Select
        Catch ex As Exception
            Return ("Customer ")
        End Try
        
    End Function

    Public Function GetReplenish(ByVal replenish As Object) As String
        Try
            If replenish = "Y" Then
                Return "NO"
            Else
                Return "YES"
            End If
        Catch ex As Exception
            Return "YES"
        End Try
        
    End Function

    ''' <summary>
    ''' To get if the item is CS or Not
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetCSData(ByVal Bu As String) As String
        Dim strSQLString As String = ""
        Dim strUtilizCode As String = ""
        Dim strisCriticalSpare As String = ""
        Dim strInvItemID As String
        strInvItemID = BuildCustPartNumWithPrefix(lblproductidtext.Text.Trim)
        Try
            strSQLString = "SELECT UTILIZ_CD FROM PS_BU_ITEM_UTIL_CD WHERE SUBSTR(BUSINESS_UNIT,3) = '" & Right(Bu, 3) & "' AND INV_ITEM_ID='" & strInvItemID & "'"
            strUtilizCode = ORDBData.GetScalar(strSQLString)
            If strUtilizCode.Trim = "CS" Then
                '' lblCrtSpr.Text = "YES"
                strisCriticalSpare = "YES"
            Else
                '' lblCrtSpr.Text = "NO"
                strisCriticalSpare = "NO"
            End If
        Catch ex As Exception
            '' lblCrtSpr.Text = "NO"
            strisCriticalSpare = "NO"
        End Try
        Return strisCriticalSpare
    End Function


#End Region

#Region "ItemUsage"

    Private Sub buildusagedisplay(ByVal sitemid As String)
        Try
            Dim strBU As String
            Dim strInvItemID As String
            strInvItemID = BuildCustPartNumWithPrefix(sitemid)
            strBU = Session("BUSUNIT")
            'buildheaderdisplay(strInvItemID, "Item Usage")
            If panelNo.Visible = True Then
                lblErrorMsg.Text = "No detail data available for this item."
                Exit Sub
            Else
                panelSTK.Visible = True
                panelInv.Visible = False
                PanelUse.Visible = True
                panelIss.Visible = False
                PanelSrc.Visible = False
                PanelLoc.Visible = False
                panelNo.Visible = False
            End If
            Dim arrDateUsage(23)
            Dim arrItemUsage(23)
            Dim sMonNow As String
            Dim sYearNow As String
            Dim intCounter As Integer
            Dim iCYear As Integer
            Dim iPYear As Integer
            Dim dtCurrent As DataTable
            Dim dtPrevious As DataTable
            Dim dr As DataRow
            Dim iItemCnt As Integer
            Dim bolReader As Boolean
            sMonNow = Month(Now())
            sYearNow = Year(Now())
            Dim strSQLstring As String = "SELECT U.MONTH, U.YEAR," & vbCrLf & _
                    " U.USAGE as Quantity" & vbCrLf & _
                    " FROM PS_ISA_MON_USE_BU U" & vbCrLf & _
                    " WHERE U.SOURCE_BUS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND U.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                    " AND U.YEAR > (U.YEAR - 2)" & vbCrLf & _
                    " ORDER BY U.YEAR DESC, U.MONTH DESC" & vbCrLf

            Dim dtrGridReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
            bolReader = dtrGridReader.Read()
            For intCounter = 0 To 23
                If sMonNow = 0 Then
                    sMonNow = 12
                    sYearNow = sYearNow - 1
                End If
                arrDateUsage(intCounter) = sMonNow & "/" & sYearNow
                If bolReader = "true" Then
                    If sMonNow = CInt(dtrGridReader.Item(0)) And sYearNow = CInt(dtrGridReader.Item(1)) Then
                        arrItemUsage(intCounter) = dtrGridReader.Item(2)
                        bolReader = dtrGridReader.Read()
                    Else
                        arrItemUsage(intCounter) = 0
                    End If
                Else
                    arrItemUsage(intCounter) = 0
                End If
                sMonNow = sMonNow - 1
            Next
            dtrGridReader.Close()
            dtCurrent = New DataTable
            dtCurrent.Columns.Add("Current Year")
            dtCurrent.Columns.Add("Quantity")
            iItemCnt = 0
            For intCounter = 0 To 11
                dr = dtCurrent.NewRow()
                dr(0) = arrDateUsage(intCounter)
                dr(1) = arrItemUsage(intCounter)
                dtCurrent.Rows.Add(dr)
                iItemCnt = iItemCnt + arrItemUsage(intCounter)
            Next
            dr = dtCurrent.NewRow()
            dr(0) = "Total"
            dr(1) = iItemCnt
            dtCurrent.Rows.Add(dr)
            DataGrid1.DataSource = dtCurrent
            DataGrid1.DataBind()
            dtPrevious = New DataTable
            dtPrevious.Columns.Add("Previous Year")
            dtPrevious.Columns.Add("Quantity")
            iItemCnt = 0
            For intCounter = 12 To 23
                dr = dtPrevious.NewRow()
                dr(0) = arrDateUsage(intCounter)
                dr(1) = arrItemUsage(intCounter)
                dtPrevious.Rows.Add(dr)
                iItemCnt = iItemCnt + arrItemUsage(intCounter)
            Next
            dr = dtPrevious.NewRow()
            dr(0) = "Total"
            dr(1) = iItemCnt
            dtPrevious.Rows.Add(dr)
            DataGrid2.DataSource = dtPrevious
            DataGrid2.DataBind()
        Catch ex As Exception

        End Try

    End Sub

#End Region

#Region "ItemIssues"

    Private Sub buildissuedisplay(ByVal sItemID As String)
        Try
            Dim strSQLString As String
            Dim strBU As String
            Dim strInvItemID As String
            strInvItemID = BuildCustPartNumWithPrefix(sItemID)
            strBU = Session("BUSUNIT")
            ' buildheaderdisplay(strInvItemID, "Item Issues")
            If panelNo.Visible = True Then
                lblErrorMsg.Text = "No detail data available for this item."
                Exit Sub
            Else
                panelSTK.Visible = True
                panelInv.Visible = False
                PanelUse.Visible = False
                panelIss.Visible = True
                PanelSrc.Visible = False
                PanelLoc.Visible = False
                panelNo.Visible = False
            End If
            strSQLString = "" & _
                "SELECT TO_CHAR(H.TRANSACTION_DATE,'YYYY-MM-DD') TranDate," & vbCrLf & _
                " 'ISSUE' as Type," & vbCrLf & _
                " H.QTY," & vbCrLf & _
                " I.ISA_EMPLOYEE_ID as EmpID, I.ISA_WORK_ORDER_NO AS Work_Order, " & vbCrLf & _
                " J.ISA_EMPLOYEE_NAME as Name," & vbCrLf & _
                " I.ISA_CUST_CHARGE_CD as ChargeCode" & vbCrLf & _
                " FROM PS_TRANSACTION_INV H," & vbCrLf & _
                " SYSADM8.PS_ISA_ORD_LINE I," & vbCrLf & _
                " PS_ISA_EMPL_TBL J" & vbCrLf & _
                " WHERE H.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                " AND H.TRANSACTION_GROUP = '030'" & vbCrLf & _
                " AND H.SOURCE_BUS_UNIT = '" & strBU & "'" & vbCrLf & _
                " AND H.SOURCE_BUS_UNIT = I.BUSINESS_UNIT" & vbCrLf & _
                " AND H.ORDER_NO = I.ORDER_NO" & vbCrLf & _
                " AND H.ORDER_INT_LINE_NO = I.ORDER_INT_LINE_NO" & vbCrLf & _
                " AND H.TRANSACTION_DATE > (SYSDATE - 365)" & vbCrLf & _
                " AND I.BUSINESS_UNIT = J.BUSINESS_UNIT(+)" & vbCrLf & _
                " AND I.ISA_EMPLOYEE_ID = J.ISA_EMPLOYEE_ID(+)" & vbCrLf & _
                "UNION ALL" & vbCrLf & _
                "SELECT TO_CHAR(K.DATETIME_ADDED,'YYYY-MM-DD') DATE_ADDED," & vbCrLf & _
                " 'RETURN'," & vbCrLf & _
                " K.QTY_RETURNED," & vbCrLf & _
                " O.ISA_EMPLOYEE_ID, O.ISA_WORK_ORDER_NO," & vbCrLf & _
                " L.ISA_EMPLOYEE_NAME," & vbCrLf & _
                " O.ISA_CUST_CHARGE_CD" & vbCrLf & _
                " FROM PS_RMA_LINE K," & vbCrLf & _
                " PS_ISA_EMPL_TBL L," & vbCrLf & _
                " SYSADM8.PS_ISA_ORD_LINE O" & vbCrLf & _
                " WHERE K.PROD_ID_ENTERED = '" & strInvItemID & "'" & vbCrLf & _
                " AND K.BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                " AND K.DATETIME_ADDED > (SYSDATE - 365)" & vbCrLf & _
                " AND K.BUSINESS_UNIT = O.BUSINESS_UNIT(+)" & vbCrLf & _
                " AND K.ORDER_NO = O.ORDER_NO(+) " & vbCrLf & _
                " AND K.ORDER_INT_LINE_NO = O.ORDER_INT_LINE_NO(+) " & vbCrLf & _
                " AND O.BUSINESS_UNIT = L.BUSINESS_UNIT(+)" & vbCrLf & _
                " AND O.ISA_EMPLOYEE_ID = L.ISA_EMPLOYEE_ID(+) " & vbCrLf & _
                "ORDER BY 1 DESC" & vbCrLf

            Try
                Dim dtrIssuesReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
                dtgIssues.DataSource = dtrIssuesReader
                dtgIssues.DataBind()
                dtrIssuesReader.Close()
            Catch objException As Exception
                sendErrorEmail(objException.ToString, "NO", Request.ServerVariables("URL"), strSQLString)
                Response.Redirect("DBErrorPage.aspx?HOME=N")
            End Try
        Catch ex As Exception

        End Try
    End Sub

#End Region

#Region "ItemMultiSites"

    Private Sub buildmultisitedisplay(ByVal sItemID As String, Optional ByRef ErrorMsg As String = "")
        Try
            Dim strUnilogString As String
            Try
                Dim strInvItemID As String
                strInvItemID = BuildCustPartNumWithPrefix(sItemID)

                If panelNo.Visible = True Then
                    lblErrorMsg.Text = "No data available for this item."
                    Exit Sub
                End If
                Dim strSQLstring As String
                Dim iItemID As Integer = GetItemID(strInvItemID)
                If iItemID = 0 Then
                    lblErrorMsg.Text = "No data available for this item."
                    panelNo.Visible = True
                    Exit Sub
                End If
                strSQLstring = "SELECT ISA_COMPANY_ID" & vbCrLf & _
                       " FROM PS_ISA_ENTERPRISE" & vbCrLf & _
                       " WHERE ISA_BUSINESS_UNIT = '" & strbu & "'"
                Dim strCompanyid As String = ORDBData.GetScalar(strSQLstring)
                Dim I As Integer = 0
                'Dim strBusunits As String
                Dim dsSisterBu As DataSet = GetSisterBusinessUnits(Session("BUSUNIT"), True)
                If Not dsSisterBu Is Nothing Then
                    If dsSisterBu.Tables(0).Rows.Count > 0 Then
                        'For I = 0 To dsSisterBu.Tables(0).Rows.Count - 1
                        '    If I > 0 Then
                        '        strBusunits = strBusunits & ","
                        '    End If
                        '    strBusunits = strBusunits & "'" & CStr(dsSisterBu.Tables(0).Rows(I).Item("BUSINESS_UNIT")) & "'"
                        'Next
                    Else
                        lblSisterSiteError.Visible = True
                    End If
                Else
                    lblSisterSiteError.Visible = True
                End If

                strUnilogString = "select IP.Subset_ID As SITE_ID, IP.Customer_Part_Number AS INV_ITEM_ID, IP.Customer_Part_Number_WO_Prefix As invitemid " & vbCrLf & _
                                    "from Item_Prices IP" & vbCrLf & _
                                    "JOIN Item_Master IM ON IM.Item_ID=IP.Item_ID" & vbCrLf & _
                                    "where IM. ITEM_ID = " & iItemID
                Dim sisBu As String = ""
                If dsSisterBu.Tables(0).Rows.Count > 0 Then
                    For I = 0 To dsSisterBu.Tables(0).Rows.Count - 1
                        If I > 0 Then
                            sisBu = sisBu & ","
                        End If
                        'sisBu = sisBu & "'" & CStr(dsSisterBu.Tables(0).Rows(I).Item("BUSINESS_UNIT")).Substring(2, 3) & "'"
                        sisBu = sisBu & "'" & CStr(dsSisterBu.Tables(0).Rows(I).Item("BUSINESS_UNIT")).Substring(1, 4).TrimStart("0") & "'"
                    Next
                End If
                Dim strCplusDbSql As String = Session("BUSUNIT").ToString().Substring(Session("BUSUNIT").ToString.Length - 4).TrimStart("0")
                If Session("SDIEmp") = "CUST" Then

                    strUnilogString = strUnilogString & " AND IP.Subset_ID IN (" & Session("CplusDbSQL").ToString & ")"

                Else
                    If Len(Session("CplusDbSQL").ToString) > 3 Then
                        strUnilogString = strUnilogString & " AND IP.Subset_ID IN ('" & Session("CplusDbSQL").ToString & "')"
                    Else

                        If Trim(sisBu) <> "" Then
                            strUnilogString = strUnilogString & " AND IP.Subset_ID IN (" & sisBu & ")"
                        Else
                            strUnilogString = strUnilogString & " AND IP.Subset_ID IN ('" & strCplusDbSql & "')"
                        End If

                    End If

                End If
                Dim ds As DataSet = ORDBData.UnilogGetAdapter(strUnilogString)
                If ds.Tables(0).Rows.Count = 0 Then
                    lblErrorMsg.Text = "No data available for this item."
                    panelNo.Visible = True
                Else
                    panelSTK.Visible = True
                    panelInv.Visible = False
                    PanelUse.Visible = False
                    panelIss.Visible = False
                    PanelSrc.Visible = True
                    PanelLoc.Visible = False
                    panelNo.Visible = False
                End If
                Dim strSiteName As String = ""
                Dim strDescription As String = ""
                Dim decPrice As Decimal
                Dim dsCplus As DataSet

                ds.Tables(0).Columns.Add("SiteName")
                ds.Tables(0).Columns.Add("Description")
                ds.Tables(0).Columns.Add("Price")
                ds.Tables(0).Columns.Add("UOM")
                ds.Tables(0).Columns.Add("QOH")

                '' proposed new code - not finished yet
                'Dim strSiteId As String = ""
                'Dim bIsRegularSites As String = True
                'Try
                '    If ds.Tables(0).Rows.Count = 1 Then
                '        strSiteId = ds.Tables(0).Rows(0).Item("SITE_ID")
                '        If strSiteId.StartsWith("90") Then
                '            'this is sites with shared catalog
                '            bIsRegularSites = False
                '        End If
                '    End If
                'Catch ex As Exception
                '    bIsRegularSites = True
                'End Try
                'If bIsRegularSites Then

                '    For I = 0 To ds.Tables(0).Rows.Count - 1
                '        strSiteId = ds.Tables(0).Rows(I).Item("SITE_ID")
                '        If strSiteId.StartsWith("90") Then
                '            strSiteName = GetSiteName(strCplusDbSql, True)  '   & " (Shared Catlg: " & strSiteId & ")"
                '        Else
                '            strSiteName = GetSiteName(strSiteId, True)
                '        End If
                '        ds.Tables(0).Rows(I).Item("SiteName") = strSiteName
                '        Dim dtUnilog As DataSet = getDtUnilogData(iItemID, ds.Tables(0).Rows(I).Item("SITE_ID"))
                '        If dtUnilog.Tables(0).Rows.Count = 0 Then
                '            ds.Tables(0).Rows(I).Item("Description") = ""
                '            ds.Tables(0).Rows(I).Item("Price") = "Price on Request"
                '        Else
                '            ds.Tables(0).Rows(I).Item("Description") = dtUnilog.Tables(0).Rows(0).Item("SHORT_DESC")
                '            ds.Tables(0).Rows(I).Item("UOM") = dtUnilog.Tables(0).Rows(0).Item("SALES_UOM")
                '        End If
                '        If strSiteId.StartsWith("90") Then
                '            ds.Tables(0).Rows(I).Item("QOH") = getQOH(ds.Tables(0).Rows(I).Item("INV_ITEM_ID"), strCplusDbSql)
                '        Else
                '            ds.Tables(0).Rows(I).Item("QOH") = getQOH(ds.Tables(0).Rows(I).Item("INV_ITEM_ID"), strSiteId)
                '        End If
                '    Next

                'Else
                '    'this is sites with shared catalog

                '    '        waiting for Ben to give guidance how to process them

                'End If  '  If bIsRegularSites Then

                ' production code
                For I = 0 To ds.Tables(0).Rows.Count - 1
                    strSiteName = getSiteName(ds.Tables(0).Rows(I).Item("SITE_ID"))
                    ds.Tables(0).Rows(I).Item("SiteName") = strSiteName
                    Dim dtUnilog As DataSet = getDtUnilogData(iItemID, ds.Tables(0).Rows(I).Item("SITE_ID"))
                    If dtUnilog.Tables(0).Rows.Count = 0 Then
                        ds.Tables(0).Rows(I).Item("Description") = ""
                        ds.Tables(0).Rows(I).Item("Price") = "Price on Request"
                    Else
                        ds.Tables(0).Rows(I).Item("Description") = dtUnilog.Tables(0).Rows(0).Item("SHORT_DESC")
                        ds.Tables(0).Rows(I).Item("UOM") = dtUnilog.Tables(0).Rows(0).Item("SALES_UOM")
                    End If
                    ds.Tables(0).Rows(I).Item("QOH") = getQOH(ds.Tables(0).Rows(I).Item("INV_ITEM_ID"), ds.Tables(0).Rows(I).Item("SITE_ID"))
                Next
                ds.AcceptChanges()
                dtgMultisites.DataSource = ds
                dtgMultisites.DataBind()
            Catch ex As Exception
                Dim errMsg As String = ex.ToString()  '  Convert.ToString(ex.Message) & "-" & Convert.ToString(ex.InnerException)
                'If errMsg.Length > 49 Then
                '    errMsg = errMsg.Substring(0, 49)
                'End If
                ErrorMsg = "Related site data not available at this time due to Error: " & errMsg
                'Send911Email(ex, String.Empty, ErrorMsg, strUnilogString & " --- from ItemDetailNew.aspx.vb, buildmultisitedisplay ")
                SendSDiExchErrorMail(" --- From ItemDetailNew.aspx.vb, buildmultisitedisplay. " & ErrorMsg, "Error in SDiExchange: from ItemDetailNew.aspx.vb, buildmultisitedisplay")
                PanelSrc.Visible = False
                lblSisterSiteError.Visible = True
            End Try
        Catch ex As Exception
        End Try
    End Sub

    Private Function getDtUnilogData(ByVal intItemID As Integer, ByVal strSiteID As String) As DataSet
        Try
            Dim iSiteID As Integer = 0
            Dim ds As New DataSet
            If Integer.TryParse(strSiteID, iSiteID) Then
                Dim strUnilogString = _
                    "SELECT V.SALES_UOM, V.SHORT_DESC " & vbCrLf & _
                    " FROM SEARCH_ITEM_MASTER_VIEW_V2 V, " & vbCrLf &
                    "       ITEM_PRICES P " & vbCrLf & _
                    " WHERE P.item_id = " & intItemID & vbCrLf & _
                    " AND P.subset_id = " & Session("CplusDbSQL").ToString & vbCrLf & _
                    " AND P.item_id = V.item_id "

                ds = ORDBData.UnilogGetAdapter(strUnilogString)
            End If
            Return ds
        Catch ex As Exception
        End Try
    End Function

    Private Function getQOH(ByVal strinvitemid As String, ByVal strSite As String) As Integer
        ' chnaged from QOH to qty available on 2/1/07 per Eric W.
        Dim strSQLstring As String = "SELECT SUM(QTY_AVAILABLE) as QOH" & vbCrLf & _
                " FROM PS_BU_ITEMS_INV" & vbCrLf & _
                " WHERE BUSINESS_UNIT like '%" & strSite & "'" & vbCrLf & _
                " AND INV_ITEM_ID = '" & strinvitemid & "'"

        Dim strQOH As String

        strQOH = ORDBData.GetScalar(strSQLstring)

        If strQOH Is Nothing Or strQOH = "" Then
            getQOH = 0
        Else
            If IsNumeric(strQOH) Then
                getQOH = CType(strQOH, Decimal)
            Else
                getQOH = 0
            End If
        End If

    End Function

#End Region

#Region "ItemLocation"

    Private Sub buildLocationdisplay(ByVal sItemID As String)
        Try
            Dim strSiteID As String
            Dim strInvItemID As String

            strInvItemID = BuildCustPartNumWithPrefix(sItemID)
            strSiteID = Session("BUSUNIT").ToString.Substring(2, 3)
            ' buildheaderdisplay(strInvItemID, "Item Storeroom Location")

            If panelNo.Visible = True Then
                lblErrorMsg.Text = "No detail data available for this item."
                Exit Sub
            Else
                panelSTK.Visible = True
                panelInv.Visible = False
                PanelUse.Visible = False
                panelIss.Visible = False
                PanelSrc.Visible = False
                PanelLoc.Visible = True
                panelNo.Visible = False
            End If
            Dim strSQLString As String = ""

            ' according to JohnDG
            '   new items' location are in default_loc and will ONLY have Zero (0) qty in bu_items
            '   once item gets received, physical inv gets created and bu_items' qty gets incremented
            '   so to show available locations for the item, default_loc is also considered but we search where qty is zero so
            '   whatever qty in physical inv wins.
            '       - erwin

            ''' Ascend shit here xxxxxxxxxxx
            Dim objEnterprise As New clsEnterprise(strbu)
            Dim dtrBUItemsReader As OleDbDataReader
            Dim objclsInvItemID As New clsInvItemID(strInvItemID)
            'zzzzzpfd 

            'Code for displaying serially tracked items in BIN Location'
            Dim dsLocDataSet As DataSet
            Dim newQuery = "SELECT MST.LOT_CONTROL, MST.SERIAL_CONTROL" & vbCrLf & _
                                "FROM SYSADM8.PS_INV_ITEMS ITM, SYSADM8.PS_MASTER_ITEM_TBL MST" & vbCrLf & _
                                "WHERE(ITM.SETID = MST.SETID)" & vbCrLf & _
                                "AND ITM.INV_ITEM_ID = MST.INV_ITEM_ID" & vbCrLf & _
                                "AND ITM.INV_ITEM_ID = '" & strInvItemID & "'"

            Dim dsLotNumber As DataSet
            Dim lc As String = ""
            Dim sno As String = ""
            dsLotNumber = ORDBData.GetAdapter(newQuery)
            If dsLotNumber.Tables(0).Rows.Count > 0 Then
                lc = dsLotNumber.Tables(0).Rows(0)("LOT_CONTROL").ToString()
                sno = dsLotNumber.Tables(0).Rows(0)("SERIAL_CONTROL").ToString()
            End If
            Dim trackFlag = "Not Tracked"

            If (dsLotNumber.Tables(0).Rows.Count > 0) And (lc = "Y" Or sno = "Y") Then

                strSQLString = "select PINV.BUSINESS_UNIT, PINV.INV_LOT_ID, PINV.INV_ITEM_ID, PINV.SERIAL_ID, PINV.STORAGE_AREA, PINV.UNIT_OF_MEASURE, PINV.QTY, " & vbCrLf & _
                            "BINLOC.DESCR254, BINLOC.PRODUCT_ALIAS, PINV.STOR_LEVEL_1, PINV.STOR_LEVEL_2, PINV.STOR_LEVEL_3,PINV.STOR_LEVEL_4,A.descr" & vbCrLf & _
                            "FROM SYSADM8.PS_PHYSICAL_INV PINV," & vbCrLf & _
                            "ps_stor_area_inv A," & vbCrLf & _
                            "(SELECT" & vbCrLf & _
                              "B.INV_ITEM_ID" & vbCrLf & _
                             ",SUBSTR(B.INV_ITEM_ID,4) as PRODUCT_ALIAS " & vbCrLf & _
                             ",B.DESCR254 " & vbCrLf & _
                             ",C.BUSINESS_UNIT " & vbCrLf & _
                             ",C.STORAGE_AREA " & vbCrLf & _
                             ",C.STOR_LEVEL_1 " & vbCrLf & _
                             ",C.STOR_LEVEL_2 " & vbCrLf & _
                             ",C.STOR_LEVEL_3 " & vbCrLf & _
                             ",C.STOR_LEVEL_4 " & vbCrLf & _
                             ",C.QTY " & vbCrLf & _
                             ",C.UNIT_OF_MEASURE " & vbCrLf & _
                             "FROM  " & vbCrLf & _
                              "PS_INV_ITEMS B " & vbCrLf & _
                             ",(" & vbCrLf & _
                              "SELECT  " & vbCrLf & _
                               "LOC.BUSINESS_UNIT " & vbCrLf & _
                              ",LOC.INV_ITEM_ID " & vbCrLf & _
                              ",LOC.STORAGE_AREA " & vbCrLf & _
                              ",LOC.STOR_LEVEL_1 " & vbCrLf & _
                              ",LOC.STOR_LEVEL_2 " & vbCrLf & _
                              ",LOC.STOR_LEVEL_3 " & vbCrLf & _
                              ",LOC.STOR_LEVEL_4 " & vbCrLf & _
                              ",LOC.UNIT_OF_MEASURE " & vbCrLf & _
                              ",SUM(LOC.QTY) AS QTY " & vbCrLf & _
                              "FROM  " & vbCrLf & _
                              "( " & vbCrLf & _
                               "SELECT  " & vbCrLf & _
                                "C1.BUSINESS_UNIT " & vbCrLf & _
                               ",C1.INV_ITEM_ID " & vbCrLf & _
                               ",C1.STORAGE_AREA " & vbCrLf & _
                               ",C1.STOR_LEVEL_1 " & vbCrLf & _
                               ",C1.STOR_LEVEL_2 " & vbCrLf & _
                               ",C1.STOR_LEVEL_3 " & vbCrLf & _
                               ",C1.STOR_LEVEL_4 " & vbCrLf & _
                               ",C1.QTY " & vbCrLf & _
                               ",C1.UNIT_OF_MEASURE " & vbCrLf & _
                                "FROM PS_PHYSICAL_INV C1 " & vbCrLf & _
                                "WHERE C1.BUSINESS_UNIT = '" & strSiteID & "' " & vbCrLf & _
                                 "AND C1.INV_ITEM_ID = '" & strInvItemID & "' " & vbCrLf & _
                                 "AND C1.QTY > 0 " & vbCrLf & _
                               "UNION " & vbCrLf & _
                               "SELECT  " & vbCrLf & _
                                "C21.BUSINESS_UNIT " & vbCrLf & _
                               ",C21.INV_ITEM_ID " & vbCrLf & _
                               ",C21.STORAGE_AREA " & vbCrLf & _
                               ",C21.STOR_LEVEL_1 " & vbCrLf & _
                               ",C21.STOR_LEVEL_2 " & vbCrLf & _
                               ",C21.STOR_LEVEL_3 " & vbCrLf & _
                               ",C21.STOR_LEVEL_4 " & vbCrLf & _
                               ",C22.QTY_AVAILABLE AS QTY ,C22.STD_PACK_UOM AS UNIT_OF_MEASURE " & vbCrLf & _
                               "FROM  " & vbCrLf & _
                                "PS_DEFAULT_LOC_INV C21 " & vbCrLf & _
                               ",PS_BU_ITEMS_INV C22 " & vbCrLf & _
                               ",PS_PHYSICAL_INV C23 " & vbCrLf & _
                               "WHERE C21.BUSINESS_UNIT = '" & strSiteID & "' " & vbCrLf & _
                                 "AND C21.INV_ITEM_ID = '" & strInvItemID & "' " & vbCrLf & _
                                 "AND C21.BUSINESS_UNIT = C22.BUSINESS_UNIT " & vbCrLf & _
                                 "AND C21.INV_ITEM_ID = C22.INV_ITEM_ID " & vbCrLf & _
                                 "AND C21.BUSINESS_UNIT = C23.BUSINESS_UNIT (+) " & vbCrLf & _
                                 "AND C21.INV_ITEM_ID =   C23.INV_ITEM_ID (+) " & vbCrLf & _
                                 "AND C21.STORAGE_AREA =  C23.STORAGE_AREA(+) " & vbCrLf & _
                                 "AND C21.STOR_LEVEL_1 =  C23.STOR_LEVEL_1(+) " & vbCrLf & _
                                 "AND C21.STOR_LEVEL_2 =  C23.STOR_LEVEL_2(+) " & vbCrLf & _
                                 "AND C21.STOR_LEVEL_3 =  C23.STOR_LEVEL_3(+) " & vbCrLf & _
                                 "AND C21.STOR_LEVEL_4 =  C23.STOR_LEVEL_4(+) " & vbCrLf & _
                              ") LOC " & vbCrLf & _
                              "GROUP BY  " & vbCrLf & _
                               "LOC.BUSINESS_UNIT " & vbCrLf & _
                              ",LOC.INV_ITEM_ID " & vbCrLf & _
                              ",LOC.STORAGE_AREA " & vbCrLf & _
                              ",LOC.STOR_LEVEL_1 " & vbCrLf & _
                              ",LOC.STOR_LEVEL_2 " & vbCrLf & _
                              ",LOC.STOR_LEVEL_3 " & vbCrLf & _
                              ",LOC.STOR_LEVEL_4 " & vbCrLf & _
                              ",LOC.UNIT_OF_MEASURE " & vbCrLf & _
                              ") C " & vbCrLf & _
                             "WHERE B.INV_ITEM_ID = '" & strInvItemID & "' " & vbCrLf & _
                               "AND B.EFFDT = ( " & vbCrLf & _
                                              "SELECT MAX(B_ED.EFFDT)  " & vbCrLf & _
                                              "FROM PS_INV_ITEMS B_ED " & vbCrLf & _
                                              "WHERE B.SETID = B_ED.SETID " & vbCrLf & _
                                                "AND B.INV_ITEM_ID = B_ED.INV_ITEM_ID " & vbCrLf & _
                                                "AND B_ED.EFFDT <= SYSDATE " & vbCrLf & _
                                             ") " & vbCrLf & _
                               "AND B.INV_ITEM_ID = C.INV_ITEM_ID(+) " & vbCrLf & _
                               "AND C.BUSINESS_UNIT(+) = '" & strSiteID & "' ) BINLOC" & vbCrLf & _
                            "WHERE BINLOC.INV_ITEM_ID = PINV.INV_ITEM_ID" & vbCrLf & _
                              "AND (PINV.INV_LOT_ID <> 'NONE' OR PINV.SERIAL_ID <> 'NONE') " & vbCrLf & _
                              "AND PINV.AVAIL_STATUS = '1' " & vbCrLf & _
                              "AND PINV.QTY > 0 AND pinv.business_unit = A.business_unit(+) AND pinv.storage_area = A.storage_area(+)"
                'removed ",C23.QTY " & vbCrLf & _


                trackFlag = "Tracked"

            Else
                ' if cosigned treat it as a normal item with SDI controlling the Inventory - show location
                If objEnterprise.IOH = "Y" And Not Session("Cosigned") Then

                    strSQLString = "SELECT I.INV_ITEM_ID" & vbCrLf & _
                                                     " ,SUBSTR(I.INV_ITEM_ID,4) as PRODUCT_ALIAS" & vbCrLf & _
                                                     " ,I.INV_ITEM_ID" & vbCrLf & _
                                                     " ,B.DESCR254 " & vbCrLf & _
                                                     " ,I.BUSINESS_UNIT " & vbCrLf & _
                                                     " ,I.STORAGE_AREA " & vbCrLf & _
                                                     " ,I.STOR_LEVEL_1 " & vbCrLf & _
                                                     " ,I.STOR_LEVEL_2 " & vbCrLf & _
                                                     " ,I.STOR_LEVEL_3 " & vbCrLf & _
                                                     " ,I.STOR_LEVEL_4 " & vbCrLf & _
                                                     " ,trunc(I.QTY_ONHAND,0) as QTY" & vbCrLf & _
                                                     " ,'NA' as UNIT_OF_MEASURE " & vbCrLf & _
                                                     " ,A.descr " & vbCrLf & _
                                                     " FROM sysadm8.ps_isa_oro_ioh I," & vbCrLf & _
                                                     " PS_INV_ITEMS B,  " & vbCrLf & _
                                                     " ps_stor_area_inv A " & vbCrLf & _
                                                     " WHERE I.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                                                     " and B.INV_ITEM_ID = I.INV_ITEM_ID " & vbCrLf & _
                                                    "   AND B.EFFDT = ( " & vbCrLf & _
                                                    "                  SELECT MAX(B_ED.EFFDT)  " & vbCrLf & _
                                                     "                  FROM PS_INV_ITEMS B_ED " & vbCrLf & _
                                                    "                  WHERE B.SETID = B_ED.SETID " & vbCrLf & _
                                                    "                    AND B.INV_ITEM_ID = B_ED.INV_ITEM_ID " & vbCrLf & _
                                                    "                    AND B_ED.EFFDT <= SYSDATE " & vbCrLf & _
                                                    "                 ) " & vbCrLf & _
                                                    "   AND B.INV_ITEM_ID = I.INV_ITEM_ID(+) " & vbCrLf & _
                                                    "   AND I.BUSINESS_UNIT(+) like '%" & strSiteID & "' " & vbCrLf & _
                                                    "   AND I.business_unit = A.business_unit(+) " & vbCrLf & _
                                                    "   AND I.storage_area = A.storage_area(+) " & vbCrLf & _
                                                                        ""

                Else

                    strSQLString = "" &
            "SELECT A.BUSINESS_UNIT, A.QTY_ONHAND as Qty_On_Hand,  A.QTY_AVAILABLE as Qty_Available, A.QTY_RESERVED as Total_Qty_Reserved," & vbCrLf &
"B.STORAGE_AREA, B.STOR_LEVEL_1, B.STOR_LEVEL_2, B.STOR_LEVEL_3, B.STOR_LEVEL_4" & vbCrLf &
", case When c.storage_Area is not null then 'Yes' else 'No' END as Default_Loc" & vbCrLf &
", B.QTY as Qty, B.UNIT_OF_MEASURE, B.QTY_RESERVED as Qty_Reserved_Not_Picked " & vbCrLf &
"FROM sysadm8.PS_BU_ITEMS_INV A, sysadm8.PS_PHYSICAL_INV B, sysadm8.ps_default_loc_inv C" & vbCrLf &
"WHERE A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)" & vbCrLf &
"AND A.INV_ITEM_ID = B.INV_ITEM_ID(+)" & vbCrLf &
"AND B.BUSINESS_UNIT = C.BUSINESS_UNIT(+)" & vbCrLf &
"AND B.INV_ITEM_ID = C.INV_ITEM_ID(+)" & vbCrLf &
"AND B.STORAGE_AREA = C.STORAGE_AREA(+)" & vbCrLf &
"AND B.STOR_LEVEL_1 = C.STOR_LEVEL_1(+)" & vbCrLf &
"AND B.STOR_LEVEL_2 = C.STOR_LEVEL_2(+) " & vbCrLf &
"AND B.STOR_LEVEL_3 = C.STOR_LEVEL_3(+)" & vbCrLf &
"AND B.STOR_LEVEL_4 = C.STOR_LEVEL_4(+)" & vbCrLf &
"AND A.INV_ITEM_ID LIKE '" & strInvItemID & "'" & vbCrLf &
"UNION" & vbCrLf &
"SELECT A.BUSINESS_UNIT, A.QTY_ONHAND as Qty_On_Hand,  A.QTY_AVAILABLE as Qty_Available, A.QTY_RESERVED as Total_Qty_Reserved, " & vbCrLf &
"C.STORAGE_AREA, C.STOR_LEVEL_1, C.STOR_LEVEL_2, C.STOR_LEVEL_3, C.STOR_LEVEL_4, case When c.storage_Area Is Not null then 'Yes' else 'No' END as Default_Loc, 0 as Qty, d.unit_measure_std,  0 as Qty_Reserved_Not_Picked " & vbCrLf &
"FROM sysadm8.PS_BU_ITEMS_INV A, sysadm8.ps_default_loc_inv C, sysadm8.ps_master_item_tbl d" & vbCrLf &
"WHERE A.BUSINESS_UNIT = C.BUSINESS_UNIT" & vbCrLf &
"AND A.INV_ITEM_ID = C.INV_ITEM_ID" & vbCrLf &
"and a.inv_item_id = d.inv_item_id" & vbCrLf &
"and not exists (select 'X' from sysadm8.ps_physical_inv b" & vbCrLf &
        "where c.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf &
        "AND c.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf &
        "AND c.STORAGE_AREA = b.STORAGE_AREA" & vbCrLf &
        "AND c.STOR_LEVEL_1 = b.STOR_LEVEL_1" & vbCrLf &
        "AND c.STOR_LEVEL_2 = b.STOR_LEVEL_2 " & vbCrLf &
        "AND c.STOR_LEVEL_3 = b.STOR_LEVEL_3" & vbCrLf &
        "AND c.STOR_LEVEL_4 = b.STOR_LEVEL_4)" & vbCrLf &
"And A.INV_ITEM_ID Like '" & strInvItemID & "'"


                End If
            End If
            lblNoData.Visible = False
            dsLocDataSet = ORDBData.GetAdapter(strSQLString)
            If Not dsLocDataSet Is Nothing Then
                If dsLocDataSet.Tables.Count > 0 Then
                    If trackFlag = "Not Tracked" Then
                        dsLocDataSet.Tables(0).Columns.Add("SERIAL_ID", GetType(String))
                        btnSetAsTrack.Enabled = True
                        dsLocDataSet.AcceptChanges()
                    End If
                End If
            End If
            Dim I As Integer
            Dim decQty As Decimal
            If Not dsLocDataSet Is Nothing Then
                If dsLocDataSet.Tables.Count > 0 Then
                    dsLocDataSet.Tables(0).Columns.Add("EquipGuid", GetType(String))
                    If dsLocDataSet.Tables(0).Rows.Count > 0 Then
                        'For I = 0 To dsLocDataSet.Tables(0).Rows.Count - 1
                        '    If IsDBNull(dsLocDataSet.Tables(0).Rows(I).Item("QTY")) Then
                        '        decQty = 0.0
                        '    Else
                        '        decQty = dsLocDataSet.Tables(0).Rows(I).Item("QTY")
                        '    End If
                        '    If decQty = 0 Then
                        '        Dim strDfltBin As String = GetDfltBin(dsLocDataSet.Tables(0).Rows(I))
                        '        If strDfltBin = "Delete" Then
                        '            dsLocDataSet.Tables(0).Rows(I).Delete()
                        '        End If
                        '    End If
                        'Next
                        'dsLocDataSet.AcceptChanges()

                        Dim serialNo As String = String.Empty
                        Dim locationGUID As String = String.Empty
                        Dim bLocationInfo As Boolean = False
                        For Each drLocData As DataRow In dsLocDataSet.Tables(0).Rows
                            If Not drLocData("SERIAL_ID") Is Nothing Then
                                Try
                                    serialNo = Convert.ToString(drLocData("SERIAL_ID"))
                                Catch ex As Exception
                                    serialNo = ""
                                End Try
                                If Trim(serialNo) <> "" Then
                                    locationGUID = GetEquipGUID(serialNo)
                                    drLocData("EquipGuid") = locationGUID
                                Else
                                    drLocData("EquipGuid") = ""
                                End If
                            Else
                                drLocData("EquipGuid") = ""
                            End If
                            Try
                                bLocationInfo = Len(drLocData("BUSINESS_UNIT")) > 0
                            Catch ex As Exception
                                bLocationInfo = False
                            End Try
                        Next
                        dsLocDataSet.AcceptChanges()
                        Session("dsLocDataSet") = dsLocDataSet
                        Dim dtLocDataSet As DataTable
                        dtLocDataSet = dsLocDataSet.Tables(0).AsEnumerable().GroupBy(Function(row) row.Field(Of String)("BUSINESS_UNIT")).Select(Function(group) group.First()).CopyToDataTable()
                        If bLocationInfo Then
                            LocationGrid.DataSource = dtLocDataSet
                            LocationGrid.DataBind()
                            lblNoData.Visible = False
                        Else
                            LocationGrid.DataSource = Nothing
                            LocationGrid.DataBind()
                            lblNoData.Visible = True
                        End If
                    Else
                        LocationGrid.DataSource = Nothing
                        LocationGrid.DataBind()
                        lblNoData.Visible = True
                    End If
                End If

            End If
        Catch ex As Exception
            lblNoData.Visible = False
        End Try
    End Sub

    Private Function GetEquipGUID(ByVal serialNO As String) As String
        Try
            Dim eqGUID As String = String.Empty
            Dim sql As String
            Dim rdr1 As OleDb.OleDbDataReader
            Try
                sql = "SELECT * FROM SYSADM8.PS_ISA_SDITRCK_ITM where SERIAL_ID = '" & serialNO & "'"
                rdr1 = ORDBData.GetReader(sql)
                If Not (rdr1 Is Nothing) Then
                    While rdr1.Read
                        If rdr1.HasRows Then
                            eqGUID = Convert.ToString(rdr1("ISA_GUID"))
                        End If
                    End While
                End If
                rdr1.Close()
            Catch ex77 As Exception
                Try
                    rdr1.Close()
                Catch ex As Exception

                End Try
            End Try
            Return eqGUID
        Catch ex As Exception
        End Try

    End Function

    Public Function GetOwner(ByVal ownedby As Object) As String
        Select Case Left(ownedby, 1)
            Case "I", "B"
                Return "SDI"
            Case Else
                Return "Customer"
        End Select

    End Function

    Public Function Getqty(ByVal qty As Object, ByVal uom As Object) As String
        'qty = Convert.ToInt32(qty)
        If IsNumeric(qty) Then
            qty = CType(qty, Decimal)
        Else
            qty = 0
        End If
        Return qty.ToString() & " " & uom
    End Function

    ' The PS_FXD_BIN_LOC_INV table is checked when the qty on hand is zero to make sure this is an
    ' active bin location.  All bin locations that an item may ever have had still remain in the
    ' Peoplesoft In-Site tables (PS_PHYSICAL_INV).  So if an item was moved to a different location,
    ' we do not want to show which location it was in but only which location it is currently in.
    ' The only way to do this is by checking the PS_FXD_BIN_LOC_INV table.  This is the same process
    ' used for cycle count and physical inventory.  (This was first discovered by University of Penn).
    'Update: 2018-03-06 Ticket 128885 - replacing obsolete PS_FXD_BIN_LOC_INV with PS_DEFAULT_LOC_INV

    'Public Function GetDfltBin(ByVal datarow As Object) As String
    '    Try
    '        Dim strSQLString As String = ""
    '        strSQLString = "SELECT INV_ITEM_ID" & vbCrLf & _
    '          " FROM PS_DEFAULT_LOC_INV" & vbCrLf & _
    '          " WHERE BUSINESS_UNIT = '" & datarow("BUSINESS_UNIT") & "' " & vbCrLf & _
    '          " AND INV_ITEM_ID = '" & datarow("INV_ITEM_ID") & "'" & vbCrLf & _
    '          " AND STORAGE_AREA = '" & datarow("STORAGE_AREA") & "'" & vbCrLf & _
    '          " AND STOR_LEVEL_1 = '" & datarow("STOR_LEVEL_1") & "'" & vbCrLf & _
    '          " AND STOR_LEVEL_2 = '" & datarow("STOR_LEVEL_2") & "'" & vbCrLf & _
    '          " AND STOR_LEVEL_3 = '" & datarow("STOR_LEVEL_3") & "'" & vbCrLf & _
    '          " AND STOR_LEVEL_4 = '" & datarow("STOR_LEVEL_4") & "'" '& vbCrLf & _
    '        '" AND UNIT_OF_MEASURE = '" & datarow("UNIT_OF_MEASURE") & "'" & vbCrLf
    '        Dim dtrDfltBinReader As OleDbDataReader = ORDBData.GetReader(strSQLString)
    '        If (dtrDfltBinReader.Read()) Then
    '            dtrDfltBinReader.Close()
    '            Return "Good"
    '        Else
    '            dtrDfltBinReader.Close()
    '            Return "Delete"
    '        End If
    '    Catch ex As Exception
    '    End Try
    'End Function

#End Region

    Private Sub BuItemsGrid_ItemDataBound(ByVal sender As Object, ByVal e As GridItemEventArgs) Handles BuItemsGrid.ItemDataBound
        Try
            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim strsql As String = " "
                Dim rdr1 As OleDb.OleDbDataReader
                Dim rdr2 As OleDb.OleDbDataReader
                Dim item As GridDataItem = TryCast(e.Item, GridDataItem)
                ' test with this muther S326105003G
                Dim strBusU As String = item("BUSINESS_UNIT").Text
                strBusU = strBusU.Substring(1)
                Dim str_storeroom_type As String = " "
                Dim str_vend_model As String = "Storeroom"
                Dim Str_vend_desc As String = "Storeroom Description"
                ' get Vendor model for the Vending Machines
                strsql = " SELECT Distinct " & vbCrLf & _
                "ISA_VEND_MODEL" & vbCrLf & _
                " FROM " & vbCrLf & _
                " SYSADM8.PS_ISA_VEND_DEVICE" & vbCrLf & _
                " WHERE ISA_CRIB_IDENT = '" & strBusU & "'"

                rdr1 = ORDBData.GetReader(strsql)
                If Not (rdr1 Is Nothing) Then
                    str_vend_model = " "
                    If Not rdr1.HasRows Then
                        str_storeroom_type = "STOREROOM"
                    Else
                        While rdr1.Read
                            str_vend_model = ""
                            Try
                                str_vend_model = CStr(rdr1("ISA_VEND_MODEL"))
                                If (str_vend_model Is Nothing) Then
                                    str_vend_model = "STOREROOM"
                                Else
                                    strsql = " SELECT  " & vbCrLf & _
                                    "descr" & vbCrLf & _
                                    " FROM " & vbCrLf & _
                                    " SYSADM8.PS_ISA_VEND_DEVICE" & vbCrLf & _
                                    " WHERE isa_vend_model = '" & str_vend_model & "'"
                                    rdr2 = ORDBData.GetReader(strsql)
                                    While rdr2.Read
                                        If Not rdr2.HasRows Then
                                            ' put something here
                                        Else
                                            Str_vend_desc = CStr(rdr2("descr"))
                                        End If
                                    End While
                                    rdr2.Close()
                                End If
                            Catch ex65 As Exception
                                Try
                                    rdr2.Close()
                                Catch ex As Exception

                                End Try
                            End Try
                        End While
                    End If
                End If
                If str_vend_model = " " Then
                    str_vend_model = "STOREROOM"
                End If
                If Str_vend_desc = " " Then
                    Str_vend_desc = "STRM"
                End If
                rdr1.Close()
                Dim Label2 As Label = CType(item.FindControl("Label2"), Label)
                Dim Label3 As Label = CType(item.FindControl("Label3"), Label)
                Label2.Text = str_vend_model
                Label3.Text = Str_vend_desc

            End If
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub rptSimilarItems_ItemDataBound(ByVal sender As Object, ByVal e As RepeaterItemEventArgs)
        Try
            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
                'lblprice
                Dim lblPrice As Label = CType(e.Item.FindControl("lblprice"), System.Web.UI.WebControls.Label)
                If Not lblPrice.Text Is Nothing Then
                    If Convert.ToDecimal(lblPrice.Text) <= 0 Then
                        lblPrice.Text = "Price on Request"
                    Else
                        lblPrice.Text = String.Format("{0:###,###.00}", CType(lblPrice.Text, Decimal))
                    End If
                End If

                Dim lblSupplierDesc As Label = CType(e.Item.FindControl("lblSupplierDesc"), System.Web.UI.WebControls.Label)
                If (lblSupplierDesc.Text.Length > 80) Then
                    lblSupplierDesc.Text = lblSupplierDesc.Text.Substring(0, 75) + ".."
                End If

                Dim img As System.Web.UI.WebControls.Image = CType(e.Item.FindControl("imgProduct"), System.Web.UI.WebControls.Image)
                Dim hdn As HiddenField = CType(e.Item.FindControl("hdnImage"), System.Web.UI.WebControls.HiddenField)

                If IsDBNull(hdn.Value) Or Trim(hdn.Value) = "" Or _
                    hdn.Value = "&nbsp;" Then
                    img.ImageUrl = "Images\noimage_new.png"
                Else
                    Dim imgFilename As String = ""
                    Try
                        imgFilename = hdn.Value.Trim
                    Catch ex As Exception
                    End Try

                    If Not Session("ImageLocPath") Is Nothing Then
                        img.ImageUrl = Session("ImageLocPath") & imgFilename
                        img.Style.Add("cursor", "hand")
                    End If
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub sdiTabStrip_TabClick(ByVal sender As Object, ByVal e As RadTabStripEventArgs)
        Try
            panelNo.Visible = False
            RadMultiPage1.RenderSelectedPageOnly = True
            RadMultiPage1.SelectedIndex = sdiTabStrip.SelectedIndex
            Dim position As New AjaxLoadingPanelBackgroundPosition()
            position = AjaxLoadingPanelBackgroundPosition.Center
            RadAjaxLoadingPanel1.BackgroundTransparency = 25
            Dim SitePrefix As String = ""
            Dim BU As String = ""
            ' Site Prefix
            Try
                SitePrefix = Session("SITEPREFIX").ToString()
            Catch ex As Exception
                SitePrefix = ""
            End Try
            ' Business Unit 
            Try
                BU = Session("BUSUNIT").ToString()
            Catch ex As Exception
                BU = ""
            End Try
            ' Item ID 
            Dim strInvItemID As String = ""

            strInvItemID = lblproductidtext.Text

            Select Case e.Tab.Value
                Case "CAT"
                    PanelDetailMain.Visible = True
                    PanelCata.Visible = True
                    panelInv.Visible = False
                    PanelUse.Visible = False
                    panelIss.Visible = False
                    PanelLoc.Visible = False
                    PanelSrc.Visible = False
                    Exit Select
                Case "ITM"
                    BuildDetailDisplay(lblproductidtext.Text, strbu)
                    Exit Select
                Case "USE"
                    buildusagedisplay(lblproductidtext.Text)
                    Exit Select
                Case "ISS"
                    buildissuedisplay(lblproductidtext.Text)
                    Exit Select
                Case "BIN"
                    buildLocationdisplay(lblproductidtext.Text)
                    Exit Select
                Case "SITE"
                    PanelDetailMain.Visible = False
                    PanelSrc.Visible = True
                    lblSisterSiteError.Visible = False
                    buildmultisitedisplay(lblproductidtext.Text)
                    Exit Select
                Case "CYCTRANS"
                    Dim lblMsg As Label = DirectCast(transHistory.FindControl("lblMsg"), Label)
                    Dim rgTrans As RadGrid = DirectCast(transHistory.FindControl("dtgTrans"), RadGrid)
                    Dim hdnItm As HiddenField = DirectCast(transHistory.FindControl("hdnItemID"), HiddenField)

                    If Not String.IsNullOrEmpty(strInvItemID.Trim()) Then
                        '    lblMsg.Visible = True
                        '    rgTrans.Visible = False
                        '    lblMsg.Text = ""
                        '    lblMsg.ForeColor = Color.Red
                        'Else
                        lblMsg.Visible = False
                        Dim Trans As New TransactionHistory
                        Trans.buildtransgrid(strInvItemID, SitePrefix, BU)
                        Dim ds As DataSet = Trans.TransDS
                        hdnItm.Value = SitePrefix & strInvItemID
                        rgTrans.Visible = True
                        rgTrans.DataSource = ds
                        rgTrans.DataBind()
                        Session("TransDS") = ds
                    End If
                    Exit Select

                Case "CYCPURC"

                    Dim lblErr As Label = DirectCast(PurchaseHistroy.FindControl("lblErr"), Label)
                    Dim rgPurc As RadGrid = DirectCast(PurchaseHistroy.FindControl("rgPurchase"), RadGrid)

                    If Not String.IsNullOrEmpty(strInvItemID.Trim()) Then
                        '    lblErr.Visible = True
                        '    rgPurc.Visible = False
                        '    lblErr.Text = "Please expand item and try"
                        '    lblErr.ForeColor = Color.Red
                        'Else
                        lblErr.Visible = False
                        Dim Purc As New PurchaseHistory
                        Purc.BuildPurchaseGrid(strInvItemID, SitePrefix, BU)
                        Dim ds As DataSet = Purc.PurcDS
                        rgPurc.Visible = True
                        rgPurc.DataSource = ds
                        rgPurc.DataBind()
                        Session("PurcDS") = ds
                    End If
                    Exit Select
                Case Else

            End Select
        Catch ex As Exception

        End Try
    End Sub

    Private Function GetCurrencyCode(ByVal sSDIItemID As String, ByVal sItemBasePrice As String) As String
        Try
            Dim itemBaseCurrency As sdiCurrency = Nothing
            Dim sCurrCode As String = ""
            If sItemBasePrice.ToUpper <> "PRICE ON REQUEST" Then
                Try
                    itemBaseCurrency = sdiMultiCurrency.getItemBaseCurrency(sSDIItemID)
                    sCurrCode = itemBaseCurrency.Id
                Catch ex As Exception
                End Try
            End If
            Return sCurrCode
        Catch ex As Exception
        End Try
    End Function

#Region "TANGO"

    Private Sub btnSentToTrack_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSentToTrack.Click
        Try
            If String.IsNullOrEmpty(txtSerialNo.Text) And String.IsNullOrEmpty(txtPlantTag.Text) Then
                lblEquipGuid.Text = "Please enter Serial Number and Plant Tag"
            Else
                SendToSdiTrack()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub SendToSdiTrack()
        Try
            Dim tangoEquipment As New TangoEquipment()

            Dim cdaeiunew As New com.tf7.www1.ConcertDatabaseAliasAndExternalInterfaceUser()

            Dim sTrackDBType As String = "TangoDatabaseGUID"
            Try
                sTrackDBType = Session("TrackDBType")
                If Trim(sTrackDBType) = "" Then
                    sTrackDBType = "TangoDatabaseGUID"
                End If
            Catch ex As Exception
                sTrackDBType = "TangoDatabaseGUID"
            End Try
            cdaeiunew.DatabaseAliasType = sTrackDBType  '  "TangoDatabaseGUID"

            Dim sTrackDBGUID As String = "9E3BC4CA-42C6-4189-9B21-7708F81BACE4"
            Try
                sTrackDBGUID = Session("TrackDBGUID")
                If Trim(sTrackDBGUID) = "" Then
                    sTrackDBGUID = "9E3BC4CA-42C6-4189-9B21-7708F81BACE4"
                End If
            Catch ex As Exception
                sTrackDBGUID = "9E3BC4CA-42C6-4189-9B21-7708F81BACE4"
            End Try
            cdaeiunew.DatabaseAlias = sTrackDBGUID  '  "9E3BC4CA-42C6-4189-9B21-7708F81BACE4"

            Dim sTrackDBUser As String = "ISOL"
            Try
                sTrackDBUser = Session("TrackDBUser")
                If Trim(sTrackDBUser) = "" Then
                    sTrackDBUser = "ISOL"
                End If
            Catch ex As Exception
                sTrackDBUser = "ISOL"
            End Try
            cdaeiunew.UserName = sTrackDBUser  '  "ISOL"

            Dim sTrackDBPassword As String = "Cup45B0x"
            Try
                sTrackDBPassword = Session("TrackDBPassword")
                If Trim(sTrackDBPassword) = "" Then
                    sTrackDBPassword = "Cup45B0x"
                End If
            Catch ex As Exception
                sTrackDBPassword = "Cup45B0x"
            End Try
            cdaeiunew.Password = sTrackDBPassword  '  "Cup45B0x"

            Dim npv As NewPropertyValue() = New NewPropertyValue(5) {}

            npv(0) = New NewPropertyValue()
            npv(0).EquipmentPropertyIDorName = "45"
            npv(0).PropertyUnitsID = 65535
            npv(0).Value = txtSerialNo.Text

            npv(1) = New NewPropertyValue()
            npv(1).EquipmentPropertyIDorName = "46"
            npv(1).PropertyUnitsID = 65535
            npv(1).Value = txtPlantTag.Text

            npv(2) = New NewPropertyValue()
            npv(2).EquipmentPropertyIDorName = "48"
            npv(2).PropertyUnitsID = 65535
            npv(2).Value = "ItemID"

            npv(3) = New NewPropertyValue()
            npv(3).EquipmentPropertyIDorName = "780"
            npv(3).PropertyUnitsID = 65535
            npv(3).Value = "001"

            npv(4) = New NewPropertyValue()
            npv(4).EquipmentPropertyIDorName = "781"
            npv(4).PropertyUnitsID = 65535
            npv(4).Value = "002"

            npv(5) = New NewPropertyValue()
            npv(5).EquipmentPropertyIDorName = "782"
            npv(5).PropertyUnitsID = 65535
            npv(5).Value = "003"

            Dim gd As New Guid()
            gd = tangoEquipment.AddEquipment(cdaeiunew, 93, DateTime.Today, npv)

            Dim strEquipGuid As String

            strEquipGuid = gd.ToString()

            lblEquipGuid.Text = "Equipment Guid Returned: " + strEquipGuid

            Dim strAddEquip = "Insert into SYSADM8.PS_ISA_SDITRCK_ITM(PLANT_TAG, INV_ITEM_ID, SERIAL_ID, ISA_GUID, DT_TIMESTAMP, OPRID,SDTRK_GUID,BUSINESS_UNIT) values " & vbCrLf & _
                                 "('" & txtPlantTag.Text & "','" & lblproductidtext.Text & "','" & txtSerialNo.Text & "', '" & strEquipGuid & "','" & DateTime.Today & "','TANGO',' ','" & Session("BUSUNIT") & "')"

            Dim rowsAffect As Integer = ORDBData.ExecNonQuery(strAddEquip)

        Catch ex As Exception
            lblEquipGuid.Text = "Exception: " + ex.Message
        End Try
    End Sub

    Sub LocationGrid_ItemCommand(ByVal sender As System.Object, ByVal e As GridCommandEventArgs) Handles LocationGrid.ItemCommand
        Try
            If e.CommandName = "TangoItemDetail" Then
                'Code to create the user session to SDiTrack

                Dim tangouser As New TangoUser()
                Dim cdaeiu1 As New com.tf7.www.ConcertDatabaseAliasAndExternalInterfaceUser()
                Dim sTrackDBType As String = "TangoDatabaseGUID"
                Try
                    sTrackDBType = Session("TrackDBType")
                    If Trim(sTrackDBType) = "" Then
                        sTrackDBType = "TangoDatabaseGUID"
                    End If
                Catch ex As Exception
                    sTrackDBType = "TangoDatabaseGUID"
                End Try
                cdaeiu1.DatabaseAliasType = sTrackDBType   '  "TangoDatabaseGUID"
                Dim sTrackDBGUID As String = "9E3BC4CA-42C6-4189-9B21-7708F81BACE4"
                Try
                    sTrackDBGUID = Session("TrackDBGUID")
                    If Trim(sTrackDBGUID) = "" Then
                        sTrackDBGUID = "9E3BC4CA-42C6-4189-9B21-7708F81BACE4"
                    End If
                Catch ex As Exception
                    sTrackDBGUID = "9E3BC4CA-42C6-4189-9B21-7708F81BACE4"
                End Try
                cdaeiu1.DatabaseAlias = sTrackDBGUID  '  "9E3BC4CA-42C6-4189-9B21-7708F81BACE4"
                Dim sTrackDBUser As String = "ISOL"    '  Session("TrackDBPassword")
                Try
                    sTrackDBUser = Session("TrackDBUser")
                    If Trim(sTrackDBUser) = "" Then
                        sTrackDBUser = "ISOL"
                    End If
                Catch ex As Exception
                    sTrackDBUser = "ISOL"
                End Try
                cdaeiu1.UserName = sTrackDBUser  '  "ISOL"
                Dim sTrackDBPassword As String = "Cup45B0x"    '  Session("TrackDBPassword")
                Try
                    sTrackDBPassword = Session("TrackDBPassword")
                    If Trim(sTrackDBPassword) = "" Then
                        sTrackDBPassword = "Cup45B0x"
                    End If
                Catch ex As Exception
                    sTrackDBPassword = "Cup45B0x"
                End Try
                cdaeiu1.Password = sTrackDBPassword  '  "Cup45B0x"
                Dim objGuid As Guid
                Try
                    Dim un As String
                    un = Session("SDITRACKUSRNAME")
                    objGuid = tangouser.AddUserSession(cdaeiu1, Session("SDITRACKUSRNAME"), Session("SDITRACKPWD"), 10, "TangoWeb")
                Catch ex As Exception
                    'lblError.Text = "Exception - " + ex.Message
                End Try

                Dim xmlDoc As New System.Xml.XmlDocument()
                Dim userNode As System.Xml.XmlNode = xmlDoc.CreateElement("EquipmentGuid")
                userNode.InnerText = e.CommandArgument
                Dim strURI As String
                strURI = tangouser.Uri(cdaeiu1, "MobileEquipmentDefinition", objGuid, userNode)
                Response.Redirect(strURI, False)
            End If
            If e.CommandName = RadGrid.ExpandCollapseCommandName Then
                Dim item As GridItem
                For Each item In e.Item.OwnerTableView.Items
                    If item.Expanded AndAlso Not item Is e.Item Then
                        item.Expanded = False
                    End If
                Next item
            End If
        Catch ex As Exception
        End Try
    End Sub

#End Region

    Protected Sub btnShowExtndOraDesc_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnShowExtndOraDesc.Click
        Try
            Dim script As String = "function f(){$find(""" + rwOracleDescPopup.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub LocationGrid_DetailTableDataBind(sender As Object, e As GridDetailTableDataBindEventArgs)
        Try
            Dim parentItem As GridDataItem = CType(e.DetailTableView.ParentItem, GridDataItem)
            Dim BusinessUnit As String = parentItem.GetDataKeyValue("BUSINESS_UNIT").ToString()
            Dim dsLocDataSet As DataSet
            Dim dv As DataView
            Dim dt As DataTable
            dsLocDataSet = Session("dsLocDataSet")
            Try
                dt = dsLocDataSet.Tables(0).AsEnumerable().Where(Function(r) r.Field(Of String)("BUSINESS_UNIT") = BusinessUnit).CopyToDataTable()
                dv = dt.DefaultView()
            Catch ex As Exception
                dt = dsLocDataSet.Tables(0)
                dt.Rows.Clear()
                dv = dt.DefaultView()
            End Try
            e.DetailTableView.DataSource = dv
            e.DetailTableView.EnableNoRecordsTemplate = True
            e.DetailTableView.NoDetailRecordsText = "No records found"
        Catch ex As Exception

        End Try
    End Sub
    Protected Sub LocationGrid_PreRender(sender As Object, e As EventArgs)
        Try
            For i As Integer = 0 To LocationGrid.MasterTableView.Items.Count - 1
                If i = 0 Then
                    LocationGrid.MasterTableView.Items(i).Expanded = True
                End If

            Next
        Catch ex As Exception

        End Try
    End Sub
End Class