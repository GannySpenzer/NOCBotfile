Public Class ProcessorClass
    Private m_RunInfo As runInformation
    Private m_logger As LoggerClass

    Public Sub New(ByVal runInfo As runInformation, ByVal Logger As LoggerClass)
        m_RunInfo = runInfo
        m_logger = Logger
    End Sub

    Public Function Main(Optional ByVal dttmNow As DateTime = Nothing) As Boolean
        Dim bReturn As Boolean = False
        'msgbox("entering processor main")
        m_logger.Log_Event("Processor Class Main")
        Select Case m_RunInfo.RunName
            'Case Is = "CytecRCT"

            Case Else
                m_logger.Log_Event("entering processor main case else ")
                Dim RunClass As New GenericClass(m_RunInfo)
                RunClass.clsLogger = m_logger

                bReturn = RunClass.ProcessMailbox(dttmNow)
                RunClass = Nothing

        End Select
        Return bReturn

    End Function
End Class
