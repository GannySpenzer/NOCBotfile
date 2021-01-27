Imports Insiteonline.WebPartnerFunctions.WebPSharedFunc
Imports System.Drawing.Color
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports Insiteonline.ORDBData
Imports Insiteonline.SQLDBData
Imports System.Web.Mail
Imports Telerik.Web.UI
Imports System.Collections.Generic
Imports Insiteonline.clsAccessPrivileges
Imports System.Text
Imports System.Diagnostics
Imports System.Text.RegularExpressions

'***********************************************************************************************************************************************
'*  YA 20180629 Ticket 137359/Task 1316 disabling checkable property of inactive nodes in user priv panel to prevent unique constraint errors  *
'*  VR 10/10/2018 Ticket 129378/task 1403 - allow SDIEXCHANGE user inactivation in the Employee table  
'*  YA 20200120 Ticket SDI-5982 Modified buildgroupList query to display correct location                                                      *
'***********************************************************************************************************************************************

Partial Class Profile
    Inherits System.Web.UI.Page
    Public Const PARAM_RETURN_EXEMPT_FLAG As String = "EXEMPT_FLAG"
    Private Const m_cProdDispType_CatalogSQL As String = "C" ' Catalog SQL
    Private Const m_cProdDispType_PSClient As String = "P" ' PeopleSoft Oracle 
    Private Const m_cUserGroup_Vendor As String = "SUPPLIER"
    Private Const m_cUserGroup_Mexico As String = "MEXICO"

    Private m_sAppTotalOrig As String
    Private m_sAppEmpIDOrig As String
    Private m_sAppAltOrig As String
    Private UserIdSessionvalue As String
    Private intCount As Integer
    Private Page_Action As String

    Private m_sReq_AltAppr_Orig As String
    Private m_sReq_AltAppr_EmpID As String

    Private UserCreated As String = String.Empty
    Private AutoGenID As String = String.Empty
    Private rcbMultiSelect As RadComboBox = New RadComboBox
#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub
    Protected WithEvents ltlAlert As System.Web.UI.WebControls.Literal

    'Dim rcbGroup As RadComboBox = New RadComboBox

    Dim txtGroup As TextBox = New TextBox
    Dim txtGroupID As TextBox = New TextBox

    Dim valGroup As RequiredFieldValidator = New RequiredFieldValidator
    Dim rfvMultiBU As RequiredFieldValidator = New RequiredFieldValidator

    'NOTE: The following placeholder declaration is required by the Web Form Designer.
    'Do not delete or move it.
    Private designerPlaceholderDeclaration As System.Object

    Private Sub Page_CommitTransaction(sender As Object, e As EventArgs) Handles Me.CommitTransaction

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Protected Overrides Sub OnPreInit(ByVal e As EventArgs)
        MyBase.OnPreInit(e)
        'MasterPageFile = "~/MasterPage/SDIXMaster.Master" 
    End Sub

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Dim int As Integer = rcbMultiSelect.CheckedItems.Count()
            Session("SetClickValue_Usertype") = radioUserType.SelectedValue

            Call GetUserValues()

            Page_Action = Session("PageAction")


            If Page.IsPostBack = False Then
                Session("PageAction") = "EDIT"
                Session("ChangePWDEnabled") = ""
                Call GetUserValues()
                BuildDeptDropdown()
            End If

            Page_Action = Session("PageAction")

            Dim sddss As String = Page_Action

            Me.Title = "Profile"

            Session("SCREENNAME") = "Profile.aspx"


            Dim drpdownvalue As String = rcbdropSelectUser.SelectedValue
            Session("SelectUserDrpdwn") = drpdownvalue



        Catch ex2 As Exception

        End Try

        'Code for SDI Track Starts
        Dim oSDITrack As New clsSDITrack()
        Try
            ' Don't show SDiTrack for Vendor or Mexico Vendor
            If Not IsVendor() And Not IsMexicoVendor() Then
                If oSDITrack.IsAccountSDITrack And oSDITrack.IsPrivilegeSDITrack Then
                    tbStripUserDetails.FindTabByValue("TST").Visible = True
                Else
                    tbStripUserDetails.FindTabByValue("TST").Visible = False
                End If
            End If
        Catch ex As Exception
            lblValidation.Text = "SDiTrack Issue"
        End Try
        'Code for SDI Track Ends

        'Setting page title
        Dim lblTitle As Label = CType(Master.FindControl("lblTitle"), Label)
        Dim strSelectedGroupValue As String = ""

        lblTitle.Text = "Profile Update"
        RadMultiPage1.RenderSelectedPageOnly = True

        Me.lblempl.Visible = False
        Me.lblVendr.Visible = False

        Dim VendorQueryValue = Request.QueryString("VENDOR")
        Dim CustomerQueryValue = Request.QueryString("CUSTOMER")
        Dim SDIEmpId As String = Session("SDIEMP")
        Dim SDIUserId As String = Session("USERID")
        Dim SDIUserIdVP As String = Session("USERID_VP")

        If Session("SDIEMP") = "" Or Session("USERID") = "" Then
            Session.RemoveAll()
            Response.Redirect("default.aspx")
        End If

        rcbMultiSelect.ID = "rcbMultiSelect"
        rcbMultiSelect.CheckBoxes = True
        rcbMultiSelect.EnableCheckAllItemsCheckBox = True
        rfvMultiBU.ID = "rfvMultiBU"
        rcbMultiSelect.CssClass = "MultiselectChkbox"
        rfvMultiBU.ControlToValidate = "rcbMultiSelect"
        rfvMultiBU.ErrorMessage = "Select BU for Multi Site Access"
        rfvMultiBU.CssClass = "usergroup-field"
        PLMultiSelect.Controls.Add(rcbMultiSelect)
        Dim int1 As Integer = rcbMultiSelect.CheckedItems.Count()

        If MultiSiteChk.Checked Then
            rcbMultiSelect.Visible = True
        Else
            rcbMultiSelect.Visible = False
        End If

        turnvalidationoff()

        If Session("SDIEMP") = "CUST" Then
            MultiSiteChk.Visible = True
        End If

        If Session("USERTYPE") = "SUPER" And Not IsVendor() And Not IsMexicoVendor() Then
            If Page.IsPostBack Then
                strSelectedGroupValue = rcbGroup.SelectedValue
            End If
            'rcbGroup.ID = "rcbGroup"
            'rcbGroup.EnableViewState = True
            'rcbGroup.Width = rcbGroupTab2.Width
            'rcbGroup.MaxHeight = rcbGroupTab2.MaxHeight
            'rcbGroup.Filter = RadComboBoxFilter.Contains            
            'PLGroup.Controls.Add(rcbGroup)
            Me.drpBUnit.Visible = False
            Me.lblBusUnit.Visible = False
            Label_usrtype.Visible = True
            radioUserType.Visible = True
            Label_rolefield.Visible = True
            roleDropdownList.Visible = True
            txtUserid.MaxLength = 10
        Else
            If Not IsVendor() And Not IsMexicoVendor() Then
                Me.lblVendr.Visible = False
                Me.lblempl.Visible = False
            Else
                'Me.lblVendr.Visible = True
                'Me.lblempl.Visible = True
            End If
            txtGroup.EnableViewState = True
            txtGroup.Width.Pixel(160)
            txtGroupID.EnableViewState = True
            txtGroupID.Visible = False
            PLGroup.Controls.Add(txtGroup)
            PLGroup.Controls.Add(txtGroupID)
            Label_usrtype.Visible = True

            If Session("USERTYPEVALUE") = "S" Then
                If Session("ROLE") = "ADMIN" Or Session("ROLE") = "USER" Or Session("ROLE") = "CORPADMIN" Then
                    If Page_Action = "EDIT" Then
                        radioUserType.Visible = True
                        radioUserType.Enabled = False
                        roleDropdownList.Visible = True
                        roleDropdownList.Enabled = False
                        lblSelectUser.Visible = False
                        'dropSelectUser.Visible = False
                        rcbdropSelectUser.Visible = False
                        btnAdd.Visible = False
                    End If
                Else
                    radioUserType.Visible = True
                    roleDropdownList.Visible = True
                End If
            End If

            Label_rolefield.Visible = True
            txtUserid.MaxLength = 10
            Me.drpBUnit.Visible = True
            Me.lblBusUnit.Visible = True
            lblGroup.Visible = False
            MultiSiteChk.Visible = False

        End If
        If Session("USERTYPEVALUE") = "C" Then
            If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                If Page_Action = "ADD" Then
                    If radioUserType.SelectedValue = "C" Then
                        rcbGroup.EnableViewState = False
                        Label_usrtype.Visible = False
                        MultiSiteChk.Visible = True
                        MultiSiteChk.Enabled = True
                        lblBusUnit.Visible = False
                        drpBUnit.Visible = False
                        lblGroup.Visible = True
                        rcbMultiSelect.Enabled = True
                    ElseIf Session("USERTYPEVALUE") = "C" Then
                        'rcbGroup.ID = "rcbGroup"
                        'rcbGroup.EnableViewState = True
                        'rcbGroup.Width = rcbGroupTab2.Width
                        'rcbGroup.MaxHeight = rcbGroupTab2.MaxHeight
                        'rcbGroup.Filter = RadComboBoxFilter.Contains
                        'PLGroup.Controls.Add(rcbGroup)
                        rcbGroup.Visible = True
                        buildGroupList(rcbGroup)
                    End If
                End If
            ElseIf Session("ROLE") = "USER" Then
                If Page_Action = "ADD" Then
                    If radioUserType.SelectedValue = "C" Or radioUserType.SelectedValue = "S" Then
                        'rcbGroup.ID = "rcbGroup"
                        'rcbGroup.EnableViewState = True
                        'rcbGroup.Width = rcbGroupTab2.Width
                        'rcbGroup.MaxHeight = rcbGroupTab2.MaxHeight
                        'rcbGroup.Filter = RadComboBoxFilter.Contains
                        'PLGroup.Controls.Add(rcbGroup)
                        rcbGroup.Visible = True
                    End If
                End If
            End If
        ElseIf Session("USERTYPEVALUE") = "S" Then
            If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                If Page_Action = "ADD" Then
                    If radioUserType.SelectedValue = "C" Then
                        lblGroup.Visible = True
                        MultiSiteChk.Visible = True
                        lblBusUnit.Visible = False
                        drpBUnit.Visible = False
                        Label_usrtype.Visible = False
                    End If
                End If
            ElseIf Session("ROLE") = "USER" Then
                If Page_Action = "ADD" Then
                    If radioUserType.SelectedValue = "C" Or radioUserType.SelectedValue = "S" Then
                        'rcbGroup.ID = "rcbGroup"
                        'rcbGroup.EnableViewState = True
                        'rcbGroup.Width = rcbGroupTab2.Width
                        'rcbGroup.MaxHeight = rcbGroupTab2.MaxHeight
                        'rcbGroup.Filter = RadComboBoxFilter.Contains
                        'PLGroup.Controls.Add(rcbGroup)
                        rcbGroup.Visible = True
                    End If
                End If
            End If
        End If

        If Session("USERTYPE") = "CORPADMIN" Then
            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
        Else
            If roleDropdownList.SelectedValue = "CORPADMIN" Then
                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            End If
        End If

        UserIdSessionvalue = Session("UserIDStoredValue")
        Dim Countvalue As Integer
        If Page.IsPostBack Then
            Dim strSiteGroupValuedrpdn As String = rcbGroup.SelectedValue
            If strSiteGroupValuedrpdn <> "" Then
                If UserIdSessionvalue IsNot Nothing Then
                    Countvalue = Session("CountIncrement")
                    If Countvalue = 0 Then
                        Session.Remove("UserIDStoredValue")
                        Session("CountIncrement") += 1
                        btnSave_Click(btnSave, EventArgs.Empty)
                    Else
                        Session.Remove("UserIDStoredValue")
                        UserIdSessionvalue = String.Empty
                        Session.Remove("CountIncrement")
                    End If
                End If
            ElseIf txtGroupID.Text <> "" Then
                If Session("USERTYPEVALUE") = "C" Or Session("USERTYPEVALUE") = "S" Then
                    If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                        If UserIdSessionvalue IsNot Nothing Then
                            Countvalue = Session("CountIncrement")
                            If Countvalue = 0 Then
                                Session.Remove("UserIDStoredValue")
                                Session("CountIncrement") += 1
                                btnSave_Click(btnSave, EventArgs.Empty)
                            Else
                                Session.Remove("UserIDStoredValue")
                                UserIdSessionvalue = String.Empty
                                Session.Remove("CountIncrement")
                            End If
                        End If
                    End If
                End If
            End If
        End If

        If Page.IsPostBack Then
            m_sAppTotalOrig = Session("APPR_TOTAL")
            m_sAppEmpIDOrig = Session("APPR_APR_EMPID")
            m_sAppAltOrig = Session("APPR_APR_ALT")

            If strSelectedGroupValue <> "" Then
                If Not rcbGroup.FindItemByValue(strSelectedGroupValue) Is Nothing Then
                    rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(strSelectedGroupValue).Index
                End If
            End If
        Else
            WebLog()

            If Session("PUNCHIN") = "YES" Then
                btnChangePassw.Visible = False
                If IsAscend(Session("BUSUNIT")) Or Insiteonline.VoucherSharedFunctions.VoucherClass.IsEnergizer(Session("BUSUNIT")) Then
                    tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                Else
                    tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                End If
            End If
            lblAction.Text = Page_Action
            lblVendor.Text = Request.QueryString("VENDOR")
            lblMexico.Text = Request.QueryString("MEXICO")

            If Page_Action = "EDIT" Then
                lblGroup.Visible = False
                btnChangePassw.Visible = False
                lblSelectUser.Visible = True
                'dropSelectUser.Visible = True
                rcbdropSelectUser.Visible = True
                btnAdd.Visible = True
                lblSwitch.Visible = False
                buildSelectDropDown()
                txtUserid.ReadOnly = True
                txtUserid.BackColor = LightGray
                valuserid1.Enabled = False
                valUserid2.Enabled = False
                tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = True
                tbStripUserDetails.Tabs.FindTabByValue("MOB").Visible = True
                btnAccess.Visible = False
                If Session("PUNCHIN") = "YES" Then
                    If IsAscend(Session("BUSUNIT")) Or Insiteonline.VoucherSharedFunctions.VoucherClass.IsEnergizer(Session("BUSUNIT")) Then
                        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                    Else
                        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                    End If
                Else
                    tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                End If
                Select Case Trim(Session("APPRTYPE"))
                    Case "O", "D", "M"

                        tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                    Case Else

                        tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                End Select
                If Session("USERTYPE") = "ADMINR" Then

                    tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                    tbStripUserDetails.FindTabByValue("TST").Visible = False
                End If
                Label_VendorID.Visible = False
                'TextBox_VendorId.Visible = False
                'Val_txtVendorID.Visible = False
                If clsAccessPrivileges.IsPrivilegeOn(Session("USERID"), Session("BUSUNIT"), clsAccessPrivileges.UserPrivsEnum.ZeusAnalytical, "GOZEUS") Then

                    tbStripUserDetails.Tabs.FindTabByValue("ZEUS").Visible = True
                Else
                    tbStripUserDetails.Tabs.FindTabByValue("ZEUS").Visible = False
                End If


            ElseIf Page_Action = "ADD" Then
                lblGroup.Visible = False
                setuppasswordfields("SETUP")
                lblSelectUser.Visible = False
                btnEdit.Visible = False
                lblSwitch.Visible = False
                tbStripUserDetails.FindTabByValue("TST").Visible = False
                tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = False
                btnAccess.Visible = False

                If Session("PUNCHIN") = "YES" Then
                    If IsAscend(Session("BUSUNIT")) Or Insiteonline.VoucherSharedFunctions.VoucherClass.IsEnergizer(Session("BUSUNIT")) Then
                        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                    Else
                        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                    End If
                Else : tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True

                End If

                If Request.QueryString("VENDOR") = "NO" Then
                    If CustomerQueryValue = "NO" OrElse Nothing Then
                        lblSelectUser.Visible = False
                        radioUserType.SelectedIndex = 0
                    End If
                End If

                If Request.QueryString("CUSTOMER") = "YES" Then
                    txtUserid.ReadOnly = True
                    txtUserid.BackColor = LightGray
                    valuserid1.Enabled = False
                    valUserid2.Enabled = False
                    MultiSiteChk.Visible = True
                End If

                If Request.QueryString("VENDOR") = "YES" Then
                    PLGroup.Visible = False
                    lblGroup.Visible = False
                    radioUserType.SelectedIndex = 1
                    Label_VendorID.Visible = True
                    'TextBox_VendorId.Visible = True
                    'Val_txtVendorID.Visible = True
                    lblSelectUser.Visible = False
                    MultiSiteChk.Visible = False
                    txtUserid.ReadOnly = True
                    txtUserid.BackColor = LightGray
                Else
                    If Session("CurrentValueOfUserTypeField") <> Nothing Then
                        radioUserType.SelectedValue = Session("CurrentValueOfUserTypeField")
                        Label_VendorID.Visible = False
                        'TextBox_VendorId.Visible = False
                        ''Val_txtVendorID.Visible = False
                    Else
                        radioUserType.SelectedIndex = 0
                        Label_VendorID.Visible = False
                        'TextBox_VendorId.Visible = False
                        'Val_txtVendorID.Visible = False
                    End If
                End If
                Select Case Trim(Session("APPRTYPE"))
                    Case "O", "D", "M"
                        'btnApprovals.Visible = True
                        tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                    Case Else
                        'btnApprovals.Visible = False
                        tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                End Select
                If Session("USERTYPE") = "ADMINR" Then
                    tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                    tbStripUserDetails.FindTabByValue("TST").Visible = False
                End If
                roleDropdownList.Items.Insert(0, New ListItem("Select Type", "0"))
            Else
                lblSelectUser.Visible = True
                'TextBox_VendorId.Visible = False
                txtUserid.ReadOnly = True
                txtUserid.BackColor = LightGray
                valuserid1.Enabled = False
                valUserid2.Enabled = False
                lblGroup.Visible = False
                MultiSiteChk.Visible = False
                buildEditUser(Session("USERID"))
                buildSelectDropDown()
                If Session("PUNCHIN") = "YES" Then
                    If IsAscend(Session("BUSUNIT")) Or Insiteonline.VoucherSharedFunctions.VoucherClass.IsEnergizer(Session("BUSUNIT")) Then
                        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                    Else
                        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                    End If
                Else
                    tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                End If
            End If


            If Session("USERTYPE") = "SUPER" Then
                If IsVendor() Then
                    txtGroupID.Text = "0"
                    txtGroup.Text = m_cUserGroup_Vendor
                    txtGroup.ReadOnly = True
                    txtGroup.BackColor = LightGray
                    btnAccess.Visible = False
                    tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                    tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                    tbStripUserDetails.FindTabByValue("TST").Visible = False
                ElseIf IsMexicoVendor() Then

                    txtGroupID.Text = "0"
                    txtGroup.Text = m_cUserGroup_Mexico
                    txtGroup.ReadOnly = True
                    txtGroup.BackColor = LightGray
                    btnAccess.Visible = False
                    drpBUnit.Visible = False
                    lblBusUnit.Visible = False
                    tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                    tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                    tbStripUserDetails.FindTabByValue("TST").Visible = False
                Else
                    buildGroupList(rcbGroup)
                    'buildGroupList(rcbGroupSites)
                    tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                End If
            ElseIf Session("USERTYPE") = "ADMIN" Or _
                Session("USERTYPE") = "ADMINX" Or _
                Session("USERTYPE") = "ADMINR" Or _
                Session("USERTYPE") = "CORPADMIN" Then
                Dim intGroupid As String = getGroupID(Session("USERID"))
                txtGroupID.Text = intGroupid.ToString
                txtGroup.Text = getUserGroupsName(intGroupid)
                txtGroup.ReadOnly = True
                txtGroup.BackColor = LightGray
                tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
            Else
                tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = False
                tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                tbStripUserDetails.FindTabByValue("TST").Visible = False
            End If
        End If

        If Not IsVendor() And Page_Action = "ADD" Then
            ' For Add New User or Add New Mexico, display only the User Information tab.
            tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = False
            tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
            tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
            tbStripUserDetails.Tabs.FindTabByValue("PREF").Visible = False
            tbStripUserDetails.Tabs.FindTabByValue("TST").Visible = False

            'lblSelectUser.Visible = True
            'lblSelectUser.Text = "Add Profile"

            setuppasswordfields("SETUP")
        End If
        If IsVendor() Or IsMexicoVendor() Then
            tbStripUserDetails.Tabs.FindTabByValue("PREF").Visible = False
        End If

        If Not Page.IsPostBack Then
            If Page_Action = "EDIT" Then
                If Session("USERTYPEVALUE") = "C" Then
                    If Session("ROLE") = "CORPADMIN" Then
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    End If
                    buildEditUser(Session("USERID"))
                End If
            End If
        End If

        Dim sss As String = Session("ROLE")
        Dim ee As String = Session("USERTYPEVALUE")

        If Not Page.IsPostBack Then
            If Session("USERTYPEVALUE") = "S" Then ''SDI Employee
                If Page_Action = "EDIT" Then
                    valuserid1.Enabled = False
                    'userid_Regex_validation.Enabled = False
                    SDIUsers()
                End If
            ElseIf Session("USERTYPEVALUE") = "V" Then ''Vendor

            Else ''Customer
                If Page_Action = "EDIT" Then
                    CustomersUsers()
                End If
            End If
        End If


        ' check/auto-select logged in user amongst the list (if exist)
        '   - erwin 2009.09.22
        If Not Page.IsPostBack Then
            Dim sId As String = CStr(Session("USERID")).Trim

            If sId.Length > 0 And Me.rcbdropSelectUser.Items.Count > 0 Then
                Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=sId))
                buildEditUser(rcbdropSelectUser.SelectedValue)
                'buildEditUser(sId)
                If Session("PUNCHIN") = "YES" Then
                    btnChangePassw.Visible = False
                Else
                    btnChangePassw.Visible = True
                End If
            End If
        End If

        If Me.IsPostBack Then
            txtPassword.Attributes("value") = txtPassword.Text
            txtConfirm.Attributes("value") = txtConfirm.Text
        End If

        If Session("USERTYPEVALUE") = "S" Then
            If Session("ROLE") = "SUPER" Then
                If Page_Action = "ADD" Then
                    If radioUserType.SelectedValue = "V" Then
                        tr_vendorId_fields.Style.Remove("display")
                        tr_BU_unit_field.Style.Remove("display")
                        Label_VendorID.Visible = True
                        lblBusUnit.Visible = True
                        drpBUnit.Visible = True
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    ElseIf radioUserType.SelectedValue = "C" Then
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    Else
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    End If
                Else
                    lblPassword.Visible = False
                    lblConfirm.Visible = False
                    If radioUserType.SelectedValue = "C" Then
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    Else
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    End If
                End If
            End If
        ElseIf Session("USERTYPEVALUE") = "C" Then
            If Session("ROLE") = "USER" Then
                btnAdd.Visible = False
                btnEdit.Visible = False
            End If
            If Session("ROLE") = "CORPADMIN" Then
                If roleDropdownList.SelectedValue = "CORPADMIN" Then
                    PLGroup.Visible = False
                    rcbGroup.Visible = True
                    MultiSiteChk.Visible = False
                    rcbMultiSelect.Visible = False
                Else
                    If Page_Action = "EDIT" Then
                        If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        Else
                            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        End If
                    End If
                    PLGroup.Visible = False
                    rcbGroup.Visible = True
                    MultiSiteChk.Visible = False
                    rcbMultiSelect.Visible = False
                End If
            End If
            If Session("ROLE") = "ADMIN" Then
                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                If MultiSiteChk.Checked Then
                    rcbMultiSelect.Visible = True
                Else
                    rcbMultiSelect.Visible = False
                End If
            End If
        End If
        If Session("USERTYPEVALUE") = "C" Then
            If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                If Page_Action = "EDIT" Then
                    'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
                    Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))
                End If
            End If
        End If

        If Session("USERTYPE") = "CORPADMIN" Then
            If Page_Action = "ADD" Then
                tr_Multiselect_fields.Style.Add("display", "none")
                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            Else
                tr_Multiselect_fields.Style.Add("display", "none")
                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            End If
        End If

        If Not Page.IsPostBack Then
            If Session("ZEUSNOCATALOGSITE") = "Y" Then
                'Table13.Visible = False
                rrbProdDispCatSQL.Visible = False
                rrbProdDispPSClient.Visible = False
                lblProdDispTyp.Visible = False
            End If
        End If

        If Session("USERTYPEVALUE") = "S" Then
            If Session("USERTYPE") = "SUPER" Then
                If radioUserType.SelectedValue = "S" Then
                    If Page_Action = "ADD" Then
                        lblDept.Visible = True
                    End If
                End If
            End If
        ElseIf Session("USERTYPEVALUE") = "C" Then
            tr_Dept_details_fields.Style.Add("display", "none")
        End If
        If radioUserType.SelectedValue = "V" Or roleDropdownList.SelectedValue = "CORPADMIN" Then
            MultiSiteChk.Visible = False
            PLMultiSelect.Visible = False
        Else
            MultiSiteChk.Visible = True
            PLMultiSelect.Visible = True
        End If

        Dim int3 As Integer = rcbMultiSelect.CheckedItems.Count()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        If Page_Action = "ADD" Then
            Response.Redirect("Profile.aspx")
        Else
            Response.Redirect(Session("DEFAULTPAGE").ToString())
        End If
    End Sub

    Private Sub btnSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Dim int3 As Integer = rcbMultiSelect.CheckedItems.Count()
        Dim strSiteGroupValuedrpdn As String = rcbGroup.SelectedValue
        Session("StoreRCBSessionValue") = strSiteGroupValuedrpdn
        Dim sssdd As Integer = Session("CountIncrement")

        'Dim VendorCombobox As String = RadcomboforVendorID.SelectedValue
        Dim VendorCombobox As String = Trim(MainvndrID.Text)

        If Page_Action = "EDIT" Then
            If Session("USERTYPEVALUE") = "S" Then
                If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                    lblSelectUser.Visible = False
                    'dropSelectUser.Visible = False
                    rcbdropSelectUser.Visible = False
                    Label_usrtype.Visible = False
                    radioUserType.Visible = False
                ElseIf Session("ROLE") = "USER" Then
                    btnAdd.Visible = False
                End If
            End If
            If Session("USERTYPEVALUE") = "C" Then
                If Session("ROLE") = "USER" Then
                    btnAdd.Visible = False
                    btnEdit.Visible = False
                End If
            Else
                If Session("ROLE") = "USER" Then
                    btnAdd.Visible = False
                Else
                    btnAdd.Visible = True
                End If
            End If
            btnEdit.Visible = False
            lblSelectUser.Visible = True
            'dropSelectUser.Visible = True
            rcbdropSelectUser.Visible = True
        ElseIf Page_Action = "ADD" Then
            If Session("USERTYPEVALUE") = "S" Then
                If Session("ROLE") = "" Then
                    lblGroup.Visible = True
                    rcbGroup.Visible = False
                    rcbGroupTab2.Visible = False
                    rcbGroupTab2.EnableViewState = False
                    rcbGroup.EnableViewState = False
                    MultiSiteChk.Visible = True
                End If

            End If
            btnAdd.Visible = False
            btnEdit.Visible = True
            lblSelectUser.Visible = False
            'dropSelectUser.Visible = False
            rcbdropSelectUser.Visible = False
            lblPassword.Visible = True
            lblConfirm.Visible = True
        End If
        If Session("USERTYPEVALUE") = "S" Then
            If Session("ROLE") = "USER" Then
                lblSelectUser.Visible = False
                'dropSelectUser.Visible = False
                rcbdropSelectUser.Visible = False
            End If
        End If

        lblMessage.Text = ""

        valFirst.Enabled = True
        valLast.Enabled = True
        valType.Enabled = True

        valEmail1.Enabled = True
        valEmail2.Enabled = True

        valuserid1.Enabled = False

        valGroup.Enabled = True
        valPhone1.Enabled = True
        valPhone2.Enabled = True
        Val_VendorID.Enabled = False
        If Page_Action = "ADD" Then
            valUserid2.Enabled = False
            valPassword.Enabled = True
            valConfirm.Enabled = True
            valConfirm2.Enabled = True

        End If

        If radioUserType.SelectedValue = "C" Then
            txtUserid.ReadOnly = True
            txtUserid.BackColor = LightGray
            valuserid1.Enabled = False
            valUserid2.Enabled = False
            lblGroup.Enabled = False
            'Val_txtVendorID.Enabled = False
            lblBusUnit.Visible = False
            drpBUnit.Visible = False
        ElseIf radioUserType.SelectedValue = "V" Then
            'Val_txtVendorID.Enabled = True
            'valuserid1.Enabled = False
            SiteBUValidator.Enabled = True
            Val_VendorID.Enabled = True
        End If


        If Page_Action = "ADD" Then
            If radioUserType.SelectedValue = "C" Or radioUserType.SelectedValue = "S" Then
                If Session("ROLE") = "SUPER" Then
                    Dim Site_Flag As String = GetFlagValue(rcbGroup.SelectedValue)
                    If Site_Flag = "V" Then
                        txtUserid.ReadOnly = False
                        txtUserid.BackColor = White
                        txtUserid.Enabled = True
                        'userid_Regex_validation.Enabled = True
                        valuserid1.Enabled = True
                        valUserid2.Enabled = True
                    Else
                        'userid_Regex_validation.Enabled = False
                        valuserid1.Enabled = False
                    End If
                ElseIf Session("ROLE") = "ADMIN" Then
                    rcbGroup.Visible = False
                    If Session("Flag_AddUser") = "V" Then
                        txtUserid.ReadOnly = False
                        txtUserid.BackColor = White
                        txtUserid.Enabled = True
                        'userid_Regex_validation.Enabled = True
                        valuserid1.Enabled = True
                        valUserid2.Enabled = True
                    ElseIf Session("ROLE") = "CORPADMIN" Then
                        rcbGroup.Visible = True
                    Else
                        'userid_Regex_validation.Enabled = False
                        valuserid1.Enabled = False
                    End If
                End If
            End If
        ElseIf Page_Action = "EDIT" Then
            'userid_Regex_validation.Enabled = False
            valuserid1.Enabled = False
        End If

        Page.Validate()
        Dim drpvalues As String = Convert.ToString(roleDropdownList.SelectedItem)

        Dim strUserGroup As String
        Dim strBU As String
        GetSelectedBUandGroup(strBU, strUserGroup)

        If strBU = "0" Then
            If radioUserType.SelectedValue = "V" Then
                SiteBUValidator.Enabled = True
            Else
                Error_LabelSite.Text = "Error - Invalid BU - check productview id's!"
                Error_LabelSite.Visible = True
                SiteBUValidator.Enabled = False
            End If

            'ltlAlert.Text = strMessage.Say("Error - Invalid BU - check productview id's!")
            Exit Sub
        ElseIf strBU.Trim.Length = 0 Then
            Error_LabelSite.Text = "Please select a valid User Group (business unit)."
            Error_LabelSite.Visible = True
            'ltlAlert.Text = strMessage.Say("Please select a valid User Group (business unit).")
            Exit Sub
        ElseIf MultiSiteChk.Checked = True Then
            If rcbMultiSelect.CheckedItems.Count = 0 Then
                If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                    Error_LabelSite.Text = "Please select a business unit to access the multi site."
                    Error_LabelSite.Visible = True
                    'ltlAlert.Text = strMessage.Say("Please select a business unit to access the multi site.")
                    Exit Sub
                End If
            End If
        Else
            Error_LabelSite.Visible = False
        End If
        If IsValid = True Then
            If Page_Action = "ADD" Or Page_Action = "EDIT" Then
                If radioUserType.SelectedValue = "V" Then

                    If Trim(MainvndrID.Text) <> "" Then
                        Dim ChkVendorID As String = Chk_VendrID(Trim(MainvndrID.Text))
                        If ChkVendorID <> "" Then
                            Error_VendorID.Text = "Vendor Name : " + ChkVendorID
                            Error_VendorID.ForeColor = Color.Green
                            Error_VendorID.Visible = True
                        Else
                            Error_VendorID.Text = "Entered Vendor ID is invalid"
                            Error_VendorID.ForeColor = Color.Red
                            Error_VendorID.Visible = True
                            Exit Sub
                        End If
                    Else
                        Error_VendorID.Text = "Please enter Vendor ID"
                        Error_VendorID.ForeColor = Color.Red
                        Error_VendorID.Visible = True
                        Exit Sub
                    End If
                Else
                    If Page_Action = "ADD" Then
                        Dim UserIDExits As Boolean = ExistsUserid()
                        If UserIDExits = True Then
                            Page.Validate()
                            Exit Sub
                        Else
                            CheckUserID_Val.Visible = False
                            CheckUserID_Val.Text = ""
                        End If
                    End If
                End If
            End If
        End If


        If IsValid Then
            Dim Flag_Value As String
            If Page_Action = "ADD" Then
                If Session("USERTYPEVALUE") = "S" Then
                    If Session("ROLE") = "SUPER" Then
                        Flag_Value = GetFlagValue(rcbGroup.SelectedValue)
                        If Flag_Value = "V" Then ''Verified by entering user id
                            Dim bool As Boolean = Verify_UserID(rcbGroup.SelectedValue)

                            If bool = True Then

                            Else

                            End If

                        ElseIf Flag_Value = "A" Then ''Automatic

                        Else ''Manually Verified
                            If radioUserType.SelectedValue = "C" Or radioUserType.SelectedValue = "S" Then
                                If UserIdSessionvalue IsNot Nothing Then

                                Else
                                    Session("Fname_TxtboxValue") = Trim(txtFirst.Text)
                                    Session("Lname_TxtboxValue") = Trim(txtLast.Text)
                                    ManuallyVerified_Flags(rcbGroup.SelectedValue)
                                    Exit Sub
                                End If
                            End If
                        End If
                    Else
                        Flag_Value = Session("Flag_AddUser")
                        If Flag_Value = "V" Then ''Verified by entering user id

                        ElseIf Flag_Value = "A" Then ''Automatic

                        Else ''Manually Verified
                            If radioUserType.SelectedValue = "C" Or radioUserType.SelectedValue = "S" Then
                                If UserIdSessionvalue IsNot Nothing Then

                                Else
                                    Session("Fname_TxtboxValue") = Trim(txtFirst.Text)
                                    Session("Lname_TxtboxValue") = Trim(txtLast.Text)
                                    ManuallyVerified_Flags(txtGroupID.Text)
                                    Exit Sub
                                End If
                            End If
                        End If
                    End If
                Else
                    If radioUserType.SelectedValue <> "V" Then
                        If Session("Flag_AddUser") = "A" Then

                        ElseIf Session("Flag_AddUser") = "V" Then

                        Else
                            If UserIdSessionvalue IsNot Nothing Then

                            Else
                                Session("Fname_TxtboxValue") = Trim(txtFirst.Text)
                                Session("Lname_TxtboxValue") = Trim(txtLast.Text)
                                ManuallyVerified_Flags(txtGroupID.Text)
                                Exit Sub
                            End If
                        End If
                    End If
                End If
            End If
        End If

        If IsValid Then
            Dim zzz = UserIdSessionvalue
            ProfileUpdate()
            turnvalidationoff()
        End If
        If Page_Action = "ADD" Then
            If Session("USERTYPE") = "CORPADMIN" Then
                GetSisterSitesBU(Session("BUSUNIT"))
            End If

            If Session("USERTYPEVALUE") = "S" Then
                If Session("USERTYPE") = "SUPER" Then
                    If radioUserType.SelectedValue = "S" Then
                        lblDept.Visible = True
                    End If
                End If
            End If
        Else
            If Session("USERTYPEVALUE") = "C" Then
                If Session("ROLE") = "CORPADMIN" Then
                    If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    Else
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    End If
                End If
            End If
        End If

        If Session("USERTYPE") = "SUPER" Then
            If Page_Action = "EDIT" Then
                If radioUserType.SelectedValue = "S" Then
                    lblSelectUser.Visible = True
                    roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    lblDept.Visible = True
                ElseIf radioUserType.SelectedValue = "V" Then
                    lblSelectUser.Visible = True
                    Label_VendorID.Visible = True
                    tr_BU_unit_field.Style.Remove("display")
                    drpBUnit.Visible = True
                    lblBusUnit.Visible = True
                    Radcombobox_vendorID()
                    'RadcomboforVendorID.Visible = True
                    MainvndrID.Visible = True
                    lblGroup.Visible = False
                    rcbGroup.Visible = False
                    MultiSiteChk.Visible = False
                    roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    rcbMultiSelect.Visible = False
                Else
                    lblSelectUser.Visible = True
                    roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    lblDept.Visible = False
                End If
            Else

            End If
        ElseIf Session("USERTYPE") = "ADMIN" Then
            If Session("USERTYPEVALUE") = "S" Then
                drpBUnit.Visible = False
                lblBusUnit.Visible = False
                lblSelectUser.Visible = True
                'dropSelectUser.Visible = True
                rcbdropSelectUser.Visible = True
                lblGroup.Visible = True
                btnAdd.Visible = True
                lblPassword.Visible = False
                lblConfirm.Visible = False
                radioUserType.Visible = False
                Label_usrtype.Visible = False
                tr_PwdFields.Style.Add("display", "none")
                tr_CpwdFields.Style.Add("display", "none")
                tr_BU_unit_field.Style.Add("display", "none")
                MultiSiteChk.Visible = True
                rcbGroup.Visible = False
                If Page_Action = "ADD" Then
                    lblGroup.Visible = True
                    rcbGroup.Visible = False
                    MultiSiteChk.Visible = True
                End If
                If roleDropdownList.SelectedValue = "CORPADMIN" Then
                    MultiSiteChk.Visible = False
                End If
            ElseIf Session("USERTYPEVALUE") = "C" Then
                If Page_Action = "EDIT" Then
                    CustomersUsers()
                Else

                End If
            End If
        ElseIf Session("USERTYPE") = "USER" Then
            If Session("USERTYPEVALUE") = "S" Then
                lblSelectUser.Visible = False
                'TextBox_VendorId.Visible = False
                lblBusUnit.Visible = False
                drpBUnit.Visible = False
                lblSelectUser.Visible = False
                'dropSelectUser.Visible = False
                rcbdropSelectUser.Visible = False
                Label_usrtype.Visible = False
                radioUserType.Visible = False
                lblGroup.Visible = True
                btnAdd.Visible = False
                lblPassword.Visible = False
                lblConfirm.Visible = False
                rcbGroup.Visible = False
                tr_PwdFields.Style.Add("display", "none")
                tr_CpwdFields.Style.Add("display", "none")
                tr_BU_unit_field.Style.Add("display", "none")
                tr_selectuser_fields.Style.Add("display", "none")
                ''tr_Multiselect_fields.Style.Add("display", "none")
            ElseIf Session("USERTYPEVALUE") = "C" Then
                If Page_Action = "EDIT" Then
                    CustomersUsers()
                Else

                End If
            End If
        ElseIf Session("USERTYPE") = "CORPADMIN" Then
            If Page_Action = "EDIT" Then
                CustomersUsers()
                rcbGroup.Visible = True
                MultiSiteChk.Visible = False
                rcbMultiSelect.Visible = False
                If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                Else
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                End If
            Else

            End If
        End If
        If Page_Action = "EDIT" Then
            btnAdd.Visible = True
            btnEdit.Visible = False
            If Session("USERTYPEVALUE") = "S" Then
                If Session("USERTYPE") = "ADMIN" Then
                    If Session("USERID") = rcbdropSelectUser.SelectedValue Then
                        lblDept.Visible = True
                        rcbDept.Visible = True
                    Else
                        lblDept.Visible = False
                        rcbDept.Visible = False
                    End If
                ElseIf Session("USERTYPE") = "USER" Then
                    lblDept.Visible = True
                    rcbDept.Visible = True
                    btnAdd.Visible = False
                    btnEdit.Visible = False
                End If
            ElseIf Session("USERTYPEVALUE") = "C" Then
                If Session("USERTYPE") = "USER" Then
                    btnAdd.Visible = False
                    btnEdit.Visible = False
                Else

                End If
            End If
            If Session("USERTYPE") = "USER" Then
                If radioUserType.SelectedValue = "S" Then
                    SDIUsers()
                ElseIf radioUserType.SelectedValue = "C" Then
                    CustomersUsers()
                End If
            End If
        Else
            btnAdd.Visible = False
            btnEdit.Visible = True
        End If

    End Sub

    Sub buildEditUser(ByVal strUserID)
        Dim strMessage As New Alert
        lblMessage.Text = ""
        Dim strSQLString As String = ""
        If Session("USERTYPEVALUE") = "C" Then
            If Session("ROLE") = "ADMIN" Then
                strSQLString = "SELECT ISA_USER_ID, FIRST_NAME_SRCH," & vbCrLf & _
                            " LAST_NAME_SRCH, ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                            " ISA_EMPLOYEE_ID, PHONE_NUM, ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                            " ISA_SDI_EMPLOYEE, BUSINESS_UNIT, active_status,Multi_Site, ISA_VENDOR_ID, TCKTDEPT" & vbCrLf & _
                            " FROM SDIX_USERS_TBL" & vbCrLf & _
                            " WHERE ISA_EMPLOYEE_ID = '" & strUserID & "' AND ISA_EMPLOYEE_ACTYP <> 'CORPADMIN'" & vbCrLf
                If Session("PUNCHIN") = "YES" Then
                    strSQLString = strSQLString & "AND BUSINESS_UNIT = '" & Session("BUSUNIT") & "'"
                End If
            Else
                strSQLString = "SELECT ISA_USER_ID, FIRST_NAME_SRCH," & vbCrLf & _
                            " LAST_NAME_SRCH, ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                            " ISA_EMPLOYEE_ID, PHONE_NUM, ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                            " ISA_SDI_EMPLOYEE, BUSINESS_UNIT, active_status,Multi_Site, ISA_VENDOR_ID, TCKTDEPT" & vbCrLf & _
                            " FROM SDIX_USERS_TBL" & vbCrLf & _
                            " WHERE ISA_EMPLOYEE_ID = '" & strUserID & "'" & vbCrLf
                If Session("PUNCHIN") = "YES" Then
                    strSQLString = strSQLString & "AND BUSINESS_UNIT = '" & Session("BUSUNIT") & "'"
                End If

            End If
        Else
            strSQLString = "SELECT ISA_USER_ID, FIRST_NAME_SRCH," & vbCrLf & _
                            " LAST_NAME_SRCH, ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                            " ISA_EMPLOYEE_ID, PHONE_NUM, ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                            " ISA_SDI_EMPLOYEE, BUSINESS_UNIT, active_status,Multi_Site, ISA_VENDOR_ID, TCKTDEPT" & vbCrLf & _
                            " FROM SDIX_USERS_TBL" & vbCrLf & _
                            " WHERE ISA_EMPLOYEE_ID = '" & strUserID & "'" & vbCrLf
        End If

        If Not IsUserCanReinstate() Then
            ' If the logged in user cannot activate/inactivate an account, then filter out all the inactive users.
            ' If the logged in user can activate/inactivate, let them see all accounts unrestricted by active status.
            strSQLString &= " AND ACTIVE_STATUS IN ('" & clsUserTbl.ActiveStatus_Active & "','" & clsUserTbl.ActiveStatus_FailedLogin & "')" & vbCrLf
        End If

        Dim dsOREmp As DataSet = ORDBData.GetAdapter(strSQLString)

        If dsOREmp.Tables(0).Rows.Count = 0 Then
            'Me.dropSelectUser.SelectedIndex = 0
            Me.rcbdropSelectUser.SelectedIndex = 0
            ' ltlAlert.Text = strMessage.Say("Error - User does not exist in ISA_USERS_TBL!")
            Exit Sub
        ElseIf dsOREmp.Tables(0).Rows.Count > 1 Then
            ltlAlert.Text = strMessage.Say("Error - User exist more than once in ISA_USERS_TBL table!")
            Exit Sub
            'ElseIf dsOREmp.Tables(0).Rows.Count = 1 Then
            '    Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_USER_ID"))))
        End If

        Session("UserTypeSessionValue") = dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")

        txtFirst.Text = dsOREmp.Tables(0).Rows(0).Item("FIRST_NAME_SRCH")
        txtLast.Text = dsOREmp.Tables(0).Rows(0).Item("LAST_NAME_SRCH")
        Dim strPhoneNumber As String = Trim(dsOREmp.Tables(0).Rows(0).Item("PHONE_NUM"))
        If strPhoneNumber.Contains("-") Then
            txtPhoneNum.Text = Strings.Right(strPhoneNumber, 12)
            txtExt.Text = strPhoneNumber.Remove(strPhoneNumber.Length - 12)
        Else
            txtPhoneNum.Text = Strings.Right(strPhoneNumber, 10)
            txtExt.Text = strPhoneNumber.Remove(strPhoneNumber.Length - 10)
        End If

        txtEmail.Text = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_EMAIL")
        txtUserid.Text = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ID")
        lblUserIDHide.Text = dsOREmp.Tables(0).Rows(0).Item("ISA_USER_ID")
        lblActiveStatusHide.Text = dsOREmp.Tables(0).Rows(0).Item("active_status").ToString.ToUpper
        If dsOREmp.Tables(0).Rows(0).Item("MULTI_SITE").ToString() = "Y" Then
            MultiSiteChk.Checked = True
            rcbMultiSelect.Visible = True
            If MultiSiteChk.Checked Then
                'If Request.QueryString("MEXICO") = "YES" Then
                '    drpBUnit.Visible = False
                '    lblBusUnit.Visible = False
                'End If
                If dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT") <> "SDM00" Then
                    GetMultiBusinessUnit(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT"))
                Else
                    MultiSiteChk.Checked = False
                    rcbMultiSelect.Visible = False
                End If
            Else
                drpBUnit.Visible = False
                lblBusUnit.Visible = False
                rcbMultiSelect.Visible = False

            End If
        Else
            MultiSiteChk.Checked = False
            rcbMultiSelect.Visible = False
            rcbMultiSelect.ClearCheckedItems()
        End If

        Dim strQuery As String = "Select BUSINESS_UNIT FROM SDIX_MULTI_SITE WHERE ISA_EMPLOYEE_ID='" & strUserID & "'"
        Dim dsMultiBU As DataSet = ORDBData.GetAdapter(strQuery)
        If dsMultiBU.Tables(0).Rows.Count > 0 Then
            For i As Integer = 0 To dsMultiBU.Tables(0).Rows.Count - 1
                For Each checkedItem As RadComboBoxItem In rcbMultiSelect.Items
                    If checkedItem.Value = dsMultiBU.Tables(0).Rows(i).Item("BUSINESS_UNIT") Then
                        checkedItem.Checked = True
                        If dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT") = checkedItem.Value Then
                            checkedItem.Enabled = False
                        End If
                    End If
                    'Do Something (insert)
                Next
            Next
        End If


        lblEmplActiveStatusHide.Text = " "
        Dim strCurrBU As String = " "
        strCurrBU = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
        If Trim(strCurrBU) = "" Or Trim(strCurrBU) = "0" Then
            strCurrBU = Session("BUSUNIT")
        End If
        lblCurrBUHide.Text = strCurrBU
        ' check Employee table EFF_STATUS
        strSQLString = "SELECT A.EFF_STATUS from PS_ISA_EMPL_TBL A " & _
         " WHERE A.ISA_EMPLOYEE_ID = '" & strUserID & "' AND A.BUSINESS_UNIT = '" & strCurrBU & "'"
        Dim strEmplEffStatus As String = ""
        Try
            strEmplEffStatus = ORDBData.GetScalar(strSQLString, False)
            If Trim(strEmplEffStatus) = "" Then
                strEmplEffStatus = "N"
            End If
        Catch ex As Exception
            strEmplEffStatus = "N"
        End Try

        lblEmplActiveStatusHide.Text = strEmplEffStatus

        roleDropdownList.ClearSelection()
        If dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE") = "S" And Not IsMexicoVendor() Then
            Select Case dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")
                Case "USER"
                    roleDropdownList.SelectedIndex = 3
                Case "ADMIN"
                    roleDropdownList.SelectedIndex = 2
                Case "SUPER"
                    roleDropdownList.SelectedIndex = 0
                Case "CORPADMIN"
                    roleDropdownList.SelectedIndex = 1
            End Select
        Else
            Select Case dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")
                Case "USER"
                    roleDropdownList.SelectedIndex = 3
                Case "ADMIN"
                    roleDropdownList.SelectedIndex = 2
                Case "ADMINA"
                    roleDropdownList.SelectedIndex = 2
                Case "ADMINR"
                    roleDropdownList.SelectedIndex = 2
                Case "ADMINX"
                    roleDropdownList.SelectedIndex = 2
                Case "ADMINX"
                    roleDropdownList.SelectedIndex = 2
                Case "SUPER"
                    roleDropdownList.SelectedIndex = 0
                Case "CORPADMIN"
                    roleDropdownList.SelectedIndex = 1
            End Select
        End If
        radioUserType.ClearSelection()
        Select Case dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
            Case "S"
                radioUserType.SelectedIndex = 0
            Case "V"
                radioUserType.SelectedIndex = 1
            Case "C"
                radioUserType.SelectedIndex = 2
        End Select

        If Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")) = "V" Then
            Dim sBU As String = dsOREmp.Tables(0).Rows(0).Item("business_unit")
            Dim liItem As ListItem = drpBUnit.Items.FindByValue(sBU)
            If liItem IsNot Nothing Then
                drpBUnit.SelectedIndex = drpBUnit.Items.IndexOf(liItem)
                drpBUnit.Visible = True
                lblBusUnit.Visible = True
            End If
        End If

        If IsVendor() Or IsMexicoVendor() Then
            'drpUserType.Visible = False
            Dim sBU As String = dsOREmp.Tables(0).Rows(0).Item("business_unit")
            Dim liItem As ListItem = drpBUnit.Items.FindByValue(sBU)
            If liItem IsNot Nothing Then
                drpBUnit.SelectedIndex = drpBUnit.Items.IndexOf(liItem)
            End If
            Exit Sub
        End If

        If Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID")) <> "" Then
            'TextBox_VendorId.Text = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID"))
            'RadcomboforVendorID.SelectedValue = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID"))
            If Session("USERTYPE") = "SUPER" Then
                If radioUserType.SelectedValue = "V" Then
                    MainvndrID.Text = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID"))
                    Dim GetVndrUserName As String = Chk_VendrID(Trim(MainvndrID.Text))
                    If GetVndrUserName <> "" Then
                        Error_VendorID.Text = "Vendor Name : " + GetVndrUserName
                        Error_VendorID.ForeColor = Color.Green
                        Error_VendorID.Visible = True
                    Else
                        MainvndrID.Text = ""
                        Error_VendorID.Text = ""
                        Error_VendorID.ForeColor = Color.Red
                        Error_VendorID.Visible = False
                    End If
                Else
                    MainvndrID.Text = ""
                    Error_VendorID.Text = ""
                    Error_VendorID.ForeColor = Color.Red
                    Error_VendorID.Visible = False
                End If
            Else
                MainvndrID.Text = ""
                Error_VendorID.Text = ""
                Error_VendorID.ForeColor = Color.Red
                Error_VendorID.Visible = False
            End If
        Else
            If radioUserType.SelectedValue = "V" Then
                Dim GetVndrUserName As String = Chk_VendrID(Trim(txtUserid.Text))
                If GetVndrUserName <> "" Then
                    MainvndrID.Text = Trim(txtUserid.Text)
                    Error_VendorID.Text = "Vendor Name : " + GetVndrUserName
                    Error_VendorID.ForeColor = Color.Green
                    Error_VendorID.Visible = True
                Else
                    MainvndrID.Text = ""
                    Error_VendorID.Text = ""
                    Error_VendorID.ForeColor = Color.Red
                    Error_VendorID.Visible = False
                End If
            Else
                MainvndrID.Text = ""
                Error_VendorID.Text = ""
                Error_VendorID.ForeColor = Color.Red
                Error_VendorID.Visible = False
            End If
        End If

        If Session("USERTYPEVALUE") = "S" Then
            If radioUserType.SelectedValue = "S" Then
                If Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("TCKTDEPT")) <> "" Then
                    rcbDept.SelectedValue = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("TCKTDEPT"))
                Else
                    rcbDept.SelectedValue = ""
                End If
            Else
                rcbDept.SelectedValue = ""
                rcbDept.Visible = False
                lblDept.Visible = False
            End If
        Else
            rcbDept.SelectedValue = ""
            rcbDept.Visible = False
            lblDept.Visible = False
        End If

        'new code for assigning the BU
        If Session("USERTYPE") = "SUPER" Then
            rcbGroup.ClearSelection()
            Dim strBU As String = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
            If Not rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")) Is Nothing Then
                rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")).Index
            End If
        ElseIf Session("USERTYPE") = "CORPADMIN" Then
            If roleDropdownList.SelectedValue = "CORPADMIN" Then
                rcbGroup.ClearSelection()
                Dim strBU As String = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
                GetSisterSitesBU(strBU)
                If Not rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")) Is Nothing Then
                    rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")).Index
                End If
                MultiSiteChk.Visible = False
                PLGroup.Visible = False
            Else
                rcbGroup.ClearSelection()
                Dim strBU As String = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
                GetSisterSitesBU(strBU)
                If Not rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")) Is Nothing Then
                    rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")).Index
                End If
                MultiSiteChk.Visible = False
                PLGroup.Visible = False
            End If
        Else
            txtGroup.ReadOnly = True
            txtGroup.BackColor = LightGray
            txtGroup.Text = getUserGroupsName(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT"))
            txtGroupID.Text = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
        End If

        ' First, just set these to invisible to start then figure out what should show with the logic below.
        lblAccountDisabled.Visible = False
        btnActivateAccount.Visible = False
        btnInactivateAccount.Visible = False

        btnEmplInactivateAccount.Visible = False
        btnEmplActivateAccount.Visible = False
        lblEmplAccountDisabled.Visible = False

        If IsUserCanReinstate() Then
            If lblActiveStatusHide.Text = clsUserTbl.ActiveStatus_FailedLogin Then
                ' User is temporarily inactivated because they previously failed the login attempts.
                lblAccountDisabled.Visible = True
                lblAccountDisabled.Text = "This SDIX User account has been disabled due to excessive invalid login attempts. Please contact the Help Desk at 1-855-382-1093 to reinstate the account. "
                btnActivateAccount.Visible = True
                btnInactivateAccount.Visible = True ' Allow user to inactivate this account that's disabled due to excessive login attempts just in case we no longer want the account...
            ElseIf lblActiveStatusHide.Text = clsUserTbl.ActiveStatus_Inactive Then
                lblAccountDisabled.Visible = True
                lblAccountDisabled.Text = "This SDIX User account is inactive."
                If IsUserCanReinstate() Then
                    btnActivateAccount.Visible = True ' This account is already inactive. The only thing we can do is activate it.
                End If
            Else
                ' This account is active so just allow logged in user ability to inactivate.                
                btnInactivateAccount.Visible = True
                btnEmplActivateAccount.Visible = False
                btnActivateAccount.Visible = False
            End If
            'employee related
            If Not Session("USERTYPE") = "CORPADMIN" Then
                If lblEmplActiveStatusHide.Text = "N" Then
                    btnEmplInactivateAccount.Visible = False
                    btnEmplActivateAccount.Visible = False
                    lblEmplAccountDisabled.Visible = False
                Else
                    If lblEmplActiveStatusHide.Text = clsUserTbl.EmplActiveStatus_Active Then
                        btnEmplInactivateAccount.Visible = True
                        btnEmplActivateAccount.Visible = False
                        lblEmplAccountDisabled.Visible = False
                    Else
                        btnEmplInactivateAccount.Visible = False
                        btnEmplActivateAccount.Visible = True
                        lblEmplAccountDisabled.Text = "This Employee account is inactive."
                        lblEmplAccountDisabled.Visible = True
                    End If
                End If  '  If lblEmplActiveStatusHide.Text = "N" Then
            End If
        Else
            ' The logged in user cannot inactivate or reinstate an account. So just show an appropriate message for the account.
            If lblActiveStatusHide.Text = clsUserTbl.ActiveStatus_FailedLogin Then
                lblAccountDisabled.Visible = True
                lblAccountDisabled.Text = "This SDIX User account has been disabled due to excessive invalid login attempts. Please contact the Help Desk at 215-633-1900, option 7 to reinstate the account."
            ElseIf lblActiveStatusHide.Text = clsUserTbl.ActiveStatus_Inactive Then
                ' This should never happen because we don't show inactive users to a logged in user that can't reinstate an account.
                ' But we'll keep this logic just in case...
                lblAccountDisabled.Visible = True
                lblAccountDisabled.Text = "This SDIX User account in inactive."
            End If
            'employee related
            If Not Session("USERTYPE") = "CORPADMIN" Then
                If lblEmplActiveStatusHide.Text = "N" Then
                    btnEmplInactivateAccount.Visible = False
                    btnEmplActivateAccount.Visible = False
                    lblEmplAccountDisabled.Visible = False
                Else
                    If lblEmplActiveStatusHide.Text = clsUserTbl.EmplActiveStatus_Active Then
                        btnEmplInactivateAccount.Visible = False
                        btnEmplActivateAccount.Visible = False
                        lblEmplAccountDisabled.Visible = False
                    Else
                        btnEmplInactivateAccount.Visible = False
                        btnEmplActivateAccount.Visible = False
                        lblEmplAccountDisabled.Text = "This Employee account is inactive."
                        lblEmplAccountDisabled.Visible = True
                    End If
                End If  ' If lblEmplActiveStatusHide.Text = "N" Then
            End If
        End If


        If Session("USERTYPEVALUE") = "C" Then
            If Session("ROLE") = "CORPADMIN" Then
                If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                Else
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                End If
            End If
        End If
        If Session("USERTYPE") = "SUPER" Then
            If roleDropdownList.SelectedValue = "CORPADMIN" Then
                If Page_Action = "EDIT" Then
                    MultiSiteChk.Visible = False
                    PLGroup.Visible = False
                Else
                    MultiSiteChk.Visible = True
                    PLGroup.Visible = True
                End If
            Else
                MultiSiteChk.Visible = True
                PLGroup.Visible = True
            End If
        End If

    End Sub

    Private Sub buildGroupList(ByVal rcbUserGroups As RadComboBox)
        Dim strSQLString As String

        If roleDropdownList.SelectedValue = "CORPADMIN" Then
            Dim dsBUSisterSites As DataSet
            If Trim(ViewState("BU")) <> "" Then
                dsBUSisterSites = UnilogORDBData.SisterBusinessUnits(ViewState("BU"))
            Else
                dsBUSisterSites = UnilogORDBData.SisterBusinessUnits(Session("BUSUNIT"))
            End If

            If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
                rcbUserGroups.Visible = True
                rcbUserGroups.DataSource = dsBUSisterSites
                rcbUserGroups.DataTextField = "DESCRIPTION"
                rcbUserGroups.DataValueField = "BUSINESS_UNIT"
                rcbUserGroups.DataBind()
                rcbUserGroups.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
                rcbUserGroups.DataValueField.Insert(0, "0")
            Else
                rcbUserGroups.Visible = True
                rcbUserGroups.DataSource = dsBUSisterSites
                rcbUserGroups.DataTextField = "DESCRIPTION"
                rcbUserGroups.DataValueField = "BUSINESS_UNIT"
                rcbUserGroups.DataBind()
                rcbUserGroups.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
                rcbUserGroups.DataValueField.Insert(0, "0")
                If Trim(ViewState("BU")) <> "" Then
                    rcbUserGroups.Items.Insert(1, New RadComboBoxItem(ViewState("BU"), ViewState("BU")))
                    rcbUserGroups.DataValueField.Insert(0, ViewState("BU"))
                Else
                    rcbUserGroups.Items.Insert(1, New RadComboBoxItem(Session("BUSUNIT"), Session("BUSUNIT")))
                    rcbUserGroups.DataValueField.Insert(0, Session("BUSUNIT"))
                End If
            End If
        Else
            strSQLString = "SELECT  A.ISA_BUSINESS_UNIT as groupid,A.ISA_BUSINESS_UNIT || ' - ' || E.descr  as  groupname " & vbCrLf & _
                      " FROM  SYSADM8.PS_ISA_ENTERPRISE A, SYSADM8.PS_LOCATION_TBL B, PS_BUS_UNIT_TBL_FS E " & vbCrLf & _
                      " WHERE  B.location =  'L'|| substr(A.ISA_BUSINESS_UNIT,2) || '-01'" & vbCrLf & _
                      " AND A.BU_STATUS = '1' " & vbCrLf & _
                      " AND B.EFFDT =" & vbCrLf & _
                      " (SELECT MAX(A_ED.EFFDT) FROM PS_LOCATION_TBL A_ED " & vbCrLf & _
                      " WHERE B.SETID = A_ED.SETID" & vbCrLf & _
                      " AND B.LOCATION = A_ED.LOCATION" & vbCrLf & _
                      " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                      " AND E.BUSINESS_UNIT (+) = A.ISA_BUSINESS_UNIT" & vbCrLf & _
                      " ORDER BY A.ISA_BUSINESS_UNIT "
            Dim dsDQLGroups As DataSet = ORDBData.GetAdapter(strSQLString)

            rcbUserGroups.DataSource = dsDQLGroups
            rcbUserGroups.DataValueField = "groupid"
            rcbUserGroups.DataTextField = "groupname"
            rcbUserGroups.DataBind()
            rcbUserGroups.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
            rcbUserGroups.DataValueField.Insert(0, "0")
        End If
    End Sub

    Private Sub buildSelectDropDown()
        Dim dsORUsers As DataSet = GetSelectDropDownData()
        'dropSelectUser.DataSource = dsORUsers
        'dropSelectUser.DataValueField = "ISA_EMPLOYEE_ID"
        'dropSelectUser.DataTextField = "USERANDBU" '"ISA_USER_NAME"
        ''dropSelectUser.DataTextField = "ISA_USER_NAME"
        'dropSelectUser.DataBind()

        'dropSelectUser.Items.Insert(0, New ListItem("<< Select User >>"))

        rcbdropSelectUser.DataSource = dsORUsers
        rcbdropSelectUser.DataValueField = "ISA_EMPLOYEE_ID"
        rcbdropSelectUser.DataTextField = "USERANDBU"
        rcbdropSelectUser.DataBind()

        rcbdropSelectUser.Items.Insert(0, New RadComboBoxItem("<< Select User >>"))

        If IsVendor() Then
            buildSelectDropDownAP()
        End If
    End Sub

    Private Sub buildSelectDropDownAP()
        Me.drpuserAp.Visible = True
        Dim strSQLSelect As String
        Dim strSQLWhere As String
        Dim strSQLOrder As String = ""
        Dim strSQLString As String

        strSQLSelect = "SELECT ISA_USER_NAME, ISA_EMPLOYEE_ID" & vbCrLf & _
                        " FROM SDIX_USERS_TBL"
        strSQLWhere = " WHERE  ISA_SDI_EMPLOYEE = 'S'"
        If Not IsUserCanReinstate() Then
            ' If the logged in user cannot activate/inactivate an account, filter out inactive users.
            ' If the logged in user can activate/inactivate, let them see all accounts unrestricted by active status.
            strSQLOrder = "  and ACTIVE_STATUS IN ('" & clsUserTbl.ActiveStatus_Active & "','" & clsUserTbl.ActiveStatus_FailedLogin & "')" & vbCrLf
        End If
        strSQLOrder &= " ORDER BY ISA_USER_NAME"

        strSQLString = strSQLSelect & strSQLWhere & strSQLOrder

        Dim dsORUsers As DataSet = ORDBData.GetAdapter(strSQLString)

        Me.drpuserAp.DataSource = dsORUsers
        Me.drpuserAp.DataValueField = "ISA_EMPLOYEE_ID"
        Me.drpuserAp.DataTextField = "ISA_USER_NAME"
        Me.drpuserAp.DataBind()

        Me.drpuserAp.Items.Insert(0, New ListItem("<< Select User >>"))
    End Sub

    Private Function checkUserPrivs(ByVal strUserid) As Boolean
        Dim strbunit As String = " "
        'If Me.txtUserid.Text.Substring(0, 2) = "M0" Or Me.txtUserid.Text.Substring(0, 2) = "MU" Then
        If strUserid.Substring(0, 2) = "M0" Or strUserid.Substring(0, 2) = "MU" Then
            strbunit = "SDM00"
        Else
            strbunit = "ISA00"
        End If
        Dim strSQLString As String = "SELECT A.ISA_EMPLOYEE_ID" & vbCrLf & _
                        " FROM SDIX_USERS_PRIVS A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & strbunit & "'" & vbCrLf & _
                        " AND A.ISA_EMPLOYEE_ID = '" & strUserid & "'" & vbCrLf & _
                        " AND A.ISA_IOL_OP_NAME = 'ASN'" & vbCrLf & _
                        " AND A.ISA_IOL_OP_VALUE = 'Y'" & vbCrLf & _
                        " AND A.ISA_IOL_OP_TYPE = 'SUP'"

        Dim strUserResults As String = ORDBData.GetScalar(strSQLString)
        If strUserResults Is Nothing Then
            checkUserPrivs = False
        Else
            checkUserPrivs = True
        End If
    End Function

    Private Function checkCustEmpTbl(ByVal strBU As String) As Boolean

        Dim strSQLstring As String = "Select ISA_EMPLOYEE_ID" & vbCrLf & _
                    " FROM PS_ISA_EMPL_TBL" & vbCrLf & _
                    " WHERE UPPER(isa_employee_id) = '" & txtUserid.Text.ToUpper & "'" & vbCrLf & _
                    " AND BUSINESS_UNIT = '" & strBU & "'"

        Dim dsCustUserid As DataSet = ORDBData.GetAdapter(strSQLstring)

        If dsCustUserid.Tables(0).Rows.Count = 0 Then
            checkCustEmpTbl = False
        Else
            checkCustEmpTbl = True
        End If
    End Function

    Private Function checkSDIEmpTbl() As Boolean

        Dim intSubstringLen As Integer = txtUserid.Text.Length - 1
        Dim strSQLstring As String = "SELECT OPRID" & vbCrLf & _
                    " FROM PSOPRDEFN" & vbCrLf & _
                    " WHERE OPRID like '" & txtUserid.Text.Substring(0, intSubstringLen).ToUpper & "_'" '

        Dim dsSDIUserid As DataSet = ORDBData.GetAdapter(strSQLstring)

        If txtUserid.Text = "DEMO" Then
            checkSDIEmpTbl = True
        ElseIf txtUserid.Text.Substring(0, 4).ToUpper = "TEMP" Then
            checkSDIEmpTbl = True
        ElseIf dsSDIUserid.Tables(0).Rows.Count = 0 Then
            checkSDIEmpTbl = False
        Else
            checkSDIEmpTbl = True
        End If

    End Function

    Private Function checkUserid() As Boolean
        Dim strSQLstring As String = "Select isa_user_id" & vbCrLf & _
                    " FROM SDIX_USERS_TBL" & vbCrLf & _
                    " WHERE isa_employee_id = '" & txtUserid.Text.ToUpper & "'" & vbCrLf & _
                    " AND ACTIVE_STATUS = 'A'" & vbCrLf

        Dim dsUserid As DataSet = ORDBData.GetAdapter(strSQLstring)

        If dsUserid.Tables(0).Rows.Count = 0 Then
            checkUserid = False
        ElseIf dsUserid.Tables(0).Rows.Count > 1 Then
            Dim strMessage As New Alert
            checkUserid = True
            'ltlAlert.Text = strMessage.Say("Error - User exists more than once in user table!")
        Else
            checkUserid = True
        End If
    End Function

    Private Function ExistsUserid() As Boolean
        Dim bExistsUserID = False
        Try
            Dim strSQLstring As String = "Select ISA_USER_ID FROM SDIX_USERS_TBL WHERE isa_employee_id = '" & Trim(txtUserid.Text.ToUpper) & "'"

            Dim dsUserid As DataSet = ORDBData.GetAdapter(strSQLstring)


            If dsUserid.Tables(0).Rows.Count = 0 Then
                bExistsUserID = False
            ElseIf dsUserid.Tables(0).Rows.Count > 1 Then
                Dim strMessage As New Alert
                bExistsUserID = True
                'ltlAlert.Text = strMessage.Say("Error - User exists more than once in user table!")
            Else
                bExistsUserID = True
            End If


        Catch ex As Exception

        End Try
        Return bExistsUserID
    End Function

    Private Function getPWsql(ByVal strPWencr) As String

        Dim s As String = ""

        Dim strAlert As String = "Error - password has already been used"
        Dim dteNow As Date
        dteNow = Now().ToString("d")
        Dim strSQLstring As String
        strSQLstring = "SELECT A.ISA_USER_ID, A.ISA_EMPLOYEE_ID," & vbCrLf & _
                        " A.ISA_ISOL_PW1, A.ISA_ISOL_PW_DATE1," & vbCrLf & _
                        " A.ISA_ISOL_PW2, A.ISA_ISOL_PW_DATE2," & vbCrLf & _
                        " A.ISA_ISOL_PW3, A.ISA_ISOL_PW_DATE3" & vbCrLf & _
                        " FROM SDIX_ISOL_PW A" & vbCrLf & _
                        " WHERE A.ISA_USER_ID = '" & lblUserIDHide.Text & "'" & vbCrLf & _
                        " AND A.ISA_EMPLOYEE_ID = '" & txtUserid.Text & "'"

        Try
            Dim dtrPWReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
            Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
            If m_weblogstring = "true" Then
                'WebLogOpenConn()
            End If
            If dtrPWReader.Read() Then
                If dtrPWReader.Item("ISA_ISOL_PW1") = strPWencr Or _
                    dtrPWReader.Item("ISA_ISOL_PW2") = strPWencr Or _
                    dtrPWReader.Item("ISA_ISOL_PW3") = strPWencr Then
                    'getPWsql = strAlert
                    s = strAlert
                    dtrPWReader.Close()
                    'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                    If m_weblogstring = "true" Then
                        'WebLogCloseConn()
                    End If
                    'Exit Function
                    Return (s)
                End If
                Dim pw2 As String = " "
                Try
                    pw2 = CStr(dtrPWReader.Item("ISA_ISOL_PW1")).Trim
                Catch ex As Exception
                    pw2 = " "
                End Try
                Dim dt2 As String = ""
                Try
                    dt2 = CStr(dtrPWReader.Item("ISA_ISOL_PW_DATE1")).Trim
                    If dt2.Length > 0 Then
                        dt2 = CDate(dt2).ToString("MM/dd/yyyy")
                    End If
                Catch ex As Exception
                    dt2 = ""
                End Try
                Dim pw3 As String = " "
                Try
                    pw3 = CStr(dtrPWReader.Item("ISA_ISOL_PW2")).Trim
                Catch ex As Exception
                    pw3 = " "
                End Try
                Dim dt3 As String = ""
                Try
                    dt3 = CStr(dtrPWReader.Item("ISA_ISOL_PW_DATE2")).Trim
                    If dt3.Length > 0 Then
                        dt3 = CDate(dt3).ToString("MM/dd/yyyy")
                    End If
                Catch ex As Exception
                    dt3 = ""
                End Try
                s = ""
                s &= "" & _
                     "UPDATE SDIX_ISOL_PW " & vbCrLf & _
                     "SET " & vbCrLf & _
                     " ISA_ISOL_PW1 = '" & strPWencr & "' " & vbCrLf & _
                     ",ISA_ISOL_PW_DATE1 = TO_DATE('" & dteNow.ToString("MM/dd/yyyy") & "', 'MM/DD/YYYY') " & vbCrLf & _
                     ""
                If pw2.Trim.Length > 0 And _
                   dt2.Trim.Length > 0 Then
                    s &= "" & _
                         ",ISA_ISOL_PW2 = '" & pw2 & "' " & vbCrLf & _
                         ",ISA_ISOL_PW_DATE2 = TO_DATE('" & dt2 & "', 'MM/DD/YYYY') " & vbCrLf & _
                         ""
                Else
                    s &= "" & _
                         ",ISA_ISOL_PW2 = ' ' " & vbCrLf & _
                         ",ISA_ISOL_PW_DATE2 = NULL " & vbCrLf & _
                         ""
                End If
                If pw3.Trim.Length > 0 And _
                   dt3.Trim.Length > 0 Then
                    s &= "" & _
                         ",ISA_ISOL_PW3 = '" & pw3 & "' " & vbCrLf & _
                         ",ISA_ISOL_PW_DATE3 = TO_DATE('" & dt3 & "', 'MM/DD/YYYY') " & vbCrLf & _
                         ""
                Else
                    s &= "" & _
                         ",ISA_ISOL_PW3 = ' ' " & vbCrLf & _
                         ",ISA_ISOL_PW_DATE3 = NULL " & vbCrLf & _
                         ""
                End If
                s &= "" & _
                     "WHERE ISA_USER_ID = '" & lblUserIDHide.Text & "' " & vbCrLf & _
                     "  AND ISA_EMPLOYEE_ID = '" & txtUserid.Text & "' " & vbCrLf & _
                     ""
            Else
                s = ""
                s &= "" & _
                     "INSERT INTO SDIX_ISOL_PW " & vbCrLf & _
                     "(" & vbCrLf & _
                     " ISA_USER_ID " & vbCrLf & _
                     ",ISA_EMPLOYEE_ID " & vbCrLf & _
                     ",ISA_ISOL_PW1 " & vbCrLf & _
                     ",ISA_ISOL_PW_DATE1 " & vbCrLf & _
                     ",ISA_ISOL_PW2 " & vbCrLf & _
                     ",ISA_ISOL_PW_DATE2 " & vbCrLf & _
                     ",ISA_ISOL_PW3 " & vbCrLf & _
                     ",ISA_ISOL_PW_DATE3 " & vbCrLf & _
                     ") " & vbCrLf & _
                     "VALUES " & vbCrLf & _
                     "(" & vbCrLf & _
                     " " & lblUserIDHide.Text & vbCrLf & _
                     ",'" & txtUserid.Text & "' " & vbCrLf & _
                     ",'" & strPWencr & "' " & vbCrLf & _
                     ",TO_DATE('" & dteNow.ToString("MM/dd/yyyy") & "', 'MM/DD/YYYY') " & vbCrLf & _
                     ",' ' " & vbCrLf & _
                     ",NULL " & vbCrLf & _
                     ",' ' " & vbCrLf & _
                     ",NULL " & vbCrLf & _
                     ") " & vbCrLf & _
                     ""
            End If

            dtrPWReader.Close()
            'WebLogCloseConn()

        Catch objException As Exception
            sendErrorEmail(objException.ToString, "NO", Request.ServerVariables("URL"), strSQLstring)
            Response.Redirect("DBErrorPage.aspx?HOME=N")
        End Try

        Return (s)
    End Function

    Sub ProfileUpdate()

        Dim strMessage As New Alert
        If Request.ServerVariables("HTTP_HOST").ToString().ToUpper.Substring(0, 6) = "CPTEST" And _
            Session("USERNAME").toupper = "SDI TEMP USER" Then
            ltlAlert.Text = strMessage.Say("Warning - Profile update has been disabled in test")
            Exit Sub
        End If
        If Session("USERNAME").toupper = "USER NAME" Then
            ltlAlert.Text = strMessage.Say("Warning - Profile update has been disabled for User DEMO")
            Exit Sub
        End If


        'Exit Sub

        Dim strUserGroup As String
        Dim strRndCplusPassw As String
        Dim strUserType As String
        Dim strSDICust As String
        Dim strSQLString As String
        Dim strSQLPW As String
        Dim strPasswEncrp As String
        Dim strSQLUPD1 As String
        Dim strSQLUPD2 As String
        Dim strPasswUpdate As String
        Dim strVendorIdvalue As String
        Dim MultiSiteAccess As String
        Dim dteNow As Date
        dteNow = Now().ToString("d")

        Dim strFirst As String = Trim(txtFirst.Text)
        strFirst = Replace(strFirst, "'", "")
        Dim strLast As String = Trim(txtLast.Text)
        strLast = Replace(strLast, "'", "")
        Dim strFullName As String = strLast & "," & strFirst
        Dim strFullName40 As String = strLast & "," & strFirst
        If strFullName.Length > 50 Then
            strFullName = strFullName.Substring(0, 50)
        End If
        If strFullName40.Length > 40 Then
            strFullName40 = strFullName40.Substring(0, 40)
        End If
        'strVendorIdvalue = Trim(TextBox_VendorId.Text)
        strVendorIdvalue = Trim(MainvndrID.Text)
        Dim strExt As String = txtExt.Text
        Dim strPh As String = txtPhoneNum.Text
        Dim strPhone As String = strExt + strPh
        strPhone = Replace(strPhone, "(", "")
        strPhone = Replace(strPhone, ")", "")
        strPhone = Replace(strPhone, " ", "-")
        strPhone = Replace(strPhone, "+", "")
        'Dim strUSERID As String = GetDisplayedUserID()
        Dim strUSERID As String = String.Empty

        If radioUserType.SelectedValue <> "V" Then
            If Session("ROLE") = "SUPER" Then
                Dim Site_Flag As String = GetFlagValue(rcbGroup.SelectedValue)
                If Site_Flag = "A" Then
                    If Page_Action = "ADD" Then
                        strUSERID = Auto_UserIdGenerate_Flags()
                    ElseIf Page_Action = "EDIT" Then
                        strUSERID = GetDisplayedUserID()
                    End If
                ElseIf Site_Flag = "V" Then
                    strUSERID = Trim(txtUserid.Text.ToUpper)
                Else
                    strUSERID = GetDisplayedUserID()
                End If
            Else
                If Session("Flag_AddUser") = "A" Then
                    If Page_Action = "ADD" Then
                        strUSERID = Auto_UserIdGenerate_Flags()
                    ElseIf Page_Action = "EDIT" Then
                        strUSERID = GetDisplayedUserID()
                    End If
                ElseIf Session("Flag_AddUser") = "V" Then
                    strUSERID = Trim(txtUserid.Text.ToUpper)
                Else
                    strUSERID = GetDisplayedUserID()
                End If
            End If
        Else
            If Page_Action = "ADD" Then
                strUSERID = Auto_UserIdGenerate_Flags()
            ElseIf Page_Action = "EDIT" Then
                strUSERID = GetDisplayedUserID()
            End If
        End If


        Dim strBU As String
        GetSelectedBUandGroup(strBU, strUserGroup)

        If MultiSiteChk.Checked = True Then
            If rcbMultiSelect.CheckedItems.Count = 0 Then
                ''Error_LabelSite.Text = "Please select a business unit to access the multi site."
                'ltlAlert.Text = strMessage.Say("Please select a business unit to access the multi site.")
                ''Exit Sub
            Else
                If Session("USERTYPE") = "SUPER" Then
                    If radioUserType.SelectedValue = "S" Or radioUserType.SelectedValue = "C" Then
                        If roleDropdownList.SelectedValue = "CORPADMIN" Then
                            MultiSiteAccess = " "
                        Else
                            MultiSiteAccess = "Y"
                        End If
                    Else
                        MultiSiteAccess = " "
                    End If
                ElseIf Session("USERTYPE") = "ADMIN" Then
                    If radioUserType.SelectedValue = "S" Or radioUserType.SelectedValue = "C" Then
                        If roleDropdownList.SelectedValue = "CORPADMIN" Then
                            MultiSiteAccess = " "
                        Else
                            MultiSiteAccess = "Y"
                        End If
                    Else
                        MultiSiteAccess = " "
                    End If
                ElseIf Session("USERTYPE") = "CORPADMIN" Then

                Else
                    MultiSiteAccess = " "
                End If
            End If

        End If

        Dim strDepartmentValue As String = ""
        If Session("USERTYPEVALUE") = "S" Then
            If radioUserType.SelectedValue = "S" Then
                strDepartmentValue = rcbDept.SelectedValue
            ElseIf Session("USERTYPE") = "ADMIN" Or Session("USERTYPE") = "USER" Then
                strDepartmentValue = rcbDept.SelectedValue
            Else
                strDepartmentValue = ""
            End If
        Else
            strDepartmentValue = ""
        End If

        strRndCplusPassw = GenerateRndPassword(strUSERID, False, "")

        GetDisplayedUserType(strSDICust, strUserType)
        Dim sSDIEmp As String = ConvertSDICustToSDIEmp(strSDICust)

        If Not txtPassword.Text = "" Then
            strPasswEncrp = GenerateHash(txtPassword.Text)
            strPasswUpdate = " ISA_PASSWORD_ENCR = '" & strPasswEncrp & "'," & vbCrLf
        End If
        Dim MultiSiteAvailible As String = ""
        Dim IsBaseBU As Boolean = False
        Dim multiBU As String = String.Empty
        'If lblAction.Text = "ADD" Then
        If Page_Action = "ADD" Then
            Dim strFromWhichScreen As String = "Profile.aspx/ProfileUpdate"
            Dim lngIsaUserId As Long = GetNextUserId(strUSERID, strBU, strFromWhichScreen)
            If radioUserType.SelectedValue = "V" Then
                strSQLString = "INSERT INTO SDIX_USERS_TBL" & vbCrLf & _
                        " (ISA_USER_ID, ISA_USER_NAME," & vbCrLf & _
                        " ISA_PASSWORD_ENCR, FIRST_NAME_SRCH," & vbCrLf & _
                        " LAST_NAME_SRCH, BUSINESS_UNIT," & vbCrLf & _
                        " ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME, PHONE_NUM," & vbCrLf & _
                        " ISA_DAILY_ALLOW, ISA_EMPLOYEE_PASSW," & vbCrLf & _
                        " ISA_EMPLOYEE_EMAIL, ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                        " CUST_ID, ISA_SESSION," & vbCrLf & _
                        " ISA_LAST_SYNC_DATE, ISA_SDI_EMPLOYEE, ISA_CUST_SERV_FLG," & vbCrLf & _
                        " LASTUPDOPRID, LASTUPDDTTM, ACTIVE_STATUS, LAST_ACTIVITY, ISA_VENDOR_ID)" & vbCrLf & _
                        " VALUES(" & lngIsaUserId & "," & vbCrLf & _
                        " '" & strFullName.ToUpper & "'," & vbCrLf & _
                        " '" & strPasswEncrp & "'," & vbCrLf & _
                        " '" & strFirst.ToUpper & "'," & vbCrLf & _
                        " '" & strLast.ToUpper & "'," & vbCrLf & _
                        " '" & strBU & "'," & vbCrLf & _
                        " '" & strUSERID & "'," & vbCrLf & _
                        " '" & strFullName40.ToUpper & "'," & vbCrLf & _
                        " '" & strPhone & "'," & vbCrLf & _
                        " 0, ' ', '" & Trim(txtEmail.Text) & "'," & vbCrLf & _
                        " '" & strUserType & "'," & vbCrLf & _
                        " '0', 0, '', '" & strSDICust & "', ' '," & vbCrLf & _
                        " '" & Session("USERID") & "'," & vbCrLf & _
                        " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
                        " 'A', TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'), '" & strVendorIdvalue & "')"
            Else
                strSQLString = "INSERT INTO SDIX_USERS_TBL" & vbCrLf & _
                        " (ISA_USER_ID, ISA_USER_NAME," & vbCrLf & _
                        " ISA_PASSWORD_ENCR, FIRST_NAME_SRCH," & vbCrLf & _
                        " LAST_NAME_SRCH, BUSINESS_UNIT," & vbCrLf & _
                        " ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME, PHONE_NUM," & vbCrLf & _
                        " ISA_DAILY_ALLOW, ISA_EMPLOYEE_PASSW," & vbCrLf & _
                        " ISA_EMPLOYEE_EMAIL, ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                        " CUST_ID, ISA_SESSION," & vbCrLf & _
                        " ISA_LAST_SYNC_DATE, ISA_SDI_EMPLOYEE, ISA_CUST_SERV_FLG," & vbCrLf & _
                        " LASTUPDOPRID, LASTUPDDTTM, ACTIVE_STATUS, LAST_ACTIVITY, MULTI_SITE, TCKTDEPT)" & vbCrLf & _
                        " VALUES(" & lngIsaUserId & "," & vbCrLf & _
                        " '" & strFullName.ToUpper & "'," & vbCrLf & _
                        " '" & strPasswEncrp & "'," & vbCrLf & _
                        " '" & strFirst.ToUpper & "'," & vbCrLf & _
                        " '" & strLast.ToUpper & "'," & vbCrLf & _
                        " '" & strBU & "'," & vbCrLf & _
                        " '" & strUSERID & "'," & vbCrLf & _
                        " '" & strFullName40.ToUpper & "'," & vbCrLf & _
                        " '" & strPhone & "'," & vbCrLf & _
                        " 0, ' ', '" & Trim(txtEmail.Text) & "'," & vbCrLf & _
                        " '" & strUserType & "'," & vbCrLf & _
                        " '0', 0, '', '" & strSDICust & "', ' '," & vbCrLf & _
                        " '" & Session("USERID") & "'," & vbCrLf & _
                        " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
                        " 'A', TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'), '" & MultiSiteAccess & "', '" & Trim(strDepartmentValue) & "')"
            End If

            strSQLPW = "INSERT INTO SDIX_ISOL_PW" & vbCrLf & _
                        " (ISA_USER_ID, ISA_EMPLOYEE_ID," & vbCrLf & _
                        " ISA_ISOL_PW1, ISA_ISOL_PW_DATE1," & vbCrLf & _
                        " ISA_ISOL_PW2, ISA_ISOL_PW_DATE2," & vbCrLf & _
                        " ISA_ISOL_PW3, ISA_ISOL_PW_DATE3)" & vbCrLf & _
                        " VALUES (" & lngIsaUserId & "," & vbCrLf & _
                        " '" & strUSERID & "'," & vbCrLf & _
                        " '" & strPasswEncrp & "'," & vbCrLf & _
                        " TO_DATE('" & dteNow & "', 'MM/DD/YYYY')," & vbCrLf & _
                        " ' ', '', ' ','')"

            'ElseIf lblAction.Text = "EDIT" Or lblAction.Text = "USER" Then
        ElseIf Page_Action = "EDIT" Or Page_Action = "USER" Then
            strSQLUPD1 = "UPDATE SDIX_USERS_TBL" & vbCrLf & _
                        " SET ISA_USER_NAME = '" & strFullName.ToUpper & "'," & vbCrLf

            strSQLUPD2 = " FIRST_NAME_SRCH = '" & strFirst.ToUpper & "'," & vbCrLf &
                        " LAST_NAME_SRCH = '" & strLast.ToUpper & "'," & vbCrLf &
                        " BUSINESS_UNIT = '" & strBU & "'," & vbCrLf &
                        " ISA_EMPLOYEE_NAME = '" & strFullName40.ToUpper & "'," & vbCrLf &
                        " PHONE_NUM = '" & strPhone & "'," & vbCrLf &
                        " ISA_EMPLOYEE_EMAIL = '" & Trim(txtEmail.Text) & "'," & vbCrLf &
                        " ISA_VENDOR_ID = '" & strVendorIdvalue & "', " & vbCrLf

            If roleDropdownList.Visible Then
                If Not Trim(strUserType) = "" Then
                    strSQLUPD2 = strSQLUPD2 & " ISA_EMPLOYEE_ACTYP = '" & strUserType & "'," & vbCrLf
                End If
                If Not Trim(strSDICust) = "" Then
                    strSQLUPD2 = strSQLUPD2 & " ISA_SDI_EMPLOYEE = '" & strSDICust & "'," & vbCrLf
                End If
            End If

            If MultiSiteChk.Checked And rcbMultiSelect.CheckedItems.Count > 1 Then
                ''strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf

                If Session("USERTYPEVALUE") = "S" Then
                    If Session("USERTYPE") = "SUPER" Then
                        If radioUserType.SelectedValue = "S" Or radioUserType.SelectedValue = "C" Then
                            If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                                MultiSiteAvailible = "Y"
                                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf
                            Else
                                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                            End If
                        Else
                            strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                        End If
                    ElseIf Session("USERTYPE") = "ADMIN" Then
                        If radioUserType.SelectedValue = "S" Or radioUserType.SelectedValue = "C" Then
                            If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                                MultiSiteAvailible = "Y"
                                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf
                            Else
                                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                            End If
                        Else
                            strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                        End If
                    Else
                        MultiSiteAvailible = "Y"
                        strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf
                    End If
                ElseIf Session("USERTYPEVALUE") = "C" Then
                    If Session("USERTYPE") = "CORPADMIN" Then
                        If radioUserType.SelectedValue = "C" Then
                            If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                                MultiSiteAvailible = "Y"
                                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf
                            Else
                                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                            End If
                        Else
                            strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                        End If
                    ElseIf Session("USERTYPE") = "ADMIN" Then
                        If radioUserType.SelectedValue = "C" Then
                            If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                                MultiSiteAvailible = "Y"
                                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf
                            Else
                                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                            End If
                        Else
                            strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                        End If
                    Else
                        MultiSiteAvailible = "Y"
                        strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf
                    End If
                Else
                    strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                End If
            Else
                strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                rcbMultiSelect.ClearCheckedItems()
                MultiSiteChk.Checked = False
                rcbMultiSelect.Visible = False
            End If

            If Session("USERTYPEVALUE") = "S" Then
                If radioUserType.SelectedValue = "S" Then
                    strSQLUPD2 = strSQLUPD2 & " TCKTDEPT = '" & Trim(strDepartmentValue) & "'," & vbCrLf
                ElseIf Session("USERTYPE") = "ADMIN" Or Session("USERTYPE") = "USER" Then
                    strSQLUPD2 = strSQLUPD2 & " TCKTDEPT = '" & Trim(strDepartmentValue) & "'," & vbCrLf
                End If
            End If

            Dim strCurrentRoleType As String = ""
            Dim strCurrentBU As String = ""
            Dim strCurrentRoleNUM As String = ""
            strCurrentRoleType = Get_UserValues(Trim(strUSERID).ToUpper, "ISA_SDI_EMPLOYEE")
            strCurrentBU = Get_UserValues(Trim(strUSERID).ToUpper, "BUSINESS_UNIT")
            strCurrentRoleNUM = Get_UserValues(Trim(strUSERID).ToUpper, "ROLENUM")

            If strCurrentRoleType <> strSDICust Then
                DeleteUserPriv(Trim(strUSERID).ToUpper, strBU)
                strSQLUPD2 = strSQLUPD2 &
                    " ROLENUM = null, " & vbCrLf
            Else
                Select Case strCurrentRoleType
                    Case "V"
                        If strCurrentBU <> strBU Then
                            If strCurrentRoleNUM <> "" Then
                                DeleteUserPriv(Trim(strUSERID).ToUpper, strBU)
                                strSQLUPD2 = strSQLUPD2 &
                                        " ROLENUM = null, " & vbCrLf
                            End If
                        End If
                    Case "C"
                        If strCurrentBU <> strBU Then
                            Dim RoleExist As Boolean = False
                            If strCurrentRoleNUM <> "" Then
                                RoleExist = Get_RoleExist(strCurrentRoleNUM, strBU)
                                If Not RoleExist Then
                                    strSQLUPD2 = strSQLUPD2 &
                                        " ROLENUM = null, " & vbCrLf
                                End If
                            End If
                        End If
                End Select
            End If
            strSQLUPD2 = strSQLUPD2 &
                    " LASTUPDOPRID = '" & Session("USERID") & "'," & vbCrLf &
                    " LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                    " WHERE ISA_EMPLOYEE_ID = '" & Trim(strUSERID).ToUpper & "'"

            strSQLString = strSQLUPD1 & strPasswUpdate & strSQLUPD2
            If Not txtPassword.Text = "" Then
                strSQLPW = getPWsql(strPasswEncrp)
                If strSQLPW.Substring(0, 5) = "Error" Then
                    'lblPassword.Visible = True
                    'lblConfirm.Visible = True
                    txtPassword.Text = ""
                    txtConfirm.Text = ""
                    lblMessage.Text = "Error - password has already been used"
                    setuppasswordfields("PWCHANGE")
                    Exit Sub
                End If
            End If
        End If

        Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSQLString)
        If rowsaffected = 0 Then
            lblDBError.Text = "Error Updating ISA_USERS_TBL Table"
            lblDBError.Visible = True
            Exit Sub
        Else

            Dim query As String = "DELETE FROM SDIX_MULTI_SITE WHERE ISA_EMPLOYEE_ID='" & strUSERID & "'"
            ORDBData.ExecNonQuery(query)
            'If Page_Action = "ADD" Then
            '    If Session("ROLE") = "SUPER" Then
            '        Dim Site_Flag As String = GetFlagValue(rcbGroup.SelectedValue)
            '        If Site_Flag = "A" Then
            '            UpdateUserIDSeqTBL()
            '        ElseIf radioUserType.SelectedValue = "V" Then
            '            UpdateUserIDSeqTBL()
            '        End If
            '    Else
            '        If Session("Flag_AddUser") = "A" Then
            '            UpdateUserIDSeqTBL()
            '        End If
            '    End If
            'End If

            If rcbMultiSelect.CheckedItems.Count > 1 Then
                If Page_Action = "EDIT" Then
                    If MultiSiteAvailible = "Y" Then
                        For Each item As RadComboBoxItem In rcbMultiSelect.CheckedItems
                            multiBU = item.Value
                            query = "INSERT INTO SDIX_MULTI_SITE VALUES('" & strUSERID & "','" & multiBU & "','" & Session("USERID") & "',TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
                            ORDBData.ExecNonQuery(query)
                        Next
                    End If
                Else
                    If Trim(MultiSiteAccess) = "Y" Then
                        For Each item As RadComboBoxItem In rcbMultiSelect.CheckedItems
                            multiBU = item.Value
                            query = "INSERT INTO SDIX_MULTI_SITE VALUES('" & strUSERID & "','" & multiBU & "','" & Session("USERID") & "',TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
                            ORDBData.ExecNonQuery(query)
                        Next
                    End If
                End If
            End If

        End If
        If Not txtPassword.Text = "" Then
            rowsaffected = ORDBData.ExecNonQuery(strSQLPW)
            If rowsaffected = 0 Then
                lblDBError.Text = "Error Updating ISA_ISOL_PW Table"
                lblDBError.Visible = True
                Exit Sub
            End If
        End If
        If strSDICust = "C" Then
            If checkCustEmpTbl(strBU) = False Then

                strSQLString = "Insert Into PS_ISA_EMPL_TBL" & vbCrLf & _
                                " ( BUSINESS_UNIT, ISA_EMPLOYEE_ID," & vbCrLf & _
                                " ISA_EMPLOYEE_NAME, ISA_DAILY_ALLOW," & vbCrLf & _
                                " ISA_EMPLOYEE_PASSW, ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                                " ISA_EMPLOYEE_ACTYP, CUST_ID, EFF_STATUS) " & vbCrLf & _
                               " Values('" & strBU & "'," & vbCrLf & _
                               "'" & strUSERID.ToUpper & "'," & vbCrLf & _
                               "'" & strFullName40.ToUpper & "'" & vbCrLf & _
                               ",0,' '," & vbCrLf & _
                               "' '," & vbCrLf & _
                               "' ', ' ', 'A')" & vbCrLf

                rowsaffected = ORDBData.ExecNonQuery(strSQLString)
                If rowsaffected = 0 Then
                    lblDBError.Text = "Error Updating PS_ISA_EMPL_TBL Table"
                    lblDBError.Visible = True
                    Exit Sub
                End If

            End If
        End If

        If Not strBU = "SDI00" And Not strBU = "SDM00" Then

        Else
            Dim strbunit As String = " "
            'If Me.txtUserid.Text.Substring(0, 2) = "M0" Or Me.txtUserid.Text.Substring(0, 2) = "MU" Then
            If strUSERID.Substring(0, 2) = "M0" Or strUSERID.Substring(0, 2) = "MU" Then
                strbunit = "SDM00"
            Else
                strbunit = "ISA00"
            End If
            'Dim bolUserPrivs As Boolean = checkUserPrivs(Trim(txtUserid.Text).ToUpper)
            Dim bolUserPrivs As Boolean = checkUserPrivs(Trim(strUSERID).ToUpper)

            If bolUserPrivs = False Then
                strSQLString = "INSERT INTO SDIX_USERS_PRIVS" & vbCrLf & _
                    " (ISA_EMPLOYEE_ID," & vbCrLf & _
                    " BUSINESS_UNIT," & vbCrLf & _
                    " ISA_IOL_OP_NAME," & vbCrLf & _
                    " ISA_IOL_OP_VALUE," & vbCrLf & _
                    " ISA_IOL_OP_TYPE," & vbCrLf & _
                    " LASTUPDOPRID," & vbCrLf & _
                    " LASTUPDDTTM)" & vbCrLf & _
                    " VALUES('" & strUSERID.ToUpper & "'," & vbCrLf & _
                    " '" & strbunit & "'," & vbCrLf & _
                    " 'ASN','Y'," & vbCrLf & _
                    " 'SUP'," & vbCrLf & _
                    " '" & Session("USERID") & "'," & vbCrLf & _
                    " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"


                rowsaffected = ORDBData.ExecNonQuery(strSQLString)
                If rowsaffected = 0 Then
                    lblDBError.Text = "Error Updating ISA_USERS_PRIVS Table"
                    lblDBError.Visible = True
                    Exit Sub
                End If
            End If
        End If

        '  End If
        'If lblAction.Text = "ADD" Then
        If Page_Action = "ADD" Then
            UserCreated = strUSERID
            resetallfields()
        Else

            lblMessage.Text = "User information has been modified and saved successfully."
            setuppasswordfields("REMOVE")
            tr_CpwdFields.Style.Add("display", "none")
            tr_PwdFields.Style.Add("display", "none")
            If Session("USERTYPEVALUE") = "C" Then
                If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                    If Page_Action = "EDIT" Then
                        'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
                        Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))
                    End If
                End If
            End If
        End If

    End Sub

    Private Sub resetallfields()

        txtFirst.Text = ""
        txtLast.Text = ""
        txtEmail.Text = ""
        txtPhoneNum.Text = ""
        txtExt.Text = "+"
        txtUserid.Text = ""
        lblUserIDHide.Text = ""
        lblActiveStatusHide.Text = ""
        txtPassword.Text = ""
        txtConfirm.Text = ""
        txtPassword.Attributes("value") = ""
        txtConfirm.Attributes("value") = ""
        CheckUserID_Val.Text = ""
        CheckUserID_Val.Visible = False
        MultiSiteChk.Checked = False
        rcbGroupTab2.Visible = False
        rcbMultiSelect.Visible = False
        Try
            rcbGroup.SelectedItem.Text = "CSC Agent"
        Catch ex As Exception
        End Try
        txtUserid.ReadOnly = True
        txtUserid.BackColor = LightGray
        Error_LabelSite.Visible = False
        roleDropdownList.ClearSelection()
        roleDropdownList.SelectedItem.Text = "Select Type"
        buildGroupList(rcbGroup)
        'RadcomboforVendorID.ClearSelection()
        drpBUnit.ClearSelection()
        drpBUnit.SelectedValue = "0"
        MainvndrID.Text = ""
        Error_VendorID.Text = ""
        Error_VendorID.ForeColor = Color.Green
        Error_VendorID.Visible = False
        rcbDept.SelectedValue = ""
        lblMessage.Text = "New user-<b>" + UserCreated + "</b> created successfully."
        UserCreated = String.Empty
        'Page.ClientScript.RegisterStartupScript(Me.GetType(), "alert1", "Successmsg(" & UserCreated & ");", True)
        Session.Remove("CountIncrement")
    End Sub

    Private Sub sendEmail(ByVal strGroupName, ByVal strUserType, ByVal strSDICust)
        strSDICust = ConvertSDICustToSDIEmp(strSDICust)
        Dim Mailer = New MailMessage
        Dim strccfirst As String = "pete.doyle"
        Dim strcclast As String = "sdi.com"
        Mailer.to = strccfirst & "@" & strcclast
        Mailer.From = "SDIExchange@SDI.com"
        Mailer.cc = ""
        'If lblAction.Text = "ADD" Then
        If Page_Action = "ADD" Then
            Mailer.subject = "SDiExchange - Profile Added"
            Mailer.body = "SDiExchange - Profile Added" & vbCrLf & vbCrLf
        Else
            Mailer.subject = "SDiExchange - Profile Updated"
            Mailer.body = "SDiExchange - Profile Updated" & vbCrLf & vbCrLf
        End If

        Mailer.body = Mailer.body & "	First Name: " & txtFirst.Text & vbCrLf
        Mailer.body = Mailer.body & "	Last Name:  " & txtLast.Text & vbCrLf
        Mailer.body = Mailer.body & "	Email:      " & txtEmail.Text & vbCrLf
        Mailer.body = Mailer.body & "	Phone:      " & txtPhoneNum.Text & vbCrLf
        Mailer.body = Mailer.body & "	UserID:     " & txtUserid.Text & vbCrLf
        Mailer.body = Mailer.body & "	User Type:  " & strUserType & vbCrLf
        Mailer.body = Mailer.body & "	SDI/CUST:   " & strSDICust & vbCrLf
        Mailer.body = Mailer.body & "	GroupName:  " & strGroupName & vbCrLf
        'SmtpMail.Send(Mailer)
        Insiteonline.WebPartnerFunctions.WebPSharedFunc.sendemail(Mailer)

    End Sub

    Sub setuppasswordfields(ByVal strAction)
        tr_PwdFields.Style.Remove("display")
        tr_CpwdFields.Style.Remove("display")
        If strAction = "SETUP" Or strAction = "PWCHANGE" Then
            btnChangePassw.Visible = False
            lblPassword.Visible = True
            lblConfirm.Visible = True
            txtPassword.Visible = True
            txtConfirm.Visible = True
            If strAction = "PWCHANGE" Then
                valPassword.Enabled = True
                valConfirm.Enabled = True
                valConfirm2.Enabled = True
            End If
        Else
            If Session("PUNCHIN") = "YES" Then
                btnChangePassw.Visible = False
            Else
                btnChangePassw.Visible = True
            End If
            lblPassword.Visible = False
            lblConfirm.Visible = False
            txtPassword.Visible = False
            txtConfirm.Visible = False
        End If

    End Sub

    Protected Sub turnvalidationoff()
        valFirst.Enabled = False
        valLast.Enabled = False
        valPassword.Enabled = False
        valEmail1.Enabled = False
        valEmail2.Enabled = False
        valConfirm.Enabled = False
        valuserid1.Enabled = False
        valUserid2.Enabled = False
        valGroup.Enabled = False
        valPhone1.Enabled = False
        valPhone2.Enabled = False
        valConfirm2.Enabled = False
        valType.Enabled = False
    End Sub

    Protected Sub valUserid2_ServerValidate(ByVal source As System.Object, ByVal args As System.Web.UI.WebControls.ServerValidateEventArgs) Handles valUserid2.ServerValidate

        Dim buValue As String
        If Session("ROLE") = "SUPER" Then
            buValue = rcbGroup.SelectedValue
        Else
            buValue = txtGroupID.Text.Trim
        End If

        Dim Validate_bool As Boolean = Verify_UserID(buValue)

        If Validate_bool = True Then
            Dim bolUseridexist As Boolean = ExistsUserid()  '  checkUserid()
            If bolUseridexist = True Then
                'valUserid2.ErrorMessage = "User ID already exists"
                valUserid2.IsValid = False
                args.IsValid = False
                valUserid2.ErrorMessage = "User ID already exists"
            End If
        Else
            Dim Mask_Value As String = GetMaskValues_BU(buValue)
            If Mask_Value <> "" Then
                valUserid2.IsValid = False
                args.IsValid = False
                valUserid2.ErrorMessage = "Please enter UserID in this <b>'" & Mask_Value & "'</b> format"
            Else
                valUserid2.IsValid = False
                args.IsValid = False
                valUserid2.ErrorMessage = "Invalid User ID"
            End If
        End If

    End Sub

    'Private Sub dropSelectUser_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dropSelectUser.SelectedIndexChanged
    '    drpBUnit.Visible = False
    '    lblBusUnit.Visible = False
    '    lblSelectUser.Visible = True
    '    lblPassword.Visible = False
    '    lblConfirm.Visible = False
    '    'TextBox_VendorId.Text = ""
    '    Dim sss As String = dropSelectUser.SelectedValue
    '    Dim selecteuserdrpdwn As String = Session("SelectUserDrpdwn")
    '    buildEditUser(selecteuserdrpdwn)
    '    Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Session("SelectUserDrpdwn")))

    '    'buildEditUser(dropSelectUser.SelectedValue)
    '    'buildSelectDropDown()
    '    If Session("PUNCHIN") = "YES" Then
    '        btnChangePassw.Visible = False
    '    Else
    '        btnChangePassw.Visible = True
    '    End If

    'End Sub

    Private Sub btnChangePassw_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnChangePassw.Click
        Session("ChangePWDEnabled") = "YES"
        setuppasswordfields("PWCHANGE")
        If Session("USERTYPE") = "SUPER" Then
            If Page_Action = "EDIT" Then
                If radioUserType.SelectedValue = "S" Then
                    lblSelectUser.Visible = True
                    roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                ElseIf radioUserType.SelectedValue = "V" Then
                    lblSelectUser.Visible = True
                    Label_VendorID.Visible = True
                    tr_BU_unit_field.Style.Remove("display")
                    drpBUnit.Visible = True
                    lblBusUnit.Visible = True
                    Radcombobox_vendorID()
                    'RadcomboforVendorID.Visible = True
                    MainvndrID.Visible = True
                    lblGroup.Visible = False
                    rcbGroup.Visible = False
                    MultiSiteChk.Visible = False
                    roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    rcbMultiSelect.Visible = False
                Else
                    lblSelectUser.Visible = True
                    roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                End If
            Else

            End If
        End If

        If Session("USERTYPEVALUE") = "S" Then
            If Session("USERTYPE") = "SUPER" Then
                If radioUserType.SelectedValue = "S" Then
                    lblDept.Visible = True
                ElseIf radioUserType.SelectedValue = "C" Then


                Else

                End If
            ElseIf Session("USERTYPE") = "ADMIN" Then
                If radioUserType.SelectedValue = "S" Then
                    Label_usrtype.Visible = False
                    radioUserType.Visible = False
                    lblGroup.Visible = True
                    rcbdropSelectUser.Visible = True
                    lblSelectUser.Visible = True
                ElseIf radioUserType.SelectedValue = "C" Then
                    Label_usrtype.Visible = False
                    radioUserType.Visible = False
                    lblGroup.Visible = True
                    rcbdropSelectUser.Visible = True
                    lblSelectUser.Visible = True
                Else

                End If
                If Session("USERID") = rcbdropSelectUser.SelectedValue Then
                    lblDept.Visible = True
                End If
            Else
                Label_usrtype.Visible = False
                lblGroup.Visible = True
                radioUserType.Visible = False
                rcbDept.Visible = False
            End If
        ElseIf Session("USERTYPEVALUE") = "C" Then
            If Session("USERTYPE") = "CORPADMIN" Then
                If radioUserType.SelectedValue = "S" Then

                ElseIf radioUserType.SelectedValue = "C" Then
                    Label_usrtype.Visible = False
                    lblGroup.Visible = True
                    lblSelectUser.Visible = True
                Else

                End If
            ElseIf Session("USERTYPE") = "ADMIN" Then
                If radioUserType.SelectedValue = "S" Then

                ElseIf radioUserType.SelectedValue = "C" Then
                    Label_usrtype.Visible = False
                    lblGroup.Visible = True
                    lblSelectUser.Visible = True
                Else

                End If
            Else
                Label_usrtype.Visible = False
                lblGroup.Visible = True
            End If
        Else

        End If
    End Sub

    Private Sub btnAccess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAccess.Click

        If Session("BUSUNIT") = "" Then
            Session.RemoveAll()
            Response.Redirect("default.aspx")
        End If

        Dim strBU As String
        Dim strMessage As New Alert
        If Session("USERTYPE") = "SUPER" Then

            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
            If strBU = "" Then
                ltlAlert.Text = strMessage.Say("Error - No Business Unit Selected!")
                Exit Sub
            End If
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If

        If strBU = "0" Then
            ltlAlert.Text = strMessage.Say("Error - Invalid BU - check productview id's!")
            Exit Sub
        End If


        ''new RadWindow code
        Dim tmp As String = ""  '  http://" & Request.ServerVariables("HTTP_HOST") & GetWebAppName1()
        tmp = "accessprivs.aspx?userid=" & txtUserid.Text & "&userbu=" & strBU & "&operid=" & Session("USERID") & "&usertype=" & Session("USERTYPE") & "&FROMRAD=RAD"

        RadWindowProfilePrivs.NavigateUrl = tmp
        RadWindowProfilePrivs.OnClientClose = "OnClientCloseProfile"
        RadWindowProfilePrivs.Title = "Update Access Privileges"
        Dim script As String = "function f(){$find(""" + RadWindowProfilePrivs.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", script, True)

    End Sub

    Private Sub btnStatEml_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStatEml.Click

        If Session("BUSUNIT") = "" Then
            Session.RemoveAll()
            Response.Redirect("default.aspx")
        End If

        Dim strBU As String
        Dim strMessage As New Alert
        If Session("USERTYPE") = "SUPER" Then
            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If
        'pppppppppppppppfd
        If strBU = "0" Then
            ltlAlert.Text = strMessage.Say("Error - Invalid Business Unit - check productview id's!")
            Exit Sub
        End If

        Dim strScript As String = ""

        strScript = "<script>"
        strScript = strScript & "window.showModalDialog('AccOrdStatEml.aspx?userid=" & txtUserid.Text & "&userbu=" & strBU & "&operid=" & Session("USERID") & "&usertype=" & Session("USERTYPE") & "',null,'status:no;dialogWidth:700px;dialogHeight:590px;dialogHide:true;help:no;scroll:yes');"
        'strScript = strScript & "window.open('accessprivs.aspx?userid=" & txtUserid.Text & "&userbu=" & strBU & "&operid=" & Session("USERID") & "&usertype=" & Session("USERTYPE") & "',null,'height=500,width=700,status=yes,toolbar=no,menubar=no,location=no,left=50,top=50');"
        'strScript = strScript & "document.frmProfileContent.submit();"
        strScript = strScript & "document.forms[0].submit();"
        strScript = strScript & "</script>"
        Page.RegisterStartupScript("ClientScript", strScript)

    End Sub

    Private Sub btnApprovals_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnApprovals.Click
        If Session("BUSUNIT") = "" Then
            Session.RemoveAll()
            Response.Redirect("default.aspx")
        End If

        Dim strBU As String
        Dim strMessage As New Alert
        If Session("USERTYPE") = "SUPER" Then
            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If
        'ppppppppppppppppppppfd
        If strBU = "0" Then
            ltlAlert.Text = strMessage.Say("Error - Invalid Business Unit - check productview id's!")
            Exit Sub
        End If

        Dim sOrdApprType As String = "O"
        'Dim objEnterprise As New clsEnterprise(strBU)
        Select Case sOrdApprType  '  objEnterprise.OrdApprType
            Case "O", "D", "M"
                'OK 
            Case Else
                ltlAlert.Text = strMessage.Say("Business unit is not set up as an approver site.")
                Exit Sub
        End Select

        Dim strScript As String = ""

        strScript = "<script>"
        strScript = strScript & "window.showModalDialog('approvals.aspx?userid=" & txtUserid.Text & "&userbu=" & strBU & "&operid=" & Session("USERID") & "&usertype=" & Session("USERTYPE") & "',null,'status:no;dialogWidth:750px;dialogHeight:500px;dialogHide:true;help:no;scroll:no');"
        'strScript = strScript & "window.open('accessprivs.aspx?userid=" & txtUserid.Text & "&userbu=" & strBU & "&operid=" & Session("USERID") & "&usertype=" & Session("USERTYPE") & "',null,'height=500,width=700,status=yes,toolbar=no,menubar=no,location=no,left=50,top=50');"
        'strScript = strScript & "document.frmProfileContent.submit();"
        strScript = strScript & "document.forms[0].submit();"
        strScript = strScript & "</script>"
        Page.RegisterStartupScript("ClientScript", strScript)
    End Sub

    'Private Sub btnEdit_Clicker(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEdit.Clicker

    '    If Trim(lblMexico.Text) = "" Then
    '        Response.Redirect("profile.aspx?type=EDIT&VENDOR=" & lblVendor.Text)
    '    Else
    '        Response.Redirect("profile.aspx?type=EDIT&MEXICO=" & lblVendor.Text)
    '    End If
    'End Sub

    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        lblMessage.Text = ""
        Session("ChangePWDEnabled") = ""
        'lblMessage.Visible = False
        Session("PageAction") = "ADD"

        lblAccountDisabled.Text = ""
        lblEmplAccountDisabled.Text = ""

        tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = False
        tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
        tbStripUserDetails.Tabs.FindTabByValue("PREF").Visible = False
        tbStripUserDetails.Tabs.FindTabByValue("TST").Visible = False
        tbStripUserDetails.Tabs.FindTabByValue("MOB").Visible = False
        tbStripUserDetails.Tabs.FindTabByValue("ZEUS").Visible = False
        'RadcomboforVendorID.ClearSelection()
        MainvndrID.Text = ""
        Error_VendorID.Text = ""
        Error_VendorID.ForeColor = Color.Red
        Error_VendorID.Visible = False
        txtUserid.Visible = True
        lblUserid.Visible = True
        Label_usrtype.Visible = True
        radioUserType.Visible = True
        Label_rolefield.Visible = True
        roleDropdownList.Visible = True
        lblFirst.Visible = True
        txtFirst.Visible = True
        lblLast.Visible = True
        txtLast.Visible = True
        lblEmail.Visible = True
        txtEmail.Visible = True
        lblPhone1.Visible = True
        txtPhoneNum.Visible = True
        txtExt.Visible = True
        lblPassword.Visible = True
        txtPassword.Visible = True
        lblConfirm.Visible = True
        txtConfirm.Visible = True
        lblSelectUser.Visible = False
        'dropSelectUser.Visible = False
        rcbdropSelectUser.Visible = False
        btnEdit.Visible = True
        btnAdd.Visible = False
        btnChangePassw.Visible = False
        Label_usrtype.Visible = True
        lblMessage.Text = " "
        buildGroupList(rcbGroup)
        roleDropdownList.Items.Insert(0, New ListItem("Select Type", "0"))
        roleDropdownList.SelectedIndex = 0
        tr_PwdFields.Style.Remove("display")
        tr_CpwdFields.Style.Remove("display")
        tr_selectuser_fields.Style.Add("display", "none")
        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
        rcbDept.SelectedValue = ""
        If Session("USERTYPEVALUE") = "S" Then
            rcbGroup.ClearSelection()
            rcbGroup.SelectedItem.Text = "CSC Agent"
            MultiSiteChk.Checked = False
            rcbMultiSelect.ClearSelection()
            rcbMultiSelect.Visible = False
            rcbMultiSelect.EnableViewState = False

            If Session("ROLE") = "SUPER" Then
                If radioUserType.SelectedValue = "S" Then
                    'RadcomboforVendorID.Visible = False
                    MainvndrID.Visible = False
                    txtUserid.Text = ""
                    txtFirst.Text = ""
                    txtLast.Text = ""
                    txtEmail.Text = ""
                    txtPhoneNum.Text = ""
                    txtExt.Text = "+"
                    txtPassword.Text = ""
                    txtConfirm.Text = ""
                    'TextBox_VendorId.Text = ""
                    txtUserid.Enabled = False
                    lblGroup.Visible = True
                    rcbGroup.Visible = True
                    lblBusUnit.Visible = False
                    drpBUnit.Visible = False
                    MultiSiteChk.Visible = True
                    lblDept.Visible = True
                    rcbGroup.Visible = True
                    'tr_Dept_details_fields.Style.Remove("display")
                ElseIf radioUserType.SelectedValue = "C" Then
                    'RadcomboforVendorID.Visible = False
                    MainvndrID.Visible = False
                    txtUserid.Text = ""
                    txtFirst.Text = ""
                    txtLast.Text = ""
                    txtEmail.Text = ""
                    txtPhoneNum.Text = ""
                    txtExt.Text = "+"
                    txtPassword.Text = ""
                    txtConfirm.Text = ""
                    'TextBox_VendorId.Text = ""
                    txtUserid.Enabled = False
                    lblGroup.Visible = True
                    rcbGroup.Visible = True
                    MultiSiteChk.Visible = True
                    lblBusUnit.Visible = False
                    drpBUnit.Visible = False
                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True

                    lblDept.Visible = False
                    rcbDept.Visible = False
                    'tr_Dept_details_fields.Style.Add("display", "none")
                ElseIf radioUserType.SelectedValue = "V" Then
                    rcbGroup.Visible = False
                    'RadcomboforVendorID.Visible = True
                    MainvndrID.Visible = True
                    Radcombobox_vendorID()
                    txtUserid.Text = ""
                    txtFirst.Text = ""
                    txtLast.Text = ""
                    txtEmail.Text = ""
                    txtPhoneNum.Text = ""
                    txtExt.Text = "+"
                    txtPassword.Text = ""
                    txtConfirm.Text = ""
                    'TextBox_VendorId.Text = ""
                    txtUserid.Enabled = False
                    lblBusUnit.Visible = True
                    drpBUnit.Visible = True

                    lblDept.Visible = False
                    rcbDept.Visible = False
                    'tr_Dept_details_fields.Style.Add("display", "none")
                End If
            ElseIf Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                'RadcomboforVendorID.Visible = False
                MainvndrID.Visible = False
                txtUserid.Text = ""
                txtFirst.Text = ""
                txtLast.Text = ""
                txtEmail.Text = ""
                txtPhoneNum.Text = ""
                txtExt.Text = "+"
                txtPassword.Text = ""
                txtConfirm.Text = ""
                radioUserType.SelectedValue = "C"
                roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                radioUserType.Enabled = False
                txtUserid.Enabled = False
                lblGroup.Visible = True
                rcbGroup.Visible = False
                MultiSiteChk.Visible = True
                lblBusUnit.Visible = False
                drpBUnit.Visible = False
                roleDropdownList.Enabled = True
                radioUserType.Visible = False
                Label_usrtype.Visible = False
                If Session("Flag_AddUser") = "V" Then
                    txtUserid.ReadOnly = False
                    txtUserid.BackColor = White
                    txtUserid.Enabled = True
                End If
                lblDept.Visible = False
                rcbDept.Visible = False
                tr_Dept_details_fields.Style.Add("display", "none")
            ElseIf Session("ROLE") = "USERS" Then

                'RadcomboforVendorID.Visible = False
                MainvndrID.Visible = False
                If radioUserType.SelectedValue = "S" Then
                ElseIf radioUserType.SelectedValue = "C" Then
                ElseIf radioUserType.SelectedValue = "V" Then

                End If
                lblDept.Visible = True
                rcbDept.Visible = True
                'tr_Dept_details_fields.Style.Remove("display")
            End If
        ElseIf Session("USERTYPEVALUE") = "C" Then
            lblDept.Visible = False
            rcbDept.Visible = False
            'tr_Dept_details_fields.Style.Add("display", "none")
            'RadcomboforVendorID.Visible = False
            MainvndrID.Visible = False
            txtUserid.Text = ""
            txtFirst.Text = ""
            txtLast.Text = ""
            txtEmail.Text = ""
            txtPhoneNum.Text = ""
            txtExt.Text = "+"
            txtPassword.Text = ""
            txtConfirm.Text = ""
            MultiSiteChk.Visible = True
            MultiSiteChk.Enabled = True
            rcbMultiSelect.Enabled = True
            radioUserType.Visible = False
            Label_usrtype.Visible = False
            roleDropdownList.Enabled = True
            btnActivateAccount.Visible = False
            btnInactivateAccount.Visible = False
            btnEmplActivateAccount.Visible = False
            btnEmplInactivateAccount.Visible = False
            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            rcbGroup.Visible = False
            If Session("Flag_AddUser") = "V" Then
                txtUserid.ReadOnly = False
                txtUserid.BackColor = White
                txtUserid.Enabled = True
            End If
            If Session("ROLE") = "CORPADMIN" Then
                MultiSiteChk.Visible = False
                PLMultiSelect.Visible = False
                rcbGroup.Visible = True
                GetSisterSitesBU(Session("BUSUNIT"))
                tr_Multiselect_fields.Style.Add("display", "none")

            End If
            lblGroup.Visible = True
        ElseIf Session("USERTYPEVALUE") = "V" Then

        End If
    End Sub

    Private Sub drpBUnit_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles drpBUnit.SelectedIndexChanged
        buildSelectDropDown()
    End Sub

    Private Sub btnEdit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEdit.Click
        lblMessage.Text = ""
        Session("PageAction") = "EDIT"
        If Trim(lblMexico.Text) = "" Then
            Response.Redirect("profile.aspx")
        Else
            Response.Redirect("profile.aspx")
        End If
    End Sub

    Protected Sub tbStripUserDetails_TabClick(ByVal sender As Object, ByVal e As RadTabStripEventArgs)
        RadMultiPage1.RenderSelectedPageOnly = True
        RadMultiPage1.SelectedIndex = tbStripUserDetails.SelectedIndex
        'System.Threading.Thread.Sleep(1000)
        Dim position As New AjaxLoadingPanelBackgroundPosition()
        position = AjaxLoadingPanelBackgroundPosition.Center
        RadAjaxLoadingPanel1.BackgroundTransparency = 25
        lblMessage.Text = ""
        Dim DsUPVL As DataSet
        Dim Is_Vendor As String
        Dim BUvalue As String
        Dim SQLSTRINGQuery As String

        If Trim(txtUserid.Text) <> "" Then
            SQLSTRINGQuery = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text) & "'"
        Else
            SQLSTRINGQuery = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & Trim(Session("USERID")) & "'"
        End If
        DsUPVL = ORDBData.GetAdapter(SQLSTRINGQuery)
        If DsUPVL.Tables(0).Rows.Count = 1 Then
            Is_Vendor = DsUPVL.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
            BUvalue = DsUPVL.Tables(0).Rows(0).Item("BUSINESS_UNIT")
        End If

        Select Case e.Tab.Value
            Case "UDTL"
                If Session("USERTYPEVALUE") = "C" Then
                    If Session("ROLE") = "ADMIN" Then
                        ''roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        If MultiSiteChk.Checked Then
                            rcbMultiSelect.Visible = True
                        Else
                            rcbMultiSelect.Visible = False
                        End If
                    End If
                End If
                If Session("USERTYPE") = "SUPER" Then
                    If Page_Action = "EDIT" Then
                        If radioUserType.SelectedValue = "S" Then
                            lblSelectUser.Visible = True
                            roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                            lblDept.Visible = True

                        ElseIf radioUserType.SelectedValue = "V" Then
                            lblSelectUser.Visible = True
                            Label_VendorID.Visible = True
                            tr_BU_unit_field.Style.Remove("display")
                            drpBUnit.Visible = True
                            lblBusUnit.Visible = True
                            Radcombobox_vendorID()
                            'RadcomboforVendorID.Visible = True
                            MainvndrID.Visible = True
                            lblGroup.Visible = False
                            rcbGroup.Visible = False
                            MultiSiteChk.Visible = False
                            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                            rcbMultiSelect.Visible = False
                        Else
                            lblSelectUser.Visible = True
                            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                            lblDept.Visible = False
                        End If
                    Else

                    End If
                ElseIf Session("USERTYPE") = "ADMIN" Then
                    If Session("USERTYPEVALUE") = "S" Then
                        drpBUnit.Visible = False
                        lblBusUnit.Visible = False
                        lblSelectUser.Visible = True
                        'dropSelectUser.Visible = True
                        rcbdropSelectUser.Visible = True
                        lblGroup.Visible = True
                        btnAdd.Visible = True
                        lblPassword.Visible = False
                        lblConfirm.Visible = False
                        radioUserType.Visible = False
                        Label_usrtype.Visible = False
                        tr_PwdFields.Style.Add("display", "none")
                        tr_CpwdFields.Style.Add("display", "none")
                        tr_BU_unit_field.Style.Add("display", "none")
                        MultiSiteChk.Visible = True
                        rcbGroup.Visible = False
                        If Page_Action = "ADD" Then
                            lblGroup.Visible = True
                            rcbGroup.Visible = False
                            MultiSiteChk.Visible = True
                        End If
                        If roleDropdownList.SelectedValue = "CORPADMIN" Then
                            MultiSiteChk.Visible = False
                        End If
                        If Session("USERID") = rcbdropSelectUser.SelectedValue Then
                            lblDept.Visible = True
                        End If
                    ElseIf Session("USERTYPEVALUE") = "C" Then
                        lblGroup.Visible = True
                    End If
                ElseIf Session("USERTYPE") = "USER" Then
                    If Session("USERTYPEVALUE") = "S" Then
                        lblSelectUser.Visible = False
                        'TextBox_VendorId.Visible = False
                        lblBusUnit.Visible = False
                        drpBUnit.Visible = False
                        lblSelectUser.Visible = False
                        'dropSelectUser.Visible = False
                        rcbdropSelectUser.Visible = False
                        Label_usrtype.Visible = False
                        radioUserType.Visible = False
                        lblGroup.Visible = True
                        btnAdd.Visible = False
                        lblPassword.Visible = False
                        lblConfirm.Visible = False
                        rcbGroup.Visible = False
                        tr_PwdFields.Style.Add("display", "none")
                        tr_CpwdFields.Style.Add("display", "none")
                        tr_BU_unit_field.Style.Add("display", "none")
                        tr_selectuser_fields.Style.Add("display", "none")
                        ''tr_Multiselect_fields.Style.Add("display", "none")
                        lblDept.Visible = True
                        rcbDept.Visible = True
                        btnAdd.Visible = False
                        btnEdit.Visible = False
                        SDIUsers()
                    ElseIf Session("USERTYPEVALUE") = "C" Then
                        CustomersUsers()
                    End If
                ElseIf Session("USERTYPE") = "CORPADMIN" Then
                    If Page_Action = "EDIT" Then
                        CustomersUsers()
                        rcbGroup.Visible = True
                        MultiSiteChk.Visible = False
                        rcbMultiSelect.Visible = False
                        If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        Else
                            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        End If
                    Else

                    End If
                End If
                If btnChangePassw.Visible = True Then
                    lblPassword.Visible = False
                    lblConfirm.Visible = False
                Else
                    lblPassword.Visible = True
                    lblConfirm.Visible = True
                End If
                Exit Select
            Case "UPVL"
                If Session("BUSUNIT") = "" Then
                    Session.RemoveAll()
                    Response.Redirect("default.aspx")
                End If

                Dim strBU As String
                Dim strMessage As New Alert

                If Session("USERTYPE") = "SUPER" Then
                    If Is_Vendor = "V" Then
                        strBU = m_cUserGroup_Vendor
                    Else
                        strBU = GetBUbyGroup(BUvalue)
                    End If


                    If strBU = "" Then
                        RadMultiPage1.RenderSelectedPageOnly = True
                        RadMultiPage1.SelectedIndex = 0
                        tbStripUserDetails.Tabs(0).Selected = True
                        ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - No Business Unit Selected!"), True)
                        Exit Sub
                    End If
                Else
                    strBU = GetBUbyGroup(txtGroupID.Text)
                End If

                If strBU = "0" Then
                    RadMultiPage1.RenderSelectedPageOnly = True
                    RadMultiPage1.SelectedIndex = 0
                    tbStripUserDetails.Tabs(0).Selected = True
                    ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - Invalid BU - check productview id's!"), True)
                    Exit Sub
                End If

                Dim sDisplayedSDICust As String = ""
                Dim sDisplayedUserType As String = ""
                'GetDisplayedUserType(sDisplayedSDICust, sDisplayedUserType)
                sDisplayedSDICust = DsUPVL.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
                sDisplayedUserType = DsUPVL.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")

                If checkUserid() Then
                    LoadUserprivilegesData(strBU, sDisplayedSDICust, sDisplayedUserType)
                Else
                    RadMultiPage1.RenderSelectedPageOnly = True
                    RadMultiPage1.SelectedIndex = 0
                    tbStripUserDetails.Tabs(0).Selected = True
                    If Page_Action = "EDIT" Then
                        ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Please select a user in User Detail before editing privileges!"), True)
                    Else
                        ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Please save User Detail data before editing privileges!"), True)
                    End If
                    Exit Sub
                End If

                'Added for hiding privilege type, program tree, expand , collapse , select all and deselect all button for corp admin
                If Session("ROLE") = "CORPADMIN" Then
                    lblPrivilegeType.Visible = False
                    rblType.Visible = False
                    lblProgram.Visible = False
                    rtvPrograms.Visible = False
                    btnExpandAll.Visible = False
                    btnCollapseAll.Visible = False
                    btnSelectAll.Visible = False
                    btnDeselectAll.Visible = False
                    GetUserAccessType()
                End If


                lblMessage1N.Text = ""
                Label1.Text = ""
                Exit Select
            Case "APP"
                If Session("BUSUNIT") = "" Then
                    Session.RemoveAll()
                    Response.Redirect("default.aspx")
                End If
                txtAppTotal.Text = ""
                lblMsg.Text = ""
                Dim strBU As String
                Dim strMessage As New Alert
                If Session("USERTYPE") = "SUPER" Then
                    strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
                Else
                    strBU = GetBUbyGroup(txtGroupID.Text)
                End If
                If strBU = "0" Then
                    RadMultiPage1.RenderSelectedPageOnly = True
                    RadMultiPage1.SelectedIndex = 0
                    tbStripUserDetails.Tabs(0).Selected = True
                    'ltlAlert.Text = strMessage.Say("Error - Invalid Business Unit - check productview id's!"
                    'ltlAlert.Text = "Error - Invalid Business Unit - check productview id''s!"
                    ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - Invalid Business Unit - check productview id!"), True)
                    Exit Sub
                End If

                Dim sOrdApprType As String = "O"
                'Dim objEnterprise As New clsEnterprise(strBU)
                Select Case sOrdApprType  '  objEnterprise.OrdApprType
                    Case "O", "D", "M"
                        'OK 
                    Case Else
                        RadMultiPage1.RenderSelectedPageOnly = True
                        RadMultiPage1.SelectedIndex = 0
                        tbStripUserDetails.Tabs(0).Selected = True
                        'ltlAlert.Text = strMessage.Say("Business unit is not set up as an approver site.")
                        'ltlAlert.Text = "Business unit is not set up as an approver site."
                        ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Business unit is not set up as an approver site."), True)
                        Exit Sub
                End Select
                'LoadApprovals(txtUserid.Text, strBU, Session("USERTYPE"))

                If Session("USERTYPE") = "SUPER" Then
                    'Showing the Budgetory Approval fields
                    lblAppEmpID.Visible = True
                    DropAppEmpID.Visible = True
                    chbDelete.Visible = True
                    lblAppTotal.Visible = True
                    txtAppTotal.Visible = True
                    btnSubmit.Visible = True
                    lblMsg.Visible = True
                    btnSetReqAppr.Visible = True
                    lblSiteBaseCurrencyCode.Text = ""
                    tr_OrderLimit_fields.Style.Add("display", "contents")

                    'Hiding the Requestor Approval fields
                    lblAltReqAppr.Visible = False
                    ddReqAppr.Visible = False
                    chkDeleteReqAppr.Visible = False
                    btnSubmitReqAppr.Visible = False
                    lblMsgReqAppr.Visible = False
                    btnSetBudAppr.Visible = False

                    LoadApprovals(txtUserid.Text, strBU, Session("USERTYPE"))
                Else
                    'Hiding the Budgetory Approval fields
                    lblAppEmpID.Visible = False
                    DropAppEmpID.Visible = False
                    chbDelete.Visible = False
                    lblAppTotal.Visible = False
                    txtAppTotal.Visible = False
                    btnSubmit.Visible = False
                    lblMsg.Visible = False
                    btnSetReqAppr.Visible = False
                    lblSiteBaseCurrencyCode.Text = ""
                    tr_OrderLimit_fields.Style.Add("display", "none")

                    'Showing the Requestor Approval fields
                    lblAltReqAppr.Visible = True
                    ddReqAppr.Visible = True
                    chkDeleteReqAppr.Visible = True
                    btnSubmitReqAppr.Visible = True
                    lblMsgReqAppr.Visible = True
                    btnSetBudAppr.Visible = False

                    LoadReqApprovals(txtUserid.Text, strBU, Session("USERTYPE"))
                End If

                Exit Select
            Case "OSE"
                If Session("BUSUNIT") = "" Then
                    Session.RemoveAll()
                    Response.Redirect("default.aspx")
                End If
                resetIOLCheckboxes()
                Dim strBU As String = ""
                Dim strMessage As New Alert
                If Session("USERTYPE") = "SUPER" Then
                    strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
                Else
                    strBU = GetBUbyGroup(txtGroupID.Text)
                End If
                If strBU = "0" Then
                    RadMultiPage1.RenderSelectedPageOnly = True
                    RadMultiPage1.SelectedIndex = 0
                    tbStripUserDetails.Tabs(0).Selected = True
                    'ltlAlert.Text = strMessage.Say("Error - Invalid Business Unit - check productview id's!")
                    ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - Invalid Business Unit - check productview ids!"), True)
                    Exit Sub
                End If

                LoadOrderStatusEmail(Session("USERTYPE"), strBU, txtUserid.Text)
                Exit Select
            Case "PREF"
                If Session("BUSUNIT") = "" Then
                    Session.RemoveAll()
                    Response.Redirect("default.aspx")
                End If
                LoadPreferences(BUvalue)
                Exit Select
            Case "TST"
                SetUpTrackTab()
                Exit Select
            Case "MOB"
                LoadGRIBIDENTITY()
                Exit Select
            Case "ZEUS"
                LoadZuesValues()
                Exit Select
        End Select
    End Sub

    Private Sub btnTangoAddUser_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTangoAddUser.Click
        AddTangoUser(Session("USERID"), Session("BUSUNIT"), txtTangoUserName, txtTangoPassword, lblTangoPassword, lblValidation, _
                     lblTangoUserNameStored, lblSDiTrackDateTime, lblSDiTrackDateTimeVal, lblSDiTrackGuid, lblSDiTrackGuidVal, _
                     btnTangoAddUser)
    End Sub

#Region "Approvals"
    Private Sub LoadApprovals(ByVal userID As String, ByVal userBU As String, ByVal userType As String)
        Const SESSION_SITE_CURRENCY As String = "__siteCurrency"
        Dim m_siteCurrency As sdiCurrency = Nothing

        Try
            Dim siteBU As String = ""
            Try
                siteBU = CStr(Session("BUSUNIT")).Trim.ToUpper
                If (siteBU Is Nothing) Then
                    siteBU = ""
                End If
            Catch ex As Exception
            End Try
            m_siteCurrency = Nothing
            If Page.IsPostBack Then
                ' retrieve from session var
                Try
                    m_siteCurrency = CType(Session(SESSION_SITE_CURRENCY), sdiCurrency)
                Catch ex As Exception
                End Try
            End If
            If (Not Page.IsPostBack) Or _
               (m_siteCurrency Is Nothing) Then
                m_siteCurrency = sdiMultiCurrency.getSiteCurrency(siteBU)
                Session(SESSION_SITE_CURRENCY) = m_siteCurrency
            End If

            Me.lblSiteBaseCurrencyCode.Text = ""
            Try
                Me.lblSiteBaseCurrencyCode.Text = m_siteCurrency.Id
            Catch ex As Exception
            End Try
            getApprovals(userID, userBU)
            btnSetReqAppr.Visible = True
        Catch ex As Exception

        End Try
    End Sub

    Private Sub getApprovals(ByVal strUserid, ByVal strBU)
        Dim strSQLstring As String
        Dim strAppEmpID As String

        strSQLstring = "SELECT ISA_IOL_APR_EMP_ID, ISA_IOL_APR_LIMIT, ISA_IOL_APR_ALT " & vbCrLf & _
                " FROM SDIX_USERS_APPRV" & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & strUserid & "'" & vbCrLf & _
                " AND BUSINESS_UNIT = '" & strBU & "'"

        Dim dtrAprReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)

        If dtrAprReader.Read Then
            txtAppExist.Text = "UPD"
            strAppEmpID = dtrAprReader.Item("ISA_IOL_APR_EMP_ID")
            txtAppTotal.Text = dtrAprReader.Item("ISA_IOL_APR_LIMIT")
            m_sAppTotalOrig = txtAppTotal.Text
            m_sAppEmpIDOrig = strAppEmpID
            m_sAppAltOrig = dtrAprReader.Item("ISA_IOL_APR_ALT")
        Else
            txtAppExist.Text = "ADD"
            m_sAppTotalOrig = ""
            m_sAppEmpIDOrig = ""
            m_sAppAltOrig = ""
        End If
        dtrAprReader.Close()

        Session("APPR_TOTAL") = m_sAppTotalOrig
        Session("APPR_APR_EMPID") = m_sAppEmpIDOrig
        Session("APPR_APR_ALT") = m_sAppAltOrig

        strSQLstring = "SELECT Distinct (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A JOIN SDIX_MULTI_SITE B" & vbCrLf & _
                        " ON A.ISA_EMPLOYEE_ID=B.ISA_EMPLOYEE_ID WHERE B.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND " & vbCrLf & _
                    "NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' " & vbCrLf & _
                    "UNION SELECT (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A WHERE " & vbCrLf & _
                    "A.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' ORDER BY EMP_TEXT"

        Dim dtrEMPReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
        DropAppEmpID.DataSource = dtrEMPReader
        DropAppEmpID.DataValueField = "ISA_EMPLOYEE_ID"
        DropAppEmpID.DataTextField = "EMP_TEXT"
        DropAppEmpID.DataBind()
        DropAppEmpID.Items.Insert(0, New ListItem("-- ALL --"))
        If Not DropAppEmpID.Items.FindByValue(strAppEmpID) Is Nothing Then
            DropAppEmpID.Items.FindByValue(strAppEmpID).Selected = True
        End If

        dtrEMPReader.Close()

    End Sub

    Sub deleteAPPRVRecord()
        Dim rowsaffected As Integer = 0
        Dim strBU As String = ""
        If Session("USERTYPE") = "SUPER" Then
            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If
        Dim strSQLstring As String = ""
        strSQLstring = "DELETE FROM SDIX_USERS_APPRV" & vbCrLf & _
                    " WHERE ISA_EMPLOYEE_ID = '" & txtUserid.Text & "'" & vbCrLf & _
                    " AND BUSINESS_UNIT = '" & strBU & "'"

        rowsaffected = ORDBData.ExecNonQueryWithTransaction(strSQLstring)

        If rowsaffected > 0 Then
            clsSDIAudit.AddRecord("profile.aspx", "Delete Order Limit Record", "SDIX_USERS_APPRV", Session("USERID").ToString, strBU, txtUserid.Text, _
                      sUDF1:="Limit " & m_sAppTotalOrig, sUDF2:="Approver " & m_sAppEmpIDOrig, sUDF3:="AltApprover " & m_sAppAltOrig)
            m_sAppTotalOrig = ""
            m_sAppEmpIDOrig = ""
            m_sAppAltOrig = ""
            Session("APPR_TOTAL") = m_sAppTotalOrig
            Session("APPR_APR_EMPID") = m_sAppEmpIDOrig
            Session("APPR_APR_ALT") = m_sAppAltOrig

            Label1.Text =
            lblMessage1N.Text = Label1.Text
            lblMessage.Text = "Budgetory Approver details Deleted Sucessfully."
            lblMessage.Visible = True
            lblMsg.Text = "Budgetory Approver details Deleted Sucessfully."

        End If

    End Sub

    Private Sub updateAPPRVTable()
        Dim strBU As String

        If Session("USERTYPE") = "SUPER" Then
            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If

        Dim strSQLstring As String
        Dim bUpdate As Boolean = (txtAppExist.Text = "UPD")

        If Not bUpdate Then
            If ExistsApprvRecord(strBU) Then
                ' Verify that the record doesn't already exist if we're to insert the record.
                ' If the record already exists, change the "insert" action to an "update" action.
                bUpdate = True
            End If
        End If

        If bUpdate Then
            strSQLstring = "UPDATE SDIX_USERS_APPRV" & vbCrLf & _
                " SET ISA_IOL_APR_EMP_ID = '" & DropAppEmpID.SelectedValue & "'," & vbCrLf & _
                " ISA_IOL_APR_ALT = '" & DropAppEmpID.SelectedValue & "'," & vbCrLf & _
                " ISA_IOL_APR_LIMIT = " & Convert.ToDecimal(txtAppTotal.Text) & "," & vbCrLf & _
                " LASTUPDOPRID = '" & Session("USERID") & "'," & vbCrLf & _
                " LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & txtUserid.Text & "'" & vbCrLf & _
                " AND BUSINESS_UNIT = '" & strBU & "'"

        Else
            strSQLstring = "INSERT INTO SDIX_USERS_APPRV" & vbCrLf & _
                " (ISA_EMPLOYEE_ID," & vbCrLf & _
                " BUSINESS_UNIT," & vbCrLf & _
                " ISA_IOL_APR_EMP_ID," & vbCrLf & _
                " ISA_IOL_APR_LIMIT," & vbCrLf & _
                " ISA_IOL_APR_ALT," & vbCrLf & _
                " LASTUPDOPRID," & vbCrLf & _
                " LASTUPDDTTM)" & vbCrLf & _
                " VALUES('" & txtUserid.Text & "'," & vbCrLf & _
                " '" & strBU & "'," & vbCrLf & _
                " '" & DropAppEmpID.SelectedValue & "'," & vbCrLf & _
                " " & Convert.ToDecimal(txtAppTotal.Text) & "," & vbCrLf & _
                " '" & DropAppEmpID.SelectedValue & "'," & vbCrLf & _
                " '" & Session("USERID") & "'," & vbCrLf & _
                " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
        End If

        Dim rowsaffected As Integer
        rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

        If rowsaffected > 0 Then
            If bUpdate Then
                Dim decOrigTotal As Decimal = 0
                Dim decNewTotal As Decimal = 0
                Try
                    decOrigTotal = CType(m_sAppTotalOrig, Decimal)
                Catch ex As Exception
                    decOrigTotal = 0
                End Try
                Try
                    decNewTotal = CType(txtAppTotal.Text.ToString, Decimal)
                Catch ex As Exception
                    decNewTotal = 0
                End Try
                If decOrigTotal <> decNewTotal Then
                    clsSDIAudit.AddRecord("profile.aspx", "Update Order Limit Record", "SDIX_USERS_APPRV", Session("USERID").ToString, strBU, _
                                          txtUserid.Text, sColumnChg:="isa_iol_apr_limit", sOldValue:=m_sAppTotalOrig, sNewValue:=txtAppTotal.Text.ToString)
                    m_sAppTotalOrig = txtAppTotal.Text
                    Session("APPR_TOTAL") = m_sAppTotalOrig
                End If
                If m_sAppEmpIDOrig <> DropAppEmpID.SelectedValue Then
                    clsSDIAudit.AddRecord("profile.aspx", "Update Order Limit Record", "SDIX_USERS_APPRV", Session("USERID").ToString, strBU, _
                                          txtUserid.Text, sColumnChg:="isa_iol_apr_emp_id", sOldValue:=m_sAppEmpIDOrig, sNewValue:=DropAppEmpID.SelectedValue)
                    m_sAppEmpIDOrig = DropAppEmpID.SelectedValue
                    Session("APPR_APR_EMPID") = m_sAppEmpIDOrig
                End If
                If m_sAppAltOrig <> DropAppEmpID.SelectedValue Then
                    clsSDIAudit.AddRecord("profile.aspx", "Update Order Limit Record", "SDIX_USERS_APPRV", Session("USERID").ToString, strBU, _
                                          txtUserid.Text, sColumnChg:="isa_iol_apr_alt", sOldValue:=m_sAppAltOrig, sNewValue:=DropAppEmpID.SelectedValue)
                    m_sAppAltOrig = DropAppEmpID.SelectedValue
                    Session("APPR_APR_ALT") = m_sAppAltOrig
                End If

                lblMessage.Text = "Budgetory Approver details Updated Successfully."
                lblMessage.Visible = True
                lblMsg.Text = "Budgetory Approver details Updated Successfully."

            Else
                clsSDIAudit.AddRecord("profile.aspx", "Insert Order Limit Record", "SDIX_USERS_APPRV", Session("USERID").ToString, strBU, _
                                      txtUserid.Text, sUDF1:="Limit " & txtAppTotal.Text.ToString, sUDF2:="Approver " & DropAppEmpID.SelectedValue)
                m_sAppTotalOrig = txtAppTotal.Text
                m_sAppEmpIDOrig = DropAppEmpID.SelectedValue
                m_sAppAltOrig = DropAppEmpID.SelectedValue
                Session("APPR_TOTAL") = m_sAppTotalOrig
                Session("APPR_APR_EMPID") = m_sAppEmpIDOrig
                Session("APPR_APR_ALT") = m_sAppAltOrig

                lblMessage.Text = "Budgetory Approver details Inserted Sucessfuly."
                lblMessage.Visible = True
                lblMsg.Text = "Budgetory Approver details Inserted Sucessfuly."
            End If
        End If
    End Sub

    Private Sub btnSubmit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSubmit.Click
        If Session("USERTYPE") = "ADMIN" Or _
            Session("USERTYPE") = "ADMINX" Or _
            Session("USERTYPE") = "ADMINA" Or _
            Session("USERTYPE") = "SUPER" Then
            If DropAppEmpID.SelectedIndex = 0 Then
                lblMsg.Text = "A valid approval name must be selected"
                Exit Sub
            Else
                lblMsg.Text = ""
            End If
            If txtAppTotal.Text = "" Then
                txtAppTotal.Text = "0"
            End If
            If chbDelete.Checked Then
                deleteAPPRVRecord()
                chbDelete.Checked = False
                txtAppTotal.Text = ""
                DropAppEmpID.SelectedIndex = 0
            Else
                updateAPPRVTable()
            End If
        End If
    End Sub
#End Region

#Region "Requestor Approvals"
    Private Sub LoadReqApprovals(ByVal userID As String, ByVal userBU As String, ByVal userType As String)
        Try
            'Hiding the Budgetory Approval fields
            lblAppEmpID.Visible = False
            DropAppEmpID.Visible = False
            chbDelete.Visible = False
            lblAppTotal.Visible = False
            txtAppTotal.Visible = False
            btnSubmit.Visible = False
            lblMsg.Visible = False
            'btnSetReqAppr.Visible = False
            lblSiteBaseCurrencyCode.Text = ""
            tr_OrderLimit_fields.Style.Add("display", "none")

            'Showing the Requestor Approval fields
            lblAltReqAppr.Visible = True
            ddReqAppr.Visible = True
            chkDeleteReqAppr.Visible = True
            btnSubmitReqAppr.Visible = True
            lblMsgReqAppr.Visible = True
            'btnSetBudAppr.Visible = True
            lblMsgReqAppr.Text = ""

            If userType = "SUPER" Then
                btnSetReqAppr.Visible = True
                btnSetBudAppr.Visible = False
            Else
                btnSetReqAppr.Visible = False
                btnSetBudAppr.Visible = False
            End If

            Dim strUserID As String = String.Empty
            Dim strBU As String = String.Empty

            If Session("USERTYPE") = "SUPER" Then
                strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
            Else
                strBU = GetBUbyGroup(txtGroupID.Text)
            End If
            strUserID = Trim(txtUserid.Text)

            getReqApprovals(strUserID, strBU)
        Catch ex As Exception

        End Try
    End Sub
    Private Sub getReqApprovals(ByVal strUserid, ByVal strBU)

        Dim strSQLstring As String
        Dim strAppEmpID As String

        strSQLstring = "SELECT ISA_REQ_APR_ALT " & vbCrLf & _
                " FROM SDIX_USERS_REQ_APPRV" & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & strUserid & "'" & vbCrLf & _
                " AND BUSINESS_UNIT = '" & strBU & "'"

        Dim dtrAprReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)

        If dtrAprReader.Read Then
            txtAppExist.Text = "UPD"
            m_sReq_AltAppr_EmpID = strUserid
            m_sReq_AltAppr_Orig = dtrAprReader.Item("ISA_REQ_APR_ALT")
        Else
            txtAppExist.Text = "ADD"
            m_sAppEmpIDOrig = ""
            m_sAppAltOrig = ""
        End If
        dtrAprReader.Close()

        Session("REQ_APPR_EMPID") = m_sReq_AltAppr_EmpID
        Session("REQ_APPR_ALT") = m_sReq_AltAppr_Orig

        'strSQLstring = "SELECT Distinct (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A JOIN SDIX_MULTI_SITE B" & vbCrLf & _
        '                " ON A.ISA_EMPLOYEE_ID=B.ISA_EMPLOYEE_ID WHERE B.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND " & vbCrLf & _
        '            "NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' " & vbCrLf & _
        '            "UNION SELECT (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A WHERE " & vbCrLf & _
        '            "A.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' ORDER BY EMP_TEXT"
        strSQLstring = "SELECT Distinct(A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT, A.ISA_EMPLOYEE_ID FROM PS_ISA_EMPL_TBL A, SDIX_USERS_TBL B" & vbCrLf & _
                        "WHERE A.BUSINESS_UNIT = '" & strBU & "'  AND A.EFF_STATUS = 'A' AND NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND " & vbCrLf & _
                        "B.ISA_EMPLOYEE_ID = A.ISA_EMPLOYEE_ID AND B.ACTIVE_STATUS = 'A' AND B.ISA_SDI_EMPLOYEE = 'C' ORDER BY EMP_TEXT"
        Dim dtrEMPReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
        ddReqAppr.DataSource = dtrEMPReader
        ddReqAppr.DataValueField = "ISA_EMPLOYEE_ID"
        ddReqAppr.DataTextField = "EMP_TEXT"
        ddReqAppr.DataBind()
        ddReqAppr.Items.Insert(0, New ListItem("-- ALL --"))
        If Not ddReqAppr.Items.FindByValue(m_sReq_AltAppr_Orig) Is Nothing Then
            ddReqAppr.Items.FindByValue(m_sReq_AltAppr_Orig).Selected = True
        End If
        dtrEMPReader.Close()
    End Sub

    Sub deleteReqAPPRVRecord()
        Dim rowsaffected As Integer = 0
        Dim strBU As String = ""
        If Session("USERTYPE") = "SUPER" Then
            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If
        Dim strSQLstring As String = ""
        strSQLstring = "DELETE FROM SDIX_USERS_REQ_APPRV" & vbCrLf & _
                    " WHERE ISA_EMPLOYEE_ID = '" & txtUserid.Text & "'" & vbCrLf & _
                    " AND BUSINESS_UNIT = '" & strBU & "'"

        rowsaffected = ORDBData.ExecNonQueryWithTransaction(strSQLstring)
        If rowsaffected > 0 Then
            clsSDIAudit.AddRecord("profile.aspx", "Delete Requestor Approver Record", "SDIX_USERS_REQ_APPRV", Session("USERID").ToString, strBU, txtUserid.Text, _
                      sUDF2:="Approver " & m_sReq_AltAppr_EmpID, sUDF3:="ReqAltApprover " & m_sReq_AltAppr_Orig)
            m_sReq_AltAppr_Orig = ""
            m_sReq_AltAppr_EmpID = ""
            Session("REQ_APPR_EMPID") = m_sReq_AltAppr_EmpID
            Session("REQ_APPR_ALT") = m_sReq_AltAppr_Orig

            lblMsgReqAppr.Text = "Requestor Approver deleted Sucessfuly."
            'lblMessage.Text = "Requestor Approver deleted Sucessfuly."
            'lblMessage.Visible = True

        End If
    End Sub

    Private Sub updateReqAPPRVTable()
        Dim strBU As String

        If Session("USERTYPE") = "SUPER" Then
            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If

        Dim strSQLstring As String
        Dim bUpdate As Boolean = (txtAppExist.Text = "UPD")

        If Not bUpdate Then
            If ExistsReqApprvRecord(strBU) Then
                ' Verify that the record doesn't already exist if we're to insert the record.
                ' If the record already exists, change the "insert" action to an "update" action.
                bUpdate = True
            End If
        End If

        If bUpdate Then
            strSQLstring = "UPDATE SDIX_USERS_REQ_APPRV" & vbCrLf & _
                " SET ISA_REQ_APR_ALT = '" & ddReqAppr.SelectedValue & "'," & vbCrLf & _
                " LASTUPDOPRID = '" & Session("USERID") & "'," & vbCrLf & _
                " LASTUPDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & txtUserid.Text & "'" & vbCrLf & _
                " AND BUSINESS_UNIT = '" & strBU & "'"

        Else
            strSQLstring = "INSERT INTO SDIX_USERS_REQ_APPRV" & vbCrLf & _
                " (ISA_EMPLOYEE_ID," & vbCrLf & _
                " BUSINESS_UNIT," & vbCrLf & _
                " ISA_REQ_APR_ALT," & vbCrLf & _
                " LASTUPDOPRID," & vbCrLf & _
                " LASTUPDTTM)" & vbCrLf & _
                " VALUES('" & txtUserid.Text & "'," & vbCrLf & _
                " '" & strBU & "'," & vbCrLf & _
                " '" & ddReqAppr.SelectedValue & "'," & vbCrLf & _
                " '" & Session("USERID") & "'," & vbCrLf & _
                " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
        End If

        Dim rowsaffected As Integer
        rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

        If rowsaffected > 0 Then
            If bUpdate Then
                If m_sReq_AltAppr_Orig <> ddReqAppr.SelectedValue Then
                    clsSDIAudit.AddRecord("profile.aspx", "Update Requestor Approver Record", "SDIX_USERS_REQ_APPRV", Session("USERID").ToString, strBU, _
                                          txtUserid.Text, sColumnChg:="ISA_REQ_APR_ALT", sOldValue:=m_sReq_AltAppr_Orig, sNewValue:=ddReqAppr.SelectedValue)
                    m_sReq_AltAppr_Orig = ddReqAppr.SelectedValue
                End If

                'lblMessage.Text = "Requestor Approver Updated Sucessfuly."
                'lblMessage.Visible = True
                lblMsgReqAppr.Text = "Requestor Approver Updated Sucessfuly."
            Else
                clsSDIAudit.AddRecord("profile.aspx", "Insert Requestor Approver Record", "SDIX_USERS_REQ_APPRV", Session("USERID").ToString, strBU, _
                                      txtUserid.Text, sUDF2:="ReqAltApprover " & ddReqAppr.SelectedValue)
                m_sReq_AltAppr_Orig = ddReqAppr.SelectedValue
                Session("REQ_APPR_EMPID") = m_sReq_AltAppr_EmpID
                Session("REQ_APPR_ALT") = m_sReq_AltAppr_Orig

                'lblMessage.Text = "Requestor Approver Inserted Sucessfuly."
                'lblMessage.Visible = True
                lblMsgReqAppr.Text = "Requestor Approver Inserted Sucessfuly."
            End If
        End If
    End Sub
#End Region

#Region "Order Status Emails"
    Private Sub LoadOrderStatusEmail(ByVal userType As String, ByVal userBU As String, ByVal userID As String)
        Try
            getPrivleges(userID, userBU)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub execPrivsSQL(ByVal strSQLstring As String, Optional ByVal strUpdType As String = "", _
            Optional ByVal strOpValue As String = "", Optional ByVal strOpName As String = "")

        Dim rowsaffected As Integer = 0
        If Trim(strUpdType) <> "" Then
            If UCase(Trim(strUpdType)) = "INS" Then
                Try
                    rowsaffected = ORDBData.ExecNonQuery(strSQLstring, False, False)
                Catch ex As Exception
                    rowsaffected = 0
                End Try
                If rowsaffected = 0 Then
                    Try
                        If Trim(strOpValue) <> "" And Trim(strOpName) <> "" Then
                            strSQLstring = getSQLprivupdate(strOpName, "IOL", strOpValue, "UPD")
                            rowsaffected = ORDBData.ExecNonQuery(strSQLstring, False)

                        End If

                    Catch exUI As Exception

                    End Try

                End If
            End If
        Else
            Try
                rowsaffected = ORDBData.ExecNonQuery(strSQLstring, False)
            Catch exU As Exception

            End Try

        End If
    End Sub

    Private Sub getPrivleges(ByVal strUserid, ByVal strbu)

        lblUpdMsg.Text = ""
        Dim strItem As String
        Dim hashPrivs As Hashtable
        hashPrivs = New Hashtable
        Dim roleHashPrivs As Hashtable = Nothing

        hashPrivs = getprivhashtable(strUserid, strbu, "Y", roleHashPrivs, "Y")

        If hashPrivs.ContainsKey("NONE") Then
            chbCRE.Checked = False
            chbQTW.Checked = False
            chbQTC.Checked = False
            chbQTS.Checked = False
            chbCST.Checked = False
            chbVND.Checked = False
            chbAPR.Checked = False
            chbQTA.Checked = False
            chbQTR.Checked = False
            chbRFA.Checked = False
            chbRFR.Checked = False
            chbRFC.Checked = False
            chbRCF.Checked = False
            chbRCP.Checked = False
            chbCNC.Checked = False
            chbDLF.Checked = False
            Exit Sub
        End If
        ' ORDER STATUS CODES that are used in PS_ISAORDSTATLOG table
        ' to get the status codes from the variable below:
        ' in the query in program StatusChaneEmail
        ' need to do a substr(ISA_IOL_OP_NAME 0,10)  10th byte is the status code 
        ' saved - EMLSAVED-0   status code = 0
        '0       Saved 
        '1       Submitted 
        '2       Processing Order 
        '3       Ordered 
        '4       Picking 
        '5       Partially Shipped 
        '6       Shipped 
        '7       Picked Order 
        'B       Waiting Budget Approval
        'C       Cancelled 
        'E       Error
        'Q       Waiting Quote
        'W       Waiting Order Approval


        'If hashPrivs.ContainsValue("SUP") Then
        '    resetIOLCheckboxes()
        'Else
        For Each strItem In hashPrivs.Keys
            Select Case strItem
                Case "EMAILCRE01"
                    chbCRE.Checked = True
                Case "EMAILQTW02"
                    chbQTW.Checked = True
                Case "EMAILQTC03"
                    chbQTC.Checked = True
                Case "EMAILQTS04"
                    chbQTS.Checked = True
                Case "EMAILCST05"
                    chbCST.Checked = True
                Case "EMAILVND06"
                    chbVND.Checked = True
                Case "EMAILAPR07"
                    chbAPR.Checked = True
                Case "EMAILQTA08"
                    chbQTA.Checked = True
                Case "EMAILQTR09"
                    chbQTR.Checked = True
                Case "EMAILRFA10"
                    chbRFA.Checked = True
                Case "EMAILRFR11"
                    chbRFR.Checked = True
                Case "EMAILRFC12"
                    chbRFC.Checked = True
                Case "EMAILRCF13"
                    chbRCF.Checked = True
                Case "EMAILRCP14"
                    chbRCP.Checked = True
                Case "EMAILCNC15"
                    chbCNC.Checked = True
                Case "EMAILDLF16"
                    chbDLF.Checked = True
            End Select
        Next
        'If clsAccessPrivileges.IsPrivilgEqualsN(strUserid, strbu, _
        '                    clsAccessPrivileges.UserPrivsEnum.SendEmailRecvdPo) Then  'Case "EMLRECVD"
        '    chbRecvdPo.Checked = False
        'Else
        '    chbRecvdPo.Checked = True
        'End If

        If IsPrivilegeOn(strUserid, strbu, UserPrivsEnum.IncidentAssign) Then
            txtCustSrvFlag.Text = "Y"
        Else
            Dim objUserTbl As New clsUserTbl(strUserid, strbu)
            txtCustSrvFlag.Text = objUserTbl.CustSrvFlag
        End If

        If Not roleHashPrivs Is Nothing Then
            For Each strItem In roleHashPrivs.Keys
                Select Case strItem
                    Case "EMAILCRE01"
                        chbCRE.Enabled = True
                    Case "EMAILQTW02"
                        chbQTW.Enabled = True
                    Case "EMAILQTC03"
                        chbQTC.Enabled = True
                    Case "EMAILQTS04"
                        chbQTS.Enabled = True
                    Case "EMAILCST05"
                        chbCST.Enabled = True
                    Case "EMAILVND06"
                        chbVND.Enabled = True
                    Case "EMAILAPR07"
                        chbAPR.Enabled = True
                    Case "EMAILQTA08"
                        chbQTA.Enabled = True
                    Case "EMAILQTR09"
                        chbQTR.Enabled = False
                    Case "EMAILRFA10"
                        chbRFA.Enabled = True
                    Case "EMAILRFR11"
                        chbRFR.Enabled = True
                    Case "EMAILRFC12"
                        chbRFC.Enabled = True
                    Case "EMAILRCF13"
                        chbRCF.Enabled = True
                    Case "EMAILRCP14"
                        chbRCP.Enabled = True
                    Case "EMAILCNC15"
                        chbCNC.Enabled = True
                    Case "EMAILDLF16"
                        chbDLF.Enabled = True
                End Select
            Next  ' For Each strItem In roleHashPrivs.Keys
            lblOrdStatusEml.Text = UCase(strUserid) & " is assigned a Role."
        End If ' If Not roleHashPrivs Is Nothing Then

    End Sub

    Function getSQLprivupdate(ByVal strOpName, ByVal strOpType, ByVal strOpValue, ByVal strUpdType) As String

        Dim strBU As String
        If Session("USERTYPE") = "SUPER" Then
            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If

        If strUpdType = "UPD" Then
            getSQLprivupdate = "UPDATE SDIX_USERS_PRIVS" & vbCrLf &
                " SET ISA_IOL_OP_VALUE = '" & strOpValue & "'," & vbCrLf &
                " LASTUPDOPRID = '" & Session("USERID") & "'," & vbCrLf &
                " LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & txtUserid.Text.ToUpper & "'" & vbCrLf &
                " AND BUSINESS_UNIT = '" & strBU.ToUpper & "'" & vbCrLf &
                " AND ISA_IOL_OP_NAME = '" & strOpName & "'"
        Else
            getSQLprivupdate = "INSERT INTO SDIX_USERS_PRIVS" & vbCrLf &
                " (ISA_EMPLOYEE_ID," & vbCrLf &
                " BUSINESS_UNIT," & vbCrLf &
                " ISA_IOL_OP_NAME," & vbCrLf &
                " ISA_IOL_OP_VALUE," & vbCrLf &
                " ISA_IOL_OP_TYPE," & vbCrLf &
                " LASTUPDOPRID," & vbCrLf &
                " LASTUPDDTTM)" & vbCrLf &
                " VALUES('" & txtUserid.Text.ToUpper & "'," & vbCrLf &
                " '" & strBU & "'," & vbCrLf &
                " '" & strOpName & "'," & vbCrLf &
                " '" & strOpValue & "'," & vbCrLf &
                " '" & strOpType & "'," & vbCrLf &
                " '" & Session("USERID") & "'," & vbCrLf &
                " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
        End If
    End Function

    Sub resetIOLCheckboxes()
        chbCRE.Checked = False
        chbQTW.Checked = False
        chbQTC.Checked = False
        chbQTS.Checked = False
        chbCST.Checked = False
        chbVND.Checked = False
        chbAPR.Checked = False
        chbQTA.Checked = False
        chbQTR.Checked = False
        chbRFA.Checked = False
        chbRFR.Checked = False
        chbRFC.Checked = False
        chbRCF.Checked = False
        chbRCP.Checked = False
        chbCNC.Checked = False
        chbDLF.Checked = False
    End Sub

    Private Sub btnOrdStatEmailSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnOrdStatEmailSubmit.Click
        updatePrivsTable()
    End Sub

    Sub updatePrivsTable()
        Dim strOpValue As String
        Dim strPrivSQL As String

        Dim strBU As String
        If Session("USERTYPE") = "SUPER" Then
            strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
        Else
            strBU = GetBUbyGroup(txtGroupID.Text)
        End If

        Dim hashPrivs As Hashtable
        hashPrivs = New Hashtable
        Dim roleHashPrivs As Hashtable = Nothing
        hashPrivs = getprivhashtable(txtUserid.Text, strBU, "N", roleHashPrivs, "Y")

        If chbCRE.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILCRE01") Then
            strPrivSQL = getSQLprivupdate("EMAILCRE01", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbCRE.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILCRE01", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILCRE01")
            End If
        End If

        If chbQTW.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILQTW02") Then
            strPrivSQL = getSQLprivupdate("EMAILQTW02", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbQTW.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILQTW02", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILQTW02")
            End If
        End If

        If chbQTC.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILQTC03") Then
            strPrivSQL = getSQLprivupdate("EMAILQTC03", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbQTC.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILQTC03", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILQTC03")
            End If
        End If

        If chbQTS.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILQTS04") Then
            strPrivSQL = getSQLprivupdate("EMAILQTS04", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbQTS.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILQTS04", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILQTS04")
            End If
        End If

        If chbCST.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILCST05") Then
            strPrivSQL = getSQLprivupdate("EMAILCST05", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbCST.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILCST05", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILCST05")
            End If
        End If

        If chbVND.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILVND06") Then
            strPrivSQL = getSQLprivupdate("EMAILVND06", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbVND.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILVND06", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILVND06")
            End If
        End If

        If chbAPR.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILAPR07") Then
            strPrivSQL = getSQLprivupdate("EMAILAPR07", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbAPR.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILAPR07", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILAPR07")
            End If
        End If

        If chbQTA.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILQTA08") Then
            strPrivSQL = getSQLprivupdate("EMAILQTA08", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbQTA.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILQTA08", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILQTA08")
            End If
        End If

        If chbQTR.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILQTR09") Then
            strPrivSQL = getSQLprivupdate("EMAILQTR09", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbQTR.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILQTR09", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILQTR09")
            End If
        End If

        If chbRFA.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILRFA10") Then
            strPrivSQL = getSQLprivupdate("EMAILRFA10", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbRFA.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILRFA10", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILRFA10")
            End If
        End If

        If chbRFR.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILRFR11") Then
            strPrivSQL = getSQLprivupdate("EMAILRFR11", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbRFR.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILRFR11", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILRFR11")
            End If
        End If

        If chbRFC.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILRFC12") Then
            strPrivSQL = getSQLprivupdate("EMAILRFC12", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbRFC.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILRFC12", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILRFC12")
            End If
        End If

        If chbRCF.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILRCF13") Then
            strPrivSQL = getSQLprivupdate("EMAILRCF13", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbRCF.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILRCF13", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILRCF13")
            End If
        End If

        If chbRCP.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILRCP14") Then
            strPrivSQL = getSQLprivupdate("EMAILRCP14", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbRCP.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILRCP14", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILRCP14")
            End If
        End If

        If chbCNC.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILCNC15") Then
            strPrivSQL = getSQLprivupdate("EMAILCNC15", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbCNC.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILCNC15", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILCNC15")
            End If
        End If

        If chbDLF.Checked = True Then
            strOpValue = "Y"
        Else
            strOpValue = "N"
        End If
        If hashPrivs.ContainsKey("EMAILDLF16") Then
            strPrivSQL = getSQLprivupdate("EMAILDLF16", "IOL", strOpValue, "UPD")
            execPrivsSQL(strPrivSQL)
        Else
            If chbDLF.Checked = True Then
                strPrivSQL = getSQLprivupdate("EMAILDLF16", "IOL", strOpValue, "INS")
                execPrivsSQL(strPrivSQL, "INS", strOpValue, "EMAILDLF16")
            End If
        End If

        lblUpdMsg.Text = "Database has been updated..."

    End Sub

#End Region

#Region "Privileges"
    Private Sub LoadUserprivilegesData(ByVal displayedBU As String, ByVal sDisplayedSDICust As String, ByVal sDisplayedUserType As String)
        Try
            Dim strBU As String
            ViewState("BU") = ""
            If IsVendor() Or IsMexicoVendor() Or sDisplayedSDICust = "V" Then
                rcbGroupTab2.Visible = False
                txtVendorUserGroup.Visible = True
                txtVendorUserGroup.Text = displayedBU
                ViewState("BU") = drpBUnit.SelectedValue
            Else
                rcbGroupTab2.Visible = True
                txtVendorUserGroup.Visible = False
                ViewState("BU") = displayedBU
                buildGroupList(rcbGroupTab2)
                Try
                    rcbGroupTab2.Items.FindItemByValue(displayedBU).Selected = True
                Catch ex As Exception

                End Try

            End If
            lblUserVal.Text = txtUserid.Text
            ' If the user can change to the privileges tab, it's not a "USER"; it's a "SUPER" or "ADMIN"
            ' who is editing privileges for themselves or for a USER. In that case, load the tree
            ' for the user who is editing the privileges.
            Dim sDisplayedUserID As String = GetDisplayedUserID()
            Dim sDisplayedSDIEmp As String = ConvertSDICustToSDIEmp(sDisplayedSDICust)
            Dim sPortal As String = GetPortal(sDisplayedSDICust)
            LoadProgramData(sPortal, Session("USERTYPE").ToString, sDisplayedUserID, sDisplayedUserType, sDisplayedSDIEmp)
            If sPortal = "Vendor" Then
                LoadRoleMaster(sPortal, drpBUnit.SelectedValue, "V")
            Else
                LoadRoleMaster(sPortal, displayedBU, sDisplayedSDICust)
            End If
            If Session("Role") = "CORPADMIN" Then
                LoadRoleMaster("Customer", displayedBU, "C")
            End If
            GetUserAccessType()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub LoadProgramData(ByVal sPortal As String, ByVal sEditorsUserType As String, ByVal sEditeesUserID As String, ByVal sEditeesUserType As String, ByVal sEditeesSDIEmp As String)
        Dim dsProgramData As DataSet
        Try
            Dim eAccessGroup As clsProgramMaster.AccessGroupEnum = clsProgramMaster.AccessGroupEnum.MostRestricted
            If sEditorsUserType.ToLower.Equals("admin") Or sEditorsUserType.ToLower.Equals("corpadmin") Then
                eAccessGroup = clsProgramMaster.AccessGroupEnum.Admin
            ElseIf sEditorsUserType.ToLower.Equals("super") Then
                eAccessGroup = clsProgramMaster.AccessGroupEnum.Super
            End If
            dsProgramData = clsProgramMaster.GetPrograms(sPortal, sEditeesUserID, sEditeesUserType, sEditeesSDIEmp, eAccessGroup)

            If dsProgramData IsNot Nothing Then
                If dsProgramData.Tables(0).Rows.Count > 0 Then
                    rtvPrograms.DataTextField = "PROGRAMNAME"
                    rtvPrograms.DataValueField = "SECURITYALIAS"
                    rtvPrograms.DataFieldID = "ISA_IDENTIFIER"
                    rtvPrograms.DataFieldParentID = "ISA_PARENT_IDENT"

                    For Each row As DataRow In dsProgramData.Tables(0).Rows
                        row.Item("programname") = row.Item("programname").ToString & " (" & row.Item("securityalias") & ")"
                        If row.Item("active").ToString() = clsProgramMaster.InactiveProgramCode.ToString Then
                            row.Item("programname") = row.Item("programname").ToString & " - inactive"
                        ElseIf row.Item("securityalias").ToString.Trim.Length = 0 Then
                            row.Item("programname") = row.Item("programname").ToString & " - program not available"
                        End If
                    Next
                    dsProgramData.AcceptChanges()

                    rtvPrograms.DataSource = dsProgramData
                    rtvPrograms.DataBind()
                    HideExpandCollapseButtons()
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub LoadRoleMaster(ByVal roleType As String, ByVal str_BU As String, ByVal str_Usrrole As String)
        Dim dsRoleData As DataSet
        Try
            Dim strSQLstring As String
            Dim userType As String = Convert.ToString(Session("USERTYPE"))

            If str_Usrrole = "S" Then
                If userType.ToLower.Equals("admin") Then
                    strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT From SDIX_USRROLE_TBL WHERE ROLETYPE = 'SDI'" & vbCrLf & _
                        "AND ROLENUM NOT IN (SELECT ROLENUM FROM SDIX_ROLEDETAIL WHERE ALIAS_NAME IN (" & vbCrLf & _
                        "SELECT SECURITYALIAS FROM SDIX_PRGRMMASTER WHERE ACCESS_GROUP = 'SUPER'))"
                Else
                    strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT From SDIX_USRROLE_TBL WHERE ROLETYPE = 'SDI'"
                End If
            Else
                If userType.ToLower.Equals("admin") Then
                    strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT From SDIX_USRROLE_TBL WHERE ROLETYPE = '" + roleType + "'" & vbCrLf & _
                        "AND ROLENUM NOT IN (SELECT ROLENUM FROM SDIX_ROLEDETAIL WHERE ALIAS_NAME IN (" & vbCrLf & _
                        "SELECT SECURITYALIAS FROM SDIX_PRGRMMASTER WHERE ACCESS_GROUP = 'SUPER')) AND BUSINESS_UNIT = '" & str_BU & "'"
                Else
                    strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT From SDIX_USRROLE_TBL WHERE ROLETYPE = '" + roleType + "' AND BUSINESS_UNIT = '" & str_BU & "'"
                End If
            End If

            dsRoleData = ORDBData.GetAdapter(strSQLstring)
            ddlUserRole.DataSource = dsRoleData
            ddlUserRole.DataTextField = "ROLENAME"
            ddlUserRole.DataValueField = "ROLENUM"
            ddlUserRole.DataBind()
            'Added for disabling rolename dropdown if no value
            If ddlUserRole.Items.Count = 0 Then
                ddlUserRole.Enabled = False
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Sub GetRoleData(ByVal id As String)
        Dim dsRolePrograms As DataSet
        Try
            rtvPrograms.ClearCheckedNodes()
            Dim strQuery As String = "select ALIAS_NAME from SDIX_ROLEDETAIL where ROLENUM = " + id
            dsRolePrograms = ORDBData.GetAdapter(strQuery)
            Dim roleRow As DataRow
            Dim navTreePrograms As IList(Of RadTreeNode) = rtvPrograms.GetAllNodes()
            ' loop through nav tree
            For Each navTreeNode As RadTreeNode In navTreePrograms
                ' loop thru role programs to find a match for the current nav tree program
                For Each roleRow In dsRolePrograms.Tables(0).Rows
                    Dim roleProgramAlias As String = roleRow("ALIAS_NAME")
                    If navTreeNode.Value = roleProgramAlias Then
                        navTreeNode.Checked = True
                        Exit For
                    End If
                Next
            Next

        Catch ex As Exception

        End Try
    End Sub


    Protected Sub rblType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles rblType.SelectedIndexChanged
        Try
            rcbGroupTab2.Visible = True
            If rblType.SelectedIndex = 1 Then  '  If rblType.SelectedItem.Value.ToLower().Equals("role") Then
                'Dim sPortal As String = GetPortal()
                'LoadRoleMaster(sPortal)
                ddlUserRole.Enabled = True
                GetRoleData(ddlUserRole.SelectedItem.Value)
            Else
                ddlUserRole.Enabled = False
                GetAlaCarteData()
            End If
            If radioUserType.SelectedValue = "V" Then
                rcbGroupTab2.Visible = False
            Else

            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub ddlUserRole_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        Try
            rcbGroupTab2.Visible = True
            GetRoleData(ddlUserRole.SelectedValue)
            'Added for hiding privilege & program label for corp admin
            If Session("ROLE") = "CORPADMIN" Then
                lblPrivilegeType.Visible = False
                rblType.Visible = False
                lblProgram.Visible = False
                rtvPrograms.Visible = False
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnUserAccessSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUserAccessSave.Click

        lblMessage1N.Text = ""
        Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.CheckedNodes
        Try
            If rblType.SelectedIndex = 1 Then  '  If rblType.SelectedItem.Value.ToLower().Equals("role") Then

                SaveUserAccessRolePrivileges(txtUserid.Text, ViewState("BU"), ddlUserRole.SelectedItem.Value)

                Label1.Text = "User information has been modified and saved successfully."
                lblMessage1N.Text = Label1.Text
                Session("MenuUpdated") = True
            Else
                If nodeCollection.Count > 0 Then
                    SaveUserAccessPrivileges(txtUserid.Text, ViewState("BU"), nodeCollection)
                    Label1.Text = "User information has been modified and saved successfully."
                    lblMessage1N.Text = Label1.Text
                    Session("MenuUpdated") = True
                Else
                    'Adding error msg to select any one of the privilege under the program after deselecting all 
                    Label1.Text = "Please Select Atleast One Program."
                    lblMessage1N.Text = Label1.Text
                End If
            End If

            rcbGroup.Items.FindItemByValue(rcbGroup.SelectedItem.Value).Selected = True

            If txtVendorUserGroup.Visible = True Then
                rcbGroupTab2.Visible = False
            Else
                rcbGroupTab2.Visible = True
            End If
            'Added for hiding privilge type & program tree for corp admin
            If Session("ROLE") = "CORPADMIN" Then
                lblPrivilegeType.Visible = False
                rblType.Visible = False
                lblProgram.Visible = False
                rtvPrograms.Visible = False
            End If


        Catch ex As Exception
            lblMessage1N.Text = ""
        End Try
    End Sub

    Private Sub GetUserAccessType()
        Try
            'rblType.Items.FindByValue("Alacarte").Selected = True
            rblType.SelectedIndex = 0
            Dim userRoleID As Integer = GetUserAccessRole(txtUserid.Text, ViewState("BU"))
            If userRoleID <= 0 Then
                GetAlaCarteData()
                ddlUserRole.Enabled = False
            ElseIf userRoleID <> 0 And ddlUserRole.Enabled = False Then
                GetRoleData(userRoleID)
                ddlUserRole.Enabled = True
                'rblType.Items.FindByValue("Role").Selected = True
                rblType.SelectedIndex = 1
                Try
                    ddlUserRole.Items.FindByValue(userRoleID).Selected = True
                Catch ex As Exception
                    Dim strRoleNames As String = ""
                    strRoleNames = Get_RoleValues(userRoleID)
                    ddlUserRole.Items.FindByText(strRoleNames.ToUpper()).Selected = True
                End Try
            Else
                GetRoleData(userRoleID)
                rblType.SelectedIndex = 1
                ''ddlUserRole.Items.FindByValue(userRoleID).Selected = True
                Try
                    ddlUserRole.Items.FindByValue(userRoleID).Selected = True
                Catch ex As Exception
                    Dim strRoleNames As String = ""
                    strRoleNames = Get_RoleValues(userRoleID)
                    ddlUserRole.Items.FindByText(strRoleNames.ToUpper()).Selected = True
                End Try
            End If
        Catch ex As Exception

        End Try
    End Sub
#End Region

#Region "Preferences"
    Private Sub LoadPreferences(Optional ByVal strBU As String = "")
        Try
            pf1.Attributes.Add("style", "display:block")
            pf2.Attributes.Add("style", "display:block")
            pf3.Attributes.Add("style", "display:block")
            pf4.Attributes.Add("style", "display:block")
            pf5.Attributes.Add("style", "display:block")
            pf6.Attributes.Add("style", "display:block")
            GetProdDispType(strBU)

            Response.Cache.SetExpires(DateTime.Now.AddSeconds(3))
            lblBU.Text = strBU

            ' this program should NOT return the "exempt flag" when
            '   called from labelsbyChgCd.aspx page.
            '   - erwin 2009.02.10
            Dim bIsIncludeExemptFlag As Boolean = True      ' since I don't know what program(s) needed this flag, let's default to "return flag"
            Try
                If Not (Request.QueryString(PARAM_RETURN_EXEMPT_FLAG) Is Nothing) Then
                    Dim valTrue As String = "Y~1~TRUE"
                    Dim valFalse As String = "N~0~FALSE"
                    If valTrue.IndexOf(Request.QueryString(PARAM_RETURN_EXEMPT_FLAG).Trim.ToUpper) > -1 Then
                        bIsIncludeExemptFlag = True
                    ElseIf valFalse.IndexOf(Request.QueryString(PARAM_RETURN_EXEMPT_FLAG).Trim.ToUpper) > -1 Then
                        bIsIncludeExemptFlag = False
                    End If
                End If
            Catch ex As Exception
                'ignore
                '   this will return the default value
            End Try

            Dim strScript As String = ""
            If Not Page.IsPostBack Then

                WebLog()
            End If
            getccdata(strBU)
            If Session("USERTYPE") = "SUPER" Then
                'lblBlockPrice.Visible = True
                chkbxDisplayPrice.Enabled = True
            Else
                'lblBlockPrice.Visible = False 
                chkbxDisplayPrice.Enabled = False
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub getccdata(Optional ByVal strBU As String = "")

        Dim arrChrCdRules As ArrayList
        Dim I As Integer
        Dim Count As Integer = 0
        Dim litlabel As LiteralControl
        Dim dropChrCD As DropDownList
        Dim txtChrCD As TextBox

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
        Try
            ' Command, Data Reader  

            Dim dtrChrCDReader As OleDbDataReader = ORDBData.GetReader(strSQL)
            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
            Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
            If m_weblogstring = "true" Then
                'WebLogOpenConn()
            End If
            Dim bolFocusSet As Boolean = False

            If dtrChrCDReader.HasRows() = True Then
                dtrChrCDReader.Read()
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_1"))
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_2"))
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_3"))
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_4"))
                arrChrCdRules.Add(dtrChrCDReader.Item("ISA_CHG_CD_SEG_5"))

                'If dtrChrCDReader.Item("ISA_CHG_CD_SEP_FLG") = "Y" Then
                '    txtChgCdSep.Text = dtrChrCDReader.Item("ISA_CHG_CD_SEP")
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

                ''  txtChgCdSep.Text = strChgCdSep1

                dtrChrCDReader.Close()
                'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                If m_weblogstring = "true" Then
                    'WebLogCloseConn()
                End If

                'Code for new AEES processing
                Dim arrIEESNames(2) As String

                Dim bIsChangeNameForIEES As Boolean = False
                Try
                    If IsAEES(strBU) Then
                        Dim sNames As String = ""
                        sNames = UCase(ConfigurationSettings.AppSettings("IeesSegmNames").ToString)
                        If Trim(sNames) <> "" Then
                            arrIEESNames = Split(sNames, "~")
                            bIsChangeNameForIEES = True
                        End If
                    End If
                Catch ex As Exception
                    bIsChangeNameForIEES = False
                End Try

                Do While I < 5
                    Select Case arrChrCdRules(I)
                        Case "E"
                            'litlabel = New LiteralControl
                            'dropChrCD = New DropDownList
                            Count = Count + 1
                            cc1.Attributes.Add("style", "display:block")
                            dropCD1Seg.Visible = True
                            lblChrCD.Visible = True
                            Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                            If Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                lbl1.Text = "<p class='displaylabel'>" & objChgCdDesc.ChgCdDesc & ""
                            Else
                                lbl1.Text = "<p class='displaylabel'>Employee Seqment" & vbCrLf
                            End If

                            'PlaceHolder1.Controls.Add(litlabel)
                            'strSQL = "SELECT ISA_CHGCD_EMPL_SEG, ISA_CHGCD_EMPL_SEG as SEQ" & vbCrLf & _
                            '    " FROM PS_ISA_CHGCD_EMPL" & vbCrLf & _
                            '    " where  BUSINESS_UNIT = '" & Request.QueryString("BU") & "'" & vbCrLf
                            strSQL = "" & vbCrLf &
                                    " SELECT " & vbCrLf &
                                    "  (ISA_CUST_CHARGE_CD || ' ' || DESCR60) AS ISA_CUST_CHARGE_CD" & vbCrLf &
                                    " ,ISA_CUST_CHARGE_CD AS SEQ" & vbCrLf &
                                    " FROM SYSADM8.PS_ISA_EMPL_CHR_CD" & vbCrLf &
                                    " where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                                    " AND ISA_EMPLOYEE_ID = '" & Request.QueryString("USER") & "'" & vbCrLf &
                                    " AND ISA_CUST_CHARGE_CD <> ' '" & vbCrLf &
                                    " ORDER BY ISA_CUST_CHARGE_CD " & vbCrLf &
                                    ""

                            Dim dtrChrCDEmpSeq As OleDbDataReader = ORDBData.GetReader(strSQL)
                            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                            'Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
                            If m_weblogstring = "true" Then
                                'WebLogOpenConn()
                            End If

                            dropCD1Seg.DataSource = dtrChrCDEmpSeq
                            dropCD1Seg.DataValueField = "SEQ"
                            'dropChrCD.DataTextField = "ISA_CHGCD_EMPL_SEG"

                            dropCD1Seg.DataTextField = "ISA_CUST_CHARGE_CD"
                            dropCD1Seg.DataBind()
                            dtrChrCDEmpSeq.Close()
                            'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                            If m_weblogstring = "true" Then
                                'WebLogCloseConn()
                            End If
                            dropCD1Seg.Items.Insert(0, New ListItem(" "))
                            'dropChrCD.ID = "dropEmpSeg"

                            Response.Write("<TD>" & vbCrLf)
                            'PlaceHolder1.Controls.Add(dropChrCD)

                        Case "I"
                            Count = Count + 1
                            If Request.QueryString("Line") = "Y" Then
                                'litlabel = New LiteralControl
                                'dropChrCD = New DropDownList
                                dropCD2Seg.Visible = True
                                lblChrCD.Visible = True
                                cc2.Attributes.Add("style", "display:block")
                                Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                                If Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                    lbl2.Text = "<p class='displaylabel'>" & objChgCdDesc.ChgCdDesc & "&nbsp;&nbsp;&nbsp;</b>"
                                Else
                                    lbl2.Text = "<p class='displaylabel'>Item Seqment"
                                End If

                                'PlaceHolder1.Controls.Add(litlabel)
                                strSQL = "" & vbCrLf &
                                    "SELECT " & vbCrLf &
                                    " (ISA_CHGCD_ITEM_SEG || ' ' || DESCR60) AS ISA_CHGCD_ITEM_SEG" & vbCrLf &
                                    ",ISA_CHGCD_ITEM_SEG AS SEQ " & vbCrLf &
                                    " FROM SYSADM8.PS_ISA_CHGCD_ITEM" & vbCrLf &
                                    " where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                                    " ORDER BY ISA_CHGCD_ITEM_SEG" & vbCrLf &
                                    ""
                                Dim dtrChrCDItmSeq As OleDbDataReader = ORDBData.GetReader(strSQL)
                                ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                                'Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
                                If m_weblogstring = "true" Then
                                    'WebLogOpenConn()
                                End If

                                dropCD2Seg.DataSource = dtrChrCDItmSeq
                                dropCD2Seg.DataValueField = "SEQ"
                                dropCD2Seg.DataTextField = "ISA_CHGCD_ITEM_SEG"
                                dropCD2Seg.DataBind()
                                dtrChrCDItmSeq.Close()
                                'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                                If m_weblogstring = "true" Then
                                    'WebLogCloseConn()
                                End If

                                dropCD2Seg.Items.Insert(0, New ListItem(" "))
                                'dropChrCD.ID = "dropItmSeg"

                                'PlaceHolder1.Controls.Add(dropChrCD)

                            End If
                            ' the below code is being commented out
                            ' because at Stanley there is a default item
                            ' charge code and it may be different
                            ' for each item in the cart.  The default
                            ' chrage code will be appended to the front
                            ' of the user entered charge at the order level


                        Case "M"
                            'litlabel = New LiteralControl
                            'dropChrCD = New DropDownList
                            Count = Count + 1
                            dropCD3Seg.Visible = True
                            lblChrCD.Visible = True
                            cc3.Attributes.Add("style", "display:block")
                            Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                            If Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                lbl3.Text = "<p class='displaylabel'>" & objChgCdDesc.ChgCdDesc
                            Else
                                lbl3.Text = "<p class='displaylabel'>Machine Seqment"
                            End If

                            'PlaceHolder1.Controls.Add(litlabel)
                            strSQL = "" &
                                "SELECT " & vbCrLf &
                                " (ISA_MACHINE_NO || ISA_CUST_CHARGE_CD || ' ' || DESCR60) AS ISA_CUST_CHARGE_CD" & vbCrLf &
                                ",ISA_CUST_CHARGE_CD as SEQ" & vbCrLf &
                                " FROM SYSADM8.PS_ISA_MCHNE_CHGCD" & vbCrLf &
                                " where  BUSINESS_UNIT = '" & strBU & "' " & vbCrLf &
                                " ORDER BY (ISA_MACHINE_NO || ISA_CUST_CHARGE_CD)" & vbCrLf &
                                ""
                            Dim dtrChrCDMchSeq As OleDbDataReader = ORDBData.GetReader(strSQL)
                            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                            'Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
                            If m_weblogstring = "true" Then
                                'WebLogOpenConn()
                            End If

                            dropCD3Seg.DataSource = dtrChrCDMchSeq
                            dropCD3Seg.DataValueField = "SEQ"
                            dropCD3Seg.DataTextField = "ISA_CUST_CHARGE_CD"
                            dropCD3Seg.DataBind()
                            dtrChrCDMchSeq.Close()
                            dropCD3Seg.Items.Insert(0, New ListItem(" "))
                            'dropChrCD.ID = "dropMchSeg"

                            'PlaceHolder1.Controls.Add(dropChrCD)

                        Case "P"
                            '  litlabel = New LiteralControl
                            'dropChrCD = New DropDownList
                            Count = Count + 1
                            dropCD4Seg.Visible = True
                            lblChrCD.Visible = True
                            cc4.Attributes.Add("style", "display:block")
                            Dim CD1SegValue As String = dropCD4Seg.SelectedValue
                            Dim CD2SegValue As String = dropCD5Seg.SelectedValue
                            Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                            If bIsChangeNameForIEES Then
                                lbl4.Text = "<p class='displaylabel'>User Segment 1"
                                If Not arrIEESNames(0) Is Nothing Then
                                    If Trim(arrIEESNames(0)) <> "" Then
                                        lbl4.Text = "<p class='displaylabel'>" & arrIEESNames(0)
                                    End If
                                End If
                            Else
                                If Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                    lbl4.Text = "<p class='displaylabel'>" & objChgCdDesc.ChgCdDesc
                                Else
                                    lbl4.Text = "<p class='displaylabel'>User Segment 1"
                                End If
                            End If
                            strSQL = "SELECT  (ISA_CHGCD_CHILD1 || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR1_SEG" & vbCrLf &
                                     ",(ISA_CHGCD_CHILD1) AS SEQ  FROM SYSADM8.PS_ISA_CHGCD_CHLD1  where BUSINESS_UNIT = '" & strBU & "' " & vbCrLf &
                                     " order by (ISA_CHGCD_USR1_SEG || ' ' || TAX_GROUP) ASC "

                            Dim dtrChrCD1Seq As OleDbDataReader = ORDBData.GetReader(strSQL)
                            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                            'Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
                            If m_weblogstring = "true" Then
                                'WebLogOpenConn()
                            End If

                            dropCD4Seg.DataSource = dtrChrCD1Seq
                            dropCD4Seg.DataTextField = "ISA_CHGCD_USR1_SEG"
                            dropCD4Seg.DataValueField = "SEQ"

                            dropCD4Seg.DataBind()
                            dropCD4Seg.Items.Insert(0, New ListItem(""))


                            If CD1SegValue.Trim = "" Then
                                Dim strDefChgCd1 As String = ""
                                strDefChgCd1 = getDefaultChrCd("1")

                                If Not strDefChgCd1 = "" Then
                                    If Not dropCD4Seg.Items.FindByValue(strDefChgCd1) Is Nothing Then
                                        dropCD4Seg.SelectedValue = strDefChgCd1
                                        dropCD5Seg.Visible = True
                                        dropCD4Seg_SelectedIndexChanged(Nothing, EventArgs.Empty)
                                        dropCD4Seg.Enabled = True
                                    End If

                                End If
                            Else
                                dropCD4Seg.SelectedValue = CD1SegValue
                            End If
                            'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                            If m_weblogstring = "true" Then
                                'WebLogCloseConn()
                            End If
                            ' PlaceHolder1.Controls.Add(dropChrCD)

                        Case "F"
                            Dim objEnterprise As New clsEnterprise(Trim(strBU))
                            Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                            'litlabel = New LiteralControl
                            'dropChrCD = New DropDownList
                            Count = Count + 1
                            dropCD5Seg.Visible = True
                            lblChrCD.Visible = True
                            cc5.Attributes.Add("style", "display:block")
                            If bIsChangeNameForIEES Then
                                lbl5.Text = "<p class='displaylabel'>User Segment 2"
                                If Not arrIEESNames(1) Is Nothing Then
                                    If Trim(arrIEESNames(1)) <> "" Then
                                        lbl5.Text = "<p class='displaylabel'>" & arrIEESNames(1)
                                    End If
                                End If
                            Else
                                If objEnterprise.CompanyID = "STANLEY" Then
                                    lbl5.Text = "<p class='displaylabel'>Category"
                                ElseIf Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                    lbl5.Text = "<p class='displaylabel'>" & objChgCdDesc.ChgCdDesc
                                Else
                                    lbl5.Text = "<p class='displaylabel'>User Segment 2"
                                End If
                            End If

                            If Not dropCD4Seg.Visible Then
                                Dim CD5SegValue As String = dropCD5Seg.SelectedValue
                                Dim Strcrgcde As String
                                Strcrgcde = "SELECT  (ISA_CHGCD_CHILD2 || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR2_SEG" & vbCrLf &
                                         ",(ISA_CHGCD_CHILD2) AS SEQ  FROM SYSADM8.PS_ISA_CHGCD_CHLD2  where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                                         " order by (ISA_CHGCD_USR2_SEG || ' ' || TAX_GROUP) asc "
                                Dim dtrChrCD5Seq As OleDbDataReader = ORDBData.GetReader(Strcrgcde)
                                dropCD5Seg.DataSource = dtrChrCD5Seq
                                dropCD5Seg.DataTextField = "ISA_CHGCD_USR2_SEG"
                                dropCD5Seg.DataValueField = "SEQ"
                                dropCD5Seg.DataBind()
                                dropCD5Seg.Items.Insert(0, New ListItem(""))
                                dropCD5Seg.SelectedValue = CD5SegValue

                            End If
                        Case "S"
                            'litlabel = New LiteralControl
                            'dropChrCD = New DropDownList
                            Count = Count + 1
                            dropCD6Seg.Visible = False
                            lblChrCD.Visible = True
                            lbl6.Visible = False
                            cc6.Attributes.Add("style", "display:block")
                            Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                            If bIsChangeNameForIEES Then
                                lbl6.Text = "<br /><br /><p class='displaylabel'><b>&nbsp;&nbsp;&nbsp;User Segment 3&nbsp;&nbsp;&nbsp;</b>"
                                If Not arrIEESNames(2) Is Nothing Then
                                    If Trim(arrIEESNames(2)) <> "" Then
                                        lbl6.Text = "<br /><br /><p class='displaylabel'><b>&nbsp;&nbsp;&nbsp;" & arrIEESNames(2) & "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</b>"
                                    End If
                                End If
                            Else
                                If Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                    lbl6.Text = "<br /><br /><p class='displaylabel'><b>&nbsp;&nbsp;&nbsp;" & objChgCdDesc.ChgCdDesc & "&nbsp;&nbsp;&nbsp;</b>"
                                Else
                                    lbl6.Text = "<br /><br /><p class='displaylabel'><b>&nbsp;&nbsp;&nbsp;User Segment 3&nbsp;&nbsp;&nbsp;</b>"
                                End If
                            End If

                            If Not dropCD4Seg.Visible And Not dropCD2Seg.Visible Then
                                Dim Strcrgcde As String = "SELECT  (ISA_CHGCD_CHILD3 || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR2_SEG" & vbCrLf &
                               ",(ISA_CHGCD_CHILD3) AS SEQ  FROM SYSADM8.ps_isa_chgcd_CHLD3  where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                               "order by (ISA_CHGCD_USR2_SEG || ' ' || TAX_GROUP) asc "
                                Dim dtrChrCD6Seq As OleDbDataReader = ORDBData.GetReader(Strcrgcde)
                                dropCD6Seg.DataSource = dtrChrCD6Seq
                                dropCD6Seg.DataTextField = "ISA_CHGCD_USR2_SEG"
                                dropCD6Seg.DataValueField = "SEQ"
                                dropCD6Seg.DataBind()
                                dropCD6Seg.Items.Insert(0, New ListItem(" "))
                            End If

                        Case "1"
                            dropCD7Seg.Visible = True
                            lblChrCD.Visible = True
                            cc7.Attributes.Add("style", "display:block")
                            Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                            If bIsChangeNameForIEES Then
                                lbl7.Text = "<p class='displaylabel'>User Segment 1"
                                If Not arrIEESNames(0) Is Nothing Then
                                    If Trim(arrIEESNames(0)) <> "" Then
                                        lbl7.Text = "<p class='displaylabel'>" & arrIEESNames(0)
                                    End If
                                End If
                            Else
                                If Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                    lbl7.Text = "<p class='displaylabel'>" & objChgCdDesc.ChgCdDesc
                                Else
                                    lbl7.Text = "<p class='displaylabel'>User Segment 1"
                                End If
                            End If
                            'strSQL = "SELECT ISA_CHGCD_USR1_SEG, ISA_CHGCD_USR1_SEG as SEQ" & vbCrLf & _
                            '                               " FROM PS_ISA_CHGCD_USR1" & vbCrLf & _
                            '                               " where  BUSINESS_UNIT = '" & Request.QueryString("BU") & "'" & vbCrLf

                            'strSQL = "SELECT ISA_CHGCD_USR1_SEG, (ISA_CHGCD_USR1_SEG || ' ' || tax_group) as SEQ" & vbCrLf & _
                            strSQL = "" & vbCrLf &
                                "SELECT " & vbCrLf &
                                " (ISA_CHGCD_USR1_SEG || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR1_SEG" & vbCrLf &
                                ",(ISA_CHGCD_USR1_SEG) AS SEQ" & vbCrLf &
                                " FROM SYSADM8.PS_ISA_CHGCD_USR1" & vbCrLf &
                                " where BUSINESS_UNIT = '" & strBU & "' " & vbCrLf &
                                " order by (ISA_CHGCD_USR1_SEG || ' ' || TAX_GROUP) ASC" & vbCrLf &
                                ""

                            Dim dtrChrCD7Seq As OleDbDataReader = ORDBData.GetReader(strSQL)
                            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                            'Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim


                            If m_weblogstring = "true" Then
                                'WebLogOpenConn()
                            End If
                            'TAX_GROUP = 'EXEMP_CERT' and 

                            dropCD7Seg.DataSource = dtrChrCD7Seq
                            'pfd 08152008
                            'dropChrCD.DataValueField = "SEQ"
                            'dropChrCD.DataTextField = "ISA_CHGCD_USR1_SEG"
                            dropCD7Seg.DataTextField = "ISA_CHGCD_USR1_SEG"
                            dropCD7Seg.DataValueField = "SEQ"
                            dropCD7Seg.DataBind()
                            dtrChrCD7Seq.Close()

                            dropCD7Seg.Items.Insert(0, New ListItem(""))
                            dropCD7Seg.ID = "dropCD7Seg"

                            Dim strDefChgCd1 As String = ""
                            strDefChgCd1 = getDefaultChrCd("1")

                            If Not strDefChgCd1 = "" Then
                                If Not dropCD7Seg.Items.FindByValue(strDefChgCd1) Is Nothing Then
                                    dropCD7Seg.SelectedValue = strDefChgCd1
                                    dropCD7Seg.Enabled = True
                                End If
                            End If

                            'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                            If m_weblogstring = "true" Then
                                'WebLogCloseConn()
                            End If


                        Case "2"
                            dropCD8Seg.Visible = True
                            lblChrCD.Visible = True
                            cc8.Attributes.Add("style", "display:block")
                            Dim objEnterprise As New clsEnterprise(Trim(strBU))
                            Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                            If bIsChangeNameForIEES Then
                                lbl8.Text = "<p class='displaylabel'>User Segment 2"
                                If Not arrIEESNames(1) Is Nothing Then
                                    If Trim(arrIEESNames(1)) <> "" Then
                                        lbl8.Text = "<p class='displaylabel'>" & arrIEESNames(1)
                                    End If
                                End If
                            Else
                                If objEnterprise.CompanyID = "STANLEY" Then
                                    lbl8.Text = "<p class='displaylabel'>Category"
                                ElseIf Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                    lbl8.Text = "<p class='displaylabel'>" & objChgCdDesc.ChgCdDesc
                                Else
                                    lbl8.Text = "<p class='displaylabel'>User Segment 2"
                                End If
                            End If

                            'strSQL = "SELECT ISA_CHGCD_USR2_SEG, ISA_CHGCD_USR2_SEG as SEQ" & vbCrLf & _
                            '  " FROM PS_ISA_CHGCD_USR2" & vbCrLf & _
                            '  " where  BUSINESS_UNIT = '" & Request.QueryString("BU") & "'" & vbCrLf
                            'pfd 08152008

                            'strSQL = "SELECT ISA_CHGCD_USR2_SEG, (ISA_CHGCD_USR2_SEG || ' ' || tax_group) as SEQ" & vbCrLf & _
                            strSQL = "" & vbCrLf &
                                "SELECT " & vbCrLf &
                                " (ISA_CHGCD_USR2_SEG || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR2_SEG" & vbCrLf &
                                ",(ISA_CHGCD_USR2_SEG) AS SEQ" & vbCrLf &
                                " FROM SYSADM8.PS_ISA_CHGCD_USR2" & vbCrLf &
                                " where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                                " order by (ISA_CHGCD_USR2_SEG || ' ' || TAX_GROUP) asc " & vbCrLf &
                                ""
                            Dim dtrChrCD8Seq As OleDbDataReader = ORDBData.GetReader(strSQL)
                            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                            'Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
                            If m_weblogstring = "true" Then
                                'WebLogOpenConn()
                            End If
                            ' TAX_GROUP = 'EXEMP_CERT' and

                            dropCD8Seg.DataSource = dtrChrCD8Seq
                            'dropChrCD.DataValueField = "SEQ"
                            'dropChrCD.DataTextField = "ISA_CHGCD_USR2_SEG"
                            'pfd 08152008
                            dropCD8Seg.DataTextField = "ISA_CHGCD_USR2_SEG"
                            dropCD8Seg.DataValueField = "SEQ"
                            dropCD8Seg.DataBind()
                            dtrChrCD8Seq.Close()
                            'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                            If m_weblogstring = "true" Then
                                'WebLogCloseConn()
                            End If
                            dropCD8Seg.Items.Insert(0, New ListItem(""))
                            dropCD8Seg.ID = "dropCD8Seg"

                            Dim strDefChgCd1 As String = ""
                            strDefChgCd1 = getDefaultChrCd("2")

                            If Not strDefChgCd1 = "" Then
                                If Not dropCD8Seg.Items.FindByValue(strDefChgCd1) Is Nothing Then
                                    dropCD8Seg.SelectedValue = strDefChgCd1
                                    dropCD8Seg.Enabled = True
                                End If
                            End If

                        Case "3"
                            dropCD9Seg.Visible = False
                            lblChrCD.Visible = True
                            lbl9.Visible = False
                            cc9.Attributes.Add("style", "display:block")
                            Dim objChgCdDesc As New clsChgCDDescr(Trim(strBU), arrChrCdRules(I))
                            If bIsChangeNameForIEES Then
                                lbl9.Text = "<p class='displaylabel'>User Segment 3"
                                If Not arrIEESNames(2) Is Nothing Then
                                    If Trim(arrIEESNames(2)) <> "" Then
                                        lbl9.Text = "<p class='displaylabel'>" & arrIEESNames(2)
                                    End If
                                End If
                            Else
                                If Not Trim(objChgCdDesc.ChgCdDesc) = "" Then
                                    lbl9.Text = "<p class='displaylabel'>" & objChgCdDesc.ChgCdDesc
                                Else
                                    lbl9.Text = "<p class='displaylabel'>User Segment 3"
                                End If
                            End If
                            strSQL = "" & vbCrLf &
                                "SELECT " & vbCrLf &
                                " (ISA_CHGCD_USR3_SEG || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR3_SEG" & vbCrLf &
                                ",(ISA_CHGCD_USR3_SEG) AS SEQ" & vbCrLf &
                                " FROM SYSADM8.PS_ISA_CHGCD_USR3" & vbCrLf &
                                " where  BUSINESS_UNIT = '" & strBU & "'" & vbCrLf &
                                " order by (ISA_CHGCD_USR3_SEG || ' ' || TAX_GROUP) asc " & vbCrLf &
                                ""
                            'pfd 08152008
                            'strSQL = "SELECT ISA_CHGCD_USR3_SEG, ISA_CHGCD_USR3_SEG as SEQ" & vbCrLf & _
                            '  " FROM PS_ISA_CHGCD_USR3" & vbCrLf & _
                            '  " where  BUSINESS_UNIT = '" & Request.QueryString("BU") & "'" & vbCrLf

                            Dim dtrChrCD9Seq As OleDbDataReader = ORDBData.GetReader(strSQL)
                            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
                            'Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
                            If m_weblogstring = "true" Then
                                'WebLogOpenConn()
                            End If
                            'TAX_GROUP = 'EXEMP_CERT' and
                            dropCD9Seg.DataSource = dtrChrCD9Seq
                            dropCD9Seg.DataValueField = "SEQ"
                            dropCD9Seg.DataTextField = "ISA_CHGCD_USR3_SEG"
                            'pfd 08152008
                            'dropChrCD.DataTextField = "ISA_CHGCD_USR3_SEG"
                            dropCD9Seg.DataBind()
                            dtrChrCD9Seq.Close()
                            'Log whenever connection is closed with the DB: Vijay - 2/15/2013
                            If m_weblogstring = "true" Then
                                'WebLogCloseConn()
                            End If
                            dropCD9Seg.Items.Insert(0, New ListItem(" "))
                            dropCD9Seg.ID = "dropCD9Seg"

                    End Select
                    ''debug
                    '' - erwin
                    'If Not (dropChrCD Is Nothing) Then
                    '    dropChrCD.Attributes.Add("onChange", "javascript:__showSelected(this.name)")
                    'End If
                    I = I + 1
                Loop
                'SetFocus(PlaceHolder1.Controls(1))


            End If
        Catch objException As Exception
            sendErrorEmail(objException.ToString, "NO", Request.ServerVariables("URL"), strSQL)
            Response.Redirect("DBErrorPage.aspx?HOME=N")

        End Try

    End Sub

    Private Sub dropCD4Seg_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dropCD4Seg.SelectedIndexChanged
        Dim Strcrgcde As String
        Dim dtrChrCD5Seq As OleDbDataReader
        If dropCD5Seg.Visible Then
            '  Dim CD1Seg2Value = dropCD2Seg.SelectedValue
            dropCD5Seg.Items.Clear()
            If dropCD6Seg.Visible Then
                dropCD6Seg.Items.Clear()
            End If
            Strcrgcde = "SELECT  (ISA_CHGCD_CHILD2 || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR2_SEG" & vbCrLf &
                                             ",(ISA_CHGCD_CHILD2) AS SEQ  FROM SYSADM8.PS_ISA_CHGCD_CHLD2  where  BUSINESS_UNIT = '" & lblBU.Text & "'" & vbCrLf &
                                             "AND ISA_CHGCD_CHILD1='" & dropCD4Seg.SelectedValue & "' order by (ISA_CHGCD_USR2_SEG || ' ' || TAX_GROUP) asc "
            dtrChrCD5Seq = ORDBData.GetReader(Strcrgcde)
            If dtrChrCD5Seq.HasRows Then
                dropCD5Seg.DataSource = dtrChrCD5Seq
                dropCD5Seg.DataTextField = "ISA_CHGCD_USR2_SEG"
                dropCD5Seg.DataValueField = "SEQ"
                dropCD5Seg.DataBind()
                dropCD5Seg.Items.Insert(0, New ListItem(""))
                '    dropCD2Seg.SelectedValue = CD1Seg2Value

                Dim strDefChgCd1 As String = ""
                strDefChgCd1 = getDefaultChrCd("2")

                If Not strDefChgCd1 = "" Then
                    If Not dropCD5Seg.Items.FindByValue(strDefChgCd1) Is Nothing Then
                        dropCD5Seg.SelectedValue = strDefChgCd1
                        dropCD6Seg.Visible = False
                        dropCD5Seg_SelectedIndexChanged(Nothing, EventArgs.Empty)
                        dropCD5Seg.Enabled = True
                    End If
                End If
            End If
            dtrChrCD5Seq.Close()
        ElseIf dropCD6Seg.Visible Then
            dropCD6Seg.Items.Clear()
            Strcrgcde = "SELECT  (ISA_CHGCD_CHILD3 || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR2_SEG" & vbCrLf &
                                ",(ISA_CHGCD_CHILD3) AS SEQ  FROM SYSADM8.ps_isa_chgcd_CHLD3  where  BUSINESS_UNIT = '" & lblBU.Text & "'" & vbCrLf &
                                "AND ISA_CHGCD_CHILD2='" & dropCD5Seg.SelectedValue & "' order by (ISA_CHGCD_USR2_SEG || ' ' || TAX_GROUP) asc "
            dtrChrCD5Seq = ORDBData.GetReader(Strcrgcde)
            If dtrChrCD5Seq.HasRows Then
                dropCD6Seg.DataSource = dtrChrCD5Seq
                dropCD6Seg.DataTextField = "ISA_CHGCD_USR2_SEG"
                dropCD6Seg.DataValueField = "SEQ"
                dropCD6Seg.DataBind()
                dropCD6Seg.Items.Insert(0, New ListItem(" "))
            End If
        End If
    End Sub

    Private Sub dropCD5Seg_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles dropCD5Seg.SelectedIndexChanged
        If dropCD6Seg.Visible Then
            dropCD6Seg.Items.Clear()
            Dim Strcrgcde As String = "SELECT  (ISA_CHGCD_CHILD3 || ' ' || TAX_GROUP || ' ' || DESCR60) AS ISA_CHGCD_USR2_SEG" & vbCrLf &
                                ",(ISA_CHGCD_CHILD3) AS SEQ  FROM SYSADM8.ps_isa_chgcd_CHLD3  where  BUSINESS_UNIT = '" & lblBU.Text & "'" & vbCrLf &
                                "AND ISA_CHGCD_CHILD2='" & dropCD5Seg.SelectedValue & "' "
            If Not String.IsNullOrEmpty(dropCD4Seg.SelectedValue.Trim()) And dropCD4Seg.Visible Then
                Strcrgcde = Strcrgcde + "AND ISA_CHGCD_CHILD1='" & dropCD4Seg.SelectedValue & "' "
            End If
            Strcrgcde = Strcrgcde + "order by (ISA_CHGCD_USR2_SEG || ' ' || TAX_GROUP) asc "
            Dim dtrChrCD5Seq As OleDbDataReader = ORDBData.GetReader(Strcrgcde)
            If dtrChrCD5Seq.HasRows Then
                dropCD6Seg.DataSource = dtrChrCD5Seq
                dropCD6Seg.DataTextField = "ISA_CHGCD_USR2_SEG"
                dropCD6Seg.DataValueField = "SEQ"
                dropCD6Seg.DataBind()
                dropCD6Seg.Items.Insert(0, New ListItem(" "))
            End If
            dtrChrCD5Seq.Close()
        End If
    End Sub
    Private Function getDefaultChrCd(strSegmentType As String) As String
        Dim strChrCd As String = ""
        Dim strQuery As String = ""
        Try
            If strSegmentType = "1" Then
                strQuery = "SELECT ISA_CHRCD_CHILD1 FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID='" & rcbdropSelectUser.SelectedValue & "' AND BUSINESS_UNIT='" & lblBU.Text & "'"
            ElseIf strSegmentType = "2" Then
                strQuery = "SELECT ISA_CHRCD_CHILD2 FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID='" & rcbdropSelectUser.SelectedValue & "' AND BUSINESS_UNIT='" & lblBU.Text & "'"
            End If

            strChrCd = ORDBData.GetScalar(strQuery)

        Catch ex As Exception
            strChrCd = ""
        End Try
        Return strChrCd
    End Function

    Private Sub GetProdDispType(Optional ByVal strBU As String = "")

        Dim defaultShipTo As String = " "
        Dim strSQLstring As String
        Dim dsUserPreference As DataSet
        Dim strProdDisp As String = m_cProdDispType_CatalogSQL

        Dim strShitpTo As String = ""
        Dim strDept As String = ""
        Dim strBusinessUnit As String = ""
        Try
            strBusinessUnit = rcbGroup.SelectedValue.Trim
            If Trim(strBusinessUnit) = "" Then
                strBusinessUnit = strBU
            End If

        Catch ex As Exception
            strBusinessUnit = strBU
        End Try

        strSQLstring = "SELECT ISA_PROD_DISPLAY, ISA_PRICE_BLOCK,ISA_SDI_EMPLOYEE,SHIPTO_DEFAULT,ISA_DEPT FROM SDIX_USERS_TBL" & vbCrLf & _
            " WHERE ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text).ToUpper & "'"
        Try
            'strProdDisp = ORDBData.GetScalar(strSQLstring)
            dsUserPreference = ORDBData.GetAdapter(strSQLstring)

            strSQLstring = "SELECT SHIP_TO_FLG FROM PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT='" & strBusinessUnit & "' "

            strShitpTo = ORDBData.GetScalar(strSQLstring)

            If Not dsUserPreference Is Nothing And dsUserPreference.Tables(0).Rows.Count > 0 Then
                Dim drPreference As DataRow = dsUserPreference.Tables(0).Rows(0)
                strProdDisp = Convert.ToString(drPreference("ISA_PROD_DISPLAY"))

                If Convert.ToString(drPreference("ISA_PRICE_BLOCK")).ToUpper().Equals("Y") Then
                    chkbxDisplayPrice.Checked = True
                Else
                    chkbxDisplayPrice.Checked = False
                End If
                'Load ship to location 
                If Convert.ToString(drPreference("ISA_SDI_EMPLOYEE")).ToUpper().Equals("C") Or strShitpTo = "Y" Then
                    defaultShipTo = Convert.ToString(drPreference("SHIPTO_DEFAULT"))
                    Try
                        Dim shiptoDs As DataSet = getShipToLoc(UCase(strBusinessUnit))
                        If Not shiptoDs Is Nothing And shiptoDs.Tables(0).Rows.Count > 0 Then
                            lblShipto.Visible = True
                            dropShipto.Visible = True
                            dropShipto.DataSource = shiptoDs
                            dropShipto.DataValueField = "CUSTID"
                            dropShipto.DataTextField = "locname"
                            dropShipto.DataBind()
                            dropShipto.Items.Insert(0, "-- Select ShipTo --")
                            'Chaeck default location is available in user table or not.
                            If Not defaultShipTo = "" And Not defaultShipTo = Nothing Then
                                dropShipto.SelectedValue = defaultShipTo
                            End If
                        End If
                    Catch ex As Exception

                    End Try

                Else
                    lblShipto.Visible = False
                    dropShipto.Visible = False
                    If Not dropShipto.DataSource = Nothing Then
                        dropShipto.SelectedValue = 0
                    End If
                End If

                If Convert.ToString(drPreference("ISA_SDI_EMPLOYEE")).ToUpper().Equals("S") Then
                    strDept = Convert.ToString(drPreference("ISA_DEPT"))
                    Try
                        strSQLstring = "SELECT DEPT_ID,DEPT_NAME FROM SDIX_TCKT_DEPT"
                        Dim deptDs As DataSet = ORDBData.GetAdapter(strSQLstring)

                        If Not deptDs Is Nothing And deptDs.Tables(0).Rows.Count > 0 Then
                            lblUserDept.Visible = True
                            drpDept.Visible = True
                            drpDept.DataSource = deptDs
                            drpDept.DataValueField = "DEPT_ID"
                            drpDept.DataTextField = "DEPT_NAME"
                            drpDept.DataBind()
                            drpDept.Items.Insert(0, "-- Select Dept --")
                            If Not strDept = "" And Not strDept = Nothing Then
                                drpDept.SelectedValue = strDept
                            End If
                        End If

                    Catch ex As Exception

                    End Try
                Else
                    lblUserDept.Visible = False
                    drpDept.Visible = False
                    If Not drpDept.DataSource = Nothing Then
                        drpDept.SelectedValue = 0
                    End If

                End If

            End If
        Catch ex As Exception
            strProdDisp = m_cProdDispType_CatalogSQL
            chkbxDisplayPrice.Checked = False
        End Try
        If strProdDisp = m_cProdDispType_PSClient Then
            rrbProdDispPSClient.Checked = True
            rrbProdDispCatSQL.Checked = False
        Else
            rrbProdDispCatSQL.Checked = True
            rrbProdDispPSClient.Checked = False
        End If
    End Sub

    Private Sub btnSubmitPrefs_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSubmitPrefs.Click
        UpdatePrefs()
    End Sub

    Private Sub UpdatePrefs()
        Try
            lblMessage.Text = ""
            Dim strSQLstring As String = ""
            Dim strProdDisplay As String = m_cProdDispType_CatalogSQL
            Dim strBlockPrice As String = ""
            Dim strShipTo As String = ""
            Dim strDept As String = ""
            Dim rowsaffected As Integer = 0

            If chkbxDisplayPrice.Checked Then
                strBlockPrice = "Y"
            End If

            If rrbProdDispPSClient.Checked Then
                strProdDisplay = m_cProdDispType_PSClient
            End If

            If Not dropShipto.SelectedValue = "-- Select ShipTo --" And dropShipto.SelectedIndex > 0 Then
                strShipTo = dropShipto.SelectedValue
            End If

            If Not drpDept.SelectedValue = "-- Select Dept --" And drpDept.SelectedIndex > 0 Then
                strDept = drpDept.SelectedValue
            End If

            Dim CHILD1 As String
            Dim CHILD2 As String
            If Not dropCD4Seg.SelectedValue = "" Then
                CHILD1 = dropCD4Seg.SelectedValue
                CHILD2 = dropCD5Seg.SelectedValue
            Else
                CHILD1 = dropCD7Seg.SelectedValue
                CHILD2 = dropCD8Seg.SelectedValue
            End If
            If (dropCD4Seg.Visible = True And dropCD5Seg.Visible = True) Or (dropCD7Seg.Visible = True And dropCD8Seg.Visible = True) Then

                If Not CHILD1 = "" And Not CHILD2 = "" Then
                    strSQLstring = "UPDATE SDIX_USERS_TBL" & vbCrLf &
                        " SET ISA_PROD_DISPLAY = '" & strProdDisplay & "'" & vbCrLf &
                        ", ISA_PRICE_BLOCK = '" & strBlockPrice & "'" & vbCrLf &
                        " , SHIPTO_DEFAULT='" & strShipTo & "'" & vbCrLf &
                        " , ISA_DEPT ='" & strDept & "'" & vbCrLf &
                        ",ISA_CHRCD_CHILD1='" & CHILD1 & "'" & vbCrLf &
                        ",ISA_CHRCD_CHILD2='" & CHILD2 & "'" & vbCrLf &
                        " WHERE ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text).ToUpper & "'"

                    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                    If rowsaffected > 0 Then
                        Session("ISAShipToID") = strShipTo
                        'lblMessage.Visible = True
                        'lblMessage.Text = "User preference has been modified and saved successfully."
                        lblPrefMessage.Visible = True
                        lblPrefMessage.Text = "User preference has been modified and saved successfully."
                    End If
                Else
                    lblPrefMessage.Visible = True
                    lblPrefMessage.Text = "Please select the ChargeCode"
                End If
            ElseIf (dropCD4Seg.Visible = True Or dropCD5Seg.Visible = True) Or (dropCD7Seg.Visible = True Or dropCD8Seg.Visible = True) Then
                If Not CHILD1 = "" Then
                    strSQLstring = "UPDATE SDIX_USERS_TBL" & vbCrLf &
                        " SET ISA_PROD_DISPLAY = '" & strProdDisplay & "'" & vbCrLf &
                        ", ISA_PRICE_BLOCK = '" & strBlockPrice & "'" & vbCrLf &
                        " , SHIPTO_DEFAULT='" & strShipTo & "'" & vbCrLf &
                        " , ISA_DEPT ='" & strDept & "'" & vbCrLf &
                        ",ISA_CHRCD_CHILD1='" & CHILD1 & "'" & vbCrLf &
                        ",ISA_CHRCD_CHILD2='" & CHILD2 & "'" & vbCrLf &
                        " WHERE ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text).ToUpper & "'"

                    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                    If rowsaffected > 0 Then
                        Session("ISAShipToID") = strShipTo
                        'lblMessage.Visible = True
                        'lblMessage.Text = "User preference has been modified and saved successfully."
                        lblPrefMessage.Visible = True
                        lblPrefMessage.Text = "User preference has been modified and saved successfully."
                    End If
                Else
                    lblPrefMessage.Visible = True
                    lblPrefMessage.Text = "Please select ChargeCode"
                End If
            Else
                strSQLstring = "UPDATE SDIX_USERS_TBL" & vbCrLf &
                   " SET ISA_PROD_DISPLAY = '" & strProdDisplay & "'" & vbCrLf &
                   ", ISA_PRICE_BLOCK = '" & strBlockPrice & "'" & vbCrLf &
                   " , SHIPTO_DEFAULT='" & strShipTo & "'" & vbCrLf &
                   " , ISA_DEPT ='" & strDept & "'" & vbCrLf &
                   ",ISA_CHRCD_CHILD1='" & CHILD1 & "'" & vbCrLf &
                   ",ISA_CHRCD_CHILD2='" & CHILD2 & "'" & vbCrLf &
                   " WHERE ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text).ToUpper & "'"

                rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
                If rowsaffected > 0 Then
                    Session("ISAShipToID") = strShipTo
                    'lblMessage.Visible = True
                    'lblMessage.Text = "User preference has been modified and saved successfully."
                    lblPrefMessage.Visible = True
                    lblPrefMessage.Text = "User preference has been modified and saved successfully."
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub
#End Region

#Region "TANGO"
    Private Sub SetUpTrackTab()
        Dim oSDITrack As New clsSDITrack()
        Try
            Dim sTrackAddedUserName As String
            Dim sTrackAddedUserDate As String
            Dim sTrackAddedUserGUID As String

            lblSDiTrackCurrUserVal.Text = Session("USERID").ToString

            If oSDITrack.IsAccountUser(sTrackAddedUserName, sTrackAddedUserDate, sTrackAddedUserGUID) Then
                txtTangoUserName.Text = sTrackAddedUserName
                txtTangoUserName.Visible = False
                lblTangoUserNameStored.Text = sTrackAddedUserName
                lblTangoUserNameStored.Visible = True
                txtTangoUserName.ReadOnly = True
                txtTangoPassword.Visible = False
                lblTangoPassword.Visible = False
                txtTangoPassword.ReadOnly = True
                lblSDiTrackDateTime.Visible = True
                lblSDiTrackDateTimeVal.Visible = True
                lblSDiTrackGuid.Visible = True
                lblSDiTrackGuidVal.Visible = True
                lblSDiTrackDateTimeVal.Text = sTrackAddedUserDate
                lblSDiTrackGuidVal.Text = sTrackAddedUserGUID
                btnTangoAddUser.Visible = False
            Else
                txtTangoUserName.ReadOnly = False
                txtTangoPassword.ReadOnly = False
                btnTangoAddUser.Visible = True
                lblSDiTrackDateTime.Visible = False
                lblSDiTrackDateTimeVal.Visible = False
                lblSDiTrackGuid.Visible = False
                lblSDiTrackGuidVal.Visible = False
            End If

            If Page_Action = "EDIT" Then
                ddlSDiUsers.Visible = True

                Dim dsORUsers As DataSet = GetSelectDropDownData()
                ddlSDiUsers.DataSource = dsORUsers
                ddlSDiUsers.DataValueField = "ISA_EMPLOYEE_ID"
                ddlSDiUsers.DataTextField = "USERANDBU" ' "ISA_USER_NAME"
                ddlSDiUsers.DataBind()

                ShowTrackData()
            End If

        Catch ex As Exception
            lblValidation.Text = "SDiTrack Issue"
        End Try
    End Sub
#End Region

#Region "MOBILITY"
    Private Sub LoadGRIBIDENTITY()
        Dim StrQuery As String = String.Empty
        Dim UserID As String = String.Empty
        lblGRIBErr.Text = ""
        Try
            UserID = Trim(txtUserid.Text).ToUpper
        Catch ex As Exception
            UserID = " "
        End Try

        Dim StrGRIB As String = GetUserIDENT(UserID)

        StrQuery = "SELECT DISTINCT(SUBSTR(BUSINESS_UNIT_IN, 2, 4)) As BUUNIT FROM SYSADM8.PS_DS_NETDET_TBL WHERE DS_NETWORK_CODE = (SELECT DS_NETWORK_CODE " & vbCrLf & _
        "FROM SYSADM8.PS_BUS_UNIT_TBL_OM WHERE BUSINESS_UNIT = (SELECT BUSINESS_UNIT FROM SDIX_USERS_TBL WHERE UPPER(ISA_EMPLOYEE_ID) = '" & UserID & "'))"
        Dim dtrEMPReader As OleDbDataReader = ORDBData.GetReader(StrQuery)

        ddlGrib.DataSource = dtrEMPReader
        ddlGrib.DataTextField = "BUUNIT"
        ddlGrib.DataValueField = "BUUNIT"
        ddlGrib.DataBind()
        ddlGrib.Items.Insert(0, New ListItem("-- SELECT --"))


        If Not ddlGrib.Items.FindByValue(StrGRIB) Is Nothing Then
            ddlGrib.Items.FindByValue(StrGRIB).Selected = True
        End If
        dtrEMPReader.Close()
    End Sub

    Private Function GetUserIDENT(ByVal StrUser As String) As String
        Dim UserIdent As String = String.Empty
        Dim StrQuery As String = "SELECT ISA_CRIB_IDENT FROM SDIX_USERS_TBL WHERE UPPER(ISA_EMPLOYEE_ID)='" & StrUser & "'"
        UserIdent = ORDBData.GetScalar(StrQuery)
        Return UserIdent
    End Function

    Protected Sub btnGribSubmit_Click(sender As Object, e As EventArgs)
        Dim strMessage As New Alert

        If ddlGrib.SelectedIndex = 0 Then
            ltlAlert.Text = strMessage.Say("Please select Crib")
            Exit Sub
        End If

        Dim StrGRIB As String = ddlGrib.SelectedValue

        Dim StrQury As String = "UPDATE SDIX_USERS_TBL SET LASTUPDOPRID='" & CStr(Session("USERID")).Trim.ToUpper & "' , LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
            ", ISA_CRIB_IDENT='" & StrGRIB & "' WHERE UPPER(ISA_EMPLOYEE_ID)='" & Trim(txtUserid.Text).ToUpper & "' "

        Dim rowAffected As Integer = ORDBData.ExecNonQuery(StrQury, False)

        If rowAffected > 0 Then
            lblGRIBErr.Text = "Successfully Updated"
        Else
            lblGRIBErr.Text = "Update Failed. Error message was sent to IT Group. Please contact Help Desk."
        End If
    End Sub

#End Region

    Private Sub btnUserAccessCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUserAccessCancel.Click
        Session("PageAction") = ""
        Session("ChangePWDEnabled") = ""
        Response.Redirect(Session("DEFAULTPAGE").ToString())
    End Sub

    'Private Sub rtvPrograms_NodeCheck(ByVal sender As Object, ByVal e As Telerik.Web.UI.RadTreeNodeEventArgs) Handles rtvPrograms.NodeCheck
    '    If rblType.SelectedItem.Value.ToLower().Equals("role") Then
    '        ' If the user clicks on a program and the user is currently
    '        ' set to a role not ala carte, change to ala carte.
    '        rblType.Items.FindByText("Role").Selected = False
    '        rblType.Items.FindByText("Ala Carte").Selected = True
    '    End If
    'End Sub

    Private Sub rtvPrograms_NodeDataBound(ByVal sender As Object, ByVal e As Telerik.Web.UI.RadTreeNodeEventArgs) Handles rtvPrograms.NodeDataBound
        If e.Node.DataItem("active").ToString() = clsProgramMaster.InactiveProgramCode.ToString Then
            e.Node.Enabled = False
            e.Node.Checkable = False 'YA 20180629 Ticket 137359/Task 1316 disabling checkable property of inactive nodes in user priv panel to prevent unique constraint errors
        ElseIf e.Node.DataItem("securityalias").ToString.Trim.Length = 0 Then
            e.Node.Enabled = False
        ElseIf e.Node.DataItem("securityalias").ToString().ToUpper() = GetPrivilegeMoniker(UserPrivsEnum.Home) Then
            e.Node.Selected = True
            e.Node.Enabled = False
        End If
        ''To disable the SDI users menu items for the cust user 
        If radioUserType.SelectedValue = "C" Then
            If e.Node.DataItem("access_group").ToString = "SDI" Then
                e.Node.Visible = False
            End If
        End If
    End Sub

    Private Sub btnCollapseAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCollapseAll.Click
        rtvPrograms.CollapseAllNodes()
    End Sub

    Private Sub btnExpandAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnExpandAll.Click
        rtvPrograms.ExpandAllNodes()
    End Sub
    'Button click to check all the items of checkbox
    Private Sub btnSelectAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSelectAll.Click
        rtvPrograms.CheckAllNodes()
    End Sub
    'Button click to uncheck all the items of checkbox
    Private Sub btnDeselectAll_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnDeselectAll.Click
        rtvPrograms.UncheckAllNodes()
    End Sub

    Private Sub GetAlaCarteData()
        Try
            Dim strSDICust As String = ""
            Dim strUserType As String = ""
            GetDisplayedUserType(strSDICust, strUserType)

            rtvPrograms.ClearCheckedNodes()
            Dim dsUserPrivileges As DataSet = New DataSet
            'dsUserPrivileges = BuildingMenus.BuildMenu.GetDBDrivenMenu(ViewState("BU"), txtUserid.Text)  '  , strSDICust)
            dsUserPrivileges = BuildingMenus.BuildMenu.GetUserMenu(ViewState("BU"), txtUserid.Text, strSDICust)
            If Not dsUserPrivileges Is Nothing Or dsUserPrivileges.Tables(0).Rows.Count > 0 Then
                Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.GetAllNodes()
                ' Loop through privilege tree
                For Each node As RadTreeNode In nodeCollection
                    ' Find current privilege tree program in user list of privileges
                    Dim bFound As Boolean = False
                    Dim iRowIndex As Integer = 0
                    While iRowIndex < dsUserPrivileges.Tables(0).Rows.Count And Not bFound
                        Dim name As String = ""
                        Dim dr As DataRow = dsUserPrivileges.Tables(0).Rows(iRowIndex)
                        Try
                            name = dr("securityalias")
                        Catch ex As Exception
                            Try
                                ' Use this in case the privilege was deleted at some point in the past.
                                name = dr("securityalias", DataRowVersion.Original)
                            Catch ex1 As Exception
                            End Try
                        End Try
                        If name.Length > 0 Then
                            If node.Value = name Then
                                node.Checked = True
                                bFound = True
                            End If
                        End If

                        iRowIndex = iRowIndex + 1
                    End While
                Next
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Function GetSelectDropDownData() As DataSet
        Dim strSQLSelect As String
        Dim strSQLWhere As String
        Dim strSQLOrder As String
        Dim strSQLString As String
        Dim dsORUsers As DataSet

        Try
            Dim SelectedValueforUserType As String = radioUserType.SelectedValue
            '' strSQLSelect = "SELECT ISA_USER_NAME, BUSINESS_UNIT, ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL Where ISA_SDI_EMPLOYEE = '" & SelectedValueforUserType & "' AND ACTIVE_STATUS IN ('A','F') ORDER BY ISA_USER_NAME"
            strSQLSelect = "select isa_user_name, isa_employee_id" & vbCrLf
            If SelectedValueforUserType = "V" Then
                strSQLSelect = strSQLSelect & ", isa_user_name || ' - ' || business_unit || ' - ' || isa_employee_id ||' - ' || isa_vendor_id || decode(active_status,'I',' - INACTIVE','F',' - FAILED LOGIN',' ')  as  userandbu" & vbCrLf
            Else
                strSQLSelect = strSQLSelect & ", isa_user_name || ' - ' || business_unit || ' - ' || isa_employee_id || decode(active_status,'I',' - INACTIVE','F',' - FAILED LOGIN',' ')  as  userandbu" & vbCrLf
            End If

            strSQLSelect = strSQLSelect & " from sdix_users_tbl"
            'If Session("usertype") = "admin" Or _
            '    Session("usertype") = "adminx" Or _
            '    Session("usertype") = "adminr" Then
            '    strSQLWhere = " where business_unit = '" & Session("busunit") & "'" & _
            '            " and isa_sdi_employee = 'c'" & _
            '            " and isa_employee_actyp <> 'annou'"

            If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                strSQLWhere = " where business_unit = '" & Session("busunit") & "'" & _
                        " and isa_sdi_employee = 'C'" & _
                        " and isa_employee_actyp <> 'annou'"
            Else
                strSQLWhere = " where isa_employee_actyp <> 'annou'"
            End If

            If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                strSQLWhere = strSQLWhere & " and not isa_employee_id like '%<%' and not isa_employee_id like '%>%' "
            Else
                strSQLWhere = strSQLWhere & " and not isa_employee_id like '%<%' and not isa_employee_id like '%>%'  and isa_sdi_employee='" & SelectedValueforUserType & "'"
            End If



            ' save changes - if any - for  business_unit = '" & rcbgroup.selectedvalue & "'

            'if isvendor() then
            '    strsqlwhere = strsqlwhere & " and business_unit = '" & me.drpbunit.selectedvalue & "'"
            '    'strsqlwhere = strsqlwhere & " and business_unit = 'isa00'"
            'elseif ismexicovendor() then
            '    strsqlwhere = strsqlwhere & " and business_unit = 'sdm00'"
            'else
            '    strsqlwhere = strsqlwhere & " and business_unit <> 'isa00'"
            'end if

            If Not IsUserCanReinstate() Then
                ' if logged in user can reinstate an account, don't filter out by active_status; show all users regardless of status.
                ' if the logged in user cannot reinstate an account, just show active users and users who are locked out because of too many failed login attempts.
                strSQLOrder = " and active_status in ('" & clsUserTbl.ActiveStatus_Active & "','" & clsUserTbl.ActiveStatus_FailedLogin & "')"

            End If
            If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                strSQLOrder &= " or isa_employee_id = '" & Session("USERID") & "' order by isa_user_name"
            Else
                strSQLOrder &= " order by isa_user_name"
            End If
            'strSQLOrder &= " order by isa_user_name"

            strSQLString = strSQLSelect & strSQLWhere & strSQLOrder

            dsORUsers = ORDBData.GetAdapter(strSQLString)
        Catch ex As Exception
            Dim esb As Integer = 1
        End Try

        Return dsORUsers
    End Function

    Private Sub ddlSDiUsers_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlSDiUsers.SelectedIndexChanged
        ShowTrackData()
    End Sub

    Private Sub ShowTrackData()
        Try
            If ddlSDiUsers.SelectedValue.ToString.Trim.Length > 0 Then
                Dim oUserTbl As New clsUserTbl(ddlSDiUsers.SelectedValue, "")
                lblOtherUserIDVal.Text = ddlSDiUsers.SelectedValue

                'add user = off
                btnTangoAddOtherUser.Visible = False
                'message = off
                lblOtherUserBUMessage.Visible = False
                'Track user ID = off
                txtOtherUserTangoUserNameVal.Visible = False
                lblOtherUserTangoUserNameValStored.Visible = False
                lblOtherUserTangoUserNameValStored.Text = ""
                txtOtherUserTangoUserNameVal.Text = ""
                'password = off
                lblOtherUserTangoPassword.Visible = False
                txtOtherUserTangoPasswordVal.Visible = False
                'added date = off
                lblOtherUserAddedOn.Visible = False
                lblOtherUserAddedOnVal.Visible = False
                'GUID = off
                lblOtherUserGUID.Visible = False
                lblOtherUserGUIDVal.Visible = False
                'validation = off
                lblValidationOtherUser.Text = ""

                Dim oGroup As Object = rcbGroup.FindItemByValue(oUserTbl.BusinessUnit)
                If oGroup IsNot Nothing Then
                    lblOtherUserBUVal.Text = CType(oGroup, RadComboBoxItem).Text

                    'no BU message = off
                    lblOtherUserNoBU.Visible = False
                    lblOtherUserBUVal.Visible = True

                    Dim oEnterprise As New clsEnterprise(oUserTbl.BusinessUnit)
                    If oEnterprise.TrackDBType.Trim().Length = 0 Then
                        'message = on
                        lblOtherUserBUMessage.Visible = True
                        'Track user ID = off
                        lblOtherUserTangoUserName.Visible = False
                    Else
                        Dim oSDITrack As New clsSDITrack
                        If oUserTbl.TrackUserGUID.Trim.Length > 0 Then
                            'Track user ID = on, display
                            lblOtherUserTangoUserName.Visible = True
                            lblOtherUserTangoUserNameValStored.Text = oUserTbl.TrackUserName
                            lblOtherUserTangoUserNameValStored.Visible = True
                            'added date = on
                            lblOtherUserAddedOn.Visible = True
                            lblOtherUserAddedOnVal.Text = oUserTbl.TrackToDate
                            lblOtherUserAddedOnVal.Visible = True
                            'GUID = on
                            lblOtherUserGUID.Visible = True
                            lblOtherUserGUIDVal.Text = oUserTbl.TrackUserGUID
                            lblOtherUserGUIDVal.Visible = True
                        Else
                            'add user = on
                            btnTangoAddOtherUser.Visible = True
                            'Track user ID = on, editable
                            lblOtherUserTangoUserName.Visible = True
                            txtOtherUserTangoUserNameVal.Visible = True
                            'password = on
                            lblOtherUserTangoPassword.Visible = True
                            txtOtherUserTangoPasswordVal.Visible = True
                        End If
                    End If
                Else
                    'Track user ID = off
                    lblOtherUserTangoUserName.Visible = False
                    'no BU message = on
                    lblOtherUserNoBU.Visible = True
                    lblOtherUserBUVal.Visible = False
                End If
            End If
        Catch ex As Exception
            Dim esb As Integer = 1
        End Try
    End Sub

    Private Sub AddTangoUser(ByVal sUserID As String, ByVal sBU As String, ByVal txtUserName As TextBox, ByVal txtPassword As TextBox, ByVal lblPassword As Label, ByVal lblValidate As Label, _
                             ByVal lblUserNameStored As Label, ByVal lblDateTime As Label, ByVal lblDateTimeVal As Label, ByVal lblGUID As Label, _
                             ByVal lblGUIDVal As Label, ByVal btnAddUser As Button)
        Dim oSDITrack As New clsSDITrack()
        Dim bUserExists As Boolean = False
        Try
            Dim sNewUserGUID As String
            Dim dtTodaysDate As Date
            If oSDITrack.AddUser(sUserID, sBU, txtUserName.Text, txtPassword.Text, bUserExists, sNewUserGUID, dtTodaysDate) Then
                lblValidate.Text = "User Added to SDiTrack"
                lblUserNameStored.Text = txtUserName.Text
                lblUserNameStored.Visible = True
                txtUserName.Visible = False
                lblDateTime.Visible = True
                lblDateTimeVal.Visible = True
                lblGUID.Visible = True
                lblGUIDVal.Visible = True
                txtPassword.Visible = False
                lblPassword.Visible = False
                btnAddUser.Visible = False
            ElseIf bUserExists Then
                lblValidate.Text = "UserName Already Exists in SDiTrack"
            End If
            lblGUIDVal.Text = sNewUserGUID
            lblDateTimeVal.Text = dtTodaysDate.ToString()
        Catch ex As Exception
            If bUserExists Then
                lblValidate.Text = "UserName Already Exists in SDiTrack"
            End If
        End Try
    End Sub

    Private Sub btnTangoAddOtherUser_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnTangoAddOtherUser.Click
        Dim oUserTbl As New clsUserTbl(ddlSDiUsers.SelectedValue, "")
        Dim sBU As String = oUserTbl.BusinessUnit

        AddTangoUser(lblOtherUserIDVal.Text, sBU, txtOtherUserTangoUserNameVal, txtOtherUserTangoPasswordVal, _
                     lblOtherUserTangoPassword, lblValidationOtherUser, lblOtherUserTangoUserNameValStored, lblOtherUserAddedOn, _
                     lblOtherUserAddedOnVal, lblOtherUserGUID, lblOtherUserGUIDVal, btnTangoAddOtherUser)
    End Sub

    Private Sub AddNode(ByVal ePriv As UserPrivsEnum, ByVal rtv As RadTreeView, ByVal dsPrograms As DataSet)
        Dim sSecurityAlias As String = GetPrivilegeMoniker(ePriv)
        If sSecurityAlias.Length > 0 Then
            Dim newNode As New RadTreeNode
            Dim rows() As DataRow = dsPrograms.Tables(0).Select("securityalias = '" & sSecurityAlias & "'")

            If rows.Length = 1 Then
                newNode.Text = rows(0).Item("programname")
                newNode.Value = sSecurityAlias
                newNode.Checked = True
                rtv.Nodes.Add(newNode)
            End If
        End If
    End Sub

    Private Sub GetDisplayedUserType(ByRef strSDICust As String, ByRef strUserType As String)
        Try
            If lblAction.Text = "ADD" Then
                If IsVendor() Or IsMexicoVendor() Then
                    strSDICust = "V"
                Else
                    strSDICust = "C"
                End If
                strUserType = "USER"
            End If

            'If drpUserType.Visible = True Then
            'Select Case drpUserType.SelectedIndex
            '    Case 0, 1, 2, 3, 4
            '        If IsVendor() Or IsMexicoVendor() Then
            '            strSDICust = "V"
            '        Else
            '            strSDICust = "C"
            '        End If
            '        strUserType = drpUserType.SelectedValue
            '    Case 5, 6, 7
            '        If IsVendor() Or IsMexicoVendor() Then
            '            strSDICust = "V"
            '        Else
            '            strSDICust = "S"
            '        End If
            '        strUserType = drpUserType.SelectedValue.Substring(3)
            'End Select
            strSDICust = radioUserType.SelectedValue
            strUserType = roleDropdownList.SelectedValue

            'End If
        Catch ex As Exception

        End Try
    End Sub

    Private Function GetDisplayedUserID() As String
        Dim strUSERID As String
        If Page_Action = "ADD" Then
            If radioUserType.SelectedValue = "C" Or radioUserType.SelectedValue = "S" Then
                If UserIdSessionvalue <> "" Then
                    strUSERID = UserIdSessionvalue.Trim.ToUpper
                    strUSERID = Replace(strUSERID, "'", "")
                End If
            Else
                strUSERID = txtUserid.Text.Trim.ToUpper
                strUSERID = Replace(strUSERID, "'", "")
            End If
        Else
            strUSERID = txtUserid.Text.Trim.ToUpper
            strUSERID = Replace(strUSERID, "'", "")
        End If

        'Dim strUSERID As String = txtUserid.Text.Trim.ToUpper
        'strUSERID = Replace(strUSERID, "'", "")
        Return strUSERID
    End Function

    Private Function ConvertSDICustToSDIEmp(ByVal sSDICust As String)
        Dim sSDIEmp As String
        If sSDICust = "S" Then
            sSDIEmp = "SDI"
        Else
            sSDIEmp = "CUST"
        End If
        Return sSDIEmp
    End Function

    Private Function GetPortal(ByVal sDisplayedSDICust As String) As String
        ' Default is Customer portal
        Dim sPortal As String = clsProgramMaster.PortalCustomer

        If IsVendor() Or sDisplayedSDICust = "V" Then
            sPortal = clsProgramMaster.PortalVendor
        ElseIf IsMexicoVendor() Then
            sPortal = clsProgramMaster.PortalVendor
        End If

        Return sPortal
    End Function

    Private Function IsVendor() As Boolean
        Dim bIsVendor As Boolean = False


        If Request.QueryString("VENDOR") = "YES" Then
            bIsVendor = True
        End If

        Return bIsVendor
    End Function

    Private Function IsMexicoVendor() As Boolean
        Dim bIsMexico As Boolean = False

        If Request.QueryString("MEXICO") = "YES" Then
            bIsMexico = True
        End If

        Return bIsMexico
    End Function

    Private Sub HideExpandCollapseButtons()
        Dim iNodeIndex As Integer = 0
        Dim bFoundChildren As Boolean = False

        ' Following returns top-level parents
        Dim navTreePrograms As IList(Of RadTreeNode) = rtvPrograms.GetAllNodes()

        While iNodeIndex < navTreePrograms.Count And Not bFoundChildren
            ' Loop through top-level parents to see if there's a first level of children
            Dim childNodes As IList(Of RadTreeNode) = navTreePrograms(iNodeIndex).GetAllNodes
            If childNodes.Count > 0 Then
                bFoundChildren = True
            End If
            iNodeIndex = iNodeIndex + 1
        End While

        btnExpandAll.Visible = bFoundChildren
        btnCollapseAll.Visible = bFoundChildren
        'Added for hiding expand collapse select and deselect all button for corp admin
        If Session("Role") = "CORPADMIN" Then
            btnExpandAll.Visible = False
            btnCollapseAll.Visible = False
            btnSelectAll.Visible = False
            btnDeselectAll.Visible = False
        End If
    End Sub

    Private Function ExistsApprvRecord(ByVal strBU As String) As Boolean
        Dim bExistsRecord As Boolean = False
        Dim strSQLstring As String

        Try
            strSQLstring = "SELECT * FROM SDIX_USERS_APPRV WHERE ISA_EMPLOYEE_ID = '" & txtUserid.Text & _
                "' AND business_unit = '" & strBU & "'"
            Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)
            If ds.Tables(0).Rows.Count > 0 Then
                bExistsRecord = True
            End If
        Catch ex As Exception

        End Try

        Return bExistsRecord
    End Function

    Private Function ExistsReqApprvRecord(ByVal strBU As String) As Boolean
        Dim bExistsRecord As Boolean = False
        Dim strSQLstring As String

        Try
            strSQLstring = "SELECT * FROM SDIX_USERS_REQ_APPRV WHERE ISA_EMPLOYEE_ID = '" & txtUserid.Text & _
                "' AND business_unit = '" & strBU & "'"
            Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)
            If ds.Tables(0).Rows.Count > 0 Then
                bExistsRecord = True
            End If
        Catch ex As Exception

        End Try

        Return bExistsRecord
    End Function

    Private Function IsUserCanReinstate() As Boolean
        Dim bCanReinstate As Boolean = False

        Dim user_terminate As String = ConfigurationSettings.AppSettings("Profile_inactivate_setting")
        Dim sAllowedUsers() As String = user_terminate.ToUpper.Split(",")

        Dim i As Integer = 0
        While i < sAllowedUsers.Length And Not bCanReinstate
            If Session("USERID").ToString.ToUpper = sAllowedUsers(i) Then
                bCanReinstate = True
            End If
            i = i + 1
        End While

        If Not bCanReinstate Then
            If Not Session("USERTYPE") Is Nothing And Not Session("SDIEMP") Is Nothing Then
                If Convert.ToString(Session("USERTYPE")).ToUpper().Equals("ADMIN") And Convert.ToString(Session("SDIEMP")).ToUpper().Equals("CUST") Then
                    bCanReinstate = True
                ElseIf Convert.ToString(Session("USERTYPE")).ToUpper().Equals("CORPADMIN") And Convert.ToString(Session("SDIEMP")).ToUpper().Equals("CUST") Then
                    bCanReinstate = True
                End If
            End If
        End If
        'If Session("USERID") = user_terminate Then
        '    bCanReinstate = True
        'End If

        Return bCanReinstate
    End Function

    Private Sub btnActivateAccount_Click(sender As Object, e As EventArgs) Handles btnActivateAccount.Click
        Dim strSQLstring As String

        Try
            ' Set the account active
            strSQLstring = "UPDATE SDIX_USERS_TBL " & vbCrLf & _
                " SET active_status = '" & clsUserTbl.ActiveStatus_Active & "' " & vbCrLf & _
                " , lastupdoprid = '" & Session("USERID").ToString & "' " & vbCrLf & _
                " , lastupddttm = TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                " WHERE isa_employee_id = '" & txtUserid.Text & "' " & vbCrLf & _
                " AND active_status = '" & lblActiveStatusHide.Text & "' " & vbCrLf & _
                " AND isa_user_id = '" & lblUserIDHide.Text & "' "
            Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSQLstring, False)
            If rowsaffected = 1 Then
                Dim strBU As String = ""
                Dim strUserGroup As String = ""
                GetSelectedBUandGroup(strBU, strUserGroup)
                ' Write an audit record saying the account was reactivated.
                clsSDIAudit.AddRecord("Profile.aspx", "Activate account", "SDIX_USERS_TBL", Session("USERID").ToString, strBU, txtUserid.Text, sColumnChg:="active_status", _
                      sOldValue:=lblActiveStatusHide.Text, sNewValue:=clsUserTbl.ActiveStatus_Active)

                lblAccountDisabled.Text = "This SDIX User account is now active."
                btnActivateAccount.Visible = False
                btnInactivateAccount.Visible = True ' After activating an account, we want to give the ability to inactivate.

                'If user went from "failed login" to active, remove the "failed login" from the user dropdown
                'Dim iIndex As Integer = dropSelectUser.SelectedItem.Text.IndexOf(" - FAILED LOGIN")
                'If iIndex > 0 Then
                '    dropSelectUser.SelectedItem.Text = dropSelectUser.SelectedItem.Text.Substring(0, iIndex)
                'Else
                '    ' If user went from "inactive" to active, remove the "inactive" from the user dropdown
                '    iIndex = dropSelectUser.SelectedItem.Text.IndexOf(" - INACTIVE")
                '    dropSelectUser.SelectedItem.Text = dropSelectUser.SelectedItem.Text.Substring(0, iIndex)
                'End If
                Dim iIndex As Integer = rcbdropSelectUser.SelectedItem.Text.IndexOf(" - FAILED LOGIN")
                If iIndex > 0 Then
                    rcbdropSelectUser.SelectedItem.Text = rcbdropSelectUser.SelectedItem.Text.Substring(0, iIndex)
                Else
                    ' If user went from "inactive" to active, remove the "inactive" from the user dropdown
                    iIndex = rcbdropSelectUser.SelectedItem.Text.IndexOf(" - INACTIVE")
                    rcbdropSelectUser.SelectedItem.Text = rcbdropSelectUser.SelectedItem.Text.Substring(0, iIndex)
                End If
                lblActiveStatusHide.Text = clsUserTbl.ActiveStatus_Active ' Update the hidden label indicating the active status
            Else
                lblDBError.Text = "There was an issue trying to activate this account."
                lblDBError.Visible = True
            End If
            FieldsPosition()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub GetSelectedBUandGroup(ByRef strBU As String, ByRef strUserGroup As String)
        If Session("USERTYPE") = "SUPER" Then
            If lblVendor.Text = "YES" Or radioUserType.SelectedValue = "V" Then
                strBU = Me.drpBUnit.SelectedValue.ToString.Trim
                strUserGroup = m_cUserGroup_Vendor
            ElseIf lblMexico.Text = "YES" Then
                strBU = "SDM00"
                strUserGroup = m_cUserGroup_Mexico
            Else
                strBU = GetBUbyGroup(rcbGroup.SelectedValue)
                strUserGroup = rcbGroup.SelectedValue
            End If
        ElseIf Session("USERTYPE") = "ADMIN" Or Session("USERTYPE") = "CORPADMIN" Then
            If lblVendor.Text = "YES" Or radioUserType.SelectedValue = "V" Then
                If Page_Action = "ADD" Then
                    strBU = Me.drpBUnit.SelectedValue.ToString.Trim
                    strUserGroup = m_cUserGroup_Vendor
                Else
                    strBU = GetBUbyGroup(txtGroupID.Text)
                    strUserGroup = txtGroupID.Text
                End If
            ElseIf radioUserType.SelectedValue = "C" Then
                If Session("USERTYPE") = "CORPADMIN" Then
                    If Page_Action = "EDIT" Or Page_Action = "ADD" Then
                        strBU = GetBUbyGroup(rcbGroup.SelectedValue)
                        strUserGroup = rcbGroup.SelectedValue
                    End If
                Else
                    If Page_Action = "EDIT" Or Page_Action = "ADD" Then
                        strBU = GetBUbyGroup(txtGroupID.Text)
                        strUserGroup = txtGroupID.Text
                    ElseIf Page_Action = "ADD" Then
                        strBU = GetBUbyGroup(rcbGroup.SelectedValue)
                        strUserGroup = rcbGroup.SelectedValue
                    End If
                End If
            ElseIf radioUserType.SelectedValue = "S" Then
                strBU = GetBUbyGroup(txtGroupID.Text)
                strUserGroup = txtGroupID.Text
            End If
        ElseIf Session("USERTYPE") = "USER" Then
            If Session("USERTYPEVALUE") = "S" Then
                If Page_Action = "ADD" Then
                    If radioUserType.SelectedValue = "V" Then
                        strBU = Me.drpBUnit.SelectedValue.ToString.Trim
                        strUserGroup = m_cUserGroup_Vendor
                    Else
                        If Session("StoreRCBSessionValue") <> Nothing Then
                            rcbGroup.SelectedValue = Session("StoreRCBSessionValue")
                            strBU = GetBUbyGroup(rcbGroup.SelectedValue)
                            strUserGroup = rcbGroup.SelectedValue
                            Session.Remove("StoreRCBSessionValue")
                        End If
                    End If
                ElseIf Page_Action = "EDIT" Then
                    If radioUserType.SelectedValue = "S" Or radioUserType.SelectedValue = "C" Then
                        strBU = GetBUbyGroup(txtGroupID.Text)
                        strUserGroup = txtGroupID.Text
                    End If
                End If
            Else
                If Page_Action = "EDIT" Or Page_Action = "ADD" Then
                    strBU = GetBUbyGroup(txtGroupID.Text)
                    strUserGroup = txtGroupID.Text
                End If
            End If

        End If
    End Sub

    Private Sub btnInactivateAccount_Click(sender As Object, e As EventArgs) Handles btnInactivateAccount.Click
        Try
            Dim strSqlupd1 As String
            ' Inactivate the account
            strSqlupd1 = " UPDATE " & vbCrLf & _
                               " SDIX_USERS_TBL " & vbCrLf & _
                         " SET " & vbCrLf & _
                               " ACTIVE_STATUS = '" & clsUserTbl.ActiveStatus_Inactive & "'," & vbCrLf & _
                               " LASTUPDOPRID = '" & Session("USERID") & "'," & vbCrLf & _
                               " LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                         " WHERE " & vbCrLf & _
                               " ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text).ToUpper & "' " & vbCrLf & _
                               " AND active_status = '" & lblActiveStatusHide.Text & "' " & vbCrLf & _
                               " AND isa_user_id = '" & lblUserIDHide.Text & "' "

            Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSqlupd1, False)
            If rowsaffected = 1 Then
                Dim strBU As String = ""
                Dim strUserGroup As String = ""
                GetSelectedBUandGroup(strBU, strUserGroup)
                ' Write an audit record saying the account was reactivated.
                clsSDIAudit.AddRecord("Profile.aspx", "Inactivate account", "SDIX_USERS_TBL", Session("USERID").ToString, strBU, txtUserid.Text, sColumnChg:="active_status", _
                      sOldValue:=lblActiveStatusHide.Text, sNewValue:=clsUserTbl.ActiveStatus_Inactive)

                lblAccountDisabled.Text = "This SDIX User account is now inactive."
                lblAccountDisabled.Visible = True
                btnInactivateAccount.Visible = False
                btnActivateAccount.Visible = True ' After inactivating an account, we want to give the ability to activate.

                'If user went from "failed login" to inactive, remove the "failed login" from the user dropdown and change to inactive
                'Dim iIndex As Integer = dropSelectUser.SelectedItem.Text.IndexOf(" - FAILED LOGIN")
                'If iIndex > 0 Then
                '    dropSelectUser.SelectedItem.Text = dropSelectUser.SelectedItem.Text.Substring(0, iIndex) & " - INACTIVE"
                'Else
                '    dropSelectUser.SelectedItem.Text = dropSelectUser.SelectedItem.Text & " - INACTIVE"
                'End If
                Dim iIndex As Integer = rcbdropSelectUser.SelectedItem.Text.IndexOf(" - FAILED LOGIN")
                If iIndex > 0 Then
                    rcbdropSelectUser.SelectedItem.Text = rcbdropSelectUser.SelectedItem.Text.Substring(0, iIndex) & " - INACTIVE"
                Else
                    rcbdropSelectUser.SelectedItem.Text = rcbdropSelectUser.SelectedItem.Text & " - INACTIVE"
                End If
                lblActiveStatusHide.Text = clsUserTbl.ActiveStatus_Inactive ' Update the hidden label indicating the active status
            Else
                lblDBError.Text = "There was an issue trying to inactivate this account."
                lblDBError.Visible = True
            End If
            FieldsPosition()
        Catch ex As Exception
        End Try
    End Sub

    Private Sub GetMultiBusinessUnit(ByVal businessUnit As String)
        Try
            'Dim query As String = "SELECT Distinct ClientSite.SITE_ID AS BUSINESS_UNIT,ClientSite.SITE_ID || '-' || ClentSiteDesc.Descr AS DESCRIPTION " & vbCrLf & _
            '                                                   "FROM Bidwadmin.dw_client_site_loc ClientSite JOIN PS_BUS_UNIT_TBL_FS ClentSiteDesc ON " & vbCrLf & _
            '                                                   "ClientSite.SITE_ID=ClentSiteDesc.business_unit WHERE CLIENT =(SELECT Distinct CLIENT FROM Bidwadmin.dw_client_site_loc " & vbCrLf & _
            '                                                   "WHERE SITE_ID= 'I0914' AND site_status = 'Active' AND client != 'Inactive Client Sites') AND SITE_STATUS='Active' " & vbCrLf & _
            '                                                   "AND ClientSite.SITE_ID != 'ISA00'"
            Dim ds As DataSet = UnilogORDBData.SisterBusinessUnits(businessUnit)
            If ds.Tables(0).Rows.Count > 0 Then
                rcbMultiSelect.Visible = True
                rcbMultiSelect.DataSource = ds
                rcbMultiSelect.DataTextField = "DESCRIPTION"
                rcbMultiSelect.DataValueField = "BUSINESS_UNIT"
                rcbMultiSelect.DataBind()
            Else
                rcbMultiSelect.Visible = False
                Dim strScript As String = "<script language='javascript'>alert('Sister sites are not available for this selected BU:' + '" + businessUnit + "');</script>"
                ScriptManager.RegisterClientScriptBlock(Me.Page, Me.[GetType](), "PopupScript", strScript, False)
                MultiSiteChk.Checked = False
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub GetSisterSitesBU(ByVal businessUnit As String)
        Try
            Dim dsBUSisterSites As DataSet = UnilogORDBData.SisterBusinessUnits(businessUnit)
            If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
                rcbGroup.Visible = True
                rcbGroup.DataSource = dsBUSisterSites
                rcbGroup.DataTextField = "DESCRIPTION"
                rcbGroup.DataValueField = "BUSINESS_UNIT"
                rcbGroup.DataBind()
                'rcbGroup.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
                'rcbGroup.DataValueField.Insert(0, "0")
            Else
                dsBUSisterSites = GetBUDesc(businessUnit)
                If Not dsBUSisterSites Is Nothing Then
                    If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
                        rcbGroup.Visible = True
                        rcbGroup.DataSource = dsBUSisterSites
                        rcbGroup.DataTextField = "DESCRIPTION"
                        rcbGroup.DataValueField = "BUSINESS_UNIT"
                        rcbGroup.DataBind()
                        'rcbGroup.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
                        'rcbGroup.DataValueField.Insert(0, "0")
                    Else
                        rcbGroup.Visible = False
                        Dim strScript As String = "<script language='javascript'>alert('Sister sites are not available for this selected BU:' + '" + businessUnit + "');</script>"
                        ScriptManager.RegisterClientScriptBlock(Me.Page, Me.[GetType](), "PopupScript", strScript, False)
                        MultiSiteChk.Checked = False
                    End If
                Else
                    rcbGroup.Visible = False
                    Dim strScript As String = "<script language='javascript'>alert('Sister sites are not available for this selected BU:' + '" + businessUnit + "');</script>"
                    ScriptManager.RegisterClientScriptBlock(Me.Page, Me.[GetType](), "PopupScript", strScript, False)
                    MultiSiteChk.Checked = False
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Function GetBUDesc(ByVal strBU As String) As DataSet
        Try
            Dim QueryGetCurrentBU As String = "SELECT * FROM (SELECT  A.ISA_BUSINESS_UNIT as BUSINESS_UNIT,A.ISA_BUSINESS_UNIT || ' - ' || B.descr  as  DESCRIPTION " & vbCrLf & _
                "FROM  SYSADM8.PS_ISA_ENTERPRISE A, SYSADM8.PS_LOCATION_TBL B " & vbCrLf & _
                "WHERE  B.location =  'L'|| substr(A.ISA_BUSINESS_UNIT,2) || '-01' AND A.BU_STATUS = '1' " & vbCrLf & _
                "AND B.EFFDT = (SELECT MAX(A_ED.EFFDT) FROM PS_LOCATION_TBL A_ED WHERE B.SETID = A_ED.SETID " & vbCrLf & _
                "AND B.LOCATION = A_ED.LOCATION AND A_ED.EFFDT <= SYSDATE) " & vbCrLf & _
                "ORDER BY A.ISA_BUSINESS_UNIT) WHERE BUSINESS_UNIT = '" & strBU & "'"
            Dim dsBUData As DataSet
            dsBUData = ORDBData.GetAdapter(QueryGetCurrentBU)
            Return dsBUData
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Sub GetSisterSitesUsers(ByVal businessUnit As String, ByVal strType As String, ByVal strRole As String)
        Try
            Dim dsBUSisterSites As DataSet = UnilogORDBData.SisterBusinessUnits(businessUnit)
            If Not dsBUSisterSites Is Nothing Then
                If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
                    For Each dr As DataRow In dsBUSisterSites.Tables(0).Rows
                        Dim strGetUsers As String = "SELECT * FROM SDIX_USERS_TBL WHERE BUSINESS_UNIT = '" & dr.Item("BUSINESS_UNIT") & "' AND ISA_SDI_EMPLOYEE = '" & strType & "' AND ISA_EMPLOYEE_ACTYP = '" & strRole & "' AND ACTIVE_STATUS = 'A'"
                    Next
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub SelectBaseBU(ByVal BaseBU As String)
        Try
            For Each checkedItem As RadComboBoxItem In rcbMultiSelect.Items
                If checkedItem.Value = BaseBU Then
                    checkedItem.Checked = True
                    checkedItem.Enabled = False
                    Exit For
                Else
                    checkedItem.Enabled = True
                End If
            Next
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub MultiSiteChk_CheckedChanged(sender As Object, e As EventArgs)
        Try
            rcbMultiSelect.ID = "rcbMultiSelect"
            rcbMultiSelect.CheckBoxes = True
            rcbMultiSelect.EnableCheckAllItemsCheckBox = True
            rfvMultiBU.ID = "rfvMultiBU"
            rcbMultiSelect.CssClass = "MultiselectChkbox"
            rfvMultiBU.ControlToValidate = "rcbMultiSelect"
            rfvMultiBU.ErrorMessage = "Select BU for Multi Site Access"
            rfvMultiBU.CssClass = "usergroup-field"
            PLMultiSelect.Controls.Add(rcbMultiSelect)

            If MultiSiteChk.Checked Then
                If Request.QueryString("MEXICO") = "YES" Then
                    drpBUnit.Visible = False
                    lblBusUnit.Visible = False
                    'Dim BU As String = dropSelectUser.SelectedItem.Text.Split("-")(1).Trim()
                    Dim BU As String = rcbdropSelectUser.SelectedItem.Text.Split("-")(1).Trim()
                    GetMultiBusinessUnit(BU)
                    SelectBaseBU(BU)
                Else
                    If Session("USERTYPEVALUE") = "C" Or Session("USERTYPEVALUE") = "S" Then
                        If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                            GetMultiBusinessUnit(txtGroupID.Text)
                            SelectBaseBU(txtGroupID.Text)
                        ElseIf Session("ROLE") = "USER" Then
                            GetMultiBusinessUnit(txtGroupID.Text)
                            SelectBaseBU(txtGroupID.Text)
                        Else
                            GetMultiBusinessUnit(rcbGroup.SelectedValue)
                            SelectBaseBU(rcbGroup.SelectedValue)
                        End If
                    Else
                        GetMultiBusinessUnit(rcbGroup.SelectedValue)
                        SelectBaseBU(rcbGroup.SelectedValue)
                    End If
                End If
            Else
                If Request.QueryString("MEXICO") = "YES" Then
                    rcbMultiSelect.Visible = False
                    rcbMultiSelect.ClearCheckedItems()
                Else
                    'rcbGroup.Visible = True
                    'valGroup.Visible = True
                    rcbMultiSelect.Visible = False
                    rcbMultiSelect.ClearCheckedItems()

                End If

            End If
            If Session("USERTYPE") = "SUPER" Then
                If Page_Action = "EDIT" Then
                    If radioUserType.SelectedValue = "S" Then
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        lblDept.Visible = True
                    ElseIf radioUserType.SelectedValue = "V" Then
                        lblSelectUser.Visible = True
                        Label_VendorID.Visible = True
                        tr_BU_unit_field.Style.Remove("display")
                        drpBUnit.Visible = True
                        lblBusUnit.Visible = True
                        Radcombobox_vendorID()
                        'RadcomboforVendorID.Visible = True
                        MainvndrID.Visible = True
                        lblGroup.Visible = False
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = False
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        lblDept.Visible = False
                    Else
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        lblDept.Visible = False
                    End If
                Else

                End If
            ElseIf Session("USERTYPE") = "ADMIN" Then
                If Session("USERTYPEVALUE") = "S" Then
                    drpBUnit.Visible = False
                    lblBusUnit.Visible = False
                    lblSelectUser.Visible = True
                    'dropSelectUser.Visible = True
                    rcbdropSelectUser.Visible = True
                    lblGroup.Visible = True
                    btnAdd.Visible = True
                    lblPassword.Visible = False
                    lblConfirm.Visible = False
                    radioUserType.Visible = False
                    Label_usrtype.Visible = False
                    tr_PwdFields.Style.Add("display", "none")
                    tr_CpwdFields.Style.Add("display", "none")
                    tr_BU_unit_field.Style.Add("display", "none")
                    MultiSiteChk.Visible = True
                    rcbGroup.Visible = False
                    If Page_Action = "ADD" Then
                        lblGroup.Visible = True
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = True
                        MainvndrID.Visible = False
                        radioUserType.SelectedValue = "C"
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        radioUserType.Enabled = False
                        txtUserid.Enabled = False
                        lblGroup.Visible = True
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = True
                        lblBusUnit.Visible = False
                        drpBUnit.Visible = False
                        roleDropdownList.Enabled = True
                        radioUserType.Visible = False
                        Label_usrtype.Visible = False
                        If Session("Flag_AddUser") = "V" Then
                            txtUserid.ReadOnly = False
                            txtUserid.BackColor = White
                            txtUserid.Enabled = True
                        End If
                        lblDept.Visible = False
                        rcbDept.Visible = False
                        tr_Dept_details_fields.Style.Add("display", "none")
                        tr_PwdFields.Style.Remove("display")
                        tr_CpwdFields.Style.Remove("display")
                        lblPassword.Visible = True
                        txtPassword.Visible = True
                        lblConfirm.Visible = True
                        txtConfirm.Visible = True
                        btnAdd.Visible = False
                        btnEdit.Visible = True
                    Else

                    End If
                ElseIf Session("USERTYPEVALUE") = "C" Then
                    If Page_Action = "EDIT" Then
                        CustomersUsers()
                    Else
                        lblGroup.Visible = True
                        rcbGroup.Visible = False
                    End If
                End If
            ElseIf Session("USERTYPE") = "USER" Then
                If radioUserType.SelectedValue = "S" Then
                    SDIUsers()
                ElseIf radioUserType.SelectedValue = "C" Then
                    CustomersUsers()
                End If
            End If

            If btnChangePassw.Visible = True Then
                lblPassword.Visible = False
                lblConfirm.Visible = False
            Else
                lblPassword.Visible = True
                lblConfirm.Visible = True
            End If

        Catch ex As Exception

        End Try
    End Sub

    Protected Sub btnZuseSubmit_Click(sender As Object, e As EventArgs)

        Dim rowAffected As Integer
        Dim UserName(1) As String
        lblZuseError.Text = ""
        UserName = Session("USERNAME").ToString().Split(",")
        Try
            If cbxZeus.Checked Then
                cbxZeus.Checked = False
                If lblZeus.Text = "Disable Zeus" Then
                    Dim StrQury As String = "DELETE FROM SDIX_QLIK_USERS WHERE USERID= '" & Trim(txtUserid.Text).ToUpper & "'"
                    rowAffected = ORDBData.ExecNonQuery(StrQury, False)
                    lblZeus.Text = "Enable Zeus"
                Else
                    Dim StrQury As String = "INSERT INTO SDIX_QLIK_USERS VALUES ('" & Trim(txtUserid.Text).ToUpper & "','" & UserName(1).ToString & " " & UserName(0).ToString & "')"
                    rowAffected = ORDBData.ExecNonQuery(StrQury, False)
                    lblZeus.Text = "Disable Zeus"
                End If

                If rowAffected > 0 Then
                    lblZuseError.Text = "Successfully Updated"
                    lblZuseError.ForeColor = Green
                Else
                    lblZuseError.Text = "Update Failed"
                    lblZuseError.ForeColor = Red
                End If

            Else
                lblZuseError.Text = "Please Select the Option"
                lblZuseError.ForeColor = Red
            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Function LoadZuesValues()
        Dim Name As String = ""
        lblZuseError.Text = ""
        cbxZeus.Checked = False
        Try
            Dim StrQury As String = "SELECT * FROM SDIX_QLIK_USERS WHERE USERID= '" & Trim(txtUserid.Text).ToUpper & "'"
            Name = ORDBData.GetScalar(StrQury)
            If Not Name = "" Then
                lblZeus.Text = "Disable Zeus"
                cbxZeus.ToolTip = "Disable Zeus"
            Else
                lblZeus.Text = "Enable Zeus"
                cbxZeus.ToolTip = "Enable Zeus"
            End If
        Catch ex As Exception

        End Try
    End Function

    Private Sub btnEmplActivateAccount_Click(sender As Object, e As EventArgs) Handles btnEmplActivateAccount.Click
        Dim strSQLstring As String = ""
        Dim iRowsAffected As Integer = 0
        Dim strUserId As String = ""
        Dim strCurrBU As String = ""

        ' Update Employee table based on User Id and Business Unit selected
        Try
            strCurrBU = lblCurrBUHide.Text
            If Trim(strCurrBU) = "" Or Trim(strCurrBU) = "0" Then
                'error
                lblDBError.Text = "There was an issue trying to activate this Employee account."
                lblDBError.Visible = True
                Exit Sub
            End If
            'strUserId = dropSelectUser.SelectedValue
            strUserId = rcbdropSelectUser.SelectedValue
            If Trim(strUserId) = "" Then
                'error
                lblDBError.Text = "There was an issue trying to activate this Employee account."
                lblDBError.Visible = True
                Exit Sub
            End If
            'update code
            strSQLstring = "UPDATE PS_ISA_EMPL_TBL " & vbCrLf & _
                " SET EFF_STATUS = '" & clsUserTbl.EmplActiveStatus_Active & "' " & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & strUserId & "' AND BUSINESS_UNIT = '" & strCurrBU & "' " & vbCrLf & _
                " AND EFF_STATUS = '" & lblEmplActiveStatusHide.Text & "' "

            iRowsAffected = ORDBData.ExecNonQuery(strSQLstring, False)
            If iRowsAffected = 1 Then
                btnEmplActivateAccount.Visible = False
                btnEmplInactivateAccount.Visible = True
                lblEmplAccountDisabled.Text = "This Employee account is now active."
                lblEmplAccountDisabled.Visible = True
                'create audit record
                clsSDIAudit.AddRecord("Profile.aspx", "Activate Employee", "SDIX_USERS_TBL", Session("USERID").ToString, strCurrBU, strUserId, sColumnChg:="EFF_STATUS", _
                      sOldValue:=clsUserTbl.EmplActiveStatus_Inactive, sNewValue:=clsUserTbl.EmplActiveStatus_Active)

                lblEmplActiveStatusHide.Text = clsUserTbl.EmplActiveStatus_Active
            Else
                'error updating
                lblDBError.Text = "There was an issue trying to activate this Employee account."
                lblDBError.Visible = True
                Exit Sub
            End If
            FieldsPosition()
        Catch ex As Exception
            lblDBError.Text = "There was an issue trying to activate this Employee account."
            lblDBError.Visible = True
            ' send error email
        End Try
    End Sub

    Private Sub btnEmplInactivateAccount_Click(sender As Object, e As EventArgs) Handles btnEmplInactivateAccount.Click
        Dim strSQLstring As String = ""
        Dim iRowsAffected As Integer = 0
        Dim strUserId As String = ""
        Dim strCurrBU As String = ""

        ' Update Employee table based on User Id and Business Unit selected
        Try
            strCurrBU = lblCurrBUHide.Text
            If Trim(strCurrBU) = "" Or Trim(strCurrBU) = "0" Then
                'error
                lblDBError.Text = "There was an issue trying to inactivate this Employee account."
                lblDBError.Visible = True
                Exit Sub
            End If
            'strUserId = dropSelectUser.SelectedValue
            strUserId = rcbdropSelectUser.SelectedValue
            If Trim(strUserId) = "" Then
                'error
                lblDBError.Text = "There was an issue trying to inactivate this Employee account."
                lblDBError.Visible = True
                Exit Sub
            End If
            'update code
            strSQLstring = "UPDATE PS_ISA_EMPL_TBL " & vbCrLf & _
                " SET EFF_STATUS = '" & clsUserTbl.EmplActiveStatus_Inactive & "' " & vbCrLf & _
                " WHERE ISA_EMPLOYEE_ID = '" & strUserId & "' AND BUSINESS_UNIT = '" & strCurrBU & "' " & vbCrLf & _
                " AND EFF_STATUS = '" & lblEmplActiveStatusHide.Text & "' "

            iRowsAffected = ORDBData.ExecNonQuery(strSQLstring, False)
            If iRowsAffected = 1 Then
                btnEmplActivateAccount.Visible = True
                btnEmplInactivateAccount.Visible = False
                lblEmplAccountDisabled.Text = "This Employee account is now NOT active."
                lblEmplAccountDisabled.Visible = True
                'create audit record
                clsSDIAudit.AddRecord("Profile.aspx", "Inactivate Employee", "PS_ISA_EMPL_TBL", Session("USERID").ToString, strCurrBU, strUserId, sColumnChg:="EFF_STATUS", _
                      sOldValue:=clsUserTbl.EmplActiveStatus_Active, sNewValue:=clsUserTbl.EmplActiveStatus_Inactive)

                lblEmplActiveStatusHide.Text = clsUserTbl.EmplActiveStatus_Inactive
            Else
                'error updating
                lblDBError.Text = "There was an issue trying to inactivate this Employee account."
                lblDBError.Visible = True
                Exit Sub
            End If
            FieldsPosition()
        Catch ex As Exception
            lblDBError.Text = "There was an issue trying to inactivate this Employee account."
            lblDBError.Visible = True
            ' send error email
            Dim strSubject As String = "Error in Profile.aspx"
        End Try
    End Sub

    Protected Sub radioUserType_SelectedIndexChanged(sender As Object, e As EventArgs)
        '' ErrorMsgforRoleField.Visible = False
        lblMessage.Text = ""
        'lblMessage.Visible = False
        'Error_LabelFname.Text = ""
        'Error_LabelLname.Text = ""
        lblSelectUser.Visible = True
        Error_VendorID.Text = ""
        Error_LabelSite.Text = ""
        'buildSelectDropDown()
        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
        If Trim(Page_Action) = "ADD" Then
            lblSelectUser.Visible = False
            'dropSelectUser.Visible = False
            rcbdropSelectUser.Visible = False
            'TextBox_VendorId.Visible = False
            Label_VendorID.Visible = False
            lblPassword.Visible = True
            lblConfirm.Visible = True
            'tr_BU_unit_field.Style.Add("display", "none")
            tr_Site_details_fields.Style.Add("display", "none")

            If radioUserType.SelectedValue = "V" Then
                tr_Multiselect_fields.Style.Add("display", "none")
                'TextBox_VendorId.Visible = True
                Label_VendorID.Visible = True
                tr_BU_unit_field.Style.Remove("display")
                drpBUnit.Visible = True
                lblBusUnit.Visible = True
                Radcombobox_vendorID()
                MainvndrID.Visible = True
                lblGroup.Visible = False
                rcbGroup.Visible = False
                MultiSiteChk.Visible = False
                roleDropdownList.Items.FindByValue("SUPER").Enabled = False
            ElseIf radioUserType.SelectedValue = "C" Then
                'RadcomboforVendorID.Visible = False
                MainvndrID.Visible = False
                'TextBox_VendorId.Visible = False
                Label_VendorID.Visible = False
                drpBUnit.Visible = False
                lblBusUnit.Visible = False
                tr_Site_details_fields.Style.Remove("display")
                tr_Multiselect_fields.Style.Remove("display")
                tr_BU_unit_field.Style.Add("display", "none")
                lblGroup.Visible = True
                rcbGroup.Visible = True
                MultiSiteChk.Visible = True
                roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                Error_VendorID.Visible = False
                Error_VendorID.Text = ""
                buildGroupList(rcbGroup)
                Dim Site_Flag As String = GetFlagValue(rcbGroup.SelectedValue)
                If Site_Flag = "V" Then
                    txtUserid.ReadOnly = False
                    txtUserid.BackColor = White
                    txtUserid.Enabled = False
                End If
            Else
                tr_BU_unit_field.Style.Add("display", "none")
                'RadcomboforVendorID.Visible = False
                MainvndrID.Visible = False
                'TextBox_VendorId.Visible = False
                Label_VendorID.Visible = False
                drpBUnit.Visible = False
                lblBusUnit.Visible = False
                lblGroup.Visible = True
                rcbGroup.Visible = True
                roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                Error_VendorID.Visible = False
                Error_VendorID.Text = ""
                tr_Multiselect_fields.Style.Remove("display")
                MultiSiteChk.Visible = True
                tr_Site_details_fields.Style.Remove("display")
                buildGroupList(rcbGroup)
                Dim Site_Flag As String = GetFlagValue(rcbGroup.SelectedValue)
                If Site_Flag = "V" Then
                    txtUserid.ReadOnly = False
                    txtUserid.BackColor = White
                    txtUserid.Enabled = False
                End If
            End If
            btnAdd.Visible = False
            btnEdit.Visible = True
        ElseIf Trim(Page_Action) = "EDIT" Then
            Session("PageAction") = "EDIT"
            If radioUserType.SelectedValue = "V" Then
                Radcombobox_vendorID()
                'RadcomboforVendorID.Visible = True
                MainvndrID.Visible = True
                rcbGroup.Visible = False
                tr_BU_unit_field.Style.Remove("display")
                drpBUnit.Visible = True
                lblBusUnit.Visible = True
                lblGroup.Visible = False
                Label_VendorID.Visible = True
                roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                rcbGroup.Visible = False
            Else
                'RadcomboforVendorID.Visible = False
                MainvndrID.Visible = False
                rcbGroup.Visible = True
                tr_BU_unit_field.Style.Add("display", "none")
                drpBUnit.Visible = False
                lblBusUnit.Visible = False
                tr_Site_details_fields.Style.Remove("display")
                Label_VendorID.Visible = False
            End If

            If radioUserType.SelectedValue = "C" Then

                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                roleDropdownList.Items.FindByValue("SUPER").Enabled = False
            ElseIf radioUserType.SelectedValue = "S" Then
                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                roleDropdownList.Items.FindByValue("SUPER").Enabled = True
            End If

            If Session("USERTYPE") = "SUPER" Then
                If Page_Action = "EDIT" Then
                    If radioUserType.SelectedValue = "S" Then
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        lblDept.Visible = True
                    ElseIf radioUserType.SelectedValue = "V" Then
                        lblSelectUser.Visible = True
                        Label_VendorID.Visible = True
                        tr_BU_unit_field.Style.Remove("display")
                        drpBUnit.Visible = True
                        lblBusUnit.Visible = True
                        Radcombobox_vendorID()
                        'RadcomboforVendorID.Visible = True
                        MainvndrID.Visible = True
                        lblGroup.Visible = False
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = False
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        rcbMultiSelect.Visible = False
                    Else
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        lblDept.Visible = False
                    End If
                Else

                End If
            End If

            lblPassword.Visible = False
            lblConfirm.Visible = False
            radioUserType.SelectedValue = Session("SetClickValue_Usertype")
            buildGroupList(rcbGroup)
            Dim SQLStringQuery As String = "SELECT BUSINESS_UNIT, ISA_SDI_EMPLOYEE FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID='" & Trim(txtUserid.Text) & "'"
            Dim dsbuvalue As DataSet = ORDBData.GetAdapter(SQLStringQuery)
            If Convert.ToString(dsbuvalue.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")) <> "V" Then
                Try
                    rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(dsbuvalue.Tables(0).Rows(0).Item("BUSINESS_UNIT")).Index
                Catch ex As Exception

                End Try
            End If
            buildSelectDropDown()
            'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
            Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))
        Else

        End If

        If Page_Action = "ADD" Then
            If radioUserType.SelectedValue = "S" Then
                lblDept.Visible = True
                rcbDept.Visible = True
                tr_Dept_details_fields.Style.Remove("display")
            Else
                lblDept.Visible = False
                rcbDept.Visible = False
                tr_Dept_details_fields.Style.Add("display", "none")
            End If
            If Session("ROLE") = "SUPER" Then
                If radioUserType.SelectedValue = "S" Then
                    MultiSiteChk.Visible = True
                End If
            End If
        Else
            If radioUserType.SelectedValue = "S" Then
                lblDept.Visible = True
                rcbDept.Visible = True
                tr_Dept_details_fields.Style.Remove("display")
            Else
                lblDept.Visible = False
                rcbDept.Visible = False
                tr_Dept_details_fields.Style.Add("display", "none")
            End If
        End If
        If roleDropdownList.Items.FindByValue("CORPADMIN").Enabled Then
            MultiSiteChk.Visible = False
            PLMultiSelect.Visible = False
        End If

        If radioUserType.SelectedValue = "V" Or roleDropdownList.SelectedValue = "CORPADMIN" Then
            MultiSiteChk.Visible = False
            PLMultiSelect.Visible = False
        Else
            MultiSiteChk.Visible = True
            PLMultiSelect.Visible = True
        End If
    End Sub

    Private Sub GetUserValues()
        Try
            Dim SDIUserId As String = Session("USERID")
            Dim SQLSTRINGQuery As String = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & SDIUserId & "'"
            Dim dsOREmp As DataSet = ORDBData.GetAdapter(SQLSTRINGQuery)

            If dsOREmp.Tables(0).Rows.Count() > 0 Then
                Session("ROLE") = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP").ToString.ToUpper
                Session("USERTYPEVALUE") = dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
                Session("LoginedInUser_BU") = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
            End If

            Dim SQLAutoFlagQuery As String = "SELECT AUTO_ASSIGN_FLG FROM PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT = '" & Session("LoginedInUser_BU") & "'"
            Dim dsFlagBU As DataSet = ORDBData.GetAdapter(SQLAutoFlagQuery)
            Session("Flag_AddUser") = dsFlagBU.Tables(0).Rows(0).Item("AUTO_ASSIGN_FLG")
        Catch ex As Exception

        End Try
    End Sub

    Private Function GetUserType(ByVal struserid As String) As String
        Try
            Dim get_Usertype As String = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & struserid & "'"
            Dim dsOREmp As DataSet = ORDBData.GetAdapter(get_Usertype)

            If dsOREmp.Tables(0).Rows.Count() > 0 Then
                Return Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP"))
            Else
                Return ""
            End If
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Public Function Get_UserValues(ByVal struserid As String, ByVal strColumn As String) As String
        Try
            Dim get_Usertype As String = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & struserid & "'"
            Dim dsOREmp As DataSet = ORDBData.GetAdapter(get_Usertype)

            If dsOREmp.Tables(0).Rows.Count() > 0 Then
                Return Convert.ToString(dsOREmp.Tables(0).Rows(0).Item(strColumn))
            Else
                Return ""
            End If
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function Get_RoleValues(ByVal strroleid As String) As String
        Try
            Dim get_Usertype As String = "SELECT * FROM SDIX_USRROLE_TBL WHERE rolenum = " + strroleid + ""
            Dim dsOREmp As DataSet = ORDBData.GetAdapter(get_Usertype)

            If dsOREmp.Tables(0).Rows.Count() > 0 Then
                Return Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ROLENAME"))
            Else
                Return ""
            End If
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Function Get_RoleExist(ByVal strRoleID As String, ByVal strBU As String) As Boolean
        Dim strBoolvalue As Boolean = False
        Try
            Dim get_Usertype As String = "SELECT * FROM SDIX_USRROLE_TBL WHERE UPPER(ROLENAME) = (SELECT UPPER(ROLENAME) FROM SDIX_USRROLE_TBL WHERE ROLENUM = " + strRoleID + ") AND BUSINESS_UNIT = '" + strBU + "'"
            Dim dsOREmp As DataSet = ORDBData.GetAdapter(get_Usertype)

            If Not dsOREmp Is Nothing Then
                If dsOREmp.Tables(0).Rows.Count() > 0 Then
                    strBoolvalue = True
                End If
            End If
        Catch ex As Exception

        End Try
        Return strBoolvalue
    End Function

    Private Function DeleteUserPriv(ByVal struserid As String, ByVal strBU As String) As String
        Try
            Dim get_Usertype As String = "Delete from SDIX_USERS_PRIVS where ISA_EMPLOYEE_ID = '" + struserid.ToUpper() + "' AND BUSINESS_UNIT = '" + strBU + "' AND ISA_IOL_OP_NAME != 'EMLRECVD'"
            Dim retVal As Integer = 0
            retVal = ORDBData.ExecNonQueryWithTransaction(get_Usertype)
        Catch ex As Exception
            Return ""
        End Try
    End Function


    Private Sub SDIUsers()
        lblDept.Visible = True
        rcbDept.Visible = True

        If Session("ROLE") = "ADMIN" Then
            drpBUnit.Visible = False
            lblBusUnit.Visible = False
            lblSelectUser.Visible = True
            'dropSelectUser.Visible = True
            rcbdropSelectUser.Visible = True
            lblGroup.Visible = True
            btnAdd.Visible = True
            lblPassword.Visible = False
            lblConfirm.Visible = False
            radioUserType.Visible = False
            Label_usrtype.Visible = False
            tr_PwdFields.Style.Add("display", "none")
            tr_CpwdFields.Style.Add("display", "none")
            tr_BU_unit_field.Style.Add("display", "none")
            MultiSiteChk.Visible = True
            rcbGroup.Visible = False
            If Page_Action = "ADD" Then
                lblGroup.Visible = True
                rcbGroup.Visible = False
                MultiSiteChk.Visible = True
            End If
        ElseIf Session("ROLE") = "USER" Then

            lblSelectUser.Visible = False
            'TextBox_VendorId.Visible = False
            lblBusUnit.Visible = False
            drpBUnit.Visible = False
            lblSelectUser.Visible = False
            'dropSelectUser.Visible = False
            rcbdropSelectUser.Visible = False
            Label_usrtype.Visible = False
            radioUserType.Visible = False
            lblGroup.Visible = True
            btnAdd.Visible = False
            lblPassword.Visible = False
            lblConfirm.Visible = False
            rcbGroup.Visible = False
            tr_PwdFields.Style.Add("display", "none")
            tr_CpwdFields.Style.Add("display", "none")
            tr_BU_unit_field.Style.Add("display", "none")
            tr_selectuser_fields.Style.Add("display", "none")
            ''tr_Multiselect_fields.Style.Add("display", "none")
            MultiSiteChk.Visible = True
        Else '------Super
            tr_PwdFields.Style.Add("display", "none")
            tr_CpwdFields.Style.Add("display", "none")
            tr_Site_details_fields.Style.Add("display", "none")
            ''tr_Multiselect_fields.Style.Add("display", "none")
            tr_BU_unit_field.Style.Add("display", "none")
            'dropSelectUser.Visible = True
            rcbdropSelectUser.Visible = True
            btnAdd.Visible = True
            txtGroup.Visible = False
            txtGroupID.Visible = False
            txtGroup.EnableViewState = False
            txtGroupID.EnableViewState = False
            MultiSiteChk.Visible = True
            If radioUserType.SelectedValue = "V" Then
                'Radcombobox_vendorID()
                'RadcomboforVendorID.Visible = True
                MainvndrID.Visible = True
                tr_BU_unit_field.Style.Remove("display")
                lblGroup.Visible = False
                'TextBox_VendorId.Visible = True
                'TextBox_VendorId.Enabled = True
                Label_VendorID.Visible = True
                lblBusUnit.Visible = True
                drpBUnit.Visible = True
                drpuserAp.Visible = False
                lblGroup.Visible = False
                MultiSiteChk.Visible = False

                rcbMultiSelect.Visible = False
                rcbGroupTab2.Visible = False
                rcbGroup.Visible = False
                txtGroup.Visible = False
                txtGroupID.Visible = False
                roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                buildSelectDropDown()
                'rcbGroupSites.Visible = False
                lblDept.Visible = False
                rcbDept.Visible = False
                'tr_Dept_details_fields.Style.Add("display", "none")
            ElseIf radioUserType.SelectedValue = "C" Then
                'RadcomboforVendorID.Visible = False
                MainvndrID.Visible = False
                tr_Site_details_fields.Style.Remove("display")
                tr_Multiselect_fields.Style.Remove("display")
                lblGroup.Visible = True
                lblPassword.Visible = False
                lblConfirm.Visible = False
                'TextBox_VendorId.Visible = False
                Label_VendorID.Visible = False
                roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                MultiSiteChk.Visible = True
                'rcbGroupSites.Visible = True
                rcbGroup.Visible = True
                lblDept.Visible = False
                rcbDept.Visible = False
                'tr_Dept_details_fields.Style.Add("display", "none")
            ElseIf radioUserType.SelectedValue = "S" Then
                'RadcomboforVendorID.Visible = False
                MainvndrID.Visible = False
                tr_Site_details_fields.Style.Remove("display")
                lblGroup.Visible = True
                lblPassword.Visible = False
                lblConfirm.Visible = False
                'TextBox_VendorId.Visible = False
                'TextBox_VendorId.Enabled = False
                Label_VendorID.Visible = False
                lblGroup.Visible = True
                rcbGroup.Visible = True
                rcbMultiSelect.Visible = False
                rcbGroupTab2.Visible = False
                MultiSiteChk.Visible = False
                roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                buildSelectDropDown()
                'rcbGroupSites.Visible = True
                lblDept.Visible = True
                rcbDept.Visible = True
            End If
            lblSelectUser.Visible = True
            'dropSelectUser.Visible = True
            rcbdropSelectUser.Visible = True
            btnAdd.Visible = True

            If roleDropdownList.SelectedValue = "CORPADMIN" Then
                MultiSiteChk.Visible = False
                PLGroup.Visible = False
            Else
                MultiSiteChk.Visible = True
                PLGroup.Visible = True
            End If
        End If
        txtUserid.ReadOnly = True
        txtUserid.BackColor = LightGray
        valuserid1.Enabled = False
        valUserid2.Enabled = False
        If Session("ROLE") <> "USERS" Then
            'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
            Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))
        End If

        'buildSelectDropDown()
    End Sub

    Private Sub CustomersUsers()
        lblDept.Visible = False
        rcbDept.Visible = False
        'tr_Dept_details_fields.Style.Add("display", "none")
        tr_PwdFields.Style.Add("display", "none")
        tr_CpwdFields.Style.Add("display", "none")
        tr_BU_unit_field.Style.Add("display", "none")
        tr_Multiselect_fields.Style.Add("display", "none")
        tr_selectuser_fields.Style.Add("display", "none")
        'tr_vendorId_fields.Style.Add("display", "none")
        'RadcomboforVendorID.Visible = False
        MainvndrID.Visible = False
        If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
            tr_selectuser_fields.Style.Remove("display")
            'tr_Multiselect_fields.Style.Remove("display")
            lblSelectUser.Visible = True
            'dropSelectUser.Visible = True
            rcbdropSelectUser.Visible = True
            radioUserType.Enabled = False
            roleDropdownList.Enabled = True
            radioUserType.Visible = True
            roleDropdownList.Visible = True
            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
            lblPassword.Visible = False
            lblConfirm.Visible = False
            radioUserType.Visible = False
            Label_usrtype.Visible = False
            rcbGroup.Visible = False
            tr_Multiselect_fields.Style.Remove("display")
            MultiSiteChk.Visible = True
            lblGroup.Visible = True
            drpBUnit.Visible = False
            lblBusUnit.Visible = False
            If MultiSiteChk.Checked = True Then
                rcbMultiSelect.Enabled = True
                rcbMultiSelect.Visible = True
            Else
                rcbMultiSelect.Enabled = False
                rcbMultiSelect.Visible = False
            End If
            buildSelectDropDown()
            Dim selecteuserdrpdwn As String = Session("SelectUserDrpdwn")
            Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Session("SelectUserDrpdwn")))
            btnAdd.Visible = True
        ElseIf Session("ROLE") = "USER" Then
            radioUserType.Enabled = False
            roleDropdownList.Enabled = False
            radioUserType.Visible = True
            roleDropdownList.Visible = True
            rcbGroup.Visible = False
            MultiSiteChk.Enabled = True
            lblGroup.Visible = True
            lblSelectUser.Visible = False
            'dropSelectUser.Visible = False
            rcbdropSelectUser.Visible = False
            drpBUnit.Visible = False
            lblBusUnit.Visible = False
            lblPassword.Visible = False
            lblConfirm.Visible = False
            radioUserType.Visible = False
            Label_usrtype.Visible = False
            'buildSelectDropDown()
            btnAdd.Visible = False
            btnEdit.Visible = False
            tr_Multiselect_fields.Style.Remove("display")
        End If
    End Sub

    Private Sub Radcombobox_vendorID()
        Try
            Dim strSQLString As String

            strSQLString = "SELECT DISTINCT ISA_VENDOR_ID FROM SDIX_USERS_TBL where ISA_SDI_EMPLOYEE = 'V' and ISA_VENDOR_ID is not null"

            Dim dsDQLGroups As DataSet = ORDBData.GetAdapter(strSQLString)
            'RadcomboforVendorID.DataSource = dsDQLGroups
            'RadcomboforVendorID.DataValueField = "ISA_VENDOR_ID"
            'RadcomboforVendorID.DataTextField = "ISA_VENDOR_ID"
            'RadcomboforVendorID.DataBind()
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub rcbGroup_SelectedIndexChanged(sender As Object, e As RadComboBoxSelectedIndexChangedEventArgs)

        Try
            Dim FlagValue As String = GetFlagValue(rcbGroup.SelectedValue)
            If Page_Action = "ADD" Then
                FlagValue = GetFlagValue(rcbGroup.SelectedValue)
                If FlagValue = "V" Then
                    txtUserid.Enabled = True
                    txtUserid.ReadOnly = False
                    txtUserid.BackColor = White
                Else
                    txtUserid.Text = ""
                    txtUserid.Enabled = False
                    txtUserid.ReadOnly = True
                    txtUserid.BackColor = LightGray
                End If
            End If

            If Session("USERTYPEVALUE") = "C" Then
                If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                    If Page_Action = "EDIT" Then
                        'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
                        Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))
                    End If
                End If
            End If
            If Session("USERTYPE") = "CORPADMIN" Then
                If Page_Action = "ADD" Then
                    Dim strValue As String = rcbGroup.SelectedValue
                    GetSisterSitesBU(Session("BUSUNIT"))
                    rcbGroup.SelectedValue = strValue
                Else
                    If rcbdropSelectUser.SelectedValue <> "" Then
                        Dim strusertype As String = ""
                        strusertype = GetUserType(rcbdropSelectUser.SelectedValue)
                        If strusertype <> "CORPADMIN" Then
                            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        End If
                    End If
                End If
            End If
            If Session("USERTYPEVALUE") = "S" Then
                If radioUserType.SelectedValue = "S" Or Session("USERTYPE") = "ADMIN" Or Session("USERTYPE") = "USER" Then
                    lblDept.Visible = True
                    rcbDept.Visible = True
                    'tr_Dept_details_fields.Style.Remove("display")
                Else
                    lblDept.Visible = False
                    rcbDept.Visible = False
                    'tr_Dept_details_fields.Style.Add("display", "none")
                End If
            Else
                lblDept.Visible = False
                rcbDept.Visible = False
                'tr_Dept_details_fields.Style.Add("display", "none")
            End If
            If Session("USERTYPE") = "SUPER" Then
                If Page_Action = "EDIT" Then
                    If radioUserType.SelectedValue = "S" Then
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    ElseIf radioUserType.SelectedValue = "V" Then
                        lblSelectUser.Visible = True
                        Label_VendorID.Visible = True
                        tr_BU_unit_field.Style.Remove("display")
                        drpBUnit.Visible = True
                        lblBusUnit.Visible = True
                        Radcombobox_vendorID()
                        'RadcomboforVendorID.Visible = True
                        MainvndrID.Visible = True
                        lblGroup.Visible = False
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = False
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        rcbMultiSelect.Visible = False
                    Else
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    End If
                Else

                End If
            ElseIf Session("USERTYPE") = "ADMIN" Then
                If Session("USERTYPEVALUE") = "C" Then
                    If Page_Action = "EDIT" Then
                        CustomersUsers()
                    Else

                    End If

                End If
            End If
            If btnChangePassw.Visible = True Then
                lblPassword.Visible = False
                lblConfirm.Visible = False
            Else
                lblPassword.Visible = True
                lblConfirm.Visible = True
            End If

            If Page_Action = "EDIT" Then
                If rcbGroup.SelectedValue <> "" Or rcbGroup.SelectedValue <> "0" Then
                    Error_LabelSite.Text = ""
                End If
            End If
        Catch ex As Exception

        End Try

    End Sub

    Private Function GetFlagValue(ByVal SiteValue As String) As String
        Try
            Dim SQLAutoFlagQuery As String
            Dim FlagValues As String
            SQLAutoFlagQuery = "SELECT AUTO_ASSIGN_FLG FROM PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT = '" & Trim(rcbGroup.SelectedValue) & "'"
            Dim dsFlagBU As DataSet = ORDBData.GetAdapter(SQLAutoFlagQuery)
            FlagValues = dsFlagBU.Tables(0).Rows(0).Item("AUTO_ASSIGN_FLG")
            Return FlagValues
        Catch ex As Exception

        End Try

    End Function

    Private Function Auto_UserIdGenerate_Flags() As String

        Try
            Dim AutoGenerateUserID As String
            Dim strFirst As String = Trim(txtFirst.Text)
            strFirst = Replace(strFirst, "'", "")
            Dim strLast As String = Trim(txtLast.Text)
            strLast = Replace(strLast, "'", "")
            Dim FirstPart_UserID As String
            Dim SecondPart_UserID As String

            If strFirst.Length > 3 And strLast.Length > 3 Then
                FirstPart_UserID = strFirst.Substring(0, 3)
                SecondPart_UserID = strLast.Substring(0, 3)
            ElseIf strFirst.Length < 3 And strLast.Length < 3 Then
                FirstPart_UserID = strFirst.Substring(0, strFirst.Length)
                SecondPart_UserID = strLast.Substring(0, strLast.Length)
            ElseIf strFirst.Length > 3 Or strLast.Length < 3 Then
                SecondPart_UserID = strLast.Substring(0, strLast.Length)
                If strFirst.Length <> 3 Then
                    If strLast.Length = 1 Then
                        FirstPart_UserID = strFirst.Substring(0, 5)
                    ElseIf strLast.Length = 2 Then
                        FirstPart_UserID = strFirst.Substring(0, 4)
                    ElseIf strLast.Length = 3 Then
                        FirstPart_UserID = strFirst.Substring(0, 3)
                    End If
                Else
                    FirstPart_UserID = strFirst.Substring(0, strFirst.Length)
                End If
            ElseIf strFirst.Length < 3 Or strLast.Length > 3 Then
                FirstPart_UserID = strFirst.Substring(0, strFirst.Length)
                If strLast.Length <> 3 Then
                    If strFirst.Length = 1 Then
                        SecondPart_UserID = strLast.Substring(0, 5)
                    ElseIf strFirst.Length = 2 Then
                        SecondPart_UserID = strLast.Substring(0, 4)
                    ElseIf strFirst.Length = 3 Then
                        SecondPart_UserID = strLast.Substring(0, 3)
                    End If
                Else
                    SecondPart_UserID = strLast.Substring(0, strLast.Length)
                End If
            Else
                FirstPart_UserID = strFirst.Substring(0, 3)
                SecondPart_UserID = strLast.Substring(0, 3)
            End If

            Dim usrid As String = FirstPart_UserID + SecondPart_UserID

            If usrid.Length < 6 Then
                If usrid.Length = 5 Then
                    usrid = usrid + "0"
                ElseIf usrid.Length = 4 Then
                    usrid = usrid + "00"
                ElseIf usrid.Length = 3 Then
                    usrid = usrid + "000"
                ElseIf usrid.Length = 2 Then
                    usrid = usrid + "0000"
                End If
            End If

            Dim VerifiedUsrID As Boolean = False

            'Dim dsUsertbl As DataSet
            Dim strSQLUserIdQuery As String = "Select USERID_SEQ from ISA_USERID_SEQ order by  USERID_SEQ desc"
            Dim dsOREmp As DataSet = ORDBData.GetAdapter(strSQLUserIdQuery)

            If dsOREmp.Tables(0).Rows.Count > 0 Then
                Dim Prev_UserID As String = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("USERID_SEQ"))


                Dim Inc_Count As Integer = 1
                If Prev_UserID <> "" Then
                    Do
                        Dim UserIDval As Integer = Convert.ToInt32(Prev_UserID) + Inc_Count
                        Dim strUser_ID_Created As String = Convert.ToString(UserIDval)
                        If strUser_ID_Created.Length = 1 Then
                            strUser_ID_Created = "000" + strUser_ID_Created
                        ElseIf strUser_ID_Created.Length = 2 Then
                            strUser_ID_Created = "00" + strUser_ID_Created
                        ElseIf strUser_ID_Created.Length = 3 Then
                            strUser_ID_Created = "0" + strUser_ID_Created
                        Else

                        End If

                        Dim UserID_Created As String = usrid + strUser_ID_Created

                        Dim userIDLen As Integer = UserID_Created.Length

                        If userIDLen > 10 Then
                            userIDLen = userIDLen - 10
                            usrid = usrid.Substring(0, usrid.Length - userIDLen)
                            UserID_Created = usrid + strUser_ID_Created
                        End If

                        AutoGenerateUserID = UserID_Created.ToUpper

                        Dim strSQLstring As String = "Select ISA_USER_ID FROM SDIX_USERS_TBL WHERE isa_employee_id = '" & AutoGenerateUserID & "'"

                        Dim dsUserid As DataSet = ORDBData.GetAdapter(strSQLstring)

                        If dsUserid.Tables(0).Rows.Count > 0 Then
                            VerifiedUsrID = True
                            Inc_Count = Inc_Count + 1
                        Else
                            VerifiedUsrID = False
                        End If

                    Loop Until (VerifiedUsrID = False)

                    UpdateUserIDSeqTBL(Inc_Count)

                End If
                AutoGenID = "A"
                Return AutoGenerateUserID
            End If
        Catch ex As Exception

        End Try


    End Function

    Private Function ManuallyVerified_Flags(ByVal Bu_value As String) As String
        Try
            If UserIdSessionvalue IsNot Nothing Then
                'Session.Remove("UserIDStoredValue")
            Else ''ManualVerified
                Session("BU_Val") = Bu_value
                Dim Script As String
                RadWindow1.NavigateUrl = "AddUserID.aspx"
                RadWindow1.OnClientClose = "OnClientClose"
                RadWindow1.Title = "Add User ID"
                Script = "function f(){$find(""" + RadWindow1.ClientID + """).show(); Sys.Application.remove_load(f);}Sys.Application.add_load(f);"
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "key", Script, True)
                Exit Function
            End If
        Catch ex As Exception

        End Try

    End Function

    Private Function UpdateUserIDSeqTBL(ByVal Inc_value As Integer)
        Try
            Dim strUserIdQuery As String = "Select USERID_SEQ from ISA_USERID_SEQ order by USERID_SEQ desc"
            Dim ds_PreviousUserIDSeq As DataSet = ORDBData.GetAdapter(strUserIdQuery)
            Dim strPreviousUserIDSeq As Integer = Convert.ToInt32(ds_PreviousUserIDSeq.Tables(0).Rows(0).Item("USERID_SEQ"))
            Dim UpdateQuery_UserIdSeq As String
            If Inc_value = 0 Then
                UpdateQuery_UserIdSeq = "Update ISA_USERID_SEQ set USERID_SEQ = " & strPreviousUserIDSeq + 1 & " where USERID_SEQ = " & strPreviousUserIDSeq & ""
            Else
                UpdateQuery_UserIdSeq = "Update ISA_USERID_SEQ set USERID_SEQ = " & strPreviousUserIDSeq + Inc_value & " where USERID_SEQ = " & strPreviousUserIDSeq & ""
            End If

            Dim ds_UpdatedUserIDSeq As DataSet = ORDBData.GetAdapter(UpdateQuery_UserIdSeq)
        Catch ex As Exception

        End Try

    End Function

    Private Function GetMaskValues_BU(ByVal str_BU As String) As String
        Dim Mask_BU_Value As String
        Try
            If str_BU <> "" Then
                Dim strUserIdQuery As String = "Select ISA_BU_MASK from SDIX_SITE_CONFIG where ISA_BUSINESS_UNIT = '" & str_BU & "'"
                Mask_BU_Value = ORDBData.GetScalar(strUserIdQuery)
                Return Mask_BU_Value
            End If
        Catch ex As Exception

        End Try
    End Function
    Private Function GetRegexPattern_MaskValues(ByVal strMask As String) As String

        Dim Regex_Value As String
        Dim PatternGen As New List(Of String)
        Try
            If strMask <> "" Then
                PatternGen.Add("(")
                For Each Chr As Char In strMask
                    If Char.IsNumber(Chr) Then
                        PatternGen.Add("([0-9])")
                    ElseIf Char.IsLetter(Chr) Then
                        PatternGen.Add("([a-zA-Z])")
                    Else
                        PatternGen.Add("([" & Chr & "])")
                    End If
                Next
                PatternGen.Add(")")
                Regex_Value = String.Join("", PatternGen.ToArray)
                Return Regex_Value
            End If
        Catch ex As Exception

        End Try
    End Function

    Private Function Verify_UserID(ByVal str_BU As String) As Boolean

        Dim Regex_Value As String
        Dim PatternGen As New List(Of String)
        Dim Mask_Value As String
        Dim GetPattenRegex As String
        Try
            Mask_Value = GetMaskValues_BU(str_BU)
            If Mask_Value <> "" Then
                GetPattenRegex = GetRegexPattern_MaskValues(Mask_Value)

                Dim regex As Regex = New Regex(GetPattenRegex)
                Dim match As Match = regex.Match(txtUserid.Text.Trim)
                If match.Success Then
                    If match.Value = txtUserid.Text.Trim Then
                        Return True
                    Else
                        Dim Rslt = "FAILED"
                        Return False
                    End If
                Else
                    Dim Rslt = "FAILED"
                    Return False
                End If
            Else
                If txtUserid.Text.Contains("'") Then
                    Return False
                Else
                    Return True
                End If
            End If

        Catch ex As Exception
        End Try
    End Function

    Protected Sub btnSetReqAppr_Click(sender As Object, e As EventArgs)
        Try
            'Hiding the Budgetory Approval fields
            lblAppEmpID.Visible = False
            DropAppEmpID.Visible = False
            chbDelete.Visible = False
            lblAppTotal.Visible = False
            txtAppTotal.Visible = False
            btnSubmit.Visible = False
            lblMsg.Visible = False
            btnSetReqAppr.Visible = False
            lblSiteBaseCurrencyCode.Text = ""
            tr_OrderLimit_fields.Style.Add("display", "none")

            'Showing the Requestor Approval fields
            lblAltReqAppr.Visible = True
            ddReqAppr.Visible = True
            chkDeleteReqAppr.Visible = True
            btnSubmitReqAppr.Visible = True
            lblMsgReqAppr.Visible = True
            btnSetBudAppr.Visible = True

            lblMsgReqAppr.Text = ""

            Dim strUserID As String = String.Empty
            Dim strBU As String = String.Empty

            If Session("USERTYPE") = "SUPER" Then
                strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
            Else
                strBU = GetBUbyGroup(txtGroupID.Text)
            End If
            strUserID = Trim(txtUserid.Text)

            getReqApprovals(strUserID, strBU)

        Catch ex As Exception

        End Try
    End Sub

    Protected Sub btnSubmitReqAppr_Click(sender As Object, e As EventArgs)
        Try
            If ddReqAppr.SelectedIndex = 0 Then
                lblMsgReqAppr.Text = "A valid approval name must be selected"
                Exit Sub
            Else
                lblMsgReqAppr.Text = ""
            End If
            If chkDeleteReqAppr.Checked Then
                deleteReqAPPRVRecord()
                chkDeleteReqAppr.Checked = False
                ddReqAppr.SelectedIndex = 0
            Else
                updateReqAPPRVTable()
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub btnSetBudAppr_Click(sender As Object, e As EventArgs)
        Try
            'Showing the Budgetory Approval fields
            lblAppEmpID.Visible = True
            DropAppEmpID.Visible = True
            chbDelete.Visible = True
            lblAppTotal.Visible = True
            txtAppTotal.Visible = True
            btnSubmit.Visible = True
            lblMsg.Visible = True
            btnSetReqAppr.Visible = True
            lblSiteBaseCurrencyCode.Text = ""
            tr_OrderLimit_fields.Style.Add("display", "contents")

            'Hiding the Requestor Approval fields
            lblAltReqAppr.Visible = False
            ddReqAppr.Visible = False
            chkDeleteReqAppr.Visible = False
            btnSubmitReqAppr.Visible = False
            lblMsgReqAppr.Visible = False
            btnSetBudAppr.Visible = False
            lblMsgReqAppr.Text = ""

            If Session("BUSUNIT") = "" Then
                Session.RemoveAll()
                Response.Redirect("default.aspx")
            End If
            txtAppTotal.Text = ""
            lblMsg.Text = ""
            Dim strBU As String
            Dim strMessage As New Alert
            If Session("USERTYPE") = "SUPER" Then
                strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
            Else
                strBU = GetBUbyGroup(txtGroupID.Text)
            End If
            If strBU = "0" Then
                RadMultiPage1.RenderSelectedPageOnly = True
                RadMultiPage1.SelectedIndex = 0
                tbStripUserDetails.Tabs(0).Selected = True
                'ltlAlert.Text = strMessage.Say("Error - Invalid Business Unit - check productview id's!"
                'ltlAlert.Text = "Error - Invalid Business Unit - check productview id''s!"
                ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - Invalid Business Unit - check productview id!"), True)
                Exit Sub
            End If

            Dim sOrdApprType As String = "O"
            'Dim objEnterprise As New clsEnterprise(strBU)
            Select Case sOrdApprType  '  objEnterprise.OrdApprType
                Case "O", "D", "M"
                    'OK 
                Case Else
                    RadMultiPage1.RenderSelectedPageOnly = True
                    RadMultiPage1.SelectedIndex = 0
                    tbStripUserDetails.Tabs(0).Selected = True
                    'ltlAlert.Text = strMessage.Say("Business unit is not set up as an approver site.")
                    'ltlAlert.Text = "Business unit is not set up as an approver site."
                    ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Business unit is not set up as an approver site."), True)
                    Exit Sub
            End Select
            LoadApprovals(txtUserid.Text, strBU, Session("USERTYPE"))
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub roleDropdownList_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            If Session("USERTYPE") = "SUPER" Then
                If roleDropdownList.SelectedValue = "CORPADMIN" Then
                    MultiSiteChk.Visible = False
                    PLMultiSelect.Visible = False
                Else
                    MultiSiteChk.Visible = True
                    PLMultiSelect.Visible = True
                End If
            ElseIf Session("USERTYPE") = "CORPADMIN" Then

            End If

            If Session("USERTYPEVALUE") = "C" Then
                If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                    If Page_Action = "EDIT" Then
                        'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
                        Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))
                    End If
                End If
            End If
            If Session("ROLE") = "CORPADMIN" Then
                If Page_Action = "ADD" Then
                    Dim strvalue As String = rcbGroup.SelectedValue
                    GetSisterSitesBU(Session("BUSUNIT"))
                    rcbGroup.SelectedValue = strvalue
                End If
            End If

            If Session("USERTYPEVALUE") = "S" Then
                If radioUserType.SelectedValue = "S" Or Session("USERTYPE") = "ADMIN" Or Session("USERTYPE") = "USER" Then
                    lblDept.Visible = True
                    rcbDept.Visible = True
                    'tr_Dept_details_fields.Style.Remove("display")
                Else
                    lblDept.Visible = False
                    rcbDept.Visible = False
                    'tr_Dept_details_fields.Style.Add("display", "none")
                End If
            Else
                lblDept.Visible = False
                rcbDept.Visible = False
                'tr_Dept_details_fields.Style.Add("display", "none")
            End If

            If Page_Action = "ADD" Then
                If Session("USERTYPEVALUE") = "S" Then
                    If Session("ROLE") <> "SUPER" Then
                        lblDept.Visible = False
                        rcbDept.Visible = False
                    Else
                        If radioUserType.SelectedValue = "V" Then
                            MultiSiteChk.Visible = False
                            PLMultiSelect.Visible = False
                        End If
                    End If
                Else

                End If
            End If

            If Session("USERTYPE") = "SUPER" Then
                If Page_Action = "EDIT" Then
                    If radioUserType.SelectedValue = "S" Then
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    ElseIf radioUserType.SelectedValue = "V" Then
                        lblSelectUser.Visible = True
                        Label_VendorID.Visible = True
                        tr_BU_unit_field.Style.Remove("display")
                        drpBUnit.Visible = True
                        lblBusUnit.Visible = True
                        Radcombobox_vendorID()
                        'RadcomboforVendorID.Visible = True
                        MainvndrID.Visible = True
                        lblGroup.Visible = False
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = False
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        rcbMultiSelect.Visible = False
                    Else
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    End If
                Else

                End If
            ElseIf Session("USERTYPE") = "ADMIN" Then
                If Session("USERTYPEVALUE") = "C" Then
                    If Page_Action = "EDIT" Then
                        CustomersUsers()
                    Else
                        rcbGroup.Visible = False
                    End If

                End If
            ElseIf Session("USERTYPE") = "CORPADMIN" Then
                If Page_Action = "EDIT" Then
                    CustomersUsers()
                    rcbGroup.Visible = True
                    MultiSiteChk.Visible = False
                    rcbMultiSelect.Visible = False
                    If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    Else
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    End If
                Else

                End If
            End If
            If btnChangePassw.Visible = True Then
                lblPassword.Visible = False
                lblConfirm.Visible = False
            Else
                lblPassword.Visible = True
                lblConfirm.Visible = True
            End If

        Catch ex As Exception

        End Try
    End Sub

    Protected Sub rcbdropSelectUser_SelectedIndexChanged(sender As Object, e As RadComboBoxSelectedIndexChangedEventArgs)
        Try
            drpBUnit.Visible = False
            lblBusUnit.Visible = False
            lblSelectUser.Visible = True
            lblPassword.Visible = False
            lblConfirm.Visible = False
            'TextBox_VendorId.Text = ""
            'Dim sss As String = dropSelectUser.SelectedValue
            Dim sss As String = rcbdropSelectUser.SelectedValue
            Dim selecteuserdrpdwn As String = Session("SelectUserDrpdwn")
            buildEditUser(selecteuserdrpdwn)
            'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Session("SelectUserDrpdwn")))
            Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Session("SelectUserDrpdwn")))
            'buildEditUser(dropSelectUser.SelectedValue)
            'buildSelectDropDown()
            If Session("PUNCHIN") = "YES" Then
                btnChangePassw.Visible = False
            Else
                btnChangePassw.Visible = True
            End If

            If Session("USERTYPEVALUE") = "S" Then
                If Session("ROLE") = "ADMIN" Then
                    If radioUserType.SelectedValue <> "S" Then
                        tr_Dept_details_fields.Style.Add("display", "none")
                    Else
                        tr_Dept_details_fields.Style.Remove("display")
                    End If
                End If
            End If
            If radioUserType.SelectedValue = "V" Or roleDropdownList.SelectedValue = "CORPADMIN" Then
                MultiSiteChk.Visible = False
                PLMultiSelect.Visible = False
            Else
                MultiSiteChk.Visible = True
                PLMultiSelect.Visible = True
            End If

            If Session("USERTYPE") = "SUPER" Then
                If Page_Action = "EDIT" Then
                    If radioUserType.SelectedValue = "S" Then
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        lblDept.Visible = True
                    ElseIf radioUserType.SelectedValue = "V" Then
                        lblSelectUser.Visible = True
                        Label_VendorID.Visible = True
                        tr_BU_unit_field.Style.Remove("display")
                        drpBUnit.Visible = True
                        lblBusUnit.Visible = True
                        Radcombobox_vendorID()
                        'RadcomboforVendorID.Visible = True
                        MainvndrID.Visible = True
                        lblGroup.Visible = False
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = False
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        rcbMultiSelect.Visible = False
                    Else
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        lblDept.Visible = False
                    End If
                Else

                End If
            ElseIf Session("USERTYPE") = "ADMIN" Then
                If Session("USERTYPEVALUE") = "S" Then
                    drpBUnit.Visible = False
                    lblBusUnit.Visible = False
                    lblSelectUser.Visible = True
                    'dropSelectUser.Visible = True
                    rcbdropSelectUser.Visible = True
                    lblGroup.Visible = True
                    btnAdd.Visible = True
                    lblPassword.Visible = False
                    lblConfirm.Visible = False
                    radioUserType.Visible = False
                    Label_usrtype.Visible = False
                    tr_PwdFields.Style.Add("display", "none")
                    tr_CpwdFields.Style.Add("display", "none")
                    tr_BU_unit_field.Style.Add("display", "none")
                    MultiSiteChk.Visible = True
                    rcbGroup.Visible = False
                    If Page_Action = "ADD" Then
                        lblGroup.Visible = True
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = True
                    End If
                    If Session("USERID") = Trim(rcbdropSelectUser.SelectedValue) Then
                        lblDept.Visible = True
                        rcbDept.Visible = True
                    Else
                        lblDept.Visible = False
                        rcbDept.Visible = False
                    End If
                    If roleDropdownList.SelectedValue = "CORPADMIN" Then
                        MultiSiteChk.Visible = False
                    End If
                ElseIf Session("USERTYPEVALUE") = "C" Then
                    If Page_Action = "EDIT" Then
                        CustomersUsers()
                    Else

                    End If
                End If
            ElseIf Session("USERTYPE") = "CORPADMIN" Then
                If Page_Action = "EDIT" Then
                    CustomersUsers()
                    rcbGroup.Visible = True
                    MultiSiteChk.Visible = False
                    rcbMultiSelect.Visible = False
                    If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    Else
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    End If
                Else

                End If
            Else

            End If

        Catch ex As Exception

        End Try
    End Sub

    Protected Sub MainvndrID_TextChanged(sender As Object, e As EventArgs)
        Try
            Dim strQueryChk_Vendor As String = ""
            Dim dsVendorExist As DataSet
            Dim ChkVendorId As String = ""
            If Trim(MainvndrID.Text) <> "" Then

                ChkVendorId = Chk_VendrID(Trim(MainvndrID.Text))
                If ChkVendorId <> "" Then
                    Error_VendorID.Text = "Vendor Name : " + ChkVendorId
                    Error_VendorID.ForeColor = Color.Green
                    Error_VendorID.Visible = True
                    Dim dsORUsers As DataSet = GetSelectDropDownData()

                    Dim dtORUsers As DataTable = dsORUsers.Tables(0).AsEnumerable().Where(Function(r) r.Field(Of String)("userandbu").Contains(MainvndrID.Text)).CopyToDataTable()
                    rcbdropSelectUser.DataSource = dtORUsers
                    rcbdropSelectUser.DataValueField = "ISA_EMPLOYEE_ID"
                    rcbdropSelectUser.DataTextField = "USERANDBU"
                    rcbdropSelectUser.DataBind()
                    rcbdropSelectUser.Items.Insert(0, New RadComboBoxItem("<< Select User >>"))
                Else
                    Error_VendorID.Text = "Entered Vendor ID is Invalid"
                    Error_VendorID.ForeColor = Color.Red
                    Error_VendorID.Visible = True
                    buildSelectDropDown()
                End If
            Else
                Error_VendorID.Text = "Please enter Vendor ID"
                Error_VendorID.ForeColor = Color.Red
                Error_VendorID.Visible = True
                buildSelectDropDown()
            End If

            If Page_Action = "EDIT" Then
                lblSelectUser.Visible = True
                Label_VendorID.Visible = True
                tr_BU_unit_field.Style.Remove("display")
                drpBUnit.Visible = True
                lblBusUnit.Visible = True
                Radcombobox_vendorID()
                'RadcomboforVendorID.Visible = True
                MainvndrID.Visible = True
                lblGroup.Visible = False
                rcbGroup.Visible = False
                MultiSiteChk.Visible = False
                roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                rcbMultiSelect.Visible = False
            Else

            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Function Chk_VendrID(ByVal strVendrID As String) As String
        Dim strValue As String = ""
        Try

            Dim strQueryChk_Vendor As String = ""
            Dim dsVendorExist As DataSet

            strQueryChk_Vendor = "SELECT * FROM SYSADM8.PS_VENDOR WHERE VENDOR_STATUS = 'A' AND VENDOR_ID = '" & Trim(strVendrID) & "'"
            dsVendorExist = ORDBData.GetAdapter(strQueryChk_Vendor)

            If Not dsVendorExist Is Nothing Then
                If dsVendorExist.Tables(0).Rows.Count > 0 Then
                    strValue = Convert.ToString(dsVendorExist.Tables(0).Rows(0).Item("NAME1"))
                Else
                    strValue = ""
                End If
            Else
                strValue = ""
            End If
            Return Trim(strValue)
        Catch ex As Exception
            Return Trim(strValue)
        End Try
    End Function

    Private Sub BuildDeptDropdown()
        Try
            Dim strGetDeptDropdown = "SELECT * FROM SDIX_TCKT_DEPT"
            Dim dsDeptData As DataSet
            dsDeptData = ORDBData.GetAdapter(strGetDeptDropdown)

            If Not dsDeptData Is Nothing Then
                If dsDeptData.Tables(0).Rows.Count > 0 Then
                    rcbDept.DataSource = dsDeptData.Tables(0)
                    rcbDept.DataValueField = "DEPT_ID"
                    rcbDept.DataTextField = "DEPT_NAME"
                    rcbDept.DataBind()
                    rcbDept.Items.Insert(0, New RadComboBoxItem("Select Department", "0"))
                    rcbDept.DataValueField.Insert(0, "")
                Else

                End If
            Else

            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub FieldsPosition()
        Try
            If Session("USERTYPE") = "SUPER" Then
                If Page_Action = "EDIT" Then
                    If radioUserType.SelectedValue = "S" Then
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        lblDept.Visible = True
                    ElseIf radioUserType.SelectedValue = "V" Then
                        lblSelectUser.Visible = True
                        Label_VendorID.Visible = True
                        tr_BU_unit_field.Style.Remove("display")
                        drpBUnit.Visible = True
                        lblBusUnit.Visible = True
                        Radcombobox_vendorID()
                        'RadcomboforVendorID.Visible = True
                        MainvndrID.Visible = True
                        lblGroup.Visible = False
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = False
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        rcbMultiSelect.Visible = False
                    Else
                        lblSelectUser.Visible = True
                        roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        lblDept.Visible = False
                    End If
                Else

                End If
            ElseIf Session("USERTYPE") = "ADMIN" Then
                If Session("USERTYPEVALUE") = "S" Then
                    drpBUnit.Visible = False
                    lblBusUnit.Visible = False
                    lblSelectUser.Visible = True
                    'dropSelectUser.Visible = True
                    rcbdropSelectUser.Visible = True
                    lblGroup.Visible = True
                    btnAdd.Visible = True
                    lblPassword.Visible = False
                    lblConfirm.Visible = False
                    radioUserType.Visible = False
                    Label_usrtype.Visible = False
                    tr_PwdFields.Style.Add("display", "none")
                    tr_CpwdFields.Style.Add("display", "none")
                    tr_BU_unit_field.Style.Add("display", "none")
                    MultiSiteChk.Visible = True
                    rcbGroup.Visible = False
                    If Page_Action = "ADD" Then
                        lblGroup.Visible = True
                        rcbGroup.Visible = False
                        MultiSiteChk.Visible = True
                    End If
                    If roleDropdownList.SelectedValue = "CORPADMIN" Then
                        MultiSiteChk.Visible = False
                    End If
                ElseIf Session("USERTYPEVALUE") = "C" Then
                    If Page_Action = "EDIT" Then
                        CustomersUsers()
                    Else

                    End If
                End If
            ElseIf Session("USERTYPE") = "USER" Then
                If Session("USERTYPEVALUE") = "S" Then
                    lblSelectUser.Visible = False
                    'TextBox_VendorId.Visible = False
                    lblBusUnit.Visible = False
                    drpBUnit.Visible = False
                    lblSelectUser.Visible = False
                    'dropSelectUser.Visible = False
                    rcbdropSelectUser.Visible = False
                    Label_usrtype.Visible = False
                    radioUserType.Visible = False
                    lblGroup.Visible = True
                    btnAdd.Visible = False
                    lblPassword.Visible = False
                    lblConfirm.Visible = False
                    rcbGroup.Visible = False
                    tr_PwdFields.Style.Add("display", "none")
                    tr_CpwdFields.Style.Add("display", "none")
                    tr_BU_unit_field.Style.Add("display", "none")
                    tr_selectuser_fields.Style.Add("display", "none")
                    tr_Multiselect_fields.Style.Add("display", "none")
                ElseIf Session("USERTYPEVALUE") = "C" Then
                    If Page_Action = "EDIT" Then
                        CustomersUsers()
                    Else

                    End If
                End If
            ElseIf Session("USERTYPE") = "CORPADMIN" Then
                If Page_Action = "EDIT" Then
                    CustomersUsers()
                    rcbGroup.Visible = True
                    MultiSiteChk.Visible = False
                    rcbMultiSelect.Visible = False
                    If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    Else
                        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    End If
                Else

                End If
            End If
            If Page_Action = "EDIT" Then
                btnAdd.Visible = True
                btnEdit.Visible = False
                If Session("USERTYPEVALUE") = "S" Then
                    If Session("USERTYPE") = "ADMIN" Then
                        If Session("USERID") = rcbdropSelectUser.SelectedValue Then
                            lblDept.Visible = True
                            rcbDept.Visible = True
                        Else
                            lblDept.Visible = False
                            rcbDept.Visible = False
                        End If
                    ElseIf Session("USERTYPE") = "USER" Then
                        lblDept.Visible = True
                        rcbDept.Visible = True
                        btnAdd.Visible = False
                        btnEdit.Visible = False
                    End If
                ElseIf Session("USERTYPEVALUE") = "C" Then
                    If Session("USERTYPE") = "USER" Then
                        btnAdd.Visible = False
                        btnEdit.Visible = False
                    Else

                    End If
                End If
            Else
                btnAdd.Visible = False
                btnEdit.Visible = True
            End If
        Catch ex As Exception

        End Try
    End Sub

End Class
