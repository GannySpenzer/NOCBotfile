Public Class po

    Public Sub New()

    End Sub

    Public Sub New(ByVal bu As String, ByVal poId As String)
        m_purchasingBU = bu
        m_poId = poId
    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Dim s As String = Me.PurchasingBusinessUnit & "." & Me.POId
            Return (s)
        End Get
    End Property

    Private m_poId As String = ""

    Public Property [POId]() As String
        Get
            Return m_poId
        End Get
        Set(ByVal Value As String)
            m_poId = Value
        End Set
    End Property

    Private m_purchasingBU As String = ""

    Public Property [PurchasingBusinessUnit]() As String
        Get
            Return m_purchasingBU
        End Get
        Set(ByVal Value As String)
            m_purchasingBU = Value
        End Set
    End Property

    Private m_poDate As String = ""

    Public Property PurchaseOrderDate() As String
        Get
            Return m_poDate
        End Get
        Set(ByVal Value As String)
            m_poDate = Value
        End Set
    End Property

    '// collection of poLine object type
    Private m_arrPOLines As New ArrayList

    Public ReadOnly Property POLines() As ArrayList
        Get
            If (m_arrPOLines Is Nothing) Then
                m_arrPOLines = New ArrayList
            End If
            Return m_arrPOLines
        End Get
    End Property

    Public Function GetPOLine(ByVal poLn As poLine) As poLine
        Dim ln As poLine = Nothing
        If Me.POLines.Count > 0 Then
            For Each o As poLine In Me.POLines
                If o.Id = poLn.Id Then
                    ln = o
                    Exit For
                End If
            Next
        End If
        Return (ln)
    End Function

End Class
