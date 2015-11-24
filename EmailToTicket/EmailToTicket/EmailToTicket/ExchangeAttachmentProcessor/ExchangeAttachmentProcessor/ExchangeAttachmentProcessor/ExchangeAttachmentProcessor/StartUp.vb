Module StartUp
    Public clsLogger As LoggerClass

    Sub Main()



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

                clsLogger = New LoggerClass(runFolder & "\Logs")
                clsLogger.Open_Log_file("ExchangeAttachProcessor")
                runFolder &= "\RunControl.xml"


                'Dim myExchange As New PersonalEmailClass
                'myExchange.Main()
                runControl = runControl.Load(runFolder, GetType(runControl))

                If runControl.Items.Count > 0 Then

                    For Each runInfo As runInformation In runControl.Items
                        'If runInfo.RunName = "ZAPA" Then
                        If runInfo.Enabled Then
                            CurrentRunTime = Now
                            clsLogger.Log_Event("Processing RunName: " & runInfo.RunName & " at " & CurrentRunTime.ToString)

                            Processor = New ProcessorClass(runInfo, clsLogger)
                            clsLogger.Log_Event("After process class instantiation")
                            If Processor.Main(CurrentRunTime) Then
                                clsLogger.Log_Event("Processor.Main returned True")
                                'msgbox("after Processor.Main() = true")
                                runInfo.LastRunTime = CurrentRunTime
                            Else
                                clsLogger.Log_Event("Processor.Main returned False")
                            End If

                        Else
                            clsLogger.Log_Event(runInfo.RunName & " Is not enabled")
                        End If

                    Next
                    runControl.Save(runFolder)
                End If
            Catch ex As Exception
                clsLogger.Log_Event("START UP MAIN ERROR: " & ex.Message)
            End Try
            clsLogger.Close_Log_file()
            clsLogger = Nothing
        End If
    End Sub

End Module
