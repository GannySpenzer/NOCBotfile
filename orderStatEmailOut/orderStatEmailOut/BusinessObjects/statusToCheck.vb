Public Class statusToCheck

    '// prevent this class to get instantiated
    Private Sub New()

    End Sub

    Private m_statusId As eOrderStatuses = orderStatEmailOut.eOrderStatuses.Unknown

    Public Property [StatusId]() As Integer
        Get
            Return m_statusId
        End Get
        Set(ByVal Value As Integer)
            m_statusId = Value
        End Set
    End Property

    '// array of myIdDesc object type
    Private m_arrUserEmailPrivs As New ArrayList

    Public ReadOnly Property UserEmailPrivsForStatus() As ArrayList
        Get
            Return m_arrUserEmailPrivs
        End Get
    End Property

    Private m_startDT As DateTime = Now

    Public Property [StartDate]() As DateTime
        Get
            Return m_startDT
        End Get
        Set(ByVal Value As DateTime)
            m_startDT = Value
        End Set
    End Property

    Public Shared Function GetStatusUsingName(ByVal statusName As String) As statusToCheck
        Dim stat As New statusToCheck
        If Not (statusName Is Nothing) Then
            statusName = statusName.Trim.ToUpper
            Select Case statusName
                Case eOrderStatuses.Cancelled.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.Cancelled
                Case eOrderStatuses.Ordered.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.Ordered
                Case eOrderStatuses.PartiallyShipped.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.PartiallyShipped
                Case eOrderStatuses.Picking.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.Picking
                Case eOrderStatuses.ProcessingOrder.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.ProcessingOrder
                Case eOrderStatuses.Saved.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.Saved
                Case eOrderStatuses.Shipped.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.Shipped
                Case eOrderStatuses.Submitted.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.Submitted
                Case eOrderStatuses.WaitingBudgetApproval.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.WaitingBudgetApproval
                Case eOrderStatuses.WaitingOrderApproval.ToString.ToUpper
                    stat.StatusId = eOrderStatuses.WaitingOrderApproval
                Case Else
                    ' ignore
                    '   defaults to Unknown
            End Select
        End If
        Return (stat)
    End Function

End Class
