Public Class Enums

    ' prevent instance of this class being created
    Private Sub New()

    End Sub

    ' LastSendProcessStatus
    Public Enum LastSendProcessStatus
        Unknown = -1
        Successful = 0
        Warning = 1
        [Error] = 9
    End Enum

End Class
