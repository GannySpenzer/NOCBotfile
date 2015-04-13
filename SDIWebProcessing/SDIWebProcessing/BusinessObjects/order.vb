Public Class order

    Implements IDisposable

    Private m_id As Integer = -1

    Private m_grpOrderNo As String = ""
    Private m_sdiOrderNo As String = ""
    Private m_arrOrderLines As New ArrayList

    Private m_loc As String = ""
    Private m_orderDate As String = ""
    Private m_targetOp As orderLine.eTargetOperation = orderLine.eTargetOperation.UnknownOperation
    Private m_priorityCode As String = ""
    Private m_bu As String = ""
    Private m_sitePrefix As String = ""
    Private m_empId As String = ""
    Private m_status As String = ""

    Private m_siteInfo As clsEnterprise = Nothing


    Public Sub New()

    End Sub

    Public Property [Id]() As Integer
        Get
            Return m_id
        End Get
        Set(ByVal Value As Integer)
            m_id = Value
        End Set
    End Property

    Public Property [OrderNo_Group]() As String
        Get
            Return m_grpOrderNo
        End Get
        Set(ByVal Value As String)
            m_grpOrderNo = Value
        End Set
    End Property

    Public Property [OrderNo_SDI]() As String
        Get
            Return m_sdiOrderNo
        End Get
        Set(ByVal Value As String)
            m_sdiOrderNo = Value
        End Set
    End Property

    Public ReadOnly Property [OrderLines]() As ArrayList
        Get
            If (m_arrOrderLines Is Nothing) Then
                m_arrOrderLines = New ArrayList
            End If
            Return m_arrOrderLines
        End Get
    End Property

    Public Property BusinessUnit() As String
        Get
            Return m_bu
        End Get
        Set(ByVal Value As String)
            m_bu = Value
        End Set
    End Property

    Public Property Location() As String
        Get
            Return m_loc
        End Get
        Set(ByVal Value As String)
            m_loc = Value
        End Set
    End Property

    Public Property PriorityCode() As String
        Get
            Return m_priorityCode
        End Get
        Set(ByVal Value As String)
            m_priorityCode = Value
        End Set
    End Property

    Public Property OrderDate() As String
        Get
            Return m_orderDate
        End Get
        Set(ByVal Value As String)
            m_orderDate = Value
        End Set
    End Property

    Public Property SitePrefix() As String
        Get
            Return m_sitePrefix
        End Get
        Set(ByVal Value As String)
            m_sitePrefix = Value
        End Set
    End Property

    Public Property TargetOperation() As orderLine.eTargetOperation
        Get
            Return m_targetOp
        End Get
        Set(ByVal Value As orderLine.eTargetOperation)
            m_targetOp = Value
        End Set
    End Property

    Public Property [EmployeeId]() As String
        Get
            Return m_empId
        End Get
        'pfd
        Set(ByVal Value As String)
            If Value = " " Or Value = "" Then
                m_empId = Value
            Else
                m_empId = Value
                If m_empId.Length > 8 Then
                    m_empId = m_empId.Substring(0, 8)
                End If
                ''pfd
                End If


        End Set
    End Property

    Public Property SiteInfo() As clsEnterprise
        Get
            Return m_siteInfo
        End Get
        Set(ByVal Value As clsEnterprise)
            m_siteInfo = Value
        End Set
    End Property

    Public Property OrderStatus() As String
        Get
            Return m_status
        End Get
        Set(ByVal Value As String)
            m_status = Value
        End Set
    End Property

    Public Function AddOrderLine(ByVal ordLine As orderLine) As Integer
        Dim idx As Integer = -1
        Dim bIsFound As Boolean = False
        For Each o As orderLine In Me.OrderLines
            If o.OrderLineKey = ordLine.OrderLineKey Then
                bIsFound = True
                Exit For
            End If
        Next
        If Not bIsFound Then
            idx = Me.OrderLines.Add(value:=ordLine)
        End If
        Return idx
    End Function

    Public Function CreateOrderLine(ByVal orderLineNo As Integer) As orderLine
        ' create orderLine instance
        Dim ordLn As New orderLine(ord:=Me, lineNo:=orderLineNo)

        ' inherit parent properties - initial/default values
        ordLn.BusinessUnit = Me.BusinessUnit
        ' - 2009.03.13 do not inherit location property to child
        'ordLn.Location = Me.Location
        ordLn.OrderDate = Me.OrderDate
        ordLn.DueDate = Me.OrderDate
        ordLn.PriorityCode = Me.PriorityCode
        ordLn.TargetOperation = Me.TargetOperation
        ordLn.EmployeeId = Me.EmployeeId
        ' return orderLine object
        Return ordLn
    End Function

#Region " IDisposable Implementation "

    Public Sub Dispose() Implements System.IDisposable.Dispose
        m_siteInfo = Nothing
    End Sub

#End Region

End Class
