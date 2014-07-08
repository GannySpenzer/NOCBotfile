Public Interface IApplicationLogger

    Sub WriteLog(ByVal msg As String, ByVal logAs As System.Diagnostics.TraceLevel)
    Sub WriteVerboseLog(ByVal msg As String)
    Sub WriteInformationLog(ByVal msg As String)
    Sub WriteWarningLog(ByVal msg As String)
    Sub WriteErrorLog(ByVal msg As String)
    ReadOnly Property LoggingLevel() As System.Diagnostics.TraceLevel

End Interface