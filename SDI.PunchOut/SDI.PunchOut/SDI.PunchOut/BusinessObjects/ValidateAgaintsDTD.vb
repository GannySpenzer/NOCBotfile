Imports System
Imports System.IO
Imports System.Xml
Imports System.Xml.Schema


Public Class ValidateAgaintsDTD

    Private m_obj As IValidator = Nothing

    Public Sub New(ByVal obj As IValidator)
        m_obj = obj
    End Sub

    Public Sub Validate(ByVal xmlString As String)

        Dim rtn As String = "ValidateAgainstDTD.Validate"

        Dim stream As StringReader = Nothing
        Dim rdr As XmlTextReader = Nothing
        Dim vr As XmlValidatingReader = Nothing

        Try

            stream = New System.IO.StringReader(xmlString)
            rdr = New System.Xml.XmlTextReader(stream)

            vr = New XmlValidatingReader(rdr)
            vr.ValidationType = ValidationType.DTD

            AddHandler vr.ValidationEventHandler, AddressOf ValidationCallBack

            While (vr.Read())
            End While

            RemoveHandler vr.ValidationEventHandler, AddressOf ValidationCallBack

        Catch ex As Exception
            If Not (m_obj Is Nothing) Then
                m_obj.SendMessage(rtn & "::" & ex.ToString, XmlSeverityType.Error)
            End If
        Finally
            Try
                vr.Close()
            Catch ex As Exception
            Finally
                vr = Nothing
            End Try
            Try
                rdr.Close()
            Catch ex As Exception
            Finally
                rdr = Nothing
            End Try
            Try
                stream.Close()
            Catch ex As Exception
            Finally
                stream = Nothing
            End Try
        End Try

    End Sub

    Private Sub ValidationCallBack(ByVal sender As Object, ByVal args As ValidationEventArgs)
        If Not (m_obj Is Nothing) Then
            Dim s As String = ""
            s &= "*** Validation message " & vbCrLf
            s &= "Severity:" & args.Severity.ToString & vbCrLf
            s &= "Message:" & args.Message & vbCrLf
            s &= "Exception.SourceURI:" & args.Exception.SourceUri & vbCrLf
            s &= args.Exception.ToString & vbCrLf
            Try
                m_obj.SendMessage(msg:=s, lvl:=args.Severity)
            Catch ex As Exception
            End Try
        End If
    End Sub

End Class
