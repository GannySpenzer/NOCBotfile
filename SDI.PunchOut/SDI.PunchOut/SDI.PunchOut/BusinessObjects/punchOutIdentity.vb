Public Class punchOutIdentity

    Inherits userCredential

    Private m_agent As String = ""

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByVal Id As String, ByVal pw As String)
        MyBase.New(Id, pw)
    End Sub

    Public Sub New(ByVal Id As String, ByVal pw As String, ByVal domain As String)
        MyBase.New(Id, pw, domain)
    End Sub

    Public Property Agent() As String
        Get
            Return m_agent
        End Get
        Set(ByVal Value As String)
            m_agent = Value
        End Set
    End Property

End Class
