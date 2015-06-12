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
                                " A.Business_unit," & vbCrLf & _
                                " A.ISA_EMPLOYEE_ID," & vbCrLf & _
                                " A.ISA_SDI_EMPLOYEE," & vbCrLf & _
                                " A.Active_status," & vbCrLf & _
                                " B.NAME1 " & vbCrLf & _
                                " FROM PS_ISA_USERS_TBL A, ps_customer B , ps_isa_enterprise C, ps_isa_sdr_bu_loc D " & vbCrLf & _
                                " WHERE upper(A.ISA_EMPLOYEE_name) like UPPER('" & Employee_name & "%')" & vbCrLf & _
                                " and C.isa_business_unit = A.Business_unit " & vbCrLf & _
                                " AND C.CUST_id = B.CUST_ID" & vbCrLf & _
                                " AND A.business_unit = D.isa_business_unit" & vbCrLf & _
                                " AND D.bu_status = '1'" & vbCrLf & _
                                " AND D.location = 'L' || SUBSTR(D.isa_business_unit,2) || '-01'"
                '" AND A.active_status <> 'I'" & vbCrLf & _

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
                                " A.Business_unit," & vbCrLf & _
                                " A.ISA_EMPLOYEE_ID," & vbCrLf & _
                                " A.ISA_SDI_EMPLOYEE," & vbCrLf & _
                                " A.Active_status," & vbCrLf & _
                                " B.NAME1 " & vbCrLf & _
                                " FROM PS_ISA_USERS_TBL A, ps_customer B , ps_isa_enterprise C " & vbCrLf & _
                                " WHERE upper(A.ISA_EMPLOYEE_name) like UPPER('" & Employee_name & "%')" & vbCrLf & _
                                " and C.isa_business_unit = A.Business_unit " & vbCrLf & _
                                " AND C.CUST_id = B.CUST_ID" & vbCrLf
                '" AND A.active_status <> 'I'" & vbCrLf & _
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
            Dim objReader As OleDbDataReader
            iLine = 51
            Try
                iLine = 6
                Dim Command1 As New OleDbCommand
                iLine = 7
                Command1 = New OleDbCommand(strSQLstring, connectionDB)
                iLine = 8
                objReader = Command1.ExecuteReader()
                iLine = 11
                Dim bFound As Boolean = False
                Dim bMoreToRead As Boolean = True
                Dim sActiveStatus As String = ""
                Dim sEmailAddressOrig As String = sEmailAddress
                sEmailAddress = sEmailAddress.ToUpper.Trim
                While bMoreToRead And Not bFound
                    iLine = 12
                    If objReader.Read() Then
                        iLine = 120
                        sActiveStatus = objReader.Item("ACTIVE_STATUS")
                        iLine = 121
                        strEmployeeEmail = objReader.Item("ISA_EMPLOYEE_EMAIL").ToString.ToUpper.Trim
                        iLine = 13

                        If sActiveStatus = "I" Then
                            iLine = 130
                            clsLogger.Log_Event("clsUserTbl:New User is inactive; strEmployeeEmail=" & strEmployeeEmail)
                            iLine = 131
                        Else
                            iLine = 14
                            If sEmailAddress.Length > 0 Then
                                iLine = 15
                                If strEmployeeEmail = sEmailAddress Then
                                    iLine = 16
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
                                        bFound = True
                                    End If
                                End If
                            Else
                                iLine = 150
                                bFound = True
                            End If
                            If bFound Then
                                iLine = 17
                                intUniqueUserID = objReader.Item("ISA_USER_ID")
                                iLine = 18
                                strFirstNameSrch = objReader.Item("FIRST_NAME_SRCH")
                                iLine = 19
                                strLastNameSrch = objReader.Item("LAST_NAME_SRCH")
                                iLine = 20
                                strPasswordEncr = objReader.Item("ISA_PASSWORD_ENCR")
                                iLine = 21
                                strBusinessUnit = objReader.Item("BUSINESS_UNIT")
                                iLine = 22
                                strEmployeeName = objReader.Item("ISA_EMPLOYEE_NAME")
                                iLine = 23
                                strPhoneNum = objReader.Item("PHONE_NUM")
                                iLine = 24
                                strBU = objReader.Item("Business_unit")
                                iLine = 25
                                strstatus = objReader.Item("ACTIVE_STATUS")
                                iLine = 26
                                strEmpID = objReader.Item("ISA_EMPLOYEE_ID")
                                iLine = 27
                                strSiteName = objReader.Item("Name1")
                                iLine = 28
                            End If
                        End If
                    Else
                        bMoreToRead = False
                    End If
                End While
                If Not bFound Then
                    bSuccess = False
                    clsLogger.Log_Event("clsUserTbl:New User record for email not found; sEmailAddress=" & sEmailAddressOrig)
                End If
                iLine = 29
                objReader.Close()
                iLine = 30
            Catch objException As Exception
                bSuccess = False
                'connectionDB.Close()
                clsLogger.Log_Event("clsUserTbl:New iLine=" & iLine.ToString & " strSQLString=" & strSQLstring & " ERROR: " & objException.Message)
                If objReader IsNot Nothing Then
                    objReader.Close()
                End If
            End Try
        End If
    End Sub

End Class
