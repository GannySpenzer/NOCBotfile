
Imports System.Data
Imports System.Data.OleDb
Imports System.Xml
Imports System.IO
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Reflection
Imports SDI.ApplicationLogger
Imports System.Net.Mail
Imports System.Text

Module Module1

    Sub Main()

        Dim objPODueDTChangeEmail As PODueDTChangeEmail = New PODueDTChangeEmail

        Call objPODueDTChangeEmail.Main2()

    End Sub

End Module
