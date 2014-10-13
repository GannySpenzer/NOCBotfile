Public Class demandQty

    Private m_id As String = ""
    Private m_qtyRequested As Integer = 0
    Private m_qtyAllocated As Integer = 0

    Public Sub New()

    End Sub

    Public Sub New(ByVal Id As String, ByVal qtyRequested As Integer, ByVal qtyAllocated As Integer)
        m_id = Id
        m_qtyRequested = qtyRequested
        m_qtyAllocated = qtyAllocated
    End Sub

    Public Property [Id]() As String
        Get
            Return m_id
        End Get
        Set(ByVal Value As String)
            m_id = Value
        End Set
    End Property

    Public Property [QtyRequested]() As Integer
        Get
            Return m_qtyRequested
        End Get
        Set(ByVal Value As Integer)
            m_qtyRequested = Value
        End Set
    End Property

    Public Property [QtyAllocated]() As Integer
        Get
            Return m_qtyAllocated
        End Get
        Set(ByVal Value As Integer)
            m_qtyAllocated = Value
        End Set
    End Property

    Public Shared Function CreateId(ByVal sourceBU As String, _
                                    ByVal orderNo As String, _
                                    ByVal demandSource As String, _
                                    ByVal schedLineNo As Integer, _
                                    ByVal itemId As String) As String
        Dim s As String = "" & _
                          sourceBU & "." & _
                          orderNo & "." & _
                          demandSource & "." & _
                          schedLineNo.ToString(Format:="00000") & "." & _
                          itemId & _
                          ""
        Return (s)
    End Function

    Public Shared Function GetDemandQty(ByVal sourceBU As String, _
                                        ByVal orderNo As String, _
                                        ByVal demandSource As String, _
                                        ByVal schedLineNo As Integer, _
                                        ByVal itemId As String, _
                                        ByVal pathSQLs As String, _
                                        ByVal oraCNstring As String) As demandQty
        Dim o As New demandQty(Id:=demandQty.CreateId(sourceBU, orderNo, demandSource, schedLineNo, itemId), _
                               QtyRequested:=0, _
                               QtyAllocated:=0)

        Dim cn As New OleDb.OleDbConnection(oraCNstring)

        cn.Open()

        If cn.State = ConnectionState.Open Then

            Dim sql As SQLBuilder = Nothing

            sql = New SQLBuilder(myCommon.LoadQuery(pathSQLs & "getQtyReqAndAlloc_InvTransfer_SELECT.sql"))

            sql.Parameters.Add(":KEY_SOURCE_BU", sourceBU)
            sql.Parameters.Add(":KEY_ORDER_NO", orderNo)
            sql.Parameters.Add(":KEY_DEMAND_SOURCE", demandSource)
            sql.Parameters.Add(":KEY_SCHED_LINE_NO", schedLineNo)
            sql.Parameters.Add(":KEY_INV_ITEM_ID", itemId)

            Dim cmd As OleDb.OleDbCommand = cn.CreateCommand

            cmd.CommandType = CommandType.Text
            cmd.CommandText = sql.ToString

            sql = Nothing

            Dim rdr As OleDb.OleDbDataReader = cmd.ExecuteReader

            If Not (rdr Is Nothing) Then
                If rdr.Read Then
                    Try
                        o.QtyRequested = CInt(rdr("QTY_REQUESTED"))
                    Catch ex As Exception
                    End Try
                    Try
                        o.QtyAllocated = CInt(rdr("QTY_ALLOCATED"))
                    Catch ex As Exception
                    End Try
                End If
            End If

            rdr.Close()
            rdr = Nothing

            cmd.Dispose()
            cmd = Nothing

        End If

        cn.Close()
        cn.Dispose()
        cn = Nothing

        Return (o)
    End Function

End Class
