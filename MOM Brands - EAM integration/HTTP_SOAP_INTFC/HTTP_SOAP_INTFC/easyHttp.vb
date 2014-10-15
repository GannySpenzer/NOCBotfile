Imports System
Imports System.Web
Imports System.Net
Imports System.IO


Public Class easyHttp

    Public Enum HTTPMethod As Short
        HTTP_GET = 0
        HTTP_POST = 1
    End Enum

    Private m_targetURL As String = ""
    Private m_dataToPost As String = ""
    Private m_httpMethod As HTTPMethod = HTTPMethod.HTTP_POST
    Private m_contentType As String = "application/x-www-form-urlencoded"
    Private m_ignoreServerCert As Boolean = False
    Private m_requestHeaders As System.Collections.Specialized.NameValueCollection = Nothing

    Public Sub New()

    End Sub

    Public ReadOnly Property HeaderAttributes As System.Collections.Specialized.NameValueCollection
        Get
            If (m_requestHeaders Is Nothing) Then
                m_requestHeaders = New System.Collections.Specialized.NameValueCollection
            End If
            Return m_requestHeaders
        End Get
    End Property

    Public Property URL() As String
        Get
            Return m_targetURL
        End Get
        Set(ByVal Value As String)
            m_targetURL = Value
        End Set
    End Property

    Public Property DataToPost() As String
        Get
            Return m_dataToPost
        End Get
        Set(ByVal Value As String)
            m_dataToPost = Value
        End Set
    End Property

    Public ReadOnly Property DataToPostAsBytes() As Byte()
        Get
            Dim enc As New System.Text.ASCIIEncoding
            Return enc.GetBytes(m_dataToPost)
        End Get
    End Property

    Public Property Method() As HTTPMethod
        Get
            Return m_httpMethod
        End Get
        Set(ByVal Value As HTTPMethod)
            m_httpMethod = Value
        End Set
    End Property

    Public Property ContentType() As String
        Get
            Return m_contentType
        End Get
        Set(ByVal Value As String)
            m_contentType = Value
        End Set
    End Property

    Public Property IgnoreServerCertificate() As Boolean
        Get
            Return m_ignoreServerCert
        End Get
        Set(ByVal Value As Boolean)
            m_ignoreServerCert = Value
        End Set
    End Property

    Private m_creden As System.Net.ICredentials = Nothing
    Public Property Crendentials() As System.Net.ICredentials
        Get
            Return m_creden
        End Get
        Set(ByVal Value As System.Net.ICredentials)
            m_creden = Value
        End Set
    End Property

    Public Function SendAsString() As String
        Return Send()
    End Function

    Public Function SendAsBytes() As String
        Return Send(SendAsArrayOfBytes:=True)
    End Function

    '// default - sends data as string type
    Private Function Send(Optional ByVal SendAsArrayOfBytes As Boolean = False) As String

        Dim rtn As String = "easyHttp.Send::"
        Dim responseData As String = ""

        If m_targetURL.Trim.Length > 0 Then
            Dim request As HttpWebRequest = DirectCast(WebRequest.Create(m_targetURL), HttpWebRequest)
            Dim response As HttpWebResponse = Nothing
            Dim sw As StreamWriter = Nothing
            Dim sr As StreamReader = Nothing

            ' applied http header credentials is supplied
            If Not (Me.Crendentials Is Nothing) Then
                request.Credentials = Me.Crendentials
            End If

            ' prepare request Object
            request.Method = Method.ToString().Substring(5)

            ' check/set form/post content-type if necessary
            If (m_httpMethod = HTTPMethod.HTTP_POST And _
                m_dataToPost.Trim.Length > 0 And _
                m_contentType.Trim.Length = 0) Then
                m_contentType = "application/x-www-form-urlencoded"
            End If

            ' set the content type of the request
            If (m_contentType.Trim.Length > 0) Then
                request.ContentType = m_contentType
                If SendAsArrayOfBytes Then
                    request.ContentLength = Me.DataToPostAsBytes.Length
                Else
                    request.ContentLength = Me.DataToPost.Length
                End If
            End If

            ' check if we need to ignore the server certificate (invalid or expired or something ...)
            '   need to create the policy for this!
            If m_ignoreServerCert Then
                System.Net.ServicePointManager.CertificatePolicy = New AlwaysIgnoreCertPolicy
            End If

            ' this line of code forces .NET to use SSL30 as security protocol
            '    since TLS does not work when trying to communicate with Grainger/Kaman (https)
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3

            ' action type
            'request.Headers.Add(name:="SOAPAction", value:="https://schemas.microsoft.com/crm/2006/WebServices/Retrieve")
            If Not (m_requestHeaders Is Nothing) Then
                For n As Integer = 0 To (m_requestHeaders.Count - 1)
                    request.Headers.Add(name:=m_requestHeaders.Keys(n), value:=m_requestHeaders.Item(n))
                Next
            End If

            ' Send request, If request
            If (m_httpMethod = HTTPMethod.HTTP_POST) Then
                Try
                    sw = New StreamWriter(request.GetRequestStream())
                    If SendAsArrayOfBytes Then
                        For Each b As Byte In Me.DataToPostAsBytes
                            sw.Write(Chr(b))
                        Next
                    Else
                        sw.Write(Me.DataToPost)
                    End If
                    sw.Flush()
                Catch Ex As Exception
                    Throw New ApplicationException(message:=rtn & Ex.Message, innerException:=Ex)
                Finally
                    ' need to close this writer before even trying
                    '   to read the response!
                    sw.Close()
                End Try
            End If

            ' receive response
            Try
                response = DirectCast(request.GetResponse(), HttpWebResponse)
                sr = New StreamReader(response.GetResponseStream())
                responseData = sr.ReadToEnd()
            Catch webEx As System.Net.WebException
                sr = New StreamReader(webEx.Response.GetResponseStream())
                responseData = sr.ReadToEnd()
                'Throw New ApplicationException(message:=rtn & responseData, innerException:=webEx)
                Throw New ApplicationException(message:=responseData, innerException:=webEx)
            Catch ex As Exception
                Throw New ApplicationException(message:=rtn & ex.Message, innerException:=ex)
            Finally
                sr.Close()
            End Try
        End If

        Return responseData

    End Function

End Class
