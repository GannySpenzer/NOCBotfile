Public Interface ISDiReport

    Event AssignedPrintedPickedOrderLines(ByVal sender As Object, ByVal e As EventArgs)
    ReadOnly Property PrintedPickedOrderLines() As PrintedPickedOrderLines

End Interface
