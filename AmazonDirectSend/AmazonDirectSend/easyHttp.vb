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

    Public Function SendAsString(Optional ByRef strErrors As String = "") As String
        Return Send(False, strErrors)
    End Function

    Public Function SendAsBytes() As String
        Return Send(SendAsArrayOfBytes:=True)
    End Function

    '// default - sends data as string type
    Private Function Send(Optional ByVal SendAsArrayOfBytes As Boolean = False, Optional ByRef strErrors As String = "") As String

        Dim rtn As String = "easyHttp.Send::"
        Dim responseData As String = ""
        strErrors = "Start;"

        If m_targetURL.Trim.Length > 0 Then
            strErrors = strErrors & " after m_targetURL.Trim.Length > 0;"
            Dim request As HttpWebRequest = Nothing '  Dim req As WebRequest = WebRequest.Create(urlAddress)
            Try
                strErrors = strErrors & " after 2;"
                request = DirectCast(WebRequest.Create(m_targetURL), HttpWebRequest)
                strErrors = strErrors & " after 3;"
            Catch exCreate As Exception
                responseData = "Place - Create " & exCreate.ToString()
                strErrors = strErrors & " after 4 - err;"
                Return responseData
                Exit Function
            End Try
            Dim response As HttpWebResponse = Nothing
            Dim sw As StreamWriter = Nothing
            Dim sr As StreamReader = Nothing
            strErrors = strErrors & " after 5;"

            ' applied http header credentials is supplied
            If Not (Me.Crendentials Is Nothing) Then
                request.Credentials = Me.Crendentials
                strErrors = strErrors & " after 6;"
            End If

            ' prepare request Object
            Dim strMethod As String = Method.ToString()
            If Len(strMethod) > 6 Then
                request.Method = Method.ToString().Substring(5)
            Else
                request.Method = "POST"
            End If
            strErrors = strErrors & " after 7;"

            ' check/set form/post content-type if necessary
            If (m_httpMethod = HTTPMethod.HTTP_POST And _
                m_dataToPost.Trim.Length > 0 And _
                m_contentType.Trim.Length = 0) Then
                m_contentType = "application/x-www-form-urlencoded"
                strErrors = strErrors & " after 8;"
            End If

            ' set the content type of the request
            If (m_contentType.Trim.Length > 0) Then
                strErrors = strErrors & " after 9;"
                request.ContentType = m_contentType
                If SendAsArrayOfBytes Then
                    request.ContentLength = Me.DataToPostAsBytes.Length
                    strErrors = strErrors & " after 10;"
                Else
                    'IPM-145 Need to ensure Amazon CXML process works
                    'request.ContentLength = Me.DataToPost.Length
                    strErrors = strErrors & " after 11;"
                End If
            End If

            ' check if we need to ignore the server certificate (invalid or expired or something ...)
            '   need to create the policy for this!
            If m_ignoreServerCert Then
                System.Net.ServicePointManager.CertificatePolicy = New AlwaysIgnoreCertPolicy
            End If
            strErrors = strErrors & " after 12;"

            'System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12  ' 3072 ' SecurityProtocolType.Tls
            strErrors = strErrors & " after 13;"

            ' action type
            'request.Headers.Add(name:="SOAPAction", value:="https://schemas.microsoft.com/crm/2006/WebServices/Retrieve")
            If Not (m_requestHeaders Is Nothing) Then
                strErrors = strErrors & " after 14;"
                For n As Integer = 0 To (m_requestHeaders.Count - 1)
                    request.Headers.Add(name:=m_requestHeaders.Keys(n), value:=m_requestHeaders.Item(n))
                Next
                strErrors = strErrors & " after 15;"
            End If

            request.Timeout = 100000

            ' Send request, If request
            If (m_httpMethod = HTTPMethod.HTTP_POST) Then
                strErrors = strErrors & " after 16;"
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
                    strErrors = strErrors & " after 17;"
                Catch Ex As Exception
                    'Throw New ApplicationException(message:=rtn & Ex.Message, innerException:=Ex)
                    responseData = "Place - sw.Write( " & Ex.ToString()
                    strErrors = strErrors & " after 18 - err;"
                    Return responseData
                Finally
                    ' need to close this writer before even trying
                    '   to read the response!
                    If Not sw Is Nothing Then
                        Try
                            sw.Close()
                        Catch ex354 As Exception

                        End Try
                    End If
                End Try
            End If

            ' receive response
            Try
                strErrors = strErrors & " after 19;"
                response = DirectCast(request.GetResponse(), HttpWebResponse)
                sr = New StreamReader(response.GetResponseStream())
                responseData = sr.ReadToEnd()
                strErrors = strErrors & " after 20;"
            Catch webEx As System.Net.WebException
                Try
                    sr = New StreamReader(webEx.Response.GetResponseStream())
                    responseData = sr.ReadToEnd()

                Catch ex As Exception

                End Try
                'Throw New ApplicationException(message:=rtn & responseData, innerException:=webEx)
                'Throw New ApplicationException(message:=responseData, innerException:=webEx)
                responseData = responseData & "Place - Catch webEx " & webEx.ToString()
                strErrors = strErrors & " after 21 - err;"
                Return responseData
            Catch ex As Exception
                strErrors = strErrors & " after 22 - err;"
                'Throw New ApplicationException(message:=rtn & Ex.Message, innerException:=Ex)
                responseData = "Place - second catch " & ex.ToString()
                Return responseData
            Finally
                If Not sr Is Nothing Then
                    Try
                        sr.Close()
                    Catch ex As Exception

                    End Try
                End If
            End Try
        End If

        strErrors = ""
        Return responseData

    End Function

End Class
