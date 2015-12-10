Public Class AssigneeInfo

    Public Sub New(sUserID As String)
        LoadUserInfo(sUserID)
    End Sub

    Private m_bExistsUser As Boolean = False
    Public ReadOnly Property ExistsUser As Boolean
        Get
            Return m_bExistsUser
        End Get
    End Property

    Private m_sEmailUser As String = " "
    Public ReadOnly Property EmailUser As String
        Get
            Return m_sEmailUser
        End Get
    End Property

    Private m_sEmailAssignee As String = " "
    Public ReadOnly Property EmailAssignee As String
        Get
            Return m_sEmailAssignee
        End Get
    End Property

    Private m_sDepartment As String = " "
    Public ReadOnly Property Department As String
        Get
            Return m_sDepartment
        End Get
    End Property

    Private m_sBU As String = " "
    Public ReadOnly Property BU As String
        Get
            Return m_sBU
        End Get
    End Property

    Private m_sSiteName As String = " "
    Public ReadOnly Property SiteName As String
        Get
            Return m_sSiteName
        End Get
    End Property

    Private m_sPhoneUser As String = " "
    Public ReadOnly Property PhoneUser As String
        Get
            Return m_sPhoneUser
        End Get
    End Property

    Private m_sPhoneAssignee As String = " "
    Public ReadOnly Property PhoneAssignee As String
        Get
            Return m_sPhoneAssignee
        End Get
    End Property

    Private m_sAssigneeName As String
    Public ReadOnly Property AssigneeName As String
        Get
            Return m_sAssigneeName
        End Get
    End Property

    Private m_sFirstName As String
    Public ReadOnly Property FirstName As String
        Get
            Return m_sFirstName
        End Get
    End Property

    Private m_sLastName As String
    Public ReadOnly Property LastName As String
        Get
            Return m_sLastName
        End Get
    End Property

    Private Sub LoadUserInfo(sUserID As String)
        Dim strSQLString As String = " "

        Try
            ' Get some data from the SQL Server "assignee" table m_user
            strSQLString = "SELECT " & vbCrLf & _
                " P.s_userid AS DEPT, U.s_phone, U.s_fullName, U.s_firstName, U.s_lastName, U.s_email " & vbCrLf & _
                " FROM m_user U " & vbCrLf & _
                "       , (SELECT n_id, s_userid FROM m_user) P " & vbCrLf & _
                " WHERE U.s_userid = '" & sUserID & "' " & vbCrLf & _
                "       AND " & vbCrLf & _
                "       ( " & vbCrLf & _
                "           (U.n_parentid != 0 AND U.n_parentid = P.n_id) " & vbCrLf & _
                "           OR " & vbCrLf & _
                "           (U.n_parentid = 0 AND U.n_id = P.n_id) " & vbCrLf & _
                "       )"
            Dim dsOREMP As DataSet = SQLDBData.GetSQLAdapterDazzle(strSQLString)

            If dsOREMP.Tables(0).Rows.Count = 0 Then
                ' If no record exists in the SQL Server "assignee" table, 
                ' we indicate that the assignee doesn't exist.
                m_bExistsUser = False
            Else
                ' Load the "assignee" data from SQL Server. The SQL Server
                ' table m_user is what we use in the Ticketing System for now.
                ' The fields we pull data from below are the same ones
                ' we use in the Ticketing System.
                m_sDepartment = dsOREMP.Tables(0).Rows(0).Item("DEPT").ToString
                m_sPhoneAssignee = dsOREMP.Tables(0).Rows(0).Item("s_phone").ToString
                m_sAssigneeName = dsOREMP.Tables(0).Rows(0).Item("s_fullName").ToString
                m_sEmailAssignee = dsOREMP.Tables(0).Rows(0).Item("s_email").ToString

                Dim sAssigneeFirstName As String = dsOREMP.Tables(0).Rows(0).Item("s_firstName").ToString.Trim.ToUpper
                Dim sAssigneeLastName As String = dsOREMP.Tables(0).Rows(0).Item("s_lastName").ToString.Trim.ToUpper

                ' Now get some more data but this time from the Oracle users and enterprise tables
                ' using the assignee ID. These are the same fields we use for the assignee in
                ' the Ticketing System.
                strSQLString = "SELECT " & vbCrLf & _
                        "A.ISA_USER_ID," & vbCrLf & _
                        "A.FIRST_NAME_SRCH," & vbCrLf & _
                        "A.LAST_NAME_SRCH," & vbCrLf & _
                        "A.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                        "A.ISA_EMPLOYEE_ID," & vbCrLf & _
                        "A.PHONE_NUM," & vbCrLf & _
                        "A.ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                        "A.ISA_SDI_EMPLOYEE," & vbCrLf & _
                        "A.BUSINESS_UNIT," & vbCrLf & _
                        "A.ISA_EMPLOYEE_NAME," & vbCrLf & _
                        "C.ISA_COMPANY_ID as Site_Name " & vbCrLf & _
                              " FROM ps_isa_users_tbl A,  ps_isa_enterprise C" & vbCrLf & _
                              " WHERE UPPER(A.ISA_EMPLOYEE_ID) = '" & sUserID.ToUpper & "'" & vbCrLf & _
                              " AND A.ACTIVE_STATUS = 'A' and" & vbCrLf & _
                              " C.SETID = 'MAIN1' AND " & vbCrLf & _
                              " C.ISA_BUSINESS_UNIT = A.BUSINESS_UNIT" & vbCrLf

                dsOREMP = ORDBData.GetAdapter(strSQLString)
                If dsOREMP.Tables(0).Rows.Count = 0 Then
                    ' If the additional Oracle data doesn't exist for the specific
                    ' assignee ID, try a search using the first name and last name.
                    ' We do this because some assignee IDs, like NetCom for example, 
                    ' have a user ID of NETCOM in the SQL Server table m_user but have
                    ' a user ID of NETCOM1 in the Oracle users data. Not sure why but
                    ' we need this extra step to find the additional data.
                    strSQLString = "SELECT " & vbCrLf & _
                            "A.ISA_USER_ID," & vbCrLf & _
                            "A.FIRST_NAME_SRCH," & vbCrLf & _
                            "A.LAST_NAME_SRCH," & vbCrLf & _
                            "A.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                            "A.ISA_EMPLOYEE_ID," & vbCrLf & _
                            "A.PHONE_NUM," & vbCrLf & _
                            "A.ISA_EMPLOYEE_ACTYP," & vbCrLf & _
                            "A.ISA_SDI_EMPLOYEE," & vbCrLf & _
                            "A.BUSINESS_UNIT," & vbCrLf & _
                            "A.ISA_EMPLOYEE_NAME," & vbCrLf & _
                            "C.ISA_COMPANY_ID as Site_Name " & vbCrLf & _
                                  " FROM ps_isa_users_tbl A,  ps_isa_enterprise C" & vbCrLf & _
                                  " WHERE UPPER(A.first_name_srch) = '" & sAssigneeFirstName & "'" & vbCrLf & _
                                  " AND UPPER(A.last_name_srch) = '" & sAssigneeLastName & "'" & vbCrLf & _
                                  " AND A.ACTIVE_STATUS = 'A' and" & vbCrLf & _
                                  " C.SETID = 'MAIN1' AND " & vbCrLf & _
                                  " C.ISA_BUSINESS_UNIT = A.BUSINESS_UNIT" & vbCrLf
                    dsOREMP = ORDBData.GetAdapter(strSQLString)
                End If
                If dsOREMP.Tables(0).Rows.Count = 0 Then
                    ' If the additional Oracle data doesn't exist, we can't complete
                    ' the assignee information so indicate that the assignee doesn't exist.
                    m_bExistsUser = False
                Else
                    m_bExistsUser = True
                    m_sEmailUser = dsOREMP.Tables(0).Rows(0).Item("ISA_EMPLOYEE_EMAIL")
                    m_sBU = dsOREMP.Tables(0).Rows(0).Item("BUSINESS_UNIT")
                    m_sPhoneUser = dsOREMP.Tables(0).Rows(0).Item("PHONE_NUM")
                    m_sSiteName = dsOREMP.Tables(0).Rows(0).Item("SITE_NAME")
                    m_sFirstName = dsOREMP.Tables(0).Rows(0).Item("FIRST_NAME_SRCH")
                    m_sLastName = dsOREMP.Tables(0).Rows(0).Item("LAST_NAME_SRCH")
                End If
            End If

        Catch ex As Exception
            m_bExistsUser = False
        End Try
    End Sub

End Class
