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
    Private m_cXMLversion As String = "1.2.013"
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

    Public Sub New(ByVal strType As String, Optional ByVal strInvoiceNo As String = "")
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
                If Trim(strInvoiceNo) = "" Then
                    m_payloadId = sNow & ".cXML." & randomId.ToString("000000000") & "." & "OrderRequest@sdi.com"
                Else
                    m_payloadId = sNow & ".cXML." & strInvoiceNo & "." & randomId.ToString("000000000") & "." & "OrderRequest@sdi.com"
                End If
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

    Public Shared Function CreateOrderRequestDoc(ByVal vendorSettings As punchoutVendorConfig, Optional ByVal strInvoiceNo As String = "") As punchOutSetupRequestDoc
        Dim doc As punchOutSetupRequestDoc = Nothing
        If Not (vendorSettings Is Nothing) Then
            doc = New punchOutSetupRequestDoc("OrderRequest", strInvoiceNo)

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
                Dim strOrder As String = "select * from SYSADM8.PS_ISA_PO_DISP_XML where po_id='" & strOrderNo & "'"
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
                            '' Here we are starting changes for UNIV. of CHICAGO
                            ' create invoice top node
                            Dim nodeOrder As XmlNode = nodeOrderReq.AppendChild(docXML.CreateElement(name:="InvoiceDetailRequest"))

                            Dim iOrd As Integer = 0
                            
                            'create order header node
                            Dim nodeOrderHeader As XmlNode = nodeOrder.AppendChild(docXML.CreateElement(name:="InvoiceDetailRequestHeader"))

                            'add attributes: InvoiceID, purpose="standard", operation="new", InvoiceDate 
                            Dim strPoDate As String = OrderDataSet.Tables(0).Rows(iOrd).Item("PO_DT").ToString() ' this should be InvoiceDate
                            Dim dDate1 As DateTime = CType(strPoDate, DateTime)
                            Dim dateOffset1 As New DateTimeOffset(dDate1, TimeZoneInfo.Local.GetUtcOffset(dDate1))
                            strPoDate = dateOffset1.ToString("o")
                            ' (1) InvoiceID
                            attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="invoiceID"))
                            attrib.Value = strOrder

                            'Dim strPoId As String = OrderDataSet.Tables(0).Rows(iOrd).Item("PO_ID").ToString()
                            '(2) purpose
                            attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="purpose"))
                            attrib.Value = "standard"

                            'Dim strPoOrderType As String = OrderDataSet.Tables(0).Rows(iOrd).Item("ORDER_TYPE").ToString()
                            '(3) operation
                            attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="operation"))
                            attrib.Value = "new"

                            Dim strPoVersion As String = OrderDataSet.Tables(0).Rows(iOrd).Item("ORDER_VERSION").ToString()
                            ' (4) invoiceDate
                            attrib = nodeOrderHeader.Attributes.Append(docXML.CreateAttribute(name:="invoiceDate"))
                            attrib.Value = strPoDate

                            ' adding nodes under nodeOrderHeader ("InvoiceDetailRequestHeader")
                            '2 empty nodes
                            Dim nodeEmpt1 As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="InvoiceDetailHeaderIndicator"))
                            Dim nodeEmpt2 As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="InvoiceDetailLineIndicator"))
                            ' InvoicePartner - 1st time - remit to
                            Dim nodeInvPartnr1 As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="InvoicePartner"))
                            ' node Contact1 - under InvoicePartner1
                            Dim nodeContct1 As XmlNode = nodeInvPartnr1.AppendChild(docXML.CreateElement(name:="Contact"))
                            ' attrib  - for node Contact1 - under InvoicePartner1
                            attrib = nodeContct1.Attributes.Append(docXML.CreateAttribute(name:="role"))
                            attrib.Value = "remitTo"
                            ' node Name - under node Contact1 - under InvoicePartner1
                            Dim nodeName1 As XmlNode = nodeContct1.AppendChild(docXML.CreateElement(name:="Name"))
                            ' attrib  - for  node Name1 - under node Contact1 - under InvoicePartner1
                            attrib = nodeName1.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                            attrib.Value = "en-US"
                            'Name for "remitTo" NEEDS TO DETERMUNE where from get
                            Dim strNameRemitTo As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            nodeName1.InnerText = strNameRemitTo ' get from dataset Name for "remitTo"

                            ' node PostalAddress - under node Contact1 - under InvoicePartner1
                            Dim nodePostalAddress1 As XmlNode = nodeContct1.AppendChild(docXML.CreateElement(name:="PostalAddress"))

                            ' node node32 (Street) under node PostalAddress - under node Contact1 - under InvoicePartner1
                            Dim node32 As XmlNode = nodePostalAddress1.AppendChild(docXML.CreateElement(name:="Street"))
                            Dim strStreet1 As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node32.InnerText = strStreet1 ' get from dataset

                            ' node node32 (City) under node PostalAddress - under node Contact1 - under InvoicePartner1
                            node32 = nodePostalAddress1.AppendChild(docXML.CreateElement(name:="City"))
                            Dim strCity1 As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node32.InnerText = strCity1 ' get from dataset

                            ' node node32 (State) under node PostalAddress - under node Contact1 - under InvoicePartner1
                            node32 = nodePostalAddress1.AppendChild(docXML.CreateElement(name:="State"))
                            Dim strState1 As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node32.InnerText = strState1 ' get from dataset

                            ' node node32 (PostalCode) under node PostalAddress - under node Contact1 - under InvoicePartner1
                            node32 = nodePostalAddress1.AppendChild(docXML.CreateElement(name:="PostalCode"))
                            Dim strPostalCode1 As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node32.InnerText = strPostalCode1 ' get from dataset

                            ' node node32 (Country) under node PostalAddress - under node Contact1 - under InvoicePartner1
                            node32 = nodePostalAddress1.AppendChild(docXML.CreateElement(name:="Country"))
                            Dim strCountry1 As String = "United States" ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            ' add attrib for node node32 (Country) under node PostalAddress - under node Contact1 - under InvoicePartner1
                            attrib = node32.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                            attrib.Value = "US"
                            node32.InnerText = strCountry1 ' get from dataset


                            ' InvoicePartner - 2nd time - sold to
                            Dim nodeInvPartnr2 As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="InvoicePartner"))
                            ' node Contact2 - under InvoicePartner2
                            Dim nodeContct2 As XmlNode = nodeInvPartnr2.AppendChild(docXML.CreateElement(name:="Contact"))
                            ' attrib  - for node Contact2 - under InvoicePartner2
                            attrib = nodeContct2.Attributes.Append(docXML.CreateAttribute(name:="role"))
                            attrib.Value = "soldTo"
                            ' node Name - under node Contact2 - under InvoicePartner2
                            Dim nodeName2 As XmlNode = nodeContct2.AppendChild(docXML.CreateElement(name:="Name"))
                            ' attrib  - for  node Name2 - under node Contact1 - under InvoicePartner1
                            attrib = nodeName2.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                            attrib.Value = "en-US"
                            'Name for "soldTo" NEEDS TO DETERMUNE where from get
                            Dim strNameSoldTo As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            nodeName2.InnerText = strNameSoldTo ' get from dataset Name for "soldTo"

                            ' node PostalAddress - under node Contact2 - under InvoicePartner2
                            Dim nodePostalAddress2 As XmlNode = nodeContct2.AppendChild(docXML.CreateElement(name:="PostalAddress"))

                            ' node node321 (Street) under node PostalAddress2 - under node Contact2 - under InvoicePartner2
                            Dim node321 As XmlNode = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="Street"))
                            Dim strStreet2 As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node321.InnerText = strStreet2 ' get from dataset

                            ' node node321 (City) under node PostalAddress - under node Contact2 - under InvoicePartner2
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="City"))
                            Dim strCity2 As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node321.InnerText = strCity2 ' get from dataset

                            ' node node321 (State) under node PostalAddress - under node Contact2 - under InvoicePartner2
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="State"))
                            Dim strState2 As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node321.InnerText = strState2 ' get from dataset

                            ' node node321 (PostalCode) under node PostalAddress - under node Contact2 - under InvoicePartner2
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="PostalCode"))
                            Dim strPostalCode2 As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node321.InnerText = strPostalCode2 ' get from dataset

                            ' node node321 (Country) under node PostalAddress - under node Contact2 - under InvoicePartner2
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="Country"))
                            Dim strCountry2 As String = "United States" ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            ' add attrib for node node321 (Country) under node PostalAddress - under node Contact2 - under InvoicePartner2
                            attrib = node321.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                            attrib.Value = "US"
                            node321.InnerText = strCountry2 ' get from dataset


                            ' InvoiceDetailShipping
                            Dim nodeInvDetlShipping As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="InvoiceDetailShipping"))
                            ' node Contact under InvoiceDetailShipping
                            Dim node3211 As XmlNode = nodeInvDetlShipping.AppendChild(docXML.CreateElement(name:="Contact"))
                            ' add attribs role and addressID
                            attrib = node3211.Attributes.Append(docXML.CreateAttribute(name:="role"))
                            attrib.Value = "shipTo"
                            attrib = node3211.Attributes.Append(docXML.CreateAttribute(name:="addressID"))
                            attrib.Value = " "
                            ' node Name - under node Contact under InvoiceDetailShipping
                            Dim nodeNameDetShip As XmlNode = node3211.AppendChild(docXML.CreateElement(name:="Name"))
                            ' attrib  - for  node nodeNameDetShip - under node Contact - under InvoiceDetailShipping
                            attrib = nodeNameDetShip.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                            attrib.Value = "en-US"
                            'NEEDS TO DETERMUNE where from get
                            Dim strNameDetShip As String = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            nodeNameDetShip.InnerText = strNameDetShip ' get from dataset 

                            'node Postal Address under Contact under InvoiceDetailShipping
                            nodePostalAddress2 = node3211.AppendChild(docXML.CreateElement(name:="PostalAddress"))

                            ' node node321 (Street) under node PostalAddress2 
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="Street"))
                            strStreet2 = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node321.InnerText = strStreet2 ' get from dataset

                            ' node node321 (City) under node PostalAddress2 
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="City"))
                            strCity2 = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node321.InnerText = strCity2 ' get from dataset

                            ' node node321 (State) under node PostalAddress2 
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="State"))
                            strState2 = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node321.InnerText = strState2 ' get from dataset

                            ' node node321 (PostalCode) under node PostalAddress2 
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="PostalCode"))
                            strPostalCode2 = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            node321.InnerText = strPostalCode2 ' get from dataset

                            ' node node321 (Country) under node PostalAddress2 2
                            node321 = nodePostalAddress2.AppendChild(docXML.CreateElement(name:="Country"))
                            strCountry2 = "United States" ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            ' add attrib for node node321 (Country) under node PostalAddress2 
                            attrib = node321.Attributes.Append(docXML.CreateAttribute(name:="isoCountryCode"))
                            attrib.Value = "US"
                            node321.InnerText = strCountry2 ' get from dataset


                            ' another node same name: node Contact under InvoiceDetailShipping
                            node3211 = nodeInvDetlShipping.AppendChild(docXML.CreateElement(name:="Contact"))
                            ' add attrib role
                            attrib = node3211.Attributes.Append(docXML.CreateAttribute(name:="role"))
                            attrib.Value = "shipFrom"
                            ' node Name - under node Contact under InvoiceDetailShipping
                            nodeNameDetShip = node3211.AppendChild(docXML.CreateElement(name:="Name"))
                            ' attrib  - for  node nodeNameDetShip - under node Contact (2nd in a row) - under InvoiceDetailShipping
                            attrib = nodeNameDetShip.Attributes.Append(docXML.CreateAttribute(name:="xml:lang"))
                            attrib.Value = "en-US"
                            'NEEDS TO DETERMUNE where from get
                            strNameDetShip = " " ' OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            nodeNameDetShip.InnerText = strNameDetShip ' get from dataset 

                            ' node Extrinsic under nodeOrderHeader
                            Dim nodeExtrnsc As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="Extrinsic"))
                            ' add attrib name
                            attrib = nodeExtrnsc.Attributes.Append(docXML.CreateAttribute(name:="name"))
                            attrib.Value = "invoice due date"
                            Dim strInvDueDate As String = " "  ' NEED TO DETERMINE
                            '  strInvDueDate = OrderDataSet.Tables(0).Rows(iOrd).Item("TOTAL").ToString()
                            nodeExtrnsc.InnerText = strInvDueDate

                            ' create top node InvoiceDetailOrder
                            Dim nodeInvDetlOrder As XmlNode = nodeOrder.AppendChild(docXML.CreateElement(name:="InvoiceDetailOrder"))
                            ' node InvoiceDetailOrderInfo under  nodeInvDetlOrder ---- "InvoiceDetailOrder"
                            Dim nodeInvDetlOrderInfo As XmlNode = nodeInvDetlOrder.AppendChild(docXML.CreateElement(name:="InvoiceDetailOrderInfo"))
                            'add nodes for node InvoiceDetailOrderInfo
                            '  node OrderReference
                            Dim nodeOrderReference As XmlNode = nodeInvDetlOrderInfo.AppendChild(docXML.CreateElement(name:="OrderReference"))
                            'add attribs and nodes
                            'attrib = nodeOrderReference.Attributes.Append(docXML.CreateAttribute(name:="name"))
                            'attrib.Value = "invoice due date"

                            'attrib = nodeOrderReference.Attributes.Append(docXML.CreateAttribute(name:="name"))
                            'attrib.Value = "invoice due date"

                            'Dim nodeExtrnsc As XmlNode = nodeOrderHeader.AppendChild(docXML.CreateElement(name:="Extrinsic"))

                            '  node SupplierOrderInfo
                            Dim nodeSupplierOrderInfo As XmlNode = nodeInvDetlOrderInfo.AppendChild(docXML.CreateElement(name:="SupplierOrderInfo"))
                            attrib = nodeSupplierOrderInfo.Attributes.Append(docXML.CreateAttribute(name:="orderID"))
                            Dim strSupplierOrderID As String = " " ' NEED TO DETERMINE
                            attrib.Value = strSupplierOrderID


                            'cycle starts here
                            For iOrd = 0 To OrderDataSet.Tables(0).Rows.Count - 1

                                ' top line node: InvoiceDetailOrderInfo under  nodeInvDetlOrder ---- "InvoiceDetailOrder"
                                Dim nodeLine As XmlNode = nodeInvDetlOrder.AppendChild(docXML.CreateElement(name:="InvoiceDetailItem"))

                                'add attributes
                                'Dim strQty As String = OrderDataSet.Tables(0).Rows(iOrd).Item("QTY_PO").ToString()
                                'Try
                                '    If IsNumeric(strQty) Then
                                '        strQty = FormatNumber(CType(strQty, Decimal), 1)
                                '    End If
                                'Catch ex As Exception

                                'End Try

                                'Dim strLineNo As String = OrderDataSet.Tables(0).Rows(iOrd).Item("LINE_NBR").ToString()

                            Next   '  Loop through Order

                            'end InvoiceDetailOrder node

                            ' node InvoiceDetailSummary under  nodeOrder ---- "InvoiceDetailRequest"
                            Dim nodeInvDetlSummary As XmlNode = nodeOrder.AppendChild(docXML.CreateElement(name:="InvoiceDetailSummary"))
                            'add attribs and nodes for node nodeInvDetlSummary


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
