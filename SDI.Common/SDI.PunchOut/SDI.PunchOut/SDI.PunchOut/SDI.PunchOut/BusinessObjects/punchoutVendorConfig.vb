Imports System.Xml


Public Class punchoutVendorConfig

    Implements IDisposable

    Public Const CONFIG_GROUP_ID_DEFAULT As String = "default"

    Private m_id As String = ""
    Private m_identityFrom As punchOutIdentity = Nothing
    Private m_identityTo As punchOutIdentity = Nothing
    Private m_identitySender As punchOutIdentity = Nothing
    Private m_vendorPunchoutSetupURL As String = ""
    Private m_docType As String = ""
    Private m_cXMLversion As String = ""
    Private m_configPathFile As String = ""
    Private m_bIsValidateAgainstSchema As Boolean = False
    Private m_validateSchemaURL As String = ""
    Private m_logLevel As System.Diagnostics.TraceLevel = TraceLevel.Off
    Private m_configVersion As String = ""
    Private m_groupId As String = SDI.PunchOut.punchoutVendorConfig.CONFIG_GROUP_ID_DEFAULT
    Private m_shipToOverride As ShipTo = Nothing
    Private m_sendUsingWebClient As Boolean = False



    Public Sub New(ByVal Id As String)
        m_id = Id
    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Return m_id
        End Get
    End Property

    Public Property DOCType() As String
        Get
            Return m_docType
        End Get
        Set(ByVal Value As String)
            m_docType = Value
        End Set
    End Property

    Public Property ConfigFileVersion() As String
        Get
            Return m_configVersion
        End Get
        Set(ByVal Value As String)
            m_configVersion = Value
        End Set
    End Property

    Public Property LogLevel() As System.Diagnostics.TraceLevel
        Get
            Return m_logLevel
        End Get
        Set(ByVal Value As System.Diagnostics.TraceLevel)
            m_logLevel = Value
        End Set
    End Property

    Private m_sdiVendorId As String = ""
    Public Property [sdiVendorId]() As String
        Get
            Return m_sdiVendorId
        End Get
        Set(ByVal Value As String)
            m_sdiVendorId = Value
        End Set
    End Property

    Public Property cXMLVersion() As String
        Get
            Return m_cXMLversion
        End Get
        Set(ByVal Value As String)
            m_cXMLversion = Value
        End Set
    End Property

    Public Property FromIdentity() As punchOutIdentity
        Get
            If (m_identityFrom Is Nothing) Then
                m_identityFrom = New punchOutIdentity
            End If
            Return m_identityFrom
        End Get
        Set(ByVal Value As punchOutIdentity)
            m_identityFrom = Value
        End Set
    End Property

    Public Property ToIdentity() As punchOutIdentity
        Get
            If (m_identityTo Is Nothing) Then
                m_identityTo = New punchOutIdentity
            End If
            Return m_identityTo
        End Get
        Set(ByVal Value As punchOutIdentity)
            m_identityTo = Value
        End Set
    End Property

    Public Property SenderIdentity() As punchOutIdentity
        Get
            If (m_identitySender Is Nothing) Then
                m_identitySender = New punchOutIdentity
            End If
            Return m_identitySender
        End Get
        Set(ByVal Value As punchOutIdentity)
            m_identitySender = Value
        End Set
    End Property

    Private m_identityHTTPHeader As punchOutIdentity = Nothing
    Public Property httpHeaderCredential() As punchOutIdentity
        Get
            If (m_identityHTTPHeader Is Nothing) Then
                m_identityHTTPHeader = New punchOutIdentity
            End If
            Return m_identityHTTPHeader
        End Get
        Set(ByVal Value As punchOutIdentity)
            m_identityHTTPHeader = Value
        End Set
    End Property

    Public Property VendorPunchoutSetupURL() As String
        Get
            Return m_vendorPunchoutSetupURL
        End Get
        Set(ByVal Value As String)
            m_vendorPunchoutSetupURL = Value
        End Set
    End Property

    Public Property ConfigFile() As String
        Get
            Return m_configPathFile
        End Get
        Set(ByVal Value As String)
            m_configPathFile = Value
        End Set
    End Property

    Public Property GroupId() As String
        Get
            Return m_groupId
        End Get
        Set(ByVal Value As String)
            m_groupId = Value
        End Set
    End Property

    Public Property IsValidateAgainstSchema() As Boolean
        Get
            Return m_bIsValidateAgainstSchema
        End Get
        Set(ByVal Value As Boolean)
            m_bIsValidateAgainstSchema = Value
        End Set
    End Property

    Public Property ValidateAgainstSchemaURL() As String
        Get
            Return m_validateSchemaURL
        End Get
        Set(ByVal Value As String)
            m_validateSchemaURL = Value
        End Set
    End Property

    Public Property ShipToOverride() As ShipTo
        Get
            If (m_shipToOverride Is Nothing) Then
                m_shipToOverride = New ShipTo
            End If
            Return m_shipToOverride
        End Get
        Set(ByVal Value As ShipTo)

        End Set
    End Property

    Public Property SendUsingWebClient As Boolean
        Get
            Return m_sendUsingWebClient
        End Get
        Set(value As Boolean)
            m_sendUsingWebClient = value
        End Set
    End Property

    Public Shared Function GetVendorConfig(ByVal vendorId As String, Optional ByVal groupId As String = SDI.PunchOut.punchoutVendorConfig.CONFIG_GROUP_ID_DEFAULT) As punchoutVendorConfig
        Dim rtn As String = "punchoutVendorConfig.GetVendorConfig"
        Dim vendorConfig As punchoutVendorConfig = Nothing

        vendorId = vendorId.Trim
        groupId = groupId.Trim

        If vendorId.Length > 0 Then
            Try
                vendorConfig = New punchoutVendorConfig(vendorId)

                ' change the default group if one was specified (and not the default)
                '   if not specified, we'll use the default credential/account group
                If groupId.Length > 0 Then
                    If groupId.ToUpper <> SDI.PunchOut.punchoutVendorConfig.CONFIG_GROUP_ID_DEFAULT.ToUpper Then
                        vendorConfig.GroupId = groupId
                    End If
                End If

                ' config path/file
                Dim xmlDir As String = System.Web.HttpContext.Current.Server.MapPath("") & "\PunchOutcXML\PunchOut.xml"
                'Dim xmlDir As String = System.AppDomain.CurrentDomain.BaseDirectory.ToString & _
                '                       "PunchOutcXML/PunchOut.xml"
                vendorConfig.ConfigFile = xmlDir

                Dim cfg As New XmlDocument
                Dim stringer As theStringer = New theStringer(Common.LoadPathFile(xmlDir))
                cfg.LoadXml(Xml:=stringer.ToString)
                stringer = Nothing

                Dim sId As String = ""
                Dim sGrp As String = ""
                Dim sVal As String = ""

                Dim nodeCatalogs As XmlNode = cfg.SelectSingleNode(XPath:="xmlData")

                If Not (nodeCatalogs Is Nothing) Then
                    Try
                        sVal = nodeCatalogs.Attributes(name:="version").Value
                        If sVal.Trim.Length > 0 Then
                            vendorConfig.ConfigFileVersion = sVal
                        End If
                    Catch ex As Exception
                    End Try
                    Try
                        sVal = nodeCatalogs.Attributes(name:="logging").Value
                        If sVal.Trim.Length > 0 Then
                            vendorConfig.LogLevel = ConvertLogLevel(sVal)
                        End If
                    Catch ex As Exception
                    End Try

                    If nodeCatalogs.ChildNodes.Count > 0 Then
                        For Each vendorCfg As XmlNode In nodeCatalogs.ChildNodes
                            If vendorCfg.NodeType = XmlNodeType.Element And vendorCfg.Name = "Catalog" Then
                                '// get the id.groupId (if exist) for this "catalog"
                                '//     no group id, then this group will be treated as a "default" group for this "catalog"
                                '//     SO in the PunchOut.xml file, the "id" should be in the form "id.groupId" to implemented multi-credential vendor "catalog"
                                '//         (eg., GraybaR can have several pricing group thus several credential to use for logging in)
                                '//  example: id="GRAYBAR" IS EQUAL TO id="GRAYBAR.default"
                                sId = ""
                                sGrp = SDI.PunchOut.punchoutVendorConfig.CONFIG_GROUP_ID_DEFAULT
                                Try
                                    Dim sIdAndGroup As String = vendorCfg.Attributes(name:="id").Value
                                    If sIdAndGroup.Trim.Length > 0 Then
                                        ' check for separator within the Id
                                        If sIdAndGroup.IndexOf(".") > -1 Then
                                            ' parse the Id and GroupId parts
                                            Dim sParts As String() = sIdAndGroup.Trim.Split(CChar("."))
                                            sId = sParts(0)
                                            If sParts.Length > 1 Then
                                                sGrp = sParts(1)
                                            End If
                                        Else
                                            ' take the whole Id as the sole Id
                                            sId = sIdAndGroup.Trim
                                        End If
                                    End If
                                Catch ex As Exception
                                End Try
                                If (sId.Trim.ToUpper & "." & sGrp.Trim.ToUpper) = (vendorConfig.Id.ToUpper & "." & vendorConfig.GroupId.ToUpper) Then
                                    Try
                                        sVal = vendorCfg.Attributes(name:="docType").Value
                                        If sVal.Trim.Length > 0 Then
                                            vendorConfig.DOCType = sVal
                                        End If
                                    Catch ex As Exception
                                    End Try
                                    Try
                                        sVal = vendorCfg.Attributes(name:="docVersion").Value
                                        If sVal.Trim.Length > 0 Then
                                            vendorConfig.cXMLVersion = sVal
                                        End If
                                    Catch ex As Exception
                                    End Try
                                    Try
                                        sVal = vendorCfg.Attributes(name:="logging").Value
                                        If sVal.Trim.Length > 0 Then
                                            vendorConfig.LogLevel = ConvertLogLevel(sVal)
                                        End If
                                    Catch ex As Exception
                                    End Try
                                    Try
                                        sVal = vendorCfg.Attributes(name:="sdiVendorId").Value
                                        If sVal.Trim.Length > 0 Then
                                            vendorConfig.sdiVendorId = sVal
                                        End If
                                    Catch ex As Exception
                                    End Try
                                    Try
                                        sVal = vendorCfg.Attributes(name:="sendUsingWebClient").Value
                                        If sVal.Trim.Length > 0 Then
                                            vendorConfig.SendUsingWebClient = CBool(sVal)
                                        End If
                                    Catch ex As Exception
                                    End Try

                                    Dim node As XmlNode = Nothing
                                    Dim node2 As XmlNode = Nothing

                                    node = vendorCfg.SelectSingleNode("Identities/From")
                                    If Not (node Is Nothing) Then
                                        Try
                                            sVal = node.Attributes(name:="domain").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.FromIdentity.Domain = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="userId").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.FromIdentity.Id = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="userPw").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.FromIdentity.Password = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        node2 = node.SelectSingleNode("UserAgentOverride")
                                        If Not (node2 Is Nothing) Then
                                            Try
                                                sVal = node2.InnerText
                                                If sVal.Trim.Length > 0 Then
                                                    vendorConfig.FromIdentity.Agent = sVal
                                                End If
                                            Catch ex As Exception
                                            End Try
                                        End If
                                    End If

                                    node = vendorCfg.SelectSingleNode("Identities/To")
                                    If Not (node Is Nothing) Then
                                        Try
                                            sVal = node.Attributes(name:="domain").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.ToIdentity.Domain = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="userId").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.ToIdentity.Id = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="userPw").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.ToIdentity.Password = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        node2 = node.SelectSingleNode("UserAgentOverride")
                                        If Not (node2 Is Nothing) Then
                                            Try
                                                sVal = node2.InnerText
                                                If sVal.Trim.Length > 0 Then
                                                    vendorConfig.ToIdentity.Agent = sVal
                                                End If
                                            Catch ex As Exception
                                            End Try
                                        End If
                                    End If

                                    node = vendorCfg.SelectSingleNode("Identities/Sender")
                                    If Not (node Is Nothing) Then
                                        Try
                                            sVal = node.Attributes(name:="domain").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.SenderIdentity.Domain = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="userId").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.SenderIdentity.Id = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="userPw").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.SenderIdentity.Password = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        node2 = node.SelectSingleNode("UserAgentOverride")
                                        If Not (node2 Is Nothing) Then
                                            Try
                                                sVal = node2.InnerText
                                                If sVal.Trim.Length > 0 Then
                                                    vendorConfig.SenderIdentity.Agent = sVal
                                                End If
                                            Catch ex As Exception
                                            End Try
                                        End If
                                    End If

                                    ' http header credentials (if supplied)
                                    node = vendorCfg.SelectSingleNode("Identities/httpHeaderCredential")
                                    If Not (node Is Nothing) Then
                                        Try
                                            sVal = node.Attributes(name:="domain").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.httpHeaderCredential.Domain = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="userId").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.httpHeaderCredential.Id = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="userPw").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.httpHeaderCredential.Password = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        node2 = node.SelectSingleNode("UserAgentOverride")
                                        If Not (node2 Is Nothing) Then
                                            Try
                                                sVal = node2.InnerText
                                                If sVal.Trim.Length > 0 Then
                                                    vendorConfig.httpHeaderCredential.Agent = sVal
                                                End If
                                            Catch ex As Exception
                                            End Try
                                        End If
                                    End If

                                    ' vendor URL
                                    node = vendorCfg.SelectSingleNode("VendorURL")
                                    If Not (node Is Nothing) Then
                                        Try
                                            sVal = node.InnerText
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.VendorPunchoutSetupURL = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                    End If

                                    node = vendorCfg.SelectSingleNode("PunchOutSetupRequest/ShipToOverride")
                                    If Not (node Is Nothing) Then
                                        Try
                                            sVal = node.Attributes(name:="addressId").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.ShipToOverride.AddressId = sVal.Trim
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="addressName").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.ShipToOverride.AddressName = sVal.Trim
                                            End If
                                        Catch ex As Exception
                                        End Try
                                    End If

                                    node = vendorCfg.SelectSingleNode("PunchOutOrderMessage")
                                    If Not (node Is Nothing) Then
                                        Try
                                            sVal = node.Attributes(name:="isValidateAgainstSchema").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.IsValidateAgainstSchema = CBool(sVal)
                                            End If
                                        Catch ex As Exception
                                        End Try
                                        Try
                                            sVal = node.Attributes(name:="validateSchemaURL").Value
                                            If sVal.Trim.Length > 0 Then
                                                vendorConfig.ValidateAgainstSchemaURL = sVal
                                            End If
                                        Catch ex As Exception
                                        End Try
                                    End If

                                End If
                            End If
                        Next
                    End If
                End If

                cfg = Nothing
            Catch ex As Exception
                Throw New ApplicationException(rtn & "::" & ex.Message, ex)
            End Try
        End If

        Return vendorConfig
    End Function

    Private Shared Function ConvertLogLevel(ByVal levelString As String) As System.Diagnostics.TraceLevel
        Dim ret As System.Diagnostics.TraceLevel = TraceLevel.Off
        levelString = levelString.Trim.ToUpper
        If Not (levelString Is Nothing) Then
            If System.Diagnostics.TraceLevel.Verbose.ToString.ToUpper.IndexOf(levelString) > -1 Then
                ' verbose
                ret = TraceLevel.Verbose
            ElseIf (levelString = System.Diagnostics.TraceLevel.Info.ToString.ToUpper) Or _
                   (levelString = "INFORMATION") Then
                ' info
                ret = TraceLevel.Info
            ElseIf System.Diagnostics.TraceLevel.Warning.ToString.ToUpper.IndexOf(levelString) > -1 Then
                ' warn/warning
                ret = TraceLevel.Warning
            ElseIf System.Diagnostics.TraceLevel.Error.ToString.ToUpper.IndexOf(levelString) > -1 Then
                ' error/err
                ret = TraceLevel.Error
            Else
                ' NO LOGGING
                ret = TraceLevel.Off
            End If
        End If
        Return ret
    End Function

#Region " IDisposable Implementations "

    Private m_bIsDisposing As Boolean = False

    Public Sub Dispose() Implements System.IDisposable.Dispose
        If Not m_bIsDisposing Then
            m_bIsDisposing = True
            Try
                m_identityFrom.Dispose()
            Catch ex As Exception
            Finally
                m_identityFrom = Nothing
            End Try
            Try
                m_identityTo.Dispose()
            Catch ex As Exception
            Finally
                m_identityTo = Nothing
            End Try
            Try
                m_identitySender.Dispose()
            Catch ex As Exception
            Finally
                m_identitySender = Nothing
            End Try
        End If
    End Sub

#End Region

End Class
