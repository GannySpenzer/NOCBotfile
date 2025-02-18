Public Interface ICanCheckNSTKOrderShippedStatus

    Sub checkNSTKOrderShippedStatus(ByVal sender As Object, ByVal e As orderShippedStatusEventArgs)
    Sub sendNSTKOrderShippedStatusEmail(ByVal sender As Object, ByVal e As orderShippedStatusEventArgs)

End Interface
