<%@ Page Title="Role Master" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPage/SDIXMaster.Master"
    CodeBehind="RoleMaster.aspx.vb" Inherits="Insiteonline.RoleMaster" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .rgDataDiv {
            height: 477px !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label runat="server" ID="lblAction" Visible="false"></asp:Label>
    <div style="width: 100%;text-align: center;padding-top: 1%;">
        <asp:Label runat="server" Text="" Visible="false" ID="lblMsg" ForeColor="red"></asp:Label>
    </div>
    <div class="rolemaster-addbtncont">
        <asp:RadioButtonList ID="rblType" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
            <asp:ListItem Text="Customer" Value="Customer" Selected="True"></asp:ListItem>
            <asp:ListItem Text="Vendor" Value="Vendor"></asp:ListItem>
        </asp:RadioButtonList>

        <asp:Button ID="btnCpyRole" runat="server" CssClass="button-fancy1 Extract_to_Excel_cont"
            Text="Copy Role" style="margin-left: 1%;"></asp:Button>
        <asp:Button ID="btnAddRole" runat="server" CssClass="button-fancy1 Extract_to_Excel_cont"
            Text="Add Role"></asp:Button>        
    </div>
    <div class="rolemaster_Table1A">
        <table class="RoleMaster_Grid" id="Table1A" cellspacing="1" cellpadding="1">
            <tr>
                <td class="approval_rpt_grid_cont">
                    <asp:Panel ID="pnlGrid" runat="server">
                        <telerik:RadGrid CssClass="RoleMaster_Grid" ID="rgRoles" runat="server" AllowFilteringByColumn="true"
                            AutoGenerateColumns="False" AllowSorting="True" OnSortCommand="rgRoles_SortCommand"
                            OnItemCommand="rgRoles_ItemCommand" ClientSettings-Scrolling-AllowScroll="true" PageSize="10" 
                            AllowPaging="true" OnItemDataBound="rgRoles_ItemDataBound" OnNeedDataSource="rgRoles_NeedDataSource"
                            OnDeleteCommand="rgRoles_DeleteCommand">
                            <GroupingSettings CaseSensitive="false" />
                            <ItemStyle CssClass="displayfield"></ItemStyle>
                            <HeaderStyle CssClass="approvalrpt_header"></HeaderStyle>
                            <MasterTableView NoMasterRecordsText="No roles have been set up." DataKeyNames="ROLENUM, ROLENAME">                                
                                <Columns>
                                    <telerik:GridBoundColumn DataField="ROLENUM" UniqueName="ROLENUM" HeaderText="ROLE ID" SortExpression="ROLENAME" AllowFiltering="false">                                        
                                    </telerik:GridBoundColumn>                                    
                                    <telerik:GridBoundColumn DataField="ROLENAME" UniqueName="ROLENAME" HeaderText="ROLE NAME" SortExpression="ROLENAME">                                        
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="ROLETYPE" UniqueName="ROLETYPE" HeaderText="PORTAL" SortExpression="ROLETYPE">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="BUSINESS_UNIT" UniqueName="BUSINESS_UNIT" HeaderText="BUSINESS UNIT" SortExpression="BUSINESS_UNIT">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridBoundColumn DataField="ROLEPAGE" UniqueName="ROLEPAGE" HeaderText="LANDING PAGE" SortExpression="ROLEPAGE">
                                    </telerik:GridBoundColumn>
                                    <telerik:GridEditCommandColumn CancelImageUrl="../Images/rejected.png" UpdateImageUrl="../Images/approved.png" ButtonType="ImageButton">                        
                                    </telerik:GridEditCommandColumn>    
                                    <telerik:GridButtonColumn ConfirmTextFormatString="Do you want to delete role name '{0}'?" ConfirmTextFields="ROLENAME" Text="Delete" ButtonType="ImageButton" UniqueName="Delete" CommandName="Delete" ImageUrl="Images/delete_icon.png">                        
                                    </telerik:GridButtonColumn>
                                </Columns>
                            </MasterTableView>
                        </telerik:RadGrid>
                    </asp:Panel>
                    <asp:Panel ID="pnlDetails" runat="server">
                        <table id="Table1" class="pfleUpdtate_table" cellspacing="1" cellpadding="1">
                            <tr>
                                <td class="align_center">
                                    <asp:Label ID="lblMessage" CssClass="pfleupdate_lblMessage" runat="server"></asp:Label>
                                    <table id="Table2" class="pfleUpdtate_table2Cont" border="4" cellspacing="1" cellpadding="1">
                                        <tbody>
                                            <tr>
                                                <td>
                                                    <table id="Table3" class="pfleUpdtate_table" cellspacing="5" cellpadding="1">
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1">
                                                                <asp:Label ID="lblRoleID" runat="server" CssClass="displaylabel" Visible="false" EnableViewState="False">Role ID</asp:Label>
                                                            </td>
                                                            <td>
                                                                <asp:Label ID="lblVRoleID" runat="server" CssClass="pfleupdate_txtbox"></asp:Label>
                                                                <asp:HiddenField ID="hdnRoleID" runat="server" Value="0" />
                                                                <asp:HiddenField ID="hdnRoleName" runat="server" Value="0" />
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:Label ID="lblroleslst" runat="server" CssClass="displaylabel" Visible="false" EnableViewState="False">Roles List</asp:Label>
                                                            </td>
                                                            <td>
                                                                <telerik:RadComboBox runat="server" ID="rdRolesLst" Filter="Contains"  Width="210px" style="float: left;" 
                                                                    OnSelectedIndexChanged="rdRolesLst_SelectedIndexChanged" AutoPostBack="true">
                                                                </telerik:RadComboBox>
                                                                <asp:Label ID="lblerrorr_rolelst" runat="server" CssClass="pfleupdate_txtbox" ForeColor="Red" style="padding-left: 12px;"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:Label ID="lblRole" runat="server" CssClass="displaylabel" EnableViewState="False">Role Name</asp:Label>
                                                            </td>
                                                            <td>
                                                                <asp:TextBox ID="txtRole" runat="server" CssClass="pfleupdate_txtbox" MaxLength="30" AutoPostBack="true" OnTextChanged="txtRole_TextChanged"
                                                                    Style='text-transform: uppercase'></asp:TextBox>
                                                                <asp:Label ID="roleErrLbl" runat="server" CssClass="pfleupdate_txtbox" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:Label ID="lblPortalHdr" runat="server" CssClass="displaylabel" EnableViewState="false">Portal</asp:Label>
                                                            </td>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:Label ID="lblPortal" runat="server" CssClass="displaylabel" EnableViewState="true"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:Label ID="lblUserType" runat="server" CssClass="displaylabel" EnableViewState="false" Visible="false">UserType</asp:Label>
                                                            </td>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:RadioButtonList CssClass="usertype-list" ID="radioUserType" runat="server" Visible="false" OnSelectedIndexChanged="radioUserType_SelectedIndexChanged" AutoPostBack="true" RepeatDirection="Horizontal">
                                                                    <asp:ListItem Text="Customer" Value="Customer" Selected="True"></asp:ListItem>
                                                                    <asp:ListItem Text="SDI" Value="SDI"></asp:ListItem>                                                                    
                                                                </asp:RadioButtonList>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:Label ID="lblRoleDefaultPage" runat="server" CssClass="displaylabel" EnableViewState="false">Default Page</asp:Label>
                                                            </td>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:TextBox ID="txtRoleDefaultPage" runat="server" CssClass="pfleupdate_txtbox" MaxLength="30"></asp:TextBox>
                                                                <asp:Label ID="ltrErrDefaultPage" runat="server" CssClass="pfleupdate_txtbox" ForeColor="Red" Text="Invalid URL/Page" Visible="false"></asp:Label>
                                                            </td>
                                                              <td class="pfleupdate_table3_td1">

                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1">
                                                                <asp:Label ID="lblBU" runat="server" CssClass="displaylabel" EnableViewState="False">Business Unit</asp:Label>
                                                            </td>
                                                            <td>
                                                                <telerik:RadComboBox ID="rcbBU" runat="server" Filter="Contains" Width="65%"
                                                                    MaxHeight="150" EnableLoadOnDemand="false" style="float: left;">
                                                                </telerik:RadComboBox>
                                                                <telerik:RadComboBox ID="RdbxBU" Filter="Contains" CssClass="" runat="server" CheckBoxes="true" Width="350px"
                                                                 MaxHeight="200px" EnableCheckAllItemsCheckBox="true" Visible="false">
                                                                </telerik:RadComboBox>
                                                                <asp:Label ID="lblError_BU" runat="server" CssClass="pfleupdate_txtbox" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1">

                                                            </td>
                                                            <td>
                                                                <asp:CheckBox runat="server" Text=" Apply to all Sister Sites" ID="chkboxApplyallSisterBU" Visible="false" OnCheckedChanged="chkboxApplyallSisterBU_CheckedChanged" AutoPostBack="true"/>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table3_td1" valign="top">
                                                                <asp:Label ID="lblProgram" runat="server" CssClass="displaylabel" EnableViewState="False">Programs</asp:Label>
                                                            </td>
                                                            <td>
                                                                <asp:Button ID="btnExpandAll" runat="server" CssClass="button-fancy1" Text="Expand All" />
                                                                <asp:Button ID="btnCollapseAll" runat="server" CssClass="button-fancy1" Text="Collapse All" />
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td></td>
                                                            <td>
                                                                <telerik:RadTreeView ID="rtvPrograms" runat="server" Width="450px" Height="500px"
                                                                    CheckBoxes="true" TriStateCheckBoxes="true">
                                                                    <DataBindings>
                                                                        <telerik:RadTreeNodeBinding Expanded="True"></telerik:RadTreeNodeBinding>
                                                                    </DataBindings>
                                                                </telerik:RadTreeView>
                                                                <div>
                                                                      <asp:Label ID="lbLNodeValidation" ForeColor="Red" runat="server"></asp:Label>
                                                                </div>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="pfleupdate_table4Cont" colspan="2">
                                                                <table id="Table4" class="pfleUpdtate_table" cellspacing="1" cellpadding="1">
                                                                    <tr>
                                                                        <td class="pfleupdate_savebtn">
                                                                            <asp:Button ID="btnSave" runat="server" CssClass="button-fancy1 pflupdate_btncont"
                                                                                Text="Save"></asp:Button>
                                                                        </td>
                                                                        <td class="pfleupdate_cancelbtn">
                                                                            <asp:Button ID="btnCancel" runat="server" CssClass="button-fancy1 pflupdate_btncont"
                                                                                Text="Exit" CausesValidation="False"></asp:Button>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td align="center">
                    <asp:Label CssClass="NoMessage_cont" ID="lblNoMessage" runat="server" Visible="False"></asp:Label>
                </td>
            </tr>
        </table>
    </div> 
    <script><asp:Literal id="ltlAlert" runat="server" EnableViewState="False"></asp:Literal></script>    
</asp:Content>
