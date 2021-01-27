<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="VendorProfile.aspx.vb" Inherits="Insiteonline.VendorProfile" MasterPageFile="~/MasterPage/ISOLOnlineVendor.Master" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Profile Update</title>
    <script src="../js/GenJSFunc.js" type="text/javascript"></script>
    <style type="text/css">
        .style7 {
            text-align: right;
            width: 100px;
        }

        .vendor-width {
            width: 215px;
        }
    </style>

    <script type="text/javascript" language="Javascript"> 

        function OnClientCloseProfile(oWnd, args) {

            document.forms[0].submit();
        }
        function OnClientClose(oWnd, args) {

            document.forms[0].submit();
        }

    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label runat="server" ID="lblTestting"></asp:Label>
    <telerik:RadAjaxManager ID="RadAjaxManager1" runat="server"></telerik:RadAjaxManager>

    <table id="Table1" class="pfleUpdtate_table pfleUpdtate_table-tabradmenu" cellspacing="1"
        cellpadding="1">
        <tr>
            <td class="align_center">
                
                <asp:UpdatePanel ID="updPnlProfile" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="exampleWrapper">
                            <telerik:RadAjaxPanel ID="radAjaxPanel1" runat="server" EnableAJAX="true" LoadingPanelID="RadAjaxLoadingPanel1">
                                <telerik:RadTabStrip ID="tbStripUserDetails" runat="server" AutoPostBack="false" SelectedIndex="0"
                                    OnTabClick="tbStripUserDetails_TabClick" Align="Justify">
                                    <Tabs>
                                        <telerik:RadTab Text="User Detail" runat="server" PageViewID="rpvUserDetail" Value="UDTL" Selected="True">
                                        </telerik:RadTab>
                                        <telerik:RadTab Text="User Privileges" runat="server" PageViewID="rpvUserPrivilege"
                                            Value="UPVL">
                                        </telerik:RadTab>
                                      
                                    </Tabs>
                                </telerik:RadTabStrip>
                                <telerik:RadMultiPage ID="RadMultiPage1" runat="server" SelectedIndex="0">
                                    <telerik:RadPageView ID="rpvUserDetail" runat="server">
                                        <table id="Table3" class="pfleUpdtate_table pfleUpdtate_table-tabmenuCont" cellspacing="5"
                                            cellpadding="1">
                                            <tr>
                                                <td class="spacer vendor-width">
                                                    <asp:Label ID="Label_VendorID" runat="server" CssClass="displaylabel">Vendor ID</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:TextBox ID="TextBox_VendorId" runat="server" CssClass="userprofile-input" MaxLength="10" Visible="true" placeholder="Enter Vendor ID"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="Val_txtVendorID" CssClass="error-align" runat="server" ErrorMessage="Vendor ID is required"
                                                        ControlToValidate="TextBox_VendorId" Display="Dynamic"></asp:RequiredFieldValidator>
                                                    <asp:Button ID="btnAdd" runat="server" Visible="False" CssClass="button-fancy1 userprofile-btn" Text="ADD USER" CausesValidation="false"></asp:Button>
                                                    <asp:Button ID="btnEdit" runat="server" Visible="False" CssClass="button-fancy1  userprofile-btn" Text="Edit Mode" CausesValidation="false"></asp:Button>
                                                    <asp:Button ID="btnChangePassw" runat="server" CssClass="button-fancy1 move-right  userprofile-btn" Text="Change Password"></asp:Button>

                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="lblUserid" runat="server" CssClass="displaylabel" EnableViewState="False">User ID</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:TextBox ID="txtUserid" runat="server" CssClass="userprofile-input" MaxLength="30"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="valuserid1" CssClass="error-align" runat="server" ErrorMessage="User ID is required"
                                                        ControlToValidate="txtUserid"></asp:RequiredFieldValidator>
                                                    <asp:CustomValidator CssClass="invalid_errormsgCont" ID="valUserid2" runat="server"
                                                        ControlToValidate="txtUserid" Display="Dynamic" OnServerValidate="valUserid2_ServerValidate"></asp:CustomValidator>
                                                    <asp:Label ID="ErrorlabelUserId" Visible="false" runat="server"></asp:Label>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="Label_usrtype" runat="server" Visible="false" CssClass="displaylabel">User Type</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:RadioButtonList ID="radioUserType" runat="server" Visible="false" OnSelectedIndexChanged="radioUserType_SelectedIndexChanged" AutoPostBack="true">
                                                        <asp:ListItem Text="Vendor" Value="V" Selected="True"></asp:ListItem>
                                                    </asp:RadioButtonList>
                                                </td>
                                            </tr>
                                          
                                            <tr runat="server" id="tr_SelectUser_fields">

                                                <td class="spacer">
                                                    <asp:Label ID="lblSelectUser" runat="server" Visible="False" CssClass="displaylabel"
                                                        EnableViewState="False">Search User</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <table id="Table5" class="pfleUpdtate_table" cellspacing="1" cellpadding="1">
                                                        <tr>
                                                            <td class="pfleupdate_table5_td1" nowrap>
                                                                <asp:DropDownList ID="dropSelectUser" CssClass="userprofile-input" runat="server"
                                                                    Visible="False" AutoPostBack="True" Width="400px">
                                                                </asp:DropDownList>
                                                                <asp:Label ID="lblVendr" runat="server" Visible="False" CssClass="displaylabel NewTextAlignIE" EnableViewState="False">Vendor
                                                                </asp:Label>
                                                            </td>
                                                            <td class="align_Right" nowrap>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                                
                                                                <asp:Button ID="btnEditm" runat="server" CssClass="button-fancy1" Enabled="False"
                                                                    Text="Edit Moder" Visible="False" />                                                               
                                                            </td>
                                                        </tr>
                                                    </table>
                                                    <asp:DropDownList ID="drpuserAp" CssClass="userprofile-input" runat="server" Visible="False"
                                                        AutoPostBack="True">
                                                    </asp:DropDownList>
                                                    <asp:Label ID="lblempl" runat="server" Visible="False" CssClass="displaylabel" EnableViewState="False">  Employee</asp:Label>
                                                </td>
                                            </tr>

                                            <tr class="separation-line">
                                            </tr>

                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="Label_rolefield" Visible="False" runat="server" CssClass="displaylabel">Type</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:DropDownList CssClass="userprofile-input" Visible="False" ID="roleDropdownList" runat="server">
                                                        <asp:ListItem Value="ADMIN">Admin</asp:ListItem>
                                                        <asp:ListItem Value="USER">User</asp:ListItem>
                                                    </asp:DropDownList>
                                                    <asp:RequiredFieldValidator ValidationGroup="userProfileValid" ID="valType" CssClass="error-align" runat="server" ControlToValidate="roleDropdownList"
                                                        InitialValue="0" ErrorMessage="Please Select Type" Enabled="False" Display="Dynamic"></asp:RequiredFieldValidator>
                                                   
                                                </td>

                                            </tr>
                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="lblFirst" runat="server" CssClass="displaylabel" EnableViewState="False">First Name</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:TextBox ID="txtFirst" runat="server" CssClass="userprofile-input" MaxLength="30" placeholder="Enter First Name"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="valFirst" CssClass="error-align" runat="server" ErrorMessage="First Name is required"
                                                        ControlToValidate="txtFirst" Display="Dynamic"></asp:RequiredFieldValidator>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                    <%--<asp:Label ID="Error_LabelFname" runat="server" Visible="false" CssClass="error-align"></asp:Label>--%>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="lblLast" runat="server" CssClass="displaylabel" EnableViewState="False">Last Name</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:TextBox ID="txtLast" runat="server" CssClass="userprofile-input" MaxLength="30" placeholder="Enter Last Name"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="valLast" CssClass="error-align" runat="server" ErrorMessage="Last Name is required"
                                                        ControlToValidate="txtLast" Display="Dynamic"></asp:RequiredFieldValidator>
                                                    <%--<asp:Label ID="Error_LabelLname" CssClass="error-align" runat="server" Visible="false"></asp:Label>--%>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="lblEmail" runat="server" CssClass="displaylabel" EnableViewState="False">Email Address</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="userprofile-input" MaxLength="60" placeholder="Enter Email ID"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="valEmail1" CssClass="error-align" runat="server" ErrorMessage="Email is required"
                                                        ControlToValidate="txtEmail" Display="Dynamic"></asp:RequiredFieldValidator>
                                                    <asp:RegularExpressionValidator ID="valEmail2" CssClass="error-align" runat="server" ErrorMessage="Invalid Email Address"
                                                        ControlToValidate="txtEmail" Display="Dynamic" ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*([,;]\s*\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)*"></asp:RegularExpressionValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="lblPhone1" runat="server" CssClass="displaylabel" EnableViewState="False">Phone Number</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <telerik:RadMaskedTextBox CssClass="userprofile-input" ID="txtPhone" runat="server" Height="26px" LabelWidth="60px"
                                                        Mask="###-###-####" Rows="1">
                                                    </telerik:RadMaskedTextBox>
                                                    <asp:RequiredFieldValidator CssClass="error-align align-validation" ID="valPhone1" runat="server" ErrorMessage="Phone number is required"
                                                        ControlToValidate="txtPhone"></asp:RequiredFieldValidator><asp:RegularExpressionValidator
                                                            ID="valPhone2" runat="server" ErrorMessage="Invalid phone number" ControlToValidate="txtPhone"
                                                            Display="Dynamic" ValidationExpression="((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}"></asp:RegularExpressionValidator>
                                                </td>
                                            </tr>
                                          
                                            <tr runat="server" id="tr_pwd_fields">
                                                <td class="spacer">
                                                    <asp:Label ID="lblPassword" runat="server" Visible="True" CssClass="displaylabel"
                                                        EnableViewState="False">Password</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:TextBox ID="txtPassword" runat="server" Visible="False" CssClass="userprofile-input" placeholder="Enter Password"
                                                        MaxLength="10" TextMode="Password"></asp:TextBox>
                                                    <asp:RequiredFieldValidator ID="valPassword" CssClass="error-align" runat="server" ErrorMessage="Password is required"
                                                        ControlToValidate="txtPassword" Display="Dynamic" Enabled="False"></asp:RequiredFieldValidator><br>
                                                </td>
                                            </tr>
                                            <tr runat="server" id="tr_Cpwd_fields">
                                                <td class="spacer">
                                                    <asp:Label ID="lblConfirm" runat="server" Visible="True" CssClass="displaylabel"
                                                        EnableViewState="False">Confirm Password</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:TextBox ID="txtConfirm" runat="server" Visible="False" CssClass="userprofile-input"
                                                        MaxLength="10" TextMode="Password" placeholder="Enter Confirm Password"></asp:TextBox>
                                                    <asp:CompareValidator ID="valConfirm" runat="server" ErrorMessage="Invalid Confirm Password"
                                                        ControlToValidate="txtConfirm" Display="Dynamic" CssClass="error-align" Enabled="False" ControlToCompare="txtPassword"
                                                        Operator="Equal"></asp:CompareValidator>
                                                    <asp:RequiredFieldValidator ID="valConfirm2" runat="server" CssClass="error-align" ErrorMessage="Confirm Password required" ControlToValidate="txtConfirm" Enabled="False"></asp:RequiredFieldValidator>
                                                 
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="lblBusUnit" Visible="false" runat="server" CssClass="displaylabel" EnableViewState="False">Site</asp:Label>                                                    
                                                </td>
                                                <td class="spacer">
                                                    <asp:DropDownList CssClass="userprofile-input" Visible="true" ID="drpBUnit" runat="server">
                                                        <asp:ListItem Value="0">Select Site</asp:ListItem>
                                                        <asp:ListItem Value="ISA00">ISA00</asp:ListItem>
                                                        <asp:ListItem Value="SDM00">SDM00</asp:ListItem>                                                       
                                                    </asp:DropDownList>
                                                    <asp:RequiredFieldValidator ValidationGroup ="userProfileValid" ID="SiteBUValidator" CssClass="error-align" runat="server" ControlToValidate="drpBUnit"
                                                        InitialValue="0" ErrorMessage="Please Select Site" Enabled="False" Display ="Dynamic"></asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr runat="server" id="tr_Site_fields">
                                                <td class="spacer">
                                                    <asp:Label ID="lblGroup" runat="server" CssClass="displaylabel" EnableViewState="False" Visible="false">Site</asp:Label>
                                                </td>
                                                <td class="spacer">
                                                    <asp:PlaceHolder ID="PLGroup" runat="server" Visible="False"></asp:PlaceHolder>

                                                </td>
                                            </tr>
                                            <tr runat="server" id="tr_MultiSiteChk_fields">
                                                <td class="spacer"></td>
                                                <td class="pfleupdate_trcont_td2 multiside-access-field spacer">
                                                    <asp:CheckBox ID="MultiSiteChk" runat="server" Text="Enable Multisite access" Visible="false" OnCheckedChanged="MultiSiteChk_CheckedChanged" AutoPostBack="true" />
                                                    <asp:PlaceHolder ID="PLMultiSelect" runat="server"></asp:PlaceHolder>
                                                </td>
                                            </tr>

                                            <tr>
                                                <td class="spacer">
                                                    <asp:Label ID="lblAccountDisabled" runat="server" Visible="false" Font-Bold="true" ForeColor="Red"></asp:Label>
                                                    <asp:Button ID="btnActivateAccount" runat="server" Visible="false" Text="Activate Account" CssClass="button-fancy1" />
                                                    <asp:Button ID="btnInactivateAccount" runat="server" Visible="false" Text="Inactivate Account" CssClass="button-fancy1" />
                                                </td>
                                                <td class="spacer">
                                                    <asp:Label ID="lblEmplAccountDisabled" runat="server" Visible="false" Font-Bold="true" ForeColor="Red"></asp:Label>
                                                    <asp:Button ID="btnEmplActivateAccount" runat="server" Visible="false" Text="Activate Employee Account" CssClass="button-fancy1" />
                                                    <asp:Button ID="btnEmplInactivateAccount" runat="server" Visible="false" Text="Inactivate Employee Account" CssClass="button-fancy1" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_table4Cont" colspan="2">
                                                    <table id="Table4" class="pfleUpdtate_table" cellspacing="1" cellpadding="1">
                                                        <tr>
                                                            <td class="pfleupdate_table4Contemptytd" style="width: 19.5%"></td>
                                                            <td class="">
                                                                <asp:Button ID="btnCancel" runat="server" CssClass="button-fancy1 pflupdate_btncont userprofile-btn exit-btn"
                                                                    Text="Exit" CausesValidation="False"></asp:Button>
                                                                <asp:Button ID="btnSave" runat="server" CssClass="button-fancy1 pflupdate_btncont userprofile-btn"
                                                                    Text="Save"></asp:Button><asp:Label ID="lblMessage" CssClass="pfleupdate_lblMessage" runat="server"></asp:Label>
                                                                <asp:Button ID="btnAccess" runat="server" Visible="False" CssClass="button-fancy1 pflupdate_btncont"
                                                                    Text="Update Privileges"></asp:Button>
                                                                <asp:Button ID="btnApprovals" runat="server" Visible="False" CssClass="button-fancy1 pflupdate_btncont"
                                                                    Text="Update Approvals"></asp:Button>
                                                                <asp:Button ID="btnStatEml" runat="server" Visible="False" CssClass="button-fancy1 pfleupdate_btnStatEml"
                                                                    Text="Update Order Status  Emails"></asp:Button>&nbsp;&nbsp;
                                                                <asp:Button ID="btnVendPriv" runat="server" Visible="False" CssClass="button-fancy1 pflupdte_btnVendPriv"
                                                                    Text="Vendor Privleges"></asp:Button>
                                                            </td>

                                                        </tr>
                                                    </table>
                                                    <asp:RegularExpressionValidator CssClass="error-align" ID="regLen" runat="server"
                                                        ErrorMessage="Password must contain at least six characters!       .   " ControlToValidate="txtPassword"
                                                        ValidationExpression="\w{6,10}"></asp:RegularExpressionValidator>
                                                    <asp:RegularExpressionValidator ID="regNum" runat="server" CssClass="error-align" ErrorMessage="Password must contain at least 1 number and 1 letter - must start with a letter!"
                                                        ControlToValidate="txtPassword" ValidationExpression="[a-zA-Z]+\w*\d+\w*" Font-Bold="True"></asp:RegularExpressionValidator>
                                                </td>
                                            </tr>
                                          
                                        </table>
                                        <asp:Label ID="lblAction" runat="server" Visible="False"></asp:Label>
                                        <asp:Label ID="lblDBError" runat="server" Visible="False"></asp:Label>
                                        <asp:Label ID="lblVendor" runat="server" Visible="False" CssClass="error-align"></asp:Label>
                                        <asp:Label ID="lblMexico" runat="server" Visible="False"></asp:Label>
                                        <asp:Label ID="lblUserIDHide" runat="server" Visible="False"></asp:Label>
                                        <asp:Label ID="lblActiveStatusHide" runat="server" Visible="false"></asp:Label>
                                        <asp:Label ID="lblEmplActiveStatusHide" runat="server" Visible="false"></asp:Label>
                                        <asp:Label ID="lblCurrBUHide" runat="server" Visible="False"></asp:Label>
                                    </telerik:RadPageView>
                                    <telerik:RadPageView ID="rpvUserPrivilege" runat="server">
                                        <table id="Table6" class="approvals-Table1 approvals-Table1-approvals" cellspacing="1"
                                            cellpadding="1">
                                            <tr>
                                                <td>
                                                    <asp:Label ID="Label1" CssClass="pfleupdate_lblMessage" runat="server"></asp:Label>
                                                    <table id="Table8" class="pfleUpdtate_table pfleUpdtate_table-tabmenuCont" cellspacing="5" cellpadding="1">
                                                        <tr>
                                                            <td colspan="2">
                                                                <table id="Table9" class="pfleUpdtate_table" cellspacing="5"
                                                                    cellpadding="1">
                                                                    <tr>
                                                                        <td class="pfleupdate_table3_td1">
                                                                            <asp:Label ID="lbluser" runat="server" CssClass="displaylabel" EnableViewState="False">User</asp:Label>
                                                                        </td>
                                                                        <td class="pfleupdate_trcont_td2 NewText-Align">
                                                                            <asp:Label ID="lblUserVal" runat="server" CssClass="displaylabel" />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="pfleupdate_table3_td1">
                                                                            <asp:Label ID="lblUserGrouptab2" runat="server" CssClass="displaylabel" EnableViewState="false">User Group</asp:Label>
                                                                        </td>
                                                                        <td>
                                                                            <telerik:RadComboBox ID="rcbGroupTab2" runat="server" Filter="Contains" Width="30%"
                                                                                MaxHeight="150" EnableLoadOnDemand="false" AutoPostBack="true">
                                                                            </telerik:RadComboBox>
                                                                            <asp:TextBox ID="txtVendorUserGroup" runat="server" Visible="false" ReadOnly="true"
                                                                                BackColor="LightGray"></asp:TextBox>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="pfleupdate_table3_td1" valign="top">
                                                                            <asp:Label ID="lblPrivilegeType" runat="server" CssClass="displaylabel" EnableViewState="False">Privilege Type</asp:Label>
                                                                        </td>
                                                                        <td class="pfleupdate_trcont_td2">
                                                                            <asp:RadioButtonList ID="rblType" runat="server" RepeatDirection="Vertical" RepeatLayout="Flow"
                                                                                AutoPostBack="true" OnSelectedIndexChanged="rblType_SelectedIndexChanged">
                                                                                <asp:ListItem Text="Ala Carte" Value="Alacarte" Selected="True"></asp:ListItem>
                                                                                <asp:ListItem Text="Role" Value="Role"></asp:ListItem>
                                                                            </asp:RadioButtonList>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="pfleupdate_table3_td1" valign="top">
                                                                            <asp:Label ID="lblRole" runat="server" CssClass="displaylabel" EnableViewState="False">Role Name</asp:Label>
                                                                        </td>
                                                                        <td class="pfleupdate_trcont_td2">
                                                                            <asp:DropDownList ID="ddlUserRole" CssClass="pfleupdate_table5_dpdn" runat="server"
                                                                                OnSelectedIndexChanged="ddlUserRole_SelectedIndexChanged" AutoPostBack="True" Enabled="false">
                                                                            </asp:DropDownList>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td></td>
                                                                        <td>
                                                                            <asp:Button ID="btnExpandAll" runat="server" CssClass="button-fancy1" Text="Expand All" />
                                                                            <asp:Button ID="btnCollapseAll" runat="server" CssClass="button-fancy1" Text="Collapse All" />
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="pfleupdate_table3_td1" valign="top">
                                                                            <asp:Label ID="lblProgram" runat="server" CssClass="displaylabel" EnableViewState="False">Programs</asp:Label>
                                                                        </td>
                                                                        <td class="pfleupdate_trcont_td2">
                                                                            <telerik:RadTreeView ID="rtvPrograms" runat="server" Width="450px" Height="500px"
                                                                                CheckBoxes="true" TriStateCheckBoxes="true">
                                                                                <DataBindings>
                                                                                    <telerik:RadTreeNodeBinding Expanded="True"></telerik:RadTreeNodeBinding>
                                                                                </DataBindings>
                                                                            </telerik:RadTreeView>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td>
                                                                            <table id="Table10" class="pfleUpdtate_table" cellspacing="1" cellpadding="1">
                                                                                <tr>
                                                                                    <td class="style7">
                                                                                        <asp:Button ID="btnUserAccessSave" runat="server" CssClass="button-fancy1 pflupdate_btncont"
                                                                                            Text="Save"></asp:Button>
                                                                                    </td>
                                                                                    <td class="style7">
                                                                                        <asp:Button ID="btnUserAccessCancel" runat="server" CssClass="button-fancy1 pflupdate_btncont"
                                                                                            Text="Exit" CausesValidation="False"></asp:Button>
                                                                                    </td>
                                                                                </tr>
                                                                            </table>
                                                                        </td>
                                                                        <td style="vertical-align: bottom;">&nbsp;&nbsp;
                                                                                        <asp:Label ID="lblMessage1N" CssClass="pfleupdate_lblMessage" runat="server" Width="390px"></asp:Label>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td colspan="2">
                                                                            <asp:ValidationSummary CssClass="profile_ValidationSummary1Cont" ID="ValidationSummary2"
                                                                                runat="server"></asp:ValidationSummary>
                                                                        </td>
                                                                    </tr>
                                                                </table>
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                        </table>
                                    </telerik:RadPageView>
                                    <telerik:RadPageView ID="rpvApprovals" runat="server">
                                        <table class="approvals-Table1 approvals-Table1-approvals" id="Table7" cellspacing="1"
                                            cellpadding="1" width="100%" border="0">
                                            <tr>
                                                <td class="approvals-Table1-emptd2"></td>
                                                <td class="approvals-Table1-emptd3 approvals-Table3-td1">
                                                    <asp:TextBox ID="txtAppExist" runat="server" Visible="False"></asp:TextBox>
                                                </td>
                                                <td class="approvals-Table1-emptd4 approvals-Table3-td1"></td>
                                            </tr>
                                            <tr>
                                                <td class="approvals-Table1-td2">
                                                    <asp:Label ID="lblAppEmpID" runat="server" CssClass="displaylabel">Approval required by:</asp:Label>
                                                </td>
                                                <td class="approvals-Table1-td3">
                                                    <asp:DropDownList ID="DropAppEmpID" runat="server">
                                                    </asp:DropDownList>
                                                    &nbsp;
                                                </td>
                                                <td class="approvals-Table1-td4">
                                                    <asp:CheckBox ID="chbDelete" runat="server" Text="Delete"></asp:CheckBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="approvals-Table1-emptd2">&nbsp;
                                                </td>
                                                <td class="approvals-Table1-emptd3"></td>
                                                <td class="approvals-Table1-emptd4"></td>
                                            </tr>
                                            <tr>
                                                <td class="approvals-Table1-emptd2">
                                                    <asp:Label ID="lblAppTotal" runat="server" CssClass="displaylabel">Order total requiring approval:</asp:Label>
                                                </td>
                                                <td class="approvals-Table1-emptd3 NewText-Align">
                                                    <asp:TextBox ID="txtAppTotal" runat="server" onkeydown="return isNumberKey(event)"></asp:TextBox>
                                                    <asp:Label CssClass="approvals-lblSiteBaseCurrencyCode displaylabel" ID="lblSiteBaseCurrencyCode"
                                                        runat="server"></asp:Label>
                                                </td>
                                                <td class="approvals-Table1-emptd4"></td>
                                            </tr>
                                            <tr>
                                                <td class="approvals-Table1-emptd2">&nbsp;
                                                </td>
                                                <td class="approvals-Table1-emptd3"></td>
                                                <td class="approvals-Table1-emptd4"></td>
                                            </tr>
                                            <tr>
                                                <td class="approvals-Table1-emptd6"></td>
                                                <td class="approvals-Table1-emptd7">
                                                    <asp:Button ID="btnSubmit" runat="server" CssClass="button-fancy1" Text="Submit"></asp:Button>
                                                </td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="approvals-Table1-emptd6"></td>
                                                <td class="approvals-Table1-emptd8" colspan="2">
                                                    <asp:Label CssClass="approvals-lblMsg" ID="lblMsg" runat="server"></asp:Label>
                                                </td>
                                                <td></td>
                                            </tr>
                                        </table>
                                    </telerik:RadPageView>
                                    <telerik:RadPageView ID="rpvOrderStatusEmails" runat="server">
                                        <table class="AccOrdStatEml-Table1" id="Table2" cellspacing="1" cellpadding="1">
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-td3">
                                                    <asp:Label ID="lblOrdStatusEml" runat="server" CssClass="fieldlabel">Order Status Emails</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td2" colspan="4"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td2">
                                                    <asp:Label ID="lblSavedEml" runat="server" CssClass="displaylabel"> Saved</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td2">
                                                    <asp:CheckBox ID="chbSaved" runat="server"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td2"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td2"></td>
                                                <td class="AccOrdStatEml-Table1-td2"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td4">
                                                    <asp:Label ID="lblSubEml" runat="server" CssClass="displaylabel"> Submitted</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td4">
                                                    <asp:CheckBox ID="chbSubmitted" runat="server"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td4"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td4"></td>
                                                <td class="AccOrdStatEml-Table1-td4"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td7">
                                                    <asp:Label CssClass="AccOrdStatEml-lblMatRet displaylabel" ID="Label2" runat="server"
                                                        Visible="True">Received PO</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td7">
                                                    <asp:CheckBox CssClass="AccOrdStatEml-lblMatRet" ID="chbRecvdPo" runat="server"
                                                        Visible="True"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td7"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td7"></td>
                                                <td class="AccOrdStatEml-Table1-td7"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2">
                                                    <asp:Label ID="lblProcessing" runat="server" CssClass="displaylabel">Processing Order</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3">
                                                    <asp:CheckBox ID="chbProcOrd" runat="server"></asp:CheckBox>
                                                </td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2">
                                                    <asp:Label ID="lblOrdered" runat="server" CssClass="displaylabel"> Ordered</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3">
                                                    <asp:CheckBox ID="chbOrdered" runat="server"></asp:CheckBox>
                                                </td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2">
                                                    <asp:Label ID="lblPicking" runat="server" CssClass="displaylabel"> Picking</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3">
                                                    <asp:CheckBox ID="chbPicking" runat="server"></asp:CheckBox>
                                                </td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2">
                                                    <asp:Label ID="lblPartialPick" runat="server" CssClass="displaylabel" Visible="True">Partially Picked</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3">
                                                    <asp:CheckBox ID="chbParPicked" runat="server" Visible="True"></asp:CheckBox>
                                                </td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2">
                                                    <asp:Label ID="lblShipped" runat="server" CssClass="displaylabel" Visible="True">Shipped</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3">
                                                    <asp:CheckBox ID="chbShipped" runat="server" Visible="True"></asp:CheckBox>
                                                </td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td5">
                                                    <asp:Label ID="lblPickedOrder" runat="server" CssClass="displaylabel" Visible="True">Picked Order</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td5">
                                                    <asp:CheckBox ID="chbPickedOrder" runat="server" Visible="True"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td5"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td5"></td>
                                                <td class="AccOrdStatEml-Table1-td5"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td5">
                                                    <asp:Label ID="lblWbudgetApp" runat="server" CssClass="displaylabel" Visible="True">Waiting Budget Approval</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td5">
                                                    <asp:CheckBox ID="chbWaitBudApp" runat="server" Visible="false"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td5"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td5"></td>
                                                <td class="AccOrdStatEml-Table1-td5"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td6">
                                                    <asp:Label ID="lblCancelled" runat="server" CssClass="displaylabel" Visible="True">Cancelled</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td6">
                                                    <asp:CheckBox ID="chbcancelled" runat="server" Visible="True"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-td6"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td6">
                                                    <asp:Label ID="lblPODueDateChange" runat="server" CssClass="displaylabel" Visible="True">PO Due Date Change</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td6">
                                                    <asp:CheckBox ID="chbPODueDateChange" runat="server" Visible="True"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-td6"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2"></td>
                                                <td class="AccOrdStatEml-Table1-emptd3"></td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2">
                                                    <asp:Label ID="lblError" runat="server" CssClass="displaylabel" Visible="True">Error</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3">
                                                    <asp:CheckBox ID="chError" runat="server" Visible="True"></asp:CheckBox>
                                                </td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2">
                                                    <asp:Label ID="lblWaitingQuote" runat="server" CssClass="displaylabel" Visible="True">Waiting Quote</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3">
                                                    <asp:CheckBox ID="chbWaitingQuote" runat="server" Visible="True"></asp:CheckBox>
                                                </td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td6">
                                                    <asp:Label ID="lblOrdApprv" runat="server" CssClass="displaylabel" Visible="True">Waiting Order Approval</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td6">
                                                    <asp:CheckBox ID="chbWaitingOrdApp" runat="server" Visible="True"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-td6"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td7">
                                                    <asp:Label CssClass="AccOrdStatEml-lblMatRet displaylabel" ID="lblMatRet" runat="server"
                                                        Visible="True">Material Return</asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td7">
                                                    <asp:CheckBox CssClass="AccOrdStatEml-lblMatRet" ID="chbMaterialReturn" runat="server"
                                                        Visible="True"></asp:CheckBox>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td7"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td7"></td>
                                                <td class="AccOrdStatEml-Table1-td7"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td7"></td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td7">
                                                    <asp:Label CssClass="AccOrdStatEml-lblUpdMsg" ID="lblUpdMsg" runat="server"></asp:Label>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-td7"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td7"></td>
                                                <td class="AccOrdStatEml-Table1-td7"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2">
                                                    <asp:Button ID="btnOrdStatEmailSubmit" runat="server" CssClass="button-fancy1" Text="Submit"></asp:Button>
                                                </td>
                                                <td class="AccOrdStatEml-Table1-emptd3"></td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td>
                                                    <asp:TextBox CssClass="AccOrdStatEml-txtCustSrvFlag" ID="txtCustSrvFlag" runat="server"
                                                        Visible="False"></asp:TextBox>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2 AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-emptd3 AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-emptd4 AccOrdStatEml-Table1-td6"></td>
                                                <td class="AccOrdStatEml-Table1-td6"></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd1"></td>
                                                <td class="AccOrdStatEml-Table1-emptd2"></td>
                                                <td class="AccOrdStatEml-Table1-emptd3"></td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="AccOrdStatEml-Table1-emptd2"></td>
                                                <td class="AccOrdStatEml-Table1-emptd3">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                </td>
                                                <td></td>
                                                <td class="AccOrdStatEml-Table1-emptd4"></td>
                                                <td></td>
                                            </tr>
                                        </table>
                                    </telerik:RadPageView>
                                    <telerik:RadPageView ID="rpvPreferences" runat="server">
                                        <table id="Table12" class="pfleUpdtate_table pfleUpdtate_table-tabmenuCont" cellspacing="5"
                                            cellpadding="1">
                                            <tr>
                                                <td class="pfleupdate_table3td">
                                                    <asp:Label ID="lblProdDispTyp" runat="server" CssClass="displaylabel"
                                                        EnableViewState="False">Product Display Type</asp:Label>
                                                </td>
                                                <td>
                                                    <table id="Table13" class="pfleUpdtate_table" cellspacing="1" cellpadding="1">
                                                        <tr>
                                                            <td class="pfleupdate_table5_td1" nowrap>
                                                                <telerik:RadButton ID="rrbProdDispCatSQL" runat="server" ToggleType="Radio"
                                                                    ButtonType="ToggleButton" Text="Catalog" GroupName="ProdDispTyp">
                                                                </telerik:RadButton>
                                                                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                                <telerik:RadButton ID="rrbProdDispPSClient" runat="server" ToggleType="Radio"
                                                                    ButtonType="ToggleButton" Text="Client" GroupName="ProdDispTyp">
                                                                </telerik:RadButton>
                                                            </td>
                                                            <td class="align_Right" nowrap>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_table3_td1">
                                                    <asp:Label ID="lblBlockPrice" runat="server" CssClass="displaylabel"
                                                        EnableViewState="False">Block Price Display</asp:Label>
                                                </td>
                                                <td>
                                                    <asp:CheckBox ID="chkbxDisplayPrice" runat="server" Text="" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_table3_td1">
                                                    <asp:Label ID="lblShipto" runat="server" CssClass="displaylabel" Visible="false">ShipTo:</asp:Label></span>
                                                </td>
                                                <td>
                                                    <asp:DropDownList ID="dropShipto" runat="server" Visible="false"></asp:DropDownList></span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_table3_td1">
                                                    <asp:Label ID="lblUserDept" runat="server" CssClass="displaylabel" Visible="false">Department:</asp:Label></span>
                                                </td>
                                                <td>
                                                    <asp:DropDownList ID="drpDept" runat="server" Visible="false"></asp:DropDownList></span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_table3_td1"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_table3_td1"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_table3_td1"></td>
                                                <td></td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_lblUserid"></td>
                                                <td class="pfleupdate_table3_td3"></td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_lblPasswordCont"></td>
                                                <td class="pfleupdate_PasswordCont"></td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_cnfrmpwd"></td>
                                                <td class="pfleupdate_table3_td3"></td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_trcont_td1"></td>
                                                <td class="pfleupdate_trcont_td2"></td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_trcont_td1"></td>
                                                <td class="pfleupdate_trcont_td2">&nbsp;&nbsp;
                                                    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="pfleupdate_table4Cont" colspan="2">
                                                    <table id="Table14" class="pfleUpdtate_table" cellspacing="1" cellpadding="1">
                                                        <tr>
                                                            <td class="pfleupdate_table4Contemptytd"></td>
                                                            <td class="pfleupdate_savebtn">
                                                                <asp:Button ID="btnSubmitPrefs" runat="server" CssClass="button-fancy1 pflupdate_btncont"
                                                                    Text="Submit"></asp:Button>
                                                            </td>
                                                            <td class="pfleupdate_cancelbtn"></td>
                                                            <td class="pfleupdate_Accessbtn"></td>
                                                            <td class="pfleupdate_Approvalsbtn"></td>
                                                            <td class="align_left"></td>
                                                        </tr>
                                                    </table>
                                    </telerik:RadPageView>
                                    <telerik:RadPageView ID="rpvTangoSdiTracks" runat="server">
                                        <div class="rpvTangoSdiTracksConts">
                                            <asp:Label CssClass="TangoTitleLbl" Text="SDiTRACK" ID="lblTangoTitle" runat="server" />
                                            <br />
                                            <br />
                                            <ul>
                                                <li>
                                                    <asp:Label ID="lblSDiTrackCurrUser" runat="server" Text="Current User"></asp:Label>
                                                    <asp:Label ID="lblSDiTrackCurrUserVal" runat="server"></asp:Label>
                                                </li>
                                                <li>
                                                    <asp:Label ID="lblTangoUserName" runat="server" Text="SDiTrack User ID" />
                                                    <asp:TextBox Visible="true" ID="txtTangoUserName" runat="server" />
                                                    <asp:Label Visible="false" ID="lblTangoUserNameStored" runat="server" />
                                                </li>
                                                <li>
                                                    <asp:Label Visible="true" ID="lblTangoPassword" runat="server" Text="Password" />
                                                    <asp:TextBox Visible="true" ID="txtTangoPassword" TextMode="Password" runat="server" />
                                                </li>
                                                <li>
                                                    <asp:Label Visible="false" ID="lblSDiTrackDateTime" runat="server" Text="Added On" />
                                                    <asp:Label Visible="false" ID="lblSDiTrackDateTimeVal" runat="server" />
                                                </li>
                                                <li>
                                                    <asp:Label Visible="false" ID="lblSDiTrackGuid" runat="server" Text="SDiTrack GUID" />
                                                    <asp:Label Visible="false" ID="lblSDiTrackGuidVal" runat="server" />
                                                </li>
                                                <li>
                                                    <asp:Button Visible="true" CssClass="btnTangoAddUserCont" ID="btnTangoAddUser" runat="server" Text="Add To SDi Track" />
                                                    <asp:Label ID="lblValidation" ForeColor="Red" runat="server" />
                                                </li>
                                            </ul>
                                        </div>
                                        <br />
                                        <br />
                                        <br />
                                        <div class="rpvTangoSdiTracksConts">
                                            <asp:Label CssClass="TangoTitleLbl" Text="Add Other Users to SDiTrack" ID="lblAddOtherUsers" runat="server" />
                                            <br />
                                            <br />
                                            <ul>
                                                <li>
                                                    <asp:Label ID="lblSpaceFiller" runat="server"></asp:Label>
                                                    <asp:DropDownList ID="ddlSDiUsers" CssClass="pfleupdate_table5_dpdn" runat="server"
                                                        Visible="False" AutoPostBack="True" Width="400px">
                                                    </asp:DropDownList>
                                                </li>
                                                <li>
                                                    <asp:Label ID="lblOtherUserID" runat="server" Text="User ID"></asp:Label>
                                                    <asp:Label ID="lblOtherUserIDVal" runat="server"></asp:Label>
                                                </li>
                                                <li>
                                                    <asp:Label ID="lblOtherUserBU" runat="server" Text="Business Unit"></asp:Label>
                                                    <asp:Label ID="lblOtherUserBUVal" runat="server"></asp:Label>
                                                    <asp:Label ID="lblOtherUserNoBU" runat="server" Visible="false" ForeColor="Red"
                                                        Text="The user is not assigned to a valid BU."></asp:Label>
                                                </li>
                                                <li>
                                                    <asp:Label ID="lblOtherUserTangoUserName" runat="server" Text="SDiTrack User ID"
                                                        Visible="false" />
                                                    <asp:TextBox ID="txtOtherUserTangoUserNameVal" runat="server" Visible="false" />
                                                    <asp:Label ID="lblOtherUserTangoUserNameValStored" runat="server" Visible="false"></asp:Label>
                                                </li>
                                                <li>
                                                    <asp:Label ID="lblSpacing" runat="server" Text=" "></asp:Label>
                                                    <asp:Label ID="lblOtherUserBUMessage" runat="server" Visible="false" ForeColor="Red"
                                                        Text="The BU does not have an SDiTrack account."></asp:Label>
                                                </li>
                                                <li>
                                                    <asp:Label Visible="false" ID="lblOtherUserTangoPassword" runat="server" Text="Password" />
                                                    <asp:TextBox Visible="false" ID="txtOtherUserTangoPasswordVal" TextMode="Password" runat="server" />
                                                </li>
                                                <li>
                                                    <asp:Label Visible="false" ID="lblOtherUserAddedOn" runat="server" Text="Added On" />
                                                    <asp:Label Visible="false" ID="lblOtherUserAddedOnVal" runat="server" />
                                                </li>
                                                <li>
                                                    <asp:Label Visible="false" ID="lblOtherUserGUID" runat="server" Text="SDiTrack GUID" />
                                                    <asp:Label Visible="false" ID="lblOtherUserGUIDVal" runat="server" />
                                                </li>
                                                <li>
                                                    <asp:Button Visible="false" CssClass="btnTangoAddUserCont" ID="btnTangoAddOtherUser"
                                                        runat="server" Text="Add To SDi Track" />
                                                    <asp:Label ID="lblValidationOtherUser" ForeColor="Red" runat="server" />
                                                </li>
                                            </ul>
                                        </div>
                                    </telerik:RadPageView>
                                    <telerik:RadPageView ID="rpvMobility" runat="server">
                                        <table class="approvals-Table1 approvals-Table1-approvals mobility-grid" id="tblMob" cellpadding="1" cellspacing="1" width="100%" border="0">
                                            <tr>
                                                <td class="approvals-Table1-td2">
                                                    <asp:Label runat="server" ID="lblGrib" CssClass="displaylabel">Crib Identifier:</asp:Label>
                                                </td>
                                                <td class="approvals-Table1-td3">
                                                    <asp:DropDownList ID="ddlGrib" runat="server"></asp:DropDownList>
                                                    &nbsp;
                                                    <asp:RequiredFieldValidator ForeColor="Red" Display="Dynamic" ID="rfvGrib" runat="server" InitialValue="-- SELECT --" ErrorMessage="Please Select Crib" ControlToValidate="ddlGrib" ValidationGroup="GRIBIDENT"></asp:RequiredFieldValidator>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="approvals-Table1-emptd6"></td>
                                                <td class="approvals-Table1-emptd7">
                                                    <asp:Button ID="btnGribSubmit" runat="server" CssClass="button-fancy1" Text="Submit" OnClick="btnGribSubmit_Click" ValidationGroup="GRIBIDENT"></asp:Button>
                                                    <br />
                                                    <br />
                                                    <asp:Label ID="lblGRIBErr" runat="server" ForeColor="Red" Width="575px"></asp:Label>
                                                </td>
                                            </tr>

                                        </table>
                                    </telerik:RadPageView>
                                    <telerik:RadPageView ID="rpvZuse" runat="server">
                                        <table class="approvals-Table1 approvals-Table1-approvals mobility-grid" id="tblZues" cellpadding="1" cellspacing="1" width="100%" border="0">
                                            <tr>
                                                <td class="approvals-Table1-td2">
                                                    <asp:Label runat="server" ID="lblZeus" CssClass="displaylabel"></asp:Label>
                                                </td>
                                                <td class="approvals-Table1-td3">
                                                    <asp:CheckBox ID="cbxZeus" runat="server" Text="" />
                                                </td>
                                            </tr>
                                            <tr>
                                                <td class="approvals-Table1-emptd6"></td>
                                                <td class="approvals-Table1-emptd7">
                                                    <br />
                                                    <asp:Button ID="btnZuseSubmit" runat="server" CssClass="button-fancy1" Text="Submit" OnClick="btnZuseSubmit_Click"></asp:Button>
                                                    <br />
                                                    <br />
                                                    <asp:Label ID="lblZuseError" runat="server" Width="575px"></asp:Label>
                                                </td>
                                            </tr>

                                        </table>

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
                    <Triggers>
                        <asp:PostBackTrigger ControlID="btnAccess" />
                        <asp:PostBackTrigger ControlID="dropSelectUser" />
                        <asp:PostBackTrigger ControlID="btnSave" />
                        <asp:PostBackTrigger ControlID="btnOrdStatEmailSubmit" />
                        <asp:PostBackTrigger ControlID="btnSubmit" />
                        <asp:PostBackTrigger ControlID="btnVendPriv" />
                        <asp:PostBackTrigger ControlID="btnSubmitPrefs" />
                        <asp:PostBackTrigger ControlID="btnTangoAddUser" />
                        <asp:PostBackTrigger ControlID="btnTangoAddOtherUser" />
                    </Triggers>
                </asp:UpdatePanel>
            </td>
        </tr>
    </table>
    <script><asp:Literal id="ltlAlert" runat="server" EnableViewState="False"></asp:Literal></script>

    <script type="text/javascript">
        function GridCreated(sender, eventArgs) {
            //gets the main table scrollArea HTLM element  
            var scrollArea = document.getElementById(sender.get_element().id + "_GridData");
            var row = sender.get_masterTableView().get_selectedItems()[0];

            //if the position of the selected row is below the viewable grid area  
            if (row) {
                if ((row.get_element().offsetTop - scrollArea.scrollTop) + row.get_element().offsetHeight + 20 > scrollArea.offsetHeight) {
                    //scroll down to selected row  
                    scrollArea.scrollTop = scrollArea.scrollTop + ((row.get_element().offsetTop - scrollArea.scrollTop) +
                row.get_element().offsetHeight - scrollArea.offsetHeight) + row.get_element().offsetHeight;
                }
                    //if the position of the the selected row is above the viewable grid area  
                else if ((row.get_element().offsetTop - scrollArea.scrollTop) < 0) {
                    //scroll the selected row to the top  
                    scrollArea.scrollTop = row.get_element().offsetTop;
                }
            }
        }
    </script>

    <telerik:RadWindowManager ID="RadWindowManager1" runat="server" ReloadOnShow="true" VisibleStatusbar="false">
        <Windows>
            <telerik:RadWindow ID="RadWindowProfilePrivs" runat="server" Modal="true" OnClientClose="OnClientCloseProfile"
                NavigateUrl="//www.sdizeus.com" Width="1080px" Height="500px" Animation="FlyIn"
                Left="100px" Top="210px" CenterIfModal="false" VisibleStatusbar="false" Title="Update Access Privileges">
            </telerik:RadWindow>
        </Windows>
    </telerik:RadWindowManager>

    <telerik:RadWindowManager ID="RadWindowManager2" runat="server" ReloadOnShow="true" VisibleStatusbar="false">
        <Windows>
            <telerik:RadWindow ID="RadWindow1" runat="server" Modal="true" OnClientClose="OnClientClose"
                NavigateUrl="//www.sdizeus.com" Width="1080px" Height="500px" Animation="FlyIn"
                Left="100px" Top="210px" CenterIfModal="false" VisibleStatusbar="false" Title="Update Access Privileges">
            </telerik:RadWindow>
        </Windows>
    </telerik:RadWindowManager>

</asp:Content>
