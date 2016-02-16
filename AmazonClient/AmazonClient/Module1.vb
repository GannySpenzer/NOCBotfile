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
    Dim connectOR As New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=STAR")

    Sub Main()

        Console.WriteLine("Started to check Amazon ready to Dispatch orders ")
        Console.WriteLine("")

        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\AmazonLOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\AmazonLOGS")
        End If

        objStrmWrtrXMLRspns = File.CreateText(filePathResponse)
        objStreamWriterXML = File.CreateText(filePath)
        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Started to check Amazon ready to Dispatch orders " & Now())
        Dim strInput As String = ""  '  <?xml version=""1.0"" encoding=""UTF-8""?><!DOCTYPE cXML SYSTEM ""http://xml.cxml.org/schemas/cXML/1.2.013/cXML.dtd""[]><cXML payloadID=""3/30/2015 11:56:16 AM 019768490@sdi.com"" xml:lang=""en-US"" timestamp=""3/30/2015 11:56:16 AM""><Header><From><Credential domain=""NetworkId""><Identity>SDIINC</Identity></Credential></From><To><Credential domain=""NetworkId""><Identity>Amazon</Identity></Credential></To><Sender><Credential domain=""DUNS""><Identity>SDIINC</Identity><SharedSecret>Y2XN7SefSxpPAoD5i6OtYix4w5TK402d</SharedSecret></Credential><UserAgent>Ariba Network 1.2</UserAgent></Sender></Header><Request><PunchOutSetupRequest operation=""create""><BuyerCookie>3xx1vu5dn5sttwrc2zqspprl</BuyerCookie><Extrinsic name=""UniqueName"">ROVENSKY,VITALY</Extrinsic><Extrinsic name=""UserEmail"">vitaly.rovensky@sdi.com</Extrinsic><Extrinsic name=""CostCenter"">I0256</Extrinsic><BrowserFormPost><URL>http://localhost/InsiteOnline/shopredirect.aspx?PUNOUT=YES</URL></BrowserFormPost><ShipTo><Address addressID=""L0256-01""><Name xml:lang=""en-US"">UNCC Facility Maint. Shop</Name><PostalAddress><DeliverTo>SDI c/o UNCC Facility Maint Shop</DeliverTo><Street>9201 University City Blvd.</Street><City>Charlotte</City><State>NC</State><PostalCode>28223</PostalCode><Country isoCountryCode=""US"">United States</Country></PostalAddress></Address></ShipTo></PunchOutSetupRequest></Request></cXML>"
        Dim strOutput As String = ""

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
        Dim Response_Doc As String
        Dim msgEx As String = ""
        Dim strMsgVendConfig As String = ""
        Dim doc As New XmlDocument

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
            strInput = getOrderRequest(strPunSite, OrderListDataSet)

            objStreamWriter.WriteLine("Finished building XML out " & Now())
            If Trim(strInput) = "" Then
                If Not m_vendorConfig Is Nothing Then
                    If Not m_vendorConfig.ConfigFile Is Nothing Then
                        Try
                            strMsgVendConfig = m_vendorConfig.ConfigFile

                        Catch ex2 As Exception
                            strMsgVendConfig = " 'm_vendorConfig.ConfigFile' is not defined. SubError: " & vbCrLf & ex2.Message & vbCrLf
                        End Try
                    End If
                End If
            Else
                doc.InnerXml = strInput
            End If
        Catch ex As Exception
            msgEx = "Not a valid identity or vendor URL for this catalog.<BR>Please report error" & _
                                  vbCrLf & "config =" & strMsgVendConfig & _
                                  vbCrLf & "ERROR:: " & vbCrLf & ex.ToString & _
                                  ""

            objStreamWriter.WriteLine(msgEx)

            Exit Sub
        End Try
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
        ' check strOutput - how? need structure of response
        ' here - for now - I'm using just sample response file (like generated in UNCC XMLProcess/XmlIn.aspx)
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
                        objStreamWriter.WriteLine("Response XML file checked OK. Changing Order statuses " & Now())
                        Dim strValueToWrite As String = Now.Year.ToString() & Now.Month.ToString() & Now.Day.ToString() & Now.GetHashCode.ToString()
                        Dim intNumberToWrite As Long = 0
                        If IsNumeric(strValueToWrite) Then
                            intNumberToWrite = CType(strValueToWrite, Long)
                        Else
                            intNumberToWrite = Now.GetHashCode
                        End If
                        Dim bNoErrors As Boolean = True
                        Dim strOrderNo As String = ""
                        connectOR.Open()
                        Try
                            Dim iOrdCount As Integer = OrderListDataSet.Tables(0).Rows.Count
                            ' run this query for every order sent:
                            ' "update SYSADM.PS_PO_DISPATCHED set ECQUEUEINSTANCE=" & intNumberToWrite & " where po_id='" & "M010373791" & "'"
                            If connectOR.State = ConnectionState.Open Then
                            Else
                                connectOR.Open()
                            End If
                            Dim rowsAffected As Integer = 0
                            Dim iCnt As Integer = 0
                            For iCnt = 0 To iOrdCount - 1
                                If iCnt = 1 Then Exit For ' for testing ONLY!
                                rowsAffected = 0
                                strOrderNo = OrderListDataSet.Tables(0).Rows(iCnt).Item("po_id").ToString()
                                'run query
                                Dim strUpdateQuery As String = "update SYSADM.PS_PO_DISPATCHED set ECQUEUEINSTANCE=" & intNumberToWrite & " where po_id='" & strOrderNo & "'"
                                'commented out for testing
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
                            Next
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


    Private Function getOrderRequest(ByVal strPunSite As String, ByRef OrderListDataSet As System.Data.DataSet) As String
        'Dim OrderListDataSet As System.Data.DataSet = New System.Data.DataSet()
        Dim rtn As String = "AmazonClient.Module1.getOrderRequest"
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

                    ' create the doc based on order Request template
                    m_setupReqDoc = punchOutSetupRequestDoc.CreateOrderRequestDoc(m_vendorConfig)

                    ' put header info based on info already collected, and build same XML Header as in Punchout; 
                    ' read view to get Order(s) info, and build XML based on XML order structure supplied
                    cXML = m_setupReqDoc.CreateOrderRequestXML(connectOR, OrderListDataSet)

                End If
            End If

        Catch ex As Exception

        End Try

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

        strBox2 = ""   '  my test URL: "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"   '  
        ' "https://https.amazonsedi.com/073dbe31-c230-403f-990c-6f74eeed1510"  '  

        Dim sHttpResponse As String = ""
        Dim httpSession As New easyHttp

        httpSession.URL = "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"  '"http://localhost/SDIWebProcessors/XmlInSDI.aspx"   '  "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"  '   "https://https.amazonsedi.com/073dbe31-c230-403f-990c-6f74eeed1510"  '    "https://www.amazon.com/eprocurement/punchout"  '    "https://supplydev.hajoca.com/hajomid/eclipse.ecl"
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
