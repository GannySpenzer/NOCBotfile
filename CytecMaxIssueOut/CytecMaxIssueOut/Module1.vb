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
        Dim ds As New DataSet

        Dim rtn As String = "Module1.BuildIssuesOutXML"

        Dim strSqlString As String = "SELECT * FROM SYSADM8.PS_ISA_MXM_MOV_OUT WHERE PROCESS_FLAG != 'Y'"

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

            Try

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
                Dim strSiteId As String = ""
                Dim strWorkOrdNo As String = ""
                'Dim strWoNum As String = ""

                For I = 0 To ds.Tables(0).Rows.Count - 1
                    'for testing:
                    If I = 6 Then Exit For

                    ' Item Number 
                    strItemNum = ""
                    Try
                        strItemNum = CStr(ds.Tables(0).Rows(I).Item("ISA_ITEM"))
                    Catch ex As Exception
                        strItemNum = ""
                    End Try
                    ' int:STORELOC (ISA_BIN_ID) 
                    strStorLoc = ""
                    Try
                        strStorLoc = CStr(ds.Tables(0).Rows(I).Item("ISA_BIN_ID"))
                    Catch ex As Exception
                        strStorLoc = ""
                    End Try
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
                            strTransType = CStr(ds.Tables(0).Rows(I).Item("TRANS_TYPE"))
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

                        'strOrgId = "" 
                        'Try 
                        '    strOrgId = CStr(ds.Tables(0).Rows(I).Item("??????")) 
                        'Catch ex As Exception 
                        'strOrgId = "" 
                        'End Try 
                        'If Trim(strOrgId) <> "" Then 

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
                    Else
                        m_logger.WriteVerboseLog(rtn & " :: Trim(strItemNum) = '' (" & strItemNum & ") Or Trim(strStorLoc) = '' (" & strStorLoc & ") for line number (0 based): " & I.ToString())
                    End If  '  Trim(strItemNum) <> "" And Trim(strStorLoc) <> ""

                    'update field to 'Y'
                    Dim strUpdateSql As String = "UPDATE SYSADM8.PS_ISA_MXM_MOV_OUT SET PROCESS_FLAG = 'Y' WHERE ISA_IDENTIFIER = " & ds.Tables(0).Rows(I).Item("ISA_IDENTIFIER") & ""

                    rowsaffected = 0
                    rowsaffected = ORDBAccess.ExecNonQuery(strUpdateSql, connectOR)

                    If rowsaffected = 0 Then
                        m_logger.WriteVerboseLog(rtn & " :: Update field PROCESS_FLAG to 'Y' returned 'rowsaffected = 0'. strUpdateSql : " & strUpdateSql)
                        bolError = True
                    End If
                Next  '  I = 0 To ds.Tables(0).Rows.Count - 1

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

                m_logger.WriteVerboseLog(rtn & " :: Finished creating XML Out string")
                Dim objStreamWriterXML As System.IO.StreamWriter
                objStreamWriterXML = New System.IO.StreamWriter(rootDir & "\XMLOUT\CytecMaxWriteIssuesOut" & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & Now.GetHashCode & ".xml")
                objStreamWriterXML.WriteLine(strOuterXml)
                objStreamWriterXML.Flush()
                objStreamWriterXML.Close()
                m_logger.WriteVerboseLog(rtn & " :: Saved XML Out file")
                bolError = False
            Catch ex As Exception
                bolError = True
                m_logger.WriteErrorLog(rtn & " :: Error while building Issues Out XML")
                m_logger.WriteErrorLog(ex.Message)
            End Try


        End If

        Return bolError

    End Function

    Private Sub SendEmail()

        Dim rtn As String = "Module1.SendEmail"

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "webdev@sdi.com"

        'The subject of the email
        email.Subject = "Cytec Maximo Issues build XML Error"

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<table><tr><td>'Cytec Maximo Issues build XML' process has completed with errors, review log.</td></tr></table>"

        'Send the email and handle any error that occurs
        Try

            UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "", "Y", email.Body, connectOR)
        Catch

            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent")
        End Try

    End Sub

End Module
