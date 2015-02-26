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

   

    
    Public Sub New(ByVal Employee_name As String, ByVal Business_unit As String, ByVal connectionDB As OleDbConnection)
        Dim strSQLstring As String
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
                        " and C.isa_business_unit = A.Business_unit and" & vbCrLf & _
                        " C.CUST_id = B.CUST_ID" & vbCrLf

        If Not Business_unit = " " Then
            strSQLstring = strSQLstring & " AND A.BUSINESS_UNIT = '" & Business_unit & "'" & vbCrLf
        End If


        Dim Command1 As New OleDbCommand
        Command1 = New OleDbCommand(strSQLstring, connectionDB)

        Dim objReader As OleDbDataReader
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
            strBU = objReader.Item("Business_unit")
            strstatus = objReader.Item("ACTIVE_STATUS")
            strEmpID = objReader.Item("ISA_EMPLOYEE_ID")
            strSiteName = objReader.Item("Name1")


        End If
        objReader.Close()
    End Sub

End Class
