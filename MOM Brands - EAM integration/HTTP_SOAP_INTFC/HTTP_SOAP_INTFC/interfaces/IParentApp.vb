Public Interface IParentApp

    ReadOnly Property Logger As SDI.ApplicationLogger.IApplicationLogger
    ReadOnly Property EmailNotifier As SDI.EmailNotifier.IEmailNotifier
    ReadOnly Property [Name] As String
    ReadOnly Property [Version] As String
    ReadOnly Property TargetURL As String
    ReadOnly Property CustomerIdentifier As String
    ReadOnly Property OracleConnectionString1 As String
    ReadOnly Property TemporaryFilePath As String
    ReadOnly Property Username As String
    ReadOnly Property Password As String
    ReadOnly Property Attributes As Hashtable

End Interface
