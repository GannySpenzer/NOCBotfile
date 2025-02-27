﻿Module StartUp
    Public ClsLogger As LoggerClass

    Sub Main1()



        If Command() = "Setup" Then
            'If Command() = "" Then

            '  Dim myExchange As New CytecRCTClass
            Dim frmSetup As New SetupForm

            frmSetup.ShowDialog()

        Else
            Try



                Dim runControl As runControl
                Dim _currentRunControlIndex As Integer
                Dim CurrentRunControlIndex As Integer
                Dim runFolder As String
                Dim Processor As ProcessorClass
                Dim CurrentRunTime As DateTime
                runFolder = Environment.CurrentDirectory

                ClsLogger = New LoggerClass(runFolder & "\Logs")
                ClsLogger.Open_Log_file("ExchangeAttachProcessor")
                runFolder &= "\RunControl.xml"


                'Dim myExchange As New PersonalEmailClass
                'myExchange.Main()
                runControl = runControl.Load(runFolder, GetType(runControl))

                If runControl.Items.Count > 0 Then

                    For Each runInfo As runInformation In runControl.Items

                        If runInfo.Enabled Then
                            CurrentRunTime = Now
                            ClsLogger.Log_Event("Processing RunName: " & runInfo.RunName & " at " & CurrentRunTime.ToString)

                            Processor = New ProcessorClass(runInfo, ClsLogger)
                            Try
                                Processor.oraCNString = CStr(My.Settings("oraCNString")).Trim
                            Catch ex As Exception
                            End Try
                            Try
                                Processor.sqlCNString = CStr(My.Settings("sqlCNString")).Trim
                            Catch ex As Exception
                            End Try
                            ClsLogger.Log_Event("After process class instantiation")
                            If Processor.Main(CurrentRunTime) Then
                                ClsLogger.Log_Event("Processor.Main returned True")
                                'msgbox("after Processor.Main() = true")
                                runInfo.LastRunTime = CurrentRunTime
                            Else
                                ClsLogger.Log_Event("Processor.Main returned False")
                            End If

                        Else
                            ClsLogger.Log_Event(runInfo.RunName & " Is not enabled")
                        End If
                    Next
                    runControl.Save(runFolder)
                End If
            Catch ex As Exception
                ClsLogger.Log_Event("START UP MAIN ERROR: " & ex.Message)
            End Try
            ClsLogger.Close_Log_file()
            ClsLogger = Nothing
        End If
    End Sub

End Module
