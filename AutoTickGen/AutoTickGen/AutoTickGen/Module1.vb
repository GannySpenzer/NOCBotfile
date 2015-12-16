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
                m_oLogger.WriteLine(" ")
                m_oLogger.WriteLine("*** Ticket Info " & iTicketInfo.ToString & " ***")
                m_oLogger.WriteLine("AssigneeID = " & oTicketInfo.AssigneeID)
                m_oLogger.WriteLine("Priority = " & oTicketInfo.Priority)
                m_oLogger.WriteLine("RequestorID = " & oTicketInfo.RequestorID)
                m_oLogger.WriteLine("TicketType = " & oTicketInfo.TicketType)
                m_oLogger.WriteLine("IncludeWeekends = " & oTicketInfo.IncludeWeekends)
                m_oLogger.WriteLine("Frequency = " & oTicketInfo.Frequency)
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
                If IsCreateTicketToday(oTicketInfo) Then
                    'Dim oTicketMgr As New TicketMgr(IsProductionEnvironment)

                    'If oTicketMgr.AddTicket(oTicketInfo) Then
                    '    m_oLogger.WriteLine("Module1." & cFunction & ": Created ticket " & oTicketMgr.TicketID & " for " & oTicketInfo.AssigneeID & " (" & oTicketInfo.AssigneeInfo.EmailUser & ")")
                    '    If oTicketInfo.AssigneeInfo.ExistsUser Then
                    '        If Not oTicketMgr.SendEmail(oTicketInfo) Then
                    '            m_oLogger.WriteLine("Module1." & cFunction & ": Error from oTicket.SendEmail for ticket ID " & oTicketMgr.TicketID & ": " & oTicketMgr.LastError)
                    '        End If
                    '    Else
                    '        m_oLogger.WriteLine("Module1." & cFunction & ": For ticket ID " & oTicketMgr.TicketID & ", email was not sent to assignee " & oTicketInfo.AssigneeID & " since this user doesn't exist")
                    '    End If
                    'Else
                    '    m_oLogger.WriteLine("Module1." & cFunction & ": Error from oTicket.AddTicket: " & oTicketMgr.LastError)
                    'End If
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

    Private Function IsCreateTicketToday(oTicketInfo As TicketInfo) As Boolean
        Const cFunction As String = "IsCreateTicketToday"

        Dim bCreateTicket As Boolean = False

        Try
            If oTicketInfo.Frequency.Trim.Length > 0 Then
                ' If the frequency is set, create the ticket
                ' based on the setting.
                Dim sFrequency As String = oTicketInfo.Frequency.Trim.ToUpper
                Select Case sFrequency
                    Case "DAILY"
                        ' Creates tickets on weekends also.
                        ' Ignores the "IncludeWeekends" setting.
                        bCreateTicket = True
                    Case "WEEKDAYS"
                        If IsWeekend() Then
                            m_oLogger.WriteLine("Module1." & cFunction & ": Frequency is WEEKDAYS but it's the weekend so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                        Else
                            bCreateTicket = True
                        End If
                    Case "WEEKLY"
                        If IsMonday() Then
                            bCreateTicket = True
                        Else
                            m_oLogger.WriteLine("Module1." & cFunction & ": Frequency is WEEKLY but it's not Monday so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                        End If
                    Case "MONTHLY"
                        If IsFirstMondayOfMonth() Then
                            bCreateTicket = True
                        Else
                            m_oLogger.WriteLine("Module1." & cFunction & ": Frequency is MONTHLY but it's not the first Monday of the month so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                        End If
                    Case "YEARLY"
                        If IsFirstMondayOfYear() Then
                            bCreateTicket = True
                        Else
                            m_oLogger.WriteLine("Module1." & cFunction & ": Frequency is YEARLY but it's not the first Monday of the year so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                        End If
                    Case Else
                        Dim iDateOfMonth As Integer
                        If Integer.TryParse(sFrequency, iDateOfMonth) Then
                            ' If we're given an integer, it represents the
                            ' date of any month.
                            If Now.Day = iDateOfMonth Then
                                bCreateTicket = True
                            Else
                                m_oLogger.WriteLine("Module1." & cFunction & ": Frequency is day " & iDateOfMonth.ToString & " of every month but today's day is " & Now.Day.ToString & " so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                            End If
                        Else
                            Dim dt As DateTime
                            If IsMonthDate(sFrequency, dt) Then
                                ' If we're given a specific month and date, see if today is the day.
                                If dt.ToString("MM/dd/yyyy") = Now.ToString("MM/dd/yyyy") Then
                                    bCreateTicket = True
                                Else
                                    m_oLogger.WriteLine("Module1." & cFunction & ": Frequency is " & oTicketInfo.Frequency & " but today is " & Now.ToString("MMMM d") & " so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                                End If
                            Else
                                Dim sGivenDayNames() As String = Nothing
                                If IsDayNames(sFrequency, sGivenDayNames) Then
                                    ' If we're given a specific day of the week, check if today is the day.
                                    If sGivenDayNames.Contains(Now.DayOfWeek.ToString.ToUpper) Then
                                        bCreateTicket = True
                                    Else
                                        m_oLogger.WriteLine("Module1." & cFunction & ": Frequency is " & oTicketInfo.Frequency & " which is interpreted as named days but today is " & Now.DayOfWeek.ToString & " so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                                    End If
                                Else
                                    m_oLogger.WriteLine("Module1." & cFunction & ": Frequency is " & oTicketInfo.Frequency & " which could not be interpreted so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                                End If
                            End If
                        End If
                End Select
            Else
                ' If the frequency is not set, create the
                ' ticket every day except, possibly, weekends.
                If IsWeekend() Then
                    ' If it's a weekend, check if the user wants the ticket
                    ' created on the weekend.
                    If oTicketInfo.IncludeWeekends.Trim.ToUpper = "TRUE" Then
                        bCreateTicket = True
                    Else
                        m_oLogger.WriteLine("Module1." & cFunction & ": No frequency was specified so we default to daily but weekends are not included and today is " & Now.DayOfWeek & " so don't create a ticket for subject " & oTicketInfo.EmailSubject)
                    End If
                Else
                    ' If it's a weekday, create the ticket.
                    bCreateTicket = True
                End If
            End If

        Catch ex As Exception
            bCreateTicket = False
            LogError(cFunction, ex)
        End Try

        Return bCreateTicket
    End Function

    Private Function IsWeekend() As Boolean
        Dim bIsWeekend As Boolean = False

        Select Case Weekday(Now)
            Case DayOfWeek.Saturday, DayOfWeek.Sunday
                bIsWeekend = True
            Case Else
                bIsWeekend = False
        End Select

        Return bIsWeekend
    End Function

    Private Function IsMonday() As Boolean
        Dim bIsMonday As Boolean = False

        If Weekday(Now) = DayOfWeek.Monday Then
            bIsMonday = True
        End If

        Return bIsMonday
    End Function

    Private Function IsFirstMondayOfMonth() As Boolean
        Dim bIsFirstMonday As Boolean = False

        Try
            Dim dtFirstMonday As DateTime = GetFirstMondayInMonth(Now.Year, Now.Month)
            If dtFirstMonday = Now Then
                bIsFirstMonday = True
            End If
        Catch ex As Exception
        End Try

        Return bIsFirstMonday
    End Function

    Private Function IsFirstMondayOfYear() As Boolean
        Const cJanuary As Integer = 1

        Dim bIsFirstMonday As Boolean = False

        Try
            Dim dtFirstMonday As DateTime = GetFirstMondayInMonth(Now.Year, cJanuary)
            If dtFirstMonday = Now Then
                bIsFirstMonday = True
            End If
        Catch ex As Exception
        End Try

        Return bIsFirstMonday
    End Function

    Private Function GetFirstMondayInMonth(iYear As Integer, iMonth As Integer) As DateTime
        ' This function returns the date of the first Monday in the given month.

        Const cDaysInWeek As Integer = 7

        Dim dt As New DateTime(Now.Year, Now.Month, 1)

        Try
            If dt.DayOfWeek <> DayOfWeek.Monday Then
                Dim iDays As Integer

                If dt.DayOfWeek > DayOfWeek.Monday Then
                    ' If the first day in the month is Tuesday through Saturday,
                    ' calculate the number of days until the next Monday.
                    iDays = cDaysInWeek - dt.DayOfWeek + DayOfWeek.Monday
                Else
                    ' Get number of days from Sunday to Monday.
                    ' It'll take only one day to get to Monday.
                    iDays = DayOfWeek.Monday - dt.DayOfWeek
                End If

                ' Add the number of days calculated to get to
                ' the date of the first Monday in the month.
                dt = dt.AddDays(iDays)
            End If
        Catch ex As Exception
        End Try

        Return dt
    End Function

    Private Function IsMonthDate(sFrequency As String, ByRef dt As DateTime) As Boolean
        Const cMonthComponent As Integer = 0
        Const cDateComponent As Integer = 1
        Const cMonthAndDate As Integer = 2
        Const cMonthsInYear As Integer = 12

        Dim sMonthsOfYear() As String = {"JANUARY", "FEBRUARY", "MARCH", "APRIL", "MAY", "JUNE", "JULY", "AUGUST", "SEPTEMBER", "OCTOBER", "NOVEMBER", "DECEMBER"}
        Dim bIsMonthDate As Boolean = False

        Try
            ' Split the frequency into month and date components.
            Dim sDateComponents() As String
            sDateComponents = sFrequency.Split(" ")

            ' Make sure there are two components: month and date
            If sDateComponents.Length = cMonthAndDate Then
                Dim iDateOfMonth As Integer

                ' The second component must be an integer, or, the date of the month
                If Integer.TryParse(sDateComponents(cDateComponent), iDateOfMonth) Then

                    ' The first component must be a valid month
                    Dim iMonth As Integer = 1
                    Dim sGivenMonth As String = sDateComponents(cMonthComponent).Trim.ToUpper

                    If sGivenMonth.Length > 0 Then
                        While iMonth <= cMonthsInYear And Not bIsMonthDate
                            If sMonthsOfYear(iMonth - 1) = sGivenMonth Then
                                bIsMonthDate = True
                                dt = New DateTime(Now.Year, iMonth, iDateOfMonth)
                            End If

                            iMonth += 1
                        End While
                    End If
                End If
            End If

        Catch ex As Exception
        End Try

        Return bIsMonthDate
    End Function

    Private Function IsDayNames(sFrequency As String, ByRef sGivenDayNames() As String) As Boolean
        Dim bIsDayName As Boolean = False

        Try
            sFrequency = sFrequency.Replace(" ", "")
            sGivenDayNames = sFrequency.ToUpper.Split(",")
            If sGivenDayNames.Length > 0 Then
                bIsDayName = True
            End If

        Catch ex As Exception
        End Try

        Return bIsDayName
    End Function
End Module
