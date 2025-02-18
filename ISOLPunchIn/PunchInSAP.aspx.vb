Imports System
Imports System.Diagnostics
Imports System.Xml
Imports ISOLPunchIn.ISOLPunchINFunc


Public Class PunchInSAP
    Inherits System.Web.UI.Page

    Private m_logFilePath As String = ""
    Private m_logLevel As System.Diagnostics.TraceLevel = Diagnostics.TraceLevel.Off
    Private m_logger As ApplicationLogger = Nothing
    Private m_punchInUser As punchInUser = Nothing


#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub
    Protected WithEvents lblSDI As System.Web.UI.WebControls.Label
    Protected WithEvents Label1 As System.Web.UI.WebControls.Label

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

        Dim rtn As String = "PunchInSAP(aspx.vb).Page_Load"
        'Try
        '    EventLog.WriteEntry("ISOLPunchIn", rtn & " - Session id = " & Session.SessionID & vbCrLf & _
        '                        "Request.RawURL=" & Request.RawUrl)
        'Catch ex As Exception
        'End Try
        Try


            Session.RemoveAll()

            ' create a log per day
            m_logFilePath = Server.MapPath("") & "\logs\" & Now.Year.ToString("0000") & Now.Month.ToString("00") & Now.Day.ToString("00") & "punchIn.log"
            ' set Log level - not read log level from web.config file
            'Try
            m_logLevel = TraceLevel.Verbose  '   CType(ConfigurationSettings.AppSettings("appLogLevel"), System.Diagnostics.TraceLevel)
            'Catch ex As Exception

            '    EventLog.WriteEntry("ISOLPunchIn Error level 2: ", ex.Message)
            'End Try
            m_logger = New ApplicationLogger(m_logFilePath, m_logLevel)
            m_logger.WriteInformationLog(rtn & " Starting Request " & Now.ToString("MM/dd/yyyy HH:mm:ss"))

            ' log request string
            m_logger.WriteInformationLog(rtn & "::Request.RawURL=" & Request.RawUrl & "; Page.IsPostBack=" & Page.IsPostBack.ToString)
        Catch ex As Exception
            'EventLog.WriteEntry("ISOLPunchIn Error Level 1: ", ex.Message)
        End Try

        Dim sRawUrl As String = Request.RawUrl

        'If Request.QueryString("HOOK_URL") <> "" Then
        '    'according to Honeywell/Hubwoo, USERNAME is reserved word on their 
        '    '   system, so they can't send this parameter. Changing this to say EMAIL instead.
        '    ' - erwin 2008.12.18
        '    'Session("USERNAME") = Request.QueryString("USERNAME")
        '    Session("USERNAME") = Request.QueryString("EMAIL")
        '    Session("PASSWORD") = Request.QueryString("PASSWORD")
        '    Session("COMP_ID") = Request.QueryString("COMP_ID")
        '    Session("USER_ID") = Request.QueryString("USER_ID")
        '    Session("USER_EMAIL") = Request.QueryString("EMAIL")
        '    Session("HOOK_URL") = Request.QueryString("HOOK_URL")
        'Else
        '    'redirect to an error page 
        'End If

        m_punchInUser = New punchInUser

        If Request.QueryString("HOOK_URL") <> "" Then
            Dim s As String = ""
            Dim userEmail As String = ""
            Try
                userEmail = Request.QueryString("EMAIL")
                If (userEmail Is Nothing) Then
                    userEmail = ""
                End If
            Catch ex As Exception
            End Try
            m_punchInUser.EmailAddress = userEmail.Trim
            ' default name to the email address (initially)
            '   then, override with actual user name if supplied
            Dim userName As String = userEmail.Trim
            Try
                s = Request.QueryString("USERNAME")
                If (s Is Nothing) Then
                    s = ""
                End If
                If s.Length > 0 Then
                    userName = s
                End If
            Catch ex As Exception
            End Try
            m_punchInUser.UserName = userName.Trim
            Dim pw As String = ""
            Try
                pw = Request.QueryString("PASSWORD")
                If (pw Is Nothing) Then
                    pw = ""
                End If
            Catch ex As Exception
            End Try
            m_punchInUser.CompanyPassword = pw.Trim
            Dim companyId As String = ""
            Try
                companyId = Request.QueryString("COMP_ID")
                If (companyId Is Nothing) Then
                    companyId = ""
                End If
            Catch ex As Exception
            End Try
            m_punchInUser.CompanyId = companyId.Trim
            Dim userId As String = ""
            Try
                userId = Request.QueryString("USER_ID")
                If (userId Is Nothing) Then
                    userId = ""
                End If
            Catch ex As Exception
            End Try
            m_punchInUser.UserId = userId.Trim
            'code to move user from 'insiteonline/ISOLPunchin' to 'sdiexchange'
            'determine is company Id in the list in web.config key
            ' if yes redirect  to 'sdiexchange'
            Dim strSdiExchList As String = ""
            Dim sNewUrl As String = ""
            If companyId.Trim <> "" Then
                Try
                    If Not ConfigurationSettings.AppSettings("RedirectToSdiExchange") Is Nothing Then
                        strSdiExchList = ConfigurationSettings.AppSettings("RedirectToSdiExchange")
                    Else
                        strSdiExchList = ""
                    End If
                Catch ex As Exception
                    strSdiExchList = ""
                End Try
                If strSdiExchList <> "" Then
                    If strSdiExchList.IndexOf(companyId.Trim) > -1 Then
                        ' build new URL, replace '/isolpunchin' with 'www.sdiexchange.com'
                        sNewUrl = "http://www.sdiexchange.com" & Mid(sRawUrl, 13)
                        Response.Redirect(sNewUrl)
                    End If
                End If
            End If
            Dim hookURL As String = ""
            Try
                hookURL = Request.QueryString("HOOK_URL")
                If (hookURL Is Nothing) Then
                    hookURL = ""
                End If
            Catch ex As Exception
            End Try
            m_punchInUser.HookURL = hookURL.Trim
        Else
            ' ....
        End If

        ' this part is special for Miller Coors sites
        '   for the meantime ...
        '   since Satish's is having a problem setting up SAP to grab logged in user's email, we're going to help by using
        '       theirr user Id and concatenating "@millercoors.com" for their email address
        Dim millerCoorsCompIds As String = "~MIBUY425~MIBUY426~MIBUY427~MIBUY022~MIBUY445~MIBUY446~MIBUY447~MIBUY448"
        If millerCoorsCompIds.IndexOf(m_punchInUser.CompanyId.Trim.ToUpper) > -1 Then
            If m_punchInUser.UserId.Length > 0 Then
                m_punchInUser.EmailAddress = m_punchInUser.UserId & "@millercoors.com"
            End If
        End If

        Session("USER_EMAIL") = m_punchInUser.EmailAddress
        Session("USERNAME") = m_punchInUser.UserName
        Session("PASSWORD") = m_punchInUser.CompanyPassword
        Session("COMP_ID") = m_punchInUser.CompanyId
        Session("USER_ID") = m_punchInUser.UserId
        Session("HOOK_URL") = m_punchInUser.HookURL  '  HOOK_URL

        ' this part is special for ASCEND - VR 07/18/2013
        Dim sWorkOrder As String = ""
        Session("WorkOrderPnchInA") = sWorkOrder ' init SESSION variable

        If m_punchInUser.UserId.Length > 5 Then
            If UCase(Microsoft.VisualBasic.Left(m_punchInUser.UserId, 6)) = "ASCEND" Then
                'Dim sEmailA As String = ""

                'Try
                '    sEmailA = Request.QueryString("Email1")
                '    If (sEmailA Is Nothing) Then
                '        sEmailA = ""
                '    End If
                'Catch ex As Exception
                'End Try
                'Session("EMAILA_PnchIn") = sEmailA
                'm_logger.WriteInformationLog("Extra Email from Ascend: " & sEmailA)

                Try
                    sWorkOrder = Request.QueryString("workorder")
                    If (sWorkOrder Is Nothing) Then
                        sWorkOrder = ""
                    End If
                Catch ex As Exception
                    sWorkOrder = ""
                    m_logger.WriteInformationLog("Error getting 'Work Order' from Ascend.")
                End Try
                m_punchInUser.Workorder = sWorkOrder
                Session("WorkOrderPnchInA") = sWorkOrder
                m_logger.WriteInformationLog("Work Order from Ascend: " & sWorkOrder)

            End If  '  UCase(Microsoft.VisualBasic.Left(m_punchInUser.UserId, 6)) = "ASCEND"
        End If  '  m_punchInUser.UserId.Length > 5 

        'Dim strUserid As String
        'Dim strSiteBU As String
        'Dim strUsername As String

        'If Trim(Request.QueryString("USER_ID")) = "" Then
        '    strUserid = "UnknPI"
        'Else
        '    If Request.QueryString("USER_ID").Length > 8 Then
        '        strUserid = Request.QueryString("USER_ID").Substring(0, 8)
        '    Else
        '        strUserid = Request.QueryString("USER_ID")
        '    End If
        'End If
        If m_punchInUser.UserId.Length = 0 Then
            m_punchInUser.UserId = "UnknPI"
        Else
            If m_punchInUser.UserId.Length > 8 Then
                m_punchInUser.UserId = m_punchInUser.UserId.Substring(0, 8)
            End If
        End If

        'If Trim(Request.QueryString("COMP_ID")) = "" Then
        '    strSiteBU = "NoBUI"
        'Else
        '    If Request.QueryString("COMP_ID").Length > 5 Then
        '        strSiteBU = Request.QueryString("COMP_ID").Substring(0, 5)
        '    Else
        '        strSiteBU = Request.QueryString("COMP_ID")
        '    End If
        'End If
        If m_punchInUser.CompanyId.Length = 0 Then
            m_punchInUser.CompanyId = "NoBUI"
        Else
            If m_punchInUser.CompanyId.Length > 5 Then
                m_punchInUser.CompanyId = m_punchInUser.CompanyId.Substring(0, 5)
            End If
        End If

        ''If Trim(Request.QueryString("USERNAME")) = "" Then
        'If Trim(Request.QueryString("EMAIL")) = "" Then
        '    strUsername = "No Username in Query String"
        'Else
        '    'If Request.QueryString("USERNAME").Length > 40 Then
        '    If Request.QueryString("EMAIL").Length > 40 Then
        '        'strUsername = Request.QueryString("USERNAME").Substring(0, 40)
        '        strUsername = Request.QueryString("EMAIL").Substring(0, 40)
        '    Else
        '        'strUsername = Request.QueryString("USERNAME")
        '        strUsername = Request.QueryString("EMAIL")
        '    End If
        'End If
        If m_punchInUser.UserName.Length = 0 Then
            m_punchInUser.UserName = "No Username in Query String"
        Else
            If m_punchInUser.UserName.Length > 40 Then
                m_punchInUser.UserName = m_punchInUser.UserName.Substring(0, 40)
            End If
        End If

        Dim sHUrl As String = Session("HOOK_URL")
        'm_logger.WriteInformationLog(rtn & "::user Id=" & strUserid & "; user Site(BU)=" & strSiteBU & "; user Name=" & strUsername)
        'WebLog(strUserid, strSiteBU, strUsername)
        m_logger.WriteInformationLog(rtn & _
                                     "::Session id = " & Session.SessionID & _
                                     "::user Id=" & m_punchInUser.UserId & _
                                     "; user Site(BU)=" & m_punchInUser.CompanyId & _
                                     "; HOOK URL=" & m_punchInUser.HookURL & " || " & sHUrl & _
                                     "; user Name=" & m_punchInUser.UserName)
        WebLog(m_punchInUser.UserId, m_punchInUser.CompanyId, m_punchInUser.UserName)

    End Sub


End Class
