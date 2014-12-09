Imports System.Data
Imports System.Data.OleDb


Public Class orderNoMapper

    Public Const UNCC_BUSINESS_UNIT_OM As String = "I0256"

    Public Enum orderNoType As Integer
        unknown = 0
        UNCC = 1
        SDI = 2
    End Enum

    '// will prevent client to create an instance of this class
    Private Sub New()

    End Sub

    Public Shared Function changeToUNCCOrderNo(ByVal orderNo As String) As String
        Dim ret As String = ""
        If Not (orderNo Is Nothing) Then
            orderNo = orderNo.Trim
            If orderNo.Length > 0 Then
                Dim centuryCode As String = Now.ToString(Format:="yyyy").Substring(0, 2)
                Dim bChangeRequired As Boolean = True
                Try
                    bChangeRequired = Not (orderNo.Substring(0, 2) = centuryCode)
                Catch ex As Exception
                    'ignore
                End Try
                If bChangeRequired Then
                    Try
                        ' replace the first 2 digit of the passed order no 
                        '   with century of the current year + the rest of passed order no
                        ret = "" & _
                              centuryCode & _
                              orderNo.Substring(2, (orderNo.Length - 2)) & _
                              ""
                    Catch ex As Exception
                        'ignore
                    End Try
                Else
                    ret = orderNo
                End If
            End If
        End If
        Return ret
    End Function

    Public Shared Function FormatOrderNoToShow(ByVal sdiOrderNo As String, ByVal unccOrderNo As String) As String
        Dim s As String = ""
        Try
            sdiOrderNo = sdiOrderNo.Trim
        Catch ex As Exception
            sdiOrderNo = ""
        End Try
        Try
            unccOrderNo = unccOrderNo.Trim
        Catch ex As Exception
            unccOrderNo = ""
        End Try
        s = sdiOrderNo
        If unccOrderNo.Length > 0 Then
            If Not (sdiOrderNo.ToUpper = unccOrderNo.ToUpper) Then
                s = unccOrderNo & " (" & sdiOrderNo & ")"
            Else
                s = unccOrderNo & " *"  'visual indicator that SDI system still have this UNCC order code
            End If
        End If
        Return s
    End Function

    Public Overloads Shared Function retrieveAllSDIOrderNos(ByVal unccOrderNo As String, ByVal cnString As String) As SDI.UNCC.WorkOrderAdapter.unccOrderNo
        ' ...
    End Function

    Public Overloads Shared Function retrieveAllSDIOrderNos(ByVal unccOrderNo As String, ByVal cn As OleDbConnection) As SDI.UNCC.WorkOrderAdapter.unccOrderNo
        ' ...
    End Function

    Private Shared Function mapToSDIOrderNos(ByVal orderNo As String, ByVal cn As OleDbConnection) As SDI.UNCC.WorkOrderAdapter.unccOrderNo
        Dim o As SDI.UNCC.WorkOrderAdapter.unccOrderNo
        ' ...
        Return o
    End Function

End Class
