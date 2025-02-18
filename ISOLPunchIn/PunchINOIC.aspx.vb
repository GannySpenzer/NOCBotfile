Imports System
Imports System.Diagnostics
Imports System.Xml
Imports ISOLPunchIn.ISOLPunchINFunc


Public Class PunchINOIC

    Inherits System.Web.UI.Page

    Private m_logger As ApplicationLogger = Nothing

    Private m_sessionId As String = ""

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub
    Protected WithEvents lblError As System.Web.UI.WebControls.Label
    Protected WithEvents btnReturn As System.Web.UI.WebControls.Button

    'NOTE: The following placeholder declaration is required by the Web Form Designer.
    'Do not delete or move it.
    Private designerPlaceholderDeclaration As System.Object

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

    Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Try
        '    EventLog.WriteEntry("ISOLPunchIn", "ISOLPunchin/punchINOIC.aspx - " & vbCrLf & _
        '                                       "       Session id      - " & Session.SessionID & vbCrLf & _
        '                                       "       Session comp id - " & Session("COMP_ID") & vbCrLf & _
        '                                       "       Session Hook    - " & Session("HOOK_URL"))
        'Catch ex As Exception
        'End Try
        m_sessionId = Session.SessionID

        If Not Page.IsPostBack Then

            'added more debugging statements
            Dim m_logFilePath As String = ""
            Dim m_logLevel As System.Diagnostics.TraceLevel = Diagnostics.TraceLevel.Off
            'Dim m_logger As ApplicationLogger = Nothing
            ' create a log per day
            m_logFilePath = Server.MapPath("") & "\logs\" & Now.Year.ToString("0000") & Now.Month.ToString("00") & Now.Day.ToString("00") & "punchIn.log"
            ' read log level from web.config file
            Try
                m_logLevel = Diagnostics.TraceLevel.Verbose  '   CType(ConfigurationSettings.AppSettings("appLogLevel"), System.Diagnostics.TraceLevel)
            Catch ex As Exception
            End Try
            m_logger = New ApplicationLogger(m_logFilePath, m_logLevel)

            'm_logger.WriteInformationLog("Session ID=" & Session.SessionID & _
            '    "Hook URL=" & CType(Session("HOOK_URL"), String))

            Dim hookURL As String = ""
            Try
                hookURL = Session("HOOK_URL")
                If (hookURL Is Nothing) Then
                    hookURL = ""
                End If
            Catch ex As Exception
            End Try

            Dim compId As String = ""
            Try
                compId = Session("COMP_ID")
                If (compId Is Nothing) Then
                    compId = ""
                End If
            Catch ex As Exception
            End Try
            Dim pw As String = ""
            Try
                pw = Session("PASSWORD")
                If (pw Is Nothing) Then
                    pw = ""
                End If
            Catch ex As Exception
            End Try
            Dim emailAdd As String = ""
            Try
                emailAdd = Session("USER_EMAIL")
                If (emailAdd Is Nothing) Then
                    emailAdd = ""
                End If
            Catch ex As Exception
            End Try
            ' VR 07/19/2013 this part is special for ASCEND - 1
            Dim sWorkOrder As String = ""
            Try
                sWorkOrder = Session("WorkOrderPnchInA")
                If (sWorkOrder Is Nothing) Then
                    sWorkOrder = ""
                End If
            Catch ex As Exception
            End Try
            ' VR 07/19/2013 END this part is special for ASCEND - 1

            'm_logger.WriteInformationLog("Session ID=" & Session.SessionID & _
            '    "Hook URL=" & hookURL & "  ||  " & compId & "  ||  " & pw & "  ||  " & emailAdd)

            'If Session("HOOK_URL") <> "" Then
            If hookURL.Length > 0 And _
               compId.Length > 0 And _
               pw.Length > 0 And _
               emailAdd.Length > 0 Then
                Dim strINStatus As String = UpdPunchinSession()
                If Not strINStatus = "" Then
                    btnReturn.Visible = True
                    lblError.Text = strINStatus
                    Exit Sub
                End If
                Dim strURL As String
                'strURL = "catalogtree.aspx?sid=" & Session.SessionID
                strURL = "catalogtree.aspx?sid=" & Session.SessionID
                'Response.Redirect("http://localhost/insiteonline/" & strURL)
                'If Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "LOCALHOST" Then
                '    Response.Redirect("http://localhost/insiteonline/loading.aspx?HOME=N&Page=" & strURL)
                'ElseIf Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST" Then
                '    Response.Redirect("http://cptest/insiteonline/loading.aspx?HOME=N&Page=" & strURL)
                'ElseIf Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST.SDI.COM" Then
                '    Response.Redirect("http://cptest.sdi.com/insiteonline/loading.aspx?HOME=N&Page=" & strURL)
                'Else
                '    Response.Redirect("http://www.insiteonline.com/loading.aspx?HOME=N&Page=" & strURL)
                'End If

                'm_logger.WriteInformationLog("INFO :  SessionID = " & Session.SessionID & vbCrLf & _
                '                             "         Comp id   = " & compId & vbCrLf & _
                '                             "         Email     = " & emailAdd & vbCrLf & _
                '                             "         HOOKURL   = " & hookURL.ToString)

                Dim sIMSRedir As String = CType(ConfigurationSettings.AppSettings("ISOLforIMS"), String)
                Dim sIMSSdiComRedir As String = CType(ConfigurationSettings.AppSettings("IMSSdiComISOL"), String)
                Dim sLocalhostRedir As String = CType(ConfigurationSettings.AppSettings("ISOLforLOCALHOST"), String)

                If Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "LOCALHOST" Then
                    'Response.Redirect("http://localhost/insiteonline/" & strURL)
                    Response.Redirect(sLocalhostRedir & strURL)
                    'Response.Redirect("http://localhost/DEPLinsiteonline/" & strURL)
                ElseIf Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST" Then
                    Response.Redirect("http://cptest/insiteonline/" & strURL)
                ElseIf Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "IMS" Then
                    'Response.Redirect("http://IMS/insiteonline/" & strURL)
                    Response.Redirect(sIMSRedir & strURL)
                    'Response.Redirect("http://IMS/webplusims/" & strURL)
                ElseIf Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "IMS.SDI.COM" Then
                    'Response.Redirect("http://IMS.SDI.COM/insiteonline/" & strURL)
                    Response.Redirect(sIMSSdiComRedir & strURL)
                    'Response.Redirect("http://IMS.SDI.COM/webplusims/" & strURL)
                ElseIf Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPREPLACE_COPY" Then
                    Response.Redirect("http://CPREPLACE_COPY:90/" & strURL)
                ElseIf Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPREPLACE_COPY:90" Then
                    Response.Redirect("http://CPREPLACE_COPY:90/" & strURL)
                ElseIf Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "CPTEST.SDI.COM" Then
                    Response.Redirect("http://cptest.sdi.com/insiteonline/" & strURL)
                Else
                    Response.Redirect("http://www.insiteonline.com/" & strURL)
                End If
            Else
                ' log request string
                m_logger.WriteInformationLog("ERROR :  SessionID = " & Session.SessionID & vbCrLf & _
                                             "         Comp id   = " & compId & vbCrLf & _
                                             "         Email     = " & emailAdd & vbCrLf & _
                                             "         HOOKURL   = " & hookURL.ToString)

                Dim sString As String = ""
                If hookURL.Length = 0 Then
                    sString &= " Hook URL "
                Else
                    If compId.Length = 0 Then
                        sString &= " Company Id "
                    End If
                    If pw.Length = 0 Then
                        sString &= " Password "
                    End If
                    If emailAdd.Length = 0 Then
                        sString &= " Email "
                    End If
                End If

                'lblError.Text = "Error --- Missing Hook URL"
                lblError.Text = "Error --- Missing Parameter(s) " & sString
                m_logger.WriteInformationLog("ERROR :  Missing Parameter(s) " & sString)

            End If
        End If

    End Sub

    Private Function UpdPunchinSession() As String

        Dim strCustID As String
        Dim strBusinessUnit As String
        Dim strfromDomain As String
        Dim strSenderDomain As String
        Dim strSenderIdentity As String
        Dim strSharedSecret As String
        Dim strUserAgent As String
        Dim strFirstName As String
        Dim strLastName As String
        Dim strUserEmail As String
        ' VR 07/22/2013 this part is special for ASCEND - 1
        Dim strWorkorder As String
        ' VR 07/22/2013 this part is special for ASCEND - 1
        Dim strUniqueName As String
        Dim strCostCenter As String
        Dim strURL As String
        Dim strShipToName As String
        Dim strShipToDeliverTo As String
        Dim strShipToStreet As String
        Dim strShipToCity As String
        Dim strShipToState As String
        Dim strShipToPostalCode As String
        Dim strShipToCountry As String

        '        'debug
        '        '-erwin
        '#If DEBUG Then
        '        Dim s As String = ""
        '        For Each sKey As String In Session.Keys
        '            s &= sKey & " = " & CStr(Session(sKey)) & vbCrLf
        '        Next
        '        Stop
        '#End If

        Dim strXMLDir As String = Server.MapPath("") & "\PunchinXML\PunchINSAP.xml"
        Dim reader As XmlTextReader = Nothing
        ' Load the XmlTextReader from the stream
        reader = New XmlTextReader(strXMLDir)
        While reader.Read
            If reader.NodeType = XmlNodeType.Element Then
                If reader.Name.ToUpper = "CATALOG" Then
                    reader.Read()
                    reader.Read()
                    If reader.NodeType = XmlNodeType.Element And _
                        reader.Name.ToUpper = "COMPID" Then
                        reader.Read()
                        If reader.Value.ToUpper = Session("COMP_ID").ToUpper Then
                            strUniqueName = reader.Value
                            If reader.HasValue Then
                                reader.Read()
                            End If
                            reader.Read()
                            reader.Read()
                            If reader.NodeType = XmlNodeType.Element And _
                                reader.Name.ToUpper = "FROMDOMAIN" Then
                                reader.Read()
                                strfromDomain = reader.Value
                            End If
                            If reader.HasValue Then
                                reader.Read()
                            End If
                            reader.Read()
                            reader.Read()
                            If reader.NodeType = XmlNodeType.Element And _
                                reader.Name.ToUpper = "CUSTID" Then
                                reader.Read()
                                strCustID = reader.Value
                            End If
                            If reader.HasValue Then
                                reader.Read()
                            End If
                            reader.Read()
                            reader.Read()
                            If reader.NodeType = XmlNodeType.Element And _
                                reader.Name.ToUpper = "PASSWORD" Then
                                reader.Read()
                                strSharedSecret = reader.Value
                            End If
                            If reader.HasValue Then
                                reader.Read()
                            End If
                        Else
                            reader.Skip()
                        End If
                    End If
                End If
            End If
        End While
        reader.Close()

        'validate password
        Dim strPassword As String = Session("PASSWORD")
        'decrypt password
        Dim strPasswhash As String = GenerateHash(Trim(strSharedSecret))
        If strPassword <> strPasswhash Then
            Return " Error --- Invalid Password "
        End If

        If Trim(strfromDomain) = "" Then
            strfromDomain = " "
        End If
        Dim objEnterprise As New clsEnterprise(strCustID)
        strBusinessUnit = objEnterprise.BusinessUnit
        If Trim(strBusinessUnit) = "" Then
            Return " Invalid Identity (customer number) "
        End If
        strSenderDomain = strfromDomain
        strSenderIdentity = Session("COMP_ID")
        If Trim(strSenderIdentity) = "" Then
            strSenderIdentity = " "
        End If

        strUserAgent = Session("USER_ID")
        If Trim(strUserAgent) = "" Then
            Return " Missing UserID "
        End If

        ' VR 07/22/2013 this part is special for ASCEND - 2
        strWorkorder = Session("WorkOrderPnchInA")
        If Trim(strWorkorder) = "" Then
            strWorkorder = " "
        End If
        ' VR 07/22/2013 this part is special for ASCEND - 2  -  END

        strUserEmail = Session("USER_EMAIL")
        If Trim(strUserEmail) = "" Then
            strUserEmail = " "
        End If

        strCostCenter = " "
        strFirstName = Session("USER_ID")
        If Trim(strFirstName) = "" Then
            strFirstName = " "
        End If
        'strLastName = Session("USERNAME")
        strLastName = Session("USER_EMAIL")
        If Trim(strLastName) = "" Then
            Return " Missing UserName "
        End If
        strURL = Session("HOOK_URL")
        If Trim(strURL) = "" Then
            Return " Empty BrowserFormPost URL "
        End If
        ' don't limit to get error
        'If Not (strURL Is Nothing) Then
        '    strURL = strURL.Trim
        '    If strURL.Length > 640 Then
        '        strURL = strURL.Substring(0, 640)
        '    End If
        'End If
        strShipToName = " "
        strShipToDeliverTo = " "
        strShipToStreet = " "
        strShipToCity = " "
        strShipToState = " "
        strShipToPostalCode = " "
        strShipToCountry = " "

        Dim strSQLstring As String
        strSQLstring = "" & _
                       "SELECT A.CUST_ID " & vbCrLf & _
                       "FROM PS_ISA_IOL_PUNCHIN A " & vbCrLf & _
                       "WHERE A.ISA_PI_IOL_SES_ID = '" & m_sessionId & "' " & vbCrLf & _
                       ""

        Dim strExist As String = ORDBData.GetScalar(strSQLstring)

        ' VR 07/22/2013 this part is special for ASCEND - 3 - SEE LINES 382 AND 410
        If Trim(strExist) = "" Then
            strSQLstring = "" & _
                           "INSERT INTO PS_ISA_IOL_PUNCHIN " & vbCrLf & _
                           "(" & vbCrLf & _
                           " ISA_PI_SESSION_ID," & vbCrLf & _
                           " CUST_ID," & vbCrLf & _
                           " ISA_BUSINESS_UNIT," & vbCrLf & _
                           " ISA_PI_FROMDOMAIN," & vbCrLf & _
                           " ISA_PI_SNDERDOMAIN," & vbCrLf & _
                           " ISA_PI_SENDERID," & vbCrLf & _
                           " ISA_PI_SHAREDSECRE," & vbCrLf & _
                           " ISA_PI_USERAGENT," & vbCrLf & _
                           " ISA_PI_EMAIL," & vbCrLf & _
                           " ISA_WORK_ORDER_NO," & vbCrLf & _
                           " ISA_PI_UNIQUENAME," & vbCrLf & _
                           " FIRST_NAME_SRCH," & vbCrLf & _
                           " LAST_NAME_SRCH," & vbCrLf & _
                           " ISA_PI_COSTCENTER," & vbCrLf & _
                           " ISA_PI_URL," & vbCrLf & _
                           " ISA_PI_SHIPTONAME," & vbCrLf & _
                           " ISA_PI_SHIPTODELTO," & vbCrLf & _
                           " ISA_PI_SHIPTOSTRET," & vbCrLf & _
                           " ISA_PI_SHIPTOCITY," & vbCrLf & _
                           " ISA_PI_SHIPTOSTATE," & vbCrLf & _
                           " ISA_PI_SHIPTOZIP," & vbCrLf & _
                           " ISA_PI_SHIPTOCNTRY," & vbCrLf & _
                           " ISA_PI_STATUS," & vbCrLf & _
                           " ISA_PI_IOL_SES_ID," & vbCrLf & _
                           " LASTUPDDTTM" & vbCrLf & _
                           ")" & vbCrLf & _
                           " VALUES " & vbCrLf & _
                           "(" & vbCrLf & _
                           " '" & m_sessionId & "', " & vbCrLf & _
                           " '" & strCustID & "'," & vbCrLf & _
                           " '" & strBusinessUnit & "'," & vbCrLf & _
                           " '" & strfromDomain & "'," & vbCrLf & _
                           " '" & strSenderDomain & "'," & vbCrLf & _
                           " '" & strSenderIdentity & "'," & vbCrLf & _
                           " '" & strSharedSecret & "'," & vbCrLf & _
                           " '" & strUserAgent & "'," & vbCrLf & _
                           " '" & strUserEmail & "'," & vbCrLf & _
                           " '" & strWorkorder & "'," & vbCrLf & _
                           " '" & strUniqueName & "'," & vbCrLf & _
                           " '" & strFirstName & "'," & vbCrLf & _
                           " '" & strLastName & "'," & vbCrLf & _
                           " '" & strCostCenter & "'," & vbCrLf & _
                           " '" & strURL & "'," & vbCrLf & _
                           " '" & strShipToName & "'," & vbCrLf & _
                           " '" & strShipToDeliverTo & "'," & vbCrLf & _
                           " '" & strShipToStreet & "'," & vbCrLf & _
                           " '" & strShipToCity & "'," & vbCrLf & _
                           " '" & strShipToState & "'," & vbCrLf & _
                           " '" & strShipToPostalCode & "'," & vbCrLf & _
                           " '" & strShipToCountry & "'," & vbCrLf & _
                           " 'A',' '," & vbCrLf & _
                           " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM') " & vbCrLf & _
                           ")" & vbCrLf & _
                           ""
        Else
            strSQLstring = "" & _
                           "UPDATE PS_ISA_IOL_PUNCHIN " & vbCrLf & _
                           "SET " & vbCrLf & _
                           " ISA_PI_UNIQUENAME = '" & strUniqueName & "', " & vbCrLf & _
                           " ISA_PI_FROMDOMAIN = '" & strfromDomain & "', " & vbCrLf & _
                           " CUST_ID = '" & strCustID & "', " & vbCrLf & _
                           " ISA_BUSINESS_UNIT = '" & strBusinessUnit & "', " & vbCrLf & _
                           " ISA_WORK_ORDER_NO = '" & strWorkorder & "', " & vbCrLf & _
                           " ISA_PI_SNDERDOMAIN = '" & strSenderDomain & "', " & vbCrLf & _
                           " ISA_PI_SENDERID = '" & strSenderDomain & "', " & vbCrLf & _
                           " ISA_PI_URL = '" & strURL & "' " & vbCrLf & _
                           "WHERE ISA_PI_SESSION_ID = '" & m_sessionId & "' " & vbCrLf & _
                           ""
        End If

        Dim rowsaffected As Integer = ORDBData.ExecNonQuery(strSQLstring)
        If rowsaffected = 0 Then
            Return "Error Inserting into PunchIN table"
        End If

        Return ""

    End Function

    Private Sub btnReturn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReturn.Click
        Response.Redirect(Session("HOOK_URL"))
    End Sub

End Class
