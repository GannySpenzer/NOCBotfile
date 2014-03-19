Public Class processMsg

    Private m_msg As String = ""
    Private m_lvl As System.Diagnostics.TraceLevel = TraceLevel.Off


    Public Sub New(ByVal msg As String)
        m_msg = msg
    End Sub

    Public Sub New(ByVal msg As String, ByVal lvl As System.Diagnostics.TraceLevel)
        m_msg = msg
        m_lvl = lvl
    End Sub

    Public Property [Level]() As System.Diagnostics.TraceLevel
        Get
            Return m_lvl
        End Get
        Set(ByVal Value As System.Diagnostics.TraceLevel)
            m_lvl = Value
        End Set
    End Property

    Public Property [Message]() As String
        Get
            Return m_msg
        End Get
        Set(ByVal Value As String)
            m_msg = Value
        End Set
    End Property

End Class
