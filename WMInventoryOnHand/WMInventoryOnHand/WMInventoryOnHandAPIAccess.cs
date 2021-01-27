<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="PODetails.aspx.vb" Inherits="Insiteonline.PODetails" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">    
    <%--   <link href="Styles/SDI_Style.css" rel="stylesheet" type="text/css" />--%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server" CssClass="position-relative">

    
    <div class="po-details-panel">  
        
        <div class="po-details-header PO-Errorinfo-label-PODetails">            
            <asp:Label runat="server" ID="lblPOID" CssClass="PO-header-name" Visible="false">Purchase Order Number : </asp:Label>
            <asp:Label runat="server" ID="lbltxtPO" CssClass="PO-header-value" Visible="false"></asp:Label>
            <asp:Label runat="server" CssClass="block-element" ID="lblPOHeaderInfo" Visible="false"></asp:Label> 
        </div>

        <div class="po-details-left">
            <asp:TextBox ID="txtPO" CssClass="POConfirm_txtPO"  runat="server" ReadOnly="True" Visible="false"></asp:TextBox>
             <asp:TextBox ID="txtPOBU" CssClass="POConfirm_txtPO" runat="server" ReadOnly="True" Visible="false"></asp:TextBox>
            <asp:TextBox ID="txtHidepo" runat="server" Visible="False" CssClass="POConfirm_txtHidepo"></asp:TextBox>
            <asp:TextBox ID="txtVendor" runat="server" Visible="False" CssClass="POConfirm_txtVendor"></asp:TextBox>
            <div class="po-value-enter" runat ="server" id="divPOLink">
                <asp:Label ID="lblPO_emaillink">PO number : </asp:Label>
                <asp:Label runat="server" ID="lblPOnumber_emaillink" CssClass="txtPO_Single_cont"></asp:Label>
            </div>            
            <div class="po-value-enter" runat ="server" id="divPOSearch">                
                <asp:Label runat="server" ID="lblPOText">PO number</asp:Label>
                <asp:TextBox ID="txtPO_Single" runat="server" CssClass="txtPO_Single_cont" MaxLength="10"></asp:TextBox>                
                <asp:Button ID="btnSearchPO" runat="server" CssClass="button-fancy1 btnBldPOList_cont" Text="Submit"></asp:Button>                
                <asp:Button ID="btnBack" runat="server" CssClass="button-fancy1 btnBldPOList_cont" Text="Back To Home"></asp:Button> 
                <div class="error-info">
                    <asp:Label runat="server" CssClass="block-element POConfirm_lblTrkError" ID="lblSinglePOInfo" Visible="false"></asp:Label>
                    <asp:Label ID="lblPOInfoText" CssClass="block-element" runat="server" Text="(If you do not have the PO number contact your buyer)"></asp:Label>
                </div>
            </div>
            <div>
                <asp:Label runat="server" ID="lblCurrentTab" Visible="true" CssClass="PO-Tabname-PODetails"/>
            </div>  
            <div>
                
            </div>
            <div class="PO-values" runat ="server" ID="divPOinfo1">
                <div class="po-grid">
                    <asp:Label ID="lblPODate" runat="server">PO Date : </asp:Label>
                    <asp:Label ID="lblPODatetxt" runat="server"></asp:Label>
                </div>

                <div class="po-grid">
                    <asp:Label ID="lblPOLines" runat="server" >Total # of Lines : </asp:Label>
                    <asp:Label ID="lblPOLinestxt" runat="server"></asp:Label>
                </div>

                <div class="po-grid">
                    <asp:Label ID="lblPOTotal" runat="server">PO Total : </asp:Label>
                    <asp:Label ID="lblPOTotaltxt" runat="server"></asp:Label>
                </div>

                <div class="po-grid">
                   
                    <asp:Button ID="btnPrintPO"  runat="server" CssClass="button-fancy1 btnBldPOList_cont po-btn" Text="Print Purchase Order"></asp:Button> 
                    <asp:Button ID="btnhidden" Style="display: none" TabIndex="7" runat="server" Text="Submit Line" CssClass="SDI_SUBMIT_BTN" onclick="btnhidden_Click"></asp:Button>
                </div>
            </div>
             <div class="PO-values" runat ="server" ID="divWorkOrd" visible="false">               
               <div class="po-gridWO">
                    <asp:Label ID="lblWrkOrd" runat="server">Work Order# : </asp:Label>
                    <asp:Label ID="lblWrkOrdNo" runat="server"></asp:Label>
                   </div>
                 </div>         
            
        </div>

        <div class="po-details-right" runat ="server" ID="divPOinfo2">
            <div class="po-utility">
                <asp:Label ID="lblShipto" CssClass="po-utility-row" runat="server">Ship TO : </asp:Label>
                <asp:TextBox ID="txtShipTO" runat="server" CssClass="po-shipto" TextMode="MultiLine" Rows="4"></asp:TextBox>
            </div>


            <div class="po-utility">
                <asp:Label ID="lblBuyerInfo" CssClass="po-utility-row" runat="server">Buyer Info : </asp:Label>
                <asp:TextBox ID="txtBuyerInfo" runat="server" CssClass="po-shipto" TextMode="MultiLine" Rows="4"></asp:TextBox>
                <asp:TextBox ID="txtShiptoID" runat="server" Visible="False"></asp:TextBox>
            </div>
        </div>
    </div>
    <asp:Label runat="server" ID="lblTestting"></asp:Label>
    <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server"></telerik:RadAjaxManager>
    <table id="Table1" class="pfleUpdtate_table pfleUpdtate_table-tabradmenu" cellspacing="1" runat ="server" 
        cellpadding="1">
        <tr>
            <td class="align_center">
                <asp:Label ID="lblMessage" CssClass="pfleupdate_lblMessage" runat="server"></asp:Label>
                <asp:UpdatePanel ID="updPnlProfile" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="exampleWrapper">
                            <telerik:RadAjaxPanel ID="radAjaxPanel1" runat="server" EnableAJAX="true" LoadingPanelID="RadAjaxLoadingPanel1">
                                <telerik:RadTabStrip ID="tbStripPODetails" runat="server" AutoPostBack="true" OnClientTabSelected="OnClientTabSelectingHandler" SelectedIndex="0" OnTabClick="tbStripPODetails_TabClick"
                                    Align="Justify">
                                    <Tabs>
                                        <telerik:RadTab Text="PO Confirm" runat="server" PageViewID="rpvPOConfirm" Value="POCON">
                                        </telerik:RadTab>
                                        <telerik:RadTab Text="ASN" runat="server" PageViewID="rpvPOASN" Value="POASN">
                                        </telerik:RadTab>
                                        <telerik:RadTab Text="Invoice Entry" runat="server" PageViewID="rpvPOVEntry" Value="POVENT">
                                        </telerik:RadTab>

                                    </Tabs>
                                </telerik:RadTabStrip>
                                <telerik:RadMultiPage ID="RadMultiPage1" CssClass="telerik-po-details" runat="server" SelectedIndex="0">
                                    <telerik:RadPageView ID="rpvPOConfirm" runat="server">

                                        <div class="telerik-panel-po" runat="server" id="POConfirmHDR">
                                            <div class="telerik-panel-error">
                                                <asp:Label ID="lblTrkError" Visible="false" CssClass="POConfirm_lblTrkError" runat="server"></asp:Label>
                                                <asp:Label ID="lblDueDtError" Visible="false" runat="server" CssClass="POConfirm_lblTrkError"></asp:Label>
                                            </div>
                                            <asp:Label ID="lblOrderNo" CssClass="displayfield LineOrderCss" runat="server" Text="Order No:"></asp:Label>
                                                <asp:TextBox ID="txtOrder_No" runat="server" CssClass="txtPO_Single_cont LineOrdertxtCss" MaxLength="12"></asp:TextBox>
                                            <div class="telerik-po-link">
                                                
                                                <asp:LinkButton ID="btnSelectAll" runat="server">Select all lines</asp:LinkButton>
                                            </div>
                                        </div>
                                        <%-- <asp:Button ID="btnSelectAll" runat="server" CssClass="button-fancy1 btnBldPOList_cont" Text="Select All Lines"></asp:Button>--%>

                                        <telerik:RadGrid ID="dtgPO" runat="server" AutoGenerateColumns="False" CssClass="POConfirm_dtgPO" RegisterWithScriptManager="true"
                                            OnItemDataBound="dtgPO_ItemDataBound" Width="100%">
                                            <ItemStyle CssClass="displayfield"></ItemStyle>
                                            <HeaderStyle CssClass="displaylabel POConfirm_dtgPO_hs"></HeaderStyle>
                                            <MasterTableView>
                                                <Columns>
                                                    <telerik:GridTemplateColumn HeaderText="Confirm">
                                                        <ItemStyle CssClass="POConfirm_Text-Center"></ItemStyle>
                                                        <HeaderStyle CssClass="POConfirm_Text-Center1"></HeaderStyle>
                                                        <ItemTemplate>
                                                            <asp:CheckBox runat="server" ID="chkConfirm"></asp:CheckBox>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn DataField="LINE_NBR" HeaderText="Ln">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Center"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="SCHED_NBR" HeaderText="Sc">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Center"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="INV_ITEM_ID" HeaderText="SDI ItemID">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle Wrap="false" CssClass="POConfirm_Text-Center"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="DESCR254_MIXED" HeaderText="Description" Display="false">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center1"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Left"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Description" UniqueName="DescrMy_VP" DataField ="DESCR254_MIXED">
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="txtDescr254Mixed" runat="server" TextMode="MultiLine" BorderWidth="0"
                                                                Font-Size="Small" ForeColor="Blue" Font-Names="Courier New"
                                                                Rows="3" Width="260px" ReadOnly="true" Text='<%# Eval("DESCR254_MIXED")%>' CssClass="catalog_align_centerRF">
                                                            </asp:TextBox>
                                                            <asp:Label Visible="false" runat="server" ID="lbltxtDescr" Text='<%# Eval("DESCR254_MIXED")%>'></asp:Label>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Mfg Item ID" DataField="MFG_ITM_ID" >
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Right"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox CssClass="POConfirm_txtMfgItemID" MaxLength="35" EnableViewState="True" ID="txtMfgItemID" runat="server"
                                                                Text='<%# DataBinder.Eval(Container, "DataItem.MFG_ITM_ID") %>'>
                                                            </asp:TextBox>
                                                            <asp:Label Visible="false" runat="server" ID="lbltxtMfgItemID" Text='<%# DataBinder.Eval(Container, "DataItem.MFG_ITM_ID") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>

                                                    <telerik:GridTemplateColumn HeaderText="UPC ID" Visible="false" DataField ="UPC_ID">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Right"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox MaxLength="18" EnableViewState="True" ID="txtUPCID" runat="server" CssClass="POConfirm_txtMfgItemID"
                                                                Text='<%# Trim(DataBinder.Eval(Container, "DataItem.UPC_ID"))%>'>
                                                            </asp:TextBox>
                                                            <asp:Label Visible="false" runat="server" ID="lbltxtUPCID" Text='<%# DataBinder.Eval(Container, "DataItem.UPC_ID")%>'></asp:Label>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Mfg Name" DataField ="MFG_ID">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Right"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox MaxLength="18" EnableViewState="True" ID="txtMFGID" runat="server" CssClass="POConfirm_txtMfgItemID"
                                                                Text='<%# Trim(DataBinder.Eval(Container, "DataItem.MFG_ID"))%>'>
                                                            </asp:TextBox>
                                                            <asp:Label Visible="false" runat="server" ID="lbltxtMFGID" Text='<%# DataBinder.Eval(Container, "DataItem.MFG_ID")%>'></asp:Label>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>

                                                    <telerik:GridTemplateColumn HeaderText="Supplier Item ID" DataField ="ITM_ID_VNDR">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Right"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox MaxLength="35" EnableViewState="True" ID="txtVndItemID" runat="server" CssClass="POConfirm_txtMfgItemID"
                                                                Text='<%# DataBinder.Eval(Container, "DataItem.ITM_ID_VNDR") %>'>
                                                            </asp:TextBox>
                                                            <asp:Label Visible="false" runat="server" ID="lbltxtVndItemID" Text='<%# DataBinder.Eval(Container, "DataItem.ITM_ID_VNDR") %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="QTY" DataField ="QTY_PO">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Right"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox MaxLength="12" EnableViewState="True" ID="txtQTY" CssClass="POConfirm_txtQTY"
                                                                runat="server" Text='<%#FormatNumber(DataBinder.Eval(Container, "DataItem.QTY_PO"), 2) %>'>
                                                            </asp:TextBox>
                                                            <asp:Label Visible="false" runat="server" ID="lbltxtQTY" Text='<%#FormatNumber(DataBinder.Eval(Container, "DataItem.QTY_PO"), 2) %>'></asp:Label>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn DataField="UNIT_OF_MEASURE" HeaderText="UOM">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Center"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Price" DataField ="PRICE_PO">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center"></HeaderStyle>
                                                        <ItemStyle CssClass="POConfirm_Text-Center"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox MaxLength="12" EnableViewState="True" ID="decPrice" CssClass="POConfirm_txtMfgItemID POConfirm_Text-Right"
                                                                runat="server" Text='<%# FormatNumber(DataBinder.Eval(Container, "DataItem.PRICE_PO"), 5)%>'>
                                                            </asp:TextBox>
                                                            <asp:Label Visible="false" runat="server" ID="lbldecPrice" Text='<%# FormatNumber(DataBinder.Eval(Container, "DataItem.PRICE_PO"), 5)%>'></asp:Label>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Due Date">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center1"></HeaderStyle>
                                                        <ItemStyle Wrap="False" CssClass="POConfirm_Text-Center"></ItemStyle>
                                                        <ItemTemplate>
                                                            <telerik:RadDatePicker ID="dpDeliveredDate" runat="server" SelectedDate='<%# DataBinder.Eval(Container, "DataItem.DUE_DT", "{0:mm/dd/yyyy}") %>' Calendar-FastNavigationStep="12">
                                                                <Calendar ID="Calendar1" runat="server" FirstDayOfWeek="Monday" ShowRowHeaders="false"></Calendar>
                                                            </telerik:RadDatePicker>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="QTY_PO">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="PRICE_PO">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="DUE_DT">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="MFG_ID">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="MERCHANDISE_AMT">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="CURRENCY_CD" ReadOnly="True" HeaderText="Currency">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center1"></HeaderStyle>
                                                        <ItemStyle Wrap="False" CssClass="POConfirm_Text-Center"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="FREIGHT_TERMS">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="SHIP_TYPE_ID">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="MFG_ITM_ID">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="ITM_ID_VNDR">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="UPC_ID">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="IN_PROCESS_FLG">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="BUYER_ID" HeaderText="Contact Person">
                                                        <HeaderStyle CssClass="POConfirm_Text-Center1"></HeaderStyle>
                                                        <ItemStyle Wrap="False" CssClass="POConfirm_Text-Center"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="REQ_ID">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="REQ_LINE_NBR">
                                                    </telerik:GridBoundColumn>
                                                </Columns>
                                            </MasterTableView>
                                        </telerik:RadGrid>

                                        <div class="telerik-bottom-action" runat="server" id="POConfirmFDR">

                                            <div class="action-right">
                                                <div class="action-values">
                                                    <asp:Label ID="lblDBError" runat="server" CssClass="POConfirm_lblTrkError" Visible="false"></asp:Label>
                                                    <asp:Label ID="ltlAlert" runat="server" CssClass="POConfirm_lblTrkError" Visible="false"></asp:Label>
                                                </div>
                                                <div class="action-values">
                                                    <asp:Button ID="btnSubmit" runat="server" Text="Confirm" CssClass="button-fancy1"></asp:Button>
                                                    <asp:Button ID="btn90" runat="server" Text="Continue" CssClass="button-fancy1" Visible="False"></asp:Button>
                                                    <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="button-fancy1"></asp:Button>
                                                    <asp:Button ID="btnClose" runat="server" Visible="False" Text="Close" CssClass="button-fancy1"></asp:Button>
                                                </div>

                                                <%--<asp:linkbutton id="btnalreadyconfirm" runat="server" visible="false">view already confirmed lines for this po</asp:linkbutton>--%>
                                                <asp:TextBox ID="txtPO_REF" runat="server" Visible="False" CssClass="POConfirm_txtPO_REF"></asp:TextBox>
                                                <asp:TextBox ID="txtPYMNT_TERMS_CD" runat="server" Visible="False" CssClass="POConfirm_txtPO_REF"></asp:TextBox>
                                                <asp:TextBox ID="txtCNTCT_SEQ_NUM" runat="server" Visible="False" CssClass="POConfirm_txtPO_REF"></asp:TextBox>
                                                <asp:TextBox ID="txtBILL_LOCATION" runat="server" Visible="False" CssClass="POConfirm_txtPO_REF"></asp:TextBox>
                                                <asp:TextBox ID="txtTAX_EXEMPT" runat="server" Visible="False" CssClass="POConfirm_txtPO_REF"></asp:TextBox>
                                                <asp:TextBox ID="txtTAX_EXEMPT_ID" runat="server" Visible="False" CssClass="POConfirm_txtPO_REF"></asp:TextBox>
                                                <asp:TextBox ID="txtCURRENCY_CD_HDR" runat="server" Visible="False" CssClass="POConfirm_txtPO_REF"></asp:TextBox>
                                                <asp:TextBox ID="txtRT_TYPE" runat="server" Visible="False" CssClass="POConfirm_txtPO_REF"></asp:TextBox>


                                            </div>

                                            <%-- <asp:Literal ID="ltlAlert" runat="server" EnableViewState="False"></asp:Literal>--%>
                                        </div>

                                    </telerik:RadPageView>

                                    <telerik:RadPageView ID="rpvPOASN" runat="server">
                                        <div class="telerik-panel-po" runat="server" id="divASNinfo1">
                                            <div class="telerik-panel-error">
                                                <p>
                                                    Packing Slip/Invoice&nbsp;No.&nbsp;
                                            <asp:TextBox ID="txtCustomerShipping" runat="server" MaxLength="30" CssClass="custshipping_txt_cont">

                                            </asp:TextBox>&nbsp;(optional)
                                                </p>
                                                <asp:TextBox ID="txtSmallSite" runat="server" Visible="False" CssClass="smallsite_txt_cont"></asp:TextBox>
                                                <asp:TextBox ID="txtERSAction" runat="server" Visible="False" CssClass="ersaction_txt_cont"></asp:TextBox>
                                                <asp:TextBox ID="txtvndrLoc" runat="server" Visible="False" CssClass="vndrloc_txt_cont"></asp:TextBox>
                                                <asp:TextBox ID="txtasnflag" runat="server" Visible="False"></asp:TextBox>
                                                
                                                </div>

                                        </div>
                                        <telerik:RadGrid ID="dtgPOASN" runat="server" AutoGenerateColumns="False" OnItemDataBound ="dtgPOASN_ItemDataBound1">
                                            <ItemStyle CssClass="displayfield"></ItemStyle>
                                            <HeaderStyle CssClass="displaylabel radgrid_header_cont"></HeaderStyle>
                                            <MasterTableView HierarchyDefaultExpanded="true">
                                                <NestedViewTemplate>
                                                    <div style="background-color: #FCEE99; border-bottom-color: black; border-bottom-style: solid">
                                                        <asp:Label ID="lblDescr1" runat="server" Width="100px" Text="&nbsp;&nbsp; Item Description:"></asp:Label>


                                                        <span id="Descr">
                                                            <asp:Label ID="lblDescrData1" runat="server" Width="1200px"
                                                                Text='<%# Eval("DESCR254_MIXED") %>' Font-Bold="true"></asp:Label>
                                                        </span>
                                                    </div>
                                                </NestedViewTemplate>
                                                <Columns>
                                                    <telerik:GridBoundColumn DataField="LINE_NBR" HeaderText="Ln">
                                                        <%-- 0 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="SCHED_NBR" HeaderText="Sc">
                                                        <%-- 1 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="INV_ITEM_ID" HeaderText="SDI ItemID">
                                                        <%-- 2 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle Wrap="false" CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="MFG_ITM_ID" HeaderText="Mfg Item ID">
                                                        <%-- 3 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle Wrap="false" CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="ITM_ID_VNDR" HeaderText="Vnd Item ID">
                                                        <%-- 4 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle Wrap="false" CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn DataField="DESCR254_MIXED" HeaderText="Description" UniqueName="DESCR254_MIXED" Visible="false">
                                                        <%-- 5 --%>
                                                        <HeaderStyle CssClass="radgrid_alignwidth_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_alignleft_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Description" UniqueName="DescrMy2" Display="false">
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox ID="txtDescr254Mixed" runat="server" TextMode="MultiLine" BorderWidth="0"
                                                                Font-Size="Small" ForeColor="Blue" Font-Names="Courier New"
                                                                Rows="3" Width="260px" ReadOnly="true" Text='<%# Eval("DESCR254_MIXED")%>'>
                                                            </asp:TextBox>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="PO QTY" DataField ="QTY_PO">
                                                        <%-- 6 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle Wrap="false" CssClass="radgrid_alignright_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <span>
                                                                <asp:Label ID="lblQtyPo" runat="server" Text='<%#FormatNumber(Eval("QTY_PO"), 2)%>' />
                                                                <%--<%# formatnumber(Eval("QTY_PO"),2)%>--%>
                                                            </span>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Shipped QTY" Visible="false">
                                                        <%-- 7 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle Wrap="false" CssClass="radgrid_alignright_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <span>
                                                                <asp:Label ID="lblShippedQty" runat="server" Text='<%#FormatNumber(GetLNAccpt(Eval("QTY_LN_ACCPT")), 2)%>' />
                                                                <%--<%# formatnumber(GetLNAccpt(Eval("QTY_LN_ACCPT")),2)%>--%>
                                                            </span>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Open QTY" UniqueName="OpenQTY">
                                                        <HeaderStyle CssClass="RcvRpt_Center_Align"></HeaderStyle>
                                                        <ItemStyle Wrap="False" CssClass="radgrid_alignright_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <span>
                                                                <asp:Label ID="lblOpenQTY" runat="server"></asp:Label>
                                                             </span>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="QTY">
                                                        <%-- 8 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_alignright_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox EnableViewState="True" ID="txtQTY" CssClass="qty_txt_cont" runat="server"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn DataField="UNIT_OF_MEASURE" HeaderText="UOM">
                                                        <%-- 9 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Price">
                                                        <%-- 10 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle Wrap="false" CssClass="radgrid_alignright_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <span>
                                                                <%# FormatCurrency(Eval("PRICE_PO"), 5)%>
                                                            </span>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Ext.">
                                                        <%-- 11 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle Wrap="false" CssClass="radgrid_alignright_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <span>
                                                                <%#FormatCurrency(Eval("MERCHANDISE_AMT"), 2)%>
                                                            </span>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Tracking #">
                                                        <%-- 12 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox EnableViewState="True" ID="txtTrckno" runat="server" CssClass="trckno_txt_cont" MaxLength="25"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Ship via">
                                                        <%-- 13 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:DropDownList EnableViewState="True" ID="cmbShipvia" runat="server" DataValueField="SHIP_TYPE_ID"
                                                                DataTextField="DESCRSHORT" DataSource="<%#dsShipVia%>">
                                                            </asp:DropDownList>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Delivery Date">
                                                        <%-- 14 --%>
                                                        <HeaderStyle CssClass="radgrid_alignwidth_cont"></HeaderStyle>
                                                        <ItemStyle Wrap="False" CssClass="radgrid_align_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <telerik:RadDatePicker ID="dpDeliveredDate" runat="server" Calendar-FastNavigationStep="12">
                                                                <Calendar ID="Calendar1" runat="server" FirstDayOfWeek="Monday" ShowRowHeaders="false">
                                                                </Calendar>
                                                            </telerik:RadDatePicker>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="MFG_ID">
                                                        <%-- 15 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="INV_STOCK_TYPE">
                                                        <%-- 16 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="hQTYPO">
                                                        <%-- 17 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="hQTYLNACCPT">
                                                        <%-- 18 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="hMERCHANDISEAMT">
                                                        <%-- 19 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <%--<telerik:GridBoundColumn Display="False" DataField="LINE_NBR" HeaderText="Ln">
                                <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn Display="False" DataField="SCHED_NBR" HeaderText="Sc">
                                <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                            </telerik:GridBoundColumn>--%>
                                                    <telerik:GridTemplateColumn Display="False" HeaderText="Ln">
                                                        <%-- 20 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:Literal ID="hdnLn" runat="server" Visible="false" Text='<%# Eval("LINE_NBR")%>' />
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridTemplateColumn Display="False" HeaderText="Sc">
                                                        <%-- 21 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:Literal ID="hdnSc" runat="server" Visible="false" Text='<%# Eval("SCHED_NBR")%>' />
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="INV_ITEM_ID" HeaderText="SDI ItemID">
                                                        <%-- 22 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridTemplateColumn HeaderText="Common Carrier">
                                                        <%-- 23 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                        <ItemTemplate>
                                                            <asp:TextBox EnableViewState="True" ID="txtCommon" CssClass="common_txt_cont" runat="server" MaxLength="23"></asp:TextBox>
                                                        </ItemTemplate>
                                                    </telerik:GridTemplateColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="ISA_ASN_SHIP_DT" HeaderText="ISA_ASN_SHIP_DT">
                                                        <%-- 24 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="ISA_ASN_TRACK_NO" HeaderText="ISA_ASN_TRACK_NO">
                                                        <%-- 25 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="ISA_ASN_SHIP_VIA" HeaderText="ISA_ASN_SHIP_VIA">
                                                        <%-- 26 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="ISA_ASN_SHIP_VIA_ID" HeaderText="ISA_ASN_SHIP_VIA_ID">
                                                        <%-- 27 --%>
                                                        <HeaderStyle CssClass="radgrid_align_cont"></HeaderStyle>
                                                        <ItemStyle CssClass="radgrid_align_cont"></ItemStyle>
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="PO_BU"></telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="CURRENCY_CD" HeaderText="Currency"></telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="SHIP_CURRENCY" HeaderText="SHIP_CURRENCY">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="SHIP_BASE_CURRENCY" HeaderText="SHIP_BASE_CURRENCY">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="DIST_CURRENCY" HeaderText="DIST_CURRENCY">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="DIST_BASE_CURRENCY" HeaderText="DIST_BASE_CURRENCY">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="RECEIVER_ID" HeaderText="RECEIVER_ID">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="SHIPTO_ID" HeaderText="SHIPTO ID">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="QTY_LN_ACCPT" HeaderText="QTY_LN_ACCPT">
                                                    </telerik:GridBoundColumn>
                                                    <telerik:GridBoundColumn Display="False" DataField="QTY_PO" HeaderText="QTY_PO">
                                                    </telerik:GridBoundColumn>
                                                </Columns>
                                            </MasterTableView>
                                        </telerik:RadGrid>
                               
                                        <div class="telerik-bottom-action">                                            
                                            <div class="action-right">                                                
                                                <div class="action-values">
                                                    <asp:Button ID="btnASNSubmit" runat="server" CssClass="button-fancy1" Text="Submit"></asp:Button>
                                                    <asp:Button ID="btnASNCancel" runat="server" CssClass="button-fancy1" Text="Cancel"></asp:Button>
                                                </div>                                                
                                            </div>
                                            <div class="action-aligncenter">
                                                 <asp:Label ID="lblPOASNSubmitErrorInfo" runat="server" CssClass="POConfirm_lblTrkError" Visible="false"></asp:Label>
                                            </div>
                                            <div class="action-aligncenter">
                                                <asp:Button ID="btnPrint" Visible="false" runat="server"  CssClass="asn-button-fancy1" OnClientClick="return DisableSubmitButton(this,'btnPrint')" Text="Print ProForma" />
                                            </div>

                                            <%-- <asp:Literal ID="ltlAlert" runat="server" EnableViewState="False"></asp:Literal>--%>
                                        </div>
                                    </telerik:RadPageView>

                                    <telerik:RadPageView ID="rpvPOVEntry" runat="server">

                                        <div class="telerik-panel-po" runat="server" id="divVocherHead">
                                            <div class="telerik-panel-error">
                                                <asp:Label ID="lblInvoiceNo" runat="server" Width="110px" CssClass="displayfield">Invoice Number</asp:Label>
                                                <asp:TextBox ID="txtInvoiceNo" runat="server" Width="120px"></asp:TextBox>
                                                <asp:Label Style="Z-INDEX: 0" ID="lblInvoiceMessage" runat="server" CssClass="displayfield"
                                                    Font-Underline="True" Font-Bold="True" ForeColor="Maroon">Invoice # must be exactly as it appears on Invoice</asp:Label>

                                                <asp:Label ID="lblInvoiceDate" runat="server" Width="110px" CssClass="displayfield">Invoice Date</asp:Label>
                                                <%--  <cc1:maskbox id="txtInvoiceDate" runat="server" width="94px" maskformat="LongDate" height="21px" visible="false"></cc1:maskbox>--%>
                                                <asp:TextBox ID="txtInvoiceDate1" runat="server" Width="120px" Height="21px" Visible="false"></asp:TextBox>
                                                <%-- <asp:ImageButton ID="btnCalendar" runat="server" ImageUrl="~/images/cal.gif" />--%>
                                                <telerik:RadDatePicker ID="dpInvoiceDate" runat="server" SelectedDate='<%# DataBinder.Eval(Container, "DataItem.DUE_DT", "{0:mm/dd/yyyy}") %>' Calendar-FastNavigationStep="12">
                                                    <Calendar ID="Calendar1" runat="server" FirstDayOfWeek="Monday" ShowRowHeaders="false"></Calendar>
                                                </telerik:RadDatePicker>

                                                 
                                            </div>


                                        </div>
                                        <div class="telerik-panel-po" runat="server" id="div1">
                                            <div class="telerik-panel-error">
                                                

                                                <asp:Label ID="lblInvoiceAmt" runat="server" Width="110px" CssClass="displayfield">Pre Invoice Amt</asp:Label>
                                                <asp:TextBox ID="txtInvoiceAmt" runat="server" Width="120px"></asp:TextBox>                    
                                                <asp:Label ID="lblTaxAmount" runat="server" visible="false" Width="110px" CssClass="displayfield" >Invoice Tax</asp:Label>
                                                <asp:TextBox ID="txtTaxAmount" runat="server" visible="false" Width="120px" AutoPostBack="true" OnTextChanged="txtTaxAmount_TextChanged"></asp:TextBox>
                                                <asp:Label ID="lblTotalInvoiceAmt" runat="server" Width="125px" visible="false" CssClass="displayfield totalinvoiceamtCss">Total Invoice Amount: $</asp:Label>
                                                <asp:Label ID="txtTotalInvoiceAmt" runat="server" visible="false"></asp:Label>
                                            </div>


                                        </div>
                                        <div class="Po-error-text vouchererrorCss">
                                            <asp:Label ID="lblShpDtError" runat="server" ForeColor="Red"></asp:Label>
                                            <asp:Label ID="lblEntryErrMsg" runat="server" ForeColor="Red"></asp:Label>
                                            <asp:Label ID="lblMsg" runat="server"></asp:Label>
                                        </div>
                                        <div class="po-table-style">
                                            <div class="telerik-po-link po-btn-addline" runat="server" id="divVoucAddLine">
                                                <asp:Button ID="btnAddLine" runat="server" Width="110px" CssClass="button-fancy1" Text="Add Line"></asp:Button>
                                            </div>
                                                                                    
                                        </div>
                                            <telerik:RadGrid ID="grid_Voucher" runat="server" AutoGenerateColumns="false" AllowAutomaticInserts="false"
                                                 OnNeedDataSource="grid_Voucher_NeedDataSource" OnUpdateCommand="grid_Voucher_UpdateCommand"
                                                 OnEditCommand="grid_Voucher_EditCommand" OnItemDataBound="grid_Voucher_ItemDataBound"
                                                 OnCancelCommand="grid_Voucher_CancelCommand" OnDeleteCommand="grid_Voucher_DeleteCommand">
                                                <MasterTableView DataKeyNames="Key" EditMode="InPlace">
                                                    <Columns>
                                                        <telerik:GridBoundColumn DataField="Key" Visible="false" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridBoundColumn UniqueName="Ln" HeaderText="Ln" Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridBoundColumn UniqueName="Sc" HeaderText="Sc" Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridBoundColumn UniqueName="evSDIItemId" HeaderText="SDI Item ID" Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridBoundColumn UniqueName="evMfgItemId" HeaderText="Mfg Item ID" Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridBoundColumn UniqueName="evVendorItemId" HeaderText="Vnd Item ID" Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridTemplateColumn Display="True" HeaderText="Item Description" Visible="true">
                                                            <ItemTemplate>
                                                                <asp:Label ID="lblCurrentInvItemDesc" runat="server">
                                                                </asp:Label>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:Label ID="lblCurrentInvItemDescEdit" runat="server"></asp:Label>
                                                                <asp:DropDownList ID="cboCurrentInvItemDescEdit" runat="server" style="width: 77%;"></asp:DropDownList>
                                                            </EditItemTemplate>
                                                        </telerik:GridTemplateColumn>
                                                        <telerik:GridBoundColumn UniqueName="evQtyPO" HeaderText="PO Qty" Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridBoundColumn UniqueName="evUnitPricePO" HeaderText="PO Price" Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridBoundColumn UniqueName="evExtendedPricePO" HeaderText="PO Ext." Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridBoundColumn UniqueName="evPriorInvoicedQty" HeaderText="Prior Invoiced QTY" Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridTemplateColumn Display="True" HeaderText="Current Invoice QTY" Visible="true">
                                                            <ItemTemplate>
                                                                <asp:Label ID="lblCurrentInvQty" runat="server"></asp:Label>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox ID="txtCurrentInvQty" runat="server"  style="width: 50%;"></asp:TextBox>
                                                            </EditItemTemplate>
                                                        </telerik:GridTemplateColumn>
                                                        <telerik:GridTemplateColumn Display="True" HeaderText="UOM" Visible="true">
                                                            <ItemTemplate>
                                                                <asp:Label ID="lblCurrentInvUOM" runat="server"></asp:Label>
                                                            </ItemTemplate>
                                                        </telerik:GridTemplateColumn>
                                                        <telerik:GridTemplateColumn Display="True" HeaderText="Unit Invoice Price" Visible="true">
                                                            <ItemTemplate>
                                                                <asp:Label ID="lblCurrentInvUnitPrice" runat="server"></asp:Label>
                                                            </ItemTemplate>
                                                            <EditItemTemplate>
                                                                <asp:TextBox ID="txtCurrentInvUnitPrice" runat="server"  style="width: 50%;"></asp:TextBox>
                                                            </EditItemTemplate>
                                                        </telerik:GridTemplateColumn>
                                                        <telerik:GridBoundColumn UniqueName="evExtendedPriceInvoice" HeaderText="Invoice Price Ext." Visible="true" ReadOnly="true">

                                                        </telerik:GridBoundColumn>
                                                        <telerik:GridEditCommandColumn CancelImageUrl="../Images/rejected.png" UpdateImageUrl="../Images/approved.png" ButtonType="ImageButton">
                                                            <ItemStyle CssClass ="action-customwidth"/>
                                                            
                                                        </telerik:GridEditCommandColumn>
                                                        <%--<telerik:GridButtonColumn ConfirmDialogType="RadWindow" ConfirmTitle="Delete" ButtonType="FontIconButton" CommandName="Delete" ImageUrl="../Images/delete_icon.png">

                                                        </telerik:GridButtonColumn>--%>
                                                        <telerik:GridButtonColumn ConfirmTextFormatString="Do you want to Delete this item?" ConfirmTextFields="Key" Text="Delete" ButtonType="ImageButton" UniqueName="Delete" CommandName="Delete" ImageUrl="../Images/delete_icon.png">

                                                        </telerik:GridButtonColumn>
                                                    </Columns>
                                                </MasterTableView>
                                            </telerik:RadGrid>
                                        <div class="telerik-bottom-action">
                                            <div class="action-right vocher-total-lbl">
                                            <asp:Label ID="lblTotalPrice" Text="Actual Total: " Visible="false" runat="server"></asp:Label>
                                            <asp:Label ID="totalPriceCal" runat="server"></asp:Label>
                                                </div>
                                        </div>                                        
                                        <div class="telerik-bottom-action">

                                            <div class="action-right">
                                                <div class="action-values">
                                                    <%--   <asp:Label ID="Label1" runat="server" CssClass="POConfirm_lblTrkError" Visible="false"></asp:Label>--%>
                                                    <asp:Label ID="lblVocherErrInfo" runat="server" CssClass="POConfirm_lblTrkError" Visible="false"></asp:Label>
                                                </div>
                                                <div class="action-values">

                                                    <asp:Button ID="btnVoucSubmit" runat="server" CssClass="button-fancy1" Text="Submit" ToolTip="Saves current changes and submit for processing."></asp:Button>
                                                    <asp:Button ID="btnVoucSave" runat="server" CssClass="button-fancy1" Visible="False" Enabled="False" Text="Save" ToolTip="Saves current changes but does not submit for processing."></asp:Button>
                                                    <asp:Button ID="btnVoucCancel" runat="server" CssClass="button-fancy1" Text="Cancel" ToolTip="Cancels changes"></asp:Button>
                                                    <%-- <asp:Button ID="Button1" runat="server" CssClass="button-fancy1" Text="Submit"></asp:Button>
                                                    <asp:Button ID="Button2" runat="server" CssClass="button-fancy1" Text="Cancel"></asp:Button>--%>
                                                </div>
                                            </div>

                                            <%-- <asp:Literal ID="ltlAlert" runat="server" EnableViewState="False"></asp:Literal>--%>
                                        </div>

                                    </telerik:RadPageView>
                                </telerik:RadMultiPage>
                            </telerik:RadAjaxPanel>

                            <telerik:RadAjaxLoadingPanel ID="RadAjaxLoadingPanel1" runat="server" Height="75px"
                                Width="75px" Transparency="25">
                                <img alt="Loading..." src="../load21.gif"
                                    style="border: 0;" />
                            </telerik:RadAjaxLoadingPanel>
                        </div>
                    </ContentTemplate>

                </asp:UpdatePanel>

                         <telerik:RadWindowManager ID="RadWindowManager6" runat="server">
                                            <Windows>
                                                <telerik:RadWindow ID="RadWindowShowPDF" runat="server" Modal="True" Width="1000px" Height="800px" Left="200px" Top="160px" CenterIfModal="false"
                                                    NavigateUrl="//www.sdizeus.com" Title="Proforma Invoice Report">
                                                </telerik:RadWindow>                                              
                                                   
                                            </Windows>
                                        </telerik:RadWindowManager>
                                   


            </td>
        </tr>
    </table>

    <script type="text/javascript">


        function CheckNumeric() {
            // Get ASCII value of key that user pressed

            var key
            if (window.event) key = window.event.keyCode;
            else if (e) key = e.which;
            else return;

            // Was key that was pressed a numeric character (0-9)?
            if (key == 46)
                return; // if so, do nothing
            else if (key > 47 && key < 58)
                return; // if so, do nothing
            else
                window.event.returnValue = null; // otherwise, 
            // discard character
        }

        function CheckTrckNo(strTrckno) {
            if (strTrckno.value == "") {
                alert("Tracking number cannot be blank.");
                strTrckno.focus();
                strTrckno.select();
                return false
            }
        }

        function CheckQTY(decTotQty, decRcvQty, decNewQty) {
            if (decNewQty.value > 0) {
                var newqty = decNewQty.value;
                var rcvqty = decRcvQty;
                var totqty = decTotQty;
                //alert("checkqty");
                //alert(Number(newqty));
                //alert(Number(rcvqty));
                //alert(Number(totqty));

                if ((Number(newqty) + Number(rcvqty)) > Number(totqty)) {
                    alert("Please Note! Quantity entered + Quantity already shipped\nis greater than Quantity Ordered.");
                    decNewQty.value = '';
                    decNewQty.focus();
                    decNewQty.select();
                    return false
                }
            }
        }
    </script>
    <script type="text/javascript">
        function DisableSubmitButton(btnSub,btnSubUniqID) {           
            document.getElementById('<%=btnhidden.ClientID()%>').click();
            return false;

        }  

        function CheckNumeric() {
            // Get ASCII value of key that user pressed

            var key
            if (window.event) key = window.event.keyCode;
            else if (e) key = e.which;
            else return;

            // Was key that was pressed a numeric character (0-9)?
            if (key == 46)
                return; // if so, do nothing
            else if (key > 47 && key < 58)
                return; // if so, do nothing
            else
                window.event.returnValue = null; // otherwise, 
            // discard character
        }
    </script>
    <script type="text/javascript">
        function OnClientTabSelectingHandler(sender, args) {
            //debugger;
            var tabStrip = $find("<%= tbStripPODetails.ClientID %>");
            var tab = tabStrip._selectedIndex;
            if (tab == 1) {
                var tabtext = tabStrip.get_allTabs()[1].get_text()
                var ctrl = document.getElementById('<%=lblCurrentTab.ClientID%>');
                ctrl.innerText = '*' + tabtext + '*';
            } else if (tab == 0) {
                var tabtext = tabStrip.get_allTabs()[0].get_text()
                var ctrl1 = document.getElementById('<%=lblCurrentTab.ClientID%>');
                ctrl1.innerText = '*' + tabtext + '*';
            } else if (tab == 2) {
                var tabtext = tabStrip.get_allTabs()[2].get_text()
                var ctrl2 = document.getElementById('<%=lblCurrentTab.ClientID%>');
                ctrl2.innerText = '*' + tabtext + '*';
            }
}
    </script>

    <style>
        .Body_Content {
            position: relative;
        }
    </style>

</asp:Content>
