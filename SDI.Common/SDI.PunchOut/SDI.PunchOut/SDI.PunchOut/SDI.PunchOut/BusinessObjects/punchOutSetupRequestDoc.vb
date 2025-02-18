Imports System
Imports System.IO
Imports System.Xml



Public Class punchOutSetupRequestDoc

    Implements IDisposable

    Private Const FILE_SETUP_REQUEST_TEMPLATE As String = "PunchOutcXML/punchOutSetupRequestTemplate.xml"

    Private m_docType As String = "http://xml.cxml.org/schemas/cXML/1.2.013/cXML.dtd"
    Private m_payloadId As String = ""
    Private m_timeStamp As String = Now.ToString
    Private m_cXMLversion As String = "1.2.017"
    Private m_identityFrom As punchOutIdentity = Nothing
    Private m_identityTo As punchOutIdentity = Nothing
    Private m_identitySender As punchOutIdentity = Nothing
    Private m_buyer As Buyer = Nothing
    Private m_postbackURL As String = ""
    Private m_shipTo As ShipTo = Nothing
    Private m_templatePathFile As String = ""

    Public Sub New()
        Dim randomId As Single
        Randomize()
        randomId = (100000001 * Rnd())
        m_payloadId = Now.ToString & " " & randomId.ToString("000000000") & "@sdi.com"

        m_templatePathFile = System.AppDomain.CurrentDomain.BaseDirectory.ToString & _
                             FILE_SETUP_REQUEST_TEMPLATE
    End Sub

    Public Property DOCType() As String
        Get
            Return m_docType
        End Get
        Set(ByVal Value As String)
            m_docType = Value
        End Set
    End Property

    Public Property PayloadId() As String
        Get
            Return m_payloadId
        End Get
        Set(ByVal Value As String)
            m_payloadId = Value
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

    Public ReadOnly Property cXMLTemplatePathFile() As String
        Get
            Return m_templatePathFile
        End Get
    End Property

    Public Property PostbackURL() As String
        Get
            Return m_postbackURL
        End Get
        Set(ByVal Value As String)
            m_postbackURL = Value
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

    Public Property BuyerInfo() As Buyer
        Get
            If (m_buyer Is Nothing) Then
                m_buyer = New Buyer
            End If
            Return m_buyer
        End Get
        Set(ByVal Value As Buyer)
            m_buyer = Value
        End Set
    End Property

    Public Property ShipToInfo() As ShipTo
        Get
            If (m_shipTo Is Nothing) Then
                m_shipTo = New ShipTo
            End If
            Return m_shipTo
        End Get
        Set(ByVal Value As ShipTo)
            m_shipTo = Value
        End Set
    End Property

    Public Overrides Function [ToString]() As String
        Dim rtn As String = "punchOutSetupRequestDoc.ToString"
        Dim cXML As String = ""

        Try
            Dim docXML As New XmlDocument
            Dim stringer As theStringer = Nothing

            ' load template for cXML and replace header variables
            stringer = New theStringer(Common.LoadPathFile(m_templatePathFile))

            stringer.Parameters.Add("{__DOCTYPE}", m_docType)
            stringer.Parameters.Add("{__PAYLOAD_ID}", m_payloadId)
            stringer.Parameters.Add("{__TIMESTAMP}", m_timeStamp)

            ' load the template
            docXML.LoadXml(Xml:=stringer.ToString)

            Const tagCreden As String = "Credential"
            Const tagIden As String = "Identity"
            Const tagSecret As String = "SharedSecret"
            Const tagAgent As String = "UserAgent"
            Const attribDomain As String = "domain"
            Const attrib_cXMLversion As String = "version"

            Dim nodeCreden As XmlNode = Nothing
            Dim nodeAgent As XmlNode = Nothing
            Dim node As XmlNode = Nothing
            Dim attrib As XmlAttribute = Nothing

            ' get/set attributes for main node (cXML) if any
            Dim nodeMain As XmlNode = docXML.SelectSingleNode(XPath:="//cXML")

            '   (1) cXML version if specified
            If m_cXMLversion.Length > 0 Then
                attrib = docXML.CreateAttribute(name:=attrib_cXMLversion)
                attrib.Value = m_cXMLversion.Trim
                nodeMain.Attributes.Append(attrib)
            End If

            ' get/set the header.from node
            Dim nodeHdrFrom As XmlNode = docXML.SelectSingleNode(XPath:="//cXML//Header//From")
            nodeCreden = nodeHdrFrom.AppendChild(docXML.CreateElement(name:=tagCreden))
            attrib = nodeCreden.Attributes.Append(docXML.CreateAttribute(name:=attribDomain))
            attrib.Value = Me.FromIdentity.Domain
            node = nodeCreden.AppendChild(docXML.CreateElement(name:=tagIden))
            node.InnerText = Me.FromIdentity.Id
            If Me.FromIdentity.Password.Length > 0 Then
                node = nodeCreden.AppendChild(docXML.CreateElement(name:=tagSecret))
                node.InnerText = Me.FromIdentity.Password
            End If
            If Me.FromIdentity.Agent.Length > 0 Then
                node = nodeHdrFrom.AppendChild(docXML.CreateElement(name:=tagAgent))
                node.InnerText = Me.FromIdentity.Agent
            End If

            ' get/set the header.to node
            Dim nodeHdrTo As XmlNode = docXML.SelectSingleNode(XPath:="//cXML//Header//To")
            nodeCreden = nodeHdrTo.AppendChild(docXML.CreateElement(name:=tagCreden))
            attrib = nodeCreden.Attributes.Append(docXML.CreateAttribute(name:=attribDomain))
            attrib.Value = Me.ToIdentity.Domain
            node = nodeCreden.AppendChild(docXML.CreateElement(name:=tagIden))
            node.InnerText = Me.ToIdentity.Id
            If Me.ToIdentity.Password.Length > 0 Then
                node = nodeCreden.AppendChild(docXML.CreateElement(name:=tagSecret))
                node.InnerText = Me.ToIdentity.Password
            End If
            If Me.ToIdentity.Agent.Length > 0 Then
                node = nodeHdrTo.AppendChild(docXML.CreateElement(name:=tagAgent))
                node.InnerText = Me.ToIdentity.Agent
            End If

            ' get/set the header.sender node
            Dim nodeSender As XmlNode = docXML.SelectSingleNode(XPath:="//cXML//Header//Sender")
            nodeCreden = nodeSender.AppendChild(docXML.CreateElement(name:=tagCreden))
            attrib = nodeCreden.Attributes.Append(docXML.CreateAttribute(name:=attribDomain))
            attrib.Value = Me.SenderIdentity.Domain
            node = nodeCreden.AppendChild(docXML.CreateElement(name:=tagIden))
            node.InnerText = Me.SenderIdentity.Id
            If Me.SenderIdentity.Password.Length > 0 Then
                node = nodeCreden.AppendChild(docXML.CreateElement(name:=tagSecret))
                node.InnerText = Me.SenderIdentity.Password
            End If
            If Me.SenderIdentity.Agent.Length > 0 Then
                node = nodeSender.AppendChild(docXML.CreateElement(name:=tagAgent))
                node.InnerText = Me.SenderIdentity.Agent
            End If

            Dim nodeSetupReq As XmlNode = docXML.SelectSingleNode(XPath:="//cXML//Request//PunchOutSetupRequest")

            ' add buyer cookie
            node = nodeSetupReq.AppendChild(docXML.CreateElement(name:="BuyerCookie"))
            node.InnerText = Me.BuyerInfo.SessionCookie

            ' add Extrinsic nodes
            '   (1) UniqueName
            node = nodeSetupReq.AppendChild(docXML.CreateElement(name:="Extrinsic"))
            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="name"))
            attrib.Value = "UniqueName"
            node.InnerText = Me.BuyerInfo.Name
            '   (2) UserEmail
            node = nodeSetupReq.AppendChild(docXML.CreateElement(name:="Extrinsic"))
            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="name"))
            attrib.Value = "UserEmail"
            node.InnerText = Me.BuyerInfo.EMail
            '   (3) CostCenter
            node = nodeSetupReq.AppendChild(docXML.CreateElement(name:="Extrinsic"))
            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="name"))
            attrib.Value = "CostCenter"
            node.InnerText = Me.BuyerInfo.CostCenter

            ' browser form to post to
            node = nodeSetupReq.AppendChild(docXML.CreateElement(name:="BrowserFormPost"))
            Dim nodeURL As XmlNode = node.AppendChild(docXML.CreateElement(name:="URL"))
            nodeURL.InnerText = m_postbackURL

            ' ship to
            node = nodeSetupReq.AppendChild(docXML.CreateElement(name:="ShipTo"))
            Dim nodeAddr As XmlNode = node.AppendChild(docXML.CreateElement(name:="Address"))
            attrib = nodeAddr.Attributes.Append(docXML.CreateAttribute(name:="addressID"))
            attrib.Value = Me.ShipToInfo.AddressId
            '   (1) Name
            node = nodeAddr.AppendChild(docXML.CreateElement(name:="Name"))
            attrib = node.Attributes.Append(docXML.CreateAttribute("xml", "lang", "xml"))
            attrib.Value = Me.ShipToInfo.LanguageCode
            node.InnerText = Me.ShipToInfo.Name
            '   (2) PostalAddress
            node = nodeAddr.AppendChild(docXML.CreateElement(name:="PostalAddress"))
            '   (2.1) DeliverTo
            Dim nodeAdd2 As XmlNode = node.AppendChild(docXML.CreateElement(name:="DeliverTo"))
            nodeAdd2.InnerText = Me.ShipToInfo.DeliverTo
            '   (2.2) Street
            nodeAdd2 = node.AppendChild(docXML.CreateElement(name:="Street"))
            nodeAdd2.InnerText = Me.ShipToInfo.Street
            '   (2.3) City
            nodeAdd2 = node.AppendChild(docXML.CreateElement(name:="City"))
            nodeAdd2.InnerText = Me.ShipToInfo.City
            '   (2.4) State
            nodeAdd2 = node.AppendChild(docXML.CreateElement(name:="State"))
            nodeAdd2.InnerText = Me.ShipToInfo.State
            '   (2.5) ZIP code
            nodeAdd2 = node.AppendChild(docXML.CreateElement(name:="PostalCode"))
            nodeAdd2.InnerText = Me.ShipToInfo.ZIPCode
            '   (2.6) Country
            nodeAdd2 = node.AppendChild(docXML.CreateElement(name:="Country"))
            attrib = nodeAdd2.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
            attrib.Value = Me.ShipToInfo.CountryCode
            nodeAdd2.InnerText = Me.ShipToInfo.Country

            cXML = docXML.OuterXml
        Catch ex As Exception
            Throw New ApplicationException(rtn & "::" & ex.Message, ex)
        End Try

        Return cXML
    End Function

    Public Shared Function CreatePunchoutSetupRequestDoc(ByVal vendorSettings As punchoutVendorConfig) As punchOutSetupRequestDoc
        Dim doc As punchOutSetupRequestDoc = Nothing
        If Not (vendorSettings Is Nothing) Then
            doc = New punchOutSetupRequestDoc

            doc.DOCType = vendorSettings.DOCType
            doc.cXMLVersion = vendorSettings.cXMLVersion

            doc.FromIdentity = vendorSettings.FromIdentity
            doc.ToIdentity = vendorSettings.ToIdentity
            doc.SenderIdentity = vendorSettings.SenderIdentity
        End If
        Return doc
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
