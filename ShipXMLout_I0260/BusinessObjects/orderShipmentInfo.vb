Public Class orderShipmentInfo

    Public Sub New()

    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Return (m_orderNo)
        End Get
    End Property

    Private m_demandSrc As String = ""

    Public Property [DemandSource]() As String
        Get
            Return m_demandSrc
        End Get
        Set(ByVal Value As String)
            m_demandSrc = Value
        End Set
    End Property

    Private m_orderNo As String

    Public Property [OrderNo]() As String
        Get
            Return m_orderNo
        End Get
        Set(ByVal Value As String)
            m_orderNo = Value
        End Set
    End Property

    Public Function IsOrderFullyShipped() As Boolean
        Dim bIsFullyShipped As Boolean = False
        If Me.OrderLines.Count > 0 Then
            bIsFullyShipped = True
            For Each i As orderLineInfo In Me.OrderLines
                If i.LineNo < 1 Or _
                   i.QuantityPicked <= 0 Or _
                   i.ShipDateTime.Trim.Length = 0 Then
                    bIsFullyShipped = False
                    Exit For
                End If
            Next
        Else
            ' no order line, I would say this order is not FULLY shipped
        End If
        Return (bIsFullyShipped)
    End Function

    Public ReadOnly Property [ShipDateTime]() As String
        Get
            Dim s As String = ""
            If Me.OrderLines.Count > 0 Then
                ' get most recent ship d/t for this order
                For Each lne As orderLineInfo In Me.OrderLines
                    If lne.ShipDateTime.Length > 0 Then
                        If IsDate(lne.ShipDateTime) Then
                            If s.Length = 0 Then
                                s = lne.ShipDateTime
                            Else
                                If CDate(s) < CDate(lne.ShipDateTime) Then
                                    s = lne.ShipDateTime
                                End If
                            End If
                        End If
                    End If
                Next
            End If
            Return (s)
        End Get
    End Property

    Private m_shipper As String = ""

    Public Property [Shipper]() As String
        Get
            Return m_shipper
        End Get
        Set(ByVal Value As String)
            m_shipper = Value
        End Set
    End Property

    Private m_bIsFABOrder As Boolean = False
    Public Property [IsFABOrder]() As Boolean
        Get
            Return m_bIsFABOrder
        End Get
        Set(ByVal Value As Boolean)
            m_bIsFABOrder = Value
        End Set
    End Property

    '// collection of containerInfo object type
    Private m_arrContainers As ArrayList = Nothing

    Public ReadOnly Property [Containers]() As ArrayList
        Get
            If (m_arrContainers Is Nothing) Then
                m_arrContainers = New ArrayList
            End If
            Return m_arrContainers
        End Get
    End Property

    Public Function GetContainer(ByVal cntr As containerInfo) As containerInfo
        Dim i As containerInfo = Nothing
        If Me.Containers.Count > 0 Then
            For Each o As containerInfo In Me.Containers
                If o.Id = cntr.Id Then
                    i = o
                    Exit For
                End If
            Next
        End If
        Return (i)
    End Function

    '// collection of orderLineInfo object type
    Private m_arrOrderLines As ArrayList = Nothing

    Public ReadOnly Property [OrderLines]() As ArrayList
        Get
            If (m_arrOrderLines Is Nothing) Then
                m_arrOrderLines = New ArrayList
            End If
            Return m_arrOrderLines
        End Get
    End Property

End Class
