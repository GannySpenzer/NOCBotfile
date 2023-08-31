Imports System
Imports System.IO
Imports System.Data
Imports System.Data.OleDb
Imports System.Xml



Public Class punchOutSetupRequestDoc

    Implements IDisposable

    Private Const FILE_SETUP_REQUEST_TEMPLATE As String = "PunchOutcXML/punchOutSetupRequestTemplate.xml"

    Private Const SETUP_ORDER_REQUEST_TEMPLATE As String = "PunchOutcXML/OrderRequestTemplate.xml"

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

    Public Sub New(ByVal strType As String)
        Dim randomId As Single
        Randomize()
        randomId = (100000001 * Rnd())

        Select Case strType
            Case "OrderRequest"
                m_templatePathFile = System.AppDomain.CurrentDomain.BaseDirectory.ToString & _
                                     SETUP_ORDER_REQUEST_TEMPLATE
                Dim sNow As String = ""
                Dim dateOffset As New DateTimeOffset(Now, TimeZoneInfo.Local.GetUtcOffset(Now))
                sNow = dateOffset.ToString("o")
                m_payloadId = sNow & "." & randomId.ToString("000000000") & "." & "OrderRequest@sdi.com"
            Case Else
                m_templatePathFile = System.AppDomain.CurrentDomain.BaseDirectory.ToString & _
                                     FILE_SETUP_REQUEST_TEMPLATE
                m_payloadId = Now.ToString & " " & randomId.ToString("000000000") & "@sdi.com"
        End Select
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

    Public Shared Function CreateOrderRequestDoc(ByVal vendorSettings As punchoutVendorConfig) As punchOutSetupRequestDoc
        Dim doc As punchOutSetupRequestDoc = Nothing
        If Not (vendorSettings Is Nothing) Then
            doc = New punchOutSetupRequestDoc("OrderRequest")

            doc.DOCType = vendorSettings.DOCType
            doc.cXMLVersion = vendorSettings.cXMLVersion

            doc.FromIdentity = vendorSettings.FromIdentity
            doc.ToIdentity = vendorSettings.ToIdentity
            doc.SenderIdentity = vendorSettings.SenderIdentity
        End If
        Return doc
    End Function

    Public Function CreateOrderRequestXML(ByVal connectOR As OleDbConnection, ByVal strOrderNo As String) As String
        Dim rtn As String = "CreateOrderRequestXML"
        Dim cXML As String = ""

        Try
            Dim docXML As New XmlDocument
            Dim stringer As theStringer = Nothing
            ' load template for cXML and replace header variables
            stringer = New theStringer(Common.LoadPathFile(m_templatePathFile))

            stringer.Parameters.Add("{__DOCTYPE}", m_docType)
            stringer.Parameters.Add("{__PAYLOAD_ID}", m_payloadId)
            Dim dateOffset As New DateTimeOffset(Now, TimeZoneInfo.Local.GetUtcOffset(Now))
            m_timeStamp = dateOffset.ToString("o")
            stringer.Parameters.Add("{__TIMESTAMP}", m_timeStamp)

            ' load the template
            docXML.LoadXml(xml:=stringer.ToString)

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
            Dim nodeMain As XmlNode = docXML.SelectSingleNode(xpath:="//cXML")

            '   (1) cXML version if specified
            If m_cXMLversion.Length > 0 Then
                attrib = docXML.CreateAttribute(name:=attrib_cXMLversion)
                attrib.Value = m_cXMLversion.Trim
                nodeMain.Attributes.Append(attrib)
            End If

            ' get/set the header.from node
            Dim nodeHdrFrom As XmlNode = docXML.SelectSingleNode(xpath:="//cXML//Header//From")
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
            Dim nodeHdrTo As XmlNode = docXML.SelectSingleNode(xpath:="//cXML//Header//To")
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
            Dim nodeSender As XmlNode = docXML.SelectSingleNode(xpath:="//cXML//Header//Sender")
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

            Try

                ' get existing node Request
                Dim nodeOrderReq As XmlNode = docXML.SelectSingleNode(xpath:="//cXML//Request")

                ' for each order get all order info including lines info
                Dim strOrder As String = "select * from SYSADM8.PS_ISA_PO_DISP_XML where po_id='" & strOrderNo & "' and business_unit = 'EMC00'"
                ' Dim strOrder As String = "select * from SYSADM8.PS_ISA_PO_DISP_XML WHERE PO_ID = 'E015087861'"
                If Not connectOR.State = ConnectionState.Open Then
                    connectOR.Open()
                End If
                Dim OrdCommand As OleDbCommand = New OleDbCommand(strOrder, connectOR)
                OrdCommand.CommandTimeout = 120
                Dim OrdDataAdapter As OleDbDataAdapter = New OleDbDataAdapter(OrdCommand)

                Dim OrderDataSet As System.Data.DataSet = New System.Data.DataSet()

                OrdDataAdapter.Fill(OrderDataSet)

                If Not OrderDataSet Is Nothing Then
                    If OrderDataSet.Tables.Count > 0 Then
                        If OrderDataSet.Tables(0).Rows.Count > 0 Then

                            ' create order top node
                            Dim nodeOrder As XmlNode = nodeOrderReq.AppendChild(docXML.CreateElement(name:="OrderRequest"))

                            Dim iOrd As Integer = 0
                            For iOrd = 0 To OrderDataSet.Tables(0).Rows.Count - 1

                                If iOrd = 0 Then
                                    'create order header node
                                    Dim nodeOrderHeader As XmlNode = nodeOrder.AppendChild(docXML.CreateElement(name:="OrderRequestHeader"))

                                    'add attributes
                                    Dim strPoDate As String = OrderDataSet.Tables(0).Rows(iOrd).Item("PO_DT").ToString()
                                    Dim dDate1 As DateTime = CType(strPoDate, DateTime)
                                    Dim dateOffset1 As New DateTimeOffset(dDate1, TimeZoneInfo.Local.GetUtcOffset(dDate1))
                                    strPoDate = dateOffset1.ToString("o")
                                    ' (1) orderDate
                                    attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="orderDate"))
                                    attrib.Value = strPoDate ' get from dataset

                                    Dim strPoId As String = OrderDataSet.Tables(0).Rows(iOrd).Item("PO_ID").ToString()
                                    '(2) orderID
                                    attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="orderID"))
                                    attrib.Value = strPoId ' get from dataset

                                    Dim strPoOrderType As String = OrderDataSet.Tables(0).Rows(iOrd).Item("ORDER_TYPE").ToString()
                                    '(3) orderType
                                    attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="orderType"))
                                    attrib.Value = LCase(strPoOrderType)  '  "regular"

                                    Dim strPoVersion As String = OrderDataSet.Tables(0).Rows(iOrd).Item("ORDER_VERSION").ToString()
                                    ' (4) orderVersion
                                    attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="orderVersion"))
                                    attrib.Value = strPoVersion  '  "1"

                                    Dim strPoType As String = OrderDataSet.Tables(0).Rows(iOrd).Item("TYPE").ToString()
                                    '(5) type
                                    attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="type"))
                                    attrib.Value = LCase(strPoType)  '  "new"

                                    ' Total
                                    Dim nodeTotal As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="Total"))
                                    ' Money - under Total
                                    node = nodeTotal.AppendChild(docXML.CreateElement(name:="Money"))
                                    Dim strMoneyTotal As String = OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                                    Dim strCurrnc As String = OrderDataSet.Tables(0).Rows(iOrd).Item("CURRENCY").ToString()
                                    ' (1) currency
                                    attrib = node.Attributes.Append(docXML.CreateAttribute(name:="currency"))
                                    attrib.Value = strCurrnc ' get from dataset
                                    node.InnerText = strMoneyTotal ' get from dataset Money

                                    'ShipTo
                                    Dim nodeShipTo As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="ShipTo"))
                                    '    Address - under ShipTo
                                    Dim nodeAddressShipTo As XmlNode = nodeShipTo.AppendChild(docXML.CreateElement(name:="Address"))
                                    '       (1) addressID
                                    attrib = nodeAddressShipTo.Attributes.Append(docXML.CreateAttribute(name:="addressID"))
                                    Dim straddressID As String = OrderDataSet.Tables(0).Rows(iOrd).Item("SHIPTO_ID").ToString()
                                    attrib.Value = straddressID ' ShipToID -  get from dataset - like 'L0470-01'

                                    Dim strIsoCountryCode As String = OrderDataSet.Tables(0).Rows(iOrd).Item("COUNTRY_2CHAR").ToString()
                                    '       (2) isoCountryCode
                                    attrib = nodeAddressShipTo.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                                    attrib.Value = strIsoCountryCode ' get from dataset
                                    '       name - under address - under ShipTo
                                    Dim nodeName As XmlNode = nodeAddressShipTo.AppendChild(docXML.CreateElement(name:="Name"))
                                    '          (1) xml:lang - under name
                                    attrib = nodeName.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                                    attrib.Value = "en-US"
                                    Dim strShipToName As String = OrderDataSet.Tables(0).Rows(iOrd).Item("DESCR").ToString()
                                    nodeName.InnerText = strShipToName ' get from dataset Money


                                    '       PostalAddress - under address - under ShipTo
                                    Dim nodePostalAddress As XmlNode = nodeAddressShipTo.AppendChild(docXML.CreateElement(name:="PostalAddress"))
                                    '          (1) name
                                    attrib = nodePostalAddress.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                    attrib.Value = "default"

                                    '         DeliverTo under PostalAddress - under address - under ShipTo
                                    node = nodePostalAddress.AppendChild(docXML.CreateElement(name:="DeliverTo"))
                                    Dim strDeliverTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("ADDRESS1").ToString()
                                    node.InnerText = strDeliverTo

                                    '         Street under PostalAddress - under address - under ShipTo
                                    node = nodePostalAddress.AppendChild(docXML.CreateElement(name:="Street"))
                                    Dim strStreet2 As String = OrderDataSet.Tables(0).Rows(iOrd).Item("ADDRESS2").ToString()
                                    node.InnerText = strStreet2

                                    '         Street under PostalAddress - under address - under ShipTo
                                    node = nodePostalAddress.AppendChild(docXML.CreateElement(name:="Street"))
                                    Dim strStreet3 As String = OrderDataSet.Tables(0).Rows(iOrd).Item("ADDRESS3").ToString()
                                    node.InnerText = strStreet3

                                    '         City under PostalAddress - under address - under ShipTo
                                    node = nodePostalAddress.AppendChild(docXML.CreateElement(name:="City"))
                                    Dim strCity As String = OrderDataSet.Tables(0).Rows(iOrd).Item("CITY").ToString()
                                    node.InnerText = strCity

                                    '         State under PostalAddress - under address - under ShipTo
                                    node = nodePostalAddress.AppendChild(docXML.CreateElement(name:="State"))
                                    Dim strState As String = OrderDataSet.Tables(0).Rows(iOrd).Item("STATE").ToString()
                                    node.InnerText = strState

                                    '         PostalCode under PostalAddress - under address - under ShipTo
                                    node = nodePostalAddress.AppendChild(docXML.CreateElement(name:="PostalCode"))
                                    Dim strPostalCode As String = OrderDataSet.Tables(0).Rows(iOrd).Item("POSTAL").ToString()
                                    node.InnerText = strPostalCode ' get from dataset

                                    '         Country under PostalAddress - under address - under ShipTo
                                    node = nodePostalAddress.AppendChild(docXML.CreateElement(name:="Country"))
                                    '       (1) isoCountryCode
                                    attrib = node.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                                    attrib.Value = strIsoCountryCode ' get from dataset
                                    Dim strCountry As String = OrderDataSet.Tables(0).Rows(iOrd).Item("COUNTRY_CODE").ToString()
                                    node.InnerText = strCountry ' get from dataset


                                    '       Email - under address - under ShipTo
                                    Dim nodeEmail As XmlNode = nodeAddressShipTo.AppendChild(docXML.CreateElement(name:="Email"))
                                    '            (1) name
                                    attrib = nodeEmail.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                    attrib.Value = "default"
                                    Dim strEmail As String = ""
                                    If Not OrderDataSet.Tables(0).Rows(iOrd).Item("EMAILID").ToString() Is Nothing Then
                                        Try
                                            If Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("EMAILID").ToString()) <> "" Then
                                                strEmail = Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("EMAILID").ToString())
                                            Else
                                                strEmail = ""
                                            End If
                                        Catch ex As Exception
                                            strEmail = ""
                                        End Try
                                    End If
                                    If Trim(strEmail) = "" Then
                                        strEmail = "WebDev@sdi.com"
                                    End If
                                    nodeEmail.InnerText = strEmail

                                    '       Phone - under address - under ShipTo
                                    Dim nodePhone As XmlNode = nodeAddressShipTo.AppendChild(docXML.CreateElement(name:="Phone"))
                                    '            (1) name
                                    attrib = nodePhone.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                    attrib.Value = "default"
                                    '         TelephoneNumber under Phone - under address - under ShipTo
                                    Dim nodeTelephoneNumber As XmlNode = nodePhone.AppendChild(docXML.CreateElement(name:="TelephoneNumber"))
                                    '           CountryCode under TelephoneNumber under Phone - under address - under ShipTo
                                    node = nodeTelephoneNumber.AppendChild(docXML.CreateElement(name:="CountryCode"))
                                    '                (1) isoCountryCode
                                    attrib = node.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                                    attrib.Value = strIsoCountryCode ' get from dataset
                                    Dim strCountryCodeShipTo As String = ""  '  OrderDataSet.Tables(0).Rows(iOrd).Item("????").ToString()
                                    Dim strAreaOrCityCode As String = ""  '  OrderDataSet.Tables(0).Rows(iOrd).Item("area_code").ToString()
                                    Dim strNumberSuffix As String = ""  '  OrderDataSet.Tables(0).Rows(iOrd).Item("phone_suffix").ToString()
                                    Dim strPhoneShipTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("PHONE").ToString()
                                    'Dim iPhoneLength As Integer = 0
                                    Try
                                        If Not strPhoneShipTo Is Nothing Then
                                            If Trim(strPhoneShipTo) <> "" Then
                                                strPhoneShipTo = Trim(strPhoneShipTo)
                                                If IsNumeric(strPhoneShipTo) Then
                                                    Call GetShipToPhoneParts(strPhoneShipTo, strCountryCodeShipTo, strAreaOrCityCode, strNumberSuffix)
                                                Else ' not numeric
                                                    Dim strMyPhn1 As String = strPhoneShipTo
                                                    strMyPhn1 = Replace(strMyPhn1, "(", "")
                                                    strMyPhn1 = Replace(strMyPhn1, ")", "")
                                                    strMyPhn1 = Replace(strMyPhn1, "-", "")
                                                    If Trim(strMyPhn1) <> "" Then
                                                        If IsNumeric(strMyPhn1) Then
                                                            Call GetShipToPhoneParts(strMyPhn1, strCountryCodeShipTo, strAreaOrCityCode, strNumberSuffix)
                                                        Else ' still not numeric
                                                            strCountryCodeShipTo = ""
                                                            strAreaOrCityCode = ""
                                                            strNumberSuffix = strPhoneShipTo
                                                        End If
                                                    Else
                                                        strCountryCodeShipTo = ""
                                                        strAreaOrCityCode = ""
                                                        strNumberSuffix = ""
                                                    End If
                                                End If

                                            End If
                                        End If
                                    Catch ex As Exception
                                        strCountryCodeShipTo = ""
                                        strAreaOrCityCode = ""
                                        strNumberSuffix = ""
                                    End Try

                                    node.InnerText = strCountryCodeShipTo

                                    '           AreaOrCityCode under TelephoneNumber under Phone - under address - under ShipTo
                                    node = nodeTelephoneNumber.AppendChild(docXML.CreateElement(name:="AreaOrCityCode"))
                                    node.InnerText = strAreaOrCityCode

                                    '           Number under TelephoneNumber under Phone - under address - under ShipTo
                                    node = nodeTelephoneNumber.AppendChild(docXML.CreateElement(name:="Number"))
                                    node.InnerText = strNumberSuffix
                                    'IPM-145 Need to ensure Amazon CXML process works
                                    'Fetching Account Value
                                    Dim strAccount As String = ""
                                    If Not OrderDataSet.Tables(0).Rows(iOrd).Item("ACCOUNT").ToString() Is Nothing Then
                                        Try
                                            If Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("ACCOUNT").ToString()) <> "" Then
                                                strAccount = Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("ACCOUNT").ToString())
                                            Else
                                                strAccount = ""
                                            End If
                                        Catch ex As Exception
                                            strAccount = ""
                                        End Try
                                    End If

                                    ' Extrinsic for Account
                                    Dim nodeExtrAccount As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="Extrinsic"))
                                    '            (1) name
                                    attrib = nodeExtrAccount.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                    attrib.Value = "Account"
                                    nodeExtrAccount.InnerText = strAccount

                                    ' Extrinsic for e-mail
                                    Dim nodeExtrEmail As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="Extrinsic"))
                                    '            (1) name
                                    attrib = nodeExtrEmail.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                    attrib.Value = "default"
                                    nodeExtrEmail.InnerText = strEmail

                                    'BillTo
                                    Dim nodeBillTo As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="BillTo"))
                                    '    Address - under BillTo
                                    Dim nodeAddressBillTo As XmlNode = nodeBillTo.AppendChild(docXML.CreateElement(name:="Address"))
                                    '       (1) addressID
                                    attrib = nodeAddressBillTo.Attributes.Append(docXML.CreateAttribute(name:="addressID"))
                                    Dim straddressIDBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_ID").ToString()
                                    attrib.Value = straddressIDBillTo ' BillToID -  get from dataset - like 'L0470-01'

                                    'Dim strIsoCountryCode As String = OrderDataSet.Tables(0).Rows(iOrd).Item("country").ToString()
                                    '       (2) isoCountryCode
                                    attrib = nodeAddressBillTo.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                                    attrib.Value = strIsoCountryCode ' get from dataset
                                    '       name - under address - under BillTo
                                    Dim nodeNameBillTo As XmlNode = nodeAddressBillTo.AppendChild(docXML.CreateElement(name:="Name"))
                                    '          (1) xml:lang - under name
                                    attrib = nodeNameBillTo.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                                    attrib.Value = "en-US"
                                    Dim strBillToName As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_NAME").ToString()
                                    nodeNameBillTo.InnerText = strBillToName ' get from dataset Money


                                    '       PostalAddress - under address - under BillTo
                                    Dim nodePostalAddressBillTo As XmlNode = nodeAddressBillTo.AppendChild(docXML.CreateElement(name:="PostalAddress"))
                                    '          (1) name
                                    attrib = nodePostalAddressBillTo.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                    attrib.Value = "default"

                                    '         DeliverTo under PostalAddress - under address - under BillTo
                                    node = nodePostalAddressBillTo.AppendChild(docXML.CreateElement(name:="DeliverTo"))
                                    node.InnerText = strBillToName

                                    '         DeliverTo (2nd time) under PostalAddress - under address - under BillTo
                                    node = nodePostalAddressBillTo.AppendChild(docXML.CreateElement(name:="DeliverTo"))
                                    Dim strDeliverToBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_NAME").ToString()
                                    node.InnerText = strDeliverToBillTo

                                    '         Street under PostalAddress - under address - under BillTo
                                    node = nodePostalAddressBillTo.AppendChild(docXML.CreateElement(name:="Street"))
                                    Dim strStreet2BillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_ADD1").ToString()
                                    node.InnerText = strStreet2BillTo

                                    '         Street under PostalAddress - under address - under BillTo
                                    node = nodePostalAddressBillTo.AppendChild(docXML.CreateElement(name:="Street"))
                                    Dim strStreet3BillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_ADD2").ToString()
                                    node.InnerText = strStreet3BillTo

                                    '         City under PostalAddress - under address - under BillTo
                                    node = nodePostalAddressBillTo.AppendChild(docXML.CreateElement(name:="City"))
                                    Dim strCityBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_CITY").ToString()
                                    node.InnerText = strCityBillTo

                                    '         State under PostalAddress - under address - under BillTo
                                    node = nodePostalAddressBillTo.AppendChild(docXML.CreateElement(name:="State"))
                                    Dim strStateBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_STATE").ToString()
                                    node.InnerText = strStateBillTo

                                    '         PostalCode under PostalAddress - under address - under BillTo
                                    node = nodePostalAddressBillTo.AppendChild(docXML.CreateElement(name:="PostalCode"))
                                    Dim strPostalCodeBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_POSTAL").ToString()
                                    node.InnerText = strPostalCodeBillTo ' get from dataset

                                    '         Country under PostalAddress - under address - under BillTo
                                    node = nodePostalAddressBillTo.AppendChild(docXML.CreateElement(name:="Country"))
                                    '       (1) isoCountryCode
                                    attrib = node.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                                    attrib.Value = strIsoCountryCode ' get from dataset
                                    'Dim strCountryBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("name").ToString()
                                    node.InnerText = strCountry ' get from dataset


                                    '       Email - under address - under BillTo
                                    Dim nodeEmailBillTo As XmlNode = nodeAddressBillTo.AppendChild(docXML.CreateElement(name:="Email"))
                                    '            (1) name
                                    attrib = nodeEmailBillTo.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                    attrib.Value = "default"
                                    Dim strEmailBillTo As String = ""
                                    If Not OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_EMAILID").ToString() Is Nothing Then
                                        Try
                                            If Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_EMAILID").ToString()) <> "" Then
                                                strEmailBillTo = Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_EMAILID").ToString())
                                            Else
                                                strEmailBillTo = ""
                                            End If
                                        Catch ex As Exception
                                            strEmailBillTo = ""
                                        End Try
                                    End If
                                    nodeEmailBillTo.InnerText = strEmailBillTo

                                    '       Phone - under address - under BillTo
                                    Dim nodePhoneBillTo As XmlNode = nodeAddressBillTo.AppendChild(docXML.CreateElement(name:="Phone"))
                                    '            (1) name
                                    attrib = nodePhoneBillTo.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                    attrib.Value = "default"
                                    '         TelephoneNumber under Phone - under address - under BillTo
                                    Dim nodeTelephoneNumberBillTo As XmlNode = nodePhoneBillTo.AppendChild(docXML.CreateElement(name:="TelephoneNumber"))
                                    '           CountryCode under TelephoneNumber under Phone - under address - under BillTo
                                    node = nodeTelephoneNumberBillTo.AppendChild(docXML.CreateElement(name:="CountryCode"))
                                    '                (1) isoCountryCode
                                    attrib = node.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                                    attrib.Value = strIsoCountryCode ' get from dataset
                                    Dim strCountryCodeBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_PHONE1").ToString()
                                    node.InnerText = strCountryCodeBillTo  '  strCountryCode

                                    '           AreaOrCityCode under TelephoneNumber under Phone - under address - under BillTo
                                    node = nodeTelephoneNumberBillTo.AppendChild(docXML.CreateElement(name:="AreaOrCityCode"))
                                    Dim strAreaOrCityCodeBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_PHONE2").ToString()
                                    node.InnerText = strAreaOrCityCodeBillTo  '  strAreaOrCityCode

                                    '           Number under TelephoneNumber under Phone - under address - under BillTo
                                    node = nodeTelephoneNumberBillTo.AppendChild(docXML.CreateElement(name:="Number"))
                                    Dim strNumberSuffixBillTo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("BILLTO_PHONE3").ToString()
                                    node.InnerText = strNumberSuffixBillTo  '  strNumberSuffix

                                    'Shipping
                                    Dim nodeShipping As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="Shipping"))
                                    ' Money - under Shipping
                                    node = nodeShipping.AppendChild(docXML.CreateElement(name:="Money"))
                                    ' (1) currency
                                    attrib = node.Attributes.Append(docXML.CreateAttribute(name:="currency"))
                                    attrib.Value = strCurrnc ' get from dataset
                                    node.InnerText = "0.00"
                                    'Description
                                    node = nodeShipping.AppendChild(docXML.CreateElement(name:="Description"))
                                    ' (1) xml:lang - under Description
                                    attrib = node.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                                    attrib.Value = "en-US"
                                    node.InnerText = "Cost of shipping, not including shipping tax"

                                    'PaymentTerm
                                    Dim nodePaymentTerm As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="PaymentTerm"))
                                    Dim strPayInNumberOfDays As String = OrderDataSet.Tables(0).Rows(iOrd).Item("PYMNT_TERMS_CD").ToString()
                                    ' (1) payInNumberOfDays
                                    attrib = nodePaymentTerm.Attributes.Append(docXML.CreateAttribute(name:="payInNumberOfDays"))
                                    attrib.Value = strPayInNumberOfDays ' get from dataset

                                    'Comments
                                    Dim nodeComments As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="Comments"))
                                    ' (1) xml:lang - under Comments
                                    attrib = nodeComments.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                                    attrib.Value = "en-US"
                                    nodeComments.InnerText = "Vendor name: Amazon"

                                    'node = node.AppendChild(docXML.CreateElement(name:="BuyerCookie"))
                                    'node.InnerText = Me.BuyerInfo.SessionCookie

                                    'Dim nodeURL As XmlNode = node.AppendChild(docXML.CreateElement(name:="URL"))
                                    'nodeURL.InnerText = m_postbackURL
                                End If

                                ' create line top node ItemOut
                                Dim nodeLine As XmlNode = nodeOrder.AppendChild(docXML.CreateElement(name:="ItemOut"))

                                'add attributes
                                Dim strQty As String = OrderDataSet.Tables(0).Rows(iOrd).Item("QTY_PO").ToString()
                                Try
                                    If IsNumeric(strQty) Then
                                        strQty = FormatNumber(CType(strQty, Decimal), 1)
                                    End If
                                Catch ex As Exception

                                End Try
                                ' (1) quantity
                                attrib = nodeLine.Attributes.Append(docXML.CreateAttribute(name:="quantity"))
                                attrib.Value = strQty ' get from dataset

                                '(2) lineNumber
                                Dim strLineNo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("LINE_NBR").ToString()
                                attrib = nodeLine.Attributes.Append(docXML.CreateAttribute(name:="lineNumber"))
                                attrib.Value = strLineNo ' get from dataset

                                '       ItemID under ItemOut
                                Dim nodeItemID As XmlNode = nodeLine.AppendChild(docXML.CreateElement(name:="ItemID"))
                                '          SupplierPartID under ItemID under ItemOut
                                node = nodeItemID.AppendChild(docXML.CreateElement(name:="SupplierPartID"))
                                Dim strSupplierPartID As String = ""
                                If IsDBNull(OrderDataSet.Tables(0).Rows(iOrd).Item("ITM_ID_VNDR").ToString()) Then
                                    strSupplierPartID = ""
                                Else
                                    If Not OrderDataSet.Tables(0).Rows(iOrd).Item("ITM_ID_VNDR").ToString() Is Nothing Then
                                        Try
                                            If Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("ITM_ID_VNDR").ToString()) <> "" Then
                                                strSupplierPartID = Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("ITM_ID_VNDR").ToString())
                                            Else
                                                strSupplierPartID = ""
                                            End If
                                        Catch ex As Exception
                                            strSupplierPartID = ""
                                        End Try
                                    End If
                                End If
                                ''for  testing only
                                'If strSupplierPartID = "" Then
                                '    strSupplierPartID = "0448439042"
                                'End If
                                node.InnerText = strSupplierPartID
                                '          SupplierPartAuxiliaryID under ItemID under ItemOut
                                Dim strSupplierPartAuxiliaryID As String = ""
                                If IsDBNull(OrderDataSet.Tables(0).Rows(iOrd).Item("ISA_ATTRVAL30").ToString()) Then
                                    strSupplierPartAuxiliaryID = ""
                                Else
                                    If Not OrderDataSet.Tables(0).Rows(iOrd).Item("ISA_ATTRVAL30").ToString() Is Nothing Then
                                        Try
                                            If Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("ISA_ATTRVAL30").ToString()) <> "" Then
                                                strSupplierPartAuxiliaryID = Trim(OrderDataSet.Tables(0).Rows(iOrd).Item("ISA_ATTRVAL30").ToString())
                                            Else
                                                strSupplierPartAuxiliaryID = ""
                                            End If
                                        Catch ex As Exception
                                            strSupplierPartAuxiliaryID = ""
                                        End Try
                                    End If
                                End If

                                ''for  testing only
                                'If strSupplierPartAuxiliaryID = "" Then
                                '    strSupplierPartAuxiliaryID = "184-2574155-1031518"
                                'End If
                                If Trim(strSupplierPartAuxiliaryID) <> "" Then
                                    strSupplierPartAuxiliaryID = Trim(strSupplierPartAuxiliaryID)
                                    If Len(strSupplierPartAuxiliaryID) > 59 Then
                                        'line came from Amazon Search
                                        '  Extrinsic
                                        node = nodeItemID.AppendChild(docXML.CreateElement(name:="Extrinsic"))
                                        '                 (1) name - under Extrinsic
                                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                        attrib.Value = "OfferId"

                                    Else
                                        'line came from Amazon Punch Out
                                        '   SupplierPartAuxiliaryID 
                                        node = nodeItemID.AppendChild(docXML.CreateElement(name:="SupplierPartAuxiliaryID"))
                                    End If
                                Else
                                    'line came from Amazon Punch Out
                                    '   SupplierPartAuxiliaryID 
                                    node = nodeItemID.AppendChild(docXML.CreateElement(name:="SupplierPartAuxiliaryID"))
                                End If

                                node.InnerText = strSupplierPartAuxiliaryID

                                '       ItemDetail under ItemOut
                                Dim nodeItemDetail As XmlNode = nodeLine.AppendChild(docXML.CreateElement(name:="ItemDetail"))
                                '           UnitPrice under ItemDetail under ItemOut
                                Dim nodeUnitPrice As XmlNode = nodeItemDetail.AppendChild(docXML.CreateElement(name:="UnitPrice"))
                                '              Money under UnitPrice under ItemDetail under ItemOut
                                node = nodeUnitPrice.AppendChild(docXML.CreateElement(name:="Money"))
                                Dim strMoneyLine As String = OrderDataSet.Tables(0).Rows(iOrd).Item("PRICE_PO").ToString()
                                Try
                                    If IsNumeric(strMoneyLine) Then
                                        strMoneyLine = FormatNumber(CType(strMoneyLine, Decimal), 2)
                                    End If
                                Catch ex As Exception

                                End Try
                                Dim strCurrncLine As String = OrderDataSet.Tables(0).Rows(iOrd).Item("PO_CURRENCY").ToString()
                                ' (1) currency
                                attrib = node.Attributes.Append(docXML.CreateAttribute(name:="currency"))
                                attrib.Value = strCurrncLine ' get from dataset
                                node.InnerText = strMoneyLine ' get from dataset 

                                '           Description under ItemDetail under ItemOut
                                node = nodeItemDetail.AppendChild(docXML.CreateElement(name:="Description"))
                                ' (1) xml:lang - under Description
                                attrib = node.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                                attrib.Value = "en-US"
                                Dim strDescriptionLine As String = OrderDataSet.Tables(0).Rows(iOrd).Item("DESCR254_MIXED").ToString()
                                node.InnerText = strDescriptionLine  '  get from dataset 

                                '           UnitOfMeasure under ItemDetail under ItemOut
                                node = nodeItemDetail.AppendChild(docXML.CreateElement(name:="UnitOfMeasure"))
                                Dim strUnitOfMeasure As String = OrderDataSet.Tables(0).Rows(iOrd).Item("UNIT_OF_MEASURE").ToString()
                                node.InnerText = strUnitOfMeasure  '  get from dataset

                                '           Classification under ItemDetail under ItemOut
                                node = nodeItemDetail.AppendChild(docXML.CreateElement(name:="Classification"))
                                '                 (1) domain - under Classification
                                attrib = node.Attributes.Append(docXML.CreateAttribute(name:="domain"))
                                attrib.Value = "SPSC"
                                node.InnerText = ""
                                '           Extrinsic under ItemDetail under ItemOut
                                node = nodeItemDetail.AppendChild(docXML.CreateElement(name:="Extrinsic"))
                                '                 (1) name - under Extrinsic
                                attrib = node.Attributes.Append(docXML.CreateAttribute(name:="name"))
                                attrib.Value = "ExtDescription"
                                node.InnerText = ""

                            Next   '  Loop through Order
                        End If  '  OrderDataSet
                    End If  '  OrderDataSet
                End If  '  OrderDataSet
                Try
                    OrdDataAdapter.Dispose()
                Catch ex As Exception

                End Try
                Try
                    OrdCommand.Dispose()
                Catch ex As Exception

                End Try
                Try
                    OrderDataSet.Dispose()
                Catch ex As Exception

                End Try


            Catch ex As Exception
                Try
                    connectOR.Close()
                Catch ex1 As Exception

                End Try
            End Try

            Try
                connectOR.Close()
            Catch ex1 As Exception

            End Try

            cXML = docXML.OuterXml
        Catch ex As Exception
            Throw New ApplicationException(rtn & "::" & ex.Message, ex)
        End Try

        Return cXML
    End Function

    Private Sub GetShipToPhoneParts(ByVal strPhoneShipTo As String, ByRef strCountryCodeShipTo As String, _
                    ByRef strAreaOrCityCode As String, ByRef strNumberSuffix As String)
        Dim iPhoneLength As Integer = Len(strPhoneShipTo)
        Select Case iPhoneLength
            Case 11
                strCountryCodeShipTo = Microsoft.VisualBasic.Left(strPhoneShipTo, 1)
                strAreaOrCityCode = Mid(strPhoneShipTo, 2, 3)
                strNumberSuffix = Mid(strPhoneShipTo, 5, 7)
            Case 10
                strCountryCodeShipTo = ""
                strAreaOrCityCode = Microsoft.VisualBasic.Left(strPhoneShipTo, 3)
                strNumberSuffix = Mid(strPhoneShipTo, 4)
            Case 7
                strCountryCodeShipTo = ""
                strAreaOrCityCode = ""
                strNumberSuffix = strPhoneShipTo
            Case Else
                If iPhoneLength > 11 Then
                    strPhoneShipTo = Microsoft.VisualBasic.Left(strPhoneShipTo, 11)
                    strCountryCodeShipTo = Microsoft.VisualBasic.Left(strPhoneShipTo, 1)
                    strAreaOrCityCode = Mid(strPhoneShipTo, 2, 3)
                    strNumberSuffix = Mid(strPhoneShipTo, 5, 7)
                End If
                If iPhoneLength < 7 Then
                    strCountryCodeShipTo = ""
                    strAreaOrCityCode = ""
                    strNumberSuffix = ""
                End If
        End Select
    End Sub

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
