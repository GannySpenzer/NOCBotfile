Module CYCCNT

    Sub Main()

        Const processCode As String = "CYCCNT"

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
            s1 = My.Settings(propertyName:="CYCCNT_GetBatchSize")
        Catch ex As Exception
            s1 = "0"
        End Try
        If CInt(s1) > 0 Then
            nItemPerBatch = CInt(s1)
        End If
        Try
            s1 = My.Settings(propertyName:="CYCCNT_MaxReq")
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
        Dim oBatch As CYCCNT_batch = Nothing
        Dim oraCN1 As New OleDb.OleDbConnection(oraCNString)
        Dim sr As System.IO.StreamReader = Nothing
        Dim s As String = ""
        Dim nRec As Integer = 0

        oraCN1.Open()

        If (oraCN1.State = ConnectionState.Open) Then

            logger.WriteVerboseLog("DB connection opened.")
            logger.WriteVerboseLog("using DB : " & oraCN1.Database)

            Dim oraTran1 As OleDb.OleDbTransaction = Nothing

            '
            ' retrieve records to process from PeopleSoft table and send adjustment request - per record
            '

            oBatch = New CYCCNT_batch

            sr = New System.IO.StreamReader(CStr(My.Settings("CYCCNT_SelectToSend")))
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
                logger.WriteErrorLog("errorred retrieving cycle count records to process from table.")
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

                    Dim sPlant As String = ""
                    Dim sItem As String = ""
                    Dim sBin As String = ""
                    Dim nQty As Decimal = 0
                    Dim sEventId As String = ""

                    Dim sInitialProcessFlag As String = ""

                    Dim nRecId As Integer = 0
                    Dim sKey As String = ""

                    Dim sUpdateKeyColumns As String = CStr(My.Settings("CYCCNT_UpdateKeyColumns"))

                    While dr.Read

                        nRecId += 1

                        '// grab row values
                        sPlant = ""
                        Try
                            sPlant = CStr(dr("PLANT")).Trim
                        Catch ex As Exception
                        End Try
                        If (sPlant Is Nothing) Then
                            sPlant = ""
                        End If

                        sItem = ""
                        Try
                            sItem = CStr(dr("INV_ITEM_ID")).Trim
                        Catch ex As Exception
                        End Try
                        If (sItem Is Nothing) Then
                            sItem = ""
                        End If

                        sBin = ""
                        Try
                            sBin = CStr(dr("ISA_CUST_BIN")).Trim
                        Catch ex As Exception
                        End Try
                        If (sBin Is Nothing) Then
                            sBin = ""
                        End If

                        nQty = 0
                        Try
                            nQty = CDec(dr("QTY"))
                        Catch ex As Exception
                        End Try

                        sEventId = ""
                        Try
                            sEventId = CStr(dr("EVENT_ID")).Trim
                        Catch ex As Exception
                        End Try
                        If (sEventId Is Nothing) Then
                            sEventId = ""
                        End If

                        sInitialProcessFlag = ""
                        Try
                            sInitialProcessFlag = CStr(dr("PROCESS_FLAG")).Trim
                        Catch ex As Exception
                        End Try
                        If (sInitialProcessFlag Is Nothing) Then
                            sInitialProcessFlag = ""
                        End If

                        sKey = sEventId & "." & sPlant & "-" & sItem & "-" & sBin & "." & nRecId.ToString.PadLeft(4, "0"c)

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

                        sr = New System.IO.StreamReader(CStr(My.Settings("CYCCNT_HTTP_SOAP_Batch_Item")))
                        s = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields
                        '	1010 	STORE
                        '	1020 	PART
                        '	1030 	BIN
                        '	1040 	QTY
                        '	1050 	SDI_CYCLE_COUNT_ID
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(s, sPlant, sItem, sBin, nQty.ToString(), sEventId)
                        iList &= sb.ToString
                        sb = Nothing

                        '   (2) batch
                        iBatch = ""

                        sr = New System.IO.StreamReader(CStr(My.Settings("CYCCNT_HTTP_SOAP_Batch")))
                        s = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields
                        '   Username {0}
                        '   Password {1}
                        '   "cycle count records" {2}
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(s, oParent.Username, oParent.Password, iList)
                        iBatch = sb.ToString
                        sb = Nothing

                        'logger.WriteVerboseLog("loaded CYCCNT HTTP SOAP request XML : " & s)
                        logger.WriteVerboseLog("loaded CYCCNT HTTP SOAP request XML : " & CStr(My.Settings("CYCCNT_HTTP_SOAP_Batch")))

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
                            sHttpResponse &= ex.ToString
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
                        Dim sRespErrMsg As String = ""

                        ' parse through the response string to check for any errors 
                        '       and update PS_ISA_MB_ADJ table of each inventory adjustment record/request
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
                        End If

                        nd2 = Nothing
                        nd1 = Nothing

                        nd = Nothing
                        nsmgr = Nothing
                        xmlIn = Nothing

                        '// update our table - PS_ISA_MB_CYCLCNT

                        Dim sProcessFlag As String = ""

                        If bSucceed Then
                            sProcessFlag = "Y"
                        Else
                            sProcessFlag = "E"
                        End If

                        Dim sWhere As String = ""

                        For n As Integer = 0 To (arrKeyValue.Keys.Count - 1)
                            sFldKey = CStr(arrKeyValue.Keys(n))
                            If Not (n < (arrKeyValue.Keys.Count - 1)) Then
                                ' we're on the last field
                                sWhere &= " " & sFldKey & " " & CStr(arrKeyValue(sFldKey))
                            Else
                                ' we're NOT on the last field
                                sWhere &= " " & sFldKey & " " & CStr(arrKeyValue(sFldKey)) & " AND"
                            End If
                        Next

                        sr = New System.IO.StreamReader(CStr(My.Settings("CYCCNT_UpdateInputTable")))
                        s = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields 
                        '   PROCESS_FLAG {0}
                        '   "WHERE clause" {1}
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(s, sProcessFlag, sWhere)
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
                                logger.WriteErrorLog("errorred updating cycle count record.")
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
                    logger.WriteInformationLog("no cycle count record to process.")
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

End Module
