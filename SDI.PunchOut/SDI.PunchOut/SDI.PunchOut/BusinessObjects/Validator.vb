Imports System
Imports System.IO
Imports System.Xml
Imports System.Xml.Schema


Public Class Validator

    Implements IValidator

    Public Const SDIcXMLSchemaURL As String = """http://cptest.sdi.com/insiteonline/PunchOutcXML/cXML.dtd"""

    Private m_bIsValid As Boolean = False
    Private m_arrErr As ArrayList = Nothing
    Private m_arrWarn As ArrayList = Nothing
    Private m_sXML As String = ""

    Public Sub New(ByVal xmlString As String)
        m_sXML = xmlString
    End Sub

    Public Property IsValid() As Boolean
        Get
            Return m_bIsValid
        End Get
        Set(ByVal Value As Boolean)
            m_bIsValid = Value
        End Set
    End Property

    Public ReadOnly Property ErrorMessages() As ArrayList
        Get
            If (m_arrErr Is Nothing) Then
                m_arrErr = New ArrayList
            End If
            Return m_arrErr
        End Get
    End Property

    Public ReadOnly Property WarningMessages() As ArrayList
        Get
            If (m_arrWarn Is Nothing) Then
                m_arrWarn = New ArrayList
            End If
            Return m_arrWarn
        End Get
    End Property

    Public ReadOnly Property xmlString() As String
        Get
            Return m_sXML
        End Get
    End Property

    Public Shared Function Validate(ByVal xmlString As String, _
                                    Optional ByVal vendorConfig As SDI.PunchOut.punchoutVendorConfig = Nothing, _
                                    Optional ByVal appLogger As SDI.PunchOut.ApplicationLogger = Nothing) As Validator
        Dim rtn As String = "Validator.Validate"

        Dim bIsValidateAgainstSchema As Boolean = False
        Dim validateAgainstSchemaURL As String = ""

        If Not (vendorConfig Is Nothing) Then
            bIsValidateAgainstSchema = vendorConfig.IsValidateAgainstSchema
            If vendorConfig.ValidateAgainstSchemaURL.Trim.Length > 0 Then
                validateAgainstSchemaURL = vendorConfig.ValidateAgainstSchemaURL
            End If
        End If

        Dim o As Validator = Nothing
        Dim dtd As ValidateAgaintsDTD = Nothing

        xmlString = xmlString.Trim
        LogMessage(appLogger, rtn & "::xmlString(default) = " & xmlString, TraceLevel.Verbose)

        Try

            ' create an instance of the validator object
            o = New Validator(xmlString)
            LogMessage(appLogger, rtn & "::new validator object created.", TraceLevel.Verbose)

            If Not (xmlString Is Nothing) Then
                If xmlString.Length > 0 Then

                    '// bIsValidateAgainstSchema tells us if we should validate the returned cXML
                    '//     against either the default (what comes within the file) schema or against
                    '//     the schema on the vendor's config settings.
                    If bIsValidateAgainstSchema Then

                        If validateAgainstSchemaURL.Trim.Length > 0 Then
                            ' we then need to validate returned cXML against what was on the vendor settings
                            o = New Validator(CheckReplaceSchemaURL(xmlString, validateAgainstSchemaURL))
                            LogMessage(appLogger, rtn & "::validating against schema on vendor setting.", TraceLevel.Verbose)
                            LogMessage(appLogger, rtn & "::xmlString(modified)" & o.xmlString, TraceLevel.Verbose)
                        Else
                            ' just validate against schema within cXML
                            o = New Validator(xmlString)
                            LogMessage(appLogger, rtn & "::validating against schema specified on cXML file.", TraceLevel.Verbose)
                        End If

                        dtd = New ValidateAgaintsDTD(o)
                        dtd.Validate(o.xmlString)

                        o.IsValid = (o.ErrorMessages.Count = 0)

                    Else

                        ' don't do any validation (default with existing code base)
                        o.IsValid = True
                        LogMessage(appLogger, rtn & "::skipping validation against schema.", TraceLevel.Verbose)

                    End If

                Else
                    o.ErrorMessages.Add(rtn & "::Passed parameter string is blank.")
                    o.IsValid = False
                End If
            Else
                o.ErrorMessages.Add(rtn & "::Passed parameter string is not set to an instance of an object.")
                o.IsValid = False
            End If
        Catch ex As Exception
            Throw New ApplicationException(rtn & "::" & ex.Message, ex)
        Finally
            dtd = Nothing
        End Try

        Return o
    End Function

    Private Shared Sub LogMessage(ByVal appLogger As SDI.PunchOut.ApplicationLogger, ByVal msg As String, ByVal level As System.Diagnostics.TraceLevel)
        If Not (appLogger Is Nothing) Then
            appLogger.WriteLog(msg, level)
        End If
    End Sub

    Private Shared Function CheckReplaceSchemaURL(ByVal xmlString As String, ByVal schemaURL As String) As String

        Dim rtn As String = "Validator.CheckReplaceSchemaURL"
        Dim ret As String = ""

        Try

            Dim doc As New XmlDocument
            doc.LoadXml(xmlString)

            ret = doc.OuterXml

            Dim searchThis As String = ""

            Try
                searchThis = doc.DocumentType.OuterXml
            Catch ex As Exception
            End Try

            If searchThis.Trim.Length > 0 And _
               schemaURL.Trim.Length > 0 Then
                Dim s As String = ""
                s &= "<!DOCTYPE cXML SYSTEM "
                s &= """" & schemaURL & """"
                s &= "[]>"
                ret = ret.Replace(searchThis, s)
            End If

            doc = Nothing

        Catch ex As Exception
            Throw New ApplicationException(rtn & "::" & ex.Message, ex)
        End Try

        Return ret

    End Function

    Public Sub SendMessage(ByVal msg As String, ByVal lvl As System.Xml.Schema.XmlSeverityType) Implements IValidator.SendMessage
        Select Case lvl
            Case XmlSeverityType.Warning
                Me.WarningMessages.Add(msg)
            Case XmlSeverityType.Error
                Me.ErrorMessages.Add(msg)
        End Select
    End Sub

End Class
