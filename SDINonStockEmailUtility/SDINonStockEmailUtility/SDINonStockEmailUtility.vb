Imports System.Configuration

Module SDINonStockEmailUtility

    Dim connectionString As String = ConfigurationManager.AppSettings("OLEDBconString")
    Sub Main()
        Try

            Dim objQNStkProcessor = New QuoteNonStockProcessor(connectionString)
            objQNStkProcessor.Execute()
        Catch ex As Exception

        End Try
    End Sub

End Module
