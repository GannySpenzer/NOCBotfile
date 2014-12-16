Imports System.Data.OleDb
Public Class clsSDRBULOC

    Private strBusinessUnit As String
    Public ReadOnly Property BusinessUnit() As String
        Get
            Return strBusinessUnit
        End Get
    End Property

    Private strSmallSiteFlag As String
    Public ReadOnly Property SmallSiteFlag() As String
        Get
            Return strSmallSiteFlag
        End Get
    End Property


    Public Sub New(ByVal Location As String, ByVal strLocType As String)
        Dim strSQLstring As String
        If strLocType = "BU" Then
            strSQLstring = "SELECT A.LOCATION, A.DESCR, A.ISA_BUSINESS_UNIT," & vbCrLf & _
            " A.DEPTID, A.ISA_SITE_CODE, A.ISA_SITE_ID," & vbCrLf & _
            " A.BU_STATUS, A.ISA_ASN_SITE, A.ISA_SMALLSITE_FLAG" & vbCrLf & _
            " FROM PS_ISA_SDR_BU_LOC A" & vbCrLf & _
            " WHERE A.ISA_BUSINESS_UNIT = '" & Location & "'" & vbCrLf & _
            " AND ROWNUM < 2"
        Else
            strSQLstring = "SELECT A.LOCATION, A.DESCR, A.ISA_BUSINESS_UNIT," & vbCrLf & _
            " A.DEPTID, A.ISA_SITE_CODE, A.ISA_SITE_ID," & vbCrLf & _
            " A.BU_STATUS, A.ISA_ASN_SITE, A.ISA_SMALLSITE_FLAG" & vbCrLf & _
            " FROM PS_ISA_SDR_BU_LOC A" & vbCrLf & _
            " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
            " AND A.LOCATION = '" & Location & "'"
        End If

        Dim objReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
        If objReader.Read() Then
            strBusinessUnit = objReader.Item("ISA_BUSINESS_UNIT")
            strSmallSiteFlag = objReader.Item("ISA_SMALLSITE_FLAG")
        End If


    End Sub

End Class
