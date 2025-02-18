Public Class userCredential

    Implements IDisposable

    Private m_domain As String = ""
    Private m_id As String = ""
    Private m_pw As String = ""

    Public Sub New()

    End Sub

    Public Sub New(ByVal Id As String, ByVal pw As String)
        m_id = Id
        m_pw = pw
    End Sub

    Public Sub New(ByVal Id As String, ByVal pw As String, ByVal domain As String)
        m_domain = domain
        m_id = Id
        m_pw = pw
    End Sub

    Public Property [Id]() As String
        Get
            Return m_id
        End Get
        Set(ByVal Value As String)
            m_id = Value
        End Set
    End Property

    Public Property [Password]() As String
        Get
            Return m_pw
        End Get
        Set(ByVal Value As String)
            m_pw = Value
        End Set
    End Property

    Public Property Domain() As String
        Get
            Return m_domain
        End Get
        Set(ByVal Value As String)
            m_domain = Value
        End Set
    End Property

#Region " IDisposable Implementation "

    Private m_bIsDisposing As Boolean = False

    Public Sub Dispose() Implements System.IDisposable.Dispose
        If Not m_bIsDisposing Then
            m_bIsDisposing = True
            ' dispose here
        End If
    End Sub

#End Region

End Class
