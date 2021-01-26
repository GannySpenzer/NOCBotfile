<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="VoucherInfo2_brt.aspx.vb"
    Inherits="Insiteonline.VoucherInfo2_brt" Async="true" MasterPageFile="~/MasterPage/ISOLOnlineVendor.Master"%>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

           <style type="text/css">

        .SDI_Payment_From {
    margin-left: 20Px;
}
                .SDI_Payment_To {
    margin-left: 50Px;
}
         </style> 
     <script type="text/javascript" src="js/jquery-1.8.2.js"></script>
    <script type="text/javascript">
        $(function () {
            blinkeffect('#<%= lblLoadData.ClientID %>');
        })
        function blinkeffect(selector) {
            $(selector).fadeOut('slow', function () {
                $(this).fadeIn('slow', function () {
                    blinkeffect(this);
                });
            });
        }
    </script> 
    <script type="text/javascript">
        var on_color = "#FF0000";
        var off_color = "#FFFFFF";
        var blink_onoff = 1;
        var blinkspeed = 500;
        function blink() {

            if (blink_onoff == 1) {
                document.all.blink.style.color = on_color;
                blink_onoff = 0;
            }
            else {
                document.all.blink.style.color = off_color;
                blink_onoff = 1;
            }
        }
        function blinkLoading() {
            document.getElementById("<%=lblLoadData.ClientID %>").style.visibility = 'visible';
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label ID="lblNoPOS" CssClass="lblNoPOS_cont" runat="server" Visible="False"></asp:Label>
    <asp:Panel ID="Panel3" runat="server">
        <table class="Table3_voucher2_cont" id="Table3" cellpadding="1">

            <tr>
                <td>
                    <table id="Table1B" border="0" cellspacing="1" cellpadding="1" width="100%">
                        <tr>
                            <td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />
                                <telerik:RadGrid ID="Statgrid8" runat="server" CssClass="dtgConf_cont"
                                    AutoGenerateColumns="False" RegisterWithScriptManager="true"
                                    BorderStyle="None" CellPadding="3"
                                    Width="100%">
                                    <ItemStyle CssClass="displayfield"></ItemStyle>
                                    <HeaderStyle CssClass="Statgrid_HeaderStyle_cont"></HeaderStyle>
                                    <MasterTableView>
                                        <Columns>
                                            <telerik:GridBoundColumn DataFormatString="status"></telerik:GridBoundColumn>
                                        </Columns>
                                    </MasterTableView>
                                </telerik:RadGrid>
                                    <asp:Panel ID="Panel1" runat="server" BorderWidth="0px" BorderStyle="None" >
                                    <p>
                                        <asp:Label ID="Label1" runat="server" Font-Bold="true">From Invoice Date:</asp:Label>&nbsp;
												&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                        <asp:Label ID="PaymentFrom" class="SDI_Payment_From" runat="server" Font-Bold="true">From Payment Date:</asp:Label>&nbsp;
												&nbsp;&nbsp;
                                                <telerik:RadDatePicker ID="RadDatePickerFrom" runat="server" >
                                                    <Calendar ID="Calendar1" runat="server" FirstDayOfWeek="Monday" ShowRowHeaders="false">
                                                        <SpecialDays>
                                                            <telerik:RadCalendarDay Repeatable="Today" ItemStyle-BackColor="#D8D8D8">
                                                            </telerik:RadCalendarDay>
                                                        </SpecialDays>
                                                    </Calendar>


                                                </telerik:RadDatePicker>
                                        &nbsp;&nbsp;
                                         
                                                <telerik:RadDatePicker ID="RadPaymentFrom" runat="server" >
                                                    <Calendar ID="Calendar3" runat="server" FirstDayOfWeek="Monday" ShowRowHeaders="false">
                                                        <SpecialDays>
                                                            <telerik:RadCalendarDay Repeatable="Today" ItemStyle-BackColor="#D8D8D8">
                                                            </telerik:RadCalendarDay>
                                                        </SpecialDays>
                                                    </Calendar>
                                                </telerik:RadDatePicker>
                                        &nbsp;&nbsp;
												<%--<asp:Label ID="lblErrFromDate" runat="server" ForeColor="Red" Visible="False"></asp:Label>--%>
                                    </p>
                                    <p>&nbsp;</p>
                                    <p>
                                        <asp:Label ID="Label2" runat="server" Font-Bold="true">To Invoice Date:</asp:Label> 
												&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                         <asp:Label ID="PaymentTo" class="SDI_Payment_To" runat="server" Font-Bold="true">To Payment Date:</asp:Label> 
												&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                <telerik:RadDatePicker ID="RadDatePickerTo" runat="server">
                                                    <Calendar ID="Calendar2" runat="server" FirstDayOfWeek="Monday" ShowRowHeaders="false">
                                                        <SpecialDays>
                                                            <telerik:RadCalendarDay Repeatable="Today" ItemStyle-BackColor="#D8D8D8">
                                                            </telerik:RadCalendarDay>
                                                        </SpecialDays>
                                                    </Calendar>

                                                </telerik:RadDatePicker>
                                        &nbsp;&nbsp;
                                       
                                                <telerik:RadDatePicker ID="RadPaymentTo" runat="server">
                                                    <Calendar ID="Calendar4" runat="server" FirstDayOfWeek="Monday" ShowRowHeaders="false">
                                                        <SpecialDays>
                                                            <telerik:RadCalendarDay Repeatable="Today" ItemStyle-BackColor="#D8D8D8">
                                                            </telerik:RadCalendarDay>
                                                        </SpecialDays>
                                                    </Calendar>

                                                </telerik:RadDatePicker>
                                        &nbsp;&nbsp;
												<%--<asp:Label ID="lblErrToDate" runat="server" ForeColor="Red" Visible="False" ></asp:Label>&nbsp;&nbsp;&nbsp;&nbsp; --%>
                                        
                                    </p>
                                    <asp:Label ID="lblErr" runat="server" ForeColor="Red" Visible="False"></asp:Label>
                                    <p>(date range is limited to 3 months...keep date range smaller, to get faster results)</p>
												&nbsp;&nbsp;
                                    <p>
                                        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
												<asp:Button ID="btnSubmit" runat="server" CssClass="button-fancy1" Text="Submit" Visible="false"></asp:Button>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
												<asp:Label ID="lblErrdate" runat="server" ForeColor="Red" Visible="False"></asp:Label>
                                    </p>
                                </asp:Panel>
                            </td>
                        </tr>
                        <tr>
                            <!-- <td style="FONT-SIZE: 14pt; FONT-WEIGHT: bold">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
										<asp:Label ID="lblOR" runat="server">OR</asp:Label></td> -->
                        </tr>
                    </table>
                    <asp:Panel ID="Panel2" runat="server" BorderWidth="0px" BorderStyle="None">
                        <p>
                            <!-- <asp:Label ID="Label3" runat="server" CssClass="displaylabel">Search by</asp:Label>&nbsp;&nbsp; 
						         <asp:DropDownList ID="dropSearchBy" runat="server">
                                        <asp:ListItem Value="Invoice ID">Invoice ID</asp:ListItem>
                                        <asp:ListItem Value="Payment ID">Payment ID</asp:ListItem>
                                        <asp:ListItem Value="Voucher ID">Voucher ID</asp:ListItem>
                                    </asp:DropDownList>&nbsp;&nbsp;&nbsp;
                            -->
									<asp:Label ID="Label4" runat="server">Search By Invoice ID (optional)</asp:Label>&nbsp;
									
                        </p>
                        <p>
                            <asp:TextBox ID="txtSearchCriteria" Width="131px" runat="server"></asp:TextBox>
                        </p>
                        <br />
                        <asp:Label ID="lblPOSearch" runat="server">Search By PO ID (optional)</asp:Label>&nbsp;<br />
                        <asp:TextBox ID="txtPOSearch" Width="131px" runat="server"></asp:TextBox>

                        <p> <br /> <br />
                            &nbsp;&nbsp;&nbsp;
									<asp:Button ID="btnSubmit2" runat="server" CssClass="button-fancy1" Text="Submit"></asp:Button>&nbsp;&nbsp;&nbsp;
							<asp:Button ID="btnResetDate" runat="server" CssClass="button-fancy1" Text="Reset Date"></asp:Button>&nbsp;&nbsp;&nbsp;		
                            <asp:Label ID="lblErrCriteria" runat="server" ForeColor="Red" Visible="False"></asp:Label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
									<asp:CheckBox ID="chbDateRange" runat="server" CssClass="displaylabel" Text="Use date range entered above to limit search" Checked="true" Visible="false"></asp:CheckBox>
                        </p>
                    </asp:Panel>
                </td>
            </tr>

            <tr>
                <td class="Table3_voucher2_td_cont">
                    <asp:Label ID="lblSelectRpt" runat="server" ToolTip="Use Search Transactions to find the status of specific invoice's." Visible="false"
                        CssClass="displaylabel">
								<span class="tip">Select report (by invoice date)</span></asp:Label>&nbsp;&nbsp;
                    <asp:DropDownList ID="dropSelectReport" runat="server" Visible="false">
                        <asp:ListItem Value="X">Select Report</asp:ListItem>
                        <asp:ListItem Value="7">Last 7 Days</asp:ListItem>
                        <asp:ListItem Value="S-30">Next 30 days - 30-60 days</asp:ListItem>
                        <asp:ListItem Value="S-60">Next 30 days - 60-90 days</asp:ListItem>
                        <asp:ListItem Value="S-90">Next 30 days -90-120 days</asp:ListItem>
                        <asp:ListItem Value="S-120">Next 30 days - 120-150 days</asp:ListItem>
                        <asp:ListItem Value="S-150">Next 30 days - 150-180 days</asp:ListItem>
                        <asp:ListItem Value="S-180">Next 30 days - 180-210 days</asp:ListItem>
                        <asp:ListItem Value="S-210">Next 30 days - 210-240 days</asp:ListItem>
                        <asp:ListItem Value="15">Last 15 Days</asp:ListItem>
                        <asp:ListItem Value="30">Last 30 Days</asp:ListItem>
                        <asp:ListItem Value="0">This Month</asp:ListItem>
                        <asp:ListItem Value="-1">Last Month</asp:ListItem>
                        <asp:ListItem Value="3M">Last 3 Months</asp:ListItem>
                        <asp:ListItem Value="6M">Last 6 Months</asp:ListItem>
                        <asp:ListItem Value="1Y">Last 12 months</asp:ListItem>
                        <asp:ListItem Value="99" Selected="true">Search Transactions</asp:ListItem>
                    </asp:DropDownList>
                    &nbsp;
                    <asp:Button ID="btnGo" runat="server" CssClass="button-fancy1" Text="Go" Visible="false"></asp:Button>
                </td>
        
						<TD style="WIDTH: 356px; HEIGHT: 14px">
							<asp:CheckBox id="chbNoLimit" runat="server" Visible="False" CssClass="displaylabel" Text="Unlimited results"></asp:CheckBox>
                            &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </TD>
                    <TD style="HEIGHT: 14px" align="right">
							<asp:button id="btnExcel" runat="server" CssClass="button-fancy1" Text="Extract to Excel"></asp:button>
                    </TD>
            </tr>
           
            
        </table>
					
				<TABLE>	 
					<TR>
						<TD style="WIDTH: 457px; HEIGHT: 29px" vAlign="top" noWrap align="left">&nbsp;
							<asp:CheckBox style="Z-INDEX: 0" id="chkunpaidVoucher" runat="server" CssClass="displaylabel"
								Text="Unpaid Vouchers" AutoPostBack="True" Checked="True" Visible="false"></asp:CheckBox>
							<asp:CheckBox style="Z-INDEX: 0" id="chkPaidVoucher" runat="server" CssClass="displaylabel" Text="Paid vouchers" Checked="true"
								AutoPostBack="True" Visible="false"></asp:CheckBox></TD>
						<TD style="WIDTH: 356px"><FONT id="blink">
								<asp:Label id="lblLoadData" runat="server">Loading Data...</asp:Label></FONT></TD>
						<TD align="right">
							<asp:CheckBox id="chbFreeze" runat="server" CssClass="displaylabel" Text="Freeze grid headings (IE 5 and up)"
								AutoPostBack="True"></asp:CheckBox></TD>
					</TR>
				</TABLE>
				<TABLE id="Table1A" border="0" cellSpacing="1" cellPadding="1" width="100%">
					<TR>
						<TD>
				            <asp:label id="lblNoData"
					            runat="server" Font-Bold="true" ForeColor="Red"> There are no items to display.</asp:label>
                            <br />
                            <asp:TextBox ID="TextBox1" runat="server" Visible="False"></asp:TextBox>
                            <asp:TextBox ID="TextBox2" runat="server" Visible="False"></asp:TextBox>
                            <asp:TextBox ID="TextBox3" runat="server" Visible="False"></asp:TextBox>
                            <asp:TextBox ID="TextBox4" runat="server" Visible="False"></asp:TextBox>
                            <asp:TextBox ID="TextBox5" runat="server" Visible="False"></asp:TextBox>
                            <asp:TextBox ID="TextBox6" runat="server" Visible="False"></asp:TextBox>
                            <br />
                            <br />
                            <br />
                        <telerik:RadGrid ID="Statgrid" runat="server"  OnPreRender="Statgrid_PreRender"
                            RegisterWithScriptManager="true" BorderWidth="1"
                            BorderColor="#000040" BorderStyle="None"  Skin="Office2010Black"
							AutoGenerateColumns="False" AllowSorting="True" 
                            AlternatingItemStyle-BackColor="White" AllowFilteringByColumn="true" ShowFooter="true" ShowHeader="true" ShowStatusBar="true" AllowPaging="true">
                            <PagerStyle Position="TopAndBottom" Mode="NextPrevAndNumeric" PageSizeControlType="None" AlwaysVisible="true" ShowPagerText ="true"></PagerStyle>
                                <GroupingSettings CaseSensitive="false" />
                            <MasterTableView DataKeyNames="PYMNT_ID_REF">
                                <Columns>
                                    <telerik:GridBoundColumn DataField="REMIT_VENDOR" HeaderText="Vendor ID" 
                                        SortExpression="REMIT_VENDOR" UniqueName="REMIT_VENDOR">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="NAME1" HeaderText="Vendor Name">
                                          <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="PYMNT_ID" HeaderText="Payment ID" 
                                        SortExpression="PYMNT_ID" Visible="false">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="PYMNT_ID_REF" HeaderText="Paym. ID Ref." 
                                         Display="false">
                                          <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                        <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 

                                    <telerik:GridTemplateColumn HeaderText="Payment ID Ref." UniqueName="PaymentIDRef" SortExpression="PYMNT_ID_REF">
                                        <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                        <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                    <ItemTemplate>
                                        <%--<asp:HyperLink Text='<%# Container.dataitem("PYMNT_ID_REF") %>' runat="server" ID="lnk1payref" />--%>
                                        <asp:LinkButton ID="lnk1payref" runat="server" Text='<%# Container.dataitem("PYMNT_ID_REF") %>' OnClick="lnk1payref_Click"></asp:LinkButton>                                        
                                    </ItemTemplate>
                                    </telerik:GridTemplateColumn>


                                    <telerik:GridBoundColumn DataField="PYMNT_DATE" UniqueName="PayDate" HeaderText="Payment Date" 
                                        SortExpression="PYMNT_DATE">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 

                                    <telerik:GridBoundColumn DataField="PO_ID" HeaderText="PO Number" 
                                        SortExpression="PYMNT_ID" Visible="true">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 

                                <telerik:GridTemplateColumn DataField="VOUCHER_ID" HeaderText="Voucher ID" SortExpression="VOUCHER_ID" UniqueName="VOUCHERID">
                                    <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" ID="lbtnVOUCHERID" Text='<%# Container.DataItem("VOUCHER_ID")%>'></asp:LinkButton>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>
                                    <telerik:GridBoundColumn DataField="DUE_DATE" HeaderText="Due Date" 
                                        SortExpression="DUE_DATE" Visible="False">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="DSCNT_DUE_DATE" HeaderText="Discount Due Date" 
                                        SortExpression="DSCNT_DUE_DATE" Visible="False">
                                          <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                   
                                     <telerik:GridTemplateColumn HeaderText="Discount Amt." SortExpression="DSCNT_PAY_AMT">
                                        <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                        <ItemTemplate>
                                         <span> <%# FormatCurrency(Container.DataItem("DSCNT_PAY_AMT"), 2)%> </span>
                                          
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>

                                     <telerik:GridBoundColumn DataField="SCHEDULED_PAY_DATE" HeaderText="Scheduled Due Date" 
                                        SortExpression="SCHEDULED_PAY_DATE">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                     <telerik:GridTemplateColumn HeaderText="Paid Discount" SortExpression="PAID_AMT_DSCNT">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont" ></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                        <ItemTemplate>
                                        <span> <%# FormatCurrency(Container.DataItem("PAID_AMT_DSCNT"), 2)%>
                                            </span>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                                                            
                                    <telerik:GridTemplateColumn DataField="INVOICE_ID" HeaderText="Invoice ID" UniqueName="INVOICEID" SortExpression="INVOICE_ID">
                                    <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                    <ItemTemplate>
                                        <asp:LinkButton runat="server" ID="lbtnINVOICEID" Text='<%# Container.DataItem("INVOICE_ID")%>'></asp:LinkButton>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>                                                                       
                                     
                                    <telerik:GridBoundColumn DataField="INVOICE_DATE" UniqueName="INVOICE_DATE" HeaderText="Invoice Date" 
                                        SortExpression="INVOICE_DATE">
                                       <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    
                                    <telerik:GridBoundColumn DataField="VCHR_TTL_LINES" HeaderText="Voucher Lines">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 


                                    <telerik:GridBoundColumn DataField="GROSS_AMT" HeaderText="AmountOld" Visible="False">
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridTemplateColumn HeaderText="Amount" UniqueName="GROSS_AMT" SortExpression="GROSS_AMT">
                                     <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                    <ItemTemplate>
                                          <span> <%# FormatCurrency(Container.DataItem("GROSS_AMT"), 2)%> </span>
                                    </ItemTemplate>
                                </telerik:GridTemplateColumn>

                                    <telerik:GridBoundColumn DataField="PYMNT_STATUS" HeaderText="Payment Status">
                                          <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="DEPTID" HeaderText="Dept. ID" Visible="False">
                                        <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="ACCOUNT" HeaderText="AccountLn #" Visible="False">
                                        <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="DESCR254_MIXED" HeaderText="Descr" Visible="False">
                                         <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="MATCH_STATUS" HeaderText="Match Excp. Status">
                                        <HeaderStyle CssClass="dtgConf_Centeritem_cont"></HeaderStyle>
                                    <ItemStyle CssClass="dtgConf_Centeritem_cont"></ItemStyle>
                                   </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="Invoice_ID" HeaderText="INVOICE ID" Display="false" UniqueName="INVOICE_ID">
                                    </telerik:GridBoundColumn> 
                                    <telerik:GridBoundColumn DataField="BUSINESS_UNIT" Display="false" UniqueName="BUSINESS_UNIT"></telerik:GridBoundColumn>
                                </Columns>
                            </MasterTableView>                      
                                <HeaderStyle BorderStyle="None" BorderColor="#000040"
                                    BorderWidth="1px" ForeColor="White" Font-Bold="true" />

                                <FilterMenu EnableImageSprites ="false" BackColor ="#FBAA12"></FilterMenu>
                            <HeaderContextMenu BackColor="#525466"></HeaderContextMenu>
                    </telerik:RadGrid>                            
                        </TD>
					</TR>
					<tr>
						<td>
							<asp:textbox id="txtSortField" runat="server" Width="48px" Visible="False"></asp:textbox>
							<asp:textbox id="txtFromDate" runat="server" Width="56px" Visible="False"></asp:textbox>
							<asp:textbox id="txtToDate" runat="server" Width="40px" Visible="False"></asp:textbox>
                            &nbsp;&nbsp;&nbsp;&nbsp;
                            <br />
                            <br />
                        </td>
					</tr>
					



				</TABLE>
           <telerik:RadWindowManager ID="RadWindowManager1" runat="server">
                <Windows>
                    <telerik:RadWindow ID="RadWindowShowPDF" runat="server" Modal="True" Width="1028px" Height="495px" CenterIfModal="true" 
                        NavigateUrl="//www.sdizeus.com" Title="Invoice">
                    </telerik:RadWindow>
                </Windows>
           </telerik:RadWindowManager> 

        
	</asp:panel>
			<script type="text/javascript">
					if (document.all.blink) {
					document.all.blink.style.color = off_color;
					setInterval("blink()",blinkspeed); }
			</script>
	
		<script><asp:Literal id="ltlAlert" runat="server" EnableViewState="False"></asp:Literal></script>

                    <telerik:RadWindowManager ID="RadWindowManager2" runat="server">
                        <Windows>
                            <telerik:RadWindow ID="RadWindowPaymentID" runat="server" Modal="True" Width="1028px" Height="650px" CenterIfModal="true" 
                                    NavigateUrl="//www.sdizeus.com" Title="Payment Details" Behaviors="Close, Maximize">
                            </telerik:RadWindow>
                        </Windows>
                    </telerik:RadWindowManager>	
 
</asp:Content>