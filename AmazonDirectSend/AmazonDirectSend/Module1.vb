Imports System.Net
Imports System.IO
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.Web
Imports System.Data
Imports System.Data.OleDb
Imports System.Xml
Imports System.Text
Imports System.Configuration
Imports System.Text.Encoding


Module Module1

    'WARNING!!! This project is read Oracle for AMAZON orders and send them to Amazon as cXML file - VR 03/06/2018


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

    Dim strUrlToSend As String = "https://https-ats.amazonsedi.com/5375ff74-a613-4b12-9555-41bcd38c5fd0"  ' VR 03/15/2018 - new from AMAZON  '   "https://https.amazonsedi.com/c47fcf9d-286d-498a-ba9f-df390c2757a2"
    Dim rootDir As String = "C:\Program Files\sdi\AmazonClient"
    Dim logpath As String = "C:\Program Files\sdi\AmazonClient\AmazonLOGS\AmazonClientOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim filePath As String = "C:\Program Files\sdi\AmazonClient\AmazonXMLFiles\AmazonClientXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
    Dim filePathResponse As String = "C:\Program Files\sdi\AmazonClient\AmazonXMLFiles\AmznClntXMLRspns" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=DEVL")
    Dim cnString As String = ""
    Sub Main()

        Dim strInput As String = ""  '  <?xml version=""1.0"" encoding=""UTF-8""?><!DOCTYPE cXML SYSTEM ""http://xml.cxml.org/schemas/cXML/1.2.013/cXML.dtd""[]><cXML payloadID=""3/30/2015 11:56:16 AM 019768490@sdi.com"" xml:lang=""en-US"" timestamp=""3/30/2015 11:56:16 AM""><Header><From><Credential domain=""NetworkId""><Identity>SDIINC</Identity></Credential></From><To><Credential domain=""NetworkId""><Identity>Amazon</Identity></Credential></To><Sender><Credential domain=""DUNS""><Identity>SDIINC</Identity><SharedSecret>Y2XN7SefSxpPAoD5i6OtYix4w5TK402d</SharedSecret></Credential><UserAgent>Ariba Network 1.2</UserAgent></Sender></Header><Request><PunchOutSetupRequest operation=""create""><BuyerCookie>3xx1vu5dn5sttwrc2zqspprl</BuyerCookie><Extrinsic name=""UniqueName"">ROVENSKY,VITALY</Extrinsic><Extrinsic name=""UserEmail"">vitaly.rovensky@sdi.com</Extrinsic><Extrinsic name=""CostCenter"">I0256</Extrinsic><BrowserFormPost><URL>http://localhost/InsiteOnline/shopredirect.aspx?PUNOUT=YES</URL></BrowserFormPost><ShipTo><Address addressID=""L0256-01""><Name xml:lang=""en-US"">UNCC Facility Maint. Shop</Name><PostalAddress><DeliverTo>SDI c/o UNCC Facility Maint Shop</DeliverTo><Street>9201 University City Blvd.</Street><City>Charlotte</City><State>NC</State><PostalCode>28223</PostalCode><Country isoCountryCode=""US"">United States</Country></PostalAddress></Address></ShipTo></PunchOutSetupRequest></Request></cXML>"
        Dim strOutput As String = ""
        'IPM-145 Need to ensure Amazon CXML process works
        Dim strWhatToTest As String = "EMCORAMAZON"   ' "NEW_AMAZON"  '    "CYTECMXM" 
        Dim Response_Doc As String
        Dim msgEx As String = ""
        Dim strMsgVendConfig As String = ""
        objStreamWriter = File.CreateText(logpath)

        ' This is for SDI Direct Amazon project
        Console.WriteLine("Started to check Amazon ready to Dispatch orders ")
        Console.WriteLine("")

        objStreamWriter.WriteLine("Started to check Amazon ready to Dispatch orders " & Now())

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
        'IPM-145 Need to ensure Amazon CXML process works
        Dim strPunSite As String = "EMCORAMAZON"
        ' list of orders - will be used later to change status flag
        Dim OrderListDataSet As System.Data.DataSet = New System.Data.DataSet()

        ' get info from The view (which is currently in PROD) - SYSADM8.PS_ISA_PO_DISP_XML.      

        strMsgVendConfig = " 'm_vendorConfig.ConfigFile' is not defined"
        objStreamWriter.WriteLine(strUrlToSend)
        Try
            objStreamWriter.WriteLine("Started building XML out " & Now())
            'process view

            ' function to send order request - using strPunSite only
            strInput = getOrderRequest(strPunSite, OrderListDataSet, strMsgVendConfig)

        Catch ex As Exception
            msgEx = "Not a valid identity or vendor URL for this catalog.<BR>Please report error" &
                                          vbCrLf & "config =" & strMsgVendConfig &
                                          vbCrLf & "ERROR:: " & vbCrLf & ex.ToString &
                                          ""

            objStreamWriter.WriteLine(msgEx)

            Exit Sub
        End Try

        Exit Sub

    End Sub

    Private Function getOrderRequest(ByVal strPunSite As String, ByRef OrderListDataSet As System.Data.DataSet, ByRef strMsgVendConfig As String) As String

        Dim rtn As String = "AmazonClient.Module1.getOrderRequest"
        Dim cXML As String = ""

        m_setupReqDoc = Nothing
        m_vendorConfig = Nothing

        Try

            Dim punchOutSiteId As String = CStr(strPunSite).Trim.ToUpper
            Dim punchOutSiteGrpId As String = "default"

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


            objStreamWriter.WriteLine("Started GetVendorConfig. Params: " & punchOutSiteId & " ; " & punchOutSiteGrpId & " ; " & Now())
            m_vendorConfig = punchoutVendorConfig.GetVendorConfig(punchOutSiteId, punchOutSiteGrpId)
            If Not (m_vendorConfig Is Nothing) Then
                objStreamWriter.WriteLine("VendorConfig is not Nothing " & Now())
                If m_vendorConfig.ToIdentity.Id.Length > 0 And
                   m_vendorConfig.VendorPunchoutSetupURL.Length > 0 Then
                    objStreamWriter.WriteLine("Got VendorConfig " & Now())

                    'read view, get list of orders
                    Dim strListOrders As String = "select distinct po_id from SYSADM8.PS_ISA_PO_DISP_XML where business_unit = 'EMC00'"  '   WHERE PO_ID = 'S010792283'"
                    'Dim strListOrders As String = "select PO_ID from SYSADM8.PS_ISA_PO_DISP_XML WHERE PO_ID = 'E015087861'"  '   WHERE PO_ID = 'S010792283'" 

                    Try
                        Dim Command As OleDbCommand = New OleDbCommand(strListOrders, connectOR)
                        Command.CommandTimeout = 120
                        connectOR.Open()
                        Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(Command)

                        OrderListDataSet = New System.Data.DataSet()

                        dataAdapter.Fill(OrderListDataSet)

                        objStreamWriter.WriteLine("Filled Dataset " & Now())
                        Dim strOrderNo As String = ""
                        If Not OrderListDataSet Is Nothing Then
                            If OrderListDataSet.Tables.Count > 0 Then
                                If OrderListDataSet.Tables(0).Rows.Count > 0 Then
                                    Dim iLst As Integer = 0

                                    objStreamWriter.WriteLine("Have data " & Now())
                                    For iLst = 0 To OrderListDataSet.Tables(0).Rows.Count - 1
                                        strOrderNo = OrderListDataSet.Tables(0).Rows(iLst).Item("po_id").ToString()
                                        'If iLst = 1 Then
                                        '    Exit For
                                        'End If

                                        objStreamWriter.WriteLine("Before CreateText " & Now())
                                        Dim filePathN1 As String = "C:\Program Files\sdi\AmazonClient\AmazonXMLFiles\AmazonClientXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
                                        Dim filePathResponseN1 As String = "C:\Program Files\sdi\AmazonClient\AmazonXMLFiles\AmznClntXMLRspns" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".xml"
                                        objStrmWrtrXMLRspnsN1 = File.CreateText(filePathResponseN1)
                                        objStreamWriterXMLN1 = File.CreateText(filePathN1)

                                        objStreamWriter.WriteLine("Before CreateOrderRequestDoc " & Now())
                                        ' create the doc based on order Request template
                                        m_setupReqDoc = punchOutSetupRequestDoc.CreateOrderRequestDoc(m_vendorConfig)

                                        objStreamWriter.WriteLine("Before CreateOrderRequestXML " & Now())
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

                                        objStreamWriter.WriteLine("Before Send1 " & Now())
                                        ' start processing strInput
                                        Dim strOutput As String = ""
                                        If Trim(strInput) <> "" Then
                                            objStreamWriter.WriteLine("Saving XML file to send for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                            objStreamWriterXMLN1.WriteLine(strInput)

                                            Dim strCreds As String = ""
                                            Call Send1(strInput, strOutput, strCreds)

                                            objStreamWriter.WriteLine("Creds + : " & strCreds)
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
                                                            objStreamWriter.WriteLine("Response XML file checked OK. Changing Order statuses " & Now())
                                                            Dim strValueToWrite As String = Now.Year.ToString() & Now.Month.ToString() & Now.Day.ToString() & Now.GetHashCode.ToString()
                                                            Dim intNumberToWrite As Long = 0
                                                            If IsNumeric(strValueToWrite) Then
                                                                intNumberToWrite = CType(strValueToWrite, Long)
                                                            Else
                                                                intNumberToWrite = Now.GetHashCode
                                                            End If
                                                            Dim bNoErrors As Boolean = True

                                                            connectOR.Open()
                                                            'IPM-145 Need to ensure Amazon CXML process works
                                                            Try
                                                                'Dim iOrdCount As Integer = OrderListDataSet.Tables(0).Rows.Count
                                                                ' run query for every order sent
                                                                If connectOR.State = ConnectionState.Open Then
                                                                Else
                                                                    connectOR.Open()
                                                                End If
                                                                Dim rowsAffected As Integer = 0
                                                                Dim iCnt As Integer = 0
                                                                'For iCnt = 0 To iOrdCount - 1
                                                                'If iCnt = 1 Then Exit For ' for testing ONLY!
                                                                rowsAffected = 0
                                                                'strOrderNo = OrderListDataSet.Tables(0).Rows(iCnt).Item("po_id").ToString()
                                                                'run query
                                                                Dim strToWrite As String = intNumberToWrite.ToString()
                                                                    If Microsoft.VisualBasic.Left(strToWrite, 1) = "-" Then
                                                                        strToWrite = Mid(strToWrite, 2)
                                                                    End If
                                                                    Dim strUpdateQuery As String = "update SYSADM8.PS_PO_DISPATCHED set EIP_CTL_ID='" & intNumberToWrite.ToString() & "' where po_id='" & strOrderNo & "'"

                                                                    Dim UpdCommand As OleDbCommand = New OleDbCommand(strUpdateQuery, connectOR)
                                                                    UpdCommand.CommandTimeout = 120
                                                                    rowsAffected = UpdCommand.ExecuteNonQuery()
                                                                    Try
                                                                        UpdCommand.Dispose()
                                                                    Catch ex As Exception

                                                                    End Try
                                                                    If rowsAffected = 0 Then
                                                                        bNoErrors = False
                                                                        objStreamWriter.WriteLine("Order status change returned: 'rowsAffected = 0' for Order: " & strOrderNo)
                                                                    End If
                                                                'Next
                                                            Catch ex As Exception
                                                                bNoErrors = False
                                                                objStreamWriter.WriteLine("Error trying to update Order: " & strOrderNo & " Error Message: " & ex.Message)
                                                                Try
                                                                    connectOR.Close()
                                                                Catch ex1 As Exception

                                                                End Try
                                                            End Try
                                                            Try
                                                                connectOR.Close()
                                                            Catch ex As Exception

                                                            End Try
                                                            If bNoErrors Then
                                                                objStreamWriter.WriteLine("Order statuses changed without errors " & Now())
                                                            Else
                                                            End If
                                                        Else
                                                            objStreamWriter.WriteLine("Response XML file is NOT checked OK for this OrderNo: " & strOrderNo & " ; Date/Time: " & Now())
                                                            Dim msg As String = ""
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

                                            If Not bIsOK Then
                                                'send err email
                                                Call SendErrorEmail(strOrderNo, strOutput, filePathResponseN1)
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

                        objStreamWriter.WriteLine("Before Dispose ")

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
                        Dim error_Message As String = ex.Message
                        Dim str_OrderNo As String = ""
                        Try
                            connectOR.Close()
                        Catch ex1 As Exception

                        End Try
                        'INC0028226 - Triggering error email when utility doesnt run
                        Call Module1.SendErrorEmail(str_OrderNo, error_Message, logpath)
                        objStreamWriter.WriteLine("Error: " & ex.Message & vbCrLf & " ; Trace: " & ex.StackTrace)

                    End Try

                End If  ' got Vendor Config

                objStreamWriter.WriteLine("After  End If  ' got Vendor Config")

            End If  '  not Nothing

            objStreamWriter.WriteLine("After  End If  '  not Nothing")

        Catch ex As Exception

            objStreamWriter.WriteLine("Error: " & ex.Message)

        End Try

        objStreamWriter.Flush()
        objStreamWriter.Close()

        Return cXML

    End Function

    Private Sub Send1(ByRef strBox1 As String, ByRef strBox2 As String, Optional ByRef strCreds As String = "")

        strBox2 = ""

        ' new secure on SDIX (Production): "https://www.sdiexchange.com/websdi/xmlinsdi.aspx"
        ' new secure on IMS -   "https://sdiexchtest.sdi.com/WebSvcSDI/xmlinsdi.aspx"
        '  my test URL: "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"   ' not seen outside: "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"   '  
        ' Amazon SDI Direct (test): "https://https.amazonsedi.com/073dbe31-c230-403f-990c-6f74eeed1510"  '  
        ' new one:  "http://localhost/SDIWebProcessors/CytecPurchReqs.aspx"    '   "http://ims.sdi.com:8913/sdiwebinSvc/CytecMatMastIn.aspx"  
        '  "http://ims.sdi.com:8913/sdiwebinSvc/CytecNstkPoRecpts.aspx"   '  "http://192.168.253.46:8011/sdiwebin/CytecMatMastIn.aspx"

        Dim sHttpResponse As String = ""
        Dim httpSession As New easyHttp

        ' for KLA-Tencor: "https://sdiexchtest.sdi.com/WebSvcSDI/KLATencor.aspx "  '  "http://sdixbatch.sdi.com:8084/SDIWebSvcIn/KLATencor.aspx"

        ' 03/28/2018 Newer Amazon URL: "https://https-ats.amazonsedi.com/5375ff74-a613-4b12-9555-41bcd38c5fd0"

        httpSession.URL = strUrlToSend

        'httpSession.URL = "https://sdiexchtest.sdi.com/SDIWebProcessors/CytecPurchReqs.aspx"  ' strUrlToSend  '  "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"  

        '  "https://https.amazonsedi.com/c47fcf9d-286d-498a-ba9f-df390c2757a2"  '  "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"    '  "http://192.168.253.46:8011/sdiwebin/CytecMatMastIn.aspx"  '   "http://ims.sdi.com:8913/sdiwebinSvc/CytecNstkPoRecpts.aspx"   ' "http://ims.sdi.com:8913/sdiwebinSvc/CytecPurchReqs.aspx"    '  "http://ims.sdi.com:8913/sdiwebinSvc/CytecStkReservIn.aspx"    '  "http://ims.sdi.com:8913/sdiwebinSvc/CytecMatMastIn.aspx"    '  "http://localhost/SDIWebProcessors/CytecMatMastIn.aspx"    '    "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx"  '    "http://localhost/SDIWebProcessors/XmlInSDI.aspx"   '  "http://ims.sdi.com:8913/sdiwebinSvc/xmlinsdi.aspx" 
        '   "https://https.amazonsedi.com/073dbe31-c230-403f-990c-6f74eeed1510"  '    "https://www.amazon.com/eprocurement/punchout"  '    "https://supplydev.hajoca.com/hajomid/eclipse.ecl"

        Dim strUserNm1 As String = "SDIOrdering"
        Dim strPass12wd As String = "SDIZeus2019"

        Try
            strUserNm1 = My.Settings("UsNam1").ToString.Trim
        Catch ex As Exception
            strUserNm1 = "SDIOrdering"
        End Try

        Try
            strPass12wd = My.Settings("AmznPsswd").ToString.Trim
        Catch ex As Exception
            strPass12wd = "SDIZeus2019"
        End Try
        strCreds = strUserNm1 & " " & strPass12wd

        Dim basicAuthBase641 As String = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", strUserNm1, strPass12wd)))

        httpSession.DataToPost = strBox1
        httpSession.ContentType = "text/xml"
        httpSession.Method = easyHttp.HTTPMethod.HTTP_POST
        httpSession.IgnoreServerCertificate = True
        httpSession.HeaderAttributes.Add(name:="Authorization", value:=String.Format("Basic {0}", basicAuthBase641))
        httpSession.HeaderAttributes.Add(name:="SOAPAction", value:="https://schemas.microsoft.com/crm/2006/WebServices/Retrieve")

        sHttpResponse = ""
        Dim strErrs As String = ""

        Try
            'sHttpResponse = httpSession.SendAsBytes
            sHttpResponse = httpSession.SendAsString(strErrs)
        Catch ex As Exception
            sHttpResponse &= ex.Message
        End Try

        httpSession = Nothing

        strBox2 = sHttpResponse

        If Trim(strErrs) <> "" Then
            strBox2 = strBox2 & " Messages from httpSession.SendAsString: " & strErrs
        End If

    End Sub

    Private Function RemoteCertValidate(ByVal sender As Object, ByVal cert As X509Certificate, ByVal chain As X509Chain, ByVal [error] As System.Net.Security.SslPolicyErrors) As Boolean
        Return True
    End Function

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As AmazonSDIDirectEmailSvc.EmailServices = New AmazonSDIDirectEmailSvc.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

    Private Sub SendErrorEmail(ByVal strOrdrNo As String, ByVal errorMessage As String, ByVal path As String)
        Dim emailid As String = ""
        Try
            Dim strSQLstring As String = "select a.buyer_id, c.emailid from ps_buyer_tbl a, PS_PO_HDR b, PSOPRDEFN c where a.buyer_id = b.BUYER_ID and a.buyer_id = c.oprid and b.PO_ID= '" & strOrdrNo & "'"
            Dim Command As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)
            Command.CommandTimeout = 120
            connectOR.Open()
            Dim dataAdapter As OleDbDataAdapter = New OleDbDataAdapter(Command)
            Dim emailidDataSet As System.Data.DataSet = New System.Data.DataSet()
            dataAdapter.Fill(emailidDataSet)
            emailid = emailidDataSet.Tables(0).Rows(0).Item("emailid").ToString()
        Catch ex As Exception

        End Try

        Dim rtn As String = "Module1.SendErrorEmail"
        Dim strEmailCc As String = ""
        Dim strEmailBcc As String = ""
        Dim strEmailTo As String = ""
        Dim strEmailSubject As String = ""

        Dim test_Emailto As String = ConfigurationSettings.AppSettings("test_mail").ToString()
        Dim prod_Emailto As String = ConfigurationSettings.AppSettings("prod_mail").ToString()
        'If cnString.Substring(cnString.Length - 4).ToUpper = "PLGR" Or
        '           cnString.Substring(cnString.Length - 4).ToUpper = "STAR" Or
        '           cnString.Substring(cnString.Length - 4).ToUpper = "DEVL" Or
        '            cnString.Substring(cnString.Length - 4).ToUpper = "SNBX" Or
        '            cnString.Substring(cnString.Length - 5).ToUpper = "FSTST" Or
        '             cnString.Substring(cnString.Length - 5).ToUpper = "FSUAT" Or
        '       cnString.Substring(cnString.Length - 4).ToUpper = "RPTG" Then
        If Not getDBName() Then

            If strOrdrNo <> "" Then
                strEmailTo = test_Emailto
                strEmailSubject = " <<Test>> 'Amazon SDI Direct send XML out' was completed with Error(s) for Order No: " & strOrdrNo
            Else
                strEmailTo = test_Emailto
                strEmailSubject = " <<Test>> 'Amazon SDI Direct send XML out' was completed with Error(s)"
            End If

        Else
            'strEmailTo = If(emailid = "", "WebDev@sdi.com", emailid)
            strEmailTo = If(emailid = "", prod_Emailto, emailid & ";" & prod_Emailto)
            'strEmailBcc = "WebDev@sdi.com" 
            'strEmailBcc = "WebDevNotifications@sdi.com"   [webdevnotification is added to prod_Emailto, so excluded]
            If strOrdrNo <> "" Then
                strEmailSubject = "Amazon SDI Direct send XML out' was completed with Error(s) for Order No: " & strOrdrNo
            Else
                strEmailSubject = " Amazon SDI Direct send XML out' was completed with Error(s)"
            End If
        End If

        Dim strEmailBody As String = ""
        strEmailBody = "<table><tr><td>'Amazon SDI Direct send XML out' process has completed with errors - check application Logs in the path-'" & path & "' .</td></tr></table>"
        strEmailBody = strEmailBody & "<br> ErrorMessage: " & errorMessage & "<br>"

        'Send email and handle any error that occurs

        Dim bSend As Boolean = False
        Try

            SendLogger(strEmailSubject, strEmailBody, "AMAZONORDRXMLOUT", "Mail", strEmailTo, strEmailCc, strEmailBcc)
            bSend = True
        Catch ex As Exception
            bSend = False
        End Try

        If Not bSend Then
            objStreamWriter.WriteLine(rtn & " :: Error - the email was not sent.")
        End If
    End Sub

    'INC0028226 - Triggering error email when utility doesnt run
    Public Function getDBName() As Boolean
        Dim isPRODDB As Boolean = False
        ' Dim PRODDbList As String = ConfigurationManager.AppSettings("OraPRODDbList").ToString()
        Dim DbUrl As String = m_xmlConfig("configuration")("sourceDB").Attributes("cnString").InnerText.Trim
        Try
            DbUrl = DbUrl.Substring(DbUrl.Length - 5).ToUpper()
            '  isPRODDB = (PRODDbList.IndexOf(DbUrl.Trim.ToUpper) > -1)
            If DbUrl = "PROD" Or DbUrl = "FSPRD" Then
                isPRODDB = True
            Else
                isPRODDB = False
            End If

        Catch ex As Exception
            isPRODDB = False
        End Try
        Return isPRODDB
    End Function


End Module
