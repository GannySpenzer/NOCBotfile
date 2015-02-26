Imports EmailOpsStaff.SQLDBData
Imports System.Data.SqlClient

Public Class clsNewTicNO
    Dim FRED As New System.Web.UI.Page
    Private strTickID As Integer
    Public ReadOnly Property TickID() As Integer
        Get
            Return strTickID
        End Get
    End Property
    Public Sub New(ByVal connectSQLPROD As SqlConnection)

        Dim strSQLstring As String = "SELECT max(Magic_Ticket_number) " & vbCrLf & _
                   " FROM Magic_Numbers"
        strTickID = GetSQLScalar(strSQLstring)
        strTickID = strTickID + 1
        strSQLstring = "update Magic_numbers set magic_ticket_number = '" & strTickID & "'"
        Dim rows_affected As Integer = ExecNonQuerySQL(strSQLstring)
        If rows_affected = 0 Then
            'Return rows_affected
            FRED.Response.Write("<Script language='javascript'> alert('Cannot create ticket please call HelpDesk'); </script>")
            FRED.Server.Transfer("MagicIndex.aspx", True)
        End If
    End Sub

End Class
