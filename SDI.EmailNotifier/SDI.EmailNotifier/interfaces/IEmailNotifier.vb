Public Interface IEmailNotifier

    Property [TO] As String
    Property [FROM] As String
    Property [CC] As String
    Property [BCC] As String
    Property [SUBJECT] As String
    Property [BODY] As String

    ReadOnly Property [LastSendProcessMessage] As String
    ReadOnly Property [LastSendProcessState] As Enums.LastSendProcessStatus

    Sub Send()

End Interface
