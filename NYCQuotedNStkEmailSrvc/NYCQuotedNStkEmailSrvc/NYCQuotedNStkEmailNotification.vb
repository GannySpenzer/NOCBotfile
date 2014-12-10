Imports System.ServiceProcess
Imports System.Xml

Public Class NYCQuotedNStkEmailNotification

    Inherits System.ServiceProcess.ServiceBase

    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\configSetting.xml"
    'Private m_sLogSource As String = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).Name
    Private m_sCommonMsgText As String = "NYC Quoted Non-Stock Email Service:"
    Private m_emlAlert As System.Web.Mail.MailMessage
    Private m_sMachineName As String = System.Environment.MachineName

    Private WithEvents m_tmrChkQuotes As System.Timers.Timer


#Region " Component Designer generated code "

    Public Sub New()
        MyBase.New()

        ' This call is required by the Component Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        InitService()
    End Sub

    'UserService overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    ' The main entry point for the process
    <MTAThread()> _
    Shared Sub Main()
        Dim ServicesToRun() As System.ServiceProcess.ServiceBase

        ' More than one NT Service may run within the same process. To add
        ' another service to this process, change the following line to
        ' create a second service object. For example,
        '
        '   ServicesToRun = New System.ServiceProcess.ServiceBase () {New Service1, New MySecondUserService}
        '
        ServicesToRun = New System.ServiceProcess.ServiceBase() {New NYCQuotedNStkEmailNotification}

        System.ServiceProcess.ServiceBase.Run(ServicesToRun)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    ' NOTE: The following procedure is required by the Component Designer
    ' It can be modified using the Component Designer.  Do not modify it
    ' using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        '
        'NYCQuotedNStkEmailNotification
        '
        Me.CanShutdown = True
        Me.ServiceName = "NYCQuotedNStkEmailNotification"

    End Sub

#End Region

#Region " Override Methods "

    Protected Overrides Sub OnStart(ByVal args() As String)
        Try
            ' load config file
            m_xmlConfig = New XmlDocument
            m_xmlConfig.Load(filename:=m_configFile)

            ' initialize timer object for checking quoted non-stock items
            ' default interval to 300,000 milliseconds or 300 secs or 5 mins
            m_tmrChkQuotes = New System.Timers.Timer(300000)
            m_tmrChkQuotes.Enabled = False

            ' search for timer interval property within the config file and use it
            ' still using "secs" unit and doesn't have code to compute for different unit (ie., mins, milliseconds,...)
            If Not (m_xmlConfig.Item("configuration").Item("settingTimer").Attributes("interval").InnerText Is Nothing) Then
                Dim nInterval As Integer = CType(m_xmlConfig.Item("configuration").Item("settingTimer").Attributes("interval").InnerText.Trim, Integer)
                If nInterval > 0 Then
                    m_tmrChkQuotes.Interval = (nInterval * 1000)
                End If
            End If

            ' initialize the alert message for sending status alerts for this service
            m_emlAlert = InitAlertEmailMsg(m_xmlConfig)

            ' send notification that this service starts successfully
            m_emlAlert.Body = "Service STARTED at " & m_sMachineName & " " & System.DateTime.Now.ToString
            System.Web.Mail.SmtpMail.Send(message:=m_emlAlert)

            ' start/enable our timer object
            m_tmrChkQuotes.Start()

        Catch ex As Exception
            MyBase.EventLog.WriteEntry(m_sCommonMsgText & vbCrLf & ex.ToString, EventLogEntryType.Error)
        End Try
    End Sub

    Protected Overrides Sub OnStop()
        Try
            ' stop the timer object
            m_tmrChkQuotes.Stop()

            ' check for the instance of the alert email message, create if needed
            If m_emlAlert Is Nothing Then
                m_emlAlert = InitAlertEmailMsg(m_xmlConfig)
            End If

            ' insert the body of the message and send the message
            m_emlAlert.Body = "Service STOPPED at " & m_sMachineName & " " & System.DateTime.Now.ToString
            m_emlAlert.Priority = Web.Mail.MailPriority.High
            System.Web.Mail.SmtpMail.Send(message:=m_emlAlert)

            ' clean-up
            m_emlAlert = Nothing

            m_tmrChkQuotes.Close()
            m_tmrChkQuotes.Dispose()

            m_xmlConfig = Nothing

        Catch ex As Exception
            MyBase.EventLog.WriteEntry(m_sCommonMsgText & vbCrLf & ex.ToString, EventLogEntryType.Error)
        End Try
    End Sub

    Protected Overrides Sub OnShutdown()
        Try
            ' stop the timer object
            m_tmrChkQuotes.Stop()

            ' check for the instance of the alert email message, create if needed
            If m_emlAlert Is Nothing Then
                m_emlAlert = InitAlertEmailMsg(m_xmlConfig)
            End If

            ' insert the body of the message and send the message
            m_emlAlert.Body = "Service receives shutdown message from host." & vbCrLf & _
                              m_sMachineName & " " & System.DateTime.Now.ToString
            m_emlAlert.Priority = Web.Mail.MailPriority.High
            System.Web.Mail.SmtpMail.Send(message:=m_emlAlert)

            '' clean-up
            'm_emlAlert = Nothing

            'm_tmrChkQuotes.Close()
            'm_tmrChkQuotes.Dispose()

            'm_xmlConfig = Nothing

        Catch ex As Exception
            MyBase.EventLog.WriteEntry(m_sCommonMsgText & vbCrLf & ex.ToString, EventLogEntryType.Error)
        End Try
    End Sub

#End Region

#Region " Private Methods "

    Private Sub InitService()

        ' add code to initialize objects that will exist with the duration of this service itself

    End Sub

    Private Function InitAlertEmailMsg(ByVal xmlConfig As XmlDocument) As System.Web.Mail.MailMessage
        Try
            Dim eml As New System.Web.Mail.MailMessage
            Dim SrvcNotification As XmlElement

            eml.Subject = ""
            eml.From = ""
            eml.To = ""
            eml.Body = ""

            If Not (xmlConfig("configuration")("serviceNotification") Is Nothing) Then
                SrvcNotification = xmlConfig("configuration")("serviceNotification")
            End If

            If Not (SrvcNotification Is Nothing) Then
                ' get the subject line for this service notification messages
                If Not (SrvcNotification.Attributes("notifySubject").InnerText Is Nothing) Then
                    eml.Subject = SrvcNotification.Attributes("notifySubject").InnerText.Trim
                End If

                ' get sender email address (automated)
                If Not (SrvcNotification.Attributes("notifyFrom").InnerText Is Nothing) Then
                    eml.From = SrvcNotification.Attributes("notifyFrom").InnerText.Trim
                End If

                ' get email address list on whom will receives this notification
                If Not (SrvcNotification.ChildNodes Is Nothing) Then
                    If SrvcNotification.ChildNodes.Count > 0 Then
                        For Each nodeTO As System.Xml.XmlNode In SrvcNotification.ChildNodes
                            If nodeTO.Name = "statusNotify" And Not (nodeTO.Attributes("addy").InnerText Is Nothing) Then
                                If nodeTO.Attributes("addy").InnerText.Trim.Length > 0 Then
                                    eml.To &= nodeTO.Attributes("addy").InnerText.Trim & ";"
                                End If
                            End If
                        Next
                    End If
                End If
            End If

            Return eml

        Catch ex As Exception
            MyBase.EventLog.WriteEntry(m_sCommonMsgText & vbCrLf & ex.ToString, EventLogEntryType.Error)
        End Try
    End Function

#End Region

    Private Sub m_tmrChkQuotes_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_tmrChkQuotes.Elapsed
        Try
            ' stop the timer since this process may take time to accomplish
            m_tmrChkQuotes.Stop()

            ' retrieve the source DB connection string to use
            Dim cnString As String = ""
            If Not (m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText Is Nothing) Then
                cnString = m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText.Trim
            End If

            ' instantiate the process engine for checking quoted non-stock and processing messages
            Dim quoteProcess As New QuoteNonStockProcessor(cnString)

            ' assign needed configuration values
            With quoteProcess
                .EventLogger = MyBase.EventLog

                If .Evaluate Then
                    Dim xmlEmailElement As XmlElement

                    ' get encryption key for email use
                    If Not (m_xmlConfig("configuration")("s").Attributes("id").InnerText Is Nothing) Then
                        If m_xmlConfig("configuration")("s").Attributes("id").InnerText.Trim.Length > 0 Then
                            .EncryptionKey = m_xmlConfig("configuration")("s").Attributes("id").InnerText.Trim
                        End If
                    End If

                    ' get "email" node of the configuration file
                    If Not (m_xmlConfig("configuration")("email") Is Nothing) Then
                        xmlEmailElement = m_xmlConfig("configuration")("email")
                    End If

                    If Not (xmlEmailElement Is Nothing) Then
                        ' get the default sender properties
                        If Not (xmlEmailElement("defaultFrom").Attributes("addy").InnerText Is Nothing) Then
                            .defaultMsgFROM = xmlEmailElement("defaultFrom").Attributes("addy").InnerText.Trim
                        End If

                        ' get additional recipient (as TO) email addresses
                        If Not (xmlEmailElement("additionalTo").ChildNodes Is Nothing) Then
                            If xmlEmailElement("additionalTo").ChildNodes.Count > 0 Then
                                For Each toItem As XmlNode In xmlEmailElement("additionalTo").ChildNodes
                                    If toItem.Name = "toItem" And Not (toItem.Attributes("addy").InnerText Is Nothing) Then
                                        If toItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                            .xTO.Add(toItem.Attributes("addy").InnerText.Trim)
                                        End If
                                    End If
                                Next
                            End If
                        End If

                        ' get additional recipient (as CC) email addresses
                        If Not (xmlEmailElement("additionalCc").ChildNodes Is Nothing) Then
                            If xmlEmailElement("additionalCc").ChildNodes.Count > 0 Then
                                For Each ccItem As XmlNode In xmlEmailElement("additionalCc").ChildNodes
                                    If ccItem.Name = "ccItem" And Not (ccItem.Attributes("addy").InnerText Is Nothing) Then
                                        If ccItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                            .xCC.Add(ccItem.Attributes("addy").InnerText.Trim)
                                        End If
                                    End If
                                Next
                            End If
                        End If

                        ' get additional recipient (as BCC) email addresses
                        If Not (xmlEmailElement("additionalBcc").ChildNodes Is Nothing) Then
                            If xmlEmailElement("additionalBcc").ChildNodes.Count > 0 Then
                                For Each bccItem As XmlNode In xmlEmailElement("additionalBcc").ChildNodes
                                    If bccItem.Name = "bccItem" And Not (bccItem.Attributes("addy").InnerText Is Nothing) Then
                                        If bccItem.Attributes("addy").InnerText.Trim.Length > 0 Then
                                            .xBCC.Add(bccItem.Attributes("addy").InnerText.Trim)
                                        End If
                                    End If
                                Next
                            End If
                        End If

                        ' get the default subject for this email(s)
                        If Not (xmlEmailElement("defaultSubject").Attributes("text").InnerText Is Nothing) Then
                            .SubjectLine = xmlEmailElement("defaultSubject").Attributes("text").InnerText.Trim
                        End If

                        ' get the URL to use for approving this quoted item
                        If Not (xmlEmailElement("body").Attributes("linkURL").InnerText Is Nothing) Then
                            .URL = xmlEmailElement("body").Attributes("linkURL").InnerText.Trim
                        End If
                    End If

                    ' start processing
                    .Execute()
                End If
            End With

            ' destroy the instance of the process engine
            quoteProcess = Nothing

            ' re-start the timer before going out
            m_tmrChkQuotes.Start()

        Catch ex As Exception
            MyBase.EventLog.WriteEntry(m_sCommonMsgText & vbCrLf & ex.ToString, EventLogEntryType.Error)
        End Try
    End Sub

End Class
