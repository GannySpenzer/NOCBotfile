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
				if (m_oraCNString.Trim.Length > 0) Then
					RunClass.SetORACN(m_oraCNString.Trim)
				end if
				if (m_sqlCNString.Trim.Length > 0) Then
					RunClass.SetSQLCN(m_sqlCNString.Trim)
				end if
                RunClass.clsLogger = m_logger

                bReturn = RunClass.ProcessMailbox(dttmNow)
                RunClass = Nothing

        End Select
        Return bReturn

    End Function
	
	private m_oraCNString as string = ""
	public property oraCNString as string
        Get
            Return m_oraCNString
        End Get
        Set(ByVal Value As String)
            m_oraCNString = Value
        End Set
	end property 
	
	private m_sqlCNString as string = ""
	public property sqlCNString as string
        Get
            Return m_sqlCNString
        End Get
        Set(ByVal Value As String)
            m_sqlCNString = Value
        End Set
	end property
End Class
