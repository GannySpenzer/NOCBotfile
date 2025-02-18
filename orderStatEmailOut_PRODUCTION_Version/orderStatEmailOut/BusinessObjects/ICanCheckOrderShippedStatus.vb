Public Interface ICanCheckOrderShippedStatus

    Sub checkOrderShippedStatus(ByVal sender As Object, ByVal e As orderShippedStatusEventArgs)
    Sub sendOrderShippedStatusEmail(ByVal sender As Object, ByVal e As orderShippedStatusEventArgs)

End Interface
