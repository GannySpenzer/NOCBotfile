Imports System.Data.OleDb
Imports System.IO
Imports Newtonsoft.Json
Imports System.Web.Mail
Imports System.Web.UI
Imports SDI.Walmart.API.ORDBData
Imports SDI.Walmart.API.WebPSharedFunc
Public Class UserProfileBL
    Private AutoGenID As String = String.Empty
    Private Const m_cUserGroup_Vendor As String = "SUPPLIER"
    Private Const m_cUserGroup_Mexico As String = "MEXICO"
    Private Const m_cProdDispType_CatalogSQL As String = "C" ' Catalog SQL
    Private Const m_cProdDispType_PSClient As String = "P" ' PeopleSoft Oracle 
    Private m_sAppTotalOrig As String
    Private m_sAppEmpIDOrig As String
    Private m_sAppAltOrig As String
    Private m_sReq_AltAppr_Orig As String
    Private m_sReq_AltAppr_EmpID As String
    '**
    '* Ref: UP_PC_175 & UP_PC_176
    '* 
    '* This method is triggered when User Profile Page is loaded
    '* 
    Public Function Page_Load(ByVal data As String) As String
        Try
            Dim _result As New DataSet
            If Not String.IsNullOrEmpty(data) Then
                Dim objUserProfilePageLoadBO As UserProfileBO = JsonConvert.DeserializeObject(Of UserProfileBO)(data)
                Dim Login_UserId As String = objUserProfilePageLoadBO.Session_USERID
                Dim Login_UserBU As String = objUserProfilePageLoadBO.Session_BUSUNIT

                ' Here they are getting the count of mulitsite drop down checked.
                'Dim int As Integer = rcbMultiSelect.CheckedItems.Count() 17/07/2020 
                '17/07/2020----This session is not neccesary because it is used to get the user type and used in side the profile page alone.
                'Session("SetClickValue_Usertype") = radioUserType.SelectedValue
                Dim SisterSiteTable As New DataTable
                SisterSiteTable.TableName = "SisterSiteTable"
                Dim HideTabList As New DataTable
                HideTabList.TableName = "HideTabList"
                HideTabList.Columns.Add("SDITrackTab")
                HideTabList.Columns.Add("OSETab")
                HideTabList.Columns.Add("APPTab")
                HideTabList.Columns.Add("PREFTab")
                HideTabList.Columns.Add("UPVLTab")
                HideTabList.Columns.Add("ZeusTab")
                Dim HideTabListRow As DataRow = HideTabList.NewRow()
                HideTabListRow("SDITrackTab") = False
                HideTabListRow("OSETab") = False
                HideTabListRow("APPTab") = False
                HideTabListRow("PREFTab") = False
                HideTabListRow("UPVLTab") = False
                HideTabListRow("ZeusTab") = False
                Dim Error_Table As New DataTable
                Error_Table.TableName = "Error_Table"
                Error_Table.Columns.Add("SDiTrack")
                Error_Table.Columns.Add("UserExist")


                Dim UserDetalis As DataSet = GetUserValues(Login_UserId, Login_UserBU, objUserProfilePageLoadBO.Session_ThirdParty_CompanyID)

                Dim User_ROLE As String = UserDetalis.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")
                Dim USERTYPEVALUE As String = UserDetalis.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")

                Dim AddUserFlag As New DataTable

                AddUserFlag.Columns.Add("AUTO_ASSIGN_FLG")
                Dim AddUserFlag_row As DataRow = AddUserFlag.NewRow()
                AddUserFlag_row("AUTO_ASSIGN_FLG") = UserDetalis.Tables(1).Rows(0).Item("AUTO_ASSIGN_FLG")
                AddUserFlag.Rows.Add(AddUserFlag_row)
                AddUserFlag.TableName = "AddUserFlag"
                _result.Tables.Add(AddUserFlag)

                '17/07/2020 To get the Third Party Company list
                Dim SQLSTRINGQuery As String
                Dim ThirdPartytable As New DataTable
                If Not String.IsNullOrEmpty(objUserProfilePageLoadBO.Session_ThirdParty_CompanyID) Then

                    If objUserProfilePageLoadBO.Session_ThirdParty_CompanyID = 0 Then
                        SQLSTRINGQuery = "Select * from SDIX_ThirdParty_Table where ISA_BUSINESS_UNIT = '" & Login_UserBU & "'"
                    Else
                        SQLSTRINGQuery = "Select * from SDIX_ThirdParty_Table where ISA_BUSINESS_UNIT = '" & Login_UserBU & "' And THIRDPARTY_COMP_ID = '" & objUserProfilePageLoadBO.Session_ThirdParty_CompanyID & "'"
                    End If

                    Dim dsOREmp As DataSet = ORDBData.GetAdapter(SQLSTRINGQuery)


                    ThirdPartytable = dsOREmp.Tables(0).Copy()

                End If
                ThirdPartytable.TableName = "ThirdPartytable"
                _result.Tables.Add(ThirdPartytable)
                'Page_Action = Session("PageAction")   17/07/2020

                '1707/2020 The below code is used to load the login user details when edit mode button is clicked in add user page.
                'If Page.IsPostBack = False Then
                '    Session("PageAction") = "EDIT"
                '    Session("ChangePWDEnabled") = ""
                '    Call GetUserValues()
                '    BuildDeptDropdown()
                'End If

                'Page_Action = Session("PageAction") 17/07/2020

                'Dim sddss As String = Page_Action 17/07/2020  NOt used

                'Me.Title = "Profile"   17/07/2020 Handle in the front end

                'Session("SCREENNAME") = "Profile.aspx" 17/07/2020 Handle in the front end

                '17/07/2020 during page load the select user value is logined user id.
                'Dim drpdownvalue As String = rcbdropSelectUser.SelectedValue
                'Session("SelectUserDrpdwn") = drpdownvalue

                'Code for SDI Track Starts
                Dim oSDITrack As New clsSDITrack()
                Try
                    ' Don't show SDiTrack for Vendor or Mexico Vendor
                    'If Not String.IsNullOrEmpty(objUserProfilePageLoadBO.Query_IsVendor) And Not String.IsNullOrEmpty(objUserProfilePageLoadBO.Query_IsMexicoVendor) Then
                    If Not objUserProfilePageLoadBO.Query_IsVendor And Not objUserProfilePageLoadBO.Query_IsMexicoVendor Then
                        If oSDITrack.IsAccountSDITrack(objUserProfilePageLoadBO.Session_TrackDBType) And oSDITrack.IsPrivilegeSDITrack(Login_UserId, Login_UserBU) Then
                            'Show_SDITrackRow("Show_SDITrackTab") = True   17/07/2020
                        Else
                            HideTabListRow("SDITrackTab") = True
                        End If
                    End If
                    'End If
                Catch ex As Exception
                    Dim Error_TableRow As DataRow = Error_Table.NewRow()
                    Error_TableRow("SDiTrack") = "SDiTrack Issue"
                    Error_Table.Rows.Add(Error_TableRow)
                End Try
                Dim DepartmentTable As New DataTable
                If USERTYPEVALUE = "S" Then

                    DepartmentTable = BuildDeptDropdown()

                End If
                DepartmentTable.TableName = "DepartmentTable"
                _result.Tables.Add(DepartmentTable)
                'Code for SDI Track Ends

                'Setting page title
                '17/07/2020 MOve to front end
                'Dim lblTitle As Label = CType(Master.FindControl("lblTitle"), Label)
                'Dim strSelectedGroupValue As String = ""

                'lblTitle.Text = "Profile Update"
                'RadMultiPage1.RenderSelectedPageOnly = True

                'Me.lblempl.Visible = False
                'Me.lblVendr.Visible = False

                'Dim VendorQueryValue = Request.QueryString("VENDOR")
                'Dim CustomerQueryValue = Request.QueryString("CUSTOMER")
                'Dim SDIEmpId As String = Session("SDIEMP")
                'Dim SDIUserId As String = Session("USERID")
                'Dim SDIUserIdVP As String = Session("USERID_VP") 17/07/2020 NOt is use

                'If Session("SDIEMP") = "" Or Session("USERID") = "" Then
                '    Session.RemoveAll()
                '    Response.Redirect("default.aspx")
                'End If
                '17/07/2020 Move to Fornt end -----------------------
                'rcbMultiSelect.ID = "rcbMultiSelect"
                'rcbMultiSelect.CheckBoxes = True
                'rcbMultiSelect.EnableCheckAllItemsCheckBox = True
                'rfvMultiBU.ID = "rfvMultiBU"
                'rcbMultiSelect.CssClass = "MultiselectChkbox"
                'rfvMultiBU.ControlToValidate = "rcbMultiSelect"
                'rfvMultiBU.ErrorMessage = "Select BU for Multi Site Access"
                'rfvMultiBU.CssClass = "usergroup-field"
                'PLMultiSelect.Controls.Add(rcbMultiSelect)
                'Dim int1 As Integer = rcbMultiSelect.CheckedItems.Count()

                'If MultiSiteChk.Checked Then
                '    rcbMultiSelect.Visible = True
                'Else
                '    rcbMultiSelect.Visible = False
                'End If

                'turnvalidationoff()

                'If Session("SDIEMP") = "CUST" Then
                '    MultiSiteChk.Visible = True
                'End If
                '================END===========
                '---17/07/2020   Move to front end
                'If Session("USERTYPE") = "SUPER" And Not IsVendor() And Not IsMexicoVendor() Then
                '    If Page.IsPostBack Then
                '        strSelectedGroupValue = rcbGroup.SelectedValue
                '    End If
                '    'rcbGroup.ID = "rcbGroup"
                '    'rcbGroup.EnableViewState = True
                '    'rcbGroup.Width = rcbGroupTab2.Width
                '    'rcbGroup.MaxHeight = rcbGroupTab2.MaxHeight
                '    'rcbGroup.Filter = RadComboBoxFilter.Contains            
                '    'PLGroup.Controls.Add(rcbGroup)
                '    Me.drpBUnit.Visible = False
                '    Me.lblBusUnit.Visible = False
                '    Label_usrtype.Visible = True
                '    radioUserType.Visible = True
                '    Label_rolefield.Visible = True
                '    roleDropdownList.Visible = True
                '    txtUserid.MaxLength = 10
                'Else
                '    If Not IsVendor() And Not IsMexicoVendor() Then
                '        Me.lblVendr.Visible = False
                '        Me.lblempl.Visible = False
                '    Else
                '        'Me.lblVendr.Visible = True
                '        'Me.lblempl.Visible = True
                '    End If
                '    txtGroup.EnableViewState = True
                '    txtGroup.Width.Pixel(160)
                '    txtGroupID.EnableViewState = True
                '    txtGroupID.Visible = False
                '    PLGroup.Controls.Add(txtGroup)
                '    PLGroup.Controls.Add(txtGroupID)
                '    Label_usrtype.Visible = True

                '    If Session("USERTYPEVALUE") = "S" Then
                '        If Session("ROLE") = "ADMIN" Or Session("ROLE") = "USER" Or Session("ROLE") = "CORPADMIN" Then
                '            If Page_Action = "EDIT" Then
                '                radioUserType.Visible = True
                '                radioUserType.Enabled = False
                '                roleDropdownList.Visible = True
                '                roleDropdownList.Enabled = False
                '                lblSelectUser.Visible = False
                '                'dropSelectUser.Visible = False
                '                rcbdropSelectUser.Visible = False
                '                btnAdd.Visible = False
                '            End If
                '        Else
                '            radioUserType.Visible = True
                '            roleDropdownList.Visible = True
                '        End If
                '    End If

                '    Label_rolefield.Visible = True
                '    txtUserid.MaxLength = 10
                '    Me.drpBUnit.Visible = True
                '    Me.lblBusUnit.Visible = True
                '    lblGroup.Visible = False
                '    MultiSiteChk.Visible = False

                'End If
                '===========================END.
                '---17/07/2020 The below code is used to build the add new user form.
                'If USERTYPEVALUE = "C" Then
                '    If User_ROLE = "ADMIN" Or User_ROLE = "CORPADMIN" Then
                '        If Page_Action = "ADD" Then
                '            If radioUserType.SelectedValue = "C" Then
                '                rcbGroup.EnableViewState = False
                '                Label_usrtype.Visible = False
                '                MultiSiteChk.Visible = True
                '                MultiSiteChk.Enabled = True
                '                lblBusUnit.Visible = False
                '                drpBUnit.Visible = False
                '                lblGroup.Visible = True
                '                rcbMultiSelect.Enabled = True
                '            ElseIf Session("USERTYPEVALUE") = "C" Then
                '                'rcbGroup.ID = "rcbGroup"
                '                'rcbGroup.EnableViewState = True
                '                'rcbGroup.Width = rcbGroupTab2.Width
                '                'rcbGroup.MaxHeight = rcbGroupTab2.MaxHeight
                '                'rcbGroup.Filter = RadComboBoxFilter.Contains
                '                'PLGroup.Controls.Add(rcbGroup)
                '                rcbGroup.Visible = True
                '                buildGroupList(rcbGroup)
                '            End If
                '        End If
                '    ElseIf Session("ROLE") = "USER" Then
                '        If Page_Action = "ADD" Then
                '            If radioUserType.SelectedValue = "C" Or radioUserType.SelectedValue = "S" Then
                '                'rcbGroup.ID = "rcbGroup"
                '                'rcbGroup.EnableViewState = True
                '                'rcbGroup.Width = rcbGroupTab2.Width
                '                'rcbGroup.MaxHeight = rcbGroupTab2.MaxHeight
                '                'rcbGroup.Filter = RadComboBoxFilter.Contains
                '                'PLGroup.Controls.Add(rcbGroup)
                '                rcbGroup.Visible = True
                '            End If
                '        End If
                '    End If
                'ElseIf Session("USERTYPEVALUE") = "S" Then
                '    If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                '        If Page_Action = "ADD" Then
                '            If radioUserType.SelectedValue = "C" Then
                '                lblGroup.Visible = True
                '                MultiSiteChk.Visible = True
                '                lblBusUnit.Visible = False
                '                drpBUnit.Visible = False
                '                Label_usrtype.Visible = False
                '            End If
                '        End If
                '    ElseIf Session("ROLE") = "USER" Then
                '        If Page_Action = "ADD" Then
                '            If radioUserType.SelectedValue = "C" Or radioUserType.SelectedValue = "S" Then
                '                'rcbGroup.ID = "rcbGroup"
                '                'rcbGroup.EnableViewState = True
                '                'rcbGroup.Width = rcbGroupTab2.Width
                '                'rcbGroup.MaxHeight = rcbGroupTab2.MaxHeight
                '                'rcbGroup.Filter = RadComboBoxFilter.Contains
                '                'PLGroup.Controls.Add(rcbGroup)
                '                rcbGroup.Visible = True
                '            End If
                '        End If
                '    End If
                'End If
                '---------------------------END
                '17/07/2020 Handle in the Front end.
                'If Session("USERTYPE") = "CORPADMIN" Then
                '    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                'Else
                '    If roleDropdownList.SelectedValue = "CORPADMIN" Then
                '        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                '    End If
                'End If
                '---------------------- ENd
                '17/07/2020   UserIdSessionvalue Has the new user id entered manually
                'UserIdSessionvalue = Session("UserIDStoredValue")
                '----------End
                '17/07/2020 the below code is used during the Add user form submit click
                'Dim Countvalue As Integer
                'If Page.IsPostBack Then
                '    Dim strSiteGroupValuedrpdn As String = rcbGroup.SelectedValue
                '    If strSiteGroupValuedrpdn <> "" Then
                '        If UserIdSessionvalue IsNot Nothing Then
                '            Countvalue = Session("CountIncrement")
                '            If Countvalue = 0 Then
                '                Session.Remove("UserIDStoredValue")
                '                Session("CountIncrement") += 1
                '                btnSave_Click(btnSave, EventArgs.Empty)
                '            Else
                '                Session.Remove("UserIDStoredValue")
                '                UserIdSessionvalue = String.Empty
                '                Session.Remove("CountIncrement")
                '            End If
                '        End If
                '    ElseIf txtGroupID.Text <> "" Then
                '        If Session("USERTYPEVALUE") = "C" Or Session("USERTYPEVALUE") = "S" Then
                '            If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
                '                If UserIdSessionvalue IsNot Nothing Then
                '                    Countvalue = Session("CountIncrement")
                '                    If Countvalue = 0 Then
                '                        Session.Remove("UserIDStoredValue")
                '                        Session("CountIncrement") += 1
                '                        btnSave_Click(btnSave, EventArgs.Empty)
                '                    Else
                '                        Session.Remove("UserIDStoredValue")
                '                        UserIdSessionvalue = String.Empty
                '                        Session.Remove("CountIncrement")
                '                    End If
                '                End If
                '            End If
                '        End If
                '    End If
                'End If
                '17/07/2020 
                'If Page.IsPostBack Then
                '    m_sAppTotalOrig = Session("APPR_TOTAL")
                '    m_sAppEmpIDOrig = Session("APPR_APR_EMPID")
                '    m_sAppAltOrig = Session("APPR_APR_ALT")

                '    If strSelectedGroupValue <> "" Then
                '        If Not rcbGroup.FindItemByValue(strSelectedGroupValue) Is Nothing Then
                '            rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(strSelectedGroupValue).Index
                '        End If
                '    End If
                'Else
                WebLog(Login_UserId, Login_UserBU, objUserProfilePageLoadBO.Session_UserName, objUserProfilePageLoadBO.Session_SDIEMP)
                '19/07/2020 Punchin feature is not included for the version 1
                'If objUserProfilePageLoadBO.Session_PUNCHIN = "YES" Then
                '    'btnChangePassw.Visible = False 17/07/2020
                '    If IsAscend(Login_UserBU) Or VoucherClass.IsEnergizer(Login_UserBU) Then
                '        'tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True  17/07/2020
                '    Else
                '        HideTabListRow("OSETab") = False
                '    End If
                'End If
                '17/07/2020 MOve to the front ENd
                'lblAction.Text = Page_Action
                '    lblVendor.Text = Request.QueryString("VENDOR")
                '    lblMexico.Text = Request.QueryString("MEXICO")

                '    If Page_Action = "EDIT" Then
                '        lblGroup.Visible = False
                '        btnChangePassw.Visible = False
                '        lblSelectUser.Visible = True
                '        'dropSelectUser.Visible = True
                '        rcbdropSelectUser.Visible = True
                '        btnAdd.Visible = True
                '        lblSwitch.Visible = False
                '---------------End
                Dim SearchUsertable As New DataTable

                SearchUsertable = buildSelectDropDown(Login_UserId, Login_UserBU, User_ROLE, objUserProfilePageLoadBO.Session_ThirdParty_CompanyID, objUserProfilePageLoadBO.Session_SDIEMP, USERTYPEVALUE, objUserProfilePageLoadBO.Query_IsVendor)
                SearchUsertable.TableName = "SearchUsertable"
                _result.Tables.Add(SearchUsertable)
                'txtUserid.ReadOnly = True
                'txtUserid.BackColor = LightGray
                'valuserid1.Enabled = False
                'valUserid2.Enabled = False
                'tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = True
                'tbStripUserDetails.Tabs.FindTabByValue("MOB").Visible = True
                'btnAccess.Visible = False
                'If Session("PUNCHIN") = "YES" Then
                '    If IsAscend(Session("BUSUNIT")) Or Insiteonline.VoucherSharedFunctions.VoucherClass.IsEnergizer(Session("BUSUNIT")) Then
                '        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                '    Else
                '        tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                '    End If
                'Else
                '    tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                'End If
                Select Case Trim(objUserProfilePageLoadBO.Session_APPRTYPE)
                    Case "O", "D", "M"

                        'tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True 17/07/2020
                    Case Else

                        HideTabListRow("APPTab") = False
                End Select
                If User_ROLE = "ADMINR" Then

                    '        tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                    'tbStripUserDetails.FindTabByValue("TST").Visible = False
                    HideTabListRow("APPTab") = True
                    HideTabListRow("SDITrackTab") = True
                End If
                'Label_VendorID.Visible = False
                'TextBox_VendorId.Visible = False
                'Val_txtVendorID.Visible = False
                If clsAccessPrivileges.IsPrivilegeOn(Login_UserId, Login_UserBU, clsAccessPrivileges.UserPrivsEnum.ZeusAnalytical, "GOZEUS") Then

                    HideTabListRow("ZeusTab") = False
                Else
                    HideTabListRow("ZeusTab") = True
                End If
                '17/07/2020   Handle in the front end
                'ElseIf Page_Action = "ADD" Then
                '    lblGroup.Visible = False
                '    setuppasswordfields("SETUP")
                '    lblSelectUser.Visible = False
                '    btnEdit.Visible = False
                '    lblSwitch.Visible = False
                '    tbStripUserDetails.FindTabByValue("TST").Visible = False
                '    tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = False
                '    btnAccess.Visible = False

                '    If Session("PUNCHIN") = "YES" Then
                '        If IsAscend(Session("BUSUNIT")) Or Insiteonline.VoucherSharedFunctions.VoucherClass.IsEnergizer(Session("BUSUNIT")) Then
                '            tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                '        Else
                '            tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                '        End If
                '    Else : tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True

                '    End If

                'If Request.QueryString("VENDOR") = "NO" Then
                '            If CustomerQueryValue = "NO" OrElse Nothing Then
                '                lblSelectUser.Visible = False
                '                radioUserType.SelectedIndex = 0
                '            End If
                '        End If

                '        If Request.QueryString("CUSTOMER") = "YES" Then
                '            txtUserid.ReadOnly = True
                '            txtUserid.BackColor = LightGray
                '            valuserid1.Enabled = False
                '            valUserid2.Enabled = False
                '            MultiSiteChk.Visible = True
                '        End If

                '        If Request.QueryString("VENDOR") = "YES" Then
                '            PLGroup.Visible = False
                '            lblGroup.Visible = False
                '            radioUserType.SelectedIndex = 1
                '            Label_VendorID.Visible = True
                '            'TextBox_VendorId.Visible = True
                '            'Val_txtVendorID.Visible = True
                '            lblSelectUser.Visible = False
                '            MultiSiteChk.Visible = False
                '            txtUserid.ReadOnly = True
                '            txtUserid.BackColor = LightGray
                '        Else
                '            If Session("CurrentValueOfUserTypeField") <> Nothing Then
                '                radioUserType.SelectedValue = Session("CurrentValueOfUserTypeField")
                '                Label_VendorID.Visible = False
                '                'TextBox_VendorId.Visible = False
                '                ''Val_txtVendorID.Visible = False
                '            Else
                '                radioUserType.SelectedIndex = 0
                '                Label_VendorID.Visible = False
                '                'TextBox_VendorId.Visible = False
                '                'Val_txtVendorID.Visible = False
                '            End If
                '        End If
                '---------ENd
                '17/07/2020 Below condition is already handle above
                'Select Case Trim(Session("APPRTYPE"))
                '            Case "O", "D", "M"
                '                'btnApprovals.Visible = True
                '                tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                '            Case Else
                '                'btnApprovals.Visible = False
                '                tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                '        End Select
                'If Session("USERTYPE") = "ADMINR" Then
                '    tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                '    tbStripUserDetails.FindTabByValue("TST").Visible = False
                'End If
                '        roleDropdownList.Items.Insert(0, New ListItem("Select Type", "0"))
                '    Else
                '        lblSelectUser.Visible = True
                '        'TextBox_VendorId.Visible = False
                '        txtUserid.ReadOnly = True
                '        txtUserid.BackColor = LightGray
                '        valuserid1.Enabled = False
                '        valUserid2.Enabled = False
                '        lblGroup.Visible = False
                '        MultiSiteChk.Visible = False
                '        buildEditUser(Session("USERID"))
                '        buildSelectDropDown()
                '        If Session("PUNCHIN") = "YES" Then
                '            If IsAscend(Session("BUSUNIT")) Or Insiteonline.VoucherSharedFunctions.VoucherClass.IsEnergizer(Session("BUSUNIT")) Then
                '                tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                '            Else
                '                tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                '            End If
                '        Else
                '            tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = True
                '        End If
                '    End If
                '----------------------END
                Dim GroupTable As New DataTable
                GroupTable.TableName = "GroupTable"
                GroupTable.Columns.Add("txtGroupID")
                GroupTable.Columns.Add("txtGroup")
                If User_ROLE = "SUPER" Then

                    If objUserProfilePageLoadBO.Query_IsVendor Then
                        'txtGroupID.Text = "0"  17/07/2020
                        'txtGroup.Text = m_cUserGroup_Vendor
                        'txtGroup.ReadOnly = True
                        'txtGroup.BackColor = LightGray
                        'btnAccess.Visible = False

                        HideTabListRow("OSETab") = True
                        HideTabListRow("APPTab") = True
                        HideTabListRow("SDITrackTab") = True
                        'tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False 17/07/2020
                        'tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                        'tbStripUserDetails.FindTabByValue("TST").Visible = False
                    ElseIf objUserProfilePageLoadBO.Query_IsMexicoVendor Then
                        '17/07/2020
                        'txtGroupID.Text = "0"
                        'txtGroup.Text = m_cUserGroup_Mexico
                        'txtGroup.ReadOnly = True
                        'txtGroup.BackColor = LightGray
                        'btnAccess.Visible = False
                        'drpBUnit.Visible = False
                        'lblBusUnit.Visible = False
                        'tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                        'tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                        'tbStripUserDetails.FindTabByValue("TST").Visible = False
                        HideTabListRow("OSETab") = True
                        HideTabListRow("APPTab") = True
                        HideTabListRow("SDITrackTab") = True
                    Else
                        SisterSiteTable = buildGroupList(User_ROLE, Login_UserBU)
                        HideTabListRow("APPTab") = False
                        'buildGroupList(rcbGroupSites)
                        'tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                    End If
                ElseIf User_ROLE = "ADMIN" Or
                    User_ROLE = "ADMINX" Or
                    User_ROLE = "ADMINR" Or
                    User_ROLE = "CORPADMIN" Then
                    Dim intGroupid As String = getGroupID(Login_UserId)


                    Dim GroupTable_rows As DataRow = GroupTable.NewRow()
                    GroupTable_rows("txtGroupID") = intGroupid.ToString
                    GroupTable_rows("txtGroup") = getUserGroupsName(intGroupid)
                    GroupTable.Rows.Add(GroupTable_rows)
                    HideTabListRow("APPTab") = False
                    'txtGroup.ReadOnly = True  19/07/2020
                    'txtGroup.BackColor = LightGray
                    'tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                Else
                    HideTabListRow("UPVLTab") = True
                    HideTabListRow("SDITrackTab") = True
                    HideTabListRow("APPTab") = False
                    'tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = False
                    'tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                    'tbStripUserDetails.FindTabByValue("TST").Visible = False
                End If
                'End If
                _result.Tables.Add(GroupTable)
                '19/07/2020 Handle in the front end
                'If Not IsVendor() And Page_Action = "ADD" Then
                '    ' For Add New User or Add New Mexico, display only the User Information tab.
                '    tbStripUserDetails.Tabs.FindTabByValue("UPVL").Visible = False
                '    tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = False
                '    tbStripUserDetails.Tabs.FindTabByValue("OSE").Visible = False
                '    tbStripUserDetails.Tabs.FindTabByValue("PREF").Visible = False
                '    tbStripUserDetails.Tabs.FindTabByValue("TST").Visible = False

                '    'lblSelectUser.Visible = True
                '    'lblSelectUser.Text = "Add Profile"

                '    setuppasswordfields("SETUP")
                'End If
                If objUserProfilePageLoadBO.Query_IsVendor Or objUserProfilePageLoadBO.Query_IsMexicoVendor Then
                    'tbStripUserDetails.Tabs.FindTabByValue("PREF").Visible = False
                    HideTabListRow("PREFTab") = False
                End If

                buildEditUser(Login_UserId, Login_UserBU, objUserProfilePageLoadBO.Session_SDIEMP, USERTYPEVALUE, User_ROLE, objUserProfilePageLoadBO.Query_IsMexicoVendor, Error_Table, _result, SisterSiteTable)
                '19/07/2020 Not necessary
                'If Not Page.IsPostBack Then
                'If Page_Action = "EDIT" Then
                '    If Session("USERTYPEVALUE") = "C" Then
                '        If Session("ROLE") = "CORPADMIN" Then
                '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                '        End If
                '        buildEditUser(Session("USERID"))
                '    End If
                'End If
                HideTabList.Rows.Add(HideTabListRow)
                SisterSiteTable.TableName = "SisterSiteTable"
                _result.Tables.Add(SisterSiteTable)
                _result.Tables.Add(HideTabList)
                _result.Tables.Add(Error_Table)
                data = JsonConvert.SerializeObject(_result)
            End If
            '19/07/2020 In the below code the form fields hide and show is handled based on the user type value which should be handled in the front end.
            'Dim sss As String = Session("ROLE")
            'Dim ee As String = Session("USERTYPEVALUE")

            'If Not Page.IsPostBack Then
            '    If Session("USERTYPEVALUE") = "S" Then ''SDI Employee
            '        If Page_Action = "EDIT" Then
            '            valuserid1.Enabled = False
            '            'userid_Regex_validation.Enabled = False
            '            SDIUsers()
            '        End If
            '    ElseIf Session("USERTYPEVALUE") = "V" Then ''Vendor

            '    Else ''Customer
            '        If Page_Action = "EDIT" Then
            '            CustomersUsers()
            '        End If
            '    End If
            'End If

            '19/07/2020 Not neccessary In the page Load part
            ' check/auto-select logged in user amongst the list (if exist)
            '   - erwin 2009.09.22
            'If Not Page.IsPostBack Then
            '    Dim sId As String = CStr(Session("USERID")).Trim

            '    If sId.Length > 0 And Me.rcbdropSelectUser.Items.Count > 0 Then
            '        Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=sId))
            '        buildEditUser(rcbdropSelectUser.SelectedValue)
            '        'buildEditUser(sId)
            '        If Session("PUNCHIN") = "YES" Then
            '            btnChangePassw.Visible = False
            '        Else
            '            btnChangePassw.Visible = True
            '        End If
            '    End If
            'End If

            'If Me.IsPostBack Then
            '    txtPassword.Attributes("value") = txtPassword.Text
            '    txtConfirm.Attributes("value") = txtConfirm.Text
            'End If

            'If Session("USERTYPEVALUE") = "S" Then
            '    If Session("ROLE") = "SUPER" Then
            '        If Page_Action = "ADD" Then
            '            If radioUserType.SelectedValue = "V" Then
            '                tr_vendorId_fields.Style.Remove("display")
            '                tr_BU_unit_field.Style.Remove("display")
            '                Label_VendorID.Visible = True
            '                lblBusUnit.Visible = True
            '                drpBUnit.Visible = True
            '                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '            ElseIf radioUserType.SelectedValue = "C" Then
            '                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            '            Else
            '                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '            End If
            '        Else
            '            lblPassword.Visible = False
            '            lblConfirm.Visible = False
            '            If radioUserType.SelectedValue = "C" Then
            '                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            '            Else
            '                roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '            End If
            '        End If
            '    End If
            'ElseIf Session("USERTYPEVALUE") = "C" Then
            '    If Session("ROLE") = "USER" Then
            '        btnAdd.Visible = False
            '        btnEdit.Visible = False
            '    End If
            '    If Session("ROLE") = "CORPADMIN" Then
            '        If roleDropdownList.SelectedValue = "CORPADMIN" Then
            '            PLGroup.Visible = False
            '            rcbGroup.Visible = True
            '            MultiSiteChk.Visible = False
            '            rcbMultiSelect.Visible = False
            '        Else
            '            If Page_Action = "EDIT" Then
            '                If roleDropdownList.SelectedValue <> "CORPADMIN" Then
            '                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '                Else
            '                    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            '                End If
            '            End If
            '            PLGroup.Visible = False
            '            rcbGroup.Visible = True
            '            MultiSiteChk.Visible = False
            '            rcbMultiSelect.Visible = False
            '        End If
            '    End If
            '    If Session("ROLE") = "ADMIN" Then
            '        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '        If MultiSiteChk.Checked Then
            '            rcbMultiSelect.Visible = True
            '        Else
            '            rcbMultiSelect.Visible = False
            '        End If
            '    End If
            'End If
            'If Session("USERTYPEVALUE") = "C" Then
            '    If Session("ROLE") = "ADMIN" Or Session("ROLE") = "CORPADMIN" Then
            '        If Page_Action = "EDIT" Then
            '            'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
            '            Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))
            '        End If
            '    End If
            'End If

            'If Session("USERTYPE") = "CORPADMIN" Then
            '    If Page_Action = "ADD" Then
            '        tr_Multiselect_fields.Style.Add("display", "none")
            '        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '    Else
            '        tr_Multiselect_fields.Style.Add("display", "none")
            '        roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            '    End If
            'End If

            'If Not Page.IsPostBack Then
            '    If Session("ZEUSNOCATALOGSITE") = "Y" Then
            '        'Table13.Visible = False
            '        rrbProdDispCatSQL.Visible = False
            '        rrbProdDispPSClient.Visible = False
            '        lblProdDispTyp.Visible = False
            '    End If
            'End If

            'If Session("USERTYPEVALUE") = "S" Then
            '    If Session("USERTYPE") = "SUPER" Then
            '        If radioUserType.SelectedValue = "S" Then
            '            If Page_Action = "ADD" Then
            '                lblDept.Visible = True
            '            End If
            '        End If
            '    End If
            'ElseIf Session("USERTYPEVALUE") = "C" Then
            '    tr_Dept_details_fields.Style.Add("display", "none")
            'End If
            'If radioUserType.SelectedValue = "V" Or roleDropdownList.SelectedValue = "CORPADMIN" Then
            '    MultiSiteChk.Visible = False
            '    PLMultiSelect.Visible = False
            'Else
            '    MultiSiteChk.Visible = True
            '    PLMultiSelect.Visible = True
            'End If

            'Dim int3 As Integer = rcbMultiSelect.CheckedItems.Count()
            'End If
            '----------------END

        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty

        End Try
        Return data
    End Function
    '********** This method is used to get the user primary details.
    Private Function GetUserValues(ByVal Session_UserID As String, ByVal Session_UserBu As String, ByVal Session_ThirdParty_CompanyID As String)
        Dim UserDetalis As New DataSet
        Try
            Dim SDIUserId As String = Session_UserID
            Dim SQLSTRINGQuery As String = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & SDIUserId & "'"
            If Not String.IsNullOrEmpty(Session_ThirdParty_CompanyID) Then
                If Not Session_ThirdParty_CompanyID = 0 Then
                    SQLSTRINGQuery = SQLSTRINGQuery + "And THIRDPARTY_COMP_ID = '" & Session_ThirdParty_CompanyID & "'"
                End If

            End If
            Dim dsOREmp As DataSet = ORDBData.GetAdapter(SQLSTRINGQuery)


            If dsOREmp.Tables(0).Rows.Count() > 0 Then
                dsOREmp.Tables(0).TableName = "Employee Details"
                UserDetalis.Tables.Add(dsOREmp.Tables(0).Copy())
                '17/07/2020
                'Session("ROLE") = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")
                'Session("USERTYPEVALUE") = dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
                'Session("LoginedInUser_BU") = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
            End If

            Dim SQLAutoFlagQuery As String = "SELECT AUTO_ASSIGN_FLG FROM PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT = '" & Session_UserBu & "'"
            Dim dsFlagBU As DataSet = ORDBData.GetAdapter(SQLAutoFlagQuery)
            dsFlagBU.Tables(0).TableName = "Employee Flag Details"
            UserDetalis.Tables.Add(dsFlagBU.Tables(0).Copy())
            'Session("Flag_AddUser") = dsFlagBU.Tables(0).Rows(0).Item("AUTO_ASSIGN_FLG") 17/07/2020
        Catch ex As Exception

        End Try
        Return UserDetalis
    End Function


    Public Function buildEditUser(ByVal strUserID As String, ByVal UserBu As String, ByVal Session_SDIEMP As String,
                                  ByVal USERTYPEVALUE As String, ByVal User_ROle As String, ByVal IsMexicoVendor As Boolean, ByRef Error_Table As DataTable, ByRef _result As DataSet, ByRef SisterSiteTable As DataTable)
        'Dim strMessage As New Alert  19/07/2020
        'lblMessage.Text = ""
        'Dim UserProfileBuildSet As New DataSet
        Try
            Dim Error_TableRow As DataRow = Error_Table.NewRow()
            Dim Multisite As New DataTable
            Dim MultiSite_Checked As New DataTable
            Dim UserProfileDeatils As New DataTable
            UserProfileDeatils.TableName = "UserProfileDeatils"
            UserProfileDeatils.Columns.Add("Employee_ID") 'User Id Text box Value
            UserProfileDeatils.Columns.Add("Businees_Unit")
            UserProfileDeatils.Columns.Add("UserType") 'User type Radio button select value
            UserProfileDeatils.Columns.Add("MainvndrID") 'Vendor Id Text box Value
            UserProfileDeatils.Columns.Add("Vendor Name") 'Vendor Company name label Value
            UserProfileDeatils.Columns.Add("UserRole") 'User Role Drop down value
            UserProfileDeatils.Columns.Add("Hide_User_ID") 'Hiden User ID 
            UserProfileDeatils.Columns.Add("active_status") 'User Active Status hidden value
            UserProfileDeatils.Columns.Add("Employee_active_status")
            UserProfileDeatils.Columns.Add("First_Name") 'User First name text box value
            UserProfileDeatils.Columns.Add("Last_Name") 'User Last name text box value
            UserProfileDeatils.Columns.Add("Email_Address") 'User Email Address text box value
            UserProfileDeatils.Columns.Add("Phone_No") 'User Phone Number text box value
            UserProfileDeatils.Columns.Add("ThirdParty_Id")
            UserProfileDeatils.Columns.Add("Site") 'User Site Drop down value
            UserProfileDeatils.Columns.Add("Departement") 'User Department Drop down value
            UserProfileDeatils.Columns.Add("Enable_MultiSite") 'User Multi site enable checked box value
            UserProfileDeatils.Columns.Add("txtGroup") 'User Site Group If drop down doesn't exist
            UserProfileDeatils.Columns.Add("txtGroupID") 'User Site Group Id If drop down doesn't exist
            UserProfileDeatils.Columns.Add("btnActivateAccount") 'Value to Show or hide Active Account Button
            UserProfileDeatils.Columns.Add("btnInactivateAccount") 'Value to Show or hide DeActive Account Button
            UserProfileDeatils.Columns.Add("btnEmplActivateAccount") 'Value to Show or hide Active Employee Account Button
            UserProfileDeatils.Columns.Add("btnEmplInactivateAccount") 'Value to Show or hide DeActive Employee Account Button
            UserProfileDeatils.Columns.Add("lblAccountDisabled") 'Error message If Account is not Active
            UserProfileDeatils.Columns.Add("lblEmplAccountDisabled") 'Error message Employee account is not active
            UserProfileDeatils.Columns.Add("lblAccountVisible") 'Error message If Account is not Active
            UserProfileDeatils.Columns.Add("lblEmplAccountVisible") 'Error message Employee account is not active

            Dim UserProfileDeatils_Row As DataRow = UserProfileDeatils.NewRow()

            Dim strSQLString As String = ""

            If USERTYPEVALUE = "C" Then
                If User_ROle = "ADMIN" Then
                    strSQLString = "SELECT ISA_USER_ID, FIRST_NAME_SRCH," & vbCrLf &
                            " LAST_NAME_SRCH, ISA_EMPLOYEE_EMAIL," & vbCrLf &
                            " ISA_EMPLOYEE_ID, PHONE_NUM, ISA_EMPLOYEE_ACTYP," & vbCrLf &
                            " ISA_SDI_EMPLOYEE, BUSINESS_UNIT, active_status,Multi_Site, ISA_VENDOR_ID, TCKTDEPT,THIRDPARTY_COMP_ID" & vbCrLf &
                            " FROM SDIX_USERS_TBL" & vbCrLf &
                            " WHERE ISA_EMPLOYEE_ID = '" & strUserID & "' AND ISA_EMPLOYEE_ACTYP <> 'CORPADMIN'" & vbCrLf
                    'If Session("PUNCHIN") = "YES" Then 19/07/2020 Punchin Does not exist
                    '    strSQLString = strSQLString & "AND BUSINESS_UNIT = '" & Session("BUSUNIT") & "'"
                    'End If
                Else
                    strSQLString = "SELECT ISA_USER_ID, FIRST_NAME_SRCH," & vbCrLf &
                            " LAST_NAME_SRCH, ISA_EMPLOYEE_EMAIL," & vbCrLf &
                            " ISA_EMPLOYEE_ID, PHONE_NUM, ISA_EMPLOYEE_ACTYP," & vbCrLf &
                            " ISA_SDI_EMPLOYEE, BUSINESS_UNIT, active_status,Multi_Site, ISA_VENDOR_ID, TCKTDEPT,THIRDPARTY_COMP_ID" & vbCrLf &
                            " FROM SDIX_USERS_TBL" & vbCrLf &
                            " WHERE ISA_EMPLOYEE_ID = '" & strUserID & "'" & vbCrLf
                    'If Session("PUNCHIN") = "YES" Then 19/07/2020 Punchin Does Not exist
                    '    strSQLString = strSQLString & "AND BUSINESS_UNIT = '" & Session("BUSUNIT") & "'"
                    'End If

                End If
            Else
                strSQLString = "SELECT ISA_USER_ID, FIRST_NAME_SRCH," & vbCrLf &
                            " LAST_NAME_SRCH, ISA_EMPLOYEE_EMAIL," & vbCrLf &
                            " ISA_EMPLOYEE_ID, PHONE_NUM, ISA_EMPLOYEE_ACTYP," & vbCrLf &
                            " ISA_SDI_EMPLOYEE, BUSINESS_UNIT, active_status,Multi_Site, ISA_VENDOR_ID, TCKTDEPT,THIRDPARTY_COMP_ID" & vbCrLf &
                            " FROM SDIX_USERS_TBL" & vbCrLf &
                            " WHERE ISA_EMPLOYEE_ID = '" & strUserID & "'" & vbCrLf
            End If

            If Not IsUserCanReinstate(strUserID, User_ROle, Session_SDIEMP) Then
                ' If the logged in user cannot activate/inactivate an account, then filter out all the inactive users.
                ' If the logged in user can activate/inactivate, let them see all accounts unrestricted by active status.
                strSQLString &= " AND ACTIVE_STATUS IN ('" & clsUserTbl.ActiveStatus_Active & "','" & clsUserTbl.ActiveStatus_FailedLogin & "')" & vbCrLf
            End If

            Dim dsOREmp As DataSet = ORDBData.GetAdapter(strSQLString)

            If dsOREmp.Tables(0).Rows.Count = 0 Then
                'Me.dropSelectUser.SelectedIndex = 0
                'Me.rcbdropSelectUser.SelectedIndex = 0 19/07/2020
                Error_TableRow("UserExist") = "Error - User does not exist in ISA_USERS_TBL!"
                Error_Table.Rows.Add(Error_TableRow)
                Multisite.TableName = "Multisite"
                _result.Tables.Add(Multisite)
                MultiSite_Checked.TableName = "MultiSite_Checked"
                _result.Tables.Add(MultiSite_Checked)
                _result.Tables.Add(UserProfileDeatils)
                ' ltlAlert.Text = strMessage.Say("Error - User does not exist in ISA_USERS_TBL!")
                Exit Function
            ElseIf dsOREmp.Tables(0).Rows.Count > 1 Then
                Error_TableRow("UserExist") = "Error - User exist more than once in ISA_USERS_TBL table!"
                Error_Table.Rows.Add(Error_TableRow)
                Multisite.TableName = "Multisite"
                _result.Tables.Add(Multisite)
                MultiSite_Checked.TableName = "MultiSite_Checked"
                _result.Tables.Add(MultiSite_Checked)
                _result.Tables.Add(UserProfileDeatils)
                'ltlAlert.Text = strMessage.Say("Error - User exist more than once in ISA_USERS_TBL table!")
                Exit Function
                'ElseIf dsOREmp.Tables(0).Rows.Count = 1 Then
                '    Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_USER_ID"))))
            End If
            UserProfileDeatils_Row("ThirdParty_Id") = dsOREmp.Tables(0).Rows(0).Item("THIRDPARTY_COMP_ID")
            UserProfileDeatils_Row("UserType") = dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
            UserProfileDeatils_Row("Businees_Unit") = dsOREmp.Tables(0).Rows(0).Item("business_unit")
            UserProfileDeatils_Row("First_Name") = dsOREmp.Tables(0).Rows(0).Item("FIRST_NAME_SRCH")
            UserProfileDeatils_Row("Last_Name") = dsOREmp.Tables(0).Rows(0).Item("LAST_NAME_SRCH")
            UserProfileDeatils_Row("Phone_No") = Trim(dsOREmp.Tables(0).Rows(0).Item("PHONE_NUM"))
            UserProfileDeatils_Row("Email_Address") = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_EMAIL")
            UserProfileDeatils_Row("Employee_ID") = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ID")
            UserProfileDeatils_Row("Hide_User_ID") = dsOREmp.Tables(0).Rows(0).Item("ISA_USER_ID")
            UserProfileDeatils_Row("active_status") = dsOREmp.Tables(0).Rows(0).Item("active_status").ToString.ToUpper
            UserProfileDeatils_Row("Departement") = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("TCKTDEPT"))
            UserProfileDeatils_Row("Enable_MultiSite") = dsOREmp.Tables(0).Rows(0).Item("MULTI_SITE").ToString()
            Dim lblActiveStatusHide As String = dsOREmp.Tables(0).Rows(0).Item("active_status").ToString.ToUpper

            If dsOREmp.Tables(0).Rows(0).Item("MULTI_SITE").ToString() = "Y" Then
                'MultiSiteChk.Checked = True 19/07/2020
                'rcbMultiSelect.Visible = True 19/07/2020
                'If MultiSiteChk.Checked Then 19/07/2020
                'If Request.QueryString("MEXICO") = "YES" Then
                '    drpBUnit.Visible = False
                '    lblBusUnit.Visible = False
                'End If
                If dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT") <> "SDM00" Then


                    'Multisite.TableName = "Multisite"
                    Multisite = GetMultiBusinessUnit(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT"))

                    'Else 19/07/2020
                    '    MultiSiteChk.Checked = False
                    '    rcbMultiSelect.Visible = False
                End If
                '19/07/2020 Handle in Front end
                '    Else
                '        drpBUnit.Visible = False
                '        lblBusUnit.Visible = False
                '        rcbMultiSelect.Visible = False

                '    End If
                'Else
                '    MultiSiteChk.Checked = False
                '    rcbMultiSelect.Visible = False
                '    rcbMultiSelect.ClearCheckedItems()


                Dim strQuery As String = "Select BUSINESS_UNIT FROM SDIX_MULTI_SITE WHERE ISA_EMPLOYEE_ID='" & strUserID & "'"
                Dim dsMultiBU As DataSet = ORDBData.GetAdapter(strQuery)


                MultiSite_Checked = dsMultiBU.Tables(0).Copy()

            End If
            Multisite.TableName = "Multisite"
            _result.Tables.Add(Multisite)
            MultiSite_Checked.TableName = "MultiSite_Checked"
            _result.Tables.Add(MultiSite_Checked)
            '19/07/2020 Handle in the front end
            'If dsMultiBU.Tables(0).Rows.Count > 0 Then
            '    For i As Integer = 0 To dsMultiBU.Tables(0).Rows.Count - 1
            '        For Each checkedItem As RadComboBoxItem In rcbMultiSelect.Items
            '            If checkedItem.Value = dsMultiBU.Tables(0).Rows(i).Item("BUSINESS_UNIT") Then
            '                checkedItem.Checked = True
            '                If dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT") = checkedItem.Value Then
            '                    checkedItem.Enabled = False
            '                End If
            '            End If
            '            'Do Something (insert)
            '        Next
            '    Next
            'End If


            Dim lblEmplActiveStatusHide As String = " "
            Dim strCurrBU As String = " "
            strCurrBU = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
            If Trim(strCurrBU) = "" Or Trim(strCurrBU) = "0" Then
                strCurrBU = UserBu
            End If
            'lblCurrBUHide.Text = strCurrBU  19/07/2020
            ' check Employee table EFF_STATUS
            strSQLString = "SELECT A.EFF_STATUS from PS_ISA_EMPL_TBL A " &
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

            UserProfileDeatils_Row("Employee_active_status") = strEmplEffStatus
            lblEmplActiveStatusHide = strEmplEffStatus

            'roleDropdownList.ClearSelection() 19/07/2020
            If dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE") = "S" And Not IsMexicoVendor Then
                Select Case dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")
                    Case "USER"
                        UserProfileDeatils_Row("UserRole") = "USER"
                    'roleDropdownList.SelectedIndex = 3
                    Case "ADMIN"
                        UserProfileDeatils_Row("UserRole") = "ADMIN"
                    'roleDropdownList.SelectedIndex = 2
                    Case "SUPER"
                        UserProfileDeatils_Row("UserRole") = "SUPER"
                    'roleDropdownList.SelectedIndex = 0
                    Case "CORPADMIN"
                        UserProfileDeatils_Row("UserRole") = "CORPADMIN"
                        'roleDropdownList.SelectedIndex = 1
                End Select
            Else
                Select Case dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")
                    Case "USER"
                        UserProfileDeatils_Row("UserRole") = "USER"
                    'roleDropdownList.SelectedIndex = 3
                    Case "ADMIN"
                        UserProfileDeatils_Row("UserRole") = "ADMIN"
                    'roleDropdownList.SelectedIndex = 2
                    Case "ADMINA"
                        UserProfileDeatils_Row("UserRole") = "ADMIN"
                    'roleDropdownList.SelectedIndex = 2
                    Case "ADMINR"
                        UserProfileDeatils_Row("UserRole") = "ADMIN"
                    'roleDropdownList.SelectedIndex = 2
                    Case "ADMINX"
                        UserProfileDeatils_Row("UserRole") = "ADMIN"
                    'roleDropdownList.SelectedIndex = 2
                    Case "ADMINX"
                        UserProfileDeatils_Row("UserRole") = "ADMIN"
                    'roleDropdownList.SelectedIndex = 2
                    Case "SUPER"
                        UserProfileDeatils_Row("UserRole") = "SUPER"
                    'roleDropdownList.SelectedIndex = 0
                    Case "CORPADMIN"
                        UserProfileDeatils_Row("UserRole") = "CORPADMIN"
                        'roleDropdownList.SelectedIndex = 1
                End Select
            End If
            'radioUserType.ClearSelection() 19/07/2020
            'Select Case dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
            '    Case "S"
            '        radioUserType.SelectedIndex = 0
            '    Case "V"
            '        radioUserType.SelectedIndex = 1
            '    Case "C"
            '        radioUserType.SelectedIndex = 2
            'End Select
            ' 19/07/2020 In the below code the Vendor site is selected in the vendor site drop down.this should be handled in the front end.
            'If Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")) = "V" Then
            '    Dim sBU As String = dsOREmp.Tables(0).Rows(0).Item("business_unit")
            '    Dim liItem As ListItem = drpBUnit.Items.FindByValue(sBU)
            '    If liItem IsNot Nothing Then
            '        drpBUnit.SelectedIndex = drpBUnit.Items.IndexOf(liItem)
            '        drpBUnit.Visible = True
            '        lblBusUnit.Visible = True
            '    End If
            'End If
            '19/07/2020 Handle in the front end
            'If IsVendor() Or IsMexicoVendor() Then
            '    'drpUserType.Visible = False
            '    Dim sBU As String = dsOREmp.Tables(0).Rows(0).Item("business_unit")
            '    Dim liItem As ListItem = drpBUnit.Items.FindByValue(sBU)
            '    If liItem IsNot Nothing Then
            '        drpBUnit.SelectedIndex = drpBUnit.Items.IndexOf(liItem)
            '    End If
            '    Exit Sub
            'End If

            If Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID")) <> "" Then
                'TextBox_VendorId.Text = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID"))
                'RadcomboforVendorID.SelectedValue = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID"))
                If User_ROle = "SUPER" Then
                    If dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE") = "V" Then
                        UserProfileDeatils_Row("MainvndrID") = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID"))
                        Dim GetVndrUserName As String = Chk_VendrID(Trim(Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_VENDOR_ID"))))
                        If GetVndrUserName <> "" Then
                            UserProfileDeatils_Row("Vendor Name") = "Vendor Name : " + GetVndrUserName
                            'Error_VendorID.ForeColor = Color.Green 19/07/2020
                            'Error_VendorID.Visible = True
                            'Else
                            '    MainvndrID.Text = ""
                            '    Error_VendorID.Text = ""
                            '    Error_VendorID.ForeColor = Color.Red
                            '    Error_VendorID.Visible = False
                        End If
                        'Else  19/07/2020
                        '    MainvndrID.Text = ""
                        '    Error_VendorID.Text = ""
                        '    Error_VendorID.ForeColor = Color.Red
                        '    Error_VendorID.Visible = False
                    End If
                    'Else 19/07/2020
                    '    MainvndrID.Text = ""
                    '    Error_VendorID.Text = ""
                    '    Error_VendorID.ForeColor = Color.Red
                    '    Error_VendorID.Visible = False
                End If
            Else
                If dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE") = "V" Then
                    Dim GetVndrUserName As String = Chk_VendrID(Trim(Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ID"))))
                    If GetVndrUserName <> "" Then
                        UserProfileDeatils_Row("MainvndrID") = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ID"))
                        UserProfileDeatils_Row("Vendor Name") = "Vendor Name : " + GetVndrUserName
                        '    Error_VendorID.ForeColor = Color.Green 19/07/2020
                        '    Error_VendorID.Visible = True
                        'Else
                        '    MainvndrID.Text = ""
                        '    Error_VendorID.Text = ""
                        '    Error_VendorID.ForeColor = Color.Red
                        '    Error_VendorID.Visible = False
                    End If
                    'Else
                    '    MainvndrID.Text = "" 19/07/2020
                    '    Error_VendorID.Text = ""
                    '    Error_VendorID.ForeColor = Color.Red
                    '    Error_VendorID.Visible = False
                End If
            End If
            '19/07/2020 Department select value should be done in the front end
            'If Session("USERTYPEVALUE") = "S" Then
            '    If radioUserType.SelectedValue = "S" Then
            '        If Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("TCKTDEPT")) <> "" Then
            '            rcbDept.SelectedValue = Convert.ToString(dsOREmp.Tables(0).Rows(0).Item("TCKTDEPT"))
            '        Else
            '            rcbDept.SelectedValue = ""
            '        End If
            '    Else
            '        rcbDept.SelectedValue = ""
            '        rcbDept.Visible = False
            '        lblDept.Visible = False
            '    End If
            'Else
            '    rcbDept.SelectedValue = ""
            '    rcbDept.Visible = False
            '    lblDept.Visible = False
            'End If
            'Dim SisterSites As New DataTable 19/07/2020
            'SisterSites.TableName = "SisterSites"
            'new code for assigning the BU
            If User_ROle = "SUPER" Then
                'rcbGroup.ClearSelection() 19/07/2020 Handle in the front end
                'Dim strBU As String = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
                'If Not rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")) Is Nothing Then
                '    rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")).Index
                'End If
            ElseIf User_ROle = "CORPADMIN" Then
                If User_ROle = "CORPADMIN" Then
                    '19/07/2020 handle in the front end
                    'rcbGroup.ClearSelection()
                    SisterSiteTable.Clear()
                    Dim strBU As String = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
                    SisterSiteTable = GetSisterSitesBU(strBU)
                    SisterSiteTable.TableName = "SisterSiteTable"
                    '_result.Tables.Add(SisterSites) 19/07/2020
                    '19/07/2020 handle in the front end
                    'If Not rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")) Is Nothing Then
                    '    rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")).Index
                    'End If
                    'MultiSiteChk.Visible = False
                    'PLGroup.Visible = False
                Else
                    '19/07/2020 handle in the front end
                    'rcbGroup.ClearSelection()
                    SisterSiteTable.Clear()
                    Dim strBU As String = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
                    SisterSiteTable = GetSisterSitesBU(strBU)
                    SisterSiteTable.TableName = "SisterSiteTable"
                    '_result.Tables.Add(SisterSites) 19/07/2020
                    '19/07/2020 handle in the front end
                    'If Not rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")) Is Nothing Then
                    '    rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")).Index
                    'End If
                    'MultiSiteChk.Visible = False
                    'PLGroup.Visible = False
                End If
            Else
                'txtGroup.ReadOnly = True 19/07/2020
                'txtGroup.BackColor = LightGray
                UserProfileDeatils_Row("txtGroup") = getUserGroupsName(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT"))
                UserProfileDeatils_Row("txtGroupID") = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
            End If

            ' First, just set these to invisible to start then figure out what should show with the logic below.
            '19/07/2020
            UserProfileDeatils_Row("lblAccountDisabled") = False
            UserProfileDeatils_Row("btnActivateAccount") = False
            UserProfileDeatils_Row("btnInactivateAccount") = False

            UserProfileDeatils_Row("btnEmplActivateAccount") = False
            UserProfileDeatils_Row("btnEmplInactivateAccount") = False
            UserProfileDeatils_Row("lblEmplAccountDisabled") = False
            UserProfileDeatils_Row("lblEmplAccountVisible") = False
            UserProfileDeatils_Row("lblAccountVisible") = False

            If IsUserCanReinstate(strUserID, User_ROle, Session_SDIEMP) Then
                If lblActiveStatusHide = clsUserTbl.ActiveStatus_FailedLogin Then
                    ' User is temporarily inactivated because they previously failed the login attempts.
                    'lblAccountDisabled.Visible = True 19/07/2020
                    UserProfileDeatils_Row("lblAccountVisible") = True
                    UserProfileDeatils_Row("lblAccountDisabled") = "This SDIX User account has been DISABLED due to excessive invalid login attempts."
                    UserProfileDeatils_Row("btnActivateAccount") = True
                    UserProfileDeatils_Row("btnInactivateAccount") = True ' Allow user to inactivate this account that's disabled due to excessive login attempts just in case we no longer want the account...
                ElseIf lblActiveStatusHide = clsUserTbl.ActiveStatus_Inactive Then
                    'lblAccountDisabled.Visible = True 19/07/2020
                    UserProfileDeatils_Row("lblAccountVisible") = True
                    UserProfileDeatils_Row("lblAccountDisabled") = "This SDIX User account is inactive."
                    If IsUserCanReinstate(strUserID, User_ROle, Session_SDIEMP) Then
                        UserProfileDeatils_Row("btnActivateAccount") = True ' This account is already inactive. The only thing we can do is activate it.
                    End If
                Else
                    ' This account is active so just allow logged in user ability to inactivate.                
                    UserProfileDeatils_Row("btnInactivateAccount") = True
                    UserProfileDeatils_Row("btnEmplActivateAccount") = False
                    UserProfileDeatils_Row("btnActivateAccount") = False
                End If
                'employee related
                If Not User_ROle = "CORPADMIN" Then
                    If lblEmplActiveStatusHide = "N" Then
                        UserProfileDeatils_Row("btnEmplInactivateAccount") = False
                        UserProfileDeatils_Row("btnEmplActivateAccount") = False
                        UserProfileDeatils_Row("lblEmplAccountVisible") = True
                        'lblEmplAccountDisabled.Visible = False 19/07/2020
                    Else
                        If lblEmplActiveStatusHide = clsUserTbl.EmplActiveStatus_Active Then
                            UserProfileDeatils_Row("btnEmplInactivateAccount") = True
                            UserProfileDeatils_Row("btnEmplActivateAccount") = False
                            UserProfileDeatils_Row("lblEmplAccountVisible") = False
                            'lblEmplAccountDisabled.Visible = False 19/07/2020
                        Else
                            UserProfileDeatils_Row("btnEmplInactivateAccount") = False
                            UserProfileDeatils_Row("btnEmplActivateAccount") = True
                            UserProfileDeatils_Row("lblEmplAccountDisabled") = "This Employee account is inactive."
                            UserProfileDeatils_Row("lblEmplAccountVisible") = True
                            'lblEmplAccountDisabled.Visible = True 19/07/2020
                        End If
                    End If  '  If lblEmplActiveStatusHide.Text = "N" Then
                End If
            Else
                ' The logged in user cannot inactivate or reinstate an account. So just show an appropriate message for the account.
                If lblActiveStatusHide = clsUserTbl.ActiveStatus_FailedLogin Then
                    'lblAccountDisabled.Visible = True 19/07/2020
                    UserProfileDeatils_Row("lblAccountVisible") = True
                    UserProfileDeatils_Row("lblAccountDisabled") = "This SDIX User account has been disabled due to excessive invalid login attempts. Please contact the Help Desk at 215-633-1900, option 7 to reinstate the account."
                ElseIf lblActiveStatusHide = clsUserTbl.ActiveStatus_Inactive Then
                    ' This should never happen because we don't show inactive users to a logged in user that can't reinstate an account.
                    ' But we'll keep this logic just in case...
                    'lblAccountDisabled.Visible = True 19/07/2020
                    UserProfileDeatils_Row("lblAccountVisible") = True
                    UserProfileDeatils_Row("lblAccountDisabled") = "This SDIX User account in inactive."
                End If
                'employee related
                If Not User_ROle = "CORPADMIN" Then
                    If lblEmplActiveStatusHide = "N" Then
                        UserProfileDeatils_Row("btnEmplInactivateAccount") = False
                        UserProfileDeatils_Row("btnEmplActivateAccount") = False
                        UserProfileDeatils_Row("lblEmplAccountVisible") = False
                        'lblEmplAccountDisabled.Visible = False 19/07/2020
                    Else
                        If lblEmplActiveStatusHide = clsUserTbl.EmplActiveStatus_Active Then
                            UserProfileDeatils_Row("btnEmplInactivateAccount") = False
                            UserProfileDeatils_Row("btnEmplActivateAccount") = False
                            UserProfileDeatils_Row("lblEmplAccountVisible") = False
                            'lblEmplAccountDisabled.Visible = False   19/07/2020
                        Else
                            UserProfileDeatils_Row("btnEmplInactivateAccount") = False
                            UserProfileDeatils_Row("btnEmplActivateAccount") = False
                            UserProfileDeatils_Row("lblEmplAccountDisabled") = "This Employee account is inactive."
                            UserProfileDeatils_Row("lblEmplAccountVisible") = True
                            'lblEmplAccountDisabled.Visible = True   19/07/2020
                        End If
                    End If  ' If lblEmplActiveStatusHide.Text = "N" Then
                End If
            End If
            UserProfileDeatils.Rows.Add(UserProfileDeatils_Row)
            UserProfileDeatils.TableName = "UserProfileDeatils"
            _result.Tables.Add(UserProfileDeatils)
            '19/07/2020 handle in the Front end
            'If Session("USERTYPEVALUE") = "C" Then
            '    If Session("ROLE") = "CORPADMIN" Then
            '        If roleDropdownList.SelectedValue <> "CORPADMIN" Then
            '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '        Else
            '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            '        End If
            '    End If
            'End If
            'If Session("USERTYPE") = "SUPER" Then
            '    If roleDropdownList.SelectedValue = "CORPADMIN" Then
            '        If Page_Action = "EDIT" Then
            '            MultiSiteChk.Visible = False
            '            PLGroup.Visible = False
            '        Else
            '            MultiSiteChk.Visible = True
            '            PLGroup.Visible = True
            '        End If
            '    Else
            '        MultiSiteChk.Visible = True
            '        PLGroup.Visible = True
            '    End If
            'End If
        Catch ex As Exception

        End Try
        'Return UserProfileBuildSet
    End Function

    Private Function IsUserCanReinstate(ByVal UserID As String, ByVal UserType As String, ByVal Session_SDIEMP As String) As Boolean
        Dim bCanReinstate As Boolean = False

        Dim user_terminate As String = ConfigurationSettings.AppSettings("Profile_inactivate_setting")
        Dim sAllowedUsers() As String = user_terminate.ToUpper.Split(",")

        Dim i As Integer = 0
        While i < sAllowedUsers.Length And Not bCanReinstate
            If UserID.ToString.ToUpper = sAllowedUsers(i) Then
                bCanReinstate = True
            End If
            i = i + 1
        End While

        If Not bCanReinstate Then
            If Not UserType Is Nothing And Not Session_SDIEMP Is Nothing Then
                If Convert.ToString(UserType).ToUpper().Equals("ADMIN") And Convert.ToString(Session_SDIEMP).ToUpper().Equals("CUST") Then
                    bCanReinstate = True
                ElseIf Convert.ToString(UserType).ToUpper().Equals("CORPADMIN") And Convert.ToString(Session_SDIEMP).ToUpper().Equals("CUST") Then
                    bCanReinstate = True
                End If
            End If
        End If
        'If Session("USERID") = user_terminate Then
        '    bCanReinstate = True
        'End If

        Return bCanReinstate
    End Function
    Private Function buildSelectDropDown(ByVal UserID As String, ByVal UserBu As String, ByVal UserROle As String, ByVal Session_ThirdParty_CompanyID As String, ByVal session_SDIEMP As String, ByVal UserType As String, ByVal IsVendor As Boolean)
        Dim dsORUsers As DataSet = GetSelectDropDownData(UserID, UserBu, UserROle, Session_ThirdParty_CompanyID, UserType, session_SDIEMP)
        'dropSelectUser.DataSource = dsORUsers
        'dropSelectUser.DataValueField = "ISA_EMPLOYEE_ID"
        'dropSelectUser.DataTextField = "USERANDBU" '"ISA_USER_NAME"
        ''dropSelectUser.DataTextField = "ISA_USER_NAME"
        'dropSelectUser.DataBind()

        'dropSelectUser.Items.Insert(0, New ListItem("<< Select User >>"))
        '17/07/2020 Move to the front End
        'rcbdropSelectUser.DataSource = dsORUsers
        'rcbdropSelectUser.DataValueField = "ISA_EMPLOYEE_ID"
        'rcbdropSelectUser.DataTextField = "USERANDBU"
        'rcbdropSelectUser.DataBind()

        'rcbdropSelectUser.Items.Insert(0, New RadComboBoxItem("<< Select User >>"))
        '---------ENd
        If IsVendor Then
            dsORUsers = buildSelectDropDownAP(UserID, UserBu, UserROle, UserType, session_SDIEMP)
        End If
        Return dsORUsers.Tables(0).Copy()
    End Function
    Private Function GetSelectDropDownData(ByVal UserID As String, ByVal UserBu As String, ByVal UserROle As String, ByVal Session_ThirdParty_CompanyID As String, ByVal UserType As String, ByVal session_SDIEMP As String) As DataSet
        Dim strSQLSelect As String
        Dim strSQLWhere As String
        Dim strSQLOrder As String
        Dim strSQLString As String
        Dim dsORUsers As New DataSet

        Try
            Dim SelectedValueforUserType As String = UserType
            '' strSQLSelect = "SELECT ISA_USER_NAME, BUSINESS_UNIT, ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL Where ISA_SDI_EMPLOYEE = '" & SelectedValueforUserType & "' AND ACTIVE_STATUS IN ('A','F') ORDER BY ISA_USER_NAME"
            strSQLSelect = "select isa_user_name, isa_employee_id" & vbCrLf
            strSQLSelect = strSQLSelect & ", isa_user_name || ' - ' || business_unit || ' - ' || isa_employee_id || decode(active_status,'I',' - INACTIVE','F',' - FAILED LOGIN',' ')  as  userandbu" & vbCrLf
            strSQLSelect = strSQLSelect & " from sdix_users_tbl"

            'If Session("usertype") = "admin" Or _
            '    Session("usertype") = "adminx" Or _
            '    Session("usertype") = "adminr" Then
            '    strSQLWhere = " where business_unit = '" & Session("busunit") & "'" & _
            '            " and isa_sdi_employee = 'c'" & _
            '            " and isa_employee_actyp <> 'annou'"

            If UserROle = "ADMIN" Or UserROle = "CORPADMIN" Then
                strSQLWhere = " where business_unit = '" & UserBu & "'" &
                        " and isa_sdi_employee = 'C'" &
                        " and isa_employee_actyp <> 'annou'"
            Else
                strSQLWhere = " where isa_employee_actyp <> 'annou'"
            End If
            'If Not String.IsNullOrEmpty(Session_ThirdParty_CompanyID) Then
            '    If Not Session_ThirdParty_CompanyID = 0 Then

            '    End If

            'End If


            If UserROle = "ADMIN" Or UserROle = "CORPADMIN" Then
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

            If Not IsUserCanReinstate(UserID, UserROle, session_SDIEMP) Then
                ' if logged in user can reinstate an account, don't filter out by active_status; show all users regardless of status.
                ' if the logged in user cannot reinstate an account, just show active users and users who are locked out because of too many failed login attempts.
                strSQLOrder = " and active_status in ('" & clsUserTbl.ActiveStatus_Active & "','" & clsUserTbl.ActiveStatus_FailedLogin & "')"

            End If
            If UserROle = "ADMIN" And UserType = "C" Then
                strSQLOrder = "And isa_employee_actyp <> 'CORPADMIN'"
            End If

            If Not String.IsNullOrEmpty(Session_ThirdParty_CompanyID) Then
                If Not Session_ThirdParty_CompanyID = 0 Then
                    strSQLWhere = strSQLWhere + "and THIRDPARTY_COMP_ID = '" & Session_ThirdParty_CompanyID & "'"
                End If

            End If

            If UserROle = "ADMIN" Or UserROle = "CORPADMIN" Then
                strSQLOrder &= " or isa_employee_id = '" & UserID & "' order by isa_user_name"
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
    Private Function buildSelectDropDownAP(ByVal UserID As String, ByVal UserBu As String, ByVal UserROle As String, ByVal UserType As String, ByVal session_SDIEMP As String)
        'Me.drpuserAp.Visible = True 17/07/2020
        Dim strSQLSelect As String
        Dim strSQLWhere As String
        Dim strSQLOrder As String = ""
        Dim strSQLString As String

        strSQLSelect = "SELECT ISA_USER_NAME, ISA_EMPLOYEE_ID" & vbCrLf &
                        " FROM SDIX_USERS_TBL"
        strSQLWhere = " WHERE  ISA_SDI_EMPLOYEE = 'S'"
        If Not IsUserCanReinstate(UserID, UserROle, session_SDIEMP) Then
            ' If the logged in user cannot activate/inactivate an account, filter out inactive users.
            ' If the logged in user can activate/inactivate, let them see all accounts unrestricted by active status.
            strSQLOrder = "  and ACTIVE_STATUS IN ('" & clsUserTbl.ActiveStatus_Active & "','" & clsUserTbl.ActiveStatus_FailedLogin & "')" & vbCrLf
        End If
        strSQLOrder &= " ORDER BY ISA_USER_NAME"

        strSQLString = strSQLSelect & strSQLWhere & strSQLOrder

        Dim dsORUsers As DataSet = ORDBData.GetAdapter(strSQLString)
        '17/07/2020 Binding should be done as the front end
        'Me.drpuserAp.DataSource = dsORUsers
        'Me.drpuserAp.DataValueField = "ISA_EMPLOYEE_ID"
        'Me.drpuserAp.DataTextField = "ISA_USER_NAME"
        'Me.drpuserAp.DataBind()

        'Me.drpuserAp.Items.Insert(0, New ListItem("<< Select User >>"))
        '-------------End

    End Function

    Private Function buildGroupList(ByVal User_Role As String, ByVal UserBU As String)
        Dim strSQLString As String
        Dim SiteTable As New DataTable
        If User_Role = "CORPADMIN" Then
            Dim dsBUSisterSites As DataSet
            'If Trim(ViewState("BU")) <> "" Then 19/07/2020
            dsBUSisterSites = UnilogORDBData.SisterBusinessUnits(UserBU)
            SiteTable = dsBUSisterSites.Tables(0).Copy()
            'Else
            'dsBUSisterSites = UnilogORDBData.SisterBusinessUnits(Session("BUSUNIT")) 19/07/2020
            'End If
            '9/07/2020 Handle in the Front end. The below code is used for binding.
            'If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
            '    rcbUserGroups.Visible = True
            '    rcbUserGroups.DataSource = dsBUSisterSites
            '    rcbUserGroups.DataTextField = "DESCRIPTION"
            '    rcbUserGroups.DataValueField = "BUSINESS_UNIT"
            '    rcbUserGroups.DataBind()
            '    rcbUserGroups.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
            '    rcbUserGroups.DataValueField.Insert(0, "0")
            'Else
            '    rcbUserGroups.Visible = True
            '    rcbUserGroups.DataSource = dsBUSisterSites
            '    rcbUserGroups.DataTextField = "DESCRIPTION"
            '    rcbUserGroups.DataValueField = "BUSINESS_UNIT"
            '    rcbUserGroups.DataBind()
            '    rcbUserGroups.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
            '    rcbUserGroups.DataValueField.Insert(0, "0")
            '    If Trim(ViewState("BU")) <> "" Then
            '        rcbUserGroups.Items.Insert(1, New RadComboBoxItem(ViewState("BU"), ViewState("BU")))
            '        rcbUserGroups.DataValueField.Insert(0, ViewState("BU"))
            '    Else
            '        rcbUserGroups.Items.Insert(1, New RadComboBoxItem(Session("BUSUNIT"), Session("BUSUNIT")))
            '        rcbUserGroups.DataValueField.Insert(0, Session("BUSUNIT"))
            '    End If
            'End If
        Else
            strSQLString = "SELECT  A.ISA_BUSINESS_UNIT as groupid,A.ISA_BUSINESS_UNIT || ' - ' || E.descr  as  groupname " & vbCrLf &
                      " FROM  SYSADM8.PS_ISA_ENTERPRISE A, SYSADM8.PS_LOCATION_TBL B, PS_BUS_UNIT_TBL_FS E " & vbCrLf &
                      " WHERE  B.location =  'L'|| substr(A.ISA_BUSINESS_UNIT,2) || '-01'" & vbCrLf &
                      " AND A.BU_STATUS = '1' " & vbCrLf &
                      " AND B.EFFDT =" & vbCrLf &
                      " (SELECT MAX(A_ED.EFFDT) FROM PS_LOCATION_TBL A_ED " & vbCrLf &
                      " WHERE B.SETID = A_ED.SETID" & vbCrLf &
                      " AND B.LOCATION = A_ED.LOCATION" & vbCrLf &
                      " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf &
                      " AND E.BUSINESS_UNIT (+) = A.ISA_BUSINESS_UNIT" & vbCrLf &
                      " ORDER BY A.ISA_BUSINESS_UNIT "
            Dim dsDQLGroups As DataSet = ORDBData.GetAdapter(strSQLString)
            SiteTable = dsDQLGroups.Tables(0).Copy()
            'Binding should be handled in the front end. 19/07/2020
            'rcbUserGroups.DataSource = dsDQLGroups
            'rcbUserGroups.DataValueField = "groupid"
            'rcbUserGroups.DataTextField = "groupname"
            'rcbUserGroups.DataBind()
            'rcbUserGroups.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
            'rcbUserGroups.DataValueField.Insert(0, "0")
        End If
        Return SiteTable
    End Function

    Private Function GetMultiBusinessUnit(ByVal businessUnit As String)
        Try
            'Dim query As String = "SELECT Distinct ClientSite.SITE_ID AS BUSINESS_UNIT,ClientSite.SITE_ID || '-' || ClentSiteDesc.Descr AS DESCRIPTION " & vbCrLf & _
            '                                                   "FROM Bidwadmin.dw_client_site_loc ClientSite JOIN PS_BUS_UNIT_TBL_FS ClentSiteDesc ON " & vbCrLf & _
            '                                                   "ClientSite.SITE_ID=ClentSiteDesc.business_unit WHERE CLIENT =(SELECT Distinct CLIENT FROM Bidwadmin.dw_client_site_loc " & vbCrLf & _
            '                                                   "WHERE SITE_ID= 'I0914' AND site_status = 'Active' AND client != 'Inactive Client Sites') AND SITE_STATUS='Active' " & vbCrLf & _
            '                                                   "AND ClientSite.SITE_ID != 'ISA00'"
            Dim ds As DataSet = UnilogORDBData.SisterBusinessUnits(businessUnit)
            Return ds.Tables(0).Copy()

            'If ds.Tables(0).Rows.Count > 0 Then
            '    rcbMultiSelect.Visible = True
            '    rcbMultiSelect.DataSource = ds
            '    rcbMultiSelect.DataTextField = "DESCRIPTION"
            '    rcbMultiSelect.DataValueField = "BUSINESS_UNIT"
            '    rcbMultiSelect.DataBind()
            'Else
            '    rcbMultiSelect.Visible = False
            '    Dim strScript As String = "<script language='javascript'>alert('Sister sites are not available for this selected BU:' + '" + businessUnit + "');</script>"
            '    ScriptManager.RegisterClientScriptBlock(Me.Page, Me.[GetType](), "PopupScript", strScript, False)
            '    MultiSiteChk.Checked = False
            'End If
        Catch ex As Exception

        End Try
    End Function
    Private Function GetSisterSitesBU(ByVal businessUnit As String)
        Dim SisterSite As New DataTable
        SisterSite.TableName = "SisterSite"
        Try
            Dim dsBUSisterSites As DataSet = UnilogORDBData.SisterBusinessUnits(businessUnit)
            If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
                SisterSite = dsBUSisterSites.Tables(0).Copy()
                '19/07/2020 Binding should be handled in the front end
                'rcbGroup.Visible = True
                'rcbGroup.DataSource = dsBUSisterSites
                'rcbGroup.DataTextField = "DESCRIPTION"
                'rcbGroup.DataValueField = "BUSINESS_UNIT"
                'rcbGroup.DataBind()
                'rcbGroup.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
                'rcbGroup.DataValueField.Insert(0, "0")
            Else
                dsBUSisterSites = GetBUDesc(businessUnit)
                If Not dsBUSisterSites Is Nothing Then
                    If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
                        SisterSite = dsBUSisterSites.Tables(0).Copy()
                        '19/07/2020 Handle in the front end
                        'rcbGroup.Visible = True
                        'rcbGroup.DataSource = dsBUSisterSites
                        'rcbGroup.DataTextField = "DESCRIPTION"
                        'rcbGroup.DataValueField = "BUSINESS_UNIT"
                        'rcbGroup.DataBind()
                        ''rcbGroup.Items.Insert(0, New RadComboBoxItem("CSC Agent", "0"))
                        ''rcbGroup.DataValueField.Insert(0, "0")
                        'Else
                        '    rcbGroup.Visible = False
                        '    Dim strScript As String = "<script language='javascript'>alert('Sister sites are not available for this selected BU:' + '" + businessUnit + "');</script>"
                        '    ScriptManager.RegisterClientScriptBlock(Me.Page, Me.[GetType](), "PopupScript", strScript, False)
                        '    MultiSiteChk.Checked = False
                    End If
                    '19/07/2020 Handle in the front end
                    'Else
                    '    rcbGroup.Visible = False
                    '    Dim strScript As String = "<script language='javascript'>alert('Sister sites are not available for this selected BU:' + '" + businessUnit + "');</script>"
                    '    ScriptManager.RegisterClientScriptBlock(Me.Page, Me.[GetType](), "PopupScript", strScript, False)
                    '    MultiSiteChk.Checked = False
                End If
            End If
            Return SisterSite
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Private Function GetBUDesc(ByVal strBU As String) As DataSet
        Try
            Dim QueryGetCurrentBU As String = "SELECT * FROM (SELECT  A.ISA_BUSINESS_UNIT as BUSINESS_UNIT,A.ISA_BUSINESS_UNIT || ' - ' || B.descr  as  DESCRIPTION " & vbCrLf &
                "FROM  SYSADM8.PS_ISA_ENTERPRISE A, SYSADM8.PS_LOCATION_TBL B " & vbCrLf &
                "WHERE  B.location =  'L'|| substr(A.ISA_BUSINESS_UNIT,2) || '-01' AND A.BU_STATUS = '1' " & vbCrLf &
                "AND B.EFFDT = (SELECT MAX(A_ED.EFFDT) FROM PS_LOCATION_TBL A_ED WHERE B.SETID = A_ED.SETID " & vbCrLf &
                "AND B.LOCATION = A_ED.LOCATION AND A_ED.EFFDT <= SYSDATE) " & vbCrLf &
                "ORDER BY A.ISA_BUSINESS_UNIT) WHERE BUSINESS_UNIT = '" & strBU & "'"
            Dim dsBUData As DataSet
            dsBUData = ORDBData.GetAdapter(QueryGetCurrentBU)
            Return dsBUData
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
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
    Public Function BuildDeptDropdown()

        Dim DeptDataTable As New DataTable
        Try
            Dim strGetDeptDropdown = "SELECT * FROM SDIX_TCKT_DEPT"
            Dim dsDeptData As DataSet
            dsDeptData = ORDBData.GetAdapter(strGetDeptDropdown)

            DeptDataTable.TableName = "DeptDataTable"
            DeptDataTable = dsDeptData.Tables(0).Copy()
            'If Not dsDeptData Is Nothing Then
            '    If dsDeptData.Tables(0).Rows.Count > 0 Then
            '        rcbDept.DataSource = dsDeptData.Tables(0)
            '        rcbDept.DataValueField = "DEPT_ID"
            '        rcbDept.DataTextField = "DEPT_NAME"
            '        rcbDept.DataBind()
            '        rcbDept.Items.Insert(0, New RadComboBoxItem("Select Department", "0"))
            '        rcbDept.DataValueField.Insert(0, "")
            '    Else

            '    End If
            'Else

            'End If
        Catch ex As Exception

        End Try
        Return DeptDataTable
    End Function
    '**
    '* Ref: UP_PC_181
    '* 
    '* In this Method,Search user drop down value are returned
    '* This method is triggered when the User Type radio button is checked.
    Public Function radioUserType_SelectedIndexChanged(ByVal data As String) As String
        Try
            Dim _result As New DataSet
            If Not String.IsNullOrEmpty(data) Then
                Dim objUserTypeChangedBO As UserTypeChangeBO = JsonConvert.DeserializeObject(Of UserTypeChangeBO)(data)
                Dim Page_Action As String = objUserTypeChangedBO.Page_Action
                Dim Seleted_UserType As String = objUserTypeChangedBO.Selected_UserType
                Dim Login_UserRole As String = objUserTypeChangedBO.Session_UserRole
                Dim SisterSiteTable As New DataTable
                SisterSiteTable.TableName = "SisterSiteTable"
                Dim SearchDropDownTable As New DataTable
                SearchDropDownTable.TableName = "SearchDropDownTable"
                '19/07/2020 Not necessary
                '' ErrorMsgforRoleField.Visible = False
                'lblMessage.Text = ""
                ''lblMessage.Visible = False
                ''Error_LabelFname.Text = ""
                ''Error_LabelLname.Text = ""
                'lblSelectUser.Visible = True
                'Error_VendorID.Text = ""
                'Error_LabelSite.Text = ""
                ''buildSelectDropDown()
                'roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                If Trim(Page_Action) = "ADD" Then
                    'lblSelectUser.Visible = False '19/07/2020 Move the Hide and show to the front end
                    ''dropSelectUser.Visible = False
                    'rcbdropSelectUser.Visible = False
                    ''TextBox_VendorId.Visible = False
                    'Label_VendorID.Visible = False
                    'lblPassword.Visible = True
                    'lblConfirm.Visible = True
                    ''tr_BU_unit_field.Style.Add("display", "none")
                    'tr_Site_details_fields.Style.Add("display", "none")

                    If Seleted_UserType = "V" Then
                        '19/07/2020 In the below code entitlement in done which should be carried out to the front end
                        'tr_Multiselect_fields.Style.Add("display", "none")
                        ''TextBox_VendorId.Visible = True
                        'Label_VendorID.Visible = True
                        'tr_BU_unit_field.Style.Remove("display")
                        'drpBUnit.Visible = True
                        'lblBusUnit.Visible = True
                        'Radcombobox_vendorID() ' This method Not need because only getting the vendor id list but not used
                        'MainvndrID.Visible = True
                        'lblGroup.Visible = False
                        'rcbGroup.Visible = False
                        'MultiSiteChk.Visible = False
                        'roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                    ElseIf Seleted_UserType = "C" Then
                        'RadcomboforVendorID.Visible = False
                        '19/07/2020 In the below code entitlement in done which should be carried out to the front end
                        'MainvndrID.Visible = False
                        ''TextBox_VendorId.Visible = False
                        'Label_VendorID.Visible = False
                        'drpBUnit.Visible = False
                        'lblBusUnit.Visible = False
                        'tr_Site_details_fields.Style.Remove("display")
                        'tr_Multiselect_fields.Style.Remove("display")
                        'tr_BU_unit_field.Style.Add("display", "none")
                        'lblGroup.Visible = True
                        'rcbGroup.Visible = True
                        'MultiSiteChk.Visible = True
                        'roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        'roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        'Error_VendorID.Visible = False
                        'Error_VendorID.Text = ""
                        SisterSiteTable = buildGroupList(Login_UserRole, objUserTypeChangedBO.Session_BUSUNIT)
                        '19/07/2020 We have this flag value in the front end.
                        'Dim Site_Flag As String = GetFlagValue(rcbGroup.SelectedValue)
                        'If Site_Flag = "V" Then
                        '    txtUserid.ReadOnly = False
                        '    txtUserid.BackColor = White
                        '    txtUserid.Enabled = False
                        'End If
                    Else
                        '19/07/2020 In the below code entitlement in done which should be carried out to the front end
                        'tr_BU_unit_field.Style.Add("display", "none")
                        ''RadcomboforVendorID.Visible = False
                        'MainvndrID.Visible = False
                        ''TextBox_VendorId.Visible = False
                        'Label_VendorID.Visible = False
                        'drpBUnit.Visible = False
                        'lblBusUnit.Visible = False
                        'lblGroup.Visible = True
                        'rcbGroup.Visible = True
                        'roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                        'Error_VendorID.Visible = False
                        'Error_VendorID.Text = ""
                        'tr_Multiselect_fields.Style.Remove("display")
                        'MultiSiteChk.Visible = True
                        'tr_Site_details_fields.Style.Remove("display")
                        SisterSiteTable = buildGroupList(Login_UserRole, objUserTypeChangedBO.Session_BUSUNIT)
                        '19/07/2020 We have this flag value in the front end.
                        'Dim Site_Flag As String = GetFlagValue(rcbGroup.SelectedValue)
                        'If Site_Flag = "V" Then
                        '    txtUserid.ReadOnly = False
                        '    txtUserid.BackColor = White
                        '    txtUserid.Enabled = False
                        'End If
                    End If
                    '19/07/2020 In the below code entitlement in done which should be carried out to the front end
                    'btnAdd.Visible = False
                    'btnEdit.Visible = True
                ElseIf Trim(Page_Action) = "EDIT" Then
                    'Session("PageAction") = "EDIT" 19/07/2020
                    If Seleted_UserType = "V" Then
                        '19/07/2020 In the below code entitlement in done which should be carried out to the front end
                        'Radcombobox_vendorID()
                        ''RadcomboforVendorID.Visible = True
                        'MainvndrID.Visible = True
                        'rcbGroup.Visible = False
                        'tr_BU_unit_field.Style.Remove("display")
                        'drpBUnit.Visible = True
                        'lblBusUnit.Visible = True
                        'lblGroup.Visible = False
                        'Label_VendorID.Visible = True
                        'roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        'rcbGroup.Visible = False
                    Else
                        ''RadcomboforVendorID.Visible = False
                        '19/07/2020 In the below code entitlement in done which should be carried out to the front end
                        'MainvndrID.Visible = False
                        'rcbGroup.Visible = True
                        'tr_BU_unit_field.Style.Add("display", "none")
                        'drpBUnit.Visible = False
                        'lblBusUnit.Visible = False
                        'tr_Site_details_fields.Style.Remove("display")
                        'Label_VendorID.Visible = False
                    End If
                    '19/07/2020 In the below code entitlement in done which should be carried out to the front end
                    'If Seleted_UserType = "C" Then

                    '    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    '    roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                    'ElseIf radioUserType.SelectedValue = "S" Then
                    '    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    '    roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                    'End If
                    '19/07/2020 In the below code entitlement in done which should be carried out to the front end
                    'If Login_UserRole = "SUPER" Then
                    '    If Page_Action = "EDIT" Then
                    '        If radioUserType.SelectedValue = "S" Then
                    '            lblSelectUser.Visible = True
                    '            roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                    '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    '            lblDept.Visible = True
                    '        ElseIf Seleted_UserType = "V" Then
                    '            lblSelectUser.Visible = True
                    '            Label_VendorID.Visible = True
                    '            tr_BU_unit_field.Style.Remove("display")
                    '            drpBUnit.Visible = True
                    '            lblBusUnit.Visible = True
                    '            Radcombobox_vendorID()
                    '            'RadcomboforVendorID.Visible = True
                    '            MainvndrID.Visible = True
                    '            lblGroup.Visible = False
                    '            rcbGroup.Visible = False
                    '            MultiSiteChk.Visible = False
                    '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                    '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                    '            rcbMultiSelect.Visible = False
                    '        Else
                    '            lblSelectUser.Visible = True
                    '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                    '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                    '            lblDept.Visible = False
                    '        End If
                    '    Else

                    '    End If
                    'End If

                    'lblPassword.Visible = False
                    'lblConfirm.Visible = False
                    'radioUserType.SelectedValue = Session("SetClickValue_Usertype")
                    '=========================================End
                    SisterSiteTable = buildGroupList(Login_UserRole, objUserTypeChangedBO.Session_BUSUNIT)
                    '19/07/2020 The below code is used to select the Site drop down value with the Userid site Shown in the textbox.
                    '***Which we will have in the front end.
                    'Dim SQLStringQuery As String = "SELECT BUSINESS_UNIT, ISA_SDI_EMPLOYEE FROM SDIX_USERS_TBL WHERE ISA_EMPLOYEE_ID='" & Trim(txtUserid.Text) & "'"
                    'Dim dsbuvalue As DataSet = ORDBData.GetAdapter(SQLStringQuery)
                    'If Convert.ToString(dsbuvalue.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")) <> "V" Then
                    '    Try
                    '        rcbGroup.SelectedIndex = rcbGroup.FindItemByValue(dsbuvalue.Tables(0).Rows(0).Item("BUSINESS_UNIT")).Index
                    '    Catch ex As Exception

                    '    End Try
                    'End If

                    SearchDropDownTable = buildSelectDropDown(objUserTypeChangedBO.Session_USERID, objUserTypeChangedBO.Session_BUSUNIT, Login_UserRole, objUserTypeChangedBO.Session_ThirdParty_CompanyID, objUserTypeChangedBO.Session_SDIEMP, Seleted_UserType, objUserTypeChangedBO.Query_IsVendor)
                    'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
                    '19/07/2020 Move to the front end
                    'Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))

                Else

                End If
                '19/07/2020 Handle in the front end
                'If Page_Action = "ADD" Then
                '    If radioUserType.SelectedValue = "S" Then
                '        lblDept.Visible = True
                '        rcbDept.Visible = True
                '        tr_Dept_details_fields.Style.Remove("display")
                '    Else
                '        lblDept.Visible = False
                '        rcbDept.Visible = False
                '        tr_Dept_details_fields.Style.Add("display", "none")
                '    End If
                '    If Session("ROLE") = "SUPER" Then
                '        If radioUserType.SelectedValue = "S" Then
                '            MultiSiteChk.Visible = True
                '        End If
                '    End If
                'Else
                '    If radioUserType.SelectedValue = "S" Then
                '        lblDept.Visible = True
                '        rcbDept.Visible = True
                '        tr_Dept_details_fields.Style.Remove("display")
                '    Else
                '        lblDept.Visible = False
                '        rcbDept.Visible = False
                '        tr_Dept_details_fields.Style.Add("display", "none")
                '    End If
                'End If
                'If roleDropdownList.Items.FindByValue("CORPADMIN").Enabled Then
                '    MultiSiteChk.Visible = False
                '    PLMultiSelect.Visible = False
                'End If

                'If radioUserType.SelectedValue = "V" Or roleDropdownList.SelectedValue = "CORPADMIN" Then
                '    MultiSiteChk.Visible = False
                '    PLMultiSelect.Visible = False
                'Else
                '    MultiSiteChk.Visible = True
                '    PLMultiSelect.Visible = True
                'End If
                '======================End
                SisterSiteTable.TableName = "SisterSiteTable"
                SearchDropDownTable.TableName = "SearchDropDownTable"
                _result.Tables.Add(SisterSiteTable)
                _result.Tables.Add(SearchDropDownTable)
                data = JsonConvert.SerializeObject(_result)
            End If
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '**
    '* Ref: UP_PC_180
    '* 
    '* In this Method,Selected user details is returned
    '* This method is triggered when the User is selected in the Search user Drop down.
    Public Function rcbdropSelectUser_SelectedIndexChanged(ByVal data As String) As String
        Try
            Dim _result As New DataSet
            If Not String.IsNullOrEmpty(data) Then
                Dim objSearchUserBuildBO As SearchUserBuildBO = JsonConvert.DeserializeObject(Of SearchUserBuildBO)(data)
                Dim USERTYPEVALUE As String = objSearchUserBuildBO.Selected_UserType
                Dim User_ROLE As String = objSearchUserBuildBO.Session_UserRole
                'drpBUnit.Visible = False 19/07/2020
                'lblBusUnit.Visible = False
                'lblSelectUser.Visible = True
                'lblPassword.Visible = False
                'lblConfirm.Visible = False
                ''TextBox_VendorId.Text = ""
                ''Dim sss As String = dropSelectUser.SelectedValue
                'Dim sss As String = rcbdropSelectUser.SelectedValue
                'Dim selecteuserdrpdwn As String = Session("SelectUserDrpdwn")
                Dim SisterSiteTable As New DataTable
                SisterSiteTable.TableName = "SisterSiteTable"
                Dim GroupTable As New DataTable
                GroupTable.TableName = "GroupTable"
                GroupTable.Columns.Add("txtGroupID")
                GroupTable.Columns.Add("txtGroup")
                Dim Error_Table As New DataTable
                Error_Table.TableName = "Error_Table"
                Error_Table.Columns.Add("UserExist")
                If User_ROLE = "SUPER" Then

                    If objSearchUserBuildBO.Query_IsVendor Then

                    ElseIf objSearchUserBuildBO.Query_IsMexicoVendor Then

                    Else
                        SisterSiteTable = buildGroupList(User_ROLE, objSearchUserBuildBO.Session_BUSUNIT)

                        'buildGroupList(rcbGroupSites)
                        'tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True
                    End If
                ElseIf User_ROLE = "ADMIN" Or
                    User_ROLE = "ADMINX" Or
                    User_ROLE = "ADMINR" Or
                    User_ROLE = "CORPADMIN" Then
                    Dim intGroupid As String = getGroupID(objSearchUserBuildBO.Session_USERID)


                    Dim GroupTable_rows As DataRow = GroupTable.NewRow()
                    GroupTable_rows("txtGroupID") = intGroupid.ToString
                    GroupTable_rows("txtGroup") = getUserGroupsName(intGroupid)
                    GroupTable.Rows.Add(GroupTable_rows)

                    'txtGroup.ReadOnly = True  19/07/2020
                    'txtGroup.BackColor = LightGray
                    'tbStripUserDetails.Tabs.FindTabByValue("APP").Visible = True


                End If
                _result.Tables.Add(GroupTable)

                buildEditUser(objSearchUserBuildBO.Selected_UserID, objSearchUserBuildBO.Session_BUSUNIT, objSearchUserBuildBO.Session_SDIEMP, USERTYPEVALUE, User_ROLE, objSearchUserBuildBO.Query_IsMexicoVendor, Error_Table, _result, SisterSiteTable)
                SisterSiteTable.TableName = "SisterSiteTable"
                Error_Table.TableName = "Error_Table"
                _result.Tables.Add(SisterSiteTable)
                _result.Tables.Add(Error_Table)
                Dim ThirdPartytable As New DataTable
                Dim Userprofile As DataTable = _result.Tables("UserProfileDeatils")
                If Userprofile.Rows.Count > 0 Then
                    Dim SelectedUserBU As String = Userprofile.Rows(0)("Businees_Unit")
                    Dim SQLSTRINGQuery As String = ""

                    If Not String.IsNullOrEmpty(objSearchUserBuildBO.Session_ThirdParty_CompanyID) Then

                        If objSearchUserBuildBO.Session_ThirdParty_CompanyID = 0 Then
                            SQLSTRINGQuery = "Select * from SDIX_ThirdParty_Table where ISA_BUSINESS_UNIT = '" & SelectedUserBU & "'"
                        Else
                            SQLSTRINGQuery = "Select * from SDIX_ThirdParty_Table where ISA_BUSINESS_UNIT = '" & SelectedUserBU & "' And THIRDPARTY_COMP_ID = '" & objSearchUserBuildBO.Session_ThirdParty_CompanyID & "'"
                        End If

                        Dim dsOREmp As DataSet = ORDBData.GetAdapter(SQLSTRINGQuery)


                        ThirdPartytable = dsOREmp.Tables(0).Copy()

                    End If
                End If
                ThirdPartytable.TableName = "ThirdPartytable"
                _result.Tables.Add(ThirdPartytable)
                Dim DepartmentTable As New DataTable
                If USERTYPEVALUE = "S" Then
                    DepartmentTable = BuildDeptDropdown()
                End If
                DepartmentTable.TableName = "DepartmentTable"
                _result.Tables.Add(DepartmentTable)
                data = JsonConvert.SerializeObject(_result)
                'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Session("SelectUserDrpdwn")))
                '19/07/2020 Handle in the front end
                'Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Session("SelectUserDrpdwn")))
                'buildEditUser(dropSelectUser.SelectedValue)
                'buildSelectDropDown()
                '19/07/2020 Punch in Is not included for Version 1
                'If Session("PUNCHIN") = "YES" Then
                '    btnChangePassw.Visible = False
                'Else
                '    btnChangePassw.Visible = True
                'End If
                '19/07/2020 Entitlement should be handled in the front end
                'If USERTYPEVALUE = "S" Then
                '    If Session("ROLE") = "ADMIN" Then
                '        If radioUserType.SelectedValue <> "S" Then
                '            tr_Dept_details_fields.Style.Add("display", "none")
                '        Else
                '            tr_Dept_details_fields.Style.Remove("display")
                '        End If
                '    End If
                'End If
                'If radioUserType.SelectedValue = "V" Or roleDropdownList.SelectedValue = "CORPADMIN" Then
                '    MultiSiteChk.Visible = False
                '    PLMultiSelect.Visible = False
                'Else
                '    MultiSiteChk.Visible = True
                '    PLMultiSelect.Visible = True
                'End If

                'If Session("USERTYPE") = "SUPER" Then
                '    If Page_Action = "EDIT" Then
                '        If radioUserType.SelectedValue = "S" Then
                '            lblSelectUser.Visible = True
                '            roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                '            lblDept.Visible = True
                '        ElseIf radioUserType.SelectedValue = "V" Then
                '            lblSelectUser.Visible = True
                '            Label_VendorID.Visible = True
                '            tr_BU_unit_field.Style.Remove("display")
                '            drpBUnit.Visible = True
                '            lblBusUnit.Visible = True
                '            Radcombobox_vendorID()
                '            'RadcomboforVendorID.Visible = True
                '            MainvndrID.Visible = True
                '            lblGroup.Visible = False
                '            rcbGroup.Visible = False
                '            MultiSiteChk.Visible = False
                '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                '            rcbMultiSelect.Visible = False
                '        Else
                '            lblSelectUser.Visible = True
                '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                '            lblDept.Visible = False
                '        End If
                '    Else

                '    End If
                'ElseIf Session("USERTYPE") = "ADMIN" Then
                '    If Session("USERTYPEVALUE") = "S" Then
                '        drpBUnit.Visible = False
                '        lblBusUnit.Visible = False
                '        lblSelectUser.Visible = True
                '        'dropSelectUser.Visible = True
                '        rcbdropSelectUser.Visible = True
                '        lblGroup.Visible = True
                '        btnAdd.Visible = True
                '        lblPassword.Visible = False
                '        lblConfirm.Visible = False
                '        radioUserType.Visible = False
                '        Label_usrtype.Visible = False
                '        tr_PwdFields.Style.Add("display", "none")
                '        tr_CpwdFields.Style.Add("display", "none")
                '        tr_BU_unit_field.Style.Add("display", "none")
                '        MultiSiteChk.Visible = True
                '        rcbGroup.Visible = False
                '        If Page_Action = "ADD" Then
                '            lblGroup.Visible = True
                '            rcbGroup.Visible = False
                '            MultiSiteChk.Visible = True
                '        End If
                '        If Session("USERID") = Trim(rcbdropSelectUser.SelectedValue) Then
                '            lblDept.Visible = True
                '            rcbDept.Visible = True
                '        Else
                '            lblDept.Visible = False
                '            rcbDept.Visible = False
                '        End If
                '        If roleDropdownList.SelectedValue = "CORPADMIN" Then
                '            MultiSiteChk.Visible = False
                '        End If
                '    ElseIf Session("USERTYPEVALUE") = "C" Then
                '        If Page_Action = "EDIT" Then
                '            CustomersUsers()       Entitlement is handled
                '        Else

                '        End If
                '    End If
                'ElseIf Session("USERTYPE") = "CORPADMIN" Then
                '    If Page_Action = "EDIT" Then
                '        CustomersUsers()
                '        rcbGroup.Visible = True
                '        MultiSiteChk.Visible = False
                '        rcbMultiSelect.Visible = False
                '        If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                '        Else
                '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                '        End If
                '    Else

                '    End If
                'Else

                'End If
                '======================End
            End If

        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '* Ref: UP_PC_182
    '* 
    Public Function MainvndrID_TextChanged(ByVal data As String) As String
        Try
            Dim _result As New DataSet
            If Not String.IsNullOrEmpty(data) Then
                Dim objCheckVendorIdBO As CheckVendorIdBO = JsonConvert.DeserializeObject(Of CheckVendorIdBO)(data)
                Dim strQueryChk_Vendor As String = ""
                Dim dsVendorExist As New DataTable
                dsVendorExist.TableName = "dsVendorExist"
                dsVendorExist.Columns.Add("Vendor_CompanyName")
                Dim dsVendorExist_Rows As DataRow = dsVendorExist.NewRow()
                Dim Error_Table As New DataTable
                Error_Table.TableName = "Error_Table"
                Error_Table.Columns.Add("VendorId_NotExist")
                Dim Error_Table_Rows As DataRow = Error_Table.NewRow()
                Dim ChkVendorId As String = ""
                If Trim(objCheckVendorIdBO.Entered_VendorId) <> "" Then

                    ChkVendorId = Chk_VendrID(Trim(objCheckVendorIdBO.Entered_VendorId))
                    If ChkVendorId <> "" Then
                        dsVendorExist_Rows("Vendor_CompanyName") = "Vendor Name : " + ChkVendorId
                        dsVendorExist.Rows.Add(dsVendorExist_Rows)
                        'Error_VendorID.ForeColor = Color.Green 19/07/2020
                        'Error_VendorID.Visible = True
                    Else
                        Error_Table_Rows("VendorId_NotExist") = "Entered Vendor ID is Invalid"
                        Error_Table.Rows.Add(Error_Table_Rows)
                        'Error_VendorID.ForeColor = Color.Red 19/07/2020
                        'Error_VendorID.Visible = True
                    End If
                Else
                    Error_Table_Rows("VendorId_NotExist") = "Please enter Vendor ID"
                    Error_Table.Rows.Add(Error_Table_Rows)
                    'Error_VendorID.ForeColor = Color.Red  19/07/2020
                    'Error_VendorID.Visible = True
                End If
                _result.Tables.Add(dsVendorExist)
                _result.Tables.Add(Error_Table)
                data = JsonConvert.SerializeObject(_result)
            End If
            '19/07/2020 
            'If Page_Action = "EDIT" Then
            '    lblSelectUser.Visible = True
            '    Label_VendorID.Visible = True
            '    tr_BU_unit_field.Style.Remove("display")
            '    drpBUnit.Visible = True
            '    lblBusUnit.Visible = True
            '    Radcombobox_vendorID()
            '    'RadcomboforVendorID.Visible = True
            '    MainvndrID.Visible = True
            '    lblGroup.Visible = False
            '    rcbGroup.Visible = False
            '    MultiSiteChk.Visible = False
            '    roleDropdownList.Items.FindByValue("SUPER").Enabled = False
            '    roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '    rcbMultiSelect.Visible = False
            'Else

            'End If
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty

        End Try
        Return data
    End Function
    Public Function SiteBased_ThirdParty(ByVal data As String)
        Dim _result As New DataSet
        Try
            If Not String.IsNullOrEmpty(data) Then
                Dim objSiteBasedThirdPartyBo As SiteBasedThirdPartyBo = JsonConvert.DeserializeObject(Of SiteBasedThirdPartyBo)(data)
                Dim SQLSTRINGQuery As String = ""
                Dim ThirdPartytable As New DataTable
                Dim Multisite As New DataTable
                Dim MultiSite_Checked As New DataTable
                If Not String.IsNullOrEmpty(objSiteBasedThirdPartyBo.Session_ThirdParty_CompanyID) Then

                    If objSiteBasedThirdPartyBo.Session_ThirdParty_CompanyID = 0 Then
                        SQLSTRINGQuery = "Select * from SDIX_ThirdParty_Table where ISA_BUSINESS_UNIT = '" & objSiteBasedThirdPartyBo.SiteValue & "'"
                    Else
                        SQLSTRINGQuery = "Select * from SDIX_ThirdParty_Table where ISA_BUSINESS_UNIT = '" & objSiteBasedThirdPartyBo.SiteValue & "' And THIRDPARTY_COMP_ID = '" & objSiteBasedThirdPartyBo.Session_ThirdParty_CompanyID & "'"
                    End If

                    Dim dsOREmp As DataSet = ORDBData.GetAdapter(SQLSTRINGQuery)


                    ThirdPartytable = dsOREmp.Tables(0).Copy()

                End If
                ThirdPartytable.TableName = "ThirdPartytable"
                _result.Tables.Add(ThirdPartytable)
                If objSiteBasedThirdPartyBo.Page_Action = "EDIT" Then
                    SQLSTRINGQuery = "SELECT ISA_USER_ID, FIRST_NAME_SRCH," & vbCrLf &
                                " LAST_NAME_SRCH, ISA_EMPLOYEE_EMAIL," & vbCrLf &
                                " ISA_EMPLOYEE_ID, PHONE_NUM, ISA_EMPLOYEE_ACTYP," & vbCrLf &
                                " ISA_SDI_EMPLOYEE, BUSINESS_UNIT, active_status,Multi_Site, ISA_VENDOR_ID, TCKTDEPT,THIRDPARTY_COMP_ID" & vbCrLf &
                                " FROM SDIX_USERS_TBL" & vbCrLf &
                                " WHERE ISA_EMPLOYEE_ID = '" & objSiteBasedThirdPartyBo.SelectedUserId & "'" & vbCrLf
                    Dim dsOREmp As DataSet = ORDBData.GetAdapter(SQLSTRINGQuery)
                    If dsOREmp.Tables.Count > 0 Then
                        If dsOREmp.Tables(0).Rows.Count > 0 Then
                            If dsOREmp.Tables(0).Rows(0).Item("MULTI_SITE").ToString() = "Y" And objSiteBasedThirdPartyBo.SiteValue = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT") Then

                                If dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT") <> "SDM00" Then

                                    Multisite = GetMultiBusinessUnit(dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT"))

                                End If
                                Dim strQuery As String = "Select BUSINESS_UNIT FROM SDIX_MULTI_SITE WHERE ISA_EMPLOYEE_ID='" & objSiteBasedThirdPartyBo.SelectedUserId & "'"
                                Dim dsMultiBU As DataSet = ORDBData.GetAdapter(strQuery)


                                MultiSite_Checked = dsMultiBU.Tables(0).Copy()

                            End If

                        End If
                    End If
                End If
                Multisite.TableName = "Multisite"
                _result.Tables.Add(Multisite)
                MultiSite_Checked.TableName = "MultiSite_Checked"
                _result.Tables.Add(MultiSite_Checked)
            End If
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '* Ref: UP_PC_203
    '* 
    Public Function MultiSiteChk_CheckedChanged(ByVal data As String) As String
        Try
            '19/07/2020
            'rcbMultiSelect.ID = "rcbMultiSelect"
            'rcbMultiSelect.CheckBoxes = True
            'rcbMultiSelect.EnableCheckAllItemsCheckBox = True
            'rfvMultiBU.ID = "rfvMultiBU"
            'rcbMultiSelect.CssClass = "MultiselectChkbox"
            'rfvMultiBU.ControlToValidate = "rcbMultiSelect"
            'rfvMultiBU.ErrorMessage = "Select BU for Multi Site Access"
            'rfvMultiBU.CssClass = "usergroup-field"
            'PLMultiSelect.Controls.Add(rcbMultiSelect)
            Dim _result As New DataSet
            If Not String.IsNullOrEmpty(data) Then
                Dim objMultiSiteCheckedBo As MultiSiteCheckedBo = JsonConvert.DeserializeObject(Of MultiSiteCheckedBo)(data)
                Dim USERTYPEVALUE As String = objMultiSiteCheckedBo.Selected_UserType
                Dim User_Role As String = objMultiSiteCheckedBo.Session_UserRole
                Dim BU As String = objMultiSiteCheckedBo.SelectedUserBu
                Dim MultiSite As New DataTable
                MultiSite.TableName = "MultiSite"
                If objMultiSiteCheckedBo.MultiSiteChecked Then

                    If objMultiSiteCheckedBo.Query_Mexico = "YES" Then
                        'drpBUnit.Visible = False 19/07/2020
                        'lblBusUnit.Visible = False
                        'Dim BU As String = dropSelectUser.SelectedItem.Text.Split("-")(1).Trim()
                        'Dim BU As String = rcbdropSelectUser.SelectedItem.Text.Split("-")(1).Trim() 19/07/2020

                        MultiSite = GetMultiBusinessUnit(BU)
                        'SelectBaseBU(BU) 19/07/2020 In this method the primary bu of the user is Checked and enable false
                    Else
                        If USERTYPEVALUE = "C" Or USERTYPEVALUE = "S" Then
                            If User_Role = "ADMIN" Or User_Role = "CORPADMIN" Then
                                MultiSite = GetMultiBusinessUnit(BU)
                                'SelectBaseBU(txtGroupID.Text) 19/07/2020 In this method the primary bu of the user is Checked and enable false
                            ElseIf User_Role = "USER" Then
                                MultiSite = GetMultiBusinessUnit(BU)
                                'SelectBaseBU(txtGroupID.Text) 19/07/2020 In this method the primary bu of the user is Checked and enable false
                            Else
                                MultiSite = GetMultiBusinessUnit(BU)
                                'SelectBaseBU(rcbGroup.SelectedValue) 19/07/2020 In this method the primary bu of the user is Checked and enable false
                            End If
                        Else
                            MultiSite = GetMultiBusinessUnit(BU)
                            'SelectBaseBU(rcbGroup.SelectedValue) 19/07/2020 In this method the primary bu of the user is Checked and enable false
                        End If
                    End If
                End If
                '19/07/2020 In the below code MultiSite is UnChecked 
                'Else
                '    If Request.QueryString("MEXICO") = "YES" Then
                '        rcbMultiSelect.Visible = False
                '        rcbMultiSelect.ClearCheckedItems()
                '    Else
                '        'rcbGroup.Visible = True
                '        'valGroup.Visible = True
                '        rcbMultiSelect.Visible = False
                '        rcbMultiSelect.ClearCheckedItems()

                '    End If
                _result.Tables.Add(MultiSite)
                data = JsonConvert.SerializeObject(_result)
            End If
            '19/07/2020 Entitlement should be handled in the front end
            'If Session("USERTYPE") = "SUPER" Then
            '    If Page_Action = "EDIT" Then
            '        If radioUserType.SelectedValue = "S" Then
            '            lblSelectUser.Visible = True
            '            roleDropdownList.Items.FindByValue("SUPER").Enabled = True
            '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '            lblDept.Visible = True
            '        ElseIf radioUserType.SelectedValue = "V" Then
            '            lblSelectUser.Visible = True
            '            Label_VendorID.Visible = True
            '            tr_BU_unit_field.Style.Remove("display")
            '            drpBUnit.Visible = True
            '            lblBusUnit.Visible = True
            '            Radcombobox_vendorID()
            '            'RadcomboforVendorID.Visible = True
            '            MainvndrID.Visible = True
            '            lblGroup.Visible = False
            '            rcbGroup.Visible = False
            '            MultiSiteChk.Visible = False
            '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
            '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
            '            lblDept.Visible = False
            '        Else
            '            lblSelectUser.Visible = True
            '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
            '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            '            lblDept.Visible = False
            '        End If
            '    Else

            '    End If
            'ElseIf Session("USERTYPE") = "ADMIN" Then
            '    If Session("USERTYPEVALUE") = "S" Then
            '        drpBUnit.Visible = False
            '        lblBusUnit.Visible = False
            '        lblSelectUser.Visible = True
            '        'dropSelectUser.Visible = True
            '        rcbdropSelectUser.Visible = True
            '        lblGroup.Visible = True
            '        btnAdd.Visible = True
            '        lblPassword.Visible = False
            '        lblConfirm.Visible = False
            '        radioUserType.Visible = False
            '        Label_usrtype.Visible = False
            '        tr_PwdFields.Style.Add("display", "none")
            '        tr_CpwdFields.Style.Add("display", "none")
            '        tr_BU_unit_field.Style.Add("display", "none")
            '        MultiSiteChk.Visible = True
            '        rcbGroup.Visible = False
            '        If Page_Action = "ADD" Then
            '            lblGroup.Visible = True
            '            rcbGroup.Visible = False
            '            MultiSiteChk.Visible = True
            '            MainvndrID.Visible = False
            '            radioUserType.SelectedValue = "C"
            '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
            '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
            '            radioUserType.Enabled = False
            '            txtUserid.Enabled = False
            '            lblGroup.Visible = True
            '            rcbGroup.Visible = False
            '            MultiSiteChk.Visible = True
            '            lblBusUnit.Visible = False
            '            drpBUnit.Visible = False
            '            roleDropdownList.Enabled = True
            '            radioUserType.Visible = False
            '            Label_usrtype.Visible = False
            '            If Session("Flag_AddUser") = "V" Then
            '                txtUserid.ReadOnly = False
            '                txtUserid.BackColor = White
            '                txtUserid.Enabled = True
            '            End If
            '            lblDept.Visible = False
            '            rcbDept.Visible = False
            '            tr_Dept_details_fields.Style.Add("display", "none")
            '            tr_PwdFields.Style.Remove("display")
            '            tr_CpwdFields.Style.Remove("display")
            '            lblPassword.Visible = True
            '            txtPassword.Visible = True
            '            lblConfirm.Visible = True
            '            txtConfirm.Visible = True
            '            btnAdd.Visible = False
            '            btnEdit.Visible = True
            '        Else

            '        End If
            '    ElseIf Session("USERTYPEVALUE") = "C" Then
            '        If Page_Action = "EDIT" Then
            '            CustomersUsers()
            '        Else
            '            lblGroup.Visible = True
            '            rcbGroup.Visible = False
            '        End If
            '    End If
            'ElseIf Session("USERTYPE") = "USER" Then
            '    If radioUserType.SelectedValue = "S" Then
            '        SDIUsers()
            '    ElseIf radioUserType.SelectedValue = "C" Then
            '        CustomersUsers()
            '    End If
            'End If

            'If btnChangePassw.Visible = True Then
            '    lblPassword.Visible = False
            '    lblConfirm.Visible = False
            'Else
            '    lblPassword.Visible = True
            '    lblConfirm.Visible = True
            'End If

        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '* Ref: UP_PC_179 &  UP_PC_183
    '* 
    Public Function btnSave_Click(ByVal data As String) As String
        '19/07/2020 Handle in the front end
        'Dim strMessage As New Alert
        'If Request.ServerVariables("HTTP_HOST").ToString().ToUpper.Substring(0, 6) = "CPTEST" And
        '    Session("USERNAME").toupper = "SDI TEMP USER" Then
        '    ltlAlert.Text = strMessage.Say("Warning - Profile update has been disabled in test")
        '    Exit Sub
        'End If
        'If Session("USERNAME").toupper = "USER NAME" Then
        '    ltlAlert.Text = strMessage.Say("Warning - Profile update has been disabled for User DEMO")
        '    Exit Sub
        'End If


        'Exit Sub
        Try

            Dim _result As New DataSet
            If Not String.IsNullOrEmpty(data) Then
                Dim objSaveUserProfileBO As SaveUserProfileBO = JsonConvert.DeserializeObject(Of SaveUserProfileBO)(data)
                Dim Error_Table As New DataTable
                Error_Table.Columns.Add("PasswordExist")
                Error_Table.Columns.Add("UserIdExist")
                Error_Table.Columns.Add("UserIdFormatError")
                Error_Table.Columns.Add("InvalidUserId")
                Error_Table.Columns.Add("Error_Upd_ISA_USERS_TBL")
                Error_Table.Columns.Add("Error_Upd_ISA_ISOL_PW")
                Error_Table.Columns.Add("Error_Upd_PS_ISA_EMPL_TBL")
                Error_Table.Columns.Add("Error_Upd_ISA_USERS_PRIVS")
                Dim Error_TableRow As DataRow = Error_Table.NewRow()
                Dim Success_Message As New DataTable
                Success_Message.Columns.Add("AddUserMessage")
                Success_Message.Columns.Add("EditUserMessage")
                Dim Success_MessageRow As DataRow = Success_Message.NewRow()
                Dim strUserGroup As String = ""
                Dim strRndCplusPassw As String = ""
                Dim strUserType As String = ""
                Dim strSDICust As String = ""
                Dim strSQLString As String = ""
                Dim strSQLPW As String = ""
                Dim strPasswEncrp As String = ""
                Dim strSQLUPD1 As String = ""
                Dim strSQLUPD2 As String = ""
                Dim strPasswUpdate As String = ""
                Dim strVendorIdvalue As String = ""
                Dim MultiSiteAccess As String = ""
                Dim dteNow As Date
                dteNow = Now().ToString("d")

                Dim strFirst As String = Trim(objSaveUserProfileBO.FirstName)
                strFirst = Replace(strFirst, "'", "")
                Dim strLast As String = Trim(objSaveUserProfileBO.LastName)
                strLast = Replace(strLast, "'", "")
                Dim strFullName As String = strLast & "," & strFirst
                Dim strFullName40 As String = strLast & "," & strFirst
                If strFullName.Length > 50 Then
                    strFullName = strFullName.Substring(0, 50)
                End If
                If strFullName40.Length > 40 Then
                    strFullName40 = strFullName40.Substring(0, 40)
                End If
                'Added ShiptoDefault value as a input to update SHIPTO_DEFAULT column
                Dim shipto_default As String = " "
                If objSaveUserProfileBO.Shipto_Default IsNot Nothing And objSaveUserProfileBO.Shipto_Default <> "" Then
                    shipto_default = objSaveUserProfileBO.Shipto_Default
                End If

                'strVendorIdvalue = Trim(TextBox_VendorId.Text)
                strVendorIdvalue = Trim(objSaveUserProfileBO.VendorID)
                Dim strPhone As String = objSaveUserProfileBO.UserPhoneNum
                strPhone = Replace(strPhone, "(", "")
                strPhone = Replace(strPhone, ")", "")
                strPhone = Replace(strPhone, " ", "-")
                'Dim strUSERID As String = GetDisplayedUserID()
                Dim strUSERID As String = String.Empty
                Dim UserIdValidate As Boolean = valUserid2_ServerValidate(objSaveUserProfileBO.SiteValue, objSaveUserProfileBO.ManuallyEntered_UserId, Error_Table)
                If UserIdValidate Then
                    _result.Tables.Add(Error_Table)
                    _result.Tables.Add(Success_Message)
                    data = JsonConvert.SerializeObject(_result)
                    Return data
                    Exit Function
                End If
                Dim ThirdParty_CompanyID As Int16
                'If objSaveUserProfileBO.ThirdParty_CompanyID = "" Or IsDBNull(objSaveUserProfileBO.ThirdParty_CompanyID) Then
                '    ThirdParty_CompanyID = 0
                'End If
                ThirdParty_CompanyID = objSaveUserProfileBO.ThirdParty_CompanyID
                If objSaveUserProfileBO.UserType <> "V" Then
                    If objSaveUserProfileBO.Session_Role = "SUPER" Then
                        Dim Site_Flag As String = GetFlagValue(objSaveUserProfileBO.SiteValue)
                        If Site_Flag = "A" Then
                            If objSaveUserProfileBO.Page_Action = "ADD" Then
                                strUSERID = Auto_UserIdGenerate_Flags(objSaveUserProfileBO.FirstName, objSaveUserProfileBO.LastName)
                            ElseIf objSaveUserProfileBO.Page_Action = "EDIT" Then
                                strUSERID = GetDisplayedUserID(objSaveUserProfileBO.Page_Action, objSaveUserProfileBO.UserType, objSaveUserProfileBO.ManuallyEntered_UserId, objSaveUserProfileBO.UserID)
                            End If
                        ElseIf Site_Flag = "V" Then
                            strUSERID = Trim(objSaveUserProfileBO.UserID.ToUpper)
                        Else
                            strUSERID = GetDisplayedUserID(objSaveUserProfileBO.Page_Action, objSaveUserProfileBO.UserType, objSaveUserProfileBO.ManuallyEntered_UserId, objSaveUserProfileBO.UserID)
                        End If
                    Else
                        If objSaveUserProfileBO.Flag_AddUser = "A" Then
                            If objSaveUserProfileBO.Page_Action = "ADD" Then
                                strUSERID = Auto_UserIdGenerate_Flags(objSaveUserProfileBO.FirstName, objSaveUserProfileBO.LastName)
                            ElseIf objSaveUserProfileBO.Page_Action = "EDIT" Then
                                strUSERID = GetDisplayedUserID(objSaveUserProfileBO.Page_Action, objSaveUserProfileBO.UserType, objSaveUserProfileBO.ManuallyEntered_UserId, objSaveUserProfileBO.UserID)
                            End If
                        ElseIf objSaveUserProfileBO.Flag_AddUser = "V" Then
                            strUSERID = Trim(objSaveUserProfileBO.UserID.ToUpper)
                        Else
                            strUSERID = GetDisplayedUserID(objSaveUserProfileBO.Page_Action, objSaveUserProfileBO.UserType, objSaveUserProfileBO.ManuallyEntered_UserId, objSaveUserProfileBO.UserID)
                        End If
                    End If
                Else
                    If objSaveUserProfileBO.Page_Action = "ADD" Then
                        strUSERID = Auto_UserIdGenerate_Flags(objSaveUserProfileBO.FirstName, objSaveUserProfileBO.LastName)
                    ElseIf objSaveUserProfileBO.Page_Action = "EDIT" Then
                        strUSERID = GetDisplayedUserID(objSaveUserProfileBO.Page_Action, objSaveUserProfileBO.UserType, objSaveUserProfileBO.ManuallyEntered_UserId, objSaveUserProfileBO.UserID)
                    End If
                End If


                Dim strBU As String
                GetSelectedBUandGroup(strBU, strUserGroup, objSaveUserProfileBO.Session_Role, objSaveUserProfileBO.Session_UserType, objSaveUserProfileBO.Page_Action, objSaveUserProfileBO.UserType, objSaveUserProfileBO.SiteValue, objSaveUserProfileBO.Query_IsMexico, objSaveUserProfileBO.Query_IsVendor)

                If objSaveUserProfileBO.MultiSiteChecked = True Then
                    If objSaveUserProfileBO.MultiSite.Count = 0 Then
                        ''Error_LabelSite.Text = "Please select a business unit to access the multi site."
                        'ltlAlert.Text = strMessage.Say("Please select a business unit to access the multi site.")
                        ''Exit Sub
                    Else
                        If objSaveUserProfileBO.Session_Role = "SUPER" Then
                            If objSaveUserProfileBO.UserType = "S" Or objSaveUserProfileBO.UserType = "C" Then
                                If objSaveUserProfileBO.UserRole = "CORPADMIN" Then
                                    MultiSiteAccess = " "
                                Else
                                    MultiSiteAccess = "Y"
                                End If
                            Else
                                MultiSiteAccess = " "
                            End If
                        ElseIf objSaveUserProfileBO.Session_Role = "ADMIN" Then
                            If objSaveUserProfileBO.UserType = "S" Or objSaveUserProfileBO.UserType = "C" Then
                                If objSaveUserProfileBO.UserRole = "CORPADMIN" Then
                                    MultiSiteAccess = " "
                                Else
                                    MultiSiteAccess = "Y"
                                End If
                            Else
                                MultiSiteAccess = " "
                            End If
                        ElseIf objSaveUserProfileBO.Session_Role = "CORPADMIN" Then

                        Else
                            MultiSiteAccess = " "
                        End If
                    End If

                End If

                Dim strDepartmentValue As String = ""
                If objSaveUserProfileBO.Session_UserType = "S" Then
                    If objSaveUserProfileBO.UserType = "S" Then
                        strDepartmentValue = objSaveUserProfileBO.UserDepart
                    ElseIf objSaveUserProfileBO.Session_Role = "ADMIN" Or objSaveUserProfileBO.Session_Role = "USER" Then
                        strDepartmentValue = objSaveUserProfileBO.UserDepart
                    Else
                        strDepartmentValue = ""
                    End If
                Else
                    strDepartmentValue = ""
                End If

                strRndCplusPassw = GenerateRndPassword(strUSERID, False, "")

                GetDisplayedUserType(strSDICust, strUserType, objSaveUserProfileBO.Page_Action, objSaveUserProfileBO.Query_IsVendor, objSaveUserProfileBO.Query_IsMexicoVendor, objSaveUserProfileBO.UserType, objSaveUserProfileBO.UserRole)
                Dim sSDIEmp As String = ConvertSDICustToSDIEmp(strSDICust)

                If Not objSaveUserProfileBO.PassWord = "" Then
                    strPasswEncrp = GenerateHash(objSaveUserProfileBO.PassWord)
                    strPasswUpdate = " ISA_PASSWORD_ENCR = '" & strPasswEncrp & "'," & vbCrLf
                End If
                Dim MultiSiteAvailible As String = ""
                Dim IsBaseBU As Boolean = False
                Dim multiBU As String = String.Empty
                'If lblAction.Text = "ADD" Then
                If objSaveUserProfileBO.Page_Action = "ADD" Then
                    Dim strFromWhichScreen As String = "Profile.aspx/ProfileUpdate"

                    Dim lngIsaUserId As Long = GetNextUserId(strUSERID, strBU, strFromWhichScreen)
                    If objSaveUserProfileBO.UserType = "V" Then
                        strSQLString = "INSERT INTO SDIX_USERS_TBL" & vbCrLf &
                        " (ISA_USER_ID, ISA_USER_NAME," & vbCrLf &
                        " ISA_PASSWORD_ENCR, FIRST_NAME_SRCH," & vbCrLf &
                        " LAST_NAME_SRCH, BUSINESS_UNIT," & vbCrLf &
                        " ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME, PHONE_NUM," & vbCrLf &
                        " ISA_DAILY_ALLOW, ISA_EMPLOYEE_PASSW," & vbCrLf &
                        " ISA_EMPLOYEE_EMAIL, ISA_EMPLOYEE_ACTYP," & vbCrLf &
                        " CUST_ID, ISA_SESSION," & vbCrLf &
                        " ISA_LAST_SYNC_DATE, ISA_SDI_EMPLOYEE, ISA_CUST_SERV_FLG," & vbCrLf &
                        " LASTUPDOPRID, LASTUPDDTTM, ACTIVE_STATUS, LAST_ACTIVITY, ISA_VENDOR_ID,THIRDPARTY_COMP_ID)" & vbCrLf &
                        " VALUES(" & lngIsaUserId & "," & vbCrLf &
                        " '" & strFullName.ToUpper & "'," & vbCrLf &
                        " '" & strPasswEncrp & "'," & vbCrLf &
                        " '" & strFirst.ToUpper & "'," & vbCrLf &
                        " '" & strLast.ToUpper & "'," & vbCrLf &
                        " '" & strBU & "'," & vbCrLf &
                        " '" & strUSERID & "'," & vbCrLf &
                        " '" & strFullName40.ToUpper & "'," & vbCrLf &
                        " '" & strPhone & "'," & vbCrLf &
                        " 0, ' ', '" & Trim(objSaveUserProfileBO.UserEmailID) & "'," & vbCrLf &
                        " '" & strUserType & "'," & vbCrLf &
                        " '0', 0, '', '" & strSDICust & "', ' '," & vbCrLf &
                        " '" & Trim(objSaveUserProfileBO.Session_UserID) & "'," & vbCrLf &
                        " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf &
                        " 'A', TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'), '" & strVendorIdvalue & "','" & ThirdParty_CompanyID & "')"
                    Else
                        strSQLString = "INSERT INTO SDIX_USERS_TBL" & vbCrLf &
                        " (ISA_USER_ID, ISA_USER_NAME," & vbCrLf &
                        " ISA_PASSWORD_ENCR, FIRST_NAME_SRCH," & vbCrLf &
                        " LAST_NAME_SRCH, BUSINESS_UNIT," & vbCrLf &
                        " ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME, PHONE_NUM," & vbCrLf &
                        " ISA_DAILY_ALLOW, ISA_EMPLOYEE_PASSW," & vbCrLf &
                        " ISA_EMPLOYEE_EMAIL, ISA_EMPLOYEE_ACTYP," & vbCrLf &
                        " CUST_ID, ISA_SESSION," & vbCrLf &
                        " ISA_LAST_SYNC_DATE, ISA_SDI_EMPLOYEE, ISA_CUST_SERV_FLG," & vbCrLf &
                        " LASTUPDOPRID, LASTUPDDTTM, ACTIVE_STATUS, LAST_ACTIVITY, MULTI_SITE, TCKTDEPT,THIRDPARTY_COMP_ID)" & vbCrLf &
                        " VALUES(" & lngIsaUserId & "," & vbCrLf &
                        " '" & strFullName.ToUpper & "'," & vbCrLf &
                        " '" & strPasswEncrp & "'," & vbCrLf &
                        " '" & strFirst.ToUpper & "'," & vbCrLf &
                        " '" & strLast.ToUpper & "'," & vbCrLf &
                        " '" & strBU & "'," & vbCrLf &
                        " '" & strUSERID & "'," & vbCrLf &
                        " '" & strFullName40.ToUpper & "'," & vbCrLf &
                        " '" & strPhone & "'," & vbCrLf &
                        " 0, ' ', '" & Trim(objSaveUserProfileBO.UserEmailID) & "'," & vbCrLf &
                        " '" & strUserType & "'," & vbCrLf &
                        " '0', 0, '', '" & strSDICust & "', ' '," & vbCrLf &
                        " '" & Trim(objSaveUserProfileBO.Session_UserID) & "'," & vbCrLf &
                        " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf &
                        " 'A', TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'), '" & MultiSiteAccess & "', '" & Trim(strDepartmentValue) & "','" & ThirdParty_CompanyID & "')"
                    End If

                    strSQLPW = "INSERT INTO SDIX_ISOL_PW" & vbCrLf &
                        " (ISA_USER_ID, ISA_EMPLOYEE_ID," & vbCrLf &
                        " ISA_ISOL_PW1, ISA_ISOL_PW_DATE1," & vbCrLf &
                        " ISA_ISOL_PW2, ISA_ISOL_PW_DATE2," & vbCrLf &
                        " ISA_ISOL_PW3, ISA_ISOL_PW_DATE3)" & vbCrLf &
                        " VALUES (" & lngIsaUserId & "," & vbCrLf &
                        " '" & strUSERID & "'," & vbCrLf &
                        " '" & strPasswEncrp & "'," & vbCrLf &
                        " TO_DATE('" & dteNow & "', 'MM/DD/YYYY')," & vbCrLf &
                        " ' ', '', ' ','')"

                    'ElseIf lblAction.Text = "EDIT" Or lblAction.Text = "USER" Then
                ElseIf objSaveUserProfileBO.Page_Action = "EDIT" Or objSaveUserProfileBO.Page_Action = "USER" Then
                    strSQLUPD1 = "UPDATE SDIX_USERS_TBL" & vbCrLf &
                        " SET ISA_USER_NAME = '" & strFullName.ToUpper & "'," & vbCrLf

                    strSQLUPD2 = " FIRST_NAME_SRCH = '" & strFirst.ToUpper & "'," & vbCrLf &
                        " LAST_NAME_SRCH = '" & strLast.ToUpper & "'," & vbCrLf &
                        " BUSINESS_UNIT = '" & strBU & "'," & vbCrLf &
                        " ISA_EMPLOYEE_NAME = '" & strFullName40.ToUpper & "'," & vbCrLf &
                        " PHONE_NUM = '" & strPhone & "'," & vbCrLf &
                        " ISA_EMPLOYEE_EMAIL = '" & Trim(objSaveUserProfileBO.UserEmailID) & "'," & vbCrLf &
                        " ISA_VENDOR_ID = '" & strVendorIdvalue & "', " & vbCrLf &
                    " THIRDPARTY_COMP_ID = '" & ThirdParty_CompanyID & "', " & vbCrLf &
                    " SHIPTO_DEFAULT = '" & shipto_default & "', " & vbCrLf

                    'If roleDropdownList.Visible Then 19/07/2020
                    If Not Trim(strUserType) = "" Then
                        strSQLUPD2 = strSQLUPD2 & " ISA_EMPLOYEE_ACTYP = '" & strUserType & "'," & vbCrLf
                    End If
                    If Not Trim(strSDICust) = "" Then
                        strSQLUPD2 = strSQLUPD2 & " ISA_SDI_EMPLOYEE = '" & strSDICust & "'," & vbCrLf
                    End If
                    'End If

                    If objSaveUserProfileBO.MultiSiteChecked And objSaveUserProfileBO.MultiSite.Count > 1 Then
                        ''strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf

                        If objSaveUserProfileBO.Session_UserType = "S" Then
                            If objSaveUserProfileBO.Session_Role = "SUPER" Then
                                If objSaveUserProfileBO.UserType = "S" Or objSaveUserProfileBO.UserType = "C" Then
                                    If objSaveUserProfileBO.UserRole <> "CORPADMIN" Then
                                        MultiSiteAvailible = "Y"
                                        strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf
                                    Else
                                        strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                                    End If
                                Else
                                    strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                                End If
                            ElseIf objSaveUserProfileBO.Session_Role = "ADMIN" Then
                                If objSaveUserProfileBO.UserType = "S" Or objSaveUserProfileBO.UserType = "C" Then
                                    If objSaveUserProfileBO.UserRole <> "CORPADMIN" Then
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
                        ElseIf objSaveUserProfileBO.Session_UserType = "C" Then
                            If objSaveUserProfileBO.Session_Role = "CORPADMIN" Then
                                If objSaveUserProfileBO.UserType = "C" Then
                                    If objSaveUserProfileBO.UserRole <> "CORPADMIN" Then
                                        MultiSiteAvailible = "Y"
                                        strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  ='Y'," & vbCrLf
                                    Else
                                        strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                                    End If
                                Else
                                    strSQLUPD2 = strSQLUPD2 & " MULTI_SITE  =' '," & vbCrLf
                                End If
                            ElseIf objSaveUserProfileBO.Session_Role = "ADMIN" Then
                                If objSaveUserProfileBO.UserType = "C" Then
                                    If objSaveUserProfileBO.UserRole <> "CORPADMIN" Then
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
                        'rcbMultiSelect.ClearCheckedItems() 19/07/2020 Handle in the front end
                        'MultiSiteChk.Checked = False
                        'rcbMultiSelect.Visible = False
                    End If

                    If objSaveUserProfileBO.Session_UserType = "S" Then
                        If objSaveUserProfileBO.UserType = "S" Then
                            strSQLUPD2 = strSQLUPD2 & " TCKTDEPT = '" & Trim(strDepartmentValue) & "'," & vbCrLf
                        ElseIf objSaveUserProfileBO.Session_Role = "ADMIN" Or objSaveUserProfileBO.Session_Role = "USER" Then
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
                    " LASTUPDOPRID = '" & objSaveUserProfileBO.Session_UserID & "'," & vbCrLf &
                    " LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                    " WHERE ISA_EMPLOYEE_ID = '" & Trim(strUSERID).ToUpper & "'"

                    strSQLString = strSQLUPD1 & strPasswUpdate & strSQLUPD2
                    If Not objSaveUserProfileBO.PassWord = "" Then
                        strSQLPW = getPWsql(strPasswEncrp, objSaveUserProfileBO.lblUserIDHide, objSaveUserProfileBO.UserID)
                        If strSQLPW.Substring(0, 5) = "Error" Then
                            'lblPassword.Visible = True
                            'lblConfirm.Visible = True
                            'txtPassword.Text = ""19/07/2020
                            'txtConfirm.Text = ""19/07/2020
                            Error_TableRow("PasswordExist") = "Error - password has already been used"
                            Error_Table.Rows.Add(Error_TableRow)
                            _result.Tables.Add(Error_Table)
                            _result.Tables.Add(Success_Message)
                            data = JsonConvert.SerializeObject(_result)
                            Return data
                            'setuppasswordfields("PWCHANGE")19/07/2020 handle in the front end
                            Exit Function
                        End If
                    End If
                End If

                Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSQLString)
                If rowsaffected = 0 Then
                    'lblDBError.Text = "Error Updating ISA_USERS_TBL Table" 19/07/2020
                    'lblDBError.Visible = True
                    Error_TableRow("Error_Upd_ISA_USERS_TBL") = "Error Updating ISA_USERS_TBL Table"
                    Error_Table.Rows.Add(Error_TableRow)
                    _result.Tables.Add(Error_Table)
                    _result.Tables.Add(Success_Message)
                    data = JsonConvert.SerializeObject(_result)
                    Exit Function
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

                    If objSaveUserProfileBO.MultiSite.Count > 1 Then
                        If objSaveUserProfileBO.Page_Action = "EDIT" Then
                            If MultiSiteAvailible = "Y" Then
                                For Each item In objSaveUserProfileBO.MultiSite
                                    multiBU = item
                                    query = "INSERT INTO SDIX_MULTI_SITE VALUES('" & strUSERID & "','" & multiBU & "','" & objSaveUserProfileBO.Session_UserID & "',TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
                                    ORDBData.ExecNonQuery(query)
                                Next
                            End If
                        Else
                            If Trim(MultiSiteAccess) = "Y" Then
                                For Each item In objSaveUserProfileBO.MultiSite
                                    multiBU = item
                                    query = "INSERT INTO SDIX_MULTI_SITE VALUES('" & strUSERID & "','" & multiBU & "','" & objSaveUserProfileBO.Session_UserID & "',TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
                                    ORDBData.ExecNonQuery(query)
                                Next
                            End If
                        End If
                    End If

                End If
                If Not objSaveUserProfileBO.PassWord = "" Then
                    rowsaffected = ORDBData.ExecNonQuery(strSQLPW)
                    If rowsaffected = 0 Then
                        'lblDBError.Text = "Error Updating ISA_ISOL_PW Table" 19/07/2020
                        'lblDBError.Visible = True
                        Error_TableRow("Error_Upd_ISA_ISOL_PW") = "Error Updating ISA_ISOL_PW Table"
                        Error_Table.Rows.Add(Error_TableRow)
                        _result.Tables.Add(Error_Table)
                        _result.Tables.Add(Success_Message)
                        data = JsonConvert.SerializeObject(_result)
                        Exit Function
                    End If
                End If
                If strSDICust = "C" Then
                    If checkCustEmpTbl(strBU, objSaveUserProfileBO.UserID) = False Then

                        strSQLString = "Insert Into PS_ISA_EMPL_TBL" & vbCrLf &
                                " ( BUSINESS_UNIT, ISA_EMPLOYEE_ID," & vbCrLf &
                                " ISA_EMPLOYEE_NAME, ISA_DAILY_ALLOW," & vbCrLf &
                                " ISA_EMPLOYEE_PASSW, ISA_EMPLOYEE_EMAIL," & vbCrLf &
                                " ISA_EMPLOYEE_ACTYP, CUST_ID, EFF_STATUS) " & vbCrLf &
                               " Values('" & strBU & "'," & vbCrLf &
                               "'" & strUSERID.ToUpper & "'," & vbCrLf &
                               "'" & strFullName40.ToUpper & "'" & vbCrLf &
                               ",0,' '," & vbCrLf &
                               "' '," & vbCrLf &
                               "' ', ' ', 'A')" & vbCrLf

                        rowsaffected = ORDBData.ExecNonQuery(strSQLString)
                        If rowsaffected = 0 Then
                            'lblDBError.Text = "Error Updating PS_ISA_EMPL_TBL Table" 19/07/2020
                            'lblDBError.Visible = True
                            Error_TableRow("Error_Upd_PS_ISA_EMPL_TBL") = "Error Updating PS_ISA_EMPL_TBL Table"
                            Error_Table.Rows.Add(Error_TableRow)
                            _result.Tables.Add(Error_Table)
                            _result.Tables.Add(Success_Message)
                            data = JsonConvert.SerializeObject(_result)
                            Return data
                            Exit Function
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
                        strSQLString = "INSERT INTO SDIX_USERS_PRIVS" & vbCrLf &
                    " (ISA_EMPLOYEE_ID," & vbCrLf &
                    " BUSINESS_UNIT," & vbCrLf &
                    " ISA_IOL_OP_NAME," & vbCrLf &
                    " ISA_IOL_OP_VALUE," & vbCrLf &
                    " ISA_IOL_OP_TYPE," & vbCrLf &
                    " LASTUPDOPRID," & vbCrLf &
                    " LASTUPDDTTM)" & vbCrLf &
                    " VALUES('" & strUSERID.ToUpper & "'," & vbCrLf &
                    " '" & strbunit & "'," & vbCrLf &
                    " 'ASN','Y'," & vbCrLf &
                    " 'SUP'," & vbCrLf &
                    " '" & objSaveUserProfileBO.Session_UserID & "'," & vbCrLf &
                    " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"


                        rowsaffected = ORDBData.ExecNonQuery(strSQLString)
                        If rowsaffected = 0 Then
                            'lblDBError.Text = "Error Updating ISA_USERS_PRIVS Table" 19/07/2020
                            'lblDBError.Visible = True
                            Error_TableRow("Error_Upd_ISA_USERS_PRIVS") = "Error Updating ISA_USERS_PRIVS Table"
                            Error_Table.Rows.Add(Error_TableRow)

                            _result.Tables.Add(Error_Table)
                            _result.Tables.Add(Success_Message)
                            data = JsonConvert.SerializeObject(_result)
                            Return data
                            Exit Function
                        End If
                    End If
                End If

                '  End If
                'If lblAction.Text = "ADD" Then
                If objSaveUserProfileBO.Page_Action = "ADD" Then
                    'lblMessage.Text = "New user-<b>" + strUSERID + "</b> created successfully." 19/07/2020
                    Success_MessageRow("AddUserMessage") = "New user " + strUSERID + " created successfully."
                    Success_Message.Rows.Add(Success_MessageRow)
                    'UserCreated = strUSERID 19/07/2020
                    'resetallfields()
                Else

                    'lblMessage.Text = "User information has been modified and saved successfully." 19/07/2020
                    Success_MessageRow("EditUserMessage") = "User information has been modified and saved successfully."
                    Success_Message.Rows.Add(Success_MessageRow)
                    'setuppasswordfields("REMOVE") 19/07/2020
                    'tr_CpwdFields.Style.Add("display", "none")
                    'tr_PwdFields.Style.Add("display", "none")
                    If objSaveUserProfileBO.Session_UserType = "C" Then
                        If objSaveUserProfileBO.Session_Role = "ADMIN" Or objSaveUserProfileBO.Session_Role = "CORPADMIN" Then
                            If objSaveUserProfileBO.Page_Action = "EDIT" Then
                                'Me.dropSelectUser.SelectedIndex = Me.dropSelectUser.Items.IndexOf(Me.dropSelectUser.Items.FindByValue(value:=Trim(txtUserid.Text)))
                                '19/07/2020
                                'Me.rcbdropSelectUser.SelectedIndex = Me.rcbdropSelectUser.Items.IndexOf(Me.rcbdropSelectUser.Items.FindItemByValue(value:=Trim(txtUserid.Text)))
                            End If
                        End If
                    End If
                End If
                _result.Tables.Add(Error_Table)
                _result.Tables.Add(Success_Message)
                data = JsonConvert.SerializeObject(_result)
            End If

        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    Private Function Verify_UserID(ByVal str_BU As String, ByVal UserID As String) As Boolean

        Dim Regex_Value As String
        Dim PatternGen As New List(Of String)
        Dim Mask_Value As String
        Dim GetPattenRegex As String
        Try
            Mask_Value = GetMaskValues_BU(str_BU)
            If Mask_Value <> "" Then
                GetPattenRegex = GetRegexPattern_MaskValues(Mask_Value)

                Dim regex As Regex = New Regex(GetPattenRegex)
                Dim match As Match = regex.Match(UserID.Trim)
                If match.Success Then
                    If match.Value = UserID.Trim Then
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
                If UserID.Contains("'") Then
                    Return False
                Else
                    Return True
                End If
            End If

        Catch ex As Exception
        End Try
    End Function
    Protected Function valUserid2_ServerValidate(ByVal StrBU As String, ByVal UserId As String, ByRef Error_Table As DataTable) As Boolean


        Dim Validate_bool As Boolean = Verify_UserID(StrBU, UserId)
        Dim Error_TableRow As DataRow = Error_Table.NewRow()
        Dim Validation As Boolean = False
        If Validate_bool = True Then
            Dim bolUseridexist As Boolean = ExistsUserid(UserId)  '  checkUserid()
            If bolUseridexist = True Then
                'valUserid2.ErrorMessage = "User ID already exists"
                'valUserid2.IsValid = False
                'args.IsValid = False
                Validation = True
                Error_TableRow("UserIdExist") = "User ID already exists"
                Error_Table.Rows.Add(Error_TableRow)
            End If
        Else
            Dim Mask_Value As String = GetMaskValues_BU(StrBU)
            If Mask_Value <> "" Then
                'valUserid2.IsValid = False
                'args.IsValid = False
                Validation = True
                Error_TableRow("UserIdFormatError") = "Please enter UserID in this <b>'" & Mask_Value & "'</b> format"
                Error_Table.Rows.Add(Error_TableRow)
            Else
                'valUserid2.IsValid = False
                'args.IsValid = False
                Validation = True
                Error_TableRow("InvalidUserId") = "Invalid User ID"
                Error_Table.Rows.Add(Error_TableRow)
            End If
        End If
        Return Validation
    End Function
    Private Function ExistsUserid(ByVal UserId As String) As Boolean
        Dim bExistsUserID = False
        Try
            Dim strSQLstring As String = "Select ISA_USER_ID FROM SDIX_USERS_TBL WHERE isa_employee_id = '" & Trim(UserId) & "'"

            Dim dsUserid As DataSet = ORDBData.GetAdapter(strSQLstring)


            If dsUserid.Tables(0).Rows.Count = 0 Then
                bExistsUserID = False
            ElseIf dsUserid.Tables(0).Rows.Count > 1 Then
                'Dim strMessage As New Alert
                bExistsUserID = True
                'ltlAlert.Text = strMessage.Say("Error - User exists more than once in user table!")
            Else
                bExistsUserID = True
            End If


        Catch ex As Exception

        End Try
        Return bExistsUserID
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
    Private Sub GetSelectedBUandGroup(ByRef strBU As String, ByRef strUserGroup As String, ByVal Session_UserRole As String, ByVal Session_UserType As String, ByVal Page_Action As String, ByVal UserType As String,
                                      ByVal Selected_Site As String, ByVal Query_Mexico As String, ByVal Query_Vendor As String)
        If Session_UserRole = "SUPER" Then
            If Query_Vendor = "YES" Or UserType = "V" Then
                strBU = Selected_Site.ToString.Trim
                strUserGroup = m_cUserGroup_Vendor
            ElseIf Query_Mexico = "YES" Then
                strBU = "SDM00"
                strUserGroup = m_cUserGroup_Mexico
            Else
                strBU = GetBUbyGroup(Selected_Site)
                strUserGroup = Selected_Site
            End If
        ElseIf Session_UserRole = "ADMIN" Or Session_UserRole = "CORPADMIN" Then
            If Query_Vendor = "YES" Or UserType = "V" Then
                If Page_Action = "ADD" Then
                    strBU = Selected_Site.ToString.Trim
                    strUserGroup = m_cUserGroup_Vendor
                Else
                    strBU = GetBUbyGroup(Selected_Site)
                    strUserGroup = Selected_Site
                End If
            ElseIf UserType = "C" Then
                If Session_UserRole = "CORPADMIN" Then
                    If Page_Action = "EDIT" Or Page_Action = "ADD" Then
                        strBU = GetBUbyGroup(Selected_Site)
                        strUserGroup = Selected_Site
                    End If
                Else
                    If Page_Action = "EDIT" Or Page_Action = "ADD" Then
                        strBU = GetBUbyGroup(Selected_Site)
                        strUserGroup = Selected_Site
                    ElseIf Page_Action = "ADD" Then
                        strBU = GetBUbyGroup(Selected_Site)
                        strUserGroup = Selected_Site
                    End If
                End If
            ElseIf UserType = "S" Then
                strBU = GetBUbyGroup(Selected_Site)
                strUserGroup = Selected_Site
            End If
        ElseIf Session_UserRole = "USER" Then
            If Session_UserType = "S" Then
                If Page_Action = "ADD" Then
                    If UserType = "V" Then
                        strBU = Selected_Site.ToString.Trim
                        strUserGroup = m_cUserGroup_Vendor
                    Else
                        If Selected_Site <> Nothing Then
                            'rcbGroup.SelectedValue = Session("StoreRCBSessionValue") 20/07/2020
                            strBU = GetBUbyGroup(Selected_Site)
                            strUserGroup = Selected_Site
                            'Session.Remove("StoreRCBSessionValue") 20/07/2020
                        End If
                    End If
                ElseIf Page_Action = "EDIT" Then
                    If UserType = "S" Or UserType = "C" Then
                        strBU = GetBUbyGroup(Selected_Site)
                        strUserGroup = Selected_Site
                    End If
                End If
            Else
                If Page_Action = "EDIT" Or Page_Action = "ADD" Then
                    strBU = GetBUbyGroup(Selected_Site)
                    strUserGroup = Selected_Site
                End If
            End If

        End If
    End Sub
    Private Function GetDisplayedUserID(ByVal Page_Action As String, ByVal UserType As String, ByVal UserIdSessionvalue As String, ByVal txtUserid As String) As String
        Dim strUSERID As String
        If Page_Action = "ADD" Then
            If UserType = "C" Or UserType = "S" Then
                If UserIdSessionvalue <> "" Then
                    strUSERID = UserIdSessionvalue.Trim.ToUpper
                    strUSERID = Replace(strUSERID, "'", "")
                End If
            Else
                strUSERID = txtUserid.Trim.ToUpper
                strUSERID = Replace(strUSERID, "'", "")
            End If
        Else
            strUSERID = txtUserid.Trim.ToUpper
            strUSERID = Replace(strUSERID, "'", "")
        End If

        'Dim strUSERID As String = txtUserid.Text.Trim.ToUpper
        'strUSERID = Replace(strUSERID, "'", "")
        Return strUSERID
    End Function
    Private Function GetFlagValue(ByVal SiteValue As String) As String
        Try
            Dim SQLAutoFlagQuery As String
            Dim FlagValues As String
            SQLAutoFlagQuery = "SELECT AUTO_ASSIGN_FLG FROM PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT = '" & Trim(SiteValue) & "'"
            Dim dsFlagBU As DataSet = ORDBData.GetAdapter(SQLAutoFlagQuery)
            FlagValues = dsFlagBU.Tables(0).Rows(0).Item("AUTO_ASSIGN_FLG")
            Return FlagValues
        Catch ex As Exception

        End Try

    End Function
    Private Function Auto_UserIdGenerate_Flags(ByVal FirstName As String, ByVal LastName As String) As String

        Try
            Dim AutoGenerateUserID As String
            Dim strFirst As String = Trim(FirstName)
            strFirst = Replace(strFirst, "'", "")
            Dim strLast As String = Trim(LastName)
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
    Private Function DeleteUserPriv(ByVal struserid As String, ByVal strBU As String) As String
        Try
            Dim get_Usertype As String = "Delete from SDIX_USERS_PRIVS where ISA_EMPLOYEE_ID = '" + struserid.ToUpper() + "' AND BUSINESS_UNIT = '" + strBU + "' AND ISA_IOL_OP_NAME != 'EMLRECVD'"
            Dim retVal As Integer = 0
            retVal = ORDBData.ExecNonQueryWithTransaction(get_Usertype)
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
    Private Function getPWsql(ByVal strPWencr As String, ByVal lblUserIDHide As String, ByVal txtUserid As String) As String

        Dim s As String = ""

        Dim strAlert As String = "Error - password has already been used"
        Dim dteNow As Date
        dteNow = Now().ToString("d")
        Dim strSQLstring As String
        strSQLstring = "SELECT A.ISA_USER_ID, A.ISA_EMPLOYEE_ID," & vbCrLf &
                        " A.ISA_ISOL_PW1, A.ISA_ISOL_PW_DATE1," & vbCrLf &
                        " A.ISA_ISOL_PW2, A.ISA_ISOL_PW_DATE2," & vbCrLf &
                        " A.ISA_ISOL_PW3, A.ISA_ISOL_PW_DATE3" & vbCrLf &
                        " FROM SDIX_ISOL_PW A" & vbCrLf &
                        " WHERE A.ISA_USER_ID = '" & lblUserIDHide & "'" & vbCrLf &
                        " AND A.ISA_EMPLOYEE_ID = '" & txtUserid & "'"

        Try
            Dim dtrPWReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
            ' Log whenever connection is established with the DB: Vijay - 2/15/2013
            Dim m_weblogstring As String = CStr(ConfigurationSettings.AppSettings("Weblogstring")).Trim
            If m_weblogstring = "true" Then
                'WebLogOpenConn()
            End If
            If dtrPWReader.Read() Then
                If dtrPWReader.Item("ISA_ISOL_PW1") = strPWencr Or
                    dtrPWReader.Item("ISA_ISOL_PW2") = strPWencr Or
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
                s &= "" &
                     "UPDATE SDIX_ISOL_PW " & vbCrLf &
                     "SET " & vbCrLf &
                     " ISA_ISOL_PW1 = '" & strPWencr & "' " & vbCrLf &
                     ",ISA_ISOL_PW_DATE1 = TO_DATE('" & dteNow.ToString("MM/dd/yyyy") & "', 'MM/DD/YYYY') " & vbCrLf &
                     ""
                If pw2.Trim.Length > 0 And
                   dt2.Trim.Length > 0 Then
                    s &= "" &
                         ",ISA_ISOL_PW2 = '" & pw2 & "' " & vbCrLf &
                         ",ISA_ISOL_PW_DATE2 = TO_DATE('" & dt2 & "', 'MM/DD/YYYY') " & vbCrLf &
                         ""
                Else
                    s &= "" &
                         ",ISA_ISOL_PW2 = ' ' " & vbCrLf &
                         ",ISA_ISOL_PW_DATE2 = NULL " & vbCrLf &
                         ""
                End If
                If pw3.Trim.Length > 0 And
                   dt3.Trim.Length > 0 Then
                    s &= "" &
                         ",ISA_ISOL_PW3 = '" & pw3 & "' " & vbCrLf &
                         ",ISA_ISOL_PW_DATE3 = TO_DATE('" & dt3 & "', 'MM/DD/YYYY') " & vbCrLf &
                         ""
                Else
                    s &= "" &
                         ",ISA_ISOL_PW3 = ' ' " & vbCrLf &
                         ",ISA_ISOL_PW_DATE3 = NULL " & vbCrLf &
                         ""
                End If
                s &= "" &
                     "WHERE ISA_USER_ID = '" & lblUserIDHide & "' " & vbCrLf &
                     "  AND ISA_EMPLOYEE_ID = '" & txtUserid & "' " & vbCrLf &
                     ""
            Else
                s = ""
                s &= "" &
                     "INSERT INTO SDIX_ISOL_PW " & vbCrLf &
                     "(" & vbCrLf &
                     " ISA_USER_ID " & vbCrLf &
                     ",ISA_EMPLOYEE_ID " & vbCrLf &
                     ",ISA_ISOL_PW1 " & vbCrLf &
                     ",ISA_ISOL_PW_DATE1 " & vbCrLf &
                     ",ISA_ISOL_PW2 " & vbCrLf &
                     ",ISA_ISOL_PW_DATE2 " & vbCrLf &
                     ",ISA_ISOL_PW3 " & vbCrLf &
                     ",ISA_ISOL_PW_DATE3 " & vbCrLf &
                     ") " & vbCrLf &
                     "VALUES " & vbCrLf &
                     "(" & vbCrLf &
                     " " & lblUserIDHide & vbCrLf &
                     ",'" & txtUserid & "' " & vbCrLf &
                     ",'" & strPWencr & "' " & vbCrLf &
                     ",TO_DATE('" & dteNow.ToString("MM/dd/yyyy") & "', 'MM/DD/YYYY') " & vbCrLf &
                     ",' ' " & vbCrLf &
                     ",NULL " & vbCrLf &
                     ",' ' " & vbCrLf &
                     ",NULL " & vbCrLf &
                     ") " & vbCrLf &
                     ""
            End If

            dtrPWReader.Close()
            'WebLogCloseConn()

        Catch objException As Exception
            Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
            sendErrorEmail(objException.ToString, "NO", currentApp.Request.ServerVariables("URL"), strSQLstring)
            'Response.Redirect("DBErrorPage.aspx?HOME=N") 19/07/2020
        End Try

        Return (s)
    End Function
    Private Function checkCustEmpTbl(ByVal strBU As String, ByVal txtUserid As String) As Boolean

        Dim strSQLstring As String = "Select ISA_EMPLOYEE_ID" & vbCrLf &
                    " FROM PS_ISA_EMPL_TBL" & vbCrLf &
                    " WHERE UPPER(isa_employee_id) = '" & txtUserid.ToUpper & "'" & vbCrLf &
                    " AND BUSINESS_UNIT = '" & strBU & "'"

        Dim dsCustUserid As DataSet = ORDBData.GetAdapter(strSQLstring)

        If dsCustUserid.Tables(0).Rows.Count = 0 Then
            checkCustEmpTbl = False
        Else
            checkCustEmpTbl = True
        End If
    End Function
    Private Function checkUserPrivs(ByVal strUserid) As Boolean
        Dim strbunit As String = " "
        'If Me.txtUserid.Text.Substring(0, 2) = "M0" Or Me.txtUserid.Text.Substring(0, 2) = "MU" Then
        If strUserid.Substring(0, 2) = "M0" Or strUserid.Substring(0, 2) = "MU" Then
            strbunit = "SDM00"
        Else
            strbunit = "ISA00"
        End If
        Dim strSQLString As String = "SELECT A.ISA_EMPLOYEE_ID" & vbCrLf &
                        " FROM SDIX_USERS_PRIVS A" & vbCrLf &
                        " WHERE A.BUSINESS_UNIT = '" & strbunit & "'" & vbCrLf &
                        " AND A.ISA_EMPLOYEE_ID = '" & strUserid & "'" & vbCrLf &
                        " AND A.ISA_IOL_OP_NAME = 'ASN'" & vbCrLf &
                        " AND A.ISA_IOL_OP_VALUE = 'Y'" & vbCrLf &
                        " AND A.ISA_IOL_OP_TYPE = 'SUP'"

        Dim strUserResults As String = ORDBData.GetScalar(strSQLString)
        If strUserResults Is Nothing Then
            checkUserPrivs = False
        Else
            checkUserPrivs = True
        End If
    End Function
    Private Sub GetDisplayedUserType(ByRef strSDICust As String, ByRef strUserType As String, ByVal Page_Action As String, ByVal Query_IsVendor As Boolean, ByVal Query_IsMexicoVendor As Boolean, ByVal Usertype As String, ByVal UserRole As String)
        Try
            If Page_Action = "ADD" Then
                If Query_IsVendor Or Query_IsMexicoVendor Then
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
            strSDICust = Usertype.ToUpper()
            strUserType = UserRole.ToUpper()
            'End If
        Catch ex As Exception

        End Try
    End Sub
    Private Function ConvertSDICustToSDIEmp(ByVal sSDICust As String)
        Dim sSDIEmp As String
        If sSDICust = "S" Then
            sSDIEmp = "SDI"
        Else
            sSDIEmp = "CUST"
        End If
        Return sSDIEmp
    End Function
    '* Ref: UP_PC_184
    '* 
    Public Function tbStripUserDetails_TabClick(ByVal data As String) As String
        '19/07/2020 Not necessary because we are not going to use Telerik
        'RadMultiPage1.RenderSelectedPageOnly = True
        'RadMultiPage1.SelectedIndex = tbStripUserDetails.SelectedIndex
        ''System.Threading.Thread.Sleep(1000)
        'Dim position As New AjaxLoadingPanelBackgroundPosition()
        'position = AjaxLoadingPanelBackgroundPosition.Center
        'RadAjaxLoadingPanel1.BackgroundTransparency = 25
        'lblMessage.Text = ""
        Try

            Dim _result As New DataSet
            If Not String.IsNullOrEmpty(data) Then
                Dim objTabChangeBo As TabChangeBo = JsonConvert.DeserializeObject(Of TabChangeBo)(data)
                Dim DsUPVL As DataSet
                Dim Is_Vendor As String = ""
                Dim BUvalue As String = ""
                Dim SQLSTRINGQuery As String
                Dim Error_Table As New DataTable
                Error_Table.Columns.Add("SdiTrack_Issue")
                Error_Table.Columns.Add("BU_Invalid")

                If Trim(objTabChangeBo.UserId) <> "" Then
                    SQLSTRINGQuery = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & Trim(objTabChangeBo.UserId) & "'"
                Else
                    SQLSTRINGQuery = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & Trim(objTabChangeBo.Session_UserId) & "'"
                End If
                DsUPVL = ORDBData.GetAdapter(SQLSTRINGQuery)
                If DsUPVL.Tables(0).Rows.Count = 1 Then
                    Is_Vendor = DsUPVL.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
                    BUvalue = DsUPVL.Tables(0).Rows(0).Item("BUSINESS_UNIT")
                End If

                Select Case objTabChangeBo.TabName
                    Case "UDTL"
                        Dim SearchUserTable As New DataTable
                        SearchUserTable = buildSelectDropDown(objTabChangeBo.UserId, objTabChangeBo.Session_BUSUNIT, objTabChangeBo.UserRole, objTabChangeBo.Session_ThirdParty_CompanyID, objTabChangeBo.Session_SDIEMP, objTabChangeBo.UserType, objTabChangeBo.Query_IsVendor)
                        _result.Tables.Add(SearchUserTable)
                        '19/07/2020 All Entitlement Can be handled in the front end.
                        'If Session("USERTYPEVALUE") = "C" Then
                        '    If Session("ROLE") = "ADMIN" Then
                        '        ''roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        '        If MultiSiteChk.Checked Then
                        '            rcbMultiSelect.Visible = True
                        '        Else
                        '            rcbMultiSelect.Visible = False
                        '        End If
                        '    End If
                        'End If
                        'If Session("USERTYPE") = "SUPER" Then
                        '    If Page_Action = "EDIT" Then
                        '        If radioUserType.SelectedValue = "S" Then
                        '            lblSelectUser.Visible = True
                        '            roleDropdownList.Items.FindByValue("SUPER").Enabled = True
                        '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        '            lblDept.Visible = True

                        '        ElseIf radioUserType.SelectedValue = "V" Then
                        '            lblSelectUser.Visible = True
                        '            Label_VendorID.Visible = True
                        '            tr_BU_unit_field.Style.Remove("display")
                        '            drpBUnit.Visible = True
                        '            lblBusUnit.Visible = True
                        '            Radcombobox_vendorID()
                        '            'RadcomboforVendorID.Visible = True
                        '            MainvndrID.Visible = True
                        '            lblGroup.Visible = False
                        '            rcbGroup.Visible = False
                        '            MultiSiteChk.Visible = False
                        '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        '            rcbMultiSelect.Visible = False
                        '        Else
                        '            lblSelectUser.Visible = True
                        '            roleDropdownList.Items.FindByValue("SUPER").Enabled = False
                        '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        '            lblDept.Visible = False
                        '        End If
                        '    Else

                        '    End If
                        'ElseIf Session("USERTYPE") = "ADMIN" Then
                        '    If Session("USERTYPEVALUE") = "S" Then
                        '        drpBUnit.Visible = False
                        '        lblBusUnit.Visible = False
                        '        lblSelectUser.Visible = True
                        '        'dropSelectUser.Visible = True
                        '        rcbdropSelectUser.Visible = True
                        '        lblGroup.Visible = True
                        '        btnAdd.Visible = True
                        '        lblPassword.Visible = False
                        '        lblConfirm.Visible = False
                        '        radioUserType.Visible = False
                        '        Label_usrtype.Visible = False
                        '        tr_PwdFields.Style.Add("display", "none")
                        '        tr_CpwdFields.Style.Add("display", "none")
                        '        tr_BU_unit_field.Style.Add("display", "none")
                        '        MultiSiteChk.Visible = True
                        '        rcbGroup.Visible = False
                        '        If Page_Action = "ADD" Then
                        '            lblGroup.Visible = True
                        '            rcbGroup.Visible = False
                        '            MultiSiteChk.Visible = True
                        '        End If
                        '        If roleDropdownList.SelectedValue = "CORPADMIN" Then
                        '            MultiSiteChk.Visible = False
                        '        End If
                        '        If Session("USERID") = rcbdropSelectUser.SelectedValue Then
                        '            lblDept.Visible = True
                        '        End If
                        '    ElseIf Session("USERTYPEVALUE") = "C" Then
                        '        lblGroup.Visible = True
                        '    End If
                        'ElseIf Session("USERTYPE") = "USER" Then
                        '    If Session("USERTYPEVALUE") = "S" Then
                        '        lblSelectUser.Visible = False
                        '        'TextBox_VendorId.Visible = False
                        '        lblBusUnit.Visible = False
                        '        drpBUnit.Visible = False
                        '        lblSelectUser.Visible = False
                        '        'dropSelectUser.Visible = False
                        '        rcbdropSelectUser.Visible = False
                        '        Label_usrtype.Visible = False
                        '        radioUserType.Visible = False
                        '        lblGroup.Visible = True
                        '        btnAdd.Visible = False
                        '        lblPassword.Visible = False
                        '        lblConfirm.Visible = False
                        '        rcbGroup.Visible = False
                        '        tr_PwdFields.Style.Add("display", "none")
                        '        tr_CpwdFields.Style.Add("display", "none")
                        '        tr_BU_unit_field.Style.Add("display", "none")
                        '        tr_selectuser_fields.Style.Add("display", "none")
                        '        ''tr_Multiselect_fields.Style.Add("display", "none")
                        '        lblDept.Visible = True
                        '        rcbDept.Visible = True
                        '        btnAdd.Visible = False
                        '        btnEdit.Visible = False
                        '        SDIUsers()
                        '    ElseIf Session("USERTYPEVALUE") = "C" Then
                        '        CustomersUsers()
                        '    End If
                        'ElseIf Session("USERTYPE") = "CORPADMIN" Then
                        '    If Page_Action = "EDIT" Then
                        '        CustomersUsers()
                        '        rcbGroup.Visible = True
                        '        MultiSiteChk.Visible = False
                        '        rcbMultiSelect.Visible = False
                        '        If roleDropdownList.SelectedValue <> "CORPADMIN" Then
                        '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = False
                        '        Else
                        '            roleDropdownList.Items.FindByValue("CORPADMIN").Enabled = True
                        '        End If
                        '    Else

                        '    End If
                        'End If
                        'If btnChangePassw.Visible = True Then
                        '    lblPassword.Visible = False
                        '    lblConfirm.Visible = False
                        'Else
                        '    lblPassword.Visible = True
                        '    lblConfirm.Visible = True
                        'End If
                        Exit Select
                    Case "UPVL"
                        Dim Redirect_Page As New DataTable
                        Redirect_Page.Columns.Add("DBErrorPage.aspx")
                        'If Session("BUSUNIT") = "" Then  19/07/2020
                        '    Session.RemoveAll()
                        '    Response.Redirect("default.aspx")
                        'End If
                        Dim ddlUserRole_Enabled As Boolean = False
                        Dim Error_TablrRows As DataRow = Error_Table.NewRow()
                        Dim strBU As String
                        'Dim strMessage As New Alert

                        If objTabChangeBo.Session_USERTYPE = "SUPER" Then
                            If Is_Vendor = "V" Then
                                strBU = m_cUserGroup_Vendor
                            Else
                                strBU = GetBUbyGroup(BUvalue)
                            End If


                            If strBU = "" Then
                                Error_TablrRows("BU_Invalid") = "Error - No Business Unit Selected!"
                                Error_Table.Rows.Add(Error_TablrRows)
                                _result.Tables.Add(Error_Table)
                                data = JsonConvert.SerializeObject(_result)
                                Return data
                                '21/07/2020
                                'RadMultiPage1.RenderSelectedPageOnly = True
                                'RadMultiPage1.SelectedIndex = 0
                                'tbStripUserDetails.Tabs(0).Selected = True
                                'ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - No Business Unit Selected!"), True)
                                Exit Function
                            End If
                        Else
                            strBU = GetBUbyGroup(objTabChangeBo.SiteValue)
                        End If

                        If strBU = "0" Then
                            Error_TablrRows("BU_Invalid") = "Error - Invalid BU - check productview id's!"
                            Error_Table.Rows.Add(Error_TablrRows)
                            _result.Tables.Add(Error_Table)
                            data = JsonConvert.SerializeObject(_result)
                            Return data
                            '21/07/2020
                            'RadMultiPage1.RenderSelectedPageOnly = True
                            'RadMultiPage1.SelectedIndex = 0
                            'tbStripUserDetails.Tabs(0).Selected = True
                            'ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - Invalid BU - check productview id's!"), True)
                            Exit Function
                        End If

                        Dim sDisplayedSDICust As String = ""
                        Dim sDisplayedUserType As String = ""
                        'GetDisplayedUserType(sDisplayedSDICust, sDisplayedUserType)
                        sDisplayedSDICust = DsUPVL.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
                        sDisplayedUserType = DsUPVL.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")

                        If checkUserid(objTabChangeBo.UserId) Then
                            LoadUserprivilegesData(Redirect_Page, _result, strBU, sDisplayedSDICust, sDisplayedUserType, objTabChangeBo.Page_Action, objTabChangeBo.UserType, objTabChangeBo.Session_USERTYPE,
                                                   objTabChangeBo.UserId, objTabChangeBo.UserRole, objTabChangeBo.Session_BUSUNIT, objTabChangeBo.Query_IsVendor, objTabChangeBo.Query_IsMexicoVendor, ddlUserRole_Enabled)

                            If Redirect_Page.Rows.Count > 0 Then
                                _result.Tables.Add(Redirect_Page)
                                data = JsonConvert.SerializeObject(Redirect_Page)
                                Return data
                                Exit Function
                            End If
                        Else


                            If objTabChangeBo.Page_Action = "EDIT" Then
                                Error_TablrRows("BU_Invalid") = "Please select a user in User Detail before editing privileges!"
                                Error_Table.Rows.Add(Error_TablrRows)
                            Else
                                Error_TablrRows("BU_Invalid") = "Please save User Detail data before editing privileges!"
                                Error_Table.Rows.Add(Error_TablrRows)
                            End If
                            _result.Tables.Add(Error_Table)
                            data = JsonConvert.SerializeObject(_result)
                            Return data
                            'RadMultiPage1.RenderSelectedPageOnly = True
                            'RadMultiPage1.SelectedIndex = 0
                            'tbStripUserDetails.Tabs(0).Selected = True
                            'If Page_Action = "EDIT" Then
                            '    ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Please select a user in User Detail before editing privileges!"), True)
                            'Else
                            '    ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Please save User Detail data before editing privileges!"), True)
                            'End If
                            Exit Function
                        End If
                        'Added for hiding privilege type, program tree, expand&collapse button for corp admin
                        If objTabChangeBo.Session_USERTYPE = "CORPADMIN" Then
                            '21/07/2020 Handle in the front end
                            'lblPrivilegeType.Visible = False
                            'rblType.Visible = False
                            'lblProgram.Visible = False
                            'rtvPrograms.Visible = False
                            'btnExpandAll.Visible = False
                            'btnCollapseAll.Visible = False
                            'btnSelectAll.Visible = False
                            'btnDeselectAll.Visible = False
                            GetUserAccessType(Redirect_Page, _result, objTabChangeBo.UserId, objTabChangeBo.SiteValue, objTabChangeBo.Page_Action, objTabChangeBo.UserType, objTabChangeBo.Query_IsVendor, objTabChangeBo.Query_IsMexicoVendor, objTabChangeBo.UserRole, ddlUserRole_Enabled)

                            If Redirect_Page.Rows.Count > 0 Then
                                _result.Tables.Add(Redirect_Page)
                                data = JsonConvert.SerializeObject(Redirect_Page)
                                Return data
                                Exit Function
                            End If
                        End If
                        '21/07/2020
                        'lblMessage1N.Text = ""
                        'Label1.Text = ""
                        Exit Select
                    Case "APP"
                        'If Session("BUSUNIT") = "" Then  20/07/2020
                        '    Session.RemoveAll()
                        '    Response.Redirect("default.aspx")
                        'End If
                        'txtAppTotal.Text = ""
                        'lblMsg.Text = ""
                        Dim strBU As String
                        'Dim strMessage As New Alert
                        If objTabChangeBo.Session_USERTYPE = "SUPER" Then
                            strBU = GetBUbyGroup(objTabChangeBo.SiteValue)
                        Else
                            strBU = GetBUbyGroup(objTabChangeBo.SiteValue)
                        End If
                        If strBU = "0" Then
                            'RadMultiPage1.RenderSelectedPageOnly = True  20/07/2020
                            'RadMultiPage1.SelectedIndex = 0
                            'tbStripUserDetails.Tabs(0).Selected = True
                            Dim Error_TablrRows As DataRow = Error_Table.NewRow()
                            Error_TablrRows("BU_Invalid") = "Error - Invalid Business Unit - check productview id!"
                            Error_Table.Rows.Add(Error_TablrRows)
                            _result.Tables.Add(Error_Table)
                            data = JsonConvert.SerializeObject(_result)
                            Return data
                            'ltlAlert.Text = strMessage.Say("Error - Invalid Business Unit - check productview id's!"
                            'ltlAlert.Text = "Error - Invalid Business Unit - check productview id''s!"
                            'ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - Invalid Business Unit - check productview id!"), True)
                            Exit Function
                        End If

                        Dim sOrdApprType As String = "O"
                        'Dim objEnterprise As New clsEnterprise(strBU)
                        Select Case sOrdApprType  '  objEnterprise.OrdApprType
                            Case "O", "D", "M"
                                'OK 
                            Case Else
                                'RadMultiPage1.RenderSelectedPageOnly = True   20/07/2020
                                'RadMultiPage1.SelectedIndex = 0
                                'tbStripUserDetails.Tabs(0).Selected = True
                                Dim Error_TablrRows As DataRow = Error_Table.NewRow()
                                Error_TablrRows("BU_Invalid") = "Business unit is not set up as an approver site."
                                Error_Table.Rows.Add(Error_TablrRows)
                                _result.Tables.Add(Error_Table)
                                data = JsonConvert.SerializeObject(_result)
                                Return data
                                'ltlAlert.Text = strMessage.Say("Business unit is not set up as an approver site.")
                                'ltlAlert.Text = "Business unit is not set up as an approver site."
                                'ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Business unit is not set up as an approver site."), True)
                                Exit Function
                        End Select
                        'LoadApprovals(txtUserid.Text, strBU, Session("USERTYPE"))

                        If objTabChangeBo.Session_USERTYPE = "SUPER" Then
                            'Showing the Budgetory Approval fields
                            '20/07/2020 Handle in the Front end
                            'lblAppEmpID.Visible = True
                            'DropAppEmpID.Visible = True
                            'chbDelete.Visible = True
                            'lblAppTotal.Visible = True
                            'txtAppTotal.Visible = True
                            'btnSubmit.Visible = True
                            'lblMsg.Visible = True
                            'btnSetReqAppr.Visible = True
                            'lblSiteBaseCurrencyCode.Text = ""
                            'tr_OrderLimit_fields.Style.Add("display", "contents")

                            ''Hiding the Requestor Approval fields
                            'lblAltReqAppr.Visible = False
                            'ddReqAppr.Visible = False
                            'chkDeleteReqAppr.Visible = False
                            'btnSubmitReqAppr.Visible = False
                            'lblMsgReqAppr.Visible = False
                            'btnSetBudAppr.Visible = False

                            _result = LoadApprovals(objTabChangeBo.UserId, strBU, objTabChangeBo.Session_ThirdParty_CompanyID)
                        Else
                            'Hiding the Budgetory Approval fields
                            '20/07/2020 Handle in the Front end
                            'lblAppEmpID.Visible = False
                            'DropAppEmpID.Visible = False
                            'chbDelete.Visible = False
                            'lblAppTotal.Visible = False
                            'txtAppTotal.Visible = False
                            'btnSubmit.Visible = False
                            'lblMsg.Visible = False
                            'btnSetReqAppr.Visible = False
                            'lblSiteBaseCurrencyCode.Text = ""
                            'tr_OrderLimit_fields.Style.Add("display", "none")

                            ''Showing the Requestor Approval fields
                            'lblAltReqAppr.Visible = True
                            'ddReqAppr.Visible = True
                            'chkDeleteReqAppr.Visible = True
                            'btnSubmitReqAppr.Visible = True
                            'lblMsgReqAppr.Visible = True
                            'btnSetBudAppr.Visible = False

                            _result = LoadReqApprovals(objTabChangeBo.UserId, strBU, objTabChangeBo.Session_ThirdParty_CompanyID)
                        End If

                        Exit Select
                    Case "OSE"
                        'If Session("BUSUNIT") = "" Then  20/07/2020
                        '    Session.RemoveAll()
                        '    Response.Redirect("default.aspx")
                        'End If
                        'resetIOLCheckboxes() In this method all the check box is cleared.
                        Dim strBU As String = ""
                        'Dim strMessage As New Alert  20/07/2020
                        If objTabChangeBo.Session_USERTYPE = "SUPER" Then
                            strBU = GetBUbyGroup(objTabChangeBo.SiteValue)
                        Else
                            strBU = GetBUbyGroup(objTabChangeBo.SiteValue)
                        End If
                        Dim Redirect_Page As New DataTable
                        Redirect_Page.Columns.Add("DBErrorPage.aspx")
                        Dim ROleExist As New DataTable
                        ROleExist.Columns.Add("ROleExist")
                        Dim RoleExist_row As DataRow = ROleExist.NewRow()
                        RoleExist_row("ROleExist") = "FALSE"
                        ROleExist.Rows.Add(RoleExist_row)
                        Dim userRoleID As Integer = clsAccessPrivileges.GetUserAccessRole(objTabChangeBo.UserId, strBU, Redirect_Page)
                        If Redirect_Page.Rows.Count > 0 Then
                            Dim Hashtable As New DataTable
                            Hashtable.TableName = "Hashtable"
                            ROleExist.TableName = "ROleExist"
                            _result.Tables.Add(ROleExist)
                            _result.Tables.Add(Hashtable)
                        End If
                        If strBU = "0" Then
                            Dim Error_TablrRows As DataRow = Error_Table.NewRow()
                            Error_TablrRows("BU_Invalid") = "Error - Invalid Business Unit - check productview ids!"
                            Error_Table.Rows.Add(Error_TablrRows)
                            _result.Tables.Add(Error_Table)
                            Dim Hashtable As New DataTable
                            Hashtable.TableName = "Hashtable"
                            _result.Tables.Add(Hashtable)
                            ROleExist.TableName = "ROleExist"
                            _result.Tables.Add(ROleExist)
                            data = JsonConvert.SerializeObject(_result)
                            Return data
                            '20/07/2020
                            'RadMultiPage1.RenderSelectedPageOnly = True
                            'RadMultiPage1.SelectedIndex = 0
                            'tbStripUserDetails.Tabs(0).Selected = True
                            ''ltlAlert.Text = strMessage.Say("Error - Invalid Business Unit - check productview id's!")
                            'ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - Invalid Business Unit - check productview ids!"), True)

                            Exit Function
                        End If
                        ''*******NEW(1/5/2021)**
                        _result = LoadOrderStatusEmail(strBU, objTabChangeBo.UserId, objTabChangeBo.Session_EMAIL, objTabChangeBo.Session_BUSUNIT, objTabChangeBo.Session_USERTYPE)
                        '********************END*************************
                        If userRoleID = 0 Then
                            ''*******NEW(1/5/2021)**
                            '' _result = LoadOrderStatusEmail(strBU, objTabChangeBo.UserId, objTabChangeBo.Session_EMAIL, objTabChangeBo.Session_BUSUNIT, objTabChangeBo.Session_USERTYPE)
                            '********************END*************************
                        Else
                            ROleExist.Rows(0)("ROleExist") = objTabChangeBo.UserId + " is assigned a role."
                            ROleExist.AcceptChanges()
                        End If
                        ROleExist.TableName = "ROleExist"
                        _result.Tables.Add(ROleExist)
                        Exit Select
                    Case "PREF"
                        'If Session("BUSUNIT") = "" Then  19/07/2020
                        '    Session.RemoveAll()
                        '    Response.Redirect("default.aspx")
                        'End If
                        _result = LoadPreferences(objTabChangeBo.UserId, objTabChangeBo.SiteValue, BUvalue)
                        Exit Select
                    Case "TST"
                        _result = SetUpTrackTab(objTabChangeBo.Session_UserId, objTabChangeBo.Session_ThirdParty_CompanyID, objTabChangeBo.Session_BUSUNIT, objTabChangeBo.Page_Action, objTabChangeBo.UserRole, objTabChangeBo.UserType, objTabChangeBo.Session_SDIEMP, Error_Table)
                        If Error_Table.Rows.Count > 0 Then
                            _result.Tables.Add(Error_Table)
                        End If
                        Exit Select
                    Case "MOB"
                        _result = LoadGRIBIDENTITY(objTabChangeBo.UserId)
                        Exit Select
                    Case "ZEUS"
                        _result.Tables.Add(LoadZuesValues(objTabChangeBo.UserId))
                        Exit Select
                End Select
                data = JsonConvert.SerializeObject(_result)
            End If
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function

    Private Sub LoadUserprivilegesData(ByRef Redirect_Page As DataTable, ByRef _result As DataSet, ByVal displayedBU As String, ByVal sDisplayedSDICust As String, ByVal sDisplayedUserType As String,
                                       ByVal Page_Action As String, ByVal UserType As String, ByVal Session_UserRole As String, ByVal DisplayedUserId As String, ByVal LoginUserRole As String, ByVal Session_UserBu As String,
                                       ByVal Query_IsVendor As String, ByVal Query_IsMexicoVendor As String, ByRef ddlUserRole_Enabled As Boolean)
        Try
            Dim strBU As String
            Dim SisterSiteList As New DataTable
            SisterSiteList.TableName = "SisterSiteList"
            Dim ProgramsList As New DataTable
            ProgramsList.TableName = "ProgramsList"
            Dim RoleDDList As New DataTable
            RoleDDList.TableName = "RoleDDList"
            'ViewState("BU") = "" 21/07/2020
            If Query_IsVendor = "True" Or Query_IsMexicoVendor = "True" Or sDisplayedSDICust = "V" Then
                '21/07/2020
                'rcbGroupTab2.Visible = False
                'txtVendorUserGroup.Visible = True
                'txtVendorUserGroup.Text = displayedBU
                'ViewState("BU") = drpBUnit.SelectedValue
            Else
                'rcbGroupTab2.Visible = True 21/07/2020
                'txtVendorUserGroup.Visible = False
                'ViewState("BU") = displayedBU
                SisterSiteList = buildGroupList(LoginUserRole, Session_UserBu)
                SisterSiteList.TableName = "SisterSiteList"
                _result.Tables.Add(SisterSiteList)
                '21/07/2020
                'Try
                '    rcbGroupTab2.Items.FindItemByValue(displayedBU).Selected = True
                'Catch ex As Exception

                'End Try

            End If
            'lblUserVal.Text = txtUserid.Text 21/07/2020
            ' If the user can change to the privileges tab, it's not a "USER"; it's a "SUPER" or "ADMIN"
            ' who is editing privileges for themselves or for a USER. In that case, load the tree
            ' for the user who is editing the privileges.

            Dim sDisplayedUserID As String = GetDisplayedUserID(Page_Action, UserType, "", DisplayedUserId)
            Dim sDisplayedSDIEmp As String = ConvertSDICustToSDIEmp(sDisplayedSDICust)
            Dim sPortal As String = GetPortal(sDisplayedSDICust, Query_IsVendor, Query_IsMexicoVendor)
            ProgramsList = LoadProgramData(sPortal, Session_UserRole.ToString, sDisplayedUserID, sDisplayedUserType, sDisplayedSDIEmp)
            ProgramsList.TableName = "ProgramsList"
            _result.Tables.Add(ProgramsList)
            If sPortal = "Vendor" Then
                RoleDDList = LoadRoleMaster(sPortal, displayedBU, "V", Session_UserRole)
                RoleDDList.TableName = "RoleDDList"

            Else
                RoleDDList = LoadRoleMaster(sPortal, displayedBU, sDisplayedSDICust, Session_UserRole)
                RoleDDList.TableName = "RoleDDList"
                '_result.Tables.Add(RoleDDList)
            End If
            If Session_UserRole = "CORPADMIN" Then
                RoleDDList = LoadRoleMaster("Customer", displayedBU, "C", Session_UserRole)
                RoleDDList.TableName = "RoleDDList"
                '_result.Tables.Add(RoleDDList)
            End If
            _result.Tables.Add(RoleDDList)
            If RoleDDList.Rows.Count > 0 Then
                ddlUserRole_Enabled = True
            End If
            GetUserAccessType(Redirect_Page, _result, DisplayedUserId, displayedBU, Page_Action, UserType, Query_IsVendor, Query_IsMexicoVendor, LoginUserRole, ddlUserRole_Enabled)

            If Redirect_Page.Rows.Count > 0 Then
                Exit Sub
            End If


        Catch ex As Exception

        End Try
    End Sub

    Private Function GetUserAccessType(ByRef Redirect_Page As DataTable, ByRef _result As DataSet, ByVal UserId As String, ByVal DiaplayedBU As String, ByVal Page_Action As String, ByVal UserType As String,
                                       ByVal Query_IsVendor As String, ByVal Query_IsMexicoVendor As String, ByVal UserRole As String, ByVal ddlUserRole_Enabled As Boolean)
        Try
            Dim AlacartProgramTable As New DataTable
            AlacartProgramTable.TableName = "AlacartProgramTable"
            Dim RoleProgramTable As New DataTable
            RoleProgramTable.TableName = "RoleProgramTable"
            Dim UserRoleExist As New DataTable
            UserRoleExist.TableName = "UserRoleExist"
            UserRoleExist.Columns.Add("RoleId")
            UserRoleExist.Columns.Add("RoleName")
            Dim UserRoleExist_rows As DataRow = UserRoleExist.NewRow()
            'rblType.Items.FindByValue("Alacarte").Selected = True
            '21/07/2020 Handle in the front end
            'rblType.SelectedIndex = 0
            Dim userRoleID As Integer = clsAccessPrivileges.GetUserAccessRole(UserId, DiaplayedBU, Redirect_Page)
            If Redirect_Page.Rows.Count > 0 Then
                _result.Tables.Add(AlacartProgramTable)
                _result.Tables.Add(RoleProgramTable)
                _result.Tables.Add(UserRoleExist)
                Exit Function
            End If
            If userRoleID <= 0 Then
                AlacartProgramTable = GetAlaCarteData(UserId, DiaplayedBU, Page_Action, UserType, Query_IsVendor, Query_IsMexicoVendor, UserRole)

                'ddlUserRole.Enabled = False
            ElseIf userRoleID <> 0 And ddlUserRole_Enabled = False Then
                RoleProgramTable = GetRoleData(userRoleID)
                'RoleProgramTable.TableName = "RoleProgramTable"
                '_result.Tables.Add(RoleProgramTable)
                'ddlUserRole.Enabled = True '21/07/2020 Handle in the front end
                'rblType.Items.FindByValue("Role").Selected = True
                'rblType.SelectedIndex = 1 '21/07/2020 Handle in the front end
                Try
                    UserRoleExist_rows("RoleId") = userRoleID
                    'ddlUserRole.Items.FindByValue(userRoleID).Selected = True '21/07/2020 Handle in the front end
                Catch ex As Exception
                    Dim strRoleNames As String = ""

                    strRoleNames = Get_RoleValues(userRoleID)
                    UserRoleExist_rows("RoleName") = strRoleNames
                    'ddlUserRole.Items.FindByText(strRoleNames.ToUpper()).Selected = True '21/07/2020 Handle in the front end
                End Try

            Else
                RoleProgramTable = GetRoleData(userRoleID)

                'rblType.SelectedIndex = 1 '21/07/2020 Handle in the front end
                ''ddlUserRole.Items.FindByValue(userRoleID).Selected = True
                Try
                    UserRoleExist_rows("RoleId") = userRoleID
                    'ddlUserRole.Items.FindByValue(userRoleID).Selected = True '21/07/2020 Handle in the front end
                Catch ex As Exception
                    Dim strRoleNames As String = ""
                    strRoleNames = Get_RoleValues(userRoleID)
                    UserRoleExist_rows("RoleName") = strRoleNames
                    'ddlUserRole.Items.FindByText(strRoleNames.ToUpper()).Selected = True '21/07/2020 Handle in the front end
                End Try

            End If
            AlacartProgramTable.TableName = "AlacartProgramTable"
            _result.Tables.Add(AlacartProgramTable)
            RoleProgramTable.TableName = "RoleProgramTable"
            _result.Tables.Add(RoleProgramTable)
            UserRoleExist.Rows.Add(UserRoleExist_rows)
            _result.Tables.Add(UserRoleExist)

        Catch ex As Exception

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
    Private Function GetRoleData(ByVal id As String)
        Dim dsRolePrograms As DataSet
        Try
            'rtvPrograms.ClearCheckedNodes()
            Dim strQuery As String = "select ALIAS_NAME from SDIX_ROLEDETAIL where ROLENUM = " + id
            dsRolePrograms = ORDBData.GetAdapter(strQuery)
            Return dsRolePrograms.Tables(0).Copy()
            '21/07/2020 Handle in the front end
            'Dim roleRow As DataRow
            'Dim navTreePrograms As IList(Of RadTreeNode) = rtvPrograms.GetAllNodes()
            '' loop through nav tree
            'For Each navTreeNode As RadTreeNode In navTreePrograms
            '    ' loop thru role programs to find a match for the current nav tree program
            '    For Each roleRow In dsRolePrograms.Tables(0).Rows
            '        Dim roleProgramAlias As String = roleRow("ALIAS_NAME")
            '        If navTreeNode.Value = roleProgramAlias Then
            '            navTreeNode.Checked = True
            '            Exit For
            '        End If
            '    Next
            'Next

        Catch ex As Exception

        End Try
    End Function
    Private Function GetAlaCarteData(ByVal UserId As String, ByVal DiaplayedBU As String, ByVal Page_Action As String, ByVal UserType As String, ByVal Query_IsVendor As String, ByVal Query_IsMexicoVendor As String, ByVal UserRole As String)
        Try
            Dim strSDICust As String = ""
            Dim strUserType As String = ""
            GetDisplayedUserType(strSDICust, strUserType, Page_Action, Query_IsVendor, Query_IsMexicoVendor, UserType, UserRole)

            'rtvPrograms.ClearCheckedNodes()   21/07/2020
            Dim dsUserPrivileges As DataSet = New DataSet
            'dsUserPrivileges = BuildingMenus.BuildMenu.GetDBDrivenMenu(ViewState("BU"), txtUserid.Text)  '  , strSDICust)
            dsUserPrivileges = BuildingMenus.BuildMenu.GetUserMenu(DiaplayedBU, UserId, strSDICust)
            Return dsUserPrivileges.Tables(0).Copy()
            '21/07/2020 Handle in the front end
            'If Not dsUserPrivileges Is Nothing Or dsUserPrivileges.Tables(0).Rows.Count > 0 Then
            '    Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.GetAllNodes()
            '    ' Loop through privilege tree
            '    For Each node As RadTreeNode In nodeCollection
            '        ' Find current privilege tree program in user list of privileges
            '        Dim bFound As Boolean = False
            '        Dim iRowIndex As Integer = 0
            '        While iRowIndex < dsUserPrivileges.Tables(0).Rows.Count And Not bFound
            '            Dim name As String = ""
            '            Dim dr As DataRow = dsUserPrivileges.Tables(0).Rows(iRowIndex)
            '            Try
            '                name = dr("securityalias")
            '            Catch ex As Exception
            '                Try
            '                    ' Use this in case the privilege was deleted at some point in the past.
            '                    name = dr("securityalias", DataRowVersion.Original)
            '                Catch ex1 As Exception
            '                End Try
            '            End Try
            '            If name.Length > 0 Then
            '                If node.Value = name Then
            '                    node.Checked = True
            '                    bFound = True
            '                End If
            '            End If

            '            iRowIndex = iRowIndex + 1
            '        End While
            '    Next
            'End If
        Catch ex As Exception

        End Try
    End Function
    Private Function GetPortal(ByVal sDisplayedSDICust As String, ByVal Query_IsVendor As String, ByVal Query_IsMexicoVendor As String) As String
        ' Default is Customer portal
        Dim sPortal As String = clsProgramMaster.PortalCustomer

        If Query_IsVendor = "True" Or sDisplayedSDICust = "V" Then
            sPortal = clsProgramMaster.PortalVendor
        ElseIf Query_IsMexicoVendor = "True" Then
            sPortal = clsProgramMaster.PortalVendor
        End If

        Return sPortal
    End Function

    Private Function LoadRoleMaster(ByVal roleType As String, ByVal str_BU As String, ByVal str_Usrrole As String, ByVal Session_UserRole As String)
        Dim dsRoleData As DataSet
        Try
            Dim strSQLstring As String
            Dim userType As String = Convert.ToString(Session_UserRole)

            If str_Usrrole = "S" Then
                If userType.ToLower.Equals("admin") Then
                    strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT From SDIX_USRROLE_TBL WHERE ROLETYPE = 'SDI'" & vbCrLf &
                        "AND ROLENUM NOT IN (SELECT ROLENUM FROM SDIX_ROLEDETAIL WHERE ALIAS_NAME IN (" & vbCrLf &
                        "SELECT SECURITYALIAS FROM SDIX_PRGRMMASTER WHERE ACCESS_GROUP = 'SUPER'))"
                Else
                    strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT From SDIX_USRROLE_TBL WHERE ROLETYPE = 'SDI'"
                End If
            Else
                If userType.ToLower.Equals("admin") Then
                    strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT From SDIX_USRROLE_TBL WHERE ROLETYPE = '" + roleType + "'" & vbCrLf &
                        "AND ROLENUM NOT IN (SELECT ROLENUM FROM SDIX_ROLEDETAIL WHERE ALIAS_NAME IN (" & vbCrLf &
                        "SELECT SECURITYALIAS FROM SDIX_PRGRMMASTER WHERE ACCESS_GROUP = 'SUPER')) AND BUSINESS_UNIT = '" & str_BU & "'"
                Else
                    strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT From SDIX_USRROLE_TBL WHERE ROLETYPE = '" + roleType + "' AND BUSINESS_UNIT = '" & str_BU & "'"
                End If
            End If

            dsRoleData = ORDBData.GetAdapter(strSQLstring)
            Return dsRoleData.Tables(0).Copy()
            '21/07/2020 Handle in the first end
            'ddlUserRole.DataSource = dsRoleData
            'ddlUserRole.DataTextField = "ROLENAME"
            'ddlUserRole.DataValueField = "ROLENUM"
            'ddlUserRole.DataBind()
            'Added for disabling rolename dropdown if no value
            'If ddlUserRole.Items.Count = 0 Then
            '    ddlUserRole.Enabled = False
            'End If
        Catch ex As Exception

        End Try

    End Function
    Private Function LoadProgramData(ByVal sPortal As String, ByVal sEditorsUserType As String, ByVal sEditeesUserID As String, ByVal sEditeesUserType As String, ByVal sEditeesSDIEmp As String)
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
                    '21/07/2020 Handle in the Fornt end
                    'rtvPrograms.DataTextField = "PROGRAMNAME"
                    'rtvPrograms.DataValueField = "SECURITYALIAS"
                    'rtvPrograms.DataFieldID = "ISA_IDENTIFIER"
                    'rtvPrograms.DataFieldParentID = "ISA_PARENT_IDENT"

                    For Each row As DataRow In dsProgramData.Tables(0).Rows
                        row.Item("programname") = row.Item("programname").ToString & " (" & row.Item("securityalias") & ")"
                        If row.Item("active").ToString() = clsProgramMaster.InactiveProgramCode.ToString Then
                            row.Item("programname") = row.Item("programname").ToString & " - inactive"
                        ElseIf row.Item("securityalias").ToString.Trim.Length = 0 Then
                            row.Item("programname") = row.Item("programname").ToString & " - program not available"
                        End If
                    Next
                    dsProgramData.AcceptChanges()
                    Return dsProgramData.Tables(0).Copy()
                    'rtvPrograms.DataSource = dsProgramData
                    'rtvPrograms.DataBind()
                    'HideExpandCollapseButtons()  This method is used to Show Expand,Collapse button. Handle in the front end.
                End If

            End If
        Catch ex As Exception

        End Try
    End Function

    Private Function LoadZuesValues(ByVal UserId As String)
        Dim Name As String = ""
        Dim ZeusTable As New DataTable
        ZeusTable.TableName = "ZeusTable"
        ZeusTable.Columns.Add("Zeus_Disabled")
        ZeusTable.Columns.Add("Zeus_Enabled")
        Dim ZeusTable_Rows As DataRow = ZeusTable.NewRow()
        'lblZuseError.Text = ""  19/07/2020
        'cbxZeus.Checked = False
        Try
            Dim StrQury As String = "SELECT * FROM SDIX_QLIK_USERS WHERE USERID= '" & Trim(UserId).ToUpper & "'"
            Name = ORDBData.GetScalar(StrQury)
            If Not Name = "" Then
                ZeusTable_Rows("Zeus_Disabled") = "Disable Zeus"
                'cbxZeus.ToolTip = "Disable Zeus" 19/07/2020
            Else
                ZeusTable_Rows("Zeus_Enabled") = "Enable Zeus"
                'cbxZeus.ToolTip = "Enable Zeus" 19/07/2020
            End If
            ZeusTable.Rows.Add(ZeusTable_Rows)

        Catch ex As Exception

        End Try
        Return ZeusTable
    End Function
    Private Function LoadGRIBIDENTITY(ByVal strUserId As String)
        Dim StrQuery As String = String.Empty
        Dim UserID As String = String.Empty
        Dim _result As New DataSet
        Dim GribTable As New DataTable
        GribTable.TableName = "GribTable"
        'lblGRIBErr.Text = "" 19/07/2020
        Dim GribAlreadyExist As New DataTable
        GribAlreadyExist.TableName = "GribAlreadyExist"
        GribAlreadyExist.Columns.Add("GribAlreadyExist")
        Dim GribAlreadyExist_Rows As DataRow = GribAlreadyExist.NewRow()
        Try
            UserID = Trim(strUserId).ToUpper
        Catch ex As Exception
            UserID = " "
        End Try

        Dim StrGRIB As String = GetUserIDENT(UserID)
        GribAlreadyExist_Rows("GribAlreadyExist") = StrGRIB
        GribAlreadyExist.Rows.Add(GribAlreadyExist_Rows)
        StrQuery = "SELECT DISTINCT(SUBSTR(BUSINESS_UNIT_IN, 2, 4)) As BUUNIT FROM SYSADM8.PS_DS_NETDET_TBL WHERE DS_NETWORK_CODE = (SELECT DS_NETWORK_CODE " & vbCrLf &
        "FROM SYSADM8.PS_BUS_UNIT_TBL_OM WHERE BUSINESS_UNIT = (SELECT BUSINESS_UNIT FROM SDIX_USERS_TBL WHERE UPPER(ISA_EMPLOYEE_ID) = '" & UserID & "'))"
        Dim dtrEMPReader As OleDbDataReader = ORDBData.GetReader(StrQuery)
        GribTable.Load(dtrEMPReader)
        'ddlGrib.DataSource = dtrEMPReader 19/07/2020
        'ddlGrib.DataTextField = "BUUNIT"
        'ddlGrib.DataValueField = "BUUNIT"
        'ddlGrib.DataBind()
        'ddlGrib.Items.Insert(0, New ListItem("-- SELECT --"))

        '19/07/2020
        'If Not ddlGrib.Items.FindByValue(StrGRIB) Is Nothing Then
        '    ddlGrib.Items.FindByValue(StrGRIB).Selected = True
        'End If
        dtrEMPReader.Close()
        _result.Tables.Add(GribTable)
        _result.Tables.Add(GribAlreadyExist)
        Return _result
    End Function
    Private Function GetUserIDENT(ByVal StrUser As String) As String
        Dim UserIdent As String = String.Empty
        Dim StrQuery As String = "SELECT ISA_CRIB_IDENT FROM SDIX_USERS_TBL WHERE UPPER(ISA_EMPLOYEE_ID)='" & StrUser & "'"
        UserIdent = ORDBData.GetScalar(StrQuery)
        Return UserIdent
    End Function
    Private Function SetUpTrackTab(ByVal Session_UserId As String, ByVal Session_ThirdParty_CompanyID As String, ByVal UserBu As String, ByVal Page_Action As String, ByVal UserROle As String, ByVal UserType As String, ByVal session_SDIEMP As String, ByRef Error_Table As DataTable)
        Dim oSDITrack As New clsSDITrack()
        Try
            Dim sTrackAddedUserName As String = ""
            Dim sTrackAddedUserDate As String = ""
            Dim sTrackAddedUserGUID As String = ""
            Dim SDITrackDetails As New DataTable
            SDITrackDetails.Columns.Add("Current_User")
            SDITrackDetails.Columns.Add("SDITrack_UserID")
            SDITrackDetails.Columns.Add("sTrackAddedUserName")
            SDITrackDetails.Columns.Add("AddedUserDate")
            SDITrackDetails.Columns.Add("AddedUserGUID")
            Dim SDITrackDetails_Rows As DataRow = SDITrackDetails.NewRow()


            SDITrackDetails_Rows("Current_User") = Session_UserId

            If oSDITrack.IsAccountUser(sTrackAddedUserName, sTrackAddedUserDate, sTrackAddedUserGUID, Session_UserId, UserBu) Then
                SDITrackDetails_Rows("SDITrack_UserID") = sTrackAddedUserName
                'txtTangoUserName.Visible = False 19/07/2020
                SDITrackDetails_Rows("sTrackAddedUserName") = sTrackAddedUserName
                'lblTangoUserNameStored.Visible = True  19/07/2020
                'txtTangoUserName.ReadOnly = True
                'txtTangoPassword.Visible = False
                'lblTangoPassword.Visible = False
                'txtTangoPassword.ReadOnly = True
                'lblSDiTrackDateTime.Visible = True
                'lblSDiTrackDateTimeVal.Visible = True
                'lblSDiTrackGuid.Visible = True
                'lblSDiTrackGuidVal.Visible = True
                SDITrackDetails_Rows("AddedUserDate") = sTrackAddedUserDate
                SDITrackDetails_Rows("AddedUserGUID") = sTrackAddedUserGUID
                'btnTangoAddUser.Visible = False 19/07/2020
                'Else 19/07/2020
                '    txtTangoUserName.ReadOnly = False
                '    txtTangoPassword.ReadOnly = False
                '    btnTangoAddUser.Visible = True
                '    lblSDiTrackDateTime.Visible = False
                '    lblSDiTrackDateTimeVal.Visible = False
                '    lblSDiTrackGuid.Visible = False
                '    lblSDiTrackGuidVal.Visible = False
            End If
            SDITrackDetails.Rows.Add(SDITrackDetails_Rows)
            SDITrackDetails.TableName = "SDITrackDetails"

            Dim _result As New DataSet
            If Page_Action = "EDIT" Then
                'ddlSDiUsers.Visible = True  19/07/2020
                Dim OtherUserDetails As New DataTable

                Dim dsORUsers As DataSet = GetSelectDropDownData(Session_UserId, UserBu, UserROle, Session_ThirdParty_CompanyID, UserType, session_SDIEMP)
                Dim SelectedUser As String = dsORUsers.Tables(0).Rows(0)("ISA_EMPLOYEE_ID") '20/07/2020 During the page load the first value Sdi track details will be shown
                dsORUsers.Tables(0).TableName = "EmployeeList"
                _result.Tables.Add(dsORUsers.Tables(0).Copy())
                '19/07/2020 Handle in the front end
                'ddlSDiUsers.DataSource = dsORUsers
                'ddlSDiUsers.DataValueField = "ISA_EMPLOYEE_ID"
                'ddlSDiUsers.DataTextField = "USERANDBU" ' "ISA_USER_NAME"
                'ddlSDiUsers.DataBind()

                OtherUserDetails = ShowTrackData(SelectedUser)
                _result.Tables.Add(OtherUserDetails)

            End If
            _result.Tables.Add(SDITrackDetails)
            Return _result
        Catch ex As Exception
            Dim Error_TablrRows As DataRow = Error_Table.NewRow()
            Error_TablrRows("SdiTrack_Issue") = "SDiTrack Issue"
            Error_Table.Rows.Add(Error_TablrRows)
            'lblValidation.Text = "SDiTrack Issue"  19/07/2020
        End Try
    End Function
    Private Function ShowTrackData(ByVal SelectedUser As String)
        Try
            Dim ADDUserFormTable As New DataTable
            ADDUserFormTable.Columns.Add("OtherUserID")
            ADDUserFormTable.Columns.Add("BusinessUnit")
            ADDUserFormTable.Columns.Add("OtherUserBUMessage")
            ADDUserFormTable.Columns.Add("OtherUserTangoUserName")
            ADDUserFormTable.Columns.Add("OtherUserTangoUserNameValStored")
            ADDUserFormTable.Columns.Add("OtherUserTangoUserNameValStored_Visible")
            ADDUserFormTable.Columns.Add("OtherUserAddedOn")
            ADDUserFormTable.Columns.Add("OtherUserAddedOnVal")
            ADDUserFormTable.Columns.Add("OtherUserAddedOnVal_Visisble")
            ADDUserFormTable.Columns.Add("OtherUserGUID")
            ADDUserFormTable.Columns.Add("OtherUserGUIDVal")
            ADDUserFormTable.Columns.Add("OtherUserGUIDVal_Visisble")
            ADDUserFormTable.Columns.Add("txtOtherUserTangoUserNameVal_Visible")
            ADDUserFormTable.Columns.Add("OtherUserTangoPassword_Visible")

            ADDUserFormTable.Columns.Add("txtOtherUserTangoPasswordVal_Visible")
            ADDUserFormTable.Columns.Add("OtherUserNoBU")
            Dim ADDUserFormTable_Rows As DataRow = ADDUserFormTable.NewRow()

            If SelectedUser.ToString.Trim.Length > 0 Then
                Dim oUserTbl As New clsUserTbl(SelectedUser, "")
                ADDUserFormTable_Rows("OtherUserID") = SelectedUser

                'add user = off 20/07/2020 Handle in the front end
                'btnTangoAddOtherUser.Visible = False
                ''message = off
                'lblOtherUserBUMessage.Visible = False
                ''Track user ID = off
                'txtOtherUserTangoUserNameVal.Visible = False
                'lblOtherUserTangoUserNameValStored.Visible = False
                'lblOtherUserTangoUserNameValStored.Text = ""
                'txtOtherUserTangoUserNameVal.Text = ""
                ''password = off
                'lblOtherUserTangoPassword.Visible = False
                'txtOtherUserTangoPasswordVal.Visible = False
                ''added date = off
                'lblOtherUserAddedOn.Visible = False
                'lblOtherUserAddedOnVal.Visible = False
                ''GUID = off
                'lblOtherUserGUID.Visible = False
                'lblOtherUserGUIDVal.Visible = False
                ''validation = off
                'lblValidationOtherUser.Text = ""


                Dim dsBUSisterSites As DataSet
                Dim SiteTable As New DataTable
                dsBUSisterSites = UnilogORDBData.SisterBusinessUnits(oUserTbl.BusinessUnit)
                SiteTable = dsBUSisterSites.Tables(0).Copy()
                Dim oGroup As String = ""

                For I = 0 To SiteTable.Rows.Count - 1

                    Dim row As DataRow
                    row = SiteTable.Rows(I)
                    If row("BUSINESS_UNIT") = oUserTbl.BusinessUnit Then
                        oGroup = oUserTbl.BusinessUnit
                    End If
                Next

                'Dim oGroup As Object = SiteTable.Columns(oUserTbl.BusinessUnit)
                If oGroup IsNot Nothing Then
                    ADDUserFormTable_Rows("BusinessUnit") = oGroup

                    ''no BU message = off
                    'lblOtherUserNoBU.Visible = False  20/07/2020
                    'lblOtherUserBUVal.Visible = True

                    Dim oEnterprise As New clsEnterprise(oUserTbl.BusinessUnit)
                    If oEnterprise.TrackDBType = Nothing Then

                        'message = on
                        ADDUserFormTable_Rows("OtherUserBUMessage") = "The BU does not have an SDiTrack account."
                        'Track user ID = off
                        'lblOtherUserTangoUserName.Visible = False 20/07/2020
                    Else
                        'Dim oSDITrack As New clsSDITrack Not needed 20/07/2020
                        If oUserTbl.TrackUserGUID.Trim.Length > 0 Then
                            'Track user ID = on, display
                            ADDUserFormTable_Rows("OtherUserTangoUserName") = "SDiTrack User ID"
                            ADDUserFormTable_Rows("OtherUserTangoUserNameValStored") = oUserTbl.TrackUserName
                            ADDUserFormTable_Rows("OtherUserTangoUserNameValStored_Visible") = True
                            'added date = on
                            'lblOtherUserAddedOn.Visible = True  20/07/2020
                            ADDUserFormTable_Rows("OtherUserAddedOnVal") = oUserTbl.TrackToDate
                            ADDUserFormTable_Rows("OtherUserAddedOnVal_Visible") = True
                            'GUID = on
                            'lblOtherUserGUID.Visible = True 20/07/2020
                            ADDUserFormTable_Rows("OtherUserGUIDVal") = oUserTbl.TrackUserGUID
                            ADDUserFormTable_Rows("OtherUserGUIDVal_Visible") = True
                        Else
                            '20/07/2020 Handle in the front end
                            ''add user = on
                            ''btnTangoAddOtherUser.Visible = True 20/09/2020
                            ''Track user ID = on, editable
                            'lblOtherUserTangoUserName.Visible = True
                            'txtOtherUserTangoUserNameVal.Visible = True
                            ''password = on
                            'lblOtherUserTangoPassword.Visible = True
                            'txtOtherUserTangoPasswordVal.Visible = True
                        End If
                    End If
                Else
                    ''Track user ID = off 20/07/2020
                    'lblOtherUserTangoUserName.Visible = False
                    ''no BU message = on
                    ADDUserFormTable_Rows("lblOtherUserNoBU") = "The user is not assigned to a valid BU."
                    'lblOtherUserBUVal.Visible = False 20/07/2020
                End If
                ADDUserFormTable.Rows.Add(ADDUserFormTable_Rows)
                ADDUserFormTable.TableName = "ADDUserFormTable"
            End If
            Return ADDUserFormTable
        Catch ex As Exception
            Dim esb As Integer = 1
        End Try
    End Function
    '*UP_PC_195
    '*
    Public Function ddlSDiUsers_SelectedIndexChanged(ByVal data As String) As String
        Dim _result As New DataSet
        Try
            Dim OtherUserDetails As New DataTable
            If Not String.IsNullOrEmpty(data) Then

                Dim objSDIUserDetailsBO As SDIUserDetailsBO = JsonConvert.DeserializeObject(Of SDIUserDetailsBO)(data)
                OtherUserDetails = ShowTrackData(objSDIUserDetailsBO.SelectedUser)
                _result.Tables.Add(OtherUserDetails)
            End If
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    Private Function LoadPreferences(ByVal UserId As String, ByVal SelectedSite As String, Optional ByVal strBU As String = "")
        Dim _result As New DataSet
        Try
            _result = GetProdDispType(UserId, SelectedSite, strBU)
            '20/07/2020 Handle in the front end
            'pf1.Attributes.Add("style", "display:block")
            'pf2.Attributes.Add("style", "display:block")
            'pf3.Attributes.Add("style", "display:block")
            'pf4.Attributes.Add("style", "display:block")
            'pf5.Attributes.Add("style", "display:block")
            'pf6.Attributes.Add("style", "display:block")

            '20/07/2020 Handle in the front end
            'Response.Cache.SetExpires(DateTime.Now.AddSeconds(3))
            'lblBU.Text = strBU

            ' this program should NOT return the "exempt flag" when
            '   called from labelsbyChgCd.aspx page.
            '   - erwin 2009.02.10
            '20/07/2020 Handle in the front end
            'Dim bIsIncludeExemptFlag As Boolean = True      ' since I don't know what program(s) needed this flag, let's default to "return flag"
            'Try
            '    If Not (Request.QueryString(PARAM_RETURN_EXEMPT_FLAG) Is Nothing) Then
            '        Dim valTrue As String = "Y~1~TRUE"
            '        Dim valFalse As String = "N~0~FALSE"
            '        If valTrue.IndexOf(Request.QueryString(PARAM_RETURN_EXEMPT_FLAG).Trim.ToUpper) > -1 Then
            '            bIsIncludeExemptFlag = True
            '        ElseIf valFalse.IndexOf(Request.QueryString(PARAM_RETURN_EXEMPT_FLAG).Trim.ToUpper) > -1 Then
            '            bIsIncludeExemptFlag = False
            '        End If
            '    End If
            'Catch ex As Exception
            '    'ignore
            '    '   this will return the default value
            'End Try

            'Dim strScript As String = ""
            'If Not Page.IsPostBack Then 20/07/2020 This will not Occur

            '    WebLog()
            'End If
            'getccdata(strBU)  20/07/2020 Charge Code is not include in this module
            '20/07/2020 Handle in the front end
            'If Session("USERTYPE") = "SUPER" Then
            '    'lblBlockPrice.Visible = True
            '    chkbxDisplayPrice.Enabled = True
            'Else
            '    'lblBlockPrice.Visible = False 
            '    chkbxDisplayPrice.Enabled = False
            'End If
            Return _result
        Catch ex As Exception

        End Try
    End Function
    Private Function GetProdDispType(ByVal UserId As String, ByVal SelectedSite As String, Optional ByVal strBU As String = "")

        Dim defaultShipTo As String = " "
        Dim strSQLstring As String
        Dim _result As New DataSet
        Dim dsUserPreference As DataSet
        Dim strProdDisp As String = m_cProdDispType_CatalogSQL

        Dim strShitpTo As String = ""
        Dim strDept As String = ""
        Dim strBusinessUnit As String = ""
        Dim strShipToTable As New DataTable
        strShipToTable.Columns.Add("strShitpTo")
        Dim strShipToTable_rows As DataRow = strShipToTable.NewRow()
        Try
            strBusinessUnit = SelectedSite.Trim
            If Trim(strBusinessUnit) = "" Then
                strBusinessUnit = strBU
            End If

        Catch ex As Exception
            strBusinessUnit = strBU
        End Try

        strSQLstring = "SELECT ISA_PROD_DISPLAY, ISA_PRICE_BLOCK,ISA_SDI_EMPLOYEE,SHIPTO_DEFAULT,ISA_DEPT FROM SDIX_USERS_TBL" & vbCrLf &
            " WHERE ISA_EMPLOYEE_ID = '" & Trim(UserId).ToUpper & "'"
        Try
            'strProdDisp = ORDBData.GetScalar(strSQLstring)
            dsUserPreference = ORDBData.GetAdapter(strSQLstring)
            dsUserPreference.Tables(0).TableName = "PreferenceTable"
            _result.Tables.Add(dsUserPreference.Tables(0).Copy())

            strSQLstring = "SELECT SHIP_TO_FLG FROM PS_ISA_ENTERPRISE WHERE ISA_BUSINESS_UNIT='" & strBusinessUnit & "' "

            strShitpTo = ORDBData.GetScalar(strSQLstring)
            strShipToTable_rows("strShitpTo") = strShitpTo
            strShipToTable.Rows.Add(strShipToTable_rows)
            strShipToTable.TableName = "Default_ShipTo"
            _result.Tables.Add(strShipToTable)

            If Not dsUserPreference Is Nothing And dsUserPreference.Tables(0).Rows.Count > 0 Then
                Dim drPreference As DataRow = dsUserPreference.Tables(0).Rows(0)
                strProdDisp = Convert.ToString(drPreference("ISA_PROD_DISPLAY"))
                '20/07/2020 Handle in the front end
                'If Convert.ToString(drPreference("ISA_PRICE_BLOCK")).ToUpper().Equals("Y") Then
                '    chkbxDisplayPrice.Checked = True
                'Else
                '    chkbxDisplayPrice.Checked = False
                'End If
                'Load ship to location 
                If Convert.ToString(drPreference("ISA_SDI_EMPLOYEE")).ToUpper().Equals("C") Or strShitpTo = "Y" Then
                    defaultShipTo = Convert.ToString(drPreference("SHIPTO_DEFAULT"))
                    Try
                        Dim shiptoDs As DataSet = getShipToLoc(UCase(strBusinessUnit))
                        If Not shiptoDs Is Nothing And shiptoDs.Tables(0).Rows.Count > 0 Then
                            Dim ShipToTable As New DataTable
                            ShipToTable = shiptoDs.Tables(0).Copy()
                            ShipToTable.TableName = "ShipToTable"
                            _result.Tables.Add(ShipToTable)
                            '20/07/2020 Handle in the front end
                            'lblShipto.Visible = True
                            'dropShipto.Visible = True
                            'dropShipto.DataSource = shiptoDs
                            'dropShipto.DataValueField = "CUSTID"
                            'dropShipto.DataTextField = "locname"
                            'dropShipto.DataBind()
                            'dropShipto.Items.Insert(0, "-- Select ShipTo --")
                            ''Chaeck default location is available in user table or not.
                            'If Not defaultShipTo = "" And Not defaultShipTo = Nothing Then
                            '    dropShipto.SelectedValue = defaultShipTo
                            'End If
                        End If
                    Catch ex As Exception

                    End Try
                    '20/07/2020 Handle in the Front end 
                    'Else
                    '    lblShipto.Visible = False
                    '    dropShipto.Visible = False
                    '    If Not dropShipto.DataSource = Nothing Then
                    '        dropShipto.SelectedValue = 0
                    '    End If
                End If

                If Convert.ToString(drPreference("ISA_SDI_EMPLOYEE")).ToUpper().Equals("S") Then
                    strDept = Convert.ToString(drPreference("ISA_DEPT"))
                    Try
                        strSQLstring = "SELECT DEPT_ID,DEPT_NAME FROM SDIX_TCKT_DEPT"
                        Dim deptDs As DataSet = ORDBData.GetAdapter(strSQLstring)
                        deptDs.Tables(0).TableName = "DepartList"
                        If Not deptDs Is Nothing And deptDs.Tables(0).Rows.Count > 0 Then
                            Dim DepartList As New DataTable
                            DepartList = deptDs.Tables(0).Copy()
                            DepartList.TableName = "DepartList"
                            _result.Tables.Add(DepartList)
                            '_result.Tables.Add(deptDs.Tables(0))
                            '20/07/2020 handle in the front end
                            'lblUserDept.Visible = True
                            'drpDept.Visible = True
                            'drpDept.DataSource = deptDs
                            'drpDept.DataValueField = "DEPT_ID"
                            'drpDept.DataTextField = "DEPT_NAME"
                            'drpDept.DataBind()
                            'drpDept.Items.Insert(0, "-- Select Dept --")
                            'If Not strDept = "" And Not strDept = Nothing Then
                            '    drpDept.SelectedValue = strDept
                            'End If
                        End If

                    Catch ex As Exception

                    End Try
                    '20/07/2020 handle in the front end
                    'Else
                    '    lblUserDept.Visible = False
                    '    drpDept.Visible = False
                    '    If Not drpDept.DataSource = Nothing Then
                    '        drpDept.SelectedValue = 0
                    '    End If

                End If

            End If
            Return _result
        Catch ex As Exception
            '20/07/2020 Handle in the front end
            'strProdDisp = m_cProdDispType_CatalogSQL
            'chkbxDisplayPrice.Checked = False
        End Try
        '20/07/2020 Handle in the front end
        'If strProdDisp = m_cProdDispType_PSClient Then
        '    rrbProdDispPSClient.Checked = True
        '    rrbProdDispCatSQL.Checked = False
        'Else
        '    rrbProdDispCatSQL.Checked = True
        '    rrbProdDispPSClient.Checked = False
        'End If
    End Function
    Private Function LoadOrderStatusEmail(ByVal userBU As String, ByVal userID As String, ByVal Session_EMAIL As String, ByVal Session_BUSUNIT As String, ByVal Session_USERTYPE As String)
        Dim _result As New DataSet
        Try
            _result = getPrivleges(userID, userBU, Session_EMAIL, Session_BUSUNIT, Session_USERTYPE)
        Catch ex As Exception

        End Try
        Return _result
    End Function
    Private Function getPrivleges(ByVal strUserid As String, ByVal strbu As String, ByVal Session_EMAIL As String, ByVal Session_BUSUNIT As String, ByVal Session_USERTYPE As String)
        Dim _result As New DataSet
        Try
            'lblUpdMsg.Text = ""  20/07/2020
            Dim strItem As String
            Dim hashPrivs As Hashtable
            hashPrivs = New Hashtable
            Dim SessionTable As New DataTable
            SessionTable.Columns.Add("Session_EMAIL")
            SessionTable.Columns.Add("Session_BUSUNIT")
            SessionTable.Columns.Add("Session_USERTYPE")
            Dim _Rows As DataRow = SessionTable.NewRow()
            _Rows("Session_EMAIL") = Session_EMAIL
            _Rows("Session_BUSUNIT") = Session_BUSUNIT
            _Rows("Session_USERTYPE") = Session_USERTYPE

            Dim roleHashPrivs As Hashtable = Nothing
            hashPrivs = getprivhashtable(strUserid, strbu, "Y", _Rows, roleHashPrivs)

            Dim PrivHashTable As New DataTable

            PrivHashTable = ConvertHashtableRowsToDataTableColumns(hashPrivs)
            _result.Tables.Add(PrivHashTable)
            '20/07/2020 Handle in the front end
            'If hashPrivs.ContainsKey("NONE") Then
            '    chbCRE.Checked = False
            '    chbQTW.Checked = False
            '    chbQTC.Checked = False
            '    chbQTS.Checked = False
            '    chbCST.Checked = False
            '    chbVND.Checked = False
            '    chbAPR.Checked = False
            '    chbQTA.Checked = False
            '    chbQTR.Checked = False
            '    chbRFA.Checked = False
            '    chbRFR.Checked = False
            '    chbRFC.Checked = False
            '    chbRCF.Checked = False
            '    chbRCP.Checked = False
            '    chbCNC.Checked = False
            '    chbDLF.Checked = False
            '    Exit Sub
            'End If
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
            '20/07/2020  handle in the front end 
            'For Each strItem In hashPrivs.Keys
            '    Select Case strItem
            '        Case "EMAILCRE01"
            '            chbCRE.Checked = True
            '        Case "EMAILQTW02"
            '            chbQTW.Checked = True
            '        Case "EMAILQTC03"
            '            chbQTC.Checked = True
            '        Case "EMAILQTS04"
            '            chbQTS.Checked = True
            '        Case "EMAILCST05"
            '            chbCST.Checked = True
            '        Case "EMAILVND06"
            '            chbVND.Checked = True
            '        Case "EMAILAPR07"
            '            chbAPR.Checked = True
            '        Case "EMAILQTA08"
            '            chbQTA.Checked = True
            '        Case "EMAILQTR09"
            '            chbQTR.Checked = True
            '        Case "EMAILRFA10"
            '            chbRFA.Checked = True
            '        Case "EMAILRFR11"
            '            chbRFR.Checked = True
            '        Case "EMAILRFC12"
            '            chbRFC.Checked = True
            '        Case "EMAILRCF13"
            '            chbRCF.Checked = True
            '        Case "EMAILRCP14"
            '            chbRCP.Checked = True
            '        Case "EMAILCNC15"
            '            chbCNC.Checked = True
            '        Case "EMAILDLF16"
            '            chbDLF.Checked = True
            '    End Select
            'Next
            'If clsAccessPrivileges.IsPrivilgEqualsN(strUserid, strbu, _
            '                    clsAccessPrivileges.UserPrivsEnum.SendEmailRecvdPo) Then  'Case "EMLRECVD"
            '    chbRecvdPo.Checked = False
            'Else
            '    chbRecvdPo.Checked = True
            'End If
            Dim txtCustSrvFlag As New DataTable
            txtCustSrvFlag.Columns.Add("txtCustSrvFlag")
            Dim txtCustSrvFlag_Row As DataRow = txtCustSrvFlag.NewRow()
            If clsAccessPrivileges.IsPrivilegeOn(strUserid, strbu, clsAccessPrivileges.UserPrivsEnum.IncidentAssign) Then
                txtCustSrvFlag_Row("txtCustSrvFlag") = "Y"
                txtCustSrvFlag.Rows.Add(txtCustSrvFlag_Row)
            Else
                Dim objUserTbl As New clsUserTbl(strUserid, strbu)
                txtCustSrvFlag_Row("txtCustSrvFlag") = objUserTbl.CustSrvFlag
                txtCustSrvFlag.Rows.Add(txtCustSrvFlag_Row)
            End If
            _result.Tables.Add(txtCustSrvFlag)
            '20/07/2020 handle in the front end
            'If Not roleHashPrivs Is Nothing Then
            '    For Each strItem In roleHashPrivs.Keys
            '        Select Case strItem
            '            Case "EMAILCRE01"
            '                chbCRE.Enabled = True
            '            Case "EMAILQTW02"
            '                chbQTW.Enabled = True
            '            Case "EMAILQTC03"
            '                chbQTC.Enabled = True
            '            Case "EMAILQTS04"
            '                chbQTS.Enabled = True
            '            Case "EMAILCST05"
            '                chbCST.Enabled = True
            '            Case "EMAILVND06"
            '                chbVND.Enabled = True
            '            Case "EMAILAPR07"
            '                chbAPR.Enabled = True
            '            Case "EMAILQTA08"
            '                chbQTA.Enabled = True
            '            Case "EMAILQTR09"
            '                chbQTR.Enabled = False
            '            Case "EMAILRFA10"
            '                chbRFA.Enabled = True
            '            Case "EMAILRFR11"
            '                chbRFR.Enabled = True
            '            Case "EMAILRFC12"
            '                chbRFC.Enabled = True
            '            Case "EMAILRCF13"
            '                chbRCF.Enabled = True
            '            Case "EMAILRCP14"
            '                chbRCP.Enabled = True
            '            Case "EMAILCNC15"
            '                chbCNC.Enabled = True
            '            Case "EMAILDLF16"
            '                chbDLF.Enabled = True
            '        End Select
            '    Next  ' For Each strItem In roleHashPrivs.Keys
            '    lblOrdStatusEml.Text = UCase(strUserid) & " is assigned a Role."
            'End If ' If Not roleHashPrivs Is Nothing Then
        Catch
        End Try
        Return _result
    End Function
    Public Function ConvertHashtableRowsToDataTableColumns(ByVal hashtable As System.Collections.Hashtable) As DataTable
        Dim dataTable = New DataTable(hashtable.[GetType]().Name)
        dataTable.Columns.Add("Key", GetType(Object))
        dataTable.Columns.Add("Value", GetType(Object))
        Dim enumerator As IDictionaryEnumerator = hashtable.GetEnumerator()

        While enumerator.MoveNext()
            dataTable.Rows.Add(enumerator.Key, enumerator.Value)
        End While

        Return dataTable
    End Function

    Private Function LoadApprovals(ByVal userID As String, ByVal userBU As String, ByVal Session_ThirdParty_CompanyID As String)
        Const SESSION_SITE_CURRENCY As String = "__siteCurrency"
        Dim m_siteCurrency As sdiCurrency = Nothing
        Dim _result As New DataSet
        Try
            m_siteCurrency = sdiMultiCurrency.getSiteCurrency(userBU)
            Dim siteCurrencyTable As DataTable = New DataTable
            siteCurrencyTable.Columns.Add("id")
            siteCurrencyTable.Columns.Add("Description")
            siteCurrencyTable.Columns.Add("Country")
            siteCurrencyTable.Columns.Add("IsKnownCurrency")
            siteCurrencyTable.Columns.Add("ShortDescription")
            siteCurrencyTable.Columns.Add("Symbol")
            Dim _row As DataRow = siteCurrencyTable.NewRow()
            _row("id") = m_siteCurrency.Id
            _row("Description") = m_siteCurrency.Description
            _row("Country") = m_siteCurrency.Country
            _row("IsKnownCurrency") = m_siteCurrency.IsKnownCurrency
            _row("ShortDescription") = m_siteCurrency.ShortDescription
            _row("Symbol") = m_siteCurrency.Symbol
            siteCurrencyTable.Rows.Add(_row)
            siteCurrencyTable.TableName = "siteCurrencyTable"

            _result = getApprovals(userID, userBU, Session_ThirdParty_CompanyID)
            _result.Tables.Add(siteCurrencyTable)
            'Dim siteBU As String = ""
            'Try
            '    siteBU = CStr(Session_UserBu).Trim.ToUpper
            '    If (siteBU Is Nothing) Then
            '        siteBU = ""
            '    End If
            'Catch ex As Exception
            'End Try
            'm_siteCurrency = Nothing
            'If Page.IsPostBack Then
            '    ' retrieve from session var
            '    Try
            '        m_siteCurrency = CType(Session(SESSION_SITE_CURRENCY), sdiCurrency)
            '    Catch ex As Exception
            '    End Try
            'End If
            'If (Not Page.IsPostBack) Or
            '   (m_siteCurrency Is Nothing) Then
            '    m_siteCurrency = sdiMultiCurrency.getSiteCurrency(siteBU)
            '    Session(SESSION_SITE_CURRENCY) = m_siteCurrency
            'End If

            'Me.lblSiteBaseCurrencyCode.Text = ""
            'Try
            '    Me.lblSiteBaseCurrencyCode.Text = m_siteCurrency.Id
            'Catch ex As Exception
            'End Try

            'btnSetReqAppr.Visible = True
        Catch ex As Exception

        End Try
        Return _result
    End Function
    Private Function getApprovals(ByVal strUserid As String, ByVal strBU As String, ByVal Session_ThirdParty_CompanyID As String)
        Dim strSQLstring As String
        Dim strAppEmpID As String = ""
        Dim _result As New DataSet
        Dim ExistBudApprover As New DataTable
        Dim BudApproverDD As New DataTable
        BudApproverDD.TableName = "BudApproverDD"

        Try
            strSQLstring = "SELECT ISA_IOL_APR_EMP_ID, ISA_IOL_APR_LIMIT, ISA_IOL_APR_ALT " & vbCrLf &
                " FROM SDIX_USERS_APPRV" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & strUserid & "'" & vbCrLf &
                " AND BUSINESS_UNIT = '" & strBU & "'"

            Dim dtrAprReader As DataSet = ORDBData.GetAdapter(strSQLstring)
            ExistBudApprover = dtrAprReader.Tables(0).Copy()
            ExistBudApprover.TableName = "ExistBudApprover"
            '20/072020 Handle in Front End
            If ExistBudApprover.Rows.Count > 0 Then
                ' If dtrAprReader.Read Then
                '    txtAppExist.Text = "UPD"
                '    strAppEmpID = dtrAprReader.Item("ISA_IOL_APR_EMP_ID")
                '    txtAppTotal.Text = dtrAprReader.Item("ISA_IOL_APR_LIMIT")
                m_sAppTotalOrig = ExistBudApprover.Rows(0)("ISA_IOL_APR_LIMIT")
                m_sAppEmpIDOrig = ExistBudApprover.Rows(0)("ISA_IOL_APR_EMP_ID")
                m_sAppAltOrig = ExistBudApprover.Rows(0)("ISA_IOL_APR_ALT")
                'Else
                '    txtAppExist.Text = "ADD"
                '    m_sAppTotalOrig = ""
                '    m_sAppEmpIDOrig = ""
                '    m_sAppAltOrig = ""
                'End If
            End If
            'dtrAprReader.Close()
            _result.Tables.Add(ExistBudApprover)
            If String.IsNullOrEmpty(Session_ThirdParty_CompanyID) Then
                Session_ThirdParty_CompanyID = " "
            End If
            '20/07/2020
            'Session("APPR_TOTAL") = m_sAppTotalOrig
            'Session("APPR_APR_EMPID") = m_sAppEmpIDOrig
            'Session("APPR_APR_ALT") = m_sAppAltOrig
            If Session_ThirdParty_CompanyID = "0" Or Session_ThirdParty_CompanyID = " " Then
                strSQLstring = "SELECT Distinct (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A JOIN SDIX_MULTI_SITE B" & vbCrLf &
                        " ON A.ISA_EMPLOYEE_ID=B.ISA_EMPLOYEE_ID WHERE B.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND " & vbCrLf &
                    "NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' " & vbCrLf &
                    "UNION SELECT (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A WHERE " & vbCrLf &
                    "A.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' ORDER BY EMP_TEXT"
            Else
                strSQLstring = "SELECT Distinct (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A JOIN SDIX_MULTI_SITE B" & vbCrLf &
                        " ON A.ISA_EMPLOYEE_ID=B.ISA_EMPLOYEE_ID WHERE B.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND " & vbCrLf &
                    "NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' And THIRDPARTY_COMP_ID = '" & Session_ThirdParty_CompanyID & "'" & vbCrLf &
                    "UNION SELECT (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A WHERE " & vbCrLf &
                    "A.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' ORDER BY EMP_TEXT"
            End If
            Dim dtrEMPReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
            BudApproverDD.Load(dtrEMPReader)
            '20/07/2020 Handle in the front end.
            'DropAppEmpID.DataSource = dtrEMPReader
            'DropAppEmpID.DataValueField = "ISA_EMPLOYEE_ID"
            'DropAppEmpID.DataTextField = "EMP_TEXT"
            'DropAppEmpID.DataBind()
            'DropAppEmpID.Items.Insert(0, New ListItem("-- ALL --"))
            'If Not DropAppEmpID.Items.FindByValue(strAppEmpID) Is Nothing Then
            '    DropAppEmpID.Items.FindByValue(strAppEmpID).Selected = True
            'End If

            dtrEMPReader.Close()
            _result.Tables.Add(BudApproverDD)
        Catch ex As Exception
        End Try
        Return _result
    End Function
    Private Function LoadReqApprovals(ByVal userID As String, ByVal userBU As String, ByVal Session_ThirdParty_CompanyID As String)
        Dim _result As New DataSet
        Try
            'Hiding the Budgetory Approval fields
            'lblAppEmpID.Visible = False
            'DropAppEmpID.Visible = False
            'chbDelete.Visible = False
            'lblAppTotal.Visible = False
            'txtAppTotal.Visible = False
            'btnSubmit.Visible = False
            'lblMsg.Visible = False
            ''btnSetReqAppr.Visible = False
            'lblSiteBaseCurrencyCode.Text = ""
            'tr_OrderLimit_fields.Style.Add("display", "none")

            ''Showing the Requestor Approval fields
            'lblAltReqAppr.Visible = True
            'ddReqAppr.Visible = True
            'chkDeleteReqAppr.Visible = True
            'btnSubmitReqAppr.Visible = True
            'lblMsgReqAppr.Visible = True
            ''btnSetBudAppr.Visible = True
            'lblMsgReqAppr.Text = ""

            'If userType = "SUPER" Then
            '    btnSetReqAppr.Visible = True
            '    btnSetBudAppr.Visible = False
            'Else
            '    btnSetReqAppr.Visible = False
            '    btnSetBudAppr.Visible = False
            'End If

            'Dim strUserID As String = String.Empty
            'Dim strBU As String = String.Empty

            'If Session("USERTYPE") = "SUPER" Then
            '    strBU = GetBUbyGroup(rcbGroup.Items(rcbGroup.SelectedIndex).Value)
            'Else
            '    strBU = GetBUbyGroup(txtGroupID.Text)
            'End If
            'strUserID = Trim(txtUserid.Text)

            _result = getReqApprovals(userID, userBU, Session_ThirdParty_CompanyID)
        Catch ex As Exception

        End Try
        Return _result
    End Function
    Private Function getReqApprovals(ByVal strUserid As String, ByVal strBU As String, ByVal Session_ThirdParty_CompanyID As String)

        Dim strSQLstring As String
        Dim strAppEmpID As String = ""
        Dim _result As New DataSet
        Dim ExistReqApprover As New DataTable
        Dim ReqApproverDD As New DataTable
        ReqApproverDD.TableName = "ReqApproverDD"
        ExistReqApprover.TableName = "ExistReqApprover"
        Try
            strSQLstring = "SELECT ISA_REQ_APR_ALT " & vbCrLf &
                " FROM SDIX_USERS_REQ_APPRV" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & strUserid & "'" & vbCrLf &
                " AND BUSINESS_UNIT = '" & strBU & "'"

            Dim dtrAprReader As DataSet = ORDBData.GetAdapter(strSQLstring)
            ExistReqApprover = dtrAprReader.Tables(0).Copy()
            ExistReqApprover.TableName = "ExistReqApprover"
            If ExistReqApprover.Rows.Count > 0 Then
                'If dtrAprReader.Read Then
                'txtAppExist.Text = "UPD" 20/07/2020
                m_sReq_AltAppr_EmpID = strUserid
                m_sReq_AltAppr_Orig = ExistReqApprover.Rows(0)("ISA_REQ_APR_ALT")
            Else
                'txtAppExist.Text = "ADD" 20/07/2020
                m_sReq_AltAppr_EmpID = ""
                m_sReq_AltAppr_Orig = ""
                'm_sAppEmpIDOrig = ""
                '    m_sAppAltOrig = ""
                'End If
            End If
            'dtrAprReader.Close()
            _result.Tables.Add(ExistReqApprover)
            If String.IsNullOrEmpty(Session_ThirdParty_CompanyID) Then
                Session_ThirdParty_CompanyID = ""
            End If
            '20/07/2020 Hnadle in the front end
            'Session("REQ_APPR_EMPID") = m_sReq_AltAppr_EmpID
            'Session("REQ_APPR_ALT") = m_sReq_AltAppr_Orig
            '==================End
            'strSQLstring = "SELECT Distinct (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A JOIN SDIX_MULTI_SITE B" & vbCrLf & _
            '                " ON A.ISA_EMPLOYEE_ID=B.ISA_EMPLOYEE_ID WHERE B.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND " & vbCrLf & _
            '            "NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' " & vbCrLf & _
            '            "UNION SELECT (A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT,A.ISA_EMPLOYEE_ID FROM SDIX_USERS_TBL A WHERE " & vbCrLf & _
            '            "A.BUSINESS_UNIT='" & strBU & "' AND A.ISA_SDI_EMPLOYEE = 'C' AND NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND A.ACTIVE_STATUS = 'A' ORDER BY EMP_TEXT"
            If Session_ThirdParty_CompanyID = "0" Or Session_ThirdParty_CompanyID = "" Then
                strSQLstring = "SELECT Distinct(A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT, A.ISA_EMPLOYEE_ID FROM PS_ISA_EMPL_TBL A, SDIX_USERS_TBL B" & vbCrLf &
                        "WHERE A.BUSINESS_UNIT = '" & strBU & "'  AND A.EFF_STATUS = 'A' AND NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND " & vbCrLf &
                        "B.ISA_EMPLOYEE_ID = A.ISA_EMPLOYEE_ID AND B.ACTIVE_STATUS = 'A' AND B.ISA_SDI_EMPLOYEE = 'C' ORDER BY EMP_TEXT"
            Else
                strSQLstring = "SELECT Distinct(A.ISA_EMPLOYEE_NAME || ' - ' || A.ISA_EMPLOYEE_ID) as EMP_TEXT, A.ISA_EMPLOYEE_ID FROM PS_ISA_EMPL_TBL A, SDIX_USERS_TBL B" & vbCrLf &
                        "WHERE A.BUSINESS_UNIT = '" & strBU & "'  AND A.EFF_STATUS = 'A' AND NOT A.ISA_EMPLOYEE_ID = '" & strUserid & "' AND " & vbCrLf &
                        "B.ISA_EMPLOYEE_ID = A.ISA_EMPLOYEE_ID AND B.ACTIVE_STATUS = 'A' And THIRDPARTY_COMP_ID = '" & Session_ThirdParty_CompanyID & "' AND B.ISA_SDI_EMPLOYEE = 'C' ORDER BY EMP_TEXT"
            End If

            Dim dtrEMPReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
            ReqApproverDD.Load(dtrEMPReader)
            '20/07/2020 handle in the front end
            'ddReqAppr.DataSource = dtrEMPReader
            'ddReqAppr.DataValueField = "ISA_EMPLOYEE_ID"
            'ddReqAppr.DataTextField = "EMP_TEXT"
            'ddReqAppr.DataBind()
            'ddReqAppr.Items.Insert(0, New ListItem("-- ALL --"))
            'If Not ddReqAppr.Items.FindByValue(m_sReq_AltAppr_Orig) Is Nothing Then
            '    ddReqAppr.Items.FindByValue(m_sReq_AltAppr_Orig).Selected = True
            'End If
            dtrEMPReader.Close()
            _result.Tables.Add(ReqApproverDD)
        Catch ex As Exception
        End Try
        Return _result
    End Function
    Private Function checkUserid(ByVal UserId As String) As Boolean
        Dim strSQLstring As String = "Select isa_user_id" & vbCrLf &
                    " FROM SDIX_USERS_TBL" & vbCrLf &
                    " WHERE isa_employee_id = '" & UserId.ToUpper & "'" & vbCrLf &
                    " AND ACTIVE_STATUS = 'A'" & vbCrLf

        Dim dsUserid As DataSet = ORDBData.GetAdapter(strSQLstring)

        If dsUserid.Tables(0).Rows.Count = 0 Then
            checkUserid = False
        ElseIf dsUserid.Tables(0).Rows.Count > 1 Then
            'Dim strMessage As New Alert 20/07/2020
            checkUserid = True
            'ltlAlert.Text = strMessage.Say("Error - User exists more than once in user table!")
        Else
            checkUserid = True
        End If
    End Function
    '*UP_PC_188
    '*
    Public Function btnSetBudAppr_Click(ByVal data As String) As String
        Dim _result As New DataSet
        Try

            If Not String.IsNullOrEmpty(data) Then
                Dim objSetBudAppBo As SetBudAppBo = JsonConvert.DeserializeObject(Of SetBudAppBo)(data)
                Dim Error_Table As New DataTable

                Error_Table.Columns.Add("BU_Invalid")
                'Showing the Budgetory Approval fields
                '20/07/2020 Handle in the Front end
                'lblAppEmpID.Visible = True
                'DropAppEmpID.Visible = True
                'chbDelete.Visible = True
                'lblAppTotal.Visible = True
                'txtAppTotal.Visible = True
                'btnSubmit.Visible = True
                'lblMsg.Visible = True
                'btnSetReqAppr.Visible = True
                'lblSiteBaseCurrencyCode.Text = ""
                'tr_OrderLimit_fields.Style.Add("display", "contents")

                ''Hiding the Requestor Approval fields
                'lblAltReqAppr.Visible = False
                'ddReqAppr.Visible = False
                'chkDeleteReqAppr.Visible = False
                'btnSubmitReqAppr.Visible = False
                'lblMsgReqAppr.Visible = False
                'btnSetBudAppr.Visible = False
                'lblMsgReqAppr.Text = ""

                'If Session("BUSUNIT") = "" Then
                '    Session.RemoveAll()
                '    Response.Redirect("default.aspx")
                'End If
                'txtAppTotal.Text = ""
                'lblMsg.Text = ""
                Dim strBU As String
                'Dim strMessage As New Alert  20/07/2020
                If objSetBudAppBo.Session_UserRole = "SUPER" Then
                    strBU = GetBUbyGroup(objSetBudAppBo.SiteValue)
                Else
                    strBU = GetBUbyGroup(objSetBudAppBo.SiteValue)
                End If
                If strBU = "0" Then
                    'RadMultiPage1.RenderSelectedPageOnly = True  20/07/2020
                    'RadMultiPage1.SelectedIndex = 0
                    'tbStripUserDetails.Tabs(0).Selected = True
                    Dim Error_TablrRows As DataRow = Error_Table.NewRow()
                    Error_TablrRows("BU_Invalid") = "Error - Invalid Business Unit - check productview id!"
                    Error_Table.Rows.Add(Error_TablrRows)
                    _result.Tables.Add(Error_Table)
                    data = JsonConvert.SerializeObject(_result)
                    Return data
                    'ltlAlert.Text = strMessage.Say("Error - Invalid Business Unit - check productview id's!"
                    'ltlAlert.Text = "Error - Invalid Business Unit - check productview id''s!"
                    'ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Error - Invalid Business Unit - check productview id!"), True)
                    Exit Function
                End If

                Dim sOrdApprType As String = "O"
                'Dim objEnterprise As New clsEnterprise(strBU)
                Select Case sOrdApprType  '  objEnterprise.OrdApprType
                    Case "O", "D", "M"
                        'OK 
                    Case Else
                        'RadMultiPage1.RenderSelectedPageOnly = True   20/07/2020
                        'RadMultiPage1.SelectedIndex = 0
                        'tbStripUserDetails.Tabs(0).Selected = True
                        Dim Error_TablrRows As DataRow = Error_Table.NewRow()
                        Error_TablrRows("BU_Invalid") = "Business unit is not set up as an approver site."
                        Error_Table.Rows.Add(Error_TablrRows)
                        _result.Tables.Add(Error_Table)
                        data = JsonConvert.SerializeObject(_result)
                        Return data
                        'ltlAlert.Text = strMessage.Say("Business unit is not set up as an approver site.")
                        'ltlAlert.Text = "Business unit is not set up as an approver site."
                        'ScriptManager.RegisterStartupScript(Page, Me.GetType, Guid.NewGuid.ToString(), String.Format("alert('{0}');", "Business unit is not set up as an approver site."), True)
                        Exit Function
                End Select

                _result = LoadApprovals(objSetBudAppBo.UserId, strBU, objSetBudAppBo.Session_ThirdParty_CompanyID)
            End If
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty

        End Try

        Return data
    End Function
    '*UP_PC_192
    '*
    Public Function btnSetReqAppr_Click(ByVal data As String) As String

        'Hiding the Budgetory Approval fields
        '20/07/2020 Handle in the front end
        'lblAppEmpID.Visible = False
        'DropAppEmpID.Visible = False
        'chbDelete.Visible = False
        'lblAppTotal.Visible = False
        'txtAppTotal.Visible = False
        'btnSubmit.Visible = False
        'lblMsg.Visible = False
        'btnSetReqAppr.Visible = False
        'lblSiteBaseCurrencyCode.Text = ""
        'tr_OrderLimit_fields.Style.Add("display", "none")

        ''Showing the Requestor Approval fields
        'lblAltReqAppr.Visible = True
        'ddReqAppr.Visible = True
        'chkDeleteReqAppr.Visible = True
        'btnSubmitReqAppr.Visible = True
        'lblMsgReqAppr.Visible = True
        'btnSetBudAppr.Visible = True

        'lblMsgReqAppr.Text = ""
        Dim _result As New DataSet
        Try

            If Not String.IsNullOrEmpty(data) Then
                Dim objSetReqAppBo As SetReqAppBo = JsonConvert.DeserializeObject(Of SetReqAppBo)(data)

                Dim strUserID As String = String.Empty
                Dim strBU As String
                'Dim strMessage As New Alert  20/07/2020
                If objSetReqAppBo.Session_UserRole = "SUPER" Then
                    strBU = GetBUbyGroup(objSetReqAppBo.SiteValue)
                Else
                    strBU = GetBUbyGroup(objSetReqAppBo.SiteValue)
                End If
                strUserID = Trim(objSetReqAppBo.UserId)

                _result = getReqApprovals(strUserID, strBU, objSetReqAppBo.Session_ThirdParty_CompanyID)
            End If
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '*UP_PC_190
    '*
    Public Function btnSubmit_Click(ByVal data As String) As String
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.Columns.Add("DeleteSuccess")
            SuceessMessage.Columns.Add("DeleteFailed")
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("InsertFailed")
            If Not String.IsNullOrEmpty(data) Then
                Dim objUpdBudAppBo As UpdBudAppBo = JsonConvert.DeserializeObject(Of UpdBudAppBo)(data)
                Dim strBU As String = ""
                If objUpdBudAppBo.Session_UserRole = "SUPER" Then
                    strBU = GetBUbyGroup(objUpdBudAppBo.SiteValue)
                Else
                    strBU = GetBUbyGroup(objUpdBudAppBo.SiteValue)
                End If
                ExistsApprvRecord(strBU, objUpdBudAppBo.UserId)
                If objUpdBudAppBo.Session_UserRole = "ADMIN" Or
                   objUpdBudAppBo.Session_UserRole = "ADMINX" Or
                   objUpdBudAppBo.Session_UserRole = "ADMINA" Or
                   objUpdBudAppBo.Session_UserRole = "SUPER" Then
                    '20/07/2020 Handle in the front end
                    'If DropAppEmpID.SelectedIndex = 0 Then
                    '    lblMsg.Text = "A valid approval name must be selected"
                    '    Exit Sub
                    'Else
                    '    lblMsg.Text = ""
                    'End If
                    'If txtAppTotal.Text = "" Then
                    '    txtAppTotal.Text = "0"
                    'End If
                    If objUpdBudAppBo.Delete_Checked Then
                        deleteAPPRVRecord(objUpdBudAppBo.SiteValue, objUpdBudAppBo.UserId, objUpdBudAppBo.Session_UserId, objUpdBudAppBo.Session_WEBSITED, objUpdBudAppBo.Session_UserRole, SuceessMessage)
                        '20/07/2020 Handle in the front end
                        'chbDelete.Checked = False
                        'txtAppTotal.Text = ""
                        'DropAppEmpID.SelectedIndex = 0
                    Else
                        updateAPPRVTable(objUpdBudAppBo.BudApp_Selected, objUpdBudAppBo.BudLimit, objUpdBudAppBo.AppExist, objUpdBudAppBo.Session_UserId, objUpdBudAppBo.SiteValue, objUpdBudAppBo.Session_UserRole, objUpdBudAppBo.UserId, objUpdBudAppBo.Session_WEBSITED, SuceessMessage)
                    End If
                End If
            End If
            _result.Tables.Add(SuceessMessage)
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    Public Function deleteAPPRVRecord(ByVal SiteSelected As String, ByVal UserId As String, ByVal Session_UserID As String,
                                      ByVal Session_WEBSITED As String, ByVal Session_UserRole As String, ByRef SuceessMessage As DataTable)
        Dim rowsaffected As Integer = 0
        Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
        Dim strBU As String = ""
        If Session_UserRole = "SUPER" Then
            strBU = GetBUbyGroup(SiteSelected)
        Else
            strBU = GetBUbyGroup(SiteSelected)
        End If
        Dim strSQLstring As String = ""
        strSQLstring = "DELETE FROM SDIX_USERS_APPRV" & vbCrLf &
                    " WHERE ISA_EMPLOYEE_ID = '" & UserId & "'" & vbCrLf &
                    " AND BUSINESS_UNIT = '" & strBU & "'"

        rowsaffected = ORDBData.ExecNonQueryWithTransaction(strSQLstring)

        If rowsaffected > 0 Then
            clsSDIAudit.AddRecord("profile.aspx", "Delete Order Limit Record", "SDIX_USERS_APPRV", Session_UserID.ToString, strBU, UserId, Session_WEBSITED,
                      sUDF1:="Limit " & m_sAppTotalOrig, sUDF2:="Approver " & m_sAppEmpIDOrig, sUDF3:="AltApprover " & m_sAppAltOrig)
            m_sAppTotalOrig = ""
            m_sAppEmpIDOrig = ""
            m_sAppAltOrig = ""
            '20/07/2020 Handle in the front end
            'Session("APPR_TOTAL") = m_sAppTotalOrig
            'Session("APPR_APR_EMPID") = m_sAppEmpIDOrig
            'Session("APPR_APR_ALT") = m_sAppAltOrig
            SuceessMessage_row("DeleteSuccess") = "Budgetory approver details deleted successfully."
            SuceessMessage.Rows.Add(SuceessMessage_row)
            'Label1.Text =  20/07/2020
            'lblMessage1N.Text = Label1.Text
            'lblMessage.Text = "Budgetory Approver details Deleted Sucessfully."
            'lblMessage.Visible = True
            'lblMsg.Text = "Budgetory Approver details Deleted Sucessfully."
        Else
            SuceessMessage_row("DeleteFailed") = "Budgetory approver details deleted failed."
            SuceessMessage.Rows.Add(SuceessMessage_row)
        End If

    End Function
    Private Sub updateAPPRVTable(ByVal BudApp_Selected As String, ByVal BudLimit As String, ByVal AppExist As String, ByVal Session_UserID As String,
                                 ByVal SiteSelected As String, ByVal Session_Userole As String, ByVal UserId As String, ByVal Session_WEBSITED As String, ByRef SuceessMessage As DataTable)
        Dim strBU As String
        Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
        If Session_Userole = "SUPER" Then
            strBU = GetBUbyGroup(SiteSelected)
        Else
            strBU = GetBUbyGroup(SiteSelected)
        End If

        Dim strSQLstring As String
        Dim bUpdate As Boolean = (AppExist = "UPD")

        If Not bUpdate Then
            If ExistsApprvRecord(strBU, UserId) Then
                ' Verify that the record doesn't already exist if we're to insert the record.
                ' If the record already exists, change the "insert" action to an "update" action.
                bUpdate = True
            End If
        End If
        If BudLimit = "" Then
            BudLimit = "0"
        End If
        If bUpdate Then
            strSQLstring = "UPDATE SDIX_USERS_APPRV" & vbCrLf &
                " SET ISA_IOL_APR_EMP_ID = '" & BudApp_Selected & "'," & vbCrLf &
                " ISA_IOL_APR_ALT = '" & BudApp_Selected & "'," & vbCrLf &
                " ISA_IOL_APR_LIMIT = " & Convert.ToDecimal(BudLimit) & "," & vbCrLf &
                " LASTUPDOPRID = '" & Session_UserID & "'," & vbCrLf &
                " LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & UserId & "'" & vbCrLf &
                " AND BUSINESS_UNIT = '" & strBU & "'"

        Else
            strSQLstring = "INSERT INTO SDIX_USERS_APPRV" & vbCrLf &
                " (ISA_EMPLOYEE_ID," & vbCrLf &
                " BUSINESS_UNIT," & vbCrLf &
                " ISA_IOL_APR_EMP_ID," & vbCrLf &
                " ISA_IOL_APR_LIMIT," & vbCrLf &
                " ISA_IOL_APR_ALT," & vbCrLf &
                " LASTUPDOPRID," & vbCrLf &
                " LASTUPDDTTM)" & vbCrLf &
                " VALUES('" & UserId & "'," & vbCrLf &
                " '" & strBU & "'," & vbCrLf &
                " '" & BudApp_Selected & "'," & vbCrLf &
                " " & Convert.ToDecimal(BudLimit) & "," & vbCrLf &
                " '" & BudApp_Selected & "'," & vbCrLf &
                " '" & Session_UserID & "'," & vbCrLf &
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
                    decNewTotal = CType(BudLimit.ToString, Decimal)
                Catch ex As Exception
                    decNewTotal = 0
                End Try

                If decOrigTotal <> decNewTotal Then
                    clsSDIAudit.AddRecord("profile.aspx", "Update Order Limit Record", "SDIX_USERS_APPRV", Session_UserID.ToString, strBU, UserId, Session_WEBSITED, sColumnChg:="isa_iol_apr_limit", sOldValue:=m_sAppTotalOrig, sNewValue:=BudLimit.ToString)
                    m_sAppTotalOrig = BudLimit
                    'Session("APPR_TOTAL") = m_sAppTotalOrig 20/07/2020
                End If
                If m_sAppEmpIDOrig <> BudApp_Selected Then
                    clsSDIAudit.AddRecord("profile.aspx", "Update Order Limit Record", "SDIX_USERS_APPRV", Session_UserID.ToString, strBU,
                                          UserId, Session_WEBSITED, sColumnChg:="isa_iol_apr_emp_id", sOldValue:=m_sAppEmpIDOrig, sNewValue:=BudApp_Selected)
                    m_sAppEmpIDOrig = BudApp_Selected
                    'Session("APPR_APR_EMPID") = m_sAppEmpIDOrig 20/07/2020
                End If
                If m_sAppAltOrig <> BudApp_Selected Then
                    clsSDIAudit.AddRecord("profile.aspx", "Update Order Limit Record", "SDIX_USERS_APPRV", Session_UserID.ToString, strBU,
                                          UserId, Session_WEBSITED, sColumnChg:="isa_iol_apr_alt", sOldValue:=m_sAppAltOrig, sNewValue:=BudApp_Selected)
                    m_sAppAltOrig = BudApp_Selected
                    'Session("APPR_APR_ALT") = m_sAppAltOrig 20/07/2020
                End If
                SuceessMessage_row("UpdateSuccess") = "Budgetory approver details updated successfully."
                SuceessMessage.Rows.Add(SuceessMessage_row)
                'lblMessage.Text = "Budgetory Approver details Updated Successfully."  20/07/2020
                'lblMessage.Visible = True
                'lblMsg.Text = "Budgetory Approver details Updated Successfully."

            Else
                clsSDIAudit.AddRecord("profile.aspx", "Insert Order Limit Record", "SDIX_USERS_APPRV", Session_UserID.ToString, strBU,
                                      UserId, Session_WEBSITED, sUDF1:="Limit " & BudLimit.ToString, sUDF2:="Approver " & BudApp_Selected)
                m_sAppTotalOrig = BudLimit
                m_sAppEmpIDOrig = BudApp_Selected
                m_sAppAltOrig = BudApp_Selected
                'Session("APPR_TOTAL") = m_sAppTotalOrig   20/07/2020
                'Session("APPR_APR_EMPID") = m_sAppEmpIDOrig
                'Session("APPR_APR_ALT") = m_sAppAltOrig
                SuceessMessage_row("InsertFailed") = "Budgetory approver details inserted successfully."
                SuceessMessage.Rows.Add(SuceessMessage_row)
                'lblMessage.Text = "Budgetory Approver details Inserted Sucessfuly." 20/07/2020
                'lblMessage.Visible = True
                'lblMsg.Text = "Budgetory Approver details Inserted Sucessfuly."
            End If
        End If
    End Sub
    Private Function ExistsApprvRecord(ByVal strBU As String, ByVal UserId As String) As Boolean
        Dim bExistsRecord As Boolean = False
        Dim strSQLstring As String

        Try
            strSQLstring = "SELECT * FROM SDIX_USERS_APPRV WHERE ISA_EMPLOYEE_ID = '" & UserId &
                "' AND business_unit = '" & strBU & "'"
            Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)
            If ds.Tables(0).Rows.Count > 0 Then
                m_sAppTotalOrig = ds.Tables(0).Rows(0)("ISA_IOL_APR_LIMIT")
                m_sAppEmpIDOrig = ds.Tables(0).Rows(0)("ISA_IOL_APR_EMP_ID")
                m_sAppAltOrig = ds.Tables(0).Rows(0)("ISA_IOL_APR_ALT")
                bExistsRecord = True
            End If
        Catch ex As Exception

        End Try

        Return bExistsRecord
    End Function
    '*UP_PC_191
    '*
    Public Function btnSubmitReqAppr_Click(ByVal data As String) As String
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.Columns.Add("DeleteSuccess")
            SuceessMessage.Columns.Add("DeleteFailed")
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("InsertFailed")
            If Not String.IsNullOrEmpty(data) Then
                Dim objUpdReqAppBo As UpdReqAppBo = JsonConvert.DeserializeObject(Of UpdReqAppBo)(data)
                Dim strBU As String = ""
                If objUpdReqAppBo.Session_UserRole = "SUPER" Then
                    strBU = GetBUbyGroup(objUpdReqAppBo.SiteValue)
                Else
                    strBU = GetBUbyGroup(objUpdReqAppBo.SiteValue)
                End If
                ExistsReqApprvRecord(strBU, objUpdReqAppBo.UserId)
                '20/07/2020 Handle in the front end
                'If ddReqAppr.SelectedIndex = 0 Then
                '    lblMsgReqAppr.Text = "A valid approval name must be selected"
                '    Exit Sub
                'Else
                '    lblMsgReqAppr.Text = ""
                'End If
                If objUpdReqAppBo.Delete_Checked Then
                    deleteReqAPPRVRecord(objUpdReqAppBo.SiteValue, objUpdReqAppBo.UserId, objUpdReqAppBo.Session_UserId, objUpdReqAppBo.Session_WEBSITED, objUpdReqAppBo.Session_UserRole, SuceessMessage)

                    'chkDeleteReqAppr.Checked = False  20/07/2020
                    'ddReqAppr.SelectedIndex = 0
                Else
                    updateReqAPPRVTable(objUpdReqAppBo.ReqApp_Selected, objUpdReqAppBo.AppExist, objUpdReqAppBo.Session_UserId, objUpdReqAppBo.SiteValue, objUpdReqAppBo.Session_UserRole, objUpdReqAppBo.UserId, objUpdReqAppBo.Session_WEBSITED, SuceessMessage)
                End If
            End If
            _result.Tables.Add(SuceessMessage)
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    Public Function deleteReqAPPRVRecord(ByVal SiteSelected As String, ByVal UserId As String, ByVal Session_UserID As String,
                                      ByVal Session_WEBSITED As String, ByVal Session_UserRole As String, ByRef SuceessMessage As DataTable)
        Dim rowsaffected As Integer = 0
        Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
        Dim strBU As String = ""
        If Session_UserRole = "SUPER" Then
            strBU = GetBUbyGroup(SiteSelected)
        Else
            strBU = GetBUbyGroup(SiteSelected)
        End If
        Dim strSQLstring As String = ""
        strSQLstring = "DELETE FROM SDIX_USERS_REQ_APPRV" & vbCrLf &
                    " WHERE ISA_EMPLOYEE_ID = '" & UserId & "'" & vbCrLf &
                    " AND BUSINESS_UNIT = '" & strBU & "'"

        rowsaffected = ORDBData.ExecNonQueryWithTransaction(strSQLstring)
        If rowsaffected > 0 Then
            clsSDIAudit.AddRecord("profile.aspx", "Delete Requestor Approver Record", "SDIX_USERS_REQ_APPRV", Session_UserID.ToString, strBU, UserId, Session_WEBSITED,
                      sUDF2:="Approver " & m_sReq_AltAppr_EmpID, sUDF3:="ReqAltApprover " & m_sReq_AltAppr_Orig)
            m_sReq_AltAppr_Orig = ""
            m_sReq_AltAppr_EmpID = ""
            'Session("REQ_APPR_EMPID") = m_sReq_AltAppr_EmpID   20/07/2020
            'Session("REQ_APPR_ALT") = m_sReq_AltAppr_Orig
            SuceessMessage_row("DeleteSuccess") = "Requestor approver deleted successfully."
            SuceessMessage.Rows.Add(SuceessMessage_row)
            'lblMsgReqAppr.Text = "Requestor Approver deleted Sucessfuly."  20/07/2020
            'lblMessage.Text = "Requestor Approver deleted Sucessfuly."
            'lblMessage.Visible = True
        Else
            SuceessMessage_row("DeleteFailed") = "Requestor approver details deleted failed."
            SuceessMessage.Rows.Add(SuceessMessage_row)
        End If
    End Function
    Private Function updateReqAPPRVTable(ByVal ReqApp_Selected As String, ByVal AppExist As String, ByVal Session_UserID As String,
                                 ByVal SiteSelected As String, ByVal Session_Userole As String, ByVal UserId As String, ByVal Session_WEBSITED As String, ByRef SuceessMessage As DataTable)
        Dim strBU As String
        Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
        If Session_Userole = "SUPER" Then
            strBU = GetBUbyGroup(SiteSelected)
        Else
            strBU = GetBUbyGroup(SiteSelected)
        End If

        Dim strSQLstring As String
        Dim bUpdate As Boolean = (AppExist = "UPD")

        If Not bUpdate Then
            If ExistsReqApprvRecord(strBU, UserId) Then
                ' Verify that the record doesn't already exist if we're to insert the record.
                ' If the record already exists, change the "insert" action to an "update" action.
                bUpdate = True
            End If
        End If

        If bUpdate Then
            strSQLstring = "UPDATE SDIX_USERS_REQ_APPRV" & vbCrLf &
                " SET ISA_REQ_APR_ALT = '" & ReqApp_Selected & "'," & vbCrLf &
                " LASTUPDOPRID = '" & Session_UserID & "'," & vbCrLf &
                " LASTUPDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & UserId & "'" & vbCrLf &
                " AND BUSINESS_UNIT = '" & strBU & "'"

        Else
            strSQLstring = "INSERT INTO SDIX_USERS_REQ_APPRV" & vbCrLf &
                " (ISA_EMPLOYEE_ID," & vbCrLf &
                " BUSINESS_UNIT," & vbCrLf &
                " ISA_REQ_APR_ALT," & vbCrLf &
                " LASTUPDOPRID," & vbCrLf &
                " LASTUPDTTM)" & vbCrLf &
                " VALUES('" & UserId & "'," & vbCrLf &
                " '" & strBU & "'," & vbCrLf &
                " '" & ReqApp_Selected & "'," & vbCrLf &
                " '" & Session_UserID & "'," & vbCrLf &
                " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
        End If

        Dim rowsaffected As Integer
        rowsaffected = ORDBData.ExecNonQuery(strSQLstring)

        If rowsaffected > 0 Then
            If bUpdate Then
                If m_sReq_AltAppr_Orig <> ReqApp_Selected Then
                    clsSDIAudit.AddRecord("profile.aspx", "Update Requestor Approver Record", "SDIX_USERS_REQ_APPRV", Session_UserID.ToString, strBU,
                                          UserId, Session_WEBSITED, sColumnChg:="ISA_REQ_APR_ALT", sOldValue:=m_sReq_AltAppr_Orig, sNewValue:=ReqApp_Selected)
                    m_sReq_AltAppr_Orig = ReqApp_Selected
                End If

                'lblMessage.Text = "Requestor Approver Updated Sucessfuly."
                'lblMessage.Visible = True
                'lblMsgReqAppr.Text = "Requestor Approver Updated Sucessfuly."
                SuceessMessage_row("UpdateSuccess") = "Requestor approver updated successfully."
                SuceessMessage.Rows.Add(SuceessMessage_row)
            Else
                clsSDIAudit.AddRecord("profile.aspx", "Insert Requestor Approver Record", "SDIX_USERS_REQ_APPRV", Session_UserID.ToString, strBU,
                                      UserId, Session_WEBSITED, sUDF2:="ReqAltApprover " & ReqApp_Selected)
                m_sReq_AltAppr_Orig = ReqApp_Selected
                'Session("REQ_APPR_EMPID") = m_sReq_AltAppr_EmpID   20/07/2020
                'Session("REQ_APPR_ALT") = m_sReq_AltAppr_Orig

                'lblMessage.Text = "Requestor Approver Inserted Sucessfuly."
                'lblMessage.Visible = True
                SuceessMessage_row("InsertFailed") = "Requestor approver details inserted successfully."
                SuceessMessage.Rows.Add(SuceessMessage_row)
                'lblMsgReqAppr.Text = "Requestor Approver Inserted Sucessfuly."
            End If
        End If
    End Function
    Private Function ExistsReqApprvRecord(ByVal strBU As String, ByVal UserId As String) As Boolean
        Dim bExistsRecord As Boolean = False
        Dim strSQLstring As String

        Try
            strSQLstring = "SELECT * FROM SDIX_USERS_REQ_APPRV WHERE ISA_EMPLOYEE_ID = '" & UserId &
                "' AND business_unit = '" & strBU & "'"
            Dim ds As DataSet = ORDBData.GetAdapter(strSQLstring)
            If ds.Tables(0).Rows.Count > 0 Then
                bExistsRecord = True

            End If
        Catch ex As Exception

        End Try

        Return bExistsRecord
    End Function
    '*UP_PC_193
    '*
    Public Function btnOrdStatEmailSubmit_Click(ByVal data As String) As String
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.Columns.Add("UpdateSuccess")
            If Not String.IsNullOrEmpty(data) Then
                Dim objUpdOSEBO As UpdOSEBO = JsonConvert.DeserializeObject(Of UpdOSEBO)(data)
                Dim OSEListTable As New DataTable
                OSEListTable.Columns.Add("chbCRE")
                OSEListTable.Columns.Add("chbQTW")
                OSEListTable.Columns.Add("chbQTC")
                OSEListTable.Columns.Add("chbQTS")
                OSEListTable.Columns.Add("chbCST")
                OSEListTable.Columns.Add("chbVND")
                OSEListTable.Columns.Add("chbAPR")
                OSEListTable.Columns.Add("chbQTA")
                OSEListTable.Columns.Add("chbQTR")
                OSEListTable.Columns.Add("chbRFA")
                OSEListTable.Columns.Add("chbRFR")
                OSEListTable.Columns.Add("chbRFC")
                OSEListTable.Columns.Add("chbRCF")
                OSEListTable.Columns.Add("chbRCP")
                OSEListTable.Columns.Add("chbCNC")
                OSEListTable.Columns.Add("chbDLF")
                Dim OSEListTable_row As DataRow = OSEListTable.NewRow()
                OSEListTable_row("chbCRE") = objUpdOSEBO.chbCRE
                OSEListTable_row("chbQTW") = objUpdOSEBO.chbQTW
                OSEListTable_row("chbQTC") = objUpdOSEBO.chbQTC
                OSEListTable_row("chbQTS") = objUpdOSEBO.chbQTS
                OSEListTable_row("chbCST") = objUpdOSEBO.chbCST
                OSEListTable_row("chbVND") = objUpdOSEBO.chbVND
                OSEListTable_row("chbAPR") = objUpdOSEBO.chbAPR
                OSEListTable_row("chbQTA") = objUpdOSEBO.chbQTA
                OSEListTable_row("chbQTR") = objUpdOSEBO.chbQTR
                OSEListTable_row("chbRFA") = objUpdOSEBO.chbRFA
                OSEListTable_row("chbRFR") = objUpdOSEBO.chbRFR
                OSEListTable_row("chbRFC") = objUpdOSEBO.chbRFC
                OSEListTable_row("chbRCF") = objUpdOSEBO.chbRCF
                OSEListTable_row("chbRCP") = objUpdOSEBO.chbRCP
                OSEListTable_row("chbCNC") = objUpdOSEBO.chbCNC
                OSEListTable_row("chbDLF") = objUpdOSEBO.chbDLF
                OSEListTable.Rows.Add(OSEListTable_row)
                updatePrivsTable(objUpdOSEBO.SiteValue, objUpdOSEBO.Session_UserRole, objUpdOSEBO.UserId, objUpdOSEBO.Session_EMAIL, objUpdOSEBO.Session_BUSUNIT, objUpdOSEBO.Session_UserId, OSEListTable, SuceessMessage)
            End If
            _result.Tables.Add(SuceessMessage)
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    Sub updatePrivsTable(ByVal SiteSelected As String, ByVal Session_UserROle As String, ByVal strUserId As String, ByVal Session_EMAIL As String,
                         ByVal Session_BUSUNIT As String, ByVal Session_UserID As String, ByVal OSE As DataTable, ByRef SuceessMessage As DataTable)
        Try
            Dim strOpValue As String
            Dim strPrivSQL As String

            Dim strBU As String
            If Session_UserROle = "SUPER" Then
                strBU = GetBUbyGroup(SiteSelected)
            Else
                strBU = GetBUbyGroup(SiteSelected)
            End If

            Dim hashPrivs As Hashtable
            hashPrivs = New Hashtable

            Dim SessionTable As New DataTable
            SessionTable.Columns.Add("Session_EMAIL")
            SessionTable.Columns.Add("Session_BUSUNIT")
            SessionTable.Columns.Add("Session_USERTYPE")
            Dim _Rows As DataRow = SessionTable.NewRow()
            _Rows("Session_EMAIL") = Session_EMAIL
            _Rows("Session_BUSUNIT") = Session_BUSUNIT
            _Rows("Session_USERTYPE") = Session_UserROle

            Dim roleHashPrivs As Hashtable = Nothing
            hashPrivs = getprivhashtable(strUserId, strBU, "Y", _Rows, roleHashPrivs)

            If OSE.Rows(0)("chbCRE") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILCRE01") Then
                strPrivSQL = getSQLprivupdate("EMAILCRE01", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbCRE") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILCRE01", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILCRE01")
                End If
            End If

            If OSE.Rows(0)("chbQTW") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILQTW02") Then
                strPrivSQL = getSQLprivupdate("EMAILQTW02", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbQTW") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILQTW02", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILQTW02")
                End If
            End If

            If OSE.Rows(0)("chbQTC") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILQTC03") Then
                strPrivSQL = getSQLprivupdate("EMAILQTC03", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbQTC") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILQTC03", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILQTC03")
                End If
            End If

            If OSE.Rows(0)("chbQTS") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILQTS04") Then
                strPrivSQL = getSQLprivupdate("EMAILQTS04", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbQTS") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILQTS04", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILQTS04")
                End If
            End If

            If OSE.Rows(0)("chbCST") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILCST05") Then
                strPrivSQL = getSQLprivupdate("EMAILCST05", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbCST") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILCST05", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILCST05")
                End If
            End If

            If OSE.Rows(0)("chbVND") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILVND06") Then
                strPrivSQL = getSQLprivupdate("EMAILVND06", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbVND") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILVND06", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILVND06")
                End If
            End If

            If OSE.Rows(0)("chbAPR") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILAPR07") Then
                strPrivSQL = getSQLprivupdate("EMAILAPR07", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbAPR") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILAPR07", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILAPR07")
                End If
            End If

            If OSE.Rows(0)("chbQTA") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILQTA08") Then
                strPrivSQL = getSQLprivupdate("EMAILQTA08", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbQTA") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILQTA08", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILQTA08")
                End If
            End If

            If OSE.Rows(0)("chbQTR") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILQTR09") Then
                strPrivSQL = getSQLprivupdate("EMAILQTR09", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbQTR") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILQTR09", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILQTR09")
                End If
            End If

            If OSE.Rows(0)("chbRFA") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILRFA10") Then
                strPrivSQL = getSQLprivupdate("EMAILRFA10", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbRFA") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILRFA10", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILRFA10")
                End If
            End If

            If OSE.Rows(0)("chbRFR") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILRFR11") Then
                strPrivSQL = getSQLprivupdate("EMAILRFR11", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbRFR") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILRFR11", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILRFR11")
                End If
            End If

            If OSE.Rows(0)("chbRFC") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILRFC12") Then
                strPrivSQL = getSQLprivupdate("EMAILRFC12", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbRFC") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILRFC12", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILRFC12")
                End If
            End If

            If OSE.Rows(0)("chbRCF") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILRCF13") Then
                strPrivSQL = getSQLprivupdate("EMAILRCF13", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbRCF") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILRCF13", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILRCF13")
                End If
            End If

            If OSE.Rows(0)("chbRCP") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILRCP14") Then
                strPrivSQL = getSQLprivupdate("EMAILRCP14", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbRCP") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILRCP14", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILRCP14")
                End If
            End If

            If OSE.Rows(0)("chbCNC") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILCNC15") Then
                strPrivSQL = getSQLprivupdate("EMAILCNC15", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbCNC") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILCNC15", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILCNC15")
                End If
            End If

            If OSE.Rows(0)("chbDLF") = True Then
                strOpValue = "Y"
            Else
                strOpValue = "N"
            End If
            If hashPrivs.ContainsKey("EMAILDLF16") Then
                strPrivSQL = getSQLprivupdate("EMAILDLF16", "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle)
            Else
                If OSE.Rows(0)("chbDLF") = True Then
                    strPrivSQL = getSQLprivupdate("EMAILDLF16", "IOL", strOpValue, "INS", SiteSelected, Session_UserID, strUserId, Session_UserROle)
                    execPrivsSQL(strPrivSQL, SiteSelected, Session_UserID, strUserId, Session_UserROle, "INS", strOpValue, "EMAILDLF16")
                End If
            End If
            Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
            SuceessMessage_row("UpdateSuccess") = "User order status email has been modified and saved successfully."
            SuceessMessage.Rows.Add(SuceessMessage_row)
        Catch
        End Try
        'lblUpdMsg.Text = "Database has been updated..."

    End Sub
    Function getSQLprivupdate(ByVal strOpName, ByVal strOpType, ByVal strOpValue, ByVal strUpdType, ByVal SiteSelected, ByVal Session_UserID, ByVal UserID, ByVal Session_UserRole) As String

        Dim strBU As String
        If Session_UserRole = "SUPER" Then
            strBU = GetBUbyGroup(SiteSelected)
        Else
            strBU = GetBUbyGroup(SiteSelected)
        End If

        If strUpdType = "UPD" Then
            getSQLprivupdate = "UPDATE SDIX_USERS_PRIVS" & vbCrLf &
                " SET ISA_IOL_OP_VALUE = '" & strOpValue & "'," & vbCrLf &
                " LASTUPDOPRID = '" & Session_UserID & "'," & vbCrLf &
                " LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & UserID.ToUpper & "'" & vbCrLf &
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
                " VALUES('" & UserID.ToUpper & "'," & vbCrLf &
                " '" & strBU & "'," & vbCrLf &
                " '" & strOpName & "'," & vbCrLf &
                " '" & strOpValue & "'," & vbCrLf &
                " '" & strOpType & "'," & vbCrLf &
                " '" & Session_UserID & "'," & vbCrLf &
                " TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM'))"
        End If
    End Function
    Private Sub execPrivsSQL(ByVal strSQLstring As String, ByVal SiteSelected As String, ByVal Session_UserID As String, ByVal UserID As String, ByVal Session_UserRole As String, Optional ByVal strUpdType As String = "",
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
                            strSQLstring = getSQLprivupdate(strOpName, "IOL", strOpValue, "UPD", SiteSelected, Session_UserID, UserID, Session_UserRole)
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
    '*UP_PC_194
    '*
    Public Function btnSubmitPrefs_Click(ByVal data As String) As String
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            If Not String.IsNullOrEmpty(data) Then
                Dim objUpdPrefBO As UpdPrefBO = JsonConvert.DeserializeObject(Of UpdPrefBO)(data)
                Dim UpdateSession As New DataTable
                UpdateSession.TableName = "UpdateSession"
                UpdateSession = UpdatePrefs(objUpdPrefBO.UserId, objUpdPrefBO.DescripType, objUpdPrefBO.Dept, objUpdPrefBO, objUpdPrefBO.ShipTo, objUpdPrefBO.BlockPrice, SuceessMessage)
                _result.Tables.Add(UpdateSession)
            End If
            _result.Tables.Add(SuceessMessage)
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function

    Private Function UpdatePrefs(ByVal UserId As String, ByVal DescripType As String, ByVal Dept As String, ByVal objUpdPrefBO As UpdPrefBO,
                                 ByVal ShipTo As String, ByVal BlockPrice As Boolean, ByRef SuceessMessage As DataTable)
        Try
            'lblMessage.Text = ""  20/07/2020
            Dim strSQLstring As String = ""
            Dim strProdDisplay As String = m_cProdDispType_CatalogSQL
            Dim strBlockPrice As String = ""
            Dim strShipTo As String = ShipTo
            Dim strDept As String = Dept
            Dim rowsaffected As Integer = 0
            Dim UpdateSession As New DataTable
            UpdateSession.Columns.Add("ISAShipToID")
            If BlockPrice = True Then
                strBlockPrice = "Y"
            End If

            If DescripType = "Client" Then
                strProdDisplay = m_cProdDispType_PSClient
            End If
            Dim Child1Value As String = objUpdPrefBO.Child1Value
            Dim Child2Value As String = objUpdPrefBO.Child2Value
            If IsDBNull(objUpdPrefBO.Child1Value) Then
                Child1Value = " "
            End If
            If IsDBNull(objUpdPrefBO.Child2Value) Then
                Child2Value = " "
            End If

            strSQLstring = "UPDATE SDIX_USERS_TBL" & vbCrLf &
                   " SET ISA_PROD_DISPLAY = '" & strProdDisplay & "'" & vbCrLf &
                   ", ISA_PRICE_BLOCK = '" & strBlockPrice & "'" & vbCrLf &
                   " , SHIPTO_DEFAULT='" & strShipTo & "'" & vbCrLf &
                   " , ISA_DEPT ='" & strDept & "'" & vbCrLf &
                   " WHERE ISA_EMPLOYEE_ID = '" & UserId.ToUpper & "'"

            strSQLstring = "UPDATE SDIX_USERS_TBL" & vbCrLf &
                   " SET ISA_PROD_DISPLAY = '" & strProdDisplay & "'" & vbCrLf &
                   ", ISA_PRICE_BLOCK = '" & strBlockPrice & "'" & vbCrLf &
                   " , SHIPTO_DEFAULT='" & strShipTo & "'" & vbCrLf &
                   " , ISA_DEPT ='" & strDept & "'" & vbCrLf &
                   ",ISA_CHRCD_CHILD1='" & Child1Value & "'" & vbCrLf &
                   ",ISA_CHRCD_CHILD2='" & Child2Value & "'" & vbCrLf &
                   " WHERE ISA_EMPLOYEE_ID = '" & UserId.ToUpper & "'"

            rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            If rowsaffected > 0 Then
                Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
                SuceessMessage_row("UpdateSuccess") = "User preference has been modified and saved successfully."
                SuceessMessage.Rows.Add(SuceessMessage_row)

                Dim UpdateSession_row As DataRow = UpdateSession.NewRow()
                UpdateSession_row("ISAShipToID") = strShipTo
                UpdateSession.Rows.Add(UpdateSession_row)
                'lblMessage.Visible = True
                'lblMessage.Text = "User preference has been modified and saved successfully."
                'lblPrefMessage.Visible = True   20/07/2020
                'lblPrefMessage.Text = "User preference has been modified and saved successfully."
            End If
            UpdateSession.TableName = "UpdateSession"
            Return UpdateSession
            'If Not dropShipto.SelectedValue = "-- Select ShipTo --" And dropShipto.SelectedIndex > 0 Then
            '    strShipTo = dropShipto.SelectedValue
            'End If

            'If Not drpDept.SelectedValue = "-- Select Dept --" And drpDept.SelectedIndex > 0 Then
            '    strDept = drpDept.SelectedValue
            'End If
            'Dim CHILD1 As String
            'Dim CHILD2 As String
            'If Not dropCD4Seg.SelectedValue = "" Then
            '    CHILD1 = dropCD4Seg.SelectedValue
            '    CHILD2 = dropCD5Seg.SelectedValue
            'Else
            '    CHILD1 = dropCD7Seg.SelectedValue
            '    CHILD2 = dropCD8Seg.SelectedValue
            'End If
            'If (dropCD4Seg.Visible = True And dropCD5Seg.Visible = True) Or (dropCD7Seg.Visible = True And dropCD8Seg.Visible = True) Then

            '    If Not CHILD1 = "" And Not CHILD2 = "" Then
            '        strSQLstring = "UPDATE SDIX_USERS_TBL" & vbCrLf &
            '            " SET ISA_PROD_DISPLAY = '" & strProdDisplay & "'" & vbCrLf &
            '            ", ISA_PRICE_BLOCK = '" & strBlockPrice & "'" & vbCrLf &
            '            " , SHIPTO_DEFAULT='" & strShipTo & "'" & vbCrLf &
            '            " , ISA_DEPT ='" & strDept & "'" & vbCrLf &
            '            ",ISA_CHRCD_CHILD1='" & CHILD1 & "'" & vbCrLf &
            '            ",ISA_CHRCD_CHILD2='" & CHILD2 & "'" & vbCrLf &
            '            " WHERE ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text).ToUpper & "'"

            '        rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            '        If rowsaffected > 0 Then
            '            Session("ISAShipToID") = strShipTo
            '            'lblMessage.Visible = True
            '            'lblMessage.Text = "User preference has been modified and saved successfully."
            '            lblPrefMessage.Visible = True
            '            lblPrefMessage.Text = "User preference has been modified and saved successfully."
            '        End If
            '    Else
            '        lblPrefMessage.Visible = True
            '        lblPrefMessage.Text = "Please select the ChargeCode"
            '    End If
            'ElseIf (dropCD4Seg.Visible = True Or dropCD5Seg.Visible = True) Or (dropCD7Seg.Visible = True Or dropCD8Seg.Visible = True) Then
            '    If Not CHILD1 = "" Then
            '        strSQLstring = "UPDATE SDIX_USERS_TBL" & vbCrLf &
            '            " SET ISA_PROD_DISPLAY = '" & strProdDisplay & "'" & vbCrLf &
            '            ", ISA_PRICE_BLOCK = '" & strBlockPrice & "'" & vbCrLf &
            '            " , SHIPTO_DEFAULT='" & strShipTo & "'" & vbCrLf &
            '            " , ISA_DEPT ='" & strDept & "'" & vbCrLf &
            '            ",ISA_CHRCD_CHILD1='" & CHILD1 & "'" & vbCrLf &
            '            ",ISA_CHRCD_CHILD2='" & CHILD2 & "'" & vbCrLf &
            '            " WHERE ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text).ToUpper & "'"

            '        rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            '        If rowsaffected > 0 Then
            '            Session("ISAShipToID") = strShipTo
            '            'lblMessage.Visible = True
            '            'lblMessage.Text = "User preference has been modified and saved successfully."
            '            lblPrefMessage.Visible = True
            '            lblPrefMessage.Text = "User preference has been modified and saved successfully."
            '        End If
            '    Else
            '        lblPrefMessage.Visible = True
            '        lblPrefMessage.Text = "Please select ChargeCode"
            '    End If
            'Else
            '    strSQLstring = "UPDATE SDIX_USERS_TBL" & vbCrLf &
            '       " SET ISA_PROD_DISPLAY = '" & strProdDisplay & "'" & vbCrLf &
            '       ", ISA_PRICE_BLOCK = '" & strBlockPrice & "'" & vbCrLf &
            '       " , SHIPTO_DEFAULT='" & strShipTo & "'" & vbCrLf &
            '       " , ISA_DEPT ='" & strDept & "'" & vbCrLf &
            '       ",ISA_CHRCD_CHILD1='" & CHILD1 & "'" & vbCrLf &
            '       ",ISA_CHRCD_CHILD2='" & CHILD2 & "'" & vbCrLf &
            '       " WHERE ISA_EMPLOYEE_ID = '" & Trim(txtUserid.Text).ToUpper & "'"

            '    rowsaffected = ORDBData.ExecNonQuery(strSQLstring)
            '    If rowsaffected > 0 Then
            '        Session("ISAShipToID") = strShipTo
            '        'lblMessage.Visible = True
            '        'lblMessage.Text = "User preference has been modified and saved successfully."
            '        lblPrefMessage.Visible = True
            '        lblPrefMessage.Text = "User preference has been modified and saved successfully."
            '    End If
            'End If
        Catch ex As Exception

        End Try
    End Function
    '*UP_PC_197
    '*
    Public Function btnGribSubmit_Click(ByVal data As String) As String
        'Dim strMessage As New Alert 21/07/2020 handle in the fronr

        'If ddlGrib.SelectedIndex = 0 Then
        '    ltlAlert.Text = strMessage.Say("Please select Crib")
        '    Exit Sub
        'End If
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            If Not String.IsNullOrEmpty(data) Then
                Dim objUpdCribBO As UpdCribBO = JsonConvert.DeserializeObject(Of UpdCribBO)(data)
                Dim StrGRIB As String = objUpdCribBO.Crib_Value
                Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()

                Dim StrQury As String = "UPDATE SDIX_USERS_TBL SET LASTUPDOPRID='" & CStr(objUpdCribBO.Session_UserId).Trim.ToUpper & "' , LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf &
                    ", ISA_CRIB_IDENT='" & StrGRIB & "' WHERE UPPER(ISA_EMPLOYEE_ID)='" & Trim(objUpdCribBO.UserId).ToUpper & "' "

                Dim rowAffected As Integer = ORDBData.ExecNonQuery(StrQury, False)

                If rowAffected > 0 Then
                    SuceessMessage_row("UpdateSuccess") = "User information has been modified and saved successfully."
                    'lblGRIBErr.Text = "Successfully Updated"   21/07/2020
                Else
                    SuceessMessage_row("UpdateFailed") = "Update failed. Error message was sent to IT group. Please contact help desk."
                    'lblGRIBErr.Text = "Update Failed. Error message was sent to IT Group. Please contact Help Desk." 21/07/2020
                End If
                SuceessMessage.Rows.Add(SuceessMessage_row)
                _result.Tables.Add(SuceessMessage)
            End If

            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '*UP_PC_198
    '* This method is triggered when the Submit button is clicked in the Zues tab
    Public Function btnZuseSubmit_Click(ByVal data As String) As String

        Dim rowAffected As Integer
        Dim UserName() As String
        'lblZuseError.Text = ""

        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            If Not String.IsNullOrEmpty(data) Then
                Dim objUpdZeusBo As UpdZeusBo = JsonConvert.DeserializeObject(Of UpdZeusBo)(data)
                'If cbxZeus.Checked Then  ****************handle in the front end  21/07/2020
                'cbxZeus.Checked = False
                UserName = objUpdZeusBo.Session_UserName.ToString().Split(",")
                Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
                If objUpdZeusBo.ZeusLabelText = "Disable Zeus" Then
                    Dim StrQury As String = "DELETE FROM SDIX_QLIK_USERS WHERE USERID= '" & Trim(objUpdZeusBo.UserId).ToUpper & "'"
                    rowAffected = ORDBData.ExecNonQuery(StrQury, False)
                    'lblZeus.Text = "Enable Zeus" ****************handle in the front end  21/07/2020
                Else
                    Dim StrQury As String = "INSERT INTO SDIX_QLIK_USERS VALUES ('" & Trim(objUpdZeusBo.UserId).ToUpper & "','" & UserName(1).ToString & " " & UserName(0).ToString & "')"
                    rowAffected = ORDBData.ExecNonQuery(StrQury, False)
                    'lblZeus.Text = "Disable Zeus" ****************handle in the front end  21/07/2020
                End If

                If rowAffected > 0 Then
                    SuceessMessage_row("UpdateSuccess") = "User information has been modified and saved successfully."
                    'lblZuseError.Text = "Successfully Updated"       handle in the front end
                    'lblZuseError.ForeColor = Green
                Else
                    SuceessMessage_row("UpdateFailed") = "Update failed. Error message was sent to IT group. Please contact help desk."
                    'lblZuseError.Text = "Update Failed"  handle in the front end
                    'lblZuseError.ForeColor = Red
                End If
                SuceessMessage.Rows.Add(SuceessMessage_row)
                _result.Tables.Add(SuceessMessage)
                'Else ****************handle in the front end   21/07/2020
                '    lblZuseError.Text = "Please Select the Option"
                '    lblZuseError.ForeColor = Red
                'End If
            End If
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '*UP_PC_196
    '** This method triggered when the Add other user button is clicked in the SDI TRack tab
    Public Function btnTangoAddOtherUser_Click(ByVal data As String) As String
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            Dim AddedUserDetails As New DataTable
            AddedUserDetails.TableName = "AddedUserDetails"
            If Not String.IsNullOrEmpty(data) Then
                Dim objAddUserSDITrackBO As AddUserSDITrackBO = JsonConvert.DeserializeObject(Of AddUserSDITrackBO)(data)

                Dim oUserTbl As New clsUserTbl(objAddUserSDITrackBO.SelectedUser, "")
                Dim sBU As String = oUserTbl.BusinessUnit

                AddedUserDetails = AddTangoUser(objAddUserSDITrackBO.SelectedUser, sBU, objAddUserSDITrackBO.SDITrack_UserId, objAddUserSDITrackBO.SDITrack_PassWord,
                             objAddUserSDITrackBO.Session_TrackDBType, objAddUserSDITrackBO.Session_TrackDBGUID, objAddUserSDITrackBO.Session_TrackDBUser, objAddUserSDITrackBO.Session_TrackDBPassword, SuceessMessage)
            End If
            _result.Tables.Add(SuceessMessage)
            _result.Tables.Add(AddedUserDetails)
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    Private Function AddTangoUser(ByVal sUserID As String, ByVal sBU As String, ByVal txtUserName As String, ByVal txtPassword As String,
                             ByVal Session_TrackDBType As String, ByVal Session_TrackDBGUID As String, ByVal Session_TrackDBUser As String, ByVal Session_TrackDBPassword As String, ByRef SuceessMessage As DataTable)
        Dim oSDITrack As New clsSDITrack()
        Dim bUserExists As Boolean = False
        Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
        Try
            Dim sNewUserGUID As String = ""
            Dim dtTodaysDate As Date
            Dim AddedUserDetails As New DataTable
            AddedUserDetails.Columns.Add("UserName")
            AddedUserDetails.Columns.Add("GUIDVal")
            AddedUserDetails.Columns.Add("DateTime")
            Dim AddedUserDetails_Rows As DataRow = AddedUserDetails.NewRow()
            If oSDITrack.AddUser(sUserID, sBU, txtUserName, txtPassword, Session_TrackDBType, Session_TrackDBGUID, Session_TrackDBUser, Session_TrackDBPassword, bUserExists, sNewUserGUID, dtTodaysDate) Then
                SuceessMessage_row("UpdateSuccess") = "User Added to SDiTrack"
                AddedUserDetails_Rows("UserName") = txtUserName
                '21/07/2020 Handle in the front end
                'lblValidate.Text = "User Added to SDiTrack"
                'lblUserNameStored.Text = txtUserName.Text
                'lblUserNameStored.Visible = True
                'txtUserName.Visible = False
                'lblDateTime.Visible = True
                'lblDateTimeVal.Visible = True
                'lblGUID.Visible = True
                'lblGUIDVal.Visible = True
                'txtPassword.Visible = False
                'lblPassword.Visible = False
                'btnAddUser.Visible = False
            ElseIf bUserExists Then
                SuceessMessage_row("UpdateFailed") = "UserName already exists in SDiTrack"
                'lblValidate.Text = "UserName Already Exists in SDiTrack"
            End If
            AddedUserDetails_Rows("GUIDVal") = sNewUserGUID
            AddedUserDetails_Rows("DateTime") = dtTodaysDate.ToString()
            AddedUserDetails.Rows.Add(AddedUserDetails_Rows)
            Return AddedUserDetails
            'lblGUIDVal.Text = sNewUserGUID
            'lblDateTimeVal.Text = dtTodaysDate.ToString()
        Catch ex As Exception
            If bUserExists Then
                SuceessMessage_row("UpdateFailed") = "UserName already exists in SDiTrack"
                'lblValidate.Text = "UserName Already Exists in SDiTrack"
            End If
        End Try
        SuceessMessage.Rows.Add(SuceessMessage_row)
    End Function
    '*UP_PC_189
    '** This method is triggered when the Privilege type radio button is clicked in the Privileges page.
    Public Function rblType_SelectedIndexChanged(ByVal data As String) As String
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            Dim AddedUserDetails As New DataTable
            AddedUserDetails.TableName = "AddedUserDetails"

            Dim AlacartProgramTable As New DataTable
            Dim RoleDataProgramTable As New DataTable
            If Not String.IsNullOrEmpty(data) Then
                Dim objPrivilegeTypeBO As PrivilegeTypeBO = JsonConvert.DeserializeObject(Of PrivilegeTypeBO)(data)
                'rcbGroupTab2.Visible = True
                If objPrivilegeTypeBO.PriviTypeIndex = 1 Then  '  If rblType.SelectedItem.Value.ToLower().Equals("role") Then
                    'Dim sPortal As String = GetPortal()
                    'LoadRoleMaster(sPortal)
                    'ddlUserRole.Enabled = True 21/07/2020 Handle in the front end
                    RoleDataProgramTable = GetRoleData(objPrivilegeTypeBO.UserRoleSelected)
                    RoleDataProgramTable.TableName = "RoleDataProgramTable"
                    _result.Tables.Add(RoleDataProgramTable)
                Else
                    'ddlUserRole.Enabled = False 21/07/2020 Handle in the front end
                    AlacartProgramTable = GetAlaCarteData(objPrivilegeTypeBO.UserId, objPrivilegeTypeBO.SiteValue, objPrivilegeTypeBO.Page_Action, objPrivilegeTypeBO.UserType, objPrivilegeTypeBO.Query_IsVendor, objPrivilegeTypeBO.Query_IsMexicoVendor, objPrivilegeTypeBO.UserRole)
                    AlacartProgramTable.TableName = "AlacartProgramTable"
                    _result.Tables.Add(AlacartProgramTable)

                End If
                '21/07/2020 Handle in the front end
                'If radioUserType.SelectedValue = "V" Then
                '    rcbGroupTab2.Visible = False
                'Else

                'End If
                data = JsonConvert.SerializeObject(_result)
            End If
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '*UP_PC_186
    '** This method is triggered when the Role drop down value is selected in the Privileges page.
    Public Function ddlUserRole_SelectedIndexChanged(ByVal data As String) As String
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            Dim AddedUserDetails As New DataTable
            AddedUserDetails.TableName = "AddedUserDetails"
            Dim RoleDataProgramTable As New DataTable
            If Not String.IsNullOrEmpty(data) Then
                Dim objPriviRoleTypeBo As PriviRoleTypeBo = JsonConvert.DeserializeObject(Of PriviRoleTypeBo)(data)
                'rcbGroupTab2.Visible = True  21/07/2020 Handle in the front end
                RoleDataProgramTable = GetRoleData(objPriviRoleTypeBo.UserRoleSelected)
                RoleDataProgramTable.TableName = "RoleDataProgramTable"
                _result.Tables.Add(RoleDataProgramTable)

                'Added for hiding privilege & program label for corp admin
                '21/07/2020 Handle in the front end
                'If Session("ROLE") = "CORPADMIN" Then
                '    lblPrivilegeType.Visible = False
                '    rblType.Visible = False
                '    lblProgram.Visible = False
                '    rtvPrograms.Visible = False
            End If
            data = JsonConvert.SerializeObject(_result)
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '*UP_PC_187
    '* This method when the Save button is click in the Privileges form

    Public Function btnUserAccessSave_Click(ByVal data As String) As String

        'lblMessage1N.Text = ""  21/07/2020
        'Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.CheckedNodes
        Dim _result As New DataSet
        Try
            Dim Redirect_Page As New DataTable
            Redirect_Page.Columns.Add("DBErrorPage.aspx")
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
            Dim update_Session As New DataTable
            update_Session.TableName = "update_Session"
            update_Session.Columns.Add("MenuUpdated")
            Dim update_Session_row As DataRow = update_Session.NewRow()
            If Not String.IsNullOrEmpty(data) Then
                Dim objUpdPrivBo As UpdPrivBo = JsonConvert.DeserializeObject(Of UpdPrivBo)(data)
                Dim SessionTable As New DataTable
                SessionTable.Columns.Add("Session_EMAIL")
                SessionTable.Columns.Add("Session_BUSUNIT")
                SessionTable.Columns.Add("Session_USERTYPE")
                Dim _Rows As DataRow = SessionTable.NewRow()
                _Rows("Session_EMAIL") = objUpdPrivBo.Session_EMAIL
                _Rows("Session_BUSUNIT") = objUpdPrivBo.Session_BUSUNIT
                _Rows("Session_USERTYPE") = objUpdPrivBo.Session_USERTYPE
                If objUpdPrivBo.PriviTypeIndex = 1 Then  '  If rblType.SelectedItem.Value.ToLower().Equals("role") Then

                    clsAccessPrivileges.SaveUserAccessRolePrivileges(Redirect_Page, objUpdPrivBo.UserId, objUpdPrivBo.SiteValue, objUpdPrivBo.UserRoleSelected)
                    If Redirect_Page.Rows.Count > 0 Then
                        Redirect_Page.TableName = "Redirect_Page"
                        _result.Tables.Add(Redirect_Page)
                        _result.Tables.Add(update_Session)
                        _result.Tables.Add(SuceessMessage)
                        data = JsonConvert.SerializeObject(_result)
                        Return data
                        Exit Function
                    End If
                    SuceessMessage_row("UpdateSuccess") = "User information has been modified and saved successfully."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage)
                    'Hide the error msg after selecting atleast one privilege and saving 

                    'lblMessage1N.Text = Label1.Text 21/07/2020

                    update_Session_row("MenuUpdated") = True
                    update_Session.Rows.Add(update_Session_row)
                    Redirect_Page.TableName = "Redirect_Page"
                    _result.Tables.Add(Redirect_Page)
                    _result.Tables.Add(update_Session)
                Else
                    If objUpdPrivBo.PrivilegeTree.Count > 0 Then
                        clsAccessPrivileges.SaveUserAccessPrivileges(Redirect_Page, objUpdPrivBo.UserId, objUpdPrivBo.SiteValue, objUpdPrivBo.Session_UserId, objUpdPrivBo.PrivilegeTree, _Rows)
                        If Redirect_Page.Rows.Count > 0 Then
                            _result.Tables.Add(Redirect_Page)
                            _result.Tables.Add(update_Session)
                            _result.Tables.Add(SuceessMessage)
                            data = JsonConvert.SerializeObject(_result)
                            Return data
                            Exit Function
                        End If
                        SuceessMessage_row("UpdateSuccess") = "User information has been modified and saved successfully."
                        SuceessMessage.Rows.Add(SuceessMessage_row)
                        _result.Tables.Add(SuceessMessage)
                        'lblMessage1N.Text = Label1.Text   21/07/2020

                        update_Session_row("MenuUpdated") = True
                        update_Session.Rows.Add(update_Session_row)
                        Redirect_Page.TableName = "Redirect_Page"
                        _result.Tables.Add(Redirect_Page)
                        _result.Tables.Add(update_Session)
                        'Else   ************** 21/07/2020 Handle in the front end
                        '    'Adding error msg to select any one of the privilege under the program after deselecting all 
                        '    Label1.Text = "Please Select Atleast One Program."
                        '    lblMessage1N.Text = Label1.Text

                    End If
                End If
                data = JsonConvert.SerializeObject(_result)
                '************** 21/07/2020 Handle in the front end
                'rcbGroup.Items.FindItemByValue(rcbGroup.SelectedItem.Value).Selected = True

                'If txtVendorUserGroup.Visible = True Then
                '    rcbGroupTab2.Visible = False
                'Else
                '    rcbGroupTab2.Visible = True
                'End If
                ''Added for hiding privilge type & program tree for corp admin
                'If Session("ROLE") = "CORPADMIN" Then
                '    lblPrivilegeType.Visible = False
                '    rblType.Visible = False
                '    lblProgram.Visible = False
                '    rtvPrograms.Visible = False
                'End If
            End If

        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
            'lblMessage1N.Text = ""
        End Try
        Return data
    End Function
    '*UP_PC_199
    '** this method will be triggered when active employee account is click.
    Public Function btnActivateAccount_Click(ByVal data As String) As String
        Dim strSQLstring As String

        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
            Dim updateStatus As New DataTable
            updateStatus.TableName = "updateStatus"
            updateStatus.Columns.Add("ActiveStatus")
            Dim updateStatus_row As DataRow = updateStatus.NewRow()
            If Not String.IsNullOrEmpty(data) Then
                Dim objActiveAccountBo As ActiveAccountBo = JsonConvert.DeserializeObject(Of ActiveAccountBo)(data)
                ' Set the account active
                strSQLstring = "UPDATE SDIX_USERS_TBL " & vbCrLf &
                " SET active_status = '" & clsUserTbl.ActiveStatus_Active & "' " & vbCrLf &
                " , lastupdoprid = '" & objActiveAccountBo.SessionUserId.ToString & "' " & vbCrLf &
                " , lastupddttm = TO_DATE('" & Now.ToString & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf &
                " WHERE isa_employee_id = '" & objActiveAccountBo.UserId & "' " & vbCrLf &
                " AND active_status = '" & objActiveAccountBo.ActiveStatus & "' " & vbCrLf &
                " AND isa_user_id = '" & objActiveAccountBo.HidenUserId & "' "
                Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSQLstring, False)
                If rowsaffected = 1 Then
                    Dim strBU As String = ""
                    Dim strUserGroup As String = ""
                    GetSelectedBUandGroup(strBU, strUserGroup, objActiveAccountBo.SessionUserId, objActiveAccountBo.Session_USERTYPE, objActiveAccountBo.Page_Action, objActiveAccountBo.UserType, objActiveAccountBo.SiteValue, objActiveAccountBo.Query_IsMexico, objActiveAccountBo.Query_IsVendor)
                    ' Write an audit record saying the account was reactivated.
                    clsSDIAudit.AddRecord("Profile.aspx", "Activate account", "SDIX_USERS_TBL", objActiveAccountBo.SessionUserId.ToString, strBU, objActiveAccountBo.UserId, objActiveAccountBo.Session_WEBSITED, sColumnChg:="active_status",
                      sOldValue:=objActiveAccountBo.ActiveStatus, sNewValue:=clsUserTbl.ActiveStatus_Active)
                    SuceessMessage_row("UpdateSuccess") = "This SDIX User account is now active."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage)
                    '21/07/2020 Handle in the front end
                    'lblAccountDisabled.Text = "This SDIX User account is now active."
                    'btnActivateAccount.Visible = False
                    'btnInactivateAccount.Visible = True ' After activating an account, we want to give the ability to inactivate.

                    ''If user went from "failed login" to active, remove the "failed login" from the user dropdown
                    ''Dim iIndex As Integer = dropSelectUser.SelectedItem.Text.IndexOf(" - FAILED LOGIN")
                    ''If iIndex > 0 Then
                    ''    dropSelectUser.SelectedItem.Text = dropSelectUser.SelectedItem.Text.Substring(0, iIndex)
                    ''Else
                    ''    ' If user went from "inactive" to active, remove the "inactive" from the user dropdown
                    ''    iIndex = dropSelectUser.SelectedItem.Text.IndexOf(" - INACTIVE")
                    ''    dropSelectUser.SelectedItem.Text = dropSelectUser.SelectedItem.Text.Substring(0, iIndex)
                    ''End If
                    'Dim iIndex As Integer = rcbdropSelectUser.SelectedItem.Text.IndexOf(" - FAILED LOGIN")
                    'If iIndex > 0 Then
                    '    rcbdropSelectUser.SelectedItem.Text = rcbdropSelectUser.SelectedItem.Text.Substring(0, iIndex)
                    'Else
                    '    ' If user went from "inactive" to active, remove the "inactive" from the user dropdown
                    '    iIndex = rcbdropSelectUser.SelectedItem.Text.IndexOf(" - INACTIVE")
                    '    rcbdropSelectUser.SelectedItem.Text = rcbdropSelectUser.SelectedItem.Text.Substring(0, iIndex)
                    'End If
                    updateStatus_row("ActiveStatus") = clsUserTbl.ActiveStatus_Active ' Update the hidden label indicating the active status
                    updateStatus.Rows.Add(updateStatus_row)
                    _result.Tables.Add(updateStatus)


                Else
                    SuceessMessage_row("UpdateFailed") = "There was an issue trying to activate this account."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage)
                    _result.Tables.Add(updateStatus)
                    'lblDBError.Text = "There was an issue trying to activate this account."  21/07/2020
                    'lblDBError.Visible = True
                End If
            End If
            data = JsonConvert.SerializeObject(_result)
            'FieldsPosition()  This method is not 
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '*UP_PC_200
    '** this method will be triggered when In active account is click.
    Public Function btnInactivateAccount_Click(ByVal data As String) As String

        Dim strSqlupd1 As String
        ' Inactivate the account
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
            Dim updateStatus As New DataTable
            updateStatus.TableName = "updateStatus"
            updateStatus.Columns.Add("ActiveStatus")
            Dim updateStatus_row As DataRow = updateStatus.NewRow()
            If Not String.IsNullOrEmpty(data) Then
                Dim objActiveAccountBo As ActiveAccountBo = JsonConvert.DeserializeObject(Of ActiveAccountBo)(data)
                strSqlupd1 = " UPDATE " & vbCrLf &
                           " SDIX_USERS_TBL " & vbCrLf &
                     " SET " & vbCrLf &
                           " ACTIVE_STATUS = '" & clsUserTbl.ActiveStatus_Inactive & "'," & vbCrLf &
                           " LASTUPDOPRID = '" & objActiveAccountBo.SessionUserId & "'," & vbCrLf &
                           " LASTUPDDTTM = TO_DATE('" & Now().ToString() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                     " WHERE " & vbCrLf &
                           " ISA_EMPLOYEE_ID = '" & objActiveAccountBo.UserId.ToUpper & "' " & vbCrLf &
                           " AND active_status = '" & objActiveAccountBo.ActiveStatus & "' " & vbCrLf &
                           " AND isa_user_id = '" & objActiveAccountBo.HidenUserId & "' "

                Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSqlupd1, False)
                If rowsaffected = 1 Then
                    Dim strBU As String = ""
                    Dim strUserGroup As String = ""
                    GetSelectedBUandGroup(strBU, strUserGroup, objActiveAccountBo.SessionUserId, objActiveAccountBo.Session_USERTYPE, objActiveAccountBo.Page_Action, objActiveAccountBo.UserType, objActiveAccountBo.SiteValue, objActiveAccountBo.Query_IsMexico, objActiveAccountBo.Query_IsVendor)
                    ' Write an audit record saying the account was reactivated.
                    clsSDIAudit.AddRecord("Profile.aspx", "Activate account", "SDIX_USERS_TBL", objActiveAccountBo.SessionUserId.ToString, strBU, objActiveAccountBo.UserId, objActiveAccountBo.Session_WEBSITED, sColumnChg:="active_status",
                      sOldValue:=objActiveAccountBo.ActiveStatus, sNewValue:=clsUserTbl.ActiveStatus_Active)
                    SuceessMessage_row("UpdateSuccess") = "This SDIX User account is now in active."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage) 'inactivating an account, we want To give the ability To activate.

                    'If user went from "failed login" to inactive, remove the "failed login" from the user dropdown and change to inactive
                    'Dim iIndex As Integer = dropSelectUser.SelectedItem.Text.IndexOf(" - FAILED LOGIN")
                    'If iIndex > 0 Then
                    '    dropSelectUser.SelectedItem.Text = dropSelectUser.SelectedItem.Text.Substring(0, iIndex) & " - INACTIVE"
                    'Else
                    '    dropSelectUser.SelectedItem.Text = dropSelectUser.SelectedItem.Text & " - INACTIVE"
                    'End If
                    'Dim iIndex As Integer = rcbdropSelectUser.SelectedItem.Text.IndexOf(" - FAILED LOGIN")   22/07/2020
                    'If iIndex > 0 Then
                    '    rcbdropSelectUser.SelectedItem.Text = rcbdropSelectUser.SelectedItem.Text.Substring(0, iIndex) & " - INACTIVE"
                    'Else
                    '    rcbdropSelectUser.SelectedItem.Text = rcbdropSelectUser.SelectedItem.Text & " - INACTIVE"
                    'End If
                    updateStatus_row("ActiveStatus") = clsUserTbl.ActiveStatus_Inactive ' Update the hidden label indicating the active status
                    updateStatus.Rows.Add(updateStatus_row)
                    _result.Tables.Add(updateStatus)

                Else
                    SuceessMessage_row("UpdateFailed") = "There was an issue trying to activate this account."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage)
                    _result.Tables.Add(updateStatus)
                    'lblDBError.Text = "There was an issue trying to activate this account."  21/07/2020
                    'lblDBError.Visible = True
                End If
            End If
            data = JsonConvert.SerializeObject(_result)
            'FieldsPosition()  This method is not 
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
        End Try
        Return data
    End Function
    '*UP_PC_201
    '** this method will be triggered when active employee account is click.
    Public Function btnEmplActivateAccount_Click(ByVal data As String) As String
        Dim strSQLstring As String = ""
        Dim iRowsAffected As Integer = 0
        Dim strUserId As String = ""
        Dim strCurrBU As String = ""

        ' Update Employee table based on User Id and Business Unit 
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
            Dim updateStatus As New DataTable
            updateStatus.TableName = "updateStatus"
            updateStatus.Columns.Add("ActiveStatus")
            Dim updateStatus_row As DataRow = updateStatus.NewRow()
            If Not String.IsNullOrEmpty(data) Then
                Dim objActiveEmpAccountBo As ActiveEmpAccountBo = JsonConvert.DeserializeObject(Of ActiveEmpAccountBo)(data)
                'strCurrBU = lblCurrBUHide.Text   20/07/2020 Handle in the front end
                'If Trim(strCurrBU) = "" Or Trim(strCurrBU) = "0" Then
                '    'error
                '    lblDBError.Text = "There was an issue trying to activate this Employee account."
                '    lblDBError.Visible = True
                '    Exit Sub
                'End If
                ''strUserId = dropSelectUser.SelectedValue
                'strUserId = rcbdropSelectUser.SelectedValue
                'If Trim(strUserId) = "" Then
                '    'error
                '    lblDBError.Text = "There was an issue trying to activate this Employee account."
                '    lblDBError.Visible = True
                '    Exit Sub
                'End If
                'update code
                strSQLstring = "UPDATE PS_ISA_EMPL_TBL " & vbCrLf &
                " SET EFF_STATUS = '" & clsUserTbl.EmplActiveStatus_Active & "' " & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & objActiveEmpAccountBo.UserId & "' AND BUSINESS_UNIT = '" & objActiveEmpAccountBo.UserBu & "' " & vbCrLf &
                " AND EFF_STATUS = '" & objActiveEmpAccountBo.ActiveStatus & "' "

                iRowsAffected = ORDBData.ExecNonQuery(strSQLstring, False)
                If iRowsAffected = 1 Then
                    SuceessMessage_row("UpdateSuccess") = "This employee account is now active."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage)
                    'btnEmplActivateAccount.Visible = False  22/07/2020
                    'btnEmplInactivateAccount.Visible = True
                    'lblEmplAccountDisabled.Text = "This Employee account is now active."
                    'lblEmplAccountDisabled.Visible = True
                    'create audit record
                    clsSDIAudit.AddRecord("Profile.aspx", "Activate account", "SDIX_USERS_TBL", objActiveEmpAccountBo.SessionUserId.ToString, objActiveEmpAccountBo.SiteValue, objActiveEmpAccountBo.UserId, objActiveEmpAccountBo.Session_WEBSITED, sColumnChg:="EFF_STATUS",
                      sOldValue:=clsUserTbl.EmplActiveStatus_Inactive, sNewValue:=clsUserTbl.EmplActiveStatus_Active)

                    updateStatus_row("ActiveStatus") = clsUserTbl.EmplActiveStatus_Active ' Update the hidden label indicating the active status
                    updateStatus.Rows.Add(updateStatus_row)
                    _result.Tables.Add(updateStatus)
                    'lblEmplActiveStatusHide.Text = clsUserTbl.EmplActiveStatus_Active
                Else
                    'error updating
                    SuceessMessage_row("UpdateFailed") = "There was an issue trying to activate this account."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage)
                    _result.Tables.Add(updateStatus)
                    data = JsonConvert.SerializeObject(_result)
                    Return data
                    Exit Function
                    'lblDBError.Text = "There was an issue trying to activate this Employee account."
                    'lblDBError.Visible = True
                    'Exit Sub
                End If
            End If
            data = JsonConvert.SerializeObject(_result)
            'FieldsPosition()  This method is not 
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
            ' send error email
        End Try
        Return data
    End Function
    '*UP_PC_202
    '** this method will be triggered when In active employee account is click.
    Public Function btnEmplInactivateAccount_Click(ByVal data As String) As String
        Dim strSQLstring As String = ""
        Dim iRowsAffected As Integer = 0
        Dim strUserId As String = ""
        Dim strCurrBU As String = ""

        ' Update Employee table based on User Id and Business Unit selected
        Dim _result As New DataSet
        Try
            Dim SuceessMessage As New DataTable
            SuceessMessage.TableName = "SuceessMessage"
            SuceessMessage.Columns.Add("UpdateSuccess")
            SuceessMessage.Columns.Add("UpdateFailed")
            Dim SuceessMessage_row As DataRow = SuceessMessage.NewRow()
            Dim updateStatus As New DataTable
            updateStatus.TableName = "updateStatus"
            updateStatus.Columns.Add("ActiveStatus")
            Dim updateStatus_row As DataRow = updateStatus.NewRow()
            If Not String.IsNullOrEmpty(data) Then
                Dim objActiveEmpAccountBo As ActiveEmpAccountBo = JsonConvert.DeserializeObject(Of ActiveEmpAccountBo)(data)
                '    strCurrBU = lblCurrBUHide.Text handle in the front end
                'If Trim(strCurrBU) = "" Or Trim(strCurrBU) = "0" Then
                '    'error
                '    lblDBError.Text = "There was an issue trying to inactivate this Employee account."
                '    lblDBError.Visible = True
                '    Exit Sub
                'End If
                ''strUserId = dropSelectUser.SelectedValue
                'strUserId = rcbdropSelectUser.SelectedValue
                'If Trim(strUserId) = "" Then
                '    'error
                '    lblDBError.Text = "There was an issue trying to inactivate this Employee account."
                '    lblDBError.Visible = True
                '    Exit Sub
                'End If
                'update code
                strSQLstring = "UPDATE PS_ISA_EMPL_TBL " & vbCrLf &
                " SET EFF_STATUS = '" & clsUserTbl.EmplActiveStatus_Inactive & "' " & vbCrLf &
                " WHERE ISA_EMPLOYEE_ID = '" & objActiveEmpAccountBo.UserId & "' AND BUSINESS_UNIT = '" & objActiveEmpAccountBo.SiteValue & "'" & vbCrLf &
                " AND EFF_STATUS = '" & objActiveEmpAccountBo.ActiveStatus & "' "

                iRowsAffected = ORDBData.ExecNonQuery(strSQLstring, False)
                If iRowsAffected = 1 Then
                    SuceessMessage_row("UpdateSuccess") = "This employee account is now in active."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage)
                    ''btnEmplActivateAccount.Visible = True  22/07/2020 
                    ''btnEmplInactivateAccount.Visible = False
                    ''lblEmplAccountDisabled.Text = "This Employee account is now NOT active."
                    ''lblEmplAccountDisabled.Visible = True
                    'create audit record
                    clsSDIAudit.AddRecord("Profile.aspx", "Activate account", "PS_ISA_EMPL_TBL", objActiveEmpAccountBo.SessionUserId.ToString, objActiveEmpAccountBo.SiteValue, objActiveEmpAccountBo.UserId, objActiveEmpAccountBo.Session_WEBSITED, sColumnChg:="EFF_STATUS",
                      sOldValue:=clsUserTbl.EmplActiveStatus_Active, sNewValue:=clsUserTbl.EmplActiveStatus_Inactive)
                    updateStatus_row("ActiveStatus") = clsUserTbl.EmplActiveStatus_Inactive ' Update the hidden label indicating the active status
                    updateStatus.Rows.Add(updateStatus_row)
                    _result.Tables.Add(updateStatus)

                    'lblEmplActiveStatusHide.Text = clsUserTbl.EmplActiveStatus_Inactive
                Else
                    SuceessMessage_row("UpdateFailed") = "There was an issue trying to inactivate this employee account."
                    SuceessMessage.Rows.Add(SuceessMessage_row)
                    _result.Tables.Add(SuceessMessage)
                    _result.Tables.Add(updateStatus)
                    data = JsonConvert.SerializeObject(_result)
                    Return data
                    Exit Function
                    'error updating
                    'lblDBError.Text = "There was an issue trying to inactivate this Employee account."
                    'lblDBError.Visible = True
                    'Exit Sub
                End If
            End If
            data = JsonConvert.SerializeObject(_result)
            'FieldsPosition()
        Catch ex As Exception
            Dim eobj As ExceptionHelper = New ExceptionHelper()
            eobj.writeException(ex)
            data = String.Empty
            'lblDBError.Text = "There was an issue trying to inactivate this Employee account."
            'lblDBError.Visible = True
            '' send error email
            'Dim strSubject As String = "Error in Profile.aspx"
        End Try
        Return data
    End Function
End Class
