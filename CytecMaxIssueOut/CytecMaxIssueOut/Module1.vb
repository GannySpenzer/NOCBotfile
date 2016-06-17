Imports System
Imports System.Data
Imports System.Web
Imports System.Xml
Imports System.Data.OleDb
Imports System.Web.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter
Imports System.IO

Module Module1

    Private m_logger As appLogger = Nothing
    Private m_timeStamp As String = Now.ToString

    Dim rootDir As String = "C:\CytecMxmIn"
    Dim logpath As String = "C:\CytecMxmIn\LOGS\CytecMaxIssueOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")

    Dim strOverride As String = ""
    Dim bolWarning As Boolean = False

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Private m_onError_emailFrom As String = "TechSupport@sdi.com"
    Private m_onError_emailTo As String = "michael.randall@sdi.com;vitaly.rovensky@sdi.com;"
    Private m_onError_emailSubject As String = "Cytec Maximo XML OUT Error"

    Private m_Url_Cytec_Maximo As String = "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"

    '   "http://archibus.uncc.edu:8080/webtier/receivexml.jsp"

    Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start of the Cytec Maximo Issues build XML out")
        Console.WriteLine("")

        '   (1) connection string / db connection
        Dim cnString As String = ""
        Try
            cnString = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
        End Try
        If (cnString.Length > 0) Then
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
            connectOR = New OleDbConnection(cnString)
        End If

        '   (2) root directory
        Dim rDir As String = ""
        Try
            rDir = My.Settings("rootDir").ToString.Trim
        Catch ex As Exception
        End Try
        If (rDir.Length > 0) Then
            rootDir = rDir
        End If

        '   (3) log path/file
        Dim sLogPath As String = ""
        Try
            sLogPath = My.Settings("logPath").ToString.Trim
        Catch ex As Exception
        End Try
        If (sLogPath.Length > 0) Then
            logpath = sLogPath & "\CytecMaxIssueOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

        '   (6) "on error" send email values
        Dim sOnErrEmail As String = ""
        Try
            sOnErrEmail = My.Settings("onError_emailFrom").ToString.Trim
        Catch ex As Exception
        End Try
        If (sOnErrEmail.Length > 0) Then
            m_onError_emailFrom = sOnErrEmail
        End If
        sOnErrEmail = ""
        Try
            sOnErrEmail = My.Settings("onError_emailTo").ToString.Trim
        Catch ex As Exception
        End Try
        If (sOnErrEmail.Length > 0) Then
            m_onError_emailTo = sOnErrEmail
        End If
        sOnErrEmail = ""
        Try
            sOnErrEmail = My.Settings("onError_emailSubject").ToString.Trim
        Catch ex As Exception
        End Try
        If (sOnErrEmail.Length > 0) Then
            m_onError_emailSubject = sOnErrEmail
        End If
        '   (7)  url
        Dim sUrl As String = ""
        Try
            sUrl = My.Settings("Url_Cytec_Maximo").ToString.Trim
        Catch ex As Exception
        End Try
        If (sUrl.Length > 0) Then
            m_Url_Cytec_Maximo = sUrl
        End If

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' get/parse param
        Dim param As String() = Command.Split(" ")
        Dim arr As New ArrayList
        For Each s As String In param
            If s.Trim.Length > 0 Then
                arr.Add(s.Trim.ToUpper)
            End If
        Next
        If (arr.IndexOf("/LOG=VERBOSE") > -1) Then
            logLevel = TraceLevel.Verbose
        ElseIf (arr.IndexOf("/LOG=INFO") > -1) Or (arr.IndexOf("/LOG=INFORMATION") > -1) Then
            logLevel = TraceLevel.Info
        ElseIf (arr.IndexOf("/LOG=WARNING") > -1) Or (arr.IndexOf("/LOG=WARN") > -1) Then
            logLevel = TraceLevel.Warning
        ElseIf (arr.IndexOf("/LOG=ERROR") > -1) Then
            logLevel = TraceLevel.Error
        ElseIf (arr.IndexOf("/LOG=OFF") > -1) Or (arr.IndexOf("/LOG=FALSE") > -1) Then
            logLevel = TraceLevel.Off
        Else
            ' don't change default
        End If
        arr = Nothing
        param = Nothing

        ' initialize log
        m_logger = New appLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        ' log verbose DB connection string
        m_logger.WriteInformationLog(rtn & " :: Start of Cytec Maximo Issues build XML process.")
        m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

        Dim bolError As Boolean = BuildIssuesOutXML()

        If bolError = True Then
            SendEmail()
        End If

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending Cytec Maximo Issues build XML process.")

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Function BuildIssuesOutXML() As Boolean
        Dim bolError As Boolean = False
        Dim bolLineError As Boolean = False
        Dim ds As New DataSet

        Dim rtn As String = "Module1.BuildIssuesOutXML"

        Dim strSqlString As String = "SELECT * FROM SYSADM8.PS_ISA_MXM_MOV_OUT WHERE PROCESS_FLAG = 'N'"

        Try

            ds = ORDBAccess.GetAdapter(strSqlString, connectOR)

        Catch OleDBExp As OleDbException
            m_logger.WriteErrorLog(rtn & " :: Error reading source table SYSADM8.PS_ISA_MXM_MOV_OUT")
            m_logger.WriteErrorLog("ERROR MSG: " & OleDBExp.Message)
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            Return True
        End Try

        If ds Is Nothing Then
            m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo Issues to process at this time")
            Return False
        Else

            If ds.Tables.Count = 0 Then
                m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo Issues to process at this time")
                Return False
            Else

                If ds.Tables(0).Rows.Count < 1 Then
                    m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo Issues to process at this time")
                    Return False
                End If
            End If
        End If

        Dim I As Integer
        Dim rowsaffected As Integer
        If ds.Tables(0).Rows.Count > 0 Then

            Dim strTemplateXml As String = System.AppDomain.CurrentDomain.BaseDirectory.ToString & "CytecMaxIssuesTemplate.xml"

            For I = 0 To ds.Tables(0).Rows.Count - 1
                Dim intIsaIdent As Long = 0
                Dim sErrMsgFromSendXmlOut As String = ""
                Dim strUpdateSql As String = ""
                Try
                    intIsaIdent = CType(ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER"), Long)
                    ' build and send XML file for every line separately
                    Dim docXML As New XmlDocument
                    Dim stringer As theStringer = Nothing
                    Dim node As XmlNode = Nothing
                    Dim attrib As XmlAttribute = Nothing

                    ' load template for XML and replace header variables
                    stringer = New theStringer(Common.LoadPathFile(strTemplateXml))

                    'Dim dateOffset As New DateTimeOffset(Now, TimeZoneInfo.Local.GetUtcOffset(Now))
                    Dim dateOffset As New DateTimeOffset(Now, TimeZone.CurrentTimeZone.GetUtcOffset(Now))
                    m_timeStamp = dateOffset.ToString("s")
                    stringer.Parameters.Add("{__TIMESTAMP}", m_timeStamp)

                    ' load the template
                    docXML.LoadXml(xml:=stringer.ToString)

                    m_logger.WriteVerboseLog(rtn & " :: Template loaded")

                    ' get existing node Request
                    Dim nodeOrderReq As XmlNode = docXML.SelectSingleNode(xpath:="//Envelope//Body//MXINVISSUEInterface//Content")

                    Dim strItemNum As String = ""
                    Dim strStorLoc As String = ""
                    Dim strDtTimestamp As String = ""
                    Dim strShipDate As String = ""
                    Dim strQtyShipped As String = ""
                    Dim strEmplId As String = ""
                    Dim strTransType As String = ""
                    'Dim strOrgId As String = ""
                    Dim strGLDebitAcctValue As String = ""
                    Dim strSiteId As String = ""
                    Dim strWorkOrdNo As String = ""
                    'Dim strWoNum As String = ""

                    ' Item Number ' int:ITEMNUM (ISA_ITEM)
                    strItemNum = ""
                    Try
                        strItemNum = CStr(ds.Tables(0).Rows(I).Item("ISA_ITEM"))
                    Catch ex As Exception
                        strItemNum = ""
                    End Try
                    ' int:STORELOC (STORAGE_AREA)   '  ISA_BIN_ID) 
                    strStorLoc = ""
                    Try
                        strStorLoc = CStr(ds.Tables(0).Rows(I).Item("STORAGE_AREA"))   '  ISA_BIN_ID"))
                    Catch ex As Exception
                        strStorLoc = ""
                    End Try

                    Dim strXmlFileNameToSave21 As String = "CytecMaxWriteIssuesOut_LineIdent_" & intIsaIdent.ToString()
                    Dim strXMLError As String = ""
                    If Trim(strItemNum) <> "" And Trim(strStorLoc) <> "" Then ' Required!

                        'create XML element/lines 
                        ' build <int:MXINVISSUE
                        Dim nodeOrder As XmlNode = nodeOrderReq.AppendChild(docXML.CreateElement(name:="int:MXINVISSUE"))
                        ' build      <int:MATUSETRANS action="Add"
                        Dim nodeItem As XmlNode = nodeOrder.AppendChild(docXML.CreateElement(name:="int:MATUSETRANS"))
                        ' add attribute: action="Add"
                        attrib = nodeItem.Attributes.Append(docXML.CreateAttribute(name:="action"))
                        attrib.Value = "Add"

                        'build actual elements based on ds rows

                        If Trim(strItemNum) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:ITEMNUM"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strItemNum) ' get from dataset
                        End If

                        If Trim(strStorLoc) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:STORELOC"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strStorLoc) ' get from dataset
                        End If

                        ' int:TRANSDATE 
                        strDtTimestamp = ""
                        Try
                            strDtTimestamp = CStr(ds.Tables(0).Rows(I).Item("DT_TIMESTAMP"))
                            If IsDate(strDtTimestamp) Then
                                strDtTimestamp = CType(strDtTimestamp, DateTime).ToString("s")
                            End If
                        Catch ex As Exception
                            strDtTimestamp = ""
                        End Try
                        If Trim(strDtTimestamp) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:TRANSDATE"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strDtTimestamp) ' get from dataset
                        End If

                        ' int:ACTUALDATE 
                        strShipDate = ""
                        Try
                            strShipDate = CStr(ds.Tables(0).Rows(I).Item("SHIP_DATE"))
                            If IsDate(strShipDate) Then
                                strShipDate = CType(strShipDate, DateTime).ToString("s")
                            End If
                        Catch ex As Exception
                            strShipDate = ""
                        End Try
                        If Trim(strShipDate) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:ACTUALDATE"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strShipDate) ' get from dataset
                        End If

                        ' int:QUANTITY 
                        strQtyShipped = ""
                        Try
                            strQtyShipped = CStr(ds.Tables(0).Rows(I).Item("QTY_SHIPPED"))
                        Catch ex As Exception
                            strQtyShipped = ""
                        End Try
                        If Trim(strQtyShipped) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:QUANTITY"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strQtyShipped) ' get from dataset
                        End If

                        ' int:ENTERBY 
                        strEmplId = ""
                        Try
                            strEmplId = CStr(ds.Tables(0).Rows(I).Item("EMPLID"))
                        Catch ex As Exception
                            strEmplId = ""
                        End Try
                        If Trim(strEmplId) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:ENTERBY"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strEmplId) ' get from dataset
                        End If

                        '' int:MEMO  - NOT MAPPED
                        node = nodeItem.AppendChild(docXML.CreateElement(name:="int:MEMO"))
                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        node.InnerText = "From SDI auto build. Line number (0 based): " & I.ToString() & ". Item Number: " & Trim(strItemNum) '  Trim(strMemo) ' get from dataset

                        ' int:ISSUETYPE 
                        strTransType = ""
                        Try
                            strTransType = CStr(ds.Tables(0).Rows(I).Item("TRANS_TYPE_5"))
                        Catch ex As Exception
                            strTransType = ""
                        End Try
                        If Trim(strTransType) <> "" Then
                            strTransType = Trim(strTransType)
                            If UCase(strTransType) = "ISS" Then
                                strTransType = "ISSUE"
                            End If
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:ISSUETYPE"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strTransType) ' get from dataset
                        End If

                        '<int:GLDEBITACCT changed = 'true'
                        Dim nodeGlDebitAcct As XmlNode = nodeItem.AppendChild(docXML.CreateElement(name:="int:GLDEBITACCT"))
                        ' add attribute: action="Add"
                        attrib = nodeGlDebitAcct.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        '<int:VALUE
                        strGLDebitAcctValue = ""
                        Try
                            strGLDebitAcctValue = CStr(ds.Tables(0).Rows(I).Item("ISA_CUST_CHARGE_CD"))
                        Catch ex As Exception
                            strGLDebitAcctValue = ""
                        End Try
                        If Trim(strGLDebitAcctValue) <> "" Then
                            node = nodeGlDebitAcct.AppendChild(docXML.CreateElement(name:="int:VALUE"))
                            'attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            'attrib.Value = "true"
                            node.InnerText = Trim(strGLDebitAcctValue) ' got from dataset
                        End If


                        ' int:ORGID - NOT MAPPED 
                        node = nodeItem.AppendChild(docXML.CreateElement(name:="int:ORGID"))
                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        node.InnerText = "02"   '  Trim(strOrgId)  
                        'End If 

                        ' int:SITEID 
                        strSiteId = ""
                        Try
                            strSiteId = CStr(ds.Tables(0).Rows(I).Item("PLANT"))
                        Catch ex As Exception
                            strSiteId = ""
                        End Try
                        If Trim(strSiteId) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:SITEID"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strSiteId) ' get from dataset
                        End If

                        ' int:REFWO 
                        strWorkOrdNo = ""
                        Try
                            strWorkOrdNo = CStr(ds.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO"))
                        Catch ex As Exception
                            strWorkOrdNo = ""
                        End Try
                        If Trim(strWorkOrdNo) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:REFWO"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strWorkOrdNo) ' get from dataset
                        End If

                        ' int:LINETYPE
                        node = nodeItem.AppendChild(docXML.CreateElement(name:="int:LINETYPE"))
                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        node.InnerText = "ITEM"

                        ' int:ITEMSETID 
                        node = nodeItem.AppendChild(docXML.CreateElement(name:="int:ITEMSETID"))
                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        node.InnerText = "SET02"

                        ''strWoNum = ""
                        ''Try
                        ''    strWoNum = CStr(ds.Tables(0).Rows(I).Item("?????"))
                        ''Catch ex As Exception
                        ''strWoNum = ""
                        ''End Try
                        ''If Trim(strWoNum) <> "" Then

                        '' int:WONUM - NOT MAPPED
                        'node = nodeItem.AppendChild(docXML.CreateElement(name:="int:WONUM"))
                        'attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        'attrib.Value = "true"
                        'node.InnerText = "10198942"  '  Trim(strWoNum) ' get from dataset

                        ''End If

                        ' int:TOSITEID
                        node = nodeItem.AppendChild(docXML.CreateElement(name:="int:TOSITEID"))
                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        node.InnerText = Trim(strSiteId) ' get from dataset

                        ' int:TRANS_LANGCODE
                        node = nodeItem.AppendChild(docXML.CreateElement(name:="int:TRANS_LANGCODE"))
                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        node.InnerText = "EN"
                        ' FINISHED Building all XML nodes

                        Dim strOuterXml As String = docXML.OuterXml
                        strOuterXml = Replace(strOuterXml, "<Envelope", "<soapenv:Envelope")
                        strOuterXml = Replace(strOuterXml, "<Header />", "<soapenv:Header/>")
                        strOuterXml = Replace(strOuterXml, "<Body>", "<soapenv:Body>")
                        strOuterXml = Replace(strOuterXml, "<MXINVISSUEInterface", "<int:MXINVISSUEInterface")
                        strOuterXml = Replace(strOuterXml, "<Content>", "<int:Content>")
                        strOuterXml = Replace(strOuterXml, "<MXINVISSUE>", "<int:MXINVISSUE>")
                        strOuterXml = Replace(strOuterXml, "<MATUSETRANS", "<int:MATUSETRANS")
                        strOuterXml = Replace(strOuterXml, "<ITEMNUM", "<int:ITEMNUM")
                        strOuterXml = Replace(strOuterXml, "</ITEMNUM>", "</int:ITEMNUM>")
                        strOuterXml = Replace(strOuterXml, "<STORELOC", "<int:STORELOC")
                        strOuterXml = Replace(strOuterXml, "</STORELOC>", "</int:STORELOC>")
                        strOuterXml = Replace(strOuterXml, "<TRANSDATE", "<int:TRANSDATE")
                        strOuterXml = Replace(strOuterXml, "</TRANSDATE>", "</int:TRANSDATE>")

                        strOuterXml = Replace(strOuterXml, "<ACTUALDATE", "<int:ACTUALDATE")
                        strOuterXml = Replace(strOuterXml, "</ACTUALDATE>", "</int:ACTUALDATE>")
                        strOuterXml = Replace(strOuterXml, "<QUANTITY", "<int:QUANTITY")
                        strOuterXml = Replace(strOuterXml, "</QUANTITY>", "</int:QUANTITY>")
                        strOuterXml = Replace(strOuterXml, "<ENTERBY", "<int:ENTERBY")
                        strOuterXml = Replace(strOuterXml, "</ENTERBY>", "</int:ENTERBY>")
                        strOuterXml = Replace(strOuterXml, "<ISSUETYPE", "<int:ISSUETYPE")
                        strOuterXml = Replace(strOuterXml, "</ISSUETYPE>", "</int:ISSUETYPE>")
                        strOuterXml = Replace(strOuterXml, "<ORGID", "<int:ORGID")
                        strOuterXml = Replace(strOuterXml, "</ORGID>", "</int:ORGID>")
                        strOuterXml = Replace(strOuterXml, "<SITEID", "<int:SITEID")
                        strOuterXml = Replace(strOuterXml, "</SITEID>", "</int:SITEID>")
                        strOuterXml = Replace(strOuterXml, "<REFWO", "<int:REFWO")
                        strOuterXml = Replace(strOuterXml, "</REFWO>", "</int:REFWO>")
                        strOuterXml = Replace(strOuterXml, "<LINETYPE", "<int:LINETYPE")
                        strOuterXml = Replace(strOuterXml, "</LINETYPE>", "</int:LINETYPE>")
                        strOuterXml = Replace(strOuterXml, "<ITEMSETID", "<int:ITEMSETID")
                        strOuterXml = Replace(strOuterXml, "</ITEMSETID>", "</int:ITEMSETID>")
                        strOuterXml = Replace(strOuterXml, "<GLDEBITACCT", "<int:GLDEBITACCT")
                        strOuterXml = Replace(strOuterXml, "</GLDEBITACCT>", "</int:GLDEBITACCT>")
                        strOuterXml = Replace(strOuterXml, "<VALUE", "<int:VALUE")
                        strOuterXml = Replace(strOuterXml, "</VALUE>", "</int:VALUE>")
                        'strOuterXml = Replace(strOuterXml, "<WONUM", "<int:WONUM")
                        'strOuterXml = Replace(strOuterXml, "</WONUM>", "</int:WONUM>")
                        strOuterXml = Replace(strOuterXml, "<TOSITEID", "<int:TOSITEID")
                        strOuterXml = Replace(strOuterXml, "</TOSITEID>", "</int:TOSITEID>")
                        strOuterXml = Replace(strOuterXml, "<TRANS_LANGCODE", "<int:TRANS_LANGCODE")
                        strOuterXml = Replace(strOuterXml, "</TRANS_LANGCODE>", "</int:TRANS_LANGCODE>")
                        strOuterXml = Replace(strOuterXml, "<MEMO", "<int:MEMO")
                        strOuterXml = Replace(strOuterXml, "</MEMO>", "</int:MEMO>")
                        strOuterXml = Replace(strOuterXml, "</MATUSETRANS>", "</int:MATUSETRANS>")
                        strOuterXml = Replace(strOuterXml, "</MXINVISSUE>", "</int:MXINVISSUE>")
                        strOuterXml = Replace(strOuterXml, "</Content>", "</int:Content>")
                        strOuterXml = Replace(strOuterXml, "</MXINVISSUEInterface>", "</int:MXINVISSUEInterface>")
                        strOuterXml = Replace(strOuterXml, "</Body>", "</soapenv:Body>")
                        strOuterXml = Replace(strOuterXml, "</Envelope>", "</soapenv:Envelope>")
                        'strOuterXml = Replace(strOuterXml, "Envelope", "soapenvnvelope")
                        'strOuterXml = Replace(strOuterXml, "Envelope", "soapenvnvelope")

                        m_logger.WriteVerboseLog(rtn & " :: Finished creating Issues XML Out string for Line Ident.: " & intIsaIdent.ToString())

                        Dim strXmlFileNameStartWith As String = "CytecMaxWriteIssuesOut_LineIdent_" & intIsaIdent.ToString() & "_"

                        Dim objStreamWriterXML As System.IO.StreamWriter
                        Dim sFileName As String = strXmlFileNameStartWith & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & Now.GetHashCode & ".xml"

                        strXmlFileNameToSave21 = rootDir & "\XMLOUT\" & sFileName  '   strXmlFileNameStartWith & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & Now.GetHashCode & ".xml"
                        objStreamWriterXML = New System.IO.StreamWriter(strXmlFileNameToSave21)  '  rootDir & "\XMLOUT\CytecMaxWriteIssuesOut" & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & Now.GetHashCode & ".xml")
                        objStreamWriterXML.WriteLine(strOuterXml)
                        objStreamWriterXML.Flush()
                        objStreamWriterXML.Close()
                        m_logger.WriteVerboseLog(rtn & " :: Saved XML Out file for Line Indent.: " & intIsaIdent.ToString())
                        bolLineError = False

                        'send XML file to Cytec
                        bolLineError = SendXMLOut(strXmlFileNameToSave21, sFileName, sErrMsgFromSendXmlOut)
                        Dim strErr254 As String = ""
                        If Trim(sErrMsgFromSendXmlOut) <> "" Then
                            sErrMsgFromSendXmlOut = Trim(sErrMsgFromSendXmlOut)
                            If Len(sErrMsgFromSendXmlOut) > 254 Then
                                strErr254 = Microsoft.VisualBasic.Left(sErrMsgFromSendXmlOut, 254)
                            Else
                                strErr254 = sErrMsgFromSendXmlOut
                            End If
                            If Len(sErrMsgFromSendXmlOut) > 80 Then
                                sErrMsgFromSendXmlOut = Microsoft.VisualBasic.Left(sErrMsgFromSendXmlOut, 80)
                            End If
                        Else
                            sErrMsgFromSendXmlOut = "Empty Error Message returned from 'SendXMLOut'"
                            strErr254 = sErrMsgFromSendXmlOut
                        End If

                        If Trim(sErrMsgFromSendXmlOut) <> "" Then
                            m_logger.WriteVerboseLog(sErrMsgFromSendXmlOut)
                        Else
                            m_logger.WriteVerboseLog("No Error Message returned from 'SendXMLOut'")
                        End If

                        If Not bolLineError Then
                            'update field to 'Y'
                            strUpdateSql = "UPDATE SYSADM8.PS_ISA_MXM_MOV_OUT SET PROCESS_FLAG = 'Y' WHERE ISA_IDENTIFIER = " & ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER") & ""
                        Else

                            'set flag to E; write error message
                            strUpdateSql = "UPDATE SYSADM8.PS_ISA_MXM_MOV_OUT SET PROCESS_FLAG = 'E', ISA_ERROR_MSG_80 = '" & sErrMsgFromSendXmlOut & "' WHERE ISA_IDENTIFIER = " & ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER") & ""
                        End If
                        
                        rowsaffected = 0
                        rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)

                        If rowsaffected = 0 Then
                            Dim strRowsAffctZero As String = rtn & " :: Update field PROCESS_FLAG to 'Y' returned 'rowsaffected = 0'. strUpdateSql : " & strUpdateSql
                            m_logger.WriteVerboseLog(strRowsAffctZero)  '  rtn & " :: Update field PROCESS_FLAG to 'Y' returned 'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                            bolLineError = True
                            
                        End If

                        If bolLineError Then
                            strUpdateSql = "INSERT INTO SYSADM.PS_NLNK2_ERR_MSG (CUST_ID,ISA_IDENTIFIER,ERROR_LEVEL," & vbCrLf & _
                            "MESSAGE_ID,MESSAGE_NBR,MESSAGE_TEXT_254,DATETIME_ADDED) " & vbCrLf & _
                            " VALUES('CYTEC'," & intIsaIdent & ",'E'," & vbCrLf & _
                            "' ',0,'" & strErr254 & "',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))" & vbCrLf & _
                            ""

                            rowsaffected = 0
                            rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                            If rowsaffected = 0 Then
                                m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                            End If
                        End If
                        
                    Else
                        'save line error and description in error files; set flag to E; write error message
                        Dim sNoInfoError As String = rtn & " :: Trim(strItemNum) = '' (" & strItemNum & ") Or Trim(strStorLoc) = '' (" & strStorLoc & ") for line Ident.: " & intIsaIdent.ToString()
                        m_logger.WriteVerboseLog(sNoInfoError)  '  rtn & " :: Trim(strItemNum) = '' (" & strItemNum & ") Or Trim(strStorLoc) = '' (" & strStorLoc & ") for line Ident.: " & intIsaIdent.ToString())
                        bolLineError = True
                       
                        strUpdateSql = "INSERT INTO SYSADM.PS_NLNK2_ERR_MSG (CUST_ID,ISA_IDENTIFIER,ERROR_LEVEL," & vbCrLf & _
                            "MESSAGE_ID,MESSAGE_NBR,MESSAGE_TEXT_254,DATETIME_ADDED) " & vbCrLf & _
                            " VALUES('CYTEC'," & intIsaIdent & ",'E'," & vbCrLf & _
                            "' ',0,'" & sNoInfoError & "',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))" & vbCrLf & _
                            ""

                        rowsaffected = 0
                        rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                        If rowsaffected = 0 Then
                            m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                        End If

                        strUpdateSql = "UPDATE SYSADM8.PS_ISA_MXM_MOV_OUT SET PROCESS_FLAG = 'E', ISA_ERROR_MSG_80 = '" & sNoInfoError & "' WHERE ISA_IDENTIFIER = " & ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER") & ""

                        rowsaffected = 0
                        rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                        If rowsaffected = 0 Then
                            m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                        End If
                    End If  '  Trim(strItemNum) <> "" And Trim(strStorLoc) <> ""

                Catch ex As Exception
                    bolLineError = True
                    m_logger.WriteErrorLog(rtn & " :: Error while Building/Sending Issues Out XML for Line Ident.: " & intIsaIdent.ToString())
                    m_logger.WriteErrorLog(ex.Message)
                    'save line error and description in error files; set flag to E in table; write error message
                    Dim strGeneralError As String = ex.Message
                    strUpdateSql = ""
                    If Len(strGeneralError) > 254 Then
                        strGeneralError = Microsoft.VisualBasic.Left(strGeneralError, 254)
                    End If
                    strUpdateSql = "INSERT INTO SYSADM.PS_NLNK2_ERR_MSG (CUST_ID,ISA_IDENTIFIER,ERROR_LEVEL," & vbCrLf & _
                        "MESSAGE_ID,MESSAGE_NBR,MESSAGE_TEXT_254,DATETIME_ADDED) " & vbCrLf & _
                        " VALUES('CYTEC'," & intIsaIdent & ",'E'," & vbCrLf & _
                        "' ',0,'" & strGeneralError & "',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))" & vbCrLf & _
                        ""

                    rowsaffected = 0
                    rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                    If rowsaffected = 0 Then
                        m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                    End If

                    If Len(strGeneralError) > 80 Then
                        strGeneralError = Microsoft.VisualBasic.Left(strGeneralError, 80)
                    End If

                    strUpdateSql = "UPDATE SYSADM8.PS_ISA_MXM_MOV_OUT SET PROCESS_FLAG = 'E', ISA_ERROR_MSG_80 = '" & strGeneralError & "' WHERE ISA_IDENTIFIER = " & intIsaIdent.ToString() & ""

                    rowsaffected = 0
                    rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                    If rowsaffected = 0 Then
                        m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                    End If
                End Try

                If bolLineError Then
                    bolError = True
                End If
            Next ' For I = 0 To ds.Tables(0).Rows.Count - 1
            

        End If

        ' check are error files are empty - to determine bolError value

        Return bolError

    End Function

    Private Function SendXMLOut(ByVal strXmlFileNameToSave21 As String, ByVal sFileName As String, _
                ByRef sErrMsgFromSendXmlOut As String) As Boolean
        Dim rtn As String = "Cytec Maximo Process XML Out.getXMLOut"
        Console.WriteLine("Start send of Cytec Maximo XML out")
        Console.WriteLine("")

        Dim sr As System.IO.StreamReader = Nothing
        Dim XMLContent As String = ""
        Dim strXMLError As String = ""
        Dim bolError As Boolean = False

        Dim xmlRequest As New XmlDocument

        Dim I As Integer = 0
        Dim strItems As String = ""
        Dim intItems As Integer = 0

        m_logger.WriteVerboseLog(rtn & " :: started sending file " & strXmlFileNameToSave21)

        Try

            sr = New System.IO.StreamReader(strXmlFileNameToSave21)
            XMLContent = sr.ReadToEnd()
            sr.Close()

            Try
                xmlRequest.LoadXml(XMLContent)
            Catch ex As Exception
                Console.WriteLine("")
                Console.WriteLine("***error - " & ex.ToString)
                Console.WriteLine("")
                strXMLError = ex.ToString
                m_logger.WriteErrorLog(rtn & " :: load out Error " & strXMLError & " in file " & sFileName)

            End Try

            Dim Response_Doc As String
            If Trim(strXMLError) = "" Then

                Response_Doc = SendOut(xmlRequest.InnerXml)

                '-----------------------------------------------------------------------
                ' Parse the XML response from Intercall Extranet
                '-----------------------------------------------------------------------

                Dim xmlResponse As New XmlDocument
                Try
                    If Trim(Response_Doc) = "" Then
                        m_logger.WriteVerboseLog("     empty response from file " & sFileName)
                    Else
                        m_logger.WriteVerboseLog(Response_Doc)
                        xmlResponse.LoadXml(Response_Doc)
                        ' error trapping code
                        If Response_Doc.Contains("<soapenv:Fault>") Or Response_Doc.Contains("</faultcode>") Or _
                                    Response_Doc.Contains("</faultstring>") Then
                            bolError = True
                            'get Response_Doc error part
                            Dim strErrorPart As String = ""
                            Dim intStartPos1 As Integer = 0
                            If Response_Doc.Contains("<faultstring>") Then
                                intStartPos1 = Response_Doc.IndexOf("<faultstring>") + 14
                                strErrorPart = Mid(Response_Doc, intStartPos1)
                            End If
                            strXMLError = strErrorPart
                        End If
                    End If
                Catch ex As Exception
                    Console.WriteLine("")
                    Console.WriteLine("***error - " & ex.ToString)
                    Console.WriteLine("")
                    strXMLError = ex.ToString
                    m_logger.WriteErrorLog(rtn & " ::Error: " & strXMLError & " in file " & strXmlFileNameToSave21)

                End Try

                '-----------------------------------------------------------------------
                ' Get server status
                '-----------------------------------------------------------------------

                If Trim(strXMLError) = "" Then
                    Try
                        File.Copy(strXmlFileNameToSave21, "C:\CytecMxmIn\XMLOUTProcessed\" & sFileName, True)
                        m_logger.WriteVerboseLog(" End - FILE " & strXmlFileNameToSave21 & " has been moved")
                        File.Delete(strXmlFileNameToSave21)
                    Catch ex As Exception

                        m_logger.WriteErrorLog(rtn & "  copy file Error in file " & sFileName)
                        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

                        strXMLError = ex.ToString
                    End Try
                Else '  Trim(strXMLError) <> ""
                    sErrMsgFromSendXmlOut = strXMLError
                    Try
                        File.Copy(strXmlFileNameToSave21, "C:\CytecMxmIn\BadXML\" & sFileName, True)
                        m_logger.WriteVerboseLog("Error description: " & strXMLError)
                        m_logger.WriteVerboseLog(" Error - FILE: " & strXmlFileNameToSave21 & " - has been moved to BadXML directory")
                        File.Delete(strXmlFileNameToSave21)
                    Catch ex As Exception

                        m_logger.WriteErrorLog(rtn & " ::  move file to BadXML directory Error in file " & sFileName)
                        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

                        strXMLError = ex.ToString
                    End Try
                    bolError = True
                End If '  If Trim(strXMLError) = ""

            Else  '  Trim(strXMLError) <> ""
                sErrMsgFromSendXmlOut = strXMLError
                Try
                    File.Copy(strXmlFileNameToSave21, "C:\CytecMxmIn\BadXML\" & sFileName, True)
                    m_logger.WriteVerboseLog(" Error - FILE: " & strXmlFileNameToSave21 & " - has been moved to BadXML directory")
                    File.Delete(strXmlFileNameToSave21)
                Catch ex As Exception

                    m_logger.WriteErrorLog(rtn & " ::  move file to BadXML directory Error in file " & sFileName)
                    m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

                    strXMLError = ex.ToString
                End Try
                bolError = True
            End If  '  If Trim(strXMLError) = ""

        Catch ex As Exception
            m_logger.WriteErrorLog(rtn & " ::Error " & strXMLError & " in file " & sFileName)
            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)

            strXMLError = ex.ToString

            bolError = True
        End Try

        sErrMsgFromSendXmlOut = strXMLError

        If Not bolError Then
            sErrMsgFromSendXmlOut = ""
        End If

        Return bolError

    End Function

    Private Function SendOut(ByVal strInnerXml As String) As String
        Dim Response_Doc As String = ""
        Dim XMLhttp As Object
        Dim xmlDoc2 As Object

        XMLhttp = CreateObject("MSXML2.ServerXMLHTTP")
        XMLhttp.Open("POST", m_Url_Cytec_Maximo, False)
        XMLhttp.setRequestHeader("Content-Type", "text/xml; charset=UTF-8")
        XMLhttp.setRequestHeader("SOAPAction", "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx")
        XMLhttp.setTimeouts(10000, 120000, 60000, 60000)
        XMLhttp.Send(strInnerXml)

        '-----------------------------------------------------------------------
        ' Get XML response from Extranet
        '-----------------------------------------------------------------------
        xmlDoc2 = CreateObject("MSXML2.DOMDocument")
        xmlDoc2.setProperty("ServerHTTPRequest", True)
        xmlDoc2.async = False
        xmlDoc2.LoadXML(XMLhttp.ResponseXML.xml)

        Response_Doc = XMLhttp.responseXML.xml

        Return Response_Doc

    End Function

    Private Sub SendEmail()

        Dim rtn As String = "Module1.SendEmail"

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "vitaly.rovensky@sdi.com"
        email.Cc = " "
        email.Bcc = "webdev@sdi.com"

        'The subject of the email
        email.Subject = "Cytec Maximo Issues XML Out Error(s)"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>'Cytec Maximo Issues XML Out' process has completed with errors, review tables SYSADM8.PS_ISA_MXM_MOV_OUT and SYSADM.PS_NLNK2_ERR_MSG for Errors description.</td></tr></table>"

        'Send the email and handle any error that occurs
        Dim bSend As Boolean = False
        Try
            SendEmail1(email)
            bSend = True
            m_arrXMLErrFiles = ""
            m_arrErrorsList = ""
        Catch ex As Exception
            bSend = False
        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent.")
        End If
    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            SendLogger(mailer.Subject, mailer.Body, "CYTECMXMISSUESXMLOUT", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

        Catch ex As Exception

        End Try
    End Sub

    Public Sub SendLogger(ByVal subject As String, ByVal body As String, ByVal messageType As String, ByVal MailType As String, ByVal EmailTo As String, ByVal EmailCc As String, ByVal EmailBcc As String)
        Try
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String()
            Dim MailAttachmentbytes As New List(Of Byte())()
            Dim objException As String
            Dim objExceptionTrace As String

            'EmailTo = "vitaly.rovensky@sdi.com"
            SDIEmailService.EmailUtilityServices(MailType, "SDIExchADMIN@sdi.com", EmailTo, subject, EmailCc, EmailBcc, body, messageType, MailAttachmentName, MailAttachmentbytes.ToArray())

        Catch ex As Exception

        End Try
    End Sub

End Module
