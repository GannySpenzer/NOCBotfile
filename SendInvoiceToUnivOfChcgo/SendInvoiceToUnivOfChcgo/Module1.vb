Imports System.Net
Imports System.IO
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.Web
Imports System.Data
Imports System.Data.OleDb
Imports System.Xml
Imports System.Text
Imports System.Text.Encoding

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

    Dim objStreamWriterXMLN1 As StreamWriter
    Dim objStrmWrtrXMLRspnsN1 As StreamWriter

    ' if folder changed - make sure check all code for EXPLICIT directory settings like 'C:\Program Files\sdi\AmazonClient'!!!

    Dim strUrlToSend As String = "https://usertest-messages.sciquest.com/apps/Router/CXMLInvoiceImport"  ' VR 03/15/2018 - new from AMAZON  '   "https://https.amazonsedi.com/c47fcf9d-286d-498a-ba9f-df390c2757a2"
    Dim rootDir As String = "C:\Program Files\SDI\SendInvoiceUnivChcgo"
    Dim logpath As String = "C:\Program Files\SDI\SendInvoiceUnivChcgo\LOGS\UnivChcgoClientOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim filePath As String = "C:\Program Files\SDI\SendInvoiceUnivChcgo\XMLFiles\UnivChcgoClientXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
    Dim filePathResponse As String = "C:\Program Files\SDI\SendInvoiceUnivChcgo\XMLFiles\UnivChcgoClntXMLRspns" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=RPTG")

    Sub Main()

        Dim strInput As String = ""  '  <?xml version=""1.0"" encoding=""UTF-8""?><!DOCTYPE cXML SYSTEM ""http://xml.cxml.org/schemas/cXML/1.2.013/cXML.dtd""[]><cXML payloadID=""3/30/2015 11:56:16 AM 019768490@sdi.com"" xml:lang=""en-US"" timestamp=""3/30/2015 11:56:16 AM""><Header><From><Credential domain=""NetworkId""><Identity>SDIINC</Identity></Credential></From><To><Credential domain=""NetworkId""><Identity>Amazon</Identity></Credential></To><Sender><Credential domain=""DUNS""><Identity>SDIINC</Identity><SharedSecret>Y2XN7SefSxpPAoD5i6OtYix4w5TK402d</SharedSecret></Credential><UserAgent>Ariba Network 1.2</UserAgent></Sender></Header><Request><PunchOutSetupRequest operation=""create""><BuyerCookie>3xx1vu5dn5sttwrc2zqspprl</BuyerCookie><Extrinsic name=""UniqueName"">ROVENSKY,VITALY</Extrinsic><Extrinsic name=""UserEmail"">vitaly.rovensky@sdi.com</Extrinsic><Extrinsic name=""CostCenter"">I0256</Extrinsic><BrowserFormPost><URL>http://localhost/InsiteOnline/shopredirect.aspx?PUNOUT=YES</URL></BrowserFormPost><ShipTo><Address addressID=""L0256-01""><Name xml:lang=""en-US"">UNCC Facility Maint. Shop</Name><PostalAddress><DeliverTo>SDI c/o UNCC Facility Maint Shop</DeliverTo><Street>9201 University City Blvd.</Street><City>Charlotte</City><State>NC</State><PostalCode>28223</PostalCode><Country isoCountryCode=""US"">United States</Country></PostalAddress></Address></ShipTo></PunchOutSetupRequest></Request></cXML>"
        Dim strOutput As String = ""
        Dim strWhatToTest As String = "UCHICAGO"
        Dim Response_Doc As String = ""
        Dim msgEx As String = ""
        Dim strMsgVendConfig As String = ""
        objStreamWriter = File.CreateText(logpath)

        Console.WriteLine("Started to check Univ. of Chicago ready to send Invoices ")
        Console.WriteLine("")

        objStreamWriter.WriteLine("Started to check Univ. of Chicago ready to send Invoices " & Now())

        '  URL send To
        Dim rUrl As String = ""
        Try
            rUrl = My.Settings("UrlToSend").ToString.Trim
        Catch ex As Exception
        End Try
        If (rUrl.Length > 0) Then
            strUrlToSend = rUrl
        End If

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

        Dim strPunSite As String = ""

        strMsgVendConfig = " 'm_vendorConfig.ConfigFile' is not defined"
        Try
            objStreamWriter.WriteLine("Started building XML out " & Now())

            ' function to send cXML file to Univ. Chicago


        Catch ex As Exception
            msgEx = "Not a valid identity or vendor URL for this catalog.<BR>Please report error" & _
                                          vbCrLf & "config =" & strMsgVendConfig & _
                                          vbCrLf & "ERROR:: " & vbCrLf & ex.ToString & _
                                          ""

            objStreamWriter.WriteLine(msgEx)

            Exit Sub
        End Try

        Exit Sub

    End Sub

End Module
