Imports System.Xml
Imports System.Net.Mail.MailMessage

Module Module1

    Dim m_oLogger As Logger

    Sub Main()
        InitializeLogger()

        Dim oTaskLists As TaskLists = Nothing
        If ReadTaskFile(oTaskLists) Then
            LogTasks(oTaskLists)
            CreateTickets(oTaskLists)
        End If
    End Sub

    Private Sub InitializeLogger()
        Dim sLogPath As String = Environment.CurrentDirectory
        If Not sLogPath.EndsWith("\") Then
            sLogPath &= "\"
        End If
        sLogPath &= "Logs"
        m_oLogger = New Logger(sLogPath, "AutoTickGen")
    End Sub

    Private Function ReadTaskFile(ByRef oTaskLists As TaskLists) As Boolean
        Dim bSuccess As Boolean = False

        Try
            m_oLogger.WriteLine("START Module1.ReadTaskFile")

            Dim sTaskListFileSpec As String = Environment.CurrentDirectory & "\TaskLists.xml"
            m_oLogger.WriteLine("Task list file is " & sTaskListFileSpec)
            m_oLogger.WriteLine("ProductionMode = " & My.Settings.ProductionMode)
            If IsProductionEnvironment() Then
                m_oLogger.WriteLine("Oracle connection string = " & My.Settings.oraCNStringPROD)
                m_oLogger.WriteLine("SQL Server connection string = " & My.Settings.sqlCNStringPROD)
            Else
                m_oLogger.WriteLine("Oracle connection string = " & My.Settings.oraCNString)
                m_oLogger.WriteLine("SQL Server connection string = " & My.Settings.sqlCNString)
            End If

            Dim sError As String = ""
            If TaskLists.Load(sTaskListFileSpec, GetType(TaskLists), oTaskLists, sError) Then
                bSuccess = True
            Else
                m_oLogger.WriteLine("ERROR Loading task list file")
                m_oLogger.WriteLine(sError)
            End If
        Catch ex As Exception
            LogError("ReadTaskFile", ex)
        End Try

        m_oLogger.WriteLine("END Module1.ReadTaskFile with " & bSuccess.ToString & ControlChars.NewLine)
        Return bSuccess
    End Function

    Private Sub LogTasks(oTaskLists As TaskLists)
        Try
            m_oLogger.WriteLine("START Module1.LogTasks")
            m_oLogger.WriteLine("Number of ticket infos read: " & oTaskLists.Items.Count.ToString)

            Dim iTicketInfo As Integer = 1
            For Each oTicketInfo As TicketInfo In oTaskLists.NewTicketInformation
                m_oLogger.WriteLine("*** Ticket Info " & iTicketInfo.ToString & " ***")
                m_oLogger.WriteLine("AssigneeID = " & oTicketInfo.AssigneeID)
                m_oLogger.WriteLine("Priority = " & oTicketInfo.Priority)
                m_oLogger.WriteLine("RequestorID = " & oTicketInfo.RequestorID)
                m_oLogger.WriteLine("TicketType = " & oTicketInfo.TicketType)
                For iCC As Integer = 0 To oTicketInfo.EmailCCList.Count - 1
                    m_oLogger.WriteLine("EmailCCList(" & iCC.ToString & ") = " & oTicketInfo.EmailCCList(iCC).ToString)
                Next
                m_oLogger.WriteLine("EmailCC = " & oTicketInfo.EmailCCs)
                m_oLogger.WriteLine("EmailSubject = " & oTicketInfo.EmailSubject)
                For iTask As Integer = 0 To oTicketInfo.TaskArray.Count - 1
                    m_oLogger.WriteLine("Task " & iTask.ToString & " = " & oTicketInfo.TaskArray.Item(iTask).ToString)
                Next
                iTicketInfo = iTicketInfo + 1
            Next

        Catch ex As Exception
            LogError("LogTasks", ex)
        End Try

        m_oLogger.WriteLine("END Module1.LogTasks" & ControlChars.NewLine)

    End Sub

    Private Sub CreateTickets(oTaskLists As TaskLists)
        Const cFunction As String = "CreateTickets"

        Try
            m_oLogger.WriteLine("START Module1.CreateTickets")

            For Each oTicketInfo As TicketInfo In oTaskLists.NewTicketInformation
                Dim oTicketMgr As New TicketMgr(IsProductionEnvironment)

                If oTicketMgr.AddTicket(oTicketInfo) Then
                    m_oLogger.WriteLine("Module1.CreateTickets: Created ticket " & oTicketMgr.TicketID & " for " & oTicketInfo.AssigneeID & " (" & oTicketInfo.AssigneeInfo.EmailUser & ")")
                    If oTicketInfo.AssigneeInfo.ExistsUser Then
                        If Not oTicketMgr.SendEmail(oTicketInfo) Then
                            m_oLogger.WriteLine("Module1." & cFunction & ": Error from oTicket.SendEmail for ticket ID " & oTicketMgr.TicketID & ": " & oTicketMgr.LastError)
                        End If
                    Else
                        m_oLogger.WriteLine("Module1." & cFunction & ": For ticket ID " & oTicketMgr.TicketID & ", email was not sent to assignee " & oTicketInfo.AssigneeID & " since this user doesn't exist")
                    End If
                Else
                    m_oLogger.WriteLine("Module1.CreateTickets: Error from oTicket.AddTicket: " & oTicketMgr.LastError)
                End If
            Next

        Catch ex As Exception
            LogError(cFunction, ex)
        End Try

        m_oLogger.WriteLine("END Module1." & cFunction & ControlChars.NewLine)
    End Sub

    Private Sub LogError(sFunction As String, ex As Exception, Optional strSQLstring As String = "")
        m_oLogger.WriteLine("Module1." & sFunction & ": Unexpected error - " & ex.Message & " " & ex.StackTrace)
        If strSQLstring.Trim.Length > 0 Then
            m_oLogger.WriteLine("strSQLstring = " & strSQLstring)
        End If
    End Sub

    Private Function IsProductionEnvironment() As Boolean
        Return (My.Settings.ProductionMode.ToUpper = "TRUE")
    End Function
End Module
