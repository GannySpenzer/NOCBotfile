Imports System.Data.OleDb
Public Class clsUserTbl

    Private intUniqueUserID As String
    Public ReadOnly Property UniqueUserID() As Integer
        Get
            Return intUniqueUserID
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

    Private strIOLAppEmpID As String
    Public ReadOnly Property IOLAppEmpID() As String
        Get
            Return strIOLAppEmpID
        End Get
    End Property

    Private decIOLAppLimit As Decimal
    Public ReadOnly Property IOLAppLimit() As String
        Get
            Return decIOLAppLimit
        End Get
    End Property

    Public Sub New(ByVal Employee_ID As String, ByVal Business_unit As String, ByVal connectionDB As OleDbConnection)
        Dim strSQLstring As String

        strSQLstring = "SELECT A.ISA_USER_ID, A.FIRST_NAME_SRCH," & vbCrLf & _
                        " A.LAST_NAME_SRCH," & vbCrLf & _
                        " A.ISA_PASSWORD_ENCR," & vbCrLf & _
                        " A.BUSINESS_UNIT," & vbCrLf & _
                        " A.ISA_EMPLOYEE_NAME," & vbCrLf & _
                        " A.ISA_EMPLOYEE_EMAIL," & vbCrLf & _
                        " A.PHONE_NUM," & vbCrLf & _
                        " B.ISA_IOL_APR_EMP_ID," & vbCrLf & _
                        " B.ISA_IOL_APR_LIMIT" & vbCrLf & _
                        " FROM PS_ISA_USERS_TBL A," & vbCrLf & _
                        " PS_ISA_USERS_APPRV B" & vbCrLf & _
                        " WHERE A.ISA_EMPLOYEE_ID = '" & Employee_ID & "'" & vbCrLf & _
                        " AND A.ACTIVE_STATUS = 'A'" & vbCrLf
        If Not Business_unit = "" Then
            strSQLstring = strSQLstring & " AND A.BUSINESS_UNIT = '" & Business_unit & "'" & vbCrLf
        End If
        strSQLstring = strSQLstring & _
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)" & vbCrLf & _
                    " AND A.ISA_EMPLOYEE_ID = B.ISA_EMPLOYEE_ID(+)"

        Dim Command1 As New OleDbCommand
        Command1 = New OleDbCommand(strSQLstring, connectionDB)

        Dim objReader As OleDbDataReader = Nothing
        Try
            objReader = Command1.ExecuteReader()
        Catch objException As Exception
            'connectionDB.Close()

        End Try
        If objReader.Read() Then
            intUniqueUserID = objReader.Item("ISA_USER_ID")
            strFirstNameSrch = objReader.Item("FIRST_NAME_SRCH")
            strLastNameSrch = objReader.Item("LAST_NAME_SRCH")
            strPasswordEncr = objReader.Item("ISA_PASSWORD_ENCR")
            strBusinessUnit = objReader.Item("BUSINESS_UNIT")
            strEmployeeName = objReader.Item("ISA_EMPLOYEE_NAME")
            strEmployeeEmail = objReader.Item("ISA_EMPLOYEE_EMAIL")
            strPhoneNum = objReader.Item("PHONE_NUM")
            If IsDBNull(objReader.Item("ISA_IOL_APR_EMP_ID")) Then
                strIOLAppEmpID = "NOAPPRVR"
            Else
                strIOLAppEmpID = objReader.Item("ISA_IOL_APR_EMP_ID")
            End If
            If IsDBNull(objReader.Item("ISA_IOL_APR_LIMIT")) Then
                decIOLAppLimit = 1000000
            Else
                decIOLAppLimit = objReader.Item("ISA_IOL_APR_LIMIT")
            End If
        End If
        objReader.Close()
    End Sub

End Class
