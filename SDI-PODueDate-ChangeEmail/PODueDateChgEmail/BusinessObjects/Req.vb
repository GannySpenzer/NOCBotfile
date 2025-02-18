Public Class REQ

    Public Sub New()

    End Sub
    Public Sub New(ByVal bu As String, ByVal ReqId As String, ByVal EmployeeId As String)
        m_BU = bu
        m_ReqId = ReqId
        m_EmployeeId = EmployeeId
    End Sub
    Public Sub New(ByVal bu As String, ByVal ReqId As String, ByVal EmployeeId As String, ByVal POID As String, ByVal PO_BU As String)
        m_BU = bu
        m_ReqId = ReqId
        m_EmployeeId = EmployeeId
        m_POID = POID
        m_PO_BU = PO_BU
    End Sub
    Public Sub New(ByVal bu As String, ByVal ReqId As String)
        m_BU = bu
        m_ReqId = ReqId
    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Dim s As String = Me.BusinessUnit & "." & Me.ReqId
            Return (s)
        End Get
    End Property

    Private m_POID As String = ""

    Public Property [POID]() As String
        Get
            Return m_POID
        End Get
        Set(ByVal Value As String)
            m_POID = Value
        End Set
    End Property

    Private m_EmployeeId As String = ""

    Public Property [EmployeeId]() As String
        Get
            Return m_EmployeeId
        End Get
        Set(ByVal Value As String)
            m_EmployeeId = Value
        End Set
    End Property

    Private m_EmployeeEmail As String = ""

    Public Property [EmployeeEmail]() As String
        Get
            Return m_EmployeeEmail
        End Get
        Set(ByVal Value As String)
            m_EmployeeEmail = Value
        End Set
    End Property

    Private m_ReqId As String = ""

    Public Property [ReqId]() As String
        Get
            Return m_ReqId
        End Get
        Set(ByVal Value As String)
            m_ReqId = Value
        End Set
    End Property

    Private m_BU As String = ""

    Public Property [BusinessUnit]() As String
        Get
            Return m_BU
        End Get
        Set(ByVal Value As String)
            m_BU = Value
        End Set
    End Property

    Private m_PO_BU As String = ""

    Public Property [POBusinessUnit]() As String
        Get
            Return m_PO_BU
        End Get
        Set(ByVal Value As String)
            m_PO_BU = Value
        End Set
    End Property
    

    '// collection of poLine object type
    Private m_arrReqLines As New ArrayList

    Public ReadOnly Property ReqLines() As ArrayList
        Get
            If (m_arrReqLines Is Nothing) Then
                m_arrReqLines = New ArrayList
            End If
            Return m_arrReqLines
        End Get
    End Property

    Public Function GetReqLine(ByVal ReqLn As ReqLine) As ReqLine
        Dim ln As ReqLine = Nothing
        If Me.ReqLines.Count > 0 Then
            For Each o As ReqLine In Me.ReqLines
                If o.ReqLineNo = ReqLn.ReqLineNo Then
                    ln = o
                    Exit For
                End If
            Next
        End If
        Return (ln)
    End Function


End Class
