Imports System.Net
Imports System.IO
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.Web
Imports System.Data
Imports System.Data.OleDb
Imports System.Xml

Module Module1

    Private strVendorURL As String
    Private m_setupReqDoc As punchOutSetupRequestDoc = Nothing
    Private m_vendorConfig As punchoutVendorConfig = Nothing
    Private strRespnceDoc As String
    Private m_xmlConfig As XmlDocument
    Private m_configFile As String = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\configSetting.xml"
    Dim objStreamWriter As StreamWriter
    Dim objStreamWriterXML As StreamWriter
    Dim objStrmWrtrXMLRspns As StreamWriter
    Dim rootDir As String = "C:\Program Files\sdi\AmazonClient"
    Dim logpath As String = "C:\Program Files\sdi\AmazonClient\AmazonLOGS\AmazonClientOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim filePath As String = "C:\Program Files\sdi\AmazonClient\AmazonXMLFiles\AmazonClientXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
    Dim filePathResponse As String = "C:\Program Files\sdi\AmazonClient\AmazonXMLFiles\AmznClntXMLRspns" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")

    Sub Main()

        Dim strInput As String = ""  '  <?xml version=""1.0"" encoding=""UTF-8""?><!DOCTYPE cXML SYSTEM ""http://xml.cxml.org/schemas/cXML/1.2.013/cXML.dtd""[]><cXML payloadID=""3/30/2015 11:56:16 AM 019768490@sdi.com"" xml:lang=""en-US"" timestamp=""3/30/2015 11:56:16 AM""><Header><From><Credential domain=""NetworkId""><Identity>SDIINC</Identity></Credential></From><To><Credential domain=""NetworkId""><Identity>Amazon</Identity></Credential></To><Sender><Credential domain=""DUNS""><Identity>SDIINC</Identity><SharedSecret>Y2XN7SefSxpPAoD5i6OtYix4w5TK402d</SharedSecret></Credential><UserAgent>Ariba Network 1.2</UserAgent></Sender></Header><Request><PunchOutSetupRequest operation=""create""><BuyerCookie>3xx1vu5dn5sttwrc2zqspprl</BuyerCookie><Extrinsic name=""UniqueName"">ROVENSKY,VITALY</Extrinsic><Extrinsic name=""UserEmail"">vitaly.rovensky@sdi.com</Extrinsic><Extrinsic name=""CostCenter"">I0256</Extrinsic><BrowserFormPost><URL>http://localhost/InsiteOnline/shopredirect.aspx?PUNOUT=YES</URL></BrowserFormPost><ShipTo><Address addressID=""L0256-01""><Name xml:lang=""en-US"">UNCC Facility Maint. Shop</Name><PostalAddress><DeliverTo>SDI c/o UNCC Facility Maint Shop</DeliverTo><Street>9201 University City Blvd.</Street><City>Charlotte</City><State>NC</State><PostalCode>28223</PostalCode><Country isoCountryCode=""US"">United States</Country></PostalAddress></Address></ShipTo></PunchOutSetupRequest></Request></cXML>"
        Dim strOutput As String = ""
        Dim strWhatToTest As String = "AMAZON"  '  "AMAZON"  ' "NEW_AMAZON"  '    "CYTECMXM"  ' "AMAZON"  '
        Dim Response_Doc As String
        Dim msgEx As String = ""
        Dim strMsgVendConfig As String = ""
        objStreamWriter = File.CreateText(logpath)

        Select Case strWhatToTest
            Case "AMAZON"
                Console.WriteLine("Started to check Amazon ready to Dispatch orders ")
                Console.WriteLine("")

                'objStrmWrtrXMLRspns = File.CreateText(filePathResponse)
                'objStreamWriterXML = File.CreateText(filePath)
                objStreamWriter.WriteLine("Started to check Amazon ready to Dispatch orders " & Now())


                m_xmlConfig = New XmlDocument
                m_xmlConfig.Load(filename:=m_configFile)

                Dim cnString As String = ""
                Try
                    ' retrieve the source DB connection string to use
                    If Not (m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText Is Nothing) Then
                        cnString = m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText.Trim
                    End If
                Catch ex As Exception
                    cnString = ""
                End Try

                If Trim(cnString) <> "" Then
                    connectOR.ConnectionString = cnString
                End If

                Dim strPunSite As String = "AMAZON"
                ' list of orders - will be used later to change status flag
                Dim OrderListDataSet As System.Data.DataSet = New System.Data.DataSet()

                ' get info from The view (which is currently only available in DEVL) - SYSADM.PS_ISA_PO_DISP_XML.      

                'Dim strURL As String

                'Dim XMLhttp As Object
                'Dim xmlDoc2 As Object
                'Dim XMLPath As String
                'Dim VendorURL As String

                strMsgVendConfig = " 'm_vendorConfig.ConfigFile' is not defined"
                Try
                    objStreamWriter.WriteLine("Started building XML out " & Now())
                    Dim userBU As String = "I0256"

                    ''DO NOT DELELTE COMMENTED OUT CODE BELOW!
                    '' this function is a copy of the PuncoutWin.aspx call to vendor to establish Punchout session
                    '' returning XML file which is ready to be send
                    'strInput = getPOSR(strPunSite, userBU)

                    'If Trim(strInput) <> "" Then
                    '    objStreamWriter.WriteLine("Saving XML file to send " & Now())
                    '    objStreamWriterXML.WriteLine(strInput)

                    '    Call Send1(strInput, strOutput)

                    '    objStreamWriter.WriteLine("Saving Response XML file " & Now())
                    '    objStrmWrtrXMLRspns.WriteLine(strOutput)
                    'Else

                    '    objStreamWriter.WriteLine("Input string is empty. Possible cause: " & strMsgVendConfig)
                    '    objStreamWriter.Flush()
                    '    objStreamWriter.Close()

                    '    objStreamWriterXML.Flush()
                    '    objStreamWriterXML.Close()

                    '    objStrmWrtrXMLRspns.Flush()
                    '    objStrmWrtrXMLRspns.Close()
                    '    Exit Sub
                    'End If

                    'objStreamWriter.Flush()
                    'objStreamWriter.Close()

                    'objStreamWriterXML.Flush()
                    'objStreamWriterXML.Close()

                    'objStrmWrtrXMLRspns.Flush()
                    'objStrmWrtrXMLRspns.Close()
                    'Exit Sub
                    ''end special test

                    ' function to send order request - using strPunSite only
                    strInput = getOrderRequest(strPunSite, OrderListDataSet, strMsgVendConfig)

                Catch ex As Exception
                    msgEx = "Not a valid identity or vendor URL for this catalog.<BR>Please report error" & _
                                          vbCrLf & "config =" & strMsgVendConfig & _
                                          vbCrLf & "ERROR:: " & vbCrLf & ex.ToString & _
                                          ""

                    objStreamWriter.WriteLine(msgEx)

                    Exit Sub
                End Try

                Exit Sub

            Case "CYTECMXM"
                Console.WriteLine("Started to test CYTEC MXM ")
                Console.WriteLine("")

                objStrmWrtrXMLRspns = File.CreateText(filePathResponse)
                objStreamWriterXML = File.CreateText(filePath)
                objStreamWriter = File.CreateText(logpath)
                objStreamWriter.WriteLine("Started CYTEC MXM Test " & Now())

                'just define sInput for Send1 procedure
                'strInput = "<?xml version=""1.0"" encoding=""UTF-8""?><!DOCTYPE cXML SYSTEM ""http://xml.cxml.org/schemas/cXML/1.2.013/cXML.dtd""[]><cXML payloadID=""2016-02-18T09:06:44.8376289-05:00.043793890.OrderRequest@sdi.com"" xml:lang=""en-US"" timestamp=""2016-02-18T09:06:44.8486311-05:00""><Header><From><Credential domain=""NetworkId""><Identity>CYTECMXM</Identity></Credential></From><To><Credential domain=""NetworkId""><Identity>SDIINC</Identity></Credential></To><Sender><Credential domain=""CYTECNETWORK""><Identity>CYTECMXM</Identity><SharedSecret>JsymuZ/YkUJ3RYPa7dZurQ==</SharedSecret></Credential><UserAgent>Ariba Network 1.2</UserAgent></Sender></Header><Request><OrderRequest><OrderRequestHeader orderDate=""2015-01-09T00:00:00.0000000-05:00"" orderID=""0003118726"" orderType=""REGULAR"" orderVersion=""1"" type=""NEW""><Total><Money currency=""USD"">2292.15</Money></Total><ShipTo><Address addressID=""L0275-02"" isoCountryCode=""US""><Name xml:lang=""en-US"">Cytec HdG Maintenance</Name><PostalAddress name=""default""><DeliverTo>Cytc HdG</DeliverTo><Street>c/o SDI  Maintenance</Street><Street>1300 Revolution Street</Street><City>Havre de Grace</City><State>MD</State><PostalCode>21078</PostalCode><Country isoCountryCode=""US"">United States</Country></PostalAddress><Email name=""default"">Sheldon.Abramowitz@sdi.com</Email><Phone name=""default""><TelephoneNumber><CountryCode isoCountryCode=""US"">1</CountryCode><AreaOrCityCode>215</AreaOrCityCode><Number>633-1900</Number></TelephoneNumber></Phone></Address></ShipTo><BillTo><Address addressID="""" isoCountryCode=""US""><Name xml:lang=""en-US"">SDI</Name><PostalAddress name=""default""><DeliverTo>SDI</DeliverTo><DeliverTo>ATTN: ACCOUNTS PAYABLE</DeliverTo><Street>1414 RADCLIFFE ST</Street><Street></Street><City>BRISTOL</City><State>PA</State><PostalCode>19007</PostalCode><Country isoCountryCode=""US"">United States</Country></PostalAddress><Email name=""default"">Sheldon.Abramowitz@sdi.com</Email><Phone name=""default""><TelephoneNumber><CountryCode isoCountryCode=""US"">1</CountryCode><AreaOrCityCode>215</AreaOrCityCode><Number>633-1900</Number></TelephoneNumber></Phone></Address></BillTo><Shipping><Money currency=""USD"">0.00</Money><Description xml:lang=""en-US"">Cost of shipping, not including shipping tax</Description></Shipping><PaymentTerm payInNumberOfDays=""30"" /><Comments xml:lang=""en-US"">Vendor name: Amazon</Comments></OrderRequestHeader><ItemOut quantity=""5.0"" lineNumber=""1""><ItemID><SupplierPartID>J71C-AVN1-C</SupplierPartID><SupplierPartAuxiliaryID></SupplierPartAuxiliaryID></ItemID><ItemDetail><UnitPrice><Money currency=""USD"">458.43</Money></UnitPrice><Description xml:lang=""en-US"">NO NOUN,NO MODIFIE:COUPLER, J71, 1"":EMCO COULPLER--J71C-AVN1-C</Description><UnitOfMeasure>EA</UnitOfMeasure><Classification domain=""SPSC""></Classification><Extrinsic name=""ExtDescription""></Extrinsic></ItemDetail></ItemOut></OrderRequest></Request></cXML>"
                'strInput = "<?xml version=""1.0"" encoding=""UTF-8""?><!DOCTYPE cXML SYSTEM ""http://xml.cxml.org/schemas/cXML/1.2.013/cXML.dtd""[]><cXML payloadID=""2016-02-18T09:06:44.8376289-05:00.043793890.OrderRequest@sdi.com"" xml:lang=""en-US"" timestamp=""2016-02-18T09:06:44.8486311-05:00""><Header><From><Credential domain=""NetworkId""><Identity>CYTECMXM</Identity></Credential></From><To><Credential domain=""NetworkId""><Identity>SDIINC</Identity></Credential></To><Sender><Credential domain=""CYTECNETWORK""><Identity>CYTECMXM</Identity><SharedSecret>JsymuZ/YkUJ3RYPa7dZurQ==</SharedSecret></Credential><UserAgent>Ariba Network 1.2</UserAgent></Sender></Header><Request><MMITEMIN><ITEM><SITEID>Cytec 2nd Plant</SITEID><ITEMNUM>5600012</ITEMNUM><COMMODITYGROUP>0012</COMMODITYGROUP><DESCRIPTION>sOMETHING SPECIAL - 254 </DESCRIPTION><ISSUEUNIT>EA</ISSUEUNIT><ORDERUNIT>EA</ORDERUNIT></ITEM><INVENTORY><CATEGORY>MRO</CATEGORY><MANUFACTURER>3M</MANUFACTURER><MODELNUM>K726-34</MODELNUM><BINNUM>V4-6-21-8</BINNUM><MINLEVEL>2</MINLEVEL><MAXLEVEL>6</MAXLEVEL><ORDERQTY>2</ORDERQTY></INVENTORY><INVCOST><STDCOST>12.5</STDCOST></INVCOST></MMITEMIN></Request></cXML>"
                '                strInput = "<?xml version=""1.0""?>" & _
                '"<!--Created on 12/17/2014 11:00:22 AM-->" & _
                '"<DATA>" & _
                '   "<ISAORDSTATUSLOG>" & _
                '      "<ORDER_NO>2015023525</ORDER_NO>" & _
                '      "<LINE_NBR>23</LINE_NBR>" & _
                '      "<DTTM_STAMP>12/17/2014 10:23:19 AM</DTTM_STAMP>" & _
                '     " <ISA_ORDER_STATUS>2</ISA_ORDER_STATUS>" & _
                '   "</ISAORDSTATUSLOG>" & _
                '   "<ISAORDSTATUSLOG>" & _
                '      "<ORDER_NO>2015023525</ORDER_NO>" & _
                '      "<LINE_NBR>22</LINE_NBR>" & _
                '      "<DTTM_STAMP>12/17/2014 10:23:19 AM</DTTM_STAMP>" & _
                '      "<ISA_ORDER_STATUS>2</ISA_ORDER_STATUS>" & _
                '   "</ISAORDSTATUSLOG>" & _
                '"</DATA>"

                strInput = "<?xml version=""1.0"" encoding=""UTF-8""?><MXITEMInterface xmlns=""http://www.mro.com/mx/integration"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" language=""EN""><Header operation=""Notify"" event=""1""><SenderID type=""MAXIMO"" majorversion=""6"" minorversion=""2"" build=""20100824-1415"" dbbuild=""V600-768"">MX</SenderID><CreationDateTime>2016-03-31T14:44:34-05:00</CreationDateTime><RecipientID>CY_HTTP</RecipientID><MessageID>14594534745575197</MessageID></Header><Content><MXITEM><ITEM action=""Add""><ITEMNUM>15428</ITEMNUM><DESCRIPTION langenabled=""1"">Test Item 1</DESCRIPTION><ROTATING>0</ROTATING><LOTTYPE maxvalue=""NOLOT"">NOLOT</LOTTYPE><CAPITALIZED>0</CAPITALIZED><MSDSNUM /><OUTSIDE>0</OUTSIDE><IN19 /><IN20 /><IN21 /><IN22 xsi:nil=""true"" /><IN23 xsi:nil=""true"" /><SPAREPARTAUTOADD>0</SPAREPARTAUTOADD><INSPECTIONREQUIRED>0</INSPECTIONREQUIRED><SOURCESYSID /><OWNERSYSID /><EXTERNALREFID /><IN24 /><IN25 /><IN26 /><IN27 /><SENDERSYSID>MX</SENDERSYSID><ITEMSETID>SET02</ITEMSETID><ORDERUNIT>EACH</ORDERUNIT><ISSUEUNIT>EACH</ISSUEUNIT><DESCRIPTION_LONGDESCRIPTION langenabled=""1"" /><CONDITIONENABLED>0</CONDITIONENABLED><GROUPNAME /><METERNAME /><COMMODITYGROUP /><COMMODITY /><ITEMTYPE>ITEM</ITEMTYPE><PRORATE>0</PRORATE><ITEMID>326385</ITEMID><ISKIT>0</ISKIT><ATTACHONISSUE>0</ATTACHONISSUE><STOCKTYPE /><IN1 /><IN2 /><IN5 /><IN14 xsi:nil=""true"" /><MATERIALCLASS /><PACKSIZE xsi:nil=""true"" /><SETUPDATE xsi:nil=""true"" /><PLUSCISINHOUSECAL>1</PLUSCISINHOUSECAL><PLUSCISMTE>0</PLUSCISMTE><PLUSCISMTECLASS /><TRANS_LANGCODE>EN</TRANS_LANGCODE></ITEM></MXITEM></Content></MXITEMInterface>"

            Case "NEW_AMAZON"
                Console.WriteLine("Started to test a New AMAZON service")
                Console.WriteLine("")

                objStrmWrtrXMLRspns = File.CreateText(filePathResponse)
                objStreamWriterXML = File.CreateText(filePath)
                objStreamWriter = File.CreateText(logpath)
                objStreamWriter.WriteLine("Started NEW_AMAZON Test " & Now())

                '' Ship Notice

                strInput = "<?xml version=""1.0"" encoding=""UTF-8""?><!DOCTYPE cXML SYSTEM "
                strInput += """http://xml.cxml.org/schemas/cXML/1.2.024/Fulfill.dtd""> "
                strInput += "<cXML payloadID=""1394645847931.18059.42xx@amazon.com"" "
                strInput += "timestamp=""2014-03-12T17:37:27+17:37"" xml:lang=""en-US""><Header><From><Credential domain=""NetworkId""><Identity>0000039777</Identity></Credential></From><To><Credential domain=""NetworkId""><Identity>SDIDirectOrdering2356630089</Identity></Credential></To><Sender><Credential domain=""NetworkId""><Identity>0000039777</Identity><SharedSecret>CNznizfS4klqFVc2FDCJGQ==</SharedSecret></Credential><UserAgent>Amazon LLC eProcurement Application</UserAgent></Sender></Header><Request><ShipNoticeRequest><ShipNoticeHeader shipmentID=""2199947375124"" operation="
                strInput += "  ""new"" noticeDate=""2014-03-12T17:37:35+17:37"" shipmentDate="
                strInput += "   ""2014-03-12T17:32:02+17:32"" "
                strInput += "   deliveryDate=""2014-03-14T03:00:00+03:00"" "
                strInput += "    shipmentType=""actual""/><ShipControl><CarrierIdentifier domain=""companyName""></CarrierIdentifier><ShipmentIdentifier>1Z1Y2E270339295041</ShipmentIdentifier><PackageIdentification rangeBegin=""1"" rangeEnd=""1""/></ShipControl><ShipNoticePortion><OrderReference orderDate=""2014-03-11T15:57:28-04:00"" orderID=""E010653511xxxx""><DocumentReference payloadID=""1394567848065.10310490xx.OrderRequest@xx.org""/></OrderReference><ShipNoticeItem lineNumber=""10"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""4"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""13"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""6"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""5"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""7"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""2"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""9"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""12"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""8"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""14"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""11"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""15"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""3"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem><ShipNoticeItem lineNumber=""1"" quantity=""2""><UnitOfMeasure>EA</UnitOfMeasure></ShipNoticeItem></ShipNoticePortion></ShipNoticeRequest></Request></cXML>"

                ''Order Confirm

                'strInput = "<?xml version=""1.0"" encoding=""UTF-8""?><!DOCTYPE cXML SYSTEM"
                'strInput += " ""http://xml.cxml.org/schemas/cXML/1.2.024/Fulfill.dtd"">"
                'strInput += "<cXML payloadID=""1394568264055.446.73xx@amazon.com"" "
                'strInput += "timestamp=""2014-03-11T13:04:24+13:04"" xml:lang=""en-US""><Header><From><Credential "                'strInput += "domain=""NetworkId""><Identity>0000039777</Identity></Credential></From><To><Credential "                'strInput += "domain=""NetworkId""><Identity>SDIDirectOrdering2356630089</Identity></Credential></To><Sender><Credential "                'strInput += "domain=""NetworkId""><Identity>0000039777</Identity><SharedSecret>CNznizfS4klqFVc2FDCJGQ==</SharedSecret></Credential><UserAgent>Amazon LLC eProcurement "                'strInput += "Application</UserAgent></Sender></Header><Request><ConfirmationRequest><ConfirmationHeader confirmID=""105-5528563-5618617"" "
                'strInput += " operation=""new"" type=""detail"" "
                'strInput += " noticeDate=""2014-03-11T13:04:24+13:04""><Total><Money currency=""USD"">137.18</Money></Total><Shipping><Money currency=""USD"">0.00</Money><Description "                'strInput += "xml:lang=""en-US"">Cost of "
                'strInput += "shipping, not including shipping tax</Description></Shipping><Tax><Money currency=""USD"">8.60</Money><Description xml:lang=""en-US"">Cost of tax, including "
                'strInput += "shipping tax</Description></Tax></ConfirmationHeader><OrderReference orderID=""E010653511913xxx"" orderDate="
                'strInput += " ""2014-03-11T12:57:28-07:00"">"
                'strInput += "<DocumentReference payloadID="
                'strInput += " ""1394567848065.10310490xx.OrderRequest@xx.org""/></OrderReference><ConfirmationItem quantity=""2"" "                'strInput += "lineNumber=""1""><UnitOfMeasure>EA</UnitOfMeasure><ConfirmationStatus quantity=""2"" type=""detail""><UnitOfMeasure>EA</UnitOfMeasure><Tax><Money "                'strInput += "currency=""USD"">0.60</Money><Description xml:lang=""en-US"">Cost of tax, including "
                'strInput += "shipping tax</Description></Tax><Shipping><Money currency=""USD"">0.00</Money><Description xml:lang=""en-US"">Cost of shipping, not including "
                'strInput += "shipping tax</Description></Shipping><Comments type=""confirmID"">105-5528563-5618617</Comments></ConfirmationStatus></ConfirmationItem><ConfirmationItem "                'strInput += "quantity=""2"" lineNumber=""13""><UnitOfMeasure>EA</UnitOfMeasure><ConfirmationStatus quantity=""2"" type=""detail""><UnitOfMeasure>EA</UnitOfMeasure><Tax><Money "                'strInput += "currency=""USD"">0.60</Money><Description xml:lang=""en-US"">Cost of tax, including "
                'strInput += "shipping tax</Description></Tax><Shipping><Money currency=""USD"">0.00</Money><Description xml:lang=""en-US"">Cost of shipping, not including "
                'strInput += "shipping tax</Description></Shipping><Comments type=""confirmID"">105-5528563-5618617</Comments></ConfirmationStatus></ConfirmationItem><ConfirmationItem "                'strInput += "quantity=""2"" lineNumber=""14""><UnitOfMeasure>EA</UnitOfMeasure><ConfirmationStatus quantity=""2"" type=""detail""><UnitOfMeasure>EA</UnitOfMeasure><UnitPrice><Money "                'strInput += "currency=""USD"">2.96</Money></UnitPrice><Tax><Money currency=""USD"">0.40</Money><Description xml:lang=""en-US"">Cost of tax, including "
                'strInput += "shipping tax</Description></Tax><Shipping><Money currency=""USD"">0.00</Money><Description xml:lang=""en-US"">Cost of shipping, not including "
                'strInput += "shipping tax</Description></Shipping><Comments type=""confirmID"">105-5528563-5618617</Comments></ConfirmationStatus></ConfirmationItem><ConfirmationItem "                'strInput += "quantity=""2"" lineNumber=""15""><UnitOfMeasure>EA</UnitOfMeasure><ConfirmationStatus quantity=""2"" type=""detail""><UnitOfMeasure>EA</UnitOfMeasure><Tax><Money "                'strInput += "currency=""USD"">0.60</Money><Description xml:lang=""en-US"">Cost of tax, including "
                'strInput += "shipping tax</Description></Tax><Shipping><Money currency=""USD"">0.00</Money><Description xml:lang=""en-US"">Cost of shipping, not including "
                'strInput += "shipping tax</Description></Shipping><Comments type=""confirmID"">105-5528563-"                'strInput += "5618617</Comments></ConfirmationStatus></ConfirmationItem></ConfirmationRequest></Request></cXML>"

                '    "https://sdiexchtest.sdi.com/WebSvcSDI/xmlinsdi.aspx"

                ''strInput = ""
                ''strInput += ""

                'strInput = "<?xml version = '1.0' encoding = 'UTF-8'?>"
                'strInput += "<!DOCTYPE cXML SYSTEM ""http://xml.cxml.org/schemas/cXML/1.2.017/cXML.dtd"">"
                'strInput += "<cXML version=""1.2.017"" xml:lang=""en-US"" payloadID=""20151206ABCDE.1449509406.3418072@amazon12345.com"" timestamp=""2015-12-07T09:30:06-0800"">"
                'strInput += " <Header><From><Credential domain=""NetworkId""><Identity>SDIDirectOrdering2356630089</Identity></Credential></From><To><Credential "                'strInput += "domain=""NetworkId""><Identity>Amazon</Identity></Credential></To><Sender><Credential "                'strInput += "domain=""NetworkId""><Identity>SDIDirectOrdering2356630089</Identity><SharedSecret>6LhhKUA2D53IrSMYujgODVmJq6AgK5</SharedSecret></Credential></Sender></Header><Request "                'strInput += "deploymentMode=""production""><OrderRequest><OrderRequestHeader orderDate=""2015-12-07T09:30:06-0800"" orderID=""ASBCDE12345"" orderType=""regular"" orderVersion=""1"" "                'strInput += "type=""new""><ShipTo><Address isoCountryCode=""US""><Name xml:lang=""en-US"">Main Address</Name><PostalAddress name=""default""><DeliverTo>Mr. "                'strInput += "James</DeliverTo><Street>600 Street ABCD</Street><Street>Apt 12345</Street><City>Seattle</City><State>WA</State><PostalCode>98019</PostalCode><Country "                'strInput += "isoCountryCode=""US"">United States</Country></PostalAddress><Email name=""default"">michael.randall@isacs.com</Email><Phone "                'strInput += "name=""default""><TelephoneNumber><CountryCode "                'strInput += "isoCountryCode=""US"">1</CountryCode><AreaOrCityCode>111</AreaOrCityCode><Number>1111111</Number></TelephoneNumber></Phone></Address></ShipTo><Extrinsic "                'strInput += "name=""email"">michael.randall@isacs.com</Extrinsic></OrderRequestHeader><ItemOut quantity=""1"" "                'strInput += "lineNumber=""1""><ItemID><SupplierPartID>0314194878</SupplierPartID></ItemID><ItemDetail><UnitPrice><Money "                'strInput += "currency=""USD"">87.94</Money></UnitPrice><UnitOfMeasure>EA</UnitOfMeasure></ItemDetail></ItemOut></OrderRequest></Request> </cXML>"

                'https://https.amazonsedi.com/c47fcf9d-286d-498a-ba9f-df390c2757a2

            Case Else
                Exit Sub
        End Select

        

        If Trim(strInput) <> "" Then
            objStreamWriter.WriteLine("Saving XML file to send " & Now())
            objStreamWriterXML.WriteLine(strInput)

            Call Send1(strInput, strOutput)

            objStreamWriter.WriteLine("Saving Response XML file " & Now())
            objStrmWrtrXMLRspns.WriteLine(strOutput)
        Else

            objStreamWriter.WriteLine("Input string is empty. Possible cause: " & strMsgVendConfig)
            objStreamWriter.Flush()
            objStreamWriter.Close()

            objStreamWriterXML.Flush()
            objStreamWriterXML.Close()

            objStrmWrtrXMLRspns.Flush()
            objStrmWrtrXMLRspns.Close()
            Exit Sub
        End If

        objStreamWriter.WriteLine("Checking Response XML file " & Now())
        ' check strOutput 
        Dim bIsOK As Boolean = False
        Response_Doc = Common.RemoveCrLf(strOutput)
        If Trim(Response_Doc) <> "" Then

            '-----------------------------------------------------------------------
            ' Parse the Server response and retrieve XML file
            '-----------------------------------------------------------------------

            Dim xmlResponse As New XmlDocument
            Dim root As XmlElement = Nothing
            Try
                xmlResponse.LoadXml(Response_Doc)
                root = xmlResponse.DocumentElement
                bIsOK = True
            Catch ex As Exception
                objStreamWriter.WriteLine("Response XML file is NOT checked OK - 'LoadXml' area  " & Now())
                bIsOK = False
            End Try
            'Dim objnode As XmlNode

            If bIsOK Then
                Try
                    Try
                        If Not root.SelectNodes("Response/Status").Item(0).Attributes(name:="code").Value Is Nothing Then
                            Try
                                bIsOK = (root.SelectNodes("Response/Status").Item(0).Attributes(name:="code").Value = "200")
                            Catch ex As Exception
                                bIsOK = False
                            End Try
                        End If

                        If Not bIsOK Then
                            If Not root.SelectNodes("Response/Status").Item(0).Attributes(name:="text").Value Is Nothing Then
                                Try
                                    bIsOK = (root.SelectNodes("Response/Status").Item(0).Attributes(name:="text").Value.ToUpper = "SUCCESS")
                                Catch ex As Exception
                                    bIsOK = False
                                End Try
                            End If

                        End If
                        If Not bIsOK Then
                            If Not root.SelectNodes("Response/Status").Item(0).Attributes(name:="text").Value Is Nothing Then
                                Try
                                    bIsOK = (root.SelectNodes("Response/Status").Item(0).Attributes(name:="text").Value.ToUpper = "OK")
                                Catch ex As Exception
                                    bIsOK = False
                                End Try
                            End If
                        End If
                    Catch ex As Exception
                        bIsOK = False
                    End Try

                    If bIsOK Then
                        'objStreamWriter.WriteLine("Response XML file checked OK. Changing Order statuses " & Now())
                        'Dim strValueToWrite As String = Now.Year.ToString() & Now.Month.ToString() & Now.Day.ToString() & Now.GetHashCode.ToString()
                        'Dim intNumberToWrite As Long = 0
                        'If IsNumeric(strValueToWrite) Then
                        '    intNumberToWrite = CType(strValueToWrite, Long)
                        'Else
                        '    intNumberToWrite = Now.GetHashCode
                        'End If
                        'Dim bNoErrors As Boolean = True
                        'Dim strOrderNo As String = ""
                        'connectOR.Open()
                        'Try
                        '    Dim iOrdCount As Integer = OrderListDataSet.Tables(0).Rows.Count
                        '    ' run this query for every order sent:
                        '    ' "update SYSADM.PS_PO_DISPATCHED set ECQUEUEINSTANCE=" & intNumberToWrite & " where po_id='" & "M010373791" & "'"
                        '    If connectOR.State = ConnectionState.Open Then
                        '    Else
                        '        connectOR.Open()
                        '    End If
                        '    Dim rowsAffected As Integer = 0
                        '    Dim iCnt As Integer = 0
                        '    For iCnt = 0 To iOrdCount - 1
                        '        If iCnt = 1 Then Exit For ' for testing ONLY!
                        '        rowsAffected = 0
                        '        strOrderNo = OrderListDataSet.Tables(0).Rows(iCnt).Item("po_id").ToString()
                        '        'run query
                        '        Dim strUpdateQuery As String = "update SYSADM.PS_PO_DISPATCHED set ECQUEUEINSTANCE=" & intNumberToWrite & " where po_id='" & strOrderNo & "'"
                        '        'commented out for testing
                        '        Dim UpdCommand As OleDbCommand = New OleDbCommand(strUpdateQuery, connectOR)
                        '        UpdCommand.CommandTimeout = 120
                        '        rowsAffected = UpdCommand.ExecuteNonQuery()
                        '        Try
                        '            UpdCommand.Dispose()
                        '        Catch ex As Exception

                        '        End Try
                        '        If rowsAffected = 0 Then
                        '            bNoErrors = False
                        '            objStreamWriter.WriteLine("Order status change returned: 'rowsAffected = 0' for Order: " & strOrderNo)
                        '        End If
                        '    Next
                        'Catch ex As Exception
                        '    bNoErrors = False
                        '    objStreamWriter.WriteLine("Error trying to update Order: " & strOrderNo & " Error Message: " & ex.Message)
                        '    Try
                        '        connectOR.Close()
                        '    Catch ex1 As Exception

                        '    End Try
                        'End Try
                        'Try
                        '    connectOR.Close()
                        'Catch ex As Exception

                        'End Try
                        'If bNoErrors Then
                        '    objStreamWriter.WriteLine("Order statuses changed without errors " & Now())
                        'Else
                        'End If
                    Else
                        objStreamWriter.WriteLine("Response XML file is NOT checked OK " & Now())
                        Dim msg As String = "" '  & _
                        Try
                            If Not root.SelectNodes("Response/Status").Item(0).Attributes(0) Is Nothing Then
                                If Not root.SelectNodes("Response/Status").Item(0).Attributes(0).Value Is Nothing Then
                                    Try
                                        msg += "'Code' = " & root.SelectNodes("Response/Status").Item(0).Attributes(0).Value '  & _
                                    Catch ex As Exception
                                        msg += "'Code' retrieving Error (inner): " & ex.Message
                                    End Try
                                Else
                                    msg += vbCrLf & " 'Code': root.SelectNodes('Response/Status').Item(0).Attributes(0).Value is Nothing"
                                End If
                            Else
                                msg += vbCrLf & " 'Code': root.SelectNodes('Response/Status').Item(0).Attributes(0) is Nothing"
                            End If
                        Catch ex As Exception
                            msg += vbCrLf & "'Code' retrieving Error (outer): " & ex.Message
                        End Try

                        Try
                            If Not root.SelectNodes("Response/Status").Item(0).Attributes(1) Is Nothing Then
                                If Not root.SelectNodes("Response/Status").Item(0).Attributes(1).Value Is Nothing Then
                                    Try
                                        msg += "'Reason' = " & root.SelectNodes("Response/Status").Item(0).Attributes(1).Value '  & _
                                    Catch ex As Exception
                                        msg += "'Reason' retrieving Error (inner): " & ex.Message
                                    End Try
                                Else
                                    msg += vbCrLf & " 'Reason': root.SelectNodes('Response/Status').Item(0).Attributes(1).Value is Nothing"
                                End If
                            Else
                                msg += vbCrLf & " 'Reason': root.SelectNodes('Response/Status').Item(0).Attributes(1) is Nothing"
                            End If
                        Catch ex As Exception
                            msg += vbCrLf & "'Reason' retrieving Error (outer): " & ex.Message
                        End Try

                        msg += "" & vbCrLf
                        objStreamWriter.WriteLine(msg & "  Timestamp: " & Now())
                    End If

                Catch ex As Exception
                    objStreamWriter.WriteLine("Error processing Response: " & ex.Message)
                End Try
            End If
            
        Else
            ' output is empty - send is unsuccessful at all
            objStreamWriter.WriteLine("Received empty Output string. " & Now())
            bIsOK = False
        End If

        objStreamWriter.WriteLine("End of Amazon Client build/send XML " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()

        objStreamWriterXML.Flush()
        objStreamWriterXML.Close()

        objStrmWrtrXMLRspns.Flush()
        objStrmWrtrXMLRspns.Close()

    End Sub


    Private Function getOrderRequest(ByVal strPunSite As String, ByRef OrderListDataSet As System.Data.DataSet, ByRef strMsgVendConfig As String) As String
        'Dim OrderListDataSet As System.Data.DataSet = New System.Data.DataSet()
        Dim rtn As String = "AmazonClient.Module1.getOrderRequest"
        Dim cXML As String = ""

        Dim objStreamWriterXMLN1 As StreamWriter
        Dim objStrmWrtrXMLRspnsN1 As StreamWriter

        m_setupReqDoc = Nothing
        m_vendorConfig = Nothing

        Try

            Dim punchOutSiteId As String = CStr(strPunSite).Trim.ToUpper
            Dim punchOutSiteGrpId As String = "default"

            'Dim userBU As String = ""
            Dim grpDefinitionFile As String = ""
            Dim grpIdentifier As punchOutGroupIdentifier = Nothing

            'Try
            '    'userBU = CStr(Session("BUSUNIT"))
            'Catch ex As Exception
            'End Try

            'this is not used by Amazon - for possible future use
            'Select Case punchOutSiteId
            '    Case "GRAYBAR"
            '        grpDefinitionFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\PunchOutcXML\GraybaR.PriceGrpIdentifier.xml"
            '        grpIdentifier = punchOutGroupIdentifier.LoadPunchOutGroupIdentifier(punchOutSiteId, grpDefinitionFile)
            '    Case "GRAINGER"
            '        grpDefinitionFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\PunchOutcXML\Grainger.PriceGrpIdentifier.xml"
            '        grpIdentifier = punchOutGroupIdentifier.LoadPunchOutGroupIdentifier(punchOutSiteId, grpDefinitionFile)
            '    Case Else
            '        ' just use the default
            'End Select
            'If Not (grpIdentifier Is Nothing) Then
            '    Dim grp As punchOutGrpSites = Nothing
            '    For Each sKey As String In grpIdentifier.Groups.Keys
            '        grp = DirectCast(grpIdentifier.Groups(sKey), punchOutGrpSites)
            '        If grp.easySiteIdSearch.IndexOf(userBU) > -1 Then
            '            punchOutSiteGrpId = grp.Id
            '            Exit For
            '        End If
            '    Next
            '    grp = Nothing
            'End If


            m_vendorConfig = punchoutVendorConfig.GetVendorConfig(punchOutSiteId, punchOutSiteGrpId)
            If Not (m_vendorConfig Is Nothing) Then
                If m_vendorConfig.ToIdentity.Id.Length > 0 And _
                   m_vendorConfig.VendorPunchoutSetupURL.Length > 0 Then

                    'read view, get list of orders
                    Dim strListOrders As String = "select distinct po_id from SYSADM.PS_ISA_PO_DISP_XML"

                    Try
                        Dim Command As OleDbCommand = New OleDbCommand(strListOrders, connectOR)
                        Command.CommandTimeout = 120
                        connectOR.Open()
                        Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(Command)

                        OrderListDataSet = New System.Data.DataSet()

                        dataAdapter.Fill(OrderListDataSet)

                        Dim strOrderNo As String = ""
                        If Not OrderListDataSet Is Nothing Then
                            If OrderListDataSet.Tables.Count > 0 Then
                                If OrderListDataSet.Tables(0).Rows.Count > 0 Then
                                    Dim iLst As Integer = 0


                                    For iLst = 0 To OrderListDataSet.Tables(0).Rows.Count - 1
                                        strOrderNo = OrderListDataSet.Tables(0).Rows(iLst).Item("po_id").ToString()
                                        If iLst = 2 Then
                                            Exit For
                                        End If

                                        Dim filePathN1 As String = "C:\Program Files\sdi\AmazonClient\AmazonXMLFiles\AmazonClientXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
                                        Dim filePathResponseN1 As String = "C:\Program Files\sdi\AmazonClient\AmazonXMLFiles\AmznClntXMLRspns" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
                                        objStrmWrtrXMLRspnsN1 = File.CreateText(filePathResponseN1)
                                        objStreamWriterXMLN1 = File.CreateText(filePathN1)
                                        ' create the doc based on order Request template
                                        m_setupReqDoc = punchOutSetupRequestDoc.CreateOrderRequestDoc(m_vendorConfig)

                                        ' put header info based on info already collected, and build same XML Header as in Punchout; 
                                        ' read view to get Order(s) info, and build XML based on XML order structure supplied
                                        cXML = m_setupReqDoc.CreateOrderRequestXML(connectOR, strOrderNo)

                                        objStreamWriter.WriteLine("Finished building XML out for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                        Dim doc As New XmlDocument
                                        Dim strInput As String = cXML
                                        If Trim(strInput) = "" Then
                                            If Not m_vendorConfig Is Nothing Then
                                                If Not m_vendorConfig.ConfigFile Is Nothing Then
                                                    Try
                                                        strMsgVendConfig = m_vendorConfig.ConfigFile

                                                    Catch ex2 As Exception
                                                        strMsgVendConfig = " 'm_vendorConfig.ConfigFile' is not defined. SubError: " & vbCrLf & ex2.Message & vbCrLf
                                                    End Try
                                                    objStreamWriter.WriteLine(strMsgVendConfig)
                                                End If
                                            End If
                                        Else
                                            doc.InnerXml = strInput
                                        End If

                                        ' start processing strInput
                                        Dim strOutput As String = ""
                                        If Trim(strInput) <> "" Then
                                            objStreamWriter.WriteLine("Saving XML file to send for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                            objStreamWriterXMLN1.WriteLine(strInput)

                                            Call Send1(strInput, strOutput)

                                            objStreamWriter.WriteLine("Saving Response XML file for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                            objStrmWrtrXMLRspnsN1.WriteLine(strOutput)

                                            'start analysing Output string

                                            objStreamWriter.WriteLine("Checking Response XML file for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                            ' check strOutput 
                                            Dim Response_Doc As String
                                            Dim bIsOK As Boolean = False
                                            Response_Doc = Common.RemoveCrLf(strOutput)
                                            If Trim(Response_Doc) <> "" Then

                                                '-----------------------------------------------------------------------
                                                ' Parse the Server response and retrieve XML file
                                                '-----------------------------------------------------------------------

                                                Dim xmlResponse As New XmlDocument
                                                Dim root As XmlElement = Nothing
                                                Try
                                                    xmlResponse.LoadXml(Response_Doc)
                                                    root = xmlResponse.DocumentElement
                                                    bIsOK = True
                                                Catch ex As Exception
                                                    objStreamWriter.WriteLine("Response XML file is NOT checked OK - 'LoadXml' area for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                                    bIsOK = False
                                                End Try
                                                'Dim objnode As XmlNode

                                                If bIsOK Then
                                                    Try
                                                        Try
                                                            If Not root.SelectNodes("Response/Status").Item(0).Attributes(name:="code").Value Is Nothing Then
                                                                Try
                                                                    bIsOK = (root.SelectNodes("Response/Status").Item(0).Attributes(name:="code").Value = "200")
                                                                Catch ex As Exception
                                                                    bIsOK = False
                                                                End Try
                                                            End If

                                                            If Not bIsOK Then
                                                                If Not root.SelectNodes("Response/Status").Item(0).Attributes(name:="text").Value Is Nothing Then
                                                                    Try
                                                                        bIsOK = (root.SelectNodes("Response/Status").Item(0).Attributes(name:="text").Value.ToUpper = "SUCCESS")
                                                                    Catch ex As Exception
                                                                        bIsOK = False
                                                                    End Try
                                                                End If

                                                            End If
                                                            If Not bIsOK Then
                                                                If Not root.SelectNodes("Response/Status").Item(0).Attributes(name:="text").Value Is Nothing Then
                                                                    Try
                                                                        bIsOK = (root.SelectNodes("Response/Status").Item(0).Attributes(name:="text").Value.ToUpper = "OK")
                                                                    Catch ex As Exception
                                                                        bIsOK = False
                                                                    End Try
                                                                End If
                                                            End If
                                                        Catch ex As Exception
                                                            bIsOK = False
                                                        End Try

                                                        If bIsOK Then
                                                            'objStreamWriter.WriteLine("Response XML file checked OK. Changing Order statuses " & Now())
                                                            'Dim strValueToWrite As String = Now.Year.ToString() & Now.Month.ToString() & Now.Day.ToString() & Now.GetHashCode.ToString()
                                                            'Dim intNumberToWrite As Long = 0
                                                            'If IsNumeric(strValueToWrite) Then
                                                            '    intNumberToWrite = CType(strValueToWrite, Long)
                                                            'Else
                                                            '    intNumberToWrite = Now.GetHashCode
                                                            'End If
                                                            'Dim bNoErrors As Boolean = True
                                                            'Dim strOrderNo As String = ""
                                                            'connectOR.Open()
                                                            'Try
                                                            '    Dim iOrdCount As Integer = OrderListDataSet.Tables(0).Rows.Count
                                                            '    ' run this query for every order sent:
                                                            '    ' "update SYSADM.PS_PO_DISPATCHED set ECQUEUEINSTANCE=" & intNumberToWrite & " where po_id='" & "M010373791" & "'"
                                                            '    If connectOR.State = ConnectionState.Open Then
                                                            '    Else
                                                            '        connectOR.Open()
                                                            '    End If
                                                            '    Dim rowsAffected As Integer = 0
                                                            '    Dim iCnt As Integer = 0
                                                            '    For iCnt = 0 To iOrdCount - 1
                                                            '        If iCnt = 1 Then Exit For ' for testing ONLY!
                                                            '        rowsAffected = 0
                                                            '        strOrderNo = OrderListDataSet.Tables(0).Rows(iCnt).Item("po_id").ToString()
                                                            '        'run query
                                                            '        Dim strUpdateQuery As String = "update SYSADM.PS_PO_DISPATCHED set ECQUEUEINSTANCE=" & intNumberToWrite & " where po_id='" & strOrderNo & "'"
                                                            '        'commented out for testing
                                                            '        Dim UpdCommand As OleDbCommand = New OleDbCommand(strUpdateQuery, connectOR)
                                                            '        UpdCommand.CommandTimeout = 120
                                                            '        rowsAffected = UpdCommand.ExecuteNonQuery()
                                                            '        Try
                                                            '            UpdCommand.Dispose()
                                                            '        Catch ex As Exception

                                                            '        End Try
                                                            '        If rowsAffected = 0 Then
                                                            '            bNoErrors = False
                                                            '            objStreamWriter.WriteLine("Order status change returned: 'rowsAffected = 0' for Order: " & strOrderNo)
                                                            '        End If
                                                            '    Next
                                                            'Catch ex As Exception
                                                            '    bNoErrors = False
                                                            '    objStreamWriter.WriteLine("Error trying to update Order: " & strOrderNo & " Error Message: " & ex.Message)
                                                            '    Try
                                                            '        connectOR.Close()
                                                            '    Catch ex1 As Exception

                                                            '    End Try
                                                            'End Try
                                                            'Try
                                                            '    connectOR.Close()
                                                            'Catch ex As Exception

                                                            'End Try
                                                            'If bNoErrors Then
                                                            '    objStreamWriter.WriteLine("Order statuses changed without errors " & Now())
                                                            'Else
                                                            'End If
                                                        Else
                                                            objStreamWriter.WriteLine("Response XML file is NOT checked OK for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                                            Dim msg As String = "" '  & _
                                                            Try
                                                                If Not root.SelectNodes("Response/Status").Item(0).Attributes(0) Is Nothing Then
                                                                    If Not root.SelectNodes("Response/Status").Item(0).Attributes(0).Value Is Nothing Then
                                                                        Try
                                                                            msg += "'Code' = " & root.SelectNodes("Response/Status").Item(0).Attributes(0).Value '  & _
                                                                        Catch ex As Exception
                                                                            msg += "'Code' retrieving Error (inner): " & ex.Message
                                                                        End Try
                                                                    Else
                                                                        msg += vbCrLf & " 'Code': root.SelectNodes('Response/Status').Item(0).Attributes(0).Value is Nothing"
                                                                    End If
                                                                Else
                                                                    msg += vbCrLf & " 'Code': root.SelectNodes('Response/Status').Item(0).Attributes(0) is Nothing"
                                                                End If
                                                            Catch ex As Exception
                                                                msg += vbCrLf & "'Code' retrieving Error (outer): " & ex.Message
                                                            End Try

                                                            Try
                                                                If Not root.SelectNodes("Response/Status").Item(0).Attributes(1) Is Nothing Then
                                                                    If Not root.SelectNodes("Response/Status").Item(0).Attributes(1).Value Is Nothing Then
                                                                        Try
                                                                            msg += "'Reason' = " & root.SelectNodes("Response/Status").Item(0).Attributes(1).Value '  & _
                                                                        Catch ex As Exception
                                                                            msg += "'Reason' retrieving Error (inner): " & ex.Message
                                                                        End Try
                                                                    Else
                                                                        msg += vbCrLf & " 'Reason': root.SelectNodes('Response/Status').Item(0).Attributes(1).Value is Nothing"
                                                                    End If
                                                                Else
                                                                    msg += vbCrLf & " 'Reason': root.SelectNodes('Response/Status').Item(0).Attributes(1) is Nothing"
                                                                End If
                                                            Catch ex As Exception
                                                                msg += vbCrLf & "'Reason' retrieving Error (outer): " & ex.Message
                                                            End Try

                                                            msg += "" & vbCrLf
                                                            objStreamWriter.WriteLine(msg & "  Timestamp: " & Now())
                                                        End If

                                                    Catch ex As Exception
                                                        objStreamWriter.WriteLine("Error processing Response for this OrderNo: " & strOrderNo & " ; Error: " & ex.Message)
                                                    End Try
                                                End If

                                            Else
                                                ' output is empty - send is unsuccessful at all
                                                objStreamWriter.WriteLine("Received empty Output string for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                                bIsOK = False
                                            End If

                                            objStreamWriter.WriteLine("End of Amazon Client build/send XML for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())

                                        Else

                                            objStreamWriter.WriteLine("Input string is empty. Possible cause: " & strMsgVendConfig)

                                        End If

                                        objStreamWriterXMLN1.Flush()
                                        objStreamWriterXMLN1.Close()

                                        objStrmWrtrXMLRspnsN1.Flush()
                                        objStrmWrtrXMLRspnsN1.Close()

                                    Next  '  For iLst = 0 To OrderListDataSet.Tables(0).Rows.Count - 1
                                End If  '  If OrderListDataSet.Tables(0).Rows.Count > 0 Then
                            End If
                        End If
                        Try
                            dataAdapter.Dispose()
                        Catch ex As Exception

                        End Try
                        Try
                            Command.Dispose()
                        Catch ex As Exception

                        End Try
                        Try
                            OrderListDataSet.Dispose()
                        Catch ex As Exception

                        End Try
                        Try
                            connectOR.Close()
                        Catch ex1 As Exception

                        End Try
                    Catch ex As Exception
                        Try
                            connectOR.Close()
                        Catch ex1 As Exception

                        End Try
                    End Try

                End If  ' got Vendor Config
            End If  '  not Nothing

        Catch ex As Exception

        End Try

        objStreamWriter.Flush()
        objStreamWriter.Close()

        'objStreamWriterXML.Flush()
        'objStreamWriterXML.Close()

        'objStrmWrtrXMLRspns.Flush()
        'objStrmWrtrXMLRspns.Close()

        Return cXML

    End Function

    Private Function getPOSR(ByVal strPunSite As String, ByVal userBU As String) As String

        Dim rtn As String = "AmazonClient.Module1.getPOSR"
        Dim cXML As String = ""

        m_setupReqDoc = Nothing
        m_vendorConfig = Nothing

        Try

            Dim punchOutSiteId As String = CStr(strPunSite).Trim.ToUpper
            Dim punchOutSiteGrpId As String = "default"

            'Dim userBU As String = ""
            Dim grpDefinitionFile As String = ""
            Dim grpIdentifier As punchOutGroupIdentifier = Nothing

            'Try
            '    'userBU = CStr(Session("BUSUNIT"))
            'Catch ex As Exception
            'End Try

            Select Case punchOutSiteId
                Case "GRAYBAR"
                    grpDefinitionFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\PunchOutcXML\GraybaR.PriceGrpIdentifier.xml"
                    grpIdentifier = punchOutGroupIdentifier.LoadPunchOutGroupIdentifier(punchOutSiteId, grpDefinitionFile)
                Case "GRAINGER"
                    grpDefinitionFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName) & "\PunchOutcXML\Grainger.PriceGrpIdentifier.xml"
                    grpIdentifier = punchOutGroupIdentifier.LoadPunchOutGroupIdentifier(punchOutSiteId, grpDefinitionFile)
                Case Else
                    ' just use the default
            End Select
            If Not (grpIdentifier Is Nothing) Then
                Dim grp As punchOutGrpSites = Nothing
                For Each sKey As String In grpIdentifier.Groups.Keys
                    grp = DirectCast(grpIdentifier.Groups(sKey), punchOutGrpSites)
                    If grp.easySiteIdSearch.IndexOf(userBU) > -1 Then
                        punchOutSiteGrpId = grp.Id
                        Exit For
                    End If
                Next
                grp = Nothing
            End If


            m_vendorConfig = punchoutVendorConfig.GetVendorConfig(punchOutSiteId, punchOutSiteGrpId)
            If Not (m_vendorConfig Is Nothing) Then
                If m_vendorConfig.ToIdentity.Id.Length > 0 And _
                   m_vendorConfig.VendorPunchoutSetupURL.Length > 0 Then

                    ' create the doc
                    m_setupReqDoc = punchOutSetupRequestDoc.CreatePunchoutSetupRequestDoc(m_vendorConfig)

                    ' SDI site to post back to 
                    m_setupReqDoc.PostbackURL = "http://localhost/InsiteOnline/shopredirect.aspx?PUNOUT=YES"

                    ' this is actually Buyer info - is it in this new view in DEVL?
                    Dim strUserId As String = "VROV1"
                    Dim strBU As String = userBU  '  "I0256"
                    Dim strEmail As String = "vitaly.rovensky@sdi.com"
                    Dim strUserName As String = "Rovensky,Vitaly"

                    ' apply user agent from session ONLY if none was specified from the config file
                    '   for this verndor (<UserAgentOverride> tag).  Old code only specify this on the "Sender" section.
                    If Not (m_vendorConfig.SenderIdentity.Agent.Trim.Length > 0) Then
                        m_vendorConfig.SenderIdentity.Agent = strUserId
                        m_setupReqDoc.SenderIdentity.Agent = strUserId
                    End If

                    '' apply Extrinsic (Buyer info) property values
                    m_setupReqDoc.BuyerInfo.Name = strUserName
                    m_setupReqDoc.BuyerInfo.EMail = strEmail
                    m_setupReqDoc.BuyerInfo.CostCenter = strBU
                    m_setupReqDoc.BuyerInfo.SessionCookie = "LokYa24fg657u"  '  "KolYa24fg657u" "NastYa24fg657u"

                    Dim strCustId As String = "90425"
                    Dim strCompanyName As String = "UNCC Facility Maint. Shop"

                    'Try
                    '    'strCustId = Session("CUSTID")
                    '    'strCompanyName = Session("CONAME")
                    'Catch ex As Exception
                    'End Try

                    ' customer Id and company name
                    m_setupReqDoc.ShipToInfo.AddressId = strCustId
                    m_setupReqDoc.ShipToInfo.Name = strCompanyName

                    '// shipTo information

                    ' assign the default shipTo information (this was on the original code)
                    m_setupReqDoc.ShipToInfo.DeliverTo = "Strategic Distribution, Inc."
                    m_setupReqDoc.ShipToInfo.Street = "1414 Radcliffe Street"
                    m_setupReqDoc.ShipToInfo.City = "Bristol"
                    m_setupReqDoc.ShipToInfo.State = "PA"
                    m_setupReqDoc.ShipToInfo.ZIPCode = "19007-5423"

                    ' retrieve shipTo location/information using
                    '   logged in user's business unit, OVERRIDE DEFAULT info
                    Dim userShipToInfo As ShipTo = GetSiteShipToLocation(userBU)
                    If Not (userShipToInfo Is Nothing) Then
                        m_setupReqDoc.ShipToInfo = userShipToInfo
                    End If

                    ' check if shipTo id/name was specified within the PunchOut.xml configuration file
                    '   override default/retrieved values with what was configured within the file
                    If m_vendorConfig.ShipToOverride.AddressId.Trim.Length > 0 Then
                        m_setupReqDoc.ShipToInfo.AddressId = m_vendorConfig.ShipToOverride.AddressId
                    End If
                    If m_vendorConfig.ShipToOverride.AddressName.Trim.Length > 0 Then
                        m_setupReqDoc.ShipToInfo.AddressName = m_vendorConfig.ShipToOverride.AddressName
                    End If

                    ' cXML to return ('ToString' is a special function which is actually creating XML file! Not the regular ToString() )

                    cXML = m_setupReqDoc.ToString

                End If
            End If
        Catch ex As Exception

            'SendSDiExchErrorMail(rtn & ":: Not processed error during Punchout: " & ex.Message)
        End Try

        Return cXML

    End Function

    Private Function GetSiteShipToLocation(ByVal userBU As String) As ShipTo
        Dim shipToInfo As ShipTo = Nothing
        Dim sql As String = ""

        userBU = userBU.Trim.ToUpper

        sql &= "SELECT "
        sql &= "  A.LOCATION "
        sql &= ", A.ISA_BUSINESS_UNIT "
        sql &= ", A.BUSINESS_UNIT "
        sql &= ", B.DESCR "
        sql &= ", B.ADDRESS1 "
        sql &= ", B.ADDRESS2 "
        sql &= ", B.CITY "
        sql &= ", B.STATE "
        sql &= ", B.POSTAL "
        sql &= ", B.COUNTRY "
        sql &= ", B.SETID "
        sql &= ", B.LOCATION "
        sql &= ", TO_CHAR(B.EFFDT,'YYYY-MMDD') "
        sql &= "FROM "
        sql &= "  PS_ISA_SDR_BU_LOC A "
        sql &= ", PS_LOCATION_TBL B "
        sql &= "WHERE "
        sql &= "      A.LOCATION LIKE '%-01' "
        sql &= "  AND A.BU_STATUS = '1' "
        sql &= "  AND A.SETID = B.SETID "
        sql &= "  AND A.LOCATION = B.LOCATION "
        sql &= "  AND B.EFFDT = ( "
        sql &= "                  SELECT MAX(B_ED.EFFDT) "
        sql &= "                  FROM PS_LOCATION_TBL B_ED "
        sql &= "                  WHERE "
        sql &= "                        B.SETID = B_ED.SETID "
        sql &= "                    AND B.LOCATION = B_ED.LOCATION "
        sql &= "                    AND B_ED.EFFDT <= SYSDATE "
        sql &= "                ) "
        sql &= "  AND A.SETID = 'MAIN1' "
        sql &= "  AND A.ISA_BUSINESS_UNIT = '" & userBU & "' "


        Dim rdr As OleDbDataReader
        Try
            Dim Command As OleDbCommand = New OleDbCommand(sql, connectOR)
            Command.CommandTimeout = 120
            connectOR.Open()
            rdr = Command.ExecuteReader(CommandBehavior.CloseConnection)

            Try
                Command.Dispose()
            Catch ex As Exception

            End Try
        Catch ex As Exception
            rdr = Nothing
            Try
                connectOR.Close()
            Catch ex1 As Exception

            End Try
        End Try

        If Not (rdr Is Nothing) Then
            ' parse the first record - should ONLY return 1
            If rdr.Read Then
                shipToInfo = New ShipTo
                Try
                    shipToInfo.AddressId = CStr(rdr("LOCATION")).Trim
                Catch ex As Exception
                End Try
                Try
                    shipToInfo.AddressName = CStr(rdr("DESCR")).Trim
                Catch ex As Exception
                End Try
                Try
                    shipToInfo.Name = CStr(rdr("DESCR")).Trim
                Catch ex As Exception
                End Try
                Try
                    shipToInfo.DeliverTo = CStr(rdr("ADDRESS1")).Trim
                Catch ex As Exception
                End Try
                Try
                    shipToInfo.Street = CStr(rdr("ADDRESS2")).Trim
                Catch ex As Exception
                End Try
                Try
                    shipToInfo.City = CStr(rdr("CITY")).Trim
                Catch ex As Exception
                End Try
                Try
                    shipToInfo.State = CStr(rdr("STATE")).Trim
                Catch ex As Exception
                End Try
                Try
                    shipToInfo.ZIPCode = CStr(rdr("POSTAL")).Trim
                Catch ex As Exception
                End Try
            End If
        End If

        Try
            rdr.Close()
        Catch ex As Exception
        Finally
            rdr = Nothing
        End Try
        Try
            connectOR.Close()
        Catch ex1 As Exception

        End Try

        Return shipToInfo
    End Function

    Private Sub Send1(ByRef strBox1 As String, ByRef strBox2 As String)

        strBox2 = ""

        ' new secure on IMS -   "https://sdiexchtest.sdi.com/WebSvcSDI/xmlinsdi.aspx"
        '  my test URL: "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"   ' not seen outside: "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"   '  
        ' "https://https.amazonsedi.com/073dbe31-c230-403f-990c-6f74eeed1510"  '  
        ' new one:  "http://localhost/SDIWebProcessors/CytecPurchReqs.aspx"    '   "http://ims.sdi.com:8913/sdiwebinSvc/CytecMatMastIn.aspx"  
        '  "http://ims.sdi.com:8913/sdiwebinSvc/CytecNstkPoRecpts.aspx"   '  "http://192.168.253.46:8011/sdiwebin/CytecMatMastIn.aspx"

        Dim sHttpResponse As String = ""
        Dim httpSession As New easyHttp

        httpSession.URL = "https://sdiexchtest.sdi.com/WebSvcSDI/xmlinsdi.aspx"  '   "https://https.amazonsedi.com/c47fcf9d-286d-498a-ba9f-df390c2757a2"  '  "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"    '  "http://192.168.253.46:8011/sdiwebin/CytecMatMastIn.aspx"  '   "http://ims.sdi.com:8913/sdiwebinSvc/CytecNstkPoRecpts.aspx"   ' "http://ims.sdi.com:8913/sdiwebinSvc/CytecPurchReqs.aspx"    '  "http://ims.sdi.com:8913/sdiwebinSvc/CytecStkReservIn.aspx"    '  "http://ims.sdi.com:8913/sdiwebinSvc/CytecMatMastIn.aspx"    '  "http://localhost/SDIWebProcessors/CytecMatMastIn.aspx"    '    "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"  '    "http://localhost/SDIWebProcessors/XmlInSDI.aspx"   '  "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx" 
        '   "https://https.amazonsedi.com/073dbe31-c230-403f-990c-6f74eeed1510"  '    "https://www.amazon.com/eprocurement/punchout"  '    "https://supplydev.hajoca.com/hajomid/eclipse.ecl"

        httpSession.DataToPost = strBox1
        httpSession.ContentType = "text/xml; charset=utf-8"
        httpSession.Method = easyHttp.HTTPMethod.HTTP_POST
        httpSession.IgnoreServerCertificate = True
        httpSession.HeaderAttributes.Add(name:="SOAPAction", value:="https://schemas.microsoft.com/crm/2006/WebServices/Retrieve")

        sHttpResponse = ""

        Try
            'sHttpResponse = httpSession.SendAsBytes
            sHttpResponse = httpSession.SendAsString
        Catch ex As Exception
            sHttpResponse &= ex.Message
        End Try

        httpSession = Nothing

        strBox2 = sHttpResponse

    End Sub

    Private Function RemoteCertValidate(ByVal sender As Object, ByVal cert As X509Certificate, ByVal chain As X509Chain, ByVal [error] As System.Net.Security.SslPolicyErrors) As Boolean
        Return True
    End Function

    Private Sub Send2(ByVal strBox1 As String, ByVal strBox2 As String)

        Dim stepId As String = "start"

        strBox2 = ""

        Try

            ' for Weinstein
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls

            'System.Net.ServicePointManager.Expect100Continue = False

            'System.Net.ServicePointManager.UseNagleAlgorithm = False
            'System.Net.ServicePointManager.DefaultConnectionLimit = 1000
            'System.Net.ServicePointManager.SetTcpKeepAlive(True, 30000, 30000)
            ''VR 03/17/2015 this is the solution:
            'ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf RemoteCertValidate)

            Dim o As New System.Net.WebClient

            Dim oParam As New Specialized.NameValueCollection

            stepId = "before param"
            oParam.Add("SOAPAction", "http://schemas.microsoft.com/crm/2006/WebServices/Retrieve")
            oParam.Add("Content-Type", "text/xml; charset=utf-8")
            oParam.Add("Content-Length", strBox1.Length)
            oParam.Add("Content", strBox1)

            stepId = "before UploadValues"
            Dim resBytes As Byte() = o.UploadValues("https://supplydev.hajoca.com/hajomid/eclipse.ecl", "POST", oParam)

            stepId = "before read serverresponse"
            Dim resBody As Object = (New System.Text.UTF8Encoding).GetString(resBytes)

            strBox2 = resBody

            o.Dispose()
            o = Nothing

        Catch ex As Exception
            strBox2 = stepId & "::" & ex.ToString
        End Try

        'Dim o As New SDI.PunchOut.easyHttp

        'o.Method = SDI.PunchOut.easyHttp.HTTPMethod.HTTP_POST
        'o.URL = "https://supplydev.hajoca.com/hajomid/eclipse.ecl"
        'o.DataToPost = Me.TextBox1.Text

        'Me.TextBox2.Text = o.SendUsingWebClient

        'o = Nothing

    End Sub
End Module
