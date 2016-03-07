Public Class ReqLine

    ' Private m_ReqID As String = ""

    Public Sub New()

    End Sub
    Private m_oldDate As String = ""
    Private m_newDate As String = ""

    'Public Sub New(ByVal ReqID As String)
    '    m_ReqID = ReqID
    'End Sub
    Public Property [newDate]() As String
        Get
            Return m_newDate
        End Get
        Set(ByVal Value As String)
            m_newDate = Value
        End Set
    End Property
    Public Property [oldDate]() As String
        Get
            Return m_oldDate
        End Get
        Set(ByVal Value As String)
            m_oldDate = Value
        End Set
    End Property

    Public Sub New(ByVal lineNo As Integer)
        'm_ReqID = ReqId
        m_lineNo = lineNo

        'm_EmployeeID()
    End Sub
    Public Sub New(ByVal lineNo As Integer, ByVal newDate As String, ByVal POLineNBR As String, _
                   ByVal Description As String, ByVal ItemID As String, ByVal schedLineNo As String)
        m_lineNo = lineNo
        m_newDate = newDate
        m_POLine_NBR = POLineNBR
        m_Desc = Description
        m_ItemID = ItemID
        m_POLineSched_NBR = schedLineNo
    End Sub
    Public Sub New(ByVal lineNo As Integer, ByVal newDate As String, ByVal POLineNBR As String, _
                  ByVal Description As String, ByVal ItemID As String, ByVal schedLineNo As String, _
                  ByVal PO_BU As String, ByVal PO_ID As String)
        m_lineNo = lineNo
        m_newDate = newDate
        m_POLine_NBR = POLineNBR
        m_Desc = Description
        m_ItemID = ItemID
        m_POLineSched_NBR = schedLineNo
        m_PO_BU = PO_BU
        m_POID = PO_ID
    End Sub
    Public Sub New(ByVal lineNo As Integer, ByVal newDate As String)
        'm_ReqID = ReqId
        m_lineNo = lineNo

        m_newDate = newDate

        'm_EmployeeID()
    End Sub

    'Public Sub New(ByVal ReqId As String, ByVal lineNo As Integer)
    '    m_ReqID = ReqId
    '    m_lineNo = lineNo

    'End Sub

    'Public Property [ReqNo]() As Integer
    '    Get
    '        Return m_ReqID
    '    End Get
    '    Set(ByVal Value As Integer)
    '        m_ReqID = Value
    '    End Set
    'End Property
    Private m_lineNo As Integer = -1

    Public Property [ReqLineNo]() As Integer
        Get
            Return m_lineNo
        End Get
        Set(ByVal Value As Integer)
            m_lineNo = Value
        End Set
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

    Private m_POLine_NBR As String = ""

    Public Property [POLine_NBR]() As String
        Get
            Return m_POLine_NBR
        End Get
        Set(ByVal Value As String)
            m_POLine_NBR = Value
        End Set
    End Property

    Private m_POLineSched_NBR As String = ""

    Public Property [POLineSched_NBR]() As String
        Get
            Return m_POLineSched_NBR
        End Get
        Set(ByVal Value As String)
            m_POLineSched_NBR = Value
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

    Private m_POID As String = ""

    Public Property [POID]() As String
        Get
            Return m_POID
        End Get
        Set(ByVal Value As String)
            m_POID = Value
        End Set
    End Property
End Class
