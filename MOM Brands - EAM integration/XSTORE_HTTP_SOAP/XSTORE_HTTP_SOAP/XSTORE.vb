Module XSTORE

    Sub Main()

        Const processCode As String = "XSTORE"

        ' default values
        Dim nItemPerBatch As Integer = 100
        Dim nMaxReq As Integer = 100
        Dim custId As String = "UNKNO"
        Dim oraCNString As String = ""
        Dim logger As SDI.ApplicationLogger.IApplicationLogger = Nothing
        Dim emailer As SDI.EmailNotifier.IEmailNotifier = Nothing

        Dim appVar As New SDI.HTTP_SOAP_INTFC.appVarCollection

        appVar.AddDefaultVars()

        '
        ' configuration (override defaults)
        '
        Dim s1 As String = ""

        '   (1) grab/create "pluggable" parent app instance
        '       parent app holds the (a) logger (b) email notifier (c) customer ID and (d) target customer URL
        Dim oParent As SDI.HTTP_SOAP_INTFC.IParentApp = Nothing
        Dim sParentAppDll As String = ""
        Try
            s1 = My.Settings(propertyName:="ParentApp")
        Catch ex As Exception
            s1 = ""
        End Try
        If (s1.Trim.Length > 0) Then
            sParentAppDll = s1
        End If
        Dim sParentAppClass As String = ""
        Try
            s1 = My.Settings(propertyName:="ParentAppClass")
        Catch ex As Exception
            s1 = ""
        End Try
        If (s1.Trim.Length > 0) Then
            sParentAppClass = s1
        End If
        Dim ass As System.Reflection.Assembly = System.Reflection.Assembly.LoadFrom(sParentAppDll)
        Dim typ As System.Type = ass.GetType(sParentAppClass)
        Try
            oParent = DirectCast(Activator.CreateInstance(typ), SDI.HTTP_SOAP_INTFC.IParentApp)
        Catch ex As Exception
            oParent = Nothing
        End Try
        typ = Nothing
        ass = Nothing
        If (oParent Is Nothing) Then
            Throw New ApplicationException(message:="missing parent application OR unable to create instance.")
        End If

        ' inherited from parent app
        oraCNString = oParent.OracleConnectionString1
        custId = oParent.CustomerIdentifier
        logger = oParent.Logger
        emailer = oParent.EmailNotifier

        appVar.AddEntry(key:="{SYSTEM:CUSTOMER_ID}", dataType:="STRING", value:=custId)
        appVar.AddEntry(key:="{SYSTEM:PROCESS_INSTANCE}", dataType:="STRING", value:="0")


        '   (2) grab child configuration
        Try
            s1 = My.Settings(propertyName:="XSTORE_GetBatchSize")
        Catch ex As Exception
            s1 = "0"
        End Try
        If CInt(s1) > 0 Then
            nItemPerBatch = CInt(s1)
        End If
        Try
            s1 = My.Settings(propertyName:="XSTORE_MaxReq")
        Catch ex As Exception
            s1 = "0"
        End Try
        If CInt(s1) > 0 Then
            nMaxReq = CInt(s1)
        End If
        Try
            ' this overrides the connection string (instead of using parent app's) if available
            s1 = My.Settings(propertyName:="oraCNString1")
        Catch ex As Exception
            s1 = ""
        End Try
        If (s1.Trim.Length > 0) Then
            oraCNString = s1.Trim
        End If

        ' this app name/version
        Dim meName As String = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName
        Dim meVer As String = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString

        logger.WriteInformationLog("starting " & meName & " v" & meVer)
        logger.WriteVerboseLog("using parent app " & oParent.Name & " v" & oParent.Version)
        logger.WriteVerboseLog(processCode & " processing for customer " & custId)
        logger.WriteVerboseLog("target customer URL : " & oParent.TargetURL)
        logger.WriteVerboseLog("temporary file path : " & oParent.TemporaryFilePath)
        logger.WriteVerboseLog("processing batch size " & nItemPerBatch.ToString & " for a maximum of " & nMaxReq.ToString & " request(s)")

        ' 
        ' process
        '
        Dim nLoopCtr As Integer = 0
        Dim nTotalRecordsProcessed As Integer = 0
        Dim bErr As Boolean = False

        Dim processId As String = processCode & "_" & Guid.NewGuid.ToString

        Dim dr As OleDb.OleDbDataReader = Nothing

        Dim cmd As OleDb.OleDbCommand = Nothing
        Dim sb As System.Text.StringBuilder = Nothing
        Dim sql As String = ""
        Dim oBatch As XSTORE_batch = Nothing
        Dim oraCN1 As New OleDb.OleDbConnection(oraCNString)
        Dim sr As System.IO.StreamReader = Nothing
        Dim s As String = ""
        Dim nRec As Integer = 0

        oraCN1.Open()

        If (oraCN1.State = ConnectionState.Open) Then

            logger.WriteVerboseLog("DB connection opened.")
            logger.WriteVerboseLog("using DB : " & oraCN1.DataSource)

            Dim oraTran1 As OleDb.OleDbTransaction = Nothing

            '
            ' retrieve records to process from PeopleSoft table and receipt request - per record
            '

            oBatch = New XSTORE_batch

            sr = New System.IO.StreamReader(CStr(My.Settings("XSTORE_SelectToSend")))
            s = sr.ReadToEnd
            sr.Dispose()
            sr = Nothing

            ' fields 
            '   customer Id {0}
            sb = New System.Text.StringBuilder
            sb.AppendFormat(s, custId)
            s = sb.ToString
            sb = Nothing

            cmd = oraCN1.CreateCommand
            cmd.CommandText = s
            cmd.CommandType = CommandType.Text
            'cmd.Transaction = oraTran1

            Try
                dr = cmd.ExecuteReader
            Catch ex As Exception
                oBatch.ex = ex
                logger.WriteErrorLog("errorred retrieving store-to-store inventory transfer records to process from table.")
                Try
                    dr.Close()
                Catch ex1 As Exception
                End Try
                dr = Nothing
            End Try

            ' check if we have some record/s to process
            If Not (dr Is Nothing) Then
                If dr.HasRows Then

                    Dim arrKeyValue As Hashtable = Nothing

                    Dim iList As String = ""
                    Dim iBatch As String = ""

                    Dim sFromPlant As String = ""
                    Dim sToPlant As String = ""
                    Dim sItem As String = ""
                    Dim sFromBin As String = ""
                    Dim sToBin As String = ""
                    Dim nQty As Decimal = 0
                    Dim sAssetId As String = ""

                    Dim sPrevErrMsg As String = ""
                    Dim sPrevErrMsgDesc As String = ""

                    Dim sInitialProcessFlag As String = ""

                    Dim nRecId As Integer = 0
                    Dim sKey As String = ""

                    Dim sUpdateKeyColumns As String = CStr(My.Settings("XSTORE_UpdateKeyColumns"))

                    While dr.Read

                        nRecId += 1

                        '// grab row values
                        sFromPlant = ""
                        Try
                            sFromPlant = CStr(dr("PLANT")).Trim
                        Catch ex As Exception
                        End Try
                        If (sFromPlant Is Nothing) Then
                            sFromPlant = ""
                        End If

                        sToPlant = ""
                        Try
                            sToPlant = CStr(dr("ISA_TO_PLANT")).Trim
                        Catch ex As Exception
                        End Try
                        If (sToPlant Is Nothing) Then
                            sToPlant = ""
                        End If

                        sItem = ""
                        Try
                            sItem = CStr(dr("INV_ITEM_ID")).Trim
                        Catch ex As Exception
                        End Try
                        If (sItem Is Nothing) Then
                            sItem = ""
                        End If

                        sFromBin = ""
                        Try
                            sFromBin = CStr(dr("ISA_CUST_BIN")).Trim
                        Catch ex As Exception
                        End Try
                        If (sFromBin Is Nothing) Then
                            sFromBin = ""
                        End If

                        sToBin = ""
                        Try
                            sToBin = CStr(dr("ISA_CUST_TO_BIN")).Trim
                        Catch ex As Exception
                        End Try
                        If (sToBin Is Nothing) Then
                            sToBin = ""
                        End If

                        nQty = 0
                        Try
                            nQty = CDec(dr("QTY"))
                        Catch ex As Exception
                        End Try

                        sAssetId = ""
                        Try
                            sAssetId = CStr(dr("SERIAL_ID")).Trim
                        Catch ex As Exception
                        End Try
                        If (sAssetId Is Nothing) Then
                            sAssetId = ""
                        End If

                        sInitialProcessFlag = ""
                        Try
                            sInitialProcessFlag = CStr(dr("PROCESS_FLAG")).Trim
                        Catch ex As Exception
                        End Try
                        If (sInitialProcessFlag Is Nothing) Then
                            sInitialProcessFlag = ""
                        End If

                        sPrevErrMsg = ""
                        Try
                            sPrevErrMsg = CStr(dr("ISA_ERROR_RAW")).Trim
                        Catch ex As Exception
                        End Try
                        If (sPrevErrMsg Is Nothing) Then
                            sPrevErrMsg = ""
                        End If

                        sPrevErrMsgDesc = ""
                        Try
                            sPrevErrMsgDesc = CStr(dr("ISA_ERROR_MSG_80")).Trim
                        Catch ex As Exception
                        End Try
                        If (sPrevErrMsgDesc Is Nothing) Then
                            sPrevErrMsgDesc = ""
                        End If

                        'sKey = sFromPlant & "-" & sItem & "-" & sFromBin & "." & sToPlant & "-" & sToBin & "." & nRecId.ToString.PadLeft(4, "0"c)
                        sKey = sFromPlant & "-" & sItem & "." & sToPlant & "." & nRecId.ToString.PadLeft(4, "0"c)

                        ' grab row values that we'll compare when updating this record later on
                        arrKeyValue = New Hashtable

                        Dim sFldKey As String = ""
                        Dim sFldVal As String = ""

                        For Each row As System.Data.DataRow In dr.GetSchemaTable.Rows
                            sFldKey = CStr(row("ColumnName"))
                            sFldVal = "= ''"
                            If (sUpdateKeyColumns.IndexOf(sFldKey) > -1) Then
                                If (dr(sFldKey) Is DBNull.Value) Then
                                    sFldVal = "IS NULL"
                                Else
                                    Select Case row("DataType")
                                        Case GetType(System.Decimal)
                                            ' numeric
                                            sFldVal = "= '" & CDec(dr(sFldKey)).ToString() & "'"
                                        Case GetType(System.DateTime)
                                            ' date/time
                                            sFldVal = "= TO_DATE('" & CDate(dr(sFldKey)).ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS')"
                                        Case Else
                                            ' regard as System.String
                                            If (CStr(dr(sFldKey)).Trim.Length = 0) Then
                                                sFldVal = "= ' '"
                                            Else
                                                sFldVal = "= '" & CStr(dr(sFldKey)).Trim & "'"
                                            End If
                                    End Select
                                End If
                                If (Not arrKeyValue.ContainsKey(sFldKey)) Then
                                    arrKeyValue.Add(sFldKey, sFldVal)
                                End If
                            End If
                        Next

                        ''// transaction and record locking
                        ''       update this record so we have lock on it before any other process does - prevents other from changing

                        'oraTran1 = oraCN1.BeginTransaction


                        '// soap messages (both item and batch)
                        '   (1) item
                        iList = ""

                        sr = New System.IO.StreamReader(CStr(My.Settings("XSTORE_HTTP_SOAP_Batch_Item")))
                        s = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields
                        '	1010 	FROM STORE
                        '	1020 	TO STORE
                        '	1030 	PART
                        '	1040 	FROM BIN
                        '	1050 	TO BIN
                        '	1060 	QTY
                        '	1070 	Asset ID 		passing <blank> at the moment
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(s, sFromPlant, sToPlant, sItem, sFromBin, sToBin, nQty.ToString, sAssetId)
                        iList &= sb.ToString
                        sb = Nothing

                        '   (2) batch
                        iBatch = ""

                        sr = New System.IO.StreamReader(CStr(My.Settings("XSTORE_HTTP_SOAP_Batch")))
                        s = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields
                        '   Username {0}
                        '   Password {1}
                        '   "store-to-store transfers" {2}
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(s, oParent.Username, oParent.Password, iList)
                        iBatch = sb.ToString
                        sb = Nothing

                        'logger.WriteVerboseLog("loaded XSTORE HTTP SOAP request XML : " & iBatch)
                        logger.WriteVerboseLog("loaded XSTORE HTTP SOAP request XML : " & CStr(My.Settings("XSTORE_HTTP_SOAP_Batch")))

                        '// send soap message
                        Dim flePath As String = oParent.TemporaryFilePath.Trim
                        While (flePath.Length > 0) And (flePath.LastIndexOf("\"c) = (flePath.Length - 1))
                            flePath = flePath.TrimEnd("\"c)
                        End While
                        Dim fle As String = ""
                        Dim sw As System.IO.StreamWriter = Nothing

                        s = SDI.HTTP_SOAP_INTFC.Common.RemoveCrLf(iBatch)

                        ' only create this request xml file if logging on verbose for tracing
                        If (logger.LoggingLevel = TraceLevel.Verbose) Then
                            fle = flePath & "\" & processId & " (" & sKey & ")-req.xml"
                            sw = New System.IO.StreamWriter(path:=fle, append:=False)
                            sw.WriteLine(s)
                            sw.Flush()
                            sw.Close()
                            sw.Dispose()
                            sw = Nothing
                        End If

                        Dim sHttpResponse As String = ""
                        Dim httpSession As New SDI.HTTP_SOAP_INTFC.easyHttp

                        'httpSession.URL = "https://eam.saas.infor.com/EAM85WS/axis/services/EWSConnector"
                        httpSession.URL = oParent.TargetURL
                        httpSession.DataToPost = s
                        httpSession.ContentType = "text/xml; charset=utf-8"
                        httpSession.Method = SDI.HTTP_SOAP_INTFC.easyHttp.HTTPMethod.HTTP_POST
                        httpSession.IgnoreServerCertificate = False
                        httpSession.HeaderAttributes.Add(name:="SOAPAction", value:="https://schemas.microsoft.com/crm/2006/WebServices/Retrieve")

                        sHttpResponse = ""

                        Try
                            'sHttpResponse = httpSession.SendAsBytes
                            sHttpResponse = httpSession.SendAsString
                        Catch ex As Exception
                            ' new 7i instance for MOM Brand is actually sending us an exception (System.Net.WebException) now
                            '       instead just the actual error on the response and NOT producing a "webException" ... thus this change to 
                            '       try and process that response even on webException - Erwin 2014.10.15
                            'sHttpResponse &= ex.ToString
                            sHttpResponse &= ex.Message
                        End Try

                        httpSession = Nothing

                        ' clean the response string
                        '   "System.Xml.XmlException: The XML declaration is unexpected ..." error message due to unexpected char before the xml declaration!?
                        sHttpResponse = SDI.HTTP_SOAP_INTFC.Common.RemoveCrLf(sHttpResponse)

                        If (logger.LoggingLevel = TraceLevel.Verbose) Then
                            fle = flePath & "\" & processId & " (" & sKey & ")-resp.xml"
                            sw = New System.IO.StreamWriter(path:=fle, append:=False)
                            sw.WriteLine(sHttpResponse)
                            sw.Flush()
                            sw.Close()
                            sw.Dispose()
                            sw = Nothing
                        End If

                        logger.WriteVerboseLog("response XML file : " & fle)

                        Dim bSucceed As Boolean = False

                        ' inherit current error messages from the table
                        Dim sRespErrMsg As String = sPrevErrMsg
                        Dim sRespErrMsgDesc As String = sPrevErrMsgDesc

                        ' parse through the response string to check for any errors 
                        '       and update PS_ISA_MB_IBU_XFER table of each tramnsfer record/request
                        Dim xmlIn As New System.Xml.XmlDocument

                        xmlIn.LoadXml(xml:=sHttpResponse)

                        Dim nsmgr As New Xml.XmlNamespaceManager(xmlIn.NameTable)

                        nsmgr.AddNamespace("ns1", "http://schemas.datastream.net/MP_results/MP6228_001")
                        nsmgr.AddNamespace("ns2", "http://schemas.datastream.net/MP_fields") '"http://schemas.datastream.net/MP_functions")
                        nsmgr.AddNamespace("ns3", "http://schemas.datastream.net/MP_fields")
                        nsmgr.AddNamespace("ns4", "http://schemas.datastream.net/MP_fields")

                        Dim nd As Xml.XmlNode = Nothing
                        Dim nd1 As Xml.XmlNode = Nothing
                        Dim nd2 As Xml.XmlNode = Nothing

                        nd = xmlIn.SelectSingleNode("//ns1:ResultData", nsmgr)

                        If Not (nd Is Nothing) Then
                            For i As Integer = 0 To (nd.ChildNodes.Count - 1) ' GROUPRESULT
                                bSucceed = False
                                nd1 = nd.ChildNodes(i).SelectSingleNode("ns3:MESSAGE", nsmgr)
                                If Not (nd1 Is Nothing) Then
                                    s = nd1.SelectSingleNode("ns3:type", nsmgr).InnerText.Trim.ToUpper
                                    bSucceed = (s = "SUCCESS")
                                End If
                                nd1 = Nothing
                                ' since we're only processing a single request
                                '       we're assumming there's only a single response (GROUPRESULT)
                                Exit For
                            Next
                        Else
                            ' request must have errored out since I can't find the "ResultData" node
                            bSucceed = False
                        End If

                        If Not bSucceed Then
                            ' since the request seems to have generated an error
                            '       let's see if we can retrieve the error message (detail)
                            sRespErrMsg = ""

                            nsmgr = New Xml.XmlNamespaceManager(xmlIn.NameTable)
                            nsmgr.AddNamespace("ns1", "http://schemas.datastream.net/MP_functions")

                            nd = xmlIn.SelectSingleNode("//ns1:ExceptionInfo", nsmgr)

                            If Not (nd Is Nothing) Then
                                nd1 = nd.SelectSingleNode("ns1:Exception", nsmgr)
                                If Not (nd1 Is Nothing) Then
                                    s = nd1.SelectSingleNode("ns1:Message", nsmgr).InnerText.Trim
                                    sRespErrMsg = s
                                End If
                            End If

                            If (sRespErrMsg.Trim.Length > 0) Then
                                ' try to interpret this message
                                sRespErrMsgDesc = GetDescriptiveErrorMessage(oraCN1, sRespErrMsg)
                            End If
                        Else
                            ' on success, check flag if we need to clear previous (if any) error message that we wrote
                            Dim bClearErrMsg As Boolean = False
                            Try
                                bClearErrMsg = CBool(My.Settings("XSTORE_ClearErrMsgOnSuccess"))
                            Catch ex As Exception
                            End Try
                            If bClearErrMsg Then
                                ' reset both "raw" and "descriptive" error messages
                                sRespErrMsg = ""
                                sRespErrMsgDesc = ""
                            End If
                        End If

                        nd2 = Nothing
                        nd1 = Nothing

                        nd = Nothing
                        nsmgr = Nothing
                        xmlIn = Nothing

                        '// update our table - PS_ISA_MB_IBU_XFER

                        Dim sProcessFlag As String = ""

                        If bSucceed Then
                            sProcessFlag = "Y"
                        Else
                            sProcessFlag = "E"
                        End If

                        ' accommodate Oracle interpretation of empty string = NULL
                        '   and since table can only handle 80 chars each field ...
                        If (sRespErrMsg.Trim.Length = 0) Then
                            ' single space
                            sRespErrMsg = " "
                        Else
                            sRespErrMsg = (sRespErrMsg.Trim + (New String(" "c, 80))).Substring(0, 80).TrimEnd
                        End If
                        If (sRespErrMsgDesc.Trim.Length = 0) Then
                            sRespErrMsgDesc = " "
                        Else
                            sRespErrMsgDesc = (sRespErrMsgDesc.Trim + (New String(" "c, 80))).Substring(0, 80).TrimEnd
                        End If

                        Dim sWhere As String = ""

                        For n As Integer = 0 To (arrKeyValue.Keys.Count - 1)
                            sFldKey = CStr(arrKeyValue.Keys(n))
                            If Not (n < (arrKeyValue.Keys.Count - 1)) Then
                                ' we're on the last field
                                sWhere &= " " & sFldKey & " " & arrKeyValue(sFldKey)
                            Else
                                ' we're NOT on the last field
                                sWhere &= " " & sFldKey & " " & arrKeyValue(sFldKey) & " AND"
                            End If
                        Next

                        sr = New System.IO.StreamReader(CStr(My.Settings("XSTORE_UpdateInputTable")))
                        s = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields 
                        '   PROCESS_FLAG {0}
                        '   ISA_ERROR_RAW = '{2}'
                        '   ISA_ERROR_MSG_80 = '{3}'
                        '   "WHERE clause" {1}
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(s, sProcessFlag, sWhere, sRespErrMsg, sRespErrMsgDesc)
                        s = sb.ToString
                        sb = Nothing

                        If (sInitialProcessFlag = "E" And sProcessFlag = "E") Then
                            ' do nothing - don't update flag NOR timestamp
                            '   since initially this is an error record already and re-processing it still stays in error
                        Else
                            Dim cmd1 As OleDb.OleDbCommand = oraCN1.CreateCommand
                            cmd1.CommandText = s
                            cmd1.CommandType = CommandType.Text
                            'cmd1.Transaction = oraTran1

                            Dim iRec As Integer = -1

                            Try
                                iRec = cmd1.ExecuteNonQuery
                            Catch ex As Exception
                                'oBatch.ex = ex
                                logger.WriteErrorLog("errorred updating store-to-store transfer record that was processed.")
                            End Try

                            Try
                                cmd1.Dispose()
                            Catch ex As Exception
                            End Try
                            cmd1 = Nothing
                        End If

                    End While       ' While dr.Read

                    arrKeyValue = Nothing

                Else        ' If dr.HasRows Then
                    logger.WriteInformationLog("no store-to-store transfer record to process.")
                End If
            End If      ' If Not (dr Is Nothing) Then

            Try
                cmd.Dispose()
            Catch ex As Exception
            End Try
            cmd = Nothing

            oBatch = Nothing

            Try
                oraTran1.Dispose()
            Catch ex As Exception
            End Try
            oraTran1 = Nothing

        Else        ' ELSE : If (oraCN1.State = ConnectionState.Open) Then
            ' unable to open connection!
            logger.WriteErrorLog("DB : unable to open connection : " & oraCNString)
        End If      ' If (oraCN1.State = ConnectionState.Open) Then

        ' clean- up
        Try
            oraCN1.Close()
        Catch ex As Exception
        Finally
            oraCN1.Dispose()
        End Try
        oraCN1 = Nothing

        logger.WriteInformationLog("finished executing " & meName)

        oBatch = Nothing
        emailer = Nothing
        logger = Nothing
        oParent = Nothing

    End Sub

    Private Function GetDescriptiveErrorMessage(ByVal oraCN As OleDb.OleDbConnection, _
                                                ByVal sRaw As String) As String

        Dim sRawMsg As String = ""
        Dim sMsgDesc As String = ""

        If Not (sRaw Is Nothing) Then
            Try
                sRawMsg = sRaw.Trim
            Catch ex As Exception
            End Try
        End If

        If (sRawMsg.Length > 0) Then

            Dim sErrKeyValSeparator As Char = "="c
            Dim sErrKeyVal As String() = New String() {}

            Dim sErrCodePattern As String = ""
            Dim sErrIdPattern As String = ""

            Dim sErrId As String = ""

            Try
                sErrCodePattern = CStr(My.Settings("XSTORE_CanParseErrMsgSignature")).Trim
            Catch ex As Exception
            End Try
            If (sErrCodePattern Is Nothing) Then
                sErrCodePattern = ""
            End If

            Try
                sErrIdPattern = CStr(My.Settings("XSTORE_ErrMsgId")).Trim
            Catch ex As Exception
            End Try
            If (sErrIdPattern Is Nothing) Then
                sErrIdPattern = ""
            End If

            ' can I parse this error message (ie., is expected error code pattern on raw string)
            Dim oErrRaw As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(sRawMsg, sErrCodePattern)

            If Not (oErrRaw Is Nothing) Then
                If oErrRaw.Success Then
                    ' found error code pattern. grab the first instance and search for the error ID (based on known pattern)
                    Dim oErrId As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(oErrRaw.Groups(0).Value, sErrIdPattern)
                    If Not (oErrId Is Nothing) Then
                        Try
                            sErrKeyVal = oErrId.Groups(0).Value.Trim.Split(sErrKeyValSeparator)
                        Catch ex As Exception
                        End Try
                    End If
                    If (sErrKeyVal.Length > 1) Then
                        Try
                            sErrId = sErrKeyVal(1).Trim.ToUpper
                        Catch ex As Exception
                        End Try
                    End If
                    If Not (sErrId.Length > 0) Then
                        sMsgDesc = "unable to identify error ID from message"
                    End If
                Else
                    ' unable to find the error code pattern, so I'm reporting the exact error message 
                    sMsgDesc = sRawMsg
                End If
            End If

            ' check for error key/value pair (ie., N=*)
            '   if error ID value found, try to search table
            If (sErrId.Length > 0) Then
                Dim cmd As OleDb.OleDbCommand = Nothing
                Dim sb As System.Text.StringBuilder = Nothing
                Dim sql As String = ""
                Dim sr As System.IO.StreamReader = Nothing

                sr = New System.IO.StreamReader(CStr(My.Settings("XSTORE_GetErrMsgDesc")))
                sql = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                ' fields
                '	ERR_ID
                sb = New System.Text.StringBuilder
                sb.AppendFormat(sql, sErrId)
                sql = sb.ToString
                sb = Nothing

                cmd = oraCN.CreateCommand
                cmd.CommandText = sql
                cmd.CommandType = CommandType.Text

                Try
                    sMsgDesc = CStr(cmd.ExecuteScalar).Trim
                Catch ex As Exception
                End Try

                Try
                    cmd.Dispose()
                Catch ex As Exception
                End Try
                cmd = Nothing

                If (sMsgDesc.Trim.Length = 0) Then
                    sMsgDesc = "error code NOT DEFINED : " & sErrId
                End If
            End If

        End If

        Return (sMsgDesc)

    End Function

End Module
