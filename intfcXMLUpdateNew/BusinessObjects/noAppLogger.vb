Public Class noAppLogger

    Implements SDI.ApplicationLogger.IApplicationLogger

    Public Sub New()

    End Sub

    Public ReadOnly Property LoggingLevel() As System.Diagnostics.TraceLevel Implements SDI.ApplicationLogger.IApplicationLogger.LoggingLevel
        Get

        End Get
    End Property

    Public Sub WriteErrorLog(ByVal msg As String) Implements SDI.ApplicationLogger.IApplicationLogger.WriteErrorLog

    End Sub

    Public Sub WriteInformationLog(ByVal msg As String) Implements SDI.ApplicationLogger.IApplicationLogger.WriteInformationLog

    End Sub

    Public Sub WriteLog(ByVal msg As String, ByVal logAs As System.Diagnostics.TraceLevel) Implements SDI.ApplicationLogger.IApplicationLogger.WriteLog

    End Sub

    Public Sub WriteVerboseLog(ByVal msg As String) Implements SDI.ApplicationLogger.IApplicationLogger.WriteVerboseLog

    End Sub

    Public Sub WriteWarningLog(ByVal msg As String) Implements SDI.ApplicationLogger.IApplicationLogger.WriteWarningLog

    End Sub

End Class
