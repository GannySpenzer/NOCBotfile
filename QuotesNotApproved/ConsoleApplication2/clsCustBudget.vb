Imports System.Data.OleDb
Public Class clsCustBudget

    Private strChargeCodes As String
    Public ReadOnly Property ChargeCodes() As String
        Get
            Return strChargeCodes
        End Get
    End Property

    Public Sub New(ByVal Employee_ID As String, ByVal Business_unit As String)
        Dim strSQLstring As String
        Dim I As Integer

        strSQLstring = "SELECT A.ISA_CUST_CHARGE_CD" & vbCrLf & _
                        " FROM PS_ISA_CUST_BUDGET A" & vbCrLf & _
                        " WHERE A.BUSINESS_UNIT = '" & Business_unit & "'" & vbCrLf & _
                        " AND A.ISA_IOL_APR_ALT = '" & Employee_ID & "'"

        '" AND A.ISA_IOL_APR_EMP_ID = '" & Employee_ID & "'"

        Dim objDS As DataSet = ORDBData.GetAdapter(strSQLstring)
        If objDS.Tables(0).Rows.Count = 0 Then
            strChargeCodes = "'NOCHGCDS'"
        Else
            For I = 0 To objDS.Tables(0).Rows.Count - 1
                If I > 0 Then
                    strChargeCodes = strChargeCodes & ","
                End If
                strChargeCodes = strChargeCodes & "'" & objDS.Tables(0).Rows(I).Item("ISA_CUST_CHARGE_CD") & "'"
            Next
        End If

    End Sub

End Class
