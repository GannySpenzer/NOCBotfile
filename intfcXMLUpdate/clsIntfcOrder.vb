Imports System.Data.OleDb
Public Class clsIntfcOrder

    Private strOrderNo As String
    Public ReadOnly Property OrderNo() As String
        Get
            Return strOrderNo
        End Get
    End Property

    Public Sub New(ByVal strIntfcreqid As String, ByVal strBU As String, ByVal connectOR As OleDbConnection)

        Dim strSQLstring As String

        strSQLstring = "SELECT A.ORDER_NO" & vbCrLf & _
                        " FROM PS_ISA_ORD_INTF_HD A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                        " AND A.ORDER_NO = '" & strIntfcreqid & "'"

        Dim objReaderReq As OleDbDataReader = ORDBAccess.GetReader(strSQLstring, connectOR)
        If objReaderReq.Read() Then
            strOrderNo = objReaderReq.Item("ORDER_NO")
        End If
        objReaderReq.Close()

    End Sub

End Class
