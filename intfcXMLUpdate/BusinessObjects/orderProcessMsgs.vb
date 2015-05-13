Public Class orderProcessMsgs

    Private m_instanceId As String = ""
    Private m_arr As New ArrayList

    Public Sub New()
        m_instanceId = Guid.NewGuid.ToString
    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Return m_instanceId
        End Get
    End Property

    Public ReadOnly Property [Messages]() As ArrayList
        Get
            If (m_arr Is Nothing) Then
                m_arr = New ArrayList
            End If
            Return m_arr
        End Get
    End Property

    Public Function createMessage(ByVal msg As String, Optional ByVal lvl As System.Diagnostics.TraceLevel = TraceLevel.Verbose) As orderProcessMsg
        Dim oMsg As New orderProcessMsg(msg, lvl)
        Return oMsg
    End Function

    Public Function addMessage(ByVal msg As String, Optional ByVal lvl As System.Diagnostics.TraceLevel = TraceLevel.Verbose) As orderProcessMsg
        Dim oMsg As New orderProcessMsg(msg, lvl)
        m_arr.Add(oMsg)
        Return oMsg
    End Function

End Class


Public Class orderProcessMsg
    Public Sub New(ByVal msg As String)

    End Sub
    Public Sub New(ByVal msg As String, ByVal lvl As System.Diagnostics.TraceLevel)

    End Sub
    Public Property [Level]() As System.Diagnostics.TraceLevel
        Get

        End Get
        Set(ByVal Value As System.Diagnostics.TraceLevel)

        End Set
    End Property
    Public Property [Message]() As String
        Get

        End Get
        Set(ByVal Value As String)

        End Set
    End Property
End Class
