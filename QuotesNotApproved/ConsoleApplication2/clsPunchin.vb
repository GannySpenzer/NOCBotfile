Imports System.Data.OleDb
Public Class clsPunchin

    Private strCUSTID As String
    Public ReadOnly Property CUSTID() As String
        Get
            Return strCUSTID
        End Get
    End Property

    Private strBusinessUnit As String
    Public ReadOnly Property BusinessUnit() As String
        Get
            Return strBusinessUnit
        End Get
    End Property

    Private strPIURL As String
    Public ReadOnly Property PIURL() As String
        Get
            Return strPIURL
        End Get
    End Property

    Private strPIStatus As String
    Public ReadOnly Property PIStatus() As String
        Get
            Return strPIStatus
        End Get
    End Property

    Private strFromDomain As String
    Public ReadOnly Property FromDomain() As String
        Get
            Return strFromDomain
        End Get
    End Property

    Private strSenderDomain As String
    Public ReadOnly Property SenderDomain() As String
        Get
            Return strSenderDomain
        End Get
    End Property

    Private strSenderID As String
    Public ReadOnly Property SenderID() As String
        Get
            Return strSenderID
        End Get
    End Property

    Private strFirstName As String
    Public ReadOnly Property FirstName() As String
        Get
            Return strFirstName
        End Get
    End Property

    Private strLastName As String
    Public ReadOnly Property LastName() As String
        Get
            Return strLastName
        End Get
    End Property

    Private strUserAgent As String
    Public ReadOnly Property UserAgent() As String
        Get
            Return strUserAgent
        End Get
    End Property

    Private strEmail As String
    Public ReadOnly Property Email() As String
        Get
            Return strEmail
        End Get
    End Property


    Private strSharedSecret As String
    Public ReadOnly Property SharedSecret() As String
        Get
            Return strSharedSecret
        End Get
    End Property

    Public Sub New(ByVal strSessionID As String)

        Dim strSQLstring As String
        strSQLstring = "SELECT A.CUST_ID," & vbCrLf & _
                " A.ISA_BUSINESS_UNIT, A.ISA_PI_FROMDOMAIN," & vbCrLf & _
                " A.ISA_PI_SNDERDOMAIN, A.ISA_PI_SENDERID," & vbCrLf & _
                " A.ISA_PI_SHAREDSECRE, A.ISA_PI_USERAGENT," & vbCrLf & _
                " A.FIRST_NAME_SRCH, A.LAST_NAME_SRCH," & vbCrLf & _
                " A.ISA_PI_EMAIL, A.ISA_PI_UNIQUENAME," & vbCrLf & _
                " A.ISA_PI_COSTCENTER, A.ISA_PI_URL," & vbCrLf & _
                " A.ISA_PI_STATUS" & vbCrLf & _
                " FROM PS_ISA_IOL_PUNCHIN A" & vbCrLf & _
                " WHERE A.ISA_PI_IOL_SES_ID = '" & strSessionID & "'"

        Dim objReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
        If objReader.Read() Then
            strCUSTID = objReader.Item("CUST_ID")
            strBusinessUnit = objReader.Item("ISA_BUSINESS_UNIT")
            strPIURL = objReader.Item("ISA_PI_URL")
            strPIStatus = objReader.Item("ISA_PI_STATUS")
            strFromDomain = objReader.Item("ISA_PI_FROMDOMAIN")
            strSenderDomain = objReader.Item("ISA_PI_SNDERDOMAIN")
            strSenderID = objReader.Item("ISA_PI_SENDERID")
            strUserAgent = objReader.Item("ISA_PI_USERAGENT")
            strFirstName = objReader.Item("FIRST_NAME_SRCH")
            strLastName = objReader.Item("LAST_NAME_SRCH")
            strEmail = objReader.Item("ISA_PI_EMAIL")
            strSharedSecret = objReader.Item("ISA_PI_SHAREDSECRE")

        End If
        objReader.Close()
    End Sub
End Class
