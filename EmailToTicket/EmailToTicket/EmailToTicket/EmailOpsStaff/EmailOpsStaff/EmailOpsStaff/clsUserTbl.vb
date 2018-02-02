Imports System.Data.OleDb
Public Class clsUserTbl

    Private intUniqueUserID As String
    Public ReadOnly Property UniqueUserID() As Integer
        Get
            Return intUniqueUserID
        End Get
    End Property
    Private strBU As String
    Public ReadOnly Property Business_unit() As String
        Get
            Return strBU
        End Get
    End Property

    Private strFirstNameSrch As String
    Public ReadOnly Property FirstNameSrch() As String
        Get
            Return strFirstNameSrch
        End Get
    End Property

    Private strLastNameSrch As String
    Public ReadOnly Property LastNameSrch() As String
        Get
            Return strLastNameSrch
        End Get
    End Property

    Private strPasswordEncr As String
    Public ReadOnly Property PasswordEncr() As String
        Get
            Return strPasswordEncr
        End Get
    End Property

    Private strBusinessUnit As String
    Public ReadOnly Property BusinessUnit() As String
        Get
            Return strBusinessUnit
        End Get
    End Property

    Private strEmployeeName As String
    Public ReadOnly Property EmployeeName() As String
        Get
            Return strEmployeeName
        End Get
    End Property

    Private strEmployeeEmail As String
    Public ReadOnly Property EmployeeEmail() As String
        Get
            Return strEmployeeEmail
        End Get
    End Property

    Private strPhoneNum As String
    Public ReadOnly Property PhoneNum() As String
        Get
            Return strPhoneNum
        End Get
    End Property

    Private strstatus As String
    Public ReadOnly Property ActiveStatus() As String
        Get
            Return strstatus
        End Get
    End Property
    Private strEmpID As String
    Public ReadOnly Property EmployeeID() As String
        Get
            Return strEmpID
        End Get
    End Property
    Private strSiteName As String
    Public ReadOnly Property SiteName() As String
        Get
            Return strSiteName
        End Get
    End Property

   

    
    Public Sub New(ByVal Employee_name As String, ByVal Business_unit As String, ByVal connectionDB As OleDbConnection, _
                   clsLogger As LoggerClass, ByRef bSuccess As Boolean, Optional sEmailAddress As String = " ")
        Dim strSQLstring As String = ""
        Dim iLine As Integer = 0
        bSuccess = True

        Dim bIsEmail As Boolean = False
        If Employee_name.Contains(".") And Employee_name.Contains("@") Then
            bIsEmail = True
        End If

        Try

            If Business_unit = " " Then
                iLine = 1
                strSQLstring = "SELECT A.ISA_USER_ID, A.FIRST_NAME_SRCH," & vbCrLf & _
                                " A.LAST_NAME_SRCH," & vbCrLf & _
                                " A.ISA_PASSWORD_ENCR," & vbCrLf & _
                                " A.BUSINESS_UNIT," & vbCrLf & _
                                " A.ISA_EMPLOYEE_NAME," & vbCrLf & _
                                " A.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                                " A.PHONE_NUM, " & vbCrLf & _
                                " A.ISA_EMPLOYEE_ID," & vbCrLf & _
                                " A.ISA_SDI_EMPLOYEE," & vbCrLf & _
                                " A.Active_status," & vbCrLf & _
                                " B.NAME1 " & vbCrLf & _
                                " FROM PS_ISA_USERS_TBL A, ps_customer B , ps_isa_enterprise C " & vbCrLf
                If bIsEmail Then
                    strSQLstring = strSQLstring & " WHERE upper(A.ISA_EMPLOYEE_EMAIL) like UPPER('" & Employee_name & "%')" & vbCrLf
                Else
                    strSQLstring = strSQLstring & " WHERE upper(A.ISA_EMPLOYEE_name) like UPPER('" & Employee_name & "%')" & vbCrLf
                End If
                strSQLstring = strSQLstring & " and C.isa_business_unit = A.Business_unit " & vbCrLf & _
                                " AND A.active_status <> 'I'" & vbCrLf & _
                                " AND C.CUST_id = B.CUST_ID" & vbCrLf 

                iLine = 2
            Else
                iLine = 3
                strSQLstring = "SELECT A.ISA_USER_ID, A.FIRST_NAME_SRCH," & vbCrLf & _
                                " A.LAST_NAME_SRCH," & vbCrLf & _
                                " A.ISA_PASSWORD_ENCR," & vbCrLf & _
                                " A.BUSINESS_UNIT," & vbCrLf & _
                                " A.ISA_EMPLOYEE_NAME," & vbCrLf & _
                                " A.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                                " A.PHONE_NUM, " & vbCrLf & _
                                " A.ISA_EMPLOYEE_ID," & vbCrLf & _
                                " A.ISA_SDI_EMPLOYEE," & vbCrLf & _
                                " A.Active_status," & vbCrLf & _
                                " B.NAME1 " & vbCrLf & _
                                " FROM PS_ISA_USERS_TBL A, ps_customer B , ps_isa_enterprise C " & vbCrLf
                If bIsEmail Then
                    strSQLstring = strSQLstring & " WHERE UPPER(A.ISA_EMPLOYEE_EMAIL) like UPPER('" & Employee_name & "%')" & vbCrLf
                Else
                    strSQLstring = strSQLstring & " WHERE UPPER(A.ISA_EMPLOYEE_name) like UPPER('" & Employee_name & "%')" & vbCrLf
                End If
                strSQLstring = strSQLstring & " and C.isa_business_unit = A.Business_unit " & vbCrLf & _
                                " AND A.active_status <> 'I'" & vbCrLf & _
                                " AND C.CUST_id = B.CUST_ID" & vbCrLf
                iLine = 4
                strSQLstring = strSQLstring & " AND A.BUSINESS_UNIT = '" & Business_unit & "'" & vbCrLf
                iLine = 5
            End If
        Catch ex As Exception
            bSuccess = False
            Dim sParameters As String = "Employee_name="
            If Employee_name Is Nothing Then
                sParameters &= " is nothing"
            Else
                sParameters &= Employee_name
            End If
            sParameters &= " Business_unit="
            If Business_unit Is Nothing Then
                sParameters &= " is nothing"
            Else
                sParameters &= Business_unit
            End If
            sParameters &= " sEmailAddress="
            If sEmailAddress Is Nothing Then
                sParameters &= " is nothing"
            Else
                sParameters &= sEmailAddress
            End If
            clsLogger.Log_Event("clsUserTbl:New iLine=" & iLine.ToString & " strSQLString=" & strSQLstring & _
                                " sParameters=" & sParameters & " ERROR: " & ex.Message)

        End Try

        If bSuccess Then
            iLine = 50
            Try
                iLine = 6
                Dim Command1 As New OleDbCommand
                iLine = 7
                Command1 = New OleDbCommand(strSQLstring, connectionDB)
                iLine = 8
                Dim ds As New DataSet
                iLine = 9
                Command1.CommandTimeout = 120
                iLine = 10
                If connectionDB.State <> ConnectionState.Open Then
                    connectionDB.Open()
                End If
                iLine = 101
                Dim daUsers As OleDbDataAdapter = New OleDbDataAdapter(Command1)
                iLine = 102
                daUsers.Fill(ds)
                iLine = 103

                ' Get users table records for the user name, BU, etc. 
                ' If only one record is returned, assume it's the correct user.
                ' If more than one record is returned, then do the email address comparison.
                Dim bFound As Boolean = False
                iLine = 105
                Dim iFoundUserIndex As Integer
                iLine = 106
                If ds.Tables(0).Rows.Count > 0 Then
                    iLine = 107
                    Dim sEmailAddressOrig As String = sEmailAddress
                    iLine = 108
                    If ds.Tables(0).Rows.Count = 1 Or bIsEmail Then
                        iLine = 109
                        bFound = True
                        iLine = 1090
                        iFoundUserIndex = 0
                        iLine = 1091
                    Else
                        iLine = 11
                        sEmailAddress = sEmailAddress.ToUpper.Trim
                        iLine = 110
                        If sEmailAddress.Length > 0 Then
                            Dim iRowIndex As Integer = 0
                            While iRowIndex < ds.Tables(0).Rows.Count And Not bFound
                                iLine = 114
                                Dim dr As DataRow = ds.Tables(0).Rows(iRowIndex)
                                iLine = 12
                                strEmployeeEmail = dr.Item("ISA_EMPLOYEE_EMAIL").ToString.ToUpper.Trim
                                iLine = 13

                                If strEmployeeEmail = sEmailAddress Then
                                    iLine = 16
                                    iFoundUserIndex = iRowIndex
                                    bFound = True
                                Else
                                    ' Check if the given email address is somewhere in the email address field.
                                    ' Some users have multiple addresses in the field separated by semicolon.
                                    ' Check for this but be very conservative so we don't inadvertently pick
                                    ' the wrong user.
                                    iLine = 160
                                    Dim sSemiInFrontNoSpace As String = ";" & sEmailAddress
                                    iLine = 161
                                    Dim sSemiInFrontWithSpace As String = "; " & sEmailAddress
                                    iLine = 162
                                    Dim sSemiInBackNoSpace As String = sEmailAddress & ";"
                                    iLine = 163
                                    Dim sSemiInBackWithSpace As String = sEmailAddress & " ;"
                                    iLine = 164
                                    If strEmployeeEmail.IndexOf(sSemiInFrontNoSpace) >= 0 Or _
                                        strEmployeeEmail.IndexOf(sSemiInFrontWithSpace) >= 0 Or _
                                        strEmployeeEmail.IndexOf(sSemiInBackNoSpace) >= 0 Or _
                                        strEmployeeEmail.IndexOf(sSemiInBackWithSpace) >= 0 Then
                                        iLine = 165
                                        iFoundUserIndex = iRowIndex
                                        bFound = True
                                    End If
                                End If

                                iRowIndex = iRowIndex + 1
                            End While
                        End If
                    End If
                    If bFound Then
                        Dim drUser As DataRow = ds.Tables(0).Rows(iFoundUserIndex)
                        iLine = 166
                        strEmployeeEmail = drUser.Item("ISA_EMPLOYEE_EMAIL").ToString.ToUpper.Trim
                        iLine = 17
                        intUniqueUserID = drUser.Item("ISA_USER_ID")
                        iLine = 18
                        strFirstNameSrch = drUser.Item("FIRST_NAME_SRCH")
                        iLine = 19
                        strLastNameSrch = drUser.Item("LAST_NAME_SRCH")
                        iLine = 20
                        strPasswordEncr = drUser.Item("ISA_PASSWORD_ENCR")
                        iLine = 21
                        strBusinessUnit = drUser.Item("BUSINESS_UNIT")
                        iLine = 22
                        strEmployeeName = drUser.Item("ISA_EMPLOYEE_NAME")
                        iLine = 23
                        strPhoneNum = drUser.Item("PHONE_NUM")
                        iLine = 24
                        strBU = drUser.Item("Business_unit")
                        iLine = 25
                        strstatus = drUser.Item("ACTIVE_STATUS")
                        iLine = 26
                        strEmpID = drUser.Item("ISA_EMPLOYEE_ID")
                        iLine = 27
                        strSiteName = drUser.Item("Name1")
                        iLine = 28
                    Else
                        bSuccess = False
                        clsLogger.Log_Event("clsUserTbl:New - User record for email not found; sEmailAddress=" & _
                                            sEmailAddressOrig & " strSQLstring=" & strSQLstring)
                    End If
                    iLine = 29
                Else
                    bSuccess = False
                    Dim sError As String = "clsUserTbl:New - No records found for user. " & vbCrLf & _
                        "strSQLstring=" & strSQLstring
                    clsLogger.Log_Event(sError)
                End If
            Catch objException As Exception
                bSuccess = False
                clsLogger.Log_Event("clsUserTbl:New - iLine=" & iLine.ToString & " strSQLString=" & strSQLstring & " ERROR: " & objException.Message)
            End Try
        End If
    End Sub

End Class
