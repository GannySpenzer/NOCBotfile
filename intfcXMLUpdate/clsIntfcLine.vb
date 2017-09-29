Imports System.Data.OleDb
Public Class clsIntfcLine
    Private strParentID As String
    Public ReadOnly Property ParentID() As String
        Get
            Return strParentID
        End Get
    End Property

    Private strLineStatus As String
    Public ReadOnly Property LineStatus() As String
        Get
            Return strLineStatus
        End Get
    End Property

    Public Sub New(ByVal strIntfcreqid As String, ByVal strBU As String, ByVal strLine As String, ByVal connectOR As OleDbConnection)

        Dim strSQLstring As String

        strSQLstring = "SELECT B.ORDER_NO, B.ISA_LINE_STATUS" & vbCrLf & _
                        " FROM PS_ISA_ORD_INTF_HD A, PS_ISA_ORD_INTF_LN B" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                        " AND A.ORDER_NO = '" & strIntfcreqid & "'" & vbCrLf & _
                        " AND A.BUSINESS_UNIT_OM = B.BUSINESS_UNIT_OM" & vbCrLf & _
                        " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                        " AND B.ISA_INTFC_LN = '" & strLine & "'"

        Dim objReaderReq As OleDbDataReader = ORDBAccess.GetReader(strSQLstring, connectOR)
        If objReaderReq.Read() Then
            strParentID = objReaderReq.Item("ORDER_NO")
            strLineStatus = objReaderReq.Item("ISA_LINE_STATUS")
        End If

    End Sub

End Class
