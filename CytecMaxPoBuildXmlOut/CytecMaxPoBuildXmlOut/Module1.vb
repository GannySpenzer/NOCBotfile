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
    Private myLoggr1 As appLogger = Nothing
    Private m_timeStamp As String = Now.ToString

    Dim rootDir As String = "C:\CytecMxmIn"
    Dim logpath As String = "C:\CytecMxmIn\LOGS\UpdPoCytecXmlOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\CytecMxmIn\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=DEVL")

    Private m_Url_Cytec_Maximo As String = "http://websrv.sdi.com/sdiwebin/xmlinsdi.aspx"

    Dim strOverride As String = ""
    Dim bolWarning As Boolean = False

    Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start of the Cytec Maximo PO build XML out")
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
            logpath = sLogPath & "\CytecMaxIOH_XmlBuildOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
        End If

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
        m_logger.WriteInformationLog(rtn & " :: Start of Cytec Maximo PO build XML process.")
        m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

        Dim bolError As Boolean = BuildPoOutXML()

        If bolError = True Then
            SendEmail()
        End If

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending Cytec Maximo IOH build XML process.")

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try

    End Sub

    Private Function BuildPoOutXML() As Boolean
        Dim bolError As Boolean = False
        Dim bolLineError As Boolean = False
        Dim ds As New DataSet

        Dim rtn As String = "Module1.BuildPoOutXML"

        Dim strSqlString As String = "SELECT * FROM SYSADM8.PS_ISA_MXM_PO_OUT WHERE PROCESS_FLAG = 'N'"

        Try

            ds = ORDBAccess.GetAdapter(strSqlString, connectOR)

        Catch OleDBExp As OleDbException
            m_logger.WriteErrorLog(rtn & " :: Error reading source table SYSADM8.PS_ISA_MXM_PO_OUT")
            m_logger.WriteErrorLog("ERROR MSG: " & OleDBExp.Message)
            Try
                connectOR.Close()
            Catch ex As Exception

            End Try
            Return True
        End Try

        If ds Is Nothing Then
            m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo POs to process at this time")
            Return False
        Else

            If ds.Tables.Count = 0 Then
                m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo POs to process at this time")
                Return False
            Else

                If ds.Tables(0).Rows.Count < 1 Then
                    m_logger.WriteWarningLog(rtn & " :: Warning - no Cytec Maximo POs to process at this time")
                    Return False
                End If
            End If
        End If  '  ds Is Nothing - no Cytec Maximo POs to process

        Dim I As Integer = 0
        Dim rowsaffected As Integer = 0
        If ds.Tables(0).Rows.Count > 0 Then

            Dim strTemplateXml As String = System.AppDomain.CurrentDomain.BaseDirectory.ToString & "CytecMaxPoTemplate.xml"

            For I = 0 To ds.Tables(0).Rows.Count - 1
                Dim intIsaIdent As Long = 0
                Dim sErrMsgFromSendXmlOut As String = ""
                Dim strUpdateSql As String = ""
                'build SEPARATE XML file for each line in data set

                Dim strPoNumber As String = ""
                Dim strPoLineNum As String = ""

                Try
                    strPoNumber = CStr(ds.Tables(0).Rows(I).Item("PO_ID"))
                Catch ex As Exception
                    strPoNumber = ""
                End Try

                Try
                    strPoLineNum = CStr(ds.Tables(0).Rows(I).Item("LINE_NBR"))
                Catch ex As Exception
                    strPoLineNum = ""
                End Try
                Dim strXmlFileNameToSave21 As String = "CytecMaxPoOut_LineNo_" & I.ToString()
                Dim strXMLError As String = ""
                If Trim(strPoNumber) <> "" And Trim(strPoLineNum) <> "" Then
                    strPoNumber = Trim(strPoNumber)
                    strPoLineNum = Trim(strPoLineNum)
                    'start preparing XML file
                    Try
                        intIsaIdent = CType(ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER"), Long)
                        bolLineError = False
                        Dim docXML As New XmlDocument
                        Dim stringer As theStringer = Nothing
                        Dim node As XmlNode = Nothing
                        Dim attrib As XmlAttribute = Nothing

                        ' load template for XML and replace header variables
                        stringer = New theStringer(Common.LoadPathFile(strTemplateXml))

                        Dim dateOffset As New DateTimeOffset(Now, TimeZone.CurrentTimeZone.GetUtcOffset(Now))
                        m_timeStamp = dateOffset.ToString("s")
                        stringer.Parameters.Add("{__TIMESTAMP}", m_timeStamp)

                        ' load the template
                        docXML.LoadXml(xml:=stringer.ToString)

                        m_logger.WriteVerboseLog(rtn & " :: Template loaded")

                        ' get existing node Content
                        Dim nodeOrderReq As XmlNode = docXML.SelectSingleNode(xpath:="//Envelope//Body//MXPOInterface//Content")

                        Dim strOrgId As String = "02"
                        Dim strSiteId As String = "PC"
                        Dim strTransLangCode As String = "EN"
                        Dim strLineType As String = "MATERIAL"
                        Dim strItemSetId As String = ""
                        Dim strReqIdNum As String = ""
                        Dim strReqIdLineNum As String = ""
                        Dim strReqDelivDate As String = ""
                        Dim strInvItemNum As String = ""
                        Dim strLineDescr As String = ""
                        Dim strOrderQty As String = ""
                        Dim strPricePo As String = ""
                        Dim strGLDebitAcctValue As String = ""
                        Dim strRefWorkOrder As String = ""
                        Dim strRequestedBy As String = ""
                        Dim strEnteredBy As String = ""
                        Dim strEnteredDate As String = ""
                        Dim strOrderUom As String = ""

                        ' build <int:MXPO
                        Dim nodeOrder As XmlNode = nodeOrderReq.AppendChild(docXML.CreateElement(name:="int:MXPO"))
                        ' build      <int:PO action="Add"
                        Dim nodeItem As XmlNode = nodeOrder.AppendChild(docXML.CreateElement(name:="int:PO"))
                        ' add attribute: action="Add"
                        attrib = nodeItem.Attributes.Append(docXML.CreateAttribute(name:="action"))
                        attrib.Value = "Add"  '   "AddChange"

                        '<int:SITEID  ' strSiteId
                        strSiteId = ""
                        Try
                            strSiteId = CStr(ds.Tables(0).Rows(I).Item("PLANT"))
                        Catch ex As Exception
                            strSiteId = ""
                        End Try
                        If Trim(strSiteId) = "" Then
                            strSiteId = "PC"
                        End If
                        If Trim(strSiteId) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:SITEID"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strSiteId) ' got from dataset / predefined
                        End If

                        '<int:ORGID ' strOrgId
                        If Trim(strOrgId) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:ORGID"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strOrgId) ' predefined
                        End If

                        '<int:TRANS_LANGCODE  ' strTransLangCode
                        If Trim(strTransLangCode) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:TRANS_LANGCODE"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strTransLangCode) ' predefined
                        End If

                        'build actual elements based on ds rows

                        '<int:PONUM
                        If Trim(strPoNumber) <> "" Then
                            node = nodeItem.AppendChild(docXML.CreateElement(name:="int:PONUM"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strPoNumber) ' got from dataset
                        End If

                        'line nodes
                        ' build      <int:POLINE action="Add"
                        Dim nodePoLine As XmlNode = nodeItem.AppendChild(docXML.CreateElement(name:="int:POLINE"))
                        ' add attribute: action="Add"
                        attrib = nodePoLine.Attributes.Append(docXML.CreateAttribute(name:="action"))
                        attrib.Value = "Add"  '   "AddChange"

                        '<int:POLINENUM
                        If Trim(strPoLineNum) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:POLINENUM"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strPoLineNum) ' got from dataset
                        End If

                        '<int:PRNUM
                        strReqIdNum = ""
                        Try
                            strReqIdNum = CStr(ds.Tables(0).Rows(I).Item("REQ_ID"))
                        Catch ex As Exception
                            strReqIdNum = ""
                        End Try
                        If Trim(strReqIdNum) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:PRNUM"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strReqIdNum) ' got from dataset
                        End If

                        '<int:PRLINENUM
                        strReqIdLineNum = ""
                        Try
                            strReqIdLineNum = CStr(ds.Tables(0).Rows(I).Item("REQ_LINE_NBR"))
                        Catch ex As Exception
                            strReqIdLineNum = ""
                        End Try
                        If Trim(strReqIdLineNum) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:PRLINENUM"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strReqIdLineNum) ' got from dataset
                        End If

                        '<int:REQDELIVERYDATE
                        strReqDelivDate = ""
                        Try
                            strReqDelivDate = CStr(ds.Tables(0).Rows(I).Item("DUE_DATE"))
                            If IsDate(strReqDelivDate) Then
                                strReqDelivDate = CType(strReqDelivDate, DateTime).ToString("s")
                            End If
                        Catch ex As Exception
                            strReqDelivDate = ""
                        End Try
                        If Trim(strReqDelivDate) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:REQDELIVERYDATE"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strReqDelivDate) ' got from dataset
                        End If

                        '<int:ITEMNUM
                        strInvItemNum = ""
                        Try
                            strInvItemNum = CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID"))
                        Catch ex As Exception
                            strInvItemNum = ""
                        End Try
                        If Trim(strInvItemNum) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:ITEMNUM"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strInvItemNum) ' got from dataset
                        End If

                        '<int:DESCRIPTION
                        strLineDescr = ""
                        Try
                            strLineDescr = CStr(ds.Tables(0).Rows(I).Item("DESCR"))
                        Catch ex As Exception
                            strLineDescr = ""
                        End Try
                        If Trim(strLineDescr) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:DESCRIPTION"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strLineDescr) ' got from dataset
                        End If

                        '<int:ORDERQTY
                        strOrderQty = ""
                        Try
                            strOrderQty = CStr(ds.Tables(0).Rows(I).Item("QTY_PO"))
                        Catch ex As Exception
                            strOrderQty = ""
                        End Try
                        If Trim(strOrderQty) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:ORDERQTY"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strOrderQty) ' got from dataset
                        End If

                        '<int:LINETYPE - predefined
                        node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:LINETYPE"))
                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        node.InnerText = Trim(strLineType) ' predefined

                        '<int:ITEMSETID - predefined
                        node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:ITEMSETID"))
                        'attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        'attrib.Value = "true"
                        node.InnerText = Trim(strItemSetId) ' predefined

                        '<int:ISSUE - predefined
                        node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:ISSUE"))
                        attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                        attrib.Value = "true"
                        node.InnerText = "1" ' predefined

                        '<int:TOSITEID - predefined
                        If Trim(strSiteId) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:TOSITEID"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strSiteId) ' predefined
                        End If

                        '<int:LINECOST
                        strPricePo = ""
                        Try
                            strPricePo = CStr(ds.Tables(0).Rows(I).Item("PRICE_PO"))
                        Catch ex As Exception
                            strPricePo = ""
                        End Try
                        If Trim(strPricePo) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:LINECOST"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strPricePo) ' got from dataset
                        End If

                        '<int:GLDEBITACCT changed = 'true'
                        Dim nodeGlDebitAcct As XmlNode = nodePoLine.AppendChild(docXML.CreateElement(name:="int:GLDEBITACCT"))
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

                        '<int:REFWO
                        strRefWorkOrder = ""
                        Try
                            strRefWorkOrder = CStr(ds.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO"))
                        Catch ex As Exception
                            strRefWorkOrder = ""
                        End Try
                        If Trim(strRefWorkOrder) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:REFWO"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strRefWorkOrder) ' got from dataset
                        End If

                        '<int:REQUESTEDBY
                        strRequestedBy = ""
                        Try
                            strRequestedBy = CStr(ds.Tables(0).Rows(I).Item("EMPLID"))
                        Catch ex As Exception
                            strRequestedBy = ""
                        End Try
                        If Trim(strRequestedBy) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:REQUESTEDBY"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strRequestedBy) ' got from dataset
                        End If

                        '<int:ENTERBY
                        strEnteredBy = ""
                        Try
                            strEnteredBy = CStr(ds.Tables(0).Rows(I).Item("OPRID_ENTERED_BY"))
                        Catch ex As Exception
                            strEnteredBy = ""
                        End Try
                        If Trim(strEnteredBy) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:ENTERBY"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strEnteredBy) ' got from dataset
                        End If

                        '<int:ENTERDATE
                        strEnteredDate = ""
                        Try
                            strEnteredDate = CStr(ds.Tables(0).Rows(I).Item("ENTERED_DT"))
                            If IsDate(strEnteredDate) Then
                                strEnteredDate = CType(strEnteredDate, DateTime).ToString("s")
                            End If
                        Catch ex As Exception
                            strEnteredDate = ""
                        End Try
                        If Trim(strEnteredDate) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:ENTERDATE"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strEnteredDate) ' got from dataset
                        End If

                        '<int:LOADEDCOST  '  same as LINECOST
                        If Trim(strPricePo) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:LOADEDCOST"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strPricePo) ' got from dataset
                        End If

                        '<int:ORDERUNIT
                        strOrderUom = ""
                        Try
                            strOrderUom = CStr(ds.Tables(0).Rows(I).Item("UNIT_OF_MEASURE"))
                        Catch ex As Exception
                            strOrderUom = ""
                        End Try
                        If Trim(strOrderUom) <> "" Then
                            node = nodePoLine.AppendChild(docXML.CreateElement(name:="int:ORDERUNIT"))
                            attrib = node.Attributes.Append(docXML.CreateAttribute(name:="changed"))
                            attrib.Value = "true"
                            node.InnerText = Trim(strOrderUom) ' got from dataset
                        End If

                        'finished building all the nodes

                        ' build/change XML out file
                        Dim strOuterXml As String = docXML.OuterXml
                        strOuterXml = Replace(strOuterXml, "<Envelope", "<soapenv:Envelope")
                        strOuterXml = Replace(strOuterXml, "<Body>", "<soapenv:Body>")
                        strOuterXml = Replace(strOuterXml, "<MXPOInterface", "<int:MXPOInterface")
                        strOuterXml = Replace(strOuterXml, "<Content>", "<int:Content>")
                        strOuterXml = Replace(strOuterXml, "<MXPO>", "<int:MXPO>")
                        strOuterXml = Replace(strOuterXml, "<PO", "<int:PO")
                        strOuterXml = Replace(strOuterXml, "<PRNUM", "<int:PRNUM")
                        strOuterXml = Replace(strOuterXml, "</PRNUM>", "</int:PRNUM>")
                        strOuterXml = Replace(strOuterXml, "<POLINENUM", "<int:POLINENUM")
                        strOuterXml = Replace(strOuterXml, "</POLINENUM>", "</int:POLINENUM>")
                        strOuterXml = Replace(strOuterXml, "<POLINE", "<int:POLINE")
                        strOuterXml = Replace(strOuterXml, "</POLINE>", "</int:POLINE>")
                        strOuterXml = Replace(strOuterXml, "<PONUM", "<int:PONUM")
                        strOuterXml = Replace(strOuterXml, "</PONUM>", "</int:PONUM>")
                        strOuterXml = Replace(strOuterXml, "<ISSUE", "<int:ISSUE")
                        strOuterXml = Replace(strOuterXml, "</ISSUE>", "</int:ISSUE>")
                        strOuterXml = Replace(strOuterXml, "<TOSITEID", "<int:TOSITEID")
                        strOuterXml = Replace(strOuterXml, "</TOSITEID>", "</int:TOSITEID>")
                        strOuterXml = Replace(strOuterXml, "<TRANS_LANGCODE", "<int:TRANS_LANGCODE")
                        strOuterXml = Replace(strOuterXml, "</TRANS_LANGCODE>", "</int:TRANS_LANGCODE>")
                        strOuterXml = Replace(strOuterXml, "<ORGID", "<int:ORGID")
                        strOuterXml = Replace(strOuterXml, "</ORGID>", "</int:ORGID>")
                        strOuterXml = Replace(strOuterXml, "<SITEID", "<int:SITEID")
                        strOuterXml = Replace(strOuterXml, "</SITEID>", "</int:SITEID>")
                        strOuterXml = Replace(strOuterXml, "<LINECOST", "<int:LINECOST")
                        strOuterXml = Replace(strOuterXml, "</LINECOST>", "</int:LINECOST>")
                        strOuterXml = Replace(strOuterXml, "<GLDEBITACCT", "<int:GLDEBITACCT")
                        strOuterXml = Replace(strOuterXml, "</GLDEBITACCT>", "</int:GLDEBITACCT>")
                        strOuterXml = Replace(strOuterXml, "<VALUE", "<int:VALUE")
                        strOuterXml = Replace(strOuterXml, "</VALUE>", "</int:VALUE>")
                        strOuterXml = Replace(strOuterXml, "<REFWO", "<int:REFWO")
                        strOuterXml = Replace(strOuterXml, "</REFWO>", "</int:REFWO>")
                        strOuterXml = Replace(strOuterXml, "<ORDERQTY", "<int:ORDERQTY")
                        strOuterXml = Replace(strOuterXml, "</ORDERQTY>", "</int:ORDERQTY>")
                        strOuterXml = Replace(strOuterXml, "<DESCRIPTION", "<int:DESCRIPTION")
                        strOuterXml = Replace(strOuterXml, "</DESCRIPTION>", "</int:DESCRIPTION>")
                        strOuterXml = Replace(strOuterXml, "<ITEMNUM", "<int:ITEMNUM")
                        strOuterXml = Replace(strOuterXml, "</ITEMNUM>", "</int:ITEMNUM>")

                        strOuterXml = Replace(strOuterXml, "<LINETYPE", "<int:LINETYPE")
                        strOuterXml = Replace(strOuterXml, "</LINETYPE>", "</int:LINETYPE>")
                        strOuterXml = Replace(strOuterXml, "<ITEMSETID", "<int:ITEMSETID")
                        strOuterXml = Replace(strOuterXml, "</ITEMSETID>", "</int:ITEMSETID>")

                        strOuterXml = Replace(strOuterXml, "<REQDELIVERYDATE", "<int:REQDELIVERYDATE")
                        strOuterXml = Replace(strOuterXml, "</REQDELIVERYDATE>", "</int:REQDELIVERYDATE>")
                        strOuterXml = Replace(strOuterXml, "<PRLINENUM", "<int:PRLINENUM")
                        strOuterXml = Replace(strOuterXml, "</PRLINENUM>", "</int:PRLINENUM>")
                        strOuterXml = Replace(strOuterXml, "<REQUESTEDBY", "<int:REQUESTEDBY")
                        strOuterXml = Replace(strOuterXml, "</REQUESTEDBY>", "</int:REQUESTEDBY>")
                        strOuterXml = Replace(strOuterXml, "<ENTERBY", "<int:ENTERBY")
                        strOuterXml = Replace(strOuterXml, "</ENTERBY>", "</int:ENTERBY>")
                        strOuterXml = Replace(strOuterXml, "<ENTERDATE", "<int:ENTERDATE")
                        strOuterXml = Replace(strOuterXml, "</ENTERDATE>", "</int:ENTERDATE>")
                        strOuterXml = Replace(strOuterXml, "<LOADEDCOST", "<int:LOADEDCOST")
                        strOuterXml = Replace(strOuterXml, "</LOADEDCOST>", "</int:LOADEDCOST>")
                        strOuterXml = Replace(strOuterXml, "<ORDERUNIT", "<int:ORDERUNIT")
                        strOuterXml = Replace(strOuterXml, "</ORDERUNIT>", "</int:ORDERUNIT>")

                        strOuterXml = Replace(strOuterXml, "</MXPO>", "</int:MXPO>")
                        strOuterXml = Replace(strOuterXml, "</PO>", "</int:PO>")
                        strOuterXml = Replace(strOuterXml, "</Content>", "</int:Content>")
                        strOuterXml = Replace(strOuterXml, "</MXPOInterface>", "</int:MXPOInterface>")
                        strOuterXml = Replace(strOuterXml, "</Body>", "</soapenv:Body>")
                        strOuterXml = Replace(strOuterXml, "</Envelope>", "</soapenv:Envelope>")

                        m_logger.WriteVerboseLog(rtn & " :: Finished creating PO XML Out string for LineIndent: " & intIsaIdent.ToString())

                        Dim strXmlFileNameStartWith As String = "CytecMaxPoOut_LineIndent_" & intIsaIdent.ToString() & "_"

                        Dim objStreamWriterXML As System.IO.StreamWriter
                        Dim sFileName As String = strXmlFileNameStartWith & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & Now.GetHashCode & ".xml"

                        strXmlFileNameToSave21 = rootDir & "\XMLOUT\" & sFileName  '   strXmlFileNameStartWith & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & Now.GetHashCode & ".xml"
                        objStreamWriterXML = New System.IO.StreamWriter(strXmlFileNameToSave21)  '  rootDir & "\XMLOUT\" & strXmlFileNameStartWith & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & Now.GetHashCode & ".xml")
                        objStreamWriterXML.WriteLine(strOuterXml)
                        objStreamWriterXML.Flush()
                        objStreamWriterXML.Close()
                        m_logger.WriteVerboseLog(rtn & " :: Saved PO XML Out file for Line Indent.: " & intIsaIdent.ToString())
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
                            strUpdateSql = "UPDATE SYSADM8.PS_ISA_MXM_PO_OUT SET PROCESS_FLAG = 'Y' WHERE ISA_IDENTIFIER = " & ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER") & ""
                        Else

                            'set flag to E; write error message
                            strUpdateSql = "UPDATE SYSADM8.PS_ISA_MXM_PO_OUT SET PROCESS_FLAG = 'E', ISA_ERROR_MSG_80 = '" & sErrMsgFromSendXmlOut & "' WHERE ISA_IDENTIFIER = " & ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER") & ""
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

                    Catch ex As Exception

                        bolLineError = True

                        strXMLError = ex.Message

                        If Len(strXMLError) > 254 Then
                            strXMLError = Microsoft.VisualBasic.Left(strXMLError, 254)
                        End If
                        m_logger.WriteErrorLog(rtn & " :: Error while building POs Out XML for the dataset Line No: " & I.ToString())
                        m_logger.WriteErrorLog(ex.Message)
                        strUpdateSql = ""
                        strUpdateSql = "INSERT INTO SYSADM.PS_NLNK2_ERR_MSG (CUST_ID,ISA_IDENTIFIER,ERROR_LEVEL," & vbCrLf & _
                            "MESSAGE_ID,MESSAGE_NBR,MESSAGE_TEXT_254,DATETIME_ADDED) " & vbCrLf & _
                            " VALUES('CYTEC'," & intIsaIdent & ",'E'," & vbCrLf & _
                            "' ',0,'" & strXMLError & "',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))" & vbCrLf & _
                            ""

                        rowsaffected = 0
                        rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                        If rowsaffected = 0 Then
                            m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                        End If

                        If Len(strXMLError) > 80 Then
                            strXMLError = Microsoft.VisualBasic.Left(strXMLError, 80)
                        End If

                        strUpdateSql = "UPDATE SYSADM8.PS_ISA_MXM_PO_OUT SET PROCESS_FLAG = 'E', ISA_ERROR_MSG_80 = '" & strXMLError & "' WHERE ISA_IDENTIFIER = " & intIsaIdent.ToString() & ""

                        rowsaffected = 0
                        rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                        If rowsaffected = 0 Then
                            m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                        End If
                    End Try
                Else
                    bolLineError = True
                    Dim strErrMsg1 As String = ""
                    strErrMsg1 = rtn & " :: Trim(strPoNumber) = '' (" & strPoNumber & ") Or Trim(strPoLineNum) = '' (" & strPoLineNum & ") for line number: " & (I + 1).ToString()

                    strXMLError = strErrMsg1

                    m_logger.WriteVerboseLog(strErrMsg1)  '  rtn & " :: Trim(strPoNumber) = '' (" & strPoNumber & ") Or Trim(strPoLineNum) = '' (" & strPoLineNum & ") for line number: " & (I + 1).ToString())

                    Try
                        strUpdateSql = "INSERT INTO SYSADM.PS_NLNK2_ERR_MSG (CUST_ID,ISA_IDENTIFIER,ERROR_LEVEL," & vbCrLf & _
                        "MESSAGE_ID,MESSAGE_NBR,MESSAGE_TEXT_254,DATETIME_ADDED) " & vbCrLf & _
                        " VALUES('CYTEC'," & intIsaIdent & ",'E'," & vbCrLf & _
                        "' ',0,'" & strErrMsg1 & "',TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))" & vbCrLf & _
                        ""

                        rowsaffected = 0
                        rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                        If rowsaffected = 0 Then
                            m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                        End If

                        strUpdateSql = "UPDATE SYSADM8.PS_ISA_MXM_PO_OUT SET PROCESS_FLAG = 'E', ISA_ERROR_MSG_80 = '" & strErrMsg1 & "' WHERE ISA_IDENTIFIER = " & ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER") & ""

                        rowsaffected = 0
                        rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)
                        If rowsaffected = 0 Then
                            m_logger.WriteVerboseLog(rtn & " ::  'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                        End If
                    Catch ex As Exception
                        m_logger.WriteVerboseLog(rtn & " :: Error trying to save this message: " & strErrMsg1)
                        m_logger.WriteVerboseLog(rtn & " :: Error: " & ex.Message)
                    End Try
                    
                End If

                If bolLineError Then
                    bolError = True
                End If
            Next  '  For I = 0 To ds.Tables(0).Rows.Count - 1

        End If  '  If ds.Tables(0).Rows.Count > 0 Then

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
        email.Subject = "Cytec Maximo PO XML Out Error(s)"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>'Cytec Maximo PO XML Out' process has completed with errors, review tables SYSADM8.PS_ISA_MXM_PO_OUT and SYSADM.PS_NLNK2_ERR_MSG for Errors description.</td></tr></table>"

        'Send the email and handle any error that occurs
        Dim bSend As Boolean = False
        Try
            SendEmail1(email)
            bSend = True
            'm_arrXMLErrFiles = ""
            'm_arrErrorsList = ""
        Catch ex As Exception
            bSend = False
        End Try

        If Not bSend Then
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent.")
        End If
    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            SendLogger(mailer.Subject, mailer.Body, "CYTECMXMPOBUILDXMLOUT", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

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
