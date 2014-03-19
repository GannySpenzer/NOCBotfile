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

        strSQLstring = "SELECT B.ISA_PARENT_IDENT, B.ISA_ORDER_STATUS" & vbCrLf & _
                        " FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                        " AND A.ORDER_NO = '" & strIntfcreqid & "'" & vbCrLf & _
                        " AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                        " AND B.LINE_NBR = '" & strLine & "'"

        Dim objReaderReq As OleDbDataReader = ORDBAccess.GetReader(strSQLstring, connectOR)
        If objReaderReq.Read() Then
            strParentID = objReaderReq.Item("ISA_PARENT_IDENT")
            strLineStatus = objReaderReq.Item("ISA_ORDER_STATUS")
        End If

    End Sub

End Class
