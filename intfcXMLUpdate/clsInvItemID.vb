Imports System.Data.OleDb
Public Class clsInvItemID

    Private strDESCR254 As String
    Public ReadOnly Property DESCR254() As String
        Get
            Return strDESCR254
        End Get
    End Property

    Private strMFGID As String
    Public ReadOnly Property MFGID() As String
        Get
            Return strMFGID
        End Get
    End Property

    Private strMFGITMID As String
    Public ReadOnly Property MFGITMID() As String
        Get
            Return strMFGITMID
        End Get
    End Property

    Private strDESCR As String
    Public ReadOnly Property DESCR() As String
        Get
            Return strDESCR
        End Get
    End Property

    Public Sub New(ByVal strInvItemID As String, ByVal connectOR As OleDbConnection)

        Dim strSQLstring As String
        strSQLstring = "SELECT A.DESCR254, B.MFG_ID, B.MFG_ITM_ID," & vbCrLf & _
                    " C.DESCR" & vbCrLf & _
                    " FROM PS_INV_ITEMS A, PS_ITEM_MFG B, PS_MANUFACTURER C" & vbCrLf & _
                    " WHERE A.EFFDT =" & vbCrLf & _
                    " (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf & _
                    " WHERE A.SETID = A_ED.SETID" & vbCrLf & _
                    " AND A.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
                    " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                    " AND A.SETID = 'MAIN1'" & vbCrLf & _
                    " AND A.INV_ITEM_ID = '" & strInvItemID & "'" & vbCrLf & _
                    " AND A.SETID = B.SETID" & vbCrLf & _
                    " AND A.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
                    " AND B.SETID = C.SETID" & vbCrLf & _
                    " AND B.MFG_ID = C.MFG_ID"

        Dim objReader As OleDbDataReader = ORDBAccess.GetReader(strSQLstring, connectOR)
        If objReader.Read() Then
            strDESCR254 = objReader.Item("DESCR254")
            strMFGID = objReader.Item("MFG_ID")
            strMFGITMID = objReader.Item("MFG_ITM_ID")
            strDESCR = objReader.Item("DESCR")
        End If
        objReader.Close()

    End Sub

End Class
