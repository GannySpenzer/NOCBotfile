Public Class poLine

    Private m_po As po = Nothing

    Public Sub New()

    End Sub

    Public Sub New(ByVal parentPO As po)
        m_po = parentPO
    End Sub

    Public Sub New(ByVal parentPO As po, ByVal lineNo As Integer)
        m_po = parentPO
        m_lineNo = lineNo
    End Sub

    Public Property [ParentPO]() As po
        Get
            Return m_po
        End Get
        Set(ByVal Value As po)
            m_po = Value
        End Set
    End Property

    Public ReadOnly Property [Id]() As String
        Get
            Dim s As String = ""
            If Not (m_po Is Nothing) Then
                s &= m_po.Id & "." & Me.PurchaseOrderLineNo.ToString(Format:="00000")
            Else
                s &= (New po).Id & "." & Me.PurchaseOrderLineNo.ToString(Format:="00000")
            End If
            Return (s)
        End Get
    End Property

    Private m_lineNo As Integer = -1

    Public Property [PurchaseOrderLineNo]() As Integer
        Get
            Return m_lineNo
        End Get
        Set(ByVal Value As Integer)
            m_lineNo = Value
        End Set
    End Property

    '// collection of poLineSched object type
    Private m_arrSched As New ArrayList

    Public ReadOnly Property Schedules() As ArrayList
        Get
            If (m_arrSched Is Nothing) Then
                m_arrSched = New ArrayList
            End If
            Return m_arrSched
        End Get
    End Property

    Private m_ItemID As String = ""


    Public Property [ItemID]() As String
        Get
            Return m_ItemID
        End Get
        Set(ByVal Value As String)
            m_ItemID = Value
        End Set
    End Property

    Private m_Desc As String = ""


    Public Property [Desc]() As String
        Get
            Return m_Desc
        End Get
        Set(ByVal Value As String)
            m_Desc = Value
        End Set
    End Property
End Class
