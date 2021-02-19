Imports Oracle.DataAccess.Client
Imports Telerik.Web.UI
Imports System.Data.OleDb
Imports System.Collections.Generic
Imports System.Data.OracleClient
Imports Insiteonline.clsAccessPrivileges
Imports Insiteonline.WebPartnerFunctions.WebPSharedFunc

Public Class RoleMaster
    ''Inherits SDIPageBase
    Inherits PageBase

    Private Const m_cNewRole As Integer = -1

#Region "Page Load"
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Dim lblTitle As Label = CType(Master.FindControl("lblTitle"), Label)
            lblTitle.Text = "Role Master"

            Response.Cache.SetCacheability(HttpCacheability.NoCache)

            If Session("SDIEMP") = "" Or Session("USERID") = "" Then
                Session.RemoveAll()
                Response.Redirect("default.aspx")
            Else
                If Not IsPostBack Then
                    GetUserDatas()
                End If
            End If

            If Not IsPostBack Then
                If Session("USERTYPEVALUE") = "C" Then
                    BuildGroupList()
                    BindRoleGrid()
                Else
                    BuildGroupList()
                    BindRoleGrid()
                End If
                WebLog()
            End If

            Dim testdata As String = Session("USERTYPEVALUE")
            If Session("USERTYPEVALUE") = "C" Then
                If Session("USERTYPE") = "CORPADMIN" Then
                    rblType.SelectedValue = "Customer"
                    rblType.Items.FindByValue("Vendor").Enabled = False
                    rblType.Items.FindByValue("Vendor").Attributes.CssStyle(HtmlTextWriterStyle.Display) = "none"
                    'chkboxApplyallSisterBU.Visible = True
                Else
                    rblType.SelectedValue = "Customer"
                    rblType.Items.FindByValue("Vendor").Enabled = False
                    rblType.Items.FindByValue("Vendor").Attributes.CssStyle(HtmlTextWriterStyle.Display) = "none"
                    'chkboxApplyallSisterBU.Visible = False
                End If
            End If
            lblMsg.Text = ""

        Catch ex As Exception

        End Try
    End Sub
#End Region

#Region "Private Methods and Functions"
    Private Function GetRoleDetails(ByVal iRoleID As Integer) As DataSet
        Dim dsRoleData As DataSet

        Try
            Dim strSQLstring As String

            strSQLstring = "SELECT URT.ROLENUM, ROLENAME, BUSINESS_UNIT, ROLETYPE, ROLEPAGE, ALIAS_NAME " & vbCrLf & _
                            " FROM SDIX_USRROLE_TBL URT, SDIX_ROLEDETAIL RDT " & vbCrLf & _
                            " WHERE URT.ROLENUM = RDT.ROLENUM AND URT.ROLENUM = " & iRoleID

            dsRoleData = ORDBData.GetAdapter(strSQLstring)
        Catch ex As Exception

        End Try
        Return dsRoleData
    End Function

    Private Sub BindRoleGrid()
        Try
            Dim dsRoleData As DataSet
            dsRoleData = GetRoles()
            rgRoles.DataSource = dsRoleData
            rgRoles.DataBind()
            Session("ROLES_DS") = dsRoleData            
            HideShowPanels(True)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub BindRoleDetails(ByVal iRoleNum As Integer)
        Try
            Dim dsRoleData As DataSet

            dsRoleData = GetRoleDetails(iRoleNum)

            If dsRoleData.Tables(0).Rows.Count > 0 Then
                Dim drEntData As DataRow
                drEntData = dsRoleData.Tables(0).Rows(0)
                lblVRoleID.Text = drEntData("ROLENUM")
                txtRole.Text = drEntData("ROLENAME")                
                lblPortal.Text = drEntData("ROLETYPE")                
                If Trim(lblPortal.Text) = "Customer" Then
                    rcbBU.FindItemByValue(drEntData("BUSINESS_UNIT")).Selected = True
                    If Session("USERTYPEVALUE") = "S" Then
                        lblUserType.Visible = True
                        radioUserType.Visible = True
                        radioUserType.SelectedValue = "Customer"
                    Else
                        radioUserType.Visible = False
                        lblUserType.Visible = False
                    End If
                ElseIf Trim(lblPortal.Text) = "Vendor" Then
                    rcbBU.Items.Clear()
                    Dim Option1 As RadComboBoxItem = New RadComboBoxItem()
                    Option1.Text = "ISA00"
                    Option1.Value = "ISA00"
                    Dim Option2 As RadComboBoxItem = New RadComboBoxItem()
                    Option2.Text = "SDM00"
                    Option2.Value = "SDM00"
                    rcbBU.Items.Clear()
                    rcbBU.Items.Add(Option1)
                    rcbBU.Items.Add(Option2)
                    rcbBU.FindItemByValue(drEntData("BUSINESS_UNIT")).Selected = True
                    radioUserType.Visible = False
                    lblUserType.Visible = False
                Else
                    rcbBU.Visible = False
                    lblBU.Visible = False
                    chkboxApplyallSisterBU.Visible = False
                    If Session("USERTYPEVALUE") = "S" Then
                        lblUserType.Visible = True
                        radioUserType.Visible = True
                        radioUserType.SelectedValue = "SDI"
                    Else
                        radioUserType.Visible = False
                        lblUserType.Visible = False
                    End If
                End If
                txtRoleDefaultPage.Text = Convert.ToString(drEntData("ROLEPAGE"))
            End If
            HideShowPanels(False)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub HideShowPanels(ByVal isGridVisible As Boolean)
        Try
            pnlGrid.Visible = isGridVisible
            pnlDetails.Visible = Not isGridVisible
            btnAddRole.Visible = isGridVisible
            btnCpyRole.Visible = isGridVisible
            rblType.Visible = isGridVisible
        Catch ex As Exception

        End Try
    End Sub

    Private Sub GetProgramData(ByVal sPortal As String)
        Const cAllAccessGroups As String = ""

        Dim dsProgramData As DataSet
        Try
            dsProgramData = clsProgramMaster.GetPrograms(sPortal, Session("USERID"), Session("UserType"), Session("SDIEmp"), clsProgramMaster.AccessGroupEnum.LeastRestricted)

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

            rtvPrograms.DataSource = dsProgramData
            rtvPrograms.DataBind()
        Catch ex As Exception

        End Try
    End Sub

    Private Function GetRoleData(ByVal iRoleID As Integer) As DataSet
        Dim dsProgramData As DataSet
        Try

            If iRoleID = m_cNewRole Then
                ' This initializes programs to "on" as a default for a new role.
                Dim homeNode As RadTreeNode
                homeNode = rtvPrograms.Nodes.FindNodeByValue(GetPrivilegeMoniker(UserPrivsEnum.Home))
                If homeNode IsNot Nothing Then
                    homeNode.Checked = True
                End If
            Else
                Dim strQuery As String = "select ALIAS_NAME from SDIX_ROLEDETAIL where ROLENUM = " & iRoleID
                dsProgramData = ORDBData.GetAdapter(strQuery)

                Dim dr As DataRow
                Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.GetAllNodes()
                For Each node As RadTreeNode In nodeCollection
                    For Each dr In dsProgramData.Tables(0).Rows
                        Dim name As String = dr("ALIAS_NAME")
                        If node.Value = name Then
                            node.Checked = True
                        End If
                    Next
                Next
            End If
        Catch ex As Exception

        End Try
        Return dsProgramData
    End Function

#End Region

#Region "Event Handlers"
    Sub rgRoles_SortCommand(ByVal s As Object, ByVal e As GridSortCommandEventArgs)
        Try
            BindRoleGrid()
        Catch ex As Exception

        End Try
    End Sub

    Sub rgRoles_ItemCommand(ByVal sender As Object, ByVal e As CommandEventArgs)
        Try
            lblerrorr_rolelst.Text = ""
            rdRolesLst.Visible = False
            lblError_BU.Text = ""
            RdbxBU.Visible = False
            If e.CommandName.ToLower = "viewdetails" Then
                hdnRoleID.Value = e.CommandArgument
                BindRoleDetails(hdnRoleID.Value)
                roleErrLbl.Text = ""
                lblRoleID.Visible = False
                lblVRoleID.Visible = False
                Dim sPortal As String = GetRoleType(hdnRoleID.Value)
                If Session("USERTYPEVALUE") = "S" Then
                    If radioUserType.SelectedValue = "SDI" Then
                        chkboxApplyallSisterBU.Visible = False
                    Else
                        chkboxApplyallSisterBU.Visible = True
                    End If
                Else
                    chkboxApplyallSisterBU.Visible = True
                End If
                GetProgramData(sPortal)
                Dim dsRolePrograms As DataSet = GetRoleData(hdnRoleID.Value)
                UpdateRoleWithProgramMasterChanges(hdnRoleID.Value, dsRolePrograms, sPortal)
                If lblPortal.Text = "SDI" Then
                    lblPortal.Text = "Customer"
                ElseIf lblPortal.Text = "Vendor" Then
                    chkboxApplyallSisterBU.Visible = False
                    rcbBU.Visible = True
                    lblBU.Visible = True
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        Try        
            If lblAction.Text = "COPYROLES" Then
                lblroleslst.Visible = True
                lblPortalHdr.Visible = True
                lblUserType.Visible = True
                rcbBU.Visible = False
                Dim roleName As String
                Dim roleID As New DataSet
                Dim sSQLstring As String
                Dim txtRole_String As String
                txtRole_String = txtRole.Text
                Dim errorcounts As Integer = 0
                If rdRolesLst.SelectedValue = "0" Then
                    lblerrorr_rolelst.Text = "Please select any roles"
                    errorcounts = errorcounts + 1
                Else
                    lblerrorr_rolelst.Text = ""
                End If
                If Trim(txtRole.Text) = "" Then
                    roleErrLbl.Text = "Please enter role name"
                    errorcounts = errorcounts + 1
                Else
                    roleErrLbl.Text = ""
                End If
                If radioUserType.SelectedValue = "Customer" Or lblPortal.Text = "Vendor" Then
                    If RdbxBU.CheckedItems.Count() = 0 Then
                        lblError_BU.Text = "Please select business unit"
                        errorcounts = errorcounts + 1
                    Else
                        lblError_BU.Text = ""
                    End If
                Else
                    lblBU.Visible = False
                End If
                If lblPortal.Text = "Vendor" Then
                    lblUserType.Visible = False
                End If
                If Session("USERTYPEVALUE") = "C" Then
                    If lblPortal.Text = "Customer" Then
                        lblUserType.Visible = False
                    Else
                        lblUserType.Visible = False
                    End If
                End If
                If errorcounts > 0 Then
                    Exit Sub
                End If
                If radioUserType.SelectedValue = "Customer" Or lblPortal.Text = "Vendor" Then                    
                    If RdbxBU.CheckedItems.Count() > 0 Then
                        Dim buError As List(Of String) = New List(Of String)()
                        For Each item As RadComboBoxItem In RdbxBU.CheckedItems
                            sSQLstring = "Select ROLENAME from SDIX_USRROLE_TBL Where UPPER(ROLENAME)= '" + txtRole_String.ToUpper() + "' AND BUSINESS_UNIT = '" + item.Value + "'"
                            roleName = ORDBData.GetScalar(sSQLstring)
                            If String.IsNullOrEmpty(roleName) Then
                                roleErrLbl.Text = ""
                            Else
                                roleErrLbl.Text = "Role name already exists."
                                buError.Add(item.Value)
                            End If
                        Next
                        If buError.Count() > 0 Then
                            ''roleErrLbl.Text = "The RoleName Already Exists for Business units " & String.Join(",", buError)
                            ''roleErrLbl.Text = "The RoleName Already Exists for Business units " & String.Join(",", buError)
                            errorcounts = errorcounts + 1
                        Else
                            roleErrLbl.Text = ""
                        End If
                    End If
                Else
                    sSQLstring = "Select ROLENAME from SDIX_USRROLE_TBL Where UPPER(ROLENAME)= '" + txtRole_String.ToUpper() + "'"
                    roleName = ORDBData.GetScalar(sSQLstring)
                    If String.IsNullOrEmpty(roleName) Then
                        roleErrLbl.Text = ""
                    Else
                        roleErrLbl.Text = "Role name already exists."
                        errorcounts = errorcounts + 1
                    End If                    
                End If
                
                If errorcounts > 0 Then
                    Exit Sub
                End If

                Dim nodeCollection1 As IList(Of RadTreeNode) = rtvPrograms.CheckedNodes
                If nodeCollection1.Count = 0 Then
                    rcbBU.Visible = False
                    lbLNodeValidation.Text = "Select one or more Programs"
                Else
                    If nodeCollection1.Count = 1 Then
                        Dim dtChildNodes As New DataTable
                        dtChildNodes.Clear()
                        dtChildNodes.Columns.Add("ProductName", GetType(String))
                        Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.CheckedNodes
                        For Each node As RadTreeNode In nodeCollection
                            Dim values As String() = {node.Value}
                            If Not String.IsNullOrWhiteSpace(values(0)) Then
                                dtChildNodes.Rows.Add(values(0))
                            End If
                        Next
                        If Convert.ToString(dtChildNodes.Rows(0).Item("ProductName")) = "HOME" Then

                            lbLNodeValidation.Text = "Select one or more Programs"
                            EnableDisableFields()
                            If lblPortal.Text = "Vendor" Then
                                radioUserType.Visible = False
                                lblUserType.Visible = False
                                chkboxApplyallSisterBU.Visible = False
                            End If
                            rcbBU.Visible = False
                            Exit Sub
                        Else
                            lbLNodeValidation.Text = ""
                        End If
                    End If
                    Dim dtMissingMonikers As New DataTable
                    If IsMissingMonikers(dtMissingMonikers) Then
                        rcbBU.Visible = False
                        lbLNodeValidation.Text = "Monikers are missing for the following selected programs: <br /><br />"
                        For Each row As DataRow In dtMissingMonikers.Rows
                            lbLNodeValidation.Text += "<br/>" + row.Item(0).ToString
                        Next
                    Else
                        If radioUserType.SelectedValue = "Customer" Or lblPortal.Text = "Vendor" Then
                            For Each item As RadComboBoxItem In RdbxBU.CheckedItems
                                Dim sRoleType As String = ""
                                Dim sBusinessUnit As String = ""
                                If lblPortal.Text = "Customer" Then
                                    If Session("USERTYPEVALUE") = "S" Then
                                        If radioUserType.SelectedValue = "SDI" Then
                                            sRoleType = Convert.ToString(radioUserType.SelectedValue)
                                            sBusinessUnit = " "
                                        Else
                                            sRoleType = lblPortal.Text
                                            sBusinessUnit = item.Value
                                        End If
                                    Else
                                        sRoleType = lblPortal.Text
                                        sBusinessUnit = item.Value
                                    End If
                                Else
                                    sRoleType = lblPortal.Text
                                    sBusinessUnit = item.Value
                                End If
                                Dim txtRoleString As String = txtRole.Text.ToUpper
                                lbLNodeValidation.Text = " "

                                AddNewRole(txtRoleString, sBusinessUnit, sRoleType)
                                lblMsg.Text = "New Role details created Successfully."
                                lblMsg.Visible = True
                                clsSDIAudit.AddRecord("RoleMaster.aspx", "Created New Role", "SDIX_USRROLE_TBL", Session("USERID").ToString, Session("BUSUNIT").ToString, Session("USERID").ToString, _
                                sOldValue:=txtRoleString, sNewValue:=txtRoleString, sUDF1:=txtRoleString, sUDF2:="Portal " & sRoleType, sUDF3:="BU " & sBusinessUnit)
                            Next
                        Else
                            Dim sRoleType As String = ""
                            Dim sBusinessUnit As String = ""
                            sRoleType = Convert.ToString(radioUserType.SelectedValue)
                            sBusinessUnit = " "
                            Dim txtRoleString As String = txtRole.Text.ToUpper
                            lbLNodeValidation.Text = " "
                            AddNewRole(txtRoleString, sBusinessUnit, sRoleType)
                            lblMsg.Text = "New Role details created Successfully."
                            lblMsg.Visible = True
                            clsSDIAudit.AddRecord("RoleMaster.aspx", "Created New Role", "SDIX_USRROLE_TBL", Session("USERID").ToString, Session("BUSUNIT").ToString, Session("USERID").ToString, _
                            sOldValue:=txtRoleString, sNewValue:=txtRoleString, sUDF1:=txtRoleString, sUDF2:="Portal " & sRoleType, sUDF3:="BU " & sBusinessUnit)
                        End If

                        txtRole.Text = ""
                        txtRoleDefaultPage.Text = ""
                        rtvPrograms.UncheckAllNodes()
                    End If
                End If
            Else
                Dim roleName As String
                Dim roleID As New DataSet
                Dim sSQLstring As String
                Dim txtRole_String As String

                ltrErrDefaultPage.Visible = False
                If Not PageExists(txtRoleDefaultPage.Text) Then
                    ltrErrDefaultPage.Visible = True
                    EnableDisableFields()
                    If lblPortal.Text = "Vendor" Then
                        radioUserType.Visible = False
                        lblUserType.Visible = False
                        chkboxApplyallSisterBU.Visible = False
                    End If
                    Exit Sub
                End If

                If Trim(txtRole.Text) = "" Then
                    roleErrLbl.Text = "Please enter role name"
                    EnableDisableFields()
                    If lblPortal.Text = "Vendor" Then
                        radioUserType.Visible = False
                        lblUserType.Visible = False
                        chkboxApplyallSisterBU.Visible = False
                    End If

                    Exit Sub
                Else
                    roleErrLbl.Text = ""
                End If
                txtRole_String = Trim(txtRole.Text)
                If lblAction.Text = "ADDROLES" Then
                    If radioUserType.SelectedValue = "Customer" Or lblPortal.Text = "Vendor" Then

                        sSQLstring = "Select ROLENAME from SDIX_USRROLE_TBL Where UPPER(ROLENAME)= '" + txtRole_String.ToUpper() + "' AND BUSINESS_UNIT = '" + rcbBU.SelectedValue + "'"
                        roleName = ORDBData.GetScalar(sSQLstring)
                        If String.IsNullOrEmpty(roleName) Then
                            roleErrLbl.Text = ""
                        Else
                            roleErrLbl.Text = "Role name already exists."
                            Exit Sub
                        End If
                    Else
                        lblBU.Visible = False
                        lblUserType.Visible = True
                        sSQLstring = "Select ROLENAME from SDIX_USRROLE_TBL Where UPPER(ROLENAME)= '" + txtRole_String.ToUpper() + "'"
                        roleName = ORDBData.GetScalar(sSQLstring)
                        If String.IsNullOrEmpty(roleName) Then
                            roleErrLbl.Text = ""
                        Else
                            roleErrLbl.Text = "Role name already exists."
                            Exit Sub
                        End If
                    End If
                End If
                
                Dim nodeCollection1 As IList(Of RadTreeNode) = rtvPrograms.CheckedNodes
                If nodeCollection1.Count = 0 Then
                    lbLNodeValidation.Text = "Select one or more Programs"
                Else
                    If nodeCollection1.Count = 1 Then
                        Dim dtChildNodes As New DataTable
                        dtChildNodes.Clear()
                        dtChildNodes.Columns.Add("ProductName", GetType(String))
                        Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.CheckedNodes
                        For Each node As RadTreeNode In nodeCollection
                            Dim values As String() = {node.Value}
                            If Not String.IsNullOrWhiteSpace(values(0)) Then
                                dtChildNodes.Rows.Add(values(0))
                            End If
                        Next
                        If Convert.ToString(dtChildNodes.Rows(0).Item("ProductName")) = "HOME" Then
                            lbLNodeValidation.Text = "Select one or more Programs"
                            EnableDisableFields()
                            If lblPortal.Text = "Vendor" Then
                                radioUserType.Visible = False
                                lblUserType.Visible = False
                                chkboxApplyallSisterBU.Visible = False
                            End If
                            Exit Sub
                        Else
                            lbLNodeValidation.Text = ""
                        End If
                    End If
                    Dim dtMissingMonikers As New DataTable
                    If IsMissingMonikers(dtMissingMonikers) Then
                        lbLNodeValidation.Text = "Monikers are missing for the following selected programs: <br /><br />"
                        For Each row As DataRow In dtMissingMonikers.Rows
                            lbLNodeValidation.Text += "<br/>" + row.Item(0).ToString
                        Next
                    Else
                        Dim sRoleType As String = ""
                        Dim sBusinessUnit As String = ""
                        If lblPortal.Text = "Customer" Then
                            If Session("USERTYPEVALUE") = "S" Then
                                If radioUserType.SelectedValue = "SDI" Then
                                    sRoleType = Convert.ToString(radioUserType.SelectedValue)
                                    sBusinessUnit = " "
                                Else
                                    sRoleType = lblPortal.Text
                                    sBusinessUnit = rcbBU.SelectedValue
                                End If
                            Else
                                sRoleType = lblPortal.Text
                                sBusinessUnit = rcbBU.SelectedValue
                            End If
                        Else
                            sRoleType = lblPortal.Text
                            sBusinessUnit = rcbBU.SelectedValue
                        End If

                        Dim txtRoleString As String = txtRole.Text.ToUpper

                        lbLNodeValidation.Text = " "

                        If hdnRoleID.Value = "0" And String.IsNullOrEmpty(roleErrLbl.Text) Then
                            If chkboxApplyallSisterBU.Checked Then
                                Dim dsBUSisterSites As DataSet = UnilogORDBData.SisterBusinessUnits(rcbBU.SelectedValue)
                                If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
                                    For Each dr As DataRow In dsBUSisterSites.Tables(0).Rows
                                        AddNewRole(txtRoleString, dr.Item("BUSINESS_UNIT"), sRoleType)
                                    Next
                                    lblMsg.Text = "New Role details created Sucessfully for all the sister site BU's."
                                    lblMsg.Visible = True
                                    clsSDIAudit.AddRecord("RoleMaster.aspx", "Created New Role", "SDIX_USRROLE_TBL", Session("USERID").ToString, Session("BUSUNIT").ToString, Session("USERID").ToString, _
                                    sOldValue:=txtRoleString, sNewValue:=txtRoleString, sUDF1:=txtRoleString, sUDF2:="Portal " & sRoleType, sUDF3:="BU " & sBusinessUnit)
                                Else

                                End If
                            Else
                                AddNewRole(txtRoleString, sBusinessUnit, sRoleType)
                                lblMsg.Text = "New Role details created Successfully."
                                lblMsg.Visible = True
                                clsSDIAudit.AddRecord("RoleMaster.aspx", "Created New Role", "SDIX_USRROLE_TBL", Session("USERID").ToString, Session("BUSUNIT").ToString, Session("USERID").ToString, _
                                sOldValue:=txtRoleString, sNewValue:=txtRoleString, sUDF1:=txtRoleString, sUDF2:="Portal " & sRoleType, sUDF3:="BU " & sBusinessUnit)
                            End If
                            txtRole.Text = ""
                            txtRoleDefaultPage.Text = ""
                            rtvPrograms.UncheckAllNodes()
                        Else
                            If chkboxApplyallSisterBU.Checked Then
                                Dim dsBUSisterSites As DataSet = UnilogORDBData.SisterBusinessUnits(rcbBU.SelectedValue)
                                If dsBUSisterSites.Tables(0).Rows.Count > 0 Then
                                    Dim oldRoleName As String = GetRoleName()
                                    Dim CurrentRoleName As String = hdnRoleName.Value
                                    For Each dr As DataRow In dsBUSisterSites.Tables(0).Rows
                                        If sBusinessUnit = dr.Item("BUSINESS_UNIT") Then
                                            UpdateExistingRole(txtRoleString, dr.Item("BUSINESS_UNIT"), sRoleType)
                                        Else
                                            Dim s_RoleID As String
                                            Dim s_SQLstring As String = "Select ROLENUM FROM SDIX_USRROLE_TBL WHERE ROLENAME = '" & CurrentRoleName & "' AND BUSINESS_UNIT = '" & dr.Item("BUSINESS_UNIT") & "'"
                                            s_RoleID = ORDBData.GetScalar(s_SQLstring)
                                            If s_RoleID <> "" Then
                                                ''Update
                                                UpdateExistingRole_WhileUpdatingForSisterBU(txtRoleString, dr.Item("BUSINESS_UNIT"), sRoleType, s_RoleID)
                                            Else
                                                ''Insert
                                                AddNewRole(txtRoleString, dr.Item("BUSINESS_UNIT"), sRoleType)
                                            End If
                                        End If
                                    Next
                                    lblMsg.Text = "Role details Updated Sucessfully for all the sister site BU's."
                                    lblMsg.Visible = True
                                    clsSDIAudit.AddRecord("RoleMaster.aspx", "Updated Role Details", "SDIX_USRROLE_TBL", Session("USERID").ToString, Session("BUSUNIT").ToString, Session("USERID").ToString, _
                                    sOldValue:=oldRoleName, sNewValue:=txtRoleString, sColumnChg:="ROLENAME", sUDF1:=txtRoleString, sUDF2:="Portal " & sRoleType, sUDF3:="BU " & sBusinessUnit)
                                End If
                            Else
                                Dim oldRoleName As String = GetRoleName()
                                UpdateExistingRole(txtRoleString, sBusinessUnit, sRoleType)
                                lblMsg.Text = "Role details Updated Successfully."
                                lblMsg.Visible = True
                                clsSDIAudit.AddRecord("RoleMaster.aspx", "Updated Role Details", "SDIX_USRROLE_TBL", Session("USERID").ToString, Session("BUSUNIT").ToString, Session("USERID").ToString, _
                                sOldValue:=oldRoleName, sNewValue:=txtRoleString, sColumnChg:="ROLENAME", sUDF1:=txtRoleString, sUDF2:="Portal " & sRoleType, sUDF3:="BU " & sBusinessUnit)
                            End If
                        End If
                    End If
                End If
                EnableDisableFields()
                If lblPortal.Text = "Vendor" Then
                    radioUserType.Visible = False
                    lblUserType.Visible = False
                    chkboxApplyallSisterBU.Visible = False
                End If
            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Function GetRoleName()
        Try
            Dim dsRoleData As DataSet
            Dim roleName As String = ""
            Dim sSQLstring As String = "Select ROLENAME from SDIX_USRROLE_TBL Where ROLENUM= '" & hdnRoleID.Value & "'"
            dsRoleData = ORDBData.GetAdapter(sSQLstring)

            If Not dsRoleData Is Nothing Then
                If dsRoleData.Tables(0).Rows.Count > 0 Then
                    roleName = Convert.ToString(dsRoleData.Tables(0).Rows(0).Item("ROLENAME"))
                Else
                    roleName = ""
                End If
            Else
                roleName = ""
            End If
            Return roleName
        Catch ex As Exception
            Return ""
        End Try
    End Function


    Protected Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCancel.Click
        Try
            Response.Redirect("RoleMaster.aspx")            
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub btnAddRole_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddRole.Click
        Try
            lblerrorr_rolelst.Text = ""
            roleErrLbl.Text = ""
            lblError_BU.Text = ""

            lblAction.Text = "ADDROLES"
            lblroleslst.Visible = False
            rdRolesLst.Visible = False
            HideShowPanels(False)
            lblRoleID.Visible = False
            lblVRoleID.Visible = False
            RdbxBU.Visible = False
            rcbBU.Visible = True
            lblPortal.Text = rblType.SelectedItem.Value.ToString
            lbLNodeValidation.Text = ""
            chkboxApplyallSisterBU.Checked = False
            hdnRoleID.Value = 0
            hdnRoleName.Value = "0"
            roleErrLbl.Text = ""
            If rblType.SelectedItem.Value = "Customer" Then
                GetProgramData(rblType.SelectedItem.Value)
                GetRoleData(m_cNewRole)
                chkboxApplyallSisterBU.Visible = True
                rcbBU.Visible = True
                If Session("USERTYPEVALUE") = "S" Then
                    lblUserType.Visible = True
                    radioUserType.Visible = True
                    radioUserType.SelectedValue = "Customer"
                Else
                    lblUserType.Visible = False
                    radioUserType.Visible = False
                End If
            Else
                GetProgramData(rblType.SelectedItem.Value)
                GetRoleData(m_cNewRole)
                Dim Option1 As RadComboBoxItem = New RadComboBoxItem()
                Option1.Text = "ISA00"
                Option1.Value = "ISA00"
                Dim Option2 As RadComboBoxItem = New RadComboBoxItem()
                Option2.Text = "SDM00"
                Option2.Value = "SDM00"
                rcbBU.Items.Clear()
                rcbBU.Items.Add(Option1)
                rcbBU.Items.Add(Option2)
                rcbBU.Visible = True
                chkboxApplyallSisterBU.Visible = False
                radioUserType.Visible = False
                lblUserType.Visible = False
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub btnCpyRole_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCpyRole.Click
        Try            
            lblerrorr_rolelst.Text = ""
            roleErrLbl.Text = ""
            lblError_BU.Text = ""

            lblPortal.Text = rblType.SelectedItem.Value.ToString
            lblroleslst.Visible = True
            rdRolesLst.Visible = True
            RdbxBU.Visible = True
            rcbBU.Visible = False
            lblPortalHdr.Visible = True
            lblAction.Text = "COPYROLES"
            txtRoleDefaultPage.Text = ""
            Try
                Dim ds As DataSet
                ds = Session("ROLES_DS")
                Dim dt As DataTable
                If Not ds Is Nothing Then
                    If ds.Tables(0).Rows.Count() > 0 Then
                        If Trim(lblPortal.Text) = "Customer" Then
                            dt = ds.Tables(0).AsEnumerable().Where(Function(r) r.Field(Of String)("ROLETYPE") <> "Vendor").CopyToDataTable()
                        Else
                            dt = ds.Tables(0).AsEnumerable().Where(Function(r) r.Field(Of String)("ROLETYPE") = "Vendor").CopyToDataTable()
                        End If
                    End If
                End If
                rdRolesLst.DataSource = dt
                rdRolesLst.DataTextField = "ROLES"
                rdRolesLst.DataValueField = "ROLENUM"
                rdRolesLst.DataBind()
                rdRolesLst.Items.Insert(0, New RadComboBoxItem("Select Roles", "0"))
                rdRolesLst.DataValueField.Insert(0, "0")                
            Catch ex As Exception

            End Try
            HideShowPanels(False)
            lblRoleID.Visible = False
            lblVRoleID.Visible = False

            lbLNodeValidation.Text = ""
            chkboxApplyallSisterBU.Checked = False
            hdnRoleID.Value = 0
            hdnRoleName.Value = "0"
            roleErrLbl.Text = ""
            If rblType.SelectedItem.Value = "Customer" Then
                GetProgramData(rblType.SelectedItem.Value)
                GetRoleData(m_cNewRole)
                chkboxApplyallSisterBU.Visible = True
                If Session("USERTYPEVALUE") = "S" Then
                    lblUserType.Visible = True
                    radioUserType.Visible = True
                    radioUserType.SelectedValue = "Customer"
                Else
                    lblUserType.Visible = False
                    radioUserType.Visible = False
                End If
            Else
                GetProgramData(rblType.SelectedItem.Value)
                GetRoleData(m_cNewRole)
                Dim Option1 As RadComboBoxItem = New RadComboBoxItem()
                Option1.Text = "ISA00"
                Option1.Value = "ISA00"
                Dim Option2 As RadComboBoxItem = New RadComboBoxItem()
                Option2.Text = "SDM00"
                Option2.Value = "SDM00"
                RdbxBU.Items.Clear()
                RdbxBU.Items.Add(Option1)
                RdbxBU.Items.Add(Option2)
                RdbxBU.Visible = True
                chkboxApplyallSisterBU.Visible = False
                radioUserType.Visible = False
                lblUserType.Visible = False
            End If

        Catch ex As Exception

        End Try
    End Sub

    Protected Sub txtRole_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtRole.TextChanged

        Dim roleName As String
        Dim txtRoleString As String
        txtRoleString = txtRole.Text
        Dim roleID As New DataSet
        Dim sSQLstring As String

        If lblAction.Text = "COPYROLES" Then
            lblroleslst.Visible = True            
            lblPortalHdr.Visible = True
            If lblPortal.Text = "Vendor" Then
                lblUserType.Visible = False
            Else
                lblUserType.Visible = True
                If Session("USERTYPE") = "CORPADMIN" Then
                    lblUserType.Visible = False
                End If
            End If
            rcbBU.Visible = False
            chkboxApplyallSisterBU.Visible = False
            If rblType.SelectedValue = "SDI" Then

            End If
            If radioUserType.SelectedValue = "Customer" Then
                chkboxApplyallSisterBU.Visible = True
            ElseIf radioUserType.SelectedValue = "SDI" Then
                chkboxApplyallSisterBU.Visible = False
            Else
                chkboxApplyallSisterBU.Visible = False
            End If
            If lblPortal.Text = "Vendor" Then
                chkboxApplyallSisterBU.Visible = False
            End If

            If radioUserType.SelectedValue = "SDI" Then
                chkboxApplyallSisterBU.Visible = False
                lblBU.Visible = False
            End If
            If RdbxBU.CheckedItems.Count() > 0 Then
                Dim buError As List(Of String) = New List(Of String)()
                For Each item As RadComboBoxItem In RdbxBU.CheckedItems
                    sSQLstring = "Select ROLENAME from SDIX_USRROLE_TBL Where UPPER(ROLENAME)= '" + txtRoleString.ToUpper() + "' AND BUSINESS_UNIT = '" + item.Value + "'"
                    roleName = ORDBData.GetScalar(sSQLstring)
                    If String.IsNullOrEmpty(roleName) Then
                        roleErrLbl.Text = ""
                    Else
                        roleErrLbl.Text = "Role name already exists."
                        buError.Add(item.Value)
                    End If
                Next
                If buError.Count() > 0 Then
                    'roleErrLbl.Text = "Role name already exists for Business units " & String.Join(",", buError)
                    roleErrLbl.Text = "Role name already exists."
                    Exit Sub
                Else
                    roleErrLbl.Text = ""
                End If
            End If
            If Trim(txtRole.Text) <> "" Then
                roleErrLbl.Text = ""
            End If
            If Session("USERTYPEVALUE") = "C" Then
                lblUserType.Visible = False
            End If
        Else
            sSQLstring = "Select ROLENAME from SDIX_USRROLE_TBL Where ROLENAME= '" & txtRoleString & "'"
            roleName = ORDBData.GetScalar(sSQLstring)

            If String.IsNullOrEmpty(roleName) Then
                roleErrLbl.Text = ""
            Else
                roleErrLbl.Text = "Role name already exists."
            End If
            If Session("USERTYPEVALUE") = "S" Then
                lblUserType.Visible = True
                radioUserType.Visible = True
                If radioUserType.SelectedValue = "SDI" Then
                    rcbBU.Visible = False
                    lblBU.Visible = False
                    chkboxApplyallSisterBU.Visible = False
                Else
                    rcbBU.Visible = True
                    lblBU.Visible = True
                    chkboxApplyallSisterBU.Visible = True
                End If
            Else
                lblUserType.Visible = False
                radioUserType.Visible = False
            End If
            If lblPortal.Text = "Vendor" Then
                radioUserType.Visible = False
                lblUserType.Visible = False
                chkboxApplyallSisterBU.Visible = False
                rcbBU.Visible = True
                lblBU.Visible = True
            End If
        End If
        lblPortalHdr.Visible = True
    End Sub

    Private Function IsMissingMonikers(dtMissingMonikers As DataTable) As Boolean
        Try
            dtMissingMonikers.Columns.Add("ProductName", GetType(String))
            Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.CheckedNodes
            For Each node As RadTreeNode In nodeCollection
                'node.CheckState could also be TreeNodeCheckState.Indeterminate at this point
                'so make sure it's TreeNodeCheckState.Checked.
                If node.CheckState = TreeNodeCheckState.Checked Then
                    Dim values As String() = {node.Value}
                    If String.IsNullOrWhiteSpace(values(0)) Then
                        dtMissingMonikers.Rows.Add(node.FullPath)
                    End If
                End If
            Next
            If dtMissingMonikers.Rows.Count > 0 Then
                IsMissingMonikers = True
            Else
                IsMissingMonikers = False
            End If
        Catch ex As Exception
            lbLNodeValidation.Text = ex.Message
        End Try
    End Function

    Private Sub BuildGroupList()
        Dim strSQLString As String

        If Session("USERTYPEVALUE") = "C" Then
            Dim dsBUSisterSites As DataSet = UnilogORDBData.SisterBusinessUnits(Session("BUSUNIT"))

            rcbBU.DataSource = dsBUSisterSites
            rcbBU.DataValueField = "BUSINESS_UNIT"
            rcbBU.DataTextField = "DESCRIPTION"
            rcbBU.DataBind()

            RdbxBU.DataSource = dsBUSisterSites
            RdbxBU.DataValueField = "BUSINESS_UNIT"
            RdbxBU.DataTextField = "DESCRIPTION"
            RdbxBU.DataBind()
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

            Dim dsGroups As DataSet = ORDBData.GetAdapter(strSQLString)

            rcbBU.DataSource = dsGroups
            rcbBU.DataValueField = "groupid"
            rcbBU.DataTextField = "groupname"
            rcbBU.DataBind()

            RdbxBU.DataSource = dsGroups
            RdbxBU.DataValueField = "groupid"
            RdbxBU.DataTextField = "groupname"
            RdbxBU.DataBind()
        End If
    End Sub

    Private Sub btnCollapseAll_Click(sender As Object, e As System.EventArgs) Handles btnCollapseAll.Click
        rtvPrograms.CollapseAllNodes()
        If lblAction.Text = "COPYROLES" Then
            If lblPortal.Text = "Customer" Or lblPortal.Text = "SDI" Then
                If radioUserType.SelectedValue = "Customer" Then
                    lblroleslst.Visible = True
                    lblUserType.Visible = True
                Else
                    lblroleslst.Visible = True
                    lblUserType.Visible = True
                    lblBU.Visible = False
                End If
            Else
                lblroleslst.Visible = True
                chkboxApplyallSisterBU.Visible = False
            End If
            If Session("USERTYPEVALUE") = "C" Then
                lblUserType.Visible = False
            End If
        Else
            If lblPortal.Text = "Customer" Or lblPortal.Text = "SDI" Then
                If radioUserType.SelectedValue = "Customer" Then
                    lblUserType.Visible = True

                Else
                    lblUserType.Visible = True
                    lblBU.Visible = False
                End If
            Else

            End If
            If Session("USERTYPEVALUE") = "C" Then
                lblUserType.Visible = False
            End If
        End If
    End Sub

    Private Sub btnExpandAll_Click(sender As Object, e As System.EventArgs) Handles btnExpandAll.Click
        rtvPrograms.ExpandAllNodes()
        If lblAction.Text = "COPYROLES" Then
            If lblPortal.Text = "Customer" Or lblPortal.Text = "SDI" Then
                If radioUserType.SelectedValue = "Customer" Then
                    lblroleslst.Visible = True
                    lblUserType.Visible = True
                Else
                    lblroleslst.Visible = True
                    lblUserType.Visible = True
                    lblBU.Visible = False
                End If
            Else
                lblroleslst.Visible = True
                chkboxApplyallSisterBU.Visible = False
            End If
            If Session("USERTYPEVALUE") = "C" Then
                lblUserType.Visible = False
            End If
        Else
            If lblPortal.Text = "Customer" Or lblPortal.Text = "SDI" Then
                If radioUserType.SelectedValue = "Customer" Then
                    lblUserType.Visible = True

                Else
                    lblUserType.Visible = True
                    lblBU.Visible = False
                End If
            Else

            End If
            If Session("USERTYPEVALUE") = "C" Then
                lblUserType.Visible = False
            End If
        End If
    End Sub
#End Region


    Private Sub rtvPrograms_NodeDataBound(sender As Object, e As Telerik.Web.UI.RadTreeNodeEventArgs) Handles rtvPrograms.NodeDataBound

        If e.Node.DataItem("active").ToString() <> "1" Then
            e.Node.Enabled = False
        ElseIf e.Node.DataItem("securityalias").ToString.Trim.Length = 0 Then
            e.Node.Enabled = False
            'ElseIf e.Node.DataItem("access_group").ToString() = "SDI" Then
            '    e.Node.Visible = False
        End If
        If e.Node.DataItem("securityalias").ToString().ToUpper() = GetPrivilegeMoniker(UserPrivsEnum.Home) Then
            'e.Node.Selected = True
            e.Node.Enabled = False
        End If
    End Sub

    Private Sub AddNewRole(txtRoleString As String, sBusinessUnit As String, sRoleType As String)
        Dim sRoleID As String
        Dim sSQLstring As String
        Dim iRoleID As Integer

        ' Insert role name
        sSQLstring = "Insert into SDIX_USRROLE_TBL(ROLENAME,BUSINESS_UNIT,ROLETYPE,ROLEPAGE) values ('" & txtRoleString & "','" & sBusinessUnit & "','" & sRoleType & "','" & txtRoleDefaultPage.Text & "')"
        ORDBData.ExecNonQuery(sSQLstring)

        ' Get the auto-generated role ID
        sSQLstring = "Select ROLENUM FROM SDIX_USRROLE_TBL WHERE ROLENAME = '" & txtRoleString & "' AND BUSINESS_UNIT = '" & sBusinessUnit & "'"
        sRoleID = ORDBData.GetScalar(sSQLstring)
        Try
            iRoleID = CType(sRoleID, Integer)

            ' Insert role details
            Dim dtChildNodes As New DataTable
            dtChildNodes.Clear()
            dtChildNodes.Columns.Add("ProductName", GetType(String))
            Dim nodeCollection As IList(Of RadTreeNode) = rtvPrograms.CheckedNodes
            For Each node As RadTreeNode In nodeCollection
                'Need to save where node.CheckState is TreeNodeCheckState.Indeterminate
                'because it is a parent of a child node that was checked.
                'We need the parent nodes for the root level of the menu we build.
                'If node.CheckState = TreeNodeCheckState.Checked Then
                Dim values As String() = {node.Value}
                If Not String.IsNullOrWhiteSpace(values(0)) Then
                    dtChildNodes.Rows.Add(values(0))
                End If
            Next
            SubmitRoleChilddt(iRoleID, dtChildNodes)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub UpdateExistingRole(txtRoleString As String, sBusinessUnit As String, sRoleType As String)
        roleErrLbl.Text = " "

        ' Update role name
        Dim strSQLstring As String
        strSQLstring = "update SDIX_USRROLE_TBL set ROLENAME = '" & txtRoleString & "' , BUSINESS_UNIT = '" & sBusinessUnit & "' , ROLEPAGE = '" & txtRoleDefaultPage.Text & "', ROLETYPE = '" & sRoleType & "' where  ROLENUM = " & hdnRoleID.Value
        Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSQLstring)

        ' Delete existing role details
        DeleteRoleChild(hdnRoleID.Value)

        ' Insert new role details
        Dim dtChildNodes As New DataTable
        dtChildNodes.Clear()
        dtChildNodes.Columns.Add("ProductName", GetType(String))

        GetCheckedNodes(rtvPrograms.Nodes, dtChildNodes)
        SubmitRoleChilddt(hdnRoleID.Value, dtChildNodes)
    End Sub

    Private Sub UpdateExistingRole_WhileUpdatingForSisterBU(txtRoleString As String, sBusinessUnit As String, sRoleType As String, ByVal sRoleId As String)
        roleErrLbl.Text = " "

        ' Update role name
        Dim strSQLstring As String
        strSQLstring = "update SDIX_USRROLE_TBL set ROLENAME = '" & txtRoleString & "' , BUSINESS_UNIT = '" & sBusinessUnit & "' , ROLEPAGE = '" & txtRoleDefaultPage.Text & "' where  ROLENUM = " & sRoleId
        Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSQLstring)

        ' Delete existing role details
        DeleteRoleChild(sRoleId)

        ' Insert new role details
        Dim dtChildNodes As New DataTable
        dtChildNodes.Clear()
        dtChildNodes.Columns.Add("ProductName", GetType(String))

        GetCheckedNodes(rtvPrograms.Nodes, dtChildNodes)
        SubmitRoleChilddt(sRoleId, dtChildNodes)
    End Sub

    Private Sub UpdateRoleWithProgramMasterChanges(iRoleID As Integer, dsRolePrograms As DataSet, sPortal As String)
        'Dim bUpdateRoleDetails As Boolean = False

        ' Nodes currently stored
        Dim dsRoleData As DataSet
        dsRoleData = GetRoleDetails(iRoleID)

        ' Nodes in nav tree
        Dim iNumCheckedNodes As Integer = 0
        Dim bUpdateRoleDetails As Boolean = False
        IsRoleDetailsChanged(rtvPrograms.Nodes, iNumCheckedNodes, dsRoleData, bUpdateRoleDetails)

        If Not bUpdateRoleDetails Then
            If dsRoleData.Tables(0).Rows.Count <> iNumCheckedNodes Then
                bUpdateRoleDetails = True
            End If
        End If

        If bUpdateRoleDetails Then
            Dim sRoleType As String = rblType.SelectedItem.Value.ToString
            Dim txtRoleString As String = txtRole.Text.ToUpper
            Dim sBusinessUnit As String = rcbBU.SelectedValue

            UpdateExistingRole(txtRoleString, sBusinessUnit, sRoleType)
        End If
    End Sub

    Private Sub IsRoleDetailsChanged(nodes As RadTreeNodeCollection, ByRef iNumCheckedNodes As Integer, dsRoleData As DataSet, _
                                          ByRef bUpdateRoleDetails As Boolean)
        Dim iNodeIndex As Integer = 0
        While iNodeIndex < nodes.Count And Not bUpdateRoleDetails
            Dim node As RadTreeNode = nodes(iNodeIndex)

            If node.CheckState <> TreeNodeCheckState.Unchecked Then 'ensures we get "checked" and "indeteminate" check states
                iNumCheckedNodes = iNumCheckedNodes + 1
                Dim savedNode() As DataRow = dsRoleData.Tables(0).Select("alias_name = '" & node.Value & "'")
                If savedNode.Length = 0 Then
                    bUpdateRoleDetails = True
                End If
            End If

            If node.Nodes.Count > 0 And Not bUpdateRoleDetails Then
                IsRoleDetailsChanged(node.Nodes, iNumCheckedNodes, dsRoleData, bUpdateRoleDetails)
            End If

            iNodeIndex = iNodeIndex + 1
        End While
    End Sub

    Private Sub GetCheckedNodes(nodes As RadTreeNodeCollection, ByRef dtChildNodes As DataTable)
        For Each node As RadTreeNode In nodes
            If node.CheckState <> TreeNodeCheckState.Unchecked Then 'ensures we get "checked" and "indeteminate" check states
                dtChildNodes.Rows.Add(node.Value)
            End If

            If node.Nodes.Count > 0 Then
                GetCheckedNodes(node.Nodes, dtChildNodes)
            End If
        Next
    End Sub

    Private Function GetRoles() As DataSet
        Dim dsRoleData As DataSet

        Try

            Dim strSQLstring As String
            If Session("USERTYPEVALUE") = "C" Then
                ''Will get only the Current & Sister Sites BU data's for CORPORATE ADMIN LOGIN
                strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT, ROLETYPE, ROLEPAGE,ROLENAME || ' - ' || BUSINESS_UNIT as Roles From SDIX_Usrrole_Tbl " & vbCrLf & _
                "WHERE BUSINESS_UNIT IN (SELECT Distinct ClientSite.SITE_ID AS groupid FROM Bidwadmin.dw_client_site_loc ClientSite " & vbCrLf & _
                "JOIN PS_BUS_UNIT_TBL_FS ClentSiteDesc ON ClientSite.SITE_ID=ClentSiteDesc.business_unit WHERE CLIENT =(SELECT Distinct CLIENT FROM Bidwadmin.dw_client_site_loc " & vbCrLf & _
                "WHERE SITE_ID= '" & Session("BUSUNIT") & "' AND site_status = 'Active' AND client != 'Inactive Client Sites') AND SITE_STATUS='Active' AND ClientSite.SITE_ID NOT IN ('ISA00','I0005')) ORDER BY ROLENUM DESC"

                dsRoleData = ORDBData.GetAdapter(strSQLstring)

            Else
                strSQLstring = "Select ROLENUM, ROLENAME, BUSINESS_UNIT, ROLETYPE, ROLEPAGE, ROLENAME || ' - ' || BUSINESS_UNIT as Roles From SDIX_Usrrole_Tbl ORDER BY ROLENUM DESC"
                dsRoleData = ORDBData.GetAdapter(strSQLstring)
            End If
            
        Catch ex As Exception

        End Try
        Return dsRoleData
    End Function

    Private Function GetRoleType(ByVal iRoleID As Integer) As String
        Dim sPortal As String = ""
        Dim sSQLstring As String
        Try
            sSQLstring = "Select ROLETYPE FROM SDIX_USRROLE_TBL WHERE ROLENUM = " & iRoleID
            sPortal = ORDBData.GetScalar(sSQLstring)
        Catch objException As Exception

        End Try

        Return sPortal
    End Function

    Private Sub DeleteRoleChild(ByVal iRoleNum As Integer)
        Dim sSQLstring As String = ""
        sSQLstring = "Delete from SDIX_ROLEDETAIL where ROLENUM = " & iRoleNum
        Dim rowsAffected As Integer = 0

        rowsAffected = ORDBData.ExecNonQueryWithTransaction(sSQLstring)

    End Sub

    Private Sub SubmitRoleChilddt(ByVal iRoleID As Integer, ByVal dt As DataTable)
        Dim strSQLstring As String

        Try
            If iRoleID > 0 Then
                If dt.Rows.Count > 0 Then
                    For Each row As DataRow In dt.Rows
                        strSQLstring = "Insert into SDIX_ROLEDETAIL(ROLENUM,ALIAS_NAME) values (" & iRoleID & ",'" & row.Item("ProductName").ToString().Replace("'", "''") & "')"
                        ORDBData.ExecNonQuery(strSQLstring)
                    Next
                End If
            End If
        Catch objException As Exception
        End Try

    End Sub

    Private Sub GetUserDatas()
        Try
            Dim SDIUserId As String = Session("USERID")
            Dim SQLSTRINGQuery As String = "Select * from SDIX_USERS_TBL where ISA_EMPLOYEE_ID = '" & SDIUserId & "'"
            Dim dsOREmp As DataSet = ORDBData.GetAdapter(SQLSTRINGQuery)

            If dsOREmp.Tables(0).Rows.Count() > 0 Then
                Session("USERTYPE") = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_ACTYP")
                Session("USERTYPEVALUE") = dsOREmp.Tables(0).Rows(0).Item("ISA_SDI_EMPLOYEE")
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Function GetUserMenuFromRole(dsRolePrograms As DataSet, sPortal As String) As DataSet
        Dim dsUserMenu As DataSet
        Dim strSQLstring As String

        If dsRolePrograms.Tables(0).Rows.Count > 0 Then
            Dim sSecurityAliases As String = ""

            For Each dr As DataRow In dsRolePrograms.Tables(0).Rows
                If sSecurityAliases.Length > 0 Then
                    sSecurityAliases &= ","
                End If
                sSecurityAliases &= "'" & dr.Item("alias_name") & "'"
            Next

            strSQLstring = "SELECT ISA_IDENTIFIER, PROGRAMNAME, nullif(ISA_PARENT_IDENT, 0) ISA_PARENT_IDENT, SECURITYALIAS, ISA_NAVIGATIONURL, PARMLIST, " & vbCrLf & _
                " ' ' AS FULLPATH " & vbCrLf & _
                " FROM SDIX_PRGRMMASTER " & vbCrLf & _
                " WHERE active <> " & clsProgramMaster.InactiveProgramCode & vbCrLf & _
                " AND isa_portal = '" & sPortal & "' AND SECURITYALIAS IN " & vbCrLf & _
                " (" & sSecurityAliases & ")" & vbCrLf & _
                " ORDER BY ACTIVE, ISA_IDENTIFIER"
            dsUserMenu = ORDBData.GetAdapter(strSQLstring)
        End If

        Return dsUserMenu
    End Function

    Protected Sub radioUserType_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            If lblAction.Text = "COPYROLES" Then
                lblroleslst.Visible = True
                lblBU.Visible = True                
                lblPortalHdr.Visible = True
                lblUserType.Visible = True
                If radioUserType.SelectedValue = "Customer" Then
                    RdbxBU.Visible = True
                    lblBU.Visible = True
                    lblError_BU.Text = ""
                    chkboxApplyallSisterBU.Visible = True
                ElseIf radioUserType.SelectedValue = "SDI" Then
                    RdbxBU.Visible = False
                    lblBU.Visible = False
                    lblError_BU.Text = ""
                    chkboxApplyallSisterBU.Visible = False
                Else
                    RdbxBU.Visible = False
                    lblBU.Visible = False
                    lblError_BU.Text = ""
                    chkboxApplyallSisterBU.Visible = False
                End If
            Else
                If radioUserType.SelectedValue = "SDI" Then
                    rcbBU.Visible = False
                    lblBU.Visible = False
                    chkboxApplyallSisterBU.Visible = False
                Else
                    rcbBU.Visible = True
                    lblBU.Visible = True
                    chkboxApplyallSisterBU.Visible = True
                End If
                If Session("USERTYPEVALUE") = "S" Then
                    lblUserType.Visible = True
                    radioUserType.Visible = True
                Else
                    lblUserType.Visible = False
                    radioUserType.Visible = False
                End If
            End If
            
        Catch ex As Exception

        End Try
    End Sub

    Private Sub EnableDisableFields()
        Try
            If radioUserType.SelectedValue = "SDI" Then
                rcbBU.Visible = False
                lblBU.Visible = False
                chkboxApplyallSisterBU.Visible = False
            Else
                rcbBU.Visible = True
                lblBU.Visible = True
                chkboxApplyallSisterBU.Visible = True
            End If
            If Session("USERTYPEVALUE") = "S" Then
                lblUserType.Visible = True
                radioUserType.Visible = True
            Else
                lblUserType.Visible = False
                radioUserType.Visible = False
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub rgRoles_ItemDataBound(sender As Object, e As GridItemEventArgs)
        Try
            If TypeOf e.Item Is GridEditableItem AndAlso e.Item.IsInEditMode Then
                Dim Item As GridEditableItem = (DirectCast(e.Item, GridEditableItem))
                Dim ROLEID = Item.GetDataKeyValue("ROLENUM")
                Dim ROLENAME = Item.GetDataKeyValue("ROLENAME")
                ''Dim ROLEID As String = Item.Item("ROLENUM").Text.Trim()
                hdnRoleID.Value = ROLEID
                hdnRoleName.Value = ROLENAME
                BindRoleDetails(hdnRoleID.Value)
                roleErrLbl.Text = ""
                lblRoleID.Visible = False
                lblVRoleID.Visible = False
                Dim sPortal As String = GetRoleType(hdnRoleID.Value)
                If Session("USERTYPEVALUE") = "S" Then
                    If radioUserType.SelectedValue = "SDI" Then
                        chkboxApplyallSisterBU.Visible = False
                        rcbBU.Visible = False
                        lblBU.Visible = False
                    Else
                        chkboxApplyallSisterBU.Visible = True
                        rcbBU.Visible = True
                        lblBU.Visible = True
                    End If
                Else
                    chkboxApplyallSisterBU.Visible = True
                    rcbBU.Visible = True
                    lblBU.Visible = True
                End If
                GetProgramData(sPortal)
                Dim dsRolePrograms As DataSet = GetRoleData(hdnRoleID.Value)
                UpdateRoleWithProgramMasterChanges(hdnRoleID.Value, dsRolePrograms, sPortal)
                If lblPortal.Text = "SDI" Then
                    lblPortal.Text = "Customer"
                ElseIf lblPortal.Text = "Vendor" Then
                    chkboxApplyallSisterBU.Visible = False
                End If
                Item.Edit = False
                ''rgRoles.MasterTableView.Rebind()                
            ElseIf TypeOf e.Item Is GridDataItem Then
                Dim item As GridDataItem = CType(e.Item, GridDataItem)
                If Trim(item.Item("BUSINESS_UNIT").Text) = "&nbsp;" Or Trim(item.Item("BUSINESS_UNIT").Text) = "" Then
                    item.Item("BUSINESS_UNIT").Text = "-"
                End If
                If Trim(item.Item("ROLEPAGE").Text) = "&nbsp;" Or Trim(item.Item("ROLEPAGE").Text) = "" Then
                    item.Item("ROLEPAGE").Text = "-"
                End If
            Else

            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub rgRoles_NeedDataSource(sender As Object, e As GridNeedDataSourceEventArgs)
        Try
            Dim dsRoleData As DataSet
            dsRoleData = GetRoles()
            rgRoles.DataSource = dsRoleData
            rgRoles.DataBind()
        Catch ex As Exception

        End Try
    End Sub

    Private Function ExistForUser(ByVal RoleID As String) As Boolean
        Dim dsRoleData As DataSet
        Dim ExistsData As Boolean = False
        Try
            Dim chkQuery As String = "SELECT * FROM SDIX_USERS_TBL WHERE ROLENUM = " & RoleID & ""
            dsRoleData = ORDBData.GetAdapter(chkQuery)
            If Not dsRoleData Is Nothing Then
                If dsRoleData.Tables(0).Rows.Count() > 0 Then
                    ExistsData = True
                Else
                    ExistsData = False
                End If
            Else
                ExistsData = False
            End If
        Catch ex As Exception
            ExistsData = False
        End Try
        Return ExistsData
    End Function

    Protected Sub rgRoles_DeleteCommand(sender As Object, e As GridCommandEventArgs)
        Try
            Dim DeleteQuery As String = ""
            Dim DeleteItem As GridEditableItem = (DirectCast(e.Item, GridEditableItem))
            Dim ROLEID As String = DeleteItem.Item("ROLENUM").Text.Trim()

            Dim Exists As Boolean = False
            Exists = ExistForUser(ROLEID)
            Dim rowaffected As Integer = 0
            If Exists = False Then
                DeleteQuery = "DELETE FROM SDIX_USRROLE_TBL WHERE ROLENUM = " + ROLEID + ""
                rowaffected = ORDBData.ExecNonQuery(DeleteQuery)
                DeleteQuery = "DELETE FROM SDIX_ROLEDETAIL WHERE ROLENUM = " + ROLEID + ""
                rowaffected = ORDBData.ExecNonQuery(DeleteQuery)
                If rowaffected > 0 Then
                    lblMsg.Text = "Role name deleted sucessfully."
                    lblMsg.Visible = True
                    clsSDIAudit.AddRecord("RoleMaster.aspx", "Delete Role Name", "SDIX_USRROLE_TBL", Session("USERID").ToString, Session("BUSUNIT").ToString, Session("USERID").ToString, _
                      sUDF1:=DeleteItem.Item("ROLENAME").Text.Trim(), sUDF2:="Portal " & DeleteItem.Item("ROLETYPE").Text.Trim(), sUDF3:="BU " & DeleteItem.Item("BUSINESS_UNIT").Text.Trim())
                Else
                    lblMsg.Text = "Error in deleting the role name."
                    lblMsg.Visible = True
                End If
            Else
                lblMsg.Text = "This Role name is applied to users."
                lblMsg.Visible = True
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub rdRolesLst_SelectedIndexChanged(sender As Object, e As RadComboBoxSelectedIndexChangedEventArgs)
        Try
            Dim strMessage As New Alert
            If e.Value <> "0" Then
                hdnRoleID.Value = e.Value
                BindRoleDetails(e.Value)
                txtRole.Text = ""
                roleErrLbl.Text = ""
                lblRoleID.Visible = False
                lblVRoleID.Visible = False
                lblError_BU.Text = ""
                Dim sPortal As String = GetRoleType(hdnRoleID.Value)
                If Session("USERTYPEVALUE") = "S" Then
                    If radioUserType.SelectedValue = "SDI" Then
                        chkboxApplyallSisterBU.Visible = False
                    Else
                        chkboxApplyallSisterBU.Visible = True
                    End If
                Else
                    chkboxApplyallSisterBU.Visible = True
                End If
                GetProgramData(sPortal)
                Dim dsRolePrograms As DataSet = GetRoleData(hdnRoleID.Value)                
                If lblPortal.Text = "SDI" Then
                    lblPortal.Text = "Customer"
                    lblUserType.Visible = True
                ElseIf lblPortal.Text = "Vendor" Then
                    chkboxApplyallSisterBU.Visible = False
                    lblUserType.Visible = False
                End If
                If lblPortal.Text = "Customer" Then
                    chkboxApplyallSisterBU.Visible = True
                    lblUserType.Visible = True                
                Else
                    chkboxApplyallSisterBU.Visible = False
                End If
                rcbBU.Visible = False
                lblroleslst.Visible = True
                lblPortalHdr.Visible = True

                If radioUserType.SelectedValue = "SDI" Then                    
                    chkboxApplyallSisterBU.Visible = False
                    lblBU.Visible = False
                    RdbxBU.Visible = False
                ElseIf radioUserType.SelectedValue = "Customer" Then
                    chkboxApplyallSisterBU.Visible = True
                    RdbxBU.Visible = True
                End If
                lblerrorr_rolelst.Text = ""
                If lblPortal.Text = "Vendor" Then
                    chkboxApplyallSisterBU.Visible = False
                Else

                End If
                If Session("USERTYPEVALUE") = "C" Then
                    lblUserType.Visible = False
                End If
            Else
                rtvPrograms.ClearCheckedNodes()
                lblroleslst.Visible = True
                txtRoleDefaultPage.Text = ""
                radioUserType.SelectedValue = "Customer"
                If lblPortal.Text = "Customer" Then
                    lblBU.Visible = True
                    RdbxBU.Visible = True
                    rcbBU.Visible = False
                    chkboxApplyallSisterBU.Visible = True
                    lblUserType.Visible = True
                ElseIf lblPortal.Text = "SDI" Then
                    lblBU.Visible = False
                    RdbxBU.Visible = False
                    rcbBU.Visible = False
                    chkboxApplyallSisterBU.Visible = False
                    lblUserType.Visible = True
                Else
                    lblBU.Visible = True
                    RdbxBU.Visible = True
                    rcbBU.Visible = False
                    chkboxApplyallSisterBU.Visible = False
                    lblUserType.Visible = False
                End If
                If Session("USERTYPEVALUE") = "C" Then
                    lblUserType.Visible = False
                End If
            End If
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub chkboxApplyallSisterBU_CheckedChanged(sender As Object, e As EventArgs)
        Try
            Dim strMessage As New Alert
            If lblAction.Text = "COPYROLES" Then
                lblroleslst.Visible = True
                If chkboxApplyallSisterBU.Checked = True Then
                    If RdbxBU.CheckedItems.Count() > 0 Then
                        For Each item As RadComboBoxItem In RdbxBU.CheckedItems
                            Dim dsBUSisterSites As DataSet = UnilogORDBData.SisterBusinessUnits(item.Value)
                            If Not dsBUSisterSites Is Nothing Then
                                If dsBUSisterSites.Tables(0).Rows.Count() > 0 Then
                                    For Each rw As DataRow In dsBUSisterSites.Tables(0).Rows
                                        RdbxBU.FindItemByValue(rw.Item("BUSINESS_UNIT")).Checked = True
                                    Next
                                End If
                            Else

                            End If
                        Next
                    Else
                        chkboxApplyallSisterBU.Checked = False
                        ltlAlert.Text = strMessage.Say("Please select any business units")
                    End If
                Else
                    RdbxBU.ClearCheckedItems()
                End If
                If lblPortal.Text = "Customer" Or lblPortal.Text = "SDI" Then
                    lblUserType.Visible = True
                Else
                    lblUserType.Visible = False
                End If
                If Session("USERTYPEVALUE") = "C" Then
                    lblUserType.Visible = False
                End If
            Else
                If Session("USERTYPEVALUE") = "C" Then
                    lblUserType.Visible = False
                Else
                    lblUserType.Visible = True
                End If
            End If
            

        Catch ex As Exception

        End Try
    End Sub
End Class