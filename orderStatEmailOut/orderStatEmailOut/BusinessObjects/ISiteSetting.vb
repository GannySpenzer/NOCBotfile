Public Interface ISiteSetting

    Property [SiteId]() As String
    Property [SiteName]() As String
    Property [StartDate]() As DateTime
    Property [bccForAll]() As String
    Property [bccForNSTK]() As String
    Property [bccForNY0A]() As String
    ReadOnly Property [StatusesToCheck]() As ArrayList

End Interface
