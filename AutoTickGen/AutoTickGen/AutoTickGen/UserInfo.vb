Public Class UserInfo
    Public Sub New(sUserID As String)
        LoadUserInfo(sUserID)
    End Sub

    Private m_bExistsUser As Boolean = False
    Public ReadOnly Property ExistsUser As Boolean
        Get
            Return m_bExistsUser
        End Get
    End Property

    Private m_sEmail As String = " "
    Public ReadOnly Property Email As String
        Get
            Return m_sEmail
        End Get
    End Property

    Private m_sSiteName As String = " "
    Public ReadOnly Property SiteName As String
        Get
            Return m_sSiteName
        End Get
    End Property

    Private m_sBU As String = " "
    Public ReadOnly Property BU As String
        Get
            Return m_sBU
        End Get
    End Property

    Private m_sPhone As String = " "
    Public ReadOnly Property Phone As String
        Get
            Return m_sPhone
        End Get
    End Property

    Private m_sEmployeeName As String
    Public ReadOnly Property EmployeeName As String
        Get
            Return m_sEmployeeName
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
                    "C.ISA_COMPANY_ID " & vbCrLf & _
                          " FROM ps_isa_users_tbl A,  ps_isa_enterprise C" & vbCrLf & _
                          " WHERE UPPER(A.ISA_EMPLOYEE_ID) = '" & sUserID.ToUpper & "'" & vbCrLf & _
                          " AND A.ACTIVE_STATUS = 'A' and" & vbCrLf & _
                          " C.SETID = 'MAIN1' AND " & vbCrLf & _
                          " C.ISA_BUSINESS_UNIT = A.BUSINESS_UNIT" & vbCrLf
            Dim dsOREmp As New DataSet
            dsOREmp = ORDBData.GetAdapter(strSQLString)
            If dsOREmp.Tables(0).Rows.Count = 0 Then
                m_bExistsUser = False
            Else
                m_bExistsUser = True
                m_sEmail = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_EMAIL")

                m_sBU = dsOREmp.Tables(0).Rows(0).Item("BUSINESS_UNIT")
                m_sPhone = dsOREmp.Tables(0).Rows(0).Item("PHONE_NUM")
                m_sEmployeeName = dsOREmp.Tables(0).Rows(0).Item("ISA_EMPLOYEE_NAME")

                m_sSiteName = dsOREmp.Tables(0).Rows(0).Item("ISA_COMPANY_ID")
                m_sSiteName = Replace(m_sSiteName, "'", "")

                m_sFirstName = dsOREmp.Tables(0).Rows(0).Item("FIRST_NAME_SRCH")
                m_sLastName = dsOREmp.Tables(0).Rows(0).Item("LAST_NAME_SRCH")
            End If

        Catch ex As Exception
            m_bExistsUser = False
        End Try
    End Sub

End Class
